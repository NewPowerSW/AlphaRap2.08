namespace AlphaRap
{
    partial class VpCalibCard
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
            this.card = new AlphaRap.CardPanel();
            this.gridPoints = new System.Windows.Forms.DataGridView();
            this.cPx = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cPy = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cMx = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cMy = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblTip = new AlphaRap.UiLabel();
            this.expRow = new System.Windows.Forms.Panel();
            this.lblExpCaption = new AlphaRap.UiLabel();
            this.tbExposure = new System.Windows.Forms.TextBox();
            this.btns = new System.Windows.Forms.FlowLayoutPanel();
            this.btnAddCalibration = new AlphaRap.UiButton();
            this.btnEditTb = new AlphaRap.UiButton();
            this.btnCapture = new AlphaRap.UiButton();
            this.btnAddPoint = new AlphaRap.UiButton();
            this.btnDeletePoint = new AlphaRap.UiButton();
            this.btnApplyToCamera = new AlphaRap.UiButton();
            this.lblTitle = new AlphaRap.UiLabel();
            this.card.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridPoints)).BeginInit();
            this.expRow.SuspendLayout();
            this.btns.SuspendLayout();
            this.SuspendLayout();
            // 
            // card
            // 
            this.card.CornerRadius = 10;
            this.card.Controls.Add(this.gridPoints);
            this.card.Controls.Add(this.lblTip);
            this.card.Controls.Add(this.expRow);
            this.card.Controls.Add(this.btns);
            this.card.Controls.Add(this.lblTitle);
            this.card.Dock = System.Windows.Forms.DockStyle.Fill;
            this.card.Location = new System.Drawing.Point(0, 0);
            this.card.Name = "card";
            this.card.Padding = new System.Windows.Forms.Padding(14, 8, 14, 8);
            this.card.Size = new System.Drawing.Size(900, 296);
            this.card.TabIndex = 0;
            // 
            // gridPoints
            // 
            this.gridPoints.AllowUserToAddRows = false;
            this.gridPoints.AllowUserToDeleteRows = false;
            this.gridPoints.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gridPoints.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridPoints.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.cPx,
            this.cPy,
            this.cMx,
            this.cMy});
            this.gridPoints.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridPoints.Location = new System.Drawing.Point(14, 38);
            this.gridPoints.Name = "vpCalibGrid";
            this.gridPoints.RowHeadersVisible = false;
            this.gridPoints.Size = new System.Drawing.Size(872, 158);
            this.gridPoints.TabIndex = 0;
            // 
            // cPx
            // 
            this.cPx.HeaderText = "PixelX";
            this.cPx.Name = "cPx";
            this.cPx.ReadOnly = true;
            // 
            // cPy
            // 
            this.cPy.HeaderText = "PixelY";
            this.cPy.Name = "cPy";
            this.cPy.ReadOnly = true;
            // 
            // cMx
            // 
            this.cMx.HeaderText = "MotorPosX";
            this.cMx.Name = "cMx";
            // 
            // cMy
            // 
            this.cMy.HeaderText = "MotorPosY";
            this.cMy.Name = "cMy";
            // 
            // lblTip
            // 
            this.lblTip.AutoSize = false;
            this.lblTip.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblTip.Font = new System.Drawing.Font("宋体", 10.5F);
            this.lblTip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(110)))), ((int)(((byte)(120)))), ((int)(((byte)(132)))));
            this.lblTip.Location = new System.Drawing.Point(14, 196);
            this.lblTip.Name = "vpCalibTip_dyn";
            this.lblTip.Size = new System.Drawing.Size(872, 26);
            this.lblTip.TabIndex = 1;
            this.lblTip.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // expRow
            // 
            this.expRow.BackColor = System.Drawing.Color.Transparent;
            this.expRow.Controls.Add(this.lblExpCaption);
            this.expRow.Controls.Add(this.tbExposure);
            this.expRow.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.expRow.Location = new System.Drawing.Point(14, 222);
            this.expRow.Name = "vpCalibExpRow";
            this.expRow.Size = new System.Drawing.Size(872, 34);
            this.expRow.TabIndex = 2;
            // 
            // lblExpCaption
            // 
            this.lblExpCaption.AutoSize = false;
            this.lblExpCaption.Font = new System.Drawing.Font("宋体", 11.25F);
            this.lblExpCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(92)))), ((int)(((byte)(106)))));
            this.lblExpCaption.Location = new System.Drawing.Point(0, 3);
            this.lblExpCaption.Name = "vpRow_CalibExposure";
            this.lblExpCaption.Size = new System.Drawing.Size(76, 28);
            this.lblExpCaption.TabIndex = 0;
            this.lblExpCaption.Text = "标定曝光";
            this.lblExpCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbExposure
            // 
            this.tbExposure.Font = new System.Drawing.Font("宋体", 11.25F);
            this.tbExposure.Location = new System.Drawing.Point(80, 4);
            this.tbExposure.Name = "vpCalibExpBox";
            this.tbExposure.Size = new System.Drawing.Size(86, 25);
            this.tbExposure.TabIndex = 1;
            // 
            // btns
            // 
            this.btns.AutoSize = false;
            this.btns.BackColor = System.Drawing.Color.Transparent;
            this.btns.Controls.Add(this.btnAddCalibration);
            this.btns.Controls.Add(this.btnEditTb);
            this.btns.Controls.Add(this.btnCapture);
            this.btns.Controls.Add(this.btnAddPoint);
            this.btns.Controls.Add(this.btnDeletePoint);
            this.btns.Controls.Add(this.btnApplyToCamera);
            this.btns.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btns.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.btns.Location = new System.Drawing.Point(14, 256);
            this.btns.Name = "vpCalibBtns";
            this.btns.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.btns.Size = new System.Drawing.Size(872, 40);
            this.btns.TabIndex = 3;
            this.btns.WrapContents = false;
            // 
            // btnAddCalibration
            // 
            this.btnAddCalibration.BackColor = System.Drawing.Color.White;
            this.btnAddCalibration.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddCalibration.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(216)))), ((int)(((byte)(230)))));
            this.btnAddCalibration.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddCalibration.Font = new System.Drawing.Font("宋体", 11.25F);
            this.btnAddCalibration.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.btnAddCalibration.Location = new System.Drawing.Point(0, 5);
            this.btnAddCalibration.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.btnAddCalibration.Name = "vpBtn_AddCalibration";
            this.btnAddCalibration.Size = new System.Drawing.Size(112, 30);
            this.btnAddCalibration.TabIndex = 0;
            this.btnAddCalibration.Text = "添加标定";
            // 
            // btnEditTb
            // 
            this.btnEditTb.BackColor = System.Drawing.Color.White;
            this.btnEditTb.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEditTb.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(216)))), ((int)(((byte)(230)))));
            this.btnEditTb.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEditTb.Font = new System.Drawing.Font("宋体", 11.25F);
            this.btnEditTb.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.btnEditTb.Location = new System.Drawing.Point(120, 5);
            this.btnEditTb.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.btnEditTb.Name = "vpBtn_EditTb";
            this.btnEditTb.Size = new System.Drawing.Size(112, 30);
            this.btnEditTb.TabIndex = 1;
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
            this.btnCapture.Location = new System.Drawing.Point(240, 5);
            this.btnCapture.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.btnCapture.Name = "vpBtn_Capture";
            this.btnCapture.Size = new System.Drawing.Size(112, 30);
            this.btnCapture.TabIndex = 2;
            this.btnCapture.Text = "拍照";
            // 
            // btnAddPoint
            // 
            this.btnAddPoint.BackColor = System.Drawing.Color.White;
            this.btnAddPoint.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddPoint.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(216)))), ((int)(((byte)(230)))));
            this.btnAddPoint.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddPoint.Font = new System.Drawing.Font("宋体", 11.25F);
            this.btnAddPoint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.btnAddPoint.Location = new System.Drawing.Point(360, 5);
            this.btnAddPoint.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.btnAddPoint.Name = "vpBtn_AddPoint";
            this.btnAddPoint.Size = new System.Drawing.Size(112, 30);
            this.btnAddPoint.TabIndex = 3;
            this.btnAddPoint.Text = "添加标定点";
            // 
            // btnDeletePoint
            // 
            this.btnDeletePoint.BackColor = System.Drawing.Color.White;
            this.btnDeletePoint.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDeletePoint.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(216)))), ((int)(((byte)(230)))));
            this.btnDeletePoint.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeletePoint.Font = new System.Drawing.Font("宋体", 11.25F);
            this.btnDeletePoint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.btnDeletePoint.Location = new System.Drawing.Point(480, 5);
            this.btnDeletePoint.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.btnDeletePoint.Name = "vpBtn_DeletePoint";
            this.btnDeletePoint.Size = new System.Drawing.Size(112, 30);
            this.btnDeletePoint.TabIndex = 4;
            this.btnDeletePoint.Text = "删除选中点";
            // 
            // btnApplyToCamera
            // 
            this.btnApplyToCamera.BackColor = System.Drawing.Color.White;
            this.btnApplyToCamera.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnApplyToCamera.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(216)))), ((int)(((byte)(230)))));
            this.btnApplyToCamera.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnApplyToCamera.Font = new System.Drawing.Font("宋体", 11.25F);
            this.btnApplyToCamera.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.btnApplyToCamera.Location = new System.Drawing.Point(600, 5);
            this.btnApplyToCamera.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.btnApplyToCamera.Name = "vpBtn_ApplyToCamera";
            this.btnApplyToCamera.Size = new System.Drawing.Size(112, 30);
            this.btnApplyToCamera.TabIndex = 5;
            this.btnApplyToCamera.Text = "应用到本相机";
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = false;
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblTitle.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.lblTitle.Location = new System.Drawing.Point(14, 8);
            this.lblTitle.Name = "vpCalibTitle_dyn";
            this.lblTitle.Size = new System.Drawing.Size(872, 30);
            this.lblTitle.TabIndex = 4;
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // VpCalibCard
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(243)))), ((int)(((byte)(249)))));
            this.Controls.Add(this.card);
            this.Name = "VpCalibCard";
            this.Size = new System.Drawing.Size(900, 296);
            this.card.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridPoints)).EndInit();
            this.expRow.ResumeLayout(false);
            this.expRow.PerformLayout();
            this.btns.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private CardPanel card;
        private System.Windows.Forms.DataGridView gridPoints;
        private System.Windows.Forms.DataGridViewTextBoxColumn cPx;
        private System.Windows.Forms.DataGridViewTextBoxColumn cPy;
        private System.Windows.Forms.DataGridViewTextBoxColumn cMx;
        private System.Windows.Forms.DataGridViewTextBoxColumn cMy;
        private UiLabel lblTip;
        private System.Windows.Forms.Panel expRow;
        private UiLabel lblExpCaption;
        private System.Windows.Forms.TextBox tbExposure;
        private System.Windows.Forms.FlowLayoutPanel btns;
        private UiButton btnAddCalibration;
        private UiButton btnEditTb;
        private UiButton btnCapture;
        private UiButton btnAddPoint;
        private UiButton btnDeletePoint;
        private UiButton btnApplyToCamera;
        private UiLabel lblTitle;
    }
}
