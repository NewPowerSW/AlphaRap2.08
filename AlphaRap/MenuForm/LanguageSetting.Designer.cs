namespace AlphaRap.MenuForm
{
    partial class LanguageSetting
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LanguageSetting));
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.bindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            this.bindingNavigator1 = new System.Windows.Forms.BindingNavigator(this.components);
            this.btnUpdate = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnSave = new System.Windows.Forms.ToolStripButton();
            this.btnCancel = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnRefresh = new System.Windows.Forms.ToolStripButton();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.id = new DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn();
            this.form = new DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn();
            this.objeto = new DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn();
            this.componenttext = new DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn();
            this.spanish = new DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn();
            this.chinese = new DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingNavigator1)).BeginInit();
            this.bindingNavigator1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.id,
            this.form,
            this.objeto,
            this.componenttext,
            this.spanish,
            this.chinese});
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(3, 85);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 62;
            this.dataGridView1.RowTemplate.Height = 28;
            this.dataGridView1.Size = new System.Drawing.Size(1881, 937);
            this.dataGridView1.TabIndex = 0;
            // 
            // bindingNavigator1
            // 
            this.bindingNavigator1.AddNewItem = null;
            this.bindingNavigator1.BackColor = System.Drawing.Color.White;
            this.bindingNavigator1.CountItem = null;
            this.bindingNavigator1.DeleteItem = null;
            this.bindingNavigator1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bindingNavigator1.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.bindingNavigator1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.bindingNavigator1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnUpdate,
            this.toolStripSeparator1,
            this.btnSave,
            this.btnCancel,
            this.toolStripSeparator2,
            this.btnRefresh});
            this.bindingNavigator1.Location = new System.Drawing.Point(0, 0);
            this.bindingNavigator1.MoveFirstItem = null;
            this.bindingNavigator1.MoveLastItem = null;
            this.bindingNavigator1.MoveNextItem = null;
            this.bindingNavigator1.MovePreviousItem = null;
            this.bindingNavigator1.Name = "bindingNavigator1";
            this.bindingNavigator1.PositionItem = null;
            this.bindingNavigator1.Size = new System.Drawing.Size(1887, 82);
            this.bindingNavigator1.TabIndex = 37;
            this.bindingNavigator1.Text = "bindingNavigator1";
            // 
            // btnUpdate
            // 
            this.btnUpdate.Image = ((System.Drawing.Image)(resources.GetObject("btnUpdate.Image")));
            this.btnUpdate.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(86, 77);
            this.btnUpdate.Text = "Update";
            this.btnUpdate.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 82);
            // 
            // btnSave
            // 
            this.btnSave.Image = ((System.Drawing.Image)(resources.GetObject("btnSave.Image")));
            this.btnSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(61, 77);
            this.btnSave.Text = "Save";
            this.btnSave.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Image = ((System.Drawing.Image)(resources.GetObject("btnCancel.Image")));
            this.btnCancel.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(78, 77);
            this.btnCancel.Text = "Cancel";
            this.btnCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 82);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Image = ((System.Drawing.Image)(resources.GetObject("btnRefresh.Image")));
            this.btnRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(87, 77);
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.bindingNavigator1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.dataGridView1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 8F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 92F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1887, 1025);
            this.tableLayoutPanel1.TabIndex = 38;
            // 
            // id
            // 
            this.id.AutomaticSortingEnabled = false;
            this.id.DataPropertyName = "ID";
            this.id.HeaderText = "ID";
            this.id.MinimumWidth = 8;
            this.id.Name = "id";
            this.id.ReadOnly = true;
            this.id.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.id.Width = 150;
            // 
            // form
            // 
            this.form.AutomaticSortingEnabled = false;
            this.form.DataPropertyName = "Form";
            this.form.HeaderText = "Form";
            this.form.MinimumWidth = 8;
            this.form.Name = "form";
            this.form.ReadOnly = true;
            this.form.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.form.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.form.Width = 150;
            // 
            // objeto
            // 
            this.objeto.AutomaticSortingEnabled = false;
            this.objeto.DataPropertyName = "Object";
            this.objeto.HeaderText = "Object";
            this.objeto.MinimumWidth = 8;
            this.objeto.Name = "objeto";
            this.objeto.ReadOnly = true;
            this.objeto.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.objeto.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.objeto.Width = 150;
            // 
            // componenttext
            // 
            this.componenttext.AutomaticSortingEnabled = false;
            this.componenttext.DataPropertyName = "English";
            this.componenttext.HeaderText = "English";
            this.componenttext.MinimumWidth = 8;
            this.componenttext.Name = "componenttext";
            this.componenttext.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.componenttext.Width = 300;
            // 
            // spanish
            // 
            this.spanish.AutomaticSortingEnabled = false;
            this.spanish.DataPropertyName = "Spanish";
            this.spanish.HeaderText = "Spanish";
            this.spanish.MinimumWidth = 8;
            this.spanish.Name = "spanish";
            this.spanish.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.spanish.Width = 300;
            // 
            // chinese
            // 
            this.chinese.AutomaticSortingEnabled = false;
            this.chinese.DataPropertyName = "Chinese";
            this.chinese.HeaderText = "Chinese";
            this.chinese.MinimumWidth = 8;
            this.chinese.Name = "chinese";
            this.chinese.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.chinese.Width = 300;
            // 
            // LanguageSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1887, 1025);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "LanguageSetting";
            this.Text = "LanguageSetting";
            this.Load += new System.EventHandler(this.LanguageSetting_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingNavigator1)).EndInit();
            this.bindingNavigator1.ResumeLayout(false);
            this.bindingNavigator1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.BindingSource bindingSource1;
        private System.Windows.Forms.BindingNavigator bindingNavigator1;
        private System.Windows.Forms.ToolStripButton btnUpdate;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnSave;
        private System.Windows.Forms.ToolStripButton btnCancel;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton btnRefresh;
        private DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn id;
        private DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn form;
        private DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn objeto;
        private DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn componenttext;
        private DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn spanish;
        private DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn chinese;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}