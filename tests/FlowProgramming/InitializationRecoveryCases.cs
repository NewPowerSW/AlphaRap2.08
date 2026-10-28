using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using AlphaRap;
using AlphaRap.FlowProgramming;
using NPSDK;

internal sealed class InitializationRecoveryPage : Flow_BaseForm
{
    internal readonly Flow_Chart Entry = new Flow_Chart { Name = "beforeFailure", text = "Completed initialization" };
    internal readonly Flow_Chart Work = new Flow_Chart { Name = "recoverableWork", text = "Recoverable initialization" };
    internal readonly Flow_Chart Last = new Flow_Chart { Name = "afterFailure", text = "Finish initialization" };
    internal readonly Flow_Chart Automatic = new Flow_Chart { Name = "automatic", text = "Automatic production" };
    internal int EntryCalls, WorkCalls, LastCalls, AutomaticCalls;
    internal volatile bool Ready;
    internal string Failure;
    internal Func<bool> PeerCompleted;

    /// <summary>Build an offline native sequence with two previously completed steps around a recoverable operation.</summary>
    internal InitializationRecoveryPage()
    {
        Name = "InitializationRecovery";
        Entry.Next = Work; Work.Next = Last;
        Entry.FlowChartRun += BeforeFailure; Work.FlowChartRun += RecoverableWork;
        Last.FlowChartRun += AfterFailure; Automatic.FlowChartRun += AutomaticWork;
        InitialPage.Controls.AddRange(new Control[] { Entry, Work, Last }); AutoPage.Controls.Add(Automatic);
    }

    /// <summary>Count the initialization work that must not be repeated on continuation.</summary>
    [FlowTool("初始化前段", "Before initialization failure", "Antes del fallo", UsesHardware = false)]
    private Flow_Chart.ResultType BeforeFailure() { Interlocked.Increment(ref EntryCalls); return Flow_Chart.ResultType.NEXT; }

    /// <summary>Simulate a user alarm, exception or deadline without connecting or moving any equipment.</summary>
    [FlowTool("初始化恢复", "Recover initialization", "Recuperar inicialización", UsesHardware = false)]
    private Flow_Chart.ResultType RecoverableWork()
    {
        Interlocked.Increment(ref WorkCalls);
        if (Ready) return Flow_Chart.ResultType.NEXT;
        if (PeerCompleted != null && !PeerCompleted()) return Flow_Chart.ResultType.IDLE;
        if (Failure == "Alarm") Alarm.Show("2019", "Offline initialization alarm", true, "E");
        if (Failure == "Exception") throw new InvalidOperationException("Offline initialization exception");
        return Flow_Chart.ResultType.IDLE;
    }

    /// <summary>Count the final step reached only after initialization has been continued.</summary>
    [FlowTool("初始化后段", "After initialization failure", "Después del fallo", UsesHardware = false)]
    private Flow_Chart.ResultType AfterFailure() { Interlocked.Increment(ref LastCalls); return Flow_Chart.ResultType.End; }

    /// <summary>Detect an incorrect automatic Start dispatch while continuing initialization.</summary>
    [FlowTool("自动生产", "Automatic production", "Producción automática", UsesHardware = false)]
    private Flow_Chart.ResultType AutomaticWork() { Interlocked.Increment(ref AutomaticCalls); return Flow_Chart.ResultType.End; }
}

internal static partial class Cases
{
    /// <summary>Exercise the real Initialize click, alarm clearing, status and peer-station completion in an isolated host.</summary>
    private static void VerifyInitializationRecovery(string failure)
    {
        WithMachineEntryState(driver =>
        {
            using (InitializationRecoveryPage page = new InitializationRecoveryPage { Failure = failure })
            using (EntryOnlyPage peer = new EntryOnlyPage { Name = "CompletedPeer" })
            using (MainForm main = new MainForm())
            {
                FlowModuleEditor owner = FlowPrograms.Attach(page), other = FlowPrograms.Attach(peer);
                SetRecoveryEntries(owner, page.Entry.Name, page.Automatic.Name);
                SetRecoveryEntries(other, peer.InitialNode.Name, peer.AutoNode.Name);
                owner.Program.Steps.Single(step => step.ControlName == page.Work.Name).TimeoutMs = 500;
                owner.Publish(); other.Publish();
                Assert(page.Work.Handle != IntPtr.Zero, "The offline recovery node did not create its display handle.");
                page.PeerCompleted = () => peer.InitialStatus;
                try
                {
                    string notice = CaptureRuntimeDialog(() => ClickInitialization(main));
                    Assert(notice == null, "Starting initialization showed an unrelated dialog: " + notice);
                    ActionUntil(() => Alarm.IsError && owner.Runtime.IsPaused && peer.InitialStatus && FlowPrograms.CanContinueInitialization,
                        "Initialization did not retain its failed node and completed peer.");
                    host.GetMethod("PauseRun").Invoke(null, null);
                    string epoch = FlowPrograms.Results.Epoch;
                    int calls = Volatile.Read(ref page.WorkCalls);
                    Thread.Sleep(650);
                    Assert(owner.Runtime.IsRunning && page.EntryCalls == 1 && page.LastCalls == 0 && peer.InitialCalls == 1
                        && Volatile.Read(ref page.WorkCalls) == calls, "An alarm repeated work or ended the initialization session.");
                    Assert((failure == "Alarm") == !owner.Runtime.IsWaitingForRetry, "An alarm pause was confused with a failed-node retry.");
                    VerifyRecoveryButtons(main, owner, page, false);
                    notice = CaptureRuntimeDialog(() => ClickInitialization(main));
                    Assert(notice == FlowText.T("clearAlarmBeforeContinue"), "Initialize did not explain that the active alarm must be cleared.");
                    bool blocked = false;
                    try { FlowPrograms.RequireIdle(); } catch (InvalidOperationException) { blocked = true; }
                    Assert(blocked, "The retained initialization allowed saving or recipe replacement.");
                    host.GetMethod("AlarmClear").Invoke(null, null);
                    VerifyRecoveryButtons(main, owner, page, true);
                    page.Ready = true;
                    notice = CaptureRuntimeDialog(() => ClickInitialization(main));
                    Assert(notice == null, "Continuing through Initialize failed: " + notice);
                    Wait(owner.Runtime.Completion); Wait(other.Runtime.Completion);
                    Assert(owner.Runtime.Error == null && page.InitialStatus && peer.InitialStatus && FlowPrograms.IsInitialized,
                        "The continued initialization did not complete.");
                    Assert(page.EntryCalls == 1 && page.LastCalls == 1 && peer.InitialCalls == 1 && page.AutomaticCalls == 0 && peer.AutoCalls == 0,
                        "Initialize restarted a completed node/station or dispatched automatic production.");
                    Assert(FlowPrograms.Results.Epoch == epoch && !FlowPrograms.HasInitializationSession,
                        "Continuation replaced the result batch or left a live initialization session.");
                    RefreshRecoveryButtons(main);
                    Assert(!main.Controls.Find("MENU_Reset", true)[0].Enabled && main.Controls.Find("MENU_Run", true)[0].Enabled,
                        "Start was not enabled only after the initialization completed.");
                    if (failure == "Exception") VerifyStoppedInitialization(main, page, owner, other);
                    Assert(driver.BusyReads == 0 && driver.HomeCalls == 0 && driver.Stops == 0,
                        "Initialization or continuation queried or operated unrelated hardware.");
                }
                finally { owner.Runtime.Stop(); other.Runtime.Stop(); Wait(owner.Runtime.Completion); Wait(other.Runtime.Completion); }
            }
        });
    }

    /// <summary>Choose native initialization and production entries before publishing the test sequence.</summary>
    private static void SetRecoveryEntries(FlowModuleEditor owner, string initial, string automatic)
    {
        owner.Program.InitialEntry = owner.Program.Steps.Single(step => step.ControlName == initial).Id;
        owner.Program.AutoEntry = owner.Program.Steps.Single(step => step.ControlName == automatic).Id;
    }

    /// <summary>Invoke exactly the existing main Initialize click handler, never the automatic Start handler.</summary>
    private static void ClickInitialization(MainForm main)
    {
        typeof(MainForm).GetMethod("MENU_Reset_Click", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(main, new object[] { null, EventArgs.Empty });
    }

    /// <summary>Refresh the actual main timer's command availability and equipment status.</summary>
    private static void RefreshRecoveryButtons(MainForm main)
    {
        typeof(MainForm).GetMethod("timer1_Tick", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(main, new object[] { null, EventArgs.Empty });
    }

    /// <summary>Keep automatic Start disabled while only Initialize may continue the alarmed initialization.</summary>
    private static void VerifyRecoveryButtons(MainForm main, FlowModuleEditor owner, InitializationRecoveryPage page, bool canContinue)
    {
        RefreshRecoveryButtons(main);
        Assert(main.Controls.Find("MENU_Reset", true)[0].Enabled == canContinue && !main.Controls.Find("MENU_Run", true)[0].Enabled,
            "Initialize/Start availability does not match the alarmed initialization state.");
        if (!canContinue) return;
        Application.DoEvents();
        LanguageType previous = SysPara.LanguageShow;
        try
        {
            string[] captions = { "初始化待继续", "INIT PAUSED", "INICIO EN PAUSA" };
            for (int index = 0; index < captions.Length; index++)
            {
                SwitchLanguage((LanguageType)index); RefreshRecoveryButtons(main);
                Assert(main.Controls.Find("MachineStatus", true)[0].Text.Contains(captions[index]), "The initialization badge did not follow language switching.");
                if (owner.Runtime.IsWaitingForRetry)
                    Assert(page.Work.Controls.Find("label1", true)[0].Text.Contains(FlowText.T("RecoveryPaused")), "The failed node's recovery state did not follow language switching.");
            }
        }
        finally { SwitchLanguage(previous); }
    }

    /// <summary>Stopping the retained session discards the checkpoint and the next Initialize begins at its entry.</summary>
    private static void VerifyStoppedInitialization(MainForm main, InitializationRecoveryPage page, FlowModuleEditor owner, FlowModuleEditor peer)
    {
        SysPara.SystemMode = RunMode.IDLE; page.Ready = false;
        ClickInitialization(main);
        ActionUntil(() => owner.Runtime.IsWaitingForRetry && Alarm.IsError, "The second initialization did not reach its controlled exception.");
        owner.Handle(FlowProgramCommand.Stop); peer.Handle(FlowProgramCommand.Stop);
        Wait(owner.Runtime.Completion); Wait(peer.Runtime.Completion);
        SysPara.SystemMode = RunMode.IDLE;
        host.GetMethod("AlarmClear").Invoke(null, null);
        Assert(!FlowPrograms.HasInitializationSession && !FlowPrograms.CanContinueInitialization, "Stop retained a resumable initialization checkpoint.");
        page.Ready = true; ClickInitialization(main); Wait(owner.Runtime.Completion); Wait(peer.Runtime.Completion);
        Assert(page.EntryCalls == 3 && page.LastCalls == 2 && owner.Runtime.Error == null,
            "Initialization after Stop did not restart from the saved entry.");
    }
}
