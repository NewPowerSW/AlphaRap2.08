using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    [Designer(typeof(FlowDocumentCanvasDesigner))]
    public partial class FlowDocumentCanvas : UserControl
    {
        private bool renderingPreview;
        private string documentKey = "";
        private string moduleId = "";
        private string legacyParentPath = "";
        private string previewFile = "";
        private readonly List<Flow_Chart> templates = new List<Flow_Chart>();
        [DefaultValue("")]
        public string DocumentKey { get { return documentKey; } set { documentKey = value ?? ""; RefreshDesignPreview(); } }
        [DefaultValue("")]
        public string ModuleId { get { return moduleId; } set { moduleId = value ?? ""; RefreshDesignPreview(); } }
        [DefaultValue("")]
        public string LegacyParentPath { get { return legacyParentPath; } set { legacyParentPath = value ?? ""; RefreshDesignPreview(); } }
        [DefaultValue("")]
        public string PreviewFile { get { return previewFile; } set { previewFile = value ?? ""; RefreshDesignPreview(); } }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ControlCollection Controls { get { return base.Controls; } }
        internal IEnumerable<Flow_Chart> Templates { get { return templates; } }
        internal bool IsDesignPreview { get { return LicenseManager.UsageMode == LicenseUsageMode.Designtime || DesignMode || UiTheme.IsDesignTime; } }

        /// <summary>建立空的文档画布，实际节点由统一工程恢复。</summary>
        public FlowDocumentCanvas() { InitializeComponent(); }

        /// <summary>设计器完成挂载后只读取工程文件，不解析设备或调用业务事件。</summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            RefreshDesignPreview();
        }

        /// <summary>登记首次迁移模板并保留旧事件引用所需的同一控件实例。</summary>
        public void RegisterLegacyNodes(params Flow_Chart[] nodes)
        {
            if (IsDesignPreview) return;
            foreach (Flow_Chart node in nodes)
            {
                templates.Add(node);
                Controls.Add(node);
            }
        }

        /// <summary>沿父级查找节点实际所属的文档画布。</summary>
        internal static FlowDocumentCanvas Find(Control control)
        {
            while (control != null)
            {
                FlowDocumentCanvas canvas = control as FlowDocumentCanvas;
                if (canvas != null) return canvas;
                control = control.Parent;
            }
            return null;
        }

        /// <summary>预览默认读取源码工程的 Debug 配置，部署运行不使用该路径。</summary>
        public string ResolvePreviewFile()
        {
            string path = string.IsNullOrWhiteSpace(PreviewFile) ? @"bin\x64\Debug\ModuleData\SettingData\FlowPrograms.xml" : PreviewFile;
            return Path.IsPathRooted(path) ? path : Path.GetFullPath(Path.Combine(SourceDirectory(), path));
        }

        /// <summary>编译器记录源码工程位置，避免依赖 VS 的工作目录。</summary>
        private static string SourceDirectory([CallerFilePath] string file = "")
        {
            return Directory.GetParent(Path.GetDirectoryName(file)).FullName;
        }

        /// <summary>预览与加载共用同一工程定义；读取失败只显示诊断，不写配置或连接设备。</summary>
        public void RefreshDesignPreview()
        {
            if (!IsDesignPreview || string.IsNullOrEmpty(DocumentKey) || string.IsNullOrEmpty(ModuleId)) return;
            RenderPreview(ResolvePreviewFile());
        }

        /// <summary>按指定文件绘制发布布局，支持离线验收与设计器刷新。</summary>
        public void RenderPreview(string path)
        {
            if (templates.Count > 0) throw new InvalidOperationException(FlowText.T("previewRuntime"));
            renderingPreview = false;
            foreach (Control child in Controls.Cast<Control>().ToArray())
            {
                Flow_Chart node = child as Flow_Chart;
                if (node != null) Flow_Module.FlowChart_List.Remove(node);
                child.Dispose();
            }
            try
            {
                FlowProgram program = FlowProgramFile.Load(path).Modules.SingleOrDefault(item => item.ModuleId == ModuleId);
                if (program == null) { ShowPreviewMessage(FlowText.T("previewNotSaved")); return; }
                FlowPageDefinition page = program.Pages.SingleOrDefault(item => item.Key == DocumentKey);
                if (page == null) { ShowPreviewMessage(FlowText.T("previewNotSaved")); return; }
                IEnumerable<FlowStep> steps = program.Steps.Where(item => item.PageId == page.Id);
                Dictionary<string, Flow_Chart> rendered = new Dictionary<string, Flow_Chart>();
                foreach (FlowStep step in steps)
                {
                    Flow_Chart node = new Flow_Chart { Name = step.ControlName, text = step.Caption, Location = new Point(step.X, step.Y), ProgramManaged = true };
                    if (step.Width > 0 && step.Height > 0) node.Size = new Size(step.Width, step.Height);
                    node.EnableProgramEditor(false); Controls.Add(node); rendered.Add(step.Id, node);
                    Flow_Module.FlowChart_List.Remove(node);
                }
                foreach (FlowStep step in steps)
                {
                    Flow_Chart node = rendered[step.Id];
                    node.Next = PreviewTarget(rendered, step.Next); node.Casse1 = PreviewTarget(rendered, step.Case1);
                    node.Casse2 = PreviewTarget(rendered, step.Case2); node.Casse3 = PreviewTarget(rendered, step.Case3);
                }
                renderingPreview = true; Invalidate(true);
            }
            catch (Exception error) { ShowPreviewMessage(string.Format(FlowText.T("error"), error.Message)); }
        }

        /// <summary>跨页面目标不在当前预览内制造另一份节点。</summary>
        private static Flow_Chart PreviewTarget(IDictionary<string, Flow_Chart> nodes, string id)
        {
            Flow_Chart node;
            return id != null && nodes.TryGetValue(id, out node) ? node : null;
        }

        /// <summary>预览连接在正常绘制周期重画，避免遮挡或离屏截图后丢失连线。</summary>
        protected override void OnPaint(PaintEventArgs args)
        {
            base.OnPaint(args);
            if (!renderingPreview) return;
            using (AdjustableArrowCap arrow = new AdjustableArrowCap(4, 4, true))
            using (Pen pen = new Pen(Color.Black, 2) { CustomEndCap = arrow })
                foreach (Flow_Chart node in Controls.OfType<Flow_Chart>())
                    foreach (Flow_Chart target in new[] { node.Next, node.Casse1, node.Casse2, node.Casse3 })
                    {
                        if (target == null || target.Parent != this) continue;
                        bool vertical = Math.Abs(target.Top - node.Top) >= Math.Abs(target.Left - node.Left);
                        bool forward = vertical ? target.Top >= node.Top : target.Left >= node.Left;
                        Point start = vertical ? new Point(node.Left + node.Width / 2, forward ? node.Bottom : node.Top)
                            : new Point(forward ? node.Right : node.Left, node.Top + node.Height / 2);
                        Point end = vertical ? new Point(target.Left + target.Width / 2, forward ? target.Top : target.Bottom)
                            : new Point(forward ? target.Left : target.Right, target.Top + target.Height / 2);
                        args.Graphics.DrawLine(pen, start, end);
                    }
        }

        /// <summary>用只读提示呈现未保存或加载失败的工程。</summary>
        private void ShowPreviewMessage(string message)
        {
            Controls.Add(new Label { AutoSize = true, Text = message, Location = new Point(12, 12), ForeColor = SystemColors.GrayText });
        }
    }

    public sealed class FlowDocumentCanvasDesigner : ControlDesigner
    {
        /// <summary>在 VS 提供显式刷新，不执行节点事件或写回 Designer 子控件。</summary>
        public override DesignerVerbCollection Verbs
        {
            get { return new DesignerVerbCollection { new DesignerVerb(FlowText.T("refreshDocument"), (sender, args) => ((FlowDocumentCanvas)Control).RefreshDesignPreview()) }; }
        }
    }
}
