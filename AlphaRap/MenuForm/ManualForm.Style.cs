using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>手动 IO 页的外观：扁平选项卡、IO 点按输入 / 输出分区自动排布。</summary>
    public partial class ManualForm
    {
        private void ApplyPageStyle()
        {
            BackColor = UiKit.PageBg;
            StyleTabs(this);
            IoPanelLayout.ArrangeAll(this);
        }

        /// <summary>语言切换后刷新分区标题和选项卡宽度。</summary>
        private void RefreshPageTexts()
        {
            IoPanelLayout.RefreshAllTexts();
            StyleTabs(this);
        }

        private void StyleTabs(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                TabControl tc = c as TabControl;
                if (tc != null) UiTheme.StyleTabControl(tc, tc == tabControl1);
                StyleTabs(c);
            }
        }
    }
}
