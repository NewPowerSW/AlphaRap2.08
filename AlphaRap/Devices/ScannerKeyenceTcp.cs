using System;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using AlphaRap.FlowProgramming;
using NPSDK;

namespace AlphaRap
{
    /// <summary>基恩士行协议扫码枪，所有入口共用本实例的连接和请求租约。</summary>
    public partial class ScannerKeyenceTcp : AbstractDevice
    {
        private enum Operation { Connect, Scan, Send, Receive }
        private readonly object stateSync = new object();
        private readonly SemaphoreSlim requests = new SemaphoreSlim(1, 1);
        private ScannerConnection connection;
        private CancellationTokenSource activeCancellation;
        private long generation;
        private bool scanning;
        private ScannerConnectionState state;
        private ScannerResult lastResult = new ScannerResult(ScannerStatus.NotConnected);
        private Task stopCompletion = Task.FromResult(0);

        /// <summary>建立设备身份，不在构造或设计器浏览阶段连接硬件。</summary>
        public ScannerKeyenceTcp(string deviceName) : base(deviceName) { }

        public string IP { get; set; } = "192.168.100.100";
        public int Port { get; set; } = 9004;
        /// <summary>连接或一条完整命令的总上限，含排队、发送与完整应答，单位毫秒。</summary>
        public int TimeOut { get; set; } = 1000;
        /// <summary>一条应答在终止符之前允许的最大字节数。</summary>
        public int length { get; set; } = 100;

        [Browsable(false)]
        public ScannerConnectionState ConnectionState { get { lock (stateSync) return state; } }
        [Browsable(false)]
        public ScannerResult LastResult { get { lock (stateSync) return lastResult; } }
        public override bool IsRunning { get { lock (stateSync) return scanning; } }
        public override bool IsConnected
        {
            get { lock (stateSync) return state == ScannerConnectionState.Connected && connection != null && connection.Host == IP && connection.Port == Port && connection.IsOpen; }
        }
        public override bool SupportsRawIo { get { return true; } }

        /// <summary>连接兼容入口，将失败返回给调用方统一显示或进入流程故障。</summary>
        public override void Open()
        {
            Flow_Chart.ThrowIfExecutionStopped();
            CancellationToken token = Flow_Chart.CurrentExecutionToken;
            ConnectAsync(token).GetAwaiter().GetResult().RequireValue(token);
            Flow_Chart.ThrowIfExecutionStopped();
        }

        /// <summary>异步显式连接，重复打开同一端点不创建第二条连接。</summary>
        [FlowToolIgnore]
        public Task<ScannerResult> ConnectAsync(CancellationToken token = default(CancellationToken))
        {
            return ExecuteAsync(Operation.Connect, null, token);
        }

        /// <summary>停止和关闭使用同一路径，包含正在连接或已断线的旧套接字。</summary>
        public override void Close() { StopAsync().GetAwaiter().GetResult(); }

        /// <summary>保持旧启动语义，只建立连接，不自动触发扫码。</summary>
        public override void StartRunning() { Open(); }

        /// <summary>撤销排队请求并中断当前连接，实际收发退出后返回。</summary>
        public override void StopRunning() { StopAsync().GetAwaiter().GetResult(); }

        /// <summary>停止后保持未连接状态，需要显式打开才可重新扫码。</summary>
        [FlowToolIgnore]
        public Task StopAsync()
        {
            lock (stateSync)
            {
                if (!stopCompletion.IsCompleted) return stopCompletion;
                generation++; state = ScannerConnectionState.Closing;
                if (activeCancellation != null) activeCancellation.Cancel();
                stopCompletion = StopOwnedAsync(connection);
                return stopCompletion;
            }
        }

        /// <summary>确认旧套接字和占用它的操作全部退出，再结束停止状态。</summary>
        private async Task StopOwnedAsync(ScannerConnection closing)
        {
            if (closing != null) await closing.AbortAsync().ConfigureAwait(false);
            await requests.WaitAsync().ConfigureAwait(false);
            try
            {
                lock (stateSync)
                {
                    if (ReferenceEquals(connection, closing)) connection = null;
                    if (connection == null) state = ScannerConnectionState.Disconnected;
                }
            }
            finally { requests.Release(); }
        }

        /// <summary>旧代码保留 ERROR 哨兵；取消抛出异常，新流程使用类型化结果。</summary>
        [FlowToolIgnore]
        public string ScanOnce()
        {
            Flow_Chart.ThrowIfExecutionStopped();
            CancellationToken token = Flow_Chart.CurrentExecutionToken;
            ScannerResult result = ScanOnceAsync(token).GetAwaiter().GetResult();
            if (result.Status == ScannerStatus.Cancelled) result.RequireValue(token);
            Flow_Chart.ThrowIfExecutionStopped();
            return result.Succeeded ? result.Value : "ERROR";
        }

        /// <summary>一次触发只接收一条完整条码，不自动重新触发或重试失败请求。</summary>
        [FlowToolIgnore]
        public Task<ScannerResult> ScanOnceAsync(CancellationToken token = default(CancellationToken))
        {
            return ExecuteAsync(Operation.Scan, null, token);
        }

        /// <summary>兼容手动发送；一条原始命令必须接收或停止后才接受下一条命令。</summary>
        public override string Send(string data)
        {
            Flow_Chart.ThrowIfExecutionStopped();
            string text = data ?? "";
            if (text.Length > 65536) return Record(new ScannerResult(ScannerStatus.InvalidConfiguration)).Message;
            if (!text.EndsWith("\n", StringComparison.Ordinal)) text += "\r\n";
            if (text == "LOFF\r\n") { StopRunning(); return ""; }
            CancellationToken token = Flow_Chart.CurrentExecutionToken;
            ScannerResult result = ExecuteAsync(Operation.Send, text, token).GetAwaiter().GetResult();
            if (result.Status == ScannerStatus.Cancelled) result.RequireValue(token);
            Flow_Chart.ThrowIfExecutionStopped();
            return result.Message;
        }

        /// <summary>兼容手动接收，失败返回空串，LastResult 保留具体通信原因。</summary>
        public override string Receive()
        {
            CancellationToken token = Flow_Chart.CurrentExecutionToken;
            ScannerResult result = ReceiveAsync(token).GetAwaiter().GetResult();
            if (result.Status == ScannerStatus.Cancelled) result.RequireValue(token);
            Flow_Chart.ThrowIfExecutionStopped();
            return result.Succeeded ? result.Value : "";
        }

        /// <summary>读取完整应答并保留错误状态，供对象方法和诊断调用。</summary>
        [FlowToolIgnore]
        public Task<ScannerResult> ReceiveAsync(CancellationToken token = default(CancellationToken))
        {
            return ExecuteAsync(Operation.Receive, null, token);
        }

        /// <summary>固定快照、排队和执行共用一个超时，排队失败不关闭其他请求的连接。</summary>
        private async Task<ScannerResult> ExecuteAsync(Operation operation, string text, CancellationToken token)
        {
            string host = IP; int port = Port, timeout = TimeOut, maximum = length;
            if (string.IsNullOrWhiteSpace(host) || host.Length > 255 || port < 1 || port > 65535 || timeout < 1 || timeout > 60000 || maximum < 1 || maximum > 65536)
                return Record(new ScannerResult(ScannerStatus.InvalidConfiguration));
            long version;
            lock (stateSync) version = generation;
            using (var deadline = CancellationTokenSource.CreateLinkedTokenSource(token))
            {
                deadline.CancelAfter(timeout);
                try { await requests.WaitAsync(deadline.Token).ConfigureAwait(false); }
                catch (OperationCanceledException) { return Record(Cancelled(token, version)); }
                try { return await ExecuteOwnedAsync(operation, text, host, port, maximum, version, token, deadline).ConfigureAwait(false); }
                finally { requests.Release(); }
            }
        }

        /// <summary>登记当前实际操作，清理完成前不释放扫码状态或请求租约。</summary>
        private async Task<ScannerResult> ExecuteOwnedAsync(Operation operation, string text, string host, int port, int maximum,
            long version, CancellationToken token, CancellationTokenSource deadline)
        {
            lock (stateSync)
            {
                if (version != generation || state == ScannerConnectionState.Closing) return new ScannerResult(ScannerStatus.Cancelled);
                activeCancellation = deadline; scanning = operation == Operation.Scan;
            }
            try
            {
                ScannerResult result = await ExecuteWireAsync(operation, text, host, port, maximum, version, token, deadline.Token).ConfigureAwait(false);
                return Record(result);
            }
            finally { lock (stateSync) { activeCancellation = null; scanning = false; } }
        }

        /// <summary>所有协议动作只访问当前实例的连接，使用过的扫码连接先退休再触发下一次。</summary>
        private async Task<ScannerResult> ExecuteWireAsync(Operation operation, string text, string host, int port, int maximum,
            long version, CancellationToken caller, CancellationToken token)
        {
            ScannerConnection current;
            lock (stateSync) current = connection;
            if (operation != Operation.Connect && (current == null || ConnectionState != ScannerConnectionState.Connected)) return new ScannerResult(ScannerStatus.NotConnected);
            if (current != null && current.RawPending && (operation == Operation.Scan || operation == Operation.Send)) return new ScannerResult(ScannerStatus.Busy);
            if (operation == Operation.Connect && current != null && current.IsOpen && current.Host == host && current.Port == port) return new ScannerResult(ScannerStatus.Success, "");
            bool replace = operation == Operation.Connect || ((operation == Operation.Scan || operation == Operation.Send) && current.Used);
            if (replace && current != null) await current.AbortAsync().ConfigureAwait(false);
            if (replace && current != null && (current.Host != host || current.Port != port))
            {
                lock (stateSync)
                {
                    if (version != generation) return new ScannerResult(ScannerStatus.Cancelled);
                    version = ++generation;
                }
            }
            if (replace) current = CreateConnection(host, port, version, token);
            if (current == null) return Cancelled(caller, version);
            if (current.Host != host || current.Port != port) return new ScannerResult(ScannerStatus.InvalidConfiguration);
            return await PerformAsync(current, operation, text, maximum, version, caller, token).ConfigureAwait(false);
        }

        /// <summary>在停止标记之后不再创建套接字，端点替换会撤销旧队列。</summary>
        private ScannerConnection CreateConnection(string host, int port, long version, CancellationToken token)
        {
            lock (stateSync)
            {
                if (version != generation || token.IsCancellationRequested) return null;
                connection = new ScannerConnection(host, port); state = ScannerConnectionState.Connecting;
                return connection;
            }
        }

        /// <summary>取消会关闭本次捕获的套接字，使 .NET Framework 中的挂起收发实际返回。</summary>
        private async Task<ScannerResult> PerformAsync(ScannerConnection current, Operation operation, string text, int maximum,
            long version, CancellationToken caller, CancellationToken token)
        {
            CancellationTokenRegistration cancellation = token.Register(() => { current.AbortAsync(); });
            try
            {
                if (current.Stream == null) await current.ConnectAsync(token).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                if (operation == Operation.Connect) return Publish(current, version, "", token);
                current.Used = true;
                if (operation == Operation.Send || operation == Operation.Scan)
                    await current.SendAsync(operation == Operation.Scan ? "LON\r\n" : text, token, operation == Operation.Scan || text == "LON\r\n").ConfigureAwait(false);
                if (operation == Operation.Send) { current.RawPending = true; return Publish(current, version, "", token); }
                string value = await current.ReadLineAsync(maximum, token).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                if (string.IsNullOrWhiteSpace(value) || value == "ERROR") return await FailAsync(current, new ScannerResult(ScannerStatus.Rejected)).ConfigureAwait(false);
                if (operation == Operation.Scan)
                {
                    await current.SendAsync("LOFF\r\n", token, false).ConfigureAwait(false);
                    current.TriggerSent = false;
                }
                current.RawPending = false;
                return Publish(current, version, value, token);
            }
            catch (Exception error) when (error is IOException || error is InvalidDataException || error is SocketException || error is ObjectDisposedException || error is OperationCanceledException || error is InvalidOperationException || error is ArgumentException)
            {
                bool uncertain = current.CommandStarted && (operation != Operation.Receive || current.RawPending);
                ScannerResult result = Classify(error, caller, token, version, uncertain);
                return await FailAsync(current, result).ConfigureAwait(false);
            }
            finally
            {
                cancellation.Dispose();
                await current.PendingCleanup.ConfigureAwait(false);
            }
        }

        /// <summary>仅当前端点和代次仍有效时发布成功，停止后的返回值被拒绝。</summary>
        private ScannerResult Publish(ScannerConnection current, long version, string value, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            lock (stateSync)
            {
                if (version != generation || !ReferenceEquals(connection, current) || current.Host != IP || current.Port != Port)
                    throw new OperationCanceledException(token);
                state = ScannerConnectionState.Connected;
                return new ScannerResult(ScannerStatus.Success, value);
            }
        }

        /// <summary>故障后断开并撤销旧队列，只有显式打开能恢复设备通信。</summary>
        private async Task<ScannerResult> FailAsync(ScannerConnection current, ScannerResult result)
        {
            await current.AbortAsync().ConfigureAwait(false);
            lock (stateSync)
            {
                if (ReferenceEquals(connection, current)) connection = null;
                if (state != ScannerConnectionState.Closing) { state = ScannerConnectionState.Faulted; generation++; }
            }
            if (result.CommandMayHaveExecuted) Flow_Module.Module_AddAlarmLog(result.Message);
            return result;
        }

        /// <summary>区别用户取消、总时限、远端断开和不完整协议，失败没有条码数据。</summary>
        private ScannerResult Classify(Exception error, CancellationToken caller, CancellationToken token, long version, bool uncertain)
        {
            if (token.IsCancellationRequested || error is OperationCanceledException)
            {
                if (!token.IsCancellationRequested) return new ScannerResult(ScannerStatus.Cancelled, uncertain: uncertain);
                ScannerResult cancellation = Cancelled(caller, version);
                return new ScannerResult(cancellation.Status, uncertain: uncertain);
            }
            if (error is InvalidDataException) return new ScannerResult(ScannerStatus.ProtocolError, uncertain: uncertain);
            if (error is EndOfStreamException) return new ScannerResult(ScannerStatus.Disconnected, uncertain: uncertain);
            if (error is ArgumentException) return new ScannerResult(ScannerStatus.InvalidConfiguration);
            return new ScannerResult(ScannerStatus.TransportError, detail: error.GetType().Name + ": " + error.Message, uncertain: uncertain);
        }

        /// <summary>队列和执行共用取消分类，不把另一条请求的超时算作本请求已发送。</summary>
        private ScannerResult Cancelled(CancellationToken token, long version)
        {
            lock (stateSync) return new ScannerResult(token.IsCancellationRequested || version != generation ? ScannerStatus.Cancelled : ScannerStatus.Timeout);
        }

        /// <summary>保存最近结果供手动诊断，状态不参与设备配置持久化。</summary>
        private ScannerResult Record(ScannerResult result)
        {
            lock (stateSync) lastResult = result;
            return result;
        }
    }
}
