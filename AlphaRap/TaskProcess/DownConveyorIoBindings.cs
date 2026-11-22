using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using AlphaRap.Configuration;
using AlphaRapLibrary;
using NPSDK;
using NPSDK.Motion;

namespace AlphaRap.TaskProcess
{
    /// <summary>将下料工艺信号绑定到已保存的机器 IO，并保留旧设备的固定点位。</summary>
    internal sealed class DownConveyorIoBindings
    {
        internal const string StopperRaise = nameof(StopperRaise);
        internal const string StopperLower = nameof(StopperLower);

        private readonly HardwareResources resources;
        private readonly NP_Cylinder cylinder;
        private readonly string path;
        private readonly object outputSync = new object();
        private readonly Dictionary<string, ControlBaseInterface> fixedPoints;
        private Dictionary<string, string> saved = new Dictionary<string, string>();
        private Dictionary<string, string> draft = new Dictionary<string, string>();
        private byte[] originalFile;
        private string loadError;
        private volatile RuntimeBinding active;
        private RuntimeBinding pendingStop;

        internal IReadOnlyDictionary<string, string> Draft => draft;
        internal IEnumerable<string> Signals => fixedPoints.Keys;

        /// <summary>登记下料工位的旧机台点位，读取机器级绑定文件。</summary>
        internal DownConveyorIoBindings(HardwareResources resources, ManualForm manual)
        {
            this.resources = resources ?? throw new ArgumentNullException(nameof(resources));
            path = Path.GetFullPath(Path.Combine(SysPara.SettingDataDirectory, "DownConveyorIoBindings.xml"));
            cylinder = manual.OB_DownConveyor_Stop_Cylinder;
            fixedPoints = new Dictionary<string, ControlBaseInterface>
            {
                { nameof(DownConveyorInput.StopperUp), manual.IB_DownConveyor_StopUp_Sensor },
                { nameof(DownConveyorInput.StopperDown), manual.IB_DownConveyor_StopDown_Sensor },
                { nameof(DownConveyorInput.BoardIn), manual.IB_DownConveyor_BoardIn_Sensor },
                { nameof(DownConveyorInput.Slowdown), manual.IB_DownConveyor_BoardSpeedSwitch_Sensor },
                { nameof(DownConveyorInput.BoardAtStop), manual.IB_DownConveyor_BoardStop_Sensor },
                { nameof(DownConveyorInput.BoardOut), manual.IB_DownConveyor_BoardOut_Sensor },
                { nameof(DownConveyorInput.UpstreamAvailable), manual.IB_DownConveyor_UpMachineAvailable_SMEMA },
                { nameof(DownConveyorInput.DownstreamReady), manual.IB_DownConveyor_DownMachineReady_SMEMA },
                { nameof(DownConveyorOutput.Forward), manual.OB_DownConveyor_Forward },
                { nameof(DownConveyorOutput.Reverse), manual.OB_DownConveyor_Reverse },
                { nameof(DownConveyorOutput.SlowSpeed), manual.OB_DownConveyor_SpeedSwitch },
                { nameof(DownConveyorOutput.ReadyForBoard), manual.OB_DownConveyor_LocalMachineReady_SMEMA },
                { nameof(DownConveyorOutput.BoardAvailable), manual.OB_DownConveyor_LocalMachineAvailable_SMEMA },
                { StopperRaise, cylinder.CyOn_OB1 },
                { StopperLower, cylinder.CyOff_OB1 }
            };
            Load();
        }

        /// <summary>注册三语文案并尊重项目已有语言包。</summary>
        internal static string Text(string key, string zh, string en, string es)
        {
            return FrameworkLanguage.LangMsg("DownConveyorIo", key, zh, en, es);
        }

        /// <summary>给应用人员显示工艺信号的名称，XML 继续使用固定键。</summary>
        internal static string SignalName(string signal)
        {
            switch (signal)
            {
                case nameof(DownConveyorInput.StopperUp): return Text(signal, "挡停升起到位", "Stopper up sensor", "Sensor de tope subido");
                case nameof(DownConveyorInput.StopperDown): return Text(signal, "挡停下降到位", "Stopper down sensor", "Sensor de tope bajado");
                case nameof(DownConveyorInput.BoardIn): return Text(signal, "进板感应", "Board entry sensor", "Sensor de entrada de placa");
                case nameof(DownConveyorInput.Slowdown): return Text(signal, "减速感应", "Slowdown sensor", "Sensor de desaceleración");
                case nameof(DownConveyorInput.BoardAtStop): return Text(signal, "挡停位置有板", "Board at stopper", "Placa en el tope");
                case nameof(DownConveyorInput.BoardOut): return Text(signal, "出板感应", "Board exit sensor", "Sensor de salida de placa");
                case nameof(DownConveyorInput.UpstreamAvailable): return Text(signal, "上游有板", "Upstream board available", "Placa disponible aguas arriba");
                case nameof(DownConveyorInput.DownstreamReady): return Text(signal, "下游可接板", "Downstream ready", "Equipo siguiente listo");
                case nameof(DownConveyorOutput.Forward): return Text(signal, "输送正转", "Conveyor forward", "Avance del transportador");
                case nameof(DownConveyorOutput.Reverse): return Text(signal, "输送反转", "Conveyor reverse", "Retroceso del transportador");
                case nameof(DownConveyorOutput.SlowSpeed): return Text(signal, "输送减速", "Conveyor slow speed", "Velocidad baja del transportador");
                case nameof(DownConveyorOutput.ReadyForBoard): return Text(signal, "本机可接板", "Local ready for board", "Equipo local listo");
                case nameof(DownConveyorOutput.BoardAvailable): return Text(signal, "本机有板", "Local board available", "Placa disponible en equipo local");
                case StopperRaise: return Text(signal, "挡停升起输出", "Raise stopper output", "Salida para subir el tope");
                case StopperLower: return Text(signal, "挡停下降输出", "Lower stopper output", "Salida para bajar el tope");
                default: return signal;
            }
        }

        /// <summary>列出此信号方向相同的动态 IO，按业务名称显示。</summary>
        internal IEnumerable<HardwareEntry> Choices(string signal)
        {
            bool input = fixedPoints[signal] is NP_Input;
            return resources.Entries.Where(entry => input ? entry.Device is NP_Input : entry.Device is NP_Output)
                .OrderBy(entry => entry.Definition.Name, StringComparer.CurrentCultureIgnoreCase);
        }

        /// <summary>显示源文件错误，保护无法读取的原文件。</summary>
        internal void RequireLoaded()
        {
            if (loadError != null) throw new InvalidOperationException(loadError);
        }

        /// <summary>只更新内存草稿，不连接设备或发布工位绑定。</summary>
        internal void Edit(Dictionary<string, string> changes)
        {
            RequireLoaded();
            Resolve(changes);
            draft = new Dictionary<string, string>(changes);
        }

        /// <summary>保存时检查工位引用，包括同次保存中添加、重命名或删除的资源。</summary>
        internal void Validate()
        {
            RequireLoaded();
            Resolve(draft);
        }

        /// <summary>将更改加入主保存事务，全部文件成功后才发布本次选择。</summary>
        internal ConfigurationChange PrepareSave()
        {
            if (SameBindings()) return new ConfigurationChange(Array.Empty<ConfigurationFile>());
            var snapshot = new Dictionary<string, string>(draft);
            var document = new XDocument(new XElement("DownConveyorIoBindings", new XAttribute("version", 1),
                snapshot.OrderBy(pair => pair.Key, StringComparer.Ordinal).Select(pair => new XElement("Binding",
                    new XAttribute("signal", pair.Key), new XAttribute("resourceId", pair.Value)))));
            byte[] bytes;
            using (var output = new MemoryStream())
            {
                document.Save(output);
                bytes = output.ToArray();
            }
            return new ConfigurationChange(new[] { ConfigurationFile.CompareAndReplace(path, originalFile, pending => File.WriteAllBytes(pending, bytes)) },
                () => { saved = snapshot; originalFile = bytes; });
        }

        /// <summary>撤销编辑草稿，恢复已保存的资源选择。</summary>
        internal void Discard() { draft = new Dictionary<string, string>(saved); }

        /// <summary>检查主保存前后的两份映射是否一致。</summary>
        private bool SameBindings()
        {
            return saved.Count == draft.Count && saved.All(pair => draft.TryGetValue(pair.Key, out string id)
                && string.Equals(pair.Value, id, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>读取绑定文件；全新和旧项目缺少文件时沿用固定点。</summary>
        private void Load()
        {
            try
            {
                originalFile = File.ReadAllBytes(path);
                using (var input = new MemoryStream(originalFile)) saved = ReadBindings(XDocument.Load(input).Root);
                Discard();
            }
            catch (FileNotFoundException) { }
            catch (DirectoryNotFoundException) { }
            catch (Exception error) when (error is System.Xml.XmlException || error is InvalidDataException || error is IOException || error is UnauthorizedAccessException)
            {
                loadError = string.Format(Text("loadError", "无法读取下料 IO 绑定文件 {0}。{1}",
                    "Cannot read down conveyor IO bindings from {0}. {1}", "No se pueden leer las asignaciones de E/S de descarga de {0}. {1}"), path, error.Message);
            }
        }

        /// <summary>拒绝未知信号、重复键和无效 ID，以免损坏的文件启用旧端口。</summary>
        private Dictionary<string, string> ReadBindings(XElement root)
        {
            if (root?.Name != "DownConveyorIoBindings" || (string)root.Attribute("version") != "1")
                throw new InvalidDataException("DownConveyorIoBindings / version");
            var bindings = new Dictionary<string, string>();
            foreach (XElement row in root.Elements())
            {
                string signal = (string)row.Attribute("signal") ?? "";
                string id = (string)row.Attribute("resourceId");
                if (row.Name != "Binding" || !fixedPoints.ContainsKey(signal) || !Guid.TryParseExact(id, "N", out _) || bindings.ContainsKey(signal))
                    throw new InvalidDataException(signal + " / " + id);
                bindings.Add(signal, id);
            }
            return bindings;
        }

        /// <summary>按 ID 与方向解析所有已选资源，拒绝缺失、错配或重复。</summary>
        private Dictionary<string, HardwareEntry> Resolve(IReadOnlyDictionary<string, string> bindings)
        {
            var entries = resources.Entries.ToDictionary(entry => entry.Definition.Id, StringComparer.OrdinalIgnoreCase);
            var points = new Dictionary<string, HardwareEntry>();
            var used = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in bindings)
            {
                string signal = pair.Key;
                if (!fixedPoints.TryGetValue(signal, out ControlBaseInterface fixedPoint) ||
                    !entries.TryGetValue(pair.Value, out HardwareEntry entry) ||
                    (fixedPoint is NP_Input ? !(entry.Device is NP_Input) : !(entry.Device is NP_Output)))
                    throw new InvalidOperationException(string.Format(Text("missing", "下料信号「{0}」绑定的 IO 已删除或方向不符（ID {1}）。请重新选择。",
                        "The IO for down conveyor signal '{0}' is missing or has the wrong direction (ID {1}). Select it again.",
                        "La E/S de descarga «{0}» no existe o tiene la dirección incorrecta (ID {1}). Selecciónela de nuevo."), SignalName(signal), pair.Value));
                if (used.TryGetValue(pair.Value, out string other))
                    throw new InvalidOperationException(string.Format(Text("duplicate", "下料信号「{0}」和「{1}」选择了同一个 IO「{2}」。",
                        "Down conveyor signals '{0}' and '{1}' use the same IO '{2}'.",
                        "Las señales de descarga «{0}» y «{1}» usan la misma E/S «{2}»."), SignalName(other), SignalName(signal), entry.Definition.Name));
                used.Add(pair.Value, signal);
                points.Add(signal, entry);
            }
            return points;
        }

        /// <summary>在初始化派发之前捕获一套完整、已保存且适用于当前硬件的绑定。</summary>
        internal RuntimeBinding PrepareInitialization(bool simulation)
        {
            RequireLoaded();
            if (!SameBindings() || (saved.Count > 0 && resources.IsDirty))
                throw new InvalidOperationException(Text("saveFirst", "下料 IO 配置有待保存的修改，请先从主界面保存或放弃。",
                    "Down conveyor IO has pending changes. Save or discard from the main window first.",
                    "Hay cambios pendientes en las E/S de descarga. Guarde o descarte primero en la ventana principal."));
            if (saved.Count > 0 && resources.RequiresRestart)
                throw new InvalidOperationException(Text("restart", "硬件参数已改变，请重启程序后再初始化下料 IO。",
                    "Hardware parameters changed. Restart the application before initializing down conveyor IO.",
                    "Los parámetros de hardware cambiaron. Reinicie antes de inicializar las E/S de descarga."));
            Dictionary<string, HardwareEntry> selected = Resolve(saved);
            foreach (var pair in selected)
            {
                try { resources.ValidatePhysicalConnection(pair.Value); }
                catch (InvalidOperationException error)
                {
                    throw new InvalidOperationException(SignalName(pair.Key) + " / " + pair.Value.Definition.Name
                        + Environment.NewLine + error.Message, error);
                }
            }
            return selected.Count == 0 ? null : new RuntimeBinding(fixedPoints, selected, simulation);
        }

        /// <summary>确认所有动态点就绪后，由当前初始化命令发布整组绑定。</summary>
        internal void Activate(RuntimeBinding binding) { active = binding; }

        /// <summary>读取一个工艺输入；未知状态不作为关闭信号。</summary>
        internal bool? Read(DownConveyorInput signal)
        {
            RuntimeBinding binding = active;
            if (binding != null && binding.Simulation) return null;
            if (binding == null && saved.Count > 0) return null;
            NP_Input input = (NP_Input)(binding == null ? fixedPoints[signal.ToString()] : binding.Points[signal.ToString()]);
            return input.IsReady ? (bool?)input.IsOn() : null;
        }

        /// <summary>输出使用同一套初始化结果，挡停先关闭反向线圈再驱动目标线圈。</summary>
        internal void Write(DownConveyorOutput signal, bool on)
        {
            lock (outputSync)
            {
                RuntimeBinding binding = active;
                if ((binding == null && saved.Count > 0) || (binding != null && binding.Simulation)) return;
                if (signal != DownConveyorOutput.Stopper)
                {
                    Drive((NP_Output)(binding == null ? fixedPoints[signal.ToString()] : binding.Points[signal.ToString()]), on);
                    return;
                }
                if (binding == null || !binding.HasMappedStopper)
                {
                    if (on) cylinder.On(); else cylinder.Off();
                    return;
                }
                Drive((NP_Output)binding.Points[on ? StopperLower : StopperRaise], false);
                Drive((NP_Output)binding.Points[on ? StopperRaise : StopperLower], true);
            }
        }

        /// <summary>通过原 SDK IO 实例写入已配置通道。</summary>
        private static void Drive(NP_Output output, bool on)
        {
            if (on) output.On(); else output.Off();
        }

        /// <summary>撤销已启用的动态绑定并关闭本工位输送与握手输出，保留挡停位置。</summary>
        internal void Stop()
        {
            lock (outputSync)
            {
                RuntimeBinding binding = active ?? pendingStop;
                active = null;
                if (binding == null || binding.Simulation) return;
                pendingStop = binding;
                var errors = new List<Exception>();
                foreach (DownConveyorOutput signal in Enum.GetValues(typeof(DownConveyorOutput)))
                {
                    if (signal == DownConveyorOutput.Stopper) continue;
                    try { Drive((NP_Output)binding.Points[signal.ToString()], false); }
                    catch (Exception error) { errors.Add(error); }
                }
                if (errors.Count > 0) throw new AggregateException(errors);
                pendingStop = null;
            }
        }

        /// <summary>工站使用的 IO 快照，驱动仍由对应 IO 资源对象持有。</summary>
        internal sealed class RuntimeBinding
        {
            private readonly Dictionary<string, HardwareEntry> selected;
            internal IReadOnlyDictionary<string, ControlBaseInterface> Points { get; }
            internal bool Simulation { get; }
            internal bool HasMappedStopper => selected.ContainsKey(StopperRaise) || selected.ContainsKey(StopperLower);

            /// <summary>将已配置资源覆盖到对应工艺用途，保留其余固定点。</summary>
            internal RuntimeBinding(Dictionary<string, ControlBaseInterface> fixedPoints, Dictionary<string, HardwareEntry> selected, bool simulation)
            {
                this.selected = selected;
                var points = new Dictionary<string, ControlBaseInterface>(fixedPoints);
                foreach (var pair in selected) points[pair.Key] = pair.Value.Device;
                Points = points;
                Simulation = simulation;
            }

            /// <summary>初始化本次绑定的动态 IO 并检查所有选定驱动。仿真只解析配置。</summary>
            internal void InitializeDrivers()
            {
                if (Simulation) return;
                foreach (var pair in selected) InitializePoint(pair.Key, pair.Value);
                if (!HasMappedStopper) return;
                RequireStopperOutput(StopperRaise);
                RequireStopperOutput(StopperLower);
            }

            /// <summary>连接单个尚未就绪的 IO，与扫描和控件释放互斥。</summary>
            private static void InitializePoint(string signal, HardwareEntry entry)
            {
                ControlBaseInterface device = entry.Device;
                try
                {
                    lock (device)
                    {
                        if (IsReady(device)) return;
                        device.DisposeObj();
                        device.Initial();
                        if (IsReady(device)) return;
                    }
                }
                catch (Exception error) { throw NotReady(signal, entry.Definition.Name, error); }
                throw NotReady(signal, entry.Definition.Name);
            }

            /// <summary>使用动态挡停时，同时检查仍用固定点的另一侧输出。</summary>
            private void RequireStopperOutput(string signal)
            {
                var output = (NP_Output)Points[signal];
                if (output.IsReady) return;
                throw NotReady(signal, output.Text);
            }

            /// <summary>读取输入或输出的驱动就绪状态。</summary>
            private static bool IsReady(ControlBaseInterface device)
            {
                return device is NP_Input input ? input.IsReady : ((NP_Output)device).IsReady;
            }

            /// <summary>在初始化失败时报告工艺信号及选中的 IO 名称。</summary>
            private static InvalidOperationException NotReady(string signal, string ioName, Exception cause = null)
            {
                string message = string.Format(Text("notReady", "下料信号「{0}」对应的 IO「{1}」未就绪。",
                    "IO '{1}' for down conveyor signal '{0}' is not ready.",
                    "La E/S «{1}» de la señal de descarga «{0}» no está lista."), SignalName(signal), ioName);
                return new InvalidOperationException(cause == null ? message : message + Environment.NewLine + cause.Message, cause);
            }
        }
    }
}
