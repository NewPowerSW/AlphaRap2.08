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
            this.conditions = new System.Windows.Forms.FlowLayoutPanel();
            this.codeLabel = new System.Windows.Forms.Label();
            this.targetLabel = new System.Windows.Forms.Label();
            this.conditionHint = new System.Windows.Forms.Label();
            this.target = new System.Windows.Forms.ComboBox();
            this.unconditional = new System.Windows.Forms.RadioButton();
            this.all = new System.Windows.Forms.RadioButton();
            this.any = new System.Windows.Forms.RadioButton();
            this.addCondition = new System.Windows.Forms.Button();
            this.remove = new System.Windows.Forms.ToolStripMenuItem();
            this.conditionDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.branchMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
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
            this.layout.Name = "layout";
            this.layout.RowCount = 4;
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 64F));
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            //
            // header
            //
            this.header.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.codeLabel,
                this.targetLabel,
                this.target,
                this.unconditional,
                this.all,
                this.any});
            this.header.ContextMenuStrip = this.branchMenu;
            this.header.Dock = System.Windows.Forms.DockStyle.Fill;
            this.header.Name = "header";
            this.header.WrapContents = true;
            //
            // codeLabel
            //
            this.codeLabel.ContextMenuStrip = this.branchMenu;
            this.codeLabel.Font = new System.Drawing.Font("Microsoft YaHei", 10F, System.Drawing.FontStyle.Bold);
            this.codeLabel.Name = "codeLabel_dyn";
            this.codeLabel.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.codeLabel.Text = "NEXT";
            this.codeLabel.Width = 60;
            //
            // targetLabel
            //
            this.targetLabel.AutoSize = true;
            this.targetLabel.Name = "branchTargetLabel";
            this.targetLabel.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.targetLabel.Tag = "branchTarget";
            this.targetLabel.Text = "跳转目标";
            //
            // target
            //
            this.target.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.target.Name = "branchTargetValue";
            this.target.Width = 260;
            //
            // unconditional
            //
            this.unconditional.AutoSize = true;
            this.unconditional.Checked = true;
            this.unconditional.Name = "branchUnconditional";
            this.unconditional.Tag = "unconditional";
            this.unconditional.Text = "无条件通过";
            //
            // all
            //
            this.all.AutoSize = true;
            this.all.Name = "branchAll";
            this.all.Tag = "all";
            this.all.Text = "所有条件满足 (AND)";
            //
            // any
            //
            this.any.AutoSize = true;
            this.any.Name = "branchAny";
            this.any.Tag = "any";
            this.any.Text = "任一条件满足 (OR)";
            //
            // conditions
            //
            this.conditions.AutoSize = true;
            this.conditions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.conditions.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.conditions.Name = "branchConditions";
            this.conditions.WrapContents = false;
            //
            // addCondition
            //
            this.addCondition.AutoSize = true;
            this.addCondition.Dock = System.Windows.Forms.DockStyle.Left;
            this.addCondition.Name = "addCondition";
            this.addCondition.Tag = "addCondition";
            this.addCondition.Text = "+ 添加条件";
            //
            // conditionHint
            //
            this.conditionHint.AutoEllipsis = true;
            this.conditionHint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.conditionHint.Name = "conditionHint";
            this.conditionHint.Tag = "conditionHelp";
            this.conditionHint.Text = "直接选择字段，或输入固定值；true / false 可从下拉选择。右键删除条件。";
            //
            // remove
            //
            this.remove.Enabled = false;
            this.remove.Name = "removeBranch";
            this.remove.Tag = "removeBranch";
            this.remove.Text = "移除此 CASE";
            //
            // conditionDelete
            //
            this.conditionDelete.Name = "conditionDelete";
            this.conditionDelete.Tag = "deleteCondition";
            this.conditionDelete.Text = "删除条件";
            //
            // branchMenu
            //
            this.branchMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this.remove });
            this.branchMenu.Name = "branchMenu";
            //
            // conditionMenu
            //
            this.conditionMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this.conditionDelete });
            this.conditionMenu.Name = "conditionMenu";
            //
            // FlowBranchEditor
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.layout);
            this.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
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