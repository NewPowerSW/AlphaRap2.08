using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NPSDK;
using NPSDK.Motion;

namespace AlphaRap.FlowProgramming
{
    /// <summary>可选的业务方法显示信息；没有特性的方法仍按代码名称自动进入工具箱。</summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public sealed class FlowToolAttribute : Attribute
    {
        public readonly string[] Title;
        public string Id { get; set; }
        public bool UsesHardware { get; set; } = true;
        public string SimulationMethod { get; set; }

        /// <summary>为工具提供中文、英文和西班牙文标题。</summary>
        public FlowToolAttribute(string chinese, string english, string spanish)
        {
            Title = new[] { chinese, english, spanish };
        }
    }

    /// <summary>不把此公开辅助方法作为流程工具。</summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public sealed class FlowToolIgnoreAttribute : Attribute { }

    /// <summary>将设备、轴和 IO 的派生业务方法转换为统一工具，不建立或登记第二份实例。</summary>
    internal static class FlowInheritedMethods
    {
        private static readonly ConcurrentDictionary<Type, MethodInfo[]> methods = new ConcurrentDictionary<Type, MethodInfo[]>();

        /// <summary>读取派生业务层的公开方法，元数据浏览不调用构造函数、属性或设备操作。</summary>
        internal static IEnumerable<FlowMethod> Discover(object instance)
        {
            if (instance == null) yield break;
            foreach (MethodInfo method in methods.GetOrAdd(instance.GetType(), FindMethods))
                yield return Describe(instance, method);
        }

        /// <summary>限定设备业务继承层，已有显式方法目录的层继续使用其原有声明。</summary>
        private static MethodInfo[] FindMethods(Type type)
        {
            Type root = RootType(type);
            if (root == null) return new MethodInfo[0];
            List<MethodInfo> candidates = new List<MethodInfo>();
            for (Type owner = type; owner != null && owner != root; owner = owner.BaseType)
            {
                if (owner.GetMethod("GetFlowMethods", BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance,
                    null, new[] { typeof(bool) }, null) != null) continue;
                candidates.AddRange(owner.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(method => CanDescribe(method, root)));
            }
            HashSet<string> simulations = new HashSet<string>(candidates.Select(method => method.GetCustomAttribute<FlowToolAttribute>(true))
                .Where(attribute => attribute != null && !string.IsNullOrEmpty(attribute.SimulationMethod)).Select(attribute => attribute.SimulationMethod), StringComparer.Ordinal);
            return candidates.Where(method => !simulations.Contains(method.Name))
                .GroupBy(Signature).Select(group => group.First()).OrderBy(Signature, StringComparer.Ordinal).ToArray();
        }

        /// <summary>只从框架支持的实际对象继承层提取方法，排除 WinForms 生命周期。</summary>
        private static Type RootType(Type type)
        {
            return new[] { typeof(AbstractDevice), typeof(NP_Motor), typeof(NP_Input), typeof(NP_Output), typeof(NP_Cylinder), typeof(VisionproInterface) }
                .FirstOrDefault(root => root.IsAssignableFrom(type));
        }

        /// <summary>只接受具有明确输入输出的业务签名，基类动作仍由原适配器负责。</summary>
        private static bool CanDescribe(MethodInfo method, Type root)
        {
            if (method.IsSpecialName || method.IsAbstract || method.ContainsGenericParameters || method.Name == "GetFlowMethods") return false;
            if (method.ReturnType == typeof(void) && method.IsDefined(typeof(System.Runtime.CompilerServices.AsyncStateMachineAttribute), true)) return false;
            if (method.IsDefined(typeof(FlowToolIgnoreAttribute), true)) return false;
            MethodInfo original = method.GetBaseDefinition();
            if (original.DeclaringType == root || original.DeclaringType == typeof(object)
                || typeof(System.Windows.Forms.Control).IsAssignableFrom(root) && !root.IsAssignableFrom(original.DeclaringType)) return false;
            if (method.GetParameters().Any(parameter => parameter.ParameterType != typeof(CancellationToken) && !IsFieldType(FieldType(parameter)))) return false;
            return ResultType(method) == typeof(void) || IsFieldType(ResultType(method));
        }

        /// <summary>支持明确标量、枚举及一维标量数组；复杂对象不猜测字段结构。</summary>
        private static bool IsFieldType(Type type)
        {
            if (type.IsEnum) return Enum.GetNames(type).Length > 0;
            if (type.IsArray) return type.GetArrayRank() == 1 && !type.GetElementType().IsArray && IsFieldType(type.GetElementType());
            TypeCode code = Type.GetTypeCode(type);
            return type == typeof(string) || type == typeof(bool) || type == typeof(char) || code >= TypeCode.SByte && code <= TypeCode.Decimal;
        }

        /// <summary>ref/out 的字段类型使用元素类型。</summary>
        private static Type FieldType(ParameterInfo parameter)
        {
            return parameter.ParameterType.IsByRef ? parameter.ParameterType.GetElementType() : parameter.ParameterType;
        }

        /// <summary>异步方法按完成后的实际返回类型声明输出。</summary>
        private static Type ResultType(MethodInfo method)
        {
            Type type = method.ReturnType;
            if (type == typeof(Task)) return typeof(void);
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>) ? type.GetGenericArguments()[0] : type;
        }

        /// <summary>稳定签名区分重载并对继承覆盖去重，不使用程序集版本或元数据序号。</summary>
        private static string Signature(MethodInfo method)
        {
            return method.Name + "(" + string.Join(",", method.GetParameters().Select(parameter =>
                (parameter.IsOut ? "out:" : parameter.ParameterType.IsByRef ? "ref:" : "") + TypeName(parameter.ParameterType))) + "):" + TypeName(method.ReturnType);
        }

        /// <summary>类型签名只保留代码名称，泛型参数同样排除程序集版本与部署路径。</summary>
        private static string TypeName(Type type)
        {
            if (type.IsByRef) return TypeName(type.GetElementType()) + "&";
            if (type.IsArray) return TypeName(type.GetElementType()) + "[]";
            if (type.IsGenericType)
                return type.GetGenericTypeDefinition().FullName + "<" + string.Join(",", type.GetGenericArguments().Select(TypeName)) + ">";
            return type.FullName;
        }

        /// <summary>由签名生成可持久化且符合语言 XML 键格式的方法身份。</summary>
        private static string MethodId(MethodInfo method, FlowToolAttribute attribute)
        {
            if (attribute != null && !string.IsNullOrWhiteSpace(attribute.Id))
            {
                System.Xml.XmlConvert.VerifyNCName("action." + attribute.Id);
                return attribute.Id;
            }
            using (SHA256 hash = SHA256.Create())
                return "auto." + method.Name + "." + BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(Signature(method))), 0, 12).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>自动建立类型字段、返回值和本实例调用；可选特性只提供显示与模拟信息。</summary>
        private static FlowMethod Describe(object instance, MethodInfo source)
        {
            FlowToolAttribute attribute = source.GetCustomAttribute<FlowToolAttribute>(true);
            string name = source.Name + "(" + string.Join(", ", source.GetParameters().Where(parameter => parameter.ParameterType != typeof(CancellationToken)).Select(parameter => FieldType(parameter).Name)) + ")";
            string[] title = attribute == null ? new[] { name, name, name } : attribute.Title;
            FlowMethod method = new FlowMethod(MethodId(source, attribute), title[0], title[1], title[2]) { UsesHardware = attribute == null || attribute.UsesHardware };
            foreach (ParameterInfo parameter in source.GetParameters())
            {
                if (parameter.ParameterType == typeof(CancellationToken)) continue;
                if (!parameter.IsOut) method.Inputs.Add(new FlowField(parameter.Name, FieldType(parameter), DefaultValue(parameter)));
                if (parameter.ParameterType.IsByRef) method.Outputs.Add(new FlowField(parameter.Name, FieldType(parameter)));
            }
            if (ResultType(source) != typeof(void)) method.Outputs.Add(new FlowField(ReturnName(source), ResultType(source))
            {
                DisplayName = new[] { "返回值", "Return value", "Valor devuelto" }
            });
            method.Invoke = call => Invoke(instance, source, call);
            if (attribute != null && !string.IsNullOrWhiteSpace(attribute.SimulationMethod))
            {
                MethodInfo simulate = instance.GetType().GetMethod(attribute.SimulationMethod, BindingFlags.Public | BindingFlags.Instance,
                    null, source.GetParameters().Select(parameter => parameter.ParameterType).ToArray(), null);
                if (simulate == null || simulate.ReturnType != source.ReturnType)
                    throw new InvalidOperationException(FlowInheritedText.T("simulationSignature") + " " + source.Name);
                method.Simulate = call => Invoke(instance, simulate, call, source);
            }
            return method;
        }

        /// <summary>默认值使用不变区域；数组要求显式绑定实际数组数据。</summary>
        private static string DefaultValue(ParameterInfo parameter)
        {
            Type type = FieldType(parameter);
            if (parameter.HasDefaultValue && parameter.DefaultValue != null && parameter.DefaultValue != DBNull.Value && parameter.DefaultValue != Missing.Value)
                return Convert.ToString(parameter.DefaultValue, CultureInfo.InvariantCulture);
            if (type == typeof(string) || type.IsArray) return "";
            if (type == typeof(char)) return " ";
            if (type.IsEnum) return Enum.GetNames(type)[0];
            return Convert.ToString(Activator.CreateInstance(type), CultureInfo.InvariantCulture);
        }

        /// <summary>返回值与同名 ref/out 参数分别保存，避免覆盖任一输出。</summary>
        private static string ReturnName(MethodInfo method)
        {
            HashSet<string> outputs = new HashSet<string>(method.GetParameters().Where(parameter => parameter.ParameterType.IsByRef).Select(parameter => parameter.Name));
            string name = "Value";
            while (outputs.Contains(name)) name = "Return" + name;
            return name;
        }

        /// <summary>在实际对象已有租约内执行，协作取消后仍等待底层调用结束才返回。</summary>
        private static async Task<IDictionary<string, object>> Invoke(object instance, MethodInfo method, FlowCall call, MethodInfo contract = null)
        {
            ParameterInfo[] fields = (contract ?? method).GetParameters();
            object[] values = Arguments(method, fields, call);
            call.Token.ThrowIfCancellationRequested();
            object result = await Task.Run(() => { call.Token.ThrowIfCancellationRequested(); return Call(instance, method, values); }).ConfigureAwait(false);
            Task pending = result as Task;
            if (pending != null)
            {
                await pending.ConfigureAwait(false);
                result = ResultType(method) == typeof(void) ? null : method.ReturnType.GetProperty("Result").GetValue(pending);
            }
            call.Token.ThrowIfCancellationRequested();
            Dictionary<string, object> output = new Dictionary<string, object>();
            for (int index = 0; index < fields.Length; index++)
                if (fields[index].ParameterType.IsByRef) output.Add(fields[index].Name, values[index]);
            if (ResultType(method) != typeof(void)) output.Add(ReturnName(contract ?? method), result);
            return output;
        }

        /// <summary>按签名排列实际输入，取消信号由运行器注入，out 参数不要求界面输入。</summary>
        private static object[] Arguments(MethodInfo method, ParameterInfo[] fields, FlowCall call)
        {
            ParameterInfo[] parameters = method.GetParameters();
            object[] values = new object[parameters.Length];
            for (int index = 0; index < parameters.Length; index++)
            {
                ParameterInfo parameter = parameters[index];
                if (parameter.ParameterType == typeof(CancellationToken)) { values[index] = call.Token; continue; }
                if (!parameter.IsOut) { values[index] = call.Inputs[fields[index].Name]; continue; }
                Type type = FieldType(parameter);
                values[index] = type.IsValueType ? Activator.CreateInstance(type) : null;
            }
            return values;
        }

        /// <summary>反射执行保留真实业务异常，不将 TargetInvocationException 作为报警内容。</summary>
        private static object Call(object instance, MethodInfo method, object[] values)
        {
            try { return method.Invoke(instance, values); }
            catch (TargetInvocationException error)
            {
                if (error.InnerException == null) throw;
                ExceptionDispatchInfo.Capture(error.InnerException).Throw();
                throw;
            }
        }
    }

    /// <summary>自动方法接入的三语固定提示。</summary>
    internal static class FlowInheritedText
    {
        /// <summary>通过框架语言入口报告模拟签名错误。</summary>
        internal static string T(string key)
        {
            return MiddleLayer.LangMsg("EquipmentFlow", "autoTool_" + key,
                "模拟方法的参数与返回类型必须和实际方法一致：", "The simulation method must match the real method's parameters and return type:",
                "El método de simulación debe coincidir con los parámetros y el tipo de retorno del método real:");
        }
    }
}
