using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    /// <summary>编辑节点自己的源码和类型化端口，不执行脚本、不保存文件。</summary>
    public partial class FlowScriptEditor : UserControl
    {
        private sealed class PortType
        {
            public string Id { get; set; }
            public string Label { get { return FlowScriptUiText.T("script" + Id); } }
        }

        private FlowStep bound;
        private FlowActions catalog;
        private bool filling;
        private DataGridView contextGrid;
        private int contextIndex = -1;
        private readonly List<string> scriptResources = new List<string>();
        public event EventHandler DefinitionChanged;

        /// <summary>建立固定编辑控件，设计环境不捕获设备或编译代码。</summary>
        public FlowScriptEditor()
        {
            InitializeComponent();
            ConfigureGrid(inputPorts); ConfigureGrid(outputPorts);
            addInput.Click += (sender, args) => AddPort(inputPorts, "Input");
            addOutput.Click += (sender, args) => AddPort(outputPorts, "Output");
            compile.Click += CompileSource;
            portMenu.Opening += OpenPortMenu;
            deletePort.Click += DeletePort;
            objectChoice.SelectedIndexChanged += (sender, args) => FillMethods();
            insertCall.Click += InsertCall;
            removeBinding.Click += RemoveBinding;
            bindingMenu.Opening += (sender, args) => args.Cancel = resourceBindings.SelectedIndex < 0;
            if (UiTheme.IsDesignTime) return;
            MiddleLayer.LanguageChanged += RefreshLanguage;
        }

        /// <summary>为端口列绑定稳定类型，编辑结束后通知宿主刷新输入定义。</summary>
        private void ConfigureGrid(DataGridView grid)
        {
            DataGridViewComboBoxColumn type = (DataGridViewComboBoxColumn)grid.Columns["portType"];
            type.DisplayMember = "Label"; type.ValueMember = "Id";
            type.DataSource = new[] { "Boolean", "Integer", "Number", "Text" }.Select(id => new PortType { Id = id }).ToList();
            grid.CellValueChanged += PortChanged;
            grid.MouseDown += SelectPort;
            grid.CurrentCellDirtyStateChanged += (sender, args) => { if (grid.IsCurrentCellDirty && grid.CurrentCell is DataGridViewComboBoxCell) grid.CommitEdit(DataGridViewDataErrorContexts.Commit); };
        }

        /// <summary>绑定独立节点副本，保留已有源码；全新脚本提供最小输入输出示例。</summary>
        public void Bind(FlowStep step)
        {
            bound = FlowProgramFile.Snapshot(new FlowProgram { Steps = new List<FlowStep> { step } }).Steps[0];
            scriptResources.Clear(); scriptResources.AddRange(bound.ScriptResources);
            filling = true;
            try
            {
                bool empty = string.IsNullOrWhiteSpace(bound.ScriptSource) && bound.ScriptInputs.Count == 0 && bound.ScriptOutputs.Count == 0;
                source.Text = empty ? "Outputs.Value = Inputs.Value;" : bound.ScriptSource ?? "";
                inputPorts.Rows.Clear(); outputPorts.Rows.Clear();
                if (empty)
                {
                    inputPorts.Rows.Add("Value", "Number", "0"); outputPorts.Rows.Add("Value", "Number", "0");
                }
                else
                {
                    foreach (FlowScriptPort port in bound.ScriptInputs) inputPorts.Rows.Add(port.Name, port.Type, port.DefaultValue);
                    foreach (FlowScriptPort port in bound.ScriptOutputs) outputPorts.Rows.Add(port.Name, port.Type, port.DefaultValue);
                }
                errors.Clear();
            }
            finally { filling = false; }
            RefreshLanguage(this, EventArgs.Empty);
            RefreshBindings();
        }

        /// <summary>向传入副本收集源码与端口，不更改绑定节点或原工程。</summary>
        public void ReadTo(FlowStep step)
        {
            inputPorts.EndEdit(); outputPorts.EndEdit();
            step.ScriptSource = source.Text;
            step.ScriptInputs = ReadPorts(inputPorts);
            step.ScriptOutputs = ReadPorts(outputPorts);
            step.ScriptResources = scriptResources.ToList();
        }

        /// <summary>绑定浏览目录仅用于插入调用，不连接或执行实际对象。</summary>
        internal void BindObjects(FlowActions actions)
        {
            catalog = actions;
            string current = Convert.ToString(objectChoice.SelectedValue);
            objectChoice.DisplayMember = "Label"; objectChoice.ValueMember = "Id";
            objectChoice.DataSource = actions.Resources.Items.Values.Where(item => item.Selectable && item.Methods.Count > 0).Select(item => new FlowChoice { Id = item.Id, Label = item.Label }).ToList();
            if (!string.IsNullOrEmpty(current)) objectChoice.SelectedValue = current;
            FillMethods();
            RefreshBindings();
        }

        /// <summary>显示端口定义或编译错误，保留当前输入供修正。</summary>
        internal void ShowError(string message) { errors.Text = message ?? ""; }

        /// <summary>读取每个端口的稳定字段，验证由脚本服务统一完成。</summary>
        private static List<FlowScriptPort> ReadPorts(DataGridView grid)
        {
            return grid.Rows.Cast<DataGridViewRow>().Where(row => !row.IsNewRow).Select(row => new FlowScriptPort {
                Name = Convert.ToString(row.Cells["portName"].Value), Type = Convert.ToString(row.Cells["portType"].Value), DefaultValue = Convert.ToString(row.Cells["portDefault"].Value)
            }).ToList();
        }

        /// <summary>新增有唯一初始名称的端口，数字类型默认从零开始。</summary>
        private void AddPort(DataGridView grid, string prefix)
        {
            int index = 1;
            HashSet<string> names = new HashSet<string>(ReadPorts(grid).Select(port => port.Name), StringComparer.Ordinal);
            while (names.Contains(prefix + index)) index++;
            int added = grid.Rows.Add(prefix + index, "Number", "0");
            grid.CurrentCell = grid.Rows[added].Cells["portName"];
            NotifyDefinitionChanged();
        }

        /// <summary>端口提交后更新方法目录，源码每次按键不触发重建或编译。</summary>
        private void PortChanged(object sender, DataGridViewCellEventArgs args)
        {
            if (filling || args.RowIndex < 0) return;
            NotifyDefinitionChanged();
        }

        /// <summary>只记录右键命中的端口行，空白区域不复用旧目标。</summary>
        private void SelectPort(object sender, MouseEventArgs args)
        {
            if (args.Button != MouseButtons.Right) return;
            contextGrid = (DataGridView)sender; contextIndex = contextGrid.HitTest(args.X, args.Y).RowIndex;
            if (contextIndex < 0) return;
            contextGrid.CurrentCell = contextGrid.Rows[contextIndex].Cells[0];
        }

        /// <summary>只允许删除本次右键命中的有效端口行。</summary>
        private void OpenPortMenu(object sender, CancelEventArgs args)
        {
            args.Cancel = contextGrid == null || contextGrid != portMenu.SourceControl || contextIndex < 0 || contextIndex >= contextGrid.Rows.Count;
        }

        /// <summary>删除端口草稿并通知宿主，原节点仍可通过取消完整保留。</summary>
        private void DeletePort(object sender, EventArgs args)
        {
            if (contextGrid == null || contextIndex < 0 || contextIndex >= contextGrid.Rows.Count) return;
            contextGrid.Rows.RemoveAt(contextIndex); contextIndex = -1; NotifyDefinitionChanged();
        }

        /// <summary>统一通知宿主读取已结束编辑的端口副本。</summary>
        private void NotifyDefinitionChanged()
        {
            if (filling) return;
            EventHandler handler = DefinitionChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>编译节点副本并显示含行号的诊断；成功不会保存或执行脚本。</summary>
        private void CompileSource(object sender, EventArgs args)
        {
            if (bound == null) return;
            try
            {
                FlowStep candidate = FlowProgramFile.Snapshot(new FlowProgram { Steps = new List<FlowStep> { bound } }).Steps[0];
                candidate.ExecutionMode = "Script"; ReadTo(candidate); FlowScripts.Compile(candidate);
                errors.Text = FlowScriptUiText.T("scriptCompiled") + Environment.NewLine + string.Format(FlowScriptUiText.T("scriptSourcePath"), FlowScripts.GetSourcePath(candidate));
            }
            catch (Exception error) { errors.Text = error.Message; }
        }

        /// <summary>列出所选真实对象自己的能力，不根据显示名替换对象。</summary>
        private void FillMethods()
        {
            if (catalog == null) return;
            FlowResource resource = catalog.FindResource(Convert.ToString(objectChoice.SelectedValue));
            methodChoice.FormattingEnabled = true;
            methodChoice.DataSource = resource == null ? new List<FlowMethod>() : resource.Methods;
            insertCall.Enabled = methodChoice.Items.Count > 0;
        }

        /// <summary>把目录生成的调用插入光标，默认参数仅供工程师按工艺修改。</summary>
        private void InsertCall(object sender, EventArgs args)
        {
            FlowMethod method = methodChoice.SelectedItem as FlowMethod;
            if (method == null) return;
            string target = Convert.ToString(objectChoice.SelectedValue);
            if (!scriptResources.Contains(target)) { scriptResources.Add(target); RefreshBindings(); }
            string arguments = string.Join(", ", method.Inputs.Select(field => "{ " + Quote(field.Name) + ", " + DefaultLiteral(field) + " }"));
            int suffix = 1;
            while (source.Text.Contains("callResult" + suffix)) suffix++;
            source.SelectedText = "var callResult" + suffix + " = await Context.CallAsync(" + Quote(target) + ", " + Quote(method.Id) + ", new Dictionary<string, object> { " + arguments + " });" + Environment.NewLine;
            errors.Text = FlowScriptUiText.T("scriptInsertHelp"); source.Focus();
        }

        /// <summary>显示脚本已声明的实际对象，缺失对象保留身份供修正。</summary>
        private void RefreshBindings()
        {
            resourceBindings.DisplayMember = "Label"; resourceBindings.ValueMember = "Id";
            resourceBindings.DataSource = scriptResources.Select(id => { FlowResource resource = catalog == null ? null : catalog.FindResource(id); return new FlowChoice { Id = id, Label = resource == null ? id : resource.Label }; }).ToList();
        }

        /// <summary>移除对象声明不改写用户源码，执行服务将拒绝未声明对象的调用。</summary>
        private void RemoveBinding(object sender, EventArgs args)
        {
            string id = Convert.ToString(resourceBindings.SelectedValue);
            if (resourceBindings.SelectedIndex < 0) return;
            scriptResources.Remove(id); RefreshBindings(); errors.Text = FlowScriptUiText.T("scriptRemovedObject");
        }

        /// <summary>根据声明默认值生成类型明确的 C# 字面值，复杂类型保留默认值表达式。</summary>
        private static string DefaultLiteral(FlowField field)
        {
            if (field.Type == typeof(string)) return Quote(field.DefaultValue ?? "");
            if (field.Type.IsEnum || field.Type == typeof(object)) return Quote(field.DefaultValue ?? "");
            if (field.Type == typeof(bool)) return string.Equals(field.DefaultValue, "true", StringComparison.OrdinalIgnoreCase) || field.DefaultValue == "1" ? "true" : "false";
            try
            {
                object value = field.ConvertValue(field.DefaultValue);
                string text = Convert.ToString(value, CultureInfo.InvariantCulture);
                if (field.Type == typeof(double)) return text + (text.Contains(".") || text.Contains("E") || text.Contains("e") ? "d" : ".0d");
                if (field.Type == typeof(float)) return text + "f";
                if (field.Type == typeof(decimal)) return text + "m";
                if (field.Type == typeof(long)) return text + "L";
                if (field.Type.IsPrimitive && field.Type != typeof(char)) return text;
            }
            catch (Exception error) when (error is FormatException || error is OverflowException || error is InvalidCastException || error is InvalidOperationException || error is ArgumentException) { }
            return "null";
        }

        /// <summary>把目录身份转成安全的 C# 字符串字面值。</summary>
        private static string Quote(string text) { return "\"" + (text ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n") + "\""; }

        /// <summary>刷新固定标题与类型显示，不重建端口行、不翻译源码。</summary>
        private void RefreshLanguage(object sender, EventArgs args)
        {
            FlowLanguageForm.ApplyTexts(this); inputPorts.Invalidate(); outputPorts.Invalidate();
            methodChoice.Format -= FormatMethod; methodChoice.Format += FormatMethod; methodChoice.Refresh();
            if (catalog == null) return;
            FlowMethod selected = methodChoice.SelectedItem as FlowMethod;
            string methodId = selected == null ? null : selected.Id;
            BindObjects(catalog);
            if (methodId != null) methodChoice.SelectedItem = methodChoice.Items.Cast<FlowMethod>().FirstOrDefault(method => method.Id == methodId);
        }

        /// <summary>使用对象方法的三语标题显示插入目录。</summary>
        private static void FormatMethod(object sender, ListControlConvertEventArgs args)
        {
            FlowMethod method = args.ListItem as FlowMethod;
            if (method != null) args.Value = FlowMethods.Title(method);
        }

        /// <summary>释放控件时解除框架语言事件。</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && !UiTheme.IsDesignTime) MiddleLayer.LanguageChanged -= RefreshLanguage;
            base.Dispose(disposing);
        }
    }
}
