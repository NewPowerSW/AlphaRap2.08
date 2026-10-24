using System;
using System.Collections.Generic;
using System.Linq;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    public partial class FlowNodeForm
    {
        private FlowStep editingStep;
        private string executionMode = "Action";
        private FlowMethod executionMethod;
        private readonly Dictionary<string, List<FlowInput>> modeInputs = new Dictionary<string, List<FlowInput>>();

        private FlowMethod CurrentMethod { get { return executionMode == "Action" ? actionValue.SelectedItem as FlowMethod : executionMethod; } }

        /// <summary>为三种执行方式建立独立内存草稿，所有修改在确定节点之前保持局部。</summary>
        private void InitializeExecution(FlowStep step)
        {
            editingStep = FlowProgramFile.Snapshot(new FlowProgram { Steps = new List<FlowStep> { step } }).Steps[0];
            executionMode = string.IsNullOrEmpty(step.ExecutionMode) ? "Action" : step.ExecutionMode;
            modeInputs[executionMode] = CopyInputs(step.Inputs);
            scriptEditor.Bind(editingStep); scriptEditor.BindObjects(catalog);
            breakpointValue.Checked = step.Breakpoint;
            FillExecutionChoices();
            ApplyExecutionLayout();
            try { RefreshExecutionMethod(); }
            catch (Exception error) { executionMethod = null; scriptEditor.ShowError(error.Message); }
            if (executionMode == "Script") executionTabs.SelectedTab = scriptPage;
            executionValue.SelectedIndexChanged += ExecutionChanged;
            scriptEditor.DefinitionChanged += ScriptDefinitionChanged;
        }

        /// <summary>更新执行方式显示文字，保持当前稳定代码和脚本内容。</summary>
        private void FillExecutionChoices()
        {
            executionValue.DisplayMember = "Label"; executionValue.ValueMember = "Id";
            executionValue.DataSource = new[] { new FlowChoice { Id = "Action", Label = FlowScriptUiText.T("scriptAction") }, new FlowChoice { Id = "Script", Label = FlowScriptUiText.T("scriptCustom") }, new FlowChoice { Id = "None", Label = FlowScriptUiText.T("scriptNone") } };
            executionValue.SelectedValue = executionMode;
        }

        /// <summary>复制绑定引用，模式切换不共用可变输入实例。</summary>
        private static List<FlowInput> CopyInputs(IEnumerable<FlowInput> inputs)
        {
            return inputs.Select(input => new FlowInput { Name = input.Name, Source = input.Source, Reference = input.Reference }).ToList();
        }

        /// <summary>保存当前参数行到本模式草稿；被暂时移除的脚本端口仍保留最后绑定。</summary>
        private void CaptureModeInputs()
        {
            if (executionMode == "Action")
            {
                FlowMethod selected = actionValue.SelectedItem as FlowMethod;
                if (selected != null) editingStep.Action = selected.Id;
                editingStep.Target = Convert.ToString(targetValue.SelectedValue);
            }
            List<FlowInput> values;
            if (!modeInputs.TryGetValue(executionMode, out values)) values = new List<FlowInput>();
            modeInputs[executionMode] = values.Concat(InputRows.Select(row => row.ReadInput())).GroupBy(input => input.Name).Select(group => group.Last()).ToList();
        }

        /// <summary>切换节点执行方式，仅变换本窗口草稿与对应参数界面。</summary>
        private void ExecutionChanged(object sender, EventArgs args)
        {
            if (filling) return;
            CaptureModeInputs(); scriptEditor.ReadTo(editingStep);
            executionMode = Convert.ToString(executionValue.SelectedValue);
            filling = true;
            try
            {
                ApplyExecutionLayout(); RefreshExecutionMethod();
                List<FlowInput> previous;
                FillInputs(modeInputs.TryGetValue(executionMode, out previous) ? previous : new List<FlowInput>());
            }
            catch (Exception error) { scriptEditor.ShowError(error.Message); }
            finally { filling = false; if (executionMode == "Script") executionTabs.SelectedTab = scriptPage; }
        }

        /// <summary>以脚本端口或空动作声明更新参数元数据，业务执行仍由统一目录负责。</summary>
        private void RefreshExecutionMethod()
        {
            editingStep.ExecutionMode = executionMode;
            if (executionMode == "Action") { executionMethod = null; return; }
            executionMethod = null;
            scriptEditor.ReadTo(editingStep);
            executionMethod = catalog.FindMethod(editingStep);
        }

        /// <summary>脚本端口确认编辑后保留同名输入绑定，再刷新条件可选结果。</summary>
        private void ScriptDefinitionChanged(object sender, EventArgs args)
        {
            if (filling || executionMode != "Script") return;
            CaptureModeInputs();
            filling = true;
            try { RefreshExecutionMethod(); FillInputs(modeInputs["Script"]); scriptEditor.ShowError(""); }
            catch (Exception error) { scriptEditor.ShowError(error.Message); }
            finally { filling = false; }
        }

        /// <summary>脚本以内嵌页签编辑，普通对象配置不显示脚本区。</summary>
        private void ApplyExecutionLayout()
        {
            bool action = executionMode == "Action";
            categoryValue.Enabled = action; targetValue.Enabled = action; actionValue.Enabled = action;
            foreach (System.Windows.Forms.Control control in new System.Windows.Forms.Control[] { categoryLabel, categoryValue, targetLabel, targetValue, actionLabel, actionValue }) control.Visible = action;
            header.SetColumnSpan(captionValue, action ? 1 : 3);
            header.RowStyles[1].SizeType = action ? System.Windows.Forms.SizeType.Percent : System.Windows.Forms.SizeType.Absolute;
            header.RowStyles[1].Height = action ? 33.33F : 0;
            root.RowStyles[0].Height = action ? 138 : 92;
            executionTabs.Visible = executionMode != "None";
            if (executionMode == "Script")
            {
                if (!executionTabs.TabPages.Contains(scriptPage)) executionTabs.TabPages.Add(scriptPage);
                if (!executionTabs.TabPages.Contains(branchPage)) executionTabs.TabPages.Add(branchPage);
                branchPage.Controls.Add(branchesGroup);
                root.SetRowSpan(executionTabs, 2);
                root.RowStyles[2].Height = 76;
            }
            else
            {
                executionTabs.TabPages.Remove(scriptPage);
                executionTabs.TabPages.Remove(branchPage);
                root.SetRowSpan(executionTabs, 1);
                root.Controls.Add(branchesGroup, 0, 3);
                executionTabs.SelectedTab = parameterPage;
            }
        }

        /// <summary>将源码、端口和断点收集到候选节点，保持编辑取消不会写入原工程。</summary>
        private void ReadExecutionTo(FlowStep candidate)
        {
            scriptEditor.ReadTo(candidate);
            candidate.ExecutionMode = executionMode;
            candidate.Breakpoint = breakpointValue.Checked;
            if (executionMode == "Script") FlowScripts.Compile(candidate);
        }
    }
}
