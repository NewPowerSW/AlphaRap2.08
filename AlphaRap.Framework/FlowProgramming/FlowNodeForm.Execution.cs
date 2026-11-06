using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    public partial class FlowNodeForm
    {
        private string executionMode = "Action";
        private FlowMethod executionMethod;
        private bool retiredAwaitingChoice;
        private readonly Dictionary<string, List<FlowInput>> modeInputs = new Dictionary<string, List<FlowInput>>();

        private FlowMethod CurrentMethod
        {
            get
            {
                if (retiredAwaitingChoice) return null;
                return executionMode == "Action" ? actionValue.SelectedItem as FlowMethod : executionMethod;
            }
        }

        /// <summary>绑定现有功能或条件节点，旧脚本及代码模块只能由用户明确转换。</summary>
        private void InitializeExecution(FlowStep step)
        {
            retiredAwaitingChoice = !FlowActions.IsSupported(step);
            executionMode = retiredAwaitingChoice ? "" : step.ExecutionMode;
            if (string.IsNullOrEmpty(executionMode) && !retiredAwaitingChoice) executionMode = "Action";
            if (!retiredAwaitingChoice) modeInputs[executionMode] = CopyInputs(step.Inputs);
            breakpointValue.Checked = step.Breakpoint;
            FillExecutionChoices();
            RefreshExecutionMethod();
            ApplyExecutionLayout();
            actionMode.CheckedChanged += ExecutionChanged;
            conditionMode.CheckedChanged += ExecutionChanged;
        }

        /// <summary>按稳定模式代码选择按钮，旧脚本或代码模块保持未选择状态。</summary>
        private void FillExecutionChoices()
        {
            bool previous = filling;
            filling = true;
            actionMode.Checked = !retiredAwaitingChoice && executionMode == "Action";
            conditionMode.Checked = !retiredAwaitingChoice && executionMode == "None";
            filling = previous;
        }

        /// <summary>说明旧节点的转换要求，取消窗口始终保留原定义。</summary>
        private void RefreshRetirementNotice()
        {
            retiredNotice.Text = retiredAwaitingChoice ? FlowText.T("retiredModeNotice") : "";
            retiredNotice.Visible = retiredAwaitingChoice;
            root.RowStyles[1].Height = retiredAwaitingChoice ? 76 : 0;
        }

        /// <summary>复制输入绑定，模式草稿不共享可变引用。</summary>
        private static List<FlowInput> CopyInputs(IEnumerable<FlowInput> values)
        {
            return values.Select(input => new FlowInput { Name = input.Name, Source = input.Source, Reference = input.Reference }).ToList();
        }

        /// <summary>记录当前方式的输入，已停用节点的脚本参数不导入动作草稿。</summary>
        private void CaptureModeInputs()
        {
            if (retiredAwaitingChoice) return;
            modeInputs[executionMode] = InputRows.Select(row => row.ReadInput()).ToList();
        }

        /// <summary>恢复当前方式的输入；首次转换时使用所选方法的字段默认值。</summary>
        private void RestoreModeInputs()
        {
            List<FlowInput> values;
            FillInputs(modeInputs.TryGetValue(executionMode, out values) ? CopyInputs(values) : new List<FlowInput>());
        }

        /// <summary>只切换本窗口的动作和条件草稿，确定前不修改原工程。</summary>
        private void ExecutionChanged(object sender, EventArgs args)
        {
            if (filling || !((RadioButton)sender).Checked) return;
            CaptureModeInputs();
            retiredAwaitingChoice = false;
            executionMode = sender == actionMode ? "Action" : "None";
            filling = true;
            try
            {
                RefreshExecutionMethod();
                ApplyExecutionLayout();
                RestoreModeInputs();
            }
            finally { filling = false; }
        }

        /// <summary>条件节点只提供空动作元数据，现有功能直接读取所选对象方法。</summary>
        private void RefreshExecutionMethod()
        {
            executionMethod = null;
            if (!retiredAwaitingChoice && executionMode == "None")
                executionMethod = catalog.FindMethod(new FlowStep { ExecutionMode = "None", Action = "flow.none" });
        }

        /// <summary>现有功能显示动作设置，条件节点直接进入下一步，固定控件均由设计器管理。</summary>
        private void ApplyExecutionLayout()
        {
            bool action = !retiredAwaitingChoice && executionMode == "Action";
            actionSettings.Visible = action;
            actionSettings.Enabled = action;
            executionTabs.SuspendLayout();
            executionTabs.TabPages.Clear();
            if (action) executionTabs.TabPages.Add(parameterPage);
            executionTabs.TabPages.Add(branchPage);
            executionTabs.TabPages.Add(settingsPage);
            executionTabs.SelectedIndex = 0;
            executionTabs.ResumeLayout();
            FlowLanguageForm.ApplyTexts(executionTabs);
            RefreshRetirementNotice();
        }

        /// <summary>写入明确选择的执行方式，并移除已停用的源码、端口和模块角色。</summary>
        private void ReadExecutionTo(FlowStep candidate)
        {
            candidate.ExecutionMode = executionMode;
            candidate.Breakpoint = breakpointValue.Checked;
            candidate.ScriptSource = "";
            candidate.ScriptInputs = new List<FlowScriptPort>();
            candidate.ScriptOutputs = new List<FlowScriptPort>();
            candidate.ScriptResources = new List<string>();
            candidate.CodeModuleId = "";
            candidate.CodeModuleVersion = 0;
            candidate.ResourceBindings = new List<FlowResourceBinding>();
        }

        /// <summary>释放按模式暂时移出页签的页面。</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                foreach (TabPage page in new[] { parameterPage, branchPage, settingsPage })
                    if (page != null && page.Parent == null && !page.IsDisposed) page.Dispose();
            base.Dispose(disposing);
        }
    }
}
