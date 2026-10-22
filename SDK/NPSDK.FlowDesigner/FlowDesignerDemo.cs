using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NPSDK.Design
{
    public sealed class FlowDemoInstances
    {
        public bool InputState { get; set; } = true;
        public string Barcode { get; set; } = "DEMO-0001";
        public double VisionX { get; set; } = 0.25;
        public double VisionY { get; set; } = 0.1;
        public double VisionAngle { get; set; } = 0.5;
        public int ReadyDelayMs { get; set; } = 300;
    }

    public static class FlowDesignerDemo
    {
        /// <summary>构建不初始化硬件和全局报警状态的独立示例环境。</summary>
        public static FlowDesignerEnvironment CreateEnvironment()
        {
            var environment = new FlowDesignerEnvironment
            {
                Registry = new FlowActionRegistry(),
                OutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProductionData"),
                DefaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FlowChart.xml")
            };
            environment.Text = key =>
            {
                string[] values;
                return FlowDesignerTexts.Defaults.TryGetValue(key, out values) ? values[environment.Locale == "English" ? 1 : environment.Locale == "Español" ? 2 : 0] : key;
            };
            var instances = AddDemoBindings(environment.Registry);
            environment.ShowInstances = () =>
            {
                using (var dialog = new FlowDemoInstanceDialog(instances))
                {
                    FlowDesignerUi.Apply(dialog, environment);
                    dialog.ShowDialog();
                }
            };
            environment.Example = CreateExample(environment.Registry);
            return environment;
        }

        /// <summary>登记明确标为 DEMO 的对象，不假冒已连接设备。</summary>
        public static FlowDemoInstances AddDemoBindings(FlowActionRegistry registry)
        {
            var instances = new FlowDemoInstances();
            registry.Add(new FlowActionBinding { MethodName = "IO.Read", TargetId = "DEMO.Input", Outputs = new[] { "IsOn" }, Poll = call => FlowActionSample.Of("IsOn", instances.InputState) });
            registry.Add(new FlowActionBinding { MethodName = "Motion.Move", TargetId = "DEMO.Axis", Parameters = new[] { Parameter("Position", "Double", "10") }, Outputs = new[] { "Completed", "Position" }, Start = call => call.State = Stopwatch.StartNew(), Poll = call => (Stopwatch)call.State != null && ((Stopwatch)call.State).ElapsedMilliseconds < Math.Max(0, instances.ReadyDelayMs) ? new FlowActionSample { Waiting = true } : FlowActionSample.Of("Completed", true, "Position", call.Inputs["Position"]) });
            registry.Add(new FlowActionBinding { MethodName = "Scanner.Trigger", TargetId = "DEMO.Scanner", Outputs = new[] { "Barcode" }, Start = call => call.State = Stopwatch.StartNew(), Poll = call => ((Stopwatch)call.State).ElapsedMilliseconds < Math.Max(0, instances.ReadyDelayMs) ? new FlowActionSample { Waiting = true } : FlowActionSample.Of("Barcode", instances.Barcode) });
            registry.Add(new FlowActionBinding { MethodName = "Vision.Run", TargetId = "DEMO.Vision", Outputs = new[] { "X", "Y", "Angle", "Passed" }, Start = call => call.State = Stopwatch.StartNew(), Poll = call => ((Stopwatch)call.State).ElapsedMilliseconds < Math.Max(0, instances.ReadyDelayMs) ? new FlowActionSample { Waiting = true } : FlowActionSample.Of("X", instances.VisionX, "Y", instances.VisionY, "Angle", instances.VisionAngle, "Passed", true) });
            AddCommonBindings(registry);
            return instances;
        }

        /// <summary>登记可复用的真实本地记录与 NPSDK 报警动作。</summary>
        public static void AddCommonBindings(FlowActionRegistry registry)
        {
            if (registry.Find("Data.Save", "LocalCsv") == null)
                registry.Add(new FlowActionBinding { MethodName = "Data.Save", TargetId = "LocalCsv", Outputs = new[] { "Saved", "Path" }, Start = call => call.Pending = Task.Run(() => call.State = SaveRecord(call)), Poll = call =>
                {
                    if (!call.Pending.IsCompleted)
                        return new FlowActionSample
                        {
                            Waiting = true
                        };
                    call.Pending.GetAwaiter().GetResult();
                    return FlowActionSample.Of("Saved", true, "Path", (string)call.State);
                } });
            if (registry.Find("Alarm.Show", "NPSDK.Alarm") == null)
                registry.Add(new FlowActionBinding { MethodName = "Alarm.Show", TargetId = "NPSDK.Alarm", Parameters = new[] { Parameter("Code", "String", registry.AlarmCodes.FirstOrDefault() ?? ""), Parameter("Content", "String", "") }, Outputs = new[] { "Shown" }, Start = call =>
                {
                    string code = (string)call.Inputs["Code"];
                    if (!registry.AlarmCodes.Contains(code))
                        throw new FlowDesignIssue("UnknownAlarm", code);
                    string content = (string)call.Inputs["Content"];
                    Alarm.Show(code, string.IsNullOrEmpty(content) ? null : content);
                }, Poll = call => FlowActionSample.Of("Shown", true) });
        }

        /// <summary>创建扫描、运动、视觉、记录示例，数据通过节点字段引用传递。</summary>
        public static FlowDesignDocument CreateExample(FlowActionRegistry registry)
        {
            var document = new FlowDesignDocument();
            document.Nodes.Add(Node(registry, "Scan", "Scanner.Trigger", "DEMO.Scanner", "Move", 30, 40, "Barcode", FlowComparison.NotEmpty));
            document.Nodes.Add(Node(registry, "Move", "Motion.Move", "DEMO.Axis", "Vision", 30, 150, "Completed", FlowComparison.IsTrue));
            document.Nodes.Add(Node(registry, "Vision", "Vision.Run", "DEMO.Vision", "Save", 330, 150, "X", FlowComparison.InRange));
            document.Nodes[2].Expected = "-1,1";
            var save = Node(registry, "Save", "Data.Save", "LocalCsv", "", 630, 150, "Saved", FlowComparison.IsTrue);
            save.Parameters.Add(new FlowDesignParameter { Name = "Barcode", Source = "Scan.Barcode" });
            save.Parameters.Add(new FlowDesignParameter { Name = "X", Type = "Double", Source = "Vision.X" });
            save.Parameters.Add(new FlowDesignParameter { Name = "Y", Type = "Double", Source = "Vision.Y" });
            save.Parameters.Add(new FlowDesignParameter { Name = "Angle", Type = "Double", Source = "Vision.Angle" });
            document.Nodes.Add(save);
            return document;
        }

        /// <summary>生成节点底稿，保留稳定实例名称和默认参数。</summary>
        private static FlowDesignNode Node(FlowActionRegistry registry, string id, string method, string target, string next, int x, int y, string source, FlowComparison comparison)
        {
            return new FlowDesignNode
            {
                Id = id,
                Text = id,
                MethodName = method,
                Target = target,
                Next = next,
                X = x,
                Y = y,
                ConditionSource = source,
                Comparison = comparison,
                Timeout = 3000,
                Parameters = registry.Find(method, target).Parameters.Select(p => p.Copy()).ToList()
            };
        }

        /// <summary>创建有类型的常量参数。</summary>
        public static FlowDesignParameter Parameter(string name, string type, string value)
        {
            return new FlowDesignParameter
            {
                Name = name,
                Type = type,
                Value = value
            };
        }

        /// <summary>每次记录使用唯一文件，成功刷盘后原子发布，避免跨轮混写。</summary>
        private static string SaveRecord(FlowInvocation call)
        {
            if (call.Cancelled)
                throw new OperationCanceledException();
            Directory.CreateDirectory(call.OutputDirectory);
            string path = Path.Combine(call.OutputDirectory, DateTime.UtcNow.ToString("yyyyMMddTHHmmssfff", CultureInfo.InvariantCulture) + "_" + call.RunId + "_" + Guid.NewGuid().ToString("N") + ".csv");
            string temporary = path + ".tmp";
            try
            {
                using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                using (var writer = new StreamWriter(stream, new UTF8Encoding(true)))
                {
                    var columns = new[]
                    {
                        "RunId",
                        "UtcTime",
                        "Node"
                    }.Concat(call.Inputs.Keys);
                    var values = new object[]
                    {
                        call.RunId,
                        DateTime.UtcNow.ToString("O"),
                        call.Node.Id
                    }.Concat(call.Inputs.Values);
                    writer.WriteLine(string.Join(",", columns.Select(Quote)));
                    writer.WriteLine(string.Join(",", values.Select(value => Quote(Convert.ToString(value, CultureInfo.InvariantCulture)))));
                    writer.Flush();
                    stream.Flush(true);
                }

                if (call.Cancelled)
                    throw new OperationCanceledException();
                File.Move(temporary, path);
                return path;
            }
            finally
            {
                if (File.Exists(temporary))
                    File.Delete(temporary);
            }
        }

        /// <summary>按 CSV 规则转义字段，保留生产数据原值。</summary>
        private static string Quote(string value)
        {
            return "\"" + (value ?? "").Replace("\"", "\"\"") + "\"";
        }
    }
}
