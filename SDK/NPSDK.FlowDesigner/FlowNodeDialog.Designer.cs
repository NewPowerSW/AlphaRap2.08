using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace NPSDK.Design
{
    partial class FlowNodeDialog
    {
        private IContainer components;
        private TableLayoutPanel layout;
        private PropertyGrid propertyGrid;
        private DataGridView parameterGrid;
        private DataGridViewTextBoxColumn nameColumn, valueColumn;
        private DataGridViewComboBoxColumn sourceColumn, typeColumn;
        private FlowLayoutPanel buttons;
        private Button okButton, cancelButton, addParameter, deleteParameter;
        private TextBox errorBox;
        private Label parameterHint;
        /// <summary>释放固定界面组件。</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>创建类似 SDK 节点属性窗口的固定布局。</summary>
        private void InitializeComponent()
        {
            components = new Container();
            layout = new TableLayoutPanel();
            propertyGrid = new PropertyGrid();
            parameterGrid = new DataGridView();
            nameColumn = new DataGridViewTextBoxColumn();
            typeColumn = new DataGridViewComboBoxColumn();
            sourceColumn = new DataGridViewComboBoxColumn();
            valueColumn = new DataGridViewTextBoxColumn();
            buttons = new FlowLayoutPanel();
            okButton = new Button();
            cancelButton = new Button();
            addParameter = new Button();
            deleteParameter = new Button();
            errorBox = new TextBox();
            parameterHint = new Label();
            SuspendLayout();
            layout.SuspendLayout();
            layout.Dock = DockStyle.Fill;
            layout.ColumnCount = 1;
            layout.RowCount = 5;
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 62));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            propertyGrid.Dock = DockStyle.Fill;
            propertyGrid.Name = "propertyGrid";
            propertyGrid.PropertySort = PropertySort.Categorized;
            parameterHint.Name = "parameterHint";
            parameterHint.Text = "参数与前驱数据绑定";
            parameterHint.Dock = DockStyle.Fill;
            parameterHint.Padding = new Padding(6);
            parameterGrid.Name = "parameterGrid";
            parameterGrid.Dock = DockStyle.Fill;
            parameterGrid.AllowUserToAddRows = false;
            parameterGrid.AllowUserToDeleteRows = false;
            parameterGrid.AutoGenerateColumns = false;
            parameterGrid.RowHeadersVisible = false;
            parameterGrid.BackgroundColor = SystemColors.Window;
            nameColumn.Name = "Name";
            nameColumn.HeaderText = "Name";
            nameColumn.Width = 130;
            typeColumn.Name = "Type";
            typeColumn.HeaderText = "Type";
            typeColumn.Width = 100;
            typeColumn.ReadOnly = true;
            typeColumn.Items.AddRange("String", "Boolean", "Int32", "Double");
            typeColumn.FlatStyle = FlatStyle.Flat;
            sourceColumn.Name = "Source";
            sourceColumn.HeaderText = "Source";
            sourceColumn.Width = 180;
            sourceColumn.FlatStyle = FlatStyle.Flat;
            valueColumn.Name = "Value";
            valueColumn.HeaderText = "Value";
            valueColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            parameterGrid.Columns.AddRange(nameColumn, typeColumn, sourceColumn, valueColumn);
            errorBox.Name = "errorBox_dyn";
            errorBox.Multiline = true;
            errorBox.ReadOnly = true;
            errorBox.Dock = DockStyle.Fill;
            buttons.Dock = DockStyle.Fill;
            buttons.FlowDirection = FlowDirection.RightToLeft;
            okButton.Name = "okButton";
            okButton.Text = "确定";
            okButton.Size = new Size(130, 32);
            cancelButton.Name = "cancelButton";
            cancelButton.Text = "取消";
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Size = new Size(130, 32);
            addParameter.Name = "addParameter";
            addParameter.Text = "增加保存字段";
            addParameter.AutoSize = true;
            addParameter.Height = 32;
            deleteParameter.Name = "deleteParameter";
            deleteParameter.Text = "删除保存字段";
            deleteParameter.AutoSize = true;
            deleteParameter.Height = 32;
            buttons.Controls.Add(cancelButton);
            buttons.Controls.Add(okButton);
            buttons.Controls.Add(addParameter);
            buttons.Controls.Add(deleteParameter);
            layout.Controls.Add(propertyGrid, 0, 0);
            layout.Controls.Add(parameterHint, 0, 1);
            layout.Controls.Add(parameterGrid, 0, 2);
            layout.Controls.Add(errorBox, 0, 3);
            layout.Controls.Add(buttons, 0, 4);
            AutoScaleDimensions = new SizeF(7, 15);
            AutoScaleMode = AutoScaleMode.Font;
            Font = new Font("Microsoft YaHei UI", 9);
            ClientSize = new Size(760, 850);
            MinimumSize = new Size(680, 720);
            Name = "FlowNodeDialog";
            Text = "Flow_Chart 节点属性";
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            CancelButton = cancelButton;
            Controls.Add(layout);
            layout.ResumeLayout(false);
            layout.PerformLayout();
            ResumeLayout(false);
        }
    }
}
