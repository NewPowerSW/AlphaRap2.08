using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    public static class FlowPrograms
    {
        private static readonly Dictionary<Flow_BaseForm, FlowModuleEditor> modules = new Dictionary<Flow_BaseForm, FlowModuleEditor>();
        private static FlowProject project = new FlowProject();
        private static Exception loadError;
        private static bool installed;
        public static readonly FlowResultHub Results = new FlowResultHub();
        public static bool DebugEnabled { get; set; }
        public static bool AnyRunning { get { return modules.Values.Any(module => module.IsRunning) || Flow_Module.Module_GetRunningStatus; } }
        public static bool AnyEditing { get { return modules.Values.Any(module => module.IsEditing); } }
        public static bool HasPrograms { get { return modules.Count > 0; } }
        public static bool IsInitialized { get { return HasPrograms && modules.Values.All(module => module.IsEnabled && module.Form.InitialStatus); } }
        public static bool HasInitializationSession { get { return modules.Values.Any(module => module.IsInitializing); } }
        public static bool CanContinueInitialization
        {
            get
            {
                bool ready = false;
                foreach (FlowModuleEditor module in modules.Values)
                {
                    if (!module.IsInitializing) continue;
                    if (module.Runtime.IsPaused && !module.Runtime.CanResume) return false;
                    ready |= module.Runtime.CanResume;
                }
                return ready;
            }
        }
        public static bool CanEdit { get { return SysPara.UserPermission == PermissionType.Administrator || SysPara.UserPermission == PermissionType.Maintenance; } }
        public static IEnumerable<FlowModuleEditor> Modules { get { return modules.Values.ToArray(); } }

        public static string FilePath
        {
            get { return Path.Combine(string.IsNullOrEmpty(SysPara.SettingDataDirectory) ? @".\ModuleData\SettingData" : SysPara.SettingDataDirectory, "FlowPrograms.xml"); }
        }

        /// <summary>为所有现有及未来的 Flow_BaseForm 派生窗体安装统一工厂。</summary>
        public static void Install()
        {
            if (installed) return;
            try { project = FlowProgramFile.Load(FilePath); }
            catch (Exception error) { loadError = error; }
            installed = true;
            Flow_Module.ProgramControllerFactory = form => Attach(form);
            foreach (FlowProgram module in project.Modules.ToArray())
            {
                string prefix = typeof(Flow_BaseForm).FullName + "/";
                if (!module.ModuleId.StartsWith(prefix, StringComparison.Ordinal)) continue;
                string name = module.ModuleId.Substring(prefix.Length);
                if (Flow_Module.FlowChart_ModuleList.Any(form => form.GetType() == typeof(Flow_BaseForm) && form.Name == name)) continue;
                Flow_BaseForm restoredModule = new Flow_BaseForm { Name = name, Text = name };
            }
            foreach (Flow_BaseForm form in Flow_Module.FlowChart_ModuleList.ToArray()) form.EnsureProgramController();
        }

        /// <summary>在 UI 线程创建通用控制器，不检查具体工位类名。</summary>
        public static FlowModuleEditor Attach(Flow_BaseForm form)
        {
            if (MiddleLayer.MainF != null && MiddleLayer.MainF.IsHandleCreated && MiddleLayer.MainF.InvokeRequired)
                return (FlowModuleEditor)MiddleLayer.MainF.Invoke(new Func<FlowModuleEditor>(() => Attach(form)));
            FlowModuleEditor existing;
            if (modules.TryGetValue(form, out existing)) return existing;
            string id = form.GetType().FullName + "/" + form.Name;
            FlowProgram definition = project.Modules.SingleOrDefault(item => item.ModuleId == id);
            if (definition == null) { definition = new FlowProgram { ModuleId = id }; project.Modules.Add(definition); }
            if (modules.Values.Any(item => item.Program.ModuleId == id)) throw new InvalidDataException("Duplicate flow module identity: " + id);
            FlowModuleEditor editor = new FlowModuleEditor(form, definition);
            modules.Add(form, editor);
            if (form.ProgramController == null) form.ProgramController = editor;
            form.Disposed += (sender, args) => { editor.Runtime.Stop(); modules.Remove(form); Flow_Module.FlowChart_ModuleList.Remove(form); };
            return editor;
        }

        /// <summary>运行和修改前统一检查工位、机器状态及权限。</summary>
        public static void RequireIdle()
        {
            if (AnyRunning || SysPara.SystemMode != RunMode.IDLE)
            {
                string names = string.Join(", ", Flow_Module.FlowChart_ModuleList.Where(Flow_Module.IsExecutionActive).Select(form => form.Name).Distinct());
                string legacy = string.Join(", ", Flow_Module.FlowChart_List.Where(node => node.IsLegacyRunning).Select(node => node.text).Distinct());
                if (legacy.Length > 0) names = names.Length == 0 ? legacy : names + ", " + legacy;
                throw new InvalidOperationException(FlowText.T("busy") + (names.Length == 0 ? "" : Environment.NewLine + names));
            }
            if (!CanEdit) throw new InvalidOperationException(FlowText.T("permission"));
        }

        /// <summary>保存完整工程；加载失败时禁止将损坏文件覆盖为空工程。</summary>
        public static void Save()
        {
            WriteDraft(FilePath);
            PublishSaved();
        }

        /// <summary>将完整草稿写入指定暂存文件，统一保存成功前不发布。</summary>
        public static void WriteDraft(string path)
        {
            ValidateForSave();
            FlowProgramFile.Save(path, project);
        }

        /// <summary>捕获本次主保存使用的完整独立工程，源码与 XML 使用同一快照。</summary>
        internal static FlowProject CaptureForSave()
        {
            ValidateForSave();
            return new FlowProject { Version = 3, Modules = project.Modules.Select(FlowProgramFile.Snapshot).ToList() };
        }

        /// <summary>所有文件成功提交后发布同一保存快照，不重新采样未保存的编辑值。</summary>
        public static void PublishSaved(FlowProject snapshot)
        {
            foreach (FlowModuleEditor editor in modules.Values)
            {
                FlowProgram saved = snapshot.Modules.Single(item => item.ModuleId == editor.Program.ModuleId);
                editor.Publish(saved);
            }
            project.Version = snapshot.Version;
            OperationLog.Write("Flow", "Save flow project: " + FilePath);
        }

        /// <summary>主保存全部落盘成功后发布流程运行版本。</summary>
        public static void PublishSaved()
        {
            foreach (FlowModuleEditor editor in modules.Values) editor.Publish();
            OperationLog.Write("Flow", "Save flow project: " + FilePath);
        }

        /// <summary>主保存落盘前校验工程草稿及当前参数，不发布运行版本。</summary>
        public static void ValidateForSave()
        {
            RequireIdle();
            RequireLoaded();
            foreach (FlowModuleEditor editor in modules.Values) { editor.CaptureLayout(); editor.Validate(null); }
        }

        /// <summary>主界面放弃修改时恢复上次保存；首次保存前恢复各模块的初始快照。</summary>
        public static void DiscardChanges()
        {
            if (System.IO.File.Exists(FilePath)) { Reload(); return; }
            foreach (FlowModuleEditor editor in modules.Values) editor.RestorePublished();
            project.Modules = modules.Values.Select(editor=>editor.Program).ToList();
        }

        /// <summary>首次加载失败时禁止保存或生产执行，修复后重新加载才能恢复。</summary>
        internal static void RequireLoaded()
        {
            if (loadError != null) throw new InvalidDataException(FlowText.T("projectReloadRequired"), loadError);
        }

        /// <summary>先完整读取并验证，再替换内存工程；失败时保留当前设计。</summary>
        public static void Reload()
        {
            RequireIdle();
            FlowProject candidate = FlowProgramFile.Load(FilePath);
            if (candidate.Modules.GroupBy(module => module.ModuleId).Any(group => group.Count() != 1)) throw new InvalidDataException("Duplicate module ID.");
            foreach (FlowProgram module in candidate.Modules)
            {
                List<string> errors = FlowProgramFile.Validate(module);
                if (errors.Count > 0) throw new InvalidDataException(string.Join(Environment.NewLine, errors));
            }
            foreach (FlowModuleEditor editor in modules.Values)
            {
                FlowProgram definition = candidate.Modules.SingleOrDefault(item => item.ModuleId == editor.Program.ModuleId);
                if (definition != null) editor.PrepareRestore(definition);
            }
            foreach (FlowModuleEditor editor in modules.Values)
            {
                FlowProgram definition = candidate.Modules.SingleOrDefault(item => item.ModuleId == editor.Program.ModuleId);
                if (definition == null) { definition = new FlowProgram { ModuleId = editor.Program.ModuleId }; candidate.Modules.Add(definition); }
                editor.Restore(definition);
                editor.Publish();
            }
            project = candidate;
            loadError = null;
        }

        /// <summary>为新的测试或设备运行失效旧结果；停止未完成时不能开始下一批。</summary>
        public static void BeginBatch()
        {
            if (AnyRunning) throw new InvalidOperationException(FlowText.T("busy"));
            Results.Reset();
        }

        /// <summary>取消全部工程会话，底层结束前保留占用和运行状态。</summary>
        public static void StopAll() { foreach (FlowModuleEditor module in modules.Values) module.Runtime.Stop(); }

        /// <summary>恢复当前初始化会话及故障节点，保留已完成工位、运行结果和参数快照。</summary>
        public static void ContinueInitialization()
        {
            if (DebugEnabled && !CanEdit) throw new InvalidOperationException(FlowText.T("permission"));
            if (Alarm.IsError) throw new InvalidOperationException(FlowText.T("clearAlarmBeforeContinue"));
            if (!CanContinueInitialization) throw new InvalidOperationException(FlowText.T("busy"));
            foreach (FlowModuleEditor module in modules.Values)
                if (module.IsInitializing || (!module.Runtime.IsRunning && module.Form.InitialStatus)) module.Handle(FlowProgramCommand.Resume);
            Flow_Module.AllPause = false;
            SysPara.SystemRun = false;
            SysPara.SystemMode = RunMode.INITIAL;
        }

        /// <summary>在运行时创建普通基类流程模块，工程师无需编写新的 Form 类。</summary>
        public static Flow_BaseForm AddModule(string name)
        {
            RequireIdle();
            if (string.IsNullOrWhiteSpace(name) || name.IndexOfAny(new[] { '/', '\\', '\r', '\n' }) >= 0 || Flow_Module.FlowChart_ModuleList.Any(form => form.Name == name))
                throw new InvalidDataException("Invalid or duplicate module name.");
            Flow_BaseForm module = new Flow_BaseForm { Name = name, Text = name };
            module.EnsureProgramController();
            return module;
        }

        /// <summary>递归检查整个窗体，任何页签或嵌套容器有节点时禁止删除模块。</summary>
        public static void ValidateModuleDeletion(FlowModuleEditor module)
        {
            RequireIdle();
            if (module == null || !modules.ContainsValue(module) || module.Program.Removed) throw new InvalidOperationException(FlowText.T("moduleMissing"));
            if (module.Program.Steps.Count > 0 || ContainsFlowNode(module.Form))
                throw new InvalidOperationException(string.Format(FlowText.T("moduleHasNodes"), module.Form.Name));
        }

        /// <summary>删除空模块的设计草稿，保存后持久化删除标记并阻止原回调重新启用。</summary>
        public static void DeleteModule(FlowModuleEditor module)
        {
            ValidateModuleDeletion(module);
            module.SetEditing(false);
            module.Program.Removed = true;
            module.Program.Mode = "Disabled";
            module.Program.InitialEntry = null;
            module.Program.AutoEntry = null;
        }

        /// <summary>包括不可见页签在内检查所有子容器，不依赖节点是否已经导入模型。</summary>
        private static bool ContainsFlowNode(Control parent)
        {
            foreach (Control child in parent.Controls)
                if (child is Flow_Chart || ContainsFlowNode(child)) return true;
            return false;
        }

        /// <summary>删除输出源之前列出引用它的其他节点，避免留下无法解析的数据来源。</summary>
        internal static void ValidateNodeDeletion(FlowModuleEditor owner, string nodeId)
        {
            List<string> references = NodeReferences(owner.Program, nodeId);
            if (references.Count > 0) throw new InvalidOperationException(string.Format(FlowText.T("nodeReferenced"), string.Join(Environment.NewLine, references)));
        }

        /// <summary>列出当前模块草稿及其他模块对指定节点输出的引用。</summary>
        internal static List<string> NodeReferences(FlowProgram program, string nodeId)
        {
            string prefix = program.ModuleId + "/" + nodeId + "/";
            List<string> references = new List<string>();
            foreach (FlowProgram module in project.Modules.Where(module => module.ModuleId != program.ModuleId).Concat(new[] { program }))
            {
                foreach (FlowStep step in module.Steps)
                {
                    if (module.ModuleId == program.ModuleId && step.Id == nodeId) continue;
                    IEnumerable<FlowCondition> conditions = step.Conditions.Concat(step.Branches.SelectMany(branch => branch.Conditions));
                    IEnumerable<FlowInput> inputs = step.Inputs.Concat(conditions.SelectMany(condition => new[] { condition.Left, condition.Right }));
                    if (inputs.Any(input => input.Source == "Output" && (input.Reference ?? "").StartsWith(prefix, StringComparison.Ordinal)))
                        references.Add(module.ModuleId + " / " + step.Caption);
                }
            }
            return references;
        }

        /// <summary>常量删改前检查所有已载入工程的输入和出口条件，保留仍被引用的业务键与类型。</summary>
        public static void RequireParameterUnused(string reference)
        {
            Install();
            if (loadError != null) throw new InvalidDataException("Restore the flow project before changing parameter identities.", loadError);
            List<string> users = new List<string>();
            foreach (FlowProgram module in project.Modules)
            {
                foreach (FlowStep step in module.Steps)
                {
                    IEnumerable<FlowCondition> conditions = step.Conditions.Concat(step.Branches.SelectMany(branch => branch.Conditions));
                    IEnumerable<FlowInput> inputs = step.Inputs.Concat(conditions.SelectMany(condition => new[] { condition.Left, condition.Right }));
                    if (inputs.Any(input => input.Source == "Parameter" && input.Reference == reference) || ReadsConstant(step, reference)) users.Add(module.ModuleId + " / " + step.Caption);
                }
            }
            if (users.Count > 0) throw new InvalidOperationException(string.Format(Configuration.ConstantText.T("referenced"), string.Join(Environment.NewLine, users)));
        }

        /// <summary>常量表读取节点按作用域和业务键保护引用，动态键读取时保护该表的键。</summary>
        private static bool ReadsConstant(FlowStep step, string reference)
        {
            if (step.ExecutionMode == "Script" && (reference ?? "").StartsWith("Constants/", StringComparison.Ordinal))
            {
                string scope = reference.Substring("Constants/".Length).Split('/')[0];
                if (step.ScriptResources.Contains("constants/" + scope)) return true;
            }
            if (step.ExecutionMode != "Action") return false;
            if ((step.Action != "constant.read" && step.Action != "constant.set") || !(reference ?? "").StartsWith("Constants/", StringComparison.Ordinal)) return false;
            int separator = reference.IndexOf('/', "Constants/".Length);
            if (separator < 0 || step.Target != "constants/" + reference.Substring("Constants/".Length, separator - "Constants/".Length)) return false;
            FlowInput key = step.Inputs.FirstOrDefault(input => input.Name == "Key");
            return key != null && (key.Source != "Constant" || key.Reference == Uri.UnescapeDataString(reference.Substring(separator + 1)));
        }

        /// <summary>删除组合硬件前检查动作目标及点位参数引用，包含不可达草稿和独立 CASE 条件。</summary>
        public static void RequireHardwareUnused(string id,string tableName)
        {
            Install();
            if (loadError != null) throw new InvalidDataException("Restore the flow project before deleting hardware.",loadError);
            string targetPrefix = "hardware/"+id+"/";
            string parameterPrefix = "Recipe/HardForm/"+tableName+"/";
            List<string> users = new List<string>();
            foreach (FlowProgram module in project.Modules)
            {
                foreach (FlowStep step in module.Steps)
                {
                    IEnumerable<FlowCondition> conditions = step.Conditions.Concat(step.Branches.SelectMany(branch=>branch.Conditions));
                    IEnumerable<FlowInput> inputs = step.Inputs.Concat(conditions.SelectMany(condition=>new[]{condition.Left,condition.Right}));
                    bool scriptUsesResource = step.ExecutionMode == "Script" && step.ScriptResources.Any(target => target.StartsWith(targetPrefix, StringComparison.Ordinal));
                    bool codeUsesResource = step.ResourceBindings.Any(binding => (binding.Target ?? "").StartsWith(targetPrefix, StringComparison.Ordinal));
                    if (codeUsesResource || scriptUsesResource || (step.ExecutionMode == "Action" && (step.Target??"").StartsWith(targetPrefix,StringComparison.Ordinal)) || inputs.Any(input=>input.Source=="Parameter" && (input.Reference??"").StartsWith(parameterPrefix,StringComparison.Ordinal))) users.Add(module.ModuleId+" / "+step.Caption);
                }
            }
            if (users.Count>0) throw new InvalidOperationException(string.Format(Configuration.HardwareText.T("referenced"),string.Join(Environment.NewLine,users)));
        }

        /// <summary>删除外部对象或 VPP 前反查脚本与尚未处理的旧定义引用。</summary>
        public static void RequireResourceUnused(string resourceId)
        {
            Install();
            RequireIdle();
            if (loadError != null) throw new InvalidDataException("Restore the flow project before deleting resources.", loadError);
            List<string> users = project.Modules.SelectMany(module => module.Steps
                .Where(step => step.Target == resourceId || step.ScriptResources.Contains(resourceId) || step.ResourceBindings.Any(binding => binding.Target == resourceId))
                .Select(step => module.ModuleId + " / " + step.Caption)).ToList();
            if (users.Count != 0) throw new InvalidOperationException(string.Format(FlowResourceText.T("resourceUsed"), string.Join(Environment.NewLine, users)));
        }

        /// <summary>检查统一流程的生产入口、初始化状态及已保存定义。</summary>
        public static bool CanStartMachine(bool initialize)
        {
            if (DebugEnabled && !CanEdit) { MessageBox.Show(FlowText.T("permission"), FlowText.T("title")); return false; }
            if (!installed) return true;
            if (AnyEditing)
            {
                string names = string.Join(", ", modules.Values.Where(module => module.IsEditing).Select(module => module.Form.Name));
                MessageBox.Show(string.Format(FlowText.T("editingBlocksStart"), names), FlowText.T("title"));
                return false;
            }
            if (AnyRunning && (initialize || SysPara.SystemMode != RunMode.PAUSE))
            {
                if (initialize) StopAll();
                MessageBox.Show(FlowText.T("busy"), FlowText.T("title"));
                return false;
            }
            if (!HasPrograms) return true;
            try
            {
                RequireLoaded();
                foreach (FlowModuleEditor module in modules.Values)
                {
                    if (!initialize && !module.Form.InitialStatus) throw new InvalidOperationException("Initialize the module first: " + module.Form.Name);
                    module.ValidatePublished();
                }
                return true;
            }
            catch (Exception error) { MessageBox.Show(string.Format(FlowText.T("error"), error.Message), FlowText.T("title")); return false; }
        }

        /// <summary>按草稿或已发布计划提供全部模块的具名输出，引用使用稳定节点身份。</summary>
        public static List<FlowChoice> OutputChoices(FlowActions actions)
        {
            List<FlowChoice> result = new List<FlowChoice>();
            foreach (FlowModuleEditor editor in modules.Values)
                foreach (FlowStep step in (actions.UsePublishedOutputs ? editor.PublishedProgram : editor.Program).Steps)
                    AddOutputChoices(result, actions, editor, step);
            return result;
        }

        /// <summary>从动作描述及真实 VPP 字段生成一个节点的输出选项。</summary>
        private static void AddOutputChoices(List<FlowChoice> list, FlowActions actions, FlowModuleEditor module, FlowStep step)
        {
            FlowMethod action;
            try
            {
                FlowResource source = actions.FindResource(step.Target);
                action = step.UsesFlowResult && source != null && source.Kind == "native"
                    ? source.Methods.SingleOrDefault(method => method.Id == step.Action && method.UsesFlowResult) : actions.FindMethod(step);
            }
            catch (InvalidOperationException) { return; }
            if (action == null) return;
            foreach (FlowField field in action.Outputs)
                list.Add(new FlowChoice { Id = module.Program.ModuleId + "/" + step.Id + "/" + Uri.EscapeDataString(field.Name),
                    Label = module.Form.Name + " / " + step.Caption + " / " + FlowMethods.FieldTitle(action.Id, field, true),
                    ValueType = field.Type, ActionId = action.Id, FieldName = field.Name, TargetId = step.Target, Advanced = field.Advanced });
        }
    }

    public sealed partial class FlowModuleEditor : IFlowProgramController, IFlowProgramStatus
    {
        private readonly Dictionary<string, Flow_Chart> nodes = new Dictionary<string, Flow_Chart>();
        private readonly ToolTip bindingHints = new ToolTip { AutoPopDelay = 15000, InitialDelay = 350, ReshowDelay = 100 };
        private bool paused;
        private bool editing;
        private bool simulating;
        private bool isInitializing;
        private bool isProduction;
        private readonly HashSet<string> originalControls;
        private readonly FlowControlSource controlSource;
        private readonly Dictionary<string, Flow_Chart> removedNodes = new Dictionary<string, Flow_Chart>();
        private readonly ConcurrentDictionary<string, string> nodeStates = new ConcurrentDictionary<string, string>();
        private Flow_Chart timedNode;
        public readonly Flow_BaseForm Form;
        public readonly FlowProgramRuntime Runtime = new FlowProgramRuntime();
        public bool IsRunning { get { return Runtime.IsRunning; } }
        public bool IsInitializing { get { return isProduction && isInitializing && Runtime.IsRunning; } }
        public FlowProgram Program { get; private set; }
        private FlowProgram published;
        internal FlowProgram PublishedProgram { get { return published; } }
        public bool IsEnabled { get { return true; } }
        public bool IsProgram { get { return published != null && published.Mode == "Program"; } }
        public bool IsRemovedPublished { get { return published != null && published.Removed; } }
        public bool IsEditing { get { return editing; } }
        public event EventHandler Changed;

        /// <summary>统一挂接任意派生窗体，并恢复或首次导入其全部 Flow_Chart。</summary>
        public FlowModuleEditor(Flow_BaseForm form, FlowProgram program)
        {
            Form = form;
            originalControls = new HashSet<string>(Enumerate(form).Select(node => node.Name));
            controlSource = new FlowControlSource(form);
            Restore(program);
            Publish();
            Runtime.StateChanged += UpdateNodeExecution;
            Runtime.DebugChanged += CaptureDebug;
            Runtime.AlarmRequested += code => { if (!simulating) Alarm.Show(string.IsNullOrWhiteSpace(code) ? "2019" : code); };
            Runtime.Finished += Finish;
            MiddleLayer.LanguageChanged += RefreshBindingHints;
            Form.Disposed += DisposeEditor;
        }

        /// <summary>释放编辑器提示和为撤销暂存的脱离画布节点。</summary>
        private void DisposeEditor(object sender, EventArgs args)
        {
            MiddleLayer.LanguageChanged -= RefreshBindingHints;
            bindingHints.Dispose();
            foreach (Flow_Chart node in removedNodes.Values)
                if (node.Parent == null) node.Dispose();
            removedNodes.Clear();
        }

        /// <summary>递归枚举节点及其父容器，保留现有窗体布局。</summary>
        private static IEnumerable<Flow_Chart> Enumerate(Control root)
        {
            foreach (Control child in root.Controls)
            {
                Flow_Chart node = child as Flow_Chart;
                if (node != null) { yield return node; continue; }
                foreach (Flow_Chart nested in Enumerate(child)) yield return nested;
            }
        }

        /// <summary>导入新的原有节点，再恢复持久化的节点与连接。</summary>
        public void Restore(FlowProgram program)
        {
            if (Runtime.IsRunning) throw new InvalidOperationException(FlowText.T("busy"));
            FlowProgram prepared = PrepareRestore(program);
            program.Steps = prepared.Steps;
            bool importing = program.Mode == "Legacy";
            if (importing) program.Mode = "Program";
            foreach (ScrollableControl canvas in program.Steps.Select(step => FindParent(step.ParentPath)).OfType<ScrollableControl>().Distinct()) canvas.AutoScrollPosition = Point.Empty;
            foreach (Flow_Chart node in nodes.Values) node.ConfigurationRequested -= Configure;
            nodes.Clear();
            nodeStates.Clear();
            Program = program;
            foreach (KeyValuePair<string, Flow_Chart> removed in removedNodes.ToArray())
            {
                if (program.RemovedControls.Contains(removed.Key)) continue;
                FlowStep restored = program.Steps.SingleOrDefault(step => step.ControlName == removed.Key);
                if (restored == null) continue;
                Form.Controls.Add(removed.Value);
                if (!Flow_Module.FlowChart_List.Contains(removed.Value)) Flow_Module.FlowChart_List.Add(removed.Value);
                removedNodes.Remove(removed.Key);
            }
            List<Flow_Chart> existing = Enumerate(Form).ToList();
            foreach (Flow_Chart node in existing)
            {
                if (program.Removed || program.RemovedControls.Contains(node.Name) || (!originalControls.Contains(node.Name) && program.Steps.All(definition => definition.ControlName != node.Name)))
                {
                    if (program.Removed && !program.RemovedControls.Contains(node.Name)) program.RemovedControls.Add(node.Name);
                    RemoveControl(node);
                    continue;
                }
                FlowStep step = program.Steps.SingleOrDefault(item => item.ControlName == node.Name);
                if (step == null)
                {
                    step = new FlowStep { ControlName = node.Name, Caption = node.text, X = node.Left, Y = node.Top, ParentPath = ParentPath(node.Parent) };
                    program.Steps.Add(step);
                }
                nodes.Add(step.Id, node);
            }
            foreach (FlowStep step in program.Steps)
            {
                if (!nodes.ContainsKey(step.Id)) nodes.Add(step.Id, CreateNode(step));
                Flow_Chart node = nodes[step.Id];
                Control savedParent = FindParent(step.ParentPath);
                if (node.Parent != savedParent) savedParent.Controls.Add(node);
                node.Location = new Point(step.X, step.Y);
                node.text = step.Caption;
                node.ConfigurationRequested += Configure;
                node.EnableProgramEditor(editing);
                node.ProgramManaged = IsEnabled;
                node.ProgramBreakpoint = step.Breakpoint;
                node.BringToFront();
            }
            ImportLegacyLinks();
            foreach (FlowStep step in Program.Steps)
            {
                FlowNativeMethods.BindExisting(Form, nodes[step.Id], step);
                controlSource.ApplyNativeAlarm(step);
            }
            if (importing)
            {
                if (string.IsNullOrEmpty(program.InitialEntry)) program.InitialEntry = ImportedEntry(Form.InitialPage);
                if (string.IsNullOrEmpty(program.AutoEntry)) program.AutoEntry = ImportedEntry(Form.AutoPage);
            }
            ApplyLinks();
            foreach (ScrollableControl canvas in nodes.Values.Select(node => node.Parent).OfType<ScrollableControl>().Distinct()) UpdateCanvasBounds(canvas);
        }

        /// <summary>先检查全部节点和父容器，避免无效布局加载到一半破坏当前工程。</summary>
        public void ValidateLayout(FlowProgram program)
        {
            List<string> errors = FlowProgramFile.Validate(program);
            if (errors.Count > 0) throw new InvalidDataException(string.Join(Environment.NewLine, errors));
            if (program.Steps.Any(step => string.IsNullOrWhiteSpace(step.ControlName)) || program.Steps.GroupBy(step => step.ControlName).Any(group => group.Count() != 1))
                throw new InvalidDataException("Invalid or duplicate flow control name.");
            foreach (FlowStep step in program.Steps) FindParent(step.ParentPath);
        }

        /// <summary>在改变当前画布之前完成源码同步及布局校验。</summary>
        internal FlowProgram PrepareRestore(FlowProgram program)
        {
            FlowProgram prepared = controlSource.Prepare(program);
            ValidateLayout(prepared);
            return prepared;
        }

        /// <summary>仅首次导入旧节点的控件连接，之后以工程定义为准。</summary>
        private void ImportLegacyLinks()
        {
            foreach (FlowStep step in Program.Steps.Where(item => item.Action == "legacy"))
            {
                Flow_Chart node = nodes[step.Id];
                if (step.Next == null) step.Next = IdOf(node.Next);
                if (step.Case1 == null) step.Case1 = IdOf(node.Casse1);
                if (step.Case2 == null) step.Case2 = IdOf(node.Casse2);
                if (step.Case3 == null) step.Case3 = IdOf(node.Casse3);
            }
        }

        /// <summary>首次导入仅采用所在页唯一的无入边原节点，多个流程入口由工程师明确选择。</summary>
        private string ImportedEntry(Control page)
        {
            FlowStep[] steps = Program.Steps.Where(step => originalControls.Contains(step.ControlName)
                && page.Contains(nodes[step.Id])).ToArray();
            HashSet<string> destinations = new HashSet<string>(steps.SelectMany(step => new[] { step.Next, step.Case1, step.Case2, step.Case3 }).Where(id => id != null));
            FlowStep[] roots = steps.Where(step => !destinations.Contains(step.Id)).ToArray();
            return roots.Length == 1 ? roots[0].Id : null;
        }

        /// <summary>取得节点稳定身份。</summary>
        internal string IdOf(Flow_Chart node) { return node == null ? null : nodes.Where(item => ReferenceEquals(item.Value, node)).Select(item => item.Key).FirstOrDefault(); }

        /// <summary>保存相对窗体的父容器路径。</summary>
        private string ParentPath(Control parent)
        {
            if (parent == null || ReferenceEquals(parent, Form)) return "";
            return ParentPath(parent.Parent) + "/" + parent.Name;
        }

        /// <summary>在原父容器创建真实 SDK 节点，找不到父容器时拒绝恢复。</summary>
        private Flow_Chart CreateNode(FlowStep step)
        {
            Control parent = FindParent(step.ParentPath);
            Flow_Chart node = new Flow_Chart { Name = step.ControlName, text = step.Caption, Location = new Point(step.X, step.Y) };
            ((Control)node).Name = step.ControlName;
            parent.Controls.Add(node);
            return node;
        }

        /// <summary>按保存的容器路径解析原布局。</summary>
        private Control FindParent(string path)
        {
            Control parent = Form;
            foreach (string segment in (path ?? "").Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries))
                parent = parent.Controls.Cast<Control>().Single(control => control.Name == segment);
            return parent;
        }

        /// <summary>把模型控制连接映射回 SDK 节点，继续复用原来的连线显示。</summary>
        public void ApplyLinks()
        {
            foreach (FlowStep step in Program.Steps)
            {
                Flow_Chart node = nodes[step.Id];
                node.text = step.Caption;
                node.Next = Node(step.Next);
                node.Casse1 = Node(step.Case1);
                node.Casse2 = Node(step.Case2);
                node.Casse3 = Node(step.Case3);
                node.ProgramManaged = IsEnabled;
                node.ProgramBreakpoint = step.Breakpoint;
            }
            RefreshBindingHints(null, EventArgs.Empty);
            Form.Invalidate(true);
        }

        /// <summary>节点独立标示功能、代码或条件，显示绑定详情且不修改业务标题。</summary>
        private void RefreshBindingHints(object sender, EventArgs args)
        {
            foreach (FlowStep step in Program.Steps)
            {
                Flow_Chart node = Node(step.Id);
                if (node == null || node.IsDisposed) continue;
                string key = step.ControlOrigin == FlowControlSource.MissingOrigin ? "badgePending" : step.ExecutionMode == "None" ? "badgeCondition" : step.UsesFlowResult ? "badgeNative" : step.Action == "legacy" ? "badgePending" : "badgeAction";
                node.ProgramBindingLabel = FlowNativeText.T(key);
                string description;
                if (step.ControlOrigin == FlowControlSource.MissingOrigin) description = string.Format(FlowText.T("sourceControlMissing"), step.Caption, step.ControlName);
                else if (step.ExecutionMode == "None") description = FlowNativeText.T("conditionBinding");
                else if (step.UsesFlowResult)
                {
                    string method = step.Action ?? "";
                    int signature = method.LastIndexOf('.');
                    if (method.StartsWith("native.", StringComparison.Ordinal) && signature > "native.".Length)
                        method = method.Substring("native.".Length, signature - "native.".Length) + "()";
                    description = string.Format(FlowNativeText.T("nativeBinding"), Form.Name, method);
                }
                else description = string.Format(FlowNativeText.T("actionBinding"), step.Target ?? "", step.Action ?? "");
                node.AccessibleDescription = description;
                BindHint(node, description);
                string state;
                if (nodeStates.TryGetValue(step.Id, out state)) DisplayState(step, state);
            }
        }

        /// <summary>鼠标经过节点的任意原有表面时均可看到同一执行绑定。</summary>
        private void BindHint(Control control, string description)
        {
            bindingHints.SetToolTip(control, description);
            foreach (Control child in control.Controls) BindHint(child, description);
        }

        /// <summary>安全解析可空的连接终点。</summary>
        private Flow_Chart Node(string id) { Flow_Chart node; return id != null && nodes.TryGetValue(id, out node) ? node : null; }

        /// <summary>统一切换节点拖动及配置状态。</summary>
        public void SetEditing(bool value)
        {
            if (value && Program.Removed) throw new InvalidOperationException(FlowText.T("moduleMissing"));
            if (value) FlowPrograms.RequireIdle();
            editing = value;
            foreach (Flow_Chart node in nodes.Values) node.EnableProgramEditor(value);
        }

        /// <summary>在当前画布添加普通节点，保存时同步生成所属窗体的流程代码。</summary>
        public FlowStep AddNode()
        {
            Control parent = Enumerate(Form).Where(item => item.Visible && item.Parent != Form).Select(item => item.Parent).FirstOrDefault();
            if (parent == null) parent = (Control)Form.FlowChartTable.SelectedTab ?? Form;
            Point offset = parent is ScrollableControl ? ((ScrollableControl)parent).AutoScrollPosition : Point.Empty;
            int y = nodes.Values.Where(existing => existing.Parent == parent).Select(existing => existing.Bottom - offset.Y + 40).DefaultIfEmpty(40).Max();
            FlowStep step = NewStep();
            InitializeAction(step);
            return AddNodeCore(step, parent, new Point(40, y));
        }

        /// <summary>只创建对象动作或条件节点，拒绝已移除的执行方式。</summary>
        public FlowStep AddNode(string mode, Control canvas, Point logicalLocation)
        {
            if (mode != "Action" && mode != "None") throw new InvalidOperationException(FlowText.T("retiredNodeChoice"));
            FlowStep step = NewStep();
            step.ExecutionMode = mode;
            if (mode == "Action") InitializeAction(step);
            else step.Action = "flow.none";
            return AddNodeCore(step, canvas, logicalLocation);
        }

        /// <summary>新动作节点默认传递一个值，再由编辑器选择实际对象和方法。</summary>
        private static void InitializeAction(FlowStep step)
        {
            step.ExecutionMode = "Action";
            step.Action = "value";
            step.Inputs = new List<FlowInput> { new FlowInput { Name = "Value", Reference = "" } };
        }

        /// <summary>从工具箱绑定真实对象的方法，保留对象身份及方法声明的默认输入。</summary>
        public FlowStep AddNode(string targetId, string actionId, Control canvas, Point logicalLocation)
        {
            FlowActions actions = new FlowActions(FlowResources.Capture(true), Form);
            FlowResource resource = actions.FindResource(targetId);
            if (resource == null && !string.IsNullOrEmpty(targetId)) throw new InvalidOperationException(FlowText.T("methodMissing"));
            FlowMethod method = actions.MethodsFor(resource).Single(item => item.Id == actionId);
            FlowStep step = NewStep();
            step.ExecutionMode = "Action";
            step.Caption = resource == null ? FlowMethods.Title(method) : resource.Label + " · " + FlowMethods.Title(method);
            step.Target = targetId;
            step.Action = actionId;
            step.UsesFlowResult = method.UsesFlowResult;
            if (step.UsesFlowResult) { step.TimeoutMs = Timeout.Infinite; step.AlarmCode = null; }
            step.Inputs = method.Inputs.Select(field => new FlowInput { Name = field.Name, Source = string.IsNullOrEmpty(field.OutputSourceMethod) ? "Constant" : "Output", Reference = field.DefaultValue }).ToList();
            return AddNodeCore(step, canvas, logicalLocation);
        }

        /// <summary>建立独立节点身份，默认只有 NEXT，入口沿用原工位页面。</summary>
        private FlowStep NewStep()
        {
            return new FlowStep { ControlName = "flow_" + Guid.NewGuid().ToString("N"), ControlOrigin = FlowControlSource.DesignerOrigin, Caption = "Step " + (Program.Steps.Count + 1),
                AlarmCode = "2019", Branches = FlowBranch.Default() };
        }

        /// <summary>统一落点、滚动坐标、入口和配置事件，不重复构造运行器。</summary>
        private FlowStep AddNodeCore(FlowStep step, Control parent, Point location)
        {
            FlowPrograms.RequireIdle();
            if (Program.Removed) throw new InvalidOperationException(FlowText.T("moduleMissing"));
            if (parent == null || (parent != Form && !Form.Contains(parent)) || parent is Flow_Chart)
                throw new ArgumentException("The drop target is not a flow canvas.");
            step.ParentPath = ParentPath(parent);
            step.X = Math.Max(0, location.X);
            step.Y = Math.Max(0, location.Y);
            Program.Steps.Add(step);
            bool initial = parent == Form.InitialPage || Form.InitialPage.Contains(parent);
            if (initial && string.IsNullOrEmpty(Program.InitialEntry)) Program.InitialEntry = step.Id;
            if (!initial && string.IsNullOrEmpty(Program.AutoEntry)) Program.AutoEntry = step.Id;
            Flow_Chart node = CreateNode(step);
            nodes.Add(step.Id, node);
            node.ConfigurationRequested += Configure;
            node.EnableProgramEditor(editing);
            node.BringToFront();
            ScrollableControl canvas = parent as ScrollableControl;
            if (canvas != null)
            {
                Point scroll = canvas.AutoScrollPosition;
                node.Location = new Point(step.X + scroll.X, step.Y + scroll.Y);
                UpdateCanvasBounds(canvas);
                canvas.ScrollControlIntoView(node);
            }
            RefreshBindingHints(null, EventArgs.Empty);
            if (Changed != null) Changed(this, EventArgs.Empty);
            return step;
        }

        /// <summary>按全部节点的逻辑位置恢复画布滚动范围，保留现有节点布局。</summary>
        private void UpdateCanvasBounds(ScrollableControl canvas)
        {
            Point scroll = canvas.AutoScrollPosition;
            Flow_Chart[] children = nodes.Values.Where(node => node.Parent == canvas).ToArray();
            int right = children.Select(node => node.Right - scroll.X + 40).DefaultIfEmpty(0).Max();
            int bottom = children.Select(node => node.Bottom - scroll.Y + 40).DefaultIfEmpty(0).Max();
            canvas.AutoScroll = true;
            canvas.AutoScrollMinSize = new Size(right, bottom);
        }

        /// <summary>删除选中节点并清理全部入口及连接。</summary>
        public void Delete(string id)
        {
            FlowPrograms.RequireIdle();
            Flow_Chart node = Node(id);
            if (node == null) return;
            FlowPrograms.ValidateNodeDeletion(this, id);
            ScrollableControl canvas = node.Parent as ScrollableControl;
            node.ConfigurationRequested -= Configure;
            Program.RemovedControls.Add(node.Name);
            RemoveControl(node);
            nodes.Remove(id);
            string previousState;
            nodeStates.TryRemove(id, out previousState);
            Program.Steps.RemoveAll(step => step.Id == id);
            foreach (FlowStep step in Program.Steps)
            {
                if (step.Next == id) step.Next = null;
                if (step.Case1 == id) step.Case1 = null;
                if (step.Case2 == id) step.Case2 = null;
                if (step.Case3 == id) step.Case3 = null;
            }
            if (Program.InitialEntry == id) Program.InitialEntry = null;
            if (Program.AutoEntry == id) Program.AutoEntry = null;
            ApplyLinks();
            if (canvas != null) UpdateCanvasBounds(canvas);
        }

        /// <summary>保留原控件及回调，重新加载旧版本时可恢复；移出全局运行列表。</summary>
        private void RemoveControl(Flow_Chart node)
        {
            if (node.Parent != null) node.Parent.Controls.Remove(node);
            Flow_Module.FlowChart_List.Remove(node);
            node.ProgramManaged = true;
            if (originalControls.Contains(node.Name)) removedNodes[node.Name] = node;
            else node.Dispose();
        }

        /// <summary>保存所有节点当前坐标，业务名称由编辑窗口维护。</summary>
        public void CaptureLayout()
        {
            foreach (FlowStep step in Program.Steps)
            {
                Flow_Chart node = nodes[step.Id];
                Point scroll = node.Parent is ScrollableControl ? ((ScrollableControl)node.Parent).AutoScrollPosition : Point.Empty;
                step.X = node.Left - scroll.X; step.Y = node.Top - scroll.Y; step.ParentPath = ParentPath(node.Parent);
            }
        }

        /// <summary>双击仅配置功能和条件节点，代码节点保持只读绑定。</summary>
        private void Configure(object sender, EventArgs args)
        {
            if (!editing) return;
            string id = IdOf((Flow_Chart)sender);
            FlowStep step = Program.Steps.SingleOrDefault(item => item.Id == id);
            if (step == null || step.UsesFlowResult) return;
            try { EditNode(id); }
            catch (Exception error) { MessageBox.Show(string.Format(FlowText.T("error"), error.Message), FlowText.T("title")); }
        }

        /// <summary>功能和条件节点共用编辑入口；代码节点不打开属性窗口。</summary>
        public void EditNode(string id)
        {
            FlowPrograms.RequireIdle();
            if (!editing) throw new InvalidOperationException(FlowText.T("enableEditing"));
            FlowStep step = Program.Steps.SingleOrDefault(item => item.Id == id);
            if (step == null) throw new InvalidOperationException(FlowText.T("methodMissing"));
            if (step.UsesFlowResult) return;
            using (FlowNodeForm dialog = new FlowNodeForm(this, step))
                if (dialog.ShowDialog(Form) == DialogResult.OK) { ApplyLinks(); if (Changed != null) Changed(this, EventArgs.Empty); }
        }

        /// <summary>检查工程结构及已选择入口的真实资源、参数和动作。</summary>
        public void Validate(string entry)
        {
            ValidatePlan(Program, entry, new FlowActions(FlowResources.Capture(true), Form));
        }

        /// <summary>恢复当前模块已发布的初始或保存快照。</summary>
        internal void RestorePublished()
        {
            Restore(FlowProgramFile.Snapshot(published));
            if (Changed != null) Changed(this,EventArgs.Empty);
        }

        /// <summary>校验固定的执行计划，生产执行只使用已保存版本。</summary>
        private void ValidatePlan(FlowProgram plan, string entry, FlowActions actions)
        {
            List<string> errors = FlowProgramFile.Validate(plan);
            if (errors.Count > 0) throw new InvalidDataException(string.Join(Environment.NewLine, errors));
            if (plan.Mode == "Disabled" || plan.Removed) return;
            if (plan.Mode != "Program") throw new InvalidDataException(FlowText.T("invalidFlowMode") + " " + Form.Name);
            if (entry == null) ValidateEntries(plan);
            IEnumerable<FlowStep> reachable = entry == null
                ? FlowProgramFile.Reachable(plan, plan.InitialEntry).Concat(FlowProgramFile.Reachable(plan, plan.AutoEntry))
                : FlowProgramFile.Reachable(plan, entry);
            foreach (FlowStep step in reachable) actions.Validate(step);
        }

        /// <summary>有节点的流程页必须明确指定入口，空模块允许保留为待编排页面。</summary>
        private void ValidateEntries(FlowProgram plan)
        {
            bool initialNodes = plan.Steps.Any(step => Form.InitialPage.Contains(Node(step.Id)));
            bool autoNodes = plan.Steps.Any(step => Form.AutoPage.Contains(Node(step.Id)));
            if (initialNodes && string.IsNullOrEmpty(plan.InitialEntry)) throw new InvalidDataException(FlowText.T("initialEntryRequired") + " " + Form.Name);
            if (autoNodes && string.IsNullOrEmpty(plan.AutoEntry)) throw new InvalidDataException(FlowText.T("autoEntryRequired") + " " + Form.Name);
            if (plan.Steps.Count > 0 && string.IsNullOrEmpty(plan.InitialEntry) && string.IsNullOrEmpty(plan.AutoEntry))
                throw new InvalidDataException(FlowText.T("flowEntryRequired") + " " + Form.Name);
        }

        /// <summary>通过同一控制器处理所有派生窗体的生命周期。</summary>
        public bool Handle(FlowProgramCommand command)
        {
            if (command == FlowProgramCommand.Stop)
            {
                isInitializing = false; paused = false;
                Form.InitialStatus = false; Form.PauseStatus = false; Form.StopStatus = true;
                if (timedNode != null)
                {
                    timedNode.PauseProgramTiming();
                    string id = IdOf(timedNode);
                    if (id != null) nodeStates[id] = "Cancelled";
                    timedNode.SetProgramState(FlowText.T("Cancelled"), Color.White);
                }
                Runtime.Stop(); return true;
            }
            if (command == FlowProgramCommand.Pause) { Runtime.Pause(); paused = true; if (timedNode != null) timedNode.PauseProgramTiming(); return true; }
            if (command == FlowProgramCommand.Resume || (command == FlowProgramCommand.Start && paused && Runtime.IsRunning))
            {
                bool retry = Runtime.IsWaitingForRetry;
                Runtime.Resume(); paused = Runtime.IsPaused; Form.PauseStatus = paused;
                string id = timedNode == null ? null : IdOf(timedNode), state;
                if (!paused && !retry && Runtime.IsRunning && id != null && nodeStates.TryGetValue(id, out state) && (state == "Running" || state == "Waiting"))
                    timedNode.ResumeProgramTiming();
                return true;
            }
            if (published.Mode == "Disabled") { Form.InitialStatus = true; Form.RunningStatus = false; return true; }
            Action start = () => StartCore(command == FlowProgramCommand.Initialize ? published.InitialEntry : published.AutoEntry, SysPara.Simulation, command == FlowProgramCommand.Initialize, true, FlowPrograms.DebugEnabled);
            if (MiddleLayer.MainF != null && MiddleLayer.MainF.InvokeRequired) MiddleLayer.MainF.Invoke(start); else start();
            return true;
        }

        /// <summary>用不可变参数及点位快照执行选定入口。</summary>
        public void Start(string entry, bool simulate, bool initializing, bool production = false)
        {
            StartCore(entry, simulate, initializing, production, false);
        }

        /// <summary>使用同一已校验运行计划，可选在节点执行前进入调试等待。</summary>
        private void StartCore(string entry, bool simulate, bool initializing, bool production, bool debug)
        {
            if (Runtime.IsRunning) throw new InvalidOperationException(FlowText.T("busy"));
            if (production) FlowPrograms.RequireLoaded();
            SetEditing(false);
            FlowProgram plan = production ? published : Program;
            if (plan.Removed) throw new InvalidOperationException(FlowText.T("moduleMissing"));
            FlowResources resources = FlowResources.Capture();
            FlowActions actions = new FlowActions(resources, Form) { Simulate = simulate, RequestInput = FlowInputForm.Request, UsePublishedOutputs = production };
            ValidatePlan(plan, entry, actions);
            if (string.IsNullOrEmpty(entry)) { Form.InitialStatus = true; Form.RunningStatus = false; return; }
            simulating = simulate;
            isInitializing = initializing;
            isProduction = production;
            Runtime.ExecuteAction = actions.Execute;
            Runtime.ExecuteStep = actions.ExecuteOutcome;
            Runtime.PauseOnFailure = production && initializing;
            DebugSnapshot = null;
            PrepareDebugPlan(plan, entry, actions, debug);
            Form.RunningStatus = true;
            if (initializing) Form.InitialStatus = false;
            Form.StopStatus = false;
            Form.PauseStatus = false;
            paused = false;
            timedNode = null; nodeStates.Clear();
            foreach (Flow_Chart node in nodes.Values) { node.ResetProgramTiming(); node.SetProgramState("", Color.White); }
            foreach (Flow_Chart node in nodes.Values) node.ProgramManaged = true;
            try
            {
                Runtime.Start(plan, entry, new FlowExecutionContext { Hub = FlowPrograms.Results, RunId = FlowPrograms.Results.Epoch,
                    CycleId = "batch", ModuleId = plan.ModuleId, RecipeName = resources.RecipeName, Parameters = resources.Parameters }, debug);
            }
            catch
            {
                Form.RunningStatus = false;
                if (initializing) Form.InitialStatus = false;
                throw;
            }
        }

        /// <summary>执行任务完成前更新模块状态，避免旧回调覆盖下一次启动。</summary>
        private void Finish()
        {
            if (timedNode != null) timedNode.PauseProgramTiming();
            Form.RunningStatus = false;
            foreach (Flow_Chart node in nodes.Values) node.ProgramManaged = IsEnabled;
            if (isInitializing) Form.InitialStatus = Runtime.Error == null;
            if (Runtime.Error != null) OperationLog.Write("Flow", Form.Name + ": " + Runtime.Error);
            if (isProduction && !Runtime.AlarmWasRaised && Runtime.Error != null && Runtime.Error != "Cancelled") Alarm.Show("2019", Form.Name + ": " + Runtime.Error);
        }

        /// <summary>切换到节点 UI 显示状态，语言切换不修改节点业务名称。</summary>
        private void UpdateNodeExecution(FlowStep step, string state)
        {
            Flow_Chart node = Node(step.Id);
            if (node == null) return;
            if (state == "Running") { timedNode = node; node.BeginProgramTiming(); }
            if (state == "Completed" || state == "Timeout" || state == "Failed" || state == "Cancelled" || state == "RecoveryPaused") node.PauseProgramTiming();
            DisplayState(step, state);
        }

        /// <summary>复用流程页刷新周期，仅更新当前节点的右上角耗时。</summary>
        public void RefreshElapsed() { if (timedNode != null && !timedNode.IsDisposed) timedNode.RefreshProgramTiming(); }

        /// <summary>显示节点业务状态，语言刷新不重新启动计时。</summary>
        private void DisplayState(FlowStep step, string state)
        {
            Flow_Chart node = Node(step.Id);
            if (node == null) return;
            nodeStates[step.Id] = state;
            Color color = state == "DebugPaused" || state == "RecoveryPaused" ? Color.LightSalmon : state == "Running" ? Color.LightGreen : state == "Completed" ? Color.White : state == "Waiting" ? Color.LightYellow : Color.MistyRose;
            node.SetProgramState(FlowText.T(state), color);
        }

        /// <summary>在暂停与恢复间切换同一执行会话。</summary>
        public void TogglePause()
        {
            if (IsInitializing && Runtime.IsPaused) throw new InvalidOperationException(FlowText.T("continueInitializationHint"));
            Handle(paused ? FlowProgramCommand.Resume : FlowProgramCommand.Pause);
        }

        /// <summary>工程成功落盘后固定生产版本，草稿仅供工程测试。</summary>
        public void Publish() { published = FlowProgramFile.Snapshot(Program); ApplyLinks(); }

        /// <summary>发布主保存已经提交的独立定义，当前编辑画布继续使用原草稿对象。</summary>
        internal void Publish(FlowProgram saved) { published = FlowProgramFile.Snapshot(saved); ApplyLinks(); }

        /// <summary>生产启动前检查最后保存版本。</summary>
        public void ValidatePublished()
        {
            FlowPrograms.RequireLoaded();
            ValidatePlan(published, null, new FlowActions(FlowResources.Capture(), Form) { UsePublishedOutputs = true });
        }
    }
}
