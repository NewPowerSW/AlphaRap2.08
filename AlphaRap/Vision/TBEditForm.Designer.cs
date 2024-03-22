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
			((System.ComponentModel.ISupportInitialize)(this.cogToolBlockEditV21)).BeginInit();
			this.SuspendLayout();
			// 
			// cogToolBlockEditV21
			// 
			this.cogToolBlockEditV21.AllowDrop = true;
			this.cogToolBlockEditV21.ContextMenuCustomizer = null;
			this.cogToolBlockEditV21.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cogToolBlockEditV21.Location = new System.Drawing.Point(0, 0);
			this.cogToolBlockEditV21.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.cogToolBlockEditV21.MinimumSize = new System.Drawing.Size(652, 0);
			this.cogToolBlockEditV21.Name = "cogToolBlockEditV21";
			this.cogToolBlockEditV21.ShowNodeToolTips = true;
			this.cogToolBlockEditV21.Size = new System.Drawing.Size(1332, 716);
			this.cogToolBlockEditV21.SuspendElectricRuns = false;
			this.cogToolBlockEditV21.TabIndex = 0;
			// 
			// TBEditForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1332, 716);
			this.Controls.Add(this.cogToolBlockEditV21);
			this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.Name = "TBEditForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "TBEditForm";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TBEditForm_FormClosing);
			((System.ComponentModel.ISupportInitialize)(this.cogToolBlockEditV21)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private Cognex.VisionPro.ToolBlock.CogToolBlockEditV2 cogToolBlockEditV21;
    }
}