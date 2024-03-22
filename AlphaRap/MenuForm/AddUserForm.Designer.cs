namespace AlphaRap
{
    partial class AddUserForm
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
			this.label1 = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.panel4 = new System.Windows.Forms.Panel();
			this.label11 = new System.Windows.Forms.Label();
			this.dgvUserList = new System.Windows.Forms.DataGridView();
			this.panel3 = new System.Windows.Forms.Panel();
			this.btnAddUser = new System.Windows.Forms.Button();
			this.textPasswordConfirm = new System.Windows.Forms.TextBox();
			this.textPassword = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.textAddUserName = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.cbAddUserPermission = new System.Windows.Forms.ComboBox();
			this.label5 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.panel2 = new System.Windows.Forms.Panel();
			this.btnDeleteUser = new System.Windows.Forms.Button();
			this.label9 = new System.Windows.Forms.Label();
			this.btnModifyPermission = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.textSelectedUserName = new System.Windows.Forms.TextBox();
			this.cbSelectedPermission = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.dgvRightsConfig = new System.Windows.Forms.DataGridView();
			this.panel1.SuspendLayout();
			this.panel4.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvUserList)).BeginInit();
			this.panel3.SuspendLayout();
			this.panel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvRightsConfig)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(108)))), ((int)(((byte)(182)))));
			this.label1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.label1.Dock = System.Windows.Forms.DockStyle.Top;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold);
			this.label1.ForeColor = System.Drawing.Color.White;
			this.label1.Location = new System.Drawing.Point(0, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(1207, 30);
			this.label1.TabIndex = 0;
			this.label1.Text = "User Data";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.panel4);
			this.panel1.Controls.Add(this.panel3);
			this.panel1.Controls.Add(this.panel2);
			this.panel1.Location = new System.Drawing.Point(0, 34);
			this.panel1.Margin = new System.Windows.Forms.Padding(0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(1207, 369);
			this.panel1.TabIndex = 1;
			// 
			// panel4
			// 
			this.panel4.Controls.Add(this.label11);
			this.panel4.Controls.Add(this.dgvUserList);
			this.panel4.Location = new System.Drawing.Point(3, 4);
			this.panel4.Name = "panel4";
			this.panel4.Size = new System.Drawing.Size(452, 362);
			this.panel4.TabIndex = 5;
			// 
			// label11
			// 
			this.label11.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(108)))), ((int)(((byte)(182)))));
			this.label11.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.label11.Dock = System.Windows.Forms.DockStyle.Top;
			this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold);
			this.label11.ForeColor = System.Drawing.Color.White;
			this.label11.Location = new System.Drawing.Point(0, 0);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(452, 32);
			this.label11.TabIndex = 5;
			this.label11.Text = "User Information";
			this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// dgvUserList
			// 
			this.dgvUserList.AllowUserToAddRows = false;
			this.dgvUserList.AllowUserToDeleteRows = false;
			this.dgvUserList.BackgroundColor = System.Drawing.Color.White;
			this.dgvUserList.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.Sunken;
			this.dgvUserList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvUserList.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.dgvUserList.Location = new System.Drawing.Point(0, 28);
			this.dgvUserList.MultiSelect = false;
			this.dgvUserList.Name = "dgvUserList";
			this.dgvUserList.ReadOnly = true;
			this.dgvUserList.RowTemplate.Height = 23;
			this.dgvUserList.Size = new System.Drawing.Size(452, 334);
			this.dgvUserList.TabIndex = 1;
			this.dgvUserList.TabStop = false;
			this.dgvUserList.CurrentCellChanged += new System.EventHandler(this.dgvUserList_CurrentCellChanged);
			// 
			// panel3
			// 
			this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel3.Controls.Add(this.btnAddUser);
			this.panel3.Controls.Add(this.textPasswordConfirm);
			this.panel3.Controls.Add(this.textPassword);
			this.panel3.Controls.Add(this.label6);
			this.panel3.Controls.Add(this.label4);
			this.panel3.Controls.Add(this.textAddUserName);
			this.panel3.Controls.Add(this.label7);
			this.panel3.Controls.Add(this.cbAddUserPermission);
			this.panel3.Controls.Add(this.label5);
			this.panel3.Controls.Add(this.label10);
			this.panel3.Location = new System.Drawing.Point(810, 4);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(394, 363);
			this.panel3.TabIndex = 4;
			// 
			// btnAddUser
			// 
			this.btnAddUser.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold);
			this.btnAddUser.Location = new System.Drawing.Point(20, 288);
			this.btnAddUser.Name = "btnAddUser";
			this.btnAddUser.Size = new System.Drawing.Size(308, 48);
			this.btnAddUser.TabIndex = 11;
			this.btnAddUser.Text = "Add User";
			this.btnAddUser.UseVisualStyleBackColor = true;
			this.btnAddUser.Click += new System.EventHandler(this.btnAddUser_Click);
			// 
			// textPasswordConfirm
			// 
			this.textPasswordConfirm.Location = new System.Drawing.Point(229, 229);
			this.textPasswordConfirm.Name = "textPasswordConfirm";
			this.textPasswordConfirm.Size = new System.Drawing.Size(129, 42);
			this.textPasswordConfirm.TabIndex = 13;
			this.textPasswordConfirm.TextChanged += new System.EventHandler(this.AddUser_TextChanged);
			// 
			// textPassword
			// 
			this.textPassword.Location = new System.Drawing.Point(162, 161);
			this.textPassword.Name = "textPassword";
			this.textPassword.Size = new System.Drawing.Size(129, 42);
			this.textPassword.TabIndex = 19;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold);
			this.label6.Location = new System.Drawing.Point(10, 235);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(219, 29);
			this.label6.TabIndex = 12;
			this.label6.Text = "PasswordConfirm";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold);
			this.label4.Location = new System.Drawing.Point(10, 171);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(128, 29);
			this.label4.TabIndex = 18;
			this.label4.Text = "Password";
			// 
			// textAddUserName
			// 
			this.textAddUserName.Location = new System.Drawing.Point(162, 110);
			this.textAddUserName.Name = "textAddUserName";
			this.textAddUserName.Size = new System.Drawing.Size(129, 42);
			this.textAddUserName.TabIndex = 17;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold);
			this.label7.Location = new System.Drawing.Point(10, 110);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(144, 29);
			this.label7.TabIndex = 16;
			this.label7.Text = "User Name";
			// 
			// cbAddUserPermission
			// 
			this.cbAddUserPermission.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbAddUserPermission.FormattingEnabled = true;
			this.cbAddUserPermission.Location = new System.Drawing.Point(162, 45);
			this.cbAddUserPermission.Name = "cbAddUserPermission";
			this.cbAddUserPermission.Size = new System.Drawing.Size(129, 43);
			this.cbAddUserPermission.TabIndex = 10;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold);
			this.label5.Location = new System.Drawing.Point(10, 46);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(144, 29);
			this.label5.TabIndex = 9;
			this.label5.Text = "Permission";
			// 
			// label10
			// 
			this.label10.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(108)))), ((int)(((byte)(182)))));
			this.label10.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.label10.Dock = System.Windows.Forms.DockStyle.Top;
			this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold);
			this.label10.ForeColor = System.Drawing.Color.White;
			this.label10.Location = new System.Drawing.Point(0, 0);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(392, 32);
			this.label10.TabIndex = 5;
			this.label10.Text = "Add User";
			this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// panel2
			// 
			this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel2.Controls.Add(this.btnDeleteUser);
			this.panel2.Controls.Add(this.label9);
			this.panel2.Controls.Add(this.btnModifyPermission);
			this.panel2.Controls.Add(this.label2);
			this.panel2.Controls.Add(this.textSelectedUserName);
			this.panel2.Controls.Add(this.cbSelectedPermission);
			this.panel2.Controls.Add(this.label3);
			this.panel2.Location = new System.Drawing.Point(461, 3);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(343, 363);
			this.panel2.TabIndex = 3;
			// 
			// btnDeleteUser
			// 
			this.btnDeleteUser.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold);
			this.btnDeleteUser.Location = new System.Drawing.Point(20, 236);
			this.btnDeleteUser.Name = "btnDeleteUser";
			this.btnDeleteUser.Size = new System.Drawing.Size(305, 48);
			this.btnDeleteUser.TabIndex = 5;
			this.btnDeleteUser.Text = "Delete User";
			this.btnDeleteUser.UseVisualStyleBackColor = true;
			this.btnDeleteUser.Click += new System.EventHandler(this.btnDeleteUser_Click);
			// 
			// label9
			// 
			this.label9.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(108)))), ((int)(((byte)(182)))));
			this.label9.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.label9.Dock = System.Windows.Forms.DockStyle.Top;
			this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold);
			this.label9.ForeColor = System.Drawing.Color.White;
			this.label9.Location = new System.Drawing.Point(0, 0);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(341, 32);
			this.label9.TabIndex = 4;
			this.label9.Text = "Change User";
			this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// btnModifyPermission
			// 
			this.btnModifyPermission.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold);
			this.btnModifyPermission.Location = new System.Drawing.Point(20, 172);
			this.btnModifyPermission.Name = "btnModifyPermission";
			this.btnModifyPermission.Size = new System.Drawing.Size(305, 48);
			this.btnModifyPermission.TabIndex = 4;
			this.btnModifyPermission.Text = "Modify Permission";
			this.btnModifyPermission.UseVisualStyleBackColor = true;
			this.btnModifyPermission.Click += new System.EventHandler(this.btnModifyPermission_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold);
			this.label2.Location = new System.Drawing.Point(13, 51);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(144, 29);
			this.label2.TabIndex = 0;
			this.label2.Text = "Permission";
			// 
			// textSelectedUserName
			// 
			this.textSelectedUserName.Enabled = false;
			this.textSelectedUserName.Location = new System.Drawing.Point(161, 105);
			this.textSelectedUserName.Name = "textSelectedUserName";
			this.textSelectedUserName.Size = new System.Drawing.Size(164, 42);
			this.textSelectedUserName.TabIndex = 3;
			// 
			// cbSelectedPermission
			// 
			this.cbSelectedPermission.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbSelectedPermission.FormattingEnabled = true;
			this.cbSelectedPermission.Location = new System.Drawing.Point(161, 45);
			this.cbSelectedPermission.Name = "cbSelectedPermission";
			this.cbSelectedPermission.Size = new System.Drawing.Size(164, 43);
			this.cbSelectedPermission.TabIndex = 2;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold);
			this.label3.Location = new System.Drawing.Point(13, 110);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(144, 29);
			this.label3.TabIndex = 1;
			this.label3.Text = "User Name";
			// 
			// label8
			// 
			this.label8.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(108)))), ((int)(((byte)(182)))));
			this.label8.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label8.ForeColor = System.Drawing.Color.White;
			this.label8.Location = new System.Drawing.Point(0, 406);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(1207, 32);
			this.label8.TabIndex = 2;
			this.label8.Text = "Permission Setup";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// dgvRightsConfig
			// 
			this.dgvRightsConfig.AllowUserToAddRows = false;
			this.dgvRightsConfig.AllowUserToDeleteRows = false;
			this.dgvRightsConfig.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
			this.dgvRightsConfig.BackgroundColor = System.Drawing.Color.White;
			this.dgvRightsConfig.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.Sunken;
			this.dgvRightsConfig.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgvRightsConfig.Location = new System.Drawing.Point(0, 441);
			this.dgvRightsConfig.MultiSelect = false;
			this.dgvRightsConfig.Name = "dgvRightsConfig";
			this.dgvRightsConfig.RowTemplate.Height = 23;
			this.dgvRightsConfig.Size = new System.Drawing.Size(1207, 258);
			this.dgvRightsConfig.TabIndex = 3;
			this.dgvRightsConfig.TabStop = false;
			this.dgvRightsConfig.Click += new System.EventHandler(this.DataChange_Click);
			// 
			// AddUserForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(1207, 1028);
			this.ControlBox = false;
			this.Controls.Add(this.dgvRightsConfig);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.label1);
			this.Font = new System.Drawing.Font("微软雅黑", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "AddUserForm";
			this.Text = "Add User";
			this.Leave += new System.EventHandler(this.AddUserForm_Leave);
			this.panel1.ResumeLayout(false);
			this.panel4.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dgvUserList)).EndInit();
			this.panel3.ResumeLayout(false);
			this.panel3.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgvRightsConfig)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnDeleteUser;
        private System.Windows.Forms.Button btnModifyPermission;
        private System.Windows.Forms.TextBox textSelectedUserName;
        private System.Windows.Forms.ComboBox cbSelectedPermission;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textPasswordConfirm;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnAddUser;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.DataGridView dgvRightsConfig;
		private System.Windows.Forms.Panel panel4;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.DataGridView dgvUserList;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.TextBox textPassword;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox textAddUserName;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.ComboBox cbAddUserPermission;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Label label9;
	}
}