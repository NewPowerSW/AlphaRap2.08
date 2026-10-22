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
                new[] { new FlowField("Value", typeof(object)) }, new[] { new FlowField("Value", typeof(object)) }, call => FlowCall.Result("Value", call.Inputs["Value"]), false);
            FlowField operation = new FlowField("Operation", typeof(string), "+");
            foreach (string item in new[] { "+", "-", "*", "/" }) operation.Options.Add(new FlowOption { Value = item, Label = item });
            yield return FlowMethods.Sync("calculate", "数值计算", "Calculate values", "Calcular valores",
                new[] { new FlowField("A", typeof(double), "0"), operation, new FlowField("B", typeof(double), "0") },
                new[] { new FlowField("Value", typeof(double)) }, Calculate, false);
            yield return FlowMethods.Sync("counter", "循环索引", "Loop index", "Índice de ciclo",
                new[] { new FlowField("Start", typeof(int), "0"), new FlowField("End", typeof(int), "9"), new FlowField("Step", typeof(int), "1") },
                new[] { new FlowField("Index", typeof(int)) }, Count, false);
            FlowMethod delay = FlowMethods.Sync("delay", "等待时间", "Delay", "Esperar", new[] { new FlowField("Milliseconds", typeof(int), "1000") },
                new[] { new FlowField("ElapsedMs", typeof(int)) }, call => null, false);
            delay.Invoke = async call => {
                int milliseconds = call.Get<int>("Milliseconds");
                if (milliseconds < 0) throw new InvalidOperationException(FlowText.T("error.delayRange"));
                await Task.Delay(milliseconds, call.Token).ConfigureAwait(false);
                return FlowCall.Result("ElapsedMs", milliseconds);
            };
            yield return delay;
            yield return FlowMethods.Sync("cycle.begin", "开始产品周期", "Begin product cycle", "Iniciar ciclo de producto",
                new[] { new FlowField("Key", typeof(string)) }, new[] { new FlowField("CycleId", typeof(string)) }, call => {
                    call.Context.BeginCycle(call.Get<string>("Key")); return FlowCall.Result("CycleId", call.Context.CycleId);
                }, false);
            yield return FlowMethods.Sync("cycle.end", "结束产品周期并释放结果", "Complete cycle and release results", "Completar ciclo y liberar resultados",
                new FlowField[0], new FlowField[0], call => { call.Context.CompleteCycle(); return new Dictionary<string, object>(); }, false);
            FlowMethod prompt = FlowMethods.Sync("operator.input", "等待操作员输入", "Request operator input", "Solicitar entrada",
                new[] { new FlowField("Prompt", typeof(string)) }, new[] { new FlowField("Value", typeof(string)) }, call => null, false);
            prompt.Invoke = async call => {
                if (call.RequestInput == null) throw new InvalidOperationException(FlowText.T("error.promptHost"));
                return FlowCall.Result("Value", await call.RequestInput(call.Get<string>("Prompt"), call.Token).ConfigureAwait(false));
            };
            yield return prompt;
            yield return FlowMethods.Sync("alarm.show", "触发报警", "Show alarm", "Mostrar alarma",
                new[] { new FlowField("Code", typeof(string)) }, new[] { new FlowField("Code", typeof(string)) }, call => {
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
