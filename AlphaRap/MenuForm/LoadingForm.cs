using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>
    /// 启动加载页：沿用主界面顶栏那套视觉 —— 品牌蓝 (4,108,182) 头带 + 白色 NP 标 +
    /// 产品名 / 版本号，下面是当前加载阶段与一条来回走的进度条。
    ///
    /// ⚠ **本页所有内容都必须由 <see cref="RefreshUI"/> 在后台线程里直接画到各个控件表面上。**
    /// 原因：Program.Main 里 `Show()` 之后主线程立刻进 `MiddleLayer.InitialProject()` 干活，
    /// 整个加载期间**没有消息泵**，窗体和控件都不会自己重绘 —— 谁不在这里画，谁就是一片空白
    /// （所以这里既不靠 PictureBox 自动显示图片，也不靠控件的 BackColor）。
    ///
    /// 各区域用现成的"表面"：`splitContainer1` = 分隔缝、`splitContainer1.Panel1` = 蓝色头带、
    /// `picLogo` = 白色 NP 标、`picText1` = 产品名 + 版本、`picText2` = 白区（阶段文字 + 进度条）。
    /// </summary>
    public partial class LoadingForm : Form
    {
        private string _Caption = string.Empty;
        public bool StopRefresh;

        // ---------------- 视觉参数（设计值一律按 96 DPI 写，绘制时用 UiTheme.Dpi 换算） ----------------
        private const int PadX = 40;            // 左右留白
        private const int BarHeight = 10;       // 进度条高度
        private const int ThumbWidth = 130;     // 进度条滑块宽度
        private const int StatusLineH = 24;     // 状态文字占位高（12pt 行）
        private const int StatusToBar = 26;     // 状态文字 → 进度条
        private const int MarqueeFrames = 56;   // 进度条来回一次用多少帧
        private const int FrameMs = 40;         // 刷新周期（≈25 fps）

        /// <summary>进度条底槽色（浅灰蓝，与卡片分割线同一族）。</summary>
        private static readonly Color TrackColor = Color.FromArgb(230, 236, 242);

        public LoadingForm()
        {
            InitializeComponent();
            UiTheme.ApplyDpi(this);
            ApplyRoundRegion();
        }

        public void SetCaption(string Caption)
        {
            _Caption = Caption;
        }

        /// <summary>窗口圆角（与主界面同样处理），让加载页看起来是一张卡片而不是硬边方框。</summary>
        private void ApplyRoundRegion()
        {
            try
            {
                int d = UiTheme.Dpi(12) * 2;
                if (ClientSize.Width <= d * 2 || ClientSize.Height <= d * 2) return;

                Rectangle r = new Rectangle(0, 0, ClientSize.Width, ClientSize.Height);
                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddArc(r.X, r.Y, d, d, 180, 90);
                    path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
                    path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
                    path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
                    path.CloseFigure();
                    if (Region != null) Region.Dispose();
                    Region = new Region(path);
                }
            }
            catch { }
        }

        private static string AppName { get { return "AlphaRap-SRM"; } }

        private static string AppVersion
        {
            get
            {
                try
                {
                    Version v = Assembly.GetExecutingAssembly().GetName().Version;
                    return v.Major + "." + v.Minor + "." + v.Build;
                }
                catch { return "1.0.0"; }
            }
        }

        /// <summary>
        /// 加载动画。**在后台线程里跑**，直到 Program.Main 把 <see cref="StopRefresh"/> 置位。
        /// </summary>
        public void RefreshUI()
        {
            Graphics gSplit = null, gBand = null, gLogo = null, gTitle = null, gBody = null;
            Font fTitle = null, fVersion = null, fStatus = null;
            try
            {
                if (StopRefresh) return;

                // 每块表面只取一次 Graphics，整个动画期间复用
                gSplit = splitContainer1.CreateGraphics();
                gBand = splitContainer1.Panel1.CreateGraphics();
                gLogo = picLogo.CreateGraphics();
                gTitle = picText1.CreateGraphics();
                gBody = picText2.CreateGraphics();

                fTitle = new Font("微软雅黑", 20f, FontStyle.Bold, GraphicsUnit.Point, 134);
                fVersion = new Font("微软雅黑", 10f, FontStyle.Regular, GraphicsUnit.Point, 134);
                fStatus = new Font("微软雅黑", 12f, FontStyle.Regular, GraphicsUnit.Point, 134);

                // ---- 静态部分：头带、Logo、产品名/版本号、白区底色，只画一次 ----
                gSplit.Clear(Color.White);              // Splitter 那条 1px 缝：涂白，蓝带与白区干净相接
                gBand.Clear(UiKit.Brand);               // 头带（与主界面顶栏同色）
                gLogo.Clear(UiKit.Brand);               // 图片是"白 + 透明"，先铺蓝底再贴
                gTitle.Clear(UiKit.Brand);
                gBody.Clear(Color.White);

                DrawLogo(gLogo);
                DrawTitle(gTitle, fTitle, fVersion);

                // ---- 动态部分：阶段文字 + 进度条 ----
                int frame = 0;
                while (!StopRefresh)
                {
                    DrawProgress(gBody, fStatus, frame);
                    frame++;
                    Thread.Sleep(FrameMs);
                }
            }
            catch { }
            finally
            {
                // 线程已退出（Main 里先 Join 再 Close），这里统一回收 GDI 资源
                try { if (gSplit != null) gSplit.Dispose(); } catch { }
                try { if (gBand != null) gBand.Dispose(); } catch { }
                try { if (gLogo != null) gLogo.Dispose(); } catch { }
                try { if (gTitle != null) gTitle.Dispose(); } catch { }
                try { if (gBody != null) gBody.Dispose(); } catch { }
                try { if (fTitle != null) fTitle.Dispose(); } catch { }
                try { if (fVersion != null) fVersion.Dispose(); } catch { }
                try { if (fStatus != null) fStatus.Dispose(); } catch { }
            }
        }

        /// <summary>白色 NP 标：等比缩放居中铺在蓝色底上。</summary>
        private void DrawLogo(Graphics g)
        {
            Image logo = picLogo.Image;
            if (logo == null) return;

            Size box = picLogo.ClientSize;
            if (box.Width <= 0 || box.Height <= 0) return;

            double k = Math.Min((double)box.Width / logo.Width, (double)box.Height / logo.Height);
            int w = Math.Max(1, (int)Math.Round(logo.Width * k));
            int h = Math.Max(1, (int)Math.Round(logo.Height * k));

            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.DrawImage(logo, new Rectangle((box.Width - w) / 2, (box.Height - h) / 2, w, h));
        }

        /// <summary>产品名 + 版本号（白色，压在品牌蓝底上）。</summary>
        private void DrawTitle(Graphics g, Font fTitle, Font fVersion)
        {
            Size s = picText1.ClientSize;
            if (s.Width <= 0 || s.Height <= 0) return;

            // 白字压在实色蓝底上：用灰度抗锯齿，免得 ClearType 在白字上出彩边
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (SolidBrush b = new SolidBrush(Color.White))
                g.DrawString(AppName, fTitle, b, new PointF(0, UiTheme.Dpi(9)));

            using (SolidBrush b = new SolidBrush(Color.FromArgb(185, 255, 255, 255)))
                g.DrawString("Version " + AppVersion, fVersion, b, new PointF(UiTheme.Dpi(3), UiTheme.Dpi(48)));
        }

        /// <summary>当前加载阶段 + 来回走的进度条（整块白区重画，保证不留残影）。</summary>
        private void DrawProgress(Graphics g, Font fStatus, int frame)
        {
            Size s = picText2.ClientSize;
            if (s.Width <= 0 || s.Height <= 0) return;

            int padX = UiTheme.Dpi(PadX);
            int barH = UiTheme.Dpi(BarHeight);
            int groupH = UiTheme.Dpi(StatusLineH + StatusToBar + BarHeight);
            int top = Math.Max(UiTheme.Dpi(12), (s.Height - groupH) / 2);
            int barY = top + UiTheme.Dpi(StatusLineH + StatusToBar);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // 整块重画（文字内容会变），避免上一帧的字留印
            using (SolidBrush bg = new SolidBrush(Color.White))
                g.FillRectangle(bg, 0, 0, s.Width, s.Height);

            // 阶段文字 + 动态点
            string dots = string.Empty;
            for (int i = 0; i <= (frame % 4); i++) dots += " .";
            using (SolidBrush b = new SolidBrush(UiKit.TextPrimary))
                g.DrawString((_Caption ?? string.Empty) + dots, fStatus, b, new PointF(padX, top));

            // 进度条
            int barW = s.Width - padX * 2;
            if (barW <= barH * 3) return;

            using (GraphicsPath track = UiKit.RoundRect(new Rectangle(padX, barY, barW, barH), barH / 2))
            using (SolidBrush tb = new SolidBrush(TrackColor))
                g.FillPath(tb, track);

            int thumbW = Math.Min(UiTheme.Dpi(ThumbWidth), barW);
            int travel = barW - thumbW;
            double t = (frame % MarqueeFrames) / (double)MarqueeFrames;
            double pos = t < 0.5 ? t * 2.0 : (1.0 - t) * 2.0;     // 0 → 1 → 0 往返
            int thumbX = padX + (int)Math.Round(travel * pos);

            using (GraphicsPath thumb = UiKit.RoundRect(new Rectangle(thumbX, barY, thumbW, barH), barH / 2))
            using (SolidBrush pb = new SolidBrush(UiKit.Brand))
                g.FillPath(pb, thumb);
        }
    }
}
