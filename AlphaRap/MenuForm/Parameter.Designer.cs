namespace AlphaRap
{
    partial class Parameter
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
            this.dataTable1 = new System.Data.DataTable();
            this.dataColumn1 = new System.Data.DataColumn();
            this.dataColumn2 = new System.Data.DataColumn();
            this.dataColumn3 = new System.Data.DataColumn();
            this.dataColumn4 = new System.Data.DataColumn();
            this.dataColumn5 = new System.Data.DataColumn();
            this.dataTable2 = new System.Data.DataTable();
            this.panelHost = new System.Windows.Forms.Panel();
            this.btnAddDevice = new AlphaRap.FlatButton();
            this.tabDevices = new System.Windows.Forms.TabControl();
            this.panelHost.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SettingData)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.RecipeData)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataTable1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataTable2)).BeginInit();
            this.SuspendLayout();
            // 
            // SettingData
            // 
            this.SettingData.Tables.AddRange(new System.Data.DataTable[] {
            this.dataTable1,
            this.dataTable2});
            // 
            // dataTable1
            // 
            this.dataTable1.Columns.AddRange(new System.Data.DataColumn[] {
            this.dataColumn1,
            this.dataColumn2,
            this.dataColumn3,
            this.dataColumn4,
            this.dataColumn5});
            this.dataTable1.TableName = "MSet";
            // 
            // dataColumn1
            // 
            this.dataColumn1.ColumnName = "PLCIP";
            this.dataColumn1.DefaultValue = "192.168.10.10";
            // 
            // dataColumn2
            // 
            this.dataColumn2.ColumnName = "PLCPort";
            this.dataColumn2.DataType = typeof(int);
            this.dataColumn2.DefaultValue = 502;
            // 
            // dataColumn3
            // 
            this.dataColumn3.ColumnName = "ScannIP";
            // 
            // dataColumn4
            // 
            this.dataColumn4.ColumnName = "ScannPort";
            this.dataColumn4.DataType = typeof(int);
            this.dataColumn4.DefaultValue = 1024;
            // 
            // dataColumn5
            // 
            this.dataColumn5.ColumnName = "Pressure_COM";
            // 
            // dataTable2
            // 
            this.dataTable2.TableName = "PSet";
            // 
            // panelHost（顶部工具条：添加设备）
            // 
            this.panelHost.BackColor = System.Drawing.Color.White;
            this.panelHost.Controls.Add(this.btnAddDevice);
            this.panelHost.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHost.Location = new System.Drawing.Point(0, 0);
            this.panelHost.Name = "panelHost";
            this.panelHost.Size = new System.Drawing.Size(1192, 46);
            this.panelHost.TabIndex = 0;
            // 
            // btnAddDevice
            // 
            this.btnAddDevice.BackColor = System.Drawing.Color.Transparent;
            this.btnAddDevice.CornerRadius = 6;
            this.btnAddDevice.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddDevice.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnAddDevice.Icon = null;
            this.btnAddDevice.IconGap = 8;
            this.btnAddDevice.IconSize = 18;
            this.btnAddDevice.Location = new System.Drawing.Point(10, 8);
            this.btnAddDevice.Name = "btnAddDevice";
            this.btnAddDevice.Selectable = false;
            this.btnAddDevice.Size = new System.Drawing.Size(132, 30);
            this.btnAddDevice.TabIndex = 0;
            this.btnAddDevice.TabStop = false;
            this.btnAddDevice.Text = "添加设备";
            this.btnAddDevice.Variant = AlphaRap.FlatButtonVariant.Primary;
            this.btnAddDevice.Click += new System.EventHandler(this.btnAddDevice_Click);
            // 
            // tabDevices（设备页签容器：一个页签 = 一台设备，横向排列）
            // 
            this.tabDevices.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabDevices.Location = new System.Drawing.Point(0, 46);
            this.tabDevices.Name = "tabDevices";
            this.tabDevices.Size = new System.Drawing.Size(1192, 977);
            this.tabDevices.TabIndex = 1;
            // 
            // Parameter
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1192, 1023);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "Parameter";
            this.Text = " ";
            this.Controls.Add(this.tabDevices);
            this.Controls.Add(this.panelHost);
            ((System.ComponentModel.ISupportInitialize)(this.SettingData)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.RecipeData)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataTable1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataTable2)).EndInit();
            this.panelHost.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Data.DataTable dataTable1;
        private System.Data.DataTable dataTable2;
		private System.Data.DataColumn dataColumn1;
		private System.Data.DataColumn dataColumn2;
		private System.Data.DataColumn dataColumn3;
		private System.Data.DataColumn dataColumn4;
		private System.Data.DataColumn dataColumn5;
        private System.Windows.Forms.Panel panelHost;
        private FlatButton btnAddDevice;
        private System.Windows.Forms.TabControl tabDevices;
    }
}