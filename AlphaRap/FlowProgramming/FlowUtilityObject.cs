using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    /// <summary>与硬件无关的流程上下文、数值和交互能力。</summary>
    internal sealed class FlowUtilityObject : IFlowObject
    {
        /// <summary>声明流程基础方法，不混入设备操作。</summary>
        public IEnumerable<FlowMethod> GetFlowMethods(bool includeDraft = false)
        {
            yield return FlowMethods.Sync("value", "读取或传递数据", "Read or pass a value", "Leer o pasar un valor",
                new[] { new FlowField("Value", typeof(object)) { DisplayName = new[] { "传递的值", "Value to pass", "Valor a transferir" } } },
                new[] { new FlowField("Value", typeof(object)) { DisplayName = new[] { "传递结果", "Passed value", "Valor transferido" } } }, call => FlowCall.Result("Value", call.Inputs["Value"]), false);
            FlowField operation = new FlowField("Operation", typeof(string), "+") { DisplayName = new[] { "运算符", "Operator", "Operador" } };
            foreach (string item in new[] { "+", "-", "*", "/" }) operation.Options.Add(new FlowOption { Value = item, Label = item });
            yield return FlowMethods.Sync("calculate", "数值计算", "Calculate values", "Calcular valores",
                new[] { new FlowField("A", typeof(double), "0") { DisplayName = new[] { "左侧数值", "Left value", "Valor izquierdo" } }, operation,
                    new FlowField("B", typeof(double), "0") { DisplayName = new[] { "右侧数值", "Right value", "Valor derecho" } } },
                new[] { new FlowField("Value", typeof(double)) { DisplayName = new[] { "计算结果", "Calculation result", "Resultado del cálculo" } } }, Calculate, false);
            yield return FlowMethods.Sync("counter", "循环索引", "Loop index", "Índice de ciclo",
                new[] { new FlowField("Start", typeof(int), "0") { DisplayName = new[] { "起始序号", "Start index", "Índice inicial" } },
                    new FlowField("End", typeof(int), "9") { DisplayName = new[] { "结束序号", "End index", "Índice final" },
                        Help = new[] { "包含此序号；超过后从起始序号重新计数。", "Includes this index; the next count wraps back to the start after this index is exceeded.", "Incluye este índice; el siguiente conteo vuelve al inicio después de superarlo." } },
                    new FlowField("Step", typeof(int), "1") { DisplayName = new[] { "每次增加", "Increment", "Incremento" }, Advanced = true } },
                new[] { new FlowField("Index", typeof(int)) { DisplayName = new[] { "当前序号", "Current index", "Índice actual" } } }, Count, false);
            FlowMethod delay = FlowMethods.Sync("delay", "等待时间", "Delay", "Esperar", new[] { new FlowField("Milliseconds", typeof(int), "1000") {
                DisplayName = new[] { "等待时长", "Wait duration", "Duración de espera" }, Unit = "ms" } },
                new[] { new FlowField("ElapsedMs", typeof(int)) { DisplayName = new[] { "已等待时长", "Elapsed wait", "Espera transcurrida" }, Unit = "ms" } }, call => null, false);
            delay.Invoke = async call => {
                int milliseconds = call.Get<int>("Milliseconds");
                if (milliseconds < 0) throw new InvalidOperationException(FlowText.T("error.delayRange"));
                await Task.Delay(milliseconds, call.Token).ConfigureAwait(false);
                return FlowCall.Result("ElapsedMs", milliseconds);
            };
            yield return delay;
            yield return FlowMethods.Sync("cycle.begin", "开始产品周期", "Begin product cycle", "Iniciar ciclo de producto",
                new[] { new FlowField("Key", typeof(string)) { DisplayName = new[] { "产品标识", "Product identifier", "Identificador de producto" },
                    Help = new[] { "用于关联本件产品的条码或业务标识；开始周期会释放上一周期的结果。", "Barcode or business identifier for this product; beginning a cycle releases the previous cycle's results.", "Código de barras o identificador de este producto; al iniciar un ciclo se liberan los resultados del anterior." } } },
                new[] { new FlowField("CycleId", typeof(string)) { DisplayName = new[] { "产品周期编号", "Product cycle identifier", "Identificador del ciclo de producto" } } }, call => {
                    call.Context.BeginCycle(call.Get<string>("Key")); return FlowCall.Result("CycleId", call.Context.CycleId);
                }, false);
            yield return FlowMethods.Sync("cycle.end", "结束产品周期并释放结果", "Complete cycle and release results", "Completar ciclo y liberar resultados",
                new FlowField[0], new FlowField[0], call => { call.Context.CompleteCycle(); return new Dictionary<string, object>(); }, false);
            FlowMethod prompt = FlowMethods.Sync("operator.input", "等待操作员输入", "Request operator input", "Solicitar entrada",
                new[] { new FlowField("Prompt", typeof(string)) { DisplayName = new[] { "操作提示", "Operator prompt", "Mensaje para el operador" },
                    Help = new[] { "显示给操作员的输入说明，内容按原样显示。", "Input instructions shown to the operator exactly as entered.", "Instrucciones de entrada que se muestran al operador tal como se escriben." } } },
                new[] { new FlowField("Value", typeof(string)) { DisplayName = new[] { "操作员输入", "Operator input", "Entrada del operador" } } }, call => null, false);
            prompt.Invoke = async call => {
                if (call.RequestInput == null) throw new InvalidOperationException(FlowText.T("error.promptHost"));
                return FlowCall.Result("Value", await call.RequestInput(call.Get<string>("Prompt"), call.Token).ConfigureAwait(false));
            };
            yield return prompt;
            yield return FlowMethods.Sync("alarm.show", "触发报警", "Show alarm", "Mostrar alarma",
                new[] { new FlowField("Code", typeof(string)) { DisplayName = new[] { "报警号", "Alarm code", "Código de alarma" } } },
                new[] { new FlowField("Code", typeof(string)) { DisplayName = new[] { "已触发报警号", "Triggered alarm code", "Código de alarma activada" } } }, call => {
                    string code = call.Get<string>("Code");
                    if (string.IsNullOrWhiteSpace(code)) throw new InvalidOperationException(FlowText.T("error.alarmCode"));
                    if (!call.Simulation) Alarm.Show(code);
                    return FlowCall.Result("Code", code);
                }, false);
        }

        /// <summary>计算有限数值，除零及未知运算符直接报错。</summary>
        private static IDictionary<string, object> Calculate(FlowCall call)
        {
            double a = call.Get<double>("A"), b = call.Get<double>("B"), value;
            switch (call.Get<string>("Operation"))
            {
                case "+": value = a + b; break;
                case "-": value = a - b; break;
                case "*": value = a * b; break;
                case "/": if (b == 0) throw new InvalidOperationException(FlowText.T("error.divisionZero")); value = a / b; break;
                default: throw new InvalidOperationException(FlowText.T("error.calculationOperator"));
            }
            return FlowCall.Result("Value", FlowActions.Number(value));
        }

        /// <summary>以当前节点保存循环索引，避免其他节点共享计数状态。</summary>
        private static IDictionary<string, object> Count(FlowCall call)
        {
            int start = call.Get<int>("Start"), end = call.Get<int>("End"), increment = call.Get<int>("Step");
            if (increment <= 0 || end < start) throw new InvalidOperationException(FlowText.T("error.counterRange"));
            object previous;
            long next = call.Context.Variables.TryGetValue(call.Step.Id, out previous) ? (long)(int)previous + increment : start;
            int index = next > end ? start : (int)next;
            call.Context.Variables[call.Step.Id] = index;
            return FlowCall.Result("Index", index);
        }
    }
}
