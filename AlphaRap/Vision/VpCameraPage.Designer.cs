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
            this.lblCapCameraIndex = new AlphaRap.UiLabel();
            this.tbIndex = new System.Windows.Forms.TextBox();
            this.lblCapDefaultExposure = new AlphaRap.UiLabel();
            this.tbExposure = new System.Windows.Forms.TextBox();
            this.btnLive = new AlphaRap.UiButton();
            this.btnDeleteCamera = new AlphaRap.UiButton();
            this.head.SuspendLayout();
            this.SuspendLayout();
            // 
            // vppTabs（本相机的 VPP 子页容器：每个 VPP 一页，页面本体是 VpVppPage）
            // 
            this.vppTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.vppTabs.Font = new System.Drawing.Font("宋体", 14.25F);
            this.vppTabs.Location = new System.Drawing.Point(0, 320);
            this.vppTabs.Name = "vppTabs";
            this.vppTabs.SelectedIndex = 0;
            this.vppTabs.Size = new System.Drawing.Size(660, 300);
            this.vppTabs.TabIndex = 3;
            // 
            // calibHost（标定卡宿主：一台相机一张标定卡，由 VPForm 在运行时塞进来）
            // 
            this.calibHost.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(243)))), ((int)(((byte)(249)))));
            this.calibHost.Dock = System.Windows.Forms.DockStyle.Top;
            this.calibHost.Location = new System.Drawing.Point(0, 52);
            this.calibHost.Name = "calibHost";
            this.calibHost.Padding = new System.Windows.Forms.Padding(8, 4, 8, 6);
            this.calibHost.Size = new System.Drawing.Size(660, 268);
            this.calibHost.TabIndex = 2;
            // 
            // dispHost（右侧：整台相机共用的显示区，ActiveX 显示控件由 VPForm 在运行时塞进来）
            // 
            this.dispHost.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(243)))), ((int)(((byte)(249)))));
            this.dispHost.Dock = System.Windows.Forms.DockStyle.Right;
            this.dispHost.Location = new System.Drawing.Point(660, 0);
            this.dispHost.Name = "dispHost";
            this.dispHost.Size = new System.Drawing.Size(380, 620);
            this.dispHost.TabIndex = 1;
            // 
            // head（相机属性条：名称 / CameraIndex / 默认曝光 / 实时显示 / 删除本相机）
            // 
            this.head.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(243)))), ((int)(((byte)(249)))));
            this.head.Controls.Add(this.lblCapCameraName);
            this.head.Controls.Add(this.tbName);
            this.head.Controls.Add(this.lblCapCameraIndex);
            this.head.Controls.Add(this.tbIndex);
            this.head.Controls.Add(this.lblCapDefaultExposure);
            this.head.Controls.Add(this.tbExposure);
            this.head.Controls.Add(this.btnLive);
            this.head.Controls.Add(this.btnDeleteCamera);
            this.head.Dock = System.Windows.Forms.DockStyle.Top;
            this.head.Location = new System.Drawing.Point(0, 0);
            this.head.Name = "head";
            this.head.Size = new System.Drawing.Size(660, 52);
            this.head.TabIndex = 0;
            // 
            // lblCapCameraName
            // 
            this.lblCapCameraName.AutoSize = false;
            this.lblCapCameraName.Font = new System.Drawing.Font("宋体", 11.25F);
            this.lblCapCameraName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(92)))), ((int)(((byte)(106)))));
            this.lblCapCameraName.Location = new System.Drawing.Point(10, 15);
            this.lblCapCameraName.Name = "vpCap_CameraName";
            this.lblCapCameraName.Size = new System.Drawing.Size(98, 22);
            this.lblCapCameraName.TabIndex = 0;
            this.lblCapCameraName.Text = "相机名称";
            this.lblCapCameraName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbName
            // 
            this.tbName.Font = new System.Drawing.Font("宋体", 11.25F);
            this.tbName.Location = new System.Drawing.Point(112, 13);
            this.tbName.Name = "vpCamNameBox";
            this.tbName.Size = new System.Drawing.Size(120, 25);
            this.tbName.TabIndex = 1;
            // 
            // lblCapCameraIndex
            // 
            this.lblCapCameraIndex.AutoSize = false;
            this.lblCapCameraIndex.Font = new System.Drawing.Font("宋体", 11.25F);
            this.lblCapCameraIndex.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(92)))), ((int)(((byte)(106)))));
            this.lblCapCameraIndex.Location = new System.Drawing.Point(248, 15);
            this.lblCapCameraIndex.Name = "vpCap_CameraIndex";
            this.lblCapCameraIndex.Size = new System.Drawing.Size(98, 22);
            this.lblCapCameraIndex.TabIndex = 2;
            this.lblCapCameraIndex.Text = "CameraIndex";
            this.lblCapCameraIndex.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbIndex
            // 
            this.tbIndex.Font = new System.Drawing.Font("宋体", 11.25F);
            this.tbIndex.Location = new System.Drawing.Point(350, 13);
            this.tbIndex.Name = "vpCamIndexBox";
            this.tbIndex.Size = new System.Drawing.Size(64, 25);
            this.tbIndex.TabIndex = 3;
            // 
            // lblCapDefaultExposure
            // 
            this.lblCapDefaultExposure.AutoSize = false;
            this.lblCapDefaultExposure.Font = new System.Drawing.Font("宋体", 11.25F);
            this.lblCapDefaultExposure.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(92)))), ((int)(((byte)(106)))));
            this.lblCapDefaultExposure.Location = new System.Drawing.Point(430, 15);
            this.lblCapDefaultExposure.Name = "vpCap_DefaultExposure";
            this.lblCapDefaultExposure.Size = new System.Drawing.Size(98, 22);
            this.lblCapDefaultExposure.TabIndex = 4;
            this.lblCapDefaultExposure.Text = "默认曝光";
            this.lblCapDefaultExposure.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbExposure
            // 
            this.tbExposure.Font = new System.Drawing.Font("宋体", 11.25F);
            this.tbExposure.Location = new System.Drawing.Point(532, 13);
            this.tbExposure.Name = "vpCamExpBox";
            this.tbExposure.Size = new System.Drawing.Size(70, 25);
            this.tbExposure.TabIndex = 5;
            // 
            // btnLive（实时显示：相机级。文字跟语言 + 实时状态走，名字带 _dyn 不进语言表）
            // 
            this.btnLive.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(232)))), ((int)(((byte)(244)))), ((int)(((byte)(255)))));
            this.btnLive.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLive.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(140)))), ((int)(((byte)(180)))), ((int)(((byte)(220)))));
            this.btnLive.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLive.Font = new System.Drawing.Font("宋体", 11.25F);
            this.btnLive.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.btnLive.Location = new System.Drawing.Point(618, 12);
            this.btnLive.Name = "vpBtn_Live_dyn";
            this.btnLive.Size = new System.Drawing.Size(118, 30);
            this.btnLive.TabIndex = 6;
            // 
            // btnDeleteCamera
            // 
            this.btnDeleteCamera.BackColor = System.Drawing.Color.White;
            this.btnDeleteCamera.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDeleteCamera.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(203)))), ((int)(((byte)(216)))), ((int)(((byte)(230)))));
            this.btnDeleteCamera.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDeleteCamera.Font = new System.Drawing.Font("宋体", 11.25F);
            this.btnDeleteCamera.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.btnDeleteCamera.Location = new System.Drawing.Point(744, 12);
            this.btnDeleteCamera.Name = "vpBtn_DeleteCamera";
            this.btnDeleteCamera.Size = new System.Drawing.Size(118, 30);
            this.btnDeleteCamera.TabIndex = 7;
            this.btnDeleteCamera.Text = "删除本相机";
            // 
            // VpCameraPage
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(243)))), ((int)(((byte)(249)))));
            // 停靠顺序（后加入的先停靠）：自上而下 head(52) / 标定卡(268) / vppTabs(填充)，右侧显示区(380)
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
        private UiLabel lblCapCameraIndex;
        private System.Windows.Forms.TextBox tbIndex;
        private UiLabel lblCapDefaultExposure;
        private System.Windows.Forms.TextBox tbExposure;
        private UiButton btnLive;
        private UiButton btnDeleteCamera;
    }
}
