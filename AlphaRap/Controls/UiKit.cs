using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>
    /// 登录 / 用户管理两个窗体共用的视觉规范。
    ///
    /// 配色直接沿用主界面：品牌蓝顶栏、白色工作区、浅蓝灰页面底色。
    /// 这里集中定义，避免两个窗体各写一套色值——以前正是因为色值散落各处，
    /// 改一次底色就要满工程找十六进制。
    /// </summary>
    public static class UiKit
    {
        // ---------------- 品牌色 ----------------

        public static readonly Color Brand = Color.FromArgb(4, 108, 182);
        public static readonly Color BrandDark = Color.FromArgb(2, 78, 133);
        public static readonly Color BrandSoft = Color.FromArgb(230, 241, 251);

        // ---------------- 面 / 线 / 字 ----------------

        public static readonly Color PageBg = Color.FromArgb(244, 248, 249);
        public static readonly Color Surface = Color.White;
        public static readonly Color Line = Color.FromArgb(223, 231, 240);
        public static readonly Color LineStrong = Color.FromArgb(203, 216, 230);
        public static readonly Color TextPrimary = Color.FromArgb(38, 50, 64);
        public static readonly Color TextMuted = Color.FromArgb(122, 138, 155);
        public static readonly Color TextOnBrand = Color.White;
        public static readonly Color Hover = Color.FromArgb(226, 236, 247);
        public static readonly Color FieldBg = Color.FromArgb(250, 252, 254);
        public static readonly Color DisabledBg = Color.FromArgb(232, 237, 243);
        public static readonly Color DisabledText = Color.FromArgb(166, 177, 190);

        // ---------------- 语义色 ----------------

        public static readonly Color Success = Color.FromArgb(46, 150, 67);
        public static readonly Color Warn = Color.FromArgb(214, 158, 32);
        public static readonly Color Danger = Color.FromArgb(206, 62, 62);

        public const string FontName = "微软雅黑";

        public static Font Regular(float size)
        {
            return new Font(FontName, size, FontStyle.Regular, GraphicsUnit.Point, ((byte)(134)));
        }

        public static Font Bold(float size)
        {
            return new Font(FontName, size, FontStyle.Bold, GraphicsUnit.Point, ((byte)(134)));
        }

        // ---------------- 圆角 / 版式 ----------------

        /// <summary>把控件裁剪成圆角，并在尺寸变化时自动重算。</summary>
        public static void Round(Control c, int radius)
        {
            Round(c, radius, true);
        }

        public static void Round(Control c, int radius, bool autoResize)
        {
            if (c == null) return;
            ApplyRegion(c, radius);
            if (autoResize)
            {
                Control target = c;
                int r = radius;
                c.Resize += delegate { ApplyRegion(target, r); };
            }
        }

        private static void ApplyRegion(Control c, int radius)
        {
            try
            {
                if (c.Width <= 0 || c.Height <= 0) return;
                int d = Math.Max(2, Math.Min(radius * 2, Math.Min(c.Width, c.Height)));
                using (GraphicsPath path = new GraphicsPath())
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

        /// <summary>圆角矩形路径（统一走这里，半径自动收敛，不会溢出短边）。</summary>
        public static GraphicsPath RoundRect(Rectangle r, int radius)
        {
            int d = Math.Max(2, Math.Min(radius * 2, Math.Min(r.Width, r.Height)));
            GraphicsPath path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        /// <summary>
        /// 向上找到第一个"不透明"的祖先背景色。
        ///
        /// 自绘控件要先把整块区域刷成父容器底色，才能把圆角以外的部分"擦掉"。
        /// 但 TableLayoutPanel 常被设成 BackColor = Transparent（A=0），
        /// 直接拿它去 g.Clear 会刷成黑色 —— 必须一路往上找。
        /// </summary>
        public static Color ResolveParentBack(Control c)
        {
            try
            {
                Control p = (c != null) ? c.Parent : null;
                int guard = 0;
                while (p != null && guard++ < 64)
                {
                    if (p.BackColor.A == 255) return p.BackColor;
                    p = p.Parent;
                }
            }
            catch { }
            return Surface;
        }

        /// <summary>卡片标题行：一条主标题 + 一条说明。</summary>
        public static Label Caption(string text, float size, bool bold, Color color)
        {
            // 用 UiLabel 并显式起名：裸 Label 会被语言扫描按名字登记，
            // 名字为空时 XMLExpand.GetElement 会拼出 "中文/窗体名/" 这种空段路径，直接抛 XPathException。
            UiLabel l = new UiLabel();
            l.Name = "uiCaption";
            l.AutoSize = true;
            l.Text = text;
            l.Font = bold ? Bold(size) : Regular(size);
            l.ForeColor = color;
            l.BackColor = Color.Transparent;
            return l;
        }

        /// <summary>
        /// 把 DataGridView 刷成扁平样式。
        /// 原来的网格是 3D 蚀刻表头 + 凹陷单元格边框，很显旧；
        /// 这里统一成"浅色表头 + 无单元格边框 + 斑马纹 + 品牌色选中行"。
        /// </summary>
        public static void StyleGrid(DataGridView g)
        {
            if (g == null) return;
            try
            {
                g.BorderStyle = BorderStyle.None;
                g.BackgroundColor = Surface;
                g.GridColor = Line;
                g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
                g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
                g.RowHeadersVisible = false;
                g.AllowUserToResizeRows = false;
                g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                g.EnableHeadersVisualStyles = false;

                g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 245, 250);
                g.ColumnHeadersDefaultCellStyle.ForeColor = TextPrimary;
                g.ColumnHeadersDefaultCellStyle.Font = Bold(9.5f);
                g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(240, 245, 250);
                g.ColumnHeadersDefaultCellStyle.SelectionForeColor = TextPrimary;
                g.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
                g.ColumnHeadersHeight = 36;
                g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

                g.DefaultCellStyle.BackColor = Surface;
                g.DefaultCellStyle.ForeColor = TextPrimary;
                g.DefaultCellStyle.SelectionBackColor = BrandSoft;
                g.DefaultCellStyle.SelectionForeColor = BrandDark;
                g.DefaultCellStyle.Font = Regular(9.5f);
                g.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
                g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 251, 253);
                g.RowTemplate.Height = 32;
                g.Font = Regular(9.5f);
            }
            catch { }
        }

        /// <summary>在指定 Graphics 上画一条水平分隔线。</summary>
        public static void HLine(Graphics g, int x1, int x2, int y, Color color)
        {
            using (Pen p = new Pen(color, 1f))
                g.DrawLine(p, x1, y + 0.5f, x2, y + 0.5f);
        }

        /// <summary>
        /// 登录页主视觉用的大尺寸指纹图形。
        /// 特意不复用 AppIcons 的 24 网格图标：那套描边按比例放大会粗到把纹路之间的缝隙吃光，
        /// 大尺寸必须单独用细描边（这里按 100 的设计网格、约 3px 描边）。
        /// </summary>
        public static Image HeroFingerprint(int size, Color color)
        {
            try
            {
                if (size < 32) size = 32;
                Bitmap bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    float k = size / 100f;
                    using (Pen p = new Pen(color, Math.Max(1.6f, 2.6f * k)))
                    {
                        p.StartCap = LineCap.Round;
                        p.EndCap = LineCap.Round;
                        g.DrawArc(p, 14f * k, 10f * k, 72f * k, 72f * k, 200f, 140f);
                        g.DrawArc(p, 25f * k, 21f * k, 50f * k, 50f * k, 200f, 140f);
                        g.DrawArc(p, 36f * k, 32f * k, 28f * k, 28f * k, 200f, 140f);
                        g.DrawLine(p, 50f * k, 41f * k, 50f * k, 60f * k);
                        g.DrawArc(p, 20f * k, 52f * k, 60f * k, 34f * k, 36f, 48f);
                        g.DrawArc(p, 33f * k, 60f * k, 34f * k, 24f * k, 30f, 42f);
                    }
                }
                return bmp;
            }
            catch { return null; }
        }
    }

    /// <summary>
    /// 圆角卡片面板：自绘白底 + 1px 浅描边 + 圆角。
    ///
    /// 为什么不用"Panel + Region"：Region 是硬裁（边缘锯齿），而且 Panel 的边框画在矩形边上、
    /// 圆角处会被裁出缺口。只有整块自绘才能同时得到干净的圆角与抗锯齿描边。
    /// </summary>
    public class CardPanel : Panel
    {
        public int CornerRadius { get; set; }
        public Color BorderColor { get; set; }
        public bool ShowBorder { get; set; }
        public Color FillColor { get; set; }

        public CardPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            CornerRadius = 10;
            BorderColor = UiKit.Line;
            ShowBorder = true;
            FillColor = UiKit.Surface;
            BackColor = UiKit.Surface;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(UiKit.ResolveParentBack(this));

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = UiKit.RoundRect(rect, CornerRadius))
            {
                using (SolidBrush b = new SolidBrush(FillColor)) g.FillPath(b, path);
                if (ShowBorder)
                    using (Pen p = new Pen(BorderColor, 1f)) g.DrawPath(p, path);
            }
        }
    }

    /// <summary>
    /// 登录页左侧的品牌区：深蓝渐变 + 顶部高光，自绘。
    /// 单独做成控件是为了让渐变在窗体拉伸时始终铺满（用 PNG 背景会有拉伸接缝）。
    /// </summary>
    public class BrandPanel : Panel
    {
        public BrandPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = UiKit.Brand;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (Width <= 0 || Height <= 0) return;

            // 对角渐变：左上偏亮 → 右下品牌深色
            using (LinearGradientBrush b = new LinearGradientBrush(
                new Rectangle(0, 0, Width, Height),
                Color.FromArgb(12, 126, 202), UiKit.BrandDark, 55f))
            {
                g.FillRectangle(b, 0, 0, Width, Height);
            }

            // 右下角一层极淡的同心圆，作为"视觉检测"的暗示（很轻，不抢内容）
            using (Pen p = new Pen(Color.FromArgb(26, 255, 255, 255), 1.4f))
            {
                for (int i = 0; i < 3; i++)
                {
                    int r = 120 + i * 54;
                    g.DrawEllipse(p, Width - r / 2 - 40, Height - r / 2 - 30, r, r);
                }
            }
        }
    }

    /// <summary>
    /// 不受语言包接管的 Label。
    ///
    /// 为什么需要它：
    ///   MiddleLayer.InitialLanguageData() 会把每个控件的文字登记进三份语言表，
    ///   而登记时用的是"控件当时的 Text"。这些页面是在 SysPara.LanguageShow 被赋值**之前**
    ///   创建的（SysPara.LanguageShow 的初值是 English），于是登记进去的是英文；
    ///   更要命的是 InitialLanguageData() 每次启动都会用这份登记结果**覆盖写回**
    ///   Chinese.xml / English.xml / Español.xml —— 中文表里也被写成英文，
    ///   之后切中文就会把英文再贴回来，页面永远切不回中文。
    ///
    /// 而语言扫描的白名单是**精确类型**比对（ControlType == typeof(Label)），
    /// 所以只要继承一层，控件就不会被登记：
    ///   → 不会被语言包改写，也不会把我的页面文案污染进语言 XML；
    ///   → 文案完全由页面自己的 ApplyLanguage() 决定。
    ///
    /// 按钮侧同理：FlatButton 继承自 Control、FieldBox 继承自 Panel，本来就不在白名单里。
    /// </summary>
    public class UiLabel : Label
    {
    }

    /// <summary>不受语言包接管的 CheckBox（理由见 <see cref="UiLabel"/>）。</summary>
    public class UiCheckBox : CheckBox
    {
    }

    /// <summary>
    /// 不受语言包接管的 Button（理由见 <see cref="UiLabel"/>）。
    ///
    /// 运行时（代码里）创建的按钮尤其要用它：
    /// 语言扫描会把 `Button` 按名字登记进语言表，然后 XMLExpand.GetElement 拿名字拼 XPath，
    /// **名字为空的控件会拼出 "Chinese/FormName/" 这种带空段的路径，直接抛
    /// XPathException(表达式的计算结果必须为节点集)** —— 设计器里的控件都有名字所以从没暴露过，
    /// 代码里 new 出来的控件一旦忘了起名，程序启动就崩。
    /// </summary>
    public class UiButton : Button
    {
    }
}
