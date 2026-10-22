using System;
using AlaphaRap;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NPSDK;
using NPSDK.Motion;

namespace AlphaRap.FlowProgramming
{
    public sealed class FlowAction
    {
        public string Id;
        public string Category;
        public string Kind;
        public string TitleKey;
        public string[] Inputs;
        public string[] Outputs;
        /// <summary>按当前界面语言显示动作名称。</summary>
        public override string ToString() { return FlowText.T(TitleKey); }
    }

    public sealed partial class FlowActions
    {
        public readonly FlowResources Resources;
        public readonly List<FlowAction> Actions = new List<FlowAction>();
        public bool Simulate;
        public Func<string, CancellationToken, Task<string>> RequestInput;

        /// <summary>建立明确的动作目录，设备浏览不调用动作。</summary>
        public FlowActions(FlowResources resources)
        {
            Resources = resources;
            Add("value", "flow", "", "读取或传递数据", "Read or pass a value", "Leer o pasar un valor", "Value", "Value");
            Add("calculate", "flow", "", "数值计算", "Calculate values", "Calcular valores", "A,Operation,B", "Value");
            Add("counter", "flow", "", "循环索引", "Loop index", "Índice de ciclo", "Start,End,Step", "Index");
            Add("delay", "flow", "", "等待时间", "Delay", "Esperar", "Milliseconds", "ElapsedMs");
            Add("cycle.begin", "flow", "", "开始产品周期", "Begin product cycle", "Iniciar ciclo de producto", "Key", "CycleId");
            Add("cycle.end", "flow", "", "结束产品周期并释放结果", "Complete cycle and release results", "Completar ciclo y liberar resultados", "", "");
            Add("operator.input", "flow", "", "等待操作员输入", "Request operator input", "Solicitar entrada", "Prompt", "Value");
            Add("alarm.show", "flow", "", "触发报警", "Show alarm", "Mostrar alarma", "Code", "Code");
            Add("io.read", "io", "input", "读取输入", "Read input", "Leer entrada", "", "Value");
            Add("io.wait", "io", "input", "等待输入稳定", "Wait for stable input", "Esperar entrada estable", "Value,StableMs", "Value");
            Add("io.set", "io", "output", "设置输出", "Set output", "Establecer salida", "Value", "Value");
            Add("cylinder.set", "io", "cylinder", "气缸动作并等待到位", "Drive cylinder and wait", "Accionar cilindro y esperar", "Value", "Value");
            Add("point.move", "motion", "point", "移动到点位表索引", "Move to point table index", "Mover al índice de posición", "Index,Offset,Tolerance", "Index,Position");
            Add("tray.position", "motion", "tray", "计算托盘穴位偏移", "Calculate tray pocket offset", "Calcular desplazamiento de bandeja", "Index", "Row,Column,OffsetX,OffsetY");
            Add("axis.position", "motion", "axis", "读取轴位置", "Read axis position", "Leer posición del eje", "", "Position");
            Add("axis.stop", "motion", "axis", "停止轴", "Stop axis", "Detener eje", "", "Position");
            Add("vision.run", "vision", "vision", "运行 VPP 并获取输出", "Run VPP and read outputs", "Ejecutar VPP y leer salidas", "", "RunId,X,Y,Angle");
            Add("device.open", "device", "device", "连接设备", "Open device", "Conectar dispositivo", "", "Connected");
            Add("device.close", "device", "device", "断开设备", "Close device", "Desconectar dispositivo", "", "Connected");
            Add("device.send", "device", "device", "发送命令", "Send command", "Enviar comando", "Text", "Value");
            Add("device.receive", "device", "device", "读取接收数据", "Receive data", "Recibir datos", "", "Value");
            Add("scanner.once", "device", "device", "扫码一次", "Scan once", "Escanear una vez", "", "Barcode");
            Add("ateq.result", "device", "device", "读取气密结果", "Read leak test result", "Leer prueba de fugas", "", "Value");
            Add("hioki.result", "device", "device", "读取电阻结果", "Read resistance results", "Leer resultados de resistencia", "", "Valid,Values");
            Add("keyence.trigger", "device", "device", "触发 3D", "Trigger 3D", "Activar 3D", "Position", "Value");
            Add("keyence.change", "device", "device", "切换 3D 程序", "Change 3D program", "Cambiar programa 3D", "Position", "Value");
            Add("modbus.read", "device", "device", "读取 Modbus 寄存器", "Read Modbus registers", "Leer registros Modbus", "Slave,Start,Count,DelayMs", "Bytes,Hex");
            Add("plc.read", "device", "plc", "读取 PLC", "Read PLC", "Leer PLC", "Address,Count,Type", "Value");
            Add("plc.write", "device", "plc", "写入 PLC", "Write PLC", "Escribir PLC", "Address,Value,Type", "Value");
            Add("data.save", "data", "data", "保存 DataForm 生产记录", "Save DataForm production record", "Guardar registro de producción", "", "RecordId,Path");
        }

        /// <summary>登记一个动作及完整三语底稿。</summary>
        private void Add(string id, string category, string kind, string zh, string en, string es, string inputs, string outputs)
        {
            string key = "action." + id;
            if (!FlowText.Defaults.ContainsKey(key)) FlowText.Defaults.Add(key, new[] { zh, en, es });
            Actions.Add(new FlowAction { Id = id, Category = category, Kind = kind, TitleKey = key,
                Inputs = Split(inputs), Outputs = Split(outputs) });
        }

        /// <summary>拆分固定字段定义。</summary>
        private static string[] Split(string text) { return text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries); }

        /// <summary>按真实实例筛选具有对应协议适配的动作。</summary>
        public bool Supports(FlowAction action, FlowResource resource)
        {
            if (string.IsNullOrEmpty(action.Kind)) return resource == null;
            if (resource == null || resource.Kind != action.Kind) return false;
            if (action.Id == "scanner.once") return resource.Instance is ScannerKeyenceTcp;
            if (action.Id == "ateq.result") return resource.Instance is ATEQ_F620;
            if (action.Id == "hioki.result") return resource.Instance is HIOKI_RM3545;
            if (action.Id.StartsWith("keyence.")) return resource.Instance is Keyence3DTcp;
            if (action.Id == "modbus.read") return resource.Instance is ModBus_RTU;
            if (action.Id == "device.send" || action.Id == "device.receive") return ((AbstractDevice)resource.Instance).SupportsRawIo;
            return true;
        }

        /// <summary>读取动作真实字段，生产表字段来自 DataForm。</summary>
        public string[] InputNames(FlowAction action)
        {
            if (action.Id == "data.save" && MiddleLayer.DataF != null) return MiddleLayer.DataF.GetProductionFields();
            return action.Inputs;
        }

        /// <summary>列出 VPP 的实际输出以及设备的具名输出。</summary>
        public string[] OutputNames(FlowAction action, FlowResource resource)
        {
            VisionproInterface station = resource == null ? null : resource.Instance as VisionproInterface;
            if (action.Id == "vision.run" && station != null && station.TB != null)
                return action.Outputs.Concat(station.TB.Outputs.Cast<Cognex.VisionPro.ToolBlock.CogToolBlockTerminal>().Select(item => "TB:" + item.Name)).ToArray();
            HIOKI_RM3545 hioki = resource == null ? null : resource.Instance as HIOKI_RM3545;
            if (action.Id == "hioki.result" && hioki != null)
                return action.Outputs.Concat(Enumerable.Range(0, hioki.ChannelNum).Select(index => "Value" + index)).ToArray();
            return action.Outputs;
        }

        /// <summary>校验已配置目标和输入字段，未迁移旧回调不能执行。</summary>
        public void Validate(FlowStep step)
        {
            if (FlowProgramFile.RequiresBranchReview(step)) throw new InvalidOperationException(FlowText.T("reviewBranches") + " " + step.Caption);
            List<string> branchErrors = FlowProgramFile.ValidateBranches(step);
            if (branchErrors.Count > 0) throw new InvalidOperationException(string.Join(Environment.NewLine, branchErrors));
            FlowAction action = Actions.SingleOrDefault(item => item.Id == step.Action);
            if (action == null) throw new InvalidOperationException("Unconfigured action: " + step.Caption);
            FlowResource resource = FindResource(step.Target);
            if (!Supports(action, resource)) throw new InvalidOperationException("Missing or incompatible resource: " + step.Caption);
            foreach (string name in InputNames(action))
                if (step.Inputs.Count(item => item.Name == name) != 1) throw new InvalidOperationException("Missing input: " + step.Caption + "/" + name);
            foreach (FlowInput input in step.Inputs) ValidateInput(input, false, action, resource);
            foreach (FlowCondition condition in FlowProgramFile.BranchesOf(step).Where(branch => !branch.Unconditional).SelectMany(branch => branch.Conditions))
            {
                ValidateInput(condition.Left, true, action, resource);
                ValidateInput(condition.Right, true, action, resource);
            }
        }

        /// <summary>保存前检查引用目录及来源类别，当前动作结果只可用于动作后的条件。</summary>
        private void ValidateInput(FlowInput input, bool condition, FlowAction action, FlowResource resource)
        {
            if (input.Source == "Constant") return;
            if (input.Source == "Parameter" && Resources.Parameters.ContainsKey(input.Reference ?? "")) return;
            if (input.Source == "Context" && new[] { "CycleId", "RunId", "RecipeName", "ExecutionId" }.Contains(input.Reference)) return;
            if (input.Source == "Result" && condition && OutputNames(action, resource).Contains(input.Reference)) return;
            if (input.Source == "Output" && FlowPrograms.OutputChoices(this).Any(choice => choice.Id == input.Reference)) return;
            throw new InvalidOperationException("Missing or invalid input source: " + input.Name + "/" + input.Source + "/" + input.Reference);
        }

        /// <summary>解析资源身份；旧名称删除后明确报告，不替换为其他对象。</summary>
        public FlowResource FindResource(string id)
        {
            FlowResource resource;
            return id != null && Resources.Items.TryGetValue(id, out resource) ? resource : null;
        }

        /// <summary>解析输入一次并取得资源租约，整个底层动作退出后才释放。</summary>
        public async Task<IDictionary<string, object>> Execute(FlowStep step, FlowExecutionContext context, CancellationToken token)
        {
            Validate(step);
            Dictionary<string, object> input = step.Inputs.ToDictionary(item => item.Name, context.Resolve);
            FlowResource resource = FindResource(step.Target);
            object instance = resource == null ? null : resource.Instance;
            List<object> locks = ResourceLocks(instance);
            using (FlowResourceGate.Lease lease = await FlowResourceGate.Acquire(locks, token).ConfigureAwait(false))
                return await lease.Run(() => ExecuteCore(step, context, input, resource, token)).ConfigureAwait(false);
        }

        /// <summary>气缸占用实际输出，点位占用真实轴，相机同采集通道共用租约。</summary>
        private static List<object> ResourceLocks(object instance)
        {
            FlowPointAxis point = instance as FlowPointAxis;
            if (point != null) return new List<object> { point.Motor };
            NP_Cylinder cylinder = instance as NP_Cylinder;
            if (cylinder != null) return new object[] { cylinder, cylinder.CyOn_OB1, cylinder.CyOn_OB2, cylinder.CyOff_OB1, cylinder.CyOff_OB2 }.Where(item => item != null).ToList();
            VpStation station = instance as VpStation;
            if (station != null) return new List<object> { station, string.Intern("camera:" + station.CameraName) };
            return instance == null ? new List<object>() : new List<object> { instance };
        }

        /// <summary>读取有限数值，所有区域设置使用相同的持久化数值格式。</summary>
        internal static double Number(object value)
        {
            double number = Convert.ToDouble(value, CultureInfo.InvariantCulture);
            if (double.IsNaN(number) || double.IsInfinity(number)) throw new InvalidOperationException("A finite number is required.");
            return number;
        }

        /// <summary>读取整数，禁止将小数或越界值当作点位索引。</summary>
        internal static int Integer(object value)
        {
            double number = Number(value);
            if (number != Math.Truncate(number) || number < int.MinValue || number > int.MaxValue) throw new InvalidOperationException("An integer is required.");
            return (int)number;
        }

        /// <summary>解析明确的布尔值。</summary>
        internal static bool Boolean(object value)
        {
            string text = Convert.ToString(value, CultureInfo.InvariantCulture);
            if (text == "1") return true;
            if (text == "0") return false;
            return Convert.ToBoolean(value, CultureInfo.InvariantCulture);
        }

        /// <summary>创建具名输出。</summary>
        private static IDictionary<string, object> Result(string name, object value) { return new Dictionary<string, object> { { name, value } }; }
    }
}
