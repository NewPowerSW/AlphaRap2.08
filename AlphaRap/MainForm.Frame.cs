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
            Mes,
            Robot
        }

        /// <summary>当前选中的页面。</summary>
        private MENU_PageType MENU_SelectPage = MENU_PageType.Home;

        /// <summary>菜单按钮，下标与 <see cref="MENU_PageType"/> 一致。</summary>
        private PictureBox[] MENU_Picture;

        // ---------------- 尺寸 ----------------

        private static readonly Size NavButtonSize = new Size(74, 54);
        private const int NavIconSize = 24;
        private static readonly Size ToolButtonSize = new Size(92, 58);
        private const int ToolIconSize = 24;    // 图标在 24 网格上绘制，取整数倍才不会因缩放发虚

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
            RefreshButtonIcons();
            RefreshMenuBackcolor();
        }

        // ---------------- 顶栏 ----------------

        private void StyleTopBar()
        {
            lbRecipeName.Font = UiKit.Bold(10.5f);
            lbRecipeName.ForeColor = Color.White;
            LoginText.Font = UiTheme.Body;
            LoginText.ForeColor = Color.FromArgb(214, 230, 246);
            MachineStatus.Font = UiKit.Bold(15f);
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
                flow.Padding = new Padding(0, 6, 0, 0);
                flow.BackColor = Color.Transparent;
            }
            foreach (PictureBox b in NavButtons())
            {
                b.Size = NavButtonSize;
                b.Margin = new Padding(1, 2, 1, 2);
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
            panel_PictureShowLeft.Width = 5 * (ToolButtonSize.Width + 8) + 16;
            panel_PictureShowRight.Width = 3 * (ToolButtonSize.Width + 8) + 16;
            panel_PictureShowLeft.Padding = new Padding(12, 4, 0, 0);
            panel_PictureShowRight.Padding = new Padding(0, 4, 8, 0);
            panel_PictureShowLeft.BackColor = Color.Transparent;
            panel_PictureShowRight.BackColor = Color.Transparent;
            foreach (PictureBox b in ToolbarButtons())
            {
                b.Size = ToolButtonSize;
                b.Margin = new Padding(4, 1, 4, 1);
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
            uiTabControl1.Font = UiKit.Bold(10.5f);
            uiTabControl1.ItemSize = new Size(132, UiTheme.TabHeight);
            uiTabControl1.FillColor = UiTheme.FrameBg;
            uiTabControl1.TabBackColor = UiTheme.FrameBg;
            uiTabControl1.TabSelectedColor = UiKit.PageBg;
            uiTabControl1.TabSelectedForeColor = UiKit.Brand;
            uiTabControl1.TabUnSelectedForeColor = UiKit.TextMuted;
            uiTabControl1.TabSelectedHighColor = UiKit.Brand;
            uiTabControl1.TabSelectedHighColorSize = 3;
        }

        // ---------------- 报警栏 ----------------

        private void StyleAlarmPanel()
        {
            panelAlarmFilter.Height = 38;
            panelAlarmFilter.BackColor = UiKit.Surface;
            flowAlarmFilter.Padding = new Padding(8, 5, 0, 0);
            flowAlarmFilter.BackColor = UiKit.Surface;
            foreach (Control chip in new Control[] { btnAlarmFilterAll, btnAlarmFilterWarn, btnAlarmFilterAlarm })
            {
                AlarmChip c = (AlarmChip)chip;
                c.Height = 28;
                c.Margin = new Padding(0, 0, 8, 0);
                c.CornerRadius = 14;
                c.BorderColor = Color.FromArgb(200, 210, 222);
                c.TextColor = UiKit.TextPrimary;
            }

            UiTheme.StyleListView(WarnningMessage, 30);
            UiTheme.StyleListView(listView1, UiTheme.RowHeight);
            if (WarnningMessage.Columns.Count >= 4)
            {
                WarnningMessage.Columns[0].Width = 180;
                WarnningMessage.Columns[1].Width = 80;
                WarnningMessage.Columns[2].Width = 90;
                WarnningMessage.Columns[3].Width = 900;
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

                SetNavImage(MENU_Home, AppIcon.Home);
                SetNavImage(MENU_Save, AppIcon.Save);
                SetNavImage(MENU_Rapid, AppIcon.Rapid);
                SetNavImage(MENU_Vision, AppIcon.Vision);
                SetNavImage(MENU_Hard, AppIcon.Hard);
                SetNavImage(MENU_Manual, AppIcon.Manual);
                SetNavImage(MENU_Data, AppIcon.Data);
                SetNavImage(MENU_Log, AppIcon.Log);
                SetNavImage(MENU_Check, AppIcon.Check);
                SetNavImage(MENU_AddUser, AppIcon.AddUser);
                SetNavImage(MENU_Mes, AppIcon.Mes);
                SetNavImage(MENU_LifeSpan, AppIcon.LifeSpan);

                ApplyToolbarButtonStyles();

                // 顶栏：深蓝底上的白色图标
                SetImage(MENU_Product, AppIcons.Get(AppIcon.Product, 24,
                    MENU_Product.Enabled ? AppIconColor.OnDarkBar : AppIconColor.DisabledOnDark));
                SetImage(MENU_Login, AppIcons.Get(AppIcon.Login, 24,
                    MENU_Login.Enabled ? AppIconColor.OnDarkBar : AppIconColor.DisabledOnDark));
            }
            catch { }
        }

        private static void SetImage(PictureBox b, Image img)
        {
            if (b != null && !ReferenceEquals(b.Image, img)) b.Image = img;
        }

        /// <summary>导航 / 工具栏按钮的显示内容：图标、文字、文字色、是否画选中竖条。</summary>
        private sealed class ButtonFace
        {
            public readonly Image Icon;
            public readonly string Text;
            public readonly Font Font;
            public readonly Color TextColor;
            public readonly bool AccentBar;

            public ButtonFace(Image icon, string text, Font font, Color textColor, bool accentBar)
            {
                Icon = icon;
                Text = text ?? "";
                Font = font;
                TextColor = textColor;
                AccentBar = accentBar;
            }

            public bool SameAs(ButtonFace o)
            {
                return o != null && ReferenceEquals(Icon, o.Icon) && Text == o.Text && ReferenceEquals(Font, o.Font) &&
                       TextColor.ToArgb() == o.TextColor.ToArgb() && AccentBar == o.AccentBar;
            }
        }

        private readonly Dictionary<PictureBox, ButtonFace> _buttonFaces = new Dictionary<PictureBox, ButtonFace>();

        /// <summary>设置按钮显示内容；内容变化时才重绘。文字在 Paint 中直接绘制，不预先烘焙成图片。</summary>
        private void SetFace(PictureBox b, ButtonFace face)
        {
            if (b.Image != null) b.Image = null;
            ButtonFace old;
            if (_buttonFaces.TryGetValue(b, out old))
            {
                if (old.SameAs(face)) return;
            }
            else
            {
                b.Paint += ButtonFace_Paint;
            }
            _buttonFaces[b] = face;
            b.Invalidate();
        }

        private void ButtonFace_Paint(object sender, PaintEventArgs e)
        {
            PictureBox b = sender as PictureBox;
            ButtonFace f;
            if (b == null || !_buttonFaces.TryGetValue(b, out f)) return;
            UiTheme.PaintIconLabel(e.Graphics, b.ClientSize, f.Icon, f.Text, f.Font, f.TextColor, f.AccentBar);
        }

        private bool IsSelectedNav(PictureBox b)
        {
            int i = (int)MENU_SelectPage;
            return MENU_Picture != null && i >= 0 && i < MENU_Picture.Length && MENU_Picture[i] == b;
        }

        private void SetNavImage(PictureBox b, AppIcon icon)
        {
            if (b == null) return;
            bool sel = IsSelectedNav(b);
            Color c = !b.Enabled ? AppIconColor.Disabled : sel ? UiKit.Brand : AppIconColor.Nav;
            Color t = !b.Enabled ? UiKit.DisabledText : sel ? UiKit.Brand : UiKit.TextPrimary;
            SetFace(b, new ButtonFace(AppIcons.Get(icon, NavIconSize, c), ButtonText(b), UiTheme.NavLabel, t, sel));
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

            Color main = on ? iconColor : AppIconColor.Disabled;
            Color accent = on ? accentColor : AppIconColor.Disabled;
            Color text = !on ? UiKit.DisabledText : (iconColor == AppIconColor.Nav ? UiKit.TextPrimary : iconColor);
            SetFace(btn, new ButtonFace(AppIcons.Get(icon, ToolIconSize, main, accent), ButtonText(btn), UiTheme.ToolLabel, text, false));
        }

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
            ApplyRoundRegion(NavButtons(), 12);
            ApplyRoundRegion(ToolbarButtons(), 12);
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
