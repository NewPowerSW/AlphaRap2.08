namespace AlphaRap.FlowProgramming
{
    partial class FlowCodeBindingsControl
    {
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.DataGridViewTextBoxColumn roleColumn;
        private System.Windows.Forms.DataGridViewComboBoxColumn bindingColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn statusColumn;
        private System.Windows.Forms.Label emptyHint;
        private System.Windows.Forms.ContextMenuStrip bindingMenu;
        private System.Windows.Forms.ToolStripMenuItem clearBinding;

        /// <summary>单层资源与业务字段表，固定布局支持设计器预览。</summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.grid = new System.Windows.Forms.DataGridView();
            this.roleColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.bindingColumn = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.statusColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.emptyHint = new System.Windows.Forms.Label();
            this.bindingMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.clearBinding = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.SuspendLayout();
            this.grid.Name = "codeResourceBindings";
            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.AutoGenerateColumns = false;
            this.grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grid.AllowUserToAddRows = false;
            this.grid.AllowUserToDeleteRows = false;
            this.grid.RowHeadersVisible = false;
            this.grid.MultiSelect = false;
            this.grid.RowTemplate.Height = 30;
            this.roleColumn.Name = "resourceRole";
            this.roleColumn.HeaderText = "模块资源 / 业务字段";
            this.roleColumn.ReadOnly = true;
            this.roleColumn.FillWeight = 32F;
            this.bindingColumn.Name = "resourceBinding";
            this.bindingColumn.HeaderText = "实际对象 / VPP 字段";
            this.bindingColumn.FillWeight = 43F;
            this.statusColumn.Name = "bindingStatus";
            this.statusColumn.HeaderText = "说明";
            this.statusColumn.ReadOnly = true;
            this.statusColumn.FillWeight = 25F;
            this.grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { this.roleColumn, this.bindingColumn, this.statusColumn });
            this.emptyHint.Name = "codeResourcesEmpty";
            this.emptyHint.Text = "此模块不需要绑定设备对象。";
            this.emptyHint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.emptyHint.Padding = new System.Windows.Forms.Padding(8);
            this.emptyHint.ForeColor = System.Drawing.SystemColors.GrayText;
            this.clearBinding.Name = "codeClearBinding";
            this.clearBinding.Text = "清除此绑定";
            this.bindingMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this.clearBinding });
            this.grid.ContextMenuStrip = this.bindingMenu;
            this.Controls.Add(this.grid);
            this.Controls.Add(this.emptyHint);
            this.Name = "FlowCodeBindingsControl";
            this.Size = new System.Drawing.Size(930, 210);
            this.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
