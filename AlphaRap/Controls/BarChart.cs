using System;
using System.Collections.Generic;
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

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);

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

        /// <summary>横向：左侧项目名右对齐，条形按最大值等比，数值跟在条尾。</summary>
        private void DrawHorizontal(Graphics g, Rectangle plot, int max)
        {
            int n = _data.Count;
            int rowH = plot.Height / n;
            int labelW = Math.Min(UiTheme.Dpi(84), plot.Width / 3);
            int valueW = UiTheme.Dpi(46);
            int barMax = plot.Width - labelW - valueW - UiTheme.Dpi(16);
            if (barMax < 20) return;

            int barH = Math.Max(UiTheme.Dpi(8), Math.Min(UiTheme.Dpi(16), rowH - UiTheme.Dpi(6)));

            for (int i = 0; i < n; i++)
            {
                int y = plot.Top + rowH * i + (rowH - barH) / 2;
                KeyValuePair<string, int> it = _data[i];

                TextRenderer.DrawText(g, it.Key, Font,
                    new Rectangle(plot.Left, plot.Top + rowH * i, labelW - UiTheme.Dpi(6), rowH),
                    UiKit.TextSecondary, TextFormatFlags.Right | TextFormatFlags.VerticalCenter
                        | TextFormatFlags.EndEllipsis);

                int w = (int)Math.Round(barMax * (double)it.Value / max);
                if (w < 1 && it.Value > 0) w = 1;
                Rectangle bar = new Rectangle(plot.Left + labelW, y, w, barH);
                using (GraphicsPath p = UiKit.RoundRect(bar, barH / 2))
                    using (SolidBrush b = new SolidBrush(BarColor)) g.FillPath(b, p);

                TextRenderer.DrawText(g, it.Value.ToString(), Font,
                    new Rectangle(bar.Right + UiTheme.Dpi(6), plot.Top + rowH * i, valueW, rowH),
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
