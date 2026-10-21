using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using AlphaRap;
using AlphaRapLibrary;
using NPSDK;

/// <summary>记录模块回调，不加载机器参数或访问设备。</summary>
internal sealed class ProbeModule : ModuleBaseForm
{
    public int InitializeCount;
    public int AlwaysCount;
    public int ResetCount;
    public bool FailAlways;
    public bool FailReset;

    /// <summary>记录工厂初始化，不调用设备模块初始化。</summary>
    public override void ModuleInitialize(string name) { InitializeCount++; }

    /// <summary>记录周期回调，可模拟单个模块抛出异常。</summary>
    public override void AlwaysRun()
    {
        AlwaysCount++;
        if (FailAlways) throw new InvalidOperationException("Simulated periodic callback failure.");
    }

    /// <summary>记录运行复位，可模拟必须停止生产的异常。</summary>
    public override void RunReset()
    {
        ResetCount++;
        if (FailReset) throw new InvalidOperationException("Simulated reset failure.");
    }
}

/// <summary>替代实际生产流程，只记录 SDK 调用次数。</summary>
internal sealed class ProbeFlow : Flow_BaseForm
{
    public int InitialCount;
    public int StopCount;
    public int PauseCount;

    /// <summary>记录初始化指令，不启动生产线程。</summary>
    public override void Initial() { InitialCount++; }

    /// <summary>记录停止指令，不访问电机。</summary>
    public override void StopRun() { StopCount++; }

    /// <summary>记录暂停指令，不访问输送线。</summary>
    public override void PauseRun() { PauseCount++; }
}

internal static partial class Tests
{
    /// <summary>隔离模块、流程、设备列表和运行状态，保证离线测试不会触及真实设备。</summary>
    private sealed class MachineFixture : IDisposable
    {
        private readonly List<ModuleBaseForm> _modules = ModuleManager.ModuleList;
        private readonly List<Flow_BaseForm> _flows = Flow_Module.FlowChart_ModuleList;
        private readonly List<ControlBaseInterface> _controls = SDKPara.ControlList;
        private readonly Dictionary<FieldInfo, object> _fields = new Dictionary<FieldInfo, object>();
        private readonly RunMode _mode = SysPara.SystemMode;
        private readonly bool _running = SysPara.SystemRun;
        private readonly bool _initialized = SysPara.UpConveyorInitialOk;
        private readonly bool _allStop = Flow_Module.AllStop;
        private readonly bool _allPause = Flow_Module.AllPause;
        public readonly ProbeModule Module;
        public readonly ProbeFlow Flow;

        /// <summary>建立只包含回调替身的运行环境，所有硬件列表置空。</summary>
        public MachineFixture()
        {
            foreach (string name in new[] { "VPF", "ManualF", "DataF", "MainF" })
            {
                FieldInfo field = LanguageHost.GetField(name);
                _fields.Add(field, field.GetValue(null));
                field.SetValue(null, null);
            }
            ModuleManager.ModuleList = new List<ModuleBaseForm>();
            Flow_Module.FlowChart_ModuleList = new List<Flow_BaseForm>();
            SDKPara.ControlList = new List<ControlBaseInterface>();
            Module = new ProbeModule();
            Flow = new ProbeFlow();
            if (!ModuleManager.ModuleList.Contains(Module)) ModuleManager.ModuleList.Add(Module);
            if (!Flow_Module.FlowChart_ModuleList.Contains(Flow)) Flow_Module.FlowChart_ModuleList.Add(Flow);
            SysPara.SystemMode = RunMode.IDLE;
            SysPara.SystemRun = false;
            SysPara.UpConveyorInitialOk = false;
            Alarm.Clear();
        }

        /// <summary>恢复测试前的全局引用与运行状态，释放不含设备的替身窗体。</summary>
        public void Dispose()
        {
            ModuleManager.ModuleList = _modules;
            Flow_Module.FlowChart_ModuleList = _flows;
            SDKPara.ControlList = _controls;
            foreach (KeyValuePair<FieldInfo, object> field in _fields) field.Key.SetValue(null, field.Value);
            SysPara.SystemMode = _mode;
            SysPara.SystemRun = _running;
            SysPara.UpConveyorInitialOk = _initialized;
            Flow_Module.AllStop = _allStop;
            Flow_Module.AllPause = _allPause;
            Alarm.Clear();
            Module.Dispose(); Flow.Dispose();
        }
    }

    /// <summary>重复创建调用复用同一实例，初始化和窗体登记各执行一次。</summary>
    private static void FormFactoryReuse()
    {
        FieldInfo forms = LanguageHost.GetField("lstForm");
        object previous = forms.GetValue(null);
        List<ModuleBaseForm> previousModules = ModuleManager.ModuleList;
        ModuleManager.ModuleList = new List<ModuleBaseForm>();
        forms.SetValue(null, Activator.CreateInstance(forms.FieldType));
        try
        {
            using (ProbeModule module = new ProbeModule())
            {
                MethodInfo create = LanguageHost.GetMethod("CreateForm");
                object first = create.Invoke(null, new object[] { module, "VPForm" });
                object second = create.Invoke(null, new object[] { module, "VPForm" });
                Check(ReferenceEquals(first, module) && ReferenceEquals(second, module), "Factory did not reuse the supplied instance.");
                Check(module.InitializeCount == 1, "Factory initialized the same module twice.");
                Check(((System.Collections.IList)forms.GetValue(null)).Count == 1, "Factory registered the same instance twice.");
                Check(create.Invoke(null, new object[] { null, "UnknownForm" }) == null, "Unknown form lookup changed compatibility behavior.");
            }
        }
        finally { forms.SetValue(null, previous); ModuleManager.ModuleList = previousModules; }
    }

    /// <summary>初始化从空闲重新进入初始化，清除旧运行状态且只通知流程一次。</summary>
    private static void MachineInitialTransition()
    {
        using (MachineFixture machine = new MachineFixture())
        {
            SysPara.SystemRun = true;
            SysPara.UpConveyorInitialOk = true;
            LanguageHost.GetMethod("Initial").Invoke(null, null);
            Check(SysPara.SystemMode == RunMode.INITIAL && !SysPara.SystemRun && !SysPara.UpConveyorInitialOk, "Initialization retained stale running state.");
            Check(machine.Flow.InitialCount == 1 && machine.Flow.StopCount == 1, "Initialization repeated a flow command.");
        }
    }

    /// <summary>停止流程只下发一次停止，并取消手动任务和清理运行标志。</summary>
    private static void MachineStopTransition()
    {
        using (MachineFixture machine = new MachineFixture())
        {
            SysPara.SystemMode = RunMode.RUN;
            SysPara.SystemRun = true;
            SysPara.UpConveyorInitialOk = true;
            LanguageHost.GetMethod("StopRun").Invoke(null, null);
            Check(SysPara.SystemMode == RunMode.IDLE && !SysPara.SystemRun && !SysPara.UpConveyorInitialOk, "Stop did not reset the run state.");
            Check(machine.Flow.StopCount == 1 && machine.Module.StopManualTask.IsCancellationRequested, "Stop repeated a flow command or missed manual cancellation.");
        }
    }

    /// <summary>暂停保留自动运行标记以便恢复，同时取消手动任务。</summary>
    private static void MachinePauseTransition()
    {
        using (MachineFixture machine = new MachineFixture())
        {
            SysPara.SystemMode = RunMode.RUN;
            SysPara.SystemRun = true;
            SysPara.UpConveyorInitialOk = true;
            LanguageHost.GetMethod("PauseRun").Invoke(null, null);
            Check(SysPara.SystemMode == RunMode.PAUSE && SysPara.SystemRun && SysPara.UpConveyorInitialOk, "Pause lost the state needed for resume.");
            Check(machine.Flow.PauseCount == 1 && machine.Module.StopManualTask.IsCancellationRequested, "Pause missed a flow or manual task.");
        }
    }

    /// <summary>周期任务失败仍继续下一模块，保持原有周期扫描的异常隔离策略。</summary>
    private static void ModulePeriodicIsolation()
    {
        using (MachineFixture machine = new MachineFixture())
        using (ProbeModule next = new ProbeModule())
        {
            machine.Module.FailAlways = true;
            if (!ModuleManager.ModuleList.Contains(next)) ModuleManager.ModuleList.Add(next);
            LanguageHost.GetMethod("AlwaysRun").Invoke(null, null);
            Check(machine.Module.AlwaysCount == 1 && next.AlwaysCount == 1, "A failed periodic callback blocked the next module.");
            Check(machine.Flow.StopCount == 0, "Periodic callback failure changed the existing stop policy.");
        }
    }

    /// <summary>运行复位异常必须停止生产，并继续隔离其余模块的复位操作。</summary>
    private static void ModuleResetFailureStops()
    {
        using (MachineFixture machine = new MachineFixture())
        using (ProbeModule next = new ProbeModule())
        {
            SysPara.SystemMode = RunMode.RUN;
            SysPara.SystemRun = true;
            machine.Module.FailReset = true;
            if (!ModuleManager.ModuleList.Contains(next)) ModuleManager.ModuleList.Add(next);
            LanguageHost.GetMethod("RunReset").Invoke(null, null);
            Check(machine.Module.ResetCount == 1 && next.ResetCount == 1, "A failed reset blocked other module callbacks.");
            Check(machine.Flow.StopCount == 1 && SysPara.SystemMode == RunMode.IDLE && !SysPara.SystemRun, "Reset failure did not stop production exactly once.");
        }
    }

    /// <summary>扫描退出信号跨线程可见，停止会等待退出且不会让线程等待自身。</summary>
    private static void ScanThreadShutdown()
    {
        FlowControl scan = new FlowControl();
        FieldInfo workerField = typeof(FlowControl).GetField("FlowControlThread", BindingFlags.Instance | BindingFlags.NonPublic);
        using (ManualResetEvent entered = new ManualResetEvent(false))
        {
            Thread worker = new Thread(delegate()
            {
                entered.Set();
                while (!scan.bStopWork) Thread.Yield();
            });
            worker.IsBackground = true;
            workerField.SetValue(scan, worker);
            worker.Start();
            Check(entered.WaitOne(1000), "Controlled scan did not start.");
            scan.StopThread();
            Check(scan.bStopWork && !worker.IsAlive, "Scan shutdown returned before its worker exited.");
            scan.StopThread();
        }
        scan.bStopWork = false;
        Thread self = new Thread(scan.StopThread);
        self.IsBackground = true;
        workerField.SetValue(scan, self);
        self.Start();
        Check(self.Join(1000) && scan.bStopWork, "Scan worker attempted to join itself.");
    }

    /// <summary>内存索引与三语 XML 的所有文案一致，记录与原 XPath 查找的本机耗时对比。</summary>
    private static void LanguageIndexLookup()
    {
        WithLanguageFiles(delegate
        {
            Func<string, string, string, string> lookup = (Func<string, string, string, string>)Delegate.CreateDelegate(
                typeof(Func<string, string, string, string>), LanguageHost.GetMethod("LangText"));
            foreach (LanguageType language in Enum.GetValues(typeof(LanguageType)))
            {
                SysPara.LanguageShow = language;
                XElement document = XElement.Load(Path.Combine(SysPara.LanguageDataDirectory, language + ".xml"));
                foreach (XElement form in document.Elements())
                    foreach (XElement entry in form.Elements())
                        Check(lookup(form.Name.LocalName, entry.Name.LocalName, "missing") == (string)entry.Attribute("ComponentText"), "Indexed translation differs from XML.");
            }
            SysPara.LanguageShow = LanguageType.English;
            XmlDocument legacy = new XmlDocument();
            legacy.Load(Path.Combine(SysPara.LanguageDataDirectory, "English.xml"));
            object sync = LanguageHost.GetField("LanguageSync", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            Func<string> xpath = delegate
            {
                lock (sync)
                    return ((XmlElement)legacy.SelectSingleNode("/" + SysPara.LanguageShow + "/VPForm/vpCompCol_Item")).GetAttribute("ComponentText");
            };
            for (int i = 0; i < 2000; i++) { xpath(); lookup("VPForm", "vpCompCol_Item", "missing"); }
            Stopwatch elapsed = Stopwatch.StartNew();
            for (int i = 0; i < 100000; i++) xpath();
            double oldMs = elapsed.Elapsed.TotalMilliseconds;
            elapsed.Restart();
            for (int i = 0; i < 100000; i++) lookup("VPForm", "vpCompCol_Item", "missing");
            Console.WriteLine("PERF 100000 language lookups: XPath=" + oldMs.ToString("F1") + " ms; index=" + elapsed.Elapsed.TotalMilliseconds.ToString("F1") + " ms");
        });
    }
}
