using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NPSDK
{
    public partial class Flow_Chart
    {
        private static readonly object executionSync = new object();
        private static readonly HashSet<LegacyRun> legacyRuns = new HashSet<LegacyRun>();
        private static readonly AsyncLocal<CancellationToken> executionToken = new AsyncLocal<CancellationToken>();
        private static readonly AsyncLocal<LegacyRun> currentExecution = new AsyncLocal<LegacyRun>();
        private LegacyRun legacyRun;

        /// <summary>当前旧式流程的完成任务；阻塞调用真正退出前不会完成。</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Task<object> task { get; private set; }

        /// <summary>供节点中的网络、等待和驱动封装协作取消；流程之外返回不可取消令牌。</summary>
        public static CancellationToken CurrentExecutionToken { get { return executionToken.Value; } }

        /// <summary>在同步驱动调用前后检查停止标记，避免取消通知尚未派发时继续发送命令。</summary>
        public static void ThrowIfExecutionStopped()
        {
            LegacyRun run = currentExecution.Value;
            if (run != null && run.Stopping) throw new OperationCanceledException(executionToken.Value);
            executionToken.Value.ThrowIfCancellationRequested();
        }

        /// <summary>当前会话是否已收到停止请求；仍在执行的同步调用保持占用。</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsStopping { get { lock (executionSync) return legacyRun != null && legacyRun.Stopping; } }

        /// <summary>最近一次启动或执行失败的诊断，不写入配方。</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string LastRunError { get; private set; }

        internal static bool HasActiveLegacyRuns { get { lock (executionSync) return legacyRuns.Count > 0; } }

        private sealed class LegacyStep
        {
            internal FlowChartHandle Handler;
            internal Flow_Chart[] Targets;
            internal string Caption;
        }

        private sealed class LegacyRun
        {
            internal readonly Dictionary<Flow_Chart, LegacyStep> Steps;
            internal readonly TaskCompletionSource<object> Completion = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            internal readonly CancellationTokenSource Cancellation = new CancellationTokenSource();
            internal readonly bool CanPause, CanStop;
            internal volatile bool Stopping;
            internal Task CancelTask;

            /// <summary>保存本次执行的链接与回调快照。</summary>
            internal LegacyRun(Dictionary<Flow_Chart, LegacyStep> steps, bool canPause, bool canStop)
            {
                Steps = steps; CanPause = canPause; CanStop = canStop;
            }

            /// <summary>立即标记停止，在后台通知取消回调，避免网络取消回调阻塞操作线程。</summary>
            internal void RequestStop()
            {
                if (Stopping) return;
                Stopping = true;
                CancelTask = Task.Run(() =>
                {
                    try { Cancellation.Cancel(); }
                    catch (Exception error) { Flow_Module.Module_AddAlarmLog(error.ToString()); }
                });
            }
        }

        /// <summary>校验并启动一条流程；重复启动直接返回，不等待或释放正在执行的任务。</summary>
        public void FlowChart_Run(bool IsPause = true, bool IsStop = true)
        {
            if (IsDisposed || Disposing || ProgramEditing || ProgramManaged || DesignMode
                || LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;
            LegacyRun run;
            try
            {
                lock (executionSync)
                {
                    if (legacyRun != null) return;
                    if (IsStop && (Flow_Module.AllStop || Flow_Module.Module_GetStopStatus)) return;
                    Dictionary<Flow_Chart, LegacyStep> plan = CaptureLegacyPlan();
                    if (plan.Keys.Any(node => node.legacyRun != null))
                        throw new InvalidOperationException("Flow graph is still occupied by an unfinished run: " + Name);
                    run = new LegacyRun(plan, IsPause, IsStop);
                    foreach (Flow_Chart node in plan.Keys)
                    {
                        node.legacyRun = run;
                        node.task = run.Completion.Task;
                        node.LastRunError = null;
                        node.Flow_ChartIsCanPause = IsPause;
                        node.Flow_ChartIsCanStop = IsStop;
                    }
                    legacyRuns.Add(run);
                }
                _ = Task.Run(() => ExecuteLegacyRun(run));
            }
            catch (Exception error) { ReportLegacyError(error); }
        }

        /// <summary>冻结可达节点及出口；拒绝失效、跨窗体、无回调和多重回调的链接。</summary>
        private Dictionary<Flow_Chart, LegacyStep> CaptureLegacyPlan()
        {
            Dictionary<Flow_Chart, LegacyStep> plan = new Dictionary<Flow_Chart, LegacyStep>();
            Stack<Flow_Chart> pending = new Stack<Flow_Chart>();
            Form owner = FindForm();
            pending.Push(this);
            while (pending.Count > 0)
            {
                Flow_Chart node = pending.Pop();
                if (plan.ContainsKey(node)) continue;
                if (node.IsDisposed || node.Disposing || node.ProgramEditing || node.ProgramManaged)
                    throw new InvalidOperationException("Flow link targets an unavailable node: " + node.Name);
                if (!ReferenceEquals(node.FindForm(), owner))
                    throw new InvalidOperationException("Flow link crosses form boundaries: " + node.Name);
                FlowChartHandle[] handlers = node.GetFlowChartHandlers();
                if (handlers.Length != 1)
                    throw new InvalidOperationException("Flow node must have exactly one handler: " + node.Name);
                Flow_Chart[] targets = { node._Next, node._Casse1, node._Casse2, node._Casse3 };
                plan.Add(node, new LegacyStep { Handler = handlers[0], Targets = targets, Caption = node.Name + ":" + node.text });
                foreach (Flow_Chart target in targets) if (target != null) pending.Push(target);
            }
            return plan;
        }

        /// <summary>用一个异步任务顺序执行整条流程，保留节点占用直到所有调用与取消回调退出。</summary>
        private async Task ExecuteLegacyRun(LegacyRun run)
        {
            Flow_Chart current = this;
            int transitions = 0;
            executionToken.Value = run.Cancellation.Token;
            currentExecution.Value = run;
            try
            {
                while (current != null)
                {
                    current.CheckLegacyStop(run);
                    LegacyStep step = run.Steps[current];
                    ResultType result = await current.ExecuteLegacyStep(run, step).ConfigureAwait(false);
                    current.CheckLegacyStop(run);
                    current = SelectLegacyTarget(step, result);
                    if (++transitions % 64 == 0) await Task.Delay(1, run.Cancellation.Token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) when (run.Stopping || run.Cancellation.IsCancellationRequested) { }
            catch (Exception error)
            {
                (current ?? this).ReportLegacyError(error);
                LastRunError = error.Message;
            }
            finally
            {
                executionToken.Value = CancellationToken.None;
                currentExecution.Value = null;
                Task cancel;
                lock (executionSync)
                {
                    // 禁止新的取消回调进入，完成任务之前仍保留整条流程的占用。
                    run.Stopping = true;
                    cancel = run.CancelTask;
                }
                if (cancel != null) await cancel.ConfigureAwait(false);
                run.Cancellation.Dispose();
                lock (executionSync)
                {
                    foreach (Flow_Chart node in run.Steps.Keys)
                    {
                        node.RunningStatus = false;
                        node.legacyRun = null;
                    }
                    legacyRuns.Remove(run);
                    run.Completion.TrySetResult(null);
                }
            }
        }

        /// <summary>执行一个节点，IDLE 使用可取消等待；暂停不计入耗时，超时报警仅报告一次。</summary>
        private async Task<ResultType> ExecuteLegacyStep(LegacyRun run, LegacyStep step)
        {
            RunningStatus = true;
            FlowChart_StartTime = DateTime.Now;
            BeginProgramTiming();
            Flow_Module.Module_AddRunLog(step.Caption + "------Begin");
            bool alarmReported = false;
            try
            {
                SetProgramState("", Color.Green);
                while (true)
                {
                    await WaitForLegacyResume(run).ConfigureAwait(false);
                    CheckLegacyStop(run);
                    ResultType result = step.Handler();
                    CheckLegacyStop(run);
                    RefreshProgramTiming();
                    if (!alarmReported && gAlarmStaus && FlowChart_ElapsedMilliseconds > gAlarmTimeOut)
                    {
                        alarmReported = true;
                        Alarm.AlarmDataClass alarm = Alarm.Show(gAlarmCode);
                        Flow_Module.Module_AddAlarmLog("Alarm>>Code:" + alarm.Code + ">>Content:" + alarm.Content + ">>Position:" + step.Caption);
                    }
                    await WaitForLegacyResume(run).ConfigureAwait(false);
                    if (result != ResultType.IDLE) return result;
                    await Task.Delay(50, run.Cancellation.Token).ConfigureAwait(false);
                }
            }
            finally
            {
                PauseProgramTiming();
                FlowChart_EndTime = DateTime.Now;
                RunningStatus = false;
                SetProgramState("", Color.White);
                Flow_Module.Module_AddRunLog(step.Caption + "------Over");
            }
        }

        /// <summary>等待暂停解除，期间持续检查取消，避免占用工作线程或重发当前动作。</summary>
        private async Task WaitForLegacyResume(LegacyRun run)
        {
            CheckLegacyStop(run);
            if (!run.CanPause || (!Flow_Module.AllPause && !Flow_Module.Module_GetPauseStatus)) return;
            PauseProgramTiming();
            SetProgramState("", Color.Yellow);
            while (Flow_Module.AllPause || Flow_Module.Module_GetPauseStatus)
            {
                CheckLegacyStop(run);
                await Task.Delay(20, run.Cancellation.Token).ConfigureAwait(false);
            }
            ResumeProgramTiming();
            SetProgramState("", Color.Green);
            CheckLegacyStop(run);
        }

        /// <summary>取消已停止或失效的会话；同步网络调用返回后必须先检查，再选择下一节点。</summary>
        private void CheckLegacyStop(LegacyRun run)
        {
            if (run.Stopping || IsDisposed || Disposing || ProgramEditing || ProgramManaged
                || (run.CanStop && (Flow_Module.AllStop || Flow_Module.Module_GetStopStatus)))
            {
                lock (executionSync) run.RequestStop();
                throw new OperationCanceledException(run.Cancellation.Token);
            }
        }

        /// <summary>NEXT 未连接沿用结束语义；选中的 CASE 必须连接，不把漏连当成正常结束。</summary>
        private static Flow_Chart SelectLegacyTarget(LegacyStep step, ResultType result)
        {
            if (result == ResultType.End) return null;
            if (result < ResultType.NEXT || result > ResultType.CASE3)
                throw new InvalidOperationException("Invalid flow result: " + result + " at " + step.Caption);
            Flow_Chart target = step.Targets[(int)result];
            if (target == null && result != ResultType.NEXT)
                throw new InvalidOperationException("Missing flow link: " + result + " at " + step.Caption);
            return target;
        }

        /// <summary>发送旧式流程停止请求，不等待网络调用或取消回调完成。</summary>
        internal static void StopLegacyRuns()
        {
            lock (executionSync)
                foreach (LegacyRun run in legacyRuns) if (run.CanStop) run.RequestStop();
        }

        /// <summary>释放节点时撤销其会话，即使该会话忽略普通停止按钮。</summary>
        private void StopLegacyRunOnDispose()
        {
            lock (executionSync) if (legacyRun != null) legacyRun.RequestStop();
        }

        /// <summary>记录故障而不弹出阻塞后台任务的模态窗口。</summary>
        private void ReportLegacyError(Exception error)
        {
            LastRunError = error.Message;
            Flow_Module.Module_AddAlarmLog("Flow: " + Name + ": " + error);
            SetProgramState("", Color.Red);
        }

        /// <summary>兼容旧后台工作器入口，执行权统一交给流程会话。</summary>
        [Obsolete("Use FlowChart_Run to start a flow.")]
        public void Do_work(object sender, DoWorkEventArgs e) { FlowChart_Run(Flow_ChartIsCanPause, Flow_ChartIsCanStop); }

        /// <summary>保留旧完成回调签名；会话自身负责跳转，迟到回调不再启动节点。</summary>
        [Obsolete("Flow transitions are managed by the execution session.")]
        public void method(object state) { }
    }
}
