using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlphaRap;
using DataSave;
using Microsoft.VisualBasic.FileIO;

internal static class RuntimeLogProbe
{
    private static string output;
    private static string runRoot, errorRoot;
    private static int passed, failed;
    private const string OriginalContent = "Station ready, \"part\"\r\nCycle complete";

    private sealed class DesignerContext : LicenseContext
    {
        public override LicenseUsageMode UsageMode { get { return LicenseUsageMode.Designtime; } }
    }

    /// <summary>Run isolated file and window checks without invoking machine startup.</summary>
    [STAThread]
    private static int Main(string[] args)
    {
        string assemblies = Path.GetFullPath(args[0]);
        AppDomain.CurrentDomain.AssemblyResolve += (sender, request) =>
        {
            string name = new AssemblyName(request.Name).Name;
            foreach (string root in new[] { assemblies, @"C:\Program Files\Cognex\VisionPro\ReferencedAssemblies" })
                foreach (string extension in new[] { ".dll", ".exe" })
                    if (File.Exists(Path.Combine(root, name + extension))) return Assembly.LoadFrom(Path.Combine(root, name + extension));
            return null;
        };
        output = Path.GetFullPath(args[1]);
        Environment.CurrentDirectory = output;
        Application.EnableVisualStyles();
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
        Test("existing runtime and error writers round-trip original CSV fields", WriterQuery);
        Test("date, level and keyword filters preserve duplicate timestamps", FilterQuery);
        Test("latest-record limit and cancellation are explicit", LimitsAndCancellation);
        Test("locked and malformed log files report partial results", PartialResults);
        Test("CSV export retains quotes, commas and newlines", ExportRoundTrip);
        foreach (Size size in new[] { new Size(1048, 708), new Size(1368, 908) })
            for (int language = 0; language < 3; language++)
            {
                Size currentSize = size; int currentLanguage = language;
                Test(size + " language=" + language, () => PageLayout(currentSize, currentLanguage));
            }
        Test("actual asynchronous refresh and cancellation use the existing source", AsyncPage);
        Console.WriteLine("RESULT: " + passed + "/" + (passed + failed) + " passed");
        Console.WriteLine("No machine Load, motion, IO write or camera capture was executed.");
        return failed == 0 ? 0 : 1;
    }

    /// <summary>Use the same two schemas and writer as DataForm.</summary>
    private static void WriterQuery()
    {
        runRoot = Path.Combine(output, "run"); errorRoot = Path.Combine(output, "error");
        using (TypeCollection1 run = Writer(runRoot, "Value"))
        using (TypeCollection1 error = Writer(errorRoot, "Content"))
        {
            run.SaveDataToFile(new[] { OriginalContent });
            run.SaveDataToFile(new[] { "Another station: waiting" });
            error.SaveDataToFile(new[] { "Camera connection failed" });
        }
        RuntimeLogResult found = Query(RuntimeLogHistory.All, "");
        Check(found.Records.Count == 3 && found.FileCount == 2 && found.FailedDays == 0, "Existing writers were not read: " + found.Records.Count);
        Check(found.Records.Any(record => record.Content == OriginalContent), "Original CSV content was changed.");
        Check(found.Records.Count(record => record.Level == RuntimeLogHistory.Error) == 1, "Error source identity was lost.");
        Check(found.Records.Select(record => record.Time).SequenceEqual(found.Records.OrderByDescending(record => record.Time).Select(record => record.Time)), "Records are not newest first.");
    }

    /// <summary>Build an isolated instance of the existing CSV writer.</summary>
    private static TypeCollection1 Writer(string path, string field)
    {
        TypeCollection1 writer = new TypeCollection1 { SavePath = path };
        writer.Items.Add(new DataFormat { Name = "Time" }); writer.Items.Add(new DataFormat { Name = field });
        return writer;
    }

    /// <summary>Query today's existing fixture logs.</summary>
    private static RuntimeLogResult Query(string level, string keyword)
    {
        return RuntimeLogHistory.Query(runRoot, errorRoot, DateTime.Today, DateTime.Today, level, keyword, CancellationToken.None);
    }

    /// <summary>Verify that filters use business codes and exact day boundaries.</summary>
    private static void FilterQuery()
    {
        Check(Query(RuntimeLogHistory.Run, "").Records.Count == 2, "Run-only filter includes errors.");
        Check(Query(RuntimeLogHistory.Error, "CAMERA").Records.Count == 1, "Keyword matching is not case-insensitive.");
        Check(Query(RuntimeLogHistory.All, "missing-keyword").Records.Count == 0, "Keyword filter retained unrelated rows.");
        Check(RuntimeLogHistory.Query(runRoot, errorRoot, DateTime.Today.AddDays(-1), DateTime.Today.AddDays(-1), RuntimeLogHistory.All, "", CancellationToken.None).Records.Count == 0,
            "A date query included the next day.");
        string duplicateRoot = Path.Combine(output, "duplicates");
        WriteFile(duplicateRoot, DateTime.Today, new[] { "Time,Value", Stamp(DateTime.Today.AddHours(1)) + ",Same", Stamp(DateTime.Today.AddHours(1)) + ",Same" });
        Check(RuntimeLogHistory.Query(duplicateRoot, "", DateTime.Today, DateTime.Today, RuntimeLogHistory.All, "", CancellationToken.None).Records.Count == 2,
            "Duplicate timestamps were dropped.");
    }

    /// <summary>Keep only the latest records and stop cancelled or invalid queries.</summary>
    private static void LimitsAndCancellation()
    {
        string root = Path.Combine(output, "many");
        List<string> lines = new List<string> { "Time,Value" };
        for (int index = 0; index < RuntimeLogHistory.MaxRows + 3; index++) lines.Add(Stamp(DateTime.Today.AddMilliseconds(index)) + ",Record " + index);
        WriteFile(root, DateTime.Today, lines);
        RuntimeLogResult found = RuntimeLogHistory.Query(root, "", DateTime.Today, DateTime.Today, RuntimeLogHistory.All, "", CancellationToken.None);
        Check(found.Records.Count == RuntimeLogHistory.MaxRows && found.MatchedCount == RuntimeLogHistory.MaxRows + 3, "The display limit silently discarded its count.");
        Check(found.Records.First().Content == "Record 10002" && found.Records.Last().Content == "Record 3", "The limit kept older records.");
        using (CancellationTokenSource cancellation = new CancellationTokenSource())
        {
            cancellation.Cancel(); bool stopped = false;
            try { RuntimeLogHistory.Query(root, "", DateTime.Today, DateTime.Today, RuntimeLogHistory.All, "", cancellation.Token); }
            catch (OperationCanceledException) { stopped = true; }
            Check(stopped, "Cancelled query continued.");
        }
        bool invalid = false;
        try { RuntimeLogHistory.Query(root, "", DateTime.Today, DateTime.Today.AddDays(-1), RuntimeLogHistory.All, "", CancellationToken.None); }
        catch (ArgumentException) { invalid = true; }
        Check(invalid, "A reversed date range was silently accepted.");
        string badRoot = Path.Combine(output, "bad");
        WriteFile(badRoot, DateTime.Today, new[] { "Time,WrongColumn", Stamp(DateTime.Today) + ",Bad" });
        Check(RuntimeLogHistory.Query(badRoot, "", DateTime.Today, DateTime.Today, RuntimeLogHistory.All, "", CancellationToken.None).FailedDays == 1,
            "A malformed log schema was reported as complete.");
    }

    /// <summary>Read exported CSV through a separate standard parser.</summary>
    private static void ExportRoundTrip()
    {
        string path = Path.Combine(output, "export.csv");
        RuntimeLogHistory.Export(Query(RuntimeLogHistory.All, "").Records, path);
        byte[] bytes = File.ReadAllBytes(path);
        Check(bytes.Take(3).SequenceEqual(new byte[] { 239, 187, 191 }), "CSV has no UTF-8 BOM.");
        using (TextFieldParser parser = new TextFieldParser(path, Encoding.UTF8))
        {
            parser.SetDelimiters(","); parser.HasFieldsEnclosedInQuotes = true;
            Check(parser.ReadFields().SequenceEqual(new[] { "Time", "Level", "Content" }), "Export business keys changed.");
            List<string[]> rows = new List<string[]>(); while (!parser.EndOfData) rows.Add(parser.ReadFields());
            Check(rows.Count == 3 && rows.Any(row => row[2] == OriginalContent), "Export lost original content.");
        }
    }

    /// <summary>Keep readable sources when one source is locked, and distinguish invalid rows from an empty query.</summary>
    private static void PartialResults()
    {
        string runtimeFile = Directory.GetFiles(Path.Combine(runRoot, DateTime.Today.ToString("yyyy"), DateTime.Today.ToString("MM")), "*.csv").Single();
        byte[] original = File.ReadAllBytes(runtimeFile);
        using (FileStream locked = new FileStream(runtimeFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            RuntimeLogResult partial = Query(RuntimeLogHistory.All, "");
            Check(partial.FailedDays == 1 && partial.Records.Count == 1 && partial.Records[0].Level == RuntimeLogHistory.Error,
                "A locked source silently hid the failure or discarded the readable error source.");
        }
        Check(original.SequenceEqual(File.ReadAllBytes(runtimeFile)), "Query modified the source log.");
        string malformed = Path.Combine(output, "malformed-row");
        WriteFile(malformed, DateTime.Today, new[] { "Time,Value", "bad-time,Message" });
        RuntimeLogResult invalid = RuntimeLogHistory.Query(malformed, "", DateTime.Today, DateTime.Today, RuntimeLogHistory.All, "", CancellationToken.None);
        Check(invalid.InvalidRecords == 1 && invalid.Records.Count == 0, "Invalid timestamps were reported as a complete empty query.");
    }

    /// <summary>Render all language variants while retaining the current filters, row objects and original details.</summary>
    private static void PageLayout(Size size, int language)
    {
        LicenseContext previous = LicenseManager.CurrentContext;
        Func<string, string, string, string, string, string> provider = UiTheme.TextProvider;
        try
        {
            LicenseManager.CurrentContext = new DesignerContext(); UiTheme.ForceDesignTimeForTest(true);
            using (RuntimeLogForm page = new RuntimeLogForm())
            {
                page.ClientSize = size; page.Show(); Application.DoEvents();
                RuntimeLogResult records = Query(RuntimeLogHistory.All, ""); Invoke(page, "BindResult", records);
                DataGridView grid = Field<DataGridView>(page, "gridLogs");
                Field<TextBox>(page, "keywordFilter").Text = "Keep input";
                Field<ComboBox>(page, "levelFilter").SelectedIndex = 2;
                DateTime from = Field<DateTimePicker>(page, "dateFrom").Value;
                object data = grid.DataSource;
                UiTheme.TextProvider = (owner, key, zh, en, es) => language == 0 ? zh : language == 1 ? en : es;
                UiTheme.ForceDesignTimeForTest(false); LicenseManager.CurrentContext = previous;
                Invoke(page, "ApplyTexts"); Layout(page);
                Check(ReferenceEquals(data, grid.DataSource) && Field<TextBox>(page, "keywordFilter").Text == "Keep input"
                    && Field<ComboBox>(page, "levelFilter").SelectedIndex == 2 && Field<DateTimePicker>(page, "dateFrom").Value == from,
                    "Language switching replaced data or query input.");
                Check(grid.Rows.Count == 3 && grid.ReadOnly && Field<TextBox>(page, "detailText").ReadOnly, "Log rows became editable or disappeared.");
                foreach (string name in new[] { "rootLayout", "rangeRow", "searchRow" }) CheckChildren(Field<Control>(page, name));
                Check(Field<Label>(page, "lblTitle").Text == (language == 0 ? "运行日志" : language == 1 ? "Runtime logs" : "Registro de ejecución"), "Missing page translation.");
                Field<TextBox>(page, "keywordFilter").Text = "";
                Field<ComboBox>(page, "levelFilter").SelectedIndex = 0;
                typeof(RuntimeLogForm).GetField("criteriaDirty", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(page, false);
                Invoke(page, "BindResult", records);
                grid.CurrentCell = grid.Rows[grid.Rows.Count - 1].Cells[0];
                Check(Field<TextBox>(page, "detailText").Text == OriginalContent, "Full multiline details were not preserved: " + Field<TextBox>(page, "detailText").Text);
                using (Bitmap image = new Bitmap(page.Width, page.Height))
                {
                    page.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
                    image.Save(Path.Combine(output, "runtime-log-" + size.Width + "-" + language + ".png"));
                }
            }
        }
        finally { UiTheme.TextProvider = provider; UiTheme.ForceDesignTimeForTest(false); LicenseManager.CurrentContext = previous; }
    }

    /// <summary>Exercise actual background reading, status completion, cancellation and timer shutdown.</summary>
    private static void AsyncPage()
    {
        LicenseContext previous = LicenseManager.CurrentContext;
        RuntimeLogForm page = null; DataForm data = null;
        try
        {
            LicenseManager.CurrentContext = new DesignerContext(); UiTheme.ForceDesignTimeForTest(true);
            data = new StationDataForm(); page = new RuntimeLogForm();
            data.GetLogSource("LogRun").SavePath = runRoot; data.GetLogSource("LogError").SavePath = errorRoot;
            ApplicationHost.RegisterModule(data);
            Console.WriteLine("  Isolated runtime sources ready");
            UiTheme.ForceDesignTimeForTest(false); LicenseManager.CurrentContext = previous;
            page.Show(); PumpUntil(() => !Field<bool>(page, "loading"));
            Console.WriteLine("  Initial background query completed");
            Check(Field<DataGridView>(page, "gridLogs").Rows.Count == 3 && Field<Control>(page, "btnExport").Enabled,
                "Completed asynchronous query did not display or enable export.");
            Check(!Field<Label>(page, "lblStatus").Text.Contains("正在"), "Completed query still says loading.");
            Field<TextBox>(page, "keywordFilter").Text = "no-such-message";
            Task oldQuery = (Task)Invoke(page, "ReloadAsync");
            Field<TextBox>(page, "keywordFilter").Text = "Camera";
            Task current = (Task)Invoke(page, "ReloadAsync");
            PumpUntil(() => oldQuery.IsCompleted && current.IsCompleted);
            Console.WriteLine("  Cancelled and current queries completed");
            Check(Field<DataGridView>(page, "gridLogs").Rows.Count == 1
                && ((RuntimeLogRecord)Field<DataGridView>(page, "gridLogs").Rows[0].DataBoundItem).Content == "Camera connection failed",
                "A cancelled query replaced newer filters.");
            page.Hide(); Application.DoEvents();
            Console.WriteLine("  Page hidden");
            Check(!Field<System.Windows.Forms.Timer>(page, "refreshTimer").Enabled, "The hidden page kept refreshing.");
        }
        finally
        {
            if (page != null) page.Dispose(); if (data != null) data.Dispose();
            UiTheme.ForceDesignTimeForTest(false); LicenseManager.CurrentContext = previous;
        }
    }

    /// <summary>Wait for short background IO while dispatching window events.</summary>
    private static void PumpUntil(Func<bool> finished)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(10);
        do { Application.DoEvents(); Thread.Sleep(10); } while (!finished() && DateTime.UtcNow < deadline);
        Check(finished(), "Asynchronous UI query did not finish.");
    }

    /// <summary>Write deterministic dated CSV fixtures without changing application configuration.</summary>
    private static void WriteFile(string root, DateTime day, IEnumerable<string> lines)
    {
        string folder = Path.Combine(root, day.ToString("yyyy"), day.ToString("MM")); Directory.CreateDirectory(folder);
        File.WriteAllLines(Path.Combine(folder, day.ToString("yyyy-MM-dd") + ".csv"), lines, new UTF8Encoding(true));
    }

    /// <summary>Use the timestamp format emitted by DataSave.</summary>
    private static string Stamp(DateTime time) { return time.ToString("yyyy-MM-dd HH:mm:ss:fff", System.Globalization.CultureInfo.InvariantCulture); }

    /// <summary>Complete nested docking before layout assertions.</summary>
    private static void Layout(Control control) { control.PerformLayout(); foreach (Control child in control.Controls) Layout(child); control.PerformLayout(); }

    /// <summary>Require query controls to remain inside their cells without overlap.</summary>
    private static void CheckChildren(Control parent)
    {
        Control[] children = parent.Controls.Cast<Control>().Where(child => child.Visible && child.Width > 0 && child.Height > 0).ToArray();
        foreach (Control child in children) Check(parent.ClientRectangle.Contains(child.Bounds), "Clipped " + child.Name + ": " + child.Bounds + " inside " + parent.ClientRectangle);
        for (int left = 0; left < children.Length; left++)
            for (int right = left + 1; right < children.Length; right++) Check(!children[left].Bounds.IntersectsWith(children[right].Bounds), "Overlapping query controls.");
    }

    /// <summary>Read a real page field.</summary>
    private static T Field<T>(object owner, string name) { return (T)owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner); }

    /// <summary>Invoke a page method without machine navigation.</summary>
    private static object Invoke(object owner, string name, params object[] args) { return owner.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(owner, args); }

    /// <summary>Report independent failures and retain all layout screenshots.</summary>
    private static void Test(string name, Action action)
    {
        Console.WriteLine("RUN " + name);
        try { action(); passed++; Console.WriteLine("PASS " + name); }
        catch (Exception error) { failed++; Console.WriteLine("FAIL " + name + ": " + error); }
    }

    /// <summary>Fail a meaningful behavior or geometry assertion.</summary>
    private static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}
