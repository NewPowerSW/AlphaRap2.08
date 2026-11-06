using HslCommunication.Profinet.Omron;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HslCommunication;
using HslCommunication.Core.Net;

namespace NPSDK
{
    /// <summary>
    /// 具体设备类：欧姆龙PLC，使用网口通讯
    /// 可配置的属性包括：网口连接参数
    /// 版本1.0 初始版本
    /// 修改时间2025-12-13----------------WZF
    /// </summary>
    public class OmronPLC : AbstractDevice
    {
        #region 1. 字段、属性

        //Connect to Omrom IP Adress
        public OmronCipNet omron = new OmronCipNet();         

        // 字段：用于读写网口时，锁线程
        private object _lockObj = new object();

        public bool isSuccess = false;

        private bool connect = false;
        /// <summary>
        /// 重写父类的属性：获取连接状态
        /// </summary>
        public override bool IsConnected
        {
            get
            {
                if (omron != null && connect)
                {
                    return true;
                }
                else
                {
                    return false;
                }
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
        /// 带参实例构造函数：提供PLC的设备名，加载并设置参数，打开网口
        /// </summary>
        /// <param name="deviceName"></param>
        public OmronPLC(string deviceName) : base(deviceName)
        {
            
        }
        #endregion

        #region 3. 可配置的属性

        /// <summary>
        /// 可配置的属性：网口的 IP 地址，默认值为 192.168.100.100
        /// </summary>
        public string IP { get; set; } = "192.168.10.1";

        /// <summary>
        /// 可配置的属性：网口的端口号，默认值为 9004
        /// </summary>
        public int Port { get; set; } = 44818;

        /// <summary>
        /// 可配置的属性：网口读取超时。读取等待超过此时间，视为未接收到数据。单位 ms，默认值为 1000
        /// </summary>
        public int TimeOut { get; set; } = 1000;

        /// <summary>
        /// 可配置的属性：网口在读取条码时的缓存区最大长度。默认值为 100
        /// </summary> 
        public int length { get; set; } = 100;

        public bool IsOperateOK { get; set; } = false;

        #endregion

        #region 4. 主要功能：重写 Open / Close：打开、关闭网口

        /// <summary>
        ///  Connect To Omrom PLC
        /// </summary>
        /// <param name="IPAddressplc"></param>
        /// <param name="Portplc"></param>
        /// <returns></returns>
        private bool ConnectToOmromPLC(string IPAddressplc, int Portplc)
        {
            {
                System.Net.IPAddress iPAddress;
                bool flag = !System.Net.IPAddress.TryParse(IPAddressplc, out iPAddress);
                if (flag)
                {
                    return false;
                }
                else
                {
                    string data = IPAddressplc.ToString();
                    this.omron.IpAddress = data;
                    this.omron.Port = Portplc;
                    omron.ConnectTimeOut = 1000; // 网络连接的超时时间
                    // 读写超时：Hsl 默认是 5s 级，一次卡顿会把整条 IO 链路（含所有写输出）冻住好几秒。
                    // 局域网单次往返通常 1~3ms，500ms 已经非常宽松；超时就让本次读失败，
                    // 由映像沿用上一帧，并靠 IsComAlarm 报警。
                    omron.ReceiveTimeOut = 500;
                    try
                    {
                        OperateResult operateResult = this.omron.ConnectServer();
                        isSuccess = operateResult.IsSuccess;
                        if (isSuccess)
                        {
                            connect = true;
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }
            }
        }

        /// <summary>
        ///  Disconnect From Omrom PlC   
        /// </summary>
        /// <returns></returns>
        private bool DisConnectOmromPLC()
        {
            //OmromPLC断开
            {
                try
                {
                    OperateResult operateResult = this.omron.ConnectClose();
                    isSuccess = operateResult.IsSuccess;
                    if (isSuccess)
                    {
                        connect = false;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public void PLCDispose()
        {
            if (this.omron != null)
            {
                this.omron.Dispose();
            }
        }

        /// <summary>
        /// 主要功能：重写父类方法：加载配置，并打开网口
        /// </summary>
        /// 
        public override void Open()
        {
           IsOperateOK =  ConnectToOmromPLC(IP,Port);
        }

        /// <summary>
        /// 主要功能：重写父类方法：停止扫码，并关闭网口
        /// </summary>
        public override void Close()
        {
            IsOperateOK = DisConnectOmromPLC();
        }
        #endregion

        /// <summary>
        /// 主要功能：重写父类方法：开始运行：开网口
        /// </summary>
        public override void StartRunning()
        {
            
        }

        /// <summary>
        /// 主要功能：重写父类方法：停止运行：停止扫码
        /// </summary>
        public override void StopRunning()
        {
            
        }

        public bool WriteString(string address, string value)
        {
            lock (_lockObj)
            {
                OperateResult write = omron.Write(address, value);
                if (write.IsSuccess)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }         
        }


        public bool WriteInt16(string address, Int16 value)
        {
            lock (_lockObj)
            {
                OperateResult write = omron.Write(address, value);
                if (write.IsSuccess)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            } 

        }

        public bool WriteInt32(string address, Int32 value)
        {
            lock (_lockObj)
            {
                OperateResult write = omron.Write(address, value);
                if (write.IsSuccess)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }     

        }

        public bool WriteUInt16(string address, UInt16 value)
        {
            lock (_lockObj)
            {
                OperateResult write = omron.Write(address, value);
                if (write.IsSuccess)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }

        public bool WriteUInt32(string address, UInt32 value)
        {
            lock (_lockObj)
            {
                OperateResult write = omron.Write(address, value);
                if (write.IsSuccess)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }


        public bool WriteFloat(string address, float value)
        {
            lock (_lockObj)
            {
                OperateResult write = omron.Write(address, value);
                if (write.IsSuccess)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool WriteDouble(string address, Double value)
        {
            lock (_lockObj)
            {
                OperateResult write = omron.Write(address, value);
                if (write.IsSuccess)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        public bool WriteBool(string address, bool value)
        {
            lock (_lockObj)
            {
                OperateResult write = omron.Write(address, value);
                if (write.IsSuccess)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Write data to Omrom PLC
        /// </summary>
        /// <param name="address"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool WriteOmromPLC(string address, object obj)
        {
            var temp = obj.GetType().ToString();
            var content = Convert.ToString(obj);
            if (address == string.Empty || content == string.Empty)
            {
                return false;
            }
            switch (temp)
            {
                case "System.Int32":
                    try
                    {
                        // 写入 int32 类型数据
                        OperateResult write = omron.Write(address, Convert.ToInt32(content));
                        if (write.IsSuccess)
                        {
                            return true;

                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                case "System.String":
                    try
                    {
                        //写入 string 类型数据
                        OperateResult write = omron.Write(address, (string)"abc");                              
                        if (write.IsSuccess)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                case "System.Double":
                    try
                    {
                        //写入 double 类型数据
                        OperateResult write = omron.Write(address, (float)1.2);
                        if (write.IsSuccess)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        //MiddleLayer.HomeF.write_log(ex.ToString(), Color.Red);
                        return false;
                    }
                case "System.Boolean":
                    try
                    {
                        //写入 bool 类型数据
                        OperateResult write = omron.Write(address, Convert.ToBoolean(content));
                        if (write.IsSuccess)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        return false;

                    }
                case "System.Int16":
                    try
                    {
                        //写入 int16 类型数据
                        OperateResult write = omron.Write(address, Convert.ToInt16(content));
                        if (write.IsSuccess)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                case "System.Single":
                    try
                    {
                        //写入 single 类型数据
                        OperateResult write = omron.Write(address, Convert.ToSingle(content));
                        if (write.IsSuccess)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        return false;

                    }
                case "System.bool":
                    try
                    {
                        // 写入 int32 类型数据
                        OperateResult write = omron.Write(address,(bool) true);
                        if (write.IsSuccess)
                        {
                            return true;

                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                default:
                    return false;

            }
        }
        public string OmronReadBool(string address)
        {
            lock (_lockObj)
            {
                try
                {
                    OperateResult<bool> result = omron.ReadBool(address);
                    var read = string.Empty;
                    if (result.IsSuccess)
                    {
                        read = result.Content.ToString(); //读取 int32 类型数据
                        return read;
                    }
                    else
                    {
                        read = "Fail";
                        return read;
                    }
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }
            }
        }

        public string OmronReadInt16(string address)
        {
            lock (_lockObj)
            {
                try
                {
                    OperateResult<short> result = omron.ReadInt16(address);
                    var read = string.Empty;
                    if (result.IsSuccess)
                    {
                        read = result.Content.ToString(); //读取 int32 类型数据
                        return read;
                    }
                    else
                    {
                        read = "Fail";
                        return read;
                    }
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }
            }
            
        }
        /// <summary>
        ///  Read int32 from Omrom PLC  
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public string OmronReadInt32(string address)
        {
            lock (_lockObj)
            {
                try
                {
                    OperateResult<int> result = omron.ReadInt32(address);
                    var read = string.Empty;
                    if (result.IsSuccess)
                    {
                        read = result.Content.ToString(); //读取 int32 类型数据
                        return read;
                    }
                    else
                    {
                        read = "0";
                        return read;
                    }
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }
            }    
        }

        /// <summary>
        /// Read uint16 (hexadecimnal) from Omrom PLC 
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public string OmronReadUInt16(string address)
        {
            lock (_lockObj)
            {
                try
                {
                    OperateResult<UInt16> result = omron.ReadUInt16(address);
                    var read = string.Empty;
                    if (result.IsSuccess)
                    {
                        read = result.Content.ToString("X"); //读取 uint16 类型数据
                        return read;
                    }
                    else
                    {
                        read = "0";
                        return read;
                    }
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }
            }
        }

        /// <summary>
        /// Read uint32 (hexadecimal) from Omrom PLC  
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public string OmronReadUInt32(string address)
        {
            lock (_lockObj)
            {
                try
                {
                    OperateResult<UInt32> result = omron.ReadUInt32(address);
                    var read = string.Empty;
                    if (result.IsSuccess)
                    {
                        read = result.Content.ToString("X"); //读取 uint32 类型数据
                        return read;
                    }
                    else
                    {
                        read = "0";
                        return read;
                    }
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }
            }
        }

        /// <summary>
        /// Read double from Omrom PLC 
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public string OmronReadDouble(string address)
        {
            lock (_lockObj)
            {
                try
                {
                    OperateResult<double> result = omron.ReadDouble(address);
                    var read = string.Empty;
                    if (result.IsSuccess)
                    {
                        read = result.Content.ToString(); //读取 double 类型数据
                        return read;
                    }
                    else
                    {
                        read = "Fail";
                        return read;
                    }
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }
            }
        }

        /// <summary>
        /// Read string from Omrom PLC  
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public string OmronReadString(string address)
        {
            lock(_lockObj)
            {
                try
                {
                    OperateResult<string> result = omron.ReadString(address, 1);
                    string read = string.Empty;
                    if (result.IsSuccess)
                    {
                        read = result.Content.ToString();
                        if (read.Contains("\0"))
                        {
                            string[] sub = read.ToString().Split(new char[] { '\0' });  //解析返回的字符串数据
                            return sub[1];
                        }
                        else
                        {
                            return read;
                        }
                    }
                    else
                    {
                        string s = result.Message;
                        read = "Fail";
                        return read;
                    }
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }
            }

        }

        /// <summary>
        ///  Read bool from Omrom PLC  
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public bool OmronReadBoolean(string address)
        {


            lock (_lockObj)
            {
                try
                {
                    if (address==null)
                        return false;
                    
                    OperateResult<bool> result = omron.ReadBool(address);
                    bool read;
                    if (result.IsSuccess)
                    {
                        read = result.Content; //读取 bool 类型数据
                        return read;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        /// <summary>
        ///  Read float from Omrom PLC  
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public string OmronReadFloat(string address)
        {
            lock ( _lockObj)
            {
                try
                {
                    OperateResult<float> result = omron.ReadFloat(address);
                    var read = string.Empty;
                    if (result.IsSuccess)
                    {
                        read = result.Content.ToString(); //读取 float 类型数据
                        return read;
                    }
                    else
                    {
                        read = "Fail";
                        return read;
                    }
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                }
            }
        }

        /// <summary>
        /// Read PLC boolean signal
        /// </summary>
        /// <returns>null = communication error/read fail; true/false = PLC signal value</returns>
        public bool? ReadBoolean(string address)
        {
            lock (_lockObj)
            {
                try
                {
                    OperateResult<bool> result = omron.ReadBool(address);
                    if (result.IsSuccess)
                    {
                        return result.Content;
                    }
                    // Read operation failed
                    return null;
                }
                catch (Exception ex)
                {
                    // Exception occurs (connection lost etc.)
                    return null;
                }
            }
        }

        /// <summary>整块读成功次数（用于验证驱动是否支持数组整读）。</summary>
        public int BlockReadOkCount = 0;
        /// <summary>整块读失败、退回逐位读的次数（不为 0 且 BlockReadOkCount 为 0 说明整块读一直没成功）。</summary>
        public int BlockReadFallbackCount = 0;

        /// <summary>
        /// 整块读连续失败多少次（且同一批位逐点读正常）后，判定该基名不支持数组整读。
        /// 判定后不再尝试整块读，避免每个周期都白烧一个（可能还要等到超时的）请求。
        /// </summary>
        private const int BlockReadFailThreshold = 3;

        private readonly Dictionary<string, int> _blockReadFailStreak = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly HashSet<string> _blockReadUnsupported = new HashSet<string>(StringComparer.Ordinal);

        /// <summary>true 表示已有基名被判定不支持整块读，正在走逐位读（功能正确但慢）。</summary>
        public bool BlockReadUnsupported { get { return _blockReadUnsupported.Count > 0; } }

        /// <summary>
        /// 整块读一个 BOOL 数组/字（如 I04 一次读 32 位）。
        /// 成功返回 length 个位；失败返回 null，调用方应沿用上一帧。
        /// 先试驱动的数组整读；不支持时退回逐位读，保证功能正确（会慢）。
        /// </summary>
        public bool[] ReadBoolBlock(string baseName, int length)
        {
            if (string.IsNullOrEmpty(baseName) || length <= 0)
                return null;

            // 已判定该基名不支持整块读时，连试都不试（否则每周期白搭一个失败请求）。
            bool triedBlock = false;
            if (!_blockReadUnsupported.Contains(baseName))
            {
                triedBlock = true;
                lock (_lockObj)
                {
                    try
                    {
                        OperateResult<bool[]> r = omron.ReadBool(baseName, (ushort)length);
                        if (r.IsSuccess && r.Content != null && r.Content.Length >= length)
                        {
                            BlockReadOkCount++;
                            _blockReadFailStreak[baseName] = 0;
                            return r.Content;
                        }
                    }
                    catch
                    {
                    }
                }
            }

            BlockReadFallbackCount++;

            // 兜底：逐位读。任一位通信失败就整块判失败，让调用方沿用上一帧。
            bool[] arr = new bool[length];
            for (int i = 0; i < length; i++)
            {
                bool? v = ReadBoolean(baseName + "[" + i + "]");
                if (!v.HasValue)
                    return null;      // 通信问题：不能据此判定"驱动不支持整块读"
                arr[i] = v.Value;
            }

            // 整块读失败、但同一批位逐点读得到 → 说明是这个基名不支持数组整读（而不是网络问题）。
            // 连续若干次后钉死，之后只走逐位读；只打印一次，避免刷屏。
            if (triedBlock)
            {
                int streak;
                _blockReadFailStreak.TryGetValue(baseName, out streak);
                streak++;
                _blockReadFailStreak[baseName] = streak;

                if (streak >= BlockReadFailThreshold && _blockReadUnsupported.Add(baseName))
                {
                    Console.WriteLine("OmronPLC: block read of \"" + baseName
                        + "\" is not supported, using per-bit read (slow). BlockReadFallbackCount="
                        + BlockReadFallbackCount);
                }
            }

            return arr;
        }

    }// class
}// namespace

