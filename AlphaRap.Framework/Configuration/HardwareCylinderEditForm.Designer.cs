using System.Drawing;
using System.Windows.Forms;

namespace AlphaRap.Configuration
{
    partial class HardwareCylinderEditForm
    {
        private Label lblName, lblOn, lblOff;
        private TextBox cylinderName;
        private ComboBox onOutput, offOutput;
        private Button btnOk, btnCancel;

        /// <summary>在设计器和运行时显示同一套名称、输出选择和确认布局。</summary>
        private void InitializeComponent()
        {
            var fields = new TableLayoutPanel();
            var commands = new FlowLayoutPanel();
            lblName = new Label();
            lblOn = new Label();
            lblOff = new Label();
            cylinderName = new TextBox();
            onOutput = new ComboBox();
            offOutput = new ComboBox();
            btnOk = new Button();
            btnCancel = new Button();
            SuspendLayout();
            fields.SuspendLayout();

            fields.Name = "fields";
            fields.Dock = DockStyle.Fill;
            fields.ColumnCount = 2;
            fields.RowCount = 3;
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 132));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            lblName.Name = "lblName";
            lblName.Text = "资源名称";
            lblName.TextAlign = ContentAlignment.MiddleLeft;
            lblName.Dock = DockStyle.Fill;
            lblOn.Name = "lblOn";
            lblOn.Text = "ON 输出";
            lblOn.TextAlign = ContentAlignment.MiddleLeft;
            lblOn.Dock = DockStyle.Fill;
            lblOff.Name = "lblOff";
            lblOff.Text = "OFF 输出";
            lblOff.TextAlign = ContentAlignment.MiddleLeft;
            lblOff.Dock = DockStyle.Fill;
            cylinderName.Name = "cylinderName";
            cylinderName.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            onOutput.Name = "onOutput";
            onOutput.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            onOutput.DropDownStyle = ComboBoxStyle.DropDownList;
            onOutput.DropDownWidth = 500;
            offOutput.Name = "offOutput";
            offOutput.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            offOutput.DropDownStyle = ComboBoxStyle.DropDownList;
            offOutput.DropDownWidth = 500;
            fields.Controls.Add(lblName, 0, 0);
            fields.Controls.Add(cylinderName, 1, 0);
            fields.Controls.Add(lblOn, 0, 1);
            fields.Controls.Add(onOutput, 1, 1);
            fields.Controls.Add(lblOff, 0, 2);
            fields.Controls.Add(offOutput, 1, 2);

            commands.Name = "commands";
            commands.Dock = DockStyle.Bottom;
            commands.Height = 42;
            commands.FlowDirection = FlowDirection.RightToLeft;
            btnOk.Name = "btnOk";
            btnOk.Text = "确定";
            btnOk.Size = new Size(92, 30);
            btnOk.Click += Confirm;
            btnCancel.Name = "btnCancel";
            btnCancel.Text = "取消";
            btnCancel.Size = new Size(92, 30);
            btnCancel.DialogResult = DialogResult.Cancel;
            commands.Controls.Add(btnCancel);
            commands.Controls.Add(btnOk);

            Name = "HardwareCylinderEditForm";
            Text = "气缸组合";
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(660, 188);
            Padding = new Padding(12);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            MinimizeBox = false;
            MaximizeBox = false;
            AcceptButton = btnOk;
            CancelButton = btnCancel;
            Controls.Add(fields);
            Controls.Add(commands);
            fields.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
