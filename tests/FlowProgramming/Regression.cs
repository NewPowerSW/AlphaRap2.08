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
using AlphaRap.Configuration;
using AlphaRapLibrary;
using NPSDK;
using NPSDK.Motion;

internal static class Program
{
    /// <summary>Resolve runtime dependencies before loading application types.</summary>
    [STAThread]
    private static int Main(string[] args)
    {
        AppDomain.CurrentDomain.AssemblyResolve += (sender, request) =>
        {
            string name = new AssemblyName(request.Name).Name;
            foreach (string folder in new[] { args[0], @"C:\Program Files\Cognex\VisionPro\ReferencedAssemblies" })
                foreach (string extension in new[] { ".dll", ".exe" })
                {
                    string file = Path.Combine(folder, name + extension);
                    if (File.Exists(file)) return Assembly.LoadFrom(file);
                }
            return null;
        };
        return Cases.Run(args);
    }
}

internal sealed class FutureStation : Flow_BaseForm
{
    public int LegacyStarts;
    public readonly DesignerProbe Capability = new DesignerProbe();
    /// <summary>Detect accidental execution of an existing application callback.</summary>
    public override void StartRun() { LegacyStarts++; }
    public readonly Panel Canvas = new Panel { Name = "canvas", Dock = DockStyle.Fill };
    public readonly Flow_Chart Existing = new Flow_Chart { Name = "existing", text = "Existing callback", Location = new Point(40, 100) };
    /// <summary>Represent an independently added subclass with a nested node.</summary>
    public FutureStation() { Name = "FutureStation"; Canvas.Controls.Add(Existing); Controls.Add(Canvas); }
}

internal sealed class JogDriver : MotorBaseInterface
{
    public int Starts, Stops;
    private bool moving;
    /// <summary>Record a positive jog without touching hardware.</summary>
    public override void JogP() { Starts++; moving = true; }
    /// <summary>Record a negative jog without touching hardware.</summary>
    public override void JogN() { Starts++; moving = true; }
    /// <summary>Complete a test stop immediately.</summary>
    public override void FastStop() { Stops++; moving = false; }
    /// <summary>Expose only the fake driver's motion state.</summary>
    public override bool GetBusy() { return moving; }
}

internal sealed class InitializationProbe : ControlBaseInterface
{
    public int Starts;
    /// <summary>Count driver initialization without physical IO.</summary>
    public override void Initial() { Starts++; }
}

internal static partial class Cases
{
    private static int count;
    private static int failures;
    private static string root;
    private static Type host;
    private static DataForm data;
    private static ModuleBaseForm recipe;
    private static HardForm hardware;
    private static ManualForm ioPage;
    private static FutureStation station;
    private static FlowModuleEditor editor;
    private static FlowStep model;
    private static FlowStep measured;
    private static FlowStep save;
    private static SystemSetting constantForm;
    private static HardwareDefinition groupedAxis, groupedInput, groupedOutput;

    /// <summary>Run offline business, persistence, isolation and UI regression scenarios.</summary>
    public static int Run(string[] args)
    {
        Application.EnableVisualStyles();
        root = Path.GetFullPath(args[1]);
        Directory.CreateDirectory(root);
        Environment.CurrentDirectory = root;
        SysPara.SettingDataDirectory = Path.Combine(root, "Settings");
        SysPara.LanguageDataDirectory = Path.Combine(root, "LanguageData");
        SysPara.AlarmTableDirectory = Path.Combine(root, "AlarmTable");
        SysPara.RecipeName = "Model-A";
        SysPara.RecipeDataDirectory = @".\Recipes"; Directory.CreateDirectory(SysPara.RecipeDataDirectory);
        SysPara.SystemMode = RunMode.IDLE;
        SysPara.UserPermission = PermissionType.Administrator;
        SysPara.UserName = "Regression";
        CopyFolder(Path.Combine(args[2], "LanguageData"), SysPara.LanguageDataDirectory);
        CopyFolder(Path.Combine(args[2], "AlarmTable"), SysPara.AlarmTableDirectory);
        host = typeof(SysPara).Assembly.GetType("AlphaRap.MiddleLayer");
        ProductionDataStore.SetRoot(Path.Combine(root, "Production"));
        Test("future Flow_BaseForm subclass attaches without a station allowlist", AttachFuture);
        Test("recipe snapshot excludes uncommitted edits", RecipeSnapshot);
        Test("point table resolves ParameterSet when WorkMotor is null", PointBinding);
        Test("configured scanner instance is reused without opening it", DeviceInstance);
        Test("named results flow through decisions into the real DataForm storage", ProductionChain);
        Test("production retry does not duplicate a saved record", DuplicateRecord);
        Test("new fields expand today's header without dropping previous data", SchemaEvolution);
        Test("point simulation checks actual index and explicit offset", PointSimulation);
        Test("ProductManagerForm recipe loading changes parameters and points together", ProductManagerRecipe);
        Test("timeout alarms once without taking CASE2 or resending the action", Timeout);
        Test("NEXT CASE1 CASE2 CASE3 each use their own condition and target", NativeBranches);
        Test("each exit has an independent AND or OR condition group", BranchGroups);
        Test("action exceptions alarm and do not route to CASE2", ExceptionIsNotCase);
        Test("an independent timeout alarms before a blocking action returns", BlockingTimeout);
        Test("legacy exit files require review and preserve native connection identities", LegacyExits);
        Test("cancellation retains ownership until the underlying action finishes", StopLease);
        Test("cross-cycle and expired run outputs are rejected", ResultIsolation);
        Test("condition waiting observes another completed flow without repeating the action", WaitForOutput);
        Test("cycle keys cannot reuse previous product results", CycleReuse);
        Test("completing a product cycle releases its outputs before reuse", CompleteCycle);
        Test("engineering drafts do not change the published execution mode", PublishedDraft);
        Test("SDK lifecycle dispatch invokes the published designer flow", Lifecycle);
        Test("main start button uses configured module initialization state", MainStartReady);
        Test("save/reload restores nodes, layout, links and deleted original nodes", Persistence);
        Test("damaged project does not overwrite the active design", CorruptLoad);
        Test("three-language node editor retains pending values", EditorLanguages);
        Test("node category object and action changes rebuild the correct input fields", NodeSelections);
        Test("editor defaults to NEXT and manually adds independent CASE conditions", NativeBranchEditor);
        Test("condition modes are exclusive and retain edits across language changes and reload", BranchConditionModes);
        Test("node drag captures the mouse and preserves double-click configuration", MouseEditing);
        Test("adding nodes uses the selected page and keeps the new node visible", AddVisibleNode);
        Test("node deletion rejects input and CASE output consumers and clears links", NodeDeletion);
        Test("module deletion checks modeled and hidden nested live FlowChat nodes", ModuleDeletionGuards);
        Test("empty inherited module deletion persists and cannot restore legacy callbacks", ModuleDeletionPersistence);
        Test("existing system flags migrate to persistent Setting constants", ConstantExamples);
        Test("constant CRUD uses saved typed snapshots and stable keys", ConstantCrud);
        Test("ProductManager recipe switching changes Recipe constants and preserves Setting", ConstantRecipes);
        Test("referenced constants cannot be renamed deleted or retyped", ConstantReferences);
        Test("constant validation and failed writes preserve committed values", ConstantFailures);
        Test("constant table and editor follow three languages without losing drafts", ConstantLanguages);
        Test("axis and IO groups reuse one real instance and expose double-click settings", HardwareGroups);
        Test("grouped hardware and recipe points are selectable flow resources", HardwareBindings);
        Test("resource definitions and current-recipe points reload independently", HardwarePersistence);
        Test("resource deletion rejects action parameter and lease references", HardwareDeletion);
        Test("grouped resource editor follows language changes without committing edits", HardwareLanguages);
        Test("SDK refresh survives adding and disposing registered controls", HardwareRegistry);
        Test("continuous jog survives capture gain and stops on release or capture loss", JogCapture);
        Test("SDK startup leaves host-managed hardware initialization to application flows", DeferredInitialization);
        Test("axis and IO pages create resources through the actual add and confirm buttons", HardwareAddUi);
        Test("resource constant and point menus target the actual right-click object", ContextTargets);
        Test("CASE condition and production-field drafts delete through context menus", DraftMenus);
        Test("new object methods are discovered and invoked without changing the designer catalog", ObjectContract);
        Test("axis methods bind only their own point tables and use typed fields", AxisMethodBindings);
        Test("Goto false means movement in progress and Home follows the driver state machine", AxisDriverSemantics);
        Test("cancelling an axis method stops the real instance before releasing its lease", AxisMethodCancellation);
        Test("VPP methods expose actual terminals and allow NEXT CASE conditions to handle NG", VisionObjectMethods);
        Test("vision result consumers reject another run and compensation follows the main save snapshot", VisionMethodSnapshots);
        Test("object method language changes retain unfinished input and selected identities", ObjectMethodLanguage);
        Test("axis vendors and IO fields match real driver capabilities", HardwareCapabilityFields);
        Test("changing IO vendor preserves pending text and shows only relevant properties", HardwareVendorEditor);
        Test("IO group and resource labels follow the original control Text without changing IDs", IoDisplayNames);
        Test("axis menus exclude trays and movement uses only the page-bound point table", AxisBoundPointChoices);
        Test("resource property context menus edit the clicked instance as a draft", ResourceProperties);
        Test("Setting and Recipe constant tables are selectable objects with saved typed values", ConstantTableObjects);
        Test("constant read nodes protect referenced keys and types", ConstantReadReferences);
        Test("compact IO cards reuse the original instance and guard manual toggles", CompactIoCards);
        Test("direct condition rows preserve references literals operators and languages", DirectConditionEditing);
        Test("constant setters update actual table rows and preserve main-save boundaries", ConstantRuntimeWrites);
        Test("constant table writes use the UI thread and reject cancelled queued actions", ConstantTableWriteDispatch);
        Test("axis point picker includes every valid row with table context in three languages", AxisMultiplePointChoices);
        Test("P0 axis parameter editing exposes business points and preserves advanced drafts", P0AxisParameterEditing);
        Test("P0 language changes retain unavailable objects and methods without adopting defaults", P0MissingMethodLanguage);
        Test("P0 Empty conditions preserve hidden right bindings and typed candidate filtering", P0ConditionEmptyDrafts);
        Test("P0 typed rows retain boolean spellings missing options and structured sources", P0TypedParameterRows);
        Test("P0 unified binding filters searches and preserves source identity across languages", P0UnifiedSourcePicker);
        Test("P0 image sources bind the selected VPP capture node without numeric run IDs", P0VisionProducerBindings);
        Test("vision selection contains only configured VPPs without unused Camera1 fields", ConfiguredVisionCatalog);
        Test("DI DO and cylinder categories each list their own actual objects", IoCategorySelection);
        Test("tray node explains the current layout and calculates the selected pocket", TrayNodeHelp);
        RegisterP0EntryRepairs();
        RegisterActionOnlyCases();
        RegisterAutoMethodCases();
        RegisterHardwareDerivedTypeCases();
        RegisterFrameworkInheritanceCases();
        RegisterNativeEventCases();
        Test("first Boolean CASE defaults to the opposite result and missing objects explain repair", BeginnerNodeDefaults);
        Test("all inherited forms and runtime-created modules appear in the flow page", FlowPage);
        ExportP0FieldTexts();
        Console.WriteLine("RESULT: " + (count - failures) + "/" + count + " passed");
        return failures == 0 ? 0 : 1;
    }

    /// <summary>Copy only language and alarm XML test inputs.</summary>
    private static void CopyFolder(string source, string target)
    {
        Directory.CreateDirectory(target);
        foreach (string path in Directory.GetFiles(source, "*.xml")) File.Copy(path, Path.Combine(target, Path.GetFileName(path)), true);
    }

    /// <summary>Record each business scenario independently.</summary>
    private static void Test(string name, Action test)
    {
        count++;
        try { test(); Console.WriteLine("PASS " + name); }
        catch (Exception error) { failures++; Console.WriteLine("FAIL " + name + ": " + error); }
    }

    /// <summary>Fail with a meaningful invariant.</summary>
    private static void Assert(bool value, string message) { if (!value) throw new InvalidOperationException(message); }

    /// <summary>Set an application host field without changing its visibility.</summary>
    private static void SetHost(string name, object value) { host.GetField(name, BindingFlags.Static | BindingFlags.Public).SetValue(null, value); }

    /// <summary>Register an actual form with the framework resource discovery list.</summary>
    private static void RegisterForm(Form form) { ((IList)host.GetField("lstForm", BindingFlags.Static | BindingFlags.Public).GetValue(null)).Add(form); }

    /// <summary>Create a real subclass after factory installation.</summary>
    private static void AttachFuture()
    {
        FlowPrograms.Install();
        station = new FutureStation();
        station.Show(); Application.DoEvents(); station.Hide();
        editor = station.ProgramController as FlowModuleEditor;
        Assert(editor != null && editor.Program.Steps.Count == 1, "The subclass was not automatically attached.");
        Assert(editor.Program.Steps[0].ParentPath == "/canvas", "Nested canvas identity was lost.");
        data = new DataForm(); SetHost("DataF", data); RegisterForm(data);
        data.SetProductionDraft(new List<ProductionField> { new ProductionField { Name = "Product" }, new ProductionField { Name = "Measurement", Type = "Number" } });
        Directory.CreateDirectory(SysPara.SettingDataDirectory);
        data.WriteProductionDraft(Path.Combine(SysPara.SettingDataDirectory, "DataForm.Production.xml"));
        data.AcceptProductionDraft();
    }

    /// <summary>Verify original recipe values rather than pending editor values.</summary>
    private static void RecipeSnapshot()
    {
        recipe = new ProductManagerForm(); SetHost("ProductF", recipe);
        DataTable table = recipe.RecipeData.Tables["ProductSetting"];
        DataRow row = table.NewRow(); row["PRODUCTID"] = "PRODUCT-A"; table.Rows.Add(row);
        recipe.RecipeData.AcceptChanges(); RegisterForm(recipe);
        table.Rows[0][0] = "UNSAVED";
        Assert((string)FlowResources.Capture().Parameters["Recipe/ProductManagerForm/ProductSetting/0/PRODUCTID"] == "PRODUCT-A", "Pending data leaked into execution.");
        table.RejectChanges();
    }

    /// <summary>Verify the actual HardForm table-to-axis association.</summary>
    private static void PointBinding()
    {
        hardware = new HardForm(); SetHost("HardF", hardware); RegisterForm(hardware);
        DataTable table = hardware.RecipeData.Tables["tb_X1_Post"]; table.Rows.Clear(); table.Rows.Add(12.5, "Capture point"); hardware.RecipeData.AcceptChanges();
        FlowResource resource = FlowResources.Capture().Items.Values.Single(item => item.Kind == "point");
        Assert(((FlowPointAxis)resource.Instance).Positions[0] == 12.5, "The actual point was not captured.");
        Assert(resource.Points[0].Label.Contains("P1 [0]"), "Index display is ambiguous.");
        table.Rows[0]["X1"] = 999;
        Assert(((FlowPointAxis)FlowResources.Capture().Items.Values.Single(item => item.Kind == "point").Instance).Positions[0] == 12.5, "Uncommitted position leaked.");
        table.RejectChanges();
    }

    /// <summary>Discover an actual configured device object without connecting it.</summary>
    private static void DeviceInstance()
    {
        using (Form form = new Form { Name = "DeviceFixture" })
        using (DeviceControl control = new DeviceControl { Name = "scanner", SingleDevice = true })
        {
            form.Controls.Add(control); control.RefreshDeviceTypes();
            Assert(control.BindDevice("ScannerKeyenceTcp", "Scanner-A") == "", "Scanner binding failed.");
            AlphaRap.AbstractDevice instance = control.Device;
            Assert(DeviceControl.GetFlowDevices().Values.Contains(instance), "Registry did not reuse the configured instance.");
            Assert(!instance.IsConnected, "Browsing connected hardware.");
        }
    }

    /// <summary>Create a simple configured node in the inherited form.</summary>
    private static FlowStep Step(string action, params string[] pairs)
    {
        FlowStep step = editor.AddNode(); step.Action = action;
        step.Inputs.Clear();
        for (int i = 0; i < pairs.Length; i += 2) step.Inputs.Add(new FlowInput { Name = pairs[i], Reference = pairs[i + 1] });
        return step;
    }

    /// <summary>Build references using stable node identities.</summary>
    private static FlowInput Output(string name, FlowStep step, string field)
    {
        return new FlowInput { Name = name, Source = "Output", Reference = editor.Program.ModuleId + "/" + step.Id + "/" + Uri.EscapeDataString(field) };
    }

    /// <summary>Wait while pumping the UI thread so state updates can render.</summary>
    private static void Wait(Task task, int timeout = 4000)
    {
        DateTime limit = DateTime.UtcNow.AddMilliseconds(timeout);
        while (!task.IsCompleted && DateTime.UtcNow < limit) { Application.DoEvents(); Thread.Sleep(5); }
        Assert(task.IsCompleted, "Execution did not finish."); task.GetAwaiter().GetResult(); Application.DoEvents();
    }

    /// <summary>Exercise actual parameter capture, output resolution, branch selection and production save.</summary>
    private static void ProductionChain()
    {
        model = Step("value", "Value", ""); model.Caption = "Read model";
        model.Inputs[0] = new FlowInput { Name = "Value", Source = "Parameter", Reference = "Recipe/ProductManagerForm/ProductSetting/0/PRODUCTID" };
        measured = Step("value", "Value", "2.5"); measured.Caption = "Measurement";
        measured.Branches[0].Unconditional = false;
        measured.Branches[0].Conditions.Add(new FlowCondition { Left = new FlowInput { Source = "Result", Reference = "Value" }, Operator = "<=", Right = new FlowInput { Reference = "3" } });
        save = Step("data.save"); save.Caption = "Save production"; save.Target = "data/Production";
        save.Inputs.Add(Output("Product", model, "Value")); save.Inputs.Add(Output("Measurement", measured, "Value"));
        model.Next = measured.Id; measured.Next = save.Id; editor.Program.AutoEntry = model.Id; editor.Program.Mode = "Program";
        FlowPrograms.BeginBatch(); editor.Start(model.Id, true, false); Wait(editor.Runtime.Completion);
        Assert(editor.Runtime.Error == null, editor.Runtime.Error);
        int files, total; List<ProductionRecord> records = ProductionDataStore.Query(DateTime.Today, DateTime.Today.AddDays(1), "", out files, out total);
        Assert(records.Count == 1 && records[0].Get("Product") == "PRODUCT-A" && records[0].Get("Measurement") == "2.5", "Named data was not saved.");
        Assert(records[0].Get("Simulated") == "True", "Simulation records were not identified.");
    }

    /// <summary>Retrying the same business record remains idempotent.</summary>
    private static void DuplicateRecord()
    {
        string id = Convert.ToString(FlowPrograms.Results.Read(FlowPrograms.Results.Epoch, "batch", editor.Program.ModuleId + "/" + save.Id, "RecordId"));
        data.SaveProductionRecord(id, "batch", "Model-A", new Dictionary<string, object> { { "Product", "PRODUCT-A" }, { "Measurement", 2.5 } }, true);
        int files, total; Assert(ProductionDataStore.Query(DateTime.Today, DateTime.Today.AddDays(1), "", out files, out total).Count == 1, "Duplicate record appended.");
    }

    /// <summary>Test the storage limitation that previously discarded new fields on the same day.</summary>
    private static void SchemaEvolution()
    {
        Dictionary<string, string> fields = new Dictionary<string, string> { { "RecordId", "new-schema" }, { "NewField", "KEPT" } };
        Assert(ProductionDataStore.AppendNamed(DateTime.Now, fields, "new-schema") == "", "Schema write failed.");
        int files, total; var records = ProductionDataStore.Query(DateTime.Today, DateTime.Today.AddDays(1), "", out files, out total);
        Assert(records.Count == 2 && records.Any(row => row.Get("Product") == "PRODUCT-A") && records.Any(row => row.Get("NewField") == "KEPT"), "Schema migration dropped data.");
    }

    /// <summary>Simulated moves still validate real point indices and compensation limits.</summary>
    private static void PointSimulation()
    {
        FlowActions actions = new FlowActions(FlowResources.Capture()) { Simulate = true };
        FlowStep move = new FlowStep { Action = "point.move", Target = actions.Resources.Items.Values.Single(item => item.Kind == "point").Id };
        move.Inputs.AddRange(new[] { new FlowInput { Name = "Index", Reference = "0" }, new FlowInput { Name = "Offset", Reference = "0.5" }, new FlowInput { Name = "Tolerance", Reference = "0.2" } });
        var context = Context(new FlowResultHub()); var result = actions.Execute(move, context, CancellationToken.None).GetAwaiter().GetResult();
        Assert((double)result["Position"] == 13.0, "Offset was not explicitly applied.");
        move.Inputs[0].Reference = "9";
        bool failed = false; try { actions.Execute(move, context, CancellationToken.None).GetAwaiter().GetResult(); } catch (InvalidOperationException) { failed = true; }
        Assert(failed, "Missing point was silently accepted.");
    }

    /// <summary>Create an isolated runtime context.</summary>
    private static FlowExecutionContext Context(FlowResultHub hub) { return new FlowExecutionContext { Hub = hub, RunId = hub.Epoch, CycleId = "product", ModuleId = "test" }; }

    /// <summary>Create an independent result condition for one native exit.</summary>
    private static FlowBranch EqualBranch(string code, string value)
    {
        return new FlowBranch { Code = code, Conditions = new List<FlowCondition> { new FlowCondition {
            Left = new FlowInput { Source = "Result", Reference = "Value" }, Right = new FlowInput { Reference = value } } } };
    }

    /// <summary>Every exit must use its own condition, target and stable SDK code.</summary>
    private static void NativeBranches()
    {
        string[] codes = { "NEXT", "CASE1", "CASE2", "CASE3" };
        FlowStep decision = new FlowStep { Caption = "Native decision", Branches = codes.Select((code, i) => EqualBranch(code, i.ToString())).ToList() };
        List<FlowStep> destinations = codes.Select(code => new FlowStep { Caption = code }).ToList();
        for (int i = 0; i < codes.Length; i++) decision.SetTarget(codes[i], destinations[i].Id);
        FlowProgram plan = new FlowProgram { Steps = new[] { decision }.Concat(destinations).ToList() };
        Assert(FlowProgramFile.Reachable(plan, decision.Id).Count == 5, "CASE3 is missing from reachable validation.");
        for (int i = 0; i < codes.Length; i++)
        {
            int value = i, starts = 0; string hit = null;
            FlowProgramRuntime runtime = new FlowProgramRuntime { ExecuteAction = (step, context, token) => {
                if (step.Id == decision.Id) starts++; else hit = step.Caption;
                return Task.FromResult((IDictionary<string, object>)new Dictionary<string, object> { { "Value", value } }); } };
            runtime.Start(plan, decision.Id, Context(new FlowResultHub())); Wait(runtime.Completion);
            Assert(runtime.Error == null && hit == codes[i] && starts == 1, "Wrong native exit for result " + i + ": " + hit);
        }
        decision.Branches[0].Conditions[0].Operator = ">=";
        string priority = null;
        FlowProgramRuntime ordered = new FlowProgramRuntime { ExecuteAction = (step, context, token) => {
            if (step.Id != decision.Id) priority = step.Caption;
            return Task.FromResult((IDictionary<string, object>)new Dictionary<string, object> { { "Value", 1 } }); } };
        ordered.Start(plan, decision.Id, Context(new FlowResultHub())); Wait(ordered.Completion);
        Assert(priority == "NEXT", "Multiple true exits did not follow NEXT, CASE1, CASE2, CASE3 order.");
    }

    /// <summary>Alarm at the deadline even when a legacy SDK call cannot immediately cancel.</summary>
    private static void BlockingTimeout()
    {
        object resource = new object(); ManualResetEventSlim release = new ManualResetEventSlim(), alarm = new ManualResetEventSlim();
        int alarms = 0; FlowStep step = new FlowStep { Caption = "Blocked SDK", TimeoutMs = 40, AlarmCode = "2019" };
        FlowProgramRuntime runtime = new FlowProgramRuntime { ExecuteAction = async (node, context, token) => {
            using (var lease = await FlowResourceGate.Acquire(new[] { resource }, token))
                return await lease.Run(async () => { await Task.Run(() => release.Wait()); return (IDictionary<string, object>)new Dictionary<string, object>(); }); } };
        runtime.AlarmRequested += code => { Interlocked.Increment(ref alarms); alarm.Set(); };
        runtime.Start(new FlowProgram { Steps = new List<FlowStep> { step } }, step.Id, Context(new FlowResultHub()));
        bool raised = alarm.Wait(1000); bool retained = runtime.IsRunning && !FlowResourceGate.CanAccess(resource);
        release.Set(); Wait(runtime.Completion);
        Assert(raised && retained && alarms == 1 && runtime.Error.Contains("Timeout") && FlowResourceGate.CanAccess(resource), "Timeout waited for the SDK or released an active resource.");
    }

    /// <summary>Changing NEXT to OR must not alter CASE1's own condition group.</summary>
    private static void BranchGroups()
    {
        FlowStep first = new FlowStep { Branches = new List<FlowBranch> { EqualBranch("NEXT", "1"), EqualBranch("CASE1", "1") } };
        first.Branches[0].Conditions.Add(EqualBranch("NEXT", "2").Conditions[0]);
        FlowStep next = new FlowStep { Caption = "NEXT" }, other = new FlowStep { Caption = "CASE1" }; first.Next = next.Id; first.Case1 = other.Id;
        foreach (bool any in new[] { false, true })
        {
            first.Branches[0].AnyCondition = any; string hit = null;
            FlowProgramRuntime runtime = new FlowProgramRuntime { ExecuteAction = (step, context, token) => {
                if (step.Id != first.Id) hit = step.Caption;
                return Task.FromResult((IDictionary<string, object>)new Dictionary<string, object> { { "Value", 1 } }); } };
            runtime.Start(new FlowProgram { Steps = new List<FlowStep> { first, next, other } }, first.Id, Context(new FlowResultHub())); Wait(runtime.Completion);
            Assert(hit == (any ? "NEXT" : "CASE1"), "AND / OR leaked between exits.");
        }
    }

    /// <summary>A true CASE2 condition never turns CASE2 into an exception handler.</summary>
    private static void ExceptionIsNotCase()
    {
        FlowStep first = new FlowStep { AlarmCode = "2019", Branches = new List<FlowBranch> { EqualBranch("NEXT", "0"), EqualBranch("CASE2", "1") } }, other = new FlowStep();
        first.Case2 = other.Id; bool followed = false; int alarms = 0;
        FlowProgramRuntime runtime = new FlowProgramRuntime { ExecuteAction = (step, context, token) => {
            if (step.Id == first.Id) throw new IOException("Device failed"); followed = true;
            return Task.FromResult((IDictionary<string, object>)new Dictionary<string, object>()); } };
        runtime.AlarmRequested += code => alarms++;
        runtime.Start(new FlowProgram { Steps = new List<FlowStep> { first, other } }, first.Id, Context(new FlowResultHub())); Wait(runtime.Completion);
        Assert(!followed && alarms == 1 && runtime.Error.Contains("Device failed"), "Exception entered CASE2 or did not alarm.");
    }

    /// <summary>Old XML is readable but old failure routing cannot silently become a native CASE.</summary>
    private static void LegacyExits()
    {
        string path = Path.Combine(root, "legacy-exits.xml");
        File.WriteAllText(path, "<FlowProject><Version>1</Version><Modules><FlowProgram><ModuleId>test</ModuleId><Steps><FlowStep><Action>value</Action><Otherwise>one</Otherwise><Failure>two</Failure><Third>three</Third></FlowStep></Steps></FlowProgram></Modules></FlowProject>");
        FlowProject project = FlowProgramFile.Load(path); FlowStep step = project.Modules[0].Steps[0];
        Assert(project.Version == 1 && step.Case1 == "one" && step.Case2 == "two" && step.Case3 == "three", "Legacy version or connections were lost before a successful save.");
        Assert(FlowProgramFile.RequiresBranchReview(step), "Old exception routing was silently accepted.");
        step.Branches = new[] { "NEXT", "CASE1", "CASE2", "CASE3" }.Select((code, i) => EqualBranch(code, i.ToString())).ToList();
        FlowProgramFile.Save(path, project);
        string xml = File.ReadAllText(path);
        Assert(project.Version == 3 && !xml.Contains("<Failure>") && xml.Contains("<Case3>three</Case3>") && FlowProgramFile.Load(path).Modules[0].Steps[0].Branches.Count == 4, "Native branch persistence duplicated or renamed exits.");
    }

    /// <summary>Load model files through the existing ProductManagerForm recipe entry point.</summary>
    private static void ProductManagerRecipe()
    {
        foreach (ModuleBaseForm module in ModuleManager.ModuleList) module.ModuleInitialize(module.Text);
        string first = Path.GetFullPath(Path.Combine(SysPara.RecipeDataDirectory, "Model-A.xml")), second = Path.GetFullPath(Path.Combine(SysPara.RecipeDataDirectory, "Model-B.xml"));
        recipe.WriteRecipeData(first);
        recipe.RecipeData.Tables["ProductSetting"].Rows[0]["PRODUCTID"] = "PRODUCT-B";
        hardware.RecipeData.Tables["tb_X1_Post"].Rows[0][0] = 25.0;
        foreach (ModuleBaseForm module in ModuleManager.ModuleList) module.RecipeData.AcceptChanges();
        recipe.WriteRecipeData(second);
        Assert((bool)host.GetMethod("OpenRecipe").Invoke(null, new object[] { second }), "Product manager recipe load failed.");
        FlowResources snapshot = FlowResources.Capture();
        Assert(snapshot.RecipeName == "Model-B" && (string)snapshot.Parameters["Recipe/ProductManagerForm/ProductSetting/0/PRODUCTID"] == "PRODUCT-B", "Flow did not use the product manager model.");
        Assert(((FlowPointAxis)snapshot.Items.Values.Single(item => item.Kind == "point").Instance).Positions[0] == 25.0, "Points did not follow the recipe.");
        Assert((bool)host.GetMethod("OpenRecipe").Invoke(null, new object[] { first }), "Cannot restore model A.");
        Assert(snapshot.RecipeName == "Model-B" && (string)snapshot.Parameters["Recipe/ProductManagerForm/ProductSetting/0/PRODUCTID"] == "PRODUCT-B", "A captured run changed when the active recipe changed.");
    }

    /// <summary>Release a completed product's values so repeated production remains bounded.</summary>
    private static void CompleteCycle()
    {
        FlowResultHub hub = new FlowResultHub(); FlowExecutionContext context = Context(hub);
        context.BeginCycle("P-1"); hub.Publish(context.RunId, context.CycleId, "module/node", new Dictionary<string, object> { { "Value", 42 } });
        context.CompleteCycle(); bool missing = false;
        try { hub.Read(context.RunId, context.CycleId, "module/node", "Value"); } catch (FlowOutputUnavailableException) { missing = true; }
        Assert(missing && !context.RetainResult, "Completed product outputs remain visible.");
        context.BeginCycle("P-1"); Assert(context.CycleId == "P-1", "Completed association cannot be reused.");
    }

    /// <summary>Confirm timeout transitions and one-shot command execution.</summary>
    private static void Timeout()
    {
        FlowStep first = new FlowStep { Caption = "Wait", TimeoutMs = 40 }, failure = new FlowStep { Caption = "Failure" };
        first.Branches = new List<FlowBranch> { EqualBranch("NEXT", "0"), EqualBranch("CASE2", "2") };
        first.Case2 = failure.Id; first.AlarmCode = "2019"; int starts = 0, alarms = 0; bool handled = false;
        FlowProgramRuntime runtime = new FlowProgramRuntime();
        runtime.AlarmRequested += code => { Assert(code == "2019", "Wrong alarm code."); Interlocked.Increment(ref alarms); };
        runtime.ExecuteAction = async (step, context, token) => { if (step.Id == first.Id) { starts++; await Task.Delay(500, token); } else handled = true; return new Dictionary<string, object>(); };
        runtime.Start(new FlowProgram { Steps = new List<FlowStep> { first, failure } }, first.Id, Context(new FlowResultHub())); Wait(runtime.Completion);
        Assert(starts == 1 && !handled && alarms == 1 && runtime.Error.Contains("Timeout"), "Timeout entered a CASE, repeated the action or duplicated the alarm.");
    }

    /// <summary>Verify pending cancellation cannot free a resource still used by a blocking action.</summary>
    private static void StopLease()
    {
        object resource = new object(); ManualResetEventSlim started = new ManualResetEventSlim(), release = new ManualResetEventSlim();
        FlowProgramRuntime runtime = new FlowProgramRuntime(); FlowStep step = new FlowStep { Caption = "Blocking" };
        runtime.ExecuteAction = async (node, context, token) =>
        {
            using (var lease = await FlowResourceGate.Acquire(new[] { resource }, token))
                return await lease.Run(async () => { started.Set(); await Task.Run(() => release.Wait()); token.ThrowIfCancellationRequested(); return (IDictionary<string, object>)new Dictionary<string, object>(); });
        };
        runtime.Start(new FlowProgram { Steps = new List<FlowStep> { step } }, step.Id, Context(new FlowResultHub()));
        Assert(started.Wait(1000), "Action did not start."); runtime.Stop();
        Assert(runtime.IsRunning && !FlowResourceGate.CanAccess(resource), "Resource released before the underlying call completed.");
        release.Set(); Wait(runtime.Completion);
        Assert(runtime.Error == "Cancelled" && FlowResourceGate.CanAccess(resource), "Stopped resource was not released.");
    }

    /// <summary>Ensure previous cycles and execution epochs cannot supply current data.</summary>
    private static void ResultIsolation()
    {
        FlowResultHub hub = new FlowResultHub(); string run = hub.Epoch;
        hub.Publish(run, "A", "module/node", new Dictionary<string, object> { { "Value", 42 } });
        bool rejected = false; try { hub.Read(run, "B", "module/node", "Value"); } catch (InvalidOperationException) { rejected = true; }
        Assert(rejected, "Previous product data was visible."); hub.Reset();
        rejected = false; try { hub.Read(run, "A", "module/node", "Value"); } catch (InvalidOperationException) { rejected = true; }
        Assert(rejected, "Expired run data was visible.");
    }

    /// <summary>Wait for a related flow output without invoking the current action again.</summary>
    private static void WaitForOutput()
    {
        FlowResultHub hub = new FlowResultHub(); FlowExecutionContext context = Context(hub); int starts = 0;
        FlowStep step = new FlowStep { Caption = "Join", TimeoutMs = 1000, Branches = new List<FlowBranch> { new FlowBranch() } };
        step.Branches[0].Conditions.Add(new FlowCondition { Left = new FlowInput { Source = "Output", Reference = "other/node/Value" }, Right = new FlowInput { Reference = "1" } });
        FlowProgramRuntime runtime = new FlowProgramRuntime { ExecuteAction = (node, ctx, token) => { starts++; return Task.FromResult((IDictionary<string, object>)new Dictionary<string, object>()); } };
        runtime.Start(new FlowProgram { Steps = new List<FlowStep> { step } }, step.Id, context);
        Thread.Sleep(80); hub.Publish(context.RunId, context.CycleId, "other/node", new Dictionary<string, object> { { "Value", 1 } });
        Wait(runtime.Completion); Assert(runtime.Error == null && starts == 1, "Joining outputs repeated the action or did not wait.");
    }

    /// <summary>Repeated product keys require an explicit new association rather than stale output reuse.</summary>
    private static void CycleReuse()
    {
        FlowExecutionContext context = Context(new FlowResultHub()); context.BeginCycle("P-1");
        bool rejected = false; try { context.BeginCycle("P-1"); } catch (InvalidOperationException) { rejected = true; }
        Assert(rejected, "Repeated key accepted old product data.");
    }

    /// <summary>Only saved definitions change production dispatch.</summary>
    private static void PublishedDraft()
    {
        Assert(editor.IsEnabled && editor.IsProgram, "The unified controller did not own production dispatch.");
        FlowPrograms.Save(); Assert(editor.IsEnabled, "Saved program did not publish.");
        editor.Program.Mode = "Disabled"; Assert(editor.IsProgram, "Unsaved draft disabled the published flow."); editor.Program.Mode = "Program";
    }

    /// <summary>Exercise the base-form controller entry used by the machine lifecycle.</summary>
    private static void Lifecycle()
    {
        SysPara.Simulation = true;
        Assert(station.TryProgramCommand(FlowProgramCommand.Initialize), "Generic initialization was not intercepted.");
        Wait(editor.Runtime.Completion); Assert(station.InitialStatus, "Configured initialization did not complete.");
        FlowPrograms.BeginBatch();
        Assert(station.TryProgramCommand(FlowProgramCommand.Start), "Generic production start was not intercepted.");
        Wait(editor.Runtime.Completion);
        Assert(editor.Runtime.Error == null && station.LegacyStarts == 0, "An existing callback was mixed into the designer runtime.");
    }

    /// <summary>Use the real main-form timer to test the configured runtime's production readiness.</summary>
    private static void MainStartReady()
    {
        RunMode previous = SysPara.SystemMode; bool oldReady = SysPara.UpConveyorInitialOk;
        SysPara.SystemMode = RunMode.INITIAL; SysPara.UpConveyorInitialOk = false;
        try
        {
            using (MainForm main = new MainForm())
            {
                typeof(MainForm).GetMethod("timer1_Tick", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(main, new object[] { null, EventArgs.Empty });
                Assert(main.Controls.Find("MENU_Run", true)[0].Enabled, "Start stays disabled after all configured modules finish initialization.");
                station.InitialStatus = false;
                typeof(MainForm).GetMethod("timer1_Tick", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(main, new object[] { null, EventArgs.Empty });
                Assert(!main.Controls.Find("MENU_Run", true)[0].Enabled, "Start enabled before configured initialization completed."); station.InitialStatus = true;
            }
        }
        finally { SysPara.SystemMode = previous; SysPara.UpConveyorInitialOk = oldReady; }
    }

    /// <summary>Restore layout, identifiers, connection topology and removals from disk.</summary>
    private static void Persistence()
    {
        editor.CaptureLayout(); FlowPrograms.Save();
        string savedId = model.Id;
        editor.Delete(editor.Program.Steps.Single(step => step.ControlName == "existing").Id);
        FlowPrograms.Reload(); Assert(editor.Program.Steps.Any(step => step.ControlName == "existing"), "Original deleted node did not restore.");
        editor.Delete(editor.Program.Steps.Single(step => step.ControlName == "existing").Id); FlowPrograms.Save(); FlowPrograms.Reload();
        Assert(editor.Program.Steps.All(step => step.ControlName != "existing"), "Deleted original node reappeared.");
        Assert(editor.Program.Steps.Single(step => step.Id == savedId).Next == measured.Id, "Stable identity or link changed.");
        FlowProgram restored = FlowProgramFile.Load(FlowPrograms.FilePath).Modules.Single(module => module.ModuleId == editor.Program.ModuleId);
        using (FutureStation other = new FutureStation())
        {
            FlowModuleEditor session = new FlowModuleEditor(other, FlowProgramFile.Snapshot(restored));
            Assert(session.Program.Steps.All(step => step.ControlName != "existing") && !other.Canvas.Controls.Contains(other.Existing), "Restart did not honor deletion.");
        }
    }

    /// <summary>Reject damaged XML without discarding the active model.</summary>
    private static void CorruptLoad()
    {
        byte[] saved = File.ReadAllBytes(FlowPrograms.FilePath); int before = editor.Program.Steps.Count;
        File.WriteAllText(FlowPrograms.FilePath, "<broken>"); bool rejected = false;
        try { FlowPrograms.Reload(); } catch (InvalidOperationException) { rejected = true; }
        finally { File.WriteAllBytes(FlowPrograms.FilePath, saved); }
        Assert(rejected && editor.Program.Steps.Count == before, "Corrupt load changed the active design.");
    }

    /// <summary>Render the actual node editor in all languages and preserve an unfinished edit.</summary>
    private static void EditorLanguages()
    {
        foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
        {
            SwitchLanguage(language);
            using (FlowNodeForm dialog = new FlowNodeForm(editor, editor.Program.Steps.Single(step => step.Id == model.Id)))
            {
                dialog.Show(); Application.DoEvents();
                TextBox input = (TextBox)dialog.Controls.Find("captionValue", true)[0]; input.Text = "Pending product name";
                SwitchLanguage(language == LanguageType.Chinese ? LanguageType.English : LanguageType.Chinese);
                Assert(input.Text == "Pending product name", "Language change overwrote the draft.");
                SwitchLanguage(language); Application.DoEvents();
                using (Bitmap bitmap = new Bitmap(dialog.Width, dialog.Height)) { dialog.DrawToBitmap(bitmap, new Rectangle(Point.Empty, dialog.Size)); bitmap.Save(Path.Combine(root, "node-" + language + ".png")); }
                dialog.Close();
            }
        }
    }

    /// <summary>Switch through the application's actual language entry point.</summary>
    private static void SwitchLanguage(LanguageType language) { host.GetMethod("SwitchLanguage", BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { language }); }

    /// <summary>Exercise the actual editor binding events rather than constructing only its initial state.</summary>
    private static void NodeSelections()
    {
        using (FlowNodeForm dialog = new FlowNodeForm(editor, editor.Program.Steps.Single(step => step.Id == model.Id)))
        {
            dialog.Show(); Application.DoEvents();
            Control label = dialog.Controls.Find("targetLabel", true)[0], value = dialog.Controls.Find("targetValue", true)[0];
            Assert(Math.Abs((label.Top + label.Height / 2) - (value.Top + value.Height / 2)) <= 4, "Object label and editor are vertically misaligned.");
            ComboBox category = (ComboBox)dialog.Controls.Find("categoryValue", true)[0];
            ComboBox target = (ComboBox)dialog.Controls.Find("targetValue", true)[0];
            ComboBox action = (ComboBox)dialog.Controls.Find("actionValue", true)[0];
            FlowLayoutPanel inputs = (FlowLayoutPanel)dialog.Controls.Find("inputs", true)[0];
            category.SelectedValue = "motion"; Application.DoEvents();
            string axis = FlowResources.Capture().Items.Values.Single(item => item.Kind == "axis").Id;
            target.SelectedValue = axis; Application.DoEvents();
            action.SelectedItem = ((IList<FlowMethod>)action.DataSource).Single(item => item.Id == "point.move"); Application.DoEvents();
            Assert(inputs.Controls.OfType<FlowInputRow>().Select(row => row.FieldName).SequenceEqual(new[] { "Index", "Offset", "Tolerance" }), "Axis-bound point inputs did not refresh.");
            category.SelectedValue = "data"; Application.DoEvents();
            Assert((action.SelectedItem as FlowMethod).Id == "data.save" && inputs.Controls.OfType<FlowInputRow>().Count() == data.GetProductionSchema().Count, "DataForm fields did not refresh.");
            category.SelectedValue = "flow"; action.SelectedItem = ((IList<FlowMethod>)action.DataSource).Single(item => item.Id == "delay"); Application.DoEvents();
            Assert(inputs.Controls.OfType<FlowInputRow>().Count() == 1 && inputs.Controls.OfType<FlowInputRow>().Single().FieldName == "Milliseconds", "Changing actions retained another action's fields.");
            dialog.Close();
        }
    }

    /// <summary>Use the actual Add CASE buttons, per-exit grids and OK handler.</summary>
    private static void NativeBranchEditor()
    {
        FlowStep node = Step("value", "Value", "1");
        using (FlowNodeForm dialog = new FlowNodeForm(editor, node))
        {
            dialog.Show(); Application.DoEvents();
            TabControl tabs = (TabControl)dialog.Controls.Find("executionTabs", true)[0];
            tabs.SelectedTab = tabs.TabPages["scriptRoutes"]; Application.DoEvents();
            FlowLayoutPanel panel = (FlowLayoutPanel)dialog.Controls.Find("branchPanel", true)[0];
            Assert(panel.Controls.OfType<FlowBranchEditor>().Select(branch => branch.Code).SequenceEqual(new[] { "NEXT" }), "A new node pre-created CASE exits.");
            Button add = (Button)dialog.Controls.Find("addCase", true)[0];
            for (int i = 0; i < 3; i++) add.PerformClick();
            Assert(!add.Enabled && panel.Controls.Count == 4, "CASE addition did not stop at CASE3.");
            int index = 0;
            foreach (FlowBranchEditor branch in panel.Controls.OfType<FlowBranchEditor>())
            {
                ((ComboBox)branch.ConditionRows.First().Controls.Find("conditionRight",true)[0]).Text = (index++).ToString();
                ((ComboBox)branch.Controls.Find("branchTargetValue", true)[0]).SelectedValue = measured.Id;
            }
            ((Button)dialog.Controls.Find("ok", true)[0]).PerformClick();
            Assert(dialog.DialogResult == DialogResult.OK, "Four independent conditions could not be saved.");
        }
        FlowStep saved = editor.Program.Steps.Single(step => step.Id == node.Id);
        Assert(saved.Branches.Count == 4 && saved.Case3 == measured.Id && saved.Branches.Single(branch => branch.Code == "CASE3").Conditions[0].Right.Reference == "3", "CASE3 condition or target was not persisted.");
    }

    /// <summary>Use the real CASE command and missing-object editor without executing hardware or changing original definitions.</summary>
    private static void BeginnerNodeDefaults()
    {
        FlowStep boolean = BoundStep(FlowResources.Capture(true).Items["hardware/" + groupedInput.Id + "/input"], "io.read");
        boolean.Caption = "Boolean routing";
        boolean.Branches = FlowBranch.Default();
        using (FlowNodeForm dialog = new FlowNodeForm(editor, boolean))
        {
            dialog.Show(); TabControl tabs = (TabControl)dialog.Controls.Find("executionTabs", true)[0];
            tabs.SelectedTab = tabs.TabPages["scriptRoutes"]; Application.DoEvents();
            ((Button)dialog.Controls.Find("addCase", true)[0]).PerformClick();
            FlowBranchEditor[] branches = dialog.Controls.Find("branchPanel", true)[0].Controls.OfType<FlowBranchEditor>().ToArray();
            Assert(branches.Length == 2 && branches[0].ReadBranch().Conditions.Single().Right.Reference == "true"
                && branches[1].ReadBranch().Conditions.Single().Right.Reference == "false", "NEXT and first CASE both accepted the same Boolean value.");
        }
        Assert(boolean.Branches.Count == 1, "Canceling CASE editing modified the original action node.");
        boolean.Branches[0].Unconditional = false;
        boolean.Branches[0].Conditions.Add(new FlowCondition { Left = new FlowInput { Source = "Result", Reference = "Value" }, Right = new FlowInput { Reference = "1" } });
        using (FlowNodeForm dialog = new FlowNodeForm(editor, boolean))
        {
            dialog.Show(); TabControl tabs = (TabControl)dialog.Controls.Find("executionTabs", true)[0]; tabs.SelectedTab = tabs.TabPages["scriptRoutes"]; Application.DoEvents();
            ((Button)dialog.Controls.Find("addCase", true)[0]).PerformClick();
            FlowBranchEditor added = dialog.Controls.Find("branchPanel", true)[0].Controls.OfType<FlowBranchEditor>().Single(branch => branch.Code == "CASE1");
            Assert(added.ReadBranch().Conditions.Single().Right.Reference == "false", "A legacy Boolean 1 condition produced a duplicate true CASE.");
        }
        Assert(boolean.Branches[0].Conditions.Single().Right.Reference == "1", "Editing CASE rewrote the original Boolean spelling.");
        FlowStep missing = new FlowStep { Caption = "Missing axis", Target = "removed/axis", Action = "point.move", Branches = FlowBranch.Default() };
        using (FlowNodeForm dialog = new FlowNodeForm(editor, missing))
        {
            dialog.Show(); Application.DoEvents();
            ComboBox methods = (ComboBox)dialog.Controls.Find("actionValue", true)[0];
            Label help = (Label)dialog.Controls.Find("methodHint_dyn", true)[0];
            foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
            {
                SwitchLanguage(language); Application.DoEvents();
                Assert(methods.Items.Count == 0 && help.Text.Length > 30 && help.Visible, "A missing object exposed unrelated methods or hid its repair instructions.");
            }
        }
        SwitchLanguage(LanguageType.Chinese);
    }

    /// <summary>Use a base form with both SDK pages and a small viewport, as in the actual host.</summary>
    private static void AddVisibleNode()
    {
        using (Flow_BaseForm form = FlowPrograms.AddModule("ViewportFixture"))
        {
            form.Show(); Application.DoEvents();
            FlowModuleEditor module = (FlowModuleEditor)form.ProgramController;
            form.FlowChartTable.SelectedTab = form.InitialPage; module.AddNode();
            form.FlowChartTable.SelectedTab = form.AutoPage; Application.DoEvents();
            for (int i = 0; i < 8; i++) module.AddNode();
            FlowStep last = module.Program.Steps.Last(); Flow_Chart node = form.Controls.Find(last.ControlName, true).OfType<Flow_Chart>().Single();
            Assert(node.Parent == form.AutoPage, "Added to an inactive page.");
            Assert(node.Parent.ClientRectangle.Contains(node.Bounds), "Added node is outside the visible viewport.");
            module.CaptureLayout(); int y = last.Y;
            ((ScrollableControl)node.Parent).AutoScrollPosition = Point.Empty; module.CaptureLayout();
            Assert(last.Y == y, "Scrolling changes the saved node coordinate.");
            form.Close();
        }
    }

    /// <summary>Raise real WinForms mouse events without connecting hardware or relying on screenshots.</summary>
    private static void MouseEditing()
    {
        using (Form form = new Form())
        {
            Flow_Chart node = new Flow_Chart { Location = new Point(20, 20) }; form.Controls.Add(node); form.Show(); Application.DoEvents();
            node.EnableProgramEditor(true); Control button = node.Controls.Find("button1", true)[0]; int opened = 0;
            node.ConfigurationRequested += (sender, args) => opened++;
            RaiseMouse(button, "OnMouseDown", new MouseEventArgs(MouseButtons.Left, 1, 8, 8, 0)); Assert(node.Capture, "Drag does not capture the pointer from the inner button.");
            RaiseMouse(node, "OnMouseMove", new MouseEventArgs(MouseButtons.Left, 0, 90, 60, 0));
            RaiseMouse(node, "OnMouseUp", new MouseEventArgs(MouseButtons.Left, 1, 20, 20, 0));
            Assert(node.Left > 20 && !node.Capture && opened == 0, "Dragging did not move the node or incorrectly opened configuration.");
            for (int i = 0; i < 2; i++) { RaiseMouse(button, "OnMouseDown", new MouseEventArgs(MouseButtons.Left, 1, 8, 8, 0)); RaiseMouse(node, "OnMouseUp", new MouseEventArgs(MouseButtons.Left, 1, 8, 8, 0)); }
            Assert(opened == 1, "A double-click did not open exactly one editor."); form.Close();
        }
    }

    /// <summary>Dispatch one event through the control's native event handlers.</summary>
    private static void RaiseMouse(Control control, string method, MouseEventArgs args)
    {
        typeof(Control).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(control, new object[] { args });
    }

    /// <summary>Persist the selected AND or OR mode and preserve an active condition edit while switching through the framework's languages.</summary>
    private static void BranchConditionModes()
    {
        SwitchLanguage(LanguageType.Chinese);
        using (FlowNodeForm dialog = new FlowNodeForm(editor, editor.Program.Steps.Single(step => step.Id == model.Id)))
        {
            dialog.Show(); Application.DoEvents();
            TabControl tabs = (TabControl)dialog.Controls.Find("executionTabs", true)[0]; tabs.SelectedTab = tabs.TabPages["scriptRoutes"]; Application.DoEvents();
            FlowBranchEditor branch = (FlowBranchEditor)dialog.Controls.Find("branch_NEXT_dyn", true)[0];
            RadioButton all = (RadioButton)branch.Controls.Find("branchAll", true)[0];
            RadioButton any = (RadioButton)branch.Controls.Find("branchAny", true)[0];
            RadioButton unconditional = (RadioButton)branch.Controls.Find("branchUnconditional", true)[0];
            Assert(unconditional.Checked && !all.Checked && !any.Checked, "Default mode is ambiguous.");
            all.Checked = true;
            Assert(!unconditional.Checked && !any.Checked && branch.ConditionRows.Count()==1, "AND is not exclusive or does not create a direct-edit condition.");
            ComboBox right = (ComboBox)branch.ConditionRows.First().Controls.Find("conditionRight",true)[0];
            right.Focus(); right.Text = "pending threshold";
            SwitchLanguage(LanguageType.English); Application.DoEvents();
            Assert(all.Text == "All conditions (AND)" && all.Checked && right.Text == "pending threshold", "English switch changed the mode or active edit.");
            SwitchLanguage(LanguageType.Español); Application.DoEvents();
            Assert(all.Text == "Todas las condiciones (AND)" && all.Checked, "Spanish mode label or state is wrong.");
            FlowBranch saved = branch.ReadBranch();
            FlowProgram copy = FlowProgramFile.Snapshot(new FlowProgram { Steps = new List<FlowStep> { new FlowStep { Branches = new List<FlowBranch> { saved } } } });
            branch.Bind(copy.Steps[0].Branches[0], "", new List<FlowChoice> { new FlowChoice { Id = "", Label = "End" } });
            Assert(all.Checked && !branch.ReadBranch().AnyCondition && branch.ReadBranch().Conditions.Count == 1 && branch.ReadBranch().Conditions[0].Right.Reference == "pending threshold", "AND did not reload with its edited value.");
            any.Checked = true;
            Assert(!all.Checked && !unconditional.Checked && branch.ReadBranch().AnyCondition, "OR is not exclusive or was not persisted.");
            unconditional.Checked = true;
            Assert(!all.Checked && !any.Checked && !branch.Controls.Find("branchConditions",true)[0].Visible, "Unconditional does not collapse the condition area.");
            dialog.Close();
        }
        SwitchLanguage(LanguageType.Chinese);
    }

    /// <summary>Reject deleting a source used in inputs or independent CASE conditions before touching links or controls.</summary>
    private static void NodeDeletion()
    {
        using (Flow_BaseForm sourceForm = FlowPrograms.AddModule("Delete source"))
        using (Flow_BaseForm consumerForm = FlowPrograms.AddModule("Delete consumer"))
        {
            FlowModuleEditor source = FlowPrograms.Attach(sourceForm), consumer = FlowPrograms.Attach(consumerForm);
            FlowStep producer = source.AddNode(), link = source.AddNode(), reader = consumer.AddNode();
            string reference = source.Program.ModuleId + "/" + producer.Id + "/Value";
            reader.Inputs[0].Source = "Output"; reader.Inputs[0].Reference = reference;
            bool rejected = false;
            try { source.Delete(producer.Id); } catch (InvalidOperationException) { rejected = true; }
            Assert(rejected && source.Program.Steps.Contains(producer), "Referenced input source was deleted.");
            reader.Inputs[0].Source = "Constant";
            reader.Branches.Add(new FlowBranch { Code = "CASE3", Conditions = new List<FlowCondition> { new FlowCondition { Left = new FlowInput { Source = "Output", Reference = reference }, Operator = "==", Right = new FlowInput { Reference = "1" } } } });
            rejected = false;
            try { source.Delete(producer.Id); } catch (InvalidOperationException) { rejected = true; }
            Assert(rejected, "CASE3's output reference did not block deletion.");
            reader.Branches.RemoveAt(1);
            link.Next = producer.Id; link.Case1 = producer.Id; link.Case2 = producer.Id; link.Case3 = producer.Id;
            source.Program.InitialEntry = producer.Id; source.Program.AutoEntry = producer.Id;
            Flow_Chart control = (Flow_Chart)sourceForm.Controls.Find(producer.ControlName, true)[0];
            source.Delete(producer.Id);
            Assert(!source.Program.Steps.Contains(producer) && !Flow_Module.FlowChart_List.Contains(control), "Deleted node still exists in the global runtime.");
            Assert(new[] { link.Next, link.Case1, link.Case2, link.Case3, source.Program.InitialEntry, source.Program.AutoEntry }.All(string.IsNullOrEmpty), "Deleting a node left a dangling link or entry.");
        }
    }

    /// <summary>Check every page and child control even when a new node has not yet been imported into the engineering model.</summary>
    private static void ModuleDeletionGuards()
    {
        using (FutureStation form = new FutureStation { Name = "Delete guard" })
        {
            FlowModuleEditor module = FlowPrograms.Attach(form);
            bool rejected = false;
            try { FlowPrograms.DeleteModule(module); } catch (InvalidOperationException) { rejected = true; }
            Assert(rejected && !module.Program.Removed, "Nonempty inherited module was deleted.");
            module.Delete(module.Program.Steps.Single().Id);
            using (Panel nested = new Panel { Visible = false })
            {
                nested.Controls.Add(new Flow_Chart { Name = "unimported" });
                form.AutoPage.Controls.Add(nested);
                rejected = false;
                try { FlowPrograms.DeleteModule(module); } catch (InvalidOperationException) { rejected = true; }
                Assert(rejected && module.Program.Steps.Count == 0 && !module.Program.Removed, "Hidden, unmodeled child node did not block module deletion.");
                form.AutoPage.Controls.Remove(nested);
            }
            FlowPrograms.DeleteModule(module);
            Assert(module.Program.Removed && module.Program.Mode == "Disabled", "Empty module was not removed.");
            rejected = false;
            try { module.AddNode(); } catch (InvalidOperationException) { rejected = true; }
            Assert(rejected, "A deleted module accepted new nodes.");
        }
    }

    /// <summary>Restore an unsaved deletion, retain a saved deletion after restart and suppress the original form lifecycle.</summary>
    private static void ModuleDeletionPersistence()
    {
        using (FutureStation form = new FutureStation { Name = "Delete persistence" })
        {
            FlowModuleEditor module = FlowPrograms.Attach(form);
            module.Delete(module.Program.Steps.Single().Id);
            FlowPrograms.Save();
            FlowPrograms.DeleteModule(module);
            Assert(!module.IsRemovedPublished, "An unsaved module deletion changed production mode.");
            FlowPrograms.Reload();
            Assert(!module.Program.Removed, "Reload did not discard an unsaved module deletion.");
            FlowPrograms.DeleteModule(module); FlowPrograms.Save(); FlowPrograms.Reload();
            FlowProgram savedModule = FlowProgramFile.Load(FlowPrograms.FilePath).Modules.Single(item => item.ModuleId == module.Program.ModuleId);
            Assert(savedModule.Removed && savedModule.Steps.Count == 0 && module.IsRemovedPublished, "Saved deletion did not persist.");
            using (FutureStation restarted = new FutureStation { Name = "Delete persistence" })
            {
                FlowModuleEditor restored = new FlowModuleEditor(restarted, FlowProgramFile.Snapshot(savedModule));
                Assert(restored.Program.Steps.Count == 0 && !restarted.Canvas.Controls.Contains(restarted.Existing), "Restart recreated a removed original node.");
                Assert(restored.Handle(FlowProgramCommand.Start) && restarted.LegacyStarts == 0, "Deleted module ran its original callback.");
            }
            savedModule.Steps.Add(new FlowStep());
            Assert(FlowProgramFile.Validate(savedModule).Count > 0, "A corrupt removed module containing nodes was accepted.");
        }
    }

    /// <summary>Migrate the actual two historical fields and create all eight Setting examples once.</summary>
    private static void ConstantExamples()
    {
        ModuleManager.SettingDataDirectory = SysPara.SettingDataDirectory; Directory.CreateDirectory(ModuleManager.SettingDataDirectory);
        DataSet old = new DataSet(); DataTable emptyMarker = old.Tables.Add("MSet"); emptyMarker.Rows.Add(emptyMarker.NewRow());
        DataTable flags = old.Tables.Add("PSet"); flags.Columns.Add("DisScann", typeof(bool)); flags.Columns.Add("UseTryRun", typeof(bool)); flags.Rows.Add(true, false);
        old.WriteXml(Path.Combine(ModuleManager.SettingDataDirectory, "SysSetting.xml"));
        constantForm = new SystemSetting(); constantForm.ModuleInitialize(constantForm.Text); SetHost("SystemS", constantForm); RegisterForm(constantForm);
        Assert(constantForm.Constants.Table("Setting").Select("Key <> ''").Length == 8, "The existing switches were not added as eight Setting constants.");
        Assert((bool)FlowResources.Capture().Parameters["Constants/Setting/DisScann"], "Existing scanner setting was overwritten.");
        Assert(!(bool)FlowResources.Capture().Parameters["Constants/Setting/UseTryRun"], "Existing dry-run setting was overwritten.");
        constantForm.Constants.Put("Setting", "UseTryRun", new ConstantValue { Key = "UseTryRun", Type = "Boolean", Value = "True" }); constantForm.Constants.Save("Setting");
        Assert((bool)constantForm.GetSettingValue("PSet", "UseTryRun"), "Historical PSet binding is not preserved.");
    }

    /// <summary>Read only saved values and keep references stable when rows are edited, sorted or deleted.</summary>
    private static void ConstantCrud()
    {
        ConstantStore store = constantForm.Constants;
        store.Put("Setting", null, new ConstantValue { Key = "MaxRetries", Type = "Integer", Value = "3", Description = "Retry count" }); store.Save("Setting");
        store.Put("Setting", "MaxRetries", new ConstantValue { Key = "MaxRetries", Type = "Integer", Value = "5", Description = "Retry count" });
        Assert((long)FlowResources.Capture().Parameters["Constants/Setting/MaxRetries"] == 3L, "Unsaved constant leaked into the running snapshot.");
        store.Table("Setting").DefaultView.Sort = "Key DESC"; store.Save("Setting");
        Assert((long)FlowResources.Capture().Parameters["Constants/Setting/MaxRetries"] == 5L, "Sorting changed the constant reference.");
        store.Put("Setting", null, new ConstantValue { Key = "Temporary", Type = "Text", Value = "test" }); store.Save("Setting");
        store.Delete("Setting", "Temporary");
        Assert(FlowResources.Capture().Parameters.ContainsKey("Constants/Setting/Temporary"), "A draft deletion changed the production snapshot.");
        store.Save("Setting");
        Assert(!FlowResources.Capture().Parameters.ContainsKey("Constants/Setting/Temporary"), "Saved deletion was not published.");
        store.Delete("Setting", "Dis3D"); store.Save("Setting");
        using (SystemSetting reloaded = new SystemSetting())
        {
            try { reloaded.ModuleInitialize(reloaded.Text); Assert(ConstantStore.Find(reloaded.Constants.Table("Setting"), "Dis3D") == null, "Deleted examples reappeared on restart."); }
            finally { ModuleManager.ModuleList.Remove(reloaded); }
        }
        store.Put("Setting", null, new ConstantValue { Key = "Dis3D", Type = "Boolean", Value = "False" }); store.Save("Setting");
    }

    /// <summary>Use the real recipe loading entry and prove a missing Recipe key never reuses the previous model's value.</summary>
    private static void ConstantRecipes()
    {
        ConstantStore store = constantForm.Constants;
        string first = Path.Combine(SysPara.RecipeDataDirectory, "Constant-A.xml"), second = Path.Combine(SysPara.RecipeDataDirectory, "Constant-B.xml");
        store.Put("Recipe", null, new ConstantValue { Key = "Limit", Type = "Number", Value = "1.5" });
        store.Put("Recipe", null, new ConstantValue { Key = "OnlyA", Type = "Text", Value = "A" }); constantForm.WriteRecipeData(first);
        store.Table("Recipe").Clear(); store.Put("Recipe", null, new ConstantValue { Key = "Limit", Type = "Number", Value = "9.5" }); constantForm.WriteRecipeData(second);
        Assert((bool)host.GetMethod("OpenRecipe", BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { first }), "Recipe A did not load.");
        Assert((double)FlowResources.Capture().Parameters["Constants/Recipe/Limit"] == 1.5 && (long)FlowResources.Capture().Parameters["Constants/Setting/MaxRetries"] == 5L, "Recipe load mixed the two scopes.");
        Assert((bool)host.GetMethod("OpenRecipe", BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { second }), "Recipe B did not load.");
        FlowResources loaded = FlowResources.Capture();
        Assert((double)loaded.Parameters["Constants/Recipe/Limit"] == 9.5 && !loaded.Parameters.ContainsKey("Constants/Recipe/OnlyA"), "Recipe B retained Recipe A constants.");
        store.Put("Recipe", "Limit", new ConstantValue { Key = "Limit", Type = "Number", Value = "7.25" }); store.Save("Recipe");
        constantForm.ReadRecipeData(SysPara.FilePath);
        Assert((double)FlowResources.Capture().Parameters["Constants/Recipe/Limit"] == 7.25, "Recipe-table save did not reload.");
        Assert(recipe.RecipeData.Tables["ProductSetting"].Rows.Count > 0, "Saving constants damaged ProductManager data.");
    }

    /// <summary>Protect parameters referenced by any node input or CASE condition.</summary>
    private static void ConstantReferences()
    {
        ConstantStore store = constantForm.Constants;
        using (Flow_BaseForm form = FlowPrograms.AddModule("Constant consumer"))
        {
            FlowModuleEditor module = FlowPrograms.Attach(form); FlowStep node = module.AddNode();
            node.Inputs[0].Source = "Parameter"; node.Inputs[0].Reference = "Constants/Setting/MaxRetries";
            bool rejected = false; try { store.Delete("Setting", "MaxRetries"); } catch (InvalidOperationException) { rejected = true; } Assert(rejected, "A referenced constant was deleted.");
            rejected = false; try { store.Put("Setting", "MaxRetries", new ConstantValue { Key = "Renamed", Type = "Integer", Value = "1" }); } catch (InvalidOperationException) { rejected = true; } Assert(rejected, "A referenced constant was renamed.");
            node.Inputs[0].Source = "Constant";
            node.Branches.Add(new FlowBranch { Code = "CASE3", Conditions = new List<FlowCondition> { new FlowCondition { Left = new FlowInput { Source = "Parameter", Reference = "Constants/Setting/MaxRetries" }, Right = new FlowInput { Reference = "1" } } } });
            rejected = false; try { store.Put("Setting", "MaxRetries", new ConstantValue { Key = "MaxRetries", Type = "Text", Value = "1" }); } catch (InvalidOperationException) { rejected = true; } Assert(rejected, "A CASE reference did not protect the type.");
            module.Delete(node.Id); FlowPrograms.DeleteModule(module);
        }
    }

    /// <summary>Reject invalid types and preserve the last committed snapshot if atomic file replacement fails.</summary>
    private static void ConstantFailures()
    {
        ConstantStore store = constantForm.Constants;
        foreach (string invalid in new[] { "NaN", "Infinity", "1,25" })
        {
            bool rejected = false; try { store.Put("Setting", null, new ConstantValue { Key = "Invalid", Type = "Number", Value = invalid }); } catch (InvalidOperationException) { rejected = true; } Assert(rejected, "Invalid number was accepted: " + invalid);
        }
        store.Put("Setting", "MaxRetries", new ConstantValue { Key = "MaxRetries", Type = "Integer", Value = "8" });
        string previous = ModuleManager.SettingDataDirectory;
        ModuleManager.SettingDataDirectory = Path.Combine(root, "BlockedSettings"); Directory.CreateDirectory(Path.Combine(ModuleManager.SettingDataDirectory, "SysSetting.xml"));
        bool failed = false;
        try { store.Save("Setting"); } catch (IOException) { failed = true; } finally { ModuleManager.SettingDataDirectory = previous; }
        Assert(failed && (long)FlowResources.Capture().Parameters["Constants/Setting/MaxRetries"] == 5L, "Failed save published a draft value.");
        constantForm.SettingData.RejectChanges();
        bool denied = false; PermissionType role = SysPara.UserPermission; SysPara.UserPermission = PermissionType.Operator;
        try { store.Put("Setting", null, new ConstantValue { Key = "Unauthorized", Type = "Text", Value = "x" }); } catch (InvalidOperationException) { denied = true; } finally { SysPara.UserPermission = role; }
        Assert(denied, "Operator edited constants through a service entry.");
    }

    /// <summary>Render all three language versions and retain text, scope and typed selections during switching.</summary>
    private static void ConstantLanguages()
    {
        constantForm.Show(); Application.DoEvents();
        foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
        {
            SwitchLanguage(language); Application.DoEvents();
            using (Bitmap bitmap = new Bitmap(constantForm.Width,constantForm.Height)) { constantForm.DrawToBitmap(bitmap,new Rectangle(Point.Empty,constantForm.Size)); bitmap.Save(Path.Combine(root,"constants-"+language+".png")); }
        }
        using (ConstantEditForm dialog = new ConstantEditForm(new ConstantValue { Key = "Pending", Type = "Number", Value = "1.25" }, value => { }))
        {
            dialog.Show(); Application.DoEvents(); TextBox value = (TextBox)dialog.Controls.Find("constantValueValue",true)[0]; value.Text = "pending value";
            ComboBox type = (ComboBox)dialog.Controls.Find("constantTypeValue",true)[0];
            SwitchLanguage(LanguageType.Chinese); SwitchLanguage(LanguageType.English);
            Assert(value.Text == "pending value" && Convert.ToString(type.SelectedItem) == "Number" && dialog.Controls.Find("constantKeyValue",true)[0].Text == "Pending", "Language switch overwrote the constant draft.");
            dialog.Close();
        }
        constantForm.Hide(); SwitchLanguage(LanguageType.Chinese);
    }

    /// <summary>Create actual NPSDK templates without connecting hardware and verify their shared instances and setting entry.</summary>
    private static void HardwareGroups()
    {
        ioPage = new ManualForm(); ioPage.ModuleInitialize(ioPage.Text); SetHost("ManualF",ioPage); RegisterForm(ioPage);
        hardware.ModuleInitialize(hardware.Text);
        groupedAxis = new HardwareDefinition { Name = "Grouped X", Port = "2" };
        groupedInput = hardware.Resources.NewDefinition("Input"); groupedInput.Name = "Grouped sensor";
        groupedOutput = hardware.Resources.NewDefinition("Output"); groupedOutput.Name = "Grouped valve";
        HardwareEntry axis = hardware.Resources.Put(groupedAxis), input = hardware.Resources.Put(groupedInput), output = hardware.Resources.Put(groupedOutput);
        NP_AxisGroup group = (NP_AxisGroup)axis.Group;
        Assert(ReferenceEquals(group.Motor,group.Points.WorkMotor) && ReferenceEquals(group.Motor,group.Points.ParameterSet.Single().Axis), "The group duplicated its axis instance.");
        Assert(input.Device is NP_Input && output.Device is NP_Output && ((NP_IoGroup)output.Group).IO == output.Device, "IO direction or instance binding is wrong.");
        Assert(!axis.Device.InitializeWithSdk && !input.Device.InitializeWithSdk && !output.Device.InitializeWithSdk,"Host-managed resources would connect during SDK startup.");
        Assert(axis.Group.FindForm()==hardware && input.Group.FindForm()==ioPage && output.Group.FindForm()==ioPage,"Axis or IO groups were placed on the wrong page.");
        int settings = 0;
        using (NP_AxisGroup probeAxis = new NP_AxisGroup())
        using (NP_InputGroup probeInput = new NP_InputGroup())
        {
            probeAxis.ParametersRequested += (sender,args)=>settings++; probeAxis.ShowParameters();
            probeInput.ParametersRequested += (sender,args)=>settings++; probeInput.ShowParameters();
        }
        Assert(settings == 2, "Group settings did not come from the composite entry.");
        Assert(hardware.Resources.IsDirty, "Resource edits lost their Main Save boundary.");
        bool blocked = false; try { hardware.Resources.Put(new HardwareDefinition { Name = "Duplicate port",Port = "2" }); } catch (InvalidOperationException) { blocked = true; } Assert(blocked,"A second axis object was bound to the same physical channel.");
        hardware.Resources.Save();
        Assert(!hardware.Resources.IsDirty && HardwareResources.Load(hardware.Resources.PathName).Resources.Count == 3,"Resource definitions were not saved.");
        using (NP_Motor duplicate = new NP_Motor { Port=groupedAxis.Port,Vendor=groupedAxis.Vendor })
        {
            blocked = false; try { hardware.Resources.ValidateForSave(); } catch (InvalidOperationException) { blocked = true; }
            Assert(blocked,"Main Save did not detect a channel conflict introduced by a legacy control.");
        }
    }

    /// <summary>Capture committed point rows and real driver instances through stable hardware IDs.</summary>
    private static void HardwareBindings()
    {
        HardwareEntry entry = hardware.Resources.Entries.Single(item=>item.Definition.Id==groupedAxis.Id);
        NP_AxisGroup group = (NP_AxisGroup)entry.Group;
        group.Points.DataSet1.Rows.Add(25.5,"Fixture point"); hardware.RecipeData.AcceptChanges();
        FlowResources capture = FlowResources.Capture();
        string prefix = "hardware/"+groupedAxis.Id+"/";
        Assert(ReferenceEquals(capture.Items[prefix+"axis"].Instance,entry.Device),"Flow nodes use a different axis object.");
        FlowPointAxis points = (FlowPointAxis)capture.Items[prefix+"points/Position"].Instance;
        Assert(points.Positions[0]==25.5 && ReferenceEquals(points.Motor,entry.Device),"The point binding lost its row or axis.");
        Assert(capture.Items["hardware/"+groupedInput.Id+"/input"].Instance is NP_Input && capture.Items["hardware/"+groupedOutput.Id+"/output"].Instance is NP_Output,"IO resources are not discoverable.");
        string stableId = entry.Definition.Id; HardwareDefinition renamed = entry.Definition.Copy(); renamed.Name = "Renamed X";
        hardware.Resources.Put(renamed); hardware.Resources.Save();
        Assert(FlowResources.Capture().Items.ContainsKey("hardware/"+stableId+"/points/Position"),"Renaming a group changed its flow identity.");
    }

    /// <summary>Use real recipe XML loading and metadata reload while keeping Setting hardware addresses unchanged.</summary>
    private static void HardwarePersistence()
    {
        DataTable table = hardware.RecipeData.Tables[HardwareResources.PointsName(groupedAxis.Id)];
        string first = Path.Combine(SysPara.RecipeDataDirectory,"Hardware-A.xml"), second = Path.Combine(SysPara.RecipeDataDirectory,"Hardware-B.xml");
        hardware.WriteRecipeData(first); table.Rows[0]["Position"] = 81.25; hardware.WriteRecipeData(second);
        Assert((bool)host.GetMethod("OpenRecipe",BindingFlags.Static|BindingFlags.Public).Invoke(null,new object[]{first}),"Hardware recipe A failed to open.");
        Assert((double)((FlowPointAxis)FlowResources.Capture().Items["hardware/"+groupedAxis.Id+"/points/Position"].Instance).Positions[0]==25.5,"Point rows did not follow recipe A.");
        Assert((bool)host.GetMethod("OpenRecipe",BindingFlags.Static|BindingFlags.Public).Invoke(null,new object[]{second}),"Hardware recipe B failed to open.");
        using (HardForm restored = new HardForm())
        {
            try
            {
                restored.ModuleInitialize(restored.Text); restored.ReadRecipeData(second);
                HardwareEntry axis = restored.Resources.Entries.Single(item=>item.Definition.Id==groupedAxis.Id);
                Assert(axis.Definition.Port=="2" && ((NP_AxisGroup)axis.Group).Points.DataSet1.Rows[0].Field<double>("Position")==81.25,"Hardware definitions or recipe points failed to reload.");
            }
            finally { ModuleManager.ModuleList.Remove(restored); }
        }
        hardware.ReadRecipeData(second);
    }

    /// <summary>Prevent dangling hardware targets, point parameters and deletion while a flow owns the IO channel.</summary>
    private static void HardwareDeletion()
    {
        using (Flow_BaseForm form = FlowPrograms.AddModule("Hardware consumer"))
        {
            FlowModuleEditor module = FlowPrograms.Attach(form); FlowStep node = module.AddNode();
            node.Target = "hardware/"+groupedAxis.Id+"/axis";
            bool blocked = false; try { hardware.Resources.Delete(groupedAxis.Id); } catch (InvalidOperationException) { blocked = true; } Assert(blocked,"A hardware action target was deleted.");
            node.Target = ""; node.Inputs[0].Source = "Parameter"; node.Inputs[0].Reference = "Recipe/HardForm/"+HardwareResources.PointsName(groupedAxis.Id)+"/0/Position";
            blocked = false; try { hardware.Resources.Delete(groupedAxis.Id); } catch (InvalidOperationException) { blocked = true; } Assert(blocked,"Referenced point data did not block deletion.");
            module.Delete(node.Id); FlowPrograms.DeleteModule(module);
        }
        HardwareEntry output = hardware.Resources.Entries.Single(item=>item.Definition.Id==groupedOutput.Id);
        using (FlowResourceGate.Lease lease = FlowResourceGate.Acquire(new object[]{output.Device},CancellationToken.None).GetAwaiter().GetResult())
        {
            bool blocked = false; try { hardware.Resources.Delete(groupedOutput.Id); } catch (InvalidOperationException) { blocked = true; } Assert(blocked,"Leased IO was deleted.");
        }
        ControlBaseInterface removed = output.Device; hardware.Resources.Delete(groupedOutput.Id); hardware.Resources.Save();
        Assert(removed.IsDisposed && !SDKPara.ControlsSnapshot().Contains(removed) && HardwareResources.Load(hardware.Resources.PathName).Resources.All(item=>item.Id!=groupedOutput.Id),"Deleted IO remained registered or reloaded.");
    }

    /// <summary>Render group templates in three languages and preserve unfinished parameter edits and actual point rows.</summary>
    private static void HardwareLanguages()
    {
        host.GetMethod("InitialLanguageData",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);
        hardware.Show(); ((Button)hardware.Controls.Find("configuredResources",true)[0]).PerformClick(); Application.DoEvents();
        Control panel = hardware.Controls.Find("HardwareResourcePanel",true)[0], toolbar = hardware.Controls.Find("resourceToolbar",false)[0];
        Assert(!panel.Bounds.IntersectsWith(toolbar.Bounds), "The group canvas covers the hardware navigation toolbar.");
        DataTable table = hardware.RecipeData.Tables[HardwareResources.PointsName(groupedAxis.Id)]; DataRow row = table.Rows[0];
        ioPage.Show(); ((TabControl)ioPage.Controls.Find("tabControl1",true)[0]).SelectedTab = (TabPage)ioPage.Controls.Find("ioResourcesPage",true)[0];
        foreach (LanguageType language in new[]{LanguageType.Chinese,LanguageType.English,LanguageType.Español})
        {
            SwitchLanguage(language); Application.DoEvents(); Assert(ReferenceEquals(table.Rows[0],row) && (double)row["Position"]==81.25,"Language switch rebuilt point rows.");
            using (Bitmap bitmap = new Bitmap(hardware.Width,hardware.Height)) { hardware.DrawToBitmap(bitmap,new Rectangle(Point.Empty,hardware.Size)); bitmap.Save(Path.Combine(root,"hardware-"+language+".png")); }
            using (Bitmap bitmap = new Bitmap(ioPage.Width,ioPage.Height)) { ioPage.DrawToBitmap(bitmap,new Rectangle(Point.Empty,ioPage.Size)); bitmap.Save(Path.Combine(root,"io-"+language+".png")); }
        }
        using (HardwareEditForm dialog = new HardwareEditForm(groupedAxis,value=>{}))
        {
            dialog.Show(); Application.DoEvents(); DataGridView grid = (DataGridView)dialog.Controls.Find("hardwareParameters",true)[0];
            grid.CurrentCell = grid.Rows.Cast<DataGridViewRow>().Single(item=>Convert.ToString(item.Cells[0].Value)=="WorkSpeed").Cells[1]; grid.BeginEdit(false); ((TextBox)grid.EditingControl).Text = "pending speed";
            SwitchLanguage(LanguageType.Chinese); SwitchLanguage(LanguageType.English);
            Assert(((TextBox)grid.EditingControl).Text=="pending speed","Language switch committed the hardware parameter edit."); dialog.Close();
        }
        hardware.Hide(); ioPage.Hide(); SwitchLanguage(LanguageType.Chinese);
    }

    /// <summary>Refresh a stable snapshot while the UI thread creates and disposes composite control instances.</summary>
    private static void HardwareRegistry()
    {
        int before = SDKPara.ControlsSnapshot().Length;
        Task scan = Task.Run(()=>{ for(int i=0;i<200;i++) SDKKernal.RefreshControls(); });
        for(int i=0;i<30;i++) using(NP_InputGroup group = new NP_InputGroup()) { Assert(SDKPara.ControlsSnapshot().Contains(group.IO),"New group IO was not registered."); }
        scan.GetAwaiter().GetResult(); Assert(SDKPara.ControlsSnapshot().Length==before,"Disposed group controls leaked into the SDK scan registry.");
    }

    /// <summary>Exercise real mouse capture with a fake driver and no physical motion card.</summary>
    private static void JogCapture()
    {
        using (Form fixture = new Form())
        using (NP_AxisGroup group = new NP_AxisGroup())
        {
            JogDriver driver = new JogDriver();
            typeof(NP_Motor).GetField("dMotor",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(group.Motor,driver);
            typeof(NP_Motor).GetField("ready",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(group.Motor,true);
            fixture.Controls.Add(group); fixture.Show(); Application.DoEvents();
            ((CheckBox)group.Controls.Find("axisContinuous",true)[0]).Checked = true;
            Control positive = group.Controls.Find("axisPositive",true)[0];
            RaiseMouse(positive,"OnMouseDown",new MouseEventArgs(MouseButtons.Left,1,5,5,0));
            Assert(driver.Starts==1 && driver.Stops==0 && group.IsManualRunning,"Gaining mouse capture stopped the jog before it began.");
            RaiseMouse(positive,"OnMouseUp",new MouseEventArgs(MouseButtons.Left,1,5,5,0)); positive.Capture = false;
            Assert(driver.Stops==1 && !group.IsManualRunning,"Releasing the jog did not stop exactly once.");
            RaiseMouse(positive,"OnMouseDown",new MouseEventArgs(MouseButtons.Left,1,5,5,0)); positive.Capture = false;
            Assert(driver.Starts==2 && driver.Stops==2 && !group.IsManualRunning,"Losing mouse capture left the axis jogging.");
            fixture.Close();
        }
    }

    /// <summary>Initialize a temporary registry containing only fake controls, then restore every original object.</summary>
    private static void DeferredInitialization()
    {
        ControlBaseInterface[] original = SDKPara.ControlsSnapshot();
        lock (SDKPara.ControlList) SDKPara.ControlList.Clear();
        try
        {
            using (InitializationProbe automatic = new InitializationProbe())
            using (InitializationProbe deferred = new InitializationProbe { InitializeWithSdk = false })
            {
                NSDK.InitialControls(); Assert(automatic.Starts==1 && deferred.Starts==0,"SDK initialized a deferred host-managed resource.");
            }
        }
        finally { lock (SDKPara.ControlList) { SDKPara.ControlList.Clear(); SDKPara.ControlList.AddRange(original); } }
    }

    /// <summary>Verify menus locate clicked rows and groups without using previous selection or confirmation dialogs.</summary>
    private static void ContextTargets()
    {
        hardware.Show(); ((Button)hardware.Controls.Find("configuredResources",true)[0]).PerformClick(); Application.DoEvents();
        HardwareResourcePanel panel = (HardwareResourcePanel)hardware.Controls.Find("HardwareResourcePanel",true)[0];
        HardwareEntry input = hardware.Resources.Entries.Single(item=>item.Definition.Id==groupedInput.Id);
        ioPage.Show(); ((TabControl)ioPage.Controls.Find("tabControl1",true)[0]).SelectedTab = (TabPage)ioPage.Controls.Find("ioResourcesPage",true)[0]; Application.DoEvents();
        HardwareResourcePanel ioPanel = (HardwareResourcePanel)ioPage.Controls.Find("ioResourcePanel",true)[0];
        ContextMenuStrip menu = input.Group.ContextMenuStrip; menu.Show(input.Group,new Point(5,5)); Application.DoEvents();
        Assert((string)typeof(HardwareResourcePanel).GetField("contextResourceId",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(ioPanel)==groupedInput.Id,"Resource menu used the toolbar's previous selection."); menu.Close(); ioPage.Hide();
        Assert(panel.Controls.Find("resourceDelete",true).Length==0,"A resource delete button remains on the toolbar.");
        constantForm.Show(); Application.DoEvents(); DataGridView constants = (DataGridView)constantForm.Controls.Find("constantsGrid",true)[0];
        Rectangle area = constants.GetCellDisplayRectangle(0,1,false); RaiseMouse(constants,"OnMouseDown",new MouseEventArgs(MouseButtons.Right,1,area.Left+4,area.Top+4,0));
        constants.ContextMenuStrip.Show(constants,new Point(area.Left+4,area.Top+4)); Application.DoEvents();
        Assert((string)typeof(SystemSetting).GetField("contextKey",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(constantForm)==Convert.ToString(constants.Rows[1].Cells[0].Value),"Constant menu did not target the clicked row."); constants.ContextMenuStrip.Close();
        RaiseMouse(constants,"OnMouseDown",new MouseEventArgs(MouseButtons.Right,1,5,5,0)); constants.ContextMenuStrip.Show(constants,new Point(5,5));
        Assert(!constants.ContextMenuStrip.Visible,"A constant header inherited a previous row deletion."); constantForm.Hide(); hardware.Hide();
        using (Form fixture = new Form())
        using (NP_PositionTable points = new NP_PositionTable())
        {
            fixture.Controls.Add(points); fixture.Show(); Application.DoEvents(); points.DataSet1.Rows.Add(1); points.DataSet1.Rows.Add(2); Application.DoEvents();
            DataGridView grid = points.Position; area = grid.GetCellDisplayRectangle(0,1,false);
            RaiseMouse(grid,"OnMouseDown",new MouseEventArgs(MouseButtons.Right,1,area.Left+4,area.Top+4,0)); grid.ContextMenuStrip.Show(grid,new Point(area.Left+4,area.Top+4)); Application.DoEvents();
            Assert(ReferenceEquals(typeof(NP_PositionTable).GetField("contextRow",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(points),points.DataSet1.Rows[1]),"Point menu did not target the clicked row.");
            Assert(points.Controls.Find("btn_DeletePost",true).Length==0 && !grid.AllowUserToDeleteRows,"Point deletion still has a button or keyboard shortcut."); grid.ContextMenuStrip.Close();
            points.CanEdit = ()=>false; grid.ContextMenuStrip.Show(grid,new Point(area.Left+4,area.Top+4)); Assert(!grid.ContextMenuStrip.Items[0].Enabled,"Point deletion ignored the host permission guard."); grid.ContextMenuStrip.Close(); fixture.Close();
        }
    }

    /// <summary>Complete the real modal editor through button events, checking defaults, parent page and saved definition reload.</summary>
    private static void HardwareAddUi()
    {
        hardware.Show(); ((Button)hardware.Controls.Find("configuredResources",true)[0]).PerformClick();
        ioPage.Show(); ((TabControl)ioPage.Controls.Find("tabControl1",true)[0]).SelectedTab = (TabPage)ioPage.Controls.Find("ioResourcesPage",true)[0]; Application.DoEvents();
        HardwareResourcePanel axisPanel = (HardwareResourcePanel)hardware.Controls.Find("hardwareResourcePanel",true)[0];
        HardwareResourcePanel ioPanel = (HardwareResourcePanel)ioPage.Controls.Find("ioResourcePanel",true)[0];
        Assert(!axisPanel.Controls.Find("resourceAddInput",true)[0].Visible && !ioPanel.Controls.Find("resourceAddAxis",true)[0].Visible,"Axis and IO creation tools are mixed on one page.");
        foreach (string kind in new[]{"Axis","Input","Output"})
        {
            string[] original = hardware.Resources.Entries.Select(entry=>entry.Definition.Id).ToArray();
            Exception failure = null; bool confirmed = false;
            using (System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer { Interval=30 })
            {
                timer.Tick += (sender,args)=>
                {
                    HardwareEditForm dialog = Application.OpenForms.OfType<HardwareEditForm>().FirstOrDefault();
                    if (dialog==null) return;
                    timer.Stop();
                    try { confirmed=true; ((Button)dialog.Controls.Find("hardwareOk",true)[0]).PerformClick(); }
                    catch (Exception error) { failure=error; dialog.DialogResult=DialogResult.Cancel; }
                };
                timer.Start(); Control owner = kind=="Axis" ? (Control)axisPanel : ioPanel;
                ((Button)owner.Controls.Find("resourceAdd"+kind,true)[0]).PerformClick();
            }
            if (failure!=null) throw failure;
            Assert(confirmed,"The add button did not open its parameter editor.");
            HardwareEntry created = hardware.Resources.Entries.Single(entry=>!original.Contains(entry.Definition.Id));
            Assert(created.Definition.Kind==kind && created.Group.FindForm()==(kind=="Axis" ? (Form)hardware : ioPage),"Confirmed resource was not created on the correct page.");
            hardware.Resources.Save(); Assert(HardwareResources.Load(hardware.Resources.PathName).Resources.Any(item=>item.Id==created.Definition.Id),"Confirmed resource failed to persist.");
            hardware.Resources.Delete(created.Definition.Id); hardware.Resources.Save();
        }
        hardware.Hide(); ioPage.Hide();
    }

    /// <summary>Remove only draft rows via right-click commands, and retain NEXT as a mandatory exit.</summary>
    private static void DraftMenus()
    {
        using (Form fixture = new Form())
        using (FlowBranchEditor branch = new FlowBranchEditor())
        using (FlowSchemaForm schema = new FlowSchemaForm(data))
        {
            branch.Bind(new FlowBranch { Code="CASE1" },"",new List<FlowChoice>{new FlowChoice{Id="",Label="End"}});
            branch.MakeConditional(); fixture.Controls.Add(branch); fixture.Show(); Application.DoEvents();
            FlowConditionRow conditionRow = branch.ConditionRows.First();
            conditionRow.ContextMenuStrip.Show(conditionRow,new Point(4,4)); conditionRow.ContextMenuStrip.Items[0].PerformClick();
            Assert(!branch.ConditionRows.Any(),"Condition context deletion failed.");
            Assert(branch.Controls.Find("removeBranch",true).Length==0,"CASE still has a toolbar delete button.");
            schema.Show(); Application.DoEvents(); DataGridView grid = (DataGridView)schema.Controls.Find("productionFields",true)[0]; int before = grid.Rows.Count;
            Rectangle area = grid.GetCellDisplayRectangle(0,0,false); RaiseMouse(grid,"OnMouseDown",new MouseEventArgs(MouseButtons.Right,1,area.Left+3,area.Top+3,0)); grid.ContextMenuStrip.Show(grid,new Point(area.Left+3,area.Top+3)); grid.ContextMenuStrip.Items[0].PerformClick();
            Assert(grid.Rows.Count==before-1 && !grid.AllowUserToDeleteRows,"Production-field context deletion failed."); schema.Close(); fixture.Close();
        }
    }

    /// <summary>Verify module hosting, right-click targeting and removal of empty modules from the actual flow page.</summary>
    private static void FlowPage()
    {
        Flow_BaseForm created = FlowPrograms.AddModule("Configured module");
        using (Flow page = new Flow())
        {
            page.Show(); Application.DoEvents();
            Assert(page.Controls.Find("initialize",true).Length == 0 && page.Controls.Find("test",true).Length == 0 && page.Controls.Find("simulation",true).Length == 0, "The flow page still has separate initialization/start or simulation controls.");
            TabControl tabs = (TabControl)page.Controls.Find("tabControl1", true)[0];
            Assert(page.Controls.Find("delete",true).Length==0 && page.Controls.Find("deleteModule",true).Length==0,"Flow toolbar still exposes delete buttons.");
            int moduleIndex = tabs.TabPages.Cast<TabPage>().ToList().FindIndex(tab=>tab.Text=="Configured module");
            Rectangle header = tabs.GetTabRect(moduleIndex); RaiseMouse(tabs,"OnMouseDown",new MouseEventArgs(MouseButtons.Right,1,header.Left+5,header.Top+5,0));
            Assert(ReferenceEquals(typeof(Flow).GetField("contextPage",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(page),FlowPrograms.Attach(created)),"Module context menu selected a different page.");
            ((ContextMenuStrip)typeof(Flow).GetField("moduleMenu",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(page)).Close(); tabs.SelectedIndex = 0;
            Assert(tabs.TabPages.Cast<TabPage>().Any(tab => tab.Text == "FutureStation") && tabs.TabPages.Cast<TabPage>().Any(tab => tab.Text == "Configured module"), "Generic module hosting failed.");
            Flow_Chart visible = (Flow_Chart)station.Controls.Find(model.ControlName, true)[0];
            ((ScrollableControl)visible.Parent).ScrollControlIntoView(visible); Application.DoEvents();
            Assert(visible.Visible && visible.Parent.ClientRectangle.IntersectsWith(visible.Bounds), "Restored nodes are not visible on the flow page.");
            Assert(visible.Parent.GetChildAtPoint(new Point(visible.Left + 5, visible.Top + 5)) == visible, "The existing canvas covers configured nodes.");
            Button surface = visible.Controls.OfType<Button>().Single();
            ContextMenuStrip menu = surface.ContextMenuStrip;
            Assert(menu != null && menu.Items["nodeDelete"] != null && menu.Items["nodeProperties"] != null, "The node's internal button has no property/delete context menu.");
            menu.Show(surface, new Point(5, 5)); Application.DoEvents();
            FieldInfo menuNode = typeof(Flow).GetField("contextNodeId", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert(Convert.ToString(menuNode.GetValue(page)) == model.Id && menu.Items["nodeDelete"].Enabled, "Right-click deletion targets the wrong node."); menu.Close();
            SwitchLanguage(LanguageType.English);
            Assert(menu.Items["nodeDelete"].Text == "Delete node" && menu.Items["nodeProperties"].Text == "Edit properties", "Context menu did not follow the framework language switch."); SwitchLanguage(LanguageType.Chinese);
            VerifyNodeProperties(menu, surface);
            menu.Show(surface, new Point(5, 5)); Application.DoEvents();
            menu.Items["nodeBreakpoint"].PerformClick(); menu.Close();
            Assert(editor.Program.Steps.Single(item => item.Id == model.Id).Breakpoint && visible.ProgramBreakpoint, "Node context breakpoint did not update its model and red marker.");
            menu.Show(surface, new Point(5, 5)); menu.Items["nodeBreakpoint"].PerformClick(); menu.Close();
            VerifyDebugCommands(page, FlowPrograms.Attach(created), tabs);
            FlowPrograms.DeleteModule(FlowPrograms.Attach(created)); page.AddFlowChart();
            Application.DoEvents();
            Assert(!tabs.TabPages.Cast<TabPage>().Any(tab => tab.Text == "Configured module") && !created.IsDisposed, "Deleted module remains visible or lost reload support.");
            Assert(visible.Visible && station.Visible, "Removing another module hid the remaining module.");
            VerifyToolboxDrops(page);
            VerifyCompactFlowPage(page);
            using (Bitmap bitmap = new Bitmap(page.Width, page.Height)) { page.DrawToBitmap(bitmap, new Rectangle(Point.Empty, page.Size)); bitmap.Save(Path.Combine(root, "flow-page.png")); }
            page.Close();
        }
    }

    /// <summary>Keep the unified flow controls reachable at normal and narrow widths in every supported language.</summary>
    private static void VerifyCompactFlowPage(Flow page)
    {
        Assert(page.Controls.Find("modeValue", true).Length == 0 && page.Controls.Find("modeEntryLabel", true).Length == 0,
            "The obsolete Legacy/Program execution-mode selector is still visible.");
        CheckBox enabled = (CheckBox)page.Controls.Find("moduleEnabled", true).Single();
        FlowModuleEditor current = (FlowModuleEditor)((TabControl)page.Controls.Find("tabControl1", true).Single()).SelectedTab.Tag;
        enabled.Checked = false; Assert(current.Program.Mode == "Disabled", "The flow enable switch did not update its draft.");
        enabled.Checked = true; Assert(current.Program.Mode == "Program", "The flow enable switch restored an obsolete mode.");
        foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
        {
            SwitchLanguage(language);
            foreach (int width in new[] { 1280, 900 })
            {
                page.ClientSize = new Size(width, 700); Application.DoEvents();
                foreach (string name in new[] { "toolbar", "entries" })
                {
                    FlowLayoutPanel row = (FlowLayoutPanel)page.Controls.Find(name, true).Single();
                    Assert(row.AutoSize && row.Controls.Cast<Control>().Where(control => control.Visible).All(control => row.ClientRectangle.Contains(control.Bounds)),
                        "A compact flow command or entry was clipped: " + name + " / " + language + " / " + width);
                }
                Control workspace = page.Controls.Find("flowWorkspace", true).Single();
                Assert(workspace.Top < 180 && workspace.Height > 300, "Fixed toolbar whitespace squeezed the flow canvas.");
                Screenshot(page, "flow-compact-" + language + "-" + width);
            }
        }
        SwitchLanguage(LanguageType.Chinese); page.ClientSize = new Size(1280, 700);
    }

    /// <summary>Exercise the actual flow-page debugging controls and observe action output in three languages.</summary>
    private static void VerifyDebugCommands(Flow page, FlowModuleEditor module, TabControl tabs)
    {
        FlowStep step = module.AddNode(); step.ExecutionMode = "Action"; step.Action = "value"; step.Target = "";
        step.Inputs = new List<FlowInput> { new FlowInput { Name = "Value", Reference = "4.25" } };
        CheckBox debug = (CheckBox)page.Controls.Find("debugMode", true)[0];
        try
        {
            tabs.SelectedTab = tabs.TabPages.Cast<TabPage>().Single(tab => ReferenceEquals(tab.Tag, module));
            debug.Checked = true; Application.DoEvents();
            Assert(FlowPrograms.DebugEnabled, "The flow debug switch did not configure the shared start path.");
            module.StartDebug(step.Id, true);
            ActionUntil(() => module.Runtime.IsDebugPaused, "Flow-page debug never stopped at its entry.");
            typeof(Flow).GetMethod("RefreshRuntime", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(page, new object[] { null, EventArgs.Empty });
            Control panel = page.Controls.Find("debugPanel", true)[0];
            Assert(panel.Visible, "The flow-page input/output observer is hidden during debugging.");
            DataGridView grid = (DataGridView)panel.Controls.Find("debugValues", true)[0];
            Assert(grid.Rows.Cast<DataGridViewRow>().Any(row => Convert.ToString(row.Cells[2].Value) == "4.25"), "The paused node did not show its prepared input value.");
            ((Button)panel.Controls.Find("debugStep", true)[0]).PerformClick(); Wait(module.Runtime.Completion);
            foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
            {
                SwitchLanguage(language); Application.DoEvents();
                typeof(Flow).GetMethod("RefreshRuntime", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(page, new object[] { null, EventArgs.Empty });
                Assert(grid.Rows.Cast<DataGridViewRow>().Any(row => Convert.ToString(row.Cells[2].Value) == "4.25"), "Language refresh lost the completed action output.");
                Screenshot(page, "action-debug-page-" + language);
            }
        }
        finally
        {
            module.Runtime.Stop(); Wait(module.Runtime.Completion); debug.Checked = false;
            FlowPrograms.DebugEnabled = false; module.Delete(step.Id); SwitchLanguage(LanguageType.Chinese);
            tabs.SelectedIndex = 0;
        }
    }
}
