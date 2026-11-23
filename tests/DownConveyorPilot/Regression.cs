using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
    private static int checks;

    /// <summary>模拟 SDK 驱动，在内存中保存点位状态。</summary>
    private sealed class OfflineIo : IOBaseInterface
    {
        internal bool State;
        internal int Reads;
        internal bool FailOff;

        /// <summary>返回模拟输入或输出状态。</summary>
        public override bool GetState() { Reads++; return State; }

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
                string key = entry.Definition.StationKey;
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
            foreach (string extension in new[] { ".dll", ".exe" })
            {
                string path = Path.Combine(args[0], name + extension);
                if (File.Exists(path)) return Assembly.LoadFrom(path);
            }
            return null;
        };
        return Verify(args[1]);
    }

    /// <summary>通过应用真实工站入口验证下料流程，使用内存驱动。</summary>
    private static int Verify(string directory)
    {
        SysPara.SettingDataDirectory = Path.Combine(directory, "Settings");
        SysPara.LanguageDataDirectory = Path.Combine(directory, "LanguageData");
        ModuleManager.SettingDataDirectory = SysPara.SettingDataDirectory;
        var app = (MachineApplication)Activator.CreateInstance(typeof(DownConveryor).Assembly.GetType("AlphaRap.StationApplication"), true);
        using (HardForm hardware = app.CreateHardwarePage())
        using (ManualPage manual = app.CreateManualPage())
        {
            SaveOfflineSettings(hardware.ResourceDefaults);
            ApplicationHost.RegisterModule(hardware);
            manual.BindResources(hardware.Resources);
            var simulator = new Simulator(hardware.Resources);
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
            }
        }

        Console.WriteLine("Passed {0} checks.", checks);
        return 0;
    }

    /// <summary>只为离线用例中的下料点位分配独立地址，写入测试配置目录。</summary>
    private static void SaveOfflineSettings(HardwareProject project)
    {
        Directory.CreateDirectory(SysPara.SettingDataDirectory);
        int pointIndex = 0;
        foreach (HardwareDefinition point in project.Resources.Where(item => item.StationKey.Contains("_DownConveyor_")))
        {
            point.Port = "001" + (pointIndex++).ToString("D2");
            point.ConnectionPending = false;
        }

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
