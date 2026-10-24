using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    /// <summary>可等待的对象调用；记录实际读取结果，避免再次抛出脚本已处理的异常。</summary>
    public sealed class FlowScriptCall
    {
        internal readonly Task<IDictionary<string, object>> Completion;
        private int observed;
        internal bool Observed { get { return Volatile.Read(ref observed) != 0; } }

        /// <summary>关联已登记的实际调用任务。</summary>
        internal FlowScriptCall(Task<IDictionary<string, object>> completion) { Completion = completion; }

        /// <summary>允许脚本直接 await 调用，不在获取等待器时误报结果已经读取。</summary>
        public Awaiter GetAwaiter() { return new Awaiter(this, true); }

        /// <summary>选择是否恢复原同步上下文，同时保留实际结果观察记录。</summary>
        public ConfiguredCall ConfigureAwait(bool continueOnCapturedContext) { return new ConfiguredCall(this, continueOnCapturedContext); }

        public struct ConfiguredCall
        {
            private readonly FlowScriptCall call;
            private readonly bool captureContext;

            /// <summary>记录调用和异步恢复方式。</summary>
            internal ConfiguredCall(FlowScriptCall call, bool captureContext) { this.call = call; this.captureContext = captureContext; }

            /// <summary>创建仍会登记结果读取的等待器。</summary>
            public Awaiter GetAwaiter() { return new Awaiter(call, captureContext); }
        }

        public struct Awaiter : ICriticalNotifyCompletion
        {
            private readonly FlowScriptCall call;
            private readonly ConfiguredTaskAwaitable<IDictionary<string, object>>.ConfiguredTaskAwaiter awaiter;

            /// <summary>保留同一个调用及其完成状态。</summary>
            internal Awaiter(FlowScriptCall call, bool captureContext) { this.call = call; awaiter = call.Completion.ConfigureAwait(captureContext).GetAwaiter(); }
            public bool IsCompleted { get { return awaiter.IsCompleted; } }

            /// <summary>实际取回结果或异常时记录已观察，脚本可正常捕获并重试。</summary>
            public IDictionary<string, object> GetResult()
            {
                Interlocked.Exchange(ref call.observed, 1);
                return awaiter.GetResult();
            }

            /// <summary>在实际调用完成后恢复脚本。</summary>
            public void OnCompleted(Action continuation) { awaiter.OnCompleted(continuation); }

            /// <summary>将编译器的异步恢复交给原任务等待器。</summary>
            public void UnsafeOnCompleted(Action continuation) { awaiter.UnsafeOnCompleted(continuation); }
        }
    }

    /// <summary>脚本可见的运行信息和对象调用入口；不直接提供驱动实例。</summary>
    public sealed class FlowScriptContext
    {
        private readonly Func<string, string, IDictionary<string, object>, CancellationToken, Task<IDictionary<string, object>>> invoke;
        private readonly CancellationTokenSource scope;
        private readonly object sync = new object();
        private readonly List<FlowScriptCall> calls = new List<FlowScriptCall>();
        private TaskCompletionSource<bool> closedCompletion;
        private bool disposed;
        public CancellationToken Token { get; private set; }
        public bool Simulation { get; private set; }
        public bool DebugBreak { get; private set; }
        public string RecipeName { get; private set; }

        /// <summary>注入当前流程会话与真实对象调用管线，不创建或连接设备。</summary>
        public FlowScriptContext(FlowCall call, bool debugBreak,
            Func<string, string, IDictionary<string, object>, CancellationToken, Task<IDictionary<string, object>>> invoke)
        {
            if (call == null) throw new ArgumentNullException("call");
            scope = CancellationTokenSource.CreateLinkedTokenSource(call.Token);
            Token = scope.Token;
            Simulation = call.Simulation;
            DebugBreak = debugBreak;
            RecipeName = call.Context == null ? "" : call.Context.RecipeName ?? "";
            this.invoke = invoke;
        }

        /// <summary>使用节点的断点设置创建脚本上下文，调用委托由流程运行器注入。</summary>
        public FlowScriptContext(FlowCall call,
            Func<string, string, IDictionary<string, object>, CancellationToken, Task<IDictionary<string, object>>> invoke)
            : this(call, call != null && call.Step != null && call.Step.Breakpoint, invoke) { }

        /// <summary>先登记再发起对象调用，遗漏 await 的任务也归当前节点管理；关闭后拒绝新调用。</summary>
        public FlowScriptCall CallAsync(string targetId, string methodId, IDictionary<string, object> inputs)
        {
            TaskCompletionSource<IDictionary<string, object>> completion;
            FlowScriptCall call;
            Dictionary<string, object> values;
            lock (sync)
            {
                if (closedCompletion != null) throw new InvalidOperationException(FlowScriptText.T("scriptScopeClosed"));
                Token.ThrowIfCancellationRequested();
                if (string.IsNullOrWhiteSpace(targetId) || string.IsNullOrWhiteSpace(methodId) || inputs == null)
                    throw new InvalidOperationException(FlowScriptText.T("scriptCallArguments"));
                if (invoke == null) throw new InvalidOperationException(FlowScriptText.T("scriptCallUnavailable"));
                values = new Dictionary<string, object>(inputs);
                completion = new TaskCompletionSource<IDictionary<string, object>>(TaskCreationOptions.RunContinuationsAsynchronously);
                call = new FlowScriptCall(completion.Task);
                calls.Add(call);
            }
            _ = RunCall(targetId, methodId, values, completion);
            return call;
        }

        /// <summary>执行已登记调用，实际委托返回前不将任务标记为完成或取消。</summary>
        private async Task RunCall(string targetId, string methodId, IDictionary<string, object> inputs, TaskCompletionSource<IDictionary<string, object>> completion)
        {
            try
            {
                Token.ThrowIfCancellationRequested();
                IDictionary<string, object> result = await invoke(targetId, methodId, inputs, Token).ConfigureAwait(false);
                Token.ThrowIfCancellationRequested();
                if (result == null) throw new InvalidOperationException(FlowScriptText.T("scriptResultMissing", methodId));
                completion.TrySetResult(result);
            }
            catch (OperationCanceledException)
            {
                CancelClosingScope();
                completion.TrySetCanceled();
            }
            catch (Exception error)
            {
                CancelClosingScope();
                completion.TrySetException(error);
            }
        }

        /// <summary>关闭对象调用入口并收拢全部已登记任务；重复关闭复用同一个完成信号。</summary>
        internal Task CloseAsync(bool cancel)
        {
            FlowScriptCall[] pending = null;
            TaskCompletionSource<bool> completion;
            lock (sync)
            {
                if (closedCompletion == null)
                {
                    closedCompletion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                    pending = calls.ToArray();
                }
                completion = closedCompletion;
            }
            if (cancel) CancelScope();
            if (pending != null) _ = DrainCalls(pending, completion);
            return completion.Task;
        }

        /// <summary>已开始收拢的调用失败时取消其他调用，但不以取消替代等待硬件实际返回。</summary>
        private void CancelClosingScope()
        {
            bool closing;
            lock (sync) closing = closedCompletion != null;
            if (closing) CancelScope();
        }

        /// <summary>请求同一作用域取消，取消回调错误只记录，不覆盖原用户或调用异常。</summary>
        private void CancelScope()
        {
            lock (sync) { if (disposed) return; }
            try { scope.Cancel(); }
            catch (ObjectDisposedException) { }
            catch (AggregateException error) { Debug.WriteLine(error); }
        }

        /// <summary>等待所有真实调用完成后释放取消资源，故障和取消均通过关闭任务返回。</summary>
        private async Task DrainCalls(FlowScriptCall[] pending, TaskCompletionSource<bool> completion)
        {
            Exception failure = null;
            bool cancelled = false;
            try
            {
                if (pending.Any(call => !call.Observed && (call.Completion.IsFaulted || call.Completion.IsCanceled))) CancelScope();
                try { await Task.WhenAll(pending.Select(call => call.Completion)).ConfigureAwait(false); }
                catch (Exception) { /* 全部实际调用已收拢，下面仅报告脚本未读取的故障。 */ }
                FlowScriptCall failed = pending.FirstOrDefault(call => !call.Observed && call.Completion.IsFaulted)
                    ?? pending.FirstOrDefault(call => !call.Observed && call.Completion.IsCanceled);
                if (failed != null) failed.Completion.GetAwaiter().GetResult();
            }
            catch (OperationCanceledException) { cancelled = true; }
            catch (Exception error) { failure = error; }
            finally
            {
                lock (sync) disposed = true;
                scope.Dispose();
            }
            if (failure != null) completion.TrySetException(failure);
            else if (cancelled) completion.TrySetCanceled();
            else completion.TrySetResult(true);
        }

        /// <summary>按真实结果字段读取指定类型，缺失、空值或非法数值明确报错而不返回默认值。</summary>
        public T Get<T>(IDictionary<string, object> result, string name)
        {
            Token.ThrowIfCancellationRequested();
            object value;
            if (result == null || string.IsNullOrWhiteSpace(name) || !result.TryGetValue(name, out value))
                throw new InvalidOperationException(FlowScriptText.T("scriptResultMissing", name));
            try { return (T)new FlowField(name, typeof(T)).ConvertValue(value); }
            catch (Exception error) when (error is FormatException || error is OverflowException || error is InvalidCastException || error is InvalidOperationException || error is ArgumentException)
            {
                throw new InvalidOperationException(FlowScriptText.T("scriptResultType", name), error);
            }
        }
    }
}
