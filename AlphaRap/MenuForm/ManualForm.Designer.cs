namespace AlphaRap
{
    partial class ManualForm
    {
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage ioResourcesPage;
        private Configuration.HardwareResourcePanel ioResourcePanel;

        /// <summary>在设计器和运行时使用同一个动态 IO 面板。</summary>
        private void InitializeComponent()
        {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.ioResourcesPage = new System.Windows.Forms.TabPage();
            this.ioResourcePanel = new Configuration.HardwareResourcePanel();
            this.tabControl1.SuspendLayout();
            this.ioResourcesPage.SuspendLayout();
            this.SuspendLayout();

            this.tabControl1.Controls.Add(this.ioResourcesPage);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;

            this.ioResourcesPage.Controls.Add(this.ioResourcePanel);
            this.ioResourcesPage.Name = "ioResourcesPage";
            this.ioResourcesPage.Text = "IO 资源";
            this.ioResourcesPage.UseVisualStyleBackColor = true;

            this.ioResourcePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ioResourcePanel.Name = "ioResourcePanel";
            this.ioResourcePanel.ResourceKind = "IO";

            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1288, 650);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ManualForm";
            this.Text = "ManualForm";
            this.tabControl1.ResumeLayout(false);
            this.ioResourcesPage.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
