using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using AlphaRap.Configuration;
using AlphaRapLibrary;
using NPSDK;
using NPSDK.Motion;

namespace AlphaRap.TaskProcess
{
    /// <summary>为本站工艺信号使用硬件目录中的实际 IO 实例。</summary>
    internal sealed class StationIo
    {
        private readonly HardwareResources resources;
        private volatile Dictionary<string, HardwareEntry> points = new Dictionary<string, HardwareEntry>(StringComparer.OrdinalIgnoreCase);
        private static readonly string[] FlowPoints =
        {
            "IB_DownConveyor_BoardIn_Sensor",
            "IB_DownConveyor_BoardOut_Sensor",
            "IB_DownConveyor_BoardSpeedSwitch_Sensor",
            "IB_DownConveyor_BoardStop_Sensor",
            "IB_DownConveyor_DownMachineReady_SMEMA",
            "IB_DownConveyor_StopDown_Sensor",
            "IB_DownConveyor_StopUp_Sensor",
            "IB_DownConveyor_UpMachineAvailable_SMEMA",
            "IB_UpConveyor_Buffer_BoardIn_Sensor",
            "IB_UpConveyor_Buffer_BoardSpeedSwitch_Sensor",
            "IB_UpConveyor_Buffer_BoardStop_Sensor",
            "IB_UpConveyor_Buffer_StopDown_Sensor",
            "IB_UpConveyor_Buffer_StopUp_Sensor",
            "IB_UpConveyor_DownMachineReady_SMEMA",
            "IB_UpConveyor_UpMachineAvailable_SMEMA",
            "IB_UpConveyor_Work_BoardAlarm",
            "IB_UpConveyor_Work_BoardOut_Sensor",
            "IB_UpConveyor_Work_BoardSpeedSwitch_Sensor",
            "IB_UpConveyor_Work_BoardStop_Sensor",
            "IB_UpConveyor_Work_JackingDown_Sensor",
            "IB_UpConveyor_Work_JackingUp_Sensor",
            "IB_UpConveyor_Work_StopDown_Sensor",
            "IB_UpConveyor_Work_StopUp_Sensor",
            "OB_DownConveyor_Forward",
            "OB_DownConveyor_LocalMachineAvailable_SMEMA",
            "OB_DownConveyor_LocalMachineReady_SMEMA",
            "OB_DownConveyor_Reverse",
            "OB_DownConveyor_SpeedSwitch",
            "OB_Upconveyor_AlarmClear",
            "OB_UpConveyor_Buffer_ConveyorForward",
            "OB_UpConveyor_Buffer_ConveyorReverse",
            "OB_UpConveyor_Buffer_ConveyorSpeedSwitch",
            "OB_UpConveyor_LocalMachineAvailable_SMEMA",
            "OB_UpConveyor_LocalMachineReady_SMEMA",
            "OB_UpConveyor_Work_ConveyorForward",
            "OB_UpConveyor_Work_ConveyorReverse",
            "OB_UpConveyor_Work_ConveyorSpedSwitch",
        };
        internal static readonly string[] PauseOutputs =
        {
            "OB_UpConveyor_Work_ConveyorForward", "OB_UpConveyor_Work_ConveyorReverse",
            "OB_UpConveyor_LocalMachineReady_SMEMA", "OB_UpConveyor_LocalMachineAvailable_SMEMA"
        };

        private static readonly string[] FlowCylinders =
        {
            "OB_UpConveyor_Buffer_Stop_Cylinder", "OB_UpConveyor_Work_Stop_Cylinder",
            "OB_UpConveyor_Work_Jacking_Cylinder", "OB_DownConveyor_Stop_Cylinder"
        };

        /// <summary>加载项目随程序集发布的初始页签及未接线点位。</summary>
        internal static HardwareProject LoadDefaults()
        {
            using (var input = typeof(StationIo).Assembly.GetManifestResourceStream("AlphaRap.StationIoDefaults.xml"))
                return (HardwareProject)new XmlSerializer(typeof(HardwareProject)).Deserialize(input);
        }

        /// <summary>按当前语言显示工站点位，缺少配置时用原工艺键定位问题。</summary>
        private string Caption(string key)
        {
            HardwareDefinition definition = Find(key)?.Definition;
            return definition == null ? key : HardwareResources.DisplayText(definition);
        }

        /// <summary>缓存工艺用的 IO，配置重载后使用当前硬件目录。</summary>
        internal StationIo(HardwareResources resources, HardwareResourcePanel panel)
        {
            this.resources = resources;
            RefreshPoints(this, EventArgs.Empty);
            resources.Changed += RefreshPoints;
            panel.Disposed += (sender, args) => resources.Changed -= RefreshPoints;
        }

        /// <summary>替换目录快照，避免撤销配置后继续持有旧 IO 控件。</summary>
        private void RefreshPoints(object sender, EventArgs args)
        {
            points = resources.Entries.Where(entry => !string.IsNullOrEmpty(entry.Definition.StationKey))
                .ToDictionary(entry => entry.Definition.StationKey, StringComparer.OrdinalIgnoreCase);
            foreach (NP_Output point in points.Values.Select(entry => entry.Device).OfType<NP_Output>())
            {
                point.ManualStateChanged -= LogManualOutput;
                point.ManualStateChanged += LogManualOutput;
            }
        }

        /// <summary>手动操作仍按实际状态写入原操作日志。</summary>
        private static void LogManualOutput(NP_Output output, bool state)
        {
            OperationLog.Write("ManualForm", "手动输出：" + output.Text + " → " + (state ? "开" : "关"));
        }

        /// <summary>准备已保存的 IO，所有工艺所需点位完全就绪后才允许派发流程。</summary>
        internal void Initialize()
        {
            if (resources.IsDirty || resources.RequiresRestart)
                throw new InvalidOperationException(resources.RequiresRestart ? HardwareText.T("restart") :
                    FrameworkLanguage.LangText("StationIo", "save", "请先从主界面保存或放弃 IO 配置修改。"));
            if (SysPara.Simulation) return;
            var failures = new List<string>();
            var stationPoints = points.Values.Where(entry => !entry.Definition.ConnectionPending && IsSignalDirectionValid(entry.Definition.StationKey, entry.Device));
            foreach (HardwareEntry entry in stationPoints.Concat(resources.CylinderOutputs()).Distinct())
            {
                try { InitializePoint(entry.Device); }
                catch (Exception error) { failures.Add(HardwareResources.DisplayText(entry.Definition) + " · " + error.Message); }
            }
            string[] unavailable = FlowPoints.Where(key => !SignalReady(key)).Select(Caption)
                .Concat(FlowCylinders.Where(key => !resources.IsStationCylinderReady(key)).Select(resources.StationCylinderName)).ToArray();
            if (unavailable.Length == 0) return;
            throw new InvalidOperationException(FrameworkLanguage.LangText("StationIo", "unavailable", "这些工艺 IO 尚未配置或驱动未就绪。") + Environment.NewLine +
                string.Join(Environment.NewLine, unavailable.Concat(failures)));
        }

        /// <summary>在独立的设备准备步骤中初始化同一个控件所持有的驱动。</summary>
        private static void InitializePoint(ControlBaseInterface device)
        {
            lock (device)
            {
                if (device.IsDisposed || Ready(device)) return;
                device.DisposeObj();
                device.Initial();
            }
        }

        /// <summary>读取已经完成连接的工艺输入，未知状态保持为空。</summary>
        internal bool? Read(string key)
        {
            var input = Find(key)?.Device as NP_Input;
            return !SysPara.Simulation && Ready(input) ? (bool?)input.IsOn() : null;
        }

        /// <summary>读取必需工艺输入，连接缺失时报告该信号。</summary>
        internal bool IsOn(string key) { return Read(key) ?? throw Missing(key); }

        /// <summary>返回已就绪的工艺输出，供流程写入。</summary>
        internal NP_Output Output(string key) { return ReadyOutput(key) ?? throw Missing(key); }

        /// <summary>查找不为空且已就绪的工艺输出，供停止及暂停使用。</summary>
        internal NP_Output ReadyOutput(string key)
        {
            var output = Find(key)?.Device as NP_Output;
            return !SysPara.Simulation && Ready(output) ? output : null;
        }

        /// <summary>使用该气缸在动态目录中已连接的两路输出。</summary>
        internal NP_Cylinder Cylinder(string key) { return resources.GetStationCylinder(key); }

        /// <summary>读取工站用途的同一个已建立资源，未分配地址的点位不参与工艺。</summary>
        private HardwareEntry Find(string key)
        {
            return points.TryGetValue(key, out HardwareEntry entry) && !entry.Definition.ConnectionPending ? entry : null;
        }

        /// <summary>校验工艺键对应的 IO 方向及驱动状态。</summary>
        private bool SignalReady(string key)
        {
            ControlBaseInterface device = Find(key)?.Device;
            return IsSignalDirectionValid(key, device) && Ready(device);
        }

        /// <summary>工艺输出必须绑定输出对象，其余站内信号均为输入。</summary>
        private static bool IsSignalDirectionValid(string key, ControlBaseInterface device)
        {
            return key.StartsWith("OB_", StringComparison.OrdinalIgnoreCase) ? device is NP_Output : device is NP_Input;
        }

        /// <summary>检查驱动对象是否已经连接。</summary>
        private static bool Ready(ControlBaseInterface device)
        {
            if (device == null || device.IsDisposed) return false;
            return device is NP_Input input ? input.IsReady : ((NP_Output)device).IsReady;
        }

        /// <summary>按工艺名称提示不可用的 IO。</summary>
        private InvalidOperationException Missing(string key)
        {
            string message = FrameworkLanguage.LangText("StationIo", "missing", "工艺 IO「{0}」未就绪。");
            return new InvalidOperationException(string.Format(message, Caption(key)));
        }
    }
}
