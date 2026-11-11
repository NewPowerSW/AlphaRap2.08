using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using AlphaRap;
using NPSDK;

internal static partial class RuntimeLogProbe
{
    private const string FlowPrefix = "FlowChart [Module=AlphaRap.TaskProcess.DownConveryor/DownConveryor, Entry=93f2e6c0e48d440c8f668ea6048d8f1e, Run=43465fe35ead43c08bde432e1dab17a1] ";
    private const string FlowNode = "Node=下流道阻挡上升1.3 [613205fab6794e3d909008e00c0307f6] ";

    /// <summary>Keep the real timeout cause visible while preserving raw errors, routes and historic messages.</summary>
    private static void ReadableFlowLogs()
    {
        string raw = FlowPrefix + FlowNode + "System.InvalidOperationException: 下流道阻挡上升1.3: Timeout after 5000 ms ---> System.OperationCanceledException: 已取消该操作。\r\n   在 System.Threading.CancellationToken.ThrowOperationCanceledException()";
        RuntimeLogRecord timeout = new RuntimeLogRecord { Time = DateTime.Today.AddHours(10), Level = RuntimeLogHistory.Error, Content = raw };
        Check(timeout.Station == "DownConveryor" && timeout.Node == "下流道阻挡上升1.3", "Flow identity did not separate station and node.");
        RuntimeLogRecord bracket = new RuntimeLogRecord { Time = timeout.Time.AddSeconds(-2), Level = RuntimeLogHistory.Run, Content = FlowPrefix + "Node=等待 [入料] 信号 [613205fa-b679-4e3d-9090-08e00c0307f6] Waiting" };
        Check(bracket.Node == "等待 [入料] 信号", "Node captions containing brackets or spaces were truncated.");
        bracket.Content = "Unrecognized machine message\r\nsecond line";
        Check(bracket.Node == "" && bracket.Station == "", "Changing raw content left stale parsed fields.");
        RuntimeLogResult records = new RuntimeLogResult();
        records.Records.Add(timeout);
        foreach (string state in new[] { "RecoveryPaused", "Timeout", "Paused", "Waiting", "Running", "Exit=NEXT, Next=7f1d7f9888d348f2b83475d5a5196e5e", "Completed", "Finished: Cancelled" })
            records.Records.Add(new RuntimeLogRecord { Time = timeout.Time.AddMilliseconds(-records.Records.Count * 10), Level = RuntimeLogHistory.Run, Content = FlowPrefix + FlowNode + state });
        records.Records.Add(new RuntimeLogRecord { Time = timeout.Time.AddSeconds(-1), Level = RuntimeLogHistory.Run, Content = "Gantry+++++++++++++++Pause" });
        records.Records.Add(bracket);
        string export = Path.Combine(output, "readable-flow-original.csv");
        RuntimeLogHistory.Export(records.Records, export);
        using (Microsoft.VisualBasic.FileIO.TextFieldParser parser = new Microsoft.VisualBasic.FileIO.TextFieldParser(export, System.Text.Encoding.UTF8))
        {
            parser.SetDelimiters(","); parser.HasFieldsEnclosedInQuotes = true; parser.ReadFields();
            Check(parser.ReadFields()[2] == raw, "Presentation changed exported raw stack or identifiers.");
        }
        foreach (Size size in new[] { new Size(1048, 708), new Size(1368, 908) })
            ReadableFlowLayout(size, records, raw);
    }

    /// <summary>Render readable and original details across all languages without rebuilding the bound records.</summary>
    private static void ReadableFlowLayout(Size size, RuntimeLogResult records, string raw)
    {
        LicenseContext previous = LicenseManager.CurrentContext;
        Func<string, string, string, string, string, string> provider = UiTheme.TextProvider;
        try
        {
            LicenseManager.CurrentContext = new DesignerContext(); UiTheme.ForceDesignTimeForTest(true);
            using (RuntimeLogForm page = new RuntimeLogForm())
            {
                page.ClientSize = size; page.Show(); Application.DoEvents();
                Invoke(page, "BindResult", records);
                DataGridView grid = Field<DataGridView>(page, "gridLogs");
                TabControl tabs = Field<TabControl>(page, "detailTabs");
                TextBox summary = Field<TextBox>(page, "summaryText");
                TextBox original = Field<TextBox>(page, "detailText");
                Check(tabs.SelectedIndex == 0 && summary.WordWrap && summary.Font.Size >= 10 && summary.ReadOnly,
                    "Readable details are not the default or cannot wrap.");
                Field<TextBox>(page, "keywordFilter").Text = "keep input";
                object bound = grid.DataSource;
                for (int language = 0; language < 3; language++)
                {
                    UiTheme.TextProvider = (owner, key, zh, en, es) => language == 0 ? zh : language == 1 ? en : es;
                    UiTheme.ForceDesignTimeForTest(false); LicenseManager.CurrentContext = previous;
                    Invoke(page, "LanguageChanged", null, EventArgs.Empty); Layout(page);
                    string text = Convert.ToString(grid.Rows[0].Cells["colContent"].FormattedValue);
                    Check(text.Contains("5") && !text.Contains("Exception") && !text.Contains("FlowChart") && !text.Contains("取消"),
                        "Timeout cause was obscured by the inner cancellation: " + text);
                    Check(summary.Text.Contains(text) && summary.Text.Contains("下流道阻挡上升1.3") && original.Text == raw,
                        "Details lost either the readable cause or original stack.");
                    Check(grid.Columns.Count == 5 && grid.Columns["colNode"].HeaderText == (language == 0 ? "节点" : language == 1 ? "Node" : "Nodo"), "Missing translated node column.");
                    Check(ReferenceEquals(bound, grid.DataSource) && Field<TextBox>(page, "keywordFilter").Text == "keep input",
                        "Language refresh replaced data or input.");
                    Check(Field<SplitContainer>(page, "detailSplit").Panel2.Height >= 150 && summary.Height >= 90,
                        "Detail area is too small to read.");
                    Check(grid.Columns.Cast<DataGridViewColumn>().Sum(column => column.Width) <= grid.ClientSize.Width,
                        "The five columns require horizontal scrolling at " + size);
                    foreach (string column in new[] { "colTime", "colLevel" })
                        Check(TextRenderer.MeasureText(Convert.ToString(grid.Rows[0].Cells[column].FormattedValue), grid.DefaultCellStyle.Font).Width + grid.DefaultCellStyle.Padding.Horizontal + 2 <= grid.Columns[column].Width,
                            "Timestamp or translated level is clipped: " + column);
                    original.Select(5, 8);
                    tabs.SelectedIndex = 1;
                    Invoke(page, "BindResult", records);
                    bound = grid.DataSource;
                    Check(tabs.SelectedIndex == 1 && original.Text == raw, "Auto refresh reset the raw tab or selected record.");
                    Check(original.SelectionStart == 5 && original.SelectionLength == 8, "Auto refresh disturbed reading the same raw detail.");
                    tabs.SelectedIndex = 0;
                    grid.Focus();
                    using (Bitmap image = new Bitmap(page.Width, page.Height))
                    {
                        page.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
                        image.Save(Path.Combine(output, "readable-flow-" + size.Width + "-" + language + ".png"));
                    }
                }
                grid.CurrentCell = grid.Rows[records.Records.Count - 2].Cells[0];
                Check(!Field<TextBox>(page, "summaryText").Text.Contains("++++"), "Legacy pause decorations remain visible.");
                grid.CurrentCell = grid.Rows[records.Records.Count - 1].Cells[0];
                Check(summary.Text.Contains("Unrecognized machine message\r\nsecond line"), "Unknown multiline machine content was hidden.");
            }
        }
        finally { UiTheme.TextProvider = provider; UiTheme.ForceDesignTimeForTest(false); LicenseManager.CurrentContext = previous; }
    }

    private sealed class PauseLogProbe : Flow_BaseForm
    {
        internal int Requests;
        /// <summary>Count pause callbacks without invoking any devices.</summary>
        public override void PauseRun() { Requests++; }
    }

    /// <summary>Log a pause transition once while continuing to dispatch every requested pause.</summary>
    private static void PauseLogTransitions()
    {
        bool previousPause = Flow_Module.AllPause;
        List<Flow_BaseForm> modules = Flow_Module.FlowChart_ModuleList.ToList();
        List<string> priorLogs = new List<string>();
        string entry;
        while ((entry = Flow_Module.Module_GetRunLog()) != null) priorLogs.Add(entry);
        try
        {
            Flow_Module.FlowChart_ModuleList.Clear();
            using (PauseLogProbe module = new PauseLogProbe { Name = "OfflinePause" })
            {
                if (!Flow_Module.FlowChart_ModuleList.Contains(module)) Flow_Module.FlowChart_ModuleList.Add(module);
                Flow_Module.Module_PauseRun(); Flow_Module.Module_PauseRun();
                Check(module.Requests == 2 && module.PauseStatus, "Repeated pause no longer reached the module.");
                Check(Flow_Module.Module_GetRunLog() == "OfflinePause+++++++++++++++Pause" && Flow_Module.Module_GetRunLog() == null,
                    "Repeated pause requests produced duplicate transition logs.");
                module.PauseStatus = false; Flow_Module.Module_PauseRun();
                Check(module.Requests == 3 && Flow_Module.Module_GetRunLog() == "OfflinePause+++++++++++++++Pause",
                    "A new pause after resume was not logged.");
            }
        }
        finally
        {
            Flow_Module.FlowChart_ModuleList.Clear(); Flow_Module.FlowChart_ModuleList.AddRange(modules);
            Flow_Module.AllPause = previousPause;
            while (Flow_Module.Module_GetRunLog() != null) { }
            foreach (string log in priorLogs) Flow_Module.Module_AddRunLog(log);
        }
    }
}
