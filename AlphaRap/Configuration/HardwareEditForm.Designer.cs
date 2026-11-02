using System.Drawing;
using System.Windows.Forms;

namespace AlphaRap.Configuration
{
    partial class HardwareEditForm
    {
        private DataGridView hardwareParameters;
        private DataGridViewTextBoxColumn property, value;
        private FlowLayoutPanel hardwareButtons;
        private Button hardwareOk,hardwareCancel;

        /// <summary>资源参数窗口使用固定表头及按钮布局。</summary>
        private void InitializeComponent()
        {
            this.hardwareParameters = new DataGridView();
            this.hardwareParameters.Name = "hardwareParameters";
            this.hardwareParameters.Dock = DockStyle.Fill;
            this.hardwareParameters.AutoGenerateColumns = false;
            this.hardwareParameters.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.hardwareParameters.AllowUserToAddRows = false;
            this.hardwareParameters.AllowUserToDeleteRows = false;
            this.hardwareParameters.RowHeadersVisible = false;
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
            hardwareParameters.Columns.AddRange(property,value);
            this.hardwareButtons = new FlowLayoutPanel();
            this.hardwareButtons.Name = "hardwareButtons";
            this.hardwareButtons.Dock = DockStyle.Bottom;
            this.hardwareButtons.Height = 44;
            this.hardwareButtons.FlowDirection = FlowDirection.RightToLeft;
            this.hardwareOk = new Button();
            this.hardwareOk.Name = "hardwareOk";
            this.hardwareOk.Text = "确定";
            this.hardwareOk.AutoSize = true;
            this.hardwareCancel = new Button();
            this.hardwareCancel.Name = "hardwareCancel";
            this.hardwareCancel.Text = "取消";
            this.hardwareCancel.AutoSize = true;
            this.hardwareCancel.DialogResult = DialogResult.Cancel;
            hardwareButtons.Controls.Add(hardwareCancel); hardwareButtons.Controls.Add(hardwareOk); Controls.Add(hardwareParameters); Controls.Add(hardwareButtons);
            Name = "HardwareEditForm"; Tag = "title"; Text = "硬件资源组合"; ClientSize = new Size(670,660); MinimumSize = new Size(640,520); Font = new Font("Microsoft YaHei",9F); StartPosition = FormStartPosition.CenterParent; AutoScaleMode = AutoScaleMode.None; AcceptButton = hardwareOk; CancelButton = hardwareCancel;
        }
    }
}
