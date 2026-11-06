using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace NPSDK
{
    /// <summary>受控调用的观察状态，遗漏 await 的故障仍归所属节点。</summary>
    internal interface IFlowCodePendingCall
    {
        Task Task { get; }
        bool Observed { get; }
    }

    /// <summary>可等待的类型化资源调用；只有实际观察结果才标记故障已被业务处理。</summary>
    public sealed class FlowCodeCall<T> : IFlowCodePendingCall
    {
        private readonly Task<T> task;
        private int observed;
        Task IFlowCodePendingCall.Task { get { return task; } }
        bool IFlowCodePendingCall.Observed { get { return Volatile.Read(ref observed) != 0; } }

        /// <summary>将真实调用任务归入节点的等待范围。</summary>
        internal FlowCodeCall(Task<T> task) { this.task = task; }

        /// <summary>支持 await 并在读取结果时记录已观察状态。</summary>
        public Awaiter GetAwaiter() { return new Awaiter(this, task.GetAwaiter()); }

        /// <summary>允许业务代码选择恢复上下文，仍保留节点调用追踪。</summary>
        public ConfiguredCall ConfigureAwait(bool continueOnCapturedContext) { return new ConfiguredCall(this, task.ConfigureAwait(continueOnCapturedContext)); }

        /// <summary>受控调用的普通 await 等待器。</summary>
        public struct Awaiter : ICriticalNotifyCompletion
        {
            private readonly FlowCodeCall<T> owner;
            private readonly TaskAwaiter<T> awaiter;
            /// <summary>保存真实任务等待器。</summary>
            internal Awaiter(FlowCodeCall<T> owner, TaskAwaiter<T> awaiter) { this.owner = owner; this.awaiter = awaiter; }
            public bool IsCompleted { get { return awaiter.IsCompleted; } }
            /// <summary>读取结果并记录调用已被业务代码观察。</summary>
            public T GetResult() { Interlocked.Exchange(ref owner.observed, 1); return awaiter.GetResult(); }
            /// <summary>登记异步恢复。</summary>
            public void OnCompleted(Action continuation) { awaiter.OnCompleted(continuation); }
            /// <summary>登记编译器生成的异步恢复。</summary>
            public void UnsafeOnCompleted(Action continuation) { awaiter.UnsafeOnCompleted(continuation); }
        }

        /// <summary>带恢复上下文设置的受控调用。</summary>
        public struct ConfiguredCall
        {
            private readonly FlowCodeCall<T> owner;
            private readonly ConfiguredTaskAwaitable<T> configured;
            /// <summary>保存配置后的真实任务。</summary>
            internal ConfiguredCall(FlowCodeCall<T> owner, ConfiguredTaskAwaitable<T> configured) { this.owner = owner; this.configured = configured; }
            /// <summary>为配置后的 await 保留结果观察状态。</summary>
            public ConfiguredAwaiter GetAwaiter() { return new ConfiguredAwaiter(owner, configured.GetAwaiter()); }
        }

        /// <summary>配置后的受控调用等待器。</summary>
        public struct ConfiguredAwaiter : ICriticalNotifyCompletion
        {
            private readonly FlowCodeCall<T> owner;
            private readonly ConfiguredTaskAwaitable<T>.ConfiguredTaskAwaiter awaiter;
            /// <summary>保存实际任务与恢复设置。</summary>
            internal ConfiguredAwaiter(FlowCodeCall<T> owner, ConfiguredTaskAwaitable<T>.ConfiguredTaskAwaiter awaiter) { this.owner = owner; this.awaiter = awaiter; }
            public bool IsCompleted { get { return awaiter.IsCompleted; } }
            /// <summary>读取配置后任务结果并记录观察状态。</summary>
            public T GetResult() { Interlocked.Exchange(ref owner.observed, 1); return awaiter.GetResult(); }
            /// <summary>登记配置后的异步恢复。</summary>
            public void OnCompleted(Action continuation) { awaiter.OnCompleted(continuation); }
            /// <summary>登记编译器生成的配置恢复。</summary>
            public void UnsafeOnCompleted(Action continuation) { awaiter.UnsafeOnCompleted(continuation); }
        }
    }

    /// <summary>一次代码节点执行的受控对象访问范围，调用结束前不会放弃任何实际子任务。</summary>
    public sealed class FlowCodeContext : IDisposable
    {
        private readonly object sync = new object();
        private readonly FlowCall call;
        private readonly FlowCodeDefinition definition;
        private readonly CancellationTokenSource scope;
        private readonly List<IFlowCodePendingCall> calls = new List<IFlowCodePendingCall>();
        private TaskCompletionSource<bool> closed;
        private Exception cancellationFailure;
        public CancellationToken Token { get; private set; }
        public bool Simulation { get { return call.Simulation; } }
        public string RecipeName { get { return call.Context == null ? "" : call.Context.RecipeName; } }
        public string CycleId { get { return call.Context == null ? "" : call.Context.CycleId; } }
        public string InvocationId { get { return call.InvocationId; } }

        /// <summary>建立只属于当前节点调用的取消及资源访问范围。</summary>
        internal FlowCodeContext(FlowCall call, FlowCodeDefinition definition)
        {
            this.call = call; this.definition = definition;
            scope = CancellationTokenSource.CreateLinkedTokenSource(call.Token);
            Token = scope.Token;
        }

        /// <summary>选择节点声明并绑定的运动轴，不返回底层控件。</summary>
        public FlowCodeAxis Axis(string slot) { RequireRole(slot, "axis"); return new FlowCodeAxis(this, slot); }
        /// <summary>选择节点声明并绑定的输入。</summary>
        public FlowCodeInput Input(string slot) { RequireRole(slot, "input"); return new FlowCodeInput(this, slot); }
        /// <summary>选择节点声明并绑定的输出。</summary>
        public FlowCodeOutput Output(string slot) { RequireRole(slot, "output"); return new FlowCodeOutput(this, slot); }
        /// <summary>选择节点声明并绑定的视觉 VPP。</summary>
        public FlowCodeVision Vision(string slot) { RequireRole(slot, "vision"); return new FlowCodeVision(this, slot); }

        /// <summary>调用已声明角色的方法，宿主按真实对象的契约检查参数和资源租约。</summary>
        public FlowCodeCall<IDictionary<string, object>> CallAsync(string slot, string methodId, IDictionary<string, object> inputs = null)
        { return Invoke<IDictionary<string, object>>(slot, methodId, inputs, result => new Dictionary<string, object>(result, StringComparer.Ordinal)); }

        /// <summary>在当前取消范围内等待，不创建后台业务任务。</summary>
        public Task DelayAsync(int milliseconds) { return Task.Delay(milliseconds, Token); }

        /// <summary>先登记再发起资源调用，类型代理与原始调用使用同一追踪机制。</summary>
        internal FlowCodeCall<T> Invoke<T>(string slot, string methodId, IDictionary<string, object> inputs, Func<IDictionary<string, object>, T> read)
        {
            RequireRole(slot, null);
            if (string.IsNullOrWhiteSpace(methodId) || read == null) throw new ArgumentException("A method identity and result reader are required.");
            TaskCompletionSource<T> completion = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            FlowCodeCall<T> pending = new FlowCodeCall<T>(completion.Task);
            Dictionary<string, object> values = inputs == null ? new Dictionary<string, object>() : new Dictionary<string, object>(inputs, StringComparer.Ordinal);
            lock (sync)
            {
                if (closed != null) throw new InvalidOperationException("The code node resource scope is closed.");
                Token.ThrowIfCancellationRequested();
                if (call.InvokeResourceAsync == null) throw new InvalidOperationException("The controlled resource provider is missing.");
                calls.Add(pending);
            }
            _ = RunCall(slot, methodId, values, read, completion);
            return pending;
        }

        /// <summary>等待实际资源方法结束后才完成任务；取消不提前伪造硬件结束。</summary>
        private async Task RunCall<T>(string slot, string methodId, IDictionary<string, object> inputs, Func<IDictionary<string, object>, T> read, TaskCompletionSource<T> completion)
        {
            try
            {
                Token.ThrowIfCancellationRequested();
                IDictionary<string, object> result = await call.InvokeResourceAsync(slot, methodId, inputs, Token).ConfigureAwait(false);
                Token.ThrowIfCancellationRequested();
                if (result == null) throw new InvalidOperationException("The resource method returned no result: " + slot + "/" + methodId);
                completion.TrySetResult(read(result));
            }
            catch (OperationCanceledException) { CancelWhenClosing(); completion.TrySetCanceled(); }
            catch (Exception error) { CancelWhenClosing(); completion.TrySetException(error); }
        }

        /// <summary>只允许声明的角色及相容代理，缺失对象不会按名称猜测替代。</summary>
        private void RequireRole(string name, string capability)
        {
            FlowCodeResourceSlot slot = definition.ResourceSlots.SingleOrDefault(item => item.Name == name);
            if (slot == null || (capability != null && slot.Capability != capability)) throw new InvalidOperationException("Invalid resource role or capability: " + name);
            FlowResourceBinding binding = call.Step.ResourceBindings.SingleOrDefault(item => item.Slot == name);
            if (binding == null || string.IsNullOrWhiteSpace(binding.Target)) throw new InvalidOperationException("Resource role is not bound: " + name);
        }

        /// <summary>读取已校验的业务端口映射，不向模块暴露 ToolBlock 或端口对象。</summary>
        internal string MappedField(string slot, string name, string direction)
        {
            FlowResourceBinding binding = call.Step.ResourceBindings.Single(item => item.Slot == slot);
            FlowResourceFieldBinding field = binding.Fields.SingleOrDefault(item => item.Name == name && item.Direction == direction);
            if (field == null) throw new InvalidOperationException("Resource field is not mapped: " + slot + "/" + name);
            return field.Field;
        }

        /// <summary>将视觉输出按角色的业务字段生成独立值快照。</summary>
        internal IDictionary<string, object> MapVisionOutputs(string slot, IDictionary<string, object> result)
        {
            Dictionary<string, object> values = new Dictionary<string, object>(result, StringComparer.Ordinal);
            FlowCodeResourceSlot role = definition.ResourceSlots.Single(item => item.Name == slot);
            foreach (FlowCodeResourceFieldDefinition field in role.Fields.Where(item => item.Direction == "Output"))
            {
                string source = MappedField(slot, field.Name, "Output");
                object value;
                if (!result.TryGetValue(source, out value)) throw new InvalidOperationException("Vision output is missing: " + source);
                values[field.Name] = new FlowField(field.Name, field.Type).ConvertValue(value);
            }
            return values;
        }

        /// <summary>关闭调用入口并收拢全部真实任务，已被业务捕获的历史故障不再次抛出。</summary>
        internal Task CloseAsync(bool cancel)
        {
            IFlowCodePendingCall[] pending = null;
            lock (sync)
            {
                if (closed == null)
                {
                    closed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                    pending = calls.ToArray();
                }
            }
            if (cancel) CancelScope();
            if (pending != null) _ = DrainCalls(pending);
            return closed.Task;
        }

        /// <summary>等待所有子调用，遗漏 await 的故障阻止输出发布，非合作调用仍必须实际结束。</summary>
        private async Task DrainCalls(IFlowCodePendingCall[] pending)
        {
            foreach (IFlowCodePendingCall item in pending)
                if (!item.Observed && (item.Task.IsFaulted || item.Task.IsCanceled)) CancelScope();
            try { await Task.WhenAll(pending.Select(item => item.Task)).ConfigureAwait(false); }
            catch { /* 下方按观察状态逐项报告，已处理故障不重复传播。 */ }
            List<Exception> errors = pending.Where(item => !item.Observed && item.Task.IsFaulted).SelectMany(item => item.Task.Exception.InnerExceptions).ToList();
            lock (sync) if (cancellationFailure != null) errors.Add(cancellationFailure);
            if (errors.Count > 0) { closed.TrySetException(errors); return; }
            if (pending.Any(item => item.Task.IsCanceled)) { closed.TrySetCanceled(); return; }
            closed.TrySetResult(true);
        }

        /// <summary>收拢期间新发生故障时取消其他子调用，正常执行中的可捕获故障允许业务重试。</summary>
        private void CancelWhenClosing()
        {
            bool closing;
            lock (sync) closing = closed != null;
            if (closing) CancelScope();
        }

        /// <summary>发送取消并记录回调故障，不允许回调异常跳过实际任务收拢。</summary>
        private void CancelScope()
        {
            try { scope.Cancel(); }
            catch (Exception error) { lock (sync) cancellationFailure = cancellationFailure == null ? error : new AggregateException(cancellationFailure, error); }
        }

        /// <summary>真实子调用收拢后释放本次取消范围。</summary>
        public void Dispose() { scope.Dispose(); }
    }
}
