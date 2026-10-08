using System;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>首页外观：KPI 卡片、每小时产量图、生产信息卡片的文字与数值显示。</summary>
    public partial class HomeForm
    {
        private void ApplyPageStyle()
        {
            RefreshPageTexts();
        }

        /// <summary>按当前语言设置首页标题与说明文字（语言切换后再次调用）。</summary>
        private void RefreshPageTexts()
        {
            lblKpiInputTitle.Text = UiTheme.T("HomeForm", "kpi_Input", "投入", "Input", "Entrada");
            lblKpiOkTitle.Text = UiTheme.T("HomeForm", "kpi_Ok", "良品", "OK", "OK");
            lblKpiNgTitle.Text = UiTheme.T("HomeForm", "kpi_Ng", "不良", "NG", "NG");
            lblKpiYieldTitle.Text = UiTheme.T("HomeForm", "kpi_Yield", "良率", "Yield", "Rendimiento");
            lblKpiCtTitle.Text = UiTheme.T("HomeForm", "kpi_Ct", "节拍", "Cycle time", "Tiempo de ciclo");

            lblChartTitle.Text = UiTheme.T("HomeForm", "chart_Title", "今日每小时产量", "Hourly output today", "Producción por hora (hoy)");
            hourlyChart.OkLegend = UiTheme.T("HomeForm", "chart_Ok", "良品", "OK", "OK");
            hourlyChart.NgLegend = UiTheme.T("HomeForm", "chart_Ng", "不良", "NG", "NG");
            hourlyChart.EmptyText = UiTheme.T("HomeForm", "chart_Empty", "暂无数据", "No data yet", "Sin datos");
            hourlyChart.Invalidate();

            lblInfoTitle.Text = UiTheme.T("HomeForm", "info_Title", "生产信息", "Production info", "Información de producción");
            string barcode = UiTheme.T("HomeForm", "info_Barcode", "条码", "Barcode", "Código");
            lblBarcode1.Text = barcode + " 1";
            lblBarcode2.Text = barcode + " 2";
            lblBarcode3.Text = barcode + " 3";
            lblBarcode4.Text = barcode + " 4";

            lblHourTitle.Text = UiTheme.T("HomeForm", "hour_Title", "本小时", "This hour", "Esta hora");
            lblHourInput.Text = UiTheme.T("HomeForm", "hour_Input", "投入", "Input", "Entrada");
            lblHourOutput.Text = UiTheme.T("HomeForm", "hour_Output", "产出", "Output", "Salida");
            lblHourReject.Text = UiTheme.T("HomeForm", "hour_Reject", "不良", "NG", "NG");
            lblHourYield.Text = UiTheme.T("HomeForm", "hour_Yield", "良率", "Yield", "Rendimiento");
        }

        /// <summary>
        /// 显示生产数据。ok / ng 为累计数量；cycleTime 为节拍（秒），无效时显示 "--"；
        /// hourlyOk / hourlyNg 为当天 0–23 点每小时数量。
        /// </summary>
        internal void ShowValues(double ok, double ng, string cycleTime, double[] hourlyOk, double[] hourlyNg, int hour)
        {
            double input = ok + ng;
            SetText(lblKpiInputValue, input.ToString("0"));
            SetText(lblKpiOkValue, ok.ToString("0"));
            SetText(lblKpiNgValue, ng.ToString("0"));
            SetText(lblKpiYieldValue, input > 0 ? (ok * 100.0 / input).ToString("0.0") + "%" : "--");

            double ct;
            bool hasCt = double.TryParse(cycleTime, out ct) && ct > 0;
            SetText(lblKpiCtValue, hasCt ? ct.ToString("0.0") + " s" : "-- s");

            hourlyChart.SetData(hourlyOk, hourlyNg, hour);

            double hOk = (hourlyOk != null && hour >= 0 && hour < hourlyOk.Length) ? hourlyOk[hour] : 0;
            double hNg = (hourlyNg != null && hour >= 0 && hour < hourlyNg.Length) ? hourlyNg[hour] : 0;
            double hIn = hOk + hNg;
            SetText(tHourlyInput, hIn.ToString("0"));
            SetText(tHourlyOutput, hOk.ToString("0"));
            SetText(tHourlyReject, hNg.ToString("0"));
            SetText(tHourlyYield, hIn > 0 ? (hOk * 100.0 / hIn).ToString("0.0") + "%" : "--");
        }

        /// <summary>只在文字变化时赋值，避免定时刷新造成闪烁。</summary>
        private static void SetText(Control c, string text)
        {
            if (c.Text != text) c.Text = text;
        }
    }
}
