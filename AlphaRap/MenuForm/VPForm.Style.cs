using System.Drawing;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>视觉页外观：与首页同一套 UiKit / FlatButton / 扁平 Tab。</summary>
    public partial class VPForm
    {
        private void ApplyPageStyle()
        {
            BackColor = UiKit.PageBg;
            if (tabControl1 != null)
                UiTheme.StyleTabControl(tabControl1, true);
            StyleVpToolBar();
            StyleAllCameraPages();
        }

        private void StyleVpToolBar()
        {
            if (_vpBar == null) return;
            _vpBar.BackColor = UiKit.Surface;
            _vpBar.Padding = new Padding(UiTheme.Dpi(4), 0, UiTheme.Dpi(4), 0);
            foreach (Control c in _vpBar.Controls)
            {
                FlowLayoutPanel flow = c as FlowLayoutPanel;
                if (flow != null)
                {
                    flow.BackColor = UiKit.Surface;
                    continue;
                }
                Label hint = c as Label;
                if (hint != null)
                {
                    hint.Font = UiTheme.Caption;
                    hint.ForeColor = UiKit.TextMuted;
                }
            }
        }

        private void StyleAllCameraPages()
        {
            if (tabControl1 == null) return;
            foreach (TabPage page in tabControl1.TabPages)
            {
                page.BackColor = UiKit.PageBg;
                page.UseVisualStyleBackColor = false;
                foreach (Control c in page.Controls)
                {
                    VpCameraPage camPage = c as VpCameraPage;
                    if (camPage != null) camPage.ApplyPageStyle();
                }
            }
            UiTheme.StyleTabControl(tabControl1, true);
        }

        /// <summary>工具条按钮：白底描边圆角。</summary>
        internal static FlatButton CreateBarFlatButton(string key, string text)
        {
            FlatButton b = new FlatButton();
            b.Name = key;
            b.Text = text;
            b.Variant = FlatButtonVariant.Surface;
            b.CornerRadius = 8;
            b.Font = UiKit.Bold(10.5f);
            b.Size = new Size(UiTheme.Dpi(130), UiTheme.Dpi(32));
            b.Margin = new Padding(UiTheme.Dpi(2), 0, UiTheme.Dpi(6), 0);
            b.Cursor = Cursors.Hand;
            return b;
        }

        /// <summary>操作区按钮统一尺寸与圆角。</summary>
        internal static void StyleActionButton(FlatButton b, FlatButtonVariant variant)
        {
            if (b == null) return;
            b.Variant = variant;
            b.CornerRadius = 8;
            b.Font = UiKit.Bold(10.5f);
            b.Cursor = Cursors.Hand;
            if (b.Height < UiTheme.Dpi(30))
                b.Height = UiTheme.Dpi(32);
        }
    }
}
