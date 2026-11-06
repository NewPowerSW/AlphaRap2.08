namespace AlphaRap.FlowProgramming
{
    partial class FlowSchemaForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView fields;
        private System.Windows.Forms.DataGridViewTextBoxColumn fieldName;
        private System.Windows.Forms.DataGridViewComboBoxColumn fieldType;
        private System.Windows.Forms.Button ok;
        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.FlowLayoutPanel buttons;
        private System.Windows.Forms.ContextMenuStrip fieldMenu;
        private System.Windows.Forms.ToolStripMenuItem fieldDelete;
        /// <summary>释放生产字段的行菜单。</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        /// <summary>创建设计器与运行时共用的生产字段表和操作按钮。</summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.fields = new System.Windows.Forms.DataGridView();
            this.fieldName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fieldType = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.fieldMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.fieldDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.buttons = new System.Windows.Forms.FlowLayoutPanel();
            this.cancel = new System.Windows.Forms.Button();
            this.ok = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.fields)).BeginInit();
            this.fieldMenu.SuspendLayout();
            this.buttons.SuspendLayout();
            this.SuspendLayout();
            // 
            // fields
            // 
            this.fields.AllowUserToDeleteRows = false;
            this.fields.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.fields.AutoGenerateColumns = false;
            this.fields.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.fieldName,
            this.fieldType});
            this.fields.ContextMenuStrip = this.fieldMenu;
            this.fields.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fields.Location = new System.Drawing.Point(0, 0);
            this.fields.Name = "productionFields";
            this.fields.Size = new System.Drawing.Size(700, 456);
            this.fields.TabIndex = 0;
            // 
            // fieldName
            // 
            this.fieldName.HeaderText = "字段";
            this.fieldName.Name = "fieldName";
            this.fieldName.Tag = "name";
            // 
            // fieldType
            // 
            this.fieldType.HeaderText = "数据类型";
            this.fieldType.Items.AddRange(new object[] {
            "Text",
            "Number",
            "Boolean"});
            this.fieldType.Name = "fieldType";
            this.fieldType.Tag = "fieldType";
            // 
            // fieldMenu
            // 
            this.fieldMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fieldDelete});
            this.fieldMenu.Name = "fieldMenu";
            this.fieldMenu.Size = new System.Drawing.Size(125, 26);
            // 
            // fieldDelete
            // 
            this.fieldDelete.Name = "fieldDelete";
            this.fieldDelete.Size = new System.Drawing.Size(124, 22);
            this.fieldDelete.Tag = "deleteField";
            this.fieldDelete.Text = "删除字段";
            // 
            // buttons
            // 
            this.buttons.Controls.Add(this.cancel);
            this.buttons.Controls.Add(this.ok);
            this.buttons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.buttons.Location = new System.Drawing.Point(0, 456);
            this.buttons.Name = "buttons";
            this.buttons.Padding = new System.Windows.Forms.Padding(6);
            this.buttons.Size = new System.Drawing.Size(700, 44);
            this.buttons.TabIndex = 1;
            // 
            // cancel
            // 
            this.cancel.AutoSize = true;
            this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancel.Location = new System.Drawing.Point(610, 9);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(75, 27);
            this.cancel.TabIndex = 0;
            this.cancel.Tag = "cancel";
            this.cancel.Text = "取消";
            // 
            // ok
            // 
            this.ok.AutoSize = true;
            this.ok.Location = new System.Drawing.Point(529, 9);
            this.ok.Name = "ok";
            this.ok.Size = new System.Drawing.Size(75, 27);
            this.ok.TabIndex = 1;
            this.ok.Tag = "ok";
            this.ok.Text = "确定";
            // 
            // FlowSchemaForm
            // 
            this.AcceptButton = this.ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.cancel;
            this.ClientSize = new System.Drawing.Size(700, 500);
            this.Controls.Add(this.fields);
            this.Controls.Add(this.buttons);
            this.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(700, 500);
            this.Name = "FlowSchemaForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "title";
            this.Text = "流程工程";
            ((System.ComponentModel.ISupportInitialize)(this.fields)).EndInit();
            this.fieldMenu.ResumeLayout(false);
            this.buttons.ResumeLayout(false);
            this.buttons.PerformLayout();
            this.ResumeLayout(false);

        }
    }
}
