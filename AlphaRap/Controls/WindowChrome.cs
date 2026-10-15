using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>
    /// 框架风格的无边框窗口外观（与报警查询窗口一致）：
    /// 品牌蓝标题栏（图标 + 标题 + 最小化 / 关闭）、按住标题栏拖动、右下角缩放柄、
    /// 圆角窗口、Esc 关闭。
    ///
    /// 用法（构造函数里，<c>UiTheme.ApplyDpi</c> 与建界面之后）：
    /// <code>
    /// _chrome = WindowChrome.Attach(this, AppIcon.Data);
    /// _chrome.Title = "生产数据查询";
    /// </code>
    /// 语言切换后重新给 <see cref="Title"/> 赋值即可。
    /// </summary>
    public sealed class WindowChrome
    {
        /// <summary>窗口圆角半径（96 DPI 设计值）。</summary>
        private const int WindowCornerRadius = 12;

        private const int WmNcLButtonDown = 0x00A1;
        private const int HtCaption = 2;
        private const int HtBottomRight = 17;

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(int left, int top, int right, int bottom, int w, int h);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        private readonly Form _form;
        private readonly Panel _titleBar;
        private readonly Label _label;
        private readonly PictureBox _iconBox;
        private readonly FlatButton _min;
        private readonly FlatButton _close;
        private readonly Panel _grip;

        private Image _iconImage;
        private string _title = "";

        private WindowChrome(Form form)
        {
            _form = form;

            // ---- 标题栏（品牌蓝，46 设计像素高，底部一条深色线）----
            _titleBar = new Panel();
            _titleBar.Dock = DockStyle.Top;
            _titleBar.Height = D(46);
            _titleBar.BackColor = UiKit.Brand;
            _titleBar.Name = "chromeTitleBar";

            Panel line = new Panel();
            line.Dock = DockStyle.Bottom;
            line.Height = 1;
            line.BackColor = UiKit.BrandDark;
            line.Name = "chromeTitleLine";
            _titleBar.Controls.Add(line);

            _iconBox = new PictureBox();
            _iconBox.BackColor = Color.Transparent;
            _iconBox.Location = new Point(D(20), D(13));
            _iconBox.Size = new Size(D(20), D(20));
            _iconBox.SizeMode = PictureBoxSizeMode.CenterImage;
            _iconBox.Name = "chromeTitleIcon";
            _iconBox.TabStop = false;
            _titleBar.Controls.Add(_iconBox);

            _label = new Label();
            _label.AutoSize = false;
            _label.BackColor = Color.Transparent;
            _label.Font = UiKit.Bold(12f);
            _label.ForeColor = UiKit.TextOnBrand;
            _label.Location = new Point(D(50), 0);
            _label.Size = new Size(D(700), D(45));
            _label.TextAlign = ContentAlignment.MiddleLeft;
            _label.Name = "chromeTitleLabel";
            _titleBar.Controls.Add(_label);

            // 右侧的标题栏按钮：Dock=Right 的流式容器，从右往左排「最小化 / 关闭」
            FlowLayoutPanel flowCap = new FlowLayoutPanel();
            flowCap.Dock = DockStyle.Right;
            flowCap.BackColor = UiKit.Brand;
            flowCap.FlowDirection = FlowDirection.LeftToRight;
            flowCap.WrapContents = false;
            flowCap.Padding = new Padding(0, D(7), D(12), 0);
            flowCap.Margin = new Padding(0);
            flowCap.Name = "chromeCaption";
            flowCap.AutoSize = true;
            _titleBar.Controls.Add(flowCap);

            _min = NewCaptionButton("chromeMin");
            flowCap.Controls.Add(_min);

            _close = NewCaptionButton("chromeClose");
            flowCap.Controls.Add(_close);

            // ---- 右下角缩放柄 ----
            _grip = new Panel();
            _grip.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _grip.BackColor = UiKit.PageBg;
            _grip.Cursor = Cursors.SizeNWSE;
            _grip.Size = new Size(D(22), D(20));
            _grip.Name = "chromeGrip";
            _grip.Paint += Grip_Paint;
            _grip.MouseDown += Grip_MouseDown;

            // 拖动：标题栏本身与它上面所有非按钮控件都能拖
            _titleBar.MouseDown += TitleBar_MouseDown;
            _label.MouseDown += TitleBar_MouseDown;
            _iconBox.MouseDown += TitleBar_MouseDown;
            line.MouseDown += TitleBar_MouseDown;

            _min.Click += delegate { _form.WindowState = FormWindowState.Minimized; };
            // Ghost 变体的悬停底色是浅蓝，白图标会糊掉 ⇒ 悬停时图标同步转品牌色
            _min.MouseEnter += delegate { _min.Icon = WhiteIcon(AppIcon.Minimize, UiKit.Brand); };
            _min.MouseLeave += delegate { _min.Icon = WhiteIcon(AppIcon.Minimize, Color.White); };

            _close.MouseEnter += delegate { _close.Variant = FlatButtonVariant.Danger; };
            _close.MouseLeave += delegate { _close.Variant = FlatButtonVariant.Ghost; };
            _close.Click += delegate { _form.Close(); };

            // 无边框窗口：Esc 就是"关掉"
            _form.KeyPreview = true;
            _form.KeyDown += Form_KeyDown;
            _form.Resize += Form_Resize;
        }

        private FlatButton NewCaptionButton(string name)
        {
            FlatButton b = new FlatButton();
            b.CornerRadius = 7;
            b.IconGap = 0;
            b.IconSize = D(14);
            b.Size = new Size(D(38), D(30));
            b.Margin = new Padding(0, 0, D(4), 0);
            b.Variant = FlatButtonVariant.Ghost;
            b.Name = name;
            b.TabStop = false;
            return b;
        }

        /// <summary>标题栏文字（语言切换后重新赋值即可）。</summary>
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value ?? "";
                try
                {
                    _label.Text = _title;
                    _form.Text = _title;      // 任务栏 / 任务管理器里也要有名字
                }
                catch { }
            }
        }

        /// <summary>
        /// 给窗体挂上框架风格的标题栏。会设置 <c>FormBorderStyle = None</c>，
        /// 并把标题栏放到内容之上（最后加入 ⇒ 先占顶部，内容 Fill 拿剩下的）。
        /// </summary>
        public static WindowChrome Attach(Form form, AppIcon icon, string title)
        {
            if (form == null) return null;

            WindowChrome chrome = new WindowChrome(form);
            try
            {
                form.FormBorderStyle = FormBorderStyle.None;
                form.ShowIcon = false;
                form.MinimizeBox = true;
                form.MaximizeBox = true;

                form.SuspendLayout();
                form.Controls.Add(chrome._titleBar);   // 最后加入：Dock 顺序上先占顶部
                form.Controls.Add(chrome._grip);

                // 缩放柄必须显式定位到右下角：Anchor 的基准是"控件当前的边距"，
                // Location 留在 (0,0) 时它记下的是"距右边缘 W-22"，之后只会跟着尺寸变化量漂移，
                // 永远到不了右下角（实测初始 {0,0}、期望 {W-22,H-20}）。
                int gripW = chrome._grip.Width;
                int gripH = chrome._grip.Height;
                chrome._grip.Location = new Point(
                    Math.Max(0, form.ClientSize.Width - gripW),
                    Math.Max(0, form.ClientSize.Height - gripH));
                // 否则它排在 Controls 末尾 = 最底层，被 Dock=Fill 的内容面板完全盖住
                chrome._grip.BringToFront();
                form.ResumeLayout(true);

                chrome._iconImage = AppIcons.Get(icon, D(20), Color.White);
                chrome._iconBox.Image = chrome._iconImage;
                chrome._min.Icon = WhiteIcon(AppIcon.Minimize, Color.White);
                chrome._close.Icon = WhiteIcon(AppIcon.Close, Color.White);

                chrome.Title = string.IsNullOrEmpty(title) ? form.Text : title;
                chrome.UpdateWindowRegion();
            }
            catch { }
            return chrome;
        }

        /// <summary>解绑窗体事件（窗体关闭时调用，避免泄漏）。</summary>
        public void Detach()
        {
            try
            {
                _form.KeyDown -= Form_KeyDown;
                _form.Resize -= Form_Resize;
            }
            catch { }
        }

        private static Image WhiteIcon(AppIcon icon, Color color)
        {
            return AppIcons.Get(icon, D(14), color);
        }

        private static int D(int v) { return UiTheme.Dpi(v); }

        // ---------------- 行为 ----------------

        /// <summary>按住标题栏拖动：无边框窗口没有系统标题栏，借系统的 WM_NCLBUTTONDOWN(HTCAPTION) 走原生拖动。</summary>
        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            try { ReleaseCapture(); SendMessage(_form.Handle, WmNcLButtonDown, (IntPtr)HtCaption, IntPtr.Zero); }
            catch { }
        }

        /// <summary>右下角缩放柄：同样是借系统的边框缩放（HTBOTTOMRIGHT）。</summary>
        private void Grip_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || _form.WindowState != FormWindowState.Normal) return;
            try { ReleaseCapture(); SendMessage(_form.Handle, WmNcLButtonDown, (IntPtr)HtBottomRight, IntPtr.Zero); }
            catch { }
        }

        /// <summary>右下角缩放柄上的三道斜线（系统缩放柄的经典样式）。</summary>
        private void Grip_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                e.Graphics.Clear(_grip.BackColor);
                int w = _grip.Width - 1;
                int h = _grip.Height - 1;
                using (Pen p = new Pen(UiKit.LineStrong, 1f))
                {
                    for (int i = 1; i <= 3; i++)
                    {
                        int off = D(i * 5);
                        if (off >= w || off >= h) break;
                        e.Graphics.DrawLine(p, w - off, h, w, h - off);
                    }
                }
            }
            catch { }
        }

        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Escape) return;
            _form.Close();
            e.Handled = true;
        }

        private void Form_Resize(object sender, EventArgs e)
        {
            UpdateWindowRegion();
        }

        /// <summary>按当前尺寸重建窗口圆角；最大化时不裁角，否则四角会把全屏内容切掉。</summary>
        private void UpdateWindowRegion()
        {
            try
            {
                if (_form.IsDisposed || _form.Width <= 0 || _form.Height <= 0) return;
                if (UiTheme.IsDesignTime) return;          // 设计器里不裁，免得看不到完整边框
                if (_form.WindowState == FormWindowState.Minimized) return;
                if (_form.WindowState == FormWindowState.Maximized)
                {
                    _form.Region = null;
                    return;
                }

                int corner = D(WindowCornerRadius);
                IntPtr hRgn = CreateRoundRectRgn(0, 0, _form.Width + 1, _form.Height + 1, corner, corner);
                if (hRgn == IntPtr.Zero) return;

                Region old = _form.Region;
                _form.Region = Region.FromHrgn(hRgn);
                DeleteObject(hRgn);
                if (old != null) old.Dispose();
            }
            catch { }
        }
    }
}
