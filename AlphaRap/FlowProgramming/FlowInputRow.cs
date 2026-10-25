using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    /// <summary>按实际对象声明呈现单个业务参数，引用身份与显示文案保持分离。</summary>
    public partial class FlowInputRow : UserControl
    {
        internal static readonly Dictionary<string, string[]> TextDefaults = new Dictionary<string, string[]>
        {
            { "inputBind", new[] { "绑定数据", "Bind data", "Vincular datos" } },
            { "inputUseLiteral", new[] { "使用固定值", "Use a fixed value", "Usar un valor fijo" } },
            { "inputYes", new[] { "是", "Yes", "Sí" } },
            { "inputNo", new[] { "否", "No", "No" } },
            { "inputMissingReference", new[] { "引用已失效：{0}", "Missing reference: {0}", "Referencia no disponible: {0}" } },
            { "inputEmptyReference", new[] { "尚未绑定，请选择数据来源", "Not bound; select a data source", "Sin vínculo; seleccione una fuente de datos" } },
            { "inputInvalidChoice", new[] { "选项已失效：{0}", "Unavailable choice: {0}", "Opción no disponible: {0}" } },
            { "inputEmptyChoice", new[] { "请选择", "Select a value", "Seleccione un valor" } },
            { "inputProducerEmpty", new[] { "请选择产生数据的节点", "Select the node producing this data", "Seleccione el nodo que produce estos datos" } },
            { "inputContextRecipe", new[] { "当前配方", "Current recipe", "Receta actual" } },
            { "inputContextCycle", new[] { "当前产品", "Current product", "Producto actual" } },
            { "inputContextRun", new[] { "本次运行（高级兼容）", "Current run (advanced compatibility)", "Ejecución actual (compatibilidad avanzada)" } },
            { "inputContextExecution", new[] { "本次执行（高级兼容）", "Current execution (advanced compatibility)", "Llamada actual (compatibilidad avanzada)" } }
        };

        private sealed class LiteralChoice
        {
            internal string Value, Label;
            /// <summary>显示选项文案，固定值保存于独立字段。</summary>
            public override string ToString() { return Label; }
        }

        private FlowField field;
        private FlowActions catalog;
        private string methodId;
        private FlowInput current = new FlowInput();
        private bool filling;

        public event EventHandler ValueChanged;
        public event EventHandler BindRequested;
        public string FieldName { get { return field == null ? current.Name : field.Name; } }
        public bool IsAdvanced { get { return field != null && field.Advanced; } }
        public bool HasCustomValue
        {
            get
            {
                if (field == null) return false;
                if (current.Source != "Constant") return true;
                StringComparison comparison = field.Type == typeof(bool) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                return !string.Equals(current.Reference ?? "", field.DefaultValue ?? "", comparison);
            }
        }

        /// <summary>创建可独立预览的参数行；不加载对象、不连接硬件。</summary>
        public FlowInputRow()
        {
            InitializeComponent();
            if (UiTheme.IsDesignTime) return;
            literalValue.TextChanged += LiteralChanged;
            optionValue.SelectedIndexChanged += OptionChanged;
            bindData.Click += RequestBinding;
            useLiteral.Click += UseLiteral;
            valueMenu.Opening += PrepareMenu;
        }

        /// <summary>绑定参数草稿与真实字段声明；methodId 用于隔离同名字段的语言键。</summary>
        public void Bind(FlowInput input, FlowField definition, FlowActions actions, string actionId)
        {
            if (definition == null) throw new ArgumentNullException("definition");
            field = definition;
            catalog = actions;
            methodId = actionId ?? "";
            Name = "input_" + field.Name + "_dyn";
            ApplyInput(input ?? new FlowInput { Name = field.Name, Reference = field.DefaultValue });
        }

        /// <summary>读取原始固定值或结构化绑定；未完成的数字输入留到节点确定时校验。</summary>
        public FlowInput ReadInput()
        {
            return new FlowInput { Name = FieldName, Source = current.Source, Reference = current.Reference };
        }

        /// <summary>更新单项草稿并通知宿主，不持久化配置，也不解析或替换缺失引用。</summary>
        public void SetInput(FlowInput input)
        {
            if (input == null) throw new ArgumentNullException("input");
            ApplyInput(input);
            NotifyChanged();
        }

        /// <summary>只刷新业务标题与选项文案，保留当前文本、引用、布尔字面值和光标位置。</summary>
        public void RefreshLanguage()
        {
            int start = literalValue.SelectionStart;
            int length = literalValue.SelectionLength;
            filling = true;
            try { RefreshDisplay(); }
            finally { filling = false; }
            literalValue.SelectionStart = Math.Min(start, literalValue.TextLength);
            literalValue.SelectionLength = Math.Min(length, literalValue.TextLength - literalValue.SelectionStart);
        }

        /// <summary>复制输入，刷新本行状态；绑定期间的控件事件不修改草稿。</summary>
        private void ApplyInput(FlowInput input)
        {
            filling = true;
            try
            {
                current = new FlowInput { Name = field == null ? input.Name : field.Name, Source = input.Source, Reference = input.Reference };
                RefreshDisplay();
            }
            finally { filling = false; }
        }

        /// <summary>根据固定值或引用选择一种编辑方式，固定界面结构沿用设计器。</summary>
        private void RefreshDisplay()
        {
            fieldLabel.Text = field == null ? "目标点位" : FlowMethods.FieldTitle(methodId, field);
            unitLabel.Text = field == null ? "" : field.Unit ?? "";
            bindData.Text = TextOf("inputBind");
            useLiteral.Text = TextOf("inputUseLiteral");
            string help = field == null ? "" : FlowMethods.FieldHelp(methodId, field);
            helpTip.SetToolTip(fieldLabel, string.IsNullOrEmpty(help) ? fieldLabel.Text : fieldLabel.Text + Environment.NewLine + help);
            helpTip.SetToolTip(literalValue, help);
            helpTip.SetToolTip(optionValue, help);
            bool reference = current.Source != "Constant" || RequiresProducer();
            bool options = !reference && field != null && (field.Options.Count > 0 || field.Type == typeof(bool));
            literalValue.Visible = !reference && !options;
            optionValue.Visible = options;
            referenceValue.Visible = reference;
            literalValue.Text = current.Reference ?? "";
            if (options) FillOptions();
            if (!reference) return;
            referenceValue.Text = ReferenceLabel();
            referenceValue.ForeColor = IsMissingReference() ? SystemColors.GrayText : SystemColors.ControlText;
            helpTip.SetToolTip(referenceValue, referenceValue.Text);
        }

        /// <summary>选项以稳定值匹配；空值和失效旧值单独显示，不自动采用第一个选项。</summary>
        private void FillOptions()
        {
            List<LiteralChoice> choices = field.Options.Select(option => new LiteralChoice { Value = option.Value, Label = option.Label }).ToList();
            if (choices.Count == 0 && field.Type == typeof(bool))
            {
                choices.Add(new LiteralChoice { Value = "True", Label = TextOf("inputYes") });
                choices.Add(new LiteralChoice { Value = "False", Label = TextOf("inputNo") });
            }
            LiteralChoice selected = choices.FirstOrDefault(choice => choice.Value == current.Reference);
            if (selected == null && field.Type == typeof(bool) && field.Options.Count == 0)
                selected = choices.FirstOrDefault(choice => BooleanLiteral(choice.Value) == BooleanLiteral(current.Reference) && BooleanLiteral(current.Reference).HasValue);
            if (selected == null)
            {
                selected = new LiteralChoice { Value = current.Reference, Label = string.IsNullOrEmpty(current.Reference) ? TextOf("inputEmptyChoice") : string.Format(TextOf("inputInvalidChoice"), current.Reference) };
                choices.Insert(0, selected);
            }
            optionValue.BeginUpdate();
            optionValue.Items.Clear();
            optionValue.Items.AddRange(choices.ToArray());
            optionValue.SelectedItem = selected;
            optionValue.EndUpdate();
        }

        /// <summary>识别现有布尔字面值以显示是或否，不改变其已保存的格式。</summary>
        private static bool? BooleanLiteral(string value)
        {
            if (value == "1") return true;
            if (value == "0") return false;
            bool result;
            return bool.TryParse(value, out result) ? (bool?)result : null;
        }

        /// <summary>按真实目录生成可读引用；生产节点来源省略内部结果身份字段。</summary>
        private string ReferenceLabel()
        {
            if (string.IsNullOrEmpty(current.Reference)) return TextOf(RequiresProducer() ? "inputProducerEmpty" : "inputEmptyReference");
            if (current.Source == "Context") return ContextLabel();
            FlowChoice choice = ReferenceChoice();
            if (choice == null) return string.Format(TextOf("inputMissingReference"), current.Reference);
            if (!RequiresProducer()) return choice.Label;
            int separator = choice.Label.LastIndexOf(" / ", StringComparison.Ordinal);
            return separator < 0 ? choice.Label : choice.Label.Substring(0, separator);
        }

        /// <summary>只查询已捕获的实际常量和节点目录，不按显示名猜测替代对象。</summary>
        private FlowChoice ReferenceChoice()
        {
            if (catalog == null) return null;
            if (current.Source == "Parameter") return catalog.Resources.ParameterChoices.FirstOrDefault(choice => choice.Id == current.Reference);
            if (current.Source == "Output") return FlowPrograms.OutputChoices(catalog).FirstOrDefault(choice => choice.Id == current.Reference);
            return null;
        }

        /// <summary>用业务含义显示旧上下文绑定，运行编号只保留在结构化引用中。</summary>
        private string ContextLabel()
        {
            switch (current.Reference)
            {
                case "RecipeName": return TextOf("inputContextRecipe");
                case "CycleId": return TextOf("inputContextCycle");
                case "RunId": return TextOf("inputContextRun");
                case "ExecutionId": return TextOf("inputContextExecution");
                default: return string.Format(TextOf("inputMissingReference"), current.Reference);
            }
        }

        /// <summary>标记空引用或不在当前目录中的引用，保留原值供宿主修复。</summary>
        private bool IsMissingReference()
        {
            if (string.IsNullOrEmpty(current.Reference)) return true;
            if (current.Source == "Context") return !new[] { "CycleId", "RunId", "RecipeName", "ExecutionId" }.Contains(current.Reference);
            return ReferenceChoice() == null;
        }

        /// <summary>来源节点参数由宿主选择生产节点，不提供内部运行编号的固定编辑框。</summary>
        private bool RequiresProducer() { return field != null && (!string.IsNullOrEmpty(field.OutputSourceMethod) || !string.IsNullOrEmpty(field.OutputSourceField)); }

        /// <summary>读取行固定三语文案，并尊重现场语言包。</summary>
        private static string TextOf(string key)
        {
            string[] words = TextDefaults[key];
            if (UiTheme.IsDesignTime) return words[0];
            return MiddleLayer.LangMsg("EquipmentFlow", key, words[0], words[1], words[2]);
        }

        /// <summary>保留正在编辑的固定文本，不在每次按键时解析数值。</summary>
        private void LiteralChanged(object sender, EventArgs args)
        {
            if (filling || current.Source != "Constant" || RequiresProducer()) return;
            current.Reference = literalValue.Text;
            NotifyChanged();
        }

        /// <summary>用户选择后才写入固定选项的稳定值。</summary>
        private void OptionChanged(object sender, EventArgs args)
        {
            if (filling || current.Source != "Constant") return;
            LiteralChoice selected = optionValue.SelectedItem as LiteralChoice;
            if (selected == null) return;
            current.Reference = selected.Value;
            NotifyChanged();
        }

        /// <summary>由宿主统一打开绑定选择器，本行不创建嵌套配置窗口。</summary>
        private void RequestBinding(object sender, EventArgs args)
        {
            EventHandler handler = BindRequested;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>仅普通参数允许明确切回固定值；来源节点参数继续由宿主选择。</summary>
        private void PrepareMenu(object sender, System.ComponentModel.CancelEventArgs args)
        {
            useLiteral.Enabled = field != null && !RequiresProducer() && current.Source != "Constant";
        }

        /// <summary>明确放弃当前绑定后使用对象声明的固定默认值，文件仍由主保存落盘。</summary>
        private void UseLiteral(object sender, EventArgs args)
        {
            if (field == null || RequiresProducer()) return;
            SetInput(new FlowInput { Name = field.Name, Source = "Constant", Reference = field.DefaultValue });
        }

        /// <summary>通知宿主刷新高级参数展开状态，不修改其他参数。</summary>
        private void NotifyChanged()
        {
            EventHandler handler = ValueChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }
    }
}
