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

        /// <summary>建立固定节点编辑布局；设计期不关联运行服务或加载工程。</summary>
        public FlowNodeForm()
        {
            InitializeComponent();
            if (UiTheme.IsDesignTime) return;
            scriptEditor.BindSettings(scriptSettings);
        }

        /// <summary>编辑节点副本，取消时不改变原工程和已有输入。</summary>
        public FlowNodeForm(FlowModuleEditor owner, FlowStep step) : this()
        {
            if (UiTheme.IsDesignTime) return;
            module = owner;
            original = step;
            catalog = new FlowActions(FlowResources.Capture(true));
            actionValue.FormattingEnabled = true;
            actionValue.Format += (sender, args) => { FlowMethod method = args.ListItem as FlowMethod; if (method != null) args.Value = FlowMethods.Title(method); };
            filling = true;
            captionValue.Text = step.Caption;
            timeoutValue.Value = Math.Max(timeoutValue.Minimum, Math.Min(timeoutValue.Maximum, step.TimeoutMs));
            categoryValue.DataSource = new[] { "flow", "constants", "di", "do", "cylinder", "motion", "vision", "device", "tray", "data" }
                .Select(id => new FlowChoice { Id = id, Label = FlowText.T(id) }).ToList();
            categoryValue.DisplayMember = "Label"; categoryValue.ValueMember = "Id";
            FillAlarms(string.IsNullOrEmpty(step.AlarmCode) ? "2019" : step.AlarmCode);
            FlowResource selectedResource = catalog.FindResource(step.Target);
            bool codeNode = !string.IsNullOrEmpty(step.CodeModuleId);
            categoryValue.SelectedValue = codeNode || selectedResource == null ? "flow" : selectedResource.Category;
            FillTargets(codeNode ? "" : step.Target);
            FillActions(codeNode ? "value" : step.Action);
            InitializeExecution(step);
            FillInputs(step.Inputs);
            foreach (FlowBranch branch in FlowProgramFile.BranchesOf(step)) AddBranchEditor(branch, step.TargetOf(branch.Code));
            filling = false;
            categoryValue.SelectedIndexChanged += CategoryChanged;
            targetValue.SelectedIndexChanged += TargetChanged;
            actionValue.SelectedIndexChanged += ActionChanged;
            showAdvanced.CheckedChanged += (sender, args) => RefreshParameterLayout();
            inputs.Resize += (sender, args) => ResizeInputs();
            addCase.Click += AddCase;
            branchPanel.Resize += (sender, args) => ResizeBranches();
            ResizeBranches();
            ok.Click += SaveNode;
        }

        private IEnumerable<FlowInputRow> InputRows { get { return inputs.Controls.OfType<FlowInputRow>(); } }

        /// <summary>用同一设计器模板绑定默认 NEXT 和按需添加的 CASE。</summary>
        private FlowBranchEditor AddBranchEditor(FlowBranch branch, string targetId)
        {
            FlowBranchEditor editor = branch.Code == "NEXT" ? nextBranch : new FlowBranchEditor();
            editor.Name = "branch_" + branch.Code + "_dyn";
            editor.Configure(catalog,CurrentMethod);
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
            if (nextBranch.IsUnconditional) nextBranch.MakeConditional();
            FlowBranch branch = new FlowBranch { Code = code };
            FlowBranch next = nextBranch.ReadBranch();
            FlowCondition condition = next.Conditions.Count == 1 ? next.Conditions[0] : null;
            if (code == "CASE1" && condition != null && condition.Operator == "==" && condition.Left.Source == "Result" && condition.Right.Source == "Constant")
            {
                FlowField field = CurrentMethod == null ? null : CurrentMethod.Outputs.FirstOrDefault(output => output.Name == condition.Left.Reference);
                bool expected = condition.Right.Reference == "1";
                bool known = condition.Right.Reference == "1" || condition.Right.Reference == "0" || bool.TryParse(condition.Right.Reference, out expected);
                if (field != null && field.Type == typeof(bool) && known)
                    branch.Conditions.Add(new FlowCondition { Left = new FlowInput { Source = "Result", Reference = field.Name }, Operator = "==", Right = new FlowInput { Reference = expected ? "false" : "true" } });
            }
            FlowBranchEditor added = AddBranchEditor(branch, "");
            added.MakeConditional();
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
        private void FillAlarms(string selected)
        {
            alarmValue.BeginUpdate();
            alarmValue.Items.Clear();
            alarmValue.DisplayMember = "Label";
            foreach (System.Xml.Linq.XElement alarm in AlarmCatalog.RuntimeTable(SysPara.LanguageShow).Elements())
            {
                string name = alarm.Name.LocalName;
                if (!name.StartsWith("A")) continue;
                alarmValue.Items.Add(new FlowChoice { Id = name.Substring(1), Label = name.Substring(1) + " · " + (string)alarm.Attribute("Content") });
            }
            alarmValue.SelectedItem = alarmValue.Items.Cast<FlowChoice>().FirstOrDefault(choice => choice.Id == selected);
            if (alarmValue.SelectedIndex < 0) alarmValue.Text = selected ?? "";
            alarmValue.EndUpdate();
        }

        private string SelectedAlarmCode { get { FlowChoice choice = alarmValue.SelectedItem as FlowChoice; return choice == null ? alarmValue.Text.Trim() : choice.Id; } }

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
            bool contextObject = category == "flow" && string.IsNullOrEmpty(Convert.ToString(targetValue.SelectedValue));
            actionValue.DataSource = resource == null && !contextObject ? new List<FlowMethod>() : catalog.MethodsFor(resource).ToList();
            FlowMethod choice = ((List<FlowMethod>)actionValue.DataSource).SingleOrDefault(action => action.Id == selected);
            if (choice != null) actionValue.SelectedItem = choice;
            else if (!string.IsNullOrEmpty(selected)) actionValue.SelectedIndex = -1;
        }

        /// <summary>按对象声明建立参数行，已有非默认高级值自动展开。</summary>
        private void FillInputs(List<FlowInput> previous)
        {
            foreach (Control control in inputs.Controls.Cast<Control>().ToArray()) control.Dispose();
            FlowMethod action = CurrentMethod;
            if (action == null) { outputsValue.Text = ""; RefreshMethodHelp(); return; }
            inputs.SuspendLayout();
            foreach (FlowField field in action.Inputs)
            {
                FlowInput value = previous.SingleOrDefault(item => item.Name == field.Name) ?? new FlowInput {
                    Name = field.Name, Source = string.IsNullOrEmpty(field.OutputSourceMethod) && string.IsNullOrEmpty(field.OutputSourceField) ? "Constant" : "Output", Reference = field.DefaultValue };
                FlowInputRow row = new FlowInputRow { Name = "parameter_" + field.Name + "_dyn" };
                row.Bind(value, field, catalog, action.Id);
                row.BindRequested += PickInput;
                inputs.Controls.Add(row);
            }
            inputs.ResumeLayout();
            showAdvanced.Checked = InputRows.Any(row => row.IsAdvanced && row.HasCustomValue);
            RefreshOutputTitles(action);
            RefreshMethodHelp();
            foreach (FlowBranchEditor editor in branchEditors) editor.Configure(catalog,action);
        }

        /// <summary>显示可供后续节点选择的业务输出，内部编号仅供高级绑定使用。</summary>
        private void RefreshOutputTitles(FlowMethod action)
        {
            string[] titles = action == null ? new string[0] : action.Outputs.Where(field => !field.Advanced).Select(field => FlowMethods.FieldTitle(action.Id, field, true)).ToArray();
            outputsValue.Visible = titles.Length > 0;
            outputsValue.Text = titles.Length == 0 ? "" : string.Format(FlowText.T("nodeOutputs"), string.Join("   ", titles));
        }

        /// <summary>根据参数数量和高级展开状态安排行控件，不改动输入数据。</summary>
        private void RefreshParameterLayout()
        {
            FlowInputRow[] rows = InputRows.ToArray();
            bool advanced = rows.Any(row => row.IsAdvanced);
            showAdvanced.Visible = advanced;
            inputLayout.RowStyles[1].Height = advanced ? 28 : 0;
            foreach (FlowInputRow row in rows) row.Visible = !row.IsAdvanced || showAdvanced.Checked;
            int visible = rows.Count(row => !row.IsAdvanced || showAdvanced.Checked);
            inputLayout.RowStyles[0].SizeType = visible > 0 ? SizeType.Percent : SizeType.Absolute;
            inputLayout.RowStyles[0].Height = visible > 0 ? 100 : 0;
            inputs.Visible = visible > 0;
            ResizeInputs();
        }

        /// <summary>参数行填满可视区域，滚动条不遮挡绑定按钮。</summary>
        private void ResizeInputs()
        {
            int width = Math.Max(360, inputs.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 8);
            foreach (FlowInputRow row in InputRows) row.Width = width;
        }

        /// <summary>解释绑定点位、托盘计算及表值设置，无参数方法不显示空输入网格。</summary>
        private void RefreshMethodHelp()
        {
            FlowMethod action = CurrentMethod;
            methodHint.Text = "";
            methodHint.Visible = false; inputLayout.RowStyles[2].Height = 0;
            if (action == null)
            {
                methodHint.Text = MissingActionHelp();
                methodHint.Visible = true; inputLayout.RowStyles[2].Height = 76;
                RefreshParameterLayout(); return;
            }
            if (action.Inputs.Count == 0) methodHint.Text = FlowText.T("noInputs");
            if (action.Id == "constant.set") methodHint.Text = FlowText.T("constantSetHelp");
            FlowResource resource = catalog.FindResource(Convert.ToString(targetValue.SelectedValue));
            if (action.Id == "point.move" && resource != null && resource.BoundPoints != null)
            {
                FlowPointAxis points = (FlowPointAxis)resource.BoundPoints.Instance;
                string table = points.TableName.StartsWith("AxisPoints_", StringComparison.Ordinal) ? resource.Label : points.TableName;
                methodHint.Text = string.Format(FlowText.T("axisPointsHelp"), resource.Label, table, points.RowCount, points.Positions.Count);
            }
            FlowTray tray = resource == null ? null : resource.Instance as FlowTray;
            if (tray != null) methodHint.Text = string.Format(FlowText.T("trayHelp"),tray.Rows,tray.Columns,tray.RowPitch,tray.ColumnPitch,(long)tray.Rows*tray.Columns-1);
            bool hasHelp = methodHint.Text.Length > 0;
            methodHint.Visible = hasHelp;
            inputLayout.RowStyles[2].Height = hasHelp ? 52 : 0;
            RefreshParameterLayout();
        }

        /// <summary>目录为空或旧对象失效时说明可执行的修复位置，不替换成另一对象。</summary>
        private string MissingActionHelp()
        {
            string target = Convert.ToString(targetValue.SelectedValue);
            if (!string.IsNullOrEmpty(target) && catalog.FindResource(target) == null) return FlowText.T("nodeMissingObjectHelp");
            if (targetValue.Items.Count > 0) return FlowText.T("nodeChooseActionHelp");
            switch (Convert.ToString(categoryValue.SelectedValue))
            {
                case "motion": return FlowText.T("nodeNoAxisHelp");
                case "vision": return FlowText.T("nodeNoVisionHelp");
                case "di": case "do": case "cylinder": return FlowText.T("nodeNoIoHelp");
                case "constants": return FlowText.T("nodeNoConstantsHelp");
                case "data": return FlowText.T("nodeNoDataHelp");
                default: return FlowText.T("nodeNoObjectHelp");
            }
        }

        /// <summary>刷新方法、类别和来源的显示语言，保留输入网格及尚未提交的编辑内容。</summary>
        protected override void RefreshLanguageChoices()
        {
            if (catalog == null) return;
            string category = Convert.ToString(categoryValue.SelectedValue);
            string target = Convert.ToString(targetValue.SelectedValue);
            FlowMethod method = CurrentMethod;
            FlowMethod selectedAction = actionValue.SelectedItem as FlowMethod;
            string alarmCode = SelectedAlarmCode;
            filling = true;
            try
            {
                categoryValue.DataSource = new[] { "flow", "constants", "di", "do", "cylinder", "motion", "vision", "device", "tray", "data" }
                    .Select(id => new FlowChoice { Id = id, Label = FlowText.T(id) }).ToList();
                categoryValue.SelectedValue = category;
                FillTargets(target); FillActions(selectedAction == null ? null : selectedAction.Id);
                if (executionMode == "Action" && method == null) actionValue.SelectedIndex = -1;
                FillExecutionChoices();
                FillAlarms(alarmCode);
                foreach (FlowInputRow row in InputRows) row.RefreshLanguage();
                RefreshOutputTitles(method);
                foreach (FlowBranchEditor editor in branchEditors)
                {
                    editor.RefreshDestinationLanguage();
                }
                RefreshMethodHelp();
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

        /// <summary>用同一窗口选择固定值或类型匹配的数据，引用保存稳定身份。</summary>
        private void PickInput(object sender, EventArgs args)
        {
            FlowInputRow row = (FlowInputRow)sender;
            FlowMethod action = CurrentMethod;
            if (action == null) return;
            FlowField field = action.Inputs.Single(item => item.Name == row.FieldName);
            string description = field.Name == "Index" && action.Id == "point.move" ? methodHint.Text : null;
            using (FlowSourceForm dialog = new FlowSourceForm(row.ReadInput(), catalog, action, field, description, codeEditing ? "code/" + codeId : Convert.ToString(targetValue.SelectedValue)))
                if (dialog.ShowDialog(this) == DialogResult.OK) row.SetInput(dialog.Selection);
        }

        /// <summary>完成字段校验后一次更新原节点，失败保留编辑窗口。</summary>
        private void SaveNode(object sender, EventArgs args)
        {
            try
            {
                FlowPrograms.RequireIdle();
                FlowMethod action = CurrentMethod;
                if (action == null || string.IsNullOrWhiteSpace(captionValue.Text)) throw new InvalidOperationException(FlowText.T("selectNode"));
                FlowStep candidate = FlowProgramFile.Snapshot(new FlowProgram { Steps = new List<FlowStep> { original } }).Steps[0];
                candidate.Caption = captionValue.Text;
                candidate.Action = action.Id;
                candidate.Target = executionMode == "Action" ? Convert.ToString(targetValue.SelectedValue) : "";
                candidate.TimeoutMs = (int)timeoutValue.Value;
                candidate.AlarmCode = SelectedAlarmCode;
                candidate.Inputs = InputRows.Select(row => row.ReadInput()).ToList();
                ReadExecutionTo(candidate);
                candidate.Conditions.Clear(); candidate.WaitForCondition = false; candidate.AnyCondition = false;
                if (codeEditing) codeRoutes.ReadTo(candidate);
                else
                {
                    candidate.Branches = branchEditors.Select(editor => editor.ReadBranch()).ToList();
                    foreach (string code in new[] { "NEXT", "CASE1", "CASE2", "CASE3" }) candidate.SetTarget(code, null);
                    foreach (FlowBranchEditor editor in branchEditors) candidate.SetTarget(editor.Code, editor.TargetId);
                }
                catalog.Validate(candidate);
                int index = module.Program.Steps.IndexOf(original);
                module.Program.Steps[index] = candidate;
                DialogResult = DialogResult.OK;
            }
            catch (Exception error) { MessageBox.Show(string.Format(FlowText.T("error"), error.Message), FlowText.T("title")); }
        }

    }
}
