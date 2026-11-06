
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
            this.tree = new System.Windows.Forms.TreeView();
            this.contextHint = new System.Windows.Forms.Label();
            this.searchPanel = new System.Windows.Forms.Panel();
            this.searchValue = new System.Windows.Forms.TextBox();
            this.searchLabel = new System.Windows.Forms.Label();
            this.footer = new System.Windows.Forms.Panel();
            this.statusHint = new System.Windows.Forms.Label();
            this.value = new System.Windows.Forms.TextBox();
            this.valueLabel = new System.Windows.Forms.Label();
            this.buttons = new System.Windows.Forms.FlowLayoutPanel();
            this.cancel = new System.Windows.Forms.Button();
            this.ok = new System.Windows.Forms.Button();
            this.searchPanel.SuspendLayout();
            this.footer.SuspendLayout();
            this.buttons.SuspendLayout();
            this.SuspendLayout();
            // 
            // tree
            // 
            this.tree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tree.FullRowSelect = true;
            this.tree.HideSelection = false;
            this.tree.Location = new System.Drawing.Point(0, 90);
            this.tree.Name = "sources";
            this.tree.PathSeparator = " / ";
            this.tree.Size = new System.Drawing.Size(740, 334);
            this.tree.TabIndex = 0;
            // 
            // contextHint
            // 
            this.contextHint.Dock = System.Windows.Forms.DockStyle.Top;
            this.contextHint.Location = new System.Drawing.Point(0, 0);
            this.contextHint.Name = "sourceContext_dyn";
            this.contextHint.Padding = new System.Windows.Forms.Padding(8, 5, 8, 4);
            this.contextHint.Size = new System.Drawing.Size(740, 52);
            this.contextHint.TabIndex = 2;
            this.contextHint.Text = "选择固定值、机器设置、当前配方或节点结果。";
            // 
            // searchPanel
            // 
            this.searchPanel.Controls.Add(this.searchValue);
            this.searchPanel.Controls.Add(this.searchLabel);
            this.searchPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.searchPanel.Location = new System.Drawing.Point(0, 52);
            this.searchPanel.Name = "searchPanel";
            this.searchPanel.Padding = new System.Windows.Forms.Padding(8, 4, 8, 5);
            this.searchPanel.Size = new System.Drawing.Size(740, 38);
            this.searchPanel.TabIndex = 1;
            // 
            // searchValue
            // 
            this.searchValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.searchValue.Location = new System.Drawing.Point(123, 4);
            this.searchValue.Name = "searchValue";
            this.searchValue.Size = new System.Drawing.Size(609, 23);
            this.searchValue.TabIndex = 0;
            // 
            // searchLabel
            // 
            this.searchLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.searchLabel.Location = new System.Drawing.Point(8, 4);
            this.searchLabel.Name = "searchLabel";
            this.searchLabel.Size = new System.Drawing.Size(115, 29);
            this.searchLabel.TabIndex = 1;
            this.searchLabel.Tag = "bindingSearch";
            this.searchLabel.Text = "搜索名称";
            this.searchLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // footer
            // 
            this.footer.Controls.Add(this.statusHint);
            this.footer.Controls.Add(this.value);
            this.footer.Controls.Add(this.valueLabel);
            this.footer.Controls.Add(this.buttons);
            this.footer.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.footer.Location = new System.Drawing.Point(0, 424);
            this.footer.Name = "footer";
            this.footer.Padding = new System.Windows.Forms.Padding(8);
            this.footer.Size = new System.Drawing.Size(740, 136);
            this.footer.TabIndex = 3;
            // 
            // statusHint
            // 
            this.statusHint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.statusHint.ForeColor = System.Drawing.SystemColors.GrayText;
            this.statusHint.Location = new System.Drawing.Point(8, 55);
            this.statusHint.Name = "sourceStatus_dyn";
            this.statusHint.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.statusHint.Size = new System.Drawing.Size(724, 39);
            this.statusHint.TabIndex = 0;
            // 
            // value
            // 
            this.value.Dock = System.Windows.Forms.DockStyle.Top;
            this.value.Location = new System.Drawing.Point(8, 32);
            this.value.Name = "referenceValue";
            this.value.Size = new System.Drawing.Size(724, 23);
            this.value.TabIndex = 1;
            // 
            // valueLabel
            // 
            this.valueLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.valueLabel.Location = new System.Drawing.Point(8, 8);
            this.valueLabel.Name = "valueLabel";
            this.valueLabel.Size = new System.Drawing.Size(724, 24);
            this.valueLabel.TabIndex = 2;
            this.valueLabel.Tag = "bindingValue";
            this.valueLabel.Text = "当前选择 / 固定值";
            // 
            // buttons
            // 
            this.buttons.Controls.Add(this.cancel);
            this.buttons.Controls.Add(this.ok);
            this.buttons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.buttons.Location = new System.Drawing.Point(8, 94);
            this.buttons.Name = "buttons";
            this.buttons.Size = new System.Drawing.Size(724, 34);
            this.buttons.TabIndex = 3;
            // 
            // cancel
            // 
            this.cancel.AutoSize = true;
            this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancel.Location = new System.Drawing.Point(641, 3);
            this.cancel.MinimumSize = new System.Drawing.Size(80, 28);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(80, 28);
            this.cancel.TabIndex = 0;
            this.cancel.Tag = "cancel";
            this.cancel.Text = "取消";
            // 
            // ok
            // 
            this.ok.AutoSize = true;
            this.ok.Location = new System.Drawing.Point(555, 3);
            this.ok.MinimumSize = new System.Drawing.Size(80, 28);
            this.ok.Name = "ok";
            this.ok.Size = new System.Drawing.Size(80, 28);
            this.ok.TabIndex = 1;
            this.ok.Tag = "ok";
            this.ok.Text = "确定";
            // 
            // FlowSourceForm
            // 
            this.AcceptButton = this.ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.cancel;
            this.ClientSize = new System.Drawing.Size(740, 560);
            this.Controls.Add(this.tree);
            this.Controls.Add(this.searchPanel);
            this.Controls.Add(this.contextHint);
            this.Controls.Add(this.footer);
            this.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(700, 520);
            this.Name = "FlowSourceForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "bindingTitle";
            this.Text = "绑定数据";
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
