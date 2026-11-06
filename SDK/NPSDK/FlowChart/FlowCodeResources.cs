using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NPSDK
{
    /// <summary>类型化代理共用的简单结果转换，不依赖宿主控件或视觉类库。</summary>
    internal static class FlowCodeResourceValues
    {
        /// <summary>读取指定输出并沿用流程字段的有限数值及精确整数校验。</summary>
        internal static T Read<T>(IDictionary<string, object> values, string name)
        {
            object value;
            if (!values.TryGetValue(name, out value)) throw new InvalidOperationException("Resource output is missing: " + name);
            return (T)new FlowField(name, typeof(T)).ConvertValue(value);
        }
    }

    /// <summary>已绑定运动轴的受控访问，点位只使用该轴自己的绑定表。</summary>
    public sealed class FlowCodeAxis
    {
        private readonly FlowCodeContext context;
        private readonly string slot;
        /// <summary>保存轴角色，不持有电机控件或点位表实例。</summary>
        internal FlowCodeAxis(FlowCodeContext context, string slot) { this.context = context; this.slot = slot; }
        /// <summary>移动到绑定表的零基索引并等待到位，返回实际位置。</summary>
        public FlowCodeCall<double> MoveToAsync(int index, double offset = 0, double tolerance = 0.2)
        {
            return context.Invoke(slot, "point.move", new Dictionary<string, object> { { "Index", index }, { "Offset", offset }, { "Tolerance", tolerance } },
                result => FlowCodeResourceValues.Read<double>(result, "Position"));
        }
        /// <summary>回零并等待完成，返回位置。</summary>
        public FlowCodeCall<double> HomeAsync() { return context.Invoke(slot, "axis.home", null, result => FlowCodeResourceValues.Read<double>(result, "Position")); }
        /// <summary>读取编码器位置。</summary>
        public FlowCodeCall<double> GetPositionAsync() { return context.Invoke(slot, "axis.position", null, result => FlowCodeResourceValues.Read<double>(result, "Position")); }
        /// <summary>读取指令位置。</summary>
        public FlowCodeCall<double> GetCommandAsync() { return context.Invoke(slot, "axis.command", null, result => FlowCodeResourceValues.Read<double>(result, "Position")); }
        /// <summary>通过绑定轴的停止方法停止，返回停止位置。</summary>
        public FlowCodeCall<double> StopAsync() { return context.Invoke(slot, "axis.stop", null, result => FlowCodeResourceValues.Read<double>(result, "Position")); }
        /// <summary>复位绑定轴报警，成功返回 true。</summary>
        public FlowCodeCall<bool> ResetAsync() { return context.Invoke(slot, "axis.reset", null, result => true); }
    }

    /// <summary>已绑定输入的读取及稳定等待。</summary>
    public sealed class FlowCodeInput
    {
        private readonly FlowCodeContext context;
        private readonly string slot;
        /// <summary>保存输入角色，不暴露底层 IO 控件。</summary>
        internal FlowCodeInput(FlowCodeContext context, string slot) { this.context = context; this.slot = slot; }
        /// <summary>读取当前有效输入状态。</summary>
        public FlowCodeCall<bool> ReadAsync() { return context.Invoke(slot, "io.read", null, result => FlowCodeResourceValues.Read<bool>(result, "Value")); }
        /// <summary>等待输入达到指定状态并稳定指定毫秒数。</summary>
        public FlowCodeCall<bool> WaitAsync(bool value, int stableMs = 50)
        {
            return context.Invoke(slot, "io.wait", new Dictionary<string, object> { { "Value", value }, { "StableMs", stableMs } },
                result => FlowCodeResourceValues.Read<bool>(result, "Value"));
        }
    }

    /// <summary>已绑定输出的受控读写。</summary>
    public sealed class FlowCodeOutput
    {
        private readonly FlowCodeContext context;
        private readonly string slot;
        /// <summary>保存输出角色，不重新创建 IO 实例。</summary>
        internal FlowCodeOutput(FlowCodeContext context, string slot) { this.context = context; this.slot = slot; }
        /// <summary>设置输出有效状态并返回实际状态。</summary>
        public FlowCodeCall<bool> SetAsync(bool value)
        { return context.Invoke(slot, "io.set", new Dictionary<string, object> { { "Value", value } }, result => FlowCodeResourceValues.Read<bool>(result, "Value")); }
        /// <summary>读取输出当前有效状态。</summary>
        public FlowCodeCall<bool> ReadAsync() { return context.Invoke(slot, "io.output.read", null, result => FlowCodeResourceValues.Read<bool>(result, "Value")); }
    }

    /// <summary>一次 VPP 运行的独立结果，业务端口按节点映射读取。</summary>
    public sealed class FlowCodeVisionResult
    {
        public long RunId { get; private set; }
        public bool Accepted { get; private set; }
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Angle { get; private set; }
        public IDictionary<string, object> Fields { get; private set; }
        /// <summary>复制一次运行的结果，禁止后续拍照覆盖此快照。</summary>
        internal FlowCodeVisionResult(IDictionary<string, object> result, IDictionary<string, object> mapped)
        {
            RunId = FlowCodeResourceValues.Read<long>(result, "RunId"); Accepted = FlowCodeResourceValues.Read<bool>(result, "Accepted");
            X = FlowCodeResourceValues.Read<double>(result, "X"); Y = FlowCodeResourceValues.Read<double>(result, "Y"); Angle = FlowCodeResourceValues.Read<double>(result, "Angle");
            Fields = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>(mapped, StringComparer.Ordinal));
        }
        /// <summary>读取已映射的业务字段并检查实际值类型。</summary>
        public T Get<T>(string name) { return FlowCodeResourceValues.Read<T>(Fields, name); }
    }

    /// <summary>VPP 补偿表中的一个值及其上下限，不自动触发轴运动。</summary>
    public sealed class FlowCodeCompensation
    {
        public double Compensation { get; private set; }
        public double Min { get; private set; }
        public double Max { get; private set; }
        /// <summary>读取本次补偿查询的值快照。</summary>
        internal FlowCodeCompensation(IDictionary<string, object> result)
        {
            Compensation = FlowCodeResourceValues.Read<double>(result, "Compensation");
            Min = FlowCodeResourceValues.Read<double>(result, "Min"); Max = FlowCodeResourceValues.Read<double>(result, "Max");
        }
    }

    /// <summary>已绑定 VPP 的拍照、结果、照片及补偿访问，不引用 VisionPro。</summary>
    public sealed class FlowCodeVision
    {
        private readonly FlowCodeContext context;
        private readonly string slot;
        /// <summary>保存视觉角色，运行时使用本次会话解析的站实例。</summary>
        internal FlowCodeVision(FlowCodeContext context, string slot) { this.context = context; this.slot = slot; }
        /// <summary>按业务字段设置映射输入并拍照运行，返回本次独立结果。</summary>
        public FlowCodeCall<FlowCodeVisionResult> RunAsync(IDictionary<string, object> inputs = null)
        {
            Dictionary<string, object> values = new Dictionary<string, object>(StringComparer.Ordinal);
            if (inputs != null)
                foreach (KeyValuePair<string, object> input in inputs) values.Add(context.MappedField(slot, input.Key, "Input"), input.Value);
            return context.Invoke(slot, "vision.run", values, result => new FlowCodeVisionResult(result, context.MapVisionOutputs(slot, result)));
        }
        /// <summary>按明确运行编号读取映射输出，不读取未经关联的最新结果。</summary>
        public FlowCodeCall<T> ReadOutputAsync<T>(long runId, string businessField)
        {
            string source = context.MappedField(slot, businessField, "Output");
            if (source.StartsWith("TB:", StringComparison.Ordinal)) source = source.Substring(3);
            return context.Invoke(slot, "vision.output", new Dictionary<string, object> { { "RunId", runId }, { "Output", source } },
                result => FlowCodeResourceValues.Read<T>(result, "Value"));
        }
        /// <summary>按本站保存配置保存指定运行照片，返回实际文件路径。</summary>
        public FlowCodeCall<string> SaveImageAsync(long runId)
        {
            return context.Invoke(slot, "vision.image.save", new Dictionary<string, object> { { "RunId", runId } },
                result => FlowCodeResourceValues.Read<string>(result, "Path"));
        }
        /// <summary>读取本站补偿表的业务项目，不自动应用至运动控制。</summary>
        public FlowCodeCall<FlowCodeCompensation> ReadCompensationAsync(string item)
        { return context.Invoke(slot, "vision.compensation", new Dictionary<string, object> { { "Item", item } }, result => new FlowCodeCompensation(result)); }
    }
}
