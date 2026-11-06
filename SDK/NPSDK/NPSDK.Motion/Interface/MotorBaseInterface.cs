using System.CodeDom;

namespace NPSDK
{
	public class MotorBaseInterface
	{
		protected int AxisID;

		public bool IsStepMotor;

		public double SpeedRatio;

		public double MaxSpeed;

		public double WorkSpeed;

		public double Tacc;

		public double Tdec;

		public bool MoveDir;

		public bool HomeDir;

		/// <summary>��ԭģʽ��������Ӧ nmc_set_home_profile �� home_mode���軪��Ӧ APS ��� 16��</summary>
		public int HomeMode;

		public bool HomeModel;

		public double GearRatio = 1000;

		public double EncGearRatio = 1000;

		public bool InvertComPos;

		public bool InvertEncPos;

		public int DestPos;

		public PulseModeType PulseMode;

		public double TriggerStartPos;

		public double TriggerEndPos;

		public double TriggerDis;

		public double HomeHighSpd;

		public double HomeLowSpd;

		public double HomePos;

		public int HomeDelay;

		public bool InvertEL;

		public bool InvertORG;

		public bool UseHomeSensor;

		protected bool IsHoming;

		protected int iHomeTask;

		protected bool IsHomeOk;

       

        protected JTimer tmHome = new JTimer();

		public MotionState MState;

		public AxisIOState IOState;


        public virtual bool Initial(string Port)
		{
			return false;
		}

		public virtual void Dispose()
		{
		}

		public virtual void SetSpeedRatio(double mRatio)
		{
		}

		public virtual void SetMaxSpeed(double mSpeed)
		{
		}

		public virtual void SetSpeed(double mSpeed)
		{
		}

		public virtual void SetAcceleration(double mAcceleration)
		{
		}

		public virtual void SetDeceleration(double mDeceleration)
		{

		}

		public virtual void SetHomeDir(bool mHomeDir)
		{
			HomeDir = mHomeDir;
		}

		public virtual void SetHomeMode(int mHomeMode)
		{
			HomeMode = mHomeMode;
		}

		public virtual void SetMoveDir(bool mMoveDir)
		{
		}

		public virtual void SetGearRatio(double mRatio)
		{
			GearRatio = mRatio;
		}

		public virtual void SetEncoderGearRatio(double mRatio)
		{
			EncGearRatio = mRatio;
		}

		public virtual int GetHomeMode()
		{
			return HomeMode;
		}

		public virtual void SetPos(double Pos)
		{
		}

		public virtual void SetEncPos(double RealPos)
		{
		}

		public virtual void SetHomeHighSpd(double mSpeed)
		{
		}

		public virtual void SetHomeLowSpd(double mSpeed)
		{
		}

		public virtual void SetHomePos(double mPos)
		{
		}

		public virtual void SetHomeDelay(int mDelayTM)
		{
		}

		public virtual void SetInvertEL(bool mInvert)
		{
		}

		public virtual void SetInvertORG(bool mInvert)
		{
		}

		public virtual void SetInvertComPos(bool mInvert)
		{
		}

		public virtual void SetInvertEncPos(bool mInvert)
		{
		}

		public virtual void SetPulseMode(PulseModeType mMode)
		{
		}

		public virtual void SetTriggerStartPos(double mTriggerStartPos)
		{
		}

		public virtual void SetTriggerEndPos(double mTriggerEndPos)
		{
		}

		public virtual void SetTriggerDis(double mTriggerDis)
		{
		}

		public virtual void SetSoftLimit(double PLimit, double NLimit)
		{
		}

		public virtual void SetUseHomeSensor(bool mUseHomeSensor)
		{
		}

		public virtual void ServoOn()
		{
		}

		public virtual void ServoOff()
		{
		}

		public virtual bool Goto(double Pos)
		{
			return false;
		}

        public virtual bool LinearMove(MotorBaseInterface Slave, double TargetPos, double SlaveTargetPos)
        {
            return false;
        }
		//Ť��ģʽ
        public virtual bool SET_field_bus_od_data(int Borad, int  Bus_No, int MOD_No,int SubMod_No,int ODIndex,uint RawData)
		{
			return false;
		}
        //��ȡŤ��
        public virtual uint GetTargetTorqueValue(int Borad, int Bus_No, int MOD_No, int SubMod_No, int ODIndex, uint RawData)
        {
			return 0;
        }

        public virtual bool Home()
		{
			return false;
		}

		public virtual void HomeReset()
		{
		}

		public virtual void Stop()
		{
		}

		public virtual void FastStop()
		{
		}

		public virtual void JogP()
		{
		}

		public virtual void JogN()
		{
		}

		public virtual void TriggerManual()
		{
		}

		public virtual void TriggerAuto(bool bOn)
		{
		}

		public virtual void AlarmReset()
		{
		}

		public virtual bool EnableSoftLimit()
		{
			return false;
		}

		public virtual void Refresh()
		{
		}

		public virtual double GetPos()
		{
			return 0.0;
		}

		public virtual double GetEncPos()
		{
			return 0.0;
		}

		public virtual double GetWorkSpeed()
		{
			return 0.0;
		}

		public virtual double GetAcceleration()
		{
			return 0.0;
		}

		public virtual double GetDeceleration()
		{
			return 0.0;
		}

		public virtual bool GetHomeDirection()
		{
			return false;
		}

		public virtual bool GetMoveDirection()
		{
			return false;
		}

		public virtual double GetGearRatio()
		{
			return GearRatio;
		}

		public virtual double GetEncoderGearRatio()
		{
			return EncGearRatio;
		}

		public virtual MotionState GetMotionState()
		{
			return default(MotionState);
		}

		public virtual AxisIOState GetAxisIOState()
		{
			return default(AxisIOState);
		}

		public virtual bool GetInvertEL()
		{
			return false;
		}

		public virtual bool GetInvertORG()
		{
			return false;
		}

		public virtual bool GetInvertComPos()
		{
			return false;
		}

		public virtual bool GetInvertEncPos()
		{
			return false;
		}

		public virtual bool GetBusy()
		{
			return false;
		}

		public virtual bool GetHoming()
		{
			return false;
		}

		public int GetAxisID()
		{
			return AxisID;
		}

		public virtual void Couple(MotorBaseInterface Master)
		{
		}

		public virtual void Decouple()
		{
		}

		protected double MMSToPPS(double mValue)
		{
			double num = mValue / GearRatio;
			num /= 10.0;
			return num * 10000.0;
		}

		public virtual bool LockPos(int iPos)
		{
			if (iPos == DestPos)
			{
				return true;
			}
			DestPos = iPos;
			return false;
		}
	}
}
