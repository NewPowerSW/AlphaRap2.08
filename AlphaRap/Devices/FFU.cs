using AlphaRap;
using AlphaRap.FlowProgramming;
using System;
using System.Diagnostics;
using System.IO.Ports;

namespace NP_PressureSensor
{
    public class FFU : AbstractDevice
    {
        private readonly object connectionSync = new object();
        private SerialPort sp;
        public int Timeout = 100;

        public double FFUSpeedRate = 250.0 / 1350.0;
        /// <summary>创建可由旧代码直接使用的风机实例。</summary>
        public FFU() : this(nameof(FFU)) { }

        /// <summary>创建由设备管理器命名的风机实例。</summary>
        public FFU(string deviceName) : base(deviceName) { }

        public string PortName { get; set; } = string.Empty;
        public int BaudRate { get; set; } = 9600;
        public Parity PortParity { get; set; } = Parity.None;
        public int DataBits { get; set; } = 8;
        public StopBits PortStopBits { get; set; } = StopBits.One;

        public override bool IsConnected { get { lock (connectionSync) return sp != null && sp.IsOpen; } }

        /// <summary>使用设备面板配置打开串口。</summary>
        public override void Open()
        {
            if (!IsConnected && !string.IsNullOrWhiteSpace(PortName)) OpenPort(PortName, BaudRate, PortParity, DataBits, PortStopBits);
        }

        /// <summary>关闭串口并释放端口句柄。</summary>
        public override void Close() { ClosePort(); }

        /// <summary>
        /// 打开串口
        /// </summary>
        /// <param name="PortName">COM号</param>
        /// <param name="BaudRate">波特率</param>
        /// <param name="paity">奇偶位</param>
        /// <param name="DataBits">数据位</param>
        /// <param name="stopBits">停止位</param>
        [FlowToolIgnore]
        public bool OpenPort(string PortName, int BaudRate, Parity paity, int DataBits, StopBits stopBits)
        {
            lock (connectionSync)
            {
                if (IsConnected && sp.PortName == PortName && sp.BaudRate == BaudRate &&
                    sp.Parity == paity && sp.DataBits == DataBits && sp.StopBits == stopBits) return true;
                if (!ClosePort()) return false;

                try
                {
                    sp = new SerialPort(PortName, BaudRate, paity, DataBits, stopBits) { ReadTimeout = 10 };
                    sp.Open();
                    return true;
                }
                catch (Exception ex)
                {
                    ClosePort();
                    Debug.WriteLine(ex.Message);
                    return false;
                }
            }
        }

        /// <summary>释放当前串口，允许同一设备实例再次打开。</summary>
        [FlowToolIgnore]
        public bool ClosePort()
        {
            lock (connectionSync)
            {
                SerialPort port = sp;
                sp = null;
                if (port == null) return true;

                try { port.Dispose(); return true; }
                catch (Exception ex)
                {
                    if (port.IsOpen) sp = port;
                    Debug.WriteLine(ex.Message);
                    return false;
                }
            }
        }

        /// <summary>读取风机返回的十六进制字节文本。</summary>
        public string DataReceiveFunction()
        {
            lock (connectionSync)
            {
                if (!IsConnected) return null;

                try
                {
                    byte[] buffer = new byte[1024];
                    sp.ReadTimeout = Timeout;
                    int count = sp.Read(buffer, 0, buffer.Length);
                    if (count == 0) return null;
                    return BitConverter.ToString(buffer, 0, count).Replace('-', ' ') + " ";
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        /// <summary>将风机响应中的原始设定值转换为风速。</summary>
        public double ConvertSpeed(string dataSpeed)
        {
            double speed = 0;
            string[] separator = new string[] { " " };
            if (!string.IsNullOrEmpty(dataSpeed))
            {
                string[] command = dataSpeed.Split(separator, StringSplitOptions.RemoveEmptyEntries);

                if (command.Length >= 4) { speed = To16Convert10(command[3]) / FFUSpeedRate; }
            }
            return speed;
        }

        /// <summary>将十六进制值转换为整数，无效值返回零。</summary>
        private static int To16Convert10(string str)
        {
            int res = 0;

            try
            {
                str = str.Trim().Replace(" ", "");
                res = int.Parse(str, System.Globalization.NumberStyles.AllowHexSpecifier);
            }
            catch (Exception)
            {
                res = 0;
            }

            return res;
        }

        /// <summary>将连续的十六进制字符转换为字节数组。</summary>
        public static byte[] HexStringToByteArray(string s)
        {
            s = s.Replace(" ", "");
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
                buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            return buffer;
        }

        /// <summary>
        /// 数据发送
        /// </summary>
        /// <param name="data">发送的数据</param>
        public bool SendSerialPortData(string data)
        {
            lock (connectionSync)
            {
                if (!IsConnected) return false;

                try
                {
                    byte[] bytes = HexStringToBytes(data);
                    sp.Write(bytes, 0, bytes.Length);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        /// <summary>解析以空格分隔的十六进制报文。</summary>
        private static byte[] HexStringToBytes(string hs)
        {
            string[] strArr = hs.Trim().Split(' ');
            byte[] b = new byte[strArr.Length];
            //逐个字符变为16进制字节数据
            for (int i = 0; i < strArr.Length; i++)
            {
                b[i] = Convert.ToByte(strArr[i], 16);
            }
            //按照指定编码将字节数组变为字符串
            return b;
        }
    }
}
