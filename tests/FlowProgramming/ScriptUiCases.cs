using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlphaRap;
using AlphaRap.FlowProgramming;
using NPSDK;

internal static partial class Cases
{
    /// <summary>Register real WinForms script editing scenarios after the existing fixture has been prepared.</summary>
    public static void RegisterScriptUiCases()
    {
        Test("script node modes retain independent inputs source and references through languages and cancel", ScriptUiModeDrafts);
        Test("script port edits rebuild typed parameter bindings and right-click deletion targets one port", ScriptUiPortEditing);
        Test("script object insertion declares only the selected actual instance without invoking it", ScriptUiObjectInsertion);
        Test("script compile shows source mapping and diagnostics without committing the node", ScriptUiCompilation);
        Test("script node confirmation commits typed ports inputs breakpoints and native branches", ScriptUiConfirmation);
        Test("flat script tabs preserve runtime settings and give source editing room in three languages", ScriptUiFlatLayout);
        Test("readable script editing keeps wrapping selection undo and safe insertion boundaries", ScriptUiSourceEditing);
    }

    /// <summary>Find a named control in the actual fixed editor layout.</summary>
    private static T ScriptControl<T>(Control owner, string name) where T : Control
    {
        return (T)owner.Controls.Find(name, true).Single();
    }

    /// <summary>Select an always-visible execution mode through its actual button-style radio control.</summary>
    private static void ScriptMode(FlowNodeForm dialog, string mode)
    {
        Assert(mode == "Script" || mode == "None", "The test requested a retired execution mode.");
        string name = mode == "Script" ? "scriptCustom" : "scriptNone";
        RadioButton button = ScriptControl<RadioButton>(dialog, name);
        Assert(button.Visible && button.Appearance == Appearance.Button, "The execution mode entry is hidden or still requires a dropdown.");
        button.Checked = true; Application.DoEvents();
        Assert(button.Checked, "The selected execution mode did not become active.");
    }

    /// <summary>Open one of the peer node pages before interacting with its visible controls.</summary>
    private static TabPage ScriptPage(FlowNodeForm dialog, string name)
    {
        TabControl tabs = ScriptControl<TabControl>(dialog, "executionTabs");
        TabPage page = tabs.TabPages.Cast<TabPage>().Single(item => item.Name == name);
        tabs.SelectedTab = page; Application.DoEvents();
        Assert(page.Visible, "The requested node page did not become visible: " + name);
        return page;
    }

    /// <summary>Read metadata from the input/output settings and source from the separate code editor.</summary>
    private static FlowStep ScriptReadUiDraft(FlowNodeForm dialog)
    {
        FlowStep value = new FlowStep { ExecutionMode = "Script" };
        UserControl settings = ScriptControl<UserControl>(dialog, "scriptSettings");
        settings.GetType().GetMethod("ReadTo").Invoke(settings, new object[] { value });
        ScriptControl<FlowScriptEditor>(dialog, "scriptEditor").ReadTo(value);
        return value;
    }

    /// <summary>Compile and execute the actual editor draft using the same visible typed input rows as confirmation.</summary>
    private static IDictionary<string, object> ScriptExecuteUiDraft(FlowNodeForm dialog)
    {
        FlowStep value = ScriptReadUiDraft(dialog);
        value.Inputs = ScriptControl<FlowLayoutPanel>(dialog, "inputs").Controls.OfType<FlowInputRow>().Select(row => row.ReadInput()).ToList();
        FlowScripts.Compile(value);
        return ScriptExecute(new FlowActions(FlowResources.Capture()), value);
    }

    /// <summary>Select the existing in-memory application object and its real input-bearing capability.</summary>
    private static FlowResource ScriptSelectProbeCall(FlowNodeForm dialog)
    {
        ScriptPage(dialog, "scriptParameters");
        FlowResource resource = FlowResources.Capture().Items.Values.Single(item => ReferenceEquals(item.Instance, station.Capability));
        UserControl settings = ScriptControl<UserControl>(dialog, "scriptSettings");
        ScriptControl<ComboBox>(settings, "scriptObjectChoice").SelectedValue = resource.Id; Application.DoEvents();
        ComboBox methods = ScriptControl<ComboBox>(settings, "scriptMethodChoice");
        methods.SelectedItem = methods.Items.Cast<FlowMethod>().Single(method => method.Id == "probe.read");
        return resource;
    }

    /// <summary>Insert the selected object through the real input/output-page command and return to source.</summary>
    private static void ScriptInsertSelectedCall(FlowNodeForm dialog)
    {
        UserControl settings = ScriptControl<UserControl>(dialog, "scriptSettings");
        ScriptControl<Button>(settings, "scriptInsert").PerformClick(); Application.DoEvents();
        Assert(ScriptControl<TabControl>(dialog, "executionTabs").SelectedTab.Name == "scriptTab", "Inserting an object method did not return to the source page.");
    }

    /// <summary>Require the mode's exact peer pages and keep nested tab containers out of the node editor.</summary>
    private static void ScriptAssertPages(FlowNodeForm dialog, params string[] names)
    {
        TabControl tabs = ScriptControl<TabControl>(dialog, "executionTabs");
        Assert(tabs.TabPages.Cast<TabPage>().Select(page => page.Name).SequenceEqual(names), "Execution mode contains unexpected or missing peer pages.");
        foreach (TabPage page in tabs.TabPages)
            Assert(page.Text == FlowText.T((string)page.Tag), "A newly attached task page did not apply the current language: " + page.Name + " / " + page.Text);
        Assert(ScriptDescendants(dialog).OfType<TabControl>().Count() == 1, "The node editor still nests tab containers inside one another.");
        Assert(dialog.Controls.Find("executionValue_dyn", true).Length == 0, "The removed execution-mode dropdown still exists.");
    }

    /// <summary>Enumerate only the node dialog's current control tree for structural layout checks.</summary>
    private static IEnumerable<Control> ScriptDescendants(Control parent)
    {
        foreach (Control child in parent.Controls)
        {
            yield return child;
            foreach (Control nested in ScriptDescendants(child)) yield return nested;
        }
    }

    /// <summary>Keep an explicit retired action fixture to prove conversion remains local until confirmation.</summary>
    private static FlowStep ScriptUiNode()
    {
        FlowStep step = editor.AddNode(); step.Caption = "Script UI fixture"; step.Action = "value"; step.Target = "";
        step.ExecutionMode = "Action"; step.ScriptSource = "";
        step.ScriptInputs.Clear(); step.ScriptOutputs.Clear(); step.ScriptResources.Clear();
        step.Inputs = new List<FlowInput> { new FlowInput { Name = "Value", Reference = "original literal" } };
        return step;
    }

    /// <summary>Read the live typed parameter row by its stable script port name.</summary>
    private static FlowInputRow ScriptInput(FlowNodeForm dialog, string name)
    {
        return ScriptControl<FlowLayoutPanel>(dialog, "inputs").Controls.OfType<FlowInputRow>().Single(row => row.FieldName == name);
    }

    /// <summary>Mode selection and language changes preserve source text and each mode's exact structured input drafts.</summary>
    private static void ScriptUiModeDrafts()
    {
        FlowStep step = ScriptUiNode();
        try
        {
            using (FlowNodeForm dialog = new FlowNodeForm(editor, step))
            {
                dialog.Show(); Application.DoEvents();
                Assert(!ScriptControl<RadioButton>(dialog, "scriptCustom").Checked && !ScriptControl<RadioButton>(dialog, "scriptNone").Checked,
                    "Opening a retired node silently chose a replacement execution mode.");
                Assert(dialog.Controls.Find("scriptAction", true).Length == 0 && dialog.Controls.Find("codeMode", true).Length == 0,
                    "The retired Action or Code module mode remains in the editor.");
                ScriptMode(dialog, "Script");
                FlowScriptEditor code = ScriptControl<FlowScriptEditor>(dialog, "scriptEditor");
                string sourceText = "Outputs.Value = Inputs.Value + 3; // pending source";
                ScriptControl<RichTextBox>(code, "scriptSource").Text = sourceText;
                ScriptPage(dialog, "scriptParameters");
                ScriptInput(dialog, "Value").SetInput(new FlowInput { Name = "Value", Source = "Context", Reference = "CycleId" });
                foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
                {
                    SwitchLanguage(language); Application.DoEvents();
                    Assert(ScriptControl<RichTextBox>(code, "scriptSource").Text == sourceText, "Language switching translated or replaced script source.");
                    Assert(ScriptInput(dialog, "Value").ReadInput().Source == "Context" && ScriptInput(dialog, "Value").ReadInput().Reference == "CycleId", "Script structured input identity changed with language.");
                    ScriptAssertPages(dialog, "scriptTab", "scriptParameters", "scriptRoutes", "scriptRunSettings");
                    Screenshot(dialog, "script-node-" + language);
                }
                ScriptMode(dialog, "None");
                ScriptAssertPages(dialog, "scriptRoutes", "scriptRunSettings");
                Assert(!dialog.Controls.Find("inputs", true).Any(control => control.Visible), "Conditions-only mode retained visible executable inputs.");
                ScriptMode(dialog, "Script");
                Assert(ScriptControl<TabControl>(dialog, "executionTabs").SelectedTab.Name == "scriptTab", "Entering script mode did not open its source page.");
                ScriptPage(dialog, "scriptParameters");
                Assert(ScriptInput(dialog, "Value").ReadInput().Reference == "CycleId" && ScriptControl<RichTextBox>(code, "scriptSource").Text == sourceText, "A mode round trip lost the script draft.");
                ScriptPage(dialog, "scriptRunSettings");
                ScriptControl<CheckBox>(dialog, "scriptBreakpoint").Checked = true;
                ScriptControl<Button>(dialog, "cancel").PerformClick();
            }
            Assert(step.ExecutionMode == "Action" && !step.Breakpoint && step.ScriptSource == "" && step.Inputs.Single().Reference == "original literal", "Cancelling modified the original node.");
        }
        finally { editor.Delete(step.Id); SwitchLanguage(LanguageType.Chinese); }
    }

    /// <summary>Port changes refresh only metadata and retain values already bound to unchanged names.</summary>
    private static void ScriptUiPortEditing()
    {
        FlowStep step = ScriptUiNode();
        try
        {
            using (FlowNodeForm dialog = new FlowNodeForm(editor, step))
            {
                dialog.Show(); ScriptMode(dialog, "Script"); ScriptPage(dialog, "scriptParameters");
                FlowScriptEditor code = ScriptControl<FlowScriptEditor>(dialog, "scriptEditor");
                UserControl settings = ScriptControl<UserControl>(dialog, "scriptSettings");
                ScriptInput(dialog, "Value").SetInput(new FlowInput { Name = "Value", Reference = "9.25" });
                ScriptControl<Button>(settings, "scriptAddInput").PerformClick(); Application.DoEvents();
                DataGridView ports = ScriptControl<DataGridView>(settings, "scriptInputPorts");
                Assert(ports.Rows.Count == 2 && ScriptInput(dialog, "Value").ReadInput().Reference == "9.25", "Adding a port lost another input's binding.");
                ports.Rows[1].Cells["portName"].Value = "Count";
                ports.Rows[1].Cells["portType"].Value = "Integer"; Application.DoEvents();
                FlowStep read = ScriptReadUiDraft(dialog);
                Assert(read.ScriptInputs.Single(port => port.Name == "Count").Type == "Integer", "The editor saved a translated type instead of its stable code.");
                Assert(ScriptInput(dialog, "Count").ReadInput().Reference == "0", "New typed script port did not get its declared default.");
                System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
                settings.GetType().GetField("contextGrid", flags).SetValue(settings, ports);
                settings.GetType().GetField("contextIndex", flags).SetValue(settings, 1);
                ContextMenuStrip menu = ports.ContextMenuStrip; ((ToolStripMenuItem)menu.Items[0]).PerformClick(); Application.DoEvents();
                Assert(ports.Rows.Count == 1 && ScriptInput(dialog, "Value").ReadInput().Reference == "9.25", "Right-click port deletion changed another binding or failed to rebuild inputs.");
                ScriptControl<Button>(dialog, "cancel").PerformClick();
            }
            Assert(step.ScriptInputs.Count == 0, "Editing script ports changed the original action before confirming.");
        }
        finally { editor.Delete(step.Id); }
    }

    /// <summary>Generated call snippets use real object identity, retain the allowed list and do not operate hardware.</summary>
    private static void ScriptUiObjectInsertion()
    {
        FlowStep step = ScriptUiNode();
        try
        {
            using (FlowNodeForm dialog = new FlowNodeForm(editor, step))
            {
                dialog.Show(); ScriptMode(dialog, "Script");
                FlowScriptEditor code = ScriptControl<FlowScriptEditor>(dialog, "scriptEditor");
                FlowResource resource = ScriptSelectProbeCall(dialog); string expected = resource.Id; int before = station.Capability.Calls;
                ScriptInsertSelectedCall(dialog);
                FlowStep read = ScriptReadUiDraft(dialog);
                Assert(read.ScriptResources.SequenceEqual(new[] { expected }), "Inserting one call did not declare exactly the chosen actual object.");
                Assert(read.ScriptSource.Contains("targetId: \"" + expected + "\"") && read.ScriptSource.Contains("methodId: \"probe.read\"")
                    && read.ScriptSource.Contains("inputs: new Dictionary<string, object>"), "Readable generated source changed the actual identity, method or named argument contract.");
                string[] lines = read.ScriptSource.Split('\n');
                Assert(lines.Any(line => line.TrimStart().StartsWith("//") && line.Contains(resource.Label))
                    && lines.Any(line => line.Contains("{ \"Increment\", 2 }") && line.Contains("//")), "The object or its typed parameter is missing its separate business comment.");
                Assert(station.Capability.Calls == before, "Inserting source executed an object method during editing.");
                ScriptExecuteUiDraft(dialog);
                Assert(station.Capability.Calls == before + 2, "Generated source did not call the selected instance with its actual typed default.");
                ScriptSelectProbeCall(dialog); ScriptInsertSelectedCall(dialog);
                read = ScriptReadUiDraft(dialog);
                Assert(read.ScriptSource.Contains("var callResult1 =") && read.ScriptSource.Contains("var callResult2 ="), "Repeated insertion reused a local variable name.");
                ScriptExecuteUiDraft(dialog);
                Assert(station.Capability.Calls == before + 6, "Repeated generated calls failed compilation, changed parameters or invoked the wrong object.");
                ScriptBuildCallContracts(code);
                string originalCode = read.ScriptSource;
                ScriptPage(dialog, "scriptParameters");
                UserControl settings = ScriptControl<UserControl>(dialog, "scriptSettings");
                ComboBox bindings = ScriptControl<ComboBox>(settings, "scriptBindings_dyn"); ((ToolStripMenuItem)bindings.ContextMenuStrip.Items[0]).PerformClick();
                read = ScriptReadUiDraft(dialog);
                Assert(read.ScriptResources.Count == 0 && read.ScriptSource == originalCode, "Removing an allowed-object binding silently rewrote the user's source.");
                ScriptControl<Button>(dialog, "cancel").PerformClick();
            }
            Assert(step.ScriptResources.Count == 0 && step.ScriptSource == "", "Object insertion committed before node confirmation.");
        }
        finally { editor.Delete(step.Id); }
    }

    /// <summary>Compile generated zero- and multi-input calls and preserve typed defaults and escaped business text.</summary>
    private static void ScriptBuildCallContracts(FlowScriptEditor code)
    {
        System.Reflection.MethodInfo build = typeof(FlowScriptEditor).GetMethod("BuildCall", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        string target = "fixture/quoted\"\r\nOutputs.Value = 999;\u0085\u2028\u2029";
        string payload = "payload\"\\\r\n\t\0\u0085\u2028\u2029";
        string title = "业务方法\r\nOutputs.Value = 999;\u0085\u2028\u2029";
        FlowMethod multi = new FlowMethod("readability.multi", title, title, title);
        multi.Inputs.Add(new FlowField("Enabled", typeof(bool), "true"));
        multi.Inputs.Add(new FlowField("Count", typeof(long), "9007199254740993"));
        multi.Inputs.Add(new FlowField("Distance", typeof(double), "12.5"));
        multi.Inputs.Add(new FlowField("Payload", typeof(string), payload) { DisplayName = new[] { title, title, title } });
        string generated = (string)build.Invoke(code, new object[] { target, multi, 41, "    " });
        Assert(generated.Split('\n').Count(line => line.Contains("{ \"Enabled\"") || line.Contains("{ \"Count\"") || line.Contains("{ \"Distance\"") || line.Contains("{ \"Payload\"")) == 4,
            "Multi-input generation combined or omitted typed parameter rows.");
        Assert(!generated.Contains("\u0085") && !generated.Contains("\u2028") && !generated.Contains("\u2029"), "Business text escaped its generated line comment or string literal.");
        ScriptRunGeneratedCall(generated, target, multi.Id, 1, values =>
            values.Count == 4 && values["Enabled"] is bool && (bool)values["Enabled"] && values["Count"] is long
            && (long)values["Count"] == 9007199254740993L && values["Distance"] is double && (double)values["Distance"] == 12.5
            && (string)values["Payload"] == payload);
        FlowMethod empty = new FlowMethod("readability.empty", "无参数方法", "No-input method", "Método sin entradas");
        generated = (string)build.Invoke(code, new object[] { "fixture/empty", empty, 42, "" });
        ScriptRunGeneratedCall(generated, "fixture/empty", empty.Id, 2, values => values.Count == 0);
    }

    /// <summary>Execute generated code through the actual script service and inspect exact target, method and dictionary values.</summary>
    private static void ScriptRunGeneratedCall(string source, string target, string method, double expected, Func<IDictionary<string, object>, bool> inspect)
    {
        FlowStep step = ScriptNode("Outputs.Value = " + expected.ToString(System.Globalization.CultureInfo.InvariantCulture) + ";\n" + source, ScriptPort("Value", "Number"));
        FlowCall call = new FlowCall { Step = step, Inputs = new Dictionary<string, object>(), Context = new FlowExecutionContext(), Token = CancellationToken.None };
        int calls = 0;
        FlowScriptContext context = new FlowScriptContext(call, (targetId, methodId, inputs, token) =>
        {
            Assert(targetId == target && methodId == method && inspect(inputs), "Readable generation altered a stable identity, type or escaped default.");
            calls++; return Task.FromResult(FlowCall.Result("Unused", true));
        });
        Task<IDictionary<string, object>> result = FlowScripts.Execute(step, call, context); Wait(result, 10000);
        Assert(calls == 1 && (double)result.Result["Value"] == expected, "Generated call failed execution or business comments injected executable source.");
    }

    /// <summary>Compilation is an offline check and reports user source mapping and compiler diagnostics in place.</summary>
    private static void ScriptUiCompilation()
    {
        FlowStep step = ScriptUiNode();
        try
        {
            using (FlowNodeForm dialog = new FlowNodeForm(editor, step))
            {
                dialog.Show(); ScriptMode(dialog, "Script");
                FlowScriptEditor code = ScriptControl<FlowScriptEditor>(dialog, "scriptEditor");
                ScriptControl<Button>(code, "scriptCompile").PerformClick();
                TextBox errors = ScriptControl<TextBox>(code, "scriptErrors_dyn");
                Assert(errors.Text.Contains("UserScript.cs") && !errors.Text.Contains("error CS"), "Compilation did not show the debug source mapping.");
                ScriptControl<RichTextBox>(code, "scriptSource").Text = "Outputs.Value = ;";
                Assert(errors.Text.Length == 0 && !errors.Visible, "Editing source retained an obsolete compilation success message.");
                ScriptControl<Button>(code, "scriptCompile").PerformClick();
                Assert(errors.Text.Contains("CS") && errors.Text.Contains("UserScript.cs"), "A compiler error did not retain source diagnostics and location.");
                Screenshot(dialog, "script-compile-diagnostics");
                ScriptControl<Button>(dialog, "cancel").PerformClick();
            }
            Assert(step.ScriptSource == "" && step.ExecutionMode == "Action", "Compile check changed or published the original node.");
        }
        finally { editor.Delete(step.Id); }
    }

    /// <summary>Confirm commits one coherent script definition while file persistence stays outside the node dialog.</summary>
    private static void ScriptUiConfirmation()
    {
        FlowStep step = ScriptUiNode();
        try
        {
            using (FlowNodeForm dialog = new FlowNodeForm(editor, step))
            {
                dialog.Show(); ScriptMode(dialog, "Script"); ScriptPage(dialog, "scriptParameters");
                ScriptInput(dialog, "Value").SetInput(new FlowInput { Name = "Value", Reference = "12.5" });
                ScriptPage(dialog, "scriptRunSettings");
                ScriptControl<CheckBox>(dialog, "scriptBreakpoint").Checked = true;
                Assert(ScriptControl<Button>(dialog, "ok").Visible && ScriptControl<Button>(dialog, "cancel").Visible, "Node confirmation footer disappeared on the run settings page.");
                ScriptControl<Button>(dialog, "ok").PerformClick();
                Assert(dialog.DialogResult == DialogResult.OK, "A valid script node could not be confirmed.");
            }
            FlowStep saved = editor.Program.Steps.Single(item => item.Id == step.Id);
            Assert(saved.ExecutionMode == "Script" && saved.Action == "script.execute" && saved.Target == "" && saved.Breakpoint, "Script confirmation retained a legacy action target or lost the breakpoint.");
            Assert(saved.ScriptInputs.Single().Name == "Value" && saved.ScriptOutputs.Single().Name == "Value" && saved.Inputs.Single().Reference == "12.5", "Script confirmation lost its typed ports or bound input.");
            Assert(saved.Branches.Count == 1 && saved.Branches[0].Code == "NEXT", "Script confirmation replaced the native default branch.");
        }
        finally { editor.Delete(step.Id); }
    }

    /// <summary>Read one stable alarm identity and verify the same run settings on every execution mode page.</summary>
    private static void ScriptAssertRunSettings(FlowNodeForm dialog, int timeout, string alarmCode, bool breakpoint)
    {
        ScriptPage(dialog, "scriptRunSettings");
        ComboBox alarms = ScriptControl<ComboBox>(dialog, "alarmValue");
        FlowChoice choice = alarms.SelectedItem as FlowChoice;
        string selected = choice == null ? alarms.Text.Trim() : choice.Id;
        Assert((int)ScriptControl<NumericUpDown>(dialog, "timeoutValue").Value == timeout && selected == alarmCode
            && ScriptControl<CheckBox>(dialog, "scriptBreakpoint").Checked == breakpoint, "Mode or language switching replaced run settings: timeout=" + ScriptControl<NumericUpDown>(dialog, "timeoutValue").Value + ", alarm=" + selected + ", text=" + alarms.Text + ", index=" + alarms.SelectedIndex + ", breakpoint=" + ScriptControl<CheckBox>(dialog, "scriptBreakpoint").Checked);
        Assert(ScriptControl<Button>(dialog, "ok").Visible && ScriptControl<Button>(dialog, "cancel").Visible, "The fixed footer is not reachable from run settings.");
    }

    /// <summary>Check the actual source viewport and ensure empty diagnostics reserve no layout space.</summary>
    private static void ScriptAssertSourceRoom(FlowNodeForm dialog)
    {
        ScriptPage(dialog, "scriptTab");
        FlowScriptEditor code = ScriptControl<FlowScriptEditor>(dialog, "scriptEditor");
        RichTextBox source = ScriptControl<RichTextBox>(code, "scriptSource");
        TextBox errors = ScriptControl<TextBox>(code, "scriptErrors_dyn");
        Assert(source.Visible && source.Height > 350, "Source editing is still squeezed into a short viewport.");
        Button compile = ScriptControl<Button>(code, "scriptCompile");
        Assert(compile.Bounds.Top >= 0 && compile.Bounds.Bottom <= compile.Parent.ClientSize.Height && compile.Height >= 23, "The compilation command is clipped outside its toolbar.");
        TableLayoutPanel layout = errors.Parent as TableLayoutPanel;
        Assert(string.IsNullOrEmpty(errors.Text) && !errors.Visible && layout != null && layout.GetRowHeights()[layout.GetRow(errors)] <= 1,
            "Empty compiler diagnostics still consume source editing space.");
        Assert(code.Controls.Find("scriptInputPorts", true).Length == 0 && code.Controls.Find("scriptObjectChoice", true).Length == 0,
            "The source editor still embeds port tables or object pickers.");
    }

    /// <summary>Exercise the flat task pages and preserve drafts and run settings across three complete language passes.</summary>
    private static void ScriptUiFlatLayout()
    {
        FlowStep step = ScriptUiNode(); step.TimeoutMs = 4321; step.AlarmCode = "2019"; step.Breakpoint = true;
        const string sourceDraft = "Outputs.Value = Inputs.Value + 7; // source stays editable";
        try
        {
            using (FlowNodeForm dialog = new FlowNodeForm(editor, step))
            {
                dialog.Show(); ScriptMode(dialog, "Script");
                FlowScriptEditor code = ScriptControl<FlowScriptEditor>(dialog, "scriptEditor");
                ScriptControl<RichTextBox>(code, "scriptSource").Text = sourceDraft;
                ScriptPage(dialog, "scriptParameters");
                ScriptInput(dialog, "Value").SetInput(new FlowInput { Name = "Value", Reference = "21.5" });
                foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
                {
                    SwitchLanguage(language); Application.DoEvents();
                    foreach (string name in new[] { "scriptCustom", "scriptNone" })
                    {
                        RadioButton mode = ScriptControl<RadioButton>(dialog, name);
                        Assert(mode.Visible && mode.Appearance == Appearance.Button, "An execution-mode button disappeared during language switching.");
                    }
                    Assert(dialog.Controls.Find("scriptAction", true).Length == 0 && dialog.Controls.Find("codeMode", true).Length == 0,
                        "A retired execution mode reappeared after language switching.");
                    ScriptMode(dialog, "None");
                    ScriptMode(dialog, "Script");
                    ScriptAssertPages(dialog, "scriptTab", "scriptParameters", "scriptRoutes", "scriptRunSettings");
                    Assert(ScriptControl<TabControl>(dialog, "executionTabs").SelectedTab.Name == "scriptTab", "Entering script mode did not select its source page.");
                    ScriptAssertSourceRoom(dialog);
                    Assert(ScriptControl<RichTextBox>(code, "scriptSource").Text == sourceDraft, "Mode or language changes replaced the pending source.");
                    Screenshot(dialog, "script-flat-source-" + language);
                    ScriptPage(dialog, "scriptParameters");
                    foreach (string name in new[] { "scriptObjectChoice", "scriptMethodChoice", "scriptInsert" })
                    {
                        Control control = ScriptControl<Control>(dialog, name);
                        Assert(control.Visible && control.Height >= 23 && control.Bounds.Top >= 0 && control.Bounds.Bottom <= control.Parent.ClientSize.Height,
                            "An object-call control is clipped outside its row: " + name + " / " + control.Bounds);
                    }
                    Assert(ScriptControl<UserControl>(dialog, "scriptSettings").Visible && ScriptControl<DataGridView>(dialog, "scriptInputPorts").Visible
                        && ScriptInput(dialog, "Value").ReadInput().Reference == "21.5", "The input/output page hid settings or lost the typed binding draft.");
                    Screenshot(dialog, "script-flat-input-output-" + language);
                    TabPage routes = ScriptPage(dialog, "scriptRoutes");
                    Assert(routes.Contains(ScriptControl<FlowLayoutPanel>(dialog, "branchPanel")), "NEXT and CASE editing remains outside its peer task page.");
                    Screenshot(dialog, "script-flat-routes-" + language);
                    ScriptAssertRunSettings(dialog, 4321, "2019", true);
                    Screenshot(dialog, "script-flat-run-settings-" + language);
                    ScriptMode(dialog, "None");
                    ScriptAssertPages(dialog, "scriptRoutes", "scriptRunSettings");
                    Assert(ScriptControl<TabControl>(dialog, "executionTabs").SelectedTab.Name == "scriptRoutes", "Entering conditions-only mode did not open its routes.");
                    ScriptAssertRunSettings(dialog, 4321, "2019", true);
                }
                ComboBox customAlarm = ScriptControl<ComboBox>(dialog, "alarmValue");
                customAlarm.SelectedIndex = -1; customAlarm.Text = "999991";
                ScriptMode(dialog, "None"); ScriptMode(dialog, "Script");
                SwitchLanguage(LanguageType.Chinese); Application.DoEvents();
                ScriptAssertRunSettings(dialog, 4321, "999991", true);
                ScriptControl<Button>(dialog, "cancel").PerformClick();
            }
            Assert(step.ExecutionMode == "Action" && step.TimeoutMs == 4321 && step.AlarmCode == "2019" && step.Breakpoint
                && step.ScriptSource == "" && step.ScriptInputs.Count == 0 && step.Inputs.Single().Reference == "original literal", "Closing the layout preview committed an editing draft.");
        }
        finally { editor.Delete(step.Id); SwitchLanguage(LanguageType.Chinese); }
    }

    /// <summary>Verify source layout changes preserve native handles, user selection and the existing undo operation.</summary>
    private static void ScriptCheckWrapping(FlowScriptEditor code)
    {
        RichTextBox source = ScriptControl<RichTextBox>(code, "scriptSource");
        CheckBox wrap = ScriptControl<CheckBox>(code, "scriptWordWrap");
        Control numbers = ScriptControl<Control>(code, "scriptLineNumbers");
        Assert(wrap.Checked && numbers.Visible && numbers.Width >= 25 && source.Font.Name == "Consolas" && Math.Abs(source.Font.SizeInPoints - 11F) < 0.2F,
            "The readable source editor lost default wrapping, line numbers or its 11-point code font.");
        source.Text = "if (true)\n{\n    Outputs.Value = Inputs.Value; // " + new string('x', 500) + "\n}\n";
        string original = source.Text; int edit = original.IndexOf("Inputs.Value", StringComparison.Ordinal);
        source.Select(edit, "Inputs.Value".Length); source.SelectedText = "Inputs.Value + 2";
        string changed = source.Text; source.Select(edit, "Inputs.Value".Length);
        int selection = source.SelectionStart, length = source.SelectionLength;
        IntPtr handle = source.Handle; int start = changed.IndexOf("// ", StringComparison.Ordinal), end = changed.IndexOf('\n', start) - 1;
        Application.DoEvents();
        Assert(source.GetPositionFromCharIndex(end).Y > source.GetPositionFromCharIndex(start).Y, "Default wrapping did not lay out the actual long source line.");
        wrap.Checked = false; Application.DoEvents();
        Assert(source.GetPositionFromCharIndex(end).Y == source.GetPositionFromCharIndex(start).Y, "Disabling wrapping left the native long line wrapped.");
        Assert(source.Handle == handle && source.Text == changed && source.SelectionStart == selection && source.SelectionLength == length,
            "Disabling wrapping recreated the editor or lost text and selection.");
        wrap.Checked = true; Application.DoEvents();
        Assert(source.GetPositionFromCharIndex(end).Y > source.GetPositionFromCharIndex(start).Y && source.Handle == handle
            && source.Text == changed && source.SelectionStart == selection && source.SelectionLength == length, "Re-enabling wrapping changed source state or did not reflow the long line.");
        Assert(source.CanUndo, "Wrapping changes cleared the user's pending undo."); source.Undo();
        Assert(source.Text == original, "Undo after changing wrapping did not restore the user's previous text.");
    }

    /// <summary>Keep selected source and indentation intact while inserting at a logical line boundary and file end.</summary>
    private static void ScriptCheckInsertionEditing(FlowNodeForm dialog)
    {
        RichTextBox source = ScriptControl<RichTextBox>(dialog, "scriptSource");
        source.Text = "if (true)\n{\n    Outputs.Value = Inputs.Value;\n}\n";
        string original = source.Text; int selection = original.IndexOf("Inputs.Value", StringComparison.Ordinal), line = original.LastIndexOf('\n', selection) + 1;
        source.Select(selection, "Inputs.Value".Length); ScriptSelectProbeCall(dialog); ScriptInsertSelectedCall(dialog);
        string inserted = source.Text, prefix = original.Substring(0, line), suffix = original.Substring(line);
        Assert(inserted.StartsWith(prefix, StringComparison.Ordinal) && inserted.EndsWith(suffix, StringComparison.Ordinal), "Call insertion replaced selected business code or changed surrounding lines.");
        string block = inserted.Substring(prefix.Length, inserted.Length - prefix.Length - suffix.Length);
        Assert(block.Split('\n').Where(value => value.Trim().Length > 0).All(value => value.StartsWith("    ", StringComparison.Ordinal)) && block.EndsWith("\n", StringComparison.Ordinal),
            "Generated call did not reuse indentation and isolate itself from the preserved source line.");
        Assert(source.SelectionLength == 0 && source.SelectionStart == inserted.Length - suffix.Length, "Insertion left the caret inside preserved source instead of after its code block.");
        Assert(source.CanUndo, "Insertion was not added to the ordinary editor undo stack."); source.Undo();
        Assert(source.Text == original, "One undo did not remove the whole generated block while preserving selected code.");
        source.Text = "Outputs.Value = Inputs.Value;"; original = source.Text; source.Select(source.TextLength, 0);
        ScriptSelectProbeCall(dialog); ScriptInsertSelectedCall(dialog);
        Assert(source.Text.StartsWith(original + "\n", StringComparison.Ordinal) && source.SelectionStart == source.TextLength,
            "Appending at file end merged the call into a statement or misplaced the caret.");
        ScriptExecuteUiDraft(dialog);
    }

    /// <summary>Insert outside multi-line string and comment tokens, preserving their contents while calling the real object.</summary>
    private static void ScriptCheckMultiLineTokens(FlowNodeForm dialog)
    {
        RichTextBox source = ScriptControl<RichTextBox>(dialog, "scriptSource");
        foreach (string token in new[] { "var text = @\"first\ninside token\nlast\";", "/* first\ninside token\nlast */" })
        {
            source.Text = "Outputs.Value = Inputs.Value;\n" + token + "\n";
            string originalToken = token; source.Select(source.Text.IndexOf("inside token", StringComparison.Ordinal) + 3, 0);
            int before = station.Capability.Calls; ScriptSelectProbeCall(dialog); ScriptInsertSelectedCall(dialog);
            Assert(source.Text.Contains(originalToken), "Insertion altered an existing multi-line token.");
            ScriptExecuteUiDraft(dialog);
            Assert(station.Capability.Calls == before + 2, "Insertion stayed inside a string or comment instead of executing the selected real object call.");
        }
    }

    /// <summary>Exercise readable source editing without losing drafts, caret state, undo or language-independent code.</summary>
    private static void ScriptUiSourceEditing()
    {
        FlowStep step = ScriptUiNode();
        try
        {
            using (FlowNodeForm dialog = new FlowNodeForm(editor, step))
            {
                dialog.Show(); ScriptMode(dialog, "Script");
                FlowScriptEditor code = ScriptControl<FlowScriptEditor>(dialog, "scriptEditor");
                ScriptCheckWrapping(code); ScriptCheckInsertionEditing(dialog); ScriptCheckMultiLineTokens(dialog);
                RichTextBox source = ScriptControl<RichTextBox>(code, "scriptSource");
                string original = source.Text; int start = original.IndexOf("targetId:", StringComparison.Ordinal);
                source.Select(start, "targetId".Length); int length = source.SelectionLength;
                foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
                {
                    SwitchLanguage(language); Application.DoEvents();
                    Assert(source.Text == original && source.SelectionStart == start && source.SelectionLength == length, "Language switching rewrote generated source or moved the editor selection.");
                    Screenshot(dialog, "script-readable-" + language);
                }
                ScriptControl<Button>(dialog, "cancel").PerformClick();
            }
            Assert(step.ExecutionMode == "Action" && step.ScriptSource == "", "Editing readability options or generated calls committed the node before confirmation.");
        }
        finally { editor.Delete(step.Id); SwitchLanguage(LanguageType.Chinese); }
    }
}
