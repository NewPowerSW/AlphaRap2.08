using System.Collections.Generic;
using System.Linq;

namespace AlphaRap.FlowRuntime
{
    public static class DemoFlow
    {
        /// <summary>创建可验证 OK、NG、超时、停止和本地保存的仿真工艺。</summary>
        public static FlowDefinition Create(ActionCatalog catalog)
        {
            var flow = new FlowDefinition();
            flow.Devices.Add(new SimDevice { Id = "ReadyInput", Kind = DeviceKind.Input, State = true, DelayMs = 200 });
            flow.Devices.Add(new SimDevice { Id = "PickModule", Kind = DeviceKind.Module, DelayMs = 300 });
            flow.Devices.Add(new SimDevice { Id = "CameraA", Kind = DeviceKind.Vision, X = 0.12, Y = 0.2, Angle = 0.5, DelayMs = 300 });
            flow.Devices.Add(new SimDevice { Id = "CameraB", Kind = DeviceKind.Vision, X = 2.5, Y = 0.3, Angle = 0.7, DelayMs = 300 });
            flow.Devices.Add(new SimDevice { Id = "TestOutput", Kind = DeviceKind.Output, DelayMs = 100 });
            flow.Nodes.Add(Node(catalog, "Start", "trigger.manual", "", "Wait"));
            flow.Nodes.Add(Node(catalog, "Wait", "io.wait", "ReadyInput", "Move"));
            flow.Nodes.Add(Node(catalog, "Move", "module.move", "PickModule", "Vision"));
            flow.Nodes.Add(Node(catalog, "Vision", "vision.run", "CameraA", "Check"));
            var check = Node(catalog, "Check", "condition.range", "", "OK");
            check.FalseNext = "NG";
            check.Inputs[0].Source = "Vision.X";
            flow.Nodes.Add(check);
            flow.Nodes.Add(Node(catalog, "OK", "flow.note", "", "Save"));
            var ng = Node(catalog, "NG", "flow.note", "", "Save");
            ng.Inputs[0].Value = "NG";
            flow.Nodes.Add(ng);
            var save = Node(catalog, "Save", "data.save", "", "");
            string[] sources = { "Start.Barcode", "Vision.X", "Vision.Y", "Vision.Angle", "Check.Passed" };
            for (int i = 0; i < sources.Length; i++) save.Inputs[i].Source = sources[i];
            flow.Nodes.Add(save);
            return flow;
        }

        /// <summary>按动作契约创建节点和完整默认输入。</summary>
        public static FlowNode Node(ActionCatalog catalog, string id, string action, string target, string next)
        {
            return new FlowNode { Id = id, Title = id, ActionId = action, TargetId = target, Next = next,
                Inputs = catalog.Find(action).Inputs.Select(i => new InputBinding { Name = i.Name, Value = i.Default }).ToList() };
        }
    }
}
