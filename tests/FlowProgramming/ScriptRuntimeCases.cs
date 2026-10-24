using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using AlphaRap;
using AlphaRap.FlowProgramming;
using NPSDK;

internal sealed class FlowScriptObjectProbe : IFlowObject
{
    public int Calls, Simulations;
    public int FailuresBeforeSuccess;
    public bool Hold, AccessAllowed;
    public CancellationToken ObservedToken;
    public readonly ManualResetEventSlim Started = new ManualResetEventSlim();
    public readonly TaskCompletionSource<bool> Release = new TaskCompletionSource<bool>();

    /// <summary>Declare one hardware-shaped capability while keeping every operation in memory.</summary>
    public IEnumerable<FlowMethod> GetFlowMethods(bool includeDraft = false)
    {
        FlowMethod method = new FlowMethod("script.probe", "脚本对象测试", "Script object probe", "Prueba de objeto de script") { UsesHardware = true };
        method.Inputs.Add(new FlowField("Increment", typeof(long), "2"));
        method.Outputs.Add(new FlowField("Count", typeof(long)));
        method.Invoke = async call =>
        {
            int invocation = Interlocked.Increment(ref Calls);
            AccessAllowed = FlowResourceGate.CanAccess(this);
            ObservedToken = call.Token;
            Started.Set();
            if (invocation <= FailuresBeforeSuccess) throw new InvalidOperationException("SCRIPT-PROBE-TRANSIENT");
            if (Hold) await Release.Task.ConfigureAwait(false);
            call.Token.ThrowIfCancellationRequested();
            return FlowCall.Result("Count", call.Get<long>("Increment") + 40L);
        };
        method.Simulate = call =>
        {
            Interlocked.Increment(ref Simulations);
            return Task.FromResult(FlowCall.Result("Count", 900L));
        };
        yield return method;
    }
}

internal static partial class Cases
{
    /// <summary>Register script execution, actual object calls and node debugging regressions.</summary>
    public static void RegisterScriptRuntimeCases()
    {
        Test("script typed inputs calculate outputs route NEXT and save through the real DataForm", ScriptTypedProductionChain);
        Test("script compiler diagnostics map to the exact user source line", ScriptCompilationDiagnostics);
        Test("script outputs require assignment and reject NaN and infinity", ScriptOutputContracts);
        Test("empty nodes skip actions while retaining ordinary branch execution", ScriptNoneExecution);
        Test("script source ports resource identities and breakpoints survive XML reload", ScriptSerialization);
        Test("unchanged script definitions reuse one loaded assembly and cached source", ScriptCompileCache);
        Test("script object calls reuse the actual instance lease and declared resource boundary", ScriptControlledObjectLease);
        Test("simulated script object calls invoke simulation without touching hardware", ScriptSimulation);
        Test("cancelled and timed-out scripts never publish delayed output", ScriptCancellationAndTimeout);
        Test("node debugging pauses first steps once and continues only to configured breakpoints", ScriptDebugSteps);
        Test("debug stops cancel before action and waiting conditions expose completed action outputs", ScriptDebugStopAndWait);
        Test("script calls missing await remain owned until the real action finishes", ScriptUnawaitedCallLifetime);
        Test("script body failures cancel and drain non-cooperative object calls before finishing", ScriptFailedBodyDrainsCalls);
        Test("two controlled DataForm saves in one script keep distinct payloads and record identities", ScriptDistinctProductionCalls);
    }

    /// <summary>Create a script node whose input bindings match its declared strong port types.</summary>
    private static FlowStep ScriptNode(string source, params FlowScriptPort[] outputs)
    {
        return new FlowStep { Caption = "Script regression", ExecutionMode = "Script", ScriptSource = source,
            TimeoutMs = 4000, Branches = FlowBranch.Default(), ScriptOutputs = outputs.ToList() };
    }

    /// <summary>Create a scalar script port without deriving type from its display text.</summary>
    private static FlowScriptPort ScriptPort(string name, string type, string value = "")
    {
        return new FlowScriptPort { Name = name, Type = type, DefaultValue = value };
    }

    /// <summary>Capture actual framework resources and use an isolated result hub for this scenario.</summary>
    private static FlowExecutionContext ScriptContext(FlowActions actions, string module = "ScriptRegression")
    {
        FlowResultHub hub = new FlowResultHub();
        return new FlowExecutionContext { Hub = hub, RunId = hub.Epoch, CycleId = "script-product", ModuleId = module,
            RecipeName = SysPara.RecipeName, Parameters = actions.Resources.Parameters };
    }

    /// <summary>Execute one real host action while pumping the UI thread for framework callbacks.</summary>
    private static IDictionary<string, object> ScriptExecute(FlowActions actions, FlowStep step)
    {
        Task<IDictionary<string, object>> task = actions.Execute(step, ScriptContext(actions), CancellationToken.None);
        Wait(task, 10000);
        return task.GetAwaiter().GetResult();
    }

    /// <summary>Observe an asynchronous state without blocking UI-thread table updates.</summary>
    private static void ScriptUntil(Func<bool> ready, string message, int timeout = 4000)
    {
        DateTime limit = DateTime.UtcNow.AddMilliseconds(timeout);
        while (!ready() && DateTime.UtcNow < limit) { Application.DoEvents(); Thread.Sleep(5); }
        Assert(ready(), message);
    }

    /// <summary>Prove a node has no readable result after cancellation or failure.</summary>
    private static bool ScriptOutputMissing(FlowExecutionContext context, FlowStep step, string name)
    {
        try { context.Hub.Read(context.RunId, context.CycleId, context.ModuleId + "/" + step.Id, name); return false; }
        catch (FlowOutputUnavailableException) { return true; }
    }

    /// <summary>Run all four port types through the real action pipeline and production record storage.</summary>
    private static void ScriptTypedProductionChain()
    {
        string product = "SCRIPT-" + Guid.NewGuid().ToString("N");
        FlowStep script = ScriptNode("Outputs.Measurement = Inputs.Base + Inputs.Count * 0.5;\nOutputs.Product = Inputs.Model;\nOutputs.Pass = Inputs.Enabled && Outputs.Measurement == 10.5;",
            ScriptPort("Measurement", "Number"), ScriptPort("Product", "Text"), ScriptPort("Pass", "Boolean"));
        script.ScriptInputs.AddRange(new[] { ScriptPort("Base", "Number", "9"), ScriptPort("Count", "Integer", "3"), ScriptPort("Model", "Text", product), ScriptPort("Enabled", "Boolean", "true") });
        script.Inputs = script.ScriptInputs.Select(port => new FlowInput { Name = port.Name, Reference = port.DefaultValue }).ToList();
        script.Branches[0].Unconditional = false;
        script.Branches[0].Conditions.Add(new FlowCondition { Left = new FlowInput { Source = "Result", Reference = "Pass" }, Right = new FlowInput { Reference = "true" } });
        FlowActions actions = new FlowActions(FlowResources.Capture()) { Simulate = true };
        FlowStep storage = BoundStep(actions.FindResource("data/Production"), "data.save");
        storage.Caption = "Save script result"; storage.Branches = FlowBranch.Default();
        foreach (FlowInput input in storage.Inputs)
        {
            if (input.Name == "Product") { input.Source = "Output"; input.Reference = editor.Program.ModuleId + "/" + script.Id + "/Product"; }
            else if (input.Name == "Measurement") { input.Source = "Output"; input.Reference = editor.Program.ModuleId + "/" + script.Id + "/Measurement"; }
            else input.Reference = "0";
        }
        script.Next = storage.Id;
        editor.Program.Steps.Add(script); editor.Program.Steps.Add(storage);
        try
        {
            FlowExecutionContext context = ScriptContext(actions, editor.Program.ModuleId);
            FlowProgramRuntime runtime = new FlowProgramRuntime { ExecuteAction = actions.Execute };
            runtime.Start(new FlowProgram { ModuleId = context.ModuleId, Steps = new List<FlowStep> { script, storage } }, script.Id, context);
            Wait(runtime.Completion, 10000);
            Assert(runtime.Error == null, runtime.Error);
            Assert((double)context.Hub.Read(context.RunId, context.CycleId, context.ModuleId + "/" + script.Id, "Measurement") == 10.5, "Typed script result did not reach the branch.");
            string path = (string)context.Hub.Read(context.RunId, context.CycleId, context.ModuleId + "/" + storage.Id, "Path");
            Assert(File.Exists(path), "DataForm did not create the local production file.");
            int files, total;
            List<ProductionRecord> rows = ProductionDataStore.Query(DateTime.Today, DateTime.Today.AddDays(1), "", out files, out total);
            Assert(rows.Any(row => row.Get("Product") == product && row.Get("Measurement") == "10.5" && row.Get("Simulated") == "True"), "The saved production row lost a script output or simulation marker.");
        }
        finally { editor.Program.Steps.Remove(storage); editor.Program.Steps.Remove(script); }
    }

    /// <summary>Reject compiler errors with the user file and exact source line instead of wrapper lines.</summary>
    private static void ScriptCompilationDiagnostics()
    {
        FlowStep script = ScriptNode("double value = 1;\nOutputs.Value = value + MissingScriptIdentifier;", ScriptPort("Value", "Number"));
        string path = FlowScripts.GetSourcePath(script), message = null;
        try { FlowScripts.Compile(script); }
        catch (InvalidOperationException error) { message = error.Message; }
        Assert(message != null && message.Contains("CS0103") && message.Contains("UserScript.cs(2,") && message.Contains("MissingScriptIdentifier"), "Compiler diagnostic did not point to line two of user source.");
        Assert(File.Exists(path) && File.ReadAllText(path) == script.ScriptSource, "Compiler diagnostic source was not preserved verbatim.");
    }

    /// <summary>Prevent successful execution with unassigned, non-finite or wrongly typed scalar outputs.</summary>
    private static void ScriptOutputContracts()
    {
        FlowActions actions = new FlowActions(FlowResources.Capture());
        foreach (string source in new[] { "await Task.Delay(1);", "Outputs.Value = double.NaN;", "Outputs.Value = double.PositiveInfinity;" })
        {
            bool rejected = false;
            try { ScriptExecute(actions, ScriptNode(source, ScriptPort("Value", "Number"))); }
            catch (InvalidOperationException) { rejected = true; }
            Assert(rejected, "Invalid script output was accepted: " + source);
        }
        FlowStep integer = ScriptNode("Outputs.Value = Inputs.Count;", ScriptPort("Value", "Integer"));
        integer.ScriptInputs.Add(ScriptPort("Count", "Integer")); integer.Inputs.Add(new FlowInput { Name = "Count", Reference = "1.5" });
        bool fractional = false;
        try { ScriptExecute(actions, integer); } catch (InvalidOperationException) { fractional = true; }
        Assert(fractional, "Fractional input was truncated into an Integer port.");
        Assert(FlowProgramFile.Compare(true, "==", "true") && FlowProgramFile.Compare(false, "==", "FALSE")
            && FlowProgramFile.Compare("1", "==", true) && FlowProgramFile.Compare(false, "==", 0), "Actual Boolean conditions did not normalize valid scalar spellings.");
        Assert(!FlowProgramFile.Compare("true", "==", "True") && FlowProgramFile.Compare(true, "Contains", "rue")
            && !FlowProgramFile.Compare(true, "Empty", ""), "Boolean comparison changed ordinary text, Contains or Empty semantics.");
        bool invalidBoolean = false;
        try { FlowProgramFile.Compare(true, "==", "not-a-boolean"); }
        catch (InvalidOperationException error) { invalidBoolean = error.Message.Contains("Boolean"); }
        Assert(invalidBoolean, "Invalid Boolean conditions silently compared as arbitrary text.");
        Assert(!FlowProgramFile.Compare(9007199254740992L, "==", 9007199254740993L)
            && FlowProgramFile.Compare(9007199254740993L, ">", "9007199254740992")
            && FlowProgramFile.Compare("9007199254740992", "<", 9007199254740993L), "Adjacent Integer values above 2^53 lost comparison precision.");
        Assert(!FlowProgramFile.Compare(long.MaxValue, "==", long.MaxValue - 1)
            && FlowProgramFile.Compare(long.MaxValue, ">", "9223372036854775806")
            && FlowProgramFile.Compare("-9223372036854775808", "<", long.MinValue + 1)
            && FlowProgramFile.Compare(long.MinValue, "==", "-9223372036854775808"), "Int64 boundary conditions rounded distinct integers into the same value.");
    }

    /// <summary>Execute a branch-only empty node while ignoring inactive action and script configuration.</summary>
    private static void ScriptNoneExecution()
    {
        FlowStep step = new FlowStep { ExecutionMode = "None", Action = "not-a-method", Target = "not-an-object", ScriptSource = "not C#", Branches = FlowBranch.Default() };
        FlowActions actions = new FlowActions(FlowResources.Capture());
        FlowMethod method = actions.FindMethod(step);
        Assert(method.Inputs.Count == 0 && method.Outputs.Count == 0 && !method.UsesHardware, "Empty node declared hardware or inactive ports.");
        FlowScripts.Compile(step);
        Assert(ScriptExecute(actions, step).Count == 0, "Empty node attempted to resolve an inactive object or script.");
        FlowStep next = ScriptNode("Outputs.Value = 71;", ScriptPort("Value", "Integer")); step.Next = next.Id;
        step.Branches[0].Unconditional = false; step.Branches[0].Conditions.Add(new FlowCondition { Left = new FlowInput { Reference = "1" }, Right = new FlowInput { Reference = "1" } });
        FlowExecutionContext context = ScriptContext(actions);
        FlowProgramRuntime runtime = new FlowProgramRuntime { ExecuteAction = actions.Execute };
        runtime.Start(new FlowProgram { ModuleId = context.ModuleId, Steps = new List<FlowStep> { step, next } }, step.Id, context);
        Wait(runtime.Completion, 10000);
        Assert(runtime.Error == null && (long)context.CurrentResult["Value"] == 71L, "Empty node did not evaluate its configured NEXT condition.");
    }

    /// <summary>Round-trip complete script configuration and keep missing legacy fields on the original action path.</summary>
    private static void ScriptSerialization()
    {
        FlowStep script = ScriptNode("Outputs.Value = Inputs.Count * 2;", ScriptPort("Value", "Integer"));
        script.Breakpoint = true; script.ScriptInputs.Add(ScriptPort("Count", "Integer", "3")); script.ScriptResources.Add("actual/object/id");
        string path = Path.Combine(root, "ScriptProject.xml");
        FlowProgramFile.Save(path, new FlowProject { Modules = new List<FlowProgram> { new FlowProgram { ModuleId = "ScriptXml", Steps = new List<FlowStep> { script } } } });
        FlowStep loaded = FlowProgramFile.Load(path).Modules[0].Steps[0];
        Assert(loaded.ExecutionMode == "Script" && loaded.ScriptSource == script.ScriptSource && loaded.Breakpoint && loaded.ScriptInputs[0].DefaultValue == "3" && loaded.ScriptOutputs[0].Type == "Integer" && loaded.ScriptResources.SequenceEqual(script.ScriptResources), "Script configuration changed after XML reload.");
        const string old = "<FlowProgram><ModuleId>Old</ModuleId><Steps><FlowStep><Id>old</Id><Action>value</Action><TimeoutMs>1000</TimeoutMs></FlowStep></Steps></FlowProgram>";
        FlowProgram legacy;
        using (StringReader reader = new StringReader(old)) legacy = (FlowProgram)new XmlSerializer(typeof(FlowProgram)).Deserialize(reader);
        Assert(legacy.Steps[0].ExecutionMode == "Action" && !legacy.Steps[0].Breakpoint && legacy.Steps[0].ScriptInputs.Count == 0, "Legacy XML unexpectedly entered script or debug mode.");
        loaded.ScriptResources.Add("actual/object/id"); loaded.ScriptOutputs.Add(ScriptPort("Value", "Unknown"));
        Assert(FlowProgramFile.ValidateExecution(loaded).Count >= 3, "SDK validation accepted duplicate ports, bad type or duplicate resources.");
    }

    /// <summary>Confirm identical source and port definitions do not rebuild or load another assembly.</summary>
    private static void ScriptCompileCache()
    {
        FlowStep script = ScriptNode("Outputs.Value = 192.25;", ScriptPort("Value", "Number"));
        FlowScripts.Compile(script);
        string source = FlowScripts.GetSourcePath(script), folder = Path.GetDirectoryName(source);
        string assembly = Path.Combine(folder, "FlowScript_" + Path.GetFileName(folder) + ".dll");
        DateTime sourceTime = File.GetLastWriteTimeUtc(source), assemblyTime = File.GetLastWriteTimeUtc(assembly);
        int loaded = AppDomain.CurrentDomain.GetAssemblies().Count(item => item.GetName().Name == Path.GetFileNameWithoutExtension(assembly));
        FlowActions actions = new FlowActions(FlowResources.Capture());
        FlowScripts.Compile(script); ScriptExecute(actions, script); ScriptExecute(actions, script);
        Assert(loaded == 1 && AppDomain.CurrentDomain.GetAssemblies().Count(item => item.GetName().Name == Path.GetFileNameWithoutExtension(assembly)) == loaded, "Identical script loaded multiple assemblies.");
        Assert(sourceTime == File.GetLastWriteTimeUtc(source) && assemblyTime == File.GetLastWriteTimeUtc(assembly), "Cached script was rebuilt during repeated execution.");
        script.ScriptResources.Add("changes-cache-identity");
        Assert(FlowScripts.GetSourcePath(script) != source, "Changing the controlled-object contract reused the old compiled version.");
    }

    /// <summary>Create a real instance resource and a script that calls only its explicitly declared capability.</summary>
    private static FlowActions ScriptProbeActions(FlowScriptObjectProbe probe, out FlowResource resource, out FlowStep script)
    {
        FlowResources resources = FlowResources.Capture();
        resource = new FlowResource { Id = "script-regression/probe", Kind = "device", Label = "Actual script probe", Instance = probe, Methods = probe.GetFlowMethods().ToList() };
        resources.Items.Add(resource.Id, resource);
        script = ScriptNode("var result = await Context.CallAsync(\"script-regression/probe\", \"script.probe\", new Dictionary<string, object> { { \"Increment\", Inputs.Increment } });\nOutputs.Count = Context.Get<long>(result, \"Count\");", ScriptPort("Count", "Integer"));
        script.ScriptInputs.Add(ScriptPort("Increment", "Integer", "2")); script.Inputs.Add(new FlowInput { Name = "Increment", Reference = "2" });
        script.ScriptResources.Add(resource.Id);
        return new FlowActions(resources);
    }

    /// <summary>Hold the original object's lease until its operation ends and reject undeclared calls before invocation.</summary>
    private static void ScriptControlledObjectLease()
    {
        FlowScriptObjectProbe probe = new FlowScriptObjectProbe { Hold = true };
        FlowResource resource; FlowStep script;
        FlowActions actions = ScriptProbeActions(probe, out resource, out script);
        FlowScripts.Compile(script);
        FlowExecutionContext context = ScriptContext(actions);
        Task<IDictionary<string, object>> running = actions.Execute(script, context, CancellationToken.None);
        Assert(probe.Started.Wait(2000) && probe.Calls == 1 && probe.AccessAllowed && !FlowResourceGate.CanAccess(probe), "Script call did not hold the original instance lease.");
        try
        {
            using (CancellationTokenSource waiting = new CancellationTokenSource())
            {
                Task<IDictionary<string, object>> competitor = actions.Execute(BoundStep(resource, "script.probe"), ScriptContext(actions), waiting.Token);
                Assert(!competitor.IsCompleted && probe.Calls == 1, "A competing action bypassed the actual object lease.");
                waiting.Cancel();
                bool cancelled = false; try { Wait(competitor); } catch (OperationCanceledException) { cancelled = true; }
                Assert(cancelled && probe.Calls == 1, "Cancelling a queued competitor executed its hardware method.");
            }
        }
        finally { probe.Release.TrySetResult(true); Wait(running, 10000); }
        Assert((long)running.Result["Count"] == 42L && FlowResourceGate.CanAccess(probe) && ReferenceEquals(resource.Instance, probe), "Controlled call recreated the device or failed to release ownership.");
        script.ScriptResources.Clear(); int before = probe.Calls; bool blocked = false;
        try { ScriptExecute(actions, script); } catch (InvalidOperationException) { blocked = true; }
        Assert(blocked && probe.Calls == before, "An undeclared object was invoked by the script.");
        FlowScriptObjectProbe stopping = new FlowScriptObjectProbe { Hold = true };
        actions = ScriptProbeActions(stopping, out resource, out script);
        using (CancellationTokenSource stop = new CancellationTokenSource())
        {
            Task<IDictionary<string, object>> pending = actions.Execute(script, ScriptContext(actions), stop.Token);
            Assert(stopping.Started.Wait(2000), "The cancellable controlled call never started."); stop.Cancel();
            try { Assert(!pending.IsCompleted && !FlowResourceGate.CanAccess(stopping), "Cancellation released a still-running controlled object's lease."); }
            finally { stopping.Release.TrySetResult(true); }
            bool cancelled = false; try { Wait(pending); } catch (OperationCanceledException) { cancelled = true; }
            Assert(cancelled && stopping.Calls == 1 && FlowResourceGate.CanAccess(stopping), "Cancelled controlled call published a result or retained its completed lease.");
        }
        ScriptHandledCallRetry();
    }

    /// <summary>Reuse the declared hardware simulation without entering the physical method from script.</summary>
    private static void ScriptSimulation()
    {
        FlowScriptObjectProbe probe = new FlowScriptObjectProbe(); FlowResource resource; FlowStep script;
        FlowActions actions = ScriptProbeActions(probe, out resource, out script); actions.Simulate = true;
        IDictionary<string, object> result = ScriptExecute(actions, script);
        Assert((long)result["Count"] == 900L && probe.Simulations == 1 && probe.Calls == 0 && !probe.Started.IsSet, "Simulation script invoked the actual hardware method.");
    }

    /// <summary>Cancel or time out a non-cooperative delay without publishing its eventual result.</summary>
    private static void ScriptCancellationAndTimeout()
    {
        FlowStep script = ScriptNode("await Task.Delay(260);\nOutputs.Value = 123;", ScriptPort("Value", "Number"));
        FlowScripts.Compile(script);
        FlowActions actions = new FlowActions(FlowResources.Capture());
        FlowExecutionContext context = ScriptContext(actions);
        FlowProgram plan = new FlowProgram { ModuleId = context.ModuleId, Steps = new List<FlowStep> { script } };
        FlowProgramRuntime runtime = new FlowProgramRuntime { ExecuteAction = actions.Execute }; int starts = 0;
        runtime.StateChanged += (step, state) => { if (state == "Running") Interlocked.Increment(ref starts); };
        runtime.Start(plan, script.Id, context); ScriptUntil(() => starts == 1, "Cancellation script never started.");
        Thread.Sleep(35); runtime.Stop();
        Assert(runtime.IsRunning, "Cancellation abandoned a still-running user function.");
        Wait(runtime.Completion, 10000);
        Assert(runtime.Error == "Cancelled" && !runtime.AlarmWasRaised && ScriptOutputMissing(context, script, "Value"), "Cancelled script published delayed data or raised an alarm.");
        script.TimeoutMs = 45; context = ScriptContext(actions); int alarms = 0;
        runtime.AlarmRequested += code => Interlocked.Increment(ref alarms);
        runtime.Start(plan, script.Id, context); ScriptUntil(() => runtime.AlarmWasRaised, "Script timeout did not notify the alarm.");
        Assert(runtime.IsRunning, "Timeout discarded the still-running script function.");
        Wait(runtime.Completion, 10000);
        Assert(alarms == 1 && runtime.Error != null && runtime.Error.Contains("Timeout") && ScriptOutputMissing(context, script, "Value"), "Timed-out script published delayed data or repeated its alarm.");
    }

    /// <summary>Check debug entry, per-node stepping, configured breakpoints and normal-run compatibility.</summary>
    private static void ScriptDebugSteps()
    {
        FlowStep first = ScriptNode("Outputs.Value = Inputs.Shared;", ScriptPort("Value", "Integer")); first.TimeoutMs = 250;
        first.ScriptInputs.Add(ScriptPort("Shared", "Integer")); first.Inputs.Add(new FlowInput { Name = "Shared", Source = "Parameter", Reference = "ScriptShared" });
        FlowStep second = new FlowStep { ExecutionMode = "None", Branches = FlowBranch.Default() };
        FlowStep third = ScriptNode("Outputs.Value = 3;", ScriptPort("Value", "Integer")); third.Breakpoint = true;
        first.Next = second.Id; second.Next = third.Id; FlowScripts.Compile(first); FlowScripts.Compile(third);
        FlowActions actions = new FlowActions(FlowResources.Capture()); actions.Resources.Parameters["ScriptShared"] = 1L; FlowExecutionContext context = ScriptContext(actions);
        FlowProgram plan = new FlowProgram { ModuleId = context.ModuleId, Steps = new List<FlowStep> { first, second, third } };
        ConcurrentQueue<FlowDebugSnapshot> snapshots = new ConcurrentQueue<FlowDebugSnapshot>();
        FlowProgramRuntime runtime = new FlowProgramRuntime { ExecuteAction = actions.Execute };
        runtime.DebugChanged += snapshot => snapshots.Enqueue(snapshot);
        runtime.Start(plan, first.Id, context, true); ScriptUntil(() => runtime.IsDebugPaused, "Debug did not stop at entry.");
        Thread.Sleep(350);
        Assert(runtime.IsRunning && !runtime.AlarmWasRaised && ScriptOutputMissing(context, first, "Value"), "Breakpoint consumed timeout or executed the node.");
        context.Hub.SetParameter(context.RunId, "ScriptShared", 5L);
        runtime.StepDebug(); ScriptUntil(() => runtime.IsDebugPaused && snapshots.Any(item => item.State == "Paused" && item.StepId == second.Id), "Step did not stop before the second node.");
        Assert(snapshots.Count(item => item.State == "ActionCompleted" && item.StepId == first.Id) == 1, "Single-step invoked its script more than once.");
        Assert((long)snapshots.Single(item => item.State == "ActionCompleted" && item.StepId == first.Id).Inputs["Shared"] == 5L, "Completed-action input snapshot retained the old paused parameter value.");
        runtime.ContinueDebug(); ScriptUntil(() => runtime.IsDebugPaused && snapshots.Any(item => item.State == "Paused" && item.StepId == third.Id), "Continue did not reach the configured breakpoint.");
        runtime.StepDebug(); Wait(runtime.Completion, 10000);
        Assert(runtime.Error == null && !runtime.IsDebugging && !context.Debugging && snapshots.Last().State == "Finished", "Final debug step did not clear session state.");
        int events = snapshots.Count; context = ScriptContext(actions); runtime.Start(plan, first.Id, context); Wait(runtime.Completion, 10000);
        Assert(runtime.Error == null && snapshots.Count == events && (long)context.CurrentResult["Value"] == 3L, "Normal execution obeyed debug breakpoints or changed typed output.");
    }

    /// <summary>Stop debug gates safely and expose action output before its downstream conditions become true.</summary>
    private static void ScriptDebugStopAndWait()
    {
        FlowActions actions = new FlowActions(FlowResources.Capture());
        FlowStep script = ScriptNode("Outputs.Value = 7;", ScriptPort("Value", "Integer")); FlowScripts.Compile(script);
        FlowProgramRuntime runtime = new FlowProgramRuntime { ExecuteAction = actions.Execute }; int finished = 0;
        runtime.DebugChanged += snapshot => { throw new InvalidOperationException("Observer failure"); };
        runtime.Finished += () => { throw new InvalidOperationException("Observer failure"); };
        runtime.Finished += () => Interlocked.Increment(ref finished);
        FlowExecutionContext context = ScriptContext(actions); FlowProgram plan = new FlowProgram { ModuleId = context.ModuleId, Steps = new List<FlowStep> { script } };
        runtime.Start(plan, script.Id, context, true); ScriptUntil(() => runtime.IsDebugPaused, "Debug stop did not reach the entry.");
        runtime.Stop(); Wait(runtime.Completion);
        Assert(runtime.Error == "Cancelled" && !runtime.AlarmWasRaised && finished == 1 && ScriptOutputMissing(context, script, "Value"), "Debug gate stop executed an action, alarmed or skipped finish cleanup.");
        FlowStep next = new FlowStep { ExecutionMode = "None", Branches = FlowBranch.Default() }; script.Next = next.Id; plan.Steps.Add(next);
        ConcurrentQueue<FlowDebugSnapshot> stopped = new ConcurrentQueue<FlowDebugSnapshot>(); runtime.DebugChanged += item => stopped.Enqueue(item);
        context = ScriptContext(actions); runtime.Start(plan, script.Id, context, true); ScriptUntil(() => runtime.IsDebugPaused, "Second-node stop never reached its first gate.");
        runtime.StepDebug(); ScriptUntil(() => runtime.IsDebugPaused && stopped.Any(item => item.State == "Paused" && item.StepId == next.Id), "Second-node stop never reached the next gate.");
        runtime.Stop(); Wait(runtime.Completion);
        Assert(stopped.Last().State == "Finished" && stopped.Last().StepId == next.Id && stopped.Last().Outputs.Count == 0,
            "Stopping before an unexecuted node attributed the previous node's result to it.");
        script.Next = null; plan.Steps.Remove(next);
        context = ScriptContext(actions); context.Parameters["ScriptReady"] = false; actions.Resources.Parameters["ScriptReady"] = false;
        script.Branches = new List<FlowBranch> { new FlowBranch { Conditions = new List<FlowCondition> { new FlowCondition {
            Left = new FlowInput { Source = "Parameter", Reference = "ScriptReady" }, Right = new FlowInput { Reference = "true" } } } } };
        ConcurrentQueue<FlowDebugSnapshot> snapshots = new ConcurrentQueue<FlowDebugSnapshot>(); runtime.DebugChanged += item => snapshots.Enqueue(item);
        runtime.Start(plan, script.Id, context, true); ScriptUntil(() => runtime.IsDebugPaused, "Waiting debug did not stop at entry."); runtime.ContinueDebug();
        ScriptUntil(() => snapshots.Any(item => item.State == "ActionCompleted"), "Action output was hidden while waiting for the branch.");
        Assert(runtime.IsRunning && !snapshots.Any(item => item.State == "Completed") && (long)snapshots.First(item => item.State == "ActionCompleted").Outputs["Value"] == 7L, "Action-complete snapshot did not preserve the real output.");
        context.Hub.SetParameter(context.RunId, "ScriptReady", true); Wait(runtime.Completion);
        Assert(runtime.Error == null && snapshots.Count(item => item.State == "ActionCompleted") == 1, "Waiting on conditions repeated or lost the script action.");
    }

    /// <summary>Retain a deliberately unawaited controlled call and its resource before publishing node outputs.</summary>
    private static void ScriptUnawaitedCallLifetime()
    {
        FlowScriptObjectProbe probe = new FlowScriptObjectProbe { Hold = true }; FlowResource resource; FlowStep script;
        FlowActions actions = ScriptProbeActions(probe, out resource, out script);
        script.ScriptSource = "Context.CallAsync(\"script-regression/probe\", \"script.probe\", new Dictionary<string, object> { { \"Increment\", Inputs.Increment } });\nOutputs.Count = 17;";
        FlowScripts.Compile(script);
        FlowExecutionContext context = ScriptContext(actions); ConcurrentQueue<FlowDebugSnapshot> snapshots = new ConcurrentQueue<FlowDebugSnapshot>();
        FlowProgramRuntime runtime = new FlowProgramRuntime { ExecuteAction = actions.Execute }; runtime.DebugChanged += item => snapshots.Enqueue(item);
        runtime.Start(new FlowProgram { ModuleId = context.ModuleId, Steps = new List<FlowStep> { script } }, script.Id, context, true);
        ScriptUntil(() => runtime.IsDebugPaused, "Unawaited-call debug did not reach the entry."); runtime.ContinueDebug();
        ScriptUntil(() => probe.Started.IsSet, "Unawaited controlled call never started.");
        try
        {
            Assert(runtime.IsRunning && !FlowResourceGate.CanAccess(probe) && ScriptOutputMissing(context, script, "Count"), "An unawaited call escaped node lifetime or published its parent output early.");
            Assert(snapshots.Last().State == "Running" && !snapshots.Any(item => item.State == "ActionCompleted"), "A long-running controlled call still appeared paused or complete in debug observation.");
        }
        finally { probe.Release.TrySetResult(true); Wait(runtime.Completion, 10000); }
        Assert(runtime.Error == null && probe.Calls == 1 && FlowResourceGate.CanAccess(probe)
            && (long)context.Hub.Read(context.RunId, context.CycleId, context.ModuleId + "/" + script.Id, "Count") == 17L, "Drained unawaited call lost its result or invoked the real object twice.");
        FlowStep closed = ScriptNode("Outputs.Value = 1;", ScriptPort("Value", "Integer"));
        FlowCall call = new FlowCall { Step = closed, Context = context, Inputs = new Dictionary<string, object>(), Token = CancellationToken.None };
        int calls = 0;
        FlowScriptContext closedContext = new FlowScriptContext(call, (target, method, values, token) => { calls++; return Task.FromResult(FlowCall.Result("Value", 1)); });
        Wait(FlowScripts.Execute(closed, call, closedContext), 10000);
        bool rejected = false;
        try { closedContext.CallAsync("any/object", "any.method", new Dictionary<string, object>()); }
        catch (InvalidOperationException) { rejected = true; }
        Assert(rejected && calls == 0, "A completed script context accepted a new detached object call.");
        ScriptUnawaitedImmediateFailure();
    }

    /// <summary>Drain an unawaited blocking object after body failure while preserving its lease and original error.</summary>
    private static void ScriptFailedBodyDrainsCalls()
    {
        FlowScriptObjectProbe probe = new FlowScriptObjectProbe { Hold = true }; FlowResource resource; FlowStep script;
        FlowActions actions = ScriptProbeActions(probe, out resource, out script);
        script.ScriptSource = "Context.CallAsync(\"script-regression/probe\", \"script.probe\", new Dictionary<string, object> { { \"Increment\", Inputs.Increment } });\nthrow new InvalidOperationException(\"BODY-FAILED-BEFORE-DEVICE-END\");";
        FlowScripts.Compile(script);
        FlowExecutionContext context = ScriptContext(actions);
        FlowProgramRuntime runtime = new FlowProgramRuntime { ExecuteAction = actions.Execute };
        runtime.Start(new FlowProgram { ModuleId = context.ModuleId, Steps = new List<FlowStep> { script } }, script.Id, context);
        ScriptUntil(() => probe.Started.IsSet && probe.ObservedToken.IsCancellationRequested, "Body failure did not cancel its outstanding controlled call.");
        try
        {
            Assert(runtime.IsRunning && !FlowResourceGate.CanAccess(probe) && ScriptOutputMissing(context, script, "Count"), "Body failure completed the node before its non-cooperative object returned.");
        }
        finally { probe.Release.TrySetResult(true); Wait(runtime.Completion, 10000); }
        Assert(runtime.Error != null && runtime.Error.Contains("BODY-FAILED-BEFORE-DEVICE-END") && runtime.AlarmWasRaised
            && probe.Calls == 1 && FlowResourceGate.CanAccess(probe) && ScriptOutputMissing(context, script, "Count"), "Failed-body cleanup replaced the original exception, leaked ownership or published late output.");
    }

    /// <summary>Allow a script to catch a transient object failure and succeed after a second awaited call.</summary>
    private static void ScriptHandledCallRetry()
    {
        FlowScriptObjectProbe probe = new FlowScriptObjectProbe { FailuresBeforeSuccess = 1 }; FlowResource resource; FlowStep script;
        FlowActions actions = ScriptProbeActions(probe, out resource, out script);
        script.ScriptSource = "bool retry = false;\ntry {\nawait Context.CallAsync(\"script-regression/probe\", \"script.probe\", new Dictionary<string, object> { { \"Increment\", Inputs.Increment } });\n}\ncatch (InvalidOperationException) { retry = true; }\nif (retry) {\nvar operation = Context.CallAsync(\"script-regression/probe\", \"script.probe\", new Dictionary<string, object> { { \"Increment\", Inputs.Increment } });\nvar retried = await operation.ConfigureAwait(false);\nvar repeated = await operation;\nOutputs.Count = Context.Get<long>(retried, \"Count\");\nif (Context.Get<long>(repeated, \"Count\") != Outputs.Count) throw new InvalidOperationException(\"REPEATED-AWAIT-CHANGED-RESULT\");\n}";
        FlowScripts.Compile(script);
        FlowExecutionContext context = ScriptContext(actions);
        FlowProgramRuntime runtime = new FlowProgramRuntime { ExecuteAction = actions.Execute }; int alarms = 0;
        runtime.AlarmRequested += code => Interlocked.Increment(ref alarms);
        runtime.Start(new FlowProgram { ModuleId = context.ModuleId, Steps = new List<FlowStep> { script } }, script.Id, context);
        Wait(runtime.Completion, 10000);
        Assert(runtime.Error == null && !runtime.AlarmWasRaised && alarms == 0 && probe.Calls == 2 && FlowResourceGate.CanAccess(probe)
            && (long)context.Hub.Read(context.RunId, context.CycleId, context.ModuleId + "/" + script.Id, "Count") == 42L,
            "A caught and recovered object failure was raised again while closing the script scope.");
    }

    /// <summary>Reject an immediately failed call that the script never awaited even when it assigned valid outputs.</summary>
    private static void ScriptUnawaitedImmediateFailure()
    {
        FlowScriptObjectProbe probe = new FlowScriptObjectProbe { FailuresBeforeSuccess = 1 }; FlowResource resource; FlowStep script;
        FlowActions actions = ScriptProbeActions(probe, out resource, out script);
        script.ScriptSource = "Context.CallAsync(\"script-regression/probe\", \"script.probe\", new Dictionary<string, object> { { \"Increment\", Inputs.Increment } });\nOutputs.Count = 99;";
        FlowScripts.Compile(script);
        FlowExecutionContext context = ScriptContext(actions);
        FlowProgramRuntime runtime = new FlowProgramRuntime { ExecuteAction = actions.Execute }; int alarms = 0;
        runtime.AlarmRequested += code => Interlocked.Increment(ref alarms);
        runtime.Start(new FlowProgram { ModuleId = context.ModuleId, Steps = new List<FlowStep> { script } }, script.Id, context);
        Wait(runtime.Completion, 10000);
        Assert(runtime.Error != null && runtime.Error.Contains("SCRIPT-PROBE-TRANSIENT") && runtime.AlarmWasRaised && alarms == 1
            && probe.Calls == 1 && FlowResourceGate.CanAccess(probe) && ScriptOutputMissing(context, script, "Count"),
            "An unawaited immediate object failure was hidden by an otherwise successful script body.");
    }

    /// <summary>Keep each actual DataForm invocation distinct inside a single script node execution.</summary>
    private static void ScriptDistinctProductionCalls()
    {
        string firstProduct = "SCRIPT-MULTISAVE-" + Guid.NewGuid().ToString("N") + "-A", secondProduct = firstProduct + "-B";
        FlowActions actions = new FlowActions(FlowResources.Capture()) { Simulate = true };
        FlowResource storage = actions.FindResource("data/Production");
        Assert(storage != null && storage.Methods.Single(method => method.Id == "data.save").Inputs.Select(field => field.Name).OrderBy(name => name).SequenceEqual(new[] { "Measurement", "Product" }),
            "Production regression fixture no longer contains the expected Product and Measurement fields.");
        string source = "var first = await Context.CallAsync(\"data/Production\", \"data.save\", new Dictionary<string, object> { { \"Product\", \"" + firstProduct + "\" }, { \"Measurement\", 11.0 } });\n"
            + "var second = await Context.CallAsync(\"data/Production\", \"data.save\", new Dictionary<string, object> { { \"Product\", \"" + secondProduct + "\" }, { \"Measurement\", 22.0 } });\n"
            + "Outputs.FirstId = Context.Get<string>(first, \"RecordId\");\nOutputs.SecondId = Context.Get<string>(second, \"RecordId\");";
        FlowStep script = ScriptNode(source, ScriptPort("FirstId", "Text"), ScriptPort("SecondId", "Text")); script.ScriptResources.Add(storage.Id);
        FlowScripts.Compile(script);
        FlowExecutionContext context = ScriptContext(actions);
        FlowProgramRuntime runtime = new FlowProgramRuntime { ExecuteAction = actions.Execute };
        runtime.Start(new FlowProgram { ModuleId = context.ModuleId, Steps = new List<FlowStep> { script } }, script.Id, context);
        Wait(runtime.Completion, 10000);
        Assert(runtime.Error == null, runtime.Error);
        string firstId = (string)context.CurrentResult["FirstId"], secondId = (string)context.CurrentResult["SecondId"];
        Assert(!string.IsNullOrEmpty(firstId) && !string.IsNullOrEmpty(secondId) && firstId != secondId,
            "Distinct controlled save calls reused one node-level production record identity.");
        int files, total;
        List<ProductionRecord> rows = ProductionDataStore.Query(DateTime.Today, DateTime.Today.AddDays(1), "", out files, out total);
        Assert(rows.Count(row => row.Get("RecordId") == firstId && row.Get("Product") == firstProduct && row.Get("Measurement") == "11") == 1
            && rows.Count(row => row.Get("RecordId") == secondId && row.Get("Product") == secondProduct && row.Get("Measurement") == "22") == 1,
            "DataForm deduplication dropped or replaced a different payload from the same script execution.");
    }
}
