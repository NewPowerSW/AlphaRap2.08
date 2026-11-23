using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;
using AlphaRap.Configuration;
using AlphaRap.FlowProgramming;
using AlphaRapLibrary;
using NPSDK;
using NPSDK.Motion;

namespace AlphaRap.TaskProcess
{
    /// <summary>为本站工艺信号使用硬件目录中的实际 IO 实例。</summary>
    internal sealed class StationIo
    {
        private readonly HardwareResources resources;
        private readonly Dictionary<string, CylinderAction> cylinders = new Dictionary<string, CylinderAction>();
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

        private sealed class CylinderAction
        {
            internal NP_Cylinder Control;
            internal GroupBox Card;
            internal string OnKey;
            internal string OffKey;
            internal string Chinese;
            internal string English;
            internal string Spanish;
        }

        /// <summary>加载项目随程序集发布的初始页签及未接线点位。</summary>
        internal static HardwareProject LoadDefaults()
        {
            using (var input = typeof(StationIo).Assembly.GetManifestResourceStream("AlphaRap.StationIoDefaults.xml"))
                return (HardwareProject)new XmlSerializer(typeof(HardwareProject)).Deserialize(input);
        }

        /// <summary>翻译项目自带标题，在现场改名后显示设置中的名称。</summary>
        internal static string Caption(string key, string name)
        {
            if (key != name) return name;
            switch (key)
            {
                case "System": return Text(key, "系统", "System", "Sistema");
                case "UpBuffer": return Text(key, "上料缓冲", "Loader buffer", "Búfer de carga");
                case "UpWork": return Text(key, "上料工作区", "Loader work area", "Zona de carga");
                case "DownConveyor": return Text(key, "下料", "Unload conveyor", "Transportador de descarga");
                case "Smema": return Text(key, "联机握手", "Line handshake", "Enlace de línea");
                case "adlink_Input2": return Text(key, "停止按钮 2", "Stop button 2", "Botón de parada 2");
                default: return ApplicationHost.Text("ManualForm", key, name);
            }
        }

        /// <summary>在手动资源页复用已建立的硬件点位并创建气缸组合操作。</summary>
        internal StationIo(HardwareResources resources, HardwareResourcePanel panel)
        {
            this.resources = resources;
            AddCylinder(panel, "OB_UpConveyor_Buffer_Stop_Cylinder", "OB_UpConveyor_Buffer_StopDown_Cylinder", "OB_UpConveyor_Buffer_StopUp_Cylinder",
                "上料缓冲挡停", "Loader buffer stopper", "Tope del búfer de carga");
            AddCylinder(panel, "OB_UpConveyor_Work_Stop_Cylinder", "OB_UpConveyor_Work_StopDown_Cylinder", "OB_UpConveyor_Work_StopUp_Cylinder",
                "上料工作区挡停", "Loader work stopper", "Tope de la zona de carga");
            AddCylinder(panel, "OB_UpConveyor_Work_Jacking_Cylinder", "OB_UpConveyor_Work_StopDown_Cylinder", "OB_UpConveyor_Work_JackingUp_Cylinder",
                "上料顶升", "Loader lift", "Elevador de carga");
            AddCylinder(panel, "OB_DownConveyor_Stop_Cylinder", "OB_DownConveyor_StopDown_Cylinder", "OB_DownConveyor_StopUp_Cylinder",
                "下料挡停", "Unload stopper", "Tope de descarga");
            RefreshPoints(this, EventArgs.Empty);
            RefreshLanguage(this, EventArgs.Empty);
            resources.Changed += RefreshPoints;
            ApplicationHost.LanguageChanged += RefreshLanguage;
            panel.Disposed += (sender, args) =>
            {
                resources.Changed -= RefreshPoints;
                ApplicationHost.LanguageChanged -= RefreshLanguage;
            };
        }

        /// <summary>组合操作只引用目录中的两路输出，手动点击使用既有空闲及权限门槛。</summary>
        private void AddCylinder(HardwareResourcePanel panel, string key, string on, string off, string zh, string en, string es)
        {
            var control = new NP_Cylinder { Name = "cylinder_" + key + "_dyn", Dock = DockStyle.Fill };
            var card = new GroupBox { Name = "action_" + key + "_dyn", Height = 56, Margin = new Padding(5) };
            control.ManualOperationAllowed = () => FlowPrograms.CanEdit && SysPara.SystemMode == RunMode.IDLE &&
                !MiddleLayer.MachineCommandPending && !ProjectConfiguration.IsBusy && !FlowPrograms.AnyRunning &&
                !resources.RequiresRestart && !resources.IsDirty && !SysPara.Simulation && CylinderReady(control);
            control.ManualDriven += state => OperationLog.Write("ManualForm", "手动气缸动作：" + card.Text);
            card.Controls.Add(control);
            cylinders.Add(key, new CylinderAction { Control = control, Card = card, OnKey = on, OffKey = off,
                Chinese = zh, English = en, Spanish = es });
            panel.AddIoAction(card, on);
        }

        /// <summary>替换目录快照并更新线圈引用，避免撤销配置后继续持有旧 IO 控件。</summary>
        private void RefreshPoints(object sender, EventArgs args)
        {
            points = resources.Entries.Where(entry => !string.IsNullOrEmpty(entry.Definition.StationKey))
                .ToDictionary(entry => entry.Definition.StationKey, StringComparer.OrdinalIgnoreCase);
            foreach (NP_Output point in points.Values.Select(entry => entry.Device).OfType<NP_Output>())
            {
                point.ManualStateChanged -= LogManualOutput;
                point.ManualStateChanged += LogManualOutput;
            }
            foreach (CylinderAction action in cylinders.Values)
            {
                action.Control.CyOn_OB1 = Find(action.OnKey)?.Device as NP_Output;
                action.Control.CyOff_OB1 = Find(action.OffKey)?.Device as NP_Output;
            }
        }

        /// <summary>手动操作仍按实际状态写入原操作日志。</summary>
        private static void LogManualOutput(NP_Output output, bool state)
        {
            OperationLog.Write("ManualForm", "手动输出：" + output.Text + " → " + (state ? "开" : "关"));
        }

        /// <summary>只刷新组合按钮的标题。</summary>
        private void RefreshLanguage(object sender, EventArgs args)
        {
            foreach (var pair in cylinders)
                pair.Value.Card.Text = Text(pair.Key, pair.Value.Chinese, pair.Value.English, pair.Value.Spanish);
        }

        /// <summary>准备已保存的 IO，所有工艺所需点位完全就绪后才允许派发流程。</summary>
        internal void Initialize()
        {
            if (resources.IsDirty || resources.RequiresRestart)
                throw new InvalidOperationException(resources.RequiresRestart ? HardwareText.T("restart") :
                    Text("save", "请先从主界面保存或放弃 IO 配置修改。", "Save or discard the IO edits from the main window first.", "Guarde o descarte primero los cambios de E/S en la ventana principal."));
            if (SysPara.Simulation) return;
            var failures = new List<string>();
            foreach (HardwareEntry entry in points.Values.Where(entry => !entry.Definition.ConnectionPending && IsSignalDirectionValid(entry.Definition.StationKey, entry.Device)))
            {
                try { InitializePoint(entry.Device); }
                catch (Exception error) { failures.Add(Caption(entry.Definition.StationKey, entry.Definition.Name) + " · " + error.Message); }
            }
            IEnumerable<string> required = FlowPoints.Concat(cylinders.Values.SelectMany(action => new[] { action.OnKey, action.OffKey }));
            string[] unavailable = required.Distinct().Where(key => !SignalReady(key)).Select(key => Caption(key, Find(key)?.Definition.Name ?? key)).ToArray();
            if (unavailable.Length == 0) return;
            throw new InvalidOperationException(Text("unavailable", "这些工艺 IO 尚未配置或驱动未就绪。", "These workflow IO points are unassigned or their drivers are not ready.",
                "Estas E/S del proceso no están asignadas o no están listas.") + Environment.NewLine +
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
        internal NP_Cylinder Cylinder(string key)
        {
            NP_Cylinder cylinder = cylinders[key].Control;
            if (SysPara.Simulation || !CylinderReady(cylinder)) throw Missing(key);
            return cylinder;
        }

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

        /// <summary>气缸两路输出都就绪后才接受操作。</summary>
        private static bool CylinderReady(NP_Cylinder cylinder) { return Ready(cylinder.CyOn_OB1) && Ready(cylinder.CyOff_OB1); }

        /// <summary>按工艺名称提示不可用的 IO。</summary>
        private static InvalidOperationException Missing(string key)
        {
            return new InvalidOperationException(string.Format(Text("missing", "工艺 IO「{0}」未就绪。",
                "Workflow IO '{0}' is not ready.", "La E/S de proceso «{0}» no está lista."), Caption(key, key)));
        }

        /// <summary>使用框架项目词条提供三语说明。</summary>
        private static string Text(string key, string zh, string en, string es) { return FrameworkLanguage.LangMsg("StationIo", key, zh, en, es); }
    }
}
