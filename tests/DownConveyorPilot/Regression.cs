using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AlphaRap;
using AlphaRap.TaskProcess;
using NPSDK;
using Result = NPSDK.Flow_Chart.ResultType;

internal static class Regression
{
    private static readonly BindingFlags Instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private static int checks;

    private sealed class Simulator
    {
        internal readonly Dictionary<string, bool?> Inputs = new Dictionary<string, bool?>();
        internal readonly Dictionary<string, bool> Outputs = new Dictionary<string, bool>();
        internal readonly List<string> Reads = new List<string>();
        internal readonly List<object> Transfers = new List<object>();
        internal string FailingOutput;

        /// <summary>给出当前模拟感应器值，尚未提供样本时返回未知。</summary>
        public bool? Read(object input)
        {
            string key = input.ToString();
            Reads.Add(key);
            bool? value;
            return Inputs.TryGetValue(key, out value) ? value : null;
        }

        /// <summary>记录输出值，可模拟一个输出写入异常。</summary>
        public void Write(object output, bool active)
        {
            string key = output.ToString();
            if (key == FailingOutput) throw new IOException(key);
            Outputs[key] = active;
        }

        /// <summary>保留已完成的交接快照供生产存储及导出验证。</summary>
        public void Record(object transfer) { Transfers.Add(transfer); }
    }

    /// <summary>使用实际的流程节点和文件存储完成离线下料验收。</summary>
    [STAThread]
    private static int Main(string[] args)
    {
        var simulator = new Simulator();
        using (DownConveryor flow = CreateFlow(simulator))
        {
            VerifyTransfer(flow, simulator);
            VerifyStop(flow, simulator);
        }
        VerifyProduction(simulator.Transfers.Single(), args[0]);
        Console.WriteLine("Passed {0} checks.", checks);
        return 0;
    }

    /// <summary>创建应用的下料流程监控页，注入模拟进程，不创建手动页或初始化驱动。</summary>
    private static DownConveryor CreateFlow(Simulator simulator)
    {
        Type flowType = typeof(DownConveryor);
        ConstructorInfo flowConstructor = flowType.GetConstructors(Instance).Single(ctor => ctor.GetParameters().Length == 1);
        Type processType = flowConstructor.GetParameters()[0].ParameterType;
        ConstructorInfo processConstructor = processType.GetConstructors(Instance).Single();
        ParameterInfo[] ports = processConstructor.GetParameters();
        object process = processConstructor.Invoke(new object[] {
            Bind(ports[0].ParameterType, simulator, "Read"),
            Bind(ports[1].ParameterType, simulator, "Write"),
            Bind(ports[2].ParameterType, simulator, "Record")
        });
        return (DownConveryor)flowConstructor.Invoke(new object[] { process });
    }

    /// <summary>将模拟器公开的测试方法适配到应用内部的 IO 及记录类型。</summary>
    private static Delegate Bind(Type delegateType, Simulator simulator, string method)
    {
        MethodInfo target = typeof(Simulator).GetMethod(method);
        ParameterExpression[] parameters = delegateType.GetMethod("Invoke").GetParameters()
            .Select(parameter => Expression.Parameter(parameter.ParameterType)).ToArray();
        Expression[] arguments = target.GetParameters().Select((parameter, index) =>
            (Expression)Expression.Convert(parameters[index], parameter.ParameterType)).ToArray();
        return Expression.Lambda(delegateType, Expression.Call(Expression.Constant(simulator), target, arguments), parameters).Compile();
    }

    /// <summary>按真实流程连线验证接板、换速、挡停、下游交接及完成记录。</summary>
    private static void VerifyTransfer(DownConveryor flow, Simulator simulator)
    {
        SysPara.RecipeName = "Pilot,Model";
        Flow_Chart node = FindNode(flow, "1_1");
        Step(ref node, "1_1", Result.NEXT);
        Step(ref node, "1_2", Result.IDLE);
        simulator.Inputs["BoardAtStop"] = false;
        Step(ref node, "1_2", Result.NEXT);
        Step(ref node, "1_4", Result.IDLE);
        Check(simulator.Outputs["Stopper"], "Stopper must be raised while waiting for feedback.");
        simulator.Inputs["StopperUp"] = true;
        Step(ref node, "1_4", Result.NEXT);
        Step(ref node, "1_5", Result.NEXT);
        Step(ref node, "1_6", Result.IDLE);
        simulator.Inputs["UpstreamAvailable"] = true;
        Step(ref node, "1_6", Result.NEXT);
        Step(ref node, "1_7", Result.NEXT);
        Check(simulator.Outputs["ReadyForBoard"] && simulator.Outputs["Forward"], "Infeed handshake and drive must be active.");
        Step(ref node, "1_8", Result.IDLE);
        simulator.Inputs["BoardIn"] = true;
        Step(ref node, "1_8", Result.NEXT);
        simulator.Inputs["BoardIn"] = null;
        Step(ref node, "1_9", Result.IDLE);
        simulator.Inputs["BoardIn"] = false;
        Step(ref node, "1_9", Result.NEXT);
        Step(ref node, "2_0", Result.NEXT);
        simulator.Inputs["Slowdown"] = false;
        Step(ref node, "2_1", Result.NEXT);
        Check(simulator.Reads.Contains("Slowdown"), "Existing slowdown sampling must be preserved.");
        Step(ref node, "2_2", Result.NEXT);
        Check(simulator.Outputs["SlowSpeed"], "Speed output must follow the speed change step.");
        Step(ref node, "2_3", Result.IDLE);
        simulator.Inputs["BoardAtStop"] = true;
        Step(ref node, "2_3", Result.NEXT);
        Step(ref node, "2_4", Result.NEXT);
        Check(!simulator.Outputs["Forward"] && !simulator.Outputs["SlowSpeed"], "Positioning must stop forward and speed outputs.");
        Step(ref node, "2_5", Result.NEXT);
        Step(ref node, "2_6", Result.IDLE);
        simulator.Inputs["DownstreamReady"] = true;
        Step(ref node, "2_6", Result.NEXT);
        Step(ref node, "2_7", Result.IDLE);
        simulator.Inputs["StopperDown"] = true;
        Step(ref node, "2_7", Result.NEXT);
        Step(ref node, "2_8", Result.NEXT);
        Step(ref node, "2_9", Result.IDLE);
        simulator.Inputs["BoardOut"] = true;
        Step(ref node, "2_9", Result.NEXT);
        Step(ref node, "3_0", Result.IDLE);
        simulator.Inputs["BoardOut"] = false;
        Step(ref node, "3_0", Result.NEXT);
        Step(ref node, "3_1", Result.IDLE);
        simulator.Inputs["DownstreamReady"] = false;
        Step(ref node, "3_1", Result.NEXT);
        Step(ref node, "3_2", Result.NEXT);
        Check(simulator.Transfers.Count == 0, "A record cannot precede the completed handshake.");
        SysPara.RecipeName = "NextModel";
        Step(ref node, "3_3", Result.NEXT);
        Check(!simulator.Outputs["Forward"] && simulator.Transfers.Count == 1, "Only one completed transfer must be emitted.");
        node = FindNode(flow, "3_3");
        Step(ref node, "3_3", Result.NEXT);
        Check(simulator.Transfers.Count == 1, "Repeating the completion node must not duplicate the record.");
    }

    /// <summary>确认已有板件走出板分支，停止时继续撤销其余输出并取消未完工记录。</summary>
    private static void VerifyStop(DownConveryor flow, Simulator simulator)
    {
        Flow_Chart node = FindNode(flow, "1_2");
        Step(ref node, "1_2", Result.CASE1);
        Step(ref node, "1_3", Result.NEXT);
        Step(ref node, "2_5", Result.NEXT);
        simulator.Outputs["ReadyForBoard"] = true;
        simulator.Outputs["Forward"] = true;
        simulator.FailingOutput = "SlowSpeed";
        bool reported = false;
        try { Flow_Module.Module_StopRun(); }
        catch (AggregateException error) { reported = error.Flatten().InnerExceptions.Count == 1; }
        Check(reported, "Partial stop failures must be reported.");
        Check(!simulator.Outputs["Forward"] && !simulator.Outputs["ReadyForBoard"] && !simulator.Outputs["BoardAvailable"],
            "One failed output must not skip other stop commands.");
        node = FindNode(flow, "3_3");
        Step(ref node, "3_3", Result.NEXT);
        Check(simulator.Transfers.Count == 1, "Stop must discard the incomplete transfer.");
    }

    /// <summary>按设计器名称取实际的自动步骤。</summary>
    private static Flow_Chart FindNode(DownConveryor flow, string name)
    {
        return (Flow_Chart)flow.GetType().GetField("DownConveyorAuto_Flow" + name, Instance).GetValue(flow);
    }

    /// <summary>调用节点绑定的原生事件并按结果走设计器连线。</summary>
    private static void Step(ref Flow_Chart node, string name, Result expected)
    {
        Check(node != null && node.Name == "DownConveyorAuto_Flow" + name, "Unexpected next node: " + name);
        Result actual = node.GetFlowChartHandlers().Single()();
        Check(actual == expected, node.Name + " returned " + actual + " instead of " + expected);
        if (actual == Result.NEXT) node = node.Next;
        if (actual == Result.CASE1) node = node.Casse1;
    }

    /// <summary>在临时目录用同一个交接快照写生产记录，并按客户示例列名与顺序导出。</summary>
    private static void VerifyProduction(object transfer, string output)
    {
        Type type = transfer.GetType();
        string id = (string)type.GetProperty("Id", Instance).GetValue(transfer, null);
        string model = (string)type.GetProperty("Model", Instance).GetValue(transfer, null);
        DateTime time = (DateTime)type.GetProperty("CompletedAt", Instance).GetValue(transfer, null);
        var fields = new Dictionary<string, string> {
            { "RecordId", id }, { "CycleId", id }, { "ModelName", model },
            { "Station", "DownConveyor" }, { "TransferResult", "Completed" }, { "Simulated", "True" }
        };
        ProductionDataStore.SetRoot(Path.Combine(output, "ProductionData"));
        Check(ProductionDataStore.AppendNamed(time, fields, id) == "", "Production record must be persisted.");
        Check(ProductionDataStore.AppendNamed(time, fields, id) == "", "Retrying the same record must succeed.");
        int files, total;
        ProductionRecord production = ProductionDataStore.Query(time.Date, time.Date, id, out files, out total).Single();
        Check(files == 1 && total == 1 && production.Get("RecordId") == id && production.Get("ModelName") == "Pilot,Model",
            "Stored record must keep its identity and original model without duplication.");
        string customerFile = Path.Combine(output, "CustomerTransfer.csv");
        var customer = new ProductionRecord {
            Time = production.Time,
            Columns = new[] { "CycleNumber", "Model", "Handoff", "Simulated" },
            Values = new[] { production.Get("CycleId"), production.Get("ModelName"), production.Get("TransferResult"), production.Get("Simulated") }
        };
        Check(ProductionDataStore.Export(new List<ProductionRecord> { customer }, customerFile) == "", "Customer export must succeed.");
        string[] lines = File.ReadAllLines(customerFile);
        Check(lines.Length == 2 && lines[0] == "Time,CycleNumber,Model,Handoff,Simulated" &&
            lines[1] == production.Time.ToString("yyyy-MM-dd HH:mm:ss") + "," + id + ",\"Pilot,Model\",Completed,True",
            "The customer file must contain the stored record values in the requested example order.");
        Console.WriteLine("Production CSV: " + ProductionDataStore.DayFile(time));
        Console.WriteLine("Customer CSV: " + customerFile);
    }

    /// <summary>失败时报告正在验证的实际行为。</summary>
    private static void Check(bool success, string message)
    {
        if (!success) throw new InvalidOperationException(message);
        checks++;
    }
}
