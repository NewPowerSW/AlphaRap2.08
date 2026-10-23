using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    public partial class FlowConditionRow : UserControl
    {
        private sealed class Operand
        {
            internal string Source, Reference, Label;
            internal Type ValueType;
            internal bool Advanced;
            /// <summary>显示业务名称与来源，稳定引用保存在独立字段。</summary>
            public override string ToString() { return Label; }
        }
        private static readonly string[] Operators = { "==","!=",">",">=","<","<=","Contains","Empty" };
        private static readonly string[] OperatorTexts = { "equals","notEquals","greater","greaterEqual","less","lessEqual","contains","empty" };
        private List<Operand> allChoices = new List<Operand>();
        private bool filling;

        /// <summary>建立可在设计器预览的条件行。</summary>
        public FlowConditionRow()
        {
            InitializeComponent();
            comparisonValue.SelectedIndexChanged += (sender,args) => rightValue.Enabled = Convert.ToString(comparisonValue.SelectedValue) != "Empty";
            leftValue.SelectedIndexChanged += LeftChanged;
            leftValue.TextChanged += LeftChanged;
        }

        /// <summary>读取当前行，字段选择保留来源和身份，直接输入的文本作为固定值。</summary>
        public FlowCondition ReadCondition()
        {
            string comparison = Convert.ToString(comparisonValue.SelectedValue);
            return new FlowCondition { Left = Read(leftValue), Operator = comparison, Right = comparison == "Empty" ? new FlowInput { Reference = "" } : Read(rightValue) };
        }

        /// <summary>绑定条件副本并刷新可选字段，不触发硬件或保存。</summary>
        internal void Bind(FlowCondition condition, FlowActions catalog, FlowMethod method)
        {
            filling = true;
            try
            {
                allChoices = Choices(catalog,method);
                Fill(leftValue,condition.Left,allChoices.Where(choice => !choice.Advanced).ToList());
                Fill(rightValue,condition.Right,RightChoices());
                List<FlowChoice> comparisons = Operators.Select((value,index) => new FlowChoice { Id = value, Label = FlowText.T(OperatorTexts[index]) }).ToList();
                if (!Operators.Contains(condition.Operator)) comparisons.Add(new FlowChoice { Id = condition.Operator, Label = condition.Operator });
                comparisonValue.DisplayMember = "Label"; comparisonValue.ValueMember = "Id";
                comparisonValue.DataSource = comparisons;
                comparisonValue.SelectedValue = condition.Operator;
                rightValue.Enabled = condition.Operator != "Empty";
            }
            finally { filling = false; }
        }

        /// <summary>更新目录文案时保留未完成文本、选中引用和光标位置。</summary>
        internal void RefreshChoices(FlowActions catalog, FlowMethod method)
        {
            ComboBox focused = leftValue.Focused ? leftValue : rightValue.Focused ? rightValue : null;
            int start = focused == null ? 0 : focused.SelectionStart, length = focused == null ? 0 : focused.SelectionLength;
            FlowCondition current = new FlowCondition { Left = Read(leftValue), Operator = Convert.ToString(comparisonValue.SelectedValue), Right = Read(rightValue) };
            Bind(current,catalog,method);
            if (focused == null) return;
            focused.SelectionStart = Math.Min(start,focused.Text.Length); focused.SelectionLength = Math.Min(length,focused.Text.Length-focused.SelectionStart);
        }

        /// <summary>取得完整字段目录；高级身份仅在已绑定时显示，默认保留业务结果与当前配方。</summary>
        private static List<Operand> Choices(FlowActions catalog, FlowMethod method)
        {
            List<Operand> result = new List<Operand>();
            if (method != null) result.AddRange(method.Outputs.Select(field => Choice("Result",field.Name,FlowMethods.FieldTitle(method.Id,field,true),field.Type,field.Advanced)));
            result.Add(Choice("Constant","true",FlowText.T("bindingTrue"),typeof(bool)));
            result.Add(Choice("Constant","false",FlowText.T("bindingFalse"),typeof(bool)));
            if (catalog != null)
            {
                result.AddRange(catalog.Resources.ParameterChoices.Select(value => Choice("Parameter",value.Id,value.Label,value.ValueType,value.Advanced)));
                result.AddRange(FlowPrograms.OutputChoices(catalog).Select(value => Choice("Output",value.Id,value.Label,value.ValueType,value.Advanced)));
            }
            result.Add(Choice("Context","RecipeName",ContextTitle("inputContextRecipe"),typeof(string)));
            result.Add(Choice("Context","CycleId",ContextTitle("inputContextCycle"),typeof(string),true));
            result.Add(Choice("Context","RunId",ContextTitle("inputContextRun"),typeof(string),true));
            result.Add(Choice("Context","ExecutionId",ContextTitle("inputContextExecution"),typeof(string),true));
            return result;
        }

        /// <summary>构造带来源的可读选项，固定值只显示其文本。</summary>
        private static Operand Choice(string source,string reference,string label,Type valueType = null,bool advanced = false)
        {
            return new Operand { Source = source, Reference = reference, Label = source == "Constant" ? label : FlowText.T(source) + " / " + label, ValueType = valueType, Advanced = advanced };
        }

        /// <summary>读取上下文字段的业务文案，沿用现场语言覆盖且不翻译稳定引用。</summary>
        private static string ContextTitle(string key)
        {
            string[] words = FlowInputRow.TextDefaults[key];
            if (UiTheme.IsDesignTime) return words[0];
            return MiddleLayer.LangMsg("EquipmentFlow",key,words[0],words[1],words[2]);
        }

        /// <summary>左侧字段改变时只更新右侧候选，已输入文字及现有引用保持不变。</summary>
        private void LeftChanged(object sender,EventArgs args)
        {
            if (filling) return;
            FlowInput current = Read(rightValue);
            filling = true;
            try { Fill(rightValue,current,RightChoices()); }
            finally { filling = false; }
        }

        /// <summary>按已选左字段类型推荐右值；未知类型、object 或自由文字不限制候选。</summary>
        private List<Operand> RightChoices()
        {
            Operand left = leftValue.SelectedItem as Operand;
            Type expected = left != null && leftValue.Text == left.Label ? left.ValueType : null;
            return allChoices.Where(choice => !choice.Advanced && Compatible(expected,choice.ValueType)).ToList();
        }

        /// <summary>比较字段保持布尔和文本独立，数值类型互相兼容且不把枚举当成数值。</summary>
        private static bool Compatible(Type expected,Type actual)
        {
            if (expected == null || expected == typeof(object) || actual == null || actual == typeof(object)) return true;
            if (expected == actual || expected.IsAssignableFrom(actual) || actual.IsAssignableFrom(expected)) return true;
            return IsNumeric(expected) && IsNumeric(actual);
        }

        /// <summary>识别可参与数值比较的内置类型，排除布尔、字符和枚举。</summary>
        private static bool IsNumeric(Type type)
        {
            if (type.IsEnum) return false;
            TypeCode code = Type.GetTypeCode(type);
            return code >= TypeCode.SByte && code <= TypeCode.Decimal;
        }

        /// <summary>保留不匹配、高级及缺失引用供工程师修正，不把旧字段静默变成固定文本。</summary>
        private void Fill(ComboBox box,FlowInput value,List<Operand> choices)
        {
            box.BeginUpdate(); box.Items.Clear(); box.Items.AddRange(choices.ToArray());
            Operand selected = choices.FirstOrDefault(item => item.Source == value.Source && item.Reference == value.Reference);
            if (selected == null && value.Source != "Constant")
            {
                selected = allChoices.FirstOrDefault(item => item.Source == value.Source && item.Reference == value.Reference) ?? Choice(value.Source,value.Reference,value.Reference);
                box.Items.Add(selected);
            }
            box.SelectedItem = selected;
            if (selected == null) box.Text = value.Reference ?? "";
            box.EndUpdate();
        }

        /// <summary>选择字段保存结构化引用；手工输入不被解析成可执行表达式。</summary>
        private static FlowInput Read(ComboBox box)
        {
            Operand choice = box.SelectedItem as Operand;
            if (choice != null && box.Text == choice.Label) return new FlowInput { Source = choice.Source, Reference = choice.Reference };
            return new FlowInput { Source = "Constant", Reference = box.Text };
        }
    }
}
