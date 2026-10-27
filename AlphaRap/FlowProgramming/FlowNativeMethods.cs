using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    /// <summary>从当前流程窗体发现原生事件方法，调用始终绑定该窗体实际实例。</summary>
    internal static class FlowNativeMethods
    {
        private static readonly ConcurrentDictionary<Type, MethodInfo[]> cache = new ConcurrentDictionary<Type, MethodInfo[]>();

        /// <summary>窗体类型及稳定 Name 共同标识所属页面，不随显示标题或语言变化。</summary>
        internal static string ResourceId(Flow_BaseForm form)
        {
            return "native/" + form.GetType().FullName + "/" + form.Name;
        }

        /// <summary>只读取业务继承层的方法信息，不创建窗体或执行任何事件。</summary>
        internal static List<FlowMethod> Discover(Flow_BaseForm form)
        {
            return cache.GetOrAdd(form.GetType(), FindMethods).Select(method => Describe(form, method)).ToList();
        }

        /// <summary>公开及私有无参 ResultType 方法即原生契约，SDK 基类和辅助标记被排除。</summary>
        private static MethodInfo[] FindMethods(Type type)
        {
            List<MethodInfo> found = new List<MethodInfo>();
            for (Type owner = type; owner != null && owner != typeof(Flow_BaseForm); owner = owner.BaseType)
                found.AddRange(owner.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(IsEventMethod));
            HashSet<string> simulators = new HashSet<string>(found.Select(method => method.GetCustomAttribute<FlowToolAttribute>(true))
                .Where(options => options != null && !string.IsNullOrWhiteSpace(options.SimulationMethod)).Select(options => options.SimulationMethod), StringComparer.Ordinal);
            return found.Where(method => !simulators.Contains(method.Name)).GroupBy(method => method.IsVirtual ? method.GetBaseDefinition() : method)
                .Select(group => group.First()).OrderBy(MethodIdentity, StringComparer.Ordinal).ToArray();
        }

        /// <summary>原生事件不接受泛型、静态、参数或任意返回对象，方法名无需额外登记。</summary>
        private static bool IsEventMethod(MethodInfo method)
        {
            return IsNativeSignature(method) && !method.IsDefined(typeof(FlowToolIgnoreAttribute), true);
        }

        /// <summary>模拟函数可独立隐藏，其调用签名仍必须与原生事件一致。</summary>
        private static bool IsNativeSignature(MethodInfo method)
        {
            return !method.IsStatic && !method.IsAbstract && !method.ContainsGenericParameters && !method.IsSpecialName
                && !method.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), true)
                && method.ReturnType == typeof(Flow_Chart.ResultType) && method.GetParameters().Length == 0;
        }

        /// <summary>保存程序集简单名、声明类型和原生签名，不包含程序集版本或部署路径。</summary>
        private static string MethodIdentity(MethodInfo method)
        {
            return method.DeclaringType.Assembly.GetName().Name + "|" + method.DeclaringType.FullName + "|" + method.Name + "():NPSDK.Flow_Chart.ResultType";
        }

        /// <summary>稳定方法身份区分不同业务父类中的同名私有事件。</summary>
        private static string MethodId(MethodInfo method)
        {
            using (SHA256 hash = SHA256.Create())
                return "native." + method.Name + "." + BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(MethodIdentity(method))), 0, 12).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>保留原事件返回的路由，仅提供 Result 观察字段，不自动伪造业务数据端口。</summary>
        private static FlowMethod Describe(Flow_BaseForm form, MethodInfo method)
        {
            FlowToolAttribute options = method.GetCustomAttribute<FlowToolAttribute>(true);
            string name = method.Name.EndsWith("_FlowChartRun", StringComparison.Ordinal) ? method.Name.Substring(0, method.Name.Length - "_FlowChartRun".Length) : method.Name;
            string[] title = options == null ? new[] { name, name, name } : options.Title;
            FlowMethod tool = new FlowMethod(MethodId(method), title[0], title[1], title[2])
            {
                UsesFlowResult = true,
                UsesHardware = options == null || options.UsesHardware
            };
            tool.Outputs.Add(new FlowField("Result", typeof(Flow_Chart.ResultType)) { DisplayName = new[] { "流程返回值", "Flow result", "Resultado del flujo" } });
            tool.Invoke = call => Invoke(form, method, call);
            if (options != null && !string.IsNullOrWhiteSpace(options.SimulationMethod))
            {
                MethodInfo simulator = MethodsInHierarchy(form.GetType()).FirstOrDefault(candidate => candidate.Name == options.SimulationMethod && IsNativeSignature(candidate));
                if (simulator == null) throw new InvalidOperationException(FlowNativeText.T("simulator") + method.Name);
                tool.Simulate = call => Invoke(form, simulator, call);
            }
            return tool;
        }

        /// <summary>查找同一实际窗体的私有或继承模拟方法，不访问其他页面实例。</summary>
        private static IEnumerable<MethodInfo> MethodsInHierarchy(Type type)
        {
            for (Type owner = type; owner != null && owner != typeof(Flow_BaseForm); owner = owner.BaseType)
                foreach (MethodInfo method in owner.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) yield return method;
        }

        /// <summary>单次调用原事件；IDLE 重试、暂停及超时均由同一 SDK 运行器负责。</summary>
        private static Task<IDictionary<string, object>> Invoke(Flow_BaseForm form, MethodInfo method, FlowCall call)
        {
            return Task.Run<IDictionary<string, object>>(() =>
            {
                call.Token.ThrowIfCancellationRequested();
                Flow_Chart.ResultType result;
                try { result = (Flow_Chart.ResultType)method.Invoke(form, null); }
                catch (TargetInvocationException error)
                {
                    if (error.InnerException == null) throw;
                    ExceptionDispatchInfo.Capture(error.InnerException).Throw();
                    throw;
                }
                call.Token.ThrowIfCancellationRequested();
                return FlowCall.Result("Result", result);
            });
        }

        /// <summary>仅将旧节点唯一绑定的本页事件导入草稿，多播及外部对象绑定保留待修复状态。</summary>
        internal static bool BindExisting(Flow_BaseForm form, Flow_Chart node, FlowStep step)
        {
            if (step.Action != "legacy" || step.ExecutionMode != "Action" || !string.IsNullOrEmpty(step.CodeModuleId)) return false;
            Flow_Chart.FlowChartHandle[] handlers = node.GetFlowChartHandlers();
            if (handlers.Length != 1 || !ReferenceEquals(handlers[0].Target, form) || !IsEventMethod(handlers[0].Method)) return false;
            MethodInfo method = cache.GetOrAdd(form.GetType(), FindMethods).FirstOrDefault(candidate => candidate == handlers[0].Method);
            if (method == null) return false;
            step.Target = ResourceId(form);
            step.Action = MethodId(method);
            step.UsesFlowResult = true;
            step.Inputs.Clear();
            step.Conditions.Clear(); step.WaitForCondition = false; step.AnyCondition = false;
            step.Branches = FlowBranch.Default();
            foreach (string code in new[] { "CASE1", "CASE2", "CASE3" })
                if (!string.IsNullOrEmpty(step.TargetOf(code))) step.Branches.Add(new FlowBranch { Code = code, Unconditional = true });
            return true;
        }

        /// <summary>为原来多播事件给出明确修复信息，禁止静默执行其中一个处理器。</summary>
        internal static void ValidateLegacy(Flow_BaseForm form, FlowStep step)
        {
            if (form == null || step.Action != "legacy") return;
            Flow_Chart node = Nodes(form).FirstOrDefault(candidate => candidate.Name == step.ControlName);
            if (node != null && node.GetFlowChartHandlers().Length > 1)
                throw new InvalidOperationException(string.Format(FlowNativeText.T("multiple"), step.Caption));
        }

        /// <summary>只枚举实际页面的原生节点，不进入节点内部装饰控件。</summary>
        private static IEnumerable<Flow_Chart> Nodes(Control root)
        {
            foreach (Control child in root.Controls)
            {
                Flow_Chart node = child as Flow_Chart;
                if (node != null) { yield return node; continue; }
                foreach (Flow_Chart nested in Nodes(child)) yield return nested;
            }
        }
    }

    /// <summary>原生事件工具的固定三语文案。</summary>
    internal static class FlowNativeText
    {
        internal static readonly Dictionary<string, string[]> Defaults = new Dictionary<string, string[]> {
            { "category", new[] { "代码流程", "Native flow", "Flujo nativo" } },
            { "badgeNative", new[] { "代码", "Code", "Código" } },
            { "badgeAction", new[] { "功能", "Action", "Función" } },
            { "badgeCondition", new[] { "条件", "Condition", "Condición" } },
            { "badgePending", new[] { "待配置", "Unconfigured", "Sin configurar" } },
            { "nativeBinding", new[] { "执行方式：代码流程\n所属页面：{0}\n函数：{1}\n按 ResultType 返回值跳转。", "Execution: native flow\nPage: {0}\nFunction: {1}\nRoutes by its ResultType return value.", "Ejecución: flujo nativo\nPágina: {0}\nFunción: {1}\nSelecciona la ruta según el ResultType devuelto." } },
            { "actionBinding", new[] { "执行方式：对象动作\n对象身份：{0}\n方法身份：{1}\n按 NEXT / CASE 条件跳转。", "Execution: object action\nObject identity: {0}\nMethod identity: {1}\nRoutes by NEXT / CASE conditions.", "Ejecución: acción del objeto\nIdentidad del objeto: {0}\nIdentidad del método: {1}\nSelecciona la ruta según las condiciones NEXT / CASE." } },
            { "conditionBinding", new[] { "执行方式：仅判断条件\n按 NEXT / CASE 条件选择出口。", "Execution: conditions only\nSelects an exit using NEXT / CASE conditions.", "Ejecución: solo condiciones\nSelecciona una salida mediante las condiciones NEXT / CASE." } },
            { "legacyBinding", new[] { "生产页面当前保留原流程，使用原事件执行。以下为工程节点绑定：\n{0}", "Production currently uses the original flow and its events. Engineering node binding:\n{0}", "La producción usa actualmente el flujo y los eventos originales. Vínculo del nodo de ingeniería:\n{0}" } },
            { "routes", new[] { "由函数返回 NEXT / CASE1 / CASE2 / CASE3 选择出口；这里只设置跳转目标。", "The function returns NEXT / CASE1 / CASE2 / CASE3; only configure the destination here.", "La función devuelve NEXT / CASE1 / CASE2 / CASE3; aquí solo se configura el destino." } },
            { "help", new[] { "调用当前窗体的原生 FlowChartRun 方法。IDLE 留在本节点等待，End 结束本流程；超时与断点沿用当前设置。", "Call the current form's native FlowChartRun method. IDLE waits in this node; End finishes this flow. Timeout and breakpoints use the current settings.", "Llama al método FlowChartRun nativo del formulario actual. IDLE espera en este nodo; End termina este flujo. Se aplican el tiempo de espera y los puntos de interrupción actuales." } },
            { "multiple", new[] { "节点 {0} 绑定了多个原生事件。请明确选择一个流程函数，或在一个函数中按顺序调用所需逻辑。", "Node {0} has multiple native handlers. Select one flow function, or call the required logic in order from one function.", "El nodo {0} tiene varios controladores nativos. Seleccione una función de flujo o invoque la lógica necesaria en orden desde una función." } },
            { "contract", new[] { "原生流程返回方式与所选函数不一致，请重新选择对象方法：{0}", "The native routing flag does not match the selected function. Select the object method again: {0}", "La marca de ruta nativa no coincide con la función seleccionada. Vuelva a seleccionar el método del objeto: {0}" } },
            { "simulator", new[] { "原生模拟方法必须是当前窗体的无参 ResultType 方法：", "The native simulator must be a parameterless ResultType method on the current form: ", "El simulador nativo debe ser un método ResultType sin parámetros del formulario actual: " } }
        };

        /// <summary>沿用现场语言表及框架缓存，保留节点和方法的稳定业务身份。</summary>
        internal static string T(string key)
        {
            string[] words = Defaults[key];
            return UiTheme.IsDesignTime ? words[0] : MiddleLayer.LangMsg("EquipmentFlow", "native_" + key, words[0], words[1], words[2]);
        }
    }
}
