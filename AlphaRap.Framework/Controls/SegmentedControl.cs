using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>
    /// 分段选择器：一条浅色轨道内等分若干选项，选中项浮出一张白底圆角卡片（品牌蓝加粗文字）。
    /// 整块自绘、不创建子控件；选项文字由 <see cref="SetItems"/> 按当前语言灌入（只换文字不改宽度，切语言不跳版）。
    /// </summary>
    public class SegmentedControl : Control
    {
        private readonly List<string> _items = new List<string>();

        private int _selectedIndex = -1;
        private int _hoverIndex = -1;

        /// <summary>选中项改变（用户点击与编程赋值都会触发）。</summary>
        public event EventHandler SelectedIndexChanged;

        public SegmentedControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Selectable, false);      // 点击后不留焦点虚线框

            BackColor = Color.FromArgb(240, 245, 250);
            Font = UiTheme.Body;
            TabStop = false;
        }

        /// <summary>选中项下标；-1 = 未选中。越界赋值被忽略。</summary>
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (value < -1 || value >= _items.Count || _selectedIndex == value) return;
                _selectedIndex = value;
                Invalidate();
                EventHandler h = SelectedIndexChanged;
                if (h != null) h(this, EventArgs.Empty);
            }
        }

        /// <summary>选项个数。</summary>
        public int ItemCount
        {
            get { return _items.Count; }
        }

        /// <summary>
        /// 灌入选项文字并指定选中项。**不触发 <see cref="SelectedIndexChanged"/>** ——
        /// 切语言时也会调用它，若触发会把查询重跑一遍。
        /// </summary>
        public void SetItems(IList<string> items, int selectedIndex)
        {
            _items.Clear();
            if (items != null) for (int i = 0; i < items.Count; i++) _items.Add(items[i]);

            _selectedIndex = (selectedIndex >= 0 && selectedIndex < _items.Count) ? selectedIndex : -1;
            _hoverIndex = -1;
            Invalidate();
        }

        /// <summary>轨道内边距（选中卡片与轨道边缘的间隙）。</summary>
        private int Pad
        {
            get { return UiTheme.Dpi(3); }
        }

        private int IndexAt(int x)
        {
            int n = _items.Count;
            if (n == 0) return -1;
            int segW = (Width - Pad * 2) / n;
            if (segW <= 0) return -1;
            int i = (x - Pad) / segW;
            return (i < 0 || i >= n) ? -1 : i;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);

            int n = _items.Count;
            if (n == 0 || Width < 40 || Height < 16) return;

            int pad = Pad;
            int segW = (Width - pad * 2) / n;
            int segH = Height - pad * 2;

            for (int i = 0; i < n; i++)
            {
                Rectangle seg = new Rectangle(pad + segW * i, pad, segW, segH);
                bool on = (i == _selectedIndex);

                if (on)
                {
                    using (GraphicsPath p = UiKit.RoundRect(seg, UiTheme.Dpi(7)))
                    {
                        using (SolidBrush b = new SolidBrush(UiKit.Surface)) g.FillPath(b, p);
                        using (Pen pen = new Pen(UiKit.Line, 1f)) g.DrawPath(pen, p);
                    }
                }
                else if (i == _hoverIndex)
                {
                    using (GraphicsPath p = UiKit.RoundRect(seg, UiTheme.Dpi(7)))
                    using (SolidBrush b = new SolidBrush(Color.FromArgb(70, 255, 255, 255))) g.FillPath(b, p);
                }

                TextRenderer.DrawText(g, _items[i], on ? UiTheme.BodyBold : UiTheme.Body, seg,
                    on ? UiKit.Brand : UiKit.TextSecondary,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int i = IndexAt(e.X);
            if (i != _hoverIndex)
            {
                _hoverIndex = i;
                Cursor = (i >= 0) ? Cursors.Hand : Cursors.Default;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_hoverIndex != -1)
            {
                _hoverIndex = -1;
                Invalidate();
            }
            Cursor = Cursors.Default;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;
            int i = IndexAt(e.X);
            if (i >= 0) SelectedIndex = i;
        }
    }
}
