namespace AlphaRap.FlowProgramming
{
    partial class FlowSourceViewer
    {
        private System.Windows.Forms.Label sourcePath;
        private System.Windows.Forms.SplitContainer workspace;
        private System.Windows.Forms.ListBox files;
        private System.Windows.Forms.RichTextBox source;
        private System.Windows.Forms.Button close;

        /// <summary>固定文件列表与只读源码显示，设计期不读取磁盘。</summary>
        private void InitializeComponent()
        {
            this.sourcePath = new System.Windows.Forms.Label();
            this.workspace = new System.Windows.Forms.SplitContainer();
            this.files = new System.Windows.Forms.ListBox();
            this.source = new System.Windows.Forms.RichTextBox();
            this.close = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.workspace)).BeginInit();
            this.workspace.Panel1.SuspendLayout();
            this.workspace.Panel2.SuspendLayout();
            this.SuspendLayout();
            this.sourcePath.Name = "sourcePath_dyn";
            this.sourcePath.Dock = System.Windows.Forms.DockStyle.Top;
            this.sourcePath.Height = 32;
            this.sourcePath.AutoEllipsis = true;
            this.workspace.Name = "sourceWorkspace";
            this.workspace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.workspace.Size = new System.Drawing.Size(1080, 600);
            this.workspace.SplitterDistance = 270;
            this.workspace.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.files.Name = "sourceFiles_dyn";
            this.files.Dock = System.Windows.Forms.DockStyle.Fill;
            this.files.IntegralHeight = false;
            this.files.HorizontalScrollbar = true;
            this.source.Name = "sourceCode_dyn";
            this.source.Dock = System.Windows.Forms.DockStyle.Fill;
            this.source.ReadOnly = true;
            this.source.DetectUrls = false;
            this.source.WordWrap = false;
            this.source.Font = new System.Drawing.Font("Consolas", 10F);
            this.source.BackColor = System.Drawing.Color.White;
            this.source.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Both;
            this.workspace.Panel1.Controls.Add(this.files);
            this.workspace.Panel2.Controls.Add(this.source);
            this.close.Name = "sourceClose";
            this.close.Tag = "close";
            this.close.Text = "关闭";
            this.close.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.close.Height = 32;
            this.close.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Controls.Add(this.workspace);
            this.Controls.Add(this.sourcePath);
            this.Controls.Add(this.close);
            this.Name = "FlowSourceViewer";
            this.Tag = "sourceReadOnly";
            this.Text = "生成源码（只读）";
            this.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.ClientSize = new System.Drawing.Size(1100, 700);
            this.MinimumSize = new System.Drawing.Size(720, 480);
            this.Padding = new System.Windows.Forms.Padding(10);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.MinimizeBox = false;
            this.CancelButton = this.close;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.workspace.Panel1.ResumeLayout(false);
            this.workspace.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.workspace)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
