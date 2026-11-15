using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AlphaRapLibrary;
using NPSDK;

namespace AlphaRap
{
    /// <summary>跟踪调试设备操作及取消清理，不随面板或所属模块的释放丢失实际占用。</summary>
    public static class DeviceOperations
    {
        private static readonly object sync = new object();
        private static readonly HashSet<Entry> running = new HashSet<Entry>();
        private static long stopVersion;
        private static int admissionBlocks;
        public static bool HasPending { get { lock (sync) return running.Count > 0; } }
        public static long StopVersion { get { lock (sync) return stopVersion; } }
        private static bool AdmissionBlocked { get { lock (sync) return admissionBlocks > 0; } }

        private sealed class AdmissionBlock : IDisposable
        {
            private int disposed;
            /// <summary>释放一次命令预留，不解除其他仍在生效的预留。</summary>
            public void Dispose()
            {
                if (Interlocked.Exchange(ref disposed, 1) != 0) return;
                lock (sync) admissionBlocks--;
            }
        }

        /// <summary>预留整机命令准备期，期间拒绝新调试 IO，已有任务仍保留真实占用。</summary>
        public static IDisposable SuspendNewOperations()
        {
            var block = new AdmissionBlock();
            lock (sync) admissionBlocks++;
            return block;
        }

        private sealed class Entry
        {
            internal readonly CancellationTokenSource Source = new CancellationTokenSource();
            internal volatile bool Stopping;
            internal bool AllowDuringStop;
            internal long Version;
            private readonly object cancellationSync = new object();
            private Task cancellation = Task.FromResult(0);
            private bool finished;

            /// <summary>先标记停止，再独立派发取消；回调阻塞不会阻塞 UI 或其他设备。</summary>
            internal Task RequestStop()
            {
                lock (cancellationSync)
                {
                    if (finished || Stopping) return cancellation;
                    Stopping = true;
                    return cancellation = Task.Factory.StartNew(() => Source.Cancel(), CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                }
            }

            /// <summary>实际动作退出后保留正在执行的取消回调，期间不丢失全局占用。</summary>
            internal Task Finish()
            {
                lock (cancellationSync) { finished = true; return cancellation; }
            }
        }

        /// <summary>启动后台设备操作，连接到模块手动会话及外部停止令牌。</summary>
        public static Task RunAsync(ModuleBaseForm owner, Action action, CancellationToken token = default(CancellationToken), bool allowDuringStop = false)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            var entry = new Entry { AllowDuringStop = allowDuringStop };
            lock (sync)
            {
                if (!allowDuringStop && admissionBlocks > 0) { entry.Source.Dispose(); return Task.FromCanceled(new CancellationToken(true)); }
                entry.Version = stopVersion; running.Add(entry);
            }
            return RunOwnedAsync(entry, owner, action, token);
        }

        /// <summary>实际动作、所有取消回调结束后移除登记，窗口被释放也保留此任务。</summary>
        private static async Task RunOwnedAsync(Entry entry, ModuleBaseForm owner, Action action, CancellationToken token)
        {
            CancellationTokenRegistration registration = token.Register(() => { entry.RequestStop(); });
            try
            {
                Action invoke = () => Invoke(entry, action);
                Task work = owner == null ? Task.Factory.StartNew(invoke, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default) : owner.ExecuteManualAsync(invoke);
                await work.ConfigureAwait(false);
            }
            finally
            {
                registration.Dispose();
                try { await entry.Finish().ConfigureAwait(false); }
                finally { entry.Source.Dispose(); lock (sync) running.Remove(entry); }
            }
        }

        /// <summary>在后台调用链传递停止令牌，停止先于派发时不执行任何设备操作。</summary>
        private static void Invoke(Entry entry, Action action)
        {
            if (entry.Stopping || (!entry.AllowDuringStop && (AdmissionBlocked || entry.Version != StopVersion || SysPara.SystemMode == RunMode.STOPPING || SysPara.SystemMode == RunMode.RUN || FlowProgramming.FlowPrograms.AnyRunning)))
                throw new OperationCanceledException(entry.Source.Token);
            using (var linked = CancellationTokenSource.CreateLinkedTokenSource(entry.Source.Token, Flow_Chart.CurrentExecutionToken))
            using (Flow_Chart.UseExecutionToken(linked.Token))
            {
                linked.Token.ThrowIfCancellationRequested(); action(); linked.Token.ThrowIfCancellationRequested();
            }
        }

        /// <summary>独立取消所有登记操作，HasPending 持续表示实际动作或回调是否尚未退出。</summary>
        public static Task RequestStopAllAsync()
        {
            Entry[] entries; lock (sync) { stopVersion++; entries = running.ToArray(); }
            return Task.WhenAll(entries.Where(entry => !entry.AllowDuringStop).Select(entry => entry.RequestStop()));
        }
    }
}
