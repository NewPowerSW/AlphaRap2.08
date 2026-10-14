using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>图表方向。</summary>
    public enum BarChartOrientation
    {
        /// <summary>横向条形（项目名在左，适合"Top 排行"）。</summary>
        Horizontal,
        /// <summary>纵向柱状（标签在下方，适合"按天趋势"）。</summary>
        Vertical
    }

    /// <summary>
    /// 轻量条形图：自绘白色圆角卡片 + 标题 + 若干等宽条。
    /// 数据一次性由 SetData 给出，绘制时按当前尺寸换算；不创建任何子控件。
    /// </summary>
    public class BarChart : Control
    {
        private readonly List<KeyValuePair<string, int>> _data = new List<KeyValuePair<string, int>>();

        /// <summary>横向条的底槽色：把"满格"的基准画出来，短条也能一眼读出比例。</summary>
        private static readonly Color TrackColor = Color.FromArgb(240, 245, 250);

        /// <summary>横向单行的最大高度，避免条数很少时段距被拉得过大。</summary>
        private const int MaxRowH = 38;

        public BarChart()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = UiKit.PageBg;
            Font = UiTheme.Caption;
            Orientation = BarChartOrientation.Horizontal;
            BarColor = UiKit.Brand;
            EmptyText = "无数据";
        }

        /// <summary>卡片标题。</summary>
        public string Title { get; set; }

        public BarChartOrientation Orientation { get; set; }

        public Color BarColor { get; set; }

        public string EmptyText { get; set; }

        /// <summary>最高可显示几根条（超出只显示前 N 根）。</summary>
        public int MaxBars { get; set; }

        /// <summary>设置数据（会重绘）。value 为 0 的项保留占位，便于看出"那天没报警"。</summary>
        public void SetData(IList<KeyValuePair<string, int>> data)
        {
            _data.Clear();
            int n = data == null ? 0 : data.Count;
            if (MaxBars > 0 && n > MaxBars) n = MaxBars;
            for (int i = 0; i < n; i++) _data.Add(data[i]);
            Invalidate();
        }

        /// <summary>设计期示例内容：VS 设计器里也照真实版式画一遍，否则这块永远只显示"无数据"。</summary>
        private void LoadDesignSamples()
        {
            if (string.IsNullOrEmpty(Title))
                Title = (Orientation == BarChartOrientation.Horizontal) ? "报警号 Top 10" : "每日次数";

            if (Orientation == BarChartOrientation.Horizontal)
            {
                _data.Add(new KeyValuePair<string, int>("4014", 14));
                _data.Add(new KeyValuePair<string, int>("3014", 6));
                _data.Add(new KeyValuePair<string, int>("3021", 3));
                _data.Add(new KeyValuePair<string, int>("1000", 1));
            }
            else
            {
                int[] v = { 0, 0, 0, 0, 0, 0, 8, 0, 0, 0, 0, 5, 0, 0, 0, 2 };
                for (int i = 0; i < v.Length; i++)
                    _data.Add(new KeyValuePair<string, int>(string.Format("10-{0:00}", i + 1), v[i]));
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);

            if (_data.Count == 0 && LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                LoadDesignSamples();

            Rectangle card = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = UiKit.RoundRect(card, UiTheme.Dpi(10)))
            {
                using (SolidBrush b = new SolidBrush(UiKit.Surface)) g.FillPath(b, path);
                using (Pen p = new Pen(UiKit.Line, 1f)) g.DrawPath(p, path);
            }
            if (Width < 80 || Height < 80) return;

            int pad = UiTheme.Dpi(14);
            int titleH = UiTheme.Dpi(26);
            if (!string.IsNullOrEmpty(Title))
                TextRenderer.DrawText(g, Title, UiTheme.BodyBold,
                    new Rectangle(pad, pad - UiTheme.Dpi(2), Width - pad * 2, titleH),
                    UiKit.TextPrimary, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

            Rectangle plot = new Rectangle(pad, pad + titleH, Width - pad * 2, Height - pad * 2 - titleH);
            if (plot.Width < 40 || plot.Height < 30) return;

            if (_data.Count == 0)
            {
                TextRenderer.DrawText(g, EmptyText, UiTheme.Body, plot, UiKit.TextMuted,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                return;
            }

            int max = 0;
            for (int i = 0; i < _data.Count; i++) if (_data[i].Value > max) max = _data[i].Value;
            if (max <= 0) max = 1;

            if (Orientation == BarChartOrientation.Horizontal) DrawHorizontal(g, plot, max);
            else DrawVertical(g, plot, max);
        }

        /// <summary>横向：左侧项目名右对齐，条形带底槽、按最大值等比，数值固定在槽尾之后。</summary>
        private void DrawHorizontal(Graphics g, Rectangle plot, int max)
        {
            int n = _data.Count;
            int rowH = Math.Min(UiTheme.Dpi(MaxRowH), plot.Height / n);
            if (rowH < UiTheme.Dpi(10)) return;

            int total = rowH * n;
            int oy = plot.Top + Math.Max(0, (plot.Height - total) / 2);   // 条数少时整组垂直居中
            int labelW = Math.Min(UiTheme.Dpi(84), plot.Width / 3);
            int valueW = UiTheme.Dpi(46);
            int barMax = plot.Width - labelW - valueW - UiTheme.Dpi(16);
            if (barMax < 20) return;

            int barH = Math.Max(UiTheme.Dpi(8), Math.Min(UiTheme.Dpi(18), rowH - UiTheme.Dpi(6)));

            for (int i = 0; i < n; i++)
            {
                int ry = oy + rowH * i;
                int by = ry + (rowH - barH) / 2;
                KeyValuePair<string, int> it = _data[i];

                TextRenderer.DrawText(g, it.Key, Font,
                    new Rectangle(plot.Left, ry, labelW - UiTheme.Dpi(6), rowH),
                    UiKit.TextSecondary, TextFormatFlags.Right | TextFormatFlags.VerticalCenter
                        | TextFormatFlags.EndEllipsis);

                Rectangle track = new Rectangle(plot.Left + labelW, by, barMax, barH);
                using (GraphicsPath p = UiKit.RoundRect(track, barH / 2))
                    using (SolidBrush b = new SolidBrush(TrackColor)) g.FillPath(b, p);

                int w = (int)Math.Round(barMax * (double)it.Value / max);
                if (w < 1 && it.Value > 0) w = 1;
                if (w > 0)
                {
                    Rectangle bar = new Rectangle(plot.Left + labelW, by, w, barH);
                    using (GraphicsPath p = UiKit.RoundRect(bar, barH / 2))
                        using (SolidBrush b = new SolidBrush(BarColor)) g.FillPath(b, p);
                }

                TextRenderer.DrawText(g, it.Value.ToString(), Font,
                    new Rectangle(plot.Left + labelW + barMax + UiTheme.Dpi(6), ry, valueW, rowH),
                    UiKit.TextSecondary, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            }
        }

        /// <summary>纵向：等宽柱，标签按密度抽稀显示，数值只在条数不多时画在柱顶。</summary>
        private void DrawVertical(Graphics g, Rectangle plot, int max)
        {
            int n = _data.Count;
            int labelH = UiTheme.Dpi(20);
            int valueH = n <= 14 ? UiTheme.Dpi(16) : 0;
            int baseY = plot.Bottom - labelH;
            int barMaxH = baseY - plot.Top - valueH;
            if (barMaxH < 20) return;

            int slot = plot.Width / n;
            int barW = Math.Max(UiTheme.Dpi(3), (int)(slot * 0.6));
            int step = Math.Max(1, (int)Math.Ceiling(n / 12.0));   // 标签抽稀，避免糊成一片

            // 水平网格线：柱高没有参照物时读不出量级
            using (Pen gp = new Pen(UiKit.Line, 1f))
                for (int k = 1; k <= 3; k++)
                {
                    int gy = baseY - barMaxH * k / 4;
                    g.DrawLine(gp, plot.Left, gy, plot.Right, gy);
                }

            // 基准线
            using (Pen p = new Pen(UiKit.Line, 1f)) g.DrawLine(p, plot.Left, baseY, plot.Right, baseY);

            for (int i = 0; i < n; i++)
            {
                int cx = plot.Left + slot * i + slot / 2;
                int h = (int)Math.Round(barMaxH * (double)_data[i].Value / max);
                if (h < 1 && _data[i].Value > 0) h = 1;
                if (h > 0)
                {
                    Rectangle bar = new Rectangle(cx - barW / 2, baseY - h, barW, h);
                    using (GraphicsPath p = UiKit.RoundRect(bar, Math.Min(UiTheme.Dpi(3), barW / 2)))
                        using (SolidBrush b = new SolidBrush(BarColor)) g.FillPath(b, p);
                }

                if (valueH > 0 && _data[i].Value > 0)
                    TextRenderer.DrawText(g, _data[i].Value.ToString(), Font,
                        new Rectangle(cx - slot / 2, baseY - h - valueH, slot, valueH),
                        UiKit.TextSecondary, TextFormatFlags.HorizontalCenter | TextFormatFlags.Bottom);

                if (i % step == 0)
                    TextRenderer.DrawText(g, ShortLabel(_data[i].Key), Font,
                        new Rectangle(cx - slot, baseY + UiTheme.Dpi(2), slot * 2, labelH),
                        UiKit.TextSecondary, TextFormatFlags.HorizontalCenter | TextFormatFlags.Top);
            }
        }

        /// <summary>把 yyyy-MM-dd 缩成 MM-dd，横轴才放得下。</summary>
        private static string ShortLabel(string s)
        {
            return (s != null && s.Length == 10) ? s.Substring(5) : s;
        }
    }
}
