using Alpha;
using AlphaRap.Classes;
using AlphaRapLibrary;
using Cognex.VisionPro;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace AlphaRap
{
    public partial class MainForm : ModuleBaseForm
    {
        #region test
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
(
int nleftRect,
int nTopRect,
int nRightRect,
int nBottomRect,
int nwidthEllipse,
int nheightEllipse
);

        [DllImport("Gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

#endregion

        [System.Runtime.InteropServices.DllImport("user32.dll ")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int wndproc);
        [System.Runtime.InteropServices.DllImport("user32.dll ")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        public const int GWL_STYLE = -16;
        public const int WS_DISABLED = 0x8000000;
        private MENU_PageType1 MENU_SelectPage1 = MENU_PageType1.Home;
        public enum MENU_PageType1
        {
            Save,
            Login,
            Reset,
            Run,
            Pause,
            Stop,
            Home,
            Product,
            Hard,
            Manual,
            Check,
            System,
            AddUser,
            Mes,
            Rapid,
            Data,
            Vision,
            Exit,
            Robot,
            LifeSpan,
            Log
        }

        public MainForm()
        {
            InitializeComponent();

            // 主框架样式（设计期也执行，使设计视图与运行时一致）；按钮文字取自语言包
            if (!InDesigner)
                UiTheme.TextProvider = (form, key, zh, en, es) => MiddleLayer.LangMsg(form, key, zh, en, es);
            ApplyFrameStyle();

            // 以下设置圆角区域和鼠标转发，仅在运行时执行
            if (InDesigner) return;

            UpdateWindowRegion();

            // 把整个控件树的鼠标事件转发到主窗体，这样无论按在哪个子控件上都能拖动/缩放
            HookMouseForwarding(this);

            // 界面操作日志：点任何按钮 / 勾选 / 图标 / 页签都记一条（工号 / 权限 / 时间 / 操作内容）
            OperationLog.InstallFilter();
        }

        /// <summary>是否运行在 VS 的 WinForms 设计器中（设计期跳过运行期逻辑）。</summary>
        private static bool InDesigner
        {
            get
            {
                try { return System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime; }
                catch { return false; }
            }
        }

        /// <summary>已挂接鼠标事件转发的控件集合，用于去重，避免子页面反复切换时重复订阅。</summary>
        private readonly System.Collections.Generic.HashSet<Control> _mouseForwarded = new System.Collections.Generic.HashSet<Control>();

        /// <summary>
        /// 递归给所有子控件挂接 MouseDown/MouseMove/MouseUp，统一转发到主窗体的处理逻辑。
        /// 交互控件（按钮等）按下时照常点击、但会取消拖动，避免误拖。
        /// </summary>
        private void HookMouseForwarding(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                if (_mouseForwarded.Add(c))
                {
                    c.MouseDown += Child_MouseDown;
                    c.MouseMove += Child_MouseMove;
                    c.MouseUp += Child_MouseUp;
                    c.MouseDoubleClick += Child_MouseDoubleClick;
                }
                if (c.Controls.Count > 0)
                    HookMouseForwarding(c);
            }
        }

        private void Child_MouseDown(object sender, MouseEventArgs e)
        {
            // 交互控件上按下：不启动拖动/缩放，交给控件自己处理点击
            if (sender is Control c && IsInteractiveControl(c))
            {
                _isDragging = false;
                _isResizing = false;
                return;
            }

            if (e.Button != MouseButtons.Left) return;

            // 把子控件坐标换算成窗体客户区坐标
            Control src = sender as Control;
            Point clientPoint = (src != null) ? PointToClient(src.PointToScreen(e.Location)) : e.Location;

            ResizeDirection dir = HitResizeEdge(clientPoint);
            if (dir != ResizeDirection.None)
            {
                _isResizing = true;
                _resizeDir = dir;
                _resizeStartBounds = Bounds;
                _resizeStartPoint = Control.MousePosition;
                return;
            }

            if (IsInTitleBarStrip(clientPoint) && IsTitleBarArea(clientPoint))
            {
                _isDragging = true;
                _dragOffset = new Point(Control.MousePosition.X - Location.X, Control.MousePosition.Y - Location.Y);

                DateTime now = DateTime.Now;
                if ((now - _lastTitleClickTime).TotalMilliseconds <= DoubleClickInterval)
                {
                    _isDragging = false;
                    _lastTitleClickTime = DateTime.MinValue;
                    ToggleMaximize();
                    return;
                }
                _lastTitleClickTime = now;
            }
        }

        private void Child_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender is Control c && IsInteractiveControl(c))
            {
                _isDragging = false;
                _isResizing = false;
                return;
            }

            // 坐标换算成窗体客户区坐标后复用移动逻辑
            Control src = sender as Control;
            Point clientPoint = (src != null) ? PointToClient(src.PointToScreen(e.Location)) : e.Location;
            HandleMouseMove(new MouseEventArgs(e.Button, e.Clicks, clientPoint.X, clientPoint.Y, e.Delta));
        }

        private void Child_MouseUp(object sender, MouseEventArgs e)
        {
            _isDragging = false;
            _isResizing = false;
            _resizeDir = ResizeDirection.None;
        }

        private void Child_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (sender is Control c && IsInteractiveControl(c)) return;

            Control src = sender as Control;
            Point clientPoint = (src != null) ? PointToClient(src.PointToScreen(e.Location)) : e.Location;
            if (IsInTitleBarStrip(clientPoint) && IsTitleBarArea(clientPoint))
            {
                _isDragging = false;
                ToggleMaximize();
            }
        }

        /// <summary>抽出的移动处理逻辑，供窗体自身与子控件转发共用。</summary>
        private void HandleMouseMove(MouseEventArgs e)
        {
            if (_isResizing)
            {
                int dx = Control.MousePosition.X - _resizeStartPoint.X;
                int dy = Control.MousePosition.Y - _resizeStartPoint.Y;

                Rectangle b = _resizeStartBounds;

                if (_resizeDir == ResizeDirection.Left || _resizeDir == ResizeDirection.TopLeft || _resizeDir == ResizeDirection.BottomLeft)
                {
                    b.X += dx;
                    b.Width -= dx;
                }
                if (_resizeDir == ResizeDirection.Right || _resizeDir == ResizeDirection.TopRight || _resizeDir == ResizeDirection.BottomRight)
                {
                    b.Width += dx;
                }
                if (_resizeDir == ResizeDirection.Top || _resizeDir == ResizeDirection.TopLeft || _resizeDir == ResizeDirection.TopRight)
                {
                    b.Y += dy;
                    b.Height -= dy;
                }
                if (_resizeDir == ResizeDirection.Bottom || _resizeDir == ResizeDirection.BottomLeft || _resizeDir == ResizeDirection.BottomRight)
                {
                    b.Height += dy;
                }

                if (b.Width < MinimumSize.Width)
                {
                    if (_resizeDir == ResizeDirection.Left || _resizeDir == ResizeDirection.TopLeft || _resizeDir == ResizeDirection.BottomLeft)
                        b.X -= (MinimumSize.Width - b.Width);
                    b.Width = MinimumSize.Width;
                }
                if (b.Height < MinimumSize.Height)
                {
                    if (_resizeDir == ResizeDirection.Top || _resizeDir == ResizeDirection.TopLeft || _resizeDir == ResizeDirection.TopRight)
                        b.Y -= (MinimumSize.Height - b.Height);
                    b.Height = MinimumSize.Height;
                }

                Bounds = b;
                return;
            }

            if (_isDragging)
            {
                Location = new Point(
                    Control.MousePosition.X - _dragOffset.X,
                    Control.MousePosition.Y - _dragOffset.Y);
                return;
            }

            ResizeDirection hoverDir = HitResizeEdge(e.Location);
            switch (hoverDir)
            {
                case ResizeDirection.Left:
                case ResizeDirection.Right: Cursor = Cursors.SizeWE; break;
                case ResizeDirection.Top:
                case ResizeDirection.Bottom: Cursor = Cursors.SizeNS; break;
                case ResizeDirection.TopLeft:
                case ResizeDirection.BottomRight: Cursor = Cursors.SizeNWSE; break;
                case ResizeDirection.TopRight:
                case ResizeDirection.BottomLeft: Cursor = Cursors.SizeNESW; break;
                default: Cursor = Cursors.Default; break;
            }
        }
        public MouseHook mh;
        public KeyboardHook k_hook;

        [System.Runtime.InteropServices.DllImport("uxtheme.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        private void MainForm_Load(object sender, EventArgs e)
        {
            ApplyInitialWindowSize();
            ApplyRoundedMenuRegions();

            // 顶栏机器状态徽章：圆角胶囊，尺寸变化时重算
            MachineStatus.Resize += (s, ev) => ApplyPillRegion(MachineStatus, 12);
            ApplyPillRegion(MachineStatus, 12);

            // 列表控件使用 Explorer 视觉主题（扁平表头、无 3D 边框）
            try
            {
                SetWindowTheme(WarnningMessage.Handle, "Explorer", null);
            }
            catch { }

            // 报警日志筛选条（全部 / 警告 / 报警）：设置图标、事件和语言
            InitAlarmFilterBar();

            // 顶栏语言切换器（地球图标 + 语言代码）
            InitLanguageSwitch();

            // 顶栏虚拟键盘开关：点击调出或收起软键盘
            InitVirtualKeyboardToggle();

            // 绘制全部按钮的矢量图标（timer1_Tick 按 Enabled 状态持续刷新）
            RefreshButtonIcons();

            // 底部工具栏按钮的悬停效果
            HookToolbarHover();

            // 顶栏语言下拉菜单的点击事件
            try
            {
                NumC2.Click += NumC2_Click;   // Chinese
                NumC3.Click += NumC3_Click;   // English
                NumC4.Click += NumC4_Click;   // Español
            }
            catch { }

            // 状态栏显示程序集版本号和编译时间
            ApplyVersionInfo();

            SysPara.UserName = MiddleLayer.AddF.ReadAllUserData();
            SysPara.UserPermission = PermissionType.Operator;
            SwitchPermission(SysPara.UserPermission);
            RefreshMenuBackcolor();
            LoginOutTime.Enabled = false;

            SwitchMainPage(MENU_PageType.Manual);
            SwitchMainPage(MENU_PageType.Rapid);
            SwitchMainPage(MENU_PageType.Home);

            MiddleLayer.OpenRecipe(SysPara.FilePath);

            // 全局鼠标钩子（用于无操作自动登出计时）
            mh = new MouseHook();
            mh.SetHook();
            mh.MouseDownEvent += mh_MouseDownEvent;
            mh.MouseUpEvent += mh_MouseUpEvent;
            mh.MouseMoveEvent += mh_MouseMoveEvent;

            // 全局键盘钩子（用于无操作自动登出计时）
            k_hook = new KeyboardHook();
            k_hook.KeyDownEvent += new KeyEventHandler(hook_KeyDown);
            k_hook.Start();

            MiddleLayer.alarmRunTask.AlarmTaskIsRun = true;

            // 按最终确定的语言刷新报警工具条和语言切换器
            SyncLanguageTexts();
        }

        #region 无边框窗口：初始尺寸 / 拖拽缩放 / 拖动移动（手动实现，不依赖系统窗口样式）

        // 无边框窗口（FormBorderStyle.None）的拖动移动、边缘缩放和双击顶栏最大化/还原：
        // 在 MouseDown/MouseMove/MouseUp 中直接修改 Location/Size 实现。

        private enum ResizeDirection
        {
            None,
            Left, Right, Top, Bottom,
            TopLeft, TopRight, BottomLeft, BottomRight
        }

        private const int InitialWindowWidth = 1440;
        private const int InitialWindowHeight = 900;

        /// <summary>窗口四周可拖拽缩放的感应厚度（物理像素，进程为 System DPI Aware）</summary>
        private const int ResizeGrip = 10;

        /// <summary>窗口圆角半径</summary>
        private const int WindowCornerRadius = 25;

        /// <summary>双击判定间隔（毫秒）</summary>
        private const int DoubleClickInterval = 400;

        // 拖动/缩放状态
        private bool _isDragging = false;          // 正在拖动移动窗口
        private bool _isResizing = false;          // 正在拖拽缩放
        private ResizeDirection _resizeDir = ResizeDirection.None;
        private Point _dragOffset;                 // 拖动时：鼠标屏幕坐标相对窗口左上角的偏移（MousePosition - Location）
        private Rectangle _resizeStartBounds;      // 缩放时：缩放前的窗口位置与大小
        private Point _resizeStartPoint;           // 缩放时：鼠标按下的屏幕坐标

        // 双击判定
        private DateTime _lastTitleClickTime = DateTime.MinValue;

        /// <summary>状态栏显示程序集版本号（AssemblyInfo.cs）和 exe 的编译时间。</summary>
        private void ApplyVersionInfo()
        {
            try
            {
                Version v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                toolStripStatusLabel4.Text = "Version " + v.Major + "." + v.Minor + "." + v.Build;

                DateTime built = File.GetLastWriteTime(Application.ExecutablePath);
                toolStripStatusLabel3.Text = " AlphaRap-SRM  Build " + built.ToString("yyyy/MM/dd HH:mm");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ApplyVersionInfo: " + ex.Message);
            }
        }

        /// <summary>
        /// 按 1440 x 900 初始化窗口，并保证不超出当前屏幕工作区、居中显示。
        /// </summary>
        private void ApplyInitialWindowSize()
        {
            Rectangle workArea = Screen.FromControl(this).WorkingArea;

            int targetWidth = Math.Min(InitialWindowWidth, workArea.Width);
            int targetHeight = Math.Min(InitialWindowHeight, workArea.Height);

            StartPosition = FormStartPosition.Manual;
            Size = new Size(targetWidth, targetHeight);
            Location = new Point(
                workArea.Left + (workArea.Width - Width) / 2,
                workArea.Top + (workArea.Height - Height) / 2);

            UpdateWindowRegion();
        }

        /// <summary>
        /// 按当前窗口尺寸重建圆角区域。窗口尺寸变化后必须同步，否则内容会被旧圆角裁掉。
        /// </summary>
        private void UpdateWindowRegion()
        {
            if (Width <= 0 || Height <= 0) return;
            if (WindowState == FormWindowState.Minimized) return;
            if (WindowState == FormWindowState.Maximized)
            {
                // 最大化时用矩形区域（四角不裁），避免圆角切掉全屏内容
                Region = null;
                return;
            }

            IntPtr hRgn = CreateRoundRectRgn(0, 0, Width + 1, Height + 1, WindowCornerRadius, WindowCornerRadius);
            if (hRgn == IntPtr.Zero) return;

            System.Drawing.Region oldRegion = Region;
            Region = System.Drawing.Region.FromHrgn(hRgn);
            DeleteObject(hRgn);
            if (oldRegion != null) oldRegion.Dispose();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (InDesigner) return;
            UpdateWindowRegion();
        }

        /// <summary>
        /// 判断屏幕坐标落在窗口的哪个边缘（用于缩放），并返回对应方向。
        /// </summary>
        private ResizeDirection HitResizeEdge(Point clientPoint)
        {
            if (WindowState == FormWindowState.Maximized) return ResizeDirection.None;

            int w = ClientSize.Width;
            int h = ClientSize.Height;
            int grip = ResizeGrip;

            if (clientPoint.X < 0 || clientPoint.Y < 0 || clientPoint.X >= w || clientPoint.Y >= h)
                return ResizeDirection.None;

            bool left = clientPoint.X <= grip;
            bool right = clientPoint.X >= w - grip;
            bool top = clientPoint.Y <= grip;
            bool bottom = clientPoint.Y >= h - grip;

            if (left && top) return ResizeDirection.TopLeft;
            if (right && top) return ResizeDirection.TopRight;
            if (left && bottom) return ResizeDirection.BottomLeft;
            if (right && bottom) return ResizeDirection.BottomRight;
            if (left) return ResizeDirection.Left;
            if (right) return ResizeDirection.Right;
            if (top) return ResizeDirection.Top;
            if (bottom) return ResizeDirection.Bottom;
            return ResizeDirection.None;
        }

        /// <summary>
        /// 该点是否位于顶部蓝色状态条（panel1）范围内。
        /// </summary>
        private bool IsInTitleBarStrip(Point clientPoint)
        {
            if (panel1 == null || !panel1.IsHandleCreated) return false;
            Rectangle strip = panel1.RectangleToScreen(new Rectangle(Point.Empty, panel1.Size));
            Point screenPoint = PointToScreen(clientPoint);
            return screenPoint.Y >= strip.Top && screenPoint.Y <= strip.Bottom;
        }

        /// <summary>
        /// 判断该点是否位于“可拖动标题区”：在顶栏范围内、且最深层控件不是需要点击/输入的控件。
        /// </summary>
        private bool IsTitleBarArea(Point clientPoint)
        {
            Control target = DeepestControlAt(this, clientPoint);
            return !IsInteractiveControl(target);
        }

        /// <summary>自顶向下查找指定坐标处最深层（最上层可见）的控件</summary>
        private static Control DeepestControlAt(Control parent, Point pointInParent)
        {
            Control current = parent;
            Point point = pointInParent;

            while (true)
            {
                Control child = current.GetChildAtPoint(point, GetChildAtPointSkip.Invisible | GetChildAtPointSkip.Disabled);
                if (child == null) return current;

                point = new Point(point.X - child.Left, point.Y - child.Top);
                current = child;
            }
        }

        /// <summary>判断控件是否属于需要鼠标交互的类型（按钮、输入框、列表、图像按钮等）</summary>
        private static bool IsInteractiveControl(Control control)
        {
            if (control == null || control is Form) return false;

            return control is ButtonBase
                || control is TextBoxBase
                || control is ComboBox
                || control is ListControl
                || control is ListView
                || control is TreeView
                || control is DataGridView
                || control is RichTextBox
                || control is ScrollBar
                || control is TrackBar
                || control is UpDownBase
                || control is TabControl
                || control is ToolStrip
                || control is PictureBox
                || control is LinkLabel
                || control is DateTimePicker
                || control is MonthCalendar
                || control is WebBrowser
                || control is AxHost;
        }

        /// <summary>最大化 / 还原切换</summary>
        private void ToggleMaximize()
        {
            if (WindowState == FormWindowState.Maximized)
                WindowState = FormWindowState.Normal;
            else
                WindowState = FormWindowState.Maximized;
        }

        // ===== 手动拖动 / 缩放事件 =====

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button != MouseButtons.Left) return;

            // 优先判断是否落在边缘 → 缩放
            ResizeDirection dir = HitResizeEdge(e.Location);
            if (dir != ResizeDirection.None)
            {
                _isResizing = true;
                _resizeDir = dir;
                _resizeStartBounds = Bounds;
                _resizeStartPoint = Control.MousePosition;
                return;
            }

            // 是否落在顶部标题区 → 拖动移动
            if (IsInTitleBarStrip(e.Location) && IsTitleBarArea(e.Location))
            {
                _isDragging = true;
                _dragOffset = new Point(Control.MousePosition.X - Location.X, Control.MousePosition.Y - Location.Y);

                // 双击判定：短时间内两次点击顶栏 → 最大化/还原
                DateTime now = DateTime.Now;
                if ((now - _lastTitleClickTime).TotalMilliseconds <= DoubleClickInterval)
                {
                    _isDragging = false;
                    _lastTitleClickTime = DateTime.MinValue;
                    ToggleMaximize();
                    return;
                }
                _lastTitleClickTime = now;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            HandleMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            _isDragging = false;
            _isResizing = false;
            _resizeDir = ResizeDirection.None;
        }

        #endregion

        #region show Form      
        public void SwitchMainPage(MENU_PageType PageType)
        {
            // 进入硬件/手动页会暂停设备、进入机器人页会停止设备：设备运行中先确认，取消则留在当前页
            if (!ConfirmPageSwitchSideEffect(PageType))
                return;

            MENU_SelectPage = PageType;

            // 关键操作单独记一条，比"点击了某个按钮"更清楚
            switch (PageType)
            {
                case MENU_PageType.Run: OperationLog.Write("MainForm", "启动运行"); break;
                case MENU_PageType.Stop: OperationLog.Write("MainForm", "停止运行"); break;
                case MENU_PageType.Pause: OperationLog.Write("MainForm", "暂停运行"); break;
                case MENU_PageType.Reset: OperationLog.Write("MainForm", "复位设备"); break;
                case MENU_PageType.Lock: OperationLog.Write("MainForm", "锁定设备"); break;
            }

            Panel ShowPanl = panel2;
            tableLayoutPanel4.Visible = false;

            RefreshMenuBackcolor();

            switch (MENU_SelectPage)
            {
                case MENU_PageType.Home:
                    ShowPanl = plMainShow;
                    ShowhMainPage(MiddleLayer.HomeF, ShowPanl);
                    tableLayoutPanel4.Parent = panel2;
                    tableLayoutPanel4.Visible = true;
                    break;
                case MENU_PageType.Vision:
                    ShowhMainPage(MiddleLayer.VPF, ShowPanl);
                    break;
                case MENU_PageType.Product:
                    ShowhMainPage(MiddleLayer.ProductF, ShowPanl);
                    break;
                case MENU_PageType.Hard:
                    MiddleLayer.PauseRun();
                    ShowhMainPage(MiddleLayer.HardF, ShowPanl);
                    break;
                case MENU_PageType.Manual:
                    MiddleLayer.PauseRun();
                    ShowhMainPage(MiddleLayer.ManualF, ShowPanl);
                    break;
                case MENU_PageType.Check:
                    ShowhMainPage(MiddleLayer.CheckF, ShowPanl);
                    break;
                case MENU_PageType.System:
                    ShowhMainPage(MiddleLayer.SystemF, ShowPanl);
                    break;
                case MENU_PageType.AddUser:
                    ShowhMainPage(MiddleLayer.AddF, ShowPanl);
                    break;
                case MENU_PageType.Log:
                    ShowhMainPage(MiddleLayer.LogF, ShowPanl);
                    break;
                case MENU_PageType.Data:
                    ShowhMainPage(MiddleLayer.DataF, ShowPanl);
                    break;
                case MENU_PageType.Robot:
                    MiddleLayer.StopRun();
                    ShowhMainPage(MiddleLayer.RobotF, ShowPanl);
                    break;
                case MENU_PageType.Rapid:
                    ShowhMainPage(MiddleLayer.FlowF, ShowPanl);
                    break;
                case MENU_PageType.LifeSpan:
                    ShowhMainPage(MiddleLayer.SpanLifeF, ShowPanl);
                    break;
                case MENU_PageType.Mes:
                    ShowhMainPage(MiddleLayer.MesF, ShowPanl);
                    break;
                case MENU_PageType.Exit:
                    Close();
                    break;
            }
        }
        /// <summary>
        /// 切页前的安全确认：Hard / Manual 页会暂停设备，Robot 页会停止设备。
        /// 设备正在动作时弹框确认，返回 false 表示取消切页；设备空闲时直接返回 true。
        /// </summary>
        private bool ConfirmPageSwitchSideEffect(MENU_PageType pageType)
        {
            bool busy = SysPara.SystemMode == RunMode.RUN
                     || (SysPara.SystemMode == RunMode.INITIAL && !SysPara.UpConveyorInitialOk);

            string message;
            if (pageType == MENU_PageType.Hard || pageType == MENU_PageType.Manual)
            {
                if (!busy) return true;
                message = MiddleLayer.LangMsg("MainForm", "msg_ConfirmPauseForPage",
                    "设备正在运行。\r\n进入此页面会暂停设备，是否继续？",
                    "The machine is running.\r\nOpening this page will PAUSE the machine. Continue?",
                    "La máquina está en marcha.\r\nAbrir esta página PAUSARÁ la máquina. ¿Continuar?");
            }
            else if (pageType == MENU_PageType.Robot)
            {
                if (SysPara.SystemMode == RunMode.IDLE) return true;
                message = MiddleLayer.LangMsg("MainForm", "msg_ConfirmStopForPage",
                    "进入机器人页面会停止设备，之后需要重新初始化。\r\n是否继续？",
                    "Opening the Robot page will STOP the machine and it must be initialized again.\r\nContinue?",
                    "Abrir la página del robot DETENDRÁ la máquina y deberá inicializarse de nuevo.\r\n¿Continuar?");
            }
            else
            {
                return true;
            }

            string title = MiddleLayer.LangMsg("Common", "msg_NoteTitle", "提示", "Note", "Consejo");
            // 默认按钮设为"否"：误触回车不会停机
            return MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                                   MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }







        #region 报警日志筛选 + 语言切换（全部 / 警告 / 报警）

        /// <summary>当前筛选："ALL"=全部，"E"=仅报警(错误)，"OTHER"=仅警告（W 等）</summary>
        public string AlarmLogFilter = "ALL";

        /// <summary>报警原始数据缓存（时间/类型/代码/内容），筛选重建时用它，避免重复副作用。</summary>
        public readonly System.Collections.Generic.List<string[]> AlarmLogCache =
            new System.Collections.Generic.List<string[]>();

        /// <summary>筛选按钮文案：按 LanguageType 顺序 = Chinese / English / Español</summary>
        private static readonly string[][] AlarmFilterTexts =
        {
            new string[] { "全部", "警告", "报警" },
            new string[] { "All", "Warning", "Alarm" },
            new string[] { "Todo", "Aviso", "Alarma" }
        };

        // 报警筛选标签的图标颜色：全部 = 品牌蓝 / 警告 = 琥珀 / 报警 = 红
        private static readonly Color AlarmChipBrand = Color.FromArgb(4, 108, 182);
        private static readonly Color AlarmChipWarn = Color.FromArgb(219, 149, 44);
        private static readonly Color AlarmChipError = Color.FromArgb(214, 69, 69);

        /// <summary>判断某条报警在当前筛选下是否显示。</summary>
        public bool IsAlarmRowVisible(string type)
        {
            switch (AlarmLogFilter)
            {
                case "E":
                    return string.Equals(type, "E", StringComparison.OrdinalIgnoreCase);
                case "OTHER":
                    return !string.Equals(type, "E", StringComparison.OrdinalIgnoreCase);
                default:
                    return true;
            }
        }


        /// <summary>用缓存重建报警列表（应用当前筛选），不触发任何副作用。</summary>
        public void RebuildAlarmLogFromCache()
        {
            try
            {
                WarnningMessage.BeginUpdate();
                WarnningMessage.Items.Clear();
                for (int i = 0; i < AlarmLogCache.Count; i++)
                {
                    string[] r = AlarmLogCache[i];
                    if (!IsAlarmRowVisible(r[1])) continue;
                    var it = new ListViewItem(r[0]);
                    it.SubItems.Add(r[1]);
                    it.SubItems.Add(r[2]);
                    it.SubItems.Add(r[3]);
                    ApplyAlarmRowColor(it, r[1], i);
                    WarnningMessage.Items.Add(it);
                }
                WarnningMessage.EndUpdate();
            }
            catch { }
            UpdateAlarmFilterCount();
        }

        private void SetAlarmLogFilter(string filter)
        {
            AlarmLogFilter = filter;
            UpdateAlarmFilterButtons();
            RebuildAlarmLogFromCache();
        }

        /// <summary>刷新三个筛选标签上的条数（报警任务和筛选重建时调用）。</summary>
        public void UpdateAlarmFilterCount()
        {
            RefreshAlarmChipTexts();
        }

        /// <summary>按类型统计报警条数：total = 全部 / warn = 非错误（警告）/ error = E 错误（报警）。</summary>
        private void GetAlarmCounts(out int total, out int warn, out int error)
        {
            total = AlarmLogCache.Count;
            warn = 0;
            error = 0;
            try
            {
                for (int i = 0; i < AlarmLogCache.Count; i++)
                {
                    string[] r = AlarmLogCache[i];
                    if (r == null || r.Length < 2) continue;
                    if (string.Equals(r[1], "E", StringComparison.OrdinalIgnoreCase)) error++;
                    else warn++;
                }
            }
            catch { }
        }

        /// <summary>刷新三个筛选标签的文字：当前语言的名称 + 该类型条数（如 All 73 / Warning 72 / Alarm 1）。</summary>
        private void RefreshAlarmChipTexts()
        {
            try
            {
                int lang = (int)SysPara.LanguageShow;
                if (lang < 0 || lang >= AlarmFilterTexts.Length) lang = 0;

                int total, warn, error;
                GetAlarmCounts(out total, out warn, out error);

                if (btnAlarmFilterAll != null) btnAlarmFilterAll.Text = AlarmFilterTexts[lang][0] + " " + total;
                if (btnAlarmFilterWarn != null) btnAlarmFilterWarn.Text = AlarmFilterTexts[lang][1] + " " + warn;
                if (btnAlarmFilterAlarm != null) btnAlarmFilterAlarm.Text = AlarmFilterTexts[lang][2] + " " + error;
            }
            catch { }
        }

        private void UpdateAlarmFilterButtons()
        {
            if (btnAlarmFilterAll != null) btnAlarmFilterAll.Selected = (AlarmLogFilter == "ALL");
            if (btnAlarmFilterAlarm != null) btnAlarmFilterAlarm.Selected = (AlarmLogFilter == "E");
            if (btnAlarmFilterWarn != null) btnAlarmFilterWarn.Selected = (AlarmLogFilter == "OTHER");
        }

        // ---------------- 语言切换 ----------------

        /// <summary>
        /// 顶栏语言切换器：选择语言后调用 SwitchLanguageTo；显示与启动时读取的语言同步（设置 Current 不会触发切换）。
        /// </summary>
        private void InitLanguageSwitch()
        {
            try
            {
                if (languageSwitch == null) return;

                languageSwitch.LanguageSelected += delegate
                {
                    SwitchLanguageTo(languageSwitch.Current);
                };
                languageSwitch.Current = SysPara.LanguageShow;
            }
            catch { }
        }

        // ---------------- 虚拟键盘 ----------------

        /// <summary>
        /// 顶栏虚拟键盘开关（设计器中的 picVirtualKeyboard）：点击调出 osk，再次点击收起。此处绘制图标并挂接事件。
        /// </summary>
        private void InitVirtualKeyboardToggle()
        {
            try
            {
                if (picVirtualKeyboard == null) return;

                picVirtualKeyboard.Image = AppIcons.Get(AppIcon.Keyboard, 24, AppIconColor.OnDarkBar);  // 深蓝底 → 白色图标
                picVirtualKeyboard.MouseEnter += delegate { picVirtualKeyboard.BackColor = Color.FromArgb(30, 132, 200); };
                picVirtualKeyboard.MouseLeave += delegate { picVirtualKeyboard.BackColor = Color.FromArgb(4, 108, 182); };
                picVirtualKeyboard.Click += delegate { ToggleVirtualKeyboard(); };
            }
            catch { }
        }

        /// <summary>osk 在跑就收起（杀进程），没跑就调出。</summary>
        private void ToggleVirtualKeyboard()
        {
            try
            {
                System.Diagnostics.Process[] a = System.Diagnostics.Process.GetProcessesByName("osk");
                if (a.Length > 0)
                {
                    for (int i = 0; i < a.Length; i++) { try { a[i].Kill(); } catch { } }
                    return;
                }
                if (System.IO.File.Exists(@"C:\Windows\system32\osk.exe"))
                    System.Diagnostics.Process.Start(@"C:\Windows\system32\osk.exe");
            }
            catch { }
        }

        /// <summary>统一切换界面语言（顶栏 CN 下拉 与 报警栏语言选择 共用同一入口）。</summary>
        public void SwitchLanguageTo(LanguageType lanType)
        {
            try
            {
                SysPara.LanguageShow = lanType;
                MiddleLayer.SwitchLanguage(lanType);
                SysPara.LanguageName = (lanType == LanguageType.Chinese) ? "Chinese"
                                      : (lanType == LanguageType.English) ? "English"
                                      : "Español";
                IniFile ini = new IniFile(".\\MachineSetup.ini");
                ini.WriteString("MachineSetup", "LanguageName", SysPara.LanguageName);
            }
            catch (Exception ex)
            {
                // 切换失败不中断界面，异常写入调试输出
                System.Diagnostics.Debug.WriteLine("[SwitchLanguageTo] " + lanType + " -> " + ex);
            }
            // 同步语言包覆盖不到的文字
            SyncLanguageTexts();
        }

        /// <summary>
        /// 同步语言包覆盖不到的文字：报警筛选标签的"名称 + 条数"，以及顶栏语言切换器显示的语言代码。
        /// </summary>
        private void SyncLanguageTexts()
        {
            try
            {
                // 标签文字 = 名称 + 条数
                UpdateAlarmFilterCount();

                // 导航与工具栏按钮文字
                LoadFrameTexts();
                RefreshButtonIcons();

                if (languageSwitch != null) languageSwitch.Current = SysPara.LanguageShow;

                // 登录芯片的悬停提示（含完整登录时间）
                UpdateLoginChipTip();
            }
            catch { }
        }

        // ---------------- 界面构建 ----------------

        /// <summary>
        /// 报警列表上方的筛选条（全部 / 警告 / 报警，控件在设计器中的 panelAlarmFilter）：
        /// 绘制图标、挂接点击事件，并登记到语言表。
        /// </summary>
        private void InitAlarmFilterBar()
        {
            try
            {
                if (btnAlarmFilterAll == null) return;

                btnAlarmFilterAll.Icon = AlarmChip.CreateFunnelIcon(14, AlarmChipBrand);
                btnAlarmFilterAll.IconSelected = AlarmChip.CreateFunnelIcon(14, Color.White);
                btnAlarmFilterWarn.Icon = AlarmChip.CreateAlertTriangle(14, AlarmChipWarn);
                btnAlarmFilterWarn.IconSelected = AlarmChip.CreateAlertTriangle(14, Color.White, AlarmChipWarn);
                btnAlarmFilterAlarm.Icon = AlarmChip.CreateErrorCircle(14, AlarmChipError);
                btnAlarmFilterAlarm.IconSelected = AlarmChip.CreateErrorCircle(14, Color.White, AlarmChipError);

                btnAlarmFilterAll.Click += (s, e) => SetAlarmLogFilter("ALL");
                btnAlarmFilterWarn.Click += (s, e) => SetAlarmLogFilter("OTHER");
                btnAlarmFilterAlarm.Click += (s, e) => SetAlarmLogFilter("E");

                RegisterAlarmFilterForLanguage();
                UpdateAlarmFilterButtons();
                // 按当前语言刷新标签文字
                SyncLanguageTexts();
            }
            catch { }
        }

        /// <summary>
        /// 把报警筛选条的控件登记进语言表 SysPara.ComponentLangurageList（语言表在 MainForm_Load 之前建好，需手动登记）。
        /// </summary>
        private void RegisterAlarmFilterForLanguage()
        {
            try
            {
                var lists = SysPara.ComponentLangurageList;
                if (lists == null) return;
                for (int lang = 0; lang < lists.Length && lang < AlarmFilterTexts.Length; lang++)
                {
                    var list = lists[lang];
                    if (list == null) continue;
                    AddLanguageItem(list, btnAlarmFilterAll, AlarmFilterTexts[lang][0]);
                    AddLanguageItem(list, btnAlarmFilterWarn, AlarmFilterTexts[lang][1]);
                    AddLanguageItem(list, btnAlarmFilterAlarm, AlarmFilterTexts[lang][2]);
                }
            }
            catch { }
        }

        /// <summary>把一个控件登记进指定语言的文本表，SwitchLanguage 时会自动套用。</summary>
        private static void AddLanguageItem(System.Collections.Generic.List<ComponentTextInfo> list, Control c, string text)
        {
            if (list == null || c == null) return;
            ComponentTextInfo info = new ComponentTextInfo();
            info.FormName = "MainForm";
            info.ComponentName = c.Name;
            info.ComponentText = text;
            info.Component = c;
            list.Add(info);
        }

        #endregion
        /// <summary>在指定面板中显示页面窗体（Dock 铺满）。</summary>
        public void ShowhMainPage(dynamic ShowPage, Panel ShowPanl)
        {
            ShowPanl.Focus();
            foreach (Control Fcontrol in panel2.Controls)
            {
                Fcontrol.Parent = null;
                Fcontrol.Visible = false;
            }

            if (ShowPage.GetType().IsSubclassOf(typeof(Form)))
            {
                ShowPage.TopLevel = false;
                ShowPage.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                // 嵌入的页面保持 Normal 状态，由 Dock = Fill 随容器缩放
                ShowPage.WindowState = FormWindowState.Normal;
                ShowPage.Dock = DockStyle.Fill;
            }
            else
                ShowPage.Dock = DockStyle.Fill;

            ShowPage.Parent = ShowPanl;
            ShowPage.Show();

            // 子页面也转发鼠标事件，使窗口边缘缩放在子页面区域同样有效
            if (ShowPage is Control pageControl)
                HookMouseForwarding(pageControl);
        }

        private void MENU_Click(object sender, EventArgs e)
        {
            string ItemName = Convert.ToString(((Control)sender).Tag);
            MENU_PageType MENU_Page_Type = (MENU_PageType)Enum.Parse(typeof(MENU_PageType), ItemName);
            SwitchMainPage(MENU_Page_Type);
        }
        // 用户登录按钮
        private void UserLogin_Click(object sender, EventArgs e)
        {
            UserLoginForm UserLoginF = new UserLoginForm();

            string OrgUser = SysPara.UserName;
            UserLoginF.ShowDialog();
            if (OrgUser != SysPara.UserName)
                if (!MENU_Picture[(int)MENU_SelectPage].Enabled)
                    SwitchMainPage(MENU_PageType.Home);
                else
                    RefreshMenuBackcolor();
        }
        /// <summary>按权限表（PermissionSetup）启用或禁用菜单按钮。</summary>
        public void SwitchPermission(PermissionType Permission)
        {
            string strSQL = "select * from PermissionSetup where Permission ='" + Permission.ToString() + "'";
            bool Successful = false;

            DataTable readData = DataBase.ReadData_Adapter(SysPara.MdbPath, strSQL, ref Successful);
            if (Successful)
            {
                if (readData.Rows.Count > 0)
                {
                    MENU_Product.Enabled = Convert.ToBoolean(readData.Rows[0]["Product"]);
                    MENU_Hard.Enabled = Convert.ToBoolean(readData.Rows[0]["Hard"]);
                    MENU_Manual.Enabled = Convert.ToBoolean(readData.Rows[0]["Manual"]);
                    MENU_Check.Enabled = Convert.ToBoolean(readData.Rows[0]["Check"]);
                    MENU_System.Enabled = Convert.ToBoolean(readData.Rows[0]["System"]);
                    MENU_AddUser.Enabled = (Permission == PermissionType.Administrator);
                    MENU_Mes.Enabled = Convert.ToBoolean(readData.Rows[0]["Mes"]);
                    MENU_Rapid.Enabled = Convert.ToBoolean(readData.Rows[0]["Rapid"]);
                    MENU_Data.Enabled = Convert.ToBoolean(readData.Rows[0]["Data"]);
                    MENU_Vision.Enabled = Convert.ToBoolean(readData.Rows[0]["Vision"]);
                    MENU_Exit.Enabled = Convert.ToBoolean(readData.Rows[0]["Exit"]);
                }
            }
        }
        #endregion

        #region Machine Status 
        /// <summary>各运行模式的状态文案（取自语言包，按 SysPara.LanguageShow）。</summary>
        private static Dictionary<RunMode, string> GetStatusTextMap(LanguageType language)
        {
            return new Dictionary<RunMode, string>
            {
                { RunMode.IDLE, MiddleLayer.LangMsg("MainForm", "msg_StatusIdle", "待机状态", "IDLE", "modo espera") },
                { RunMode.INITIAL, SysPara.UpConveyorInitialOk
                        ? MiddleLayer.LangMsg("MainForm", "msg_StatusInitOk", "初始化完成", "INITIAL Completed", "cargando terminado")
                        : MiddleLayer.LangMsg("MainForm", "msg_StatusInitializing", "正在初始化状态", "INITIAL", "Estado inicialización") },
                { RunMode.RUN, MiddleLayer.LangMsg("MainForm", "msg_StatusRun", "运行状态", "RUNNING", "CORRER") },
                { RunMode.PAUSE, MiddleLayer.LangMsg("MainForm", "msg_StatusPause", "暂停状态", "PAUSE", "Estado suspendido") },
            };
        }
        // 机台状态条前缀文案（按语言缓存，避免定时器反复查 XML）
        private LanguageType _statusLabelLang = (LanguageType)(-1);
        private string _lblRecipe;

        /// <summary>按当前语言（变化时才重取）准备好状态条前缀文案。</summary>
        private void EnsureStatusLabels()
        {
            try
            {
                if (_statusLabelLang == SysPara.LanguageShow && _lblRecipe != null) return;
                _statusLabelLang = SysPara.LanguageShow;
                _lblRecipe = MiddleLayer.LangMsg("MainForm", "msg_RecipeLabel", "配方:  ", "RecipeName :  ", "Fórmula :  ");
            }
            catch { }
        }

        // ---------------- 机台状态徽章：深藏蓝底 + 左侧状态色竖条 + 白字（沿用现有蓝色主题；红只用于报警） ----------------
        private static readonly Color StatusBadgeBg = Color.FromArgb(11, 46, 77);       // 深藏蓝底（比顶栏更深，压住蓝色背景）
        private static readonly Color StatusBadgeAlarmBg = Color.FromArgb(96, 26, 26);  // 报警时整块转深红

        // 竖条用的语义色：顶栏是蓝色，这几档都调亮过一档，纯色块在白字旁才够跳
        private static readonly Color StatusRunAccent = Color.FromArgb(56, 176, 88);     // 绿：运行
        private static readonly Color StatusPauseAccent = Color.FromArgb(247, 176, 47);  // 琥珀：暂停
        private static readonly Color StatusIdleAccent = Color.FromArgb(140, 160, 182);  // 灰蓝：待机
        private static readonly Color StatusInitAccent = Color.FromArgb(58, 158, 226);   // 亮蓝：初始化
        private static readonly Color StatusAlarmAccent = Color.FromArgb(232, 76, 76);   // 红：报警

        /// <summary>当前状态竖条颜色（MachineStatus_Paint 里用）。</summary>
        private Color _statusAccent = StatusIdleAccent;

        private const int StatusBarInset = 9;   // 竖条距徽章左边的距离
        private const int StatusBarWidth = 6;   // 竖条宽度
        private const int StatusBarPadY = 11;   // 竖条上下留白

        // 状态文案缓存：语言或初始化状态变化时重建
        private Dictionary<RunMode, string> _statusTextMap;
        private LanguageType _statusTextLang = (LanguageType)(-1);
        private bool _statusTextInitOk;
        private string _statusAlarmText;
        private string _lastAlarmTip;
        private readonly ToolTip _statusTip = new ToolTip();

        private void UpdateMachineStatus()
        {
            if (_statusTextMap == null || _statusTextLang != SysPara.LanguageShow
                || _statusTextInitOk != SysPara.UpConveyorInitialOk)
            {
                _statusTextMap = GetStatusTextMap(SysPara.LanguageShow);
                _statusTextLang = SysPara.LanguageShow;
                _statusTextInitOk = SysPara.UpConveyorInitialOk;
                _statusAlarmText = MiddleLayer.LangMsg("MainForm", "msg_StatusAlarm", "设备报警", "ALARM", "ALARMA");
            }

            // 有 E 类报警时优先显示红色"报警"和条数，悬停显示最新一条报警及处理方法
            int errorCount;
            string latestError;
            if (TryGetActiveErrors(out errorCount, out latestError))
            {
                SetStatusLook(StatusBadgeAlarmBg, StatusAlarmAccent,
                    errorCount > 1 ? _statusAlarmText + "  ×" + errorCount : _statusAlarmText);
                if (latestError != _lastAlarmTip)
                {
                    _lastAlarmTip = latestError;
                    _statusTip.SetToolTip(MachineStatus, latestError ?? "");
                }
                return;
            }
            if (_lastAlarmTip != null)
            {
                _lastAlarmTip = null;
                _statusTip.SetToolTip(MachineStatus, "");
            }

            // 无报警时按运行模式着色
            string statusText;
            if (!_statusTextMap.TryGetValue(SysPara.SystemMode, out statusText))
            {
                SetStatusLook(StatusBadgeBg, StatusIdleAccent, "Unknown status");
                return;
            }
            switch (SysPara.SystemMode)
            {
                case RunMode.RUN: SetStatusLook(StatusBadgeBg, StatusRunAccent, statusText); break;
                case RunMode.PAUSE: SetStatusLook(StatusBadgeBg, StatusPauseAccent, statusText); break;
                case RunMode.INITIAL:
                    // 初始化中 / 初始化完成（就绪）都用蓝色竖条，文字区分两者
                    SetStatusLook(StatusBadgeBg, StatusInitAccent, statusText); break;
                default: SetStatusLook(StatusBadgeBg, StatusIdleAccent, statusText); break;
            }
        }

        /// <summary>
        /// 设置徽章外观：深藏蓝底（报警时深红底）+ 左侧状态色竖条 + 白字。
        /// 只在值变化时赋值并重绘，避免每秒刷新造成闪烁。
        /// </summary>
        private void SetStatusLook(Color fill, Color accent, string text)
        {
            bool repaint = _statusAccent != accent;
            if (MachineStatus.BackColor != fill) MachineStatus.BackColor = fill;
            if (MachineStatus.ForeColor != Color.White) MachineStatus.ForeColor = Color.White;
            if (MachineStatus.Text != text) MachineStatus.Text = text;
            if (repaint)
            {
                _statusAccent = accent;
                MachineStatus.Invalidate();
            }
        }

        /// <summary>徽章左侧的状态色竖条（颜色随运行模式 / 报警变化）。</summary>
        private void MachineStatus_Paint(object sender, PaintEventArgs e)
        {
            int h = MachineStatus.Height - StatusBarPadY * 2;
            if (h <= 6) return;
            using (SolidBrush b = new SolidBrush(_statusAccent))
                e.Graphics.FillRectangle(b, StatusBarInset, StatusBarPadY, StatusBarWidth, h);
        }

        /// <summary>
        /// 读取当前 E 类（错误）报警的条数和最新一条内容。AlarmList 由报警线程维护，读取失败时按无报警处理。
        /// </summary>
        private static bool TryGetActiveErrors(out int count, out string latest)
        {
            count = 0;
            latest = null;
            try
            {
                if (!NPSDK.Alarm.IsError) return false;
                var list = NPSDK.Alarm.AlarmList;
                int n = list.Count;
                for (int i = 0; i < n; i++)
                {
                    NPSDK.Alarm.AlarmDataClass a = list[i];   // 结构体，不会为 null
                    if (a.Type != "E") continue;
                    count++;
                    latest = a.Code + "  " + MiddleLayer.HomeF.ResolveAlarmContent(a.Code, a.Content);
                    if (!string.IsNullOrEmpty(a.Solution))
                        latest += "\r\n" + MiddleLayer.LangMsg("MainForm", "msg_AlarmSolution", "处理方法：", "Solution: ", "Solución: ") + a.Solution;
                }
                if (count == 0) count = 1;   // IsError 为真但列表还没刷新：至少按 1 条显示
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        private void timer1_Tick(object sender, EventArgs e)
        {
            #region Machine Status 
            UpdateMachineStatus();
            #endregion

            #region ProductData
            #endregion

            #region Language
            // 状态条前缀文案（取自语言包 LanguageData\MainForm，按语言缓存）
            EnsureStatusLabels();
            #endregion

            #region  RecipeName
            toolStripStatusLabel5.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            // 右上角登录芯片：用户名 · 权限 · 登录时刻（完整日期见悬停提示；芯片宽度有限，只显示到分钟）
            string loginAt = SysPara.UserLoginTime ?? "";
            DateTime loginTime;
            if (DateTime.TryParse(loginAt, out loginTime)) loginAt = loginTime.ToString("HH:mm");
            LoginText.Text = SysPara.UserName + " · " + SysPara.UserPermission + " · " + loginAt;
            lbRecipeName.Text = _lblRecipe + MiddleLayer.ProductF.CurrentModel.Text;
            #endregion

            #region  PictureBox

            // 按运行状态设置按钮可用性，图标颜色由 RefreshButtonIcons() 按 Enabled 绘制
            MENU_Run.Enabled = SysPara.UpConveyorInitialOk
                && (SysPara.SystemMode == RunMode.INITIAL || SysPara.SystemMode == RunMode.PAUSE);
            MENU_Pause.Enabled = SysPara.SystemMode == RunMode.RUN;
            MENU_LifeSpan.Enabled = (SysPara.UserPermission == PermissionType.Administrator
                                     || SysPara.UserPermission == PermissionType.Maintenance);
            MENU_Reset.Enabled = SysPara.SystemMode == RunMode.IDLE;

            RefreshButtonIcons();
            #endregion
        }

        #region Run Message

        public void AddErrorLog(string strMessage)
        {
            MiddleLayer.DataF.AddLogError(strMessage);
        }
        public void WriteRunMessageResult(string RunTime, string strMessage)
        {
            ListViewItem lvi = new ListViewItem(RunTime);
            lvi.SubItems.Add(strMessage);
            lvi.SubItems.Add(SysPara.UserName);
            string Year = DateTime.Now.Year.ToString();
            string month = DateTime.Now.Month.ToString();
            string day = DateTime.Now.Day.ToString();
            SysPara.RunMessagePath = MiddleLayer.LogF.GetSettingValue("Path", "RunPath") + "\\RunMessageData\\" + "\\" + Year + "\\" + month + "\\" + day + "\\";
            using (ListView listView1 = new ListView())
            {
                listView1.Items.Add(lvi);
                ListViewWrite.WriteExcelData(SysPara.RunMessagePath, listView1);
            }
        }
        #endregion

        private readonly object ProductObjLock = new object();

        public void iProductOKAdd()
        {
            lock (ProductObjLock)
            {
                SysPara.iProductOK++;
                RecordHourly(true);
            }
        }
        public void iProductNGAdd()
        {
            lock (ProductObjLock)
            {
                SysPara.iProductNG++;
                RecordHourly(false);
            }
        }

        /// <summary>每小时统计所属的日期，跨天时清零。</summary>
        private DateTime _hourlyDate = DateTime.Today;

        /// <summary>记录当天每小时的投入 / 产出 / 不良 / 良率（首页每小时产量图使用）。</summary>
        private void RecordHourly(bool ok)
        {
            DateTime now = DateTime.Now;
            if (now.Date != _hourlyDate)
            {
                Array.Clear(SysPara.iProductHourlyInput, 0, SysPara.iProductHourlyInput.Length);
                Array.Clear(SysPara.iProductHourlyOutput, 0, SysPara.iProductHourlyOutput.Length);
                Array.Clear(SysPara.iProductHourlyReject, 0, SysPara.iProductHourlyReject.Length);
                Array.Clear(SysPara.iProductHourlyYield, 0, SysPara.iProductHourlyYield.Length);
                _hourlyDate = now.Date;
            }
            int h = now.Hour;
            SysPara.iProductHourlyInput[h]++;
            if (ok) SysPara.iProductHourlyOutput[h]++;
            else SysPara.iProductHourlyReject[h]++;
            SysPara.iProductHourlyYield[h] = SysPara.iProductHourlyOutput[h] * 100.0 / SysPara.iProductHourlyInput[h];
        }


        #region reminder
        /// <summary>菜单按钮悬停提示（所有按钮共用一个 ToolTip）。</summary>
        private readonly ToolTip _menuTip = new ToolTip { ShowAlways = true };

        public void ShowWord(Control con, string word)
        {
            _menuTip.SetToolTip(con, word);
        }
        private void MouseEnter1(object sender, EventArgs e)
        {
            string ItemName = Convert.ToString(((Control)sender).Tag);
            SwitchRemind((MENU_PageType1)Enum.Parse(typeof(MENU_PageType1), ItemName));
        }
        private void SwitchRemind(MENU_PageType1 PageType)
        {
            // 悬停提示文案取自语言包：键为菜单控件名（LanguageData\{语言}.xml 的 /{语言}/MainForm/{键}）
            MENU_SelectPage1 = PageType;
            switch (MENU_SelectPage1)
            {
                case MENU_PageType1.Home:
                    ShowWord(this.MENU_Home, MiddleLayer.LangMsg("MainForm", "MENU_Home", "主界面", "Home", "Inicio"));
                    break;
                case MENU_PageType1.Product:
                    ShowWord(this.MENU_Product, MiddleLayer.LangMsg("MainForm", "MENU_Product", "物料管理", "ProductManager", "Gestor de productos"));
                    break;
                case MENU_PageType1.Save:
                    ShowWord(this.MENU_Save, MiddleLayer.LangMsg("MainForm", "MENU_Save", "保存", "Save", "Guardar"));
                    break;
                case MENU_PageType1.Hard:
                    ShowWord(this.MENU_Hard, MiddleLayer.LangMsg("MainForm", "MENU_Hard", "硬件调试", "HardForm", "Depuración de hardware"));
                    break;
                case MENU_PageType1.Manual:
                    ShowWord(this.MENU_Manual, MiddleLayer.LangMsg("MainForm", "MENU_Manual", "手动界面", "ManagerForm", "Manual"));
                    break;
                case MENU_PageType1.Check:
                    ShowWord(this.MENU_Check, MiddleLayer.LangMsg("MainForm", "MENU_Check", "信号监视", "CheckForm", "Monitor de señales"));
                    break;
                case MENU_PageType1.System:
                    ShowWord(this.MENU_System, MiddleLayer.LangMsg("MainForm", "MENU_System", "系统设置", "SystemForm", "Ajustes del sistema"));
                    break;
                case MENU_PageType1.AddUser:
                    ShowWord(this.MENU_AddUser, MiddleLayer.LangMsg("MainForm", "MENU_AddUser", "用户设置", "UserManager", "Gestor de usuarios"));
                    break;
                case MENU_PageType1.Mes:
                    ShowWord(this.MENU_Mes, MiddleLayer.LangMsg("MainForm", "MENU_Mes", "数据上传", "MES", "Carga de datos"));
                    break;
                case MENU_PageType1.Rapid:
                    ShowWord(this.MENU_Rapid, MiddleLayer.LangMsg("MainForm", "MENU_Rapid", "快捷键", "RapidButton", "Atajos"));
                    break;
                case MENU_PageType1.Data:
                    ShowWord(this.MENU_Data, MiddleLayer.LangMsg("MainForm", "MENU_Data", "产品数据", "ProductData", "Datos de producto"));
                    break;
                case MENU_PageType1.Vision:
                    ShowWord(this.MENU_Vision, MiddleLayer.LangMsg("MainForm", "MENU_Vision", "视觉界面", "VisionForm", "Visión"));
                    break;
                case MENU_PageType1.Login:
                    ShowWord(this.MENU_Login, MiddleLayer.LangMsg("MainForm", "MENU_Login", "用户登录", "UserLogin", "Iniciar sesión"));
                    break;
                case MENU_PageType1.Reset:
                    ShowWord(this.MENU_Reset, MiddleLayer.LangMsg("MainForm", "MENU_Reset", "复位", "ResetButton", "Reset"));
                    break;
                case MENU_PageType1.Run:
                    ShowWord(this.MENU_Run, MiddleLayer.LangMsg("MainForm", "MENU_Run", "运行", "RunButton", "Run"));
                    break;
                case MENU_PageType1.Pause:
                    ShowWord(this.MENU_Pause, MiddleLayer.LangMsg("MainForm", "MENU_Pause", "暂停", "PauseButton", "Pausa"));
                    break;
                case MENU_PageType1.Stop:
                    ShowWord(this.MENU_Stop, MiddleLayer.LangMsg("MainForm", "MENU_Stop", "停止", "StopButton", "Stop"));
                    break;
                case MENU_PageType1.Exit:
                    ShowWord(this.MENU_Exit, MiddleLayer.LangMsg("MainForm", "MENU_Exit", "退出", "Exit", "Salir"));
                    break;
                case MENU_PageType1.Log:
                    ShowWord(this.MENU_Log, MiddleLayer.LangMsg("MainForm", "MENU_Log", "日志", "Log", "Registro"));
                    break;
            }
        }

#endregion
        /// <summary>
        /// 初始化按钮
        /// </summary>
        private void MENU_Reset_Click(object sender, EventArgs e)
        {
            MiddleLayer.Initial();
        }
        //保存数据按钮
        private void SaveData_Click(object sender, EventArgs e)
        {
            SaveData();
        }
        public void SaveData()
        {
            SysPara.items = 1;
            SysPara.items2 = 1;
            DialogResult dr;
            MachineStatus.Focus();

            OptionChoiceForm warning = new OptionChoiceForm();
            warning.fnChangeButtonsText("YES", "OK", "NO");
            warning.fnSetMessageAndButtons(MiddleLayer.LangMsg("MainForm", "msg_SaveConfirm", "确认要保存吗？", "Are you sure to save it？", "¿Seguro que quieres guardar?"), true, false, true);
            warning.ShowDialog();
            dr = warning.dResult;
            if (dr == DialogResult.Yes)
            {
                plMainShow.Focus();
                MiddleLayer.AddF.WritePermission();
                for (int i = 0; i < ModuleManager.ModuleList.Count; i++)
                {
                    ModuleManager.ModuleList[i].WriteRecipeData(SysPara.FilePath);
                    ModuleManager.ModuleList[i].WriteSettingData();
                }

                // 视觉参数单独保存在 ModuleData\SettingData\VPForm.Cameras.xml
                if (MiddleLayer.VPF != null) MiddleLayer.VPF.CommitVpConfig();

                OperationLog.Write("MainForm", "保存系统参数（配方：" + SysPara.RecipeName + "）");
            }
            else
            {
                for (int i = 0; i < ModuleManager.ModuleList.Count; i++)
                {
                    ModuleManager.ModuleList[i].ReadRecipeData(SysPara.FilePath);
                    ModuleManager.ModuleList[i].ReadSettingData();
                }

                // 选"否"：视觉参数恢复为上一次保存的值
                if (MiddleLayer.VPF != null) MiddleLayer.VPF.RevertVpConfig();

                OperationLog.Write("MainForm", "放弃保存，参数还原为上次保存值");
            }
            MiddleLayer.HardF.SaveHardData();
        }

        private void btStart_Click(object sender, EventArgs e)
        {
            MiddleLayer.StartRun();
        }

        private void btStop_Click(object sender, EventArgs e)
        {
            MiddleLayer.StopRun();
        }

        private void btPause_Click(object sender, EventArgs e)
        {
            MiddleLayer.PauseRun();
        }
        /// <summary>
        /// 把用户登录数据保存到文件
        /// </summary>
        public void AddUserResult(string UserName, string UserPermission, string LoginTime)
        {
            ListViewItem lvi = new ListViewItem(LoginTime);
            lvi.SubItems.Add(UserName);
            lvi.SubItems.Add(UserPermission);
            string Year = DateTime.Now.Year.ToString();
            string month = DateTime.Now.Month.ToString();
            string day = DateTime.Now.Day.ToString();
            SysPara.UserMessagePath = MiddleLayer.LogF.GetSettingValue("Path", "UserPath") + "\\UserLoginData\\" + Year + "\\" + month + "\\" + day + "\\";
            using (ListView listView1 = new ListView())
            {
                listView1.Items.Add(lvi);
                ListViewWrite.WriteExcelData(SysPara.UserMessagePath, listView1);
            }
        }

        //鼠标监听事件
        #region MouseMonitor

        /// <summary>
        /// 重置无操作自动登出定时器；窗体已释放时直接返回（退出过程中钩子仍可能触发）。
        /// </summary>
        private void TryResetLoginOutTimer()
        {
            try
            {
                if (IsDisposed || Disposing || LoginOutTime == null)
                    return;

                if (SysPara.UserPermission != PermissionType.Operator)
                {
                    LoginOutTime.Stop();

                    dynamic intervalValue = null;
                    if (MiddleLayer.PlatF != null)
                        intervalValue = MiddleLayer.PlatF.GetSettingValue("MSet", "Interval");

                    if (intervalValue != null)
                        LoginOutTime.Interval = intervalValue * 1000;

                    LoginOutTime.Start();
                }
            }
            catch
            {
                // 退出过程中相关对象可能已释放，忽略异常
            }
        }

        /// <summary>全局鼠标按下：重置无操作自动登出计时。</summary>
        private void mh_MouseDownEvent(object sender, MouseEventArgs e)
        {
            TryResetLoginOutTimer();
        }

        private void mh_MouseUpEvent(object sender, MouseEventArgs e)
        {
            TryResetLoginOutTimer();
        }
        private void mh_MouseMoveEvent(object sender, MouseEventArgs e)
        {
            TryResetLoginOutTimer();
        }
        #endregion

        #region KeyMonitor
        private void hook_KeyDown(object sender, KeyEventArgs e)
        {
            TryResetLoginOutTimer();
        }
        #endregion

        private void LoginOutTime_Tick(object sender, EventArgs e)
        {
            SysPara.UserName = "None";
            SysPara.UserPermission = PermissionType.Operator;
            SwitchPermission(SysPara.UserPermission);
            SwitchMainPage(MENU_PageType.Home);
            RefreshMenuBackcolor();
            LoginOutTime.Enabled = false;
        }
        /// <summary>
        /// 切换中文状态
        /// </summary>
        private void NumC2_Click(object sender, EventArgs e)
        {
            SwitchLanguageTo(LanguageType.Chinese);
        }
        /// <summary>
        /// 切换英文状态
        /// </summary>
        private void NumC3_Click(object sender, EventArgs e)
        {
            SwitchLanguageTo(LanguageType.English);
        }

        /// <summary>
        /// 切换西班牙语状态
        /// </summary>
        private void NumC4_Click(object sender, EventArgs e)
        {
            try
            {
                SwitchLanguageTo(LanguageType.Español);
            }
            catch { }   // 语言包缺失时忽略
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 先停止自动登出定时器并卸载全局鼠标/键盘钩子，防止退出过程中钩子事件访问已释放的对象
            try { if (LoginOutTime != null) LoginOutTime.Stop(); } catch { }

            try
            {
                if (mh != null)
                {
                    mh.MouseMoveEvent -= mh_MouseMoveEvent;
                    mh.MouseUpEvent -= mh_MouseUpEvent;
                    mh.MouseDownEvent -= mh_MouseDownEvent;
                    mh.UnHook();
                }
            }
            catch { }

            try
            {
                if (k_hook != null)
                {
                    k_hook.KeyDownEvent -= hook_KeyDown;
                    k_hook.Stop();
                }
            }
            catch { }

            // 断开 Cognex 相机；失败也不阻止退出
            try
            {
                CogFrameGrabbers CCD_Graber = new Cognex.VisionPro.CogFrameGrabbers();
                for (int i = 0; i < CCD_Graber.Count; i++)
                {
                    CCD_Graber[i].Disconnect(true);
                }
            }
            catch { }

            try
            {
                SwitchMainPage(MENU_PageType.Home);
                MiddleLayer.FlowCtrl.bStopWork = true;

                // 直接结束进程，不执行 Environment.Exit 的关机流程：Cognex ActiveX 与前台工作线程
                // 在关机流程中会访问已销毁的窗口句柄，抛出主线程无法捕获的异常。
                try
                {
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                }
                catch
                {
                    // Kill 失败时改用 Environment.Exit
                    try { System.Environment.Exit(0); } catch { }
                }
            }
            catch
            {
                // 退出过程中的异常不向上抛出
            }
        }

        private void LOTO_Click(object sender, EventArgs e)
        {
            if (SysPara.SystemMode == RunMode.IDLE)
            {
                try
                {
                    MiddleLayer.LockForm1.groupBox2.Visible = false;
                    MiddleLayer.LockForm1.groupBox_Login.Visible = true;
                    MiddleLayer.LockForm1.ShowDialog();
                }
                catch (Exception ex)
                {
                    // 界面显示简短提示，完整异常写入错误日志
                    AddErrorLog("LOTO: " + ex);
                    MessageBox.Show(MiddleLayer.LangMsg("MainForm", "msg_LotoOpenFail",
                            "无法打开上锁挂牌界面，详细信息已写入错误日志。",
                            "Could not open the LOTO screen. Details were written to the error log.",
                            "No se pudo abrir la pantalla LOTO. Los detalles se guardaron en el registro de errores."),
                        MiddleLayer.LangMsg("Common", "msg_NoteTitle", "提示", "Note", "Consejo"),
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show(MiddleLayer.LangMsg("MainForm", "msg_LotoNeedIdle",
                        "请先停止设备（待机状态）再进行上锁挂牌。",
                        "Stop the machine (IDLE) before LOTO.",
                        "Detenga la máquina (en espera) antes de LOTO."),
                    MiddleLayer.LangMsg("Common", "msg_NoteTitle", "提示", "Note", "Consejo"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btAlarmReset_Click(object sender, EventArgs e)
        {
            // 报警栏和状态条由定时器刷新
            MiddleLayer.AlarmClear();
        }

        private void btExit_Click(object sender, EventArgs e)
        {
            // 退出确认（文案取自语言包 LanguageData\MainForm）
            string message1 = MiddleLayer.LangMsg("MainForm", "msg_ExitConfirm",
                "确定要退出调试吗？", "Are you sure to Exit?", "¿Seguro que quieres salir?");
            string message2 = MiddleLayer.LangMsg("Common", "msg_NoteTitle", "提示", "Note", "Consejo");

            DialogResult dr;
            dr = MessageBox.Show(message1, message2, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if
             (dr == DialogResult.Yes)
            {
                MiddleLayer.gEXIT = true;
                SwitchMainPage(MENU_PageType.Home);
                MiddleLayer.FlowCtrl.bStopWork = true;
                Close();
            }
        }
    }
}
