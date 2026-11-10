using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NPSDK;

internal static class Regression
{
    private const int TimeoutMs = 5000;

    private sealed class Controller : IFlowProgramController
    {
        public bool IsEnabled { get; set; }
        public Func<FlowProgramCommand, bool> Dispatch;

        public bool Handle(FlowProgramCommand command) { return Dispatch(command); }
    }

    private sealed class LegacyForm : Flow_BaseForm
    {
        private int starts;
        private int resumes;
        public int Starts { get { return Volatile.Read(ref starts); } }
        public int Resumes { get { return Volatile.Read(ref resumes); } }

        public override void StartRun()
        {
            RunningStatus = false;
            Interlocked.Increment(ref starts);
        }
        public override void ContinusAftePause() { Interlocked.Increment(ref resumes); }
    }

    [STAThread]
    private static int Main()
    {
        try
        {
            Run(StopCancelsRemainingStart, "stop revokes remaining starts and drains the in-flight callback");
            Run(StopCancelsRemainingInitialization, "stop revokes remaining initialization callbacks");
            Run(StartFailureStopsModules, "partial startup stops every module even if one stop fails");
            Run(LegacyFallbackAndResume, "handled, legacy, and legacy resume paths dispatch once");
            Run(ParallelExecutionPositions, "parallel positions distinguish an in-flight pause, an actual pause and a pending stop");
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
            return 1;
        }
    }

    /// <summary>阻塞一个入口并完成另一个入口，检查暂停、恢复和停止的位置及状态。</summary>
    private static void ParallelExecutionPositions()
    {
        FlowStep held = new FlowStep { Caption = "Blocked action", ExecutionMode = "None" };
        FlowStep peer = new FlowStep { Caption = "Completed peer", ExecutionMode = "None" };
        FlowProgram plan = new FlowProgram { ModuleId = "Positions", Mode = "Program" };
        plan.Steps.AddRange(new[] { held, peer });
        FlowResultHub hub = new FlowResultHub();
        TaskCompletionSource<IDictionary<string, object>> release = new TaskCompletionSource<IDictionary<string, object>>(TaskCreationOptions.RunContinuationsAsynchronously);
        FlowProgramGroup group = new FlowProgramGroup();
        group.ExecuteAction = (step, context, token) => step.Id == held.Id
            ? release.Task : Task.FromResult<IDictionary<string, object>>(new Dictionary<string, object>());
        group.StartMany(plan, new[] { held.Id, peer.Id }, new FlowExecutionContext { Hub = hub, RunId = hub.Epoch, CycleId = "probe", ModuleId = plan.ModuleId });
        try
        {
            ExpectPosition(group, held, "Running");
            ExpectPosition(group, peer, "Completed");
            group.Pause();
            ExpectPosition(group, held, "PausePending");
            ExpectPosition(group, peer, "Completed");
            release.TrySetResult(new Dictionary<string, object>());
            ExpectPosition(group, held, "Paused");
            group.Resume();
            Require(group.Completion.Wait(TimeoutMs), "resume did not finish");
            ExpectPosition(group, held, "Completed");

            release = new TaskCompletionSource<IDictionary<string, object>>(TaskCreationOptions.RunContinuationsAsynchronously);
            group.StartMany(plan, new[] { held.Id }, new FlowExecutionContext { Hub = hub, RunId = hub.Epoch, CycleId = "restart", ModuleId = plan.ModuleId });
            ExpectPosition(group, held, "Running");
            group.Stop();
            ExpectPosition(group, held, "Stopping");
            Require(group.IsRunning && group.GetExecutionPositions().Length == 1, "stop lost the current position or retained an old entry");
            release.TrySetResult(new Dictionary<string, object>());
            Require(group.Completion.Wait(TimeoutMs), "stop did not finish");
            ExpectPosition(group, held, "Cancelled");
        }
        finally
        {
            group.Stop();
            release.TrySetResult(new Dictionary<string, object>());
            group.Completion.Wait(TimeoutMs);
        }
    }

    /// <summary>等待指定入口报告当前节点及状态。</summary>
    private static void ExpectPosition(FlowProgramGroup group, FlowStep step, string state)
    {
        Require(SpinWait.SpinUntil(() => group.GetExecutionPositions().Any(position =>
            position.StepId == step.Id && position.Caption == step.Caption && position.State == state), TimeoutMs),
            step.Caption + " did not report " + state);
    }

    /// <summary>Keep form registration and global command flags isolated between checks.</summary>
    private static void Run(Action check, string description)
    {
        try { check(); Console.WriteLine("PASS " + description); }
        finally
        {
            foreach (Flow_BaseForm module in Flow_Module.FlowChart_ModuleList.ToArray()) module.Dispose();
            Flow_Module.FlowChart_ModuleList.Clear();
            Flow_Module.AllPause = false;
            Flow_Module.AllStop = false;
        }
    }

    /// <summary>Allow the first start and stop to overlap, then release each callback explicitly.</summary>
    private static void StopCancelsRemainingStart()
    {
        Flow_BaseForm first = new Flow_BaseForm();
        Flow_BaseForm next = new Flow_BaseForm();
        int firstStarts = 0;
        int nextStarts = 0;
        int stops = 0;
        int stopping = 0;
        int overlappingStops = 0;
        using (ManualResetEventSlim startEntered = new ManualResetEventSlim())
        using (ManualResetEventSlim releaseStart = new ManualResetEventSlim())
        using (ManualResetEventSlim stopEntered = new ManualResetEventSlim())
        using (ManualResetEventSlim releaseStop = new ManualResetEventSlim())
        {
            first.ProgramController = new Controller { IsEnabled = true, Dispatch = command =>
            {
                if (command == FlowProgramCommand.Start)
                {
                    Interlocked.Increment(ref firstStarts);
                    startEntered.Set();
                    Require(releaseStart.Wait(TimeoutMs), "start callback was not released");
                    first.RunningStatus = true;
                }
                if (command == FlowProgramCommand.Stop)
                {
                    if (Interlocked.Increment(ref stopping) > 1) Interlocked.Increment(ref overlappingStops);
                    if (Interlocked.Increment(ref stops) == 1)
                    {
                        stopEntered.Set();
                        Require(releaseStop.Wait(TimeoutMs), "stop callback was not released");
                    }
                    Interlocked.Decrement(ref stopping);
                }
                return true;
            } };
            next.ProgramController = new Controller { IsEnabled = true, Dispatch = command =>
            {
                if (command == FlowProgramCommand.Start) Interlocked.Increment(ref nextStarts);
                return true;
            } };

            Task stop = null;
            try
            {
                Flow_Module.Module_StartRun();
                Require(startEntered.Wait(TimeoutMs), "first module did not start");
                Flow_Module.Module_StartRun();
                stop = Task.Run((Action)Flow_Module.Module_StopRun);
                Require(stopEntered.Wait(TimeoutMs), "stop was blocked by the running start callback");
                Require(Flow_Module.AllStop, "stop was not published before notifying the modules");
                Flow_Module.Module_StartRun();
            }
            finally
            {
                releaseStart.Set();
                releaseStop.Set();
                if (stop != null) Require(stop.Wait(TimeoutMs), "stop task did not exit");
            }
            WaitForDispatch();
            Require(stops == 2 && overlappingStops == 0, "in-flight start needs one non-overlapping follow-up stop");
            Require(firstStarts == 1 && nextStarts == 0, "an old or duplicate start reached a module");
            Require(Flow_Module.AllStop && !first.RunningStatus && first.StopStatus, "old callback restored running status");

            Flow_Module.Module_StartRun();
            Require(SpinWait.SpinUntil(() => Volatile.Read(ref nextStarts) == 1, TimeoutMs), "fresh start was not dispatched");
            Flow_Module.Module_StopRun();
            WaitForDispatch();
        }
    }

    /// <summary>A reentrant stop from initialization must prevent later modules from initializing.</summary>
    private static void StopCancelsRemainingInitialization()
    {
        Flow_BaseForm first = new Flow_BaseForm();
        Flow_BaseForm next = new Flow_BaseForm();
        int nextInitializations = 0;
        first.ProgramController = new Controller { IsEnabled = true, Dispatch = command =>
        {
            if (command == FlowProgramCommand.Initialize) Flow_Module.Module_StopRun();
            return true;
        } };
        next.ProgramController = new Controller { IsEnabled = true, Dispatch = command =>
        {
            if (command == FlowProgramCommand.Initialize) nextInitializations++;
            return true;
        } };
        Flow_Module.Module_InitialRun();
        Require(nextInitializations == 0 && Flow_Module.AllStop, "old initialization continued after stop");
    }

    /// <summary>Verify failure containment after a prior module has already accepted initialization.</summary>
    private static void StartFailureStopsModules()
    {
        Flow_BaseForm first = new Flow_BaseForm();
        Flow_BaseForm broken = new Flow_BaseForm();
        Flow_BaseForm last = new Flow_BaseForm();
        int starts = 0;
        int stops = 0;
        first.ProgramController = new Controller { IsEnabled = true, Dispatch = command =>
        {
            if (command == FlowProgramCommand.Initialize) starts++;
            if (command == FlowProgramCommand.Stop) throw new InvalidOperationException("stop failed");
            return true;
        } };
        broken.ProgramController = new Controller { IsEnabled = true, Dispatch = command =>
        {
            if (command == FlowProgramCommand.Initialize) throw new InvalidOperationException("start failed");
            return true;
        } };
        last.ProgramController = new Controller { IsEnabled = true, Dispatch = command =>
        {
            if (command == FlowProgramCommand.Initialize) starts++;
            if (command == FlowProgramCommand.Stop) stops++;
            return true;
        } };
        AggregateException failure = null;
        try { Flow_Module.Module_InitialRun(); }
        catch (AggregateException error) { failure = error.Flatten(); }
        Require(failure != null && failure.InnerExceptions.Count == 2, "startup and stop errors should both be reported");
        Require(starts == 1 && stops == 1 && Flow_Module.AllStop, "a partial failure did not stop every module");
    }

    /// <summary>Preserve legacy fallback, handled disabled modules, and resume semantics.</summary>
    private static void LegacyFallbackAndResume()
    {
        LegacyForm legacy = new LegacyForm();
        LegacyForm handled = new LegacyForm();
        handled.ProgramController = new Controller { IsEnabled = false, Dispatch = command => true };
        Flow_Module.Module_StartRun();
        Require(SpinWait.SpinUntil(() => legacy.Starts == 1, TimeoutMs), "legacy start not dispatched");
        WaitForDispatch();
        legacy.RunningStatus = true;
        Flow_Module.Module_PauseRun();
        Flow_Module.Module_StartRun();
        Require(SpinWait.SpinUntil(() => legacy.Resumes == 1, TimeoutMs), "legacy resume not dispatched");
        Flow_Module.Module_StopRun();
        WaitForDispatch();
        Require(legacy.Starts == 1 && handled.Starts == 0, "a handled or resumed module received a legacy start");
    }

    /// <summary>Check that no module callback or pending command still owns the dispatcher.</summary>
    private static void WaitForDispatch()
    {
        Require(SpinWait.SpinUntil(() => !Flow_Module.Module_GetRunningStatus, TimeoutMs), "dispatcher did not drain");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
