namespace AlphaRap
{
	partial class LogForm
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
			this.TableData1 = new System.Data.DataTable();
			this.dataColumn7 = new System.Data.DataColumn();
			this.dataColumn8 = new System.Data.DataColumn();
			this.dataColumn9 = new System.Data.DataColumn();
			this.dataColumn10 = new System.Data.DataColumn();
			this.dataColumn11 = new System.Data.DataColumn();
			this.dataColumn12 = new System.Data.DataColumn();
			this.dataColumn13 = new System.Data.DataColumn();
			this.dataColumn14 = new System.Data.DataColumn();
			this.dataColumn15 = new System.Data.DataColumn();
			this.dataColumn16 = new System.Data.DataColumn();
			this.dataColumn17 = new System.Data.DataColumn();
			this.dataColumn28 = new System.Data.DataColumn();
			this.dataColumn32 = new System.Data.DataColumn();
			this.dataColumn33 = new System.Data.DataColumn();
			this.TableData2 = new System.Data.DataTable();
			this.dataColumn18 = new System.Data.DataColumn();
			this.dataColumn19 = new System.Data.DataColumn();
			this.dataColumn20 = new System.Data.DataColumn();
			this.dataColumn21 = new System.Data.DataColumn();
			this.dataColumn22 = new System.Data.DataColumn();
			this.dataColumn23 = new System.Data.DataColumn();
			this.dataColumn24 = new System.Data.DataColumn();
			this.dataColumn25 = new System.Data.DataColumn();
			this.dataColumn26 = new System.Data.DataColumn();
			this.dataColumn27 = new System.Data.DataColumn();
			this.dataColumn29 = new System.Data.DataColumn();
			this.dataColumn31 = new System.Data.DataColumn();
			this.dataColumn34 = new System.Data.DataColumn();
			this.dataColumn35 = new System.Data.DataColumn();
			this.dataTable2 = new System.Data.DataTable();
			this.dataColumn36 = new System.Data.DataColumn();
			this.dataColumn37 = new System.Data.DataColumn();
			this.label27 = new System.Windows.Forms.Label();
			this.txtUser = new System.Windows.Forms.TextBox();
			this.txtAlarm = new System.Windows.Forms.TextBox();
			this.label29 = new System.Windows.Forms.Label();
			this.btUserPath = new System.Windows.Forms.Button();
			this.btAlarmPath = new System.Windows.Forms.Button();
			this.label28 = new System.Windows.Forms.Label();
			this.txtRun = new System.Windows.Forms.TextBox();
			this.btRunPath = new System.Windows.Forms.Button();
			this.btMesPath = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.txtMes = new System.Windows.Forms.TextBox();
			this.btRunErrorPath = new System.Windows.Forms.Button();
			this.txtRunError = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.SettingData)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RecipeData)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dataTable1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TableData1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TableData2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dataTable2)).BeginInit();
			this.SuspendLayout();
			// 
			// SettingData
			// 
			this.SettingData.Tables.AddRange(new System.Data.DataTable[] {
            this.dataTable1,
            this.TableData1,
            this.TableData2,
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
			this.dataTable1.TableName = "Path";
			// 
			// dataColumn1
			// 
			this.dataColumn1.ColumnName = "UserPath";
			// 
			// dataColumn2
			// 
			this.dataColumn2.ColumnName = "RunError";
			// 
			// dataColumn3
			// 
			this.dataColumn3.ColumnName = "RunPath";
			// 
			// dataColumn4
			// 
			this.dataColumn4.ColumnName = "AlarmPath";
			// 
			// dataColumn5
			// 
			this.dataColumn5.ColumnName = "MesPath";
			// 
			// TableData1
			// 
			this.TableData1.Columns.AddRange(new System.Data.DataColumn[] {
            this.dataColumn7,
            this.dataColumn8,
            this.dataColumn9,
            this.dataColumn10,
            this.dataColumn11,
            this.dataColumn12,
            this.dataColumn13,
            this.dataColumn14,
            this.dataColumn15,
            this.dataColumn16,
            this.dataColumn17,
            this.dataColumn28,
            this.dataColumn32,
            this.dataColumn33});
			this.TableData1.TableName = "TableData1";
			// 
			// dataColumn7
			// 
			this.dataColumn7.AllowDBNull = false;
			this.dataColumn7.ColumnName = "Product1exist";
			this.dataColumn7.DataType = typeof(bool);
			this.dataColumn7.DefaultValue = false;
			// 
			// dataColumn8
			// 
			this.dataColumn8.AllowDBNull = false;
			this.dataColumn8.ColumnName = "Product1NeedWork";
			this.dataColumn8.DataType = typeof(bool);
			this.dataColumn8.DefaultValue = false;
			// 
			// dataColumn9
			// 
			this.dataColumn9.ColumnName = "PCB1SN";
			this.dataColumn9.DefaultValue = "0";
			// 
			// dataColumn10
			// 
			this.dataColumn10.ColumnName = "Housing1SN";
			this.dataColumn10.DefaultValue = "0";
			// 
			// dataColumn11
			// 
			this.dataColumn11.AllowDBNull = false;
			this.dataColumn11.ColumnName = "Screw1Status";
			this.dataColumn11.DataType = typeof(bool);
			this.dataColumn11.DefaultValue = false;
			// 
			// dataColumn12
			// 
			this.dataColumn12.ColumnName = "Screw1Torque1";
			this.dataColumn12.DefaultValue = "0";
			// 
			// dataColumn13
			// 
			this.dataColumn13.ColumnName = "Screw1Angle1";
			this.dataColumn13.DefaultValue = "0";
			// 
			// dataColumn14
			// 
			this.dataColumn14.ColumnName = "Screw1Torque2";
			this.dataColumn14.DefaultValue = "0";
			// 
			// dataColumn15
			// 
			this.dataColumn15.ColumnName = "Screw1Angle2";
			this.dataColumn15.DefaultValue = "0";
			// 
			// dataColumn16
			// 
			this.dataColumn16.ColumnName = "Screw1Torque3";
			this.dataColumn16.DefaultValue = "0";
			// 
			// dataColumn17
			// 
			this.dataColumn17.ColumnName = "Screw1Angle3";
			this.dataColumn17.DefaultValue = "0";
			// 
			// dataColumn28
			// 
			this.dataColumn28.ColumnName = "LOTID1";
			this.dataColumn28.DefaultValue = "0";
			// 
			// dataColumn32
			// 
			this.dataColumn32.Caption = "AXXONDATAName1";
			this.dataColumn32.ColumnName = "AXXONDATAName1";
			this.dataColumn32.DefaultValue = "0";
			// 
			// dataColumn33
			// 
			this.dataColumn33.ColumnName = "AXXONDATAValue1";
			this.dataColumn33.DefaultValue = "0";
			// 
			// TableData2
			// 
			this.TableData2.Columns.AddRange(new System.Data.DataColumn[] {
            this.dataColumn18,
            this.dataColumn19,
            this.dataColumn20,
            this.dataColumn21,
            this.dataColumn22,
            this.dataColumn23,
            this.dataColumn24,
            this.dataColumn25,
            this.dataColumn26,
            this.dataColumn27,
            this.dataColumn29,
            this.dataColumn31,
            this.dataColumn34,
            this.dataColumn35});
			this.TableData2.TableName = "TableData2";
			// 
			// dataColumn18
			// 
			this.dataColumn18.AllowDBNull = false;
			this.dataColumn18.Caption = "Product2exist";
			this.dataColumn18.ColumnName = "Product2exist";
			this.dataColumn18.DataType = typeof(bool);
			this.dataColumn18.DefaultValue = false;
			// 
			// dataColumn19
			// 
			this.dataColumn19.AllowDBNull = false;
			this.dataColumn19.ColumnName = "Product2NeedWork";
			this.dataColumn19.DataType = typeof(bool);
			this.dataColumn19.DefaultValue = false;
			// 
			// dataColumn20
			// 
			this.dataColumn20.Caption = "PCB1SN";
			this.dataColumn20.ColumnName = "PCB2SN";
			this.dataColumn20.DefaultValue = "0";
			// 
			// dataColumn21
			// 
			this.dataColumn21.ColumnName = "Housing2SN";
			this.dataColumn21.DefaultValue = "0";
			// 
			// dataColumn22
			// 
			this.dataColumn22.AllowDBNull = false;
			this.dataColumn22.ColumnName = "Screw2Status";
			this.dataColumn22.DataType = typeof(bool);
			this.dataColumn22.DefaultValue = false;
			// 
			// dataColumn23
			// 
			this.dataColumn23.ColumnName = "Screw2Torque1";
			this.dataColumn23.DefaultValue = "0";
			// 
			// dataColumn24
			// 
			this.dataColumn24.ColumnName = "Screw2Angle1";
			this.dataColumn24.DefaultValue = "0";
			// 
			// dataColumn25
			// 
			this.dataColumn25.ColumnName = "Screw2Torque2";
			this.dataColumn25.DefaultValue = "0";
			// 
			// dataColumn26
			// 
			this.dataColumn26.ColumnName = "Screw2Angle2";
			this.dataColumn26.DefaultValue = "0";
			// 
			// dataColumn27
			// 
			this.dataColumn27.ColumnName = "Screw2Torque3";
			this.dataColumn27.DefaultValue = "0";
			// 
			// dataColumn29
			// 
			this.dataColumn29.ColumnName = "Screw2Angle3";
			this.dataColumn29.DefaultValue = "0";
			// 
			// dataColumn31
			// 
			this.dataColumn31.ColumnName = "LOTID2";
			this.dataColumn31.DefaultValue = "0";
			// 
			// dataColumn34
			// 
			this.dataColumn34.Caption = "AXXONDATAName2";
			this.dataColumn34.ColumnName = "AXXONDATAName2";
			this.dataColumn34.DefaultValue = "0";
			// 
			// dataColumn35
			// 
			this.dataColumn35.Caption = "AXXONDATAValue2";
			this.dataColumn35.ColumnName = "AXXONDATAValue2";
			this.dataColumn35.DefaultValue = "0";
			// 
			// dataTable2
			// 
			this.dataTable2.Columns.AddRange(new System.Data.DataColumn[] {
            this.dataColumn36,
            this.dataColumn37});
			this.dataTable2.TableName = "TableData3";
			// 
			// dataColumn36
			// 
			this.dataColumn36.AllowDBNull = false;
			this.dataColumn36.ColumnName = "Product1Staus";
			this.dataColumn36.DataType = typeof(bool);
			this.dataColumn36.DefaultValue = false;
			// 
			// dataColumn37
			// 
			this.dataColumn37.AllowDBNull = false;
			this.dataColumn37.ColumnName = "Product2Staus";
			this.dataColumn37.DataType = typeof(bool);
			this.dataColumn37.DefaultValue = false;
			// 
			// label27
			// 
			this.label27.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.label27.Location = new System.Drawing.Point(23, 14);
			this.label27.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label27.Name = "label27";
			this.label27.Size = new System.Drawing.Size(241, 38);
			this.label27.TabIndex = 141;
			this.label27.Text = "User Message Path:";
			this.label27.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// txtUser
			// 
			this.txtUser.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.txtUser.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.SettingData, "Path.UserPath", true));
			this.txtUser.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.txtUser.Location = new System.Drawing.Point(273, 14);
			this.txtUser.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.txtUser.MaxLength = 50;
			this.txtUser.Name = "txtUser";
			this.txtUser.ReadOnly = true;
			this.txtUser.Size = new System.Drawing.Size(339, 34);
			this.txtUser.TabIndex = 143;
			this.txtUser.Tag = "txtUser";
			this.txtUser.Text = " ";
			// 
			// txtAlarm
			// 
			this.txtAlarm.BackColor = System.Drawing.SystemColors.HighlightText;
			this.txtAlarm.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.SettingData, "Path.AlarmPath", true));
			this.txtAlarm.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.txtAlarm.Location = new System.Drawing.Point(273, 213);
			this.txtAlarm.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.txtAlarm.MaxLength = 50;
			this.txtAlarm.Name = "txtAlarm";
			this.txtAlarm.ReadOnly = true;
			this.txtAlarm.Size = new System.Drawing.Size(337, 34);
			this.txtAlarm.TabIndex = 143;
			// 
			// label29
			// 
			this.label29.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.label29.Location = new System.Drawing.Point(23, 213);
			this.label29.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label29.Name = "label29";
			this.label29.Size = new System.Drawing.Size(241, 38);
			this.label29.TabIndex = 146;
			this.label29.Text = "Alarm Message Path:";
			this.label29.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// btUserPath
			// 
			this.btUserPath.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.btUserPath.Location = new System.Drawing.Point(633, 12);
			this.btUserPath.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.btUserPath.Name = "btUserPath";
			this.btUserPath.Size = new System.Drawing.Size(147, 38);
			this.btUserPath.TabIndex = 161;
			this.btUserPath.Tag = "";
			this.btUserPath.Text = "Path";
			this.btUserPath.UseVisualStyleBackColor = true;
			this.btUserPath.Click += new System.EventHandler(this.btPath_Click);
			// 
			// btAlarmPath
			// 
			this.btAlarmPath.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.btAlarmPath.Location = new System.Drawing.Point(633, 213);
			this.btAlarmPath.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.btAlarmPath.Name = "btAlarmPath";
			this.btAlarmPath.Size = new System.Drawing.Size(147, 38);
			this.btAlarmPath.TabIndex = 161;
			this.btAlarmPath.Text = "Path";
			this.btAlarmPath.UseVisualStyleBackColor = true;
			this.btAlarmPath.Click += new System.EventHandler(this.btPath_Click);
			// 
			// label28
			// 
			this.label28.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.label28.Location = new System.Drawing.Point(23, 83);
			this.label28.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label28.Name = "label28";
			this.label28.Size = new System.Drawing.Size(241, 38);
			this.label28.TabIndex = 142;
			this.label28.Text = "Run Message Path:";
			this.label28.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// txtRun
			// 
			this.txtRun.BackColor = System.Drawing.SystemColors.HighlightText;
			this.txtRun.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.SettingData, "Path.RunPath", true));
			this.txtRun.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.txtRun.Location = new System.Drawing.Point(273, 83);
			this.txtRun.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.txtRun.MaxLength = 1000;
			this.txtRun.Name = "txtRun";
			this.txtRun.ReadOnly = true;
			this.txtRun.Size = new System.Drawing.Size(337, 34);
			this.txtRun.TabIndex = 143;
			this.txtRun.Tag = "txtRun";
			// 
			// btRunPath
			// 
			this.btRunPath.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.btRunPath.Location = new System.Drawing.Point(633, 83);
			this.btRunPath.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.btRunPath.Name = "btRunPath";
			this.btRunPath.Size = new System.Drawing.Size(147, 38);
			this.btRunPath.TabIndex = 161;
			this.btRunPath.Text = "Path";
			this.btRunPath.UseVisualStyleBackColor = true;
			this.btRunPath.Click += new System.EventHandler(this.btPath_Click);
			// 
			// btMesPath
			// 
			this.btMesPath.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.btMesPath.Location = new System.Drawing.Point(633, 286);
			this.btMesPath.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.btMesPath.Name = "btMesPath";
			this.btMesPath.Size = new System.Drawing.Size(147, 38);
			this.btMesPath.TabIndex = 164;
			this.btMesPath.Text = "Path";
			this.btMesPath.UseVisualStyleBackColor = true;
			this.btMesPath.Click += new System.EventHandler(this.btPath_Click);
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.label1.Location = new System.Drawing.Point(23, 286);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(241, 38);
			this.label1.TabIndex = 163;
			this.label1.Text = "MES Message Path:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// txtMes
			// 
			this.txtMes.BackColor = System.Drawing.SystemColors.HighlightText;
			this.txtMes.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.SettingData, "Path.MesPath", true));
			this.txtMes.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.txtMes.Location = new System.Drawing.Point(273, 286);
			this.txtMes.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.txtMes.MaxLength = 50;
			this.txtMes.Name = "txtMes";
			this.txtMes.ReadOnly = true;
			this.txtMes.Size = new System.Drawing.Size(337, 34);
			this.txtMes.TabIndex = 162;
			// 
			// btRunErrorPath
			// 
			this.btRunErrorPath.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.btRunErrorPath.Location = new System.Drawing.Point(633, 145);
			this.btRunErrorPath.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.btRunErrorPath.Name = "btRunErrorPath";
			this.btRunErrorPath.Size = new System.Drawing.Size(147, 38);
			this.btRunErrorPath.TabIndex = 167;
			this.btRunErrorPath.Text = "Path";
			this.btRunErrorPath.UseVisualStyleBackColor = true;
			this.btRunErrorPath.Click += new System.EventHandler(this.btPath_Click);
			// 
			// txtRunError
			// 
			this.txtRunError.BackColor = System.Drawing.SystemColors.HighlightText;
			this.txtRunError.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.SettingData, "Path.RunError", true));
			this.txtRunError.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.txtRunError.Location = new System.Drawing.Point(273, 145);
			this.txtRunError.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.txtRunError.MaxLength = 1000;
			this.txtRunError.Name = "txtRunError";
			this.txtRunError.ReadOnly = true;
			this.txtRunError.Size = new System.Drawing.Size(337, 34);
			this.txtRunError.TabIndex = 166;
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.label2.Location = new System.Drawing.Point(23, 145);
			this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(241, 38);
			this.label2.TabIndex = 165;
			this.label2.Text = "RunError Message Path:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// LogForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoScroll = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(1615, 1102);
			this.Controls.Add(this.btRunErrorPath);
			this.Controls.Add(this.txtRunError);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.btMesPath);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.txtMes);
			this.Controls.Add(this.btAlarmPath);
			this.Controls.Add(this.btRunPath);
			this.Controls.Add(this.btUserPath);
			this.Controls.Add(this.label29);
			this.Controls.Add(this.txtAlarm);
			this.Controls.Add(this.txtRun);
			this.Controls.Add(this.txtUser);
			this.Controls.Add(this.label28);
			this.Controls.Add(this.label27);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Margin = new System.Windows.Forms.Padding(5);
			this.Name = "LogForm";
			this.Text = "MESForm";
			this.Load += new System.EventHandler(this.LogForm_Load);
			this.Leave += new System.EventHandler(this.MESForm_Leave);
			((System.ComponentModel.ISupportInitialize)(this.SettingData)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RecipeData)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dataTable1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TableData1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TableData2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dataTable2)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Data.DataTable dataTable1;
		private System.Data.DataColumn dataColumn1;
		private System.Data.DataColumn dataColumn2;
		private System.Data.DataColumn dataColumn3;
		private System.Data.DataColumn dataColumn4;
		private System.Data.DataTable TableData1;
		private System.Data.DataTable TableData2;
		private System.Data.DataColumn dataColumn7;
		private System.Data.DataColumn dataColumn8;
		private System.Data.DataColumn dataColumn9;
		private System.Data.DataColumn dataColumn10;
		private System.Data.DataColumn dataColumn11;
		private System.Data.DataColumn dataColumn12;
		private System.Data.DataColumn dataColumn13;
		private System.Data.DataColumn dataColumn14;
		private System.Data.DataColumn dataColumn15;
		private System.Data.DataColumn dataColumn16;
		private System.Data.DataColumn dataColumn17;
		private System.Data.DataColumn dataColumn28;
		private System.Data.DataColumn dataColumn18;
		private System.Data.DataColumn dataColumn19;
		private System.Data.DataColumn dataColumn20;
		private System.Data.DataColumn dataColumn21;
		private System.Data.DataColumn dataColumn22;
		private System.Data.DataColumn dataColumn23;
		private System.Data.DataColumn dataColumn24;
		private System.Data.DataColumn dataColumn25;
		private System.Data.DataColumn dataColumn26;
		private System.Data.DataColumn dataColumn27;
		private System.Data.DataColumn dataColumn29;
		private System.Data.DataColumn dataColumn31;
		private System.Data.DataColumn dataColumn32;
		private System.Data.DataColumn dataColumn33;
		private System.Data.DataColumn dataColumn34;
		private System.Data.DataColumn dataColumn35;
		private System.Data.DataTable dataTable2;
		private System.Data.DataColumn dataColumn36;
		private System.Data.DataColumn dataColumn37;
		private System.Windows.Forms.Label label27;
		public System.Windows.Forms.TextBox txtUser;
		public System.Windows.Forms.TextBox txtAlarm;
		private System.Windows.Forms.Label label29;
		private System.Windows.Forms.Button btUserPath;
		private System.Windows.Forms.Button btAlarmPath;
		private System.Windows.Forms.Label label28;
		public System.Windows.Forms.TextBox txtRun;
		private System.Windows.Forms.Button btRunPath;
		private System.Windows.Forms.Button btMesPath;
		private System.Windows.Forms.Label label1;
		public System.Windows.Forms.TextBox txtMes;
		private System.Windows.Forms.Button btRunErrorPath;
		public System.Windows.Forms.TextBox txtRunError;
		private System.Windows.Forms.Label label2;
		private System.Data.DataColumn dataColumn5;
	}
}