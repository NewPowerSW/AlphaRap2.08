using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlphaRap;
using AlphaRap.FlowProgramming;
using NPSDK;

internal abstract class NativeEventParent : Flow_BaseForm
{
    internal int ParentCalls;

    /// <summary>Represent an existing private event implemented by an intermediate station class.</summary>
    private Flow_Chart.ResultType Parent_FlowChartRun() { ParentCalls++; return Flow_Chart.ResultType.NEXT; }
}

internal sealed class NativeUnifiedPage : Flow_BaseForm
{
    internal readonly Flow_Chart InitialFirst = new Flow_Chart { Name = "unifiedInitialFirst", text = "Initialize first", Location = new Point(45, 35) };
    internal readonly Flow_Chart InitialLast = new Flow_Chart { Name = "unifiedInitialLast", text = "Initialize last", Location = new Point(45, 115) };
    internal readonly Flow_Chart AutoFirst = new Flow_Chart { Name = "unifiedAutoFirst", text = "Automatic first", Location = new Point(75, 55) };
    internal readonly Flow_Chart AutoLast = new Flow_Chart { Name = "unifiedAutoLast", text = "Automatic last", Location = new Point(75, 155) };
    internal readonly List<string> Calls = new List<string>();
    internal int LegacyCalls;

    /// <summary>Create original native chains on the SDK initialization and automatic pages.</summary>
    internal NativeUnifiedPage(string name, bool ambiguous = false, bool empty = false)
    {
        Name = name;
        if (empty)
        {
            foreach (Flow_Chart node in new[] { InitialFirst, InitialLast, AutoFirst, AutoLast }) node.Dispose();
            return;
        }
        InitialFirst.FlowChartRun += InitializeFirst;
        InitialLast.FlowChartRun += InitializeLast;
        AutoFirst.FlowChartRun += AutomaticFirst;
        AutoLast.FlowChartRun += AutomaticLast;
        InitialFirst.Next = InitialLast;
        if (!ambiguous) AutoFirst.Next = AutoLast;
        InitialPage.Controls.AddRange(new Control[] { InitialFirst, InitialLast });
        AutoPage.Controls.AddRange(new Control[] { AutoFirst, AutoLast });
    }

    /// <summary>Record the original initialization entry without physical hardware.</summary>
    private Flow_Chart.ResultType InitializeFirst() { Calls.Add("initial.first"); return Flow_Chart.ResultType.NEXT; }

    /// <summary>Record the second original initialization function.</summary>
    private Flow_Chart.ResultType InitializeLast() { Calls.Add("initial.last"); return Flow_Chart.ResultType.NEXT; }

    /// <summary>Record the original automatic entry without physical hardware.</summary>
    private Flow_Chart.ResultType AutomaticFirst() { Calls.Add("auto.first"); return Flow_Chart.ResultType.NEXT; }

    /// <summary>Record the second original automatic function.</summary>
    private Flow_Chart.ResultType AutomaticLast() { Calls.Add("auto.last"); return Flow_Chart.ResultType.NEXT; }

    /// <summary>Detect any initialization fallback outside the unified controller.</summary>
    public override void Initial() { LegacyCalls++; }

    /// <summary>Detect any automatic fallback outside the unified controller.</summary>
    public override void StartRun() { LegacyCalls++; }

    /// <summary>Detect any pause fallback outside the unified controller.</summary>
    public override void PauseRun() { LegacyCalls++; }

    /// <summary>Detect any resume fallback outside the unified controller.</summary>
    public override void ContinusAftePause() { LegacyCalls++; }

    /// <summary>Detect any stop fallback outside the unified controller.</summary>
    public override void StopRun() { LegacyCalls++; }

    /// <summary>Release an isolated page without leaving it in the SDK module catalogue.</summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing) Flow_Module.FlowChart_ModuleList.Remove(this);
        base.Dispose(disposing);
    }
}

internal sealed class NativeEventPage : NativeEventParent
{
    internal int Calls, Polls, BoundCalls, OtherBoundCalls, GetterReads, SimulationCalls;
    internal int IdleBeforeExit;
    internal int UiThread;
    internal int CallbackThread;
    internal bool OwnedDuringCall;
    internal Flow_Chart.ResultType SelectedExit = Flow_Chart.ResultType.NEXT;
    internal readonly ManualResetEventSlim BlockStarted = new ManualResetEventSlim();
    internal readonly ManualResetEventSlim BlockRelease = new ManualResetEventSlim();
    internal readonly Panel Canvas = new Panel { Name = "nativeCanvas", Dock = DockStyle.Fill };
    internal readonly Flow_Chart BoundNode = new Flow_Chart { Name = "boundNode", text = "Existing native event" };
    internal readonly NPSDK.Motion.NP_Input LocalInput = new NPSDK.Motion.NP_Input { Name = "localInput", Text = "Native local input" };

    /// <summary>Create actual bound SDK events without a separate catalogue or tool registration.</summary>
    internal NativeEventPage(string name, bool multicast = false)
    {
        Name = name; UiThread = Thread.CurrentThread.ManagedThreadId;
        BoundNode.FlowChartRun += BoundBusiness;
        if (multicast) BoundNode.FlowChartRun += OtherBoundBusiness;
        Canvas.Controls.Add(BoundNode); Canvas.Controls.Add(LocalInput); Controls.Add(Canvas);
    }

    /// <summary>Detect arbitrary property execution while the toolbox builds metadata.</summary>
    public Flow_Chart.ResultType Diagnostic { get { GetterReads++; throw new InvalidOperationException("Native metadata read a property."); } }

    /// <summary>Expose the exact original parameterless FlowChartRun programming pattern.</summary>
    public Flow_Chart.ResultType Start_FlowChartRun()
    {
        Calls++; CallbackThread = Thread.CurrentThread.ManagedThreadId;
        return SelectedExit;
    }

    /// <summary>Poll one native node until its original event chooses an SDK result.</summary>
    private Flow_Chart.ResultType Poll_FlowChartRun()
    {
        int current = Interlocked.Increment(ref Polls);
        CallbackThread = Thread.CurrentThread.ManagedThreadId;
        return current <= IdleBeforeExit ? Flow_Chart.ResultType.IDLE : SelectedExit;
    }

    /// <summary>Keep an existing event whose method name does not use the naming convention.</summary>
    private Flow_Chart.ResultType BoundBusiness() { BoundCalls++; return Flow_Chart.ResultType.NEXT; }

    /// <summary>Represent the second callback of an ambiguous existing multicast event.</summary>
    private Flow_Chart.ResultType OtherBoundBusiness() { OtherBoundCalls++; return Flow_Chart.ResultType.NEXT; }

    /// <summary>Represent a non-cooperative original callback so stop ownership can be checked.</summary>
    private Flow_Chart.ResultType Blocking_FlowChartRun()
    {
        Calls++; OwnedDuringCall = FlowResourceGate.CanAccess(this); BlockStarted.Set(); BlockRelease.Wait();
        return Flow_Chart.ResultType.NEXT;
    }

    /// <summary>Represent an invalid application return value rather than a supported SDK exit.</summary>
    private Flow_Chart.ResultType Invalid_FlowChartRun() { Calls++; return (Flow_Chart.ResultType)777; }

    /// <summary>Use an optional simulator without replacing the actual native function.</summary>
    [FlowTool("可模拟原生节点", "Simulated native node", "Nodo nativo simulable", SimulationMethod = "PreviewNative")]
    private Flow_Chart.ResultType Simulated_FlowChartRun() { Calls++; return Flow_Chart.ResultType.CASE1; }

    /// <summary>Keep a simulator out of the toolbox while retaining its explicit callable signature.</summary>
    [FlowToolIgnore]
    private Flow_Chart.ResultType PreviewNative() { SimulationCalls++; return Flow_Chart.ResultType.NEXT; }

    /// <summary>Keep a callable helper out of the catalogue through the shared optional exclusion attribute.</summary>
    [FlowToolIgnore]
    public Flow_Chart.ResultType Hidden_FlowChartRun() { return Flow_Chart.ResultType.NEXT; }

    /// <summary>Represent an unrelated result-shaped helper that is not a native event.</summary>
    public Flow_Chart.ResultType OrdinaryHelper() { return Flow_Chart.ResultType.NEXT; }

    /// <summary>Keep compiler-generated closure methods out of persistent native identities.</summary>
    internal Flow_Chart.FlowChartHandle MakeClosure() { return () => { Calls++; return Flow_Chart.ResultType.NEXT; }; }

    /// <summary>Represent an unsupported parameterized callback.</summary>
    public Flow_Chart.ResultType Wrong_FlowChartRun(int value) { return Flow_Chart.ResultType.NEXT; }

    /// <summary>Represent an unsupported static callback.</summary>
    public static Flow_Chart.ResultType Static_FlowChartRun() { return Flow_Chart.ResultType.NEXT; }

    /// <summary>Represent an unsupported generic callback.</summary>
    public Flow_Chart.ResultType Generic_FlowChartRun<T>() { return Flow_Chart.ResultType.NEXT; }

    /// <summary>Always release a blocked offline fixture before disposing its UI.</summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing) BlockRelease.Set();
        base.Dispose(disposing);
    }
}

internal static partial class Cases
{
    /// <summary>Register original event programming alongside ordinary configured equipment actions.</summary>
    private static void RegisterNativeEventCases()
    {
        RegisterNativeAlarmCases();
        RegisterSourceControlCases();
        RegisterRuntimeStateCases();
        Test("native events discover private inherited and bound callbacks without executing business code", NativeEventDiscovery);
        Test("native event toolbox is limited to its current form and preserves identities in three languages", NativeEventToolbox);
        Test("existing single events import automatically and multicast events require one explicit callback", NativeBoundImport);
        Test("ordinary data actions native IDLE and CASE1 and DataForm save share one execution session", NativeMixedProduction);
        Test("native SDK NEXT CASE1 CASE2 CASE3 and End results control their configured exits", NativeResultRoutes);
        Test("native definitions survive shared Save reload and reconstructed form instances", NativeEventPersistence);
        Test("removed native methods and foreign form targets cannot fall back to a same-named callback", NativeMissingIdentity);
        Test("native nodes use normal debugging breakpoints and a single step includes all IDLE polls", NativeEventDebug);
        Test("pausing a native IDLE node stops future callback polling until resume", NativeEventPause);
        Test("stopping a native callback waits for its actual invocation and discards delayed output", NativeEventStop);
        Test("native IDLE polling shares one deadline and undeclared exits report one alarm", NativeEventFailures);
        Test("native simulation uses the declared preview and rejects undeclared hardware execution", NativeEventSimulation);
        Test("actual flow page selection refreshes native tools and native editor exposes targets without duplicate conditions", NativePageAndEditor);
        Test("native nodes block double clicks and property edits while preserving other node editors dragging and breakpoints", NativeDoubleClickPolicy);
        Test("unified flow imports native page roots and original links without invoking legacy lifecycle methods", NativeUnifiedImport);
        Test("saved Legacy definitions migrate to one mixed flow while preserving explicit entries positions and native identities", NativeUnifiedReload);
        Test("ambiguous native page roots block validation and starting until an entry is selected", NativeUnifiedAmbiguousEntry);
        Test("disabled and empty unified modules consume lifecycle commands without legacy fallbacks", NativeUnifiedDisabledAndEmpty);
        Test("published output bindings stay isolated from unsaved producer method and output changes", NativePublishedOutputIsolation);
        Test("removed original controls survive discard and are released with their owning form", NativeRemovedControlDisposal);
        Test("a failed project load cannot dispatch native production or publish a replacement project", NativeFailedProjectLoad);
    }

    /// <summary>Build one isolated editor without replacing the global regression project or saved machine configuration.</summary>
    private static void WithUnifiedNativePage(Action<NativeUnifiedPage, FlowModuleEditor> test, bool ambiguous = false, bool empty = false)
    {
        bool simulation = SysPara.Simulation;
        SysPara.Simulation = false;
        try
        {
            using (NativeUnifiedPage page = new NativeUnifiedPage("NativeUnified_" + Guid.NewGuid().ToString("N"), ambiguous, empty))
            {
                FlowProgram plan = new FlowProgram { ModuleId = page.GetType().FullName + "/" + page.Name, Mode = "Legacy" };
                FlowModuleEditor owner = new FlowModuleEditor(page, plan);
                page.ProgramController = owner;
                try { test(page, owner); }
                finally { owner.Runtime.Stop(); Wait(owner.Runtime.Completion); }
            }
        }
        finally { SysPara.Simulation = simulation; }
    }

    /// <summary>Reject production and saving after an initial project-load error without falling back to native lifecycle code.</summary>
    private static void NativeFailedProjectLoad()
    {
        WithNativeEvents((page, owner, actions) =>
        {
            FlowStep native = NativeNode(page, owner, actions, "Start_FlowChartRun");
            owner.Publish();
            FieldInfo errorField = typeof(FlowPrograms).GetField("loadError", BindingFlags.Static | BindingFlags.NonPublic);
            object previous = errorField.GetValue(null);
            try
            {
                errorField.SetValue(null, new InvalidDataException("Broken project fixture"));
                foreach (Action operation in new Action[] { () => owner.ValidatePublished(), () => owner.Start(native.Id, false, false, true), () => FlowPrograms.ValidateForSave() })
                {
                    bool rejected = false;
                    try { operation(); } catch (InvalidDataException) { rejected = true; }
                    Assert(rejected && page.Calls == 0 && !owner.Runtime.IsRunning, "An invalid project load allowed production or replacement saving.");
                }
            }
            finally { errorField.SetValue(null, previous); }
        });
    }

    /// <summary>Keep removed original nodes for undo only while the actual page remains alive.</summary>
    private static void NativeRemovedControlDisposal()
    {
        NativeEventPage page = new NativeEventPage("NativeDisposal_" + Guid.NewGuid().ToString("N"));
        try
        {
            FlowModuleEditor owner = FlowPrograms.Attach(page);
            FlowProgram before = FlowProgramFile.Snapshot(owner.Program);
            FlowStep step = owner.Program.Steps.Single(item => item.ControlName == page.BoundNode.Name);
            owner.Delete(step.Id);
            Assert(!page.BoundNode.IsDisposed && page.BoundNode.Parent == null, "Removing an original node destroyed its undo state.");
            owner.Restore(before);
            Assert(!page.BoundNode.IsDisposed && ReferenceEquals(page.BoundNode.Parent, page.Canvas), "Discard did not restore the original node instance.");
            owner.Delete(step.Id); page.Dispose();
            Assert(page.BoundNode.IsDisposed, "Closing the actual form leaked a removed original node.");
        }
        finally { page.Dispose(); }
    }

    /// <summary>Use the same controller-first command routing as the SDK machine lifecycle.</summary>
    private static bool DispatchUnifiedNative(NativeUnifiedPage page, FlowProgramCommand command)
    {
        if (page.TryProgramCommand(command)) return true;
        if (command == FlowProgramCommand.Initialize) page.Initial();
        if (command == FlowProgramCommand.Start) page.StartRun();
        if (command == FlowProgramCommand.Pause) page.PauseRun();
        if (command == FlowProgramCommand.Resume) page.ContinusAftePause();
        if (command == FlowProgramCommand.Stop) page.StopRun();
        return false;
    }

    /// <summary>Import initialization and automatic roots independently and run their actual original native handlers.</summary>
    private static void NativeUnifiedImport()
    {
        WithUnifiedNativePage((page, owner) =>
        {
            FlowStep initial = owner.Program.Steps.Single(step => step.ControlName == page.InitialFirst.Name);
            FlowStep initialLast = owner.Program.Steps.Single(step => step.ControlName == page.InitialLast.Name);
            FlowStep automatic = owner.Program.Steps.Single(step => step.ControlName == page.AutoFirst.Name);
            FlowStep autoLast = owner.Program.Steps.Single(step => step.ControlName == page.AutoLast.Name);
            Assert(owner.Program.Mode == "Program" && owner.IsEnabled && owner.IsProgram,
                "A newly imported native page retained the old execution mode or left its controller disabled.");
            Assert(owner.Program.InitialEntry == initial.Id && owner.Program.AutoEntry == automatic.Id
                && initial.Next == initialLast.Id && automatic.Next == autoLast.Id,
                "Unique original roots were not inferred independently or original native links were changed.");
            Assert(owner.Program.Steps.All(step => step.UsesFlowResult) && page.Calls.Count == 0 && page.LegacyCalls == 0,
                "Importing a native page executed a callback or omitted its native return contract.");
            owner.Validate(null);
            Assert(DispatchUnifiedNative(page, FlowProgramCommand.Initialize), "Initialization fell back to the original lifecycle override.");
            Wait(owner.Runtime.Completion);
            Assert(DispatchUnifiedNative(page, FlowProgramCommand.Start), "Automatic execution fell back to the original lifecycle override.");
            Wait(owner.Runtime.Completion);
            Assert(owner.Runtime.Error == null && page.Calls.SequenceEqual(new[] { "initial.first", "initial.last", "auto.first", "auto.last" })
                && page.LegacyCalls == 0, "Unified lifecycle execution skipped, repeated or replaced an original native callback.");
        });
    }

    /// <summary>Deserialize an old mode without changing saved bindings, logical layout, links or explicit entry selections.</summary>
    private static void NativeUnifiedReload()
    {
        WithUnifiedNativePage((page, owner) =>
        {
            FlowStep initialFirst = owner.Program.Steps.Single(step => step.ControlName == page.InitialFirst.Name);
            FlowStep initialLast = owner.Program.Steps.Single(step => step.ControlName == page.InitialLast.Name);
            FlowStep automaticFirst = owner.Program.Steps.Single(step => step.ControlName == page.AutoFirst.Name);
            FlowStep automaticLast = owner.Program.Steps.Single(step => step.ControlName == page.AutoLast.Name);
            owner.AddNode("", "value", page.AutoPage, new Point(450, 55)).Inputs[0].Reference = "Separate draft node";
            page.AutoFirst.Location = new Point(320, 410);
            owner.CaptureLayout();
            FlowProgram legacy = FlowProgramFile.Snapshot(owner.Program);
            legacy.Mode = "Legacy";
            legacy.InitialEntry = null;
            legacy.AutoEntry = null;
            string identities = string.Join("|", legacy.Steps.Select(step => step.Id + ":" + step.Target + ":" + step.Action));
            string file = Path.Combine(root, "native-unified-legacy-" + Guid.NewGuid().ToString("N") + ".xml");
            FlowProgramFile.Save(file, new FlowProject { Modules = new List<FlowProgram> { legacy } });
            FlowProgram loaded = FlowProgramFile.Load(file).Modules.Single();
            owner.Restore(loaded); owner.Publish();
            FlowStep moved = owner.Program.Steps.Single(step => step.ControlName == page.AutoFirst.Name);
            Assert(owner.Program.Mode == "Program" && owner.Program.InitialEntry == initialFirst.Id && owner.Program.AutoEntry == automaticFirst.Id,
                "Loading a Legacy definition retained the old mode or included unrelated draft nodes when inferring original page roots.");
            Assert(new Point(moved.X, moved.Y) == new Point(320, 410) && page.AutoFirst.Location == new Point(320, 410)
                && ReferenceEquals(page.AutoFirst.Next, page.AutoLast) && owner.Program.Steps.Count(step => step.UsesFlowResult) == 4
                && string.Join("|", owner.Program.Steps.Select(step => step.Id + ":" + step.Target + ":" + step.Action)) == identities,
                "Legacy migration changed saved node positions, native identities, SDK return contracts or original connections.");
            FlowProgram explicitEntries = FlowProgramFile.Snapshot(owner.Program);
            explicitEntries.Mode = "Legacy"; explicitEntries.InitialEntry = initialLast.Id; explicitEntries.AutoEntry = automaticLast.Id;
            owner.Restore(explicitEntries); owner.Publish();
            Assert(owner.Program.InitialEntry == initialLast.Id && owner.Program.AutoEntry == automaticLast.Id,
                "Legacy migration replaced explicitly saved initialization or automatic entry choices.");
            owner.Validate(null);
            Assert(page.Calls.Count == 0 && page.LegacyCalls == 0, "Loading or validating a Legacy file executed application logic.");
        });
    }

    /// <summary>Reject multiple original roots before executing any handler, then honor the engineer's explicit selection.</summary>
    private static void NativeUnifiedAmbiguousEntry()
    {
        WithUnifiedNativePage((page, owner) =>
        {
            Assert(!string.IsNullOrEmpty(owner.Program.InitialEntry) && string.IsNullOrEmpty(owner.Program.AutoEntry),
                "An ambiguous automatic page guessed its entry or prevented independent initialization-root inference.");
            bool invalid = false;
            try { owner.Validate(null); } catch (InvalidDataException) { invalid = true; }
            Assert(invalid, "A page with multiple automatic roots passed validation without an explicit entry.");
            bool startBlocked = false;
            try { DispatchUnifiedNative(page, FlowProgramCommand.Start); } catch (InvalidDataException) { startBlocked = true; }
            Assert(startBlocked && !owner.Runtime.IsRunning && page.Calls.Count == 0 && page.LegacyCalls == 0,
                "Ambiguous automatic execution started a native node or fell back to the original lifecycle code.");
            owner.Program.AutoEntry = owner.Program.Steps.Single(step => step.ControlName == page.AutoLast.Name).Id;
            owner.Validate(null); owner.Publish();
            Assert(DispatchUnifiedNative(page, FlowProgramCommand.Start), "An explicitly repaired entry was not owned by the unified controller.");
            Wait(owner.Runtime.Completion);
            Assert(owner.Runtime.Error == null && page.Calls.SequenceEqual(new[] { "auto.last" }) && page.LegacyCalls == 0,
                "Explicit entry repair executed another root or invoked the old lifecycle override.");
        }, true);
    }

    /// <summary>Keep disabled and empty modules under the same controller so SDK command dispatch cannot reactivate old code.</summary>
    private static void NativeUnifiedDisabledAndEmpty()
    {
        WithUnifiedNativePage((page, owner) =>
        {
            owner.Program.Mode = "Disabled"; owner.Publish(); owner.Validate(null);
            Assert(owner.IsEnabled && !owner.IsProgram, "Disabling a module detached its controller and allowed legacy fallback.");
            foreach (FlowProgramCommand command in new[] { FlowProgramCommand.Initialize, FlowProgramCommand.Start, FlowProgramCommand.Pause, FlowProgramCommand.Resume, FlowProgramCommand.Stop })
                Assert(DispatchUnifiedNative(page, command), "A disabled module did not consume its lifecycle command: " + command);
            Assert(page.Calls.Count == 0 && page.LegacyCalls == 0 && !owner.Runtime.IsRunning && !page.RunningStatus,
                "A disabled unified module invoked a native callback, started a runtime or fell back to old lifecycle code.");
        });
        WithUnifiedNativePage((page, owner) =>
        {
            Assert(owner.Program.Mode == "Program" && owner.IsEnabled && owner.Program.Steps.Count == 0
                && string.IsNullOrEmpty(owner.Program.InitialEntry) && string.IsNullOrEmpty(owner.Program.AutoEntry),
                "An empty unified module invented entries or retained the old execution mode.");
            owner.Validate(null);
            Assert(DispatchUnifiedNative(page, FlowProgramCommand.Initialize) && DispatchUnifiedNative(page, FlowProgramCommand.Start)
                && page.LegacyCalls == 0 && !owner.Runtime.IsRunning, "An empty module required an entry or fell back to the original lifecycle code.");
        }, false, true);
    }

    /// <summary>Validate and run the saved producer contract while the designer holds an incompatible unsaved method.</summary>
    private static void NativePublishedOutputIsolation()
    {
        WithNativeEvents((page, owner, actions) =>
        {
            FlowStep producer = owner.AddNode("", "value", page.Canvas, new Point(30, 80));
            producer.Inputs[0].Reference = "published-value";
            FlowStep consumer = owner.AddNode("", "value", page.Canvas, new Point(30, 160));
            consumer.Inputs[0].Source = "Output";
            consumer.Inputs[0].Reference = owner.Program.ModuleId + "/" + producer.Id + "/Value";
            producer.Next = consumer.Id;
            owner.Program.AutoEntry = producer.Id;
            owner.Validate(null);
            owner.Publish();

            producer.Action = "delay";
            producer.Inputs = new List<FlowInput> { new FlowInput { Name = "Milliseconds", Reference = "0" } };
            bool draftRejected = false;
            try { owner.Validate(null); }
            catch (InvalidOperationException) { draftRejected = true; }
            Assert(draftRejected, "An unsaved producer method with no Value output passed draft validation.");
            owner.ValidatePublished();

            FlowPrograms.BeginBatch();
            owner.Start(producer.Id, false, false, true);
            Wait(owner.Runtime.Completion);
            Assert(owner.Runtime.Error == null && Convert.ToString(FlowPrograms.Results.Read(FlowPrograms.Results.Epoch, "batch",
                owner.Program.ModuleId + "/" + consumer.Id, "Value")) == "published-value",
                "Published validation or execution resolved the saved consumer against an unsaved producer output contract: " + owner.Runtime.Error);
        });
    }

    /// <summary>Create and release an independent current-form editor through the real framework attachment path.</summary>
    private static void WithNativeEvents(Action<NativeEventPage, FlowModuleEditor, FlowActions> test, bool multicast = false)
    {
        using (NativeEventPage page = new NativeEventPage("NativeEvents_" + Guid.NewGuid().ToString("N"), multicast))
        {
            FlowModuleEditor owner = FlowPrograms.Attach(page);
            try { test(page, owner, new FlowActions(FlowResources.Capture(true), page)); }
            finally { page.BlockRelease.Set(); owner.Runtime.Stop(); Wait(owner.Runtime.Completion); }
        }
    }

    /// <summary>Resolve only the catalogue owned by the exact current form instance.</summary>
    private static FlowResource NativeResource(FlowActions actions, NativeEventPage page)
    {
        return actions.Resources.Items.Values.Single(item => item.Kind == "native" && ReferenceEquals(item.Instance, page));
    }

    /// <summary>Find a native event by its original code identifier without depending on its hashed persistent ID.</summary>
    private static FlowMethod NativeMethod(FlowActions actions, NativeEventPage page, string name)
    {
        return NativeResource(actions, page).Methods.Single(method => method.UsesFlowResult && method.Id.StartsWith("native." + name + ".", StringComparison.Ordinal));
    }

    /// <summary>Insert the same real tool entry used by drag-and-drop and return its editable draft.</summary>
    private static FlowStep NativeNode(NativeEventPage page, FlowModuleEditor owner, FlowActions actions, string name)
    {
        FlowResource resource = NativeResource(actions, page);
        FlowStep step = owner.AddNode(resource.Id, NativeMethod(actions, page, name).Id, page.Canvas, new Point(30, 80));
        Assert(step.UsesFlowResult && step.Inputs.Count == 0, "Dragging a native callback lost its SDK result contract or invented inputs.");
        return step;
    }

    /// <summary>Build an alarm-observed runtime using real captured object dispatch without showing an alarm window.</summary>
    private static FlowProgramRuntime NativeRuntime(FlowActions actions)
    {
        return new FlowProgramRuntime { ExecuteStep = actions.ExecuteOutcome };
    }

    /// <summary>Wait for a bounded observation while continuing to process actual WinForms callbacks.</summary>
    private static void NativePump(int milliseconds)
    {
        Stopwatch watch = Stopwatch.StartNew();
        while (watch.ElapsedMilliseconds < milliseconds) { Application.DoEvents(); Thread.Sleep(5); }
    }

    /// <summary>Check discovery contracts without property execution or accidental generic/static helper exposure.</summary>
    private static void NativeEventDiscovery()
    {
        WithNativeEvents((page, owner, actions) =>
        {
            FlowResource resource = NativeResource(actions, page);
            foreach (string name in new[] { "Parent_FlowChartRun", "Start_FlowChartRun", "Poll_FlowChartRun", "BoundBusiness", "Blocking_FlowChartRun" })
                Assert(NativeMethod(actions, page, name).Inputs.Count == 0, "An inherited, private or bound SDK callback was omitted: " + name);
            Assert(NativeMethod(actions, page, "OrdinaryHelper") != null, "An exact native signature incorrectly required a naming convention.");
            foreach (string name in new[] { "Hidden_FlowChartRun", "Wrong_FlowChartRun", "Static_FlowChartRun", "Generic_FlowChartRun", "Diagnostic" })
                Assert(!resource.Methods.Any(method => method.Title[0].Contains(name)), "A non-event helper entered the native tools: " + name);
            string[] identities = resource.Methods.Select(method => method.Id).ToArray();
            FlowResource again = NativeResource(new FlowActions(FlowResources.Capture(true), page), page);
            Assert(identities.SequenceEqual(again.Methods.Select(method => method.Id)), "Repeated native captures changed method identities or ordering.");
            Assert(page.Calls == 0 && page.Polls == 0 && page.ParentCalls == 0 && page.BoundCalls == 0 && page.GetterReads == 0,
                "Building or importing native metadata executed user code.");
            Assert(resource.Methods.All(method => method.UsesFlowResult && method.Outputs.Count == 1
                && method.Outputs[0].Name == "Result" && method.Outputs[0].Type == typeof(Flow_Chart.ResultType)), "Native observation output does not match the SDK result enum.");
            Assert(resource.Methods.All(method => !method.Id.Contains("<")), "A compiler-generated lambda became a persistent tool identity.");
            FlowStep inherited = NativeNode(page, owner, actions, "Parent_FlowChartRun");
            owner.Start(inherited.Id, false, false); Wait(owner.Runtime.Completion);
            Assert(owner.Runtime.Error == null && page.ParentCalls == 1, "The actual inherited private callback did not run.");
        });
    }

    /// <summary>Keep each station's callbacks local while translating and inserting through the normal toolbox command.</summary>
    private static void NativeEventToolbox()
    {
        WithNativeEvents((page, owner, actions) =>
        {
            using (NativeEventPage other = new NativeEventPage("OtherNative_" + Guid.NewGuid().ToString("N")))
            using (FlowToolboxControl toolbox = new FlowToolboxControl())
            {
                FlowPrograms.Attach(other);
                FlowActions current = new FlowActions(FlowResources.Capture(true), page);
                toolbox.BindActions(current);
                TreeView tree = ActionControl<TreeView>(toolbox, "toolboxEntries");
                TextBox search = ActionControl<TextBox>(toolbox, "toolboxSearch");
                string target = NativeResource(current, page).Id;
                string foreign = NativeResource(new FlowActions(FlowResources.Capture(true), other), other).Id;
                string action = NativeMethod(current, page, "Poll_FlowChartRun").Id;
                string identity = null;
                foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
                {
                    SwitchLanguage(language); Application.DoEvents();
                    FlowToolboxEntry[] entries = tree.Nodes.Cast<TreeNode>().SelectMany(group => group.Nodes.Cast<TreeNode>()).Select(node => (FlowToolboxEntry)node.Tag).ToArray();
                    FlowToolboxEntry selected = entries.Single(entry => entry.TargetId == target && entry.ActionId == action);
                    Assert(!entries.Any(entry => entry.TargetId == foreign) && (identity == null || selected.Id == identity), "Toolbox adopted another form's callback or changed its stable identity during translation.");
                    identity = selected.Id;
                    search.Text = "Poll_FlowChartRun"; toolbox.RefreshLanguage();
                    Assert(search.Text == "Poll_FlowChartRun" && tree.Nodes.Cast<TreeNode>().SelectMany(group => group.Nodes.Cast<TreeNode>()).Count() == 1,
                        "Native tool search was cleared or did not match its readable method name.");
                }
                FlowToolboxEntry inserted = null; toolbox.InsertRequested += entry => inserted = entry;
                tree.SelectedNode = tree.Nodes[0].Nodes[0];
                typeof(Control).GetMethod("OnKeyDown", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(tree, new object[] { new KeyEventArgs(Keys.Enter) });
                Assert(inserted != null && inserted.TargetId == target && inserted.ActionId == action, "The toolbox keyboard path did not select the native callback.");
                FlowStep added = owner.AddNode(inserted.TargetId, inserted.ActionId, page.Canvas, new Point(70, 170));
                Assert(added.UsesFlowResult && added.Target == target && page.Polls == 0, "Toolbox insertion executed or replaced the callback.");
            }
        });
        SwitchLanguage(LanguageType.Chinese);
    }

    /// <summary>Import an original single callback and reject ambiguous multicast execution without invoking either handler.</summary>
    private static void NativeBoundImport()
    {
        WithNativeEvents((page, owner, actions) =>
        {
            FlowStep imported = owner.Program.Steps.Single(step => step.ControlName == page.BoundNode.Name);
            Assert(imported.Action == NativeMethod(actions, page, "BoundBusiness").Id && imported.UsesFlowResult,
                "An existing SDK event required manual rebinding or did not use its actual handler.");
            owner.Start(imported.Id, false, false); Wait(owner.Runtime.Completion);
            Assert(owner.Runtime.Error == null && page.BoundCalls == 1, "The imported original event did not invoke its actual form instance.");
        });
        WithNativeEvents((page, owner, actions) =>
        {
            FlowStep ambiguous = owner.Program.Steps.Single(step => step.ControlName == page.BoundNode.Name);
            bool refused = false;
            try { owner.Validate(ambiguous.Id); } catch (InvalidOperationException) { refused = true; }
            Assert(refused && ambiguous.Action == "legacy" && page.BoundCalls == 0 && page.OtherBoundCalls == 0,
                "A multicast event silently selected the final delegate or executed while browsing.");
            Assert(NativeMethod(actions, page, "BoundBusiness") != null && NativeMethod(actions, page, "OtherBoundBusiness") != null,
                "The separate callbacks could not be explicitly chosen after ambiguous import was rejected.");
        }, true);
    }

    /// <summary>Pass ordinary node data through native polling and branching into actual local DataForm production storage.</summary>
    private static void NativeMixedProduction()
    {
        WithNativeEvents((page, owner, actions) =>
        {
            string product = "NATIVE-" + Guid.NewGuid().ToString("N");
            FlowStep read = owner.AddNode("", "value", page.Canvas, new Point(20, 20)); read.Inputs[0].Reference = product;
            FlowStep native = NativeNode(page, owner, actions, "Poll_FlowChartRun");
            FlowStep wrong = owner.AddNode("", "value", page.Canvas, new Point(220, 20)); wrong.Inputs[0].Reference = "WRONG";
            FlowStep record = owner.AddNode("data/Production", "data.save", page.Canvas, new Point(20, 220));
            foreach (FlowInput input in record.Inputs)
                input.Reference = input.Name == "Measurement" ? "2.75" : "";
            FlowInput productField = record.Inputs.Single(input => input.Name == "Product");
            productField.Source = "Output"; productField.Reference = owner.Program.ModuleId + "/" + read.Id + "/Value";
            read.Next = native.Id; native.Next = wrong.Id; native.Case1 = record.Id;
            native.Branches.Add(new FlowBranch { Code = "CASE1" });
            page.IdleBeforeExit = 2; page.SelectedExit = Flow_Chart.ResultType.CASE1;
            owner.Program.AutoEntry = read.Id; owner.Program.Mode = "Program";
            FlowPrograms.BeginBatch(); owner.Start(read.Id, false, false); Wait(owner.Runtime.Completion);
            Assert(owner.Runtime.Error == null && page.Polls == 3 && page.CallbackThread != page.UiThread,
                "Native IDLE did not retry the original callback, routed by configured conditions, or blocked the UI thread: " + owner.Runtime.Error);
            object result = FlowPrograms.Results.Read(FlowPrograms.Results.Epoch, "batch", owner.Program.ModuleId + "/" + native.Id, "Result");
            Assert((Flow_Chart.ResultType)result == Flow_Chart.ResultType.CASE1, "The final native result was not available to later flow observers.");
            int files, total;
            List<ProductionRecord> records = ProductionDataStore.Query(DateTime.Today, DateTime.Today.AddDays(1), "", out files, out total);
            Assert(records.Count(item => item.Get("Product") == product && item.Get("Measurement") == "2.75") == 1,
                "A mixed designer/native chain lost upstream values or failed to save one local record.");
        });
    }

    /// <summary>Preserve each SDK return code without evaluating an unrelated branch's conditions.</summary>
    private static void NativeResultRoutes()
    {
        foreach (Flow_Chart.ResultType selected in new[] { Flow_Chart.ResultType.NEXT, Flow_Chart.ResultType.CASE1, Flow_Chart.ResultType.CASE2, Flow_Chart.ResultType.CASE3, Flow_Chart.ResultType.End })
            WithNativeEvents((page, owner, actions) =>
            {
                page.SelectedExit = selected;
                FlowStep native = NativeNode(page, owner, actions, "Start_FlowChartRun");
                FlowStep following = owner.AddNode("", "value", page.Canvas, new Point(30, 220)); following.Inputs[0].Reference = selected.ToString();
                foreach (string code in new[] { "CASE1", "CASE2", "CASE3" }) native.Branches.Add(new FlowBranch { Code = code });
                native.Next = following.Id; native.Case1 = following.Id; native.Case2 = following.Id; native.Case3 = following.Id;
                FlowResultHub hub = new FlowResultHub(); FlowExecutionContext context = Context(hub); context.ModuleId = owner.Program.ModuleId;
                FlowProgramRuntime runtime = NativeRuntime(actions); int completed = 0;
                runtime.StateChanged += (step, state) => { if (step.Id == following.Id && state == "Completed") completed++; };
                runtime.Start(owner.Program, native.Id, context); Wait(runtime.Completion);
                Assert(runtime.Error == null && page.Calls == 1 && completed == (selected == Flow_Chart.ResultType.End ? 0 : 1),
                    "The native result did not follow its SDK exit: " + selected + " / " + runtime.Error);
            });
    }

    /// <summary>Use the unified saved project and restore the callback identity into a new actual page instance.</summary>
    private static void NativeEventPersistence()
    {
        string name = "NativePersist_" + Guid.NewGuid().ToString("N");
        string target, action, node, ordinaryId;
        Point nativeLocation = new Point(660, 760), ordinaryLocation = new Point(40, 360);
        using (NativeEventPage page = new NativeEventPage(name))
        {
            page.ClientSize = new Size(480, 320); page.Show(); Application.DoEvents();
            FlowModuleEditor owner = FlowPrograms.Attach(page); FlowActions actions = new FlowActions(FlowResources.Capture(true), page);
            FlowStep step = NativeNode(page, owner, actions, "Start_FlowChartRun");
            FlowStep ordinary = owner.AddNode("", "value", page.Canvas, ordinaryLocation);
            ordinary.Inputs[0].Reference = "Before native"; ordinary.Next = step.Id; ordinaryId = ordinary.Id;
            Flow_Chart nativeControl = page.Canvas.Controls.OfType<Flow_Chart>().Single(item => item.Name == step.ControlName);
            Point scroll = page.Canvas.AutoScrollPosition;
            nativeControl.Location = new Point(nativeLocation.X + scroll.X, nativeLocation.Y + scroll.Y);
            page.Canvas.AutoScrollPosition = new Point(0, 200); Application.DoEvents();
            Assert(page.Canvas.AutoScrollPosition.Y < 0, "The mixed-node canvas did not scroll before Save.");
            target = step.Target; action = step.Action; node = step.Id;
            owner.Program.Mode = "Program"; owner.Program.AutoEntry = step.Id;
            FlowPrograms.Save();
            FlowProgram persisted = FlowProgramFile.Load(FlowPrograms.FilePath).Modules.Single(item => item.ModuleId == owner.Program.ModuleId);
            Assert(persisted.Steps.Single(item => item.Id == node).UsesFlowResult && page.Calls == 0, "The shared Save omitted native metadata or executed the callback.");
            FlowStep saved = persisted.Steps.Single(item => item.Id == node);
            FlowStep savedOrdinary = persisted.Steps.Single(item => item.Id == ordinaryId);
            Assert(new Point(saved.X, saved.Y) == nativeLocation && new Point(savedOrdinary.X, savedOrdinary.Y) == ordinaryLocation
                && saved.ParentPath == "/nativeCanvas" && savedOrdinary.ParentPath == saved.ParentPath && savedOrdinary.Next == node,
                "Saving a scrolled mixed-node canvas changed logical positions, parent page or connections.");
            step.Caption = "Unsaved title"; FlowPrograms.Reload();
            Assert(owner.Program.Steps.Single(item => item.Id == node).Caption != "Unsaved title", "Unified reload did not discard the native draft.");
        }
        using (NativeEventPage page = new NativeEventPage(name))
        {
            page.ClientSize = new Size(480, 320); page.Show(); Application.DoEvents();
            FlowModuleEditor owner = FlowPrograms.Attach(page);
            FlowStep restored = owner.Program.Steps.Single(item => item.Id == node);
            Assert(restored.Target == target && restored.Action == action && restored.UsesFlowResult, "The reconstructed form changed its saved native identity.");
            Flow_Chart nativeControl = page.Canvas.Controls.OfType<Flow_Chart>().Single(item => item.Name == restored.ControlName);
            FlowStep restoredOrdinary = owner.Program.Steps.Single(item => item.Id == ordinaryId);
            Flow_Chart ordinaryControl = page.Canvas.Controls.OfType<Flow_Chart>().Single(item => item.Name == restoredOrdinary.ControlName);
            Assert(page.Canvas.AutoScroll && nativeControl.Location == nativeLocation && ordinaryControl.Location == ordinaryLocation
                && ReferenceEquals(ordinaryControl.Next, nativeControl), "Fresh-form loading lost canvas scrolling, mixed-node positions or their visible connection.");
            page.Canvas.ScrollControlIntoView(nativeControl); Application.DoEvents();
            Assert(page.Canvas.AutoScrollPosition.X < 0 && page.Canvas.AutoScrollPosition.Y < 0 && page.Canvas.ClientRectangle.IntersectsWith(nativeControl.Bounds),
                "The saved native node outside the viewport remained unreachable after restarting its actual form.");
            owner.CaptureLayout();
            Assert(new Point(restored.X, restored.Y) == nativeLocation && new Point(restoredOrdinary.X, restoredOrdinary.Y) == ordinaryLocation,
                "Scrolling a restored mixed-node canvas changed its persisted logical positions.");
            owner.Start(restored.Id, false, false, true); Wait(owner.Runtime.Completion);
            Assert(owner.Runtime.Error == null && page.Calls == 1, "The saved callback retained a disposed page or failed on the new actual form instance.");
        }
    }

    /// <summary>Reject removed methods, changed result contracts and foreign page ownership without adopting another method.</summary>
    private static void NativeMissingIdentity()
    {
        WithNativeEvents((page, owner, actions) =>
        {
            FlowStep step = NativeNode(page, owner, actions, "Start_FlowChartRun");
            string before = ActionNodeXml(step); step.Action += ".removed";
            bool rejected = false;
            try { actions.Validate(step); } catch (InvalidOperationException) { rejected = true; }
            Assert(rejected && actions.FindMethod(step) == null && page.Calls == 0, "A deleted native method fell back to a same-named callback.");
            step.Action = NativeMethod(actions, page, "Start_FlowChartRun").Id;
            using (NativeEventPage other = new NativeEventPage("NativeForeign_" + Guid.NewGuid().ToString("N")))
            {
                FlowPrograms.Attach(other);
                FlowActions foreign = new FlowActions(FlowResources.Capture(), other);
                rejected = false;
                try { foreign.Validate(step); } catch (InvalidOperationException) { rejected = true; }
                Assert(rejected && foreign.FindMethod(step) == null && page.Calls == 0 && other.Calls == 0,
                    "A callback from another current form could be validated or invoked through global discovery.");
                Assert(ActionNodeXml(step) == before, "Checking a foreign method rewrote the native node draft.");
            }
            step.UsesFlowResult = false; rejected = false;
            try { actions.Validate(step); } catch (InvalidOperationException) { rejected = true; }
            Assert(rejected, "An old or altered native result contract silently changed execution semantics.");
        });
    }

    /// <summary>Stop before native code executes and step all IDLE retries as one node before the next ordinary action.</summary>
    private static void NativeEventDebug()
    {
        WithNativeEvents((page, owner, actions) =>
        {
            FlowStep initial = owner.AddNode("", "value", page.Canvas, new Point(20, 20)); initial.Inputs[0].Reference = "Begin";
            FlowStep native = NativeNode(page, owner, actions, "Poll_FlowChartRun"); native.Breakpoint = true;
            FlowStep following = owner.AddNode("", "value", page.Canvas, new Point(20, 220)); following.Inputs[0].Reference = "End";
            initial.Next = native.Id; native.Next = following.Id; page.IdleBeforeExit = 2;
            owner.StartDebug(initial.Id, false);
            ActionUntil(() => owner.Runtime.IsDebugPaused && owner.DebugSnapshot.StepId == initial.Id, "Debug did not stop at the ordinary entry.");
            owner.Runtime.ContinueDebug();
            ActionUntil(() => owner.Runtime.IsDebugPaused && owner.DebugSnapshot.StepId == native.Id, "The native node breakpoint was skipped.");
            Assert(page.Polls == 0, "Native code executed before the framework breakpoint was released.");
            owner.Runtime.StepDebug();
            ActionUntil(() => owner.Runtime.IsDebugPaused && owner.DebugSnapshot.StepId == following.Id, "Native single-step did not reach the next ordinary node.");
            Assert(page.Polls == 3, "Single-step stopped inside IDLE polling or executed the native node more than once.");
            owner.Runtime.ContinueDebug(); Wait(owner.Runtime.Completion);
            Assert(owner.Runtime.Error == null, "A native debug session changed normal execution semantics: " + owner.Runtime.Error);
        });
    }

    /// <summary>Observe machine Pause at each native poll boundary without dispatching callbacks while paused.</summary>
    private static void NativeEventPause()
    {
        WithNativeEvents((page, owner, actions) =>
        {
            page.IdleBeforeExit = int.MaxValue;
            FlowStep step = NativeNode(page, owner, actions, "Poll_FlowChartRun"); step.TimeoutMs = 5000;
            owner.Start(step.Id, false, false);
            ActionUntil(() => Volatile.Read(ref page.Polls) >= 2, "The native IDLE callback did not start polling.");
            owner.Runtime.Pause(); NativePump(90);
            int pausedCount = Volatile.Read(ref page.Polls); NativePump(90);
            Assert(Volatile.Read(ref page.Polls) == pausedCount && owner.Runtime.IsRunning, "Pause kept dispatching native IDLE callbacks or completed the node.");
            owner.Runtime.Resume(); ActionUntil(() => Volatile.Read(ref page.Polls) > pausedCount, "Resume did not continue the same native node.");
            owner.Runtime.Stop(); Wait(owner.Runtime.Completion);
            Assert(owner.Runtime.Error == "Cancelled", "Stopping a resumed native poll did not cancel its session.");
        });
    }

    /// <summary>Hold ownership through an uncancellable call and prevent a delayed native return from publishing output.</summary>
    private static void NativeEventStop()
    {
        WithNativeEvents((page, owner, actions) =>
        {
            FlowStep step = NativeNode(page, owner, actions, "Blocking_FlowChartRun"); step.TimeoutMs = 5000;
            FlowResultHub hub = new FlowResultHub(); FlowExecutionContext context = Context(hub); context.ModuleId = owner.Program.ModuleId;
            FlowProgramRuntime runtime = NativeRuntime(actions);
            try
            {
                runtime.Start(owner.Program, step.Id, context);
                ActionUntil(() => page.BlockStarted.IsSet, "The blocking native callback did not start.");
                runtime.Stop(); NativePump(50);
                Assert(runtime.IsRunning && !FlowResourceGate.CanAccess(page) && !FlowResourceGate.CanAccess(page.LocalInput) && page.OwnedDuringCall,
                    "Stopping released the native form while its actual callback was still running.");
                page.BlockRelease.Set(); Wait(runtime.Completion);
                Assert(runtime.Error == "Cancelled" && FlowResourceGate.CanAccess(page) && FlowResourceGate.CanAccess(page.LocalInput) && page.Calls == 1,
                    "A stopped callback repeated or retained ownership after its actual return.");
                bool unpublished = false;
                try { hub.Read(context.RunId, context.CycleId, owner.Program.ModuleId + "/" + step.Id, "Result"); }
                catch (FlowOutputUnavailableException) { unpublished = true; }
                Assert(unpublished, "A native callback published a delayed result after Stop.");
            }
            finally { page.BlockRelease.Set(); runtime.Stop(); Wait(runtime.Completion); }
        });
    }

    /// <summary>Use one timeout for all IDLE calls and reject unsupported or undeclared return codes without guessing an exit.</summary>
    private static void NativeEventFailures()
    {
        WithNativeEvents((page, owner, actions) =>
        {
            page.IdleBeforeExit = int.MaxValue;
            FlowStep step = NativeNode(page, owner, actions, "Poll_FlowChartRun"); step.TimeoutMs = 1000;
            int alarms = 0; FlowProgramRuntime runtime = NativeRuntime(actions); runtime.AlarmRequested += code => Interlocked.Increment(ref alarms);
            Stopwatch watch = Stopwatch.StartNew(); runtime.Start(owner.Program, step.Id, Context(new FlowResultHub())); Wait(runtime.Completion);
            Assert(runtime.Error != null && runtime.Error.Contains("Timeout") && alarms == 1 && page.Polls >= 2 && watch.ElapsedMilliseconds < 4000,
                "Native IDLE reset its deadline, failed to poll, or raised duplicate alarms: " + runtime.Error
                + " / alarms=" + alarms + " / polls=" + page.Polls + " / elapsed=" + watch.ElapsedMilliseconds);
        });
        foreach (bool invalid in new[] { false, true })
            WithNativeEvents((page, owner, actions) =>
            {
                page.SelectedExit = Flow_Chart.ResultType.CASE2;
                FlowStep step = NativeNode(page, owner, actions, invalid ? "Invalid_FlowChartRun" : "Start_FlowChartRun");
                int alarms = 0; FlowProgramRuntime runtime = NativeRuntime(actions); runtime.AlarmRequested += code => Interlocked.Increment(ref alarms);
                runtime.Start(owner.Program, step.Id, Context(new FlowResultHub())); Wait(runtime.Completion);
                Assert(runtime.Error != null && alarms == 1 && page.Calls == 1 && (invalid || runtime.Error.Contains("CASE2")),
                    "An invalid or unconfigured native result silently selected NEXT or repeated the callback: " + runtime.Error);
            });
    }

    /// <summary>Do not run an unknown physical callback merely because the machine selected simulation.</summary>
    private static void NativeEventSimulation()
    {
        WithNativeEvents((page, owner, actions) =>
        {
            FlowStep step = NativeNode(page, owner, actions, "Simulated_FlowChartRun");
            actions.Simulate = true;
            FlowProgramRuntime runtime = NativeRuntime(actions);
            runtime.Start(owner.Program, step.Id, Context(new FlowResultHub())); Wait(runtime.Completion);
            Assert(runtime.Error == null && page.Calls == 0 && page.SimulationCalls == 1
                && !NativeResource(actions, page).Methods.Any(method => method.Id.StartsWith("native.PreviewNative.", StringComparison.Ordinal)),
                "Native simulation executed hardware or exposed its hidden preview method.");
            FlowStep physical = NativeNode(page, owner, actions, "Start_FlowChartRun");
            runtime.Start(owner.Program, physical.Id, Context(new FlowResultHub())); Wait(runtime.Completion);
            Assert(runtime.Error != null && page.Calls == 0, "A native hardware callback executed without a declared simulator.");
        });
    }

    /// <summary>Exercise the actual SDK double-click events without opening the code node's configuration dialog.</summary>
    private static void NativeDoubleClickPolicy()
    {
        WithNativeEvents((page, owner, actions) =>
        {
            FlowStep native = NativeNode(page, owner, actions, "Start_FlowChartRun");
            FlowStep action = owner.AddNode("", "value", page.Canvas, new Point(230, 80));
            FlowStep condition = owner.AddNode("None", page.Canvas, new Point(430, 80));
            Flow_Chart nativeControl = page.Canvas.Controls.OfType<Flow_Chart>().Single(node => node.Name == native.ControlName);
            page.ClientSize = new Size(850, 350); page.Show(); owner.SetEditing(true); Application.DoEvents();
            string before = ActionNodeXml(native);
            foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
            {
                SwitchLanguage(language);
                Assert(NativeEditorCount(() => NativeDoubleClick(nativeControl)) == 0 && NativeEditorCount(() => NativeDoubleClick(page.BoundNode)) == 0,
                    "A new or imported native code node opened its configuration on double-click.");
            }
            foreach (FlowStep editable in new[] { action, condition })
            {
                Flow_Chart control = page.Canvas.Controls.OfType<Flow_Chart>().Single(node => node.Name == editable.ControlName);
                Assert(NativeEditorCount(() => NativeDoubleClick(control)) == 1, "A function or condition node lost its double-click editor.");
            }
            Assert(NativeEditorCount(() => owner.EditNode(native.Id)) == 0 && ActionNodeXml(native) == before,
                "The direct property entry opened a code-node editor or changed its binding.");
            using (Flow view = new Flow())
            {
                typeof(Flow).GetMethod("BindNodeMenus", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(view, new object[] { page });
                foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
                {
                    SwitchLanguage(language);
                    foreach (Flow_Chart code in new[] { nativeControl, page.BoundNode })
                    {
                        Control codeSurface = code.Controls.Find("button1", true).Single();
                        ContextMenuStrip menu = codeSurface.ContextMenuStrip;
                        menu.Show(codeSurface, new Point(5, 5)); Application.DoEvents();
                        Assert(!menu.Items["nodeProperties"].Available && !menu.Items["nodeProperties"].Enabled
                            && menu.Items["nodeBreakpoint"].Enabled && menu.Items["nodeDelete"].Enabled,
                            "A new or imported code node retained its property entry or lost breakpoint/delete commands.");
                        Assert(NativeEditorCount(() => menu.Items["nodeProperties"].PerformClick()) == 0, "A hidden code-node property command opened an editor.");
                        menu.Close();
                    }
                    foreach (FlowStep editable in new[] { action, condition })
                    {
                        Flow_Chart control = page.Canvas.Controls.OfType<Flow_Chart>().Single(node => node.Name == editable.ControlName);
                        Control editableSurface = control.Controls.Find("button1", true).Single();
                        ContextMenuStrip menu = editableSurface.ContextMenuStrip;
                        menu.Show(editableSurface, new Point(5, 5)); Application.DoEvents();
                        Assert(menu.Items["nodeProperties"].Available && menu.Items["nodeProperties"].Enabled,
                            "Switching from a code node hid another node's property command.");
                        int opened = NativeEditorCount(() => menu.Items["nodeProperties"].PerformClick());
                        Assert(opened == 1, "An ordinary node lost its property editor: opened=" + opened + ", mode=" + editable.ExecutionMode + ", language=" + language);
                        menu.Close();
                    }
                }
            }
            Assert(ActionNodeXml(native) == before, "Inspecting code-node menus modified its saved definition.");
            Point location = nativeControl.Location;
            Control surface = nativeControl.Controls.Find("button1", true).Single();
            RaiseMouse(surface, "OnMouseDown", new MouseEventArgs(MouseButtons.Left, 1, 8, 8, 0));
            RaiseMouse(nativeControl, "OnMouseMove", new MouseEventArgs(MouseButtons.Left, 0, 90, 60, 0));
            RaiseMouse(nativeControl, "OnMouseUp", new MouseEventArgs(MouseButtons.Left, 1, 8, 8, 0));
            owner.ToggleBreakpoint(native.Id);
            Assert(nativeControl.Location != location && native.Breakpoint && nativeControl.ProgramBreakpoint && page.Calls == 0 && page.BoundCalls == 0,
                "Blocking code-node double-click disabled dragging or breakpoints, or executed native business code.");
        });
        SwitchLanguage(LanguageType.Chinese);
    }

    /// <summary>Send the same two left-button releases recognized by the SDK's inner-button double-click handler.</summary>
    private static void NativeDoubleClick(Flow_Chart node)
    {
        Control surface = node.Controls.Find("button1", true).Single();
        for (int click = 0; click < 2; click++)
        {
            RaiseMouse(surface, "OnMouseDown", new MouseEventArgs(MouseButtons.Left, 1, 8, 8, 0));
            RaiseMouse(node, "OnMouseUp", new MouseEventArgs(MouseButtons.Left, 1, 8, 8, 0));
        }
    }

    /// <summary>Observe and cancel real modal editors so an unexpected opening fails without blocking the regression run.</summary>
    private static int NativeEditorCount(Action invoke)
    {
        int opened = 0;
        HashSet<FlowNodeForm> seen = new HashSet<FlowNodeForm>();
        using (System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer { Interval = 15 })
        {
            timer.Tick += (sender, args) => {
                FlowNodeForm dialog = Application.OpenForms.OfType<FlowNodeForm>().FirstOrDefault(form => form.Visible && !seen.Contains(form));
                if (dialog == null || !seen.Add(dialog)) return;
                opened++; dialog.DialogResult = DialogResult.Cancel; dialog.Close();
            };
            timer.Start(); invoke(); NativePump(40);
        }
        return opened;
    }

    /// <summary>Exercise page-selection events and the real node editor without reparenting existing machine forms.</summary>
    private static void NativePageAndEditor()
    {
        WithNativeEvents((page, owner, actions) =>
        {
            using (NativeEventPage other = new NativeEventPage("OtherSelection_" + Guid.NewGuid().ToString("N")))
            using (Flow view = new Flow())
            {
                FlowModuleEditor second = FlowPrograms.Attach(other);
                TabControl tabs = ActionControl<TabControl>(view, "tabControl1");
                tabs.TabPages.Add(new TabPage(page.Name) { Tag = owner });
                tabs.TabPages.Add(new TabPage(other.Name) { Tag = second });
                IntPtr formHandle = view.Handle;
                IntPtr tabHandle = tabs.Handle;
                FlowToolboxControl toolbox = ActionControl<FlowToolboxControl>(view, "flowToolbox");
                TreeView tree = ActionControl<TreeView>(toolbox, "toolboxEntries");
                foreach (int index in new[] { 1, 0, 1 })
                {
                    tabs.SelectedIndex = index;
                    Flow_BaseForm selected = index == 0 ? (Flow_BaseForm)page : other;
                    string expected = "native/" + selected.GetType().FullName + "/" + selected.Name;
                    FlowToolboxEntry[] native = tree.Nodes.Cast<TreeNode>().SelectMany(group => group.Nodes.Cast<TreeNode>())
                        .Select(node => node.Tag as FlowToolboxEntry).Where(entry => entry != null && (entry.TargetId ?? "").StartsWith("native/", StringComparison.Ordinal)).ToArray();
                    Assert(native.Length > 0 && native.All(entry => entry.TargetId == expected), "Changing actual flow pages retained another page's native functions: " + expected + " / " + string.Join(",", native.Select(entry => entry.TargetId)));
                }
            }
            FlowStep step = NativeNode(page, owner, actions, "Start_FlowChartRun");
            FlowStep actionStep = owner.AddNode("", "value", page.Canvas, new Point(230, 80));
            FlowStep conditionStep = owner.AddNode("None", page.Canvas, new Point(430, 80));
            Flow_Chart nativeControl = page.Canvas.Controls.OfType<Flow_Chart>().Single(node => node.Name == step.ControlName);
            Flow_Chart actionControl = page.Canvas.Controls.OfType<Flow_Chart>().Single(node => node.Name == actionStep.ControlName);
            Flow_Chart conditionControl = page.Canvas.Controls.OfType<Flow_Chart>().Single(node => node.Name == conditionStep.ControlName);
            string caption = step.Caption;
            page.ClientSize = new Size(850, 350); page.Show(); Application.DoEvents();
            using (FlowNodeForm dialog = new FlowNodeForm(owner, step))
            {
                dialog.Show();
                foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
                {
                    SwitchLanguage(language); Application.DoEvents();
                    Assert(nativeControl.ProgramBindingLabel.Length > 0 && actionControl.ProgramBindingLabel.Length > 0
                        && nativeControl.ProgramBindingLabel != actionControl.ProgramBindingLabel && actionControl.ProgramBindingLabel != conditionControl.ProgramBindingLabel
                        && nativeControl.text == caption && nativeControl.AccessibleDescription.Contains("Start_FlowChartRun()"),
                        "The canvas did not distinguish code, action and condition bindings, or a label changed the business caption.");
                    ActionControl<TabControl>(dialog, "executionTabs").SelectedIndex = 1;
                    FlowBranchEditor branch = dialog.Controls.Find("branch_NEXT_dyn", true).OfType<FlowBranchEditor>().Single();
                    Assert(!ActionControl<RadioButton>(branch, "branchUnconditional").Visible && !branch.ConditionRows.Any()
                        && branch.ReadBranch().Unconditional && branch.ReadBranch().Conditions.Count == 0,
                        "The native editor asked for a second condition decision.");
                    Screenshot(dialog, "native-node-" + language);
                    Screenshot(page, "native-binding-labels-" + language);
                }
                ActionControl<Button>(dialog, "addCase").PerformClick();
                Assert(dialog.Controls.Find("branch_CASE1_dyn", true).OfType<FlowBranchEditor>().Single().ReadBranch().Conditions.Count == 0,
                    "Adding a native CASE invented a condition.");
                ActionControl<Button>(dialog, "ok").PerformClick();
                Assert(dialog.DialogResult == DialogResult.OK && owner.Program.Steps.Single(item => item.Id == step.Id).UsesFlowResult,
                    "The native destination-only editor could not accept its draft.");
            }
        });
        SwitchLanguage(LanguageType.Chinese);
    }
}
