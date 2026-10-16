using System.Drawing;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>相机页外观：属性条白底、输入框 / 按钮 / 子 Tab 套 UiKit。</summary>
    public partial class VpCameraPage
    {
        public void ApplyPageStyle()
        {
            BackColor = UiKit.PageBg;
            if (head != null)
            {
                head.BackColor = UiKit.Surface;
                head.Padding = new Padding(UiTheme.Dpi(12), UiTheme.Dpi(10), UiTheme.Dpi(12), UiTheme.Dpi(8));
            }
            if (calibHost != null)
            {
                calibHost.BackColor = UiKit.PageBg;
                calibHost.Padding = new Padding(UiTheme.Dpi(10), UiTheme.Dpi(6), UiTheme.Dpi(10), UiTheme.Dpi(8));
                calibHost.Height = UiTheme.Dpi(212);
            }
            if (dispHost != null)
            {
                dispHost.BackColor = UiKit.Surface;
                dispHost.Padding = new Padding(UiTheme.Dpi(8));
                dispHost.Width = UiTheme.Dpi(400);
            }

            UiTheme.StyleCaption(lblCapCameraName, false);
            UiTheme.StyleCaption(lblCapCameraIndex, false);
            UiTheme.StyleCaption(lblCapDefaultExposure, false);
            if (lblCapCameraName != null) lblCapCameraName.AutoSize = true;
            if (lblCapCameraIndex != null) lblCapCameraIndex.AutoSize = true;
            if (lblCapDefaultExposure != null) lblCapDefaultExposure.AutoSize = true;

            UiTheme.StyleTextBox(tbName);
            UiTheme.StyleTextBox(tbIndex);
            UiTheme.StyleTextBox(tbExposure);

            VPForm.StyleActionButton(btnLive, FlatButtonVariant.Primary);
            if (btnLive != null) btnLive.Size = new Size(UiTheme.Dpi(120), UiTheme.Dpi(32));

            if (vppTabs != null)
                UiTheme.StyleTabControl(vppTabs, false);

            StyleChildVppPages();
            StyleCalibCards();
        }

        private void StyleChildVppPages()
        {
            if (vppTabs == null) return;
            foreach (TabPage page in vppTabs.TabPages)
            {
                page.BackColor = UiKit.Surface;
                page.Padding = new Padding(UiTheme.Dpi(4));
                page.UseVisualStyleBackColor = false;
                foreach (Control c in page.Controls)
                {
                    VpVppPage vpp = c as VpVppPage;
                    if (vpp != null) vpp.ApplyPageStyle();
                }
            }
        }

        private void StyleCalibCards()
        {
            if (calibHost == null) return;
            foreach (Control c in calibHost.Controls)
            {
                VpCalibCard card = c as VpCalibCard;
                if (card != null) card.ApplyPageStyle();
            }
        }
    }
}
