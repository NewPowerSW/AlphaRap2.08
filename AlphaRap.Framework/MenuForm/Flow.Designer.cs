namespace AlphaRap
{
    partial class Flow
    {
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.Label status;
        private System.Windows.Forms.Timer timer;

        /// <summary>释放刷新计时器及窗口资源。</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>布置工站页签和运行错误提示。</summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.status = new System.Windows.Forms.Label();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Name = "tabControl1";
            this.status.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.status.Name = "status";
            this.status.ForeColor = System.Drawing.Color.Firebrick;
            this.status.Height = 28;
            this.status.Visible = false;
            this.timer.Interval = 250;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1280, 620);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.status);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Flow";
            this.Text = "流程";
            this.ResumeLayout(false);
        }
    }
}
