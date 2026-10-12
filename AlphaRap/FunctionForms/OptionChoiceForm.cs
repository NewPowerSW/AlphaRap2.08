using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AlphaRap;          // UiKit / UiTheme / FlatButton / MiddleLayer 都在这个命名空间

namespace Alpha
{
    /// <summary>
    /// 弹窗选择框：一句消息 + 最多三个按钮，用于"确认 / 报警提示 / 二选一"这类必须让操作员表态的场合。
    ///
    /// <para><b>结果口径</b>：
    /// <see cref="dResult"/> = <c>Yes</c>(按钮1) / <c>Ignore</c>(按钮2) / <c>Abort</c>(按钮3) / <c>None</c>(还没选)。
    /// 用法：<c>new OptionChoiceForm()</c> → <c>fnChangeButtonsText(...)</c> → <c>fnSetMessageAndButtons(...)</c> →
    /// <c>ShowDialog()</c>，然后读 <c>dResult</c>。</para>
    ///
    /// <para><b>视觉</b>：白卡片（圆角 + 1px 描边）+ 顶部严重度色条 + 底部按钮区。
    /// 色条颜色、按钮配色都由代码按语义自动决定 —— 按钮 1 = 确认（绿）、2 = 次要（白）、3 = 取消（红）；
    /// **只显示一个按钮时这个按钮升级为主操作（品牌蓝）**，色条跟着它变色。
    /// 隐藏的按钮不占位：可见按钮在按钮区里居中排布。</para>
    ///
    /// <para><b>线程安全</b>：所有对外方法内部都会切回 UI 线程。构造函数里先 <c>Show()</c> 再 <c>Hide()</c>
    /// 就是为了先把窗口句柄建出来，之后后台线程可以直接 Invoke 进来。</para>
    /// </summary>
    public partial class OptionChoiceForm : Form
    {
        /// <summary>用户的选择：Yes = 按钮1、Ignore = 按钮2、Abort = 按钮3。</summary>
        public DialogResult dResult = DialogResult.None;

        /// <summary>按钮尺寸（96 DPI 设计值，实际像素由 UiTheme.Dpi 换算）。</summary>
        private static readonly Size ButtonSize = new Size(116, 42);

        /// <summary>按钮之间的间距（同上）。</summary>
        private const int ButtonGap = 12;

        /// <summary>圆角半径（设计值）。</summary>
        private const int CornerRadius = 12;

        public OptionChoiceForm()
        {
            InitializeComponent();
            UiTheme.ApplyDpi(this);

            // 按需创建的窗体：登记进语言表并按当前语言设置文字（按钮文字随后由 fnChangeButtonsText 覆盖）
            MiddleLayer.RegisterAndApplyLanguage(this, this.Name);
            ControlBox = false;

            ApplyRoundRegion();
            panelButtons.Paint += panelButtons_Paint;             // 按钮区顶部 1px 分隔线
            panelButtons.Resize += delegate { RelayoutButtons(); };   // 布局完成/尺寸变化后重新居中
            RelayoutButtons();

            // 先建句柄再藏起来：这样别的线程可以直接 Invoke / BeginInvoke 操作本窗体
            Show();
            Hide();
        }

        #region 对外接口（签名与旧版一致，调用方不用改）

        /// <summary>设置三个按钮的文字（没传的按钮文字会被清空，与旧版一致）。</summary>
        public void fnChangeButtonsText(string sButton1Text, string sButton2Text = "", string sButton3Text = "")
        {
            RunOnUi(delegate
            {
                Button1.Text = sButton1Text ?? string.Empty;
                Button2.Text = sButton2Text ?? string.Empty;
                Button3.Text = sButton3Text ?? string.Empty;
            });
        }

        /// <summary>
        /// 设置消息文字与三个按钮的显示/隐藏，并把上次的选择清成 <see cref="DialogResult.None"/>。
        /// 隐藏的按钮不占位，可见按钮会自动重新居中。
        /// </summary>
        public void fnSetMessageAndButtons(string sMessage, bool bShowButton1, bool bShowButton2, bool bShowButton3)
        {
            RunOnUi(delegate
            {
                dResult = DialogResult.None;
                lblMessage.Text = sMessage ?? string.Empty;
                Button1.Visible = bShowButton1;
                Button2.Visible = bShowButton2;
                Button3.Visible = bShowButton3;
                RelayoutButtons();
            });
        }

        #endregion

        #region 按钮 / 布局

        private void btnChoice1_Click(object sender, EventArgs e) { Complete(DialogResult.Yes); }
        private void btnChoice2_Click(object sender, EventArgs e) { Complete(DialogResult.Ignore); }
        private void btnChoice3_Click(object sender, EventArgs e) { Complete(DialogResult.Abort); }

        /// <summary>记下选择并收起窗口（沿用旧的 Hide 语义，调用方读 dResult）。</summary>
        private void Complete(DialogResult result)
        {
            dResult = result;
            Hide();
        }

        /// <summary>
        /// 重新排布按钮：只算可见的那些，整体在按钮区里水平居中、垂直居中
        /// （所以"只有一个按钮"时它会落在正中，不会留下空档）。
        /// 同时按语义给按钮配色、把顶部色条调成第一个可见按钮的颜色。
        /// </summary>
        private void RelayoutButtons()
        {
            if (panelButtons == null || Button1 == null || Button2 == null || Button3 == null) return;

            List<FlatButton> visible = new List<FlatButton>();
            FlatButton[] all = new FlatButton[] { Button1, Button2, Button3 };
            for (int i = 0; i < all.Length; i++)
                if (all[i].Visible) visible.Add(all[i]);

            // 语义配色：1 确认（绿）/ 2 次要（白）/ 3 取消（红）；只留一个时升级为主操作（品牌蓝）
            Button1.Variant = FlatButtonVariant.Success;
            Button2.Variant = FlatButtonVariant.Surface;
            Button3.Variant = FlatButtonVariant.Danger;
            if (visible.Count == 1) visible[0].Variant = FlatButtonVariant.Primary;

            int w = UiTheme.Dpi(ButtonSize.Width);
            int h = UiTheme.Dpi(ButtonSize.Height);
            int gap = UiTheme.Dpi(ButtonGap);
            int total = visible.Count * w + Math.Max(0, visible.Count - 1) * gap;
            int x = (panelButtons.ClientSize.Width - total) / 2;
            int y = (panelButtons.ClientSize.Height - h) / 2;

            for (int i = 0; i < visible.Count; i++)
            {
                visible[i].Size = new Size(w, h);
                visible[i].Location = new Point(x, y);
                x += w + gap;
            }

            panelHeader.BackColor = (visible.Count > 0) ? VariantColor(visible[0].Variant) : UiKit.Line;
        }

        /// <summary>按钮变体对应的主色（与 FlatButton 的填充色同口径）。</summary>
        private static Color VariantColor(FlatButtonVariant variant)
        {
            switch (variant)
            {
                case FlatButtonVariant.Success: return UiKit.Success;
                case FlatButtonVariant.Danger: return UiKit.Danger;
                case FlatButtonVariant.Surface: return UiKit.LineStrong;
                default: return UiKit.Brand;
            }
        }

        /// <summary>按钮区顶部的 1px 分隔线。</summary>
        private void panelButtons_Paint(object sender, PaintEventArgs e)
        {
            Control c = (Control)sender;
            using (Pen p = new Pen(UiKit.Line, 1f))
                e.Graphics.DrawLine(p, 0, 0, c.Width, 0);
        }

        /// <summary>
        /// 窗口圆角（与主界面一致）。1px 描边不用自绘：窗体 Padding=1 + 底色是浅线色，
        /// 位于内层的三块白面板天然留出那一圈边线。
        /// </summary>
        private void ApplyRoundRegion()
        {
            try
            {
                int d = UiTheme.Dpi(CornerRadius) * 2;
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

        #endregion

        #region 线程安全

        /// <summary>没建句柄 / 已释放时安静地忽略，不让后台线程因为抛异常把业务打断。</summary>
        private void RunOnUi(Action action)
        {
            if (action == null || IsDisposed) return;
            try
            {
                if (InvokeRequired) Invoke(action);
                else action();
            }
            catch (Exception) { }
        }

        #endregion
    }
}
