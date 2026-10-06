using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>按钮变体：决定底色与文字的语义。</summary>
    public enum FlatButtonVariant
    {
        /// <summary>主操作（品牌蓝实心）</summary>
        Primary,
        /// <summary>确认类（绿实心）</summary>
        Success,
        /// <summary>危险类（红实心，用于删除）</summary>
        Danger,
        /// <summary>次要操作（白底 + 浅描边）</summary>
        Surface,
        /// <summary>文字按钮（无底色，仅悬停时浮出浅底）</summary>
        Ghost
    }

    /// <summary>
    /// 自绘圆角按钮：图标 + 文字，含悬停/按下的状态反馈。
    ///
    /// 为什么不用原生 Button：
    ///   1. 圆角 Region 是硬裁（锯齿），而 Flat 样式的边框画在矩形边上、圆角处会被裁出缺口；
    ///   2. 原生按钮没法把图标和文字作为一个整体居中排布；
    ///   3. 与 AlarmChip / FieldBox 等自绘控件保持同一套视觉语言。
    ///
    /// 默认把 Selectable 关掉（不抢焦点），避免点完按钮留下焦点虚线框。
    /// </summary>
    public class FlatButton : Control
    {
        private bool _hover;
        private bool _pressed;
        private FlatButtonVariant _variant = FlatButtonVariant.Primary;
        private Image _icon;
        private bool _selectable;

        public FlatButtonVariant Variant
        {
            get { return _variant; }
            set { _variant = value; Invalidate(); }
        }

        public Image Icon
        {
            get { return _icon; }
            set { _icon = value; Invalidate(); }
        }

        public int CornerRadius { get; set; }
        public int IconSize { get; set; }
        public int IconGap { get; set; }

        /// <summary>是否允许获取焦点（默认 false，避免留下焦点虚线框）。</summary>
        public bool Selectable
        {
            get { return _selectable; }
            set
            {
                _selectable = value;
                SetStyle(ControlStyles.Selectable, value);
                TabStop = value;
            }
        }

        public FlatButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.Selectable, false);

            CornerRadius = 8;
            IconSize = 18;
            IconGap = 8;
            TabStop = false;
            BackColor = Color.Transparent;
            Font = UiKit.Bold(10.5f);
            Size = new Size(160, 44);
            Cursor = Cursors.Hand;
        }

        private Color FillColor()
        {
            if (!Enabled) return UiKit.DisabledBg;
            switch (_variant)
            {
                case FlatButtonVariant.Primary:
                    return _pressed ? UiKit.BrandDark : (_hover ? Color.FromArgb(16, 126, 202) : UiKit.Brand);
                case FlatButtonVariant.Success:
                    return _pressed ? Color.FromArgb(36, 124, 55) : (_hover ? Color.FromArgb(56, 168, 80) : UiKit.Success);
                case FlatButtonVariant.Danger:
                    return _pressed ? Color.FromArgb(176, 48, 48) : (_hover ? Color.FromArgb(222, 80, 80) : UiKit.Danger);
                case FlatButtonVariant.Surface:
                    return _pressed ? Color.FromArgb(230, 238, 246) : (_hover ? Color.FromArgb(245, 249, 253) : UiKit.Surface);
                default: // Ghost
                    return _pressed ? Color.FromArgb(210, 227, 244) : (_hover ? UiKit.BrandSoft : Color.Transparent);
            }
        }

        private Color BorderColor()
        {
            if (!Enabled) return UiKit.Line;
            if (_variant == FlatButtonVariant.Surface) return _hover ? UiKit.LineStrong : UiKit.Line;
            if (_variant == FlatButtonVariant.Ghost) return Color.Transparent;
            return Color.Transparent;
        }

        private Color TextColor()
        {
            if (!Enabled) return UiKit.DisabledText;
            switch (_variant)
            {
                case FlatButtonVariant.Primary:
                case FlatButtonVariant.Success:
                case FlatButtonVariant.Danger:
                    return Color.White;
                case FlatButtonVariant.Ghost:
                    return UiKit.Brand;
                default:
                    return UiKit.TextPrimary;
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            Invalidate();
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Cursor = Enabled ? Cursors.Hand : Cursors.Default;
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _hover = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = false;
            _pressed = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                _pressed = true;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_pressed) { _pressed = false; Invalidate(); }
        }

        /// <summary>
        /// 等价于 Button.PerformClick()。
        /// Control 本身没有这个方法（只有 ButtonBase 才有），所以自己补一个，
        /// 让"回车提交"这类逻辑可以直接照搬原写法。
        /// </summary>
        public void PerformClick()
        {
            if (Enabled) OnClick(EventArgs.Empty);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            g.Clear(UiKit.ResolveParentBack(this));

            Color fill = FillColor();
            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = UiKit.RoundRect(rect, CornerRadius))
            {
                if (fill.A > 0)
                {
                    using (SolidBrush b = new SolidBrush(fill)) g.FillPath(b, path);
                }
                Color border = BorderColor();
                if (border.A > 0)
                    using (Pen p = new Pen(border, 1f)) g.DrawPath(p, path);
            }

            Color textColor = TextColor();

            // 图标 + 文字作为一整组居中，避免只有文字居中、加了图标就偏
            int iconW = (_icon != null) ? IconSize : 0;
            int gap = (_icon != null) ? IconGap : 0;
            Size textSize = TextRenderer.MeasureText(Text, Font, new Size(int.MaxValue, int.MaxValue),
                TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
            int contentW = iconW + gap + textSize.Width;
            int x = Math.Max(6, (Width - contentW) / 2);

            if (_icon != null)
            {
                g.DrawImage(_icon, new Rectangle(x, (Height - IconSize) / 2, IconSize, IconSize));
                x += iconW + gap;
            }

            Rectangle textRect = new Rectangle(x, 0, Math.Max(1, Width - x - 4), Height);
            TextRenderer.DrawText(g, Text, Font, textRect, textColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
        }
    }
}
