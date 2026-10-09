namespace AlphaRap
{
    partial class VpVppPage
    {
        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.head = new System.Windows.Forms.Panel();
            this.lblInfo = new AlphaRap.UiLabel();
            this.band = new System.Windows.Forms.Panel();
            this.path = new AlphaRap.UiLabel();
            this.compRow = new System.Windows.Forms.Panel();
            this.gridComp = new System.Windows.Forms.DataGridView();
            this.cItem = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cMin = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cMax = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.saveRow = new System.Windows.Forms.Panel();
            this.cbSaveImg = new AlphaRap.UiCheckBox();
            this.lblSavePath = new AlphaRap.UiLabel();
            this.tbSavePath = new System.Windows.Forms.TextBox();
            this.cbOrig = new AlphaRap.UiCheckBox();
            this.cbSnap = new AlphaRap.UiCheckBox();
            this.cbAutoDel = new AlphaRap.UiCheckBox();
            this.lblSaveDays = new AlphaRap.UiLabel();
            this.tbSaveDays = new System.Windows.Forms.TextBox();
            this.paramRow = new System.Windows.Forms.Panel();
            this.lblExp = new AlphaRap.UiLabel();
            this.tbExp = new System.Windows.Forms.TextBox();
            this.lblVppName = new AlphaRap.UiLabel();
            this.tbRename = new System.Windows.Forms.TextBox();
            this.foot = new System.Windows.Forms.FlowLayoutPanel();
            this.btnEditTb = new AlphaRap.UiButton();
            this.btnCapture = new AlphaRap.UiButton();
            this.btnSaveVpp = new AlphaRap.UiButton();
            this.btnAddCompRow = new AlphaRap.UiButton();
            this.btnDelCompRow = new AlphaRap.UiButton();
            this.head.SuspendLayout();
            this.band.SuspendLayout();
            this.compRow.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridComp)).BeginInit();
            this.saveRow.SuspendLayout();
            this.paramRow.SuspendLayout();
            this.foot.SuspendLayout();
            this.SuspendLayout();
            // 
            // head（归属信息行：相机 / CameraIndex / VPP 名）
            // 
            this.head.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(243)))), ((int)(((byte)(249)))));
            this.head.Controls.Add(this.lblInfo);
            this.head.Dock = System.Windows.Forms.DockStyle.Top;
            this.head.Location = new System.Drawing.Point(0, 0);
            this.head.Name = "head";
            this.head.Size = new System.Drawing.Size(900, 30);
            this.head.TabIndex = 0;
            // 
            // lblInfo
            // 
            this.lblInfo.AutoSize = false;
            this.lblInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblInfo.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold);
            this.lblInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.lblInfo.Location = new System.Drawing.Point(0, 0);
            this.lblInfo.Name = "vpVppInfo_dyn";
            this.lblInfo.Size = new System.Drawing.Size(900, 30);
            this.lblInfo.TabIndex = 0;
            this.lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // band（底部这一条：补偿限制表 / 路径 / 存图设置 / 参数 / 按钮）
            // 「后加入的先停靠」⇒ 路径写在表格之后，视觉上才会落在表格下方，
            // 与标定卡（标题 → 表格 → 路径 → 曝光 → 按钮）的行序保持一致。
            this.band.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(243)))), ((int)(((byte)(249)))));
            this.band.Controls.Add(this.compRow);
            this.band.Controls.Add(this.path);
            this.band.Controls.Add(this.saveRow);
            this.band.Controls.Add(this.paramRow);
            this.band.Controls.Add(this.foot);
            this.band.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.band.Location = new System.Drawing.Point(0, 34);
            this.band.Name = "band";
            this.band.Size = new System.Drawing.Size(900, 266);
            this.band.TabIndex = 1;
            // 
            // path（vpp 路径行）
            // 
            this.path.AutoSize = false;
            this.path.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.path.Font = new System.Drawing.Font("宋体", 10.5F);
            this.path.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(120)))), ((int)(((byte)(132)))));
            this.path.Location = new System.Drawing.Point(0, 0);
            this.path.Name = "vpVppPath_dyn";
            this.path.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.path.Size = new System.Drawing.Size(900, 26);
            this.path.TabIndex = 4;
            this.path.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // compRow（补偿限制表：项目 / 补偿下限 / 补偿上限，行由用户增删）
            // 
            this.compRow.BackColor = System.Drawing.Color.Transparent;
            this.compRow.Controls.Add(this.gridComp);
            this.compRow.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.compRow.Location = new System.Drawing.Point(0, 26);
            this.compRow.Name = "compRow";
            this.compRow.Padding = new System.Windows.Forms.Padding(10, 2, 10, 2);
            this.compRow.Size = new System.Drawing.Size(900, 132);
            this.compRow.TabIndex = 3;
            // 
            // gridComp
            // 
            this.gridComp.AllowUserToAddRows = false;
            this.gridComp.AllowUserToDeleteRows = false;
            this.gridComp.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gridComp.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridComp.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.cItem,
            this.cMin,
            this.cMax});
            this.gridComp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridComp.Location = new System.Drawing.Point(10, 2);
            this.gridComp.Name = "vpCompGrid";
            this.gridComp.RowHeadersVisible = false;
            this.gridComp.Size = new System.Drawing.Size(880, 128);
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
            // saveRow（存图设置：勾选 + 路径 + 原图/截图/自动删除 + 保留天数）
            // 
            this.saveRow.BackColor = System.Drawing.Color.Transparent;
            this.saveRow.Controls.Add(this.cbSaveImg);
            this.saveRow.Controls.Add(this.lblSavePath);
            this.saveRow.Controls.Add(this.tbSavePath);
            this.saveRow.Controls.Add(this.cbOrig);
            this.saveRow.Controls.Add(this.cbSnap);
            this.saveRow.Controls.Add(this.cbAutoDel);
            this.saveRow.Controls.Add(this.lblSaveDays);
            this.saveRow.Controls.Add(this.tbSaveDays);
            this.saveRow.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.saveRow.Location = new System.Drawing.Point(0, 158);
            this.saveRow.Name = "saveRow";
            this.saveRow.Size = new System.Drawing.Size(900, 34);
            this.saveRow.TabIndex = 2;
            // 
            // cbSaveImg
            // 
            this.cbSaveImg.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cbSaveImg.Font = new System.Drawing.Font("宋体", 11.25F);
            this.cbSaveImg.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(92)))), ((int)(((byte)(106)))));
            this.cbSaveImg.Location = new System.Drawing.Point(10, 5);
            this.cbSaveImg.Name = "vpRow_SaveImage";
            this.cbSaveImg.Size = new System.Drawing.Size(72, 24);
            this.cbSaveImg.TabIndex = 0;
            this.cbSaveImg.Text = "存图";
            // 
            // lblSavePath
            // 
            this.lblSavePath.AutoSize = false;
            this.lblSavePath.Font = new System.Drawing.Font("宋体", 11.25F);
            this.lblSavePath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(92)))), ((int)(((byte)(106)))));
            this.lblSavePath.Location = new System.Drawing.Point(108, 3);
            this.lblSavePath.Name = "vpRow_SavePath";
            this.lblSavePath.Size = new System.Drawing.Size(76, 28);
            this.lblSavePath.TabIndex = 1;
            this.lblSavePath.Text = "存图路径";
            this.lblSavePath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbSavePath
            // 
            this.tbSavePath.Font = new System.Drawing.Font("宋体", 11.25F);
            this.tbSavePath.Location = new System.Drawing.Point(188, 4);
            this.tbSavePath.Name = "vpSavePathBox";
            this.tbSavePath.Size = new System.Drawing.Size(200, 25);
            this.tbSavePath.TabIndex = 2;
            // 
            // cbOrig
            // 
            this.cbOrig.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cbOrig.Font = new System.Drawing.Font("宋体", 11.25F);
            this.cbOrig.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(92)))), ((int)(((byte)(106)))));
            this.cbOrig.Location = new System.Drawing.Point(414, 5);
            this.cbOrig.Name = "vpRow_SaveOriginal";
            this.cbOrig.Size = new System.Drawing.Size(66, 24);
            this.cbOrig.TabIndex = 3;
            this.cbOrig.Text = "原图";
            // 
            // cbSnap
            // 
            this.cbSnap.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cbSnap.Font = new System.Drawing.Font("宋体", 11.25F);
            this.cbSnap.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(92)))), ((int)(((byte)(106)))));
            this.cbSnap.Location = new System.Drawing.Point(506, 5);
            this.cbSnap.Name = "vpRow_SaveSnapshot";
            this.cbSnap.Size = new System.Drawing.Size(66, 24);
            this.cbSnap.TabIndex = 4;
            this.cbSnap.Text = "截图";
            // 
            // cbAutoDel
            // 
            this.cbAutoDel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cbAutoDel.Font = new System.Drawing.Font("宋体", 11.25F);
            this.cbAutoDel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(92)))), ((int)(((byte)(106)))));
            this.cbAutoDel.Location = new System.Drawing.Point(598, 5);
            this.cbAutoDel.Name = "vpRow_SaveAutoDel";
            this.cbAutoDel.Size = new System.Drawing.Size(100, 24);
            this.cbAutoDel.TabIndex = 5;
            this.cbAutoDel.Text = "自动删除";
            // 
            // lblSaveDays
            // 
            this.lblSaveDays.AutoSize = false;
            this.lblSaveDays.Font = new System.Drawing.Font("宋体", 11.25F);
            this.lblSaveDays.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(92)))), ((int)(((byte)(106)))));
            this.lblSaveDays.Location = new System.Drawing.Point(724, 3);
            this.lblSaveDays.Name = "vpRow_SaveDays";
            this.lblSaveDays.Size = new System.Drawing.Size(76, 28);
            this.lblSaveDays.TabIndex = 6;
            this.lblSaveDays.Text = "保留天数";
            this.lblSaveDays.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbSaveDays
            // 
            this.tbSaveDays.Font = new System.Drawing.Font("宋体", 11.25F);
            this.tbSaveDays.Location = new System.Drawing.Point(804, 4);
            this.tbSaveDays.Name = "vpSaveDaysBox";
            this.tbSaveDays.Size = new System.Drawing.Size(86, 25);
            this.tbSaveDays.TabIndex = 7;
            // 
            // paramRow（本 VPP 自己的参数：曝光 / VPP 名称）
            // 
            this.paramRow.BackColor = System.Drawing.Color.Transparent;
            this.paramRow.Controls.Add(this.lblExp);
            this.paramRow.Controls.Add(this.tbExp);
            this.paramRow.Controls.Add(this.lblVppName);
            this.paramRow.Controls.Add(this.tbRename);
            this.paramRow.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.paramRow.Location = new System.Drawing.Point(0, 192);
            this.paramRow.Name = "paramRow";
            this.paramRow.Size = new System.Drawing.Size(900, 34);
            this.paramRow.TabIndex = 1;
            // 
            // lblExp
            // 
            this.lblExp.AutoSize = false;
            this.lblExp.Font = new System.Drawing.Font("宋体", 11.25F);
            this.lblExp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(92)))), ((int)(((byte)(106)))));
            this.lblExp.Location = new System.Drawing.Point(10, 3);
            this.lblExp.Name = "vpRow_Exposure";
            this.lblExp.Size = new System.Drawing.Size(76, 28);
            this.lblExp.TabIndex = 0;
            this.lblExp.Text = "曝光";
            this.lblExp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbExp
            // 
            this.tbExp.Font = new System.Drawing.Font("宋体", 11.25F);
            this.tbExp.Location = new System.Drawing.Point(90, 4);
            this.tbExp.Name = "vpVppExpBox";
            this.tbExp.Size = new System.Drawing.Size(86, 25);
            this.tbExp.TabIndex = 1;
            // 
            // lblVppName
            // 
            this.lblVppName.AutoSize = false;
            this.lblVppName.Font = new System.Drawing.Font("宋体", 11.25F);
            this.lblVppName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(92)))), ((int)(((byte)(106)))));
            this.lblVppName.Location = new System.Drawing.Point(202, 3);
            this.lblVppName.Name = "vpRow_VppName";
            this.lblVppName.Size = new System.Drawing.Size(76, 28);
            this.lblVppName.TabIndex = 2;
            this.lblVppName.Text = "VPP 名称";
            this.lblVppName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbRename
            // 
            this.tbRename.Font = new System.Drawing.Font("宋体", 11.25F);
            this.tbRename.Location = new System.Drawing.Point(282, 4);
            this.tbRename.Name = "vpRenameBox";
            this.tbRename.Size = new System.Drawing.Size(130, 25);
            this.tbRename.TabIndex = 3;
            // 
            // foot（操作按钮：编辑 TB / 拍照 / 保存 VPP / 添加行 / 删除行）
            // 
            this.foot.AutoSize = false;
            this.foot.BackColor = System.Drawing.Color.Transparent;
            this.foot.Controls.Add(this.btnEditTb);
            this.foot.Controls.Add(this.btnCapture);
            this.foot.Controls.Add(this.btnSaveVpp);
            this.foot.Controls.Add(this.btnAddCompRow);
            this.foot.Controls.Add(this.btnDelCompRow);
            this.foot.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.foot.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.foot.Location = new System.Drawing.Point(0, 226);
            this.foot.Name = "foot";
            this.foot.Padding = new System.Windows.Forms.Padding(10, 5, 0, 0);
            this.foot.Size = new System.Drawing.Size(900, 40);
            this.foot.TabIndex = 0;
            this.foot.WrapContents = false;
            // 
            // btnEditTb
            // 
            this.btnEditTb.BackColor = System.Drawing.Color.White;
            this.btnEditTb.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEditTb.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(216)))), ((int)(((byte)(230)))));
            this.btnEditTb.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEditTb.Font = new System.Drawing.Font("宋体", 11.25F);
            this.btnEditTb.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.btnEditTb.Location = new System.Drawing.Point(10, 5);
            this.btnEditTb.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.btnEditTb.Name = "vpBtn_EditTb";
            this.btnEditTb.Size = new System.Drawing.Size(112, 30);
            this.btnEditTb.TabIndex = 0;
            this.btnEditTb.Text = "编辑 TB";
            // 
            // btnCapture
            // 
            this.btnCapture.BackColor = System.Drawing.Color.White;
            this.btnCapture.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCapture.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(216)))), ((int)(((byte)(230)))));
            this.btnCapture.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCapture.Font = new System.Drawing.Font("宋体", 11.25F);
            this.btnCapture.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.btnCapture.Location = new System.Drawing.Point(130, 5);
            this.btnCapture.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.btnCapture.Name = "vpBtn_Capture";
            this.btnCapture.Size = new System.Drawing.Size(112, 30);
            this.btnCapture.TabIndex = 1;
            this.btnCapture.Text = "拍照";
            // 
            // btnSaveVpp
            // 
            this.btnSaveVpp.BackColor = System.Drawing.Color.White;
            this.btnSaveVpp.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSaveVpp.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(216)))), ((int)(((byte)(230)))));
            this.btnSaveVpp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaveVpp.Font = new System.Drawing.Font("宋体", 11.25F);
            this.btnSaveVpp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.btnSaveVpp.Location = new System.Drawing.Point(250, 5);
            this.btnSaveVpp.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.btnSaveVpp.Name = "vpBtn_SaveVpp";
            this.btnSaveVpp.Size = new System.Drawing.Size(112, 30);
            this.btnSaveVpp.TabIndex = 2;
            this.btnSaveVpp.Text = "保存 VPP";
            // 
            // btnAddCompRow
            // 
            this.btnAddCompRow.BackColor = System.Drawing.Color.White;
            this.btnAddCompRow.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddCompRow.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(216)))), ((int)(((byte)(230)))));
            this.btnAddCompRow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddCompRow.Font = new System.Drawing.Font("宋体", 11.25F);
            this.btnAddCompRow.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.btnAddCompRow.Location = new System.Drawing.Point(370, 5);
            this.btnAddCompRow.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.btnAddCompRow.Name = "vpBtn_AddCompRow";
            this.btnAddCompRow.Size = new System.Drawing.Size(112, 30);
            this.btnAddCompRow.TabIndex = 3;
            this.btnAddCompRow.Text = "添加行";
            // 
            // btnDelCompRow
            // 
            this.btnDelCompRow.BackColor = System.Drawing.Color.White;
            this.btnDelCompRow.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDelCompRow.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(216)))), ((int)(((byte)(230)))));
            this.btnDelCompRow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelCompRow.Font = new System.Drawing.Font("宋体", 11.25F);
            this.btnDelCompRow.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.btnDelCompRow.Location = new System.Drawing.Point(490, 5);
            this.btnDelCompRow.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.btnDelCompRow.Name = "vpBtn_DelCompRow";
            this.btnDelCompRow.Size = new System.Drawing.Size(112, 30);
            this.btnDelCompRow.TabIndex = 4;
            this.btnDelCompRow.Text = "删除行";
            // 
            // VpVppPage
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(243)))), ((int)(((byte)(249)))));
            this.Controls.Add(this.head);
            this.Controls.Add(this.band);
            this.Name = "VpVppPage";
            this.Size = new System.Drawing.Size(900, 300);
            this.head.ResumeLayout(false);
            this.band.ResumeLayout(false);
            this.compRow.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridComp)).EndInit();
            this.saveRow.ResumeLayout(false);
            this.saveRow.PerformLayout();
            this.paramRow.ResumeLayout(false);
            this.paramRow.PerformLayout();
            this.foot.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel head;
        private UiLabel lblInfo;
        private System.Windows.Forms.Panel band;
        private UiLabel path;
        private System.Windows.Forms.Panel compRow;
        private System.Windows.Forms.DataGridView gridComp;
        private System.Windows.Forms.DataGridViewTextBoxColumn cItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn cMin;
        private System.Windows.Forms.DataGridViewTextBoxColumn cMax;
        private System.Windows.Forms.Panel saveRow;
        private UiCheckBox cbSaveImg;
        private UiLabel lblSavePath;
        private System.Windows.Forms.TextBox tbSavePath;
        private UiCheckBox cbOrig;
        private UiCheckBox cbSnap;
        private UiCheckBox cbAutoDel;
        private UiLabel lblSaveDays;
        private System.Windows.Forms.TextBox tbSaveDays;
        private System.Windows.Forms.Panel paramRow;
        private UiLabel lblExp;
        private System.Windows.Forms.TextBox tbExp;
        private UiLabel lblVppName;
        private System.Windows.Forms.TextBox tbRename;
        private System.Windows.Forms.FlowLayoutPanel foot;
        private UiButton btnEditTb;
        private UiButton btnCapture;
        private UiButton btnSaveVpp;
        private UiButton btnAddCompRow;
        private UiButton btnDelCompRow;
    }
}
