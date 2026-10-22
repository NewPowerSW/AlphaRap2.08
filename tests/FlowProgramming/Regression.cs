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
    /// <summary>Detect accidental execution of an existing application callback.</summary>
    public override void StartRun() { LegacyStarts++; }
    public readonly Panel Canvas = new Panel { Name = "canvas", Dock = DockStyle.Fill };
    public readonly Flow_Chart Existing = new Flow_Chart { Name = "existing", text = "Existing callback", Location = new Point(40, 100) };
    /// <summary>Represent an independently added subclass with a nested node.</summary>
    public FutureStation() { Name = "FutureStation"; Canvas.Controls.Add(Existing); Controls.Add(Canvas); }
}

internal static class Cases
{
    private static int count;
    private static int failures;
    private static string root;
    private static Type host;
    private static DataForm data;
    private static ModuleBaseForm recipe;
    private static HardForm hardware;
    private static FutureStation station;
    private static FlowModuleEditor editor;
    private static FlowStep model;
    private static FlowStep measured;
    private static FlowStep save;

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
        Test("timeout starts an action once and follows the failure branch", Timeout);
        Test("cancellation retains ownership until the underlying action finishes", StopLease);
        Test("cross-cycle and expired run outputs are rejected", ResultIsolation);
        Test("condition waiting observes another completed flow without repeating the action", WaitForOutput);
        Test("cycle keys cannot reuse previous product results", CycleReuse);
        Test("completing a product cycle releases its outputs before reuse", CompleteCycle);
        Test("engineering drafts do not change the published execution mode", PublishedDraft);
        Test("SDK lifecycle dispatch invokes the published designer flow", Lifecycle);
        Test("save/reload restores nodes, layout, links and deleted original nodes", Persistence);
        Test("damaged project does not overwrite the active design", CorruptLoad);
        Test("three-language node editor retains pending values", EditorLanguages);
        Test("all inherited forms and runtime-created modules appear in the flow page", FlowPage);
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
        data.SaveProductionSchema(new List<ProductionField> { new ProductionField { Name = "Product" }, new ProductionField { Name = "Measurement", Type = "Number" } });
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
        measured.Conditions.Add(new FlowCondition { Left = new FlowInput { Source = "Result", Reference = "Value" }, Operator = "<=", Right = new FlowInput { Reference = "3" } });
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
        first.Failure = failure.Id; int starts = 0; bool handled = false;
        FlowProgramRuntime runtime = new FlowProgramRuntime();
        runtime.ExecuteAction = async (step, context, token) => { if (step.Id == first.Id) { starts++; await Task.Delay(500, token); } else handled = true; return new Dictionary<string, object>(); };
        runtime.Start(new FlowProgram { Steps = new List<FlowStep> { first, failure } }, first.Id, Context(new FlowResultHub())); Wait(runtime.Completion);
        Assert(starts == 1 && handled && runtime.Error == null, "Timeout resent the action or failed to branch.");
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
        FlowStep step = new FlowStep { Caption = "Join", WaitForCondition = true, TimeoutMs = 1000 };
        step.Conditions.Add(new FlowCondition { Left = new FlowInput { Source = "Output", Reference = "other/node/Value" }, Right = new FlowInput { Reference = "1" } });
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
        Assert(!editor.IsEnabled, "Draft mode unexpectedly changed production dispatch.");
        FlowPrograms.Save(); Assert(editor.IsEnabled, "Saved program did not publish.");
        editor.Program.Mode = "Legacy"; Assert(editor.IsEnabled, "Unsaved draft changed the active mode."); editor.Program.Mode = "Program";
    }

    /// <summary>Exercise the base-form controller entry used by the machine lifecycle.</summary>
    private static void Lifecycle()
    {
        SysPara.Simulation = true;
        Assert(station.TryProgramCommand(FlowProgramCommand.Initialize) && station.InitialStatus, "Generic initialization was not intercepted.");
        FlowPrograms.BeginBatch();
        Assert(station.TryProgramCommand(FlowProgramCommand.Start), "Generic production start was not intercepted.");
        Wait(editor.Runtime.Completion);
        Assert(editor.Runtime.Error == null && station.LegacyStarts == 0, "An existing callback was mixed into the designer runtime.");
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

    /// <summary>Verify the actual flow page hosts independently added forms and newly created base modules.</summary>
    private static void FlowPage()
    {
        Flow_BaseForm created = FlowPrograms.AddModule("Configured module");
        using (Flow page = new Flow())
        {
            page.Show(); Application.DoEvents();
            TabControl tabs = (TabControl)page.Controls.Find("tabControl1", true)[0];
            Assert(tabs.TabPages.Cast<TabPage>().Any(tab => tab.Text == "FutureStation") && tabs.TabPages.Cast<TabPage>().Any(tab => tab.Text == "Configured module"), "Generic module hosting failed.");
            Flow_Chart visible = (Flow_Chart)station.Controls.Find(model.ControlName, true)[0];
            ((ScrollableControl)visible.Parent).ScrollControlIntoView(visible); Application.DoEvents();
            Assert(visible.Visible && visible.Parent.ClientRectangle.IntersectsWith(visible.Bounds), "Restored nodes are not visible on the flow page.");
            Assert(visible.Parent.GetChildAtPoint(new Point(visible.Left + 5, visible.Top + 5)) == visible, "The existing canvas covers configured nodes.");
            using (Bitmap bitmap = new Bitmap(page.Width, page.Height)) { page.DrawToBitmap(bitmap, new Rectangle(Point.Empty, page.Size)); bitmap.Save(Path.Combine(root, "flow-page.png")); }
            page.Close();
        }
    }
}
