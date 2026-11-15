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
    /// <summary>Modbus RTU 03 读取设备，完整事务共用同一个端口占用。</summary>
    public partial class ModBus_RTU : AlphaRap.AbstractDevice
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
        public CommunicationResult<byte[]> LastResult { get; private set; }
        public override bool IsConnected { get { return _SP.IsOpen; } }
        public override bool IsRunning { get { return _isRunning || operations.IsBusy; } }
        public override bool SupportsRawIo { get { return true; } }

        /// <summary>创建设备身份，不打开实际端口。</summary>
        public ModBus_RTU(string deviceName) : base(deviceName) { }

        /// <summary>按已配置参数显式打开端口。</summary>
        public override void Open() { Connect(PortName, BaudRate, Pari, DataBits, StopBit); }

        /// <summary>校验后配置并打开现有公开端口，禁止后台静默自动重连。</summary>
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

        /// <summary>关闭及旧驱动实际返回之前保留占用。</summary>
        public override void Close() { SerialPort port = _SP; operations.StopAsync(port.Close).GetAwaiter().GetResult(); }

        /// <summary>停止当前事务并关闭端口。</summary>
        public override void StopRunning() { Close(); }

        /// <summary>按原签名读取；失败保留 null，取消向流程抛出。</summary>
        [FlowToolIgnore]
        public byte[] ReadOutputRegisters(byte slaveld, ushort start, ushort count, int threadSleep)
        {
            CancellationToken token = Flow_Chart.CurrentExecutionToken;
            var result = ReadAsync(slaveld, start, count, threadSleep, token).GetAwaiter().GetResult();
            if (result.Status == CommunicationStatus.Cancelled) result.RequireValue(token);
            return result.Value;
        }

        /// <summary>一次发送和一份完整带校验应答，时限包含协议等待，不接受半帧。</summary>
        [FlowToolIgnore]
        public async Task<CommunicationResult<byte[]>> ReadAsync(byte slave, ushort start, ushort count, int delayMs, CancellationToken token = default(CancellationToken))
        {
            if (!ModbusRtuFrame.Valid(slave, start, count) || delayMs < 0 || delayMs > 60000)
                return LastResult = new CommunicationResult<byte[]>(CommunicationStatus.InvalidConfiguration);
            if (!IsConnected) return LastResult = new CommunicationResult<byte[]>(CommunicationStatus.NotConnected);
            Slaveld = slave; StartRegister = start; Count = count; ThreadSleep = delayMs;
            SerialPort port = _SP;
            LastResult = await operations.RunAsync(cancellation =>
            {
                port.DiscardInBuffer(); SerialFrames.Write(port, ModbusRtuFrame.Request(slave, start, count), cancellation);
                if (cancellation.WaitHandle.WaitOne(delayMs)) cancellation.ThrowIfCancellationRequested();
                return ModbusRtuFrame.Read(port, slave, count, cancellation);
            }, port.Close, TimeOut, token, true).ConfigureAwait(false);
            if (!LastResult.Succeeded) ResultData = "";
            if (LastResult.CommandMayHaveExecuted) Flow_Module.Module_AddAlarmLog(LastResult.Message);
            return LastResult;
        }

        /// <summary>只读取一次；保留旧双寄存器时取第二个寄存器，单寄存器读取也不会越界。</summary>
        [FlowToolIgnore]
        public string GetResultData()
        {
            byte[] data = ReadOutputRegisters(Slaveld, StartRegister, Count, ThreadSleep);
            if (data == null || data.Length < 2) return ResultData = "";
            int offset = data.Length >= 4 ? 2 : 0;
            return ResultData = (data[offset] * 256 + data[offset + 1]).ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>保留原始文本发送，读写也经过同一个请求占用。</summary>
        public override string Send(string data)
        {
            SerialPort port = _SP;
            var result = operations.RunAsync(token => { SerialFrames.Write(port, port.Encoding.GetBytes(data ?? ""), token); return true; }, port.Close, TimeOut,
                Flow_Chart.CurrentExecutionToken, true).GetAwaiter().GetResult();
            if (result.Status == CommunicationStatus.Cancelled) result.RequireValue(Flow_Chart.CurrentExecutionToken);
            return result.Message;
        }

        /// <summary>手动读取当前实际数据，不把空容量填成应答。</summary>
        public override string Receive()
        {
            SerialPort port = _SP;
            var result = operations.RunAsync(token => port.Encoding.GetString(SerialFrames.ReadAvailable(port, token)).Trim(), port.Close, TimeOut,
                Flow_Chart.CurrentExecutionToken).GetAwaiter().GetResult();
            if (result.Status == CommunicationStatus.Cancelled) result.RequireValue(Flow_Chart.CurrentExecutionToken);
            return result.Succeeded ? result.Value : "";
        }
    }
}
