using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using AlphaRap;
using AlphaRap.FlowProgramming;
using NPSDK;

public sealed class ToolboxUiInputs
{
    [FlowCodeField("补偿", "Offset", "Compensación", DefaultValue = "0")]
    public double Offset { get; set; }
    [FlowCodeField("轴点位", "Axis point", "Punto del eje", DefaultValue = "0", ResourceSlot = "Axis")]
    public int PointIndex { get; set; }
}

public sealed class ToolboxUiOutputs
{
    [FlowCodeField("高度", "Height", "Altura")]
    public double Height { get; set; }
}

[FlowCodeModule("ui.fixture.module", "vision", "UI测试模块", "UI test module", "Módulo de prueba UI", Exits = "NEXT,CASE1")]
[FlowCodeResource("Axis", "axis", "定位轴", "Positioning axis", "Eje de posición")]
[FlowCodeResource("Inspection", "vision", "检测视觉", "Inspection VPP", "VPP de inspección")]
[FlowCodeResourceField("Inspection", "Height", typeof(double), "检测高度", "Measured height", "Altura medida")]
[FlowCodeResourceField("Inspection", "Threshold", typeof(double), "检测阈值", "Inspection threshold", "Umbral de inspección", Direction = "Input")]
public sealed class ToolboxUiModule : FlowCodeNode<ToolboxUiInputs, ToolboxUiOutputs>
{
    public static int Constructed;
    /// <summary>Detect accidental execution while displaying or editing the module.</summary>
    public ToolboxUiModule() { Constructed++; }
    /// <summary>Provide a valid offline module contract without accessing any resources.</summary>
    protected override Flow_Chart.ResultType Execute(FlowCodeContext context, ToolboxUiInputs inputs, ToolboxUiOutputs outputs)
    { outputs.Height = inputs.Offset; return Flow_Chart.ResultType.NEXT; }
}

internal static partial class Cases
{
    /// <summary>Deliver actual WinForms drag events to a scrolled inherited canvas and check logical placement.</summary>
    private static void VerifyToolboxDrops(Flow page)
    {
        TabControl tabs = ScriptControl<TabControl>(page, "tabControl1");
        tabs.SelectedTab = tabs.TabPages.Cast<TabPage>().Single(tab => tab.Text == station.Name);
        CheckBox edit = ScriptControl<CheckBox>(page, "edit"); edit.Checked = true; Application.DoEvents();
        station.Canvas.AutoScroll = true; station.Canvas.AutoScrollMinSize = new Size(2400, 2400);
        station.Canvas.AutoScrollPosition = new Point(100, 300); Application.DoEvents();
        foreach (bool code in new[] { false, true })
        {
            int count = editor.Program.Steps.Count;
            Point client = new Point(55, 75), scroll = station.Canvas.AutoScrollPosition, screen = station.Canvas.PointToScreen(client);
            FlowToolboxEntry entry = code ? new FlowToolboxEntry { DefinitionId = "quality.weight-check" } : new FlowToolboxEntry { TargetId = "", ActionId = "value" };
            DataObject payload = new DataObject(FlowToolboxControl.DragFormat, entry);
            DragEventArgs args = new DragEventArgs(payload, 0, screen.X, screen.Y, DragDropEffects.Copy, DragDropEffects.None);
            typeof(Control).GetMethod("OnDragOver", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(station.Canvas, new object[] { args });
            Assert(args.Effect == DragDropEffects.Copy, "The current editing canvas did not accept a valid toolbox entry.");
            typeof(Control).GetMethod("OnDragDrop", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(station.Canvas, new object[] { args });
            FlowStep created = editor.Program.Steps.Last();
            try
            {
                Assert(editor.Program.Steps.Count == count + 1 && created.ParentPath == "/canvas" && created.X == client.X - scroll.X && created.Y == client.Y - scroll.Y,
                    "A scrolled drop used screen coordinates or a different Form canvas.");
                Assert(code ? created.CodeModuleId == "quality.weight-check" : created.Action == "value" && string.IsNullOrEmpty(created.Target), "The drop resolved another action or rejected utility functions.");
            }
            finally { editor.Delete(created.Id); }
        }
        edit.Checked = false;
    }

    /// <summary>Register toolbox navigation, real resource mapping and explicit code-route UI checks.</summary>
    public static void RegisterToolboxUiCases()
    {
        Test("toolbox groups real actions and code modules and preserves searchable identities in three languages", ToolboxUiChoices);
        Test("code role binding filters actual objects and typed VPP terminals without replacing missing references", ToolboxUiBindings);
        Test("code routes retain unconfigured missing and explicit end destinations without outer conditions", ToolboxUiRoutes);
        Test("code module editor retains independent inputs and bindings while switching modes and languages", ToolboxUiNodeDrafts);
    }

    /// <summary>Create an isolated catalog whose methods must never be invoked by UI discovery.</summary>
    private static FlowActions ToolboxUiActions()
    {
        FlowResources resources = new FlowResources();
        foreach (string kind in new[] { "axis", "input", "output", "tray", "code" })
        {
            FlowMethod action = new FlowMethod("ui." + kind, kind, kind, kind) { UsesHardware = false };
            action.Invoke = call => { throw new InvalidOperationException("UI invoked a resource."); };
            resources.Items.Add("ui/" + kind, new FlowResource { Id = "ui/" + kind, Label = "Actual " + kind, Kind = kind, Instance = new object(), Methods = new List<FlowMethod> { action } });
        }
        foreach (string name in new[] { "A", "B" })
        {
            FlowMethod run = new FlowMethod("vision.run", "拍照", "Capture", "Capturar") { UsesHardware = false };
            run.Inputs.Add(new FlowField("TB:Threshold" + name, typeof(double)));
            run.Outputs.Add(new FlowField("TB:Height" + name, typeof(double)));
            run.Outputs.Add(new FlowField("TB:Ready" + name, typeof(bool)));
            run.Outputs.Add(new FlowField("RunId", typeof(long)) { Advanced = true });
            resources.Items.Add("ui/vision" + name, new FlowResource { Id = "ui/vision" + name, Label = "Actual VPP " + name, Kind = "vision", Instance = new object(), Methods = new List<FlowMethod> { run } });
        }
        return new FlowActions(resources);
    }

    /// <summary>Use stable IDs for search and keyboard or double-click insertion without constructing the module.</summary>
    private static void ToolboxUiChoices()
    {
        int constructors = ToolboxUiModule.Constructed;
        FlowCodeDefinition definition = FlowCodeCatalog.Describe(typeof(ToolboxUiModule));
        using (Form hostWindow = new Form { Size = new Size(400, 700) })
        using (FlowToolboxControl toolbox = new FlowToolboxControl { Dock = DockStyle.Fill })
        {
            hostWindow.Controls.Add(toolbox); hostWindow.Show();
            toolbox.BindDefinitions(new[] { definition }); toolbox.BindActions(ToolboxUiActions());
            TreeView tree = ScriptControl<TreeView>(toolbox, "toolboxEntries");
            TextBox search = ScriptControl<TextBox>(toolbox, "toolboxSearch");
            FlowToolboxEntry[] entries = tree.Nodes.Cast<TreeNode>().SelectMany(node => node.Nodes.Cast<TreeNode>()).Select(node => (FlowToolboxEntry)node.Tag).ToArray();
            Assert(entries.Any(item => item.DefinitionId == definition.Id) && entries.Any(item => item.TargetId == "ui/axis") && entries.Any(item => item.TargetId == "ui/input") && entries.Any(item => item.TargetId == "ui/output") && entries.Any(item => item.TargetId == "ui/visionA"), "The toolbox omitted a real capability or code module.");
            Assert(entries.All(item => item.TargetId != "ui/code"), "Code modules were duplicated as ordinary object methods.");
            Assert(entries.Any(item => item.TargetId == ""), "Data/context utility actions were absent from the toolbox.");
            search.Text = definition.Id;
            string expectedId = "code/" + definition.Id;
            int requests = 0;
            toolbox.InsertRequested += entry => { Assert(entry.Id == expectedId, "The request used translated labels instead of a stable entry."); requests++; };
            try
            {
                foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
                {
                    SwitchLanguage(language); Application.DoEvents();
                    Assert(search.Text == definition.Id && tree.Nodes.Count == 1 && tree.Nodes[0].Nodes.Count == 1, "Translation lost a search or reintroduced filtered entries.");
                    tree.SelectedNode = tree.Nodes[0].Nodes[0];
                    typeof(TreeView).GetMethod("OnKeyDown", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(tree, new object[] { new KeyEventArgs(Keys.Enter) });
                    typeof(TreeView).GetMethod("OnNodeMouseDoubleClick", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(tree, new object[] { new TreeNodeMouseClickEventArgs(tree.SelectedNode, MouseButtons.Left, 2, 2, 2) });
                    Screenshot(hostWindow, "toolbox-" + language);
                }
                Assert(requests == 6 && ToolboxUiModule.Constructed == constructors, "Toolbox navigation constructed a module or did not share insertion paths.");
            }
            finally { SwitchLanguage(LanguageType.Chinese); }
        }
    }

    /// <summary>Invoke internal draft methods without adding test-only public APIs.</summary>
    private static object ToolboxUiInvoke(object owner, string name, params object[] values)
    { return owner.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(owner, values); }

    /// <summary>Read the stable values offered by one real resource or VPP mapping cell.</summary>
    private static string[] ToolboxUiCellValues(DataGridView grid, int row)
    { return ((IEnumerable<FlowChoice>)((DataGridViewComboBoxCell)grid.Rows[row].Cells[1]).DataSource).Select(choice => choice.Id).ToArray(); }

    /// <summary>Keep missing old bindings visible and limit terminal choices to the selected VPP and declared type.</summary>
    private static void ToolboxUiBindings()
    {
        FlowCodeDefinition definition = FlowCodeCatalog.Describe(typeof(ToolboxUiModule));
        FlowStep original = new FlowStep { ResourceBindings = new List<FlowResourceBinding> {
            new FlowResourceBinding { Slot = "Axis", Target = "ui/removed-axis", PointTableSignature = "retained-signature" },
            new FlowResourceBinding { Slot = "Inspection", Target = "ui/visionA", Fields = new List<FlowResourceFieldBinding> {
                new FlowResourceFieldBinding { Name = "Height", Field = "TB:Removed", Direction = "Output" },
                new FlowResourceFieldBinding { Name = "Threshold", Field = "TB:ThresholdA", Direction = "Input" } } }
        } };
        using (Form hostWindow = new Form { Size = new Size(950, 360) })
        using (FlowCodeBindingsControl bindings = new FlowCodeBindingsControl { Dock = DockStyle.Fill })
        {
            hostWindow.Controls.Add(bindings); hostWindow.Show();
            ToolboxUiInvoke(bindings, "Bind", definition, original, ToolboxUiActions()); Application.DoEvents();
            DataGridView grid = ScriptControl<DataGridView>(bindings, "codeResourceBindings");
            Assert(ToolboxUiCellValues(grid, 0).OrderBy(value => value).SequenceEqual(new[] { "", "ui/axis", "ui/removed-axis" }), "An axis role offered a tray, IO or another unrelated object.");
            Assert(ToolboxUiCellValues(grid, 2).OrderBy(value => value).SequenceEqual(new[] { "", "TB:HeightA", "TB:Removed" }), "The output mapping offered another VPP, a bool port or RunId.");
            Assert(ToolboxUiCellValues(grid, 3).SequenceEqual(new[] { "", "TB:ThresholdA" }), "The input mapping offered output terminals.");
            grid.Rows[1].Cells[1].Value = "ui/visionB"; Application.DoEvents();
            Assert(ToolboxUiCellValues(grid, 2).Contains("TB:HeightB") && !ToolboxUiCellValues(grid, 2).Contains("TB:HeightA") && Convert.ToString(grid.Rows[2].Cells[1].Value) == "TB:Removed", "Changing VPP left stale candidates or silently replaced the old selected terminal.");
            foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
            {
                SwitchLanguage(language); Application.DoEvents();
                Assert(Convert.ToString(grid.Rows[0].Cells[1].Value) == "ui/removed-axis" && Convert.ToString(grid.Rows[1].Cells[1].Value) == "ui/visionB" && Convert.ToString(grid.Rows[2].Cells[1].Value) == "TB:Removed", "Translation discarded a missing resource or field draft.");
            }
            ToolboxUiInvoke(bindings, "SelectContextRow", grid, new MouseEventArgs(MouseButtons.Right, 1, 5, grid.Height - 2, 0));
            ToolboxUiInvoke(bindings, "ClearBinding", grid, EventArgs.Empty);
            FlowStep draft = new FlowStep(); ToolboxUiInvoke(bindings, "ReadTo", draft, false);
            Assert(draft.ResourceBindings[0].Target == "ui/removed-axis" && draft.ResourceBindings[0].PointTableSignature == "retained-signature", "Right-clicking empty space cleared the previous row or a language refresh confirmed a point table.");
            Assert(original.ResourceBindings[1].Target == "ui/visionA", "Binding drafts mutated the source node before confirmation.");
            SwitchLanguage(LanguageType.Chinese);
        }
    }

    /// <summary>Keep declared exit routing explicit without changing code-owned branch decisions.</summary>
    private static void ToolboxUiRoutes()
    {
        FlowCodeDefinition definition = FlowCodeCatalog.Describe(typeof(ToolboxUiModule));
        FlowStep original = new FlowStep { Next = null, Case1 = "removed-node" };
        using (Form hostWindow = new Form { Size = new Size(800, 360) })
        using (FlowCodeRoutesControl routes = new FlowCodeRoutesControl { Dock = DockStyle.Fill })
        {
            hostWindow.Controls.Add(routes); hostWindow.Show();
            ToolboxUiInvoke(routes, "Bind", definition, original, new List<FlowChoice> { new FlowChoice { Id = "", Label = "End" }, new FlowChoice { Id = "next-node", Label = "Next node" } });
            ComboBox next = ScriptControl<ComboBox>(routes, "codenextTarget"), fail = ScriptControl<ComboBox>(routes, "codecase1Target");
            Assert(next.SelectedIndex == -1 && Convert.ToString(fail.SelectedValue) == "removed-node", "Unconfigured and missing exits were replaced with End.");
            try
            {
                foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
                {
                    SwitchLanguage(language); Application.DoEvents();
                    Assert(next.SelectedIndex == -1 && Convert.ToString(fail.SelectedValue) == "removed-node", "Translation changed a route identity.");
                }
                next.SelectedValue = ""; fail.SelectedValue = "next-node";
                FlowStep draft = new FlowStep(); ToolboxUiInvoke(routes, "ReadTo", draft);
                Assert(draft.Next == "" && draft.Case1 == "next-node" && draft.Case2 == null && draft.Branches.Select(branch => branch.Code).SequenceEqual(new[] { "NEXT", "CASE1" }) && draft.Branches.All(branch => branch.Unconditional && branch.Conditions.Count == 0), "Code routes added conditions or lost explicit End semantics.");
                Assert(!ScriptControl<ComboBox>(routes, "codecase2Target").Visible, "An undeclared code exit remained editable.");
            }
            finally { SwitchLanguage(LanguageType.Chinese); }
        }
    }

    /// <summary>Retain per-mode typed inputs, local role bindings and raw source while moving between peer task pages.</summary>
    private static void ToolboxUiNodeDrafts()
    {
        FlowStep step = editor.AddNode("ui.fixture.module", station.Canvas, new Point(70, 70));
        step.Inputs[0].Reference = "12.5";
        try
        {
            using (FlowNodeForm dialog = new FlowNodeForm(editor, step))
            {
                dialog.Show(); Application.DoEvents();
                RadioButton code = ScriptControl<RadioButton>(dialog, "codeModuleMode");
                Assert(code.Checked, "Opening a code module returned to ordinary Action mode.");
                ScriptPage(dialog, "codeModulePage");
                ScriptInput(dialog, "Offset").SetInput(new FlowInput { Name = "Offset", Reference = "pending-value" });
                ScriptInput(dialog, "PointIndex").SetInput(new FlowInput { Name = "PointIndex", Reference = "9876" });
                FlowResource axis = FlowResources.Capture(true).Items.Values.First(item => item.Kind == "axis" && item.BoundPoints != null && item.BoundPoints.Points.Count > 0);
                FlowCodeBindingsControl bindings = dialog.Controls.Find("codeResourceBindings", true).OfType<FlowCodeBindingsControl>().Single();
                ScriptControl<DataGridView>(bindings, "codeResourceBindings").Rows[0].Cells[1].Value = axis.Id;
                Application.DoEvents();
                ComboBox points = ScriptControl<ComboBox>(ScriptInput(dialog, "PointIndex"), "inputOption");
                Assert(points.Visible && points.Items.Count == axis.BoundPoints.Points.Count + 1 && ScriptInput(dialog, "PointIndex").ReadInput().Reference == "9876", "Selecting a role axis did not refresh its actual point table or silently replaced the old index.");
                ScriptMode(dialog, "Script");
                ScriptControl<RichTextBox>(dialog, "scriptSource").Text = "Outputs.Value = Inputs.Value; // untouched source";
                code.Checked = true; Application.DoEvents();
                Assert(ScriptInput(dialog, "Offset").ReadInput().Reference == "pending-value" && ScriptInput(dialog, "PointIndex").ReadInput().Reference == "9876", "Mode switching replaced a partial code-module input.");
                foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
                {
                    SwitchLanguage(language); Application.DoEvents();
                    Assert(code.Checked && ScriptInput(dialog, "Offset").ReadInput().Reference == "pending-value", "Language switching lost the module or typed input draft.");
                    ScriptPage(dialog, "scriptRoutes");
                    Assert(!ScriptControl<Control>(dialog, "branchPanel").Visible && ScriptControl<Control>(dialog, "codeExitTargets").Visible, "Code node exposed duplicate outer conditions.");
                    ScriptPage(dialog, "codeModulePage"); Screenshot(dialog, "code-module-node-" + language);
                }
                Assert(step.Inputs[0].Reference == "12.5" && !step.ResourceBindings.Any(binding => !string.IsNullOrEmpty(binding.Target)), "Cancelled code editing mutated the real node.");
            }
        }
        finally { SwitchLanguage(LanguageType.Chinese); editor.Delete(step.Id); }
        ToolboxConfirmCodeNode();
    }

    /// <summary>Confirm a usable code node through the real dialog, roundtrip its definition and execute its final CASE.</summary>
    private static void ToolboxConfirmCodeNode()
    {
        FlowStep step = editor.AddNode("quality.weight-check", station.Canvas, new Point(70, 70));
        try
        {
            using (FlowNodeForm dialog = new FlowNodeForm(editor, step))
            {
                dialog.Show(); Application.DoEvents();
                ScriptInput(dialog, "Weight").SetInput(new FlowInput { Name = "Weight", Reference = "15" });
                ScriptPage(dialog, "scriptRoutes");
                ScriptControl<ComboBox>(dialog, "codenextTarget").SelectedValue = "";
                ScriptControl<ComboBox>(dialog, "codecase1Target").SelectedValue = "";
                ScriptControl<Button>(dialog, "ok").PerformClick();
                Assert(dialog.DialogResult == DialogResult.OK, "The actual dialog could not confirm a fully configured code module.");
            }
            FlowStep confirmed = editor.Program.Steps.Single(item => item.Id == step.Id);
            string path = System.IO.Path.Combine(root, "confirmed-code-node.xml");
            FlowProject project = new FlowProject { Modules = new List<FlowProgram> { new FlowProgram { ModuleId = "confirmed", Steps = new List<FlowStep> { confirmed } } } };
            FlowProgramFile.Save(path, project);
            FlowStep restored = FlowProgramFile.Load(path).Modules[0].Steps[0];
            FlowActionOutcome result = new FlowActions(FlowResources.Capture()).ExecuteStep(restored, Context(new FlowResultHub()), System.Threading.CancellationToken.None).GetAwaiter().GetResult();
            Assert(restored.ExecutionMode == "Action" && restored.Action == "code.execute" && restored.Next == "" && restored.Case1 == "" && result.BranchCode == "CASE1" && (double)result.Outputs["Weight"] == 15,
                "Confirm, reload and execution did not preserve the module's typed fields and final exit.");
        }
        finally { editor.Delete(step.Id); }
    }
}
