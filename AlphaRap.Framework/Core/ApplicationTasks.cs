using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlphaRap
{
    /// <summary>应用任务的真实占用、停止请求和异常观察，不按等待时限释放实际工作。</summary>
    public sealed class ApplicationTasks
    {
        public static readonly ApplicationTasks Global = new ApplicationTasks();
        private readonly object sync = new object();
        private readonly HashSet<Entry> entries = new HashSet<Entry>();
        private readonly Queue<string> failures = new Queue<string>();
        private bool stopping;
        public string[] Pending { get { lock (sync) return entries.Select(entry => entry.Name).ToArray(); } }
        public string[] Failures { get { lock (sync) return failures.ToArray(); } }
        private sealed class Entry
        {
            internal string Name;
            internal Task Work;
            internal Func<Task> Stop;
            internal readonly TaskCompletionSource<Exception> Done = new TaskCompletionSource<Exception>(TaskCreationOptions.RunContinuationsAsynchronously);
            internal readonly object Sync = new object();
            internal Task StopWork = Task.FromResult(0);
            internal bool Requested, Finished;
            /// <summary>取消回调独立派发，阻塞不会占用调用者或阻止其他任务的停止请求。</summary>
            internal Task RequestStop()
            {
                lock (Sync)
                {
                    if (Finished || Requested || Stop == null) return StopWork;
                    Requested = true;
                    StopWork = Task.Factory.StartNew(Stop, System.Threading.CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();
                    return StopWork;
                }
            }
            /// <summary>锁定任务退出边界，保留已经启动的停止回调。</summary>
            internal Task Finish() { lock (Sync) { Finished = true; return StopWork; } }
        }

        /// <summary>观察原任务而不改变其返回结果；停止期间新登记任务也立即收到停止请求。</summary>
        public Task Track(string name, Task work, Func<Task> stop = null)
        {
            if (work == null) throw new ArgumentNullException(nameof(work));
            var entry = new Entry { Name = name, Work = work, Stop = stop };
            lock (sync) { entries.Add(entry); if (stopping) entry.RequestStop(); }
            _ = Observe(entry);
            return work;
        }

        /// <summary>观察工作与停止异常，完成前始终保留登记。</summary>
        private async Task Observe(Entry entry)
        {
            var errors = new List<Exception>();
            try { await entry.Work.ConfigureAwait(false); } catch (OperationCanceledException) { } catch (Exception error) { errors.Add(error); }
            try { await entry.Finish().ConfigureAwait(false); } catch (Exception error) { errors.Add(error); }
            Exception failure = errors.Count == 0 ? null : new AggregateException(entry.Name, errors);
            lock (sync)
            {
                entries.Remove(entry);
                if (failure != null) { failures.Enqueue(failure.ToString()); while (failures.Count > 100) failures.Dequeue(); }
            }
            entry.Done.TrySetResult(failure);
        }

        /// <summary>先并行请求所有任务停止，再异步等实际退出；不会把超时当作完成。</summary>
        public async Task StopAllAsync()
        {
            var errors = new List<Exception>();
            while (true)
            {
                Entry[] active;
                lock (sync) { stopping = true; active = entries.ToArray(); }
                if (active.Length == 0) break;
                foreach (Entry entry in active) _ = entry.RequestStop();
                Exception[] results = await Task.WhenAll(active.Select(entry => entry.Done.Task)).ConfigureAwait(false);
                errors.AddRange(results.Where(error => error != null));
            }
            if (errors.Count > 0) throw new AggregateException(errors);
        }

        /// <summary>没有旧任务占用时开启新的应用生命周期。</summary>
        public void BeginScope()
        {
            lock (sync)
            {
                if (entries.Count != 0) throw new InvalidOperationException(FrameworkLanguage.LangMsg("ApplicationTasks", "pending", "应用任务尚未退出。", "Application tasks have not exited.", "Las tareas de la aplicación aún no han finalizado."));
                stopping = false; failures.Clear();
            }
        }
    }
}
