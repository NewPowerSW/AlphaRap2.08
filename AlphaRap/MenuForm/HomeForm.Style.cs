using System;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>生产监控页的三语文案与实时指标显示。</summary>
    public partial class HomeForm
    {
        private double displayedOk, displayedNg, displayedHourOk, displayedHourNg;
        private string displayedCycleTime = "";

        /// <summary>使用现有语言入口初始化首页文案。</summary>
        private void ApplyPageStyle()
        {
            RefreshPageTexts();
        }

        /// <summary>刷新标题与动态文案，保留生产数据、列定义和设备名称。</summary>
        private void RefreshPageTexts()
        {
            lblKpiOkTitle.Text = UiTheme.T("HomeForm", "kpi_GoodOutput", "良品产出", "Good output", "Producción OK");
            lblKpiInputTitle.Text = UiTheme.T("HomeForm", "kpi_Input", "投入", "Input", "Entrada");
            lblKpiNgTitle.Text = UiTheme.T("HomeForm", "kpi_Ng", "不良", "NG", "NG");
            lblKpiCtTitle.Text = UiTheme.T("HomeForm", "kpi_Ct", "节拍", "Cycle time", "Tiempo de ciclo");
            lblKpiYieldTitle.Text = UiTheme.T("HomeForm", "kpi_Yield", "良率", "Yield", "Rendimiento");
            lblChartTitle.Text = UiTheme.T("HomeForm", "chart_TrendToday", "产量趋势 · 今日", "Output trend · Today", "Producción · Hoy");
            hourlyChart.OkLegend = UiTheme.T("HomeForm", "chart_Ok", "良品", "OK", "OK");
            hourlyChart.NgLegend = UiTheme.T("HomeForm", "chart_Ng", "不良", "NG", "NG");
            hourlyChart.EmptyText = UiTheme.T("HomeForm", "chart_Empty", "暂无数据", "No data yet", "Sin datos");
            hourlyChart.Invalidate();
            lblDeviceTitle.Text = UiTheme.T("HomeForm", "lblDeviceTitle", "设备状态", "Devices", "Dispositivos");
            lblDeviceEmpty.Text = UiTheme.T("HomeForm", "lblDeviceEmpty", "尚未配置设备", "No devices configured", "Sin dispositivos configurados");
            lblInfoTitle.Text = UiTheme.T("HomeForm", "records_Title", "生产记录", "Production records", "Registros de producción");
            btnInfoQuery.Text = UiTheme.T("HomeForm", "info_Query", "查询", "Query", "Consultar");
            RefreshKpiTexts();
            RefreshRecordCount();
            RestoreInfoHeaders();
            RefreshDeviceTexts();
        }

        /// <summary>显示累计数量、节拍及当天每小时产量；不修改生产记录和配置。</summary>
        internal void ShowValues(double ok, double ng, string cycleTime, double[] hourlyOk, double[] hourlyNg, int hour)
        {
            displayedOk = ok;
            displayedNg = ng;
            displayedCycleTime = cycleTime;
            displayedHourOk = hourlyOk != null && hour >= 0 && hour < hourlyOk.Length ? hourlyOk[hour] : 0;
            displayedHourNg = hourlyNg != null && hour >= 0 && hour < hourlyNg.Length ? hourlyNg[hour] : 0;
            RefreshKpiTexts();
            hourlyChart.SetData(hourlyOk, hourlyNg, hour);
        }

        /// <summary>从最近一次指标快照刷新数值，语言切换不会把数值替换为语言表占位符。</summary>
        private void RefreshKpiTexts()
        {
            double input = displayedOk + displayedNg;
            double hourlyInput = displayedHourOk + displayedHourNg;
            SetText(lblKpiOkValue, displayedOk.ToString("N0"));
            SetText(lblKpiInputValue, input.ToString("N0"));
            SetText(lblKpiNgValue, displayedNg.ToString("N0"));
            SetText(lblKpiYieldValue, input > 0 ? (displayedOk * 100.0 / input).ToString("0.0") + "%" : "--");
            double cycle;
            bool hasCycle = double.TryParse(displayedCycleTime, out cycle) && cycle > 0 && !double.IsInfinity(cycle);
            SetText(lblKpiCtValue, hasCycle ? cycle.ToString("0.0") + " s" : "-- s");
            string hourFormat = UiTheme.T("HomeForm", "kpi_ThisHourValue", "本小时 {0}", "This hour {0}", "Esta hora {0}");
            SetText(lblKpiOkHint, string.Format(hourFormat, displayedHourOk.ToString("N0")));
            SetText(lblKpiInputHint, string.Format(hourFormat, hourlyInput.ToString("N0")));
            SetText(lblKpiNgHint, string.Format(hourFormat, displayedHourNg.ToString("N0")));
            SetText(lblKpiYieldHint, string.Format(hourFormat, hourlyInput > 0 ? (displayedHourOk * 100.0 / hourlyInput).ToString("0.0") + "%" : "--"));
            SetText(lblKpiCtHint, UiTheme.T("HomeForm", "kpi_CurrentCycle", "当前生产节拍", "Current cycle", "Ciclo actual"));
        }

        /// <summary>显示当前页面记录数，不把显示行数当作生产计数。</summary>
        private void RefreshRecordCount()
        {
            if (lblInfoCount == null || grdInfo == null) return;
            SetText(lblInfoCount, string.Format(UiTheme.T("HomeForm", "records_Count", "{0} 条记录", "{0} records", "{0} registros"), grdInfo.Rows.Count));
        }

        /// <summary>只在文字变化时赋值，避免每秒刷新导致闪烁。</summary>
        private static void SetText(Control control, string text)
        {
            if (control.Text != text) control.Text = text;
        }
    }
}
