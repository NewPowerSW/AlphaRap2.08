using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NPSDK;

internal static class Regression
{
    private const int Timeout = 5000;
    private static int passed;

    /// <summary>运行不连接设备的启动、取消、链接和重复执行回归。</summary>
    [STAThread]
    private static int Main()
    {
        try
        {
            Run(DuplicateAndRestart, "duplicate starts and completed restart");
            Run(BlockedStop, "stop returns while a network callback is blocked, forbids restart and drops its exit");
            Run(CooperativeStop, "cooperative cancellation releases a network wait");
            Run(BlockedCancellation, "a blocked cancellation callback does not block stop or allow early restart");
            Run(LinkSnapshot, "links are frozen for the active run");
            Run(InvalidLinks, "disposed, cross-form, missing and ambiguous handlers are rejected before execution");
            Run(MissingCase, "missing CASE and unknown exits are reported");
            Run(Cycle, "cycles share one task and terminate without thread-per-node scheduling");
            Run(PauseAtBoundary, "pause resumes without repeating a completed callback");
            Run(StopWhilePaused, "stop cancels paused and IDLE flows");
            Run(DisposeBlocked, "disposal cancels the blocked flow and prevents subsequent calls");
            Run(ExceptionRecovery, "callback failure releases the entire graph for a later run");
            Run(SharedTarget, "independent starts cannot race through a shared target");
            Run(ConcurrentLogs, "concurrent logging remains bounded and readable");
            Console.WriteLine("RESULT " + passed + "/14 passed");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }

    /// <summary>隔离每项测试的全局节点与停止状态。</summary>
    private static void Run(Action action, string name)
    {
        Flow_Module.AllStop = Flow_Module.AllPause = false;
        try { action(); passed++; Console.WriteLine("PASS " + name); }
        finally
        {
            Flow_Module.Module_StopRun();
            foreach (Flow_Chart node in Flow_Module.FlowChart_List.ToArray()) node.Dispose();
            foreach (Flow_BaseForm form in Flow_Module.FlowChart_ModuleList.ToArray()) form.Dispose();
            Flow_Module.FlowChart_List.Clear();
            Flow_Module.FlowChart_ModuleList.Clear();
        }
    }

    /// <summary>创建仅使用内存回调的测试节点。</summary>
    private static Flow_Chart Node(Flow_Chart.FlowChartHandle handler)
    {
        Flow_Chart node = new Flow_Chart { Name = "Probe", text = "Probe" };
        if (handler != null) node.FlowChartRun += handler;
        return node;
    }

    /// <summary>等待真实完成任务退出，不依赖界面消息泵。</summary>
    private static void Finish(Flow_Chart node)
    {
        Require(node.task != null && node.task.Wait(Timeout), "Flow did not complete.");
        Require(!node.IsLegacyRunning && !node.RunningStatus, "Completed flow remained busy.");
    }

    /// <summary>并发重复启动只执行一次，完成后可正常再启动。</summary>
    private static void DuplicateAndRestart()
    {
        int count = 0;
        using (ManualResetEventSlim entered = new ManualResetEventSlim())
        using (ManualResetEventSlim release = new ManualResetEventSlim())
        {
            Flow_Chart node = Node(() => { Interlocked.Increment(ref count); entered.Set(); Require(release.Wait(Timeout), "release"); return Flow_Chart.ResultType.End; });
            node.FlowChart_Run(); Require(entered.Wait(Timeout), "start");
            Parallel.For(0, 100, i => node.FlowChart_Run());
            Require(count == 1, "Duplicate callbacks ran.");
            release.Set(); Finish(node);
            node.FlowChart_Run(); Finish(node); Require(count == 2, "Restart did not execute once.");
        }
    }

    /// <summary>模拟网络阻塞，停止不等待网络，旧结果和迟到写命令均被拒绝。</summary>
    private static void BlockedStop()
    {
        int downstream = 0, writes = 0, calls = 0;
        using (ManualResetEventSlim entered = new ManualResetEventSlim())
        using (ManualResetEventSlim release = new ManualResetEventSlim())
        {
            Flow_Chart node = Node(() => { Interlocked.Increment(ref calls); entered.Set(); Require(release.Wait(Timeout), "release"); Flow_Chart.ThrowIfExecutionStopped(); writes++; return Flow_Chart.ResultType.NEXT; });
            node.Next = Node(() => { downstream++; return Flow_Chart.ResultType.End; });
            node.FlowChart_Run(); Require(entered.Wait(Timeout), "start");
            Stopwatch elapsed = Stopwatch.StartNew(); Flow_Module.Module_StopRun();
            Require(elapsed.ElapsedMilliseconds < 500, "Stop blocked on the network callback.");
            Require(node.IsStopping && node.IsLegacyRunning && !node.task.IsCompleted, "Blocked call was marked finished.");
            Flow_Module.Module_StartRun(); node.FlowChart_Run();
            Require(Flow_Module.AllStop && calls == 1, "Restart bypassed the blocked call.");
            release.Set(); Finish(node);
            Require(writes == 0 && downstream == 0, "A late callback sent commands or advanced.");
        }
    }

    /// <summary>网络等待使用令牌时可以直接被停止请求唤醒。</summary>
    private static void CooperativeStop()
    {
        using (ManualResetEventSlim entered = new ManualResetEventSlim())
        {
            Flow_Chart node = Node(() => { entered.Set(); Task.Delay(Timeout, Flow_Chart.CurrentExecutionToken).GetAwaiter().GetResult(); return Flow_Chart.ResultType.End; });
            node.FlowChart_Run(); Require(entered.Wait(Timeout), "start");
            Flow_Module.Module_StopRun(); Finish(node);
            Require(node.LastRunError == null, "Normal cancellation was reported as a failure.");
        }
    }

    /// <summary>取消回调自行阻塞时，停止调用仍立即返回且会话不会提前释放。</summary>
    private static void BlockedCancellation()
    {
        using (ManualResetEventSlim entered = new ManualResetEventSlim())
        using (ManualResetEventSlim cancelling = new ManualResetEventSlim())
        using (ManualResetEventSlim release = new ManualResetEventSlim())
        {
            Flow_Chart node = Node(() => {
                Flow_Chart.CurrentExecutionToken.Register(() => { cancelling.Set(); Require(release.Wait(Timeout), "cancel release"); });
                entered.Set(); return Flow_Chart.ResultType.IDLE;
            });
            node.FlowChart_Run(); Require(entered.Wait(Timeout), "start");
            Stopwatch elapsed = Stopwatch.StartNew(); Flow_Module.Module_StopRun();
            Require(elapsed.ElapsedMilliseconds < 500 && cancelling.Wait(Timeout), "Stop blocked on cancellation.");
            Require(node.IsLegacyRunning, "Cancellation cleanup was released early.");
            release.Set(); Finish(node);
        }
    }

    /// <summary>运行中的属性修改只影响下一次启动。</summary>
    private static void LinkSnapshot()
    {
        int first = 0, replacement = 0;
        using (ManualResetEventSlim entered = new ManualResetEventSlim())
        using (ManualResetEventSlim release = new ManualResetEventSlim())
        {
            Flow_Chart node = Node(() => { entered.Set(); Require(release.Wait(Timeout), "release"); return Flow_Chart.ResultType.NEXT; });
            node.Next = Node(() => { first++; return Flow_Chart.ResultType.End; });
            node.FlowChart_Run(); Require(entered.Wait(Timeout), "start");
            node.Next = Node(() => { replacement++; return Flow_Chart.ResultType.End; });
            release.Set(); Finish(node); Require(first == 1 && replacement == 0, "Active graph changed.");
            node.FlowChart_Run(); Finish(node); Require(replacement == 1, "New graph was not captured.");
        }
    }

    /// <summary>启动前发现错误链接，整条流程尚未产生业务动作。</summary>
    private static void InvalidLinks()
    {
        int calls = 0;
        Flow_Chart node = Node(() => { calls++; return Flow_Chart.ResultType.NEXT; });
        node.Next = Node(null); node.FlowChart_Run(); Require(calls == 0 && node.LastRunError != null, "Missing handler accepted.");
        node.Next.Dispose(); node.FlowChart_Run(); Require(calls == 0 && node.LastRunError != null, "Disposed target accepted.");
        node.Next = null; node.FlowChartRun += () => Flow_Chart.ResultType.End;
        node.FlowChart_Run(); Require(calls == 0 && node.LastRunError != null, "Multiple handlers accepted.");
        using (Form a = new Form()) using (Form b = new Form())
        {
            Flow_Chart source = Node(() => { calls++; return Flow_Chart.ResultType.NEXT; });
            Flow_Chart target = Node(() => Flow_Chart.ResultType.End);
            a.Controls.Add(source); b.Controls.Add(target); source.Next = target;
            source.FlowChart_Run(); Require(calls == 0 && source.LastRunError != null, "Cross-form link accepted.");
        }
    }

    /// <summary>缺失出口和非法结果不会静默按结束处理。</summary>
    private static void MissingCase()
    {
        foreach (Flow_Chart.ResultType result in new[] { Flow_Chart.ResultType.CASE1, Flow_Chart.ResultType.CASE2, Flow_Chart.ResultType.CASE3, (Flow_Chart.ResultType)99 })
        {
            Flow_Chart node = Node(() => result); node.FlowChart_Run(); Finish(node);
            Require(node.LastRunError != null, "Invalid exit silently ended.");
        }
    }

    /// <summary>两节点循环共用完成任务，避免每次跳转创建专用线程。</summary>
    private static void Cycle()
    {
        int count = 0;
        Flow_Chart a = Node(() => ++count == 100 ? Flow_Chart.ResultType.End : Flow_Chart.ResultType.NEXT);
        Flow_Chart b = Node(() => Flow_Chart.ResultType.NEXT); a.Next = b; b.Next = a;
        a.FlowChart_Run(); Require(ReferenceEquals(a.task, b.task), "Chain has multiple completion tasks.");
        Finish(a); Require(count == 100 && a.LastRunError == null, "Cycle failed.");
    }

    /// <summary>回调结束时暂停，恢复后仅继续跳转，不重复当前回调。</summary>
    private static void PauseAtBoundary()
    {
        int first = 0, next = 0;
        Flow_Chart a = Node(() => { first++; Flow_Module.AllPause = true; return Flow_Chart.ResultType.NEXT; });
        a.Next = Node(() => { next++; return Flow_Chart.ResultType.End; });
        a.FlowChart_Run(); Require(SpinWait.SpinUntil(() => Flow_Module.AllPause, Timeout), "pause");
        Thread.Sleep(60); Require(next == 0, "Advanced while paused.");
        Flow_Module.AllPause = false; Finish(a); Require(first == 1 && next == 1, "Resume replayed a callback.");
    }

    /// <summary>暂停和 IDLE 等待都能结束，停止优先于暂停。</summary>
    private static void StopWhilePaused()
    {
        foreach (bool paused in new[] { true, false })
        {
            Flow_Module.AllStop = false; Flow_Module.AllPause = paused;
            Flow_Chart node = Node(() => Flow_Chart.ResultType.IDLE); node.FlowChart_Run();
            Flow_Module.Module_StopRun(); Finish(node);
        }
    }

    /// <summary>控件释放撤销尚未完成的会话。</summary>
    private static void DisposeBlocked()
    {
        int next = 0;
        using (ManualResetEventSlim entered = new ManualResetEventSlim())
        using (ManualResetEventSlim release = new ManualResetEventSlim())
        {
            Flow_Chart node = Node(() => { entered.Set(); Require(release.Wait(Timeout), "release"); return Flow_Chart.ResultType.NEXT; });
            node.Next = Node(() => { next++; return Flow_Chart.ResultType.End; });
            node.FlowChart_Run(); Require(entered.Wait(Timeout), "start");
            node.Dispose(); release.Set(); Finish(node); Require(next == 0, "Disposed flow advanced.");
        }
    }

    /// <summary>异常不残留忙碌状态，修复回调后同一节点可以再启动。</summary>
    private static void ExceptionRecovery()
    {
        bool fail = true;
        Flow_Chart node = Node(() => { if (fail) throw new InvalidOperationException("probe"); return Flow_Chart.ResultType.End; });
        node.FlowChart_Run(); Finish(node); Require(node.LastRunError == "probe", "Failure missing.");
        fail = false; node.FlowChart_Run(); Finish(node); Require(node.LastRunError == null, "Failure not reset.");
    }

    /// <summary>共享下游节点的两个入口不能同时获得执行资格。</summary>
    private static void SharedTarget()
    {
        using (ManualResetEventSlim entered = new ManualResetEventSlim())
        using (ManualResetEventSlim release = new ManualResetEventSlim())
        {
            Flow_Chart first = Node(() => { entered.Set(); Require(release.Wait(Timeout), "release"); return Flow_Chart.ResultType.NEXT; });
            Flow_Chart target = Node(() => Flow_Chart.ResultType.End); first.Next = target;
            int secondCalls = 0;
            Flow_Chart second = Node(() => { secondCalls++; return Flow_Chart.ResultType.NEXT; }); second.Next = target;
            first.FlowChart_Run(); Require(entered.Wait(Timeout), "start"); second.FlowChart_Run();
            Require(secondCalls == 0 && second.LastRunError != null, "Shared target was entered twice.");
            release.Set(); Finish(first);
        }
    }

    /// <summary>多条流程并发记录日志时不破坏队列。</summary>
    private static void ConcurrentLogs()
    {
        while (Flow_Module.Module_GetRunLog() != null) { }
        Parallel.For(0, 5000, i => Flow_Module.Module_AddRunLog(i.ToString()));
        int count = 0; while (Flow_Module.Module_GetRunLog() != null) count++;
        Require(count > 0 && count <= 101, "Log queue exceeded capacity.");
    }

    /// <summary>报告可定位的测试失败。</summary>
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
