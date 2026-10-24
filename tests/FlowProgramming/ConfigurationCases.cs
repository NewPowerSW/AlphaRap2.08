using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.Xml;
using Cognex.VisionPro;
using Cognex.VisionPro.ToolBlock;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using AlphaRap;
using AlphaRap.Configuration;
using AlphaRap.FlowProgramming;
using AlphaRapLibrary;
using NPSDK;
using NPSDK.Motion;

internal static partial class ConfigurationCases
{
    internal sealed class ParticipantProbe
    {
        internal string Name;
        internal Func<IEnumerable<string>> Paths;
        internal Action<int> Change;
        internal Action<int> AssertDraft;
        internal Action<int> AssertPublished;
        internal Action<int> AssertReloaded;
    }

    private static MainForm main;
    private static Type host;
    private static SystemSetting settings;
    private static HardForm hardware;
    private static DataForm data;
    private static FlowModuleEditor module;
    private static string axisId;
    private static readonly List<ParticipantProbe> participants = new List<ParticipantProbe>();
    private static int completed;

    /// <summary>Exercise real configuration transactions in the disposable startup fixture without initializing any hardware.</summary>
    internal static void Run(MainForm window, Type middleLayer, SystemSetting systemSettings)
    {
        main = window; host = middleLayer; settings = systemSettings;
        SysPara.UserPermission = PermissionType.Administrator;
        hardware = (HardForm)host.GetField("HardF", BindingFlags.Static | BindingFlags.Public).GetValue(null);
        data = (DataForm)host.GetField("DataF", BindingFlags.Static | BindingFlags.Public).GetValue(null);
        HardwareDefinition definition = hardware.Resources.NewDefinition("Axis");
        axisId = definition.Id;
        hardware.Resources.Put(definition);
        module = FlowPrograms.Attach(FlowPrograms.AddModule("Transaction fixture"));
        FlowStep step = module.AddNode(); step.ExecutionMode = "None"; step.Inputs.Clear();
        module.Program.Mode = "Program"; module.Program.AutoEntry = step.Id;
        RegisterCoreParticipant();
        RegisterAdditionalParticipants(participants, main, host);
        Test("a missing permission database rejects preparation without accepting or creating a replacement", MissingPermissionDraft);
        Test("all registered configuration files commit and publish only after Main Save", SaveAll);
        Test("all three languages preserve configuration drafts and the saved files", LanguageDrafts);
        Test("Main Save Cancel preserves draft values, files and dirty flags", CancelSave);
        Test("closing Main Save preserves draft values, files and dirty flags", CloseSave);
        Test("leaving tray and log pages preserves drafts and dirty flags when Main Save is cancelled", CancelPageLeaveSave);
        Test("staging failure preserves every original file and every committed snapshot", StagingFailure);
        Test("middle replacement failure rolls back all previously replaced files", MiddleFailure);
        Test("last replacement failure rolls back all previously replaced files", LastFailure);
        Test("a failed late replacement removes newly created VPP files and retains the draft", NewFileFailure);
        Test("missing previously saved VPP rejects discard and retains all configuration drafts", MissingSavedVpp);
        Test("invalid device XML or permission MDB rejects discard before any live draft changes", InvalidDiscardSources);
        Test("deleted visual stations retain their recipe guard and are restored by discard", DeletedStationDraft);
        Test("Main Save No reloads the last complete transaction and discards all drafts", DiscardAll);
        Test("reloading a saved transaction restores all registered participant values", ReloadAll);
        Test("Setting persists independently while Recipe values follow the selected recipe file", RecipeScope);
        Test("a never-saved ToolBlock can discard its in-memory contents without creating a file", NeverSavedVpp);
        Console.WriteLine("PASS unified save: " + completed + " cases; participants: " + string.Join(", ", participants.Select(item => item.Name)));
    }

    /// <summary>Allow new configuration owners to register probes without changing the common transaction assertions.</summary>
    static partial void RegisterAdditionalParticipants(List<ParticipantProbe> probes, MainForm window, Type middleLayer);

    /// <summary>Register actual constants, point tables, resource definitions, production schema and flow versions.</summary>
    private static void RegisterCoreParticipant()
    {
        participants.Add(new ParticipantProbe { Name = "Core", Paths = CorePaths, Change = ChangeCore,
            AssertDraft = AssertCoreDraft, AssertPublished = AssertCorePublished, AssertReloaded = AssertCoreReloaded });
    }

    /// <summary>List every core file whose generation is changed by the fixture.</summary>
    private static IEnumerable<string> CorePaths()
    {
        return new[] { Path.Combine(ModuleManager.SettingDataDirectory, settings.Text + ".xml"), SysPara.FilePath,
            hardware.Resources.PathName, (string)typeof(DataForm).GetProperty("ProductionSchemaPath", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null, null), FlowPrograms.FilePath };
    }

    /// <summary>Change actual drafts while retaining the last accepted DataRow and executable flow versions.</summary>
    private static void ChangeCore(int generation)
    {
        string text = generation.ToString(System.Globalization.CultureInfo.InvariantCulture);
        settings.Constants.Put("Setting", ConstantStore.Find(settings.Constants.Table("Setting"), "TxnSetting") == null ? null : "TxnSetting",
            new ConstantValue { Key = "TxnSetting", Type = "Integer", Value = text });
        settings.Constants.Put("Recipe", ConstantStore.Find(settings.Constants.Table("Recipe"), "TxnRecipe") == null ? null : "TxnRecipe",
            new ConstantValue { Key = "TxnRecipe", Type = "Integer", Value = text });
        NP_AxisGroup axis = (NP_AxisGroup)hardware.Resources.Entries.Single(item => item.Definition.Id == axisId).Group;
        if (axis.Points.DataSet1.Rows.Count == 0) axis.Points.DataSet1.Rows.Add(generation * 10.0, "Generation " + text);
        else { axis.Points.DataSet1.Rows[0]["Position"] = generation * 10.0; axis.Points.DataSet1.Rows[0]["Annotation"] = "Generation " + text; }
        HardwareDefinition definition = hardware.Resources.Entries.Single(item => item.Definition.Id == axisId).Definition.Copy();
        definition.Name = "Transaction axis " + text;
        hardware.Resources.Put(definition);
        data.SetProductionDraft(new List<ProductionField> { new ProductionField { Name = "Barcode" }, new ProductionField { Name = "Result" }, new ProductionField { Name = "Value" + text, Type = "Number" } });
        module.Program.Steps.Single().Caption = "Generation " + text;
    }

    /// <summary>Check editable values independently of published snapshots and disk files.</summary>
    private static void AssertCoreDraft(int generation)
    {
        string text = generation.ToString(System.Globalization.CultureInfo.InvariantCulture);
        Assert(Convert.ToString(ConstantStore.Find(settings.Constants.Table("Setting"), "TxnSetting")["Value"]) == text, "Setting draft changed.");
        Assert(Convert.ToString(ConstantStore.Find(settings.Constants.Table("Recipe"), "TxnRecipe")["Value"]) == text, "Recipe draft changed.");
        NP_AxisGroup axis = (NP_AxisGroup)hardware.Resources.Entries.Single(item => item.Definition.Id == axisId).Group;
        Assert(axis.Points.DataSet1.Rows[0].Field<double>("Position") == generation * 10.0, "Point draft changed.");
        Assert(hardware.Resources.Entries.Single(item => item.Definition.Id == axisId).Definition.Name == "Transaction axis " + text, "Resource draft changed.");
        Assert(data.GetProductionDraft().Any(field => field.Name == "Value" + text), "Production schema draft changed.");
        Assert(module.Program.Steps.Single().Caption == "Generation " + text, "Flow draft changed.");
    }

    /// <summary>Check committed constants, production metadata and the private saved flow snapshot without executing hardware.</summary>
    private static void AssertCorePublished(int generation)
    {
        Assert(Convert.ToInt64(settings.Constants.Committed("Setting").Single(item => item.Key == ConstantStore.Reference("Setting", "TxnSetting")).Value) == generation, "Setting was accepted too early or failed to publish.");
        Assert(Convert.ToInt64(settings.Constants.Committed("Recipe").Single(item => item.Key == ConstantStore.Reference("Recipe", "TxnRecipe")).Value) == generation, "Recipe was accepted too early or failed to publish.");
        Assert(data.GetProductionSchema().Any(field => field.Name == "Value" + generation), "Production schema was accepted too early or failed to publish.");
        FlowProgram published = (FlowProgram)typeof(FlowModuleEditor).GetField("published", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(module);
        Assert(published.Steps.Single().Caption == "Generation " + generation, "The executable flow snapshot changed before the transaction committed.");
    }

    /// <summary>Check values loaded back from the real files and confirm every restored draft is accepted.</summary>
    private static void AssertCoreReloaded(int generation)
    {
        AssertCoreDraft(generation); AssertCorePublished(generation);
        Assert(!hardware.Resources.IsDirty, "Reloaded hardware resources remained dirty.");
        Assert(settings.Constants.Table("Setting").GetChanges() == null && settings.Constants.Table("Recipe").GetChanges() == null, "Reloaded constants were not accepted.");
        Assert(HardwareResources.Load(hardware.Resources.PathName).Resources.Single(item => item.Id == axisId).Name == "Transaction axis " + generation, "Resource file did not reload the saved generation.");
        Assert(FlowProgramFile.Load(FlowPrograms.FilePath).Modules.Single(item => item.ModuleId == module.Program.ModuleId).Steps.Single().Caption == "Generation " + generation, "Flow file did not reload the saved generation.");
    }

    /// <summary>Persist every registered configuration owner through the real MainForm Save confirmation.</summary>
    private static void SaveAll()
    {
        ChangeAll(1); ProjectConfiguration.Validate(); List<string> paths = Manifest(); ChooseSave("Button1_dyn");
        Assert(paths.Count == paths.Distinct(StringComparer.OrdinalIgnoreCase).Count(), "The transaction contains duplicate destination paths.");
        foreach (ParticipantProbe probe in participants)
        {
            foreach (string path in probe.Paths()) Assert(paths.Contains(Path.GetFullPath(path), StringComparer.OrdinalIgnoreCase) && File.Exists(path), "Participant is missing from the transaction: " + probe.Name + " / " + path);
            probe.AssertDraft(1); probe.AssertPublished(1); probe.AssertReloaded(1);
        }
        AssertNoTemporaryFiles();
    }

    /// <summary>Switch registered languages while unsaved values remain editable and persisted bytes remain unchanged.</summary>
    private static void LanguageDrafts()
    {
        ChangeAll(2); Dictionary<string, byte[]> before = Snapshot();
        foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
        {
            host.GetMethod("SwitchLanguage", BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { language });
            Application.DoEvents();
            foreach (ParticipantProbe probe in participants) { probe.AssertDraft(2); probe.AssertPublished(1); }
            AssertFiles(before);
        }
        SysPara.UserPermission = PermissionType.Administrator;
    }

    /// <summary>Cancel the real save dialog and retain every unsaved participant and modification counter.</summary>
    private static void CancelSave()
    {
        PreserveCancelledSave("Button2_dyn");
    }

    /// <summary>Close the real save dialog through its window-close path and retain every unsaved participant.</summary>
    private static void CloseSave()
    {
        PreserveCancelledSave("Close");
    }

    /// <summary>Verify dismissing confirmation performs neither persistence nor discard and does not clear dirty flags.</summary>
    private static void PreserveCancelledSave(string choice)
    {
        Dictionary<string, byte[]> before = Snapshot(); SysPara.items = 37; SysPara.items2 = 39;
        ChooseSave(choice); AssertFiles(before);
        foreach (ParticipantProbe probe in participants) { probe.AssertDraft(2); probe.AssertPublished(1); }
        Assert(SysPara.items == 37 && SysPara.items2 == 39, "Cancelled Main Save cleared modification counters.");
        Assert(hardware.Resources.IsDirty && vision.HasRecipeDraft, "Cancelled Main Save cleared pending configuration markers.");
    }

    /// <summary>Invoke both actual page-leave handlers and require cancelling their Main Save dialog to retain every pending value.</summary>
    private static void CancelPageLeaveSave()
    {
        string[] fields = { "PlatF", "LogF" };
        string[] methods = { "PlatFormSetting_Leave", "MESForm_Leave" };
        for (int index = 0; index < fields.Length; index++)
        {
            Form page = (Form)host.GetField(fields[index], BindingFlags.Static | BindingFlags.Public).GetValue(null);
            MethodInfo leave = page.GetType().GetMethod(methods[index], BindingFlags.Instance | BindingFlags.NonPublic);
            Assert(leave != null, "The real page-leave handler changed: " + fields[index] + "." + methods[index]);
            Dictionary<string, byte[]> before = Snapshot(); SysPara.items = 37; SysPara.items2 = 39;
            ChooseSave("Button2_dyn", () => leave.Invoke(page, new object[] { page, EventArgs.Empty }));
            AssertFiles(before);
            foreach (ParticipantProbe probe in participants) { probe.AssertDraft(2); probe.AssertPublished(1); }
            Assert(SysPara.items == 37 && SysPara.items2 == 39, "Cancelled page-leave save cleared modification counters: " + fields[index]);
            Assert(hardware.Resources.IsDirty && vision.HasRecipeDraft, "Cancelled page-leave save cleared pending configuration markers: " + fields[index]);
        }
    }

    /// <summary>Deny the final source read so that staging fails before any formal file replacement.</summary>
    private static void StagingFailure()
    {
        Failure(Manifest().Last(), FileShare.None);
    }

    /// <summary>Deny deletion of a middle file while permitting staging reads and verify rollback of earlier replacements.</summary>
    private static void MiddleFailure()
    {
        List<string> paths = Manifest(); Failure(paths[Math.Max(1, paths.Count / 2)], FileShare.Read);
    }

    /// <summary>Deny the final replacement after every earlier replacement has succeeded and verify a complete rollback.</summary>
    private static void LastFailure()
    {
        Failure(Manifest().Last(), FileShare.Read);
    }

    /// <summary>Inject a Windows sharing violation and verify disk, published state and drafts as separate invariants.</summary>
    private static void Failure(string blockedPath, FileShare sharing)
    {
        Dictionary<string, byte[]> before = Snapshot(); bool failed = false; SysPara.items = 37; SysPara.items2 = 39;
        using (FileStream held = File.Open(blockedPath, FileMode.Open, FileAccess.Read, sharing))
        {
            try { ProjectConfiguration.Save(); }
            catch (IOException) { failed = true; }
            catch (AggregateException error)
            {
                if (!error.Flatten().InnerExceptions.All(item => item is IOException)) throw;
                failed = true;
            }
        }
        Assert(failed, "The locked file did not fail the real Save transaction: " + blockedPath);
        AssertFiles(before);
        foreach (ParticipantProbe probe in participants) { probe.AssertDraft(2); probe.AssertPublished(1); }
        Assert(SysPara.items == 37 && SysPara.items2 == 39 && hardware.Resources.IsDirty && vision.HasRecipeDraft, "Failed Save cleared draft markers or modification counters.");
        AssertNoTemporaryFiles();
    }

    /// <summary>Require rollback to delete VPP destinations that did not exist before the failed transaction.</summary>
    private static void NewFileFailure()
    {
        VpCameraConfig camera = VisionConfig().Cameras.Single(item => item.Name == CameraName);
        VpVppConfig added = new VpVppConfig { Name = "Never committed", FolderName = "NeverCommitted" };
        added.Station = new VpStation(camera.StorageFolder, added.StorageFolder) { Config = added };
        string target = added.Station.GetVppPath(SysPara.RecipeName); string error;
        Assert(added.Station.TryCreateTB(target, out error), error);
        camera.Vpps.Add(added);
        try
        {
            Failure(DatabasePath(), FileShare.Read);
            Assert(!File.Exists(target) && added.Station.TB != null && camera.Vpps.Contains(added), "Late rollback lost a new VPP draft or left its formal file.");
        }
        finally
        {
            CogToolBlock discarded = added.Station == null ? null : added.Station.TB;
            typeof(VPForm).GetMethod("RemoveVpp", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(vision, new object[] { camera, added, null });
            if (discarded != null) discarded.Dispose();
        }
    }

    /// <summary>Delete a previously committed source temporarily and require discard failure to preserve every unsaved participant.</summary>
    private static void MissingSavedVpp()
    {
        Dictionary<string, byte[]> before = Snapshot();
        string path = VisionVpp().Station.GetVppPath(SysPara.RecipeName);
        byte[] original = File.ReadAllBytes(path); bool rejected = false;
        try
        {
            File.Delete(path);
            try { ProjectConfiguration.Discard(); }
            catch (InvalidOperationException) { rejected = true; }
            Assert(rejected && vision.HasRecipeDraft, "Discard silently cleared a previously saved VPP whose source was missing.");
            foreach (ParticipantProbe probe in participants) probe.AssertDraft(2);
        }
        finally { File.WriteAllBytes(path, original); }
        AssertFiles(before);
        foreach (ParticipantProbe probe in participants) probe.AssertPublished(1);
    }

    /// <summary>Require both external-source preflights to finish before visual, device, permissions or module drafts are discarded.</summary>
    private static void InvalidDiscardSources()
    {
        InvalidDiscardSource(DeviceControl.XmlFilePath, System.Text.Encoding.UTF8.GetBytes("<DeviceControl><broken"));
        InvalidDiscardSource(DatabasePath(), System.Text.Encoding.UTF8.GetBytes("Not an Access database"));
    }

    /// <summary>Corrupt one source temporarily and verify that a rejected discard leaves all live drafts and all other files unchanged.</summary>
    private static void InvalidDiscardSource(string path, byte[] invalid)
    {
        Dictionary<string, byte[]> before = Snapshot();
        byte[] original = File.ReadAllBytes(path); bool rejected = false;
        try
        {
            File.WriteAllBytes(path, invalid);
            try { ProjectConfiguration.Discard(); }
            catch (Exception error)
            {
                if (!(error is InvalidOperationException) && !(error is IOException) && !(error is OleDbException)) throw;
                rejected = true;
            }
            Assert(rejected && vision.HasRecipeDraft && hardware.Resources.IsDirty, "Rejected discard cleared a live draft before preflight completed.");
            foreach (ParticipantProbe probe in participants) probe.AssertDraft(2);
        }
        finally { File.WriteAllBytes(path, original); }
        AssertFiles(before);
        foreach (ParticipantProbe probe in participants) probe.AssertPublished(1);
    }

    /// <summary>Delete a dirty saved station through the real removal path and require the saved station to remain guarded and recoverable.</summary>
    private static void DeletedStationDraft()
    {
        Dictionary<string, byte[]> before = Snapshot();
        VpCameraConfig camera = VisionConfig().Cameras.Single(item => item.Name == CameraName);
        VpVppConfig vpp = VisionVpp(); VpStation station = vpp.Station;
        typeof(VPForm).GetMethod("RemoveVpp", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(vision, new object[] { camera, vpp, null });
        Assert(!VisionConfig().Cameras.SelectMany(item => item.Vpps).Any(item => item.Id == vpp.Id) && !VisionproInterface.VList.Contains(station), "The station removal fixture did not remove the actual draft object.");
        Assert(vision.HasRecipeDraft, "Deleting a dirty saved station removed the recipe-switch guard.");
        ProjectConfiguration.Discard();
        Assert(ReferenceEquals(VisionVpp().Station, station), "Discard replaced the saved station identity.");
        foreach (ParticipantProbe probe in participants) probe.AssertReloaded(1);
        AssertFiles(before); ChangeAll(2);
    }

    /// <summary>Discard a newly created algorithm with no saved predecessor without retaining stale model contents.</summary>
    private static void NeverSavedVpp()
    {
        VpStation station = new VpStation("UnsavedCamera", "UnsavedVPP");
        string path = station.GetVppPath(SysPara.RecipeName), error;
        CogToolBlock created = null;
        try
        {
            Assert(station.TryCreateTB(path, out error), error); created = station.TB;
            Assert(station.TryDiscardTBChanges(out error), error);
            Assert(station.TB == null && !station.IsLoadTBOk && !File.Exists(path), "Discard retained or persisted a never-saved ToolBlock.");
        }
        finally
        {
            VisionproInterface.VList.Remove(station);
            if (created != null) created.Dispose();
        }
    }

    /// <summary>Use the real MainForm No choice and require every configuration owner to restore the saved generation.</summary>
    private static void DiscardAll()
    {
        ChooseSave("Button3_dyn");
        foreach (ParticipantProbe probe in participants) probe.AssertReloaded(1);
    }

    /// <summary>Save a newer generation and reload through the same discard entry to detect incomplete persistence.</summary>
    private static void ReloadAll()
    {
        ChangeAll(3); ChooseSave("Button1_dyn"); ChangeAll(4); ProjectConfiguration.Discard();
        foreach (ParticipantProbe probe in participants) probe.AssertReloaded(3);
    }

    /// <summary>Use the module recipe reload path to verify Setting stays machine-wide while the active recipe table changes.</summary>
    private static void RecipeScope()
    {
        string recipeA = SysPara.FilePath;
        string recipeB = Path.Combine(SysPara.RecipeDataDirectory, "Transaction-B.xml");
        DataSet alternate = new DataSet(); alternate.ReadXml(recipeA);
        DataTable constants = alternate.Tables[settings.RecipeData.Tables[ConstantStore.TableName].TableName];
        Assert(constants != null, "Saved recipe is missing its constants table.");
        constants.Rows.Cast<DataRow>().Single(row => Convert.ToString(row["Key"]) == "TxnRecipe")["Value"] = "9";
        alternate.WriteXml(recipeB);
        SysPara.FilePath = recipeB; SysPara.RecipeName = "Transaction-B";
        foreach (ModuleBaseForm form in ModuleManager.ModuleList) form.ReadRecipeData(recipeB);
        Assert(Convert.ToString(ConstantStore.Find(settings.Constants.Table("Setting"), "TxnSetting")["Value"]) == "3", "Recipe switch changed machine Setting.");
        Assert(Convert.ToString(ConstantStore.Find(settings.Constants.Table("Recipe"), "TxnRecipe")["Value"]) == "9", "Recipe B did not load its own value.");
        SysPara.FilePath = recipeA; SysPara.RecipeName = Path.GetFileNameWithoutExtension(recipeA);
        foreach (ModuleBaseForm form in ModuleManager.ModuleList) form.ReadRecipeData(recipeA);
        Assert(Convert.ToString(ConstantStore.Find(settings.Constants.Table("Setting"), "TxnSetting")["Value"]) == "3", "Returning to recipe A changed machine Setting.");
        Assert(Convert.ToString(ConstantStore.Find(settings.Constants.Table("Recipe"), "TxnRecipe")["Value"]) == "3", "Recipe A did not restore its own value.");
    }

    /// <summary>Apply the same generation to each real participant rather than simulating configuration files.</summary>
    private static void ChangeAll(int generation)
    {
        foreach (ParticipantProbe probe in participants) probe.Change(generation);
    }

    /// <summary>Read the real pending-file manifest without replacing the production transaction implementation.</summary>
    private static List<string> Manifest()
    {
        MethodInfo prepare = typeof(ProjectConfiguration).GetMethod("PrepareFiles", BindingFlags.Static | BindingFlags.NonPublic);
        Assert(prepare != null, "The transaction manifest API changed; update this harness adapter.");
        IEnumerable pending = (IEnumerable)prepare.Invoke(null, null);
        List<string> paths = new List<string>();
        foreach (object file in pending)
        {
            FieldInfo path = file.GetType().GetField("Path", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert(path != null, "A save task no longer exposes its destination field; update this harness adapter.");
            paths.Add(Path.GetFullPath((string)path.GetValue(file)));
        }
        return paths;
    }

    /// <summary>Capture all formal transaction destinations before fault injection.</summary>
    private static Dictionary<string, byte[]> Snapshot()
    {
        return Manifest().ToDictionary(path => path, path => File.Exists(path) ? File.ReadAllBytes(path) : null, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Require every formal file to retain its exact bytes and existence after failed or cancelled persistence.</summary>
    private static void AssertFiles(Dictionary<string, byte[]> expected)
    {
        foreach (KeyValuePair<string, byte[]> file in expected)
        {
            Assert(File.Exists(file.Key) == (file.Value != null), "File existence changed: " + file.Key);
            if (file.Value != null) Assert(File.ReadAllBytes(file.Key).SequenceEqual(file.Value), "File bytes changed: " + file.Key);
        }
    }

    /// <summary>Require failed staging and replacement to remove their temporary pending files.</summary>
    private static void AssertNoTemporaryFiles()
    {
        Assert(Directory.GetFiles(Environment.CurrentDirectory, "*.pending", SearchOption.AllDirectories).Length == 0, "A transaction left temporary pending files.");
        foreach (string directory in Manifest().Select(Path.GetDirectoryName).Distinct(StringComparer.OrdinalIgnoreCase))
            foreach (string file in Directory.GetFiles(directory))
            {
                Guid identity;
                Assert(!Guid.TryParseExact(Path.GetFileNameWithoutExtension(file), "N", out identity), "A transaction left a GUID staging file: " + file);
            }
    }

    /// <summary>Choose a visible confirmation button and fail rather than hanging if the MainForm save dialog changes.</summary>
    private static void ChooseSave(string buttonName)
    {
        ChooseSave(buttonName, main.SaveData);
    }

    /// <summary>Choose a real confirmation button for a Main Save request originating from the main window or a page-leave handler.</summary>
    private static void ChooseSave(string buttonName, Action request)
    {
        bool clicked = false; DateTime deadline = DateTime.UtcNow.AddSeconds(8);
        using (Timer timer = new Timer { Interval = 25 })
        {
            timer.Tick += (sender, args) =>
            {
                Alpha.OptionChoiceForm dialog = Application.OpenForms.OfType<Alpha.OptionChoiceForm>().FirstOrDefault(form => form.Visible && form.Modal);
                if (dialog == null && DateTime.UtcNow < deadline) return;
                if (dialog == null) { timer.Stop(); throw new InvalidOperationException("Main Save did not show its confirmation dialog."); }
                if (buttonName == "Close")
                {
                    Assert(dialog.ControlBox, "Main Save does not expose its close path.");
                    timer.Stop(); clicked = true; dialog.Close(); return;
                }
                Control[] buttons = dialog.Controls.Find(buttonName, true);
                if (buttons.Length != 1) throw new InvalidOperationException("Main Save confirmation button changed: " + buttonName);
                Assert(buttons[0].Visible, "The requested Main Save button is hidden: " + buttonName);
                timer.Stop(); clicked = true;
                typeof(Control).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(buttons[0], new object[] { EventArgs.Empty });
            };
            timer.Start(); request();
        }
        Assert(clicked, "Main Save returned without the expected confirmation.");
    }

    /// <summary>Report each independent invariant with a useful case name.</summary>
    private static void Test(string name, Action run)
    {
        try { run(); completed++; Console.WriteLine("PASS " + name); }
        catch (Exception error) { throw new InvalidOperationException(name + ": " + error.Message, error); }
    }

    /// <summary>Reject an unmet regression assertion with its exact failing invariant.</summary>
    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}

internal static partial class ConfigurationCases
{
    private static DeviceControl devicePanel;
    private static VPForm vision;
    private static AddUserForm rights;
    private static bool originalOperatorVision;
    private static string usersBefore;
    private const string CameraName = "Transaction camera";
    private const string VppName = "Transaction VPP";

    /// <summary>Copy the shipped database into the disposable fixture and keep every VPP path away from machine configuration.</summary>
    internal static void PrepareFixture(string root, string assemblies, string projectRoot)
    {
        string source = Path.Combine(assemblies, "SystemData.mdb");
        if (!File.Exists(source)) source = Path.Combine(projectRoot, "AlphaRap", "bin", "x64", "Debug", "SystemData.mdb");
        if (!File.Exists(source)) throw new FileNotFoundException("The permission fixture requires the shipped MDB schema.", source);
        string target = Path.Combine(root, "SystemData.mdb");
        File.Copy(source, target, false);
        Uri runner = new Uri(AppDomain.CurrentDomain.BaseDirectory);
        SysPara.MdbPath = Uri.UnescapeDataString(runner.MakeRelativeUri(new Uri(target)).ToString()).Replace('/', Path.DirectorySeparatorChar);
        SysPara.VisionFileDirectory = Path.Combine(root, "VisionData");
        Directory.CreateDirectory(SysPara.VisionFileDirectory);
    }

    /// <summary>Register the actual offline device, vision model and ACE permission editors as transaction participants.</summary>
    static partial void RegisterAdditionalParticipants(List<ParticipantProbe> probes, MainForm window, Type middleLayer)
    {
        devicePanel = new DeviceControl { Name = "transactionDevice", SingleDevice = true, AutoOpen = false, Visible = false };
        window.Controls.Add(devicePanel);
        string error = devicePanel.BindDevice("ScannerKeyenceTcp", "Transaction scanner");
        Assert(error == "" && !devicePanel.Device.IsConnected, "Could not create the offline scanner fixture.");
        vision = (VPForm)middleLayer.GetField("VPF", BindingFlags.Static | BindingFlags.Public).GetValue(null);
        VpConfigFile config = VisionConfig();
        VpCameraConfig camera = new VpCameraConfig { Name = CameraName, FolderName = "TransactionCamera" };
        VpVppConfig vpp = new VpVppConfig { Name = VppName, FolderName = "TransactionVpp" };
        vpp.Station = new VpStation(camera.StorageFolder, vpp.StorageFolder) { Config = vpp };
        vpp.EnsureCompLimits(); camera.Vpps.Add(vpp); config.Cameras.Add(camera);
        Assert(vpp.Station.TryCreateTB(vpp.Station.GetVppPath(SysPara.RecipeName), out error), error);
        vpp.Station.TB.Outputs.Add(new CogToolBlockTerminal("Generation", typeof(double)));
        rights = (AddUserForm)middleLayer.GetField("AddF", BindingFlags.Static | BindingFlags.Public).GetValue(null);
        originalOperatorVision = (bool)RightsTable(false).Rows.Cast<DataRow>().Single(row => Convert.ToString(row["Permission"]) == "Operator")["Vision"];
        usersBefore = ReadDatabase("UserData").GetXml();
        probes.Add(new ParticipantProbe { Name = "Device", Paths = () => new[] { DeviceControl.XmlFilePath }, Change = ChangeDevice,
            AssertDraft = AssertDeviceDraft, AssertPublished = AssertDeviceSaved, AssertReloaded = AssertDeviceReloaded });
        probes.Add(new ParticipantProbe { Name = "Vision", Paths = () => new[] { VpConfigStore.FilePath, VisionVpp().Station.GetVppPath(SysPara.RecipeName) },
            Change = ChangeVision, AssertDraft = AssertVisionDraft, AssertPublished = AssertVisionPublished, AssertReloaded = AssertVisionReloaded });
        probes.Add(new ParticipantProbe { Name = "Permissions", Paths = () => new[] { DatabasePath() }, Change = ChangeRights,
            AssertDraft = AssertRightsDraft, AssertPublished = AssertRightsPublished, AssertReloaded = AssertRightsReloaded });
    }

    /// <summary>Change TCP parameters only in the real offline scanner object.</summary>
    private static void ChangeDevice(int generation)
    {
        ScannerKeyenceTcp scanner = (ScannerKeyenceTcp)devicePanel.Device;
        scanner.IP = "192.0.2." + generation; scanner.Port = 9100 + generation;
    }

    /// <summary>Verify device drafts survive failures and language changes without opening a connection.</summary>
    private static void AssertDeviceDraft(int generation)
    {
        ScannerKeyenceTcp scanner = (ScannerKeyenceTcp)devicePanel.Device;
        Assert(scanner.IP == "192.0.2." + generation && scanner.Port == 9100 + generation && !scanner.IsConnected, "Device draft changed or opened a connection.");
    }

    /// <summary>Verify the committed device XML remains the last successful transaction version.</summary>
    private static void AssertDeviceSaved(int generation)
    {
        XmlDocument xml = new XmlDocument(); xml.Load(DeviceControl.XmlFilePath);
        XmlNode node = xml.SelectSingleNode("/DeviceControl/Device/Item[@Name='Transaction scanner']");
        Assert(node != null && node["IP"].InnerText == "192.0.2." + generation && node["Port"].InnerText == (9100 + generation).ToString(CultureInfo.InvariantCulture), "Saved scanner configuration changed before commit or did not persist.");
    }

    /// <summary>Verify device reconstruction uses the saved file and keeps the scanner disconnected.</summary>
    private static void AssertDeviceReloaded(int generation)
    {
        AssertDeviceDraft(generation); AssertDeviceSaved(generation);
    }

    /// <summary>Read the actual VPForm configuration rather than retaining an object replaced during discard.</summary>
    private static VpConfigFile VisionConfig()
    {
        return (VpConfigFile)typeof(VPForm).GetField("_config", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(vision);
    }

    /// <summary>Resolve the configured station again after any discard or reload.</summary>
    private static VpVppConfig VisionVpp()
    {
        return VisionConfig().Cameras.Single(item => item.Name == CameraName).Vpps.Single(item => item.Name == VppName);
    }

    /// <summary>Change camera configuration, compensation and a real in-memory CogToolBlock without writing its file.</summary>
    private static void ChangeVision(int generation)
    {
        VpCameraConfig camera = VisionConfig().Cameras.Single(item => item.Name == CameraName);
        VpVppConfig vpp = VisionVpp();
        camera.Exposure = 20 + generation; vpp.Exposure = 30 + generation;
        vpp.CompLimits[0].Compensation = generation * 0.25;
        vpp.Station.TB.Outputs["Generation"].Value = (double)generation;
        typeof(VisionproInterface).GetMethod("MarkToolBlockDraft", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(vpp.Station, null);
        typeof(VPForm).GetMethod("MarkVpConfigDraft", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(vision, null);
    }

    /// <summary>Verify camera values and algorithm contents are still the pending in-memory generation.</summary>
    private static void AssertVisionDraft(int generation)
    {
        VpVppConfig vpp = VisionVpp();
        Assert(VisionConfig().Cameras.Single(item => item.Name == CameraName).Exposure == 20 + generation && vpp.Exposure == 30 + generation
            && vpp.CompLimits[0].Compensation == generation * 0.25 && Convert.ToDouble(vpp.Station.TB.Outputs["Generation"].Value) == generation, "Vision draft changed.");
    }

    /// <summary>Verify the camera XML and serialized VPP retain the committed generation and the saved flow configuration matches it.</summary>
    private static void AssertVisionPublished(int generation)
    {
        string error; VpConfigFile saved = VpConfigStore.Load(out error); Assert(error == null, error);
        VpCameraConfig camera = saved.Cameras.Single(item => item.Name == CameraName);
        VpVppConfig vpp = camera.Vpps.Single(item => item.Name == VppName);
        Assert(camera.Exposure == 20 + generation && vpp.Exposure == 30 + generation && vpp.CompLimits[0].Compensation == generation * 0.25, "Camera XML changed before commit or did not persist.");
        using (CogToolBlock stored = (CogToolBlock)CogSerializer.LoadObjectFromFile(VisionVpp().Station.GetVppPath(SysPara.RecipeName)))
            Assert(Convert.ToDouble(stored.Outputs["Generation"].Value) == generation, "The VPP changed before commit or did not persist.");
        VpVppConfig published = (VpVppConfig)typeof(VpStation).GetField("flowSavedConfig", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(VisionVpp().Station);
        Assert(published != null && published.Exposure == 30 + generation && published.CompLimits[0].Compensation == generation * 0.25, "The saved visual flow configuration was accepted before commit or was not accepted.");
    }

    /// <summary>Require vision discard to reload the last VPP and clear its unsaved recipe marker.</summary>
    private static void AssertVisionReloaded(int generation)
    {
        AssertVisionDraft(generation); AssertVisionPublished(generation);
        Assert(!vision.HasRecipeDraft, "Reloaded visual algorithm still has an unsaved recipe draft.");
    }

    /// <summary>Resolve the permission grid or its accepted snapshot using the same stable role keys.</summary>
    private static DataTable RightsTable(bool published)
    {
        string field = published ? "_savedPermissions" : "dgvRightsConfig";
        object value = typeof(AddUserForm).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(rights);
        DataTable table = published ? (DataTable)value : (DataTable)((DataGridView)value).DataSource;
        Assert(table != null && table.Columns.Contains("Vision"), "The real permission table was not loaded.");
        return table;
    }

    /// <summary>Resolve the relative database path exactly as the application permission participant does.</summary>
    private static string DatabasePath()
    {
        return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SysPara.MdbPath));
    }

    /// <summary>Read actual ACE table contents with connection pooling disabled so the transaction can replace the database.</summary>
    private static DataSet ReadDatabase(string tableName)
    {
        OleDbConnectionStringBuilder connection = new OleDbConnectionStringBuilder { Provider = "Microsoft.ACE.OLEDB.12.0", DataSource = DatabasePath() };
        connection["OLE DB Services"] = -2;
        DataSet values = new DataSet();
        using (OleDbConnection database = new OleDbConnection(connection.ConnectionString))
        using (OleDbDataAdapter adapter = new OleDbDataAdapter("SELECT * FROM [" + tableName + "]", database)) adapter.Fill(values, tableName);
        return values;
    }

    /// <summary>Calculate distinct pending and committed permission values without changing the logged-in administrator.</summary>
    private static bool RightsValue(int generation)
    {
        return generation % 2 == 1 ? !originalOperatorVision : originalOperatorVision;
    }

    /// <summary>Edit a real permissions table cell without calling the legacy direct database writer.</summary>
    private static void ChangeRights(int generation)
    {
        RightsTable(false).Rows.Cast<DataRow>().Single(row => Convert.ToString(row["Permission"]) == "Operator")["Vision"] = RightsValue(generation);
    }

    /// <summary>Require the permission draft to retain its role-keyed value.</summary>
    private static void AssertRightsDraft(int generation)
    {
        Assert((bool)RightsTable(false).Rows.Cast<DataRow>().Single(row => Convert.ToString(row["Permission"]) == "Operator")["Vision"] == RightsValue(generation), "Permission draft changed.");
    }

    /// <summary>Compare the accepted permission snapshot and real MDB while requiring all user account contents to remain intact.</summary>
    private static void AssertRightsPublished(int generation)
    {
        Assert((bool)RightsTable(true).Rows.Cast<DataRow>().Single(row => Convert.ToString(row["Permission"]) == "Operator")["Vision"] == RightsValue(generation), "Permission snapshot was accepted before commit or was not accepted.");
        Assert((bool)ReadDatabase("PermissionSetup").Tables[0].Rows.Cast<DataRow>().Single(row => Convert.ToString(row["Permission"]) == "Operator")["Vision"] == RightsValue(generation), "Permission MDB changed before commit or did not persist.");
        Assert(ReadDatabase("UserData").GetXml() == usersBefore, "Saving permissions changed unrelated user accounts.");
    }

    /// <summary>Verify discard and reload rebind an accepted permission table from the actual database.</summary>
    private static void AssertRightsReloaded(int generation)
    {
        AssertRightsDraft(generation); AssertRightsPublished(generation);
        Assert(RightsTable(false).GetChanges() == null, "Reloaded permissions remained dirty.");
    }

    /// <summary>Require an unloaded permission editor to report a missing database instead of silently skipping persistence.</summary>
    private static void MissingPermissionDraft()
    {
        string savedPath = SysPara.MdbPath;
        AddUserForm unloaded = null;
        bool rejected = false;
        try
        {
            SysPara.MdbPath = Path.Combine(Path.GetDirectoryName(savedPath), "MissingPermissions.mdb");
            unloaded = new AddUserForm();
            try { typeof(AddUserForm).GetMethod("PreparePermissionSave", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(unloaded, null); }
            catch (TargetInvocationException error)
            {
                if (!(error.InnerException is InvalidOperationException)) throw;
                rejected = true;
            }
            Assert(!File.Exists(DatabasePath()), "The missing permission database was created during failed preparation.");
        }
        finally
        {
            SysPara.MdbPath = savedPath;
            if (unloaded != null) unloaded.Dispose();
        }
        Assert(rejected, "An unloaded permission editor silently skipped its save task.");
    }
}
