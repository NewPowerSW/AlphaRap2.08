using System.Drawing;
using System.Windows.Forms;

namespace AlphaRap.FlowProgramming
{
    partial class FlowScriptEditor
    {
        private TableLayoutPanel scriptLayout, toolbar;
        private Label sourceLabel, help;
        private Button compile;
        private RichTextBox source;
        private TextBox errors;

        /// <summary>固定源码编辑布局；诊断为空时将其行高收起。</summary>
        private void InitializeComponent()
        {
            SuspendLayout();
            scriptLayout = new TableLayoutPanel { Name = "scriptLayout", Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(4) };
            scriptLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            scriptLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            scriptLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 0));
            scriptLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            toolbar = new TableLayoutPanel { Name = "scriptToolbar", Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
            toolbar.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            sourceLabel = new Label { Name = "scriptCode", Tag = "scriptCode", Text = "源码", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            compile = new Button { Name = "scriptCompile", Tag = "scriptCompile", Text = "编译检查", AutoSize = true, Dock = DockStyle.Right, MinimumSize = new Size(130, 27) };
            toolbar.Controls.Add(sourceLabel, 0, 0); toolbar.Controls.Add(compile, 1, 0);
            source = new RichTextBox { Name = "scriptSource", Dock = DockStyle.Fill, Font = new Font("Consolas", 10F), AcceptsTab = true, WordWrap = false, Text = "Outputs.Value = Inputs.Value;" };
            errors = new TextBox { Name = "scriptErrors_dyn", Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, WordWrap = false, ScrollBars = ScrollBars.Both, Font = new Font("Consolas", 9F), Visible = false };
            help = new Label { Name = "scriptCodeHelp", Tag = "scriptCodeHelp", Text = "Inputs.名称 读取输入；Outputs.名称 返回输出。", Dock = DockStyle.Fill, ForeColor = SystemColors.GrayText, AutoEllipsis = true, Padding = new Padding(0, 4, 0, 0) };
            scriptLayout.Controls.Add(toolbar, 0, 0); scriptLayout.Controls.Add(source, 0, 1); scriptLayout.Controls.Add(errors, 0, 2); scriptLayout.Controls.Add(help, 0, 3);
            Controls.Add(scriptLayout); Name = "FlowScriptEditor"; Size = new Size(950, 420); MinimumSize = new Size(500, 220);
            Font = new Font("Microsoft YaHei", 9F); AutoScaleMode = AutoScaleMode.Dpi;
            ResumeLayout(false);
        }
    }
}
