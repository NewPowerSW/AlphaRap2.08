using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace NPSDK
{
    /// <summary>代码业务实例的执行边界；模块目录只允许具体泛型派生类。</summary>
    public abstract class FlowCodeNode
    {
        /// <summary>按声明的输入输出契约执行一个独立节点调用。</summary>
        internal abstract Task<FlowActionOutcome> RunAsync(FlowCodeContext context, IDictionary<string, object> values, FlowCodeDefinition definition);
    }

    /// <summary>类型化代码节点；一次进入、IDLE 检查和取消收尾都属于同一次调用。</summary>
    public abstract class FlowCodeNode<TInputs, TOutputs> : FlowCodeNode
        where TInputs : class, new()
        where TOutputs : class, new()
    {
        /// <summary>进入节点时执行一次；适合发起业务动作，不能放入重复等待检查中。</summary>
        protected virtual Task OnEnterAsync(FlowCodeContext context, TInputs inputs, TOutputs outputs) { return Task.FromResult(0); }

        /// <summary>同步业务检查；返回 IDLE 后适配器稍后再次检查，其他结果结束本次动作。</summary>
        protected virtual Flow_Chart.ResultType Execute(FlowCodeContext context, TInputs inputs, TOutputs outputs)
        { throw new InvalidOperationException("Override Execute or ExecuteAsync."); }

        /// <summary>异步业务检查；默认调用同步实现，资源访问应使用 context 的受控接口。</summary>
        protected virtual Task<Flow_Chart.ResultType> ExecuteAsync(FlowCodeContext context, TInputs inputs, TOutputs outputs)
        { return Task.FromResult(Execute(context, inputs, outputs)); }

        /// <summary>取消且实际子调用收拢后通知一次；此时资源入口已关闭，仅作本地业务收尾。</summary>
        protected virtual Task OnCancelAsync(FlowCodeContext context, TInputs inputs, TOutputs outputs) { return Task.FromResult(0); }

        /// <summary>转换输入、进入一次并轮询等待；所有子调用结束后才返回类型化输出和最终出口。</summary>
        internal sealed override async Task<FlowActionOutcome> RunAsync(FlowCodeContext context, IDictionary<string, object> values, FlowCodeDefinition definition)
        {
            TInputs inputs = ReadInputs(values, definition.Inputs);
            TOutputs outputs = new TOutputs();
            Flow_Chart.ResultType exit = Flow_Chart.ResultType.IDLE;
            Exception failure = null;
            try
            {
                context.Token.ThrowIfCancellationRequested();
                await OnEnterAsync(context, inputs, outputs).ConfigureAwait(false);
                do
                {
                    context.Token.ThrowIfCancellationRequested();
                    exit = await ExecuteAsync(context, inputs, outputs).ConfigureAwait(false);
                    if (exit == Flow_Chart.ResultType.IDLE) await Task.Delay(25, context.Token).ConfigureAwait(false);
                } while (exit == Flow_Chart.ResultType.IDLE);
            }
            catch (Exception error) { failure = error; }
            try { await context.CloseAsync(failure != null).ConfigureAwait(false); }
            catch (Exception error) { failure = Combine(failure, error); }
            if (context.Token.IsCancellationRequested)
            {
                try { await OnCancelAsync(context, inputs, outputs).ConfigureAwait(false); }
                catch (Exception error) { failure = Combine(failure, error); }
            }
            if (failure != null) ExceptionDispatchInfo.Capture(failure).Throw();
            context.Token.ThrowIfCancellationRequested();
            string code = ExitCode(exit);
            if (code != "End" && !definition.Exits.Contains(code)) throw new InvalidOperationException("Undeclared code module exit: " + definition.Id + " / " + code);
            return new FlowActionOutcome { Outputs = ReadOutputs(outputs, definition.Outputs), BranchCode = code };
        }

        /// <summary>只接受与声明一致的输入，不允许漏字段或静默使用 CLR 初值。</summary>
        private static TInputs ReadInputs(IDictionary<string, object> values, List<FlowField> fields)
        {
            if (values == null || values.Count != fields.Count || fields.Any(field => !values.ContainsKey(field.Name)))
                throw new InvalidOperationException("Code module input fields changed.");
            TInputs input = new TInputs();
            foreach (MemberInfo member in FlowCodeCatalog.Members(typeof(TInputs)))
            {
                FlowField field = fields.Single(item => item.Name == member.Name);
                object value = field.ConvertValue(values[field.Name]);
                FieldInfo storage = member as FieldInfo;
                if (storage != null) storage.SetValue(input, value);
                else ((PropertyInfo)member).SetValue(input, value, null);
            }
            return input;
        }

        /// <summary>输出快照只包含简单业务值，拒绝非有限数值，保持节点间数据独立。</summary>
        private static IDictionary<string, object> ReadOutputs(TOutputs outputs, List<FlowField> fields)
        {
            Dictionary<string, object> values = new Dictionary<string, object>(StringComparer.Ordinal);
            foreach (MemberInfo member in FlowCodeCatalog.Members(typeof(TOutputs)))
            {
                FieldInfo storage = member as FieldInfo;
                object value = storage == null ? ((PropertyInfo)member).GetValue(outputs, null) : storage.GetValue(outputs);
                values.Add(member.Name, fields.Single(field => field.Name == member.Name).ConvertValue(value));
            }
            return values;
        }

        /// <summary>把原 FlowChat 返回值转换成运行器的最终出口，不将 IDLE 当业务分支。</summary>
        private static string ExitCode(Flow_Chart.ResultType exit)
        {
            switch (exit)
            {
                case Flow_Chart.ResultType.NEXT: return "NEXT";
                case Flow_Chart.ResultType.CASE1: return "CASE1";
                case Flow_Chart.ResultType.CASE2: return "CASE2";
                case Flow_Chart.ResultType.CASE3: return "CASE3";
                case Flow_Chart.ResultType.End: return "End";
                default: throw new InvalidOperationException("Invalid code module result: " + exit);
            }
        }

        /// <summary>保留主失败和收尾失败，避免取消清理错误掩盖原始故障。</summary>
        private static Exception Combine(Exception first, Exception second)
        {
            if (first == null) return second;
            if (OnlyCancellation(first) && OnlyCancellation(second)) return new OperationCanceledException("Code module was cancelled.", first);
            return new AggregateException(first, second);
        }

        /// <summary>识别正文和任务收拢产生的同一次纯取消，保留真实故障的聚合信息。</summary>
        private static bool OnlyCancellation(Exception error)
        {
            AggregateException aggregate = error as AggregateException;
            return aggregate == null ? error is OperationCanceledException : aggregate.Flatten().InnerExceptions.All(item => item is OperationCanceledException);
        }
    }
}
