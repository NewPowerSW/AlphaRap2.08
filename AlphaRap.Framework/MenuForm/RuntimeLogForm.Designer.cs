namespace AlphaRap
{
    partial class RuntimeLogForm
    {
        private System.ComponentModel.IContainer components;

        /// <summary>释放查询、语言订阅和控件资源。</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ReleasePage();
                if (components != null) components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>建立可在设计器预览的查询、日志列表及原文详情布局。</summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.rootLayout = new System.Windows.Forms.TableLayoutPanel();
            this.titlePanel = new System.Windows.Forms.Panel();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.filterCard = new AlphaRap.CardPanel();
            this.filterLayout = new System.Windows.Forms.TableLayoutPanel();
            this.rangeRow = new System.Windows.Forms.FlowLayoutPanel();
            this.lblFrom = new System.Windows.Forms.Label();
            this.dateFrom = new System.Windows.Forms.DateTimePicker();
            this.lblTo = new System.Windows.Forms.Label();
            this.dateTo = new System.Windows.Forms.DateTimePicker();
            this.btnToday = new AlphaRap.FlatButton();
            this.btnWeek = new AlphaRap.FlatButton();
            this.autoRefresh = new System.Windows.Forms.CheckBox();
            this.searchRow = new System.Windows.Forms.TableLayoutPanel();
            this.lblLevel = new System.Windows.Forms.Label();
            this.levelFilter = new System.Windows.Forms.ComboBox();
            this.lblKeyword = new System.Windows.Forms.Label();
            this.keywordFilter = new System.Windows.Forms.TextBox();
            this.btnQuery = new AlphaRap.FlatButton();
            this.btnExport = new AlphaRap.FlatButton();
            this.lblStatus = new System.Windows.Forms.Label();
            this.detailCard = new AlphaRap.CardPanel();
            this.detailSplit = new System.Windows.Forms.SplitContainer();
            this.gridLogs = new System.Windows.Forms.DataGridView();
            this.colTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLevel = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colContent = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.detailText = new System.Windows.Forms.TextBox();
            this.lblDetail = new System.Windows.Forms.Label();
            this.refreshTimer = new System.Windows.Forms.Timer(this.components);
            this.rootLayout.SuspendLayout();
            this.titlePanel.SuspendLayout();
            this.filterCard.SuspendLayout();
            this.filterLayout.SuspendLayout();
            this.rangeRow.SuspendLayout();
            this.searchRow.SuspendLayout();
            this.detailCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.detailSplit)).BeginInit();
            this.detailSplit.Panel1.SuspendLayout();
            this.detailSplit.Panel2.SuspendLayout();
            this.detailSplit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridLogs)).BeginInit();
            this.SuspendLayout();
            // 
            // rootLayout
            // 
            this.rootLayout.ColumnCount = 1;
            this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootLayout.Controls.Add(this.titlePanel, 0, 0);
            this.rootLayout.Controls.Add(this.filterCard, 0, 1);
            this.rootLayout.Controls.Add(this.lblStatus, 0, 2);
            this.rootLayout.Controls.Add(this.detailCard, 0, 3);
            this.rootLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rootLayout.Location = new System.Drawing.Point(0, 0);
            this.rootLayout.Name = "rootLayout";
            this.rootLayout.Padding = new System.Windows.Forms.Padding(24, 16, 24, 12);
            this.rootLayout.RowCount = 4;
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 112F));
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootLayout.Size = new System.Drawing.Size(1100, 740);
            this.rootLayout.TabIndex = 0;
            // 
            // titlePanel
            // 
            this.titlePanel.Controls.Add(this.lblSubtitle);
            this.titlePanel.Controls.Add(this.lblTitle);
            this.titlePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.titlePanel.Location = new System.Drawing.Point(24, 16);
            this.titlePanel.Margin = new System.Windows.Forms.Padding(0);
            this.titlePanel.Name = "titlePanel";
            this.titlePanel.Size = new System.Drawing.Size(1052, 60);
            this.titlePanel.TabIndex = 0;
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.AutoEllipsis = true;
            this.lblSubtitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSubtitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(117)))), ((int)(((byte)(146)))));
            this.lblSubtitle.Location = new System.Drawing.Point(0, 32);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(1052, 28);
            this.lblSubtitle.TabIndex = 0;
            this.lblSubtitle.Text = "查看设备运行与错误记录，日志内容保留原文";
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(1052, 32);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "运行日志";
            // 
            // filterCard
            // 
            this.filterCard.AccentColor = System.Drawing.Color.Transparent;
            this.filterCard.BackColor = System.Drawing.Color.White;
            this.filterCard.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(231)))), ((int)(((byte)(240)))));
            this.filterCard.Controls.Add(this.filterLayout);
            this.filterCard.CornerRadius = 12;
            this.filterCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.filterCard.FillColor = System.Drawing.Color.White;
            this.filterCard.Location = new System.Drawing.Point(24, 76);
            this.filterCard.Margin = new System.Windows.Forms.Padding(0);
            this.filterCard.Name = "filterCard";
            this.filterCard.Padding = new System.Windows.Forms.Padding(16, 8, 16, 12);
            this.filterCard.ShadowColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(176)))), ((int)(((byte)(196)))), ((int)(((byte)(218)))));
            this.filterCard.ShowBorder = true;
            this.filterCard.ShowShadow = true;
            this.filterCard.Size = new System.Drawing.Size(1052, 112);
            this.filterCard.TabIndex = 1;
            // 
            // filterLayout
            // 
            this.filterLayout.ColumnCount = 1;
            this.filterLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.filterLayout.Controls.Add(this.rangeRow, 0, 0);
            this.filterLayout.Controls.Add(this.searchRow, 0, 1);
            this.filterLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.filterLayout.Location = new System.Drawing.Point(16, 8);
            this.filterLayout.Name = "filterLayout";
            this.filterLayout.RowCount = 2;
            this.filterLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.filterLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.filterLayout.Size = new System.Drawing.Size(1020, 92);
            this.filterLayout.TabIndex = 0;
            // 
            // rangeRow
            // 
            this.rangeRow.Controls.Add(this.lblFrom);
            this.rangeRow.Controls.Add(this.dateFrom);
            this.rangeRow.Controls.Add(this.lblTo);
            this.rangeRow.Controls.Add(this.dateTo);
            this.rangeRow.Controls.Add(this.btnToday);
            this.rangeRow.Controls.Add(this.btnWeek);
            this.rangeRow.Controls.Add(this.autoRefresh);
            this.rangeRow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rangeRow.Location = new System.Drawing.Point(0, 0);
            this.rangeRow.Margin = new System.Windows.Forms.Padding(0);
            this.rangeRow.Name = "rangeRow";
            this.rangeRow.Size = new System.Drawing.Size(1020, 46);
            this.rangeRow.TabIndex = 0;
            this.rangeRow.WrapContents = false;
            // 
            // lblFrom
            // 
            this.lblFrom.Location = new System.Drawing.Point(0, 0);
            this.lblFrom.Margin = new System.Windows.Forms.Padding(0);
            this.lblFrom.Name = "lblFrom";
            this.lblFrom.Size = new System.Drawing.Size(44, 36);
            this.lblFrom.TabIndex = 0;
            this.lblFrom.Text = "日期";
            this.lblFrom.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dateFrom
            // 
            this.dateFrom.CustomFormat = "yyyy-MM-dd";
            this.dateFrom.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateFrom.Location = new System.Drawing.Point(44, 5);
            this.dateFrom.Margin = new System.Windows.Forms.Padding(0, 5, 8, 0);
            this.dateFrom.Name = "dateFrom";
            this.dateFrom.Size = new System.Drawing.Size(146, 24);
            this.dateFrom.TabIndex = 1;
            this.dateFrom.ValueChanged += new System.EventHandler(this.CriteriaChanged);
            // 
            // lblTo
            // 
            this.lblTo.Location = new System.Drawing.Point(198, 0);
            this.lblTo.Margin = new System.Windows.Forms.Padding(0);
            this.lblTo.Name = "lblTo";
            this.lblTo.Size = new System.Drawing.Size(30, 36);
            this.lblTo.TabIndex = 2;
            this.lblTo.Text = "至";
            this.lblTo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // dateTo
            // 
            this.dateTo.CustomFormat = "yyyy-MM-dd";
            this.dateTo.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTo.Location = new System.Drawing.Point(228, 5);
            this.dateTo.Margin = new System.Windows.Forms.Padding(0, 5, 12, 0);
            this.dateTo.Name = "dateTo";
            this.dateTo.Size = new System.Drawing.Size(146, 24);
            this.dateTo.TabIndex = 3;
            this.dateTo.ValueChanged += new System.EventHandler(this.CriteriaChanged);
            // 
            // btnToday
            // 
            this.btnToday.BackColor = System.Drawing.Color.Transparent;
            this.btnToday.CornerRadius = 8;
            this.btnToday.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnToday.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnToday.Icon = null;
            this.btnToday.IconGap = 8;
            this.btnToday.IconSize = 18;
            this.btnToday.Location = new System.Drawing.Point(386, 2);
            this.btnToday.Margin = new System.Windows.Forms.Padding(0, 2, 8, 0);
            this.btnToday.Name = "btnToday";
            this.btnToday.Selectable = false;
            this.btnToday.Size = new System.Drawing.Size(74, 32);
            this.btnToday.TabIndex = 4;
            this.btnToday.TabStop = false;
            this.btnToday.Text = "今天";
            this.btnToday.TextColorOverride = null;
            this.btnToday.Variant = AlphaRap.FlatButtonVariant.Surface;
            this.btnToday.Click += new System.EventHandler(this.TodayClick);
            // 
            // btnWeek
            // 
            this.btnWeek.BackColor = System.Drawing.Color.Transparent;
            this.btnWeek.CornerRadius = 8;
            this.btnWeek.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnWeek.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnWeek.Icon = null;
            this.btnWeek.IconGap = 8;
            this.btnWeek.IconSize = 18;
            this.btnWeek.Location = new System.Drawing.Point(468, 2);
            this.btnWeek.Margin = new System.Windows.Forms.Padding(0, 2, 12, 0);
            this.btnWeek.Name = "btnWeek";
            this.btnWeek.Selectable = false;
            this.btnWeek.Size = new System.Drawing.Size(116, 32);
            this.btnWeek.TabIndex = 5;
            this.btnWeek.TabStop = false;
            this.btnWeek.Text = "近 7 天";
            this.btnWeek.TextColorOverride = null;
            this.btnWeek.Variant = AlphaRap.FlatButtonVariant.Surface;
            this.btnWeek.Click += new System.EventHandler(this.WeekClick);
            // 
            // autoRefresh
            // 
            this.autoRefresh.Checked = true;
            this.autoRefresh.CheckState = System.Windows.Forms.CheckState.Checked;
            this.autoRefresh.Location = new System.Drawing.Point(596, 0);
            this.autoRefresh.Margin = new System.Windows.Forms.Padding(0);
            this.autoRefresh.Name = "autoRefresh";
            this.autoRefresh.Size = new System.Drawing.Size(174, 36);
            this.autoRefresh.TabIndex = 6;
            this.autoRefresh.Text = "自动刷新（3 秒）";
            this.autoRefresh.CheckedChanged += new System.EventHandler(this.AutoRefreshChanged);
            // 
            // searchRow
            // 
            this.searchRow.ColumnCount = 6;
            this.searchRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.searchRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 146F));
            this.searchRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.searchRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.searchRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.searchRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.searchRow.Controls.Add(this.lblLevel, 0, 0);
            this.searchRow.Controls.Add(this.levelFilter, 1, 0);
            this.searchRow.Controls.Add(this.lblKeyword, 2, 0);
            this.searchRow.Controls.Add(this.keywordFilter, 3, 0);
            this.searchRow.Controls.Add(this.btnQuery, 4, 0);
            this.searchRow.Controls.Add(this.btnExport, 5, 0);
            this.searchRow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.searchRow.Location = new System.Drawing.Point(0, 46);
            this.searchRow.Margin = new System.Windows.Forms.Padding(0);
            this.searchRow.Name = "searchRow";
            this.searchRow.RowCount = 1;
            this.searchRow.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.searchRow.Size = new System.Drawing.Size(1020, 46);
            this.searchRow.TabIndex = 1;
            // 
            // lblLevel
            // 
            this.lblLevel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblLevel.Location = new System.Drawing.Point(0, 0);
            this.lblLevel.Margin = new System.Windows.Forms.Padding(0);
            this.lblLevel.Name = "lblLevel";
            this.lblLevel.Size = new System.Drawing.Size(44, 46);
            this.lblLevel.TabIndex = 0;
            this.lblLevel.Text = "类型";
            this.lblLevel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // levelFilter
            // 
            this.levelFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.levelFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.levelFilter.Location = new System.Drawing.Point(44, 8);
            this.levelFilter.Margin = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.levelFilter.Name = "levelFilter";
            this.levelFilter.Size = new System.Drawing.Size(146, 27);
            this.levelFilter.TabIndex = 1;
            this.levelFilter.SelectedIndexChanged += new System.EventHandler(this.CriteriaChanged);
            // 
            // lblKeyword
            // 
            this.lblKeyword.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblKeyword.Location = new System.Drawing.Point(190, 0);
            this.lblKeyword.Margin = new System.Windows.Forms.Padding(0);
            this.lblKeyword.Name = "lblKeyword";
            this.lblKeyword.Size = new System.Drawing.Size(90, 46);
            this.lblKeyword.TabIndex = 2;
            this.lblKeyword.Text = "关键字";
            this.lblKeyword.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // keywordFilter
            // 
            this.keywordFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.keywordFilter.Location = new System.Drawing.Point(280, 8);
            this.keywordFilter.Margin = new System.Windows.Forms.Padding(0, 8, 8, 0);
            this.keywordFilter.Name = "keywordFilter";
            this.keywordFilter.Size = new System.Drawing.Size(532, 24);
            this.keywordFilter.TabIndex = 3;
            this.keywordFilter.TextChanged += new System.EventHandler(this.CriteriaChanged);
            this.keywordFilter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.KeywordKeyDown);
            // 
            // btnQuery
            // 
            this.btnQuery.BackColor = System.Drawing.Color.Transparent;
            this.btnQuery.CornerRadius = 8;
            this.btnQuery.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnQuery.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnQuery.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnQuery.Icon = null;
            this.btnQuery.IconGap = 8;
            this.btnQuery.IconSize = 18;
            this.btnQuery.Location = new System.Drawing.Point(824, 4);
            this.btnQuery.Margin = new System.Windows.Forms.Padding(4, 4, 4, 6);
            this.btnQuery.Name = "btnQuery";
            this.btnQuery.Selectable = false;
            this.btnQuery.Size = new System.Drawing.Size(92, 36);
            this.btnQuery.TabIndex = 4;
            this.btnQuery.TabStop = false;
            this.btnQuery.Text = "查询";
            this.btnQuery.TextColorOverride = null;
            this.btnQuery.Variant = AlphaRap.FlatButtonVariant.Primary;
            this.btnQuery.Click += new System.EventHandler(this.QueryClick);
            // 
            // btnExport
            // 
            this.btnExport.BackColor = System.Drawing.Color.Transparent;
            this.btnExport.CornerRadius = 8;
            this.btnExport.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnExport.Enabled = false;
            this.btnExport.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnExport.Icon = null;
            this.btnExport.IconGap = 8;
            this.btnExport.IconSize = 18;
            this.btnExport.Location = new System.Drawing.Point(924, 4);
            this.btnExport.Margin = new System.Windows.Forms.Padding(4, 4, 0, 6);
            this.btnExport.Name = "btnExport";
            this.btnExport.Selectable = false;
            this.btnExport.Size = new System.Drawing.Size(96, 36);
            this.btnExport.TabIndex = 5;
            this.btnExport.TabStop = false;
            this.btnExport.Text = "导出";
            this.btnExport.TextColorOverride = null;
            this.btnExport.Variant = AlphaRap.FlatButtonVariant.Surface;
            this.btnExport.Click += new System.EventHandler(this.ExportClick);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoEllipsis = true;
            this.lblStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStatus.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(117)))), ((int)(((byte)(146)))));
            this.lblStatus.Location = new System.Drawing.Point(24, 188);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(1052, 34);
            this.lblStatus.TabIndex = 2;
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // detailCard
            // 
            this.detailCard.AccentColor = System.Drawing.Color.Transparent;
            this.detailCard.BackColor = System.Drawing.Color.White;
            this.detailCard.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(231)))), ((int)(((byte)(240)))));
            this.detailCard.Controls.Add(this.detailSplit);
            this.detailCard.CornerRadius = 12;
            this.detailCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.detailCard.FillColor = System.Drawing.Color.White;
            this.detailCard.Location = new System.Drawing.Point(24, 222);
            this.detailCard.Margin = new System.Windows.Forms.Padding(0);
            this.detailCard.Name = "detailCard";
            this.detailCard.Padding = new System.Windows.Forms.Padding(12);
            this.detailCard.ShadowColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(176)))), ((int)(((byte)(196)))), ((int)(((byte)(218)))));
            this.detailCard.ShowBorder = true;
            this.detailCard.ShowShadow = true;
            this.detailCard.Size = new System.Drawing.Size(1052, 506);
            this.detailCard.TabIndex = 3;
            // 
            // detailSplit
            // 
            this.detailSplit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.detailSplit.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.detailSplit.Location = new System.Drawing.Point(12, 12);
            this.detailSplit.Name = "detailSplit";
            this.detailSplit.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // detailSplit.Panel1
            // 
            this.detailSplit.Panel1.Controls.Add(this.gridLogs);
            this.detailSplit.Panel1MinSize = 120;
            // 
            // detailSplit.Panel2
            // 
            this.detailSplit.Panel2.Controls.Add(this.detailText);
            this.detailSplit.Panel2.Controls.Add(this.lblDetail);
            this.detailSplit.Panel2MinSize = 90;
            this.detailSplit.Size = new System.Drawing.Size(1028, 482);
            this.detailSplit.SplitterDistance = 342;
            this.detailSplit.TabIndex = 0;
            // 
            // gridLogs
            // 
            this.gridLogs.AllowUserToAddRows = false;
            this.gridLogs.AllowUserToDeleteRows = false;
            this.gridLogs.AllowUserToResizeRows = false;
            this.gridLogs.BackgroundColor = System.Drawing.Color.White;
            this.gridLogs.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.gridLogs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridLogs.Location = new System.Drawing.Point(0, 0);
            this.gridLogs.MultiSelect = false;
            this.gridLogs.Name = "gridLogs";
            this.gridLogs.ReadOnly = true;
            this.gridLogs.RowHeadersVisible = false;
            this.gridLogs.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridLogs.Size = new System.Drawing.Size(1028, 342);
            this.gridLogs.TabIndex = 0;
            this.gridLogs.AutoGenerateColumns = false;
            this.gridLogs.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { this.colTime, this.colLevel, this.colContent });
            this.colTime.Name = "colTime";
            this.colTime.DataPropertyName = "Time";
            this.colTime.HeaderText = "时间";
            this.colTime.Width = 190;
            this.colTime.DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss.fff";
            this.colTime.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colLevel.Name = "colLevel";
            this.colLevel.DataPropertyName = "Level";
            this.colLevel.HeaderText = "类型";
            this.colLevel.Width = 100;
            this.colLevel.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colContent.Name = "colContent";
            this.colContent.DataPropertyName = "Content";
            this.colContent.HeaderText = "内容";
            this.colContent.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colContent.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.gridLogs.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.FormatCell);
            this.gridLogs.CurrentCellChanged += new System.EventHandler(this.ShowDetail);
            // 
            // detailText
            // 
            this.detailText.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(250)))), ((int)(((byte)(254)))));
            this.detailText.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.detailText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.detailText.Font = new System.Drawing.Font("Consolas", 9F);
            this.detailText.Location = new System.Drawing.Point(0, 26);
            this.detailText.Multiline = true;
            this.detailText.Name = "detailText";
            this.detailText.ReadOnly = true;
            this.detailText.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.detailText.Size = new System.Drawing.Size(1028, 110);
            this.detailText.TabIndex = 0;
            this.detailText.WordWrap = false;
            // 
            // lblDetail
            // 
            this.lblDetail.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDetail.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblDetail.Location = new System.Drawing.Point(0, 0);
            this.lblDetail.Name = "lblDetail";
            this.lblDetail.Size = new System.Drawing.Size(1028, 26);
            this.lblDetail.TabIndex = 1;
            this.lblDetail.Text = "日志详情";
            // 
            // refreshTimer
            // 
            this.refreshTimer.Interval = 3000;
            this.refreshTimer.Tick += new System.EventHandler(this.RefreshTick);
            // 
            // RuntimeLogForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(246)))), ((int)(((byte)(252)))));
            this.ClientSize = new System.Drawing.Size(1100, 740);
            this.Controls.Add(this.rootLayout);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(36)))), ((int)(((byte)(59)))));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "RuntimeLogForm";
            this.Text = "RuntimeLogForm";
            this.rootLayout.ResumeLayout(false);
            this.titlePanel.ResumeLayout(false);
            this.filterCard.ResumeLayout(false);
            this.filterLayout.ResumeLayout(false);
            this.rangeRow.ResumeLayout(false);
            this.searchRow.ResumeLayout(false);
            this.searchRow.PerformLayout();
            this.detailCard.ResumeLayout(false);
            this.detailSplit.Panel1.ResumeLayout(false);
            this.detailSplit.Panel2.ResumeLayout(false);
            this.detailSplit.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.detailSplit)).EndInit();
            this.detailSplit.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridLogs)).EndInit();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.TableLayoutPanel rootLayout, filterLayout, searchRow;
        private System.Windows.Forms.Panel titlePanel;
        private AlphaRap.CardPanel filterCard, detailCard;
        private System.Windows.Forms.FlowLayoutPanel rangeRow;
        private System.Windows.Forms.Label lblTitle, lblSubtitle, lblFrom, lblTo, lblLevel, lblKeyword, lblStatus, lblDetail;
        private System.Windows.Forms.DateTimePicker dateFrom, dateTo;
        private AlphaRap.FlatButton btnToday, btnWeek, btnQuery, btnExport;
        private System.Windows.Forms.CheckBox autoRefresh;
        private System.Windows.Forms.ComboBox levelFilter;
        private System.Windows.Forms.TextBox keywordFilter, detailText;
        private System.Windows.Forms.SplitContainer detailSplit;
        private System.Windows.Forms.DataGridView gridLogs;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTime, colLevel, colContent;
        private System.Windows.Forms.Timer refreshTimer;
    }
}
