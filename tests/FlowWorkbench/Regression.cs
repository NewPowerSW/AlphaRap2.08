using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlphaRap.FlowRuntime;
using AlphaRap.FlowWorkbench;

internal static class Regression
{
    private static int passed;
    private static string root;
    private static readonly ActionCatalog catalog = ActionCatalog.Simulation();

    /// <summary>执行不接硬件的运行语义、保存、校验和界面回归。</summary>
    [STAThread]
    private static int Main(string[] args)
    {
        root = Path.Combine(args[0], "regression-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            RuntimeChecks().GetAwaiter().GetResult();
            ValidationChecks();
            XmlChecks();
            UiChecks();
            Console.WriteLine("RESULT: " + passed + " passed");
            return 0;
        }
        catch (Exception error) { Console.WriteLine("FAIL: " + error); return 1; }
    }

    /// <summary>检查真假分支、输入输出隔离、超时、停止与持久保存。</summary>
    private static async Task RuntimeChecks()
    {
        var flow = DemoFlow.Create(catalog);
        var trace = new List<FlowNotice>();
        var runner = new FlowRunner();
        runner.Notice += trace.Add;
        var ok = await runner.RunAsync(flow, catalog, Path.Combine(root, "ok"), CancellationToken.None);
        Check(ok.State == "Succeeded", "default flow completes");
        Check(ok.Results.ContainsKey("OK") && !ok.Results.ContainsKey("NG"), "true branch is exclusive");
        Check(trace.Count(n => n.NodeId == "Move" && n.State == "Running") == 1, "move is dispatched once");
        Check((double)ok.Results["Vision"]["X"] == 0.12 && (bool)ok.Results["Check"]["Passed"], "typed outputs arrive at condition");
        string path = (string)ok.Results["Save"]["Path"];
        Check(File.Exists(path), "save returns an existing local record");
        string csv = File.ReadAllText(path);
        Check(csv.Contains(ok.RunId) && csv.Contains("DEMO-001") && csv.Contains("0.12"), "CSV contains run identity and bound values");
        Check(File.ReadAllBytes(path).Take(3).SequenceEqual(new byte[] { 239, 187, 191 }), "CSV has UTF-8 BOM");
        Check(!Directory.GetFiles(root, "*.tmp", SearchOption.AllDirectories).Any(), "no temporary file after commit");

        flow.Nodes.First(n => n.Id == "Vision").TargetId = "CameraB";
        var ng = await runner.RunAsync(flow, catalog, Path.Combine(root, "ng"), CancellationToken.None);
        Check(ng.State == "Succeeded" && ng.Results.ContainsKey("NG") && !ng.Results.ContainsKey("OK"), "NG is a valid business branch");
        Check(!(bool)ng.Results["Check"]["Passed"] && File.ReadAllText((string)ng.Results["Save"]["Path"]).Contains("False"), "NG judgement is saved");
        Check(ok.RunId != ng.RunId && (double)ok.Results["Vision"]["X"] == 0.12, "runs have independent immutable scalar snapshots");

        flow = DemoFlow.Create(catalog);
        flow.Nodes[0].Inputs[0].Value = "产品,\"A\"\n下一行";
        var escaped = await runner.RunAsync(flow, catalog, Path.Combine(root, "csv"), CancellationToken.None);
        string escapedCsv = File.ReadAllText((string)escaped.Results["Save"]["Path"]);
        Check(escapedCsv.Contains("\"产品,\"\"A\"\"\n下一行\""), "CSV preserves comma quote newline and Unicode");

        flow = DemoFlow.Create(catalog);
        flow.Nodes.First(n => n.Id == "Wait").TimeoutMs = 30;
        trace.Clear();
        var timeout = await runner.RunAsync(flow, catalog, Path.Combine(root, "timeout"), CancellationToken.None);
        Check(timeout.State == "Failed" && timeout.Error.Key == "Timeout", "deadline produces explicit failure");
        Check(trace.Any(n => n.State == "TimedOut" && n.AlarmCode == "SIM_ACTION"), "timeout publishes bound simulation alarm");
        Check(!timeout.Results.ContainsKey("Move") && !timeout.Results.ContainsKey("Save"), "timeout prevents subsequent move and save");

        flow = DemoFlow.Create(catalog);
        flow.Devices[0].State = false;
        using (var cancel = new CancellationTokenSource())
        {
            cancel.CancelAfter(40);
            var stopped = await runner.RunAsync(flow, catalog, Path.Combine(root, "stop"), cancel.Token);
            Check(stopped.State == "Cancelled" && !stopped.Results.ContainsKey("Move"), "stop cancels waiting and blocks next action");
        }
        using (var cancel = new CancellationTokenSource())
        {
            var active = runner.RunAsync(flow, catalog, Path.Combine(root, "busy"), cancel.Token);
            bool busy = false;
            try { await runner.RunAsync(flow, catalog, root, CancellationToken.None); } catch (FlowIssue error) { busy = error.Key == "Busy"; }
            Check(busy, "same runner rejects overlapping starts");
            cancel.Cancel(); await active;
        }

        flow = DemoFlow.Create(catalog);
        var pending = runner.RunAsync(flow, catalog, Path.Combine(root, "snapshot"), CancellationToken.None);
        flow.Devices.First(d => d.Id == "CameraA").X = 100;
        flow.Nodes[0].Inputs[0].Value = "CHANGED";
        var frozen = await pending;
        Check((double)frozen.Results["Vision"]["X"] == 0.12 && (string)frozen.Results["Start"]["Barcode"] == "DEMO-001", "running plan is frozen against editor changes");

        string blocked = Path.Combine(root, "not-a-directory");
        File.WriteAllText(blocked, "file");
        var saveFailure = await runner.RunAsync(DemoFlow.Create(catalog), catalog, blocked, CancellationToken.None);
        Check(saveFailure.State == "Failed" && !saveFailure.Results.ContainsKey("Save"), "save failure never returns a record acknowledgement");

        var previousCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("es-ES");
            var cultureRun = await runner.RunAsync(DemoFlow.Create(catalog), catalog, Path.Combine(root, "culture"), CancellationToken.None);
            Check(File.ReadAllText((string)cultureRun.Results["Save"]["Path"]).Contains("0.12"), "stored numbers remain invariant in Spanish culture");
        }
        finally { CultureInfo.CurrentCulture = previousCulture; }

        var ioFlow = new FlowDefinition { StartNode = "Read" };
        ioFlow.Devices.Add(new SimDevice { Id = "Input1", Kind = DeviceKind.Input, State = false });
        ioFlow.Devices.Add(new SimDevice { Id = "Output1", Kind = DeviceKind.Output, State = false });
        ioFlow.Nodes.Add(DemoFlow.Node(catalog, "Read", "io.read", "Input1", "Set"));
        ioFlow.Nodes.Add(DemoFlow.Node(catalog, "Set", "io.write", "Output1", "Check"));
        var boolean = DemoFlow.Node(catalog, "Check", "condition.equal", "", "OK");
        boolean.FalseNext = "NG"; boolean.Inputs[0].Source = "Read.State";
        ioFlow.Nodes.Add(boolean);
        ioFlow.Nodes.Add(DemoFlow.Node(catalog, "OK", "flow.note", "", "Save"));
        ioFlow.Nodes.Add(DemoFlow.Node(catalog, "NG", "flow.note", "", "Save"));
        var ioSave = DemoFlow.Node(catalog, "Save", "data.save", "", "");
        ioSave.Inputs.First(i => i.Name == "Passed").Source = "Check.Passed";
        ioSave.Inputs.Add(new InputBinding { Name = "输出状态", Source = "Set.State" });
        ioFlow.Nodes.Add(ioSave);
        var ioResult = await runner.RunAsync(ioFlow, catalog, Path.Combine(root, "io"), CancellationToken.None);
        Check(ioResult.State == "Succeeded" && ioResult.Results.ContainsKey("NG"), "IO sample drives typed boolean branch");
        Check((bool)ioResult.Results["Set"]["State"], "simulated output write returns current state");
        Check(!ioFlow.Devices[1].State, "simulated output changes cannot mutate editor instance settings");
        Check(File.ReadAllText((string)ioResult.Results["Save"]["Path"]).Contains("输出状态"), "Unicode custom field and IO output reach CSV");
    }

    /// <summary>检查结构、分支可达性、类型和非法输入。</summary>
    private static void ValidationChecks()
    {
        Reject(f => f.Nodes[1].Id = "Start", "DuplicateId", "duplicate node IDs");
        Reject(f => f.Nodes[1].Next = "Missing", "MissingNode", "missing successor");
        Reject(f => f.Nodes[2].Next = "Wait", "Cycle", "cycle in test-version flow");
        Reject(f => f.Nodes.First(n => n.Id == "Check").FalseNext = "", "MissingBranch", "missing false branch");
        Reject(f => f.Nodes[2].TargetId = "CameraA", "InvalidTarget", "wrong target kind");
        Reject(f => f.Nodes[1].TimeoutMs = 0, "InvalidTimeout", "invalid timeout");
        Reject(f => f.Nodes[1].AlarmCode = "UNKNOWN", "UnknownAlarm", "unknown alarm");
        Reject(f => f.Nodes[1].Inputs[0].Value = "garbage", "InvalidValue", "invalid bool literal");
        Reject(f => f.Nodes[2].Inputs[0].Value = "NaN", "InvalidValue", "non-finite numeric literal");
        Reject(f => f.Nodes[2].Inputs[0].Source = "Vision.X", "InvalidBinding", "future output reference");
        Reject(f => f.Nodes[2].Inputs[0].Source = "Start.Barcode", "InvalidBinding", "wrong output type");
        Reject(f => f.Nodes.Last().Inputs[0].Source = "NG.Message", "InvalidBinding", "branch-local output at merge");
        Reject(f => f.Nodes.Last().Inputs.Add(new InputBinding { Name = "RunId" }), "InvalidInputs", "reserved record field");
        Reject(f => f.Nodes.Add(DemoFlow.Node(catalog, "Detached", "flow.note", "", "")), "Unreachable", "unreachable node");
        Reject(f => f.Devices[0].DelayMs = -1, "InvalidDevice", "invalid instance delay");
        Reject(f => { f.Nodes.RemoveAll(n => n.Id == "Save"); f.Nodes.First(n => n.Id == "OK").Next = ""; f.Nodes.First(n => n.Id == "NG").Next = ""; }, "MissingSave", "terminal path without local save");
        Check(!typeof(FlowRunner).Assembly.GetReferencedAssemblies().Any(a => a.Name.Contains("NPSDK") || a.Name.Contains("Cognex") || a.Name.Contains("Windows.Forms")), "runtime has no hardware SDK or WinForms dependency");
    }

    /// <summary>检查 XML 原子往返和外部实体拒绝。</summary>
    private static void XmlChecks()
    {
        string path = Path.Combine(root, "flow.xml");
        var flow = DemoFlow.Create(catalog);
        FlowFiles.Save(path, flow);
        flow.Nodes[0].Inputs[0].Value = "SECOND";
        FlowFiles.Save(path, flow);
        var loaded = FlowFiles.Load(path);
        FlowValidation.Validate(loaded, catalog);
        Check(loaded.Nodes[0].Inputs[0].Value == "SECOND" && loaded.Devices.Count == 5, "XML round-trip and atomic replacement");
        string hostile = Path.Combine(root, "entity.xml");
        File.WriteAllText(hostile, "<!DOCTYPE x [<!ENTITY ext SYSTEM 'file:///nonexistent'>]><FlowDefinition>&ext;</FlowDefinition>");
        bool rejected = false;
        try { FlowFiles.Load(hostile); } catch (Exception) { rejected = true; }
        Check(rejected, "XML external entities are rejected");
    }

    /// <summary>检查实际窗口绑定、三语言编辑保护和异步运行界面。</summary>
    private static void UiChecks()
    {
        Application.EnableVisualStyles();
        using (var form = new FlowWorkbenchForm())
        {
            form.Show(); Application.DoEvents();
            var inputs = Field<DataGridView>(form, "inputGrid");
            var languages = Field<ComboBox>(form, "languageBox");
            var flow = Field<FlowDefinition>(form, "flow");
            Check(Convert.ToString(languages.SelectedItem) == "Chinese", "default language is selected");
            inputs.CurrentCell = inputs.Rows[0].Cells[2];
            inputs.BeginEdit(true);
            ((TextBox)inputs.EditingControl).Text = "EDIT-IN-PROGRESS";
            int count = inputs.Rows.Count;
            languages.SelectedItem = "English";
            Check(form.Text.Contains("configuration") && inputs.Rows.Count == count, "English refresh preserves parameter rows");
            Check(inputs.EditingControl != null && ((TextBox)inputs.EditingControl).Text == "EDIT-IN-PROGRESS", "language switch preserves uncommitted input");
            languages.SelectedItem = "Español";
            Check(form.Text.Contains("Configuración") && ReferenceEquals(flow, Field<FlowDefinition>(form, "flow")), "Spanish refresh preserves model identity");
            inputs.EndEdit();
            Field<TextBox>(form, "outputBox").Text = Path.Combine(root, "ui");
            Field<Button>(form, "runButton").PerformClick();
            var timer = Stopwatch.StartNew();
            while (!Field<Button>(form, "runButton").Enabled && timer.ElapsedMilliseconds < 10000) { Application.DoEvents(); Thread.Sleep(10); }
            Application.DoEvents();
            Check(Field<Button>(form, "runButton").Enabled && !Field<Button>(form, "stopButton").Enabled, "UI completes without blocking message dispatch");
            Check(Directory.GetFiles(Path.Combine(root, "ui"), "*.csv", SearchOption.AllDirectories).Length == 1, "actual UI run saves one record");
            Check(Field<DataGridView>(form, "resultGrid").Rows.Count > 5, "UI shows propagated outputs");
            var nodes = Field<DataGridView>(form, "nodeGrid");
            nodes.CurrentCell = nodes.Rows[3].Cells[3];
            nodes.Rows[3].Cells[3].Value = "CameraB";
            Check(flow.Nodes.First(n => n.Id == "Vision").TargetId == "CameraB", "UI instance selection updates runtime target");
            nodes.CurrentCell = nodes.Rows[7].Cells[0];
            Field<Button>(form, "addFieldButton").PerformClick();
            int extra = inputs.Rows.Count - 1;
            inputs.Rows[extra].Cells[0].Value = "StationNote";
            inputs.Rows[extra].Cells[1].Value = "Move.X";
            Field<Button>(form, "runButton").PerformClick();
            timer.Restart();
            while (!Field<Button>(form, "runButton").Enabled && timer.ElapsedMilliseconds < 10000) { Application.DoEvents(); Thread.Sleep(10); }
            Application.DoEvents();
            var records = Directory.GetFiles(Path.Combine(root, "ui"), "*.csv", SearchOption.AllDirectories);
            Check(records.Length == 2 && records.Any(p => File.ReadAllText(p).Contains("False")), "UI selected instance follows NG branch and saves");
            Check(records.Any(p => File.ReadAllText(p).Contains("StationNote")), "UI custom save field survives mapping and execution");
            Check(flow.Nodes.Last().Inputs.Any(i => i.Name == "StationNote" && i.Source == "Move.X"), "UI custom binding is stored in flow model");
            var tabs = Field<TabControl>(form, "tabs");
            tabs.SelectedTab = Field<TabPage>(form, "graphTab");
            foreach (string lang in new[] { "Chinese", "English", "Español" })
            {
                languages.SelectedItem = lang;
                Application.DoEvents();
                using (var bitmap = new System.Drawing.Bitmap(form.Width, form.Height))
                {
                    form.DrawToBitmap(bitmap, new System.Drawing.Rectangle(System.Drawing.Point.Empty, bitmap.Size));
                    bitmap.Save(Path.Combine(Path.GetDirectoryName(root), "flow-workbench-" + lang + ".png"));
                }
            }
            var deviceGrid = Field<DataGridView>(form, "deviceGrid");
            deviceGrid.Rows[0].Cells[2].Value = false;
            Field<Button>(form, "runButton").PerformClick();
            form.Close();
            timer.Restart();
            while (!form.IsDisposed && timer.ElapsedMilliseconds < 3000) { Application.DoEvents(); Thread.Sleep(10); }
            Check(form.IsDisposed, "closing waits for cooperative cancellation before disposing UI");
            Check(Directory.GetFiles(Path.Combine(root, "ui"), "*.csv", SearchOption.AllDirectories).Length == 2, "closing a waiting flow does not dispatch save");
        }
    }

    /// <summary>读取测试窗口实际控件，避免另建与产品不同的界面。</summary>
    private static T Field<T>(object target, string name) { return (T)target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(target); }

    /// <summary>断言配置被指定的规则拒绝。</summary>
    private static void Reject(Action<FlowDefinition> change, string key, string name)
    {
        var flow = DemoFlow.Create(catalog); change(flow);
        try { FlowValidation.Validate(flow, catalog); }
        catch (FlowIssue error) { Check(error.Key == key, name + " rejected as " + key); return; }
        throw new Exception("Not rejected: " + name);
    }

    /// <summary>记录可复核的独立断言。</summary>
    private static void Check(bool condition, string name)
    {
        if (!condition) throw new Exception(name);
        passed++; Console.WriteLine("PASS " + name);
    }
}
