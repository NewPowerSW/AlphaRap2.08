using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>流程工位窗体（Gantry / 上下流道）共用的 Tab 刷样式入口。</summary>
    internal static class FlowStationUi
    {
        public static void Apply(Form station)
        {
            if (station == null) return;
            station.BackColor = UiKit.PageBg;
            UiTheme.StyleDescendantTabControls(station, null);
        }
    }
}