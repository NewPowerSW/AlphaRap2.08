using System;
using System.Collections.Generic;
using System.Linq;
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
    }

    /// <summary>Find a named control in the actual fixed editor layout.</summary>
    private static T ScriptControl<T>(Control owner, string name) where T : Control
    {
        return (T)owner.Controls.Find(name, true).Single();
    }

    /// <summary>Select an always-visible execution mode through its actual button-style radio control.</summary>
    private static void ScriptMode(FlowNodeForm dialog, string mode)
    {
        string name = mode == "Action" ? "scriptAction" : mode == "Script" ? "scriptCustom" : "scriptNone";
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

    /// <summary>Create a valid fixed-value action to exercise transitions to script and empty modes.</summary>
    private static FlowStep ScriptUiNode()
    {
        FlowStep step = editor.AddNode(); step.Caption = "Script UI fixture"; step.Action = "value"; step.Target = "";
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
                ScriptMode(dialog, "Action");
                ScriptInput(dialog, "Value").SetInput(new FlowInput { Name = "Value", Reference = "action draft" });
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
                ScriptMode(dialog, "Action");
                ScriptAssertPages(dialog, "scriptActionPage", "scriptRoutes", "scriptRunSettings");
                Assert(ScriptInput(dialog, "Value").ReadInput().Reference == "action draft", "Script mode replaced the action's independent parameter draft.");
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
                dialog.Show(); ScriptMode(dialog, "Script"); ScriptPage(dialog, "scriptParameters");
                FlowScriptEditor code = ScriptControl<FlowScriptEditor>(dialog, "scriptEditor");
                UserControl settings = ScriptControl<UserControl>(dialog, "scriptSettings");
                ComboBox objects = ScriptControl<ComboBox>(settings, "scriptObjectChoice");
                string expected = "hardware/" + groupedInput.Id + "/input"; objects.SelectedValue = expected; Application.DoEvents();
                ComboBox methods = ScriptControl<ComboBox>(settings, "scriptMethodChoice");
                methods.SelectedItem = methods.Items.Cast<FlowMethod>().Single(method => method.Id == "io.read");
                ScriptControl<Button>(settings, "scriptInsert").PerformClick(); Application.DoEvents();
                Assert(ScriptControl<TabControl>(dialog, "executionTabs").SelectedTab.Name == "scriptTab", "Inserting an object method did not return to the source page.");
                FlowStep read = ScriptReadUiDraft(dialog);
                Assert(read.ScriptResources.SequenceEqual(new[] { expected }), "Inserting one call did not declare exactly the chosen actual object.");
                Assert(read.ScriptSource.Contains("Context.CallAsync(\"" + expected + "\", \"io.read\"") && read.ScriptSource.Contains("Dictionary<string, object>"), "Generated source guessed a display name instead of the real identity and method.");
                string originalCode = read.ScriptSource;
                ScriptPage(dialog, "scriptParameters");
                ComboBox bindings = ScriptControl<ComboBox>(settings, "scriptBindings_dyn"); ((ToolStripMenuItem)bindings.ContextMenuStrip.Items[0]).PerformClick();
                read = ScriptReadUiDraft(dialog);
                Assert(read.ScriptResources.Count == 0 && read.ScriptSource == originalCode, "Removing an allowed-object binding silently rewrote the user's source.");
                ScriptControl<Button>(dialog, "cancel").PerformClick();
            }
            Assert(step.ScriptResources.Count == 0 && step.ScriptSource == "", "Object insertion committed before node confirmation.");
        }
        finally { editor.Delete(step.Id); }
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
                    foreach (string name in new[] { "scriptAction", "scriptCustom", "scriptNone" })
                    {
                        RadioButton mode = ScriptControl<RadioButton>(dialog, name);
                        Assert(mode.Visible && mode.Appearance == Appearance.Button, "An execution-mode button disappeared during language switching.");
                    }
                    ScriptMode(dialog, "Action");
                    ScriptAssertPages(dialog, "scriptActionPage", "scriptRoutes", "scriptRunSettings");
                    Assert(ScriptControl<TabControl>(dialog, "executionTabs").SelectedTab.Name == "scriptActionPage", "Entering action mode did not select its action page.");
                    Screenshot(dialog, "script-flat-action-" + language);
                    ScriptAssertRunSettings(dialog, 4321, "2019", true);
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
                ScriptMode(dialog, "Action"); ScriptMode(dialog, "Script");
                SwitchLanguage(LanguageType.Chinese); Application.DoEvents();
                ScriptAssertRunSettings(dialog, 4321, "999991", true);
                ScriptControl<Button>(dialog, "cancel").PerformClick();
            }
            Assert(step.ExecutionMode == "Action" && step.TimeoutMs == 4321 && step.AlarmCode == "2019" && step.Breakpoint
                && step.ScriptSource == "" && step.ScriptInputs.Count == 0 && step.Inputs.Single().Reference == "original literal", "Closing the layout preview committed an editing draft.");
        }
        finally { editor.Delete(step.Id); SwitchLanguage(LanguageType.Chinese); }
    }
}
