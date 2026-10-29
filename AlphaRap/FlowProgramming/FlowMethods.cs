using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    /// <summary>对象方法的公共声明工具，不维护设备类型或动作目录。</summary>
    internal static class FlowMethods
    {
        private static readonly ConditionalWeakTable<FlowMethod, HashSet<string>> optionalInputs = new ConditionalWeakTable<FlowMethod, HashSet<string>>();

        /// <summary>为新增可选输入声明缺省兼容，旧流程缺少这些字段时使用方法初值。</summary>
        internal static void OptionalInputs(FlowMethod method, params string[] names)
        {
            optionalInputs.Add(method, new HashSet<string>(names, StringComparer.Ordinal));
        }

        /// <summary>只允许明确声明的可选输入缺省，必需端口和重复字段仍须校验。</summary>
        internal static bool IsOptional(FlowMethod method, FlowField field)
        {
            HashSet<string> names;
            return optionalInputs.TryGetValue(method, out names) && names.Contains(field.Name);
        }

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

        /// <summary>显示字段的业务标题，三语相同的用户名称保持原值且不登记为翻译。</summary>
        internal static string FieldTitle(string methodId, FlowField field, bool output = false)
        {
            return FieldText("field." + methodId + (output ? ".output." : ".input.") + field.Name, field.DisplayName, field.Name);
        }

        /// <summary>读取字段的三语操作说明，未声明说明时不生成额外提示。</summary>
        internal static string FieldHelp(string methodId, FlowField field)
        {
            return FieldText("fieldHelp." + methodId + "." + field.Name, field.Help, "");
        }

        /// <summary>沿用现场语言覆盖并为固定文案提供三语底稿。</summary>
        private static string FieldText(string key, string[] words, string fallback)
        {
            if (words == null || words.Length != 3) return fallback;
            if (words[0] == words[1] && words[1] == words[2]) return words[0];
            if (UiTheme.IsDesignTime) return words[0];
            return MiddleLayer.LangMsg("EquipmentFlow", key, words[0], words[1], words[2]);
        }
    }
}
