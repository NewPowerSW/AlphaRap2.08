using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AlphaRap.FlowProgramming;
using NPClient;
using NPSDK;

namespace AlphaRap
{
    /// <summary>基恩士 3D 的单次切换和测量事务，使用同一设备实例的受控 TCP 会话。</summary>
    public partial class Keyence3DTcp : AbstractDevice
    {
        private readonly TCPCLient client = new TCPCLient();
        private string openedHost;
        private int openedPort, running;
        private CommunicationResult<string> last = new CommunicationResult<string>(CommunicationStatus.NotConnected);

        /// <summary>建立设备身份，构造和工具箱发现不连接控制器。</summary>
        public Keyence3DTcp(string deviceName) : base(deviceName) { }

        public string IP { get; set; } = "192.168.10.10";
        public int Port { get; set; } = 8500;
        public int TimeOut { get; set; } = 1500;
        /// <summary>现场确认的 EXW 成功应答；可用 {position} 表示实际零基位置号，未配置时不执行切换。</summary>
        public string ProgramAck { get; set; } = "";
        /// <summary>测量值可选命令前缀，默认保留原接口的 T1 前缀。</summary>
        public string MeasurementPrefix { get; set; } = "T1";
        [Browsable(false)]
        public CommunicationResult<string> LastResult { get { return last; } }
        public override bool IsConnected { get { return openedHost == IP && openedPort == Port && client.ConnectStatus(); } }
        public override bool IsRunning { get { return Volatile.Read(ref running) != 0; } }
        public override bool SupportsRawIo { get { return true; } }

        /// <summary>连接兼容入口，连接失败由现有页面或流程统一报告。</summary>
        public override void Open()
        {
            Flow_Chart.ThrowIfExecutionStopped();
            CancellationToken token = Flow_Chart.CurrentExecutionToken;
            ConnectAsync(token).GetAwaiter().GetResult().RequireValue(token);
            Flow_Chart.ThrowIfExecutionStopped();
        }

        /// <summary>只握手一次实际会话，不再建立额外探测连接。</summary>
        [FlowToolIgnore]
        public async Task<CommunicationResult<bool>> ConnectAsync(CancellationToken token = default(CancellationToken))
        {
            string host = IP; int port = Port;
            var result = await client.ConnectAsync(host, port, TimeOut, token).ConfigureAwait(false);
            if (result.Succeeded) { openedHost = host; openedPort = port; }
            return result;
        }

        /// <summary>关闭并等待本实例所有实际网络调用退出。</summary>
        public override void Close() { client.Disconnect(); }

        /// <summary>启动只建立连接，不发送测量命令。</summary>
        public override void StartRunning() { Open(); }

        /// <summary>撤销挂起测量，不猜测控制器的硬件停止命令。</summary>
        public override void StopRunning() { Close(); }

        /// <summary>兼容旧高度接口，故障保留 ERROR，取消向运行器抛出。</summary>
        [FlowToolIgnore]
        public string Triger3D(int Position)
        {
            Flow_Chart.ThrowIfExecutionStopped();
            CancellationToken token = Flow_Chart.CurrentExecutionToken;
            var result = TriggerAsync(Position, token).GetAwaiter().GetResult();
            if (result.Status == CommunicationStatus.Cancelled) result.RequireValue(token);
            Flow_Chart.ThrowIfExecutionStopped(); return result.Succeeded ? result.Value : "ERROR";
        }

        /// <summary>同一事务最多发送一次 EXW 和一次 T1；位置 0 只测量，1–6 先确认切换。</summary>
        [FlowToolIgnore]
        public Task<CommunicationResult<string>> TriggerAsync(int position, CancellationToken token = default(CancellationToken))
        {
            return RunAsync(position, true, token);
        }

        /// <summary>兼容原切换方法；失败不再用空字符串伪装完成。</summary>
        [FlowToolIgnore]
        public string Change3D(int Position)
        {
            Flow_Chart.ThrowIfExecutionStopped();
            CancellationToken token = Flow_Chart.CurrentExecutionToken;
            var result = ChangeAsync(Position, token).GetAwaiter().GetResult();
            if (result.Status == CommunicationStatus.Cancelled) result.RequireValue(token);
            Flow_Chart.ThrowIfExecutionStopped(); return result.Succeeded ? result.Value : "ERROR";
        }

        /// <summary>只发送一次切换命令，并匹配现场配置的完整成功应答。</summary>
        [FlowToolIgnore]
        public Task<CommunicationResult<string>> ChangeAsync(int position, CancellationToken token = default(CancellationToken))
        {
            return RunAsync(position, false, token);
        }

        /// <summary>保存协议参数快照，停止、协议失败和超时均禁止自动重发。</summary>
        private async Task<CommunicationResult<string>> RunAsync(int position, bool measure, CancellationToken token)
        {
            if (position < 0 || position > 6 || TimeOut < 1 || TimeOut > 60000) return Record(new CommunicationResult<string>(CommunicationStatus.InvalidConfiguration));
            string acknowledgement = ProgramAck ?? "", prefix = MeasurementPrefix ?? "";
            if (position > 0 && string.IsNullOrWhiteSpace(acknowledgement))
                return Record(new CommunicationResult<string>(CommunicationStatus.InvalidConfiguration, detail: DeviceText.T("error.keyenceAck")));
            if (!IsConnected) return Record(new CommunicationResult<string>(CommunicationStatus.NotConnected));
            if (!measure && position == 0) return Record(new CommunicationResult<string>(CommunicationStatus.Success, ""));
            string host = IP; int port = Port;
            Interlocked.Increment(ref running);
            try
            {
                var result = await client.ConverseAsync(async (wire, cancellation) =>
                {
                    if (IP != host || Port != port) throw new OperationCanceledException(cancellation);
                    string response = "";
                    if (position > 0) response = await SelectAsync(wire, position, acknowledgement, cancellation).ConfigureAwait(false);
                    if (measure)
                    {
                        await wire.SendAsync("T1\r\n", cancellation, false).ConfigureAwait(false);
                        response = ParseHeight(await wire.ReadLineAsync(65536, cancellation).ConfigureAwait(false), prefix);
                    }
                    if (IP != host || Port != port) throw new OperationCanceledException(cancellation);
                    return response;
                }, TimeOut, token).ConfigureAwait(false);
                return Record(result);
            }
            finally { Interlocked.Decrement(ref running); }
        }

        /// <summary>切换位置号沿用原零基映射，未匹配应答之前不发送测量触发。</summary>
        private static async Task<string> SelectAsync(TcpConnection wire, int position, string acknowledgement, CancellationToken token)
        {
            string index = (position - 1).ToString(CultureInfo.InvariantCulture);
            await wire.SendAsync("EXW," + index + "\r\n", token, false).ConfigureAwait(false);
            string response = await wire.ReadLineAsync(4096, token).ConfigureAwait(false);
            if (!string.Equals(response, acknowledgement.Replace("{position}", index), StringComparison.Ordinal))
                throw new DeviceCommunicationException(CommunicationStatus.Unconfirmed, true, response);
            return response;
        }

        /// <summary>只接受完整的有限标量，不删除中间字符或把多个输出拼成一个高度。</summary>
        private static string ParseHeight(string response, string prefix)
        {
            if (response.StartsWith("ER", StringComparison.OrdinalIgnoreCase)) throw new DeviceCommunicationException(CommunicationStatus.Rejected, true, response);
            string value = !string.IsNullOrEmpty(prefix) && response.StartsWith(prefix, StringComparison.Ordinal) ? response.Substring(prefix.Length).Trim() : response.Trim();
            double number;
            bool parsed = double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out number)
                || double.TryParse(value, NumberStyles.Float, CultureInfo.CurrentCulture, out number);
            if (!parsed || double.IsNaN(number) || double.IsInfinity(number)) throw new InvalidDataException("Height: " + response);
            return value;
        }

        /// <summary>保留现有原始文本发送语义，不改变业务命令。</summary>
        public override string Send(string data)
        {
            Flow_Chart.ThrowIfExecutionStopped();
            CancellationToken token = Flow_Chart.CurrentExecutionToken;
            var result = client.SendAsync(System.Text.Encoding.UTF8.GetBytes(data ?? ""), TimeOut, token).GetAwaiter().GetResult();
            if (result.Status == CommunicationStatus.Cancelled) result.RequireValue(token);
            return result.Message;
        }

        /// <summary>兼容手动接收，失败原因由 LastResult 保留。</summary>
        public override string Receive()
        {
            Flow_Chart.ThrowIfExecutionStopped();
            CancellationToken token = Flow_Chart.CurrentExecutionToken;
            var result = ReceiveAsync(token).GetAwaiter().GetResult();
            if (result.Status == CommunicationStatus.Cancelled) result.RequireValue(token);
            return result.Succeeded ? result.Value : "";
        }

        /// <summary>读取完整行，供流程接收能力保留明确错误状态。</summary>
        [FlowToolIgnore]
        public async Task<CommunicationResult<string>> ReceiveAsync(CancellationToken token = default(CancellationToken))
        {
            return Record(await client.ReceiveLineAsync(TimeOut, token).ConfigureAwait(false));
        }

        /// <summary>保留最近状态，未确认命令进入现有报警日志。</summary>
        private CommunicationResult<string> Record(CommunicationResult<string> result)
        {
            last = result;
            if (!result.Succeeded && result.CommandMayHaveExecuted) Flow_Module.Module_AddAlarmLog(result.Message);
            return result;
        }
    }
}
