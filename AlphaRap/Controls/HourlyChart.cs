using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>当天 0–23 点的产量趋势：良品面积折线、不良折线及当前小时标记。</summary>
    public class HourlyChart : Control
    {
        private readonly double[] _ok = new double[24];
        private readonly double[] _ng = new double[24];
        private int _currentHour = -1;

        public Color OkColor { get; set; } = Color.FromArgb(18, 185, 125);
        public Color NgColor { get; set; } = Color.FromArgb(239, 68, 78);
        public string OkLegend { get; set; } = "良品";
        public string NgLegend { get; set; } = "不良";
        public string EmptyText { get; set; } = "暂无数据";

        /// <summary>创建双缓冲趋势图；数据和语言由首页传入，不启动计时器或读取配置。</summary>
        public HourlyChart()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.Selectable, false);
            BackColor = UiKit.Surface;
            Font = UiTheme.Body;
        }

        /// <summary>复制按小时排列的产量；缺项或无效数量按零显示，当前小时之外的未来时段不绘制数据。</summary>
        public void SetData(double[] ok, double[] ng, int currentHour)
        {
            int hour = currentHour >= 0 && currentHour < 24 ? currentHour : -1;
            bool changed = hour != _currentHour;
            for (int h = 0; h < 24; h++)
            {
                double good = ReadQuantity(ok, h);
                double reject = ReadQuantity(ng, h);
                if (good != _ok[h] || reject != _ng[h]) changed = true;
                _ok[h] = good;
                _ng[h] = reject;
            }
            _currentHour = hour;
            if (changed) Invalidate();
        }

        /// <summary>读取一小时的非负有限数量，避免无效生产记录影响坐标计算。</summary>
        private static double ReadQuantity(double[] values, int hour)
        {
            if (values == null || hour >= values.Length) return 0;
            double value = values[hour];
            return value >= 0 && !double.IsInfinity(value) ? value : 0;
        }

        /// <summary>按当前尺寸绘制网格、图例和真实产量；没有产量时保留坐标并显示空状态。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int axisWidth = UiTheme.Dpi(52);
            int legendHeight = Math.Max(UiTheme.Dpi(28), Font.Height + UiTheme.Dpi(8));
            int hourHeight = Math.Max(UiTheme.Dpi(26), Font.Height + UiTheme.Dpi(8));
            Rectangle plot = new Rectangle(axisWidth, legendHeight + UiTheme.Dpi(10),
                Width - axisWidth - UiTheme.Dpi(16), Height - legendHeight - hourHeight - UiTheme.Dpi(14));
            if (plot.Width < UiTheme.Dpi(60) || plot.Height < UiTheme.Dpi(40)) return;

            int lastHour = _currentHour >= 0 ? _currentHour : 23;
            double maximum = 0;
            for (int hour = 0; hour <= lastHour; hour++)
                maximum = Math.Max(maximum, Math.Max(_ok[hour], _ng[hour]));
            double step;
            double top = NiceCeiling(maximum, out step);
            DrawLegend(g, new Rectangle(plot.Left, 0, plot.Width, legendHeight));
            DrawAxes(g, plot, top, step, hourHeight);

            if (maximum <= 0)
            {
                TextRenderer.DrawText(g, EmptyText, Font, plot, UiKit.TextMuted,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                base.OnPaint(e);
                return;
            }

            DrawSeries(g, plot, _ok, lastHour, top, OkColor, true);
            DrawSeries(g, plot, _ng, lastHour, top, NgColor, false);
            if (_currentHour >= 0)
            {
                DrawCurrentPoint(g, plot, _ok[_currentHour], top, OkColor);
                if (_ng[_currentHour] > 0) DrawCurrentPoint(g, plot, _ng[_currentHour], top, NgColor);
            }
            base.OnPaint(e);
        }

        /// <summary>绘制水平刻度和小时标签，窄图自动降低小时标签密度。</summary>
        private void DrawAxes(Graphics g, Rectangle plot, double top, double step, int hourHeight)
        {
            int labelHeight = Font.Height + UiTheme.Dpi(4);
            int ticks = Math.Min(Math.Max(1, (int)Math.Round(top / step)),
                Math.Max(1, plot.Height / (labelHeight + UiTheme.Dpi(6))));
            using (Pen grid = new Pen(UiKit.Line))
            {
                for (int index = 0; index <= ticks; index++)
                {
                    int y = plot.Bottom - (int)Math.Round(plot.Height * index / (double)ticks);
                    g.DrawLine(grid, plot.Left, y, plot.Right, y);
                    double value = top * (index / (double)ticks);
                    string label = value.ToString(value >= 1000000000 ? "0.#E+0" : "0.#");
                    TextRenderer.DrawText(g, label, Font, new Rectangle(0, y - labelHeight / 2,
                        plot.Left - UiTheme.Dpi(8), labelHeight), UiKit.TextMuted,
                        TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                }
            }
            int interval = plot.Width < UiTheme.Dpi(460) ? 4 : 2;
            int labelWidth = UiTheme.Dpi(32);
            for (int hour = 0; hour < 24; hour += interval)
            {
                int x = plot.Left + (int)Math.Round(plot.Width * hour / 23.0);
                TextRenderer.DrawText(g, hour.ToString(), Font,
                    new Rectangle(x - labelWidth / 2, plot.Bottom + UiTheme.Dpi(6), labelWidth, hourHeight),
                    hour == _currentHour ? OkColor : UiKit.TextMuted,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.NoPadding);
            }
        }

        /// <summary>用直线连接真实小时数据；良品增加向下渐淡的面积，不平滑虚构中间峰值。</summary>
        private void DrawSeries(Graphics g, Rectangle plot, double[] values, int lastHour, double top, Color color, bool fill)
        {
            if (lastHour < 1) return;
            using (GraphicsPath line = new GraphicsPath())
            {
                PointF previous = SeriesPoint(plot, 0, values[0], top);
                for (int hour = 1; hour <= lastHour; hour++)
                {
                    PointF next = SeriesPoint(plot, hour, values[hour], top);
                    line.AddLine(previous, next);
                    previous = next;
                }
                if (fill)
                {
                    using (GraphicsPath area = (GraphicsPath)line.Clone())
                    using (LinearGradientBrush brush = new LinearGradientBrush(plot,
                        Color.FromArgb(70, color), Color.FromArgb(6, color), LinearGradientMode.Vertical))
                    {
                        area.AddLine(previous, new PointF(previous.X, plot.Bottom));
                        area.AddLine(previous.X, plot.Bottom, plot.Left, plot.Bottom);
                        area.CloseFigure();
                        g.FillPath(brush, area);
                    }
                }
                using (Pen pen = new Pen(color, Math.Max(1f, UiTheme.Scale * (fill ? 2f : 1.5f))))
                {
                    pen.LineJoin = LineJoin.Round;
                    g.DrawPath(pen, line);
                }
            }
        }

        /// <summary>将小时及数量换算到绘图区，先归一化避免大数乘法溢出。</summary>
        private static PointF SeriesPoint(Rectangle plot, int hour, double value, double top)
        {
            return new PointF(plot.Left + plot.Width * hour / 23f, plot.Bottom - (float)(value / top * plot.Height));
        }

        /// <summary>用空心圆标出当前小时；当前小时为零点时也能显示单个真实数据点。</summary>
        private void DrawCurrentPoint(Graphics g, Rectangle plot, double value, double top, Color color)
        {
            PointF point = SeriesPoint(plot, _currentHour, value, top);
            float radius = Math.Max(3f, UiTheme.Scale * 3.5f);
            RectangleF marker = new RectangleF(point.X - radius, point.Y - radius, radius * 2, radius * 2);
            using (SolidBrush background = new SolidBrush(BackColor)) g.FillEllipse(background, marker);
            using (Pen outline = new Pen(color, Math.Max(1f, UiTheme.Scale * 2f))) g.DrawEllipse(outline, marker);
        }

        /// <summary>右对齐良品与不良图例，语言文字过长时在可用宽度内截断。</summary>
        private void DrawLegend(Graphics g, Rectangle bounds)
        {
            int marker = UiTheme.Dpi(9), gap = UiTheme.Dpi(8), space = UiTheme.Dpi(20);
            int available = Math.Max(0, (bounds.Width - space) / 2 - marker - gap);
            int okWidth = Math.Min(available, TextRenderer.MeasureText(OkLegend, Font).Width);
            int ngWidth = Math.Min(available, TextRenderer.MeasureText(NgLegend, Font).Width);
            int x = bounds.Right - okWidth - ngWidth - 2 * (marker + gap) - space;
            DrawLegendItem(g, new Rectangle(x, bounds.Top, marker + gap + okWidth, bounds.Height), OkLegend, OkColor, marker, gap);
            x += marker + gap + okWidth + space;
            DrawLegendItem(g, new Rectangle(x, bounds.Top, marker + gap + ngWidth, bounds.Height), NgLegend, NgColor, marker, gap);
        }

        /// <summary>绘制一个图例色块和与宿主语言同步的文字。</summary>
        private void DrawLegendItem(Graphics g, Rectangle bounds, string text, Color color, int marker, int gap)
        {
            using (SolidBrush brush = new SolidBrush(color))
                g.FillRectangle(brush, bounds.Left, bounds.Top + (bounds.Height - marker) / 2, marker, marker);
            TextRenderer.DrawText(g, text, Font, new Rectangle(bounds.Left + marker + gap, bounds.Top,
                Math.Max(0, bounds.Width - marker - gap), bounds.Height), UiKit.TextSecondary,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
        }

        /// <summary>纵轴刻度使用易读的 1、2、2.5、5 倍数，空图至少显示四格。</summary>
        private static double NiceCeiling(double value, out double step)
        {
            step = 1;
            if (value <= 4) return 4;
            double rough = value / 5;
            double magnitude = Math.Pow(10, Math.Floor(Math.Log10(rough)));
            double normalized = rough / magnitude;
            double multiple = normalized <= 1 ? 1 : normalized <= 2 ? 2 : normalized <= 2.5 ? 2.5 : normalized <= 5 ? 5 : 10;
            step = Math.Max(1, multiple * magnitude);
            double top = Math.Ceiling(value / step) * step;
            if (!double.IsInfinity(top)) return top;
            step = value / 5;
            return value;
        }
    }
}
