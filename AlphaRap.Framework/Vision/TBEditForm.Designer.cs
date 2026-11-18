namespace AlphaRap
{
    partial class TBEditForm
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
			this.cogToolBlockEditV21 = new Cognex.VisionPro.ToolBlock.CogToolBlockEditV2();
			this.pnlTemplates = new System.Windows.Forms.FlowLayoutPanel();
			this.lblTemplates = new System.Windows.Forms.Label();
			this.cmbTemplates = new System.Windows.Forms.ComboBox();
			this.btnUseTemplate = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.cogToolBlockEditV21)).BeginInit();
			this.pnlTemplates.SuspendLayout();
			this.SuspendLayout();
			// 
			// cogToolBlockEditV21
			// 
			this.cogToolBlockEditV21.AllowDrop = true;
			this.cogToolBlockEditV21.ContextMenuCustomizer = null;
			this.cogToolBlockEditV21.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cogToolBlockEditV21.Location = new System.Drawing.Point(0, 40);
			this.cogToolBlockEditV21.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.cogToolBlockEditV21.MinimumSize = new System.Drawing.Size(652, 0);
			this.cogToolBlockEditV21.Name = "cogToolBlockEditV21";
			this.cogToolBlockEditV21.ShowNodeToolTips = true;
			this.cogToolBlockEditV21.Size = new System.Drawing.Size(1332, 676);
			this.cogToolBlockEditV21.SuspendElectricRuns = false;
			this.cogToolBlockEditV21.TabIndex = 1;
			//
			// pnlTemplates
			//
			this.pnlTemplates.AutoSize = true;
			this.pnlTemplates.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.pnlTemplates.Controls.Add(this.lblTemplates);
			this.pnlTemplates.Controls.Add(this.cmbTemplates);
			this.pnlTemplates.Controls.Add(this.btnUseTemplate);
			this.pnlTemplates.Dock = System.Windows.Forms.DockStyle.Top;
			this.pnlTemplates.Location = new System.Drawing.Point(0, 0);
			this.pnlTemplates.Name = "pnlTemplates";
			this.pnlTemplates.Padding = new System.Windows.Forms.Padding(8, 4, 8, 4);
			this.pnlTemplates.Size = new System.Drawing.Size(1332, 40);
			this.pnlTemplates.TabIndex = 0;
			this.pnlTemplates.WrapContents = false;
			//
			// lblTemplates
			//
			this.lblTemplates.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.lblTemplates.AutoSize = true;
			this.lblTemplates.Name = "lblTemplates";
			this.lblTemplates.TabIndex = 0;
			this.lblTemplates.Text = "定位模板";
			//
			// cmbTemplates
			//
			this.cmbTemplates.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.cmbTemplates.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbTemplates.FormattingEnabled = true;
			this.cmbTemplates.Name = "cmbTemplates";
			this.cmbTemplates.Size = new System.Drawing.Size(260, 23);
			this.cmbTemplates.TabIndex = 1;
			//
			// btnUseTemplate
			//
			this.btnUseTemplate.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.btnUseTemplate.AutoSize = true;
			this.btnUseTemplate.Name = "btnUseTemplate";
			this.btnUseTemplate.Size = new System.Drawing.Size(108, 26);
			this.btnUseTemplate.TabIndex = 2;
			this.btnUseTemplate.Text = "使用此模板";
			this.btnUseTemplate.UseVisualStyleBackColor = true;
			this.btnUseTemplate.Click += new System.EventHandler(this.UseTemplateClicked);
			// 
			// TBEditForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(1332, 716);
			this.Controls.Add(this.cogToolBlockEditV21);
			this.Controls.Add(this.pnlTemplates);
			this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.MinimumSize = new System.Drawing.Size(700, 340);
			this.Name = "TBEditForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "TBEditForm";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TBEditForm_FormClosing);
			((System.ComponentModel.ISupportInitialize)(this.cogToolBlockEditV21)).EndInit();
			this.pnlTemplates.ResumeLayout(false);
			this.pnlTemplates.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private Cognex.VisionPro.ToolBlock.CogToolBlockEditV2 cogToolBlockEditV21;
        private System.Windows.Forms.FlowLayoutPanel pnlTemplates;
        private System.Windows.Forms.Label lblTemplates;
        private System.Windows.Forms.ComboBox cmbTemplates;
        private System.Windows.Forms.Button btnUseTemplate;
    }
}
