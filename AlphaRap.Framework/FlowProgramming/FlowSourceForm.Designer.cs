
namespace AlphaRap.FlowProgramming
{
    partial class FlowSourceForm
    {
        private System.Windows.Forms.TreeView tree;
        private System.Windows.Forms.Label contextHint;
        private System.Windows.Forms.Panel searchPanel;
        private System.Windows.Forms.Label searchLabel;
        private System.Windows.Forms.TextBox searchValue;
        private System.Windows.Forms.Label valueLabel;
        private System.Windows.Forms.TextBox value;
        private System.Windows.Forms.Label statusHint;
        private System.Windows.Forms.Button ok;
        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.FlowLayoutPanel buttons;
        private System.Windows.Forms.Panel footer;

        /// <summary>固定布局供设计器与运行时共用，所有绑定来源在同一窗口显示。</summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.tree = new System.Windows.Forms.TreeView();
            this.tree.Name = "sources";
            this.tree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tree.HideSelection = false;
            this.tree.FullRowSelect = true;
            this.tree.PathSeparator = " / ";
            this.contextHint = new System.Windows.Forms.Label();
            this.contextHint.Name = "sourceContext_dyn";
            this.contextHint.Dock = System.Windows.Forms.DockStyle.Top;
            this.contextHint.Height = 52;
            this.contextHint.Padding = new System.Windows.Forms.Padding(8, 5, 8, 4);
            this.contextHint.Text = "选择固定值、机器设置、当前配方或节点结果。";
            this.searchPanel = new System.Windows.Forms.Panel();
            this.searchPanel.SuspendLayout();
            this.searchPanel.Name = "searchPanel";
            this.searchPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.searchPanel.Height = 38;
            this.searchPanel.Padding = new System.Windows.Forms.Padding(8, 4, 8, 5);
            this.searchLabel = new System.Windows.Forms.Label();
            this.searchLabel.Name = "searchLabel";
            this.searchLabel.Tag = "bindingSearch";
            this.searchLabel.Text = "搜索名称";
            this.searchLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.searchLabel.Width = 115;
            this.searchLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.searchValue = new System.Windows.Forms.TextBox();
            this.searchValue.Name = "searchValue";
            this.searchValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.searchPanel.Controls.Add(this.searchValue);
            this.searchPanel.Controls.Add(this.searchLabel);
            this.footer = new System.Windows.Forms.Panel();
            this.footer.SuspendLayout();
            this.footer.Name = "footer";
            this.footer.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.footer.Height = 136;
            this.footer.Padding = new System.Windows.Forms.Padding(8);
            this.valueLabel = new System.Windows.Forms.Label();
            this.valueLabel.Name = "valueLabel";
            this.valueLabel.Tag = "bindingValue";
            this.valueLabel.Text = "当前选择 / 固定值";
            this.valueLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.valueLabel.Height = 24;
            this.value = new System.Windows.Forms.TextBox();
            this.value.Name = "referenceValue";
            this.value.Dock = System.Windows.Forms.DockStyle.Top;
            this.statusHint = new System.Windows.Forms.Label();
            this.statusHint.Name = "sourceStatus_dyn";
            this.statusHint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.statusHint.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.statusHint.ForeColor = System.Drawing.SystemColors.GrayText;
            this.buttons = new System.Windows.Forms.FlowLayoutPanel();
            this.buttons.SuspendLayout();
            this.buttons.Name = "buttons";
            this.buttons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttons.Height = 34;
            this.buttons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.ok = new System.Windows.Forms.Button();
            this.ok.Name = "ok";
            this.ok.Tag = "ok";
            this.ok.Text = "确定";
            this.ok.AutoSize = true;
            this.ok.MinimumSize = new System.Drawing.Size(80, 28);
            this.cancel = new System.Windows.Forms.Button();
            this.cancel.Name = "cancel";
            this.cancel.Tag = "cancel";
            this.cancel.Text = "取消";
            this.cancel.AutoSize = true;
            this.cancel.MinimumSize = new System.Drawing.Size(80, 28);
            this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.AcceptButton = this.ok;
            this.CancelButton = this.cancel;
            this.buttons.Controls.Add(this.cancel);
            this.buttons.Controls.Add(this.ok);
            this.footer.Controls.Add(this.statusHint);
            this.footer.Controls.Add(this.value);
            this.footer.Controls.Add(this.valueLabel);
            this.footer.Controls.Add(this.buttons);
            this.Controls.Add(this.tree);
            this.Controls.Add(this.searchPanel);
            this.Controls.Add(this.contextHint);
            this.Controls.Add(this.footer);
            this.Name = "FlowSourceForm";
            this.Tag = "bindingTitle";
            this.Text = "绑定数据";
            this.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(740, 560);
            this.MinimumSize = new System.Drawing.Size(700, 520);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.ShowInTaskbar = false;
            this.MaximizeBox = false;
            this.searchPanel.ResumeLayout(false);
            this.searchPanel.PerformLayout();
            this.footer.ResumeLayout(false);
            this.footer.PerformLayout();
            this.buttons.ResumeLayout(false);
            this.buttons.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}
