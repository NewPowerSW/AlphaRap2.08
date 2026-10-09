using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using NPSDK;

namespace AlphaRap
{
    /// <summary>
    /// IO 点（Adlink_Input / Adlink_Output）与气缸（Adlink_Cylinder）的统一外观与自动排布。
    /// 自动排布按"输入 / 输出"分区、按可用宽度分列，设计器中的坐标只用来决定先后顺序，
    /// 因此新增 IO 点时只需放进容器，不必手工摆位。
    /// </summary>
    public static class IoPanelLayout
    {
        public const int TileHeight = 40;
        public const int TileMinWidth = 260;
        public const int TileMaxWidth = 360;
        public const int TileGap = 10;
        public const int HeaderHeight = 30;

        private class Section
        {
            public List<Control> Inputs = new List<Control>();
            public List<Control> Outputs = new List<Control>();
            public List<Control> Cylinders = new List<Control>();
            public Label InputHeader;
            public Label OutputHeader;
            public Label CylinderHeader;
            public bool Arranging;
        }

        private static readonly Dictionary<Control, Section> Sections = new Dictionary<Control, Section>();

        public static bool IsIo(Control c)
        {
            return c is Adlink_Input || c is Adlink_Output || c is Adlink_Cylinder;
        }

        /// <summary>对控件树中所有直接包含 IO 点的容器做样式与排布。</summary>
        public static void ArrangeAll(Control root)
        {
            if (root == null) return;
            List<Control> containers = new List<Control>();
            Collect(root, containers);
            foreach (Control c in containers) Arrange(c);
        }

        private static void Collect(Control parent, List<Control> result)
        {
            bool hasIo = false;
            foreach (Control c in parent.Controls)
            {
                if (IsIo(c)) hasIo = true;
                else Collect(c, result);
            }
            if (hasIo) result.Add(parent);
        }

        /// <summary>样式化并排布一个容器内的 IO 点；容器尺寸变化时自动重排。</summary>
        public static void Arrange(Control container)
        {
            Section s;
            if (!Sections.TryGetValue(container, out s))
            {
                s = new Section();
                // 按设计器中的位置（先行后列）确定显示顺序
                List<Control> io = container.Controls.Cast<Control>().Where(IsIo)
                    .OrderBy(c => c.Top / 20).ThenBy(c => c.Left).ToList();
                foreach (Control c in io)
                {
                    if (c is Adlink_Input) s.Inputs.Add(c);
                    else if (c is Adlink_Cylinder) s.Cylinders.Add(c);
                    else s.Outputs.Add(c);
                    IoTileStyle.Style(c);
                }
                s.InputHeader = MakeHeader(container);
                s.OutputHeader = MakeHeader(container);
                s.CylinderHeader = MakeHeader(container);
                Sections[container] = s;

                container.BackColor = (container is TabPage) ? UiKit.PageBg : container.BackColor;
                if (container is GroupBox)
                {
                    // 分组框只作容器：不画边框与标题（标题与选项卡名重复）
                    container.Paint += (o, e) =>
                    {
                        using (SolidBrush b = new SolidBrush(container.BackColor)) e.Graphics.FillRectangle(b, container.ClientRectangle);
                    };
                    container.BackColor = UiKit.PageBg;
                }
                if (container is ScrollableControl sc) sc.AutoScroll = true;
                container.Resize += (o, e) => Layout(container);
            }
            RefreshTexts(container);
            Layout(container);
        }

        /// <summary>按当前语言刷新分区标题（语言切换后调用）。</summary>
        public static void RefreshTexts(Control container)
        {
            Section s;
            if (!Sections.TryGetValue(container, out s)) return;
            s.InputHeader.Text = UiTheme.T("Common", "io_InputSection", "输入", "Inputs", "Entradas") + "  ·  " + s.Inputs.Count;
            s.OutputHeader.Text = UiTheme.T("Common", "io_OutputSection", "输出", "Outputs", "Salidas") + "  ·  " + s.Outputs.Count;
            s.CylinderHeader.Text = UiTheme.T("Common", "io_CylinderSection", "气缸", "Cylinders", "Cilindros") + "  ·  " + s.Cylinders.Count;
        }

        /// <summary>对所有已排布的容器刷新分区标题。</summary>
        public static void RefreshAllTexts()
        {
            foreach (Control c in Sections.Keys.ToList()) RefreshTexts(c);
        }

        private static Label MakeHeader(Control container)
        {
            UiLabel l = new UiLabel();
            l.Name = MiddleLayerNames.Dyn("ioSectionHeader");
            l.AutoSize = false;
            l.Height = HeaderHeight;
            l.Font = UiTheme.Section;
            l.ForeColor = UiKit.TextPrimary;
            l.BackColor = Color.Transparent;
            l.TextAlign = ContentAlignment.MiddleLeft;
            container.Controls.Add(l);
            return l;
        }

        private static void Layout(Control container)
        {
            Section s;
            if (!Sections.TryGetValue(container, out s) || s.Arranging) return;
            s.Arranging = true;
            try
            {
                int pad = UiTheme.PagePad;
                Point scroll = (container is ScrollableControl sc) ? sc.AutoScrollPosition : Point.Empty;
                int avail = container.ClientSize.Width - pad * 2 - SystemInformation.VerticalScrollBarWidth;
                if (avail < TileMinWidth) avail = TileMinWidth;
                int cols = Math.Max(1, (avail + TileGap) / (TileMinWidth + TileGap));
                int tileW = Math.Min(TileMaxWidth, (avail - (cols - 1) * TileGap) / cols);

                int y = pad + (container is GroupBox ? 0 : 0);
                y = Place(s.InputHeader, s.Inputs, pad, y, cols, tileW, scroll);
                if (s.Inputs.Count > 0 && s.Outputs.Count > 0) y += UiTheme.Gap;
                y = Place(s.OutputHeader, s.Outputs, pad, y, cols, tileW, scroll);
                if (s.Cylinders.Count > 0 && (s.Inputs.Count > 0 || s.Outputs.Count > 0)) y += UiTheme.Gap;
                y = Place(s.CylinderHeader, s.Cylinders, pad, y, cols, tileW, scroll);
            }
            finally { s.Arranging = false; }
        }

        private static int Place(Label header, List<Control> tiles, int x0, int y, int cols, int tileW, Point scroll)
        {
            header.Visible = tiles.Count > 0;
            if (tiles.Count == 0) return y;
            header.Bounds = new Rectangle(x0 + scroll.X, y + scroll.Y, cols * tileW + (cols - 1) * TileGap, HeaderHeight);
            y += HeaderHeight + 4;
            for (int i = 0; i < tiles.Count; i++)
            {
                int r = i / cols, c = i % cols;
                tiles[i].Bounds = new Rectangle(x0 + c * (tileW + TileGap) + scroll.X,
                                                y + r * (TileHeight + TileGap) + scroll.Y, tileW, TileHeight);
                IoTileStyle.PlaceIndicator(tiles[i]);
            }
            int rows = (tiles.Count + cols - 1) / cols;
            return y + rows * (TileHeight + TileGap) - TileGap;
        }
    }

    /// <summary>
    /// 单个 IO 点的外观：白底圆角卡片，左侧色条区分输入（灰）/ 输出（品牌蓝），
    /// 右侧显示 IO 地址（Port），状态指示（开关 / 指示灯）贴右侧居中。
    /// </summary>
    public static class IoTileStyle
    {
        private const int IndicatorMargin = 8;
        private static readonly HashSet<Control> Styled = new HashSet<Control>();
        private static readonly HashSet<Control> IndicatorHooked = new HashSet<Control>();

        public static void Style(Control c)
        {
            if (c == null || Styled.Contains(c)) return;
            Styled.Add(c);

            if (c is Adlink_Cylinder)
            {
                StyleCylinder(c);
                return;
            }

            Label tile = c as Label;
            if (tile == null) return;
            tile.MaximumSize = Size.Empty;
            tile.MinimumSize = Size.Empty;
            tile.AutoSize = false;
            tile.BorderStyle = BorderStyle.None;
            tile.BackColor = UiKit.Surface;
            tile.ForeColor = UiKit.TextPrimary;
            tile.Font = UiTheme.Body;
            tile.TextAlign = ContentAlignment.MiddleLeft;
            tile.AutoEllipsis = true;
            tile.Margin = new Padding(0);
            tile.Paint += Tile_Paint;
            tile.SizeChanged += (o, e) => PlaceIndicator(tile);
            UpdatePadding(tile);
        }

        /// <summary>把状态指示图（开关 / 指示灯）放到卡片右侧并垂直居中。</summary>
        public static void PlaceIndicator(Control tile)
        {
            if (tile is Adlink_Cylinder) return;
            PictureBox pic = tile.Controls.OfType<PictureBox>().FirstOrDefault();
            if (pic == null) return;
            if (pic.Dock != DockStyle.None) pic.Dock = DockStyle.None;
            pic.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            if (pic.BackColor != UiKit.Surface) pic.BackColor = UiKit.Surface;
            if (!IndicatorHooked.Contains(pic))
            {
                IndicatorHooked.Add(pic);
                // 指示图的底色保持与卡片一致（控件内部刷新状态时可能改回系统色）
                pic.BackColorChanged += (o, e) => { if (pic.BackColor != UiKit.Surface) pic.BackColor = UiKit.Surface; };
            }
            int h = Math.Min(pic.Height, tile.Height - 8);
            pic.Bounds = new Rectangle(tile.Width - pic.Width - IndicatorMargin, (tile.Height - h) / 2, pic.Width, h);
            UpdatePadding(tile);
        }

        private static string PortText(Control tile)
        {
            string port = null;
            if (tile is Adlink_Input) port = ((Adlink_Input)tile).Port;
            else if (tile is Adlink_Output) port = ((Adlink_Output)tile).Port;
            return string.IsNullOrEmpty(port) ? "" : port.Trim();
        }

        private static int PortWidth(Control tile)
        {
            string p = PortText(tile);
            return p.Length == 0 ? 0 : TextRenderer.MeasureText(p, UiTheme.Caption).Width + 10;
        }

        private static void UpdatePadding(Control tile)
        {
            PictureBox pic = tile.Controls.OfType<PictureBox>().FirstOrDefault();
            int right = (pic != null ? pic.Width + IndicatorMargin * 2 : IndicatorMargin) + PortWidth(tile);
            Padding want = new Padding(14, 0, right, 0);
            if (tile.Padding != want) tile.Padding = want;
        }

        private static void Tile_Paint(object sender, PaintEventArgs e)
        {
            Control tile = (Control)sender;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle r = new Rectangle(0, 0, tile.Width - 1, tile.Height - 1);

            // 圆角以外的部分刷成父容器底色
            Color parentBack = UiKit.ResolveParentBack(tile);
            using (GraphicsPath outer = new GraphicsPath())
            using (GraphicsPath card = UiKit.RoundRect(r, 6))
            {
                outer.AddRectangle(new Rectangle(-1, -1, tile.Width + 2, tile.Height + 2));
                outer.AddPath(card, false);
                using (SolidBrush b = new SolidBrush(parentBack)) g.FillPath(b, outer);
                using (Pen p = new Pen(UiKit.Line)) g.DrawPath(p, card);
            }

            // 左侧色条：输入灰、输出品牌蓝
            Color mark = tile is Adlink_Input ? UiTheme.InputMark
                       : tile is Adlink_Cylinder ? UiKit.Warn : UiKit.Brand;
            using (SolidBrush b = new SolidBrush(mark)) g.FillRectangle(b, 1, 8, 3, tile.Height - 16);

            // IO 地址
            string port = PortText(tile);
            if (port.Length > 0)
            {
                PictureBox pic = tile.Controls.OfType<PictureBox>().FirstOrDefault();
                int right = tile.Width - (pic != null ? pic.Width + IndicatorMargin * 2 : IndicatorMargin);
                int w = PortWidth(tile);
                Rectangle pr = new Rectangle(right - w, 0, w, tile.Height);
                TextRenderer.DrawText(g, port, UiTheme.Caption, pr, UiKit.TextMuted,
                    TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            }
        }

        /// <summary>气缸卡片：左侧名称，右侧"伸出 / 缩回"两个按钮。</summary>
        private static void StyleCylinder(Control cyl)
        {
            UserControl uc = cyl as UserControl;
            if (uc != null) uc.BorderStyle = BorderStyle.None;
            cyl.MaximumSize = Size.Empty;
            cyl.MinimumSize = Size.Empty;
            cyl.BackColor = UiKit.Surface;
            cyl.Padding = new Padding(14, 5, 6, 5);

            foreach (Button b in Descendants(cyl).OfType<Button>())
            {
                UiTheme.StyleButton(b, false);
                b.Font = UiTheme.BodyBold;
                b.Margin = new Padding(4, 0, 0, 0);
                b.Padding = Padding.Empty;
                b.TextAlign = ContentAlignment.MiddleCenter;
                b.ImageAlign = ContentAlignment.MiddleCenter;
                b.Image = null;
                b.Dock = DockStyle.Fill;
            }
            TableLayoutPanel tlp = Descendants(cyl).OfType<TableLayoutPanel>().FirstOrDefault();
            if (tlp != null)
            {
                tlp.BackColor = UiKit.Surface;
                tlp.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
                tlp.Padding = Padding.Empty;
                tlp.Dock = DockStyle.Right;
                tlp.Width = 150;
                tlp.Margin = new Padding(0);
            }

            UiLabel name = new UiLabel();
            name.Name = MiddleLayerNames.Dyn(cyl.Name + "_title");
            name.AutoSize = false;
            name.Dock = DockStyle.Fill;
            name.Font = UiTheme.Body;
            name.ForeColor = UiKit.TextPrimary;
            name.BackColor = UiKit.Surface;
            name.TextAlign = ContentAlignment.MiddleLeft;
            name.AutoEllipsis = true;
            name.Text = CylinderTitle(cyl);
            cyl.Controls.Add(name);
            name.SendToBack();
            if (tlp != null) tlp.SendToBack();
            name.BringToFront();

            cyl.Paint += Tile_Paint;
        }

        /// <summary>气缸名称：控件 Text 不为空时用 Text，否则取控件名最后两段（如 Stop_Cylinder → Stop Cylinder）。</summary>
        private static string CylinderTitle(Control cyl)
        {
            if (!string.IsNullOrWhiteSpace(cyl.Text)) return cyl.Text.Trim();
            string[] parts = (cyl.Name ?? "").Split('_');
            int n = parts.Length;
            if (n >= 2) return parts[n - 2] + " " + parts[n - 1];
            return cyl.Name;
        }

        private static IEnumerable<Control> Descendants(Control c)
        {
            foreach (Control k in c.Controls)
            {
                yield return k;
                foreach (Control d in Descendants(k)) yield return d;
            }
        }
    }

    /// <summary>运行时创建的控件名约定（与 MiddleLayer.DynTextSuffix 一致：带此后缀的控件不进语言表）。</summary>
    internal static class MiddleLayerNames
    {
        public const string DynSuffix = "_dyn";
        public static string Dyn(string name) { return name + DynSuffix; }
    }
}
