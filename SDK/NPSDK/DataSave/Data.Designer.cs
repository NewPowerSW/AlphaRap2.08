namespace DataSave
{
    public partial class Data
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.pnlRoot = new System.Windows.Forms.Panel();
            this.pnlRecords = new System.Windows.Forms.Panel();
            this.listView1 = new System.Windows.Forms.ListView();
            this.pnlRecordsHeader = new System.Windows.Forms.Panel();
            this.lblRecordsTitle = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.pnlStats = new System.Windows.Forms.Panel();
            this.pnlStatRecords = new System.Windows.Forms.Panel();
            this.lblStatRecordsValue = new System.Windows.Forms.Label();
            this.lblStatRecordsTitle = new System.Windows.Forms.Label();
            this.dotRecords = new System.Windows.Forms.Label();
            this.pnlStatDays = new System.Windows.Forms.Panel();
            this.lblStatDaysValue = new System.Windows.Forms.Label();
            this.lblStatDaysTitle = new System.Windows.Forms.Label();
            this.dotDays = new System.Windows.Forms.Label();
            this.pnlStatColumns = new System.Windows.Forms.Panel();
            this.lblStatColumnsValue = new System.Windows.Forms.Label();
            this.lblStatColumnsTitle = new System.Windows.Forms.Label();
            this.dotColumns = new System.Windows.Forms.Label();
            this.pnlFilter = new System.Windows.Forms.Panel();
            this.flowActions = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonToday = new System.Windows.Forms.Button();
            this.button7Days = new System.Windows.Forms.Button();
            this.button30Days = new System.Windows.Forms.Button();
            this.ProcudtDataSlect = new System.Windows.Forms.ComboBox();
            this.SelectType = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.flowDate = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.label2 = new System.Windows.Forms.Label();
            this.dateTimePicker2 = new System.Windows.Forms.DateTimePicker();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblHeader = new System.Windows.Forms.Label();
            this.spacer0 = new System.Windows.Forms.Panel();
            this.spacer1 = new System.Windows.Forms.Panel();
            this.spacer2 = new System.Windows.Forms.Panel();
            this.pnlRoot.SuspendLayout();
            this.pnlRecords.SuspendLayout();
            this.pnlRecordsHeader.SuspendLayout();
            this.pnlStats.SuspendLayout();
            this.pnlStatRecords.SuspendLayout();
            this.pnlStatDays.SuspendLayout();
            this.pnlStatColumns.SuspendLayout();
            this.pnlFilter.SuspendLayout();
            this.flowActions.SuspendLayout();
            this.flowDate.SuspendLayout();
            this.pnlHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlRoot
            // 
            this.pnlRoot.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(239)))), ((int)(((byte)(244)))));
            this.pnlRoot.Controls.Add(this.pnlRecords);
            this.pnlRoot.Controls.Add(this.spacer2);
            this.pnlRoot.Controls.Add(this.pnlStats);
            this.pnlRoot.Controls.Add(this.spacer1);
            this.pnlRoot.Controls.Add(this.pnlFilter);
            this.pnlRoot.Controls.Add(this.spacer0);
            this.pnlRoot.Controls.Add(this.pnlHeader);
            this.pnlRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRoot.Location = new System.Drawing.Point(0, 0);
            this.pnlRoot.Name = "pnlRoot";
            this.pnlRoot.Padding = new System.Windows.Forms.Padding(12);
            this.pnlRoot.Size = new System.Drawing.Size(980, 700);
            this.pnlRoot.TabIndex = 0;
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(58)))), ((int)(((byte)(109)))));
            this.pnlHeader.Controls.Add(this.lblHeader);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(12, 12);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(956, 42);
            this.pnlHeader.TabIndex = 0;
            // 
            // lblHeader
            // 
            this.lblHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHeader.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold);
            this.lblHeader.ForeColor = System.Drawing.Color.White;
            this.lblHeader.Location = new System.Drawing.Point(0, 0);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Padding = new System.Windows.Forms.Padding(14, 0, 0, 0);
            this.lblHeader.Size = new System.Drawing.Size(956, 42);
            this.lblHeader.TabIndex = 0;
            this.lblHeader.Text = "  Production data";
            this.lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // spacer0
            // 
            this.spacer0.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(239)))), ((int)(((byte)(244)))));
            this.spacer0.Dock = System.Windows.Forms.DockStyle.Top;
            this.spacer0.Location = new System.Drawing.Point(12, 54);
            this.spacer0.Name = "spacer0";
            this.spacer0.Size = new System.Drawing.Size(956, 10);
            this.spacer0.TabIndex = 10;
            // 
            // spacer1
            // 
            this.spacer1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(239)))), ((int)(((byte)(244)))));
            this.spacer1.Dock = System.Windows.Forms.DockStyle.Top;
            this.spacer1.Location = new System.Drawing.Point(12, 174);
            this.spacer1.Name = "spacer1";
            this.spacer1.Size = new System.Drawing.Size(956, 10);
            this.spacer1.TabIndex = 11;
            // 
            // spacer2
            // 
            this.spacer2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(239)))), ((int)(((byte)(244)))));
            this.spacer2.Dock = System.Windows.Forms.DockStyle.Top;
            this.spacer2.Location = new System.Drawing.Point(12, 256);
            this.spacer2.Name = "spacer2";
            this.spacer2.Size = new System.Drawing.Size(956, 10);
            this.spacer2.TabIndex = 12;
            // 
            // pnlFilter
            // 
            this.pnlFilter.BackColor = System.Drawing.Color.White;
            this.pnlFilter.Controls.Add(this.flowActions);
            this.pnlFilter.Controls.Add(this.flowDate);
            this.pnlFilter.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFilter.Location = new System.Drawing.Point(12, 54);
            this.pnlFilter.Margin = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.pnlFilter.Name = "pnlFilter";
            this.pnlFilter.Padding = new System.Windows.Forms.Padding(16, 12, 16, 12);
            this.pnlFilter.Size = new System.Drawing.Size(956, 110);
            this.pnlFilter.TabIndex = 1;
            this.pnlFilter.Paint += new System.Windows.Forms.PaintEventHandler(this.CardPanel_Paint);
            // 
            // flowDate
            // 
            this.flowDate.Controls.Add(this.label1);
            this.flowDate.Controls.Add(this.dateTimePicker1);
            this.flowDate.Controls.Add(this.label2);
            this.flowDate.Controls.Add(this.dateTimePicker2);
            this.flowDate.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowDate.Location = new System.Drawing.Point(16, 12);
            this.flowDate.Name = "flowDate";
            this.flowDate.Size = new System.Drawing.Size(924, 40);
            this.flowDate.TabIndex = 0;
            this.flowDate.WrapContents = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 9.5F);
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(130)))), ((int)(((byte)(145)))));
            this.label1.Location = new System.Drawing.Point(3, 8);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 8, 10, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 23);
            this.label1.TabIndex = 0;
            this.label1.Text = "Date range";
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.CalendarFont = new System.Drawing.Font("微软雅黑", 9.5F);
            this.dateTimePicker1.CustomFormat = "yyyy/MM/dd";
            this.dateTimePicker1.Font = new System.Drawing.Font("微软雅黑", 9.5F);
            this.dateTimePicker1.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker1.Location = new System.Drawing.Point(98, 5);
            this.dateTimePicker1.Margin = new System.Windows.Forms.Padding(3, 5, 8, 3);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(150, 28);
            this.dateTimePicker1.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("微软雅黑", 9.5F);
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(120)))), ((int)(((byte)(130)))), ((int)(((byte)(145)))));
            this.label2.Location = new System.Drawing.Point(259, 8);
            this.label2.Margin = new System.Windows.Forms.Padding(3, 8, 8, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(25, 23);
            this.label2.TabIndex = 2;
            this.label2.Text = "to";
            // 
            // dateTimePicker2
            // 
            this.dateTimePicker2.CalendarFont = new System.Drawing.Font("微软雅黑", 9.5F);
            this.dateTimePicker2.CustomFormat = "yyyy/MM/dd";
            this.dateTimePicker2.Font = new System.Drawing.Font("微软雅黑", 9.5F);
            this.dateTimePicker2.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker2.Location = new System.Drawing.Point(295, 5);
            this.dateTimePicker2.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
            this.dateTimePicker2.Name = "dateTimePicker2";
            this.dateTimePicker2.Size = new System.Drawing.Size(150, 28);
            this.dateTimePicker2.TabIndex = 3;
            // 
            // flowActions
            // 
            this.flowActions.Controls.Add(this.buttonToday);
            this.flowActions.Controls.Add(this.button7Days);
            this.flowActions.Controls.Add(this.button30Days);
            this.flowActions.Controls.Add(this.ProcudtDataSlect);
            this.flowActions.Controls.Add(this.SelectType);
            this.flowActions.Controls.Add(this.button1);
            this.flowActions.Controls.Add(this.button2);
            this.flowActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowActions.Location = new System.Drawing.Point(16, 52);
            this.flowActions.Name = "flowActions";
            this.flowActions.Size = new System.Drawing.Size(924, 46);
            this.flowActions.TabIndex = 1;
            this.flowActions.WrapContents = false;
            // 
            // buttonToday
            // 
            this.buttonToday.BackColor = System.Drawing.Color.White;
            this.buttonToday.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonToday.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(208)))), ((int)(((byte)(220)))));
            this.buttonToday.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(248)))), ((int)(((byte)(252)))));
            this.buttonToday.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonToday.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.buttonToday.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(60)))), ((int)(((byte)(75)))));
            this.buttonToday.Location = new System.Drawing.Point(0, 4);
            this.buttonToday.Margin = new System.Windows.Forms.Padding(0, 4, 8, 0);
            this.buttonToday.Name = "buttonToday";
            this.buttonToday.Size = new System.Drawing.Size(88, 34);
            this.buttonToday.TabIndex = 0;
            this.buttonToday.Text = "Today";
            this.buttonToday.UseVisualStyleBackColor = false;
            this.buttonToday.Click += new System.EventHandler(this.buttonToday_Click);
            // 
            // button7Days
            // 
            this.button7Days.BackColor = System.Drawing.Color.White;
            this.button7Days.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button7Days.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(208)))), ((int)(((byte)(220)))));
            this.button7Days.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(248)))), ((int)(((byte)(252)))));
            this.button7Days.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button7Days.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.button7Days.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(60)))), ((int)(((byte)(75)))));
            this.button7Days.Location = new System.Drawing.Point(96, 4);
            this.button7Days.Margin = new System.Windows.Forms.Padding(0, 4, 8, 0);
            this.button7Days.Name = "button7Days";
            this.button7Days.Size = new System.Drawing.Size(110, 34);
            this.button7Days.TabIndex = 1;
            this.button7Days.Text = "Last 7 days";
            this.button7Days.UseVisualStyleBackColor = false;
            this.button7Days.Click += new System.EventHandler(this.button7Days_Click);
            // 
            // button30Days
            // 
            this.button30Days.BackColor = System.Drawing.Color.White;
            this.button30Days.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button30Days.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(208)))), ((int)(((byte)(220)))));
            this.button30Days.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(248)))), ((int)(((byte)(252)))));
            this.button30Days.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button30Days.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.button30Days.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(60)))), ((int)(((byte)(75)))));
            this.button30Days.Location = new System.Drawing.Point(214, 4);
            this.button30Days.Margin = new System.Windows.Forms.Padding(0, 4, 16, 0);
            this.button30Days.Name = "button30Days";
            this.button30Days.Size = new System.Drawing.Size(118, 34);
            this.button30Days.TabIndex = 2;
            this.button30Days.Text = "Last 30 days";
            this.button30Days.UseVisualStyleBackColor = false;
            this.button30Days.Click += new System.EventHandler(this.button30Days_Click);
            // 
            // ProcudtDataSlect
            // 
            this.ProcudtDataSlect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ProcudtDataSlect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ProcudtDataSlect.Font = new System.Drawing.Font("微软雅黑", 9.5F);
            this.ProcudtDataSlect.FormattingEnabled = true;
            this.ProcudtDataSlect.Location = new System.Drawing.Point(348, 6);
            this.ProcudtDataSlect.Margin = new System.Windows.Forms.Padding(0, 6, 8, 0);
            this.ProcudtDataSlect.Name = "ProcudtDataSlect";
            this.ProcudtDataSlect.Size = new System.Drawing.Size(180, 27);
            this.ProcudtDataSlect.TabIndex = 3;
            // 
            // SelectType
            // 
            this.SelectType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SelectType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SelectType.Font = new System.Drawing.Font("微软雅黑", 9.5F);
            this.SelectType.FormattingEnabled = true;
            this.SelectType.Items.AddRange(new object[] {
            "ALL",
            "PASS",
            "FAIL"});
            this.SelectType.Location = new System.Drawing.Point(536, 6);
            this.SelectType.Margin = new System.Windows.Forms.Padding(0, 6, 16, 0);
            this.SelectType.Name = "SelectType";
            this.SelectType.Size = new System.Drawing.Size(100, 27);
            this.SelectType.TabIndex = 4;
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.White;
            this.button1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button1.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(208)))), ((int)(((byte)(220)))));
            this.button1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("微软雅黑", 9.5F, System.Drawing.FontStyle.Bold);
            this.button1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(50)))), ((int)(((byte)(65)))));
            this.button1.Location = new System.Drawing.Point(652, 4);
            this.button1.Margin = new System.Windows.Forms.Padding(0, 4, 8, 0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 34);
            this.button1.TabIndex = 5;
            this.button1.Text = "Query";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.White;
            this.button2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.button2.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(208)))), ((int)(((byte)(220)))));
            this.button2.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(246)))), ((int)(((byte)(255)))));
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Font = new System.Drawing.Font("微软雅黑", 9.5F, System.Drawing.FontStyle.Bold);
            this.button2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(50)))), ((int)(((byte)(65)))));
            this.button2.Location = new System.Drawing.Point(760, 4);
            this.button2.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(100, 34);
            this.button2.TabIndex = 6;
            this.button2.Text = "Export";
            this.button2.UseVisualStyleBackColor = false;
            // 
            // pnlStats
            // 
            this.pnlStats.BackColor = System.Drawing.Color.White;
            this.pnlStats.Controls.Add(this.pnlStatColumns);
            this.pnlStats.Controls.Add(this.pnlStatDays);
            this.pnlStats.Controls.Add(this.pnlStatRecords);
            this.pnlStats.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlStats.Location = new System.Drawing.Point(12, 172);
            this.pnlStats.Name = "pnlStats";
            this.pnlStats.Padding = new System.Windows.Forms.Padding(8);
            this.pnlStats.Size = new System.Drawing.Size(956, 72);
            this.pnlStats.TabIndex = 2;
            this.pnlStats.Paint += new System.Windows.Forms.PaintEventHandler(this.CardPanel_Paint);
            // 
            // pnlStatRecords
            // 
            this.pnlStatRecords.Controls.Add(this.lblStatRecordsValue);
            this.pnlStatRecords.Controls.Add(this.lblStatRecordsTitle);
            this.pnlStatRecords.Controls.Add(this.dotRecords);
            this.pnlStatRecords.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlStatRecords.Location = new System.Drawing.Point(8, 8);
            this.pnlStatRecords.Name = "pnlStatRecords";
            this.pnlStatRecords.Size = new System.Drawing.Size(300, 56);
            this.pnlStatRecords.TabIndex = 0;
            // 
            // dotRecords
            // 
            this.dotRecords.Font = new System.Drawing.Font("微软雅黑", 14F, System.Drawing.FontStyle.Bold);
            this.dotRecords.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.dotRecords.Location = new System.Drawing.Point(18, 14);
            this.dotRecords.Name = "dotRecords";
            this.dotRecords.Size = new System.Drawing.Size(24, 28);
            this.dotRecords.TabIndex = 0;
            this.dotRecords.Text = "●";
            // 
            // lblStatRecordsTitle
            // 
            this.lblStatRecordsTitle.AutoSize = true;
            this.lblStatRecordsTitle.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblStatRecordsTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(100)))), ((int)(((byte)(115)))));
            this.lblStatRecordsTitle.Location = new System.Drawing.Point(44, 16);
            this.lblStatRecordsTitle.Name = "lblStatRecordsTitle";
            this.lblStatRecordsTitle.Size = new System.Drawing.Size(74, 23);
            this.lblStatRecordsTitle.TabIndex = 1;
            this.lblStatRecordsTitle.Text = "Records";
            // 
            // lblStatRecordsValue
            // 
            this.lblStatRecordsValue.AutoSize = true;
            this.lblStatRecordsValue.Font = new System.Drawing.Font("微软雅黑", 16F, System.Drawing.FontStyle.Bold);
            this.lblStatRecordsValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.lblStatRecordsValue.Location = new System.Drawing.Point(130, 8);
            this.lblStatRecordsValue.Name = "lblStatRecordsValue";
            this.lblStatRecordsValue.Size = new System.Drawing.Size(30, 36);
            this.lblStatRecordsValue.TabIndex = 2;
            this.lblStatRecordsValue.Text = "0";
            // 
            // pnlStatDays
            // 
            this.pnlStatDays.Controls.Add(this.lblStatDaysValue);
            this.pnlStatDays.Controls.Add(this.lblStatDaysTitle);
            this.pnlStatDays.Controls.Add(this.dotDays);
            this.pnlStatDays.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlStatDays.Location = new System.Drawing.Point(308, 8);
            this.pnlStatDays.Name = "pnlStatDays";
            this.pnlStatDays.Size = new System.Drawing.Size(300, 56);
            this.pnlStatDays.TabIndex = 1;
            // 
            // dotDays
            // 
            this.dotDays.Font = new System.Drawing.Font("微软雅黑", 14F, System.Drawing.FontStyle.Bold);
            this.dotDays.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(184)))), ((int)(((byte)(92)))));
            this.dotDays.Location = new System.Drawing.Point(18, 14);
            this.dotDays.Name = "dotDays";
            this.dotDays.Size = new System.Drawing.Size(24, 28);
            this.dotDays.TabIndex = 0;
            this.dotDays.Text = "●";
            // 
            // lblStatDaysTitle
            // 
            this.lblStatDaysTitle.AutoSize = true;
            this.lblStatDaysTitle.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblStatDaysTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(100)))), ((int)(((byte)(115)))));
            this.lblStatDaysTitle.Location = new System.Drawing.Point(44, 16);
            this.lblStatDaysTitle.Name = "lblStatDaysTitle";
            this.lblStatDaysTitle.Size = new System.Drawing.Size(51, 23);
            this.lblStatDaysTitle.TabIndex = 1;
            this.lblStatDaysTitle.Text = "Days";
            // 
            // lblStatDaysValue
            // 
            this.lblStatDaysValue.AutoSize = true;
            this.lblStatDaysValue.Font = new System.Drawing.Font("微软雅黑", 16F, System.Drawing.FontStyle.Bold);
            this.lblStatDaysValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(184)))), ((int)(((byte)(92)))));
            this.lblStatDaysValue.Location = new System.Drawing.Point(110, 8);
            this.lblStatDaysValue.Name = "lblStatDaysValue";
            this.lblStatDaysValue.Size = new System.Drawing.Size(30, 36);
            this.lblStatDaysValue.TabIndex = 2;
            this.lblStatDaysValue.Text = "0";
            // 
            // pnlStatColumns
            // 
            this.pnlStatColumns.Controls.Add(this.lblStatColumnsValue);
            this.pnlStatColumns.Controls.Add(this.lblStatColumnsTitle);
            this.pnlStatColumns.Controls.Add(this.dotColumns);
            this.pnlStatColumns.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlStatColumns.Location = new System.Drawing.Point(608, 8);
            this.pnlStatColumns.Name = "pnlStatColumns";
            this.pnlStatColumns.Size = new System.Drawing.Size(300, 56);
            this.pnlStatColumns.TabIndex = 2;
            // 
            // dotColumns
            // 
            this.dotColumns.Font = new System.Drawing.Font("微软雅黑", 14F, System.Drawing.FontStyle.Bold);
            this.dotColumns.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(170)))), ((int)(((byte)(40)))));
            this.dotColumns.Location = new System.Drawing.Point(18, 14);
            this.dotColumns.Name = "dotColumns";
            this.dotColumns.Size = new System.Drawing.Size(24, 28);
            this.dotColumns.TabIndex = 0;
            this.dotColumns.Text = "●";
            // 
            // lblStatColumnsTitle
            // 
            this.lblStatColumnsTitle.AutoSize = true;
            this.lblStatColumnsTitle.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblStatColumnsTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(100)))), ((int)(((byte)(115)))));
            this.lblStatColumnsTitle.Location = new System.Drawing.Point(44, 16);
            this.lblStatColumnsTitle.Name = "lblStatColumnsTitle";
            this.lblStatColumnsTitle.Size = new System.Drawing.Size(78, 23);
            this.lblStatColumnsTitle.TabIndex = 1;
            this.lblStatColumnsTitle.Text = "Columns";
            // 
            // lblStatColumnsValue
            // 
            this.lblStatColumnsValue.AutoSize = true;
            this.lblStatColumnsValue.Font = new System.Drawing.Font("微软雅黑", 16F, System.Drawing.FontStyle.Bold);
            this.lblStatColumnsValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(170)))), ((int)(((byte)(40)))));
            this.lblStatColumnsValue.Location = new System.Drawing.Point(140, 8);
            this.lblStatColumnsValue.Name = "lblStatColumnsValue";
            this.lblStatColumnsValue.Size = new System.Drawing.Size(30, 36);
            this.lblStatColumnsValue.TabIndex = 2;
            this.lblStatColumnsValue.Text = "0";
            // 
            // pnlRecords
            // 
            this.pnlRecords.BackColor = System.Drawing.Color.White;
            this.pnlRecords.Controls.Add(this.listView1);
            this.pnlRecords.Controls.Add(this.pnlRecordsHeader);
            this.pnlRecords.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRecords.Location = new System.Drawing.Point(12, 252);
            this.pnlRecords.Name = "pnlRecords";
            this.pnlRecords.Padding = new System.Windows.Forms.Padding(12, 8, 12, 12);
            this.pnlRecords.Size = new System.Drawing.Size(956, 436);
            this.pnlRecords.TabIndex = 3;
            this.pnlRecords.Paint += new System.Windows.Forms.PaintEventHandler(this.CardPanel_Paint);
            // 
            // pnlRecordsHeader
            // 
            this.pnlRecordsHeader.Controls.Add(this.lblStatus);
            this.pnlRecordsHeader.Controls.Add(this.lblRecordsTitle);
            this.pnlRecordsHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlRecordsHeader.Location = new System.Drawing.Point(12, 8);
            this.pnlRecordsHeader.Name = "pnlRecordsHeader";
            this.pnlRecordsHeader.Size = new System.Drawing.Size(932, 36);
            this.pnlRecordsHeader.TabIndex = 0;
            // 
            // lblRecordsTitle
            // 
            this.lblRecordsTitle.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblRecordsTitle.Font = new System.Drawing.Font("微软雅黑", 11F, System.Drawing.FontStyle.Bold);
            this.lblRecordsTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(58)))), ((int)(((byte)(109)))));
            this.lblRecordsTitle.Location = new System.Drawing.Point(0, 0);
            this.lblRecordsTitle.Name = "lblRecordsTitle";
            this.lblRecordsTitle.Size = new System.Drawing.Size(120, 36);
            this.lblRecordsTitle.TabIndex = 0;
            this.lblRecordsTitle.Text = "Records";
            this.lblRecordsTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblStatus
            // 
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatus.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(150)))), ((int)(((byte)(158)))), ((int)(((byte)(170)))));
            this.lblStatus.Location = new System.Drawing.Point(120, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(812, 36);
            this.lblStatus.TabIndex = 1;
            this.lblStatus.Text = "Select a date range and click Query";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // listView1
            // 
            this.listView1.BackColor = System.Drawing.Color.White;
            this.listView1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.Font = new System.Drawing.Font("微软雅黑", 9.5F);
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(12, 44);
            this.listView1.Name = "listView1";
            this.listView1.OwnerDraw = true;
            this.listView1.Size = new System.Drawing.Size(932, 380);
            this.listView1.TabIndex = 1;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.listView1_DrawColumnHeader);
            this.listView1.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.listView1_DrawItem);
            this.listView1.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listView1_ItemSelectionChanged);
            // 
            // Data
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(239)))), ((int)(((byte)(244)))));
            this.Controls.Add(this.pnlRoot);
            this.Name = "Data";
            this.Size = new System.Drawing.Size(980, 700);
            this.Load += new System.EventHandler(this.Data_Load);
            this.pnlRoot.ResumeLayout(false);
            this.pnlRecords.ResumeLayout(false);
            this.pnlRecordsHeader.ResumeLayout(false);
            this.pnlStats.ResumeLayout(false);
            this.pnlStatRecords.ResumeLayout(false);
            this.pnlStatRecords.PerformLayout();
            this.pnlStatDays.ResumeLayout(false);
            this.pnlStatDays.PerformLayout();
            this.pnlStatColumns.ResumeLayout(false);
            this.pnlStatColumns.PerformLayout();
            this.pnlFilter.ResumeLayout(false);
            this.flowActions.ResumeLayout(false);
            this.flowDate.ResumeLayout(false);
            this.flowDate.PerformLayout();
            this.pnlHeader.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlRoot;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.Panel spacer0;
        private System.Windows.Forms.Panel spacer1;
        private System.Windows.Forms.Panel spacer2;
        private System.Windows.Forms.Panel pnlFilter;
        private System.Windows.Forms.FlowLayoutPanel flowDate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker dateTimePicker2;
        private System.Windows.Forms.FlowLayoutPanel flowActions;
        private System.Windows.Forms.Button buttonToday;
        private System.Windows.Forms.Button button7Days;
        private System.Windows.Forms.Button button30Days;
        private System.Windows.Forms.ComboBox ProcudtDataSlect;
        private System.Windows.Forms.ComboBox SelectType;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Panel pnlStats;
        private System.Windows.Forms.Panel pnlStatRecords;
        private System.Windows.Forms.Label dotRecords;
        private System.Windows.Forms.Label lblStatRecordsTitle;
        private System.Windows.Forms.Label lblStatRecordsValue;
        private System.Windows.Forms.Panel pnlStatDays;
        private System.Windows.Forms.Label dotDays;
        private System.Windows.Forms.Label lblStatDaysTitle;
        private System.Windows.Forms.Label lblStatDaysValue;
        private System.Windows.Forms.Panel pnlStatColumns;
        private System.Windows.Forms.Label dotColumns;
        private System.Windows.Forms.Label lblStatColumnsTitle;
        private System.Windows.Forms.Label lblStatColumnsValue;
        private System.Windows.Forms.Panel pnlRecords;
        private System.Windows.Forms.Panel pnlRecordsHeader;
        private System.Windows.Forms.Label lblRecordsTitle;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ListView listView1;
    }
}
