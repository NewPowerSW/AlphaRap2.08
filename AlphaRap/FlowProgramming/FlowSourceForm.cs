using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    public partial class FlowSourceForm : FlowLanguageForm
    {
        public string Reference { get; private set; }

        /// <summary>Build the fixed controls without loading application data.</summary>
        public FlowSourceForm() { InitializeComponent(); }

        /// <summary>按来源显示实际可选对象或常量；引用键与显示名称分开。</summary>
        public FlowSourceForm(string source, string current, FlowActions actions, FlowMethod action, string field) : this()
        {
            value.Text = current ?? "";
            value.ReadOnly = source != "Constant";
            List<FlowChoice> choices = Choices(source, actions, action, field);
            if (source == "Constant" && choices.Count > 0) value.ReadOnly = true;
            foreach (FlowChoice choice in choices)
            {
                TreeNode node = new TreeNode(choice.Label) { Tag = choice.Id };
                TreeNodeCollection siblings = tree.Nodes;
                if (source == "Output" || source == "Parameter")
                {
                    string[] groups = choice.Label.Split(new[] { " / " }, StringSplitOptions.None);
                    for (int index = 0; index < groups.Length - 1; index++)
                    {
                        TreeNode group = siblings.Cast<TreeNode>().FirstOrDefault(item => item.Tag == null && item.Text == groups[index]);
                        if (group == null) { group = new TreeNode(groups[index]); siblings.Add(group); }
                        siblings = group.Nodes;
                    }
                    node.Text = groups[groups.Length - 1];
                }
                siblings.Add(node);
                if (choice.Id == current) tree.SelectedNode = node;
            }
            if (value.ReadOnly && tree.SelectedNode != null) value.Text = tree.SelectedNode.FullPath;
            tree.AfterSelect += (sender, args) => { if (args.Node.Tag != null) value.Text = value.ReadOnly ? args.Node.FullPath : Convert.ToString(args.Node.Tag); };
            tree.NodeMouseDoubleClick += (sender, args) => AcceptReference();
            ok.Click += (sender, args) => AcceptReference();
        }

        /// <summary>统一列出型号参数、全部模块输出、当前动作结果与上下文字段。</summary>
        private static List<FlowChoice> Choices(string source, FlowActions actions, FlowMethod action, string field)
        {
            if (source == "Parameter") return actions.Resources.ParameterChoices;
            if (source == "Output") return FlowPrograms.OutputChoices(actions);
            if (source == "Result" && action != null) return actions.OutputNames(action).Select(name => new FlowChoice { Id = name, Label = name }).ToList();
            if (source == "Context") return new[] { "RecipeName", "CycleId", "RunId", "ExecutionId" }.Select(name => new FlowChoice { Id = name, Label = name }).ToList();
            FlowField parameter = action == null ? null : action.Inputs.FirstOrDefault(item => item.Name == field);
            if (source == "Constant" && parameter != null)
                return parameter.Options.Select(option => new FlowChoice { Id = option.Value, Label = option.Label }).ToList();
            return new List<FlowChoice>();
        }

        /// <summary>提交引用；非固定值必须从真实目录选取。</summary>
        private void AcceptReference()
        {
            if (value.ReadOnly && (tree.SelectedNode == null || tree.SelectedNode.Tag == null)) return;
            Reference = value.ReadOnly ? Convert.ToString(tree.SelectedNode.Tag) : value.Text;
            DialogResult = DialogResult.OK;
        }
    }
}
