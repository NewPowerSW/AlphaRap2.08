using System.Drawing;
using System.Windows.Forms;

namespace AlphaRap.FlowProgramming
{
    partial class FlowScriptEditor
    {
        private TableLayoutPanel scriptLayout, toolbar;
        private Label sourceLabel, help;
        private Button compile;
        private CheckBox sourceWrap;
        private TableLayoutPanel sourceLayout;
        private Panel lineNumbers;
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
            toolbar = new TableLayoutPanel { Name = "scriptToolbar", Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1 };
            toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180)); toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
            toolbar.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            sourceLabel = new Label { Name = "scriptCode", Tag = "scriptCode", Text = "源码", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            compile = new Button { Name = "scriptCompile", Tag = "scriptCompile", Text = "编译检查", AutoSize = true, Dock = DockStyle.Right, MinimumSize = new Size(130, 27) };
            sourceWrap = new CheckBox { Name = "scriptWordWrap", Tag = "scriptWordWrap", Text = "自动换行", Checked = true, AutoSize = true, Dock = DockStyle.Fill };
            toolbar.Controls.Add(sourceLabel, 0, 0); toolbar.Controls.Add(sourceWrap, 1, 0); toolbar.Controls.Add(compile, 2, 0);
            sourceLayout = new TableLayoutPanel { Name = "scriptSourceLayout", Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, CellBorderStyle = TableLayoutPanelCellBorderStyle.None, Margin = new Padding(3) };
            sourceLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50)); sourceLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); sourceLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            lineNumbers = new Panel { Name = "scriptLineNumbers", Dock = DockStyle.Fill, BackColor = Color.FromArgb(244, 246, 248), Margin = new Padding(0), Font = new Font("Consolas", 11F) };
            source = new RichTextBox { Name = "scriptSource", Dock = DockStyle.Fill, Font = new Font("Consolas", 11F), AcceptsTab = true, WordWrap = true, Text = "Outputs.Value = Inputs.Value;", Margin = new Padding(0), BorderStyle = BorderStyle.None, ScrollBars = RichTextBoxScrollBars.Both, DetectUrls = false, HideSelection = false };
            sourceLayout.Controls.Add(lineNumbers, 0, 0); sourceLayout.Controls.Add(source, 1, 0);
            errors = new TextBox { Name = "scriptErrors_dyn", Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, WordWrap = false, ScrollBars = ScrollBars.Both, Font = new Font("Consolas", 9F), Visible = false };
            help = new Label { Name = "scriptCodeHelp", Tag = "scriptCodeHelp", Text = "Inputs.名称 读取输入；Outputs.名称 返回输出。", Dock = DockStyle.Fill, ForeColor = SystemColors.GrayText, AutoEllipsis = true, Padding = new Padding(0, 4, 0, 0) };
            scriptLayout.Controls.Add(toolbar, 0, 0); scriptLayout.Controls.Add(sourceLayout, 0, 1); scriptLayout.Controls.Add(errors, 0, 2); scriptLayout.Controls.Add(help, 0, 3);
            Controls.Add(scriptLayout); Name = "FlowScriptEditor"; Size = new Size(950, 420); MinimumSize = new Size(500, 220);
            Font = new Font("Microsoft YaHei", 9F); AutoScaleMode = AutoScaleMode.Dpi;
            ResumeLayout(false);
        }
    }
}
