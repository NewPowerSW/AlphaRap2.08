using System;
using System.IO;
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
            LoadAlarmTable(SysPara.LanguageShow);
        }

        /// <summary>生成临时运行报警表交给 SDK，加载后释放临时文件。</summary>
        private static void LoadAlarmTable(LanguageType language)
        {
            string temporary = null;
            try
            {
                if (!File.Exists(AlarmCatalog.FilePath(language, true))) return;
                temporary = Path.GetTempFileName();
                AlarmCatalog.RuntimeTable(language).Save(temporary);
                Alarm.Initial(temporary);
            }
            catch (Exception error) { System.Diagnostics.Debug.WriteLine(error); }
            finally
            {
                try { if (temporary != null) File.Delete(temporary); }
                catch (Exception error) { System.Diagnostics.Debug.WriteLine(error); }
            }
        }
    }
}
