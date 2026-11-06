using System.Drawing;
using System.Windows.Forms;

namespace AlphaRap.Configuration
{
    partial class ConstantEditForm
    {
        private TableLayoutPanel constantEditLayout;
        private TextBox constantKeyValue, constantValueValue, constantDescriptionValue;
        private ComboBox constantTypeValue;
        private Button constantOk, constantCancel;
        private Label constantKeyLabel, constantTypeLabel, constantValueLabel, constantDescriptionLabel;
        private FlowLayoutPanel constantEditButtons;

        /// <summary>固定编辑布局和值控件，供设计器与运行时共同使用。</summary>
        private void InitializeComponent()
        {
            this.constantEditLayout = new System.Windows.Forms.TableLayoutPanel();
            this.constantKeyLabel = new System.Windows.Forms.Label();
            this.constantKeyValue = new System.Windows.Forms.TextBox();
            this.constantTypeLabel = new System.Windows.Forms.Label();
            this.constantTypeValue = new System.Windows.Forms.ComboBox();
            this.constantValueLabel = new System.Windows.Forms.Label();
            this.constantValueValue = new System.Windows.Forms.TextBox();
            this.constantDescriptionLabel = new System.Windows.Forms.Label();
            this.constantDescriptionValue = new System.Windows.Forms.TextBox();
            this.constantEditButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.constantCancel = new System.Windows.Forms.Button();
            this.constantOk = new System.Windows.Forms.Button();
            this.constantEditLayout.SuspendLayout();
            this.constantEditButtons.SuspendLayout();
            this.SuspendLayout();
            //
            // constantEditLayout
            //
            this.constantEditLayout.ColumnCount = 2;
            this.constantEditLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this.constantEditLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.constantEditLayout.Controls.Add(this.constantKeyLabel, 0, 0);
            this.constantEditLayout.Controls.Add(this.constantKeyValue, 1, 0);
            this.constantEditLayout.Controls.Add(this.constantTypeLabel, 0, 1);
            this.constantEditLayout.Controls.Add(this.constantTypeValue, 1, 1);
            this.constantEditLayout.Controls.Add(this.constantValueLabel, 0, 2);
            this.constantEditLayout.Controls.Add(this.constantValueValue, 1, 2);
            this.constantEditLayout.Controls.Add(this.constantDescriptionLabel, 0, 3);
            this.constantEditLayout.Controls.Add(this.constantDescriptionValue, 1, 3);
            this.constantEditLayout.Controls.Add(this.constantEditButtons, 0, 4);
            this.constantEditLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.constantEditLayout.Location = new System.Drawing.Point(0, 0);
            this.constantEditLayout.Name = "constantEditLayout";
            this.constantEditLayout.Padding = new System.Windows.Forms.Padding(12);
            this.constantEditLayout.RowCount = 5;
            this.constantEditLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.constantEditLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.constantEditLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.constantEditLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.constantEditLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.constantEditLayout.Size = new System.Drawing.Size(620, 301);
            this.constantEditLayout.TabIndex = 0;
            //
            // constantKeyLabel
            //
            this.constantKeyLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.constantKeyLabel.Location = new System.Drawing.Point(15, 12);
            this.constantKeyLabel.Name = "constantKeyLabel";
            this.constantKeyLabel.Size = new System.Drawing.Size(124, 40);
            this.constantKeyLabel.TabIndex = 0;
            this.constantKeyLabel.Tag = "key";
            this.constantKeyLabel.Text = "常量键";
            //
            // constantKeyValue
            //
            this.constantKeyValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.constantKeyValue.Location = new System.Drawing.Point(145, 15);
            this.constantKeyValue.Name = "constantKeyValue";
            this.constantKeyValue.Size = new System.Drawing.Size(460, 23);
            this.constantKeyValue.TabIndex = 1;
            //
            // constantTypeLabel
            //
            this.constantTypeLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.constantTypeLabel.Location = new System.Drawing.Point(15, 52);
            this.constantTypeLabel.Name = "constantTypeLabel";
            this.constantTypeLabel.Size = new System.Drawing.Size(124, 40);
            this.constantTypeLabel.TabIndex = 2;
            this.constantTypeLabel.Tag = "type";
            this.constantTypeLabel.Text = "类型";
            //
            // constantTypeValue
            //
            this.constantTypeValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.constantTypeValue.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.constantTypeValue.FormattingEnabled = true;
            this.constantTypeValue.Location = new System.Drawing.Point(145, 55);
            this.constantTypeValue.Name = "constantTypeValue";
            this.constantTypeValue.Size = new System.Drawing.Size(460, 25);
            this.constantTypeValue.TabIndex = 3;
            //
            // constantValueLabel
            //
            this.constantValueLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.constantValueLabel.Location = new System.Drawing.Point(15, 92);
            this.constantValueLabel.Name = "constantValueLabel";
            this.constantValueLabel.Size = new System.Drawing.Size(124, 50);
            this.constantValueLabel.TabIndex = 4;
            this.constantValueLabel.Tag = "value";
            this.constantValueLabel.Text = "值";
            //
            // constantValueValue
            //
            this.constantValueValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.constantValueValue.Location = new System.Drawing.Point(145, 95);
            this.constantValueValue.Name = "constantValueValue";
            this.constantValueValue.Size = new System.Drawing.Size(460, 23);
            this.constantValueValue.TabIndex = 5;
            //
            // constantDescriptionLabel
            //
            this.constantDescriptionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.constantDescriptionLabel.Location = new System.Drawing.Point(15, 142);
            this.constantDescriptionLabel.Name = "constantDescriptionLabel";
            this.constantDescriptionLabel.Size = new System.Drawing.Size(124, 90);
            this.constantDescriptionLabel.TabIndex = 6;
            this.constantDescriptionLabel.Tag = "description";
            this.constantDescriptionLabel.Text = "说明";
            //
            // constantDescriptionValue
            //
            this.constantDescriptionValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.constantDescriptionValue.Location = new System.Drawing.Point(145, 145);
            this.constantDescriptionValue.Multiline = true;
            this.constantDescriptionValue.Name = "constantDescriptionValue";
            this.constantDescriptionValue.Size = new System.Drawing.Size(460, 84);
            this.constantDescriptionValue.TabIndex = 7;
            //
            // constantEditButtons
            //
            this.constantEditLayout.SetColumnSpan(this.constantEditButtons, 2);
            this.constantEditButtons.Controls.Add(this.constantCancel);
            this.constantEditButtons.Controls.Add(this.constantOk);
            this.constantEditButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.constantEditButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.constantEditButtons.Location = new System.Drawing.Point(15, 235);
            this.constantEditButtons.Name = "constantEditButtons";
            this.constantEditButtons.Size = new System.Drawing.Size(590, 51);
            this.constantEditButtons.TabIndex = 8;
            //
            // constantCancel
            //
            this.constantCancel.AutoSize = true;
            this.constantCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.constantCancel.Location = new System.Drawing.Point(512, 3);
            this.constantCancel.Name = "constantCancel";
            this.constantCancel.Size = new System.Drawing.Size(75, 27);
            this.constantCancel.TabIndex = 0;
            this.constantCancel.Text = "取消";
            //
            // constantOk
            //
            this.constantOk.AutoSize = true;
            this.constantOk.Location = new System.Drawing.Point(431, 3);
            this.constantOk.Name = "constantOk";
            this.constantOk.Size = new System.Drawing.Size(75, 27);
            this.constantOk.TabIndex = 1;
            this.constantOk.Text = "确定";
            //
            // ConstantEditForm
            //
            this.AcceptButton = this.constantOk;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this.constantCancel;
            this.ClientSize = new System.Drawing.Size(620, 301);
            this.Controls.Add(this.constantEditLayout);
            this.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(600, 340);
            this.Name = "ConstantEditForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "title";
            this.Text = "设计器常量";
            this.constantEditLayout.ResumeLayout(false);
            this.constantEditLayout.PerformLayout();
            this.constantEditButtons.ResumeLayout(false);
            this.constantEditButtons.PerformLayout();
            this.ResumeLayout(false);

        }
    }
}
