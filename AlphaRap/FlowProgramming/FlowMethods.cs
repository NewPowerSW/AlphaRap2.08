using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    /// <summary>对象方法的公共声明工具，不维护设备类型或动作目录。</summary>
    internal static class FlowMethods
    {
        /// <summary>声明同步方法，实际调用在后台执行并保留资源租约至返回。</summary>
        internal static FlowMethod Sync(string id, string zh, string en, string es, FlowField[] inputs, FlowField[] outputs,
            Func<FlowCall, IDictionary<string, object>> invoke, bool hardware = true)
        {
            FlowMethod method = new FlowMethod(id, zh, en, es) { UsesHardware = hardware };
            method.Inputs.AddRange(inputs); method.Outputs.AddRange(outputs);
            method.Invoke = call => Task.Run(() => { call.Token.ThrowIfCancellationRequested(); return invoke(call); });
            if (hardware) method.Simulate = call => Task.FromResult(Simulated(method));
            return method;
        }

        /// <summary>模拟结果保留真实字段类型并明确标记，不能替代实际硬件验收。</summary>
        private static IDictionary<string, object> Simulated(FlowMethod method)
        {
            Dictionary<string, object> result = method.Outputs.ToDictionary(field => field.Name,
                field => field.Type == typeof(string) ? (object)("SIM-" + method.Id) : field.Type.IsArray ? Array.CreateInstance(field.Type.GetElementType(), 0) : field.Type.IsValueType ? Activator.CreateInstance(field.Type) : null);
            result.Add("Simulated", true);
            return result;
        }

        /// <summary>通过框架语言入口显示方法标题，业务身份不参与翻译。</summary>
        internal static string Title(FlowMethod method)
        {
            string key = "action." + method.Id;
            FlowText.Defaults[key] = method.Title;
            return FlowText.T(key);
        }
    }
}
