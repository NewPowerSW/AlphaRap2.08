using System.Drawing;
using System.Windows.Forms;

namespace AlphaRap.FlowProgramming
{
    partial class FlowScriptSettings
    {
        private TableLayoutPanel settingsLayout, portsLayout, callLayout, bindingLayout;
        private FlowLayoutPanel inputTools, outputTools;
        private DataGridView inputPorts, outputPorts;
        private Button addInput, addOutput, insertCall;
        private ComboBox objectChoice, methodChoice, resourceBindings;
        private Label settingsHint;
        private ContextMenuStrip portMenu, bindingMenu;
        private ToolStripMenuItem deletePort, removeBinding;

        /// <summary>固定端口与可选对象调用布局，可在设计器中独立预览。</summary>
        private void InitializeComponent()
        {
            SuspendLayout();
            settingsLayout = new TableLayoutPanel { Name = "scriptSettingsLayout", Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(4) };
            settingsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            settingsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            settingsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            settingsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            portsLayout = new TableLayoutPanel { Name = "portsLayout", Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
            portsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); portsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            portsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32)); portsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            inputTools = new FlowLayoutPanel { Name = "inputTools", Dock = DockStyle.Fill, WrapContents = false };
            outputTools = new FlowLayoutPanel { Name = "outputTools", Dock = DockStyle.Fill, WrapContents = false };
            inputTools.Controls.Add(new Label { Name = "scriptInputs", Tag = "scriptInputs", Text = "输入端口", Width = 140, Padding = new Padding(0, 5, 0, 0) });
            outputTools.Controls.Add(new Label { Name = "scriptOutputs", Tag = "scriptOutputs", Text = "输出端口", Width = 140, Padding = new Padding(0, 5, 0, 0) });
            addInput = new Button { Name = "scriptAddInput", Tag = "scriptAddInput", Text = "+ 输入", AutoSize = true };
            addOutput = new Button { Name = "scriptAddOutput", Tag = "scriptAddOutput", Text = "+ 输出", AutoSize = true };
            inputTools.Controls.Add(addInput); outputTools.Controls.Add(addOutput);
            inputPorts = new DataGridView { Name = "scriptInputPorts", Dock = DockStyle.Fill, AutoGenerateColumns = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false, RowHeadersWidth = 24, MultiSelect = false };
            outputPorts = new DataGridView { Name = "scriptOutputPorts", Dock = DockStyle.Fill, AutoGenerateColumns = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false, RowHeadersWidth = 24, MultiSelect = false };
            inputPorts.Columns.AddRange(new DataGridViewTextBoxColumn { Name = "portName", Tag = "scriptName", HeaderText = "名称（C# 标识符）", FillWeight = 45 }, new DataGridViewComboBoxColumn { Name = "portType", Tag = "scriptType", HeaderText = "类型", FillWeight = 25 }, new DataGridViewTextBoxColumn { Name = "portDefault", Tag = "scriptDefault", HeaderText = "默认值", FillWeight = 30 });
            outputPorts.Columns.AddRange(new DataGridViewTextBoxColumn { Name = "portName", Tag = "scriptName", HeaderText = "名称（C# 标识符）", FillWeight = 60 }, new DataGridViewComboBoxColumn { Name = "portType", Tag = "scriptType", HeaderText = "类型", FillWeight = 40 }, new DataGridViewTextBoxColumn { Name = "portDefault", Tag = "scriptDefault", HeaderText = "默认值", Visible = false });
            inputPorts.Rows.Add("Value", "Number", "0"); outputPorts.Rows.Add("Value", "Number", "0");
            portsLayout.Controls.Add(inputTools, 0, 0); portsLayout.Controls.Add(outputTools, 1, 0); portsLayout.Controls.Add(inputPorts, 0, 1); portsLayout.Controls.Add(outputPorts, 1, 1);
            callLayout = new TableLayoutPanel { Name = "scriptCallLayout", Dock = DockStyle.Fill, ColumnCount = 5, RowCount = 1 };
            callLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 65)); callLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46));
            callLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 65)); callLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54)); callLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
            callLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            objectChoice = new ComboBox { Name = "scriptObjectChoice", DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill, Anchor = AnchorStyles.Left | AnchorStyles.Right, FormattingEnabled = true };
            methodChoice = new ComboBox { Name = "scriptMethodChoice", DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill, Anchor = AnchorStyles.Left | AnchorStyles.Right, FormattingEnabled = true };
            insertCall = new Button { Name = "scriptInsert", Tag = "scriptInsert", Text = "插入对象方法", AutoSize = true, Dock = DockStyle.Fill, Enabled = false };
            callLayout.Controls.Add(new Label { Name = "scriptObjectLabel", Tag = "scriptObjectLabel", Text = "对象", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
            callLayout.Controls.Add(objectChoice, 1, 0);
            callLayout.Controls.Add(new Label { Name = "scriptMethodLabel", Tag = "scriptMethodLabel", Text = "方法", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 2, 0);
            callLayout.Controls.Add(methodChoice, 3, 0); callLayout.Controls.Add(insertCall, 4, 0);
            bindingLayout = new TableLayoutPanel { Name = "scriptBindingLayout", Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            bindingLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170)); bindingLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            bindingLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            bindingLayout.Controls.Add(new Label { Name = "scriptBoundObjects", Tag = "scriptBoundObjects", Text = "已绑定对象", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
            resourceBindings = new ComboBox { Name = "scriptBindings_dyn", DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
            bindingLayout.Controls.Add(resourceBindings, 1, 0);
            settingsHint = new Label { Name = "scriptSettingsHint_dyn", Text = "先定义输入输出；需要调用实际对象时再选择对象和方法。", Dock = DockStyle.Fill, ForeColor = SystemColors.GrayText, AutoEllipsis = true };
            portMenu = new ContextMenuStrip(); deletePort = new ToolStripMenuItem { Name = "scriptDeletePort", Tag = "scriptDeletePort", Text = "删除端口" }; portMenu.Items.Add(deletePort);
            bindingMenu = new ContextMenuStrip(); removeBinding = new ToolStripMenuItem { Name = "scriptRemoveObject", Tag = "scriptRemoveObject", Text = "移除此对象绑定" }; bindingMenu.Items.Add(removeBinding);
            inputPorts.ContextMenuStrip = portMenu; outputPorts.ContextMenuStrip = portMenu; resourceBindings.ContextMenuStrip = bindingMenu;
            settingsLayout.Controls.Add(portsLayout, 0, 0); settingsLayout.Controls.Add(callLayout, 0, 1); settingsLayout.Controls.Add(bindingLayout, 0, 2); settingsLayout.Controls.Add(settingsHint, 0, 3);
            Controls.Add(settingsLayout); Name = "FlowScriptSettings"; Size = new Size(950, 260); MinimumSize = new Size(600, 240);
            Font = new Font("Microsoft YaHei", 9F); AutoScaleMode = AutoScaleMode.Dpi;
            ResumeLayout(false);
        }
    }
}
