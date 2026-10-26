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

internal enum AutoProbeMode { Normal, Extended }

internal abstract class AutoDeviceParent : AlphaRap.AbstractDevice
{
    internal int Calls, Opens, GetterReads, Starts, Stops;

    /// <summary>Create an offline device without declaring flow metadata.</summary>
    protected AutoDeviceParent(string name) : base(name) { }

    /// <summary>Expose connection state without hardware access.</summary>
    public override bool IsConnected { get { return false; } }

    /// <summary>Detect accidental property execution during metadata discovery.</summary>
    public string Diagnostic { get { GetterReads++; throw new InvalidOperationException("A property getter was invoked."); } }

    /// <summary>Record a connection attempt without touching equipment.</summary>
    public override void Open() { Opens++; }

    /// <summary>Close the offline fixture without external effects.</summary>
    public override void Close() { }

    /// <summary>Record the actual parameterless lifecycle start.</summary>
    public override void StartRunning() { Starts++; }

    /// <summary>Record the actual parameterless lifecycle stop.</summary>
    public override void StopRunning() { Stops++; }

    /// <summary>Supply a business method inherited through an intermediate device class.</summary>
    public int ReadInherited(int count) { Calls++; return count + 10; }
}

internal sealed class AutoBusinessDevice : AutoDeviceParent
{
    /// <summary>Expose a business overload alongside the inherited lifecycle operation.</summary>
    public int StartRunning(int speed) { return speed; }

    /// <summary>Expose a stop overload without making lifecycle lookup ambiguous.</summary>
    public int StopRunning(int reason) { return reason; }

    internal readonly TaskCompletionSource<bool> Started = new TaskCompletionSource<bool>();
    internal readonly TaskCompletionSource<int> Release = new TaskCompletionSource<int>();
    internal bool OwnedDuringCall;
    internal int Cancelled;

    /// <summary>Create one actual independently callable device instance.</summary>
    public AutoBusinessDevice(string name) : base(name) { }

    /// <summary>Use scalar parameters and optional defaults in a normal business method.</summary>
    public double Weigh(double grams, int pieces = 2) { Calls++; return grams * pieces; }

    /// <summary>Return the identity of the actual selected device.</summary>
    public string Identify(string suffix) { Calls++; return DeviceName + suffix; }

    /// <summary>Represent a business action with no return value.</summary>
    public void ResetCount() { Calls = 0; }

    /// <summary>Provide a numeric overload with a distinct persistent identity.</summary>
    public int Select(int value) { Calls++; return value + 1; }

    /// <summary>Provide a text overload with the same CLR method name.</summary>
    public string Select(string value) { Calls++; return value + "!"; }

    /// <summary>Accept an enum as a finite choice rather than an untyped object.</summary>
    public bool CheckMode(AutoProbeMode mode) { Calls++; return mode == AutoProbeMode.Extended; }

    /// <summary>Return a value and a named out parameter from one operation.</summary>
    public bool TryRead(out double weight) { Calls++; weight = 1.25; return true; }

    /// <summary>Expose the initial and final values of a ref parameter.</summary>
    public void Adjust(ref int count) { Calls++; count += 3; }

    /// <summary>Return an asynchronously computed typed value.</summary>
    public async Task<double> ReadAsync(double grams, CancellationToken cancellationToken)
    {
        await Task.Delay(1, cancellationToken).ConfigureAwait(false);
        Calls++;
        return grams + 0.5;
    }

    /// <summary>Wait for cancellation using the injected invocation token.</summary>
    public async Task WaitForCancel(CancellationToken cancellationToken)
    {
        Calls++;
        Started.TrySetResult(true);
        try { await Task.Delay(Timeout.Infinite, cancellationToken).ConfigureAwait(false); }
        catch (OperationCanceledException) { Cancelled++; throw; }
    }

    /// <summary>Hold the actual instance until a non-cooperative operation finishes.</summary>
    public async Task<int> WaitForSignal()
    {
        Calls++;
        OwnedDuringCall = FlowResourceGate.CanAccess(this);
        Started.TrySetResult(true);
        return await Release.Task.ConfigureAwait(false);
    }

    /// <summary>Expose the original business exception to the flow runtime.</summary>
    public int Fail() { throw new InvalidOperationException("Auto business failure"); }

    /// <summary>Represent a static helper that must not become an instance action.</summary>
    public static int StaticHelper(int value) { return value; }

    /// <summary>Represent an unsupported generic helper.</summary>
    public T GenericHelper<T>(T value) { return value; }

    /// <summary>Represent an unsupported object-shaped parameter.</summary>
    public int ObjectHelper(object value) { return 0; }

    /// <summary>Represent an unsupported framework collection parameter.</summary>
    public int CollectionHelper(List<int> values) { return values.Count; }

    /// <summary>Return structured samples through the supported typed array result.</summary>
    public double[] ReadSamples() { return new[] { 1.25, 2.5 }; }

    /// <summary>Consume a real bound array without parsing a textual placeholder.</summary>
    public double SumSamples(double[] values) { return values.Sum(); }

    /// <summary>Provide optional display and simulation metadata without declaring an IFlowObject catalogue.</summary>
    [FlowTool("校准读取", "Read calibration", "Leer calibración", Id = "probe.calibrated", SimulationMethod = "PreviewCalibration")]
    public int ReadCalibration(int count) { Calls++; return count + 11; }

    /// <summary>Use different CLR parameter names while retaining the actual tool input contract.</summary>
    public int PreviewCalibration(int amount) { return amount + 100; }

    /// <summary>Keep a public helper callable by code without showing it as a flow action.</summary>
    [FlowToolIgnore]
    public int HiddenHelper() { return 99; }
}

internal sealed class AutoAxisProbe : NP_Motor
{
    /// <summary>Supply a derived axis business operation without declaring metadata.</summary>
    public double OffsetFor(double origin, double offset) { return origin + offset; }
}

internal sealed class AutoInputProbe : NP_Input
{
    /// <summary>Supply a derived input operation without reading physical IO.</summary>
    public bool DebounceValue(bool value) { return value; }
}

internal sealed class AutoCylinderProbe : NP_Cylinder
{
    /// <summary>Supply a cylinder calculation without actuating its outputs.</summary>
    public int StrokeCode(int value) { return value + 100; }
}

internal class AutoDeclaredOutput : NP_Output, IFlowObject
{
    /// <summary>Keep an explicitly published capability alongside standard IO operations.</summary>
    public IEnumerable<FlowMethod> GetFlowMethods(bool includeDraft = false)
    {
        FlowMethod method = new FlowMethod("probe.explicit", "显式输出", "Explicit output", "Salida explícita") { UsesHardware = false };
        method.Inputs.Add(new FlowField("value", typeof(int), "2"));
        method.Outputs.Add(new FlowField("Value", typeof(int)));
        method.Invoke = call => Task.FromResult(FlowCall.Result("Value", Published(call.Get<int>("value"))));
        yield return method;
    }

    /// <summary>Implement the method already represented by the explicit capability declaration.</summary>
    public int Published(int value) { return value + 20; }
}

internal sealed class AutoOutputProbe : AutoDeclaredOutput
{
    /// <summary>Supply a newly derived output operation without changing the inherited declaration.</summary>
    public int PulseCode(int value) { return value + 30; }
}

internal sealed class AutoMethodHost : Form
{
    public readonly AutoBusinessDevice First = new AutoBusinessDevice("Automatic device A");
    public readonly AutoBusinessDevice Second = new AutoBusinessDevice("Automatic device B");
    internal readonly AutoAxisProbe Axis = new AutoAxisProbe { Name = "AutoAxis", Text = "Automatic axis" };
    internal readonly AutoInputProbe Input = new AutoInputProbe { Name = "AutoInput", Text = "Automatic input" };
    internal readonly AutoOutputProbe Output = new AutoOutputProbe { Name = "AutoOutput", Text = "Automatic output" };
    internal readonly AutoCylinderProbe Cylinder = new AutoCylinderProbe { Name = "AutoCylinder", Text = "Automatic cylinder" };

    /// <summary>Expose only existing device fields and hardware controls to normal page discovery.</summary>
    public AutoMethodHost()
    {
        Name = "AutomaticMethods";
        Controls.Add(Axis); Controls.Add(Input); Controls.Add(Output); Controls.Add(Cylinder);
        NP_PositionTable points = new NP_PositionTable { Name = "AutoPoints" };
        points.DataSet1 = new DataTable("Automatic points");
        points.DataSet1.Columns.Add("Position", typeof(double));
        points.ParameterSet.Clear();
        points.ParameterSet.Add(new PositionColumn { Axis = Axis, Name = "Position", Limit_Low = -100, Limit_High = 100 });
        points.DataSet1.Rows.Add(12.5); points.DataSet1.Rows.Add(25.0); points.DataSet1.AcceptChanges();
        Controls.Add(points);
    }
}

internal static partial class Cases
{
    /// <summary>Register inheritance-only tools without any per-tool registration or metadata adapter.</summary>
    private static void RegisterAutoMethodCases()
    {
        Test("inherited device business methods appear without GetFlowMethods and metadata browsing has no side effects", AutoMethodDiscovery);
        Test("automatic methods dispatch typed inputs to the selected actual device instance", AutoMethodTypedDispatch);
        Test("automatic overloads enums ref and out parameters keep exact typed contracts", AutoMethodSignatures);
        Test("automatic asynchronous methods inject cancellation and preserve business exceptions", AutoMethodAsync);
        Test("automatic non-cooperative calls retain the actual resource lease through cancellation", AutoMethodOwnership);
        Test("derived IO and axis methods coexist with standard actions and the axis bound point table", AutoMethodHardware);
        Test("explicit IO capabilities remain authoritative while a new derived layer is discovered automatically", AutoMethodExplicitInheritance);
        Test("automatic tools appear in three languages and insert through the existing toolbox command", AutoMethodToolbox);
        Test("automatic action identities survive file reload and bind the reconstructed actual object", AutoMethodPersistence);
        Test("optional tool titles and simulation metadata do not require a registration catalogue", AutoMethodOptionalMetadata);
    }

    /// <summary>Register a temporary real page and always remove its resources from subsequent captures.</summary>
    private static void WithAutoMethods(Action<AutoMethodHost, FlowActions> test)
    {
        using (AutoMethodHost page = new AutoMethodHost())
        {
            RegisterForm(page);
            try { test(page, new FlowActions(FlowResources.Capture())); }
            finally { ((IList)host.GetField("lstForm").GetValue(null)).Remove(page); }
        }
    }

    /// <summary>Find the actual resource by reference so translated labels cannot alter dispatch.</summary>
    private static FlowResource AutoResource(FlowActions actions, object instance)
    {
        return actions.Resources.Items.Values.Single(item => ReferenceEquals(item.Instance, instance));
    }

    /// <summary>Locate a generated signature by its business method name and exact input types.</summary>
    private static FlowMethod AutoMethod(FlowResource resource, string name, params Type[] inputTypes)
    {
        return resource.Methods.Single(method => method.Id.StartsWith("auto.", StringComparison.Ordinal)
            && method.Title[0].Contains(name) && method.Inputs.Select(field => field.Type).SequenceEqual(inputTypes));
    }

    /// <summary>Build one ordinary configured node with fixed input values in declaration order.</summary>
    private static FlowStep AutoStep(FlowResource resource, FlowMethod method, params string[] values)
    {
        FlowStep step = BoundStep(resource, method.Id);
        for (int index = 0; index < values.Length; index++) step.Inputs[index].Reference = values[index];
        return step;
    }

    /// <summary>Check the generated catalogue without invoking constructors, properties or methods while browsing.</summary>
    private static void AutoMethodDiscovery()
    {
        WithAutoMethods((page, actions) =>
        {
            string[] initial = AutoResource(actions, page.First).Methods.Select(method => method.Id).ToArray();
            FlowMethod inherited = AutoMethod(AutoResource(actions, page.First), "ReadInherited", typeof(int));
            Assert(inherited.Inputs.Single().Name == "count" && inherited.Outputs.Single().Name == "Value", "Inherited business signature was lost.");
            for (int index = 0; index < 3; index++)
            {
                FlowResource current = AutoResource(new FlowActions(FlowResources.Capture()), page.First);
                Assert(initial.SequenceEqual(current.Methods.Select(method => method.Id)), "Metadata identities or ordering changed between captures.");
            }
            Assert(page.First.Calls == 0 && page.First.Opens == 0 && page.First.GetterReads == 0 && page.Second.Calls == 0,
                "Metadata discovery executed a device operation or arbitrary property.");
            Assert(initial.Count(id => id == "device.open") == 1 && initial.Count(id => id == "device.close") == 1,
                "Standard device capabilities were duplicated or removed.");
            string[] generated = AutoResource(actions, page.First).Methods.Where(method => method.Id.StartsWith("auto.", StringComparison.Ordinal)).Select(method => method.Title[0]).ToArray();
            foreach (string omitted in new[] { "StaticHelper", "GenericHelper", "ObjectHelper", "CollectionHelper", "Diagnostic", "Open", "Close", "GetFlowMethods", "ToString", "GetType", "HiddenHelper", "PreviewCalibration" })
                Assert(!generated.Any(title => title.Contains(omitted)), "Unsupported or already mapped member was exposed: " + omitted);
            FlowResource resource = AutoResource(actions, page.First);
            Assert(AutoMethod(resource, "StartRunning", typeof(int)) != null && AutoMethod(resource, "StopRunning", typeof(int)) != null,
                "A business lifecycle overload was not discovered.");
            ExecuteBound(actions, BoundStep(resource, "device.start"));
            ExecuteBound(actions, BoundStep(resource, "device.stop"));
            Assert(page.First.Starts == 1 && page.First.Stops == 1 && page.Second.Starts == 0 && page.Second.Stops == 0,
                "Standard lifecycle operations did not call the selected instance's parameterless override.");
        });
    }

    /// <summary>Bind the same real object for optional simulation and prevent undeclared simulation from reaching hardware.</summary>
    private static void AutoMethodOptionalMetadata()
    {
        WithAutoMethods((page, actions) =>
        {
            FlowResource resource = AutoResource(actions, page.First);
            FlowMethod method = resource.Methods.Single(item => item.Id == "probe.calibrated");
            Assert(method.Title.SequenceEqual(new[] { "校准读取", "Read calibration", "Leer calibración" }), "Optional three-language titles were not preserved.");
            actions.Simulate = true;
            Assert((int)ExecuteBound(actions, AutoStep(resource, method, "2"))["Value"] == 102 && page.First.Calls == 0,
                "Simulation called hardware or used the simulator's parameter names as bindings.");
            bool rejected = false;
            try { ExecuteBound(actions, AutoStep(resource, AutoMethod(resource, "Weigh", typeof(double), typeof(int)), "2", "3")); }
            catch (InvalidOperationException) { rejected = true; }
            Assert(rejected && page.First.Calls == 0, "An automatic hardware method without a simulator executed in simulation mode.");
            actions.Simulate = false;
            Assert((int)ExecuteBound(actions, AutoStep(resource, method, "2"))["Value"] == 13 && page.First.Calls == 1,
                "Optional simulation metadata replaced actual execution.");
        });
    }

    /// <summary>Dispatch primitives, optional defaults and void methods through ordinary action validation.</summary>
    private static void AutoMethodTypedDispatch()
    {
        WithAutoMethods((page, actions) =>
        {
            FlowResource first = AutoResource(actions, page.First), second = AutoResource(actions, page.Second);
            FlowMethod weigh = AutoMethod(first, "Weigh", typeof(double), typeof(int));
            Assert(weigh.Inputs.Select(field => field.Name).SequenceEqual(new[] { "grams", "pieces" }) && weigh.Inputs[1].DefaultValue == "2",
                "Parameter names or optional defaults were not inherited from the method.");
            Assert((double)ExecuteBound(actions, AutoStep(first, weigh, "1.25"))["Value"] == 2.5, "Optional typed inputs did not reach the actual method.");
            FlowStep firstIdentity = AutoStep(first, AutoMethod(first, "Identify", typeof(string)), " / result");
            FlowStep secondIdentity = AutoStep(second, AutoMethod(second, "Identify", typeof(string)), " / result");
            Assert((string)ExecuteBound(actions, firstIdentity)["Value"] == "Automatic device A / result"
                && (string)ExecuteBound(actions, secondIdentity)["Value"] == "Automatic device B / result"
                && page.First.Calls == 2 && page.Second.Calls == 1, "The selected instance was replaced by another same-type device.");
            FlowMethod reset = AutoMethod(first, "ResetCount");
            Assert(reset.Outputs.Count == 0 && ExecuteBound(actions, AutoStep(first, reset)).Count == 0 && page.First.Calls == 0,
                "A void method invented a return value or did not invoke the selected object.");
        });
    }

    /// <summary>Keep overload identities, enum choices and ref/out parameter names distinct.</summary>
    private static void AutoMethodSignatures()
    {
        WithAutoMethods((page, actions) =>
        {
            FlowResource resource = AutoResource(actions, page.First);
            FlowMethod number = AutoMethod(resource, "Select", typeof(int)), text = AutoMethod(resource, "Select", typeof(string));
            Assert(number.Id != text.Id && (int)ExecuteBound(actions, AutoStep(resource, number, "4"))["Value"] == 5
                && (string)ExecuteBound(actions, AutoStep(resource, text, "part"))["Value"] == "part!", "Overloaded business methods share an identity or dispatch path.");
            FlowMethod mode = AutoMethod(resource, "CheckMode", typeof(AutoProbeMode));
            Assert(mode.Inputs.Single().Options.Select(option => option.Value).OrderBy(value => value).SequenceEqual(new[] { "Extended", "Normal" })
                && (bool)ExecuteBound(actions, AutoStep(resource, mode, "Extended"))["Value"], "Enum choices were lost or could not execute.");
            FlowMethod read = AutoMethod(resource, "TryRead");
            IDictionary<string, object> result = ExecuteBound(actions, AutoStep(resource, read));
            Assert(read.Inputs.Count == 0 && read.Outputs.Any(field => field.Name == "weight" && field.Type == typeof(double))
                && (bool)result["Value"] && (double)result["weight"] == 1.25, "An out parameter was incorrectly required as input or lost its declared output.");
            FlowMethod adjust = AutoMethod(resource, "Adjust", typeof(int));
            Assert(adjust.Inputs[0].Name == "count" && adjust.Outputs.Single().Name == "count"
                && (int)ExecuteBound(actions, AutoStep(resource, adjust, "4"))["count"] == 7, "A ref parameter did not preserve both input and final output.");
            double[] samples = (double[])ExecuteBound(actions, AutoStep(resource, AutoMethod(resource, "ReadSamples")))["Value"];
            FlowStep sum = AutoStep(resource, AutoMethod(resource, "SumSamples", typeof(double[])));
            actions.Resources.Parameters["Automatic samples"] = samples;
            sum.Inputs[0].Source = "Parameter"; sum.Inputs[0].Reference = "Automatic samples";
            Assert(samples.SequenceEqual(new[] { 1.25, 2.5 }) && (double)ExecuteBound(actions, sum)["Value"] == 3.75,
                "An automatic array result or typed bound array input was lost.");
        });
    }

    /// <summary>Inject invocation cancellation without exposing it as a binding field and unwrap business faults.</summary>
    private static void AutoMethodAsync()
    {
        WithAutoMethods((page, actions) =>
        {
            FlowResource resource = AutoResource(actions, page.First);
            FlowMethod read = AutoMethod(resource, "ReadAsync", typeof(double));
            Assert(read.Inputs.Single().Name == "grams" && read.Outputs.Single().Type == typeof(double)
                && (double)ExecuteBound(actions, AutoStep(resource, read, "2"))["Value"] == 2.5, "Task<T> output or injected token was exposed incorrectly.");
            FlowMethod wait = AutoMethod(resource, "WaitForCancel");
            using (CancellationTokenSource cancellation = new CancellationTokenSource())
            {
                Task pending = actions.Execute(AutoStep(resource, wait), new FlowExecutionContext(), cancellation.Token);
                ActionUntil(() => page.First.Started.Task.IsCompleted, "An automatically discovered asynchronous method did not start.");
                cancellation.Cancel();
                bool cancelled = false;
                try { pending.GetAwaiter().GetResult(); } catch (OperationCanceledException) { cancelled = true; }
                Assert(wait.Inputs.Count == 0 && wait.Outputs.Count == 0 && cancelled && page.First.Cancelled == 1 && FlowResourceGate.CanAccess(page.First),
                    "Cancellation was not injected or left the real device locked.");
            }
            bool original = false;
            try { ExecuteBound(actions, AutoStep(resource, AutoMethod(resource, "Fail"))); }
            catch (InvalidOperationException error) { original = error.Message == "Auto business failure"; }
            Assert(original, "Reflection wrapped or replaced the original business exception.");
        });
    }

    /// <summary>Retain resource ownership and reject delayed outputs while a cancelled operation drains.</summary>
    private static void AutoMethodOwnership()
    {
        WithAutoMethods((page, actions) =>
        {
            FlowResource resource = AutoResource(actions, page.First);
            FlowStep step = AutoStep(resource, AutoMethod(resource, "WaitForSignal"));
            using (CancellationTokenSource cancellation = new CancellationTokenSource())
            {
                Task pending = actions.Execute(step, new FlowExecutionContext(), cancellation.Token);
                try
                {
                    ActionUntil(() => page.First.Started.Task.IsCompleted, "The non-cooperative automatic operation did not start.");
                    cancellation.Cancel();
                    Assert(page.First.OwnedDuringCall && !FlowResourceGate.CanAccess(page.First) && !pending.IsCompleted,
                        "Cancellation released the device before its actual operation returned.");
                    page.First.Release.TrySetResult(42);
                    bool cancelled = false;
                    try { pending.GetAwaiter().GetResult(); } catch (OperationCanceledException) { cancelled = true; }
                    Assert(cancelled && page.First.Calls == 1 && FlowResourceGate.CanAccess(page.First), "A cancelled delayed result escaped or the actual instance remained locked.");
                }
                finally { page.First.Release.TrySetResult(42); }
            }
        });
    }

    /// <summary>Append derived IO and motor methods without replacing the standard actual-object adapters.</summary>
    private static void AutoMethodHardware()
    {
        WithAutoMethods((page, actions) =>
        {
            FlowResource axis = AutoResource(actions, page.Axis), input = AutoResource(actions, page.Input), output = AutoResource(actions, page.Output), cylinder = AutoResource(actions, page.Cylinder);
            Assert(axis.Methods.Any(method => method.Id == "axis.home") && axis.Methods.Any(method => method.Id == "point.move")
                && input.Methods.Any(method => method.Id == "io.read") && input.Methods.Any(method => method.Id == "io.wait")
                && output.Methods.Any(method => method.Id == "io.set") && output.Methods.Any(method => method.Id == "io.output.read")
                && cylinder.Methods.Any(method => method.Id == "cylinder.set"), "Derived resource methods replaced standard motion or IO operations.");
            Assert(axis.BoundPoints != null && axis.BoundPoints.Points.Count == 2
                && ReferenceEquals(((FlowPointAxis)axis.BoundPoints.Instance).Motor, page.Axis)
                && axis.Methods.Single(method => method.Id == "point.move").Inputs[0].Options.Count == 2,
                "Derived motor discovery detached the actual axis from its own point table.");
            Assert((double)ExecuteBound(actions, AutoStep(axis, AutoMethod(axis, "OffsetFor", typeof(double), typeof(double)), "2", "3"))["Value"] == 5
                && (bool)ExecuteBound(actions, AutoStep(input, AutoMethod(input, "DebounceValue", typeof(bool)), "True"))["Value"]
                && (int)ExecuteBound(actions, AutoStep(output, AutoMethod(output, "PulseCode", typeof(int)), "2"))["Value"] == 32
                && (int)ExecuteBound(actions, AutoStep(cylinder, AutoMethod(cylinder, "StrokeCode", typeof(int)), "4"))["Value"] == 104,
                "A derived hardware business method was not callable on its selected actual instance.");
            foreach (FlowResource resource in new[] { axis, input, output, cylinder })
                Assert(!resource.Methods.Where(method => method.Id.StartsWith("auto.", StringComparison.Ordinal)).Any(method => new[] { "Dispose", "Refresh", "Show", "Focus", "CreateControl", "GetType" }.Any(name => method.Title[0].Contains(name))),
                    "Inherited WinForms infrastructure was exposed as an equipment operation.");
        });
    }

    /// <summary>Preserve explicit metadata while automatically extending only the newly derived business layer.</summary>
    private static void AutoMethodExplicitInheritance()
    {
        WithAutoMethods((page, actions) =>
        {
            FlowResource resource = AutoResource(actions, page.Output);
            Assert(resource.Methods.Count(method => method.Id == "probe.explicit") == 1
                && !resource.Methods.Any(method => method.Id.StartsWith("auto.", StringComparison.Ordinal) && method.Title[0].Contains("Published")),
                "An explicitly declared capability was removed or rediscovered as a duplicate automatic action.");
            Assert((int)ExecuteBound(actions, AutoStep(resource, resource.Methods.Single(method => method.Id == "probe.explicit"), "3"))["Value"] == 23
                && AutoMethod(resource, "PulseCode", typeof(int)) != null, "The inherited declaration blocked a newly derived public business method.");
        });
    }

    /// <summary>Expose inherited methods through the existing translated toolbox and ordinary insertion event.</summary>
    private static void AutoMethodToolbox()
    {
        WithAutoMethods((page, actions) =>
        {
            using (FlowToolboxControl toolbox = new FlowToolboxControl())
            {
                toolbox.BindActions(actions);
                TreeView tree = ActionControl<TreeView>(toolbox, "toolboxEntries");
                FlowResource resource = AutoResource(actions, page.First);
                string action = AutoMethod(resource, "Weigh", typeof(double), typeof(int)).Id;
                string entryId = null;
                foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
                {
                    SwitchLanguage(language); Application.DoEvents();
                    FlowToolboxEntry[] entries = tree.Nodes.Cast<TreeNode>().SelectMany(group => group.Nodes.Cast<TreeNode>()).Select(node => (FlowToolboxEntry)node.Tag).ToArray();
                    FlowToolboxEntry entry = entries.Single(item => item.TargetId == resource.Id && item.ActionId == action);
                    Assert(entry.ExecutionMode == "Action" && (entryId == null || entry.Id == entryId)
                        && entries.All(item => item.ExecutionMode == "Action" || item.ExecutionMode == "None"), "Language refresh changed an automatic action identity or restored a retired node mode.");
                    entryId = entry.Id;
                    tree.SelectedNode = tree.Nodes.Cast<TreeNode>().SelectMany(group => group.Nodes.Cast<TreeNode>()).Single(node => ((FlowToolboxEntry)node.Tag).Id == entryId);
                }
                FlowToolboxEntry inserted = null; toolbox.InsertRequested += value => inserted = value;
                typeof(Control).GetMethod("OnKeyDown", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(tree, new object[] { new KeyEventArgs(Keys.Enter) });
                Assert(inserted != null && inserted.TargetId == resource.Id && inserted.ActionId == action, "The normal toolbox command did not insert the automatically discovered equipment action.");
            }
        });
        SwitchLanguage(LanguageType.Chinese);
    }

    /// <summary>Round-trip stable target and signature identities and resolve a newly constructed page instance.</summary>
    private static void AutoMethodPersistence()
    {
        string path = Path.Combine(root, "automatic-methods.xml");
        string target = null, action = null;
        WithAutoMethods((page, actions) =>
        {
            FlowResource resource = AutoResource(actions, page.First);
            FlowStep step = AutoStep(resource, AutoMethod(resource, "Weigh", typeof(double), typeof(int)), "3.5", "4");
            FlowProgram program = new FlowProgram { ModuleId = "AutomaticMethods/Persisted", Mode = "Program", AutoEntry = step.Id };
            program.Steps.Add(step);
            FlowProject project = new FlowProject(); project.Modules.Add(program);
            FlowProgramFile.Save(path, project);
            target = step.Target; action = step.Action;
            Assert(page.First.Calls == 0, "Saving an automatic method invoked the equipment.");
        });
        WithAutoMethods((page, actions) =>
        {
            FlowStep restored = FlowProgramFile.Load(path).Modules.Single().Steps.Single();
            Assert(restored.Target == target && restored.Action == action && AutoResource(actions, page.First).Id == target,
                "Saving or reconstructing the equipment changed its persistent target or method identity.");
            Assert((double)ExecuteBound(actions, restored)["Value"] == 14 && page.First.Calls == 1 && page.Second.Calls == 0,
                "A saved automatic method retained an old object or invoked another instance after reload.");
        });
    }
}
