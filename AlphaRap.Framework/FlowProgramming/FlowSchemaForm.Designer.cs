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
            this.SuspendLayout();
            this.fields = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.fields)).BeginInit();
            this.fields.Name = "productionFields";
            this.fields.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fields.AutoGenerateColumns = false;
            this.fields.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.fields.AllowUserToAddRows = true;
            this.fields.AllowUserToDeleteRows = false;
            this.fieldMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.fieldMenu.SuspendLayout();
            this.fieldMenu.Name = "fieldMenu";
            this.fieldDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.fieldDelete.Name = "fieldDelete";
            this.fieldDelete.Tag = "deleteField";
            this.fieldDelete.Text = "删除字段";
            this.fieldMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this.fieldDelete });
            this.fields.ContextMenuStrip = this.fieldMenu;
            this.fieldName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fieldName.Name = "fieldName";
            this.fieldName.Tag = "name";
            this.fieldName.HeaderText = "字段";
            this.fieldType = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.fieldType.Name = "fieldType";
            this.fieldType.Tag = "fieldType";
            this.fieldType.HeaderText = "数据类型";
            this.fieldType.Items.AddRange(new object[] { "Text", "Number", "Boolean" });
            this.fields.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { this.fieldName, this.fieldType });
            this.buttons = new System.Windows.Forms.FlowLayoutPanel();
            this.buttons.SuspendLayout();
            this.buttons.Name = "buttons";
            this.buttons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttons.Height = 44;
            this.buttons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.buttons.Padding = new System.Windows.Forms.Padding(6);
            this.ok = new System.Windows.Forms.Button();
            this.ok.Name = "ok";
            this.ok.Tag = "ok";
            this.ok.Text = "确定";
            this.ok.AutoSize = true;
            this.cancel = new System.Windows.Forms.Button();
            this.cancel.Name = "cancel";
            this.cancel.Tag = "cancel";
            this.cancel.Text = "取消";
            this.cancel.AutoSize = true;
            this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.AcceptButton = this.ok;
            this.CancelButton = this.cancel;
            this.buttons.Controls.Add(this.cancel);
            this.buttons.Controls.Add(this.ok);
            this.Controls.Add(this.fields);
            this.Controls.Add(this.buttons);
            this.Name = "FlowSchemaForm";
            this.Tag = "title";
            this.Text = "流程工程";
            this.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(700, 500);
            this.MinimumSize = new System.Drawing.Size(700, 500);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.ShowInTaskbar = false;
            this.MaximizeBox = false;
            this.buttons.ResumeLayout(false);
            this.buttons.PerformLayout();
            this.fieldMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.fields)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
