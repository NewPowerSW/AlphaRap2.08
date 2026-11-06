using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NPSDK
{
    /// <summary>由实际对象声明可用于流程编程的方法；浏览目录不执行对象操作。</summary>
    public interface IFlowObject
    {
        /// <summary>读取实例当前可绑定的方法元数据；includeDraft 仅用于编辑和主保存校验。</summary>
        IEnumerable<FlowMethod> GetFlowMethods(bool includeDraft = false);
    }

    /// <summary>方法参数或结果的类型与固定选择项；Name 是不随语言变化的业务键。</summary>
    public sealed class FlowField
    {
        public string Name;
        public Type Type;
        public string DefaultValue = "";
        /// <summary>字段的中文、英文、西班牙文显示名称；缺省时显示稳定业务键。</summary>
        public string[] DisplayName;
        /// <summary>字段的中文、英文、西班牙文操作说明；不参与运行输入或序列化。</summary>
        public string[] Help;
        /// <summary>明确由对象定义的单位；空值表示不额外显示单位。</summary>
        public string Unit;
        /// <summary>使用默认值时可收起的高级参数，已绑定或已修改的参数仍应显示。</summary>
        public bool Advanced;
        /// <summary>代码模块中该整数输入对应的轴角色；用于选择该轴绑定表的零基点位索引。</summary>
        public string ResourceSlot;
        /// <summary>只允许从此方法的已绑定节点选择输出来源；空值表示不限制方法。</summary>
        public string OutputSourceMethod;
        /// <summary>来源节点中需要绑定的稳定输出键；与来源方法限制配合使用。</summary>
        public string OutputSourceField;
        public readonly List<FlowOption> Options = new List<FlowOption>();

        /// <summary>创建具有明确类型和编辑初值的字段。</summary>
        public FlowField(string name, Type type, string defaultValue = "")
        {
            if (type == null) throw new ArgumentNullException("type");
            Name = name; Type = type; DefaultValue = defaultValue;
            if (type.IsEnum)
                foreach (string item in Enum.GetNames(type)) Options.Add(new FlowOption { Value = item, Label = item });
        }

        /// <summary>按不变区域转换输入；整数不允许截断，浮点值必须有限。</summary>
        public object ConvertValue(object value)
        {
            if (Type == typeof(object)) return value;
            if (value == null || value == DBNull.Value) throw new InvalidOperationException("Missing value: " + Name);
            if (Type == typeof(string)) return Convert.ToString(value, CultureInfo.InvariantCulture);
            if (Type.IsEnum)
            {
                object selected = Enum.Parse(Type, Convert.ToString(value, CultureInfo.InvariantCulture), false);
                if (!Enum.IsDefined(Type, selected)) throw new InvalidOperationException("Invalid enum value: " + Name);
                return selected;
            }
            if (Type == typeof(bool))
            {
                string text = Convert.ToString(value, CultureInfo.InvariantCulture);
                if (text == "1") return true;
                if (text == "0") return false;
                return Convert.ToBoolean(value, CultureInfo.InvariantCulture);
            }
            TypeCode code = System.Type.GetTypeCode(Type);
            if (code >= TypeCode.SByte && code <= TypeCode.UInt64)
            {
                decimal number = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                if (number != decimal.Truncate(number)) throw new InvalidOperationException("An integer is required: " + Name);
            }
            if (Type == typeof(double) || Type == typeof(float))
            {
                double number = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                if (double.IsNaN(number) || double.IsInfinity(number)) throw new InvalidOperationException("A finite value is required: " + Name);
            }
            if (Type.IsInstanceOfType(value)) return value;
            object converted = Convert.ChangeType(value, Type, CultureInfo.InvariantCulture);
            if (Type == typeof(float) && (float.IsNaN((float)converted) || float.IsInfinity((float)converted))) throw new InvalidOperationException("A finite value is required: " + Name);
            return converted;
        }
    }

    public sealed class FlowOption
    {
        public string Value;
        public string Label;
        public string Group;
    }

    /// <summary>方法元数据与绑定到实际实例的调用，编辑器和运行时共用同一声明。</summary>
    public sealed class FlowMethod
    {
        public string Id;
        public string[] Title;
        public readonly List<FlowField> Inputs = new List<FlowField>();
        public readonly List<FlowField> Outputs = new List<FlowField>();
        public bool UsesHardware;
        /// <summary>使用原生 FlowChartRun 返回值选择出口；IDLE 由运行器重复调用同一动作。</summary>
        public bool UsesFlowResult;
        public Action<IDictionary<string, object>> ValidateInputs;
        public Func<FlowCall, Task<IDictionary<string, object>>> Invoke;
        public Func<FlowCall, Task<IDictionary<string, object>>> Simulate;

        /// <summary>创建方法定义，Id 用于持久化，标题由宿主按现有语言机制翻译。</summary>
        public FlowMethod(string id, string zh, string en, string es)
        {
            Id = id; Title = new[] { zh, en, es };
        }

        /// <summary>校验目录身份和字段唯一性，拒绝缺少实现的声明。</summary>
        public void ValidateDefinition()
        {
            if (string.IsNullOrWhiteSpace(Id) || Title == null || Title.Length != 3 || Invoke == null)
                throw new InvalidOperationException("Invalid flow method definition: " + Id);
            foreach (List<FlowField> fields in new[] { Inputs, Outputs })
                if (fields.Any(field => string.IsNullOrWhiteSpace(field.Name) || field.Type == null) || fields.GroupBy(field => field.Name).Any(group => group.Count() > 1))
                    throw new InvalidOperationException("Invalid or duplicate flow fields: " + Id);
        }
    }

    /// <summary>单次调用的输入、产品上下文及取消信号，不存入流程工程文件。</summary>
    public sealed class FlowCall
    {
        public FlowStep Step;
        public FlowExecutionContext Context;
        /// <summary>本次对象方法调用的独立身份；未提供时沿用节点执行身份。</summary>
        public string InvocationId;
        public IDictionary<string, object> Inputs;
        public CancellationToken Token;
        public bool Simulation;
        public Func<string, CancellationToken, Task<string>> RequestInput;
        /// <summary>按节点声明的角色调用已绑定资源；宿主负责身份、类型、租约和模拟校验。</summary>
        public Func<string, string, IDictionary<string, object>, CancellationToken, Task<IDictionary<string, object>>> InvokeResourceAsync;

        /// <summary>读取经过字段类型校验的具名输入。</summary>
        public T Get<T>(string name) { return (T)Inputs[name]; }

        /// <summary>创建单字段输出，便于对象直接返回自己的结果。</summary>
        public static IDictionary<string, object> Result(string name, object value)
        {
            return new Dictionary<string, object> { { name, value } };
        }
    }
}
