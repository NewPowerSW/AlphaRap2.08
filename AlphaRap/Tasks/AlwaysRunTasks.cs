using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AlphaRap.Configuration;
using AlphaRap.TaskProcess;
using static AlphaRap.FrameworkLanguage;

namespace AlphaRap
{
    /// <summary>发现应用常驻任务，为每个任务单独等待其下一轮刷新。</summary>
    internal sealed class AlwaysRunTasks : IDisposable
    {
        private readonly Entry[] entries;
        private readonly object stopSync = new object();
        private CancellationTokenSource cancellation;
        private Task stopRequest;

        private sealed class Entry
        {
            internal string Name;
            internal int IntervalMs;
            internal string[] InputNames;
            internal IAlwaysRunTask Task;
        }

        /// <summary>读取任务配置，创建失败的任务记入运行日志。</summary>
        internal AlwaysRunTasks(Assembly assembly)
        {
            var discovered = new List<Entry>();
            foreach (Type type in LoadTypes(assembly))
            {
                if (type == null || !type.IsClass || type.IsAbstract || type.ContainsGenericParameters ||
                    !typeof(IAlwaysRunTask).IsAssignableFrom(type)) continue;
                try { discovered.Add(CreateEntry(type)); }
                catch (Exception error) { ReportFailure(type.FullName, error.GetBaseException()); }
            }
            entries = discovered.ToArray();
        }

        /// <summary>随站内 IO 在初始化阶段连接这些配置名称。</summary>
        internal IEnumerable<string> InputNames => entries.SelectMany(entry => entry.InputNames);

        /// <summary>发现能加载的具体类，保留部分成功结果。</summary>
        private static Type[] LoadTypes(Assembly assembly)
        {
            try { return assembly.GetTypes(); }
            catch (ReflectionTypeLoadException error)
            {
                ReportFailure(assembly.GetName().Name, error);
                return error.Types;
            }
        }

        /// <summary>创建一个无参数的任务实例及其固定调度参数。</summary>
        private static Entry CreateEntry(Type type)
        {
            var task = (IAlwaysRunTask)Activator.CreateInstance(type, true);
            int interval = task.IntervalMs;
            if (interval <= 0)
                throw new ArgumentOutOfRangeException(nameof(task.IntervalMs));
            string[] inputNames = task.InputNames ?? Array.Empty<string>();
            if (inputNames.Any(string.IsNullOrWhiteSpace))
                throw new ArgumentException(nameof(task.InputNames));
            return new Entry { Name = type.FullName, Task = task, IntervalMs = interval, InputNames = inputNames.ToArray() };
        }

        /// <summary>启动独立任务并登记真实占用，供关闭应用时等待。</summary>
        internal void Start(StationIo io)
        {
            if (SysPara.Simulation || entries.Length == 0 || cancellation != null) return;
            cancellation = new CancellationTokenSource();
            CancellationToken token = cancellation.Token;
            var context = new AlwaysRunContext(io);
            Task[] running = entries.Select(entry => Task.Run(() => RunAsync(entry, context, token))).ToArray();
            ApplicationTasks.Global.Track("AlwaysRun", Task.WhenAll(running), RequestStop);
        }

        /// <summary>异步请求所有任务取消，重复请求使用同一个取消任务。</summary>
        internal Task RequestStop()
        {
            lock (stopSync)
            {
                if (cancellation == null) return Task.CompletedTask;
                return stopRequest ?? (stopRequest = Task.Run((Action)cancellation.Cancel));
            }
        }

        /// <summary>按间隔顺序等待刷新，每次连续故障记录一次。</summary>
        private static async Task RunAsync(Entry entry, AlwaysRunContext context, CancellationToken token)
        {
            bool failureReported = false;
            while (!token.IsCancellationRequested && !ApplicationHost.IsShuttingDown)
            {
                await Task.Delay(entry.IntervalMs, token).ConfigureAwait(false);
                if (SysPara.Simulation || ApplicationHost.IsShuttingDown || ProjectConfiguration.IsBusy) continue;
                token.ThrowIfCancellationRequested();

                try
                {
                    await entry.Task.RefreshAsync(context, token).ConfigureAwait(false);
                    failureReported = false;
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested) { return; }
                catch (Exception error)
                {
                    if (!failureReported) ReportFailure(entry.Name, error);
                    failureReported = true;
                }
            }
        }

        /// <summary>按任务类名记录失败，日志不可写时保留调试输出。</summary>
        private static void ReportFailure(string name, Exception error)
        {
            string format = LangMsg("AlwaysRun", "taskFailed", "常驻任务「{0}」失败。", "Background task '{0}' failed.", "La tarea en segundo plano «{0}» falló.");
            string message = string.Format(format, name) + Environment.NewLine + error;
            Debug.WriteLine(message);
            try { ApplicationHost.Data?.AddLogError(message); }
            catch (Exception logError) { Debug.WriteLine(logError); }
        }

        /// <summary>所有刷新和取消回调退出后释放取消源。</summary>
        public void Dispose()
        {
            lock (stopSync)
            {
                cancellation?.Dispose();
                cancellation = null;
            }
        }
    }
}
