namespace AlphaRap
{
    partial class VpVppPage
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

        #region 组件设计器生成的代码

        private void InitializeComponent()
        {
            this.head = new System.Windows.Forms.Panel();
            this.lblInfo = new AlphaRap.UiLabel();
            this.compRow = new System.Windows.Forms.Panel();
            this.gridComp = new System.Windows.Forms.DataGridView();
            this.cItem = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cMin = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cMax = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.path = new AlphaRap.UiLabel();
            this.pathRow = new System.Windows.Forms.FlowLayoutPanel();
            this.cbSaveImg = new AlphaRap.UiCheckBox();
            this.lblSavePath = new AlphaRap.UiLabel();
            this.tbSavePath = new System.Windows.Forms.TextBox();
            this.btnBrowsePath = new AlphaRap.FlatButton();
            this.optionsRow = new System.Windows.Forms.FlowLayoutPanel();
            this.cbOrig = new AlphaRap.UiCheckBox();
            this.cbSnap = new AlphaRap.UiCheckBox();
            this.cbAutoDel = new AlphaRap.UiCheckBox();
            this.keepGroup = new System.Windows.Forms.Panel();
            this.lblSaveDays = new AlphaRap.UiLabel();
            this.tbSaveDays = new System.Windows.Forms.TextBox();
            this.paramRow = new System.Windows.Forms.FlowLayoutPanel();
            this.lblExp = new AlphaRap.UiLabel();
            this.tbExp = new System.Windows.Forms.TextBox();
            this.lblVppName = new AlphaRap.UiLabel();
            this.tbRename = new System.Windows.Forms.TextBox();
            this.foot = new System.Windows.Forms.FlowLayoutPanel();
            this.btnEditTb = new AlphaRap.FlatButton();
            this.btnCapture = new AlphaRap.FlatButton();
            this.btnSaveVpp = new AlphaRap.FlatButton();
            this.btnAddCompRow = new AlphaRap.FlatButton();
            this.btnDelCompRow = new AlphaRap.FlatButton();
            this.head.SuspendLayout();
            this.compRow.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridComp)).BeginInit();
            this.pathRow.SuspendLayout();
            this.optionsRow.SuspendLayout();
            this.keepGroup.SuspendLayout();
            this.paramRow.SuspendLayout();
            this.foot.SuspendLayout();
            this.SuspendLayout();
            // 
            // head
            // 
            this.head.BackColor = System.Drawing.Color.White;
            this.head.Controls.Add(this.lblInfo);
            this.head.Controls.Add(this.path);
            this.head.Dock = System.Windows.Forms.DockStyle.Top;
            this.head.Location = new System.Drawing.Point(0, 0);
            this.head.Name = "head";
            this.head.Padding = new System.Windows.Forms.Padding(12, 0, 12, 0);
            this.head.Size = new System.Drawing.Size(720, 36);
            this.head.TabIndex = 0;
            // 
            // lblInfo
            // 
            this.lblInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblInfo.Font = new System.Drawing.Font("微软雅黑", 11F, System.Drawing.FontStyle.Bold);
            this.lblInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.lblInfo.Location = new System.Drawing.Point(12, 0);
            this.lblInfo.Name = "vpVppInfo_dyn";
            this.lblInfo.Size = new System.Drawing.Size(696, 36);
            this.lblInfo.TabIndex = 0;
            this.lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // foot：按钮已并入 paramRow，不再单独占行
            // 
            this.foot.Name = "foot";
            this.foot.Visible = false;
            this.foot.Size = new System.Drawing.Size(0, 0);
            // 
            // paramRow（曝光 + VPP名称 + 操作按钮：一行，放在存图上方）
            // 
            this.paramRow.BackColor = System.Drawing.Color.White;
            this.paramRow.Controls.Add(this.lblExp);
            this.paramRow.Controls.Add(this.tbExp);
            this.paramRow.Controls.Add(this.lblVppName);
            this.paramRow.Controls.Add(this.tbRename);
            this.paramRow.Controls.Add(this.btnEditTb);
            this.paramRow.Controls.Add(this.btnCapture);
            this.paramRow.Controls.Add(this.btnSaveVpp);
            this.paramRow.Controls.Add(this.btnAddCompRow);
            this.paramRow.Controls.Add(this.btnDelCompRow);
            this.paramRow.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.paramRow.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.paramRow.Location = new System.Drawing.Point(0, 332);
            this.paramRow.Name = "paramRow";
            this.paramRow.Padding = new System.Windows.Forms.Padding(12, 4, 12, 4);
            this.paramRow.Size = new System.Drawing.Size(720, 44);
            this.paramRow.TabIndex = 5;
            this.paramRow.WrapContents = false;
            // 
            // optionsRow：已并入 pathRow，不再单独占行
            // 
            this.optionsRow.Name = "optionsRow";
            this.optionsRow.Visible = false;
            this.optionsRow.Size = new System.Drawing.Size(0, 0);
            // 
            // pathRow（原图/截图/自动删除/保留天数 + 存图/路径：同一行）
            // 
            this.pathRow.BackColor = System.Drawing.Color.White;
            this.pathRow.Controls.Add(this.cbOrig);
            this.pathRow.Controls.Add(this.cbSnap);
            this.pathRow.Controls.Add(this.cbAutoDel);
            this.pathRow.Controls.Add(this.keepGroup);
            this.pathRow.Controls.Add(this.cbSaveImg);
            this.pathRow.Controls.Add(this.lblSavePath);
            this.pathRow.Controls.Add(this.tbSavePath);
            this.pathRow.Controls.Add(this.btnBrowsePath);
            this.pathRow.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pathRow.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.pathRow.Location = new System.Drawing.Point(0, 332);
            this.pathRow.Name = "pathRow";
            this.pathRow.Padding = new System.Windows.Forms.Padding(12, 4, 12, 4);
            this.pathRow.Size = new System.Drawing.Size(720, 40);
            this.pathRow.TabIndex = 4;
            this.pathRow.WrapContents = false;
            // 
            // path（vpp 路径：贴标题行右侧）
            // 
            this.path.Dock = System.Windows.Forms.DockStyle.Right;
            this.path.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.path.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(138)))), ((int)(((byte)(155)))));
            this.path.Location = new System.Drawing.Point(300, 0);
            this.path.Name = "vpVppPath_dyn";
            this.path.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.path.Size = new System.Drawing.Size(408, 36);
            this.path.TabIndex = 1;
            this.path.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // compRow（补偿表：占满中间剩余空间）
            // 
            this.compRow.BackColor = System.Drawing.Color.White;
            this.compRow.Controls.Add(this.gridComp);
            this.compRow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.compRow.Location = new System.Drawing.Point(0, 36);
            this.compRow.Name = "compRow";
            this.compRow.Padding = new System.Windows.Forms.Padding(12, 4, 12, 4);
            this.compRow.Size = new System.Drawing.Size(720, 230);
            this.compRow.TabIndex = 1;
            // 
            // gridComp
            // 
            this.gridComp.AllowUserToAddRows = false;
            this.gridComp.AllowUserToDeleteRows = false;
            this.gridComp.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gridComp.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.gridComp.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.cItem,
            this.cMin,
            this.cMax});
            this.gridComp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridComp.Location = new System.Drawing.Point(12, 4);
            this.gridComp.Name = "vpCompGrid";
            this.gridComp.RowHeadersVisible = false;
            this.gridComp.Size = new System.Drawing.Size(696, 222);
            this.gridComp.TabIndex = 0;
            // 
            // cItem
            // 
            this.cItem.HeaderText = "项目";
            this.cItem.Name = "cItem";
            // 
            // cMin
            // 
            this.cMin.HeaderText = "补偿下限";
            this.cMin.Name = "cMin";
            // 
            // cMax
            // 
            this.cMax.HeaderText = "补偿上限";
            this.cMax.Name = "cMax";
            // 
            // cbSaveImg
            // 
            this.cbSaveImg.AutoSize = true;
            this.cbSaveImg.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cbSaveImg.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.cbSaveImg.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(108)))), ((int)(((byte)(126)))));
            this.cbSaveImg.Location = new System.Drawing.Point(12, 10);
            this.cbSaveImg.Margin = new System.Windows.Forms.Padding(0, 4, 10, 0);
            this.cbSaveImg.Name = "vpRow_SaveImage";
            this.cbSaveImg.Size = new System.Drawing.Size(55, 24);
            this.cbSaveImg.TabIndex = 0;
            this.cbSaveImg.Text = "存图";
            // 
            // lblSavePath
            // 
            this.lblSavePath.AutoSize = true;
            this.lblSavePath.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblSavePath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(108)))), ((int)(((byte)(126)))));
            this.lblSavePath.Location = new System.Drawing.Point(77, 8);
            this.lblSavePath.Margin = new System.Windows.Forms.Padding(0, 6, 6, 0);
            this.lblSavePath.Name = "vpRow_SavePath";
            this.lblSavePath.Size = new System.Drawing.Size(65, 20);
            this.lblSavePath.TabIndex = 1;
            this.lblSavePath.Text = "存图路径";
            this.lblSavePath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbSavePath
            // 
            this.tbSavePath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(252)))), ((int)(((byte)(254)))));
            this.tbSavePath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbSavePath.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.tbSavePath.Location = new System.Drawing.Point(148, 8);
            this.tbSavePath.Margin = new System.Windows.Forms.Padding(0, 2, 6, 0);
            this.tbSavePath.Name = "vpSavePathBox";
            this.tbSavePath.Size = new System.Drawing.Size(140, 25);
            this.tbSavePath.TabIndex = 2;
            // 
            // btnBrowsePath
            // 
            this.btnBrowsePath.BackColor = System.Drawing.Color.Transparent;
            this.btnBrowsePath.CornerRadius = 8;
            this.btnBrowsePath.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBrowsePath.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.btnBrowsePath.Location = new System.Drawing.Point(374, 6);
            this.btnBrowsePath.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            this.btnBrowsePath.Name = "vpBtn_BrowsePath";
            this.btnBrowsePath.Size = new System.Drawing.Size(40, 28);
            this.btnBrowsePath.TabIndex = 3;
            this.btnBrowsePath.Text = "...";
            this.btnBrowsePath.Variant = AlphaRap.FlatButtonVariant.Surface;
            // 
            // cbOrig
            // 
            this.cbOrig.AutoSize = true;
            this.cbOrig.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cbOrig.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.cbOrig.Margin = new System.Windows.Forms.Padding(0, 6, 4, 0);
            this.cbOrig.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(108)))), ((int)(((byte)(126)))));
            this.cbOrig.Location = new System.Drawing.Point(12, 8);
            this.cbOrig.Margin = new System.Windows.Forms.Padding(0, 4, 12, 0);
            this.cbOrig.Name = "vpRow_SaveOriginal";
            this.cbOrig.Size = new System.Drawing.Size(55, 24);
            this.cbOrig.TabIndex = 0;
            this.cbOrig.Text = "原图";
            // 
            // cbSnap
            // 
            this.cbSnap.AutoSize = true;
            this.cbSnap.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cbSnap.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.cbSnap.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(108)))), ((int)(((byte)(126)))));
            this.cbSnap.Location = new System.Drawing.Point(79, 8);
            this.cbSnap.Margin = new System.Windows.Forms.Padding(0, 4, 12, 0);
            this.cbSnap.Name = "vpRow_SaveSnapshot";
            this.cbSnap.Size = new System.Drawing.Size(55, 24);
            this.cbSnap.TabIndex = 1;
            this.cbSnap.Text = "截图";
            // 
            // cbAutoDel
            // 
            this.cbAutoDel.AutoSize = true;
            this.cbAutoDel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cbAutoDel.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.cbAutoDel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(108)))), ((int)(((byte)(126)))));
            this.cbAutoDel.Location = new System.Drawing.Point(146, 8);
            this.cbAutoDel.Margin = new System.Windows.Forms.Padding(0, 4, 16, 0);
            this.cbAutoDel.Name = "vpRow_SaveAutoDel";
            this.cbAutoDel.Size = new System.Drawing.Size(87, 24);
            this.cbAutoDel.TabIndex = 2;
            this.cbAutoDel.Text = "自动删除";
            // 
            // keepGroup（标签+输入框绑定，避免换行拆散）
            // 
            this.keepGroup.Controls.Add(this.lblSaveDays);
            this.keepGroup.Controls.Add(this.tbSaveDays);
            this.keepGroup.Location = new System.Drawing.Point(249, 4);
            this.keepGroup.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            this.keepGroup.Name = "keepGroup";
            this.keepGroup.Size = new System.Drawing.Size(168, 30);
            this.keepGroup.TabIndex = 3;
            // 
            // lblSaveDays
            // 
            this.lblSaveDays.AutoSize = true;
            this.lblSaveDays.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblSaveDays.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(108)))), ((int)(((byte)(126)))));
            this.lblSaveDays.Location = new System.Drawing.Point(0, 5);
            this.lblSaveDays.Name = "vpRow_SaveDays";
            this.lblSaveDays.Size = new System.Drawing.Size(65, 20);
            this.lblSaveDays.TabIndex = 0;
            this.lblSaveDays.Text = "保留天数";
            // 
            // tbSaveDays
            // 
            this.tbSaveDays.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tbSaveDays.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(252)))), ((int)(((byte)(254)))));
            this.tbSaveDays.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbSaveDays.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.tbSaveDays.Location = new System.Drawing.Point(108, 2);
            this.tbSaveDays.Name = "vpSaveDaysBox";
            this.tbSaveDays.Size = new System.Drawing.Size(56, 25);
            this.tbSaveDays.TabIndex = 1;
            this.tbSaveDays.Text = "0";
            // 
            // lblExp
            // 
            this.lblExp.AutoSize = true;
            this.lblExp.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblExp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(108)))), ((int)(((byte)(126)))));
            this.lblExp.Location = new System.Drawing.Point(12, 10);
            this.lblExp.Margin = new System.Windows.Forms.Padding(0, 4, 6, 0);
            this.lblExp.Name = "vpRow_Exposure";
            this.lblExp.Size = new System.Drawing.Size(37, 20);
            this.lblExp.TabIndex = 0;
            this.lblExp.Text = "曝光";
            // 
            // tbExp
            // 
            this.tbExp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(252)))), ((int)(((byte)(254)))));
            this.tbExp.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbExp.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.tbExp.Location = new System.Drawing.Point(55, 8);
            this.tbExp.Margin = new System.Windows.Forms.Padding(0, 2, 16, 0);
            this.tbExp.Name = "vpVppExpBox";
            this.tbExp.Size = new System.Drawing.Size(64, 25);
            this.tbExp.TabIndex = 1;
            // 
            // lblVppName
            // 
            this.lblVppName.AutoSize = true;
            this.lblVppName.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblVppName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(108)))), ((int)(((byte)(126)))));
            this.lblVppName.Location = new System.Drawing.Point(151, 10);
            this.lblVppName.Margin = new System.Windows.Forms.Padding(0, 4, 6, 0);
            this.lblVppName.Name = "vpRow_VppName";
            this.lblVppName.Size = new System.Drawing.Size(65, 20);
            this.lblVppName.TabIndex = 2;
            this.lblVppName.Text = "VPP 名称";
            // 
            // tbRename
            // 
            this.tbRename.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(252)))), ((int)(((byte)(254)))));
            this.tbRename.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbRename.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.tbRename.Location = new System.Drawing.Point(222, 8);
            this.tbRename.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.tbRename.Name = "vpRenameBox";
            this.tbRename.Size = new System.Drawing.Size(100, 25);
            this.tbRename.TabIndex = 3;
            // 
            // btnEditTb
            // 
            this.btnEditTb.BackColor = System.Drawing.Color.Transparent;
            this.btnEditTb.CornerRadius = 8;
            this.btnEditTb.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEditTb.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.btnEditTb.Location = new System.Drawing.Point(12, 8);
            this.btnEditTb.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.btnEditTb.Name = "vpBtn_EditTb";
            // 设计时尺寸贴近运行时 LayoutParamRow（英文 Delete row 需更宽）
            this.btnEditTb.Size = new System.Drawing.Size(88, 30);
            this.btnEditTb.TabIndex = 0;
            this.btnEditTb.Text = "编辑 TB";
            this.btnEditTb.Variant = AlphaRap.FlatButtonVariant.Surface;
            // 
            // btnCapture
            // 
            this.btnCapture.BackColor = System.Drawing.Color.Transparent;
            this.btnCapture.CornerRadius = 8;
            this.btnCapture.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCapture.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.btnCapture.Location = new System.Drawing.Point(128, 8);
            this.btnCapture.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.btnCapture.Name = "vpBtn_Capture";
            this.btnCapture.Size = new System.Drawing.Size(72, 30);
            this.btnCapture.TabIndex = 1;
            this.btnCapture.Text = "拍照";
            this.btnCapture.Variant = AlphaRap.FlatButtonVariant.Primary;
            // 
            // btnSaveVpp
            // 
            this.btnSaveVpp.BackColor = System.Drawing.Color.Transparent;
            this.btnSaveVpp.CornerRadius = 8;
            this.btnSaveVpp.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSaveVpp.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.btnSaveVpp.Location = new System.Drawing.Point(236, 8);
            this.btnSaveVpp.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.btnSaveVpp.Name = "vpBtn_SaveVpp";
            this.btnSaveVpp.Size = new System.Drawing.Size(96, 30);
            this.btnSaveVpp.TabIndex = 2;
            this.btnSaveVpp.Text = "保存 VPP";
            this.btnSaveVpp.Variant = AlphaRap.FlatButtonVariant.Primary;
            // 
            // btnAddCompRow
            // 
            this.btnAddCompRow.BackColor = System.Drawing.Color.Transparent;
            this.btnAddCompRow.CornerRadius = 8;
            this.btnAddCompRow.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddCompRow.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.btnAddCompRow.Location = new System.Drawing.Point(352, 8);
            this.btnAddCompRow.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.btnAddCompRow.Name = "vpBtn_AddCompRow";
            this.btnAddCompRow.Size = new System.Drawing.Size(96, 30);
            this.btnAddCompRow.TabIndex = 3;
            this.btnAddCompRow.Text = "添加行";
            this.btnAddCompRow.Variant = AlphaRap.FlatButtonVariant.Surface;
            // 
            // btnDelCompRow
            // 
            this.btnDelCompRow.BackColor = System.Drawing.Color.Transparent;
            this.btnDelCompRow.CornerRadius = 8;
            this.btnDelCompRow.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDelCompRow.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.btnDelCompRow.Location = new System.Drawing.Point(460, 8);
            this.btnDelCompRow.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            this.btnDelCompRow.Name = "vpBtn_DelCompRow";
            this.btnDelCompRow.Size = new System.Drawing.Size(108, 30);
            this.btnDelCompRow.TabIndex = 4;
            this.btnDelCompRow.Text = "删除行";
            this.btnDelCompRow.Variant = AlphaRap.FlatButtonVariant.Danger;
            // 
            // VpVppPage
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.compRow);
            this.Controls.Add(this.paramRow);
            this.Controls.Add(this.pathRow);
            this.Controls.Add(this.head);
            this.Name = "VpVppPage";
            this.Padding = new System.Windows.Forms.Padding(0);
            this.Size = new System.Drawing.Size(720, 464);
            this.head.ResumeLayout(false);
            this.compRow.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridComp)).EndInit();
            this.pathRow.ResumeLayout(false);
            this.pathRow.PerformLayout();
            this.optionsRow.ResumeLayout(false);
            this.optionsRow.PerformLayout();
            this.keepGroup.ResumeLayout(false);
            this.keepGroup.PerformLayout();
            this.paramRow.ResumeLayout(false);
            this.paramRow.PerformLayout();
            this.foot.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel head;
        private UiLabel lblInfo;
        private System.Windows.Forms.Panel compRow;
        private System.Windows.Forms.DataGridView gridComp;
        private System.Windows.Forms.DataGridViewTextBoxColumn cItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn cMin;
        private System.Windows.Forms.DataGridViewTextBoxColumn cMax;
        private UiLabel path;
        private System.Windows.Forms.FlowLayoutPanel pathRow;
        private UiCheckBox cbSaveImg;
        private UiLabel lblSavePath;
        private System.Windows.Forms.TextBox tbSavePath;
        private FlatButton btnBrowsePath;
        private System.Windows.Forms.FlowLayoutPanel optionsRow;
        private UiCheckBox cbOrig;
        private UiCheckBox cbSnap;
        private UiCheckBox cbAutoDel;
        private System.Windows.Forms.Panel keepGroup;
        private UiLabel lblSaveDays;
        private System.Windows.Forms.TextBox tbSaveDays;
        private System.Windows.Forms.FlowLayoutPanel paramRow;
        private UiLabel lblExp;
        private System.Windows.Forms.TextBox tbExp;
        private UiLabel lblVppName;
        private System.Windows.Forms.TextBox tbRename;
        private System.Windows.Forms.FlowLayoutPanel foot;
        private FlatButton btnEditTb;
        private FlatButton btnCapture;
        private FlatButton btnSaveVpp;
        private FlatButton btnAddCompRow;
        private FlatButton btnDelCompRow;
    }
}
