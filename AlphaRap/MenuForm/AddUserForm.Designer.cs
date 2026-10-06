namespace AlphaRap
{
    partial class AddUserForm
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
        /// 用户管理页。
        ///
        /// 重构成自适应的卡片布局：原来是一屏固定 1207x1028 的绝对坐标，
        /// 而宿主面板只有约 1348x740 —— 底部那一大块权限表必然被裁掉。
        /// 现在根容器 Dock=Fill，整页跟着宿主伸缩。
        /// </summary>
        private void InitializeComponent()
        {
            this.rootTable = new System.Windows.Forms.TableLayoutPanel();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblPageHint = new AlphaRap.UiLabel();
            this.lblPageTitle = new AlphaRap.UiLabel();
            this.picPageIcon = new System.Windows.Forms.PictureBox();
            this.tblCards = new System.Windows.Forms.TableLayoutPanel();
            this.cardUsers = new AlphaRap.CardPanel();
            this.dgvUserList = new System.Windows.Forms.DataGridView();
            this.lblUsersCaption = new AlphaRap.UiLabel();
            this.cardModify = new AlphaRap.CardPanel();
            this.tblModify = new System.Windows.Forms.TableLayoutPanel();
            this.lblModifyPermission = new AlphaRap.UiLabel();
            this.cbSelectedPermission = new System.Windows.Forms.ComboBox();
            this.lblSelectedUserName = new AlphaRap.UiLabel();
            this.textSelectedUserName = new AlphaRap.FieldBox();
            this.btnModifyPermission = new AlphaRap.FlatButton();
            this.btnDeleteUser = new AlphaRap.FlatButton();
            this.lblModifyCaption = new AlphaRap.UiLabel();
            this.cardAdd = new AlphaRap.CardPanel();
            this.tblAdd = new System.Windows.Forms.TableLayoutPanel();
            this.lblAddPermission = new AlphaRap.UiLabel();
            this.cbAddUserPermission = new System.Windows.Forms.ComboBox();
            this.lblAddUserName = new AlphaRap.UiLabel();
            this.textAddUserName = new AlphaRap.FieldBox();
            this.lblAddPassword = new AlphaRap.UiLabel();
            this.textPassword = new AlphaRap.FieldBox();
            this.lblAddConfirm = new AlphaRap.UiLabel();
            this.textPasswordConfirm = new AlphaRap.FieldBox();
            this.btnAddUser = new AlphaRap.FlatButton();
            this.lblAddCaption = new AlphaRap.UiLabel();
            this.cardRights = new AlphaRap.CardPanel();
            this.dgvRightsConfig = new System.Windows.Forms.DataGridView();
            this.pnlRightsBar = new System.Windows.Forms.Panel();
            this.lblRightsHint = new AlphaRap.UiLabel();
            this.btnSaveRights = new AlphaRap.FlatButton();
            this.lblRightsCaption = new AlphaRap.UiLabel();
            this.rootTable.SuspendLayout();
            this.pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPageIcon)).BeginInit();
            this.tblCards.SuspendLayout();
            this.cardUsers.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUserList)).BeginInit();
            this.cardModify.SuspendLayout();
            this.tblModify.SuspendLayout();
            this.cardAdd.SuspendLayout();
            this.tblAdd.SuspendLayout();
            this.cardRights.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRightsConfig)).BeginInit();
            this.pnlRightsBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // rootTable
            // 
            this.rootTable.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(248)))), ((int)(((byte)(249)))));
            this.rootTable.ColumnCount = 1;
            this.rootTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootTable.Controls.Add(this.pnlHeader, 0, 0);
            this.rootTable.Controls.Add(this.tblCards, 0, 1);
            this.rootTable.Controls.Add(this.cardRights, 0, 2);
            this.rootTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rootTable.Location = new System.Drawing.Point(0, 0);
            this.rootTable.Margin = new System.Windows.Forms.Padding(0);
            this.rootTable.Name = "rootTable";
            this.rootTable.Padding = new System.Windows.Forms.Padding(20, 16, 20, 18);
            this.rootTable.RowCount = 3;
            this.rootTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 52F));
            this.rootTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 360F));
            this.rootTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rootTable.Size = new System.Drawing.Size(1360, 780);
            this.rootTable.TabIndex = 0;
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.Transparent;
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
            // lblPageHint
            // 
            this.lblPageHint.AutoSize = true;
            this.lblPageHint.BackColor = System.Drawing.Color.Transparent;
            this.lblPageHint.Font = new System.Drawing.Font("微软雅黑", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblPageHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(138)))), ((int)(((byte)(155)))));
            this.lblPageHint.Location = new System.Drawing.Point(47, 31);
            this.lblPageHint.Name = "lblPageHint";
            this.lblPageHint.Size = new System.Drawing.Size(139, 19);
            this.lblPageHint.TabIndex = 2;
            this.lblPageHint.Text = "管理账号、密码与权限";
            // 
            // lblPageTitle
            // 
            this.lblPageTitle.AutoSize = true;
            this.lblPageTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblPageTitle.Font = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblPageTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.lblPageTitle.Location = new System.Drawing.Point(44, 0);
            this.lblPageTitle.Name = "lblPageTitle";
            this.lblPageTitle.Size = new System.Drawing.Size(92, 27);
            this.lblPageTitle.TabIndex = 1;
            this.lblPageTitle.Text = "用户管理";
            // 
            // picPageIcon
            // 
            this.picPageIcon.BackColor = System.Drawing.Color.Transparent;
            this.picPageIcon.Location = new System.Drawing.Point(0, 6);
            this.picPageIcon.Name = "picPageIcon";
            this.picPageIcon.Size = new System.Drawing.Size(34, 34);
            this.picPageIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picPageIcon.TabIndex = 0;
            this.picPageIcon.TabStop = false;
            // 
            // tblCards
            // 
            this.tblCards.BackColor = System.Drawing.Color.Transparent;
            this.tblCards.ColumnCount = 3;
            this.tblCards.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 34F));
            this.tblCards.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32F));
            this.tblCards.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 34F));
            this.tblCards.Controls.Add(this.cardUsers, 0, 0);
            this.tblCards.Controls.Add(this.cardModify, 1, 0);
            this.tblCards.Controls.Add(this.cardAdd, 2, 0);
            this.tblCards.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblCards.Location = new System.Drawing.Point(20, 68);
            this.tblCards.Margin = new System.Windows.Forms.Padding(0);
            this.tblCards.Name = "tblCards";
            this.tblCards.RowCount = 1;
            this.tblCards.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblCards.Size = new System.Drawing.Size(1320, 360);
            this.tblCards.TabIndex = 1;
            // 
            // cardUsers
            // 
            this.cardUsers.BackColor = System.Drawing.Color.White;
            this.cardUsers.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(231)))), ((int)(((byte)(240)))));
            this.cardUsers.Controls.Add(this.dgvUserList);
            this.cardUsers.Controls.Add(this.lblUsersCaption);
            this.cardUsers.CornerRadius = 10;
            this.cardUsers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardUsers.FillColor = System.Drawing.Color.White;
            this.cardUsers.Location = new System.Drawing.Point(0, 0);
            this.cardUsers.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.cardUsers.Name = "cardUsers";
            this.cardUsers.Padding = new System.Windows.Forms.Padding(14, 10, 14, 14);
            this.cardUsers.ShowBorder = true;
            this.cardUsers.Size = new System.Drawing.Size(436, 360);
            this.cardUsers.TabIndex = 0;
            // 
            // dgvUserList
            // 
            this.dgvUserList.AllowUserToAddRows = false;
            this.dgvUserList.AllowUserToDeleteRows = false;
            this.dgvUserList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvUserList.Location = new System.Drawing.Point(14, 36);
            this.dgvUserList.MultiSelect = false;
            this.dgvUserList.Name = "dgvUserList";
            this.dgvUserList.ReadOnly = true;
            this.dgvUserList.Size = new System.Drawing.Size(408, 310);
            this.dgvUserList.TabIndex = 1;
            this.dgvUserList.TabStop = false;
            this.dgvUserList.CurrentCellChanged += new System.EventHandler(this.dgvUserList_CurrentCellChanged);
            // 
            // lblUsersCaption
            // 
            this.lblUsersCaption.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblUsersCaption.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblUsersCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.lblUsersCaption.Location = new System.Drawing.Point(14, 10);
            this.lblUsersCaption.Name = "lblUsersCaption";
            this.lblUsersCaption.Size = new System.Drawing.Size(408, 26);
            this.lblUsersCaption.TabIndex = 0;
            this.lblUsersCaption.Text = "用户列表";
            this.lblUsersCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cardModify
            // 
            this.cardModify.BackColor = System.Drawing.Color.White;
            this.cardModify.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(231)))), ((int)(((byte)(240)))));
            this.cardModify.Controls.Add(this.tblModify);
            this.cardModify.Controls.Add(this.lblModifyCaption);
            this.cardModify.CornerRadius = 10;
            this.cardModify.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardModify.FillColor = System.Drawing.Color.White;
            this.cardModify.Location = new System.Drawing.Point(448, 0);
            this.cardModify.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.cardModify.Name = "cardModify";
            this.cardModify.Padding = new System.Windows.Forms.Padding(14, 10, 14, 14);
            this.cardModify.ShowBorder = true;
            this.cardModify.Size = new System.Drawing.Size(410, 360);
            this.cardModify.TabIndex = 1;
            // 
            // tblModify
            // 
            this.tblModify.BackColor = System.Drawing.Color.Transparent;
            this.tblModify.ColumnCount = 1;
            this.tblModify.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblModify.Controls.Add(this.lblModifyPermission, 0, 0);
            this.tblModify.Controls.Add(this.cbSelectedPermission, 0, 1);
            this.tblModify.Controls.Add(this.lblSelectedUserName, 0, 2);
            this.tblModify.Controls.Add(this.textSelectedUserName, 0, 3);
            this.tblModify.Controls.Add(this.btnModifyPermission, 0, 5);
            this.tblModify.Controls.Add(this.btnDeleteUser, 0, 6);
            this.tblModify.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblModify.Location = new System.Drawing.Point(14, 36);
            this.tblModify.Margin = new System.Windows.Forms.Padding(0);
            this.tblModify.Name = "tblModify";
            this.tblModify.RowCount = 7;
            this.tblModify.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tblModify.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tblModify.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tblModify.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tblModify.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblModify.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tblModify.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tblModify.Size = new System.Drawing.Size(382, 310);
            this.tblModify.TabIndex = 0;
            // 
            // lblModifyPermission
            // 
            this.lblModifyPermission.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblModifyPermission.Font = new System.Drawing.Font("微软雅黑", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblModifyPermission.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(138)))), ((int)(((byte)(155)))));
            this.lblModifyPermission.Location = new System.Drawing.Point(0, 0);
            this.lblModifyPermission.Margin = new System.Windows.Forms.Padding(0);
            this.lblModifyPermission.Name = "lblModifyPermission";
            this.lblModifyPermission.Size = new System.Drawing.Size(382, 24);
            this.lblModifyPermission.TabIndex = 0;
            this.lblModifyPermission.Text = "权限";
            this.lblModifyPermission.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cbSelectedPermission
            // 
            this.cbSelectedPermission.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbSelectedPermission.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSelectedPermission.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbSelectedPermission.Font = new System.Drawing.Font("微软雅黑", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cbSelectedPermission.FormattingEnabled = true;
            this.cbSelectedPermission.Location = new System.Drawing.Point(0, 26);
            this.cbSelectedPermission.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.cbSelectedPermission.Name = "cbSelectedPermission";
            this.cbSelectedPermission.Size = new System.Drawing.Size(382, 28);
            this.cbSelectedPermission.TabIndex = 1;
            // 
            // lblSelectedUserName
            // 
            this.lblSelectedUserName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSelectedUserName.Font = new System.Drawing.Font("微软雅黑", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblSelectedUserName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(138)))), ((int)(((byte)(155)))));
            this.lblSelectedUserName.Location = new System.Drawing.Point(0, 62);
            this.lblSelectedUserName.Margin = new System.Windows.Forms.Padding(0);
            this.lblSelectedUserName.Name = "lblSelectedUserName";
            this.lblSelectedUserName.Size = new System.Drawing.Size(382, 24);
            this.lblSelectedUserName.TabIndex = 2;
            this.lblSelectedUserName.Text = "用户名";
            this.lblSelectedUserName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textSelectedUserName
            // 
            this.textSelectedUserName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(252)))), ((int)(((byte)(254)))));
            this.textSelectedUserName.ContentReadOnly = true;
            this.textSelectedUserName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textSelectedUserName.Icon = null;
            this.textSelectedUserName.Location = new System.Drawing.Point(0, 88);
            this.textSelectedUserName.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.textSelectedUserName.MaxLength = 32767;
            this.textSelectedUserName.Name = "textSelectedUserName";
            this.textSelectedUserName.PasswordChar = '\0';
            this.textSelectedUserName.Size = new System.Drawing.Size(382, 34);
            this.textSelectedUserName.TabIndex = 3;
            this.textSelectedUserName.Value = "";
            // 
            // btnModifyPermission
            // 
            this.btnModifyPermission.BackColor = System.Drawing.Color.Transparent;
            this.btnModifyPermission.CornerRadius = 8;
            this.btnModifyPermission.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnModifyPermission.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnModifyPermission.Enabled = false;
            this.btnModifyPermission.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnModifyPermission.Icon = null;
            this.btnModifyPermission.IconGap = 8;
            this.btnModifyPermission.IconSize = 18;
            this.btnModifyPermission.Location = new System.Drawing.Point(0, 220);
            this.btnModifyPermission.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.btnModifyPermission.Name = "btnModifyPermission";
            this.btnModifyPermission.Selectable = false;
            this.btnModifyPermission.Size = new System.Drawing.Size(382, 42);
            this.btnModifyPermission.TabIndex = 5;
            this.btnModifyPermission.TabStop = false;
            this.btnModifyPermission.Text = "修改权限";
            this.btnModifyPermission.Variant = AlphaRap.FlatButtonVariant.Surface;
            this.btnModifyPermission.Click += new System.EventHandler(this.btnModifyPermission_Click);
            // 
            // btnDeleteUser
            // 
            this.btnDeleteUser.BackColor = System.Drawing.Color.Transparent;
            this.btnDeleteUser.CornerRadius = 8;
            this.btnDeleteUser.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDeleteUser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnDeleteUser.Enabled = false;
            this.btnDeleteUser.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnDeleteUser.Icon = null;
            this.btnDeleteUser.IconGap = 8;
            this.btnDeleteUser.IconSize = 18;
            this.btnDeleteUser.Location = new System.Drawing.Point(0, 266);
            this.btnDeleteUser.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.btnDeleteUser.Name = "btnDeleteUser";
            this.btnDeleteUser.Selectable = false;
            this.btnDeleteUser.Size = new System.Drawing.Size(382, 44);
            this.btnDeleteUser.TabIndex = 6;
            this.btnDeleteUser.TabStop = false;
            this.btnDeleteUser.Text = "删除用户";
            this.btnDeleteUser.Variant = AlphaRap.FlatButtonVariant.Danger;
            this.btnDeleteUser.Click += new System.EventHandler(this.btnDeleteUser_Click);
            // 
            // lblModifyCaption
            // 
            this.lblModifyCaption.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblModifyCaption.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblModifyCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.lblModifyCaption.Location = new System.Drawing.Point(14, 10);
            this.lblModifyCaption.Name = "lblModifyCaption";
            this.lblModifyCaption.Size = new System.Drawing.Size(382, 26);
            this.lblModifyCaption.TabIndex = 1;
            this.lblModifyCaption.Text = "修改用户权限";
            this.lblModifyCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cardAdd
            // 
            this.cardAdd.BackColor = System.Drawing.Color.White;
            this.cardAdd.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(231)))), ((int)(((byte)(240)))));
            this.cardAdd.Controls.Add(this.tblAdd);
            this.cardAdd.Controls.Add(this.lblAddCaption);
            this.cardAdd.CornerRadius = 10;
            this.cardAdd.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardAdd.FillColor = System.Drawing.Color.White;
            this.cardAdd.Location = new System.Drawing.Point(870, 0);
            this.cardAdd.Margin = new System.Windows.Forms.Padding(0);
            this.cardAdd.Name = "cardAdd";
            this.cardAdd.Padding = new System.Windows.Forms.Padding(14, 10, 14, 14);
            this.cardAdd.ShowBorder = true;
            this.cardAdd.Size = new System.Drawing.Size(450, 360);
            this.cardAdd.TabIndex = 2;
            // 
            // tblAdd
            // 
            this.tblAdd.BackColor = System.Drawing.Color.Transparent;
            this.tblAdd.ColumnCount = 1;
            this.tblAdd.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblAdd.Controls.Add(this.lblAddPermission, 0, 0);
            this.tblAdd.Controls.Add(this.cbAddUserPermission, 0, 1);
            this.tblAdd.Controls.Add(this.lblAddUserName, 0, 2);
            this.tblAdd.Controls.Add(this.textAddUserName, 0, 3);
            this.tblAdd.Controls.Add(this.lblAddPassword, 0, 4);
            this.tblAdd.Controls.Add(this.textPassword, 0, 5);
            this.tblAdd.Controls.Add(this.lblAddConfirm, 0, 6);
            this.tblAdd.Controls.Add(this.textPasswordConfirm, 0, 7);
            this.tblAdd.Controls.Add(this.btnAddUser, 0, 9);
            this.tblAdd.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblAdd.Location = new System.Drawing.Point(14, 36);
            this.tblAdd.Margin = new System.Windows.Forms.Padding(0);
            this.tblAdd.Name = "tblAdd";
            this.tblAdd.RowCount = 10;
            this.tblAdd.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tblAdd.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tblAdd.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tblAdd.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tblAdd.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tblAdd.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tblAdd.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tblAdd.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tblAdd.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblAdd.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tblAdd.Size = new System.Drawing.Size(422, 310);
            this.tblAdd.TabIndex = 0;
            // 
            // lblAddPermission
            // 
            this.lblAddPermission.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAddPermission.Font = new System.Drawing.Font("微软雅黑", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblAddPermission.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(138)))), ((int)(((byte)(155)))));
            this.lblAddPermission.Location = new System.Drawing.Point(0, 0);
            this.lblAddPermission.Margin = new System.Windows.Forms.Padding(0);
            this.lblAddPermission.Name = "lblAddPermission";
            this.lblAddPermission.Size = new System.Drawing.Size(422, 22);
            this.lblAddPermission.TabIndex = 0;
            this.lblAddPermission.Text = "权限";
            this.lblAddPermission.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cbAddUserPermission
            // 
            this.cbAddUserPermission.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cbAddUserPermission.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbAddUserPermission.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbAddUserPermission.Font = new System.Drawing.Font("微软雅黑", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cbAddUserPermission.FormattingEnabled = true;
            this.cbAddUserPermission.Location = new System.Drawing.Point(0, 24);
            this.cbAddUserPermission.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.cbAddUserPermission.Name = "cbAddUserPermission";
            this.cbAddUserPermission.Size = new System.Drawing.Size(422, 28);
            this.cbAddUserPermission.TabIndex = 1;
            // 
            // lblAddUserName
            // 
            this.lblAddUserName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAddUserName.Font = new System.Drawing.Font("微软雅黑", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblAddUserName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(138)))), ((int)(((byte)(155)))));
            this.lblAddUserName.Location = new System.Drawing.Point(0, 58);
            this.lblAddUserName.Margin = new System.Windows.Forms.Padding(0);
            this.lblAddUserName.Name = "lblAddUserName";
            this.lblAddUserName.Size = new System.Drawing.Size(422, 20);
            this.lblAddUserName.TabIndex = 2;
            this.lblAddUserName.Text = "用户名";
            this.lblAddUserName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textAddUserName
            // 
            this.textAddUserName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(252)))), ((int)(((byte)(254)))));
            this.textAddUserName.ContentReadOnly = false;
            this.textAddUserName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textAddUserName.Icon = null;
            this.textAddUserName.Location = new System.Drawing.Point(0, 80);
            this.textAddUserName.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.textAddUserName.MaxLength = 32767;
            this.textAddUserName.Name = "textAddUserName";
            this.textAddUserName.PasswordChar = '\0';
            this.textAddUserName.Size = new System.Drawing.Size(422, 32);
            this.textAddUserName.TabIndex = 3;
            this.textAddUserName.Value = "";
            // 
            // lblAddPassword
            // 
            this.lblAddPassword.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAddPassword.Font = new System.Drawing.Font("微软雅黑", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblAddPassword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(138)))), ((int)(((byte)(155)))));
            this.lblAddPassword.Location = new System.Drawing.Point(0, 114);
            this.lblAddPassword.Margin = new System.Windows.Forms.Padding(0);
            this.lblAddPassword.Name = "lblAddPassword";
            this.lblAddPassword.Size = new System.Drawing.Size(422, 20);
            this.lblAddPassword.TabIndex = 4;
            this.lblAddPassword.Text = "密码";
            this.lblAddPassword.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textPassword
            // 
            this.textPassword.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(252)))), ((int)(((byte)(254)))));
            this.textPassword.ContentReadOnly = false;
            this.textPassword.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textPassword.Icon = null;
            this.textPassword.Location = new System.Drawing.Point(0, 136);
            this.textPassword.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.textPassword.MaxLength = 32767;
            this.textPassword.Name = "textPassword";
            this.textPassword.PasswordChar = '●';
            this.textPassword.Size = new System.Drawing.Size(422, 32);
            this.textPassword.TabIndex = 5;
            this.textPassword.Value = "";
            // 
            // lblAddConfirm
            // 
            this.lblAddConfirm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblAddConfirm.Font = new System.Drawing.Font("微软雅黑", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblAddConfirm.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(138)))), ((int)(((byte)(155)))));
            this.lblAddConfirm.Location = new System.Drawing.Point(0, 170);
            this.lblAddConfirm.Margin = new System.Windows.Forms.Padding(0);
            this.lblAddConfirm.Name = "lblAddConfirm";
            this.lblAddConfirm.Size = new System.Drawing.Size(422, 20);
            this.lblAddConfirm.TabIndex = 6;
            this.lblAddConfirm.Text = "确认密码";
            this.lblAddConfirm.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textPasswordConfirm
            // 
            this.textPasswordConfirm.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(252)))), ((int)(((byte)(254)))));
            this.textPasswordConfirm.ContentReadOnly = false;
            this.textPasswordConfirm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textPasswordConfirm.Icon = null;
            this.textPasswordConfirm.Location = new System.Drawing.Point(0, 192);
            this.textPasswordConfirm.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.textPasswordConfirm.MaxLength = 32767;
            this.textPasswordConfirm.Name = "textPasswordConfirm";
            this.textPasswordConfirm.PasswordChar = '●';
            this.textPasswordConfirm.Size = new System.Drawing.Size(422, 32);
            this.textPasswordConfirm.TabIndex = 7;
            this.textPasswordConfirm.Value = "";
            // 
            // btnAddUser
            // 
            this.btnAddUser.BackColor = System.Drawing.Color.Transparent;
            this.btnAddUser.CornerRadius = 8;
            this.btnAddUser.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddUser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAddUser.Enabled = false;
            this.btnAddUser.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnAddUser.Icon = null;
            this.btnAddUser.IconGap = 8;
            this.btnAddUser.IconSize = 18;
            this.btnAddUser.Location = new System.Drawing.Point(0, 264);
            this.btnAddUser.Margin = new System.Windows.Forms.Padding(0);
            this.btnAddUser.Name = "btnAddUser";
            this.btnAddUser.Selectable = false;
            this.btnAddUser.Size = new System.Drawing.Size(422, 46);
            this.btnAddUser.TabIndex = 9;
            this.btnAddUser.TabStop = false;
            this.btnAddUser.Text = "新增用户";
            this.btnAddUser.Variant = AlphaRap.FlatButtonVariant.Primary;
            this.btnAddUser.Click += new System.EventHandler(this.btnAddUser_Click);
            // 
            // lblAddCaption
            // 
            this.lblAddCaption.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblAddCaption.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblAddCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.lblAddCaption.Location = new System.Drawing.Point(14, 10);
            this.lblAddCaption.Name = "lblAddCaption";
            this.lblAddCaption.Size = new System.Drawing.Size(422, 26);
            this.lblAddCaption.TabIndex = 1;
            this.lblAddCaption.Text = "新增用户";
            this.lblAddCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cardRights
            // 
            this.cardRights.BackColor = System.Drawing.Color.White;
            this.cardRights.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(231)))), ((int)(((byte)(240)))));
            this.cardRights.Controls.Add(this.dgvRightsConfig);
            this.cardRights.Controls.Add(this.pnlRightsBar);
            this.cardRights.Controls.Add(this.lblRightsCaption);
            this.cardRights.CornerRadius = 10;
            this.cardRights.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardRights.FillColor = System.Drawing.Color.White;
            this.cardRights.Location = new System.Drawing.Point(20, 428);
            this.cardRights.Margin = new System.Windows.Forms.Padding(0);
            this.cardRights.Name = "cardRights";
            this.cardRights.Padding = new System.Windows.Forms.Padding(14, 10, 14, 12);
            this.cardRights.ShowBorder = true;
            this.cardRights.Size = new System.Drawing.Size(1320, 334);
            this.cardRights.TabIndex = 2;
            // 
            // dgvRightsConfig
            // 
            this.dgvRightsConfig.AllowUserToAddRows = false;
            this.dgvRightsConfig.AllowUserToDeleteRows = false;
            this.dgvRightsConfig.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dgvRightsConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvRightsConfig.Location = new System.Drawing.Point(14, 36);
            this.dgvRightsConfig.MultiSelect = false;
            this.dgvRightsConfig.Name = "dgvRightsConfig";
            this.dgvRightsConfig.Size = new System.Drawing.Size(1292, 236);
            this.dgvRightsConfig.TabIndex = 1;
            this.dgvRightsConfig.TabStop = false;
            this.dgvRightsConfig.Click += new System.EventHandler(this.DataChange_Click);
            // 
            // pnlRightsBar
            // 
            this.pnlRightsBar.BackColor = System.Drawing.Color.Transparent;
            this.pnlRightsBar.Controls.Add(this.lblRightsHint);
            this.pnlRightsBar.Controls.Add(this.btnSaveRights);
            this.pnlRightsBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlRightsBar.Location = new System.Drawing.Point(14, 272);
            this.pnlRightsBar.Name = "pnlRightsBar";
            this.pnlRightsBar.Size = new System.Drawing.Size(1292, 50);
            this.pnlRightsBar.TabIndex = 2;
            // 
            // lblRightsHint
            // 
            this.lblRightsHint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRightsHint.Font = new System.Drawing.Font("微软雅黑", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblRightsHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(138)))), ((int)(((byte)(155)))));
            this.lblRightsHint.Location = new System.Drawing.Point(0, 0);
            this.lblRightsHint.Name = "lblRightsHint";
            this.lblRightsHint.Size = new System.Drawing.Size(1122, 50);
            this.lblRightsHint.TabIndex = 0;
            this.lblRightsHint.Text = "勾选各权限允许的功能后点保存";
            this.lblRightsHint.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnSaveRights
            // 
            this.btnSaveRights.BackColor = System.Drawing.Color.Transparent;
            this.btnSaveRights.CornerRadius = 8;
            this.btnSaveRights.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSaveRights.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnSaveRights.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnSaveRights.Icon = null;
            this.btnSaveRights.IconGap = 8;
            this.btnSaveRights.IconSize = 18;
            this.btnSaveRights.Location = new System.Drawing.Point(1122, 0);
            this.btnSaveRights.Margin = new System.Windows.Forms.Padding(0);
            this.btnSaveRights.Name = "btnSaveRights";
            this.btnSaveRights.Selectable = false;
            this.btnSaveRights.Size = new System.Drawing.Size(170, 50);
            this.btnSaveRights.TabIndex = 1;
            this.btnSaveRights.TabStop = false;
            this.btnSaveRights.Text = "保存权限";
            this.btnSaveRights.Variant = AlphaRap.FlatButtonVariant.Success;
            this.btnSaveRights.Click += new System.EventHandler(this.btnSaveRights_Click);
            // 
            // lblRightsCaption
            // 
            this.lblRightsCaption.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblRightsCaption.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblRightsCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.lblRightsCaption.Location = new System.Drawing.Point(14, 10);
            this.lblRightsCaption.Name = "lblRightsCaption";
            this.lblRightsCaption.Size = new System.Drawing.Size(1292, 26);
            this.lblRightsCaption.TabIndex = 0;
            this.lblRightsCaption.Text = "权限设置";
            this.lblRightsCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // AddUserForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(248)))), ((int)(((byte)(249)))));
            this.ClientSize = new System.Drawing.Size(1360, 780);
            this.ControlBox = false;
            this.Controls.Add(this.rootTable);
            this.Font = new System.Drawing.Font("微软雅黑", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "AddUserForm";
            this.Text = "用户管理";
            this.Leave += new System.EventHandler(this.AddUserForm_Leave);
            this.rootTable.ResumeLayout(false);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPageIcon)).EndInit();
            this.tblCards.ResumeLayout(false);
            this.cardUsers.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvUserList)).EndInit();
            this.cardModify.ResumeLayout(false);
            this.tblModify.ResumeLayout(false);
            this.cardAdd.ResumeLayout(false);
            this.tblAdd.ResumeLayout(false);
            this.cardRights.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvRightsConfig)).EndInit();
            this.pnlRightsBar.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel rootTable;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.PictureBox picPageIcon;
        private AlphaRap.UiLabel lblPageTitle;
        private AlphaRap.UiLabel lblPageHint;
        private System.Windows.Forms.TableLayoutPanel tblCards;
        private AlphaRap.CardPanel cardUsers;
        private AlphaRap.UiLabel lblUsersCaption;
        private System.Windows.Forms.DataGridView dgvUserList;
        private AlphaRap.CardPanel cardModify;
        private AlphaRap.UiLabel lblModifyCaption;
        private System.Windows.Forms.TableLayoutPanel tblModify;
        private AlphaRap.UiLabel lblModifyPermission;
        private System.Windows.Forms.ComboBox cbSelectedPermission;
        private AlphaRap.UiLabel lblSelectedUserName;
        private AlphaRap.FieldBox textSelectedUserName;
        private AlphaRap.FlatButton btnModifyPermission;
        private AlphaRap.FlatButton btnDeleteUser;
        private AlphaRap.CardPanel cardAdd;
        private AlphaRap.UiLabel lblAddCaption;
        private System.Windows.Forms.TableLayoutPanel tblAdd;
        private AlphaRap.UiLabel lblAddPermission;
        private System.Windows.Forms.ComboBox cbAddUserPermission;
        private AlphaRap.UiLabel lblAddUserName;
        private AlphaRap.FieldBox textAddUserName;
        private AlphaRap.UiLabel lblAddPassword;
        private AlphaRap.FieldBox textPassword;
        private AlphaRap.UiLabel lblAddConfirm;
        private AlphaRap.FieldBox textPasswordConfirm;
        private AlphaRap.FlatButton btnAddUser;
        private AlphaRap.CardPanel cardRights;
        private AlphaRap.UiLabel lblRightsCaption;
        private System.Windows.Forms.DataGridView dgvRightsConfig;
        private System.Windows.Forms.Panel pnlRightsBar;
        private AlphaRap.UiLabel lblRightsHint;
        private AlphaRap.FlatButton btnSaveRights;
    }
}
