using System;
using System.Drawing;
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
			 //MiddleLayer.HomeF.AlarmMessageLangLanguage(SysPara.LanguageShow);
			if (NPSDK.Alarm.DoRefresh)
			{
				MiddleLayer.MainF.dataBControl1.StartAlarmTime();
				NPSDK.Alarm.DoRefresh = false;
				MiddleLayer.MainF.WarnningMessage.BeginUpdate();
				MiddleLayer.MainF.WarnningMessage.Items.Clear();
				for (int i = 0; i < NPSDK.Alarm.AlarmList.Count; i++)
				{
					NPSDK.Alarm.AlarmDataClass AlarmData = NPSDK.Alarm.AlarmList[i];
					ListViewItem Alarm = new ListViewItem(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
					Alarm.SubItems.Add(AlarmData.Type);
					Alarm.SubItems.Add(AlarmData.Code);
					Alarm.SubItems.Add(AlarmData.Content);

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
					MiddleLayer.DataF.Alarm.SavePath = MiddleLayer.LogF.GetSettingValue("Path", "AlarmPath");
					MiddleLayer.DataF.Alarm.SaveDataToFile(data);
					#endregion

					switch (AlarmData.Type)
					{
						case "E":
							DateTime TWriteLogTime = DateTime.Now;
							MiddleLayer.PauseRun();
							Alarm.SubItems[0].BackColor = Color.Red;
							break;
						case "W":
							Alarm.SubItems[0].BackColor = Color.DarkSalmon;
							break;
					}
					MiddleLayer.MainF.WarnningMessage.Items.Add(Alarm);
				}
				MiddleLayer.MainF.WarnningMessage.EndUpdate();
				MiddleLayer.HomeF.WriteExcelData();

			}
			#endregion

			#region Log
			string Runlog = NPSDK.Flow_Module.Module_GetRunLog();
			if (Runlog != "")
			{
			MiddleLayer.DataF.AddRunLog(Runlog);

			}
			string Alarmlog = NPSDK.Flow_Module.Module_GetAlarmLog();
			if (Alarmlog != "")
			{
				MiddleLayer.DataF.AddLogError(Alarmlog);

			}
			#endregion

		}
	}

}
