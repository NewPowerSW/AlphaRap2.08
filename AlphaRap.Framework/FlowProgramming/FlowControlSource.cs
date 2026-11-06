using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    /// <summary>保存 Form 创建时的原生控件快照，在恢复工程前同步源码删除与连接变化。</summary>
    internal sealed class FlowControlSource
    {
        internal const string FormOrigin = "Form";
        internal const string DesignerOrigin = "Designer";
        internal const string MissingOrigin = "MissingForm";
        private static readonly string[] Exits = { "NEXT", "CASE1", "CASE2", "CASE3" };
        private readonly Dictionary<string, FlowStep> controls = new Dictionary<string, FlowStep>(StringComparer.Ordinal);
        private readonly HashSet<string> documentControls = new HashSet<string>(StringComparer.Ordinal);

        /// <summary>在保存连接覆盖 SDK 控件之前记录源码位置、容器及四个出口，不执行事件。</summary>
        internal FlowControlSource(Control form)
        {
            foreach (Flow_Chart node in Enumerate(form))
            {
                FlowStep source = new FlowStep { ControlName = node.Name, Caption = node.text, X = node.Left, Y = node.Top, Width = node.Width, Height = node.Height,
                    ParentPath = ParentPath(node.Parent, form), ControlOrigin = FormOrigin,
                    TimeoutMs = node.AlarmStaus ? Math.Max(1, node.AlarmTimeOut) : System.Threading.Timeout.Infinite,
                    AlarmCode = node.AlarmStaus ? node.AlarmCode : null,
                    SourceLinks = new FlowControlLinks { Next = node.Next == null ? null : node.Next.Name,
                        Case1 = node.Casse1 == null ? null : node.Casse1.Name, Case2 = node.Casse2 == null ? null : node.Casse2.Name,
                        Case3 = node.Casse3 == null ? null : node.Casse3.Name } };
                FlowNativeMethods.BindExisting(form as Flow_BaseForm, node, source);
                controls.Add(node.Name, source);
                if (FlowDocumentCanvas.Find(node.Parent) != null) documentControls.Add(node.Name);
            }
        }

        /// <summary>源码原生节点沿用构造时的报警开关、期限及编号，不接受功能节点的默认参数。</summary>
        internal void ApplyNativeAlarm(FlowStep step)
        {
            if (!step.UsesFlowResult || step.ControlOrigin != FormOrigin) return;
            FlowStep source;
            if (!controls.TryGetValue(step.ControlName, out source)) return;
            step.TimeoutMs = source.TimeoutMs;
            step.AlarmCode = source.AlarmCode;
        }

        /// <summary>源码控件的父页面、标题及位置始终取设计器实际定义，动态节点不受影响。</summary>
        internal void ApplyLayout(FlowStep step)
        {
            if (!string.IsNullOrEmpty(step.PageId)) return;
            FlowStep source;
            if (!controls.TryGetValue(step.ControlName, out source)) return;
            step.ParentPath = source.ParentPath;
            step.Caption = source.Caption;
            step.X = source.X;
            step.Y = source.Y;
        }

        /// <summary>原生节点执行设计器当前绑定的本页事件，不沿用已替换的旧方法。</summary>
        internal void ApplyNativeBinding(FlowStep step)
        {
            FlowStep source;
            if (step.ControlOrigin != FormOrigin || !controls.TryGetValue(step.ControlName, out source)) return;
            if (!source.UsesFlowResult) return;
            step.Action = source.Action;
            step.Target = source.Target;
            step.ExecutionMode = "Action";
            step.UsesFlowResult = true;
            step.Inputs.Clear();
        }

        /// <summary>按当前源码标记节点来源并同步原生连线，失效节点由工程统一清理。</summary>
        internal FlowProgram Prepare(FlowProgram program)
        {
            FlowProgram candidate = FlowProgramFile.Snapshot(program);
            if (candidate.Removed) return candidate;
            foreach (FlowStep source in controls.Values)
                if (!documentControls.Contains(source.ControlName) && !candidate.RemovedControls.Contains(source.ControlName) && candidate.Steps.All(step => step.ControlName != source.ControlName))
                    candidate.Steps.Add(CopySource(source));
            foreach (FlowStep step in candidate.Steps) IdentifyOrigin(step);
            Dictionary<string, FlowStep> byId = candidate.Steps.ToDictionary(step => step.Id);
            Dictionary<string, FlowStep> byName = candidate.Steps.ToDictionary(step => step.ControlName);
            HashSet<string> removed = new HashSet<string>(candidate.Steps.Where(step => step.ControlOrigin == MissingOrigin).Select(step => step.Id));
            foreach (FlowStep step in candidate.Steps.Where(step => step.ControlOrigin == FormOrigin))
                SynchronizeLinks(step, controls[step.ControlName].SourceLinks, byId, byName, removed);
            return candidate;
        }

        /// <summary>区分实际源码控件、明确拖拽节点和旧版原生节点，不把动态节点作为已删源码清理。</summary>
        private void IdentifyOrigin(FlowStep step)
        {
            if (!string.IsNullOrEmpty(step.PageId)) { step.ControlOrigin = "Document"; return; }
            if (controls.ContainsKey(step.ControlName)) { step.ControlOrigin = FormOrigin; return; }
            Guid generated;
            bool designerName = step.ControlName != null && step.ControlName.StartsWith("flow_", StringComparison.Ordinal)
                && Guid.TryParseExact(step.ControlName.Substring(5), "N", out generated);
            bool oldNative = string.IsNullOrEmpty(step.ControlOrigin) && !designerName
                && (step.Action == "legacy" || step.UsesFlowResult && (step.Target ?? "").StartsWith("native/", StringComparison.Ordinal));
            if (step.ControlOrigin == FormOrigin || step.ControlOrigin == MissingOrigin || oldNative)
                step.ControlOrigin = MissingOrigin;
            else if (string.IsNullOrEmpty(step.ControlOrigin)) step.ControlOrigin = DesignerOrigin;
        }

        /// <summary>只更新仍沿用旧源码的连接，或指向已删源码节点的连接；保留主动插入的混合流程连接。</summary>
        private static void SynchronizeLinks(FlowStep step, FlowControlLinks current, IDictionary<string, FlowStep> byId,
            IDictionary<string, FlowStep> byName, ISet<string> removed)
        {
            foreach (string exit in Exits)
            {
                string target = step.TargetOf(exit);
                FlowStep saved;
                string savedName = target != null && byId.TryGetValue(target, out saved) ? saved.ControlName : null;
                string sourceName = current.TargetOf(exit);
                bool followsSource = step.SourceLinks != null && savedName == step.SourceLinks.TargetOf(exit)
                    && sourceName != step.SourceLinks.TargetOf(exit);
                if (!followsSource && (target == null || !removed.Contains(target))) continue;
                FlowStep destination = null;
                if (sourceName != null && !byName.TryGetValue(sourceName, out destination)) continue;
                step.SetTarget(exit, destination == null ? null : destination.Id);
                if (destination != null && step.UsesFlowResult && step.Branches.All(branch => branch.Code != exit))
                    step.Branches.Add(new FlowBranch { Code = exit, Unconditional = true });
            }
            step.SourceLinks = CopyLinks(current);
        }

        /// <summary>仅为尚无历史定义的文档页提供首次导入模板。</summary>
        internal List<FlowStep> DocumentTemplates(string path)
        {
            return controls.Values.Where(step => step.ParentPath == path).Select(step =>
                FlowProgramFile.Snapshot(new FlowProgram { Steps = new List<FlowStep> { step } }).Steps[0]).ToList();
        }

        /// <summary>复制源码节点模板并赋予独立持久化身份。</summary>
        private static FlowStep CopySource(FlowStep source)
        {
            return new FlowStep { ControlName = source.ControlName, Caption = source.Caption, X = source.X, Y = source.Y,
                ParentPath = source.ParentPath, ControlOrigin = FormOrigin, SourceLinks = CopyLinks(source.SourceLinks) };
        }

        /// <summary>复制四个源码目标，避免工程快照反向修改编译时基准。</summary>
        private static FlowControlLinks CopyLinks(FlowControlLinks links)
        {
            return new FlowControlLinks { Next = links.Next, Case1 = links.Case1, Case2 = links.Case2, Case3 = links.Case3 };
        }

        /// <summary>枚举页面源码中的原生节点，不进入节点内部装饰控件。</summary>
        private static IEnumerable<Flow_Chart> Enumerate(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                Flow_Chart node = child as Flow_Chart;
                if (node != null) { yield return node; continue; }
                foreach (Flow_Chart nested in Enumerate(child)) yield return nested;
            }
        }

        /// <summary>记录相对所属 Form 的容器路径。</summary>
        private static string ParentPath(Control parent, Control form)
        {
            return parent == form || parent == null ? "" : ParentPath(parent.Parent, form) + "/" + parent.Name;
        }
    }
}
