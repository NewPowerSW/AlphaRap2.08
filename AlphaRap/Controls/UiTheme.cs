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
        // ---------------- 尺寸 ----------------

        /// <summary>卡片之间的间距。</summary>
        public const int Gap = 12;

        /// <summary>页面四周的留白。</summary>
        public const int PagePad = 16;

        /// <summary>卡片圆角半径。</summary>
        public const int Radius = 8;

        /// <summary>列表行高（触摸操作的最小可点高度）。</summary>
        public const int RowHeight = 32;

        /// <summary>选项卡标签高度。</summary>
        public const int TabHeight = 40;

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
        public static readonly Font NavLabel = UiKit.Regular(10.5f);
        public static readonly Font ToolLabel = UiKit.Bold(10.5f);
        public static readonly Font KpiValue = UiKit.Bold(27f);

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

        // ---------------- 图标 + 文字按钮 ----------------

        /// <summary>
        /// 在按钮上直接绘制"图标在上、文字在下"（导航与工具栏共用）。
        /// 文字用 GDI 直接画到屏幕上（ClearType），图标按原始像素尺寸贴图，不做缩放插值，保证清晰。
        /// </summary>
        /// <param name="accentBar">在左侧画一条品牌色竖条（导航选中态）。</param>
        public static void PaintIconLabel(Graphics g, Size size, Image icon, string text, Font font,
            Color textColor, bool accentBar)
        {
            if (size.Width <= 0 || size.Height <= 0) return;
            int iconH = icon != null ? icon.Height : 0;
            int textH = string.IsNullOrEmpty(text) ? 0 : font.Height;
            int gap = (iconH > 0 && textH > 0) ? 3 : 0;
            int top = Math.Max(2, (size.Height - iconH - gap - textH) / 2);

            if (icon != null)
            {
                InterpolationMode im = g.InterpolationMode;
                PixelOffsetMode pm = g.PixelOffsetMode;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.DrawImage(icon, new Rectangle((size.Width - icon.Width) / 2, top, icon.Width, icon.Height));
                g.InterpolationMode = im;
                g.PixelOffsetMode = pm;
            }

            if (textH > 0)
            {
                Rectangle tr = new Rectangle(1, top + iconH + gap, size.Width - 2, textH);
                TextRenderer.DrawText(g, text, font, tr, textColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.EndEllipsis |
                    TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);
            }

            if (accentBar)
            {
                using (SolidBrush br = new SolidBrush(UiKit.Brand))
                    g.FillRectangle(br, 0, 8, 3, size.Height - 16);
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
            rows.ImageSize = new Size(1, Math.Max(16, rowHeight));
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
            Rectangle tr = new Rectangle(e.Bounds.X + 10, e.Bounds.Y, e.Bounds.Width - 12, e.Bounds.Height);
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
            Rectangle tr = new Rectangle(e.Bounds.X + 10, e.Bounds.Y, e.Bounds.Width - 12, e.Bounds.Height);
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
            tc.Padding = new Point(16, 6);
            tc.Font = primary ? Section : BodyBold;
            tc.ItemSize = new Size(MeasureTabWidth(tc), primary ? TabHeight : TabHeight - 6);
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
            int w = 96;
            foreach (TabPage p in tc.TabPages)
                w = Math.Max(w, TextRenderer.MeasureText(p.Text ?? "", tc.Font).Width + 36);
            return Math.Min(w, 320);
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
                g.FillRectangle(b, new Rectangle(r.X - 2, r.Y - 2, r.Width + 4, r.Height + 4));

            Rectangle card = new Rectangle(r.X + 2, r.Y + 3, r.Width - 4, r.Height - 3);
            if (sel)
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath path = UiKit.RoundRect(new Rectangle(card.X, card.Y, card.Width, card.Height + 8), 6))
                using (SolidBrush b = new SolidBrush(primary ? UiKit.PageBg : UiKit.Surface))
                    g.FillPath(b, path);
                using (SolidBrush b = new SolidBrush(UiKit.Brand))
                    g.FillRectangle(b, card.X + 10, card.Bottom - 3, card.Width - 20, 3);
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
