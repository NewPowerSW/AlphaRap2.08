using System.Drawing;
using System.Windows.Forms;

namespace AlphaRap.FlowProgramming
{
    partial class FlowBranchEditor
    {
        private TableLayoutPanel layout;
        private FlowLayoutPanel header,conditions;
        private Label codeLabel,targetLabel,conditionHint;
        private ComboBox target;
        private RadioButton unconditional,all,any;
        private Button addCondition;
        private ToolStripMenuItem remove,conditionDelete;
        private ContextMenuStrip branchMenu,conditionMenu;

        /// <summary>释放出口与条件行菜单。</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing) { branchMenu.Dispose(); conditionMenu.Dispose(); }
            base.Dispose(disposing);
        }

        /// <summary>出口标题和扁平条件行共用可设计模板，无条件时收起条件区域。</summary>
        private void InitializeComponent()
        {
            layout = new TableLayoutPanel { Name = "layout", Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute,64));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute,34));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute,32));
            header = new FlowLayoutPanel { Name = "header", Dock = DockStyle.Fill, WrapContents = true };
            codeLabel = new Label { Name = "codeLabel_dyn", Text = "NEXT", Width = 60, Padding = new Padding(0,6,0,0), Font = new Font("Microsoft YaHei",10F,FontStyle.Bold) };
            targetLabel = new Label { Name = "branchTargetLabel", Tag = "branchTarget", Text = "跳转目标", AutoSize = true, Padding = new Padding(0,6,0,0) };
            target = new ComboBox { Name = "branchTargetValue", DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 };
            unconditional = new RadioButton { Name = "branchUnconditional", Tag = "unconditional", Text = "无条件通过", AutoSize = true, Checked = true };
            all = new RadioButton { Name = "branchAll", Tag = "all", Text = "所有条件满足 (AND)", AutoSize = true };
            any = new RadioButton { Name = "branchAny", Tag = "any", Text = "任一条件满足 (OR)", AutoSize = true };
            header.Controls.AddRange(new Control[] { codeLabel,targetLabel,target,unconditional,all,any });
            conditions = new FlowLayoutPanel { Name = "branchConditions", Dock = DockStyle.Fill, AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };
            addCondition = new Button { Name = "addCondition", Tag = "addCondition", Text = "+ 添加条件", Dock = DockStyle.Left, AutoSize = true };
            conditionHint = new Label { Name = "conditionHint", Tag = "conditionHelp", Text = "直接选择字段，或输入固定值；true / false 可从下拉选择。右键删除条件。", Dock = DockStyle.Fill, AutoEllipsis = true };
            remove = new ToolStripMenuItem { Name = "removeBranch", Tag = "removeBranch", Text = "移除此 CASE", Enabled = false };
            conditionDelete = new ToolStripMenuItem { Name = "conditionDelete", Tag = "deleteCondition", Text = "删除条件" };
            branchMenu = new ContextMenuStrip { Name = "branchMenu" }; branchMenu.Items.Add(remove); header.ContextMenuStrip = branchMenu; codeLabel.ContextMenuStrip = branchMenu;
            conditionMenu = new ContextMenuStrip { Name = "conditionMenu" }; conditionMenu.Items.Add(conditionDelete);
            layout.Controls.Add(header,0,0); layout.Controls.Add(conditions,0,1); layout.Controls.Add(addCondition,0,2); layout.Controls.Add(conditionHint,0,3);
            Controls.Add(layout);
            Name = "FlowBranchEditor"; Padding = new Padding(4); Margin = new Padding(0,3,0,8);
            Font = new Font("Microsoft YaHei",9F); AutoScaleMode = AutoScaleMode.None; Size = new Size(930,140); MinimumSize = new Size(600,40);
        }
    }
}
