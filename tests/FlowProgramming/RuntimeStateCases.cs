using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using AlphaRap;
using AlphaRap.FlowProgramming;
using NPSDK;

internal sealed class RuntimeStatePage : Flow_BaseForm
{
    internal readonly Flow_Chart Node = new Flow_Chart { Name = "runtimeNode", text = "Runtime state" };
    internal readonly ManualResetEventSlim Entered = new ManualResetEventSlim();
    internal readonly ManualResetEventSlim Release = new ManualResetEventSlim();

    /// <summary>Create a pure offline node with no equipment or IO.</summary>
    internal RuntimeStatePage()
    {
        Name = "RuntimeState_" + Guid.NewGuid().ToString("N");
        Node.FlowChartRun += Poll;
        InitialPage.Controls.Add(Node);
    }

    /// <summary>Wait cooperatively until the node deadline without accessing hardware.</summary>
    [FlowTool("状态等待", "State wait", "Espera de estado", UsesHardware = false)]
    private Flow_Chart.ResultType Poll() { return Flow_Chart.ResultType.IDLE; }

    /// <summary>Hold an actual invocation so cancellation cannot falsely report it finished.</summary>
    [FlowTool("状态阻塞", "Blocked state", "Estado bloqueado", UsesHardware = false)]
    private Flow_Chart.ResultType Block() { Entered.Set(); Release.Wait(); return Flow_Chart.ResultType.End; }

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
}
