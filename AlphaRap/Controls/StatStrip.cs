using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>一张统计格的内容：标题 + 数值 + 数值颜色。</summary>
    public sealed class StatItem
    {
        public string Title;
        public string Value;
        public Color Accent;

        public StatItem(string title, string value, Color accent)
        {
            Title = title;
            Value = value;
            Accent = accent;
        }
    }

    /// <summary>
    /// 统计数字条：一张白色圆角卡片内等分若干格，格与格之间一条细分隔线，
    /// 每格「灰色小标题 + 彩色大数字」。全部自绘，不创建子控件（避免与设计器和 Dpi 缩放互相干扰）。
    /// </summary>
    public class StatStrip : Control
    {
        private readonly List<StatItem> _items = new List<StatItem>();

        public StatStrip()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = UiKit.PageBg;
        }

        /// <summary>设置要显示的统计格（会重绘）。</summary>
        public void SetItems(IList<StatItem> items)
        {
            _items.Clear();
            if (items != null) for (int i = 0; i < items.Count; i++) _items.Add(items[i]);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);

            using (GraphicsPath path = UiKit.RoundRect(new Rectangle(0, 0, Width - 1, Height - 1), UiTheme.Dpi(10)))
            {
                using (SolidBrush b = new SolidBrush(UiKit.Surface)) g.FillPath(b, path);
                using (Pen p = new Pen(UiKit.Line, 1f)) g.DrawPath(p, path);
            }

            if (_items.Count == 0 || Width < 120 || Height < 44) return;

            int padX = UiTheme.Dpi(20);
            int cellW = (Width - padX * 2) / _items.Count;
            if (cellW < UiTheme.Dpi(60)) return;

            for (int i = 0; i < _items.Count; i++)
            {
                StatItem it = _items[i];
                int x = padX + cellW * i;

                if (i > 0)
                {
                    int xd = x - UiTheme.Dpi(10);
                    using (Pen p = new Pen(UiKit.Line, 1f))
                        g.DrawLine(p, xd, UiTheme.Dpi(18), xd, Height - UiTheme.Dpi(19));
                }

                TextRenderer.DrawText(g, it.Title, UiTheme.Body,
                    new Rectangle(x, UiTheme.Dpi(13), cellW - UiTheme.Dpi(16), UiTheme.Dpi(20)),
                    UiKit.TextSecondary, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

                TextRenderer.DrawText(g, it.Value, UiTheme.KpiValue,
                    new Rectangle(x, UiTheme.Dpi(33), cellW - UiTheme.Dpi(16), Height - UiTheme.Dpi(44)),
                    it.Accent, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            }
        }
    }
}
