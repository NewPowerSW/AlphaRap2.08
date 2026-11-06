namespace AlphaRap
{
    partial class DataForm
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
            if (disposing) DisposeLogSources();
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
			this.data1 = new DataSave.Data();
			this.flowProductionFields = new System.Windows.Forms.Button();
            this.flowLogModules = new System.Windows.Forms.Button();
            this.dataActions = new System.Windows.Forms.FlowLayoutPanel();
			this.SuspendLayout();
			// 
			// data1
			// 
            this.flowProductionFields.Name = "flowProductionFields";
            this.flowProductionFields.Tag = "schema";
            this.flowProductionFields.Text = "生产表字段";
            this.flowProductionFields.Size = new System.Drawing.Size(175, 32);
            this.flowProductionFields.Margin = new System.Windows.Forms.Padding(4);
            this.flowProductionFields.Click += new System.EventHandler(this.FlowProductionFieldsClick);
            this.flowLogModules.Name = "flowLogModules";
            this.flowLogModules.Tag = "logModules";
            this.flowLogModules.Text = "日志模块";
            this.flowLogModules.Size = new System.Drawing.Size(175, 32);
            this.flowLogModules.Margin = new System.Windows.Forms.Padding(4);
            this.flowLogModules.Click += new System.EventHandler(this.ManageLogModules);
			this.data1.BackColor = System.Drawing.Color.White;
			this.data1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.data1.Location = new System.Drawing.Point(0, 0);
			this.data1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.data1.Name = "data1";
			this.data1.Size = new System.Drawing.Size(1224, 740);
			this.data1.TabIndex = 0;
			// 
			// DataForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(1224, 740);
			this.Controls.Add(this.data1);
            this.dataActions.Name = "dataActions";
            this.dataActions.Dock = System.Windows.Forms.DockStyle.Top;
            this.dataActions.Height = 42;
            this.dataActions.Controls.Add(this.flowLogModules);
            this.dataActions.Controls.Add(this.flowProductionFields);
            this.Controls.Add(this.dataActions);
			this.Name = "DataForm";
			this.Text = "DataForm";
			this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button flowProductionFields;
        private System.Windows.Forms.Button flowLogModules;
        private System.Windows.Forms.FlowLayoutPanel dataActions;
        private DataSave.Data data1;
	}
}