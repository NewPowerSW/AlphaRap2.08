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
    }

    /// <summary>Find a named control in the actual fixed editor layout.</summary>
    private static T ScriptControl<T>(Control owner, string name) where T : Control
    {
        return (T)owner.Controls.Find(name, true).Single();
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
                ComboBox mode = ScriptControl<ComboBox>(dialog, "executionValue_dyn");
                ScriptInput(dialog, "Value").SetInput(new FlowInput { Name = "Value", Reference = "action draft" });
                mode.SelectedValue = "Script"; Application.DoEvents();
                FlowScriptEditor code = ScriptControl<FlowScriptEditor>(dialog, "scriptEditor");
                string sourceText = "Outputs.Value = Inputs.Value + 3; // pending source";
                ScriptControl<RichTextBox>(code, "scriptSource").Text = sourceText;
                ScriptInput(dialog, "Value").SetInput(new FlowInput { Name = "Value", Source = "Context", Reference = "CycleId" });
                foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
                {
                    SwitchLanguage(language); Application.DoEvents();
                    Assert(ScriptControl<RichTextBox>(code, "scriptSource").Text == sourceText, "Language switching translated or replaced script source.");
                    Assert(ScriptInput(dialog, "Value").ReadInput().Source == "Context" && ScriptInput(dialog, "Value").ReadInput().Reference == "CycleId", "Script structured input identity changed with language.");
                    TabControl tabs = ScriptControl<TabControl>(dialog, "executionTabs");
                    Assert(tabs.TabPages.Count == 3 && tabs.TabPages.Cast<TabPage>().Any(page => page.Name == "scriptRoutes"), "Script mode hid the native branch editor entry.");
                    Screenshot(dialog, "script-node-" + language);
                }
                mode.SelectedValue = "None"; Application.DoEvents();
                Assert(!ScriptControl<FlowLayoutPanel>(dialog, "inputs").Controls.OfType<FlowInputRow>().Any(), "Conditions-only mode retained executable inputs.");
                mode.SelectedValue = "Script"; Application.DoEvents();
                Assert(ScriptInput(dialog, "Value").ReadInput().Reference == "CycleId" && ScriptControl<RichTextBox>(code, "scriptSource").Text == sourceText, "A mode round trip lost the script draft.");
                mode.SelectedValue = "Action"; Application.DoEvents();
                Assert(ScriptInput(dialog, "Value").ReadInput().Reference == "action draft", "Script mode replaced the action's independent parameter draft.");
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
                dialog.Show(); ScriptControl<ComboBox>(dialog, "executionValue_dyn").SelectedValue = "Script"; Application.DoEvents();
                FlowScriptEditor code = ScriptControl<FlowScriptEditor>(dialog, "scriptEditor");
                ScriptInput(dialog, "Value").SetInput(new FlowInput { Name = "Value", Reference = "9.25" });
                ScriptControl<Button>(code, "scriptAddInput").PerformClick(); Application.DoEvents();
                DataGridView ports = ScriptControl<DataGridView>(code, "scriptInputPorts");
                Assert(ports.Rows.Count == 2 && ScriptInput(dialog, "Value").ReadInput().Reference == "9.25", "Adding a port lost another input's binding.");
                ports.Rows[1].Cells["portName"].Value = "Count";
                ports.Rows[1].Cells["portType"].Value = "Integer"; Application.DoEvents();
                FlowStep read = new FlowStep { ExecutionMode = "Script" }; code.ReadTo(read);
                Assert(read.ScriptInputs.Single(port => port.Name == "Count").Type == "Integer", "The editor saved a translated type instead of its stable code.");
                Assert(ScriptInput(dialog, "Count").ReadInput().Reference == "0", "New typed script port did not get its declared default.");
                System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
                typeof(FlowScriptEditor).GetField("contextGrid", flags).SetValue(code, ports);
                typeof(FlowScriptEditor).GetField("contextIndex", flags).SetValue(code, 1);
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
                dialog.Show(); ScriptControl<ComboBox>(dialog, "executionValue_dyn").SelectedValue = "Script"; Application.DoEvents();
                FlowScriptEditor code = ScriptControl<FlowScriptEditor>(dialog, "scriptEditor");
                ComboBox objects = ScriptControl<ComboBox>(code, "scriptObjectChoice");
                string expected = "hardware/" + groupedInput.Id + "/input"; objects.SelectedValue = expected; Application.DoEvents();
                ComboBox methods = ScriptControl<ComboBox>(code, "scriptMethodChoice");
                methods.SelectedItem = methods.Items.Cast<FlowMethod>().Single(method => method.Id == "io.read");
                ScriptControl<Button>(code, "scriptInsert").PerformClick();
                FlowStep read = new FlowStep { ExecutionMode = "Script" }; code.ReadTo(read);
                Assert(read.ScriptResources.SequenceEqual(new[] { expected }), "Inserting one call did not declare exactly the chosen actual object.");
                Assert(read.ScriptSource.Contains("Context.CallAsync(\"" + expected + "\", \"io.read\"") && read.ScriptSource.Contains("Dictionary<string, object>"), "Generated source guessed a display name instead of the real identity and method.");
                string originalCode = read.ScriptSource;
                ComboBox bindings = ScriptControl<ComboBox>(code, "scriptBindings_dyn"); ((ToolStripMenuItem)bindings.ContextMenuStrip.Items[0]).PerformClick();
                code.ReadTo(read);
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
                dialog.Show(); ScriptControl<ComboBox>(dialog, "executionValue_dyn").SelectedValue = "Script"; Application.DoEvents();
                FlowScriptEditor code = ScriptControl<FlowScriptEditor>(dialog, "scriptEditor");
                ScriptControl<Button>(code, "scriptCompile").PerformClick();
                TextBox errors = ScriptControl<TextBox>(code, "scriptErrors_dyn");
                Assert(errors.Text.Contains("UserScript.cs") && !errors.Text.Contains("error CS"), "Compilation did not show the debug source mapping.");
                ScriptControl<RichTextBox>(code, "scriptSource").Text = "Outputs.Value = ;";
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
                dialog.Show(); ScriptControl<ComboBox>(dialog, "executionValue_dyn").SelectedValue = "Script"; Application.DoEvents();
                ScriptInput(dialog, "Value").SetInput(new FlowInput { Name = "Value", Reference = "12.5" });
                ScriptControl<CheckBox>(dialog, "scriptBreakpoint").Checked = true;
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
}
