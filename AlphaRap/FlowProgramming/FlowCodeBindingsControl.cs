using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    /// <summary>以一张表绑定代码模块的实际资源及 VPP 业务字段。</summary>
    public partial class FlowCodeBindingsControl : UserControl
    {
        private sealed class BindingRow
        {
            internal FlowResourceBinding Binding;
            internal FlowCodeResourceSlot Slot;
            internal FlowCodeResourceFieldDefinition Field;
            internal FlowResourceFieldBinding Mapping;
        }

        private FlowCodeDefinition definition;
        private FlowActions catalog;
        private List<FlowResourceBinding> bindings = new List<FlowResourceBinding>();
        private bool filling, languageSubscribed, notifyingBindings;
        private BindingRow contextRow;
        public event EventHandler BindingsChanged;
        internal int PreferredContentHeight { get { return grid.Rows.Count == 0 ? 50 : Math.Min(230, grid.ColumnHeadersHeight + grid.Rows.Cast<DataGridViewRow>().Sum(row => row.Height) + 4); } }

        /// <summary>创建固定资源表模板，设计期不读取资源或模块。</summary>
        public FlowCodeBindingsControl()
        {
            InitializeComponent();
            if (UiTheme.IsDesignTime) return;
            grid.CurrentCellDirtyStateChanged += (sender, args) => { if (grid.IsCurrentCellDirty) grid.CommitEdit(DataGridViewDataErrorContexts.Commit); };
            grid.CellValueChanged += BindingChanged;
            grid.DataError += (sender, args) => args.ThrowException = false;
            bindingMenu.Opening += (sender, args) => {
                args.Cancel = contextRow == null;
            };
            grid.MouseDown += SelectContextRow;
            clearBinding.Click += ClearBinding;
            MiddleLayer.LanguageChanged += LanguageChanged; languageSubscribed = true;
            RefreshLanguage();
        }

        /// <summary>复制节点的完整绑定，失效资源和未知旧字段不会被自动删除。</summary>
        internal void Bind(FlowCodeDefinition value, FlowStep step, FlowActions actions)
        {
            definition = value; catalog = actions; bindings = Copy(step.ResourceBindings);
            if (definition != null)
                foreach (FlowCodeResourceSlot slot in definition.ResourceSlots)
                    if (bindings.All(binding => binding.Slot != slot.Name)) bindings.Add(new FlowResourceBinding { Slot = slot.Name });
            RefreshRows();
        }

        /// <summary>复制资源与字段映射，轴表指纹由宿主确认流程维护。</summary>
        private static List<FlowResourceBinding> Copy(IEnumerable<FlowResourceBinding> source)
        {
            return (source ?? new FlowResourceBinding[0]).Select(binding => new FlowResourceBinding { Slot = binding.Slot, Target = binding.Target,
                PointTableSignature = binding.PointTableSignature, Fields = (binding.Fields ?? new List<FlowResourceFieldBinding>()).Select(field => new FlowResourceFieldBinding { Name = field.Name, Field = field.Field, Direction = field.Direction }).ToList() }).ToList();
        }

        /// <summary>收集当前窗口的资源草稿，不修改实际设备或工程原节点。</summary>
        internal void ReadTo(FlowStep step, bool confirmPoints = false)
        {
            if (!notifyingBindings) grid.EndEdit();
            step.ResourceBindings = Copy(bindings);
            if (!confirmPoints || catalog == null) return;
            foreach (FlowResourceBinding binding in step.ResourceBindings)
            {
                FlowResource resource = catalog.FindResource(binding.Target);
                if (resource != null && resource.Kind == "axis") FlowResourceAccess.ConfirmPoints(binding, resource, catalog.Resources.RecipeName);
            }
        }

        /// <summary>按声明显示资源及映射，原来的未知绑定继续可见供修正。</summary>
        private void RefreshRows()
        {
            filling = true;
            try
            {
                contextRow = null; grid.Rows.Clear();
                if (definition != null)
                    foreach (FlowCodeResourceSlot slot in definition.ResourceSlots) AddSlotRows(slot);
                foreach (FlowResourceBinding binding in bindings.Where(binding => definition == null || definition.ResourceSlots.All(slot => slot.Name != binding.Slot)))
                    AddOldSlotRows(binding);
                emptyHint.Visible = grid.Rows.Count == 0;
                emptyHint.Text = FlowCodeUiText.T(definition == null ? "missingDefinition" : "noResource");
                grid.Visible = grid.Rows.Count > 0;
            }
            finally { filling = false; }
        }

        /// <summary>通知角色变化时允许宿主读取已提交的绑定副本，不重入表格结束编辑。</summary>
        private void NotifyBindingsChanged()
        {
            notifyingBindings = true;
            try { if (BindingsChanged != null) BindingsChanged(this, EventArgs.Empty); }
            finally { notifyingBindings = false; }
        }

        /// <summary>已移除的资源声明及字段保持可见，允许逐项清除旧映射。</summary>
        private void AddOldSlotRows(FlowResourceBinding binding)
        {
            AddRow(binding.Slot, new BindingRow { Binding = binding }, new List<FlowChoice>(), binding.Target, FlowCodeUiText.T("oldBinding"));
            foreach (FlowResourceFieldBinding field in binding.Fields)
                AddRow("  " + field.Name, new BindingRow { Binding = binding, Mapping = field }, new List<FlowChoice>(), field.Field, FlowCodeUiText.T("oldBinding"));
        }

        /// <summary>一行选择实际对象，随后的扁平行选择该 VPP 的真实输入输出字段。</summary>
        private void AddSlotRows(FlowCodeResourceSlot slot)
        {
            FlowResourceBinding binding = bindings.First(item => item.Slot == slot.Name);
            string title = FlowCodeUiText.Title(definition.Id + ".slot." + slot.Name, slot.Title, slot.Name);
            List<FlowChoice> resources = catalog == null ? new List<FlowChoice>() : catalog.Resources.Items.Values.Where(resource => resource.Selectable && FlowResourceAccess.Matches(slot.Capability, resource))
                .Select(resource => new FlowChoice { Id = resource.Id, Label = resource.Label }).ToList();
            AddRow(title, new BindingRow { Binding = binding, Slot = slot }, resources, binding.Target, FlowCodeUiText.T(slot.Required ? "required" : "optional"));
            foreach (FlowCodeResourceFieldDefinition field in slot.Fields)
            {
                FlowResourceFieldBinding mapping = binding.Fields.FirstOrDefault(item => item.Name == field.Name && item.Direction == field.Direction);
                if (mapping == null) { mapping = new FlowResourceFieldBinding { Name = field.Name, Direction = field.Direction }; binding.Fields.Add(mapping); }
                string fieldTitle = FlowCodeUiText.Title(definition.Id + ".slot." + slot.Name + "." + field.Name, field.Title, field.Name);
                AddRow("  " + fieldTitle, new BindingRow { Binding = binding, Slot = slot, Field = field, Mapping = mapping }, FieldChoices(binding, field), mapping.Field,
                    FlowCodeUiText.T(field.Direction == "Input" ? "fieldInput" : "fieldOutput"));
            }
            foreach (FlowResourceFieldBinding old in binding.Fields.Where(item => slot.Fields.All(field => field.Name != item.Name || field.Direction != item.Direction)))
                AddRow("  " + old.Name, new BindingRow { Binding = binding, Slot = slot, Mapping = old }, new List<FlowChoice>(), old.Field, FlowCodeUiText.T("oldBinding"));
        }

        /// <summary>仅推荐已绑定 VPP 中方向与类型匹配的业务终端，不列运行编号等框架字段。</summary>
        private List<FlowChoice> FieldChoices(FlowResourceBinding binding, FlowCodeResourceFieldDefinition field)
        {
            FlowResource resource = catalog == null ? null : catalog.FindResource(binding.Target);
            FlowMethod run = resource == null || resource.Kind != "vision" ? null : resource.Methods.FirstOrDefault(method => method.Id == "vision.run");
            if (run == null) return new List<FlowChoice>();
            IEnumerable<FlowField> fields = field.Direction == "Input" ? run.Inputs : run.Outputs;
            return fields.Where(item => item.Name.StartsWith("TB:", StringComparison.Ordinal) && FlowResourceAccess.Compatible(item.Type, field.Type)).Select(item => new FlowChoice {
                Id = item.Name, Label = FlowMethods.FieldTitle(run.Id, item, field.Direction != "Input") }).ToList();
        }

        /// <summary>保留原值的组合框行，缺失引用会明确显示而不回退到首项。</summary>
        private void AddRow(string title, BindingRow binding, List<FlowChoice> choices, string selected, string details)
        {
            int index = grid.Rows.Add(title, null, details);
            DataGridViewRow row = grid.Rows[index]; row.Tag = binding;
            SetChoices((DataGridViewComboBoxCell)row.Cells[1], choices, selected);
        }

        /// <summary>替换一行可选字段且保留失效引用，不结束或重建正在编辑的资源行。</summary>
        private static void SetChoices(DataGridViewComboBoxCell cell, List<FlowChoice> choices, string selected)
        {
            choices.Insert(0, new FlowChoice { Id = "", Label = FlowCodeUiText.T("choose") });
            if (!string.IsNullOrEmpty(selected) && choices.All(choice => choice.Id != selected))
                choices.Add(new FlowChoice { Id = selected, Label = string.Format(FlowCodeUiText.T("missing"), selected) });
            cell.DisplayMember = "Label"; cell.ValueMember = "Id"; cell.DataSource = choices; cell.Value = selected ?? "";
        }

        /// <summary>绑定改变只更新窗口副本；更换对象保留旧字段供逐项修正。</summary>
        private void BindingChanged(object sender, DataGridViewCellEventArgs args)
        {
            if (filling || args.RowIndex < 0 || args.ColumnIndex != 1) return;
            BindingRow row = (BindingRow)grid.Rows[args.RowIndex].Tag;
            string value = Convert.ToString(grid.Rows[args.RowIndex].Cells[1].Value);
            if (row.Mapping != null) { row.Mapping.Field = value; return; }
            if (row.Binding.Target == value) return;
            row.Binding.Target = value; row.Binding.PointTableSignature = null;
            filling = true;
            try
            {
                foreach (DataGridViewRow gridRow in grid.Rows)
                {
                    BindingRow mapping = (BindingRow)gridRow.Tag;
                    if (mapping.Binding != row.Binding || mapping.Field == null) continue;
                    SetChoices((DataGridViewComboBoxCell)gridRow.Cells[1], FieldChoices(mapping.Binding, mapping.Field), mapping.Mapping.Field);
                }
            }
            finally { filling = false; }
            NotifyBindingsChanged();
        }

        /// <summary>右键只作用于实际命中的绑定行，不使用上次选择。</summary>
        private void SelectContextRow(object sender, MouseEventArgs args)
        {
            if (args.Button != MouseButtons.Right) return;
            contextRow = null;
            DataGridView.HitTestInfo hit = grid.HitTest(args.X, args.Y);
            if (hit.RowIndex < 0) return;
            grid.CurrentCell = grid.Rows[hit.RowIndex].Cells[1];
            contextRow = grid.Rows[hit.RowIndex].Tag as BindingRow;
        }

        /// <summary>显式清除所选引用，未知旧槽可在确认后移除。</summary>
        private void ClearBinding(object sender, EventArgs args)
        {
            if (contextRow == null) return;
            if (contextRow.Mapping != null && contextRow.Field == null) contextRow.Binding.Fields.Remove(contextRow.Mapping);
            else if (contextRow.Mapping != null) contextRow.Mapping.Field = "";
            else if (contextRow.Slot == null) bindings.Remove(contextRow.Binding);
            else { contextRow.Binding.Target = ""; contextRow.Binding.PointTableSignature = null; }
            contextRow = null; RefreshRows(); NotifyBindingsChanged();
        }

        /// <summary>只更新显示和选择目录，资源身份及窗口内映射草稿保持不变。</summary>
        public void RefreshLanguage()
        {
            grid.EndEdit(); roleColumn.HeaderText = FlowCodeUiText.T("resourceRole"); bindingColumn.HeaderText = FlowCodeUiText.T("resourceBinding");
            statusColumn.HeaderText = FlowCodeUiText.T("bindingStatus"); clearBinding.Text = FlowCodeUiText.T("clearBinding"); RefreshRows();
        }

        /// <summary>响应三语切换，不写入节点或设备。</summary>
        private void LanguageChanged(object sender, EventArgs args) { RefreshLanguage(); }

        /// <summary>释放语言订阅和右键菜单。</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (languageSubscribed) { MiddleLayer.LanguageChanged -= LanguageChanged; languageSubscribed = false; }
                if (components != null) components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
