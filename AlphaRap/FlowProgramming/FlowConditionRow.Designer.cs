using System.Drawing;
using System.Windows.Forms;

namespace AlphaRap.FlowProgramming
{
    partial class FlowConditionRow
    {
        private TableLayoutPanel layout;
        private ComboBox leftValue, comparisonValue, rightValue;

        /// <summary>一行直接编辑左值、比较和右值，不弹出逐级配置窗口。</summary>
        private void InitializeComponent()
        {
            layout = new TableLayoutPanel { Name = "conditionLayout", Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,160));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));
            leftValue = new ComboBox { Name = "conditionLeft", Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDown, MaxDropDownItems = 16, IntegralHeight = false, DropDownHeight = 300 };
            comparisonValue = new ComboBox { Name = "conditionComparison", Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            rightValue = new ComboBox { Name = "conditionRight", Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDown, MaxDropDownItems = 16, IntegralHeight = false, DropDownHeight = 300 };
            layout.Controls.Add(leftValue,0,0); layout.Controls.Add(comparisonValue,1,0); layout.Controls.Add(rightValue,2,0);
            Controls.Add(layout);
            Name = "FlowConditionRow"; Size = new Size(900,36); MinimumSize = new Size(560,36);
            Font = new Font("Microsoft YaHei",9F); AutoScaleMode = AutoScaleMode.None; Margin = new Padding(0,2,0,2);
        }
    }
}
