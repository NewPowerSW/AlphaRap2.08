using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlphaRapLibrary;
using NPSDK;
using static AlphaRap.FrameworkLanguage;

namespace AlphaRap
{
    partial class MiddleLayer
    {
        private static readonly object machineCommandSync = new object();
        private static readonly MachineStopCoordinator machineStop = new MachineStopCoordinator();
        private static long machineCommandVersion;
        private static bool machineCommandActive;
        private static bool machinePauseCommand;
        private static Task startupWork = Task.FromResult(0);
        private static Task manualStopWork = Task.FromResult(0);
        private static Task<bool> stopCompletion = Task.FromResult(true);

        public static bool MachineInitialReady { get { return FlowProgramming.FlowPrograms.HasPrograms ? FlowProgramming.FlowPrograms.IsInitialized : Application.IsLegacyInitialized; } }
        public static bool MachineCommandPending { get { lock (machineCommandSync) return machineCommandActive; } }
        public static bool MachineIsStopping { get { return machineStop.IsStopping || machineStop.Error != null || SysPara.SystemMode == RunMode.STOPPING; } }
        public static bool MachineStopWaiting { get { return machineStop.IsWaiting; } }
        public static string MachineStopError { get { return machineStop.Error; } }
        public static string[] PendingStopOperations { get { return machineStop.PendingOperations; } }
        public static Task<bool> MachineStopCompletion { get { lock (machineCommandSync) return stopCompletion; } }
        public static Task<bool> MachineCommandCompletion { get; private set; } = Task.FromResult(true);

        /// <summary>沿用原初始化入口，异步执行并由统一命令任务观察结果。</summary>
        public static void Initial() { _ = InitialAsync(); }

        /// <summary>停止旧会话并确认退出后初始化；重复请求不会再次派发。</summary>
        public static Task<bool> InitialAsync() { return QueueMachineCommand(InitializeMachine); }

        /// <summary>初始化暂停时继续同一会话，保留已完成工位和节点结果。</summary>
        private static bool ContinueInitializationIfActive(long version)
        {
            try
            {
                lock (machineCommandSync)
                {
                    if (version != machineCommandVersion || !FlowProgramming.FlowPrograms.HasInitializationSession) return false;
                    FlowProgramming.FlowPrograms.ContinueInitialization();
                    return true;
                }
            }
            catch (Exception error) { MessageBox.Show(error.Message, FlowProgramming.FlowText.T("title")); return false; }
        }

        /// <summary>沿用原启动入口，异步执行并保留可等待的派发结果。</summary>
        public static void StartRun() { _ = StartRunAsync(); }

        /// <summary>启动自动流程或恢复同一会话，派发失败不提前标记运行成功。</summary>
        public static Task<bool> StartRunAsync() { return QueueMachineCommand(StartMachine); }

        /// <summary>同步预留一次命令并生成代次，停止会使旧命令的后续步骤失效。</summary>
        private static Task<bool> QueueMachineCommand(Func<long, Task<bool>> command, bool pausing = false)
        {
            long version;
            TaskCompletionSource<bool> finished;
            IDisposable deviceAdmission;
            lock (machineCommandSync)
            {
                if (!recipeReady || MachineIsStopping || ApplicationHost.IsShuttingDown || Configuration.ProjectConfiguration.IsBusy || Configuration.ProjectConfiguration.RecoveryRequired) return Task.FromResult(false);
                if (machineCommandActive)
                {
                    if (pausing && !machinePauseCommand) StopRun();
                    return Task.FromResult(false);
                }
                machineCommandActive = true;
                machinePauseCommand = pausing;
                version = ++machineCommandVersion;
                finished = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                MachineCommandCompletion = finished.Task;
                deviceAdmission = DeviceOperations.SuspendNewOperations();
            }
            _ = ExecuteMachineCommand(command, version, finished, deviceAdmission);
            return finished.Task;
        }

        /// <summary>执行一次整机命令，隔离错误并在失败后请求完整停止。</summary>
        private static async Task ExecuteMachineCommand(Func<long, Task<bool>> command, long version, TaskCompletionSource<bool> finished, IDisposable deviceAdmission)
        {
            bool accepted = false;
            try { accepted = await command(version); }
            catch (Exception error)
            {
                RecordControlFailure(error);
                if (IsCurrentCommand(version))
                {
                    StopRun();
                    try
                    {
                        Application.ShowWarning(string.Format(LangMsg("MiddleLayer", "msg_CommandFailed",
                            "运行命令执行失败：{0}", "Machine command failed: {0}", "Error al ejecutar el comando: {0}"), error.Message),
                            LangMsg("Common", "msg_OK", "确定", "OK", "Aceptar"));
                    }
                    catch (Exception warningError) { RecordControlFailure(warningError); }
                }
            }
            finally
            {
                lock (machineCommandSync) { deviceAdmission.Dispose(); machineCommandActive = false; machinePauseCommand = false; }
                finished.TrySetResult(accepted);
            }
        }

        /// <summary>沿用初始化预检与恢复规则，清理完成后再创建新的流程批次。</summary>
        private static async Task<bool> InitializeMachine(long version)
        {
            if (FlowProgramming.FlowPrograms.HasInitializationSession)
                return await CheckSafetyInputs(version) && ContinueInitializationIfActive(version);
            if (Alarm.IsError) { ShowResetAlarmWarning(); return false; }
            bool paused = SysPara.SystemMode == RunMode.PAUSE;
            bool wasRunning = SysPara.SystemMode == RunMode.RUN;
            if (!FlowProgramming.FlowPrograms.AnyRunning && !FlowProgramming.FlowPrograms.CanStartMachine(true)) return false;
            if (SysPara.SystemMode == RunMode.RUN)
            {
                DialogResult choice = MessageBox.Show(
                    LangMsg("MiddleLayer", "msg_Reinitialize", "正在自动生产，确认退出自动生产并执行初始化吗？", "Exit automatic production and initialize?", "¿Salir de producción automática e inicializar?"),
                    LangMsg("MiddleLayer", "msg_InitializeTitle", "初始化", "Initialize", "Inicializar"),
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (choice != DialogResult.OK) return false;
            }
            else if (FlowProgramming.FlowPrograms.AnyRunning && !paused) return false;
            if (!IsCurrentCommand(version)) return false;
            if (!await BeginStop(version, true, wasRunning || paused)) return false;
            if (!IsCurrentCommand(version) || !FlowProgramming.FlowPrograms.CanStartMachine(true)) return false;
            if (paused && !await RunStartupWork(version, Alarm.Clear)) return false;
            if (Alarm.IsError) { ShowResetAlarmWarning(); return false; }
            TaskProcess.StationIo io = Application is StationApplication ? StationApplication.Io : null;
            if (io != null && !await RunStartupWork(version, io.Initialize)) return false;
            if (!await CheckSafetyInputs(version)) return false;
            if (FlowProgramming.FlowPrograms.HasPrograms) FlowProgramming.FlowPrograms.BeginBatch();
            lock (machineCommandSync)
            {
                if (version != machineCommandVersion) return false;
                MachineState.Global.Initialize();
            }
            return await DispatchStart(version, true);
        }

        /// <summary>区分恢复和首次自动运行，设备复位及输出恢复均可被停止撤销。</summary>
        private static async Task<bool> StartMachine(long version)
        {
            bool resume = SysPara.SystemRun && SysPara.SystemMode == RunMode.PAUSE;
            if (!resume && (SysPara.SystemRun || SysPara.SystemMode != RunMode.INITIAL || !MachineInitialReady)) return false;
            if (!FlowProgramming.FlowPrograms.CanStartMachine(false) || AnyManualRunning()) return false;
            if (!await CheckSafetyInputs(version)) return false;
            if (!await RunStartupWork(version, AlarmClear)) return false;
            if (Alarm.IsError) { ShowResetAlarmWarning(); return false; }
            if (!resume && !FlowProgramming.FlowPrograms.HasPrograms && !await BeginStop(version, false, false)) return false;
            if (!await DispatchStart(version, false)) return false;
            if (Alarm.IsError) { StopRun(); return false; }
            if (resume && !await RunStartupWork(version, Application.ResumeLegacyOutputs)) return false;
            lock (machineCommandSync)
            {
                if (version != machineCommandVersion) return false;
                if (Alarm.IsError) { StopRun(); return false; }
                MachineState.Global.Start(MachineInitialReady);
            }
            return true;
        }

        /// <summary>在后台读取现场运行条件，命令仍有效时在界面提示阻止原因。</summary>
        private static async Task<bool> CheckSafetyInputs(long version)
        {
            if (!(Application is StationApplication station) || SysPara.Simulation) return true;
            string reason = null;
            if (!await RunStartupWork(version, () => reason = station.SafetyBlockReason())) return false;
            if (reason == null) return true;
            Application.ShowWarning(reason, LangMsg("Common", "msg_OK", "确定", "OK", "Aceptar"));
            return false;
        }

        /// <summary>在命令锁内核对代次并预留 SDK 派发，避免停止后旧命令重新入队。</summary>
        private static Task<bool> DispatchStart(long version, bool initialize)
        {
            lock (machineCommandSync)
            {
                if (version != machineCommandVersion) return Task.FromResult(false);
                return initialize ? Flow_Module.Module_InitialRunAsync() : Flow_Module.Module_StartRunAsync();
            }
        }

        /// <summary>设备准备工作在独立线程执行，停止会等待其退出且阻止后续旧步骤。</summary>
        private static async Task<bool> RunStartupWork(long version, Action work)
        {
            Task pending;
            lock (machineCommandSync)
            {
                if (version != machineCommandVersion) return false;
                pending = Task.Factory.StartNew(work, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                startupWork = pending;
            }
            await pending;
            return IsCurrentCommand(version);
        }

        /// <summary>确认当前异步命令仍拥有派发后续动作的资格。</summary>
        private static bool IsCurrentCommand(long version)
        {
            lock (machineCommandSync) return version == machineCommandVersion;
        }

        /// <summary>暂停当前流程；停止期间不会恢复旧输出或更改停止状态。</summary>
        public static void PauseRun() { _ = QueueMachineCommand(PauseMachine, true); }

        /// <summary>在后台派发暂停及旧输出处理，停止可以撤销尚未发送的输出操作。</summary>
        private static async Task<bool> PauseMachine(long version)
        {
            bool pauseOutputs;
            lock (machineCommandSync)
            {
                if (version != machineCommandVersion) return false;
                pauseOutputs = SysPara.SystemMode == RunMode.RUN
                    || (SysPara.SystemMode == RunMode.INITIAL && !MachineInitialReady);
                if (pauseOutputs) MachineState.Global.Pause();
            }
            if (!await RunStartupWork(version, Flow_Module.Module_PauseRun)) return false;
            if (!await RunStartupWork(version, () => DeviceOperations.RequestStopAllAsync().GetAwaiter().GetResult())) return false;
            if (!FlowProgramming.FlowPrograms.HasPrograms)
            {
                Task manual;
                lock (machineCommandSync)
                {
                    if (version != machineCommandVersion) return false;
                    manual = manualStopWork = StartManualStops();
                }
                await manual;
                if (!IsCurrentCommand(version)) return false;
            }
            if (pauseOutputs && !FlowProgramming.FlowPrograms.HasPrograms)
                return await RunStartupWork(version, Application.PauseLegacyOutputs);
            return IsCurrentCommand(version);
        }

        /// <summary>运行条件消失时停止活动会话，待机或正在停止时不重复派发。</summary>
        internal static void StopForSafetyFault()
        {
            lock (machineCommandSync)
            {
                RunMode mode = MachineState.Global.Current.Mode;
                if (mode == RunMode.INITIAL || mode == RunMode.RUN || mode == RunMode.PAUSE) StopRun();
            }
        }

        /// <summary>立即撤销旧命令，后台派发全部停止步骤并保留可等待的完成任务。</summary>
        public static void StopRun()
        {
            lock (machineCommandSync)
            {
                long version = ++machineCommandVersion;
                stopCompletion = BeginStop(version, true);
            }
        }

        /// <summary>统一停止流程、手动任务、准备工作和电机，实际完成后才允许待机。</summary>
        private static Task<bool> BeginStop(long version, bool resetInitialization, bool stopHardware = true)
        {
            lock (machineCommandSync)
            {
                if (version != machineCommandVersion) return Task.FromResult(false);
                MachineState.Global.BeginStop();
                Flow_Module.Module_RequestStop();
                Task deviceCleanup = DeviceOperations.RequestStopAllAsync();
                Task<bool> pending = machineStop.RequestStop(CreateStopSteps(stopHardware, deviceCleanup),
                    resetInitialization ? (Action)Application.ResetLegacyInitialization : null);
                stopCompletion = ApplyStopResult(pending, version);
                return stopCompletion;
            }
        }

        /// <summary>为每个停止对象建立独立步骤，单个设备阻塞不影响其他停止请求。</summary>
        private static IEnumerable<MachineStopStep> CreateStopSteps(bool stopHardware, Task deviceCleanup)
        {
            Task preparation = startupWork;
            Task manualCleanup = manualStopWork;
            bool flowWasRunning = FlowProgramming.FlowPrograms.AnyRunning;
            yield return new MachineStopStep("Flow", Flow_Module.Module_StopRun,
                () => FlowProgramming.FlowPrograms.AnyRunning, () => flowWasRunning ? FlowProgramming.FlowPrograms.StopError : null);
            yield return new MachineStopStep("Startup", () => { }, () => !preparation.IsCompleted);
            yield return new MachineStopStep("ManualCleanup", () => { }, () => !manualCleanup.IsCompleted);
            yield return new MachineStopStep("Devices", () => deviceCleanup.GetAwaiter().GetResult(), () => DeviceOperations.HasPending);
            foreach (ModuleBaseForm module in ModuleManager.ModuleList.ToArray())
            {
                yield return new MachineStopStep("Manual/" + module.Name, () => module.RequestManualStop().GetAwaiter().GetResult(), module.IsManualRun, identity: module);
            }
            if (stopHardware)
            {
                yield return new MachineStopStep("Motors", MotorProtection.StopAllMotor);
                yield return new MachineStopStep("Application", Application.StopLegacyOutputs);
            }
        }

        /// <summary>只允许最新停止操作发布待机；停止失败保留状态和失败原因供重试。</summary>
        private static async Task<bool> ApplyStopResult(Task<bool> pending, long version)
        {
            bool succeeded = await pending.ConfigureAwait(false);
            lock (machineCommandSync)
            {
                if (version != machineCommandVersion) return false;
                MachineState.Global.CompleteStop(succeeded);
            }
            if (!succeeded) RecordControlFailure(new InvalidOperationException(machineStop.Error));
            return succeeded;
        }

        /// <summary>暂停旧流程时异步取消手动动作，电机停止与各模块取消相互独立。</summary>
        public static void StopManualRun()
        {
            Task pending;
            lock (machineCommandSync) pending = manualStopWork = StartManualStops();
            _ = ObserveManualStop(pending);
        }

        /// <summary>同时取消各模块手动会话及电机，保留全部回调的真实完成任务。</summary>
        private static Task StartManualStops()
        {
            List<Task> requests = ModuleManager.ModuleList.ToArray().Select(module => module.RequestManualStop()).ToList();
            requests.Add(DeviceOperations.RequestStopAllAsync());
            requests.Add(Task.Factory.StartNew(() => MotorProtection.StopAllMotor(), CancellationToken.None,
                TaskCreationOptions.LongRunning, TaskScheduler.Default));
            return Task.WhenAll(requests);
        }

        /// <summary>观察暂停过程中后台手动停止的异常，避免未观察任务丢失故障。</summary>
        private static async Task ObserveManualStop(Task pending)
        {
            try { await pending.ConfigureAwait(false); }
            catch (Exception error) { RecordControlFailure(error); }
        }

        /// <summary>将排队、执行及取消清理中的手动操作统一视为占用。</summary>
        public static bool AnyManualRunning() { return !manualStopWork.IsCompleted || DeviceOperations.HasPending || ModuleManager.ModuleList.ToArray().Any(module => module.IsManualRun()); }

        /// <summary>记录完整控制异常；日志源未就绪或写入失败时仍保留调试输出。</summary>
        private static void RecordControlFailure(Exception error)
        {
            System.Diagnostics.Debug.WriteLine(error);
            try { DataF?.AddLogError(error.ToString()); }
            catch (Exception logError) { System.Diagnostics.Debug.WriteLine(logError); }
        }

        /// <summary>请求应用提示操作员清除报警。</summary>
        private static void ShowResetAlarmWarning()
        {
            Application.ShowWarning(LangMsg("MiddleLayer", "msg_ResetAlarmFirst", "请先清除报警！", "Please Reset Alarm Firstly!", "¡Restablezca la alarma primero!"),
                LangMsg("Common", "msg_OK", "确定", "OK", "Aceptar"));
        }
    }
}
