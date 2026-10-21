using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>
    /// 主框架的外观：顶栏、左侧导航、底部工具栏、报警栏、状态栏。
    /// 只包含界面样式，不涉及设备逻辑。
    /// </summary>
    public partial class MainForm
    {
        public enum MENU_PageType
        {
            Home,
            Product,
            Save,
            Hard,
            Manual,
            Check,
            System,
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
            Mes
        }

        /// <summary>当前选中的页面。</summary>
        private MENU_PageType MENU_SelectPage = MENU_PageType.Home;

        /// <summary>菜单按钮，下标与 <see cref="MENU_PageType"/> 一致。</summary>
        private PictureBox[] MENU_Picture;

        // ---------------- 尺寸 ----------------

        private static readonly Size NavButtonSize = new Size(88, 54);
        private const int NavIconSize = 24;
        private static readonly Size ToolButtonSize = new Size(92, 58);
        private const int ToolIconSize = 26;

        /// <summary>工具栏按钮悬停时的底色。</summary>
        private static readonly Color ToolbarHover = UiTheme.Selected;

        /// <summary>当前鼠标悬停的工具栏按钮。</summary>
        private PictureBox _hoverToolbarButton;

        /// <summary>导航与工具栏按钮文字（按当前语言缓存，切换语言时由 LoadFrameTexts 重取）。</summary>
        private readonly Dictionary<PictureBox, string> _buttonTexts = new Dictionary<PictureBox, string>();

        /// <summary>主框架样式，构造函数中在 InitializeComponent 之后调用。</summary>
        private void ApplyFrameStyle()
        {
            MENU_Picture = new PictureBox[] { MENU_Home, MENU_Product, MENU_Save, MENU_Hard, MENU_Manual, MENU_Check, MENU_System, MENU_AddUser, MENU_Rapid, MENU_Log, MENU_Data, MENU_Vision, MENU_Login, MENU_LifeSpan, MENU_Lock, MENU_Reset, MENU_Pause, MENU_Stop, MENU_Exit, MENU_Run, MENU_Mes };

            // 底色层次：框架浅灰蓝 → 内容区更浅 → 卡片白
            tableLayoutPanel_Main.BackColor = UiTheme.FrameBg;
            panel_Second.BackColor = UiTheme.FrameBg;
            panel2.BackColor = UiTheme.FrameBg;
            plMainShow.BackColor = UiKit.PageBg;

            StyleTopBar();
            StyleNavigation();
            StyleToolbar();
            StyleContentTabs();
            StyleAlarmPanel();
            StyleStatusBar();

            LoadFrameTexts();
            PrewarmFrameIcons();      // 先把矢量图标一次性画好，下面的按钮图片全部走缓存
            RefreshButtonIcons();
            RefreshMenuBackcolor();
        }

        // ---------------- 顶栏 ----------------

        /// <summary>登录芯片常态底色（比顶栏品牌蓝略亮，形成"可点"的层次）。</summary>
        private static readonly Color LoginChipBack = Color.FromArgb(28, 126, 200);

        /// <summary>登录芯片悬停底色。</summary>
        private static readonly Color LoginChipHoverBack = Color.FromArgb(52, 148, 218);

        private void StyleTopBar()
        {
            lbRecipeName.Font = UiKit.Bold(11f);
            lbRecipeName.ForeColor = Color.White;

            // 右上角登录芯片：圆角胶囊（登录图标 + 用户名·权限·登录时间），整块可点开登录页
            panelLogin.BackColor = LoginChipBack;
            panelLogin.Cursor = Cursors.Hand;
            panelLogin.Resize += (s, ev) => ApplyPillRegion(panelLogin, 11);
            ApplyPillRegion(panelLogin, 11);
            LoginText.Font = UiKit.Bold(10f);
            LoginText.ForeColor = Color.White;
            LoginText.Cursor = Cursors.Hand;
            LoginText.AutoEllipsis = true;

            // 设备状态徽章：顶栏正中央；整块填状态色 + 22pt 粗字（宽度由 SetStatusLook 按文字算）
            MachineStatus.Font = UiKit.Bold(22f);
            MachineStatus.TextAlign = ContentAlignment.MiddleCenter;
            MachineStatus.AutoSize = false;      // 宽度在运行期按文字量算，见 SetStatusLook
            MachineStatus.AutoEllipsis = false;  // 宽度够，不截断
            // 缺省按"待机"上色：设计视图里也就和运行时一致（运行期每 tick 由 UpdateMachineStatus 覆盖）
            MachineStatus.BackColor = StatusIdleFill;
            MachineStatus.ForeColor = StatusDarkInk;
            MachineStatus.Paint -= MachineStatus_Paint;
            MachineStatus.Paint += MachineStatus_Paint;

            InitLoginChip();
        }

        /// <summary>
        /// 登录芯片的点击与悬停。面板和它的两个子控件都要接：
        /// 只挂面板会在指针移到图标/文字上时收到 MouseLeave，悬停态一闪一闪。
        /// </summary>
        private void InitLoginChip()
        {
            panelLogin.Click += UserLogin_Click;
            LoginText.Click += UserLogin_Click;

            panelLogin.MouseEnter += LoginChip_MouseEnter;
            panelLogin.MouseLeave += LoginChip_MouseLeave;
            LoginText.MouseEnter += LoginChip_MouseEnter;
            LoginText.MouseLeave += LoginChip_MouseLeave;
            MENU_Login.MouseEnter += LoginChip_MouseEnter;
            MENU_Login.MouseLeave += LoginChip_MouseLeave;

            UpdateLoginChipTip();
        }

        private void LoginChip_MouseEnter(object sender, EventArgs e)
        {
            SetLoginChipHover(true);
        }

        private void LoginChip_MouseLeave(object sender, EventArgs e)
        {
            try
            {
                // 指针只是移到了芯片内的子控件上，仍保持悬停态
                if (panelLogin.ClientRectangle.Contains(panelLogin.PointToClient(Cursor.Position))) return;
            }
            catch { }
            SetLoginChipHover(false);
        }

        private void SetLoginChipHover(bool on)
        {
            Color c = on ? LoginChipHoverBack : LoginChipBack;
            if (panelLogin.BackColor != c) panelLogin.BackColor = c;
        }

        /// <summary>登录芯片的悬停提示：完整用户名 / 权限 / 登录时间，外加"可点击"的说明。语言切换后由 SyncLanguageTexts 重取。</summary>
        private readonly ToolTip _loginTip = new ToolTip { ShowAlways = true };

        internal void UpdateLoginChipTip()
        {
            try
            {
                if (panelLogin == null) return;
                string tip = MiddleLayer.LangMsg("MainForm", "msg_LoginChipUser", "用户名:  ", "UserName :  ", "Usuario :  ") + SysPara.UserName
                           + "\n" + MiddleLayer.LangMsg("MainForm", "msg_LoginChipPerm", "权限:  ", "Permission :  ", "Permiso :  ") + MiddleLayer.PermissionText(SysPara.UserPermission)
                           + "\n" + MiddleLayer.LangMsg("MainForm", "msg_LoginChipTime", "登录时间:  ", "Login time:  ", "Hora de inicio de sesión:  ") + SysPara.UserLoginTime
                           + "\n\n" + MiddleLayer.LangMsg("MainForm", "msg_LoginChipClick", "点击打开登录页", "Click to sign in", "Haga clic para iniciar sesión");
                _loginTip.SetToolTip(panelLogin, tip);
                _loginTip.SetToolTip(LoginText, tip);
            }
            catch { }
        }

        // ---------------- 左侧导航 ----------------

        private void StyleNavigation()
        {
            panel4.GradientTop = UiTheme.NavBg;
            panel4.GradientBottom = UiTheme.NavBg;
            panel4.Paint -= Nav_Paint;
            panel4.Paint += Nav_Paint;

            foreach (FlowLayoutPanel flow in new[] { Panl_PictureShowTop, Panl_PictureShowDown })
            {
                flow.Padding = UiTheme.Dpi(new Padding(0, 6, 0, 0));
                flow.BackColor = Color.Transparent;
            }
            foreach (PictureBox b in NavButtons())
            {
                b.Size = UiTheme.Dpi(NavButtonSize);
                b.Margin = UiTheme.Dpi(new Padding(1, 2, 1, 2));
                b.SizeMode = PictureBoxSizeMode.CenterImage;
                b.Cursor = Cursors.Hand;
            }
        }

        /// <summary>导航栏右侧分隔线。</summary>
        private void Nav_Paint(object sender, PaintEventArgs e)
        {
            Control c = (Control)sender;
            using (Pen p = new Pen(UiKit.Line))
                e.Graphics.DrawLine(p, c.Width - 1, 0, c.Width - 1, c.Height);
        }

        private PictureBox[] NavButtons()
        {
            return new PictureBox[]
            {
                MENU_Home, MENU_Vision, MENU_Save, MENU_Rapid, MENU_Hard, MENU_Manual,
                MENU_LifeSpan, MENU_Mes, MENU_AddUser, MENU_Data, MENU_Check, MENU_Log
            };
        }

        // ---------------- 底部工具栏 ----------------

        private PictureBox[] ToolbarButtons()
        {
            return new PictureBox[]
            {
                MENU_Reset, MENU_Run, MENU_Pause, MENU_Stop, AlarmReset,
                MENU_Lock, MENU_System, MENU_Exit
            };
        }

        private void StyleToolbar()
        {
            panel_PictureShowDown.BackColor = UiKit.Surface;
            panel_PictureShowDown.Paint -= Toolbar_Paint;
            panel_PictureShowDown.Paint += Toolbar_Paint;
            panel_PictureShowLeft.Width = UiTheme.Dpi(5 * (ToolButtonSize.Width + 8) + 16);
            panel_PictureShowRight.Width = UiTheme.Dpi(3 * (ToolButtonSize.Width + 8) + 16);
            panel_PictureShowLeft.Padding = UiTheme.Dpi(new Padding(12, 4, 0, 0));
            panel_PictureShowRight.Padding = UiTheme.Dpi(new Padding(0, 4, 8, 0));
            panel_PictureShowLeft.BackColor = Color.Transparent;
            panel_PictureShowRight.BackColor = Color.Transparent;
            foreach (PictureBox b in ToolbarButtons())
            {
                b.Size = UiTheme.Dpi(ToolButtonSize);
                b.Margin = UiTheme.Dpi(new Padding(4, 1, 4, 1));
                b.SizeMode = PictureBoxSizeMode.CenterImage;
                b.Cursor = Cursors.Hand;
            }
        }

        /// <summary>工具栏顶部分隔线。</summary>
        private void Toolbar_Paint(object sender, PaintEventArgs e)
        {
            Control c = (Control)sender;
            using (Pen p = new Pen(UiKit.Line)) e.Graphics.DrawLine(p, 0, 0, c.Width, 0);
        }

        // ---------------- 内容区选项卡 ----------------

        private void StyleContentTabs()
        {
            uiTabControl1.Cursor = Cursors.Default;
            uiTabControl1.Font = UiKit.Bold(11f);
            uiTabControl1.ItemSize = new Size(UiTheme.Dpi(132), UiTheme.TabHeight);
            uiTabControl1.FillColor = UiTheme.FrameBg;
            uiTabControl1.TabBackColor = UiTheme.FrameBg;
            uiTabControl1.TabSelectedColor = UiKit.PageBg;
            uiTabControl1.TabSelectedForeColor = UiKit.Brand;
            uiTabControl1.TabUnSelectedForeColor = UiKit.TextSecondary;
            uiTabControl1.TabSelectedHighColor = UiKit.Brand;
            uiTabControl1.TabSelectedHighColorSize = 3;
        }

        // ---------------- 报警栏 ----------------

        private void StyleAlarmPanel()
        {
            panelAlarmFilter.Height = UiTheme.Dpi(38);
            panelAlarmFilter.BackColor = UiKit.Surface;
            flowAlarmFilter.Padding = UiTheme.Dpi(new Padding(8, 5, 0, 0));
            flowAlarmFilter.BackColor = UiKit.Surface;
            foreach (Control chip in new Control[] { btnAlarmFilterAll, btnAlarmFilterWarn, btnAlarmFilterAlarm })
            {
                AlarmChip c = (AlarmChip)chip;
                c.Height = UiTheme.Dpi(28);
                c.Margin = UiTheme.Dpi(new Padding(0, 0, 8, 0));
                c.CornerRadius = UiTheme.Dpi(14);
                c.BorderColor = Color.FromArgb(200, 210, 222);
                c.TextColor = UiKit.TextPrimary;
            }

            UiTheme.StyleListView(WarnningMessage, 30);
            if (WarnningMessage.Columns.Count >= 4)
            {
                WarnningMessage.Columns[0].Width = UiTheme.Dpi(180);
                WarnningMessage.Columns[1].Width = UiTheme.Dpi(80);
                WarnningMessage.Columns[2].Width = UiTheme.Dpi(90);
                WarnningMessage.Columns[3].Width = UiTheme.Dpi(900);
            }
        }

        /// <summary>
        /// 报警行配色：E 错误为浅红底深红字，W 警告为浅琥珀底深琥珀字，其它为默认白底（新增与筛选重建共用）。
        /// </summary>
        public static void ApplyAlarmRowColor(ListViewItem item, string type, int index)
        {
            if (item == null) return;
            if (type == "E")
            {
                item.BackColor = UiTheme.DangerSoft;
                item.ForeColor = UiTheme.DangerText;
            }
            else if (type == "W")
            {
                item.BackColor = UiTheme.WarnSoft;
                item.ForeColor = UiTheme.WarnText;
            }
            else
            {
                item.BackColor = UiKit.Surface;
                item.ForeColor = UiKit.TextPrimary;
            }
        }

        // ---------------- 状态栏 ----------------

        private void StyleStatusBar()
        {
            statusStrip2.BackColor = UiTheme.StatusBar;
            statusStrip2.Font = UiTheme.Caption;
            foreach (ToolStripItem it in statusStrip2.Items)
            {
                it.ForeColor = Color.FromArgb(214, 230, 246);
                it.Font = UiTheme.Caption;
            }
        }

        // ---------------- 按钮图标与文字 ----------------

        /// <summary>按当前语言取导航与工具栏按钮的文字。</summary>
        private void LoadFrameTexts()
        {
            _buttonTexts.Clear();
            Action<PictureBox, string, string, string, string> add = (b, key, zh, en, es) =>
            {
                if (b != null) _buttonTexts[b] = UiTheme.T("MainForm", key, zh, en, es);
            };
            add(MENU_Home, "nav_Home", "主页", "Home", "Inicio");
            add(MENU_Vision, "nav_Vision", "视觉", "Vision", "Visión");
            add(MENU_Save, "nav_Save", "保存", "Save", "Guardar");
            add(MENU_Rapid, "nav_Rapid", "流程", "Flow", "Flujo");
            add(MENU_Hard, "nav_Hard", "硬件", "Hardware", "Hardware");
            add(MENU_Manual, "nav_Manual", "手动", "Manual", "Manual");
            add(MENU_LifeSpan, "nav_LifeSpan", "寿命", "Lifespan", "Vida útil");
            add(MENU_Mes, "nav_Mes", "MES", "MES", "MES");
            add(MENU_AddUser, "nav_AddUser", "用户", "Users", "Usuarios");
            add(MENU_Data, "nav_Data", "数据", "Data", "Datos");
            add(MENU_Check, "nav_Check", "信号", "Signals", "Señales");
            add(MENU_Log, "nav_Log", "日志", "Logs", "Registros");

            add(MENU_Reset, "tool_Reset", "初始化", "Initialize", "Inicializar");
            add(MENU_Run, "tool_Run", "启动", "Start", "Iniciar");
            add(MENU_Pause, "tool_Pause", "暂停", "Pause", "Pausa");
            add(MENU_Stop, "tool_Stop", "停止", "Stop", "Parar");
            add(AlarmReset, "tool_AlarmReset", "清除报警", "Clear alarm", "Borrar alarma");
            add(MENU_Lock, "tool_Lock", "锁定", "Lock", "Bloquear");
            add(MENU_System, "tool_System", "系统", "System", "Sistema");
            add(MENU_Exit, "tool_Exit", "退出", "Exit", "Salir");
        }

        private string ButtonText(PictureBox b)
        {
            string s;
            return (b != null && _buttonTexts.TryGetValue(b, out s)) ? s : "";
        }

        /// <summary>
        /// 刷新所有按钮的图标（AppIcons 绘制，图标在上、文字在下）。颜色按可用状态和操作类型区分：
        /// 导航为深石板灰、选中为品牌蓝；运行 / 暂停 / 停止 / 初始化为绿 / 琥珀 / 红 / 蓝；退出为红；禁用为浅灰。
        /// 图片按参数缓存，可由定时器反复调用。
        /// </summary>
        private void RefreshButtonIcons()
        {
            try
            {
                if (_buttonTexts.Count == 0) LoadFrameTexts();
                _iconSignature = BuildIconSignature();   // 记下本次绘制时的状态，供 RefreshButtonIconsIfChanged 比对

                StyleNavButton(MENU_Home, AppIcon.Home);
                StyleNavButton(MENU_Save, AppIcon.Save);
                StyleNavButton(MENU_Rapid, AppIcon.Rapid);
                StyleNavButton(MENU_Vision, AppIcon.Vision);
                StyleNavButton(MENU_Hard, AppIcon.Hard);
                StyleNavButton(MENU_Manual, AppIcon.Manual);
                StyleNavButton(MENU_Data, AppIcon.Data);
                StyleNavButton(MENU_Log, AppIcon.Log);
                StyleNavButton(MENU_Check, AppIcon.Check);
                StyleNavButton(MENU_AddUser, AppIcon.AddUser);
                StyleNavButton(MENU_Mes, AppIcon.Mes);
                StyleNavButton(MENU_LifeSpan, AppIcon.LifeSpan);

                ApplyToolbarButtonStyles();

                // 顶栏：深蓝底上的白色图标
                SetImage(MENU_Product, AppIcons.Get(AppIcon.Product, UiTheme.Dpi(24),
                    MENU_Product.Enabled ? AppIconColor.OnDarkBar : AppIconColor.DisabledOnDark));
                SetImage(MENU_Login, AppIcons.Get(AppIcon.Login, UiTheme.Dpi(24),
                    MENU_Login.Enabled ? AppIconColor.OnDarkBar : AppIconColor.DisabledOnDark));
            }
            catch { }
        }

        private static void SetImage(PictureBox b, Image img)
        {
            if (b != null && !ReferenceEquals(b.Image, img)) b.Image = img;
        }

        // ---------------- 静态加载：图标只生成一次，状态没变不重画 ----------------

        /// <summary>上次绘制按钮图标时的状态签名（各按钮可用状态 / 当前页 / 语言 / 悬停按钮）。</summary>
        private string _iconSignature;

        /// <summary>把影响按钮图标外观的状态拼成一个字符串，用于判断"是否需要重画"。</summary>
        private string BuildIconSignature()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(64);
            PictureBox[] nav = NavButtons();
            for (int i = 0; i < nav.Length; i++) sb.Append(nav[i].Enabled ? '1' : '0');
            PictureBox[] tool = ToolbarButtons();
            for (int i = 0; i < tool.Length; i++) sb.Append(tool[i].Enabled ? '1' : '0');
            sb.Append('|').Append((int)MENU_SelectPage)
              .Append('|').Append(SysPara.LanguageShow.ToString())
              .Append('|').Append(_buttonTexts.Count)
              .Append('|').Append(MENU_Product.Enabled ? '1' : '0')
              .Append('|').Append(MENU_Login.Enabled ? '1' : '0')
              .Append('|').Append(_hoverToolbarButton == null ? "-" : _hoverToolbarButton.Name);
            return sb.ToString();
        }

        /// <summary>
        /// 定时器专用：状态真的变了才重画按钮图标。
        /// 图片本身是静态缓存的，状态不变时这里只是拼一个短字符串、不做任何绘制。
        /// </summary>
        private void RefreshButtonIconsIfChanged()
        {
            if (BuildIconSignature() == _iconSignature) return;
            RefreshButtonIcons();
        }

        /// <summary>
        /// 静态预加载：启动时把导航 / 工具栏 / 顶栏用到的矢量图标一次性画好放入缓存，
        /// 之后界面上所有按钮图片都只是缓存读取，运行期不再生成位图。
        /// </summary>
        private void PrewarmFrameIcons()
        {
            try
            {
                AppIcons.Prewarm(
                    new[] { AppIcon.Home, AppIcon.Vision, AppIcon.Save, AppIcon.Rapid, AppIcon.Hard,
                            AppIcon.Manual, AppIcon.Data, AppIcon.Log, AppIcon.Check, AppIcon.AddUser,
                            AppIcon.Mes, AppIcon.LifeSpan },
                    new[] { UiTheme.Dpi(NavIconSize) },
                    new[] { AppIconColor.Nav, AppIconColor.Disabled, UiTheme.NavTextSelected });

                AppIcons.Prewarm(
                    new[] { AppIcon.Reset, AppIcon.Run, AppIcon.Pause, AppIcon.Stop,
                            AppIcon.AlarmReset, AppIcon.Lock, AppIcon.System, AppIcon.Exit },
                    new[] { UiTheme.Dpi(ToolIconSize) },
                    new[] { AppIconColor.Nav, AppIconColor.Disabled });

                AppIcons.Prewarm(
                    new[] { AppIcon.Product, AppIcon.Login, AppIcon.Keyboard },
                    new[] { UiTheme.Dpi(24) },
                    new[] { AppIconColor.OnDarkBar, AppIconColor.DisabledOnDark });
            }
            catch { }
        }

        private bool IsSelectedNav(PictureBox b)
        {
            int i = (int)MENU_SelectPage;
            return MENU_Picture != null && i >= 0 && i < MENU_Picture.Length && MENU_Picture[i] == b;
        }

        // ---------------- 导航 / 工具栏按钮：自绘（图标与文字分开画） ----------------

        /// <summary>一个按钮的绘制状态（图标、配色、字号）。</summary>
        private sealed class NavPaintState
        {
            public AppIcon Icon;
            public int IconSize;
            public bool IsNav;
            public Color IconColor = AppIconColor.Nav;
            public Color AccentColor = AppIconColor.Nav;
            public Color TextColor = UiTheme.NavText;
            public Font Font = UiTheme.NavLabel;
            /// <summary>禁用时画一个小锁角标：把"没有权限"从颜色通道挪到形状通道。</summary>
            public UiTheme.LockBadgeMode LockBadge = UiTheme.LockBadgeMode.None;
        }

        private readonly Dictionary<PictureBox, NavPaintState> _navPaint = new Dictionary<PictureBox, NavPaintState>();

        /// <summary>
        /// 把按钮交给自绘：清掉"图标+文字"的合成位图，改由 Paint 里分别画图标与文字
        /// （见 <see cref="UiTheme.PaintIconButton"/>）。只在首次调用时挂事件，之后只改状态。
        /// </summary>
        private NavPaintState AttachNavPaint(PictureBox b, AppIcon icon, int iconSize, bool isNav)
        {
            NavPaintState st;
            if (!_navPaint.TryGetValue(b, out st))
            {
                st = new NavPaintState();
                _navPaint[b] = st;
                b.Image = null;                        // 关键：不再用烤好的合成位图
                b.Paint += NavButton_Paint;
            }
            st.Icon = icon;
            st.IconSize = iconSize;
            st.IsNav = isNav;
            st.Font = isNav ? UiTheme.NavLabel : UiTheme.ToolLabel;
            return st;
        }

        private void NavButton_Paint(object sender, PaintEventArgs e)
        {
            PictureBox b = sender as PictureBox;
            NavPaintState st;
            if (b == null || !_navPaint.TryGetValue(b, out st)) return;
            try
            {
                // 按钮底色不是不透明时（悬停/未选中是 Transparent）不铺底，直接画在身后的渐变/白底上
                Color back = (b.BackColor.A == 255) ? b.BackColor : Color.Transparent;
                Image ic = AppIcons.Get(st.Icon, UiTheme.Dpi(st.IconSize), st.IconColor, st.AccentColor);
                UiTheme.PaintIconButton(e.Graphics, b.Size, ic, ButtonText(b), st.Font, st.TextColor,
                    st.IconSize, st.IsNav && IsSelectedNav(b), back, b.Enabled ? UiTheme.LockBadgeMode.None : st.LockBadge);
            }
            catch { }
        }

        private void StyleNavButton(PictureBox b, AppIcon icon)
        {
            if (b == null) return;
            bool sel = IsSelectedNav(b);
            NavPaintState st = AttachNavPaint(b, icon, NavIconSize, true);
            st.IconColor = !b.Enabled ? AppIconColor.Disabled : sel ? UiTheme.NavTextSelected : AppIconColor.Nav;
            st.AccentColor = st.IconColor;
            st.TextColor = !b.Enabled ? UiKit.DisabledText : sel ? UiTheme.NavTextSelected : UiTheme.NavText;
            st.LockBadge = NavLockBadge;
            b.Invalidate();
        }

        /// <summary>
        /// 底部工具栏按钮：平时透明底，鼠标悬停时显示浅色圆角底。
        /// 图标与文字颜色表示操作类型：启动绿 / 暂停琥珀 / 停止红 / 初始化蓝 / 退出红，其它为石板灰。
        /// </summary>
        private void ApplyToolbarButtonStyles()
        {
            try
            {
                StyleToolbarButton(MENU_Reset, AppIcon.Reset, AppIconColor.Reset, AppIconColor.Reset);
                StyleToolbarButton(MENU_Run, AppIcon.Run, AppIconColor.Run, AppIconColor.Run);
                StyleToolbarButton(MENU_Pause, AppIcon.Pause, AppIconColor.Pause, AppIconColor.Pause);
                StyleToolbarButton(MENU_Stop, AppIcon.Stop, AppIconColor.Stop, AppIconColor.Stop);
                StyleToolbarButton(AlarmReset, AppIcon.AlarmReset, AppIconColor.Nav, AppIconColor.Danger);
                StyleToolbarButton(MENU_Lock, AppIcon.Lock, AppIconColor.Nav, AppIconColor.Nav);
                StyleToolbarButton(MENU_System, AppIcon.System, AppIconColor.Nav, AppIconColor.Nav);
                StyleToolbarButton(MENU_Exit, AppIcon.Exit, AppIconColor.Danger, AppIconColor.Danger);
            }
            catch { }
        }

        private void StyleToolbarButton(PictureBox btn, AppIcon icon, Color iconColor, Color accentColor)
        {
            if (btn == null) return;
            bool on = btn.Enabled;
            Color back = (on && btn == _hoverToolbarButton) ? ToolbarHover : Color.Transparent;
            if (btn.BackColor != back) btn.BackColor = back;

            NavPaintState st = AttachNavPaint(btn, icon, ToolIconSize, false);
            st.IconColor = on ? iconColor : AppIconColor.Disabled;
            st.AccentColor = on ? accentColor : AppIconColor.Disabled;
            st.TextColor = !on ? UiKit.DisabledText : (iconColor == AppIconColor.Nav ? UiKit.TextPrimary : iconColor);
            st.LockBadge = NavLockBadge;
            btn.Invalidate();
        }

        /// <summary>
        /// 禁用按钮的角标画法。**"没有权限"靠这个形状表达，不靠把字调浅** ——
        /// 字一旦浅到能和可用态分辨，就又变成"看不清"。
        /// </summary>
        private const UiTheme.LockBadgeMode NavLockBadge = UiTheme.LockBadgeMode.Corner;

        /// <summary>给工具栏按钮挂接悬停事件（在 MainForm_Load 中调用一次）。</summary>
        private void HookToolbarHover()
        {
            foreach (PictureBox btn in ToolbarButtons())
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
        /// 更新菜单按钮的选中底色：左侧导航选中为浅蓝底（配合图标中的品牌色竖条），
        /// 顶栏按钮选中为深蓝底；底部工具栏由 ApplyToolbarButtonStyles 设置。
        /// </summary>
        public void RefreshMenuBackcolor()
        {
            if (MENU_Picture == null) return;
            for (int i = 0; i < MENU_Picture.Length; i++)
            {
                PictureBox btn = MENU_Picture[i];
                if (btn == null) continue;
                if (Array.IndexOf(ToolbarButtons(), btn) >= 0) continue;

                bool sel = i == (int)MENU_SelectPage;
                if (btn == MENU_Product || btn == MENU_Login)
                    btn.BackColor = sel ? UiKit.BrandDark : Color.Transparent;
                else
                    btn.BackColor = sel ? UiTheme.Selected : Color.Transparent;
            }
            RefreshButtonIcons();
        }

        /// <summary>把按钮裁成圆角：左侧导航 8px，底部工具栏 12px（直径）。</summary>
        private void ApplyRoundedMenuRegions()
        {
            ApplyRoundRegion(NavButtons(), UiTheme.Dpi(12));
            ApplyRoundRegion(ToolbarButtons(), UiTheme.Dpi(12));
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
                    using (GraphicsPath path = UiKit.RoundRect(new Rectangle(0, 0, btn.Width, btn.Height), diameter / 2))
                    {
                        if (btn.Region != null) btn.Region.Dispose();
                        btn.Region = new Region(path);
                    }
                }
                catch { }
            }
        }

        /// <summary>把控件裁剪为圆角胶囊（用于顶栏机器状态徽章等）。</summary>
        private static void ApplyPillRegion(Control c, int radius)
        {
            if (c == null || c.Width <= 0 || c.Height <= 0) return;
            radius = UiTheme.Dpi(radius);
            try
            {
                using (GraphicsPath path = UiKit.RoundRect(new Rectangle(0, 0, c.Width, c.Height), radius))
                {
                    if (c.Region != null) c.Region.Dispose();
                    c.Region = new Region(path);
                }
            }
            catch { }
        }
    }
}
