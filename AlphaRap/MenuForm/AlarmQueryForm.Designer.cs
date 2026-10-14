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
            this.rootTable = new System.Windows.Forms.TableLayoutPanel();
            this.pnlBarOuter = new System.Windows.Forms.Panel();
            this.pnlBarInner = new System.Windows.Forms.Panel();
            this.flowQuery = new System.Windows.Forms.FlowLayoutPanel();
            this.lblFrom = new AlphaRap.UiLabel();
            this.dtpFrom = new System.Windows.Forms.DateTimePicker();
            this.lblTo = new AlphaRap.UiLabel();
            this.dtpTo = new System.Windows.Forms.DateTimePicker();
            this.lblType = new AlphaRap.UiLabel();
            this.cboType = new System.Windows.Forms.ComboBox();
            this.lblCode = new AlphaRap.UiLabel();
            this.txtCode = new System.Windows.Forms.TextBox();
            this.lblKeyword = new AlphaRap.UiLabel();
            this.txtKeyword = new System.Windows.Forms.TextBox();
            this.flowActions = new System.Windows.Forms.FlowLayoutPanel();
            this.btnQuery = new AlphaRap.FlatButton();
            this.btnExport = new AlphaRap.FlatButton();
            this.statStrip1 = new AlphaRap.StatStrip();
            this.tblCharts = new System.Windows.Forms.TableLayoutPanel();
            this.chartRank = new AlphaRap.BarChart();
            this.chartTrend = new AlphaRap.BarChart();
            this.pnlCardOuter = new System.Windows.Forms.Panel();
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
            this.rootTable.SuspendLayout();
            this.pnlBarOuter.SuspendLayout();
            this.pnlBarInner.SuspendLayout();
            this.flowQuery.SuspendLayout();
            this.flowActions.SuspendLayout();
            this.tblCharts.SuspendLayout();
            this.pnlCardOuter.SuspendLayout();
            this.pnlCardInner.SuspendLayout();
            this.pnlCardHead.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grdAlarm)).BeginInit();
            this.SuspendLayout();
            // 
            // rootTable
            // 
            this.rootTable.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(248)))), ((int)(((byte)(249)))));
            this.rootTable.ColumnCount = 1;
            this.rootTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootTable.Controls.Add(this.pnlBarOuter, 0, 0);
            this.rootTable.Controls.Add(this.statStrip1, 0, 1);
            this.rootTable.Controls.Add(this.tblCharts, 0, 2);
            this.rootTable.Controls.Add(this.pnlCardOuter, 0, 3);
            this.rootTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rootTable.Location = new System.Drawing.Point(0, 0);
            this.rootTable.Name = "rootTable";
            this.rootTable.Padding = new System.Windows.Forms.Padding(16, 14, 16, 14);
            this.rootTable.RowCount = 4;
            this.rootTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 72F));
            this.rootTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 118F));
            this.rootTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 248F));
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
            this.pnlBarOuter.Padding = new System.Windows.Forms.Padding(1);
            this.pnlBarOuter.Size = new System.Drawing.Size(1148, 50);
            this.pnlBarOuter.TabIndex = 0;
            // 
            // pnlBarInner
            // 
            this.pnlBarInner.BackColor = System.Drawing.Color.White;
            this.pnlBarInner.Controls.Add(this.flowQuery);
            this.pnlBarInner.Controls.Add(this.flowActions);
            this.pnlBarInner.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBarInner.Location = new System.Drawing.Point(1, 1);
            this.pnlBarInner.Name = "pnlBarInner";
            this.pnlBarInner.Size = new System.Drawing.Size(1146, 48);
            this.pnlBarInner.TabIndex = 0;
            // 
            // flowQuery
            // 
            this.flowQuery.BackColor = System.Drawing.Color.White;
            this.flowQuery.Controls.Add(this.lblFrom);
            this.flowQuery.Controls.Add(this.dtpFrom);
            this.flowQuery.Controls.Add(this.lblTo);
            this.flowQuery.Controls.Add(this.dtpTo);
            this.flowQuery.Controls.Add(this.lblType);
            this.flowQuery.Controls.Add(this.cboType);
            this.flowQuery.Controls.Add(this.lblCode);
            this.flowQuery.Controls.Add(this.txtCode);
            this.flowQuery.Controls.Add(this.lblKeyword);
            this.flowQuery.Controls.Add(this.txtKeyword);
            this.flowQuery.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowQuery.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.flowQuery.Location = new System.Drawing.Point(0, 0);
            this.flowQuery.Margin = new System.Windows.Forms.Padding(0);
            this.flowQuery.Name = "flowQuery";
            this.flowQuery.Padding = new System.Windows.Forms.Padding(16, 0, 0, 0);
            this.flowQuery.Size = new System.Drawing.Size(930, 48);
            this.flowQuery.TabIndex = 0;
            this.flowQuery.WrapContents = false;
            // 
            // lblFrom
            // 
            this.lblFrom.AutoSize = true;
            this.lblFrom.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblFrom.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(108)))), ((int)(((byte)(126)))));
            this.lblFrom.Location = new System.Drawing.Point(16, 19);
            this.lblFrom.Margin = new System.Windows.Forms.Padding(0, 18, 6, 0);
            this.lblFrom.Name = "lblFrom";
            this.lblFrom.Size = new System.Drawing.Size(36, 19);
            this.lblFrom.TabIndex = 0;
            this.lblFrom.Text = "开始";
            // 
            // dtpFrom
            // 
            this.dtpFrom.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpFrom.Location = new System.Drawing.Point(58, 17);
            this.dtpFrom.Margin = new System.Windows.Forms.Padding(0, 16, 14, 0);
            this.dtpFrom.Name = "dtpFrom";
            this.dtpFrom.Size = new System.Drawing.Size(118, 23);
            this.dtpFrom.TabIndex = 1;
            // 
            // lblTo
            // 
            this.lblTo.AutoSize = true;
            this.lblTo.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblTo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(108)))), ((int)(((byte)(126)))));
            this.lblTo.Location = new System.Drawing.Point(190, 19);
            this.lblTo.Margin = new System.Windows.Forms.Padding(0, 18, 6, 0);
            this.lblTo.Name = "lblTo";
            this.lblTo.Size = new System.Drawing.Size(36, 19);
            this.lblTo.TabIndex = 2;
            this.lblTo.Text = "结束";
            // 
            // dtpTo
            // 
            this.dtpTo.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpTo.Location = new System.Drawing.Point(232, 17);
            this.dtpTo.Margin = new System.Windows.Forms.Padding(0, 16, 20, 0);
            this.dtpTo.Name = "dtpTo";
            this.dtpTo.Size = new System.Drawing.Size(118, 23);
            this.dtpTo.TabIndex = 3;
            // 
            // lblType
            // 
            this.lblType.AutoSize = true;
            this.lblType.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(108)))), ((int)(((byte)(126)))));
            this.lblType.Location = new System.Drawing.Point(370, 19);
            this.lblType.Margin = new System.Windows.Forms.Padding(0, 18, 6, 0);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(36, 19);
            this.lblType.TabIndex = 4;
            this.lblType.Text = "类型";
            // 
            // cboType
            // 
            this.cboType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboType.Location = new System.Drawing.Point(412, 17);
            this.cboType.Margin = new System.Windows.Forms.Padding(0, 16, 20, 0);
            this.cboType.Name = "cboType";
            this.cboType.Size = new System.Drawing.Size(92, 23);
            this.cboType.TabIndex = 5;
            // 
            // lblCode
            // 
            this.lblCode.AutoSize = true;
            this.lblCode.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblCode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(108)))), ((int)(((byte)(126)))));
            this.lblCode.Location = new System.Drawing.Point(524, 19);
            this.lblCode.Margin = new System.Windows.Forms.Padding(0, 18, 6, 0);
            this.lblCode.Name = "lblCode";
            this.lblCode.Size = new System.Drawing.Size(36, 19);
            this.lblCode.TabIndex = 6;
            this.lblCode.Text = "编号";
            // 
            // txtCode
            // 
            this.txtCode.Location = new System.Drawing.Point(566, 17);
            this.txtCode.Margin = new System.Windows.Forms.Padding(0, 16, 20, 0);
            this.txtCode.Name = "txtCode";
            this.txtCode.Size = new System.Drawing.Size(86, 23);
            this.txtCode.TabIndex = 7;
            // 
            // lblKeyword
            // 
            this.lblKeyword.AutoSize = true;
            this.lblKeyword.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblKeyword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(108)))), ((int)(((byte)(126)))));
            this.lblKeyword.Location = new System.Drawing.Point(672, 19);
            this.lblKeyword.Margin = new System.Windows.Forms.Padding(0, 18, 6, 0);
            this.lblKeyword.Name = "lblKeyword";
            this.lblKeyword.Size = new System.Drawing.Size(62, 19);
            this.lblKeyword.TabIndex = 8;
            this.lblKeyword.Text = "内容关键字";
            // 
            // txtKeyword
            // 
            this.txtKeyword.Location = new System.Drawing.Point(740, 17);
            this.txtKeyword.Margin = new System.Windows.Forms.Padding(0, 16, 0, 0);
            this.txtKeyword.Name = "txtKeyword";
            this.txtKeyword.Size = new System.Drawing.Size(170, 23);
            this.txtKeyword.TabIndex = 9;
            // 
            // flowActions
            // 
            this.flowActions.BackColor = System.Drawing.Color.White;
            this.flowActions.Controls.Add(this.btnQuery);
            this.flowActions.Controls.Add(this.btnExport);
            this.flowActions.Dock = System.Windows.Forms.DockStyle.Right;
            this.flowActions.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.flowActions.Location = new System.Drawing.Point(930, 0);
            this.flowActions.Margin = new System.Windows.Forms.Padding(0);
            this.flowActions.Name = "flowActions";
            this.flowActions.Padding = new System.Windows.Forms.Padding(0, 11, 16, 0);
            this.flowActions.Size = new System.Drawing.Size(214, 56);
            this.flowActions.TabIndex = 1;
            this.flowActions.WrapContents = false;
            // 
            // btnQuery
            // 
            this.btnQuery.CornerRadius = 8;
            this.btnQuery.IconSize = 16;
            this.btnQuery.Location = new System.Drawing.Point(0, 11);
            this.btnQuery.Margin = new System.Windows.Forms.Padding(0, 0, 10, 0);
            this.btnQuery.Name = "btnQuery";
            this.btnQuery.Size = new System.Drawing.Size(94, 34);
            this.btnQuery.TabIndex = 0;
            this.btnQuery.Text = "查询";
            this.btnQuery.Variant = AlphaRap.FlatButtonVariant.Primary;
            // 
            // btnExport
            // 
            this.btnExport.CornerRadius = 8;
            this.btnExport.IconSize = 16;
            this.btnExport.Location = new System.Drawing.Point(104, 11);
            this.btnExport.Margin = new System.Windows.Forms.Padding(0);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(94, 34);
            this.btnExport.TabIndex = 1;
            this.btnExport.Text = "导出";
            this.btnExport.Variant = AlphaRap.FlatButtonVariant.Surface;
            // 
            // statStrip1
            // 
            this.statStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(248)))), ((int)(((byte)(249)))));
            this.statStrip1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.statStrip1.Location = new System.Drawing.Point(16, 78);
            this.statStrip1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 14);
            this.statStrip1.Name = "statStrip1";
            this.statStrip1.Size = new System.Drawing.Size(1148, 90);
            this.statStrip1.TabIndex = 1;
            // 
            // tblCharts
            // 
            this.tblCharts.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(248)))), ((int)(((byte)(249)))));
            this.tblCharts.ColumnCount = 2;
            this.tblCharts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 42F));
            this.tblCharts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 58F));
            this.tblCharts.Controls.Add(this.chartRank, 0, 0);
            this.tblCharts.Controls.Add(this.chartTrend, 1, 0);
            this.tblCharts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblCharts.Location = new System.Drawing.Point(16, 182);
            this.tblCharts.Margin = new System.Windows.Forms.Padding(0, 0, 0, 14);
            this.tblCharts.Name = "tblCharts";
            this.tblCharts.RowCount = 1;
            this.tblCharts.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblCharts.Size = new System.Drawing.Size(1148, 218);
            this.tblCharts.TabIndex = 2;
            // 
            // chartRank
            // 
            this.chartRank.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(248)))), ((int)(((byte)(249)))));
            this.chartRank.BarColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(108)))), ((int)(((byte)(182)))));
            this.chartRank.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartRank.Location = new System.Drawing.Point(0, 0);
            this.chartRank.Margin = new System.Windows.Forms.Padding(0, 0, 7, 0);
            this.chartRank.MaxBars = 10;
            this.chartRank.Name = "chartRank";
            this.chartRank.Orientation = AlphaRap.BarChartOrientation.Horizontal;
            this.chartRank.Size = new System.Drawing.Size(475, 218);
            this.chartRank.TabIndex = 0;
            // 
            // chartTrend
            // 
            this.chartTrend.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(248)))), ((int)(((byte)(249)))));
            this.chartTrend.BarColor = System.Drawing.Color.FromArgb(((int)(((byte)(112)))), ((int)(((byte)(164)))), ((int)(((byte)(208)))));
            this.chartTrend.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartTrend.Location = new System.Drawing.Point(489, 0);
            this.chartTrend.Margin = new System.Windows.Forms.Padding(7, 0, 0, 0);
            this.chartTrend.Name = "chartTrend";
            this.chartTrend.Orientation = AlphaRap.BarChartOrientation.Vertical;
            this.chartTrend.Size = new System.Drawing.Size(659, 218);
            this.chartTrend.TabIndex = 1;
            // 
            // pnlCardOuter
            // 
            this.pnlCardOuter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(231)))), ((int)(((byte)(240)))));
            this.pnlCardOuter.Controls.Add(this.pnlCardInner);
            this.pnlCardOuter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlCardOuter.Location = new System.Drawing.Point(16, 414);
            this.pnlCardOuter.Margin = new System.Windows.Forms.Padding(0);
            this.pnlCardOuter.Name = "pnlCardOuter";
            this.pnlCardOuter.Padding = new System.Windows.Forms.Padding(1);
            this.pnlCardOuter.Size = new System.Drawing.Size(1148, 332);
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
            this.pnlCardInner.Size = new System.Drawing.Size(1146, 330);
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
            this.grdAlarm.Size = new System.Drawing.Size(1146, 286);
            this.grdAlarm.TabIndex = 0;
            // 
            // colTime
            // 
            this.colTime.HeaderText = "时间";
            this.colTime.Name = "colTime";
            this.colTime.ReadOnly = true;
            this.colTime.Width = 150;
            // 
            // colType
            // 
            this.colType.HeaderText = "类型";
            this.colType.Name = "colType";
            this.colType.ReadOnly = true;
            this.colType.Width = 56;
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
            this.colRecipe.Width = 130;
            // 
            // colMode
            // 
            this.colMode.HeaderText = "模式";
            this.colMode.Name = "colMode";
            this.colMode.ReadOnly = true;
            this.colMode.Width = 70;
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
            this.pnlCardHead.Size = new System.Drawing.Size(1146, 44);
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
            this.lblDetail.Padding = new System.Windows.Forms.Padding(16, 0, 0, 0);
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
            this.lblStatus.Location = new System.Drawing.Point(626, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Padding = new System.Windows.Forms.Padding(0, 0, 16, 0);
            this.lblStatus.Size = new System.Drawing.Size(520, 44);
            this.lblStatus.TabIndex = 1;
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // AlarmQueryForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(248)))), ((int)(((byte)(249)))));
            this.ClientSize = new System.Drawing.Size(1180, 760);
            this.Controls.Add(this.rootTable);
            this.MinimumSize = new System.Drawing.Size(1000, 640);
            this.Name = "AlarmQueryForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "报警统计与历史查询";
            this.rootTable.ResumeLayout(false);
            this.pnlBarOuter.ResumeLayout(false);
            this.pnlBarInner.ResumeLayout(false);
            this.flowQuery.ResumeLayout(false);
            this.flowQuery.PerformLayout();
            this.flowActions.ResumeLayout(false);
            this.tblCharts.ResumeLayout(false);
            this.pnlCardOuter.ResumeLayout(false);
            this.pnlCardInner.ResumeLayout(false);
            this.pnlCardHead.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grdAlarm)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel rootTable;
        private System.Windows.Forms.Panel pnlBarOuter;
        private System.Windows.Forms.Panel pnlBarInner;
        private System.Windows.Forms.FlowLayoutPanel flowQuery;
        private AlphaRap.UiLabel lblFrom;
        private System.Windows.Forms.DateTimePicker dtpFrom;
        private AlphaRap.UiLabel lblTo;
        private System.Windows.Forms.DateTimePicker dtpTo;
        private AlphaRap.UiLabel lblType;
        private System.Windows.Forms.ComboBox cboType;
        private AlphaRap.UiLabel lblCode;
        private System.Windows.Forms.TextBox txtCode;
        private AlphaRap.UiLabel lblKeyword;
        private System.Windows.Forms.TextBox txtKeyword;
        private System.Windows.Forms.FlowLayoutPanel flowActions;
        private AlphaRap.FlatButton btnQuery;
        private AlphaRap.FlatButton btnExport;
        private AlphaRap.StatStrip statStrip1;
        private System.Windows.Forms.TableLayoutPanel tblCharts;
        private AlphaRap.BarChart chartRank;
        private AlphaRap.BarChart chartTrend;
        private System.Windows.Forms.Panel pnlCardOuter;
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
