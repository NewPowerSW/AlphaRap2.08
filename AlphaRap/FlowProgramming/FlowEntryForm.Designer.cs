namespace AlphaRap.FlowProgramming
{
    partial class FlowEntryForm
    {
        private System.Windows.Forms.Label stage;
        private System.Windows.Forms.Label explanation;
        private System.Windows.Forms.CheckedListBox entries;
        private System.Windows.Forms.FlowLayoutPanel commands;
        private System.Windows.Forms.Button ok;
        private System.Windows.Forms.Button cancel;

        /// <summary>固定多选入口列表与确定取消布局。</summary>
        private void InitializeComponent()
        {
            this.stage = new System.Windows.Forms.Label();
            this.explanation = new System.Windows.Forms.Label();
            this.entries = new System.Windows.Forms.CheckedListBox();
            this.commands = new System.Windows.Forms.FlowLayoutPanel();
            this.ok = new System.Windows.Forms.Button();
            this.cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            this.stage.Name = "entryStage_dyn";
            this.stage.Dock = System.Windows.Forms.DockStyle.Top;
            this.stage.Height = 32;
            this.stage.Text = "初始化流程";
            this.stage.Font = new System.Drawing.Font("Microsoft YaHei", 10F, System.Drawing.FontStyle.Bold);
            this.explanation.Name = "entryExplanation";
            this.explanation.Tag = "entryExplanation";
            this.explanation.Text = "勾选需要同时启动的流程入口；全部结束后，该模块的当前阶段才完成。";
            this.explanation.Dock = System.Windows.Forms.DockStyle.Top;
            this.explanation.Height = 48;
            this.entries.Name = "entryChoices_dyn";
            this.entries.Dock = System.Windows.Forms.DockStyle.Fill;
            this.entries.CheckOnClick = true;
            this.entries.IntegralHeight = false;
            this.commands.Name = "entryCommands";
            this.commands.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.commands.Height = 44;
            this.commands.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.commands.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.ok.Name = "entryOk";
            this.ok.Tag = "ok";
            this.ok.Text = "确定";
            this.ok.Size = new System.Drawing.Size(92, 28);
            this.cancel.Name = "entryCancel";
            this.cancel.Tag = "cancel";
            this.cancel.Text = "取消";
            this.cancel.Size = new System.Drawing.Size(92, 28);
            this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.commands.Controls.Add(this.cancel);
            this.commands.Controls.Add(this.ok);
            this.Controls.Add(this.entries);
            this.Controls.Add(this.explanation);
            this.Controls.Add(this.stage);
            this.Controls.Add(this.commands);
            this.Name = "FlowEntryForm";
            this.Tag = "entrySelection";
            this.Text = "选择启动流程";
            this.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.ClientSize = new System.Drawing.Size(660, 480);
            this.MinimumSize = new System.Drawing.Size(500, 360);
            this.Padding = new System.Windows.Forms.Padding(12);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.AcceptButton = this.ok;
            this.CancelButton = this.cancel;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ResumeLayout(false);
        }
    }
}
