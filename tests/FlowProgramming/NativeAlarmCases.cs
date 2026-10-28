using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using AlphaRap;
using AlphaRap.FlowProgramming;
using NPSDK;

internal sealed class NativeAlarmPage : Flow_BaseForm
{
    internal readonly Flow_Chart Node;
    internal volatile bool Ready;
    internal int Calls;
    internal int WaitMilliseconds;

    /// <summary>Represent a source code node whose alarm policy is configured on its original SDK control.</summary>
    internal NativeAlarmPage(string name, bool enabled)
    {
        Name = name;
        Node = new Flow_Chart { Name = "sourceAlarmNode", text = "Source alarm policy", AlarmStaus = enabled, AlarmTimeOut = 180, AlarmCode = "4004" };
        Node.FlowChartRun += WaitForInput; InitialPage.Controls.Add(Node);
    }

    /// <summary>Wait without raising an application alarm or invoking any object action.</summary>
    [FlowTool("等待条件", "Wait for condition", "Esperar condición", UsesHardware = false)]
    private Flow_Chart.ResultType WaitForInput() { Interlocked.Increment(ref Calls); return Ready || WaitMilliseconds > 0 && Node.FlowChart_ElapsedMilliseconds >= WaitMilliseconds ? Flow_Chart.ResultType.NEXT : Flow_Chart.ResultType.IDLE; }
}

internal static partial class Cases
{
    /// <summary>Exercise native source alarm settings, old configuration migration and separation from action timeouts.</summary>
    private static void RegisterNativeAlarmCases()
    {
        Test("source native timeout uses its SDK alarm switch duration and code", NativeSourceAlarmImport);
        Test("old native defaults migrate from the immutable source policy while layout and links survive", NativeSourceAlarmMigration);
        Test("disabled native alarms wait until stopped and mixed actions keep their own deadline", NativeDisabledAndMixedAlarms);
        Test("new native toolbox nodes have no implicit Action deadline or fallback alarm code", NativeToolAlarmDefaults);
        Test("actual DownConveryor code binding preserves the compiled alarm policy without invoking IO", ActualConveyorNativeAlarm);
        Test("native elapsed-time code and the corner display share one clock and retain the completed value", NativeElapsedClock);
    }

    /// <summary>Prepare an isolated native source node without starting machine threads or writing site configuration.</summary>
    private static void WithNativeAlarm(bool enabled, Action<NativeAlarmPage, FlowModuleEditor, FlowActions> test)
    {
        using (NativeAlarmPage page = new NativeAlarmPage("NativeAlarm_" + Guid.NewGuid().ToString("N"), enabled))
        {
            FlowModuleEditor owner = FlowPrograms.Attach(page);
            FlowActions actions = new FlowActions(FlowResources.Capture(true), page);
            try { test(page, owner, actions); }
            finally { owner.Runtime.Stop(); Wait(owner.Runtime.Completion); }
        }
    }

    /// <summary>Use the real callback adapter and verify one explicit source alarm instead of the generic default.</summary>
    private static void NativeSourceAlarmImport()
    {
        WithNativeAlarm(true, (page, owner, actions) =>
        {
            FlowStep step = owner.Program.Steps.Single();
            Assert(step.UsesFlowResult && step.TimeoutMs == 180 && step.AlarmCode == "4004", "Native import adopted Action's timeout or generic alarm.");
            int alarms = 0; string alarm = null;
            FlowProgramRuntime runtime = NativeRuntime(actions);
            runtime.AlarmRequested += code => { alarm = code; Interlocked.Increment(ref alarms); };
            try
            {
                runtime.Start(owner.Program, step.Id, Context(new FlowResultHub())); Wait(runtime.Completion);
                Assert(runtime.Error != null && runtime.Error.Contains("180 ms") && alarms == 1 && alarm == "4004" && page.Calls > 1,
                    "A native IDLE timeout did not use its original source alarm configuration.");
            }
            finally { runtime.Stop(); Wait(runtime.Completion); }
        });
    }

    /// <summary>Repair a previous 10000ms/empty-code import in memory, preserving saved identity, layout and file contents.</summary>
    private static void NativeSourceAlarmMigration()
    {
        WithNativeAlarm(true, (page, owner, actions) =>
        {
            FlowProgram old = FlowProgramFile.Snapshot(owner.Program);
            FlowStep saved = old.Steps.Single(); saved.TimeoutMs = 10000; saved.AlarmCode = null; saved.X = 333; saved.Y = 444;
            string path = Path.Combine(root, "native-alarm-" + Guid.NewGuid().ToString("N") + ".xml");
            FlowProgramFile.Save(path, new FlowProject { Modules = new List<FlowProgram> { old } });
            byte[] bytes = File.ReadAllBytes(path);
            page.Node.AlarmTimeOut = 9999; page.Node.AlarmCode = "runtime-mutated"; page.Node.AlarmStaus = false;
            owner.Restore(FlowProgramFile.Load(path).Modules.Single());
            FlowStep restored = owner.Program.Steps.Single();
            Assert(restored.TimeoutMs == 180 && restored.AlarmCode == "4004" && restored.Id == saved.Id && restored.X == 333 && restored.Y == 444
                && bytes.SequenceEqual(File.ReadAllBytes(path)) && page.Calls == 0, "Source alarm migration lost its original policy or changed disk/layout/identity.");
            using (NativeAlarmPage next = new NativeAlarmPage(page.Name, false))
            {
                FlowModuleEditor rebuilt = new FlowModuleEditor(next, old);
                Assert(rebuilt.Program.Steps.Single().TimeoutMs == System.Threading.Timeout.Infinite && rebuilt.Program.Steps.Single().AlarmCode == null,
                    "Disabling the alarm in recompiled source retained an old default timeout.");
                FlowProgramFile.Save(path, new FlowProject { Modules = new List<FlowProgram> { rebuilt.Program } });
                Assert(FlowProgramFile.Load(path).Modules.Single().Steps.Single().TimeoutMs == System.Threading.Timeout.Infinite,
                    "The main-save model could not persist and load a native node with its alarm disabled.");
            }
        });
    }

    /// <summary>Leave a native IDLE node pending without alarms, then route into a separately configured action timeout.</summary>
    private static void NativeDisabledAndMixedAlarms()
    {
        WithNativeAlarm(false, (page, owner, actions) =>
        {
            FlowStep native = owner.Program.Steps.Single();
            FlowStep action = new FlowStep { Caption = "Action wait", Action = "value", TimeoutMs = 120, AlarmCode = "ACTION-ONLY",
                Inputs = new List<FlowInput> { new FlowInput { Name = "Value", Reference = "actual" } },
                Branches = new List<FlowBranch> { new FlowBranch { Conditions = new List<FlowCondition> {
                    new FlowCondition { Left = new FlowInput { Source = "Result", Reference = "Value" }, Operator = "==", Right = new FlowInput { Reference = "never" } }
                } } } };
            native.Next = action.Id; owner.Program.Steps.Add(action);
            int alarms = 0; string alarm = null;
            FlowProgramRuntime runtime = NativeRuntime(actions); runtime.AlarmRequested += code => { alarm = code; Interlocked.Increment(ref alarms); };
            try
            {
                runtime.Start(owner.Program, native.Id, Context(new FlowResultHub())); NativePump(350);
                Assert(runtime.IsRunning && runtime.Error == null && alarms == 0 && page.Calls > 1, "A disabled native alarm inherited an Action timeout.");
                runtime.Stop(); Wait(runtime.Completion);
                Assert(runtime.Error == "Cancelled" && alarms == 0, "Stopping the untimed native node raised an alarm.");
                page.Ready = true;
                runtime.Start(owner.Program, native.Id, Context(new FlowResultHub())); Wait(runtime.Completion);
                Assert(runtime.Error != null && runtime.Error.Contains("Action wait") && alarm == "ACTION-ONLY" && alarms == 1,
                    "Mixed native/action execution confused their independent deadline or alarm settings.");
            }
            finally { runtime.Stop(); Wait(runtime.Completion); }
        });
    }

    /// <summary>Dragging a plain native function does not silently add a 10000ms alarm that its author did not configure.</summary>
    private static void NativeToolAlarmDefaults()
    {
        WithNativeAlarm(false, (page, owner, actions) =>
        {
            FlowResource source = actions.Resources.Items.Values.Single(resource => resource.Kind == "native" && ReferenceEquals(resource.Instance, page));
            FlowStep native = owner.AddNode(source.Id, source.Methods.Single().Id, page.InitialPage, new Point(40, 80));
            FlowStep action = owner.AddNode("Action", page.InitialPage, new Point(40, 140));
            Assert(native.UsesFlowResult && native.TimeoutMs == System.Threading.Timeout.Infinite && native.AlarmCode == null
                && !action.UsesFlowResult && action.TimeoutMs == 10000 && action.AlarmCode == "2019", "Toolbox defaults mixed native and action alarm policies.");
        });
    }

    /// <summary>Run an actual timed native callback without a sleep, verify the visible value freezes, then restart its clock.</summary>
    private static void NativeElapsedClock()
    {
        WithNativeAlarm(false, (page, owner, actions) =>
        {
            page.WaitMilliseconds = 100;
            owner.Start(owner.Program.Steps.Single().Id, false, true);
            Wait(owner.Runtime.Completion); owner.RefreshElapsed();
            long elapsed = page.Node.FlowChart_ElapsedMilliseconds;
            Assert(owner.Runtime.Error == null && elapsed >= 100 && elapsed < 2000 && page.Node.Controls.Find("elapsedTime", true).Single().Text.EndsWith(" s"),
                "A native event could not use the same elapsed clock displayed in the node corner.");
            NativePump(100);
            Assert(page.Node.FlowChart_ElapsedMilliseconds == elapsed, "A completed node kept accumulating time.");
            page.Ready = true; owner.Start(owner.Program.Steps.Single().Id, false, true); Wait(owner.Runtime.Completion);
            Assert(page.Node.FlowChart_ElapsedMilliseconds < elapsed, "A new initialization reused the previous elapsed time.");
        });
    }

    /// <summary>Inspect the actual reported business node and its compiled event without invoking its hardware callback.</summary>
    private static void ActualConveyorNativeAlarm()
    {
        Type type = typeof(MainForm).Assembly.GetType("AlphaRap.TaskProcess.DownConveryor", true);
        using (Flow_BaseForm page = (Flow_BaseForm)Activator.CreateInstance(type))
        {
            FlowModuleEditor owner = new FlowModuleEditor(page, new FlowProgram { ModuleId = type.FullName + "/" + page.Name });
            FlowStep step = owner.Program.Steps.Single(item => item.ControlName == "DownConveyInit_Flow1_3");
            Flow_Chart node = page.InitialPage.Controls.OfType<Flow_Chart>().Single(item => item.Name == step.ControlName);
            Flow_Chart.FlowChartHandle[] handlers = node.GetFlowChartHandlers();
            Assert(step.UsesFlowResult && step.Action.StartsWith("native.DownConveyInit_Flow1_3_FlowChartRun.", StringComparison.Ordinal)
                && handlers.Length == 1 && handlers[0].Method.Name == "DownConveyInit_Flow1_3_FlowChartRun"
                && step.TimeoutMs == (node.AlarmStaus ? Math.Max(1, node.AlarmTimeOut) : System.Threading.Timeout.Infinite)
                && step.AlarmCode == (node.AlarmStaus ? node.AlarmCode : null),
                "The actual conveyor callback or its compiled alarm policy was replaced by Action defaults.");
            Flow_Module.FlowChart_ModuleList.Remove(page);
        }
    }
}
