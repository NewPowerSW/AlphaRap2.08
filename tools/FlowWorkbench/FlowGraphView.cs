using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using AlphaRap.FlowRuntime;

namespace AlphaRap.FlowWorkbench
{
    public sealed class FlowGraphView : UserControl
    {
        public FlowDefinition Flow;
        public string SelectedId = "";
        public readonly Dictionary<string, string> States = new Dictionary<string, string>();
        public event Action<string> NodeSelected;

        /// <summary>创建支持滚动的流程预览，不创建执行线程。</summary>
        public FlowGraphView() { DoubleBuffered = true; AutoScroll = true; BackColor = SystemColors.Window; }

        /// <summary>绘制固定模板节点及真实控制边，状态仅由运行通知更新。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (Flow == null && DesignMode) Flow = DemoFlow.Create(ActionCatalog.Simulation());
            if (Flow == null) return;
            e.Graphics.TranslateTransform(AutoScrollPosition.X, AutoScrollPosition.Y);
            var boxes = Boxes();
            using (var pen = new Pen(SystemColors.GrayText, 1.5f))
            using (var arrow = new AdjustableArrowCap(4, 5))
            {
                pen.CustomEndCap = arrow;
                foreach (var node in Flow.Nodes)
                {
                    DrawEdge(e.Graphics, pen, boxes, node.Id, node.Next, string.IsNullOrEmpty(node.FalseNext) ? "" : "T");
                    DrawEdge(e.Graphics, pen, boxes, node.Id, node.FalseNext, "F");
                }
            }
            foreach (var node in Flow.Nodes) DrawNode(e.Graphics, node, boxes[node.Id]);
        }

        /// <summary>生成可重复的节点位置，画布布局不参与运行。</summary>
        private Dictionary<string, Rectangle> Boxes()
        {
            var boxes = new Dictionary<string, Rectangle>();
            for (int i = 0; i < Flow.Nodes.Count; i++)
                if (!boxes.ContainsKey(Flow.Nodes[i].Id)) boxes.Add(Flow.Nodes[i].Id, new Rectangle(30 + i % 3 * 260, 30 + i / 3 * 150, 220, 80));
            AutoScrollMinSize = new Size(820, 70 + (Flow.Nodes.Count + 2) / 3 * 150);
            return boxes;
        }

        /// <summary>绘制控制连线与真假端口。</summary>
        private void DrawEdge(Graphics graphics, Pen pen, Dictionary<string, Rectangle> boxes, string from, string to, string port)
        {
            if (string.IsNullOrEmpty(to) || !boxes.ContainsKey(from) || !boxes.ContainsKey(to)) return;
            var a = boxes[from]; var b = boxes[to];
            var start = new Point(a.Right, a.Top + (port == "F" ? 60 : 25));
            var end = new Point(b.Left, b.Top + 40);
            graphics.DrawLine(pen, start, end);
            if (!string.IsNullOrEmpty(port)) graphics.DrawString(port, Font, SystemBrushes.WindowText, start);
        }

        /// <summary>绘制节点身份、业务标题、动作和当前状态。</summary>
        private void DrawNode(Graphics graphics, FlowNode node, Rectangle box)
        {
            using (var brush = new SolidBrush(node.Id == SelectedId ? SystemColors.Info : SystemColors.ControlLight)) graphics.FillRectangle(brush, box);
            graphics.DrawRectangle(SystemPens.ControlDark, box);
            string state;
            States.TryGetValue(node.Id, out state);
            TextRenderer.DrawText(graphics, node.Id + "  " + node.Title + "\n" + node.ActionId + "\n" + (state ?? ""), Font, Rectangle.Inflate(box, -7, -5), SystemColors.WindowText, TextFormatFlags.WordBreak);
        }

        /// <summary>选择预览节点并通知参数编辑器。</summary>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (Flow == null) return;
            var point = new Point(e.X - AutoScrollPosition.X, e.Y - AutoScrollPosition.Y);
            foreach (var pair in Boxes())
                if (pair.Value.Contains(point)) { if (NodeSelected != null) NodeSelected(pair.Key); return; }
        }
    }
}
