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
            parameters = new DataGridView { Name = "hardwareParameters", Dock = DockStyle.Fill, AutoGenerateColumns = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false, RowHeadersVisible = false };
            property = new DataGridViewTextBoxColumn { Name = "property",Tag = "property",HeaderText = "参数", ReadOnly = true, SortMode = DataGridViewColumnSortMode.NotSortable };
            value = new DataGridViewTextBoxColumn { Name = "value",Tag = "value",HeaderText = "值", SortMode = DataGridViewColumnSortMode.NotSortable };
            parameters.Columns.AddRange(property,value);
            buttons = new FlowLayoutPanel { Name = "hardwareButtons", Dock = DockStyle.Bottom, Height = 44, FlowDirection = FlowDirection.RightToLeft };
            ok = new Button { Name = "hardwareOk",Text = "确定",AutoSize = true }; cancel = new Button { Name = "hardwareCancel",Text = "取消",AutoSize = true,DialogResult = DialogResult.Cancel };
            buttons.Controls.Add(cancel); buttons.Controls.Add(ok); Controls.Add(parameters); Controls.Add(buttons);
            Name = "HardwareEditForm"; Tag = "title"; Text = "硬件资源组合"; ClientSize = new Size(670,660); MinimumSize = new Size(640,520); Font = new Font("Microsoft YaHei",9F); StartPosition = FormStartPosition.CenterParent; AutoScaleMode = AutoScaleMode.None; AcceptButton = ok; CancelButton = cancel;
        }
    }
}
