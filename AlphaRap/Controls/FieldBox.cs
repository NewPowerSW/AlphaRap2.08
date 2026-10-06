using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>
    /// 圆角输入框：外层是自绘的圆角容器，内部放一个无边框 TextBox。
    ///
    /// 为什么不用原生 TextBox：
    ///   1. WinForms 的 TextBox 只有 3D / 单线两种边框，做不出圆角，也没有"聚焦时描边变色"；
    ///   2. 原生输入框左侧放不了图标，而带图标的输入框是这套界面的基本元素；
    ///   3. 与其它自绘控件（AlarmChip / FlatButton / CardPanel）共用同一套圆角与描边规则。
    ///
    /// 内容通过 <see cref="Value"/> 读写，**故意不复用控件的 Text**：
    /// 工程的自动语言扫描会遍历控件树并改写 Button/Label/TabPage 的 Text，
    /// 复用 Text 会让输入内容被语言切换覆盖。Value 不参与那套机制，天然安全。
    /// </summary>
    public class FieldBox : Panel
    {
        private readonly TextBox _inner = new TextBox();
        private bool _focused;
        private Image _icon;

        /// <summary>内容（真正的输入值）。</summary>
        public string Value
        {
            get { return _inner.Text; }
            set { _inner.Text = value ?? string.Empty; }
        }

        /// <summary>左侧前置图标（按 20px 绘制，传 20~36px 的图都清晰）。</summary>
        public Image Icon
        {
            get { return _icon; }
            set { _icon = value; LayoutInner(); Invalidate(); }
        }

        /// <summary>密码掩码字符（0 表示不掩码）。</summary>
        public char PasswordChar
        {
            get { return _inner.PasswordChar; }
            set { _inner.PasswordChar = value; }
        }

        /// <summary>只读内容（用于"选中的用户名"这类展示位）。</summary>
        public bool ContentReadOnly
        {
            get { return _inner.ReadOnly; }
            set { _inner.ReadOnly = value; Invalidate(); }
        }

        public int MaxLength
        {
            get { return _inner.MaxLength; }
            set { _inner.MaxLength = value; }
        }

        /// <summary>内层 TextBox，供窗体挂键盘事件（例如回车提交）。</summary>
        public TextBox Inner
        {
            get { return _inner; }
        }

        /// <summary>
        /// 把焦点交给内层输入框。
        /// 不能用继承来的 Focus()：Panel 默认不可获焦，Focus() 会直接失败。
        /// </summary>
        public void FocusInput()
        {
            try { if (_inner.CanFocus) _inner.Focus(); }
            catch { }
        }

        /// <summary>内层输入框是否持有焦点。</summary>
        public bool InputFocused
        {
            get { return _inner.Focused; }
        }

        /// <summary>内容变化（等价于 TextBox.TextChanged，但不与语言机制冲突）。</summary>
        public event EventHandler ValueChanged;

        public FieldBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            _inner.BorderStyle = BorderStyle.None;
            _inner.BackColor = UiKit.FieldBg;
            _inner.ForeColor = UiKit.TextPrimary;
            _inner.Font = UiKit.Regular(11f);
            _inner.TextChanged += delegate
            {
                if (ValueChanged != null) ValueChanged(this, EventArgs.Empty);
            };
            _inner.FontChanged += delegate { LayoutInner(); };
            _inner.GotFocus += delegate { _focused = true; Invalidate(); };
            _inner.LostFocus += delegate { _focused = false; Invalidate(); };

            Controls.Add(_inner);
            Size = new Size(260, 46);
            BackColor = UiKit.FieldBg;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (Enabled) _inner.Focus();
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            _inner.Enabled = Enabled;
            Invalidate();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            LayoutInner();
        }

        /// <summary>内层输入框：左侧让出图标位，垂直居中，右侧留内边距。</summary>
        private void LayoutInner()
        {
            try
            {
                int left = (_icon != null) ? 46 : 14;
                int h = _inner.PreferredHeight;
                if (h < 18) h = 18;
                int w = Width - left - 14;
                if (w < 10) w = 10;
                _inner.SetBounds(left, Math.Max(0, (Height - h) / 2), w, h);
            }
            catch { }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 先用父容器底色铺满，圆角外的部分就等于被"擦掉"了
            // （父容器可能是 BackColor=Transparent 的表格，所以要往上层找不透明色）
            g.Clear(UiKit.ResolveParentBack(this));

            Color fill = !Enabled
                ? UiKit.DisabledBg
                : (ContentReadOnly ? Color.FromArgb(246, 249, 252) : UiKit.FieldBg);
            Color line = !Enabled ? UiKit.Line : (_focused ? UiKit.Brand : UiKit.LineStrong);

            _inner.BackColor = fill;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = UiKit.RoundRect(rect, 8))
            {
                using (SolidBrush b = new SolidBrush(fill)) g.FillPath(b, path);
                using (Pen p = new Pen(line, _focused ? 1.6f : 1f)) g.DrawPath(p, path);
            }

            if (_icon != null)
            {
                const int ih = 20;
                g.DrawImage(_icon, new Rectangle(15, (Height - ih) / 2, ih, ih));
                // 图标与输入区之间一道浅竖线，把"前缀"和"内容"分开
                using (Pen p = new Pen(UiKit.Line, 1f))
                    g.DrawLine(p, 41f, 11f, 41f, Height - 11f);
            }
        }
    }
}
