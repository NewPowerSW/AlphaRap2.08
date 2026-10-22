using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace NPSDK.Design
{
    partial class FlowDesignerForm
    {
        private IContainer components;
        private FlowDesignerCanvas canvas;
        private ListBox toolbox;
        private ComboBox entryBox, languageBox;
        private TextBox outputBox, statusBox;
        private Label selectedLabel;
        private Button exampleButton, openButton, saveButton, validateButton, runButton, pauseButton, stopButton, editButton, deleteButton, instancesButton, folderButton;
        private DataGridView traceGrid, resultGrid;
        private Timer monitor;
        /// <summary>释放计时器和固定界面组件。</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>建立可在设计器预览的工具箱、SDK 画布、结果和运行工具栏。</summary>
        private void InitializeComponent()
        {
            components = new Container();
            monitor = new Timer(components)
            {
                Interval = 80
            };
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 70));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 30));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };
            exampleButton = new Button
            {
                Name = "exampleButton",
                Text = "示例",
                AutoSize = true
            };
            openButton = new Button
            {
                Name = "openButton",
                Text = "打开",
                AutoSize = true
            };
            saveButton = new Button
            {
                Name = "saveButton",
                Text = "保存",
                AutoSize = true
            };
            validateButton = new Button
            {
                Name = "validateButton",
                Text = "检查",
                AutoSize = true
            };
            runButton = new Button
            {
                Name = "runButton",
                Text = "运行",
                AutoSize = true
            };
            pauseButton = new Button
            {
                Name = "pauseButton",
                Text = "暂停 / 继续",
                AutoSize = true,
                Enabled = false
            };
            stopButton = new Button
            {
                Name = "stopButton",
                Text = "停止",
                AutoSize = true,
                Enabled = false
            };
            editButton = new Button
            {
                Name = "editButton",
                Text = "节点属性",
                AutoSize = true
            };
            deleteButton = new Button
            {
                Name = "deleteButton",
                Text = "删除节点",
                AutoSize = true
            };
            instancesButton = new Button
            {
                Name = "instancesButton",
                Text = "示例实例参数",
                AutoSize = true
            };
            folderButton = new Button
            {
                Name = "folderButton",
                Text = "记录目录",
                AutoSize = true
            };
            entryBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 115
            };
            languageBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 110
            };
            toolbar.Controls.AddRange(new Control[] { exampleButton, openButton, saveButton, validateButton, runButton, pauseButton, stopButton, editButton, deleteButton, instancesButton, folderButton, new Label { Name = "entryLabel", Text = "入口", AutoSize = true, Padding = new Padding(2, 6, 2, 0) }, entryBox, languageBox });
            var split = new SplitContainer
            {
                Size = new Size(1300, 500),
                Dock = DockStyle.Fill,
                FixedPanel = FixedPanel.Panel1,
                SplitterDistance = 230
            };
            var tools = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1
            };
            tools.RowStyles.Add(new RowStyle(SizeType.Absolute, 110));
            tools.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tools.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            tools.Controls.Add(new Label { Name = "canvasHint", Text = "拖出功能创建节点；拖动节点调整位置；双击配置动作、条件及跳转。", Dock = DockStyle.Fill, Padding = new Padding(8) }, 0, 0);
            toolbox = new ListBox
            {
                Dock = DockStyle.Fill,
                IntegralHeight = false
            };
            tools.Controls.Add(toolbox, 0, 1);
            selectedLabel = new Label
            {
                Name = "selected_dyn",
                Dock = DockStyle.Fill,
                AutoEllipsis = true
            };
            tools.Controls.Add(selectedLabel, 0, 2);
            canvas = new FlowDesignerCanvas
            {
                Dock = DockStyle.Fill,
                Name = "canvas",
                BackColor = Color.FromArgb(245, 248, 252)
            };
            split.Panel1.Controls.Add(tools);
            split.Panel2.Controls.Add(canvas);
            var tabs = new TabControl
            {
                Dock = DockStyle.Fill
            };
            var trace = new TabPage
            {
                Name = "tracePage",
                Text = "运行轨迹"
            };
            var results = new TabPage
            {
                Name = "resultsPage",
                Text = "本次输出"
            };
            traceGrid = new DataGridView
            {
                Name = "traceGrid",
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            traceGrid.Columns.Add("Node", "Node");
            traceGrid.Columns.Add("State", "State");
            traceGrid.Columns.Add("Alarm", "Alarm");
            traceGrid.Columns.Add("Detail", "Detail");
            resultGrid = new DataGridView
            {
                Name = "resultGrid",
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            resultGrid.Columns.Add("Node", "Node");
            resultGrid.Columns.Add("Field", "Field");
            resultGrid.Columns.Add("Value", "Value");
            trace.Controls.Add(traceGrid);
            results.Controls.Add(resultGrid);
            tabs.TabPages.AddRange(new[] { trace, results });
            var output = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2
            };
            output.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            output.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            output.Controls.Add(new Label { Name = "outputLabel", Text = "本地记录目录", Dock = DockStyle.Fill }, 0, 0);
            outputBox = new TextBox
            {
                Dock = DockStyle.Fill
            };
            output.Controls.Add(outputBox, 1, 0);
            statusBox = new TextBox
            {
                Name = "status_dyn",
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Multiline = true
            };
            layout.Controls.Add(toolbar, 0, 0);
            layout.Controls.Add(split, 0, 1);
            layout.Controls.Add(tabs, 0, 2);
            layout.Controls.Add(output, 0, 3);
            layout.Controls.Add(statusBox, 0, 4);
            AutoScaleDimensions = new SizeF(7, 15);
            AutoScaleMode = AutoScaleMode.Font;
            Font = new Font("Microsoft YaHei UI", 9);
            Name = "FlowDesignerForm";
            Text = "NPSDK Flow_Chart 设计器";
            ClientSize = new Size(1350, 850);
            MinimumSize = new Size(1050, 700);
            StartPosition = FormStartPosition.CenterScreen;
            Controls.Add(layout);
        }
    }
}
