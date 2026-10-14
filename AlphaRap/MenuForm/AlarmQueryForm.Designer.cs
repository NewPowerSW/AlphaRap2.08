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
            this.btnQuery = new AlphaRap.FlatButton();
            this.btnExport = new AlphaRap.FlatButton();
            this.statStrip1 = new AlphaRap.StatStrip();
            this.tblCharts = new System.Windows.Forms.TableLayoutPanel();
            this.chartRank = new AlphaRap.BarChart();
            this.chartTrend = new AlphaRap.BarChart();
            this.panelResult = new System.Windows.Forms.Panel();
            this.grdAlarm = new System.Windows.Forms.DataGridView();
            this.colTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colContent = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRecipe = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUser = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblStatus = new AlphaRap.UiLabel();
            this.rootTable.SuspendLayout();
            this.flowQuery.SuspendLayout();
            this.tblCharts.SuspendLayout();
            this.panelResult.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grdAlarm)).BeginInit();
            this.SuspendLayout();
            // 
            // rootTable
            // 
            this.rootTable.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(248)))), ((int)(((byte)(249)))));
            this.rootTable.ColumnCount = 1;
            this.rootTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootTable.Controls.Add(this.flowQuery, 0, 0);
            this.rootTable.Controls.Add(this.statStrip1, 0, 1);
            this.rootTable.Controls.Add(this.tblCharts, 0, 2);
            this.rootTable.Controls.Add(this.panelResult, 0, 3);
            this.rootTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rootTable.Location = new System.Drawing.Point(0, 0);
            this.rootTable.Name = "rootTable";
            this.rootTable.Padding = new System.Windows.Forms.Padding(16, 14, 16, 14);
            this.rootTable.RowCount = 4;
            this.rootTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.rootTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 88F));
            this.rootTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 250F));
            this.rootTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootTable.Size = new System.Drawing.Size(1180, 720);
            this.rootTable.TabIndex = 0;
            // 
            // flowQuery
            // 
            this.flowQuery.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(248)))), ((int)(((byte)(249)))));
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
            this.flowQuery.Controls.Add(this.btnQuery);
            this.flowQuery.Controls.Add(this.btnExport);
            this.flowQuery.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowQuery.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.flowQuery.Location = new System.Drawing.Point(16, 14);
            this.flowQuery.Margin = new System.Windows.Forms.Padding(0);
            this.flowQuery.Name = "flowQuery";
            this.flowQuery.Size = new System.Drawing.Size(1148, 50);
            this.flowQuery.TabIndex = 0;
            this.flowQuery.WrapContents = false;
            // 
            // lblFrom
            // 
            this.lblFrom.AutoSize = true;
            this.lblFrom.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblFrom.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(108)))), ((int)(((byte)(126)))));
            this.lblFrom.Location = new System.Drawing.Point(0, 19);
            this.lblFrom.Margin = new System.Windows.Forms.Padding(0, 19, 6, 0);
            this.lblFrom.Name = "lblFrom";
            this.lblFrom.Size = new System.Drawing.Size(36, 19);
            this.lblFrom.TabIndex = 0;
            this.lblFrom.Text = "开始";
            // 
            // dtpFrom
            // 
            this.dtpFrom.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpFrom.Location = new System.Drawing.Point(42, 15);
            this.dtpFrom.Margin = new System.Windows.Forms.Padding(0, 15, 12, 0);
            this.dtpFrom.Name = "dtpFrom";
            this.dtpFrom.Size = new System.Drawing.Size(120, 23);
            this.dtpFrom.TabIndex = 1;
            // 
            // lblTo
            // 
            this.lblTo.AutoSize = true;
            this.lblTo.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblTo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(108)))), ((int)(((byte)(126)))));
            this.lblTo.Location = new System.Drawing.Point(174, 19);
            this.lblTo.Margin = new System.Windows.Forms.Padding(0, 19, 6, 0);
            this.lblTo.Name = "lblTo";
            this.lblTo.Size = new System.Drawing.Size(36, 19);
            this.lblTo.TabIndex = 2;
            this.lblTo.Text = "结束";
            // 
            // dtpTo
            // 
            this.dtpTo.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpTo.Location = new System.Drawing.Point(216, 15);
            this.dtpTo.Margin = new System.Windows.Forms.Padding(0, 15, 18, 0);
            this.dtpTo.Name = "dtpTo";
            this.dtpTo.Size = new System.Drawing.Size(120, 23);
            this.dtpTo.TabIndex = 3;
            // 
            // lblType
            // 
            this.lblType.AutoSize = true;
            this.lblType.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(108)))), ((int)(((byte)(126)))));
            this.lblType.Location = new System.Drawing.Point(354, 19);
            this.lblType.Margin = new System.Windows.Forms.Padding(0, 19, 6, 0);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(36, 19);
            this.lblType.TabIndex = 4;
            this.lblType.Text = "类型";
            // 
            // cboType
            // 
            this.cboType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboType.Location = new System.Drawing.Point(396, 15);
            this.cboType.Margin = new System.Windows.Forms.Padding(0, 15, 18, 0);
            this.cboType.Name = "cboType";
            this.cboType.Size = new System.Drawing.Size(92, 23);
            this.cboType.TabIndex = 5;
            // 
            // lblCode
            // 
            this.lblCode.AutoSize = true;
            this.lblCode.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblCode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(108)))), ((int)(((byte)(126)))));
            this.lblCode.Location = new System.Drawing.Point(506, 19);
            this.lblCode.Margin = new System.Windows.Forms.Padding(0, 19, 6, 0);
            this.lblCode.Name = "lblCode";
            this.lblCode.Size = new System.Drawing.Size(36, 19);
            this.lblCode.TabIndex = 6;
            this.lblCode.Text = "编号";
            // 
            // txtCode
            // 
            this.txtCode.Location = new System.Drawing.Point(548, 15);
            this.txtCode.Margin = new System.Windows.Forms.Padding(0, 15, 18, 0);
            this.txtCode.Name = "txtCode";
            this.txtCode.Size = new System.Drawing.Size(84, 23);
            this.txtCode.TabIndex = 7;
            // 
            // lblKeyword
            // 
            this.lblKeyword.AutoSize = true;
            this.lblKeyword.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblKeyword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(108)))), ((int)(((byte)(126)))));
            this.lblKeyword.Location = new System.Drawing.Point(650, 19);
            this.lblKeyword.Margin = new System.Windows.Forms.Padding(0, 19, 6, 0);
            this.lblKeyword.Name = "lblKeyword";
            this.lblKeyword.Size = new System.Drawing.Size(62, 19);
            this.lblKeyword.TabIndex = 8;
            this.lblKeyword.Text = "内容关键字";
            // 
            // txtKeyword
            // 
            this.txtKeyword.Location = new System.Drawing.Point(718, 15);
            this.txtKeyword.Margin = new System.Windows.Forms.Padding(0, 15, 18, 0);
            this.txtKeyword.Name = "txtKeyword";
            this.txtKeyword.Size = new System.Drawing.Size(150, 23);
            this.txtKeyword.TabIndex = 9;
            // 
            // btnQuery
            // 
            this.btnQuery.CornerRadius = 8;
            this.btnQuery.IconSize = 16;
            this.btnQuery.Location = new System.Drawing.Point(886, 8);
            this.btnQuery.Margin = new System.Windows.Forms.Padding(0, 8, 10, 0);
            this.btnQuery.Name = "btnQuery";
            this.btnQuery.Size = new System.Drawing.Size(96, 34);
            this.btnQuery.TabIndex = 10;
            this.btnQuery.Text = "查询";
            this.btnQuery.Variant = AlphaRap.FlatButtonVariant.Primary;
            // 
            // btnExport
            // 
            this.btnExport.CornerRadius = 8;
            this.btnExport.IconSize = 16;
            this.btnExport.Location = new System.Drawing.Point(992, 8);
            this.btnExport.Margin = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(96, 34);
            this.btnExport.TabIndex = 11;
            this.btnExport.Text = "导出";
            this.btnExport.Variant = AlphaRap.FlatButtonVariant.Surface;
            // 
            // statStrip1
            // 
            this.statStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(248)))), ((int)(((byte)(249)))));
            this.statStrip1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.statStrip1.Location = new System.Drawing.Point(16, 64);
            this.statStrip1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.statStrip1.Name = "statStrip1";
            this.statStrip1.Size = new System.Drawing.Size(1148, 76);
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
            this.tblCharts.Location = new System.Drawing.Point(16, 152);
            this.tblCharts.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.tblCharts.Name = "tblCharts";
            this.tblCharts.RowCount = 1;
            this.tblCharts.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblCharts.Size = new System.Drawing.Size(1148, 238);
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
            this.chartRank.Size = new System.Drawing.Size(475, 238);
            this.chartRank.TabIndex = 0;
            // 
            // chartTrend
            // 
            this.chartTrend.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(248)))), ((int)(((byte)(249)))));
            this.chartTrend.BarColor = System.Drawing.Color.FromArgb(((int)(((byte)(214)))), ((int)(((byte)(158)))), ((int)(((byte)(32)))));
            this.chartTrend.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartTrend.Location = new System.Drawing.Point(489, 0);
            this.chartTrend.Margin = new System.Windows.Forms.Padding(7, 0, 0, 0);
            this.chartTrend.Name = "chartTrend";
            this.chartTrend.Orientation = AlphaRap.BarChartOrientation.Vertical;
            this.chartTrend.Size = new System.Drawing.Size(659, 238);
            this.chartTrend.TabIndex = 1;
            // 
            // panelResult
            // 
            this.panelResult.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(248)))), ((int)(((byte)(249)))));
            this.panelResult.Controls.Add(this.grdAlarm);
            this.panelResult.Controls.Add(this.lblStatus);
            this.panelResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelResult.Location = new System.Drawing.Point(16, 402);
            this.panelResult.Margin = new System.Windows.Forms.Padding(0);
            this.panelResult.Name = "panelResult";
            this.panelResult.Size = new System.Drawing.Size(1148, 304);
            this.panelResult.TabIndex = 3;
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
            this.colCode,
            this.colContent,
            this.colRecipe,
            this.colMode,
            this.colUser});
            this.grdAlarm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grdAlarm.Location = new System.Drawing.Point(0, 0);
            this.grdAlarm.MultiSelect = false;
            this.grdAlarm.Name = "grdAlarm";
            this.grdAlarm.ReadOnly = true;
            this.grdAlarm.RowHeadersVisible = false;
            this.grdAlarm.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grdAlarm.Size = new System.Drawing.Size(1148, 278);
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
            // lblStatus
            // 
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblStatus.Font = new System.Drawing.Font("微软雅黑", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(108)))), ((int)(((byte)(126)))));
            this.lblStatus.Location = new System.Drawing.Point(0, 278);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(1148, 26);
            this.lblStatus.TabIndex = 1;
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // AlarmQueryForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(248)))), ((int)(((byte)(249)))));
            this.ClientSize = new System.Drawing.Size(1180, 720);
            this.Controls.Add(this.rootTable);
            this.MinimumSize = new System.Drawing.Size(980, 600);
            this.Name = "AlarmQueryForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "报警统计与历史查询";
            this.rootTable.ResumeLayout(false);
            this.flowQuery.ResumeLayout(false);
            this.flowQuery.PerformLayout();
            this.tblCharts.ResumeLayout(false);
            this.panelResult.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grdAlarm)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel rootTable;
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
        private AlphaRap.FlatButton btnQuery;
        private AlphaRap.FlatButton btnExport;
        private AlphaRap.StatStrip statStrip1;
        private System.Windows.Forms.TableLayoutPanel tblCharts;
        private AlphaRap.BarChart chartRank;
        private AlphaRap.BarChart chartTrend;
        private System.Windows.Forms.Panel panelResult;
        private System.Windows.Forms.DataGridView grdAlarm;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn colType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colContent;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRecipe;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUser;
        private AlphaRap.UiLabel lblStatus;
    }
}
