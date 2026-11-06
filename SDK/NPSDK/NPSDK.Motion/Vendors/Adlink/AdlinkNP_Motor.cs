#if NP_VENDOR_ADLINK
using APS168_W64;
using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace NPSDK
{
    internal class AdlinkNP_Motor : MotorBaseInterface
    {
        private short CardID = 0;

        private const short BusNo = 0;



        public override bool Initial(string Port)
        {
            try
            {
                if (!NPSDKPara.bConnectNETMotion)
                {
                    APS168.APS_stop_field_bus(CardID, 0);
                    if (!NPSDKPara.bAPSInitial)
                    {
                        int BoardID_InBits = 0;
                        if (APS168.APS_initial(ref BoardID_InBits, 0) == 0)
                        {
                            NPSDKPara.bAPSInitial_Ok = true;
                            NPSDKPara.bAPSInitial = true;
                        }
                    }
                    if (NPSDKPara.bAPSInitial_Ok && APS168.APS_start_field_bus(CardID, 0, 0) == 0)
                    {
                        NPSDKPara.bConnectNETIO_Ok = true;
                        NPSDKPara.bConnectNETMotion_Ok = true;
                    }
                    NPSDKPara.bConnectNETMotion = true;
                }
                if (NPSDKPara.bConnectNETMotion_Ok && Port.Length == 3)
                {
                    int num = Convert.ToInt16(Port.Substring(0, 2));
                    int num2 = Convert.ToInt16(Port.Substring(2, 1));
                    if (num2 < 4)
                    {
                        AxisID = num * 4 + num2;
                        int num3 = APS168.APS_set_axis_param(AxisID, 2, 0);
                        num3 = APS168.APS_set_axis_param(AxisID, 3, 0);
                        num3 = APS168.APS_set_axis_param(AxisID, 8, 0);
                        num3 = APS168.APS_set_axis_param(AxisID, 9, 0);
                        num3 = APS168.APS_set_axis_param(AxisID, 354, InvertComPos ? 1 : 0);
                        switch (PulseMode)
                        {
                            case PulseModeType.AB_Pulse:
                                num3 = APS168.APS_set_axis_param(AxisID, 352, 2);
                                break;
                            case PulseModeType.CW_CCW:
                                num3 = APS168.APS_set_axis_param(AxisID, 352, 3);
                                break;
                            case PulseModeType.OUT_DIR:
                                num3 = APS168.APS_set_axis_param(AxisID, 352, 4);
                                break;
                        }
                        num3 = APS168.APS_set_axis_param_f(AxisID, 32, 0.0);
                        double num4 = MMSToPPS(WorkSpeed);
                        double value = num4 / Tacc;
                        double value2 = num4 / Tdec;
                        num3 = APS168.APS_set_axis_param_f(AxisID, 33, Convert.ToInt32(value));
                        num3 = APS168.APS_set_axis_param_f(AxisID, 34, Convert.ToInt32(value2));
                        num3 = APS168.APS_set_axis_param_f(AxisID, 35, 0.0);
                        num3 = APS168.APS_set_axis_param_f(AxisID, 36, MMSToPPS(MaxSpeed));
                        num3 = APS168.APS_set_axis_param(AxisID, 16, HomeMode > 0 ? HomeMode : 4);
                        num3 = APS168.APS_set_axis_param(AxisID, 17, HomeDir ? 1 : 0);
                        num3 = APS168.APS_set_axis_param(AxisID, 24, 0);
                        return num3 == 0;
                    }
                }
            }
            catch (Exception)
            {
            }
            return false;
        }

        public override void Dispose()
        {
            APS168.APS_stop_field_bus(CardID, 0);
            APS168.APS_close();
        }

        public override void SetSpeedRatio(double mRatio)
        {
            SpeedRatio = mRatio;
        }

        public override void SetMaxSpeed(double mSpeed)
        {
            MaxSpeed = mSpeed;
            APS168.APS_set_axis_param_f(AxisID, 36, MMSToPPS(MaxSpeed));
        }

        private double GetRealSpd(double mSpeed)
        {
            double num = MMSToPPS(MaxSpeed);
            if (mSpeed > num)
            {
                return MaxSpeed;
            }
            return mSpeed;
        }

        public override void SetSpeed(double mSpeed)
        {
            if (mSpeed > MaxSpeed)
            {
                WorkSpeed = MaxSpeed;
            }
            else
            {
                WorkSpeed = mSpeed;
            }
            SetAcceleration(Tacc);
            SetDeceleration(Tdec);
        }

        public override void SetAcceleration(double mAcceleration)
        {
            Tacc = mAcceleration;
            double num = MMSToPPS(WorkSpeed);
            double value = num / mAcceleration;
            APS168.APS_set_axis_param_f(AxisID, 33, Convert.ToInt32(value));
        }

        public override void SetDeceleration(double mDeceleration)
        {
            Tdec = mDeceleration;
            double num = MMSToPPS(WorkSpeed);
            double value = num / Tdec;
            APS168.APS_set_axis_param_f(AxisID, 34, Convert.ToInt32(value));
        }

        public override void SetHomeDir(bool HomeDirection)
        {
            HomeDir = HomeDirection;
            APS168.APS_set_axis_param(AxisID, 17, HomeDir ? 1 : 0);
        }

        public override void SetHomeMode(int mHomeMode)
        {
            HomeMode = mHomeMode;
            APS168.APS_set_axis_param(AxisID, 16, HomeMode > 0 ? HomeMode : 4);
        }

        public override void SetMoveDir(bool MoveDirection)
        {
            MoveDir = MoveDirection;
        }

        public override void SetGearRatio(double mGearRatio)
        {
            double workSpeed = WorkSpeed;
            workSpeed *= GearRatio;
            GearRatio = mGearRatio;
            SetSpeed(workSpeed / GearRatio);
        }

        public override void SetEncoderGearRatio(double mEncoderGearRatio)
        {
            EncGearRatio = mEncoderGearRatio;
        }

        public override void SetPos(double Pos)
        {
            double num = Pos / GearRatio;
            APS168.APS_set_command(AxisID, (int)(num * 1000.0));
            DestPos = (int)(num * 1000.0);
        }

        public override void SetEncPos(double RealPos)
        {
            double num = RealPos / EncGearRatio;
            APS168.APS_set_position(AxisID, (int)(num * 1000.0));
        }

        public override void SetHomeHighSpd(double mSpeed)
        {
            if (mSpeed > MaxSpeed)
            {
                HomeHighSpd = MaxSpeed;
            }
            else
            {
                HomeHighSpd = mSpeed;
            }
        }

        public override void SetHomeLowSpd(double mSpeed)
        {
            if (mSpeed > MaxSpeed)
            {
                HomeLowSpd = MaxSpeed;
            }
            else
            {
                HomeLowSpd = mSpeed;
            }
        }

        //Ť��ģʽ
        public override bool SET_field_bus_od_data(int Borad, int Bus_No, int MOD_No, int SubMod_No, int ODIndex, uint RawData)
        {
            int ret = APS168.APS_set_field_bus_od_data(Borad, Bus_No, MOD_No, SubMod_No, ODIndex, RawData);
            return ret == 0 ? true : false;


        }
        //��ȡŤ��
        public override uint GetTargetTorqueValue(int Borad, int Bus_No, int MOD_No, int SubMod_No, int ODIndex, uint RawData)
        {
            uint TargetTorqueValue = 0;
            int ret = APS168.APS_get_field_bus_od_data(Borad, Bus_No, MOD_No, SubMod_No, ODIndex, ref TargetTorqueValue);
            if (ret == 0)
            {
                return TargetTorqueValue;
            }
            else
            {
                return 0;
            }
        }


        public override void SetHomePos(double mPos)
        {
            HomePos = mPos;
        }

        public override void SetHomeDelay(int mDelayTM)
        {
            HomeDelay = mDelayTM;
        }

        public override void SetInvertEL(bool mInvert)
        {
            InvertEL = mInvert;
        }

        public override void SetInvertORG(bool mInvert)
        {
            InvertORG = mInvert;
        }

        public override void SetInvertComPos(bool mInvert)
        {
            InvertComPos = mInvert;
        }

        public override void SetInvertEncPos(bool mInvert)
        {
            InvertEncPos = mInvert;
        }

        public override void SetPulseMode(PulseModeType mMode)
        {
            PulseMode = mMode;
        }

        public override void SetTriggerStartPos(double mTriggerStartPos)
        {
            TriggerStartPos = mTriggerStartPos;
        }

        public override void SetTriggerEndPos(double mTriggerEndPos)
        {
            TriggerEndPos = mTriggerEndPos;
        }

        public override void SetTriggerDis(double mTriggerDis)
        {
            TriggerDis = mTriggerDis;
        }

        public override void SetSoftLimit(double PLimit, double NLimit)
        {
            bool flag = false;
            float num = Convert.ToInt32(PLimit * 1000.0 / GearRatio);
            float num2 = Convert.ToInt32(NLimit * 1000.0 / GearRatio);
            if (InvertComPos)
            {
                flag |= APS168.APS_set_axis_param_f(AxisID, 10, 0f - num) != 0;
                flag |= APS168.APS_set_axis_param_f(AxisID, 11, 0f - num2) != 0;
            }
            else
            {
                flag |= APS168.APS_set_axis_param_f(AxisID, 10, num2) != 0;
                flag |= APS168.APS_set_axis_param_f(AxisID, 11, num) != 0;
            }
        }

        public override void SetUseHomeSensor(bool mUseHomeSensor)
        {
            UseHomeSensor = mUseHomeSensor;
        }

        public override void ServoOn()
        {
            APS168.APS_set_servo_on(AxisID, 1);
        }

        public override void ServoOff()
        {
            APS168.APS_set_servo_on(AxisID, 0);
        }

        public override bool Goto(double Pos)
        {
            double num = (InvertComPos ? (0.0 - Pos) : Pos);
            double num2 = MMSToPPS(WorkSpeed);
            num2 *= SpeedRatio / 100.0;
            num2 = GetRealSpd(num2);
            if (!LockPos(Convert.ToInt32(num * 1000.0)))
            {
                double num3 = num / GearRatio;
                if (!GetBusy())
                {
                    APS168.APS_absolute_move(AxisID, (int)(num3 * 1000.0), (int)num2);
                }
                else
                {
                    DestPos = Convert.ToInt32(GetEncPos() / GearRatio * 1000.0);
                }
                return false;
            }
            if (Math.Abs(GetPos() * 1000.0 - Pos * 1000.0) < 5.0 * GearRatio && !MState.SMV)
            {
                return true;
            }
            return false;
        }

        public override bool LinearMove(MotorBaseInterface Slave, double TargetPos, double SlaveTargetPos)
        {
            if (!LockPos(Convert.ToInt32(TargetPos * 1000.0)))
            {
                if (!GetBusy() && !Slave.GetBusy())
                {
                    double num = MMSToPPS(WorkSpeed);
                    num *= SpeedRatio / 100.0;
                    num = GetRealSpd(num);
                    double num2 = TargetPos / GearRatio * 1000.0;
                    double num3 = SlaveTargetPos / GearRatio * 1000.0;
                    int[] axis_ID_Array = new int[2]
                    {
                        AxisID,
                        Slave.GetAxisID()
                    };
                    int[] position_Array = new int[2]
                    {
                        (int)num2,
                        (int)num3
                    };
                    APS168.APS_absolute_linear_move(2, axis_ID_Array, position_Array, (int)num);


                }
                else
                {
                    DestPos = Convert.ToInt32(GetEncPos() / GearRatio * 1000.0);
                    Slave.DestPos = Convert.ToInt32(Slave.GetEncPos() / Slave.GearRatio * 1000.0);
                }
                return false;
            }
            bool flag = false;
            bool flag2 = false;
            if (Math.Abs(GetEncPos() * 1000.0 - TargetPos * 1000.0) < 5.0 * GearRatio && !MState.SMV)
            {
                flag = true;
            }
            if (Math.Abs(Slave.GetEncPos() * 1000.0 - SlaveTargetPos * 1000.0) < 5.0 * Slave.GearRatio && !Slave.MState.SMV)
            {
                flag2 = true;
            }
            if (flag && flag2)
            {
                return true;
            }
            return false;
        }

        public override bool Home()
        {
            IsHoming = true;
            switch (iHomeTask)
            {
                case 1:
                    {
                        double mSpeed = MMSToPPS(HomeHighSpd);
                        mSpeed = GetRealSpd(mSpeed);
                        APS168.APS_set_axis_param_f(AxisID, 21, (int)mSpeed);
                        APS168.APS_set_axis_param_f(AxisID, 25, (int)mSpeed);
                        double value = mSpeed / 0.1;
                        double value2 = mSpeed / 0.1;
                        APS168.APS_set_axis_param_f(AxisID, 33, Convert.ToInt32(value));
                        APS168.APS_set_axis_param_f(AxisID, 34, Convert.ToInt32(value2));
                        APS168.APS_set_axis_param(AxisID, 16, 0);
                        APS168.APS_set_axis_param(AxisID, 17, HomeDir ? 1 : 0);
                  

                        APS168.APS_home_move(AxisID);
                        iHomeTask = 11;
                        Debug.WriteLine("Hometask 1 start homing");
                        break;
                    }
                case 11:
                    {
                        bool flag = MState.HMV;
                        NPMotionState yHMotionState = GetYHMotionState();
                        if (yHMotionState != null)
                        {
                            flag = flag || yHMotionState.HomeMove || yHMotionState.Acc || yHMotionState.Dec;
                        }
                        if (!flag)
                        {
                            tmHome.Restart();
                            Debug.WriteLine("Hometask 11 reset encoder");
                            iHomeTask = 14;
                        }
                        break;
                    }
                case 14:
                    if (tmHome.IsOn(HomeDelay))
                    {
                        SetAcceleration(Tacc);
                        SetDeceleration(Tdec);
                        Debug.WriteLine("Hometask 14 home go home success");
                        IsHomeOk = true;
                        iHomeTask++;
                    }
                    break;
                case 15:
                    IsHomeOk = true;
                    IsHoming = false;
                    DestPos = Convert.ToInt32(GetEncPos() / GearRatio * 1000.0);
                    return true;
            }
            return false;
        }

        public override void HomeReset()
        {
            IsHomeOk = false;
            iHomeTask = 1;
        }

        public override void JogP()
        {
            double num = MMSToPPS(WorkSpeed);
            num *= SpeedRatio / 100.0;
            num = GetRealSpd(num);
            int num2 = APS168.APS_velocity_move(AxisID, MoveDir ? ((int)num) : (-(int)num));
            Debug.WriteLine($"return={num2}");
        }

        public override void JogN()
        {
            double num = MMSToPPS(WorkSpeed);
            num *= SpeedRatio / 100.0;
            num = GetRealSpd(num);
            int num2 = APS168.APS_velocity_move(AxisID, MoveDir ? (-(int)num) : ((int)num));
            Debug.WriteLine($"return={num2}");
        }

        public override void Stop()
        {
            if (IsHoming)
            {
                IsHoming = false;
                HomeReset();
            }
            APS168.APS_stop_move(AxisID);
            DestPos = Convert.ToInt32(GetEncPos() / GearRatio * 1000.0);
        }

        public override void FastStop()
        {
            if (IsHoming)
            {
                IsHoming = false;
                HomeReset();
            }
            APS168.APS_emg_stop(AxisID);
            SetPos(Convert.ToInt32(GetEncPos() / GearRatio * 1000.0));
            DestPos = Convert.ToInt32(GetEncPos() / GearRatio * 1000.0);
        }

        public override void TriggerManual()
        {
            switch ((AxisID - 1500) % 4)
            {
                case 0:
                    APS168.APS_set_field_bus_trigger_param(CardID, 0, 0, 20, 1000);
                    break;
                case 1:
                    APS168.APS_set_field_bus_trigger_param(CardID, 0, 0, 21, 1000);
                    break;
                case 2:
                    APS168.APS_set_field_bus_trigger_param(CardID, 0, 0, 22, 1000);
                    break;
                case 3:
                    APS168.APS_set_field_bus_trigger_param(CardID, 0, 0, 23, 1000);
                    break;
            }
            APS168.APS_set_field_bus_trigger_manual(CardID, 0, 0, 0);
        }

        public override void TriggerAuto(bool bOn)
        {
            int num = (AxisID - 1500) % 4;
            if (bOn)
            {
                switch (num)
                {
                    case 0:
                        APS168.APS_set_field_bus_trigger_param(CardID, 0, 0, 0, 1);
                        APS168.APS_set_field_bus_trigger_param(CardID, 0, 0, 4, 1);
                        APS168.APS_set_field_bus_trigger_param(CardID, 0, 0, 8, 1);
                        APS168.APS_set_field_bus_trigger_param(CardID, 0, 0, 20, 1000);
                        APS168.APS_set_field_bus_trigger_param(CardID, 0, 0, 24, 0);
                        break;
                    case 1:
                        APS168.APS_set_field_bus_trigger_param(CardID, 0, 0, 1, 1);
                        APS168.APS_set_field_bus_trigger_param(CardID, 0, 0, 5, 1);
                        APS168.APS_set_field_bus_trigger_param(CardID, 0, 0, 9, 1);
                        APS168.APS_set_field_bus_trigger_param(CardID, 0, 0, 21, 1000);
                        APS168.APS_set_field_bus_trigger_param(CardID, 0, 0, 25, 0);
                        break;
                    case 2:
                        APS168.APS_set_field_bus_trigger_param(CardID, 0, 0, 2, 1);
                        APS168.APS_set_field_bus_trigger_param(CardID, 0, 0, 6, 1);
                        APS168.APS_set_field_bus_trigger_param(CardID, 0, 0, 10, 1);
                        APS168.APS_set_field_bus_trigger_param(CardID, 0, 0, 22, 1000);
                        APS168.APS_set_field_bus_trigger_param(CardID, 0, 0, 26, 0);
                        break;
                    case 3:
                        APS168.APS_set_field_bus_trigger_param(CardID, 0, 0, 3, 1);
                        APS168.APS_set_field_bus_trigger_param(CardID, 0, 0, 7, 1);
                        APS168.APS_set_field_bus_trigger_param(CardID, 0, 0, 11, 1);
                        APS168.APS_set_field_bus_trigger_param(CardID, 0, 0, 23, 1000);
                        APS168.APS_set_field_bus_trigger_param(CardID, 0, 0, 27, 0);
                        break;
                }
                double num2 = TriggerStartPos * 1000.0 / GearRatio;
                double num3 = TriggerEndPos * 1000.0 / GearRatio;
                double num4 = TriggerDis * 1000.0 / GearRatio;
                double num5 = (num3 - num2) / num4;
                APS168.APS_set_field_bus_trigger_linear(CardID, 0, 0, 0, (int)num2, (int)num5, (int)num4);
            }
            else
            {
                switch (num)
                {
                    case 0:
                        APS168.APS_set_field_bus_trigger_param(CardID, 0, 0, 4, 0);
                        break;
                    case 1:
                        APS168.APS_set_field_bus_trigger_param(CardID, 0, 0, 5, 0);
                        break;
                    case 2:
                        APS168.APS_set_field_bus_trigger_param(CardID, 0, 0, 6, 0);
                        break;
                    case 3:
                        APS168.APS_set_field_bus_trigger_param(CardID, 0, 0, 7, 0);
                        break;
                }
            }
        }

        public override double GetPos()
        {
            int Command = 0;
            APS168.APS_get_command(AxisID, ref Command);
            double num = (double)Command * GearRatio / 1000.0;
            return InvertComPos ? (0.0 - num) : num;
        }

        public override double GetEncPos()
        {
            int Position = 0;
            APS168.APS_get_position(AxisID, ref Position);
            return (double)Position * EncGearRatio / 1000.0;
        }

        public override double GetWorkSpeed()
        {
            return WorkSpeed;
        }

        public override double GetAcceleration()
        {
            return Tacc;
        }

        public override double GetDeceleration()
        {
            return Tdec;
        }

        public override bool GetHomeDirection()
        {
            int AXS_Param = 0;
            APS168.APS_get_axis_param(AxisID, 17, ref AXS_Param);
            return AXS_Param != 0;
        }

        public override bool GetMoveDirection()
        {
            return MoveDir;
        }

        public override double GetGearRatio()
        {
            return GearRatio;
        }

        public override double GetEncoderGearRatio()
        {
            return EncGearRatio;
        }

        public override MotionState GetMotionState()
        {
            return MState;
        }

        public override AxisIOState GetAxisIOState()
        {
            return IOState;
        }

        public override bool GetInvertEL()
        {
            return InvertEL;
        }

        public override bool GetInvertORG()
        {
            return InvertORG;
        }

        public override bool GetInvertComPos()
        {
            return InvertComPos;
        }

        public override bool GetInvertEncPos()
        {
            return InvertEncPos;
        }

        public override bool GetBusy()
        {
            Refresh();
            return MState.SMV | MState.HMV;
        }

        public override bool GetHoming()
        {
            return IsHoming;
        }

        public override void Refresh()
        {
            AxisIoStatus_Convert(APS168.APS_motion_io_status(AxisID));
            AxisMoStatus_Convert(APS168.APS_motion_status(AxisID));
        }

        private NPMotionState GetYHMotionState()
        {
            int value = APS168.APS_motion_status(AxisID);
            byte[] bytes = BitConverter.GetBytes(value);
            BitArray bitArray = new BitArray(bytes);
            bool[] array = new bool[32];
            for (int i = 0; i < bitArray.Length; i++)
            {
                array[i] = bitArray[i];
            }
            NPMotionState yHMotionState = new NPMotionState();
            yHMotionState.HomeMove = array[6];
            yHMotionState.Acc = array[2];
            yHMotionState.Dec = array[3];
            return yHMotionState;
        }

        private void AxisMoStatus_Convert(int MotionSts)
        {
            byte[] bytes = BitConverter.GetBytes(MotionSts);
            BitArray bitArray = new BitArray(bytes);
            bool[] array = new bool[32];
            for (int i = 0; i < bitArray.Length; i++)
            {
                array[i] = bitArray[i];
            }
            MState.HMV = array[6];
        }

        private void AxisIoStatus_Convert(int IoState)
        {
            byte[] bytes = BitConverter.GetBytes(IoState);
            BitArray bitArray = new BitArray(bytes);
            bool[] array = new bool[32];
            for (int i = 0; i < bitArray.Length; i++)
            {
                array[i] = bitArray[i];
            }
            IOState.ALM = array[0];
            IOState.PEL = array[1];
            IOState.MEL = array[2];
            IOState.ORG = array[3];
            IOState.EMG = array[4];
            IOState.SVON = array[7];
            IOState.SLP = array[11];
            IOState.SLN = array[12];
        }
    }
}

#else
namespace NPSDK
{
	internal class AdlinkNP_Motor : MotorBaseInterface
	{
	}
}
#endif