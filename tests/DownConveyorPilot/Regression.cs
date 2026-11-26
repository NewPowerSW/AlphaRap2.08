using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AlphaRap;
using AlphaRap.Configuration;
using AlphaRap.TaskProcess;
using AlphaRapLibrary;
using NPSDK;
using NPSDK.Motion;
using Result = NPSDK.Flow_Chart.ResultType;

internal static class Regression
{
    private static readonly BindingFlags Instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private static readonly BindingFlags Static = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    private static readonly string[] SafetyInputs =
    {
        "IB_Front_EmergencyStop", "IB_Back_EmergencyStop", "IB_Air", "IB_SafetyReady"
    };
    private const string WatchedInput = "IB_OfflineMaterialLow";
    private static int checks;

    /// <summary>模拟 SDK 驱动，在内存中保存点位状态。</summary>
    private sealed class OfflineIo : IOBaseInterface
    {
        internal bool State;
        internal int Reads;
        internal bool FailOff;
        internal bool InputReadFailed;

        /// <summary>返回模拟输入或输出状态。</summary>
        public override bool GetState() { Reads++; return State; }

        /// <summary>模拟驱动仍已初始化但本次读点通信失败。</summary>
        public override bool? ReadInputState() { return InputReadFailed ? (bool?)null : GetState(); }

        /// <summary>打开模拟输出。</summary>
        public override bool On() { State = true; return true; }

        /// <summary>关闭模拟输出，并支持验证单点故障。</summary>
        public override bool Off()
        {
            if (FailOff) throw new IOException("Offline output did not turn off.");
            State = false;
            return true;
        }
    }

    private sealed class OfflineMonitoredInput : NP_Input
    {
        internal int Initializations;

        /// <summary>记录驱动准备请求，离线测试不访问真实设备。</summary>
        public override void Initial() { Initializations++; }
    }

    /// <summary>把离线驱动装到机器配置生成的真实 IO 控件上。</summary>
    private sealed class Simulator
    {
        internal readonly Dictionary<string, OfflineIo> Points = new Dictionary<string, OfflineIo>();
        private readonly Dictionary<string, NP_Input> inputs = new Dictionary<string, NP_Input>();

        /// <summary>替换驱动实例，保持工站和页面共用同一批点位。</summary>
        internal Simulator(HardwareResources resources)
        {
            foreach (HardwareEntry entry in resources.Entries.Where(item => !item.Definition.ConnectionPending))
            {
                var driver = new OfflineIo();
                string key = string.IsNullOrEmpty(entry.Definition.StationKey) ? entry.Definition.Name : entry.Definition.StationKey;
                Points.Add(key, driver);
                var input = entry.Device as NP_Input;
                if (input != null)
                {
                    inputs.Add(key, input);
                    typeof(NP_Input).GetField("dInput", Instance).SetValue(input, driver);
                    SetInput(key, null);
                    continue;
                }

                var output = (NP_Output)entry.Device;
                typeof(NP_Output).GetField("dOutput", Instance).SetValue(output, driver);
                typeof(NP_Output).GetField("ready", Instance).SetValue(output, true);
            }
        }

        /// <summary>设置输入状态，空值模拟驱动尚未就绪。</summary>
        internal void SetInput(string key, bool? value)
        {
            Points[key].State = value.GetValueOrDefault();
            typeof(NP_Input).GetField("ready", Instance).SetValue(inputs[key], value.HasValue);
        }
    }

    /// <summary>从构建目录解析应用依赖，再执行离线下料验证。</summary>
    [STAThread]
    private static int Main(string[] args)
    {
        AppDomain.CurrentDomain.AssemblyResolve += (sender, request) =>
        {
            string name = new AssemblyName(request.Name).Name;
            foreach (string root in new[] { args[0], @"C:\Program Files\Cognex\VisionPro\ReferencedAssemblies" })
            foreach (string extension in new[] { ".dll", ".exe" })
            {
                string path = Path.Combine(root, name + extension);
                if (File.Exists(path)) return Assembly.LoadFrom(path);
            }
            return null;
        };
        try { return Verify(args[1]); }
        catch (Exception error)
        {
            for (Exception cause = error; cause != null; cause = cause.InnerException)
                Console.Error.WriteLine("{0}: {1}\n{2}", cause.GetType().FullName, cause.Message, cause.StackTrace);
            return 1;
        }
    }

    /// <summary>通过应用真实工站入口验证下料流程，使用内存驱动。</summary>
    private static int Verify(string directory)
    {
        SysPara.SettingDataDirectory = Path.Combine(directory, "Settings");
        SysPara.LanguageDataDirectory = Path.Combine(directory, "LanguageData");
        Alarm.Initial(Path.Combine(directory, "offline-alarm-table.xml"));
        ModuleManager.SettingDataDirectory = SysPara.SettingDataDirectory;
        var app = (MachineApplication)Activator.CreateInstance(typeof(DownConveryor).Assembly.GetType("AlphaRap.StationApplication"), true);
        using (HardForm hardware = app.CreateHardwarePage())
        {
            SaveOfflineSettings(hardware.ResourceDefaults);
            ApplicationHost.RegisterModule(hardware);
            var watchedStates = new List<bool>();
            object background = app.GetType().GetField("background", Instance).GetValue(app);
            MethodInfo watch = background.GetType().GetMethod("WatchInput", Instance);
            watch.Invoke(background, new object[] { WatchedInput, new Action<bool>(watchedStates.Add) });
            int missingNotifications = 0;
            watch.Invoke(background, new object[] { "IB_OptionalMissing", new Action<bool>(_ => missingNotifications++) });
            app.GetType().GetMethod("BindStationIo", Instance).Invoke(app, new object[] { hardware.Resources });
            var simulator = new Simulator(hardware.Resources);
            VerifyWithoutManualPage(app, hardware.Resources);
            VerifySafetyMonitor(app, simulator);
            VerifyNamedWatch(app, hardware.Resources, simulator, watchedStates);
            Check(missingNotifications == 0, "A missing optional input must not fire a callback.");
            Flow_BaseForm[] stations = app.CreateStations().ToArray();
            try
            {
                var flow = stations.OfType<DownConveryor>().Single();
                VerifyTransfer(flow, simulator);
                VerifyStop(flow, simulator);
            }
            finally
            {
                foreach (Flow_BaseForm station in stations) station.Dispose();
                app.GetType().GetMethod("ReleaseStationIo", Instance).Invoke(app, null);
            }
        }

        Console.WriteLine("Passed {0} checks.", checks);
        return 0;
    }

    /// <summary>检查工站 IO 和手动页共用驱动，关闭并重建手动页后保持原实例。</summary>
    private static void VerifyWithoutManualPage(MachineApplication app, HardwareResources resources)
    {
        NP_Output output = ApplicationHost.GetOutput("OB_DownConveyor_Forward");
        NP_Cylinder cylinder = ApplicationHost.GetCylinder("OB_DownConveyor_Stop_Cylinder");
        Check(output.IsReady && !cylinder.IsDisposed, "Shared I/O must be usable before any manual page exists.");

        for (int index = 0; index < 2; index++)
        {
            using (ManualPage manual = app.CreateManualPage())
            {
                manual.BindResources(resources);
                Check(manual.Contains(output) && manual.Contains(cylinder), "Manual page must display the original resource controls.");
            }

            Check(ReferenceEquals(output, ApplicationHost.GetOutput("OB_DownConveyor_Forward")) && output.IsReady && !output.IsDisposed,
                "Disposing the manual page must preserve the driver instance.");
            Check(ReferenceEquals(cylinder, ApplicationHost.GetCylinder("OB_DownConveyor_Stop_Cylinder")) && !cylinder.IsDisposed,
                "Disposing the manual page must preserve the cylinder instance.");
        }
    }

    /// <summary>验证现场许可、运行中失联、复位及仿真路径，使用真实整机停止入口。</summary>
    private static void VerifySafetyMonitor(MachineApplication app, Simulator simulator)
    {
        SysPara.LanguageShow = LanguageType.English;
        Type applicationAssemblyType = app.GetType();
        Type host = applicationAssemblyType.Assembly.GetType("AlphaRap.MiddleLayer", true);
        host.GetField("Application", Static).SetValue(null, app);
        object io = applicationAssemblyType.GetProperty("Io", Static).GetValue(null, null);
        Type monitor = applicationAssemblyType.Assembly.GetType("AlphaRap.AlwaysRunTask", true);
        MethodInfo blockReason = monitor.GetMethod("SafetyBlockReason", Static);
        Func<string> reason = () => (string)blockReason.Invoke(null, new[] { io });

        Check(reason().Contains("cannot be read"), "Unknown inputs must block initialization with a diagnostic.");
        foreach (string point in SafetyInputs) simulator.SetInput(point, point != "IB_Back_EmergencyStop");
        Check(reason() == null, "A reversed, healthy rear emergency stop and three high inputs must permit start.");

        foreach (string point in SafetyInputs)
        {
            bool healthy = point != "IB_Back_EmergencyStop";
            simulator.SetInput(point, !healthy);
            Check(reason().Contains("is not ready"), "An unhealthy input must block start: " + point);
            simulator.SetInput(point, healthy);
        }

        simulator.Points["IB_Air"].InputReadFailed = true;
        Check(reason().Contains("cannot be read"), "A read failure must stay unknown even when the driver object is ready.");
        MachineState.Global.Initialize();
        MachineState.Global.Start(true);
        Thread.Sleep(120);
        app.Scan();
        Task<bool> stop = (Task<bool>)host.GetProperty("MachineStopCompletion", Static).GetValue(null, null);
        Check(stop.Wait(5000) && stop.Result && SysPara.SystemMode == RunMode.IDLE, "Input failure during production must use coordinated stop.");
        Check(Alarm.AlarmList.Count(alarm => alarm.Code == "3023") == 1, "The monitor must publish one diagnostic for the fault.");
        Thread.Sleep(120);
        app.Scan();
        Check(Alarm.AlarmList.Count(alarm => alarm.Code == "3023") == 1, "A persistent fault must not flood alarms.");

        simulator.Points["IB_Air"].InputReadFailed = false;
        Check(reason() == null && SysPara.SystemMode == RunMode.IDLE, "Recovery must clear the permit check without starting a session.");
        SysPara.Simulation = true;
        simulator.Points["IB_Air"].InputReadFailed = true;
        Check(reason() == null, "Simulation must not require physical input reads.");
        MachineState.Global.Initialize();
        app.Scan();
        Check(SysPara.SystemMode == RunMode.INITIAL, "Simulation must not dispatch a safety stop.");
        SysPara.SystemMode = RunMode.IDLE;
        SysPara.Simulation = false;
        Alarm.Clear();
    }

    /// <summary>验证界面配置的普通输入可直接按名称初始化、采样及处理读数失效。</summary>
    private static void VerifyNamedWatch(MachineApplication app, HardwareResources resources, Simulator simulator, List<bool> watchedStates)
    {
        object io = app.GetType().GetProperty("Io", Static).GetValue(null, null);
        HardwareEntry entry = resources.Entries.Single(item => item.Definition.Name == WatchedInput);
        Check(string.IsNullOrEmpty(entry.Definition.StationKey), "A normal dynamic input must work without a template key.");
        foreach (HardwareEntry input in resources.Entries.Where(item => !item.Definition.ConnectionPending && item.Device is NP_Input))
        {
            string name = string.IsNullOrEmpty(input.Definition.StationKey) ? input.Definition.Name : input.Definition.StationKey;
            simulator.SetInput(name, false);
        }

        NP_Input original = (NP_Input)entry.Device;
        using (var probe = new OfflineMonitoredInput())
        {
            entry.Device = probe;
            try { io.GetType().GetMethod("Initialize", Instance).Invoke(io, null); }
            catch (TargetInvocationException error)
            {
                Check(error.InnerException is InvalidOperationException, "Only the unconfigured legacy station points should fail preflight.");
            }
            finally { entry.Device = original; }
            Check(probe.Initializations == 1, "The named input must be prepared through station initialization.");
        }

        simulator.SetInput(WatchedInput, false);
        Thread.Sleep(120);
        app.Scan();
        Check(watchedStates.SequenceEqual(new[] { true }), "The first valid, inverted value must reach the callback.");
        Check(Equals(io.GetType().GetMethod("ReadSafetyInput", Instance).Invoke(io, new object[] { WatchedInput }), true),
            "A new safety input can also be resolved by name.");
        Thread.Sleep(120);
        app.Scan();
        Check(watchedStates.Count == 1, "Stable state must not repeat the callback.");

        simulator.Points[WatchedInput].InputReadFailed = true;
        Thread.Sleep(120);
        app.Scan();
        Check(watchedStates.Count == 1, "Read failure must not be published as OFF.");

        simulator.Points[WatchedInput].InputReadFailed = false;
        Thread.Sleep(120);
        app.Scan();
        Check(watchedStates.SequenceEqual(new[] { true, true }), "An available input must publish its first valid value after recovery.");
        simulator.SetInput(WatchedInput, true);
        Thread.Sleep(120);
        app.Scan();
        Check(watchedStates.SequenceEqual(new[] { true, true, false }), "Changes must use the configured effective state.");

        foreach (HardwareEntry input in resources.Entries.Where(item => !item.Definition.ConnectionPending && item.Device is NP_Input))
        {
            string name = string.IsNullOrEmpty(input.Definition.StationKey) ? input.Definition.Name : input.Definition.StationKey;
            simulator.SetInput(name, null);
        }
        Alarm.Clear();
    }

    /// <summary>为离线用例中的下料及监视点位分配独立地址。</summary>
    private static void SaveOfflineSettings(HardwareProject project)
    {
        Directory.CreateDirectory(SysPara.SettingDataDirectory);
        int pointIndex = 0;
        foreach (HardwareDefinition point in project.Resources.Where(item => item.StationKey.Contains("_DownConveyor_") || SafetyInputs.Contains(item.StationKey)))
        {
            point.Port = "001" + (pointIndex++).ToString("D2");
            point.ConnectionPending = false;
            point.ReverseState = point.StationKey == "IB_Back_EmergencyStop";
        }

        project.Resources.Add(new HardwareDefinition
        {
            Kind = "Input", Name = WatchedInput, IoPageId = project.IoPages[0].Id,
            Port = "001" + pointIndex.ToString("D2"), ReverseState = true
        });

        string path = Path.Combine(SysPara.SettingDataDirectory, "HardwareResources.xml");
        using (var stream = File.Create(path)) new XmlSerializer(typeof(HardwareProject)).Serialize(stream, project);
    }

    /// <summary>按真实流程连线验证接板、换速、挡停、下游交接。</summary>
    private static void VerifyTransfer(DownConveryor flow, Simulator simulator)
    {
        Flow_Chart node = FindNode(flow, "1_1");
        Step(ref node, "1_1", Result.NEXT);
        Step(ref node, "1_2", Result.IDLE);
        simulator.SetInput("IB_DownConveyor_BoardStop_Sensor", false);
        Step(ref node, "1_2", Result.NEXT);
        Step(ref node, "1_4", Result.IDLE);
        Check(simulator.Points["OB_DownConveyor_StopDown_Cylinder"].State
            && !simulator.Points["OB_DownConveyor_StopUp_Cylinder"].State, "Stopper must be raised while waiting for feedback.");
        simulator.SetInput("IB_DownConveyor_StopUp_Sensor", true);
        Step(ref node, "1_4", Result.NEXT);
        Step(ref node, "1_5", Result.NEXT);
        Step(ref node, "1_6", Result.IDLE);
        simulator.SetInput("IB_DownConveyor_UpMachineAvailable_SMEMA", true);
        Step(ref node, "1_6", Result.NEXT);
        Step(ref node, "1_7", Result.NEXT);
        Check(simulator.Points["OB_DownConveyor_LocalMachineReady_SMEMA"].State && simulator.Points["OB_DownConveyor_Forward"].State, "Infeed handshake and drive must be active.");
        Step(ref node, "1_8", Result.IDLE);
        simulator.SetInput("IB_DownConveyor_BoardIn_Sensor", true);
        Step(ref node, "1_8", Result.NEXT);
        simulator.SetInput("IB_DownConveyor_BoardIn_Sensor", null);
        Step(ref node, "1_9", Result.IDLE);
        simulator.SetInput("IB_DownConveyor_BoardIn_Sensor", false);
        Step(ref node, "1_9", Result.NEXT);
        Step(ref node, "2_0", Result.NEXT);
        simulator.SetInput("IB_DownConveyor_BoardSpeedSwitch_Sensor", false);
        Step(ref node, "2_1", Result.NEXT);
        Check(simulator.Points["IB_DownConveyor_BoardSpeedSwitch_Sensor"].Reads > 0, "Existing slowdown sampling must be preserved.");
        Step(ref node, "2_2", Result.NEXT);
        Check(simulator.Points["OB_DownConveyor_SpeedSwitch"].State, "Speed output must follow the speed change step.");
        Step(ref node, "2_3", Result.IDLE);
        simulator.SetInput("IB_DownConveyor_BoardStop_Sensor", true);
        Step(ref node, "2_3", Result.NEXT);
        Step(ref node, "2_4", Result.NEXT);
        Check(!simulator.Points["OB_DownConveyor_Forward"].State && !simulator.Points["OB_DownConveyor_SpeedSwitch"].State, "Positioning must stop forward and speed outputs.");
        Step(ref node, "2_5", Result.NEXT);
        Step(ref node, "2_6", Result.IDLE);
        simulator.SetInput("IB_DownConveyor_DownMachineReady_SMEMA", true);
        Step(ref node, "2_6", Result.NEXT);
        Step(ref node, "2_7", Result.IDLE);
        Check(!simulator.Points["OB_DownConveyor_StopDown_Cylinder"].State
            && simulator.Points["OB_DownConveyor_StopUp_Cylinder"].State, "Discharge must use the opposite stopper output.");
        simulator.SetInput("IB_DownConveyor_StopDown_Sensor", true);
        Step(ref node, "2_7", Result.NEXT);
        Step(ref node, "2_8", Result.NEXT);
        Step(ref node, "2_9", Result.IDLE);
        simulator.SetInput("IB_DownConveyor_BoardOut_Sensor", true);
        Step(ref node, "2_9", Result.NEXT);
        Step(ref node, "3_0", Result.IDLE);
        simulator.SetInput("IB_DownConveyor_BoardOut_Sensor", false);
        Step(ref node, "3_0", Result.NEXT);
        Step(ref node, "3_1", Result.IDLE);
        simulator.SetInput("IB_DownConveyor_DownMachineReady_SMEMA", null);
        Step(ref node, "3_1", Result.IDLE);
        simulator.SetInput("IB_DownConveyor_DownMachineReady_SMEMA", false);
        Step(ref node, "3_1", Result.NEXT);
        Step(ref node, "3_2", Result.NEXT);
        Check(!simulator.Points["OB_DownConveyor_Forward"].State, "The conveyor must stop before leaving the handoff.");
        Step(ref node, "3_3", Result.NEXT);
        Step(ref node, "3_4", Result.NEXT);
        Step(ref node, "3_5", Result.NEXT);
        Check(node.Name == "DownConveyorAuto_Flow1_1", "The next cycle must use the original loop.");
    }

    /// <summary>确认已有板件走出板分支，停止时继续撤销其余输出。</summary>
    private static void VerifyStop(DownConveryor flow, Simulator simulator)
    {
        Flow_Chart node = FindNode(flow, "1_2");
        Step(ref node, "1_2", Result.CASE1);
        Step(ref node, "1_3", Result.NEXT);
        Step(ref node, "2_5", Result.NEXT);
        simulator.Points["OB_DownConveyor_LocalMachineReady_SMEMA"].State = true;
        simulator.Points["OB_DownConveyor_Forward"].State = true;
        simulator.Points["OB_DownConveyor_SpeedSwitch"].FailOff = true;
        bool reported = false;
        try { flow.StopRun(); }
        catch (AggregateException error) { reported = error.Flatten().InnerExceptions.Count == 1; }
        Check(reported, "Partial stop failures must be reported.");
        Check(!simulator.Points["OB_DownConveyor_Forward"].State && !simulator.Points["OB_DownConveyor_LocalMachineReady_SMEMA"].State && !simulator.Points["OB_DownConveyor_LocalMachineAvailable_SMEMA"].State,
            "One failed output must not skip other stop commands.");
    }

    /// <summary>按设计器名称取实际的自动步骤。</summary>
    private static Flow_Chart FindNode(DownConveryor flow, string name)
    {
        return (Flow_Chart)flow.GetType().GetField("DownConveyorAuto_Flow" + name, Instance).GetValue(flow);
    }

    /// <summary>调用节点绑定的原生事件并按结果走设计器连线。</summary>
    private static void Step(ref Flow_Chart node, string name, Result expected)
    {
        Check(node != null && node.Name == "DownConveyorAuto_Flow" + name, "Unexpected next node: " + name);
        Result actual = node.GetFlowChartHandlers().Single()();
        Check(actual == expected, node.Name + " returned " + actual + " instead of " + expected);
        if (actual == Result.NEXT) node = node.Next;
        if (actual == Result.CASE1) node = node.Casse1;
    }

    /// <summary>失败时报告正在验证的实际行为。</summary>
    private static void Check(bool success, string message)
    {
        if (!success) throw new InvalidOperationException(message);
        checks++;
    }
}
