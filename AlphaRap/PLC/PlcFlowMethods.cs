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
                new[] { new FlowField("Address", typeof(string)) { DisplayName = new[] { "PLC 地址", "PLC address", "Dirección de PLC" },
                        Help = new[] { "输入此 PLC 支持的完整地址，例如 D1010。", "Enter a complete address supported by this PLC, such as D1010.", "Introduzca una dirección completa admitida por este PLC, como D1010." } },
                    new FlowField("Count", typeof(int), "2") { DisplayName = new[] { "读取长度", "Read length", "Longitud de lectura" } }, ValueType(false) },
                new[] { new FlowField("Value", typeof(string)) { DisplayName = new[] { "PLC 读取结果", "PLC read result", "Resultado de lectura del PLC" } } }, call => {
                    string value = ReadPlc(call.Get<string>("Address"), call.Get<int>("Count"), call.Get<string>("Type"));
                    if (value == "-999") throw new InvalidOperationException(ReadErrorMessage);
                    return FlowCall.Result("Value", value);
                });
            yield return FlowMethods.Sync("plc.write", "写入 PLC", "Write PLC", "Escribir PLC",
                new[] { new FlowField("Address", typeof(string)) { DisplayName = new[] { "PLC 地址", "PLC address", "Dirección de PLC" },
                        Help = new[] { "输入此 PLC 支持的完整地址，例如 D1010。", "Enter a complete address supported by this PLC, such as D1010.", "Introduzca una dirección completa admitida por este PLC, como D1010." } },
                    new FlowField("Value", typeof(object)) { DisplayName = new[] { "写入值", "Value to write", "Valor a escribir" } }, ValueType(true) },
                new[] { new FlowField("Value", typeof(string)) { DisplayName = new[] { "已写入值", "Written value", "Valor escrito" } } }, call => {
                    string value = FlowValue.Text(call.Inputs["Value"]);
                    if (!WritePlc(call.Get<string>("Address"), value, call.Get<string>("Type"))) throw new InvalidOperationException(WritErrorMessage);
                    return FlowCall.Result("Value", value);
                });
        }

        /// <summary>提供此 PLC 驱动实际支持的数据类型。</summary>
        private static FlowField ValueType(bool write)
        {
            FlowField field = new FlowField("Type", typeof(string), "int") { DisplayName = new[] { "PLC 数据类型", "PLC data type", "Tipo de datos de PLC" } };
            foreach (string type in write ? new[] { "bit", "int16", "int", "int32", "float" } : new[] { "bit", "int", "float" }) field.Options.Add(new FlowOption { Value = type, Label = type });
            return field;
        }
    }
}
