using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NPSDK
{
    public sealed class FlowOutputUnavailableException : InvalidOperationException
    {
        /// <summary>区分尚未发布的输出与条件配置错误。</summary>
        public FlowOutputUnavailableException(string message) : base(message) { }
    }

    public sealed class FlowResultHub
    {
        private readonly object sync = new object();
        private readonly Dictionary<string, Dictionary<string, object>> results = new Dictionary<string, Dictionary<string, object>>();
        private readonly Dictionary<string, object> parameters = new Dictionary<string, object>();
        private string epoch = Guid.NewGuid().ToString("N");

        /// <summary>开始新的设备运行批次，失效此前全部结果。</summary>
        public string Reset()
        {
            lock (sync) { results.Clear(); parameters.Clear(); epoch = Guid.NewGuid().ToString("N"); return epoch; }
        }

        public string Epoch { get { lock (sync) return epoch; } }

        /// <summary>设置本次运行共享参数，不改写配置文件；旧批次的迟到写入会被拒绝。</summary>
        public void SetParameter(string run, string key, object value)
        {
            lock (sync)
            {
                if (run != epoch) throw new OperationCanceledException("Flow run expired.");
                parameters[key] = value;
            }
        }

        /// <summary>读取同批次所有模块共享的参数覆盖值，新批次从配置初值开始。</summary>
        public bool TryReadParameter(string run, string key, out object value)
        {
            lock (sync)
            {
                if (run != epoch) throw new OperationCanceledException("Flow run expired.");
                return parameters.TryGetValue(key, out value);
            }
        }

        /// <summary>发布具有运行批次、产品关联键及节点身份的结果快照。</summary>
        public void Publish(string run, string cycle, string node, IDictionary<string, object> values)
        {
            lock (sync)
            {
                if (run != epoch) throw new OperationCanceledException("Flow run expired.");
                if (results.Count >= 4096) throw new InvalidOperationException("Flow result capacity reached; begin a new run.");
                results[cycle + "|" + node] = new Dictionary<string, object>(values, StringComparer.Ordinal);
            }
        }

        /// <summary>移除即将重新执行的节点结果，禁止读取上次循环残值。</summary>
        public void Invalidate(string run, string cycle, string node)
        {
            lock (sync) if (run == epoch) results.Remove(cycle + "|" + node);
        }

        /// <summary>只读取同批次、同产品关联键且已完成的结果。</summary>
        public object Read(string run, string cycle, string node, string field)
        {
            lock (sync)
            {
                Dictionary<string, object> result;
                object value;
                if (run != epoch || !results.TryGetValue(cycle + "|" + node, out result) || !result.TryGetValue(field, out value))
                    throw new FlowOutputUnavailableException("Output is unavailable for this cycle: " + node + "/" + field);
                return value;
            }
        }

        /// <summary>所有消费者完成后显式释放该产品的运行结果。</summary>
        public void CompleteCycle(string run, string cycle)
        {
            lock (sync)
            {
                if (run != epoch) throw new OperationCanceledException("Flow run expired.");
                foreach (string key in results.Keys.Where(key => key.StartsWith(cycle + "|", StringComparison.Ordinal)).ToArray()) results.Remove(key);
            }
        }
    }

    public sealed class FlowExecutionContext
    {
        public string RunId;
        public string CycleId;
        public string ModuleId;
        public string RecipeName;
        public string ExecutionId;
        public bool Debugging;
        public bool RetainResult = true;
        public FlowResultHub Hub;
        public Dictionary<string, object> Parameters = new Dictionary<string, object>();
        public IDictionary<string, object> CurrentResult = new Dictionary<string, object>();
        public readonly Dictionary<string, object> Variables = new Dictionary<string, object>();
        private readonly HashSet<string> begunCycles = new HashSet<string>(StringComparer.Ordinal);

        /// <summary>为本模块开始新的产品关联键，拒绝复用旧周期结果。</summary>
        public void BeginCycle(string key)
        {
            string cycle = string.IsNullOrWhiteSpace(key) ? Guid.NewGuid().ToString("N") : key;
            if (!begunCycles.Add(cycle)) throw new InvalidOperationException("Product cycle key has already been used in this run: " + cycle);
            CycleId = cycle;
        }

        /// <summary>明确确认本产品全部消费者已完成后释放结果，允许下次重新关联。</summary>
        public void CompleteCycle()
        {
            Hub.CompleteCycle(RunId, CycleId);
            begunCycles.Remove(CycleId);
            RetainResult = false;
        }

        /// <summary>解析类型化来源；结果路径由选择器生成，业务名称不参与翻译。</summary>
        public object Resolve(FlowInput input)
        {
            if (input.Source == "Constant") return input.Reference ?? "";
            if (input.Source == "Context")
            {
                if (input.Reference == "CycleId") return CycleId;
                if (input.Reference == "RecipeName") return RecipeName;
                if (input.Reference == "RunId") return RunId;
                if (input.Reference == "ExecutionId") return ExecutionId;
            }
            object value;
            if (input.Source == "Parameter")
            {
                string key = input.Reference ?? "";
                if (Hub != null && Hub.TryReadParameter(RunId, key, out value)) return value;
                if (Parameters.TryGetValue(key, out value)) return value;
            }
            if (input.Source == "Result" && CurrentResult.TryGetValue(input.Reference ?? "", out value)) return value;
            if (input.Source == "Output")
            {
                int split = (input.Reference ?? "").LastIndexOf('/');
                if (split > 0) return Hub.Read(RunId, CycleId, input.Reference.Substring(0, split), Uri.UnescapeDataString(input.Reference.Substring(split + 1)));
            }
            throw new InvalidOperationException("Invalid or unavailable source: " + input.Source + "/" + input.Reference);
        }
    }

    /// <summary>调试观察时尚未产生或无法解析的输入，不替换为默认数值。</summary>
    public sealed class FlowDebugUnavailable
    {
        public readonly string Message;

        /// <summary>保留来源不可用的实际原因供调试界面显示。</summary>
        public FlowDebugUnavailable(string message) { Message = message; }

        /// <summary>显示输入尚不可读取的原因。</summary>
        public override string ToString() { return Message; }
    }

    /// <summary>单个节点的调试观察快照，不参与流程工程持久化。</summary>
    public sealed class FlowDebugSnapshot
    {
        public string EntryId;
        public string StepId;
        public string State;
        public IDictionary<string, object> Inputs;
        public IDictionary<string, object> Outputs;
        public string BranchCode;
    }

    public sealed class FlowProgramRuntime
    {
        private readonly object sync = new object();
        private CancellationTokenSource cancellation;
        private volatile bool paused;
        private volatile bool debugging;
        private bool debugPaused;
        private bool debugPauseNext;
        private TaskCompletionSource<bool> debugRelease;
        private int alarmNotified;
        private bool recoverFailures;
        private volatile bool failurePending;
        private volatile bool waitingForRetry;
        private CancellationTokenSource activeDeadline;
        private int activeTimeoutMs;
        /// <summary>节点故障后保留会话，等待显式恢复时重试当前节点；默认保留原终止行为。</summary>
        public bool PauseOnFailure { get; set; }
        public Task Completion { get; private set; } = Task.FromResult(0);
        public string Error { get; private set; }
        public bool AlarmWasRaised { get { return Volatile.Read(ref alarmNotified) != 0; } }
        public bool IsRunning { get { return !Completion.IsCompleted; } }
        public bool IsPaused { get { return IsRunning && paused; } }
        public bool IsWaitingForRetry { get { return IsRunning && waitingForRetry; } }
        public bool CanResume { get { lock (sync) return IsPaused && !failurePending && cancellation != null && !cancellation.IsCancellationRequested; } }
        public bool IsDebugging { get { return debugging; } }
        public bool IsDebugPaused { get { lock (sync) return debugPaused; } }
        public event Action<FlowStep, string> StateChanged;
        public event Action<FlowDebugSnapshot> DebugChanged;
        public event Action<string> AlarmRequested;
        public event Action Finished;
        public Func<FlowStep, FlowExecutionContext, CancellationToken, Task<IDictionary<string, object>>> ExecuteAction;
        /// <summary>返回动作输出及可选最终出口；未给出出口时沿用配置条件，旧 ExecuteAction 仍可使用。</summary>
        public Func<FlowStep, FlowExecutionContext, CancellationToken, Task<FlowActionOutcome>> ExecuteStep;

        /// <summary>为工位启动一个顺序执行会话；已有会话未退出时拒绝重复启动。</summary>
        public void Start(FlowProgram program, string entry, FlowExecutionContext context)
        {
            Start(program, entry, context, false);
        }

        /// <summary>启动可选的节点调试会话；调试先停在入口，正常运行忽略全部断点。</summary>
        public void Start(FlowProgram program, string entry, FlowExecutionContext context, bool debug)
        {
            lock (sync)
            {
                if (IsRunning) throw new InvalidOperationException("Flow is already running or stopping.");
                if (ExecuteAction == null && ExecuteStep == null) throw new InvalidOperationException("Action provider is missing.");
                FlowProgram plan = FlowProgramFile.Snapshot(program);
                foreach (FlowStep step in FlowProgramFile.Reachable(plan, entry))
                {
                    if (!string.IsNullOrEmpty(step.CodeModuleId) && ExecuteStep == null)
                        throw new InvalidOperationException("Code modules require an outcome-aware action provider: " + step.Caption);
                    if (step.UsesFlowResult && ExecuteStep == null)
                        throw new InvalidOperationException("Native flow results require an outcome-aware action provider: " + step.Caption);
                    if (FlowProgramFile.RequiresBranchReview(step)) throw new InvalidOperationException("Review the old exit configuration before running: " + step.Caption);
                    List<string> errors = FlowProgramFile.ValidateExecution(step);
                    errors.AddRange(FlowProgramFile.ValidateBranches(step));
                    if (errors.Count > 0) throw new InvalidOperationException(string.Join(Environment.NewLine, errors));
                }
                if (cancellation != null) cancellation.Dispose();
                cancellation = new CancellationTokenSource();
                CancellationToken token = cancellation.Token;
                paused = false;
                debugging = debug;
                debugPaused = false;
                debugPauseNext = debug;
                debugRelease = null;
                context.Debugging = debug;
                Error = null;
                alarmNotified = 0;
                recoverFailures = PauseOnFailure;
                failurePending = false;
                waitingForRetry = false;
                activeDeadline = null;
                Completion = Task.Run(() => Run(plan, entry, context, token));
            }
        }

        /// <summary>暂停在节点边界；已启动的硬件动作继续完成并持有资源。</summary>
        public void Pause()
        {
            lock (sync)
            {
                paused = true;
                if (activeDeadline != null && !activeDeadline.IsCancellationRequested)
                    activeDeadline.CancelAfter(Timeout.Infinite);
            }
        }

        /// <summary>恢复同一会话，不重新发送当前动作。</summary>
        public void Resume()
        {
            lock (sync)
            {
                if (!paused || failurePending || cancellation == null || cancellation.IsCancellationRequested) return;
                if (activeDeadline != null && !activeDeadline.IsCancellationRequested)
                    activeDeadline.CancelAfter(activeTimeoutMs);
                paused = false;
            }
        }

        /// <summary>从当前调试停驻点继续，到下一已配置断点前停止。</summary>
        public void ContinueDebug() { ReleaseDebug(false); }

        /// <summary>从当前调试停驻点执行一个完整节点，到下一节点前停止。</summary>
        public void StepDebug() { ReleaseDebug(true); }

        /// <summary>释放当前节点前的调试等待；运行中的重复命令不改变单步计划。</summary>
        private void ReleaseDebug(bool pauseNext)
        {
            lock (sync)
            {
                if (!debugging || !debugPaused || debugRelease == null) return;
                debugPauseNext = pauseNext;
                debugPaused = false;
                debugRelease.TrySetResult(true);
            }
        }

        /// <summary>请求取消；完成任务退出前运行状态保持为停止中。</summary>
        public void Stop() { lock (sync) if (cancellation != null) cancellation.Cancel(); }

        /// <summary>逐节点执行并发布结果，所有轮询共用同一次动作执行。</summary>
        private async Task Run(FlowProgram program, string entry, FlowExecutionContext context, CancellationToken token)
        {
            Dictionary<string, FlowStep> steps = program.Steps.ToDictionary(step => step.Id);
            HashSet<string> visited = new HashSet<string>();
            string current = entry;
            FlowStep lastStep = null;
            try
            {
                while (!string.IsNullOrEmpty(current))
                {
                    await WaitForResume(token).ConfigureAwait(false);
                    FlowStep step = steps[current];
                    lastStep = step;
                    IDictionary<string, object> inputs = await WaitForDebug(step, context, token).ConfigureAwait(false);
                    await WaitForResume(token).ConfigureAwait(false);
                    bool beginsCycle = step.ExecutionMode == "Action" && step.Action == "cycle.begin";
                    if (!visited.Add(current) && !beginsCycle)
                        throw new InvalidOperationException("Loop requires a cycle.begin node before reusing outputs: " + step.Caption);
                    if (beginsCycle) visited.Clear();
                    string resultKey = program.ModuleId + "/" + step.Id;
                    context.Hub.Invalidate(context.RunId, context.CycleId, resultKey);
                    try { current = await RunStep(step, context, resultKey, inputs, token).ConfigureAwait(false); }
                    catch (Exception error)
                    {
                        if (!recoverFailures || token.IsCancellationRequested) throw;
                        await WaitForRetry(step, context, error, token).ConfigureAwait(false);
                        visited.Remove(current);
                    }
                }
            }
            catch (OperationCanceledException) { Error = "Cancelled"; }
            catch (Exception error) { Error = error.Message; }
            finally
            {
                bool wasDebugging = debugging;
                lock (sync) { debugging = false; debugPaused = false; debugRelease = null; paused = false; waitingForRetry = false; failurePending = false; }
                context.Debugging = false;
                if (wasDebugging) NotifyDebug(lastStep, "Finished", null, null, null);
                NotifyFinished();
            }
        }

        /// <summary>底层调用退出后保留故障节点及已发布结果，显式继续时为当前节点开始新尝试。</summary>
        private async Task WaitForRetry(FlowStep step, FlowExecutionContext context, Exception error, CancellationToken token)
        {
            lock (sync)
            {
                Error = error.Message;
                paused = true;
                waitingForRetry = true;
                failurePending = false;
            }
            ChangeState(step, "RecoveryPaused");
            if (debugging) NotifyDebug(step, "RecoveryPaused", null, null, null);
            await WaitForResume(token).ConfigureAwait(false);
            lock (sync) { waitingForRetry = false; Error = null; alarmNotified = 0; }
        }

        /// <summary>在计时和资源申请前观察或停驻，等待期间仍响应停止和取消。</summary>
        private async Task<IDictionary<string, object>> WaitForDebug(FlowStep step, FlowExecutionContext context, CancellationToken token)
        {
            if (!debugging) return null;
            IDictionary<string, object> inputs = CaptureDebugInputs(step, context, token);
            NotifyDebug(step, "Preparing", inputs, null, null);
            TaskCompletionSource<bool> release;
            lock (sync)
            {
                if (!debugPauseNext && !step.Breakpoint) return inputs;
                debugPauseNext = false;
                debugPaused = true;
                release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                debugRelease = release;
            }
            ChangeState(step, "DebugPaused");
            NotifyDebug(step, "Paused", inputs, null, null);
            try
            {
                using (token.Register(() => release.TrySetCanceled())) await release.Task.ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                return inputs;
            }
            finally
            {
                lock (sync)
                {
                    if (ReferenceEquals(debugRelease, release)) { debugRelease = null; debugPaused = false; }
                }
            }
        }

        /// <summary>读取节点输入的观察值；尚未发布的来源保留原因，取消请求直接向上传递。</summary>
        private static IDictionary<string, object> CaptureDebugInputs(FlowStep step, FlowExecutionContext context, CancellationToken token)
        {
            Dictionary<string, object> values = new Dictionary<string, object>(StringComparer.Ordinal);
            foreach (FlowInput input in step.Inputs)
            {
                token.ThrowIfCancellationRequested();
                try { values[input.Name] = context.Resolve(input); }
                catch (OperationCanceledException) { throw; }
                catch (Exception error) { values[input.Name] = new FlowDebugUnavailable(error.Message); }
            }
            return values;
        }

        /// <summary>执行动作并等待配置条件或原生返回出口；整次节点进入共用一个超时期限。</summary>
        private async Task<string> RunStep(FlowStep step, FlowExecutionContext context, string resultKey, IDictionary<string, object> inputs, CancellationToken token)
        {
            ChangeState(step, "Running");
            using (CancellationTokenSource deadline = CancellationTokenSource.CreateLinkedTokenSource(token))
            {
                int outcome = 0;
                CancellationTokenRegistration watch = deadline.Token.Register(() =>
                {
                    if (token.IsCancellationRequested || Interlocked.CompareExchange(ref outcome, 1, 0) != 0) return;
                    if (recoverFailures) failurePending = true;
                    try { ChangeState(step, "Timeout"); }
                    catch (Exception error) { Debug.WriteLine(error); }
                    NotifyAlarm(step);
                });
                lock (sync)
                {
                    activeDeadline = deadline;
                    activeTimeoutMs = step.TimeoutMs;
                    if (!paused) deadline.CancelAfter(step.TimeoutMs);
                }
                try
                {
                    context.CurrentResult = new Dictionary<string, object>();
                    context.ExecutionId = Guid.NewGuid().ToString("N");
                    context.RetainResult = true;
                    if (debugging)
                    {
                        inputs = CaptureDebugInputs(step, context, deadline.Token);
                        NotifyDebug(step, "Running", inputs, null, null);
                    }
                    FlowActionOutcome result = await RunAction(step, context, deadline.Token).ConfigureAwait(false);
                    deadline.Token.ThrowIfCancellationRequested();
                    if (result == null || result.Outputs == null) throw new InvalidOperationException("The action returned no output contract: " + step.Caption);
                    IDictionary<string, object> output = new Dictionary<string, object>(result.Outputs, StringComparer.Ordinal);
                    context.CurrentResult = output;
                    if (debugging) NotifyDebug(step, "ActionCompleted", inputs, output, result.BranchCode);
                    string exit = result.BranchCode;
                    if (string.IsNullOrEmpty(exit))
                    {
                        if (step.UsesFlowResult || !string.IsNullOrEmpty(step.CodeModuleId)) throw new InvalidOperationException("The action must select its final exit: " + step.Caption);
                        exit = (await WaitForBranch(step, context, deadline.Token).ConfigureAwait(false)).Code;
                    }
                    else ValidateFinalExit(step, exit);
                    deadline.Token.ThrowIfCancellationRequested();
                    if (Interlocked.CompareExchange(ref outcome, 2, 0) != 0) throw new OperationCanceledException(deadline.Token);
                    deadline.CancelAfter(Timeout.Infinite);
                    deadline.Token.ThrowIfCancellationRequested();
                    await WaitForResume(token).ConfigureAwait(false);
                    if (context.RetainResult) context.Hub.Publish(context.RunId, context.CycleId, resultKey, output);
                    ChangeState(step, "Completed");
                    if (debugging) NotifyDebug(step, "Completed", inputs, output, exit);
                    return exit == "End" ? null : step.TargetOf(exit);
                }
                catch (Exception error)
                {
                    bool timedOut = Volatile.Read(ref outcome) == 1;
                    if (!timedOut && token.IsCancellationRequested) { ChangeState(step, "Cancelled"); throw new OperationCanceledException(token); }
                    timedOut = timedOut || deadline.IsCancellationRequested;
                    if (recoverFailures) failurePending = true;
                    Interlocked.Exchange(ref outcome, 2);
                    deadline.CancelAfter(Timeout.Infinite);
                    ChangeState(step, timedOut ? "Timeout" : "Failed");
                    NotifyAlarm(step);
                    string message = timedOut ? "Timeout after " + step.TimeoutMs + " ms" : error.Message;
                    throw new InvalidOperationException(step.Caption + ": " + message, error);
                }
                finally
                {
                    Interlocked.Exchange(ref outcome, 2); deadline.CancelAfter(Timeout.Infinite);
                    lock (sync) { if (ReferenceEquals(activeDeadline, deadline)) activeDeadline = null; }
                    watch.Dispose();
                }
            }
        }

        /// <summary>原生事件在 IDLE 后重复检查，暂停与取消期间不再调用业务方法。</summary>
        private async Task<FlowActionOutcome> RunAction(FlowStep step, FlowExecutionContext context, CancellationToken token)
        {
            bool waiting = false;
            while (true)
            {
                await WaitForResume(token).ConfigureAwait(false);
                FlowActionOutcome result = ExecuteStep == null
                    ? new FlowActionOutcome { Outputs = await ExecuteAction(step, context, token).ConfigureAwait(false) }
                    : await ExecuteStep(step, context, token).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                if (!step.UsesFlowResult) return result;
                if (result == null || result.Outputs == null) throw new InvalidOperationException("The action returned no output contract: " + step.Caption);
                if (result.BranchCode != "IDLE") return result;
                if (!waiting) { ChangeState(step, "Waiting"); waiting = true; }
                await Task.Delay(25, token).ConfigureAwait(false);
            }
        }

        /// <summary>原生事件及代码模块直接决定已声明的出口，End 只结束当前流程。</summary>
        private static void ValidateFinalExit(FlowStep step, string exit)
        {
            if (!step.UsesFlowResult && string.IsNullOrEmpty(step.CodeModuleId)) throw new InvalidOperationException("Configured actions must use their branch conditions: " + step.Caption);
            if (exit == "End") return;
            if (exit != "NEXT" && exit != "CASE1" && exit != "CASE2" && exit != "CASE3")
                throw new InvalidOperationException("Invalid final exit: " + exit);
            if (!FlowProgramFile.BranchesOf(step).Any(branch => branch.Code == exit))
                throw new InvalidOperationException("The returned exit is not configured: " + step.Caption + " / " + exit);
        }

        /// <summary>依次判断 NEXT、CASE1、CASE2、CASE3，未满足时只轮询条件。</summary>
        private async Task<FlowBranch> WaitForBranch(FlowStep step, FlowExecutionContext context, CancellationToken token)
        {
            List<FlowBranch> branches = FlowProgramFile.BranchesOf(step);
            bool waiting = false;
            while (true)
            {
                token.ThrowIfCancellationRequested();
                FlowBranch match = branches.FirstOrDefault(branch => CheckConditions(branch, context));
                if (match != null) return match;
                if (!waiting) { ChangeState(step, "Waiting"); waiting = true; }
                await Task.Delay(25, token).ConfigureAwait(false);
            }
        }

        /// <summary>只按当前出口自己的 AND 或 OR 条件判断，不借用其他出口的结果。</summary>
        private static bool CheckConditions(FlowBranch branch, FlowExecutionContext context)
        {
            if (branch.Unconditional) return true;
            if (branch.AnyCondition) return branch.Conditions.Any(item => CheckCondition(item, context));
            return branch.Conditions.Count > 0 && branch.Conditions.All(item => CheckCondition(item, context));
        }

        /// <summary>每次节点故障只通知一次报警；计时器不会等待阻塞设备调用返回。</summary>
        private void NotifyAlarm(FlowStep step)
        {
            if (Interlocked.Exchange(ref alarmNotified, 1) != 0) return;
            Action<string> requested = AlarmRequested;
            if (requested == null) return;
            foreach (Action<string> observer in requested.GetInvocationList())
            {
                try { observer(step.AlarmCode); }
                catch (Exception error) { Debug.WriteLine(error); }
            }
        }

        /// <summary>等待尚未完成的来源时保持等待；其他来源错误直接报告。</summary>
        private static bool CheckCondition(FlowCondition condition, FlowExecutionContext context)
        {
            try { return FlowProgramFile.Compare(context.Resolve(condition.Left), condition.Operator, condition.Operator == "Empty" ? null : context.Resolve(condition.Right)); }
            catch (FlowOutputUnavailableException)
            {
                return false;
            }
        }

        /// <summary>等待暂停解除并响应停止。</summary>
        private async Task WaitForResume(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            while (paused) await Task.Delay(25, token).ConfigureAwait(false);
        }

        /// <summary>发送节点状态，供宿主切换 UI 线程显示。</summary>
        private void ChangeState(FlowStep step, string state)
        {
            Action<FlowStep, string> changed = StateChanged;
            if (changed == null) return;
            foreach (Action<FlowStep, string> observer in changed.GetInvocationList())
            {
                try { observer(step, state); }
                catch (Exception error) { Debug.WriteLine(error); }
            }
        }

        /// <summary>向调试界面发送独立字典快照，观察器失败不会改变流程动作与报警结果。</summary>
        private void NotifyDebug(FlowStep step, string state, IDictionary<string, object> inputs, IDictionary<string, object> outputs, string branch)
        {
            Action<FlowDebugSnapshot> changed = DebugChanged;
            if (changed == null) return;
            try
            {
                FlowDebugSnapshot snapshot = new FlowDebugSnapshot { StepId = step == null ? null : step.Id, State = state,
                    Inputs = FreezeDebugValues(inputs), Outputs = FreezeDebugValues(outputs), BranchCode = branch };
                foreach (Action<FlowDebugSnapshot> observer in changed.GetInvocationList())
                {
                    try { observer(snapshot); }
                    catch (Exception error) { Debug.WriteLine(error); }
                }
            }
            catch (Exception error) { Debug.WriteLine(error); }
        }

        /// <summary>逐个通知执行结束，观察器异常不会阻止后续状态清理通知。</summary>
        private void NotifyFinished()
        {
            Action finished = Finished;
            if (finished == null) return;
            foreach (Action observer in finished.GetInvocationList())
            {
                try { observer(); }
                catch (Exception error) { Debug.WriteLine(error); }
            }
        }

        /// <summary>复制观察值并禁止界面增删改字典内容。</summary>
        private static IDictionary<string, object> FreezeDebugValues(IDictionary<string, object> values)
        {
            Dictionary<string, object> copy = values == null ? new Dictionary<string, object>() : new Dictionary<string, object>(values, StringComparer.Ordinal);
            return new ReadOnlyDictionary<string, object>(copy);
        }
    }
}
