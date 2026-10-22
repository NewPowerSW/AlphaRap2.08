using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    public partial class FlowNodeForm : FlowLanguageForm
    {
        private readonly FlowModuleEditor module;
        private readonly FlowStep original;
        private readonly FlowActions catalog;
        private bool filling;

        /// <summary>Build the fixed controls without loading application data.</summary>
        public FlowNodeForm() { InitializeComponent(); }

        /// <summary>编辑节点副本，取消时不改变原工程和已有输入。</summary>
        public FlowNodeForm(FlowModuleEditor owner, FlowStep step) : this()
        {
            module = owner;
            original = step;
            catalog = new FlowActions(FlowResources.Capture());
            filling = true;
            captionValue.Text = step.Caption;
            timeoutValue.Value = Math.Max(timeoutValue.Minimum, Math.Min(timeoutValue.Maximum, step.TimeoutMs));
            wait.Checked = step.WaitForCondition;
            any.Checked = step.AnyCondition;
            SetSources(inputs, 1);
            SetSources(conditions, 0);
            SetSources(conditions, 4);
            categoryValue.DataSource = new[] { "flow", "io", "motion", "vision", "device", "data" }
                .Select(id => new FlowChoice { Id = id, Label = FlowText.T(id) }).ToList();
            categoryValue.DisplayMember = "Label"; categoryValue.ValueMember = "Id";
            foreach (ComboBox combo in new[] { nextValue, otherwiseValue, failureValue }) FillDestinations(combo);
            nextValue.SelectedValue = step.Next ?? "";
            otherwiseValue.SelectedValue = step.Otherwise ?? "";
            failureValue.SelectedValue = step.Failure ?? "";
            FillAlarms();
            alarmValue.Text = step.AlarmCode ?? "";
            FlowAction action = catalog.Actions.SingleOrDefault(item => item.Id == step.Action);
            categoryValue.SelectedValue = action == null ? "flow" : action.Category;
            FillTargets(step.Target);
            FillActions(step.Action);
            FillInputs(step.Inputs);
            foreach (FlowCondition condition in step.Conditions)
                conditions.Rows.Add(condition.Left.Source, condition.Left.Reference, "…", condition.Operator, condition.Right.Source, condition.Right.Reference, "…");
            filling = false;
            categoryValue.SelectedIndexChanged += CategoryChanged;
            targetValue.SelectedIndexChanged += TargetChanged;
            actionValue.SelectedIndexChanged += ActionChanged;
            inputs.CellContentClick += InputPick;
            inputs.CellDoubleClick += InputPick;
            inputs.CellFormatting += FormatReference;
            conditions.CellFormatting += FormatReference;
            inputs.CellBeginEdit += GuardReferenceEdit;
            conditions.CellBeginEdit += GuardReferenceEdit;
            conditions.CellContentClick += ConditionPick;
            conditions.CellDoubleClick += ConditionPick;
            conditions.DefaultValuesNeeded += (sender, args) => { args.Row.Cells[0].Value = "Result"; args.Row.Cells[3].Value = "=="; args.Row.Cells[4].Value = "Constant"; };
            ok.Click += SaveNode;
        }

        /// <summary>为来源列配置稳定代码和本地化显示。</summary>
        private static void SetSources(DataGridView grid, int index)
        {
            DataGridViewComboBoxColumn column = (DataGridViewComboBoxColumn)grid.Columns[index];
            column.DataSource = new[] { "Constant", "Parameter", "Output", "Result", "Context" }.Select(id => new FlowChoice { Id = id, Label = FlowText.T(id) }).ToList();
            if (grid.Name == "inputs") column.DataSource = ((List<FlowChoice>)column.DataSource).Where(choice => choice.Id != "Result").ToList();
            column.DisplayMember = "Label"; column.ValueMember = "Id";
        }

        /// <summary>列出本模块全部连接终点，空值明确表示结束。</summary>
        private void FillDestinations(ComboBox combo)
        {
            List<FlowChoice> choices = new List<FlowChoice> { new FlowChoice { Id = "", Label = FlowText.T("end") } };
            choices.AddRange(module.Program.Steps.Where(step => step.Id != original.Id).Select(step => new FlowChoice { Id = step.Id, Label = step.Caption }));
            combo.DisplayMember = "Label"; combo.ValueMember = "Id"; combo.DataSource = choices;
        }

        /// <summary>报警条目来自与运行内核相同的目录和当前语言。</summary>
        private void FillAlarms()
        {
            List<FlowChoice> choices = new List<FlowChoice>();
            foreach (System.Xml.Linq.XElement alarm in AlarmCatalog.RuntimeTable(SysPara.LanguageShow).Elements())
            {
                string name = alarm.Name.LocalName;
                if (!name.StartsWith("A")) continue;
                choices.Add(new FlowChoice { Id = name.Substring(1), Label = name.Substring(1) + " · " + (string)alarm.Attribute("Content") });
            }
            alarmValue.DisplayMember = "Label"; alarmValue.ValueMember = "Id"; alarmValue.DataSource = choices;
            alarmValue.SelectedIndex = -1;
        }

        /// <summary>按类别展示真实配置对象，类别未配置时不伪造设备。</summary>
        private void FillTargets(string selected)
        {
            string category = Convert.ToString(categoryValue.SelectedValue);
            IEnumerable<FlowResource> resources = catalog.Resources.Items.Values.Where(resource => catalog.Actions.Any(action => action.Category == category && catalog.Supports(action, resource)));
            List<FlowChoice> choices = resources.Select(resource => new FlowChoice { Id = resource.Id, Label = resource.Label }).ToList();
            if (category == "flow") choices.Insert(0, new FlowChoice { Id = "", Label = FlowText.T("flow") });
            if (!string.IsNullOrEmpty(selected) && choices.All(choice => choice.Id != selected)) choices.Add(new FlowChoice { Id = selected, Label = "[missing] " + selected });
            targetValue.DisplayMember = "Label"; targetValue.ValueMember = "Id"; targetValue.DataSource = choices;
            if (selected != null) targetValue.SelectedValue = selected;
        }

        /// <summary>按对象能力筛选动作，保留已有动作选择。</summary>
        private void FillActions(string selected)
        {
            string category = Convert.ToString(categoryValue.SelectedValue);
            FlowResource resource = catalog.FindResource(Convert.ToString(targetValue.SelectedValue));
            actionValue.DataSource = catalog.Actions.Where(action => action.Category == category && catalog.Supports(action, resource)).ToList();
            FlowAction choice = ((List<FlowAction>)actionValue.DataSource).SingleOrDefault(action => action.Id == selected);
            if (choice != null) actionValue.SelectedItem = choice;
        }

        /// <summary>为选定动作生成固定字段和可选择的输入来源。</summary>
        private void FillInputs(List<FlowInput> previous)
        {
            inputs.Rows.Clear();
            FlowAction action = actionValue.SelectedItem as FlowAction;
            if (action == null) { outputsValue.Text = ""; return; }
            foreach (string name in catalog.InputNames(action))
            {
                FlowInput value = previous.SingleOrDefault(item => item.Name == name) ?? new FlowInput { Name = name, Reference = DefaultValue(action, name) };
                inputs.Rows.Add(name, value.Source, value.Reference, "…");
            }
            outputsValue.Text = string.Join("   ", catalog.OutputNames(action, catalog.FindResource(Convert.ToString(targetValue.SelectedValue))));
        }

        /// <summary>提供明确的编辑初值，不将缺失资源数据作为运行默认值。</summary>
        private static string DefaultValue(FlowAction action, string name)
        {
            if (name == "Tolerance") return "0.2";
            if (name == "Milliseconds") return "1000";
            if (name == "StableMs") return "50";
            if (name == "Slave" || name == "Count") return "1";
            if (name == "Index" || name == "Start" || name == "Position" || name == "DelayMs" || name == "Offset" || name == "A" || name == "B") return "0";
            if (name == "End") return "9";
            if (name == "Step") return "1";
            if (name == "Type") return "int";
            if (name == "Operation") return "+";
            if (name == "Value" && (action.Category == "io")) return "True";
            return "";
        }

        /// <summary>切换类别后刷新对象及动作，不覆盖其他节点。</summary>
        private void CategoryChanged(object sender, EventArgs args)
        {
            if (filling) return;
            filling = true; FillTargets(null); FillActions(null); FillInputs(new List<FlowInput>()); filling = false;
        }

        /// <summary>对象改变后使用该对象的真实动作和字段。</summary>
        private void TargetChanged(object sender, EventArgs args)
        {
            if (filling) return;
            filling = true; FillActions(null); FillInputs(new List<FlowInput>()); filling = false;
        }

        /// <summary>动作改变后重新生成该动作的输入定义。</summary>
        private void ActionChanged(object sender, EventArgs args) { if (!filling) FillInputs(new List<FlowInput>()); }

        /// <summary>通过选择器编辑输入引用。</summary>
        private void InputPick(object sender, DataGridViewCellEventArgs args)
        {
            if (args.RowIndex < 0 || (args.ColumnIndex != 2 && args.ColumnIndex != 3)) return;
            Pick(inputs.Rows[args.RowIndex], 1, 2, Convert.ToString(inputs.Rows[args.RowIndex].Cells[0].Value));
        }

        /// <summary>通过相同选择器编辑条件左右值。</summary>
        private void ConditionPick(object sender, DataGridViewCellEventArgs args)
        {
            if (args.RowIndex < 0 || conditions.Rows[args.RowIndex].IsNewRow) return;
            if (args.ColumnIndex == 1 || args.ColumnIndex == 2) Pick(conditions.Rows[args.RowIndex], 0, 1, "");
            if (args.ColumnIndex == 5 || args.ColumnIndex == 6) Pick(conditions.Rows[args.RowIndex], 4, 5, "");
        }

        /// <summary>提交单个来源引用，不要求工程师输入对象路径。</summary>
        private void Pick(DataGridViewRow row, int sourceColumn, int referenceColumn, string field)
        {
            inputs.EndEdit(); conditions.EndEdit();
            string source = Convert.ToString(row.Cells[sourceColumn].Value);
            using (FlowSourceForm dialog = new FlowSourceForm(source, Convert.ToString(row.Cells[referenceColumn].Value), catalog,
                actionValue.SelectedItem as FlowAction, catalog.FindResource(Convert.ToString(targetValue.SelectedValue)), field))
                if (dialog.ShowDialog(this) == DialogResult.OK) row.Cells[referenceColumn].Value = dialog.Reference;
        }

        /// <summary>引用以资源名称显示，底层仍保存稳定身份。</summary>
        private void FormatReference(object sender, DataGridViewCellFormattingEventArgs args)
        {
            if (args.RowIndex < 0 || args.Value == null) return;
            DataGridView grid = (DataGridView)sender;
            int sourceColumn = grid == inputs ? 1 : args.ColumnIndex == 1 ? 0 : args.ColumnIndex == 5 ? 4 : -1;
            if (sourceColumn < 0 || (grid == inputs && args.ColumnIndex != 2)) return;
            string source = Convert.ToString(grid.Rows[args.RowIndex].Cells[sourceColumn].Value);
            string reference = Convert.ToString(args.Value);
            IEnumerable<FlowChoice> choices = source == "Parameter" ? catalog.Resources.ParameterChoices : source == "Output" ? FlowPrograms.OutputChoices(catalog) : new List<FlowChoice>();
            FlowChoice choice = choices.FirstOrDefault(item => item.Id == reference);
            if (choice != null) { args.Value = choice.Label; args.FormattingApplied = true; }
        }

        /// <summary>固定值可直接输入，对象和数据引用通过选择器修改。</summary>
        private void GuardReferenceEdit(object sender, DataGridViewCellCancelEventArgs args)
        {
            DataGridView grid = (DataGridView)sender;
            int sourceColumn = grid == inputs && args.ColumnIndex == 2 ? 1 : grid == conditions && args.ColumnIndex == 1 ? 0 : grid == conditions && args.ColumnIndex == 5 ? 4 : -1;
            if (sourceColumn >= 0 && Convert.ToString(grid.Rows[args.RowIndex].Cells[sourceColumn].Value) != "Constant") args.Cancel = true;
        }

        /// <summary>完成字段校验后一次更新原节点，失败保留编辑窗口。</summary>
        private void SaveNode(object sender, EventArgs args)
        {
            inputs.EndEdit(); conditions.EndEdit();
            try
            {
                FlowPrograms.RequireIdle();
                FlowAction action = actionValue.SelectedItem as FlowAction;
                if (action == null || string.IsNullOrWhiteSpace(captionValue.Text)) throw new InvalidOperationException("Select a configured object and action, and enter a node name.");
                FlowStep candidate = FlowProgramFile.Snapshot(new FlowProgram { Steps = new List<FlowStep> { original } }).Steps[0];
                candidate.Caption = captionValue.Text;
                candidate.Action = action.Id;
                candidate.Target = Convert.ToString(targetValue.SelectedValue);
                candidate.TimeoutMs = (int)timeoutValue.Value;
                candidate.AlarmCode = alarmValue.SelectedIndex >= 0 ? Convert.ToString(alarmValue.SelectedValue) : alarmValue.Text.Trim();
                candidate.Next = Convert.ToString(nextValue.SelectedValue);
                candidate.Otherwise = Convert.ToString(otherwiseValue.SelectedValue);
                candidate.Failure = Convert.ToString(failureValue.SelectedValue);
                candidate.WaitForCondition = wait.Checked; candidate.AnyCondition = any.Checked;
                candidate.Inputs = inputs.Rows.Cast<DataGridViewRow>().Where(row => !row.IsNewRow).Select(row => ReadInput(row, 1, 2, Convert.ToString(row.Cells[0].Value))).ToList();
                candidate.Conditions = conditions.Rows.Cast<DataGridViewRow>().Where(row => !row.IsNewRow).Select(row =>
                    new FlowCondition { Left = ReadInput(row, 0, 1, ""), Operator = Convert.ToString(row.Cells[3].Value), Right = ReadInput(row, 4, 5, "") }).ToList();
                catalog.Validate(candidate);
                int index = module.Program.Steps.IndexOf(original);
                module.Program.Steps[index] = candidate;
                DialogResult = DialogResult.OK;
            }
            catch (Exception error) { MessageBox.Show(string.Format(FlowText.T("error"), error.Message), FlowText.T("title")); }
        }

        /// <summary>读取一个已经明确选择的来源。</summary>
        private static FlowInput ReadInput(DataGridViewRow row, int source, int reference, string name)
        {
            return new FlowInput { Name = name, Source = Convert.ToString(row.Cells[source].Value), Reference = Convert.ToString(row.Cells[reference].Value) };
        }
    }
}
