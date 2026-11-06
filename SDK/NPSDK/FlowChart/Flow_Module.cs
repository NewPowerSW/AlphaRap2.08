using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using NPSDK;
using System.ComponentModel;
using System.Collections;

namespace NPSDK
{
	public static partial class Flow_Module
	{

		public static bool AllStop = false;
		public static bool AllPause = false;


		public static List<Flow_BaseForm> FlowChart_ModuleList = new List<Flow_BaseForm>();

		public static List<Flow_Chart> FlowChart_List = new List<Flow_Chart>();

		private static Queue FlowChart_RunLog = new Queue(2);
		private static Queue FlowChart_AlarmLog = new Queue(2);
		static Flow_Module()
		{

		}


		public static string Module_GetRunLog()
		{

			if (FlowChart_RunLog.Count > 0)
			{
				return (string)FlowChart_RunLog.Dequeue();
			}
			else
			{

				return null;

			}


		}
		public static string Module_GetAlarmLog()
		{

			if (FlowChart_AlarmLog.Count > 0)
			{
				return (string)FlowChart_AlarmLog.Dequeue();
			}
			else
			{

				return null;

			}


		}
		public static void Module_AddAlarmLog(string log)
		{
			if (string.IsNullOrEmpty(log))
				return;
			SetQueueSize(FlowChart_AlarmLog);
			FlowChart_AlarmLog.Enqueue(log);


		}
		public static void Module_AddRunLog(string log)
		{
			if (string.IsNullOrEmpty(log))
				return;
			SetQueueSize(FlowChart_RunLog);
			FlowChart_RunLog.Enqueue(log);


		}
		readonly static object obj = new object();
		private static void SetQueueSize(Queue gQueue)
		{
		
				if (gQueue.Count > 100)
				{
					gQueue.Dequeue();
					SetQueueSize(gQueue);

				}

			
			

		}
		public static void Module_InitialRun()
		{
			AllStop = false;
			AllPause = false;

			for (int i = 0; i < FlowChart_ModuleList.Count; i++)
			{
                if (FlowChart_ModuleList[i].TryProgramCommand(FlowProgramCommand.Initialize)) continue;
				if (FlowChart_ModuleList[i].RunningStatus && FlowChart_ModuleList[i].PauseStatus)
				{

					FlowChart_ModuleList[i].ContinusAftePause();
					FlowChart_ModuleList[i].PauseStatus = false;
					Module_AddRunLog(FlowChart_ModuleList[i].Name + "+++++++++++++++ContinusStart");

				}
				else
				{
					FlowChart_ModuleList[i].InitialStatus = false;
					FlowChart_ModuleList[i].PauseStatus = false;
					FlowChart_ModuleList[i].StopStatus = false;
					if (!FlowChart_ModuleList[i].RunningStatus)
					{
						FlowChart_ModuleList[i].RunningStatus = true;
						FlowChart_ModuleList[i].Initial();
						Module_AddRunLog(FlowChart_ModuleList[i].Name + "+++++++++++++++Initial");
					}
				}

			}
		}
		public static void Module_StartRun()
		{
			
			BackgroundWorker StartRun = new BackgroundWorker();
			StartRun.DoWork += new DoWorkEventHandler(Module_StartRunWork);
			if (!StartRun.IsBusy)
			{
				StartRun.RunWorkerAsync();
			}

		}
		private static void Module_StartRunWork(object sender, DoWorkEventArgs e)
		{
			AllStop = false;
			AllPause = false;
			for (int i = 0; i < FlowChart_ModuleList.Count; i++)
			{
                if (FlowChart_ModuleList[i].TryProgramCommand(FlowProgramCommand.Start)) continue;
				if (!FlowChart_ModuleList[i].RunningStatus)
				{
					Module_AddRunLog(FlowChart_ModuleList[i].Name + "+++++++++++++++Start");
					FlowChart_ModuleList[i].PauseStatus = false;
					FlowChart_ModuleList[i].StopStatus = false;

				}
				else if (FlowChart_ModuleList[i].RunningStatus && FlowChart_ModuleList[i].PauseStatus)
				{

					FlowChart_ModuleList[i].ContinusAftePause();
					FlowChart_ModuleList[i].PauseStatus = false;
					Module_AddRunLog(FlowChart_ModuleList[i].Name + "+++++++++++++++ContinusStart");

				}

			}
			Thread.Sleep(100);
			foreach(Flow_BaseForm item in FlowChart_ModuleList)
			{
                if (item.ProgramController != null && item.ProgramController.IsEnabled) continue;
				if(!item.RunningStatus)
				{
					item.RunningStatus = true;
					item.StartRun();
				}
			}
			
		}
		public static void Module_StopRun()
		{

			AllStop = true;
			for (int i = 0; i < FlowChart_ModuleList.Count; i++)
			{
				FlowChart_ModuleList[i].InitialStatus = false;
				FlowChart_ModuleList[i].StopStatus = true;
				FlowChart_ModuleList[i].RunningStatus = false;
				if (!FlowChart_ModuleList[i].TryProgramCommand(FlowProgramCommand.Stop)) FlowChart_ModuleList[i].StopRun();
				Module_AddRunLog(FlowChart_ModuleList[i].Name + "+++++++++++++++Stop");
			}
		}


		public static void Module_PauseRun()
		{
			AllPause = true;
			for (int i = 0; i < FlowChart_ModuleList.Count; i++)
			{

				FlowChart_ModuleList[i].PauseStatus = true;
				if (!FlowChart_ModuleList[i].TryProgramCommand(FlowProgramCommand.Pause)) FlowChart_ModuleList[i].PauseRun();
				Module_AddRunLog(FlowChart_ModuleList[i].Name + "+++++++++++++++Pause");
			}

		}

		public static bool Module_GetStopStatus
		{
			get
			{
				bool statuse = false;

				for (int i = 0; i < FlowChart_ModuleList.Count; i++)
				{
					statuse = statuse || FlowChart_ModuleList[i].StopStatus;

				}

				return statuse;
			}
		}
		public static bool Module_GetPauseStatus
		{
			get
			{
				bool statuse = false;

				for (int i = 0; i < FlowChart_ModuleList.Count; i++)
				{
					statuse = statuse || FlowChart_ModuleList[i].PauseStatus;

				}

				return statuse;
			}
		}
		public static bool Module_GetRunningStatus
		{
			get
			{
				bool statuse = false;

				for (int i = 0; i < FlowChart_ModuleList.Count; i++)
				{
					statuse = statuse || IsExecutionActive(FlowChart_ModuleList[i]);
				}

				for (int i = 0; i < FlowChart_List.Count; i++)
				{
					statuse = statuse || IsExecutionActive(FlowChart_List[i]);
				}

				return statuse;
			}
		}

		public static bool Module_GetInitialStatus
		{
			get
			{
				bool statuse = false;

				for (int i = 0; i < FlowChart_ModuleList.Count; i++)
				{
					statuse = statuse || FlowChart_ModuleList[i].InitialStatus;

				}
				return statuse;
			}
		}


	}
}
