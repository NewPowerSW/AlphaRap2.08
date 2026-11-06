using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using NPSDK;

namespace NPSDK.Motion
{
	/// <summary>
	/// 轴控件（独立）。硬件由 Vendor 对应的 *_Motor 实现。
	/// 点位表 / 点动 / 回零等其它控件通过 WorkMotor 绑定到本控件。
	/// </summary>
	public partial class NP_Motor : ControlBaseInterface
	{
		private MotorBaseInterface dMotor;
		private bool ready;
        internal event EventHandler StopRequested;
        /// <summary>Whether the axis has an initialized driver.</summary>
        public bool IsReady { get { return ready && dMotor != null; } }
		private double workSpeed = 10;
		private double speedRatio = 100;
		private double tacc = 0.1;
		private double tdec = 0.1;
		private double maxSpeed = 1000;
		private double manualSpeed = 10;
		private double pulseEquivalent = 1000;
		private double encoderPulseEquivalent = 1000;
		private int homeMode = 28;
		private bool homeDir;
		private double homeHighSpeed = 20;
		private double homeLowSpeed = 5;
		private double homeOffset;

		[Browsable(true)]
		[Category("#NP")]
		[TypeConverter(typeof(MotionVendorConverter))]
		[Description("仅板卡厂商（Keyence / Omron 无轴，不在此列出）")]
		public NPVendor Vendor { get; set; } = NPVendor.Leadshine;

		[Browsable(true)]
		[Category("#NP")]
		[Description("轴号，例如 0 或 011")]
		public string Port { get; set; } = "0";

		public AxisIOState IOState { get; private set; }

		[Browsable(true)]
		[Category("#NP")]
		[DefaultValue(10.0)]
		public double WorkSpeed
		{
			get { return workSpeed; }
			set
			{
				workSpeed = value;
				if (ready && dMotor != null)
				{
					dMotor.SetSpeed(value);
				}
			}
		}

		[Browsable(true)]
		[Category("#NP")]
		[DefaultValue(10.0)]
		public double ManualSpeed
		{
			get { return manualSpeed; }
			set { manualSpeed = value; }
		}

		[Browsable(true)]
		[Category("#NP")]
		[DefaultValue(100.0)]
		public double SpeedRatio
		{
			get { return speedRatio; }
			set
			{
				speedRatio = value;
				if (ready && dMotor != null)
				{
					dMotor.SetSpeedRatio(value);
				}
			}
		}

		[Browsable(true)]
		[Category("#NP")]
		[DefaultValue(0.1)]
		public double Tacc
		{
			get { return tacc; }
			set
			{
				tacc = value;
				if (ready && dMotor != null)
				{
					dMotor.SetAcceleration(value);
				}
			}
		}

		[Browsable(true)]
		[Category("#NP")]
		[DefaultValue(0.1)]
		public double Tdec
		{
			get { return tdec; }
			set
			{
				tdec = value;
				if (ready && dMotor != null)
				{
					dMotor.SetDeceleration(value);
				}
			}
		}

		[Browsable(true)]
		[Category("#NP")]
		[DefaultValue(1000.0)]
		public double MaxSpeed
		{
			get { return maxSpeed; }
			set
			{
				maxSpeed = value;
				if (ready && dMotor != null)
				{
					dMotor.SetMaxSpeed(value);
				}
			}
		}

		[Browsable(true)]
		[Category("#NP-脉冲当量")]
		[Description("指令脉冲当量（每毫米对应脉冲换算系数）")]
		[DefaultValue(1000.0)]
		public double PulseEquivalent
		{
			get { return pulseEquivalent; }
			set
			{
				if (value <= 0)
				{
					return;
				}
				pulseEquivalent = value;
				if (ready && dMotor != null)
				{
					dMotor.SetGearRatio(value);
				}
			}
		}

		[Browsable(true)]
		[Category("#NP-脉冲当量")]
		[Description("编码器脉冲当量")]
		[DefaultValue(1000.0)]
		public double EncoderPulseEquivalent
		{
			get { return encoderPulseEquivalent; }
			set
			{
				if (value <= 0)
				{
					return;
				}
				encoderPulseEquivalent = value;
				if (ready && dMotor != null)
				{
					dMotor.SetEncoderGearRatio(value);
				}
			}
		}

		/// <summary>同 PulseEquivalent，供 MotorControlForm 等旧代码调用。</summary>
		[Browsable(false)]
		[DefaultValue(1000.0)]
		public double GearRatio
		{
			get { return PulseEquivalent; }
			set { PulseEquivalent = value; }
		}

		/// <summary>同 EncoderPulseEquivalent，供 MotorControlForm 等旧代码调用。</summary>
		[Browsable(false)]
		[DefaultValue(1000.0)]
		public double EncGearRatio
		{
			get { return EncoderPulseEquivalent; }
			set { EncoderPulseEquivalent = value; }
		}

		[Browsable(true)]
		[Category("#NP-回原")]
		[Description("回原模式。雷赛 CiA402/home_mode；凌华 APS 轴参 16")]
		[DefaultValue(28)]
		public int HomeMode
		{
			get { return homeMode; }
			set
			{
				homeMode = value;
				if (ready && dMotor != null)
				{
					dMotor.SetHomeMode(value);
				}
			}
		}

		[Browsable(true)]
		[Category("#NP-回原")]
		[Description("回原方向，true=正方向")]
		[DefaultValue(false)]
		public bool HomeDir
		{
			get { return homeDir; }
			set
			{
				homeDir = value;
				if (ready && dMotor != null)
				{
					dMotor.SetHomeDir(value);
				}
			}
		}

		[Browsable(true)]
		[Category("#NP-回原")]
		[DefaultValue(20.0)]
		public double HomeHighSpeed
		{
			get { return homeHighSpeed; }
			set
			{
				homeHighSpeed = value;
				if (ready && dMotor != null)
				{
					dMotor.SetHomeHighSpd(value);
				}
			}
		}

		[Browsable(true)]
		[Category("#NP-回原")]
		[DefaultValue(5.0)]
		public double HomeLowSpeed
		{
			get { return homeLowSpeed; }
			set
			{
				homeLowSpeed = value;
				if (ready && dMotor != null)
				{
					dMotor.SetHomeLowSpd(value);
				}
			}
		}

		[Browsable(true)]
		[Category("#NP-回原")]
		[Description("回原完成后偏移量")]
		[DefaultValue(0.0)]
		public double HomeOffset
		{
			get { return homeOffset; }
			set
			{
				homeOffset = value;
				if (ready && dMotor != null)
				{
					dMotor.SetHomePos(value);
				}
			}
		}

		public NP_Motor()
		{
			Text = "Motor";
			TextAlign = ContentAlignment.MiddleCenter;
			Size = new Size(180, 30);
			DoubleClick += (s, e) => ShowParaForm();
		}

		/// <summary>双击打开轴参数设置页（显示当前控件内存中的参数，保存写入本控件独立文件）。</summary>
		public void ShowParaForm()
		{
			if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
			{
				return;
			}
			using (var form = new MotorParaForm(this))
			{
				form.ShowDialog(FindForm());
			}
		}

        /// <summary>连接并配置轴驱动，连接失败不能沿用上一次的就绪标记。</summary>
		public override void Initial()
		{
            ready = false;
			MotorParaForm.TryLoadFile(this);
			NPHub.SelectVendor(Vendor);
			dMotor = NPControlBase.CreateAxis(Vendor);
			if (dMotor == null)
			{
				ready = false;
				return;
			}
			dMotor.WorkSpeed = workSpeed;
			dMotor.SpeedRatio = speedRatio;
			dMotor.Tacc = tacc;
			dMotor.Tdec = tdec;
			dMotor.MaxSpeed = maxSpeed;
			dMotor.HomeMode = homeMode;
			dMotor.HomeDir = homeDir;
			dMotor.HomeHighSpd = homeHighSpeed;
			dMotor.HomeLowSpd = homeLowSpeed;
			dMotor.HomePos = homeOffset;
			dMotor.GearRatio = pulseEquivalent;
			dMotor.EncGearRatio = encoderPulseEquivalent;
			ready = dMotor.Initial(Port);
			if (ready)
			{
				dMotor.SetHomeMode(homeMode);
				dMotor.SetHomeDir(homeDir);
				dMotor.SetHomeHighSpd(homeHighSpeed);
				dMotor.SetHomeLowSpd(homeLowSpeed);
				dMotor.SetHomePos(homeOffset);
				dMotor.SetGearRatio(pulseEquivalent);
				dMotor.SetEncoderGearRatio(encoderPulseEquivalent);
			}
			SetToolTip("Port:" + Port);
		}

		public override void RefreshControl()
		{
			if (!ready || dMotor == null)
			{
				return;
			}
			dMotor.Refresh();
			IOState = dMotor.GetAxisIOState();
		}

		public override void DisposeObj()
		{
			if (dMotor != null)
			{
				dMotor.Dispose();
				dMotor = null;
			}
			ready = false;
		}

		public AxisIOState GetAxisIOState()
		{
			if (ready && dMotor != null)
			{
				IOState = dMotor.GetAxisIOState();
			}
			return IOState;
		}

		public bool Goto(double position)
		{
            if (!NPSDK.FlowResourceGate.CanAccess(this)) return false;
			return ready && dMotor != null && dMotor.Goto(position);
		}

		public bool Home()
		{
            if (!NPSDK.FlowResourceGate.CanAccess(this)) return false;
			return ready && dMotor != null && dMotor.Home();
		}

		public void HomeReset()
		{
			if (ready && dMotor != null)
			{
				dMotor.HomeReset();
			}
		}

        /// <summary>Forward alarm reset to the initialized motor driver.</summary>
        public void AlarmReset()
        {
            if (ready && dMotor != null) dMotor.AlarmReset();
        }

		public void ServoOn()
		{
			if (ready && dMotor != null)
			{
				dMotor.ServoOn();
			}
		}

		public void ServoOff()
		{
			if (ready && dMotor != null)
			{
				dMotor.ServoOff();
			}
		}

		public MotionState GetMotionState()
		{
			if (ready && dMotor != null)
			{
				return dMotor.GetMotionState();
			}
			return default(MotionState);
		}

		public void JogP()
		{
            if (!NPSDK.FlowResourceGate.CanAccess(this)) return;
			if (ready && dMotor != null)
			{
				dMotor.JogP();
			}
		}

		public void JogN()
		{
            if (!NPSDK.FlowResourceGate.CanAccess(this)) return;
			if (ready && dMotor != null)
			{
				dMotor.JogN();
			}
		}

        /// <summary>取消绑定界面的手动会话并停止驱动；与手动回零轮询串行，防止停止后再次启动。</summary>
        public void Stop()
        {
            lock (this)
            {
                EventHandler requested = StopRequested;
                if (requested != null) requested(this, EventArgs.Empty);
                if (ready && dMotor != null) dMotor.FastStop();
            }
        }

		public double GetPos()
		{
			return ready && dMotor != null ? dMotor.GetPos() : 0;
		}

		public double GetEncPos()
		{
			return ready && dMotor != null ? dMotor.GetEncPos() : 0;
		}

		public bool GetBusy()
		{
			return ready && dMotor != null && dMotor.GetBusy();
		}
	}
}
