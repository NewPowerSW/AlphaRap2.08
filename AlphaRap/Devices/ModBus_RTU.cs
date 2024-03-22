using AlphaRap;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AlaphaRap
{
    /// <summary>
    /// 具体设备类：元益测力仪 (Y1080B)
    /// 可配置的属性包括：串口连接参数
    /// V1.0 初始版本
    /// 修改时间2023-4-06----------------蔡泳
    /// </summary>
    public class ModBus_RTU : AbstractDevice
    {
        /// <summary>
        /// 实例化一个串口对象
        /// </summary>
        public SerialPort _SP = new SerialPort();

        public ModBus_RTU(string deviceName) : base(deviceName) { }
   
        /// <summary>
        /// 重写父类的属性：串口是否打开
        /// </summary>
        public override bool IsConnected
        {
            get
            {
                return _SP.IsOpen;
            }
        }

        /// <summary>
        /// 重写父类的属性：是否正在运行：
        /// </summary>
        public override bool IsRunning
        {
            get
            {
                return _isRunning;
            }
        }

        public bool _isRunning = false;


        /// <summary>
        /// 可配置的属性：串口号
        /// </summary>
        public string PortName { get; set; } = "COM1";

        /// <summary>
        /// 可配置的属性：波特率
        /// </summary>
        public int BaudRate { get; set; } = 19200;

        /// <summary>
        /// 可配置的属性：校验位
        /// </summary>
        public int Pari { get; set; } = 0;

        /// <summary>
        /// 可配置的属性：数据位
        /// </summary>
        public int DataBits { get; set; } = 8;

        /// <summary>
        /// 可配置的属性：停止位
        /// </summary>
        public int StopBit { get; set; } = 1;

        /// <summary>
        /// 可配置的属性：从站地址
        /// </summary>
        public byte Slaveld { get; set; } = 0x01;

        /// <summary>
        /// 可配置的属性：起始寄存器
        /// </summary>
        public ushort StartRegister { get; set; } = 1000;

        /// <summary>
        /// 可配置的属性：寄存器数量
        /// </summary>
        public ushort Count { get; set; } = 2;

        /// <summary>
        /// 可配置的属性：发送报文与接收报文的间隔
        /// </summary>
        public int ThreadSleep { get; set; } = 20;


        /// <summary>
        /// 读取的结果（字符串)
        /// </summary>
        public string ResultData { get; set; } = string.Empty;


        /// <summary>
        /// 主要功能：重写父类方法：加载配置，并打开网口
        /// </summary>
        public override void Open()
        {
            if (!IsConnected)
            {
                try
                {
                    
                        Connect(this.PortName, this.BaudRate, this.Pari, this.DataBits, this.StopBit);
                   
                }
                catch (Exception ex)
                {
                    ShowException("连接串口失败!", ex);
                }
            }
        }
        /// <summary>
        /// 连接串口
        /// </summary>
        /// <param name="portName">串口号</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="parity">校验位</param>
        /// <param name="dataBits">数据位</param>
        /// <param name="stopBits">停止位</param>
        /// <returns></returns>
        public void Connect(string portName, int baudRate, int parity, int dataBits, int stopBits)
        {
            Parity par = Parity.None;
            switch (parity)
            {
                case 0:
                    par = Parity.None;
                    break;
                case 1:
                    par = Parity.Odd;
                    break;
                case 2:
                    par = Parity.Even;
                    break;
                case 3:
                    par = Parity.Mark;
                    break;
                case 4:
                    par = Parity.Space;
                    break;
            }

            StopBits sb = StopBits.None;
            switch (stopBits)
            {
                case 0:
                    sb = StopBits.None;
                    break;
                case 1:
                    sb = StopBits.One;
                    break;
                case 2:
                    sb = StopBits.Two;
                    break;
                case 3:
                    sb = StopBits.OnePointFive;
                    break;
            }

			_SP.PortName = portName;
			_SP.BaudRate = baudRate;
			_SP.Parity = par;
			_SP.DataBits = dataBits;
			_SP.StopBits = sb;

			//打开串口
			_SP.Open();           
        }

        /// <summary>
        /// 主要功能：重写父类方法：关闭串口
        /// </summary>
        public override void Close()
        {
            if (_SP.IsOpen)
            {
                try
                {
					_SP.Close();
                }
                catch(Exception ex) 
                {
                    ShowException("串口关闭失败!", ex);
                }
            }
        }

        /// <summary>
        /// 03功能码 读取数据主要方法
        /// </summary>
        /// <param name="slaveld">从站地址</param>
        /// <param name="start">起始寄存器</param>
        /// <param name="count">寄存器数量</param>
        /// <param name="threadSleep">报文发送和接收间隔</param>
        /// <returns></returns>
        public byte[] ReadOutputRegisters(byte slaveld, ushort start, ushort count, int threadSleep)
        {
            this.Slaveld= slaveld;
            this.StartRegister = start;
            this.Count = count;
            this.ThreadSleep = threadSleep;
            try
            {
                //拼接报文 从站地址+功能码+起始寄存器+寄存器数量+CRC
                List<byte> SendCommand = new List<byte>();

                //站地址
                SendCommand.Add(Slaveld);

                //功能码
                SendCommand.Add(0x03);

                //起始寄存器
                SendCommand.Add((byte)(StartRegister / 256));
                SendCommand.Add((byte)(StartRegister % 256));

                //寄存器数量
                SendCommand.Add((byte)(Count / 256));
                SendCommand.Add((byte)(Count % 256));

                byte[] crc = CalculateCRC(SendCommand.ToArray(), SendCommand.Count);

                //CRC
                SendCommand.AddRange(crc);

				//发送报文
				_SP.Write(SendCommand.ToArray(), 0, SendCommand.Count);
                
                //发送和接收间隔设置
                Thread.Sleep(ThreadSleep);

                //接收报文
                //获取接收缓冲区有多少个字节
                int length = _SP.BytesToRead;
                byte[] buffer = new byte[length];
				_SP.Read(buffer, 0, buffer.Length);

                //验证报文(CRC)
                if (buffer.Length == 5 + Count * 2 && CheckCRC(buffer) && buffer[0] == Slaveld && buffer[1] == 0x03)
                {
                    //解析报文
                    byte[] result = new byte[Count * 2];
                    Array.Copy(buffer, 3, result, 0, Count * 2);
                    return result;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                ShowException("串口读取数据失败!", ex);
                return null;
            }
        }


        /// <summary>
        /// 读取结果(字符串格式)
        /// </summary>
        /// <returns></returns>
        public string GetResultData()
        {
            if (ReadOutputRegisters(this.Slaveld, this.StartRegister, this.Count, this.ThreadSleep) != null)
            {
                byte[] data = ReadOutputRegisters(this.Slaveld, this.StartRegister, this.Count, this.ThreadSleep);
                if (data != null && data.Length == Count * 2)
                {
                    this.ResultData = (data[2] * 256 + data[3]).ToString();
                    return this.ResultData;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

        }

        #region  CRC校验
        private static readonly byte[] aucCRCHi = {
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
             0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
             0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
             0x00, 0xC1, 0x81, 0x40
         };
        private static readonly byte[] aucCRCLo = {
             0x00, 0xC0, 0xC1, 0x01, 0xC3, 0x03, 0x02, 0xC2, 0xC6, 0x06, 0x07, 0xC7,
             0x05, 0xC5, 0xC4, 0x04, 0xCC, 0x0C, 0x0D, 0xCD, 0x0F, 0xCF, 0xCE, 0x0E,
             0x0A, 0xCA, 0xCB, 0x0B, 0xC9, 0x09, 0x08, 0xC8, 0xD8, 0x18, 0x19, 0xD9,
             0x1B, 0xDB, 0xDA, 0x1A, 0x1E, 0xDE, 0xDF, 0x1F, 0xDD, 0x1D, 0x1C, 0xDC,
             0x14, 0xD4, 0xD5, 0x15, 0xD7, 0x17, 0x16, 0xD6, 0xD2, 0x12, 0x13, 0xD3,
             0x11, 0xD1, 0xD0, 0x10, 0xF0, 0x30, 0x31, 0xF1, 0x33, 0xF3, 0xF2, 0x32,
             0x36, 0xF6, 0xF7, 0x37, 0xF5, 0x35, 0x34, 0xF4, 0x3C, 0xFC, 0xFD, 0x3D,
             0xFF, 0x3F, 0x3E, 0xFE, 0xFA, 0x3A, 0x3B, 0xFB, 0x39, 0xF9, 0xF8, 0x38,
             0x28, 0xE8, 0xE9, 0x29, 0xEB, 0x2B, 0x2A, 0xEA, 0xEE, 0x2E, 0x2F, 0xEF,
             0x2D, 0xED, 0xEC, 0x2C, 0xE4, 0x24, 0x25, 0xE5, 0x27, 0xE7, 0xE6, 0x26,
             0x22, 0xE2, 0xE3, 0x23, 0xE1, 0x21, 0x20, 0xE0, 0xA0, 0x60, 0x61, 0xA1,
             0x63, 0xA3, 0xA2, 0x62, 0x66, 0xA6, 0xA7, 0x67, 0xA5, 0x65, 0x64, 0xA4,
             0x6C, 0xAC, 0xAD, 0x6D, 0xAF, 0x6F, 0x6E, 0xAE, 0xAA, 0x6A, 0x6B, 0xAB,
             0x69, 0xA9, 0xA8, 0x68, 0x78, 0xB8, 0xB9, 0x79, 0xBB, 0x7B, 0x7A, 0xBA,
             0xBE, 0x7E, 0x7F, 0xBF, 0x7D, 0xBD, 0xBC, 0x7C, 0xB4, 0x74, 0x75, 0xB5,
             0x77, 0xB7, 0xB6, 0x76, 0x72, 0xB2, 0xB3, 0x73, 0xB1, 0x71, 0x70, 0xB0,
             0x50, 0x90, 0x91, 0x51, 0x93, 0x53, 0x52, 0x92, 0x96, 0x56, 0x57, 0x97,
             0x55, 0x95, 0x94, 0x54, 0x9C, 0x5C, 0x5D, 0x9D, 0x5F, 0x9F, 0x9E, 0x5E,
             0x5A, 0x9A, 0x9B, 0x5B, 0x99, 0x59, 0x58, 0x98, 0x88, 0x48, 0x49, 0x89,
             0x4B, 0x8B, 0x8A, 0x4A, 0x4E, 0x8E, 0x8F, 0x4F, 0x8D, 0x4D, 0x4C, 0x8C,
             0x44, 0x84, 0x85, 0x45, 0x87, 0x47, 0x46, 0x86, 0x82, 0x42, 0x43, 0x83,
             0x41, 0x81, 0x80, 0x40
         };


        /// <summary>
        /// CRC校验
        /// </summary>
        /// <param name="src">字节数组</param>
        /// <param name="usLen">验证长度</param>
        /// <returns>2个字节</returns>
        private byte[] CalculateCRC(byte[] src, int usLen)
        {
            int i = 0;
            byte[] res = new byte[2] { 0xFF, 0xFF };
            ushort iIndex;
            while (usLen-- > 0)
            {
                iIndex = (ushort)(res[0] ^ src[i++]);
                res[0] = (byte)(res[1] ^ aucCRCHi[iIndex]);
                res[1] = aucCRCLo[iIndex];
            }
            return res;
        }

        private bool CheckCRC(byte[] value)
        {
            if (value == null) return false;
            if (value.Length <= 2) return false;

            //字节做校验
            byte[] crc = CalculateCRC(value, value.Length - 2);

            //判断是否一致
            if (crc[0] == value[value.Length - 2] && crc[1] == value[value.Length - 1])
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion    
    }
}
