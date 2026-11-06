namespace NPSDK
{
    partial class Flow_BaseForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing) Flow_Module.FlowChart_ModuleList.Remove(this);
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.InitialPage = new System.Windows.Forms.TabPage();
            this.FlowChartTable = new System.Windows.Forms.TabControl();
            this.AutoPage = new System.Windows.Forms.TabPage();
            this.FlowChartTable.SuspendLayout();
            this.SuspendLayout();
            // 
            // InitialPage
            // 
            this.InitialPage.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.InitialPage.Location = new System.Drawing.Point(4, 31);
            this.InitialPage.Name = "InitialPage";
            this.InitialPage.Padding = new System.Windows.Forms.Padding(3);
            this.InitialPage.Size = new System.Drawing.Size(792, 415);
            this.InitialPage.TabIndex = 0;
            this.InitialPage.Text = "Initial";
            this.InitialPage.UseVisualStyleBackColor = true;
            // 
            // FlowChartTable
            // 
            this.FlowChartTable.Controls.Add(this.InitialPage);
            this.FlowChartTable.Controls.Add(this.AutoPage);
            this.FlowChartTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FlowChartTable.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FlowChartTable.Location = new System.Drawing.Point(0, 0);
            this.FlowChartTable.Name = "FlowChartTable";
            this.FlowChartTable.SelectedIndex = 0;
            this.FlowChartTable.Size = new System.Drawing.Size(800, 450);
            this.FlowChartTable.TabIndex = 0;
            // 
            // AutoPage
            // 
            this.AutoPage.Cursor = System.Windows.Forms.Cursors.Default;
            this.AutoPage.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.AutoPage.Location = new System.Drawing.Point(4, 31);
            this.AutoPage.Name = "AutoPage";
            this.AutoPage.Padding = new System.Windows.Forms.Padding(3);
            this.AutoPage.Size = new System.Drawing.Size(792, 415);
            this.AutoPage.TabIndex = 1;
            this.AutoPage.Text = "Auto";
            this.AutoPage.UseVisualStyleBackColor = true;
            // 
            // Flow_BaseForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.FlowChartTable);
            this.Name = "Flow_BaseForm";
            this.Text = "FlowBaseForm";
            this.FlowChartTable.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.TabPage InitialPage;
        public System.Windows.Forms.TabPage AutoPage;
        public System.Windows.Forms.TabControl FlowChartTable;
    }
}