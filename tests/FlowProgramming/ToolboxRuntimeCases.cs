using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AlphaRap;
using AlphaRap.FlowProgramming;
using NPSDK;

public sealed class ToolboxCallInputs { public double Value { get; set; } }
public sealed class ToolboxCallOutputs { public double Value { get; set; } }

[FlowCodeModule("test.toolbox.call", "code", "受控调用测试", "Controlled call test", "Prueba de llamada", Exits = "NEXT,CASE1")]
[FlowCodeResource("Device", "device", "实际设备", "Actual device", "Dispositivo real")]
public sealed class ToolboxCallModule : FlowCodeNode<ToolboxCallInputs, ToolboxCallOutputs>
{
    /// <summary>Invoke the bound instance through the same host broker used by ordinary actions.</summary>
    protected override async Task<Flow_Chart.ResultType> ExecuteAsync(FlowCodeContext context, ToolboxCallInputs inputs, ToolboxCallOutputs outputs)
    {
        IDictionary<string, object> result = await context.CallAsync("Device", "test.echo", new Dictionary<string, object> { { "Value", inputs.Value } });
        outputs.Value = Convert.ToDouble(result["Value"]);
        return inputs.Value >= 0 ? Flow_Chart.ResultType.NEXT : Flow_Chart.ResultType.CASE1;
    }
}

public sealed class ToolboxVisionOutputs { public double Height { get; set; } }
[FlowCodeModule("test.toolbox.vision", "code", "视觉映射测试", "Vision mapping test", "Prueba de mapeo")]
[FlowCodeResource("Vision", "vision", "实际VPP", "Actual VPP", "VPP real")]
[FlowCodeResourceField("Vision", "Height", typeof(double), "高度", "Height", "Altura")]
public sealed class ToolboxVisionModule : FlowCodeNode<ToolboxCallInputs, ToolboxVisionOutputs>
{
    /// <summary>Read a mapped business output from a run that uses the real method's input defaults.</summary>
    protected override async Task<Flow_Chart.ResultType> ExecuteAsync(FlowCodeContext context, ToolboxCallInputs inputs, ToolboxVisionOutputs outputs)
    {
        FlowCodeVisionResult result = await context.Vision("Vision").RunAsync();
        outputs.Height = result.Get<double>("Height");
        return Flow_Chart.ResultType.NEXT;
    }
}

internal static partial class Cases
{
    /// <summary>Exercise the host integration rather than only the SDK module adapter.</summary>
    public static void RegisterToolboxRuntimeCases()
    {
        Test("code modules use real resource defaults simulation and unique method metadata", ToolboxCalls);
        Test("code and ordinary nodes share real instance leases until cancelled work actually ends", ToolboxLeases);
        Test("code VPP fields bind current typed terminals and reject changed mappings", ToolboxVisionMapping);
        Test("code outputs pass to an ordinary node and final CASE is not reevaluated by conditions", ToolboxMixedGraph);
        Test("code point metadata remains per node and current recipe confirmations reject changed tables", ToolboxPoints);
    }

    /// <summary>Create a fully routed module node using its actual discovered contract.</summary>
    private static FlowStep ToolboxStep(string id, string target, string slot)
    {
        FlowCodeDefinition definition = FlowToolboxCatalog.Find(id);
        FlowStep step = new FlowStep { Target = "code/" + id, Action = "code.execute", CodeModuleId = id, CodeModuleVersion = definition.Version,
            Branches = definition.Exits.Select(exit => new FlowBranch { Code = exit, Unconditional = true }).ToList() };
        foreach (string exit in definition.Exits) step.SetTarget(exit, "");
        foreach (FlowField input in definition.Inputs) step.Inputs.Add(new FlowInput { Name = input.Name, Reference = input.DefaultValue });
        if (slot != null) step.ResourceBindings.Add(new FlowResourceBinding { Slot = slot, Target = target });
        return step;
    }

    /// <summary>Build a resource snapshot around one actual instance and an explicitly implemented method.</summary>
    private static FlowActions ToolboxActions(object instance, FlowMethod method, string kind = "device")
    {
        FlowResources resources = new FlowResources();
        resources.Items.Add("test/actual", new FlowResource { Id = "test/actual", Label = "Actual test resource", Kind = kind, Instance = instance,
            Methods = new List<FlowMethod> { method } });
        return new FlowActions(resources);
    }

    /// <summary>Verify typed inputs and simulation select the selected instance's declared implementation.</summary>
    private static void ToolboxCalls()
    {
        int real = 0, simulated = 0;
        FlowMethod method = new FlowMethod("test.echo", "测试", "Test", "Prueba") { UsesHardware = true };
        method.Inputs.Add(new FlowField("Value", typeof(double), "7")); method.Outputs.Add(new FlowField("Value", typeof(double)));
        method.Invoke = call => { real++; return Task.FromResult(FlowCall.Result("Value", call.Get<double>("Value") * 2)); };
        method.Simulate = call => { simulated++; return Task.FromResult(FlowCall.Result("Value", call.Get<double>("Value") * 3)); };
        FlowActions actions = ToolboxActions(new object(), method);
        FlowStep step = ToolboxStep("test.toolbox.call", "test/actual", "Device"); step.Inputs[0].Reference = "-4";
        FlowActionOutcome result = actions.ExecuteStep(step, Context(new FlowResultHub()), CancellationToken.None).GetAwaiter().GetResult();
        Assert(result.BranchCode == "CASE1" && (double)result.Outputs["Value"] == -8 && real == 1, "The code module did not call its selected real object or lost CASE1.");
        actions.Simulate = true;
        result = actions.ExecuteStep(step, Context(new FlowResultHub()), CancellationToken.None).GetAwaiter().GetResult();
        Assert((double)result.Outputs["Value"] == -12 && real == 1 && simulated == 1, "Simulation touched the real implementation.");
        Assert(actions.FindMethod(step).Id != "code.execute", "Unrelated code modules share language keys for same-named ports.");
        step.CodeModuleVersion++;
        bool rejected = false; try { actions.Validate(step); } catch (InvalidOperationException) { rejected = true; }
        Assert(rejected, "A changed module contract silently executed.");
    }

    /// <summary>Cancellation waits for the real action, preventing ordinary actions from acquiring its instance early.</summary>
    private static void ToolboxLeases()
    {
        int starts = 0;
        TaskCompletionSource<IDictionary<string, object>> blocked = new TaskCompletionSource<IDictionary<string, object>>();
        FlowMethod method = new FlowMethod("test.echo", "测试", "Test", "Prueba");
        method.Inputs.Add(new FlowField("Value", typeof(double))); method.Outputs.Add(new FlowField("Value", typeof(double)));
        method.Invoke = call => Interlocked.Increment(ref starts) == 1 ? blocked.Task : Task.FromResult(FlowCall.Result("Value", call.Get<double>("Value")));
        object instance = new object(); FlowActions actions = ToolboxActions(instance, method);
        FlowStep step = ToolboxStep("test.toolbox.call", "test/actual", "Device"); step.Inputs[0].Reference = "2";
        using (CancellationTokenSource stop = new CancellationTokenSource())
        {
            Task<FlowActionOutcome> code = actions.ExecuteStep(step, Context(new FlowResultHub()), stop.Token);
            Assert(SpinWait.SpinUntil(() => starts == 1, 1000), "The module did not start its real call.");
            stop.Cancel();
            FlowStep ordinary = new FlowStep { Target = "test/actual", Action = "test.echo", Branches = FlowBranch.Default() };
            ordinary.Inputs.Add(new FlowInput { Name = "Value", Reference = "5" });
            Task<IDictionary<string, object>> next = actions.Execute(ordinary, Context(new FlowResultHub()), CancellationToken.None);
            Assert(!code.IsCompleted && !next.IsCompleted && !FlowResourceGate.CanAccess(instance) && starts == 1, "Cancellation released a non-cooperative resource early.");
            blocked.SetResult(FlowCall.Result("Value", 99.0));
            bool cancelled = false; try { code.GetAwaiter().GetResult(); } catch (OperationCanceledException) { cancelled = true; }
            Assert(cancelled && (double)next.GetAwaiter().GetResult()["Value"] == 5 && starts == 2 && FlowResourceGate.CanAccess(instance), "Late outputs were accepted or the real lease was not released after drain.");
        }
    }

    /// <summary>Current VPP metadata supplies optional input defaults and maps outputs without constructing a camera.</summary>
    private static void ToolboxVisionMapping()
    {
        FlowMethod run = new FlowMethod("vision.run", "拍照", "Capture", "Capturar");
        run.Inputs.Add(new FlowField("TB:Threshold", typeof(double), "3.25"));
        foreach (FlowField field in new[] { new FlowField("RunId", typeof(long)), new FlowField("Accepted", typeof(bool)), new FlowField("X", typeof(double)), new FlowField("Y", typeof(double)), new FlowField("Angle", typeof(double)), new FlowField("TB:H", typeof(double)) }) run.Outputs.Add(field);
        run.Invoke = call => { Assert(call.Get<double>("TB:Threshold") == 3.25, "VPP defaults were omitted."); return Task.FromResult<IDictionary<string, object>>(new Dictionary<string, object> { { "RunId", 7L }, { "Accepted", true }, { "X", 0.0 }, { "Y", 0.0 }, { "Angle", 0.0 }, { "TB:H", 4.5 } }); };
        FlowActions actions = ToolboxActions(new object(), run, "vision");
        FlowStep step = ToolboxStep("test.toolbox.vision", "test/actual", "Vision");
        step.ResourceBindings[0].Fields.Add(new FlowResourceFieldBinding { Name = "Height", Field = "TB:H" });
        FlowActionOutcome result = actions.ExecuteStep(step, Context(new FlowResultHub()), CancellationToken.None).GetAwaiter().GetResult();
        Assert((double)result.Outputs["Height"] == 4.5, "The selected business terminal did not supply the module output.");
        run.Outputs.RemoveAll(field => field.Name == "TB:H");
        bool rejected = false; try { actions.Validate(step); } catch (InvalidOperationException) { rejected = true; }
        Assert(rejected, "A missing terminal silently used another VPP field.");
    }

    /// <summary>Publish a module output and let an ordinary action resolve it in the same product context.</summary>
    private static void ToolboxMixedGraph()
    {
        FlowActions actions = new FlowActions(new FlowResources());
        FlowStep code = editor.AddNode("quality.weight-check", station.Canvas, new System.Drawing.Point(50, 60));
        foreach (FlowBranch branch in code.Branches) code.SetTarget(branch.Code, "");
        code.Inputs.Single(input => input.Name == "Weight").Reference = "20";
        FlowStep next = editor.AddNode(); next.Inputs[0].Source = "Output"; next.Inputs[0].Reference = editor.Program.ModuleId + "/" + code.Id + "/Weight";
        code.Case1 = next.Id;
        try
        {
            FlowProgram program = new FlowProgram { ModuleId = editor.Program.ModuleId, Mode = "Program", AutoEntry = code.Id, Steps = new List<FlowStep> { code, next } };
            FlowResultHub hub = new FlowResultHub(); FlowExecutionContext context = Context(hub);
            FlowProgramRuntime runtime = new FlowProgramRuntime { ExecuteStep = actions.ExecuteStep };
            runtime.Start(program, code.Id, context); Wait(runtime.Completion);
            Assert(string.IsNullOrEmpty(runtime.Error) && (double)hub.Read(context.RunId, context.CycleId, program.ModuleId + "/" + next.Id, "Value") == 20, "The code module output did not reach the ordinary action: " + runtime.Error);
        }
        finally { editor.Delete(next.Id); editor.Delete(code.Id); }
    }

    /// <summary>Dynamic axis catalogs stay isolated per node and detect recipe table changes before execution.</summary>
    private static void ToolboxPoints()
    {
        FlowCodeDefinition definition = FlowToolboxCatalog.Find("inspection.at-point");
        FlowResources resources = new FlowResources();
        foreach (string id in new[] { "a", "b" })
        {
            FlowPointAxis data = new FlowPointAxis { TableName = "Position", RowCount = 1 }; data.Positions.Add(id == "a" ? 0 : 1, 10);
            FlowResource points = new FlowResource { Id = id + "/points", Kind = "point", Instance = data };
            points.Points.Add(new FlowChoice { Id = id == "a" ? "0" : "1", Label = id + " point" });
            resources.Items.Add(id, new FlowResource { Id = id, Label = id, Kind = "axis", Instance = new object(), BoundPoints = points });
        }
        FlowStep first = ToolboxStep(definition.Id, "a", "Positioning"), second = ToolboxStep(definition.Id, "b", "Positioning");
        FlowMethod left = FlowResourceAccess.CreateMethod(definition, first, resources), right = FlowResourceAccess.CreateMethod(definition, second, resources);
        Assert(left.Inputs.Single(field => field.Name == "Point").Options.Single().Value == "0" && right.Inputs.Single(field => field.Name == "Point").Options.Single().Value == "1" && definition.Inputs.Single(field => field.Name == "Point").Options.Count == 0, "Two code nodes shared mutable point candidates.");
        FlowResourceBinding binding = first.ResourceBindings[0]; FlowResource axis = resources.Items["a"];
        FlowResourceAccess.ConfirmPoints(binding, axis, "RecipeA"); FlowResourceAccess.ConfirmPoints(binding, axis, "RecipeB");
        Assert(FlowResourceAccess.PointsConfirmed(binding, axis, "RecipeA") && FlowResourceAccess.PointsConfirmed(binding, axis, "RecipeB"), "Confirming one recipe erased another.");
        ((FlowPointAxis)axis.BoundPoints.Instance).Positions[0] = 11;
        Assert(!FlowResourceAccess.PointsConfirmed(binding, axis, "RecipeA"), "A modified point table retained an obsolete confirmation.");
    }
}
