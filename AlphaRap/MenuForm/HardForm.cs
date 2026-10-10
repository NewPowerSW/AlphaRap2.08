using System;
using System.Windows.Forms;
using System.IO;
using AlphaRapLibrary;
using Alpha;
using Alpha._0;
using NPSDK;

namespace AlphaRap
{
	public enum HeadNum
	{
		H1,
		H2
	}
	public struct PointStruct
	{
		public Double x_Point;
		public Double y_Point;
		public Double z_Point;
		public Double u_Point;

		public bool Disable;
		public bool EnableVision;
		public string Point;
		public string Annotation;
	}
	public partial class HardForm : ModuleBaseForm
	{
		public int iRunPosCount = 0;
		public Servo servo;
		public string CalibPositionPath = string.Format(@"{0}\{1}\{2}\{3}\{4}", Application.StartupPath, "ModuleData", "HardForm", SysPara.RecipeName, "CalibPosition.xml");
		public string MotorPositionPath = string.Format(@"{0}\{1}\{2}\{3}\{4}", Application.StartupPath, "ModuleData", "HardForm", SysPara.RecipeName, "MotorPosition.xml");

		public HardForm()
		{
			InitializeComponent();

			servo = new Servo(MTR_H1_X, MTR_H1_Y, MTR_H1_Z, MTR_H1_R1, MTR_H1_R2, MTR_H1_R3);

			dgv_CalibPos.DgvHeader = new string[] { "Number", "X", "Y", "Z", "R1", "R2", "Annotation" }; ;
			dgv_CalibPos.myDgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
			dgv_CalibPos.RaiseSelectedEvent += dgv_CalibPos_RaiseSelectedEvent;

			dgv_MotorPos.DgvHeader = new string[] { "Number", "X", "Y", "Z", "R1", "R2", "Annotation" }; ;
			dgv_MotorPos.myDgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
			dgv_MotorPos.RaiseSelectedEvent += dgv_MotorPos_RaiseSelectedEvent;
		}
		private void dgv_CalibPos_RaiseSelectedEvent(bool bComplete)
		{
			int index = dgv_CalibPos.myDgv.CurrentRow.Index;

			string[] selectedRowArr = dgv_CalibPos.GetDataArrayByIndex(index);                  //The value returning the current row through the index returns an array

			DataGridViewRow dataGirdRow = dgv_CalibPos.GetDataGridRow(index);
		}
		private void dgv_MotorPos_RaiseSelectedEvent(bool bComplete)
		{
			int index = dgv_MotorPos.myDgv.CurrentRow.Index;

			string[] selectedRowArr = dgv_MotorPos.GetDataArrayByIndex(index);                  //The value returning the current row through the index returns an array

			DataGridViewRow dataGirdRow = dgv_MotorPos.GetDataGridRow(index);
		}
		public override void ServoOn()
		{
			servo.ServoAllOn();
			OperationLog.Write("HardForm", "伺服使能：全部轴");
		}
		public override void ServoOff()
		{
			servo.ServoAllOff();
			OperationLog.Write("HardForm", "伺服禁用：全部轴");
		}

		// ==================== 操作日志：点动（松开时记一条） ====================

		private bool _jogLogWired;
		private bool _jogActive;
		private DateTime _jogStart;
		private string _jogDesc;

		/// <summary>建窗体时挂一次：点动按钮按下开始计时、松开记一条（轴 + 方向 + 持续时间）。</summary>
		public override void ModuleInitialize(string ModuleName)
		{
			base.ModuleInitialize(ModuleName);
			WireJogLog();
		}

		private void WireJogLog()
		{
			if (_jogLogWired) return;
			_jogLogWired = true;

			HookJog(btn_AxisX_Left, "X 负向");
			HookJog(btn_AxisX_Right, "X 正向");
			HookJog(btn_AxisY_Back, "Y 负向");
			HookJog(btn_AxisY_Forward, "Y 正向");
			HookJog(btn_AxisZ_Up, "Z 负向");
			HookJog(btn_AxisZ_Down, "Z 正向");
			HookJog(btn_AxisR_Up, "R 负向");
			HookJog(btn_AxisR_Down, "R 正向");
		}

		private void HookJog(Button btn, string desc)
		{
			if (btn == null) return;
			btn.MouseDown += delegate { _jogDesc = desc; _jogStart = DateTime.Now; _jogActive = true; };
			btn.MouseUp += delegate
			{
				if (!_jogActive) return;
				_jogActive = false;
				double sec = (DateTime.Now - _jogStart).TotalSeconds;
				OperationLog.Write("HardForm", "点动：" + _jogDesc + "，持续 " + sec.ToString("F1") + " 秒");
			};
		}

		private void But_XYZ_Click_1(object sender, EventArgs e)
		{
			MotorControlForm MCF = new MotorControlForm();
			MCF.setMotor(MotorControlForm.AxisType.X, MTR_H1_X, MotorControlForm.MoveType.RIGHT, MotorControlForm.MoveType.LEFT, null, null);
			MCF.setMotor(MotorControlForm.AxisType.Y, MTR_H1_Y, MotorControlForm.MoveType.FRONT, MotorControlForm.MoveType.BACK, null, null);
			MCF.setMotor(MotorControlForm.AxisType.Z, MTR_H1_Z, MotorControlForm.MoveType.UP, MotorControlForm.MoveType.DOWN, null, null);
			MCF.setMotor(MotorControlForm.AxisType.R1, MTR_H1_R1, MotorControlForm.MoveType.TLEFT, MotorControlForm.MoveType.TRIGHT, null, null);
			MCF.setMotor(MotorControlForm.AxisType.R2, MTR_H1_R2, MotorControlForm.MoveType.TLEFT, MotorControlForm.MoveType.TRIGHT, null, null);
			MCF.setMotor(MotorControlForm.AxisType.R3, MTR_H1_R3, MotorControlForm.MoveType.TLEFT, MotorControlForm.MoveType.TRIGHT, null, null);
			MCF.Initial();
			MCF.ShowDialog();
		}
		public  void StopAllMotor()
		{
			servo.manualReset.Reset();
			foreach (ControlBaseInterface control in SDKPara.ControlList)
				if (control is Adlink_Motor)
				{
					((Adlink_Motor)control).Stop();
				}
			servo.manualReset.Set();
		}
		private void bt_GotoCalib_Click_1(object sender, EventArgs e)
		{
			string tabName = tab_Pos.SelectedTab.Name;
			double posX = 0, posY = 0, posZ = 0, posR1 = 0, posR2 = 0;
			int index = -1;
			string[] dgvRowDataArr;
			switch (tabName)
			{
				case "tab_CalibPos":
					index = dgv_CalibPos.myDgv.CurrentRow.Index;
					dgvRowDataArr = dgv_CalibPos.GetDataArrayByIndex(index);
					if (dgvRowDataArr == null || dgvRowDataArr.Length < 5) return;
					posX = double.Parse(dgvRowDataArr[1]);
					posY = double.Parse(dgvRowDataArr[2]);
					posZ = double.Parse(dgvRowDataArr[3]);
					posR1 = double.Parse(dgvRowDataArr[4]);
					posR2 = double.Parse(dgvRowDataArr[5]);
					break;
				case "tab_FlowPos":
					index = dgv_MotorPos.myDgv.CurrentRow.Index;
					dgvRowDataArr = dgv_MotorPos.GetDataArrayByIndex(index);
					if (dgvRowDataArr == null || dgvRowDataArr.Length < 5) return;
					posX = double.Parse(dgvRowDataArr[1]);
					posY = double.Parse(dgvRowDataArr[2]);
					posZ = double.Parse(dgvRowDataArr[3]);
					posR1 = double.Parse(dgvRowDataArr[4]);
					posR2 = double.Parse(dgvRowDataArr[5]);
					break;
			}
			servo.GotoAxis(ServoAixsName.X, posX);
			servo.GotoAxis(ServoAixsName.Y, posY);
			servo.GotoAxis(ServoAixsName.Z, posZ);
			servo.GotoAxis(ServoAixsName.R1, posR1);
			servo.GotoAxis(ServoAixsName.R2, posR2);
		}

		private void btn_WriteCurrentPos_Click(object sender, EventArgs e)
		{
			string tabName = tab_Pos.SelectedTab.Name;
			OperationLog.Write("HardForm", "写入当前位置到表格："
				+ (tabName == "tab_CalibPos" ? "标定点" : "流程点") + " ("
				+ lbl_PosX.Text + ", " + lbl_PosY.Text + ", " + lbl_PosZ.Text + ", "
				+ lbl_PosR1.Text + ", " + lbl_PosR2.Text + ")");
			string[] posArr = new string[] { "" };
			switch (tabName)
			{
				case "tab_CalibPos":
					posArr = new string[] { lbl_PosX.Text, lbl_PosY.Text, lbl_PosZ.Text, lbl_PosR1.Text, lbl_PosR2.Text, "ReadMe", };

					dgv_CalibPos.WriteRowToDataGrid(posArr);

                    break;
				case "tab_FlowPos":
					posArr = new string[] { lbl_PosX.Text, lbl_PosY.Text, lbl_PosZ.Text, lbl_PosR1.Text, lbl_PosR2.Text, "ReadMe", };

					dgv_MotorPos.WriteRowToDataGrid(posArr);
					break;
			}
		}

		private void btn_UpdateSelectedPos_Click(object sender, EventArgs e)
		{
			string tabName = tab_Pos.SelectedTab.Name;
			OperationLog.Write("HardForm", "用当前位置更新选中行："
				+ (tabName == "tab_CalibPos" ? "标定点" : "流程点") + " ("
				+ lbl_PosX.Text + ", " + lbl_PosY.Text + ", " + lbl_PosZ.Text + ", "
				+ lbl_PosR1.Text + ", " + lbl_PosR2.Text + ")");
			int index = -1;
			string[] upDateArr = new string[] { lbl_PosX.Text, lbl_PosY.Text, lbl_PosZ.Text, lbl_PosR1.Text, lbl_PosR2.Text, "ReadMe" };
			int endindex;

			try
			{
				switch (tabName)
				{
					case "tab_CalibPos":
						upDateArr = new string[] { lbl_PosX.Text, lbl_PosY.Text, lbl_PosZ.Text, lbl_PosR1.Text, lbl_PosR2.Text };
						index = dgv_CalibPos.myDgv.CurrentRow.Index;
						endindex = upDateArr.Length;
						dgv_CalibPos.UpdateGridViewRow(index, 1, endindex, upDateArr);
						break;
					case "tab_FlowPos":
						upDateArr = new string[] { lbl_PosX.Text, lbl_PosY.Text, lbl_PosZ.Text, lbl_PosR1.Text, lbl_PosR2.Text };
						index = dgv_MotorPos.myDgv.CurrentRow.Index;
						endindex = upDateArr.Length;
						dgv_MotorPos.UpdateGridViewRow(index, 1, endindex, upDateArr);
						break;
				}
			}
			catch (Exception)
			{
			}
		}

		private void bt_Stop_Click(object sender, EventArgs e)
		{
			servo.manualReset.Reset();
			StopAllMotor();
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			lbl_PosX.Text = servo.ServoX.GetPos().ToString("F3");
			lbl_PosY.Text = servo.ServoY.GetPos().ToString("F3");
			lbl_PosZ.Text = servo.ServoZ.GetPos().ToString("F3");
			lbl_PosR1.Text = servo.ServoR1.GetPos().ToString("F3");
			lbl_PosR2.Text = servo.ServoR2.GetPos().ToString("F3");
		}

		private void trackBar_Speed_ValueChanged(object sender, EventArgs e)
		{
			textBox_SpeedRatio.Text = trackBar_Speed.Value.ToString();
			servo.ServoX.SpeedRatio = trackBar_Speed.Value;
			servo.ServoY.SpeedRatio = trackBar_Speed.Value;
			servo.ServoZ.SpeedRatio = trackBar_Speed.Value;
		}

		private void btn_AxisY_Back_MouseDown(object sender, MouseEventArgs e)
		{
			servo.ServoY.WorkSpeed = 50;
			servo.ServoY.SpeedRatio = 50;
			servo.ServoY.JogP();
		}

		private void btn_AxisY_Back_MouseUp(object sender, MouseEventArgs e)
		{
			servo.ServoY.Stop();
		}

		private void btn_AxisY_Forward_MouseDown(object sender, MouseEventArgs e)
		{
			servo.ServoY.WorkSpeed = 50;
			servo.ServoY.SpeedRatio = 50;
			servo.ServoY.JogN();
		}

		private void btn_AxisY_Forward_MouseUp(object sender, MouseEventArgs e)
		{
			servo.ServoY.Stop();
		}

		private void btn_AxisX_Left_MouseDown(object sender, MouseEventArgs e)
		{
			servo.ServoX.WorkSpeed = 50;
			servo.ServoX.SpeedRatio = 50;
			servo.ServoX.JogP();
		}

		private void btn_AxisX_Left_MouseUp(object sender, MouseEventArgs e)
		{
			servo.ServoX.Stop();
		}

		private void btn_AxisX_Right_MouseDown(object sender, MouseEventArgs e)
		{
			servo.ServoX.WorkSpeed = 50;
			servo.ServoX.SpeedRatio = 50;
			servo.ServoX.JogN();
		}

		private void btn_AxisX_Right_MouseUp(object sender, MouseEventArgs e)
		{
			servo.ServoX.Stop();
		}

		private void btn_HomeX_Click(object sender, EventArgs e)
		{
			string btnName = ((Button)sender).Name;
			string flagStr = "btn_Home";
			string axisName = btnName.Replace(flagStr, "");
			OperationLog.Write("HardForm", "回零：轴 " + axisName);
			double HighSpeed = GetSettingValue("MSet", "AxisZ_HomeHighSpd");
			double LowSpeed = GetSettingValue("MSet", "AxisZ_HomeLowSpd");
			int timeOut = 50000;
			switch (axisName)
			{
				case "X":
					servo.HomeAxis(ServoAixsName.X, HighSpeed, LowSpeed, timeOut);
					break;

				case "Y":
					servo.HomeAxis(ServoAixsName.Y, HighSpeed, LowSpeed, timeOut);
					break;
				case "Z":
					servo.HomeAxis(ServoAixsName.Z, HighSpeed, LowSpeed, timeOut);
					break;
				case "R1":
					servo.HomeAxis(ServoAixsName.R1, HighSpeed, LowSpeed, timeOut);
					break;
				case "R2":
					servo.HomeAxis(ServoAixsName.R2, HighSpeed, LowSpeed, timeOut);
					break;
			}
		}

		private void btn_HomeY_Click(object sender, EventArgs e)
		{
			string btnName = ((Button)sender).Name;
			string flagStr = "btn_Home";
			string axisName = btnName.Replace(flagStr, "");
			OperationLog.Write("HardForm", "回零：轴 " + axisName);
			double HighSpeed = GetSettingValue("MSet", "AxisZ_HomeHighSpd");
			double LowSpeed = GetSettingValue("MSet", "AxisZ_HomeLowSpd");
			int timeOut = 50000;
			switch (axisName)
			{
				case "X":
					servo.HomeAxis(ServoAixsName.X, HighSpeed, LowSpeed, timeOut);
					break;

				case "Y":
					servo.HomeAxis(ServoAixsName.Y, HighSpeed, LowSpeed, timeOut);
					break;
				case "Z":
					servo.HomeAxis(ServoAixsName.Z, HighSpeed, LowSpeed, timeOut);
					break;
				case "R1":
					servo.HomeAxis(ServoAixsName.R1, HighSpeed, LowSpeed, timeOut);
					break;
				case "R2":
					servo.HomeAxis(ServoAixsName.R2, HighSpeed, LowSpeed, timeOut);
					break;
			}
		}

		private void btn_HomeZ_Click(object sender, EventArgs e)
		{
			string btnName = ((Button)sender).Name;
			string flagStr = "btn_Home";
			string axisName = btnName.Replace(flagStr, "");
			OperationLog.Write("HardForm", "回零：轴 " + axisName);
			double HighSpeed = GetSettingValue("MSet", "AxisZ_HomeHighSpd");
			double LowSpeed = GetSettingValue("MSet", "AxisZ_HomeLowSpd");
			int timeOut = 50000;
			switch (axisName)
			{
				case "X":
					servo.HomeAxis(ServoAixsName.X, HighSpeed, LowSpeed, timeOut);
					break;

				case "Y":
					servo.HomeAxis(ServoAixsName.Y, HighSpeed, LowSpeed, timeOut);
					break;
				case "Z":
					servo.HomeAxis(ServoAixsName.Z, HighSpeed, LowSpeed, timeOut);
					break;
				case "R1":
					servo.HomeAxis(ServoAixsName.R1, HighSpeed, LowSpeed, timeOut);
					break;
				case "R2":
					servo.HomeAxis(ServoAixsName.R2, HighSpeed, LowSpeed, timeOut);
					break;
			}
		}

		private void btn_HomeR_Click(object sender, EventArgs e)
		{
			string btnName = ((Button)sender).Name;
			string flagStr = "btn_Home";
			string axisName = btnName.Replace(flagStr, "");
			OperationLog.Write("HardForm", "回零：轴 " + axisName);
			double HighSpeed = GetSettingValue("MSet", "AxisZ_HomeHighSpd");
			double LowSpeed = GetSettingValue("MSet", "AxisZ_HomeLowSpd");
			int timeOut = 50000;
			switch (axisName)
			{
				case "X":
					servo.HomeAxis(ServoAixsName.X, HighSpeed, LowSpeed, timeOut);
					break;

				case "Y":
					servo.HomeAxis(ServoAixsName.Y, HighSpeed, LowSpeed, timeOut);
					break;
				case "Z":
					servo.HomeAxis(ServoAixsName.Z, HighSpeed, LowSpeed, timeOut);
					break;
				case "R1":
					servo.HomeAxis(ServoAixsName.R1, HighSpeed, LowSpeed, timeOut);
					break;
				case "R2":
					servo.HomeAxis(ServoAixsName.R2, HighSpeed, LowSpeed, timeOut);
					break;
			}
		}

		private void btn_AxisZ_Up_MouseDown(object sender, MouseEventArgs e)
		{
			servo.ServoZ.WorkSpeed = 50;
			servo.ServoZ.SpeedRatio = 50;
			servo.ServoZ.JogN();
		}

		private void btn_AxisZ_Down_MouseDown(object sender, MouseEventArgs e)
		{
			servo.ServoZ.WorkSpeed = 50;
			servo.ServoZ.SpeedRatio = 50;
			servo.ServoZ.JogP();
		}

		private void btn_AxisZ_Up_MouseUp(object sender, MouseEventArgs e)
		{
			servo.ServoZ.Stop();
		}

		private void btn_AxisZ_Down_MouseUp(object sender, MouseEventArgs e)
		{
			servo.ServoZ.Stop();
		}

		private void btn_AxisR_Up_MouseDown(object sender, MouseEventArgs e)
		{
			servo.ServoR1.WorkSpeed = 50;
			servo.ServoR1.SpeedRatio = 50;
			servo.ServoR1.JogN();
		}

		private void btn_AxisR_Down_MouseDown(object sender, MouseEventArgs e)
		{
			servo.ServoR1.WorkSpeed = 50;
			servo.ServoR1.SpeedRatio = 50;
			servo.ServoR1.JogP();
		}

		private void btn_AxisR_Up_MouseUp(object sender, MouseEventArgs e)
		{
			servo.ServoR1.Stop();
		}

		private void btn_AxisR_Down_MouseUp(object sender, MouseEventArgs e)
		{
			servo.ServoR1.Stop();
		}
		public void SaveHardData()
		{
			#region
			FileInfo fi = new FileInfo(MiddleLayer.HardF.CalibPositionPath);
			if (!fi.Directory.Exists)   //如果文件不存在则创建文件
			{
				fi.Directory.Create();
			}
			fi = new FileInfo(MiddleLayer.HardF.MotorPositionPath);
			if (!fi.Directory.Exists)   //如果文件不存在则创建文件
			{
				fi.Directory.Create();
			}
			MiddleLayer.HardF.dgv_CalibPos.SaveXmlFileFromPath(MiddleLayer.HardF.CalibPositionPath);
			MiddleLayer.HardF.dgv_MotorPos.SaveXmlFileFromPath(MiddleLayer.HardF.MotorPositionPath);
			#endregion
		}

		private void btn_MoveUp_Click(object sender, EventArgs e)
		{
			string tabName = tab_Pos.SelectedTab.Name;
			switch (tabName)
			{
				case "tab_CalibPos":
					dgv_CalibPos.tsm_MoveUp_Click(null, null);
					break;

				case "tab_FlowPos":
					dgv_MotorPos.tsm_MoveUp_Click(null, null);
					break;
			}
		}

		private void btn_MoveDown_Click(object sender, EventArgs e)
		{
			string tabName = tab_Pos.SelectedTab.Name;
			switch (tabName)
			{
				#region【Reading code position => (读码位置)】
				case "tab_CalibPos":
					dgv_CalibPos.tsm_MoveDown_Click(null, null);
					break;
				#endregion

				#region 【Grab position => (拍照位置)】
				case "tab_FlowPos":
					dgv_MotorPos.tsm_MoveDown_Click(null, null);
					break;
					#endregion
			}
		}

		public double[] GetAxisValue(string pointName)
		{
			dgv_MotorPos.LoadXmlFileFromPath(MotorPositionPath);
			double posX = 0, posY = 0, posZ = 0, posR1 = 0, posR2 = 0;
			double[] AxisValue = new double[] { posX, posY, posZ, posR1, posR2 };
			string[] dgvRowDataArr;
			dgvRowDataArr = dgv_MotorPos.GetDataArrayByFlagName(0, pointName);
			if (dgvRowDataArr == null || dgvRowDataArr.Length < 5) return AxisValue;
			posX = double.Parse(dgvRowDataArr[1]);
			posY = double.Parse(dgvRowDataArr[2]);
			posZ = double.Parse(dgvRowDataArr[3]);
			posR1 = double.Parse(dgvRowDataArr[4]);
			posR2 = double.Parse(dgvRowDataArr[5]);
			AxisValue = new double[] { posX, posY, posZ, posR1, posR2 };
			return AxisValue;
		}
		public bool GotoAxisPoint(double[] AxisPoint)
		{
			servo.GotoAxis(ServoAixsName.X, AxisPoint[0]);
			servo.GotoAxis(ServoAixsName.Y, AxisPoint[1]);
			servo.GotoAxis(ServoAixsName.Z, AxisPoint[2]);
			servo.GotoAxis(ServoAixsName.R1, AxisPoint[3]);
			servo.GotoAxis(ServoAixsName.R2, AxisPoint[4]);
			double num0 = Math.Abs(servo.GetCurrentPos(ServoAixsName.X) - AxisPoint[0]);
			double num1 = Math.Abs(servo.GetCurrentPos(ServoAixsName.Y) - AxisPoint[1]);
			double num2 = Math.Abs(servo.GetCurrentPos(ServoAixsName.Z) - AxisPoint[2]);
			double num3 = Math.Abs(servo.GetCurrentPos(ServoAixsName.R1) - AxisPoint[3]);
			double num4 = Math.Abs(servo.GetCurrentPos(ServoAixsName.R2) - AxisPoint[4]);
			if (num0 < 0.01 & num1 < 0.01 & num2 < 0.01 & num3 < 0.01 & num4 < 0.01)
			{
				return true;
			}
			return false;
		}
		public bool GotoAxisPoint_Z(double[] AxisPoint)
		{
			servo.GotoAxis(ServoAixsName.Z, AxisPoint[2]);
			double num2 = Math.Abs(servo.GetCurrentPos(ServoAixsName.Z) - AxisPoint[2]);
			if (num2 < 0.01)
			{
				return true;
			}
			return false;
		}
		public bool HomeAxis(string axisName)
		{
			bool status = false;
			double HighSpeed = GetSettingValue("MSet", "AxisZ_HomeHighSpd");
			double LowSpeed = GetSettingValue("MSet", "AxisZ_HomeLowSpd");
			int timeOut = 50000;
			switch (axisName)
			{
				case "X":
					servo.HomeAxis(ServoAixsName.X, HighSpeed, LowSpeed, timeOut);
					status = servo.Servo_X_IsHomeOK;
					return status;

				case "Y":
					servo.HomeAxis(ServoAixsName.Y, HighSpeed, LowSpeed, timeOut);
					status = servo.Servo_Y_IsHomeOK;
					return status;

				case "Z":
					servo.HomeAxis(ServoAixsName.Z, HighSpeed, LowSpeed, timeOut);
					status = servo.Servo_Z_IsHomeOK;
					return status;

				case "R1":
					servo.HomeAxis(ServoAixsName.R1, HighSpeed, LowSpeed, timeOut);
					status = servo.Servo_R1_IsHomeOK;
					return status;

				case "R2":
					servo.HomeAxis(ServoAixsName.R2, HighSpeed, LowSpeed, timeOut);
					status = servo.Servo_R2_IsHomeOK;
					return status;
			}
			return status;
		}
	}
}
