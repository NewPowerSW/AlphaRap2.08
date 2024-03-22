namespace AlphaRap.MenuForm
{
    partial class SAMSUNGUI
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.STATE = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.EESMachineStatus = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.MESMachineStatus = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.CONNECTED = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.USERID = new System.Windows.Forms.TextBox();
            this.IPADDRESS = new System.Windows.Forms.TextBox();
            this.EQPID = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.WORKINFORMATION = new System.Windows.Forms.GroupBox();
            this.WORKQTY = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.TOTALQTY = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.RECIPEID = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.PRODUCTID = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.MANAZINEID = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.TRAYID = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.LOTID1 = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.menuStrip2 = new System.Windows.Forms.MenuStrip();
            this.RMS = new System.Windows.Forms.ToolStripTextBox();
            this.TKIN = new System.Windows.Forms.ToolStripTextBox();
            this.OFFLINE = new System.Windows.Forms.ToolStripTextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.textBox8 = new System.Windows.Forms.TextBox();
            this.label21 = new System.Windows.Forms.Label();
            this.CLEAR1 = new System.Windows.Forms.Button();
            this.TKOUT = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.CLEAR2 = new System.Windows.Forms.Button();
            this.DEKIT = new System.Windows.Forms.Button();
            this.KIT = new System.Windows.Forms.Button();
            this.textBox23 = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.LOSS = new System.Windows.Forms.Button();
            this.HISTORY = new System.Windows.Forms.Button();
            this.WORK = new System.Windows.Forms.Button();
            this.STATE.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.WORKINFORMATION.SuspendLayout();
            this.menuStrip2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.Control;
            this.textBox1.Location = new System.Drawing.Point(271, 22);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(666, 59);
            this.textBox1.TabIndex = 12;
            // 
            // STATE
            // 
            this.STATE.Controls.Add(this.menuStrip2);
            this.STATE.Controls.Add(this.label7);
            this.STATE.Controls.Add(this.EESMachineStatus);
            this.STATE.Controls.Add(this.label3);
            this.STATE.Controls.Add(this.MESMachineStatus);
            this.STATE.Controls.Add(this.label2);
            this.STATE.Controls.Add(this.CONNECTED);
            this.STATE.Controls.Add(this.label1);
            this.STATE.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.STATE.Location = new System.Drawing.Point(12, 98);
            this.STATE.Name = "STATE";
            this.STATE.Size = new System.Drawing.Size(200, 345);
            this.STATE.TabIndex = 11;
            this.STATE.TabStop = false;
            this.STATE.Text = "STATE";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label7.Location = new System.Drawing.Point(47, 256);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(94, 22);
            this.label7.TabIndex = 6;
            this.label7.Text = "EES MODE";
            // 
            // EESMachineStatus
            // 
            this.EESMachineStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.EESMachineStatus.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.EESMachineStatus.Location = new System.Drawing.Point(15, 204);
            this.EESMachineStatus.Name = "EESMachineStatus";
            this.EESMachineStatus.Size = new System.Drawing.Size(168, 30);
            this.EESMachineStatus.TabIndex = 5;
            this.EESMachineStatus.Text = "IDLE(EES)";
            this.EESMachineStatus.UseVisualStyleBackColor = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(47, 176);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(109, 22);
            this.label3.TabIndex = 4;
            this.label3.Text = "System(EES)";
            // 
            // MESMachineStatus
            // 
            this.MESMachineStatus.BackColor = System.Drawing.Color.SeaGreen;
            this.MESMachineStatus.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.MESMachineStatus.Location = new System.Drawing.Point(16, 130);
            this.MESMachineStatus.Name = "MESMachineStatus";
            this.MESMachineStatus.Size = new System.Drawing.Size(168, 30);
            this.MESMachineStatus.TabIndex = 3;
            this.MESMachineStatus.Text = "RUN";
            this.MESMachineStatus.UseVisualStyleBackColor = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(47, 102);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(98, 22);
            this.label2.TabIndex = 2;
            this.label2.Text = "Equipment";
            // 
            // CONNECTED
            // 
            this.CONNECTED.BackColor = System.Drawing.Color.DodgerBlue;
            this.CONNECTED.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.CONNECTED.Location = new System.Drawing.Point(15, 58);
            this.CONNECTED.Name = "CONNECTED";
            this.CONNECTED.Size = new System.Drawing.Size(168, 30);
            this.CONNECTED.TabIndex = 1;
            this.CONNECTED.Text = "CONNECTED";
            this.CONNECTED.UseVisualStyleBackColor = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(38, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(122, 22);
            this.label1.TabIndex = 0;
            this.label1.Text = "Communicate";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(227, 80);
            this.panel1.TabIndex = 10;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(227, 80);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // USERID
            // 
            this.USERID.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.USERID.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.USERID.Location = new System.Drawing.Point(1061, 90);
            this.USERID.Multiline = true;
            this.USERID.Name = "USERID";
            this.USERID.Size = new System.Drawing.Size(168, 30);
            this.USERID.TabIndex = 20;
            // 
            // IPADDRESS
            // 
            this.IPADDRESS.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.IPADDRESS.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.IPADDRESS.Location = new System.Drawing.Point(1061, 54);
            this.IPADDRESS.Multiline = true;
            this.IPADDRESS.Name = "IPADDRESS";
            this.IPADDRESS.Size = new System.Drawing.Size(168, 30);
            this.IPADDRESS.TabIndex = 21;
            // 
            // EQPID
            // 
            this.EQPID.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.EQPID.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.EQPID.Location = new System.Drawing.Point(1061, 18);
            this.EQPID.Multiline = true;
            this.EQPID.Name = "EQPID";
            this.EQPID.Size = new System.Drawing.Size(168, 30);
            this.EQPID.TabIndex = 22;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label6.Location = new System.Drawing.Point(983, 98);
            this.label6.Margin = new System.Windows.Forms.Padding(5);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(70, 22);
            this.label6.TabIndex = 19;
            this.label6.Text = "USERID";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.Location = new System.Drawing.Point(949, 59);
            this.label5.Margin = new System.Windows.Forms.Padding(5);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(104, 22);
            this.label5.TabIndex = 18;
            this.label5.Text = "IPADDRESS";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(992, 22);
            this.label4.Margin = new System.Windows.Forms.Padding(5);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(61, 22);
            this.label4.TabIndex = 17;
            this.label4.Text = "EQPID";
            // 
            // WORKINFORMATION
            // 
            this.WORKINFORMATION.Controls.Add(this.WORKQTY);
            this.WORKINFORMATION.Controls.Add(this.label13);
            this.WORKINFORMATION.Controls.Add(this.TOTALQTY);
            this.WORKINFORMATION.Controls.Add(this.label14);
            this.WORKINFORMATION.Controls.Add(this.RECIPEID);
            this.WORKINFORMATION.Controls.Add(this.label12);
            this.WORKINFORMATION.Controls.Add(this.PRODUCTID);
            this.WORKINFORMATION.Controls.Add(this.label11);
            this.WORKINFORMATION.Controls.Add(this.MANAZINEID);
            this.WORKINFORMATION.Controls.Add(this.label10);
            this.WORKINFORMATION.Controls.Add(this.TRAYID);
            this.WORKINFORMATION.Controls.Add(this.label9);
            this.WORKINFORMATION.Controls.Add(this.LOTID1);
            this.WORKINFORMATION.Controls.Add(this.label8);
            this.WORKINFORMATION.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.WORKINFORMATION.Location = new System.Drawing.Point(218, 126);
            this.WORKINFORMATION.Name = "WORKINFORMATION";
            this.WORKINFORMATION.Size = new System.Drawing.Size(996, 106);
            this.WORKINFORMATION.TabIndex = 23;
            this.WORKINFORMATION.TabStop = false;
            this.WORKINFORMATION.Text = "WORK INFORMATION";
            // 
            // WORKQTY
            // 
            this.WORKQTY.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.WORKQTY.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.WORKQTY.Location = new System.Drawing.Point(851, 54);
            this.WORKQTY.Multiline = true;
            this.WORKQTY.Name = "WORKQTY";
            this.WORKQTY.Size = new System.Drawing.Size(135, 30);
            this.WORKQTY.TabIndex = 29;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label13.Location = new System.Drawing.Point(874, 24);
            this.label13.Margin = new System.Windows.Forms.Padding(5);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(95, 22);
            this.label13.TabIndex = 28;
            this.label13.Text = "WORKQTY";
            // 
            // TOTALQTY
            // 
            this.TOTALQTY.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.TOTALQTY.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TOTALQTY.Location = new System.Drawing.Point(711, 54);
            this.TOTALQTY.Multiline = true;
            this.TOTALQTY.Name = "TOTALQTY";
            this.TOTALQTY.Size = new System.Drawing.Size(135, 30);
            this.TOTALQTY.TabIndex = 27;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label14.Location = new System.Drawing.Point(731, 24);
            this.label14.Margin = new System.Windows.Forms.Padding(5);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(97, 22);
            this.label14.TabIndex = 26;
            this.label14.Text = "TOTALQTY";
            // 
            // RECIPEID
            // 
            this.RECIPEID.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.RECIPEID.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.RECIPEID.Location = new System.Drawing.Point(570, 54);
            this.RECIPEID.Multiline = true;
            this.RECIPEID.Name = "RECIPEID";
            this.RECIPEID.Size = new System.Drawing.Size(135, 30);
            this.RECIPEID.TabIndex = 25;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label12.Location = new System.Drawing.Point(595, 24);
            this.label12.Margin = new System.Windows.Forms.Padding(5);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(84, 22);
            this.label12.TabIndex = 24;
            this.label12.Text = "RECIPEID";
            // 
            // PRODUCTID
            // 
            this.PRODUCTID.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.PRODUCTID.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.PRODUCTID.Location = new System.Drawing.Point(429, 54);
            this.PRODUCTID.Multiline = true;
            this.PRODUCTID.Name = "PRODUCTID";
            this.PRODUCTID.Size = new System.Drawing.Size(135, 30);
            this.PRODUCTID.TabIndex = 23;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label11.Location = new System.Drawing.Point(440, 24);
            this.label11.Margin = new System.Windows.Forms.Padding(5);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(109, 22);
            this.label11.TabIndex = 22;
            this.label11.Text = "PRODUCTID";
            // 
            // MANAZINEID
            // 
            this.MANAZINEID.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.MANAZINEID.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.MANAZINEID.Location = new System.Drawing.Point(288, 54);
            this.MANAZINEID.Multiline = true;
            this.MANAZINEID.Name = "MANAZINEID";
            this.MANAZINEID.Size = new System.Drawing.Size(135, 30);
            this.MANAZINEID.TabIndex = 21;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label10.Location = new System.Drawing.Point(294, 24);
            this.label10.Margin = new System.Windows.Forms.Padding(5);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(120, 22);
            this.label10.TabIndex = 20;
            this.label10.Text = "MANAZINEID";
            // 
            // TRAYID
            // 
            this.TRAYID.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.TRAYID.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TRAYID.Location = new System.Drawing.Point(147, 54);
            this.TRAYID.Multiline = true;
            this.TRAYID.Name = "TRAYID";
            this.TRAYID.Size = new System.Drawing.Size(135, 30);
            this.TRAYID.TabIndex = 19;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label9.Location = new System.Drawing.Point(182, 24);
            this.label9.Margin = new System.Windows.Forms.Padding(5);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(71, 22);
            this.label9.TabIndex = 18;
            this.label9.Text = "TRAYID";
            // 
            // LOTID1
            // 
            this.LOTID1.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.LOTID1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.LOTID1.Location = new System.Drawing.Point(6, 54);
            this.LOTID1.Multiline = true;
            this.LOTID1.Name = "LOTID1";
            this.LOTID1.Size = new System.Drawing.Size(135, 30);
            this.LOTID1.TabIndex = 17;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label8.Location = new System.Drawing.Point(45, 24);
            this.label8.Margin = new System.Windows.Forms.Padding(5);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(60, 22);
            this.label8.TabIndex = 14;
            this.label8.Text = "LOTID";
            // 
            // menuStrip2
            // 
            this.menuStrip2.Dock = System.Windows.Forms.DockStyle.None;
            this.menuStrip2.GripMargin = new System.Windows.Forms.Padding(0);
            this.menuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.RMS,
            this.TKIN,
            this.OFFLINE});
            this.menuStrip2.Location = new System.Drawing.Point(3, 290);
            this.menuStrip2.Name = "menuStrip2";
            this.menuStrip2.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.menuStrip2.Size = new System.Drawing.Size(192, 27);
            this.menuStrip2.TabIndex = 37;
            this.menuStrip2.Text = "menuStrip2";
            // 
            // RMS
            // 
            this.RMS.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.RMS.Name = "RMS";
            this.RMS.Size = new System.Drawing.Size(60, 23);
            this.RMS.Text = "RMS";
            this.RMS.TextBoxTextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // TKIN
            // 
            this.TKIN.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.TKIN.Name = "TKIN";
            this.TKIN.Size = new System.Drawing.Size(60, 23);
            this.TKIN.Text = "TKIN";
            this.TKIN.TextBoxTextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // OFFLINE
            // 
            this.OFFLINE.AutoSize = false;
            this.OFFLINE.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.OFFLINE.Margin = new System.Windows.Forms.Padding(0);
            this.OFFLINE.Name = "OFFLINE";
            this.OFFLINE.Size = new System.Drawing.Size(60, 23);
            this.OFFLINE.Text = "OFFLINE";
            this.OFFLINE.TextBoxTextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.CLEAR2);
            this.groupBox1.Controls.Add(this.DEKIT);
            this.groupBox1.Controls.Add(this.KIT);
            this.groupBox1.Controls.Add(this.CLEAR1);
            this.groupBox1.Controls.Add(this.TKOUT);
            this.groupBox1.Controls.Add(this.button4);
            this.groupBox1.Controls.Add(this.textBox4);
            this.groupBox1.Controls.Add(this.label17);
            this.groupBox1.Controls.Add(this.textBox8);
            this.groupBox1.Controls.Add(this.label21);
            this.groupBox1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox1.Location = new System.Drawing.Point(218, 238);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(996, 106);
            this.groupBox1.TabIndex = 24;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "WORK INFORMATION";
            // 
            // textBox4
            // 
            this.textBox4.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.textBox4.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox4.Location = new System.Drawing.Point(505, 54);
            this.textBox4.Multiline = true;
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(135, 30);
            this.textBox4.TabIndex = 25;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label17.Location = new System.Drawing.Point(530, 24);
            this.label17.Margin = new System.Windows.Forms.Padding(5);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(84, 22);
            this.label17.TabIndex = 24;
            this.label17.Text = "RECIPEID";
            // 
            // textBox8
            // 
            this.textBox8.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.textBox8.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox8.Location = new System.Drawing.Point(6, 54);
            this.textBox8.Multiline = true;
            this.textBox8.Name = "textBox8";
            this.textBox8.Size = new System.Drawing.Size(135, 30);
            this.textBox8.TabIndex = 17;
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label21.Location = new System.Drawing.Point(45, 24);
            this.label21.Margin = new System.Windows.Forms.Padding(5);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(60, 22);
            this.label21.TabIndex = 14;
            this.label21.Text = "LOTID";
            // 
            // CLEAR1
            // 
            this.CLEAR1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.CLEAR1.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.CLEAR1.Location = new System.Drawing.Point(375, 36);
            this.CLEAR1.Name = "CLEAR1";
            this.CLEAR1.Size = new System.Drawing.Size(100, 48);
            this.CLEAR1.TabIndex = 35;
            this.CLEAR1.Text = "CLEAR";
            this.CLEAR1.UseVisualStyleBackColor = false;
            // 
            // TKOUT
            // 
            this.TKOUT.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.TKOUT.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.TKOUT.Location = new System.Drawing.Point(269, 36);
            this.TKOUT.Name = "TKOUT";
            this.TKOUT.Size = new System.Drawing.Size(100, 48);
            this.TKOUT.TabIndex = 34;
            this.TKOUT.Text = "TKOUT";
            this.TKOUT.UseVisualStyleBackColor = false;
            // 
            // button4
            // 
            this.button4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.button4.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button4.Location = new System.Drawing.Point(163, 37);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(100, 48);
            this.button4.TabIndex = 33;
            this.button4.Text = "TKIN";
            this.button4.UseVisualStyleBackColor = false;
            // 
            // CLEAR2
            // 
            this.CLEAR2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.CLEAR2.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.CLEAR2.Location = new System.Drawing.Point(877, 36);
            this.CLEAR2.Name = "CLEAR2";
            this.CLEAR2.Size = new System.Drawing.Size(100, 48);
            this.CLEAR2.TabIndex = 38;
            this.CLEAR2.Text = "CLEAR";
            this.CLEAR2.UseVisualStyleBackColor = false;
            // 
            // DEKIT
            // 
            this.DEKIT.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.DEKIT.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.DEKIT.Location = new System.Drawing.Point(771, 36);
            this.DEKIT.Name = "DEKIT";
            this.DEKIT.Size = new System.Drawing.Size(100, 48);
            this.DEKIT.TabIndex = 37;
            this.DEKIT.Text = "DEKIT";
            this.DEKIT.UseVisualStyleBackColor = false;
            // 
            // KIT
            // 
            this.KIT.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.KIT.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.KIT.Location = new System.Drawing.Point(665, 36);
            this.KIT.Name = "KIT";
            this.KIT.Size = new System.Drawing.Size(100, 48);
            this.KIT.TabIndex = 36;
            this.KIT.Text = "KIT";
            this.KIT.UseVisualStyleBackColor = false;
            // 
            // textBox23
            // 
            this.textBox23.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.textBox23.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textBox23.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox23.Location = new System.Drawing.Point(347, 450);
            this.textBox23.Multiline = true;
            this.textBox23.Name = "textBox23";
            this.textBox23.Size = new System.Drawing.Size(666, 59);
            this.textBox23.TabIndex = 25;
            this.textBox23.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.LOSS);
            this.groupBox2.Controls.Add(this.HISTORY);
            this.groupBox2.Controls.Add(this.WORK);
            this.groupBox2.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox2.Location = new System.Drawing.Point(15, 450);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(200, 205);
            this.groupBox2.TabIndex = 26;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Management";
            // 
            // LOSS
            // 
            this.LOSS.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.LOSS.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LOSS.Location = new System.Drawing.Point(15, 150);
            this.LOSS.Name = "LOSS";
            this.LOSS.Size = new System.Drawing.Size(168, 48);
            this.LOSS.TabIndex = 4;
            this.LOSS.Text = "LOSS";
            this.LOSS.UseVisualStyleBackColor = false;
            // 
            // HISTORY
            // 
            this.HISTORY.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.HISTORY.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.HISTORY.Location = new System.Drawing.Point(15, 96);
            this.HISTORY.Name = "HISTORY";
            this.HISTORY.Size = new System.Drawing.Size(168, 48);
            this.HISTORY.TabIndex = 3;
            this.HISTORY.Text = "HISTORY";
            this.HISTORY.UseVisualStyleBackColor = false;
            // 
            // WORK
            // 
            this.WORK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.WORK.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.WORK.Location = new System.Drawing.Point(15, 42);
            this.WORK.Name = "WORK";
            this.WORK.Size = new System.Drawing.Size(168, 48);
            this.WORK.TabIndex = 2;
            this.WORK.Text = "WORK";
            this.WORK.UseVisualStyleBackColor = false;
            // 
            // SAMSUNGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1264, 667);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.textBox23);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.WORKINFORMATION);
            this.Controls.Add(this.USERID);
            this.Controls.Add(this.IPADDRESS);
            this.Controls.Add(this.EQPID);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.STATE);
            this.Controls.Add(this.panel1);
            this.Name = "SAMSUNGUI";
            this.Text = "SAMSUNGUI";
            this.STATE.ResumeLayout(false);
            this.STATE.PerformLayout();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.WORKINFORMATION.ResumeLayout(false);
            this.WORKINFORMATION.PerformLayout();
            this.menuStrip2.ResumeLayout(false);
            this.menuStrip2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.GroupBox STATE;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button EESMachineStatus;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button MESMachineStatus;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button CONNECTED;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TextBox USERID;
        private System.Windows.Forms.TextBox IPADDRESS;
        private System.Windows.Forms.TextBox EQPID;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox WORKINFORMATION;
        private System.Windows.Forms.TextBox WORKQTY;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox TOTALQTY;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox RECIPEID;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox PRODUCTID;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox MANAZINEID;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox TRAYID;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox LOTID1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.MenuStrip menuStrip2;
        private System.Windows.Forms.ToolStripTextBox RMS;
        private System.Windows.Forms.ToolStripTextBox TKIN;
        private System.Windows.Forms.ToolStripTextBox OFFLINE;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox textBox8;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Button CLEAR1;
        private System.Windows.Forms.Button TKOUT;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button CLEAR2;
        private System.Windows.Forms.Button DEKIT;
        private System.Windows.Forms.Button KIT;
        private System.Windows.Forms.TextBox textBox23;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button LOSS;
        private System.Windows.Forms.Button HISTORY;
        private System.Windows.Forms.Button WORK;
    }
}