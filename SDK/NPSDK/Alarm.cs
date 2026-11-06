using NPSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace NPSDK
{
	public static class Alarm
	{
		public struct AlarmDataClass
		{
			public string DateTime;

			public string Code;

			public string Type;

			public string Content;

			public string Cause;

			public string Solution;

			public string Group;

			public bool DoStop;

			
		}

	    private static List<AlarmDataClass> ArmList;

		public static List<AlarmDataClass> AlarmList;

		

		public static bool DoStop;

		public static bool DoRefresh;

		private static bool bInitialOk = false;

		public static bool IsError;

		public static bool IsWarning;

		public static bool IsInformation;

		public static bool Initial(string TablePath)
		{
			ArmList = new List<AlarmDataClass>();
			AlarmList = new List<AlarmDataClass>();
			return LoadFile(TablePath);
		}

		public static bool LoadFile(string TablePath)
		{
			bInitialOk = false;
			ArmList.Clear();
			AlarmList.Clear();
			XmlDocument xmlDocument = new XmlDocument();
			try
			{
				xmlDocument.Load(TablePath);
			}
			catch (Exception)
			{
				return bInitialOk;
			}
			XmlElement xmlElement = (XmlElement)xmlDocument.SelectSingleNode("AlarmTable");
			if (xmlElement != null)
			{
				XmlNodeList childNodes = xmlElement.ChildNodes;
				string text = "";
				for (int i = 0; i < childNodes.Count; i++)
				{
					AlarmDataClass item = default(AlarmDataClass);
					text = childNodes[i].Name;
					item.Code = text.Substring(1, text.Length - 1);
					item.Type = ((XmlElement)childNodes[i]).GetAttribute("Type");
					item.Content = ((XmlElement)childNodes[i]).GetAttribute("Content");
					item.Cause = ((XmlElement)childNodes[i]).GetAttribute("Cause");
					item.Solution = ((XmlElement)childNodes[i]).GetAttribute("Solution");
					item.Group = ((XmlElement)childNodes[i]).GetAttribute("Group");
					item.DoStop = Convert.ToBoolean(((XmlElement)childNodes[i]).GetAttribute("DoStop"));
					ArmList.Add(item);
				}
				bInitialOk = true;
			}
			return bInitialOk;
		}

		private static bool IsExistInSummary(string mCode, ref AlarmDataClass mArm)
		{
			for (int i = 0; i < ArmList.Count; i++)
			{
				AlarmDataClass alarmTableType = ArmList[i];
				if (alarmTableType.Code == mCode)
				{
					mArm = alarmTableType;
					return true;
				}
			}
			return false;
		}

		private static bool IsExistInUI(string mCode, string content)
		{
			for (int i = 0; i < AlarmList.Count; i++)
			{
				AlarmDataClass alarmTableType = AlarmList[i];
				string text = ((content == null) ? alarmTableType.Content : content);
				if (alarmTableType.Code == mCode && alarmTableType.Content == text)
				{
					return true;
				}
			}
			return false;
		}

		public static AlarmDataClass Show(string mCode, string content = null)
		{
			AlarmDataClass mArm = default(AlarmDataClass);
			if (!IsExistInSummary(mCode, ref mArm))
			{
				return mArm;
			}
			if (!IsExistInUI(mCode, content))
			{
				if (mArm.Type == "K" || mArm.Type == "k")
				{
					IsError = true;
				}
				if (mArm.Type == "E" || mArm.Type == "e")
				{
					IsError = true;
				}
				if (mArm.Type == "W" || mArm.Type == "w")
				{
					IsWarning = true;
				}
				if (mArm.Type == "I" || mArm.Type == "i")
				{
					IsInformation = true;
				}
				mArm.DateTime = DateTime.Now.ToString("G");
				if (content != null)
				{
					mArm.Content = ((content == null) ? mArm.Content : content);
				}
				AlarmList.Add(mArm);
				
				DoRefresh = true;
			}
			if (mArm.DoStop)
			{
				DoStop = mArm.DoStop;
				if(DoRefresh)
				Flow_Module.Module_PauseRun();
			}
			return mArm;
		}

		public static AlarmDataClass Show(string mCode, string content, bool DoStop, string Type)
		{
			AlarmDataClass mArm = default(AlarmDataClass);
			if (!IsExistInSummary(mCode, ref mArm))
			{
				mArm.Code = mCode;
				mArm.Content = content;
				mArm.DoStop = DoStop;
				mArm.Type = Type;
				ArmList.Add(mArm);
			}
			if (!IsExistInUI(mCode, content))
			{
				if (mArm.Type == "K" || mArm.Type == "k")
				{
					IsError = true;
				}
				if (mArm.Type == "E" || mArm.Type == "e")
				{
					IsError = true;
				}
				if (mArm.Type == "W" || mArm.Type == "w")
				{
					IsWarning = true;
				}
				if (mArm.Type == "I" || mArm.Type == "i")
				{
					IsInformation = true;
				}
				mArm.DateTime = DateTime.Now.ToString("G");
				if (content != null)
				{
					mArm.Content = ((content == null) ? mArm.Content : content);
				}
				AlarmList.Add(mArm);
				DoRefresh = true;
			}
			if (mArm.DoStop)
			{
				DoStop = mArm.DoStop;
				if (DoRefresh)
					Flow_Module.Module_PauseRun();
			}
			return mArm;
		}
		
		public static void Clear()
		{
			IsError = false;
			IsInformation = false;
			IsWarning = false;
			AlarmList.Clear();
			DoRefresh = true;
		}

		public static void ClearAlarm(string alarmCode)
		{
			List<AlarmDataClass> list = AlarmList.FindAll((AlarmDataClass a) => a.Code == alarmCode);
			foreach (AlarmDataClass alarm in list)
			{
				if (AlarmList.Count((AlarmDataClass a) => a.Type == alarm.Type) <= 1)
				{
					switch (alarm.Type)
					{
					case "W":
						IsWarning = false;
						break;
					case "E":
						IsError = false;
						break;
					case "I":
						IsInformation = false;
						break;
					}
				}
				AlarmList.Remove(alarm);
			}
			if (list.Count > 0)
			{
				DoRefresh = true;
			}
		}
	}
}
