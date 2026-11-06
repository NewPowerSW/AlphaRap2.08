using System;
using csLTDMC;
using NPSDK.Motion;

namespace NPSDK
{
	/// <summary>雷赛轴驱动，实现与凌华相同的 MotorBaseInterface。</summary>
	public class LeadshineNP_Motor : MotorBaseInterface
	{
		private bool ready;
		private bool homeStarted;
		private AxisIOState ioState;

        /// <summary>确认板卡及目标轴存在并设置轴参数，不发出运动或回零指令。</summary>
		public override bool Initial(string Port)
		{
            ready = false;
			try
			{
				if (!NPHub.IsInitialized)
				{
					if (!NPHub.Initial(NPVendor.Leadshine))
					{
						return false;
					}
				}
				if (string.IsNullOrEmpty(Port))
				{
					return false;
				}
				AxisID = Convert.ToInt32(Port.Substring(Port.Length - 1));
                uint axisCount = 0;
                if (LTDMC.dmc_get_total_axes(0, ref axisCount) != 0 || AxisID >= axisCount) return false;
				if (GearRatio <= 0) GearRatio = 1000;
				if (EncGearRatio <= 0) EncGearRatio = 1000;
				if (HomeHighSpd <= 0) HomeHighSpd = 20;
				if (HomeLowSpd <= 0) HomeLowSpd = 5;
                if (LTDMC.dmc_set_equiv(0, (ushort)AxisID, GearRatio) != 0) return false;
				ApplyHomeProfile();
				ready = true;
				return true;
			}
			catch
			{
				ready = false;
				return false;
			}
		}

		public override void Dispose()
		{
			ready = false;
		}

		public override void ServoOn()
		{
			if (ready) LTDMC.nmc_set_axis_enable(0, (ushort)AxisID);
		}

		public override void ServoOff()
		{
			if (ready) LTDMC.nmc_set_axis_disable(0, (ushort)AxisID);
		}

		public override void Stop()
		{
			if (!ready) return;
			LTDMC.dmc_stop(0, (ushort)AxisID, 0);
			homeStarted = false;
		}

		public override void FastStop()
		{
			if (!ready) return;
			LTDMC.dmc_stop(0, (ushort)AxisID, 1);
			homeStarted = false;
		}

		public override bool Goto(double Pos)
		{
			if (!ready) return false;
            bool done = !GetBusy();
			if (done && Math.Abs(GetEncPos() - Pos) < 0.2)
			{
				return true;
			}
			if (!done)
			{
				return false;
			}
			ApplySpeed();
			LTDMC.dmc_pmove_unit(0, (ushort)AxisID, Pos, 1);
			return false;
		}

		public override bool Home()
		{
			if (!ready) return false;
			if (homeStarted)
			{
                if (GetBusy())
				{
					return false;
				}
				ushort result = 0;
				LTDMC.dmc_get_home_result(0, (ushort)AxisID, ref result);
				IsHomeOk = result == 1;
				if (IsHomeOk)
				{
					homeStarted = false;
					IsHoming = false;
				}
				return IsHomeOk;
			}
            if (GetBusy()) return false;
			ApplyHomeProfile();
			homeStarted = true;
			IsHoming = true;
			IsHomeOk = false;
			LTDMC.nmc_home_move(0, (ushort)AxisID);
			return false;
		}

		public override void HomeReset()
		{
			homeStarted = false;
			IsHoming = false;
			IsHomeOk = false;
		}

		public override void SetHomeMode(int mHomeMode)
		{
			HomeMode = mHomeMode;
			if (ready)
			{
				ApplyHomeProfile();
			}
		}

		public override void SetHomeDir(bool mHomeDir)
		{
			HomeDir = mHomeDir;
			if (ready)
			{
				ApplyHomeProfile();
			}
		}

		public override void SetHomeHighSpd(double mSpeed)
		{
			HomeHighSpd = mSpeed;
			if (ready)
			{
				ApplyHomeProfile();
			}
		}

		public override void SetHomeLowSpd(double mSpeed)
		{
			HomeLowSpd = mSpeed;
			if (ready)
			{
				ApplyHomeProfile();
			}
		}

		public override void SetHomePos(double mPos)
		{
			HomePos = mPos;
			if (ready)
			{
				ApplyHomeProfile();
			}
		}

		public override void SetGearRatio(double mRatio)
		{
			if (mRatio <= 0)
			{
				return;
			}
			GearRatio = mRatio;
			if (ready)
			{
				LTDMC.dmc_set_equiv(0, (ushort)AxisID, GearRatio);
			}
		}

		public override void SetEncoderGearRatio(double mRatio)
		{
			if (mRatio <= 0)
			{
				return;
			}
			EncGearRatio = mRatio;
		}

		private void ApplyHomeProfile()
		{
			ushort mode = (ushort)HomeMode;
			double low = HomeLowSpd > 0 ? HomeLowSpd : 5;
			double high = HomeHighSpd > 0 ? HomeHighSpd : 20;
			double acc = Tacc > 0 ? Tacc : 0.1;
			double dec = Tdec > 0 ? Tdec : 0.1;
			LTDMC.nmc_set_home_profile(0, (ushort)AxisID, mode, low, high, acc, dec, HomePos);
			LTDMC.dmc_set_homemode(0, (ushort)AxisID, (ushort)(HomeDir ? 1 : 0), high, mode, 0);
		}

		public override void JogP()
		{
			if (!ready) return;
			ApplySpeed();
			LTDMC.dmc_vmove(0, (ushort)AxisID, 1);
		}

		public override void JogN()
		{
			if (!ready) return;
			ApplySpeed();
			LTDMC.dmc_vmove(0, (ushort)AxisID, 0);
		}

		public override void AlarmReset()
		{
			if (ready) LTDMC.nmc_clear_axis_errcode(0, (ushort)AxisID);
		}

		public override double GetPos()
		{
			double pos = 0;
			if (ready) LTDMC.dmc_get_position_unit(0, (ushort)AxisID, ref pos);
			return Math.Round(pos, 3);
		}

		public override double GetEncPos()
		{
			double pos = 0;
			if (ready) LTDMC.dmc_get_encoder_unit(0, (ushort)AxisID, ref pos);
			return Math.Round(pos, 3);
		}

		public override double GetWorkSpeed()
		{
			double speed = 0;
			if (ready) LTDMC.dmc_read_current_speed_unit(0, (ushort)AxisID, ref speed);
			return Math.Round(speed, 3);
		}

        /// <summary>仅将有效的运动反馈 0 解释为运动中，其他异常返回值作为读取失败报告。</summary>
		public override bool GetBusy()
		{
			if (!ready) return false;
            short state = LTDMC.dmc_check_done(0, (ushort)AxisID);
            if (state != 0 && state != 1) throw new InvalidOperationException("dmc_check_done returned " + state + " for axis " + AxisID + ".");
            return state == 0;
		}

		public override MotionState GetMotionState()
		{
			MotionState state = default(MotionState);
			if (!ready)
			{
				return state;
			}
			bool busy = GetBusy();
			state.HMV = busy && IsHoming;
			state.SMV = busy && !IsHoming;
			return state;
		}

		public override void Refresh()
		{
			if (!ready)
			{
				return;
			}
			uint bits = LTDMC.dmc_axis_io_status(0, (ushort)AxisID);
			AxisIOState state = default(AxisIOState);
			state.ALM = (bits & 1u) != 0;
			state.PEL = (bits & 2u) != 0;
			state.MEL = (bits & 4u) != 0;
			state.EMG = (bits & 8u) != 0;
			state.ORG = (bits & 16u) != 0;
			state.SLP = (bits & 32u) != 0;
			state.SLN = (bits & 64u) != 0;
			ushort machine = 0;
			LTDMC.nmc_get_axis_state_machine(0, (ushort)AxisID, ref machine);
			state.SVON = machine == 4;
			ioState = state;
		}

		public override AxisIOState GetAxisIOState()
		{
			return ioState;
		}

		public override void SetSpeedRatio(double mRatio)
		{
			SpeedRatio = mRatio;
		}

		public override void SetMaxSpeed(double mSpeed)
		{
			MaxSpeed = mSpeed;
		}

		public override void SetSpeed(double mSpeed)
		{
			WorkSpeed = mSpeed;
			ApplySpeed();
		}

		public override void SetAcceleration(double mAcceleration)
		{
			Tacc = mAcceleration;
		}

		public override void SetDeceleration(double mDeceleration)
		{
			Tdec = mDeceleration;
		}

		private void ApplySpeed()
		{
			if (!ready) return;
			double ratio = SpeedRatio > 1.0 ? SpeedRatio / 100.0 : SpeedRatio;
			if (ratio < 0.01) ratio = 0.1;
			LTDMC.dmc_set_profile_unit(0, (ushort)AxisID, 0, WorkSpeed * ratio, Tacc, Tdec, 0);
		}
	}
}
