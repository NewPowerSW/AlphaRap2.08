using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    /// <summary>将旧页面显式归入唯一文档；已拥有文档的页面不再从源码增删节点。</summary>
    internal sealed class FlowDocumentPages
    {
        private readonly Flow_BaseForm form;
        private readonly FlowDocumentCanvas[] hosts;

        /// <summary>仅绑定当前 Form 已声明的宿主，不硬编码工位类或页签名称。</summary>
        internal FlowDocumentPages(Flow_BaseForm owner)
        {
            form = owner; hosts = Enumerate(owner).ToArray();
            if (hosts.Any(host => string.IsNullOrWhiteSpace(host.DocumentKey)) || hosts.GroupBy(host => host.DocumentKey).Any(group => group.Count() != 1))
                throw new InvalidOperationException(FlowText.T("documentKey"));
        }

        /// <summary>页面第一次迁移采用有效旧配置，后续只读已保存文档的节点集合。</summary>
        internal void Prepare(FlowProgram program, FlowControlSource source)
        {
            foreach (FlowDocumentCanvas host in hosts)
            {
                FlowPageDefinition page = program.Pages.SingleOrDefault(item => item.Key == host.DocumentKey);
                if (page == null)
                {
                    page = new FlowPageDefinition { Key = host.DocumentKey, Caption = host.Parent.Text, Initializing = form.InitialPage.Contains(host) };
                    program.Pages.Add(page);
                    List<FlowStep> previous = program.Steps.Where(step => step.ParentPath == host.LegacyParentPath || step.ParentPath == Path(host)).ToList();
                    bool hasHistory = previous.Count > 0 || host.Templates.Any(node => program.RemovedControls.Contains(node.Name));
                    if (!hasHistory) previous = source.DocumentTemplates(Path(host));
                    foreach (FlowStep step in previous.Where(step => !program.RemovedControls.Contains(step.ControlName)))
                    {
                        if (!program.Steps.Contains(step)) program.Steps.Add(step);
                        if (host.Templates.Any(node => node.Name == step.ControlName))
                        {
                            step.ControlOrigin = FlowControlSource.FormOrigin;
                            source.ApplyNativeBinding(step); source.ApplyNativeAlarm(step);
                        }
                        step.PageId = page.Id; step.ControlOrigin = "Document";
                    }
                    Dictionary<string, FlowStep> names = program.Steps.ToDictionary(step => step.ControlName);
                    foreach (FlowStep step in previous.Where(step => !hasHistory && step.PageId == page.Id && step.SourceLinks != null))
                        foreach (string exit in new[] { "NEXT", "CASE1", "CASE2", "CASE3" })
                        {
                            FlowStep target; string name = step.SourceLinks.TargetOf(exit);
                            if (step.TargetOf(exit) != null || name == null || !names.TryGetValue(name, out target)) continue;
                            step.SetTarget(exit, target.Id);
                            if (step.Branches.All(branch => branch.Code != exit)) step.Branches.Add(new FlowBranch { Code = exit, Unconditional = true });
                        }
                }
                foreach (FlowStep step in program.Steps.Where(item => item.PageId == page.Id))
                {
                    step.ParentPath = Path(host); step.ControlOrigin = "Document";
                    Flow_Chart template = host.Templates.FirstOrDefault(node => node.Name == step.ControlName);
                    if (template != null && step.Width == 0) { step.Width = template.Width; step.Height = template.Height; }
                }
            }
        }

        /// <summary>拖入节点直接记录实际文档页身份，路径只作为渲染投影。</summary>
        internal string PageId(FlowProgram program, Control parent)
        {
            FlowDocumentCanvas host = FlowDocumentCanvas.Find(parent);
            return host == null ? null : program.Pages.Single(page => page.Key == host.DocumentKey).Id;
        }

        /// <summary>检查文档页面宿主存在，避免缺失页面时把节点移到其他生产页面。</summary>
        internal void Validate(FlowProgram program)
        {
            foreach (FlowPageDefinition page in program.Pages)
                if (hosts.All(host => host.DocumentKey != page.Key)) throw new InvalidOperationException(FlowText.T("documentHostMissing") + " " + page.Key);
        }

        /// <summary>统一生成相对 Form 的兼容路径。</summary>
        private string Path(Control control)
        {
            return control == null || control == form ? "" : Path(control.Parent) + "/" + control.Name;
        }

        /// <summary>枚举实际画布，不进入画布内部节点。</summary>
        private static IEnumerable<FlowDocumentCanvas> Enumerate(Control root)
        {
            foreach (Control child in root.Controls)
            {
                FlowDocumentCanvas host = child as FlowDocumentCanvas;
                if (host != null) { yield return host; continue; }
                foreach (FlowDocumentCanvas nested in Enumerate(child)) yield return nested;
            }
        }
    }
}
