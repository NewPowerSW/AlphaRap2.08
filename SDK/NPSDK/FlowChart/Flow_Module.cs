using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace NPSDK
{
	public static partial class Flow_Module
	{

		private static readonly object commandSync = new object();
		private static bool dispatchingStart;
		private static bool dispatchingStop;
		private static bool startStopped;
		private static Flow_BaseForm deferredStop;
		private static readonly object logSync = new object();

		public static volatile bool AllStop = false;
		public static volatile bool AllPause = false;


		public static List<Flow_BaseForm> FlowChart_ModuleList = new List<Flow_BaseForm>();

		public static List<Flow_Chart> FlowChart_List = new List<Flow_Chart>();

		private static Queue FlowChart_RunLog = new Queue(2);
		private static Queue FlowChart_AlarmLog = new Queue(2);
		/// <summary>线程安全地取出一条运行日志，无日志时返回 null。</summary>
		public static string Module_GetRunLog()
		{

			lock (logSync) return FlowChart_RunLog.Count == 0 ? null : (string)FlowChart_RunLog.Dequeue();


		}
		/// <summary>线程安全地取出一条报警日志，无日志时返回 null。</summary>
		public static string Module_GetAlarmLog()
		{

			lock (logSync) return FlowChart_AlarmLog.Count == 0 ? null : (string)FlowChart_AlarmLog.Dequeue();


		}
		/// <summary>向有界报警队列追加非空日志。</summary>
		public static void Module_AddAlarmLog(string log)
		{
			EnqueueLog(FlowChart_AlarmLog, log);


		}
		/// <summary>向有界运行队列追加非空日志。</summary>
		public static void Module_AddRunLog(string log)
		{
			EnqueueLog(FlowChart_RunLog, log);


		}
		/// <summary>将日志和容量限制作为一次操作，允许后台派发与停止并发写入。</summary>
		private static void EnqueueLog(Queue queue, string log)
		{
			if (string.IsNullOrEmpty(log)) return;
			lock (logSync)
			{
				if (queue.Count > 100) queue.Dequeue();
				queue.Enqueue(log);
			}
		}
		/// <summary>从调用线程初始化工位，停止会撤销尚未派发的工位。</summary>
		public static void Module_InitialRun()
		{
			if (TryReserveStart()) DispatchStart(FlowProgramCommand.Initialize);
		}

		/// <summary>在后台派发一次启动或恢复命令，重复请求共用当前派发。</summary>
		public static void Module_StartRun()
		{
			if (!TryReserveStart()) return;
			_ = Task.Run(() =>
			{
				try { DispatchStart(FlowProgramCommand.Start); }
				catch (Exception error)
				{
					Module_AddRunLog("Flow start failed: " + error);
					Trace.TraceError(error.ToString());
				}
			});
		}

		/// <summary>先占用派发位置，再提交后台任务，避免停止后旧任务重新取得启动资格。</summary>
		private static bool TryReserveStart()
		{
			lock (commandSync)
			{
				if (dispatchingStart || dispatchingStop) return false;
				if (AllStop && Flow_Chart.HasActiveLegacyRuns) return false;
				dispatchingStart = true;
				startStopped = false;
				return true;
			}
		}

		/// <summary>优先派发控制器命令，再启动未托管的旧工位。</summary>
		private static void DispatchStart(FlowProgramCommand command)
		{
			try
			{
				lock (commandSync)
				{
					if (startStopped) return;
					AllStop = false;
					AllPause = false;
				}

				List<Flow_BaseForm> legacyStarts = new List<Flow_BaseForm>();
				foreach (Flow_BaseForm module in FlowChart_ModuleList.ToArray())
				{
					if (StartWasStopped()) return;
					if (module.IsDisposed) continue;
					bool handled = module.TryProgramCommand(command);
					if (!CompleteStartCallback(module)) return;
					if (handled) continue;
					if (command == FlowProgramCommand.Start && module.ProgramController?.IsEnabled == true) continue;
					if (!PrepareLegacyStart(module, command, out bool resume))
					{
						if (resume && !ResumeLegacyModule(module)) return;
						continue;
					}
					if (command == FlowProgramCommand.Initialize)
					{
						if (!StartLegacyModule(module, command)) return;
						continue;
					}
					legacyStarts.Add(module);
				}

				foreach (Flow_BaseForm module in legacyStarts)
				{
					if (!StartLegacyModule(module, command)) return;
				}
			}
			catch (Exception startError)
			{
				try { Module_StopRun(); }
				catch (Exception stopError) { throw new AggregateException(startError, stopError); }
				throw;
			}
			finally
			{
				lock (commandSync) dispatchingStart = false;
			}
		}

		/// <summary>清除可启动旧工位的旧标记，保留正在运行的工位及其恢复入口。</summary>
		private static bool PrepareLegacyStart(Flow_BaseForm module, FlowProgramCommand command, out bool resume)
		{
			lock (commandSync)
			{
				resume = false;
				if (startStopped) return false;
				resume = module.RunningStatus && module.PauseStatus;
				if (resume) return false;
				if (command == FlowProgramCommand.Initialize) module.InitialStatus = false;
				if (command == FlowProgramCommand.Initialize || !module.RunningStatus)
				{
					module.PauseStatus = false;
					module.StopStatus = false;
				}
				return !module.RunningStatus;
			}
		}

		/// <summary>旧工位启动前复核资格，业务回调在命令锁外执行。</summary>
		private static bool StartLegacyModule(Flow_BaseForm module, FlowProgramCommand command)
		{
			lock (commandSync)
			{
				if (startStopped) return false;
				if (module.IsDisposed || module.RunningStatus) return true;
				module.RunningStatus = true;
			}
			if (command == FlowProgramCommand.Initialize) module.Initial();
			else module.StartRun();
			if (!CompleteStartCallback(module)) return false;
			Module_AddRunLog(module.Name + "+++++++++++++++" + (command == FlowProgramCommand.Initialize ? "Initial" : "Start"));
			return true;
		}

		/// <summary>恢复正在暂停的旧工位，保留并发停止的标记。</summary>
		private static bool ResumeLegacyModule(Flow_BaseForm module)
		{
			if (StartWasStopped()) return false;
			module.ContinusAftePause();
			if (!CompleteStartCallback(module)) return false;
			lock (commandSync)
			{
				if (startStopped) return false;
				module.PauseStatus = false;
			}
			Module_AddRunLog(module.Name + "+++++++++++++++ContinusStart");
			return true;
		}

		/// <summary>读取当前启动或初始化的撤销状态。</summary>
		private static bool StartWasStopped()
		{
			lock (commandSync) return startStopped;
		}

		/// <summary>在途回调与停止交错时，回调退出后对该工位补发停止。</summary>
		private static bool CompleteStartCallback(Flow_BaseForm module)
		{
			lock (commandSync)
			{
				if (!startStopped) return true;
				if (dispatchingStop)
				{
					deferredStop = module;
					return false;
				}
				dispatchingStop = true;
			}
			try { StopModule(module); }
			finally { lock (commandSync) dispatchingStop = false; }
			return false;
		}

		/// <summary>先撤销当前派发，再立即请求所有工位停止。</summary>
		public static void Module_StopRun()
		{
			lock (commandSync)
			{
				AllStop = true;
				startStopped = true;
				if (dispatchingStop) return;
				dispatchingStop = true;
			}

			Flow_Chart.StopLegacyRuns();
			List<Exception> errors = new List<Exception>();
			try
			{
				foreach (Flow_BaseForm module in FlowChart_ModuleList.ToArray())
				{
					try { StopModule(module); }
					catch (Exception error) { errors.Add(error); }
				}
			}
			finally
			{
				CompleteStopDispatch(errors);
			}
			if (errors.Count > 0) throw new AggregateException(errors);
		}

		/// <summary>停止派发期间退出的在途启动回调，在原停止回调之后收尾。</summary>
		private static void CompleteStopDispatch(List<Exception> errors)
		{
			Flow_BaseForm module;
			lock (commandSync)
			{
				module = deferredStop;
				deferredStop = null;
				if (module == null) dispatchingStop = false;
			}
			if (module == null) return;
			try { StopModule(module); }
			catch (Exception error) { errors.Add(error); }
			finally { lock (commandSync) dispatchingStop = false; }
		}

		/// <summary>将停止转给单个工位的控制器或旧回调。</summary>
		private static void StopModule(Flow_BaseForm module)
		{
			if (module.IsDisposed) return;
			module.InitialStatus = false;
			module.StopStatus = true;
			module.RunningStatus = false;
			if (!module.TryProgramCommand(FlowProgramCommand.Stop)) module.StopRun();
			Module_AddRunLog(module.Name + "+++++++++++++++Stop");
		}

		/// <summary>暂停所有工位，保留初始化故障后续跑的控制器行为。</summary>
		public static void Module_PauseRun()
		{
			lock (commandSync) AllPause = true;
			foreach (Flow_BaseForm module in FlowChart_ModuleList.ToArray())
			{
				if (module.IsDisposed) continue;
				module.PauseStatus = true;
				if (!module.TryProgramCommand(FlowProgramCommand.Pause)) module.PauseRun();
				Module_AddRunLog(module.Name + "+++++++++++++++Pause");
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
				lock (commandSync) if (dispatchingStart || dispatchingStop) return true;
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
