using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>
    /// 流程页外观：站位一级 Tab + 各工位 FlowChartTable / 内层 Tab 统一扁平样式。
    /// 设计器：占位页 + Font/ItemSize 已写进 Designer（OwnerDraw 设计期不触发，故 DrawMode=Normal）。
    /// 运行时：StyleTabControl 改 OwnerDraw（白底选中 + 品牌色下划线）。
    /// </summary>
    public partial class Flow
    {
        private void ApplyPageStyle()
        {
            BackColor = UiKit.PageBg;
            if (tabControl1 != null)
            {
                tabControl1.BackColor = UiKit.PageBg;
                UiTheme.StyleTabControl(tabControl1, true);
            }
            UiTheme.StyleDescendantTabControls(this, tabControl1);
        }

        private void RefreshPageStyle()
        {
            ApplyPageStyle();
        }
    }
}