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
            /// <summary>显示业务名称与来源，稳定引用保存在独立字段。</summary>
            public override string ToString() { return Label; }
        }
        private static readonly string[] Operators = { "==","!=",">",">=","<","<=","Contains","Empty" };
        private static readonly string[] OperatorTexts = { "equals","notEquals","greater","greaterEqual","less","lessEqual","contains","empty" };

        /// <summary>建立可在设计器预览的条件行。</summary>
        public FlowConditionRow()
        {
            InitializeComponent();
            comparisonValue.SelectedIndexChanged += (sender,args) => rightValue.Enabled = Convert.ToString(comparisonValue.SelectedValue) != "Empty";
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
            List<Operand> choices = Choices(catalog,method);
            Fill(leftValue,condition.Left,choices); Fill(rightValue,condition.Right,choices);
            comparisonValue.DisplayMember = "Label"; comparisonValue.ValueMember = "Id";
            comparisonValue.DataSource = Operators.Select((value,index) => new FlowChoice { Id = value, Label = FlowText.T(OperatorTexts[index]) }).ToList();
            comparisonValue.SelectedValue = condition.Operator;
            rightValue.Enabled = condition.Operator != "Empty";
        }

        /// <summary>更新目录文案时保留未完成文本、选中引用和光标位置。</summary>
        internal void RefreshChoices(FlowActions catalog, FlowMethod method)
        {
            ComboBox focused = leftValue.Focused ? leftValue : rightValue.Focused ? rightValue : null;
            int start = focused == null ? 0 : focused.SelectionStart, length = focused == null ? 0 : focused.SelectionLength;
            Bind(ReadCondition(),catalog,method);
            if (focused == null) return;
            focused.SelectionStart = Math.Min(start,focused.Text.Length); focused.SelectionLength = Math.Min(length,focused.Text.Length-focused.SelectionStart);
        }

        /// <summary>当前结果、常量表、节点输出和上下文在同一列表中直接选择。</summary>
        private static List<Operand> Choices(FlowActions catalog, FlowMethod method)
        {
            List<Operand> result = new List<Operand>();
            if (method != null) result.AddRange(method.Outputs.Select(field => Choice("Result",field.Name,field.Name + " (" + field.Type.Name + ")")));
            result.Add(Choice("Constant","true","true")); result.Add(Choice("Constant","false","false"));
            if (catalog == null) return result;
            result.AddRange(catalog.Resources.ParameterChoices.Select(value => Choice("Parameter",value.Id,value.Label)));
            result.AddRange(FlowPrograms.OutputChoices(catalog).Select(value => Choice("Output",value.Id,value.Label)));
            result.AddRange(new[] { "RecipeName","CycleId","RunId","ExecutionId" }.Select(value => Choice("Context",value,value)));
            return result;
        }

        /// <summary>构造带来源的可读选项，固定值只显示其文本。</summary>
        private static Operand Choice(string source,string reference,string label)
        {
            return new Operand { Source = source, Reference = reference, Label = source == "Constant" ? label : FlowText.T(source) + " / " + label };
        }

        /// <summary>保留缺失引用供工程师修正，不把旧字段静默变成固定文本。</summary>
        private static void Fill(ComboBox box,FlowInput value,List<Operand> choices)
        {
            box.BeginUpdate(); box.Items.Clear(); box.Items.AddRange(choices.ToArray());
            Operand selected = choices.FirstOrDefault(item => item.Source == value.Source && item.Reference == value.Reference);
            if (selected == null && value.Source != "Constant") { selected = Choice(value.Source,value.Reference,value.Reference); box.Items.Add(selected); }
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
