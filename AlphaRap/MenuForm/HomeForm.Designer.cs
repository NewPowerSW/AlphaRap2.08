namespace AlphaRap
{
    partial class HomeForm
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
            this.components = new System.ComponentModel.Container();
            this.rootLayout = new System.Windows.Forms.TableLayoutPanel();
            this.kpiLayout = new System.Windows.Forms.TableLayoutPanel();
            this.kpiInputCard = new AlphaRap.CardPanel();
            this.lblKpiInputValue = new AlphaRap.UiLabel();
            this.lblKpiInputTitle = new AlphaRap.UiLabel();
            this.kpiOkCard = new AlphaRap.CardPanel();
            this.lblKpiOkValue = new AlphaRap.UiLabel();
            this.lblKpiOkTitle = new AlphaRap.UiLabel();
            this.kpiNgCard = new AlphaRap.CardPanel();
            this.lblKpiNgValue = new AlphaRap.UiLabel();
            this.lblKpiNgTitle = new AlphaRap.UiLabel();
            this.kpiYieldCard = new AlphaRap.CardPanel();
            this.lblKpiYieldValue = new AlphaRap.UiLabel();
            this.lblKpiYieldTitle = new AlphaRap.UiLabel();
            this.kpiCtCard = new AlphaRap.CardPanel();
            this.lblKpiCtValue = new AlphaRap.UiLabel();
            this.lblKpiCtTitle = new AlphaRap.UiLabel();
            this.bodyLayout = new System.Windows.Forms.TableLayoutPanel();
            this.chartCard = new AlphaRap.CardPanel();
            this.hourlyChart = new AlphaRap.HourlyChart();
            this.lblChartTitle = new AlphaRap.UiLabel();
            this.infoCard = new AlphaRap.CardPanel();
            this.infoLayout = new System.Windows.Forms.TableLayoutPanel();
            this.lblInfoTitle = new AlphaRap.UiLabel();
            this.lblBarcode1 = new AlphaRap.UiLabel();
            this.txtBarcode = new System.Windows.Forms.TextBox();
            this.lblBarcode2 = new AlphaRap.UiLabel();
            this.txtBarcode2 = new System.Windows.Forms.TextBox();
            this.lblBarcode3 = new AlphaRap.UiLabel();
            this.txtBarcode3 = new System.Windows.Forms.TextBox();
            this.lblBarcode4 = new AlphaRap.UiLabel();
            this.txtBarcode4 = new System.Windows.Forms.TextBox();
            this.lblHourTitle = new AlphaRap.UiLabel();
            this.hourLayout = new System.Windows.Forms.TableLayoutPanel();
            this.lblHourInput = new AlphaRap.UiLabel();
            this.tHourlyInput = new System.Windows.Forms.TextBox();
            this.lblHourOutput = new AlphaRap.UiLabel();
            this.tHourlyOutput = new System.Windows.Forms.TextBox();
            this.lblHourReject = new AlphaRap.UiLabel();
            this.tHourlyReject = new System.Windows.Forms.TextBox();
            this.lblHourYield = new AlphaRap.UiLabel();
            this.tHourlyYield = new System.Windows.Forms.TextBox();
            this.connLayout = new System.Windows.Forms.FlowLayoutPanel();
            this.label_PLCStaus = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label_ScannStaus = new System.Windows.Forms.Label();
            this.LAB_ScannStaus = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.rootLayout.SuspendLayout();
            this.kpiLayout.SuspendLayout();
            this.kpiInputCard.SuspendLayout();
            this.kpiOkCard.SuspendLayout();
            this.kpiNgCard.SuspendLayout();
            this.kpiYieldCard.SuspendLayout();
            this.kpiCtCard.SuspendLayout();
            this.bodyLayout.SuspendLayout();
            this.chartCard.SuspendLayout();
            this.infoCard.SuspendLayout();
            this.infoLayout.SuspendLayout();
            this.hourLayout.SuspendLayout();
            this.connLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // rootLayout
            // 
            this.rootLayout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(248)))), ((int)(((byte)(249)))));
            this.rootLayout.ColumnCount = 1;
            this.rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootLayout.Controls.Add(this.kpiLayout, 0, 0);
            this.rootLayout.Controls.Add(this.bodyLayout, 0, 1);
            this.rootLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rootLayout.Location = new System.Drawing.Point(0, 0);
            this.rootLayout.Margin = new System.Windows.Forms.Padding(0);
            this.rootLayout.Name = "rootLayout";
            this.rootLayout.Padding = new System.Windows.Forms.Padding(16);
            this.rootLayout.RowCount = 2;
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 112F));
            this.rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootLayout.Size = new System.Drawing.Size(2101, 825);
            this.rootLayout.TabIndex = 0;
            // 
            // kpiLayout
            // 
            this.kpiLayout.ColumnCount = 5;
            this.kpiLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.kpiLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.kpiLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.kpiLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.kpiLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.kpiLayout.Controls.Add(this.kpiInputCard, 0, 0);
            this.kpiLayout.Controls.Add(this.kpiOkCard, 1, 0);
            this.kpiLayout.Controls.Add(this.kpiNgCard, 2, 0);
            this.kpiLayout.Controls.Add(this.kpiYieldCard, 3, 0);
            this.kpiLayout.Controls.Add(this.kpiCtCard, 4, 0);
            this.kpiLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kpiLayout.Location = new System.Drawing.Point(16, 16);
            this.kpiLayout.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.kpiLayout.Name = "kpiLayout";
            this.kpiLayout.RowCount = 1;
            this.kpiLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.kpiLayout.Size = new System.Drawing.Size(2069, 100);
            this.kpiLayout.TabIndex = 0;
            // 
            // kpiInputCard
            // 
            this.kpiInputCard.BackColor = System.Drawing.Color.White;
            this.kpiInputCard.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(231)))), ((int)(((byte)(240)))));
            this.kpiInputCard.Controls.Add(this.lblKpiInputValue);
            this.kpiInputCard.Controls.Add(this.lblKpiInputTitle);
            this.kpiInputCard.CornerRadius = 10;
            this.kpiInputCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kpiInputCard.FillColor = System.Drawing.Color.White;
            this.kpiInputCard.Location = new System.Drawing.Point(0, 0);
            this.kpiInputCard.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.kpiInputCard.Name = "kpiInputCard";
            this.kpiInputCard.Padding = new System.Windows.Forms.Padding(18, 10, 16, 10);
            this.kpiInputCard.ShowBorder = true;
            this.kpiInputCard.Size = new System.Drawing.Size(401, 100);
            this.kpiInputCard.TabIndex = 0;
            // 
            // lblKpiInputValue
            // 
            this.lblKpiInputValue.AutoEllipsis = true;
            this.lblKpiInputValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblKpiInputValue.Font = new System.Drawing.Font("微软雅黑", 26F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblKpiInputValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.lblKpiInputValue.Location = new System.Drawing.Point(18, 34);
            this.lblKpiInputValue.Margin = new System.Windows.Forms.Padding(0);
            this.lblKpiInputValue.Name = "lblKpiInputValue";
            this.lblKpiInputValue.Size = new System.Drawing.Size(367, 56);
            this.lblKpiInputValue.TabIndex = 0;
            this.lblKpiInputValue.Text = "0";
            this.lblKpiInputValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKpiInputTitle
            // 
            this.lblKpiInputTitle.AutoEllipsis = true;
            this.lblKpiInputTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblKpiInputTitle.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblKpiInputTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(138)))), ((int)(((byte)(155)))));
            this.lblKpiInputTitle.Location = new System.Drawing.Point(18, 10);
            this.lblKpiInputTitle.Margin = new System.Windows.Forms.Padding(0);
            this.lblKpiInputTitle.Name = "lblKpiInputTitle";
            this.lblKpiInputTitle.Size = new System.Drawing.Size(367, 24);
            this.lblKpiInputTitle.TabIndex = 1;
            this.lblKpiInputTitle.Text = "投入";
            this.lblKpiInputTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // kpiOkCard
            // 
            this.kpiOkCard.BackColor = System.Drawing.Color.White;
            this.kpiOkCard.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(231)))), ((int)(((byte)(240)))));
            this.kpiOkCard.Controls.Add(this.lblKpiOkValue);
            this.kpiOkCard.Controls.Add(this.lblKpiOkTitle);
            this.kpiOkCard.CornerRadius = 10;
            this.kpiOkCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kpiOkCard.FillColor = System.Drawing.Color.White;
            this.kpiOkCard.Location = new System.Drawing.Point(413, 0);
            this.kpiOkCard.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.kpiOkCard.Name = "kpiOkCard";
            this.kpiOkCard.Padding = new System.Windows.Forms.Padding(18, 10, 16, 10);
            this.kpiOkCard.ShowBorder = true;
            this.kpiOkCard.Size = new System.Drawing.Size(401, 100);
            this.kpiOkCard.TabIndex = 1;
            // 
            // lblKpiOkValue
            // 
            this.lblKpiOkValue.AutoEllipsis = true;
            this.lblKpiOkValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblKpiOkValue.Font = new System.Drawing.Font("微软雅黑", 26F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblKpiOkValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(150)))), ((int)(((byte)(67)))));
            this.lblKpiOkValue.Location = new System.Drawing.Point(18, 34);
            this.lblKpiOkValue.Margin = new System.Windows.Forms.Padding(0);
            this.lblKpiOkValue.Name = "lblKpiOkValue";
            this.lblKpiOkValue.Size = new System.Drawing.Size(367, 56);
            this.lblKpiOkValue.TabIndex = 0;
            this.lblKpiOkValue.Text = "0";
            this.lblKpiOkValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKpiOkTitle
            // 
            this.lblKpiOkTitle.AutoEllipsis = true;
            this.lblKpiOkTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblKpiOkTitle.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblKpiOkTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(138)))), ((int)(((byte)(155)))));
            this.lblKpiOkTitle.Location = new System.Drawing.Point(18, 10);
            this.lblKpiOkTitle.Margin = new System.Windows.Forms.Padding(0);
            this.lblKpiOkTitle.Name = "lblKpiOkTitle";
            this.lblKpiOkTitle.Size = new System.Drawing.Size(367, 24);
            this.lblKpiOkTitle.TabIndex = 1;
            this.lblKpiOkTitle.Text = "良品";
            this.lblKpiOkTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // kpiNgCard
            // 
            this.kpiNgCard.BackColor = System.Drawing.Color.White;
            this.kpiNgCard.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(231)))), ((int)(((byte)(240)))));
            this.kpiNgCard.Controls.Add(this.lblKpiNgValue);
            this.kpiNgCard.Controls.Add(this.lblKpiNgTitle);
            this.kpiNgCard.CornerRadius = 10;
            this.kpiNgCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kpiNgCard.FillColor = System.Drawing.Color.White;
            this.kpiNgCard.Location = new System.Drawing.Point(826, 0);
            this.kpiNgCard.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.kpiNgCard.Name = "kpiNgCard";
            this.kpiNgCard.Padding = new System.Windows.Forms.Padding(18, 10, 16, 10);
            this.kpiNgCard.ShowBorder = true;
            this.kpiNgCard.Size = new System.Drawing.Size(401, 100);
            this.kpiNgCard.TabIndex = 2;
            // 
            // lblKpiNgValue
            // 
            this.lblKpiNgValue.AutoEllipsis = true;
            this.lblKpiNgValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblKpiNgValue.Font = new System.Drawing.Font("微软雅黑", 26F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblKpiNgValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(206)))), ((int)(((byte)(62)))), ((int)(((byte)(62)))));
            this.lblKpiNgValue.Location = new System.Drawing.Point(18, 34);
            this.lblKpiNgValue.Margin = new System.Windows.Forms.Padding(0);
            this.lblKpiNgValue.Name = "lblKpiNgValue";
            this.lblKpiNgValue.Size = new System.Drawing.Size(367, 56);
            this.lblKpiNgValue.TabIndex = 0;
            this.lblKpiNgValue.Text = "0";
            this.lblKpiNgValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKpiNgTitle
            // 
            this.lblKpiNgTitle.AutoEllipsis = true;
            this.lblKpiNgTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblKpiNgTitle.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblKpiNgTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(138)))), ((int)(((byte)(155)))));
            this.lblKpiNgTitle.Location = new System.Drawing.Point(18, 10);
            this.lblKpiNgTitle.Margin = new System.Windows.Forms.Padding(0);
            this.lblKpiNgTitle.Name = "lblKpiNgTitle";
            this.lblKpiNgTitle.Size = new System.Drawing.Size(367, 24);
            this.lblKpiNgTitle.TabIndex = 1;
            this.lblKpiNgTitle.Text = "不良";
            this.lblKpiNgTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // kpiYieldCard
            // 
            this.kpiYieldCard.BackColor = System.Drawing.Color.White;
            this.kpiYieldCard.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(231)))), ((int)(((byte)(240)))));
            this.kpiYieldCard.Controls.Add(this.lblKpiYieldValue);
            this.kpiYieldCard.Controls.Add(this.lblKpiYieldTitle);
            this.kpiYieldCard.CornerRadius = 10;
            this.kpiYieldCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kpiYieldCard.FillColor = System.Drawing.Color.White;
            this.kpiYieldCard.Location = new System.Drawing.Point(1239, 0);
            this.kpiYieldCard.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.kpiYieldCard.Name = "kpiYieldCard";
            this.kpiYieldCard.Padding = new System.Windows.Forms.Padding(18, 10, 16, 10);
            this.kpiYieldCard.ShowBorder = true;
            this.kpiYieldCard.Size = new System.Drawing.Size(401, 100);
            this.kpiYieldCard.TabIndex = 3;
            // 
            // lblKpiYieldValue
            // 
            this.lblKpiYieldValue.AutoEllipsis = true;
            this.lblKpiYieldValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblKpiYieldValue.Font = new System.Drawing.Font("微软雅黑", 26F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblKpiYieldValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(108)))), ((int)(((byte)(182)))));
            this.lblKpiYieldValue.Location = new System.Drawing.Point(18, 34);
            this.lblKpiYieldValue.Margin = new System.Windows.Forms.Padding(0);
            this.lblKpiYieldValue.Name = "lblKpiYieldValue";
            this.lblKpiYieldValue.Size = new System.Drawing.Size(367, 56);
            this.lblKpiYieldValue.TabIndex = 0;
            this.lblKpiYieldValue.Text = "0";
            this.lblKpiYieldValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKpiYieldTitle
            // 
            this.lblKpiYieldTitle.AutoEllipsis = true;
            this.lblKpiYieldTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblKpiYieldTitle.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblKpiYieldTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(138)))), ((int)(((byte)(155)))));
            this.lblKpiYieldTitle.Location = new System.Drawing.Point(18, 10);
            this.lblKpiYieldTitle.Margin = new System.Windows.Forms.Padding(0);
            this.lblKpiYieldTitle.Name = "lblKpiYieldTitle";
            this.lblKpiYieldTitle.Size = new System.Drawing.Size(367, 24);
            this.lblKpiYieldTitle.TabIndex = 1;
            this.lblKpiYieldTitle.Text = "良率";
            this.lblKpiYieldTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // kpiCtCard
            // 
            this.kpiCtCard.BackColor = System.Drawing.Color.White;
            this.kpiCtCard.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(231)))), ((int)(((byte)(240)))));
            this.kpiCtCard.Controls.Add(this.lblKpiCtValue);
            this.kpiCtCard.Controls.Add(this.lblKpiCtTitle);
            this.kpiCtCard.CornerRadius = 10;
            this.kpiCtCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kpiCtCard.FillColor = System.Drawing.Color.White;
            this.kpiCtCard.Location = new System.Drawing.Point(1652, 0);
            this.kpiCtCard.Margin = new System.Windows.Forms.Padding(0);
            this.kpiCtCard.Name = "kpiCtCard";
            this.kpiCtCard.Padding = new System.Windows.Forms.Padding(18, 10, 16, 10);
            this.kpiCtCard.ShowBorder = true;
            this.kpiCtCard.Size = new System.Drawing.Size(417, 100);
            this.kpiCtCard.TabIndex = 4;
            // 
            // lblKpiCtValue
            // 
            this.lblKpiCtValue.AutoEllipsis = true;
            this.lblKpiCtValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblKpiCtValue.Font = new System.Drawing.Font("微软雅黑", 26F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblKpiCtValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.lblKpiCtValue.Location = new System.Drawing.Point(18, 34);
            this.lblKpiCtValue.Margin = new System.Windows.Forms.Padding(0);
            this.lblKpiCtValue.Name = "lblKpiCtValue";
            this.lblKpiCtValue.Size = new System.Drawing.Size(383, 56);
            this.lblKpiCtValue.TabIndex = 0;
            this.lblKpiCtValue.Text = "0";
            this.lblKpiCtValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKpiCtTitle
            // 
            this.lblKpiCtTitle.AutoEllipsis = true;
            this.lblKpiCtTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblKpiCtTitle.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblKpiCtTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(138)))), ((int)(((byte)(155)))));
            this.lblKpiCtTitle.Location = new System.Drawing.Point(18, 10);
            this.lblKpiCtTitle.Margin = new System.Windows.Forms.Padding(0);
            this.lblKpiCtTitle.Name = "lblKpiCtTitle";
            this.lblKpiCtTitle.Size = new System.Drawing.Size(383, 24);
            this.lblKpiCtTitle.TabIndex = 1;
            this.lblKpiCtTitle.Text = "节拍";
            this.lblKpiCtTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // bodyLayout
            // 
            this.bodyLayout.ColumnCount = 2;
            this.bodyLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 64F));
            this.bodyLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 36F));
            this.bodyLayout.Controls.Add(this.chartCard, 0, 0);
            this.bodyLayout.Controls.Add(this.infoCard, 1, 0);
            this.bodyLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bodyLayout.Location = new System.Drawing.Point(16, 128);
            this.bodyLayout.Margin = new System.Windows.Forms.Padding(0);
            this.bodyLayout.Name = "bodyLayout";
            this.bodyLayout.RowCount = 1;
            this.bodyLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.bodyLayout.Size = new System.Drawing.Size(2069, 681);
            this.bodyLayout.TabIndex = 1;
            // 
            // chartCard
            // 
            this.chartCard.BackColor = System.Drawing.Color.White;
            this.chartCard.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(231)))), ((int)(((byte)(240)))));
            this.chartCard.Controls.Add(this.hourlyChart);
            this.chartCard.Controls.Add(this.lblChartTitle);
            this.chartCard.CornerRadius = 10;
            this.chartCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chartCard.FillColor = System.Drawing.Color.White;
            this.chartCard.Location = new System.Drawing.Point(0, 0);
            this.chartCard.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.chartCard.Name = "chartCard";
            this.chartCard.Padding = new System.Windows.Forms.Padding(16, 12, 16, 12);
            this.chartCard.ShowBorder = true;
            this.chartCard.Size = new System.Drawing.Size(1312, 681);
            this.chartCard.TabIndex = 0;
            // 
            // hourlyChart
            // 
            this.hourlyChart.BackColor = System.Drawing.Color.White;
            this.hourlyChart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hourlyChart.EmptyText = "暂无数据";
            this.hourlyChart.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.hourlyChart.Location = new System.Drawing.Point(16, 42);
            this.hourlyChart.Name = "hourlyChart";
            this.hourlyChart.NgColor = System.Drawing.Color.FromArgb(((int)(((byte)(206)))), ((int)(((byte)(62)))), ((int)(((byte)(62)))));
            this.hourlyChart.NgLegend = "不良";
            this.hourlyChart.OkColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(108)))), ((int)(((byte)(182)))));
            this.hourlyChart.OkLegend = "良品";
            this.hourlyChart.Size = new System.Drawing.Size(1280, 627);
            this.hourlyChart.TabIndex = 0;
            // 
            // lblChartTitle
            // 
            this.lblChartTitle.AutoEllipsis = true;
            this.lblChartTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblChartTitle.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblChartTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.lblChartTitle.Location = new System.Drawing.Point(16, 12);
            this.lblChartTitle.Margin = new System.Windows.Forms.Padding(0);
            this.lblChartTitle.Name = "lblChartTitle";
            this.lblChartTitle.Size = new System.Drawing.Size(1280, 30);
            this.lblChartTitle.TabIndex = 1;
            this.lblChartTitle.Text = "今日每小时产量";
            this.lblChartTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // infoCard
            // 
            this.infoCard.BackColor = System.Drawing.Color.White;
            this.infoCard.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(231)))), ((int)(((byte)(240)))));
            this.infoCard.Controls.Add(this.infoLayout);
            this.infoCard.CornerRadius = 10;
            this.infoCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.infoCard.FillColor = System.Drawing.Color.White;
            this.infoCard.Location = new System.Drawing.Point(1324, 0);
            this.infoCard.Margin = new System.Windows.Forms.Padding(0);
            this.infoCard.Name = "infoCard";
            this.infoCard.Padding = new System.Windows.Forms.Padding(16, 12, 16, 12);
            this.infoCard.ShowBorder = true;
            this.infoCard.Size = new System.Drawing.Size(745, 681);
            this.infoCard.TabIndex = 1;
            // 
            // infoLayout
            // 
            this.infoLayout.BackColor = System.Drawing.Color.White;
            this.infoLayout.ColumnCount = 2;
            this.infoLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 72F));
            this.infoLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.infoLayout.Controls.Add(this.lblInfoTitle, 0, 0);
            this.infoLayout.Controls.Add(this.lblBarcode1, 0, 1);
            this.infoLayout.Controls.Add(this.txtBarcode, 1, 1);
            this.infoLayout.Controls.Add(this.lblBarcode2, 0, 2);
            this.infoLayout.Controls.Add(this.txtBarcode2, 1, 2);
            this.infoLayout.Controls.Add(this.lblBarcode3, 0, 3);
            this.infoLayout.Controls.Add(this.txtBarcode3, 1, 3);
            this.infoLayout.Controls.Add(this.lblBarcode4, 0, 4);
            this.infoLayout.Controls.Add(this.txtBarcode4, 1, 4);
            this.infoLayout.Controls.Add(this.lblHourTitle, 0, 6);
            this.infoLayout.Controls.Add(this.hourLayout, 0, 7);
            this.infoLayout.Controls.Add(this.connLayout, 0, 9);
            this.infoLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.infoLayout.Location = new System.Drawing.Point(16, 12);
            this.infoLayout.Margin = new System.Windows.Forms.Padding(0);
            this.infoLayout.Name = "infoLayout";
            this.infoLayout.RowCount = 10;
            this.infoLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.infoLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.infoLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.infoLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.infoLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.infoLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 14F));
            this.infoLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.infoLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56F));
            this.infoLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.infoLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.infoLayout.Size = new System.Drawing.Size(713, 657);
            this.infoLayout.TabIndex = 0;
            // 
            // lblInfoTitle
            // 
            this.lblInfoTitle.AutoEllipsis = true;
            this.infoLayout.SetColumnSpan(this.lblInfoTitle, 2);
            this.lblInfoTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblInfoTitle.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblInfoTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.lblInfoTitle.Location = new System.Drawing.Point(0, 0);
            this.lblInfoTitle.Margin = new System.Windows.Forms.Padding(0);
            this.lblInfoTitle.Name = "lblInfoTitle";
            this.lblInfoTitle.Size = new System.Drawing.Size(713, 32);
            this.lblInfoTitle.TabIndex = 0;
            this.lblInfoTitle.Text = "生产信息";
            this.lblInfoTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblBarcode1
            // 
            this.lblBarcode1.AutoEllipsis = true;
            this.lblBarcode1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblBarcode1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblBarcode1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(138)))), ((int)(((byte)(155)))));
            this.lblBarcode1.Location = new System.Drawing.Point(0, 32);
            this.lblBarcode1.Margin = new System.Windows.Forms.Padding(0);
            this.lblBarcode1.Name = "lblBarcode1";
            this.lblBarcode1.Size = new System.Drawing.Size(72, 38);
            this.lblBarcode1.TabIndex = 1;
            this.lblBarcode1.Text = "条码 1";
            this.lblBarcode1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtBarcode
            // 
            this.txtBarcode.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(252)))), ((int)(((byte)(254)))));
            this.txtBarcode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtBarcode.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtBarcode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.txtBarcode.Location = new System.Drawing.Point(72, 37);
            this.txtBarcode.Margin = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.txtBarcode.Name = "txtBarcode";
            this.txtBarcode.Size = new System.Drawing.Size(641, 44);
            this.txtBarcode.TabIndex = 2;
            // 
            // lblBarcode2
            // 
            this.lblBarcode2.AutoEllipsis = true;
            this.lblBarcode2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblBarcode2.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblBarcode2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(138)))), ((int)(((byte)(155)))));
            this.lblBarcode2.Location = new System.Drawing.Point(0, 70);
            this.lblBarcode2.Margin = new System.Windows.Forms.Padding(0);
            this.lblBarcode2.Name = "lblBarcode2";
            this.lblBarcode2.Size = new System.Drawing.Size(72, 38);
            this.lblBarcode2.TabIndex = 3;
            this.lblBarcode2.Text = "条码 2";
            this.lblBarcode2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtBarcode2
            // 
            this.txtBarcode2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(252)))), ((int)(((byte)(254)))));
            this.txtBarcode2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtBarcode2.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtBarcode2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.txtBarcode2.Location = new System.Drawing.Point(72, 75);
            this.txtBarcode2.Margin = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.txtBarcode2.Name = "txtBarcode2";
            this.txtBarcode2.Size = new System.Drawing.Size(641, 44);
            this.txtBarcode2.TabIndex = 4;
            // 
            // lblBarcode3
            // 
            this.lblBarcode3.AutoEllipsis = true;
            this.lblBarcode3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblBarcode3.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblBarcode3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(138)))), ((int)(((byte)(155)))));
            this.lblBarcode3.Location = new System.Drawing.Point(0, 108);
            this.lblBarcode3.Margin = new System.Windows.Forms.Padding(0);
            this.lblBarcode3.Name = "lblBarcode3";
            this.lblBarcode3.Size = new System.Drawing.Size(72, 38);
            this.lblBarcode3.TabIndex = 5;
            this.lblBarcode3.Text = "条码 3";
            this.lblBarcode3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtBarcode3
            // 
            this.txtBarcode3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(252)))), ((int)(((byte)(254)))));
            this.txtBarcode3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtBarcode3.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtBarcode3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.txtBarcode3.Location = new System.Drawing.Point(72, 113);
            this.txtBarcode3.Margin = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.txtBarcode3.Name = "txtBarcode3";
            this.txtBarcode3.Size = new System.Drawing.Size(641, 44);
            this.txtBarcode3.TabIndex = 6;
            // 
            // lblBarcode4
            // 
            this.lblBarcode4.AutoEllipsis = true;
            this.lblBarcode4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblBarcode4.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblBarcode4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(138)))), ((int)(((byte)(155)))));
            this.lblBarcode4.Location = new System.Drawing.Point(0, 146);
            this.lblBarcode4.Margin = new System.Windows.Forms.Padding(0);
            this.lblBarcode4.Name = "lblBarcode4";
            this.lblBarcode4.Size = new System.Drawing.Size(72, 38);
            this.lblBarcode4.TabIndex = 7;
            this.lblBarcode4.Text = "条码 4";
            this.lblBarcode4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtBarcode4
            // 
            this.txtBarcode4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(252)))), ((int)(((byte)(254)))));
            this.txtBarcode4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtBarcode4.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtBarcode4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.txtBarcode4.Location = new System.Drawing.Point(72, 151);
            this.txtBarcode4.Margin = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.txtBarcode4.Name = "txtBarcode4";
            this.txtBarcode4.Size = new System.Drawing.Size(641, 44);
            this.txtBarcode4.TabIndex = 8;
            // 
            // lblHourTitle
            // 
            this.lblHourTitle.AutoEllipsis = true;
            this.infoLayout.SetColumnSpan(this.lblHourTitle, 2);
            this.lblHourTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHourTitle.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblHourTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.lblHourTitle.Location = new System.Drawing.Point(0, 198);
            this.lblHourTitle.Margin = new System.Windows.Forms.Padding(0);
            this.lblHourTitle.Name = "lblHourTitle";
            this.lblHourTitle.Size = new System.Drawing.Size(713, 28);
            this.lblHourTitle.TabIndex = 9;
            this.lblHourTitle.Text = "本小时";
            this.lblHourTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // hourLayout
            // 
            this.hourLayout.ColumnCount = 4;
            this.infoLayout.SetColumnSpan(this.hourLayout, 2);
            this.hourLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.hourLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.hourLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.hourLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.hourLayout.Controls.Add(this.lblHourInput, 0, 0);
            this.hourLayout.Controls.Add(this.tHourlyInput, 0, 1);
            this.hourLayout.Controls.Add(this.lblHourOutput, 1, 0);
            this.hourLayout.Controls.Add(this.tHourlyOutput, 1, 1);
            this.hourLayout.Controls.Add(this.lblHourReject, 2, 0);
            this.hourLayout.Controls.Add(this.tHourlyReject, 2, 1);
            this.hourLayout.Controls.Add(this.lblHourYield, 3, 0);
            this.hourLayout.Controls.Add(this.tHourlyYield, 3, 1);
            this.hourLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hourLayout.Location = new System.Drawing.Point(0, 226);
            this.hourLayout.Margin = new System.Windows.Forms.Padding(0);
            this.hourLayout.Name = "hourLayout";
            this.hourLayout.RowCount = 2;
            this.hourLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.hourLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.hourLayout.Size = new System.Drawing.Size(713, 56);
            this.hourLayout.TabIndex = 10;
            // 
            // lblHourInput
            // 
            this.lblHourInput.AutoEllipsis = true;
            this.lblHourInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHourInput.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblHourInput.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(138)))), ((int)(((byte)(155)))));
            this.lblHourInput.Location = new System.Drawing.Point(0, 0);
            this.lblHourInput.Margin = new System.Windows.Forms.Padding(0);
            this.lblHourInput.Name = "lblHourInput";
            this.lblHourInput.Size = new System.Drawing.Size(178, 22);
            this.lblHourInput.TabIndex = 0;
            this.lblHourInput.Text = "投入";
            this.lblHourInput.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tHourlyInput
            // 
            this.tHourlyInput.BackColor = System.Drawing.Color.White;
            this.tHourlyInput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tHourlyInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tHourlyInput.Font = new System.Drawing.Font("微软雅黑", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tHourlyInput.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.tHourlyInput.Location = new System.Drawing.Point(0, 24);
            this.tHourlyInput.Margin = new System.Windows.Forms.Padding(0, 2, 8, 0);
            this.tHourlyInput.Name = "tHourlyInput";
            this.tHourlyInput.ReadOnly = true;
            this.tHourlyInput.Size = new System.Drawing.Size(170, 50);
            this.tHourlyInput.TabIndex = 1;
            this.tHourlyInput.TabStop = false;
            this.tHourlyInput.Text = "0";
            // 
            // lblHourOutput
            // 
            this.lblHourOutput.AutoEllipsis = true;
            this.lblHourOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHourOutput.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblHourOutput.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(138)))), ((int)(((byte)(155)))));
            this.lblHourOutput.Location = new System.Drawing.Point(178, 0);
            this.lblHourOutput.Margin = new System.Windows.Forms.Padding(0);
            this.lblHourOutput.Name = "lblHourOutput";
            this.lblHourOutput.Size = new System.Drawing.Size(178, 22);
            this.lblHourOutput.TabIndex = 2;
            this.lblHourOutput.Text = "产出";
            this.lblHourOutput.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tHourlyOutput
            // 
            this.tHourlyOutput.BackColor = System.Drawing.Color.White;
            this.tHourlyOutput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tHourlyOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tHourlyOutput.Font = new System.Drawing.Font("微软雅黑", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tHourlyOutput.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.tHourlyOutput.Location = new System.Drawing.Point(178, 24);
            this.tHourlyOutput.Margin = new System.Windows.Forms.Padding(0, 2, 8, 0);
            this.tHourlyOutput.Name = "tHourlyOutput";
            this.tHourlyOutput.ReadOnly = true;
            this.tHourlyOutput.Size = new System.Drawing.Size(170, 50);
            this.tHourlyOutput.TabIndex = 3;
            this.tHourlyOutput.TabStop = false;
            this.tHourlyOutput.Text = "0";
            // 
            // lblHourReject
            // 
            this.lblHourReject.AutoEllipsis = true;
            this.lblHourReject.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHourReject.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblHourReject.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(138)))), ((int)(((byte)(155)))));
            this.lblHourReject.Location = new System.Drawing.Point(356, 0);
            this.lblHourReject.Margin = new System.Windows.Forms.Padding(0);
            this.lblHourReject.Name = "lblHourReject";
            this.lblHourReject.Size = new System.Drawing.Size(178, 22);
            this.lblHourReject.TabIndex = 4;
            this.lblHourReject.Text = "不良";
            this.lblHourReject.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tHourlyReject
            // 
            this.tHourlyReject.BackColor = System.Drawing.Color.White;
            this.tHourlyReject.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tHourlyReject.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tHourlyReject.Font = new System.Drawing.Font("微软雅黑", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tHourlyReject.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.tHourlyReject.Location = new System.Drawing.Point(356, 24);
            this.tHourlyReject.Margin = new System.Windows.Forms.Padding(0, 2, 8, 0);
            this.tHourlyReject.Name = "tHourlyReject";
            this.tHourlyReject.ReadOnly = true;
            this.tHourlyReject.Size = new System.Drawing.Size(170, 50);
            this.tHourlyReject.TabIndex = 5;
            this.tHourlyReject.TabStop = false;
            this.tHourlyReject.Text = "0";
            // 
            // lblHourYield
            // 
            this.lblHourYield.AutoEllipsis = true;
            this.lblHourYield.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHourYield.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblHourYield.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(138)))), ((int)(((byte)(155)))));
            this.lblHourYield.Location = new System.Drawing.Point(534, 0);
            this.lblHourYield.Margin = new System.Windows.Forms.Padding(0);
            this.lblHourYield.Name = "lblHourYield";
            this.lblHourYield.Size = new System.Drawing.Size(179, 22);
            this.lblHourYield.TabIndex = 6;
            this.lblHourYield.Text = "良率";
            this.lblHourYield.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tHourlyYield
            // 
            this.tHourlyYield.BackColor = System.Drawing.Color.White;
            this.tHourlyYield.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tHourlyYield.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tHourlyYield.Font = new System.Drawing.Font("微软雅黑", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tHourlyYield.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.tHourlyYield.Location = new System.Drawing.Point(534, 24);
            this.tHourlyYield.Margin = new System.Windows.Forms.Padding(0, 2, 8, 0);
            this.tHourlyYield.Name = "tHourlyYield";
            this.tHourlyYield.ReadOnly = true;
            this.tHourlyYield.Size = new System.Drawing.Size(171, 50);
            this.tHourlyYield.TabIndex = 7;
            this.tHourlyYield.TabStop = false;
            this.tHourlyYield.Text = "0";
            // 
            // connLayout
            // 
            this.infoLayout.SetColumnSpan(this.connLayout, 2);
            this.connLayout.Controls.Add(this.label_PLCStaus);
            this.connLayout.Controls.Add(this.label10);
            this.connLayout.Controls.Add(this.label_ScannStaus);
            this.connLayout.Controls.Add(this.LAB_ScannStaus);
            this.connLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.connLayout.Location = new System.Drawing.Point(0, 627);
            this.connLayout.Margin = new System.Windows.Forms.Padding(0);
            this.connLayout.Name = "connLayout";
            this.connLayout.Size = new System.Drawing.Size(713, 30);
            this.connLayout.TabIndex = 11;
            this.connLayout.WrapContents = false;
            // 
            // label_PLCStaus
            // 
            this.label_PLCStaus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(150)))), ((int)(((byte)(67)))));
            this.label_PLCStaus.Location = new System.Drawing.Point(0, 9);
            this.label_PLCStaus.Margin = new System.Windows.Forms.Padding(0, 9, 6, 0);
            this.label_PLCStaus.Name = "label_PLCStaus";
            this.label_PLCStaus.Size = new System.Drawing.Size(12, 12);
            this.label_PLCStaus.TabIndex = 0;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label10.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.label10.Location = new System.Drawing.Point(18, 5);
            this.label10.Margin = new System.Windows.Forms.Padding(0, 5, 22, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(65, 36);
            this.label10.TabIndex = 1;
            this.label10.Text = "PLC";
            // 
            // label_ScannStaus
            // 
            this.label_ScannStaus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(150)))), ((int)(((byte)(67)))));
            this.label_ScannStaus.Location = new System.Drawing.Point(105, 9);
            this.label_ScannStaus.Margin = new System.Windows.Forms.Padding(0, 9, 6, 0);
            this.label_ScannStaus.Name = "label_ScannStaus";
            this.label_ScannStaus.Size = new System.Drawing.Size(12, 12);
            this.label_ScannStaus.TabIndex = 2;
            // 
            // LAB_ScannStaus
            // 
            this.LAB_ScannStaus.AutoSize = true;
            this.LAB_ScannStaus.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LAB_ScannStaus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.LAB_ScannStaus.Location = new System.Drawing.Point(123, 5);
            this.LAB_ScannStaus.Margin = new System.Windows.Forms.Padding(0, 5, 22, 0);
            this.LAB_ScannStaus.Name = "LAB_ScannStaus";
            this.LAB_ScannStaus.Size = new System.Drawing.Size(99, 36);
            this.LAB_ScannStaus.TabIndex = 3;
            this.LAB_ScannStaus.Text = "扫码枪";
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // HomeForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(248)))), ((int)(((byte)(249)))));
            this.ClientSize = new System.Drawing.Size(2101, 825);
            this.ControlBox = false;
            this.Controls.Add(this.rootLayout);
            this.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "HomeForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "HomeForm";
            this.Load += new System.EventHandler(this.HomeForm_Load);
            this.rootLayout.ResumeLayout(false);
            this.kpiLayout.ResumeLayout(false);
            this.kpiInputCard.ResumeLayout(false);
            this.kpiOkCard.ResumeLayout(false);
            this.kpiNgCard.ResumeLayout(false);
            this.kpiYieldCard.ResumeLayout(false);
            this.kpiCtCard.ResumeLayout(false);
            this.bodyLayout.ResumeLayout(false);
            this.chartCard.ResumeLayout(false);
            this.infoCard.ResumeLayout(false);
            this.infoLayout.ResumeLayout(false);
            this.infoLayout.PerformLayout();
            this.hourLayout.ResumeLayout(false);
            this.hourLayout.PerformLayout();
            this.connLayout.ResumeLayout(false);
            this.connLayout.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel rootLayout;
        private System.Windows.Forms.TableLayoutPanel kpiLayout;
        private AlphaRap.CardPanel kpiInputCard;
        private AlphaRap.UiLabel lblKpiInputValue;
        private AlphaRap.UiLabel lblKpiInputTitle;
        private AlphaRap.CardPanel kpiOkCard;
        private AlphaRap.UiLabel lblKpiOkValue;
        private AlphaRap.UiLabel lblKpiOkTitle;
        private AlphaRap.CardPanel kpiNgCard;
        private AlphaRap.UiLabel lblKpiNgValue;
        private AlphaRap.UiLabel lblKpiNgTitle;
        private AlphaRap.CardPanel kpiYieldCard;
        private AlphaRap.UiLabel lblKpiYieldValue;
        private AlphaRap.UiLabel lblKpiYieldTitle;
        private AlphaRap.CardPanel kpiCtCard;
        private AlphaRap.UiLabel lblKpiCtValue;
        private AlphaRap.UiLabel lblKpiCtTitle;
        private System.Windows.Forms.TableLayoutPanel bodyLayout;
        private AlphaRap.CardPanel chartCard;
        private AlphaRap.HourlyChart hourlyChart;
        private AlphaRap.UiLabel lblChartTitle;
        private AlphaRap.CardPanel infoCard;
        private System.Windows.Forms.TableLayoutPanel infoLayout;
        private AlphaRap.UiLabel lblInfoTitle;
        private AlphaRap.UiLabel lblBarcode1;
        public System.Windows.Forms.TextBox txtBarcode;
        private AlphaRap.UiLabel lblBarcode2;
        public System.Windows.Forms.TextBox txtBarcode2;
        private AlphaRap.UiLabel lblBarcode3;
        public System.Windows.Forms.TextBox txtBarcode3;
        private AlphaRap.UiLabel lblBarcode4;
        public System.Windows.Forms.TextBox txtBarcode4;
        private AlphaRap.UiLabel lblHourTitle;
        private System.Windows.Forms.TableLayoutPanel hourLayout;
        private AlphaRap.UiLabel lblHourInput;
        private System.Windows.Forms.TextBox tHourlyInput;
        private AlphaRap.UiLabel lblHourOutput;
        private System.Windows.Forms.TextBox tHourlyOutput;
        private AlphaRap.UiLabel lblHourReject;
        public System.Windows.Forms.TextBox tHourlyReject;
        private AlphaRap.UiLabel lblHourYield;
        private System.Windows.Forms.TextBox tHourlyYield;
        private System.Windows.Forms.FlowLayoutPanel connLayout;
        private System.Windows.Forms.Label label_PLCStaus;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label_ScannStaus;
        private System.Windows.Forms.Label LAB_ScannStaus;
        private System.Windows.Forms.Timer timer1;
    }
}
