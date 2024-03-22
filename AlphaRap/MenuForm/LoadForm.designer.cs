namespace AlphaRap
{
    partial class LoadForm
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoadForm));
			this.tmr_Refresh = new System.Windows.Forms.Timer(this.components);
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.ProgressBar1 = new System.Windows.Forms.ProgressBar();
			this.lbl_Greeting_Slogan = new System.Windows.Forms.Label();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.tableLayoutPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// tmr_Refresh
			// 
			this.tmr_Refresh.Enabled = true;
			this.tmr_Refresh.Tick += new System.EventHandler(this.tmr_Refresh_Tick);
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 1;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Controls.Add(this.ProgressBar1, 0, 2);
			this.tableLayoutPanel2.Controls.Add(this.lbl_Greeting_Slogan, 0, 1);
			this.tableLayoutPanel2.Controls.Add(this.pictureBox1, 0, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 3;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.3F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.3F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.4F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(1046, 276);
			this.tableLayoutPanel2.TabIndex = 7;
			// 
			// ProgressBar1
			// 
			this.ProgressBar1.BackColor = System.Drawing.Color.White;
			this.ProgressBar1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProgressBar1.Location = new System.Drawing.Point(4, 185);
			this.ProgressBar1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.ProgressBar1.Name = "ProgressBar1";
			this.ProgressBar1.Size = new System.Drawing.Size(1038, 88);
			this.ProgressBar1.TabIndex = 5;
			// 
			// lbl_Greeting_Slogan
			// 
			this.lbl_Greeting_Slogan.AutoSize = true;
			this.lbl_Greeting_Slogan.BackColor = System.Drawing.Color.LimeGreen;
			this.lbl_Greeting_Slogan.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lbl_Greeting_Slogan.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lbl_Greeting_Slogan.ForeColor = System.Drawing.Color.Black;
			this.lbl_Greeting_Slogan.Location = new System.Drawing.Point(4, 91);
			this.lbl_Greeting_Slogan.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.lbl_Greeting_Slogan.Name = "lbl_Greeting_Slogan";
			this.lbl_Greeting_Slogan.Size = new System.Drawing.Size(1038, 91);
			this.lbl_Greeting_Slogan.TabIndex = 3;
			this.lbl_Greeting_Slogan.Text = "Program loading, please wait ";
			this.lbl_Greeting_Slogan.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// pictureBox1
			// 
			this.pictureBox1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureBox1.BackgroundImage")));
			this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pictureBox1.Location = new System.Drawing.Point(4, 3);
			this.pictureBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(1038, 85);
			this.pictureBox1.TabIndex = 4;
			this.pictureBox1.TabStop = false;
			// 
			// LoadForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(1046, 276);
			this.Controls.Add(this.tableLayoutPanel2);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.Name = "LoadForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "LoadForm";
			this.Load += new System.EventHandler(this.LoadForm_Load);
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Timer tmr_Refresh;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        internal System.Windows.Forms.ProgressBar ProgressBar1;
        internal System.Windows.Forms.Label lbl_Greeting_Slogan;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}