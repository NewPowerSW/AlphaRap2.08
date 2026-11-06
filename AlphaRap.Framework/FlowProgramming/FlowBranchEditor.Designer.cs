namespace AlphaRap.FlowProgramming
{
    partial class FlowBranchEditor
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TableLayoutPanel layout;
        private System.Windows.Forms.FlowLayoutPanel header;
        private System.Windows.Forms.FlowLayoutPanel conditions;
        private System.Windows.Forms.Label codeLabel;
        private System.Windows.Forms.Label targetLabel;
        private System.Windows.Forms.Label conditionHint;
        private System.Windows.Forms.ComboBox target;
        private System.Windows.Forms.RadioButton unconditional;
        private System.Windows.Forms.RadioButton all;
        private System.Windows.Forms.RadioButton any;
        private System.Windows.Forms.Button addCondition;
        private System.Windows.Forms.ToolStripMenuItem remove;
        private System.Windows.Forms.ToolStripMenuItem conditionDelete;
        private System.Windows.Forms.ContextMenuStrip branchMenu;
        private System.Windows.Forms.ContextMenuStrip conditionMenu;

        /// <summary>释放出口、条件行菜单及其他组件。</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>创建出口标题与条件行的固定布局，条件状态由运行时绑定。</summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.layout = new System.Windows.Forms.TableLayoutPanel();
            this.header = new System.Windows.Forms.FlowLayoutPanel();
            this.codeLabel = new System.Windows.Forms.Label();
            this.branchMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.remove = new System.Windows.Forms.ToolStripMenuItem();
            this.targetLabel = new System.Windows.Forms.Label();
            this.target = new System.Windows.Forms.ComboBox();
            this.unconditional = new System.Windows.Forms.RadioButton();
            this.all = new System.Windows.Forms.RadioButton();
            this.any = new System.Windows.Forms.RadioButton();
            this.conditions = new System.Windows.Forms.FlowLayoutPanel();
            this.addCondition = new System.Windows.Forms.Button();
            this.conditionHint = new System.Windows.Forms.Label();
            this.conditionDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.conditionMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.layout.SuspendLayout();
            this.header.SuspendLayout();
            this.branchMenu.SuspendLayout();
            this.conditionMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // layout
            // 
            this.layout.ColumnCount = 1;
            this.layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layout.Controls.Add(this.header, 0, 0);
            this.layout.Controls.Add(this.conditions, 0, 1);
            this.layout.Controls.Add(this.addCondition, 0, 2);
            this.layout.Controls.Add(this.conditionHint, 0, 3);
            this.layout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layout.Location = new System.Drawing.Point(4, 4);
            this.layout.Name = "layout";
            this.layout.RowCount = 4;
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 64F));
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.layout.Size = new System.Drawing.Size(922, 132);
            this.layout.TabIndex = 0;
            // 
            // header
            // 
            this.header.ContextMenuStrip = this.branchMenu;
            this.header.Controls.Add(this.codeLabel);
            this.header.Controls.Add(this.targetLabel);
            this.header.Controls.Add(this.target);
            this.header.Controls.Add(this.unconditional);
            this.header.Controls.Add(this.all);
            this.header.Controls.Add(this.any);
            this.header.Dock = System.Windows.Forms.DockStyle.Fill;
            this.header.Location = new System.Drawing.Point(3, 3);
            this.header.Name = "header";
            this.header.Size = new System.Drawing.Size(916, 58);
            this.header.TabIndex = 0;
            // 
            // codeLabel
            // 
            this.codeLabel.ContextMenuStrip = this.branchMenu;
            this.codeLabel.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.codeLabel.Location = new System.Drawing.Point(3, 0);
            this.codeLabel.Name = "codeLabel_dyn";
            this.codeLabel.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.codeLabel.Size = new System.Drawing.Size(60, 23);
            this.codeLabel.TabIndex = 0;
            this.codeLabel.Text = "NEXT";
            // 
            // branchMenu
            // 
            this.branchMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.remove});
            this.branchMenu.Name = "branchMenu";
            this.branchMenu.Size = new System.Drawing.Size(147, 26);
            // 
            // remove
            // 
            this.remove.Enabled = false;
            this.remove.Name = "removeBranch";
            this.remove.Size = new System.Drawing.Size(146, 22);
            this.remove.Tag = "removeBranch";
            this.remove.Text = "移除此 CASE";
            // 
            // targetLabel
            // 
            this.targetLabel.AutoSize = true;
            this.targetLabel.Location = new System.Drawing.Point(69, 0);
            this.targetLabel.Name = "branchTargetLabel";
            this.targetLabel.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.targetLabel.Size = new System.Drawing.Size(56, 23);
            this.targetLabel.TabIndex = 1;
            this.targetLabel.Tag = "branchTarget";
            this.targetLabel.Text = "跳转目标";
            // 
            // target
            // 
            this.target.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.target.Location = new System.Drawing.Point(131, 3);
            this.target.Name = "branchTargetValue";
            this.target.Size = new System.Drawing.Size(260, 25);
            this.target.TabIndex = 2;
            // 
            // unconditional
            // 
            this.unconditional.AutoSize = true;
            this.unconditional.Checked = true;
            this.unconditional.Location = new System.Drawing.Point(397, 3);
            this.unconditional.Name = "branchUnconditional";
            this.unconditional.Size = new System.Drawing.Size(86, 21);
            this.unconditional.TabIndex = 3;
            this.unconditional.TabStop = true;
            this.unconditional.Tag = "unconditional";
            this.unconditional.Text = "无条件通过";
            // 
            // all
            // 
            this.all.AutoSize = true;
            this.all.Location = new System.Drawing.Point(489, 3);
            this.all.Name = "branchAll";
            this.all.Size = new System.Drawing.Size(137, 21);
            this.all.TabIndex = 4;
            this.all.Tag = "all";
            this.all.Text = "所有条件满足 (AND)";
            // 
            // any
            // 
            this.any.AutoSize = true;
            this.any.Location = new System.Drawing.Point(632, 3);
            this.any.Name = "branchAny";
            this.any.Size = new System.Drawing.Size(128, 21);
            this.any.TabIndex = 5;
            this.any.Tag = "any";
            this.any.Text = "任一条件满足 (OR)";
            // 
            // conditions
            // 
            this.conditions.AutoSize = true;
            this.conditions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.conditions.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.conditions.Location = new System.Drawing.Point(3, 67);
            this.conditions.Name = "branchConditions";
            this.conditions.Size = new System.Drawing.Size(916, 1);
            this.conditions.TabIndex = 1;
            this.conditions.WrapContents = false;
            // 
            // addCondition
            // 
            this.addCondition.AutoSize = true;
            this.addCondition.Dock = System.Windows.Forms.DockStyle.Left;
            this.addCondition.Location = new System.Drawing.Point(3, 73);
            this.addCondition.Name = "addCondition";
            this.addCondition.Size = new System.Drawing.Size(79, 28);
            this.addCondition.TabIndex = 2;
            this.addCondition.Tag = "addCondition";
            this.addCondition.Text = "+ 添加条件";
            // 
            // conditionHint
            // 
            this.conditionHint.AutoEllipsis = true;
            this.conditionHint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.conditionHint.Location = new System.Drawing.Point(3, 104);
            this.conditionHint.Name = "conditionHint";
            this.conditionHint.Size = new System.Drawing.Size(916, 32);
            this.conditionHint.TabIndex = 3;
            this.conditionHint.Tag = "conditionHelp";
            this.conditionHint.Text = "直接选择字段，或输入固定值；true / false 可从下拉选择。右键删除条件。";
            // 
            // conditionDelete
            // 
            this.conditionDelete.Name = "conditionDelete";
            this.conditionDelete.Size = new System.Drawing.Size(124, 22);
            this.conditionDelete.Tag = "deleteCondition";
            this.conditionDelete.Text = "删除条件";
            // 
            // conditionMenu
            // 
            this.conditionMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.conditionDelete});
            this.conditionMenu.Name = "conditionMenu";
            this.conditionMenu.Size = new System.Drawing.Size(125, 26);
            // 
            // FlowBranchEditor
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.layout);
            this.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.Margin = new System.Windows.Forms.Padding(0, 3, 0, 8);
            this.MinimumSize = new System.Drawing.Size(600, 40);
            this.Name = "FlowBranchEditor";
            this.Padding = new System.Windows.Forms.Padding(4);
            this.Size = new System.Drawing.Size(930, 140);
            this.layout.ResumeLayout(false);
            this.layout.PerformLayout();
            this.header.ResumeLayout(false);
            this.header.PerformLayout();
            this.branchMenu.ResumeLayout(false);
            this.conditionMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }
    }
}