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
        this.deviceControl1 = new AlphaRap.DeviceControl();
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
        // deviceControl1
        // 
            this.deviceControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.deviceControl1.Font = new System.Drawing.Font("宋体", 10F);
            this.deviceControl1.Location = new System.Drawing.Point(0, 0);
            this.deviceControl1.Name = "deviceControl1";
            this.deviceControl1.Size = new System.Drawing.Size(1192, 1023);
            this.deviceControl1.TabIndex = 0;
            this.Controls.Add(this.deviceControl1);
            // 
            // Parameter
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1192, 1023);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "Parameter";
            this.Text = " ";
            ((System.ComponentModel.ISupportInitialize)(this.SettingData)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.RecipeData)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataTable1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataTable2)).EndInit();
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
		private DeviceControl deviceControl1;
    }
}