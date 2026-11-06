using System.Collections.Generic;

namespace NPSDK
{
    /// <summary>节点资源角色到实际对象身份及业务端口的绑定，不保存实例。</summary>
    public sealed class FlowResourceBinding
    {
        public string Slot;
        public string Target;
        public string PointTableSignature;
        public List<FlowResourceFieldBinding> Fields = new List<FlowResourceFieldBinding>();
        /// <summary>没有点位表确认信息时不写空字段。</summary>
        public bool ShouldSerializePointTableSignature() { return !string.IsNullOrEmpty(PointTableSignature); }
    }

    /// <summary>业务端口到真实 VPP 端口的稳定映射；Direction 为 Input 或 Output。</summary>
    public sealed class FlowResourceFieldBinding
    {
        public string Name;
        public string Field;
        public string Direction = "Output";
    }

    /// <summary>单次动作的独立输出与可选最终出口；空出口由原配置条件决定。</summary>
    public sealed class FlowActionOutcome
    {
        public IDictionary<string, object> Outputs = new Dictionary<string, object>();
        public string BranchCode;
    }
}
