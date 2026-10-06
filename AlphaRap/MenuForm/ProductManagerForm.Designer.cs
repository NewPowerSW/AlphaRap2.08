namespace AlphaRap
{
    partial class ProductManagerForm
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
        /// 物料管理页。
        ///
        /// 重构说明：
        ///   原来是固定 1707x1102 的绝对坐标（三块 FixedSingle 面板 + 蓝色标题条），
        ///   而宿主面板只有约 1360x780 —— 右侧"管理"整块和底部都会被裁掉。
        ///   现在根容器 Dock=Fill，内部用 TableLayoutPanel 分成
        ///   「型号列表(36%) / 右侧上下两张卡(64%)」，整页跟着宿主伸缩。
        ///
        /// 视觉沿用 UiKit：品牌蓝页头图标 + 白色圆角卡片 + FieldBox 输入框 + FlatButton 按钮。
        ///
        /// 注意：listView1 与 CurrentModel 两个控件名不能改，
        /// MainForm（语言切换 / 底栏配方名）与 MiddleLayer 都在直接访问它们。
        /// 另外 this.Text 也不能改：ModuleBaseForm 用它当模块名去定位
        /// ModuleData\SettingData\ProductManagerForm.xml。
        /// </summary>
        private void InitializeComponent()
        {
            this.rootTable = new System.Windows.Forms.TableLayoutPanel();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.pnlCurrent = new AlphaRap.CardPanel();
            this.lblCurrentCaption = new AlphaRap.UiLabel();
            this.CurrentModel = new AlphaRap.UiLabel();
            this.lblPageHint = new AlphaRap.UiLabel();
            this.lblPageTitle = new AlphaRap.UiLabel();
            this.picPageIcon = new System.Windows.Forms.PictureBox();
            this.tblBody = new System.Windows.Forms.TableLayoutPanel();
            this.cardList = new AlphaRap.CardPanel();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lblModelCount = new AlphaRap.UiLabel();
            this.lblModelsCaption = new AlphaRap.UiLabel();
            this.tblRight = new System.Windows.Forms.TableLayoutPanel();
            this.cardSettings = new AlphaRap.CardPanel();
            this.tblFields = new System.Windows.Forms.TableLayoutPanel();
            this.lblProductId = new AlphaRap.UiLabel();
            this.fbProductId = new AlphaRap.FieldBox();
            this.lblStepId = new AlphaRap.UiLabel();
            this.fbStepId = new AlphaRap.FieldBox();
            this.lblRecipeId = new AlphaRap.UiLabel();
            this.fbRecipeId = new AlphaRap.FieldBox();
            this.lblPortId = new AlphaRap.UiLabel();
            this.fbPortId = new AlphaRap.FieldBox();
            this.lblSubCaption = new AlphaRap.UiLabel();
            this.lblSn1 = new AlphaRap.UiLabel();
            this.fbSn1 = new AlphaRap.FieldBox();
            this.chkEnableSn1 = new AlphaRap.UiCheckBox();
            this.lblSn2 = new AlphaRap.UiLabel();
            this.fbSn2 = new AlphaRap.FieldBox();
            this.chkEnableSn2 = new AlphaRap.UiCheckBox();
            this.lblSettingsHint = new AlphaRap.UiLabel();
            this.lblSettingsCaption = new AlphaRap.UiLabel();
            this.cardManager = new AlphaRap.CardPanel();
            this.tblManager = new System.Windows.Forms.TableLayoutPanel();
            this.lblNewName = new AlphaRap.UiLabel();
            this.fbNewName = new AlphaRap.FieldBox();
            this.tblActions = new System.Windows.Forms.TableLayoutPanel();
            this.btnCreate = new AlphaRap.FlatButton();
            this.btnDelete = new AlphaRap.FlatButton();
            this.btnUse = new AlphaRap.FlatButton();
            this.lblManagerTip = new AlphaRap.UiLabel();
            this.lblManagerCaption = new AlphaRap.UiLabel();
            this.dataTable1 = new System.Data.DataTable();
            this.dataColumn1 = new System.Data.DataColumn();
            this.dataColumn2 = new System.Data.DataColumn();
            this.dataColumn3 = new System.Data.DataColumn();
            this.dataColumn4 = new System.Data.DataColumn();
            this.dataColumn5 = new System.Data.DataColumn();
            this.dataColumn7 = new System.Data.DataColumn();
            this.dataColumn6 = new System.Data.DataColumn();
            this.dataColumn8 = new System.Data.DataColumn();
            this.rootTable.SuspendLayout();
            this.pnlHeader.SuspendLayout();
            this.pnlCurrent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPageIcon)).BeginInit();
            this.tblBody.SuspendLayout();
            this.cardList.SuspendLayout();
            this.tblRight.SuspendLayout();
            this.cardSettings.SuspendLayout();
            this.tblFields.SuspendLayout();
            this.cardManager.SuspendLayout();
            this.tblManager.SuspendLayout();
            this.tblActions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SettingData)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.RecipeData)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataTable1)).BeginInit();
            this.SuspendLayout();
            // 
            // RecipeData
            // 
            this.RecipeData.Tables.AddRange(new System.Data.DataTable[] {
            this.dataTable1});
            // 
            // rootTable
            // 
            this.rootTable.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(248)))), ((int)(((byte)(249)))));
            this.rootTable.ColumnCount = 1;
            this.rootTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootTable.Controls.Add(this.pnlHeader, 0, 0);
            this.rootTable.Controls.Add(this.tblBody, 0, 1);
            this.rootTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rootTable.Location = new System.Drawing.Point(0, 0);
            this.rootTable.Margin = new System.Windows.Forms.Padding(0);
            this.rootTable.Name = "rootTable";
            this.rootTable.Padding = new System.Windows.Forms.Padding(20, 16, 20, 18);
            this.rootTable.RowCount = 2;
            this.rootTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 52F));
            this.rootTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootTable.Size = new System.Drawing.Size(1360, 780);
            this.rootTable.TabIndex = 0;
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.Transparent;
            this.pnlHeader.Controls.Add(this.pnlCurrent);
            this.pnlHeader.Controls.Add(this.lblPageHint);
            this.pnlHeader.Controls.Add(this.lblPageTitle);
            this.pnlHeader.Controls.Add(this.picPageIcon);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlHeader.Location = new System.Drawing.Point(20, 16);
            this.pnlHeader.Margin = new System.Windows.Forms.Padding(0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(1320, 52);
            this.pnlHeader.TabIndex = 0;
            // 
            // pnlCurrent
            // 
            this.pnlCurrent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlCurrent.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(216)))), ((int)(((byte)(230)))));
            this.pnlCurrent.Controls.Add(this.CurrentModel);
            this.pnlCurrent.Controls.Add(this.lblCurrentCaption);
            this.pnlCurrent.CornerRadius = 8;
            this.pnlCurrent.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(241)))), ((int)(((byte)(251)))));
            this.pnlCurrent.Location = new System.Drawing.Point(1000, 6);
            this.pnlCurrent.Margin = new System.Windows.Forms.Padding(0);
            this.pnlCurrent.Name = "pnlCurrent";
            this.pnlCurrent.ShowBorder = true;
            this.pnlCurrent.Size = new System.Drawing.Size(320, 40);
            this.pnlCurrent.TabIndex = 3;
            // 
            // lblCurrentCaption
            // 
            this.lblCurrentCaption.AutoSize = true;
            this.lblCurrentCaption.BackColor = System.Drawing.Color.Transparent;
            this.lblCurrentCaption.Font = new System.Drawing.Font("微软雅黑", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblCurrentCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(138)))), ((int)(((byte)(155)))));
            this.lblCurrentCaption.Location = new System.Drawing.Point(13, 11);
            this.lblCurrentCaption.Margin = new System.Windows.Forms.Padding(0);
            this.lblCurrentCaption.Name = "lblCurrentCaption";
            this.lblCurrentCaption.Size = new System.Drawing.Size(0, 19);
            this.lblCurrentCaption.TabIndex = 0;
            this.lblCurrentCaption.Text = "当前型号";
            // 
            // CurrentModel
            // 
            this.CurrentModel.AutoEllipsis = true;
            this.CurrentModel.AutoSize = false;
            this.CurrentModel.BackColor = System.Drawing.Color.Transparent;
            this.CurrentModel.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.CurrentModel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(2)))), ((int)(((byte)(78)))), ((int)(((byte)(133)))));
            this.CurrentModel.Location = new System.Drawing.Point(98, 8);
            this.CurrentModel.Margin = new System.Windows.Forms.Padding(0);
            this.CurrentModel.Name = "CurrentModel";
            this.CurrentModel.Size = new System.Drawing.Size(206, 24);
            this.CurrentModel.TabIndex = 1;
            this.CurrentModel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblPageHint
            // 
            this.lblPageHint.AutoSize = true;
            this.lblPageHint.BackColor = System.Drawing.Color.Transparent;
            this.lblPageHint.Font = new System.Drawing.Font("微软雅黑", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblPageHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(138)))), ((int)(((byte)(155)))));
            this.lblPageHint.Location = new System.Drawing.Point(47, 31);
            this.lblPageHint.Margin = new System.Windows.Forms.Padding(0);
            this.lblPageHint.Name = "lblPageHint";
            this.lblPageHint.Size = new System.Drawing.Size(0, 17);
            this.lblPageHint.TabIndex = 2;
            this.lblPageHint.Text = "管理产品型号与配方参数";
            // 
            // lblPageTitle
            // 
            this.lblPageTitle.AutoSize = true;
            this.lblPageTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblPageTitle.Font = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblPageTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.lblPageTitle.Location = new System.Drawing.Point(44, 0);
            this.lblPageTitle.Margin = new System.Windows.Forms.Padding(0);
            this.lblPageTitle.Name = "lblPageTitle";
            this.lblPageTitle.Size = new System.Drawing.Size(0, 27);
            this.lblPageTitle.TabIndex = 1;
            this.lblPageTitle.Text = "物料管理";
            // 
            // picPageIcon
            // 
            this.picPageIcon.BackColor = System.Drawing.Color.Transparent;
            this.picPageIcon.Location = new System.Drawing.Point(0, 6);
            this.picPageIcon.Margin = new System.Windows.Forms.Padding(0);
            this.picPageIcon.Name = "picPageIcon";
            this.picPageIcon.Size = new System.Drawing.Size(34, 34);
            this.picPageIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picPageIcon.TabIndex = 0;
            this.picPageIcon.TabStop = false;
            // 
            // tblBody
            // 
            this.tblBody.BackColor = System.Drawing.Color.Transparent;
            this.tblBody.ColumnCount = 2;
            this.tblBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 36F));
            this.tblBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 64F));
            this.tblBody.Controls.Add(this.cardList, 0, 0);
            this.tblBody.Controls.Add(this.tblRight, 1, 0);
            this.tblBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblBody.Location = new System.Drawing.Point(20, 68);
            this.tblBody.Margin = new System.Windows.Forms.Padding(0);
            this.tblBody.Name = "tblBody";
            this.tblBody.RowCount = 1;
            this.tblBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblBody.Size = new System.Drawing.Size(1320, 694);
            this.tblBody.TabIndex = 1;
            // 
            // cardList
            // 
            this.cardList.Controls.Add(this.listView1);
            this.cardList.Controls.Add(this.lblModelCount);
            this.cardList.Controls.Add(this.lblModelsCaption);
            this.cardList.CornerRadius = 10;
            this.cardList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardList.Location = new System.Drawing.Point(0, 0);
            this.cardList.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.cardList.Name = "cardList";
            this.cardList.Padding = new System.Windows.Forms.Padding(14, 10, 14, 14);
            this.cardList.Size = new System.Drawing.Size(463, 694);
            this.cardList.TabIndex = 0;
            // 
            // listView1
            // 
            this.listView1.BackColor = System.Drawing.Color.White;
            this.listView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.listView1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.listView1.FullRowSelect = true;
            this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(14, 36);
            this.listView1.Margin = new System.Windows.Forms.Padding(0);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(435, 644);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "所有产品型号";
            this.columnHeader1.Width = 400;
            // 
            // lblModelCount
            // 
            this.lblModelCount.AutoSize = false;
            this.lblModelCount.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblModelCount.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblModelCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(138)))), ((int)(((byte)(155)))));
            this.lblModelCount.Location = new System.Drawing.Point(14, 656);
            this.lblModelCount.Margin = new System.Windows.Forms.Padding(0);
            this.lblModelCount.Name = "lblModelCount";
            this.lblModelCount.Size = new System.Drawing.Size(435, 24);
            this.lblModelCount.TabIndex = 2;
            this.lblModelCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblModelsCaption
            // 
            this.lblModelsCaption.AutoSize = false;
            this.lblModelsCaption.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblModelsCaption.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblModelsCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.lblModelsCaption.Location = new System.Drawing.Point(14, 10);
            this.lblModelsCaption.Margin = new System.Windows.Forms.Padding(0);
            this.lblModelsCaption.Name = "lblModelsCaption";
            this.lblModelsCaption.Size = new System.Drawing.Size(435, 26);
            this.lblModelsCaption.TabIndex = 1;
            this.lblModelsCaption.Text = "产品型号列表";
            this.lblModelsCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tblRight
            // 
            this.tblRight.BackColor = System.Drawing.Color.Transparent;
            this.tblRight.ColumnCount = 1;
            this.tblRight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblRight.Controls.Add(this.cardSettings, 0, 0);
            this.tblRight.Controls.Add(this.cardManager, 0, 1);
            this.tblRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblRight.Location = new System.Drawing.Point(475, 0);
            this.tblRight.Margin = new System.Windows.Forms.Padding(0);
            this.tblRight.Name = "tblRight";
            this.tblRight.RowCount = 2;
            this.tblRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblRight.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 236F));
            this.tblRight.Size = new System.Drawing.Size(845, 694);
            this.tblRight.TabIndex = 1;
            // 
            // cardSettings
            // 
            this.cardSettings.Controls.Add(this.tblFields);
            this.cardSettings.Controls.Add(this.lblSettingsHint);
            this.cardSettings.Controls.Add(this.lblSettingsCaption);
            this.cardSettings.CornerRadius = 10;
            this.cardSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardSettings.Location = new System.Drawing.Point(0, 0);
            this.cardSettings.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.cardSettings.Name = "cardSettings";
            this.cardSettings.Padding = new System.Windows.Forms.Padding(14, 10, 14, 14);
            this.cardSettings.Size = new System.Drawing.Size(845, 446);
            this.cardSettings.TabIndex = 0;
            // 
            // tblFields
            // 
            this.tblFields.BackColor = System.Drawing.Color.Transparent;
            this.tblFields.ColumnCount = 3;
            this.tblFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 190F));
            this.tblFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblFields.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 112F));
            this.tblFields.Controls.Add(this.lblProductId, 0, 0);
            this.tblFields.Controls.Add(this.fbProductId, 1, 0);
            this.tblFields.Controls.Add(this.lblStepId, 0, 1);
            this.tblFields.Controls.Add(this.fbStepId, 1, 1);
            this.tblFields.Controls.Add(this.lblRecipeId, 0, 2);
            this.tblFields.Controls.Add(this.fbRecipeId, 1, 2);
            this.tblFields.Controls.Add(this.lblPortId, 0, 3);
            this.tblFields.Controls.Add(this.fbPortId, 1, 3);
            this.tblFields.Controls.Add(this.lblSubCaption, 0, 4);
            this.tblFields.Controls.Add(this.lblSn1, 0, 5);
            this.tblFields.Controls.Add(this.fbSn1, 1, 5);
            this.tblFields.Controls.Add(this.chkEnableSn1, 2, 5);
            this.tblFields.Controls.Add(this.lblSn2, 0, 6);
            this.tblFields.Controls.Add(this.fbSn2, 1, 6);
            this.tblFields.Controls.Add(this.chkEnableSn2, 2, 6);
            this.tblFields.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblFields.Location = new System.Drawing.Point(14, 56);
            this.tblFields.Margin = new System.Windows.Forms.Padding(0);
            this.tblFields.Name = "tblFields";
            this.tblFields.RowCount = 8;
            this.tblFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tblFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tblFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tblFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tblFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tblFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tblFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tblFields.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblFields.Size = new System.Drawing.Size(817, 376);
            this.tblFields.TabIndex = 0;
            this.tblFields.SetColumnSpan(this.fbProductId, 2);
            this.tblFields.SetColumnSpan(this.fbStepId, 2);
            this.tblFields.SetColumnSpan(this.fbRecipeId, 2);
            this.tblFields.SetColumnSpan(this.fbPortId, 2);
            this.tblFields.SetColumnSpan(this.lblSubCaption, 3);
            // 
            // lblProductId
            // 
            this.lblProductId.AutoSize = false;
            this.lblProductId.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblProductId.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblProductId.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(138)))), ((int)(((byte)(155)))));
            this.lblProductId.Location = new System.Drawing.Point(0, 0);
            this.lblProductId.Margin = new System.Windows.Forms.Padding(0, 0, 14, 0);
            this.lblProductId.Name = "lblProductId";
            this.lblProductId.Size = new System.Drawing.Size(176, 50);
            this.lblProductId.TabIndex = 0;
            this.lblProductId.Text = "PRODUCTID";
            this.lblProductId.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // fbProductId
            // 
            this.fbProductId.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(252)))), ((int)(((byte)(254)))));
            this.fbProductId.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fbProductId.Location = new System.Drawing.Point(190, 4);
            this.fbProductId.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.fbProductId.Name = "fbProductId";
            this.fbProductId.Size = new System.Drawing.Size(627, 42);
            this.fbProductId.TabIndex = 1;
            // 
            // lblStepId
            // 
            this.lblStepId.AutoSize = false;
            this.lblStepId.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblStepId.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblStepId.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(138)))), ((int)(((byte)(155)))));
            this.lblStepId.Location = new System.Drawing.Point(0, 50);
            this.lblStepId.Margin = new System.Windows.Forms.Padding(0, 0, 14, 0);
            this.lblStepId.Name = "lblStepId";
            this.lblStepId.Size = new System.Drawing.Size(176, 50);
            this.lblStepId.TabIndex = 2;
            this.lblStepId.Text = "STEPID";
            this.lblStepId.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // fbStepId
            // 
            this.fbStepId.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(252)))), ((int)(((byte)(254)))));
            this.fbStepId.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fbStepId.Location = new System.Drawing.Point(190, 54);
            this.fbStepId.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.fbStepId.Name = "fbStepId";
            this.fbStepId.Size = new System.Drawing.Size(627, 42);
            this.fbStepId.TabIndex = 3;
            // 
            // lblRecipeId
            // 
            this.lblRecipeId.AutoSize = false;
            this.lblRecipeId.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRecipeId.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblRecipeId.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(138)))), ((int)(((byte)(155)))));
            this.lblRecipeId.Location = new System.Drawing.Point(0, 100);
            this.lblRecipeId.Margin = new System.Windows.Forms.Padding(0, 0, 14, 0);
            this.lblRecipeId.Name = "lblRecipeId";
            this.lblRecipeId.Size = new System.Drawing.Size(176, 50);
            this.lblRecipeId.TabIndex = 4;
            this.lblRecipeId.Text = "RECIPEID";
            this.lblRecipeId.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // fbRecipeId
            // 
            this.fbRecipeId.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(252)))), ((int)(((byte)(254)))));
            this.fbRecipeId.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fbRecipeId.Location = new System.Drawing.Point(190, 104);
            this.fbRecipeId.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.fbRecipeId.Name = "fbRecipeId";
            this.fbRecipeId.Size = new System.Drawing.Size(627, 42);
            this.fbRecipeId.TabIndex = 5;
            // 
            // lblPortId
            // 
            this.lblPortId.AutoSize = false;
            this.lblPortId.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPortId.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblPortId.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(138)))), ((int)(((byte)(155)))));
            this.lblPortId.Location = new System.Drawing.Point(0, 150);
            this.lblPortId.Margin = new System.Windows.Forms.Padding(0, 0, 14, 0);
            this.lblPortId.Name = "lblPortId";
            this.lblPortId.Size = new System.Drawing.Size(176, 50);
            this.lblPortId.TabIndex = 6;
            this.lblPortId.Text = "PORTID";
            this.lblPortId.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // fbPortId
            // 
            this.fbPortId.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(252)))), ((int)(((byte)(254)))));
            this.fbPortId.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fbPortId.Location = new System.Drawing.Point(190, 154);
            this.fbPortId.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.fbPortId.Name = "fbPortId";
            this.fbPortId.Size = new System.Drawing.Size(627, 42);
            this.fbPortId.TabIndex = 7;
            // 
            // lblSubCaption
            // 
            this.lblSubCaption.AutoSize = false;
            this.lblSubCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSubCaption.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblSubCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.lblSubCaption.Location = new System.Drawing.Point(0, 200);
            this.lblSubCaption.Margin = new System.Windows.Forms.Padding(0, 10, 0, 4);
            this.lblSubCaption.Name = "lblSubCaption";
            this.lblSubCaption.Size = new System.Drawing.Size(817, 20);
            this.lblSubCaption.TabIndex = 8;
            this.lblSubCaption.Text = "条码键值子串（KeySub）";
            this.lblSubCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblSn1
            // 
            this.lblSn1.AutoSize = false;
            this.lblSn1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSn1.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblSn1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(138)))), ((int)(((byte)(155)))));
            this.lblSn1.Location = new System.Drawing.Point(0, 234);
            this.lblSn1.Margin = new System.Windows.Forms.Padding(0, 0, 14, 0);
            this.lblSn1.Name = "lblSn1";
            this.lblSn1.Size = new System.Drawing.Size(176, 50);
            this.lblSn1.TabIndex = 9;
            this.lblSn1.Text = "外壳条码 KeySub";
            this.lblSn1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // fbSn1
            // 
            this.fbSn1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(252)))), ((int)(((byte)(254)))));
            this.fbSn1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fbSn1.Location = new System.Drawing.Point(190, 238);
            this.fbSn1.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.fbSn1.Name = "fbSn1";
            this.fbSn1.Size = new System.Drawing.Size(515, 42);
            this.fbSn1.TabIndex = 10;
            // 
            // chkEnableSn1
            // 
            this.chkEnableSn1.AutoSize = false;
            this.chkEnableSn1.BackColor = System.Drawing.Color.Transparent;
            this.chkEnableSn1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkEnableSn1.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.RecipeData, "ProductSetting.EnableSN1KeySub", true));
            this.chkEnableSn1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkEnableSn1.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.chkEnableSn1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.chkEnableSn1.Location = new System.Drawing.Point(715, 247);
            this.chkEnableSn1.Margin = new System.Windows.Forms.Padding(10, 13, 0, 13);
            this.chkEnableSn1.Name = "chkEnableSn1";
            this.chkEnableSn1.Size = new System.Drawing.Size(102, 24);
            this.chkEnableSn1.TabIndex = 11;
            this.chkEnableSn1.Text = "启用";
            this.chkEnableSn1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkEnableSn1.UseVisualStyleBackColor = false;
            this.chkEnableSn1.CheckedChanged += new System.EventHandler(this.textBox4_TextChanged);
            // 
            // lblSn2
            // 
            this.lblSn2.AutoSize = false;
            this.lblSn2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSn2.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblSn2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(138)))), ((int)(((byte)(155)))));
            this.lblSn2.Location = new System.Drawing.Point(0, 284);
            this.lblSn2.Margin = new System.Windows.Forms.Padding(0, 0, 14, 0);
            this.lblSn2.Name = "lblSn2";
            this.lblSn2.Size = new System.Drawing.Size(176, 50);
            this.lblSn2.TabIndex = 12;
            this.lblSn2.Text = "PCB 条码 KeySub";
            this.lblSn2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // fbSn2
            // 
            this.fbSn2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(252)))), ((int)(((byte)(254)))));
            this.fbSn2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fbSn2.Location = new System.Drawing.Point(190, 288);
            this.fbSn2.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.fbSn2.Name = "fbSn2";
            this.fbSn2.Size = new System.Drawing.Size(515, 42);
            this.fbSn2.TabIndex = 13;
            // 
            // chkEnableSn2
            // 
            this.chkEnableSn2.AutoSize = false;
            this.chkEnableSn2.BackColor = System.Drawing.Color.Transparent;
            this.chkEnableSn2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkEnableSn2.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.RecipeData, "ProductSetting.EnableSN2KeySub", true));
            this.chkEnableSn2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkEnableSn2.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.chkEnableSn2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.chkEnableSn2.Location = new System.Drawing.Point(715, 297);
            this.chkEnableSn2.Margin = new System.Windows.Forms.Padding(10, 13, 0, 13);
            this.chkEnableSn2.Name = "chkEnableSn2";
            this.chkEnableSn2.Size = new System.Drawing.Size(102, 24);
            this.chkEnableSn2.TabIndex = 14;
            this.chkEnableSn2.Text = "启用";
            this.chkEnableSn2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkEnableSn2.UseVisualStyleBackColor = false;
            this.chkEnableSn2.CheckedChanged += new System.EventHandler(this.textBox4_TextChanged);
            // 
            // lblSettingsHint
            // 
            this.lblSettingsHint.AutoSize = false;
            this.lblSettingsHint.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSettingsHint.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblSettingsHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(138)))), ((int)(((byte)(155)))));
            this.lblSettingsHint.Location = new System.Drawing.Point(14, 36);
            this.lblSettingsHint.Margin = new System.Windows.Forms.Padding(0);
            this.lblSettingsHint.Name = "lblSettingsHint";
            this.lblSettingsHint.Size = new System.Drawing.Size(817, 20);
            this.lblSettingsHint.TabIndex = 1;
            this.lblSettingsHint.Text = "参数随选中的型号自动载入，改动会标记为待保存";
            this.lblSettingsHint.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblSettingsCaption
            // 
            this.lblSettingsCaption.AutoSize = false;
            this.lblSettingsCaption.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSettingsCaption.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblSettingsCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.lblSettingsCaption.Location = new System.Drawing.Point(14, 10);
            this.lblSettingsCaption.Margin = new System.Windows.Forms.Padding(0);
            this.lblSettingsCaption.Name = "lblSettingsCaption";
            this.lblSettingsCaption.Size = new System.Drawing.Size(817, 26);
            this.lblSettingsCaption.TabIndex = 0;
            this.lblSettingsCaption.Text = "型号参数";
            this.lblSettingsCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cardManager
            // 
            this.cardManager.Controls.Add(this.tblManager);
            this.cardManager.Controls.Add(this.lblManagerCaption);
            this.cardManager.CornerRadius = 10;
            this.cardManager.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardManager.Location = new System.Drawing.Point(0, 458);
            this.cardManager.Margin = new System.Windows.Forms.Padding(0);
            this.cardManager.Name = "cardManager";
            this.cardManager.Padding = new System.Windows.Forms.Padding(14, 10, 14, 14);
            this.cardManager.Size = new System.Drawing.Size(845, 236);
            this.cardManager.TabIndex = 1;
            // 
            // tblManager
            // 
            this.tblManager.BackColor = System.Drawing.Color.Transparent;
            this.tblManager.ColumnCount = 1;
            this.tblManager.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblManager.Controls.Add(this.lblNewName, 0, 0);
            this.tblManager.Controls.Add(this.fbNewName, 0, 1);
            this.tblManager.Controls.Add(this.tblActions, 0, 3);
            this.tblManager.Controls.Add(this.lblManagerTip, 0, 4);
            this.tblManager.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblManager.Location = new System.Drawing.Point(14, 36);
            this.tblManager.Margin = new System.Windows.Forms.Padding(0);
            this.tblManager.Name = "tblManager";
            this.tblManager.RowCount = 5;
            this.tblManager.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tblManager.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 52F));
            this.tblManager.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 14F));
            this.tblManager.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 52F));
            this.tblManager.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblManager.Size = new System.Drawing.Size(817, 186);
            this.tblManager.TabIndex = 0;
            // 
            // lblNewName
            // 
            this.lblNewName.AutoSize = false;
            this.lblNewName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblNewName.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblNewName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.lblNewName.Location = new System.Drawing.Point(0, 0);
            this.lblNewName.Margin = new System.Windows.Forms.Padding(0);
            this.lblNewName.Name = "lblNewName";
            this.lblNewName.Size = new System.Drawing.Size(817, 22);
            this.lblNewName.TabIndex = 0;
            this.lblNewName.Text = "新建型号名称";
            this.lblNewName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // fbNewName
            // 
            this.fbNewName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(252)))), ((int)(((byte)(254)))));
            this.fbNewName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fbNewName.Location = new System.Drawing.Point(0, 26);
            this.fbNewName.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.fbNewName.Name = "fbNewName";
            this.fbNewName.Size = new System.Drawing.Size(817, 44);
            this.fbNewName.TabIndex = 1;
            // 
            // tblActions
            // 
            this.tblActions.BackColor = System.Drawing.Color.Transparent;
            this.tblActions.ColumnCount = 5;
            this.tblActions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 184F));
            this.tblActions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 14F));
            this.tblActions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 184F));
            this.tblActions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 14F));
            this.tblActions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 184F));
            this.tblActions.Controls.Add(this.btnCreate, 0, 0);
            this.tblActions.Controls.Add(this.btnDelete, 2, 0);
            this.tblActions.Controls.Add(this.btnUse, 4, 0);
            this.tblActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblActions.Location = new System.Drawing.Point(0, 88);
            this.tblActions.Margin = new System.Windows.Forms.Padding(0);
            this.tblActions.Name = "tblActions";
            this.tblActions.RowCount = 1;
            this.tblActions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblActions.Size = new System.Drawing.Size(817, 52);
            this.tblActions.TabIndex = 2;
            // 
            // btnCreate
            // 
            this.btnCreate.CornerRadius = 8;
            this.btnCreate.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCreate.IconGap = 8;
            this.btnCreate.IconSize = 18;
            this.btnCreate.Location = new System.Drawing.Point(0, 0);
            this.btnCreate.Margin = new System.Windows.Forms.Padding(0);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(184, 52);
            this.btnCreate.TabIndex = 0;
            this.btnCreate.Text = "新建型号";
            this.btnCreate.Variant = AlphaRap.FlatButtonVariant.Primary;
            this.btnCreate.Click += new System.EventHandler(this.btCreate_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.CornerRadius = 8;
            this.btnDelete.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnDelete.Enabled = false;
            this.btnDelete.IconGap = 8;
            this.btnDelete.IconSize = 18;
            this.btnDelete.Location = new System.Drawing.Point(198, 0);
            this.btnDelete.Margin = new System.Windows.Forms.Padding(0);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(184, 52);
            this.btnDelete.TabIndex = 1;
            this.btnDelete.Text = "删除型号";
            this.btnDelete.Variant = AlphaRap.FlatButtonVariant.Danger;
            this.btnDelete.Click += new System.EventHandler(this.btDelete_Click);
            // 
            // btnUse
            // 
            this.btnUse.CornerRadius = 8;
            this.btnUse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnUse.Enabled = false;
            this.btnUse.IconGap = 8;
            this.btnUse.IconSize = 18;
            this.btnUse.Location = new System.Drawing.Point(396, 0);
            this.btnUse.Margin = new System.Windows.Forms.Padding(0);
            this.btnUse.Name = "btnUse";
            this.btnUse.Size = new System.Drawing.Size(184, 52);
            this.btnUse.TabIndex = 2;
            this.btnUse.Text = "使用选中型号";
            this.btnUse.Variant = AlphaRap.FlatButtonVariant.Success;
            this.btnUse.Click += new System.EventHandler(this.btUse_Click);
            // 
            // lblManagerTip
            // 
            this.lblManagerTip.AutoSize = false;
            this.lblManagerTip.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblManagerTip.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblManagerTip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(138)))), ((int)(((byte)(155)))));
            this.lblManagerTip.Location = new System.Drawing.Point(0, 146);
            this.lblManagerTip.Margin = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.lblManagerTip.Name = "lblManagerTip";
            this.lblManagerTip.Size = new System.Drawing.Size(817, 40);
            this.lblManagerTip.TabIndex = 3;
            this.lblManagerTip.Text = "新建 = 复制当前型号的参数并另存；删除与使用前需先在左侧列表选中一个型号。";
            this.lblManagerTip.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            // 
            // lblManagerCaption
            // 
            this.lblManagerCaption.AutoSize = false;
            this.lblManagerCaption.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblManagerCaption.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblManagerCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.lblManagerCaption.Location = new System.Drawing.Point(14, 10);
            this.lblManagerCaption.Margin = new System.Windows.Forms.Padding(0);
            this.lblManagerCaption.Name = "lblManagerCaption";
            this.lblManagerCaption.Size = new System.Drawing.Size(817, 26);
            this.lblManagerCaption.TabIndex = 0;
            this.lblManagerCaption.Text = "型号管理";
            this.lblManagerCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dataTable1
            // 
            this.dataTable1.Columns.AddRange(new System.Data.DataColumn[] {
            this.dataColumn1,
            this.dataColumn2,
            this.dataColumn3,
            this.dataColumn4,
            this.dataColumn5,
            this.dataColumn7,
            this.dataColumn6,
            this.dataColumn8});
            this.dataTable1.TableName = "ProductSetting";
            // 
            // dataColumn1
            // 
            this.dataColumn1.ColumnName = "PRODUCTID";
            // 
            // dataColumn2
            // 
            this.dataColumn2.ColumnName = "STEPID";
            // 
            // dataColumn3
            // 
            this.dataColumn3.ColumnName = "RECIPEID";
            // 
            // dataColumn4
            // 
            this.dataColumn4.ColumnName = "PORTID";
            // 
            // dataColumn5
            // 
            this.dataColumn5.Caption = "SN1KeySub";
            this.dataColumn5.ColumnName = "SN1KeySub";
            this.dataColumn5.DefaultValue = "";
            // 
            // dataColumn7
            // 
            this.dataColumn7.AllowDBNull = false;
            this.dataColumn7.ColumnName = "EnableSN1KeySub";
            this.dataColumn7.DataType = typeof(bool);
            this.dataColumn7.DefaultValue = false;
            // 
            // dataColumn6
            // 
            this.dataColumn6.AllowDBNull = false;
            this.dataColumn6.ColumnName = "SN2KeySub";
            this.dataColumn6.DefaultValue = "";
            // 
            // dataColumn8
            // 
            this.dataColumn8.AllowDBNull = false;
            this.dataColumn8.ColumnName = "EnableSN2KeySub";
            this.dataColumn8.DataType = typeof(bool);
            this.dataColumn8.DefaultValue = false;
            // 
            // ProductManagerForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(248)))), ((int)(((byte)(249)))));
            this.ClientSize = new System.Drawing.Size(1360, 780);
            this.Controls.Add(this.rootTable);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "ProductManagerForm";
            this.Text = "ProductManagerForm";
            this.Load += new System.EventHandler(this.ProductManagerForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.SettingData)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.RecipeData)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataTable1)).EndInit();
            this.tblActions.ResumeLayout(false);
            this.tblManager.ResumeLayout(false);
            this.cardManager.ResumeLayout(false);
            this.tblFields.ResumeLayout(false);
            this.cardSettings.ResumeLayout(false);
            this.tblRight.ResumeLayout(false);
            this.cardList.ResumeLayout(false);
            this.tblBody.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picPageIcon)).EndInit();
            this.pnlCurrent.ResumeLayout(false);
            this.pnlCurrent.PerformLayout();
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.rootTable.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel rootTable;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.PictureBox picPageIcon;
        private AlphaRap.UiLabel lblPageTitle;
        private AlphaRap.UiLabel lblPageHint;
        private AlphaRap.CardPanel pnlCurrent;
        private AlphaRap.UiLabel lblCurrentCaption;
        public AlphaRap.UiLabel CurrentModel;
        private System.Windows.Forms.TableLayoutPanel tblBody;
        private AlphaRap.CardPanel cardList;
        public System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private AlphaRap.UiLabel lblModelCount;
        private AlphaRap.UiLabel lblModelsCaption;
        private System.Windows.Forms.TableLayoutPanel tblRight;
        private AlphaRap.CardPanel cardSettings;
        private AlphaRap.UiLabel lblSettingsCaption;
        private AlphaRap.UiLabel lblSettingsHint;
        private System.Windows.Forms.TableLayoutPanel tblFields;
        private AlphaRap.UiLabel lblProductId;
        private AlphaRap.FieldBox fbProductId;
        private AlphaRap.UiLabel lblStepId;
        private AlphaRap.FieldBox fbStepId;
        private AlphaRap.UiLabel lblRecipeId;
        private AlphaRap.FieldBox fbRecipeId;
        private AlphaRap.UiLabel lblPortId;
        private AlphaRap.FieldBox fbPortId;
        private AlphaRap.UiLabel lblSubCaption;
        private AlphaRap.UiLabel lblSn1;
        private AlphaRap.FieldBox fbSn1;
        private AlphaRap.UiCheckBox chkEnableSn1;
        private AlphaRap.UiLabel lblSn2;
        private AlphaRap.FieldBox fbSn2;
        private AlphaRap.UiCheckBox chkEnableSn2;
        private AlphaRap.CardPanel cardManager;
        private AlphaRap.UiLabel lblManagerCaption;
        private System.Windows.Forms.TableLayoutPanel tblManager;
        private AlphaRap.UiLabel lblNewName;
        private AlphaRap.FieldBox fbNewName;
        private System.Windows.Forms.TableLayoutPanel tblActions;
        private AlphaRap.FlatButton btnCreate;
        private AlphaRap.FlatButton btnDelete;
        private AlphaRap.FlatButton btnUse;
        private AlphaRap.UiLabel lblManagerTip;
        private System.Data.DataTable dataTable1;
        private System.Data.DataColumn dataColumn1;
        private System.Data.DataColumn dataColumn2;
        private System.Data.DataColumn dataColumn3;
        private System.Data.DataColumn dataColumn4;
        private System.Data.DataColumn dataColumn5;
        private System.Data.DataColumn dataColumn6;
        private System.Data.DataColumn dataColumn7;
        private System.Data.DataColumn dataColumn8;
    }
}
