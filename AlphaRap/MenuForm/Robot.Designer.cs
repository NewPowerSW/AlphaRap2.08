namespace AlphaRap.MenuForm
{
    partial class Robot
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
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.textBox4 = new System.Windows.Forms.TextBox();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.textBox3 = new System.Windows.Forms.TextBox();
			this.label13 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.textBox5 = new System.Windows.Forms.TextBox();
			this.label11 = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.dataTable1 = new System.Data.DataTable();
			this.dataColumn1 = new System.Data.DataColumn();
			this.dataColumn2 = new System.Data.DataColumn();
			this.dataColumn3 = new System.Data.DataColumn();
			this.dataColumn4 = new System.Data.DataColumn();
			this.dataTable2 = new System.Data.DataTable();
			((System.ComponentModel.ISupportInitialize)(this.SettingData)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RecipeData)).BeginInit();
			this.groupBox3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataTable1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dataTable2)).BeginInit();
			this.SuspendLayout();
			// 
			// SettingData
			// 
			this.SettingData.Tables.AddRange(new System.Data.DataTable[] {
            this.dataTable1,
            this.dataTable2});
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.textBox4);
			this.groupBox3.Controls.Add(this.textBox2);
			this.groupBox3.Controls.Add(this.textBox3);
			this.groupBox3.Controls.Add(this.label13);
			this.groupBox3.Controls.Add(this.label10);
			this.groupBox3.Controls.Add(this.textBox5);
			this.groupBox3.Controls.Add(this.label11);
			this.groupBox3.Controls.Add(this.label12);
			this.groupBox3.Location = new System.Drawing.Point(13, 13);
			this.groupBox3.Margin = new System.Windows.Forms.Padding(4);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Padding = new System.Windows.Forms.Padding(4);
			this.groupBox3.Size = new System.Drawing.Size(455, 202);
			this.groupBox3.TabIndex = 20;
			this.groupBox3.TabStop = false;
			this.groupBox3.Enter += new System.EventHandler(this.groupBox3_Enter);
			// 
			// textBox4
			// 
			this.textBox4.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.SettingData, "PSet.Robot1Address", true));
			this.textBox4.Location = new System.Drawing.Point(209, 75);
			this.textBox4.Margin = new System.Windows.Forms.Padding(4);
			this.textBox4.Name = "textBox4";
			this.textBox4.Size = new System.Drawing.Size(204, 25);
			this.textBox4.TabIndex = 1;
			// 
			// textBox2
			// 
			this.textBox2.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.SettingData, "PSet.Robot2Address", true));
			this.textBox2.Location = new System.Drawing.Point(209, 166);
			this.textBox2.Margin = new System.Windows.Forms.Padding(4);
			this.textBox2.Name = "textBox2";
			this.textBox2.Size = new System.Drawing.Size(204, 25);
			this.textBox2.TabIndex = 1;
			// 
			// textBox3
			// 
			this.textBox3.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.SettingData, "PSet.Robot1IP", true));
			this.textBox3.Location = new System.Drawing.Point(209, 25);
			this.textBox3.Margin = new System.Windows.Forms.Padding(4);
			this.textBox3.Name = "textBox3";
			this.textBox3.Size = new System.Drawing.Size(204, 25);
			this.textBox3.TabIndex = 1;
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.label13.Location = new System.Drawing.Point(5, 162);
			this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(178, 27);
			this.label13.TabIndex = 0;
			this.label13.Text = "Robot2 Address:";
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.label10.Location = new System.Drawing.Point(11, 24);
			this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(117, 27);
			this.label10.TabIndex = 0;
			this.label10.Text = "Robot1 IP:";
			// 
			// textBox5
			// 
			this.textBox5.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.SettingData, "PSet.Robot2IP", true));
			this.textBox5.Location = new System.Drawing.Point(209, 121);
			this.textBox5.Margin = new System.Windows.Forms.Padding(4);
			this.textBox5.Name = "textBox5";
			this.textBox5.Size = new System.Drawing.Size(204, 25);
			this.textBox5.TabIndex = 1;
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.label11.Location = new System.Drawing.Point(11, 71);
			this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(178, 27);
			this.label11.TabIndex = 0;
			this.label11.Text = "Robot1 Address:";
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.label12.Location = new System.Drawing.Point(8, 121);
			this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(117, 27);
			this.label12.TabIndex = 0;
			this.label12.Text = "Robot2 IP:";
			// 
			// dataTable1
			// 
			this.dataTable1.Columns.AddRange(new System.Data.DataColumn[] {
            this.dataColumn1,
            this.dataColumn2,
            this.dataColumn3,
            this.dataColumn4});
			this.dataTable1.TableName = "PSet";
			// 
			// dataColumn1
			// 
			this.dataColumn1.ColumnName = "Robot1IP";
			// 
			// dataColumn2
			// 
			this.dataColumn2.ColumnName = "Robot2IP";
			// 
			// dataColumn3
			// 
			this.dataColumn3.ColumnName = "Robot1Address";
			// 
			// dataColumn4
			// 
			this.dataColumn4.ColumnName = "Robot2Address";
			// 
			// dataTable2
			// 
			this.dataTable2.TableName = "MSet";
			// 
			// Robot
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(1199, 598);
			this.Controls.Add(this.groupBox3);
			this.Name = "Robot";
			this.Text = "Robot";
			((System.ComponentModel.ISupportInitialize)(this.SettingData)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RecipeData)).EndInit();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataTable1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dataTable2)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Data.DataTable dataTable1;
        private System.Data.DataTable dataTable2;
        private System.Data.DataColumn dataColumn1;
        private System.Data.DataColumn dataColumn2;
        private System.Data.DataColumn dataColumn3;
        private System.Data.DataColumn dataColumn4;
    }
}