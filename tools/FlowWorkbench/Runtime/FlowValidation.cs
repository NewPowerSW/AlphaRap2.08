using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AlphaRap.FlowRuntime
{
    public sealed class FlowIssue : Exception
    {
        public readonly string Key;
        public readonly object[] Arguments;

        /// <summary>携带稳定错误键与业务参数，供界面按当前语言解释。</summary>
        public FlowIssue(string key, params object[] arguments) : base(key + ": " + string.Join(", ", arguments)) { Key = key; Arguments = arguments; }
    }

    public static class FlowValidation
    {
        /// <summary>验证流程结构、动作、实例、参数、类型和所有路径上的数据可达性。</summary>
        public static void Validate(FlowDefinition flow, ActionCatalog catalog)
        {
            if (flow == null || flow.SchemaVersion != 1 || flow.Nodes == null || flow.Devices == null) throw new FlowIssue("InvalidFlow");
            if (flow.Nodes.Count == 0 || flow.Nodes.Count > 200) throw new FlowIssue("InvalidFlow");
            var nodes = new Dictionary<string, FlowNode>(StringComparer.Ordinal);
            foreach (var node in flow.Nodes)
            {
                if (node == null || !Identifier(node.Id) || nodes.ContainsKey(node.Id)) throw new FlowIssue("DuplicateId", node == null ? "" : node.Id);
                nodes.Add(node.Id, node);
            }
            var devices = new Dictionary<string, SimDevice>(StringComparer.Ordinal);
            foreach (var device in flow.Devices)
            {
                if (device == null || !Identifier(device.Id) || devices.ContainsKey(device.Id)) throw new FlowIssue("DuplicateId", device == null ? "" : device.Id);
                if (device.DelayMs < 0 || device.DelayMs > 600000 || !Finite(device.X) || !Finite(device.Y) || !Finite(device.Angle)) throw new FlowIssue("InvalidDevice", device.Id);
                devices.Add(device.Id, device);
            }
            if (!nodes.ContainsKey(flow.StartNode ?? "")) throw new FlowIssue("MissingNode", flow.StartNode);
            foreach (var node in flow.Nodes) ValidateNode(node, nodes, devices, catalog);
            var visiting = new HashSet<string>();
            var ordered = new List<string>();
            Visit(flow.StartNode, nodes, catalog, visiting, new HashSet<string>(), ordered);
            if (ordered.Count != flow.Nodes.Count) throw new FlowIssue("Unreachable");
            ordered.Reverse();
            ValidateBindings(flow, nodes, catalog, ordered);
        }

        /// <summary>验证可持久化身份，避免路径和输出引用歧义。</summary>
        public static bool Identifier(string id) { return id != null && Regex.IsMatch(id, "^[A-Za-z][A-Za-z0-9_]{0,63}$"); }

        /// <summary>拒绝非有限坐标。</summary>
        private static bool Finite(double value) { return !double.IsNaN(value) && !double.IsInfinity(value); }

        /// <summary>验证单节点的动作契约、目标、出口和输入名称。</summary>
        private static void ValidateNode(FlowNode node, Dictionary<string, FlowNode> nodes, Dictionary<string, SimDevice> devices, ActionCatalog catalog)
        {
            var spec = catalog.Find(node.ActionId);
            if (spec == null) throw new FlowIssue("UnknownAction", node.Id, node.ActionId);
            if (node.TimeoutMs < 1 || node.TimeoutMs > 600000) throw new FlowIssue("InvalidTimeout", node.Id);
            if (node.AlarmCode != "" && node.AlarmCode != "SIM_ACTION" && node.AlarmCode != "SIM_TIMEOUT") throw new FlowIssue("UnknownAlarm", node.Id);
            SimDevice device;
            if (spec.TargetKind.HasValue && (!devices.TryGetValue(node.TargetId ?? "", out device) || device.Kind != spec.TargetKind.Value)) throw new FlowIssue("InvalidTarget", node.Id, node.TargetId);
            if (!spec.TargetKind.HasValue && !string.IsNullOrEmpty(node.TargetId)) throw new FlowIssue("InvalidTarget", node.Id, node.TargetId);
            foreach (string next in Next(node, spec)) if (!nodes.ContainsKey(next)) throw new FlowIssue("MissingNode", next);
            if (spec.Branch && (string.IsNullOrEmpty(node.Next) || string.IsNullOrEmpty(node.FalseNext))) throw new FlowIssue("MissingBranch", node.Id);
            if (!spec.Branch && !string.IsNullOrEmpty(node.FalseNext)) throw new FlowIssue("MissingBranch", node.Id);
            if (node.Inputs == null || node.Inputs.Any(i => i == null || i.Name == null || !Regex.IsMatch(i.Name, "^[\\p{L}_][\\p{L}\\p{N}_]{0,63}$")) || node.Inputs.Select(i => i.Name).Distinct(StringComparer.Ordinal).Count() != node.Inputs.Count) throw new FlowIssue("InvalidInputs", node.Id);
            foreach (var input in spec.Inputs) if (!node.Inputs.Any(i => i.Name == input.Name)) throw new FlowIssue("MissingInput", node.Id, input.Name);
            foreach (var input in node.Inputs)
            {
                if (!spec.ExtraInputs && !spec.Inputs.Any(i => i.Name == input.Name)) throw new FlowIssue("MissingInput", node.Id, input.Name);
                if (new[] { "RecordId", "RunId", "StartedAt", "FlowVersion" }.Contains(input.Name) && spec.ExtraInputs) throw new FlowIssue("InvalidInputs", node.Id);
            }
        }

        /// <summary>返回真实控制边，普通动作没有假出口。</summary>
        public static IEnumerable<string> Next(FlowNode node, ActionSpec spec)
        {
            if (!string.IsNullOrEmpty(node.Next)) yield return node.Next;
            if (spec.Branch && !string.IsNullOrEmpty(node.FalseNext)) yield return node.FalseNext;
        }

        /// <summary>检查循环并生成拓扑顺序；测试版只接受无环流程。</summary>
        private static void Visit(string id, Dictionary<string, FlowNode> nodes, ActionCatalog catalog, HashSet<string> visiting, HashSet<string> visited, List<string> ordered)
        {
            if (visiting.Contains(id)) throw new FlowIssue("Cycle");
            if (!visited.Add(id)) return;
            visiting.Add(id);
            foreach (string next in Next(nodes[id], catalog.Find(nodes[id].ActionId))) Visit(next, nodes, catalog, visiting, visited, ordered);
            visiting.Remove(id);
            ordered.Add(id);
        }

        /// <summary>使用前驱交集验证绑定，禁止读取未来节点或仅另一分支存在的输出。</summary>
        private static void ValidateBindings(FlowDefinition flow, Dictionary<string, FlowNode> nodes, ActionCatalog catalog, List<string> ordered)
        {
            var before = new Dictionary<string, HashSet<string>>();
            var saved = new Dictionary<string, bool>();
            foreach (string id in ordered)
            {
                var predecessors = flow.Nodes.Where(n => Next(n, catalog.Find(n.ActionId)).Contains(id)).ToList();
                var available = new HashSet<string>(StringComparer.Ordinal);
                if (predecessors.Count > 0) available.UnionWith(before[predecessors[0].Id].Concat(new[] { predecessors[0].Id }));
                foreach (var previous in predecessors.Skip(1)) available.IntersectWith(before[previous.Id].Concat(new[] { previous.Id }));
                before.Add(id, available);
                saved[id] = nodes[id].ActionId == "data.save" || (predecessors.Count > 0 && predecessors.All(n => saved[n.Id]));
                if (flow.RequireLocalSave && !Next(nodes[id], catalog.Find(nodes[id].ActionId)).Any() && !saved[id]) throw new FlowIssue("MissingSave", id);
                foreach (var binding in nodes[id].Inputs) ValidateBinding(nodes[id], binding, catalog, nodes, available);
            }
        }

        /// <summary>校验常量格式或输出引用的来源与类型。</summary>
        private static void ValidateBinding(FlowNode node, InputBinding binding, ActionCatalog catalog, Dictionary<string, FlowNode> nodes, HashSet<string> available)
        {
            var input = catalog.Find(node.ActionId).Inputs.FirstOrDefault(i => i.Name == binding.Name);
            if (string.IsNullOrEmpty(binding.Source))
            {
                try { FlowFiles.Parse(binding.Value, input == null ? ValueKind.Text : input.Kind); }
                catch (Exception) { throw new FlowIssue("InvalidValue", node.Id, binding.Name); }
                return;
            }
            string[] parts = binding.Source.Split('.');
            if (parts.Length != 2 || !available.Contains(parts[0])) throw new FlowIssue("InvalidBinding", node.Id, binding.Source);
            var output = catalog.Find(nodes[parts[0]].ActionId).Outputs.FirstOrDefault(o => o.Name == parts[1]);
            if (output == null || (input != null && output.Kind != input.Kind)) throw new FlowIssue("InvalidBinding", node.Id, binding.Source);
        }
    }
}
