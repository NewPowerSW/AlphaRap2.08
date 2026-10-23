using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;
using NPSDK;
using AlphaRap.Configuration;

namespace AlphaRap.FlowProgramming
{
    /// <summary>Setting 或 Recipe 常量表的读取和设置能力，文件写入统一由主保存管理。</summary>
    internal sealed class FlowConstantTable : IFlowObject
    {
        private readonly Dictionary<string, object> values;
        private readonly string scope;
        private readonly ConstantStore store;
        private readonly string recipePath;

        /// <summary>只读取该作用域的已提交数据或编辑草稿，不执行保存。</summary>
        internal FlowConstantTable(ConstantStore store, string scope, bool includeDraft)
        {
            this.scope = scope; this.store=store; recipePath=SysPara.FilePath;
            string prefix = "Constants/" + scope + "/";
            values = store.Snapshot(scope, includeDraft).ToDictionary(item => Uri.UnescapeDataString(item.Key.Substring(prefix.Length)), item => item.Value);
        }

        /// <summary>按已有业务键和原类型声明读表及写表方法。</summary>
        public IEnumerable<FlowMethod> GetFlowMethods(bool includeDraft = false)
        {
            FlowField key = new FlowField("Key", typeof(string), values.Keys.FirstOrDefault() ?? "");
            key.Options.AddRange(values.Select(item => new FlowOption { Value = item.Key, Label = item.Key + " (" + item.Value.GetType().Name + ")" }));
            FlowMethod read = FlowMethods.Sync("constant.read", "读取常量值", "Read constant value", "Leer valor constante", new[] { key },
                new[] { new FlowField("Value", typeof(object)) }, Read, false);
            read.ValidateInputs = Validate;
            yield return read;
            FlowMethod write = FlowMethods.Sync("constant.set", "设置表值", "Set table value", "Establecer valor de tabla",
                new[] { key, new FlowField("Value", typeof(object)) }, new[] { new FlowField("Value", typeof(object)) }, call=>null, false);
            write.Invoke=Set;
            write.ValidateInputs = Validate;
            yield return write;
        }

        /// <summary>保存时校验固定键和值，动态绑定在执行前按同一规则检查。</summary>
        private void Validate(IDictionary<string, object> input)
        {
            if (!input.ContainsKey("Key")) return;
            string key = Convert.ToString(input["Key"]);
            if (!values.ContainsKey(key)) throw new InvalidOperationException(FlowText.T("constantKey"));
            if (input.ContainsKey("Value")) ConvertValue(key, input["Value"]);
        }

        /// <summary>读取流程已写入实际表的值，没有设置时使用本次配置快照。</summary>
        private IDictionary<string, object> Read(FlowCall call)
        {
            string key = call.Get<string>("Key"); object value;
            if (!store.TryReadFlowValue(scope,key,out value)) value=values[key];
            return FlowCall.Result("Value", value);
        }

        /// <summary>在真实常量表中设置值，页面和后续流程立即读取，文件仍由主保存写入。</summary>
        private async Task<IDictionary<string, object>> Set(FlowCall call)
        {
            string key = call.Get<string>("Key"); object value = ConvertValue(key, call.Inputs["Value"]);
            object actual=await store.SetFromFlowAsync(scope,key,value,values[key].GetType(),call,recipePath).ConfigureAwait(false);
            return FlowCall.Result("Value", actual);
        }

        /// <summary>严格保留已有类型，整数不接受小数，数值不接受 NaN 或无穷。</summary>
        private object ConvertValue(string key, object value)
        {
            object initial = values[key];
            string type = initial is bool ? "Boolean" : initial is long ? "Integer" : initial is double ? "Number" : "Text";
            return ConstantStore.Parse(type, Convert.ToString(value, CultureInfo.InvariantCulture));
        }
    }
}
