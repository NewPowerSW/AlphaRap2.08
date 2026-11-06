using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace NPSDK
{
    /// <summary>可复用代码模块的稳定身份、工具箱分组和三语名称。</summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class FlowCodeModuleAttribute : Attribute
    {
        public string Id { get; private set; }
        public string Category { get; private set; }
        public string[] Title { get; private set; }
        public int Version { get; set; } = 1;
        public string Exits { get; set; } = "NEXT";

        /// <summary>声明模块元数据；不构造模块实例或连接设备。</summary>
        public FlowCodeModuleAttribute(string id, string category, string zh, string en, string es)
        { Id = id; Category = category; Title = new[] { zh, en, es }; }
    }

    /// <summary>类型化输入或输出的三语标题、编辑初值和单位。</summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class FlowCodeFieldAttribute : Attribute
    {
        public string[] Title { get; private set; }
        public string DefaultValue { get; set; }
        public string Unit { get; set; }
        public bool Advanced { get; set; }
        public string ResourceSlot { get; set; }

        /// <summary>声明字段标题；持久业务键沿用成员名称。</summary>
        public FlowCodeFieldAttribute(string zh, string en, string es) { Title = new[] { zh, en, es }; }
    }

    /// <summary>代码模块所需的真实资源角色；能力使用稳定轴、输入、输出或视觉类别。</summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class FlowCodeResourceAttribute : Attribute
    {
        public string Name { get; private set; }
        public string Capability { get; private set; }
        public string[] Title { get; private set; }
        public bool Required { get; set; } = true;

        /// <summary>声明可由应用工程师绑定的角色，不保存任何硬件实例。</summary>
        public FlowCodeResourceAttribute(string name, string capability, string zh, string en, string es)
        { Name = name; Capability = capability; Title = new[] { zh, en, es }; }
    }

    /// <summary>资源内部的业务端口，由节点映射到真实 VPP 输入或输出。</summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class FlowCodeResourceFieldAttribute : Attribute
    {
        public string Slot { get; private set; }
        public string Name { get; private set; }
        public Type Type { get; private set; }
        public string[] Title { get; private set; }
        public string Direction { get; set; } = "Output";

        /// <summary>声明一个具名、类型固定的资源端口映射要求。</summary>
        public FlowCodeResourceFieldAttribute(string slot, string name, Type type, string zh, string en, string es)
        { Slot = slot; Name = name; Type = type; Title = new[] { zh, en, es }; }
    }

    /// <summary>工具箱显示的角色要求，运行时仍通过节点保存的稳定身份解析。</summary>
    public sealed class FlowCodeResourceSlot
    {
        public string Name;
        public string Capability;
        public string[] Title;
        public bool Required = true;
        public List<FlowCodeResourceFieldDefinition> Fields = new List<FlowCodeResourceFieldDefinition>();
    }

    /// <summary>业务端口的类型与方向，不包含当前配方的端口实例。</summary>
    public sealed class FlowCodeResourceFieldDefinition
    {
        public string Name;
        public string Direction = "Output";
        public Type Type;
        public string[] Title;
    }

    /// <summary>代码模块目录定义；每次调用才创建独立业务执行实例。</summary>
    public sealed class FlowCodeDefinition
    {
        public string Id { get; internal set; }
        public string Category { get; internal set; }
        public string[] Title { get; internal set; }
        public int Version { get; internal set; }
        public Type ModuleType { get; internal set; }
        public Type InputType { get; internal set; }
        public Type OutputType { get; internal set; }
        public readonly List<FlowField> Inputs = new List<FlowField>();
        public readonly List<FlowField> Outputs = new List<FlowField>();
        public readonly List<FlowCodeResourceSlot> ResourceSlots = new List<FlowCodeResourceSlot>();
        public readonly List<string> Exits = new List<string>();

        /// <summary>为现有 Action 编辑与校验管线提供同一份方法元数据。</summary>
        public FlowMethod CreateMethod()
        {
            FlowMethod method = new FlowMethod("code.execute", Title[0], Title[1], Title[2]);
            method.Inputs.AddRange(Inputs);
            method.Outputs.AddRange(Outputs);
            method.Invoke = InvokeOutputsAsync;
            method.Simulate = method.Invoke;
            return method;
        }

        /// <summary>验证节点的角色、契约版本及全部已声明业务端口映射。</summary>
        public void ValidateBindings(FlowStep step)
        {
            if (step == null) throw new ArgumentNullException("step");
            if (step.ExecutionMode != "Action" || step.CodeModuleId != Id || step.CodeModuleVersion != Version)
                throw new InvalidOperationException("Code module contract changed: " + Id);
            List<FlowBranch> exits = FlowProgramFile.BranchesOf(step);
            if (Exits.Any(code => exits.All(branch => branch.Code != code)) || exits.Any(branch => !Exits.Contains(branch.Code)))
                throw new InvalidOperationException("Configure exactly the declared code module exits: " + Id);
            if (step.Conditions.Count > 0 || exits.Any(branch => branch.Conditions.Count > 0 || branch.AnyCondition))
                throw new InvalidOperationException("Code module exits cannot contain additional conditions: " + Id);
            if (step.ResourceBindings == null) throw new InvalidOperationException("Missing resource bindings: " + Id);
            if (step.ResourceBindings.Any(binding => binding == null || ResourceSlots.All(slot => slot.Name != binding.Slot))
                || step.ResourceBindings.GroupBy(binding => binding.Slot, StringComparer.Ordinal).Any(group => group.Count() > 1))
                throw new InvalidOperationException("Unknown or duplicate resource role: " + Id);
            foreach (FlowCodeResourceSlot slot in ResourceSlots) ValidateSlot(slot, step.ResourceBindings.SingleOrDefault(binding => binding.Slot == slot.Name));
        }

        /// <summary>创建一个执行实例，沿用节点的取消、模拟和受控资源访问范围。</summary>
        public async Task<FlowActionOutcome> InvokeAsync(FlowCall call)
        {
            if (call == null) throw new ArgumentNullException("call");
            ValidateBindings(call.Step);
            call.Token.ThrowIfCancellationRequested();
            FlowCodeNode node = (FlowCodeNode)Activator.CreateInstance(ModuleType);
            using (FlowCodeContext context = new FlowCodeContext(call, this))
            {
                try { return await node.RunAsync(context, call.Inputs, this).ConfigureAwait(false); }
                finally { IDisposable disposable = node as IDisposable; if (disposable != null) disposable.Dispose(); }
            }
        }

        /// <summary>保留普通方法签名供元数据工具使用；运行器必须走显式出口接口。</summary>
        private async Task<IDictionary<string, object>> InvokeOutputsAsync(FlowCall call)
        { return (await InvokeAsync(call).ConfigureAwait(false)).Outputs; }

        /// <summary>校验单个资源角色的必要性与声明端口，缺失绑定不自动替换。</summary>
        private static void ValidateSlot(FlowCodeResourceSlot slot, FlowResourceBinding binding)
        {
            if (binding == null || string.IsNullOrWhiteSpace(binding.Target))
            {
                if (slot.Required) throw new InvalidOperationException("Bind resource role: " + slot.Name);
                return;
            }
            if (binding.Fields == null) throw new InvalidOperationException("Missing field mappings: " + slot.Name);
            foreach (FlowCodeResourceFieldDefinition field in slot.Fields)
                if (binding.Fields.Count(map => map != null && map.Name == field.Name && map.Direction == field.Direction && !string.IsNullOrWhiteSpace(map.Field)) != 1)
                    throw new InvalidOperationException("Bind resource field: " + slot.Name + "/" + field.Name);
            if (binding.Fields.Any(map => map == null || slot.Fields.All(field => field.Name != map.Name || field.Direction != map.Direction)))
                throw new InvalidOperationException("Unknown resource field mapping: " + slot.Name);
        }
    }

    /// <summary>只读取类型定义生成代码模块目录，不触发模块构造器和硬件访问。</summary>
    public static class FlowCodeCatalog
    {
        /// <summary>校验一个具体代码模块并读取其端口、角色及三语元数据。</summary>
        public static FlowCodeDefinition Describe(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            Type contract = FindContract(type);
            if (!type.IsClass || type.IsAbstract || type.ContainsGenericParameters || !type.IsVisible || type.GetConstructor(Type.EmptyTypes) == null || contract == null)
                throw new InvalidOperationException("A public concrete FlowCodeNode with a parameterless constructor is required: " + type.FullName);
            FlowCodeModuleAttribute module = (FlowCodeModuleAttribute)Attribute.GetCustomAttribute(type, typeof(FlowCodeModuleAttribute), false);
            if (module == null || string.IsNullOrWhiteSpace(module.Id) || string.IsNullOrWhiteSpace(module.Category) || module.Version < 1)
                throw new InvalidOperationException("Declare a stable code module identity, category and version: " + type.FullName);
            CheckTitle(module.Title, module.Id);
            Type[] arguments = contract.GetGenericArguments();
            FlowCodeDefinition definition = new FlowCodeDefinition { Id = module.Id, Category = module.Category, Title = module.Title,
                Version = module.Version, ModuleType = type, InputType = arguments[0], OutputType = arguments[1] };
            definition.Inputs.AddRange(ReadFields(arguments[0]));
            definition.Outputs.AddRange(ReadFields(arguments[1]));
            ReadExits(definition, module.Exits);
            ReadResources(definition, type);
            ValidatePointFields(definition);
            ValidateImplementation(type, contract);
            return definition;
        }

        /// <summary>定位泛型业务基类，拒绝只继承非泛型基础定义的类型。</summary>
        private static Type FindContract(Type type)
        {
            for (Type current = type; current != null; current = current.BaseType)
                if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(FlowCodeNode<,>)) return current;
            return null;
        }

        /// <summary>确认业务类型实现同步或异步的执行方法，不接受空壳模块。</summary>
        private static void ValidateImplementation(Type type, Type contract)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            if (type.GetMethod("Execute", flags).DeclaringType == contract && type.GetMethod("ExecuteAsync", flags).DeclaringType == contract)
                throw new InvalidOperationException("Override Execute or ExecuteAsync: " + type.FullName);
        }

        /// <summary>只将公开可读写的简单值成员作为端口，不实例化参数对象。</summary>
        internal static List<MemberInfo> Members(Type type)
        {
            List<MemberInfo> result = new List<MemberInfo>();
            result.AddRange(type.GetFields(BindingFlags.Public | BindingFlags.Instance).Where(field => !field.IsInitOnly));
            result.AddRange(type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(property => property.GetIndexParameters().Length == 0
                && property.GetGetMethod() != null && property.GetSetMethod() != null));
            if (result.GroupBy(member => member.Name, StringComparer.Ordinal).Any(group => group.Count() > 1))
                throw new InvalidOperationException("Duplicate code port: " + type.FullName);
            return result.OrderBy(member => member.MetadataToken).ToList();
        }

        /// <summary>读取端口类型及编辑初值，拒绝不能持久化的任意对象图。</summary>
        private static IEnumerable<FlowField> ReadFields(Type type)
        {
            foreach (MemberInfo member in Members(type))
            {
                Type valueType = MemberType(member);
                CheckValueType(valueType, member.Name);
                FlowCodeFieldAttribute label = (FlowCodeFieldAttribute)Attribute.GetCustomAttribute(member, typeof(FlowCodeFieldAttribute), true);
                string[] title = label == null ? new[] { member.Name, member.Name, member.Name } : label.Title;
                CheckTitle(title, member.Name);
                string initial = label != null && label.DefaultValue != null ? label.DefaultValue : DefaultValue(valueType);
                FlowField field = new FlowField(member.Name, valueType, initial) { DisplayName = title,
                    Unit = label == null ? null : label.Unit, Advanced = label != null && label.Advanced,
                    ResourceSlot = label == null ? null : label.ResourceSlot };
                field.ConvertValue(initial);
                yield return field;
            }
        }

        /// <summary>取得一个公开字段或属性的值类型。</summary>
        internal static Type MemberType(MemberInfo member)
        { FieldInfo field = member as FieldInfo; return field == null ? ((PropertyInfo)member).PropertyType : field.FieldType; }

        /// <summary>简单端口仅允许有限数值、布尔、文本和枚举。</summary>
        private static void CheckValueType(Type type, string name)
        {
            if (type == null) throw new InvalidOperationException("Code port type is missing: " + name);
            TypeCode code = Type.GetTypeCode(type);
            if (type.IsEnum || type == typeof(bool) || type == typeof(string) || (code >= TypeCode.SByte && code <= TypeCode.Decimal)) return;
            throw new InvalidOperationException("Unsupported code port type: " + name + " / " + type.FullName);
        }

        /// <summary>不构造业务参数对象，使用类型可表示的安全编辑初值。</summary>
        private static string DefaultValue(Type type)
        {
            if (type == typeof(string)) return "";
            if (type == typeof(bool)) return "False";
            if (type.IsEnum)
            {
                string[] names = Enum.GetNames(type);
                if (names.Length == 0) throw new InvalidOperationException("An enum port must declare a value: " + type.FullName);
                return names[0];
            }
            return Convert.ToString(0, CultureInfo.InvariantCulture);
        }

        /// <summary>检查并登记模块可能返回的业务出口，End 不需要配置去向。</summary>
        private static void ReadExits(FlowCodeDefinition definition, string source)
        {
            string[] exits = (source ?? "").Split(',').Select(value => value.Trim()).ToArray();
            if (exits.Length == 0 || exits.Any(exit => exit != "NEXT" && exit != "CASE1" && exit != "CASE2" && exit != "CASE3")
                || exits.Distinct(StringComparer.Ordinal).Count() != exits.Length || !exits.Contains("NEXT"))
                throw new InvalidOperationException("Declare unique NEXT/CASE1/CASE2/CASE3 exits including NEXT: " + definition.Id);
            definition.Exits.AddRange(new[] { "NEXT", "CASE1", "CASE2", "CASE3" }.Where(exits.Contains));
        }

        /// <summary>读取资源角色和端口映射声明，拒绝歧义名称和方向。</summary>
        private static void ReadResources(FlowCodeDefinition definition, Type type)
        {
            foreach (FlowCodeResourceAttribute resource in Attribute.GetCustomAttributes(type, typeof(FlowCodeResourceAttribute), false))
            {
                if (string.IsNullOrWhiteSpace(resource.Name) || string.IsNullOrWhiteSpace(resource.Capability) || definition.ResourceSlots.Any(slot => slot.Name == resource.Name))
                    throw new InvalidOperationException("Invalid or duplicate resource role: " + definition.Id);
                CheckTitle(resource.Title, resource.Name);
                definition.ResourceSlots.Add(new FlowCodeResourceSlot { Name = resource.Name, Capability = resource.Capability.Trim().ToLowerInvariant(), Title = resource.Title, Required = resource.Required });
            }
            foreach (FlowCodeResourceFieldAttribute field in Attribute.GetCustomAttributes(type, typeof(FlowCodeResourceFieldAttribute), false))
            {
                FlowCodeResourceSlot slot = definition.ResourceSlots.SingleOrDefault(item => item.Name == field.Slot);
                if (slot == null || string.IsNullOrWhiteSpace(field.Name) || (field.Direction != "Input" && field.Direction != "Output")
                    || slot.Fields.Any(item => item.Name == field.Name && item.Direction == field.Direction))
                    throw new InvalidOperationException("Invalid resource port: " + field.Slot + "/" + field.Name);
                CheckValueType(field.Type, field.Name);
                CheckTitle(field.Title, field.Name);
                slot.Fields.Add(new FlowCodeResourceFieldDefinition { Name = field.Name, Direction = field.Direction, Type = field.Type, Title = field.Title });
            }
        }

        /// <summary>点位选择仅适用于明确引用轴角色的 Int32 输入，输出和其他角色不能声明此元数据。</summary>
        private static void ValidatePointFields(FlowCodeDefinition definition)
        {
            if (definition.Outputs.Any(field => field.ResourceSlot != null))
                throw new InvalidOperationException("ResourceSlot is only valid on input ports: " + definition.Id);
            foreach (FlowField field in definition.Inputs.Where(item => item.ResourceSlot != null))
            {
                FlowCodeResourceSlot slot = definition.ResourceSlots.SingleOrDefault(item => item.Name == field.ResourceSlot);
                if (field.Type != typeof(int) || slot == null || slot.Capability != "axis")
                    throw new InvalidOperationException("ResourceSlot requires an Int32 input and an existing axis role: " + definition.Id + "/" + field.Name);
            }
        }

        /// <summary>模块元数据必须同时提供三种语言，不使用当前界面语言覆盖业务键。</summary>
        private static void CheckTitle(string[] title, string name)
        {
            if (title == null || title.Length != 3 || title.Any(string.IsNullOrWhiteSpace))
                throw new InvalidOperationException("Chinese, English and Spanish titles are required: " + name);
        }
    }
}
