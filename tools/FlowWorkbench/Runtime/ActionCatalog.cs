using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AlphaRap.FlowRuntime
{
    public sealed class FieldSpec
    {
        public string Name;
        public ValueKind Kind;
        public string Default;

        /// <summary>定义动作输入输出字段及常量默认值。</summary>
        public FieldSpec(string name, ValueKind kind, string value = "") { Name = name; Kind = kind; Default = value; }
    }

    public sealed class ActionSpec
    {
        public string Id;
        public DeviceKind? TargetKind;
        public bool Branch;
        public bool ExtraInputs;
        public FieldSpec[] Inputs;
        public FieldSpec[] Outputs;
        public Func<ActionCall, CancellationToken, Task<ActionOutcome>> Execute;
    }

    public sealed class ActionCall
    {
        public string RunId;
        public DateTime StartedAt;
        public string Version;
        public string OutputDirectory;
        public FlowNode Node;
        public SimDevice Device;
        public IReadOnlyDictionary<string, object> Inputs;
    }

    public sealed class ActionOutcome
    {
        public bool Pass = true;
        public readonly Dictionary<string, object> Outputs = new Dictionary<string, object>(StringComparer.Ordinal);

        /// <summary>创建带固定名称输出的动作结果。</summary>
        public static ActionOutcome Of(params object[] pairs)
        {
            var result = new ActionOutcome();
            for (int i = 0; i < pairs.Length; i += 2) result.Outputs.Add((string)pairs[i], pairs[i + 1]);
            return result;
        }
    }

    public sealed class ActionCatalog
    {
        private readonly Dictionary<string, ActionSpec> actions = new Dictionary<string, ActionSpec>(StringComparer.Ordinal);
        public IEnumerable<ActionSpec> All { get { return actions.Values; } }

        /// <summary>登记经过审核的动作；不允许重复标识或任意反射执行。</summary>
        public void Register(ActionSpec spec)
        {
            if (spec == null || string.IsNullOrWhiteSpace(spec.Id) || spec.Execute == null) throw new ArgumentException("Invalid action");
            actions.Add(spec.Id, spec);
        }

        /// <summary>按稳定动作标识查找执行契约。</summary>
        public ActionSpec Find(string id) { ActionSpec action; return id != null && actions.TryGetValue(id, out action) ? action : null; }

        /// <summary>创建不包含真实硬件驱动的标准测试动作目录。</summary>
        public static ActionCatalog Simulation()
        {
            var catalog = new ActionCatalog();
            catalog.Register(new ActionSpec { Id = "trigger.manual", Inputs = new[] { new FieldSpec("Barcode", ValueKind.Text, "DEMO-001") }, Outputs = new[] { new FieldSpec("Barcode", ValueKind.Text) }, Execute = Trigger });
            catalog.Register(new ActionSpec { Id = "io.wait", TargetKind = DeviceKind.Input, Inputs = new[] { new FieldSpec("Expected", ValueKind.Boolean, "true") }, Outputs = new[] { new FieldSpec("State", ValueKind.Boolean) }, Execute = WaitInput });
            catalog.Register(new ActionSpec { Id = "io.read", TargetKind = DeviceKind.Input, Inputs = new FieldSpec[0], Outputs = new[] { new FieldSpec("State", ValueKind.Boolean) }, Execute = ReadInput });
            catalog.Register(new ActionSpec { Id = "io.write", TargetKind = DeviceKind.Output, Inputs = new[] { new FieldSpec("State", ValueKind.Boolean, "true") }, Outputs = new[] { new FieldSpec("State", ValueKind.Boolean) }, Execute = WriteOutput });
            catalog.Register(new ActionSpec { Id = "module.move", TargetKind = DeviceKind.Module, Inputs = new[] { new FieldSpec("X", ValueKind.Number, "10"), new FieldSpec("Y", ValueKind.Number, "20") }, Outputs = new[] { new FieldSpec("X", ValueKind.Number), new FieldSpec("Y", ValueKind.Number) }, Execute = Move });
            catalog.Register(new ActionSpec { Id = "vision.run", TargetKind = DeviceKind.Vision, Inputs = new FieldSpec[0], Outputs = new[] { new FieldSpec("X", ValueKind.Number), new FieldSpec("Y", ValueKind.Number), new FieldSpec("Angle", ValueKind.Number) }, Execute = Vision });
            catalog.Register(new ActionSpec { Id = "condition.range", Branch = true, Inputs = new[] { new FieldSpec("Value", ValueKind.Number, "0"), new FieldSpec("Min", ValueKind.Number, "-1"), new FieldSpec("Max", ValueKind.Number, "1") }, Outputs = new[] { new FieldSpec("Passed", ValueKind.Boolean) }, Execute = Range });
            catalog.Register(new ActionSpec { Id = "condition.equal", Branch = true, Inputs = new[] { new FieldSpec("Left", ValueKind.Boolean, "false"), new FieldSpec("Right", ValueKind.Boolean, "true") }, Outputs = new[] { new FieldSpec("Passed", ValueKind.Boolean) }, Execute = Equal });
            catalog.Register(new ActionSpec { Id = "flow.note", Inputs = new[] { new FieldSpec("Message", ValueKind.Text, "OK") }, Outputs = new[] { new FieldSpec("Message", ValueKind.Text) }, Execute = Note });
            catalog.Register(new ActionSpec { Id = "data.save", ExtraInputs = true, Inputs = new[] { new FieldSpec("Barcode", ValueKind.Text), new FieldSpec("X", ValueKind.Number, "0"), new FieldSpec("Y", ValueKind.Number, "0"), new FieldSpec("Angle", ValueKind.Number, "0"), new FieldSpec("Passed", ValueKind.Boolean, "true") }, Outputs = new[] { new FieldSpec("RecordId", ValueKind.Text), new FieldSpec("Path", ValueKind.Text) }, Execute = Save });
            return catalog;
        }

        /// <summary>返回本次手动触发的条码输入。</summary>
        private static Task<ActionOutcome> Trigger(ActionCall call, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return Task.FromResult(ActionOutcome.Of("Barcode", call.Inputs["Barcode"]));
        }

        /// <summary>等待仿真输入，状态不满足时持续等待至取消或超时。</summary>
        private static async Task<ActionOutcome> WaitInput(ActionCall call, CancellationToken token)
        {
            await Task.Delay(call.Device.DelayMs, token).ConfigureAwait(false);
            while (call.Device.State != (bool)call.Inputs["Expected"]) await Task.Delay(20, token).ConfigureAwait(false);
            return ActionOutcome.Of("State", call.Device.State);
        }

        /// <summary>返回仿真输入的一次有效状态采样。</summary>
        private static async Task<ActionOutcome> ReadInput(ActionCall call, CancellationToken token)
        {
            await Task.Delay(call.Device.DelayMs, token).ConfigureAwait(false);
            return ActionOutcome.Of("State", call.Device.State);
        }

        /// <summary>设置本轮隔离的仿真输出，返回设置结果。</summary>
        private static async Task<ActionOutcome> WriteOutput(ActionCall call, CancellationToken token)
        {
            await Task.Delay(call.Device.DelayMs, token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            call.Device.State = (bool)call.Inputs["State"];
            return ActionOutcome.Of("State", call.Device.State);
        }

        /// <summary>模拟模组到位，不修改共享设备配置。</summary>
        private static async Task<ActionOutcome> Move(ActionCall call, CancellationToken token)
        {
            await Task.Delay(call.Device.DelayMs, token).ConfigureAwait(false);
            return ActionOutcome.Of("X", call.Inputs["X"], "Y", call.Inputs["Y"]);
        }

        /// <summary>返回所选仿真视觉实例的本次坐标快照。</summary>
        private static async Task<ActionOutcome> Vision(ActionCall call, CancellationToken token)
        {
            await Task.Delay(call.Device.DelayMs, token).ConfigureAwait(false);
            return ActionOutcome.Of("X", call.Device.X, "Y", call.Device.Y, "Angle", call.Device.Angle);
        }

        /// <summary>按闭区间判断数值并选择真假出口。</summary>
        private static Task<ActionOutcome> Range(ActionCall call, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            double min = (double)call.Inputs["Min"], max = (double)call.Inputs["Max"];
            if (min > max) throw new InvalidOperationException("Min > Max");
            double value = (double)call.Inputs["Value"];
            bool pass = value >= min && value <= max;
            var outcome = ActionOutcome.Of("Passed", pass);
            outcome.Pass = pass;
            return Task.FromResult(outcome);
        }

        /// <summary>比较布尔输入并选择真假出口。</summary>
        private static Task<ActionOutcome> Equal(ActionCall call, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            bool pass = (bool)call.Inputs["Left"] == (bool)call.Inputs["Right"];
            var outcome = ActionOutcome.Of("Passed", pass);
            outcome.Pass = pass;
            return Task.FromResult(outcome);
        }

        /// <summary>传递分支说明文本，供运行记录显示。</summary>
        private static Task<ActionOutcome> Note(ActionCall call, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return Task.FromResult(ActionOutcome.Of("Message", call.Inputs["Message"]));
        }

        /// <summary>以每次运行和节点为记录身份，原子写入 UTF-8 CSV。</summary>
        private static Task<ActionOutcome> Save(ActionCall call, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            string recordId = call.RunId + "-" + call.Node.Id;
            string directory = Path.Combine(call.OutputDirectory, call.StartedAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, recordId + ".csv");
            var values = new List<KeyValuePair<string, object>> {
                new KeyValuePair<string, object>("RecordId", recordId), new KeyValuePair<string, object>("RunId", call.RunId),
                new KeyValuePair<string, object>("StartedAt", call.StartedAt.ToString("O", CultureInfo.InvariantCulture)),
                new KeyValuePair<string, object>("FlowVersion", call.Version) };
            values.AddRange(call.Inputs);
            string content = string.Join(",", values.Select(p => Csv(p.Key))) + "\r\n" + string.Join(",", values.Select(p => Csv(Format(p.Value)))) + "\r\n";
            string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write))
                using (var writer = new StreamWriter(stream, new UTF8Encoding(true)))
                {
                    writer.Write(content);
                    writer.Flush();
                    stream.Flush(true);
                }
                token.ThrowIfCancellationRequested();
                File.Move(temporary, path);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
            return Task.FromResult(ActionOutcome.Of("RecordId", recordId, "Path", path));
        }

        /// <summary>格式化业务值，保持存储与界面文化无关。</summary>
        public static string Format(object value) { return Convert.ToString(value, CultureInfo.InvariantCulture) ?? ""; }

        /// <summary>转义逗号、引号和多行 CSV 字段。</summary>
        private static string Csv(string value) { return "\"" + value.Replace("\"", "\"\"") + "\""; }
    }
}
