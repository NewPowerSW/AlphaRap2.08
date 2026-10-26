using System;
using System.Collections.Generic;
using System.Linq;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    public partial class FlowNodeForm
    {
        private FlowStep editingStep;
        private string executionMode = "Script";
        private FlowMethod executionMethod;
        private bool retiredAwaitingChoice;
        private readonly Dictionary<string, List<FlowInput>> modeInputs = new Dictionary<string, List<FlowInput>>();

        private FlowMethod CurrentMethod { get { return executionMethod; } }

        /// <summary>建立脚本及条件节点的局部草稿，旧执行方式须由用户明确转换。</summary>
        private void InitializeExecution(FlowStep step)
        {
            editingStep = FlowProgramFile.Snapshot(new FlowProgram { Steps = new List<FlowStep> { step } }).Steps[0];
            retiredAwaitingChoice = !FlowActions.IsSupported(step);
            executionMode = retiredAwaitingChoice ? "" : step.ExecutionMode;
            if (!retiredAwaitingChoice) modeInputs[executionMode] = CopyInputs(step.Inputs);
            scriptEditor.Bind(editingStep); scriptEditor.BindObjects(catalog);
            breakpointValue.Checked = step.Breakpoint;
            FillExecutionChoices();
            ApplyExecutionLayout();
            try { RefreshExecutionMethod(); }
            catch (Exception error) { executionMethod = null; scriptEditor.ShowError(error.Message); }
            scriptMode.CheckedChanged += ExecutionChanged;
            conditionMode.CheckedChanged += ExecutionChanged;
            scriptEditor.DefinitionChanged += ScriptDefinitionChanged;
            scriptEditor.SourceRequested += (sender, args) => executionTabs.SelectedTab = scriptPage;
        }

        /// <summary>按稳定执行代码更新按钮，不把旧执行方式自动变成空脚本。</summary>
        private void FillExecutionChoices()
        {
            bool previous = filling;
            filling = true;
            scriptMode.Checked = !retiredAwaitingChoice && executionMode == "Script";
            conditionMode.Checked = !retiredAwaitingChoice && executionMode == "None";
            filling = previous;
        }

        /// <summary>显示旧执行方式的转换要求，保留取消窗口的原始定义。</summary>
        private void RefreshRetirementNotice()
        {
            retiredNotice.Text = retiredAwaitingChoice ? FlowText.T("retiredModeNotice") : "";
            retiredNotice.Visible = retiredAwaitingChoice;
            root.RowStyles[1].Height = retiredAwaitingChoice ? 76 : 0;
        }

        /// <summary>复制绑定引用，模式切换不共用可变输入实例。</summary>
        private static List<FlowInput> CopyInputs(IEnumerable<FlowInput> inputs)
        {
            return inputs.Select(input => new FlowInput { Name = input.Name, Source = input.Source, Reference = input.Reference }).ToList();
        }

        /// <summary>保存当前参数行到本模式草稿；被暂时移除的脚本端口仍保留最后绑定。</summary>
        private void CaptureModeInputs()
        {
            if (retiredAwaitingChoice) return;
            List<FlowInput> values;
            if (!modeInputs.TryGetValue(executionMode, out values)) values = new List<FlowInput>();
            modeInputs[executionMode] = values.Concat(InputRows.Select(row => row.ReadInput())).GroupBy(input => input.Name).Select(group => group.Last()).ToList();
        }

        /// <summary>从本模式草稿恢复输入，首次使用时采用脚本端口声明的默认值。</summary>
        private void RestoreModeInputs()
        {
            List<FlowInput> previous;
            FillInputs(modeInputs.TryGetValue(executionMode, out previous) ? previous : new List<FlowInput>());
        }

        /// <summary>切换节点执行方式，仅变换本窗口草稿与对应参数界面。</summary>
        private void ExecutionChanged(object sender, EventArgs args)
        {
            if (filling || !((System.Windows.Forms.RadioButton)sender).Checked) return;
            CaptureModeInputs(); scriptEditor.ReadTo(editingStep);
            retiredAwaitingChoice = false;
            executionMode = sender == scriptMode ? "Script" : "None";
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
            executionMethod = null;
            if (retiredAwaitingChoice) return;
            scriptEditor.ReadTo(editingStep);
            editingStep.ExecutionMode = executionMode;
            editingStep.CodeModuleId = "";
            editingStep.CodeModuleVersion = 0;
            editingStep.ResourceBindings = new List<FlowResourceBinding>();
            editingStep.Target = "";
            executionMethod = FlowScripts.Define(editingStep);
        }

        /// <summary>脚本端口确认编辑后保留同名输入绑定，再刷新条件可选结果。</summary>
        private void ScriptDefinitionChanged(object sender, EventArgs args)
        {
            if (filling || retiredAwaitingChoice || executionMode != "Script") return;
            CaptureModeInputs();
            filling = true;
            try { RefreshExecutionMethod(); RestoreModeInputs(); scriptEditor.ShowError(""); }
            catch (Exception error) { scriptEditor.ShowError(error.Message); }
            finally { filling = false; }
        }

        /// <summary>只显示本模式适用的同级任务页，源码和下一步不相互挤占空间。</summary>
        private void ApplyExecutionLayout()
        {
            bool script = !retiredAwaitingChoice && executionMode == "Script";
            scriptSettings.Visible = script;
            executionTabs.SuspendLayout();
            executionTabs.TabPages.Clear();
            if (script)
            {
                executionTabs.TabPages.Add(scriptPage);
                executionTabs.TabPages.Add(parameterPage);
            }
            executionTabs.TabPages.Add(branchPage);
            executionTabs.TabPages.Add(settingsPage);
            executionTabs.SelectedIndex = 0;
            executionTabs.ResumeLayout();
            FlowLanguageForm.ApplyTexts(executionTabs);
            RefreshRetirementNotice();
        }

        /// <summary>将源码、端口和断点收集到候选节点，保持编辑取消不会写入原工程。</summary>
        private void ReadExecutionTo(FlowStep candidate)
        {
            scriptEditor.ReadTo(candidate);
            candidate.ExecutionMode = executionMode;
            candidate.Breakpoint = breakpointValue.Checked;
            candidate.Target = "";
            candidate.CodeModuleId = "";
            candidate.CodeModuleVersion = 0;
            candidate.ResourceBindings = new List<FlowResourceBinding>();
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
