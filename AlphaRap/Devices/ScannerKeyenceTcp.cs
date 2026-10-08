using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AlphaRap
{
    /// <summary>
    /// 具体设备类：基恩士网口扫码枪，使用网口 Tcp 通讯，接收指令、扫码并返回条码
    /// 可配置的属性包括：网口连接参数
    /// </summary>
    public class ScannerKeyenceTcp : AbstractDevice
    {
        #region 1. 字段、属性

        // 字段：Tcp 客户端，用于通讯。默认 IP 地址为：192.168.100.100，端口号 9004，读取超时 1000ms
        private TcpClient _tcpClient = new TcpClient();

        // 字段：用于 _tcpClient 读写操作的网络流对象。在 _tcpClient 连接后创建，关闭后释放
        private NetworkStream _netStream;

        // 字段：用于读写网口时，锁线程
        private object _lockObj = new object();

        /// <summary>
        /// 重写父类的属性：扫码枪的网口是否连接且打开
        /// </summary>
        public override bool IsConnected
        {
            get
            {
                return _tcpClient.Connected;
            }
        }

        /// <summary>
        /// 重写父类的属性：扫码枪是否正在运行：扫码中
        /// </summary>
        public override bool IsRunning
        {
            get
            {
                return _isRunning;
            }
        }
        private bool _isRunning = false;

        #endregion

        #region 2. 构造函数

        /// <summary>
        /// 带参实例构造函数：提供扫码枪的设备名，加载并设置参数，打开网口
        /// </summary>
        public ScannerKeyenceTcp(string deviceName) : base(deviceName)
        {
        }

        #endregion

        #region 3. 可配置的属性

        /// <summary>
        /// 可配置的属性：网口的 IP 地址，默认值为 192.168.100.100
        /// </summary>
        public string IP { get; set; } = "192.168.100.100";

        /// <summary>
        /// 可配置的属性：网口的端口号，默认值为 9004
        /// </summary>
        public int Port { get; set; } = 9004;

        /// <summary>
        /// 可配置的属性：网口读取超时。读取等待超过此时间，视为未接收到数据。单位 ms，默认值为 1000
        /// </summary>
        public int TimeOut { get; set; } = 1000;

        /// <summary>
        /// 可配置的属性：网口在读取条码时的缓存区最大长度。默认值为 100
        /// </summary> 
        public int length { get; set; } = 100;

        #endregion

        #region 4. 主要功能：重写 Open / Close：打开、关闭网口

        /// <summary>
        /// 主要功能：重写父类方法：加载配置，并打开网口
        /// </summary>
        public override void Open()
        {
            //开网口
            if (!IsConnected)
            {
                OpenScanner();
            }
        }

        /// <summary>
        /// 主要功能：重写父类方法：停止扫码，并关闭网口
        /// </summary>
        public override void Close()
        {
            //停止运行：停止扫码
            if (IsRunning)
            {
                StopRunning();
            }

            //关网口
            if (IsConnected)
            {
                CloseScanner();
            }
        }

        // 私有方法：加载并设置参数，打开网口
        private void OpenScanner()
        {
            try
            {
                //网口关闭时，才能修改参数
                if (!_tcpClient.Connected)
                {
                    //使用异步方式

                        try
                        {
                            _tcpClient.Connect(IP, Port); //连接网口
                            _netStream = _tcpClient.GetStream(); //获取读写的流对象   
                        }
                        catch (Exception ex)
                        {
                            ShowException("连接扫码枪网口失败！", ex);
                        }
                }
            }
            catch (Exception ex)
            {
                ShowException("打开扫码枪网口失败！", ex);
            }
        }

        // 私有方法：关闭网口，释放相关资源
        private void CloseScanner()
        {
            try
            {
                _netStream.Close(); //关闭流对象
                _tcpClient.Close(); //关闭连接并释放                
            }
            catch (Exception ex)
            {
                ShowException("关闭扫码枪网口失败！", ex);
            }
        }

        #endregion

        #region 4. 主要功能：扫码、停止扫码

        /// <summary>
        /// 主要功能：重写父类方法：开始运行：开网口
        /// </summary>
        public override void StartRunning()
        {
            //打开网口
            if (!IsConnected)
            {
                Open();
            }
        }

        /// <summary>
        /// 主要功能：重写父类方法：停止运行：停止扫码
        /// </summary>
        public override void StopRunning()
        {
            //停止扫码
            if (IsRunning)
            {
                StopScanning();
            }
        }

        /// <summary>
        /// 主要功能：方法：扫码一次
        /// </summary>
        /// <returns>如果扫码成功，返回扫到的条码；如果扫码失败，停止扫码并返回 ERROR</returns>
        public string ScanOnce()
        {
            //如果扫码枪已连接
            if (IsConnected)
            {
                lock (_lockObj)
                {
                    _isRunning = true; //正在扫码中
                    try
                    {
                        //发送前：清空接收缓存
                        if (_netStream.DataAvailable)
                        {
                            byte[] bufRead0 = new byte[length];
                            _netStream.Read(bufRead0, 0, length);
                        }

                        //向服务器发送指令
                        byte[] bufWrite = Encoding.Default.GetBytes("LON\r\n"); //指令：扫码
                        _netStream.Write(bufWrite, 0, bufWrite.Length); //写入流
                        Thread.Sleep(50); //等待扫码

                        //接收返回的字符串
                        _netStream.ReadTimeout = TimeOut;   //读取超时，默认1秒钟
                        byte[] bufRead = new byte[length];
                        int bufLength = _netStream.Read(bufRead, 0, length); //读取流，返回有效数据长度
                        string strRes = Encoding.Default.GetString(bufRead, 0, bufLength); //截取有效数据，并编码为字符串
                        strRes = strRes.Trim().Replace("\r", "").Replace("\n", ""); //去除空格和换行符

                        //判断返回字符串是否合法
                        if (!(string.IsNullOrEmpty(strRes) || strRes == "ERROR"))
                        {
                            _isRunning = false; //返回字符串合法，扫码结束
                            return strRes;
                        }
                    }
                    catch (Exception ex)
					{
						ShowException("扫码枪 [" + DeviceName + "] 扫码失败！\r\n异常信息:\r\n", ex);
                    }

                    StopScanning(); //发生错误，停止扫码
                }// lock
            }// if (IsConnected)

            return "ERROR"; //扫码枪未连接，或发生错误，返回 ERROR
        }

        // 私有方法：停止扫码
        private void StopScanning()
        {
            //如果扫码枪已连接
            if (IsConnected)
            {
                byte[] bufferWrite = Encoding.Default.GetBytes("LOFF\r\n"); //指令：停止扫码 
                _netStream.Write(bufferWrite, 0, bufferWrite.Length);   //写入流
            }

            _isRunning = false; //停止扫码                            
        }

        #endregion
    }// class
}// namespace
