using System.Drawing;
using System.Windows.Forms;
namespace AlphaRap.FlowProgramming
{
    partial class FlowSchemaForm
    {
        private DataGridView fields;
        private DataGridViewTextBoxColumn fieldName;
        private DataGridViewComboBoxColumn fieldType;
        private Button ok;
        private Button cancel;
        private FlowLayoutPanel buttons;
        /// <summary>Build the fixed layout shared by designer preview and runtime.</summary>
        private void InitializeComponent()
        {
            SuspendLayout();
            fields = new DataGridView();
            fields.Name = "productionFields";
            fields.Dock = DockStyle.Fill;
            fields.AutoGenerateColumns = false;
            fields.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            fields.AllowUserToAddRows = true;
            fields.AllowUserToDeleteRows = true;
            fieldName = new DataGridViewTextBoxColumn();
            fieldName.Name = "fieldName";
            fieldName.Tag = "name";
            fieldName.HeaderText = "字段";
            fieldType = new DataGridViewComboBoxColumn();
            fieldType.Name = "fieldType";
            fieldType.Tag = "fieldType";
            fieldType.HeaderText = "数据类型";
            fieldType.Items.AddRange("Text", "Number", "Boolean");
            fields.Columns.AddRange(fieldName, fieldType);
            buttons = new FlowLayoutPanel();
            buttons.Name = "buttons";
            buttons.Dock = DockStyle.Bottom;
            buttons.Height = 44;
            buttons.FlowDirection = FlowDirection.RightToLeft;
            buttons.Padding = new Padding(6);
            ok = new Button();
            ok.Name = "ok";
            ok.Tag = "ok";
            ok.Text = "确定";
            ok.AutoSize = true;
            cancel = new Button();
            cancel.Name = "cancel";
            cancel.Tag = "cancel";
            cancel.Text = "取消";
            cancel.AutoSize = true;
            cancel.DialogResult = DialogResult.Cancel;
            AcceptButton = ok;
            CancelButton = cancel;
            buttons.Controls.Add(cancel);
            buttons.Controls.Add(ok);
            Controls.Add(fields);
            Controls.Add(buttons);
            Name = "FlowSchemaForm";
            Tag = "title";
            Text = "流程工程";
            Font = new Font("Microsoft YaHei", 9F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(700, 500);
            MinimumSize = new Size(700, 500);
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            MaximizeBox = false;
            ResumeLayout(false);
        }
    }
}
