namespace AlphaRap
{
    partial class ProductManagerForm
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
			this.label2 = new System.Windows.Forms.Label();
			this.CurrentModel = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.txtProduct = new System.Windows.Forms.TextBox();
			this.btCreat = new System.Windows.Forms.Button();
			this.btDelete = new System.Windows.Forms.Button();
			this.btUse = new System.Windows.Forms.Button();
			this.listView1 = new System.Windows.Forms.ListView();
			this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.label1 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.textBox3 = new System.Windows.Forms.TextBox();
			this.dataTable1 = new System.Data.DataTable();
			this.dataColumn1 = new System.Data.DataColumn();
			this.dataColumn2 = new System.Data.DataColumn();
			this.dataColumn3 = new System.Data.DataColumn();
			this.dataColumn4 = new System.Data.DataColumn();
			this.dataColumn5 = new System.Data.DataColumn();
			this.dataColumn7 = new System.Data.DataColumn();
			this.dataColumn6 = new System.Data.DataColumn();
			this.dataColumn8 = new System.Data.DataColumn();
			this.label9 = new System.Windows.Forms.Label();
			this.textBox4 = new System.Windows.Forms.TextBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.checkBox2 = new System.Windows.Forms.CheckBox();
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.label12 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.textBox6 = new System.Windows.Forms.TextBox();
			this.textBox5 = new System.Windows.Forms.TextBox();
			this.label10 = new System.Windows.Forms.Label();
			this.panel2 = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)(this.SettingData)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RecipeData)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dataTable1)).BeginInit();
			this.panel1.SuspendLayout();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// RecipeData
			// 
			this.RecipeData.Tables.AddRange(new System.Data.DataTable[] {
            this.dataTable1});
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
			this.label2.Location = new System.Drawing.Point(4, 7);
			this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(149, 28);
			this.label2.TabIndex = 1;
			this.label2.Text = "Product Name ";
			// 
			// CurrentModel
			// 
			this.CurrentModel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.CurrentModel.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.CurrentModel.ForeColor = System.Drawing.Color.Red;
			this.CurrentModel.Location = new System.Drawing.Point(4, 46);
			this.CurrentModel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.CurrentModel.Name = "CurrentModel";
			this.CurrentModel.Size = new System.Drawing.Size(380, 40);
			this.CurrentModel.TabIndex = 2;
			this.CurrentModel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label4
			// 
			this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
			this.label4.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
			this.label4.Location = new System.Drawing.Point(15, 93);
			this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(372, 37);
			this.label4.TabIndex = 1;
			this.label4.Text = "New Product Name";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// txtProduct
			// 
			this.txtProduct.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.txtProduct.Location = new System.Drawing.Point(19, 148);
			this.txtProduct.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.txtProduct.Multiline = true;
			this.txtProduct.Name = "txtProduct";
			this.txtProduct.Size = new System.Drawing.Size(369, 43);
			this.txtProduct.TabIndex = 3;
			// 
			// btCreat
			// 
			this.btCreat.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btCreat.Location = new System.Drawing.Point(19, 224);
			this.btCreat.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.btCreat.Name = "btCreat";
			this.btCreat.Size = new System.Drawing.Size(371, 59);
			this.btCreat.TabIndex = 4;
			this.btCreat.Text = "Add Product ";
			this.btCreat.UseVisualStyleBackColor = true;
			this.btCreat.Click += new System.EventHandler(this.btCreate_Click);
			// 
			// btDelete
			// 
			this.btDelete.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btDelete.Location = new System.Drawing.Point(19, 305);
			this.btDelete.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.btDelete.Name = "btDelete";
			this.btDelete.Size = new System.Drawing.Size(371, 59);
			this.btDelete.TabIndex = 4;
			this.btDelete.Text = "Delete Product";
			this.btDelete.UseVisualStyleBackColor = true;
			this.btDelete.Click += new System.EventHandler(this.btDelete_Click);
			// 
			// btUse
			// 
			this.btUse.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btUse.Location = new System.Drawing.Point(19, 384);
			this.btUse.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.btUse.Name = "btUse";
			this.btUse.Size = new System.Drawing.Size(371, 59);
			this.btUse.TabIndex = 4;
			this.btUse.Text = "Use Product";
			this.btUse.UseVisualStyleBackColor = true;
			this.btUse.Click += new System.EventHandler(this.btUse_Click);
			// 
			// listView1
			// 
			this.listView1.BackColor = System.Drawing.SystemColors.Window;
			this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
			this.listView1.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.listView1.FullRowSelect = true;
			this.listView1.GridLines = true;
			this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.listView1.HideSelection = false;
			this.listView1.Location = new System.Drawing.Point(16, 80);
			this.listView1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.listView1.MultiSelect = false;
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(545, 665);
			this.listView1.TabIndex = 5;
			this.listView1.UseCompatibleStateImageBehavior = false;
			this.listView1.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "所有产品型号";
			this.columnHeader1.Width = 704;
			// 
			// label1
			// 
			this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(108)))), ((int)(((byte)(182)))));
			this.label1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.label1.Dock = System.Windows.Forms.DockStyle.Top;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Bold);
			this.label1.ForeColor = System.Drawing.Color.White;
			this.label1.Location = new System.Drawing.Point(0, 0);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(1707, 44);
			this.label1.TabIndex = 6;
			this.label1.Text = "Product Settings";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
			this.label3.Location = new System.Drawing.Point(13, 48);
			this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(191, 28);
			this.label3.TabIndex = 8;
			this.label3.Text = "All Product Models ";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
			this.label5.Location = new System.Drawing.Point(1114, 48);
			this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(98, 28);
			this.label5.TabIndex = 8;
			this.label5.Text = "Manager ";
			// 
			// textBox1
			// 
			this.textBox1.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.RecipeData, "ProductSetting.PRODUCTID", true));
			this.textBox1.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.textBox1.Location = new System.Drawing.Point(4, 136);
			this.textBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(380, 40);
			this.textBox1.TabIndex = 3;
			this.textBox1.TextChanged += new System.EventHandler(this.textBox4_TextChanged);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
			this.label6.Location = new System.Drawing.Point(4, 97);
			this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(128, 28);
			this.label6.TabIndex = 1;
			this.label6.Text = "PRODUCTID ";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
			this.label7.Location = new System.Drawing.Point(4, 187);
			this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(82, 28);
			this.label7.TabIndex = 1;
			this.label7.Text = "STEPID ";
			// 
			// textBox2
			// 
			this.textBox2.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.RecipeData, "ProductSetting.STEPID", true));
			this.textBox2.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.textBox2.Location = new System.Drawing.Point(4, 226);
			this.textBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.textBox2.Multiline = true;
			this.textBox2.Name = "textBox2";
			this.textBox2.Size = new System.Drawing.Size(380, 40);
			this.textBox2.TabIndex = 3;
			this.textBox2.TextChanged += new System.EventHandler(this.textBox4_TextChanged);
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
			this.label8.Location = new System.Drawing.Point(4, 277);
			this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(100, 28);
			this.label8.TabIndex = 1;
			this.label8.Text = "RECIPEID ";
			// 
			// textBox3
			// 
			this.textBox3.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.RecipeData, "ProductSetting.RECIPEID", true));
			this.textBox3.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.textBox3.Location = new System.Drawing.Point(4, 316);
			this.textBox3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.textBox3.Multiline = true;
			this.textBox3.Name = "textBox3";
			this.textBox3.Size = new System.Drawing.Size(380, 40);
			this.textBox3.TabIndex = 3;
			this.textBox3.TextChanged += new System.EventHandler(this.textBox4_TextChanged);
			// 
			// dataTable1
			// 
			this.dataTable1.Columns.AddRange(new System.Data.DataColumn[] {
            this.dataColumn1,
            this.dataColumn2,
            this.dataColumn3,
            this.dataColumn4,
            this.dataColumn5,
            this.dataColumn7,
            this.dataColumn6,
            this.dataColumn8});
			this.dataTable1.TableName = "ProductSetting";
			// 
			// dataColumn1
			// 
			this.dataColumn1.ColumnName = "PRODUCTID";
			// 
			// dataColumn2
			// 
			this.dataColumn2.ColumnName = "STEPID";
			// 
			// dataColumn3
			// 
			this.dataColumn3.ColumnName = "RECIPEID";
			// 
			// dataColumn4
			// 
			this.dataColumn4.ColumnName = "PORTID";
			// 
			// dataColumn5
			// 
			this.dataColumn5.Caption = "SN1KeySub";
			this.dataColumn5.ColumnName = "SN1KeySub";
			this.dataColumn5.DefaultValue = "";
			// 
			// dataColumn7
			// 
			this.dataColumn7.AllowDBNull = false;
			this.dataColumn7.ColumnName = "EnableSN1KeySub";
			this.dataColumn7.DataType = typeof(bool);
			this.dataColumn7.DefaultValue = false;
			// 
			// dataColumn6
			// 
			this.dataColumn6.AllowDBNull = false;
			this.dataColumn6.ColumnName = "SN2KeySub";
			this.dataColumn6.DefaultValue = "";
			// 
			// dataColumn8
			// 
			this.dataColumn8.AllowDBNull = false;
			this.dataColumn8.ColumnName = "EnableSN2KeySub";
			this.dataColumn8.DataType = typeof(bool);
			this.dataColumn8.DefaultValue = false;
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
			this.label9.Location = new System.Drawing.Point(4, 367);
			this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(88, 28);
			this.label9.TabIndex = 1;
			this.label9.Text = "PORTID ";
			// 
			// textBox4
			// 
			this.textBox4.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.RecipeData, "ProductSetting.PORTID", true));
			this.textBox4.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.textBox4.Location = new System.Drawing.Point(4, 406);
			this.textBox4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.textBox4.Multiline = true;
			this.textBox4.Name = "textBox4";
			this.textBox4.Size = new System.Drawing.Size(380, 40);
			this.textBox4.TabIndex = 3;
			this.textBox4.TextChanged += new System.EventHandler(this.textBox4_TextChanged);
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.White;
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel1.Controls.Add(this.checkBox2);
			this.panel1.Controls.Add(this.label2);
			this.panel1.Controls.Add(this.checkBox1);
			this.panel1.Controls.Add(this.label6);
			this.panel1.Controls.Add(this.CurrentModel);
			this.panel1.Controls.Add(this.label7);
			this.panel1.Controls.Add(this.label12);
			this.panel1.Controls.Add(this.label11);
			this.panel1.Controls.Add(this.label9);
			this.panel1.Controls.Add(this.label8);
			this.panel1.Controls.Add(this.textBox3);
			this.panel1.Controls.Add(this.textBox1);
			this.panel1.Controls.Add(this.textBox2);
			this.panel1.Controls.Add(this.textBox6);
			this.panel1.Controls.Add(this.textBox5);
			this.panel1.Controls.Add(this.textBox4);
			this.panel1.Location = new System.Drawing.Point(571, 80);
			this.panel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(530, 665);
			this.panel1.TabIndex = 9;
			// 
			// checkBox2
			// 
			this.checkBox2.AutoSize = true;
			this.checkBox2.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.RecipeData, "ProductSetting.EnableSN2KeySub", true));
			this.checkBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkBox2.Location = new System.Drawing.Point(420, 602);
			this.checkBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.checkBox2.Name = "checkBox2";
			this.checkBox2.Size = new System.Drawing.Size(98, 24);
			this.checkBox2.TabIndex = 4;
			this.checkBox2.Text = "ENABLE";
			this.checkBox2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox2.UseVisualStyleBackColor = true;
			this.checkBox2.CheckedChanged += new System.EventHandler(this.textBox4_TextChanged);
			// 
			// checkBox1
			// 
			this.checkBox1.AutoSize = true;
			this.checkBox1.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.RecipeData, "ProductSetting.EnableSN1KeySub", true));
			this.checkBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.checkBox1.Location = new System.Drawing.Point(420, 507);
			this.checkBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(98, 24);
			this.checkBox1.TabIndex = 4;
			this.checkBox1.Text = "ENABLE";
			this.checkBox1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.checkBox1.UseVisualStyleBackColor = true;
			this.checkBox1.CheckedChanged += new System.EventHandler(this.textBox4_TextChanged);
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
			this.label12.Location = new System.Drawing.Point(4, 547);
			this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(207, 28);
			this.label12.TabIndex = 1;
			this.label12.Text = "PCB Barcode KeySub ";
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
			this.label11.Location = new System.Drawing.Point(4, 457);
			this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(248, 28);
			this.label11.TabIndex = 1;
			this.label11.Text = "Housing Barcode KeySub ";
			// 
			// textBox6
			// 
			this.textBox6.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.RecipeData, "ProductSetting.SN2KeySub", true));
			this.textBox6.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.textBox6.Location = new System.Drawing.Point(4, 586);
			this.textBox6.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.textBox6.Multiline = true;
			this.textBox6.Name = "textBox6";
			this.textBox6.Size = new System.Drawing.Size(380, 40);
			this.textBox6.TabIndex = 3;
			this.textBox6.TextChanged += new System.EventHandler(this.textBox4_TextChanged);
			// 
			// textBox5
			// 
			this.textBox5.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.RecipeData, "ProductSetting.SN1KeySub", true));
			this.textBox5.Font = new System.Drawing.Font("微软雅黑", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.textBox5.Location = new System.Drawing.Point(4, 496);
			this.textBox5.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.textBox5.Multiline = true;
			this.textBox5.Name = "textBox5";
			this.textBox5.Size = new System.Drawing.Size(380, 40);
			this.textBox5.TabIndex = 3;
			this.textBox5.TextChanged += new System.EventHandler(this.textBox4_TextChanged);
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
			this.label10.Location = new System.Drawing.Point(564, 48);
			this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(156, 28);
			this.label10.TabIndex = 10;
			this.label10.Text = "Model Settings ";
			// 
			// panel2
			// 
			this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel2.Controls.Add(this.btCreat);
			this.panel2.Controls.Add(this.label4);
			this.panel2.Controls.Add(this.btDelete);
			this.panel2.Controls.Add(this.btUse);
			this.panel2.Controls.Add(this.txtProduct);
			this.panel2.Location = new System.Drawing.Point(1109, 80);
			this.panel2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(534, 665);
			this.panel2.TabIndex = 5;
			// 
			// ProductManagerForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoScroll = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(1707, 1102);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.label10);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.listView1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Margin = new System.Windows.Forms.Padding(5);
			this.Name = "ProductManagerForm";
			this.Text = "ProductManagerForm";
			this.Load += new System.EventHandler(this.ProductManagerForm_Load);
			((System.ComponentModel.ISupportInitialize)(this.SettingData)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RecipeData)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dataTable1)).EndInit();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtProduct;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        public System.Windows.Forms.Label CurrentModel;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textBox3;
        private System.Data.DataTable dataTable1;
        private System.Data.DataColumn dataColumn1;
        private System.Data.DataColumn dataColumn2;
        private System.Data.DataColumn dataColumn3;
        private System.Data.DataColumn dataColumn4;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox textBox6;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Data.DataColumn dataColumn5;
        private System.Data.DataColumn dataColumn7;
        private System.Data.DataColumn dataColumn6;
        private System.Data.DataColumn dataColumn8;
        private System.Windows.Forms.Panel panel2;
        public System.Windows.Forms.Button btCreat;
        public System.Windows.Forms.Button btDelete;
        public System.Windows.Forms.Button btUse;
    }
}