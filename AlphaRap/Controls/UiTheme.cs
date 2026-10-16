using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>
    /// 界面主题：在 <see cref="UiKit"/> 的配色基础上，统一间距、字号、触控尺寸，
    /// 并提供列表、选项卡、图标按钮等通用样式。设计尺寸 1440×900，兼顾触摸与鼠标操作。
    /// </summary>
    public static class UiTheme
    {
        // ---------------- 设计期判断 ----------------

        private static bool _forceDesignTime;

        /// <summary>
        /// 是否运行在 VS 设计器里。
        /// 设计期**绝不能碰 MiddleLayer / 硬件 / 文件** —— MiddleLayer 的静态初始化会构造硬件对象，
        /// 在设计器进程里会抛异常，导致窗体在设计器里直接打不开。
        /// </summary>
        public static bool IsDesignTime
        {
            get
            {
                if (_forceDesignTime) return true;
                try
                {
                    if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return true;
                }
                catch { }
                // VS2022+ 外进程设计器偶发 UsageMode=Runtime，靠进程名兜底，避免设计面空白
                try
                {
                    string n = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                    if (string.Equals(n, "devenv", StringComparison.OrdinalIgnoreCase)) return true;
                    if (string.Equals(n, "DesignToolsServer", StringComparison.OrdinalIgnoreCase)) return true;
                }
                catch { }
                return false;
            }
        }

        /// <summary>仅供渲染探针：强制走设计期分支，好把"设计器里看到的样子"渲染出来核对。</summary>
        public static void ForceDesignTimeForTest(bool on)
        {
            _forceDesignTime = on;
        }

        // ---------------- DPI 缩放 ----------------

        /// <summary>
        /// 全局 DPI 缩放系数（1.0 = 96 DPI 设计基准）。程序启动时由 <see cref="InitDpi"/> 设定。
        /// 设计器里的坐标/尺寸一律按 96 DPI 编写，运行时由 <see cref="ApplyDpi"/> 整体放大。
        /// </summary>
        public static float Scale = 1f;

        /// <summary>把 96 DPI 的设计尺寸换算成当前 DPI 的像素。</summary>
        public static int Dpi(int v) { return Scale == 1f ? v : (int)Math.Round(v * Scale, MidpointRounding.AwayFromZero); }
        public static Size Dpi(Size v) { return Scale == 1f ? v : new Size(Dpi(v.Width), Dpi(v.Height)); }
        public static Point Dpi(Point v) { return Scale == 1f ? v : new Point(Dpi(v.X), Dpi(v.Y)); }
        public static Padding Dpi(Padding v) { return Scale == 1f ? v : new Padding(Dpi(v.Left), Dpi(v.Top), Dpi(v.Right), Dpi(v.Bottom)); }

        // ---------------- DPI 诊断（状态栏显示，用来确认缩放到底有没有生效） ----------------

        /// <summary>初始化时进程是否为 DPI 感知。</summary>
        public static bool AwareAtInit;
        /// <summary>系统 DPI（主屏，GetDpiForSystem 读数）。</summary>
        public static int DpiSystem;
        /// <summary>窗口会被创建到的那块显示器（鼠标所在屏，StartPosition=CenterScreen 的实际落点）。</summary>
        public static Screen StartupScreen;
        /// <summary>窗口会被创建到的那块显示器的 DPI（= StartupScreen 的 DPI）。</summary>
        public static int DpiMonitor;
        /// <summary>DPI 感知的声明途径：already / pmv2 / system / shcore / legacy / gdiscaled / failed。</summary>
        public static string DpiPath = "";

        /// <summary>
        /// 按**窗口所在显示器**的 DPI 初始化缩放系数（必须在创建任何窗口之前调用）。
        /// ⚠ 不能用主屏 DPI：多显示器下窗口所在屏的 DPI 常常和主屏不同，
        /// 而 System 级 DPI 感知在"窗口跨到不同 DPI 的屏"时仍会被 Windows 位图拉伸 ⇒ 照样发虚。
        /// </summary>
        public static void InitDpi()
        {
            DpiPath = EnsureDpiAware();
            AwareAtInit = SafeIsAware();

            DpiSystem = SafeDpiSystem();

            // 窗体 StartPosition = CenterScreen，会开在鼠标所在的那块屏上。
            // ⚠ 缩放系数必须按"窗口实际落在哪块屏"来算，否则窗口在两块不同 DPI 的屏之间会算错。
            Point cur = Point.Empty;
            try { cur = System.Windows.Forms.Cursor.Position; } catch { }
            try { StartupScreen = Screen.FromPoint(cur); } catch { }
            if (StartupScreen == null) { try { StartupScreen = Screen.PrimaryScreen; } catch { } }
            DpiMonitor = StartupScreen != null
                ? DpiOfMonitorAt(new Point(StartupScreen.Bounds.Left + 5, StartupScreen.Bounds.Top + 5))
                : 0;

            if (!AwareAtInit)
            {
                // 进程不是 DPI 感知：Windows 会把整个窗口位图拉伸，此时自己绝不能再缩放（会双重放大）
                Scale = 1f;
                return;
            }

            int dpi = DpiMonitor > 0 ? DpiMonitor : (DpiSystem > 0 ? DpiSystem : 96);
            Scale = dpi / 96f;
            if (Scale < 1f || Scale > 4f) Scale = 1f;   // 异常值不缩放，免得小屏上界面反而变小
        }

        private static bool SafeIsAware()
        {
            try { return IsProcessDPIAware(); } catch { return false; }
        }

        private static int SafeDpiSystem()
        {
            try { uint d = GetDpiForSystem(); if (d > 0) return (int)d; } catch { }
            try { using (Graphics g = Graphics.FromHwnd(IntPtr.Zero)) return (int)g.DpiX; } catch { }
            return 0;
        }

        /// <summary>取某点所在显示器的真实 DPI（与进程是否感知无关）；取不到返回 0。</summary>
        public static int DpiOfMonitorAt(Point p)
        {
            try
            {
                NativePoint np = new NativePoint { X = p.X, Y = p.Y };
                IntPtr mon = MonitorFromPoint(np, 2 /*MONITOR_DEFAULTTONEAREST*/);
                uint dx = 0, dy = 0;
                if (mon != IntPtr.Zero && GetDpiForMonitor(mon, 0 /*MDT_EFFECTIVE_DPI*/, out dx, out dy) == 0 && dx > 0)
                    return (int)dx;
            }
            catch { }
            return 0;
        }

        /// <summary>取某个窗口当前所在显示器的真实 DPI（诊断用）。</summary>
        public static int DpiOfWindow(Control c)
        {
            try { if (c != null && c.Handle != IntPtr.Zero) { uint d = GetDpiForWindow(c.Handle); if (d > 0) return (int)d; } } catch { }
            return 0;
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct NativePoint { public int X; public int Y; }

        /// <summary>
        /// 兜底声明 DPI 感知。manifest 正常时第一个判断即返回（无副作用）；万一 manifest 没被采纳，这里补上。
        /// **优先 Per-Monitor V2** —— 只有它能让"窗口所在屏 DPI ≠ 主屏 DPI"时不被 Windows 位图拉伸。
        /// 必须在创建第一个窗口之前调用。
        /// </summary>
        private static string EnsureDpiAware()
        {
            if (SafeIsAware()) return "already";
            try { if (SetProcessDpiAwarenessContext(new IntPtr(-4)) != IntPtr.Zero) return "pmv2"; } catch { }     // PER_MONITOR_AWARE_V2
            try { if (SetProcessDpiAwarenessContext(new IntPtr(-2)) != IntPtr.Zero) return "system"; } catch { }   // SYSTEM_AWARE
            try { if (SetProcessDpiAwareness(1) == 0) return "shcore"; } catch { }                                  // PROCESS_SYSTEM_DPI_AWARE
            try { if (SetProcessDPIAware()) return "legacy"; } catch { }                                            // Vista+
            try { if (SetProcessDpiAwarenessContext(new IntPtr(-5)) != IntPtr.Zero) return "gdiscaled"; } catch { } // 退路
            return "failed";
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool IsProcessDPIAware();

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetProcessDpiAwarenessContext(IntPtr value);

        [System.Runtime.InteropServices.DllImport("shcore.dll")]
        private static extern int SetProcessDpiAwareness(int value);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern uint GetDpiForSystem();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern uint GetDpiForWindow(IntPtr hwnd);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr MonitorFromPoint(NativePoint pt, uint flags);

        [System.Runtime.InteropServices.DllImport("shcore.dll")]
        private static extern int GetDpiForMonitor(IntPtr hmon, int type, out uint x, out uint y);

        /// <summary>
        /// 把一个窗体的整棵控件树按 DPI 整体放大一次；之后往这棵树里新加的控件会自动跟着放大
        /// （见 <see cref="HookTree"/>），所以运行时自建的页面/控件不必再逐个处理。
        /// 同一个窗体只生效一次，避免嵌套页面被重复放大。
        /// </summary>
        public static void ApplyDpi(Form f)
        {
            if (f == null || Scale == 1f) return;
            if (!Scaled.Add(f)) return;

            Size client = f.ClientSize;
            f.Scale(new SizeF(Scale, Scale));
            // Scale() 对"窗体自身尺寸"的处理依实现而定，这里显式兜底
            f.ClientSize = new Size(Dpi(client.Width), Dpi(client.Height));
            HookTree(f);
        }

        private static readonly HashSet<Control> Scaled = new HashSet<Control>();
        private static readonly HashSet<Control> Hooked = new HashSet<Control>();

        /// <summary>给整棵树挂上"新加控件自动跟随缩放"的钩子。</summary>
        private static void HookTree(Control root)
        {
            if (!Hooked.Add(root)) return;
            root.ControlAdded += TreeChildAdded;
            foreach (Control c in root.Controls) HookTree(c);
        }

        private static void TreeChildAdded(object sender, ControlEventArgs e)
        {
            Control c = e.Control;
            if (c == null) return;
            if (Scaled.Add(c)) c.Scale(new SizeF(Scale, Scale));
            HookTree(c);
        }

        // ---------------- 尺寸（96 DPI 设计值，运行期自动换算） ----------------

        /// <summary>卡片之间的间距。</summary>
        public static int Gap { get { return Dpi(12); } }

        /// <summary>页面四周的留白。</summary>
        public static int PagePad { get { return Dpi(16); } }

        /// <summary>卡片圆角半径。</summary>
        public static int Radius { get { return Dpi(8); } }

        /// <summary>列表行高（触摸操作的最小可点高度）。</summary>
        public static int RowHeight { get { return Dpi(32); } }

        /// <summary>选项卡标签高度。</summary>
        public static int TabHeight { get { return Dpi(40); } }

        // ---------------- 颜色 ----------------

        /// <summary>
        /// 主框架内容区底色（外框 / 页签条）。**必须比 <see cref="UiKit.PageBg"/> 深一档**，
        /// 否则外框与内容区糊成一片；三级层次 = FrameBg &lt; PageBg &lt; Surface(卡片白)。
        /// </summary>
        public static readonly Color FrameBg = Color.FromArgb(231, 237, 244);

        /// <summary>左侧导航底色。</summary>
        public static readonly Color NavBg = Color.White;

        /// <summary>导航 / 选项卡的选中底色（略深一档，侧栏远看也能辨认当前页）。</summary>
        public static readonly Color Selected = Color.FromArgb(198, 224, 246);

        /// <summary>列表表头底色。</summary>
        public static readonly Color HeaderBg = Color.FromArgb(242, 246, 250);

        /// <summary>状态栏底色。</summary>
        /// <summary>底状态条：与 <see cref="UiKit.BrandDark"/> 同色，和顶栏/侧栏一条色带。</summary>
        public static readonly Color StatusBar = Color.FromArgb(2, 78, 133);

        /// <summary>报警行（E 错误）底色与文字色。</summary>
        public static readonly Color DangerSoft = Color.FromArgb(253, 236, 236);
        public static readonly Color DangerText = Color.FromArgb(160, 36, 36);

        /// <summary>警告行底色与文字色。</summary>
        public static readonly Color WarnSoft = Color.FromArgb(255, 245, 225);
        public static readonly Color WarnText = Color.FromArgb(138, 88, 0);

        /// <summary>左侧导航标签文字色：近黑石板（选中态用更深的品牌蓝，在浅蓝底上对比度约 8:1）。</summary>
        public static readonly Color NavText = Color.FromArgb(24, 34, 48);

        /// <summary>导航选中项的文字色（深品牌蓝，配浅蓝底）。</summary>
        public static readonly Color NavTextSelected = Color.FromArgb(2, 78, 133);

        /// <summary>输入点（传感器）的标识色，输出点使用品牌蓝。</summary>
        public static readonly Color InputMark = Color.FromArgb(96, 112, 130);

        // ---------------- 字体 ----------------

        public static readonly Font Caption = UiKit.Regular(9f);
        public static readonly Font Body = UiKit.Regular(10.5f);
        public static readonly Font BodyBold = UiKit.Bold(10.5f);
        public static readonly Font Section = UiKit.Bold(12f);
        /// <summary>左侧导航标签字体。**不加粗** —— 11pt 的雅黑加粗在 96 DPI 下笔画会糊在一起；去粗后补半档字号。</summary>
        public static readonly Font NavLabel = UiKit.Regular(11.5f);
        /// <summary>底部工具栏标签字体（理由同 <see cref="NavLabel"/>）。</summary>
        public static readonly Font ToolLabel = UiKit.Regular(11.5f);
        public static readonly Font KpiValue = UiKit.Bold(26f);
        /// <summary>统计卡片数值字体。</summary>
        public static readonly Font StatValue = UiKit.Bold(20f);

        // ---------------- 多语言文字 ----------------

        /// <summary>
        /// 多语言文案的提供者，参数为 (分组名, 键, 中文, 英文, 西语)。
        /// 程序启动时设为 MiddleLayer.LangMsg；未设置时返回中文。
        /// </summary>
        public static Func<string, string, string, string, string, string> TextProvider;

        /// <summary>取当前语言的文案。</summary>
        public static string T(string group, string key, string zh, string en, string es)
        {
            Func<string, string, string, string, string, string> p = TextProvider;
            if (p != null)
            {
                try
                {
                    string s = p(group, key, zh, en, es);
                    if (!string.IsNullOrEmpty(s)) return s;
                }
                catch { }
            }
            return zh;
        }

        // ---------------- 图标 + 文字按钮（导航 / 工具栏）----------------

        /// <summary>禁用角标的画法。</summary>
        public enum LockBadgeMode
        {
            /// <summary>不画。</summary>
            None,
            /// <summary>画在工位右上角。</summary>
            Corner,
            /// <summary>画在文字左侧、与文字排成一行。</summary>
            Inline
        }

        /// <summary>
        /// 在控件表面上画"图标在上、文字在下"的按钮。
        ///
        /// **图标与文字分开画、不合成位图**：文字走 <see cref="TextRenderer"/> 落在真实控件表面上，
        /// 那里是 GDI 的 ClearType（亚像素）；而把字烤进内存位图只能用灰度抗锯齿，笔画边缘发灰、
        /// 小字号加粗尤其糊。拆开之后字重也能独立设置。
        /// </summary>
        /// <param name="icon">已按目标尺寸取好的矢量位图（见 <see cref="AppIcons.Get"/>）。</param>
        /// <param name="iconSize">图标边长（96 DPI 设计值）。</param>
        /// <param name="backColor">按钮实际底色；透明按钮传 <see cref="Color.Transparent"/> 表示不铺底。</param>
        /// <param name="accentBar">在左侧画一条品牌色竖条（导航选中态）。</param>
        /// <param name="lockBadge">
        /// 禁用角标：**"没有权限"靠形状表达，而不是把字调浅** ——
        /// 颜色一旦浅到能与可用态分辨，就又回到"看不清"；两个诉求挤在同一个通道上必然矛盾。
        /// </param>
        /// <summary>
        /// 把图标按钮画成位图（设计器预览用：VS 设计面经常不触发 PictureBox.Paint，
        /// 需要把结果写到 <see cref="PictureBox.Image"/> 才能看见）。
        /// </summary>
        public static Bitmap RenderIconButton(Size size, Image icon, string text, Font font,
            Color textColor, int iconSize, bool accentBar, Color backColor, LockBadgeMode lockBadge)
        {
            return RenderIconButton(size, icon, text, font, textColor, iconSize, accentBar, backColor, lockBadge, UiKit.Brand, 1f);
        }

        public static Bitmap RenderIconButton(Size size, Image icon, string text, Font font,
            Color textColor, int iconSize, bool accentBar, Color backColor, LockBadgeMode lockBadge, Color accentBarColor)
        {
            return RenderIconButton(size, icon, text, font, textColor, iconSize, accentBar, backColor, lockBadge, accentBarColor, 1f);
        }

        public static Bitmap RenderIconButton(Size size, Image icon, string text, Font font,
            Color textColor, int iconSize, bool accentBar, Color backColor, LockBadgeMode lockBadge,
            Color accentBarColor, float contentOpacity)
        {
            if (size.Width <= 0 || size.Height <= 0) return null;
            Bitmap bmp = new Bitmap(size.Width, size.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(backColor.A == 255 ? backColor : Color.FromArgb(238, 243, 248));
                PaintIconButton(g, size, icon, text, font, textColor, iconSize, accentBar,
                    backColor.A == 255 ? backColor : Color.Transparent, lockBadge, accentBarColor, contentOpacity);
            }
            return bmp;
        }

        public static void PaintIconButton(Graphics g, Size size, Image icon, string text, Font font,
            Color textColor, int iconSize, bool accentBar, Color backColor, LockBadgeMode lockBadge)
        {
            PaintIconButton(g, size, icon, text, font, textColor, iconSize, accentBar, backColor, lockBadge, UiKit.Brand, 1f);
        }

        public static void PaintIconButton(Graphics g, Size size, Image icon, string text, Font font,
            Color textColor, int iconSize, bool accentBar, Color backColor, LockBadgeMode lockBadge, Color accentBarColor)
        {
            PaintIconButton(g, size, icon, text, font, textColor, iconSize, accentBar, backColor, lockBadge, accentBarColor, 1f);
        }

        /// <param name="contentOpacity">图标与文字透明度 0~1；禁用淡化时传约 0.38，底色仍由 backColor 决定（可透明以统一条底）。</param>
        public static void PaintIconButton(Graphics g, Size size, Image icon, string text, Font font,
            Color textColor, int iconSize, bool accentBar, Color backColor, LockBadgeMode lockBadge,
            Color accentBarColor, float contentOpacity)
        {
            if (g == null || size.Width <= 0 || size.Height <= 0) return;
            if (contentOpacity < 0f) contentOpacity = 0f;
            if (contentOpacity > 1f) contentOpacity = 1f;

            if (backColor.A == 255)
                using (SolidBrush b = new SolidBrush(backColor))
                    g.FillRectangle(b, 0, 0, size.Width, size.Height);

            int textH = string.IsNullOrEmpty(text) ? 0 : font.Height;
            int icSize = (icon != null) ? Dpi(iconSize) : 0;
            int gap = (icon != null && textH > 0) ? Dpi(2) : 0;
            int top = Math.Max(Dpi(2), (size.Height - icSize - textH - gap) / 2);
            // TextRenderer 忽略 Color.A，禁用淡化改为与底色插值
            Color fadeBg = backColor.A == 255 ? backColor : UiKit.Surface;
            Color fadeText = BlendToward(textColor, fadeBg, contentOpacity);

            if (icon != null)
                DrawImageOpacity(g, icon, (size.Width - icSize) / 2, top, icSize, icSize, contentOpacity);

            if (textH > 0)
            {
                if (lockBadge == LockBadgeMode.Inline)
                {
                    int badge = Dpi(11);
                    int bgap = Dpi(3);
                    Size ts = TextRenderer.MeasureText(text, font, new Size(int.MaxValue, int.MaxValue),
                        TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
                    int x0 = (size.Width - (badge + bgap + ts.Width)) / 2;
                    if (x0 < Dpi(2)) x0 = Dpi(2);
                    Image lk = AppIcons.Get(AppIcon.Lock, badge, fadeText);
                    if (lk != null)
                        DrawImageOpacity(g, lk, x0, top + icSize + gap + (textH - badge) / 2 + Dpi(1), badge, badge, contentOpacity);
                    Rectangle tr2 = new Rectangle(x0 + badge + bgap, top + icSize + gap,
                        Math.Max(1, size.Width - x0 - badge - bgap - Dpi(2)), textH + Dpi(2));
                    TextRenderer.DrawText(g, text, font, tr2, fadeText,
                        TextFormatFlags.Left | TextFormatFlags.Top |
                        TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
                }
                else
                {
                    Rectangle tr = new Rectangle(Dpi(2), top + icSize + gap, size.Width - Dpi(4), textH + Dpi(2));
                    TextRenderer.DrawText(g, text, font, tr, fadeText,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.Top |
                        TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
                }
            }

            if (lockBadge == LockBadgeMode.Corner)
            {
                // 锁标缩小并用次级色：表达「无权限」但不抢导航文字
                int badge = Dpi(10);
                Image lk = AppIcons.Get(AppIcon.Lock, badge, UiKit.TextMuted);
                if (lk != null) g.DrawImage(lk, size.Width - badge - Dpi(3), Dpi(2), badge, badge);
            }

            if (accentBar)
            {
                Color bar = accentBarColor.IsEmpty ? UiKit.Brand : accentBarColor;
                using (SolidBrush b = new SolidBrush(FadeColor(bar, contentOpacity)))
                    g.FillRectangle(b, 0, Dpi(4), Dpi(4), size.Height - Dpi(8));
            }
        }

        private static Color FadeColor(Color c, float opacity)
        {
            if (opacity >= 0.999f) return c;
            int a = (int)(c.A * opacity + 0.5f);
            if (a < 0) a = 0;
            if (a > 255) a = 255;
            return Color.FromArgb(a, c);
        }

        /// <summary>把前景按 opacity 与背景插值（GDI 文字用，因 TextRenderer 不吃 Alpha）。</summary>
        private static Color BlendToward(Color fg, Color bg, float opacity)
        {
            if (opacity >= 0.999f) return fg;
            if (opacity <= 0f) return bg;
            float k = opacity;
            float ik = 1f - k;
            return Color.FromArgb(
                (int)(fg.R * k + bg.R * ik + 0.5f),
                (int)(fg.G * k + bg.G * ik + 0.5f),
                (int)(fg.B * k + bg.B * ik + 0.5f));
        }

        private static void DrawImageOpacity(Graphics g, Image img, int x, int y, int w, int h, float opacity)
        {
            if (img == null || g == null) return;
            if (opacity >= 0.999f)
            {
                g.DrawImage(img, x, y, w, h);
                return;
            }
            ColorMatrix cm = new ColorMatrix();
            cm.Matrix33 = opacity;
            using (ImageAttributes ia = new ImageAttributes())
            {
                ia.SetColorMatrix(cm, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                g.DrawImage(img, new Rectangle(x, y, w, h), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, ia);
            }
        }

        // ---------------- 列表 ----------------

        /// <summary>
        /// ListView 扁平样式：浅色表头、指定行高、雅黑字体。行内容仍按各项的 BackColor / ForeColor 绘制。
        /// </summary>
        public static void StyleListView(ListView lv, int rowHeight)
        {
            if (lv == null) return;
            lv.Font = Body;
            lv.BorderStyle = BorderStyle.None;
            lv.FullRowSelect = true;
            lv.GridLines = false;
            lv.BackColor = UiKit.Surface;
            lv.ForeColor = UiKit.TextPrimary;

            // 行高由 SmallImageList 的高度决定
            ImageList rows = new ImageList();
            rows.ImageSize = new Size(1, Dpi(Math.Max(16, rowHeight)));
            lv.SmallImageList = rows;

            lv.OwnerDraw = true;
            lv.DrawColumnHeader -= ListView_DrawColumnHeader;
            lv.DrawColumnHeader += ListView_DrawColumnHeader;
            lv.DrawItem -= ListView_DrawItem;
            lv.DrawItem += ListView_DrawItem;
            lv.DrawSubItem -= ListView_DrawSubItem;
            lv.DrawSubItem += ListView_DrawSubItem;
        }

        private static void ListView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            using (SolidBrush b = new SolidBrush(HeaderBg)) e.Graphics.FillRectangle(b, e.Bounds);
            using (Pen p = new Pen(UiKit.Line))
                e.Graphics.DrawLine(p, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            Rectangle tr = new Rectangle(e.Bounds.X + Dpi(10), e.Bounds.Y, e.Bounds.Width - Dpi(12), e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, e.Header.Text, BodyBold, tr, UiKit.TextSecondary,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        private static void ListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            ListView lv = (ListView)sender;
            if (lv.View != View.Details) e.DrawDefault = true;
        }

        private static void ListView_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            ListViewItem item = e.Item;
            bool selected = item.Selected;
            Color back = selected ? Selected : item.BackColor;
            Color fore = item.ForeColor;
            using (SolidBrush b = new SolidBrush(back)) e.Graphics.FillRectangle(b, e.Bounds);
            using (Pen p = new Pen(UiKit.Line))
                e.Graphics.DrawLine(p, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            Rectangle tr = new Rectangle(e.Bounds.X + Dpi(10), e.Bounds.Y, e.Bounds.Width - Dpi(12), e.Bounds.Height);
            TextRenderer.DrawText(e.Graphics, e.SubItem.Text, Body, tr, fore,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);
        }

        // ---------------- 选项卡 ----------------

        /// <summary>
        /// 标准 TabControl 的扁平样式：自绘标签头（选中项白底 + 品牌色下划线），标签宽度随文字。
        /// 设计期与运行期共用同一套 Font / ItemSize / Padding / 页底色（写进 Designer 也应贴近这些值）；
        /// 仅 OwnerDraw 在设计器里不可靠，故设计期 DrawMode=Normal，运行期再挂自绘。
        /// </summary>
        /// <param name="primary">一级选项卡用较大字号，嵌套的二、三级选项卡用正文字号。</param>
        public static void StyleTabControl(TabControl tc, bool primary)
        {
            if (tc == null) return;
            tc.SizeMode = TabSizeMode.Fixed;
            tc.Padding = new Point(Dpi(16), Dpi(6));
            tc.Font = primary ? Section : BodyBold;
            tc.ItemSize = new Size(MeasureTabWidth(tc), primary ? TabHeight : TabHeight - Dpi(6));
            tc.Tag = primary ? "tab:1" : "tab:2";
            foreach (TabPage p in tc.TabPages)
            {
                p.BackColor = primary ? UiKit.PageBg : UiKit.Surface;
                p.UseVisualStyleBackColor = false;
            }

            // VS 设计面里 OwnerDraw 经常不触发 DrawItem，标签头会变成空白框；
            // 设计期改用系统绘制，保证能看见文字（与 EXE 文案/尺寸一致）。
            if (IsDesignTime)
            {
                tc.DrawMode = TabDrawMode.Normal;
                tc.DrawItem -= TabControl_DrawItem;
                return;
            }

            tc.DrawMode = TabDrawMode.OwnerDrawFixed;
            tc.DrawItem -= TabControl_DrawItem;
            tc.DrawItem += TabControl_DrawItem;
        }

        /// <summary>
        /// 递归给 root 下所有 TabControl 刷扁平样式。
        /// <paramref name="primary"/> 非空时该实例按一级，其余按二级。
        /// </summary>
        public static void StyleDescendantTabControls(Control root, TabControl primary)
        {
            if (root == null) return;
            StyleDescendantTabControlsCore(root, primary);
        }

        private static void StyleDescendantTabControlsCore(Control parent, TabControl primary)
        {
            foreach (Control c in parent.Controls)
            {
                TabControl tc = c as TabControl;
                if (tc != null)
                    StyleTabControl(tc, primary != null && ReferenceEquals(tc, primary));
                if (c.HasChildren)
                    StyleDescendantTabControlsCore(c, primary);
            }
        }

        /// <summary>按最长的标签文字计算统一的标签宽度（TabSizeMode.Fixed 下所有标签等宽）。</summary>
        public static int MeasureTabWidth(TabControl tc)
        {
            int w = Dpi(96);
            foreach (TabPage p in tc.TabPages)
                w = Math.Max(w, TextRenderer.MeasureText(p.Text ?? "", tc.Font).Width + Dpi(36));
            return Math.Min(w, Dpi(320));
        }

        private static void TabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabControl tc = (TabControl)sender;
            if (e.Index < 0 || e.Index >= tc.TabPages.Count) return;
            bool primary = (tc.Tag as string) == "tab:1";
            bool sel = e.Index == tc.SelectedIndex;
            Graphics g = e.Graphics;
            Rectangle r = e.Bounds;

            // 标签条整体底色（覆盖系统绘制的边框）
            Color strip = primary ? FrameBg : UiKit.PageBg;
            using (SolidBrush b = new SolidBrush(strip))
                g.FillRectangle(b, new Rectangle(r.X - Dpi(2), r.Y - Dpi(2), r.Width + Dpi(4), r.Height + Dpi(4)));

            Rectangle card = new Rectangle(r.X + Dpi(2), r.Y + Dpi(3), r.Width - Dpi(4), r.Height - Dpi(3));
            if (sel)
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = UiKit.RoundRect(new Rectangle(card.X, card.Y, card.Width, card.Height + Dpi(8)), 6))
                using (SolidBrush b = new SolidBrush(primary ? UiKit.PageBg : UiKit.Surface))
                    g.FillPath(b, path);
                using (SolidBrush b = new SolidBrush(UiKit.Brand))
                    g.FillRectangle(b, card.X + Dpi(10), card.Bottom - Dpi(3), card.Width - Dpi(20), Dpi(3));
            }

            TextRenderer.DrawText(g, tc.TabPages[e.Index].Text, tc.Font, card,
                sel ? UiKit.Brand : UiKit.TextSecondary,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);
        }

        // ---------------- 按钮 ----------------

        /// <summary>普通 Button 的扁平样式。</summary>
        /// <param name="primary">主要操作：品牌色实心；否则白底描边。</param>
        public static void StyleButton(Button b, bool primary)
        {
            if (b == null) return;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.BorderColor = primary ? UiKit.Brand : UiKit.LineStrong;
            b.FlatAppearance.MouseOverBackColor = primary ? UiKit.BrandDark : UiKit.Hover;
            b.FlatAppearance.MouseDownBackColor = primary ? UiKit.BrandDark : Selected;
            b.BackColor = primary ? UiKit.Brand : UiKit.Surface;
            b.ForeColor = primary ? Color.White : UiKit.TextPrimary;
            b.Font = BodyBold;
            b.Cursor = Cursors.Hand;
            b.UseVisualStyleBackColor = false;
        }

        /// <summary>输入框：浅蓝灰底 + 主题字色（与 FieldBox 内层一致）。</summary>
        public static void StyleTextBox(TextBox t)
        {
            if (t == null) return;
            t.BorderStyle = BorderStyle.FixedSingle;
            t.BackColor = UiKit.FieldBg;
            t.ForeColor = UiKit.TextPrimary;
            t.Font = Body;
        }

        /// <summary>说明/字段标签：次级字色。</summary>
        public static void StyleCaption(Label l, bool bold)
        {
            if (l == null) return;
            l.Font = bold ? BodyBold : Body;
            l.ForeColor = bold ? UiKit.TextPrimary : UiKit.TextSecondary;
            l.BackColor = Color.Transparent;
        }

        // ---------------- 设计预览 ----------------

        /// <summary>
        /// 设计预览入口：窗体构造完成后调用其样式方法（ApplyFrameStyle / ApplyPageStyle），
        /// 与运行时构造函数中的调用一致。
        /// </summary>
        public static void ApplyPreview(Control c)
        {
            if (c == null) return;
            foreach (string name in new[] { "ApplyFrameStyle", "ApplyPageStyle" })
            {
                MethodInfo m = c.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (m != null && m.GetParameters().Length == 0) m.Invoke(c, null);
            }
        }
    }
}
