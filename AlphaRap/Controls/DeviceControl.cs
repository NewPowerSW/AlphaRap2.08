using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

namespace AlphaRap
{
    /// <summary>
    /// 设备管理控件：从工具箱拖到窗体上即可用，不需要写代码。
    ///
    /// · <b>添加 / 删除设备</b>：一个控件可以管理**多台**设备。【添加】时用反射列出本程序集里所有继承
    ///   <see cref="AbstractDevice"/> 的具体类（不论构造函数签名、不论 public/internal）任选一个，
    ///   自动起名 Device1/Device2…，名字可在"设备名"框里改。【删除】会连同它的参数一起从配置里移除。
    ///   **以后再新增通讯类，重新生成后在【添加】的列表里就会出现。**
    /// · <b>选择设备</b>：左边的下拉框列出已配置的设备（显示 名字（类名）），选哪台就编辑哪台的参数。
    /// · <b>填连接参数</b>：中间那张参数表按该设备的类自动生成（IP / Port / 串口号 / 波特率 …），直接改。
    /// · <b>打开 / 关闭 / 发送 / 接收</b>：只作用于当前选中的设备；收发优先走
    ///   <see cref="AbstractDevice.Send"/> / <see cref="AbstractDevice.Receive"/>，没重写的类退回按方法名找
    ///   Send / Sent / Write / Read 之类的公开方法；都没有时日志里会明确写出来。
    ///   未连接时【发送】【接收】置灰。
    ///
    /// 参数保存在 {SettingDataDirectory}\DeviceControl.xml，**跟随 MainForm 的【保存】按钮**（选"是"写入、选"否"回滚）。
    /// </summary>
    public partial class DeviceControl : UserControl
    {
        /// <summary>一台已配置的设备。一个 DeviceControl 可以管理多台。</summary>
        private class DeviceItem
        {
            public string Name;             // 设备名（在控件内唯一，同时是 XML 里的 Name）
            public Type DeviceType;         // AbstractDevice 的具体子类
            public AbstractDevice Instance; // 该设备的实例（参数表绑它）

            public override string ToString()
            {
                return Name + "（" + ((DeviceType == null) ? "?" : DeviceType.Name) + "）";
            }
        }

        private AbstractDevice _device;                                  // = 当前设备的实例（_current.Instance）
        private readonly List<DeviceItem> _devices = new List<DeviceItem>();
        private DeviceItem _current;                                     // 当前选中的设备
        private string _pendingCurrent;                                   // XML 里记录的"上次选中的设备名"
        private readonly List<Type> _types = new List<Type>();            // 可添加的设备类（反射扫描结果）
        private string _deviceTypeName;
        private int _logLines;

        /// <summary>单设备模式：一个面板只代表一台设备（隐藏内部设备下拉/添加/删除，右上角出【移除】）。</summary>
        private bool _singleDevice;

        /// <summary>设备列表刷新中：此时下拉框的选中变化不算作用户操作。</summary>
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
            if (!InDesigner) ApplyLanguage();   // 按当前语言覆盖 Designer 里写死的文案（设计期保留预览）

            BuildTypeList();        // 反射扫描设备类，供【添加】时挑选
            ApplyMode();            // 按 SingleDevice 决定顶行显示"设备下拉/添加/删除"还是"设备类名 + 移除"

            // 设计期不碰真实端口、也不建实例；运行期的设备来自 XML（Load 事件）或【添加】按钮
            if (!InDesigner)
            {
                LiveInstances.Add(this);
                RaiseDevicesChanged();

                _langHandler = delegate { ApplyLanguage(); };
                MiddleLayer.LanguageChanged += _langHandler;                                    // 供 MainForm 的【保存】按钮统一保存
                this.Disposed += delegate
                {
                    LiveInstances.Remove(this);
                    MiddleLayer.LanguageChanged -= _langHandler;   // 静态事件必须解绑，否则面板会被一直留住
                    RaiseDevicesChanged();
                };
                if (propGrid != null) propGrid.PropertyValueChanged += propGrid_PropertyValueChanged;
            }
        }

        #region 设计期属性

        /// <summary>【添加】时预选的设备类名（如 ScannerKeyenceTcp）。设备本身在【添加】按钮里管理。</summary>
        [Category("Device"), DefaultValue(null), Description("【添加】时预选的设备类名，如 ScannerKeyenceTcp / Keyence3DTcp / ATEQ_F620 / ModBus_RTU。")]
        public string DeviceTypeName
        {
            get { return _deviceTypeName; }
            set { _deviceTypeName = value; }
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

        /// <summary>
        /// 单设备模式：**一个面板只代表一台设备**。
        /// 打开后隐藏内部的"设备下拉框 + 【添加】/【删除】"，改为右上角显示【移除】，
        /// 顶行左侧直接显示本面板绑定的设备类名。适合宿主窗体（如 Parameter）用按钮动态增删面板。
        /// 设备本身用 <see cref="BindDevice"/> 绑定；参数仍走 XML（跟随 MainForm 的【保存】）。
        /// </summary>
        [Category("Device"), DefaultValue(false), Description("单设备模式：一个面板只代表一台设备（隐藏内部设备下拉与添加/删除，右上角显示【移除】）。")]
        public bool SingleDevice
        {
            get { return _singleDevice; }
            set { _singleDevice = value; ApplyMode(); }
        }

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

        /// <summary>单设备模式下点了【移除】。宿主收到后把本面板从容器里拿掉（并 Dispose）。</summary>
        public event EventHandler RemoveRequested;

        /// <summary>设备名变了（在"设备名"框里改，或绑定设备时定下来）。宿主用它同步页签标题之类的显示。</summary>
        public event EventHandler DeviceNameChanged;

        /// <summary>单设备模式下绑定的设备类名；还没绑定时返回空串。</summary>
        [Browsable(false)]
        public string BoundTypeName
        {
            get { return (_current == null || _current.DeviceType == null) ? string.Empty : _current.DeviceType.Name; }
        }

        /// <summary>
        /// 单设备模式：绑定一台设备（建实例 → 加入设备列表 → 切过去）。返回空串=成功，非空为失败原因。
        /// 宿主的"添加设备"按钮建好面板后调它即可。
        /// </summary>
        public string BindDevice(string typeName, string deviceName)
        {
            if (string.IsNullOrEmpty(typeName)) return L("err_NoType", "没有指定设备类。", "No device type specified.", "No se especificó el tipo de dispositivo.");

            if (_types.Count == 0) BuildTypeList();

            Type t = null;
            for (int i = 0; i < _types.Count; i++)
                if (string.Equals(_types[i].Name, typeName, StringComparison.OrdinalIgnoreCase)) { t = _types[i]; break; }
            if (t == null) return L("err_TypeMissing1", "找不到设备类 ", "Device type not found: ", "Tipo no encontrado: ") + typeName
                    + L("err_TypeMissing2", "（本程序集里没有这个 AbstractDevice 子类）。", " (no such AbstractDevice subclass in this assembly).", " (no existe esa subclase de AbstractDevice).");

            string name = string.IsNullOrEmpty(deviceName) ? AutoDeviceName() : deviceName;
            if (FindDevice(name) != null) name = AutoDeviceName();

            AbstractDevice inst;
            try { inst = NewDevice(t, name); }
            catch (Exception ex) { return L("msg_CreateFail", "创建 ", "Create ", "Crear ") + t.Name + L("msg_Failed", " 失败：", " failed: ", " falló: ") + ex.Message; }

            DeviceItem item = new DeviceItem();
            item.Name = name;
            item.DeviceType = t;
            item.Instance = inst;
            _devices.Add(item);

            RebuildDeviceList(name);
            SwitchToDevice(item);
            RaiseNameChanged();          // 名字定下来了，通知宿主（页签标题等）
            return string.Empty;
        }

        /// <summary>通知宿主"设备名变了"。</summary>
        private void RaiseNameChanged()
        {
            EventHandler h = DeviceNameChanged;
            if (h != null) h(this, EventArgs.Empty);
        }

        /// <summary>重新扫描设备类列表（新增的通讯类重新生成后调一次就能出现；已选中的类尽量保持不变）。</summary>
        public void RefreshDeviceTypes()
        {
            BuildTypeList();
        }

        /// <summary>
        /// 检测本机 TCP 是否被代理软件（Clash/Mihomo 的 TUN 模式等）**整体接管**：
        /// 去连一个保留地址 192.0.2.1:80（RFC5737 TEST-NET-1，公网永不路由），正常情况下必然连不上；
        /// 若它也能"连上"，说明所有 TCP 都被代理接住了 —— 此时任何"已连接"都不足信。
        /// </summary>
        public static bool IsTcpHijacked()
        {
            try
            {
                using (System.Net.Sockets.TcpClient probe = new System.Net.Sockets.TcpClient())
                {
                    IAsyncResult ar = probe.BeginConnect("192.0.2.1", 80, null, null);
                    if (!ar.AsyncWaitHandle.WaitOne(700, false)) return false;   // 连不上 ⇒ 正常
                    probe.EndConnect(ar);
                    return true;                                                 // 连上了 ⇒ 被代理接管
                }
            }
            catch (Exception)
            {
                return false;                                                    // 被拒绝/报错 ⇒ 正常
            }
        }

        /// <summary>打开连接；返回空串表示成功，非空为失败原因。</summary>
        public string OpenDevice()
        {
            try
            {
                if (_device == null) return L("msg_NoDevice", "还没有添加设备，请先点【添加】。", "No device yet - click Add first.", "Sin dispositivo - pulse Agregar primero.");

                _device.Open();
                AppendLog(L("tag_Open", "打开", "Open", "Abrir"), _device.DeviceName + "（" + _device.GetType().Name + "）"
                    + (_device.IsConnected ? " 已连接" : " 未连接"));
                RefreshState();

                return _device.IsConnected ? string.Empty
                    : L("msg_OpenNotConnected", "没有连上：设备报告未连接。请检查 IP / 端口 / 网线；另外代理软件（如 Clash 的 TUN 模式）会接管 TCP 连接，"
                    + "让不存在的设备也显示连上，排查时请先关掉它。",
                        "Not connected: the device reports no link. Check IP / port / cable. Proxy software (e.g. Clash TUN mode) hijacks TCP and can make a non-existent device look connected - turn it off when diagnosing.",
                        "Sin conexión: el dispositivo indica que no hay enlace. Revise IP / puerto / cable. El software proxy (p. ej. modo TUN de Clash) intercepta TCP y puede hacer que un dispositivo inexistente parezca conectado.");
            }
            catch (Exception ex)
            {
                AppendLog(L("tag_Error", "错误", "Error", "Error"), L("msg_OpenFail", "打开失败：", "Open failed: ", "Error al abrir: ") + ex.Message);
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
                AppendLog(L("tag_Close", "关闭", "Close", "Cerrar"), _device.DeviceName + L("msg_Closed", " 已关闭", " closed", " cerrado"));
            }
            catch (Exception ex)
            {
                AppendLog(L("tag_Error", "错误", "Error", "Error"), L("msg_CloseFail", "关闭失败：", "Close failed: ", "Error al cerrar: ") + ex.Message);
            }
            RefreshState();
        }

        /// <summary>发送一段文本；返回空串表示成功，非空为失败原因。</summary>
        public string SendData(string text)
        {
            if (string.IsNullOrEmpty(text)) return L("msg_EmptySend", "发送内容为空。", "Nothing to send.", "No hay nada que enviar.");

            try
            {
                if (_device == null) return L("msg_NoDevice", "还没有添加设备，请先点【添加】。", "No device yet - click Add first.", "Sin dispositivo - pulse Agregar primero.");
                if (!_device.IsConnected) return L("msg_NotConnectedSend", "未连接设备，无法发送。请先点【打开连接】。", "Not connected - cannot send. Click Open first.", "Sin conexión - no se puede enviar. Pulse Abrir.");

                string err = _device.SupportsRawIo ? _device.Send(text) : ReflectSend(_device, text);
                err = err ?? string.Empty;

                AppendLog(string.IsNullOrEmpty(err)
                        ? L("tag_Send", "发送", "Send", "Enviar")
                        : L("tag_SendFail", "发送失败", "Send failed", "Envío fallido"),
                    Escape(text) + (string.IsNullOrEmpty(err) ? string.Empty : "  ← " + err));
                RefreshState();
                return err;
            }
            catch (Exception ex)
            {
                AppendLog(L("tag_Error", "错误", "Error", "Error"), L("msg_SendFail", "发送失败：", "Send failed: ", "Error al enviar: ") + ex.Message);
                return ex.Message;
            }
        }

        /// <summary>接收一次；返回收到的文本（空串表示没有数据）。</summary>
        public string ReceiveData()
        {
            try
            {
                if (_device == null)
                {
                    AppendLog(L("tag_Tip", "提示", "Info", "Aviso"), L("msg_NoDevice", "还没有添加设备，请先点【添加】。", "No device yet - click Add first.", "Sin dispositivo - pulse Agregar primero."));
                    return string.Empty;
                }
                if (!_device.IsConnected)
                {
                    AppendLog(L("tag_Tip", "提示", "Info", "Aviso"), L("msg_NotConnectedRecv", "未连接设备，无法接收。请先点【打开连接】。", "Not connected - cannot receive. Click Open first.", "Sin conexión - no se puede recibir. Pulse Abrir."));
                    return string.Empty;
                }

                string text = _device.SupportsRawIo ? _device.Receive() : ReflectReceive(_device);
                text = text ?? string.Empty;

                if (text.Length > 0)
                {
                    AppendLog(L("tag_Recv", "接收", "Recv", "Recibir"), Escape(text));
                    EventHandler<string> h = Received;
                    if (h != null) h(this, text);
                }
                else
                {
                    AppendLog(L("tag_Recv", "接收", "Recv", "Recibir"), L("msg_NoData", "(无数据)", "(no data)", "(sin datos)"));
                }

                RefreshState();
                return text;
            }
            catch (Exception ex)
            {
                AppendLog(L("tag_Error", "错误", "Error", "Error"), L("msg_RecvFail", "接收失败：", "Receive failed: ", "Error al recibir: ") + ex.Message);
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
        /// 反射列出本程序集里所有继承 <see cref="AbstractDevice"/> 的具体类（按名字排序）。
        /// 不限制构造函数签名：实例化时按实际签名挑合适的构造函数。
        /// </summary>
        public static List<Type> ScanDeviceTypes()
        {
            List<Type> list = new List<Type>();
            Type baseType = typeof(AbstractDevice);
            try
            {
                foreach (Type t in Assembly.GetExecutingAssembly().GetTypes())
                {
                    if (t == baseType || t.IsAbstract || !baseType.IsAssignableFrom(t)) continue;
                    if (t.GetConstructors().Length == 0) continue;    // 没有可用构造函数，实例化不了
                    list.Add(t);
                }
            }
            catch (Exception) { }

            list.Sort(delegate (Type a, Type b)
            {
                return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
            });
            return list;
        }

        private void BuildTypeList()
        {
            _types.Clear();
            _types.AddRange(ScanDeviceTypes());

            // 补上已配置但已被改名 / 删除的设备类，避免它的历史配置整条丢失
            for (int i = 0; i < _devices.Count; i++)
            {
                bool found = false;
                for (int j = 0; j < _types.Count; j++)
                    if (_types[j] == _devices[i].DeviceType) { found = true; break; }
                if (!found && _devices[i].DeviceType != null) _types.Add(_devices[i].DeviceType);
            }
        }

        /// <summary>在"设备"下拉框里选中指定名字的设备。</summary>
        private void SelectDeviceByName(string name)
        {
            if (cboType == null || string.IsNullOrEmpty(name)) return;

            for (int i = 0; i < _devices.Count; i++)
            {
                if (!string.Equals(_devices[i].Name, name, StringComparison.OrdinalIgnoreCase)) continue;
                if (cboType.SelectedIndex != i) cboType.SelectedIndex = i;
                return;
            }
        }

        /// <summary>重建"设备"下拉框（数据源 = _devices），并选中指定名字的设备。</summary>
        private void RebuildDeviceList(string selectName)
        {
            if (cboType == null) return;

            _loading = true;
            try
            {
                cboType.DataSource = null;
                cboType.DisplayMember = string.Empty;               // 用 DeviceItem.ToString() ⇒ "名字（类名）"
                cboType.DataSource = new List<DeviceItem>(_devices);
                if (!string.IsNullOrEmpty(selectName)) SelectDeviceByName(selectName);
            }
            finally { _loading = false; }
        }

        private DeviceItem FindDevice(string name)
        {
            for (int i = 0; i < _devices.Count; i++)
                if (string.Equals(_devices[i].Name, name, StringComparison.OrdinalIgnoreCase)) return _devices[i];
            return null;
        }

        /// <summary>自动起一个没被占用的设备名：Device1 / Device2 …</summary>
        private string AutoDeviceName()
        {
            for (int i = 1; ; i++)
            {
                string n = "Device" + i;
                if (FindDevice(n) == null) return n;
            }
        }

        /// <summary>切换到指定设备（实例在【添加】时已建好，这里只做挂载与界面同步）。</summary>
        private void SwitchToDevice(DeviceItem item)
        {
            CloseSilently();                                     // 换设备前先断开上一台

            _current = item;
            _device = (item == null) ? null : item.Instance;

            if (txtName != null) txtName.Text = (_current == null) ? string.Empty : _current.Name;
            if (propGrid != null) propGrid.SelectedObject = _device;     // 参数表：IP / Port / 串口号…自动列出
            RefreshState();
            ApplyMode();                                                 // 单设备模式下把设备类名写进顶行

            if (_current != null && _current.DeviceType != null)
                AppendLog(L("tag_Device", "设备", "Device", "Dispositivo"), _current.Name + "（" + _current.DeviceType.Name + "）：" + DescribeIo(_device));
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
                throw new MissingMethodException(t.Name + L("err_NoCtor", " 没有可用的公开构造函数。", " has no usable public constructor.", " no tiene constructor público utilizable."));

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
            if (d.SupportsRawIo) tx = LangMsg("DeviceControl", "io_TxOverride", "支持发送（已重写 Send）", "Send OK (Send overridden)", "Envío OK (Send sobrescrito)");
            else
            {
                MethodInfo mi = FindCandidateMethod(d, SendMethodNames, 1);
                tx = (mi != null)
                    ? (LangMsg("DeviceControl", "io_TxMethod1", "支持发送（按方法名调到 ", "Send OK (resolved by name: ", "Envío OK (método detectado: ") + mi.Name + "）")
                    : LangMsg("DeviceControl", "io_TxNone", "不支持发送（未重写 SupportsRawIo/Send，也没找到可用方法）", "No send support", "Sin soporte de envío");
            }

            string rx;
            if (d.SupportsRawIo) rx = LangMsg("DeviceControl", "io_RxOverride", "支持接收", "Receive OK", "Recepción OK");
            else
            {
                MethodInfo mi = FindCandidateMethod(d, RecvMethodNames, 0);
                rx = (mi != null)
                    ? (LangMsg("DeviceControl", "io_RxMethod1", "支持接收（按方法名调到 ", "Receive OK (resolved by name: ", "Recepción OK (método detectado: ") + mi.Name + "）")
                    : LangMsg("DeviceControl", "io_RxNone", "不支持接收", "No receive support", "Sin soporte de recepción");
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
                return L("err_NoSendAbility", "该设备类没有发送能力：既未重写 SupportsRawIo/Send，也找不到 Send/Sent/Write/… 这类公开方法。", "This device type cannot send: SupportsRawIo/Send is not overridden and no Send/Sent/Write/... method exists.", "Este tipo no puede enviar: no sobrescribe SupportsRawIo/Send ni tiene Send/Sent/Write/...");

            object r = mi.Invoke(d, new object[] { text });
            if (r is string) return (string)r;
            if (r is bool) return ((bool)r) ? string.Empty : L("err_SendReturnedFalse", "设备的发送方法返回失败。", "The device send method returned failure.", "El método de envío devolvió error.");
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

        #region 设备清单 / 状态广播（其它界面按它显示，如首页底部的设备状态条）

        /// <summary>一台设备的对外状态快照。设备名就是 Parameter 里"设备名"框里的那个，改名后跟着变。</summary>
        public sealed class DeviceStatus
        {
            /// <summary>设备名（Parameter 里可改）。</summary>
            public string Name;
            /// <summary>设备类名（如 ScannerKeyenceTcp）。</summary>
            public string TypeName;
            /// <summary>是否已连接。</summary>
            public bool IsConnected;
        }

        /// <summary>
        /// 设备清单（增加 / 删除 / 改名）或连接状态变化时广播。
        /// **静态事件 —— 订阅者在 Dispose 时必须解绑**，否则已释放的窗体/控件会被一直留住。
        /// </summary>
        public static event EventHandler DevicesChanged;

        private static int _bulkDepth;      // > 0 时先攒着，等整批改完再广播一次

        /// <summary>开始一批改动（期间不广播，避免"按 XML 重建 N 台设备"刷 N 次界面）。</summary>
        public static void BeginBulkChange() { _bulkDepth++; }

        /// <summary>结束一批改动：配对到最后一层时才广播一次。</summary>
        public static void EndBulkChange()
        {
            if (_bulkDepth > 0) _bulkDepth--;
            if (_bulkDepth == 0) RaiseDevicesChanged();
        }

        /// <summary>通知订阅者"设备清单 / 连接状态变了"。</summary>
        public static void RaiseDevicesChanged()
        {
            if (_bulkDepth > 0) return;
            EventHandler h = DevicesChanged;
            if (h == null) return;
            try { h(null, EventArgs.Empty); }
            catch (Exception) { }
        }

        /// <summary>
        /// 当前所有设备的快照，顺序 = DeviceControl 的登记顺序（也就是 Parameter 里的页签顺序）。
        /// 单设备面板各算一台；多设备控件则列出它管的全部设备。
        /// </summary>
        public static List<DeviceStatus> GetDeviceStatuses()
        {
            List<DeviceStatus> list = new List<DeviceStatus>();
            for (int i = 0; i < LiveInstances.Count; i++)
            {
                DeviceControl c = LiveInstances[i];
                if (c == null || c.IsDisposed) continue;

                for (int j = 0; j < c._devices.Count; j++)
                {
                    DeviceItem it = c._devices[j];
                    if (it == null) continue;

                    DeviceStatus s = new DeviceStatus();
                    s.Name = it.Name;
                    s.TypeName = (it.DeviceType == null) ? string.Empty : it.DeviceType.Name;
                    s.IsConnected = it.Instance != null && it.Instance.IsConnected;
                    list.Add(s);
                }
            }
            return list;
        }

        /// <summary>在 UI 线程取得现有设备对象快照，不创建、连接或重配设备。</summary>
        public static List<KeyValuePair<string, AbstractDevice>> GetFlowDesignerDevices()
        {
            var result = new List<KeyValuePair<string, AbstractDevice>>();
            foreach (var control in LiveInstances)
            {
                if (control == null || control.IsDisposed) continue;
                foreach (var device in control._devices)
                    if (device.Instance != null) result.Add(new KeyValuePair<string, AbstractDevice>(control.XmlKey + "/" + device.Name, device.Instance));
            }
            return result;
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

        /// <summary>取控件所属窗体的名字（取不到就用 "Form"）；XML 的 Key 前缀就是它。</summary>
        private static string FormNameOf(DeviceControl c)
        {
            Form f = (c == null) ? null : c.FindForm();
            return (f == null || string.IsNullOrEmpty(f.Name)) ? "Form" : f.Name;
        }

        /// <summary>XML 里的唯一键：窗体名_控件名（同一个窗体上放多个 DeviceControl 也不会撞）。</summary>
        private string XmlKey
        {
            get
            {
                string ctlName = string.IsNullOrEmpty(this.Name) ? "DeviceControl" : this.Name;
                return FormNameOf(this) + "_" + ctlName;
            }
        }

        /// <summary>
        /// 主界面【保存】选"是"时由 MainForm.SaveData() 调用：把所有 DeviceControl 的参数写入 XML。
        /// 单设备面板（宿主动态增删的那种）会先把该窗体名下的旧节点整体清掉再逐面板重写 ——
        /// 否则"删掉过的面板"会在 XML 里留下孤儿节点，下次启动又冒出来。
        /// </summary>
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

                PurgeHostedFormNodes(doc);

                for (int i = 0; i < LiveInstances.Count; i++)
                    LiveInstances[i].WriteToDoc(doc);

                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

                doc.Save(path);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// "单设备面板"的宿主窗体名（**登记制**）。
        /// 保存时会先清掉这些窗体名下的全部旧 &lt;Device&gt; 节点 —— 靠"存活实例"是判不出"最后一个面板也被删了"的，
        /// 那种情况下实例数为 0，会把已删面板的节点留在 XML 里，下次启动又冒出来。
        /// </summary>
        private static readonly List<string> HostedForms = new List<string>();

        /// <summary>宿主登记：本窗体用"单设备面板"模式（构造或显示时调一次即可）。</summary>
        public static void RegisterPanelHost(string formName)
        {
            if (string.IsNullOrEmpty(formName)) return;
            if (!HostedForms.Contains(formName)) HostedForms.Add(formName);
        }

        /// <summary>宿主注销（宿主 Dispose 时调）。</summary>
        public static void UnregisterPanelHost(string formName)
        {
            if (!string.IsNullOrEmpty(formName)) HostedForms.Remove(formName);
        }

        /// <summary>清掉"有单设备面板的窗体"名下的所有 &lt;Device&gt; 节点（含已删面板的遗留）。</summary>
        private static void PurgeHostedFormNodes(XmlDocument doc)
        {
            if (doc == null || doc.DocumentElement == null) return;

            List<string> forms = new List<string>(HostedForms);
            for (int i = 0; i < LiveInstances.Count; i++)
            {
                if (!LiveInstances[i]._singleDevice) continue;
                string f = FormNameOf(LiveInstances[i]);
                if (!forms.Contains(f)) forms.Add(f);
            }
            if (forms.Count == 0) return;

            XmlElement root = doc.DocumentElement;
            for (int i = root.ChildNodes.Count - 1; i >= 0; i--)
            {
                XmlElement e = root.ChildNodes[i] as XmlElement;
                if (e == null || e.Name != "Device") continue;

                string key = e.GetAttribute("Key");
                for (int k = 0; k < forms.Count; k++)
                {
                    if (key.StartsWith(forms[k] + "_", StringComparison.OrdinalIgnoreCase))
                    {
                        root.RemoveChild(e);
                        break;
                    }
                }
            }
        }

        /// <summary>宿主收到它就该按 XML 重建面板集合（"选否"回滚时也要把面板集合一起回滚）。</summary>
        public static event EventHandler ReloadPanelsRequested;

        /// <summary>主界面【保存】选"否"时由 MainForm.SaveData() 调用：把参数还原成 XML 里上次保存的值。</summary>
        public static void ReloadAll()
        {
            // 先让宿主按 XML 重建面板（"加了没保存/删了没保存"的面板集合本身也要回滚）
            EventHandler h = ReloadPanelsRequested;
            if (h != null)
            {
                try { h(null, EventArgs.Empty); }
                catch (Exception) { }
            }

            for (int i = 0; i < LiveInstances.Count; i++)
                LiveInstances[i].LoadFromXml();
        }

        /// <summary>
        /// 列出 XML 里属于指定窗体的面板控件名（**按 XML 文档顺序** —— 也就是面板的排列顺序）。
        /// 宿主用它决定"启动时该建几个面板、每个叫什么名字"。
        /// </summary>
        public static List<string> ReadFormPanelNames(string formName)
        {
            List<string> names = new List<string>();
            if (string.IsNullOrEmpty(formName)) return names;

            try
            {
                string path = XmlFilePath;
                if (!File.Exists(path)) return names;

                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                if (doc.DocumentElement == null) return names;

                string prefix = formName + "_";
                for (int i = 0; i < doc.DocumentElement.ChildNodes.Count; i++)
                {
                    XmlElement e = doc.DocumentElement.ChildNodes[i] as XmlElement;
                    if (e == null || e.Name != "Device") continue;

                    string key = e.GetAttribute("Key");
                    if (!key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;

                    string ctl = key.Substring(prefix.Length);
                    if (ctl.Length > 0) names.Add(ctl);
                }
            }
            catch (Exception) { }
            return names;
        }

        /// <summary>
        /// 把本控件管理的**所有设备**写进 XML（每台一条 &lt;Item Name="…" Type="…"&gt;）。
        /// 旧格式（Device 直挂参数 / 按设备类分组的 &lt;Type&gt;）会在保存时统一转成新结构。
        /// </summary>
        private void WriteToDoc(XmlDocument doc)
        {
            if (doc == null || doc.DocumentElement == null) return;

            XmlElement node = FindOrCreateDeviceNode(doc, XmlKey);

            for (int i = node.ChildNodes.Count - 1; i >= 0; i--) node.RemoveChild(node.ChildNodes[i]);   // 旧结构整体重建
            node.RemoveAttribute("Type");
            node.RemoveAttribute("LastType");
            node.RemoveAttribute("DeviceName");
            node.SetAttribute("Current", (_current == null) ? string.Empty : _current.Name);             // 记住上次选中的设备

            for (int k = 0; k < _devices.Count; k++)
            {
                DeviceItem it = _devices[k];

                XmlElement itemNode = doc.CreateElement("Item");
                itemNode.SetAttribute("Name", it.Name);
                itemNode.SetAttribute("Type", it.DeviceType.Name);

                foreach (PropertyInfo p in WritableProperties(it.DeviceType))
                {
                    try
                    {
                        object v = p.GetValue(it.Instance, null);
                        XmlElement e = doc.CreateElement(p.Name);
                        e.InnerText = ToInvariantString(v, p.PropertyType);
                        itemNode.AppendChild(e);
                    }
                    catch (Exception) { }
                }
                node.AppendChild(itemNode);
            }
        }

        /// <summary>
        /// 从 XML 重建本控件的**设备列表**（离线保存的核心：整个过程不依赖设备在线）。
        /// 【保存】选"否"回滚时也走这里 —— 会把"添加过但没保存"的设备丢掉、"删掉的"设备找回来。
        /// 兼容两种旧格式：① &lt;Device Type="X"&gt; 参数直挂；② &lt;Type Name="X"&gt; 按设备类分组。
        /// </summary>
        public void LoadFromXml()
        {
            try
            {
                CloseSilently();
                _devices.Clear();
                _current = null;
                _device = null;
                _pendingCurrent = null;

                if (_types.Count == 0) BuildTypeList();     // 万一还没扫过设备类

                string path = XmlFilePath;
                if (File.Exists(path))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(path);
                    if (doc.DocumentElement != null)
                    {
                        XmlElement node = FindDeviceNode(doc, XmlKey);
                        if (node != null) LoadDevicesFrom(node);
                    }
                }

                string want = _pendingCurrent;
                if (FindDevice(want) == null) want = (_devices.Count > 0) ? _devices[0].Name : null;

                RebuildDeviceList(want);
                SwitchToDevice(FindDevice(want));

                if (_devices.Count > 0)
                    AppendLog(L("tag_Param", "参数", "Param", "Parámetro"),
                    L("msg_Loaded", "已从 ", "Loaded ", "Cargado ") + Path.GetFileName(path)
                    + L("msg_LoadedN", " 载入 ", " - ", " - ") + _devices.Count
                    + L("msg_LoadedEnd", " 台设备", " device(s)", " dispositivo(s)"));
            }
            catch (Exception) { }
        }

        /// <summary>从一个 &lt;Device&gt; 节点读出设备列表（新格式优先，其次两种旧格式）。</summary>
        private void LoadDevicesFrom(XmlElement node)
        {
            _pendingCurrent = node.GetAttribute("Current");
            string savedName = node.GetAttribute("DeviceName");     // 旧格式里的设备名
            string lastType = node.GetAttribute("LastType");        // 旧格式：上次用的设备类
            string legacyType = node.GetAttribute("Type");          // 更旧的格式：类型写在 Device 属性上

            // ① 新格式：每台设备一条 <Item Name="…" Type="…">
            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                XmlElement e = node.ChildNodes[i] as XmlElement;
                if (e == null || e.Name != "Item") continue;
                AddDeviceFromXml(e.GetAttribute("Type"), e.GetAttribute("Name"), e);
            }
            if (_devices.Count > 0) return;

            // ② 旧格式 A：<Type Name="X"> 按设备类分组（每类一条记录）
            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                XmlElement e = node.ChildNodes[i] as XmlElement;
                if (e == null || e.Name != "Type") continue;

                string tn = e.GetAttribute("Name");
                bool isLast = string.Equals(tn, lastType, StringComparison.OrdinalIgnoreCase);
                string nm = (isLast && !string.IsNullOrEmpty(savedName)) ? savedName : tn;
                AddDeviceFromXml(tn, nm, e);
                if (isLast) _pendingCurrent = nm;
            }
            if (_devices.Count > 0) return;

            // ③ 旧格式 B：<Device Type="X"> 参数直接挂在 Device 下
            if (!string.IsNullOrEmpty(legacyType))
                AddDeviceFromXml(legacyType,
                    string.IsNullOrEmpty(savedName) ? legacyType : savedName, node);
        }

        /// <summary>读完 XML 后由调用方（或自身的 Load）通知一次"设备清单变了"。</summary>
        public void NotifyLoaded()
        {
            RaiseDevicesChanged();
        }

        /// <summary>按类型名找到设备类 → 建实例 → 套 XML 里的参数 → 加入设备列表。</summary>
        private void AddDeviceFromXml(string typeName, string name, XmlElement paramNode)
        {
            if (string.IsNullOrEmpty(typeName)) return;

            Type t = null;
            for (int i = 0; i < _types.Count; i++)
                if (string.Equals(_types[i].Name, typeName, StringComparison.OrdinalIgnoreCase)) { t = _types[i]; break; }
            if (t == null) return;                                   // 类被删/改名了 ⇒ 跳过这条（其余照常载入）

            if (string.IsNullOrEmpty(name)) name = AutoDeviceName();
            if (FindDevice(name) != null) name = AutoDeviceName();    // 重名就去重

            AbstractDevice inst;
            try { inst = NewDevice(t, name); }
            catch (Exception) { return; }

            if (paramNode != null)
            {
                for (int i = 0; i < paramNode.ChildNodes.Count; i++)
                {
                    XmlNode c = paramNode.ChildNodes[i];
                    if (c is XmlElement && (((XmlElement)c).Name == "Item" || ((XmlElement)c).Name == "Type")) continue;
                    ApplyOne(inst, c.Name, c.InnerText);
                }
            }
            else { }

            DeviceItem item = new DeviceItem();
            item.Name = name;
            item.DeviceType = t;
            item.Instance = inst;
            _devices.Add(item);
        }

        /// <summary>取本控件在 XML 里的 &lt;Device&gt; 节点（找不到返回 null）。</summary>
        private static XmlElement FindDeviceNode(XmlDocument doc, string key)
        {
            if (doc == null || doc.DocumentElement == null) return null;
            for (int i = 0; i < doc.DocumentElement.ChildNodes.Count; i++)
            {
                XmlElement e = doc.DocumentElement.ChildNodes[i] as XmlElement;
                if (e != null && e.Name == "Device" && e.GetAttribute("Key") == key) return e;
            }
            return null;
        }

        /// <summary>取本控件节点下某个设备类的参数分组 &lt;Type Name="..."&gt;（找不到返回 null）。</summary>
        private static XmlElement FindTypeNode(XmlElement deviceNode, string typeName)
        {
            if (deviceNode == null || string.IsNullOrEmpty(typeName)) return null;
            for (int i = 0; i < deviceNode.ChildNodes.Count; i++)
            {
                XmlElement e = deviceNode.ChildNodes[i] as XmlElement;
                if (e != null && e.Name == "Type"
                    && string.Equals(e.GetAttribute("Name"), typeName, StringComparison.OrdinalIgnoreCase)) return e;
            }
            return null;
        }

        /// <summary>取本控件的 &lt;Device&gt; 节点，没有就建一个。</summary>
        private static XmlElement FindOrCreateDeviceNode(XmlDocument doc, string key)
        {
            XmlElement node = FindDeviceNode(doc, key);
            if (node == null)
            {
                node = doc.CreateElement("Device");
                node.SetAttribute("Key", key);
                doc.DocumentElement.AppendChild(node);
            }
            return node;
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

        /// <summary>
        /// 按 <see cref="SingleDevice"/> 切换顶行形态：
        /// · 多设备模式：设备[下拉] [添加][删除] … 设备名[框]
        /// · 单设备模式：**设备类名** … 设备名[框] … [移除]
        /// </summary>
        /// <summary>语言切换回调；Dispose 时解绑。</summary>
        private EventHandler _langHandler;

        /// <summary>本控件固定文案所在的语言包分组。</summary>
        private const string LangGroup = "DeviceControl";

        /// <summary>取当前语言的文案（走语言包，取不到回落中文）。</summary>
        private static string L(string key, string zh, string en, string es = null)
        {
            return LangMsg(LangGroup, key, zh, en, es);
        }

        /// <summary>
        /// 按当前语言刷新本控件的固定文案（按钮 / 标签 / 状态行）。
        /// </summary>
        internal void ApplyLanguage()
        {
            if (lblName != null) lblName.Text = L("lblName", "设备名", "Device name", "Nombre");
            if (lblParam != null) lblParam.Text = L("lblParam",
                "连接参数（按所选设备类自动列出，可直接编辑）",
                "Connection parameters (auto-listed by device type, editable)",
                "Parámetros de conexión (según el tipo, editables)");
            if (btnAddDev != null) btnAddDev.Text = L("btnAdd", "添加", "Add", "Agregar");
            if (btnDelDev != null) btnDelDev.Text = L("btnDel", "删除", "Remove", "Eliminar");
            if (btnRemovePanel != null) btnRemovePanel.Text = L("btnRemove", "移除", "Remove", "Quitar");
            if (btnOpen != null) btnOpen.Text = L("btnOpen", "打开连接", "Open", "Abrir");
            if (btnClose != null) btnClose.Text = L("btnClose", "关闭连接", "Close", "Cerrar");
            if (btnRecv != null) btnRecv.Text = L("btnRecv", "接收一次", "Receive", "Recibir");
            if (btnSend != null) btnSend.Text = L("btnSend", "发送", "Send", "Enviar");
            if (btnClear != null) btnClear.Text = L("btnClear", "清空记录", "Clear log", "Borrar registro");

            ApplyMode();      // 单设备模式下 lblType 写的是设备类名，这里重刷一次
            RefreshState();   // 状态行文字也跟着语言
        }

        private void ApplyMode()
        {
            if (cboType == null || lblType == null || btnAddDev == null ||
                btnDelDev == null || btnRemovePanel == null) return;

            bool single = _singleDevice;

            cboType.Visible = !single;
            btnAddDev.Visible = !single;
            btnDelDev.Visible = !single;
            btnRemovePanel.Visible = single;

            if (single)
            {
                lblType.Width = 300;
                lblType.Text = string.IsNullOrEmpty(BoundTypeName) ? L("lblType", "设备", "Device", "Dispositivo") : BoundTypeName;
            }
            else
            {
                lblType.Width = 62;
                lblType.Text = L("lblType", "设备", "Device", "Dispositivo");
            }
        }

        /// <summary>
        /// 空间紧张时压缩"连接参数"表，保证底部的【发送】行和记录框始终可见。
        /// 这几块都是固定高的 Dock 行，一旦叠加超过控件高度，最后停靠的那一栏会被压成 0 高
        /// —— 宿主把控件 Dock=Fill 到一个不够高的容器时，就会看不到【发送】。
        /// </summary>
        private void FitParamPanel()
        {
            if (_fitting) return;
            if (panelTop == null || panelMid == null || panelParam == null ||
                panelSend == null || txtRecv == null) return;

            _fitting = true;
            try
            {
                const int ParamWant = 168;   // 参数表设计高度
                const int ParamMin = 66;     // 至少要露几行
                const int SendH = 36;        // 【发送】行设计高度
                const int LogMin = 56;       // 记录框至少留这么高

                int room = ClientSize.Height - panelTop.Height - panelMid.Height - SendH - LogMin;
                int want = ParamWant;
                if (room < want) want = Math.Max(ParamMin, room);

                if (want > 0 && panelParam.Height != want) panelParam.Height = want;
            }
            finally { _fitting = false; }
        }

        /// <summary>防止 FitParamPanel 里改高度又触发 OnResize 造成递归。</summary>
        private bool _fitting;

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            FitParamPanel();     // 宿主把控件压矮时，让【发送】行与记录框不被挤掉
        }

        private void DeviceControl_Load(object sender, EventArgs e)
        {
            // 单设备模式：面板由宿主（Parameter）负责建好、绑定设备、再按 XML 套参数。
            // 这里若自作主张 LoadFromXml，会按"窗体名_控件名"去 XML 找节点，
            // 而此刻宿主可能还没给面板命名/绑定设备类 —— 所以交给宿主决定何时载入。
            if (!_singleDevice)
            {
                LoadFromXml();      // 离线参数：按 XML 上次保存的值套到设备实例上（此时才取得到窗体名做键）
                RaiseDevicesChanged();
            }

            if (AutoOpen)
            {
                string err = OpenDevice();
                if (!string.IsNullOrEmpty(err)) AppendLog(L("tag_Tip", "提示", "Info", "Aviso"), err);
            }
        }

        /// <summary>下拉框里选了另一台设备。</summary>
        private void cboType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loading) return;                                // 列表刷新引起的变化，不算用户选择
            if (InDesigner) return;                              // 设计期不碰实例

            DeviceItem item = cboType.SelectedItem as DeviceItem;
            if (item == _current) return;                        // 没变

            SwitchToDevice(item);
        }

        /// <summary>改设备名（只改条目名；设备实例不动，所以不会丢参数）。</summary>
        private void txtName_Leave(object sender, EventArgs e)
        {
            if (InDesigner || _current == null) return;

            string want = (txtName.Text ?? string.Empty).Trim();
            if (want.Length == 0) { txtName.Text = _current.Name; return; }              // 空名不接受
            if (string.Equals(want, _current.Name, StringComparison.Ordinal)) return;

            if (FindDevice(want) != null)                                               // 不能重名
            {
                AppendLog(L("tag_Tip", "提示", "Info", "Aviso"), L("msg_DupName1", "已有同名设备 ", "Name already used: ", "Nombre ya usado: ") + want + L("msg_DupName2", "，改回原名。", " - reverted.", " - restaurado."));
                txtName.Text = _current.Name;
                return;
            }

            string old = _current.Name;
            _current.Name = want;
            RebuildDeviceList(want);                        // 刷新下拉框显示
            RaiseNameChanged();                             // 让宿主同步页签标题
            RaiseDevicesChanged();                          // 首页底部的设备名跟着改
            AppendLog(L("tag_Rename", "改名", "Rename", "Renombrar"),
                    L("msg_DeviceW", "设备 ", "Device ", "Dispositivo ") + old + " -> " + want
                    + L("msg_SaveToConfig", "（点主界面【保存】后写入配置）", " (written to config on Save)", " (se guarda al pulsar Guardar)"));
        }

        /// <summary>添加一台设备：先选设备类，再自动起名（名字可在"设备名"框里改）。</summary>
        private void btnAddDev_Click(object sender, EventArgs e)
        {
            Type t = PickDeviceType();
            if (t == null) return;

            string name = AutoDeviceName();
            AbstractDevice inst;
            try { inst = NewDevice(t, name); }
            catch (Exception ex)
            {
                AppendLog(L("tag_Error", "错误", "Error", "Error"), L("msg_CreateFail", "创建 ", "Create ", "Crear ") + t.Name + L("msg_Failed", " 失败：", " failed: ", " falló: ") + ex.Message);
                return;
            }

            DeviceItem item = new DeviceItem();
            item.Name = name;
            item.DeviceType = t;
            item.Instance = inst;
            _devices.Add(item);

            RebuildDeviceList(name);
            SwitchToDevice(item);
            RaiseDevicesChanged();
            AppendLog(L("tag_Add", "添加", "Add", "Agregar"),
                    L("msg_Added1", "已添加设备 ", "Added ", "Agregado ") + name + " (" + t.Name + ")"
                    + L("msg_Added2", " —— 点主界面【保存】选\"是\"才会写入配置文件", " - click Save to persist it", " - pulse Guardar para guardarlo"));
        }

        /// <summary>删除当前设备（它的参数会在下次【保存】时从配置文件里移除）。</summary>
        private void btnDelDev_Click(object sender, EventArgs e)
        {
            if (_current == null) { AppendLog(L("tag_Tip", "提示", "Info", "Aviso"), L("msg_NoDeletable", "还没有可删除的设备。", "No device to remove.", "No hay dispositivo que eliminar.")); return; }

            string what = _current.Name + "（" + _current.DeviceType.Name + "）";
            DialogResult r = MessageBox.Show(
                string.Format(L("msg_DeleteConfirm", "确定删除设备 {0} 吗？\r\n它的参数会在下次保存时从配置文件里移除。",
                    "Delete device {0}?\r\nIts parameters will be removed from the configuration the next time you save.",
                    "¿Eliminar el dispositivo {0}?\r\nSus parámetros se quitarán de la configuración la próxima vez que guarde."), what),
                L("msg_DeleteTitle", "删除设备", "Delete device", "Eliminar dispositivo"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (r != DialogResult.Yes) return;

            CloseSilently();
            _devices.Remove(_current);
            _current = null;
            _device = null;

            DeviceItem next = (_devices.Count > 0) ? _devices[0] : null;
            RebuildDeviceList(next == null ? null : next.Name);
            SwitchToDevice(next);

            AppendLog(L("tag_Del", "删除", "Remove", "Eliminar"),
                    L("msg_Deleted1", "已删除设备 ", "Removed ", "Eliminado ") + what
                    + L("msg_Deleted2", " —— 点主界面【保存】后从配置文件移除", " - click Save to remove it from config", " - pulse Guardar para quitarlo del archivo"));
        }

        /// <summary>弹一个小框挑选设备类（列出本程序集里全部 AbstractDevice 具体子类）；取消返回 null。</summary>
        private Type PickDeviceType()
        {
            if (_types.Count == 0) BuildTypeList();
            if (_types.Count == 0)
            {
                AppendLog(L("tag_Tip", "提示", "Info", "Aviso"), L("msg_NoDeviceClass", "没有找到继承 AbstractDevice 的设备类。", "No AbstractDevice subclass found.", "No se encontró ninguna subclase de AbstractDevice."));
                return null;
            }

            Form f = FindForm();
            string group = (f == null || string.IsNullOrEmpty(f.Name)) ? "DeviceControl" : f.Name;
            return PickDeviceTypeDialog(f, this.Font, group, _deviceTypeName);
        }

        /// <summary>
        /// 弹小框选设备类（宿主窗体的"添加设备"按钮可以直接用）。取消返回 null。
        /// 文案走 MiddleLayer 语言包，分组名 = 调用方窗体名（取不到就用 "DeviceControl"）。
        /// </summary>
        public static Type PickDeviceTypeDialog(IWin32Window owner, Font uiFont, string langGroup, string presetTypeName)
        {
            List<Type> types = ScanDeviceTypes();
            if (types.Count == 0) return null;

            Type picked = null;
            using (Form dlg = new Form())
            {
                dlg.Name = "dlgPickDeviceType";
                dlg.Text = LangMsg(langGroup, "AddDevice", "添加设备", "Add device", "Agregar dispositivo");
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MinimizeBox = false;
                dlg.MaximizeBox = false;
                dlg.ShowInTaskbar = false;
                dlg.ClientSize = new Size(380, 124);
                if (uiFont != null) dlg.Font = uiFont;

                UiLabel lb = new UiLabel();          // UiLabel：不进语言白名单（但名字仍不能为空，见 XPathException 铁律）
                lb.Name = "lblPickType";
                lb.Text = LangMsg(langGroup, "DeviceType", "选择设备类：", "Device type:", "Tipo de dispositivo:");
                lb.Location = new Point(16, 16);
                lb.AutoSize = true;
                dlg.Controls.Add(lb);

                ComboBox cbo = new ComboBox();
                cbo.DropDownStyle = ComboBoxStyle.DropDownList;
                cbo.Location = new Point(16, 42);
                cbo.Width = 348;
                cbo.DisplayMember = "Name";
                cbo.DataSource = new List<Type>(types);
                if (!string.IsNullOrEmpty(presetTypeName))                   // 预设过就默认选中
                {
                    for (int i = 0; i < types.Count; i++)
                        if (string.Equals(types[i].Name, presetTypeName, StringComparison.OrdinalIgnoreCase))
                        { cbo.SelectedIndex = i; break; }
                }
                dlg.Controls.Add(cbo);

                UiButton ok = new UiButton();
                ok.Name = "btnPickOk";
                ok.Text = LangMsg(langGroup, "msg_OK", "确定", "OK", "Aceptar");
                ok.DialogResult = DialogResult.OK;
                ok.Location = new Point(208, 84);
                ok.Size = new Size(76, 26);
                dlg.Controls.Add(ok);

                UiButton cancel = new UiButton();
                cancel.Name = "btnPickCancel";
                cancel.Text = LangMsg(langGroup, "msg_Cancel", "取消", "Cancel", "Cancelar");
                cancel.DialogResult = DialogResult.Cancel;
                cancel.Location = new Point(288, 84);
                cancel.Size = new Size(76, 26);
                dlg.Controls.Add(cancel);

                dlg.AcceptButton = ok;
                dlg.CancelButton = cancel;

                if (dlg.ShowDialog(owner) == DialogResult.OK) picked = cbo.SelectedItem as Type;
            }
            return picked;
        }

        /// <summary>取当前语言的文案（走 MiddleLayer 语言包；取不到就回中文）。注意 LangMsg 首参是**窗体名**不是窗体对象。</summary>
        private string Msg(string key, string zh, string en, string es)
        {
            Form f = FindForm();
            string formName = (f == null || string.IsNullOrEmpty(f.Name)) ? "DeviceControl" : f.Name;
            return LangMsg(formName, key, zh, en, es);
        }

        /// <summary>取当前语言的文案（静态版；group 为窗体名）。</summary>
        private static string LangMsg(string group, string key, string zh, string en, string es)
        {
            try
            {
                if (string.IsNullOrEmpty(group)) group = "DeviceControl";
                return MiddleLayer.LangMsg(group, key, zh, en, es);
            }
            catch (Exception) { return zh; }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            string err = OpenDevice();
            if (!string.IsNullOrEmpty(err)) AppendLog(L("tag_Tip", "提示", "Info", "Aviso"), err);
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
            if (!string.IsNullOrEmpty(err)) AppendLog(L("tag_Tip", "提示", "Info", "Aviso"), err);
        }

        /// <summary>清空下面的记录框。</summary>
        private void btnClear_Click(object sender, EventArgs e)
        {
            txtRecv.Clear();
            _logLines = 0;
        }

        /// <summary>单设备模式：点【移除】——只是通知宿主，真正的移除（含 Dispose）由宿主做。</summary>
        private void btnRemovePanel_Click(object sender, EventArgs e)
        {
            EventHandler h = RemoveRequested;
            if (h != null) h(this, EventArgs.Empty);
        }

        /// <summary>
        /// 参数表改过值就提示"要保存"——本控件的参数不会自动落盘，
        /// 必须点主界面左侧【保存】并在确认框选"是"（见 MainForm.SaveData → DeviceControl.SaveAll）。
        /// </summary>
        private void propGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (InDesigner || _device == null) return;

            string label = (e == null || e.ChangedItem == null) ? "?" : e.ChangedItem.Label;
            string val = (e == null || e.ChangedItem == null) ? string.Empty : Convert.ToString(e.ChangedItem.Value);

            AppendLog(L("tag_Param", "参数", "Param", "Parámetro"),
                    L("msg_Modified1", "已修改 ", "Changed ", "Modificado ") + label + " = " + val
                    + L("msg_Modified2", "（未保存）", " (not saved)", " (sin guardar)")
                + L("msg_SaveHint", " —— 点主界面【保存】选\"是\"才会写入 ", " - click Save (choose Yes) to write ", " - pulse Guardar (Sí) para escribir ") + Path.GetFileName(XmlFilePath));
        }

        /// <summary>刷新状态行（设备名 / 类名 / 连没连）。</summary>
        private void RefreshState()
        {
            bool connected = _device != null && _device.IsConnected;

            if (_current == null)
            {
                lblState.Text = (_devices.Count == 0)
                    ? L("state_NoDevice", "尚未添加设备，点【添加】", "No device yet - click Add", "Sin dispositivo - pulse Agregar")
                    : L("state_NoSelect", "未选择设备", "No device selected", "Ningún dispositivo");
                lblState.ForeColor = UiKit.TextMuted;
            }
            else
            {
                lblState.Text = _current.Name + "（" + _current.DeviceType.Name + "）："
                              + (connected
                                 ? L("state_Connected", "已连接", "Connected", "Conectado")
                                 : L("state_Disconnected", "未连接", "Not connected", "Sin conexión"));
                lblState.ForeColor = connected ? UiKit.Success : UiKit.Danger;
            }

            // 收发必须在连接状态下才能触发（未连接时按钮置灰、点击不响应）
            if (btnSend != null) btnSend.Enabled = connected;
            if (btnRecv != null) btnRecv.Enabled = connected;

            EventHandler h = ConnectedChanged;
            if (h != null) h(this, EventArgs.Empty);

            RaiseDevicesChanged();      // 连接状态变了 -> 通知首页等订阅者
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
