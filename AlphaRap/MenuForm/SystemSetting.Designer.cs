namespace AlphaRap
{
    partial class SystemSetting
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
            this.dataTable2 = new System.Data.DataTable();
            this.dataColumn1 = new System.Data.DataColumn();
            this.dataColumn2 = new System.Data.DataColumn();
            this.label3 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.Radio_DisScann = new System.Windows.Forms.RadioButton();
            this.Radio_TryRun = new System.Windows.Forms.RadioButton();
            this.Radio_DisVision = new System.Windows.Forms.RadioButton();
            this.Radio_DisMes = new System.Windows.Forms.RadioButton();
            this.rbGreenOff_R = new System.Windows.Forms.RadioButton();
            this.rbGreenBlink_R = new System.Windows.Forms.RadioButton();
            this.rbBuzzOff_R = new System.Windows.Forms.RadioButton();
            this.rbBuzzBlink_R = new System.Windows.Forms.RadioButton();
            this.label5 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.SettingData)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.RecipeData)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataTable1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataTable2)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
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
            this.dataTable1.TableName = "MSet";
            // 
            // dataTable2
            // 
            this.dataTable2.Columns.AddRange(new System.Data.DataColumn[] {
            this.dataColumn1,
            this.dataColumn2});
            this.dataTable2.TableName = "PSet";
            // 
            // dataColumn1
            // 
            this.dataColumn1.ColumnName = "DisScann";
            this.dataColumn1.DataType = typeof(bool);
            // 
            // dataColumn2
            // 
            this.dataColumn2.ColumnName = "UseTryRun";
            this.dataColumn2.DataType = typeof(bool);
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(108)))), ((int)(((byte)(182)))));
            this.label3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label3.Dock = System.Windows.Forms.DockStyle.Top;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(0, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(1200, 47);
            this.label3.TabIndex = 38;
            this.label3.Text = "SystemSetting";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.White;
            this.tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 28.42287F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 21.14385F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.47661F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 24.95667F));
            this.tableLayoutPanel1.Controls.Add(this.label6, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label7, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.label8, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.Radio_DisScann, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.Radio_TryRun, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.Radio_DisVision, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.Radio_DisMes, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.rbGreenOff_R, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.rbGreenBlink_R, 3, 2);
            this.tableLayoutPanel1.Controls.Add(this.rbBuzzOff_R, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.rbBuzzBlink_R, 3, 3);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label1, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.label4, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.label9, 2, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 47);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1200, 307);
            this.tableLayoutPanel1.TabIndex = 39;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label6.Font = new System.Drawing.Font("微软雅黑", 14.25F);
            this.label6.Location = new System.Drawing.Point(5, 153);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(331, 75);
            this.label6.TabIndex = 0;
            this.label6.Text = "禁用门禁";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label7.Font = new System.Drawing.Font("微软雅黑", 14.25F);
            this.label7.Location = new System.Drawing.Point(5, 229);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(331, 77);
            this.label7.TabIndex = 0;
            this.label7.Text = "禁用蜂鸣器";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label8.Font = new System.Drawing.Font("微软雅黑", 14.25F);
            this.label8.Location = new System.Drawing.Point(5, 77);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(331, 75);
            this.label8.TabIndex = 0;
            this.label8.Text = "禁用视觉";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Radio_DisScann
            // 
            this.Radio_DisScann.AutoCheck = false;
            this.Radio_DisScann.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Radio_DisScann.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.SettingData, "PSet.DisScann", true));
            this.Radio_DisScann.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Radio_DisScann.Location = new System.Drawing.Point(345, 4);
            this.Radio_DisScann.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Radio_DisScann.Name = "Radio_DisScann";
            this.Radio_DisScann.Size = new System.Drawing.Size(244, 69);
            this.Radio_DisScann.TabIndex = 2;
            this.Radio_DisScann.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Radio_DisScann.UseVisualStyleBackColor = false;
            this.Radio_DisScann.Click += new System.EventHandler(this.Raid_Click);
            this.Radio_DisScann.Leave += new System.EventHandler(this.DataChange_Click);
            // 
            // Radio_TryRun
            // 
            this.Radio_TryRun.AutoCheck = false;
            this.Radio_TryRun.AutoSize = true;
            this.Radio_TryRun.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Radio_TryRun.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.SettingData, "PSet.UseTryRun", true));
            this.Radio_TryRun.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Radio_TryRun.Location = new System.Drawing.Point(903, 4);
            this.Radio_TryRun.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Radio_TryRun.Name = "Radio_TryRun";
            this.Radio_TryRun.Size = new System.Drawing.Size(292, 69);
            this.Radio_TryRun.TabIndex = 4;
            this.Radio_TryRun.UseVisualStyleBackColor = true;
            this.Radio_TryRun.Click += new System.EventHandler(this.Raid_Click);
            // 
            // Radio_DisVision
            // 
            this.Radio_DisVision.AutoCheck = false;
            this.Radio_DisVision.AutoSize = true;
            this.Radio_DisVision.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Radio_DisVision.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Radio_DisVision.Location = new System.Drawing.Point(345, 80);
            this.Radio_DisVision.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Radio_DisVision.Name = "Radio_DisVision";
            this.Radio_DisVision.Size = new System.Drawing.Size(244, 69);
            this.Radio_DisVision.TabIndex = 5;
            this.Radio_DisVision.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Radio_DisVision.UseVisualStyleBackColor = true;
            this.Radio_DisVision.Click += new System.EventHandler(this.Raid_Click);
            // 
            // Radio_DisMes
            // 
            this.Radio_DisMes.AutoCheck = false;
            this.Radio_DisMes.AutoSize = true;
            this.Radio_DisMes.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Radio_DisMes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Radio_DisMes.Location = new System.Drawing.Point(903, 80);
            this.Radio_DisMes.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Radio_DisMes.Name = "Radio_DisMes";
            this.Radio_DisMes.Size = new System.Drawing.Size(292, 69);
            this.Radio_DisMes.TabIndex = 7;
            this.Radio_DisMes.UseVisualStyleBackColor = true;
            this.Radio_DisMes.Click += new System.EventHandler(this.Raid_Click);
            // 
            // rbGreenOff_R
            // 
            this.rbGreenOff_R.AutoCheck = false;
            this.rbGreenOff_R.AutoSize = true;
            this.rbGreenOff_R.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbGreenOff_R.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rbGreenOff_R.Location = new System.Drawing.Point(345, 156);
            this.rbGreenOff_R.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.rbGreenOff_R.Name = "rbGreenOff_R";
            this.rbGreenOff_R.Size = new System.Drawing.Size(244, 69);
            this.rbGreenOff_R.TabIndex = 8;
            this.rbGreenOff_R.UseVisualStyleBackColor = true;
            this.rbGreenOff_R.Click += new System.EventHandler(this.Raid_Click);
            // 
            // rbGreenBlink_R
            // 
            this.rbGreenBlink_R.AutoCheck = false;
            this.rbGreenBlink_R.AutoSize = true;
            this.rbGreenBlink_R.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbGreenBlink_R.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rbGreenBlink_R.Location = new System.Drawing.Point(903, 156);
            this.rbGreenBlink_R.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.rbGreenBlink_R.Name = "rbGreenBlink_R";
            this.rbGreenBlink_R.Size = new System.Drawing.Size(292, 69);
            this.rbGreenBlink_R.TabIndex = 10;
            this.rbGreenBlink_R.UseVisualStyleBackColor = true;
            this.rbGreenBlink_R.Click += new System.EventHandler(this.Raid_Click);
            // 
            // rbBuzzOff_R
            // 
            this.rbBuzzOff_R.AutoCheck = false;
            this.rbBuzzOff_R.AutoSize = true;
            this.rbBuzzOff_R.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbBuzzOff_R.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rbBuzzOff_R.Location = new System.Drawing.Point(345, 232);
            this.rbBuzzOff_R.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.rbBuzzOff_R.Name = "rbBuzzOff_R";
            this.rbBuzzOff_R.Size = new System.Drawing.Size(244, 71);
            this.rbBuzzOff_R.TabIndex = 11;
            this.rbBuzzOff_R.UseVisualStyleBackColor = true;
            this.rbBuzzOff_R.Click += new System.EventHandler(this.Raid_Click);
            // 
            // rbBuzzBlink_R
            // 
            this.rbBuzzBlink_R.AutoCheck = false;
            this.rbBuzzBlink_R.AutoSize = true;
            this.rbBuzzBlink_R.CheckAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbBuzzBlink_R.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rbBuzzBlink_R.Location = new System.Drawing.Point(903, 232);
            this.rbBuzzBlink_R.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.rbBuzzBlink_R.Name = "rbBuzzBlink_R";
            this.rbBuzzBlink_R.Size = new System.Drawing.Size(292, 71);
            this.rbBuzzBlink_R.TabIndex = 13;
            this.rbBuzzBlink_R.UseVisualStyleBackColor = true;
            this.rbBuzzBlink_R.Click += new System.EventHandler(this.Raid_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label5.Font = new System.Drawing.Font("微软雅黑", 14.25F);
            this.label5.Location = new System.Drawing.Point(5, 1);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(331, 75);
            this.label5.TabIndex = 0;
            this.label5.Text = "禁用扫码枪";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 14.25F);
            this.label1.Location = new System.Drawing.Point(598, 1);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(296, 75);
            this.label1.TabIndex = 14;
            this.label1.Text = "使用空跑";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Font = new System.Drawing.Font("微软雅黑", 14.25F);
            this.label2.Location = new System.Drawing.Point(598, 77);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(296, 75);
            this.label2.TabIndex = 15;
            this.label2.Text = "禁用MES";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Font = new System.Drawing.Font("微软雅黑", 14.25F);
            this.label4.Location = new System.Drawing.Point(598, 153);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(296, 75);
            this.label4.TabIndex = 16;
            this.label4.Text = "禁用复检";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label9.Font = new System.Drawing.Font("微软雅黑", 14.25F);
            this.label9.Location = new System.Drawing.Point(598, 229);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(296, 77);
            this.label9.TabIndex = 17;
            this.label9.Text = "禁用3D";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // SystemSetting
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1200, 1061);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.label3);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "SystemSetting";
            this.Text = "SysSetting";
            this.Leave += new System.EventHandler(this.SystemSetting_Leave);
            ((System.ComponentModel.ISupportInitialize)(this.SettingData)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.RecipeData)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataTable1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataTable2)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Data.DataTable dataTable1;
        private System.Data.DataTable dataTable2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.RadioButton Radio_DisScann;
		private System.Windows.Forms.RadioButton Radio_TryRun;
		private System.Windows.Forms.RadioButton Radio_DisVision;
		private System.Windows.Forms.RadioButton Radio_DisMes;
		private System.Windows.Forms.RadioButton rbGreenOff_R;
		private System.Windows.Forms.RadioButton rbGreenBlink_R;
		private System.Windows.Forms.RadioButton rbBuzzOff_R;
		private System.Windows.Forms.RadioButton rbBuzzBlink_R;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label9;
		private System.Data.DataColumn dataColumn1;
		private System.Data.DataColumn dataColumn2;
	}
}