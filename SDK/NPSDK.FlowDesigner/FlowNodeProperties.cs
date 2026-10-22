using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace NPSDK.Design
{
    public sealed class FlowChoiceEnumConverter : TypeConverter
    {
        /// <summary>枚举下拉保持值身份，界面显示宿主语言。</summary>
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <summary>拒绝未定义判据和分支。</summary>
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <summary>返回对应属性的所有枚举值。</summary>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(Enum.GetValues(context.PropertyDescriptor.PropertyType));
        }

        /// <summary>支持从显示文本还原值。</summary>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type source)
        {
            return source == typeof(string) || base.CanConvertFrom(context, source);
        }

        /// <summary>支持翻译显示。</summary>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destination)
        {
            return destination == typeof(string) || base.CanConvertTo(context, destination);
        }

        /// <summary>只翻译显示，不修改 XML 枚举代码。</summary>
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destination)
        {
            var model = context == null ? null : context.Instance as FlowNodeProperties;
            return destination == typeof(string) && model != null ? model.DisplayEnum(value) : base.ConvertTo(context, culture, value, destination);
        }

        /// <summary>解析下拉显示值或稳定枚举代码。</summary>
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            var model = context == null ? null : context.Instance as FlowNodeProperties;
            if (model == null || !(value is string))
                return base.ConvertFrom(context, culture, value);
            foreach (object choice in Enum.GetValues(context.PropertyDescriptor.PropertyType))
                if ((string)value == model.DisplayEnum(choice) || (string)value == choice.ToString())
                    return choice;
            throw new FormatException(Convert.ToString(value));
        }
    }

    public sealed class FlowPropertyChoices : StringConverter
    {
        /// <summary>为方法、实例、节点、判据与报警提供受控下拉选项。</summary>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            var model = context.Instance as FlowNodeProperties;
            return new StandardValuesCollection(model == null ? new string[0] : model.Choices(context.PropertyDescriptor.Name));
        }

        /// <summary>属性使用选项列表。</summary>
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <summary>方法与实例不允许输入未登记的值。</summary>
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }
    }

    public sealed class FlowNodeProperties : ICustomTypeDescriptor
    {
        public readonly FlowDesignNode Node;
        private readonly FlowDesignDocument document;
        private readonly FlowDesignerEnvironment environment;
        /// <summary>为单个 SDK 节点配置提供分组属性。</summary>
        public FlowNodeProperties(FlowDesignNode node, FlowDesignDocument document, FlowDesignerEnvironment environment)
        {
            Node = node;
            this.document = document;
            this.environment = environment;
        }

        [Category("Flow")]
        public bool CanPause
        {
            get
            {
                return Node.CanPause;
            }

            set
            {
                Node.CanPause = value;
            }
        }

        [Category("Identity"), TypeConverter(typeof(FlowPropertyChoices)), RefreshProperties(RefreshProperties.All)]
        public string MethodName
        {
            get
            {
                return Node.MethodName;
            }

            set
            {
                if (value == Node.MethodName)
                    return;
                Node.MethodName = value;
                Node.Target = environment.Registry.Bindings.First(b => b.MethodName == value).TargetId;
                ResetParameters();
                Node.ConditionSource = "";
                Node.Comparison = FlowComparison.Always;
            }
        }

        [Category("Identity"), ReadOnly(true)]
        public string Name
        {
            get
            {
                return Node.Id;
            }
        }

        [Category("Identity")]
        public string Text
        {
            get
            {
                return Node.Text;
            }

            set
            {
                Node.Text = value;
            }
        }

        [Category("Identity"), TypeConverter(typeof(FlowPropertyChoices)), RefreshProperties(RefreshProperties.All)]
        public string Instance
        {
            get
            {
                return Node.Target;
            }

            set
            {
                Node.Target = value;
                ResetParameters();
            }
        }

        [Category("Links"), TypeConverter(typeof(FlowPropertyChoices))]
        public string Next
        {
            get
            {
                return Node.Next;
            }

            set
            {
                Node.Next = value;
            }
        }

        [Category("Links"), TypeConverter(typeof(FlowPropertyChoices))]
        public string Case1
        {
            get
            {
                return Node.Case1;
            }

            set
            {
                Node.Case1 = value;
            }
        }

        [Category("Links"), TypeConverter(typeof(FlowPropertyChoices))]
        public string Case2
        {
            get
            {
                return Node.Case2;
            }

            set
            {
                Node.Case2 = value;
            }
        }

        [Category("Links"), TypeConverter(typeof(FlowPropertyChoices))]
        public string Case3
        {
            get
            {
                return Node.Case3;
            }

            set
            {
                Node.Case3 = value;
            }
        }

        [Category("Condition"), TypeConverter(typeof(FlowPropertyChoices))]
        public string Source
        {
            get
            {
                return Node.ConditionSource;
            }

            set
            {
                Node.ConditionSource = value;
            }
        }

        [Category("Condition"), TypeConverter(typeof(FlowChoiceEnumConverter))]
        public FlowComparison Comparison
        {
            get
            {
                return Node.Comparison;
            }

            set
            {
                Node.Comparison = value;
            }
        }

        [Category("Condition")]
        public string Expected
        {
            get
            {
                return Node.Expected;
            }

            set
            {
                Node.Expected = value;
            }
        }

        [Category("Condition"), TypeConverter(typeof(FlowChoiceEnumConverter))]
        public FlowUnmetAction WhenFalse
        {
            get
            {
                return Node.WhenFalse;
            }

            set
            {
                Node.WhenFalse = value;
            }
        }

        [Category("Timing")]
        public int Delay
        {
            get
            {
                return Node.Delay;
            }

            set
            {
                Node.Delay = value;
            }
        }

        [Category("Timing")]
        public int Timeout
        {
            get
            {
                return Node.Timeout;
            }

            set
            {
                Node.Timeout = value;
            }
        }

        [Category("Alarm"), TypeConverter(typeof(FlowPropertyChoices))]
        public string AlarmCode
        {
            get
            {
                return Node.AlarmCode;
            }

            set
            {
                Node.AlarmCode = value;
            }
        }

        [Category("Alarm")]
        public string AlarmContent
        {
            get
            {
                return Node.AlarmContent;
            }

            set
            {
                Node.AlarmContent = value;
            }
        }

        /// <summary>提供枚举的语言展示层。</summary>
        public string DisplayEnum(object value)
        {
            return environment.T("Enum_" + value);
        }

        /// <summary>按所选实例重建参数底稿，兼容字段保留既有绑定。</summary>
        private void ResetParameters()
        {
            var binding = environment.Registry.Find(Node.MethodName, Node.Target);
            if (binding == null)
                return;
            var old = Node.Parameters;
            Node.Parameters = binding.Parameters.Select(p => old.FirstOrDefault(o => o.Name == p.Name && o.Type == p.Type) ?? p.Copy()).ToList();
        }

        /// <summary>生成下拉字段的稳定标识集合。</summary>
        public string[] Choices(string name)
        {
            if (name == "MethodName")
                return environment.Registry.Bindings.Select(b => b.MethodName).Distinct().ToArray();
            if (name == "Instance")
                return environment.Registry.Bindings.Where(b => b.MethodName == Node.MethodName).Select(b => b.TargetId).ToArray();
            if (name == "AlarmCode")
                return new[]
                {
                    ""
                }.Concat(environment.Registry.AlarmCodes).ToArray();
            if (name != "Source")
                return new[]
                {
                    ""
                }.Concat(document.Nodes.Select(n => n.Id)).ToArray();
            var binding = environment.Registry.Find(Node.MethodName, Node.Target);
            var fields = binding == null ? new string[0] : binding.Outputs;
            return new[]
            {
                ""
            }.Concat(fields).Concat(document.Nodes.SelectMany(n =>
            {
                var b = environment.Registry.Find(n.MethodName, n.Target);
                return b == null ? new string[0] : b.Outputs.Select(o => n.Id + "." + o).ToArray();
            })).ToArray();
        }

        /// <summary>保持 PropertyGrid 按显示语言分组，属性身份不变。</summary>
        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return new PropertyDescriptorCollection(TypeDescriptor.GetProperties(GetType(), attributes).Cast<PropertyDescriptor>().Select(p => new LocalProperty(p, environment)).ToArray());
        }

        /// <summary>返回全部属性描述。</summary>
        public PropertyDescriptorCollection GetProperties()
        {
            return GetProperties(new Attribute[0]);
        }

        /// <summary>返回编辑对象。</summary>
        public object GetPropertyOwner(PropertyDescriptor property)
        {
            return this;
        }

        /// <summary>返回标准特性。</summary>
        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(GetType());
        }

        /// <summary>返回类型名。</summary>
        public string GetClassName()
        {
            return GetType().FullName;
        }

        /// <summary>返回节点身份。</summary>
        public string GetComponentName()
        {
            return Node.Id;
        }

        /// <summary>无自定义转换器。</summary>
        public TypeConverter GetConverter()
        {
            return null;
        }

        /// <summary>无默认事件。</summary>
        public EventDescriptor GetDefaultEvent()
        {
            return null;
        }

        /// <summary>无默认属性。</summary>
        public PropertyDescriptor GetDefaultProperty()
        {
            return null;
        }

        /// <summary>无额外编辑器。</summary>
        public object GetEditor(Type editorBaseType)
        {
            return null;
        }

        /// <summary>返回空事件集合。</summary>
        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return EventDescriptorCollection.Empty;
        }

        /// <summary>返回空事件集合。</summary>
        public EventDescriptorCollection GetEvents()
        {
            return EventDescriptorCollection.Empty;
        }

        private sealed class LocalProperty : PropertyDescriptor
        {
            private readonly PropertyDescriptor inner;
            private readonly FlowDesignerEnvironment environment;
            /// <summary>装饰标准属性的显示文本。</summary>
            public LocalProperty(PropertyDescriptor inner, FlowDesignerEnvironment environment) : base(inner)
            {
                this.inner = inner;
                this.environment = environment;
            }

            public override string DisplayName
            {
                get
                {
                    return environment.T("Property_" + Name);
                }
            }

            public override string Category
            {
                get
                {
                    return environment.T("Group_" + inner.Category);
                }
            }

            public override string Description
            {
                get
                {
                    return environment.T("Help_" + Name);
                }
            }

            public override Type ComponentType
            {
                get
                {
                    return inner.ComponentType;
                }
            }

            public override bool IsReadOnly
            {
                get
                {
                    return inner.IsReadOnly;
                }
            }

            public override Type PropertyType
            {
                get
                {
                    return inner.PropertyType;
                }
            }

            public override TypeConverter Converter
            {
                get
                {
                    return inner.Converter;
                }
            }

            /// <summary>读取原属性。</summary>
            public override object GetValue(object component)
            {
                return inner.GetValue(component);
            }

            /// <summary>写入原属性。</summary>
            public override void SetValue(object component, object value)
            {
                inner.SetValue(component, value);
            }

            /// <summary>判断是否支持重置。</summary>
            public override bool CanResetValue(object component)
            {
                return inner.CanResetValue(component);
            }

            /// <summary>重置原属性。</summary>
            public override void ResetValue(object component)
            {
                inner.ResetValue(component);
            }

            /// <summary>保持原序列化规则。</summary>
            public override bool ShouldSerializeValue(object component)
            {
                return inner.ShouldSerializeValue(component);
            }
        }
    }
}
