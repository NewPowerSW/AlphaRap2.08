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
            this.label7 = new System.Windows.Forms.Label();
            this.RobotSpeed = new System.Windows.Forms.TrackBar();
            this.dataTable1 = new System.Data.DataTable();
            this.dataColumn1 = new System.Data.DataColumn();
            this.dataColumn2 = new System.Data.DataColumn();
            this.dataColumn3 = new System.Data.DataColumn();
            this.dataColumn4 = new System.Data.DataColumn();
            this.dataColumn5 = new System.Data.DataColumn();
            this.dataTable2 = new System.Data.DataTable();
            this.tabPage = new System.Windows.Forms.TabControl();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.tbMassage3D = new System.Windows.Forms.TextBox();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.tbMassage = new System.Windows.Forms.TextBox();
            this.Client = new System.Windows.Forms.Button();
            this.btServer = new System.Windows.Forms.Button();
            this.textBox16 = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox8 = new System.Windows.Forms.TextBox();
            this.button9 = new System.Windows.Forms.Button();
            this.button10 = new System.Windows.Forms.Button();
            this.button110 = new System.Windows.Forms.Button();
            this.textBox6 = new System.Windows.Forms.TextBox();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.Ping = new System.Windows.Forms.GroupBox();
            this.textBox12 = new System.Windows.Forms.TextBox();
            this.textBox11 = new System.Windows.Forms.TextBox();
            this.button13 = new System.Windows.Forms.Button();
            this.tabPage6 = new System.Windows.Forms.TabPage();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox7 = new System.Windows.Forms.TextBox();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.button11 = new System.Windows.Forms.Button();
            this.button12 = new System.Windows.Forms.Button();
            this.txtIP = new System.Windows.Forms.TextBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtRobotSpeed = new System.Windows.Forms.TextBox();
            this.tabPage7 = new System.Windows.Forms.TabPage();
            this.deviceControl1 = new AlphaRap.DeviceControl();
            ((System.ComponentModel.ISupportInitialize)(this.SettingData)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.RecipeData)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.RobotSpeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataTable1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataTable2)).BeginInit();
            this.tabPage.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.Ping.SuspendLayout();
            this.tabPage6.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage7.SuspendLayout();
            this.SuspendLayout();
            // 
            // SettingData
            // 
            this.SettingData.Tables.AddRange(new System.Data.DataTable[] {
            this.dataTable1,
            this.dataTable2});
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label7.Location = new System.Drawing.Point(62, 39);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(273, 50);
            this.label7.TabIndex = 14;
            this.label7.Text = "Speed Ratio :";
            // 
            // RobotSpeed
            // 
            this.RobotSpeed.BackColor = System.Drawing.SystemColors.HighlightText;
            this.RobotSpeed.Location = new System.Drawing.Point(276, 29);
            this.RobotSpeed.Margin = new System.Windows.Forms.Padding(4);
            this.RobotSpeed.Maximum = 100;
            this.RobotSpeed.Minimum = 1;
            this.RobotSpeed.Name = "RobotSpeed";
            this.RobotSpeed.Size = new System.Drawing.Size(631, 90);
            this.RobotSpeed.TabIndex = 13;
            this.RobotSpeed.TickStyle = System.Windows.Forms.TickStyle.Both;
            this.RobotSpeed.Value = 33;
            this.RobotSpeed.ValueChanged += new System.EventHandler(this.trackBar1_ValueChanged);
            this.RobotSpeed.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DataChangeBar_Click);
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
            // tabPage
            // 
            this.tabPage.Controls.Add(this.tabPage4);
            this.tabPage.Controls.Add(this.tabPage1);
            this.tabPage.Controls.Add(this.tabPage5);
            this.tabPage.Controls.Add(this.tabPage6);
            this.tabPage.Controls.Add(this.tabPage3);
            this.tabPage.Controls.Add(this.tabPage7);
            this.tabPage.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tabPage.Location = new System.Drawing.Point(0, 13);
            this.tabPage.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage.Name = "tabPage";
            this.tabPage.SelectedIndex = 0;
            this.tabPage.Size = new System.Drawing.Size(1200, 1372);
            this.tabPage.TabIndex = 3;
            // 
            // tabPage4
            // 
            this.tabPage4.BackColor = System.Drawing.Color.White;
            this.tabPage4.Controls.Add(this.groupBox1);
            this.tabPage4.Controls.Add(this.groupBox2);
            this.tabPage4.Location = new System.Drawing.Point(8, 56);
            this.tabPage4.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage4.Size = new System.Drawing.Size(1184, 1308);
            this.tabPage4.TabIndex = 1;
            this.tabPage4.Text = "Convery";
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.Transparent;
            this.groupBox1.Controls.Add(this.textBox2);
            this.groupBox1.Controls.Add(this.tbMassage3D);
            this.groupBox1.Controls.Add(this.button5);
            this.groupBox1.Controls.Add(this.button6);
            this.groupBox1.Controls.Add(this.textBox4);
            this.groupBox1.Controls.Add(this.label12);
            this.groupBox1.Controls.Add(this.label13);
            this.groupBox1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox1.Location = new System.Drawing.Point(26, 286);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(1113, 236);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "3D Scanner Setting";
            // 
            // textBox2
            // 
            this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox2.Location = new System.Drawing.Point(308, 44);
            this.textBox2.Margin = new System.Windows.Forms.Padding(4);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(333, 50);
            this.textBox2.TabIndex = 32;
            this.textBox2.Text = "192.168.10.10";
            this.textBox2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tbMassage3D
            // 
            this.tbMassage3D.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbMassage3D.Location = new System.Drawing.Point(308, 90);
            this.tbMassage3D.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.tbMassage3D.Multiline = true;
            this.tbMassage3D.Name = "tbMassage3D";
            this.tbMassage3D.Size = new System.Drawing.Size(672, 126);
            this.tbMassage3D.TabIndex = 31;
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(121, 90);
            this.button5.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(159, 61);
            this.button5.TabIndex = 30;
            this.button5.Text = "Connect";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(121, 156);
            this.button6.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(159, 61);
            this.button6.TabIndex = 29;
            this.button6.Text = "Run";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // textBox4
            // 
            this.textBox4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox4.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox4.Location = new System.Drawing.Point(832, 40);
            this.textBox4.Margin = new System.Windows.Forms.Padding(4);
            this.textBox4.Multiline = true;
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(141, 43);
            this.textBox4.TabIndex = 1;
            this.textBox4.Text = "8500";
            this.textBox4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBox4.Click += new System.EventHandler(this.DataChange_Click);
            // 
            // label12
            // 
            this.label12.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label12.Location = new System.Drawing.Point(667, 46);
            this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(157, 31);
            this.label12.TabIndex = 0;
            this.label12.Text = "Port :";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label13
            // 
            this.label13.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label13.Location = new System.Drawing.Point(21, 42);
            this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(223, 31);
            this.label13.TabIndex = 0;
            this.label13.Text = "IP Address :";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.Color.Transparent;
            this.groupBox2.Controls.Add(this.textBox1);
            this.groupBox2.Controls.Add(this.tbMassage);
            this.groupBox2.Controls.Add(this.Client);
            this.groupBox2.Controls.Add(this.btServer);
            this.groupBox2.Controls.Add(this.textBox16);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox2.Location = new System.Drawing.Point(26, 19);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox2.Size = new System.Drawing.Size(1113, 236);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Barcode Scanner Setting";
            // 
            // textBox1
            // 
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox1.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.SettingData, "MSet.ScannIP", true));
            this.textBox1.Location = new System.Drawing.Point(308, 44);
            this.textBox1.Margin = new System.Windows.Forms.Padding(4);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(333, 50);
            this.textBox1.TabIndex = 32;
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tbMassage
            // 
            this.tbMassage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbMassage.Location = new System.Drawing.Point(301, 91);
            this.tbMassage.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.tbMassage.Multiline = true;
            this.tbMassage.Name = "tbMassage";
            this.tbMassage.Size = new System.Drawing.Size(672, 126);
            this.tbMassage.TabIndex = 31;
            // 
            // Client
            // 
            this.Client.Location = new System.Drawing.Point(121, 91);
            this.Client.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.Client.Name = "Client";
            this.Client.Size = new System.Drawing.Size(159, 61);
            this.Client.TabIndex = 30;
            this.Client.Text = "连接扫码枪";
            this.Client.UseVisualStyleBackColor = true;
            this.Client.Click += new System.EventHandler(this.Client_Click);
            // 
            // btServer
            // 
            this.btServer.Location = new System.Drawing.Point(121, 156);
            this.btServer.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.btServer.Name = "btServer";
            this.btServer.Size = new System.Drawing.Size(159, 61);
            this.btServer.TabIndex = 29;
            this.btServer.Text = "开始扫码";
            this.btServer.UseVisualStyleBackColor = true;
            this.btServer.Click += new System.EventHandler(this.btServer_Click);
            // 
            // textBox16
            // 
            this.textBox16.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox16.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.SettingData, "MSet.ScannPort", true));
            this.textBox16.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox16.Location = new System.Drawing.Point(832, 40);
            this.textBox16.Margin = new System.Windows.Forms.Padding(4);
            this.textBox16.Multiline = true;
            this.textBox16.Name = "textBox16";
            this.textBox16.Size = new System.Drawing.Size(141, 43);
            this.textBox16.TabIndex = 1;
            this.textBox16.Text = "1024";
            this.textBox16.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBox16.Click += new System.EventHandler(this.DataChange_Click);
            // 
            // label8
            // 
            this.label8.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label8.Location = new System.Drawing.Point(667, 46);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(157, 31);
            this.label8.TabIndex = 0;
            this.label8.Text = "Port :";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label6
            // 
            this.label6.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label6.Location = new System.Drawing.Point(21, 42);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(223, 31);
            this.label6.TabIndex = 0;
            this.label6.Text = "IP Address :";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.White;
            this.tabPage1.Controls.Add(this.groupBox3);
            this.tabPage1.Location = new System.Drawing.Point(8, 56);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Size = new System.Drawing.Size(1184, 1308);
            this.tabPage1.TabIndex = 2;
            this.tabPage1.Text = "Pressure";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.textBox8);
            this.groupBox3.Controls.Add(this.button9);
            this.groupBox3.Controls.Add(this.button10);
            this.groupBox3.Controls.Add(this.button110);
            this.groupBox3.Controls.Add(this.textBox6);
            this.groupBox3.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox3.Location = new System.Drawing.Point(8, 13);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox3.Size = new System.Drawing.Size(1163, 341);
            this.groupBox3.TabIndex = 52;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Pressure";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(33, 62);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(99, 42);
            this.label5.TabIndex = 32;
            this.label5.Text = "COM";
            // 
            // textBox8
            // 
            this.textBox8.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox8.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.SettingData, "MSet.Pressure_COM", true));
            this.textBox8.Location = new System.Drawing.Point(112, 60);
            this.textBox8.Name = "textBox8";
            this.textBox8.Size = new System.Drawing.Size(158, 50);
            this.textBox8.TabIndex = 31;
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(38, 252);
            this.button9.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(232, 50);
            this.button9.TabIndex = 1;
            this.button9.Text = "Read";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // button10
            // 
            this.button10.Location = new System.Drawing.Point(38, 176);
            this.button10.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(232, 61);
            this.button10.TabIndex = 30;
            this.button10.Text = "Close SP";
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Click += new System.EventHandler(this.button10_Click);
            // 
            // button110
            // 
            this.button110.Location = new System.Drawing.Point(38, 109);
            this.button110.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.button110.Name = "button110";
            this.button110.Size = new System.Drawing.Size(232, 61);
            this.button110.TabIndex = 30;
            this.button110.Text = "ConnectSP";
            this.button110.UseVisualStyleBackColor = true;
            this.button110.Click += new System.EventHandler(this.button11_Click);
            // 
            // textBox6
            // 
            this.textBox6.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox6.Location = new System.Drawing.Point(294, 109);
            this.textBox6.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBox6.Multiline = true;
            this.textBox6.Name = "textBox6";
            this.textBox6.Size = new System.Drawing.Size(831, 193);
            this.textBox6.TabIndex = 0;
            // 
            // tabPage5
            // 
            this.tabPage5.BackColor = System.Drawing.Color.White;
            this.tabPage5.Controls.Add(this.Ping);
            this.tabPage5.Location = new System.Drawing.Point(8, 56);
            this.tabPage5.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Size = new System.Drawing.Size(1184, 1308);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "IP Ping";
            // 
            // Ping
            // 
            this.Ping.Controls.Add(this.textBox12);
            this.Ping.Controls.Add(this.textBox11);
            this.Ping.Controls.Add(this.button13);
            this.Ping.Dock = System.Windows.Forms.DockStyle.Top;
            this.Ping.Location = new System.Drawing.Point(0, 0);
            this.Ping.Name = "Ping";
            this.Ping.Size = new System.Drawing.Size(1184, 271);
            this.Ping.TabIndex = 169;
            this.Ping.TabStop = false;
            this.Ping.Text = "Ping";
            // 
            // textBox12
            // 
            this.textBox12.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox12.Location = new System.Drawing.Point(109, 70);
            this.textBox12.Margin = new System.Windows.Forms.Padding(4);
            this.textBox12.Multiline = true;
            this.textBox12.Name = "textBox12";
            this.textBox12.Size = new System.Drawing.Size(385, 35);
            this.textBox12.TabIndex = 171;
            // 
            // textBox11
            // 
            this.textBox11.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox11.Location = new System.Drawing.Point(515, 22);
            this.textBox11.Margin = new System.Windows.Forms.Padding(4);
            this.textBox11.Multiline = true;
            this.textBox11.Name = "textBox11";
            this.textBox11.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox11.Size = new System.Drawing.Size(526, 221);
            this.textBox11.TabIndex = 170;
            // 
            // button13
            // 
            this.button13.Location = new System.Drawing.Point(109, 134);
            this.button13.Margin = new System.Windows.Forms.Padding(4);
            this.button13.Name = "button13";
            this.button13.Size = new System.Drawing.Size(387, 109);
            this.button13.TabIndex = 169;
            this.button13.Text = "Test";
            this.button13.UseVisualStyleBackColor = true;
            // 
            // tabPage6
            // 
            this.tabPage6.Controls.Add(this.label4);
            this.tabPage6.Controls.Add(this.label3);
            this.tabPage6.Controls.Add(this.textBox7);
            this.tabPage6.Controls.Add(this.textBox5);
            this.tabPage6.Controls.Add(this.textBox3);
            this.tabPage6.Controls.Add(this.button11);
            this.tabPage6.Controls.Add(this.button12);
            this.tabPage6.Controls.Add(this.txtIP);
            this.tabPage6.Controls.Add(this.btnConnect);
            this.tabPage6.Location = new System.Drawing.Point(8, 56);
            this.tabPage6.Name = "tabPage6";
            this.tabPage6.Size = new System.Drawing.Size(1184, 1308);
            this.tabPage6.TabIndex = 6;
            this.tabPage6.Text = "PLC";
            this.tabPage6.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(68, 98);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(87, 42);
            this.label4.TabIndex = 48;
            this.label4.Text = "Port";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(68, 29);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 42);
            this.label3.TabIndex = 47;
            this.label3.Text = "IP";
            // 
            // textBox7
            // 
            this.textBox7.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox7.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.SettingData, "MSet.PLCPort", true));
            this.textBox7.Location = new System.Drawing.Point(73, 129);
            this.textBox7.Margin = new System.Windows.Forms.Padding(4);
            this.textBox7.Name = "textBox7";
            this.textBox7.Size = new System.Drawing.Size(169, 50);
            this.textBox7.TabIndex = 46;
            // 
            // textBox5
            // 
            this.textBox5.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox5.Location = new System.Drawing.Point(263, 129);
            this.textBox5.Margin = new System.Windows.Forms.Padding(4);
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new System.Drawing.Size(169, 50);
            this.textBox5.TabIndex = 45;
            this.textBox5.Text = "1";
            this.textBox5.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBox3
            // 
            this.textBox3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox3.Location = new System.Drawing.Point(263, 60);
            this.textBox3.Margin = new System.Windows.Forms.Padding(4);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(169, 50);
            this.textBox3.TabIndex = 44;
            this.textBox3.Text = "M100";
            this.textBox3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // button11
            // 
            this.button11.Location = new System.Drawing.Point(430, 184);
            this.button11.Margin = new System.Windows.Forms.Padding(4);
            this.button11.Name = "button11";
            this.button11.Size = new System.Drawing.Size(132, 35);
            this.button11.TabIndex = 39;
            this.button11.Text = "写入";
            this.button11.UseVisualStyleBackColor = true;
            this.button11.Click += new System.EventHandler(this.button11_Click_1);
            // 
            // button12
            // 
            this.button12.Location = new System.Drawing.Point(263, 182);
            this.button12.Margin = new System.Windows.Forms.Padding(4);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(159, 35);
            this.button12.TabIndex = 38;
            this.button12.Text = "读取";
            this.button12.UseVisualStyleBackColor = true;
            this.button12.Click += new System.EventHandler(this.button12_Click);
            // 
            // txtIP
            // 
            this.txtIP.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtIP.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.SettingData, "MSet.PLCIP", true));
            this.txtIP.Location = new System.Drawing.Point(73, 60);
            this.txtIP.Margin = new System.Windows.Forms.Padding(4);
            this.txtIP.Name = "txtIP";
            this.txtIP.Size = new System.Drawing.Size(169, 50);
            this.txtIP.TabIndex = 36;
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(73, 182);
            this.btnConnect.Margin = new System.Windows.Forms.Padding(4);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(169, 37);
            this.btnConnect.TabIndex = 35;
            this.btnConnect.Text = "连接";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // tabPage3
            // 
            this.tabPage3.BackColor = System.Drawing.Color.White;
            this.tabPage3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.tabPage3.Controls.Add(this.label2);
            this.tabPage3.Controls.Add(this.label1);
            this.tabPage3.Controls.Add(this.txtRobotSpeed);
            this.tabPage3.Controls.Add(this.RobotSpeed);
            this.tabPage3.Controls.Add(this.label7);
            this.tabPage3.Location = new System.Drawing.Point(8, 56);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage3.Size = new System.Drawing.Size(1184, 1308);
            this.tabPage3.TabIndex = 0;
            this.tabPage3.Text = "Speed";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.label2.Font = new System.Drawing.Font("微软雅黑 Light", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(860, 57);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 41);
            this.label2.TabIndex = 14;
            this.label2.Text = "100";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(277, 58);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(30, 35);
            this.label1.TabIndex = 14;
            this.label1.Text = "1";
            // 
            // txtRobotSpeed
            // 
            this.txtRobotSpeed.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtRobotSpeed.Location = new System.Drawing.Point(926, 29);
            this.txtRobotSpeed.Margin = new System.Windows.Forms.Padding(4);
            this.txtRobotSpeed.Multiline = true;
            this.txtRobotSpeed.Name = "txtRobotSpeed";
            this.txtRobotSpeed.Size = new System.Drawing.Size(131, 55);
            this.txtRobotSpeed.TabIndex = 1;
            this.txtRobotSpeed.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtRobotSpeed.TextChanged += new System.EventHandler(this.txtRobotSpeed_TextChanged);
            this.txtRobotSpeed.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtRobotSpeed_KeyPress);
            this.txtRobotSpeed.Leave += new System.EventHandler(this.txtRobotSpeed_Leave);
            this.txtRobotSpeed.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DataChangeBar_Click);
            // 
            // tabPage7
            // 
            this.tabPage7.Controls.Add(this.deviceControl1);
            this.tabPage7.Location = new System.Drawing.Point(8, 56);
            this.tabPage7.Name = "tabPage7";
            this.tabPage7.Size = new System.Drawing.Size(1184, 1308);
            this.tabPage7.TabIndex = 8;
            this.tabPage7.Text = "tabPage7";
            this.tabPage7.UseVisualStyleBackColor = true;
            // 
            // deviceControl1
            // 
            this.deviceControl1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(248)))), ((int)(((byte)(249)))));
            this.deviceControl1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.deviceControl1.DeviceName = "Device1";
            this.deviceControl1.DeviceTypeName = null;
            this.deviceControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.deviceControl1.Font = new System.Drawing.Font("宋体", 10F);
            this.deviceControl1.Location = new System.Drawing.Point(0, 0);
            this.deviceControl1.Name = "deviceControl1";
            this.deviceControl1.Size = new System.Drawing.Size(1184, 1308);
            this.deviceControl1.TabIndex = 15;
            // 
            // Parameter
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1187, 1061);
            this.Controls.Add(this.tabPage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "Parameter";
            this.Text = " ";
            this.Leave += new System.EventHandler(this.Parameter_Leave);
            ((System.ComponentModel.ISupportInitialize)(this.SettingData)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.RecipeData)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.RobotSpeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataTable1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataTable2)).EndInit();
            this.tabPage.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tabPage1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.tabPage5.ResumeLayout(false);
            this.Ping.ResumeLayout(false);
            this.Ping.PerformLayout();
            this.tabPage6.ResumeLayout(false);
            this.tabPage6.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.tabPage7.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Data.DataTable dataTable1;
        private System.Data.DataTable dataTable2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TabControl tabPage;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TextBox txtRobotSpeed;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox textBox16;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbMassage;
        private System.Windows.Forms.Button Client;
        private System.Windows.Forms.Button btServer;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        public System.Windows.Forms.TextBox tbMassage3D;
        public System.Windows.Forms.TrackBar RobotSpeed;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.Button button110;
        private System.Windows.Forms.TextBox textBox6;
        private System.Windows.Forms.TabPage tabPage5;
		private System.Windows.Forms.TabPage tabPage6;
		private System.Windows.Forms.Button button11;
		private System.Windows.Forms.Button button12;
		private System.Windows.Forms.TextBox txtIP;
		private System.Windows.Forms.Button btnConnect;
		private System.Windows.Forms.TextBox textBox3;
		private System.Windows.Forms.TextBox textBox5;
		private System.Data.DataColumn dataColumn1;
		private System.Data.DataColumn dataColumn2;
		private System.Data.DataColumn dataColumn3;
		private System.Data.DataColumn dataColumn4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox textBox7;
		private System.Windows.Forms.TextBox textBox8;
		private System.Data.DataColumn dataColumn5;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.GroupBox Ping;
		private System.Windows.Forms.TextBox textBox12;
		private System.Windows.Forms.TextBox textBox11;
		private System.Windows.Forms.Button button13;
		private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TabPage tabPage7;
        private DeviceControl deviceControl1;
    }
}