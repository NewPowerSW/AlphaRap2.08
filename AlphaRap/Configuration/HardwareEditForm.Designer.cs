using System.Drawing;
using System.Windows.Forms;

namespace AlphaRap.Configuration
{
    partial class HardwareEditForm
    {
        private DataGridView parameters;
        private DataGridViewTextBoxColumn property, value;
        private FlowLayoutPanel buttons;
        private Button ok,cancel;

        /// <summary>资源参数窗口使用固定表头及按钮布局。</summary>
        private void InitializeComponent()
        {
            this.parameters = new DataGridView();
            this.parameters.Name = "hardwareParameters";
            this.parameters.Dock = DockStyle.Fill;
            this.parameters.AutoGenerateColumns = false;
            this.parameters.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.parameters.AllowUserToAddRows = false;
            this.parameters.AllowUserToDeleteRows = false;
            this.parameters.RowHeadersVisible = false;
            this.property = new DataGridViewTextBoxColumn();
            this.property.Name = "property";
            this.property.Tag = "property";
            this.property.HeaderText = "参数";
            this.property.ReadOnly = true;
            this.property.SortMode = DataGridViewColumnSortMode.NotSortable;
            this.value = new DataGridViewTextBoxColumn();
            this.value.Name = "value";
            this.value.Tag = "value";
            this.value.HeaderText = "值";
            this.value.SortMode = DataGridViewColumnSortMode.NotSortable;
            parameters.Columns.AddRange(property,value);
            this.buttons = new FlowLayoutPanel();
            this.buttons.Name = "hardwareButtons";
            this.buttons.Dock = DockStyle.Bottom;
            this.buttons.Height = 44;
            this.buttons.FlowDirection = FlowDirection.RightToLeft;
            this.ok = new Button();
            this.ok.Name = "hardwareOk";
            this.ok.Text = "确定";
            this.ok.AutoSize = true;
            this.cancel = new Button();
            this.cancel.Name = "hardwareCancel";
            this.cancel.Text = "取消";
            this.cancel.AutoSize = true;
            this.cancel.DialogResult = DialogResult.Cancel;
            buttons.Controls.Add(cancel); buttons.Controls.Add(ok); Controls.Add(parameters); Controls.Add(buttons);
            Name = "HardwareEditForm"; Tag = "title"; Text = "硬件资源组合"; ClientSize = new Size(670,660); MinimumSize = new Size(640,520); Font = new Font("Microsoft YaHei",9F); StartPosition = FormStartPosition.CenterParent; AutoScaleMode = AutoScaleMode.None; AcceptButton = ok; CancelButton = cancel;
        }
    }
}
