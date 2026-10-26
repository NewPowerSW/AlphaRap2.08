using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using AlphaRap;
using AlphaRap.Configuration;
using AlphaRap.FlowProgramming;
using NPSDK;

internal static partial class Cases
{
    /// <summary>Register actionable validation and source-picker recovery checks for new application engineers.</summary>
    public static void RegisterP0EntryRepairs()
    {
        Test("P0 typed conditions reject empty numeric and boolean comparisons and ignore hidden Empty operands", P0RequiredConditionValues);
        Test("P0 constrained source pickers explain missing producer nodes and retain invalid bindings", P0ProducerEmptyRecovery);
    }

    /// <summary>Reject blank comparisons against known result types without changing valid text-empty or unary Empty semantics.</summary>
    private static void P0RequiredConditionValues()
    {
        FlowMethod method = new FlowMethod("p0.condition.values", "P0 condition outputs", "P0 condition outputs", "P0 condition outputs") { UsesHardware = false };
        FlowField[] fields = {
            new FlowField("Ready", typeof(bool)) { DisplayName = new[] { "P0 readiness", "P0 readiness", "P0 readiness" } },
            new FlowField("Count", typeof(long)) { DisplayName = new[] { "P0 count", "P0 count", "P0 count" } },
            new FlowField("Distance", typeof(double)) { DisplayName = new[] { "P0 distance", "P0 distance", "P0 distance" } },
            new FlowField("Text", typeof(string)) { DisplayName = new[] { "P0 text", "P0 text", "P0 text" } }
        };
        method.Outputs.AddRange(fields);
        method.Invoke = call => Task.FromResult<IDictionary<string, object>>(new Dictionary<string, object>());
        FlowActions actions = new FlowActions(FlowResources.Capture(true));
        FlowResource resource = new FlowResource { Id = "p0/condition-values", Label = "P0 condition values", Kind = "device", Instance = new object(), Methods = new List<FlowMethod> { method } };
        actions.Resources.Items.Add(resource.Id, resource);
        FlowStep step = BoundStep(resource, method.Id);
        step.Branches = new List<FlowBranch> {
            new FlowBranch { Code = "NEXT", Conditions = new List<FlowCondition> { new FlowCondition { Left = new FlowInput { Source = "Result", Reference = "Ready" }, Right = new FlowInput { Reference = "true" } } } },
            new FlowBranch { Code = "CASE1" }
        };
        try
        {
            foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
            {
                SwitchLanguage(language);
                foreach (FlowField field in fields.Take(3))
                    P0CheckRequiredCondition(actions, step, field);
            }
            step.Branches[1].Conditions = new List<FlowCondition> { new FlowCondition { Left = new FlowInput { Source = "Result", Reference = "Text" }, Right = new FlowInput { Reference = "" } } };
            actions.Validate(step);
            foreach (string hiddenSource in new[] { "Output", "Parameter" })
            {
                FlowStep empty = new FlowStep { Caption = "P0 unary Empty", ExecutionMode = "None", TimeoutMs = 1000,
                    Branches = new List<FlowBranch> { new FlowBranch { Conditions = new List<FlowCondition> { new FlowCondition {
                        Left = new FlowInput { Reference = "" }, Operator = "Empty", Right = new FlowInput { Source = hiddenSource, Reference = "p0/removed/hidden-value" } } } } } };
                actions.Validate(empty);
                FlowResultHub hub = new FlowResultHub();
                FlowExecutionContext context = new FlowExecutionContext { Hub = hub, RunId = hub.Epoch, CycleId = "P0-empty-product", ModuleId = "P0EmptyCondition", Parameters = actions.Resources.Parameters };
                FlowProgramRuntime runtime = new FlowProgramRuntime { ExecuteAction = actions.Execute };
                runtime.Start(new FlowProgram { ModuleId = context.ModuleId, Steps = new List<FlowStep> { empty } }, empty.Id, context);
                Wait(runtime.Completion);
                Assert(runtime.Error == null && !runtime.AlarmWasRaised, "A hidden stale " + hiddenSource + " operand changed unary Empty execution: " + runtime.Error);
            }
        }
        finally { SwitchLanguage(LanguageType.Chinese); }
    }

    /// <summary>Check both operand directions and confirm the error identifies the real branch and business field.</summary>
    private static void P0CheckRequiredCondition(FlowActions actions, FlowStep step, FlowField field)
    {
        foreach (bool reverse in new[] { false, true })
        {
            FlowInput result = new FlowInput { Source = "Result", Reference = field.Name };
            FlowInput blank = new FlowInput { Reference = " \t " };
            step.Branches[1].Conditions = new List<FlowCondition> { new FlowCondition { Left = reverse ? blank : result, Right = reverse ? result : blank } };
            string message = null;
            try { actions.Validate(step); } catch (InvalidOperationException error) { message = error.Message; }
            Assert(message == string.Format(FlowText.T("conditionValueRequired"), "CASE1", field.DisplayName[0]), "An empty typed comparison was accepted or omitted its branch and business field.");
            blank.Reference = field.Type == typeof(bool) ? "false" : "0";
            actions.Validate(step);
        }
    }

    /// <summary>Show a concrete producer-creation instruction when only another VPP or a missing old reference exists.</summary>
    private static void P0ProducerEmptyRecovery()
    {
        FlowMethod capture = new FlowMethod("vision.run", "P0 capture", "P0 capture", "P0 capture") { UsesHardware = false };
        capture.Outputs.Add(new FlowField("RunId", typeof(long)) { Advanced = true });
        capture.Invoke = call => Task.FromResult(FlowCall.Result("RunId", 1L));
        FlowMethod consumer = new FlowMethod("vision.output", "P0 read capture", "P0 read capture", "P0 read capture") { UsesHardware = false };
        FlowField producer = new FlowField("RunId", typeof(long)) { OutputSourceMethod = "vision.run", OutputSourceField = "RunId" };
        consumer.Inputs.Add(producer);
        consumer.Invoke = call => Task.FromResult<IDictionary<string, object>>(new Dictionary<string, object>());
        FlowActions actions = new FlowActions(FlowResources.Capture(true));
        FlowResource cameraA = new FlowResource { Id = "vision/p0-empty/a", Label = "P0 camera A / VPP A", Kind = "vision", Instance = new object(), Methods = new List<FlowMethod> { capture, consumer } };
        FlowResource cameraB = new FlowResource { Id = "vision/p0-empty/b", Label = "P0 camera B / VPP B", Kind = "vision", Instance = new object(), Methods = new List<FlowMethod> { capture, consumer } };
        actions.Resources.Items.Add(cameraA.Id, cameraA); actions.Resources.Items.Add(cameraB.Id, cameraB);
        FlowStep other = editor.AddNode(), matching = null;
        other.Target = cameraB.Id; other.Action = capture.Id; other.Caption = "P0 another VPP capture";
        try
        {
            foreach (string retained in new[] { "", "p0/removed/capture/RunId" })
            {
                FlowInput original = new FlowInput { Name = "RunId", Source = "Output", Reference = retained };
                using (FlowSourceForm picker = new FlowSourceForm(original, actions, consumer, producer, null, cameraA.Id))
                {
                    picker.Show(); Application.DoEvents();
                    TreeView tree = (TreeView)picker.Controls.Find("sources", true)[0];
                    Button ok = (Button)picker.Controls.Find("ok", true)[0];
                    TextBox value = (TextBox)picker.Controls.Find("referenceValue", true)[0];
                    Control status = picker.Controls.Find("sourceStatus_dyn", true)[0];
                    foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
                    {
                        SwitchLanguage(language); Application.DoEvents();
                        Assert(!ok.Enabled && value.ReadOnly && status.Text == FlowText.T("producerEmptyHelp"), "An empty constrained picker asked for a fixed value or accepted an unavailable producer.");
                        Assert(picker.Controls.Find("sourceContext_dyn", true)[0].Text == FlowText.T("producerEmptyHelp"), "The producer empty-state header does not explain how to add a matching node.");
                        Assert(!tree.Nodes.Cast<TreeNode>().Any(node => node.Name == "bindingOutputs" || node.Name == "bindingFixed"), "Another VPP or a fixed value was offered as a capture source.");
                        Assert(picker.Selection.Reference == retained && original.Reference == retained, "Translation replaced the missing producer reference.");
                        if (retained.Length == 0) Screenshot(picker, "p0-producer-empty-" + language);
                    }
                    typeof(FlowSourceForm).GetMethod("AcceptReference", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(picker, null);
                    Assert(picker.DialogResult != DialogResult.OK && picker.Selection.Reference == retained && status.Text == FlowText.T("producerEmptyHelp"), "Confirming an unavailable producer accepted or rewrote its original reference.");
                }
            }
            matching = editor.AddNode(); matching.Target = cameraA.Id; matching.Action = capture.Id; matching.Caption = "P0 matching VPP capture";
            using (FlowSourceForm picker = new FlowSourceForm(new FlowInput { Name = "RunId", Source = "Output", Reference = "" }, actions, consumer, producer, null, cameraA.Id))
            {
                picker.Show(); Application.DoEvents();
                Button ok = (Button)picker.Controls.Find("ok", true)[0];
                Assert(!ok.Enabled && picker.Controls.Find("sourceStatus_dyn", true)[0].Text == FlowText.T("producerChooseRequired"), "The picker suggested a fixed value instead of selecting an available matching producer.");
                string reference = editor.Program.ModuleId + "/" + matching.Id + "/RunId";
                TreeView tree = (TreeView)picker.Controls.Find("sources", true)[0];
                tree.SelectedNode = P0SourceNodes(tree.Nodes).Single(node => node.Name == "Output:" + reference);
                Assert(ok.Enabled, "Selecting a real matching producer did not enable confirmation.");
                ok.PerformClick();
                Assert(picker.DialogResult == DialogResult.OK && picker.Selection.Source == "Output" && picker.Selection.Reference == reference, "The empty-source recovery did not retain the selected producer identity.");
            }
        }
        finally
        {
            if (matching != null) editor.Delete(matching.Id);
            editor.Delete(other.Id);
            SwitchLanguage(LanguageType.Chinese);
        }
    }

    /// <summary>Export declared field translations without constructing connections or invoking any equipment action.</summary>
    private static void ExportP0FieldTexts()
    {
        FlowActions catalog = new FlowActions(FlowResources.Capture(true));
        List<FlowMethod> methods = catalog.Resources.Items.Values.SelectMany(resource => resource.Methods).ToList();
        methods.AddRange(new FlowTray { Rows = 1, Columns = 1 }.GetFlowMethods(true));
        FlowVisionProbe vision = new FlowVisionProbe();
        try
        {
            methods.AddRange(vision.GetFlowMethods(true));
            foreach (string name in new[] { "AlphaRap.ScannerKeyenceTcp", "AlphaRap.Keyence3DTcp", "AlphaRap.HIOKI_RM3545", "AlaphaRap.ATEQ_F620", "AlaphaRap.ModBus_RTU", "AlphaRap.PLC.INOVANCE" })
            {
                Type type = typeof(SysPara).Assembly.GetType(name, true);
                object instance = FormatterServices.GetUninitializedObject(type);
                if (name == "AlphaRap.HIOKI_RM3545") type.GetProperty("ChannelNum").SetValue(instance, 7, null);
                methods.AddRange(((IFlowObject)instance).GetFlowMethods(true));
            }
            Dictionary<string, string[]> defaults = new Dictionary<string, string[]>();
            Action<string, string[]> add = (key, words) => {
                if (words == null) return;
                if (words.Length != 3 || words.Any(string.IsNullOrWhiteSpace)) throw new InvalidOperationException("Incomplete field translation: " + key);
                if (words[0] == words[1] && words[1] == words[2]) return;
                string[] existing;
                if (defaults.TryGetValue(key, out existing) && !existing.SequenceEqual(words)) throw new InvalidOperationException("Conflicting field translation: " + key);
                defaults[key] = words;
            };
            foreach (FlowMethod method in methods)
            {
                foreach (FlowField input in method.Inputs)
                {
                    add("field." + method.Id + ".input." + input.Name, input.DisplayName);
                    add("fieldHelp." + method.Id + "." + input.Name, input.Help);
                }
                foreach (FlowField output in method.Outputs) add("field." + method.Id + ".output." + output.Name, output.DisplayName);
            }
            XElement document = new XElement("EquipmentFlow", defaults.OrderBy(item => item.Key).Select(item => new XElement(item.Key,
                new XAttribute("zh", item.Value[0]), new XAttribute("en", item.Value[1]), new XAttribute("es", item.Value[2]))));
            document.Save(Path.Combine(root, "flow-field-defaults.xml"));
            Console.WriteLine("Exported field translations: " + defaults.Count);
        }
        finally { VisionproInterface.VList.Remove(vision); vision.TB.Dispose(); }
    }

    /// <summary>Read the reusable input rows of a real node window by stable field identity.</summary>
    private static FlowInputRow[] P0Inputs(FlowNodeForm dialog)
    {
        return dialog.Controls.Find("inputs", true)[0].Controls.OfType<FlowInputRow>().ToArray();
    }

    /// <summary>Changing languages keeps a missing object or method unselected instead of adopting the first available action.</summary>
    private static void P0MissingMethodLanguage()
    {
        string configuredInput = "hardware/" + groupedInput.Id + "/input";
        foreach (string target in new[] { configuredInput, "hardware/p0-removed-object/input" })
        {
            FlowStep original = editor.AddNode();
            original.Caption = "P0 unavailable action";
            original.Target = target;
            original.Action = target == configuredInput ? "io.removed-method" : "io.read";
            original.Inputs.Clear();
            original.Inputs.Add(new FlowInput { Name = "RemovedInput", Source = "Constant", Reference = "unchanged" });
            try
            {
                using (FlowNodeForm dialog = new FlowNodeForm(editor, original))
                {
                    dialog.Show(); Application.DoEvents();
                    ComboBox action = (ComboBox)dialog.Controls.Find("actionValue", true)[0];
                    ComboBox objects = (ComboBox)dialog.Controls.Find("targetValue", true)[0];
                    TextBox caption = (TextBox)dialog.Controls.Find("captionValue", true)[0];
                    caption.Text = "P0 pending caption";
                    Assert(action.SelectedIndex == -1 && (target == configuredInput ? action.Items.Count > 0 : action.Items.Count == 0), "An unavailable object exposed unrelated methods or an unavailable action adopted a default.");
                    foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español, LanguageType.Chinese })
                    {
                        SwitchLanguage(language); Application.DoEvents();
                        Assert(action.SelectedIndex == -1 && action.SelectedItem == null, "Translation selected the first action for an unavailable object or method.");
                        Assert(Convert.ToString(objects.SelectedValue) == target && P0Inputs(dialog).Length == 0, "Translation changed the unavailable target or created another action's inputs.");
                        Assert(caption.Text == "P0 pending caption" && original.Inputs.Single().Reference == "unchanged", "Translation lost pending text or modified the original unavailable node.");
                    }
                    ((Button)dialog.Controls.Find("cancel", true)[0]).PerformClick();
                }
                Assert(original.Action == (target == configuredInput ? "io.removed-method" : "io.read") && original.Caption == "P0 unavailable action", "Cancelling adopted another action or committed the pending caption.");
            }
            finally { editor.Delete(original.Id); SwitchLanguage(LanguageType.Chinese); }
        }
    }

    /// <summary>Empty comparisons retain hidden right bindings across languages while type filtering keeps explicit incompatible values for review.</summary>
    private static void P0ConditionEmptyDrafts()
    {
        ConstantStore store = constantForm.Constants;
        string flag = "P0 condition boolean", distance = "P0 condition distance", barcode = "P0 condition barcode";
        store.Put("Setting", null, new ConstantValue { Key = flag, Type = "Boolean", Value = "True" });
        store.Put("Setting", null, new ConstantValue { Key = distance, Type = "Number", Value = "2.5" });
        store.Put("Setting", null, new ConstantValue { Key = barcode, Type = "Text", Value = "Product B" });
        try
        {
            FlowStep step = ScriptNode("Outputs.Value = true;", ScriptPort("Value", "Boolean"));
            using (FlowNodeForm dialog = new FlowNodeForm(editor, step))
            {
                dialog.Show(); Application.DoEvents();
                FlowBranchEditor branch = (FlowBranchEditor)dialog.Controls.Find("branch_NEXT_dyn", true)[0];
                TabControl tabs = (TabControl)dialog.Controls.Find("executionTabs", true)[0]; tabs.SelectedTab = tabs.TabPages["scriptRoutes"]; Application.DoEvents();
                ((RadioButton)branch.Controls.Find("branchAll", true)[0]).Checked = true;
                FlowConditionRow row = branch.ConditionRows.Single();
                ComboBox left = (ComboBox)row.Controls.Find("conditionLeft", true)[0];
                ComboBox right = (ComboBox)row.Controls.Find("conditionRight", true)[0];
                ComboBox comparison = (ComboBox)row.Controls.Find("conditionComparison", true)[0];
                Assert(right.Items.Cast<object>().Any(item => item.ToString().Contains(flag)) && !right.Items.Cast<object>().Any(item => item.ToString().Contains(distance) || item.ToString().Contains(barcode)), "Boolean conditions offered unrelated numeric or text constants.");
                right.SelectedItem = right.Items.Cast<object>().Single(item => item.ToString().Contains(flag));
                comparison.SelectedValue = "Empty";
                foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
                {
                    SwitchLanguage(language); Application.DoEvents();
                    Assert(Convert.ToString(comparison.SelectedValue) == "Empty" && !right.Enabled && right.SelectedItem != null && right.SelectedItem.ToString().Contains(flag), "Translation erased the hidden right operand of an Empty condition.");
                }
                comparison.SelectedValue = "==";
                Assert(right.Enabled && row.ReadCondition().Right.Source == "Parameter" && row.ReadCondition().Right.Reference == ConstantStore.Reference("Setting", flag), "Leaving Empty mode restored a label or an empty literal instead of its original binding.");
                left.SelectedItem = left.Items.Cast<object>().Single(item => item.ToString().Contains(distance));
                Assert(row.ReadCondition().Right.Reference == ConstantStore.Reference("Setting", flag), "Changing the left operand silently replaced an incompatible selected right binding.");
                Assert(right.Items.Cast<object>().Any(item => item.ToString().Contains(distance)) && !right.Items.Cast<object>().Any(item => item.ToString().Contains(barcode)), "Numeric conditions did not filter text constants or offer compatible numeric values.");
                right.SelectedItem = right.Items.Cast<object>().Single(item => item.ToString().Contains(distance));
                foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
                {
                    SwitchLanguage(language); Application.DoEvents();
                    FlowCondition condition = row.ReadCondition();
                    Assert(condition.Left.Source == "Parameter" && condition.Right.Source == "Parameter" && condition.Left.Reference == ConstantStore.Reference("Setting", distance) && condition.Right.Reference == condition.Left.Reference, "Translation changed a typed condition's stable source identity.");
                }
                dialog.Close();
            }
        }
        finally
        {
            store.Delete("Setting", flag); store.Delete("Setting", distance); store.Delete("Setting", barcode);
            SwitchLanguage(LanguageType.Chinese);
        }
    }

    /// <summary>Axis nodes expose the bound point first, preserve hidden defaults and retain drafts when translating or cancelling.</summary>
    private static void P0AxisParameterEditing()
    {
        SwitchLanguage(LanguageType.Chinese);
        FlowResource axis = new FlowActions(FlowResources.Capture(true)).FindResource("hardware/" + groupedAxis.Id + "/axis");
        FlowStep original = editor.AddNode();
        original.Target = axis.Id;
        original.Action = "point.move";
        original.Inputs = BoundStep(axis, original.Action).Inputs;
        try
        {
            using (FlowNodeForm dialog = new FlowNodeForm(editor, original))
            {
                dialog.Show(); Application.DoEvents();
                FlowInputRow[] rows = P0Inputs(dialog);
                CheckBox advanced = (CheckBox)dialog.Controls.Find("showAdvanced", true)[0];
                Assert(!advanced.Checked && rows.Where(row => row.Visible).Select(row => row.FieldName).SequenceEqual(new[] { "Index" }), "A default axis node exposes advanced coordinate parameters instead of only its target point.");
                Screenshot(dialog, "p0-axis-simple-Chinese");
                Assert(rows.Length == 3 && rows.Single(row => row.FieldName == "Offset").ReadInput().Reference == "0", "Collapsed advanced parameters lost their defaults.");
                advanced.Checked = true; Application.DoEvents();
                Assert(rows.All(row => row.Visible), "Showing advanced parameters did not expose offset and tolerance.");
                FlowInputRow offset = rows.Single(row => row.FieldName == "Offset");
                TextBox value = (TextBox)offset.Controls.Find("inputLiteral", true)[0];
                value.Focus(); value.Text = "1.";
                foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
                {
                    SwitchLanguage(language); Application.DoEvents();
                    Assert(advanced.Checked && value.Text == "1." && offset.ReadInput().Reference == "1.", "Translation parsed or discarded an unfinished axis offset.");
                    Assert(rows.All(row => !row.Controls.Find("inputField_dyn", true)[0].Text.Contains("Double") && !row.Controls.Find("inputField_dyn", true)[0].Text.Contains("Int32")), "Axis business titles expose .NET type names.");
                    Screenshot(dialog, "p0-axis-parameters-" + language);
                }
                ((Button)dialog.Controls.Find("cancel", true)[0]).PerformClick();
                Assert(original.Inputs.Single(input => input.Name == "Offset").Reference == "0", "Cancelling committed an edited axis offset to the original node.");
            }
            original.Inputs.Single(input => input.Name == "Offset").Reference = "1.25";
            using (FlowNodeForm dialog = new FlowNodeForm(editor, original))
            {
                dialog.Show(); Application.DoEvents();
                Assert(((CheckBox)dialog.Controls.Find("showAdvanced", true)[0]).Checked, "A legacy non-default offset was silently hidden.");
                FlowInputRow offset = P0Inputs(dialog).Single(row => row.FieldName == "Offset");
                ((TextBox)offset.Controls.Find("inputLiteral", true)[0]).Text = "2.5";
                ((Button)dialog.Controls.Find("ok", true)[0]).PerformClick();
                Assert(dialog.DialogResult == DialogResult.OK, "The real node confirmation did not accept valid typed axis parameters.");
            }
            FlowStep confirmed = editor.Program.Steps.Single(step => step.Id == original.Id);
            FlowStep copy = FlowProgramFile.Snapshot(new FlowProgram { Steps = new List<FlowStep> { confirmed } }).Steps[0];
            Assert(copy.Inputs.Single(input => input.Name == "Offset").Reference == "2.5" && copy.Inputs.Single(input => input.Name == "Index").Source == "Constant", "Confirmed parameters did not round-trip using stable field and source identities.");
        }
        finally { editor.Delete(original.Id); SwitchLanguage(LanguageType.Chinese); }
    }

    /// <summary>Typed parameter rows retain old boolean spellings and missing choices until the engineer explicitly edits them.</summary>
    private static void P0TypedParameterRows()
    {
        FlowActions actions = new FlowActions(FlowResources.Capture(true));
        FlowMethod wait = actions.FindResource("hardware/" + groupedInput.Id + "/input").Methods.Single(method => method.Id == "io.wait");
        FlowMethod move = actions.FindResource("hardware/" + groupedAxis.Id + "/axis").Methods.Single(method => method.Id == "point.move");
        FlowField index = move.Inputs.Single(field => field.Name == "Index");
        using (Form fixture = new Form { Width = 1000, Height = 220 })
        using (FlowInputRow boolean = new FlowInputRow { Dock = DockStyle.Top })
        using (FlowInputRow point = new FlowInputRow { Dock = DockStyle.Top })
        using (FlowInputRow producer = new FlowInputRow { Dock = DockStyle.Top })
        {
            boolean.Bind(new FlowInput { Name = "Value", Source = "Constant", Reference = "0" }, wait.Inputs.Single(field => field.Name == "Value"), actions, wait.Id);
            point.Bind(new FlowInput { Name = "Index", Source = "Constant", Reference = "removed-point" }, index, actions, move.Id);
            FlowField fieldOnly = new FlowField("Capture", typeof(long), "1") { OutputSourceField = "RunId" };
            producer.Bind(new FlowInput { Name = fieldOnly.Name, Source = "Constant", Reference = "1" }, fieldOnly, actions, "p0.capture.field");
            fixture.Controls.Add(producer); fixture.Controls.Add(point); fixture.Controls.Add(boolean); fixture.Show(); Application.DoEvents();
            ComboBox choice = (ComboBox)point.Controls.Find("inputOption", true)[0];
            ComboBox state = (ComboBox)boolean.Controls.Find("inputOption", true)[0];
            Assert(state.Visible && !boolean.Controls.Find("inputLiteral", true)[0].Visible, "A boolean parameter still requires hand-typed True/False.");
            Assert(producer.Controls.Find("inputReference_dyn", true)[0].Visible && ((TextBox)producer.Controls.Find("inputReference_dyn", true)[0]).ReadOnly && !producer.Controls.Find("inputLiteral", true)[0].Visible, "A field-only producer constraint still exposes its internal numeric value as an editable literal.");
            producer.ContextMenuStrip.Show(producer, 4, 4); Application.DoEvents();
            Assert(!producer.ContextMenuStrip.Items["inputUseLiteral"].Enabled, "A field-only producer constraint allows switching to a fixed internal identifier.");
            producer.ContextMenuStrip.Close();
            Assert(choice.SelectedItem != null && choice.SelectedItem.ToString().Contains("removed-point") && point.ReadInput().Reference == "removed-point", "A missing old point was replaced by the first available point.");
            foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
            {
                SwitchLanguage(language); boolean.RefreshLanguage(); point.RefreshLanguage(); Application.DoEvents();
                Assert(boolean.ReadInput().Reference == "0" && point.ReadInput().Reference == "removed-point", "Translation normalized a saved boolean literal or repaired a missing option without confirmation.");
                Assert(choice.SelectedItem.ToString().Contains("removed-point"), "The engineer can no longer identify the unavailable old point.");
                Assert(!boolean.Controls.Find("inputField_dyn", true)[0].Text.Contains("Boolean") && !point.Controls.Find("inputField_dyn", true)[0].Text.Contains("Int32"), "Reusable rows display implementation type names in business titles.");
            }
            state.SelectedIndex = 0;
            Assert(boolean.ReadInput().Reference == "True", "An explicit Yes choice did not save the canonical boolean value.");
            choice.SelectedIndex = 1;
            Assert(point.ReadInput().Reference == index.Options[0].Value, "Selecting a real point saved its label rather than its stable index.");
            FlowInput binding = new FlowInput { Name = "Value", Source = "Parameter", Reference = "Constants/Setting/DisScann" };
            boolean.SetInput(binding); boolean.RefreshLanguage();
            Assert(boolean.ReadInput().Source == binding.Source && boolean.ReadInput().Reference == binding.Reference && boolean.Controls.Find("inputReference_dyn", true)[0].Visible, "Binding a typed row changed its structured source or left a fixed-value editor visible.");
            Screenshot(fixture, "p0-typed-parameter-rows");
            boolean.ContextMenuStrip.Show(boolean, 4, 4); Application.DoEvents();
            ToolStripMenuItem restore = (ToolStripMenuItem)boolean.ContextMenuStrip.Items["inputUseLiteral"];
            Assert(restore.Enabled, "A normal bound parameter cannot explicitly return to its declared fixed default.");
            restore.PerformClick(); boolean.ContextMenuStrip.Close();
            Assert(boolean.ReadInput().Source == "Constant" && boolean.ReadInput().Reference == wait.Inputs.Single(field => field.Name == "Value").DefaultValue && boolean.Controls.Find("inputOption", true)[0].Visible, "The fixed-value context command lost the declared default or retained the old binding editor.");
        }
        SwitchLanguage(LanguageType.Chinese);
    }

    /// <summary>Enumerate source leaves without depending on the picker's private choice implementation.</summary>
    private static IEnumerable<TreeNode> P0SourceNodes(TreeNodeCollection nodes)
    {
        foreach (TreeNode node in nodes)
        {
            yield return node;
            foreach (TreeNode child in P0SourceNodes(node.Nodes)) yield return child;
        }
    }

    /// <summary>The unified binding window filters actual typed constants, searches business names and preserves references across languages.</summary>
    private static void P0UnifiedSourcePicker()
    {
        ConstantStore store = constantForm.Constants;
        string flag = "P0 binding signal", number = "P0 binding distance", text = "P0 binding barcode";
        store.Put("Setting", null, new ConstantValue { Key = flag, Type = "Boolean", Value = "True" });
        store.Put("Setting", null, new ConstantValue { Key = number, Type = "Number", Value = "2.5" });
        store.Put("Setting", null, new ConstantValue { Key = text, Type = "Text", Value = "Product A" });
        try
        {
            FlowActions actions = new FlowActions(FlowResources.Capture(true));
            FlowMethod wait = actions.FindResource("hardware/" + groupedInput.Id + "/input").Methods.Single(method => method.Id == "io.wait");
            FlowField field = wait.Inputs.Single(input => input.Name == "Value");
            foreach (string oldBoolean in new[] { "True", "0", "1" })
            {
                using (FlowSourceForm picker = new FlowSourceForm(new FlowInput { Name = field.Name, Source = "Constant", Reference = oldBoolean }, actions, wait, field))
                {
                    picker.Show(); Application.DoEvents();
                    ((Button)picker.Controls.Find("ok", true)[0]).PerformClick();
                    Assert(picker.DialogResult == DialogResult.OK && picker.Selection.Reference == oldBoolean, "The unified picker treated a valid old boolean spelling as an unavailable choice.");
                }
            }
            string flagReference = ConstantStore.Reference("Setting", flag);
            FlowInput original = new FlowInput { Name = field.Name, Source = "Constant", Reference = "True" };
            using (FlowSourceForm picker = new FlowSourceForm(original, actions, wait, field))
            {
                picker.Show(); Application.DoEvents();
                TreeView tree = (TreeView)picker.Controls.Find("sources", true)[0];
                TextBox search = (TextBox)picker.Controls.Find("searchValue", true)[0];
                Assert(P0SourceNodes(tree.Nodes).Any(node => node.Name == "Parameter:" + flagReference), "The picker omitted a compatible boolean table value.");
                Assert(!P0SourceNodes(tree.Nodes).Any(node => node.Name == "Parameter:" + ConstantStore.Reference("Setting", number) || node.Name == "Parameter:" + ConstantStore.Reference("Setting", text)), "The picker offered numeric or text constants to a boolean parameter.");
                tree.SelectedNode = P0SourceNodes(tree.Nodes).Single(node => node.Name == "Parameter:" + flagReference);
                Assert(((TextBox)picker.Controls.Find("referenceValue", true)[0]).ReadOnly, "A structured table binding can still be overwritten as free text.");
                search.Text = flag;
                foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
                {
                    SwitchLanguage(language); Application.DoEvents();
                    Assert(search.Text == flag && tree.SelectedNode != null && tree.SelectedNode.Name == "Parameter:" + flagReference, "Translation changed the search draft or selected stable reference.");
                    Assert(original.Source == "Constant" && original.Reference == "True", "A source-window draft modified its caller before confirmation.");
                    Screenshot(picker, "p0-unified-binding-" + language);
                }
                ((Button)picker.Controls.Find("ok", true)[0]).PerformClick();
                Assert(picker.Selection.Source == "Parameter" && picker.Selection.Reference == flagReference, "The picker saved a translated name instead of the selected table reference.");
            }
            string wrongReference = ConstantStore.Reference("Setting", number);
            using (FlowSourceForm picker = new FlowSourceForm(new FlowInput { Name = field.Name, Source = "Parameter", Reference = wrongReference }, actions, wait, field))
            {
                picker.Show(); Application.DoEvents();
                TreeView tree = (TreeView)picker.Controls.Find("sources", true)[0];
                Assert(tree.Nodes.Cast<TreeNode>().Any(node => node.Name == "bindingRetained") && P0SourceNodes(tree.Nodes).Any(node => node.Name == "Parameter:" + wrongReference), "An incompatible old reference disappeared instead of remaining available for correction.");
                ((Button)picker.Controls.Find("ok", true)[0]).PerformClick();
                Assert(picker.DialogResult != DialogResult.OK && picker.Selection.Reference == wrongReference, "The picker silently accepted or replaced an incompatible old binding.");
            }
        }
        finally
        {
            store.Delete("Setting", flag); store.Delete("Setting", number); store.Delete("Setting", text);
            SwitchLanguage(LanguageType.Chinese);
        }
    }

    /// <summary>Image consumers select a capture node from the same actual VPP and persist its RunId output binding without typing a number.</summary>
    private static void P0VisionProducerBindings()
    {
        FlowVisionProbe first = new FlowVisionProbe(), second = new FlowVisionProbe();
        FlowActions actions = new FlowActions(FlowResources.Capture(true));
        FlowResource cameraA = new FlowResource { Id = "vision/p0/vpp-a", Label = "Camera A / VPP A", Kind = "vision", Instance = first, Methods = first.GetFlowMethods(true).ToList() };
        FlowResource cameraB = new FlowResource { Id = "vision/p0/vpp-b", Label = "Camera A / VPP B", Kind = "vision", Instance = second, Methods = second.GetFlowMethods(true).ToList() };
        actions.Resources.Items.Add(cameraA.Id, cameraA); actions.Resources.Items.Add(cameraB.Id, cameraB);
        string legacyRun = "Machine/P0/LegacyCapture/0/RunId";
        actions.Resources.Parameters.Add(legacyRun, 1L);
        FlowStep captureA = editor.AddNode(), captureB = editor.AddNode(), compensation = editor.AddNode();
        captureA.Caption = "P0 capture A"; captureA.Target = cameraA.Id; captureA.Action = "vision.run";
        captureB.Caption = "P0 capture B"; captureB.Target = cameraB.Id; captureB.Action = "vision.run";
        compensation.Caption = "P0 compensation"; compensation.Target = cameraA.Id; compensation.Action = "vision.compensation";
        try
        {
            string expected = editor.Program.ModuleId + "/" + captureA.Id + "/RunId";
            foreach (string methodId in new[] { "vision.image.save", "vision.output" })
            {
                FlowMethod consumer = cameraA.Methods.Single(method => method.Id == methodId);
                FlowField producer = consumer.Inputs.Single(field => field.Name == "RunId");
                Assert(producer.OutputSourceMethod == "vision.run" && producer.OutputSourceField == "RunId", "The actual VPP consumer does not declare its capture-node dependency.");
                FlowStep valid = BoundStep(cameraA, methodId);
                valid.Inputs.Single(input => input.Name == "RunId").Source = "Output";
                valid.Inputs.Single(input => input.Name == "RunId").Reference = expected;
                actions.Validate(valid);
                valid.Inputs.Single(input => input.Name == "RunId").Reference = editor.Program.ModuleId + "/" + captureB.Id + "/RunId";
                bool rejected = false;
                try { actions.Validate(valid); } catch (InvalidOperationException) { rejected = true; }
                Assert(rejected, "A persisted image source from another VPP bypassed the picker restriction.");
                FlowInput captureInput = valid.Inputs.Single(input => input.Name == "RunId");
                foreach (string source in new[] { "Parameter", "Context" })
                {
                    captureInput.Source = source;
                    captureInput.Reference = source == "Parameter" ? legacyRun : "RunId";
                    rejected = false;
                    try { actions.Validate(valid); } catch (InvalidOperationException) { rejected = true; }
                    Assert(rejected, "A " + source + " binding bypassed the capture-node producer constraint.");
                }
                captureInput.Source = "Constant"; captureInput.Reference = "1";
                actions.Validate(valid);
                FlowInput original = new FlowInput { Name = "RunId", Source = "Output", Reference = "" };
                using (FlowSourceForm picker = new FlowSourceForm(original, actions, consumer, producer, null, cameraA.Id))
                {
                    picker.Show(); Application.DoEvents();
                    TreeView tree = (TreeView)picker.Controls.Find("sources", true)[0];
                    TreeNode[] choices = P0SourceNodes(tree.Nodes).Where(node => node.Tag != null).ToArray();
                    Assert(choices.Length == 1 && choices[0].Name == "Output:" + expected, "Image binding offered another VPP, another action or a non-RunId result.");
                    Assert(!choices[0].Text.Contains("RunId") && choices[0].Text.Contains(captureA.Caption), "The image source still exposes an internal run identifier instead of a capture node.");
                    Assert(((TextBox)picker.Controls.Find("referenceValue", true)[0]).ReadOnly && !tree.Nodes.Cast<TreeNode>().Any(node => node.Name == "bindingFixed"), "Image source selection still asks the engineer to type an internal result number.");
                    tree.SelectedNode = choices[0];
                    foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
                    {
                        SwitchLanguage(language); Application.DoEvents();
                        Assert(tree.SelectedNode != null && tree.SelectedNode.Name == "Output:" + expected, "Translation changed the image capture dependency.");
                        if (methodId == "vision.image.save") Screenshot(picker, "p0-capture-binding-" + language);
                    }
                    ((Button)picker.Controls.Find("ok", true)[0]).PerformClick();
                    FlowInput saved = picker.Selection;
                    Assert(picker.DialogResult == DialogResult.OK && saved.Source == "Output" && saved.Reference == expected && original.Reference == "", "Selecting a capture node did not save the structured output reference or changed the caller draft early.");
                    using (FlowInputRow row = new FlowInputRow())
                    {
                        row.Bind(saved, producer, actions, consumer.Id);
                        Assert(row.ReadInput().Reference == expected && row.Controls.Find("inputReference_dyn", true)[0].Text.Contains(captureA.Caption) && !row.Controls.Find("inputReference_dyn", true)[0].Text.Contains("RunId"), "The node parameter row lost the friendly capture-node display or stable binding.");
                    }
                }
            }
        }
        finally
        {
            editor.Delete(compensation.Id); editor.Delete(captureB.Id); editor.Delete(captureA.Id);
            VisionproInterface.VList.Remove(first); VisionproInterface.VList.Remove(second);
            SwitchLanguage(LanguageType.Chinese);
        }
    }
}
