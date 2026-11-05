using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using NPSDK;
using AlphaRap.FlowProgramming;

namespace AlphaRap.PLC
{
    public partial class INOVANCE : IFlowObject
    {
        /// <summary>声明现有 PLC 实例的读、写和等待方法，不在设计器创建通信对象。</summary>
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
            yield return WaitMethod();
        }

        /// <summary>声明只读等待块。运行使用现有 PLC 实例，模拟通过同一比较和超时逻辑。</summary>
        private FlowMethod WaitMethod()
        {
            FlowMethod method = new FlowMethod("plc.wait", "等待 PLC 值", "Wait for PLC value", "Esperar valor del PLC") { UsesHardware = true };
            method.Inputs.AddRange(new[] {
                new FlowField("Address", typeof(string)) { DisplayName = new[] { "PLC 地址", "PLC address", "Dirección de PLC" },
                    Help = new[] { "D/R 读取一个 32 位整数，X/Y/S/M 读取一位（0 或 1）。", "D/R reads one signed 32-bit integer. X/Y/S/M reads one bit (0 or 1).", "D/R lee un entero de 32 bits con signo. X/Y/S/M lee un bit (0 o 1)." } },
                new FlowField("ExpectedValue", typeof(int), "1") { DisplayName = new[] { "期望值", "Expected value", "Valor esperado" } },
                new FlowField("WaitMs", typeof(int), "1500") { DisplayName = new[] { "等待上限", "Wait limit", "Límite de espera" }, Unit = "ms", Advanced = true,
                    Help = new[] { "请使节点超时大于此等待上限，以便显示最后读值。", "Set the node timeout above this wait limit to see the last reading.", "Configure el tiempo del nodo por encima de este límite para ver la última lectura." } },
                new FlowField("SimulateMismatch", typeof(bool), "False") { DisplayName = new[] { "模拟值一直不匹配", "Simulate a persistent mismatch", "Simular un valor distinto permanente" }, Advanced = true }
            });
            method.Outputs.Add(new FlowField("Value", typeof(int)) { DisplayName = new[] { "实际读值", "Actual reading", "Lectura real" } });
            method.ValidateInputs = ValidateWait;
            method.Invoke = call => Task.Run(() => WaitForValue(call, ReadWaitValue));
            method.Simulate = SimulateWait;
            return method;
        }

        /// <summary>保存时校验已知参数，运行时再校验解析后的动态参数。</summary>
        private static void ValidateWait(IDictionary<string, object> values)
        {
            object value;
            if (values.TryGetValue("WaitMs", out value) && FlowActions.Integer(value) <= 0)
                throw new InvalidOperationException(DeviceText.T("error.plcWaitLimit"));
            if (!values.TryGetValue("Address", out value)) return;
            string address = FlowValue.Text(value);
            int index;
            if (address.Length < 2 || "DRXYSM".IndexOf(address[0]) < 0 ||
                !int.TryParse(address.Substring(1), NumberStyles.None, CultureInfo.InvariantCulture, out index) ||
                (IsWordAddress(address) && index == int.MaxValue))
                throw new InvalidOperationException(string.Format(DeviceText.T("error.plcWaitAddress"), address));
            if (IsWordAddress(address) || !values.TryGetValue("ExpectedValue", out value)) return;
            int expected = FlowActions.Integer(value);
            if (expected != 0 && expected != 1) throw new InvalidOperationException(DeviceText.T("error.plcWaitBit"));
        }

        /// <summary>判断当前地址是否由驱动按字读取。</summary>
        private static bool IsWordAddress(string address)
        {
            return address[0] == 'D' || address[0] == 'R';
        }

        /// <summary>定时读取一个地址，值相等即返回；超时保留最近一次读值。</summary>
        private static async Task<IDictionary<string, object>> WaitForValue(FlowCall call, Func<string, int> read)
        {
            string address = call.Get<string>("Address");
            int expected = call.Get<int>("ExpectedValue");
            int wait = call.Get<int>("WaitMs");
            Stopwatch timer = Stopwatch.StartNew();
            int? last = null;
            while (timer.ElapsedMilliseconds < wait)
            {
                call.Token.ThrowIfCancellationRequested();
                last = read(address);
                call.Token.ThrowIfCancellationRequested();
                if (timer.ElapsedMilliseconds >= wait) break;
                if (last == expected) return FlowCall.Result("Value", last.Value);
                await Task.Delay((int)Math.Max(1, Math.Min(20, wait - timer.ElapsedMilliseconds)), call.Token).ConfigureAwait(false);
            }
            call.Token.ThrowIfCancellationRequested();
            throw new TimeoutException(string.Format(DeviceText.T("error.plcWaitTimeout"), address, expected,
                last.HasValue ? last.Value.ToString(CultureInfo.InvariantCulture) : DeviceText.T("error.plcWaitNoSample"), wait));
        }

        /// <summary>模拟固定读值，使用同一等待逻辑；不访问 PLC。</summary>
        private static async Task<IDictionary<string, object>> SimulateWait(FlowCall call)
        {
            int observed = call.Get<int>("ExpectedValue");
            if (call.Get<bool>("SimulateMismatch")) observed = observed == 0 ? 1 : 0;
            IDictionary<string, object> result = await WaitForValue(call, address => observed).ConfigureAwait(false);
            result.Add("Simulated", true);
            return result;
        }

        /// <summary>读取一个已校验的地址，传输失败和合法的负数读值分别处理。</summary>
        private int ReadWaitValue(string address)
        {
            bool word = IsWordAddress(address);
            int resultCode;
            string value = ReadPlc(address, word ? 2 : 1, word ? "int" : "bit", out resultCode);
            if (resultCode != 1) throw new InvalidOperationException(string.Format(DeviceText.T("error.plcWaitRead"), address, resultCode));
            int number;
            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
                throw new InvalidOperationException(string.Format(DeviceText.T("error.plcWaitValue"), address, value));
            return number;
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
