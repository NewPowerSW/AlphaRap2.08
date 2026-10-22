using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using NPSDK;
using NPSDK.Design;

internal static class Regression
{
    private static int passed;
    private static string output;
    /// <summary>验证真实 SDK worker、分支、等待、停止、报警和数据记录。</summary>
    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            output = Path.GetFullPath(args[0]);
            Application.EnableVisualStyles();
            Control.CheckForIllegalCrossThreadCalls = true;
            string alarms = Path.Combine(output, "TestAlarms.xml");
            new XElement("AlarmTable", new XElement("A99001", new XAttribute("Content", "Timeout"), new XAttribute("Type", "E"), new XAttribute("DoStop", false))).Save(alarms);
            Check(Alarm.Initial(alarms), "actual SDK alarm initialization");
            using (var input = new NPSDK.Motion.NP_Input())
                Check(!input.IsInitialized, "uninitialized IO is distinguishable from false input");
            using (var motor = new NPSDK.Motion.NP_Motor())
                Check(!motor.IsInitialized && motor.GetType().GetMethod("AlarmReset") != null, "SDK motor readiness and reset API compatibility");
            IoCompatibility();
            ValidateConfiguration();
            Persistence();
            CanvasInteraction();
            NodeDialogEditing();
            SuccessfulRun();
            BranchRun();
            TimeoutRun();
            StopPending();
            CycleRun();
            PauseRun();
            LiveInput();
            Console.WriteLine("PASS " + passed + " checks");
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
            return 1;
        }
    }

    /// <summary>记录断言结果，失败时立即停止测试。</summary>
    private static void Check(bool value, string name)
    {
        if (!value)
            throw new Exception(name);
        passed++;
        Console.WriteLine("PASS " + name);
    }

    /// <summary>使用内存 IO 替身核对原 DLL 的逻辑取反语义，不初始化板卡。</summary>
    private static void IoCompatibility()
    {
        using (var input = new NPSDK.Motion.NP_Input())
        using (var outputControl = new NPSDK.Motion.NP_Output())
        {
            var rawInput = new FakeIo();
            var rawOutput = new FakeIo();
            typeof(NPSDK.Motion.NP_Input).GetField("dInput", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(input, rawInput);
            typeof(NPSDK.Motion.NP_Input).GetField("ready", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(input, true);
            typeof(NPSDK.Motion.NP_Output).GetField("dOutput", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(outputControl, rawOutput);
            typeof(NPSDK.Motion.NP_Output).GetField("ready", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(outputControl, true);
            foreach (bool reverse in new[]
            {
                false,
                true
            }

            )
            {
                input.ReverseState = reverse;
                outputControl.ReverseState = reverse;
                foreach (bool raw in new[]
                {
                    false,
                    true
                }

                )
                {
                    rawInput.State = raw;
                    Check(input.IsOn() == (raw != reverse), "input polarity reverse=" + reverse + " raw=" + raw);
                }

                outputControl.On();
                Check(rawOutput.State == !reverse && outputControl.IsOn(), "output on polarity reverse=" + reverse);
                outputControl.Off();
                Check(rawOutput.State == reverse && !outputControl.IsOn(), "output off polarity reverse=" + reverse);
            }
        }
    }

    /// <summary>配置副本不共享模型，未知对象及错误数值在运行前被拒绝。</summary>
    private static void ValidateConfiguration()
    {
        var env = Environment();
        env.Registry.Validate(env.Example);
        var copy = FlowDesignFiles.Copy(env.Example);
        copy.Nodes[0].Text = "Other";
        Check(env.Example.Nodes[0].Text == "Scan", "configuration clone isolation");
        string file = Path.Combine(output, "roundtrip.xml");
        FlowDesignFiles.Save(file, env.Example);
        FlowDesignFiles.Save(file, env.Example);
        Check(FlowDesignFiles.Load(file).Nodes[2].Expected == "-1,1", "atomic XML roundtrip");
        copy.Nodes[2].Expected = "9,-1";
        ExpectIssue(() => env.Registry.Validate(copy), "InvalidCondition");
        copy = FlowDesignFiles.Copy(env.Example);
        copy.Nodes[1].Target = "Unknown";
        ExpectIssue(() => env.Registry.Validate(copy), "UnknownBinding");
        copy = FlowDesignFiles.Copy(env.Example);
        copy.Nodes[3].Parameters[0].Source = "Save.Path";
        ExpectIssue(() => env.Registry.Validate(copy), "InvalidSource");
        copy = FlowDesignFiles.Copy(env.Example);
        copy.Nodes[0].AlarmCode = "404";
        ExpectIssue(() => env.Registry.Validate(copy), "UnknownAlarm");
        copy = FlowDesignFiles.Copy(env.Example);
        copy.Nodes[0].Parameters = null;
        ExpectIssue(() => env.Registry.ValidateStructure(copy), "InvalidDocument");
    }

    /// <summary>期望指定配置错误，防止检查仅覆盖成功路径。</summary>
    private static void ExpectIssue(Action action, string key)
    {
        try
        {
            action();
        }
        catch (FlowDesignIssue issue)
        {
            Check(issue.Key == key, "reject " + key);
            return;
        }

        throw new Exception("Expected " + key);
    }

    /// <summary>模拟关闭并重新创建窗口，验证自动恢复、离线绑定修复与损坏回退。</summary>
    private static void Persistence()
    {
        var env = Environment();
        string file = env.DefaultPath;
        using (var form = new FlowDesignerForm(env))
        {
            form.Document.Nodes[2].X = 711;
            form.Document.Nodes[2].Timeout = 4567;
            form.Document.Nodes[2].AlarmCode = "99001";
            form.Document.Nodes[2].Expected = "-2,2";
            form.Document.Nodes[0].Text = "扫码保留";
            form.SaveToPath(file);
        }

        using (var restored = new FlowDesignerForm(env))
        {
            Check(restored.Document.Nodes[2].X == 711 && restored.Document.Nodes[2].Timeout == 4567 && restored.Document.Nodes[2].AlarmCode == "99001", "restart restores layout, timeout and alarm");
            Check(restored.Document.Nodes[3].Parameters[0].Source == "Scan.Barcode" && restored.Document.Nodes[0].Text == "扫码保留" && restored.Document.Nodes[2].Expected == "-2,2", "restart restores values, caption and data bindings");
            Check(!restored.Executing && restored.Session == null, "restoring a design never starts equipment");
            Check(restored.Document.OutputDirectory == env.OutputDirectory, "restart restores local output directory");
            restored.SetLanguageSelection("English");
            var status = (TextBox)typeof(FlowDesignerForm).GetField("statusBox", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(restored);
            Check(status.Text.StartsWith("Design restored:"), "language refreshes restored design status");
            restored.Document.Nodes[2].Target = "Missing.Camera";
            restored.SaveToPath(file);
        }

        using (var offline = new FlowDesignerForm(env))
        {
            Check(offline.Document.Nodes[2].Target == "Missing.Camera", "missing instance preserved for offline repair");
            offline.StartConfiguredFlow();
            Check(!offline.Executing && offline.Session == null, "missing instance prevents execution");
        }

        File.WriteAllText(file, "<broken>");
        using (var backup = new FlowDesignerForm(env))
            Check(backup.Document.Nodes[2].Target == "DEMO.Vision" && backup.Document.Nodes[2].X == 711, "corrupt design restores previous atomic backup");
        File.WriteAllText(file + ".bak", "<also-broken>");
        using (var fallback = new FlowDesignerForm(env))
            Check(fallback.Document.Nodes[0].Text == "Scan" && File.ReadAllText(file) == "<broken>", "failed recovery preserves damaged files");
    }

    /// <summary>模拟内部按钮拖拽和双击，确认操作的是实际 SDK 控件。</summary>
    private static void CanvasInteraction()
    {
        int count = Flow_Module.FlowChart_List.Count;
        var env = Environment();
        using (var host = new Form
        {
            Width = 1150,
            Height = 600
        }

        )
        {
            var canvas = new FlowDesignerCanvas
            {
                Dock = DockStyle.Fill
            };
            host.Controls.Add(canvas);
            canvas.LoadDocument(env.Example);
            host.Show();
            Application.DoEvents();
            Check(canvas.Nodes["Scan"].Next == canvas.Nodes["Move"], "actual SDK Next reference");
            Check(Flow_Module.FlowChart_List.Count == count, "editor nodes excluded from global production registry");
            var node = canvas.Nodes["Scan"];
            var button = node.Controls.OfType<Button>().Single();
            int x = env.Example.Nodes[0].X;
            Mouse(button, "OnMouseDown", 1, 5, 5);
            Mouse(button, "OnMouseMove", 1, 45, 25);
            Mouse(button, "OnMouseUp", 1, 5, 5);
            Check(env.Example.Nodes[0].X == x + 40, "drag updates SDK node and persisted coordinates");
            int edits = 0;
            canvas.EditRequested += n => edits++;
            Mouse(button, "OnMouseDown", 1, 5, 5);
            Mouse(button, "OnMouseUp", 1, 5, 5);
            Mouse(button, "OnMouseDown", 2, 5, 5);
            Mouse(button, "OnMouseUp", 2, 5, 5);
            Check(edits == 1, "double-click internal SDK button opens properties once");
            canvas.Editing = false;
            canvas.RequestEdit(node);
            Check(edits == 1, "running canvas prevents property changes");
            using (var bitmap = new Bitmap(host.Width, host.Height))
            {
                host.DrawToBitmap(bitmap, host.ClientRectangle);
                bitmap.Save(Path.Combine(output, "canvas.png"));
            }

            host.Close();
        }

        Check(Flow_Module.FlowChart_List.Count == count, "disposed editor keeps global registry intact");
    }

    /// <summary>发送已知鼠标事件到真实 SDK 按钮。</summary>
    private static void Mouse(Control control, string method, int clicks, int x, int y)
    {
        typeof(Control).GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(control, new object[] { new MouseEventArgs(MouseButtons.Left, clicks, x, y, 0) });
    }

    /// <summary>取消隔离编辑；三语刷新保留正在编辑的参数值和 SDK 业务标题。</summary>
    private static void NodeDialogEditing()
    {
        var env = Environment();
        using (var dialog = new FlowNodeDialog(env.Example, env.Example.Nodes[1], env))
        {
            dialog.Show();
            Application.DoEvents();
            var grid = (DataGridView)typeof(FlowNodeDialog).GetField("parameterGrid", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(dialog);
            grid.CurrentCell = grid.Rows[0].Cells[3];
            grid.BeginEdit(true);
            ((TextBox)grid.EditingControl).Text = "12.75";
            foreach (string language in new[]
            {
                "English",
                "Español",
                "Chinese"
            }

            )
            {
                env.Locale = language;
                dialog.ApplyLanguage();
                Check(((TextBox)grid.EditingControl).Text == "12.75", "language keeps pending parameter " + language);
                using (var bitmap = new Bitmap(dialog.Width, dialog.Height))
                {
                    dialog.DrawToBitmap(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
                    bitmap.Save(Path.Combine(output, "node-properties-" + language + ".png"));
                }
            }

            using (var bitmap = new Bitmap(dialog.Width, dialog.Height))
            {
                dialog.DrawToBitmap(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
                bitmap.Save(Path.Combine(output, "node-properties.png"));
            }

            dialog.Close();
            Check(env.Example.Nodes[1].Parameters[0].Value == "10", "cancel does not edit live node");
        }
    }

    /// <summary>用实际 SDK 调度完成扫码到视觉再到 CSV 的数据链路。</summary>
    private static void SuccessfulRun()
    {
        var env = Environment();
        int started = 0;
        var scanner = env.Registry.Find("Scanner.Trigger", "DEMO.Scanner");
        var original = scanner.Start;
        scanner.Start = call =>
        {
            started++;
            original(call);
        };
        using (var form = new FlowDesignerForm(env))
        {
            form.Show();
            form.SetLanguageSelection("English");
            Check(form.Canvas.Nodes["Scan"].text == "Scan", "language preserves SDK node captions");
            form.StartConfiguredFlow();
            Pump(() => !form.Executing, 8000);
            Check(form.Session.State == "Completed" && started == 1, "SDK IDLE polling starts scanner once");
            string[] files = Directory.GetFiles(env.OutputDirectory, "*.csv");
            Check(files.Length == 1, "one local production record");
            string csv = File.ReadAllText(files[0]);
            Check(csv.Contains("DEMO-0001") && csv.Contains("0.25") && csv.Contains(form.Session.RunId), "scoped output references reach CSV");
            form.StartConfiguredFlow();
            Pump(() => !form.Executing, 8000);
            Check(started == 2 && Directory.GetFiles(env.OutputDirectory, "*.csv").Length == 2, "SDK nodes reusable for a fresh run");
            using (var bitmap = new Bitmap(form.Width, form.Height))
            {
                form.DrawToBitmap(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
                bitmap.Save(Path.Combine(output, "designer.png"));
            }

            form.Close();
        }
    }

    /// <summary>条件不满足使用 SDK CASE1，而非应用自建调度器。</summary>
    private static void BranchRun()
    {
        var env = Environment();
        env.Example.Entry = "Gate";
        env.Registry.Add(new FlowActionBinding { MethodName = "Test.Gate", TargetId = "test", Outputs = new[] { "Ready" }, Poll = c => FlowActionSample.Of("Ready", false) });
        env.Example.Nodes.Add(new FlowDesignNode { Id = "Gate", Text = "Gate", MethodName = "Test.Gate", Target = "test", ConditionSource = "Gate.Ready", Comparison = FlowComparison.IsTrue, WhenFalse = FlowUnmetAction.Case1, Case1 = "Scan" });
        using (var form = new FlowDesignerForm(env))
        {
            form.Show();
            Check(form.Canvas.Nodes["Gate"].Casse1 == form.Canvas.Nodes["Scan"], "actual SDK CASE1 reference");
            form.StartConfiguredFlow();
            Pump(() => !form.Executing, 8000);
            Check(form.Session.State == "Completed", "false condition uses SDK CASE1");
            form.Close();
        }
    }

    /// <summary>超时调用实际 Alarm.Show，并阻止后继数据保存。</summary>
    private static void TimeoutRun()
    {
        Alarm.Clear();
        var env = Environment();
        env.Example.Nodes[2].Expected = "5,6";
        env.Example.Nodes[2].Timeout = 650;
        env.Example.Nodes[2].AlarmCode = "99001";
        using (var form = new FlowDesignerForm(env))
        {
            form.Show();
            form.StartConfiguredFlow();
            Pump(() => !form.Executing, 8000);
            Check(form.Session.State == "TimedOut", "condition timeout stops SDK flow");
            Check(Alarm.AlarmList.Any(a => a.Code == "99001"), "timeout calls actual NPSDK.Alarm.Show");
            Check(!Directory.Exists(env.OutputDirectory) || Directory.GetFiles(env.OutputDirectory, "*.csv").Length == 0, "timeout skips downstream save");
            form.Close();
        }

        Alarm.Clear();
    }

    /// <summary>停止后等待已触发任务结束，丢弃迟到结果。</summary>
    private static void StopPending()
    {
        var env = Environment();
        var scan = env.Registry.Find("Scanner.Trigger", "DEMO.Scanner");
        scan.Start = call => call.Pending = Task.Run(() =>
        {
            Thread.Sleep(500);
            call.State = "Late";
        });
        scan.Poll = call => call.Pending.IsCompleted ? FlowActionSample.Of("Barcode", call.State) : new FlowActionSample
        {
            Waiting = true
        };
        using (var form = new FlowDesignerForm(env))
        {
            form.Show();
            form.StartConfiguredFlow();
            Pump(() => form.Session.PendingWork, 2000);
            form.RequestStop();
            Application.DoEvents();
            Check(form.Executing, "stop keeps editing locked while task is pending");
            Pump(() => !form.Executing, 4000);
            Check(form.Session.State == "Stopped", "stop drains worker and pending task");
            Check(!Directory.Exists(env.OutputDirectory), "late result cannot reach save");
            form.Close();
        }
    }

    /// <summary>SDK 节点自循环可停止，交接线程不会造成提前解锁。</summary>
    private static void CycleRun()
    {
        var env = Environment();
        env.Example.Nodes[0].Next = "Scan";
        int starts = 0;
        var scan = env.Registry.Find("Scanner.Trigger", "DEMO.Scanner");
        var start = scan.Start;
        scan.Start = call =>
        {
            starts++;
            start(call);
        };
        using (var form = new FlowDesignerForm(env))
        {
            form.Show();
            form.StartConfiguredFlow();
            Pump(() => starts >= 3, 6000);
            form.RequestStop();
            Pump(() => !form.Executing, 4000);
            Check(form.Session.State == "Stopped", "SDK self-loop and stop");
            form.Close();
        }
    }

    /// <summary>节点暂停可恢复，停止可以退出暂停循环。</summary>
    private static void PauseRun()
    {
        var env = Environment();
        env.Example.Nodes[0].Delay = 1500;
        using (var form = new FlowDesignerForm(env))
        {
            form.Show();
            form.StartConfiguredFlow();
            Pump(() => form.Canvas.Nodes["Scan"].IsExecutionBusy, 1000);
            form.RequestPause(true);
            Check(form.Session.PauseRequested, "pause bound to SDK execution scope");
            form.RequestPause(false);
            Check(!form.Session.PauseRequested, "resume clears SDK scope pause");
            form.RequestStop();
            Pump(() => !form.Executing, 4000);
            Check(form.Session.State == "Stopped", "stop exits a paused SDK node");
            form.Close();
        }
    }

    /// <summary>无限等待期间持续读取 IO，只触发一次动作，条件改变后由 SDK 结束。</summary>
    private static void LiveInput()
    {
        var env = Environment();
        bool ready = false;
        int reads = 0;
        env.Example = new FlowDesignDocument
        {
            Entry = "Input"
        };
        env.Example.Nodes.Add(new FlowDesignNode { Id = "Input", Text = "Input", MethodName = "IO.Read", Target = "DEMO.Input", ConditionSource = "IsOn", Comparison = FlowComparison.IsTrue, Timeout = 0 });
        env.Registry.Find("IO.Read", "DEMO.Input").Poll = call =>
        {
            reads++;
            return FlowActionSample.Of("IsOn", ready);
        };
        Flow_Module.AllStop = true;
        using (var form = new FlowDesignerForm(env))
        {
            form.Show();
            form.StartConfiguredFlow();
            Pump(() => reads >= 3, 2500);
            Check(form.Executing, "zero timeout keeps polling live IO");
            ready = true;
            Pump(() => !form.Executing, 3000);
            Check(form.Session.State == "Completed", "scoped SDK flow ignores legacy global stop and reacts to input change");
            form.Close();
        }

        Flow_Module.AllStop = false;
    }

    /// <summary>建立每项测试独立的输出目录和报警白名单。</summary>
    private static FlowDesignerEnvironment Environment()
    {
        var env = FlowDesignerDemo.CreateEnvironment();
        env.OutputDirectory = Path.Combine(output, Guid.NewGuid().ToString("N"));
        env.DefaultPath = Path.Combine(output, Guid.NewGuid().ToString("N") + ".xml");
        env.Registry.AlarmCodes.Add("99001");
        return env;
    }

    /// <summary>泵送 UI 消息直至真实 worker 完成，超时即失败。</summary>
    private static void Pump(Func<bool> completed, int timeout)
    {
        var clock = Stopwatch.StartNew();
        while (!completed() && clock.ElapsedMilliseconds < timeout)
        {
            Application.DoEvents();
            Thread.Sleep(10);
        }

        Application.DoEvents();
        if (!completed())
            throw new TimeoutException("SDK flow test timed out.");
    }
}

internal sealed class FakeIo : IOBaseInterface
{
    public bool State;
    /// <summary>读取内存物理电平。</summary>
    public override bool GetState()
    {
        return State;
    }

    /// <summary>置位内存物理电平。</summary>
    public override bool On()
    {
        State = true;
        return true;
    }

    /// <summary>复位内存物理电平。</summary>
    public override bool Off()
    {
        State = false;
        return true;
    }
}
