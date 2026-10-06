using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace AlphaRap
{
    /// <summary>
    /// 不参与焦点切换的 Chart。
    ///
    /// 本工程里的图表（温度 / 压力曲线）只用于显示，不需要键盘交互。
    /// 但标准的 Chart 被鼠标点一下就会获得焦点，并在整个控件四周画出虚线焦点框；
    /// 而承载它的 panel1 / plMainShow / 窗体本身都不可获焦，点到空白处时
    /// 焦点无处可去，那条虚线框就会一直残留、点别的地方也不会消失。
    ///
    /// 把 ControlStyles.Selectable 关掉之后，鼠标点击与 Tab 都不会再把焦点交给它，
    /// 焦点框自然不会出现；鼠标事件、ToolTip、曲线刷新都不受影响。
    /// </summary>
    public class NoFocusChart : Chart
    {
        public NoFocusChart()
        {
            // 不可获焦：点击与 Tab 都不再抢焦点
            SetStyle(ControlStyles.Selectable, false);
            TabStop = false;
        }
    }
}
