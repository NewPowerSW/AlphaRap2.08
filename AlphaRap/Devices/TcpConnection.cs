using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AlphaRap
{
    /// <summary>一次 TCP 连接的所有权、完整行缓冲和可选停止命令清理。</summary>
    internal class TcpConnection
    {
        internal readonly TcpClient Client = new TcpClient();
        private readonly Socket socket;
        private readonly Encoding textEncoding;
        private readonly string stopText;
        internal readonly string Host;
        internal readonly int Port;
        internal NetworkStream Stream;
        internal bool Used, RawPending;
        internal volatile bool TriggerSent, Sending, CommandStarted;
        private readonly Queue<byte> buffered = new Queue<byte>();
        private readonly object stopSync = new object();
        private Task stopTask;
        private bool skipLf;
        internal Task PendingCleanup { get { lock (stopSync) return stopTask ?? Task.FromResult(0); } }

        /// <summary>保存不可变端点，不在构造时连接设备。</summary>
        internal TcpConnection(string host, int port, Encoding encoding, string stopCommand = null) { Host = host; Port = port; socket = Client.Client; textEncoding = encoding; stopText = stopCommand; }

        /// <summary>建立套接字后获取流，实际超时和取消由所属操作关闭该连接。</summary>
        internal async Task ConnectAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            IPAddress[] addresses = await ResolveAsync(token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            await Task.Factory.FromAsync((callback, context) => socket.BeginConnect(addresses, Port, callback, context), socket.EndConnect, null).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            socket.NoDelay = true;
            Stream = new NetworkStream(socket, false);
        }

        /// <summary>允许现有主机名配置；停止后丢弃迟到 DNS 结果，不继续建立设备连接。</summary>
        private async Task<IPAddress[]> ResolveAsync(CancellationToken token)
        {
            IPAddress address;
            if (IPAddress.TryParse(Host, out address)) return new[] { address };
            Task<IPAddress[]> resolution = Dns.GetHostAddressesAsync(Host);
            using (var waiting = CancellationTokenSource.CreateLinkedTokenSource(token))
            {
                Task stopped = Task.Delay(Timeout.Infinite, waiting.Token);
                Task completed = await Task.WhenAny(resolution, stopped).ConfigureAwait(false);
                waiting.Cancel();
                if (completed == resolution) return await resolution.ConfigureAwait(false);
            }
            Task observed = resolution.ContinueWith(task => { var error = task.Exception; }, CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            token.ThrowIfCancellationRequested();
            throw new OperationCanceledException(token);
        }

        /// <summary>通过最近的套接字状态检查已知断开，不把握手等同于成功扫码。</summary>
        internal bool IsOpen
        {
            get
            {
                try { return Stream != null && socket.Connected && !(socket.Poll(0, SelectMode.SelectRead) && socket.Available == 0); }
                catch (SocketException) { return false; }
                catch (ObjectDisposedException) { return false; }
            }
        }

        /// <summary>每次只发送一条命令，保留触发可能已发送的诊断状态。</summary>
        internal async Task SendAsync(string text, CancellationToken token, bool trigger)
        {
            token.ThrowIfCancellationRequested();
            byte[] bytes = textEncoding.GetBytes(text);
            CommandStarted = true; Sending = true;
            try { await Stream.WriteAsync(bytes, 0, bytes.Length, token).ConfigureAwait(false); }
            finally { Sending = false; }
            if (trigger) TriggerSent = true;
            token.ThrowIfCancellationRequested();
        }

        /// <summary>读取 CR、LF 或 CRLF 结束的一整行，跨包累计并保留同包的后续完整行。</summary>
        internal async Task<string> ReadLineAsync(int maximumBytes, CancellationToken token)
        {
            var frame = new List<byte>();
            byte[] chunk = new byte[Math.Min(4096, maximumBytes + 2)];
            while (true)
            {
                token.ThrowIfCancellationRequested();
                if (buffered.Count == 0) await FillAsync(chunk, token).ConfigureAwait(false);
                byte next = buffered.Dequeue();
                if (skipLf) { skipLf = false; if (next == 10) continue; }
                if (next == 13) { skipLf = true; return textEncoding.GetString(frame.ToArray()).Trim(); }
                if (next == 10) return textEncoding.GetString(frame.ToArray()).Trim();
                if (frame.Count >= maximumBytes) throw new InvalidDataException("FrameLength");
                frame.Add(next);
            }
        }

        /// <summary>把一段实际接收的数据放入本连接的缓冲，零字节按远端断开处理。</summary>
        private async Task FillAsync(byte[] chunk, CancellationToken token)
        {
            int count = await Stream.ReadAsync(chunk, 0, chunk.Length, token).ConfigureAwait(false);
            if (count == 0) throw new EndOfStreamException();
            for (int index = 0; index < count; index++) buffered.Enqueue(chunk[index]);
        }

        /// <summary>读取实际收到的原始字节；此入口不猜测二进制协议的报文边界。</summary>
        internal async Task<byte[]> ReadSomeAsync(int maximumBytes, CancellationToken token)
        {
            byte[] chunk = new byte[maximumBytes];
            if (buffered.Count == 0) await FillAsync(chunk, token).ConfigureAwait(false);
            int count = Math.Min(buffered.Count, maximumBytes);
            byte[] result = new byte[count];
            for (int index = 0; index < count; index++) result[index] = buffered.Dequeue();
            return result;
        }

        /// <summary>发送经过上层协议编码的原始字节，不自动追加或清空数据。</summary>
        internal async Task WriteBytesAsync(byte[] bytes, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            CommandStarted = true; Sending = true;
            try { await Stream.WriteAsync(bytes, 0, bytes.Length, token).ConfigureAwait(false); }
            finally { Sending = false; }
            token.ThrowIfCancellationRequested();
        }

        /// <summary>合并重复停止；可选停止命令最多等待一百毫秒，随后关闭本连接。</summary>
        internal Task AbortAsync()
        {
            TaskCompletionSource<bool> completion;
            lock (stopSync)
            {
                if (stopTask != null) return stopTask;
                completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                stopTask = completion.Task;
            }
            Task cleanup = CompleteAbortAsync(completion);
            return completion.Task;
        }

        /// <summary>关闭套接字使挂起收发实际返回，清理异常不改变原始通信失败。</summary>
        private async Task CompleteAbortAsync(TaskCompletionSource<bool> completion)
        {
            try { await CloseWireAsync().ConfigureAwait(false); completion.TrySetResult(true); }
            catch (Exception error) { completion.TrySetException(error); }
        }

        /// <summary>最多尝试一条停止命令，然后关闭连接并观察已派发发送的结束。</summary>
        private async Task CloseWireAsync()
        {
            Task sending = null;
            try
            {
                if (Stream != null && TriggerSent && !Sending && stopText != null)
                {
                    byte[] stop = Encoding.ASCII.GetBytes(stopText);
                    sending = Stream.WriteAsync(stop, 0, stop.Length);
                    await Task.WhenAny(sending, Task.Delay(100)).ConfigureAwait(false);
                }
            }
            catch (Exception error) when (error is IOException || error is SocketException || error is ObjectDisposedException || error is InvalidOperationException) { }
            finally { Client.Close(); if (Stream != null) Stream.Dispose(); }
            if (sending != null) await ObserveAsync(sending).ConfigureAwait(false);
        }

        /// <summary>观察连接关闭后已结束的发送，不遗留未观察的异步异常。</summary>
        private static async Task ObserveAsync(Task task)
        {
            try { await task.ConfigureAwait(false); }
            catch (Exception error) when (error is IOException || error is SocketException || error is ObjectDisposedException || error is InvalidOperationException) { }
        }
    }
}
