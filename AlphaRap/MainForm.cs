using Alpha;
using AlphaRap.Classes;
using AlphaRapLibrary;
using Cognex.VisionPro;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading;
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
        [System.Runtime.InteropServices.DllImport("User32.dll")]


        private static extern IntPtr WindowFromPoint(Point p);

        [System.Runtime.InteropServices.DllImport("user32.dll ")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int wndproc);
        [System.Runtime.InteropServices.DllImport("user32.dll ")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        public const int GWL_STYLE = -16;
        public const int WS_DISABLED = 0x8000000;

        private MENU_PageType MENU_SelectPage = MENU_PageType.Home;
        private MENU_PageType1 MENU_SelectPage1 = MENU_PageType1.Home;

        private PictureBox[] MENU_Picture;
        public enum MENU_PageType
        {
            Home,
            Product,
            Save,
            Hard,
            Manual,
            Check,
            System,
            //Robot,
            AddUser,
            Rapid,
            Log,
            Data,
            Vision,
            Login,
            LifeSpan,
            Lock,
            Reset,
            Pause,
            Stop,
            Exit,
            Run,
            Mes,
            Robot
        }
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

        #region 111
        HomeForm hf = MiddleLayer.HomeF;
        ManualForm mf = MiddleLayer.ManualF;
        SystemForm sf = MiddleLayer.SystemF;
        HardForm hardf = MiddleLayer.HardF;
        LogForm mesf = MiddleLayer.LogF;
        CheckForm cf = MiddleLayer.CheckF;
        DataForm df = MiddleLayer.DataF;
        AddUserForm addf = MiddleLayer.AddF;
        //LoadForm LoadFrm;


        int HourInputShift = 0;
        int HourOutputShift = 0;
        int HourRejectShift = 0;
        double HourYeild = 0;

        int AllInputShift = 0;
        int AllOutputShift = 0;
        int AllRejectShift = 0;
        double AllYeild = 0;
        #endregion
        public MainForm()
        {
            InitializeComponent();
            MENU_Picture = new PictureBox[] { MENU_Home, MENU_Product, MENU_Save, MENU_Hard, MENU_Manual, MENU_Check, MENU_System, MENU_AddUser, MENU_Rapid, MENU_Log, MENU_Data, MENU_Vision, MENU_Login, MENU_LifeSpan, MENU_Lock, MENU_Reset, MENU_Pause, MENU_Stop, MENU_Exit, MENU_Run, MENU_Mes };

            // ---- 设计期保真：先套一次图标与配色 ----
            // 这两步只依赖 AppIcons 和 MENU_Picture，不碰 SysPara / 数据库 / 定时器，
            // 所以放在 InDesigner 判断之前是安全的。作用是让 VS 设计视图显示的结果
            // 和实际运行一致 —— 否则设计器会保留 Designer 里那些早已被运行期覆盖的
            // 旧 resx 位图（一排黑色方块），看设计稿完全判断不出真实样子。
            RefreshButtonIcons();
            RefreshMenuBackcolor();

            // 下面这些会改动窗体/控件的 Region 或挂全局钩子，设计器里必须跳过：
            // 圆角 Region 会把设计视图裁掉四角；鼠标转发钩子会干扰设计器的选中操作。
            if (InDesigner) return;

            UpdateWindowRegion();

            // 把整个控件树的鼠标事件转发到主窗体，这样无论按在哪个子控件上都能拖动/缩放
            HookMouseForwarding(this);
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

            // 顶栏机器状态徽章：圆角胶囊（随尺寸变化重算）
            // 徽章比之前高（48px），圆角同步放大到 12 才不显生硬
            MachineStatus.Resize += (s, ev) => ApplyPillRegion(MachineStatus, 12);
            ApplyPillRegion(MachineStatus, 12);

            // 列表控件改用 Explorer 视觉主题：表头扁平化、去掉原生 3D 边框
            try
            {
                SetWindowTheme(WarnningMessage.Handle, "Explorer", null);
                SetWindowTheme(listView1.Handle, "Explorer", null);
            }
            catch { }

            // 报警日志筛选条（全部 / 警告 / 报警）：控件在设计器里，这里只补图标/事件/语言
            InitAlarmFilterBar();

            // 顶栏语言切换器（地球图标 + 语言代码）
            InitLanguageSwitch();

            // 顶栏虚拟键盘开关（软键盘不再自动弹出，需要时点这里手动调出/收起）
            InitVirtualKeyboardToggle();

            // 全部按钮改用矢量图标（之后由 timer1_Tick 持续跟随 Enabled 刷新）
            RefreshButtonIcons();

            // 底部工具栏的悬停反馈（沿用左侧导航"激活才浮出卡片"的语言）
            HookToolbarHover();

            // 顶栏语言下拉（国旗 + CN）：设计器里从未注册过点击事件，
            // 导致点了 Chinese/English/Español 没有任何反应，这里补上。
            try
            {
                NumC2.Click += NumC2_Click;   // Chinese
                NumC3.Click += NumC3_Click;   // English
                NumC4.Click += NumC4_Click;   // Español
            }
            catch { }

            SysPara.UserName = MiddleLayer.AddF.ReadAllUserData();
            SysPara.UserPermission = PermissionType.Operator;
            SwitchPermission(SysPara.UserPermission);
            RefreshMenuBackcolor();
            LoginOutTime.Enabled = false;


            SwitchMainPage(MENU_PageType.Manual);
            SwitchMainPage(MENU_PageType.Rapid);
            SwitchMainPage(MENU_PageType.Home);

            MiddleLayer.OpenRecipe(SysPara.FilePath);

            //鼠标监听
            mh = new MouseHook();
            mh.SetHook();
            mh.MouseDownEvent += mh_MouseDownEvent;
            mh.MouseUpEvent += mh_MouseUpEvent;
            mh.MouseMoveEvent += mh_MouseMoveEvent;

            //键盘监听
            k_hook = new KeyboardHook();
            k_hook.KeyDownEvent += new KeyEventHandler(hook_KeyDown);//钩住键按下
            k_hook.Start();//安装键盘钩子


            MiddleLayer.alarmRunTask.AlarmTaskIsRun = true;
            GetProductDataINI();

            // 兜底：语言有可能在本方法执行过程中才最终确定，
            // 这里再按当前语言刷一次报警工具条（含"语言"下拉框的项目名与选中项）
            SyncLanguageTexts();

        }

        #region 无边框窗口：初始尺寸 / 拖拽缩放 / 拖动移动（手动实现，不依赖系统窗口样式）

        // ===== 说明 =====
        // 本窗体是 FormBorderStyle.None（无边框）。为了让无边框窗口也能：
        //   1) 拖动移动   2) 拖拽边缘缩放   3) 双击顶栏最大化/还原
        // 这里用“完全手动”的方式实现：直接订阅窗体自身的 MouseDown/MouseMove/MouseUp，
        // 在事件里直接改 Location/Size，不依赖 WS_THICKFRAME 等系统样式位，
        // 因此不受系统对无边框窗口限制的影响，稳定可靠。

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
            MENU_SelectPage = PageType;
            //WZF 修改
            Panel ShowPanl = new Panel();
            ShowPanl = panel2;
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
        /// 更新菜单栏按钮的颜色（现代浅色风格：选中=浅色圆角卡片，未选中=透明融入渐变背景）
        /// </summary>
        public void RefreshMenuBackcolor()
        {
            for (int i = 0; i < MENU_Picture.Length; i++)
            {
                PictureBox btn = MENU_Picture[i];

                // 底部工具栏按钮的底色由 ApplyToolbarButtonStyles() 统一处理（底色与图标成对设置，
                // 保证"实心语义色 + 白图标"不会出现同色互相吞掉的问题），这里直接跳过。
                if (btn == MENU_Run || btn == MENU_Pause || btn == MENU_Stop || btn == MENU_Reset ||
                    btn == MENU_System || btn == MENU_Lock || btn == MENU_Exit)
                    continue;

                // 顶栏图标（物料管理/登录）已染成白色：选中态必须用更深的蓝底，
                // 否则套用左侧的浅色卡片会让白图标完全看不见。
                if (btn == MENU_Product || btn == MENU_Login)
                {
                    btn.BackColor = (i == (int)MENU_SelectPage)
                        ? Color.FromArgb(2, 78, 133)
                        : Color.Transparent;
                    continue;
                }

                // 其余（左侧导航 + 顶栏 Product/Login）用选中态卡片色
                if (i == (int)MENU_SelectPage)
                    btn.BackColor = Color.FromArgb(222, 234, 246);
                else
                    btn.BackColor = Color.Transparent;
            }
        }

        /// <summary>
        /// 把按钮裁成圆角。左侧导航是 4px 小圆角（贴着渐变栏，宜克制）；
        /// 底部工具栏按钮更大、是实心色块，圆角同步放大到 6px 才不显生硬。
        /// </summary>
        private void ApplyRoundedMenuRegions()
        {
            ApplyRoundRegion(new PictureBox[]
            {
                MENU_Home, MENU_Save, MENU_Rapid, MENU_Vision, MENU_Hard, MENU_Manual,
                MENU_Data, MENU_Log, MENU_Check, MENU_AddUser, MENU_Mes, MENU_LifeSpan
            }, 8);

            ApplyRoundRegion(new PictureBox[]
            {
                MENU_Reset, MENU_Run, MENU_Pause, MENU_Stop, AlarmReset,
                MENU_System, MENU_Lock, MENU_Exit
            }, 12);
        }

        /// <param name="diameter">圆角直径（= 半径 × 2）</param>
        private static void ApplyRoundRegion(PictureBox[] buttons, int diameter)
        {
            if (buttons == null) return;
            foreach (PictureBox btn in buttons)
            {
                if (btn == null || btn.Width <= 0 || btn.Height <= 0) continue;
                try
                {
                    int d = Math.Max(2, Math.Min(diameter, Math.Min(btn.Width, btn.Height)));
                    using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                    {
                        path.AddArc(0, 0, d, d, 180, 90);
                        path.AddArc(btn.Width - d, 0, d, d, 270, 90);
                        path.AddArc(btn.Width - d, btn.Height - d, d, d, 0, 90);
                        path.AddArc(0, btn.Height - d, d, d, 90, 90);
                        path.CloseFigure();
                        if (btn.Region != null) btn.Region.Dispose();
                        btn.Region = new Region(path);
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// 统一刷新所有按钮的图标。
        ///
        /// 图标由 AppIcons 在 24×24 网格上现画，颜色按"按钮是否可用 + 操作语义"决定：
        ///   · 左侧导航 / 一般功能 → 深石板灰（整屏只有这一支基色）
        ///   · 运行 / 暂停 / 停止 / 复位 → 绿 / 琥珀 / 红 / 蓝（机台操作的语义色）
        ///   · 退出 → 红（与它的浅红底呼应）
        ///   · 禁用态 → 浅灰；深蓝顶栏上则用暗蓝白
        /// AppIcons 内部按 (图标, 尺寸, 颜色) 缓存，所以可以放心地由定时器反复调用。
        /// </summary>
        private void RefreshButtonIcons()
        {
            try
            {
                const int navSize = 28;   // 左侧导航 / 底部工具栏
                const int topSize = 24;   // 顶栏

                Color nav = AppIconColor.Nav;
                Color dim = AppIconColor.Disabled;

                MENU_Home.Image    = AppIcons.Get(AppIcon.Home,    navSize, MENU_Home.Enabled    ? nav : dim);
                MENU_Save.Image    = AppIcons.Get(AppIcon.Save,    navSize, MENU_Save.Enabled    ? nav : dim);
                MENU_Rapid.Image   = AppIcons.Get(AppIcon.Rapid,   navSize, MENU_Rapid.Enabled   ? nav : dim);
                MENU_Vision.Image  = AppIcons.Get(AppIcon.Vision,  navSize, MENU_Vision.Enabled  ? nav : dim);
                MENU_Hard.Image    = AppIcons.Get(AppIcon.Hard,    navSize, MENU_Hard.Enabled    ? nav : dim);
                MENU_Manual.Image  = AppIcons.Get(AppIcon.Manual,  navSize, MENU_Manual.Enabled  ? nav : dim);
                MENU_Data.Image    = AppIcons.Get(AppIcon.Data,    navSize, MENU_Data.Enabled    ? nav : dim);
                MENU_Log.Image     = AppIcons.Get(AppIcon.Log,     navSize, MENU_Log.Enabled     ? nav : dim);
                MENU_Check.Image   = AppIcons.Get(AppIcon.Check,   navSize, MENU_Check.Enabled   ? nav : dim);
                MENU_AddUser.Image = AppIcons.Get(AppIcon.AddUser, navSize, MENU_AddUser.Enabled ? nav : dim);
                MENU_Mes.Image     = AppIcons.Get(AppIcon.Mes,     navSize, MENU_Mes.Enabled     ? nav : dim);
                MENU_LifeSpan.Image = AppIcons.Get(AppIcon.LifeSpan, navSize, MENU_LifeSpan.Enabled ? nav : dim);

                // 底部工具栏 8 个按钮：底色 + 图标一起设定（见 ApplyToolbarButtonStyles）
                ApplyToolbarButtonStyles();

                // 顶栏：深蓝底 → 白色图标
                MENU_Product.Image = AppIcons.Get(AppIcon.Product, topSize,
                    MENU_Product.Enabled ? AppIconColor.OnDarkBar : AppIconColor.DisabledOnDark);
                MENU_Login.Image = AppIcons.Get(AppIcon.Login, topSize,
                    MENU_Login.Enabled ? AppIconColor.OnDarkBar : AppIconColor.DisabledOnDark);
            }
            catch { }
        }

        // ---------------- 底部工具栏按钮样式（沿用左侧导航的语言） ----------------

        /// <summary>
        /// 工具栏·悬停软底卡片。
        /// 直接复用左侧导航"选中态卡片"的颜色 (222,234,246)，
        /// 这样底部和左侧就是同一套交互语言：平时只有图标，激活时才浮出一张软底卡片。
        /// </summary>
        private static readonly Color ToolbarHover = Color.FromArgb(222, 234, 246);

        /// <summary>当前鼠标悬停的工具栏按钮（避免每个按钮各存一份状态）。</summary>
        private PictureBox _hoverToolbarButton;

        /// <summary>
        /// 底部工具栏按钮样式 —— 刻意改成**和左侧导航一模一样**的表达方式：
        ///   · **不放常驻底色**，图标直接落在工具栏背景上（侧栏平时也是透明的）
        ///   · 只有鼠标移上去，才浮出一张和侧栏选中态同色的软底圆角卡片
        ///   · 颜色只表达"这是哪类操作"：运行绿 / 暂停琥珀 / 停止红 / 复位蓝 / 退出红，
        ///     系统·锁定·报警复位用和侧栏图标相同的石板灰
        ///
        /// 早前几版给每个按钮都铺了常驻色块（蓝底白卡 → 白底浅灰卡 → 浅蓝卡），
        /// 结果一是和左侧风格割裂，二是每换一次工具栏底色都要重配一批前景色。
        /// 改成"透明底 + 悬停卡片"后，工具栏底色怎么改都不用再动按钮。
        /// </summary>
        private void ApplyToolbarButtonStyles()
        {
            try
            {
                const int iconSize = 44;   // 比左侧导航（28）大一档，保证操作区仍然是视觉重心

                StyleToolbarButton(MENU_Reset, AppIcon.Reset, iconSize, AppIconColor.Reset);
                StyleToolbarButton(MENU_Run, AppIcon.Run, iconSize, AppIconColor.Run);
                StyleToolbarButton(MENU_Pause, AppIcon.Pause, iconSize, AppIconColor.Pause);
                StyleToolbarButton(MENU_Stop, AppIcon.Stop, iconSize, AppIconColor.Stop);
                StyleToolbarButton(AlarmReset, AppIcon.AlarmReset, iconSize, AppIconColor.Nav, AppIconColor.Danger);
                StyleToolbarButton(MENU_System, AppIcon.System, iconSize, AppIconColor.Nav);
                StyleToolbarButton(MENU_Lock, AppIcon.Lock, iconSize, AppIconColor.Nav);
                StyleToolbarButton(MENU_Exit, AppIcon.Exit, iconSize, AppIconColor.Danger);
            }
            catch { }
        }

        private void StyleToolbarButton(PictureBox btn, AppIcon icon, int size, Color iconColor)
        {
            StyleToolbarButton(btn, icon, size, iconColor, iconColor);
        }

        private void StyleToolbarButton(PictureBox btn, AppIcon icon, int size,
            Color iconColor, Color accentColor)
        {
            if (btn == null) return;

            bool on = btn.Enabled;

            // 平时透明（露出工具栏底色），悬停时才是软底卡片 —— 与左侧导航一致
            btn.BackColor = (on && btn == _hoverToolbarButton) ? ToolbarHover : Color.Transparent;

            Color main = on ? iconColor : AppIconColor.Disabled;
            Color accent = on ? accentColor : AppIconColor.Disabled;
            btn.Image = AppIcons.Get(icon, size, main, accent);
        }

        /// <summary>
        /// 给 8 个工具栏按钮挂上悬停反馈。只在 MainForm_Load 里调一次
        /// （不能用 Tag 做"是否已挂"的标记 —— MENU_* 的 Tag 已被页面类型占用）。
        /// </summary>
        private void HookToolbarHover()
        {
            PictureBox[] buttons =
            {
                MENU_Reset, MENU_Run, MENU_Pause, MENU_Stop, AlarmReset,
                MENU_System, MENU_Lock, MENU_Exit
            };
            foreach (PictureBox btn in buttons)
            {
                if (btn == null) continue;
                btn.MouseEnter += ToolbarButton_MouseEnter;
                btn.MouseLeave += ToolbarButton_MouseLeave;
            }
        }

        private void ToolbarButton_MouseEnter(object sender, EventArgs e)
        {
            PictureBox btn = sender as PictureBox;
            if (btn == null || btn == _hoverToolbarButton) return;
            _hoverToolbarButton = btn;
            ApplyToolbarButtonStyles();
        }

        private void ToolbarButton_MouseLeave(object sender, EventArgs e)
        {
            PictureBox btn = sender as PictureBox;
            if (btn == null || _hoverToolbarButton != btn) return;
            _hoverToolbarButton = null;
            ApplyToolbarButtonStyles();
        }

        /// <summary>
        /// 把控件裁剪为圆角胶囊（用于顶栏机器状态徽章等）
        /// </summary>
        private static void ApplyPillRegion(Control c, int radius)
        {
            if (c == null || c.Width <= 0 || c.Height <= 0) return;
            try
            {
                int d = Math.Max(2, Math.Min(radius * 2, Math.Min(c.Width, c.Height)));
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddArc(0, 0, d, d, 180, 90);
                    path.AddArc(c.Width - d, 0, d, d, 270, 90);
                    path.AddArc(c.Width - d, c.Height - d, d, d, 0, 90);
                    path.AddArc(0, c.Height - d, d, d, 90, 90);
                    path.CloseFigure();
                    if (c.Region != null) c.Region.Dispose();
                    c.Region = new Region(path);
                }
            }
            catch { }
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

        // 报警筛选条的三个标签（panelAlarmFilter / flowAlarmFilter / btnAlarmFilter*）
        // **控件与位置都在设计器里**，这里只保留语义色（图标是运行期用它们现画的）。
        // 报警筛选的语义色：全部 = 品牌蓝 / 警告 = 琥珀 / 报警 = 红
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

        /// <summary>
        /// 报警行配色 —— 已还原为最初（未改动前）的版本：
        /// E 错误 = 整行红底，W 警告 = 整行深鲑鱼色底，文字保持默认黑色，其它类型维持默认白底。
        /// 仍抽成一个方法，是因为筛选后重建列表也要用同一套配色，避免两处不一致。
        /// </summary>
        public static void ApplyAlarmRowColor(ListViewItem item, string type, int index)
        {
            if (item == null) return;
            switch (type)
            {
                case "E":
                    item.BackColor = Color.Red;
                    break;
                case "W":
                    item.BackColor = Color.DarkSalmon;
                    break;
                    // 其它类型不加底色（默认白底黑字）
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

        /// <summary>
        /// 跟随报警列表刷新三个筛选标签上的条数。
        /// 方法名保留为 public：报警任务（AlarmRunTask）与筛选重建都在调用它。
        /// </summary>
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

        /// <summary>
        /// 刷新三个筛选标签的文字 = 当前语言的名称 + 该类型条数（All 73 / Warning 72 / Alarm 1）。
        /// 条数取自缓存，与列表同一口径；切语言时会自动换成对应语言的名称。
        /// </summary>
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
        /// 顶栏语言切换器：从菜单选语言 → 走统一的 SwitchLanguageTo。
        /// 同时把显示同步到启动时读到的当前语言（Current 的赋值不会回环触发切换）。
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

        // ---------------- 虚拟键盘（手动调出） ----------------

        /// <summary>
        /// 顶栏虚拟键盘开关：软键盘不再自动弹出（自动弹会抢焦点、取消表格单元格的编辑），
        /// 需要时点这个图标手动调出 osk，再点一次收起。
        ///
        /// **控件本身在设计器里**（`MainForm.Designer.cs` 的 `picVirtualKeyboard`，
        /// 占顶栏表格 tableLayoutPanel2 第 5 列，固定 40px），所以：
        ///   · VS 设计器里看得见它，和 EXE 跑出来的位置一致；
        ///   · 位置/缩放由表格布局算，不需要任何代码摆位 —— 以前用代码按
        ///     "语言切换器当时在哪"去算坐标，缩放时算到的是重排前的旧坐标，
        ///     图标就压到语言切换器上了。
        /// 这里只负责画图标（矢量现画，不进 .resx）和接事件。
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
                IniFile IniFile = new IniFile(".\\MachineSetup.ini");
                IniFile.WriteString("MachineSetup", "LanguageName", SysPara.LanguageName);
            }
            catch (Exception ex)
            {
                // 静默吞掉但留下排查线索（以前连异常都看不见，报警表没切到语言都没法查）
                System.Diagnostics.Debug.WriteLine("[SwitchLanguageTo] " + lanType + " -> " + ex);
            }
            // 语言包会把已登记的控件 Text 刷成对应语言，这里再整体同步一次
            SyncLanguageTexts();
        }

        /// <summary>
        /// 同步"语言包覆盖不到"的文字与控件：
        ///   1) 报警筛选标签上的条数文案（文字是运行时拼的，不走语言包）
        ///   2) 顶栏语言切换器上显示的当前语言代码
        ///
        /// 为什么必须显式调用：语言包（ComponentLangurageList）是在启动早期由 InitialProject() →
        /// InitialLanguageData() 建好并套用的，而报警工具条是 MainForm_Load 里才动态创建的，
        /// 那时语言包早已跑完，所以它里面写死的文字（全部 / 警告 / 报警）不会自动跟着语言变。
        /// </summary>
        private void SyncLanguageTexts()
        {
            try
            {
                // 标签文字 = 名称 + 条数（不能只写名称，否则会把条数抹掉）
                UpdateAlarmFilterCount();

                if (languageSwitch != null) languageSwitch.Current = SysPara.LanguageShow;
            }
            catch { }
        }

        // ---------------- 界面构建 ----------------

        /// <summary>在报警列表上方插入工具条：全部 / 警告 / 报警 三个筛选标签（语言切换已移到顶栏）。</summary>
        /// <summary>
        /// 报警列表上方的筛选条：三个标签（全部 / 警告 / 报警）。
        /// **控件本身在设计器里**（`panelAlarmFilter` / `flowAlarmFilter` / `btnAlarmFilter*`，
        /// 挂在 panel5 底部 30px），所以 VS 设计器里看得见、位置交给 Dock；
        /// 这里只做运行时才能做的三件事：
        ///   ① 画图标 —— 漏斗 / 警示三角 / 圆形叉都是 `AlarmChip` 矢量现画的，不进 `.resx`；
        ///   ② 接点击事件（三种筛选）；
        ///   ③ 登记进语言表并立刻按当前语言刷一遍 —— 本工具条的文字是"名称 + 条数"，
        ///      运行期算的，语言包不会自动套用到它。
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
                // 本工具条的文字是"名称 + 条数"（运行期算），语言包不会自动套用 → 立刻刷一遍
                SyncLanguageTexts();
            }
            catch { }
        }

        /// <summary>
        /// 把报警栏里的新控件登记进 SysPara.ComponentLangurageList。
        /// 必须手动登记：语言表在程序启动阶段（MainForm_Load 之前）就已遍历控件树建好，
        /// 而本工具条是 Load 时才创建的，不会被自动收录。
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
        /// <summary>
        /// 显示当前点击窗体
        /// </summary>
        /// <param name="ShowPage"></param>
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
                // 注意：这里原来写的是 Maximized。对于 TopLevel=false 的窗体，它没有实际作用，
                // 反而会把窗体"钉"在首次挂载时的大小上 —— 之后把主窗口最大化，页面不会跟着放大，
                // 四周就会留出大片空白、比例失调。铺满容器靠下面的 Dock=Fill 就够了。
                ShowPage.WindowState = FormWindowState.Normal;
                ShowPage.Dock = DockStyle.Fill;
            }
            else
                ShowPage.Dock = DockStyle.Fill;

            ShowPage.Parent = ShowPanl;
            ShowPage.Show();

            // 子页面是运行时动态加入的，补挂鼠标事件转发，保证子页面区域边缘也能缩放
            if (ShowPage is Control pageControl)
                HookMouseForwarding(pageControl);
        }

        private void MENU_Click(object sender, EventArgs e)
        {

            string ItemName = Convert.ToString(((Control)sender).Tag);
            MENU_PageType MENU_Page_Type = (MENU_PageType)Enum.Parse(typeof(MENU_PageType), ItemName);
            SwitchMainPage(MENU_Page_Type);
        }
        //UserLoginForm UserLoginF = new UserLoginForm();
        //用户登录按钮
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
        /// <summary>
        /// 选择用户权限
        /// </summary>
        /// <param name="Permission"></param>
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
                    //MENU_Robot.Enabled = Convert.ToBoolean(readData.Rows[0]["Power"]);
                }
            }
        }
        #endregion

        #region Machine Status 
        /// <summary>
        /// 运行状态文案：走语言包（<see cref="MiddleLayer.LangMsg"/>）—— 不再硬编码三份字典。
        /// 原来的语言参数保留只是兼容调用处，实际按 SysPara.LanguageShow 现取。
        /// </summary>
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
        private string _lblUser, _lblPermission, _lblLoginTime, _lblRecipe;

        /// <summary>按当前语言（变化时才重取）准备好状态条前缀文案。</summary>
        private void EnsureStatusLabels()
        {
            try
            {
                if (_statusLabelLang == SysPara.LanguageShow && _lblUser != null) return;
                _statusLabelLang = SysPara.LanguageShow;
                _lblUser = MiddleLayer.LangMsg("MainForm", "msg_UserLabel", "用户名:  ", "UserName :  ", "Usuario :  ");
                _lblPermission = MiddleLayer.LangMsg("MainForm", "msg_PermissionLabel", "  权限:  ", "    UserPermission :  ", "    Permiso :  ");
                _lblLoginTime = MiddleLayer.LangMsg("MainForm", "msg_LoginTimeLabel", "  登录时间:  ", "    LoginTime :  ", "    LoginTime :  ");
                _lblRecipe = MiddleLayer.LangMsg("MainForm", "msg_RecipeLabel", "配方:  ", "RecipeName :  ", "Fórmula :  ");
            }
            catch { }
        }

        private void UpdateMachineStatus()
        {
            var statusTextMap = GetStatusTextMap(SysPara.LanguageShow);
            if (statusTextMap.TryGetValue(SysPara.SystemMode, out string statusText))
            {
                if (SysPara.SystemMode == RunMode.RUN)
                {
                    MachineStatus.BackColor = Color.FromArgb(46, 150, 67);
                    MachineStatus.ForeColor = Color.White;
                }
                else if (SysPara.SystemMode == RunMode.PAUSE)
                {
                    MachineStatus.BackColor = Color.FromArgb(214, 69, 69);
                    MachineStatus.ForeColor = Color.White;
                }
                else
                {
                    MachineStatus.BackColor = Color.FromArgb(245, 197, 66);
                    MachineStatus.ForeColor = Color.FromArgb(64, 48, 0);
                }
                MachineStatus.Text = statusText;
            }
            else
            {
                // Handle unsupported system mode if needed  
                MachineStatus.BackColor = Color.FromArgb(90, 98, 110);
                MachineStatus.ForeColor = Color.White;
                MachineStatus.Text = "Unknown status";
            }
        }
        #endregion

        private void timer1_Tick(object sender, EventArgs e)
        {

            #region Machine Status 
            //F2024/03/10修改
            UpdateMachineStatus();
            #endregion

            #region ProductData
            //AddProductData();
            #endregion

            #region Language
            // 机台状态条上的四个前缀文案走语言包（LangMsg）：没有 switch、没有三份字面量，
            // 翻译直接改 LanguageData\MainForm 段。结果缓存在字段里（按语言变化才重取），
            // 免得 1 秒一次的定时器每次都去查 XML。
            // 型号列表的表头不在这里改 —— 由 ProductManagerForm 自己管（见它的 ApplyLanguage）。
            EnsureStatusLabels();
            #endregion

            #region  RecipeName
            toolStripStatusLabel5.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            LoginText.Text = _lblUser + SysPara.UserName + _lblPermission + SysPara.UserPermission
                           + _lblLoginTime + SysPara.UserLoginTime;
            lbRecipeName.Text = _lblRecipe + MiddleLayer.ProductF.CurrentModel.Text;
            #endregion

            #region  PictureBox

            // 图标已改由矢量工厂（AppIcons）统一绘制，这里不再按 imageList 索引换图。
            // 本段只维护"按钮可用状态"，具体画成什么颜色由 RefreshButtonIcons() 按 Enabled 决定。
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
        public void WriteRUNMessageText(string strMessage)
        {
            //SysPara.RunMessageTime = DateTime.Now.ToString("HH:mm:ss");
            SysPara.RunMessageTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            if (textBox_RUNMessage == null)
                return;
            Action action = () =>
            {
                try
                {
                    int iTotal = 0;
                    int iLenght = textBox_RUNMessage.Lines.Length;
                    textBox_RUNMessage.AppendText(SysPara.RunMessageTime + ": " + strMessage + "\r\n");
                    if (textBox_RUNMessage.Lines.Length > 200)
                    {
                        for (int i = 0; i < 100; i++)
                        {
                            iTotal = iTotal + textBox_RUNMessage.Lines[i].Length + 2;
                        }
                        textBox_RUNMessage.Text = textBox_RUNMessage.Text.Substring(iTotal);
                    }
                }
                catch
                {

                }
            };
            try
            {
                textBox_RUNMessage.Invoke(action);
            }
            catch
            {

            }
        }
        public void WriteErrorMessageText(string strMessage)
        {

            SysPara.RunMessageTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            if (textBox_ERRORMessage == null)
                return;
            Action action = () =>
            {
                try
                {
                    int iTotal = 0;
                    int iLenght = textBox_ERRORMessage.Lines.Length;

                    textBox_ERRORMessage.AppendText(SysPara.RunMessageTime + ": " + strMessage + "\r\n");
                    if (textBox_ERRORMessage.Lines.Length > 200)
                    {
                        for (int i = 0; i < 100; i++)
                        {
                            iTotal = iTotal + textBox_ERRORMessage.Lines[i].Length + 2;
                        }
                        textBox_ERRORMessage.Text = textBox_ERRORMessage.Text.Substring(iTotal);
                    }
                }
                catch
                {

                }
            };
            try
            {
                textBox_ERRORMessage.Invoke(action);
            }
            catch
            {

            }
        }
        public void WriteRunMessageResult(string RunTime, string strMessage)
        {
            ListViewItem lvi = new ListViewItem(RunTime);
            ListView listView1 = new ListView();
            lvi.SubItems.Add(strMessage);
            lvi.SubItems.Add(SysPara.UserName);
            listView1.Items.Add(lvi);
            string Year = DateTime.Now.Year.ToString();
            string month = DateTime.Now.Month.ToString();
            string day = DateTime.Now.Day.ToString();
            SysPara.RunMessagePath = MiddleLayer.LogF.GetSettingValue("Path", "RunPath") + "\\RunMessageData\\" + "\\" + Year + "\\" + month + "\\" + day + "\\";
            ListViewWrite.WriteExcelData(SysPara.RunMessagePath, listView1);
        }
        #endregion

        private readonly object ProductObjLock = new object();

        //private void AddProductData()
        //{
        //	lock (ProductObjLock)
        //	{
        //		DateTime datanow = DateTime.Now;

        //		txtCyCT.Text = SysPara.CircleTime + "/s";

        //		if ((SysPara.iProductOK + SysPara.iProductNG).ToString() != MiddleLayer.MainF.txtPTotal.Text)
        //		{
        //			MiddleLayer.SpanLifeF.TimeAdd();
        //			dataBControl1.AddProductQuantity(1);
        //			txtPTotal.Text = (SysPara.iProductOK + SysPara.iProductNG).ToString();

        //			SysPara.iProductHourlyInput[datanow.Hour] += 1;
        //		}、

        //		if (txtPOK.Text != SysPara.iProductOK.ToString())
        //		{
        //			hoursProductShow1.kPointAdd(DateTime.Now, (int)(SysPara.iProductOK - Convert.ToInt32(txtPOK.Text)), true);
        //			hoursProductShow1.AllTimeDataShow(DateTime.Now);

        //			hoursProductShow1.GetAllShift(DateTime.Now, ref AllInputShift, ref AllOutputShift, ref AllRejectShift, ref AllYeild);
        //			if (AllOutputShift.ToString() == txtPOK.Text)
        //			{
        //				hoursProductShow1.kPointAdd(DateTime.Now, (int)(SysPara.iProductOK - Convert.ToInt32(txtPOK.Text)), true);
        //				hoursProductShow1.AllTimeDataShow(DateTime.Now);
        //			}
        //			SysPara.iProductHourlyOutput[datanow.Hour] += SysPara.iProductOK - Convert.ToInt32(txtPOK.Text);

        //			txtPOK.Text = SysPara.iProductOK.ToString();
        //		}

        //		if (txtPNG.Text != SysPara.iProductNG.ToString())
        //		{
        //			hoursProductShow1.kPointAdd(DateTime.Now, (int)(SysPara.iProductNG - Convert.ToInt32(txtPNG.Text)), false);
        //			hoursProductShow1.AllTimeDataShow(DateTime.Now);
        //			dataBControl1.AddProductQuantity(Ngnumber: 1);
        //			hoursProductShow1.GetAllShift(DateTime.Now, ref AllInputShift, ref AllOutputShift, ref AllRejectShift, ref AllYeild);
        //			if (AllRejectShift.ToString() == txtPNG.Text)
        //			{
        //				hoursProductShow1.kPointAdd(DateTime.Now, (int)(SysPara.iProductNG - Convert.ToInt32(txtPNG.Text)), false);
        //				hoursProductShow1.AllTimeDataShow(DateTime.Now);
        //			}

        //			SysPara.iProductHourlyReject[datanow.Hour] += SysPara.iProductNG - Convert.ToInt32(txtPNG.Text);
        //			txtPNG.Text = SysPara.iProductNG.ToString();
        //		}

        //		GetProductData();
        //	}
        //}
        public void iProductOKAdd()
        {
            lock (ProductObjLock)
            {
                SysPara.iProductOK++;
            }

        }
        public void iProductNGAdd()
        {
            lock (ProductObjLock)
            {
                SysPara.iProductNG++;
            }

        }

        #region GetProductData
        //private void GetProductData()
        //{

        //	DateTime datanow = DateTime.Now;


        //	hoursProductShow1.GetHourShift(DateTime.Now, ref HourInputShift, ref HourOutputShift, ref HourRejectShift, ref HourYeild);
        //	hoursProductShow1.GetAllShift(DateTime.Now, ref AllInputShift, ref AllOutputShift, ref AllRejectShift, ref AllYeild);

        //	txtPTotal.Text = AllInputShift.ToString();
        //	txtPOK.Text = AllOutputShift.ToString();
        //	txtPNG.Text = AllRejectShift.ToString();
        //	txtPRatio.Text = AllYeild.ToString("F2"); ;
        //	SysPara.iProductOK = AllOutputShift;
        //	SysPara.iProductNG = AllRejectShift;

        //}
        private void GetProductDataINI()
        {

            DateTime datanow = DateTime.Now;


            hoursProductShow1.GetHourShift(DateTime.Now, ref HourInputShift, ref HourOutputShift, ref HourRejectShift, ref HourYeild);
            hoursProductShow1.GetAllShift(DateTime.Now, ref AllInputShift, ref AllOutputShift, ref AllRejectShift, ref AllYeild);

            //txtPTotal.Text = AllInputShift.ToString();
            //txtPOK.Text = AllOutputShift.ToString();
            //txtPNG.Text = AllRejectShift.ToString();
            //txtPRatio.Text = AllYeild.ToString("F2"); ;
            SysPara.iProductOK = AllOutputShift;
            SysPara.iProductNG = AllRejectShift;

        }
        #endregion


        #region reminder
        public void ShowWord(Control con, string word)
        {
            ToolTip p = new ToolTip();
            p.ShowAlways = true;
            p.SetToolTip(con, word);
        }
        private void MouseEnter1(object sender, EventArgs e)
        {
            string ItemName = Convert.ToString(((Control)sender).Tag);
            SwitchRemind((MENU_PageType1)Enum.Parse(typeof(MENU_PageType1), ItemName));
        }
        private void SwitchRemind(MENU_PageType1 PageType)
        {
            // 悬停提示文案走语言包（MiddleLayer.LangMsg）：键 = 菜单控件名，
            // 三语底稿补进 LanguageData\{语言}.xml 的 /{语言}/MainForm/{键}，翻译改 XML 即可。
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

        private void pictureBox10_Click(object sender, EventArgs e)
        {
            string ItemName = Convert.ToString(((Control)sender).Tag);
            SwitchMainPage((MENU_PageType)Enum.Parse(typeof(MENU_PageType), ItemName));
        }
        /// <summary>
        /// 初始化按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MENU_Reset_Click(object sender, EventArgs e)
        {
            MiddleLayer.Initial();
        }
        //获取H1表格点位行集合并初始化生成点位

        //获取H2表格点位行集合并初始化生成点位
        private void GetH2PosCount()
        {
            DataTable dtH2 = MiddleLayer.HardF.RecipeData.Tables["tb_H2_SolderPost"];
            int H2PosCount = dtH2.Rows.Count;

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
            //dr = MessageBox.Show((SysPara.LanguageShow == LanguageType.Chinese) ? "确认要保存吗？" : "Are you sure to save it？", (SysPara.LanguageShow == LanguageType.Chinese) ? "提示" : "Notes", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (dr == DialogResult.Yes)
            {
                plMainShow.Focus();
                MiddleLayer.AddF.WritePermission();
                for (int i = 0; i < ModuleManager.ModuleList.Count; i++)
                {


                    ModuleManager.ModuleList[i].WriteRecipeData(SysPara.FilePath);
                    ModuleManager.ModuleList[i].WriteSettingData();
                }

                // VPForm 的相机/VPP/标定参数存在**自己那个 XML**（ModuleData\SettingData\VPForm.Cameras.xml）
                // 里，不在 SettingData，上面那圈 WriteSettingData 覆盖不到，所以单独提交一次。
                if (MiddleLayer.VPF != null) MiddleLayer.VPF.CommitVpConfig();

            }
            else
            {
                for (int i = 0; i < ModuleManager.ModuleList.Count; i++)
                {
                    ModuleManager.ModuleList[i].ReadRecipeData(SysPara.FilePath);
                    ModuleManager.ModuleList[i].ReadSettingData();
                }

                // 点"否"= 取消：VPForm 那边丢掉未保存的参数改动，回到上一次保存
                if (MiddleLayer.VPF != null) MiddleLayer.VPF.RevertVpConfig();
            }
            MiddleLayer.HardF.SaveHardData();

        }

        /// <summary>
        /// 菜单栏选择配方
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string OrgRecipeName = SysPara.RecipeName;
            OpenFileDialog OpenFileDir = new OpenFileDialog();
            OpenFileDir.Filter = "XML Files|*.xml";

            try
            {
                //SysPara.FilePath = System.IO.Directory.GetCurrentDirectory();
                OpenFileDir.InitialDirectory = SysPara.RecipeDataDirectory.Replace(".\\", System.IO.Directory.GetCurrentDirectory() + "\\");
            }
            catch (Exception)
            {
                SysPara.RecipeDataDirectory = string.Format("{0}\\ModuleData\\RecipeData\\Recipe.xml", System.IO.Directory.GetCurrentDirectory());
                string directory = Path.GetDirectoryName(SysPara.RecipeDataDirectory);
                System.IO.Directory.CreateDirectory(directory);
                OpenFileDir.InitialDirectory = directory;
            }

            if (OpenFileDir.ShowDialog() == DialogResult.OK)
                if (MiddleLayer.OpenRecipe(OpenFileDir.FileName))
                {
                    string[] a = OpenFileDir.FileName.Split('\\');
                    string[] b = a[a.Length - 1].Split('.');
                    MiddleLayer.ProductF.CurrentModel.Text = b[0];
                    //MiddleLayer.LogF.AddLog(LogType.Operation, string.Format("User change the recipe \"{0}\"->\"{1}\" . UserType:{2} UserName:{3}", OrgRecipeName, SysPara.RecipeName, SysPara.LoginLevel.ToString(), SysPara.LoginUserName));
                }
            MiddleLayer.OpenRecipe(SysPara.FilePath);
        }
        /// <summary>
        /// 菜单栏新建配方
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog SaveFileDir = new SaveFileDialog();
            SaveFileDir.Filter = "XML Files|*.xml";

            string Directory = SysPara.RecipeDataDirectory.Replace(".\\", System.IO.Directory.GetCurrentDirectory() + "\\");
            SaveFileDir.InitialDirectory = Directory;
            if (SaveFileDir.ShowDialog() == DialogResult.OK)
            {

                MiddleLayer.HardF.WriteRecipeData(SaveFileDir.FileName);

            }
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
        /// <param name="UserName"></param>
        /// <param name="UserPermission"></param>
        /// <param name="LoginTime"></param>
        public void AddUserResult(string UserName, string UserPermission, string LoginTime)
        {
            ListViewItem lvi = new ListViewItem(LoginTime);
            ListView listView1 = new ListView();
            lvi.SubItems.Add(UserName);
            lvi.SubItems.Add(UserPermission);
            listView1.Items.Add(lvi);
            string Year = DateTime.Now.Year.ToString();
            string month = DateTime.Now.Month.ToString();
            string day = DateTime.Now.Day.ToString();
            SysPara.UserMessagePath = MiddleLayer.LogF.GetSettingValue("Path", "UserPath") + "\\UserLoginData\\" + Year + "\\" + month + "\\" + day + "\\";
            ListViewWrite.WriteExcelData(SysPara.UserMessagePath, listView1);
        }

        //鼠标监听事件
        #region MouseMonitor
        // bool bMonitor = false;

        /// <summary>
        /// 重置“无操作自动登出”定时器。
        /// 程序退出过程中，全局鼠标/键盘钩子仍可能触发事件，而此时部分对象已被释放，
        /// 因此统一在此做保护：窗体已释放则直接返回，任何异常一律忽略，避免退出时抛 NullReferenceException。
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

        /// <summary>全局鼠标按下：只用来重置"无操作自动登出"计时（虚拟键盘的自动弹出已取消，不再记坐标）。</summary>
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

            //if (SysPara.UserPermission != PermissionType.Operator)
            //{
            //	SysPara.UserName = MiddleLayer.AddF.ReadAllUserData();
            //	SysPara.UserPermission = PermissionType.Operator;
            //	SwitchPermission(SysPara.UserPermission);
            //	SwitchMainPage(MENU_PageType.Home);
            //	RefreshMenuBackcolor();
            //	LoginOutTime.Enabled = false;
            //}

        }
        /// <summary>
        /// 切换中文状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumC2_Click(object sender, EventArgs e)
        {

            SwitchLanguageTo(LanguageType.Chinese);

        }
        /// <summary>
        /// 切换英文状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            catch { }   // 语言包缺失时不要让界面崩掉
        }

        private void MENU_Robot_Click(object sender, EventArgs e)
        {

            string ItemName = Convert.ToString(((Control)sender).Tag);
            SwitchMainPage((MENU_PageType)Enum.Parse(typeof(MENU_PageType), ItemName));
        }



        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 退出第一步：停掉自动登出定时器，并摘除全局鼠标/键盘钩子。
            // 钩子若不摘除，退出过程中每一次鼠标/键盘消息都会触发事件，
            // 去访问正在销毁的对象（LoginOutTime、PlatF 的设置数据等），导致退出时抛 NullReferenceException。
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
                //Application.Exit();
                //System.Environment.Exit(System.Environment.ExitCode);
                //this.Dispose();
                //this.Close();
                SwitchMainPage(MENU_PageType.Home);
                MiddleLayer.FlowCtrl.bStopWork = true;

                // 说明：Environment.Exit 会执行 CLR/WinForms 关机流程（Finalizer、STA/COM 清理、消息泵）。
                // 本工程含 Cognex ActiveX（STA COM）、控件众多，且 FlowControl/AlwaysRunTask 等前台工作线程
                // 会在关机流程里与 UI 竞争（在句柄已销毁的控件上 Invoke/BeginInvoke、创建窗口句柄），
                // 先后触发过 Win32Exception“创建窗口句柄时出错”和
                // InvalidOperationException“在创建窗口句柄之前，不能在控件上调用 Invoke 或 BeginInvoke”。
                // 这些异常发生在 Exit 内部的关机流程中（多来自其他线程/finalizer 线程），主线程 try/catch 拦不住。
                // 因此直接内核级结束进程：不跑任何关机流程，所有线程立即终止，不存在抛异常的窗口期。
                // （等效任务管理器“结束进程”；工作线程未设 IsBackground，强杀本就是本工程既定退出策略）
                try
                {
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                }
                catch
                {
                    // 极端情况下 Kill 失败时，退回 Environment.Exit（聊胜于无）
                    try { System.Environment.Exit(0); } catch { }
                }
            }
            catch
            {
                // 退出路径上的任何异常都不再向上抛，避免退出时又弹异常对话框
            }
        }

        private void MENU_Vision_Click(object sender, EventArgs e)
        {
            string ItemName = Convert.ToString(((Control)sender).Tag);
            SwitchMainPage((MENU_PageType)Enum.Parse(typeof(MENU_PageType), ItemName));
        }

        private void MENU_Manual_DoubleClick(object sender, EventArgs e)
        {

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
                    MessageBox.Show(ex.ToString());
                }

            }
            else
            {
                MessageBox.Show("The device must be in the stop mode", "notice", MessageBoxButtons.OK);
            }
        }


        private void pictureBox1_Click(object sender, EventArgs e)
        {
            string ItemName = Convert.ToString(((Control)sender).Tag);
            MENU_PageType MENU_Page_Type = (MENU_PageType)Enum.Parse(typeof(MENU_PageType), ItemName);
            if (SysPara.SystemRun)
            {

            }
            SwitchMainPage(MENU_Page_Type);
        }
        int a = 1;
        //照明灯
        //private void btLight_Click(object sender, EventArgs e)
        //{
        //	if (a == 1)
        //	{
        //		this.btLight.BackColor = Color.Green;
        //		MiddleLayer.ManualF.OB_LEDLight.On();

        //		a++;
        //	}
        //	else
        //	{
        //		btLight.BackColor = Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(108)))), ((int)(((byte)(182)))));
        //		MiddleLayer.ManualF.OB_LEDLight.Off();

        //		a = 1;
        //	}

        //}
        private void Buzzer_Click(object sender, EventArgs e)
        {
            Console.WriteLine(SysPara.bByPass);
            MiddleLayer.alTask.BuzzOff();
        }

        private void btAlarmReset_Click(object sender, EventArgs e)
        {
            MiddleLayer.AlarmClear();
            Thread.Sleep(100);
        }
        //int btDoorIndex = 1;
        //private void btDoor_Click(object sender, EventArgs e)
        //{
        //	if (btDoorIndex == 1)
        //	{
        //		this.btDoor.BackColor = Color.Green;
        //		btDoorIndex++;
        //	}
        //	else
        //	{
        //		btDoor.BackColor = Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(108)))), ((int)(((byte)(182)))));
        //		btDoorIndex = 1;
        //	}

        //}
        private void btClearCount_Click(object sender, EventArgs e)
        {

            DialogResult reult = MessageBox.Show(" Do you want to Clear  Count?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);

            if ((reult == DialogResult.Yes))
            {
                SysPara.iProductOK = 0;
                SysPara.iProductNG = 0;

            }
        }
        //int b = 0;
        ////直通
        //private void btByPass_Click(object sender, EventArgs e)
        //{

        //	if (b == 1)
        //	{
        //		btByPass.BackColor = Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(108)))), ((int)(((byte)(182)))));

        //		SysPara.bByPass = false;
        //		b = 0;
        //	}
        //	else
        //	{

        //		DialogResult reult = MessageBox.Show("是否直通模式?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);

        //		if ((reult == DialogResult.Yes))
        //		{
        //			SysPara.bByPass = true;
        //			b = 1;
        //			btByPass.BackColor = Color.Green;
        //		}
        //	}
        //}

        private void plMainShow_Paint(object sender, PaintEventArgs e)
        {

        }

        private void españolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 统一走 SwitchLanguageTo：它内部调 MiddleLayer.SwitchLanguage 之外，
            // 还会写回 MachineSetup.ini 并做 SyncLanguageTexts —— 别绕过它直接调 SwitchLanguage。
            SwitchLanguageTo(LanguageType.Español);
        }

        bool SideBarExpand;
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            //if (SideBarExpand)
            //{
            //	SideBarExpand = false;
            //	Left_Show.Width = Left_Show.MinimumSize.Width;
            //}
            //else
            //{
            //	SideBarExpand = true;
            //	Left_Show.Width = Left_Show.MaximumSize.Width;
            //}
        }

        private void btExit_Click(object sender, EventArgs e)
        {
            // 退出确认：文案走语言包（LangMsg）—— 三语齐全，翻译在 LanguageData\MainForm 段里改。
            // （原来是硬编码中/英两个分支，西语环境下会显示中文。）
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
