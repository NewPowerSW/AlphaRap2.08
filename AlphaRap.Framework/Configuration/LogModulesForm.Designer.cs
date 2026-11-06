namespace AlphaRap.Configuration
{
    partial class LogModulesForm
    {
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.Button add, browse, ok, cancel;
        private System.Windows.Forms.ListBox moduleList;
        private System.Windows.Forms.TextBox moduleName, directory;
        private System.Windows.Forms.Label nameLabel, directoryLabel, hint;
        private System.Windows.Forms.DataGridView fields;
        private System.Windows.Forms.DataGridViewTextBoxColumn fieldName;
        private System.Windows.Forms.DataGridViewComboBoxColumn fieldType;
        private System.Windows.Forms.ContextMenuStrip moduleMenu, fieldMenu;
        private System.Windows.Forms.ToolStripMenuItem moduleDelete, fieldDelete;

        /// <summary>释放模块和字段右键菜单。</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>初始化单层模块、目录和字段编辑布局。</summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.add = new System.Windows.Forms.Button();
            this.browse = new System.Windows.Forms.Button();
            this.ok = new System.Windows.Forms.Button();
            this.cancel = new System.Windows.Forms.Button();
            this.moduleList = new System.Windows.Forms.ListBox();
            this.moduleName = new System.Windows.Forms.TextBox();
            this.directory = new System.Windows.Forms.TextBox();
            this.nameLabel = new System.Windows.Forms.Label();
            this.directoryLabel = new System.Windows.Forms.Label();
            this.hint = new System.Windows.Forms.Label();
            this.fields = new System.Windows.Forms.DataGridView();
            this.fieldName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fieldType = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.moduleMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.fieldMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.moduleDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.fieldDelete = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.fields)).BeginInit();
            this.SuspendLayout();
            this.add.Name = "logAdd";
            this.add.Tag = "logAdd";
            this.add.Text = "新增日志模块";
            this.add.Location = new System.Drawing.Point(12, 12);
            this.add.Size = new System.Drawing.Size(210, 32);
            this.moduleList.Name = "logModuleList";
            this.moduleList.Location = new System.Drawing.Point(12, 54);
            this.moduleList.Size = new System.Drawing.Size(210, 382);
            this.moduleList.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            this.moduleList.ContextMenuStrip = this.moduleMenu;
            this.moduleMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this.moduleDelete });
            this.moduleDelete.Name = "logModuleDelete";
            this.moduleDelete.Tag = "logDelete";
            this.moduleDelete.Text = "删除";
            this.nameLabel.Name = "logNameLabel";
            this.nameLabel.Tag = "logName";
            this.nameLabel.Text = "模块名称";
            this.nameLabel.Location = new System.Drawing.Point(240, 18);
            this.nameLabel.Size = new System.Drawing.Size(135, 24);
            this.moduleName.Name = "logModuleName";
            this.moduleName.Location = new System.Drawing.Point(380, 15);
            this.moduleName.Size = new System.Drawing.Size(492, 24);
            this.moduleName.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.directoryLabel.Name = "logDirectoryLabel";
            this.directoryLabel.Tag = "logDirectory";
            this.directoryLabel.Text = "保存目录";
            this.directoryLabel.Location = new System.Drawing.Point(240, 61);
            this.directoryLabel.Size = new System.Drawing.Size(135, 24);
            this.directory.Name = "logDirectoryValue";
            this.directory.Location = new System.Drawing.Point(380, 57);
            this.directory.Size = new System.Drawing.Size(446, 24);
            this.directory.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.browse.Name = "logBrowse";
            this.browse.Text = "...";
            this.browse.Location = new System.Drawing.Point(832, 54);
            this.browse.Size = new System.Drawing.Size(40, 29);
            this.browse.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.fields.Name = "logFields";
            this.fields.Location = new System.Drawing.Point(240, 98);
            this.fields.Size = new System.Drawing.Size(632, 338);
            this.fields.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.fields.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.fields.BackgroundColor = System.Drawing.Color.White;
            this.fields.AllowUserToDeleteRows = false;
            this.fields.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { this.fieldName, this.fieldType });
            this.fields.ContextMenuStrip = this.fieldMenu;
            this.fieldName.Name = "fieldName";
            this.fieldName.Tag = "name";
            this.fieldName.HeaderText = "字段名称";
            this.fieldName.FillWeight = 65;
            this.fieldType.Name = "fieldType";
            this.fieldType.Tag = "fieldType";
            this.fieldType.HeaderText = "类型";
            this.fieldType.FillWeight = 35;
            this.fieldType.Items.AddRange(new object[] { "Text", "Number", "Boolean" });
            this.fieldMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this.fieldDelete });
            this.fieldDelete.Name = "logFieldDelete";
            this.fieldDelete.Tag = "deleteField";
            this.fieldDelete.Text = "删除";
            this.hint.Name = "logManagerHint";
            this.hint.Tag = "logManagerHint";
            this.hint.Text = "Time 自动记录时间；内置日志保留。配置统一使用主界面保存。";
            this.hint.Location = new System.Drawing.Point(12, 450);
            this.hint.Size = new System.Drawing.Size(860, 54);
            this.hint.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.ok.Name = "logOk";
            this.ok.Tag = "ok";
            this.ok.Text = "确定";
            this.ok.Location = new System.Drawing.Point(664, 510);
            this.ok.Size = new System.Drawing.Size(100, 32);
            this.ok.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            this.cancel.Name = "logCancel";
            this.cancel.Tag = "cancel";
            this.cancel.Text = "取消";
            this.cancel.Location = new System.Drawing.Point(774, 510);
            this.cancel.Size = new System.Drawing.Size(100, 32);
            this.cancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.AcceptButton = this.ok;
            this.CancelButton = this.cancel;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(888, 554);
            this.MinimumSize = new System.Drawing.Size(900, 590);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.Name = "LogModulesForm";
            this.Tag = "logModules";
            this.Text = "日志模块";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Controls.AddRange(new System.Windows.Forms.Control[] { this.add, this.moduleList, this.nameLabel, this.moduleName, this.directoryLabel, this.directory, this.browse, this.fields, this.hint, this.ok, this.cancel });
            ((System.ComponentModel.ISupportInitialize)(this.fields)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
