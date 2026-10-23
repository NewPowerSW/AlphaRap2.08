using System;
using System.Collections.Generic;
using System.Linq;
using NPSDK;
using AlphaRap.Configuration;

namespace AlphaRap.FlowProgramming
{
    /// <summary>Setting 或 Recipe 常量表的读取快照，配置写入仍由原页面及主保存管理。</summary>
    internal sealed class FlowConstantTable : IFlowObject
    {
        private readonly Dictionary<string, object> values;

        /// <summary>只读取该作用域的已提交数据或编辑草稿，不执行保存。</summary>
        internal FlowConstantTable(ConstantStore store, string scope, bool includeDraft)
        {
            string prefix = "Constants/" + scope + "/";
            values = store.Snapshot(scope, includeDraft).ToDictionary(item => Uri.UnescapeDataString(item.Key.Substring(prefix.Length)), item => item.Value);
        }

        /// <summary>读取所选表中实际存在的键，保留原有布尔、整数、数值或文本类型。</summary>
        public IEnumerable<FlowMethod> GetFlowMethods(bool includeDraft = false)
        {
            FlowField key = new FlowField("Key", typeof(string), values.Keys.FirstOrDefault() ?? "");
            key.Options.AddRange(values.Select(item => new FlowOption { Value = item.Key, Label = item.Key + " (" + item.Value.GetType().Name + ")" }));
            FlowMethod read = FlowMethods.Sync("constant.read", "读取常量值", "Read constant value", "Leer valor constante", new[] { key },
                new[] { new FlowField("Value", typeof(object)) }, call => {
                    object value;
                    if (!values.TryGetValue(call.Get<string>("Key"), out value)) throw new InvalidOperationException(FlowText.T("constantKey"));
                    return FlowCall.Result("Value", value);
                }, false);
            read.ValidateInputs = input => { if (input.ContainsKey("Key") && !values.ContainsKey(Convert.ToString(input["Key"]))) throw new InvalidOperationException(FlowText.T("constantKey")); };
            yield return read;
        }
    }
}
