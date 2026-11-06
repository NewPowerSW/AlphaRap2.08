using System;
using System.Collections.Generic;
using System.Threading;
using NPSDK;

namespace AlphaRap
{
    /// <summary>报警模块发布给宿主的显示记录。</summary>
    public sealed class AlarmDisplayRow
    {
        public DateTime Time { get; }
        public string Type { get; }
        public string Code { get; }
        public string Content { get; }

        /// <summary>保留原始报警内容供不同语言的窗口显示。</summary>
        internal AlarmDisplayRow(DateTime time, Alarm.AlarmDataClass alarm)
        {
            Time = time;
            Type = alarm.Type;
            Code = alarm.Code;
            Content = alarm.Content;
        }
    }

    /// <summary>采集报警与流程日志，由订阅方显示报警快照。</summary>
    public class AlarmRunTask
    {
        private readonly Thread alarmThread;
        private volatile bool stopping;
        public volatile bool AlarmTaskIsRun;

        /// <summary>启动报警模块的后台采集线程。</summary>
        public AlarmRunTask()
        {
            alarmThread = new Thread(AlarmRunWork) { IsBackground = true };
            alarmThread.Start();
        }

        /// <summary>停止采集并等待线程退出。</summary>
        public void StopAlarmRunTask()
        {
            stopping = true;
            if (alarmThread != null && alarmThread != Thread.CurrentThread) alarmThread.Join();
        }

        /// <summary>周期采集报警及流程日志。</summary>
        public void AlarmRunWork()
        {
            while (!stopping)
            {
                AlarmRun();
                Thread.Sleep(10);
            }
        }

        /// <summary>处理一次报警更新，并写出新增流程日志。</summary>
        public void AlarmRun()
        {
            if (!AlarmTaskIsRun) return;
            if (Alarm.DoRefresh) RefreshAlarms();

            string runLog = Flow_Module.Module_GetRunLog();
            if (!string.IsNullOrEmpty(runLog)) ApplicationHost.Data.AddRunLog(runLog);
            string alarmLog = Flow_Module.Module_GetAlarmLog();
            if (!string.IsNullOrEmpty(alarmLog)) ApplicationHost.Data.AddLogError(alarmLog);
        }

        /// <summary>记录报警，执行停机规则并向宿主发布本次快照。</summary>
        private static void RefreshAlarms()
        {
            Alarm.DoRefresh = false;
            var rows = new List<AlarmDisplayRow>();
            foreach (Alarm.AlarmDataClass alarm in Alarm.AlarmList.ToArray())
            {
                rows.Add(new AlarmDisplayRow(DateTime.Now, alarm));
                SaveAlarm(alarm);
                if (alarm.Type == "E") ApplicationHost.PauseMachine();
            }

            ApplicationHost.PublishAlarms(rows.AsReadOnly());
        }

        /// <summary>按既有列写入报警原文。</summary>
        private static void SaveAlarm(Alarm.AlarmDataClass alarm)
        {
            string[] data = new string[8];
            data[0] = SysPara.UserName;
            data[1] = ApplicationHost.FindModule<ProductManagerForm>().GetRecipeValue("ProductSetting", "RECIPEID");
            data[2] = SysPara.RecipeName;
            data[3] = SysPara.SystemMode == RunMode.RUN || SysPara.SystemMode == RunMode.PAUSE ? "Auto" : "Manual";
            data[4] = alarm.Type == "E" ? "ERROR" : "Information";
            data[5] = alarm.Code;
            data[6] = alarm.Content;
            ApplicationHost.Data.WriteLog("Alarm", data);
        }
    }
}
