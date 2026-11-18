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
            if (disposing) UnbindCapture();
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
            this.lblFeature = new AlphaRap.UiLabel();
            this.cmbFeature = new System.Windows.Forms.ComboBox();
            this.btnSetup = new AlphaRap.UiButton();
            this.coordinateRow = new System.Windows.Forms.FlowLayoutPanel();
            this.lblMotorX = new AlphaRap.UiLabel();
            this.lblMotorY = new AlphaRap.UiLabel();
            this.tbMotorX = new System.Windows.Forms.TextBox();
            this.tbMotorY = new System.Windows.Forms.TextBox();
            this.help = new System.Windows.Forms.ToolTip(this.components = new System.ComponentModel.Container());
            this.btnClearSamples = new AlphaRap.UiButton();
            this.btns = new System.Windows.Forms.FlowLayoutPanel();
            this.btnAddCalibration = new AlphaRap.UiButton();
            this.btnEditTb = new AlphaRap.UiButton();
            this.btnCapture = new AlphaRap.UiButton();
            this.btnAddPoint = new AlphaRap.UiButton();
            this.btnDeletePoint = new AlphaRap.UiButton();
            this.btnApplyToCamera = new AlphaRap.UiButton();
            this.lblTitle = new AlphaRap.UiLabel();
            this.stepRow = new System.Windows.Forms.Panel();
            this.lblNineStep = new AlphaRap.UiLabel();
            this.btnRotationStep = new AlphaRap.UiButton();
            this.lblStepState = new AlphaRap.UiLabel();
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
            this.card.Controls.Add(this.btns);
            this.card.Controls.Add(this.coordinateRow);
            this.card.Controls.Add(this.expRow);
            this.card.Controls.Add(this.stepRow);
            this.card.Controls.Add(this.lblTitle);
            this.card.Dock = System.Windows.Forms.DockStyle.Fill;
            this.card.Location = new System.Drawing.Point(0, 0);
            this.card.Name = "card";
            this.card.Padding = new System.Windows.Forms.Padding(14, 8, 14, 8);
            this.card.Size = new System.Drawing.Size(900, 364);
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
            this.gridPoints.Location = new System.Drawing.Point(14, 140);
            this.gridPoints.Name = "vpCalibGrid";
            this.gridPoints.RowHeadersVisible = false;
            this.gridPoints.Size = new System.Drawing.Size(872, 134);
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
            this.lblTip.Location = new System.Drawing.Point(14, 274);
            this.lblTip.Name = "vpCalibTip_dyn";
            this.lblTip.Size = new System.Drawing.Size(872, 42);
            this.lblTip.AutoEllipsis = true;
            this.lblTip.TabIndex = 1;
            this.lblTip.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // expRow
            // 
            this.expRow.BackColor = System.Drawing.Color.Transparent;
            this.expRow.Controls.Add(this.lblExpCaption);
            this.expRow.Controls.Add(this.tbExposure);
            this.expRow.Controls.Add(this.lblFeature);
            this.expRow.Controls.Add(this.cmbFeature);
            this.expRow.Controls.Add(this.btnSetup);
            this.expRow.Dock = System.Windows.Forms.DockStyle.Top;
            this.expRow.Location = new System.Drawing.Point(14, 70);
            this.expRow.Name = "vpCalibExpRow";
            this.expRow.Size = new System.Drawing.Size(872, 34);
            this.expRow.TabIndex = 2;
            // 
            // lblExpCaption
            // 
            this.lblExpCaption.AutoSize = false;
            this.lblExpCaption.Font = new System.Drawing.Font("宋体", 9F);
            this.lblExpCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(92)))), ((int)(((byte)(106)))));
            this.lblExpCaption.Location = new System.Drawing.Point(0, 3);
            this.lblExpCaption.Name = "vpRow_CalibExposure";
            this.lblExpCaption.Size = new System.Drawing.Size(90, 28);
            this.lblExpCaption.TabIndex = 0;
            this.lblExpCaption.Text = "标定曝光";
            this.lblExpCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbExposure
            // 
            this.tbExposure.Font = new System.Drawing.Font("宋体", 11.25F);
            this.tbExposure.Location = new System.Drawing.Point(94, 4);
            this.tbExposure.Name = "vpCalibExpBox";
            this.tbExposure.Size = new System.Drawing.Size(72, 25);
            this.tbExposure.TabIndex = 1;
            //
            // lblFeature
            //
            this.lblFeature.AutoSize = false;
            this.lblFeature.Location = new System.Drawing.Point(168, 3);
            this.lblFeature.Size = new System.Drawing.Size(70, 28);
            this.lblFeature.Font = new System.Drawing.Font("宋体", 10F);
            this.lblFeature.Name = "vpRow_CalibFeature";
            this.lblFeature.Text = "采点方式";
            this.lblFeature.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // cmbFeature
            //
            this.cmbFeature.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFeature.Font = new System.Drawing.Font("宋体", 10F);
            this.cmbFeature.Location = new System.Drawing.Point(240, 4);
            this.cmbFeature.Size = new System.Drawing.Size(155, 25);
            this.cmbFeature.Name = "vpCalibFeature_dyn";
            this.cmbFeature.Items.AddRange(new object[] { "圆心", "两线交点", "模板定位（兼容）" });
            this.cmbFeature.SelectedIndex = 0;
            this.cmbFeature.TabIndex = 2;
            //
            // Feature setup
            //
            this.btnSetup.Font = new System.Drawing.Font("宋体", 9F);
            this.btnSetup.Location = new System.Drawing.Point(403, 2);
            this.btnSetup.Size = new System.Drawing.Size(120, 29);
            this.btnSetup.Name = "vpGuide_Edges";
            this.btnSetup.Text = "设置找边";
            this.btnSetup.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSetup.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 216, 230);
            this.btnSetup.BackColor = System.Drawing.Color.White;
            this.coordinateRow.Dock = System.Windows.Forms.DockStyle.Top;
            this.coordinateRow.Height = 36;
            this.coordinateRow.WrapContents = false;
            this.coordinateRow.Controls.Add(this.lblMotorX);
            this.coordinateRow.Controls.Add(this.tbMotorX);
            this.coordinateRow.Controls.Add(this.lblMotorY);
            this.coordinateRow.Controls.Add(this.tbMotorY);
            this.lblMotorX.Name = "vpGuide_MotorX";
            this.lblMotorX.Text = "实际机械 X (mm)";
            this.lblMotorX.Size = new System.Drawing.Size(140, 28);
            this.lblMotorX.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.tbMotorX.Name = "vpCalibMotorX";
            this.tbMotorX.Width = 100;
            this.lblMotorY.Name = "vpGuide_MotorY";
            this.lblMotorY.Text = "实际机械 Y (mm)";
            this.lblMotorY.Size = new System.Drawing.Size(140, 28);
            this.lblMotorY.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.tbMotorY.Name = "vpCalibMotorY";
            this.tbMotorY.Width = 100;
            //
            // btnClearSamples
            //
            this.btnClearSamples.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.btnClearSamples.Size = new System.Drawing.Size(88, 32);
            this.btnClearSamples.Name = "vpBtn_ClearCalibSamples";
            this.btnClearSamples.Font = new System.Drawing.Font("宋体", 10F);
            this.btnClearSamples.BackColor = System.Drawing.Color.White;
            this.btnClearSamples.ForeColor = System.Drawing.Color.FromArgb(38, 50, 64);
            this.btnClearSamples.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClearSamples.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 216, 230);
            this.btnClearSamples.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClearSamples.Text = "清空采样";
            this.btnClearSamples.TabIndex = 4;
            // 
            // btns
            // 
            this.btns.AutoSize = false;
            this.btns.BackColor = System.Drawing.Color.Transparent;
            this.btns.Controls.Add(this.btnAddCalibration);
            this.btns.Controls.Add(this.btnCapture);
            this.btns.Controls.Add(this.btnAddPoint);
            this.btns.Controls.Add(this.btnApplyToCamera);
            this.btns.Controls.Add(this.btnDeletePoint);
            this.btns.Controls.Add(this.btnClearSamples);
            this.btns.Controls.Add(this.btnEditTb);
            this.btns.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btns.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.btns.Location = new System.Drawing.Point(14, 316);
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
            this.btnAddCalibration.Font = new System.Drawing.Font("宋体", 9F);
            this.btnAddCalibration.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.btnAddCalibration.Location = new System.Drawing.Point(0, 5);
            this.btnAddCalibration.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.btnAddCalibration.Name = "vpBtn_AddCalibration";
            this.btnAddCalibration.Size = new System.Drawing.Size(88, 32);
            this.btnAddCalibration.TabIndex = 0;
            this.btnAddCalibration.Text = "添加标定";
            // 
            // btnEditTb
            // 
            this.btnEditTb.BackColor = System.Drawing.Color.White;
            this.btnEditTb.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEditTb.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(216)))), ((int)(((byte)(230)))));
            this.btnEditTb.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEditTb.Font = new System.Drawing.Font("宋体", 9F);
            this.btnEditTb.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.btnEditTb.Location = new System.Drawing.Point(120, 5);
            this.btnEditTb.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.btnEditTb.Name = "vpBtn_EditTb";
            this.btnEditTb.Size = new System.Drawing.Size(88, 32);
            this.btnEditTb.TabIndex = 1;
            this.btnEditTb.Text = "高级 TB";
            // 
            // btnCapture
            // 
            this.btnCapture.BackColor = System.Drawing.Color.White;
            this.btnCapture.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCapture.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(216)))), ((int)(((byte)(230)))));
            this.btnCapture.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCapture.Font = new System.Drawing.Font("宋体", 9F);
            this.btnCapture.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.btnCapture.Location = new System.Drawing.Point(240, 5);
            this.btnCapture.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.btnCapture.Name = "vpBtn_Capture";
            this.btnCapture.Size = new System.Drawing.Size(88, 32);
            this.btnCapture.TabIndex = 2;
            this.btnCapture.Text = "试拍";
            // 
            // btnAddPoint
            // 
            this.btnAddPoint.BackColor = System.Drawing.Color.FromArgb(225, 239, 255);
            this.btnAddPoint.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddPoint.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(216)))), ((int)(((byte)(230)))));
            this.btnAddPoint.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddPoint.Font = new System.Drawing.Font("宋体", 9F);
            this.btnAddPoint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.btnAddPoint.Location = new System.Drawing.Point(360, 5);
            this.btnAddPoint.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.btnAddPoint.Name = "vpBtn_AddPoint";
            this.btnAddPoint.Size = new System.Drawing.Size(88, 32);
            this.btnAddPoint.TabIndex = 3;
            this.btnAddPoint.Text = "采集此点";
            // 
            // btnDeletePoint
            // 
            this.btnDeletePoint.BackColor = System.Drawing.Color.White;
            this.btnDeletePoint.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDeletePoint.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(216)))), ((int)(((byte)(230)))));
            this.btnDeletePoint.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeletePoint.Font = new System.Drawing.Font("宋体", 9F);
            this.btnDeletePoint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.btnDeletePoint.Location = new System.Drawing.Point(480, 5);
            this.btnDeletePoint.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.btnDeletePoint.Name = "vpBtn_DeletePoint";
            this.btnDeletePoint.Size = new System.Drawing.Size(88, 32);
            this.btnDeletePoint.TabIndex = 4;
            this.btnDeletePoint.Text = "删除选中点";
            // 
            // btnApplyToCamera
            // 
            this.btnApplyToCamera.BackColor = System.Drawing.Color.White;
            this.btnApplyToCamera.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnApplyToCamera.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(216)))), ((int)(((byte)(230)))));
            this.btnApplyToCamera.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnApplyToCamera.Font = new System.Drawing.Font("宋体", 9F);
            this.btnApplyToCamera.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.btnApplyToCamera.Location = new System.Drawing.Point(600, 5);
            this.btnApplyToCamera.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.btnApplyToCamera.Name = "vpBtn_ApplyToCamera";
            this.btnApplyToCamera.Size = new System.Drawing.Size(88, 32);
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
            // stepRow
            //
            this.stepRow.Dock = System.Windows.Forms.DockStyle.Top;
            this.stepRow.Name = "vpCalibStepRow";
            this.stepRow.Location = new System.Drawing.Point(14, 38);
            this.stepRow.Size = new System.Drawing.Size(872, 32);
            this.stepRow.Controls.Add(this.lblNineStep);
            this.stepRow.Controls.Add(this.btnRotationStep);
            this.stepRow.Controls.Add(this.lblStepState);
            this.lblNineStep.Name = "vpStep_Nine";
            this.lblNineStep.Text = "1 九点标定";
            this.lblNineStep.Font = new System.Drawing.Font("宋体", 10F);
            this.lblNineStep.Location = new System.Drawing.Point(0, 2);
            this.lblNineStep.Size = new System.Drawing.Size(172, 28);
            this.lblNineStep.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnRotationStep.Name = "vpStep_Rotation";
            this.btnRotationStep.Text = "2 旋转标定（可选）";
            this.btnRotationStep.Font = new System.Drawing.Font("宋体", 9F);
            this.btnRotationStep.Location = new System.Drawing.Point(180, 1);
            this.btnRotationStep.Size = new System.Drawing.Size(190, 28);
            this.btnRotationStep.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRotationStep.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(203, 216, 230);
            this.btnRotationStep.BackColor = System.Drawing.Color.White;
            this.btnRotationStep.Enabled = false;
            this.lblStepState.Name = "vpStepState_dyn";
            this.lblStepState.Text = "请先完成九点并应用";
            this.lblStepState.Font = new System.Drawing.Font("宋体", 9F);
            this.lblStepState.Location = new System.Drawing.Point(380, 2);
            this.lblStepState.Size = new System.Drawing.Size(480, 28);
            this.lblStepState.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            this.lblStepState.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblStepState.AutoEllipsis = true;
            // 
            // VpCalibCard
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(243)))), ((int)(((byte)(249)))));
            this.Controls.Add(this.card);
            this.Name = "VpCalibCard";
            this.Size = new System.Drawing.Size(900, 364);
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
        private UiLabel lblFeature;
        private System.Windows.Forms.ComboBox cmbFeature;
        private UiButton btnSetup;
        private System.Windows.Forms.FlowLayoutPanel coordinateRow;
        private UiLabel lblMotorX, lblMotorY;
        private System.Windows.Forms.TextBox tbMotorX, tbMotorY;
        private System.Windows.Forms.ToolTip help;
        private UiButton btnClearSamples;
        private System.Windows.Forms.FlowLayoutPanel btns;
        private UiButton btnAddCalibration;
        private UiButton btnEditTb;
        private UiButton btnCapture;
        private UiButton btnAddPoint;
        private UiButton btnDeletePoint;
        private UiButton btnApplyToCamera;
        private UiLabel lblTitle;
        private System.Windows.Forms.Panel stepRow;
        private UiLabel lblNineStep;
        private UiButton btnRotationStep;
        private UiLabel lblStepState;
    }
}
