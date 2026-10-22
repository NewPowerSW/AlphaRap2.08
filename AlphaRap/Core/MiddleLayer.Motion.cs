using System;
using System.Collections.Generic;
using NPSDK;
using NPSDK.Motion;

namespace AlphaRap
{
    partial class MiddleLayer
    {
        private static readonly object ConveyorSync = new object();
        private static readonly Dictionary<NP_Output, bool> ConveyorRunning = new Dictionary<NP_Output, bool>();

        /// <summary>打开全部模块的伺服，模块失败时报警并停止生产。</summary>
        public static void ServoOn()
        {
            ExecuteModules(module => module.ServoOn(), "ServoOn", "2011", true);
        }

        /// <summary>关闭全部模块的伺服，模块失败时报警并停止生产。</summary>
        public static void ServoOff()
        {
            ExecuteModules(module => module.ServoOff(), "ServoOff", "2012", true);
        }

        /// <summary>停止已登记的全部电机，不缓存可能变化的设备列表。</summary>
        public static void StopAllMotor()
        {
            foreach (ControlBaseInterface control in SDKPara.ControlsSnapshot())
            {
                NP_Motor motor = control as NP_Motor;
                if (motor != null) lock (motor) { if (!motor.IsDisposed) motor.Stop(); }
            }
        }

        /// <summary>恢复暂停前运行的输送线，成功恢复后清除本次快照。</summary>
        public static void StartRunCV()
        {
            lock (ConveyorSync)
            {
                foreach (KeyValuePair<NP_Output, bool> conveyor in ConveyorRunning)
                    if (conveyor.Value) conveyor.Key.On();
                ConveyorRunning.Clear();
            }
        }

        /// <summary>记录输送线暂停前的状态并关闭输出，重复暂停不会覆盖首次状态。</summary>
        public static void StopRunCV()
        {
            if (ManualF == null) return;
            lock (ConveyorSync)
            {
                foreach (NP_Output conveyor in ManualF.Conveyor_List)
                {
                    if (conveyor == null) continue;
                    try
                    {
                        if (!ConveyorRunning.ContainsKey(conveyor)) ConveyorRunning.Add(conveyor, conveyor.IsOn());
                        conveyor.Off();
                    }
                    catch (Exception error) { System.Diagnostics.Debug.WriteLine(error); }
                }
            }
        }

        /// <summary>检查每根电机的报警、急停与限位，仿真时不读取设备。</summary>
        public static void CheckMotorProtected()
        {
            if (SysPara.Simulation) return;
            foreach (ControlBaseInterface control in SDKPara.ControlsSnapshot())
            {
                NP_Motor motor = control as NP_Motor;
                if (motor == null) continue;
                AxisIOState state;
                lock (motor) { if (motor.IsDisposed) continue; state = motor.GetAxisIOState(); }
                if (state.ALM) Alarm.Show("2030", string.Format(LangMsg("MiddleLayer", "msg_MotorAlarm",
                    "电机「{0}」报警。", "Motor \"{0}\" has an alarm.", "El motor «{0}» tiene una alarma."), motor.Name));
                if (state.EMG) Alarm.Show("2031", string.Format(LangMsg("MiddleLayer", "msg_MotorEmergency",
                    "电机「{0}」急停信号已触发。", "Motor \"{0}\" has an active emergency stop signal.", "El motor «{0}» tiene activa la señal de parada de emergencia."), motor.Name));
                if (state.SLN || state.SLP || state.MEL || state.PEL) Alarm.Show("2032", string.Format(LangMsg("MiddleLayer", "msg_MotorLimit",
                    "电机「{0}」触发限位。", "Motor \"{0}\" has reached a limit sensor.", "El motor «{0}» ha alcanzado un sensor de límite."), motor.Name));
            }
        }
    }
}
