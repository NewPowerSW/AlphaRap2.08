using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>一张统计卡的内容：标题 + 数值 + 左侧竖条颜色。</summary>
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
    /// 统计数字条：一行等宽白色圆角卡片，每张显示「标题 + 大号数值 + 左侧色条」。
    /// 全部自绘，卡片数量由 SetItems 决定（不动态创建控件，避免与设计器/Dpi 缩放互相干扰）。
    /// </summary>
    public class StatStrip : Control
    {
        private readonly List<StatItem> _items = new List<StatItem>();

        /// <summary>卡片间距（96 DPI 设计值）。</summary>
        public int Gap { get; set; }

        public StatStrip()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = UiKit.PageBg;
            Gap = 12;
        }

        /// <summary>设置要显示的卡片（会重绘）。</summary>
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
            if (_items.Count == 0) return;

            int gap = UiTheme.Dpi(Gap);
            int cw = (Width - gap * (_items.Count - 1)) / _items.Count;
            int h = Height;

            for (int i = 0; i < _items.Count; i++)
            {
                StatItem it = _items[i];
                Rectangle rect = new Rectangle(i * (cw + gap), 0, cw, h - 1);

                using (GraphicsPath path = UiKit.RoundRect(rect, UiTheme.Dpi(10)))
                {
                    using (SolidBrush b = new SolidBrush(UiKit.Surface)) g.FillPath(b, path);
                    using (Pen p = new Pen(UiKit.Line, 1f)) g.DrawPath(p, path);
                }

                // 左侧色条：与圆角同高、贴在卡片左内侧
                Rectangle bar = new Rectangle(rect.Left + UiTheme.Dpi(1), rect.Top + UiTheme.Dpi(12),
                                              UiTheme.Dpi(4), rect.Height - UiTheme.Dpi(24));
                using (SolidBrush b = new SolidBrush(it.Accent)) g.FillRectangle(b, bar);

                int textLeft = bar.Right + UiTheme.Dpi(12);
                int textW = rect.Right - textLeft - UiTheme.Dpi(10);
                if (textW <= 0) continue;

                TextRenderer.DrawText(g, it.Title, UiTheme.Body,
                    new Rectangle(textLeft, rect.Top + UiTheme.Dpi(10), textW, UiTheme.Dpi(20)),
                    UiKit.TextSecondary, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

                TextRenderer.DrawText(g, it.Value, UiTheme.StatValue,
                    new Rectangle(textLeft, rect.Top + UiTheme.Dpi(30), textW, rect.Height - UiTheme.Dpi(38)),
                    UiKit.TextPrimary, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            }
        }
    }
}
