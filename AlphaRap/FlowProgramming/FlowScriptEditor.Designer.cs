using System.Drawing;
using System.Windows.Forms;

namespace AlphaRap.FlowProgramming
{
    partial class FlowScriptEditor
    {
        private TableLayoutPanel scriptLayout, portsLayout;
        private Label help, sourceLabel, debugHelp;
        private DataGridView inputPorts, outputPorts;
        private FlowLayoutPanel inputTools, outputTools, compileTools;
        private Button addInput, addOutput, compile, insertCall;
        private ComboBox objectChoice, methodChoice;
        private RichTextBox source;
        private TextBox errors;
        private ContextMenuStrip portMenu;
        private ToolStripMenuItem deletePort;
        private ComboBox resourceBindings;
        private ContextMenuStrip bindingMenu;
        private ToolStripMenuItem removeBinding;

        /// <summary>建立固定源码、端口和编译信息布局，设计期不加载对象或执行脚本。</summary>
        private void InitializeComponent()
        {
            SuspendLayout();
            scriptLayout = new TableLayoutPanel { Name = "scriptLayout", Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 7, Padding = new Padding(4) };
            scriptLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            scriptLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
            scriptLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            scriptLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            scriptLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            scriptLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
            scriptLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            help = new Label { Name = "scriptHelp", Tag = "scriptHelp", Dock = DockStyle.Fill, Text = "用 Inputs.名称 读取绑定值，用 Outputs.名称 输出结果。流程跳转仍在 NEXT / CASE 设置。" };
            portsLayout = new TableLayoutPanel { Name = "portsLayout", Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
            portsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); portsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            portsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30)); portsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            inputTools = new FlowLayoutPanel { Name = "inputTools", Dock = DockStyle.Fill };
            outputTools = new FlowLayoutPanel { Name = "outputTools", Dock = DockStyle.Fill };
            inputTools.Controls.Add(new Label { Name = "scriptInputs", Tag = "scriptInputs", Text = "输入端口", Width = 140, Padding = new Padding(0, 5, 0, 0) });
            outputTools.Controls.Add(new Label { Name = "scriptOutputs", Tag = "scriptOutputs", Text = "输出端口", Width = 140, Padding = new Padding(0, 5, 0, 0) });
            addInput = new Button { Name = "scriptAddInput", Tag = "scriptAddInput", Text = "+ 输入", AutoSize = true };
            addOutput = new Button { Name = "scriptAddOutput", Tag = "scriptAddOutput", Text = "+ 输出", AutoSize = true };
            inputTools.Controls.Add(addInput); outputTools.Controls.Add(addOutput);
            inputPorts = new DataGridView { Name = "scriptInputPorts", Dock = DockStyle.Fill, AutoGenerateColumns = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false, RowHeadersWidth = 24, MultiSelect = false };
            outputPorts = new DataGridView { Name = "scriptOutputPorts", Dock = DockStyle.Fill, AutoGenerateColumns = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false, RowHeadersWidth = 24, MultiSelect = false };
            inputPorts.Columns.AddRange(new DataGridViewTextBoxColumn { Name = "portName", Tag = "scriptName", HeaderText = "名称（C# 标识符）", FillWeight = 45 }, new DataGridViewComboBoxColumn { Name = "portType", Tag = "scriptType", HeaderText = "类型", FillWeight = 25 }, new DataGridViewTextBoxColumn { Name = "portDefault", Tag = "scriptDefault", HeaderText = "默认值", FillWeight = 30 });
            outputPorts.Columns.AddRange(new DataGridViewTextBoxColumn { Name = "portName", Tag = "scriptName", HeaderText = "名称（C# 标识符）", FillWeight = 45 }, new DataGridViewComboBoxColumn { Name = "portType", Tag = "scriptType", HeaderText = "类型", FillWeight = 25 }, new DataGridViewTextBoxColumn { Name = "portDefault", Tag = "scriptDefault", HeaderText = "默认值", FillWeight = 30 });
            outputPorts.Columns["portDefault"].Visible = false;
            portsLayout.Controls.Add(inputTools, 0, 0); portsLayout.Controls.Add(outputTools, 1, 0); portsLayout.Controls.Add(inputPorts, 0, 1); portsLayout.Controls.Add(outputPorts, 1, 1);
            compileTools = new FlowLayoutPanel { Name = "compileTools", Dock = DockStyle.Fill, WrapContents = false };
            objectChoice = new ComboBox { Name = "scriptObjectChoice", DropDownStyle = ComboBoxStyle.DropDownList, Width = 210 };
            methodChoice = new ComboBox { Name = "scriptMethodChoice", DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
            insertCall = new Button { Name = "scriptInsert", Tag = "scriptInsert", Text = "插入对象方法", AutoSize = true };
            compile = new Button { Name = "scriptCompile", Tag = "scriptCompile", Text = "编译检查", AutoSize = true };
            compileTools.Controls.Add(objectChoice); compileTools.Controls.Add(methodChoice); compileTools.Controls.Add(insertCall); compileTools.Controls.Add(compile);
            sourceLabel = new Label { Name = "scriptCode", Tag = "scriptCode", Text = "源码", Dock = DockStyle.Fill };
            FlowLayoutPanel sourceTools = new FlowLayoutPanel { Name = "sourceTools", Dock = DockStyle.Fill, WrapContents = false };
            sourceLabel.Dock = DockStyle.None; sourceLabel.Width = 110; sourceLabel.Padding = new Padding(0, 5, 0, 0);
            sourceTools.Controls.Add(sourceLabel);
            sourceTools.Controls.Add(new Label { Name = "scriptBoundObjects", Tag = "scriptBoundObjects", Text = "已绑定对象", Width = 130, Padding = new Padding(0, 5, 0, 0) });
            resourceBindings = new ComboBox { Name = "scriptBindings_dyn", Width = 390, DropDownStyle = ComboBoxStyle.DropDownList };
            bindingMenu = new ContextMenuStrip(); removeBinding = new ToolStripMenuItem { Name = "scriptRemoveObject", Tag = "scriptRemoveObject", Text = "移除此对象绑定" }; bindingMenu.Items.Add(removeBinding); resourceBindings.ContextMenuStrip = bindingMenu;
            sourceTools.Controls.Add(resourceBindings);
            source = new RichTextBox { Name = "scriptSource", Dock = DockStyle.Fill, Font = new Font("Consolas", 10F), AcceptsTab = true, WordWrap = false, Text = "Outputs.Value = Inputs.Value;" };
            errors = new TextBox { Name = "scriptErrors_dyn", Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, WordWrap = false, ScrollBars = ScrollBars.Both, Font = new Font("Consolas", 9F) };
            debugHelp = new Label { Name = "scriptDebugHelp", Tag = "scriptDebugHelp", Dock = DockStyle.Fill, Text = "节点断点可检查输入输出；C# 行级调试可使用 Visual Studio 附加到 AlphaRap。保存统一使用主界面 Save。", ForeColor = SystemColors.GrayText };
            portMenu = new ContextMenuStrip(); deletePort = new ToolStripMenuItem { Name = "scriptDeletePort", Tag = "scriptDeletePort", Text = "删除端口" }; portMenu.Items.Add(deletePort);
            inputPorts.ContextMenuStrip = portMenu; outputPorts.ContextMenuStrip = portMenu;
            scriptLayout.Controls.Add(help, 0, 0); scriptLayout.Controls.Add(portsLayout, 0, 1); scriptLayout.Controls.Add(compileTools, 0, 2); scriptLayout.Controls.Add(sourceTools, 0, 3); scriptLayout.Controls.Add(source, 0, 4); scriptLayout.Controls.Add(errors, 0, 5); scriptLayout.Controls.Add(debugHelp, 0, 6);
            Controls.Add(scriptLayout); Name = "FlowScriptEditor"; Size = new Size(950, 420); MinimumSize = new Size(600, 320); Font = new Font("Microsoft YaHei", 9F); AutoScaleMode = AutoScaleMode.Dpi;
            ResumeLayout(false);
        }
    }
}
