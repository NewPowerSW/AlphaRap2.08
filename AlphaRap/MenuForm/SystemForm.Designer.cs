namespace AlphaRap
{
    partial class SystemForm
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
			this.SystemGroup = new System.Windows.Forms.GroupBox();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.label2 = new System.Windows.Forms.Label();
			this.labSystem = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.labPlat = new System.Windows.Forms.Label();
			this.labParameter = new System.Windows.Forms.Label();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel2.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// SystemGroup
			// 
			this.SystemGroup.BackColor = System.Drawing.Color.White;
			this.SystemGroup.Location = new System.Drawing.Point(4, 80);
			this.SystemGroup.Margin = new System.Windows.Forms.Padding(4);
			this.SystemGroup.Name = "SystemGroup";
			this.SystemGroup.Padding = new System.Windows.Forms.Padding(4);
			this.SystemGroup.Size = new System.Drawing.Size(1192, 962);
			this.SystemGroup.TabIndex = 1;
			this.SystemGroup.TabStop = false;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.ColumnCount = 5;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
			this.tableLayoutPanel2.Controls.Add(this.label2, 4, 0);
			this.tableLayoutPanel2.Controls.Add(this.labSystem, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.label1, 3, 0);
			this.tableLayoutPanel2.Controls.Add(this.labPlat, 1, 0);
			this.tableLayoutPanel2.Controls.Add(this.labParameter, 2, 0);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(4, 4);
			this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 1;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(1192, 68);
			this.tableLayoutPanel2.TabIndex = 3;
			// 
			// label2
			// 
			this.label2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(108)))), ((int)(((byte)(182)))));
			this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.ForeColor = System.Drawing.Color.White;
			this.label2.Location = new System.Drawing.Point(956, 0);
			this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(232, 68);
			this.label2.TabIndex = 4;
			this.label2.Text = "报警设定";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.label2.Click += new System.EventHandler(this.label2_Click);
			// 
			// labSystem
			// 
			this.labSystem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(108)))), ((int)(((byte)(182)))));
			this.labSystem.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labSystem.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labSystem.ForeColor = System.Drawing.Color.White;
			this.labSystem.Location = new System.Drawing.Point(4, 0);
			this.labSystem.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.labSystem.Name = "labSystem";
			this.labSystem.Size = new System.Drawing.Size(230, 68);
			this.labSystem.TabIndex = 2;
			this.labSystem.Text = "系统设定";
			this.labSystem.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.labSystem.Click += new System.EventHandler(this.btSystem_Click);
			// 
			// label1
			// 
			this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(108)))), ((int)(((byte)(182)))));
			this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.ForeColor = System.Drawing.Color.White;
			this.label1.Location = new System.Drawing.Point(718, 0);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(230, 68);
			this.label1.TabIndex = 3;
			this.label1.Text = "Language";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.label1.Click += new System.EventHandler(this.label1_Click);
			// 
			// labPlat
			// 
			this.labPlat.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(108)))), ((int)(((byte)(182)))));
			this.labPlat.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labPlat.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labPlat.ForeColor = System.Drawing.Color.White;
			this.labPlat.Location = new System.Drawing.Point(242, 0);
			this.labPlat.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.labPlat.Name = "labPlat";
			this.labPlat.Size = new System.Drawing.Size(230, 68);
			this.labPlat.TabIndex = 2;
			this.labPlat.Text = "平台设定";
			this.labPlat.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.labPlat.Click += new System.EventHandler(this.btPlat_Click);
			// 
			// labParameter
			// 
			this.labParameter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(108)))), ((int)(((byte)(182)))));
			this.labParameter.Dock = System.Windows.Forms.DockStyle.Fill;
			this.labParameter.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.labParameter.ForeColor = System.Drawing.Color.White;
			this.labParameter.Location = new System.Drawing.Point(480, 0);
			this.labParameter.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.labParameter.Name = "labParameter";
			this.labParameter.Size = new System.Drawing.Size(230, 68);
			this.labParameter.TabIndex = 2;
			this.labParameter.Text = "参数设定";
			this.labParameter.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.labParameter.Click += new System.EventHandler(this.btParameter_Click);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.SystemGroup, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7.257304F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 92.7427F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(1200, 1061);
			this.tableLayoutPanel1.TabIndex = 4;
			// 
			// SystemForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ClientSize = new System.Drawing.Size(1200, 1061);
			this.Controls.Add(this.tableLayoutPanel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.Name = "SystemForm";
			this.Text = "SystemForm";
			this.Load += new System.EventHandler(this.SystemForm_Load);
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.ResumeLayout(false);

        }

		#endregion

		public System.Windows.Forms.GroupBox SystemGroup;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label labSystem;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label labPlat;
		private System.Windows.Forms.Label labParameter;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
	}
}