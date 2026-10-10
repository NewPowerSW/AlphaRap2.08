using NPClient;
using System;
using System.Threading;

namespace AlphaRap
{
    /// <summary>
    /// 具体设备类：基恩士网口3D，使用网口 Tcp 通讯，接收指令、扫码并返回高度
    /// 可配置的属性包括：网口连接参数
    /// </summary>
    public class Keyence3DTcp : AbstractDevice
    {
        #region 1. 字段、属性

        // 字段：Tcp 客户端，用于通讯。默认 IP 地址为：192.168.10.10，端口号 8500，读取超时 1000ms
        private TCPCLient Keyence3D = new TCPCLient();

        /// <summary>
        /// 重写父类的属性：扫码枪的网口是否连接且打开
        /// </summary>
        public override bool IsConnected
        {
            get
            {
                try
                {
                    // ⚠ 底层 NPClient.TCPCLient 的连接是**异步发起**的（BeginConnect/ConnectCallback），
                    //    Connect() 立即返回、ConnectStatus() 会乐观地报 true ⇒ 设备不存在也显示"已连接"。
                    //    所以这里必须要求本类自己独立探测成功过（_verified）才算连上。
                    return _opened && _verified && Keyence3D.ConnectStatus();
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        /// <summary>本类是否真的调用过库的 Connect（且没抛异常）。</summary>
        private bool _opened;

        /// <summary>本类是否用标准 TcpClient 独立探测确认过 IP:Port 真能握手。</summary>
        private bool _verified;

        /// <summary>
        /// 用标准 TcpClient 独立探测一次 IP:Port（超时 timeoutMs 毫秒）。
        /// 库自己的 Connect 是异步的、拿不到结果，只有这里才是"真的连得上"的证据。
        /// </summary>
        private static bool ProbeTcp(string ip, int port, int timeoutMs)
        {
            try
            {
                using (System.Net.Sockets.TcpClient probe = new System.Net.Sockets.TcpClient())
                {
                    IAsyncResult ar = probe.BeginConnect(ip, port, null, null);
                    if (!ar.AsyncWaitHandle.WaitOne(timeoutMs, false)) return false;   // 超时 ⇒ 不可达
                    probe.EndConnect(ar);                                              // 异常 ⇒ 被拒绝
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
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
        /// 带参实例构造函数：提供3D的设备名，加载并设置参数，打开网口
        /// </summary>
        public Keyence3DTcp(string deviceName) : base(deviceName)
        {
        }

        #endregion

        #region 3. 可配置的属性

        /// <summary>
        /// 可配置的属性：网口的 IP 地址，默认值为 192.168.10.10
        /// </summary>
        public string IP { get; set; } = "192.168.10.10";

        /// <summary>
        /// 可配置的属性：网口的端口号，默认值为 8500
        /// </summary>
        public int Port { get; set; } = 8500;

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
                OpenKeyence3D();
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
                CloseKeyence3D();
            }
        }

        // 私有方法：加载并设置参数，打开网口
        private void OpenKeyence3D()
        {
            try
            {
                //网口关闭时，才能修改参数
                if (!Keyence3D.ConnectStatus())
                {
                    // 先用标准 TcpClient 独立探测：库的 Connect 是异步的、不等结果就返回，
                    // 不探测的话"设备根本不在"也会被报成已连接。
                    _verified = ProbeTcp(IP, Port, 600);
                    if (!_verified)
                    {
                        _opened = false;
                        return;                     // 探测不通，就不去调库了
                    }

                    try
                    {
                        Keyence3D.Connect(IP, Port); //连接网口
                        _opened = true;
                    }
                    catch (Exception ex)
                    {
                        _opened = false;
                        _verified = false;
                        ShowException("连接3D网口失败！", ex);
                    }
                }
                else
                {
                    _opened = true;                 // 库说已连（说明之前真的连过）
                    _verified = true;
                }
            }
            catch (Exception ex)
            {
                ShowException("打开3D网口失败！", ex);
            }
        }

        // 私有方法：关闭网口，释放相关资源
        private void CloseKeyence3D()
        {
            try
            {
                Keyence3D.Disconnect(); //关闭连接并释放
                _opened = false;
                _verified = false;
            }
            catch (Exception ex)
            {
                ShowException("关闭3D网口失败！", ex);
            }
        }

        #endregion

        #region 4. 主要功能：切换位置、读取当前位置的高度

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
        /// 主要功能：3D运行成功返回扫到的高度失败则返回Error
        /// </summary>
        /// <param name="Position">位置</param>
        public string Triger3D(int Position)
        {
            string Heightstring = "";

            try
            {
                if (Keyence3D.ConnectStatus() == false)
                {
                    for (int iX = 0; iX < 10; iX++)
                    {
                        OpenKeyence3D();
                    }
                }               
                if (Keyence3D.ConnectStatus())
                {
                    if (Position == 1)
                    {
                        Keyence3D.Sent("EXW,0\r\n");
                    }
                    if (Position == 2)
                    {
                        Keyence3D.Sent("EXW,1\r\n");
                    }
                    if (Position == 3)
                    {
                        Keyence3D.Sent("EXW,2\r\n");
                    }
                    if (Position == 4)
                    {
                        Keyence3D.Sent("EXW,3\r\n");
                    }
                    if (Position == 5)
                    {
                        Keyence3D.Sent("EXW,4\r\n");
                    }
                    if (Position == 6)
                    {
                        Keyence3D.Sent("EXW,5\r\n");
                    }
                    Thread.Sleep(300);
                    Heightstring = Keyence3D.Receive();
                    Keyence3D.Sent("T1\r\n");
                    Thread.Sleep(300);
                    Heightstring = Keyence3D.Receive();

                    for (int iX = 0; iX < 10; iX++)
                    {
                        if ((Heightstring == "") || (Heightstring == "Null") || (Heightstring == "Err,Fail") || (Heightstring == "Err"))
                        {
                            Keyence3D.Sent("T1\r\n");
                            Thread.Sleep(500);
                            Heightstring = Keyence3D.Receive();
                        }
                        else
                        {
                            break;
                        }
                    }

                    Heightstring = Heightstring.Replace("\r", "");
                    Heightstring = Heightstring.Replace("\n", "");
                    Heightstring = Heightstring.Replace("T1", "");
                    if (Heightstring == "")
                    {
                        Heightstring = "ERROR";
                    }
                    double ccc = Convert.ToDouble(Heightstring);
                }
            }
            catch (Exception ex)
            {
                ShowException("读取高度失败！", ex);
                OpenKeyence3D();
                Heightstring = "ERROR";
            }

            return Heightstring;
        }
        /// <summary>
        /// 主要功能：切换位置，在运行Triger3D先要切换到相应的位置
        /// </summary>
        /// <param name="Position">第几个位置</param>
        public string Change3D(int Position)
        {
            string Heightstring = "";

            if (Keyence3D.ConnectStatus() == false)
            {
                if (!Keyence3D.ConnectStatus())
                {
                    OpenKeyence3D();
                }
            }
            if (Keyence3D.ConnectStatus())
            {
                if (Position == 1)
                {
                    Keyence3D.Sent("EXW,0\r\n");
                    Keyence3D.Sent("EXW,0\r\n");
                }
                if (Position == 2)
                {
                    Keyence3D.Sent("EXW,1\r\n");
                    Keyence3D.Sent("EXW,1\r\n");
                }
                if (Position == 3)
                {
                    Keyence3D.Sent("EXW,2\r\n");
                    Keyence3D.Sent("EXW,2\r\n");
                }
                if (Position == 4)
                {
                    Keyence3D.Sent("EXW,3\r\n");
                    Keyence3D.Sent("EXW,3\r\n");
                }
                if (Position == 5)
                {
                    Keyence3D.Sent("EXW,4\r\n");
                    Keyence3D.Sent("EXW,4\r\n");
                }
                if (Position == 6)
                {
                    Keyence3D.Sent("EXW,5\r\n");
                    Keyence3D.Sent("EXW,5\r\n");
                }
            }
            Thread.Sleep(100);
            return Heightstring; ;
        }
        #region 5. 通用收发（供 DeviceControl 使用）

        /// <summary>本类实现了通用发送 / 接收。</summary>
        public override bool SupportsRawIo { get { return true; } }

        /// <summary>原样发送一段文本。返回空串表示成功。</summary>
        public override string Send(string data)
        {
            if (!IsConnected) return "网口未连接。";

            try
            {
                Keyence3D.Sent(data ?? string.Empty);
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>读取一段返回文本（没有数据时返回空串）。</summary>
        public override string Receive()
        {
            if (!IsConnected) return string.Empty;

            try
            {
                string s = Keyence3D.Receive();
                return (s ?? string.Empty).Trim();
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        #endregion

        #endregion
    }// class
}// namespace
