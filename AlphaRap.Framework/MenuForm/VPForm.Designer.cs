namespace AlphaRap
{
    partial class VPForm
    {
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageDesignPreview;
        private VpCameraPage cameraPagePreview;

        /// <summary>视觉管理页的通用设计预览与运行容器。</summary>
        private void InitializeComponent()
        {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageDesignPreview = new System.Windows.Forms.TabPage();
            this.cameraPagePreview = new VpCameraPage();
            this.SuspendLayout();
            this.tabControl1.Controls.Add(this.tabPageDesignPreview);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1612, 1102);
            this.tabControl1.TabIndex = 0;
            this.tabPageDesignPreview.Controls.Add(this.cameraPagePreview);
            this.tabPageDesignPreview.Location = new System.Drawing.Point(4, 26);
            this.tabPageDesignPreview.Name = "tabPageDesignPreview";
            this.tabPageDesignPreview.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageDesignPreview.Size = new System.Drawing.Size(1604, 1072);
            this.tabPageDesignPreview.TabIndex = 1;
            this.tabPageDesignPreview.Text = "相机页（设计预览）";
            this.cameraPagePreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cameraPagePreview.Location = new System.Drawing.Point(3, 3);
            this.cameraPagePreview.Name = "cameraPagePreview";
            this.cameraPagePreview.Size = new System.Drawing.Size(1598, 1066);
            this.cameraPagePreview.TabIndex = 0;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(1612, 1102);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "VPForm";
            this.Text = "VP";
            this.ResumeLayout(false);
        }
    }
}
