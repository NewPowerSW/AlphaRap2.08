using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>
    /// 顶栏语言切换器：单色地球图标（GDI+ 绘制）+ 当前语言代码，点击弹出语言菜单。
    /// </summary>
    public class LanguageSwitch : Control
    {
        /// <summary>语言代码，索引 = (int)LanguageType：0=Chinese / 1=English / 2=Español。</summary>
        private static readonly string[] LanguageCodes = { "CN", "EN", "ES" };

        /// <summary>菜单项使用各语言自己的名称（中文 / English / Español），不随界面语言翻译。</summary>
        private static readonly string[] LanguageNativeNames = { "中文", "English", "Español" };

        private const int IconSize = 20;      // 地球图标边长
        private const int GapIconText = 7;    // 图标 → 文字
        private const int GapTextArrow = 6;   // 文字 → 下拉箭头
        private const int ArrowWidth = 10;
        private const int ArrowHeight = 6;

        private LanguageType _current = LanguageType.Chinese;
        private bool _hover;
        private bool _open;
        private ContextMenuStrip _menu;

        /// <summary>用户从菜单里选定了某语言。同步 <see cref="Current"/> 不会触发它。</summary>
        public event EventHandler LanguageSelected;

        public LanguageSwitch()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Selectable, false);   // 点击不留焦点虚线框

            TabStop = false;
            Cursor = Cursors.Hand;
            BackColor = Color.FromArgb(4, 108, 182);     // 与顶栏同色，保证底色干净
            Font = new Font("微软雅黑", 10.5F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(134)));
        }

        /// <summary>当前语言。赋值只刷新显示，不会回环触发 <see cref="LanguageSelected"/>。</summary>
        public LanguageType Current
        {
            get { return _current; }
            set
            {
                if (_current == value) return;
                _current = value;
                Invalidate();
            }
        }

        // ---------------------------------------------------------------- 绘制

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 悬停 / 展开时浮出一层极淡的白，形成"可点击"的暗示
            if (_hover || _open)
            {
                Rectangle chip = new Rectangle(3, 7, Math.Max(1, Width - 7), Math.Max(1, Height - 15));
                using (var path = RoundedRect(chip, 8))
                using (var brush = new SolidBrush(Color.FromArgb(_open ? 46 : 24, 255, 255, 255)))
                    g.FillPath(brush, path);
            }

            int index = (int)_current;
            string code = (index >= 0 && index < LanguageCodes.Length) ? LanguageCodes[index] : "??";

            Size textSize = TextRenderer.MeasureText(code, Font, new Size(int.MaxValue, int.MaxValue),
                                                     TextFormatFlags.NoPadding);
            int contentWidth = IconSize + GapIconText + textSize.Width + GapTextArrow + ArrowWidth;
            int x = (Width - contentWidth) / 2;
            int cy = Height / 2;

            // 地球图标
            DrawGlobe(g, new Rectangle(x, cy - IconSize / 2, IconSize, IconSize), Color.White);
            x += IconSize + GapIconText;

            // 语言代码
            TextRenderer.DrawText(g, code, Font,
                new Rectangle(x, cy - textSize.Height / 2, textSize.Width + 2, textSize.Height),
                Color.White, TextFormatFlags.NoPadding | TextFormatFlags.Left);
            x += textSize.Width + GapTextArrow;

            // 下拉箭头
            DrawChevron(g, x, cy - 2, ArrowWidth, ArrowHeight, Color.White);
        }

        /// <summary>画一个地球：外圆 + 一条经线 + 两条纬线。18~20px 下辨识度最好。</summary>
        private static void DrawGlobe(Graphics g, Rectangle r, Color color)
        {
            float cx = r.X + r.Width / 2f;
            float cy = r.Y + r.Height / 2f;
            float rad = Math.Min(r.Width, r.Height) / 2f - 1.0f;
            if (rad <= 1f) return;

            using (var pen = new Pen(color, 1.6f))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;

                g.DrawEllipse(pen, cx - rad, cy - rad, rad * 2f, rad * 2f);                       // 外圆
                g.DrawEllipse(pen, cx - rad * 0.46f, cy - rad, rad * 0.92f, rad * 2f);            // 经线

                float lat = rad * 0.44f;                                                          // 南北纬线
                float half = (float)Math.Sqrt(Math.Max(0f, rad * rad - lat * lat));
                g.DrawLine(pen, cx - half, cy - lat, cx + half, cy - lat);
                g.DrawLine(pen, cx + half, cy + lat, cx - half, cy + lat);
            }
        }

        private static void DrawChevron(Graphics g, int x, int y, int w, int h, Color color)
        {
            using (var pen = new Pen(color, 1.6f))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                pen.LineJoin = LineJoin.Round;
                g.DrawLines(pen, new Point[]
                {
                    new Point(x, y),
                    new Point(x + w / 2, y + h),
                    new Point(x + w, y)
                });
            }
        }

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
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

        // ---------------------------------------------------------------- 交互

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
            if (e.Button == MouseButtons.Left) ShowLanguageMenu();
        }

        private void ShowLanguageMenu()
        {
            try
            {
                if (_menu == null)
                {
                    _menu = new ContextMenuStrip();
                    _menu.Font = new Font("微软雅黑", 10F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(134)));
                    _menu.ShowImageMargin = false;
                    _menu.ShowCheckMargin = true;
                    for (int i = 0; i < LanguageNativeNames.Length; i++)
                    {
                        ToolStripMenuItem item = new ToolStripMenuItem(LanguageNativeNames[i]);
                        item.Tag = (LanguageType)i;
                        item.Click += LanguageMenuItem_Click;
                        _menu.Items.Add(item);
                    }
                    _menu.Closed += delegate { _open = false; Invalidate(); };
                }

                for (int i = 0; i < _menu.Items.Count; i++)
                    ((ToolStripMenuItem)_menu.Items[i]).Checked = (i == (int)_current);

                _open = true;
                Invalidate();
                _menu.Show(this, new Point(0, Height + 2));
            }
            catch { }
        }

        private void LanguageMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ToolStripMenuItem item = sender as ToolStripMenuItem;
                if (item == null || !(item.Tag is LanguageType)) return;

                LanguageType target = (LanguageType)item.Tag;
                if (target == _current) return;      // 选中的就是当前语言，无需重复切换

                Current = target;
                if (LanguageSelected != null) LanguageSelected(this, EventArgs.Empty);
            }
            catch { }
        }
    }
}
