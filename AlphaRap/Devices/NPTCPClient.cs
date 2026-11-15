using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AlphaRap;
using NPSDK;

namespace NPClient
{
    /// <summary>共用连接、串行请求及实际取消的 TCP 客户端，旧分步接口保留原签名。</summary>
    public class TCPCLient : IDisposable
    {
        private readonly SemaphoreSlim gate = new SemaphoreSlim(1, 1);
        private readonly object sync = new object();
        private TcpConnection connection;
        private CancellationTokenSource active;
        private long generation;
        private bool connecting, closing;
        private Task stop = Task.FromResult(0);
        private CommunicationStatus lastStatus = CommunicationStatus.NotConnected;
        public CommunicationStatus LastStatus { get { lock (sync) return lastStatus; } }

        /// <summary>兼容连接入口，总上限三秒，取消令牌来自当前流程或手动动作。</summary>
        public bool Connect(string strIP, int intPort)
        {
            Flow_Chart.ThrowIfExecutionStopped();
            CancellationToken token = Flow_Chart.CurrentExecutionToken;
            CommunicationResult<bool> result = ConnectAsync(strIP, intPort, 3000, token).GetAwaiter().GetResult();
            if (result.Status == CommunicationStatus.Cancelled) result.RequireValue(token);
            Flow_Chart.ThrowIfExecutionStopped();
            return result.Succeeded;
        }

        /// <summary>显式关闭旧连接后重新握手，不重放之前的数据命令。</summary>
        public bool Reconnect(string strIP, int intPort) { Disconnect(); return Connect(strIP, intPort); }

        /// <summary>异步连接同一端点时复用会话；端点替换会撤销旧请求。</summary>
        public Task<CommunicationResult<bool>> ConnectAsync(string host, int port, int timeoutMs = 3000, CancellationToken token = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(host) || host.Length > 255 || port < 1 || port > 65535) return Task.FromResult(Failure<bool>(CommunicationStatus.InvalidConfiguration));
            if (token.IsCancellationRequested) return Task.FromResult(Failure<bool>(CommunicationStatus.Cancelled));
            lock (sync)
            {
                if (connection != null && (connection.Host != host || connection.Port != port || (!connecting && !connection.IsOpen)))
                {
                    generation++;
                    if (active != null) active.Cancel();
                }
            }
            return ExecuteAsync((wire, cancellation) => Task.FromResult(true), timeoutMs, token, host, port);
        }

        /// <summary>检查当前实际套接字；连接成功不等同于设备协议就绪。</summary>
        public bool ConnectStatus() { lock (sync) return !closing && !connecting && connection != null && connection.IsOpen; }

        /// <summary>关闭连接并等待占用它的实际请求结束。</summary>
        public void Disconnect() { DisconnectAsync().GetAwaiter().GetResult(); }

        /// <summary>合并重复断开，停止之后不允许旧队列建立新连接。</summary>
        public Task DisconnectAsync()
        {
            lock (sync)
            {
                if (!stop.IsCompleted) return stop;
                generation++; closing = true;
                if (active != null) active.Cancel();
                stop = CloseOwnedAsync(connection);
                return stop;
            }
        }

        /// <summary>回收捕获的套接字并确认请求租约已经释放。</summary>
        private async Task CloseOwnedAsync(TcpConnection old)
        {
            if (old != null) await old.AbortAsync().ConfigureAwait(false);
            await gate.WaitAsync().ConfigureAwait(false);
            try { lock (sync) { if (ReferenceEquals(connection, old)) connection = null; connecting = false; closing = false; } }
            finally { gate.Release(); }
        }

        /// <summary>释放此实例拥有的连接。</summary>
        public void Dispose() { Disconnect(); }

        /// <summary>兼容字节发送，不改变二进制内容。</summary>
        public bool Sent(byte[] data)
        {
            Flow_Chart.ThrowIfExecutionStopped();
            CancellationToken token = Flow_Chart.CurrentExecutionToken;
            var result = SendAsync(data, 100, token).GetAwaiter().GetResult();
            if (result.Status == CommunicationStatus.Cancelled) result.RequireValue(token);
            Flow_Chart.ThrowIfExecutionStopped(); return result.Succeeded;
        }

        /// <summary>兼容 UTF-8 文本发送，不自动追加换行。</summary>
        public bool Sent(string message) { return message != null && Sent(Encoding.UTF8.GetBytes(message)); }

        /// <summary>异步发送一份不可变字节快照，失败后不自动重发。</summary>
        public Task<CommunicationResult<bool>> SendAsync(byte[] bytes, int timeoutMs = 1000, CancellationToken token = default(CancellationToken))
        {
            if (bytes == null || bytes.Length > 1048576) return Task.FromResult(Failure<bool>(CommunicationStatus.InvalidConfiguration));
            byte[] snapshot = (byte[])bytes.Clone();
            return ExecuteAsync(async (wire, cancellation) => { await wire.WriteBytesAsync(snapshot, cancellation).ConfigureAwait(false); return true; }, timeoutMs, token);
        }

        /// <summary>读取实际字节，原始模式不把一次网络读取当作协议完整报文。</summary>
        public Task<CommunicationResult<byte[]>> ReceiveAsync(int timeoutMs = 1000, CancellationToken token = default(CancellationToken), int maximumBytes = 1024)
        {
            if (maximumBytes < 1 || maximumBytes > 1048576) return Task.FromResult(Failure<byte[]>(CommunicationStatus.InvalidConfiguration));
            return ExecuteAsync((wire, cancellation) => wire.ReadSomeAsync(maximumBytes, cancellation), timeoutMs, token);
        }

        /// <summary>为行协议显式读取完整 CR/LF/CRLF 报文。</summary>
        public Task<CommunicationResult<string>> ReceiveLineAsync(int timeoutMs = 1000, CancellationToken token = default(CancellationToken), int maximumBytes = 65536)
        {
            if (maximumBytes < 1 || maximumBytes > 1048576) return Task.FromResult(Failure<string>(CommunicationStatus.InvalidConfiguration));
            return ExecuteAsync((wire, cancellation) => wire.ReadLineAsync(maximumBytes, cancellation), timeoutMs, token);
        }

        /// <summary>兼容指定等待时间的原始读取，失败仍返回 null。</summary>
        public byte[] ReceiveByte(int intTMOut)
        {
            Flow_Chart.ThrowIfExecutionStopped();
            CancellationToken token = Flow_Chart.CurrentExecutionToken;
            var result = ReceiveAsync(LegacyTimeout(intTMOut), token).GetAwaiter().GetResult();
            if (result.Status == CommunicationStatus.Cancelled) result.RequireValue(token);
            Flow_Chart.ThrowIfExecutionStopped(); return result.Value;
        }

        /// <summary>兼容立即探测原始数据的旧入口。</summary>
        public byte[] ReceiveByte() { return ReceiveByte(1); }

        /// <summary>兼容旧文本错误标记；新代码应使用状态化接收入口。</summary>
        public string Receive(int intTMOut)
        {
            byte[] value = ReceiveByte(intTMOut);
            return value != null ? Encoding.UTF8.GetString(value) : LastStatus == CommunicationStatus.Timeout ? "Err,TimeOut" : "Err";
        }

        /// <summary>兼容一毫秒读取及旧 Null 标记。</summary>
        public string Receive() { string value = Receive(1); return value == "Err,TimeOut" ? "Null" : value == "Err" ? "Err,Fail" : value; }

        /// <summary>一份协议事务在同一租约中执行，不允许分步读写交叉；可隔离已使用连接。</summary>
        internal Task<CommunicationResult<T>> ConverseAsync<T>(Func<TcpConnection, CancellationToken, Task<T>> action, int timeoutMs, CancellationToken token, bool fresh = true)
        {
            return ExecuteAsync(action, timeoutMs, token, fresh: fresh);
        }

        /// <summary>排队及整个事务共用时限，排队取消不关闭他人的连接。</summary>
        private async Task<CommunicationResult<T>> ExecuteAsync<T>(Func<TcpConnection, CancellationToken, Task<T>> action, int timeoutMs, CancellationToken caller,
            string host = null, int port = 0, bool fresh = false)
        {
            if (timeoutMs < 1 || timeoutMs > 60000) return Failure<T>(CommunicationStatus.InvalidConfiguration);
            long version;
            lock (sync) version = generation;
            using (var deadline = CancellationTokenSource.CreateLinkedTokenSource(caller))
            {
                deadline.CancelAfter(timeoutMs);
                try { await gate.WaitAsync(deadline.Token).ConfigureAwait(false); }
                catch (OperationCanceledException) { return Failure<T>(CancelStatus(caller, version)); }
                try { return await ExecuteOwnedAsync(action, host, port, fresh, version, caller, deadline).ConfigureAwait(false); }
                finally { gate.Release(); }
            }
        }

        /// <summary>持有真正请求占用直到连接和 IO 都退出，停止后的数据不发布。</summary>
        private async Task<CommunicationResult<T>> ExecuteOwnedAsync<T>(Func<TcpConnection, CancellationToken, Task<T>> action, string host, int port, bool fresh,
            long version, CancellationToken caller, CancellationTokenSource deadline)
        {
            TcpConnection wire;
            lock (sync)
            {
                if (version != generation || closing) return Failure<T>(CommunicationStatus.Cancelled);
                active = deadline; wire = connection;
            }
            try
            {
                if (host == null && wire == null) return Failure<T>(CommunicationStatus.NotConnected);
                bool replace = wire != null && ((host != null && (wire.Host != host || wire.Port != port || !wire.IsOpen)) || (fresh && wire.Used));
                if (replace) await wire.AbortAsync().ConfigureAwait(false);
                if (wire == null || replace) wire = CreateWire(host ?? wire.Host, host != null ? port : wire.Port, version, deadline.Token);
                if (wire == null) return Failure<T>(CancelStatus(caller, version));
                return await InvokeWireAsync(wire, action, version, caller, deadline.Token, host == null).ConfigureAwait(false);
            }
            finally { lock (sync) active = null; }
        }

        /// <summary>只在当前代次创建连接，不复用已释放套接字。</summary>
        private TcpConnection CreateWire(string host, int port, long version, CancellationToken token)
        {
            lock (sync)
            {
                if (version != generation || closing || token.IsCancellationRequested) return null;
                connecting = true;
                return connection = new TcpConnection(host, port, Encoding.UTF8);
            }
        }

        /// <summary>取消只关闭本次捕获连接；协议异常、EOF 和超时分别保留原因。</summary>
        private async Task<CommunicationResult<T>> InvokeWireAsync<T>(TcpConnection wire, Func<TcpConnection, CancellationToken, Task<T>> action,
            long version, CancellationToken caller, CancellationToken token, bool used)
        {
            CancellationTokenRegistration registration = token.Register(() => { wire.AbortAsync(); });
            try
            {
                if (wire.Stream == null) await wire.ConnectAsync(token).ConfigureAwait(false);
                lock (sync) connecting = false;
                token.ThrowIfCancellationRequested();
                T value = await action(wire, token).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                lock (sync)
                {
                    if (version != generation || !ReferenceEquals(connection, wire)) return Failure<T>(CommunicationStatus.Cancelled, uncertain: wire.CommandStarted);
                    wire.Used |= used; lastStatus = CommunicationStatus.Success;
                }
                return new CommunicationResult<T>(CommunicationStatus.Success, value);
            }
            catch (Exception error) when (error is IOException || error is InvalidDataException || error is SocketException || error is ObjectDisposedException || error is OperationCanceledException || error is ArgumentException || error is InvalidOperationException)
            {
                CommunicationStatus status = Classify(error, caller, token, version);
                await wire.AbortAsync().ConfigureAwait(false);
                lock (sync) { if (ReferenceEquals(connection, wire)) connection = null; connecting = false; if (!closing && version == generation) generation++; }
                return Failure<T>(status, error is DeviceCommunicationException ? error.Message : error.GetType().Name + ": " + error.Message, wire.CommandStarted);
            }
            finally { registration.Dispose(); await wire.PendingCleanup.ConfigureAwait(false); }
        }

        /// <summary>区别用户取消和总时限，保留协议校验的明确状态。</summary>
        private CommunicationStatus Classify(Exception error, CancellationToken caller, CancellationToken token, long version)
        {
            if (token.IsCancellationRequested) return CancelStatus(caller, version);
            var known = error as DeviceCommunicationException;
            if (known != null) return known.Status;
            if (error is InvalidDataException) return CommunicationStatus.ProtocolError;
            if (error is EndOfStreamException) return CommunicationStatus.Disconnected;
            if (error is OperationCanceledException) return CommunicationStatus.Cancelled;
            if (error is ArgumentException) return CommunicationStatus.InvalidConfiguration;
            return CommunicationStatus.TransportError;
        }

        /// <summary>将排队或执行取消分类，不伪造接收数据。</summary>
        private CommunicationStatus CancelStatus(CancellationToken caller, long version)
        {
            lock (sync) return caller.IsCancellationRequested || version != generation ? CommunicationStatus.Cancelled : CommunicationStatus.Timeout;
        }

        /// <summary>保存状态并创建不含数据的失败结果。</summary>
        private CommunicationResult<T> Failure<T>(CommunicationStatus status, string detail = "", bool uncertain = false)
        {
            lock (sync) lastStatus = status;
            return new CommunicationResult<T>(status, detail: detail, uncertain: uncertain);
        }

        /// <summary>保留旧超时参数的缺省处理，新接口直接校验时限。</summary>
        private static int LegacyTimeout(int timeout) { return timeout <= 0 || timeout >= 60000 ? 500 : timeout; }
    }
}
