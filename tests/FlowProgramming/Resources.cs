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
    /// <summary>Verify the fixed manual entry discovers only the current page's real dynamic axes and retains language and point data.</summary>
    private static void DynamicAxisManualControl()
    {
        string previousSettings = SysPara.SettingDataDirectory;
        SysPara.SettingDataDirectory = Path.Combine(root, "dynamic-axis-manual");
        Directory.CreateDirectory(SysPara.SettingDataDirectory);
        NP_Motor[] registered = SDKPara.ControlsSnapshot().OfType<NP_Motor>().ToArray();
        HardForm form = null;
        try
        {
            form = new HardForm();
            Assert(!SDKPara.ControlsSnapshot().OfType<NP_Motor>().Except(registered).Any(), "The page still creates an unconfigured sample axis.");
            Assert(form.Controls.Find("legacyResources", true).Length == 0 && form.Controls.Find("configuredResources", true).Length == 0
                && form.Controls.Find("X1_Group", true).Length == 0, "The old axis page or its page selector remains.");
            HardwareResourcePanel panel = (HardwareResourcePanel)form.Controls.Find("hardwareResourcePanel", true).Single();
            NP_ManualControl manual = (NP_ManualControl)form.Controls.Find("nP_ManualControl1", true).Single();
            Assert(manual.WorkMotor == null && manual.Parent.Name == "manualToolbar", "Manual control is not the fixed all-axis entry.");
            panel.BindOwner(form);
            HardwareDefinition firstDefinition = form.Resources.NewDefinition("Axis"); firstDefinition.Name = "First dynamic axis";
            HardwareEntry first = form.Resources.Put(firstDefinition);
            HardwareDefinition secondDefinition = form.Resources.NewDefinition("Axis"); secondDefinition.Name = "Second dynamic axis";
            HardwareEntry second = form.Resources.Put(secondDefinition);
            NP_AxisGroup firstGroup = (NP_AxisGroup)first.Group, secondGroup = (NP_AxisGroup)second.Group;
            firstGroup.Points.DataSet1.Rows.Add(123.5, "Keep this point");
            DataRow point = firstGroup.Points.DataSet1.Rows[0];
            form.Show(); Application.DoEvents();
            Control add = panel.Controls.Find("resourceAddAxis", true).Single();
            Assert(panel.Visible && manual.Visible && manual.RectangleToScreen(manual.ClientRectangle).Bottom <= add.RectangleToScreen(add.ClientRectangle).Top,
                "Manual control is hidden or not above the axis creation row.");
            foreach (LanguageType language in Enum.GetValues(typeof(LanguageType)))
            {
                SwitchLanguage(language);
                string expected = language == LanguageType.Chinese ? "手动调试" : language == LanguageType.English ? "Manual control" : "Control manual";
                Assert(manual.Text == expected && ReferenceEquals(point, firstGroup.Points.DataSet1.Rows[0]) && (double)point["Position"] == 123.5,
                    "Language switching changed the manual caption or lost a point draft.");
            }
            TabControl tabs = (TabControl)panel.Controls.Find("axisResourceTabs", true).Single(); tabs.SelectedIndex = 0;
            InspectManualDialog(manual, new[] { firstGroup.Motor, secondGroup.Motor });
            form.Resources.Delete(second.Definition.Id);
            HardwareDefinition renamed = first.Definition.Copy(); renamed.Name = "Renamed dynamic axis";
            HardwareEntry current = form.Resources.Put(renamed);
            InspectManualDialog(manual, new[] { (NP_Motor)current.Device });
            Assert(!((NP_Motor)current.Device).IsReady && ReferenceEquals(point, ((NP_AxisGroup)current.Group).Points.DataSet1.Rows[0]), "Manual discovery initialized a driver or resource editing lost point data.");
            using (Bitmap image = new Bitmap(form.Width, form.Height))
            {
                form.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size)); image.Save(Path.Combine(root, "dynamic-axis-only.png"));
            }
        }
        finally
        {
            if (form != null) { form.Dispose(); AlphaRapLibrary.ModuleManager.ModuleList.Remove(form); }
            SysPara.SettingDataDirectory = previousSettings; SwitchLanguage(LanguageType.Chinese);
        }
    }

    /// <summary>Click the actual SDK button and inspect its dialog without selecting a motion operation or initializing hardware.</summary>
    private static void InspectManualDialog(NP_ManualControl manual, NP_Motor[] expected)
    {
        MotorControlForm opened = null; Exception failure = null;
        using (Timer timer = new Timer { Interval = 20 })
        {
            timer.Tick += delegate
            {
                opened = Application.OpenForms.OfType<MotorControlForm>().FirstOrDefault();
                if (opened == null) return;
                timer.Stop();
                try
                {
                    IEnumerable entries = (IEnumerable)typeof(MotorControlForm).GetField("lstMotor", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(opened);
                    NP_Motor[] actual = entries.Cast<object>().Select(entry => (NP_Motor)entry.GetType().GetField("Motor").GetValue(entry)).ToArray();
                    Assert(actual.Length == expected.Length && expected.All(actual.Contains), "Manual control missed a hidden dynamic tab or included a foreign/deleted motor.");
                    Control buttons = opened.Controls.Find("plMotorButton", true).Single();
                    Assert(expected.All(motor => buttons.Controls.Cast<Control>().Any(button => button.Text == motor.Text)), "Manual choices use stale axis names.");
                }
                catch (Exception error) { failure = error; }
                opened.Close();
            };
            timer.Start(); manual.PerformClick(); timer.Stop();
        }
        if (failure != null) throw failure;
        Assert(opened != null && opened.IsDisposed, "The fixed manual button did not open or release its dialog.");
    }

    /// <summary>Old resource files gain one default IO tab; grouping changes are Setting drafts and invalid references are rejected.</summary>
    private static void IoTabPersistence()
    {
        string previousSettings = SysPara.SettingDataDirectory;
        SysPara.SettingDataDirectory = Path.Combine(root, "io-tab-persistence"); Directory.CreateDirectory(SysPara.SettingDataDirectory);
        HardForm form = null;
        try
        {
            HardwareDefinition legacy = hardware.Resources.NewDefinition("Input"); legacy.IoPageId = "";
            string path = Path.Combine(SysPara.SettingDataDirectory, "HardwareResources.xml");
            using (FileStream output = File.Create(path)) new System.Xml.Serialization.XmlSerializer(typeof(HardwareProject)).Serialize(output, new HardwareProject { Resources=new System.Collections.Generic.List<HardwareDefinition> { legacy } });
            System.Xml.Linq.XDocument oldFile = System.Xml.Linq.XDocument.Load(path);
            oldFile.Root.Element("IoPages").Remove(); oldFile.Root.Element("Resources").Elements().Single().Element("IoPageId").Remove(); oldFile.Save(path);
            form = new HardForm(); ((HardwareResourcePanel)form.Controls.Find("hardwareResourcePanel", true).Single()).BindOwner(form);
            HardwareResources resources = form.Resources;
            Assert(resources.IoPages.Single().Id == "default" && resources.Entries.Single().Definition.IoPageId == "default", "A legacy IO was lost or assigned to a missing tab.");
            HardwareIoPage station = resources.AddIoPage("装配工位");
            HardwareEntry input = resources.Entries.Single(); Control actual = input.Group; object driver = input.Device;
            resources.MoveIoToPage(input.Definition.Id, station.Id); resources.RenameIoPage(station.Id, "扫码工位");
            Assert(ReferenceEquals(actual, input.Group) && ReferenceEquals(driver, input.Device) && !resources.RequiresRestart, "Regrouping IO replaced its device or requested hardware restart.");
            bool blocked = false; try { resources.DeleteIoPage(station.Id); } catch (InvalidOperationException) { blocked = true; }
            Assert(blocked, "A nonempty tab deleted its IO.");
            blocked = false; try { resources.AddIoPage("扫码工位"); } catch (InvalidOperationException) { blocked = true; }
            Assert(blocked, "Duplicate IO tab names were accepted.");
            HardwareDefinition bad = input.Definition.Copy(); bad.IoPageId = Guid.NewGuid().ToString("N");
            blocked = false; try { resources.Put(bad); } catch (InvalidOperationException) { blocked = true; }
            Assert(blocked && ReferenceEquals(driver, resources.Entries.Single().Device), "Invalid tab membership replaced live IO.");
            resources.Save(); byte[] saved = File.ReadAllBytes(path);
            HardwareIoPage detached = resources.IoPages.First(); detached.Name = "External mutation";
            resources.RenameIoPage(station.Id, "Draft name"); resources.MoveIoToPage(input.Definition.Id, "default"); resources.DeleteIoPage(station.Id);
            Assert(resources.IsDirty && File.ReadAllBytes(path).SequenceEqual(saved), "Editing tabs bypassed Main Save.");
            resources.DiscardChanges();
            Assert(resources.IoPages.Single(page => page.Id == station.Id).Name == "扫码工位" && resources.Entries.Single().Definition.IoPageId == station.Id && !resources.IsDirty, "Discard failed to restore saved tabs and membership.");
            resources.DeleteIoPage("default");
            resources.MoveIoToPage(input.Definition.Id, station.Id); resources.Delete(input.Definition.Id);
            blocked = false; try { resources.DeleteIoPage(station.Id); } catch (InvalidOperationException) { blocked = true; }
            Assert(blocked, "The last IO tab was deleted.");
            HardwareProject corrupt = HardwareResources.Load(path); corrupt.Resources.Single().IoPageId = Guid.NewGuid().ToString("N");
            string invalid = Path.Combine(SysPara.SettingDataDirectory, "Invalid.xml");
            using (FileStream output = File.Create(invalid)) new System.Xml.Serialization.XmlSerializer(typeof(HardwareProject)).Serialize(output, corrupt);
            blocked = false; try { HardwareResources.Load(invalid); } catch (InvalidDataException) { blocked = true; }
            Assert(blocked && File.ReadAllBytes(path).SequenceEqual(saved), "Missing tab references were silently accepted or overwrote saved data.");
        }
        finally
        {
            if (form != null) { form.Dispose(); AlphaRapLibrary.ModuleManager.ModuleList.Remove(form); }
            SysPara.SettingDataDirectory = previousSettings;
        }
    }

    /// <summary>Exercise actual add-tab and IO editor buttons, tab hit testing, moving menus and multilingual labels.</summary>
    private static void IoTabInterface()
    {
        string previousSettings = SysPara.SettingDataDirectory;
        SysPara.SettingDataDirectory = Path.Combine(root, "io-tab-interface"); Directory.CreateDirectory(SysPara.SettingDataDirectory);
        HardForm owner = null;
        try
        {
            owner = new HardForm(); ((HardwareResourcePanel)owner.Controls.Find("hardwareResourcePanel", true).Single()).BindOwner(owner);
            using (Form fixture = new Form { Size=new Size(1050, 650) })
            using (HardwareResourcePanel panel = new HardwareResourcePanel { ResourceKind="IO", Dock=DockStyle.Fill })
            {
                fixture.Controls.Add(panel); panel.BindResources(owner.Resources); fixture.Show(); Application.DoEvents();
                TabControl tabs = (TabControl)panel.Controls.Find("ioResourceTabs", true).Single();
                CompleteIoName(() => ((Button)panel.Controls.Find("resourceAddIoPage", true).Single()).PerformClick(), "Cancelled group", false);
                Assert(tabs.TabCount == 1 && !owner.Resources.IsDirty, "Cancelling Add tab changed the resource draft.");
                CompleteIoName(() => ((Button)panel.Controls.Find("resourceAddIoPage", true).Single()).PerformClick(), "工位 A");
                string stationId = tabs.SelectedTab.Tag as string;
                Assert(tabs.TabCount == 2 && tabs.SelectedTab.Text == "工位 A", "Add tab did not select the new group.");
                using (Timer timer = new Timer { Interval=20 })
                {
                    timer.Tick += delegate { HardwareEditForm dialog = Application.OpenForms.OfType<HardwareEditForm>().FirstOrDefault(); if (dialog == null) return; timer.Stop(); ((Button)dialog.Controls.Find("hardwareOk", true).Single()).PerformClick(); };
                    timer.Start(); ((Button)panel.Controls.Find("resourceAddInput", true).Single()).PerformClick();
                }
                HardwareEntry input = owner.Resources.Entries.Single(); object actual = input.Device;
                Assert(input.Definition.IoPageId == stationId && input.Group.Visible, "New IO did not belong to the selected tab.");
                HardwareDefinition output = owner.Resources.NewDefinition("Output"); owner.Resources.Put(output);
                tabs.SelectedTab = tabs.TabPages.Cast<TabPage>().Single(page => page.Tag as string == "default"); Application.DoEvents();
                Assert(!input.Group.Visible, "IO from another tab remained visible.");
                tabs.SelectedTab = tabs.TabPages.Cast<TabPage>().Single(page => page.Tag as string == stationId); Application.DoEvents();
                foreach (LanguageType language in Enum.GetValues(typeof(LanguageType)))
                {
                    SwitchLanguage(language);
                    Assert(tabs.SelectedTab.Text == "工位 A" && input.Group.Visible && ReferenceEquals(actual, input.Device), "Translation rebuilt IO or translated the user's tab name.");
                }
                ContextMenuStrip menu = input.Group.ContextMenuStrip; menu.Show(input.Group, new Point(5, 5)); Application.DoEvents();
                ToolStripMenuItem move = (ToolStripMenuItem)menu.Items["resourceMove"];
                move.DropDownItems["moveIo_default"].PerformClick(); menu.Close(); Application.DoEvents();
                Assert(input.Definition.IoPageId == "default" && !input.Group.Visible && ReferenceEquals(actual, input.Device), "Moving IO changed its instance or failed to hide it from the old page.");
                Rectangle tab = tabs.GetTabRect(tabs.SelectedIndex);
                RaiseMouse(tabs, "OnMouseDown", new MouseEventArgs(MouseButtons.Right, 1, tab.Left+4, tab.Top+4, 0));
                tabs.ContextMenuStrip.Show(tabs, new Point(tab.Left+4, tab.Bottom)); Application.DoEvents();
                Assert(tabs.ContextMenuStrip.Items["ioPageDelete"].Enabled && tabs.ContextMenuStrip.Items["ioPageDelete"].ToolTipText == "", "An empty optional IO tab cannot be deleted or shows a stale restriction.");
                CompleteIoName(() => tabs.ContextMenuStrip.Items["ioPageRename"].PerformClick(), "工位 B"); tabs.ContextMenuStrip.Close();
                Assert(tabs.SelectedTab.Text == "工位 B", "Right-click renamed a different tab.");
                tabs.SelectedTab = tabs.TabPages.Cast<TabPage>().Single(page => page.Tag as string == "default");
                SwitchLanguage(LanguageType.Chinese); Application.DoEvents(); Screenshot(fixture, "io-tabs-grouped");
                tabs.SelectedTab = tabs.TabPages.Cast<TabPage>().Single(page => page.Tag as string == stationId);
                owner.Resources.DeleteIoPage(stationId);
                Assert(tabs.TabCount == 1 && tabs.SelectedTab.Tag as string == "default" && input.Group.Visible && !input.Group.IsDisposed, "Deleting an empty selected tab disposed the shared IO canvas.");
                using (Bitmap image = new Bitmap(fixture.Width, fixture.Height)) { fixture.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size)); image.Save(Path.Combine(root, "io-tabs.png")); }
            }
        }
        finally
        {
            if (owner != null) { owner.Dispose(); AlphaRapLibrary.ModuleManager.ModuleList.Remove(owner); }
            SysPara.SettingDataDirectory = previousSettings; SwitchLanguage(LanguageType.Chinese);
        }
    }

    /// <summary>Finish the reused input dialog through its real OK button while preserving normal modal behavior.</summary>
    private static void CompleteIoName(Action open, string text, bool accept = true)
    {
        bool shown = false;
        using (Timer timer = new Timer { Interval=20 })
        {
            timer.Tick += delegate
            {
                FlowInputForm dialog = Application.OpenForms.OfType<FlowInputForm>().FirstOrDefault();
                if (dialog == null) return;
                timer.Stop(); shown = true; dialog.InputValue = text;
                foreach (LanguageType language in Enum.GetValues(typeof(LanguageType)))
                {
                    SwitchLanguage(language);
                    Assert(dialog.InputValue == text && dialog.Controls.Find("prompt_dyn", true).Single().Text == HardwareText.T("ioPageName"), "The tab name editor lost input or kept a stale language prompt.");
                }
                ((Button)dialog.Controls.Find(accept ? "ok" : "cancel", true).Single()).PerformClick();
            };
            timer.Start(); open(); timer.Stop();
        }
        Assert(shown, "The IO tab command did not open its name editor.");
    }

    /// <summary>Each axis has one reusable tab; selecting and translating the page never recreates motors or point tables.</summary>
    private static void AxisResourceTabs()
    {
        string previousSettings = SysPara.SettingDataDirectory;
        string folder = Path.Combine(root, "axis-tabs"); Directory.CreateDirectory(folder);
        SysPara.SettingDataDirectory = folder;
        HardForm form = null;
        try
        {
            form = new HardForm();
            HardwareResourcePanel panel = (HardwareResourcePanel)form.Controls.Find("hardwareResourcePanel", true)[0];
            panel.BindOwner(form);
            HardwareResources resources = panel.Resources;
            TabControl tabs = (TabControl)panel.Controls.Find("axisResourceTabs", true)[0];
            Assert(tabs.TabCount == 0 && panel.Controls.Find("resourceSelected", true).Length == 0, "Axis view retains duplicate selectors or sample devices.");
            foreach (string name in new[] { "X 轴", "Y axis", "旋转轴" })
            {
                HardwareDefinition definition = resources.NewDefinition("Axis"); definition.Name = name;
                HardwareEntry created = resources.Put(definition);
                Assert(((HardwareDefinition)tabs.SelectedTab.Tag).Id == created.Definition.Id, "New axis did not select its own tab.");
                form.RecipeData.Tables[HardwareResources.PointsName(created.Definition.Id)].Rows.Add(100d, "Point A");
            }
            HardwareEntry[] axes = resources.Entries.ToArray();
            TabPage[] pages = tabs.TabPages.Cast<TabPage>().ToArray();
            Assert(pages.Length == 3 && pages.Select(page => page.Text).SequenceEqual(axes.Select(entry => entry.Definition.Name)), "Axis tabs lost their configured order or names.");
            form.Show(); Application.DoEvents();
            tabs.SelectedTab = pages[0];
            NP_AxisGroup axis = (NP_AxisGroup)axes[0].Group;
            NP_Motor motor = axis.Motor;
            DataTable points = form.RecipeData.Tables[HardwareResources.PointsName(axes[0].Definition.Id)];
            DataRow point = points.Rows[0];
            foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
            {
                foreach (Size size in new[] { new Size(1280, 740), new Size(900, 620), new Size(680, 520) })
                {
                    form.ClientSize = size; SwitchLanguage(language); Application.DoEvents();
                    tabs.SelectedTab = pages[1]; tabs.SelectedTab = pages[0]; Application.DoEvents();
                    Assert(ReferenceEquals(axis.Motor, motor) && ReferenceEquals(points.Rows[0], point) && (double)point["Position"] == 100d, "Switching axis tabs or language recreated a motor or lost point edits.");
                    Assert(pages.Select(page => page.Text).SequenceEqual(axes.Select(entry => entry.Definition.Name)), "Language switching translated business axis names.");
                    Assert(axis.Parent == pages[0] && axis.Dock == DockStyle.Fill && axis.Width >= axis.MinimumSize.Width && axis.Height >= axis.MinimumSize.Height, "Selected axis does not occupy its tab or clips its minimum layout.");
                    Assert(axes.Skip(1).All(entry => !entry.Group.Visible) && !panel.Controls.Find("resourceCanvas", true)[0].Visible, "Inactive axes are still tiled into the view.");
                    using (Bitmap screenshot = new Bitmap(form.Width, form.Height))
                    {
                        form.DrawToBitmap(screenshot, new Rectangle(Point.Empty, form.Size));
                        screenshot.Save(Path.Combine(root, "axis-tabs-" + size.Width + "-" + language + ".png"));
                    }
                }
            }
            form.ClientSize = new Size(1280, 740); Application.DoEvents();
            Rectangle tabHeader = tabs.GetTabRect(1); Point hit = new Point(tabHeader.Left + 4, tabHeader.Top + 4);
            typeof(Control).GetMethod("OnMouseDown", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(tabs, new object[] { new MouseEventArgs(MouseButtons.Right, 1, hit.X, hit.Y, 0) });
            tabs.ContextMenuStrip.Show(tabs, hit); Application.DoEvents();
            Assert((string)typeof(HardwareResourcePanel).GetField("contextResourceId", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(panel) == axes[1].Definition.Id, "Tab context menu targeted a different axis.");
            tabs.ContextMenuStrip.Close();
            HardwareEntry selectedAxis = (HardwareEntry)typeof(HardwareResourcePanel).GetMethod("Selected", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(panel, null);
            Assert(selectedAxis.Definition.Id == axes[1].Definition.Id, "Parameter button ignores the active axis tab.");
            resources.Save();
            HardwareDefinition rename = axes[1].Definition.Copy(); rename.Name = "Renamed Y"; resources.Put(rename);
            Assert(ReferenceEquals(tabs.SelectedTab, pages[1]) && pages[1].Text == "Renamed Y", "Editing axis properties recreated or switched its tab.");
            resources.Delete(axes[2].Definition.Id); Assert(tabs.TabCount == 2 && pages[2].IsDisposed, "Deleted axis retained an empty tab.");
            resources.DiscardChanges();
            Assert(tabs.TabCount == 3 && tabs.TabPages.Cast<TabPage>().Select(page => page.Text).SequenceEqual(new[] { "X 轴", "Y axis", "旋转轴" }), "Discard did not restore saved axis tabs exactly once.");
            resources.Save(); resources.DiscardChanges();
            Assert(tabs.TabCount == 3 && resources.Entries.All(entry => entry.Group.Parent is TabPage && !entry.Group.IsDisposed), "Reload duplicated tabs or disposed the live axis.");
            string firstSavedId = HardwareResources.Load(resources.PathName).Resources[0].Id;
            form.Close(); form.Dispose(); AlphaRapLibrary.ModuleManager.ModuleList.Remove(form);
            form = new HardForm(); panel = (HardwareResourcePanel)form.Controls.Find("hardwareResourcePanel", true)[0]; panel.BindOwner(form);
            tabs = (TabControl)panel.Controls.Find("axisResourceTabs", true)[0];
            Assert(tabs.TabCount == 3 && tabs.SelectedTab == tabs.TabPages[0] && ((HardwareDefinition)tabs.SelectedTab.Tag).Id == firstSavedId, "Restart did not restore each saved axis once and select the first persisted tab.");
        }
        finally
        {
            if (form != null) { form.Close(); form.Dispose(); AlphaRapLibrary.ModuleManager.ModuleList.Remove(form); }
            SysPara.SettingDataDirectory = previousSettings; SwitchLanguage(LanguageType.Chinese);
        }
    }

}
