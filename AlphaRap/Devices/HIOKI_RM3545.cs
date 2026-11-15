using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO.Ports;
using System.Text;
using AlphaRap.FlowProgramming;
using NPSDK;

namespace AlphaRap
{
    /// <summary>电阻计单一接收事件累积完整行，重新读取前清除旧结果。</summary>
    public partial class HIOKI_RM3545 : AbstractDevice
    {
        public SerialPort MyCom = new SerialPort();
        private readonly SerialOperation operations = new SerialOperation();
        private readonly object resultSync = new object();
        private readonly StringBuilder partial = new StringBuilder();
        private bool fresh;
        private long receiveEpoch;
        private SerialPort resultPort;
        public string PortName { get; set; } = "COM1";
        public int BaudRate { get; set; } = 9600;
        public Parity Pari { get; set; } = Parity.None;
        public int DataBit { get; set; } = 8;
        public StopBits StopBit { get; set; } = StopBits.One;
        [Browsable(false)]
        public string strResult { get; set; } = "";
        public List<double> listResult = new List<double>();
        public int ChannelNum { get; set; } = 7;
        public int TimeOut { get; set; } = 1500;
        public const string InitSet1 = ":INITiate:CONTinuous OFF\r\n";
        public const string InitSet2 = ":TRIGger:SOURce IMMediate\r\n";
        public const string ReadCommand = ":READ?\r\n";
        public override bool IsConnected { get { return MyCom.IsOpen; } }
        public override bool IsRunning { get { return operations.IsBusy; } }
        public override bool SupportsRawIo { get { return true; } }

        /// <summary>创建设备身份，不连接硬件。</summary>
        public HIOKI_RM3545(string deviceName) : base(deviceName) { }

        /// <summary>同一端口只注册一份接收事件，显式校验参数，不固定等待三百毫秒。</summary>
        public override void Open()
        {
            Flow_Chart.ThrowIfExecutionStopped();
            string name = PortName; int baud = BaudRate, bits = DataBit, parity = (int)Pari, stop = (int)StopBit;
            if (!SerialFrames.Valid(name, baud, bits, parity, stop)) throw new ArgumentException(DeviceText.T("communication.InvalidConfiguration"));
            SerialPort port = MyCom;
            operations.RunAsync(token =>
            {
                if (port.IsOpen) return true;
                ResetResult(); SerialFrames.Configure(port, name, baud, bits, parity, stop);
                port.ReceivedBytesThreshold = 1; port.DataReceived -= MyCom_DataReceived; port.DataReceived += MyCom_DataReceived;
                port.Open(); return port.IsOpen;
            }, port.Close, TimeOut, Flow_Chart.CurrentExecutionToken).GetAwaiter().GetResult().RequireValue(Flow_Chart.CurrentExecutionToken);
        }

        /// <summary>解除事件并关闭后清除旧缓存。</summary>
        public override void Close()
        {
            SerialPort port = MyCom; port.DataReceived -= MyCom_DataReceived;
            operations.StopAsync(port.Close).GetAwaiter().GetResult(); ResetResult();
        }

        /// <summary>停止当前原生 IO 并使上次结果失效。</summary>
        public override void StopRunning() { Close(); }

        /// <summary>事件只读当前可用数据，不在事件线程等待换行。</summary>
        public void MyCom_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var port = sender as SerialPort;
            if (port == null || !ReferenceEquals(port, MyCom) || !port.IsOpen) return;
            long epoch; lock (resultSync) epoch = receiveEpoch;
            try { CaptureText(port, port.ReadExisting(), epoch); }
            catch (Exception error) when (error is System.IO.IOException || error is InvalidOperationException)
            {
                lock (resultSync) if (epoch == receiveEpoch && ReferenceEquals(port, MyCom)) ResetResult();
            }
        }

        /// <summary>按当前端口的完整行更新结果，旧端口回调和半行不会发布。</summary>
        private void CaptureText(SerialPort port, string text, long epoch)
        {
            lock (resultSync)
            {
                if (!ReferenceEquals(port, MyCom) || epoch != receiveEpoch) return;
                resultPort = port;
                foreach (char value in text) if (!CaptureCharacter(value)) return;
            }
        }

        /// <summary>处理一个行协议字符，超长数据作废。</summary>
        private bool CaptureCharacter(char value)
        {
            if (value == '\r' || value == '\n')
            {
                if (partial.Length == 0) return true;
                strResult = partial.ToString(); partial.Clear(); fresh = true; return true;
            }
            if (partial.Length >= 65536) { partial.Clear(); fresh = false; return false; }
            partial.Append(value); return true;
        }

        /// <summary>全部通道都是有限数值后才一次发布，不返回部分解析结果。</summary>
        [FlowToolIgnore]
        public List<double> GetFinalResult(ref bool bLengthOK)
        {
            lock (resultSync)
            {
                bLengthOK = false; listResult.Clear();
                if (!fresh || !ReferenceEquals(resultPort, MyCom) || ChannelNum < 1 || ChannelNum > 1024) return new List<double>();
                string[] values = strResult.Split(',');
                if (values.Length != ChannelNum) return new List<double>();
                var parsed = new List<double>();
                foreach (string text in values)
                {
                    double number;
                    if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out number) || double.IsNaN(number) || double.IsInfinity(number)) return new List<double>();
                    parsed.Add(number);
                }
                listResult.AddRange(parsed); bLengthOK = true; return new List<double>(parsed);
            }
        }

        /// <summary>读命令发送前清除旧值，写失败不会保留上一件产品结果。</summary>
        [FlowToolIgnore]
        public bool Write(string Command)
        {
            Flow_Chart.ThrowIfExecutionStopped(); SerialPort port = MyCom;
            if (Command == null) return false;
            if (Command.IndexOf("READ?", StringComparison.OrdinalIgnoreCase) >= 0) ResetResult();
            var result = operations.RunAsync(token => { SerialFrames.Write(port, port.Encoding.GetBytes(Command), token); return true; },
                port.Close, TimeOut, Flow_Chart.CurrentExecutionToken, true).GetAwaiter().GetResult();
            if (result.CommandMayHaveExecuted) Flow_Module.Module_AddAlarmLog(result.Message);
            if (!result.Succeeded) ResetResult();
            if (result.Status == CommunicationStatus.Cancelled) result.RequireValue(Flow_Chart.CurrentExecutionToken);
            return result.Succeeded;
        }

        /// <summary>发送沿用统一写入路径。</summary>
        public override string Send(string data) { return Write(data ?? "") ? "" : DeviceText.T("communication.TransportError"); }

        /// <summary>读取唯一接收事件已收齐的结果，不建立第二个串口读取者。</summary>
        public override string Receive() { lock (resultSync) return fresh ? strResult : ""; }

        /// <summary>清空半帧与旧业务结果，不修改用户配置。</summary>
        private void ResetResult() { lock (resultSync) { receiveEpoch++; partial.Clear(); strResult = ""; listResult.Clear(); fresh = false; resultPort = null; } }
    }
}
