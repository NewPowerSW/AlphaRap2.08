using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace NPSDK
{
    public sealed class FlowProject
    {
        public int Version = 4;
        public List<FlowProgram> Modules = new List<FlowProgram>();
    }

    public sealed class FlowProgram
    {
        public string ModuleId;
        public string Mode = "Legacy";
        public bool Removed;
        public List<string> InitialEntries = new List<string>();
        public List<string> AutoEntries = new List<string>();
        public string InitialEntry
        {
            get { return InitialEntries.FirstOrDefault(); }
            set { InitialEntries = string.IsNullOrEmpty(value) ? new List<string>() : new List<string> { value }; }
        }
        public string AutoEntry
        {
            get { return AutoEntries.FirstOrDefault(); }
            set { AutoEntries = string.IsNullOrEmpty(value) ? new List<string>() : new List<string> { value }; }
        }
        public List<FlowStep> Steps = new List<FlowStep>();
        public List<FlowPageDefinition> Pages = new List<FlowPageDefinition>();
        public List<string> RemovedControls = new List<string>();

        /// <summary>仅托管流程页保存独立页面定义，历史工程保持原布局格式。</summary>
        public bool ShouldSerializePages() { return Pages.Count > 0; }

        /// <summary>旧单入口仅用于读取历史工程，新工程保存完整入口集合。</summary>
        public bool ShouldSerializeInitialEntry() { return false; }
        /// <summary>旧自动入口仅用于读取历史工程，新工程保存完整入口集合。</summary>
        public bool ShouldSerializeAutoEntry() { return false; }
    }

    public sealed class FlowPageDefinition
    {
        public string Id = Guid.NewGuid().ToString("N");
        public string Key;
        public string Caption;
        public bool Initializing;
    }

    public sealed class FlowStep
    {
        public string Id = Guid.NewGuid().ToString("N");
        public string ControlName;
        public string ControlOrigin;
        public FlowControlLinks SourceLinks;
        public string ParentPath;
        public string PageId;
        public string Caption;
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public string ExecutionMode = "Action";
        public bool UsesFlowResult;
        public string Action = "legacy";
        public string Target;
        public string CodeModuleId;
        public int CodeModuleVersion;
        public List<FlowResourceBinding> ResourceBindings = new List<FlowResourceBinding>();
        public string ScriptSource = "";
        public bool Breakpoint;
        public List<FlowScriptPort> ScriptInputs = new List<FlowScriptPort>();
        public List<FlowScriptPort> ScriptOutputs = new List<FlowScriptPort>();
        public List<string> ScriptResources = new List<string>();
        public int TimeoutMs = 10000;
        public string AlarmCode;
        public bool WaitForCondition;
        public bool AnyCondition;
        public string Next;
        public string Case1;
        public string Case2;
        public string Case3;
        public string Otherwise { get { return Case1; } set { Case1 = value; } }
        public string Failure { get { return Case2; } set { Case2 = value; } }
        public string Third { get { return Case3; } set { Case3 = value; } }
        public List<FlowBranch> Branches = new List<FlowBranch>();
        public List<FlowInput> Inputs = new List<FlowInput>();
        public List<FlowCondition> Conditions = new List<FlowCondition>();

        /// <summary>托管节点按稳定页面身份定位，控件路径仅为兼容投影。</summary>
        public bool ShouldSerializePageId() { return !string.IsNullOrEmpty(PageId); }
        /// <summary>文档布局记录实际节点尺寸，历史节点保留默认外观。</summary>
        public bool ShouldSerializeWidth() { return Width > 0; }
        /// <summary>仅显式布局记录高度。</summary>
        public bool ShouldSerializeHeight() { return Height > 0; }

        /// <summary>仅已识别来源的节点保存来源标记，旧文件可继续读取。</summary>
        public bool ShouldSerializeControlOrigin() { return !string.IsNullOrEmpty(ControlOrigin); }
        /// <summary>源码节点保存编译时连接基准，拖拽节点不写源码连接。</summary>
        public bool ShouldSerializeSourceLinks() { return SourceLinks != null && ControlOrigin != "Designer"; }

        /// <summary>对象动作沿用旧格式，其他执行方式明确写入工程。</summary>
        public bool ShouldSerializeExecutionMode() { return ExecutionMode != "Action"; }
        /// <summary>仅原生事件动作保存返回值路由标记，普通动作保留原有文件格式。</summary>
        public bool ShouldSerializeUsesFlowResult() { return UsesFlowResult; }
        /// <summary>仅代码模块写入定义身份。</summary>
        public bool ShouldSerializeCodeModuleId() { return !string.IsNullOrEmpty(CodeModuleId); }
        /// <summary>仅代码模块写入契约版本。</summary>
        public bool ShouldSerializeCodeModuleVersion() { return !string.IsNullOrEmpty(CodeModuleId); }
        /// <summary>仅保存明确配置的资源角色绑定。</summary>
        public bool ShouldSerializeResourceBindings() { return ResourceBindings != null && ResourceBindings.Count > 0; }
        /// <summary>仅脚本节点持久化脚本文本。</summary>
        public bool ShouldSerializeScriptSource() { return ExecutionMode == "Script" && !string.IsNullOrEmpty(ScriptSource); }
        /// <summary>仅配置断点的节点写入断点标记。</summary>
        public bool ShouldSerializeBreakpoint() { return Breakpoint; }
        /// <summary>仅脚本节点持久化已声明的输入端口。</summary>
        public bool ShouldSerializeScriptInputs() { return ExecutionMode == "Script" && ScriptInputs != null && ScriptInputs.Count > 0; }
        /// <summary>仅脚本节点持久化已声明的输出端口。</summary>
        public bool ShouldSerializeScriptOutputs() { return ExecutionMode == "Script" && ScriptOutputs != null && ScriptOutputs.Count > 0; }
        /// <summary>仅脚本节点保存明确授权调用的真实对象身份。</summary>
        public bool ShouldSerializeScriptResources() { return ExecutionMode == "Script" && ScriptResources != null && ScriptResources.Count > 0; }
        /// <summary>旧格式出口可读取，保存只输出原生 CASE 名称。</summary>
        public bool ShouldSerializeOtherwise() { return false; }
        /// <summary>旧异常出口可读取，保存只输出 CASE2 名称。</summary>
        public bool ShouldSerializeFailure() { return false; }
        /// <summary>旧第三出口可读取，保存只输出 CASE3 名称。</summary>
        public bool ShouldSerializeThird() { return false; }
        /// <summary>新版独立条件存在时不再写旧版公共条件。</summary>
        public bool ShouldSerializeConditions() { return Branches.Count == 0 && Conditions.Count > 0; }
        /// <summary>新版出口等待规则不使用旧开关。</summary>
        public bool ShouldSerializeWaitForCondition() { return Branches.Count == 0 && WaitForCondition; }
        /// <summary>新版 AND 或 OR 由各自出口保存。</summary>
        public bool ShouldSerializeAnyCondition() { return Branches.Count == 0 && AnyCondition; }

        /// <summary>按 SDK 原生出口读取连接目标。</summary>
        public string TargetOf(string code)
        {
            switch (code) { case "NEXT": return Next; case "CASE1": return Case1; case "CASE2": return Case2; case "CASE3": return Case3; default: throw new InvalidDataException("Unknown exit: " + code); }
        }

        /// <summary>按 SDK 原生出口设置连接目标。</summary>
        public void SetTarget(string code, string target)
        {
            switch (code) { case "NEXT": Next = target; return; case "CASE1": Case1 = target; return; case "CASE2": Case2 = target; return; case "CASE3": Case3 = target; return; default: throw new InvalidDataException("Unknown exit: " + code); }
        }
    }

    /// <summary>源码控件的四个连接目标，使用控件名称而非持久化节点 ID。</summary>
    public sealed class FlowControlLinks
    {
        public string Next;
        public string Case1;
        public string Case2;
        public string Case3;

        /// <summary>读取源码控件指定出口的目标名称。</summary>
        public string TargetOf(string code)
        {
            switch (code) { case "NEXT": return Next; case "CASE1": return Case1; case "CASE2": return Case2; case "CASE3": return Case3; default: throw new InvalidDataException("Unknown exit: " + code); }
        }
    }

    /// <summary>脚本公开的具名输入或输出；类型代码不随界面语言变化。</summary>
    public sealed class FlowScriptPort
    {
        public string Name;
        public string Type = "Number";
        public string DefaultValue = "";
    }

    public sealed class FlowBranch
    {
        public string Code = "NEXT";
        public bool Unconditional;
        public bool AnyCondition;
        public List<FlowCondition> Conditions = new List<FlowCondition>();

        /// <summary>新节点默认只有 NEXT，不预建其他 CASE。</summary>
        public static List<FlowBranch> Default() { return new List<FlowBranch> { new FlowBranch { Unconditional = true } }; }
    }

    public sealed class FlowInput
    {
        public string Name;
        public string Source = "Constant";
        public string Reference;
    }

    public sealed class FlowCondition
    {
        public FlowInput Left = new FlowInput();
        public string Operator = "==";
        public FlowInput Right = new FlowInput();
    }

    public static class FlowProgramFile
    {
        /// <summary>读取版本化工程，损坏文件直接报告，禁止覆盖为空工程。</summary>
        public static FlowProject Load(string path)
        {
            if (!File.Exists(path)) return new FlowProject();
            using (FileStream stream = File.OpenRead(path))
            {
                FlowProject project = (FlowProject)new XmlSerializer(typeof(FlowProject)).Deserialize(stream);
                if (project.Version < 1 || project.Version > 5) throw new InvalidDataException("Unsupported flow project version.");
                return project;
            }
        }

        /// <summary>先生成完整临时文件，再替换工程并保留上一次备份。</summary>
        public static void Save(string path, FlowProject project)
        {
            if (project.Version < 1 || project.Version > 5) throw new InvalidDataException("Unsupported flow project version.");
            string directory = Path.GetDirectoryName(Path.GetFullPath(path));
            Directory.CreateDirectory(directory);
            string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                int version = project.Modules.Any(module => module.Pages.Count > 0) ? 5 : 4;
                FlowProject saved = new FlowProject { Version = version, Modules = project.Modules };
                using (FileStream stream = File.Create(temporary)) new XmlSerializer(typeof(FlowProject)).Serialize(stream, saved);
                Load(temporary);
                if (File.Exists(path)) File.Replace(temporary, path, path + ".bak");
                else File.Move(temporary, path);
                project.Version = version;
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }

        /// <summary>复制运行定义，编辑草稿不会改变正在执行的节点。</summary>
        public static FlowProgram Snapshot(FlowProgram program)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                XmlSerializer serializer = new XmlSerializer(typeof(FlowProgram));
                serializer.Serialize(stream, program);
                stream.Position = 0;
                return (FlowProgram)serializer.Deserialize(stream);
            }
        }

        /// <summary>检查节点身份、入口和连接，返回带节点身份的问题。</summary>
        public static List<string> Validate(FlowProgram program)
        {
            List<string> errors = new List<string>();
            HashSet<string> pageIds = new HashSet<string>(StringComparer.Ordinal);
            HashSet<string> pageKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (FlowPageDefinition page in program.Pages)
            {
                if (string.IsNullOrWhiteSpace(page.Id) || !pageIds.Add(page.Id)) errors.Add("Duplicate or empty flow page ID.");
                if (string.IsNullOrWhiteSpace(page.Key) || !pageKeys.Add(page.Key)) errors.Add("Duplicate or empty flow page key.");
            }
            if (program.Removed && (program.Mode != "Disabled" || program.Steps.Count > 0 || !string.IsNullOrEmpty(program.InitialEntry) || !string.IsNullOrEmpty(program.AutoEntry)))
                errors.Add("A removed module must be disabled and contain no nodes or entries: " + program.ModuleId);
            HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (FlowStep step in program.Steps)
            {
                if (!string.IsNullOrEmpty(step.PageId) && !pageIds.Contains(step.PageId)) errors.Add("Missing flow page: " + step.Caption);
                if (string.IsNullOrWhiteSpace(step.Id) || !ids.Add(step.Id)) errors.Add("Duplicate or empty node ID: " + step.Caption);
                if (step.TimeoutMs < 1 && !(step.UsesFlowResult && step.TimeoutMs == System.Threading.Timeout.Infinite))
                    errors.Add("Invalid timeout: " + step.Caption);
                errors.AddRange(ValidateExecution(step));
                if (step.Branches.Count > 0) errors.AddRange(ValidateBranches(step));
            }
            foreach (string entry in program.InitialEntries.Concat(program.AutoEntries))
                if (!string.IsNullOrEmpty(entry) && !ids.Contains(entry)) errors.Add("Missing entry: " + entry);
            if (ids.Count == program.Steps.Count && !ids.Contains(null) && !ids.Contains(""))
            {
                errors.AddRange(ValidateEntries(program, program.InitialEntries));
                errors.AddRange(ValidateEntries(program, program.AutoEntries));
            }
            foreach (FlowStep step in program.Steps)
                foreach (string target in new[] { step.Next, step.Case1, step.Case2, step.Case3 })
                    if (!string.IsNullOrEmpty(target) && !ids.Contains(target)) errors.Add("Missing target: " + step.Caption);
            return errors;
        }

        /// <summary>并行入口必须有效且彼此独立，防止同一节点被两条流程重复执行或覆盖输出。</summary>
        public static List<string> ValidateEntries(FlowProgram program, IEnumerable<string> entries)
        {
            List<string> errors = new List<string>();
            HashSet<string> selected = new HashSet<string>(StringComparer.Ordinal);
            HashSet<string> occupied = new HashSet<string>(StringComparer.Ordinal);
            foreach (string entry in entries)
            {
                if (string.IsNullOrWhiteSpace(entry) || !selected.Add(entry)) { errors.Add("Empty or duplicate flow entry: " + entry); continue; }
                if (program.Steps.All(step => step.Id != entry)) continue;
                List<FlowStep> reachable;
                try { reachable = Reachable(program, entry); }
                catch (InvalidDataException error) { errors.Add(error.Message); continue; }
                foreach (FlowStep step in reachable)
                    if (!occupied.Add(step.Id)) errors.Add("Parallel flows share a node: " + step.Caption);
            }
            return errors;
        }

        /// <summary>校验节点执行方式及脚本端口；脚本编译与默认值转换由宿主处理。</summary>
        public static List<string> ValidateExecution(FlowStep step)
        {
            List<string> errors = new List<string>();
            if (step.ExecutionMode != "Action" && step.ExecutionMode != "Script" && step.ExecutionMode != "None")
                errors.Add("Unknown node execution mode: " + step.Caption + " / " + step.ExecutionMode);
            if (step.UsesFlowResult && (step.ExecutionMode != "Action" || !string.IsNullOrEmpty(step.CodeModuleId)))
                errors.Add("Native flow results require an ordinary Action node: " + step.Caption);
            if (step.UsesFlowResult && (step.Conditions.Count > 0 || step.AnyCondition || step.WaitForCondition))
                errors.Add("Native flow results cannot contain conditional exit settings: " + step.Caption);
            errors.AddRange(ValidateResourceBindings(step));
            if (step.ExecutionMode != "Script") return errors;
            if (string.IsNullOrWhiteSpace(step.ScriptSource)) errors.Add("Script source is required: " + step.Caption);
            errors.AddRange(ValidatePorts(step.ScriptInputs, "input", step.Caption));
            errors.AddRange(ValidatePorts(step.ScriptOutputs, "output", step.Caption));
            if (step.ScriptResources == null) errors.Add("Script resources are missing: " + step.Caption);
            else if (step.ScriptResources.Any(string.IsNullOrWhiteSpace) || step.ScriptResources.Distinct(StringComparer.Ordinal).Count() != step.ScriptResources.Count)
                errors.Add("Empty or duplicate script resource identity: " + step.Caption);
            return errors;
        }

        /// <summary>校验持久化角色和端口映射的结构；实际对象与模块契约由宿主校验。</summary>
        private static IEnumerable<string> ValidateResourceBindings(FlowStep step)
        {
            List<string> errors = new List<string>();
            bool code = !string.IsNullOrEmpty(step.CodeModuleId);
            if (code && (string.IsNullOrWhiteSpace(step.CodeModuleId) || step.ExecutionMode != "Action" || step.CodeModuleVersion < 1))
                errors.Add("Invalid code module identity or version: " + step.Caption);
            if (code && step.Conditions.Count > 0) errors.Add("Code module exits cannot contain conditions: " + step.Caption);
            if (!code && step.CodeModuleVersion != 0) errors.Add("Code module identity is required: " + step.Caption);
            if (step.ResourceBindings == null) { errors.Add("Resource bindings are missing: " + step.Caption); return errors; }
            if (!code && step.ResourceBindings.Count > 0) errors.Add("Resource roles require a code module: " + step.Caption);
            HashSet<string> roles = new HashSet<string>(StringComparer.Ordinal);
            foreach (FlowResourceBinding binding in step.ResourceBindings)
            {
                if (binding == null || string.IsNullOrWhiteSpace(binding.Slot) || !roles.Add(binding.Slot))
                { errors.Add("Empty or duplicate resource role: " + step.Caption); continue; }
                if (binding.Fields == null) { errors.Add("Resource field mappings are missing: " + binding.Slot); continue; }
                HashSet<string> fields = new HashSet<string>(StringComparer.Ordinal);
                foreach (FlowResourceFieldBinding field in binding.Fields)
                    if (field == null || string.IsNullOrWhiteSpace(field.Name) || string.IsNullOrWhiteSpace(field.Field)
                        || (field.Direction != "Input" && field.Direction != "Output") || !fields.Add(field.Direction + "/" + field.Name))
                        errors.Add("Invalid resource field mapping: " + step.Caption + " / " + binding.Slot);
            }
            return errors;
        }

        /// <summary>拒绝空端口、重复名称和未支持的脚本值类型。</summary>
        private static IEnumerable<string> ValidatePorts(List<FlowScriptPort> ports, string direction, string caption)
        {
            if (ports == null) return new[] { "Script " + direction + " ports are missing: " + caption };
            List<string> errors = new List<string>();
            HashSet<string> names = new HashSet<string>(StringComparer.Ordinal);
            foreach (FlowScriptPort port in ports)
            {
                if (port == null) { errors.Add("Empty script " + direction + " port: " + caption); continue; }
                if (string.IsNullOrWhiteSpace(port.Name) || !names.Add(port.Name))
                    errors.Add("Empty or duplicate script " + direction + " port: " + caption + " / " + port.Name);
                if (port.Type != "Boolean" && port.Type != "Integer" && port.Type != "Number" && port.Type != "Text")
                    errors.Add("Unknown script port type: " + caption + " / " + port.Name + " / " + port.Type);
            }
            return errors;
        }

        /// <summary>检查选定入口的全部可达节点，未迁移的旧回调禁止混入新运行计划。</summary>
        public static List<FlowStep> Reachable(FlowProgram program, string entry)
        {
            Dictionary<string, FlowStep> nodes = program.Steps.ToDictionary(step => step.Id);
            List<FlowStep> result = new List<FlowStep>();
            Queue<string> pending = new Queue<string>();
            HashSet<string> visited = new HashSet<string>();
            if (!string.IsNullOrEmpty(entry)) pending.Enqueue(entry);
            while (pending.Count > 0)
            {
                string id = pending.Dequeue();
                if (!visited.Add(id)) continue;
                FlowStep step;
                if (!nodes.TryGetValue(id, out step)) throw new InvalidDataException("Missing node: " + id);
                result.Add(step);
                foreach (string next in BranchesOf(step).Select(branch => step.TargetOf(branch.Code)))
                    if (!string.IsNullOrEmpty(next)) pending.Enqueue(next);
            }
            return result;
        }

        /// <summary>旧条件模型必须人工复核，防止把异常出口当作 CASE2 条件运行。</summary>
        public static bool RequiresBranchReview(FlowStep step)
        {
            if (step.UsesFlowResult || !string.IsNullOrEmpty(step.CodeModuleId)) return false;
            return step.Action != "legacy" && step.Branches.Count == 0 && (step.Conditions.Count > 0 || !string.IsNullOrEmpty(step.Case1) || !string.IsNullOrEmpty(step.Case2) || !string.IsNullOrEmpty(step.Case3));
        }

        /// <summary>读取按 NEXT、CASE1、CASE2、CASE3 排序的出口，保留旧布局供人工配置。</summary>
        public static List<FlowBranch> BranchesOf(FlowStep step)
        {
            string[] codes = { "NEXT", "CASE1", "CASE2", "CASE3" };
            if (step.Branches.Count > 0) return step.Branches.OrderBy(branch => Array.IndexOf(codes, branch.Code)).ToList();
            List<FlowBranch> result = new List<FlowBranch> { new FlowBranch { Code = "NEXT", Unconditional = step.Conditions.Count == 0, AnyCondition = step.AnyCondition, Conditions = new List<FlowCondition>(step.Conditions) } };
            foreach (string code in codes.Skip(1))
                if (!string.IsNullOrEmpty(step.TargetOf(code))) result.Add(new FlowBranch { Code = code });
            return result;
        }

        /// <summary>校验每条出口的独立条件，拒绝被前面无条件出口遮蔽的分支。</summary>
        public static List<string> ValidateBranches(FlowStep step)
        {
            List<string> errors = new List<string>();
            string[] codes = { "NEXT", "CASE1", "CASE2", "CASE3" };
            List<FlowBranch> branches = BranchesOf(step);
            if (branches.Count(branch => branch.Code == "NEXT") != 1 || branches.GroupBy(branch => branch.Code).Any(group => group.Count() != 1)) errors.Add("NEXT is required and exit codes must be unique: " + step.Caption);
            for (int i = 0; i < branches.Count; i++)
            {
                FlowBranch branch = branches[i];
                if (!codes.Contains(branch.Code)) errors.Add("Unknown exit: " + branch.Code);
                if (step.UsesFlowResult)
                {
                    if (branch.Conditions.Count > 0 || branch.AnyCondition) errors.Add("Native flow result exits cannot contain conditions: " + step.Caption);
                    continue;
                }
                if (!string.IsNullOrEmpty(step.CodeModuleId))
                {
                    if (branch.Conditions.Count > 0 || branch.AnyCondition) errors.Add("Code module exits cannot contain conditions: " + step.Caption);
                    continue;
                }
                if (!branch.Unconditional && branch.Conditions.Count == 0) errors.Add("Configure conditions for " + branch.Code + ": " + step.Caption);
                if (branch.Unconditional && i < branches.Count - 1) errors.Add("Unconditional exit hides later exits: " + branch.Code);
            }
            return errors;
        }

        /// <summary>按明确运算符比较数值或原始文本，不执行脚本。</summary>
        public static bool Compare(object left, string operation, object right)
        {
            string a = Convert.ToString(left, CultureInfo.InvariantCulture) ?? "";
            string b = Convert.ToString(right, CultureInfo.InvariantCulture) ?? "";
            if (operation == "Contains") return a.Contains(b);
            if (operation == "Empty") return string.IsNullOrEmpty(a);
            int order;
            if (left is bool || right is bool) order = BooleanOperand(left).CompareTo(BooleanOperand(right));
            else order = NumericOrTextOrder(a, b);
            switch (operation)
            {
                case "==": return order == 0;
                case "!=": return order != 0;
                case ">": return order > 0;
                case ">=": return order >= 0;
                case "<": return order < 0;
                case "<=": return order <= 0;
                default: throw new InvalidOperationException("Unknown condition operator: " + operation);
            }
        }

        /// <summary>整数文本先精确比较，避免 Int64 邻值被 Double 舍入；其余沿用有限浮点或原始文本比较。</summary>
        private static int NumericOrTextOrder(string left, string right)
        {
            decimal integerLeft, integerRight;
            if (decimal.TryParse(left, NumberStyles.Integer, CultureInfo.InvariantCulture, out integerLeft)
                && decimal.TryParse(right, NumberStyles.Integer, CultureInfo.InvariantCulture, out integerRight))
                return integerLeft.CompareTo(integerRight);
            double x, y;
            if (!double.TryParse(left, NumberStyles.Float, CultureInfo.InvariantCulture, out x)
                || !double.TryParse(right, NumberStyles.Float, CultureInfo.InvariantCulture, out y)) return string.CompareOrdinal(left, right);
            if (double.IsNaN(x) || double.IsInfinity(x) || double.IsNaN(y) || double.IsInfinity(y))
                throw new InvalidOperationException("Condition requires finite numbers.");
            return x.CompareTo(y);
        }

        /// <summary>按实际布尔来源解析条件另一侧，接受 true、false、0、1，拒绝其他值。</summary>
        private static bool BooleanOperand(object value)
        {
            if (value is bool) return (bool)value;
            string text = Convert.ToString(value, CultureInfo.InvariantCulture) ?? "";
            if (text == "1") return true;
            if (text == "0") return false;
            bool parsed;
            if (bool.TryParse(text, out parsed)) return parsed;
            throw new InvalidOperationException("Condition requires a Boolean value: " + text);
        }
    }
}
