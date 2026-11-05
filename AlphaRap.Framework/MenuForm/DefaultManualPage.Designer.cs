namespace AlphaRap
{
    partial class DefaultManualPage
    {
        private Configuration.HardwareResourcePanel ioResourcePanel;

        /// <summary>在设计器及运行时使用同一个 IO 管理面板。</summary>
        private void InitializeComponent()
        {
            this.ioResourcePanel = new Configuration.HardwareResourcePanel();
            this.SuspendLayout();
            this.ioResourcePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ioResourcePanel.Name = "ioResourcePanel";
            this.ioResourcePanel.ResourceKind = "IO";
            this.Controls.Add(this.ioResourcePanel);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1284, 735);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ManualForm";
            this.Text = "ManualForm";
            this.ResumeLayout(false);
        }
    }
}
