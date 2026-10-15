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
            if (paramRow != null)
            {
                paramRow.Resize -= OnParamRowResize;
                paramRow.Resize += OnParamRowResize;
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
            LayoutParamRow();
            LayoutKeepGroup();
            LayoutTitlePath();
        }

        private void OnHeadResize(object sender, EventArgs e)
        {
            LayoutTitlePath();
        }

        private void OnParamRowResize(object sender, EventArgs e)
        {
            LayoutParamRow();
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

        /// <summary>
        /// 曝光 / VPP 名 / 五个操作按钮同一行：等高、垂直居中、按钮按文案量宽（避免 Delete row 截成 Delete r...）。
        /// </summary>
        internal void LayoutParamRow()
        {
            if (paramRow == null) return;
            int h = UiTheme.Dpi(30);
            int gap = UiTheme.Dpi(8);
            int fieldGap = UiTheme.Dpi(16);

            FitActionButton(btnEditTb, h, 72);
            FitActionButton(btnCapture, h, 64);
            FitActionButton(btnSaveVpp, h, 80);
            FitActionButton(btnAddCompRow, h, 72);
            FitActionButton(btnDelCompRow, h, 72);

            if (tbExp != null) tbExp.Size = new Size(UiTheme.Dpi(64), h);
            if (tbRename != null) tbRename.Size = new Size(UiTheme.Dpi(100), h);

            // FlowLayoutPanel：用统一 Top margin 把标签/输入/按钮垂直对齐
            int rowInner = paramRow.ClientSize.Height > 0
                ? paramRow.ClientSize.Height - paramRow.Padding.Vertical
                : UiTheme.Dpi(36);
            AlignParamChild(lblExp, rowInner, 0, UiTheme.Dpi(6));
            AlignParamChild(tbExp, rowInner, 0, fieldGap);
            AlignParamChild(lblVppName, rowInner, 0, UiTheme.Dpi(6));
            AlignParamChild(tbRename, rowInner, 0, gap);
            AlignParamChild(btnEditTb, rowInner, gap, gap);
            AlignParamChild(btnCapture, rowInner, 0, gap);
            AlignParamChild(btnSaveVpp, rowInner, 0, gap);
            AlignParamChild(btnAddCompRow, rowInner, 0, gap);
            AlignParamChild(btnDelCompRow, rowInner, 0, 0);
        }

        private static void FitActionButton(FlatButton b, int h, int minWDesign)
        {
            if (b == null) return;
            int minW = UiTheme.Dpi(minWDesign);
            int pad = UiTheme.Dpi(22);
            Font font = b.Font ?? UiKit.Bold(10.5f);
            int tw = TextRenderer.MeasureText(b.Text ?? "", font,
                new Size(int.MaxValue, h),
                TextFormatFlags.NoPadding | TextFormatFlags.SingleLine).Width + pad;
            if (tw < minW) tw = minW;
            b.Size = new Size(tw, h);
        }

        private static void AlignParamChild(Control c, int rowInner, int leftExtra, int right)
        {
            if (c == null) return;
            int top = Math.Max(0, (rowInner - c.Height) / 2);
            c.Margin = new Padding(leftExtra, top, right, 0);
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
