using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NPSDK.Design
{
    public sealed class FlowActionBinding
    {
        public string MethodName;
        public string TargetId;
        public FlowDesignParameter[] Parameters = new FlowDesignParameter[0];
        public string[] Outputs = new string[0];
        public Action<FlowInvocation> Start;
        public Func<FlowInvocation, FlowActionSample> Poll;
        public Action<FlowInvocation> Cancel;
    }

    public sealed class FlowInvocation
    {
        public string RunId;
        public string OutputDirectory;
        public FlowDesignNode Node;
        public Dictionary<string, object> Inputs;
        public object State;
        public Task Pending;
        public volatile bool Cancelled;
    }

    public sealed class FlowActionSample
    {
        public bool Waiting;
        public readonly Dictionary<string, object> Values = new Dictionary<string, object>(StringComparer.Ordinal);
        /// <summary>创建一次采样结果，供当前节点判据与后继数据绑定使用。</summary>
        public static FlowActionSample Of(params object[] pairs)
        {
            var sample = new FlowActionSample();
            for (int i = 0; i < pairs.Length; i += 2)
                sample.Values.Add((string)pairs[i], pairs[i + 1]);
            return sample;
        }
    }

    public sealed class FlowActionRegistry
    {
        private readonly List<FlowActionBinding> bindings = new List<FlowActionBinding>();
        public readonly List<string> AlarmCodes = new List<string>();
        public IEnumerable<FlowActionBinding> Bindings
        {
            get
            {
                return bindings.ToArray();
            }
        }

        /// <summary>显式登记方法与实例，避免配置任意反射调用。</summary>
        public void Add(FlowActionBinding binding)
        {
            if (binding == null || string.IsNullOrEmpty(binding.MethodName) || binding.Poll == null || Find(binding.MethodName, binding.TargetId) != null)
                throw new ArgumentException("binding");
            bindings.Add(binding);
        }

        /// <summary>按方法和实例身份解析真正的执行对象。</summary>
        public FlowActionBinding Find(string method, string target)
        {
            return bindings.FirstOrDefault(b => b.MethodName == method && b.TargetId == target);
        }

        /// <summary>验证身份、目标、参数、判据与所有节点跳转。</summary>
        public void Validate(FlowDesignDocument document)
        {
            ValidateStructure(document);
            var ids = new HashSet<string>(document.Nodes.Select(n => n.Id), StringComparer.Ordinal);
            if (!ids.Contains(document.Entry ?? ""))
                throw new FlowDesignIssue("MissingEntry");
            foreach (var node in document.Nodes)
                ValidateNode(document, node, ids);
        }

        /// <summary>验证可安全显示和保存的结构，允许离线修复尚未绑定的设备。</summary>
        public void ValidateStructure(FlowDesignDocument document)
        {
            if (document == null || document.Version != 1 || document.Nodes == null || document.Nodes.Count > 300)
                throw new FlowDesignIssue("InvalidDocument");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var node in document.Nodes)
            {
                if (node == null || !System.Text.RegularExpressions.Regex.IsMatch(node.Id ?? "", "^[A-Za-z][A-Za-z0-9_]{0,63}$") || !ids.Add(node.Id))
                    throw new FlowDesignIssue("InvalidNode", node == null ? "" : node.Id);
                if (node.Parameters == null || node.Parameters.Any(parameter => parameter == null) || node.X < 0 || node.Y < 0 || node.X > 100000 || node.Y > 100000)
                    throw new FlowDesignIssue("InvalidDocument");
            }

            if (!string.IsNullOrEmpty(document.Entry) && !ids.Contains(document.Entry))
                throw new FlowDesignIssue("MissingEntry");
        }

        /// <summary>校验单节点配置；超时为零表示持续等待。</summary>
        internal void ValidateNode(FlowDesignDocument document, FlowDesignNode node, HashSet<string> ids)
        {
            var binding = Find(node.MethodName, node.Target);
            if (binding == null)
                throw new FlowDesignIssue("UnknownBinding", node.Id, node.MethodName, node.Target);
            if (node.Delay < 0 || node.Delay > 600000 || node.Timeout < 0 || node.Timeout > 600000)
                throw new FlowDesignIssue("InvalidTime", node.Id);
            foreach (string next in new[]
            {
                node.Next,
                node.Case1,
                node.Case2,
                node.Case3
            }

            )
                if (!string.IsNullOrEmpty(next) && !ids.Contains(next))
                    throw new FlowDesignIssue("UnknownNext", node.Id, next);
            if (node.WhenFalse == FlowUnmetAction.Case1 && string.IsNullOrEmpty(node.Case1) || node.WhenFalse == FlowUnmetAction.Case2 && string.IsNullOrEmpty(node.Case2) || node.WhenFalse == FlowUnmetAction.Case3 && string.IsNullOrEmpty(node.Case3))
                throw new FlowDesignIssue("UnknownNext", node.Id, node.WhenFalse);
            if (!string.IsNullOrEmpty(node.AlarmCode) && !AlarmCodes.Contains(node.AlarmCode))
                throw new FlowDesignIssue("UnknownAlarm", node.AlarmCode);
            if (node.Comparison != FlowComparison.Always && string.IsNullOrEmpty(node.ConditionSource))
                throw new FlowDesignIssue("InvalidCondition", node.Id);
            if (node.Parameters == null || node.Parameters.Any(p => p == null || string.IsNullOrEmpty(p.Name)) || node.Parameters.Select(p => p.Name).Distinct().Count() != node.Parameters.Count)
                throw new FlowDesignIssue("InvalidParameter", node.Id);
            foreach (var required in binding.Parameters)
                if (!node.Parameters.Any(p => p.Name == required.Name && p.Type == required.Type))
                    throw new FlowDesignIssue("InvalidParameter", node.Id);
            if (node.MethodName != "Data.Save" && node.Parameters.Count != binding.Parameters.Length)
                throw new FlowDesignIssue("InvalidParameter", node.Id);
            if (node.MethodName == "Data.Save" && node.Parameters.Any(p => new[] { "RunId", "UtcTime", "Node" }.Contains(p.Name)))
                throw new FlowDesignIssue("InvalidParameter", node.Id);
            foreach (var parameter in node.Parameters)
            {
                if (!new[]
                {
                    "String",
                    "Boolean",
                    "Int32",
                    "Double"
                }.Contains(parameter.Type))
                    throw new FlowDesignIssue("InvalidParameter", node.Id);
                if (string.IsNullOrEmpty(parameter.Source))
                {
                    try
                    {
                        FlowDesignFiles.Parse(parameter.Type, parameter.Value);
                    }
                    catch (Exception)
                    {
                        throw new FlowDesignIssue("InvalidParameter", node.Id);
                    }

                    continue;
                }

                ValidateSource(document, node, binding, parameter.Source, false);
            }

            if (node.Comparison != FlowComparison.Always)
                ValidateSource(document, node, binding, node.ConditionSource, true);
            if (!Enum.IsDefined(typeof(FlowComparison), node.Comparison) || !Enum.IsDefined(typeof(FlowUnmetAction), node.WhenFalse))
                throw new FlowDesignIssue("InvalidCondition", node.Id);
            if (node.Comparison == FlowComparison.Greater || node.Comparison == FlowComparison.Less || node.Comparison == FlowComparison.InRange)
                ValidateNumericCondition(node);
            if (node.MethodName == "Alarm.Show")
            {
                var code = node.Parameters.FirstOrDefault(p => p.Name == "Code");
                if (code != null && string.IsNullOrEmpty(code.Source) && !AlarmCodes.Contains(code.Value))
                    throw new FlowDesignIssue("UnknownAlarm", code.Value);
            }
        }

        /// <summary>在运行设备前拒绝无效数值及倒置区间。</summary>
        private static void ValidateNumericCondition(FlowDesignNode node)
        {
            try
            {
                if (node.Comparison != FlowComparison.InRange)
                {
                    FlowDesignFiles.Parse("Double", node.Expected);
                    return;
                }

                var range = node.Expected.Split(',');
                if (range.Length != 2 || (double)FlowDesignFiles.Parse("Double", range[0]) > (double)FlowDesignFiles.Parse("Double", range[1]))
                    throw new FormatException();
            }
            catch (Exception)
            {
                throw new FlowDesignIssue("InvalidCondition", node.Id);
            }
        }

        /// <summary>校验数据来源的节点和输出键，运行时另检查本轮是否已经产生。</summary>
        private void ValidateSource(FlowDesignDocument document, FlowDesignNode node, FlowActionBinding binding, string source, bool currentAllowed)
        {
            if (currentAllowed && binding.Outputs.Contains(source))
                return;
            string[] parts = source.Split('.');
            var previous = parts.Length == 2 ? document.Nodes.FirstOrDefault(n => n.Id == parts[0]) : null;
            if (previous == null || previous.Id == node.Id && !currentAllowed)
                throw new FlowDesignIssue("InvalidSource", node.Id, source);
            var previousBinding = Find(previous.MethodName, previous.Target);
            if (previousBinding == null || !previousBinding.Outputs.Contains(parts[1]))
                throw new FlowDesignIssue("InvalidSource", node.Id, source);
        }
    }
}
