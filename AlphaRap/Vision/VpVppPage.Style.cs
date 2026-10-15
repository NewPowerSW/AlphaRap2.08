using System;
using System.Drawing;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>VPP 子页外观：白底分区、扁平表格、圆角操作按钮。</summary>
    public partial class VpVppPage
    {
        public void ApplyPageStyle()
        {
            BackColor = UiKit.Surface;
            StylePanel(head);
            StylePanel(compRow);
            StylePanel(pathRow);
            StylePanel(paramRow);
            StylePanel(keepGroup);

            if (head != null)
            {
                head.Resize -= OnHeadResize;
                head.Resize += OnHeadResize;
            }

            UiTheme.StyleCaption(lblInfo, true);
            if (lblInfo != null) lblInfo.Font = UiTheme.Section;
            UiTheme.StyleCaption(path, false);
            if (path != null)
            {
                path.ForeColor = UiKit.TextMuted;
                path.AutoEllipsis = true;
                path.TextAlign = ContentAlignment.MiddleRight;
            }

            UiTheme.StyleCaption(lblSavePath, false);
            UiTheme.StyleCaption(lblSaveDays, false);
            UiTheme.StyleCaption(lblExp, false);
            UiTheme.StyleCaption(lblVppName, false);
            StyleCheck(cbSaveImg);
            StyleCheck(cbOrig);
            StyleCheck(cbSnap);
            StyleCheck(cbAutoDel);

            UiTheme.StyleTextBox(tbSavePath);
            UiTheme.StyleTextBox(tbSaveDays);
            UiTheme.StyleTextBox(tbExp);
            UiTheme.StyleTextBox(tbRename);

            UiKit.StyleGrid(gridComp);

            VPForm.StyleActionButton(btnBrowsePath, FlatButtonVariant.Surface);
            if (btnBrowsePath != null)
                btnBrowsePath.Size = new Size(UiTheme.Dpi(40), UiTheme.Dpi(28));

            VPForm.StyleActionButton(btnEditTb, FlatButtonVariant.Surface);
            VPForm.StyleActionButton(btnCapture, FlatButtonVariant.Primary);
            VPForm.StyleActionButton(btnSaveVpp, FlatButtonVariant.Primary);
            VPForm.StyleActionButton(btnAddCompRow, FlatButtonVariant.Surface);
            VPForm.StyleActionButton(btnDelCompRow, FlatButtonVariant.Danger);
            SizeActionButtons();
            LayoutKeepGroup();
            LayoutTitlePath();
        }

        private void OnHeadResize(object sender, EventArgs e)
        {
            LayoutTitlePath();
        }

        /// <summary>vpp 路径贴标题右侧，最长约占标题栏一半，超出省略。</summary>
        internal void LayoutTitlePath()
        {
            if (head == null || path == null) return;
            int pad = UiTheme.Dpi(12);
            int usable = head.ClientSize.Width - head.Padding.Horizontal;
            int maxW = Math.Max(UiTheme.Dpi(140), usable / 2);
            int prefer = TextRenderer.MeasureText(path.Text ?? "", path.Font).Width + pad;
            path.Width = Math.Min(Math.Max(prefer, UiTheme.Dpi(80)), maxW);
            path.Height = head.ClientSize.Height;
        }

        private void SizeActionButtons()
        {
            // 与曝光/VPP 名称同一行，略缩小以免挤出可视区
            int h = UiTheme.Dpi(30);
            if (btnEditTb != null) btnEditTb.Size = new Size(UiTheme.Dpi(88), h);
            if (btnCapture != null) btnCapture.Size = new Size(UiTheme.Dpi(72), h);
            if (btnSaveVpp != null) btnSaveVpp.Size = new Size(UiTheme.Dpi(96), h);
            if (btnAddCompRow != null) btnAddCompRow.Size = new Size(UiTheme.Dpi(80), h);
            if (btnDelCompRow != null) btnDelCompRow.Size = new Size(UiTheme.Dpi(80), h);
        }

        /// <summary>按标签宽度排布「保留天数」组，保证标签与输入框始终同一行。</summary>
        internal void LayoutKeepGroup()
        {
            if (keepGroup == null || lblSaveDays == null || tbSaveDays == null) return;
            int labelW = TextRenderer.MeasureText(lblSaveDays.Text ?? "", lblSaveDays.Font).Width + 4;
            if (labelW < UiTheme.Dpi(48)) labelW = UiTheme.Dpi(48);
            lblSaveDays.Location = new Point(0, UiTheme.Dpi(5));
            tbSaveDays.Location = new Point(labelW + UiTheme.Dpi(6), UiTheme.Dpi(2));
            keepGroup.Width = tbSaveDays.Right + UiTheme.Dpi(2);
            keepGroup.Height = UiTheme.Dpi(30);
        }

        private static void StylePanel(Control c)
        {
            if (c == null) return;
            c.BackColor = UiKit.Surface;
        }

        private static void StyleCheck(UiCheckBox box)
        {
            if (box == null) return;
            box.AutoSize = true;
            box.Font = UiTheme.Body;
            box.ForeColor = UiKit.TextSecondary;
            box.BackColor = Color.Transparent;
            box.Cursor = Cursors.Hand;
        }
    }
}
