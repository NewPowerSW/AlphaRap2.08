using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace NPSDK.Design
{
    public sealed class FlowChartNotice
    {
        public string NodeId;
        public string State;
        public string AlarmCode = "";
        public string Detail = "";
        public FlowDesignIssue Issue;
        public Dictionary<string, object> Values;
    }

    public sealed class FlowChartSession : IFlowChartExecutionScope
    {
        private sealed class Step
        {
            public readonly Stopwatch Clock = Stopwatch.StartNew();
            public FlowInvocation Call;
            public FlowActionBinding Binding;
            public bool Started;
        }

        private readonly Dictionary<string, Step> steps = new Dictionary<string, Step>();
        private readonly List<FlowInvocation> calls = new List<FlowInvocation>();
        private readonly FlowActionRegistry registry;
        private readonly Dictionary<string, Dictionary<string, object>> results = new Dictionary<string, Dictionary<string, object>>();
        private readonly object sync = new object ();
        private bool stopped;
        private bool paused;
        public string RunId { get; private set; }
        public string OutputDirectory { get; private set; }
        public string State { get; private set; }

        public bool StopRequested
        {
            get
            {
                lock (sync)
                    return stopped;
            }
        }

        public bool PauseRequested
        {
            get
            {
                lock (sync)
                    return paused || ExternalPause != null && ExternalPause();
            }
        }

        public Func<bool> ExternalPause;
        public bool PendingWork
        {
            get
            {
                lock (sync)
                    return calls.Any(c => c.Pending != null && !c.Pending.IsCompleted);
            }
        }

        public event Action<FlowChartNotice> Notice;
        /// <summary>创建绑定 SDK 节点回调的单次执行范围。</summary>
        public FlowChartSession(FlowActionRegistry registry, string directory)
        {
            this.registry = registry;
            OutputDirectory = directory;
            RunId = Guid.NewGuid().ToString("N");
            State = "Running";
        }

        /// <summary>只处理动作与判据，实际等待和 Next/Case 跳转仍由 Flow_Chart 执行。</summary>
        public Flow_Chart.ResultType Tick(FlowDesignNode node)
        {
            lock (sync)
            {
                if (stopped)
                    return Flow_Chart.ResultType.End;
                try
                {
                    return TickCore(node);
                }
                catch (Exception error)
                {
                    Fail(node, "Failed", error.Message, error as FlowDesignIssue);
                    return Flow_Chart.ResultType.End;
                }
            }
        }

        /// <summary>每次进入只触发一次动作，等待中持续采样条件。</summary>
        private Flow_Chart.ResultType TickCore(FlowDesignNode node)
        {
            Step step;
            if (!steps.TryGetValue(node.Id, out step))
            {
                calls.RemoveAll(call => call.Pending == null || call.Pending.IsCompleted);
                step = new Step
                {
                    Binding = registry.Find(node.MethodName, node.Target)
                };
                step.Call = new FlowInvocation
                {
                    RunId = RunId,
                    OutputDirectory = OutputDirectory,
                    Node = node,
                    Inputs = ResolveInputs(node)
                };
                steps.Add(node.Id, step);
                calls.Add(step.Call);
                Publish(node.Id, "Running");
            }

            if (node.Timeout > 0 && step.Clock.ElapsedMilliseconds >= node.Timeout)
            {
                Fail(node, "TimedOut", "");
                return Flow_Chart.ResultType.End;
            }

            if (step.Clock.ElapsedMilliseconds < node.Delay)
                return Flow_Chart.ResultType.IDLE;
            if (!step.Started)
            {
                step.Started = true;
                if (step.Binding.Start != null)
                    step.Binding.Start(step.Call);
            }

            if (stopped)
                return Flow_Chart.ResultType.End;
            var sample = step.Binding.Poll(step.Call);
            if (sample.Waiting)
                return Flow_Chart.ResultType.IDLE;
            if (!Satisfied(node, sample.Values))
            {
                if (node.WhenFalse == FlowUnmetAction.Wait)
                    return Flow_Chart.ResultType.IDLE;
                results[node.Id] = new Dictionary<string, object>(sample.Values);
                steps.Remove(node.Id);
                Publish(node.Id, "False", sample.Values);
                if (node.WhenFalse == FlowUnmetAction.End)
                {
                    State = "Completed";
                    return Flow_Chart.ResultType.End;
                }

                return node.WhenFalse == FlowUnmetAction.Case1 ? Flow_Chart.ResultType.CASE1 : node.WhenFalse == FlowUnmetAction.Case2 ? Flow_Chart.ResultType.CASE2 : Flow_Chart.ResultType.CASE3;
            }

            results[node.Id] = new Dictionary<string, object>(sample.Values);
            steps.Remove(node.Id);
            Publish(node.Id, "Completed", sample.Values);
            if (!string.IsNullOrEmpty(node.Next))
                return Flow_Chart.ResultType.NEXT;
            State = "Completed";
            return Flow_Chart.ResultType.End;
        }

        /// <summary>解析本轮已发布的输出，禁止使用前一轮结果。</summary>
        private Dictionary<string, object> ResolveInputs(FlowDesignNode node)
        {
            var inputs = new Dictionary<string, object>(StringComparer.Ordinal);
            foreach (var parameter in node.Parameters)
            {
                object value = string.IsNullOrEmpty(parameter.Source) ? FlowDesignFiles.Parse(parameter.Type, parameter.Value) : Read(parameter.Source);
                inputs.Add(parameter.Name, FlowDesignFiles.Parse(parameter.Type, Convert.ToString(value, CultureInfo.InvariantCulture)));
            }

            return inputs;
        }

        /// <summary>读取已完成节点的本次输出。</summary>
        private object Read(string source)
        {
            string[] parts = source.Split('.');
            Dictionary<string, object> values;
            object value;
            if (parts.Length != 2 || !results.TryGetValue(parts[0], out values) || !values.TryGetValue(parts[1], out value))
                throw new FlowDesignIssue("MissingData", source);
            return value;
        }

        /// <summary>对当前采样或前驱快照执行受限比较。</summary>
        private bool Satisfied(FlowDesignNode node, Dictionary<string, object> values)
        {
            if (node.Comparison == FlowComparison.Always)
                return true;
            object value;
            string source = node.ConditionSource.StartsWith(node.Id + ".", StringComparison.Ordinal) ? node.ConditionSource.Substring(node.Id.Length + 1) : node.ConditionSource;
            if (!values.TryGetValue(source, out value))
                value = Read(node.ConditionSource);
            if (node.Comparison == FlowComparison.NotEmpty)
                return !string.IsNullOrWhiteSpace(Convert.ToString(value));
            if (node.Comparison == FlowComparison.IsTrue)
                return Convert.ToBoolean(value, CultureInfo.InvariantCulture);
            if (node.Comparison == FlowComparison.IsFalse)
                return !Convert.ToBoolean(value, CultureInfo.InvariantCulture);
            if (node.Comparison == FlowComparison.Equal)
                return Convert.ToString(value, CultureInfo.InvariantCulture) == node.Expected;
            if (node.Comparison == FlowComparison.NotEqual)
                return Convert.ToString(value, CultureInfo.InvariantCulture) != node.Expected;
            double number = (double)FlowDesignFiles.Parse("Double", Convert.ToString(value, CultureInfo.InvariantCulture));
            if (node.Comparison == FlowComparison.InRange)
            {
                string[] range = node.Expected.Split(',');
                if (range.Length != 2)
                    throw new FormatException("min,max");
                double min = (double)FlowDesignFiles.Parse("Double", range[0]), max = (double)FlowDesignFiles.Parse("Double", range[1]);
                if (min > max)
                    throw new FormatException("min > max");
                return number >= min && number <= max;
            }

            double expected = (double)FlowDesignFiles.Parse("Double", node.Expected);
            return node.Comparison == FlowComparison.Greater ? number > expected : number < expected;
        }

        /// <summary>超时或异常停止本范围，并真正调用 NPSDK.Alarm.Show。</summary>
        private void Fail(FlowDesignNode node, string state, string detail, FlowDesignIssue issue = null)
        {
            State = state;
            Stop();
            if (!string.IsNullOrEmpty(node.AlarmCode))
                Alarm.Show(node.AlarmCode, string.IsNullOrEmpty(node.AlarmContent) ? null : node.AlarmContent);
            var handler = Notice;
            if (handler != null)
                Notify(handler, new FlowChartNotice { NodeId = node.Id, State = state, AlarmCode = node.AlarmCode, Detail = detail, Issue = issue });
        }

        /// <summary>停止后不再触发新动作，已开始操作执行各自取消策略。</summary>
        public void Stop()
        {
            lock (sync)
            {
                stopped = true;
                paused = false;
                if (State == "Running")
                    State = "Stopped";
                foreach (var step in steps.Values)
                {
                    step.Call.Cancelled = true;
                    try
                    {
                        if (step.Started && step.Binding.Cancel != null)
                            step.Binding.Cancel(step.Call);
                    }
                    catch (Exception error)
                    {
                        Trace.WriteLine(error);
                    }
                }
            }
        }

        /// <summary>暂停由 SDK 的节点循环执行，当前阻塞操作需要自身提供取消能力。</summary>
        public void Pause(bool value)
        {
            lock (sync)
                paused = value;
        }

        /// <summary>发布节点观察事件，不调度下一节点。</summary>
        private void Publish(string id, string state, Dictionary<string, object> values = null)
        {
            var handler = Notice;
            if (handler != null)
                Notify(handler, new FlowChartNotice { NodeId = id, State = state, Values = values == null ? null : new Dictionary<string, object>(values) });
        }

        /// <summary>观察者异常不改变已触发的设备动作和 SDK 跳转结果。</summary>
        private static void Notify(Action<FlowChartNotice> handler, FlowChartNotice notice)
        {
            foreach (Action<FlowChartNotice> observer in handler.GetInvocationList())
                try
                {
                    observer(notice);
                }
                catch (Exception error)
                {
                    Trace.WriteLine(error);
                }
        }
    }
}
