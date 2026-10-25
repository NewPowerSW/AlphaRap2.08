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

        private FlowMethod CurrentMethod { get { return executionMode == "Action" && !codeEditing ? actionValue.SelectedItem as FlowMethod : executionMethod; } }

        /// <summary>为三种执行方式建立独立内存草稿，所有修改在确定节点之前保持局部。</summary>
        private void InitializeExecution(FlowStep step)
        {
            editingStep = FlowProgramFile.Snapshot(new FlowProgram { Steps = new List<FlowStep> { step } }).Steps[0];
            executionMode = string.IsNullOrEmpty(step.ExecutionMode) ? "Action" : step.ExecutionMode;
            codeEditing = !string.IsNullOrEmpty(step.CodeModuleId);
            codeId = codeEditing ? step.CodeModuleId : "";
            if (codeEditing) codeDrafts[codeId] = FlowProgramFile.Snapshot(new FlowProgram { Steps = new List<FlowStep> { step } }).Steps[0];
            FillCodeChoices(); BindCodeDraft();
            modeInputs[InputModeKey] = CopyInputs(step.Inputs);
            scriptEditor.Bind(editingStep); scriptEditor.BindObjects(catalog);
            breakpointValue.Checked = step.Breakpoint;
            FillExecutionChoices();
            ApplyExecutionLayout();
            try { RefreshExecutionMethod(); }
            catch (Exception error) { executionMethod = null; scriptEditor.ShowError(error.Message); }
            if (executionMode == "Script") executionTabs.SelectedTab = scriptPage;
            actionMode.CheckedChanged += ExecutionChanged;
            scriptMode.CheckedChanged += ExecutionChanged;
            conditionMode.CheckedChanged += ExecutionChanged;
            codeMode.CheckedChanged += ExecutionChanged;
            codeChoice.SelectedIndexChanged += CodeChanged;
            codeBindings.BindingsChanged += CodeBindingsChanged;
            scriptEditor.DefinitionChanged += ScriptDefinitionChanged;
            scriptEditor.SourceRequested += (sender, args) => executionTabs.SelectedTab = scriptPage;
        }

        /// <summary>更新执行方式显示文字，保持当前稳定代码和脚本内容。</summary>
        private void FillExecutionChoices()
        {
            bool previous = filling;
            filling = true;
            actionMode.Checked = executionMode == "Action" && !codeEditing;
            codeMode.Checked = codeEditing;
            scriptMode.Checked = executionMode == "Script";
            conditionMode.Checked = executionMode == "None";
            FillCodeChoices();
            filling = previous;
        }

        /// <summary>复制绑定引用，模式切换不共用可变输入实例。</summary>
        private static List<FlowInput> CopyInputs(IEnumerable<FlowInput> inputs)
        {
            return inputs.Select(input => new FlowInput { Name = input.Name, Source = input.Source, Reference = input.Reference }).ToList();
        }

        /// <summary>保存当前参数行到本模式草稿；被暂时移除的脚本端口仍保留最后绑定。</summary>
        private void CaptureModeInputs()
        {
            CaptureCodeDraft();
            if (executionMode == "Action" && !codeEditing)
            {
                FlowMethod selected = actionValue.SelectedItem as FlowMethod;
                if (selected != null) editingStep.Action = selected.Id;
                editingStep.Target = Convert.ToString(targetValue.SelectedValue);
            }
            List<FlowInput> values;
            if (!modeInputs.TryGetValue(InputModeKey, out values)) values = new List<FlowInput>();
            modeInputs[InputModeKey] = values.Concat(InputRows.Select(row => row.ReadInput())).GroupBy(input => input.Name).Select(group => group.Last()).ToList();
        }

        /// <summary>切换节点执行方式，仅变换本窗口草稿与对应参数界面。</summary>
        private void ExecutionChanged(object sender, EventArgs args)
        {
            if (filling || !((System.Windows.Forms.RadioButton)sender).Checked) return;
            CaptureModeInputs(); scriptEditor.ReadTo(editingStep);
            codeEditing = sender == codeMode;
            executionMode = sender == scriptMode ? "Script" : sender == conditionMode ? "None" : "Action";
            filling = true;
            try
            {
                ApplyExecutionLayout(); RefreshExecutionMethod();
                RestoreModeInputs();
            }
            catch (Exception error) { scriptEditor.ShowError(error.Message); }
            finally { filling = false; }
        }

        /// <summary>以脚本端口或空动作声明更新参数元数据，业务执行仍由统一目录负责。</summary>
        private void RefreshExecutionMethod()
        {
            editingStep.ExecutionMode = executionMode;
            if (codeEditing)
            {
                FlowCodeDefinition definition = FlowToolboxCatalog.Find(codeId);
                executionMethod = definition == null ? null : FlowResourceAccess.CreateMethod(definition, CodeDraft(), catalog.Resources);
                return;
            }
            editingStep.CodeModuleId = ""; editingStep.CodeModuleVersion = 0;
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

        /// <summary>只显示本模式适用的同级任务页，源码和下一步不相互挤占空间。</summary>
        private void ApplyExecutionLayout()
        {
            bool action = executionMode == "Action" && !codeEditing;
            categoryValue.Enabled = action; targetValue.Enabled = action; actionValue.Enabled = action;
            actionSettings.Visible = action;
            codeSettings.Visible = codeEditing; codeRoutes.Visible = codeEditing;
            branchPanel.Visible = !codeEditing; addCase.Visible = !codeEditing; hint.Visible = !codeEditing;
            scriptSettings.Visible = executionMode == "Script";
            parameterPage.Name = codeEditing ? "codeModulePage" : action ? "scriptActionPage" : "scriptParameters";
            parameterPage.Tag = codeEditing ? null : action ? "nodeActionPage" : "nodeDataPage";
            if (codeEditing) parameterPage.Text = FlowCodeUiText.T("codeMode");
            executionTabs.SuspendLayout();
            executionTabs.TabPages.Clear();
            if (executionMode == "Script")
            {
                executionTabs.TabPages.Add(scriptPage);
                executionTabs.TabPages.Add(parameterPage);
            }
            if (action || codeEditing) executionTabs.TabPages.Add(parameterPage);
            executionTabs.TabPages.Add(branchPage);
            executionTabs.TabPages.Add(settingsPage);
            executionTabs.SelectedIndex = 0;
            executionTabs.ResumeLayout();
            FlowLanguageForm.ApplyTexts(executionTabs);
        }

        /// <summary>将源码、端口和断点收集到候选节点，保持编辑取消不会写入原工程。</summary>
        private void ReadExecutionTo(FlowStep candidate)
        {
            scriptEditor.ReadTo(candidate);
            candidate.ExecutionMode = executionMode;
            candidate.Breakpoint = breakpointValue.Checked;
            ReadCodeTo(candidate);
            if (executionMode == "Script") FlowScripts.Compile(candidate);
        }

        /// <summary>释放当前模式未挂入页签的编辑页，解除脚本和语言事件订阅。</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                foreach (System.Windows.Forms.TabPage page in new[] { parameterPage, scriptPage, branchPage, settingsPage })
                    if (page != null && page.Parent == null && !page.IsDisposed) page.Dispose();
            base.Dispose(disposing);
        }
    }
}
