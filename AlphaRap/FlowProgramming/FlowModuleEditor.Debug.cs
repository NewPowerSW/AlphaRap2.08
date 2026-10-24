using System;
using System.Collections.Generic;
using System.Linq;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    public sealed partial class FlowModuleEditor
    {
        public FlowDebugSnapshot DebugSnapshot { get; private set; }
        private FlowProgram debugPlan;
        private readonly Dictionary<string, FlowMethod> debugMethods = new Dictionary<string, FlowMethod>();

        /// <summary>固定本次运行的节点和字段标题，运行结束后的草稿编辑不改变观察结果归属。</summary>
        private void PrepareDebugPlan(FlowProgram plan, string entry, FlowActions actions, bool enabled)
        {
            debugPlan = enabled ? FlowProgramFile.Snapshot(plan) : null;
            debugMethods.Clear();
            if (!enabled) return;
            foreach (FlowStep step in FlowProgramFile.Reachable(debugPlan, entry)) debugMethods[step.Id] = actions.FindMethod(step);
        }

        /// <summary>读取本次执行快照的节点身份，不通过当前工程草稿重新解释旧结果。</summary>
        internal FlowStep FindDebugStep(string id)
        {
            return (debugPlan ?? Program).Steps.FirstOrDefault(step => step.Id == id);
        }

        /// <summary>读取启动时捕获的字段声明，缺失元数据时由观察面板显示稳定字段名。</summary>
        internal FlowMethod FindDebugMethod(string id)
        {
            FlowMethod method;
            return id != null && debugMethods.TryGetValue(id, out method) ? method : null;
        }

        /// <summary>保存运行器的调试快照，界面轮询只读引用而不阻塞执行线程。</summary>
        private void CaptureDebug(FlowDebugSnapshot snapshot)
        {
            FlowDebugSnapshot previous = DebugSnapshot;
            if (snapshot.State == "Finished" && previous != null && (string.IsNullOrEmpty(snapshot.StepId) || snapshot.StepId == previous.StepId))
                snapshot = new FlowDebugSnapshot { StepId = previous.StepId, State = snapshot.State, Inputs = previous.Inputs, Outputs = previous.Outputs, BranchCode = previous.BranchCode };
            DebugSnapshot = snapshot;
        }

        /// <summary>使用现有工程入口启动节点调试，参数、模拟和数据生命周期仍由同一运行器拥有。</summary>
        public void StartDebug(string entry, bool simulate, bool initializing = false)
        {
            FlowPrograms.RequireIdle();
            StartCore(entry, simulate, initializing, false, true);
        }

        /// <summary>切换节点断点草稿；统一保存后发布，运行中的快照不受影响。</summary>
        public void ToggleBreakpoint(string id)
        {
            FlowPrograms.RequireIdle();
            FlowStep step = Program.Steps.SingleOrDefault(item => item.Id == id);
            if (step == null) throw new InvalidOperationException(FlowText.T("methodMissing"));
            step.Breakpoint = !step.Breakpoint;
            ApplyLinks();
            if (Changed != null) Changed(this, EventArgs.Empty);
        }
    }
}
