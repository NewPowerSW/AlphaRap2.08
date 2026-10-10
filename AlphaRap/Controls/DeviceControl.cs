using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>
    /// 设备调试控件：从工具箱拖到窗体上即可用，不需要写代码。
    ///
    /// · <b>选设备类</b>：加载时用反射列出本程序集里所有继承 <see cref="AbstractDevice"/> 且带
    ///   <c>(string deviceName)</c> 构造函数的类，下拉框直接选。
    /// · <b>填连接参数</b>：下面那张参数表按所选设备类自动生成（IP / Port / 串口号 / 波特率 …），
    ///   直接改，改完就作用到设备实例上。
    /// · <b>打开 / 关闭 / 发送 / 接收</b>：收发走 <see cref="AbstractDevice.Send"/> /
    ///   <see cref="AbstractDevice.Receive"/>；老设备类没实现时控件会退回反射查找
    ///   Write / Sent / Read 之类的公开方法。收发内容带时间戳记在最下面的框里。
    ///
    /// 设计期可在属性窗口预设 <see cref="DeviceTypeName"/>（类名，如 ScannerKeyenceTcp）与 <see cref="DeviceName"/>。
    /// </summary>
    public partial class DeviceControl : UserControl
    {
        private AbstractDevice _device;
        private readonly List<Type> _types = new List<Type>();
        private string _deviceTypeName;
        private int _logLines;

        /// <summary>列表刷新中：此时下拉框的选中变化不算作"用户选了设备类"。</summary>
        private bool _loading;

        /// <summary>名称像"发送"的方法（兜底用，找不到就报错提示重写 Send）。</summary>
        private static readonly string[] SendMethodNames =
            { "Send", "Sent", "Write", "SendData", "SendCommand", "SendString", "SendMsg" };

        /// <summary>名称像"接收"的方法（兜底用）。</summary>
        private static readonly string[] RecvMethodNames =
            { "Receive", "Recv", "Read", "ReadData", "Readdata", "ReadExisting", "ReadString", "ScanOnce" };

        public DeviceControl()
        {
            InitializeComponent();

            BuildTypeList();

            // 设计期不建设备实例（免得 VS 设计器里碰真实端口）；运行期先建一个，参数表立刻可编辑
            if (!InDesigner) CreateDevice();
        }

        #region 设计期属性

        /// <summary>预设的设备类名，如 ScannerKeyenceTcp；下拉框选择时会同步写回这里。</summary>
        [Category("Device"), Description("预设的设备类名，如 ScannerKeyenceTcp / Keyence3DTcp / ATEQ_F620 / ModBus_RTU。留空则运行时取列表第一个。")]
        public string DeviceTypeName
        {
            get { return _deviceTypeName; }
            set
            {
                _deviceTypeName = value;
                SelectTypeByName(value);
            }
        }

        /// <summary>设备名：传给设备类构造函数（一般作为配置文件 section）。</summary>
        [Category("Device"), Description("设备名，传给设备类构造函数（一般作为配置文件 section）。")]
        public string DeviceName
        {
            get { return (txtName == null) ? null : txtName.Text; }
            set { if (txtName != null) txtName.Text = value ?? string.Empty; }
        }

        /// <summary>控件加载时是否自动打开连接。</summary>
        [Category("Device"), DefaultValue(false), Description("控件加载时是否自动打开连接。")]
        public bool AutoOpen { get; set; }

        #endregion

        #region 对外接口（宿主窗体可以直接调）

        /// <summary>当前设备实例；还没选设备类时为 null。</summary>
        [Browsable(false)]
        public AbstractDevice Device { get { return _device; } }

        /// <summary>连接状态或所选设备类变化时触发。</summary>
        public event EventHandler ConnectedChanged;

        /// <summary>收到数据时触发，参数是收到的文本。</summary>
        public event EventHandler<string> Received;

        /// <summary>打开连接；返回空串表示成功，非空为失败原因。</summary>
        public string OpenDevice()
        {
            try
            {
                if (_device == null && !CreateDevice()) return "未选择设备类。";

                _device.Open();
                AppendLog("打开", _device.DeviceName + "（" + _device.GetType().Name + "）"
                    + (_device.IsConnected ? " 已连接" : " 未连接"));
                RefreshState();

                return _device.IsConnected ? string.Empty : "已调用 Open()，但设备报告未连接。";
            }
            catch (Exception ex)
            {
                AppendLog("错误", "打开失败：" + ex.Message);
                RefreshState();
                return ex.Message;
            }
        }

        /// <summary>关闭连接。</summary>
        public void CloseDevice()
        {
            try
            {
                if (_device == null) return;
                _device.Close();
                AppendLog("关闭", _device.DeviceName + " 已关闭");
            }
            catch (Exception ex)
            {
                AppendLog("错误", "关闭失败：" + ex.Message);
            }
            RefreshState();
        }

        /// <summary>发送一段文本；返回空串表示成功，非空为失败原因。</summary>
        public string SendData(string text)
        {
            if (string.IsNullOrEmpty(text)) return "发送内容为空。";

            try
            {
                if (_device == null && !CreateDevice()) return "未选择设备类。";

                string err = _device.SupportsRawIo ? _device.Send(text) : ReflectSend(text);
                err = err ?? string.Empty;

                AppendLog(string.IsNullOrEmpty(err) ? "发送" : "发送失败",
                    Escape(text) + (string.IsNullOrEmpty(err) ? string.Empty : "  ← " + err));
                RefreshState();
                return err;
            }
            catch (Exception ex)
            {
                AppendLog("错误", "发送失败：" + ex.Message);
                return ex.Message;
            }
        }

        /// <summary>接收一次；返回收到的文本（空串表示没有数据）。</summary>
        public string ReceiveData()
        {
            try
            {
                if (_device == null && !CreateDevice()) return string.Empty;

                string text = _device.SupportsRawIo ? _device.Receive() : ReflectReceive();
                text = text ?? string.Empty;

                if (text.Length > 0)
                {
                    AppendLog("接收", Escape(text));
                    EventHandler<string> h = Received;
                    if (h != null) h(this, text);
                }
                else
                {
                    AppendLog("接收", "(无数据)");
                }

                RefreshState();
                return text;
            }
            catch (Exception ex)
            {
                AppendLog("错误", "接收失败：" + ex.Message);
                return string.Empty;
            }
        }

        #endregion

        #region 设备类列表与实例

        /// <summary>是否运行在 VS 设计器中（设计期不建设备实例、不碰真实端口）。</summary>
        private static bool InDesigner
        {
            get
            {
                try { return LicenseManager.UsageMode == LicenseUsageMode.Designtime; }
                catch { return false; }
            }
        }

        /// <summary>反射列出本程序集里所有可实例化的设备类（继承 AbstractDevice 且带 (string) 构造函数）。</summary>
        private void BuildTypeList()
        {
            _types.Clear();
            Type baseType = typeof(AbstractDevice);

            try
            {
                foreach (Type t in Assembly.GetExecutingAssembly().GetTypes())
                {
                    if (t == baseType || t.IsAbstract || !baseType.IsAssignableFrom(t)) continue;
                    if (t.GetConstructor(new[] { typeof(string) }) == null) continue;   // 要 (string deviceName)
                    _types.Add(t);
                }
            }
            catch (Exception) { }

            _types.Sort(delegate (Type a, Type b)
            {
                return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
            });

            cboType.DisplayMember = "Name";
            _loading = true;
            try { cboType.DataSource = new List<Type>(_types); }
            finally { _loading = false; }
        }

        private void SelectTypeByName(string typeName)
        {
            if (cboType == null || _types.Count == 0 || string.IsNullOrEmpty(typeName)) return;

            for (int i = 0; i < _types.Count; i++)
            {
                if (!string.Equals(_types[i].Name, typeName, StringComparison.OrdinalIgnoreCase)) continue;
                if (cboType.SelectedIndex != i) cboType.SelectedIndex = i;
                return;
            }
        }

        /// <summary>按当前下拉框选择建一个设备实例，并把它的可配置属性挂到参数表上。</summary>
        private bool CreateDevice()
        {
            CloseSilently();

            _device = null;
            Type t = cboType.SelectedItem as Type;
            if (t == null)
            {
                propGrid.SelectedObject = null;
                RefreshState();
                return false;
            }

            string name = (txtName == null) ? null : txtName.Text;
            if (string.IsNullOrEmpty(name)) name = string.IsNullOrEmpty(this.Name) ? "Device1" : this.Name;

            try
            {
                _device = (AbstractDevice)Activator.CreateInstance(t, new object[] { name });
            }
            catch (Exception ex)
            {
                _device = null;
                AppendLog("错误", "创建 " + t.Name + " 失败：" + ex.Message);
            }

            propGrid.SelectedObject = _device;   // 参数表：IP / Port / 串口号…自动列出
            RefreshState();
            return _device != null;
        }

        private void CloseSilently()
        {
            try { if (_device != null) _device.Close(); }
            catch (Exception) { }
        }

        #endregion

        #region 兜底：老设备类没实现 Send / Receive 时按方法名找

        private string ReflectSend(string text)
        {
            MethodInfo mi = FindCandidateMethod(SendMethodNames, 1);
            if (mi == null)
                return "该设备类没实现 Send，也找不到可用的发送方法（可在它里面重写 AbstractDevice.Send）。";

            object r = mi.Invoke(_device, new object[] { text });
            if (r is string) return (string)r;
            if (r is bool) return ((bool)r) ? string.Empty : "设备的发送方法返回失败。";
            return string.Empty;
        }

        private string ReflectReceive()
        {
            MethodInfo mi = FindCandidateMethod(RecvMethodNames, 0);
            if (mi == null) return string.Empty;

            object r = mi.Invoke(_device, null);
            return (r == null) ? string.Empty : r.ToString();
        }

        private MethodInfo FindCandidateMethod(string[] names, int argCount)
        {
            if (_device == null) return null;

            Type t = _device.GetType();
            foreach (string n in names)
            {
                MethodInfo mi = t.GetMethod(n, BindingFlags.Public | BindingFlags.Instance);
                if (mi == null) continue;

                ParameterInfo[] ps = mi.GetParameters();
                if (ps.Length != argCount) continue;
                if (argCount == 1 && ps[0].ParameterType != typeof(string)) continue;

                if (mi.ReturnType == typeof(void) || mi.ReturnType == typeof(bool) || mi.ReturnType == typeof(string))
                    return mi;
            }
            return null;
        }

        #endregion

        #region 界面

        private void DeviceControl_Load(object sender, EventArgs e)
        {
            if (AutoOpen)
            {
                string err = OpenDevice();
                if (!string.IsNullOrEmpty(err)) AppendLog("提示", err);
            }
        }

        private void cboType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loading) return;                                // 列表刷新引起的变化，不算用户选择

            Type t = cboType.SelectedItem as Type;
            if (t != null) _deviceTypeName = t.Name;

            if (InDesigner) return;                              // 设计期不建实例
            if (_device != null && _device.GetType() == t) return;

            CreateDevice();
        }

        private void txtName_Leave(object sender, EventArgs e)
        {
            if (InDesigner || _device == null) return;
            if (_device.DeviceName == txtName.Text) return;

            if (_device.IsConnected)
            {
                AppendLog("提示", "设备已连接，改名要重开连接后才生效。");
                return;
            }
            CreateDevice();     // 未连接时直接按新名字重建
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            string err = OpenDevice();
            if (!string.IsNullOrEmpty(err)) AppendLog("提示", err);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            CloseDevice();
        }

        private void btnRecv_Click(object sender, EventArgs e)
        {
            ReceiveData();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            string err = SendData(txtSend.Text);
            if (!string.IsNullOrEmpty(err)) AppendLog("提示", err);
        }

        /// <summary>刷新状态行（设备名 / 类名 / 连没连）。</summary>
        private void RefreshState()
        {
            bool connected = _device != null && _device.IsConnected;

            if (_device == null)
            {
                lblState.Text = "未选择设备类";
                lblState.ForeColor = UiKit.TextMuted;
            }
            else
            {
                lblState.Text = _device.DeviceName + "（" + _device.GetType().Name + "）："
                              + (connected ? "已连接" : "未连接");
                lblState.ForeColor = connected ? UiKit.Success : UiKit.Danger;
            }

            EventHandler h = ConnectedChanged;
            if (h != null) h(this, EventArgs.Empty);
        }

        /// <summary>往下面的记录框追加一行（带时间戳）。</summary>
        private void AppendLog(string tag, string text)
        {
            try
            {
                if (_logLines > 500) { txtRecv.Clear(); _logLines = 0; }   // 别让记录无限长
                txtRecv.AppendText("[" + DateTime.Now.ToString("HH:mm:ss.fff") + "] " + tag + "：" + text
                    + Environment.NewLine);
                _logLines++;
            }
            catch (Exception) { }
        }

        /// <summary>把回车换行转成可见的 \r\n，免得记录框里看不出报文边界。</summary>
        private static string Escape(string s)
        {
            return (s ?? string.Empty).Replace("\r", "\\r").Replace("\n", "\\n");
        }

        #endregion
    }
}
