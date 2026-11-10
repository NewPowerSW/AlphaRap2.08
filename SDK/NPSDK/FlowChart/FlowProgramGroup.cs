using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NPSDK
{
    /// <summary>流程入口最近一次报告的位置及执行状态。</summary>
    public sealed class FlowExecutionPosition
    {
        public string StepId { get; }
        public string Caption { get; }
        public string State { get; }
        public bool IsRunning { get; }

        /// <summary>保存一个可以跨线程读取的执行位置。</summary>
        internal FlowExecutionPosition(string stepId, string caption, string state, bool isRunning)
        {
            StepId = stepId;
            Caption = caption;
            State = state;
            IsRunning = isRunning;
        }
    }

    /// <summary>并发调度同一模块的独立入口，复用原节点运行器及统一生命周期。</summary>
    public sealed class FlowProgramGroup
    {
        private sealed class Worker
        {
            internal readonly FlowProgramRuntime Runtime = new FlowProgramRuntime();
            internal FlowDebugSnapshot Snapshot;
            internal volatile FlowExecutionPosition Position;
            internal string EntryId;
        }

        private readonly object sync = new object();
        private volatile Worker[] workers = new Worker[0];
        public bool PauseOnFailure { get; set; }
        public Task Completion { get; private set; } = Task.FromResult(0);
        public bool IsRunning { get { return !Completion.IsCompleted; } }

        /// <summary>按入口顺序读取各流程的位置，区分等待暂停和真正停驻。</summary>
        public FlowExecutionPosition[] GetExecutionPositions()
        {
            return workers.Select(GetExecutionPosition).Where(position => position != null).ToArray();
        }

        /// <summary>补充当前控制请求和任务退出状态，保留最近一次报告的节点。</summary>
        private static FlowExecutionPosition GetExecutionPosition(Worker worker)
        {
            FlowExecutionPosition position = worker.Position;
            if (position == null) return null;
            FlowProgramRuntime runtime = worker.Runtime;
            string state = position.State;
            if (runtime.IsStopping) state = "Stopping";
            else if (runtime.IsPaused && (state == "Running" || state == "Waiting")) state = "PausePending";
            else if (!runtime.IsRunning && runtime.Error == "Cancelled" && state != "Timeout" && state != "Failed") state = "Cancelled";
            return new FlowExecutionPosition(position.StepId, position.Caption, state, runtime.IsRunning);
        }
        public bool IsPaused { get { return IsRunning && workers.Any(item => item.Runtime.IsPaused); } }
        public bool IsWaitingForRetry { get { return IsRunning && workers.Any(item => item.Runtime.IsWaitingForRetry); } }
        public bool CanResume { get { return IsPaused && workers.Any(item => item.Runtime.CanResume) && workers.Where(item => item.Runtime.IsPaused).All(item => item.Runtime.CanResume); } }
        public bool IsDebugging { get { return workers.Any(item => item.Runtime.IsDebugging); } }
        public bool IsDebugPaused { get { return workers.Any(item => item.Runtime.IsDebugPaused); } }
        public bool AlarmWasRaised { get { return workers.Any(item => item.Runtime.AlarmWasRaised); } }
        public string Error
        {
            get
            {
                string[] errors = workers.Select(item => item.Runtime.Error).Where(value => value != null).ToArray();
                string[] failures = errors.Where(value => value != "Cancelled").Distinct().ToArray();
                return failures.Length > 0 ? string.Join(Environment.NewLine, failures) : errors.FirstOrDefault();
            }
        }
        public IList<FlowDebugSnapshot> DebugSnapshots { get { return workers.Select(item => item.Snapshot).Where(item => item != null).ToArray(); } }
        public event Action<FlowStep, string> StateChanged;
        public event Action<FlowDebugSnapshot> DebugChanged;
        public event Action<string> AlarmRequested;
        public event Action Finished;
        public event Action PauseRequested;
        public Func<FlowStep, FlowExecutionContext, CancellationToken, Task<IDictionary<string, object>>> ExecuteAction;
        public Func<FlowStep, FlowExecutionContext, CancellationToken, Task<FlowActionOutcome>> ExecuteStep;

        /// <summary>单入口测试和正式多入口流程使用同一组运行器。</summary>
        public void Start(FlowProgram program, string entry, FlowExecutionContext context, bool debug = false)
        {
            StartMany(program, new[] { entry }, context, debug);
        }

        /// <summary>校验全部入口后并发启动；所有底层调用退出后才完成模块会话。</summary>
        public void StartMany(FlowProgram program, IEnumerable<string> entries, FlowExecutionContext context, bool debug = false)
        {
            lock (sync)
            {
                if (IsRunning) throw new InvalidOperationException("Flow is already running or stopping.");
                if (ExecuteAction == null && ExecuteStep == null) throw new InvalidOperationException("Action provider is missing.");
                FlowProgram plan = FlowProgramFile.Snapshot(program);
                string[] selected = entries.ToArray();
                ValidateStart(plan, selected);
                workers = selected.Select(CreateWorker).ToArray();
                TaskCompletionSource<bool> finished = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                Completion = finished.Task;
                try
                {
                    for (int index = 0; index < workers.Length; index++)
                        workers[index].Runtime.Start(plan, selected[index], CopyContext(context), debug);
                }
                catch
                {
                    Stop();
                    _ = Complete(finished);
                    throw;
                }
                _ = Complete(finished);
            }
        }

        /// <summary>在启动任何线程前校验入口、节点契约和原生出口配置。</summary>
        private void ValidateStart(FlowProgram plan, string[] entries)
        {
            List<string> errors = FlowProgramFile.Validate(plan);
            errors.AddRange(FlowProgramFile.ValidateEntries(plan, entries));
            if (entries.Any(entry => string.IsNullOrEmpty(entry) || plan.Steps.All(step => step.Id != entry))) errors.Add("Missing flow entry.");
            if (errors.Count > 0) throw new InvalidOperationException(string.Join(Environment.NewLine, errors));
            foreach (FlowStep step in entries.SelectMany(entry => FlowProgramFile.Reachable(plan, entry)))
            {
                if ((step.UsesFlowResult || !string.IsNullOrEmpty(step.CodeModuleId)) && ExecuteStep == null)
                    throw new InvalidOperationException("The node requires an outcome-aware action provider: " + step.Caption);
                if (FlowProgramFile.RequiresBranchReview(step)) throw new InvalidOperationException("Review the old exit configuration before running: " + step.Caption);
            }
        }

        /// <summary>每条流程使用独立上下文，共享同批次的参数和已发布节点结果。</summary>
        private static FlowExecutionContext CopyContext(FlowExecutionContext context)
        {
            return new FlowExecutionContext { RunId = context.RunId, CycleId = context.CycleId, ModuleId = context.ModuleId,
                RecipeName = context.RecipeName, Hub = context.Hub, Parameters = new Dictionary<string, object>(context.Parameters) };
        }

        /// <summary>对子运行器挂接观察与报警，不引入第二套节点执行逻辑。</summary>
        private Worker CreateWorker(string entry)
        {
            Worker worker = new Worker { EntryId = entry };
            worker.Runtime.PauseOnFailure = PauseOnFailure;
            worker.Runtime.ExecuteAction = ExecuteAction;
            worker.Runtime.ExecuteStep = ExecuteStep;
            worker.Runtime.StateChanged += (step, state) =>
            {
                worker.Position = new FlowExecutionPosition(step.Id, step.Caption, state, true);
                Action<FlowStep, string> changed = StateChanged;
                if (changed != null) changed(step, state);
            };
            worker.Runtime.DebugChanged += snapshot => ForwardDebug(worker, snapshot);
            worker.Runtime.AlarmRequested += code => { if (PauseOnFailure) Pause(); Action<string> alarm = AlarmRequested; if (alarm != null) alarm(code); };
            worker.Runtime.Finished += () => { if (!PauseOnFailure && worker.Runtime.Error != null && worker.Runtime.Error != "Cancelled") Stop(); };
            return worker;
        }

        /// <summary>为快照标记入口身份，执行结束仍保留该流程最后的输入输出。</summary>
        private void ForwardDebug(Worker worker, FlowDebugSnapshot snapshot)
        {
            FlowDebugSnapshot previous = worker.Snapshot;
            bool finished = snapshot.State == "Finished" && previous != null;
            FlowDebugSnapshot current = new FlowDebugSnapshot { EntryId = worker.EntryId, StepId = snapshot.StepId,
                State = snapshot.State, Inputs = finished ? previous.Inputs : snapshot.Inputs,
                Outputs = finished ? previous.Outputs : snapshot.Outputs, BranchCode = finished ? previous.BranchCode : snapshot.BranchCode };
            worker.Snapshot = current;
            Action<FlowDebugSnapshot> changed = DebugChanged;
            if (changed != null) changed(current);
        }

        /// <summary>等待所有调用释放资源，再通知模块完成；通知期间拒绝重复启动。</summary>
        private async Task Complete(TaskCompletionSource<bool> completion)
        {
            try
            {
                await Task.WhenAll(workers.Select(item => item.Runtime.Completion)).ConfigureAwait(false);
                Action finished = Finished;
                if (finished == null) return;
                foreach (Action observer in finished.GetInvocationList())
                    try { observer(); } catch (Exception error) { Debug.WriteLine(error); }
            }
            finally { completion.TrySetResult(true); }
        }

        /// <summary>暂停所有尚未结束的入口，已发出的动作继续持有资源。</summary>
        public void Pause()
        {
            lock (sync) foreach (Worker worker in workers) if (worker.Runtime.IsRunning) worker.Runtime.Pause();
            Action paused = PauseRequested;
            if (paused != null) paused();
        }

        /// <summary>故障调用全部退出后恢复等待中的入口，不重跑已完成流程。</summary>
        public void Resume()
        {
            lock (sync)
            {
                if (!CanResume) return;
                foreach (Worker worker in workers) if (worker.Runtime.IsPaused) worker.Runtime.Resume();
            }
        }

        /// <summary>取消全部入口；Completion 在真实底层调用退出后完成。</summary>
        public void Stop() { lock (sync) foreach (Worker worker in workers) worker.Runtime.Stop(); }

        /// <summary>继续当前选中的调试节点；未指定时继续全部停驻的入口。</summary>
        public void ContinueDebug(string stepId = null)
        {
            foreach (Worker worker in workers.Where(item => stepId == null || item.Snapshot != null && item.Snapshot.StepId == stepId)) worker.Runtime.ContinueDebug();
        }

        /// <summary>对当前选中的流程执行一个节点，其他入口继续保持断点等待。</summary>
        public void StepDebug(string stepId = null)
        {
            foreach (Worker worker in workers.Where(item => stepId == null || item.Snapshot != null && item.Snapshot.StepId == stepId)) worker.Runtime.StepDebug();
        }
    }
}
