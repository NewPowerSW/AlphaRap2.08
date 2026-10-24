using System.Drawing;
using System.Windows.Forms;

namespace AlphaRap.FlowProgramming
{
    partial class FlowDebugPanel
    {
        private Label debugTitle, debugHint;
        private DataGridView debugValues;
        private FlowLayoutPanel commands;
        private Button debugContinue, debugStep;

        /// <summary>以固定控件呈现节点输入输出和单步命令，设计期不读取工程或设备。</summary>
        private void InitializeComponent()
        {
            SuspendLayout();
            debugTitle = new Label { Name = "debugTitle_dyn", Dock = DockStyle.Top, Height = 28, Text = "节点调试", Padding = new Padding(6, 4, 0, 0), AutoEllipsis = true };
            debugHint = new Label { Name = "debugHelp", Tag = "debugHelp", Dock = DockStyle.Bottom, Height = 34, Text = "继续到下一个断点；单步执行一个节点。节点前暂停不计超时；停止沿用原停止按钮。", AutoEllipsis = true, Padding = new Padding(6, 2, 0, 0) };
            commands = new FlowLayoutPanel { Name = "debugCommands", Dock = DockStyle.Bottom, Height = 36, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(3) };
            debugContinue = new Button { Name = "debugContinue", Tag = "debugContinue", Text = "继续", AutoSize = true, MinimumSize = new Size(100, 27) };
            debugStep = new Button { Name = "debugStep", Tag = "debugStep", Text = "单步执行节点", AutoSize = true, MinimumSize = new Size(135, 27) };
            commands.Controls.Add(debugContinue); commands.Controls.Add(debugStep);
            debugValues = new DataGridView { Name = "debugValues", Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = false,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false, AllowUserToResizeRows = false, RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = SystemColors.Window, BorderStyle = BorderStyle.FixedSingle };
            debugValues.Columns.Add(new DataGridViewTextBoxColumn { Name = "direction", HeaderText = "方向", Tag = "debugDirection", FillWeight = 12 });
            debugValues.Columns.Add(new DataGridViewTextBoxColumn { Name = "field", HeaderText = "字段", Tag = "debugField", FillWeight = 28 });
            debugValues.Columns.Add(new DataGridViewTextBoxColumn { Name = "value", HeaderText = "本次值", Tag = "debugValue", FillWeight = 60 });
            Controls.Add(debugValues); Controls.Add(debugTitle); Controls.Add(commands); Controls.Add(debugHint);
            Name = "FlowDebugPanel"; Font = new Font("Microsoft YaHei", 9F); Size = new Size(1000, 235);
            AutoScaleMode = AutoScaleMode.None; Padding = new Padding(6, 0, 6, 0);
            ResumeLayout(false);
        }
    }
}
