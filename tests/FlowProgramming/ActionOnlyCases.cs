using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using AlphaRap;
using AlphaRap.FlowProgramming;
using NPSDK;

internal sealed class ActionLeaseProbe : IFlowObject
{
    internal int Calls, Simulations;
    internal bool OwnedDuringCall;
    internal readonly TaskCompletionSource<bool> Started = new TaskCompletionSource<bool>();
    internal readonly TaskCompletionSource<bool> Release = new TaskCompletionSource<bool>();

    /// <summary>Expose one hardware-shaped asynchronous method without accessing physical equipment.</summary>
    public IEnumerable<FlowMethod> GetFlowMethods(bool includeDraft = false)
    {
        FlowMethod method = new FlowMethod("probe.wait", "测试等待", "Probe wait", "Espera de prueba") { UsesHardware = true };
        method.Outputs.Add(new FlowField("Count", typeof(long)));
        method.Invoke = async call =>
        {
            Interlocked.Increment(ref Calls); OwnedDuringCall = FlowResourceGate.CanAccess(this); Started.TrySetResult(true);
            await Release.Task.ConfigureAwait(false); call.Token.ThrowIfCancellationRequested();
            return FlowCall.Result("Count", 42L);
        };
        method.Simulate = call => { Simulations++; return Task.FromResult(FlowCall.Result("Count", 900L)); };
        yield return method;
    }
}

internal static partial class Cases
{
    /// <summary>Register supported execution choices and explicit recovery of retired node definitions.</summary>
    private static void RegisterActionOnlyCases()
    {
        Test("new nodes allow only configured actions and conditions", ActionOnlyDefaults);
        Test("toolbox lists real object actions and conditions with no script or code entries in three languages", ActionOnlyToolbox);
        Test("retired Script and Code nodes reject execution and save without changing their XML", ActionOnlyRetiredPolicy);
        Test("retired nodes require explicit conversion and Cancel preserves the original definition", ActionOnlyConversions);
        Test("action object registration replacement and deletion use the actual external instance", ActionOnlyExternalResources);
        Test("real Action cancellation and timeout drain underlying object calls before releasing their leases", ActionOnlyCallOwnership);
        Test("simulated Action calls use the object's simulator without invoking hardware", ActionOnlySimulation);
    }

    /// <summary>Observe asynchronous runtime state while allowing UI-thread callbacks to complete.</summary>
    private static void ActionUntil(Func<bool> ready, string message)
    {
        DateTime limit = DateTime.UtcNow.AddSeconds(4);
        while (!ready() && DateTime.UtcNow < limit) { Application.DoEvents(); Thread.Sleep(5); }
        Assert(ready(), message);
    }

    /// <summary>Find a designer-owned child by its stable name.</summary>
    private static T ActionControl<T>(Control owner, string name) where T : Control
    {
        return (T)owner.Controls.Find(name, true).Single();
    }

    /// <summary>Serialize all persisted fields so validation and cancellation can be checked without migration.</summary>
    private static string ActionNodeXml(FlowStep step)
    {
        using (StringWriter text = new StringWriter())
        {
            new XmlSerializer(typeof(FlowStep)).Serialize(text, step);
            return text.ToString();
        }
    }

    /// <summary>Validate complete default Action and None definitions and reject obsolete drag payloads.</summary>
    private static void ActionOnlyDefaults()
    {
        foreach (string mode in new[] { "Action", "None" })
        {
            FlowStep created = editor.AddNode(mode, station.Canvas, new Point(50, 50));
            try
            {
                Assert(created.ExecutionMode == mode && FlowActions.IsSupported(created), "New node adopted a retired mode.");
                new FlowActions(FlowResources.Capture()).Validate(created);
            }
            finally { editor.Delete(created.Id); }
        }
        FlowStep standard = editor.AddNode();
        try
        {
            Assert(standard.ExecutionMode == "Action" && standard.Action == "value" && standard.Inputs.Count == 1,
                "The default node did not retain the configured Action contract.");
            Assert(ExecuteBound(new FlowActions(FlowResources.Capture()), standard).ContainsKey("Value"), "Default Action did not execute.");
            int before = editor.Program.Steps.Count;
            foreach (string retired in new[] { "Script", "Code" })
            {
                bool rejected = false;
                try { editor.AddNode(retired, station.Canvas, new Point(50, 50)); } catch (InvalidOperationException) { rejected = true; }
                Assert(rejected && editor.Program.Steps.Count == before, "A retired toolbox mode created a node.");
            }
        }
        finally { editor.Delete(standard.Id); }
        Assembly application = typeof(FlowNodeForm).Assembly;
        foreach (string retired in new[] { "FlowScripts", "FlowScriptEditor", "FlowScriptSettings", "FlowScriptContext", "FlowToolboxCatalog", "FlowCodeBindingsControl", "FlowCodeRoutesControl" })
            Assert(application.GetType("AlphaRap.FlowProgramming." + retired) == null, "Removed host implementation remains: " + retired);
    }

    /// <summary>Retain a filter while translating real capabilities and inserting through the keyboard command.</summary>
    private static void ActionOnlyToolbox()
    {
        using (Form form = new Form { Width = 430, Height = 450 })
        using (FlowToolboxControl toolbox = new FlowToolboxControl { Dock = DockStyle.Fill })
        {
            form.Controls.Add(toolbox); form.Show(); Application.DoEvents();
            FlowResources catalogue = FlowResources.Capture(true);
            toolbox.BindActions(new FlowActions(catalogue));
            TreeView tree = ActionControl<TreeView>(toolbox, "toolboxEntries");
            TextBox search = ActionControl<TextBox>(toolbox, "toolboxSearch");
            string[] chinese = null;
            foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
            {
                SwitchLanguage(language); Application.DoEvents();
                FlowToolboxEntry[] values = tree.Nodes.Cast<TreeNode>().SelectMany(group => group.Nodes.Cast<TreeNode>()).Select(node => (FlowToolboxEntry)node.Tag).ToArray();
                Assert(values.Length > 2 && values.All(item => item.ExecutionMode == "Action" || item.ExecutionMode == "None")
                    && values.Any(item => item.ExecutionMode == "None") && values.Any(item => item.ActionId == "point.move" && item.TargetId == "hardware/" + groupedAxis.Id + "/axis")
                    && values.Any(item => item.ActionId == "constant.set" && item.TargetId == "constants/Setting")
                    && values.Any(item => item.ActionId == "data.save" && item.TargetId == "data/Production")
                    && values.Any(item => item.ActionId == "value" && string.IsNullOrEmpty(item.TargetId)),
                    "Toolbox omitted real capabilities or retained a retired mode.");
                string[] titles = values.OrderBy(item => item.Id).Select(item => item.Title).ToArray();
                if (chinese == null) chinese = titles;
                else Assert(!titles.SequenceEqual(chinese), "Toolbox entries did not translate.");
                search.Text = "flow/value"; Application.DoEvents();
                Assert(tree.Nodes.Count == 1 && tree.Nodes[0].Nodes.Count == 1 && ((FlowToolboxEntry)tree.Nodes[0].Nodes[0].Tag).ExecutionMode == "Action",
                    "Stable identity search selected a different node type.");
                toolbox.RefreshLanguage(); Assert(search.Text == "flow/value", "Language refresh cleared the toolbox search.");
                Screenshot(form, "action-only-toolbox-" + language); search.Clear();
            }
            ActionLeaseProbe probe = new ActionLeaseProbe();
            catalogue.Items.Add("probe/rebind", new FlowResource { Id = "probe/rebind", Kind = "device", Label = "Rebind probe", Instance = probe, Methods = probe.GetFlowMethods().ToList() });
            FlowActions rebound = new FlowActions(catalogue);
            search.Text = "probe/rebind";
            Assert(tree.Nodes.Count == 0, "A search rebuilt the catalogue before it was rebound.");
            toolbox.BindActions(rebound);
            Assert(search.Text == "probe/rebind" && tree.Nodes.Count == 1 && tree.Nodes[0].Nodes.Count == 1
                && ((FlowToolboxEntry)tree.Nodes[0].Nodes[0].Tag).TargetId == "probe/rebind", "Rebinding did not refresh the actual catalogue or retained the wrong target.");
            SwitchLanguage(LanguageType.English); Application.DoEvents();
            Assert(search.Text == "probe/rebind" && tree.Nodes.Count == 1, "A language switch cleared the search or lost its matching tool.");
            catalogue.Items.Remove("probe/rebind");
            toolbox.BindActions(rebound);
            Assert(tree.Nodes.Count == 0 && search.Text == "probe/rebind" && probe.Calls == 0 && probe.Simulations == 0,
                "Rebinding retained a deleted target or invoked the real method while browsing.");
            search.Clear();
            FlowToolboxEntry inserted = null; toolbox.InsertRequested += value => inserted = value;
            tree.SelectedNode = tree.Nodes.Cast<TreeNode>().SelectMany(group => group.Nodes.Cast<TreeNode>()).Single(node => ((FlowToolboxEntry)node.Tag).ExecutionMode == "None");
            string selectedId = ((FlowToolboxEntry)tree.SelectedNode.Tag).Id;
            search.Text = "missing-tool-" + Guid.NewGuid().ToString("N");
            Assert(tree.Nodes.Count == 0 && ActionControl<Label>(toolbox, "toolboxEmpty").Visible, "An empty search did not show the no-results state.");
            search.Clear();
            Assert(tree.SelectedNode != null && ((FlowToolboxEntry)tree.SelectedNode.Tag).Id == selectedId,
                "Clearing a temporary filter lost the previously selected tool.");
            typeof(Control).GetMethod("OnKeyDown", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(tree, new object[] { new KeyEventArgs(Keys.Enter) });
            Assert(inserted != null && inserted.ExecutionMode == "None", "Enter did not request the selected condition node.");
        }
        SwitchLanguage(LanguageType.Chinese);
    }

    /// <summary>Create an unsupported saved-era node with source, bindings, routes and debugging metadata.</summary>
    private static FlowStep RetiredActionNode(FlowModuleEditor owner, bool code)
    {
        FlowStep step = owner.AddNode();
        step.Caption = code ? "Retired code module" : "Retired C# script";
        step.ExecutionMode = code ? "Action" : "Script";
        step.Action = code ? "code.execute" : "script.execute"; step.Target = "retired/stable-target";
        step.ScriptSource = "Outputs.Value = Inputs.Value + 7;";
        step.ScriptInputs.Add(new FlowScriptPort { Name = "Value", Type = "Number", DefaultValue = "3" });
        step.ScriptOutputs.Add(new FlowScriptPort { Name = "Value", Type = "Number" });
        step.ScriptResources.Add("retired/script-resource");
        step.Inputs = new List<FlowInput> { new FlowInput { Name = "Value", Reference = "3" } };
        step.Breakpoint = true; step.TimeoutMs = 12345; step.AlarmCode = "2019";
        if (code)
        {
            step.CodeModuleId = "retired.module"; step.CodeModuleVersion = 7;
            step.ResourceBindings.Add(new FlowResourceBinding { Slot = "Axis", Target = "retired/axis", PointTableSignature = "RecipeA=original-signature" });
        }
        return step;
    }

    /// <summary>Refuse all host execution and persistence paths while retaining complete historical node definitions.</summary>
    private static void ActionOnlyRetiredPolicy()
    {
        FlowPrograms.Save(); byte[] disk = File.ReadAllBytes(FlowPrograms.FilePath);
        foreach (bool code in new[] { false, true })
        {
            using (Flow_BaseForm form = FlowPrograms.AddModule(code ? "Retired code policy" : "Retired script policy"))
            {
                FlowModuleEditor owner = FlowPrograms.Attach(form); FlowStep old = RetiredActionNode(owner, code);
                owner.Program.Mode = "Program"; owner.Program.AutoEntry = old.Id;
                string before = ActionNodeXml(old); FlowActions actions = new FlowActions(FlowResources.Capture());
                Assert(!FlowActions.IsSupported(old) && actions.FindMethod(old) == null, "Retired mode received a live method fallback.");
                int rejected = 0;
                try { actions.Validate(old); } catch (InvalidOperationException) { rejected++; }
                try { ExecuteBound(actions, old); } catch (InvalidOperationException) { rejected++; }
                try { owner.Start(old.Id, true, false); } catch (InvalidOperationException) { rejected++; }
                try { FlowPrograms.Save(); } catch (InvalidOperationException) { rejected++; }
                Assert(rejected == 4 && !owner.Runtime.IsRunning && File.ReadAllBytes(FlowPrograms.FilePath).SequenceEqual(disk),
                    "A retired node executed, published or replaced the saved project.");
                FlowStep copy = FlowProgramFile.Snapshot(owner.Program).Steps.Single();
                Assert(ActionNodeXml(copy) == before && ActionNodeXml(old) == before, "Validation or snapshots rewrote historical XML.");
            }
        }
    }

    /// <summary>Convert retired definitions only after an explicit choice while preserving the Cancel boundary and common runtime settings.</summary>
    private static void ActionOnlyConversions()
    {
        foreach (bool code in new[] { false, true })
        {
            FlowStep old = RetiredActionNode(editor, code); string before = ActionNodeXml(old);
            try
            {
                using (FlowNodeForm dialog = new FlowNodeForm(editor, old))
                {
                    dialog.Show(); Application.DoEvents();
                    RadioButton action = ActionControl<RadioButton>(dialog, "scriptAction"), none = ActionControl<RadioButton>(dialog, "scriptNone");
                    Assert(!action.Checked && !none.Checked, "Opening a retired node silently chose its replacement.");
                    none.Checked = true; ActionControl<Button>(dialog, "cancel").PerformClick();
                }
                Assert(ActionNodeXml(old) == before, "Cancelling conversion modified the original node.");
                using (FlowNodeForm dialog = new FlowNodeForm(editor, old))
                {
                    dialog.Show(); ActionControl<RadioButton>(dialog, code ? "scriptNone" : "scriptAction").Checked = true;
                    if (!code)
                    {
                        ActionControl<ComboBox>(dialog, "categoryValue").SelectedValue = "flow";
                        ComboBox methods = ActionControl<ComboBox>(dialog, "actionValue");
                        methods.SelectedItem = ((IList<FlowMethod>)methods.DataSource).Single(item => item.Id == "value");
                    }
                    ActionControl<Button>(dialog, "ok").PerformClick();
                    Assert(dialog.DialogResult == DialogResult.OK, "Explicit conversion could not be confirmed.");
                }
                FlowStep converted = editor.Program.Steps.Single(item => item.Id == old.Id);
                Assert(converted.ExecutionMode == (code ? "None" : "Action") && string.IsNullOrEmpty(converted.CodeModuleId)
                    && converted.CodeModuleVersion == 0 && converted.ResourceBindings.Count == 0 && string.IsNullOrEmpty(converted.ScriptSource)
                    && converted.ScriptInputs.Count == 0 && converted.ScriptOutputs.Count == 0 && converted.ScriptResources.Count == 0,
                    "Conversion retained inactive source or module bindings.");
                Assert(converted.Breakpoint && converted.TimeoutMs == 12345 && converted.AlarmCode == "2019", "Conversion lost common runtime settings.");
                new FlowActions(FlowResources.Capture()).Validate(converted);
            }
            finally { editor.Delete(old.Id); }
        }
    }

    /// <summary>Deliver real drag events to a scrolled inherited form and retain the selected allowed node mode.</summary>
    private static void VerifyToolboxDrops(Flow page)
    {
        TabControl tabs = ActionControl<TabControl>(page, "tabControl1");
        tabs.SelectedTab = tabs.TabPages.Cast<TabPage>().Single(tab => tab.Text == station.Name);
        CheckBox edit = ActionControl<CheckBox>(page, "edit"); edit.Checked = true; Application.DoEvents();
        station.Canvas.AutoScroll = true; station.Canvas.AutoScrollMinSize = new Size(2400, 2400);
        station.Canvas.AutoScrollPosition = new Point(100, 300); Application.DoEvents();
        foreach (FlowToolboxEntry entry in new[] {
            new FlowToolboxEntry { Id = "flow/value", ExecutionMode = "Action", TargetId = "", ActionId = "value" },
            new FlowToolboxEntry { Id = "axis-test", ExecutionMode = "Action", TargetId = "hardware/" + groupedAxis.Id + "/axis", ActionId = "point.move" },
            new FlowToolboxEntry { Id = "None", ExecutionMode = "None" } })
        {
            int before = editor.Program.Steps.Count;
            Point client = new Point(55, 75), scroll = station.Canvas.AutoScrollPosition, screen = station.Canvas.PointToScreen(client);
            DragEventArgs args = new DragEventArgs(new DataObject(FlowToolboxControl.DragFormat, entry), 0, screen.X, screen.Y, DragDropEffects.Copy, DragDropEffects.None);
            typeof(Control).GetMethod("OnDragOver", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(station.Canvas, new object[] { args });
            Assert(args.Effect == DragDropEffects.Copy, "Canvas rejected a supported toolbox entry.");
            typeof(Control).GetMethod("OnDragDrop", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(station.Canvas, new object[] { args });
            FlowStep created = editor.Program.Steps.Last();
            try
            {
                Assert(editor.Program.Steps.Count == before + 1 && created.ParentPath == "/canvas" && created.X == client.X - scroll.X && created.Y == client.Y - scroll.Y
                    && created.ExecutionMode == entry.ExecutionMode && FlowActions.IsSupported(created), "Scrolled drop changed the mode or logical placement.");
                if (entry.ExecutionMode == "Action") Assert(created.Target == entry.TargetId && created.Action == entry.ActionId, "Drop lost the selected object's exact method identity.");
                new FlowActions(FlowResources.Capture()).Validate(created);
            }
            finally { editor.Delete(created.Id); }
        }
        edit.Checked = false;
    }

    /// <summary>Keep a stable external identity while replacing the exact invoked object and protecting references and active leases.</summary>
    private static void ActionOnlyExternalResources()
    {
        const string id = "Action registered probe", target = "external/Action%20registered%20probe";
        DesignerProbe original = new DesignerProbe(), replacement = new DesignerProbe();
        FlowExternalResources.Register(id, "Registered probe", "device", original);
        FlowStep step = editor.AddNode(); step.Target = target; step.Action = "probe.read";
        step.Inputs = new List<FlowInput> { new FlowInput { Name = "Increment", Reference = "2" } };
        try
        {
            Assert((int)ExecuteBound(new FlowActions(FlowResources.Capture()), step)["Count"] == 2 && original.Calls == 2, "Action did not invoke the registered object.");
            bool blocked = false; try { FlowExternalResources.Unregister(id); } catch (InvalidOperationException) { blocked = true; }
            Assert(blocked, "A referenced external object was unregistered.");
            using (FlowResourceGate.Lease lease = FlowResourceGate.Acquire(new object[] { original }, CancellationToken.None).GetAwaiter().GetResult())
            {
                blocked = false; try { FlowExternalResources.Replace(id, replacement); } catch (InvalidOperationException) { blocked = true; }
                Assert(blocked, "A leased external object was replaced.");
            }
            FlowExternalResources.Replace(id, replacement);
            Assert((int)ExecuteBound(new FlowActions(FlowResources.Capture()), step)["Count"] == 2 && original.Calls == 2 && replacement.Calls == 2,
                "The stable resource identity retained the replaced object.");
        }
        finally { editor.Delete(step.Id); FlowExternalResources.Unregister(id); }
    }

    /// <summary>Build an isolated resource catalogue whose object call remains blocked until explicitly released.</summary>
    private static FlowActions ProbeActions(ActionLeaseProbe probe)
    {
        FlowResources resources = new FlowResources();
        resources.Items.Add("probe/action", new FlowResource { Id = "probe/action", Kind = "device", Label = "Action lease probe", Instance = probe, Methods = probe.GetFlowMethods().ToList() });
        return new FlowActions(resources);
    }

    /// <summary>Keep actual object ownership until cancellation has drained a non-cooperative call, and discard all delayed outputs.</summary>
    private static void ActionOnlyCallOwnership()
    {
        foreach (bool timeout in new[] { false, true })
        {
            ActionLeaseProbe probe = new ActionLeaseProbe(); FlowActions actions = ProbeActions(probe);
            FlowStep step = BoundStep(actions.FindResource("probe/action"), "probe.wait"); step.TimeoutMs = timeout ? 40 : 2000; step.AlarmCode = "2019";
            FlowResultHub hub = new FlowResultHub();
            FlowExecutionContext context = new FlowExecutionContext { Hub = hub, RunId = hub.Epoch, CycleId = "ownership", ModuleId = "ActionLease", Parameters = actions.Resources.Parameters };
            FlowProgramRuntime runtime = new FlowProgramRuntime { ExecuteAction = actions.Execute }; int alarms = 0;
            runtime.AlarmRequested += code => Interlocked.Increment(ref alarms);
            runtime.Start(new FlowProgram { ModuleId = context.ModuleId, Steps = new List<FlowStep> { step } }, step.Id, context);
            try
            {
                ActionUntil(() => probe.Started.Task.IsCompleted, "The real Action call never started.");
                if (timeout) ActionUntil(() => runtime.AlarmWasRaised, "Timeout did not raise its configured alarm.");
                else runtime.Stop();
                Assert(runtime.IsRunning && !runtime.Completion.IsCompleted && !FlowResourceGate.CanAccess(probe) && probe.OwnedDuringCall,
                    "Cancellation released the actual resource before its call returned.");
                probe.Release.TrySetResult(true); Wait(runtime.Completion);
                bool missing = false;
                try { hub.Read(context.RunId, context.CycleId, context.ModuleId + "/" + step.Id, "Count"); } catch (FlowOutputUnavailableException) { missing = true; }
                Assert(probe.Calls == 1 && FlowResourceGate.CanAccess(probe) && missing && (timeout ? alarms == 1 && runtime.Error.Contains("Timeout") : runtime.Error == "Cancelled"),
                    "Delayed Action output escaped cancellation, ownership leaked or an alarm was duplicated.");
            }
            finally { probe.Release.TrySetResult(true); runtime.Stop(); Wait(runtime.Completion); }
        }
    }

    /// <summary>Require hardware-shaped Actions to dispatch their own simulated method without invoking the real object.</summary>
    private static void ActionOnlySimulation()
    {
        ActionLeaseProbe probe = new ActionLeaseProbe(); FlowActions actions = ProbeActions(probe); actions.Simulate = true;
        IDictionary<string, object> result = ExecuteBound(actions, BoundStep(actions.FindResource("probe/action"), "probe.wait"));
        Assert((long)result["Count"] == 900L && probe.Calls == 0 && probe.Simulations == 1 && FlowResourceGate.CanAccess(probe),
            "Simulation invoked the real hardware-shaped method or leaked its lease.");
    }
}
