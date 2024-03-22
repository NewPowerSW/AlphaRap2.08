namespace AlphaRap.MenuForm
{
    partial class AlarmSetting
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AlarmSetting));
            this.dgvData = new System.Windows.Forms.DataGridView();
            this.bindingNavigator1 = new System.Windows.Forms.BindingNavigator(this.components);
            this.btnAdd = new System.Windows.Forms.ToolStripButton();
            this.btnUpdate = new System.Windows.Forms.ToolStripButton();
            this.btnRemove = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnSave = new System.Windows.Forms.ToolStripButton();
            this.btnCancel = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnRefresh = new System.Windows.Forms.ToolStripButton();
            this.bindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            this.cell_Index = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cell_AlarmID = new DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn();
            this.cell_DoStop = new DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn();
            this.cell_Type = new DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn();
            this.cell_Content = new DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn();
            this.cell_EContent = new DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn();
            this.cell_SPContent = new DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingNavigator1)).BeginInit();
            this.bindingNavigator1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvData
            // 
            this.dgvData.AllowUserToAddRows = false;
            this.dgvData.BackgroundColor = System.Drawing.Color.White;
            this.dgvData.ColumnHeadersHeight = 30;
            this.dgvData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.cell_Index,
            this.cell_AlarmID,
            this.cell_DoStop,
            this.cell_Type,
            this.cell_Content,
            this.cell_EContent,
            this.cell_SPContent});
            this.dgvData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvData.Location = new System.Drawing.Point(10, 57);
            this.dgvData.Name = "dgvData";
            this.dgvData.RowHeadersWidth = 30;
            this.dgvData.RowTemplate.Height = 23;
            this.dgvData.Size = new System.Drawing.Size(1204, 841);
            this.dgvData.TabIndex = 1;
            this.dgvData.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.dgvData_CellValidating);
            // 
            // bindingNavigator1
            // 
            this.bindingNavigator1.AddNewItem = null;
            this.bindingNavigator1.BackColor = System.Drawing.Color.White;
            this.bindingNavigator1.CountItem = null;
            this.bindingNavigator1.DeleteItem = null;
            this.bindingNavigator1.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.bindingNavigator1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.bindingNavigator1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnAdd,
            this.btnUpdate,
            this.btnRemove,
            this.toolStripSeparator1,
            this.btnSave,
            this.btnCancel,
            this.toolStripSeparator2,
            this.btnRefresh});
            this.bindingNavigator1.Location = new System.Drawing.Point(10, 10);
            this.bindingNavigator1.MoveFirstItem = null;
            this.bindingNavigator1.MoveLastItem = null;
            this.bindingNavigator1.MoveNextItem = null;
            this.bindingNavigator1.MovePreviousItem = null;
            this.bindingNavigator1.Name = "bindingNavigator1";
            this.bindingNavigator1.PositionItem = null;
            this.bindingNavigator1.Size = new System.Drawing.Size(1204, 47);
            this.bindingNavigator1.TabIndex = 36;
            this.bindingNavigator1.Text = "bindingNavigator1";
            // 
            // btnAdd
            // 
            this.btnAdd.Image = ((System.Drawing.Image)(resources.GetObject("btnAdd.Image")));
            this.btnAdd.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(41, 44);
            this.btnAdd.Text = "Add";
            this.btnAdd.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnUpdate
            // 
            this.btnUpdate.Image = ((System.Drawing.Image)(resources.GetObject("btnUpdate.Image")));
            this.btnUpdate.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(62, 44);
            this.btnUpdate.Text = "Update";
            this.btnUpdate.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.Image = ((System.Drawing.Image)(resources.GetObject("btnRemove.Image")));
            this.btnRemove.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(67, 44);
            this.btnRemove.Text = "Remove";
            this.btnRemove.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 47);
            // 
            // btnSave
            // 
            this.btnSave.Image = ((System.Drawing.Image)(resources.GetObject("btnSave.Image")));
            this.btnSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(44, 44);
            this.btnSave.Text = "Save";
            this.btnSave.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Image = ((System.Drawing.Image)(resources.GetObject("btnCancel.Image")));
            this.btnCancel.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(58, 44);
            this.btnCancel.Text = "Cancel";
            this.btnCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 47);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Image = ((System.Drawing.Image)(resources.GetObject("btnRefresh.Image")));
            this.btnRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(63, 44);
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // cell_Index
            // 
            this.cell_Index.DataPropertyName = "Index";
            this.cell_Index.HeaderText = "Index";
            this.cell_Index.Name = "cell_Index";
            this.cell_Index.ReadOnly = true;
            this.cell_Index.Width = 50;
            // 
            // cell_AlarmID
            // 
            this.cell_AlarmID.DataPropertyName = "AlarID";
            this.cell_AlarmID.HeaderText = "Alarm ID";
            this.cell_AlarmID.Name = "cell_AlarmID";
            this.cell_AlarmID.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.cell_AlarmID.Width = 80;
            // 
            // cell_DoStop
            // 
            this.cell_DoStop.DataPropertyName = "DoStop";
            this.cell_DoStop.HeaderText = "DoStop";
            this.cell_DoStop.Name = "cell_DoStop";
            this.cell_DoStop.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.cell_DoStop.Width = 80;
            // 
            // cell_Type
            // 
            this.cell_Type.DataPropertyName = "Type";
            this.cell_Type.HeaderText = "Type";
            this.cell_Type.Name = "cell_Type";
            this.cell_Type.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.cell_Type.Width = 60;
            // 
            // cell_Content
            // 
            this.cell_Content.DataPropertyName = "Content";
            this.cell_Content.HeaderText = "Content";
            this.cell_Content.Name = "cell_Content";
            this.cell_Content.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.cell_Content.Width = 300;
            // 
            // cell_EContent
            // 
            this.cell_EContent.DataPropertyName = "EContent";
            this.cell_EContent.HeaderText = "EContent";
            this.cell_EContent.Name = "cell_EContent";
            this.cell_EContent.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.cell_EContent.Width = 300;
            // 
            // cell_SPContent
            // 
            this.cell_SPContent.DataPropertyName = "SPContent";
            this.cell_SPContent.HeaderText = "SPContent";
            this.cell_SPContent.Name = "cell_SPContent";
            this.cell_SPContent.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.cell_SPContent.Width = 300;
            // 
            // AlarmSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1224, 908);
            this.Controls.Add(this.dgvData);
            this.Controls.Add(this.bindingNavigator1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AlarmSetting";
            this.Padding = new System.Windows.Forms.Padding(10, 10, 10, 10);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AlarmSetting";
            this.Load += new System.EventHandler(this.AlarmSetting_Load);
            this.Shown += new System.EventHandler(this.AlarmSetting_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.dgvData)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingNavigator1)).EndInit();
            this.bindingNavigator1.ResumeLayout(false);
            this.bindingNavigator1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.DataGridView dgvData;
        private System.Windows.Forms.BindingNavigator bindingNavigator1;
        private System.Windows.Forms.ToolStripButton btnAdd;
        private System.Windows.Forms.ToolStripButton btnUpdate;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnSave;
        private System.Windows.Forms.ToolStripButton btnCancel;
        private System.Windows.Forms.ToolStripButton btnRemove;
        private System.Windows.Forms.BindingSource bindingSource1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton btnRefresh;
        private System.Windows.Forms.DataGridViewTextBoxColumn cell_Index;
        private DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn cell_AlarmID;
        private DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn cell_DoStop;
        private DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn cell_Type;
        private DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn cell_Content;
        private DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn cell_EContent;
        private DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn cell_SPContent;
    }
}