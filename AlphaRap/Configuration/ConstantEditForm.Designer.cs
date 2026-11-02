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
        private Label keyLabel, typeLabel, valueLabel, descriptionLabel;
        private FlowLayoutPanel buttons;

        /// <summary>固定编辑布局和值控件，供设计器与运行时共同使用。</summary>
        private void InitializeComponent()
        {
            this.layout = new TableLayoutPanel();
            this.layout.Name = "constantEditLayout";
            this.layout.Dock = DockStyle.Fill;
            this.layout.ColumnCount = 2;
            this.layout.RowCount = 5;
            this.layout.Padding = new Padding(12);
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,130)); layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
            this.layout.RowStyles.Add(new RowStyle(SizeType.Absolute,40));
            this.layout.RowStyles.Add(new RowStyle(SizeType.Absolute,40));
            this.layout.RowStyles.Add(new RowStyle(SizeType.Absolute,50));
            this.layout.RowStyles.Add(new RowStyle(SizeType.Absolute,90));
            this.layout.RowStyles.Add(new RowStyle(SizeType.Absolute,45));
            this.keyValue = new TextBox();
            this.keyValue.Name = "constantKeyValue";
            this.keyValue.Dock = DockStyle.Fill;
            this.typeValue = new ComboBox();
            this.typeValue.Name = "constantTypeValue";
            this.typeValue.DropDownStyle = ComboBoxStyle.DropDownList;
            this.typeValue.FormattingEnabled = true;
            this.typeValue.Dock = DockStyle.Fill;
            this.valueValue = new TextBox();
            this.valueValue.Name = "constantValueValue";
            this.valueValue.Dock = DockStyle.Fill;
            this.descriptionValue = new TextBox();
            this.descriptionValue.Name = "constantDescriptionValue";
            this.descriptionValue.Multiline = true;
            this.descriptionValue.Dock = DockStyle.Fill;
            this.keyLabel = new Label();
            this.keyLabel.Name = "constantKeyLabel";
            this.keyLabel.Tag = "key";
            this.keyLabel.Text = "常量键";
            this.keyLabel.Dock = DockStyle.Fill;
            layout.Controls.Add(keyLabel,0,0); layout.Controls.Add(keyValue,1,0);
            this.typeLabel = new Label();
            this.typeLabel.Name = "constantTypeLabel";
            this.typeLabel.Tag = "type";
            this.typeLabel.Text = "类型";
            this.typeLabel.Dock = DockStyle.Fill;
            layout.Controls.Add(typeLabel,0,1); layout.Controls.Add(typeValue,1,1);
            this.valueLabel = new Label();
            this.valueLabel.Name = "constantValueLabel";
            this.valueLabel.Tag = "value";
            this.valueLabel.Text = "值";
            this.valueLabel.Dock = DockStyle.Fill;
            layout.Controls.Add(valueLabel,0,2); layout.Controls.Add(valueValue,1,2);
            this.descriptionLabel = new Label();
            this.descriptionLabel.Name = "constantDescriptionLabel";
            this.descriptionLabel.Tag = "description";
            this.descriptionLabel.Text = "说明";
            this.descriptionLabel.Dock = DockStyle.Fill;
            layout.Controls.Add(descriptionLabel,0,3); layout.Controls.Add(descriptionValue,1,3);
            this.buttons = new FlowLayoutPanel();
            this.buttons.Name = "constantEditButtons";
            this.buttons.Dock = DockStyle.Fill;
            this.buttons.FlowDirection = FlowDirection.RightToLeft;
            this.ok = new Button();
            this.ok.Name = "constantOk";
            this.ok.Text = "确定";
            this.ok.AutoSize = true;
            this.cancel = new Button();
            this.cancel.Name = "constantCancel";
            this.cancel.Text = "取消";
            this.cancel.DialogResult = DialogResult.Cancel;
            this.cancel.AutoSize = true;
            buttons.Controls.Add(cancel); buttons.Controls.Add(ok); layout.Controls.Add(buttons,0,4); layout.SetColumnSpan(buttons,2);
            Controls.Add(layout); Name = "ConstantEditForm"; Tag = "title"; Text = "设计器常量";
            Font = new Font("Microsoft YaHei",9F); ClientSize = new Size(620,300); MinimumSize = new Size(600,340);
            AutoScaleMode = AutoScaleMode.None; StartPosition = FormStartPosition.CenterParent; MinimizeBox = false; MaximizeBox = false; AcceptButton = ok; CancelButton = cancel;
        }
    }
}
