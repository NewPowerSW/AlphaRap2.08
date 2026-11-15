using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using AlphaRap;
using AlphaRap.FlowProgramming;
using NPSDK;

namespace AlaphaRap
{
    /// <summary>气密仪串口行协议接收，完整行或明确失败分别返回。</summary>
    public partial class ATEQ_F620 : AlphaRap.AbstractDevice
    {
        public SerialPort _SP = new SerialPort();
        private readonly SerialOperation operations = new SerialOperation();
        public bool _isRunning;
        public string PortName { get; set; } = "COM3";
        public int BaudRate { get; set; } = 9600;
        public int Pari { get; set; } = 2;
        public int DataBits { get; set; } = 8;
        public int StopBit { get; set; } = 1;
        public byte Slaveld { get; set; } = 1;
        public ushort StartRegister { get; set; } = 1000;
        public ushort Count { get; set; } = 2;
        public int ThreadSleep { get; set; } = 20;
        public string ResultData { get; set; } = "";
        public int TimeOut { get; set; } = 1500;
        [Browsable(false)]
        public CommunicationResult<string> LastResult { get; private set; }
        public override bool IsConnected { get { return _SP.IsOpen; } }
        public override bool IsRunning { get { return _isRunning || operations.IsBusy; } }
        public override bool SupportsRawIo { get { return true; } }

        /// <summary>构建设备身份，不打开端口。</summary>
        public ATEQ_F620(string deviceName) : base(deviceName) { }

        /// <summary>使用已有设备配置显式连接。</summary>
        public override void Open() { Connect(PortName, BaudRate, Pari, DataBits, StopBit); }

        /// <summary>校验配置后打开，重复打开已连接对象不新增会话。</summary>
        [FlowToolIgnore]
        public void Connect(string portName, int baudRate, int parity, int dataBits, int stopBits)
        {
            Flow_Chart.ThrowIfExecutionStopped();
            if (!SerialFrames.Valid(portName, baudRate, dataBits, parity, stopBits)) throw new ArgumentException(DeviceText.T("communication.InvalidConfiguration"));
            SerialPort port = _SP;
            operations.RunAsync(token =>
            {
                if (port.IsOpen) return true;
                SerialFrames.Configure(port, portName, baudRate, dataBits, parity, stopBits); port.Open(); return port.IsOpen;
            }, port.Close, TimeOut, Flow_Chart.CurrentExecutionToken).GetAwaiter().GetResult().RequireValue(Flow_Chart.CurrentExecutionToken);
        }

        /// <summary>实际关闭和当前 IO 退出后返回。</summary>
        public override void Close() { SerialPort port = _SP; operations.StopAsync(port.Close).GetAwaiter().GetResult(); }

        /// <summary>停止当前读取，不自动重新打开端口。</summary>
        public override void StopRunning() { Close(); }

        /// <summary>保留旧字符串接口；取消抛出，其他失败返回空串。</summary>
        [FlowToolIgnore]
        public string Readdata()
        {
            CancellationToken token = Flow_Chart.CurrentExecutionToken;
            var result = ReadResultAsync(token).GetAwaiter().GetResult();
            if (result.Status == CommunicationStatus.Cancelled) result.RequireValue(token);
            return result.Succeeded ? result.Value : "";
        }

        /// <summary>等到一条完整结果，不把缓存中的半行或多行拼接当作单次结果。</summary>
        [FlowToolIgnore]
        public async Task<CommunicationResult<string>> ReadResultAsync(CancellationToken token = default(CancellationToken))
        {
            if (!IsConnected) return LastResult = new CommunicationResult<string>(CommunicationStatus.NotConnected);
            SerialPort port = _SP;
            LastResult = await operations.RunAsync(cancellation => SerialFrames.ReadLine(port, cancellation), port.Close, TimeOut, token).ConfigureAwait(false);
            ResultData = LastResult.Succeeded ? LastResult.Value : "";
            return LastResult;
        }

        /// <summary>原始发送也经过同一端口占用和停止令牌。</summary>
        public override string Send(string data)
        {
            SerialPort port = _SP; CancellationToken token = Flow_Chart.CurrentExecutionToken;
            var result = operations.RunAsync(cancellation => { SerialFrames.Write(port, port.Encoding.GetBytes(data ?? ""), cancellation); return true; },
                port.Close, TimeOut, token, true).GetAwaiter().GetResult();
            if (result.CommandMayHaveExecuted) Flow_Module.Module_AddAlarmLog(result.Message);
            if (result.Status == CommunicationStatus.Cancelled) result.RequireValue(token);
            return result.Message;
        }

        /// <summary>手动读取同一完整行，不与流程读取并发消费数据。</summary>
        public override string Receive() { return Readdata(); }
    }
}
