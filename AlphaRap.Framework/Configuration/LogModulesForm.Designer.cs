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
            this.moduleMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.moduleDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.moduleName = new System.Windows.Forms.TextBox();
            this.directory = new System.Windows.Forms.TextBox();
            this.nameLabel = new System.Windows.Forms.Label();
            this.directoryLabel = new System.Windows.Forms.Label();
            this.hint = new System.Windows.Forms.Label();
            this.fields = new System.Windows.Forms.DataGridView();
            this.fieldName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fieldType = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.fieldMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.fieldDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.moduleMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fields)).BeginInit();
            this.fieldMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // add
            // 
            this.add.Location = new System.Drawing.Point(12, 12);
            this.add.Name = "add";
            this.add.Size = new System.Drawing.Size(210, 32);
            this.add.TabIndex = 0;
            this.add.Tag = "logAdd";
            this.add.Text = "新增日志模块";
            // 
            // browse
            // 
            this.browse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browse.Location = new System.Drawing.Point(832, 54);
            this.browse.Name = "browse";
            this.browse.Size = new System.Drawing.Size(40, 29);
            this.browse.TabIndex = 6;
            this.browse.Text = "...";
            // 
            // ok
            // 
            this.ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ok.Location = new System.Drawing.Point(664, 510);
            this.ok.Name = "ok";
            this.ok.Size = new System.Drawing.Size(100, 32);
            this.ok.TabIndex = 9;
            this.ok.Tag = "ok";
            this.ok.Text = "确定";
            // 
            // cancel
            // 
            this.cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancel.Location = new System.Drawing.Point(774, 510);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(100, 32);
            this.cancel.TabIndex = 10;
            this.cancel.Tag = "cancel";
            this.cancel.Text = "取消";
            // 
            // moduleList
            // 
            this.moduleList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.moduleList.ContextMenuStrip = this.moduleMenu;
            this.moduleList.ItemHeight = 17;
            this.moduleList.Location = new System.Drawing.Point(12, 54);
            this.moduleList.Name = "moduleList";
            this.moduleList.Size = new System.Drawing.Size(210, 378);
            this.moduleList.TabIndex = 1;
            // 
            // moduleMenu
            // 
            this.moduleMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.moduleDelete});
            this.moduleMenu.Name = "moduleMenu";
            this.moduleMenu.Size = new System.Drawing.Size(101, 26);
            // 
            // moduleDelete
            // 
            this.moduleDelete.Name = "moduleDelete";
            this.moduleDelete.Size = new System.Drawing.Size(100, 22);
            this.moduleDelete.Tag = "logDelete";
            this.moduleDelete.Text = "删除";
            // 
            // moduleName
            // 
            this.moduleName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.moduleName.Location = new System.Drawing.Point(380, 15);
            this.moduleName.Name = "moduleName";
            this.moduleName.Size = new System.Drawing.Size(492, 23);
            this.moduleName.TabIndex = 3;
            // 
            // directory
            // 
            this.directory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.directory.Location = new System.Drawing.Point(380, 57);
            this.directory.Name = "directory";
            this.directory.Size = new System.Drawing.Size(446, 23);
            this.directory.TabIndex = 5;
            // 
            // nameLabel
            // 
            this.nameLabel.Location = new System.Drawing.Point(240, 18);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(135, 24);
            this.nameLabel.TabIndex = 2;
            this.nameLabel.Tag = "logName";
            this.nameLabel.Text = "模块名称";
            // 
            // directoryLabel
            // 
            this.directoryLabel.Location = new System.Drawing.Point(240, 61);
            this.directoryLabel.Name = "directoryLabel";
            this.directoryLabel.Size = new System.Drawing.Size(135, 24);
            this.directoryLabel.TabIndex = 4;
            this.directoryLabel.Tag = "logDirectory";
            this.directoryLabel.Text = "保存目录";
            // 
            // hint
            // 
            this.hint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hint.Location = new System.Drawing.Point(12, 450);
            this.hint.Name = "hint";
            this.hint.Size = new System.Drawing.Size(860, 54);
            this.hint.TabIndex = 8;
            this.hint.Tag = "logManagerHint";
            this.hint.Text = "Time 自动记录时间；内置日志保留。配置统一使用主界面保存。";
            // 
            // fields
            // 
            this.fields.AllowUserToDeleteRows = false;
            this.fields.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fields.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.fields.BackgroundColor = System.Drawing.Color.White;
            this.fields.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.fieldName,
            this.fieldType});
            this.fields.ContextMenuStrip = this.fieldMenu;
            this.fields.Location = new System.Drawing.Point(240, 98);
            this.fields.Name = "fields";
            this.fields.Size = new System.Drawing.Size(632, 338);
            this.fields.TabIndex = 7;
            // 
            // fieldName
            // 
            this.fieldName.FillWeight = 65F;
            this.fieldName.HeaderText = "字段名称";
            this.fieldName.Name = "fieldName";
            this.fieldName.Tag = "name";
            // 
            // fieldType
            // 
            this.fieldType.FillWeight = 35F;
            this.fieldType.HeaderText = "类型";
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
            this.fieldMenu.Size = new System.Drawing.Size(101, 26);
            // 
            // fieldDelete
            // 
            this.fieldDelete.Name = "fieldDelete";
            this.fieldDelete.Size = new System.Drawing.Size(100, 22);
            this.fieldDelete.Tag = "deleteField";
            this.fieldDelete.Text = "删除";
            // 
            // LogModulesForm
            // 
            this.AcceptButton = this.ok;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this.cancel;
            this.ClientSize = new System.Drawing.Size(888, 554);
            this.Controls.Add(this.add);
            this.Controls.Add(this.moduleList);
            this.Controls.Add(this.nameLabel);
            this.Controls.Add(this.moduleName);
            this.Controls.Add(this.directoryLabel);
            this.Controls.Add(this.directory);
            this.Controls.Add(this.browse);
            this.Controls.Add(this.fields);
            this.Controls.Add(this.hint);
            this.Controls.Add(this.ok);
            this.Controls.Add(this.cancel);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.MinimumSize = new System.Drawing.Size(900, 590);
            this.Name = "LogModulesForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "logModules";
            this.Text = "日志模块";
            this.moduleMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.fields)).EndInit();
            this.fieldMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
