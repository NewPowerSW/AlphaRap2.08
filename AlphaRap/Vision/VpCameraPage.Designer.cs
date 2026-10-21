namespace AlphaRap
{
    partial class VpCameraPage
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
            this.vppTabs = new System.Windows.Forms.TabControl();
            this.calibHost = new System.Windows.Forms.Panel();
            this.dispHost = new System.Windows.Forms.Panel();
            this.head = new System.Windows.Forms.Panel();
            this.lblCapCameraName = new AlphaRap.UiLabel();
            this.tbName = new System.Windows.Forms.TextBox();
            this.lblCapCameraSN = new AlphaRap.UiLabel();
            this.cbSN = new System.Windows.Forms.ComboBox();
            this.lblCapDefaultExposure = new AlphaRap.UiLabel();
            this.tbExposure = new System.Windows.Forms.TextBox();
            this.btnLive = new AlphaRap.UiButton();
            this.btnDeleteCamera = new AlphaRap.UiButton();
            this.head.SuspendLayout();
            this.SuspendLayout();
            // 
            // vppTabs
            // 
            this.vppTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.vppTabs.Font = new System.Drawing.Font("宋体", 14.25F);
            this.vppTabs.Location = new System.Drawing.Point(0, 320);
            this.vppTabs.Name = "vppTabs";
            this.vppTabs.SelectedIndex = 0;
            this.vppTabs.Size = new System.Drawing.Size(592, 300);
            this.vppTabs.TabIndex = 3;
            // 
            // calibHost
            // 
            this.calibHost.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(243)))), ((int)(((byte)(249)))));
            this.calibHost.Dock = System.Windows.Forms.DockStyle.Top;
            this.calibHost.Location = new System.Drawing.Point(0, 52);
            this.calibHost.Name = "calibHost";
            this.calibHost.Padding = new System.Windows.Forms.Padding(8, 4, 8, 6);
            this.calibHost.Size = new System.Drawing.Size(592, 268);
            this.calibHost.TabIndex = 2;
            // 
            // dispHost
            // 
            this.dispHost.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(243)))), ((int)(((byte)(249)))));
            this.dispHost.Dock = System.Windows.Forms.DockStyle.Right;
            this.dispHost.Location = new System.Drawing.Point(592, 52);
            this.dispHost.Name = "dispHost";
            this.dispHost.Size = new System.Drawing.Size(448, 568);
            this.dispHost.TabIndex = 1;
            // 
            // head
            // 
            this.head.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(243)))), ((int)(((byte)(249)))));
            this.head.Controls.Add(this.lblCapCameraName);
            this.head.Controls.Add(this.tbName);
            this.head.Controls.Add(this.lblCapCameraSN);
            this.head.Controls.Add(this.cbSN);
            this.head.Controls.Add(this.lblCapDefaultExposure);
            this.head.Controls.Add(this.tbExposure);
            this.head.Controls.Add(this.btnLive);
            this.head.Controls.Add(this.btnDeleteCamera);
            this.head.Dock = System.Windows.Forms.DockStyle.Top;
            this.head.Location = new System.Drawing.Point(0, 0);
            this.head.Name = "head";
            this.head.Size = new System.Drawing.Size(1040, 52);
            this.head.TabIndex = 0;
            // 
            // lblCapCameraName
            // 
            this.lblCapCameraName.Font = new System.Drawing.Font("宋体", 11.25F);
            this.lblCapCameraName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(92)))), ((int)(((byte)(106)))));
            this.lblCapCameraName.Location = new System.Drawing.Point(10, 15);
            this.lblCapCameraName.Name = "lblCapCameraName";
            this.lblCapCameraName.Size = new System.Drawing.Size(86, 22);
            this.lblCapCameraName.TabIndex = 0;
            this.lblCapCameraName.Text = "相机名称";
            this.lblCapCameraName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbName
            // 
            this.tbName.Font = new System.Drawing.Font("宋体", 11.25F);
            this.tbName.Location = new System.Drawing.Point(100, 13);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(110, 25);
            this.tbName.TabIndex = 1;
            // 
            // lblCapCameraSN
            // 
            this.lblCapCameraSN.Font = new System.Drawing.Font("宋体", 11.25F);
            this.lblCapCameraSN.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(92)))), ((int)(((byte)(106)))));
            this.lblCapCameraSN.Location = new System.Drawing.Point(216, 15);
            this.lblCapCameraSN.Name = "lblCapCameraSN_dyn";
            this.lblCapCameraSN.Size = new System.Drawing.Size(90, 22);
            this.lblCapCameraSN.TabIndex = 2;
            this.lblCapCameraSN.Text = "相机SN";
            this.lblCapCameraSN.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cbSN（相机 SN 下拉：列出当前可用的全部相机 SN，选中即按 SN 绑定）
            // 
            this.cbSN.Font = new System.Drawing.Font("宋体", 11.25F);
            this.cbSN.Location = new System.Drawing.Point(306, 13);
            this.cbSN.Name = "cbSN";
            this.cbSN.Size = new System.Drawing.Size(224, 25);
            this.cbSN.TabIndex = 2;

            // 
            // lblCapDefaultExposure
            // 
            this.lblCapDefaultExposure.Font = new System.Drawing.Font("宋体", 11.25F);
            this.lblCapDefaultExposure.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(92)))), ((int)(((byte)(106)))));
            this.lblCapDefaultExposure.Location = new System.Drawing.Point(536, 15);
            this.lblCapDefaultExposure.Name = "lblCapDefaultExposure";
            this.lblCapDefaultExposure.Size = new System.Drawing.Size(70, 22);
            this.lblCapDefaultExposure.TabIndex = 3;
            this.lblCapDefaultExposure.Text = "默认曝光";
            this.lblCapDefaultExposure.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbExposure
            // 
            this.tbExposure.Font = new System.Drawing.Font("宋体", 11.25F);
            this.tbExposure.Location = new System.Drawing.Point(610, 13);
            this.tbExposure.Name = "tbExposure";
            this.tbExposure.Size = new System.Drawing.Size(60, 25);
            this.tbExposure.TabIndex = 4;
            // 
            // btnLive
            // 
            this.btnLive.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.btnLive.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLive.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(180)))), ((int)(((byte)(220)))));
            this.btnLive.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLive.Font = new System.Drawing.Font("宋体", 11.25F);
            this.btnLive.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.btnLive.Location = new System.Drawing.Point(728, 12);
            this.btnLive.Name = "btnLive_dyn";
            this.btnLive.Size = new System.Drawing.Size(110, 30);
            this.btnLive.TabIndex = 5;
            this.btnLive.UseVisualStyleBackColor = false;
            // 
            // btnDeleteCamera
            // 
            this.btnDeleteCamera.BackColor = System.Drawing.Color.White;
            this.btnDeleteCamera.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDeleteCamera.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(216)))), ((int)(((byte)(230)))));
            this.btnDeleteCamera.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeleteCamera.Font = new System.Drawing.Font("宋体", 11.25F);
            this.btnDeleteCamera.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.btnDeleteCamera.Location = new System.Drawing.Point(846, 12);
            this.btnDeleteCamera.Name = "btnDeleteCamera";
            this.btnDeleteCamera.Size = new System.Drawing.Size(110, 30);
            this.btnDeleteCamera.TabIndex = 6;
            this.btnDeleteCamera.Text = "删除本相机";
            this.btnDeleteCamera.UseVisualStyleBackColor = false;
            // 
            // VpCameraPage
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(243)))), ((int)(((byte)(249)))));
            this.Controls.Add(this.vppTabs);
            this.Controls.Add(this.calibHost);
            this.Controls.Add(this.dispHost);
            this.Controls.Add(this.head);
            this.Name = "VpCameraPage";
            this.Size = new System.Drawing.Size(1040, 620);
            this.head.ResumeLayout(false);
            this.head.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl vppTabs;
        private System.Windows.Forms.Panel calibHost;
        private System.Windows.Forms.Panel dispHost;
        private System.Windows.Forms.Panel head;
        private UiLabel lblCapCameraName;
        private System.Windows.Forms.TextBox tbName;
        private UiLabel lblCapCameraSN;
        private System.Windows.Forms.ComboBox cbSN;
        private UiLabel lblCapDefaultExposure;
        private System.Windows.Forms.TextBox tbExposure;
        private UiButton btnLive;
        private UiButton btnDeleteCamera;
    }
}
