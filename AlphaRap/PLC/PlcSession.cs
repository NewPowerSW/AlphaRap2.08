using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace AlphaRap.PLC
{
    /// <summary>按连接编号共享一条有界、串行的汇川通信会话。</summary>
    internal sealed class PlcSession
    {
        private static readonly ConcurrentDictionary<int, PlcSession> Sessions = new ConcurrentDictionary<int, PlcSession>();
        private static readonly object LaunchSync = new object();
        private readonly SemaphoreSlim gate = new SemaphoreSlim(1, 1);
        private readonly object stateSync = new object();
        private readonly int netId;
        private readonly string executable;
        private Process worker;
        private AnonymousPipeServerStream input, output;
        private BinaryReader reader;
        private BinaryWriter writer;
        private bool connected;
        private Task<PlcReply> pending;
        private long generation;
        private CancellationTokenSource activeCancellation;

        /// <summary>绑定连接编号和承载驱动的应用程序；测试可使用同协议的离线宿主。</summary>
        internal PlcSession(int id, string workerExecutable)
        {
            netId = id; executable = workerExecutable;
        }

        /// <summary>所有 PLC 对象通过同一编号获取相同的连接、锁和进程。</summary>
        internal static PlcSession Get(int id)
        {
            if (id < 0) throw new ArgumentOutOfRangeException("id");
            return Sessions.GetOrAdd(id, key => new PlcSession(key, typeof(PlcSession).Assembly.Location));
        }

        /// <summary>在限定时间内建立连接；只对连接握手重试，不重放任何数据操作。</summary>
        internal PlcResult<byte[]> Connect(string host, int port, int timeoutMs, int attempts, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(host) || host.Length > 255 || port < 1 || port > 65535 || attempts < 1 || attempts > 5)
                return Failure(PlcStatus.InvalidArgument);
            return Execute(new PlcRequest { Command = PlcCommand.Connect, NetId = netId, Host = host, Port = port }, timeoutMs, token, attempts);
        }

        /// <summary>保持队列等待、发送和应答共用一个总时限。</summary>
        internal PlcResult<byte[]> Execute(PlcRequest request, int timeoutMs, CancellationToken token, int attempts = 1)
        {
            if (timeoutMs < 1 || timeoutMs > 60000) return Failure(PlcStatus.InvalidArgument);
            if (token.IsCancellationRequested) return Failure(PlcStatus.Cancelled);
            Stopwatch elapsed = Stopwatch.StartNew();
            long requestGeneration;
            lock (stateSync)
            {
                if (request.Command == PlcCommand.Connect) generation++;
                requestGeneration = generation;
            }
            try
            {
                if (!gate.Wait(timeoutMs, token)) return Failure(PlcStatus.Timeout);
            }
            catch (OperationCanceledException) { return Failure(PlcStatus.Cancelled); }
            try { return ExecuteActive(request, timeoutMs, elapsed, token, attempts, requestGeneration); }
            finally { gate.Release(); }
        }

        /// <summary>在已获得的请求租约中登记可中断操作，结束后移除该取消入口。</summary>
        private PlcResult<byte[]> ExecuteActive(PlcRequest request, int limit, Stopwatch elapsed, CancellationToken token, int attempts, long requestGeneration)
        {
            using (var cancellation = CancellationTokenSource.CreateLinkedTokenSource(token))
            {
                lock (stateSync)
                {
                    if (requestGeneration != generation) return Failure(PlcStatus.Cancelled);
                    activeCancellation = cancellation;
                }
                try { return ExecuteLocked(request, limit, elapsed, cancellation.Token, attempts, requestGeneration); }
                finally { lock (stateSync) activeCancellation = null; }
            }
        }

        /// <summary>同一连接仅执行一条实际请求；故障后保持断开，必须显式重连。</summary>
        private PlcResult<byte[]> ExecuteLocked(PlcRequest request, int limit, Stopwatch elapsed, CancellationToken token, int attempts, long requestGeneration)
        {
            if (token.IsCancellationRequested) return Failure(PlcStatus.Cancelled);
            if (Remaining(limit, elapsed) == 0) return Failure(PlcStatus.Timeout);
            if (request.Command != PlcCommand.Connect && !connected) return Failure(PlcStatus.NotConnected);
            if (request.Command != PlcCommand.Connect) return Exchange(request, Remaining(limit, elapsed), token, requestGeneration);
            if (!Retire()) return Failure(PlcStatus.WorkerError, detail: "Previous PLC worker has not exited.");
            PlcResult<byte[]> result = Failure(PlcStatus.NotConnected);
            for (int attempt = 0; attempt < attempts; attempt++)
            {
                if (token.IsCancellationRequested) return Failure(PlcStatus.Cancelled);
                int remaining = Remaining(limit, elapsed);
                if (remaining == 0) return Failure(PlcStatus.Timeout);
                result = StartAndConnect(request, remaining, token, requestGeneration);
                if (result.Succeeded || result.Status == PlcStatus.Cancelled || result.Status == PlcStatus.Timeout) return result;
                if (!Retire()) return Failure(PlcStatus.WorkerError, detail: "Previous PLC worker has not exited.");
                int backoff = Math.Min(100 * (attempt + 1), Remaining(limit, elapsed));
                if (attempt + 1 < attempts && token.WaitHandle.WaitOne(backoff)) return Failure(PlcStatus.Cancelled);
            }
            return result;
        }

        /// <summary>创建一个隔离进程并完成握手，失败不设置连接成功状态。</summary>
        private PlcResult<byte[]> StartAndConnect(PlcRequest request, int remaining, CancellationToken token, long requestGeneration)
        {
            Stopwatch elapsed = Stopwatch.StartNew();
            try
            {
                StartWorker();
                PlcResult<byte[]> result = Exchange(request, Remaining(remaining, elapsed), token, requestGeneration);
                connected = result.Succeeded;
                return result;
            }
            catch (Exception error)
            {
                Retire();
                return Failure(PlcStatus.WorkerError, detail: error.GetType().Name + ": " + error.Message);
            }
        }

        /// <summary>原生阻塞只占用子进程；取消和超时回收通道后才允许下一次连接。</summary>
        private PlcResult<byte[]> Exchange(PlcRequest request, int remaining, CancellationToken token, long requestGeneration)
        {
            if (token.IsCancellationRequested) { Retire(); return Failure(PlcStatus.Cancelled); }
            if (remaining == 0) { Retire(); return Failure(PlcStatus.Timeout); }
            bool write = request.Command == PlcCommand.Write;
            Stopwatch elapsed = Stopwatch.StartNew();
            try
            {
                pending = Task.Factory.StartNew(() => RoundTrip(request), CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                using (var deadline = CancellationTokenSource.CreateLinkedTokenSource(token))
                {
                    Task expiration = Task.Delay(remaining, deadline.Token);
                    int completed = Task.WaitAny(pending, expiration);
                    bool cancelled = token.IsCancellationRequested || requestGeneration != Interlocked.Read(ref generation);
                    bool expired = completed != 0 || elapsed.ElapsedMilliseconds >= remaining;
                    deadline.Cancel();
                    if (cancelled || expired)
                    {
                        Retire();
                        return Failure(cancelled ? PlcStatus.Cancelled : PlcStatus.Timeout, unknown: write);
                    }
                }
                PlcReply reply = pending.GetAwaiter().GetResult();
                pending = null;
                if (reply.Error.Length != 0) { Retire(); return Failure(PlcStatus.WorkerError, unknown: write, detail: reply.Error); }
                if (reply.Code != 1) { Retire(); return Failure(PlcStatus.DriverError, reply.Code, write); }
                return new PlcResult<byte[]>(PlcStatus.Success, reply.Data, reply.Code);
            }
            catch (Exception error)
            {
                Retire();
                return Failure(PlcStatus.WorkerError, unknown: write, detail: error.GetType().Name + ": " + error.Message);
            }
        }

        /// <summary>一条管道只交付一条应答；中断后整条管道废弃，迟到数据不会进入新会话。</summary>
        private PlcReply RoundTrip(PlcRequest request)
        {
            request.Write(writer);
            return PlcReply.Read(reader);
        }

        /// <summary>将两个单向匿名管道交给隐藏子进程，不在命令行传入设备地址或数据。</summary>
        private void StartWorker()
        {
            lock (LaunchSync)
            {
                input = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
                output = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
                string arguments = PlcWorker.Switch + " " + input.GetClientHandleAsString() + " " + output.GetClientHandleAsString() + " " + Process.GetCurrentProcess().Id;
                worker = Process.Start(new ProcessStartInfo(executable, arguments) { UseShellExecute = false, CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden, WorkingDirectory = Path.GetDirectoryName(executable) });
                input.DisposeLocalCopyOfClientHandle(); output.DisposeLocalCopyOfClientHandle();
                writer = new BinaryWriter(input); reader = new BinaryReader(output);
            }
        }

        /// <summary>终止本会话拥有的进程并确认退出；失败时保留实际占用，禁止新建替代连接。</summary>
        private bool Retire()
        {
            connected = false;
            if (worker != null && !worker.HasExited)
            {
                try { worker.Kill(); }
                catch (InvalidOperationException) { }
                catch (System.ComponentModel.Win32Exception) { return false; }
                if (!worker.WaitForExit(1000)) return false;
            }
            if (pending != null && !pending.IsCompleted && !((IAsyncResult)pending).AsyncWaitHandle.WaitOne(1000)) return false;
            if (pending != null && pending.IsFaulted) { var observed = pending.Exception; }
            pending = null;
            if (input != null) input.Dispose();
            if (output != null) output.Dispose();
            if (worker != null) worker.Dispose();
            input = null; output = null; worker = null; reader = null; writer = null;
            return true;
        }

        /// <summary>在已有请求完成后关闭指定连接，保持与读写相同的占用规则。</summary>
        internal bool Disconnect()
        {
            lock (stateSync)
            {
                generation++;
                if (activeCancellation != null) activeCancellation.Cancel();
            }
            gate.Wait();
            try { return Retire(); }
            finally { gate.Release(); }
        }

        /// <summary>在正常应用关闭路径回收所有通信进程。</summary>
        internal static void CloseAll()
        {
            foreach (PlcSession session in Sessions.Values) session.Disconnect();
        }

        /// <summary>计算扣除排队和重试时间后的剩余时限。</summary>
        private static int Remaining(int limit, Stopwatch elapsed)
        {
            return (int)Math.Max(0, limit - elapsed.ElapsedMilliseconds);
        }

        /// <summary>创建不携带数据的通信失败结果。</summary>
        private static PlcResult<byte[]> Failure(PlcStatus status, int code = 0, bool unknown = false, string detail = "")
        {
            return new PlcResult<byte[]>(status, driverCode: code, unknown: unknown, detail: detail);
        }
    }
}
