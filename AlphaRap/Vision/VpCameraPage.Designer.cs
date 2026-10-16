namespace AlphaRap
{
    partial class VpCameraPage
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
            this.vppTabs = new System.Windows.Forms.TabControl();
            this.calibHost = new System.Windows.Forms.Panel();
            this.dispHost = new System.Windows.Forms.Panel();
            this.head = new System.Windows.Forms.FlowLayoutPanel();
            this.lblCapCameraName = new AlphaRap.UiLabel();
            this.tbName = new System.Windows.Forms.TextBox();
            this.lblCapCameraIndex = new AlphaRap.UiLabel();
            this.tbIndex = new System.Windows.Forms.TextBox();
            this.lblCapDefaultExposure = new AlphaRap.UiLabel();
            this.tbExposure = new System.Windows.Forms.TextBox();
            this.btnLive = new AlphaRap.FlatButton();
            this.head.SuspendLayout();
            this.SuspendLayout();
            // 
            // vppTabs（占满剩余高度，给 VPP 操作区更多空间）
            // 
            this.vppTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.vppTabs.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold);
            this.vppTabs.Location = new System.Drawing.Point(0, 268);
            this.vppTabs.Name = "vppTabs";
            this.vppTabs.Padding = new System.Drawing.Point(12, 6);
            this.vppTabs.SelectedIndex = 0;
            this.vppTabs.Size = new System.Drawing.Size(640, 352);
            this.vppTabs.TabIndex = 3;
            // 
            // calibHost（略压缩，避免挤占下方 VPP）
            // 
            this.calibHost.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(243)))), ((int)(((byte)(248)))));
            this.calibHost.Dock = System.Windows.Forms.DockStyle.Top;
            this.calibHost.Location = new System.Drawing.Point(0, 56);
            this.calibHost.Name = "calibHost";
            this.calibHost.Padding = new System.Windows.Forms.Padding(10, 6, 10, 8);
            this.calibHost.Size = new System.Drawing.Size(640, 212);
            this.calibHost.TabIndex = 2;
            // 
            // dispHost
            // 
            this.dispHost.BackColor = System.Drawing.Color.White;
            this.dispHost.Dock = System.Windows.Forms.DockStyle.Right;
            this.dispHost.Location = new System.Drawing.Point(640, 56);
            this.dispHost.Name = "dispHost";
            this.dispHost.Padding = new System.Windows.Forms.Padding(8);
            this.dispHost.Size = new System.Drawing.Size(400, 564);
            this.dispHost.TabIndex = 1;
            // 
            // head（Flow：英文长标签可换行，不挤扁）
            // 
            this.head.BackColor = System.Drawing.Color.White;
            this.head.Controls.Add(this.lblCapCameraName);
            this.head.Controls.Add(this.tbName);
            this.head.Controls.Add(this.lblCapCameraIndex);
            this.head.Controls.Add(this.tbIndex);
            this.head.Controls.Add(this.lblCapDefaultExposure);
            this.head.Controls.Add(this.tbExposure);
            this.head.Controls.Add(this.btnLive);
            this.head.Dock = System.Windows.Forms.DockStyle.Top;
            this.head.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.head.Location = new System.Drawing.Point(0, 0);
            this.head.Name = "head";
            this.head.Padding = new System.Windows.Forms.Padding(12, 10, 12, 8);
            this.head.Size = new System.Drawing.Size(1040, 56);
            this.head.TabIndex = 0;
            this.head.WrapContents = true;
            // 
            // lblCapCameraName
            // 
            this.lblCapCameraName.AutoSize = true;
            this.lblCapCameraName.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblCapCameraName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(108)))), ((int)(((byte)(126)))));
            this.lblCapCameraName.Location = new System.Drawing.Point(12, 14);
            this.lblCapCameraName.Margin = new System.Windows.Forms.Padding(0, 4, 6, 0);
            this.lblCapCameraName.Name = "lblCapCameraName";
            this.lblCapCameraName.Size = new System.Drawing.Size(65, 20);
            this.lblCapCameraName.TabIndex = 0;
            this.lblCapCameraName.Text = "相机名称";
            this.lblCapCameraName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbName
            // 
            this.tbName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(252)))), ((int)(((byte)(254)))));
            this.tbName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbName.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.tbName.Location = new System.Drawing.Point(83, 12);
            this.tbName.Margin = new System.Windows.Forms.Padding(0, 2, 14, 0);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(120, 25);
            this.tbName.TabIndex = 1;
            // 
            // lblCapCameraIndex
            // 
            this.lblCapCameraIndex.AutoSize = true;
            this.lblCapCameraIndex.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblCapCameraIndex.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(108)))), ((int)(((byte)(126)))));
            this.lblCapCameraIndex.Location = new System.Drawing.Point(217, 14);
            this.lblCapCameraIndex.Margin = new System.Windows.Forms.Padding(0, 4, 6, 0);
            this.lblCapCameraIndex.Name = "lblCapCameraIndex";
            this.lblCapCameraIndex.Size = new System.Drawing.Size(94, 20);
            this.lblCapCameraIndex.TabIndex = 2;
            this.lblCapCameraIndex.Text = "CameraIndex";
            this.lblCapCameraIndex.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbIndex
            // 
            this.tbIndex.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(252)))), ((int)(((byte)(254)))));
            this.tbIndex.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbIndex.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.tbIndex.Location = new System.Drawing.Point(317, 12);
            this.tbIndex.Margin = new System.Windows.Forms.Padding(0, 2, 14, 0);
            this.tbIndex.Name = "tbIndex";
            this.tbIndex.Size = new System.Drawing.Size(56, 25);
            this.tbIndex.TabIndex = 3;
            // 
            // lblCapDefaultExposure
            // 
            this.lblCapDefaultExposure.AutoSize = true;
            this.lblCapDefaultExposure.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.lblCapDefaultExposure.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(108)))), ((int)(((byte)(126)))));
            this.lblCapDefaultExposure.Location = new System.Drawing.Point(387, 14);
            this.lblCapDefaultExposure.Margin = new System.Windows.Forms.Padding(0, 4, 6, 0);
            this.lblCapDefaultExposure.Name = "lblCapDefaultExposure";
            this.lblCapDefaultExposure.Size = new System.Drawing.Size(65, 20);
            this.lblCapDefaultExposure.TabIndex = 4;
            this.lblCapDefaultExposure.Text = "默认曝光";
            this.lblCapDefaultExposure.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tbExposure
            // 
            this.tbExposure.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(252)))), ((int)(((byte)(254)))));
            this.tbExposure.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbExposure.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.tbExposure.Location = new System.Drawing.Point(458, 12);
            this.tbExposure.Margin = new System.Windows.Forms.Padding(0, 2, 16, 0);
            this.tbExposure.Name = "tbExposure";
            this.tbExposure.Size = new System.Drawing.Size(64, 25);
            this.tbExposure.TabIndex = 5;
            // 
            // btnLive
            // 
            this.btnLive.BackColor = System.Drawing.Color.Transparent;
            this.btnLive.CornerRadius = 8;
            this.btnLive.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLive.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold);
            this.btnLive.Location = new System.Drawing.Point(538, 10);
            this.btnLive.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.btnLive.Name = "btnLive";
            this.btnLive.Size = new System.Drawing.Size(120, 32);
            this.btnLive.TabIndex = 6;
            this.btnLive.Text = "实时显示";
            this.btnLive.Variant = AlphaRap.FlatButtonVariant.Primary;
            // 
            // VpCameraPage
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(243)))), ((int)(((byte)(248)))));
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
        private System.Windows.Forms.FlowLayoutPanel head;
        private UiLabel lblCapCameraName;
        private System.Windows.Forms.TextBox tbName;
        private UiLabel lblCapCameraIndex;
        private System.Windows.Forms.TextBox tbIndex;
        private UiLabel lblCapDefaultExposure;
        private System.Windows.Forms.TextBox tbExposure;
        private FlatButton btnLive;
    }
}
