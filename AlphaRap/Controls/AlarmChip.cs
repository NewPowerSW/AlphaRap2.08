using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>标签样式：Toggle = 可点击的筛选标签（未选中描边 / 选中实心）；Info = 只读信息胶囊（浅灰填充、无描边）。</summary>
    public enum AlarmChipStyle
    {
        Toggle,
        Info
    }

    /// <summary>
    /// 报警筛选条用的标签（chip）：左侧图标 + 文字，整体自绘。
    ///
    /// 为什么不用 Button + Region：
    ///   圆角 Region 是硬裁（有锯齿），而 Flat 样式的边框又画在矩形边上，
    ///   圆角处会被裁掉出现缺口。只有自绘才能同时得到干净的圆角与抗锯齿描边。
    ///
    /// 继承 Control 但关掉 Selectable —— 点击后不会抢焦点，也就不会留下焦点虚线框
    /// （与 NoFocusChart 同一处理思路）。
    /// </summary>
    public class AlarmChip : Control
    {
        private bool _selected;
        private bool _hover;
        private Image _icon;
        private Image _iconSelected;
        private AlarmChipStyle _style = AlarmChipStyle.Toggle;

        /// <summary>主题强调色：未选中时用于图标与悬停描边，选中时用于填充。</summary>
        public Color AccentColor { get; set; }

        /// <summary>未选中时的描边色。</summary>
        public Color BorderColor { get; set; }

        /// <summary>未选中时的文字色。</summary>
        public Color TextColor { get; set; }

        /// <summary>Info 样式下的填充色与文字色。</summary>
        public Color InfoBackColor { get; set; }
        public Color InfoTextColor { get; set; }

        /// <summary>圆角半径。</summary>
        public int CornerRadius { get; set; }

        public AlarmChipStyle Style
        {
            get { return _style; }
            set
            {
                _style = value;
                Cursor = (value == AlarmChipStyle.Toggle) ? Cursors.Hand : Cursors.Default;
                Invalidate();
            }
        }

        /// <summary>未选中时显示的图标（一般是强调色版本）。</summary>
        public Image Icon
        {
            get { return _icon; }
            set { _icon = value; FitToContent(); Invalidate(); }
        }

        /// <summary>选中时显示的图标（一般是白色版本，否则实心底色上图标会"消失"）。</summary>
        public Image IconSelected
        {
            get { return _iconSelected; }
            set { _iconSelected = value; FitToContent(); Invalidate(); }
        }

        public bool Selected
        {
            get { return _selected; }
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    Invalidate();
                }
            }
        }

        public AlarmChip()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.Selectable, false);   // 不抢焦点 → 不留焦点虚线框
            TabStop = false;
            BackColor = Color.Transparent;

            AccentColor = Color.FromArgb(4, 108, 182);
            BorderColor = Color.FromArgb(44, 62, 80);
            TextColor = Color.FromArgb(44, 62, 80);
            InfoBackColor = Color.FromArgb(238, 242, 246);
            InfoTextColor = Color.FromArgb(60, 72, 88);
            CornerRadius = 6;

            Cursor = Cursors.Hand;
            Font = new Font("微软雅黑", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(134)));
            Size = new Size(64, 22);
            Margin = new Padding(0, 0, 6, 0);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            FitToContent();     // 切换语言后按钮文案会变，宽度要跟着自适应
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (_style == AlarmChipStyle.Toggle)
            {
                _hover = true;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_hover)
            {
                _hover = false;
                Invalidate();
            }
        }

        /// <summary>按"图标 + 文字"的实际宽度自适应控件宽度。</summary>
        public void FitToContent()
        {
            try
            {
                int content = 0;
                if (_icon != null) content += IconWidth() + 4;
                if (!string.IsNullOrEmpty(Text))
                    content += TextRenderer.MeasureText(Text, Font,
                        new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding).Width;

                int w = 7 + content + 7 + 2;
                if (w < 40) w = 40;
                if (Width != w) Width = w;
            }
            catch { }
        }

        private int IconWidth()
        {
            if (_icon == null) return 0;
            int h = Math.Max(10, Height - 8);
            return Math.Max(10, (int)Math.Round(_icon.Width * (h / (double)_icon.Height)));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = RoundRect(rect, CornerRadius))
            {
                if (_style == AlarmChipStyle.Info)
                {
                    using (SolidBrush b = new SolidBrush(InfoBackColor)) g.FillPath(b, path);
                }
                else if (_selected)
                {
                    using (SolidBrush b = new SolidBrush(AccentColor)) g.FillPath(b, path);
                }
                else
                {
                    using (SolidBrush b = new SolidBrush(_hover ? Color.FromArgb(245, 249, 252) : Color.White))
                        g.FillPath(b, path);
                    using (Pen p = new Pen(_hover ? AccentColor : BorderColor, 1.4f))
                        g.DrawPath(p, path);
                }
            }

            int x = 7;
            Image ic = (_style == AlarmChipStyle.Toggle && _selected) ? (_iconSelected ?? _icon) : _icon;
            if (ic != null)
            {
                int iw = IconWidth();
                int ih = Math.Max(10, Height - 8);
                g.DrawImage(ic, new Rectangle(x, (Height - ih) / 2, iw, ih));
                x += iw + 4;
            }

            Color tc = (_style == AlarmChipStyle.Info) ? InfoTextColor : (_selected ? Color.White : TextColor);
            Rectangle textRect = new Rectangle(x, 0, Math.Max(1, Width - x - 5), Height);
            TextRenderer.DrawText(g, Text, Font, textRect, tc,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
        }

        private static GraphicsPath RoundRect(Rectangle r, int radius)
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

        // ================= 图标生成（运行期用 GDI+ 画，不引入图片资源） =================

        /// <summary>警示三角（感叹号），用于"警告"与统计胶囊。</summary>
        public static Image CreateAlertTriangle(int size, Color color)
        {
            return CreateAlertTriangle(size, color, Color.White);
        }

        /// <summary>指定三角形填充色与内部感叹号颜色（选中态用"白三角 + 主题色叹号"）。</summary>
        public static Image CreateAlertTriangle(int size, Color fill, Color glyph)
        {
            try
            {
                Bitmap bmp = new Bitmap(size, size);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.Clear(Color.Transparent);

                    float top = 1.2f, bottom = size - 1.6f, left = 1.2f, right = size - 1.2f;
                    using (GraphicsPath path = new GraphicsPath())
                    {
                        path.AddPolygon(new[]
                        {
                            new PointF((left + right) / 2f, top),
                            new PointF(right, bottom),
                            new PointF(left, bottom)
                        });
                        path.CloseFigure();
                        using (SolidBrush b = new SolidBrush(fill)) g.FillPath(b, path);
                    }

                    using (Pen p = new Pen(glyph, Math.Max(1.3f, size / 9f)))
                    {
                        p.StartCap = LineCap.Round;
                        p.EndCap = LineCap.Round;
                        g.DrawLine(p, size / 2f, size * 0.40f, size / 2f, size * 0.66f);
                    }
                    using (SolidBrush b = new SolidBrush(glyph))
                        g.FillEllipse(b, size / 2f - size * 0.075f, size * 0.735f, size * 0.15f, size * 0.15f);
                }
                return bmp;
            }
            catch { return null; }
        }

        /// <summary>圆形叉，用于"报警 / 错误"。</summary>
        public static Image CreateErrorCircle(int size, Color color)
        {
            return CreateErrorCircle(size, color, Color.White);
        }

        /// <summary>指定圆盘填充色与叉的颜色（选中态用"白圆盘 + 主题色叉"）。</summary>
        public static Image CreateErrorCircle(int size, Color fill, Color glyph)
        {
            try
            {
                Bitmap bmp = new Bitmap(size, size);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.Clear(Color.Transparent);

                    using (SolidBrush b = new SolidBrush(fill))
                        g.FillEllipse(b, 0.5f, 0.5f, size - 1f, size - 1f);

                    using (Pen p = new Pen(glyph, Math.Max(1.4f, size / 7f)))
                    {
                        p.StartCap = LineCap.Round;
                        p.EndCap = LineCap.Round;
                        g.DrawLine(p, size * 0.33f, size * 0.33f, size * 0.67f, size * 0.67f);
                        g.DrawLine(p, size * 0.67f, size * 0.33f, size * 0.33f, size * 0.67f);
                    }
                }
                return bmp;
            }
            catch { return null; }
        }

        /// <summary>漏斗，用于"全部"（筛选的通用语义）。</summary>
        public static Image CreateFunnelIcon(int size, Color color)
        {
            try
            {
                Bitmap bmp = new Bitmap(size, size);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.Clear(Color.Transparent);

                    float l = size * 0.14f, r = size * 0.86f, t = size * 0.18f, b = size * 0.84f;
                    using (GraphicsPath path = new GraphicsPath())
                    {
                        path.AddPolygon(new[]
                        {
                            new PointF(l, t),
                            new PointF(r, t),
                            new PointF(size * 0.585f, size * 0.53f),
                            new PointF(size * 0.585f, b),
                            new PointF(size * 0.415f, size * 0.72f),
                            new PointF(size * 0.415f, size * 0.53f)
                        });
                        path.CloseFigure();
                        using (SolidBrush br = new SolidBrush(color)) g.FillPath(br, path);
                    }
                }
                return bmp;
            }
            catch { return null; }
        }
    }
}
