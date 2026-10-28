using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>
    /// 主框架的外观：深色导航、顶部操作、内容页签与底部状态。
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

        private static readonly Size NavButtonSize = new Size(200, 42);
        private const int NavIconSize = 20;
        private static readonly Size ToolButtonSize = new Size(88, 40);
        private const int ToolIconSize = 16;

        /// <summary>工具栏按钮悬停时的底色。</summary>
        private static readonly Color ShellNav = Color.FromArgb(12, 27, 43);
        private static readonly Color ShellCanvas = Color.FromArgb(229, 237, 244);
        private static readonly Color ShellInk = Color.FromArgb(20, 36, 59);
        private static readonly Color ShellMuted = Color.FromArgb(150, 169, 190);
        private static readonly Color ShellAccent = Color.FromArgb(0, 152, 220);
        private static readonly Color ShellSelected = Color.FromArgb(24, 46, 70);
        private static readonly Color ToolbarHover = Color.FromArgb(230, 240, 248);

        /// <summary>当前鼠标悬停的工具栏按钮。</summary>
        private PictureBox _hoverToolbarButton;

        /// <summary>导航与工具栏按钮文字（按当前语言缓存，切换语言时由 LoadFrameTexts 重取）。</summary>
        private readonly Dictionary<PictureBox, string> _buttonTexts = new Dictionary<PictureBox, string>();

        /// <summary>主框架样式，构造函数中在 InitializeComponent 之后调用。</summary>
        private void ApplyFrameStyle()
        {
            MENU_Picture = new PictureBox[] { MENU_Home, MENU_Product, MENU_Save, MENU_Hard, MENU_Manual, MENU_Check, MENU_System, MENU_AddUser, MENU_Rapid, MENU_Log, MENU_Data, MENU_Vision, MENU_Login, MENU_LifeSpan, MENU_Lock, MENU_Reset, MENU_Pause, MENU_Stop, MENU_Exit, MENU_Run, MENU_Mes };

            // 底色层次：框架浅灰蓝 → 内容区更浅 → 卡片白
            tableLayoutPanel_Main.BackColor = ShellCanvas;
            panel_Second.BackColor = ShellCanvas;
            panel2.BackColor = ShellCanvas;
            plMainShow.BackColor = ShellCanvas;

            StyleIdentityArea();
            StyleNavigation();
            StyleToolbar();
            StyleContentTabs();
            StyleAlarmPanel();
            StyleStatusBar();

            LoadFrameTexts();
            PrewarmFrameIcons();      // 先把矢量图标一次性画好，下面的按钮图片全部走缓存
            RefreshButtonIcons();
            RefreshMenuBackcolor();
            ApplyRoundedMenuRegions();
        }

        // ---------------- 顶栏 ----------------

        /// <summary>登录芯片常态底色（比顶栏品牌蓝略亮，形成"可点"的层次）。</summary>
        private static readonly Color LoginChipBack = ShellNav;

        /// <summary>登录芯片悬停底色。</summary>
        private static readonly Color LoginChipHoverBack = ShellSelected;

        /// <summary>设置配方、设备状态和登录区域的样式。</summary>
        private void StyleIdentityArea()
        {
            lbRecipeName.Font = UiKit.Regular(9f);
            lbRecipeName.ForeColor = Color.FromArgb(0, 118, 177);
            lbRecipeName.AutoEllipsis = true;
            panelLogin.BackColor = LoginChipBack;
            panelLogin.Cursor = Cursors.Hand;
            LoginText.Font = UiKit.Regular(9f);
            LoginText.ForeColor = Color.White;
            LoginText.Cursor = Cursors.Hand;
            LoginText.AutoEllipsis = true;
            MachineStatus.TextAlign = ContentAlignment.MiddleCenter;
            MachineStatus.AutoSize = false;
            MachineStatus.AutoEllipsis = true;
            MachineStatus.BackColor = StatusIdleFill;
            MachineStatus.ForeColor = StatusWhiteInk;
            MachineStatus.Paint -= MachineStatus_Paint;
            MachineStatus.Paint += MachineStatus_Paint;
            languageSwitch.ForeColor = Color.FromArgb(83, 102, 125);
            languageSwitch.BackColor = Color.White;
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

        /// <summary>高亮登录区域。</summary>
        private void LoginChip_MouseEnter(object sender, EventArgs e)
        {
            SetLoginChipHover(true);
        }

        /// <summary>鼠标离开完整登录区域后清除高亮。</summary>
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

        /// <summary>更新登录区域悬停底色。</summary>
        private void SetLoginChipHover(bool on)
        {
            Color c = on ? LoginChipHoverBack : LoginChipBack;
            if (panelLogin.BackColor != c) panelLogin.BackColor = c;
        }

        /// <summary>登录芯片的悬停提示：完整用户名 / 权限 / 登录时间，外加"可点击"的说明。语言切换后由 SyncLanguageTexts 重取。</summary>
        private readonly ToolTip _loginTip = new ToolTip { ShowAlways = true };

        internal void UpdateLoginChipTip()
        {
            if (UiTheme.IsDesignTime) return;
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

        /// <summary>设置导航分组和横向图标按钮的尺寸。</summary>
        private void StyleNavigation()
        {
            panel4.GradientTop = ShellNav;
            panel4.GradientBottom = ShellNav;
            panel4.Paint -= Nav_Paint;
            panel4.Paint += Nav_Paint;
            foreach (FlowLayoutPanel flow in new[] { Panl_PictureShowTop, Panl_PictureShowDown })
            {
                flow.Padding = UiTheme.Dpi(new Padding(12, 0, 12, 0));
                flow.BackColor = Color.Transparent;
            }
            foreach (PictureBox button in NavButtons())
            {
                button.Size = UiTheme.Dpi(NavButtonSize);
                button.Margin = UiTheme.Dpi(new Padding(0, 2, 0, 2));
                button.SizeMode = PictureBoxSizeMode.CenterImage;
                button.Cursor = Cursors.Hand;
            }
        }

        /// <summary>导航栏右侧分隔线。</summary>
        private void Nav_Paint(object sender, PaintEventArgs e)
        {
            Control control = (Control)sender;
            using (Pen pen = new Pen(Color.FromArgb(29, 48, 67)))
                e.Graphics.DrawLine(pen, control.Width - 1, 0, control.Width - 1, control.Height);
        }

        /// <summary>返回左侧导航按钮，保留原页面顺序。</summary>
        private PictureBox[] NavButtons()
        {
            return new PictureBox[]
            {
                MENU_Home, MENU_Vision, MENU_Save, MENU_Rapid, MENU_Hard, MENU_Manual,
                MENU_LifeSpan, MENU_Mes, MENU_AddUser, MENU_Data, MENU_Check, MENU_Log
            };
        }

        // ---------------- 底部工具栏 ----------------

        /// <summary>返回顶部设备操作与系统操作按钮。</summary>
        private PictureBox[] ToolbarButtons()
        {
            return new PictureBox[]
            {
                MENU_Reset, MENU_Run, MENU_Pause, MENU_Stop, AlarmReset,
                MENU_Lock, MENU_System, MENU_Exit
            };
        }

        /// <summary>设置顶部操作栏及两组按钮的间距。</summary>
        private void StyleToolbar()
        {
            panel_PictureShowDown.BackColor = Color.FromArgb(18, 38, 57);
            panel_PictureShowDown.Paint -= Toolbar_Paint;
            panel_PictureShowDown.Paint += Toolbar_Paint;
            panel_PictureShowLeft.Padding = UiTheme.Dpi(new Padding(24, 10, 0, 0));
            panel_PictureShowRight.Padding = UiTheme.Dpi(new Padding(0, 10, 16, 0));
            panel_PictureShowLeft.BackColor = Color.Transparent;
            panel_PictureShowRight.BackColor = Color.Transparent;
            foreach (PictureBox button in ToolbarButtons())
            {
                button.Size = UiTheme.Dpi(ToolButtonSize);
                button.Margin = UiTheme.Dpi(new Padding(0, 0, 8, 0));
                button.SizeMode = PictureBoxSizeMode.CenterImage;
                button.Cursor = Cursors.Hand;
            }
        }

        /// <summary>工具栏顶部分隔线。</summary>
        private void Toolbar_Paint(object sender, PaintEventArgs e)
        {
            Control control = (Control)sender;
            using (Pen pen = new Pen(Color.FromArgb(29, 48, 67)))
                e.Graphics.DrawLine(pen, 0, control.Height - 1, control.Width, control.Height - 1);
        }

        // ---------------- 内容区选项卡 ----------------

        /// <summary>设置首页、视觉与报警页签的样式。</summary>
        private void StyleContentTabs()
        {
            uiTabControl1.Cursor = Cursors.Default;
            uiTabControl1.Font = UiKit.Regular(10f);
            uiTabControl1.ItemSize = UiTheme.Dpi(new Size(68, 48));
            uiTabControl1.FillColor = ShellCanvas;
            uiTabControl1.TabBackColor = ShellCanvas;
            uiTabControl1.TabSelectedColor = ShellCanvas;
            uiTabControl1.TabSelectedForeColor = Color.FromArgb(0, 118, 177);
            uiTabControl1.TabUnSelectedForeColor = Color.FromArgb(100, 117, 138);
            uiTabControl1.TabSelectedHighColor = ShellAccent;
            uiTabControl1.TabSelectedHighColorSize = 2;
            tabPage22.BackColor = ShellCanvas;
            tabPage10.BackColor = ShellCanvas;
        }

        // ---------------- 报警栏 ----------------

        /// <summary>设置原报警筛选条与报警列表。</summary>
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
            WarnningMessage.SizeChanged += ResizeAlarmColumns;
            ResizeAlarmColumns(WarnningMessage, EventArgs.Empty);
        }

        /// <summary>按报警表可见宽度分配内容列，窄窗口也能同时查看时间、类型、编号和内容。</summary>
        private void ResizeAlarmColumns(object sender, EventArgs e)
        {
            if (WarnningMessage.Columns.Count < 4) return;
            WarnningMessage.Columns[0].Width = UiTheme.Dpi(160);
            WarnningMessage.Columns[1].Width = UiTheme.Dpi(64);
            WarnningMessage.Columns[2].Width = UiTheme.Dpi(80);
            WarnningMessage.Columns[3].Width = Math.Max(UiTheme.Dpi(180),
                WarnningMessage.ClientSize.Width - UiTheme.Dpi(304) - SystemInformation.VerticalScrollBarWidth - 2);
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

        /// <summary>设置底部时钟与版本文字。</summary>
        private void StyleStatusBar()
        {
            statusStrip2.BackColor = Color.White;
            statusStrip2.Font = UiKit.Regular(8f);
            statusStrip2.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            statusStrip2.Padding = Padding.Empty;
            foreach (ToolStripItem item in statusStrip2.Items)
            {
                item.ForeColor = Color.FromArgb(124, 143, 168);
                item.Font = UiKit.Regular(8f);
                item.Margin = UiTheme.Dpi(new Padding(8, 0, 0, 0));
            }
        }

        // ---------------- 按钮图标与文字 ----------------

        /// <summary>按当前语言取导航与工具栏按钮的文字。</summary>
        private void LoadFrameTexts()
        {
            _buttonTexts.Clear();
            lblMonitorGroup.Text = UiTheme.T("MainForm", "lblMonitorGroup", "监控", "MONITOR", "MONITORIZACIÓN");
            lblSystemGroup.Text = UiTheme.T("MainForm", "lblSystemGroup", "系统", "SYSTEM", "SISTEMA");
            lblDashboardCaption.Text = UiTheme.T("MainForm", "lblDashboardCaption", "生产监控", "Production", "Producción");
            tabPage22.Text = UiTheme.T("MainForm", "tabPage22", "主页", "Home", "Inicio");
            tabPage10.Text = UiTheme.T("MainForm", "tabPage10", "视觉", "Vision", "Visión");
            lblAlarmTitle.Text = UiTheme.T("MainForm", "lblAlarmTitle", "报警信息", "Alarms", "Alarmas");
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

        /// <summary>读取当前语言的按钮名称。</summary>
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
                    MENU_Product.Enabled ? ShellAccent : AppIconColor.Disabled));
                SetImage(MENU_Login, AppIcons.Get(AppIcon.Login, UiTheme.Dpi(24),
                    MENU_Login.Enabled ? AppIconColor.OnDarkBar : AppIconColor.DisabledOnDark));
            }
            catch { }
        }

        /// <summary>仅在图标实例变化时更新图片。</summary>
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

        /// <summary>判断按钮是否对应当前页面。</summary>
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
            public Font Font;
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
                st.Font = isNav ? UiKit.Regular(10f) : UiKit.Bold(9f);
                _navPaint[b] = st;
                b.Image = null;                        // 关键：不再用烤好的合成位图
                b.Paint += NavButton_Paint;
            }
            st.Icon = icon;
            st.IconSize = iconSize;
            st.IsNav = isNav;
            return st;
        }

        /// <summary>释放主框架缓存的按钮字体和悬停提示。</summary>
        private void DisposeFrameResources()
        {
            foreach (NavPaintState state in _navPaint.Values)
                if (state.Font != null) state.Font.Dispose();
            _navPaint.Clear();
            _loginTip.Dispose();
            _statusTip.Dispose();
        }

        /// <summary>横向绘制圆角按钮、图标、文字和权限角标。</summary>
        private void NavButton_Paint(object sender, PaintEventArgs e)
        {
            PictureBox button = sender as PictureBox;
            NavPaintState state;
            if (button == null || !_navPaint.TryGetValue(button, out state)) return;
            Graphics graphics = e.Graphics;
            graphics.Clear(UiKit.ResolveParentBack(button));
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            int padding = UiTheme.Dpi(state.IsNav ? 14 : 12);
            int iconSize = UiTheme.Dpi(state.IconSize);
            Rectangle bounds = new Rectangle(0, 0, button.Width - 1, button.Height - 1);
            using (GraphicsPath path = UiKit.RoundRect(bounds, UiTheme.Dpi(7)))
            {
                if (button.BackColor.A == 255)
                    using (SolidBrush fill = new SolidBrush(button.BackColor)) graphics.FillPath(fill, path);
                if (!state.IsNav)
                    using (Pen border = new Pen(button == MENU_Run || button == MENU_Stop ? button.BackColor : Color.FromArgb(211, 221, 231))) graphics.DrawPath(border, path);
            }
            Image icon = AppIcons.Get(state.Icon, iconSize, state.IconColor, state.AccentColor);
            graphics.DrawImage(icon, new Rectangle(padding, (button.Height - iconSize) / 2, iconSize, iconSize));
            int textLeft = padding + iconSize + UiTheme.Dpi(state.IsNav ? 14 : 7);
            TextRenderer.DrawText(graphics, ButtonText(button), state.Font,
                new Rectangle(textLeft, 0, Math.Max(1, button.Width - textLeft - UiTheme.Dpi(8)), button.Height),
                state.TextColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
            if (state.IsNav && IsSelectedNav(button))
                using (SolidBrush accent = new SolidBrush(ShellAccent)) graphics.FillRectangle(accent, 0, UiTheme.Dpi(9), UiTheme.Dpi(3), button.Height - UiTheme.Dpi(18));
            if (!button.Enabled)
            {
                Image locked = AppIcons.Get(AppIcon.Lock, UiTheme.Dpi(9), state.TextColor);
                graphics.DrawImage(locked, button.Width - UiTheme.Dpi(12), UiTheme.Dpi(3), UiTheme.Dpi(9), UiTheme.Dpi(9));
            }
        }

        /// <summary>按页面选择及权限更新导航按钮配色。</summary>
        private void StyleNavButton(PictureBox b, AppIcon icon)
        {
            if (b == null) return;
            bool selected = IsSelectedNav(b);
            NavPaintState state = AttachNavPaint(b, icon, NavIconSize, true);
            state.IconColor = !b.Enabled ? Color.FromArgb(97, 117, 140) : selected ? Color.FromArgb(66, 190, 248) : ShellMuted;
            state.AccentColor = state.IconColor;
            state.TextColor = state.IconColor;
            state.LockBadge = NavLockBadge;
            b.BackColor = selected ? ShellSelected : Color.Transparent;
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
                StyleToolbarButton(MENU_Reset, AppIcon.Reset);
                StyleToolbarButton(MENU_Run, AppIcon.Run);
                StyleToolbarButton(MENU_Pause, AppIcon.Pause);
                StyleToolbarButton(MENU_Stop, AppIcon.Stop);
                StyleToolbarButton(AlarmReset, AppIcon.AlarmReset);
                StyleToolbarButton(MENU_Lock, AppIcon.Lock);
                StyleToolbarButton(MENU_System, AppIcon.System);
                StyleToolbarButton(MENU_Exit, AppIcon.Exit);
                panel_PictureShowRight.Width = MENU_Exit.Width + MENU_System.Width + MENU_Lock.Width + UiTheme.Dpi(40);
            }
            catch { }
        }

        /// <summary>按动作设置按钮配色，并为当前语言保留完整文字宽度。</summary>
        private void StyleToolbarButton(PictureBox btn, AppIcon icon)
        {
            if (btn == null) return;
            bool enabled = btn.Enabled;
            bool primary = btn == MENU_Run || btn == MENU_Stop;
            Color back = btn == MENU_Run ? Color.FromArgb(0, 143, 211) : btn == MENU_Stop ? Color.FromArgb(232, 64, 76) : Color.White;
            if (enabled && btn == _hoverToolbarButton) back = primary ? ControlPaint.Light(back, 0.08f) : ToolbarHover;
            btn.BackColor = back;
            NavPaintState state = AttachNavPaint(btn, icon, ToolIconSize, false);
            state.IconColor = primary ? Color.White : ShellInk;
            state.AccentColor = state.IconColor;
            state.TextColor = primary ? Color.White : enabled ? ShellInk : Color.FromArgb(112, 129, 149);
            state.LockBadge = NavLockBadge;
            int textWidth = TextRenderer.MeasureText(ButtonText(btn), state.Font, Size.Empty, TextFormatFlags.NoPadding).Width;
            int width = Math.Max(UiTheme.Dpi(84), textWidth + UiTheme.Dpi(16 + 7 + 24));
            if (btn.Width != width)
            {
                btn.Width = width;
                ApplyRoundRegion(new[] { btn }, UiTheme.Dpi(12));
            }
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

        /// <summary>高亮鼠标当前指向的操作按钮。</summary>
        private void ToolbarButton_MouseEnter(object sender, EventArgs e)
        {
            PictureBox btn = sender as PictureBox;
            if (btn == null || btn == _hoverToolbarButton) return;
            _hoverToolbarButton = btn;
            ApplyToolbarButtonStyles();
        }

        /// <summary>清除操作按钮的悬停状态。</summary>
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
            MENU_Product.BackColor = Color.Transparent;
            MENU_Login.BackColor = Color.Transparent;
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
