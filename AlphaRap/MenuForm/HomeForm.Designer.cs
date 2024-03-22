namespace AlphaRap
{
    partial class HomeForm
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
			System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea3 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
			System.Windows.Forms.DataVisualization.Charting.Legend legend3 = new System.Windows.Forms.DataVisualization.Charting.Legend();
			System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
			System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea4 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
			System.Windows.Forms.DataVisualization.Charting.Legend legend4 = new System.Windows.Forms.DataVisualization.Charting.Legend();
			System.Windows.Forms.DataVisualization.Charting.Series series4 = new System.Windows.Forms.DataVisualization.Charting.Series();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HomeForm));
			this.panel1 = new System.Windows.Forms.Panel();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.label_PLCStaus = new System.Windows.Forms.Label();
			this.LAB_ScannStaus = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.label_ScannStaus = new System.Windows.Forms.Label();
			this.chart2 = new System.Windows.Forms.DataVisualization.Charting.Chart();
			this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.label12 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.txtCT = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.tHourlyYield = new System.Windows.Forms.TextBox();
			this.txtBarcode4 = new System.Windows.Forms.TextBox();
			this.txtBarcode3 = new System.Windows.Forms.TextBox();
			this.txtBarcode2 = new System.Windows.Forms.TextBox();
			this.txtBarcode = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.tHourlyInput = new System.Windows.Forms.TextBox();
			this.tHourlyOutput = new System.Windows.Forms.TextBox();
			this.tHourlyReject = new System.Windows.Forms.TextBox();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.panel1.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.chart2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.White;
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel1.Controls.Add(this.tableLayoutPanel1);
			this.panel1.Controls.Add(this.chart2);
			this.panel1.Controls.Add(this.chart1);
			this.panel1.Controls.Add(this.groupBox2);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(1521, 634);
			this.panel1.TabIndex = 5;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
			this.tableLayoutPanel1.ColumnCount = 5;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 3F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 3F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 84F));
			this.tableLayoutPanel1.Controls.Add(this.label_PLCStaus, 3, 0);
			this.tableLayoutPanel1.Controls.Add(this.LAB_ScannStaus, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.label10, 2, 0);
			this.tableLayoutPanel1.Controls.Add(this.label_ScannStaus, 1, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 607);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(1519, 25);
			this.tableLayoutPanel1.TabIndex = 16;
			// 
			// label_PLCStaus
			// 
			this.label_PLCStaus.BackColor = System.Drawing.Color.Green;
			this.label_PLCStaus.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label_PLCStaus.ForeColor = System.Drawing.Color.White;
			this.label_PLCStaus.Location = new System.Drawing.Point(202, 1);
			this.label_PLCStaus.Name = "label_PLCStaus";
			this.label_PLCStaus.Size = new System.Drawing.Size(39, 23);
			this.label_PLCStaus.TabIndex = 3;
			// 
			// LAB_ScannStaus
			// 
			this.LAB_ScannStaus.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LAB_ScannStaus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
			this.LAB_ScannStaus.Location = new System.Drawing.Point(4, 1);
			this.LAB_ScannStaus.Name = "LAB_ScannStaus";
			this.LAB_ScannStaus.Size = new System.Drawing.Size(69, 23);
			this.LAB_ScannStaus.TabIndex = 0;
			this.LAB_ScannStaus.Text = "Scann";
			this.LAB_ScannStaus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
			this.label10.Location = new System.Drawing.Point(126, 1);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(69, 23);
			this.label10.TabIndex = 2;
			this.label10.Text = "PLC";
			this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label_ScannStaus
			// 
			this.label_ScannStaus.BackColor = System.Drawing.Color.Green;
			this.label_ScannStaus.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label_ScannStaus.ForeColor = System.Drawing.Color.White;
			this.label_ScannStaus.Location = new System.Drawing.Point(80, 1);
			this.label_ScannStaus.Name = "label_ScannStaus";
			this.label_ScannStaus.Size = new System.Drawing.Size(39, 23);
			this.label_ScannStaus.TabIndex = 1;
			// 
			// chart2
			// 
			chartArea3.Name = "ChartArea1";
			this.chart2.ChartAreas.Add(chartArea3);
			legend3.Name = "Legend1";
			this.chart2.Legends.Add(legend3);
			this.chart2.Location = new System.Drawing.Point(505, 11);
			this.chart2.Name = "chart2";
			series3.ChartArea = "ChartArea1";
			series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
			series3.Legend = "Legend1";
			series3.Name = "temperature";
			this.chart2.Series.Add(series3);
			this.chart2.Size = new System.Drawing.Size(470, 300);
			this.chart2.TabIndex = 15;
			this.chart2.Text = "chart2";
			// 
			// chart1
			// 
			chartArea4.Name = "ChartArea1";
			this.chart1.ChartAreas.Add(chartArea4);
			legend4.Name = "Legend1";
			this.chart1.Legends.Add(legend4);
			this.chart1.Location = new System.Drawing.Point(18, 11);
			this.chart1.Name = "chart1";
			series4.ChartArea = "ChartArea1";
			series4.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
			series4.Legend = "Legend1";
			series4.Name = "Prseeure";
			this.chart1.Series.Add(series4);
			this.chart1.Size = new System.Drawing.Size(470, 300);
			this.chart1.TabIndex = 14;
			this.chart1.Text = "chart1";
			// 
			// groupBox2
			// 
			this.groupBox2.BackColor = System.Drawing.Color.Silver;
			this.groupBox2.Controls.Add(this.label12);
			this.groupBox2.Controls.Add(this.label6);
			this.groupBox2.Controls.Add(this.label2);
			this.groupBox2.Controls.Add(this.label5);
			this.groupBox2.Controls.Add(this.label14);
			this.groupBox2.Controls.Add(this.label8);
			this.groupBox2.Controls.Add(this.label7);
			this.groupBox2.Controls.Add(this.label1);
			this.groupBox2.Controls.Add(this.txtCT);
			this.groupBox2.Controls.Add(this.label4);
			this.groupBox2.Controls.Add(this.tHourlyYield);
			this.groupBox2.Controls.Add(this.txtBarcode4);
			this.groupBox2.Controls.Add(this.txtBarcode3);
			this.groupBox2.Controls.Add(this.txtBarcode2);
			this.groupBox2.Controls.Add(this.txtBarcode);
			this.groupBox2.Controls.Add(this.label3);
			this.groupBox2.Controls.Add(this.tHourlyInput);
			this.groupBox2.Controls.Add(this.tHourlyOutput);
			this.groupBox2.Controls.Add(this.tHourlyReject);
			this.groupBox2.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.groupBox2.Location = new System.Drawing.Point(62, 1202);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(436, 347);
			this.groupBox2.TabIndex = 11;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "生产信息";
			// 
			// label12
			// 
			this.label12.Location = new System.Drawing.Point(51, 311);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(69, 23);
			this.label12.TabIndex = 1;
			this.label12.Text = "CT :";
			this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(5, 272);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(115, 23);
			this.label6.TabIndex = 1;
			this.label6.Text = "Hourly Yield:";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label2
			// 
			this.label2.BackColor = System.Drawing.Color.Gainsboro;
			this.label2.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
			this.label2.Location = new System.Drawing.Point(248, 311);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(178, 30);
			this.label2.TabIndex = 4;
			this.label2.Text = "重   置";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label5
			// 
			this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label5.Location = new System.Drawing.Point(3, 30);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(430, 314);
			this.label5.TabIndex = 1;
			this.label5.Text = "Hourly Reject:";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label14
			// 
			this.label14.Location = new System.Drawing.Point(3, 128);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(117, 23);
			this.label14.TabIndex = 1;
			this.label14.Text = "Barcode4:";
			this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label8
			// 
			this.label8.Location = new System.Drawing.Point(3, 93);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(117, 23);
			this.label8.TabIndex = 1;
			this.label8.Text = "Barcode3:";
			this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(3, 58);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(117, 23);
			this.label7.TabIndex = 1;
			this.label7.Text = "Barcode2:";
			this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(7, 19);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(114, 23);
			this.label1.TabIndex = 1;
			this.label1.Text = "Barcode1:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// txtCT
			// 
			this.txtCT.Location = new System.Drawing.Point(126, 307);
			this.txtCT.Name = "txtCT";
			this.txtCT.Size = new System.Drawing.Size(77, 34);
			this.txtCT.TabIndex = 0;
			this.txtCT.Text = "12.3 /s";
			this.txtCT.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(-4, 200);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(125, 23);
			this.label4.TabIndex = 1;
			this.label4.Text = "Hourly Output:";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// tHourlyYield
			// 
			this.tHourlyYield.Location = new System.Drawing.Point(126, 272);
			this.tHourlyYield.Name = "tHourlyYield";
			this.tHourlyYield.Size = new System.Drawing.Size(77, 34);
			this.tHourlyYield.TabIndex = 0;
			this.tHourlyYield.Text = "100%";
			this.tHourlyYield.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// txtBarcode4
			// 
			this.txtBarcode4.Location = new System.Drawing.Point(126, 122);
			this.txtBarcode4.Name = "txtBarcode4";
			this.txtBarcode4.Size = new System.Drawing.Size(300, 34);
			this.txtBarcode4.TabIndex = 0;
			this.txtBarcode4.TabStop = false;
			this.txtBarcode4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// txtBarcode3
			// 
			this.txtBarcode3.Location = new System.Drawing.Point(126, 87);
			this.txtBarcode3.Name = "txtBarcode3";
			this.txtBarcode3.Size = new System.Drawing.Size(300, 34);
			this.txtBarcode3.TabIndex = 0;
			this.txtBarcode3.TabStop = false;
			this.txtBarcode3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// txtBarcode2
			// 
			this.txtBarcode2.Location = new System.Drawing.Point(126, 52);
			this.txtBarcode2.Name = "txtBarcode2";
			this.txtBarcode2.Size = new System.Drawing.Size(300, 34);
			this.txtBarcode2.TabIndex = 0;
			this.txtBarcode2.TabStop = false;
			this.txtBarcode2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// txtBarcode
			// 
			this.txtBarcode.Location = new System.Drawing.Point(126, 17);
			this.txtBarcode.Name = "txtBarcode";
			this.txtBarcode.Size = new System.Drawing.Size(300, 34);
			this.txtBarcode.TabIndex = 0;
			this.txtBarcode.TabStop = false;
			this.txtBarcode.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(3, 167);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(117, 23);
			this.label3.TabIndex = 1;
			this.label3.Text = "Hourly Input:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// tHourlyInput
			// 
			this.tHourlyInput.Location = new System.Drawing.Point(126, 165);
			this.tHourlyInput.Name = "tHourlyInput";
			this.tHourlyInput.Size = new System.Drawing.Size(77, 34);
			this.tHourlyInput.TabIndex = 0;
			this.tHourlyInput.Text = "0";
			this.tHourlyInput.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// tHourlyOutput
			// 
			this.tHourlyOutput.Location = new System.Drawing.Point(126, 200);
			this.tHourlyOutput.Name = "tHourlyOutput";
			this.tHourlyOutput.Size = new System.Drawing.Size(77, 34);
			this.tHourlyOutput.TabIndex = 0;
			this.tHourlyOutput.Text = "0";
			this.tHourlyOutput.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// tHourlyReject
			// 
			this.tHourlyReject.Location = new System.Drawing.Point(126, 237);
			this.tHourlyReject.Name = "tHourlyReject";
			this.tHourlyReject.Size = new System.Drawing.Size(77, 34);
			this.tHourlyReject.TabIndex = 0;
			this.tHourlyReject.Text = "0";
			this.tHourlyReject.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// timer1
			// 
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// imageList1
			// 
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList1.Images.SetKeyName(0, "无标题.png");
			// 
			// HomeForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BackColor = System.Drawing.Color.White;
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.ClientSize = new System.Drawing.Size(1521, 634);
			this.ControlBox = false;
			this.Controls.Add(this.panel1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "HomeForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "HomeForm";
			this.Load += new System.EventHandler(this.HomeForm_Load);
			this.panel1.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.chart2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox txtCT;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tHourlyYield;
        public System.Windows.Forms.TextBox tHourlyReject;
        private System.Windows.Forms.TextBox tHourlyOutput;
        private System.Windows.Forms.TextBox tHourlyInput;
        private System.Windows.Forms.Label label7;
        public System.Windows.Forms.TextBox txtBarcode2;
        public System.Windows.Forms.TextBox txtBarcode;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label8;
        public System.Windows.Forms.TextBox txtBarcode4;
        public System.Windows.Forms.TextBox txtBarcode3;
		private System.Windows.Forms.DataVisualization.Charting.Chart chart2;
		private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label label_PLCStaus;
		private System.Windows.Forms.Label LAB_ScannStaus;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label_ScannStaus;
	}
}