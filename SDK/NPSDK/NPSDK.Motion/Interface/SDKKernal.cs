namespace NPSDK
{
	public class SDKKernal
	{
		public static bool isSimulation { get; set; }

		/// <summary>兼容旧调用；实际走 NSDK.InitialControls（含参数读取）。</summary>
		public static void InitialControls()
		{
			NSDK.InitialControls();
		}

		public static void InitializeComponent()
		{
			NSDK.InitialControls();
		}

		/// <summary>释放当前登记快照中的硬件驱动。</summary>
		public static void DisposeComponent()
		{
			foreach (ControlBaseInterface control in SDKPara.ControlsSnapshot())
			{
				control.DisposeObj();
			}
		}

		/// <summary>逐个刷新有效控件，与动态释放互斥。</summary>
		public static void RefreshControls()
		{
			foreach (ControlBaseInterface control in SDKPara.ControlsSnapshot())
			{
				lock (control) { if (!control.IsDisposed) control.RefreshControl(); }
			}
		}

		public static void RefreshIO()
		{
			RefreshControls();
		}

		/// <summary>对当前硬件控件应用模拟标记。</summary>
		public static void SetSimulation(bool IsSimu)
		{
			isSimulation = IsSimu;
			foreach (ControlBaseInterface control in SDKPara.ControlsSnapshot())
			{
				control.SetSimulation(IsSimu);
			}
		}

		/// <summary>备份当前登记轴的速度及加减速参数。</summary>
		public static void BackupMotorSPD()
		{
			SDKPara.MotorSPDList.Clear();
			foreach (ControlBaseInterface control in SDKPara.ControlsSnapshot())
			{
				Motion.NP_Motor motor = control as Motion.NP_Motor;
				if (motor == null)
				{
					continue;
				}
				MotorSPDPara item = default(MotorSPDPara);
				item.Port = motor.Port;
				item.Spd = motor.WorkSpeed;
				item.SpdRate = motor.SpeedRatio;
				item.Acc = motor.Tacc;
				item.Dec = motor.Tdec;
				SDKPara.MotorSPDList.Add(item);
			}
		}

		/// <summary>按端口恢复已备份的轴速度及加减速参数。</summary>
		public static void RestoreMotorSPD()
		{
			foreach (ControlBaseInterface control in SDKPara.ControlsSnapshot())
			{
				Motion.NP_Motor motor = control as Motion.NP_Motor;
				if (motor == null)
				{
					continue;
				}
				foreach (MotorSPDPara motorSPD in SDKPara.MotorSPDList)
				{
					if (motor.Port == motorSPD.Port)
					{
						motor.WorkSpeed = motorSPD.Spd;
						motor.SpeedRatio = motorSPD.SpdRate;
						motor.Tacc = motorSPD.Acc;
						motor.Tdec = motorSPD.Dec;
					}
				}
			}
		}

		/// <summary>备份当前登记轴的最大速度。</summary>
		public static void BackupMotorMaxSPD()
		{
			SDKPara.MotorMaxSPDList.Clear();
			foreach (ControlBaseInterface control in SDKPara.ControlsSnapshot())
			{
				Motion.NP_Motor motor = control as Motion.NP_Motor;
				if (motor == null)
				{
					continue;
				}
				MotorMaxSPDPara item = default(MotorMaxSPDPara);
				item.Port = motor.Port;
				item.MaxSpd = motor.MaxSpeed;
				SDKPara.MotorMaxSPDList.Add(item);
			}
		}

		/// <summary>按端口恢复已备份的轴最大速度。</summary>
		public static void RestoreMotorMaxSPD()
		{
			foreach (ControlBaseInterface control in SDKPara.ControlsSnapshot())
			{
				Motion.NP_Motor motor = control as Motion.NP_Motor;
				if (motor == null)
				{
					continue;
				}
				foreach (MotorMaxSPDPara motorMaxSPD in SDKPara.MotorMaxSPDList)
				{
					if (motor.Port == motorMaxSPD.Port)
					{
						motor.MaxSpeed = motorMaxSPD.MaxSpd;
					}
				}
			}
		}
	}
}
