using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlphaRap;
using AlphaRap.FlowProgramming;
using AlphaRap.PLC;
using NPSDK;

internal sealed class FrameworkDeclaredDevice : AlphaRap.AbstractDevice
{
    internal int Calls;

    /// <summary>Create an existing device layer containing both explicit and automatic tools.</summary>
    public FrameworkDeclaredDevice(string name) : base(name) { }

    /// <summary>Keep the fixture disconnected without querying hardware.</summary>
    public override bool IsConnected { get { return false; } }

    /// <summary>Do not open hardware while checking metadata.</summary>
    public override void Open() { throw new InvalidOperationException("Unexpected connection."); }

    /// <summary>Close the offline fixture without side effects.</summary>
    public override void Close() { }

    /// <summary>Keep an existing curated tool in the same class as new business methods.</summary>
    public override IEnumerable<FlowMethod> GetFlowMethods(bool includeDraft = false)
    {
        foreach (FlowMethod method in base.GetFlowMethods(includeDraft)) yield return method;
        FlowMethod mapped = new FlowMethod("framework.mapped", "已包装业务", "Mapped business", "Operación publicada") { UsesHardware = false };
        mapped.Outputs.Add(new FlowField("Value", typeof(int)));
        mapped.Invoke = call => Task.FromResult(FlowCall.Result("Value", AlreadyMapped()));
        yield return mapped;
    }

    /// <summary>Supply the underlying operation already represented by a curated capability.</summary>
    [FlowToolIgnore]
    public int AlreadyMapped() { Calls++; return 10; }

    /// <summary>Publish another ordinary method without modifying the explicit catalogue.</summary>
    public string ReadBusiness(string suffix) { Calls++; return DeviceName + suffix; }
}

internal sealed class FrameworkDataPage : DataForm
{
    internal int Calls;

    /// <summary>Expose data processing independently of the base page's persistence infrastructure.</summary>
    public double CalculateAverage(double first, double second) { Calls++; return (first + second) / 2; }
}

internal sealed class FrameworkPlcObject : INOVANCE
{
    internal int Calls;

    /// <summary>Expose a custom PLC operation while retaining the base PLC's standard read/write tools.</summary>
    public int DecodeWord(int word) { Calls++; return word & 255; }
}

internal sealed class FrameworkVisionObject : VpStation
{
    internal int Calls;

    /// <summary>Represent a real derived VPP instance without loading a camera or algorithm.</summary>
    public FrameworkVisionObject() : base("Framework camera", "Framework VPP") { }

    /// <summary>Add a normal business operation beside inherited standard vision capabilities.</summary>
    public bool CheckScore(double score) { Calls++; return score >= 0.9; }
}

internal sealed class FrameworkCapabilityHost : Form
{
    internal readonly FrameworkDeclaredDevice Device = new FrameworkDeclaredDevice("Framework device");
    internal readonly FrameworkDataPage Data = new FrameworkDataPage();
    internal readonly FrameworkPlcObject Plc = new FrameworkPlcObject();
    internal readonly FrameworkVisionObject Vision = new FrameworkVisionObject();

    /// <summary>Make already-created real objects available to the normal page-field capture.</summary>
    public FrameworkCapabilityHost() { Name = "FrameworkCapabilities"; }

    /// <summary>Release the owned data page after the host is removed from resource discovery.</summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing) Data.Dispose();
        base.Dispose(disposing);
    }
}

internal static partial class Cases
{
    /// <summary>Register regression coverage for consistent inheritance across existing business roots.</summary>
    private static void RegisterFrameworkInheritanceCases()
    {
        Test("an explicit capability catalogue does not hide ordinary methods declared in the same device layer", FrameworkExplicitLayer);
        Test("derived DataForm and PLC tools reuse their actual objects while preserving standard capabilities", FrameworkDataAndPlc);
        Test("derived VPP methods preserve standard vision actions without exposing configuration helpers", FrameworkVisionInheritance);
        Test("mapped built-in device methods remain single curated tools after inheritance discovery", FrameworkMappedDevices);
    }

    /// <summary>Capture actual field instances and remove their page reliably after each case.</summary>
    private static void WithFrameworkCapabilities(Action<FrameworkCapabilityHost, FlowActions> test)
    {
        using (FrameworkCapabilityHost page = new FrameworkCapabilityHost())
        {
            RegisterForm(page);
            try { test(page, new FlowActions(FlowResources.Capture())); }
            finally { ((IList)host.GetField("lstForm").GetValue(null)).Remove(page); }
        }
    }

    /// <summary>Require ordinary methods to coexist with a curated declaration on the exact same type.</summary>
    private static void FrameworkExplicitLayer()
    {
        WithFrameworkCapabilities((page, actions) =>
        {
            FlowResource device = AutoResource(actions, page.Device);
            Assert(page.Device.Calls == 0, "Catalogue construction invoked a business method.");
            FlowMethod automatic = AutoMethod(device, "ReadBusiness", typeof(string));
            Assert(device.Methods.Count(method => method.Id == "framework.mapped") == 1,
                "The explicit capability disappeared or was duplicated.");
            Assert(!device.Methods.Any(method => method.Id.StartsWith("auto.") && method.Title[0].Contains("AlreadyMapped")),
                "A method marked as already mapped was exposed again.");
            Assert((string)ExecuteBound(actions, AutoStep(device, automatic, " / result"))["Value"] == "Framework device / result" && page.Device.Calls == 1,
                "The normal method did not execute on the selected device instance.");
        });
    }

    /// <summary>Discover only derived business methods without exposing base configuration or PLC native calls.</summary>
    private static void FrameworkDataAndPlc()
    {
        WithFrameworkCapabilities((page, actions) =>
        {
            FlowResource dataPage = AutoResource(actions, page.Data), plc = AutoResource(actions, page.Plc);
            Assert(dataPage.Methods.Any(method => method.Id == "data.save")
                && plc.Methods.Any(method => method.Id == "plc.read") && plc.Methods.Any(method => method.Id == "plc.write"),
                "A standard data or PLC capability was replaced by automatic tools.");
            Assert((double)ExecuteBound(actions, AutoStep(dataPage, AutoMethod(dataPage, "CalculateAverage", typeof(double), typeof(double)), "2", "6"))["Value"] == 4
                && page.Data.Calls == 1, "The data operation used another page instance.");
            Assert((int)ExecuteBound(actions, AutoStep(plc, AutoMethod(plc, "DecodeWord", typeof(int)), "258"))["Value"] == 2
                && page.Plc.Calls == 1, "The PLC operation used another object instance.");
            string[] titles = dataPage.Methods.Concat(plc.Methods).Where(method => method.Id.StartsWith("auto.")).Select(method => method.Title[0]).ToArray();
            foreach (string helper in new[] { "Dispose", "WriteProductionDraft", "AcceptProductionDraft", "DiscardProductionDraft", "SaveProductionSchema", "ReadPlc", "Connet", "Init_ETH" })
                Assert(!titles.Any(title => title.StartsWith(helper + "(", StringComparison.Ordinal)), "An infrastructure method became a tool: " + helper);
        });
    }

    /// <summary>Keep VPP operations tied to curated run identifiers while allowing derived business calculations.</summary>
    private static void FrameworkVisionInheritance()
    {
        WithFrameworkCapabilities((page, actions) =>
        {
            FlowResource vision = AutoResource(actions, page.Vision);
            Assert(vision.Methods.Count(method => method.Id == "vision.run") == 1, "The standard capture tool changed.");
            string[] automatic = vision.Methods.Where(method => method.Id.StartsWith("auto.")).Select(method => method.Title[0]).ToArray();
            Assert(automatic.Length == 1 && automatic[0].StartsWith("CheckScore(", StringComparison.Ordinal),
                "The VPP catalogue exposed saved-configuration or latest-image helpers.");
            Assert((bool)ExecuteBound(actions, AutoStep(vision, AutoMethod(vision, "CheckScore", typeof(double)), "0.95"))["Value"] && page.Vision.Calls == 1,
                "The custom VPP operation did not use the captured station.");
        });
    }

    /// <summary>Keep established device actions stable without requiring new per-tool registration.</summary>
    private static void FrameworkMappedDevices()
    {
        AlphaRap.AbstractDevice[] devices = { new ScannerKeyenceTcp("Scanner fixture"), new Keyence3DTcp("3D fixture"),
            new AlaphaRap.ATEQ_F620("Leak fixture"), new AlaphaRap.ModBus_RTU("Modbus fixture") };
        string[][] standard = { new[] { "scanner.once" }, new[] { "keyence.trigger", "keyence.change" }, new[] { "ateq.result" }, new[] { "modbus.read" } };
        for (int index = 0; index < devices.Length; index++)
        {
            List<FlowMethod> methods = devices[index].GetFlowMethods().ToList();
            Assert(methods.All(method => !method.Id.StartsWith("auto.", StringComparison.Ordinal)), "A standard mapped device method was published twice.");
            foreach (string id in standard[index]) Assert(methods.Count(method => method.Id == id) == 1, "A standard mapped device action changed: " + id);
        }
    }
}
