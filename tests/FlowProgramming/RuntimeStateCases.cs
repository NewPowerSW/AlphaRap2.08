using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using AlphaRap;
using AlphaRap.Configuration;
using AlphaRap.FlowProgramming;
using NPSDK;
using NPSDK.Motion;

internal sealed class RuntimeManualAxisDriver : MotorBaseInterface
{
    internal bool Busy, HoldAfterStop;
    internal int HomeCalls, Stops;

    /// <summary>Keep a simulated manual home active until the test explicitly stops its driver.</summary>
    public override bool Home() { HomeCalls++; Busy = true; return false; }
    /// <summary>Prepare the next simulated home without issuing any physical command.</summary>
    public override void HomeReset() { Busy = false; }
    /// <summary>Expose controlled physical motion independently from UI manual-session flags.</summary>
    public override bool GetBusy() { return Busy; }
    /// <summary>Model immediate settling or a driver that remains physically busy after a stop request.</summary>
    public override void FastStop() { Stops++; if (!HoldAfterStop) Busy = false; }
}

internal sealed class RuntimeStatePage : Flow_BaseForm
{
    internal readonly Flow_Chart Node = new Flow_Chart { Name = "runtimeNode", text = "Runtime state" };
    internal readonly ManualResetEventSlim Entered = new ManualResetEventSlim();
    internal readonly ManualResetEventSlim Release = new ManualResetEventSlim();
    internal int Invocations;

    /// <summary>Create a pure offline node with no equipment or IO.</summary>
    internal RuntimeStatePage()
    {
        Name = "RuntimeState_" + Guid.NewGuid().ToString("N");
        Node.FlowChartRun += Poll;
        InitialPage.Controls.Add(Node);
    }

    /// <summary>Wait cooperatively until the node deadline without accessing hardware.</summary>
    [FlowTool("状态等待", "State wait", "Espera de estado", UsesHardware = false)]
    private Flow_Chart.ResultType Poll() { Interlocked.Increment(ref Invocations); return Flow_Chart.ResultType.IDLE; }

    /// <summary>Hold an actual invocation so cancellation cannot falsely report it finished.</summary>
    [FlowTool("状态阻塞", "Blocked state", "Estado bloqueado", UsesHardware = false)]
    private Flow_Chart.ResultType Block() { Interlocked.Increment(ref Invocations); Entered.Set(); Release.Wait(); return Flow_Chart.ResultType.End; }

    /// <summary>Release this offline page from the SDK registry.</summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing) { Release.Set(); Flow_Module.FlowChart_ModuleList.Remove(this); }
        base.Dispose(disposing);
    }
}

internal static partial class Cases
{
    /// <summary>Exercise one busy decision across completed, stopping and unmanaged execution states.</summary>
    private static void RegisterRuntimeStateCases()
    {
        Test("completed managed timeouts ignore stale legacy flags and permit save validation", CompletedManagedState);
        Test("timed-out callbacks remain busy after Stop until the actual invocation returns", StoppingManagedState);
        Test("unmanaged legacy modules still block configuration changes", UnmanagedRuntimeState);
        Test("both machine entries explain all editing pages in three languages without changing drafts or starting nodes", EditingBlocksMachineStart);
        Test("direct and main motor stops cancel manual homing without bypassing hardware startup busy checks", ManualHomeStartValidation);
    }

    /// <summary>Disable only operation logging in this isolated fixture and restore the original host afterward.</summary>
    private static void WithRuntimeState(Action<RuntimeStatePage, FlowModuleEditor> test)
    {
        object previous = host.GetField("DataF").GetValue(null);
        SetHost("DataF", null);
        try
        {
            using (RuntimeStatePage page = new RuntimeStatePage())
            {
                FlowModuleEditor owner = FlowPrograms.Attach(page);
                try { test(page, owner); }
                finally { page.Release.Set(); owner.Runtime.Stop(); Wait(owner.Runtime.Completion); page.RunningStatus = false; page.Node.RunningStatus = false; }
            }
        }
        finally { SetHost("DataF", previous); }
    }

    /// <summary>Reproduce an idle task with stale form/node flags after a real deadline and prove Main Save's guard accepts it.</summary>
    private static void CompletedManagedState()
    {
        WithRuntimeState((page, owner) =>
        {
            FlowStep step = owner.Program.Steps.Single(); step.TimeoutMs = 80;
            owner.Start(step.Id, true, true);
            Wait(owner.Runtime.Completion);
            Assert(owner.Runtime.Error != null && owner.Runtime.Error.Contains("Timeout"), "The timeout fixture did not finish.");
            page.RunningStatus = true; page.Node.RunningStatus = true;
            Assert(ReferenceEquals(page.ProgramController, owner) && !owner.IsRunning && !FlowPrograms.AnyRunning && !Flow_Module.Module_GetRunningStatus,
                "A completed managed runtime was treated as busy because of stale legacy fields.");
            FlowPrograms.RequireIdle();
        });
    }

    /// <summary>Preserve resource ownership and reject saving while the callback outlives timeout and cancellation.</summary>
    private static void StoppingManagedState()
    {
        WithRuntimeState((page, owner) =>
        {
            FlowActions actions = new FlowActions(FlowResources.Capture(true), page);
            FlowResource resource = actions.Resources.Items.Values.Single(item => item.Kind == "native" && ReferenceEquals(item.Instance, page));
            FlowMethod method = resource.Methods.Single(item => item.Id.StartsWith("native.Block.", StringComparison.Ordinal));
            FlowStep step = owner.AddNode(resource.Id, method.Id, page.InitialPage, new Point(10, 50)); step.TimeoutMs = 120;
            bool timedOut = false;
            owner.Runtime.StateChanged += (node, state) => { if (state == "Timeout") Volatile.Write(ref timedOut, true); };
            owner.Start(step.Id, true, false);
            ActionUntil(() => page.Entered.IsSet && Volatile.Read(ref timedOut), "The blocking callback did not reach its deadline.");
            page.RunningStatus = false; page.Node.RunningStatus = false;
            owner.Handle(FlowProgramCommand.Stop);
            bool rejected = false;
            try { FlowPrograms.RequireIdle(); } catch (InvalidOperationException error) { rejected = error.Message.Contains(page.Name); }
            Assert(owner.IsRunning && FlowPrograms.AnyRunning && Flow_Module.Module_GetRunningStatus && rejected && !FlowResourceGate.CanAccess(page),
                "Stopping or timeout falsely released an active callback, resource or save guard.");
            page.Release.Set(); Wait(owner.Runtime.Completion);
            Assert(!owner.IsRunning && !FlowPrograms.AnyRunning && FlowResourceGate.CanAccess(page), "The returned callback did not release its busy state and ownership.");
            FlowPrograms.RequireIdle();
        });
    }

    /// <summary>Keep the old execution contract for a module that is not controlled by the mixed-flow runtime.</summary>
    private static void UnmanagedRuntimeState()
    {
        using (Flow_BaseForm legacy = new Flow_BaseForm { Name = "UnmanagedState", RunningStatus = true })
        {
            legacy.ProgramController = null;
            try
            {
                bool rejected = false;
                try { FlowPrograms.RequireIdle(); } catch (InvalidOperationException error) { rejected = error.Message.Contains(legacy.Name); }
                Assert(FlowPrograms.AnyRunning && rejected, "A legacy module's busy state was ignored.");
            }
            finally { legacy.RunningStatus = false; Flow_Module.FlowChart_ModuleList.Remove(legacy); }
        }
    }

    /// <summary>Call the actual machine-entry guard with an unseen editing page and preserve every offline editor draft.</summary>
    private static void EditingBlocksMachineStart()
    {
        var previousEditing = FlowPrograms.Modules.ToDictionary(module => module, module => module.IsEditing);
        LanguageType previousLanguage = SysPara.LanguageShow;
        CultureInfo previousCulture = Thread.CurrentThread.CurrentUICulture;
        RunMode previousMode = SysPara.SystemMode;
        PermissionType previousPermission = SysPara.UserPermission;
        bool previousDebug = FlowPrograms.DebugEnabled;
        object previousHardware = host.GetField("HardF").GetValue(null), previousData = host.GetField("DataF").GetValue(null);
        object previousInstalled = typeof(FlowPrograms).GetField("installed", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
        try
        {
            Assert(!FlowPrograms.AnyRunning && (bool)previousInstalled, "The guard fixture requires an installed, idle offline host.");
            SysPara.SystemMode = RunMode.IDLE; SysPara.UserPermission = PermissionType.Administrator;
            FlowPrograms.DebugEnabled = false; SetHost("HardF", null); SetHost("DataF", null);
            foreach (FlowModuleEditor module in previousEditing.Keys) module.SetEditing(false);
            using (RuntimeStatePage visible = new RuntimeStatePage())
            using (RuntimeStatePage hidden = new RuntimeStatePage())
                VerifyEditingStartDialogs(visible, hidden);
        }
        finally
        {
            foreach (var entry in previousEditing) entry.Key.SetEditing(entry.Value);
            SwitchLanguage(previousLanguage); Thread.CurrentThread.CurrentUICulture = previousCulture;
            SysPara.SystemMode = previousMode; SysPara.UserPermission = previousPermission;
            FlowPrograms.DebugEnabled = previousDebug; SetHost("HardF", previousHardware); SetHost("DataF", previousData);
            Assert(Equals(previousInstalled, typeof(FlowPrograms).GetField("installed", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null)),
                "Machine-entry validation changed factory installation state.");
        }
    }

    /// <summary>Check real modal text and unchanged model snapshots for initialization and start in each supported language.</summary>
    private static void VerifyEditingStartDialogs(RuntimeStatePage visible, RuntimeStatePage hidden)
    {
        FlowModuleEditor first = FlowPrograms.Attach(visible), second = FlowPrograms.Attach(hidden);
        first.Program.Steps.Single().Caption = "Unsaved visible draft";
        second.Program.Steps.Single().Caption = "Unsaved hidden draft";
        first.SetEditing(true); second.SetEditing(true);
        Assert(!hidden.Visible, "The hidden editing module was unexpectedly shown.");
        var drafts = FlowPrograms.Modules.ToDictionary(module => module, SnapshotRuntimeDraft);
        string names = string.Join(", ", FlowPrograms.Modules.Where(module => module.IsEditing).Select(module => module.Form.Name));
        foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
        {
            SwitchLanguage(language);
            string expected = string.Format(FlowText.Defaults["editingBlocksStart"][(int)language], names);
            VerifyEditingStartDialog(true, expected, visible, hidden);
            VerifyEditingStartDialog(false, expected, visible, hidden);
            foreach (var draft in drafts) Assert(SnapshotRuntimeDraft(draft.Key) == draft.Value, "Machine-entry validation altered a draft or published program.");
        }
        first.SetEditing(false); second.SetEditing(false);
        bool ignored;
        string afterEditing = CaptureMachineStartDialog(true, out ignored);
        Assert(afterEditing == null || (!afterEditing.Contains(visible.Name) && !afterEditing.Contains(hidden.Name)),
            "Machine-entry validation still reported editing pages after their editing switches were cleared.");
        Assert(visible.Invocations == 0 && hidden.Invocations == 0 && !FlowPrograms.AnyRunning,
            "Checking a machine entry executed an offline node.");
    }

    /// <summary>Require the expected localized editing instructions from a real machine-entry MessageBox.</summary>
    private static void VerifyEditingStartDialog(bool initialize, string expected, RuntimeStatePage visible, RuntimeStatePage hidden)
    {
        bool allowed;
        string actual = CaptureMachineStartDialog(initialize, out allowed);
        Assert(!allowed && actual == expected && actual.Contains(visible.Name) && actual.Contains(hidden.Name),
            "Unexpected " + (initialize ? "initialization" : "start") + " guard message: " + (actual ?? "<no dialog>"));
        Assert(visible.Invocations == 0 && hidden.Invocations == 0 && !FlowPrograms.AnyRunning,
            "An editing guard started a node or reported runtime activity.");
    }

    /// <summary>Serialize both draft and published models so validation cannot silently discard or publish edits.</summary>
    private static string SnapshotRuntimeDraft(FlowModuleEditor module)
    {
        FlowProgram published = (FlowProgram)typeof(FlowModuleEditor).GetProperty("PublishedProgram", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(module, null);
        using (StringWriter text = new StringWriter(CultureInfo.InvariantCulture))
        {
            XmlSerializer serializer = new XmlSerializer(typeof(FlowProgram));
            serializer.Serialize(text, module.Program); serializer.Serialize(text, published);
            return text.ToString();
        }
    }

    /// <summary>Use a registered axis group with a fake driver to exercise the application's actual startup validation.</summary>
    private static void ManualHomeStartValidation()
    {
        HardwareEntry entry = hardware.Resources.Entries.Single(item => item.Definition.Id == groupedAxis.Id);
        NP_AxisGroup group = (NP_AxisGroup)entry.Group;
        NP_Motor motor = group.Motor;
        FieldInfo driverField = typeof(NP_Motor).GetField("dMotor", BindingFlags.Instance | BindingFlags.NonPublic);
        FieldInfo readyField = typeof(NP_Motor).GetField("ready", BindingFlags.Instance | BindingFlags.NonPublic);
        object previousDriver = driverField.GetValue(motor), previousReady = readyField.GetValue(motor);
        Func<bool> previousCanOperate = group.CanOperate;
        LanguageType previousLanguage = SysPara.LanguageShow;
        CultureInfo previousCulture = Thread.CurrentThread.CurrentUICulture;
        System.Windows.Forms.Timer timer = (System.Windows.Forms.Timer)typeof(NP_AxisGroup).GetField("refresh", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(group);
        bool previousTimer = timer.Enabled;
        ControlBaseInterface[] previousControls = SDKPara.ControlsSnapshot();
        RuntimeManualAxisDriver driver = new RuntimeManualAxisDriver();
        try
        {
            timer.Stop(); driverField.SetValue(motor, driver); readyField.SetValue(motor, true); group.CanOperate = () => true;
            lock (SDKPara.ControlList) { SDKPara.ControlList.Clear(); SDKPara.ControlList.Add(motor); }
            hardware.Resources.ValidateStart();
            VerifyStoppedManualHome(group, driver, false, false);
            VerifyStoppedManualHome(group, driver, true, false);
            VerifyStoppedManualHome(group, driver, false, true);
            VerifyStoppedManualHome(group, driver, true, true);
        }
        finally
        {
            driver.HoldAfterStop = false; motor.Stop(); timer.Stop();
            driverField.SetValue(motor, previousDriver); readyField.SetValue(motor, previousReady); group.CanOperate = previousCanOperate;
            lock (SDKPara.ControlList) { SDKPara.ControlList.Clear(); SDKPara.ControlList.AddRange(previousControls); }
            SwitchLanguage(previousLanguage); Thread.CurrentThread.CurrentUICulture = previousCulture;
            timer.Enabled = previousTimer; driver.Dispose();
        }
    }

    /// <summary>Start homing through the group button, stop through either application path and reject only remaining physical activity.</summary>
    private static void VerifyStoppedManualHome(NP_AxisGroup group, RuntimeManualAxisDriver driver, bool mainStop, bool holdAfterStop)
    {
        driver.Busy = false; driver.HoldAfterStop = holdAfterStop;
        Button home = (Button)group.Controls.Find("axisHome", true).Single();
        typeof(Button).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(home, new object[] { EventArgs.Empty });
        PollManualAxis(group);
        Assert(group.IsManualRunning && driver.Busy, "The offline home button did not start the manual driver session.");
        int homeCalls = driver.HomeCalls, stops = driver.Stops;
        if (mainStop) host.GetMethod("StopAllMotor", BindingFlags.Static | BindingFlags.Public).Invoke(null, null);
        else group.Motor.Stop();
        PollManualAxis(group); PollManualAxis(group);
        Assert(driver.Stops == stops + 1 && driver.HomeCalls == homeCalls,
            "A stopped manual home was restarted by the group refresh timer or the driver was not stopped exactly once.");
        Assert(group.IsManualRunning == holdAfterStop && driver.Busy == holdAfterStop,
            "The group lost actual driver motion or retained a stopped manual session.");
        if (holdAfterStop) VerifyManualAxisBusyMessage(group);
        driver.Busy = false; PollManualAxis(group);
        Assert(!group.IsManualRunning, "A settled driver still has a stale manual-home session.");
        hardware.Resources.ValidateStart();
    }

    /// <summary>Invoke the real UI refresh callback without showing a machine form or allowing asynchronous timer races.</summary>
    private static void PollManualAxis(NP_AxisGroup group)
    {
        ((System.Windows.Forms.Timer)typeof(NP_AxisGroup).GetField("refresh", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(group)).Stop();
        typeof(NP_AxisGroup).GetMethod("RefreshAxis", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(group, new object[] { null, EventArgs.Empty });
    }

    /// <summary>Preserve physical-motion interlocks and identify the blocked axis using the actual three-language hardware message.</summary>
    private static void VerifyManualAxisBusyMessage(NP_AxisGroup group)
    {
        foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
        {
            SwitchLanguage(language);
            string actual = null;
            try { hardware.Resources.ValidateStart(); } catch (InvalidOperationException error) { actual = error.Message; }
            string expected = string.Format(HardwareText.Words["manualBusy"][(int)language], group.AxisCaption);
            Assert(actual == expected, "A physically busy axis was not identified in the hardware startup guard: " + (actual ?? "<allowed>"));
        }
    }

    /// <summary>Capture and dismiss only native dialogs owned by this regression thread while the actual guard is called.</summary>
    private static string CaptureMachineStartDialog(bool initialize, out bool allowed)
    {
        uint thread = GetCurrentThreadId();
        string message = null;
        using (CancellationTokenSource finished = new CancellationTokenSource())
        {
            Task observer = Task.Run(() =>
            {
                DateTime deadline = DateTime.UtcNow.AddSeconds(8);
                while (!finished.IsCancellationRequested)
                {
                    EnumThreadWindows(thread, (window, unused) => CloseRuntimeDialog(window, ref message), IntPtr.Zero);
                    if (DateTime.UtcNow >= deadline)
                    {
                        Console.Error.WriteLine("Machine-start regression could not close its own modal dialog.");
                        Environment.Exit(1);
                    }
                    Thread.Sleep(20);
                }
            });
            try { allowed = FlowPrograms.CanStartMachine(initialize); }
            finally { finished.Cancel(); observer.GetAwaiter().GetResult(); }
        }
        return message;
    }

    /// <summary>Dismiss a native MessageBox already scoped to this test's calling thread.</summary>
    private static bool CloseRuntimeDialog(IntPtr window, ref string message)
    {
        if (NativeWindowClass(window) != "#32770") return true;
        message = ReadRuntimeDialogBody(window).Replace("\r\n", "\n");
        PostMessage(window, 0x0010, IntPtr.Zero, IntPtr.Zero);
        return false;
    }

    /// <summary>Read static body text only inside an already identified regression-thread dialog.</summary>
    private static string ReadRuntimeDialogBody(IntPtr window)
    {
        StringBuilder body = new StringBuilder();
        EnumChildWindows(window, (child, state) =>
        {
            if (NativeWindowClass(child) != "Static") return true;
            string value = NativeWindowText(child);
            if (value.Length == 0) return true;
            if (body.Length != 0) body.AppendLine();
            body.Append(value);
            return true;
        }, IntPtr.Zero);
        return body.ToString();
    }

    /// <summary>Read a native class name without enumerating another process or thread.</summary>
    private static string NativeWindowClass(IntPtr window)
    {
        StringBuilder value = new StringBuilder(128); GetClassName(window, value, value.Capacity); return value.ToString();
    }

    /// <summary>Read native dialog text from a window already identified on the regression thread.</summary>
    private static string NativeWindowText(IntPtr window)
    {
        StringBuilder value = new StringBuilder(GetWindowTextLength(window) + 1); GetWindowText(window, value, value.Capacity); return value.ToString();
    }

    private delegate bool RuntimeWindowCallback(IntPtr window, IntPtr state);

    /// <summary>Identify the OS thread executing this isolated WinForms regression.</summary>
    [DllImport("kernel32.dll")] private static extern uint GetCurrentThreadId();
    /// <summary>Enumerate top-level windows only for the specified regression thread.</summary>
    [DllImport("user32.dll")] private static extern bool EnumThreadWindows(uint thread, RuntimeWindowCallback callback, IntPtr state);
    /// <summary>Enumerate controls of an already scoped regression dialog.</summary>
    [DllImport("user32.dll")] private static extern bool EnumChildWindows(IntPtr parent, RuntimeWindowCallback callback, IntPtr state);
    /// <summary>Read the class of a scoped native window.</summary>
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetClassName(IntPtr window, StringBuilder value, int capacity);
    /// <summary>Read the text of a scoped native window.</summary>
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetWindowText(IntPtr window, StringBuilder value, int capacity);
    /// <summary>Get the buffer length for scoped native dialog text.</summary>
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetWindowTextLength(IntPtr window);
    /// <summary>Post a close command only to a dialog found on the regression thread.</summary>
    [DllImport("user32.dll")] private static extern bool PostMessage(IntPtr window, uint message, IntPtr parameter, IntPtr data);
}
