namespace AlphaRap.FlowProgramming
{
    partial class FlowCodeRoutesControl
    {
        private System.Windows.Forms.TableLayoutPanel layout;
        private System.Windows.Forms.Label help;
        private System.Windows.Forms.Label nextLabel;
        private System.Windows.Forms.ComboBox nextTarget;
        private System.Windows.Forms.Label case1Label;
        private System.Windows.Forms.ComboBox case1Target;
        private System.Windows.Forms.Label case2Label;
        private System.Windows.Forms.ComboBox case2Target;
        private System.Windows.Forms.Label case3Label;
        private System.Windows.Forms.ComboBox case3Target;

        /// <summary>固定 NEXT 与 CASE 去向模板，不包含外层条件编辑器。</summary>
        private void InitializeComponent()
        {
            this.layout = new System.Windows.Forms.TableLayoutPanel();
            this.help = new System.Windows.Forms.Label();
            this.nextLabel = new System.Windows.Forms.Label();
            this.nextTarget = new System.Windows.Forms.ComboBox();
            this.case1Label = new System.Windows.Forms.Label();
            this.case1Target = new System.Windows.Forms.ComboBox();
            this.case2Label = new System.Windows.Forms.Label();
            this.case2Target = new System.Windows.Forms.ComboBox();
            this.case3Label = new System.Windows.Forms.Label();
            this.case3Target = new System.Windows.Forms.ComboBox();
            this.layout.SuspendLayout();
            this.SuspendLayout();
            this.layout.Dock = System.Windows.Forms.DockStyle.Top;
            this.layout.Height = 168;
            this.layout.ColumnCount = 2;
            this.layout.RowCount = 4;
            this.layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.nextLabel.Name = "codenextLabel";
            this.nextLabel.Text = "NEXT";
            this.nextLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nextLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.nextTarget.Name = "codenextTarget";
            this.nextTarget.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nextTarget.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.layout.Controls.Add(this.nextLabel, 0, 0);
            this.layout.Controls.Add(this.nextTarget, 1, 0);
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.case1Label.Name = "codecase1Label";
            this.case1Label.Text = "CASE1";
            this.case1Label.Dock = System.Windows.Forms.DockStyle.Fill;
            this.case1Label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.case1Target.Name = "codecase1Target";
            this.case1Target.Dock = System.Windows.Forms.DockStyle.Fill;
            this.case1Target.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.layout.Controls.Add(this.case1Label, 0, 1);
            this.layout.Controls.Add(this.case1Target, 1, 1);
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.case2Label.Name = "codecase2Label";
            this.case2Label.Text = "CASE2";
            this.case2Label.Dock = System.Windows.Forms.DockStyle.Fill;
            this.case2Label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.case2Target.Name = "codecase2Target";
            this.case2Target.Dock = System.Windows.Forms.DockStyle.Fill;
            this.case2Target.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.layout.Controls.Add(this.case2Label, 0, 2);
            this.layout.Controls.Add(this.case2Target, 1, 2);
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.case3Label.Name = "codecase3Label";
            this.case3Label.Text = "CASE3";
            this.case3Label.Dock = System.Windows.Forms.DockStyle.Fill;
            this.case3Label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.case3Target.Name = "codecase3Target";
            this.case3Target.Dock = System.Windows.Forms.DockStyle.Fill;
            this.case3Target.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.layout.Controls.Add(this.case3Label, 0, 3);
            this.layout.Controls.Add(this.case3Target, 1, 3);
            this.help.Name = "codeRoutesHelp";
            this.help.Text = "模块代码返回 NEXT / CASE；这里只选择去向，不再重复编辑判定条件。";
            this.help.Dock = System.Windows.Forms.DockStyle.Top;
            this.help.Height = 64;
            this.help.Padding = new System.Windows.Forms.Padding(6);
            this.help.ForeColor = System.Drawing.SystemColors.GrayText;
            this.Controls.Add(this.help);
            this.Controls.Add(this.layout);
            this.Name = "FlowCodeRoutesControl";
            this.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.Size = new System.Drawing.Size(930, 240);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.layout.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
