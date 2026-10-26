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
        }

        /// <summary>编辑节点副本，取消时不改变原工程和已有输入。</summary>
        public FlowNodeForm(FlowModuleEditor owner, FlowStep step) : this()
        {
            if (UiTheme.IsDesignTime) return;
            module = owner;
            original = step;
            catalog = new FlowActions(FlowResources.Capture(true));
            actionValue.FormattingEnabled = true;
            actionValue.Format += FormatMethod;
            filling = true;
            captionValue.Text = step.Caption;
            timeoutValue.Value = Math.Max(timeoutValue.Minimum, Math.Min(timeoutValue.Maximum, step.TimeoutMs));
            FillCategories();
            FlowResource resource = FlowActions.IsSupported(step) ? catalog.FindResource(step.Target) : null;
            categoryValue.SelectedValue = resource == null ? "flow" : resource.Category;
            FillTargets(FlowActions.IsSupported(step) ? step.Target : "");
            FillActions(FlowActions.IsSupported(step) ? step.Action : null);
            FillAlarms(string.IsNullOrEmpty(step.AlarmCode) ? "2019" : step.AlarmCode);
            InitializeExecution(step);
            RestoreModeInputs();
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

        /// <summary>按框架公开的功能类别显示对象入口，稳定类别代码不随语言变化。</summary>
        private void FillCategories()
        {
            categoryValue.DisplayMember = "Label";
            categoryValue.ValueMember = "Id";
            categoryValue.DataSource = new[] { "flow", "constants", "di", "do", "cylinder", "motion", "vision", "device", "tray", "data" }
                .Select(id => new FlowChoice { Id = id, Label = FlowText.T(id) }).ToList();
        }

        /// <summary>按类别展示真实配置对象，已有失效引用保留并显示缺失提示。</summary>
        private void FillTargets(string selected)
        {
            string category = Convert.ToString(categoryValue.SelectedValue);
            IEnumerable<FlowResource> resources = catalog.Resources.Items.Values.Where(resource => resource.Selectable && resource.Category == category && resource.Methods.Count > 0);
            List<FlowChoice> choices = resources.Select(resource => new FlowChoice { Id = resource.Id, Label = resource.Label }).ToList();
            if (category == "flow") choices.Insert(0, new FlowChoice { Id = "", Label = FlowText.T("flow") });
            if (!string.IsNullOrEmpty(selected) && choices.All(choice => choice.Id != selected))
                choices.Add(new FlowChoice { Id = selected, Label = FlowText.T("methodMissing") + " " + selected });
            targetValue.DisplayMember = "Label";
            targetValue.ValueMember = "Id";
            targetValue.DataSource = choices;
            if (selected != null) targetValue.SelectedValue = selected;
        }

        /// <summary>只列出所选实际对象提供的方法，不以其他对象的同名方法补位。</summary>
        private void FillActions(string selected)
        {
            string category = Convert.ToString(categoryValue.SelectedValue);
            string target = Convert.ToString(targetValue.SelectedValue);
            FlowResource resource = catalog.FindResource(target);
            bool contextObject = category == "flow" && string.IsNullOrEmpty(target);
            List<FlowMethod> methods = resource == null && !contextObject ? new List<FlowMethod>() : catalog.MethodsFor(resource).ToList();
            actionValue.DataSource = methods;
            FlowMethod choice = methods.SingleOrDefault(method => method.Id == selected);
            if (choice != null) actionValue.SelectedItem = choice;
            else if (!string.IsNullOrEmpty(selected)) actionValue.SelectedIndex = -1;
        }

        /// <summary>将对象方法的稳定身份转换为当前语言的业务名称。</summary>
        private void FormatMethod(object sender, ListControlConvertEventArgs args)
        {
            FlowMethod method = args.ListItem as FlowMethod;
            if (method != null) args.Value = FlowMethods.Title(method);
        }

        /// <summary>按对象声明建立参数行，已有非默认高级值自动展开。</summary>
        private void FillInputs(List<FlowInput> previous)
        {
            foreach (Control control in inputs.Controls.Cast<Control>().ToArray()) control.Dispose();
            FlowMethod action = CurrentMethod;
            if (action == null)
            {
                RefreshOutputTitles(null);
                RefreshMethodHelp();
                foreach (FlowBranchEditor editor in branchEditors) editor.Configure(catalog, null);
                return;
            }
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

        /// <summary>解释当前绑定轴点位、托盘和设置表，未配置对象时显示修复位置。</summary>
        private void RefreshMethodHelp()
        {
            FlowMethod action = CurrentMethod;
            methodHint.Text = "";
            if (!retiredAwaitingChoice && executionMode == "Action" && action == null) methodHint.Text = MissingActionHelp();
            if (action != null && action.Inputs.Count == 0) methodHint.Text = FlowText.T("noInputs");
            if (action != null && action.Id == "constant.set") methodHint.Text = FlowText.T("constantSetHelp");
            FlowResource resource = catalog.FindResource(Convert.ToString(targetValue.SelectedValue));
            FlowPointAxis points = resource == null || resource.BoundPoints == null ? null : resource.BoundPoints.Instance as FlowPointAxis;
            if (action != null && action.Id == "point.move" && points != null)
            {
                string table = points.TableName.StartsWith("AxisPoints_", StringComparison.Ordinal) ? resource.Label : points.TableName;
                methodHint.Text = string.Format(FlowText.T("axisPointsHelp"), resource.Label, table, points.RowCount, points.Positions.Count);
            }
            FlowTray tray = resource == null ? null : resource.Instance as FlowTray;
            if (executionMode == "Action" && tray != null)
                methodHint.Text = string.Format(FlowText.T("trayHelp"), tray.Rows, tray.Columns, tray.RowPitch, tray.ColumnPitch, (long)tray.Rows * tray.Columns - 1);
            bool hasHelp = methodHint.Text.Length > 0;
            methodHint.Visible = hasHelp;
            inputLayout.RowStyles[2].Height = hasHelp ? 52 : 0;
            RefreshParameterLayout();
        }

        /// <summary>指出当前类别的配置入口，不自动替换缺失对象或方法。</summary>
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

        /// <summary>刷新执行方式和来源的显示语言，保留输入绑定及未提交草稿。</summary>
        protected override void RefreshLanguageChoices()
        {
            if (catalog == null) return;
            string category = Convert.ToString(categoryValue.SelectedValue);
            string target = Convert.ToString(targetValue.SelectedValue);
            FlowMethod method = CurrentMethod;
            FlowMethod selected = actionValue.SelectedItem as FlowMethod;
            string alarmCode = SelectedAlarmCode;
            bool previous = filling;
            filling = true;
            try
            {
                FillCategories();
                categoryValue.SelectedValue = category;
                FillTargets(target);
                FillActions(selected == null ? null : selected.Id);
                if (selected == null) actionValue.SelectedIndex = -1;
                FillExecutionChoices();
                FillAlarms(alarmCode);
                foreach (FlowInputRow row in InputRows) row.RefreshLanguage();
                RefreshOutputTitles(method);
                foreach (FlowBranchEditor editor in branchEditors)
                {
                    editor.RefreshDestinationLanguage();
                }
                RefreshMethodHelp();
                RefreshRetirementNotice();
            }
            finally { filling = previous; }
        }

        /// <summary>切换功能类别后重新选择真实对象及方法，旧动作输入不跨类别沿用。</summary>
        private void CategoryChanged(object sender, EventArgs args)
        {
            if (filling) return;
            filling = true;
            try { FillTargets(null); FillActions(null); FillInputs(new List<FlowInput>()); }
            finally { filling = false; }
        }

        /// <summary>切换实际对象后重建该对象的方法和输入。</summary>
        private void TargetChanged(object sender, EventArgs args)
        {
            if (filling) return;
            filling = true;
            try { FillActions(null); FillInputs(new List<FlowInput>()); }
            finally { filling = false; }
        }

        /// <summary>切换方法时从其字段默认值建立新输入。</summary>
        private void ActionChanged(object sender, EventArgs args)
        {
            if (!filling) FillInputs(new List<FlowInput>());
        }

        /// <summary>用同一窗口选择固定值或类型匹配的数据，引用保存稳定身份。</summary>
        private void PickInput(object sender, EventArgs args)
        {
            FlowInputRow row = (FlowInputRow)sender;
            FlowMethod action = CurrentMethod;
            if (action == null) return;
            FlowField field = action.Inputs.Single(item => item.Name == row.FieldName);
            string description = field.Name == "Index" && action.Id == "point.move" ? methodHint.Text : null;
            using (FlowSourceForm dialog = new FlowSourceForm(row.ReadInput(), catalog, action, field, description, Convert.ToString(targetValue.SelectedValue)))
                if (dialog.ShowDialog(this) == DialogResult.OK) row.SetInput(dialog.Selection);
        }

        /// <summary>完成字段校验后一次更新原节点，失败保留编辑窗口。</summary>
        private void SaveNode(object sender, EventArgs args)
        {
            try
            {
                FlowPrograms.RequireIdle();
                if (retiredAwaitingChoice) throw new InvalidOperationException(FlowText.T("retiredModeChoose"));
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

    }
}
