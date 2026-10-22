using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AlphaRap.FlowRuntime
{
    public sealed class FlowNotice
    {
        public string NodeId;
        public string State;
        public string AlarmCode = "";
        public string Detail = "";
        public IReadOnlyDictionary<string, object> Outputs;
    }

    public sealed class FlowRunResult
    {
        public string RunId;
        public string State;
        public FlowIssue Error;
        public readonly Dictionary<string, IReadOnlyDictionary<string, object>> Results = new Dictionary<string, IReadOnlyDictionary<string, object>>();
    }

    public sealed class FlowRunner
    {
        private int running;
        public event Action<FlowNotice> Notice;

        /// <summary>独占执行冻结的流程，输出按运行与节点隔离，停止后不派发下一动作。</summary>
        public async Task<FlowRunResult> RunAsync(FlowDefinition definition, ActionCatalog catalog, string outputDirectory, CancellationToken stop)
        {
            if (Interlocked.CompareExchange(ref running, 1, 0) != 0) throw new FlowIssue("Busy");
            var result = new FlowRunResult { RunId = Guid.NewGuid().ToString("N"), State = "Running" };
            try
            {
                var flow = FlowFiles.Snapshot(definition);
                FlowValidation.Validate(flow, catalog);
                var nodes = flow.Nodes.ToDictionary(n => n.Id);
                var devices = flow.Devices.ToDictionary(d => d.Id);
                DateTime startedAt = DateTime.Now;
                string current = flow.StartNode;
                while (!string.IsNullOrEmpty(current))
                {
                    stop.ThrowIfCancellationRequested();
                    var node = nodes[current];
                    var spec = catalog.Find(node.ActionId);
                    var call = CreateCall(node, spec, result, devices, outputDirectory, startedAt, flow.Version);
                    var outcome = await Execute(call, spec, stop).ConfigureAwait(false);
                    var snapshot = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>(outcome.Outputs));
                    result.Results.Add(current, snapshot);
                    Publish(new FlowNotice { NodeId = current, State = "Completed", Outputs = snapshot });
                    stop.ThrowIfCancellationRequested();
                    current = spec.Branch && !outcome.Pass ? node.FalseNext : node.Next;
                }
                result.State = "Succeeded";
            }
            catch (OperationCanceledException) { result.State = "Cancelled"; }
            catch (FlowIssue error) { result.State = "Failed"; result.Error = error; }
            catch (Exception error) { result.State = "Failed"; result.Error = new FlowIssue("ActionFailed", error.Message); }
            finally { Interlocked.Exchange(ref running, 0); }
            return result;
        }

        /// <summary>解析已验证绑定，创建不共享编辑状态的动作调用。</summary>
        private static ActionCall CreateCall(FlowNode node, ActionSpec spec, FlowRunResult run, Dictionary<string, SimDevice> devices, string directory, DateTime started, string version)
        {
            var inputs = new Dictionary<string, object>(StringComparer.Ordinal);
            foreach (var binding in node.Inputs)
            {
                var field = spec.Inputs.FirstOrDefault(i => i.Name == binding.Name);
                if (string.IsNullOrEmpty(binding.Source)) inputs.Add(binding.Name, FlowFiles.Parse(binding.Value, field == null ? ValueKind.Text : field.Kind));
                else
                {
                    string[] parts = binding.Source.Split('.');
                    object value;
                    if (!run.Results[parts[0]].TryGetValue(parts[1], out value)) throw new FlowIssue("InvalidBinding", node.Id, binding.Source);
                    inputs.Add(binding.Name, value);
                }
            }
            return new ActionCall { RunId = run.RunId, StartedAt = started, Version = version, OutputDirectory = directory, Node = node,
                Device = spec.TargetKind.HasValue ? devices[node.TargetId] : null, Inputs = new ReadOnlyDictionary<string, object>(inputs) };
        }

        /// <summary>执行一次动作并协作超时；失败时发布节点绑定的仿真报警并终止流程。</summary>
        private async Task<ActionOutcome> Execute(ActionCall call, ActionSpec spec, CancellationToken stop)
        {
            Publish(new FlowNotice { NodeId = call.Node.Id, State = "Running" });
            using (var deadline = CancellationTokenSource.CreateLinkedTokenSource(stop))
            {
                deadline.CancelAfter(call.Node.TimeoutMs);
                try
                {
                    var outcome = await spec.Execute(call, deadline.Token).ConfigureAwait(false);
                    foreach (var field in spec.Outputs)
                    {
                        object value;
                        if (!outcome.Outputs.TryGetValue(field.Name, out value) || !Matches(value, field.Kind)) throw new InvalidOperationException("Output contract: " + field.Name);
                    }
                    return outcome;
                }
                catch (OperationCanceledException)
                {
                    if (stop.IsCancellationRequested) { Publish(new FlowNotice { NodeId = call.Node.Id, State = "Cancelled" }); throw; }
                    Publish(new FlowNotice { NodeId = call.Node.Id, State = "TimedOut", AlarmCode = call.Node.AlarmCode });
                    throw new FlowIssue("Timeout", call.Node.Id);
                }
                catch (Exception error)
                {
                    Publish(new FlowNotice { NodeId = call.Node.Id, State = "Failed", AlarmCode = call.Node.AlarmCode, Detail = error.Message });
                    throw new FlowIssue("ActionFailed", call.Node.Id + ": " + error.Message);
                }
            }
        }

        /// <summary>校验扩展动作发布的输出类型及有限数值。</summary>
        private static bool Matches(object value, ValueKind kind)
        {
            if (kind == ValueKind.Text) return value is string;
            if (kind == ValueKind.Boolean) return value is bool;
            if (kind == ValueKind.Integer) return value is int;
            return value is double && !double.IsNaN((double)value) && !double.IsInfinity((double)value);
        }

        /// <summary>发布不可影响调度的观察通知。</summary>
        private void Publish(FlowNotice notice)
        {
            var handler = Notice;
            if (handler == null) return;
            foreach (Action<FlowNotice> subscriber in handler.GetInvocationList())
                try { subscriber(notice); } catch (Exception error) { System.Diagnostics.Trace.WriteLine(error); }
        }
    }
}
