using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    public partial class FlowDebugPanel : UserControl
    {
        private FlowModuleEditor module;
        private FlowDebugSnapshot snapshot;
        public event EventHandler ContinueRequested;
        public event EventHandler StepRequested;

        /// <summary>创建只读观察窗口；不拥有运行任务或配置保存入口。</summary>
        public FlowDebugPanel()
        {
            InitializeComponent();
            if (UiTheme.IsDesignTime) return;
            debugContinue.Click += (sender, args) => { if (ContinueRequested != null) ContinueRequested(this, EventArgs.Empty); };
            debugStep.Click += (sender, args) => { if (StepRequested != null) StepRequested(this, EventArgs.Empty); };
        }

        /// <summary>切换模块或调试快照时更新观察值，不重复采样设备或执行表达式。</summary>
        public void Bind(FlowModuleEditor current)
        {
            bool changed = !ReferenceEquals(module, current) || !ReferenceEquals(snapshot, current == null ? null : current.DebugSnapshot);
            module = current;
            snapshot = module == null ? null : module.DebugSnapshot;
            debugContinue.Enabled = debugStep.Enabled = module != null && FlowPrograms.CanEdit && module.Runtime.IsDebugPaused;
            if (changed) RefreshValues();
        }

        /// <summary>翻译标签和观察字段显示名，保留运行器已产生的业务值。</summary>
        public void RefreshLanguage()
        {
            FlowLanguageForm.ApplyTexts(this);
            RefreshValues();
        }

        /// <summary>根据不可变快照显示准备、暂停、动作结果和最终出口。</summary>
        private void RefreshValues()
        {
            debugValues.Rows.Clear();
            if (snapshot == null) { debugTitle.Text = FlowText.T("debugReady"); return; }
            FlowStep step = module.FindDebugStep(snapshot.StepId);
            debugTitle.Text = string.Format(FlowText.T("debugNodeState"), module.Form.Name, step == null ? "" : step.Caption, StateTitle(snapshot.State));
            FlowMethod method = module.FindDebugMethod(snapshot.StepId);
            AddValues(snapshot.Inputs, method, false);
            AddValues(snapshot.Outputs, method, true);
            if (!string.IsNullOrEmpty(snapshot.BranchCode)) debugValues.Rows.Add(FlowText.T("debugExit"), snapshot.BranchCode, step == null ? "" : Destination(step.TargetOf(snapshot.BranchCode)));
        }

        /// <summary>把输出出口映射到本模块真实节点名称，空连接显示结束。</summary>
        private string Destination(string id)
        {
            FlowStep target = module.FindDebugStep(id);
            return target == null ? FlowText.T("end") : target.Caption;
        }

        /// <summary>读取调试状态的固定三语标题。</summary>
        private static string StateTitle(string state)
        {
            if (state == "Paused") return FlowText.T("DebugPaused");
            return FlowText.T("debug" + state);
        }

        /// <summary>显示实际输入输出，无法读取的来源明确保留原因，不用零值代替。</summary>
        private void AddValues(IDictionary<string, object> values, FlowMethod method, bool output)
        {
            if (values == null) return;
            foreach (KeyValuePair<string, object> value in values)
            {
                FlowField field = method == null ? null : (output ? method.Outputs : method.Inputs).FirstOrDefault(item => item.Name == value.Key);
                string title = field == null ? value.Key : FlowMethods.FieldTitle(method.Id, field, output);
                FlowDebugUnavailable missing = value.Value as FlowDebugUnavailable;
                string display = missing == null ? DisplayValue(value.Value) : string.Format(FlowText.T("debugUnavailable"), missing.Message);
                int row = debugValues.Rows.Add(FlowText.T(output ? "debugOutput" : "debugInput"), title, display);
                debugValues.Rows[row].Cells[2].ToolTipText = display;
            }
        }

        /// <summary>以不变数值格式展示快照，数组仅预览前十六项。</summary>
        private static string DisplayValue(object value)
        {
            if (value == null) return "(null)";
            Array array = value as Array;
            if (array != null) return "[" + string.Join(", ", array.Cast<object>().Take(16).Select(item => Convert.ToString(item, CultureInfo.InvariantCulture))) + (array.Length > 16 ? ", …]" : "]");
            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }
    }
}
