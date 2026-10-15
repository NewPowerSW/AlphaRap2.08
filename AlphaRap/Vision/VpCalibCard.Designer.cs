namespace AlphaRap
{
    partial class VpCalibCard
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
            this.card = new AlphaRap.CardPanel();
            this.gridPoints = new System.Windows.Forms.DataGridView();
            this.cPx = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cPy = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cMx = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cMy = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.actionRow = new System.Windows.Forms.FlowLayoutPanel();
            this.lblExpCaption = new AlphaRap.UiLabel();
            this.tbExposure = new System.Windows.Forms.TextBox();
            this.btnAddCalibration = new AlphaRap.FlatButton();
            this.btnEditTb = new AlphaRap.FlatButton();
            this.btnCapture = new AlphaRap.FlatButton();
            this.btnAddPoint = new AlphaRap.FlatButton();
            this.btnDeletePoint = new AlphaRap.FlatButton();
            this.btnApplyToCamera = new AlphaRap.FlatButton();
            this.titleBar = new System.Windows.Forms.Panel();
            this.lblTitle = new AlphaRap.UiLabel();
            this.lblTip = new AlphaRap.UiLabel();
            this.card.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridPoints)).BeginInit();
            this.actionRow.SuspendLayout();
            this.titleBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // card
            // 
            this.card.BackColor = System.Drawing.Color.White;
            this.card.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(231)))), ((int)(((byte)(240)))));
            this.card.Controls.Add(this.gridPoints);
            this.card.Controls.Add(this.actionRow);
            this.card.Controls.Add(this.titleBar);
            this.card.CornerRadius = 10;
            this.card.Dock = System.Windows.Forms.DockStyle.Fill;
            this.card.FillColor = System.Drawing.Color.White;
            this.card.Location = new System.Drawing.Point(0, 0);
            this.card.Name = "card";
            this.card.Padding = new System.Windows.Forms.Padding(14, 10, 14, 10);
            this.card.ShadowColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(176)))), ((int)(((byte)(196)))), ((int)(((byte)(218)))));
            this.card.ShowBorder = true;
            this.card.ShowShadow = true;
            this.card.Size = new System.Drawing.Size(651, 202);
            this.card.TabIndex = 0;
            // 
            // gridPoints
            // 
            this.gridPoints.AllowUserToAddRows = false;
            this.gridPoints.AllowUserToDeleteRows = false;
            this.gridPoints.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gridPoints.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.gridPoints.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.cPx,
            this.cPy,
            this.cMx,
            this.cMy});
            this.gridPoints.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridPoints.Location = new System.Drawing.Point(14, 40);
            this.gridPoints.Name = "gridPoints";
            this.gridPoints.RowHeadersVisible = false;
            this.gridPoints.Size = new System.Drawing.Size(623, 100);
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
            // actionRow
            // 
            this.actionRow.BackColor = System.Drawing.Color.Transparent;
            this.actionRow.Controls.Add(this.lblExpCaption);
            this.actionRow.Controls.Add(this.tbExposure);
            this.actionRow.Controls.Add(this.btnAddCalibration);
            this.actionRow.Controls.Add(this.btnEditTb);
            this.actionRow.Controls.Add(this.btnCapture);
            this.actionRow.Controls.Add(this.btnAddPoint);
            this.actionRow.Controls.Add(this.btnDeletePoint);
            this.actionRow.Controls.Add(this.btnApplyToCamera);
            this.actionRow.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.actionRow.Location = new System.Drawing.Point(14, 140);
            this.actionRow.Name = "actionRow";
            this.actionRow.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.actionRow.Size = new System.Drawing.Size(623, 52);
            this.actionRow.TabIndex = 2;
            // 
            // lblExpCaption
            // 
            this.lblExpCaption.AutoSize = true;
            this.lblExpCaption.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblExpCaption.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(108)))), ((int)(((byte)(126)))));
            this.lblExpCaption.Location = new System.Drawing.Point(0, 10);
            this.lblExpCaption.Margin = new System.Windows.Forms.Padding(0, 6, 6, 0);
            this.lblExpCaption.Name = "lblExpCaption";
            this.lblExpCaption.Size = new System.Drawing.Size(65, 20);
            this.lblExpCaption.TabIndex = 0;
            this.lblExpCaption.Text = "标定曝光";
            // 
            // tbExposure
            // 
            this.tbExposure.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(252)))), ((int)(((byte)(254)))));
            this.tbExposure.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbExposure.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.tbExposure.Location = new System.Drawing.Point(71, 8);
            this.tbExposure.Margin = new System.Windows.Forms.Padding(0, 4, 12, 0);
            this.tbExposure.Name = "tbExposure";
            this.tbExposure.Size = new System.Drawing.Size(72, 25);
            this.tbExposure.TabIndex = 1;
            // 
            // btnAddCalibration
            // 
            this.btnAddCalibration.BackColor = System.Drawing.Color.Transparent;
            this.btnAddCalibration.CornerRadius = 8;
            this.btnAddCalibration.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddCalibration.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.btnAddCalibration.Icon = null;
            this.btnAddCalibration.IconGap = 8;
            this.btnAddCalibration.IconSize = 18;
            this.btnAddCalibration.Location = new System.Drawing.Point(155, 4);
            this.btnAddCalibration.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.btnAddCalibration.Name = "btnAddCalibration";
            this.btnAddCalibration.Selectable = false;
            this.btnAddCalibration.Size = new System.Drawing.Size(132, 32);
            this.btnAddCalibration.TabIndex = 2;
            this.btnAddCalibration.TabStop = false;
            this.btnAddCalibration.Text = "添加标定";
            this.btnAddCalibration.TextColorOverride = null;
            this.btnAddCalibration.Variant = AlphaRap.FlatButtonVariant.Primary;
            // 
            // btnEditTb
            // 
            this.btnEditTb.BackColor = System.Drawing.Color.Transparent;
            this.btnEditTb.CornerRadius = 8;
            this.btnEditTb.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEditTb.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.btnEditTb.Icon = null;
            this.btnEditTb.IconGap = 8;
            this.btnEditTb.IconSize = 18;
            this.btnEditTb.Location = new System.Drawing.Point(295, 4);
            this.btnEditTb.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.btnEditTb.Name = "btnEditTb";
            this.btnEditTb.Selectable = false;
            this.btnEditTb.Size = new System.Drawing.Size(100, 32);
            this.btnEditTb.TabIndex = 3;
            this.btnEditTb.TabStop = false;
            this.btnEditTb.Text = "编辑 TB";
            this.btnEditTb.TextColorOverride = null;
            this.btnEditTb.Variant = AlphaRap.FlatButtonVariant.Surface;
            // 
            // btnCapture
            // 
            this.btnCapture.BackColor = System.Drawing.Color.Transparent;
            this.btnCapture.CornerRadius = 8;
            this.btnCapture.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCapture.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.btnCapture.Icon = null;
            this.btnCapture.IconGap = 8;
            this.btnCapture.IconSize = 18;
            this.btnCapture.Location = new System.Drawing.Point(403, 4);
            this.btnCapture.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.btnCapture.Name = "btnCapture";
            this.btnCapture.Selectable = false;
            this.btnCapture.Size = new System.Drawing.Size(96, 32);
            this.btnCapture.TabIndex = 4;
            this.btnCapture.TabStop = false;
            this.btnCapture.Text = "拍照";
            this.btnCapture.TextColorOverride = null;
            this.btnCapture.Variant = AlphaRap.FlatButtonVariant.Primary;
            // 
            // btnAddPoint
            // 
            this.btnAddPoint.BackColor = System.Drawing.Color.Transparent;
            this.btnAddPoint.CornerRadius = 8;
            this.btnAddPoint.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddPoint.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.btnAddPoint.Icon = null;
            this.btnAddPoint.IconGap = 8;
            this.btnAddPoint.IconSize = 18;
            this.btnAddPoint.Location = new System.Drawing.Point(0, 36);
            this.btnAddPoint.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.btnAddPoint.Name = "btnAddPoint";
            this.btnAddPoint.Selectable = false;
            this.btnAddPoint.Size = new System.Drawing.Size(120, 32);
            this.btnAddPoint.TabIndex = 5;
            this.btnAddPoint.TabStop = false;
            this.btnAddPoint.Text = "添加标定点";
            this.btnAddPoint.TextColorOverride = null;
            this.btnAddPoint.Variant = AlphaRap.FlatButtonVariant.Surface;
            // 
            // btnDeletePoint
            // 
            this.btnDeletePoint.BackColor = System.Drawing.Color.Transparent;
            this.btnDeletePoint.CornerRadius = 8;
            this.btnDeletePoint.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDeletePoint.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.btnDeletePoint.Icon = null;
            this.btnDeletePoint.IconGap = 8;
            this.btnDeletePoint.IconSize = 18;
            this.btnDeletePoint.Location = new System.Drawing.Point(128, 36);
            this.btnDeletePoint.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.btnDeletePoint.Name = "btnDeletePoint";
            this.btnDeletePoint.Selectable = false;
            this.btnDeletePoint.Size = new System.Drawing.Size(120, 32);
            this.btnDeletePoint.TabIndex = 6;
            this.btnDeletePoint.TabStop = false;
            this.btnDeletePoint.Text = "删除选中点";
            this.btnDeletePoint.TextColorOverride = null;
            this.btnDeletePoint.Variant = AlphaRap.FlatButtonVariant.Danger;
            // 
            // btnApplyToCamera
            // 
            this.btnApplyToCamera.BackColor = System.Drawing.Color.Transparent;
            this.btnApplyToCamera.CornerRadius = 8;
            this.btnApplyToCamera.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnApplyToCamera.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.btnApplyToCamera.Icon = null;
            this.btnApplyToCamera.IconGap = 8;
            this.btnApplyToCamera.IconSize = 18;
            this.btnApplyToCamera.Location = new System.Drawing.Point(256, 36);
            this.btnApplyToCamera.Margin = new System.Windows.Forms.Padding(0);
            this.btnApplyToCamera.Name = "btnApplyToCamera";
            this.btnApplyToCamera.Selectable = false;
            this.btnApplyToCamera.Size = new System.Drawing.Size(132, 32);
            this.btnApplyToCamera.TabIndex = 7;
            this.btnApplyToCamera.TabStop = false;
            this.btnApplyToCamera.Text = "应用到本相机";
            this.btnApplyToCamera.TextColorOverride = null;
            this.btnApplyToCamera.Variant = AlphaRap.FlatButtonVariant.Primary;
            // 
            // titleBar
            // 
            this.titleBar.BackColor = System.Drawing.Color.Transparent;
            this.titleBar.Controls.Add(this.lblTitle);
            this.titleBar.Controls.Add(this.lblTip);
            this.titleBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.titleBar.Location = new System.Drawing.Point(14, 10);
            this.titleBar.Name = "titleBar";
            this.titleBar.Size = new System.Drawing.Size(623, 30);
            this.titleBar.TabIndex = 3;
            // 
            // lblTitle
            // 
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font = new System.Drawing.Font("微软雅黑", 11F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(263, 30);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblTip
            // 
            this.lblTip.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblTip.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.lblTip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(138)))), ((int)(((byte)(155)))));
            this.lblTip.Location = new System.Drawing.Point(263, 0);
            this.lblTip.Name = "lblTip";
            this.lblTip.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.lblTip.Size = new System.Drawing.Size(360, 30);
            this.lblTip.TabIndex = 1;
            this.lblTip.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // VpCalibCard
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(243)))), ((int)(((byte)(248)))));
            this.Controls.Add(this.card);
            this.Name = "VpCalibCard";
            this.Size = new System.Drawing.Size(651, 202);
            this.card.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridPoints)).EndInit();
            this.actionRow.ResumeLayout(false);
            this.actionRow.PerformLayout();
            this.titleBar.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private CardPanel card;
        private System.Windows.Forms.DataGridView gridPoints;
        private System.Windows.Forms.DataGridViewTextBoxColumn cPx;
        private System.Windows.Forms.DataGridViewTextBoxColumn cPy;
        private System.Windows.Forms.DataGridViewTextBoxColumn cMx;
        private System.Windows.Forms.DataGridViewTextBoxColumn cMy;
        private System.Windows.Forms.FlowLayoutPanel actionRow;
        private UiLabel lblExpCaption;
        private System.Windows.Forms.TextBox tbExposure;
        private FlatButton btnAddCalibration;
        private FlatButton btnEditTb;
        private FlatButton btnCapture;
        private FlatButton btnAddPoint;
        private FlatButton btnDeletePoint;
        private FlatButton btnApplyToCamera;
        private System.Windows.Forms.Panel titleBar;
        private UiLabel lblTitle;
        private UiLabel lblTip;
    }
}