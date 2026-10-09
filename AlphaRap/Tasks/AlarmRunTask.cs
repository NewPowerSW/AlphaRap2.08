using Sunny.UI;
using System;
using System.Threading;
using System.Windows.Forms;

namespace AlphaRap
{
	public class AlarmRunTask
	{
		Thread AlarmTask;
		public bool AlarmTaskIsRun = false;
		public AlarmRunTask()
		{
			AlarmTask = new Thread(AlarmRunWork);
			AlarmTask.Start();
			AlarmTask.IsBackground = true;
		}
		public void StopAlarmRunTask()
		{
			if (AlarmTask != null)
				AlarmTask.Join();
		}
		public void AlarmRunWork()
		{
			while (true)
			{
				AlarmRun();
				Thread.Sleep(10);
			}
		}
		public void AlarmRun()
		{
			if (!AlarmTaskIsRun)
				return;

			#region Alarm Message
			if (NPSDK.Alarm.DoRefresh)
			{
				MiddleLayer.MainF.dataBControl1.StartAlarmTime();
				NPSDK.Alarm.DoRefresh = false;
				MiddleLayer.MainF.WarnningMessage.BeginUpdate();
				MiddleLayer.MainF.WarnningMessage.Items.Clear();
				MiddleLayer.MainF.AlarmLogCache.Clear();
				for (int i = 0; i < NPSDK.Alarm.AlarmList.Count; i++)
				{
					NPSDK.Alarm.AlarmDataClass AlarmData = NPSDK.Alarm.AlarmList[i];
					// 显示文案按当前语言解析（见 HomeF.ResolveAlarmContent），日志文件写入原始内容
					string showContent = MiddleLayer.HomeF.ResolveAlarmContent(AlarmData.Code, AlarmData.Content);
					ListViewItem Alarm = new ListViewItem(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
					Alarm.SubItems.Add(AlarmData.Type);
					Alarm.SubItems.Add(AlarmData.Code);
					Alarm.SubItems.Add(showContent);

					#region AlarmLogFile
					string _RECIPEID = MiddleLayer.ProductF.GetRecipeValue("ProductSetting", "RECIPEID");
					string[] data = new string[8];
					data[0] = SysPara.UserName;
					data[1] = _RECIPEID;
					data[2] = SysPara.RecipeName;
					data[3] = (SysPara.SystemMode == RunMode.RUN || SysPara.SystemMode == RunMode.PAUSE) ? "Auto" : "Manual";
					data[4] = AlarmData.Type == "E" ? "ERROR" : "Information";
					data[5] = AlarmData.Code;
					data[6] = AlarmData.Content;
					// 写报警记录失败（如日志盘不存在）不能中断报警线程，否则后续报警都不会处理
					try
					{
						MiddleLayer.DataF.Alarm.SavePath = MiddleLayer.LogF.GetSettingValue("Path", "AlarmPath");
						MiddleLayer.DataF.Alarm.SaveDataToFile(data);
					}
					catch (Exception ex)
					{
						System.Diagnostics.Trace.WriteLine("Alarm log save failed: " + ex.Message);
					}
					#endregion

					// E 错误：暂停运行（副作用只在此处执行一次）
					if (AlarmData.Type == "E")
					{
						DateTime TWriteLogTime = DateTime.Now;
						MiddleLayer.PauseRun();
					}
					// 行配色：还原为最初版本（E=整行红底 / W=整行深鲑鱼色底，黑字）
					MainForm.ApplyAlarmRowColor(Alarm, AlarmData.Type, i);

					// 缓存原始数据，供「E / 其他报警」筛选时重建列表
					// （重建走缓存，不会再写一次日志文件、也不会重复 PauseRun）
					MiddleLayer.MainF.AlarmLogCache.Add(new string[] {
						Alarm.Text, AlarmData.Type, AlarmData.Code, showContent });

					// 按当前筛选条件决定是否显示
					if (MiddleLayer.MainF.IsAlarmRowVisible(AlarmData.Type))
						MiddleLayer.MainF.WarnningMessage.Items.Add(Alarm);
				}
				MiddleLayer.MainF.WarnningMessage.EndUpdate();
				MiddleLayer.MainF.UpdateAlarmFilterCount();
			}
			#endregion

			#region Log
			string Runlog = NPSDK.Flow_Module.Module_GetRunLog();
			if (!Runlog.IsNullOrEmpty())
			{
			MiddleLayer.DataF.AddRunLog(Runlog);
			}
			string Alarmlog = NPSDK.Flow_Module.Module_GetAlarmLog();
			if (!Alarmlog.IsNullOrEmpty())
			{
				MiddleLayer.DataF.AddLogError(Alarmlog);
			}
			#endregion
		}
	}
}
