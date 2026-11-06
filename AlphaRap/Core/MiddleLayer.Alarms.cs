using NPSDK;
using NPSDK.Motion;

namespace AlphaRap
{
    partial class MiddleLayer
    {
        /// <summary>复位有报警的电机并清除框架报警记录。</summary>
        public static void AlarmClear()
        {
            foreach (ControlBaseInterface control in SDKPara.ControlList)
            {
                NP_Motor motor = control as NP_Motor;
                if (motor != null && motor.GetAxisIOState().ALM) motor.AlarmReset();
            }
            AlarmList.Clear();
            Alarm.Clear();
        }

        /// <summary>加载当前界面语言的报警文本，停机规则来自中文主表。</summary>
        private static void LoadAlarmTable()
        {
            AlarmCatalog.LoadRuntimeTable(SysPara.LanguageShow);
        }

    }
}
