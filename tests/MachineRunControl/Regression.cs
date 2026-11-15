using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlphaRap;
using AlphaRapLibrary;
using NPSDK;
using NPSDK.Motion;

internal static class MachineRunProbe
{
    private static Type host;
    private static int passed;
    private const int TimeoutMs = 7000;

    private sealed class DesignerContext : LicenseContext
    {
        public override LicenseUsageMode UsageMode { get { return LicenseUsageMode.Designtime; } }
    }

    private sealed class Profile : MachineApplication
    {
        internal volatile bool Initialized;
        internal Action PauseAction;
        internal int Warnings, Resumes;
        internal string LastWarning;
        public override bool IsLegacyInitialized { get { return Initialized; } }
        /// <summary>模拟应用的初始化复位，不访问 IO。</summary>
        public override void ResetLegacyInitialization() { Initialized = false; }
        /// <summary>记录控制错误而不弹出现场窗口。</summary>
        public override void ShowWarning(string message, string confirm) { LastWarning = message; Interlocked.Increment(ref Warnings); }
        /// <summary>允许测试阻塞旧输出暂停，验证停止期间不会继续恢复。</summary>
        public override void PauseLegacyOutputs() { if (PauseAction != null) PauseAction(); }
        /// <summary>记录旧输出恢复次数。</summary>
        public override void ResumeLegacyOutputs() { Interlocked.Increment(ref Resumes); }
    }

    private sealed class Controller : IFlowProgramController, IFlowProgramStatus
    {
        internal volatile bool Active;
        internal int Starts, Initials, Stops;
        internal Action InitializeAction, StartAction, StopAction;
        internal Flow_BaseForm Form;
        public bool IsEnabled { get { return true; } }
        public bool IsRunning { get { return Active; } }
        /// <summary>模拟工位实际生命周期，保留真实命令派发入口。</summary>
        public bool Handle(FlowProgramCommand command)
        {
            if (command == FlowProgramCommand.Initialize)
            {
                Interlocked.Increment(ref Initials);
                if (InitializeAction != null) InitializeAction();
                return true;
            }
            if (command == FlowProgramCommand.Start)
            {
                Interlocked.Increment(ref Starts);
                if (StartAction != null) StartAction(); else Active = true;
                return true;
            }
            if (command == FlowProgramCommand.Stop)
            {
                Interlocked.Increment(ref Stops);
                if (StopAction != null) StopAction(); else Active = false;
            }
            return true;
        }
    }

    private sealed class PreparationDriver : MotorBaseInterface
    {
        internal ManualResetEventSlim Entered, Release;
        /// <summary>模拟启动前检查驱动状态耗时，不读取硬件。</summary>
        public override AxisIOState GetAxisIOState() { Entered.Set(); Release.Wait(); return new AxisIOState { SVON = true }; }
    }

    private sealed class Fixture : IDisposable
    {
        internal readonly Profile Application = new Profile();
        internal readonly List<Controller> Controllers = new List<Controller>();
        internal readonly List<ManualResetEventSlim> Releases = new List<ManualResetEventSlim>();
        internal readonly List<ModuleBaseForm> ManualForms = new List<ModuleBaseForm>();
        private readonly object previous;
        /// <summary>安装不包含真实设备的应用配置。</summary>
        internal Fixture()
        {
            FieldInfo field = host.GetField("Application", BindingFlags.Static | BindingFlags.Public);
            previous = field.GetValue(null); field.SetValue(null, Application);
            SysPara.SystemMode = RunMode.IDLE; SysPara.SystemRun = false; Alarm.IsError = false;
        }
        /// <summary>创建可阻塞的离线工位。</summary>
        internal Controller Station()
        {
            Controller controller = new Controller();
            controller.Form = new Flow_BaseForm { Name = "Offline" + Controllers.Count, ProgramController = controller };
            Controllers.Add(controller); return controller;
        }
        /// <summary>登记保证清理时释放的等待句柄。</summary>
        internal ManualResetEventSlim Release()
        {
            ManualResetEventSlim release = new ManualResetEventSlim(); Releases.Add(release); return release;
        }
        /// <summary>释放所有模拟阻塞，等待真实清理结束，再恢复应用入口。</summary>
        public void Dispose()
        {
            foreach (ManualResetEventSlim release in Releases) release.Set();
            Application.PauseAction = null;
            foreach (Controller controller in Controllers) { controller.Active = false; controller.StopAction = null; }
            Invoke("StopRun"); Wait(Property<Task<bool>>("MachineStopCompletion"));
            Wait(Property<Task<bool>>("MachineCommandCompletion"));
            if (Property<string>("MachineStopError") != null) { Invoke("StopRun"); Wait(Property<Task<bool>>("MachineStopCompletion")); }
            foreach (Controller controller in Controllers) { Flow_Module.FlowChart_ModuleList.Remove(controller.Form); controller.Form.Dispose(); }
            foreach (ModuleBaseForm form in ManualForms) { ModuleManager.ModuleList.Remove(form); form.Dispose(); }
            foreach (ManualResetEventSlim release in Releases) release.Dispose();
            host.GetField("Application", BindingFlags.Static | BindingFlags.Public).SetValue(null, previous);
            SysPara.SystemMode = RunMode.IDLE; SysPara.SystemRun = false;
        }
    }

    /// <summary>验证整机命令、取消清理和重启条件；不初始化任何硬件。</summary>
    [STAThread]
    private static int Main(string[] args)
    {
        string directory = System.IO.Path.GetFullPath(args[0]);
        Environment.CurrentDirectory = System.IO.Path.GetFullPath(args[1]);
        AppDomain.CurrentDomain.AssemblyResolve += (sender, request) =>
        {
            string name = new AssemblyName(request.Name).Name;
            foreach (string root in new[] { directory, @"C:\Program Files\Cognex\VisionPro\ReferencedAssemblies" })
                foreach (string extension in new[] { ".dll", ".exe" })
                    if (System.IO.File.Exists(System.IO.Path.Combine(root, name + extension))) return Assembly.LoadFrom(System.IO.Path.Combine(root, name + extension));
            return null;
        };
        return Run();
    }

    /// <summary>程序集解析入口就绪后再载入框架和应用类型。</summary>
    private static int Run()
    {
        LicenseManager.CurrentContext = new DesignerContext(); UiTheme.ForceDesignTimeForTest(true);
        Application.EnableVisualStyles();
        host = typeof(MainForm).Assembly.GetType("AlphaRap.MiddleLayer", true);
        SysPara.LanguageDataDirectory = System.IO.Path.Combine(Environment.CurrentDirectory, "LanguageData");
        Alarm.Initial(System.IO.Path.Combine(Environment.CurrentDirectory, "offline-alarm-table.xml"));
        try
        {
            Test("independent stop requests retain occupancy beyond the waiting threshold", CoordinatorWait);
            Test("stop failures remain visible and an explicit retry can succeed", CoordinatorFailure);
            Test("a full stop extends in-flight preparation cleanup and preserves equal display names", CoordinatorEscalation);
            Test("host stop returns promptly and reaches other stations despite a blocked callback", HostBlockedStop);
            Test("stop withdraws an initialization waiting for previous cleanup", QueuedInitialization);
            Test("late initialization cannot restore ready state after stop", InFlightInitialization);
            Test("partial start failure does not publish running success", StartFailure);
            Test("a priority pause during startup cancels the dispatch instead of being dropped", PriorityPauseDuringStartup);
            Test("stop during a blocked pause prevents subsequent output restoration", PauseWithdrawal);
            Test("manual cancellation retains ownership after the action itself has returned", ManualCancellation);
            Test("host stop failures prevent restart until explicit successful retry", HostStopFailure);
            Test("host stop retains detached device IO and rejects startup until it exits", DetachedDeviceStop);
            Test("startup preparation rejects new device IO and releases admission afterwards", StartupDeviceAdmission);
            Console.WriteLine("RESULT " + passed + "/13 passed; no motion, PLC, camera or machine startup was executed.");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
        finally { UiTheme.ForceDesignTimeForTest(false); }
    }

    /// <summary>自动启动前驱动检查期间不插入手动 IO，停止后允许正常调试。</summary>
    private static void StartupDeviceAdmission()
    {
        using (Fixture fixture = new Fixture())
        using (ManualResetEventSlim entered = new ManualResetEventSlim())
        using (NP_Motor motor = new NP_Motor())
        {
            Controller station = fixture.Station();
            ManualResetEventSlim release = fixture.Release();
            var driver = new PreparationDriver { Entered = entered, Release = release };
            typeof(NP_Motor).GetField("dMotor", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(motor, driver);
            typeof(NP_Motor).GetField("ready", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(motor, true);
            if (!SDKPara.ControlList.Contains(motor)) SDKPara.ControlList.Add(motor);
            try
            {
                fixture.Application.Initialized = true; SysPara.SystemMode = RunMode.INITIAL;
                Task<bool> startup = (Task<bool>)Invoke("StartRunAsync"); Until(() => entered.IsSet);
                int calls = 0;
                Task rejected = DeviceOperations.RunAsync(null, () => Interlocked.Increment(ref calls));
                try { Wait(rejected); } catch (OperationCanceledException) { }
                Check(rejected.IsCanceled && calls == 0 && !DeviceOperations.HasPending, "Startup admitted new device IO.");
                release.Set(); Wait(startup); Check(startup.Result, "Offline startup failed: mode=" + SysPara.SystemMode + ", warnings=" + fixture.Application.LastWarning + ", starts=" + station.Starts + ", stop=" + Property<string>("MachineStopError"));
                Invoke("StopRun"); Wait(Property<Task<bool>>("MachineStopCompletion"));
                Wait(DeviceOperations.RunAsync(null, () => Interlocked.Increment(ref calls)));
                Check(calls == 1 && !DeviceOperations.HasPending, "Completed startup retained the device admission block.");
            }
            finally { release.Set(); SDKPara.ControlList.Remove(motor); motor.DisposeObj(); }
        }
    }

    /// <summary>超过等待提示阈值不释放占用，且后续清理也被纳入完成条件。</summary>
    private static void CoordinatorWait()
    {
        MachineStopCoordinator coordinator = new MachineStopCoordinator(30);
        bool busy = true; int peer = 0;
        using (ManualResetEventSlim release = new ManualResetEventSlim())
        using (ManualResetEventSlim entered = new ManualResetEventSlim())
        {
            Task<bool> stop = coordinator.RequestStop(new[] {
                new MachineStopStep("Blocked", () => { entered.Set(); release.Wait(); }, () => Volatile.Read(ref busy)),
                new MachineStopStep("Peer", () => Interlocked.Increment(ref peer))
            });
            try
            {
                Until(() => entered.IsSet && Volatile.Read(ref peer) == 1 && coordinator.IsWaiting);
                Check(ReferenceEquals(stop, coordinator.RequestStop(new MachineStopStep[0])), "Repeated stop created another session.");
                Check(!stop.IsCompleted && coordinator.PendingOperations.Contains("Blocked"), "Waiting released an occupied operation.");
                release.Set();
                Check(!stop.Wait(60), "A returned callback hid its still-active task.");
            }
            finally { busy = false; release.Set(); Wait(stop); }
            Check(stop.Result && !coordinator.IsStopping, "Completed cleanup stayed stopping.");
        }
    }

    /// <summary>独立步骤错误和最终复位错误都被保留，重试可清除旧停止错误。</summary>
    private static void CoordinatorFailure()
    {
        MachineStopCoordinator coordinator = new MachineStopCoordinator();
        int peer = 0;
        Task<bool> first = coordinator.RequestStop(new[] {
            new MachineStopStep("Broken", () => { throw new InvalidOperationException("Offline stop fault"); }),
            new MachineStopStep("Peer", () => peer++)
        }, () => { throw new InvalidOperationException("Offline final reset fault"); });
        Wait(first);
        Check(!first.Result && peer == 1 && coordinator.Error.Contains("Offline stop fault") && coordinator.Error.Contains("Offline final reset fault"), "Stop errors were lost.");
        Task<bool> retry = coordinator.RequestStop(new[] { new MachineStopStep("Retry", () => { }) });
        Wait(retry); Check(retry.Result && coordinator.Error == null, "An explicit stop retry retained an old error.");
    }

    /// <summary>一个工位停止回调阻塞时，主调用返回且其他工位仍收到停止。</summary>
    private static void HostBlockedStop()
    {
        using (Fixture fixture = new Fixture())
        using (ManualResetEventSlim entered = new ManualResetEventSlim())
        {
            Controller held = fixture.Station(), peer = fixture.Station();
            ManualResetEventSlim release = fixture.Release();
            held.Active = peer.Active = true;
            held.StopAction = () => { entered.Set(); release.Wait(); held.Active = false; };
            fixture.Application.Initialized = true; SysPara.SystemMode = RunMode.RUN; SysPara.SystemRun = true;
            Stopwatch clock = Stopwatch.StartNew(); Invoke("StopRun");
            Check(clock.ElapsedMilliseconds < 500 && SysPara.SystemMode == RunMode.STOPPING && !SysPara.SystemRun, "Host stop blocked or declared idle early.");
            Until(() => entered.IsSet && Volatile.Read(ref peer.Stops) == 1);
            Task<bool> stopped = Property<Task<bool>>("MachineStopCompletion");
            Check(!stopped.IsCompleted && !((Task<bool>)Invoke("StartRunAsync")).Result && !((Task<bool>)Invoke("InitialAsync")).Result, "An occupied stop accepted a new command.");
            release.Set(); Wait(stopped);
            Check(stopped.Result && SysPara.SystemMode == RunMode.IDLE && !fixture.Application.Initialized, "Real stop completion did not publish idle/reset.");
        }
    }

    /// <summary>完整停止可以追加硬件步骤与最终复位，同名的独立资源也都收到请求。</summary>
    private static void CoordinatorEscalation()
    {
        MachineStopCoordinator coordinator = new MachineStopCoordinator();
        int hardware = 0, reset = 0;
        using (ManualResetEventSlim entered = new ManualResetEventSlim())
        using (ManualResetEventSlim release = new ManualResetEventSlim())
        {
            Task<bool> preparation = coordinator.RequestStop(new[] { new MachineStopStep("Flow", () => { entered.Set(); release.Wait(); }) });
            Until(() => entered.IsSet);
            Task<bool> complete = coordinator.RequestStop(new[] {
                new MachineStopStep("Flow", () => { throw new InvalidOperationException("Duplicate request"); }),
                new MachineStopStep("Hardware", () => Interlocked.Increment(ref hardware), identity: new object()),
                new MachineStopStep("Hardware", () => Interlocked.Increment(ref hardware), identity: new object())
            }, () => Interlocked.Increment(ref reset));
            try { Until(() => Volatile.Read(ref hardware) == 2); Check(ReferenceEquals(preparation, complete) && !complete.IsCompleted, "Stop escalation replaced or prematurely completed its session."); }
            finally { release.Set(); Wait(complete); }
            Check(complete.Result && reset == 1, "Full-stop escalation lost hardware work or final reset.");
        }
    }

    /// <summary>真实整机入口保留停止失败状态；显式重试成功后才恢复待机。</summary>
    private static void HostStopFailure()
    {
        using (Fixture fixture = new Fixture())
        {
            Controller station = fixture.Station(); station.Active = true;
            station.StopAction = () => { station.Active = false; throw new InvalidOperationException("Offline host stop fault"); };
            SysPara.SystemMode = RunMode.RUN; SysPara.SystemRun = true;
            Invoke("StopRun"); Task<bool> first = Property<Task<bool>>("MachineStopCompletion"); Wait(first);
            Check(!first.Result && SysPara.SystemMode == RunMode.STOPPING && Property<string>("MachineStopError").Contains("Offline host stop fault"), "Host stop failure was shown as idle.");
            Check(!((Task<bool>)Invoke("InitialAsync")).Result && !((Task<bool>)Invoke("StartRunAsync")).Result, "Failed stop accepted restart.");
            station.StopAction = null; Invoke("StopRun"); Task<bool> retry = Property<Task<bool>>("MachineStopCompletion"); Wait(retry);
            Check(retry.Result && Property<string>("MachineStopError") == null && SysPara.SystemMode == RunMode.IDLE, "Host stop retry did not clear the failure.");
        }
    }

    /// <summary>初始化等待旧任务清理时收到停止，不会在清理结束后自行启动。</summary>
    private static void QueuedInitialization()
    {
        using (Fixture fixture = new Fixture())
        using (ManualResetEventSlim entered = new ManualResetEventSlim())
        {
            Controller station = fixture.Station(); ManualResetEventSlim release = fixture.Release();
            station.StopAction = () => { entered.Set(); release.Wait(); station.Active = false; };
            Task<bool> initialization = (Task<bool>)Invoke("InitialAsync");
            Until(() => entered.IsSet);
            Invoke("StopRun"); release.Set(); Wait(initialization); Wait(Property<Task<bool>>("MachineStopCompletion"));
            Check(!initialization.Result && station.Initials == 0 && SysPara.SystemMode == RunMode.IDLE, "Stopped queued initialization restarted later.");
            station.StopAction = null;
            station.InitializeAction = () => { fixture.Application.Initialized = true; station.Form.InitialStatus = true; };
            Task<bool> fresh = (Task<bool>)Invoke("InitialAsync"); Wait(fresh);
            Check(fresh.Result && station.Initials == 1 && SysPara.SystemMode == RunMode.INITIAL, "A fresh initialization was not accepted.");
        }
    }

    /// <summary>初始化回调迟到写入就绪标记后，停止最终复位仍清除该旧标记。</summary>
    private static void InFlightInitialization()
    {
        using (Fixture fixture = new Fixture())
        using (ManualResetEventSlim entered = new ManualResetEventSlim())
        {
            Controller station = fixture.Station(); ManualResetEventSlim release = fixture.Release();
            station.InitializeAction = () => { entered.Set(); release.Wait(); fixture.Application.Initialized = true; station.Form.InitialStatus = true; };
            Task<bool> initialization = (Task<bool>)Invoke("InitialAsync"); Until(() => entered.IsSet);
            Invoke("StopRun"); Task<bool> stop = Property<Task<bool>>("MachineStopCompletion");
            Check(!stop.Wait(60), "In-flight initialization was ignored by stop.");
            release.Set(); Wait(stop); Wait(initialization);
            Check(!initialization.Result && !fixture.Application.Initialized && !station.Form.InitialStatus && SysPara.SystemMode == RunMode.IDLE,
                "Late initialization restored ready state after stop.");
        }
    }

    /// <summary>启动回调失败走统一停止，状态不会提前发布为运行成功。</summary>
    private static void StartFailure()
    {
        using (Fixture fixture = new Fixture())
        {
            Controller station = fixture.Station(); fixture.Application.Initialized = true;
            station.StartAction = () => { throw new InvalidOperationException("Offline start failure"); };
            SysPara.SystemMode = RunMode.INITIAL;
            Task<bool> start = (Task<bool>)Invoke("StartRunAsync"); Wait(start); Wait(Property<Task<bool>>("MachineStopCompletion"));
            Check(!start.Result && !SysPara.SystemRun && SysPara.SystemMode == RunMode.IDLE && fixture.Application.Warnings == 1 && station.Starts == 1
                && fixture.Application.LastWarning.Contains("Offline start failure"),
                "A failed startup appeared to succeed or escaped observation.");
        }
    }

    /// <summary>暂停输出处理仍在阻塞时停止，旧暂停命令不能继续恢复输出。</summary>
    private static void PauseWithdrawal()
    {
        using (Fixture fixture = new Fixture())
        using (ManualResetEventSlim entered = new ManualResetEventSlim())
        {
            Controller station = fixture.Station(); station.Active = true;
            ManualResetEventSlim release = fixture.Release();
            fixture.Application.Initialized = true; fixture.Application.PauseAction = () => { entered.Set(); release.Wait(); };
            SysPara.SystemMode = RunMode.RUN; SysPara.SystemRun = true;
            Invoke("PauseRun"); Task<bool> pause = Property<Task<bool>>("MachineCommandCompletion"); Until(() => entered.IsSet);
            Invoke("StopRun"); Task<bool> stop = Property<Task<bool>>("MachineStopCompletion");
            Check(!stop.Wait(60) && SysPara.SystemMode == RunMode.STOPPING, "Stop ignored in-flight pause work.");
            release.Set(); Wait(stop); Wait(pause);
            Check(!pause.Result && fixture.Application.Resumes == 0 && SysPara.SystemMode == RunMode.IDLE, "Old pause work survived stop.");
        }
    }

    /// <summary>启动回调中的报警暂停不会被繁忙命令吞掉，旧启动不能再发布运行成功。</summary>
    private static void PriorityPauseDuringStartup()
    {
        using (Fixture fixture = new Fixture())
        {
            Controller station = fixture.Station(); fixture.Application.Initialized = true;
            station.StartAction = () => { station.Active = true; Invoke("PauseRun"); };
            SysPara.SystemMode = RunMode.INITIAL;
            Task<bool> start = (Task<bool>)Invoke("StartRunAsync"); Wait(start); Wait(Property<Task<bool>>("MachineStopCompletion"));
            Check(!start.Result && !SysPara.SystemRun && !station.Active && SysPara.SystemMode == RunMode.IDLE && station.Stops >= 1 && station.Starts == 1,
                "Priority pause was ignored during startup.");
        }
    }

    /// <summary>手动任务已经返回但取消回调未退出时，重复执行仍被拒绝。</summary>
    private static void ManualCancellation()
    {
        using (Fixture fixture = new Fixture())
        using (ManualResetEventSlim entered = new ManualResetEventSlim())
        using (ManualResetEventSlim cancelling = new ManualResetEventSlim())
        {
            ModuleBaseForm module = new ModuleBaseForm { Name = "OfflineManual" }; fixture.ManualForms.Add(module);
            ManualResetEventSlim releaseAction = fixture.Release(), releaseCancel = fixture.Release();
            int repeats = 0;
            module.ExecuteManual(() => { module.StopManualTask.Token.Register(() => { cancelling.Set(); releaseCancel.Wait(); }); entered.Set(); releaseAction.Wait(); });
            Until(() => entered.IsSet); Task stop = module.RequestManualStop(); Until(() => cancelling.IsSet);
            releaseAction.Set();
            module.ExecuteManual(() => Interlocked.Increment(ref repeats));
            Check(module.IsManualRun() && repeats == 0 && !stop.IsCompleted, "Manual cleanup released its ownership early.");
            releaseCancel.Set(); Wait(stop); Until(() => !module.IsManualRun());
            module.ExecuteManual(() => Interlocked.Increment(ref repeats)); Until(() => Volatile.Read(ref repeats) == 1 && !module.IsManualRun());
        }
    }

    /// <summary>设备面板已经不存在时，整机停止仍等实际设备工作退出且不允许启动。</summary>
    private static void DetachedDeviceStop()
    {
        using (Fixture fixture = new Fixture())
        using (ManualResetEventSlim entered = new ManualResetEventSlim())
        {
            ManualResetEventSlim release = fixture.Release();
            Task work = DeviceOperations.RunAsync(null, () => { entered.Set(); release.Wait(); });
            Until(() => entered.IsSet); Invoke("StopRun");
            Task<bool> stopped = Property<Task<bool>>("MachineStopCompletion");
            Check(DeviceOperations.HasPending && !stopped.Wait(40) && SysPara.SystemMode == RunMode.STOPPING, "Detached IO was ignored by machine stop.");
            Task<bool> start = (Task<bool>)Invoke("StartRunAsync"); Wait(start);
            Check(!start.Result, "Startup accepted while old device IO was still occupied.");
            release.Set(); Wait(stopped); Until(() => work.IsCompleted);
            try { work.GetAwaiter().GetResult(); } catch (OperationCanceledException) { }
            Check(stopped.Result && !DeviceOperations.HasPending && SysPara.SystemMode == RunMode.IDLE, "Device stop did not finish after actual IO exit.");
        }
    }

    /// <summary>调用真实应用控制入口，不加载主窗体或硬件。</summary>
    private static object Invoke(string method) { return host.GetMethod(method, BindingFlags.Static | BindingFlags.Public).Invoke(null, null); }
    /// <summary>读取真实命令状态快照。</summary>
    private static T Property<T>(string name) { return (T)host.GetProperty(name, BindingFlags.Static | BindingFlags.Public).GetValue(null, null); }
    /// <summary>派发 UI 消息并等待一个有界的离线条件。</summary>
    private static void Until(Func<bool> complete)
    {
        Stopwatch watch = Stopwatch.StartNew();
        while (!complete() && watch.ElapsedMilliseconds < TimeoutMs) { Application.DoEvents(); Thread.Sleep(2); }
        Check(complete(), "Timed out waiting for an offline control operation.");
    }
    /// <summary>等待真实任务完成并观察异常。</summary>
    private static void Wait(Task pending) { Until(() => pending.IsCompleted); pending.GetAwaiter().GetResult(); }
    /// <summary>输出独立回归结果。</summary>
    private static void Test(string name, Action action) { action(); passed++; Console.WriteLine("PASS " + name); }
    /// <summary>要求控制状态或资源占用满足预期。</summary>
    private static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}
