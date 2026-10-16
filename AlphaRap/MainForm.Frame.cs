using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
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

        /// <summary>导航按钮默认尺寸；实际由 <see cref="FitNavRailNoScroll"/> 按高度均分，保证无滚动条。</summary>
        private static readonly Size NavButtonSize = new Size(80, 46);
        private const int NavIconSize = 20;
        /// <summary>侧栏展开宽度；折叠为窄轨。</summary>
        private const int NavRailWidth = 96;
        private const int NavRailWidthCollapsed = 36;
        private const int NavCollapsedIconSize = 18;
        /// <summary>当前均分后的图标边长 / 字号（Fit 时写入）。</summary>
        private int _navRailIconSize = NavIconSize;
        private float _navRailFontPt = 9.5f;
        private bool _fittingNavRail;

        // ---- 侧栏蓝底白字：与顶栏同色 UiKit.Brand（回退 tag: backup-before-nav-blue-rail）----
        private static readonly Color NavRailBg = UiKit.Brand;
        private static readonly Color NavRailEdge = UiKit.BrandDark;
        private static readonly Color NavOnBlue = UiKit.TextOnBrand;
        /// <summary>选中：图标略更亮（仍白系）。</summary>
        private static readonly Color NavOnBlueSelected = Color.White;
        /// <summary>禁用：半透明白，不铺深蓝底。</summary>
        private static readonly Color NavOnBlueDisabled = Color.FromArgb(150, 200, 220);
        /// <summary>选中左侧竖条：白，蓝底上最醒目。</summary>
        private static readonly Color NavRailAccentBar = Color.White;
        /// <summary>汉堡弱化：略淡的白，不与正式入口抢戏。</summary>
        private static readonly Color NavToggleMuted = Color.FromArgb(190, 215, 235);
        private static readonly Size NavToggleSize = new Size(64, 28);
        private const int NavToggleIconSize = 16;
        private static readonly Size ToolButtonSize = new Size(92, 58);
        private const int ToolIconSize = 26;

        /// <summary>工具栏按钮悬停时的底色。</summary>
        private static readonly Color ToolbarHover = UiTheme.Selected;

        /// <summary>当前鼠标悬停的工具栏按钮。</summary>
        private PictureBox _hoverToolbarButton;

        /// <summary>导航与工具栏按钮文字（按当前语言缓存，切换语言时由 LoadFrameTexts 重取）。</summary>
        private readonly Dictionary<PictureBox, string> _buttonTexts = new Dictionary<PictureBox, string>();

        /// <summary>主框架样式，构造函数中在 InitializeComponent 之后调用（设计期也执行，使设计视图贴近 EXE）。</summary>
        private void ApplyFrameStyle()
        {
            MENU_Picture = new PictureBox[] { MENU_Home, MENU_Product, MENU_Save, MENU_Hard, MENU_Manual, MENU_Check, MENU_System, MENU_AddUser, MENU_Rapid, MENU_Log, MENU_Data, MENU_Vision, MENU_Login, MENU_LifeSpan, MENU_Lock, MENU_Reset, MENU_Pause, MENU_Stop, MENU_Exit, MENU_Run, MENU_Mes };

            // 底色层次：框架浅灰蓝 → 内容区更浅 → 卡片白
            tableLayoutPanel_Main.BackColor = UiTheme.FrameBg;
            panel_Second.BackColor = UiTheme.FrameBg;
            panel2.BackColor = UiTheme.FrameBg;
            plMainShow.BackColor = UiKit.PageBg;

            // 分段 try：某一段失败不阻断图标/文字刷新（设计器里尤其重要）
            TryFrameStep(StyleFonts);
            TryFrameStep(StyleTopBar);
            TryFrameStep(StyleNavigation);
            TryFrameStep(StyleToolbar);
            TryFrameStep(StyleContentTabs);
            TryFrameStep(StyleAlarmPanel);
            TryFrameStep(StyleStatusBar);
            TryFrameStep(LoadFrameTexts);
            TryFrameStep(PrewarmFrameIcons);
            TryFrameStep(RefreshButtonIcons);
            TryFrameStep(RefreshMenuBackcolor);
        }

        private static void TryFrameStep(Action step)
        {
            if (step == null) return;
            try { step(); }
            catch { }
        }

        // ---------------- 字体（与顶栏「配方」一致：微软雅黑） ----------------

        /// <summary>窗体与内容区默认字体；顶栏芯片/配方等仍用 Bold。</summary>
        private void StyleFonts()
        {
            Font body = UiKit.Regular(10.5f);
            this.Font = body;
            if (plMainShow != null) plMainShow.Font = body;
            if (tabPage22 != null) tabPage22.Font = body;
            if (tabPage10 != null) tabPage10.Font = body;
            if (uiTabControl1 != null) uiTabControl1.Font = UiKit.Bold(11f);
            if (WarnningMessage != null) WarnningMessage.Font = UiKit.Regular(9f);
            if (miniToolStrip != null) miniToolStrip.Font = UiKit.Bold(10.5f);
            if (menuStrip1 != null) menuStrip1.Font = UiKit.Bold(10.5f);
            if (languageSwitch != null) languageSwitch.Font = UiKit.Bold(10.5f);
            if (txtCyCT != null) txtCyCT.Font = UiKit.Bold(13.8f);
        }

        // ---------------- 顶栏 ----------------

        /// <summary>登录芯片常态底色（比顶栏品牌蓝略亮，形成"可点"的层次）。</summary>
        private static readonly Color LoginChipBack = Color.FromArgb(28, 126, 200);

        /// <summary>登录芯片悬停底色。</summary>
        private static readonly Color LoginChipHoverBack = Color.FromArgb(52, 148, 218);

        private void StyleTopBar()
        {
            // 与截图一致：微软雅黑 Bold + 白字
            lbRecipeName.Font = UiKit.Bold(10.5f);
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

            // 设备状态徽章：顶栏正中央；整块填状态色（宽度由 SetStatusLook 按文字算）
            // 放大一档便于扫视辨认；文案「主状态 · 次要信息」
            MachineStatus.Font = UiKit.Bold(17f);
            MachineStatus.TextAlign = ContentAlignment.MiddleCenter;
            MachineStatus.AutoSize = false;
            MachineStatus.AutoEllipsis = false;
            MachineStatus.Height = UiTheme.Dpi(42);
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
        private bool _loginChipHooked;

        private void InitLoginChip()
        {
            if (!_loginChipHooked)
            {
                _loginChipHooked = true;
                panelLogin.Click += UserLogin_Click;
                LoginText.Click += UserLogin_Click;
                panelLogin.MouseEnter += LoginChip_MouseEnter;
                panelLogin.MouseLeave += LoginChip_MouseLeave;
                LoginText.MouseEnter += LoginChip_MouseEnter;
                LoginText.MouseLeave += LoginChip_MouseLeave;
                MENU_Login.MouseEnter += LoginChip_MouseEnter;
                MENU_Login.MouseLeave += LoginChip_MouseLeave;
            }

            // 设计器里不要碰 MiddleLayer（静态初始化会拖硬件）
            if (!UiTheme.IsDesignTime) UpdateLoginChipTip();
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
                           + "\n" + MiddleLayer.LangMsg("MainForm", "msg_LoginChipPerm", "权限:  ", "Permission :  ", "Permiso :  ") + SysPara.UserPermission
                           + "\n" + MiddleLayer.LangMsg("MainForm", "msg_LoginChipTime", "登录时间:  ", "LoginTime :  ", "LoginTime :  ") + SysPara.UserLoginTime
                           + "\n\n" + MiddleLayer.LangMsg("MainForm", "msg_LoginChipClick", "点击打开登录页", "Click to sign in", "Haga clic para iniciar sesión");
                _loginTip.SetToolTip(panelLogin, tip);
                _loginTip.SetToolTip(LoginText, tip);
            }
            catch { }
        }

        // ---------------- 左侧导航 ----------------

        private void StyleNavigation()
        {
            // 与顶栏同一 Brand 纯色蓝底
            panel4.GradientTop = NavRailBg;
            panel4.GradientBottom = NavRailBg;
            panel4.Paint -= Nav_Paint;
            panel4.Paint += Nav_Paint;

            // 清掉旧版「更多」半区折叠残留
            RemoveLegacyNavFoldControls();
            EnsureNavRailToggle();
            EnsureNavAutoHideTab();
            // 上下 50/50 时上半区装不下会画到下半区上（蓝底禁用块看起来像重叠）→ 改单列可滚
            MergeNavIntoSingleColumn();
            LoadNavRailPrefs();

            ApplyNavRailLayout();
        }

        /// <summary>
        /// 把下半区入口并入上半区，整栏一列 + AutoScroll，避免两区交界处按钮重叠。
        /// </summary>
        private void MergeNavIntoSingleColumn()
        {
            if (panel4 == null || Panl_PictureShowTop == null) return;
            try
            {
                panel4.SuspendLayout();
                Panl_PictureShowTop.SuspendLayout();

                if (Panl_PictureShowDown != null && Panl_PictureShowDown.Controls.Count > 0)
                {
                    Control[] down = new Control[Panl_PictureShowDown.Controls.Count];
                    Panl_PictureShowDown.Controls.CopyTo(down, 0);
                    for (int i = 0; i < down.Length; i++)
                    {
                        Control c = down[i];
                        if (c == null) continue;
                        Panl_PictureShowDown.Controls.Remove(c);
                        Panl_PictureShowTop.Controls.Add(c);
                    }
                }

                Panl_PictureShowTop.Dock = DockStyle.Fill;
                Panl_PictureShowTop.FlowDirection = FlowDirection.TopDown;
                Panl_PictureShowTop.WrapContents = false;
                // 不要滚动条：按钮高度按可用高度均分（见 FitNavRailNoScroll）
                Panl_PictureShowTop.AutoScroll = false;
                Panl_PictureShowTop.Padding = UiTheme.Dpi(new Padding(0, 2, 0, 2));
                Panl_PictureShowTop.BackColor = Color.Transparent;
                Panl_PictureShowTop.Visible = true;
                Panl_PictureShowTop.Resize -= NavRailFlow_Resize;
                Panl_PictureShowTop.Resize += NavRailFlow_Resize;
                if (panel4 != null)
                {
                    panel4.Resize -= NavRailHost_Resize;
                    panel4.Resize += NavRailHost_Resize;
                }

                if (Panl_PictureShowDown != null)
                {
                    Panl_PictureShowDown.Visible = false;
                    try { panel4.Controls.Remove(Panl_PictureShowDown); } catch { }
                }

                panel4.RowCount = 1;
                panel4.RowStyles.Clear();
                panel4.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
                if (!panel4.Controls.Contains(Panl_PictureShowTop))
                    panel4.Controls.Add(Panl_PictureShowTop, 0, 0);
                else
                    panel4.SetRow(Panl_PictureShowTop, 0);

                Panl_PictureShowTop.ResumeLayout(true);
                panel4.ResumeLayout(true);
            }
            catch
            {
                try { Panl_PictureShowTop.ResumeLayout(true); } catch { }
                try { panel4.ResumeLayout(true); } catch { }
            }
        }

        /// <summary>导航栏右侧接缝：2px 深蓝，和灰白内容区分开。</summary>
        private void Nav_Paint(object sender, PaintEventArgs e)
        {
            Control c = (Control)sender;
            int w = UiTheme.Dpi(2);
            using (SolidBrush b = new SolidBrush(NavRailEdge))
                e.Graphics.FillRectangle(b, c.Width - w, 0, w, c.Height);
        }

        private PictureBox[] NavButtons()
        {
            return new PictureBox[]
            {
                MENU_Home, MENU_Vision, MENU_Save, MENU_Rapid, MENU_Hard, MENU_Manual,
                MENU_LifeSpan, MENU_Mes, MENU_AddUser, MENU_Data, MENU_Check, MENU_Log
            };
        }

        // ---------------- 整栏收起（窄轨：白底 + 品牌竖线，与报警底边细条同语言） ----------------

        /// <summary>侧栏是否收成左侧窄轨（点击再展开）。</summary>
        private bool _navRailCollapsed;
        /// <summary>用户手动折叠偏好（写入 UiPrefs）。</summary>
        private bool _navRailUserCollapsed;
        /// <summary>因高度不够自动收成窄轨。</summary>
        private bool _navRailAutoCollapsed;
        private bool _navAutoHideHover;
        /// <summary>展开态顶部汉堡：点一下收成窄轨。</summary>
        private PictureBox MENU_NavToggle;
        /// <summary>折叠态窄轨：汉堡图标 + 竖排「导航」，点击展开。</summary>
        private Panel panelNavAutoHideTab;
        private readonly ToolTip _navRailTip = new ToolTip { ShowAlways = true, AutomaticDelay = 400 };

        private void EnsureNavRailToggle()
        {
            if (MENU_NavToggle != null || Panl_PictureShowTop == null) return;
            MENU_NavToggle = new PictureBox();
            MENU_NavToggle.Name = "MENU_NavToggle";
            MENU_NavToggle.BackColor = Color.Transparent;
            MENU_NavToggle.Size = UiTheme.Dpi(NavToggleSize);
            MENU_NavToggle.Margin = UiTheme.Dpi(new Padding(8, 6, 8, 4));
            MENU_NavToggle.SizeMode = PictureBoxSizeMode.CenterImage;
            MENU_NavToggle.Cursor = Cursors.Hand;
            MENU_NavToggle.TabStop = false;
            MENU_NavToggle.Click += NavRailToggle_Click;
            Panl_PictureShowTop.Controls.Add(MENU_NavToggle);
            Panl_PictureShowTop.Controls.SetChildIndex(MENU_NavToggle, 0);
            ApplyRoundRegion(new PictureBox[] { MENU_NavToggle }, UiTheme.Dpi(12));
        }

        private void EnsureNavAutoHideTab()
        {
            if (panelNavAutoHideTab != null || panel4 == null) return;
            panelNavAutoHideTab = new Panel();
            panelNavAutoHideTab.Name = "panelNavAutoHideTab";
            panelNavAutoHideTab.Dock = DockStyle.Fill;
            panelNavAutoHideTab.Margin = Padding.Empty;
            panelNavAutoHideTab.Cursor = Cursors.Hand;
            panelNavAutoHideTab.BackColor = NavRailBg;
            panelNavAutoHideTab.Visible = false;
            panelNavAutoHideTab.Paint += NavAutoHideTab_Paint;
            panelNavAutoHideTab.Click += NavRailToggle_Click;
            panelNavAutoHideTab.Resize += NavAutoHideTab_Resize;
            panelNavAutoHideTab.MouseEnter += NavAutoHideTab_MouseEnter;
            panelNavAutoHideTab.MouseLeave += NavAutoHideTab_MouseLeave;
            // TableLayoutPanel：跨两行铺满整列，才能盖住上下导航区
            panel4.Controls.Add(panelNavAutoHideTab, 0, 0);
            try { panel4.SetRowSpan(panelNavAutoHideTab, 2); } catch { }
            panelNavAutoHideTab.BringToFront();
        }

        private void RemoveLegacyNavFoldControls()
        {
            if (Panl_PictureShowTop != null)
            {
                Control[] copy = new Control[Panl_PictureShowTop.Controls.Count];
                Panl_PictureShowTop.Controls.CopyTo(copy, 0);
                for (int i = 0; i < copy.Length; i++)
                {
                    Control c = copy[i];
                    if (c == null) continue;
                    if (c.Name != "MENU_NavMore" && c.Name != "panelNavMoreDrop") continue;
                    Panl_PictureShowTop.Controls.Remove(c);
                    try { c.Dispose(); } catch { }
                }
            }
            if (panel4 != null)
            {
                Control[] copy = new Control[panel4.Controls.Count];
                panel4.Controls.CopyTo(copy, 0);
                for (int i = 0; i < copy.Length; i++)
                {
                    Control c = copy[i];
                    if (c == null || c.Name != "panelNavMoreDrop") continue;
                    panel4.Controls.Remove(c);
                    try { c.Dispose(); } catch { }
                }
            }
        }

        private void NavRailToggle_Click(object sender, EventArgs e)
        {
            SetNavRailCollapsed(!_navRailCollapsed, true);
        }

        private void NavAutoHideTab_Resize(object sender, EventArgs e)
        {
            if (panelNavAutoHideTab != null) panelNavAutoHideTab.Invalidate();
        }

        private void NavAutoHideTab_MouseEnter(object sender, EventArgs e)
        {
            _navAutoHideHover = true;
            if (panelNavAutoHideTab != null) panelNavAutoHideTab.Invalidate();
        }

        private void NavAutoHideTab_MouseLeave(object sender, EventArgs e)
        {
            _navAutoHideHover = false;
            if (panelNavAutoHideTab != null) panelNavAutoHideTab.Invalidate();
        }

        /// <summary>整栏折叠 / 展开：折叠后变成左侧窄轨，点击展开。</summary>
        public void SetNavRailCollapsed(bool collapsed)
        {
            SetNavRailCollapsed(collapsed, true);
        }

        /// <param name="fromUser">true = 用户操作（记偏好）；false = 高度不够自动收起。</param>
        private void SetNavRailCollapsed(bool collapsed, bool fromUser)
        {
            if (fromUser)
            {
                _navRailUserCollapsed = collapsed;
                _navRailAutoCollapsed = false;
                SaveNavRailPrefs();
            }
            if (_navRailCollapsed == collapsed)
            {
                ApplyNavRailLayout();
                RefreshButtonIcons();
                return;
            }
            _navRailCollapsed = collapsed;
            ApplyNavRailLayout();
            RefreshButtonIcons();
        }

        private static string NavRailPrefsPath()
        {
            return Path.Combine(Application.StartupPath, "ModuleData", "SettingData", "UiPrefs.txt");
        }

        private void LoadNavRailPrefs()
        {
            try
            {
                string path = NavRailPrefsPath();
                if (!File.Exists(path)) return;
                string[] lines = File.ReadAllLines(path);
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i] != null ? lines[i].Trim() : "";
                    if (line.StartsWith("NavRailCollapsed=", StringComparison.OrdinalIgnoreCase))
                    {
                        _navRailUserCollapsed = line.EndsWith("1");
                        _navRailCollapsed = _navRailUserCollapsed;
                    }
                }
            }
            catch { }
        }

        private void SaveNavRailPrefs()
        {
            try
            {
                string path = NavRailPrefsPath();
                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.WriteAllText(path, "NavRailCollapsed=" + (_navRailUserCollapsed ? "1" : "0") + Environment.NewLine);
            }
            catch { }
        }

        private void ApplyNavRailLayout()
        {
            if (tableLayoutPanel1 == null) return;
            EnsureNavAutoHideTab();
            try
            {
                int railW = UiTheme.Dpi(_navRailCollapsed ? NavRailWidthCollapsed : NavRailWidth);
                tableLayoutPanel1.SuspendLayout();
                tableLayoutPanel1.ColumnStyles[0] = new ColumnStyle(SizeType.Absolute, railW);
                tableLayoutPanel1.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, 100f);
                // 折叠时去掉侧栏右侧 4px 空隙，窄轨与内容贴齐；展开时恢复设计器间距
                if (panel4 != null)
                    panel4.Margin = _navRailCollapsed
                        ? Padding.Empty
                        : UiTheme.Dpi(new Padding(0, 0, 4, 0));
                tableLayoutPanel1.ResumeLayout(true);
            }
            catch { }

            if (_navRailCollapsed)
            {
                if (Panl_PictureShowTop != null) Panl_PictureShowTop.Visible = false;
                if (panelNavAutoHideTab != null)
                {
                    panelNavAutoHideTab.Visible = true;
                    panelNavAutoHideTab.BringToFront();
                    panelNavAutoHideTab.Invalidate();
                }
            }
            else
            {
                _navAutoHideHover = false;
                if (panelNavAutoHideTab != null) panelNavAutoHideTab.Visible = false;
                if (Panl_PictureShowTop != null) Panl_PictureShowTop.Visible = true;
                FitNavRailNoScroll();
            }
            UpdateNavRailTooltips();
        }

        private void NavRailFlow_Resize(object sender, EventArgs e)
        {
            if (_navRailCollapsed || _fittingNavRail) return;
            FitNavRailNoScroll();
            RefreshButtonIcons();
        }

        /// <summary>侧栏宿主高度变化：自动窄轨时，够高再展开。</summary>
        private void NavRailHost_Resize(object sender, EventArgs e)
        {
            if (_fittingNavRail) return;
            if (_navRailAutoCollapsed && !_navRailUserCollapsed && _navRailCollapsed)
            {
                int h = panel4 != null ? panel4.ClientSize.Height : 0;
                // 经验阈值：12 入口 × 40 + 汉堡 ≈ 520
                if (h >= UiTheme.Dpi(520))
                {
                    _navRailAutoCollapsed = false;
                    SetNavRailCollapsed(false, false);
                }
                return;
            }
            if (!_navRailCollapsed) NavRailFlow_Resize(sender, e);
        }

        /// <summary>
        /// 按侧栏可用高度均分入口尺寸，关掉横/竖滚动条；字号随高度略缩小。
        /// </summary>
        private void FitNavRailNoScroll()
        {
            if (Panl_PictureShowTop == null || _fittingNavRail) return;
            _fittingNavRail = true;
            try
            {
                Panl_PictureShowTop.AutoScroll = false;
                Panl_PictureShowTop.HorizontalScroll.Maximum = 0;
                Panl_PictureShowTop.VerticalScroll.Maximum = 0;

                int availH = Panl_PictureShowTop.ClientSize.Height;
                int availW = Panl_PictureShowTop.ClientSize.Width;
                if (availH < UiTheme.Dpi(80) || availW < UiTheme.Dpi(40)) return;

                PictureBox[] nav = NavButtons();
                int btnCount = 0;
                for (int i = 0; i < nav.Length; i++)
                    if (nav[i] != null) btnCount++;
                if (btnCount <= 0) return;

                int toggleBlock = 0;
                if (MENU_NavToggle != null)
                {
                    Size ts = UiTheme.Dpi(NavToggleSize);
                    int tMargin = UiTheme.Dpi(8);
                    MENU_NavToggle.Size = ts;
                    MENU_NavToggle.Margin = new Padding((availW - ts.Width) / 2, UiTheme.Dpi(4), (availW - ts.Width) / 2, UiTheme.Dpi(2));
                    MENU_NavToggle.Visible = true;
                    toggleBlock = ts.Height + MENU_NavToggle.Margin.Vertical;
                }

                int rest = availH - Panl_PictureShowTop.Padding.Vertical - toggleBlock;
                int slot = rest / btnCount;
                // 均分后仍矮于下限 → 自动收成窄轨（用户手动展开偏好仍保留）
                if (slot < UiTheme.Dpi(36) && !_navRailUserCollapsed)
                {
                    _fittingNavRail = false;
                    _navRailAutoCollapsed = true;
                    SetNavRailCollapsed(true, false);
                    return;
                }
                if (_navRailAutoCollapsed && !_navRailUserCollapsed && slot >= UiTheme.Dpi(40))
                {
                    _fittingNavRail = false;
                    _navRailAutoCollapsed = false;
                    SetNavRailCollapsed(false, false);
                    return;
                }

                int marginV = Math.Max(0, UiTheme.Dpi(1));
                int btnH = slot - marginV * 2;
                if (btnH < UiTheme.Dpi(36)) btnH = UiTheme.Dpi(36);
                if (btnH > UiTheme.Dpi(50)) btnH = UiTheme.Dpi(50);

                int btnW = availW - UiTheme.Dpi(4);
                if (btnW > UiTheme.Dpi(88)) btnW = UiTheme.Dpi(88);
                if (btnW < UiTheme.Dpi(56)) btnW = UiTheme.Dpi(56);

                if (btnH >= UiTheme.Dpi(48))
                {
                    _navRailIconSize = 22;
                    _navRailFontPt = 10f;
                }
                else if (btnH >= UiTheme.Dpi(42))
                {
                    _navRailIconSize = 20;
                    _navRailFontPt = 9.5f;
                }
                else
                {
                    _navRailIconSize = 18;
                    _navRailFontPt = 8.5f;
                }

                Size btnSize = new Size(btnW, btnH);
                int side = Math.Max(0, (availW - btnW) / 2);
                Padding margin = new Padding(side, marginV, side, marginV);
                foreach (PictureBox b in nav)
                {
                    if (b == null) continue;
                    b.Size = btnSize;
                    b.Margin = margin;
                    b.SizeMode = PictureBoxSizeMode.CenterImage;
                    b.Cursor = Cursors.Hand;
                }
                ApplyRoundedMenuRegions();
            }
            catch { }
            finally { _fittingNavRail = false; }
        }

        private void UpdateNavRailTooltips()
        {
            try
            {
                string expandTip = UiTheme.T("MainForm", "nav_ExpandRail", "展开导航", "Expand navigation", "Expandir navegación");
                string collapseTip = UiTheme.T("MainForm", "nav_CollapseRail", "折叠导航", "Collapse navigation", "Contraer navegación");
                if (MENU_NavToggle != null) _navRailTip.SetToolTip(MENU_NavToggle, collapseTip);
                if (panelNavAutoHideTab != null) _navRailTip.SetToolTip(panelNavAutoHideTab, expandTip);

                foreach (PictureBox b in NavButtons())
                {
                    if (b == null) continue;
                    _navRailTip.SetToolTip(b, "");
                }
            }
            catch { }
        }

        /// <summary>
        /// 窄轨绘制：白底 + 左侧品牌竖条 + 顶栏汉堡 + 逐字竖排「导航」
        /// （不用旋转字，避免和主界面水平中文打架）。
        /// </summary>
        private void NavAutoHideTab_Paint(object sender, PaintEventArgs e)
        {
            Panel p = sender as Panel;
            if (p == null || p.Width <= 1 || p.Height <= 1) return;
            try
            {
                Graphics g = e.Graphics;
                Color back = _navAutoHideHover ? Color.FromArgb(12, 126, 202) : NavRailBg;
                g.Clear(back);

                int accentW = UiTheme.Dpi(3);
                using (SolidBrush accent = new SolidBrush(NavOnBlue))
                    g.FillRectangle(accent, 0, 0, accentW, p.Height);
                using (Pen edge = new Pen(NavRailEdge))
                    g.DrawLine(edge, p.Width - 1, 0, p.Width - 1, p.Height);

                int ic = UiTheme.Dpi(NavCollapsedIconSize);
                Image menu = AppIcons.Get(AppIcon.Menu, ic, NavOnBlue);
                if (menu != null)
                {
                    int ix = (p.Width - menu.Width) / 2;
                    int iy = UiTheme.Dpi(10);
                    g.DrawImage(menu, ix, iy, menu.Width, menu.Height);
                }

                string text = UiTheme.T("MainForm", "nav_AutoHideTab", "导航", "Nav", "Nav");
                if (string.IsNullOrEmpty(text)) return;
                using (Font f = UiKit.Bold(10f))
                {
                    int y = UiTheme.Dpi(10) + ic + UiTheme.Dpi(12);
                    int lineH = UiTheme.Dpi(18);
                    for (int i = 0; i < text.Length; i++)
                    {
                        string ch = text.Substring(i, 1);
                        Size cs = TextRenderer.MeasureText(g, ch, f, Size.Empty, TextFormatFlags.NoPadding);
                        int cx = (p.Width - cs.Width) / 2;
                        TextRenderer.DrawText(g, ch, f, new Point(cx, y), NavOnBlue,
                            TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
                        y += lineH;
                    }
                }
            }
            catch { }
        }

        private void StyleNavToggleButton()
        {
            if (MENU_NavToggle == null) return;
            NavPaintState st = AttachNavPaint(MENU_NavToggle, AppIcon.Menu, NavToggleIconSize, true);
            st.IconColor = NavToggleMuted;
            st.AccentColor = st.IconColor;
            st.TextColor = NavToggleMuted;
            st.AccentBarColor = Color.Empty;
            st.LockBadge = UiTheme.LockBadgeMode.None;
            if (MENU_NavToggle.BackColor != Color.Transparent) MENU_NavToggle.BackColor = Color.Transparent;
            if (UiTheme.IsDesignTime) BakeButtonPreview(MENU_NavToggle, st);
            else MENU_NavToggle.Invalidate();
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
            // 用 Controls/UiTheme 自绘扁平选项卡，替代原 Sunny.UI.UITabControl
            UiTheme.StyleTabControl(uiTabControl1, true);
            uiTabControl1.Cursor = Cursors.Default;
            uiTabControl1.ItemSize = new Size(UiTheme.Dpi(132), UiTheme.TabHeight);
        }

        // ---------------- 报警栏 ----------------

        private void StyleAlarmPanel()
        {
            panelAlarmFilter.Height = UiTheme.Dpi(38);
            panelAlarmFilter.BackColor = UiKit.Surface;
            flowAlarmFilter.Padding = UiTheme.Dpi(new Padding(8, 5, 0, 0));
            flowAlarmFilter.BackColor = UiKit.Surface;
            EnsureAlarmToggleChip();
            EnsureAlarmCollapsedBar();
            foreach (Control chip in new Control[] { btnAlarmFilterAll, btnAlarmFilterWarn, btnAlarmFilterAlarm, btnAlarmQuery, btnAlarmToggle })
            {
                AlarmChip c = chip as AlarmChip;
                if (c == null) continue;
                c.Height = UiTheme.Dpi(28);
                c.Margin = UiTheme.Dpi(new Padding(0, 0, 8, 0));
                c.CornerRadius = UiTheme.Dpi(14);
                c.BorderColor = Color.FromArgb(200, 210, 222);
                c.TextColor = UiKit.TextPrimary;
            }
            if (btnAlarmToggle != null)
                btnAlarmToggle.Margin = UiTheme.Dpi(new Padding(12, 0, 8, 0));

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
            // 与侧栏/顶栏同系：BrandDark 底 + 浅字
            statusStrip2.BackColor = UiKit.BrandDark;
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

        /// <summary>导航按钮完整文案（悬停提示用；折叠时仍保留名字）。</summary>
        private string ButtonLabel(PictureBox b)
        {
            string s;
            return (b != null && _buttonTexts.TryGetValue(b, out s)) ? s : "";
        }

        /// <summary>绘制用文字（汉堡开关不画字）。</summary>
        private string ButtonText(PictureBox b)
        {
            if (b == MENU_NavToggle) return "";
            return ButtonLabel(b);
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
                StyleNavToggleButton();
                UpdateNavRailTooltips();

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
            for (int i = 0; i < nav.Length; i++) sb.Append(nav[i] != null && nav[i].Enabled ? '1' : '0');
            PictureBox[] tool = ToolbarButtons();
            for (int i = 0; i < tool.Length; i++) sb.Append(tool[i] != null && tool[i].Enabled ? '1' : '0');
            string lang = "zh";
            try { lang = SysPara.LanguageShow.ToString(); }
            catch { }
            sb.Append('|').Append((int)MENU_SelectPage)
              .Append('|').Append(lang)
              .Append('|').Append(_buttonTexts.Count)
              .Append('|').Append(MENU_Product != null && MENU_Product.Enabled ? '1' : '0')
              .Append('|').Append(MENU_Login != null && MENU_Login.Enabled ? '1' : '0')
              .Append('|').Append(_hoverToolbarButton == null ? "-" : _hoverToolbarButton.Name)
              .Append('|').Append(_navRailCollapsed ? '1' : '0');
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
                            AppIcon.Mes, AppIcon.LifeSpan, AppIcon.Menu },
                    new[] { UiTheme.Dpi(18), UiTheme.Dpi(20), UiTheme.Dpi(22), UiTheme.Dpi(NavCollapsedIconSize) },
                    new[] { NavOnBlue, NavOnBlueDisabled, NavToggleMuted, AppIconColor.OnDarkBar });

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
            /// <summary>选中左侧竖条颜色；Empty = 用默认 Brand（工具栏）或画白条（蓝底导航）。</summary>
            public Color AccentBarColor = Color.Empty;
            /// <summary>禁用时画一个小锁角标：把"没有权限"从颜色通道挪到形状通道。</summary>
            public UiTheme.LockBadgeMode LockBadge = UiTheme.LockBadgeMode.None;
            /// <summary>图标/文字不透明度；底栏禁用淡化用约 0.4，不改按钮底色以保持条底统一。</summary>
            public float ContentOpacity = 1f;
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
                // 运行时清掉 Image 改自绘；设计期保留 Image 槽给 BakeButtonPreview
                if (!UiTheme.IsDesignTime) b.Image = null;
                b.Paint += NavButton_Paint;
            }
            st.Icon = icon;
            st.IconSize = iconSize;
            st.IsNav = isNav;
            st.Font = isNav ? UiKit.Regular(_navRailFontPt) : UiTheme.ToolLabel;
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
                bool sel = st.IsNav && IsSelectedNav(b);
                // 汉堡开关不算选中入口，不画竖条
                bool bar = sel && b != MENU_NavToggle;
                Color barColor = st.AccentBarColor.IsEmpty ? UiKit.Brand : st.AccentBarColor;
                Image ic = AppIcons.Get(st.Icon, UiTheme.Dpi(st.IconSize), st.IconColor, st.AccentColor);
                UiTheme.PaintIconButton(e.Graphics, b.Size, ic, ButtonText(b), st.Font, st.TextColor,
                    st.IconSize, bar, back, b.Enabled ? UiTheme.LockBadgeMode.None : st.LockBadge,
                    barColor, st.ContentOpacity);
            }
            catch { }
        }

        private void StyleNavButton(PictureBox b, AppIcon icon)
        {
            if (b == null) return;
            bool sel = IsSelectedNav(b);
            bool on = b.Enabled;
            // 蓝底：未选/禁用透明；选中只靠左侧白竖条（不铺色块）；禁用半透明白 + 小锁
            Color back = Color.Transparent;
            if (b.BackColor != back) b.BackColor = back;

            NavPaintState st = AttachNavPaint(b, icon, _navRailIconSize, true);
            st.IconColor = !on ? NavOnBlueDisabled : sel ? NavOnBlueSelected : NavOnBlue;
            st.AccentColor = st.IconColor;
            st.TextColor = st.IconColor;
            st.AccentBarColor = NavRailAccentBar;
            st.LockBadge = NavLockBadge;
            st.ContentOpacity = 1f;
            // 设计器不保证触发 Paint：把预览烤进 Image，运行时仍走自绘
            if (UiTheme.IsDesignTime) BakeButtonPreview(b, st);
            else b.Invalidate();
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
                // 常态键与侧栏同系 Brand；危险键保留红
                StyleToolbarButton(AlarmReset, AppIcon.AlarmReset, UiKit.BrandDark, AppIconColor.Danger);
                StyleToolbarButton(MENU_Lock, AppIcon.Lock, UiKit.BrandDark, UiKit.BrandDark);
                StyleToolbarButton(MENU_System, AppIcon.System, UiKit.BrandDark, UiKit.BrandDark);
                StyleToolbarButton(MENU_Exit, AppIcon.Exit, AppIconColor.Danger, AppIconColor.Danger);
            }
            catch { }
        }

        private void StyleToolbarButton(PictureBox btn, AppIcon icon, Color iconColor, Color accentColor)
        {
            if (btn == null) return;
            bool on = btn.Enabled;
            // 底栏统一：禁用也不再铺灰底块，只靠内容淡化 + 小锁；可用态悬停才铺 ToolbarHover
            Color back = (on && btn == _hoverToolbarButton) ? ToolbarHover : Color.Transparent;
            if (btn.BackColor != back) btn.BackColor = back;

            NavPaintState st = AttachNavPaint(btn, icon, ToolIconSize, false);
            // 禁用仍保留语义色相，整体降透明度淡化（见 ContentOpacity）
            st.IconColor = iconColor;
            st.AccentColor = accentColor;
            bool brandTone = iconColor == AppIconColor.Nav || iconColor == UiKit.BrandDark || iconColor == UiKit.Brand;
            st.TextColor = brandTone ? UiKit.BrandDark : iconColor;
            st.AccentBarColor = Color.Empty;
            st.LockBadge = NavLockBadge;
            st.ContentOpacity = on ? 1f : ToolbarDisabledOpacity;
            if (UiTheme.IsDesignTime) BakeButtonPreview(btn, st);
            else btn.Invalidate();
        }

        /// <summary>底栏禁用键内容透明度（不改底色，避免灰块破坏条底统一）。</summary>
        private const float ToolbarDisabledOpacity = 0.38f;

        /// <summary>设计期：把图标+文字画进 PictureBox.Image，避免设计面空白。</summary>
        private void BakeButtonPreview(PictureBox b, NavPaintState st)
        {
            if (b == null || st == null || b.Width <= 0 || b.Height <= 0) return;
            try
            {
                Color back = (b.BackColor.A == 255) ? b.BackColor : Color.FromArgb(238, 243, 248);
                bool sel = st.IsNav && IsSelectedNav(b) && b != MENU_NavToggle;
                Color barColor = st.AccentBarColor.IsEmpty ? UiKit.Brand : st.AccentBarColor;
                Image ic = AppIcons.Get(st.Icon, UiTheme.Dpi(st.IconSize), st.IconColor, st.AccentColor);
                Bitmap bmp = UiTheme.RenderIconButton(b.Size, ic, ButtonText(b), st.Font, st.TextColor,
                    st.IconSize, sel, back,
                    b.Enabled ? UiTheme.LockBadgeMode.None : st.LockBadge, barColor, st.ContentOpacity);
                if (bmp == null) return;
                Image old = b.Image;
                b.Image = bmp;
                if (old != null && !ReferenceEquals(old, bmp))
                {
                    try { old.Dispose(); } catch { }
                }
            }
            catch { }
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
        /// 无权限为浅灰底；顶栏按钮选中为深蓝底；底部工具栏由 ApplyToolbarButtonStyles 设置。
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
                else if (!btn.Enabled)
                    btn.BackColor = UiKit.DisabledBg;
                else
                    btn.BackColor = sel ? UiTheme.Selected : Color.Transparent;
            }
            RefreshButtonIcons();
        }

        // ---------------- 锁态点击反馈（Enabled=false 时子控件收不到 Click，由父面板承接） ----------------

        private readonly ToolTip _lockedTip = new ToolTip { ShowAlways = true, AutomaticDelay = 0, AutoPopDelay = 3200, InitialDelay = 0, ReshowDelay = 0 };
        private bool _lockedClickHooked;

        /// <summary>在导航 / 工具栏父面板上挂 MouseDown：点到禁用按钮时弹出原因说明。</summary>
        private void HookLockedClickFeedback()
        {
            if (_lockedClickHooked) return;
            _lockedClickHooked = true;
            FlowLayoutPanel[] panels = { Panl_PictureShowTop, Panl_PictureShowDown, panel_PictureShowLeft, panel_PictureShowRight };
            for (int i = 0; i < panels.Length; i++)
            {
                FlowLayoutPanel p = panels[i];
                if (p == null) continue;
                p.MouseDown += LockedPanel_MouseDown;
            }
        }

        private void LockedPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            Control panel = sender as Control;
            if (panel == null) return;

            PictureBox hit = FindDisabledButtonAt(panel, e.Location);
            if (hit == null) return;

            string reason = ExplainLockedButton(hit);
            if (string.IsNullOrEmpty(reason)) return;

            try
            {
                Point tipAt = new Point(hit.Left + hit.Width / 2, hit.Top - UiTheme.Dpi(4));
                _lockedTip.Hide(panel);
                _lockedTip.Show(reason, panel, tipAt, 3000);
            }
            catch { }
        }

        private static PictureBox FindDisabledButtonAt(Control panel, Point clientPt)
        {
            foreach (Control c in panel.Controls)
            {
                PictureBox b = c as PictureBox;
                if (b == null || b.Enabled || !b.Visible) continue;
                if (b.Bounds.Contains(clientPt)) return b;
            }
            return null;
        }

        /// <summary>按按钮区分：工具栏多为运行态限制，侧栏多为权限不足。</summary>
        private string ExplainLockedButton(PictureBox b)
        {
            if (b == null) return "";
            string name = ButtonText(b);
            if (string.IsNullOrEmpty(name)) name = b.Name;

            if (b == MENU_Run)
            {
                if (!SysPara.UpConveyorInitialOk)
                    return UiTheme.T("MainForm", "msg_LockNeedInit",
                        "无法启动：请先完成初始化。",
                        "Cannot start: initialize first.",
                        "No se puede iniciar: inicialice primero.");
                if (SysPara.SystemMode == RunMode.RUN)
                    return UiTheme.T("MainForm", "msg_LockAlreadyRun",
                        "设备已在运行中。",
                        "Already running.",
                        "Ya está en marcha.");
                if (SysPara.SystemMode == RunMode.IDLE)
                    return UiTheme.T("MainForm", "msg_LockIdleNeedInit",
                        "待机中不能直接启动，请先点「初始化」。",
                        "Idle: initialize before start.",
                        "En espera: inicialice antes de iniciar.");
                return UiTheme.T("MainForm", "msg_LockRunGeneric",
                    "当前状态不可启动。",
                    "Start is not available now.",
                    "Inicio no disponible ahora.");
            }
            if (b == MENU_Pause)
                return UiTheme.T("MainForm", "msg_LockPause",
                    "仅运行中可暂停。",
                    "Pause is only available while running.",
                    "Pausa solo disponible en marcha.");
            if (b == MENU_Reset)
                return UiTheme.T("MainForm", "msg_LockReset",
                    "仅待机时可初始化。",
                    "Initialize is only available while idle.",
                    "Inicializar solo en espera.");
            if (b == MENU_LifeSpan)
                return UiTheme.T("MainForm", "msg_LockLifeSpan",
                    "寿命页需要维护员或管理员权限。",
                    "Lifespan requires Maintenance or Admin.",
                    "Vida útil requiere Mantenimiento o Admin.");

            return UiTheme.T("MainForm", "msg_LockNoPermissionA",
                    "当前用户无权限打开「",
                    "No permission for «",
                    "Sin permiso para «")
                + name
                + UiTheme.T("MainForm", "msg_LockNoPermissionB",
                    "」。请切换到更高权限账号。",
                    "». Switch to a higher-privilege account.",
                    "». Cambie a una cuenta con más privilegios.");
        }

        /// <summary>把按钮裁成圆角：左侧导航 8px，底部工具栏 12px（直径）。</summary>
        private void ApplyRoundedMenuRegions()
        {
            ApplyRoundRegion(NavButtons(), UiTheme.Dpi(12));
            ApplyRoundRegion(ToolbarButtons(), UiTheme.Dpi(12));
            if (MENU_NavToggle != null)
                ApplyRoundRegion(new PictureBox[] { MENU_NavToggle }, UiTheme.Dpi(12));
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
