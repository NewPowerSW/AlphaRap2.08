using System;
using System.Collections.Generic;
using NPSDK;
using AlphaRap.FlowProgramming;

namespace AlphaRap.PLC
{
    public partial class INOVANCE : IFlowObject
    {
        /// <summary>声明现有 PLC 实例的读写方法，不在设计器创建通信对象。</summary>
        public IEnumerable<FlowMethod> GetFlowMethods(bool includeDraft = false)
        {
            yield return FlowMethods.Sync("plc.read", "读取 PLC", "Read PLC", "Leer PLC",
                new[] { new FlowField("Address", typeof(string)), new FlowField("Count", typeof(int), "2"), ValueType(false) }, new[] { new FlowField("Value", typeof(string)) }, call => {
                    string value = ReadPlc(call.Get<string>("Address"), call.Get<int>("Count"), call.Get<string>("Type"));
                    if (value == "-999") throw new InvalidOperationException(ReadErrorMessage);
                    return FlowCall.Result("Value", value);
                });
            yield return FlowMethods.Sync("plc.write", "写入 PLC", "Write PLC", "Escribir PLC",
                new[] { new FlowField("Address", typeof(string)), new FlowField("Value", typeof(object)), ValueType(true) }, new[] { new FlowField("Value", typeof(string)) }, call => {
                    string value = FlowValue.Text(call.Inputs["Value"]);
                    if (!WritePlc(call.Get<string>("Address"), value, call.Get<string>("Type"))) throw new InvalidOperationException(WritErrorMessage);
                    return FlowCall.Result("Value", value);
                });
        }

        /// <summary>提供此 PLC 驱动实际支持的数据类型。</summary>
        private static FlowField ValueType(bool write)
        {
            FlowField field = new FlowField("Type", typeof(string), "int");
            foreach (string type in write ? new[] { "bit", "int16", "int", "int32", "float" } : new[] { "bit", "int", "float" }) field.Options.Add(new FlowOption { Value = type, Label = type });
            return field;
        }
    }
}
