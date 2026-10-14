using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>
    /// 日期选择框：自绘的圆角白底胶囊（日历图标 + 日期文字 + 下拉箭头），点击弹出一个只含
    /// <see cref="MonthCalendar"/> 的无边框小窗。
    ///
    /// 用它替掉原生 <c>DateTimePicker</c>：后者由系统绘制 3D 边框与下拉按钮，在扁平界面里很突兀；
    /// 而且它的**高度由字体决定、不接受 Size.Height**，无法与 32 高的其他控件对齐。
    /// 本控件在 96 DPI 设计网格上工作，高度随容器给，垂直居中绘制。
    /// </summary>
    public class DateField : Control
    {
        private DateTime _value = DateTime.Today;
        private bool _hover;

        private Form _popup;
        private MonthCalendar _cal;

        /// <summary>用户在弹出的日历里选定日期后触发。</summary>
        public event EventHandler ValueChanged;

        public DateField()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Selectable, false);      // 点击后不留焦点虚线框

            BackColor = UiKit.FieldBg;
            Font = UiKit.Regular(10f);
            Cursor = Cursors.Hand;
            TabStop = false;
            Size = new Size(180, 32);
        }

        /// <summary>当前日期（只保留日期部分）。</summary>
        public DateTime Value
        {
            get { return _value; }
            set
            {
                DateTime d = value.Date;
                if (_value == d) return;
                _value = d;
                Invalidate();
                EventHandler h = ValueChanged;
                if (h != null) h(this, EventArgs.Empty);
            }
        }

        /// <summary>日期显示格式。</summary>
        public string Format { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(UiKit.ResolveParentBack(this));

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = UiKit.RoundRect(rect, UiTheme.Dpi(8)))
            {
                using (SolidBrush b = new SolidBrush(_hover ? Color.White : UiKit.FieldBg))
                    g.FillPath(b, path);
                using (Pen p = new Pen(_hover ? UiKit.Brand : UiKit.LineStrong, _hover ? 1.4f : 1f))
                    g.DrawPath(p, path);
            }

            int ih = UiTheme.Dpi(14);
            Image icon = AppIcons.Get(AppIcon.Calendar, ih, UiKit.Brand);
            if (icon != null) g.DrawImage(icon, new Rectangle(UiTheme.Dpi(9), (Height - ih) / 2, ih, ih));

            string text = _value.ToString(string.IsNullOrEmpty(Format) ? "yyyy/MM/dd" : Format);
            TextRenderer.DrawText(g, text, Font,
                new Rectangle(UiTheme.Dpi(29), 0, Math.Max(1, Width - UiTheme.Dpi(46)), Height),
                UiKit.TextPrimary, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

            // 下拉箭头
            int ax = Width - UiTheme.Dpi(16);
            int ay = Height / 2 - UiTheme.Dpi(2);
            int aw = UiTheme.Dpi(9);
            int ah = UiTheme.Dpi(5);
            using (SolidBrush b = new SolidBrush(UiKit.TextMuted))
                g.FillPolygon(b, new[]
                {
                    new Point(ax, ay), new Point(ax + aw, ay), new Point(ax + aw / 2, ay + ah)
                });
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
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left) TogglePopup();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) ClosePopup();
            base.Dispose(disposing);
        }

        // ---------------- 弹出日历 ----------------

        private void TogglePopup()
        {
            if (_popup != null && _popup.Visible) { ClosePopup(); return; }
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;   // 设计器里不弹窗

            try
            {
                _cal = new MonthCalendar();
                _cal.MaxSelectionCount = 1;
                _cal.SetDate(_value);
                _cal.DateSelected += delegate(object s, DateRangeEventArgs e)
                {
                    Value = e.Start.Date;
                    ClosePopup();
                };

                _popup = new Form();
                _popup.FormBorderStyle = FormBorderStyle.None;
                _popup.ShowInTaskbar = false;
                _popup.StartPosition = FormStartPosition.Manual;
                _popup.AutoScaleMode = AutoScaleMode.None;
                _popup.BackColor = UiKit.LineStrong;          // 1px 描边靠「底色 + Padding=1」留出
                _popup.Padding = new Padding(UiTheme.Dpi(1));
                _popup.Controls.Add(_cal);
                _cal.Location = new Point(UiTheme.Dpi(1), UiTheme.Dpi(1));
                _popup.ClientSize = new Size(_cal.Width + UiTheme.Dpi(2), _cal.Height + UiTheme.Dpi(2));
                _popup.Deactivate += delegate { ClosePopup(); };

                Point below = PointToScreen(new Point(0, Height + UiTheme.Dpi(3)));
                Screen sc = Screen.FromControl(this);
                if (below.Y + _popup.Height > sc.WorkingArea.Bottom)
                    below.Y = PointToScreen(new Point(0, 0)).Y - _popup.Height - UiTheme.Dpi(3);
                if (below.X + _popup.Width > sc.WorkingArea.Right)
                    below.X = sc.WorkingArea.Right - _popup.Width;
                _popup.Location = below;

                Form owner = FindForm();
                if (owner != null) _popup.Show(owner);
                else _popup.Show();
            }
            catch { ClosePopup(); }
        }

        private void ClosePopup()
        {
            Form p = _popup;
            _popup = null;
            _cal = null;
            if (p == null) return;
            try { p.Hide(); p.Dispose(); } catch { }
        }
    }
}
