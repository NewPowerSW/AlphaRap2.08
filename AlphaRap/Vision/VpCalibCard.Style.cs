using System;
using System.Drawing;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>标定卡外观：沿用 CardPanel，统一输入框与操作按钮。</summary>
    public partial class VpCalibCard
    {
        public void ApplyPageStyle()
        {
            BackColor = UiKit.PageBg;
            if (card != null)
            {
                card.FillColor = UiKit.Surface;
                card.BorderColor = UiKit.Line;
                card.ShowBorder = true;
                card.ShowShadow = true;
            }

            if (titleBar != null)
            {
                titleBar.BackColor = Color.Transparent;
                titleBar.Height = UiTheme.Dpi(30);
                titleBar.Resize -= OnTitleBarResize;
                titleBar.Resize += OnTitleBarResize;
            }

            UiTheme.StyleCaption(lblTitle, true);
            if (lblTitle != null) lblTitle.Font = UiTheme.Section;
            UiTheme.StyleCaption(lblTip, false);
            if (lblTip != null)
            {
                lblTip.ForeColor = UiKit.TextMuted;
                lblTip.AutoEllipsis = true;
                lblTip.TextAlign = ContentAlignment.MiddleRight;
            }
            UiTheme.StyleCaption(lblExpCaption, false);
            if (lblExpCaption != null) lblExpCaption.AutoSize = true;
            UiTheme.StyleTextBox(tbExposure);
            UiKit.StyleGrid(gridPoints);

            if (actionRow != null) actionRow.BackColor = Color.Transparent;

            VPForm.StyleActionButton(btnAddCalibration, FlatButtonVariant.Primary);
            VPForm.StyleActionButton(btnEditTb, FlatButtonVariant.Surface);
            VPForm.StyleActionButton(btnCapture, FlatButtonVariant.Primary);
            VPForm.StyleActionButton(btnAddPoint, FlatButtonVariant.Surface);
            VPForm.StyleActionButton(btnDeletePoint, FlatButtonVariant.Danger);
            VPForm.StyleActionButton(btnApplyToCamera, FlatButtonVariant.Primary);
            SizeActionButtons();
            LayoutTitlePath();
        }

        private void OnTitleBarResize(object sender, EventArgs e)
        {
            LayoutTitlePath();
        }

        private readonly ToolTip _calibPathTip = new ToolTip { ShowAlways = true, AutomaticDelay = 400 };

        /// <summary>路径贴标题右侧，最长约占标题栏一半，超出省略；完整路径靠 Tooltip。</summary>
        internal void LayoutTitlePath()
        {
            if (titleBar == null || lblTip == null) return;
            int pad = UiTheme.Dpi(12);
            int maxW = Math.Max(UiTheme.Dpi(140), titleBar.ClientSize.Width / 2);
            string full = lblTip.Text ?? "";
            int prefer = TextRenderer.MeasureText(full, lblTip.Font).Width + pad;
            lblTip.Width = Math.Min(Math.Max(prefer, UiTheme.Dpi(80)), maxW);
            lblTip.AutoEllipsis = true;
            try { _calibPathTip.SetToolTip(lblTip, full); } catch { }
        }

        private void SizeActionButtons()
        {
            if (btnAddCalibration != null) btnAddCalibration.Size = new Size(UiTheme.Dpi(132), UiTheme.Dpi(32));
            if (btnEditTb != null) btnEditTb.Size = new Size(UiTheme.Dpi(100), UiTheme.Dpi(32));
            if (btnCapture != null) btnCapture.Size = new Size(UiTheme.Dpi(96), UiTheme.Dpi(32));
            if (btnAddPoint != null) btnAddPoint.Size = new Size(UiTheme.Dpi(120), UiTheme.Dpi(32));
            if (btnDeletePoint != null) btnDeletePoint.Size = new Size(UiTheme.Dpi(120), UiTheme.Dpi(32));
            if (btnApplyToCamera != null) btnApplyToCamera.Size = new Size(UiTheme.Dpi(132), UiTheme.Dpi(32));
        }
    }
}
