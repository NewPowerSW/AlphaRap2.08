using System.Drawing;
using System.Windows.Forms;

namespace AlphaRap.FlowProgramming
{
    partial class FlowInputRow
    {
        private System.ComponentModel.IContainer components;
        private TableLayoutPanel layout;
        private Label fieldLabel, unitLabel;
        private Panel editorPanel;
        private TextBox literalValue, referenceValue;
        private ComboBox optionValue;
        private Button bindData;
        private ContextMenuStrip valueMenu;
        private ToolStripMenuItem useLiteral;
        private ToolTip helpTip;

        /// <summary>固定参数行布局供设计器和运行时共用，只在运行时绑定不同字段。</summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            SuspendLayout();
            layout = new TableLayoutPanel { Name = "inputLayout", Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, Padding = new Padding(4, 2, 4, 2), Margin = new Padding(0) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 176));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 138));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            fieldLabel = new Label { Name = "inputField_dyn", Text = "目标点位", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill, Margin = new Padding(3, 0, 6, 0), AutoEllipsis = true };
            unitLabel = new Label { Name = "inputUnit_dyn", TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill, Margin = new Padding(4, 0, 0, 0), AutoEllipsis = true };
            editorPanel = new Panel { Name = "inputEditor", Dock = DockStyle.Fill, Padding = new Padding(0, 5, 0, 0), Margin = new Padding(0) };
            literalValue = new TextBox { Name = "inputLiteral", Dock = DockStyle.Top, Text = "0" };
            optionValue = new ComboBox { Name = "inputOption", Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList, IntegralHeight = false, MaxDropDownItems = 16, DropDownHeight = 300, Visible = false };
            referenceValue = new TextBox { Name = "inputReference_dyn", Dock = DockStyle.Top, ReadOnly = true, Visible = false, BackColor = SystemColors.Window };
            bindData = new Button { Name = "inputBind", Tag = "inputBind", Text = "绑定数据", Dock = DockStyle.Fill, Margin = new Padding(5, 2, 0, 2), UseVisualStyleBackColor = true };
            valueMenu = new ContextMenuStrip(components) { Name = "inputValueMenu" };
            useLiteral = new ToolStripMenuItem { Name = "inputUseLiteral", Tag = "inputUseLiteral", Text = "使用固定值" };
            valueMenu.Items.Add(useLiteral);
            helpTip = new ToolTip(components) { ShowAlways = true, AutoPopDelay = 15000 };
            editorPanel.Controls.Add(referenceValue);
            editorPanel.Controls.Add(optionValue);
            editorPanel.Controls.Add(literalValue);
            layout.Controls.Add(fieldLabel, 0, 0);
            layout.Controls.Add(editorPanel, 1, 0);
            layout.Controls.Add(unitLabel, 2, 0);
            layout.Controls.Add(bindData, 3, 0);
            Controls.Add(layout);
            ContextMenuStrip = valueMenu;
            editorPanel.ContextMenuStrip = valueMenu;
            literalValue.ContextMenuStrip = valueMenu;
            optionValue.ContextMenuStrip = valueMenu;
            referenceValue.ContextMenuStrip = valueMenu;
            Name = "FlowInputRow";
            Font = new Font("Microsoft YaHei", 9F);
            AutoScaleMode = AutoScaleMode.None;
            Size = new Size(900, 44);
            MinimumSize = new Size(560, 44);
            Margin = new Padding(0, 1, 0, 1);
            ResumeLayout(false);
        }

        /// <summary>释放行内提示和菜单资源。</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }
    }
}
