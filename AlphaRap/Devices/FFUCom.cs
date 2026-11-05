using System;
using System.Globalization;
using System.Threading;
using AlphaRap.FlowProgramming;

namespace AlphaRap.Classes
{
    /// <summary>通过串口读取和设置双通道 FFU 的风速。</summary>
    public class FFUCom : AbstractDevice
    {
        private readonly NPSerialPort serialPort = new NPSerialPort();
        private readonly object serialSync = new object();
        private readonly bool useLegacySettings;
        private const double SpeedRate = 250.0 / 1350.0;

        public static string com = "";
        public static string para = "";
        public bool connectStatus = false;

        /// <summary>为旧调用保留无参构造和静态串口配置。</summary>
        public FFUCom() : this(nameof(FFUCom)) { useLegacySettings = true; }

        /// <summary>创建设备管理器使用的独立风机实例。</summary>
        public FFUCom(string deviceName) : base(deviceName) { }

        public string PortName { get; set; } = string.Empty;
        public string SerialParameters { get; set; } = string.Empty;

        public override bool IsConnected { get { lock (serialSync) return serialPort.ConnectStates(); } }
        public bool ConnectStatus { get { return IsConnected; } }

        /// <summary>使用本实例的串口号和通信参数建立连接。</summary>
        public override void Open()
        {
            lock (serialSync)
            {
                if (IsConnected) return;
                string portName = useLegacySettings && string.IsNullOrEmpty(PortName) ? com : PortName;
                string parameters = useLegacySettings && string.IsNullOrEmpty(SerialParameters) ? para : SerialParameters;
                if (string.IsNullOrWhiteSpace(portName) || string.IsNullOrWhiteSpace(parameters)) return;
                serialPort.ConnectComPressL(portName, parameters);
            }
        }

        /// <summary>释放已打开的串口。</summary>
        public override void Close()
        {
            lock (serialSync)
                if (IsConnected) serialPort.Dispose();
        }

        /// <summary>按旧接口接收串口号和通信参数，并保存到当前实例。</summary>
        [FlowToolIgnore]
        public bool Connect(string comStr, string paraStr)
        {
            lock (serialSync)
            {
                com = PortName = comStr;
                para = SerialParameters = paraStr;
                Close();
                Open();
                return IsConnected;
            }
        }

        /// <summary>保留旧调用的断开连接入口。</summary>
        [FlowToolIgnore]
        public void DisConnect() { Close(); }

        /// <summary>把两个风机通道设置为最大风速。</summary>
        public bool OpenFFUMaxSpeed()
        {
            return Exchange("35 41 01 FA 70", "35 42 01 FA 73").Length > 8;
        }

        /// <summary>读取第一通道的实际风速。</summary>
        public double GetFFUSpeed() { return ReadSpeed("15 21 01 CA"); }

        /// <summary>读取第二通道的实际风速。</summary>
        public double GetFFUSpeed2() { return ReadSpeed("15 22 01 C9"); }

        /// <summary>给两个通道发送相同的风速设定。</summary>
        public bool SetFFUSpeed(int speed)
        {
            string speedByte = ((int)(speed * SpeedRate)).ToString("X2");
            string first = "35 41 01 " + speedByte;
            string second = "35 42 01 " + speedByte;
            return Exchange(first + " " + GetCheckCode(first), second + " " + GetCheckCode(second)).Length > 8;
        }

        /// <summary>解析风速响应中的第四个字节。</summary>
        private double ReadSpeed(string command)
        {
            string[] fields = Exchange(command).Split(' ');
            int raw;
            if (fields.Length < 4 || !int.TryParse(fields[3], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out raw)) return 0.0;
            return raw / SpeedRate;
        }

        /// <summary>串行完成一组命令的发送和回读，避免不同调用混用响应。</summary>
        private string Exchange(params string[] commands)
        {
            lock (serialSync)
            {
                Open();
                if (!IsConnected) return string.Empty;
                foreach (string command in commands)
                    if (!serialPort.Write_StringToHex(command)) return string.Empty;
                Thread.Sleep(100);
                return serialPort.readStingByHex() ?? string.Empty;
            }
        }

        /// <summary>按风机协议计算命令字节的异或校验。</summary>
        private string GetCheckCode(string command)
        {
            byte check = 0xFF;
            foreach (byte value in serialPort.strToHexByte(command)) check ^= value;
            return check.ToString("X2");
        }
    }
}
