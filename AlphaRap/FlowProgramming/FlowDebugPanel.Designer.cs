namespace AlphaRap.FlowProgramming
{
    partial class FlowDebugPanel
    {
        private System.Windows.Forms.Label debugTitle;
        private System.Windows.Forms.Label debugHint;
        private System.Windows.Forms.DataGridView debugValues;
        private System.Windows.Forms.DataGridViewTextBoxColumn directionColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn fieldColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn valueColumn;
        private System.Windows.Forms.FlowLayoutPanel commands;
        private System.Windows.Forms.Button debugContinue;
        private System.Windows.Forms.Button debugStep;

        /// <summary>以固定控件呈现节点输入输出和单步命令，设计期不读取工程或设备。</summary>
        private void InitializeComponent()
        {
            this.debugTitle = new System.Windows.Forms.Label();
            this.debugHint = new System.Windows.Forms.Label();
            this.commands = new System.Windows.Forms.FlowLayoutPanel();
            this.debugContinue = new System.Windows.Forms.Button();
            this.debugStep = new System.Windows.Forms.Button();
            this.debugValues = new System.Windows.Forms.DataGridView();
            this.directionColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fieldColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.valueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.commands.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.debugValues)).BeginInit();
            this.SuspendLayout();
            //
            // debugTitle
            //
            this.debugTitle.AutoEllipsis = true;
            this.debugTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.debugTitle.Height = 28;
            this.debugTitle.Name = "debugTitle_dyn";
            this.debugTitle.Padding = new System.Windows.Forms.Padding(6, 4, 0, 0);
            this.debugTitle.Text = "节点调试";
            //
            // debugHint
            //
            this.debugHint.AutoEllipsis = true;
            this.debugHint.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.debugHint.Height = 34;
            this.debugHint.Name = "debugHelp";
            this.debugHint.Padding = new System.Windows.Forms.Padding(6, 2, 0, 0);
            this.debugHint.Tag = "debugHelp";
            this.debugHint.Text = "继续到下一个断点；单步执行一个节点。节点前暂停不计超时；停止沿用原停止按钮。";
            //
            // commands
            //
            this.commands.Controls.Add(this.debugContinue);
            this.commands.Controls.Add(this.debugStep);
            this.commands.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.commands.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.commands.Height = 36;
            this.commands.Name = "debugCommands";
            this.commands.Padding = new System.Windows.Forms.Padding(3);
            //
            // debugContinue
            //
            this.debugContinue.AutoSize = true;
            this.debugContinue.MinimumSize = new System.Drawing.Size(100, 27);
            this.debugContinue.Name = "debugContinue";
            this.debugContinue.Tag = "debugContinue";
            this.debugContinue.Text = "继续";
            //
            // debugStep
            //
            this.debugStep.AutoSize = true;
            this.debugStep.MinimumSize = new System.Drawing.Size(135, 27);
            this.debugStep.Name = "debugStep";
            this.debugStep.Tag = "debugStep";
            this.debugStep.Text = "单步执行节点";
            //
            // debugValues
            //
            this.debugValues.AllowUserToAddRows = false;
            this.debugValues.AllowUserToDeleteRows = false;
            this.debugValues.AllowUserToResizeRows = false;
            this.debugValues.AutoGenerateColumns = false;
            this.debugValues.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.debugValues.BackgroundColor = System.Drawing.SystemColors.Window;
            this.debugValues.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.debugValues.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.directionColumn,
                this.fieldColumn,
                this.valueColumn});
            this.debugValues.Dock = System.Windows.Forms.DockStyle.Fill;
            this.debugValues.Name = "debugValues";
            this.debugValues.ReadOnly = true;
            this.debugValues.RowHeadersVisible = false;
            //
            // directionColumn
            //
            this.directionColumn.FillWeight = 12F;
            this.directionColumn.HeaderText = "方向";
            this.directionColumn.Name = "direction";
            this.directionColumn.Tag = "debugDirection";
            //
            // fieldColumn
            //
            this.fieldColumn.FillWeight = 28F;
            this.fieldColumn.HeaderText = "字段";
            this.fieldColumn.Name = "field";
            this.fieldColumn.Tag = "debugField";
            //
            // valueColumn
            //
            this.valueColumn.FillWeight = 60F;
            this.valueColumn.HeaderText = "本次值";
            this.valueColumn.Name = "value";
            this.valueColumn.Tag = "debugValue";
            //
            // FlowDebugPanel
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.debugValues);
            this.Controls.Add(this.debugTitle);
            this.Controls.Add(this.commands);
            this.Controls.Add(this.debugHint);
            this.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.Name = "FlowDebugPanel";
            this.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.Size = new System.Drawing.Size(1000, 235);
            this.commands.ResumeLayout(false);
            this.commands.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.debugValues)).EndInit();
            this.ResumeLayout(false);
        }
    }
}