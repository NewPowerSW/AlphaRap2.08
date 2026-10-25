namespace AlphaRap.FlowProgramming
{
    partial class FlowConditionRow
    {
        private System.Windows.Forms.TableLayoutPanel layout;
        private System.Windows.Forms.ComboBox leftValue;
        private System.Windows.Forms.ComboBox comparisonValue;
        private System.Windows.Forms.ComboBox rightValue;

        /// <summary>创建左值、比较方式和右值的固定行布局。</summary>
        private void InitializeComponent()
        {
            this.layout = new System.Windows.Forms.TableLayoutPanel();
            this.leftValue = new System.Windows.Forms.ComboBox();
            this.comparisonValue = new System.Windows.Forms.ComboBox();
            this.rightValue = new System.Windows.Forms.ComboBox();
            this.layout.SuspendLayout();
            this.SuspendLayout();
            //
            // layout
            //
            this.layout.ColumnCount = 3;
            this.layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
            this.layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.layout.Controls.Add(this.leftValue, 0, 0);
            this.layout.Controls.Add(this.comparisonValue, 1, 0);
            this.layout.Controls.Add(this.rightValue, 2, 0);
            this.layout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layout.Name = "conditionLayout";
            this.layout.RowCount = 1;
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            //
            // leftValue
            //
            this.leftValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.leftValue.DropDownHeight = 300;
            this.leftValue.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.leftValue.IntegralHeight = false;
            this.leftValue.MaxDropDownItems = 16;
            this.leftValue.Name = "conditionLeft";
            //
            // comparisonValue
            //
            this.comparisonValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comparisonValue.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comparisonValue.Name = "conditionComparison";
            //
            // rightValue
            //
            this.rightValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightValue.DropDownHeight = 300;
            this.rightValue.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.rightValue.IntegralHeight = false;
            this.rightValue.MaxDropDownItems = 16;
            this.rightValue.Name = "conditionRight";
            //
            // FlowConditionRow
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.layout);
            this.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.MinimumSize = new System.Drawing.Size(560, 36);
            this.Name = "FlowConditionRow";
            this.Size = new System.Drawing.Size(900, 36);
            this.layout.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}