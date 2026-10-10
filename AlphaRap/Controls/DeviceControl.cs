using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

namespace AlphaRap
{
    /// <summary>
    /// 设备调试控件：从工具箱拖到窗体上即可用，不需要写代码。
    ///
    /// · <b>选设备类</b>：加载时用反射列出本程序集里所有继承 <see cref="AbstractDevice"/> 的具体类
    ///   （不论构造函数签名、不论 public/internal），下拉框直接选。
    ///   **以后再新增通讯类，重新生成后会自动出现在这个下拉框里。**
    /// · <b>填连接参数</b>：下面那张参数表按所选设备类自动生成（IP / Port / 串口号 / 波特率 …），
    ///   直接改，改完就作用到设备实例上。
    /// · <b>打开 / 关闭 / 发送 / 接收</b>：收发优先走 <see cref="AbstractDevice.Send"/> /
    ///   <see cref="AbstractDevice.Receive"/>；没重写的类会退回按方法名找
    ///   Send / Sent / Write / Read 之类的公开方法；两者都没有时日志里会明确写出来。
    ///   选中设备类时会先在日志里打一条该类的**收发能力**说明。
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

        /// <summary>名称像"发送"的方法（兜底用，找不到就明确提示该重写什么）。</summary>
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
            if (!InDesigner)
            {
                LiveInstances.Add(this);                                    // 供 MainForm 的【保存】按钮统一保存
                this.Disposed += delegate { LiveInstances.Remove(this); };
                CreateDevice();
            }
        }

        #region 设计期属性

        /// <summary>预设的设备类名，如 ScannerKeyenceTcp；下拉框选择时会同步写回这里。</summary>
        [Category("Device"), DefaultValue(null), Description("预设的设备类名，如 ScannerKeyenceTcp / Keyence3DTcp / ATEQ_F620 / ModBus_RTU。留空则运行时取列表第一个。")]
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

        /// <summary>当前下拉框里列出的设备类（宿主可用来做校验/提示）。</summary>
        [Browsable(false)]
        public List<Type> DeviceTypes { get { return new List<Type>(_types); } }

        /// <summary>连接状态或所选设备类变化时触发。</summary>
        public event EventHandler ConnectedChanged;

        /// <summary>收到数据时触发，参数是收到的文本。</summary>
        public event EventHandler<string> Received;

        /// <summary>重新扫描设备类列表（新增的通讯类重新生成后调一次就能出现；已选中的类尽量保持不变）。</summary>
        public void RefreshDeviceTypes()
        {
            BuildTypeList();
        }

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

                string err = _device.SupportsRawIo ? _device.Send(text) : ReflectSend(_device, text);
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

                string text = _device.SupportsRawIo ? _device.Receive() : ReflectReceive(_device);
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

        /// <summary>
        /// 反射列出本程序集里所有继承 <see cref="AbstractDevice"/> 的具体类。
        /// **刻意不限制构造函数签名**——原先要求必须有 (string) 构造，会把后来新增、
        /// 只带无参构造或别的签名的通讯类悄悄漏掉；实例化时再按实际签名挑合适的构造函数。
        /// </summary>
        private void BuildTypeList()
        {
            _types.Clear();
            Type baseType = typeof(AbstractDevice);

            string keep = cboType.SelectedItem is Type ? ((Type)cboType.SelectedItem).Name : _deviceTypeName;
            try
            {
                foreach (Type t in Assembly.GetExecutingAssembly().GetTypes())
                {
                    if (t == baseType || t.IsAbstract || !baseType.IsAssignableFrom(t)) continue;
                    if (t.GetConstructors().Length == 0) continue;    // 没有可用构造函数，实例化不了
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
            try
            {
                cboType.DataSource = new List<Type>(_types);
                if (!string.IsNullOrEmpty(keep)) SelectTypeByName(keep);   // 刷新时不跳回第一个
            }
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
                _device = NewDevice(t, name);
                AppendLog("设备类", _device.DeviceName + "（" + t.Name + "）：" + DescribeIo(_device));
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

        /// <summary>
        /// 按类实际提供的构造函数建实例：优先 (string deviceName)，其次无参，
        /// 再次取第一个公开构造函数并把参数填默认值。这样不管新类怎么写构造，控件都能把它建起来。
        /// </summary>
        private static AbstractDevice NewDevice(Type t, string name)
        {
            ConstructorInfo ci = t.GetConstructor(new[] { typeof(string) });
            if (ci != null) return (AbstractDevice)ci.Invoke(new object[] { name });

            ci = t.GetConstructor(Type.EmptyTypes);
            if (ci != null) return (AbstractDevice)ci.Invoke(null);

            ConstructorInfo[] all = t.GetConstructors();
            if (all.Length == 0)
                throw new MissingMethodException(t.Name + " 没有可用的公开构造函数。");

            ParameterInfo[] ps = all[0].GetParameters();
            object[] args = new object[ps.Length];
            for (int i = 0; i < ps.Length; i++)
                args[i] = ps[i].ParameterType.IsValueType
                        ? Activator.CreateInstance(ps[i].ParameterType)
                        : null;
            return (AbstractDevice)all[0].Invoke(args);
        }

        /// <summary>描述这个设备实例到底能不能收发（选中类时写进日志，一眼看出"有没有发送功能"）。</summary>
        private static string DescribeIo(AbstractDevice d)
        {
            if (d == null) return string.Empty;

            string tx;
            if (d.SupportsRawIo) tx = "支持发送（已重写 Send）";
            else
            {
                MethodInfo mi = FindCandidateMethod(d, SendMethodNames, 1);
                tx = (mi != null) ? ("支持发送（按方法名调到 " + mi.Name + "）")
                                  : "不支持发送（未重写 SupportsRawIo/Send，也没找到可用方法）";
            }

            string rx;
            if (d.SupportsRawIo) rx = "支持接收";
            else
            {
                MethodInfo mi = FindCandidateMethod(d, RecvMethodNames, 0);
                rx = (mi != null) ? ("支持接收（按方法名调到 " + mi.Name + "）") : "不支持接收";
            }

            return tx + "，" + rx;
        }

        private void CloseSilently()
        {
            try { if (_device != null) _device.Close(); }
            catch (Exception) { }
        }

        #endregion

        #region 兜底：老设备类没实现 Send / Receive 时按方法名找

        private string ReflectSend(AbstractDevice d, string text)
        {
            MethodInfo mi = FindCandidateMethod(d, SendMethodNames, 1);
            if (mi == null)
                return "该设备类没有发送能力：既未重写 SupportsRawIo/Send，也找不到 Send/Sent/Write/… 这类公开方法。";

            object r = mi.Invoke(d, new object[] { text });
            if (r is string) return (string)r;
            if (r is bool) return ((bool)r) ? string.Empty : "设备的发送方法返回失败。";
            return string.Empty;
        }

        private string ReflectReceive(AbstractDevice d)
        {
            MethodInfo mi = FindCandidateMethod(d, RecvMethodNames, 0);
            if (mi == null) return string.Empty;

            object r = mi.Invoke(d, null);
            return (r == null) ? string.Empty : r.ToString();
        }

        private static MethodInfo FindCandidateMethod(AbstractDevice d, string[] names, int argCount)
        {
            if (d == null) return null;

            Type t = d.GetType();
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

        #region 参数持久化（XML，跟随 MainForm 的【保存】按钮）

        /// <summary>所有存活的 DeviceControl（MainForm 的【保存】按钮会遍历它们写入 / 还原）。</summary>
        private static readonly List<DeviceControl> LiveInstances = new List<DeviceControl>();

        /// <summary>
        /// 参数文件：{SettingDataDirectory}\DeviceControl.xml —— 机器级、离线可编辑（不依赖设备在线），
        /// 与 VPForm.Cameras.xml 放在同一目录。
        /// </summary>
        public static string XmlFilePath
        {
            get
            {
                string dir = SysPara.SettingDataDirectory;
                if (string.IsNullOrEmpty(dir)) dir = @".\ModuleData\SettingData";
                return Path.Combine(dir, "DeviceControl.xml");
            }
        }

        /// <summary>XML 里的唯一键：窗体名_控件名（同一个窗体上放两个 DeviceControl 也不会撞）。</summary>
        private string XmlKey
        {
            get
            {
                Form f = FindForm();
                string formName = (f == null || string.IsNullOrEmpty(f.Name)) ? "Form" : f.Name;
                string ctlName = string.IsNullOrEmpty(this.Name) ? "DeviceControl" : this.Name;
                return formName + "_" + ctlName;
            }
        }

        /// <summary>主界面【保存】选"是"时由 MainForm.SaveData() 调用：把所有 DeviceControl 的参数写入 XML。</summary>
        public static void SaveAll()
        {
            try
            {
                string path = XmlFilePath;
                XmlDocument doc = new XmlDocument();
                try { if (File.Exists(path)) doc.Load(path); }
                catch (Exception) { doc = new XmlDocument(); }

                if (doc.DocumentElement == null)
                {
                    doc = new XmlDocument();
                    doc.AppendChild(doc.CreateElement("DeviceControl"));
                }

                for (int i = 0; i < LiveInstances.Count; i++)
                    LiveInstances[i].WriteToDoc(doc);

                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

                doc.Save(path);
            }
            catch (Exception) { }
        }

        /// <summary>主界面【保存】选"否"时由 MainForm.SaveData() 调用：把参数还原成 XML 里上次保存的值。</summary>
        public static void ReloadAll()
        {
            for (int i = 0; i < LiveInstances.Count; i++)
                LiveInstances[i].LoadFromXml();
        }

        /// <summary>把本实例当前设备参数写进 XML 文档（没有对应节点就新建）。</summary>
        private void WriteToDoc(XmlDocument doc)
        {
            if (_device == null || doc.DocumentElement == null) return;

            string key = XmlKey;
            XmlElement node = null;
            for (int i = 0; i < doc.DocumentElement.ChildNodes.Count; i++)
            {
                XmlElement e = doc.DocumentElement.ChildNodes[i] as XmlElement;
                if (e != null && e.Name == "Device" && e.GetAttribute("Key") == key) { node = e; break; }
            }
            if (node == null)
            {
                node = doc.CreateElement("Device");
                node.SetAttribute("Key", key);
                doc.DocumentElement.AppendChild(node);
            }

            node.SetAttribute("Type", _device.GetType().Name);
            node.SetAttribute("DeviceName", _device.DeviceName ?? string.Empty);

            for (int i = node.ChildNodes.Count - 1; i >= 0; i--) node.RemoveChild(node.ChildNodes[i]);   // 清掉旧参数

            foreach (PropertyInfo p in WritableProperties(_device.GetType()))
            {
                try
                {
                    object v = p.GetValue(_device, null);
                    XmlElement item = doc.CreateElement(p.Name);
                    item.InnerText = ToInvariantString(v, p.PropertyType);
                    node.AppendChild(item);
                }
                catch (Exception) { }
            }
        }

        /// <summary>从 XML 读回本实例的参数并套到设备实例上（离线保存的核心：整个过程不依赖设备在线）。</summary>
        public void LoadFromXml()
        {
            try
            {
                string path = XmlFilePath;
                if (_device == null || !File.Exists(path)) return;

                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                if (doc.DocumentElement == null) return;

                string key = XmlKey;
                bool applied = false;
                for (int i = 0; i < doc.DocumentElement.ChildNodes.Count; i++)
                {
                    XmlElement e = doc.DocumentElement.ChildNodes[i] as XmlElement;
                    if (e == null || e.Name != "Device" || e.GetAttribute("Key") != key) continue;

                    foreach (XmlNode c in e.ChildNodes)
                        if (ApplyOne(_device, c.Name, c.InnerText)) applied = true;
                    break;
                }

                if (!applied) return;

                if (propGrid != null) propGrid.Refresh();       // 参数表显示载入后的值
                AppendLog("参数", "已从 " + Path.GetFileName(path) + " 载入离线参数");
            }
            catch (Exception) { }
        }

        /// <summary>可持久化的属性：public、可读可写、非索引器、未被 [Browsable(false)] 屏蔽。</summary>
        private static List<PropertyInfo> WritableProperties(Type t)
        {
            List<PropertyInfo> list = new List<PropertyInfo>();
            foreach (PropertyInfo p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!p.CanRead || !p.CanWrite) continue;
                if (p.GetIndexParameters().Length > 0) continue;

                object[] attrs = p.GetCustomAttributes(typeof(BrowsableAttribute), false);
                if (attrs.Length > 0 && !((BrowsableAttribute)attrs[0]).Browsable) continue;

                list.Add(p);
            }
            return list;
        }

        /// <summary>把属性值转成可写进 XML 的字符串（走 TypeConverter，enum / bool / int 都能正确处理）。</summary>
        private static string ToInvariantString(object v, Type t)
        {
            if (v == null) return string.Empty;
            try
            {
                TypeConverter tc = TypeDescriptor.GetConverter(t);
                if (tc != null && tc.CanConvertTo(typeof(string))) return tc.ConvertToInvariantString(v);
            }
            catch (Exception) { }
            return v.ToString();
        }

        /// <summary>把 XML 里的字符串按属性类型转回来并写进设备实例；成功返回 true。</summary>
        private static bool ApplyOne(AbstractDevice d, string propName, string text)
        {
            try
            {
                PropertyInfo p = d.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
                if (p == null || !p.CanWrite) return false;

                Type t = p.PropertyType;
                object v;
                if (t == typeof(string)) v = text;
                else
                {
                    TypeConverter tc = TypeDescriptor.GetConverter(t);
                    v = (tc != null && tc.CanConvertFrom(typeof(string))) ? tc.ConvertFromInvariantString(text) : null;
                }
                if (v == null) return false;

                p.SetValue(d, v, null);
                return true;
            }
            catch (Exception) { return false; }
        }

        #endregion

        #region 界面

        private void DeviceControl_Load(object sender, EventArgs e)
        {
            LoadFromXml();          // 离线参数：按 XML 上次保存的值套到设备实例上（此时才取得到窗体名做键）

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
