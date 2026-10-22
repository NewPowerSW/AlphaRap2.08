using System;
using System.Collections.Generic;
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
        public static bool AnyRunning { get { return modules.Values.Any(module => module.Runtime.IsRunning); } }
        public static bool AnyEditing { get { return modules.Values.Any(module => module.IsEditing); } }
        public static bool HasPrograms { get { return modules.Values.Any(module => module.IsProgram || module.IsRemovedPublished); } }
        public static bool IsInitialized { get { return HasPrograms && modules.Values.All(module => module.IsEnabled && module.Form.InitialStatus); } }
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
            form.Disposed += (sender, args) => { editor.Runtime.Stop(); modules.Remove(form); Flow_Module.FlowChart_ModuleList.Remove(form); };
            return editor;
        }

        /// <summary>运行和修改前统一检查工位、机器状态及权限。</summary>
        public static void RequireIdle()
        {
            if (AnyRunning || SysPara.SystemMode != RunMode.IDLE || Flow_Module.Module_GetRunningStatus) throw new InvalidOperationException(FlowText.T("busy"));
            if (!CanEdit) throw new InvalidOperationException(FlowText.T("permission"));
        }

        /// <summary>保存完整工程；加载失败时禁止将损坏文件覆盖为空工程。</summary>
        public static void Save()
        {
            RequireIdle();
            if (loadError != null) throw new InvalidDataException("Reload or restore the project backup first.", loadError);
            foreach (FlowModuleEditor editor in modules.Values) { editor.CaptureLayout(); editor.Validate(null); }
            FlowProgramFile.Save(FilePath, project);
            foreach (FlowModuleEditor editor in modules.Values) editor.Publish();
            OperationLog.Write("Flow", "Save flow project: " + FilePath);
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
                if (definition != null) editor.ValidateLayout(definition);
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
            string prefix = owner.Program.ModuleId + "/" + nodeId + "/";
            List<string> references = new List<string>();
            foreach (FlowProgram module in project.Modules)
            {
                foreach (FlowStep step in module.Steps)
                {
                    if (module.ModuleId == owner.Program.ModuleId && step.Id == nodeId) continue;
                    IEnumerable<FlowCondition> conditions = step.Conditions.Concat(step.Branches.SelectMany(branch => branch.Conditions));
                    IEnumerable<FlowInput> inputs = step.Inputs.Concat(conditions.SelectMany(condition => new[] { condition.Left, condition.Right }));
                    if (inputs.Any(input => input.Source == "Output" && (input.Reference ?? "").StartsWith(prefix, StringComparison.Ordinal)))
                        references.Add(module.ModuleId + " / " + step.Caption);
                }
            }
            if (references.Count > 0) throw new InvalidOperationException(string.Format(FlowText.T("nodeReferenced"), string.Join(Environment.NewLine, references)));
        }

        /// <summary>检查生产入口，显式阻止旧回调与配置流程混合控制同一设备。</summary>
        public static bool CanStartMachine(bool initialize)
        {
            if (!installed) return true;
            if (AnyEditing || (AnyRunning && (initialize || SysPara.SystemMode != RunMode.PAUSE)))
            {
                if (initialize) StopAll();
                MessageBox.Show(FlowText.T("busy"), FlowText.T("title"));
                return false;
            }
            if (!HasPrograms) return true;
            try
            {
                foreach (FlowModuleEditor module in modules.Values)
                {
                    if (!module.IsEnabled) throw new InvalidOperationException("Set every flow module to Program or Disabled before machine execution: " + module.Form.Name);
                    if (!initialize && !module.Form.InitialStatus) throw new InvalidOperationException("Initialize the module first: " + module.Form.Name);
                    module.ValidatePublished();
                }
                return true;
            }
            catch (Exception error) { MessageBox.Show(string.Format(FlowText.T("error"), error.Message), FlowText.T("title")); return false; }
        }

        /// <summary>向源选择器提供全部模块的具名输出，引用使用稳定节点身份。</summary>
        public static List<FlowChoice> OutputChoices(FlowActions actions)
        {
            List<FlowChoice> result = new List<FlowChoice>();
            foreach (FlowModuleEditor editor in modules.Values)
                foreach (FlowStep step in editor.Program.Steps) AddOutputChoices(result, actions, editor, step);
            return result;
        }

        /// <summary>从动作描述及真实 VPP 字段生成一个节点的输出选项。</summary>
        private static void AddOutputChoices(List<FlowChoice> list, FlowActions actions, FlowModuleEditor module, FlowStep step)
        {
            FlowAction action = actions.Actions.SingleOrDefault(item => item.Id == step.Action);
            if (action == null) return;
            foreach (string field in actions.OutputNames(action, actions.FindResource(step.Target)))
                list.Add(new FlowChoice { Id = module.Program.ModuleId + "/" + step.Id + "/" + Uri.EscapeDataString(field), Label = module.Form.Name + " / " + step.Caption + " / " + field });
        }
    }

    public sealed class FlowModuleEditor : IFlowProgramController
    {
        private readonly Dictionary<string, Flow_Chart> nodes = new Dictionary<string, Flow_Chart>();
        private bool paused;
        private bool editing;
        private bool simulating;
        private bool isInitializing;
        private bool isProduction;
        private readonly HashSet<string> originalControls;
        private readonly Dictionary<string, Flow_Chart> removedNodes = new Dictionary<string, Flow_Chart>();
        public readonly Flow_BaseForm Form;
        public readonly FlowProgramRuntime Runtime = new FlowProgramRuntime();
        public FlowProgram Program { get; private set; }
        private FlowProgram published;
        public bool IsEnabled { get { return published != null && published.Mode != "Legacy"; } }
        public bool IsProgram { get { return published != null && published.Mode == "Program"; } }
        public bool IsRemovedPublished { get { return published != null && published.Removed; } }
        public bool IsEditing { get { return editing; } }
        public event EventHandler Changed;

        /// <summary>统一挂接任意派生窗体，并恢复或首次导入其全部 Flow_Chart。</summary>
        public FlowModuleEditor(Flow_BaseForm form, FlowProgram program)
        {
            Form = form;
            originalControls = new HashSet<string>(Enumerate(form).Select(node => node.Name));
            Restore(program);
            Publish();
            Runtime.StateChanged += DisplayState;
            Runtime.AlarmRequested += code => { if (!simulating) Alarm.Show(string.IsNullOrWhiteSpace(code) ? "2019" : code); };
            Runtime.Finished += Finish;
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
            ValidateLayout(program);
            foreach (ScrollableControl canvas in program.Steps.Select(step => FindParent(step.ParentPath)).OfType<ScrollableControl>().Distinct()) canvas.AutoScrollPosition = Point.Empty;
            foreach (Flow_Chart node in nodes.Values) node.ConfigurationRequested -= Configure;
            nodes.Clear();
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
                node.BringToFront();
            }
            ImportLegacyLinks();
            ApplyLinks();
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
            }
            Form.Invalidate(true);
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

        /// <summary>添加一个可配置节点，不生成应用 C# 回调。</summary>
        public FlowStep AddNode()
        {
            FlowPrograms.RequireIdle();
            if (Program.Removed) throw new InvalidOperationException(FlowText.T("moduleMissing"));
            Control parent = Enumerate(Form).Where(item => item.Visible && item.Parent != Form).Select(item => item.Parent).FirstOrDefault();
            if (parent == null) parent = (Control)Form.FlowChartTable.SelectedTab ?? Form;
            string parentPath = ParentPath(parent);
            Point scrollOffset = parent is ScrollableControl ? ((ScrollableControl)parent).AutoScrollPosition : Point.Empty;
            int y = nodes.Values.Where(existing => existing.Parent == parent).Select(existing => existing.Bottom - scrollOffset.Y + 40).DefaultIfEmpty(40).Max();
            FlowStep step = new FlowStep { ControlName = "flow_" + Guid.NewGuid().ToString("N"), Caption = "Step " + (Program.Steps.Count + 1), Action = "value", ParentPath = parentPath, X = 40, Y = y, AlarmCode = "2019", Branches = FlowBranch.Default() };
            step.Inputs.Add(new FlowInput { Name = "Value", Reference = "" });
            Program.Steps.Add(step);
            if (parent == Form.InitialPage && string.IsNullOrEmpty(Program.InitialEntry)) Program.InitialEntry = step.Id;
            if (parent != Form.InitialPage && string.IsNullOrEmpty(Program.AutoEntry)) Program.AutoEntry = step.Id;
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
                canvas.AutoScroll = true;
                canvas.AutoScrollMinSize = new Size(500, step.Y + node.Height + 40);
                canvas.ScrollControlIntoView(node);
            }
            return step;
        }

        /// <summary>删除选中节点并清理全部入口及连接。</summary>
        public void Delete(string id)
        {
            FlowPrograms.RequireIdle();
            Flow_Chart node = Node(id);
            if (node == null) return;
            FlowPrograms.ValidateNodeDeletion(this, id);
            node.ConfigurationRequested -= Configure;
            Program.RemovedControls.Add(node.Name);
            RemoveControl(node);
            nodes.Remove(id);
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

        /// <summary>双击任意 Flow_BaseForm 内的节点使用同一配置窗口。</summary>
        private void Configure(object sender, EventArgs args)
        {
            if (!editing) return;
            try
            {
                FlowPrograms.RequireIdle();
                FlowStep step = Program.Steps.Single(item => item.Id == IdOf((Flow_Chart)sender));
                using (FlowNodeForm dialog = new FlowNodeForm(this, step))
                    if (dialog.ShowDialog(Form) == DialogResult.OK) { ApplyLinks(); if (Changed != null) Changed(this, EventArgs.Empty); }
            }
            catch (Exception error) { MessageBox.Show(string.Format(FlowText.T("error"), error.Message), FlowText.T("title")); }
        }

        /// <summary>检查工程结构及已选择入口的真实资源、参数和动作。</summary>
        public void Validate(string entry)
        {
            ValidatePlan(Program, entry);
        }

        /// <summary>校验固定的执行计划，生产执行只使用已保存版本。</summary>
        private void ValidatePlan(FlowProgram plan, string entry)
        {
            List<string> errors = FlowProgramFile.Validate(plan);
            if (errors.Count > 0) throw new InvalidDataException(string.Join(Environment.NewLine, errors));
            if (plan.Mode != "Program" && entry == null) return;
            if (plan.Mode == "Program" && string.IsNullOrEmpty(plan.AutoEntry)) throw new InvalidDataException("Select an automatic entry: " + Form.Name);
            FlowActions actions = new FlowActions(FlowResources.Capture());
            IEnumerable<FlowStep> reachable = entry == null
                ? FlowProgramFile.Reachable(plan, plan.InitialEntry).Concat(FlowProgramFile.Reachable(plan, plan.AutoEntry))
                : FlowProgramFile.Reachable(plan, entry);
            foreach (FlowStep step in reachable) actions.Validate(step);
        }

        /// <summary>通过同一控制器处理所有派生窗体的生命周期。</summary>
        public bool Handle(FlowProgramCommand command)
        {
            if (!IsEnabled) return false;
            if (command == FlowProgramCommand.Stop) { Runtime.Stop(); return true; }
            if (command == FlowProgramCommand.Pause) { Runtime.Pause(); paused = true; return true; }
            if (command == FlowProgramCommand.Resume || (command == FlowProgramCommand.Start && paused && Runtime.IsRunning)) { Runtime.Resume(); paused = false; return true; }
            if (published.Mode == "Disabled") { Form.InitialStatus = true; Form.RunningStatus = false; return true; }
            Action start = () => Start(command == FlowProgramCommand.Initialize ? published.InitialEntry : published.AutoEntry, SysPara.Simulation, command == FlowProgramCommand.Initialize, true);
            if (MiddleLayer.MainF != null && MiddleLayer.MainF.InvokeRequired) MiddleLayer.MainF.Invoke(start); else start();
            return true;
        }

        /// <summary>用不可变参数及点位快照执行选定入口。</summary>
        public void Start(string entry, bool simulate, bool initializing, bool production = false)
        {
            if (Runtime.IsRunning) throw new InvalidOperationException(FlowText.T("busy"));
            SetEditing(false);
            FlowProgram plan = production ? published : Program;
            if (plan.Removed) throw new InvalidOperationException(FlowText.T("moduleMissing"));
            ValidatePlan(plan, entry);
            if (string.IsNullOrEmpty(entry)) { Form.InitialStatus = true; Form.RunningStatus = false; return; }
            FlowResources resources = FlowResources.Capture();
            FlowActions actions = new FlowActions(resources) { Simulate = simulate, RequestInput = FlowInputForm.Request };
            simulating = simulate;
            isInitializing = initializing;
            isProduction = production;
            Runtime.ExecuteAction = actions.Execute;
            Form.RunningStatus = true;
            if (initializing) Form.InitialStatus = false;
            Form.StopStatus = false;
            paused = false;
            foreach (Flow_Chart node in nodes.Values) node.ProgramManaged = true;
            Runtime.Start(plan, entry, new FlowExecutionContext { Hub = FlowPrograms.Results, RunId = FlowPrograms.Results.Epoch,
                CycleId = "batch", ModuleId = plan.ModuleId, RecipeName = resources.RecipeName, Parameters = resources.Parameters });
        }

        /// <summary>执行任务完成前更新模块状态，避免旧回调覆盖下一次启动。</summary>
        private void Finish()
        {
            Form.RunningStatus = false;
            foreach (Flow_Chart node in nodes.Values) node.ProgramManaged = IsEnabled;
            if (isInitializing) Form.InitialStatus = Runtime.Error == null;
            if (Runtime.Error != null) OperationLog.Write("Flow", Form.Name + ": " + Runtime.Error);
            if (isProduction && !Runtime.AlarmWasRaised && Runtime.Error != null && Runtime.Error != "Cancelled") Alarm.Show("2019", Form.Name + ": " + Runtime.Error);
        }

        /// <summary>切换到节点 UI 显示状态，语言切换不修改节点业务名称。</summary>
        private void DisplayState(FlowStep step, string state)
        {
            Flow_Chart node = Node(step.Id);
            if (node == null) return;
            Color color = state == "Running" ? Color.LightGreen : state == "Completed" ? Color.White : state == "Waiting" ? Color.LightYellow : Color.MistyRose;
            node.SetProgramState(FlowText.T(state), color);
        }

        /// <summary>在暂停与恢复间切换同一执行会话。</summary>
        public void TogglePause() { if (paused) Runtime.Resume(); else Runtime.Pause(); paused = !paused; }

        /// <summary>工程成功落盘后固定生产版本，草稿仅供工程测试。</summary>
        public void Publish() { published = FlowProgramFile.Snapshot(Program); ApplyLinks(); }

        /// <summary>生产启动前检查最后保存版本。</summary>
        public void ValidatePublished() { ValidatePlan(published, null); }
    }
}
