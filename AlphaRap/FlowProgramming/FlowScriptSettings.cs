using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    /// <summary>管理脚本端口和已声明对象；与源码页处于同级，不执行对象方法。</summary>
    public partial class FlowScriptSettings : UserControl
    {
        private sealed class PortType
        {
            public string Id { get; set; }
            public string Label { get { return FlowScriptUiText.T("script" + Id); } }
        }

        private FlowActions catalog;
        private bool filling;
        private DataGridView contextGrid;
        private int contextIndex = -1;
        private readonly List<string> scriptResources = new List<string>();
        public event EventHandler DefinitionChanged;
        internal event Action<string, FlowMethod> InsertRequested;
        internal event Action<string> MessageRequested;

        /// <summary>建立端口与对象设置控件，设计时不访问宿主对象或硬件。</summary>
        public FlowScriptSettings()
        {
            InitializeComponent();
            ConfigureGrid(inputPorts); ConfigureGrid(outputPorts);
            addInput.Click += (sender, args) => AddPort(inputPorts, "Input");
            addOutput.Click += (sender, args) => AddPort(outputPorts, "Output");
            portMenu.Opening += OpenPortMenu;
            deletePort.Click += DeletePort;
            objectChoice.SelectedIndexChanged += (sender, args) => { if (!filling) FillMethods(null); };
            methodChoice.SelectedIndexChanged += (sender, args) => insertCall.Enabled = methodChoice.SelectedItem is FlowMethod;
            methodChoice.Format += FormatMethod;
            insertCall.Click += RequestInsert;
            removeBinding.Click += RemoveBinding;
            bindingMenu.Opening += (sender, args) => args.Cancel = resourceBindings.SelectedIndex < 0;
            if (!UiTheme.IsDesignTime) MiddleLayer.LanguageChanged += RefreshLanguage;
        }

        /// <summary>为端口列绑定稳定类型，编辑结束后通知宿主刷新元数据。</summary>
        private void ConfigureGrid(DataGridView grid)
        {
            DataGridViewComboBoxColumn type = (DataGridViewComboBoxColumn)grid.Columns["portType"];
            type.DisplayMember = "Label"; type.ValueMember = "Id";
            type.DataSource = new[] { "Boolean", "Integer", "Number", "Text" }.Select(id => new PortType { Id = id }).ToList();
            grid.CellValueChanged += PortChanged;
            grid.MouseDown += SelectPort;
            grid.CurrentCellDirtyStateChanged += (sender, args) => { if (grid.IsCurrentCellDirty && grid.CurrentCell is DataGridViewComboBoxCell) grid.CommitEdit(DataGridViewDataErrorContexts.Commit); };
        }

        /// <summary>绑定节点端口和对象白名单，取消窗口不会修改传入节点。</summary>
        public void Bind(FlowStep step)
        {
            filling = true;
            try
            {
                scriptResources.Clear();
                scriptResources.AddRange(step.ScriptResources ?? new List<string>());
                inputPorts.Rows.Clear(); outputPorts.Rows.Clear();
                bool empty = string.IsNullOrWhiteSpace(step.ScriptSource) && step.ScriptInputs.Count == 0 && step.ScriptOutputs.Count == 0;
                if (empty)
                {
                    inputPorts.Rows.Add("Value", "Number", "0"); outputPorts.Rows.Add("Value", "Number", "0");
                }
                else
                {
                    foreach (FlowScriptPort port in step.ScriptInputs) inputPorts.Rows.Add(port.Name, port.Type, port.DefaultValue);
                    foreach (FlowScriptPort port in step.ScriptOutputs) outputPorts.Rows.Add(port.Name, port.Type, port.DefaultValue);
                }
                settingsHint.Text = FlowScriptUiText.T("scriptSettingsHelp");
                RefreshBindings();
            }
            finally { filling = false; }
            RefreshLanguage(this, EventArgs.Empty);
        }

        /// <summary>将端口及完整对象白名单复制到候选节点，不触碰源码或持久化文件。</summary>
        public void ReadTo(FlowStep step)
        {
            inputPorts.EndEdit(); outputPorts.EndEdit();
            step.ScriptInputs = ReadPorts(inputPorts); step.ScriptOutputs = ReadPorts(outputPorts);
            step.ScriptResources = scriptResources.ToList();
        }

        /// <summary>绑定只读对象目录；默认不选对象和方法，旧授权清单始终保留。</summary>
        internal void BindObjects(FlowActions actions)
        {
            catalog = actions;
            string current = objectChoice.SelectedIndex < 0 ? null : Convert.ToString(objectChoice.SelectedValue);
            FlowMethod method = methodChoice.SelectedItem as FlowMethod;
            string selectedMethod = method == null ? null : method.Id;
            filling = true;
            try
            {
                objectChoice.DisplayMember = "Label"; objectChoice.ValueMember = "Id";
                objectChoice.DataSource = actions.Resources.Items.Values.Where(item => item.Selectable && item.Methods.Count > 0).Select(item => new FlowChoice { Id = item.Id, Label = item.Label }).ToList();
                objectChoice.SelectedIndex = -1;
                if (current != null) objectChoice.SelectedValue = current;
                FillMethods(selectedMethod); RefreshBindings();
            }
            finally { filling = false; }
        }

        /// <summary>读取每个端口的稳定字段，字段合法性由脚本服务统一校验。</summary>
        private static List<FlowScriptPort> ReadPorts(DataGridView grid)
        {
            return grid.Rows.Cast<DataGridViewRow>().Where(row => !row.IsNewRow).Select(row => new FlowScriptPort {
                Name = Convert.ToString(row.Cells["portName"].Value), Type = Convert.ToString(row.Cells["portType"].Value), DefaultValue = Convert.ToString(row.Cells["portDefault"].Value)
            }).ToList();
        }

        /// <summary>新增唯一初始名称的数值端口，不改变其他参数绑定。</summary>
        private void AddPort(DataGridView grid, string prefix)
        {
            int index = 1;
            HashSet<string> names = new HashSet<string>(ReadPorts(grid).Select(port => port.Name), StringComparer.Ordinal);
            while (names.Contains(prefix + index)) index++;
            filling = true;
            int added;
            try { added = grid.Rows.Add(prefix + index, "Number", "0"); }
            finally { filling = false; }
            grid.CurrentCell = grid.Rows[added].Cells["portName"];
            NotifyDefinitionChanged();
        }

        /// <summary>端口单元格确认修改后通知宿主，源码每次按键不触发参数重建。</summary>
        private void PortChanged(object sender, DataGridViewCellEventArgs args)
        {
            if (filling || args.RowIndex < 0) return;
            NotifyDefinitionChanged();
        }

        /// <summary>记录右键实际命中的行，表头和空白区清除旧目标。</summary>
        private void SelectPort(object sender, MouseEventArgs args)
        {
            if (args.Button != MouseButtons.Right) return;
            contextGrid = (DataGridView)sender; contextIndex = contextGrid.HitTest(args.X, args.Y).RowIndex;
            if (contextIndex >= 0) contextGrid.CurrentCell = contextGrid.Rows[contextIndex].Cells[0];
        }

        /// <summary>只有本次右键命中的有效端口行显示删除菜单。</summary>
        private void OpenPortMenu(object sender, CancelEventArgs args)
        {
            args.Cancel = contextGrid == null || contextGrid != portMenu.SourceControl || contextIndex < 0 || contextIndex >= contextGrid.Rows.Count;
        }

        /// <summary>删除当前端口草稿，其他行和源码保持原值。</summary>
        private void DeletePort(object sender, EventArgs args)
        {
            if (contextGrid == null || contextIndex < 0 || contextIndex >= contextGrid.Rows.Count) return;
            contextGrid.Rows.RemoveAt(contextIndex); contextIndex = -1; NotifyDefinitionChanged();
        }

        /// <summary>通知源码编辑器转发定义修改，持久化继续由主保存负责。</summary>
        private void NotifyDefinitionChanged()
        {
            if (filling) return;
            EventHandler handler = DefinitionChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>为选定实际对象显示其能力目录，不自动采用第一项动作。</summary>
        private void FillMethods(string selected)
        {
            FlowResource resource = catalog == null ? null : catalog.FindResource(Convert.ToString(objectChoice.SelectedValue));
            methodChoice.DataSource = resource == null ? new List<FlowMethod>() : resource.Methods;
            methodChoice.SelectedIndex = -1;
            if (selected != null) methodChoice.SelectedItem = methodChoice.Items.Cast<FlowMethod>().FirstOrDefault(method => method.Id == selected);
            insertCall.Enabled = methodChoice.SelectedItem is FlowMethod;
        }

        /// <summary>先声明所选对象，再请求源码页插入方法；不会执行对象或创建实例。</summary>
        private void RequestInsert(object sender, EventArgs args)
        {
            FlowMethod method = methodChoice.SelectedItem as FlowMethod;
            if (method == null || objectChoice.SelectedIndex < 0) return;
            Action<string, FlowMethod> handler = InsertRequested;
            if (handler == null) return;
            string target = Convert.ToString(objectChoice.SelectedValue);
            if (!scriptResources.Contains(target)) { scriptResources.Add(target); RefreshBindings(); }
            handler(target, method);
        }

        /// <summary>显示所有已授权对象，失效对象保留原身份供工程师修复。</summary>
        private void RefreshBindings()
        {
            string current = resourceBindings.SelectedIndex < 0 ? null : Convert.ToString(resourceBindings.SelectedValue);
            resourceBindings.DisplayMember = "Label"; resourceBindings.ValueMember = "Id";
            resourceBindings.DataSource = scriptResources.Select(id => { FlowResource resource = catalog == null ? null : catalog.FindResource(id); return new FlowChoice { Id = id, Label = resource == null ? id : resource.Label }; }).ToList();
            if (current != null) resourceBindings.SelectedValue = current;
        }

        /// <summary>移除对象授权但不重写源码，原调用将在运行前明确校验。</summary>
        private void RemoveBinding(object sender, EventArgs args)
        {
            if (resourceBindings.SelectedIndex < 0) return;
            string id = Convert.ToString(resourceBindings.SelectedValue);
            scriptResources.Remove(id); RefreshBindings();
            string message = FlowScriptUiText.T("scriptRemovedObject"); settingsHint.Text = message;
            Action<string> handler = MessageRequested;
            if (handler != null) handler(message);
        }

        /// <summary>刷新固定标题与类型文字，保留编辑行和对象方法选择。</summary>
        private void RefreshLanguage(object sender, EventArgs args)
        {
            FlowLanguageForm.ApplyTexts(this); inputPorts.Invalidate(); outputPorts.Invalidate();
            settingsHint.Text = FlowScriptUiText.T("scriptSettingsHelp");
            if (catalog != null) BindObjects(catalog);
        }

        /// <summary>显示对象方法的当前语言标题，内部方法身份保持不变。</summary>
        private static void FormatMethod(object sender, ListControlConvertEventArgs args)
        {
            FlowMethod method = args.ListItem as FlowMethod;
            if (method != null) args.Value = FlowMethods.Title(method);
        }

        /// <summary>解除语言事件，源码编辑器拥有的事件由它自行解除。</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && !UiTheme.IsDesignTime) MiddleLayer.LanguageChanged -= RefreshLanguage;
            base.Dispose(disposing);
        }
    }
}
