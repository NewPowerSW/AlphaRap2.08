using NPSDK;
using NPSDK.Motion;
using NPSDK.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace NPSDK
{
	// Token: 0x02000014 RID: 20
	public partial class MotorControlForm : Form
	{
		// Token: 0x0600010D RID: 269 RVA: 0x000047F8 File Offset: 0x000029F8
		public MotorControlForm()
		{
			this.InitializeComponent();
			this.jogOption = new NP_JogOption();
			this.jogOption.Location = new Point(473, 194);
			this.jogOption.Size = new Size(123, 84);
			this.Controls.Add(this.jogOption);
		}

		/// <summary>从按钮所属窗体递归读取实际轴控件，包括未选中选项卡和动态组合控件中的轴。</summary>
		public void Initial(object Obj, double DefaultSpeed = 30.0, double MaximumJogSpeed = 100.0)
		{
			ResetMotors(MaximumJogSpeed);
			Control host = Obj as Control;
			Control root = host == null ? null : host.FindForm();
			if (root == null) root = host == null ? null : host.Parent;
			RegisterChildMotors(root, DefaultSpeed);
			this.tmrScan.Enabled = true;
		}

		/// <summary>遍历控件树，不创建轴、不扫描其他窗口，也不连接硬件。</summary>
		private void RegisterChildMotors(Control parent, double defaultSpeed)
		{
			if (parent == null || parent.IsDisposed || parent.Disposing) return;
			NP_Motor motor = parent as NP_Motor;
			if (motor != null) RegisterMotor(motor, defaultSpeed);
			foreach (Control child in parent.Controls) RegisterChildMotors(child, defaultSpeed);
		}

		/// <summary>仅显示指定轴，不展开为窗体内的其他轴。</summary>
		public void Initial(NP_Motor motor, double DefaultSpeed = 30.0, double MaximumJogSpeed = 100.0)
		{
			ResetMotors(MaximumJogSpeed);
			if (motor != null)
			{
				RegisterMotor(motor, DefaultSpeed);
			}
			this.tmrScan.Enabled = true;
		}

		/// <summary>清除上次的选择和轴按钮，供每次打开时重新读取实际控件树。</summary>
		private void ResetMotors(double maximumJogSpeed)
		{
			for (int i = 0; i < this.lstMotor.Count; i++)
			{
				this.lstMotor[i].ControlBtn.Dispose();
			}
			this.lstMotor.Clear();
			this.iWorkMotorIndex = -1;
			this.jogOption.WorkMotor = null;
			this.MaxJogSpeed = maximumJogSpeed;
		}

		/// <summary>为实际轴建立选择按钮，使用轴的当前显示名称。</summary>
		private void RegisterMotor(NP_Motor motor, double defaultSpeed)
		{
			if (motor.IsDisposed || motor.Disposing) return;
			Button button = new Button();
			button.Parent = this.plMotorButton;
			button.Font = new Font("Arial Narrow", 14f, FontStyle.Bold, GraphicsUnit.Point, 0);
			button.Dock = DockStyle.Left;
			button.Width = this.plMotorButton.Width / 4;
			button.Text = motor.Text;
			button.Tag = this.lstMotor.Count;
			button.Click += this.SwitchMotor_Click;
			MotorControlForm.MotorControlStruct item = default(MotorControlForm.MotorControlStruct);
			item.Motor = motor;
			item.ControlBtn = button;
			this.lstMotor.Add(item);
		}

		// Token: 0x0600010F RID: 271 RVA: 0x00004A7C File Offset: 0x00002C7C
		private void SwitchMotor_Click(object sender, EventArgs e)
		{
			bool flag = (int)Convert.ToInt16(((Button)sender).Tag) == this.iWorkMotorIndex;
			if (!flag)
			{
				this.StopRun();
				this.iWorkMotorIndex = (int)Convert.ToInt16(((Button)sender).Tag);
				this.jogOption.WorkMotor = this.lstMotor[this.iWorkMotorIndex].Motor;
				for (int i = 0; i < this.lstMotor.Count; i++)
				{
					bool flag2 = i == this.iWorkMotorIndex;
					if (flag2)
					{
						this.lstMotor[i].ControlBtn.BackColor = Color.FromArgb(0, 180, 230);
					}
					else
					{
						this.lstMotor[i].ControlBtn.BackColor = SystemColors.Control;
					}
				}
			}
		}

		// Token: 0x06000110 RID: 272 RVA: 0x00004B3B File Offset: 0x00002D3B
		private void btnClose_Click(object sender, EventArgs e)
		{
			this.tmrScan.Enabled = false;
			this.StopRun();
			base.Close();
		}

		// Token: 0x06000111 RID: 273 RVA: 0x00004B5C File Offset: 0x00002D5C
		private void tmrScan_Tick(object sender, EventArgs e)
		{
			this.tmrScan.Enabled = false;
			bool flag = this.iWorkMotorIndex >= 0;
			if (flag)
			{
				this.labMoveSpeed.Text = this.lstMotor[this.iWorkMotorIndex].Motor.WorkSpeed.ToString();
				this.labMoveAcceleration.Text = this.lstMotor[this.iWorkMotorIndex].Motor.Tacc.ToString();
				this.labMoveDeceleration.Text = this.lstMotor[this.iWorkMotorIndex].Motor.Tdec.ToString();
				this.labGearRatio.Text = this.lstMotor[this.iWorkMotorIndex].Motor.GearRatio.ToString();
				this.labEncoderGearRatio.Text = this.lstMotor[this.iWorkMotorIndex].Motor.EncGearRatio.ToString();
				this.labSpdRate.Text = this.lstMotor[this.iWorkMotorIndex].Motor.SpeedRatio.ToString();
				this.textCommandPosition.Text = this.lstMotor[this.iWorkMotorIndex].Motor.GetPos().ToString("F3");
				this.textEncoderPosition.Text = this.lstMotor[this.iWorkMotorIndex].Motor.GetEncPos().ToString("F3");
				NPSDK.MotionState MState =  this.lstMotor[this.iWorkMotorIndex].Motor.GetMotionState();
				NPSDK.AxisIOState IOState = this.lstMotor[this.iWorkMotorIndex].Motor.GetAxisIOState();
				this.pbALM.Image = (IOState.ALM ? NPSDK.Resource.ledon : NPSDK.Resource.ledoff);
				this.pbEMG.Image = (IOState.EMG ? NPSDK.Resource.ledon : NPSDK.Resource.ledoff);
				this.pbSVON.Image = (IOState.SVON ? NPSDK.Resource.ledon : NPSDK.Resource.ledoff);
				this.pbHMV.Image = (MState.HMV ? NPSDK.Resource.ledon : NPSDK.Resource.ledoff);
				this.pbSMV.Image = (MState.SMV ? NPSDK.Resource.ledon : NPSDK.Resource.ledoff);
				this.pbORG.Image = (IOState.ORG ? NPSDK.Resource.ledon : NPSDK.Resource.ledoff);
				this.pbHLN.Image = (IOState.MEL ? NPSDK.Resource.ledon : NPSDK.Resource.ledoff);
				this.pbHLP.Image = (IOState.PEL ? NPSDK.Resource.ledon : NPSDK.Resource.ledoff);
				this.pbSLN.Image = (IOState.SLN ? NPSDK.Resource.ledon : NPSDK.Resource.ledoff);
				this.pbSLP.Image = (IOState.SLP ? NPSDK.Resource.ledon : NPSDK.Resource.ledoff);
				bool flag2 = IOState.ALM || IOState.EMG;
				if (flag2)
				{
					this.StopRun();
				}
				bool flag3 = this.bRunGoto;
				if (flag3)
				{
					bool flag4 = this.lstMotor[this.iWorkMotorIndex].Motor.Goto(this.iRunGotoPos);
					if (flag4)
					{
						this.bRunGoto = false;
					}
				}
				else
				{
					bool flag5 = this.bRunHome;
					if (flag5)
					{
						bool flag6 = this.lstMotor[this.iWorkMotorIndex].Motor.Home();
						if (flag6)
						{
							this.bRunHome = false;
						}
					}
					else
					{
						bool flag7 = this.bRunLoop;
						if (flag7)
						{
							switch (this.iLoopTask)
							{
							case 1:
							{
								bool flag8 = this.lstMotor[this.iWorkMotorIndex].Motor.Goto(this.iRunPos1);
								if (flag8)
								{
									this.RunLoopTM.Restart();
									this.iLoopTask = 2;
								}
								break;
							}
							case 2:
							{
								bool flag9 = this.TextDelay.Text == "";
								if (flag9)
								{
									this.iLoopTask = 3;
								}
								else
								{
									bool flag10 = this.RunLoopTM.IsOn((long)Convert.ToInt16(this.TextDelay.Text));
									if (flag10)
									{
										this.iLoopTask = 3;
									}
								}
								break;
							}
							case 3:
							{
								bool flag11 = this.lstMotor[this.iWorkMotorIndex].Motor.Goto(this.iRunPos2);
								if (flag11)
								{
									this.RunLoopTM.Restart();
									this.iLoopTask = 4;
								}
								break;
							}
							case 4:
							{
								bool flag12 = this.TextDelay.Text == "";
								if (flag12)
								{
									this.iLoopTask = 1;
								}
								else
								{
									bool flag13 = this.RunLoopTM.IsOn((long)Convert.ToInt16(this.TextDelay.Text));
									if (flag13)
									{
										this.iLoopTask = 1;
									}
								}
								break;
							}
							}
						}
					}
				}
			}
			this.tmrScan.Enabled = true;
		}

		// Token: 0x06000112 RID: 274 RVA: 0x00005078 File Offset: 0x00003278
		private void button1_Click(object sender, EventArgs e)
		{
			bool flag = !this.GetBusy();
			if (flag)
			{
				bool flag2 = this.TextPosition1.Text != "";
				if (flag2)
				{
					this.iRunGotoPos = Convert.ToDouble(this.TextPosition1.Text);
					this.iRunGotoPos2 = Convert.ToDouble(this.TextPosition2.Text);
					this.bRunGoto = true;
				}
				else
				{
					this.TextPosition1.Focus();
				}
			}
		}

		// Token: 0x06000113 RID: 275 RVA: 0x000050F4 File Offset: 0x000032F4
		private void button2_Click(object sender, EventArgs e)
		{
			bool flag = !this.GetBusy();
			if (flag)
			{
				bool flag2 = this.TextPosition2.Text != "";
				if (flag2)
				{
					this.iRunGotoPos = Convert.ToDouble(this.TextPosition2.Text);
					this.bRunGoto = true;
				}
				else
				{
					this.TextPosition1.Focus();
				}
			}
		}

		// Token: 0x06000114 RID: 276 RVA: 0x00005159 File Offset: 0x00003359
		private void TextPosition1_DoubleClick(object sender, EventArgs e)
		{
			this.TextPosition1.Text = this.textCommandPosition.Text;
		}

		// Token: 0x06000115 RID: 277 RVA: 0x00005173 File Offset: 0x00003373
		private void TextPosition2_DoubleClick(object sender, EventArgs e)
		{
			this.TextPosition2.Text = this.textCommandPosition.Text;
		}

		// Token: 0x06000116 RID: 278 RVA: 0x00005190 File Offset: 0x00003390
		private void button3_Click(object sender, EventArgs e)
		{
			bool flag = this.TextGap1.Text != "" && this.TextPosition1.Text != "";
			if (flag)
			{
				this.TextPosition1.Text = (Convert.ToDouble(this.TextPosition1.Text) + Convert.ToDouble(this.TextGap1.Text)).ToString();
			}
		}

		// Token: 0x06000117 RID: 279 RVA: 0x00005208 File Offset: 0x00003408
		private void button4_Click(object sender, EventArgs e)
		{
			bool flag = this.TextGap2.Text != "" && this.TextPosition2.Text != "";
			if (flag)
			{
				this.TextPosition2.Text = (Convert.ToDouble(this.TextPosition2.Text) + Convert.ToDouble(this.TextGap2.Text)).ToString();
			}
		}

		// Token: 0x06000118 RID: 280 RVA: 0x00005280 File Offset: 0x00003480
		private void button6_Click(object sender, EventArgs e)
		{
			this.StopRun();
		}

		// Token: 0x06000119 RID: 281 RVA: 0x0000528C File Offset: 0x0000348C
		private void button19_Click(object sender, EventArgs e)
		{
			bool flag = this.iWorkMotorIndex >= 0 && !this.GetBusy();
			if (flag)
			{
				this.lstMotor[this.iWorkMotorIndex].Motor.HomeReset();
				this.bRunHome = true;
			}
		}

		// Token: 0x0600011A RID: 282 RVA: 0x000052D8 File Offset: 0x000034D8
		private void button9_Click(object sender, EventArgs e)
		{
			bool flag = this.iWorkMotorIndex >= 0;
			if (flag)
			{
				bool flag2 = this.TextSpeed.Text != "";
				if (flag2)
				{
					this.lstMotor[this.iWorkMotorIndex].Motor.WorkSpeed = Convert.ToDouble(this.TextSpeed.Text);
				}
				else
				{
					this.TextSpeed.Focus();
				}
			}
		}

		// Token: 0x0600011B RID: 283 RVA: 0x00005350 File Offset: 0x00003550
		private void button11_Click(object sender, EventArgs e)
		{
			bool flag = this.iWorkMotorIndex >= 0;
			if (flag)
			{
				bool flag2 = this.TextAcceleration.Text != "";
				if (flag2)
				{
					this.lstMotor[this.iWorkMotorIndex].Motor.Tacc = Convert.ToDouble(this.TextAcceleration.Text);
				}
				else
				{
					this.TextAcceleration.Focus();
				}
			}
		}

		// Token: 0x0600011C RID: 284 RVA: 0x000053C8 File Offset: 0x000035C8
		private void button15_Click(object sender, EventArgs e)
		{
			bool flag = this.iWorkMotorIndex >= 0;
			if (flag)
			{
				bool flag2 = this.TextDeceleration.Text != "";
				if (flag2)
				{
					this.lstMotor[this.iWorkMotorIndex].Motor.Tdec = Convert.ToDouble(this.TextDeceleration.Text);
				}
				else
				{
					this.TextDeceleration.Focus();
				}
			}
		}

		// Token: 0x0600011D RID: 285 RVA: 0x00005440 File Offset: 0x00003640
		private void button21_MouseDown(object sender, MouseEventArgs e)
		{
			bool flag = this.iWorkMotorIndex >= 0;
			if (flag)
			{
				double num = this.lstMotor[this.iWorkMotorIndex].Motor.WorkSpeed * this.lstMotor[this.iWorkMotorIndex].Motor.SpeedRatio / 100.0;
				bool flag2 = num > this.MaxJogSpeed;
				if (flag2)
				{
					MessageBox.Show("The motor speed is too fast, can not use jog mode. (Speed < " + this.MaxJogSpeed.ToString() + ")");
				}
				else
				{
					bool flag3 = !this.GetBusy();
					if (flag3)
					{
						this.jogOption.BeginMove(this.lstMotor[this.iWorkMotorIndex].Motor, true);
					}
				}
			}
		}

		// Token: 0x0600011E RID: 286 RVA: 0x00005504 File Offset: 0x00003704
		private void button21_MouseUp(object sender, MouseEventArgs e)
		{
			bool flag = this.iWorkMotorIndex >= 0;
			if (flag)
			{
				this.jogOption.EndMove(this.lstMotor[this.iWorkMotorIndex].Motor);
				this.bRunGoto = false;
			}
		}

		// Token: 0x0600011F RID: 287 RVA: 0x00005548 File Offset: 0x00003748
		private void button20_MouseDown(object sender, MouseEventArgs e)
		{
			bool flag = this.iWorkMotorIndex >= 0;
			if (flag)
			{
				double num = this.lstMotor[this.iWorkMotorIndex].Motor.WorkSpeed * this.lstMotor[this.iWorkMotorIndex].Motor.SpeedRatio / 100.0;
				bool flag2 = num > this.MaxJogSpeed;
				if (flag2)
				{
					MessageBox.Show("The motor speed is too fast, can not use jog mode. (Speed < " + this.MaxJogSpeed.ToString() + ")");
				}
				else
				{
					bool flag3 = !this.GetBusy();
					if (flag3)
					{
						this.jogOption.BeginMove(this.lstMotor[this.iWorkMotorIndex].Motor, false);
					}
				}
			}
		}

		// Token: 0x06000120 RID: 288 RVA: 0x0000560C File Offset: 0x0000380C
		private void button23_Click(object sender, EventArgs e)
		{
			bool flag = this.iWorkMotorIndex >= 0;
			if (flag)
			{
				bool flag2 = this.textBox2.Text != "";
				if (flag2)
				{
					this.lstMotor[this.iWorkMotorIndex].Motor.EncGearRatio = Convert.ToDouble(this.textBox2.Text);
				}
				else
				{
					this.textBox2.Focus();
				}
			}
		}

		// Token: 0x06000121 RID: 289 RVA: 0x00005684 File Offset: 0x00003884
		private void button22_Click(object sender, EventArgs e)
		{
			bool flag = this.iWorkMotorIndex >= 0;
			if (flag)
			{
				bool flag2 = this.textBox1.Text != "";
				if (flag2)
				{
					this.lstMotor[this.iWorkMotorIndex].Motor.GearRatio = Convert.ToDouble(this.textBox1.Text);
				}
				else
				{
					this.textBox1.Focus();
				}
			}
		}

		// Token: 0x06000122 RID: 290 RVA: 0x000056FC File Offset: 0x000038FC
		private void button5_Click(object sender, EventArgs e)
		{
			bool flag = !this.GetBusy() && this.TextPosition1.Text != "" && this.TextPosition2.Text != "";
			if (flag)
			{
				this.iLoopTask = 1;
				this.iRunPos1 = Convert.ToDouble(this.TextPosition1.Text);
				this.iRunPos2 = Convert.ToDouble(this.TextPosition2.Text);
				this.bRunLoop = true;
			}
		}

		// Token: 0x06000123 RID: 291 RVA: 0x00005784 File Offset: 0x00003984
		private void button7_Click(object sender, EventArgs e)
		{
			bool flag = this.iWorkMotorIndex >= 0;
			if (flag)
			{
				bool flag2 = this.TextSpeedRatio.Text != "";
				if (flag2)
				{
					this.lstMotor[this.iWorkMotorIndex].Motor.SpeedRatio = Convert.ToDouble(this.TextSpeedRatio.Text);
				}
				else
				{
					this.TextSpeedRatio.Focus();
				}
			}
		}

		// Token: 0x06000124 RID: 292 RVA: 0x000057FC File Offset: 0x000039FC
		private void TextPosition1_KeyPress(object sender, KeyPressEventArgs e)
		{
			bool flag = this.nonNumberEntered;
			if (!flag)
			{
				bool flag2 = e.KeyChar == '-' && (((TextBox)sender).SelectionStart != 0 || ((TextBox)sender).Text.IndexOf("-") >= 0);
				if (flag2)
				{
					e.Handled = true;
				}
				else
				{
					bool flag3 = e.KeyChar != '-' && e.KeyChar != '\b';
					if (flag3)
					{
						bool flag4 = e.KeyChar == '.' && ((TextBox)sender).Text.IndexOf(".") >= 0;
						if (flag4)
						{
							e.Handled = true;
						}
						bool flag5 = (e.KeyChar < '0' || e.KeyChar > '9') && e.KeyChar != '.';
						if (flag5)
						{
							e.Handled = true;
						}
					}
				}
			}
		}

		// Token: 0x06000125 RID: 293 RVA: 0x000058F0 File Offset: 0x00003AF0
		private void TextPosition1_KeyDown(object sender, KeyEventArgs e)
		{
			bool flag = e.KeyData == (Keys)131139 || e.KeyData == (Keys)131137 || e.KeyData == (Keys)131158 || e.KeyData == (Keys)131160;
			if (flag)
			{
				this.nonNumberEntered = true;
			}
			else
			{
				this.nonNumberEntered = false;
			}
		}

		// Token: 0x06000126 RID: 294 RVA: 0x00005950 File Offset: 0x00003B50
		private bool GetBusy()
		{
			bool moving = this.iWorkMotorIndex >= 0 && this.lstMotor[this.iWorkMotorIndex].Motor.GetBusy();
			return this.bRunGoto || this.bRunHome || this.bRunLoop || moving;
		}

		// Token: 0x06000127 RID: 295 RVA: 0x0000597C File Offset: 0x00003B7C
		private void StopRun()
		{
			this.bRunGoto = false;
			this.bRunHome = false;
			this.bRunLoop = false;
			bool flag = this.iWorkMotorIndex >= 0;
			if (flag)
			{
				this.lstMotor[this.iWorkMotorIndex].Motor.Stop();
			}
		}

		// Token: 0x06000128 RID: 296 RVA: 0x000059D0 File Offset: 0x00003BD0
		private void button10_Click(object sender, EventArgs e)
		{
			bool flag = this.iWorkMotorIndex >= 0;
			if (flag)
			{
				this.lstMotor[this.iWorkMotorIndex].Motor.ServoOn();
			}
		}

		// Token: 0x06000129 RID: 297 RVA: 0x00005A0C File Offset: 0x00003C0C
		private void button12_Click(object sender, EventArgs e)
		{
			bool flag = this.iWorkMotorIndex >= 0;
			if (flag)
			{
				this.lstMotor[this.iWorkMotorIndex].Motor.ServoOff();
			}
		}

		// Token: 0x0600012A RID: 298 RVA: 0x00005A48 File Offset: 0x00003C48
		private void MotorControlForm_Shown(object sender, EventArgs e)
		{
			bool flag = this.lstMotor.Count > 0;
			if (flag)
			{
				this.lstMotor[this.lstMotor.Count - 1].ControlBtn.PerformClick();
			}
		}

		// Token: 0x0400007D RID: 125
		private List<MotorControlForm.MotorControlStruct> lstMotor = new List<MotorControlForm.MotorControlStruct>();

		// Token: 0x0400007E RID: 126
		private int iWorkMotorIndex = -1;

		// Token: 0x0400007F RID: 127
		private double MaxJogSpeed = 0.0;

		// Token: 0x04000080 RID: 128
		private bool bRunGoto = false;

		// Token: 0x04000081 RID: 129
		private double iRunGotoPos = 0.0;

		// Token: 0x04000082 RID: 130
		private double iRunGotoPos2 = 0.0;

		// Token: 0x04000083 RID: 131
		private bool bRunLoop = false;

		// Token: 0x04000084 RID: 132
		private double iRunPos1 = 0.0;

		// Token: 0x04000085 RID: 133
		private double iRunPos2 = 0.0;

		// Token: 0x04000086 RID: 134
		private int iLoopTask;

		// Token: 0x04000087 RID: 135
		private JTimer RunLoopTM = new JTimer();

		// Token: 0x04000088 RID: 136
		private bool bRunHome = false;

		// Token: 0x04000089 RID: 137
		private bool nonNumberEntered = false;

		private NP_JogOption jogOption;

		// Token: 0x02000083 RID: 131
		private struct MotorControlStruct
		{
			// Token: 0x040008BE RID: 2238
			public NP_Motor Motor;

			// Token: 0x040008BF RID: 2239
			public Button ControlBtn;
		}
	}
}
