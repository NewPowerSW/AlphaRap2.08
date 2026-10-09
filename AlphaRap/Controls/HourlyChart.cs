using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>
    /// 每小时产量柱状图（0–23 点）：良品与不良堆叠显示，当前小时加浅色底标出。
    /// </summary>
    public class HourlyChart : Control
    {
        private readonly double[] _ok = new double[24];
        private readonly double[] _ng = new double[24];
        private int _currentHour = -1;

        public Color OkColor { get; set; } = UiKit.Brand;
        public Color NgColor { get; set; } = UiKit.Danger;
        public string OkLegend { get; set; } = "良品";
        public string NgLegend { get; set; } = "不良";
        public string EmptyText { get; set; } = "暂无数据";

        public HourlyChart()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.Selectable, false);
            BackColor = UiKit.Surface;
            Font = UiTheme.Caption;
        }

        /// <summary>更新数据（数组长度为 24，下标为小时）；数据未变化时不重绘。</summary>
        public void SetData(double[] ok, double[] ng, int currentHour)
        {
            bool changed = currentHour != _currentHour;
            for (int h = 0; h < 24; h++)
            {
                double o = (ok != null && h < ok.Length) ? ok[h] : 0;
                double n = (ng != null && h < ng.Length) ? ng[h] : 0;
                if (o != _ok[h] || n != _ng[h]) changed = true;
                _ok[h] = o;
                _ng[h] = n;
            }
            _currentHour = currentHour;
            if (changed) Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            const int legendH = 26;
            const int axisW = 44;
            const int hourH = 22;
            Rectangle plot = new Rectangle(axisW, legendH + 6, Width - axisW - 6, Height - legendH - hourH - 10);
            if (plot.Width < 60 || plot.Height < 40) return;

            DrawLegend(g, new Rectangle(axisW, 0, Width - axisW, legendH));

            double max = 0;
            for (int h = 0; h < 24; h++) max = Math.Max(max, _ok[h] + _ng[h]);
            double step;
            double top = NiceCeiling(max, out step);
            int ticks = (int)Math.Round(top / step);

            // 横向网格与刻度
            using (Pen grid = new Pen(UiKit.Line) { DashStyle = DashStyle.Dot })
            {
                for (int i = 0; i <= ticks; i++)
                {
                    int y = plot.Bottom - (int)Math.Round(plot.Height * i / (double)ticks);
                    g.DrawLine(grid, plot.Left, y, plot.Right, y);
                    string label = (step * i).ToString("0.#");
                    TextRenderer.DrawText(g, label, Font, new Rectangle(0, y - 9, axisW - 8, 18), UiKit.TextMuted,
                        TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
                }
            }

            float slot = plot.Width / 24f;
            float barW = Math.Max(3f, slot * 0.58f);
            for (int h = 0; h < 24; h++)
            {
                float x = plot.Left + slot * h;
                if (h == _currentHour)
                    using (SolidBrush b = new SolidBrush(UiTheme.Selected))
                        g.FillRectangle(b, x, plot.Top, slot, plot.Height);

                float bx = x + (slot - barW) / 2f;
                float okH = (float)(plot.Height * _ok[h] / top);
                float ngH = (float)(plot.Height * _ng[h] / top);
                if (okH > 0) using (SolidBrush b = new SolidBrush(OkColor)) g.FillRectangle(b, bx, plot.Bottom - okH, barW, okH);
                if (ngH > 0) using (SolidBrush b = new SolidBrush(NgColor)) g.FillRectangle(b, bx, plot.Bottom - okH - ngH, barW, ngH);

                if (h % 2 == 0)
                    TextRenderer.DrawText(g, h.ToString(), Font, new Rectangle((int)x - 6, plot.Bottom + 4, (int)slot + 12, hourH),
                        h == _currentHour ? UiKit.Brand : UiKit.TextMuted, TextFormatFlags.HorizontalCenter | TextFormatFlags.Top);
            }

            if (max <= 0)
                TextRenderer.DrawText(g, EmptyText, UiTheme.Body, plot, UiKit.TextMuted,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private void DrawLegend(Graphics g, Rectangle r)
        {
            int x = r.Left;
            foreach (var item in new[] { Tuple.Create(OkLegend, OkColor), Tuple.Create(NgLegend, NgColor) })
            {
                using (SolidBrush b = new SolidBrush(item.Item2)) g.FillRectangle(b, x, r.Top + 8, 10, 10);
                Size sz = TextRenderer.MeasureText(item.Item1, Font);
                TextRenderer.DrawText(g, item.Item1, Font, new Point(x + 14, r.Top + 5), UiKit.TextMuted);
                x += 14 + sz.Width + 16;
            }
        }

        /// <summary>纵轴上限：刻度步长取 1 / 2 / 2.5 / 5 × 10^n，且不超过 5 格。</summary>
        private static double NiceCeiling(double v, out double step)
        {
            step = 1;
            if (v <= 4) return 4;
            double rough = v / 5;
            double exp = Math.Pow(10, Math.Floor(Math.Log10(rough)));
            step = 10 * exp;
            foreach (double m in new[] { 1.0, 2.0, 2.5, 5.0, 10.0 })
                if (m * exp >= rough) { step = m * exp; break; }
            step = Math.Max(1, step);
            return Math.Ceiling(v / step) * step;
        }
    }
}
