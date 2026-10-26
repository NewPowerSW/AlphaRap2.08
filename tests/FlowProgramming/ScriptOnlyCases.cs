using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using AlphaRap;
using AlphaRap.Configuration;
using AlphaRap.FlowProgramming;
using NPSDK;
using NPSDK.Motion;

internal static partial class Cases
{
    /// <summary>Register supported node creation, migration and actual resource access regressions.</summary>
    private static void RegisterScriptOnlyCases()
    {
        Test("toolbox exposes only searchable script and condition nodes in all three languages", ScriptOnlyToolbox);
        Test("retired Action and Code nodes reject execution and save without losing their definitions", ScriptOnlyRetiredPolicy);
        Test("retired node conversion is explicit cancellable and clears inactive module bindings", ScriptOnlyConversions);
        Test("scripts reuse the page-bound axis for point moves homing and cancellation", ScriptOnlyAxisMethods);
        Test("scripts call real VPP terminals and retain the camera run identity", ScriptOnlyVisionMethods);
        Test("scripts write actual constant tables with typed values and preserve save and discard boundaries", ScriptOnlyConstantWrites);
        Test("external instance registration replacement and deletion preserve controlled script access", ScriptOnlyExternalResources);
    }

    /// <summary>Check default node creation and explicitly retire the imported callback in this valid test project.</summary>
    private static void ScriptOnlyDefaults()
    {
        FlowStep imported = editor.Program.Steps.Single();
        Assert(imported.ExecutionMode == "Action" && !FlowActions.IsSupported(imported), "Import silently adopted a replacement execution mode.");
        FlowStep created = editor.AddNode();
        try
        {
            Assert(created.ExecutionMode == "Script" && created.Action == "script.execute" && created.ScriptSource == "Outputs.Value = Inputs.Value;"
                && created.ScriptInputs.Count == 1 && created.ScriptOutputs.Count == 1 && created.Inputs.Single().Reference == "0",
                "New node defaults are not a complete, independently executable script.");
            Assert((double)ScriptExecute(new FlowActions(FlowResources.Capture()), created)["Value"] == 0, "Default script failed real execution.");
            int before = editor.Program.Steps.Count;
            bool rejected = false;
            try { editor.AddNode("Action", station.Canvas, new Point(50, 50)); } catch (InvalidOperationException) { rejected = true; }
            Assert(rejected && editor.Program.Steps.Count == before, "A retired toolbox payload created an Action node.");
        }
        finally { editor.Delete(created.Id); }
        imported.ExecutionMode = "None"; imported.Action = "flow.none"; imported.Target = "";
        imported.Inputs.Clear(); imported.Branches = FlowBranch.Default();
    }

    /// <summary>Read complete XML without invoking host migration or changing the supplied node.</summary>
    private static string ScriptNodeXml(FlowStep step)
    {
        using (StringWriter text = new StringWriter())
        {
            new XmlSerializer(typeof(FlowStep)).Serialize(text, step);
            return text.ToString();
        }
    }

    /// <summary>Build a saved-era definition whose unsupported contract must stay visible until explicit conversion.</summary>
    private static FlowStep ScriptRetiredNode(FlowModuleEditor owner, bool code)
    {
        FlowStep step = owner.AddNode();
        step.Caption = code ? "Retired code module" : "Retired configured action";
        step.ExecutionMode = "Action"; step.Action = code ? "code.execute" : "point.move"; step.Target = "retired/stable-target";
        step.ScriptSource = ""; step.ScriptInputs.Clear(); step.ScriptOutputs.Clear(); step.ScriptResources.Clear();
        step.Inputs = new List<FlowInput> { new FlowInput { Name = "Index", Reference = "3" } };
        if (code)
        {
            step.CodeModuleId = "retired.module"; step.CodeModuleVersion = 7;
            step.ResourceBindings.Add(new FlowResourceBinding { Slot = "Axis", Target = "retired/axis", PointTableSignature = "RecipeA=original-signature" });
        }
        return step;
    }

    /// <summary>Preserve XML and published files while every host execution path rejects an unsupported node.</summary>
    private static void ScriptOnlyRetiredPolicy()
    {
        FlowPrograms.Save(); byte[] disk = File.ReadAllBytes(FlowPrograms.FilePath);
        foreach (bool code in new[] { false, true })
        {
            using (Flow_BaseForm form = FlowPrograms.AddModule(code ? "Retired code policy" : "Retired action policy"))
            {
                FlowModuleEditor owner = FlowPrograms.Attach(form); FlowStep old = ScriptRetiredNode(owner, code);
                owner.Program.Mode = "Program"; owner.Program.AutoEntry = old.Id;
                string before = ScriptNodeXml(old);
                FlowActions actions = new FlowActions(FlowResources.Capture());
                Assert(!FlowActions.IsSupported(old) && actions.FindMethod(old) == null, "Unsupported mode acquired a live method fallback.");
                int rejected = 0;
                try { actions.Validate(old); } catch (InvalidOperationException) { rejected++; }
                try { ScriptExecute(actions, old); } catch (InvalidOperationException) { rejected++; }
                try { owner.Start(old.Id, true, false); } catch (InvalidOperationException) { rejected++; }
                try { FlowPrograms.Save(); } catch (InvalidOperationException) { rejected++; }
                Assert(rejected == 4 && !owner.Runtime.IsRunning && File.ReadAllBytes(FlowPrograms.FilePath).SequenceEqual(disk),
                    "A retired node executed, published or replaced the previously saved project.");
                FlowStep copy = FlowProgramFile.Snapshot(owner.Program).Steps.Single();
                Assert(ScriptNodeXml(copy) == before && ScriptNodeXml(old) == before, "Validation or snapshots discarded the unsupported definition.");
                Assert(!code || copy.CodeModuleVersion == 7 && copy.ResourceBindings.Single().PointTableSignature == "RecipeA=original-signature",
                    "An old module identity or binding signature was lost.");
            }
        }
    }

    /// <summary>Require choosing Script or None while Cancel preserves every original action and code binding.</summary>
    private static void ScriptOnlyConversions()
    {
        foreach (bool code in new[] { false, true })
        {
            FlowStep old = ScriptRetiredNode(editor, code); string before = ScriptNodeXml(old);
            try
            {
                using (FlowNodeForm dialog = new FlowNodeForm(editor, old))
                {
                    dialog.Show(); Application.DoEvents();
                    Assert(!ScriptControl<RadioButton>(dialog, "scriptCustom").Checked && !ScriptControl<RadioButton>(dialog, "scriptNone").Checked,
                        "Opening an old definition silently selected its replacement.");
                    ScriptMode(dialog, "Script");
                    ScriptControl<RichTextBox>(dialog, "scriptSource").Text = "Outputs.Value = 19;";
                    ScriptControl<Button>(dialog, "cancel").PerformClick();
                }
                Assert(ScriptNodeXml(old) == before, "Cancelling conversion modified the original XML.");
                using (FlowNodeForm dialog = new FlowNodeForm(editor, old))
                {
                    dialog.Show(); ScriptMode(dialog, code ? "None" : "Script");
                    ScriptControl<Button>(dialog, "ok").PerformClick();
                    Assert(dialog.DialogResult == DialogResult.OK, "Explicit conversion could not be confirmed.");
                }
                FlowStep converted = editor.Program.Steps.Single(item => item.Id == old.Id);
                Assert(converted.ExecutionMode == (code ? "None" : "Script") && string.IsNullOrEmpty(converted.CodeModuleId)
                    && converted.CodeModuleVersion == 0 && converted.ResourceBindings.Count == 0 && converted.Target == "",
                    "Conversion retained an inactive object or module binding.");
                IDictionary<string, object> result = ScriptExecute(new FlowActions(FlowResources.Capture()), converted);
                Assert(code ? result.Count == 0 : (double)result["Value"] == 0, "Conversion reused a retired action parameter or executed module code.");
            }
            finally { editor.Delete(old.Id); }
        }
    }

    /// <summary>Keep only two stable toolbox entries and preserve a query while translating their visible titles.</summary>
    private static void ScriptOnlyToolbox()
    {
        using (Form form = new Form { Width = 430, Height = 450 })
        using (FlowToolboxControl toolbox = new FlowToolboxControl { Dock = DockStyle.Fill })
        {
            form.Controls.Add(toolbox); form.Show(); Application.DoEvents();
            TreeView tree = ScriptControl<TreeView>(toolbox, "toolboxEntries");
            TextBox search = ScriptControl<TextBox>(toolbox, "toolboxSearch");
            string[] chinese = null;
            foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
            {
                SwitchLanguage(language); Application.DoEvents();
                FlowToolboxEntry[] values = tree.Nodes.Cast<TreeNode>().SelectMany(group => group.Nodes.Cast<TreeNode>()).Select(node => (FlowToolboxEntry)node.Tag).ToArray();
                Assert(values.Length == 2 && values.Select(item => item.ExecutionMode).OrderBy(value => value).SequenceEqual(new[] { "None", "Script" })
                    && values.All(item => item.Id == item.ExecutionMode), "The toolbox still discovers retired actions or modules.");
                string[] titles = values.OrderBy(item => item.Id).Select(item => item.Title).ToArray();
                if (chinese == null) chinese = titles;
                else Assert(!titles.SequenceEqual(chinese), "Toolbox entries did not translate.");
                search.Text = "Script"; Application.DoEvents();
                Assert(tree.Nodes.Count == 1 && tree.Nodes[0].Nodes.Count == 1 && ((FlowToolboxEntry)tree.Nodes[0].Nodes[0].Tag).ExecutionMode == "Script",
                    "Stable identity search selected a different node type.");
                toolbox.RefreshLanguage(); Assert(search.Text == "Script", "Refreshing language cleared the toolbox search draft.");
                Screenshot(form, "script-only-toolbox-" + language);
                search.Clear();
            }
            FlowToolboxEntry inserted = null; toolbox.InsertRequested += value => inserted = value;
            tree.SelectedNode = tree.Nodes[0].Nodes.Cast<TreeNode>().Single(node => ((FlowToolboxEntry)node.Tag).ExecutionMode == "None");
            typeof(Control).GetMethod("OnKeyDown", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(tree, new object[] { new KeyEventArgs(Keys.Enter) });
            Assert(inserted != null && inserted.ExecutionMode == "None", "Enter did not request the selected condition node.");
        }
        SwitchLanguage(LanguageType.Chinese);
    }

    /// <summary>Deliver real drag events to a scrolled flow canvas and require the requested supported execution mode.</summary>
    private static void VerifyToolboxDrops(Flow page)
    {
        TabControl tabs = ScriptControl<TabControl>(page, "tabControl1");
        tabs.SelectedTab = tabs.TabPages.Cast<TabPage>().Single(tab => tab.Text == station.Name);
        CheckBox edit = ScriptControl<CheckBox>(page, "edit"); edit.Checked = true; Application.DoEvents();
        station.Canvas.AutoScroll = true; station.Canvas.AutoScrollMinSize = new Size(2400, 2400);
        station.Canvas.AutoScrollPosition = new Point(100, 300); Application.DoEvents();
        foreach (string mode in new[] { "Script", "None" })
        {
            int before = editor.Program.Steps.Count;
            Point client = new Point(55, 75), scroll = station.Canvas.AutoScrollPosition, screen = station.Canvas.PointToScreen(client);
            FlowToolboxEntry entry = new FlowToolboxEntry { Id = mode, ExecutionMode = mode, Category = "flow", Title = mode };
            DragEventArgs args = new DragEventArgs(new DataObject(FlowToolboxControl.DragFormat, entry), 0, screen.X, screen.Y, DragDropEffects.Copy, DragDropEffects.None);
            typeof(Control).GetMethod("OnDragOver", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(station.Canvas, new object[] { args });
            Assert(args.Effect == DragDropEffects.Copy, "The editing canvas rejected a supported toolbox entry.");
            typeof(Control).GetMethod("OnDragDrop", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(station.Canvas, new object[] { args });
            FlowStep created = editor.Program.Steps.Last();
            try
            {
                Assert(editor.Program.Steps.Count == before + 1 && created.ParentPath == "/canvas" && created.X == client.X - scroll.X && created.Y == client.Y - scroll.Y
                    && created.ExecutionMode == mode && FlowActions.IsSupported(created), "A scrolled drop changed node type or placement.");
                new FlowActions(FlowResources.Capture()).Validate(created);
            }
            finally { editor.Delete(created.Id); }
        }
        edit.Checked = false;
    }

    /// <summary>Run point motion and home through scripts while keeping the original fake driver and actual axis lease.</summary>
    private static void ScriptOnlyAxisMethods()
    {
        NP_Motor motor = (NP_Motor)hardware.Resources.Entries.Single(item => item.Definition.Id == groupedAxis.Id).Device;
        FieldInfo field = typeof(NP_Motor).GetField("dMotor", BindingFlags.Instance | BindingFlags.NonPublic), ready = typeof(NP_Motor).GetField("ready", BindingFlags.Instance | BindingFlags.NonPublic);
        object previous = field.GetValue(motor), wasReady = ready.GetValue(motor); PointDriver driver = new PointDriver();
        field.SetValue(motor, driver); ready.SetValue(motor, true);
        string target = "hardware/" + groupedAxis.Id + "/axis";
        FlowStep script = ScriptNode("var moved = await Context.CallAsync(\"" + target + "\", \"point.move\", new Dictionary<string, object> { { \"Index\", Inputs.Index }, { \"Offset\", 0.5 }, { \"Tolerance\", 0.2 } });\nOutputs.Position = Context.Get<double>(moved, \"Position\");", ScriptPort("Position", "Number"));
        script.ScriptResources.Add(target); script.ScriptInputs.Add(ScriptPort("Index", "Integer", "0")); script.Inputs.Add(new FlowInput { Name = "Index", Reference = "0" });
        try
        {
            FlowActions actions = new FlowActions(FlowResources.Capture());
            FlowResource axis = actions.FindResource(target);
            Assert(axis.BoundPoints != null && ReferenceEquals(((FlowPointAxis)axis.BoundPoints.Instance).Motor, motor), "The script axis lost its own bound point table.");
            double expected = ((FlowPointAxis)axis.BoundPoints.Instance).Positions[0] + 0.5;
            Assert((double)ScriptExecute(actions, script)["Position"] == expected && driver.Moves == 1, "Goto false was treated as failure, resent or resolved another table.");
            script.Inputs[0].Reference = "99999"; bool rejected = false;
            try { ScriptExecute(actions, script); } catch (InvalidOperationException) { rejected = true; }
            Assert(rejected && driver.Moves == 1, "A script moved to a missing point.");
            FlowStep home = ScriptNode("await Context.CallAsync(\"" + target + "\", \"axis.home\", new Dictionary<string, object>());"); home.ScriptResources.Add(target);
            ScriptExecute(actions, home); Assert(driver.HomePolls == 2, "A script skipped the driver homing state machine.");
            driver.Hold = true; script.Inputs[0].Reference = "0";
            using (CancellationTokenSource stop = new CancellationTokenSource())
            {
                Task<IDictionary<string, object>> pending = actions.Execute(script, ScriptContext(actions), stop.Token);
                ScriptUntil(() => driver.Moves == 2 && !FlowResourceGate.CanAccess(motor), "Script motion never obtained the actual axis lease.");
                stop.Cancel(); bool cancelled = false;
                try { Wait(pending); } catch (OperationCanceledException) { cancelled = true; }
                Assert(cancelled && driver.Stops == 1 && !driver.Moving && FlowResourceGate.CanAccess(motor), "Cancelled script released an axis before stopping it.");
            }
        }
        finally { field.SetValue(motor, previous); ready.SetValue(motor, wasReady); }
    }

    /// <summary>Call one actual in-memory VPP and read its typed output without executing another camera or touching acquisition hardware.</summary>
    private static void ScriptOnlyVisionMethods()
    {
        FlowVisionProbe vision = new FlowVisionProbe { Accept = false };
        using (CapabilityHost form = new CapabilityHost { Name = "Script VPP probe", Bound = vision })
        {
            RegisterForm(form);
            try
            {
                FlowActions actions = new FlowActions(FlowResources.Capture());
                string target = actions.Resources.Items.Values.Single(item => ReferenceEquals(item.Instance, vision)).Id;
                FlowStep script = ScriptNode("var captured = await Context.CallAsync(\"" + target + "\", \"vision.run\", new Dictionary<string, object> { { \"TB:Measure\", 3.5 } });\nOutputs.Score = Context.Get<double>(captured, \"TB:Score\");\nOutputs.Accepted = Context.Get<bool>(captured, \"Accepted\");\nOutputs.RunId = Context.Get<long>(captured, \"RunId\");",
                    ScriptPort("Score", "Number"), ScriptPort("Accepted", "Boolean"), ScriptPort("RunId", "Integer"));
                script.ScriptResources.Add(target);
                IDictionary<string, object> result = ScriptExecute(actions, script);
                Assert((double)result["Score"] == 7 && !(bool)result["Accepted"] && (long)result["RunId"] > 0 && vision.Runs == 1 && vision.LastRunResult.IsFlowManaged,
                    "Script VPP dispatch lost its actual terminals, NG judgement or run identity.");
            }
            finally { ((System.Collections.IList)host.GetField("lstForm").GetValue(null)).Remove(form); VisionproInterface.VList.Remove(vision); }
        }
    }

    /// <summary>Update a real Setting value through the controlled script broker and reject invalid integer writes.</summary>
    private static void ScriptOnlyConstantWrites()
    {
        ConstantStore store = constantForm.Constants; const string key = "Script writable constant";
        store.Put("Setting", null, new ConstantValue { Key = key, Type = "Integer", Value = "2" }); store.Save("Setting");
        FlowStep script = ScriptNode("await Context.CallAsync(\"constants/Setting\", \"constant.set\", new Dictionary<string, object> { { \"Key\", Inputs.Key }, { \"Value\", Inputs.Value } });");
        script.ScriptResources.Add("constants/Setting");
        script.ScriptInputs.AddRange(new[] { ScriptPort("Key", "Text", key), ScriptPort("Value", "Text", "7") });
        script.Inputs = script.ScriptInputs.Select(port => new FlowInput { Name = port.Name, Reference = port.DefaultValue }).ToList();
        try
        {
            FlowActions actions = new FlowActions(FlowResources.Capture());
            ScriptExecute(actions, script);
            DataRow row = ConstantStore.Find(store.Table("Setting"), key);
            Assert(Convert.ToString(row["Value"]) == "7" && Convert.ToString(row["Value", DataRowVersion.Original]) == "2" && row.RowState == DataRowState.Modified,
                "Script constant setter did not preserve the Main Save boundary.");
            script.Inputs[1].Reference = "1.5"; bool rejected = false;
            try { ScriptExecute(actions, script); } catch (InvalidOperationException) { rejected = true; }
            Assert(rejected && Convert.ToString(row["Value"]) == "7", "An invalid integer write rounded or changed the table.");
            store.Table("Setting").RejectChanges();
            Assert(Convert.ToString(ConstantStore.Find(store.Table("Setting"), key)["Value"]) == "2", "Discard retained an unsaved script value.");
        }
        finally { store.Table("Setting").RejectChanges(); store.Delete("Setting", key); store.Save("Setting"); }
    }

    /// <summary>Keep externally owned object identities available to scripts without permitting replacement or deletion during a lease or reference.</summary>
    private static void ScriptOnlyExternalResources()
    {
        string id = "script/external/" + Guid.NewGuid().ToString("N"), target = "external/" + Uri.EscapeDataString(id);
        DesignerProbe original = new DesignerProbe(), replacement = new DesignerProbe();
        FlowExternalResources.Register(id, "External probe", "device", original);
        FlowStep script = editor.AddNode(); script.ScriptInputs.Clear(); script.Inputs.Clear();
        script.ScriptOutputs = new List<FlowScriptPort> { ScriptPort("Value", "Integer") };
        script.ScriptSource = "var read = await Context.CallAsync(\"" + target + "\", \"probe.read\", new Dictionary<string, object> { { \"Increment\", 2 } });\nOutputs.Value = Context.Get<int>(read, \"Count\");";
        script.ScriptResources.Add(target);
        try
        {
            Assert((long)ScriptExecute(new FlowActions(FlowResources.Capture()), script)["Value"] == 2 && original.Calls == 2, "Script did not invoke the registered original instance.");
            bool rejected = false; try { FlowExternalResources.Unregister(id); } catch (InvalidOperationException) { rejected = true; }
            Assert(rejected, "A referenced external resource was unregistered.");
            using (FlowResourceGate.Lease lease = FlowResourceGate.Acquire(new object[] { original }, CancellationToken.None).GetAwaiter().GetResult())
            {
                rejected = false; try { FlowExternalResources.Replace(id, replacement); } catch (InvalidOperationException) { rejected = true; }
                Assert(rejected, "A leased external resource was replaced.");
            }
            FlowExternalResources.Replace(id, replacement);
            Assert((long)ScriptExecute(new FlowActions(FlowResources.Capture()), script)["Value"] == 2 && original.Calls == 2 && replacement.Calls == 2,
                "A new run reused the old external instance after explicit replacement.");
        }
        finally { editor.Delete(script.Id); FlowExternalResources.Unregister(id); }
        Assert(new FlowActions(FlowResources.Capture()).FindResource(target) == null, "External unregister left a selectable stale resource.");
    }
}
