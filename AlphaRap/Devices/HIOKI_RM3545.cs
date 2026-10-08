using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;

namespace AlphaRap
{
    /// <summary>
    /// 具体设备类:波日图电阻计
    /// 可配置属性包括:串口连接参数
    /// V1.0初始版本
    /// 修改时间2023-10-26 ---------CY
    /// </summary>
    class HIOKI_RM3545 : AbstractDevice
    {
        #region 1.字段、属性
        //实例化一个串口对象
        public SerialPort MyCom = new SerialPort();

        /// <summary>
        /// 重写父类属性:串口是否打开
        /// </summary>
        public override bool IsConnected
        {
            get { return MyCom.IsOpen; }
        }

        #endregion

        #region 构造函数
        public HIOKI_RM3545(string deviceName) : base(deviceName)
        {
        }
        #endregion

        #region 可配置的属性

        /// <summary>
        /// 串口号
        /// </summary>
        public string PortName { get; set; } = "COM1";

        /// <summary>
        /// 波特率
        /// </summary>
        public int BaudRate { get; set; } = 9600;

        /// <summary>
        /// 校验位
        /// </summary>
        public Parity Pari { get; set; } = Parity.None;

        /// <summary>
        /// 数据位
        /// </summary>
        public int DataBit { get; set; } = 0;

        /// <summary>
        /// 停止位
        /// </summary>
        public StopBits StopBit { get; set; } = StopBits.One;

        /// <summary>
        /// 串口返回的结果字符串
        /// </summary>
        public string strResult { get; set; } = string.Empty;

        /// <summary>
        /// 自定义的结果集合
        /// </summary>
        public List<double> listResult = new List<double>();

        /// <summary>
        /// 设定的通道数
        /// </summary>
        public int ChannelNum { get; set; } = 7;

        /// <summary>
        /// 设置指令1
        /// </summary>
        public const string InitSet1 = ":INITiate:CONTinuous OFF\r\n";

        /// <summary>
        /// 设置指令2
        /// </summary>
        public const string InitSet2 = ":TRIGger:SOURce IMMediate\r\n";

        /// <summary>
        /// 读取指令
        /// </summary>
        public const string ReadCommand = ":READ?\r\n";

        #endregion

        #region 主要功能

        /// <summary>
        /// 连接串口
        /// </summary>
        public override void Open()
        {
            if (IsConnected)
            {
                MyCom.Close();
            }
            Thread.Sleep(300);
            try
            {
                    MyCom.PortName = PortName;
                    MyCom.BaudRate = BaudRate;
                    MyCom.Parity = Pari;
                    MyCom.DataBits = DataBit;
                    MyCom.StopBits = StopBit;

                    MyCom.ReceivedBytesThreshold = 1;
                    MyCom.DataReceived += new SerialDataReceivedEventHandler(MyCom_DataReceived);

                    MyCom.Open();
            }
            catch (Exception ex)
            {
                ShowException("电阻计连接串口失败!", ex);
            }
        }

        public void MyCom_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (!IsConnected) return;
            try
            {
                strResult = MyCom.ReadLine();
            }
            catch
            {
                strResult = string.Empty;
            }
        }

        /// <summary>
        /// 结果字符串转换为集合
        /// </summary>
        /// <param name="strResult">串口返回的结果字符串</param>
        /// <param name="bLengthOK">集合长度是否等于设定的通道数</param>
        /// <returns>返回的结果集合</returns>
        public List<double> GetFinalResult(ref bool bLengthOK)
        {
            listResult.Clear();
            string[] results = strResult.Split(',');
            if (results.Length == ChannelNum)
            {
                bLengthOK = true;

                for (int i = 0; i < results.Length; i++)
                {
                    listResult.Add(Convert.ToDouble(results[i]));
                }
            }
            else { bLengthOK = false; }

            return listResult;
        }

        /// <summary>
        /// 发送指令的方法
        /// </summary>
        public bool Write(string Command)
        {
            try
            {
                MyCom.Write(Command);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        #endregion
    }
}
