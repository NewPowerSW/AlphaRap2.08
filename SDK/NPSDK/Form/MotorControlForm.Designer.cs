namespace NPSDK
{
	// Token: 0x02000014 RID: 20
	public partial class MotorControlForm : global::System.Windows.Forms.Form
	{
		// Token: 0x0600012B RID: 299 RVA: 0x00005A90 File Offset: 0x00003C90
		protected override void Dispose(bool disposing)
		{
			bool flag = disposing && this.components != null;
			if (flag)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x0600012C RID: 300 RVA: 0x00005AC8 File Offset: 0x00003CC8
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.TextPosition1 = new System.Windows.Forms.TextBox();
            this.TextGap1 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.TextPosition2 = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.TextSpeed = new System.Windows.Forms.TextBox();
            this.button9 = new System.Windows.Forms.Button();
            this.button11 = new System.Windows.Forms.Button();
            this.TextAcceleration = new System.Windows.Forms.TextBox();
            this.button15 = new System.Windows.Forms.Button();
            this.TextDeceleration = new System.Windows.Forms.TextBox();
            this.button19 = new System.Windows.Forms.Button();
            this.button20 = new System.Windows.Forms.Button();
            this.button21 = new System.Windows.Forms.Button();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.labMoveSpeed = new System.Windows.Forms.Label();
            this.labMoveAcceleration = new System.Windows.Forms.Label();
            this.labMoveDeceleration = new System.Windows.Forms.Label();
            this.labGearRatio = new System.Windows.Forms.Label();
            this.labEncoderGearRatio = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.tmrScan = new System.Windows.Forms.Timer(this.components);
            this.button22 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.button23 = new System.Windows.Forms.Button();
            this.label26 = new System.Windows.Forms.Label();
            this.label27 = new System.Windows.Forms.Label();
            this.label28 = new System.Windows.Forms.Label();
            this.labSpdRate = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.button7 = new System.Windows.Forms.Button();
            this.TextSpeedRatio = new System.Windows.Forms.TextBox();
            this.label22 = new System.Windows.Forms.Label();
            this.TextDelay = new System.Windows.Forms.TextBox();
            this.button10 = new System.Windows.Forms.Button();
            this.button12 = new System.Windows.Forms.Button();
            this.plMotorButton = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel11 = new System.Windows.Forms.TableLayoutPanel();
            this.pbHLN = new System.Windows.Forms.PictureBox();
            this.label36 = new System.Windows.Forms.Label();
            this.tableLayoutPanel10 = new System.Windows.Forms.TableLayoutPanel();
            this.pbHLP = new System.Windows.Forms.PictureBox();
            this.label35 = new System.Windows.Forms.Label();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.pbEMG = new System.Windows.Forms.PictureBox();
            this.label25 = new System.Windows.Forms.Label();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.pbSVON = new System.Windows.Forms.PictureBox();
            this.label30 = new System.Windows.Forms.Label();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.pbSMV = new System.Windows.Forms.PictureBox();
            this.label29 = new System.Windows.Forms.Label();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.pbHMV = new System.Windows.Forms.PictureBox();
            this.label31 = new System.Windows.Forms.Label();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.pbALM = new System.Windows.Forms.PictureBox();
            this.label24 = new System.Windows.Forms.Label();
            this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
            this.pbSLP = new System.Windows.Forms.PictureBox();
            this.label32 = new System.Windows.Forms.Label();
            this.tableLayoutPanel8 = new System.Windows.Forms.TableLayoutPanel();
            this.pbORG = new System.Windows.Forms.PictureBox();
            this.label33 = new System.Windows.Forms.Label();
            this.tableLayoutPanel9 = new System.Windows.Forms.TableLayoutPanel();
            this.pbSLN = new System.Windows.Forms.PictureBox();
            this.label34 = new System.Windows.Forms.Label();
            this.textCommandPosition = new System.Windows.Forms.TextBox();
            this.textEncoderPosition = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.TextGap2 = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel11.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbHLN)).BeginInit();
            this.tableLayoutPanel10.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbHLP)).BeginInit();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbEMG)).BeginInit();
            this.tableLayoutPanel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbSVON)).BeginInit();
            this.tableLayoutPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbSMV)).BeginInit();
            this.tableLayoutPanel6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbHMV)).BeginInit();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbALM)).BeginInit();
            this.tableLayoutPanel7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbSLP)).BeginInit();
            this.tableLayoutPanel8.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbORG)).BeginInit();
            this.tableLayoutPanel9.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbSLN)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(5, 170);
            this.label1.Margin = new System.Windows.Forms.Padding(4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(162, 26);
            this.label1.TabIndex = 0;
            this.label1.Text = "Command Position";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(5, 218);
            this.label2.Margin = new System.Windows.Forms.Padding(4);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(162, 26);
            this.label2.TabIndex = 1;
            this.label2.Text = "Encoder Position";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(357, 262);
            this.button1.Margin = new System.Windows.Forms.Padding(5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(106, 29);
            this.button1.TabIndex = 5;
            this.button1.Text = "G O T O # 1";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // TextPosition1
            // 
            this.TextPosition1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TextPosition1.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextPosition1.Location = new System.Drawing.Point(175, 262);
            this.TextPosition1.Margin = new System.Windows.Forms.Padding(5);
            this.TextPosition1.Name = "TextPosition1";
            this.TextPosition1.Size = new System.Drawing.Size(85, 29);
            this.TextPosition1.TabIndex = 7;
            this.TextPosition1.Text = "0";
            this.TextPosition1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.TextPosition1.DoubleClick += new System.EventHandler(this.TextPosition1_DoubleClick);
            this.TextPosition1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextPosition1_KeyDown);
            this.TextPosition1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextPosition1_KeyPress);
            // 
            // TextGap1
            // 
            this.TextGap1.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextGap1.Location = new System.Drawing.Point(296, 262);
            this.TextGap1.Margin = new System.Windows.Forms.Padding(5);
            this.TextGap1.Name = "TextGap1";
            this.TextGap1.Size = new System.Drawing.Size(54, 29);
            this.TextGap1.TabIndex = 10;
            this.TextGap1.Text = "0";
            this.TextGap1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.TextGap1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextPosition1_KeyPress);
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(5, 263);
            this.label3.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(162, 26);
            this.label3.TabIndex = 13;
            this.label3.Text = "Target Position #1";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // button3
            // 
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button3.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button3.Location = new System.Drawing.Point(261, 262);
            this.button3.Margin = new System.Windows.Forms.Padding(5);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(34, 29);
            this.button3.TabIndex = 14;
            this.button3.Text = "+";
            this.button3.UseVisualStyleBackColor = false;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // label13
            // 
            this.label13.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(5, 343);
            this.label13.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(162, 26);
            this.label13.TabIndex = 18;
            this.label13.Text = "Target Position #2";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // TextPosition2
            // 
            this.TextPosition2.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextPosition2.Location = new System.Drawing.Point(175, 342);
            this.TextPosition2.Margin = new System.Windows.Forms.Padding(5);
            this.TextPosition2.Name = "TextPosition2";
            this.TextPosition2.Size = new System.Drawing.Size(85, 29);
            this.TextPosition2.TabIndex = 17;
            this.TextPosition2.Text = "0";
            this.TextPosition2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.TextPosition2.DoubleClick += new System.EventHandler(this.TextPosition2_DoubleClick);
            this.TextPosition2.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextPosition1_KeyPress);
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.Location = new System.Drawing.Point(357, 342);
            this.button2.Margin = new System.Windows.Forms.Padding(5);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(106, 29);
            this.button2.TabIndex = 16;
            this.button2.Text = "G O T O # 2";
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button4
            // 
            this.button4.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button4.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button4.Location = new System.Drawing.Point(261, 342);
            this.button4.Margin = new System.Windows.Forms.Padding(5);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(34, 28);
            this.button4.TabIndex = 19;
            this.button4.Text = "+";
            this.button4.UseVisualStyleBackColor = false;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button5.Location = new System.Drawing.Point(357, 303);
            this.button5.Margin = new System.Windows.Forms.Padding(5);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(106, 29);
            this.button5.TabIndex = 21;
            this.button5.Text = "L O O P";
            this.button5.UseVisualStyleBackColor = false;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button6
            // 
            this.button6.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button6.ForeColor = System.Drawing.Color.Red;
            this.button6.Location = new System.Drawing.Point(473, 196);
            this.button6.Margin = new System.Windows.Forms.Padding(5);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(124, 79);
            this.button6.TabIndex = 22;
            this.button6.Text = "S T O P";
            this.button6.UseVisualStyleBackColor = false;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // TextSpeed
            // 
            this.TextSpeed.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextSpeed.Location = new System.Drawing.Point(513, 392);
            this.TextSpeed.Margin = new System.Windows.Forms.Padding(5);
            this.TextSpeed.Name = "TextSpeed";
            this.TextSpeed.Size = new System.Drawing.Size(83, 29);
            this.TextSpeed.TabIndex = 24;
            this.TextSpeed.Text = "50";
            this.TextSpeed.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.TextSpeed.WordWrap = false;
            this.TextSpeed.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextPosition1_KeyPress);
            // 
            // button9
            // 
            this.button9.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button9.Location = new System.Drawing.Point(322, 390);
            this.button9.Margin = new System.Windows.Forms.Padding(5);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(190, 32);
            this.button9.TabIndex = 27;
            this.button9.Text = "Set Speed";
            this.button9.UseVisualStyleBackColor = false;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // button11
            // 
            this.button11.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button11.Location = new System.Drawing.Point(322, 466);
            this.button11.Margin = new System.Windows.Forms.Padding(5);
            this.button11.Name = "button11";
            this.button11.Size = new System.Drawing.Size(190, 32);
            this.button11.TabIndex = 27;
            this.button11.Text = "Set Acceleration";
            this.button11.UseVisualStyleBackColor = false;
            this.button11.Click += new System.EventHandler(this.button11_Click);
            // 
            // TextAcceleration
            // 
            this.TextAcceleration.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TextAcceleration.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextAcceleration.Location = new System.Drawing.Point(513, 468);
            this.TextAcceleration.Margin = new System.Windows.Forms.Padding(5);
            this.TextAcceleration.Name = "TextAcceleration";
            this.TextAcceleration.Size = new System.Drawing.Size(83, 29);
            this.TextAcceleration.TabIndex = 24;
            this.TextAcceleration.Text = "0.2";
            this.TextAcceleration.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.TextAcceleration.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextPosition1_KeyPress);
            // 
            // button15
            // 
            this.button15.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button15.Location = new System.Drawing.Point(322, 504);
            this.button15.Margin = new System.Windows.Forms.Padding(5);
            this.button15.Name = "button15";
            this.button15.Size = new System.Drawing.Size(190, 32);
            this.button15.TabIndex = 27;
            this.button15.Text = "Set Deceleration";
            this.button15.UseVisualStyleBackColor = false;
            this.button15.Click += new System.EventHandler(this.button15_Click);
            // 
            // TextDeceleration
            // 
            this.TextDeceleration.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TextDeceleration.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextDeceleration.Location = new System.Drawing.Point(513, 506);
            this.TextDeceleration.Margin = new System.Windows.Forms.Padding(5);
            this.TextDeceleration.Name = "TextDeceleration";
            this.TextDeceleration.Size = new System.Drawing.Size(83, 29);
            this.TextDeceleration.TabIndex = 24;
            this.TextDeceleration.Text = "0.2";
            this.TextDeceleration.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.TextDeceleration.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextPosition1_KeyPress);
            // 
            // button19
            // 
            this.button19.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button19.Location = new System.Drawing.Point(252, 100);
            this.button19.Margin = new System.Windows.Forms.Padding(5);
            this.button19.Name = "button19";
            this.button19.Size = new System.Drawing.Size(211, 46);
            this.button19.TabIndex = 32;
            this.button19.Text = "H O M E";
            this.button19.UseVisualStyleBackColor = false;
            this.button19.Click += new System.EventHandler(this.button19_Click);
            // 
            // button20
            // 
            this.button20.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button20.Location = new System.Drawing.Point(473, 282);
            this.button20.Margin = new System.Windows.Forms.Padding(5);
            this.button20.Name = "button20";
            this.button20.Size = new System.Drawing.Size(123, 89);
            this.button20.TabIndex = 33;
            this.button20.Text = "J O G -";
            this.button20.UseVisualStyleBackColor = false;
            this.button20.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button20_MouseDown);
            this.button20.MouseUp += new System.Windows.Forms.MouseEventHandler(this.button21_MouseUp);
            // 
            // button21
            // 
            this.button21.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button21.Location = new System.Drawing.Point(473, 100);
            this.button21.Margin = new System.Windows.Forms.Padding(5);
            this.button21.Name = "button21";
            this.button21.Size = new System.Drawing.Size(123, 89);
            this.button21.TabIndex = 34;
            this.button21.Text = "J O G +";
            this.button21.UseVisualStyleBackColor = false;
            this.button21.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button21_MouseDown);
            this.button21.MouseUp += new System.Windows.Forms.MouseEventHandler(this.button21_MouseUp);
            // 
            // label14
            // 
            this.label14.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.Location = new System.Drawing.Point(5, 393);
            this.label14.Margin = new System.Windows.Forms.Padding(4);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(162, 26);
            this.label14.TabIndex = 35;
            this.label14.Text = "Work Speed";
            this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label15
            // 
            this.label15.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.Location = new System.Drawing.Point(5, 469);
            this.label15.Margin = new System.Windows.Forms.Padding(4);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(162, 26);
            this.label15.TabIndex = 36;
            this.label15.Text = "Acceleration";
            this.label15.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label16
            // 
            this.label16.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.Location = new System.Drawing.Point(5, 507);
            this.label16.Margin = new System.Windows.Forms.Padding(4);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(162, 26);
            this.label16.TabIndex = 37;
            this.label16.Text = "Deceleration";
            this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label17
            // 
            this.label17.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label17.Location = new System.Drawing.Point(5, 545);
            this.label17.Margin = new System.Windows.Forms.Padding(4);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(162, 26);
            this.label17.TabIndex = 38;
            this.label17.Text = "Gear Ratio";
            this.label17.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label18
            // 
            this.label18.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label18.Location = new System.Drawing.Point(5, 583);
            this.label18.Margin = new System.Windows.Forms.Padding(4);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(162, 26);
            this.label18.TabIndex = 39;
            this.label18.Text = "Encoder Gear Ratio";
            this.label18.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labMoveSpeed
            // 
            this.labMoveSpeed.BackColor = System.Drawing.Color.WhiteSmoke;
            this.labMoveSpeed.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labMoveSpeed.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labMoveSpeed.Location = new System.Drawing.Point(175, 393);
            this.labMoveSpeed.Margin = new System.Windows.Forms.Padding(4);
            this.labMoveSpeed.Name = "labMoveSpeed";
            this.labMoveSpeed.Size = new System.Drawing.Size(85, 27);
            this.labMoveSpeed.TabIndex = 42;
            this.labMoveSpeed.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labMoveAcceleration
            // 
            this.labMoveAcceleration.BackColor = System.Drawing.Color.WhiteSmoke;
            this.labMoveAcceleration.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labMoveAcceleration.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labMoveAcceleration.Location = new System.Drawing.Point(175, 469);
            this.labMoveAcceleration.Margin = new System.Windows.Forms.Padding(4);
            this.labMoveAcceleration.Name = "labMoveAcceleration";
            this.labMoveAcceleration.Size = new System.Drawing.Size(85, 27);
            this.labMoveAcceleration.TabIndex = 43;
            this.labMoveAcceleration.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labMoveDeceleration
            // 
            this.labMoveDeceleration.BackColor = System.Drawing.Color.WhiteSmoke;
            this.labMoveDeceleration.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labMoveDeceleration.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labMoveDeceleration.Location = new System.Drawing.Point(175, 507);
            this.labMoveDeceleration.Margin = new System.Windows.Forms.Padding(4);
            this.labMoveDeceleration.Name = "labMoveDeceleration";
            this.labMoveDeceleration.Size = new System.Drawing.Size(85, 27);
            this.labMoveDeceleration.TabIndex = 44;
            this.labMoveDeceleration.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labGearRatio
            // 
            this.labGearRatio.BackColor = System.Drawing.Color.WhiteSmoke;
            this.labGearRatio.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labGearRatio.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labGearRatio.Location = new System.Drawing.Point(175, 545);
            this.labGearRatio.Margin = new System.Windows.Forms.Padding(4);
            this.labGearRatio.Name = "labGearRatio";
            this.labGearRatio.Size = new System.Drawing.Size(85, 27);
            this.labGearRatio.TabIndex = 45;
            this.labGearRatio.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labEncoderGearRatio
            // 
            this.labEncoderGearRatio.BackColor = System.Drawing.Color.WhiteSmoke;
            this.labEncoderGearRatio.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labEncoderGearRatio.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labEncoderGearRatio.Location = new System.Drawing.Point(175, 583);
            this.labEncoderGearRatio.Margin = new System.Windows.Forms.Padding(4);
            this.labEncoderGearRatio.Name = "labEncoderGearRatio";
            this.labEncoderGearRatio.Size = new System.Drawing.Size(85, 27);
            this.labEncoderGearRatio.TabIndex = 46;
            this.labEncoderGearRatio.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnClose
            // 
            this.btnClose.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.Location = new System.Drawing.Point(512, 622);
            this.btnClose.Margin = new System.Windows.Forms.Padding(5);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(84, 82);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // tmrScan
            // 
            this.tmrScan.Interval = 10;
            this.tmrScan.Tick += new System.EventHandler(this.tmrScan_Tick);
            // 
            // button22
            // 
            this.button22.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button22.Location = new System.Drawing.Point(322, 542);
            this.button22.Margin = new System.Windows.Forms.Padding(5);
            this.button22.Name = "button22";
            this.button22.Size = new System.Drawing.Size(190, 32);
            this.button22.TabIndex = 54;
            this.button22.Text = "Set Gear Ratio";
            this.button22.UseVisualStyleBackColor = false;
            this.button22.Click += new System.EventHandler(this.button22_Click);
            // 
            // textBox1
            // 
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox1.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.Location = new System.Drawing.Point(513, 544);
            this.textBox1.Margin = new System.Windows.Forms.Padding(5);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(83, 29);
            this.textBox1.TabIndex = 55;
            this.textBox1.Text = "1";
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextPosition1_KeyPress);
            // 
            // textBox2
            // 
            this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox2.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox2.Location = new System.Drawing.Point(513, 582);
            this.textBox2.Margin = new System.Windows.Forms.Padding(5);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(83, 29);
            this.textBox2.TabIndex = 57;
            this.textBox2.Text = "1";
            this.textBox2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBox2.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextPosition1_KeyPress);
            // 
            // button23
            // 
            this.button23.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button23.Location = new System.Drawing.Point(322, 580);
            this.button23.Margin = new System.Windows.Forms.Padding(5);
            this.button23.Name = "button23";
            this.button23.Size = new System.Drawing.Size(190, 32);
            this.button23.TabIndex = 56;
            this.button23.Text = "Set Encoder Gear Ratio";
            this.button23.UseVisualStyleBackColor = false;
            this.button23.Click += new System.EventHandler(this.button23_Click);
            // 
            // label26
            // 
            this.label26.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label26.Location = new System.Drawing.Point(262, 507);
            this.label26.Margin = new System.Windows.Forms.Padding(4);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(53, 26);
            this.label26.TabIndex = 63;
            this.label26.Text = "sec";
            this.label26.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label27
            // 
            this.label27.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label27.Location = new System.Drawing.Point(262, 469);
            this.label27.Margin = new System.Windows.Forms.Padding(4);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(53, 26);
            this.label27.TabIndex = 62;
            this.label27.Text = "sec";
            this.label27.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label28
            // 
            this.label28.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label28.Location = new System.Drawing.Point(262, 393);
            this.label28.Margin = new System.Windows.Forms.Padding(4);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(53, 26);
            this.label28.TabIndex = 61;
            this.label28.Text = "mm/s";
            this.label28.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labSpdRate
            // 
            this.labSpdRate.BackColor = System.Drawing.Color.WhiteSmoke;
            this.labSpdRate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labSpdRate.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labSpdRate.Location = new System.Drawing.Point(175, 431);
            this.labSpdRate.Margin = new System.Windows.Forms.Padding(4);
            this.labSpdRate.Name = "labSpdRate";
            this.labSpdRate.Size = new System.Drawing.Size(85, 27);
            this.labSpdRate.TabIndex = 65;
            this.labSpdRate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label23
            // 
            this.label23.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label23.Location = new System.Drawing.Point(5, 431);
            this.label23.Margin = new System.Windows.Forms.Padding(4);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(162, 26);
            this.label23.TabIndex = 64;
            this.label23.Text = "Speed Ratio";
            this.label23.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label21
            // 
            this.label21.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label21.Location = new System.Drawing.Point(262, 431);
            this.label21.Margin = new System.Windows.Forms.Padding(4);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(53, 26);
            this.label21.TabIndex = 66;
            this.label21.Text = "%";
            this.label21.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // button7
            // 
            this.button7.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button7.Location = new System.Drawing.Point(322, 428);
            this.button7.Margin = new System.Windows.Forms.Padding(5);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(190, 32);
            this.button7.TabIndex = 67;
            this.button7.Text = "Set Speed Rate";
            this.button7.UseVisualStyleBackColor = false;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // TextSpeedRatio
            // 
            this.TextSpeedRatio.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TextSpeedRatio.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextSpeedRatio.Location = new System.Drawing.Point(513, 430);
            this.TextSpeedRatio.Margin = new System.Windows.Forms.Padding(5);
            this.TextSpeedRatio.Name = "TextSpeedRatio";
            this.TextSpeedRatio.Size = new System.Drawing.Size(83, 29);
            this.TextSpeedRatio.TabIndex = 68;
            this.TextSpeedRatio.Text = "100";
            this.TextSpeedRatio.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.TextSpeedRatio.WordWrap = false;
            this.TextSpeedRatio.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextPosition1_KeyPress);
            // 
            // label22
            // 
            this.label22.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label22.Location = new System.Drawing.Point(5, 304);
            this.label22.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(162, 26);
            this.label22.TabIndex = 70;
            this.label22.Text = "Loop Delay";
            this.label22.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // TextDelay
            // 
            this.TextDelay.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextDelay.Location = new System.Drawing.Point(175, 303);
            this.TextDelay.Margin = new System.Windows.Forms.Padding(5);
            this.TextDelay.Name = "TextDelay";
            this.TextDelay.Size = new System.Drawing.Size(85, 29);
            this.TextDelay.TabIndex = 69;
            this.TextDelay.Text = "0";
            this.TextDelay.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.TextDelay.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextPosition1_KeyPress);
            // 
            // button10
            // 
            this.button10.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button10.Location = new System.Drawing.Point(9, 100);
            this.button10.Margin = new System.Windows.Forms.Padding(5);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(116, 46);
            this.button10.TabIndex = 72;
            this.button10.Text = "Servo On";
            this.button10.UseVisualStyleBackColor = false;
            this.button10.Click += new System.EventHandler(this.button10_Click);
            // 
            // button12
            // 
            this.button12.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button12.Location = new System.Drawing.Point(130, 100);
            this.button12.Margin = new System.Windows.Forms.Padding(5);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(116, 46);
            this.button12.TabIndex = 73;
            this.button12.Text = "Servo Off";
            this.button12.UseVisualStyleBackColor = false;
            this.button12.Click += new System.EventHandler(this.button12_Click);
            // 
            // plMotorButton
            // 
            this.plMotorButton.AutoScroll = true;
            this.plMotorButton.BackColor = System.Drawing.Color.WhiteSmoke;
            this.plMotorButton.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.plMotorButton.Location = new System.Drawing.Point(14, 622);
            this.plMotorButton.Margin = new System.Windows.Forms.Padding(5);
            this.plMotorButton.Name = "plMotorButton";
            this.plMotorButton.Size = new System.Drawing.Size(498, 82);
            this.plMotorButton.TabIndex = 74;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel11, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel10, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel5, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel4, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel6, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel7, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel8, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel9, 2, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(610, 83);
            this.tableLayoutPanel1.TabIndex = 75;
            // 
            // tableLayoutPanel11
            // 
            this.tableLayoutPanel11.ColumnCount = 2;
            this.tableLayoutPanel11.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel11.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel11.Controls.Add(this.pbHLN, 0, 0);
            this.tableLayoutPanel11.Controls.Add(this.label36, 1, 0);
            this.tableLayoutPanel11.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel11.Location = new System.Drawing.Point(248, 45);
            this.tableLayoutPanel11.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel11.Name = "tableLayoutPanel11";
            this.tableLayoutPanel11.RowCount = 1;
            this.tableLayoutPanel11.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel11.Size = new System.Drawing.Size(114, 34);
            this.tableLayoutPanel11.TabIndex = 86;
            // 
            // pbHLN
            // 
            this.pbHLN.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbHLN.Image = global::NPSDK.Resource.ledoff;
            this.pbHLN.Location = new System.Drawing.Point(4, 4);
            this.pbHLN.Margin = new System.Windows.Forms.Padding(4);
            this.pbHLN.Name = "pbHLN";
            this.pbHLN.Size = new System.Drawing.Size(27, 26);
            this.pbHLN.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbHLN.TabIndex = 76;
            this.pbHLN.TabStop = false;
            // 
            // label36
            // 
            this.label36.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label36.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label36.Location = new System.Drawing.Point(39, 0);
            this.label36.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(71, 34);
            this.label36.TabIndex = 77;
            this.label36.Text = "PEL";
            this.label36.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel10
            // 
            this.tableLayoutPanel10.ColumnCount = 2;
            this.tableLayoutPanel10.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel10.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel10.Controls.Add(this.pbHLP, 0, 0);
            this.tableLayoutPanel10.Controls.Add(this.label35, 1, 0);
            this.tableLayoutPanel10.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel10.Location = new System.Drawing.Point(126, 45);
            this.tableLayoutPanel10.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel10.Name = "tableLayoutPanel10";
            this.tableLayoutPanel10.RowCount = 1;
            this.tableLayoutPanel10.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel10.Size = new System.Drawing.Size(114, 34);
            this.tableLayoutPanel10.TabIndex = 85;
            // 
            // pbHLP
            // 
            this.pbHLP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbHLP.Image = global::NPSDK.Resource.ledoff;
            this.pbHLP.Location = new System.Drawing.Point(4, 4);
            this.pbHLP.Margin = new System.Windows.Forms.Padding(4);
            this.pbHLP.Name = "pbHLP";
            this.pbHLP.Size = new System.Drawing.Size(27, 26);
            this.pbHLP.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbHLP.TabIndex = 76;
            this.pbHLP.TabStop = false;
            // 
            // label35
            // 
            this.label35.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label35.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label35.Location = new System.Drawing.Point(39, 0);
            this.label35.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(71, 34);
            this.label35.TabIndex = 77;
            this.label35.Text = "MEL";
            this.label35.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.pbEMG, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.label25, 1, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(126, 4);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(114, 33);
            this.tableLayoutPanel3.TabIndex = 78;
            // 
            // pbEMG
            // 
            this.pbEMG.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbEMG.Image = global::NPSDK.Resource.ledoff;
            this.pbEMG.Location = new System.Drawing.Point(4, 4);
            this.pbEMG.Margin = new System.Windows.Forms.Padding(4);
            this.pbEMG.Name = "pbEMG";
            this.pbEMG.Size = new System.Drawing.Size(27, 25);
            this.pbEMG.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbEMG.TabIndex = 76;
            this.pbEMG.TabStop = false;
            // 
            // label25
            // 
            this.label25.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label25.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label25.Location = new System.Drawing.Point(39, 0);
            this.label25.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(71, 33);
            this.label25.TabIndex = 77;
            this.label25.Text = "EMG";
            this.label25.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Controls.Add(this.pbSVON, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.label30, 1, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(248, 4);
            this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 1;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(114, 33);
            this.tableLayoutPanel5.TabIndex = 80;
            // 
            // pbSVON
            // 
            this.pbSVON.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbSVON.Image = global::NPSDK.Resource.ledoff;
            this.pbSVON.Location = new System.Drawing.Point(4, 4);
            this.pbSVON.Margin = new System.Windows.Forms.Padding(4);
            this.pbSVON.Name = "pbSVON";
            this.pbSVON.Size = new System.Drawing.Size(27, 25);
            this.pbSVON.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbSVON.TabIndex = 76;
            this.pbSVON.TabStop = false;
            // 
            // label30
            // 
            this.label30.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label30.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label30.Location = new System.Drawing.Point(39, 0);
            this.label30.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(71, 33);
            this.label30.TabIndex = 77;
            this.label30.Text = "SVON";
            this.label30.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Controls.Add(this.pbSMV, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.label29, 1, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(370, 4);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(114, 33);
            this.tableLayoutPanel4.TabIndex = 79;
            // 
            // pbSMV
            // 
            this.pbSMV.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbSMV.Image = global::NPSDK.Resource.ledoff;
            this.pbSMV.Location = new System.Drawing.Point(4, 4);
            this.pbSMV.Margin = new System.Windows.Forms.Padding(4);
            this.pbSMV.Name = "pbSMV";
            this.pbSMV.Size = new System.Drawing.Size(27, 25);
            this.pbSMV.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbSMV.TabIndex = 76;
            this.pbSMV.TabStop = false;
            // 
            // label29
            // 
            this.label29.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label29.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label29.Location = new System.Drawing.Point(39, 0);
            this.label29.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(71, 33);
            this.label29.TabIndex = 77;
            this.label29.Text = "SMV";
            this.label29.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 2;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.Controls.Add(this.pbHMV, 0, 0);
            this.tableLayoutPanel6.Controls.Add(this.label31, 1, 0);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(492, 4);
            this.tableLayoutPanel6.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 1;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(114, 33);
            this.tableLayoutPanel6.TabIndex = 81;
            // 
            // pbHMV
            // 
            this.pbHMV.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbHMV.Image = global::NPSDK.Resource.ledoff;
            this.pbHMV.Location = new System.Drawing.Point(4, 4);
            this.pbHMV.Margin = new System.Windows.Forms.Padding(4);
            this.pbHMV.Name = "pbHMV";
            this.pbHMV.Size = new System.Drawing.Size(27, 25);
            this.pbHMV.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbHMV.TabIndex = 76;
            this.pbHMV.TabStop = false;
            // 
            // label31
            // 
            this.label31.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label31.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label31.Location = new System.Drawing.Point(39, 0);
            this.label31.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(71, 33);
            this.label31.TabIndex = 77;
            this.label31.Text = "HMV";
            this.label31.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.pbALM, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.label24, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(4, 4);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(114, 33);
            this.tableLayoutPanel2.TabIndex = 76;
            // 
            // pbALM
            // 
            this.pbALM.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbALM.Image = global::NPSDK.Resource.ledoff;
            this.pbALM.Location = new System.Drawing.Point(4, 4);
            this.pbALM.Margin = new System.Windows.Forms.Padding(4);
            this.pbALM.Name = "pbALM";
            this.pbALM.Size = new System.Drawing.Size(27, 25);
            this.pbALM.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbALM.TabIndex = 76;
            this.pbALM.TabStop = false;
            // 
            // label24
            // 
            this.label24.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label24.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label24.Location = new System.Drawing.Point(39, 0);
            this.label24.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(71, 33);
            this.label24.TabIndex = 77;
            this.label24.Text = "ALM";
            this.label24.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel7
            // 
            this.tableLayoutPanel7.ColumnCount = 2;
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel7.Controls.Add(this.pbSLP, 0, 0);
            this.tableLayoutPanel7.Controls.Add(this.label32, 1, 0);
            this.tableLayoutPanel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel7.Location = new System.Drawing.Point(370, 45);
            this.tableLayoutPanel7.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel7.Name = "tableLayoutPanel7";
            this.tableLayoutPanel7.RowCount = 1;
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel7.Size = new System.Drawing.Size(114, 34);
            this.tableLayoutPanel7.TabIndex = 82;
            // 
            // pbSLP
            // 
            this.pbSLP.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbSLP.Image = global::NPSDK.Resource.ledoff;
            this.pbSLP.Location = new System.Drawing.Point(4, 4);
            this.pbSLP.Margin = new System.Windows.Forms.Padding(4);
            this.pbSLP.Name = "pbSLP";
            this.pbSLP.Size = new System.Drawing.Size(27, 26);
            this.pbSLP.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbSLP.TabIndex = 76;
            this.pbSLP.TabStop = false;
            // 
            // label32
            // 
            this.label32.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label32.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label32.Location = new System.Drawing.Point(39, 0);
            this.label32.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(71, 34);
            this.label32.TabIndex = 77;
            this.label32.Text = "SLP";
            this.label32.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel8
            // 
            this.tableLayoutPanel8.ColumnCount = 2;
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel8.Controls.Add(this.pbORG, 0, 0);
            this.tableLayoutPanel8.Controls.Add(this.label33, 1, 0);
            this.tableLayoutPanel8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel8.Location = new System.Drawing.Point(4, 45);
            this.tableLayoutPanel8.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel8.Name = "tableLayoutPanel8";
            this.tableLayoutPanel8.RowCount = 1;
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel8.Size = new System.Drawing.Size(114, 34);
            this.tableLayoutPanel8.TabIndex = 83;
            // 
            // pbORG
            // 
            this.pbORG.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbORG.Image = global::NPSDK.Resource.ledoff;
            this.pbORG.Location = new System.Drawing.Point(4, 4);
            this.pbORG.Margin = new System.Windows.Forms.Padding(4);
            this.pbORG.Name = "pbORG";
            this.pbORG.Size = new System.Drawing.Size(27, 26);
            this.pbORG.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbORG.TabIndex = 76;
            this.pbORG.TabStop = false;
            // 
            // label33
            // 
            this.label33.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label33.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label33.Location = new System.Drawing.Point(39, 0);
            this.label33.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(71, 34);
            this.label33.TabIndex = 77;
            this.label33.Text = "ORG";
            this.label33.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel9
            // 
            this.tableLayoutPanel9.ColumnCount = 2;
            this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel9.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel9.Controls.Add(this.pbSLN, 0, 0);
            this.tableLayoutPanel9.Controls.Add(this.label34, 1, 0);
            this.tableLayoutPanel9.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel9.Location = new System.Drawing.Point(492, 45);
            this.tableLayoutPanel9.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel9.Name = "tableLayoutPanel9";
            this.tableLayoutPanel9.RowCount = 1;
            this.tableLayoutPanel9.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel9.Size = new System.Drawing.Size(114, 34);
            this.tableLayoutPanel9.TabIndex = 84;
            // 
            // pbSLN
            // 
            this.pbSLN.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbSLN.Image = global::NPSDK.Resource.ledoff;
            this.pbSLN.Location = new System.Drawing.Point(4, 4);
            this.pbSLN.Margin = new System.Windows.Forms.Padding(4);
            this.pbSLN.Name = "pbSLN";
            this.pbSLN.Size = new System.Drawing.Size(27, 26);
            this.pbSLN.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbSLN.TabIndex = 76;
            this.pbSLN.TabStop = false;
            // 
            // label34
            // 
            this.label34.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label34.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label34.Location = new System.Drawing.Point(39, 0);
            this.label34.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(71, 34);
            this.label34.TabIndex = 77;
            this.label34.Text = "SLN";
            this.label34.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textCommandPosition
            // 
            this.textCommandPosition.BackColor = System.Drawing.Color.WhiteSmoke;
            this.textCommandPosition.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textCommandPosition.Font = new System.Drawing.Font("Calibri", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textCommandPosition.Location = new System.Drawing.Point(175, 163);
            this.textCommandPosition.Margin = new System.Windows.Forms.Padding(4);
            this.textCommandPosition.Name = "textCommandPosition";
            this.textCommandPosition.Size = new System.Drawing.Size(288, 40);
            this.textCommandPosition.TabIndex = 76;
            this.textCommandPosition.Text = "0";
            this.textCommandPosition.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textEncoderPosition
            // 
            this.textEncoderPosition.BackColor = System.Drawing.Color.WhiteSmoke;
            this.textEncoderPosition.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textEncoderPosition.Font = new System.Drawing.Font("Calibri", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textEncoderPosition.Location = new System.Drawing.Point(175, 211);
            this.textEncoderPosition.Margin = new System.Windows.Forms.Padding(4);
            this.textEncoderPosition.Name = "textEncoderPosition";
            this.textEncoderPosition.Size = new System.Drawing.Size(288, 40);
            this.textEncoderPosition.TabIndex = 77;
            this.textEncoderPosition.Text = "0";
            this.textEncoderPosition.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(262, 302);
            this.label4.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(34, 30);
            this.label4.TabIndex = 78;
            this.label4.Text = "ms";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // TextGap2
            // 
            this.TextGap2.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextGap2.Location = new System.Drawing.Point(296, 342);
            this.TextGap2.Margin = new System.Windows.Forms.Padding(5);
            this.TextGap2.Name = "TextGap2";
            this.TextGap2.Size = new System.Drawing.Size(54, 29);
            this.TextGap2.TabIndex = 79;
            this.TextGap2.Text = "0";
            this.TextGap2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.TextGap2.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextPosition1_KeyPress);
            // 
            // MotorControlForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(610, 710);
            this.ControlBox = false;
            this.Controls.Add(this.TextGap2);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.button21);
            this.Controls.Add(this.plMotorButton);
            this.Controls.Add(this.textEncoderPosition);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button15);
            this.Controls.Add(this.textCommandPosition);
            this.Controls.Add(this.TextDeceleration);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button11);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.TextAcceleration);
            this.Controls.Add(this.TextPosition1);
            this.Controls.Add(this.TextSpeedRatio);
            this.Controls.Add(this.button12);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.TextGap1);
            this.Controls.Add(this.button9);
            this.Controls.Add(this.button10);
            this.Controls.Add(this.TextSpeed);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label21);
            this.Controls.Add(this.label22);
            this.Controls.Add(this.labSpdRate);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.label23);
            this.Controls.Add(this.TextDelay);
            this.Controls.Add(this.label26);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label27);
            this.Controls.Add(this.TextPosition2);
            this.Controls.Add(this.label28);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button23);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button22);
            this.Controls.Add(this.button19);
            this.Controls.Add(this.button20);
            this.Controls.Add(this.labEncoderGearRatio);
            this.Controls.Add(this.labGearRatio);
            this.Controls.Add(this.labMoveDeceleration);
            this.Controls.Add(this.labMoveAcceleration);
            this.Controls.Add(this.labMoveSpeed);
            this.Controls.Add(this.label18);
            this.Controls.Add(this.label17);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.label14);
            this.Font = new System.Drawing.Font("Arial Narrow", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MotorControlForm";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Shown += new System.EventHandler(this.MotorControlForm_Shown);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel11.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbHLN)).EndInit();
            this.tableLayoutPanel10.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbHLP)).EndInit();
            this.tableLayoutPanel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbEMG)).EndInit();
            this.tableLayoutPanel5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbSVON)).EndInit();
            this.tableLayoutPanel4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbSMV)).EndInit();
            this.tableLayoutPanel6.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbHMV)).EndInit();
            this.tableLayoutPanel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbALM)).EndInit();
            this.tableLayoutPanel7.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbSLP)).EndInit();
            this.tableLayoutPanel8.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbORG)).EndInit();
            this.tableLayoutPanel9.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbSLN)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		// Token: 0x0400008A RID: 138
		private global::System.ComponentModel.IContainer components = null;

		// Token: 0x0400008B RID: 139
		private global::System.Windows.Forms.Label label1;

		// Token: 0x0400008C RID: 140
		private global::System.Windows.Forms.Label label2;

		// Token: 0x0400008D RID: 141
		private global::System.Windows.Forms.Button button1;

		// Token: 0x0400008E RID: 142
		private global::System.Windows.Forms.TextBox TextPosition1;

		// Token: 0x0400008F RID: 143
		private global::System.Windows.Forms.TextBox TextGap1;

		// Token: 0x04000090 RID: 144
		private global::System.Windows.Forms.Label label3;

		// Token: 0x04000091 RID: 145
		private global::System.Windows.Forms.Button button3;

		// Token: 0x04000092 RID: 146
		private global::System.Windows.Forms.Label label13;

		// Token: 0x04000093 RID: 147
		private global::System.Windows.Forms.TextBox TextPosition2;

		// Token: 0x04000094 RID: 148
		private global::System.Windows.Forms.Button button2;

		// Token: 0x04000095 RID: 149
		private global::System.Windows.Forms.Button button4;

		// Token: 0x04000096 RID: 150
		private global::System.Windows.Forms.Button button5;

		// Token: 0x04000097 RID: 151
		private global::System.Windows.Forms.Button button6;

		// Token: 0x04000098 RID: 152
		private global::System.Windows.Forms.TextBox TextSpeed;

		// Token: 0x04000099 RID: 153
		private global::System.Windows.Forms.Button button9;

		// Token: 0x0400009A RID: 154
		private global::System.Windows.Forms.Button button11;

		// Token: 0x0400009B RID: 155
		private global::System.Windows.Forms.TextBox TextAcceleration;

		// Token: 0x0400009C RID: 156
		private global::System.Windows.Forms.Button button15;

		// Token: 0x0400009D RID: 157
		private global::System.Windows.Forms.TextBox TextDeceleration;

		// Token: 0x0400009E RID: 158
		private global::System.Windows.Forms.Button button19;

		// Token: 0x0400009F RID: 159
		private global::System.Windows.Forms.Button button20;

		// Token: 0x040000A0 RID: 160
		private global::System.Windows.Forms.Button button21;

		// Token: 0x040000A1 RID: 161
		private global::System.Windows.Forms.Label label14;

		// Token: 0x040000A2 RID: 162
		private global::System.Windows.Forms.Label label15;

		// Token: 0x040000A3 RID: 163
		private global::System.Windows.Forms.Label label16;

		// Token: 0x040000A4 RID: 164
		private global::System.Windows.Forms.Label label17;

		// Token: 0x040000A5 RID: 165
		private global::System.Windows.Forms.Label label18;

		// Token: 0x040000A6 RID: 166
		private global::System.Windows.Forms.Label labMoveSpeed;

		// Token: 0x040000A7 RID: 167
		private global::System.Windows.Forms.Label labMoveAcceleration;

		// Token: 0x040000A8 RID: 168
		private global::System.Windows.Forms.Label labMoveDeceleration;

		// Token: 0x040000A9 RID: 169
		private global::System.Windows.Forms.Label labGearRatio;

		// Token: 0x040000AA RID: 170
		private global::System.Windows.Forms.Label labEncoderGearRatio;

		// Token: 0x040000AB RID: 171
		private global::System.Windows.Forms.Button btnClose;

		// Token: 0x040000AC RID: 172
		private global::System.Windows.Forms.Timer tmrScan;

		// Token: 0x040000AD RID: 173
		private global::System.Windows.Forms.Button button22;

		// Token: 0x040000AE RID: 174
		private global::System.Windows.Forms.TextBox textBox1;

		// Token: 0x040000AF RID: 175
		private global::System.Windows.Forms.TextBox textBox2;

		// Token: 0x040000B0 RID: 176
		private global::System.Windows.Forms.Button button23;

		// Token: 0x040000B1 RID: 177
		private global::System.Windows.Forms.Label label26;

		// Token: 0x040000B2 RID: 178
		private global::System.Windows.Forms.Label label27;

		// Token: 0x040000B3 RID: 179
		private global::System.Windows.Forms.Label label28;

		// Token: 0x040000B4 RID: 180
		private global::System.Windows.Forms.Label labSpdRate;

		// Token: 0x040000B5 RID: 181
		private global::System.Windows.Forms.Label label23;

		// Token: 0x040000B6 RID: 182
		private global::System.Windows.Forms.Label label21;

		// Token: 0x040000B7 RID: 183
		private global::System.Windows.Forms.Button button7;

		// Token: 0x040000B8 RID: 184
		private global::System.Windows.Forms.TextBox TextSpeedRatio;

		// Token: 0x040000B9 RID: 185
		private global::System.Windows.Forms.Label label22;

		// Token: 0x040000BA RID: 186
		private global::System.Windows.Forms.TextBox TextDelay;

		// Token: 0x040000BB RID: 187
		private global::System.Windows.Forms.Button button10;

		// Token: 0x040000BC RID: 188
		private global::System.Windows.Forms.Button button12;

		// Token: 0x040000BD RID: 189
		private global::System.Windows.Forms.Panel plMotorButton;

		// Token: 0x040000BE RID: 190
		private global::System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;

		// Token: 0x040000BF RID: 191
		private global::System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;

		// Token: 0x040000C0 RID: 192
		private global::System.Windows.Forms.PictureBox pbALM;

		// Token: 0x040000C1 RID: 193
		private global::System.Windows.Forms.Label label24;

		// Token: 0x040000C2 RID: 194
		private global::System.Windows.Forms.TableLayoutPanel tableLayoutPanel11;

		// Token: 0x040000C3 RID: 195
		private global::System.Windows.Forms.PictureBox pbHLN;

		// Token: 0x040000C4 RID: 196
		private global::System.Windows.Forms.Label label36;

		// Token: 0x040000C5 RID: 197
		private global::System.Windows.Forms.TableLayoutPanel tableLayoutPanel10;

		// Token: 0x040000C6 RID: 198
		private global::System.Windows.Forms.PictureBox pbHLP;

		// Token: 0x040000C7 RID: 199
		private global::System.Windows.Forms.Label label35;

		// Token: 0x040000C8 RID: 200
		private global::System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;

		// Token: 0x040000C9 RID: 201
		private global::System.Windows.Forms.PictureBox pbEMG;

		// Token: 0x040000CA RID: 202
		private global::System.Windows.Forms.Label label25;

		// Token: 0x040000CB RID: 203
		private global::System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;

		// Token: 0x040000CC RID: 204
		private global::System.Windows.Forms.PictureBox pbSVON;

		// Token: 0x040000CD RID: 205
		private global::System.Windows.Forms.Label label30;

		// Token: 0x040000CE RID: 206
		private global::System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;

		// Token: 0x040000CF RID: 207
		private global::System.Windows.Forms.PictureBox pbSMV;

		// Token: 0x040000D0 RID: 208
		private global::System.Windows.Forms.Label label29;

		// Token: 0x040000D1 RID: 209
		private global::System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;

		// Token: 0x040000D2 RID: 210
		private global::System.Windows.Forms.PictureBox pbHMV;

		// Token: 0x040000D3 RID: 211
		private global::System.Windows.Forms.Label label31;

		// Token: 0x040000D4 RID: 212
		private global::System.Windows.Forms.TableLayoutPanel tableLayoutPanel7;

		// Token: 0x040000D5 RID: 213
		private global::System.Windows.Forms.PictureBox pbSLP;

		// Token: 0x040000D6 RID: 214
		private global::System.Windows.Forms.Label label32;

		// Token: 0x040000D7 RID: 215
		private global::System.Windows.Forms.TableLayoutPanel tableLayoutPanel8;

		// Token: 0x040000D8 RID: 216
		private global::System.Windows.Forms.PictureBox pbORG;

		// Token: 0x040000D9 RID: 217
		private global::System.Windows.Forms.Label label33;

		// Token: 0x040000DA RID: 218
		private global::System.Windows.Forms.TableLayoutPanel tableLayoutPanel9;

		// Token: 0x040000DB RID: 219
		private global::System.Windows.Forms.PictureBox pbSLN;

		// Token: 0x040000DC RID: 220
		private global::System.Windows.Forms.Label label34;

		// Token: 0x040000DD RID: 221
		private global::System.Windows.Forms.TextBox textCommandPosition;

		// Token: 0x040000DE RID: 222
		private global::System.Windows.Forms.TextBox textEncoderPosition;

		// Token: 0x040000DF RID: 223
		private global::System.Windows.Forms.Label label4;

		// Token: 0x040000E0 RID: 224
		private global::System.Windows.Forms.TextBox TextGap2;
	}
}
