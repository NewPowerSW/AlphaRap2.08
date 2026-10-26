using System;
using System.Collections;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using AlphaRap;
using AlphaRap.Configuration;
using AlphaRap.FlowProgramming;
using NPSDK;
using NPSDK.Motion;

internal static partial class Cases
{
    /// <summary>Find a property by stable key rather than translated text or row position.</summary>
    private static DataGridViewRow PropertyRow(DataGridView grid, string name)
    {
        return grid.Rows.Cast<DataGridViewRow>().Single(row => Convert.ToString(row.Cells[0].Value) == name);
    }

    /// <summary>Assert the editor contains exactly the active driver's fields.</summary>
    private static void AssertFields(DataGridView grid, params string[] names)
    {
        Assert(grid.Rows.Cast<DataGridViewRow>().Select(row => Convert.ToString(row.Cells[0].Value)).SequenceEqual(names), "The editor exposed properties from another driver.");
    }

    /// <summary>Confirm driver factories and editable fields agree without initializing hardware.</summary>
    private static void HardwareCapabilityFields()
    {
        HardwareDefinition axis = new HardwareDefinition();
        Assert(axis.Vendors().SequenceEqual(new[] { "Adlink", "Leadshine" }) && NPControlBase.CreateAxis(NPVendor.Omron) == null && NPControlBase.CreateAxis(NPVendor.Keyence) == null, "Unsupported PLC brands were offered as motion drivers.");
        HardwareDefinition io = new HardwareDefinition { Kind = "Input", Port = "00100", PlcIp = "unused", PlcPort = -1, OmronPort = -1, Address = -1 };
        io.Validate(); Assert(io.Fields().SequenceEqual(new[] { "Name", "ControlType", "Vendor", "Port", "ReverseState" }), "Leadshine reads unused PLC properties.");
        using (HardwareEditForm dialog = new HardwareEditForm(axis, value => { }))
        {
            DataGridView grid = (DataGridView)dialog.Controls.Find("hardwareParameters", true)[0];
            Assert(((DataGridViewComboBoxCell)PropertyRow(grid, "Vendor").Cells[1]).Items.Cast<string>().SequenceEqual(axis.Vendors()), "The axis editor ignored SDK-supported motion vendors.");
        }
    }

    /// <summary>Switch real vendor cells, retain drafts, and refresh language without replacing the active editor.</summary>
    private static void HardwareVendorEditor()
    {
        HardwareDefinition committed = null;
        using (HardwareEditForm dialog = new HardwareEditForm(new HardwareDefinition { Kind = "Input", Name = "Arrival", Port = "00100" }, value => committed = value))
        {
            dialog.Show(); Application.DoEvents();
            DataGridView grid = (DataGridView)dialog.Controls.Find("hardwareParameters", true)[0];
            AssertFields(grid, "Name", "ControlType", "Vendor", "Port", "ReverseState");
            PropertyRow(grid, "Vendor").Cells[1].Value = "Keyence"; Application.DoEvents();
            AssertFields(grid, "Name", "ControlType", "Vendor", "PlcIp", "PlcPort", "DeviceType", "DataFormat", "Address", "ReverseState");
            PropertyRow(grid, "Address").Cells[1].Value = "8";
            grid.CurrentCell = PropertyRow(grid, "PlcIp").Cells[1]; grid.BeginEdit(false); ((TextBox)grid.EditingControl).Text = "10.9.8.";
            foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
            {
                SwitchLanguage(language); Application.DoEvents();
                Assert(grid.EditingControl != null && ((TextBox)grid.EditingControl).Text == "10.9.8.", "Language switching discarded the active driver parameter.");
            }
            ((TextBox)grid.EditingControl).Text = "10.9.8.7"; grid.EndEdit();
            PropertyRow(grid, "Vendor").Cells[1].Value = "Omron"; Application.DoEvents();
            AssertFields(grid, "Name", "ControlType", "Vendor", "PlcIp", "OmronAddress", "OmronPort", "ReverseState");
            PropertyRow(grid, "OmronAddress").Cells[1].Value = "I04[0]";
            PropertyRow(grid, "Vendor").Cells[1].Value = "Leadshine"; Application.DoEvents();
            AssertFields(grid, "Name", "ControlType", "Vendor", "Port", "ReverseState");
            PropertyRow(grid, "Vendor").Cells[1].Value = "Keyence"; Application.DoEvents();
            Assert(Convert.ToString(PropertyRow(grid, "PlcIp").Cells[1].Value) == "10.9.8.7" && Convert.ToString(PropertyRow(grid, "Address").Cells[1].Value) == "8", "Switching vendor replaced the retained parameter text.");
            using (Bitmap image = new Bitmap(dialog.Width, dialog.Height)) { dialog.DrawToBitmap(image, new Rectangle(Point.Empty, dialog.Size)); image.Save(Path.Combine(root, "keyence-properties.png")); }
            ((Button)dialog.Controls.Find("hardwareOk", true)[0]).PerformClick();
            Assert(committed != null && committed.Vendor == NPVendor.Keyence && committed.Address == 8 && committed.PlcIp == "10.9.8.7", "The active vendor fields were not committed to the draft.");
        }
        SwitchLanguage(LanguageType.Chinese);
    }

    /// <summary>Keep technical identities stable while displaying actual IO Text and keeping group captions synchronized.</summary>
    private static void IoDisplayNames()
    {
        using (Form form = new Form { Name = "IoLabelProbe" })
        using (NP_Input input = new NP_Input { Name = "DI_TECHNICAL", Text = "上料到位" })
        using (NP_Output output = new NP_Output { Name = "DO_TECHNICAL", Text = "夹紧气缸" })
        using (NP_InputGroup group = new NP_InputGroup())
        {
            form.Controls.Add(input); form.Controls.Add(output); RegisterForm(form);
            try
            {
                FlowResources resources = FlowResources.Capture();
                FlowResource source = resources.Items.Values.Single(item => ReferenceEquals(item.Instance, input));
                Assert(source.Label == "上料到位" && source.Id.EndsWith("/DI_TECHNICAL") && resources.Items.Values.Single(item => ReferenceEquals(item.Instance, output)).Label == "夹紧气缸", "IO labels use Control.Name instead of Text.");
                string identity = source.Id; input.Text = "Arrival sensor";
                Assert(source.Label == "Arrival sensor" && source.Id == identity, "A display-name change altered the flow identity.");
                string technical = group.IO.Name; group.IO.Text = "原始输入文字";
                Assert(group.SignalCaption == "原始输入文字" && group.Controls.Count == 1 && group.Height == 40 && group.IO.Name == technical, "The compact group duplicated the caption or renamed the underlying object.");
            }
            finally { ((IList)host.GetField("lstForm").GetValue(null)).Remove(form); }
        }
    }

    /// <summary>Exclude trays, ignore unrelated tables, and explicitly review old Table inputs in the normal node editor.</summary>
    private static void AxisBoundPointChoices()
    {
        NP_AxisGroup group = (NP_AxisGroup)hardware.Resources.Entries.Single(item => item.Definition.Id == groupedAxis.Id).Group;
        using (Form form = new Form { Name = "UnrelatedPoints" })
        using (NP_PositionTable extra = new NP_PositionTable { Name = "ExtraPoints" })
        {
            extra.DataSet1 = new DataTable("ExtraTable"); extra.DataSet1.Columns.Add("Position", typeof(double)); extra.ParameterSet.Clear();
            extra.ParameterSet.Add(new PositionColumn { Axis = group.Motor, Name = extra.DataSet1.Columns[0].ColumnName, Limit_Low = -10000, Limit_High = 10000 });
            extra.DataSet1.Rows.Add(555); extra.DataSet1.AcceptChanges(); form.Controls.Add(extra); RegisterForm(form);
            try
            {
                FlowActions actions = new FlowActions(FlowResources.Capture()) { Simulate = true };
                FlowResource axis = actions.FindResource("hardware/" + groupedAxis.Id + "/axis");
                Assert(actions.Resources.Items.Values.Where(item => item.Selectable && item.Category == "motion").All(item => item.Kind == "axis"), "Motion objects still include trays.");
                Assert(actions.Resources.Items.Values.Where(item => item.Kind == "tray").All(item => item.Category == "tray"), "Tray calculations lost their own category.");
                FlowStep step = BoundStep(axis, "point.move");
                Assert(!axis.Methods.Single(method => method.Id == "point.move").Inputs[0].Options.Any(option => option.Label.Contains("555")), "An unrelated table leaked into the axis point selector.");
                Assert((double)ExecuteBound(actions, step)["Position"] == (double)group.Points.DataSet1.Rows[0]["Position", DataRowVersion.Original], "Motion guessed a point table instead of using the page binding.");
                FlowStep old = editor.AddNode(); old.Caption = "Review bound point"; old.Action = step.Action; old.Target = step.Target; old.Inputs = step.Inputs;
                old.Inputs.Insert(0, new FlowInput { Name = "Table", Reference = axis.BoundPoints.Id });
                bool blocked = false; try { actions.Validate(old); } catch (InvalidOperationException) { blocked = true; }
                Assert(blocked, "An old dynamic Table input was silently accepted.");
                using (FlowNodeForm dialog = new FlowNodeForm(editor, old))
                {
                    dialog.Show(); Application.DoEvents();
                    FlowLayoutPanel grid = (FlowLayoutPanel)dialog.Controls.Find("inputs", true)[0];
                    Assert(grid.Controls.OfType<FlowInputRow>().All(row => row.FieldName != "Table"), "The node editor still asks for a point table.");
                    ComboBox target = (ComboBox)dialog.Controls.Find("targetValue", true)[0];
                    Assert(((System.Collections.Generic.List<FlowChoice>)target.DataSource).All(choice => actions.Resources.Items[choice.Id].Kind == "axis"), "Motion target picker includes non-axis objects.");
                    ((Button)dialog.Controls.Find("ok", true)[0]).PerformClick();
                    Assert(dialog.DialogResult == DialogResult.OK, "Explicit review did not update the old point node.");
                }
                Assert(editor.Program.Steps.Single(node => node.Id == old.Id).Inputs.Count == 3, "The reviewed node retained a redundant Table input.");
                editor.Delete(old.Id);
            }
            finally { ((IList)host.GetField("lstForm").GetValue(null)).Remove(form); }
        }
    }

    /// <summary>Edit through the real context-menu command and keep cancel/OK separate from disk save.</summary>
    private static void ResourceProperties()
    {
        HardwareDefinition original = hardware.Resources.Entries.Single(item => item.Definition.Id == groupedInput.Id).Definition.Copy();
        byte[] disk = File.ReadAllBytes(hardware.Resources.PathName);
        ioPage.Show(); ((TabControl)ioPage.Controls.Find("tabControl1", true)[0]).SelectedTab = (TabPage)ioPage.Controls.Find("ioResourcesPage", true)[0]; Application.DoEvents();
        HardwareResourcePanel panel = (HardwareResourcePanel)ioPage.Controls.Find("ioResourcePanel", true)[0];
        ((ComboBox)panel.Controls.Find("resourceSelected", true)[0]).SelectedIndex = 0;
        foreach (bool accept in new[] { false, true })
        {
            HardwareEntry clicked = hardware.Resources.Entries.Single(item => item.Definition.Id == groupedInput.Id);
            ContextMenuStrip menu = clicked.Group.ContextMenuStrip;
            bool opened = false; Exception failure = null;
            using (System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer { Interval = 25 })
            {
                timer.Tick += (sender, args) => {
                    HardwareEditForm dialog = Application.OpenForms.OfType<HardwareEditForm>().FirstOrDefault();
                    if (dialog == null) return;
                    timer.Stop();
                    try
                    {
                        DataGridView grid = (DataGridView)dialog.Controls.Find("hardwareParameters", true)[0]; opened = true;
                        Assert(Convert.ToString(PropertyRow(grid, "Name").Cells[1].Value) == original.Name, "Context properties opened a different selected resource.");
                        PropertyRow(grid, "Name").Cells[1].Value = "Context arrival";
                        ((Button)dialog.Controls.Find(accept ? "hardwareOk" : "hardwareCancel", true)[0]).PerformClick();
                    }
                    catch (Exception error) { failure = error; dialog.DialogResult = DialogResult.Cancel; }
                };
                timer.Start(); menu.Show(clicked.Group, new Point(5, 5)); Application.DoEvents();
                Assert(menu.Items["resourceProperties"].Enabled, "Property editing was unavailable for an idle resource.");
                menu.Items["resourceProperties"].PerformClick();
            }
            if (failure != null) throw failure;
            Assert(opened && hardware.Resources.Entries.Single(item => item.Definition.Id == groupedInput.Id).Definition.Name == (accept ? "Context arrival" : original.Name), "Context property commit/cancel changed the wrong draft.");
            Assert(File.ReadAllBytes(hardware.Resources.PathName).SequenceEqual(disk), "Property OK bypassed MainForm Save.");
        }
        hardware.Resources.Put(original); hardware.Resources.Save(); ioPage.Hide();
    }

    /// <summary>Read an explicit constant-table object using its selected business key.</summary>
    private static object ConstantValueOf(FlowActions actions, string scope, string key)
    {
        FlowStep step = BoundStep(actions.FindResource("constants/" + scope), "constant.read"); step.Inputs[0].Reference = key;
        return ExecuteBound(actions, step)["Value"];
    }

    /// <summary>Expose the two real tables and preserve saved snapshots, typed keys and recipe switching.</summary>
    private static void ConstantTableObjects()
    {
        string key = "Flow constant/型号", previous = SysPara.FilePath;
        ConstantStore store = constantForm.Constants;
        store.Put("Setting", null, new ConstantValue { Key = key, Type = "Number", Value = "12.75" });
        store.Put("Recipe", null, new ConstantValue { Key = key, Type = "Integer", Value = "9" });
        Assert(!new FlowActions(FlowResources.Capture()).FindResource("constants/Setting").Methods.Single(method=>method.Id=="constant.read").Inputs[0].Options.Any(option => option.Value == key), "A draft constant entered the saved object snapshot.");
        FlowActions draft = new FlowActions(FlowResources.Capture(true));
        Assert((double)ConstantValueOf(draft, "Setting", key) == 12.75 && (long)ConstantValueOf(draft, "Recipe", key) == 9, "The two constant scopes lost their actual types.");
        store.Save("Setting"); store.Save("Recipe");
        FlowActions actions = new FlowActions(FlowResources.Capture());
        FlowStep pass = new FlowStep { Action = "value", Inputs = new System.Collections.Generic.List<FlowInput> { new FlowInput { Name = "Value", Source = "Parameter", Reference = ConstantStore.Reference("Setting", key) } } };
        Assert((double)ExecuteBound(actions, pass)["Value"] == 12.75, "Direct method input binding could not read the constants table.");
        FlowStep read = BoundStep(actions.FindResource("constants/Setting"), "constant.read"); read.Caption = "Read Setting constant"; read.Inputs[0].Reference = key;
        using (FlowNodeForm dialog = new FlowNodeForm(editor, read))
        {
            dialog.Show(); Application.DoEvents();
            ComboBox category = (ComboBox)dialog.Controls.Find("categoryValue", true)[0], target = (ComboBox)dialog.Controls.Find("targetValue", true)[0];
            Assert(Convert.ToString(category.SelectedValue) == "constants" && ((System.Collections.Generic.List<FlowChoice>)target.DataSource).Select(choice => choice.Id).OrderBy(id => id).SequenceEqual(new[] { "constants/Recipe", "constants/Setting" }), "Constant tables are hidden from the node object picker.");
            using (Bitmap image = new Bitmap(dialog.Width, dialog.Height)) { dialog.DrawToBitmap(image, new Rectangle(Point.Empty, dialog.Size)); image.Save(Path.Combine(root, "constant-table-node.png")); }
            dialog.Close();
        }
        string first = Path.Combine(SysPara.RecipeDataDirectory, "ConstantObject-A.xml"), second = Path.Combine(SysPara.RecipeDataDirectory, "ConstantObject-B.xml");
        constantForm.WriteRecipeData(first);
        store.Put("Recipe", key, new ConstantValue { Key = key, Type = "Integer", Value = "10" }); constantForm.WriteRecipeData(second);
        Assert((bool)host.GetMethod("OpenRecipe").Invoke(null, new object[] { first }), "Recipe A failed to open.");
        actions = new FlowActions(FlowResources.Capture()); Assert((long)ConstantValueOf(actions, "Recipe", key) == 9 && (double)ConstantValueOf(actions, "Setting", key) == 12.75, "Recipe A mixed constant scopes.");
        Assert((bool)host.GetMethod("OpenRecipe").Invoke(null, new object[] { second }), "Recipe B failed to open.");
        actions = new FlowActions(FlowResources.Capture()); Assert((long)ConstantValueOf(actions, "Recipe", key) == 10 && (double)ConstantValueOf(actions, "Setting", key) == 12.75, "Recipe B retained the previous recipe constant or changed Setting.");
        if (File.Exists(previous)) Assert((bool)host.GetMethod("OpenRecipe").Invoke(null, new object[] { previous }), "The previous recipe was not restored.");
        if (ConstantStore.Find(store.Table("Recipe"), key) != null) { store.Delete("Recipe", key); store.Save("Recipe"); }
        store.Delete("Setting", key); store.Save("Setting");
    }

    /// <summary>Protect read-node constant dependencies even when they use a selected key instead of Parameter source.</summary>
    private static void ConstantReadReferences()
    {
        string key = "Bound constant key"; ConstantStore store = constantForm.Constants;
        store.Put("Setting", null, new ConstantValue { Key = key, Type = "Number", Value = "1" }); store.Save("Setting");
        FlowStep consumer = editor.AddNode(); consumer.Action = "constant.read"; consumer.Target = "constants/Setting";
        consumer.Inputs = new System.Collections.Generic.List<FlowInput> { new FlowInput { Name = "Key", Reference = key } };
        bool blocked = false; try { store.Delete("Setting", key); } catch (InvalidOperationException) { blocked = true; }
        Assert(blocked, "A key selected by a constant read node was deleted.");
        blocked = false; try { store.Put("Setting", key, new ConstantValue { Key = key, Type = "Text", Value = "changed" }); } catch (InvalidOperationException) { blocked = true; }
        Assert(blocked, "A selected constant was retyped while referenced.");
        editor.Delete(consumer.Id); store.Delete("Setting", key); store.Save("Setting");
    }

    /// <summary>Open the clicked flow node through properties, then cancel without changing its definition.</summary>
    private static void VerifyNodeProperties(ContextMenuStrip menu, Control surface)
    {
        editor.SetEditing(true); bool opened = false; Exception failure = null; string before = model.Caption;
        using (System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer { Interval = 25 })
        {
            timer.Tick += (sender, args) => {
                FlowNodeForm dialog = Application.OpenForms.OfType<FlowNodeForm>().FirstOrDefault();
                if (dialog == null) return;
                timer.Stop();
                try
                {
                    TextBox caption = (TextBox)dialog.Controls.Find("captionValue", true)[0]; opened = true;
                    Assert(caption.Text == before, "Node properties used a different node."); caption.Text = "Discarded node edit";
                    ((Button)dialog.Controls.Find("cancel", true)[0]).PerformClick();
                }
                catch (Exception error) { failure = error; dialog.DialogResult = DialogResult.Cancel; }
            };
            timer.Start(); menu.Show(surface, new Point(5, 5)); Application.DoEvents(); menu.Items["nodeProperties"].PerformClick();
        }
        if (failure != null) throw failure;
        Assert(opened && model.Caption == before, "Node property cancel changed the flow definition.");
        editor.SetEditing(false); menu.Show(surface, new Point(5, 5)); Application.DoEvents();
        Assert(!menu.Items["nodeProperties"].Enabled, "Properties bypassed the flow-editing switch."); menu.Close();
    }
}
