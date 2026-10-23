using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlphaRap;
using AlphaRap.FlowProgramming;
using NPSDK;
using NPSDK.Motion;
using Cognex.VisionPro.ToolBlock;

internal sealed class DesignerProbe : IFlowObject
{
    public int Calls;
    /// <summary>Declare one application capability without executing it during browsing.</summary>
    public IEnumerable<FlowMethod> GetFlowMethods(bool includeDraft = false)
    {
        FlowMethod method = new FlowMethod("probe.read", "测试对象读取", "Read probe object", "Leer objeto de prueba");
        method.Inputs.Add(new FlowField("Increment", typeof(int), "2"));
        method.Outputs.Add(new FlowField("Count", typeof(int)));
        method.Invoke = call => Task.FromResult(FlowCall.Result("Count", Calls += call.Get<int>("Increment")));
        yield return method;
    }
}

internal sealed class PointDriver : MotorBaseInterface
{
    public int Moves, Stops, HomePolls;
    public double Position;
    public bool Hold, Moving;
    /// <summary>Issue motion while returning the real SDK in-progress value false.</summary>
    public override bool Goto(double target) { Moves++; Position = target; Moving = Hold; return false; }
    /// <summary>Complete only on the second state-machine poll.</summary>
    public override bool Home() { HomePolls++; Position = 0; return HomePolls >= 2; }
    /// <summary>Reset the homing state without starting movement.</summary>
    public override void HomeReset() { HomePolls = 0; }
    /// <summary>Return the controlled encoder value.</summary>
    public override double GetEncPos() { return Position; }
    /// <summary>Expose controlled physical activity.</summary>
    public override bool GetBusy() { return Moving; }
    /// <summary>Stop the test axis so cancellation can finish safely.</summary>
    public override void FastStop() { Stops++; Moving = false; }
}

internal sealed class CapabilityHost : Form
{
    public IFlowObject Bound;
}

internal sealed class FlowVisionProbe : VpStation
{
    public bool Accept = true;
    public int Runs;
    /// <summary>Create a real in-memory ToolBlock with typed ports and no acquisition tools.</summary>
    public FlowVisionProbe() : base("Camera-A", "VPP-A", VpStationKind.Custom)
    {
        TB = new CogToolBlock(); IsLoadTBOk = true; AutoVisionRunDisplay = false;
        TB.Inputs.Add(new CogToolBlockTerminal("Measure", typeof(double)) { Value = 2.0 });
        TB.Outputs.Add(new CogToolBlockTerminal("Score", typeof(double)));
        Config = new VpVppConfig { Name = "VPP-A", CompLimits = new List<VpCompLimitItem> { new VpCompLimitItem("X", -5, 5) { Compensation = 1 } } };
        CommitSettings();
    }
    /// <summary>Produce a deterministic real terminal value and a configurable judgement.</summary>
    public override void VisionRun() { Runs++; TB.Outputs["Score"].Value = (double)TB.Inputs["Measure"].Value * 2; IsAccept = Accept; }
    /// <summary>Exercise the same snapshot publication invoked after the main save succeeds.</summary>
    public void CommitSettings() { typeof(VpStation).GetMethod("AcceptFlowConfiguration", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(this, null); }
}

internal static partial class Cases
{
    /// <summary>Create typed step inputs from the actual object's own method declaration.</summary>
    private static FlowStep BoundStep(FlowResource resource, string methodId)
    {
        FlowMethod method = resource.Methods.Single(item => item.Id == methodId);
        return new FlowStep { Target = resource.Id, Action = methodId, Inputs = method.Inputs.Select(field => new FlowInput { Name = field.Name, Reference = field.DefaultValue }).ToList() };
    }

    /// <summary>Resolve a business call without starting machine threads or SDK initialization.</summary>
    private static IDictionary<string, object> ExecuteBound(FlowActions actions, FlowStep step)
    {
        FlowResultHub hub = new FlowResultHub();
        return actions.Execute(step, new FlowExecutionContext { Hub = hub, RunId = hub.Epoch, CycleId = "probe", Parameters = actions.Resources.Parameters }, CancellationToken.None).GetAwaiter().GetResult();
    }

    /// <summary>Verify automatic discovery and precise dispatch of a previously unknown object.</summary>
    private static void ObjectContract()
    {
        FlowResources resources = FlowResources.Capture();
        FlowResource resource = resources.Items.Values.Single(item => ReferenceEquals(item.Instance, station.Capability));
        Assert(station.Capability.Calls == 0, "Browsing invoked an object method.");
        FlowActions actions = new FlowActions(resources);
        FlowStep step = BoundStep(resource, "probe.read");
        Assert((int)ExecuteBound(actions, step)["Count"] == 2, "The bound object's declared method was not called.");
        step.Inputs[0].Reference = "1.5";
        bool blocked = false; try { actions.Validate(step); } catch (InvalidOperationException) { blocked = true; }
        Assert(blocked && station.Capability.Calls == 2, "Fractional integer input reached the method.");
        step.Action = "scanner.once";
        blocked = false; try { actions.Validate(step); } catch (InvalidOperationException) { blocked = true; }
        Assert(blocked, "Another object's method was offered as a fallback.");
        ScannerKeyenceTcp scanner = new ScannerKeyenceTcp("Offline capability probe");
        string[] names = scanner.GetFlowMethods().Select(method => method.Id).ToArray();
        Assert(names.Contains("device.open") && names.Contains("scanner.once") && !names.Contains("hioki.result") && !scanner.IsConnected, "Inherited and specific capabilities did not match the device.");
    }

    /// <summary>Require selecting an axis before choosing its own table and point index.</summary>
    private static void AxisMethodBindings()
    {
        FlowActions actions = new FlowActions(FlowResources.Capture()) { Simulate = true };
        FlowResource axis = actions.FindResource("hardware/" + groupedAxis.Id + "/axis");
        FlowMethod move = axis.Methods.Single(method => method.Id == "point.move");
        Assert(move.Inputs[0].Name == "Index" && move.Inputs.All(field => field.Name != "Table") && axis.BoundPoints.Id == "hardware/" + groupedAxis.Id + "/points/Position", "Axis methods detached from their configured point table.");
        Assert(actions.Resources.Items.Values.Where(item => item.Kind == "point").All(item => !item.Selectable), "Point tables were offered as independent motion objects.");
        FlowStep step = BoundStep(axis, "point.move");
        double expected = ((FlowPointAxis)axis.BoundPoints.Instance).Positions[0];
        Assert((double)ExecuteBound(actions, step)["Position"] == expected, "Axis points were guessed from the current selection.");
        step.Inputs[0].Reference = "999999";
        bool blocked = false; try { actions.Validate(step); } catch (InvalidOperationException) { blocked = true; }
        Assert(blocked, "Saving accepted a point table outside this axis.");
    }

    /// <summary>Use the true SDK Goto and Home conventions through the actual axis object.</summary>
    private static void AxisDriverSemantics()
    {
        NP_Motor motor = (NP_Motor)hardware.Resources.Entries.Single(item => item.Definition.Id == groupedAxis.Id).Device;
        FieldInfo driverField = typeof(NP_Motor).GetField("dMotor", BindingFlags.Instance | BindingFlags.NonPublic), readyField = typeof(NP_Motor).GetField("ready", BindingFlags.Instance | BindingFlags.NonPublic);
        object previous = driverField.GetValue(motor), wasReady = readyField.GetValue(motor);
        PointDriver driver = new PointDriver(); driverField.SetValue(motor, driver); readyField.SetValue(motor, true);
        try
        {
            FlowActions actions = new FlowActions(FlowResources.Capture());
            FlowResource axis = actions.FindResource("hardware/" + groupedAxis.Id + "/axis");
            FlowStep step = BoundStep(axis, "point.move");
            IDictionary<string, object> result = ExecuteBound(actions, step);
            Assert(driver.Moves == 1 && (double)result["Position"] == driver.Position, "Goto false was interpreted as failure or motion was resent.");
            ExecuteBound(actions, BoundStep(axis, "axis.home"));
            Assert(driver.HomePolls == 2, "HomeReset was mistaken for the homing operation.");
        }
        finally { driverField.SetValue(motor, previous); readyField.SetValue(motor, wasReady); }
    }

    /// <summary>Keep the resource lease until cancellation has stopped the axis.</summary>
    private static void AxisMethodCancellation()
    {
        NP_Motor motor = (NP_Motor)hardware.Resources.Entries.Single(item => item.Definition.Id == groupedAxis.Id).Device;
        FieldInfo driverField = typeof(NP_Motor).GetField("dMotor", BindingFlags.Instance | BindingFlags.NonPublic), readyField = typeof(NP_Motor).GetField("ready", BindingFlags.Instance | BindingFlags.NonPublic);
        object previous = driverField.GetValue(motor), wasReady = readyField.GetValue(motor);
        PointDriver driver = new PointDriver { Hold = true }; driverField.SetValue(motor, driver); readyField.SetValue(motor, true);
        try
        {
            FlowActions actions = new FlowActions(FlowResources.Capture());
            FlowStep step = BoundStep(actions.FindResource("hardware/" + groupedAxis.Id + "/axis"), "point.move");
            using (CancellationTokenSource cancellation = new CancellationTokenSource())
            {
                Task task = actions.Execute(step, new FlowExecutionContext(), cancellation.Token);
                Assert(driver.Moves == 1 && !FlowResourceGate.CanAccess(motor), "Motion did not own the actual axis.");
                cancellation.Cancel();
                bool cancelled = false; try { task.GetAwaiter().GetResult(); } catch (OperationCanceledException) { cancelled = true; }
                Assert(cancelled && driver.Stops == 1 && !driver.Moving && FlowResourceGate.CanAccess(motor), "Cancellation released a moving axis.");
            }
        }
        finally { driverField.SetValue(motor, previous); readyField.SetValue(motor, wasReady); }
    }

    /// <summary>Expose the exact VPP terminals and a judgement output suitable for CASE branching.</summary>
    private static void VisionObjectMethods()
    {
        FlowVisionProbe vision = new FlowVisionProbe { Accept = false };
        using (CapabilityHost container = new CapabilityHost { Name = "VppMethodProbe", Bound = vision })
        {
            RegisterForm(container);
            try
            {
                FlowActions actions = new FlowActions(FlowResources.Capture());
                FlowResource resource = actions.Resources.Items.Values.Single(item => ReferenceEquals(item.Instance, vision));
                FlowStep step = BoundStep(resource, "vision.run"); step.Inputs.Single().Reference = "3.5";
                IDictionary<string, object> result = ExecuteBound(actions, step);
                Assert((double)result["TB:Score"] == 7 && !(bool)result["Accepted"] && vision.Runs == 1 && vision.LastRunResult.IsFlowManaged, "VPP port binding or NG condition output was lost.");
                Assert(resource.Methods.Any(method => method.Id == "vision.image.save") && resource.Methods.Any(method => method.Id == "vision.compensation"), "The VPP's own methods were missing.");
            }
            finally { ((IList)host.GetField("lstForm").GetValue(null)).Remove(container); VisionproInterface.VList.Remove(vision); }
        }
    }

    /// <summary>Reject stale image requests and isolate compensation drafts from published settings.</summary>
    private static void VisionMethodSnapshots()
    {
        FlowVisionProbe vision = new FlowVisionProbe();
        using (CapabilityHost container = new CapabilityHost { Name = "VppSnapshotProbe", Bound = vision })
        {
            RegisterForm(container);
            try
            {
                FlowActions actions = new FlowActions(FlowResources.Capture());
                FlowResource resource = actions.Resources.Items.Values.Single(item => ReferenceEquals(item.Instance, vision));
                long first = (long)ExecuteBound(actions, BoundStep(resource, "vision.run"))["RunId"];
                ExecuteBound(actions, BoundStep(resource, "vision.run"));
                FlowStep image = BoundStep(resource, "vision.image.save"); image.Inputs[0].Reference = first.ToString();
                bool blocked = false; try { ExecuteBound(actions, image); } catch (InvalidOperationException) { blocked = true; }
                Assert(blocked, "Another product's image could be saved with a stale run identity.");
                vision.Config.CompLimits[0].Compensation = 3;
                FlowActions published = new FlowActions(FlowResources.Capture()), draft = new FlowActions(FlowResources.Capture(true));
                FlowResource liveResource = published.Resources.Items[resource.Id], draftResource = draft.Resources.Items[resource.Id];
                Assert((double)ExecuteBound(published, BoundStep(liveResource, "vision.compensation"))["Compensation"] == 1 &&
                    (double)ExecuteBound(draft, BoundStep(draftResource, "vision.compensation"))["Compensation"] == 3, "Unsaved VPP compensation leaked into production.");
                blocked = false; try { published.Validate(BoundStep(liveResource, "vision.run")); } catch (InvalidOperationException) { blocked = true; }
                Assert(blocked, "Running VPP accepted unsaved vision parameters.");
                vision.CommitSettings();
                published = new FlowActions(FlowResources.Capture());
                Assert((double)ExecuteBound(published, BoundStep(published.Resources.Items[resource.Id], "vision.compensation"))["Compensation"] == 3, "Saved compensation snapshot was not published.");
            }
            finally { ((IList)host.GetField("lstForm").GetValue(null)).Remove(container); VisionproInterface.VList.Remove(vision); }
        }
    }

    /// <summary>Keep a selected object, method and active input edit while updating language.</summary>
    private static void ObjectMethodLanguage()
    {
        FlowResource axis = FlowResources.Capture(true).Items["hardware/" + groupedAxis.Id + "/axis"];
        FlowStep step = BoundStep(axis, "point.move");
        using (FlowNodeForm dialog = new FlowNodeForm(editor, step))
        {
            dialog.Show(); Application.DoEvents();
            ((CheckBox)dialog.Controls.Find("showAdvanced", true)[0]).Checked = true;
            FlowInputRow offset = dialog.Controls.Find("inputs", true)[0].Controls.OfType<FlowInputRow>().Single(row => row.FieldName == "Offset");
            TextBox inputValue = (TextBox)offset.Controls.Find("inputLiteral", true)[0];
            inputValue.Focus(); inputValue.Text = "pending offset";
            foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
            {
                SwitchLanguage(language); Application.DoEvents();
                Assert(inputValue.Text == "pending offset" && offset.ReadInput().Reference == "pending offset", "Language refresh replaced the active editor.");
                Assert(((ComboBox)dialog.Controls.Find("targetValue", true)[0]).SelectedValue.ToString() == axis.Id &&
                    ((FlowMethod)((ComboBox)dialog.Controls.Find("actionValue", true)[0]).SelectedItem).Id == "point.move", "Language refresh changed the method binding.");
                using (Bitmap screenshot = new Bitmap(dialog.Width, dialog.Height))
                {
                    dialog.DrawToBitmap(screenshot, new Rectangle(Point.Empty, dialog.Size));
                    screenshot.Save(Path.Combine(root, "object-axis-" + language + ".png"));
                }
            }
            dialog.Close();
        }
    }
}
