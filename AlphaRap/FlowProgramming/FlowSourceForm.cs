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
        public FlowSourceForm(string source, string current, FlowActions actions, FlowAction action, FlowResource resource, string field) : this()
        {
            value.Text = current ?? "";
            value.ReadOnly = source != "Constant";
            List<FlowChoice> choices = Choices(source, actions, action, resource, field);
            foreach (FlowChoice choice in choices)
            {
                TreeNode node = new TreeNode(choice.Label) { Tag = choice.Id };
                tree.Nodes.Add(node);
                if (choice.Id == current) tree.SelectedNode = node;
            }
            tree.AfterSelect += (sender, args) => value.Text = Convert.ToString(args.Node.Tag);
            tree.NodeMouseDoubleClick += (sender, args) => AcceptReference();
            ok.Click += (sender, args) => AcceptReference();
        }

        /// <summary>统一列出型号参数、全部模块输出、当前动作结果与上下文字段。</summary>
        private static List<FlowChoice> Choices(string source, FlowActions actions, FlowAction action, FlowResource resource, string field)
        {
            if (source == "Parameter") return actions.Resources.ParameterChoices;
            if (source == "Output") return FlowPrograms.OutputChoices(actions);
            if (source == "Result" && action != null) return actions.OutputNames(action, resource).Select(name => new FlowChoice { Id = name, Label = name }).ToList();
            if (source == "Context") return new[] { "RecipeName", "CycleId", "RunId", "ExecutionId" }.Select(name => new FlowChoice { Id = name, Label = name }).ToList();
            if (source == "Constant" && field == "Index" && resource != null) return resource.Points;
            return new List<FlowChoice>();
        }

        /// <summary>提交引用；非固定值必须从真实目录选取。</summary>
        private void AcceptReference()
        {
            if (value.ReadOnly && tree.SelectedNode == null) return;
            Reference = value.Text;
            DialogResult = DialogResult.OK;
        }
    }
}
