namespace AlphaRap.FlowProgramming
{
    partial class FlowToolboxControl
    {
        private System.Windows.Forms.TableLayoutPanel layout;
        private System.Windows.Forms.Label title;
        private System.Windows.Forms.Label searchLabel;
        private System.Windows.Forms.TextBox search;
        private System.Windows.Forms.TreeView entries;
        private System.Windows.Forms.Label help;
        private System.Windows.Forms.Label emptyHint;

        /// <summary>工具箱固定搜索、分组列表和操作提示，设计期无需载入模块。</summary>
        private void InitializeComponent()
        {
            this.layout = new System.Windows.Forms.TableLayoutPanel();
            this.title = new System.Windows.Forms.Label();
            this.searchLabel = new System.Windows.Forms.Label();
            this.search = new System.Windows.Forms.TextBox();
            this.entries = new System.Windows.Forms.TreeView();
            this.help = new System.Windows.Forms.Label();
            this.emptyHint = new System.Windows.Forms.Label();
            this.layout.SuspendLayout();
            this.SuspendLayout();
            this.layout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layout.ColumnCount = 1;
            this.layout.RowCount = 6;
            this.layout.Padding = new System.Windows.Forms.Padding(6);
            this.layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 55F));
            this.title.Name = "toolboxTitle";
            this.title.Text = "功能工具箱";
            this.title.Dock = System.Windows.Forms.DockStyle.Fill;
            this.title.Font = new System.Drawing.Font("Microsoft YaHei", 10F, System.Drawing.FontStyle.Bold);
            this.searchLabel.Name = "toolboxSearchLabel";
            this.searchLabel.Text = "搜索";
            this.searchLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.search.Name = "toolboxSearch";
            this.search.Dock = System.Windows.Forms.DockStyle.Fill;
            this.entries.Name = "toolboxEntries";
            this.entries.Dock = System.Windows.Forms.DockStyle.Fill;
            this.entries.HideSelection = false;
            this.entries.FullRowSelect = true;
            this.entries.ShowLines = false;
            this.entries.ShowRootLines = false;
            this.entries.ItemHeight = 28;
            this.help.Name = "toolboxHint";
            this.help.Text = "拖到画布；也可双击或按 Enter 添加。";
            this.help.Dock = System.Windows.Forms.DockStyle.Fill;
            this.help.ForeColor = System.Drawing.SystemColors.GrayText;
            this.emptyHint.Name = "toolboxEmpty";
            this.emptyHint.Text = "没有匹配的已注册功能。";
            this.emptyHint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.emptyHint.AutoEllipsis = true;
            this.layout.Controls.Add(this.title, 0, 0);
            this.layout.Controls.Add(this.searchLabel, 0, 1);
            this.layout.Controls.Add(this.search, 0, 2);
            this.layout.Controls.Add(this.entries, 0, 3);
            this.layout.Controls.Add(this.emptyHint, 0, 4);
            this.layout.Controls.Add(this.help, 0, 5);
            this.Controls.Add(this.layout);
            this.Name = "FlowToolboxControl";
            this.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.Size = new System.Drawing.Size(280, 650);
            this.MinimumSize = new System.Drawing.Size(220, 240);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.layout.ResumeLayout(false);
            this.layout.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}
