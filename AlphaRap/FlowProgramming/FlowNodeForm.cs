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
            filling = true;
            captionValue.Text = step.Caption;
            timeoutValue.Value = Math.Max(timeoutValue.Minimum, Math.Min(timeoutValue.Maximum, step.TimeoutMs));
            FillAlarms(string.IsNullOrEmpty(step.AlarmCode) ? "2019" : step.AlarmCode);
            InitializeExecution(step);
            RestoreModeInputs();
            foreach (FlowBranch branch in FlowProgramFile.BranchesOf(step)) AddBranchEditor(branch, step.TargetOf(branch.Code));
            filling = false;
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

        /// <summary>按对象声明建立参数行，已有非默认高级值自动展开。</summary>
        private void FillInputs(List<FlowInput> previous)
        {
            foreach (Control control in inputs.Controls.Cast<Control>().ToArray()) control.Dispose();
            FlowMethod action = CurrentMethod;
            if (action == null) { RefreshOutputTitles(null); RefreshMethodHelp(); return; }
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

        /// <summary>无输入端口时收起空参数区域，并显示简短说明。</summary>
        private void RefreshMethodHelp()
        {
            FlowMethod action = CurrentMethod;
            methodHint.Text = action != null && action.Inputs.Count == 0 ? FlowText.T("noInputs") : "";
            bool hasHelp = methodHint.Text.Length > 0;
            methodHint.Visible = hasHelp;
            inputLayout.RowStyles[2].Height = hasHelp ? 52 : 0;
            RefreshParameterLayout();
        }

        /// <summary>刷新执行方式和来源的显示语言，保留输入绑定及未提交草稿。</summary>
        protected override void RefreshLanguageChoices()
        {
            if (catalog == null) return;
            FlowMethod method = CurrentMethod;
            string alarmCode = SelectedAlarmCode;
            bool previous = filling;
            filling = true;
            try
            {
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

        /// <summary>用同一窗口选择固定值或类型匹配的数据，引用保存稳定身份。</summary>
        private void PickInput(object sender, EventArgs args)
        {
            FlowInputRow row = (FlowInputRow)sender;
            FlowMethod action = CurrentMethod;
            if (action == null) return;
            FlowField field = action.Inputs.Single(item => item.Name == row.FieldName);
            using (FlowSourceForm dialog = new FlowSourceForm(row.ReadInput(), catalog, action, field))
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
                candidate.Target = "";
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
