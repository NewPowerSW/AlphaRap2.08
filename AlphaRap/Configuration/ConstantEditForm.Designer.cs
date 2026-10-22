using System.Drawing;
using System.Windows.Forms;

namespace AlphaRap.Configuration
{
    partial class ConstantEditForm
    {
        private TableLayoutPanel layout;
        private TextBox keyValue, valueValue, descriptionValue;
        private ComboBox typeValue;
        private Button ok, cancel;

        /// <summary>固定编辑布局和值控件，供设计器与运行时共同使用。</summary>
        private void InitializeComponent()
        {
            layout = new TableLayoutPanel { Name = "constantEditLayout", Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 5, Padding = new Padding(12) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,130)); layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
            foreach (int height in new[] { 40,40,50,90,45 }) layout.RowStyles.Add(new RowStyle(SizeType.Absolute,height));
            keyValue = new TextBox { Name = "constantKeyValue", Dock = DockStyle.Fill };
            typeValue = new ComboBox { Name = "constantTypeValue", DropDownStyle = ComboBoxStyle.DropDownList, FormattingEnabled = true, Dock = DockStyle.Fill };
            valueValue = new TextBox { Name = "constantValueValue", Dock = DockStyle.Fill };
            descriptionValue = new TextBox { Name = "constantDescriptionValue", Multiline = true, Dock = DockStyle.Fill };
            layout.Controls.Add(new Label { Name = "constantKeyLabel", Tag = "key", Text = "常量键", Dock = DockStyle.Fill },0,0); layout.Controls.Add(keyValue,1,0);
            layout.Controls.Add(new Label { Name = "constantTypeLabel", Tag = "type", Text = "类型", Dock = DockStyle.Fill },0,1); layout.Controls.Add(typeValue,1,1);
            layout.Controls.Add(new Label { Name = "constantValueLabel", Tag = "value", Text = "值", Dock = DockStyle.Fill },0,2); layout.Controls.Add(valueValue,1,2);
            layout.Controls.Add(new Label { Name = "constantDescriptionLabel", Tag = "description", Text = "说明", Dock = DockStyle.Fill },0,3); layout.Controls.Add(descriptionValue,1,3);
            FlowLayoutPanel buttons = new FlowLayoutPanel { Name = "constantEditButtons", Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            ok = new Button { Name = "constantOk", Text = "确定", AutoSize = true };
            cancel = new Button { Name = "constantCancel", Text = "取消", DialogResult = DialogResult.Cancel, AutoSize = true };
            buttons.Controls.Add(cancel); buttons.Controls.Add(ok); layout.Controls.Add(buttons,0,4); layout.SetColumnSpan(buttons,2);
            Controls.Add(layout); Name = "ConstantEditForm"; Tag = "title"; Text = "设计器常量";
            Font = new Font("Microsoft YaHei",9F); ClientSize = new Size(620,300); MinimumSize = new Size(600,340);
            AutoScaleMode = AutoScaleMode.None; StartPosition = FormStartPosition.CenterParent; MinimizeBox = false; MaximizeBox = false; AcceptButton = ok; CancelButton = cancel;
        }
    }
}
