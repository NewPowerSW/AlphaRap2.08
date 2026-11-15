using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AlphaRap
{
    /// <summary>一次停止操作的请求、实际占用检查及停止错误来源。</summary>
    public sealed class MachineStopStep
    {
        public string Name { get; }
        internal Action Request { get; }
        internal Func<bool> IsRunning { get; }
        internal Func<string> GetError { get; }
        internal object Identity { get; }

        /// <summary>登记停止回调；实际任务退出和停止错误检查可按能力提供。</summary>
        public MachineStopStep(string name, Action request, Func<bool> isRunning = null, Func<string> getError = null, object identity = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("A stop operation name is required.", nameof(name));
            Name = name;
            Request = request ?? throw new ArgumentNullException(nameof(request));
            IsRunning = isRunning;
            GetError = getError;
            Identity = identity ?? name;
        }
    }

    /// <summary>独立派发停止操作；所有回调及实际任务退出前保留停止状态。</summary>
    public sealed class MachineStopCoordinator
    {
        private readonly object sync = new object();
        private readonly int waitingMilliseconds;
        private readonly Stopwatch elapsed = new Stopwatch();
        private Task<bool> completion = Task.FromResult(true);
        private MachineStopStep[] steps = new MachineStopStep[0];
        private Task<Exception>[] requests = new Task<Exception>[0];
        private string error;
        private bool finalizing;
        private int revision;
        private Action afterStopped;

        public Task<bool> Completion { get { lock (sync) return completion; } }
        public bool IsStopping { get { lock (sync) return !completion.IsCompleted; } }
        public bool IsWaiting { get { lock (sync) return !completion.IsCompleted && elapsed.ElapsedMilliseconds >= waitingMilliseconds; } }
        public string Error { get { lock (sync) return error; } }
        public string[] PendingOperations
        {
            get
            {
                lock (sync)
                {
                    if (finalizing && !completion.IsCompleted) return new[] { "Finalization" };
                    return steps.Where((step, index) => index >= requests.Length || !requests[index].IsCompleted).Select(step => step.Name).ToArray();
                }
            }
        }

        /// <summary>设置等待提示阈值；超出阈值只提示等待，不释放未结束的任务。</summary>
        public MachineStopCoordinator(int waitingMilliseconds = 5000)
        {
            if (waitingMilliseconds < 0) throw new ArgumentOutOfRangeException(nameof(waitingMilliseconds));
            this.waitingMilliseconds = waitingMilliseconds;
        }

        /// <summary>重复停止共用当前操作；各设备回调独立执行，彼此阻塞不会阻止派发。</summary>
        public Task<bool> RequestStop(IEnumerable<MachineStopStep> operations, Action afterStopped = null)
        {
            if (operations == null) throw new ArgumentNullException(nameof(operations));
            MachineStopStep[] proposed = operations.ToArray();
            if (proposed.Any(step => step == null)) throw new ArgumentException("A stop operation is missing.", nameof(operations));
            lock (sync)
            {
                bool fresh = completion.IsCompleted;
                TaskCompletionSource<bool> finished = null;
                if (fresh)
                {
                    steps = new MachineStopStep[0]; requests = new Task<Exception>[0];
                    error = null; this.afterStopped = null; finalizing = false;
                    elapsed.Restart();
                    finished = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                    completion = finished.Task;
                }
                MachineStopStep[] additions = proposed.Where(step => !steps.Any(existing => Equals(existing.Identity, step.Identity))).GroupBy(step => step.Identity).Select(group => group.First()).ToArray();
                if (additions.Length > 0)
                {
                    steps = steps.Concat(additions).ToArray();
                    requests = requests.Concat(additions.Select(StopStep)).ToArray();
                    finalizing = false;
                    revision++;
                }
                if (afterStopped != null && !Equals(this.afterStopped, afterStopped)) { this.afterStopped = afterStopped; revision++; }
                if (fresh) _ = Complete(finished);
                return completion;
            }
        }

        /// <summary>独立发送一个停止请求，回调失败后仍等待其真实占用退出。</summary>
        private static async Task<Exception> StopStep(MachineStopStep step)
        {
            List<Exception> errors = new List<Exception>();
            try
            {
                await Task.Factory.StartNew(step.Request, CancellationToken.None,
                    TaskCreationOptions.LongRunning, TaskScheduler.Default).ConfigureAwait(false);
            }
            catch (Exception failure) { errors.Add(failure); }
            try
            {
                while (step.IsRunning != null && step.IsRunning()) await Task.Delay(25).ConfigureAwait(false);
                string failure = step.GetError?.Invoke();
                if (!string.IsNullOrEmpty(failure)) errors.Add(new InvalidOperationException(failure));
            }
            catch (Exception failure) { errors.Add(failure); }
            return errors.Count == 0 ? null : new AggregateException(step.Name, errors);
        }

        /// <summary>完成所有停止工作后发布汇总结果；失败保留原因，允许显式重试停止。</summary>
        private async Task Complete(TaskCompletionSource<bool> finished)
        {
            while (true)
            {
                Task<Exception>[] pending;
                int version;
                lock (sync) { pending = requests; version = revision; }
                Exception[] failures = await Task.WhenAll(pending).ConfigureAwait(false);
                List<Exception> errors = failures.Where(failure => failure != null).ToList();
                Action finalize;
                lock (sync)
                {
                    if (version != revision) continue;
                    finalize = afterStopped; finalizing = finalize != null;
                }
                if (finalize != null)
                {
                    try
                    {
                        await Task.Factory.StartNew(finalize, CancellationToken.None,
                            TaskCreationOptions.LongRunning, TaskScheduler.Default).ConfigureAwait(false);
                    }
                    catch (Exception failure) { errors.Add(failure); }
                }
                lock (sync)
                {
                    if (version != revision) continue;
                    error = string.Join(Environment.NewLine, errors.Select(failure => failure.ToString()));
                    if (error.Length == 0) error = null;
                    elapsed.Stop(); finalizing = false;
                    finished.TrySetResult(error == null);
                    return;
                }
            }
        }
    }
}
