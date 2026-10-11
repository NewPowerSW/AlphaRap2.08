using System;
using System.Collections.Generic;
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

        /// <summary>按主屏 DPI 初始化缩放系数（必须在创建任何窗口之前调用）。</summary>
        public static void InitDpi()
        {
            EnsureDpiAware();
            try
            {
                // 进程不是 DPI 感知时，Windows 会把整个窗口位图拉伸；这时自己绝不能再缩放（会双重放大）
                if (!IsProcessDPIAware()) { Scale = 1f; return; }

                using (Graphics g = Graphics.FromHwnd(IntPtr.Zero)) Scale = g.DpiX / 96f;
                if (Scale <= 0f) Scale = GetDpiForSystem() / 96f;
            }
            catch
            {
                try { Scale = GetDpiForSystem() / 96f; } catch { Scale = 1f; }
            }
            if (Scale < 1f || Scale > 4f) Scale = 1f;   // 异常值不缩放，免得小屏上界面反而变小
        }

        /// <summary>
        /// 兜底声明 DPI 感知。manifest 正常时这些调用会直接返回（无害）；
        /// 万一 manifest 没被采纳，这里补上——否则整个窗口会被 Windows 位图拉伸，全界面发虚。
        /// 必须在创建第一个窗口之前调用。
        /// </summary>
        private static void EnsureDpiAware()
        {
            try { if (IsProcessDPIAware()) return; } catch { }
            try { SetProcessDpiAwarenessContext(new IntPtr(-2)); return; } catch { }   // SYSTEM_AWARE（Win10 1607+）
            try { SetProcessDpiAwareness(1); return; } catch { }                       // PROCESS_SYSTEM_DPI_AWARE（Win8.1+）
            try { SetProcessDPIAware(); } catch { }                                    // Vista+
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

        /// <summary>主框架内容区底色。</summary>
        public static readonly Color FrameBg = Color.FromArgb(236, 241, 247);

        /// <summary>左侧导航底色。</summary>
        public static readonly Color NavBg = Color.White;

        /// <summary>导航 / 选项卡的选中底色。</summary>
        public static readonly Color Selected = Color.FromArgb(225, 238, 250);

        /// <summary>列表表头底色。</summary>
        public static readonly Color HeaderBg = Color.FromArgb(242, 246, 250);

        /// <summary>状态栏底色。</summary>
        public static readonly Color StatusBar = Color.FromArgb(2, 78, 133);

        /// <summary>报警行（E 错误）底色与文字色。</summary>
        public static readonly Color DangerSoft = Color.FromArgb(253, 236, 236);
        public static readonly Color DangerText = Color.FromArgb(160, 36, 36);

        /// <summary>警告行底色与文字色。</summary>
        public static readonly Color WarnSoft = Color.FromArgb(255, 245, 225);
        public static readonly Color WarnText = Color.FromArgb(138, 88, 0);

        /// <summary>输入点（传感器）的标识色，输出点使用品牌蓝。</summary>
        public static readonly Color InputMark = Color.FromArgb(96, 112, 130);

        // ---------------- 字体 ----------------

        public static readonly Font Caption = UiKit.Regular(9f);
        public static readonly Font Body = UiKit.Regular(10.5f);
        public static readonly Font BodyBold = UiKit.Bold(10.5f);
        public static readonly Font Section = UiKit.Bold(12f);
        public static readonly Font NavLabel = UiKit.Regular(9f);
        public static readonly Font ToolLabel = UiKit.Bold(10f);
        public static readonly Font KpiValue = UiKit.Bold(26f);

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

        // ---------------- 图标 + 文字按钮图 ----------------

        private static readonly Dictionary<string, Image> TileCache = new Dictionary<string, Image>();

        /// <summary>
        /// 生成"图标在上、文字在下"的按钮图（导航与工具栏共用），结果按参数缓存。
        /// </summary>
        /// <param name="accentBar">在左侧画一条品牌色竖条（导航选中态）。</param>
        /// <param name="backColor">
        /// 按钮实际底色（**必须与按钮的 BackColor / 其身后的实色一致**）。
        /// 位图会先用它铺一层不透明底色再画字 —— 透明底上 GDI 用不了 ClearType，
        /// 只能退回灰度抗锯齿、边缘还留半透明像素，叠出来就是"发虚"。
        /// </param>
        public static Image IconLabel(AppIcon icon, string text, Size size, int iconSize,
            Color iconColor, Color accent, Color textColor, Font font, bool accentBar, Color backColor)
        {
            if (size.Width <= 0 || size.Height <= 0) return null;

            Color bg = Color.FromArgb(255, backColor);      // 强制不透明
            string key = Scale + "|" + (int)icon + "|" + text + "|" + size.Width + "x" + size.Height + "|" + iconSize + "|" +
                         iconColor.ToArgb() + "|" + accent.ToArgb() + "|" + textColor.ToArgb() + "|" +
                         font.Size + "|" + font.Style + "|" + accentBar + "|" + bg.ToArgb();
            Image cached;
            if (TileCache.TryGetValue(key, out cached)) return cached;

            Bitmap bmp = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
            // 位图按 96 DPI 建；字号与图标边长在这里手动按 DPI 换算，保证与已放大的位图尺寸匹配
            int icSize = Dpi(iconSize);
            Font f = (Scale == 1f) ? font
                : new Font(font.FontFamily, font.Size * Scale, font.Style, GraphicsUnit.Point, ((byte)(134)));
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                // 先铺不透明底色：只有不透明表面上文字才用得上 ClearType（否则字会发虚发淡）
                using (SolidBrush b = new SolidBrush(bg))
                    g.FillRectangle(b, 0, 0, size.Width, size.Height);

                int textH = string.IsNullOrEmpty(text) ? 0 : f.Height;
                int top = Math.Max(Dpi(2), (size.Height - icSize - textH - Dpi(2)) / 2);
                Image ic = AppIcons.Get(icon, icSize, iconColor, accent);
                if (ic != null) g.DrawImage(ic, (size.Width - icSize) / 2, top, icSize, icSize);

                if (textH > 0)
                {
                    Rectangle tr = new Rectangle(Dpi(2), top + icSize + Dpi(2), size.Width - Dpi(4), textH + Dpi(2));
                    TextRenderer.DrawText(g, text, f, tr, textColor,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.Top |
                        TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
                }

                if (accentBar)
                {
                    using (SolidBrush b = new SolidBrush(UiKit.Brand))
                        g.FillRectangle(b, 0, Dpi(8), Dpi(3), size.Height - Dpi(16));
                }
            }
            if (!ReferenceEquals(f, font)) f.Dispose();
            TileCache[key] = bmp;
            return bmp;
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
            TextRenderer.DrawText(e.Graphics, e.Header.Text, BodyBold, tr, UiKit.TextMuted,
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
        /// </summary>
        /// <param name="primary">一级选项卡用较大字号，嵌套的二、三级选项卡用正文字号。</param>
        public static void StyleTabControl(TabControl tc, bool primary)
        {
            if (tc == null) return;
            tc.DrawMode = TabDrawMode.OwnerDrawFixed;
            tc.SizeMode = TabSizeMode.Fixed;
            tc.Padding = new Point(Dpi(16), Dpi(6));
            tc.Font = primary ? Section : BodyBold;
            tc.ItemSize = new Size(MeasureTabWidth(tc), primary ? TabHeight : TabHeight - Dpi(6));
            tc.Tag = primary ? "tab:1" : "tab:2";
            tc.DrawItem -= TabControl_DrawItem;
            tc.DrawItem += TabControl_DrawItem;
            foreach (TabPage p in tc.TabPages)
            {
                p.BackColor = primary ? UiKit.PageBg : UiKit.Surface;
                p.UseVisualStyleBackColor = false;
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
                sel ? UiKit.Brand : UiKit.TextMuted,
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
