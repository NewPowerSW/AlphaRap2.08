using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    public partial class FlowNodeForm : FlowLanguageForm
    {
        private readonly FlowModuleEditor module;
        private readonly FlowStep original;
        private readonly FlowActions catalog;
        private bool filling;
        private readonly List<FlowBranchEditor> branchEditors = new List<FlowBranchEditor>();

        /// <summary>Build the fixed controls without loading application data.</summary>
        public FlowNodeForm() { InitializeComponent(); }

        /// <summary>编辑节点副本，取消时不改变原工程和已有输入。</summary>
        public FlowNodeForm(FlowModuleEditor owner, FlowStep step) : this()
        {
            module = owner;
            original = step;
            catalog = new FlowActions(FlowResources.Capture(true));
            actionValue.FormattingEnabled = true;
            actionValue.Format += (sender, args) => { FlowMethod method = args.ListItem as FlowMethod; if (method != null) args.Value = FlowMethods.Title(method); };
            filling = true;
            captionValue.Text = step.Caption;
            timeoutValue.Value = Math.Max(timeoutValue.Minimum, Math.Min(timeoutValue.Maximum, step.TimeoutMs));
            SetSources(inputs, 1);
            categoryValue.DataSource = new[] { "flow", "io", "motion", "vision", "device", "data" }
                .Select(id => new FlowChoice { Id = id, Label = FlowText.T(id) }).ToList();
            categoryValue.DisplayMember = "Label"; categoryValue.ValueMember = "Id";
            FillAlarms();
            alarmValue.Text = string.IsNullOrEmpty(step.AlarmCode) ? "2019" : step.AlarmCode;
            FlowResource selectedResource = catalog.FindResource(step.Target);
            categoryValue.SelectedValue = selectedResource == null ? "flow" : selectedResource.Category;
            FillTargets(step.Target);
            FillActions(step.Action);
            FillInputs(step.Inputs);
            foreach (FlowBranch branch in FlowProgramFile.BranchesOf(step)) AddBranchEditor(branch, step.TargetOf(branch.Code));
            filling = false;
            categoryValue.SelectedIndexChanged += CategoryChanged;
            targetValue.SelectedIndexChanged += TargetChanged;
            actionValue.SelectedIndexChanged += ActionChanged;
            inputs.CellContentClick += InputPick;
            inputs.CellDoubleClick += InputPick;
            inputs.CellFormatting += FormatReference;
            inputs.CellBeginEdit += GuardReferenceEdit;
            addCase.Click += AddCase;
            branchPanel.Resize += (sender, args) => ResizeBranches();
            ResizeBranches();
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

        /// <summary>用同一设计器模板绑定默认 NEXT 和按需添加的 CASE。</summary>
        private FlowBranchEditor AddBranchEditor(FlowBranch branch, string targetId)
        {
            FlowBranchEditor editor = branch.Code == "NEXT" ? nextBranch : new FlowBranchEditor();
            editor.Name = "branch_" + branch.Code + "_dyn";
            DataGridView grid = editor.ConditionsGrid;
            SetSources(grid, 0); SetSources(grid, 4);
            grid.CellContentClick += ConditionPick; grid.CellDoubleClick += ConditionPick;
            grid.CellFormatting += FormatReference; grid.CellBeginEdit += GuardReferenceEdit;
            grid.DefaultValuesNeeded += (sender, args) => { args.Row.Cells[0].Value = "Result"; args.Row.Cells[3].Value = "=="; args.Row.Cells[4].Value = "Constant"; };
            List<FlowChoice> targets = new List<FlowChoice> { new FlowChoice { Id = "", Label = FlowText.T("end") } };
            targets.AddRange(module.Program.Steps.Where(step => step.Id != original.Id).Select(step => new FlowChoice { Id = step.Id, Label = step.Caption }));
            editor.Bind(branch, targetId, targets);
            editor.RemoveRequested += RemoveBranch;
            branchEditors.Add(editor);
            if (editor.Parent == null) branchPanel.Controls.Add(editor);
            FlowLanguageForm.ApplyTexts(editor);
            addCase.Enabled = branchEditors.Count < 4;
            return editor;
        }

        /// <summary>每次只添加一个尚未使用的 CASE，默认节点始终只有 NEXT。</summary>
        private void AddCase(object sender, EventArgs args)
        {
            string code = new[] { "CASE1", "CASE2", "CASE3" }.FirstOrDefault(value => branchEditors.All(editor => editor.Code != value));
            if (code == null) return;
            FlowMethod action = actionValue.SelectedItem as FlowMethod;
            string field = action == null ? null : catalog.OutputNames(action).FirstOrDefault();
            if (nextBranch.IsUnconditional) nextBranch.MakeConditional(field);
            FlowBranchEditor added = AddBranchEditor(new FlowBranch { Code = code }, "");
            added.MakeConditional(field);
            ResizeBranches();
            branchPanel.ScrollControlIntoView(added);
        }

        /// <summary>移除草稿中的 CASE，取消窗口仍保留原节点定义。</summary>
        private void RemoveBranch(object sender, EventArgs args)
        {
            FlowBranchEditor editor = (FlowBranchEditor)sender;
            if (editor.Code == "NEXT") return;
            branchEditors.Remove(editor); branchPanel.Controls.Remove(editor); editor.Dispose();
            addCase.Enabled = true;
        }

        /// <summary>动态出口沿用同一模板宽度，并按原生出口顺序显示。</summary>
        private void ResizeBranches()
        {
            string[] codes = { "NEXT", "CASE1", "CASE2", "CASE3" };
            List<FlowBranchEditor> ordered = branchEditors.OrderBy(editor => Array.IndexOf(codes, editor.Code)).ToList();
            for (int i = 0; i < ordered.Count; i++)
            {
                FlowBranchEditor editor = ordered[i];
                editor.Width = Math.Max(editor.MinimumSize.Width, branchPanel.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 8);
                branchPanel.Controls.SetChildIndex(editor, i);
            }
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
            IEnumerable<FlowResource> resources = catalog.Resources.Items.Values.Where(resource => resource.Selectable && resource.Category == category && resource.Methods.Count > 0);
            List<FlowChoice> choices = resources.Select(resource => new FlowChoice { Id = resource.Id, Label = resource.Label }).ToList();
            if (category == "flow") choices.Insert(0, new FlowChoice { Id = "", Label = FlowText.T("flow") });
            if (!string.IsNullOrEmpty(selected) && choices.All(choice => choice.Id != selected)) choices.Add(new FlowChoice { Id = selected, Label = catalog.FindResource(selected) == null ? FlowText.T("methodMissing") + " " + selected : catalog.FindResource(selected).Label });
            targetValue.DisplayMember = "Label"; targetValue.ValueMember = "Id"; targetValue.DataSource = choices;
            if (selected != null) targetValue.SelectedValue = selected;
        }

        /// <summary>按对象能力筛选动作，保留已有动作选择。</summary>
        private void FillActions(string selected)
        {
            string category = Convert.ToString(categoryValue.SelectedValue);
            FlowResource resource = catalog.FindResource(Convert.ToString(targetValue.SelectedValue));
            actionValue.DataSource = resource == null && category != "flow" ? new List<FlowMethod>() : catalog.MethodsFor(resource).ToList();
            FlowMethod choice = ((List<FlowMethod>)actionValue.DataSource).SingleOrDefault(action => action.Id == selected);
            if (choice != null) actionValue.SelectedItem = choice;
            else if (!string.IsNullOrEmpty(selected)) actionValue.SelectedIndex = -1;
        }

        /// <summary>为选定动作生成固定字段和可选择的输入来源。</summary>
        private void FillInputs(List<FlowInput> previous)
        {
            inputs.Rows.Clear();
            FlowMethod action = actionValue.SelectedItem as FlowMethod;
            if (action == null) { outputsValue.Text = ""; return; }
            foreach (string name in catalog.InputNames(action))
            {
                FlowInput value = previous.SingleOrDefault(item => item.Name == name) ?? new FlowInput { Name = name, Reference = action.Inputs.Single(field => field.Name == name).DefaultValue };
                inputs.Rows.Add(name, value.Source, value.Reference, "…");
            }
            outputsValue.Text = string.Join("   ", action.Outputs.Select(field => field.Name + " (" + field.Type.Name + ")"));
        }

        /// <summary>刷新方法、类别和来源的显示语言，保留输入网格及尚未提交的编辑内容。</summary>
        protected override void RefreshLanguageChoices()
        {
            if (catalog == null) return;
            string category = Convert.ToString(categoryValue.SelectedValue);
            string target = Convert.ToString(targetValue.SelectedValue);
            FlowMethod method = actionValue.SelectedItem as FlowMethod;
            string alarmText = alarmValue.Text;
            string alarmCode = alarmValue.SelectedIndex < 0 ? null : Convert.ToString(alarmValue.SelectedValue);
            filling = true;
            try
            {
                categoryValue.DataSource = new[] { "flow", "io", "motion", "vision", "device", "data" }
                    .Select(id => new FlowChoice { Id = id, Label = FlowText.T(id) }).ToList();
                categoryValue.SelectedValue = category;
                FillTargets(target); FillActions(method == null ? null : method.Id);
                FillAlarms();
                if (alarmCode != null) alarmValue.SelectedValue = alarmCode;
                if (alarmValue.SelectedIndex < 0) alarmValue.Text = alarmText;
                SetSources(inputs, 1);
                foreach (FlowBranchEditor editor in branchEditors)
                {
                    SetSources(editor.ConditionsGrid, 0); SetSources(editor.ConditionsGrid, 4);
                    editor.RefreshDestinationLanguage();
                }
            }
            finally { filling = false; }
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
            DataGridView grid = (DataGridView)sender;
            if (args.RowIndex < 0 || grid.Rows[args.RowIndex].IsNewRow) return;
            if (args.ColumnIndex == 1 || args.ColumnIndex == 2) Pick(grid.Rows[args.RowIndex], 0, 1, "");
            if (args.ColumnIndex == 5 || args.ColumnIndex == 6) Pick(grid.Rows[args.RowIndex], 4, 5, "");
        }

        /// <summary>提交单个来源引用，不要求工程师输入对象路径。</summary>
        private void Pick(DataGridViewRow row, int sourceColumn, int referenceColumn, string field)
        {
            inputs.EndEdit();
            foreach (FlowBranchEditor editor in branchEditors) editor.ConditionsGrid.EndEdit();
            string source = Convert.ToString(row.Cells[sourceColumn].Value);
            using (FlowSourceForm dialog = new FlowSourceForm(source, Convert.ToString(row.Cells[referenceColumn].Value), catalog,
                actionValue.SelectedItem as FlowMethod, field, ReadFixedInputs()))
                if (dialog.ShowDialog(this) == DialogResult.OK) row.Cells[referenceColumn].Value = dialog.Reference;
        }

        /// <summary>读取当前固定输入，点位选择器可按已选点位表筛选索引。</summary>
        private Dictionary<string, string> ReadFixedInputs()
        {
            return inputs.Rows.Cast<DataGridViewRow>().Where(row => !row.IsNewRow && Convert.ToString(row.Cells[1].Value) == "Constant")
                .ToDictionary(row => Convert.ToString(row.Cells[0].Value), row => Convert.ToString(row.Cells[2].Value));
        }

        /// <summary>引用以资源名称显示，底层仍保存稳定身份。</summary>
        private void FormatReference(object sender, DataGridViewCellFormattingEventArgs args)
        {
            if (args.RowIndex < 0 || args.Value == null) return;
            DataGridView grid = (DataGridView)sender;
            if (grid == inputs && args.ColumnIndex == 0)
            {
                FlowMethod method = actionValue.SelectedItem as FlowMethod;
                FlowField field = method == null ? null : method.Inputs.FirstOrDefault(item => item.Name == Convert.ToString(args.Value));
                if (field != null) { args.Value = field.Name + " (" + field.Type.Name + ")"; args.FormattingApplied = true; }
                return;
            }
            int sourceColumn = grid == inputs ? 1 : args.ColumnIndex == 1 ? 0 : args.ColumnIndex == 5 ? 4 : -1;
            if (sourceColumn < 0 || (grid == inputs && args.ColumnIndex != 2)) return;
            string source = Convert.ToString(grid.Rows[args.RowIndex].Cells[sourceColumn].Value);
            string reference = Convert.ToString(args.Value);
            if (grid == inputs && source == "Constant")
            {
                FlowMethod method = actionValue.SelectedItem as FlowMethod;
                FlowField field = method == null ? null : method.Inputs.FirstOrDefault(item => item.Name == Convert.ToString(grid.Rows[args.RowIndex].Cells[0].Value));
                string table;
                Dictionary<string, string> fixedInputs = ReadFixedInputs();
                FlowOption option = field == null ? null : field.Options.FirstOrDefault(item => item.Value == reference &&
                    (item.Group == null || !fixedInputs.TryGetValue("Table", out table) || item.Group == table));
                if (option != null) { args.Value = option.Label; args.FormattingApplied = true; }
                return;
            }
            IEnumerable<FlowChoice> choices = source == "Parameter" ? catalog.Resources.ParameterChoices : source == "Output" ? FlowPrograms.OutputChoices(catalog) : new List<FlowChoice>();
            FlowChoice choice = choices.FirstOrDefault(item => item.Id == reference);
            if (choice != null) { args.Value = choice.Label; args.FormattingApplied = true; }
        }

        /// <summary>固定值可直接输入，对象和数据引用通过选择器修改。</summary>
        private void GuardReferenceEdit(object sender, DataGridViewCellCancelEventArgs args)
        {
            DataGridView grid = (DataGridView)sender;
            int sourceColumn = grid == inputs && args.ColumnIndex == 2 ? 1 : grid != inputs && args.ColumnIndex == 1 ? 0 : grid != inputs && args.ColumnIndex == 5 ? 4 : -1;
            if (sourceColumn >= 0 && Convert.ToString(grid.Rows[args.RowIndex].Cells[sourceColumn].Value) != "Constant") args.Cancel = true;
            if (grid != inputs || args.ColumnIndex != 2) return;
            FlowMethod method = actionValue.SelectedItem as FlowMethod;
            string name = Convert.ToString(grid.Rows[args.RowIndex].Cells[0].Value);
            if (method != null && method.Inputs.Any(field => field.Name == name && field.Options.Count > 0)) args.Cancel = true;
        }

        /// <summary>完成字段校验后一次更新原节点，失败保留编辑窗口。</summary>
        private void SaveNode(object sender, EventArgs args)
        {
            inputs.EndEdit();
            foreach (FlowBranchEditor editor in branchEditors) editor.ConditionsGrid.EndEdit();
            try
            {
                FlowPrograms.RequireIdle();
                FlowMethod action = actionValue.SelectedItem as FlowMethod;
                if (action == null || string.IsNullOrWhiteSpace(captionValue.Text)) throw new InvalidOperationException("Select a configured object and action, and enter a node name.");
                FlowStep candidate = FlowProgramFile.Snapshot(new FlowProgram { Steps = new List<FlowStep> { original } }).Steps[0];
                candidate.Caption = captionValue.Text;
                candidate.Action = action.Id;
                candidate.Target = Convert.ToString(targetValue.SelectedValue);
                candidate.TimeoutMs = (int)timeoutValue.Value;
                candidate.AlarmCode = alarmValue.SelectedIndex >= 0 ? Convert.ToString(alarmValue.SelectedValue) : alarmValue.Text.Trim();
                candidate.Inputs = inputs.Rows.Cast<DataGridViewRow>().Where(row => !row.IsNewRow).Select(row => ReadInput(row, 1, 2, Convert.ToString(row.Cells[0].Value))).ToList();
                candidate.Conditions.Clear(); candidate.WaitForCondition = false; candidate.AnyCondition = false;
                candidate.Branches = branchEditors.Select(editor => editor.ReadBranch()).ToList();
                foreach (string code in new[] { "NEXT", "CASE1", "CASE2", "CASE3" }) candidate.SetTarget(code, null);
                foreach (FlowBranchEditor editor in branchEditors) candidate.SetTarget(editor.Code, editor.TargetId);
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
