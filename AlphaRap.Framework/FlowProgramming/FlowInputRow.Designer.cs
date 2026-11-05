namespace AlphaRap.FlowProgramming
{
    partial class FlowInputRow
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TableLayoutPanel layout;
        private System.Windows.Forms.Label fieldLabel;
        private System.Windows.Forms.Label unitLabel;
        private System.Windows.Forms.Panel editorPanel;
        private System.Windows.Forms.TextBox literalValue;
        private System.Windows.Forms.TextBox referenceValue;
        private System.Windows.Forms.ComboBox optionValue;
        private System.Windows.Forms.Button bindData;
        private System.Windows.Forms.ContextMenuStrip valueMenu;
        private System.Windows.Forms.ToolStripMenuItem useLiteral;
        private System.Windows.Forms.ToolTip helpTip;

        /// <summary>固定参数行布局供设计器和运行时共用，只在运行时绑定不同字段。</summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.layout = new System.Windows.Forms.TableLayoutPanel();
            this.fieldLabel = new System.Windows.Forms.Label();
            this.unitLabel = new System.Windows.Forms.Label();
            this.editorPanel = new System.Windows.Forms.Panel();
            this.literalValue = new System.Windows.Forms.TextBox();
            this.optionValue = new System.Windows.Forms.ComboBox();
            this.referenceValue = new System.Windows.Forms.TextBox();
            this.bindData = new System.Windows.Forms.Button();
            this.valueMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.useLiteral = new System.Windows.Forms.ToolStripMenuItem();
            this.helpTip = new System.Windows.Forms.ToolTip(this.components);
            this.layout.SuspendLayout();
            this.editorPanel.SuspendLayout();
            this.valueMenu.SuspendLayout();
            this.SuspendLayout();
            //
            // layout
            //
            this.layout.ColumnCount = 4;
            this.layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 176F));
            this.layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 48F));
            this.layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 138F));
            this.layout.Controls.Add(this.fieldLabel, 0, 0);
            this.layout.Controls.Add(this.editorPanel, 1, 0);
            this.layout.Controls.Add(this.unitLabel, 2, 0);
            this.layout.Controls.Add(this.bindData, 3, 0);
            this.layout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layout.Margin = new System.Windows.Forms.Padding(0);
            this.layout.Name = "inputLayout";
            this.layout.Padding = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.layout.RowCount = 1;
            this.layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            //
            // fieldLabel
            //
            this.fieldLabel.AutoEllipsis = true;
            this.fieldLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fieldLabel.Margin = new System.Windows.Forms.Padding(3, 0, 6, 0);
            this.fieldLabel.Name = "inputField_dyn";
            this.fieldLabel.Text = "目标点位";
            this.fieldLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // unitLabel
            //
            this.unitLabel.AutoEllipsis = true;
            this.unitLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.unitLabel.Margin = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.unitLabel.Name = "inputUnit_dyn";
            this.unitLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // editorPanel
            //
            this.editorPanel.ContextMenuStrip = this.valueMenu;
            this.editorPanel.Controls.Add(this.referenceValue);
            this.editorPanel.Controls.Add(this.optionValue);
            this.editorPanel.Controls.Add(this.literalValue);
            this.editorPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.editorPanel.Margin = new System.Windows.Forms.Padding(0);
            this.editorPanel.Name = "inputEditor";
            this.editorPanel.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
            //
            // literalValue
            //
            this.literalValue.ContextMenuStrip = this.valueMenu;
            this.literalValue.Dock = System.Windows.Forms.DockStyle.Top;
            this.literalValue.Name = "inputLiteral";
            this.literalValue.Text = "0";
            //
            // optionValue
            //
            this.optionValue.ContextMenuStrip = this.valueMenu;
            this.optionValue.Dock = System.Windows.Forms.DockStyle.Top;
            this.optionValue.DropDownHeight = 300;
            this.optionValue.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.optionValue.IntegralHeight = false;
            this.optionValue.MaxDropDownItems = 16;
            this.optionValue.Name = "inputOption";
            this.optionValue.Visible = false;
            //
            // referenceValue
            //
            this.referenceValue.BackColor = System.Drawing.SystemColors.Window;
            this.referenceValue.ContextMenuStrip = this.valueMenu;
            this.referenceValue.Dock = System.Windows.Forms.DockStyle.Top;
            this.referenceValue.Name = "inputReference_dyn";
            this.referenceValue.ReadOnly = true;
            this.referenceValue.Visible = false;
            //
            // bindData
            //
            this.bindData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bindData.Margin = new System.Windows.Forms.Padding(5, 2, 0, 2);
            this.bindData.Name = "inputBind";
            this.bindData.Tag = "inputBind";
            this.bindData.Text = "绑定数据";
            this.bindData.UseVisualStyleBackColor = true;
            //
            // valueMenu
            //
            this.valueMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this.useLiteral });
            this.valueMenu.Name = "inputValueMenu";
            //
            // useLiteral
            //
            this.useLiteral.Name = "inputUseLiteral";
            this.useLiteral.Tag = "inputUseLiteral";
            this.useLiteral.Text = "使用固定值";
            //
            // helpTip
            //
            this.helpTip.AutoPopDelay = 15000;
            this.helpTip.ShowAlways = true;
            //
            // FlowInputRow
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ContextMenuStrip = this.valueMenu;
            this.Controls.Add(this.layout);
            this.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.MinimumSize = new System.Drawing.Size(560, 44);
            this.Name = "FlowInputRow";
            this.Size = new System.Drawing.Size(900, 44);
            this.layout.ResumeLayout(false);
            this.editorPanel.ResumeLayout(false);
            this.editorPanel.PerformLayout();
            this.valueMenu.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        /// <summary>释放行内提示和菜单资源。</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}