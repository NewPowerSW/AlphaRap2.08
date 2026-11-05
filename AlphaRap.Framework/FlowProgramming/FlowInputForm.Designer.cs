namespace AlphaRap.FlowProgramming
{
    partial class FlowInputForm
    {
        private System.Windows.Forms.Label prompt;
        private System.Windows.Forms.TextBox value;
        private System.Windows.Forms.Button ok;
        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.FlowLayoutPanel buttons;
        /// <summary>创建设计器与运行时共用的操作员输入布局。</summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.prompt = new System.Windows.Forms.Label();
            this.prompt.Name = "prompt_dyn";
            this.prompt.Text = "输入产品信息";
            this.prompt.Dock = System.Windows.Forms.DockStyle.Top;
            this.prompt.Height = 56;
            this.prompt.Padding = new System.Windows.Forms.Padding(12);
            this.value = new System.Windows.Forms.TextBox();
            this.value.Name = "operatorValue";
            this.value.Dock = System.Windows.Forms.DockStyle.Top;
            this.value.Margin = new System.Windows.Forms.Padding(12);
            this.buttons = new System.Windows.Forms.FlowLayoutPanel();
            this.buttons.SuspendLayout();
            this.buttons.Name = "buttons";
            this.buttons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttons.Height = 44;
            this.buttons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.buttons.Padding = new System.Windows.Forms.Padding(6);
            this.ok = new System.Windows.Forms.Button();
            this.ok.Name = "ok";
            this.ok.Tag = "ok";
            this.ok.Text = "确定";
            this.ok.AutoSize = true;
            this.cancel = new System.Windows.Forms.Button();
            this.cancel.Name = "cancel";
            this.cancel.Tag = "cancel";
            this.cancel.Text = "取消";
            this.cancel.AutoSize = true;
            this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.AcceptButton = this.ok;
            this.CancelButton = this.cancel;
            this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttons.Controls.Add(this.cancel);
            this.buttons.Controls.Add(this.ok);
            this.Controls.Add(this.value);
            this.Controls.Add(this.prompt);
            this.Controls.Add(this.buttons);
            this.Name = "FlowInputForm";
            this.Tag = "title";
            this.Text = "流程工程";
            this.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(640, 190);
            this.MinimumSize = new System.Drawing.Size(640, 190);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.ShowInTaskbar = false;
            this.MaximizeBox = false;
            this.buttons.ResumeLayout(false);
            this.buttons.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
