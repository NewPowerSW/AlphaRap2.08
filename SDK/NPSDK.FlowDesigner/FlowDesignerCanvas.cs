using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace NPSDK.Design
{
    public sealed class FlowDesignerCanvas : Panel
    {
        private Point dragStart;
        private Point nodeStart;
        private Flow_Chart dragging;
        public readonly Dictionary<string, Flow_Chart> Nodes = new Dictionary<string, Flow_Chart>();
        public bool Editing = true;
        public event Action<FlowDesignNode> EditRequested;
        public event Action<FlowDesignNode> Selected;
        public event Action Changed;
        public event Action<string, Point> NodeDropped;
        private string lastClickId;
        private int lastClickTime;
        /// <summary>提供承载真实 SDK 节点的可滚动画布。</summary>
        public FlowDesignerCanvas()
        {
            DoubleBuffered = true;
            AutoScroll = true;
            AllowDrop = true;
            BackColor = SystemColors.Window;
            DragEnter += (s, e) =>
            {
                if (Editing && e.Data.GetDataPresent(typeof(string)))
                    e.Effect = DragDropEffects.Copy;
            };
            DragDrop += DropMethod;
        }

        /// <summary>将文档转换为真实 Flow_Chart 及其 Next/Case 引用。</summary>
        public void LoadDocument(FlowDesignDocument document)
        {
            foreach (var node in Nodes.Values.ToArray())
            {
                Controls.Remove(node);
                node.Dispose();
            }

            Nodes.Clear();
            foreach (var definition in document.Nodes)
                AddNode(definition);
            foreach (var definition in document.Nodes)
            {
                var node = Nodes[definition.Id];
                node.Next = Find(definition.Next);
                node.Casse1 = Find(definition.Case1);
                node.Casse2 = Find(definition.Case2);
                node.Casse3 = Find(definition.Case3);
            }

            Invalidate();
        }

        /// <summary>创建 SDK 节点并把鼠标事件连接到同一拖拽与属性编辑入口。</summary>
        private void AddNode(FlowDesignNode definition)
        {
            var node = new Flow_Chart(false)
            {
                Name = definition.Id,
                text = definition.Text,
                Tag = definition,
                Size = new Size(210, 55),
                Location = new Point(definition.X, definition.Y),
                ExternalConnectionDrawing = true
            };
            Nodes.Add(definition.Id, node);
            Controls.Add(node);
            Hook(node, node);
        }

        /// <summary>同时处理节点外框和内部按钮的事件，避免按钮吞掉拖拽及双击。</summary>
        private void Hook(Control part, Flow_Chart node)
        {
            part.MouseDown += (sender, e) => BeginMove(node, (Control)sender, e);
            part.MouseMove += (sender, e) => MoveNode(node, (Control)sender, e);
            part.MouseUp += EndMove;
            foreach (Control child in part.Controls)
                Hook(child, node);
        }

        /// <summary>记录画布坐标并捕获鼠标，运行时只允许选择。</summary>
        private void BeginMove(Flow_Chart node, Control part, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;
            if (Selected != null)
                Selected((FlowDesignNode)node.Tag);
            if (!Editing)
                return;
            dragging = node;
            dragStart = PointToClient(part.PointToScreen(e.Location));
            nodeStart = node.Location;
            part.Capture = true;
        }

        /// <summary>移动 SDK 节点，并同步持久化坐标。</summary>
        private void MoveNode(Flow_Chart node, Control part, MouseEventArgs e)
        {
            if (!Editing || dragging != node || e.Button != MouseButtons.Left)
                return;
            var point = PointToClient(part.PointToScreen(e.Location));
            node.Location = new Point(Math.Max(AutoScrollPosition.X, nodeStart.X + point.X - dragStart.X), Math.Max(AutoScrollPosition.Y, nodeStart.Y + point.Y - dragStart.Y));
            var definition = (FlowDesignNode)node.Tag;
            definition.X = node.Left - AutoScrollPosition.X;
            definition.Y = node.Top - AutoScrollPosition.Y;
            Invalidate();
        }

        /// <summary>结束捕获，并通知文档已修改。</summary>
        private void EndMove(object sender, MouseEventArgs e)
        {
            if (dragging == null)
                return;
            var node = dragging;
            bool click = Math.Abs(node.Left - nodeStart.X) < 4 && Math.Abs(node.Top - nodeStart.Y) < 4;
            ((Control)sender).Capture = false;
            dragging = null;
            if (click && lastClickId == node.Name && unchecked(Environment.TickCount - lastClickTime) < SystemInformation.DoubleClickTime)
            {
                lastClickId = null;
                RequestEdit(node);
                return;
            }

            lastClickId = click ? node.Name : null;
            lastClickTime = Environment.TickCount;
            if (Changed != null)
                Changed();
        }

        /// <summary>将工具箱方法拖入画布，坐标按逻辑滚动位置持久化。</summary>
        private void DropMethod(object sender, DragEventArgs e)
        {
            if (!Editing || NodeDropped == null)
                return;
            string method = e.Data.GetData(typeof(string)) as string;
            if (string.IsNullOrEmpty(method))
                return;
            Point point = PointToClient(new Point(e.X, e.Y));
            NodeDropped(method, new Point(point.X - AutoScrollPosition.X, point.Y - AutoScrollPosition.Y));
        }

        /// <summary>双击只请求编辑，不执行节点。</summary>
        public void RequestEdit(Flow_Chart node)
        {
            if (Editing && EditRequested != null)
                EditRequested((FlowDesignNode)node.Tag);
        }

        /// <summary>解析现有 SDK 控件引用。</summary>
        private Flow_Chart Find(string id)
        {
            Flow_Chart result;
            return !string.IsNullOrEmpty(id) && Nodes.TryGetValue(id, out result) ? result : null;
        }

        /// <summary>在父画布稳定重绘 SDK 连线，不依赖临时 CreateGraphics。</summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            foreach (var node in Nodes.Values)
            {
                DrawLink(e.Graphics, node, node.Next, "Next");
                DrawLink(e.Graphics, node, node.Casse1, "1");
                DrawLink(e.Graphics, node, node.Casse2, "2");
                DrawLink(e.Graphics, node, node.Casse3, "3");
            }
        }

        /// <summary>绘制一条带箭头的实际节点跳转引用。</summary>
        private void DrawLink(Graphics graphics, Flow_Chart from, Flow_Chart to, string label)
        {
            if (to == null || to.IsDisposed)
                return;
            using (var pen = new Pen(SystemColors.ControlDarkDark, 1.5f))
            using (var arrow = new AdjustableArrowCap(4, 5))
            {
                pen.CustomEndCap = arrow;
                var a = new Point(from.Left + from.Width / 2, from.Bottom);
                var b = new Point(to.Left + to.Width / 2, to.Top);
                graphics.DrawLine(pen, a, b);
                graphics.DrawString(label, Font, SystemBrushes.WindowText, (a.X + b.X) / 2, (a.Y + b.Y) / 2);
            }
        }
    }
}
