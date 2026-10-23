using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    public partial class FlowSourceForm : FlowLanguageForm
    {
        public static readonly Dictionary<string, string[]> TextDefaults = new Dictionary<string, string[]>
        {
            { "bindingTitle", new[] { "绑定数据", "Bind data", "Vincular datos" } },
            { "bindingSearch", new[] { "搜索名称", "Search names", "Buscar nombres" } },
            { "bindingFixed", new[] { "固定值", "Fixed value", "Valor fijo" } },
            { "bindingMachine", new[] { "机器设置", "Machine settings", "Ajustes de máquina" } },
            { "bindingRecipe", new[] { "当前配方", "Current recipe", "Receta actual" } },
            { "bindingOutputs", new[] { "节点结果", "Node results", "Resultados de nodos" } },
            { "bindingAdvanced", new[] { "高级来源", "Advanced sources", "Fuentes avanzadas" } },
            { "bindingRetained", new[] { "当前绑定（需检查）", "Current binding (review required)", "Vínculo actual (requiere revisión)" } },
            { "bindingLiteral", new[] { "输入固定值", "Enter a fixed value", "Introducir un valor fijo" } },
            { "bindingValue", new[] { "当前选择 / 固定值", "Selected source / Fixed value", "Fuente elegida / Valor fijo" } },
            { "bindingUnavailable", new[] { "此绑定已缺失、类型不匹配或不适用于该字段。已保留原引用，请重新选择。", "This binding is missing, incompatible, or unavailable for this field. The original reference is retained; select another source.", "Este vínculo falta, es incompatible o no está disponible para este campo. Se conserva la referencia original; seleccione otra fuente." } },
            { "bindingCompatible", new[] { "只列出适合该字段的数据；数值转换仍在保存和执行前严格校验。", "Only suitable data is listed. Numeric conversions are still checked strictly before saving and execution.", "Solo se muestran datos adecuados. Las conversiones numéricas se comprueban al guardar y ejecutar." } },
            { "bindingCapture", new[] { "选择生成本次图像的拍照节点，图像身份由框架传递。", "Select the node that captured this image. The framework passes its image identity.", "Seleccione el nodo que capturó esta imagen. El marco transmite su identidad." } },
            { "bindingChoose", new[] { "请选择数据来源或输入有效的固定值。", "Select a data source or enter a valid fixed value.", "Seleccione una fuente o introduzca un valor fijo válido." } },
            { "bindingTrue", new[] { "是 / true", "Yes / true", "Sí / true" } },
            { "bindingFalse", new[] { "否 / false", "No / false", "No / false" } },
        };

        private sealed class SourceChoice
        {
            internal string Source, Reference, Label, Group, Detail;
            internal bool FreeLiteral, Unavailable;
        }

        private FlowActions actions;
        private FlowMethod method;
        private FlowField field;
        private FlowInput pending = new FlowInput();
        private string literalDraft = "";
        private string description;
        private string targetId;
        private bool refreshing;
        private bool advancedExpanded;

        public FlowInput Selection { get; private set; }

        /// <summary>建立可在设计器预览的统一数据绑定窗口，不加载设备或工程。</summary>
        public FlowSourceForm()
        {
            InitializeComponent();
            searchValue.TextChanged += (sender, args) => RefreshTree();
            value.TextChanged += LiteralChanged;
            tree.AfterSelect += SourceSelected;
            tree.AfterExpand += (sender, args) => { if (!refreshing && args.Node.Name == "bindingAdvanced") advancedExpanded = true; };
            tree.AfterCollapse += (sender, args) => { if (!refreshing && args.Node.Name == "bindingAdvanced") advancedExpanded = false; };
            tree.NodeMouseDoubleClick += (sender, args) => { if (args.Node.Tag is SourceChoice) AcceptReference(); };
            ok.Click += (sender, args) => AcceptReference();
        }

        /// <summary>在同一窗口选择固定值、机器设置、当前配方或类型匹配的节点结果。</summary>
        public FlowSourceForm(FlowInput current, FlowActions catalog, FlowMethod action, FlowField parameter, string description = null, string targetId = null) : this()
        {
            actions = catalog;
            method = action;
            field = parameter;
            this.description = description;
            this.targetId = targetId;
            pending = Copy(current ?? new FlowInput());
            if (field != null) pending.Name = field.Name;
            Selection = Copy(pending);
            literalDraft = pending.Source == "Constant" ? pending.Reference ?? "" : "";
            advancedExpanded = pending.Source == "Context" || pending.Source == "Result" || catalog != null &&
                (FlowPrograms.OutputChoices(catalog).Any(item => item.Advanced && pending.Source == "Output" && item.Id == pending.Reference) ||
                 catalog.Resources.ParameterChoices.Any(item => item.Advanced && pending.Source == "Parameter" && item.Id == pending.Reference));
            RefreshLanguageChoices();
        }

        /// <summary>切换语言时只刷新显示文字，保留搜索、来源、固定值草稿和光标。</summary>
        protected override void RefreshLanguageChoices()
        {
            int start = value.SelectionStart, length = value.SelectionLength;
            contextHint.Text = ContextDescription();
            contextHint.Visible = !string.IsNullOrEmpty(contextHint.Text);
            RefreshTree();
            value.SelectionStart = Math.Min(start, value.Text.Length);
            value.SelectionLength = Math.Min(length, value.Text.Length - value.SelectionStart);
        }

        /// <summary>从当前轴绑定重算点位说明，语言切换不缓存含业务值的格式化文案。</summary>
        private string ContextDescription()
        {
            if (actions == null || method == null || method.Id != "point.move" || field == null || field.Name != "Index")
                return description ?? (HasOutputConstraint ? FlowText.T("bindingCapture") : FlowText.T("bindingCompatible"));
            FlowResource resource = actions.FindResource(targetId);
            FlowPointAxis points = resource == null || resource.BoundPoints == null ? null : resource.BoundPoints.Instance as FlowPointAxis;
            if (points == null) return description ?? FlowText.T("boundTable");
            string tableName = points.TableName.StartsWith("AxisPoints_", StringComparison.Ordinal) ? resource.Label : points.TableName;
            return string.Format(FlowText.T("axisPointsHelp"), resource.Label, tableName, points.RowCount, points.Positions.Count);
        }

        private bool HasOutputConstraint { get { return field != null && (!string.IsNullOrEmpty(field.OutputSourceMethod) || !string.IsNullOrEmpty(field.OutputSourceField)); } }

        /// <summary>从真实目录生成候选项，并按字段能力和数据类型过滤。</summary>
        private List<SourceChoice> Choices()
        {
            List<SourceChoice> choices = new List<SourceChoice>();
            if (!HasOutputConstraint) AddFixedChoices(choices);
            if (actions == null) return RetainCurrent(choices);
            if (!HasOutputConstraint)
            {
                foreach (FlowChoice item in actions.Resources.ParameterChoices)
                    if (Compatible(item.ValueType)) choices.Add(FromChoice("Parameter", item, item.Advanced ? "bindingAdvanced" : IsRecipe(item.Id) ? "bindingRecipe" : "bindingMachine"));
                AddAdvancedChoices(choices);
            }
            foreach (FlowChoice item in FlowPrograms.OutputChoices(actions))
            {
                if (item.Advanced && !HasOutputConstraint && !(pending.Source == "Output" && item.Id == pending.Reference)) continue;
                if (Compatible(item.ValueType) && MatchesOutput(item)) choices.Add(FromChoice("Output", item, item.Advanced && !HasOutputConstraint ? "bindingAdvanced" : "bindingOutputs"));
            }
            return RetainCurrent(choices);
        }

        /// <summary>固定选项沿用对象声明，布尔值明确显示 true 与 false，其余类型允许直接输入。</summary>
        private void AddFixedChoices(List<SourceChoice> choices)
        {
            if (field != null && field.Options.Count > 0)
            {
                choices.AddRange(field.Options.Select(option => new SourceChoice { Source = "Constant", Reference = option.Value, Label = option.Label, Group = "bindingFixed", Detail = option.Group }));
                return;
            }
            if (field != null && field.Type == typeof(bool))
            {
                choices.Add(new SourceChoice { Source = "Constant", Reference = BooleanReference(true), Label = FlowText.T("bindingTrue"), Group = "bindingFixed" });
                choices.Add(new SourceChoice { Source = "Constant", Reference = BooleanReference(false), Label = FlowText.T("bindingFalse"), Group = "bindingFixed" });
                return;
            }
            choices.Add(new SourceChoice { Source = "Constant", Reference = "", Label = FlowText.T("bindingLiteral"), Group = "bindingFixed", FreeLiteral = true });
        }

        /// <summary>保留已有有效布尔拼写，新选择使用明确的 true 或 false。</summary>
        private string BooleanReference(bool state)
        {
            string standard = state ? "true" : "false";
            if (pending.Source != "Constant") return standard;
            string current = pending.Reference ?? "";
            bool parsed;
            if (bool.TryParse(current, out parsed)) return parsed == state ? current : standard;
            if (current == "1") return state ? current : standard;
            if (current == "0") return state ? standard : current;
            return standard;
        }

        /// <summary>上下文和旧结果引用收起到高级来源，保留稳定业务键。</summary>
        private void AddAdvancedChoices(List<SourceChoice> choices)
        {
            if (Compatible(typeof(string)))
                foreach (string name in new[] { "RecipeName", "CycleId", "RunId", "ExecutionId" })
                    choices.Add(new SourceChoice { Source = "Context", Reference = name, Label = name, Group = "bindingAdvanced" });
            if (pending.Source != "Result" || method == null) return;
            foreach (FlowField output in method.Outputs)
                if (Compatible(output.Type)) choices.Add(new SourceChoice { Source = "Result", Reference = output.Name, Label = FlowText.T("Result") + " / " + output.Name, Group = "bindingAdvanced" });
        }

        /// <summary>缺失或不兼容的旧绑定仍显示，避免搜索和刷新把原引用改为默认值。</summary>
        private List<SourceChoice> RetainCurrent(List<SourceChoice> choices)
        {
            if (choices.Any(choice => Matches(choice, pending))) return choices;
            if (string.IsNullOrEmpty(pending.Reference) && HasOutputConstraint) return choices;
            choices.Add(new SourceChoice { Source = pending.Source, Reference = pending.Reference, Label = CurrentLabel(), Group = "bindingRetained", Unavailable = true });
            return choices;
        }

        /// <summary>为旧绑定优先显示真实目录名称，无法解析时仍显示原稳定引用。</summary>
        private string CurrentLabel()
        {
            if (actions == null) return pending.Reference ?? "";
            IEnumerable<FlowChoice> values = pending.Source == "Parameter" ? actions.Resources.ParameterChoices : pending.Source == "Output" ? FlowPrograms.OutputChoices(actions) : new List<FlowChoice>();
            FlowChoice known = values.FirstOrDefault(item => item.Id == pending.Reference);
            return known == null ? (pending.Reference ?? "") : known.Label;
        }

        /// <summary>引用根据声明类型推荐；数值间允许选择，具体范围和整数性由运行校验负责。</summary>
        private bool Compatible(Type actual)
        {
            if (field == null || field.Type == typeof(object)) return true;
            if (actual == null) return false;
            if (field.Type == actual || field.Type.IsAssignableFrom(actual)) return true;
            return IsNumeric(field.Type) && IsNumeric(actual);
        }

        /// <summary>判断基础数值类型，不把布尔、枚举或字符串当作数字。</summary>
        private static bool IsNumeric(Type type)
        {
            if (type == null || type.IsEnum) return false;
            TypeCode code = Type.GetTypeCode(type);
            return code >= TypeCode.SByte && code <= TypeCode.Decimal;
        }

        /// <summary>图像来源只允许绑定同一对象声明的来源方法和输出字段。</summary>
        private bool MatchesOutput(FlowChoice choice)
        {
            if (field == null) return true;
            if (!string.IsNullOrEmpty(field.OutputSourceMethod) && choice.ActionId != field.OutputSourceMethod) return false;
            if (!string.IsNullOrEmpty(field.OutputSourceField) && choice.FieldName != field.OutputSourceField) return false;
            return !HasOutputConstraint || !string.IsNullOrEmpty(targetId) && choice.TargetId == targetId;
        }

        /// <summary>把目录身份和业务显示名分开，图像来源只显示产生图像的节点。</summary>
        private SourceChoice FromChoice(string source, FlowChoice choice, string group)
        {
            string label = choice.Label ?? choice.Id;
            int separator = HasOutputConstraint ? label.LastIndexOf(" / ", StringComparison.Ordinal) : -1;
            if (separator >= 0) label = label.Substring(0, separator);
            return new SourceChoice { Source = source, Reference = choice.Id, Label = label, Group = group };
        }

        /// <summary>常量与普通配方参数按稳定作用域分组，不依赖界面翻译文字。</summary>
        private static bool IsRecipe(string reference)
        {
            return (reference ?? "").StartsWith("Recipe/", StringComparison.Ordinal) || (reference ?? "").StartsWith("Constants/Recipe/", StringComparison.Ordinal);
        }

        /// <summary>按当前搜索重绘分组并恢复选中来源，高级上下文默认保持收起。</summary>
        private void RefreshTree()
        {
            if (refreshing) return;
            refreshing = true;
            tree.BeginUpdate();
            tree.Nodes.Clear();
            string search = searchValue.Text.Trim();
            SourceChoice current = null;
            foreach (SourceChoice choice in Choices())
            {
                if (Matches(choice, pending)) current = choice;
                if (!MatchesSearch(choice, search)) continue;
                TreeNode root = tree.Nodes.Cast<TreeNode>().FirstOrDefault(node => node.Name == choice.Group);
                if (root == null) { root = new TreeNode(FlowText.T(choice.Group)) { Name = choice.Group }; tree.Nodes.Add(root); }
                TreeNodeCollection siblings = root.Nodes;
                if (!string.IsNullOrEmpty(choice.Detail))
                {
                    TreeNode section = siblings.Cast<TreeNode>().FirstOrDefault(node => node.Text == choice.Detail);
                    if (section == null) { section = new TreeNode(choice.Detail); siblings.Add(section); }
                    siblings = section.Nodes;
                }
                TreeNode leaf = new TreeNode(choice.Label) { Name = choice.Source + ":" + choice.Reference, Tag = choice, ForeColor = choice.Unavailable ? Color.Firebrick : tree.ForeColor };
                siblings.Add(leaf);
                if (Matches(choice, pending)) tree.SelectedNode = leaf;
            }
            foreach (TreeNode root in tree.Nodes)
                if (root.Name != "bindingAdvanced" || advancedExpanded) root.ExpandAll();
            tree.EndUpdate();
            ShowSelection(current);
            refreshing = false;
        }

        /// <summary>搜索业务名称、分组和固定选项，不要求工程师知道内部路径。</summary>
        private static bool MatchesSearch(SourceChoice choice, string search)
        {
            if (search.Length == 0) return true;
            string text = choice.Label + " " + choice.Detail + " " + FlowText.T(choice.Group);
            return text.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        /// <summary>固定自由文本使用同一节点；其他来源按完整来源与稳定引用恢复。</summary>
        private static bool Matches(SourceChoice choice, FlowInput input)
        {
            if (choice.Source != input.Source) return false;
            return choice.FreeLiteral || string.Equals(choice.Reference ?? "", input.Reference ?? "", StringComparison.Ordinal);
        }

        /// <summary>选择目录叶只修改本窗口草稿，不写入工程或文件。</summary>
        private void SourceSelected(object sender, TreeViewEventArgs args)
        {
            if (refreshing) return;
            SourceChoice choice = args.Node.Tag as SourceChoice;
            if (choice == null) return;
            pending.Source = choice.Source;
            pending.Reference = choice.FreeLiteral ? literalDraft : choice.Reference;
            refreshing = true;
            ShowSelection(choice);
            refreshing = false;
        }

        /// <summary>显示选中来源与可读提示，只有自由固定值允许直接输入。</summary>
        private void ShowSelection(SourceChoice choice)
        {
            value.ReadOnly = choice == null || !choice.FreeLiteral;
            value.Text = choice == null ? pending.Reference ?? "" : choice.FreeLiteral ? literalDraft : choice.Source == "Constant" ? choice.Label : FlowText.T(choice.Group) + " / " + choice.Label;
            statusHint.Text = choice != null && choice.Unavailable ? FlowText.T("bindingUnavailable") : HasOutputConstraint ? FlowText.T("bindingCapture") : FlowText.T("bindingCompatible");
            statusHint.ForeColor = choice != null && choice.Unavailable ? Color.Firebrick : SystemColors.GrayText;
        }

        /// <summary>保留未完成固定文本，搜索和语言切换不替换正在输入的值。</summary>
        private void LiteralChanged(object sender, EventArgs args)
        {
            if (refreshing || value.ReadOnly) return;
            literalDraft = value.Text;
            pending.Reference = literalDraft;
        }

        /// <summary>复制来源身份，取消窗口时不会修改调用者持有的原对象。</summary>
        private static FlowInput Copy(FlowInput input)
        {
            return new FlowInput { Name = input.Name, Source = input.Source, Reference = input.Reference };
        }

        /// <summary>确认一个有效来源；固定值按字段类型严格校验，缺失引用留在窗口供修正。</summary>
        private void AcceptReference()
        {
            SourceChoice selected = Choices().FirstOrDefault(choice => Matches(choice, pending));
            if (selected == null || selected.Unavailable)
            {
                statusHint.Text = FlowText.T(selected == null ? "bindingChoose" : "bindingUnavailable");
                statusHint.ForeColor = Color.Firebrick;
                return;
            }
            if (pending.Source == "Constant" && field != null)
            {
                try { field.ConvertValue(pending.Reference ?? ""); }
                catch (Exception error) when (error is FormatException || error is OverflowException || error is InvalidCastException || error is InvalidOperationException || error is ArgumentException)
                {
                    statusHint.Text = string.Format(FlowText.T("invalidType"), method == null ? field.Name : FlowMethods.FieldTitle(method.Id, field), field.Type.Name);
                    statusHint.ForeColor = Color.Firebrick;
                    return;
                }
            }
            Selection = Copy(pending);
            DialogResult = DialogResult.OK;
        }
    }
}
