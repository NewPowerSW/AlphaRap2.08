using System;

namespace NPSDK.Motion
{
    public partial class NP_Motor
    {
        /// <summary>按独立手动速度发送定位，不改自动速度、比例和回零参数。</summary>
        internal bool ManualGoto(double position, double speed)
        {
            return ExecuteManual(speed, () => dMotor.Goto(position));
        }

        /// <summary>按独立手动速度发送点动，不改自动速度和比例。</summary>
        internal bool ManualJog(bool forward, double speed)
        {
            return ExecuteManual(speed, () => { if (forward) dMotor.JogP(); else dMotor.JogN(); return true; });
        }

        /// <summary>手动回零的高速和低速均受当前手动速度约束，保留原回零配置。</summary>
        internal bool ManualHome(double speed)
        {
            return ExecuteManual(speed, () => dMotor.Home());
        }

        /// <summary>只在单次驱动调用期间使用手动参数，异常时也恢复驱动内存中的自动参数。</summary>
        private bool ExecuteManual(double speed, Func<bool> action)
        {
            lock (this)
            {
                if (!IsReady || !FlowResourceGate.CanAccess(this)) return false;
                if (double.IsNaN(manualSpeed) || double.IsInfinity(manualSpeed) || double.IsNaN(maxSpeed) || double.IsInfinity(maxSpeed)) return false;
                if (double.IsNaN(speed) || double.IsInfinity(speed) || speed <= 0 || speed > manualSpeed || speed > maxSpeed) return false;
                double savedSpeed = dMotor.WorkSpeed, savedRatio = dMotor.SpeedRatio;
                double savedHigh = dMotor.HomeHighSpd, savedLow = dMotor.HomeLowSpd;
                try
                {
                    dMotor.WorkSpeed = speed;
                    dMotor.SpeedRatio = 100;
                    dMotor.HomeHighSpd = Math.Min(savedHigh, speed);
                    dMotor.HomeLowSpd = Math.Min(savedLow, dMotor.HomeHighSpd);
                    return action();
                }
                finally
                {
                    dMotor.WorkSpeed = savedSpeed;
                    dMotor.SpeedRatio = savedRatio;
                    dMotor.HomeHighSpd = savedHigh;
                    dMotor.HomeLowSpd = savedLow;
                }
            }
        }
    }
}
