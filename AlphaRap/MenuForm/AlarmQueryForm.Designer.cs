namespace AlphaRap
{
    partial class AlarmQueryForm
    {
        /// <summary>设计器所需的组件容器。</summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>释放窗体占用的资源。</summary>
        /// <param name="disposing">true 表示释放托管资源。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.pnlTitle = new System.Windows.Forms.Panel();
            this.picTitleIcon = new System.Windows.Forms.PictureBox();
            this.lblTitle = new AlphaRap.UiLabel();
            this.flowCap = new System.Windows.Forms.FlowLayoutPanel();
            this.btnMin = new AlphaRap.FlatButton();
            this.btnClose = new AlphaRap.FlatButton();
            this.pnlTitleLine = new System.Windows.Forms.Panel();
            this.gripResize = new System.Windows.Forms.Panel();
            this.rootTable = new System.Windows.Forms.TableLayoutPanel();
            this.pnlBarOuter = new AlphaRap.CardPanel();
            this.pnlBarInner = new System.Windows.Forms.Panel();
            this.dateFrom = new AlphaRap.DateField();
            this.lblRangeDash = new AlphaRap.UiLabel();
            this.dateTo = new AlphaRap.DateField();
            this.btnToday = new AlphaRap.FlatButton();
            this.btnWeek = new AlphaRap.FlatButton();
            this.btnMonth = new AlphaRap.FlatButton();
            this.btnQuery = new AlphaRap.FlatButton();
            this.btnExport = new AlphaRap.FlatButton();
            this.segType = new AlphaRap.SegmentedControl();
            this.boxCode = new AlphaRap.FieldBox();
            this.boxKeyword = new AlphaRap.FieldBox();
            this.statStrip1 = new AlphaRap.StatStrip();
            this.tblCharts = new System.Windows.Forms.TableLayoutPanel();
            this.chartRank = new AlphaRap.BarChart();
            this.chartTrend = new AlphaRap.BarChart();
            this.pnlCardOuter = new AlphaRap.CardPanel();
            this.pnlCardInner = new System.Windows.Forms.Panel();
            this.grdAlarm = new System.Windows.Forms.DataGridView();
            this.colTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStop = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colContent = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRecipe = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUser = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlCardHead = new System.Windows.Forms.Panel();
            this.lblDetail = new AlphaRap.UiLabel();
            this.lblStatus = new AlphaRap.UiLabel();
            this.pnlTitle.SuspendLayout();
            this.flowCap.SuspendLayout();
            this.rootTable.SuspendLayout();
            this.pnlBarOuter.SuspendLayout();
            this.pnlBarInner.SuspendLayout();
            this.tblCharts.SuspendLayout();
            this.pnlCardOuter.SuspendLayout();
            this.pnlCardInner.SuspendLayout();
            this.pnlCardHead.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picTitleIcon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.grdAlarm)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlTitle
            // 
            this.pnlTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(108)))), ((int)(((byte)(182)))));
            this.pnlTitle.Controls.Add(this.picTitleIcon);
            this.pnlTitle.Controls.Add(this.lblTitle);
            this.pnlTitle.Controls.Add(this.flowCap);
            this.pnlTitle.Controls.Add(this.pnlTitleLine);
            this.pnlTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTitle.Location = new System.Drawing.Point(0, 0);
            this.pnlTitle.Name = "pnlTitle";
            this.pnlTitle.Size = new System.Drawing.Size(1180, 46);
            this.pnlTitle.TabIndex = 0;
            // 
            // picTitleIcon
            // 
            this.picTitleIcon.BackColor = System.Drawing.Color.Transparent;
            this.picTitleIcon.Location = new System.Drawing.Point(20, 13);
            this.picTitleIcon.Name = "picTitleIcon";
            this.picTitleIcon.Size = new System.Drawing.Size(20, 20);
            this.picTitleIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picTitleIcon.TabIndex = 0;
            this.picTitleIcon.TabStop = false;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = false;
            this.lblTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblTitle.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(50, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(700, 45);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "报警统计与历史查询";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // flowCap
            // 
            this.flowCap.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(108)))), ((int)(((byte)(182)))));
            this.flowCap.Controls.Add(this.btnMin);
            this.flowCap.Controls.Add(this.btnClose);
            this.flowCap.Dock = System.Windows.Forms.DockStyle.Right;
            this.flowCap.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.flowCap.Location = new System.Drawing.Point(1088, 0);
            this.flowCap.Margin = new System.Windows.Forms.Padding(0);
            this.flowCap.Name = "flowCap";
            this.flowCap.Padding = new System.Windows.Forms.Padding(0, 7, 12, 0);
            this.flowCap.Size = new System.Drawing.Size(92, 45);
            this.flowCap.TabIndex = 2;
            this.flowCap.WrapContents = false;
            // 
            // btnMin
            // 
            this.btnMin.CornerRadius = 7;
            this.btnMin.IconGap = 0;
            this.btnMin.IconSize = 14;
            this.btnMin.Location = new System.Drawing.Point(0, 7);
            this.btnMin.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.btnMin.Name = "btnMin";
            this.btnMin.Size = new System.Drawing.Size(38, 30);
            this.btnMin.TabIndex = 0;
            this.btnMin.Variant = AlphaRap.FlatButtonVariant.Ghost;
            // 
            // btnClose
            // 
            this.btnClose.CornerRadius = 7;
            this.btnClose.IconGap = 0;
            this.btnClose.IconSize = 14;
            this.btnClose.Location = new System.Drawing.Point(42, 7);
            this.btnClose.Margin = new System.Windows.Forms.Padding(0);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(38, 30);
            this.btnClose.TabIndex = 1;
            this.btnClose.Variant = AlphaRap.FlatButtonVariant.Ghost;
            // 
            // pnlTitleLine
            // 
            this.pnlTitleLine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(2)))), ((int)(((byte)(78)))), ((int)(((byte)(133)))));
            this.pnlTitleLine.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlTitleLine.Location = new System.Drawing.Point(0, 45);
            this.pnlTitleLine.Name = "pnlTitleLine";
            this.pnlTitleLine.Size = new System.Drawing.Size(1180, 1);
            this.pnlTitleLine.TabIndex = 3;
            // 
            // gripResize
            // 
            this.gripResize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.gripResize.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(243)))), ((int)(((byte)(248)))));
            this.gripResize.Cursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.gripResize.Location = new System.Drawing.Point(1158, 786);
            this.gripResize.Name = "gripResize";
            this.gripResize.Size = new System.Drawing.Size(22, 20);
            this.gripResize.TabIndex = 4;
            // 
            // rootTable
            // 
            this.rootTable.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(243)))), ((int)(((byte)(248)))));
            this.rootTable.ColumnCount = 1;
            this.rootTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootTable.Controls.Add(this.pnlBarOuter, 0, 0);
            this.rootTable.Controls.Add(this.statStrip1, 0, 1);
            this.rootTable.Controls.Add(this.tblCharts, 0, 2);
            this.rootTable.Controls.Add(this.pnlCardOuter, 0, 3);
            this.rootTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rootTable.Location = new System.Drawing.Point(0, 46);
            this.rootTable.Name = "rootTable";
            this.rootTable.Padding = new System.Windows.Forms.Padding(16, 14, 22, 20);
            this.rootTable.RowCount = 4;
            this.rootTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.rootTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 102F));
            this.rootTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 204F));
            this.rootTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootTable.Size = new System.Drawing.Size(1180, 760);
            this.rootTable.TabIndex = 0;
            // 
            // pnlBarOuter
            // 
            this.pnlBarOuter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(231)))), ((int)(((byte)(240)))));
            this.pnlBarOuter.Controls.Add(this.pnlBarInner);
            this.pnlBarOuter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBarOuter.Location = new System.Drawing.Point(16, 14);
            this.pnlBarOuter.Margin = new System.Windows.Forms.Padding(0, 0, 0, 14);
            this.pnlBarOuter.Name = "pnlBarOuter";
            this.pnlBarOuter.Padding = new System.Windows.Forms.Padding(1, 1, 1, 4);
            this.pnlBarOuter.Size = new System.Drawing.Size(1142, 106);
            this.pnlBarOuter.TabIndex = 0;
            // 
            // pnlBarInner
            // 
            this.pnlBarInner.BackColor = System.Drawing.Color.White;
            this.pnlBarInner.Controls.Add(this.dateFrom);
            this.pnlBarInner.Controls.Add(this.lblRangeDash);
            this.pnlBarInner.Controls.Add(this.dateTo);
            this.pnlBarInner.Controls.Add(this.btnToday);
            this.pnlBarInner.Controls.Add(this.btnWeek);
            this.pnlBarInner.Controls.Add(this.btnMonth);
            this.pnlBarInner.Controls.Add(this.btnQuery);
            this.pnlBarInner.Controls.Add(this.btnExport);
            this.pnlBarInner.Controls.Add(this.segType);
            this.pnlBarInner.Controls.Add(this.boxCode);
            this.pnlBarInner.Controls.Add(this.boxKeyword);
            this.pnlBarInner.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBarInner.Location = new System.Drawing.Point(1, 1);
            this.pnlBarInner.Name = "pnlBarInner";
            this.pnlBarInner.Size = new System.Drawing.Size(1140, 104);
            this.pnlBarInner.TabIndex = 0;
            // 
            // dateFrom
            // 
            this.dateFrom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(252)))), ((int)(((byte)(254)))));
            this.dateFrom.Location = new System.Drawing.Point(18, 14);
            this.dateFrom.Name = "dateFrom";
            this.dateFrom.Size = new System.Drawing.Size(200, 32);
            this.dateFrom.TabIndex = 1;
            // 
            // lblRangeDash
            // 
            this.lblRangeDash.AutoSize = false;
            this.lblRangeDash.BackColor = System.Drawing.Color.Transparent;
            this.lblRangeDash.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblRangeDash.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(146)))), ((int)(((byte)(160)))), ((int)(((byte)(176)))));
            this.lblRangeDash.Location = new System.Drawing.Point(244, 14);
            this.lblRangeDash.Name = "lblRangeDash";
            this.lblRangeDash.Size = new System.Drawing.Size(16, 32);
            this.lblRangeDash.TabIndex = 2;
            this.lblRangeDash.Text = "–";
            this.lblRangeDash.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // dateTo
            // 
            this.dateTo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(252)))), ((int)(((byte)(254)))));
            this.dateTo.Location = new System.Drawing.Point(266, 14);
            this.dateTo.Name = "dateTo";
            this.dateTo.Size = new System.Drawing.Size(200, 32);
            this.dateTo.TabIndex = 4;
            // 
            // btnToday
            // 
            this.btnToday.CornerRadius = 8;
            this.btnToday.IconGap = 0;
            this.btnToday.IconSize = 16;
            this.btnToday.Location = new System.Drawing.Point(520, 14);
            this.btnToday.Margin = new System.Windows.Forms.Padding(0);
            this.btnToday.Name = "btnToday";
            this.btnToday.Size = new System.Drawing.Size(66, 32);
            this.btnToday.TabIndex = 5;
            this.btnToday.Text = "今天";
            this.btnToday.Variant = AlphaRap.FlatButtonVariant.Surface;
            // 
            // btnWeek
            // 
            this.btnWeek.CornerRadius = 8;
            this.btnWeek.IconGap = 0;
            this.btnWeek.IconSize = 16;
            this.btnWeek.Location = new System.Drawing.Point(592, 14);
            this.btnWeek.Margin = new System.Windows.Forms.Padding(0);
            this.btnWeek.Name = "btnWeek";
            this.btnWeek.Size = new System.Drawing.Size(74, 32);
            this.btnWeek.TabIndex = 6;
            this.btnWeek.Text = "近 7 天";
            this.btnWeek.Variant = AlphaRap.FlatButtonVariant.Surface;
            // 
            // btnMonth
            // 
            this.btnMonth.CornerRadius = 8;
            this.btnMonth.IconGap = 0;
            this.btnMonth.IconSize = 16;
            this.btnMonth.Location = new System.Drawing.Point(672, 14);
            this.btnMonth.Margin = new System.Windows.Forms.Padding(0);
            this.btnMonth.Name = "btnMonth";
            this.btnMonth.Size = new System.Drawing.Size(84, 32);
            this.btnMonth.TabIndex = 7;
            this.btnMonth.Text = "近 30 天";
            this.btnMonth.Variant = AlphaRap.FlatButtonVariant.Surface;
            // 
            // btnQuery
            // 
            this.btnQuery.CornerRadius = 8;
            this.btnQuery.IconGap = 7;
            this.btnQuery.IconSize = 16;
            this.btnQuery.Location = new System.Drawing.Point(924, 14);
            this.btnQuery.Margin = new System.Windows.Forms.Padding(0);
            this.btnQuery.Name = "btnQuery";
            this.btnQuery.Size = new System.Drawing.Size(94, 32);
            this.btnQuery.TabIndex = 8;
            this.btnQuery.Text = "查询";
            this.btnQuery.Variant = AlphaRap.FlatButtonVariant.Primary;
            // 
            // btnExport
            // 
            this.btnExport.CornerRadius = 8;
            this.btnExport.IconGap = 7;
            this.btnExport.IconSize = 16;
            this.btnExport.Location = new System.Drawing.Point(1028, 14);
            this.btnExport.Margin = new System.Windows.Forms.Padding(0);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(94, 32);
            this.btnExport.TabIndex = 9;
            this.btnExport.Text = "导出";
            this.btnExport.Variant = AlphaRap.FlatButtonVariant.Surface;
            // 
            // segType
            // 
            this.segType.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(245)))), ((int)(((byte)(250)))));
            this.segType.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.segType.Location = new System.Drawing.Point(18, 58);
            this.segType.Name = "segType";
            this.segType.Size = new System.Drawing.Size(246, 32);
            this.segType.TabIndex = 10;
            // 
            // boxCode
            // 
            this.boxCode.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(252)))), ((int)(((byte)(254)))));
            this.boxCode.Location = new System.Drawing.Point(284, 58);
            this.boxCode.Name = "boxCode_dyn";
            this.boxCode.Size = new System.Drawing.Size(120, 32);
            this.boxCode.TabIndex = 11;
            // 
            // boxKeyword
            // 
            this.boxKeyword.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(252)))), ((int)(((byte)(254)))));
            this.boxKeyword.Location = new System.Drawing.Point(416, 58);
            this.boxKeyword.Name = "boxKeyword_dyn";
            this.boxKeyword.Size = new System.Drawing.Size(706, 32);
            this.boxKeyword.TabIndex = 12;
            // 
            // statStrip1
            // 
            this.statStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(243)))), ((int)(((byte)(248)))));
            this.statStrip1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.statStrip1.Location = new System.Drawing.Point(16, 134);
            this.statStrip1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 14);
            this.statStrip1.Name = "statStrip1";
            this.statStrip1.Size = new System.Drawing.Size(1142, 88);
            this.statStrip1.TabIndex = 1;
            // 
            // tblCharts
            // 
            this.tblCharts.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(243)))), ((int)(((byte)(248)))));
            this.tblCharts.ColumnCount = 2;
            this.tblCharts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 474F));
            this.tblCharts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblCharts.Controls.Add(this.chartRank, 0, 0);
            this.tblCharts.Controls.Add(this.chartTrend, 1, 0);
            this.tblCharts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblCharts.Location = new System.Drawing.Point(16, 236);
            this.tblCharts.Margin = new System.Windows.Forms.Padding(0, 0, 0, 14);
            this.tblCharts.Name = "tblCharts";
            this.tblCharts.RowCount = 1;
            this.tblCharts.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblCharts.Size = new System.Drawing.Size(1142, 190);
            this.tblCharts.TabIndex = 2;
            // 
            // chartRank
            // 
            this.chartRank.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(243)))), ((int)(((byte)(248)))));
            this.chartRank.BarColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(108)))), ((int)(((byte)(182)))));
            this.chartRank.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartRank.Location = new System.Drawing.Point(0, 0);
            this.chartRank.Margin = new System.Windows.Forms.Padding(0, 0, 14, 0);
            this.chartRank.MaxBars = 10;
            this.chartRank.Name = "chartRank";
            this.chartRank.Orientation = AlphaRap.BarChartOrientation.Horizontal;
            this.chartRank.Size = new System.Drawing.Size(460, 190);
            this.chartRank.TabIndex = 0;
            // 
            // chartTrend
            // 
            this.chartTrend.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(243)))), ((int)(((byte)(248)))));
            this.chartTrend.BarColor = System.Drawing.Color.FromArgb(((int)(((byte)(112)))), ((int)(((byte)(164)))), ((int)(((byte)(208)))));
            this.chartTrend.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartTrend.Location = new System.Drawing.Point(474, 0);
            this.chartTrend.Margin = new System.Windows.Forms.Padding(0);
            this.chartTrend.Name = "chartTrend";
            this.chartTrend.Orientation = AlphaRap.BarChartOrientation.Vertical;
            this.chartTrend.Size = new System.Drawing.Size(668, 190);
            this.chartTrend.TabIndex = 1;
            // 
            // pnlCardOuter
            // 
            this.pnlCardOuter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(231)))), ((int)(((byte)(240)))));
            this.pnlCardOuter.Controls.Add(this.pnlCardInner);
            this.pnlCardOuter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCardOuter.Location = new System.Drawing.Point(16, 440);
            this.pnlCardOuter.Margin = new System.Windows.Forms.Padding(0);
            this.pnlCardOuter.Name = "pnlCardOuter";
            this.pnlCardOuter.Padding = new System.Windows.Forms.Padding(1, 1, 1, 4);
            this.pnlCardOuter.Size = new System.Drawing.Size(1142, 300);
            this.pnlCardOuter.TabIndex = 3;
            // 
            // pnlCardInner
            // 
            this.pnlCardInner.BackColor = System.Drawing.Color.White;
            this.pnlCardInner.Controls.Add(this.grdAlarm);
            this.pnlCardInner.Controls.Add(this.pnlCardHead);
            this.pnlCardInner.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCardInner.Location = new System.Drawing.Point(1, 1);
            this.pnlCardInner.Name = "pnlCardInner";
            this.pnlCardInner.Size = new System.Drawing.Size(1140, 298);
            this.pnlCardInner.TabIndex = 0;
            // 
            // grdAlarm
            // 
            this.grdAlarm.AllowUserToAddRows = false;
            this.grdAlarm.AllowUserToDeleteRows = false;
            this.grdAlarm.AllowUserToResizeRows = false;
            this.grdAlarm.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.grdAlarm.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colTime,
            this.colType,
            this.colStop,
            this.colCode,
            this.colContent,
            this.colRecipe,
            this.colMode,
            this.colUser});
            this.grdAlarm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grdAlarm.Location = new System.Drawing.Point(0, 44);
            this.grdAlarm.MultiSelect = false;
            this.grdAlarm.Name = "grdAlarm";
            this.grdAlarm.ReadOnly = true;
            this.grdAlarm.RowHeadersVisible = false;
            this.grdAlarm.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grdAlarm.Size = new System.Drawing.Size(1140, 254);
            this.grdAlarm.TabIndex = 0;
            // 
            // colTime
            // 
            this.colTime.HeaderText = "时间";
            this.colTime.Name = "colTime";
            this.colTime.ReadOnly = true;
            this.colTime.Width = 160;
            // 
            // colType
            // 
            this.colType.HeaderText = "类型";
            this.colType.Name = "colType";
            this.colType.ReadOnly = true;
            this.colType.Width = 66;
            // 
            // colStop
            // 
            this.colStop.HeaderText = "停机";
            this.colStop.Name = "colStop";
            this.colStop.ReadOnly = true;
            this.colStop.Width = 62;
            // 
            // colCode
            // 
            this.colCode.HeaderText = "编号";
            this.colCode.Name = "colCode";
            this.colCode.ReadOnly = true;
            this.colCode.Width = 76;
            // 
            // colContent
            // 
            this.colContent.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colContent.HeaderText = "内容";
            this.colContent.Name = "colContent";
            this.colContent.ReadOnly = true;
            // 
            // colRecipe
            // 
            this.colRecipe.HeaderText = "配方";
            this.colRecipe.Name = "colRecipe";
            this.colRecipe.ReadOnly = true;
            this.colRecipe.Width = 110;
            // 
            // colMode
            // 
            this.colMode.HeaderText = "模式";
            this.colMode.Name = "colMode";
            this.colMode.ReadOnly = true;
            this.colMode.Width = 84;
            // 
            // colUser
            // 
            this.colUser.HeaderText = "工号";
            this.colUser.Name = "colUser";
            this.colUser.ReadOnly = true;
            this.colUser.Width = 100;
            // 
            // pnlCardHead
            // 
            this.pnlCardHead.BackColor = System.Drawing.Color.White;
            this.pnlCardHead.Controls.Add(this.lblDetail);
            this.pnlCardHead.Controls.Add(this.lblStatus);
            this.pnlCardHead.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlCardHead.Location = new System.Drawing.Point(0, 0);
            this.pnlCardHead.Name = "pnlCardHead";
            this.pnlCardHead.Size = new System.Drawing.Size(1140, 44);
            this.pnlCardHead.TabIndex = 1;
            // 
            // lblDetail
            // 
            this.lblDetail.AutoSize = false;
            this.lblDetail.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblDetail.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblDetail.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.lblDetail.Location = new System.Drawing.Point(0, 0);
            this.lblDetail.Name = "lblDetail";
            this.lblDetail.Padding = new System.Windows.Forms.Padding(18, 0, 0, 0);
            this.lblDetail.Size = new System.Drawing.Size(320, 44);
            this.lblDetail.TabIndex = 0;
            this.lblDetail.Text = "停机明细";
            this.lblDetail.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = false;
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblStatus.Font = new System.Drawing.Font("微软雅黑", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(108)))), ((int)(((byte)(126)))));
            this.lblStatus.Location = new System.Drawing.Point(620, 0);
            this.lblStatus.Name = "lblStatus_dyn";
            this.lblStatus.Padding = new System.Windows.Forms.Padding(0, 0, 18, 0);
            this.lblStatus.Size = new System.Drawing.Size(520, 44);
            this.lblStatus.TabIndex = 1;
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // AlarmQueryForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(243)))), ((int)(((byte)(248)))));
            this.ClientSize = new System.Drawing.Size(1180, 806);
            this.Controls.Add(this.gripResize);
            this.Controls.Add(this.rootTable);
            this.Controls.Add(this.pnlTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MinimumSize = new System.Drawing.Size(1020, 700);
            this.Name = "AlarmQueryForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "报警统计与历史查询";
            this.pnlTitle.ResumeLayout(false);
            this.flowCap.ResumeLayout(false);
            this.rootTable.ResumeLayout(false);
            this.pnlBarOuter.ResumeLayout(false);
            this.pnlBarInner.ResumeLayout(false);
            this.tblCharts.ResumeLayout(false);
            this.pnlCardOuter.ResumeLayout(false);
            this.pnlCardInner.ResumeLayout(false);
            this.pnlCardHead.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picTitleIcon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.grdAlarm)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlTitle;
        private System.Windows.Forms.PictureBox picTitleIcon;
        private AlphaRap.UiLabel lblTitle;
        private System.Windows.Forms.FlowLayoutPanel flowCap;
        private AlphaRap.FlatButton btnMin;
        private AlphaRap.FlatButton btnClose;
        private System.Windows.Forms.Panel pnlTitleLine;
        private System.Windows.Forms.Panel gripResize;
        private System.Windows.Forms.TableLayoutPanel rootTable;
        private AlphaRap.CardPanel pnlBarOuter;
        private System.Windows.Forms.Panel pnlBarInner;
        private AlphaRap.DateField dateFrom;
        private AlphaRap.UiLabel lblRangeDash;
        private AlphaRap.DateField dateTo;
        private AlphaRap.FlatButton btnToday;
        private AlphaRap.FlatButton btnWeek;
        private AlphaRap.FlatButton btnMonth;
        private AlphaRap.FlatButton btnQuery;
        private AlphaRap.FlatButton btnExport;
        private AlphaRap.SegmentedControl segType;
        private AlphaRap.FieldBox boxCode;
        private AlphaRap.FieldBox boxKeyword;
        private AlphaRap.StatStrip statStrip1;
        private System.Windows.Forms.TableLayoutPanel tblCharts;
        private AlphaRap.BarChart chartRank;
        private AlphaRap.BarChart chartTrend;
        private AlphaRap.CardPanel pnlCardOuter;
        private System.Windows.Forms.Panel pnlCardInner;
        private System.Windows.Forms.DataGridView grdAlarm;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn colType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStop;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colContent;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRecipe;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUser;
        private System.Windows.Forms.Panel pnlCardHead;
        private AlphaRap.UiLabel lblDetail;
        private AlphaRap.UiLabel lblStatus;
    }
}
