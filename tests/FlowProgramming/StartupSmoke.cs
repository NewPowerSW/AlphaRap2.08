using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using AlphaRap;
using AlphaRapLibrary;
using AlphaRap.Configuration;
using AlphaRap.FlowProgramming;
using NPSDK.Motion;
using NPSDK;
using System.Collections.Generic;

internal static class StartupSmoke
{
    /// <summary>Resolve installed UI dependencies, then exercise the real form creation sequence without starting hardware.</summary>
    [STAThread]
    private static int Main(string[] args)
    {
        AppDomain.CurrentDomain.AssemblyResolve += (sender, request) =>
        {
            string name = new AssemblyName(request.Name).Name;
            foreach (string directory in new[] { args[0], @"C:\Program Files\Cognex\VisionPro\ReferencedAssemblies" })
                foreach (string extension in new[] { ".dll", ".exe" })
                {
                    string file = Path.Combine(directory, name + extension);
                    if (File.Exists(file)) return Assembly.LoadFrom(file);
                }
            return null;
        };
        try { Run(args); return 0; }
        catch (Exception error) { Console.WriteLine("FAIL startup: " + error); return 1; }
    }

    /// <summary>Open the real main window in an isolated directory and release installed hooks before disposing forms.</summary>
    private static void Run(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
        string root = Path.GetFullPath(args[1]); Directory.CreateDirectory(root); Environment.CurrentDirectory = root;
        SysPara.SettingDataDirectory = StartupSettingDirectory(root); Directory.CreateDirectory(SysPara.SettingDataDirectory);
        ModuleManager.SettingDataDirectory = SysPara.SettingDataDirectory;
        SysPara.LanguageDataDirectory = Path.Combine(root,"LanguageData");
        SysPara.AlarmTableDirectory = Path.Combine(root,"AlarmTable");
        CopyXml(Path.Combine(args[2],"LanguageData"),SysPara.LanguageDataDirectory);
        CopyXml(Path.Combine(args[2],"AlarmTable"),SysPara.AlarmTableDirectory);
        SysPara.RecipeDataDirectory = Path.Combine(root,"Recipes"); Directory.CreateDirectory(SysPara.RecipeDataDirectory);
        SysPara.FilePath = Path.Combine(SysPara.RecipeDataDirectory,"Startup.xml"); SysPara.RecipeName = "Startup";
        SysPara.SystemMode = RunMode.IDLE; SysPara.Simulation = true;
        ConfigurationCases.PrepareFixture(root,args[0],args[2]);
        File.WriteAllText(Path.Combine(SysPara.SettingDataDirectory,"SysSetting.xml"),"<NewDataSet><MSet /><PSet><DisScann>true</DisScann><UseTryRun>false</UseTryRun></PSet></NewDataSet>");
        Type host = typeof(SysPara).Assembly.GetType("AlphaRap.MiddleLayer");
        host.GetMethod("LoadAlarmTable",BindingFlags.Static|BindingFlags.NonPublic,null,Type.EmptyTypes,null).Invoke(null,null);
        host.GetMethod("CreateProjectForms",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);
        host.GetMethod("InitialLanguageData",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);
        host.GetMethod("SwitchLanguage",BindingFlags.Static|BindingFlags.Public).Invoke(null,new object[]{LanguageType.Chinese});
        host.GetField("alarmRunTask",BindingFlags.Static|BindingFlags.Public).SetValue(null,System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(AlarmRunTask)));
        SystemSetting setting = (SystemSetting)host.GetField("SystemS",BindingFlags.Static|BindingFlags.Public).GetValue(null);
        if (setting.Constants.Table("Setting").Select("Key <> ''").Length!=8 || !(bool)setting.GetSettingValue("PSet","DisScann")) throw new InvalidOperationException("Legacy settings migration changed the original flag.");
        MainForm main = (MainForm)host.GetField("MainF",BindingFlags.Static|BindingFlags.Public).GetValue(null);
        Exception runError = null;
        try
        {
            main.Show(); Application.DoEvents();
            ReleaseStartupHooks(main);
            if (!main.Visible || !main.IsHandleCreated) throw new InvalidOperationException("Main window did not open.");
            using (Bitmap image = new Bitmap(main.Width,main.Height)) { main.DrawToBitmap(image,new Rectangle(Point.Empty,main.Size)); image.Save(Path.Combine(root,"startup-main.png")); }
            Console.WriteLine("PASS real startup form creation and main window with empty legacy MSet; hardware initialization and machine threads were not started.");
            VerifyMainSave(main,host,setting);
            ConfigurationCases.Run(main,host,setting);
            SourceGenerationCases.Run(root);
        }
        catch (Exception error) { runError = error; throw; }
        finally
        {
            try { ReleaseStartupForms(main); }
            catch (Exception cleanupError)
            {
                if (runError == null) throw;
                throw new AggregateException("Startup execution and cleanup both failed.", runError, cleanupError);
            }
        }
    }

    /// <summary>Use a deep settings directory to exercise transaction staging together with atomic flow-file writes.</summary>
    private static string StartupSettingDirectory(string root)
    {
        string directory = Path.Combine(root,"Settings");
        return directory.Length >= 170 ? directory : Path.Combine(directory,new string('s',Math.Max(1,169-directory.Length)));
    }

    /// <summary>Remove desktop input hooks and stop the actual auto-logout timer before testing configuration operations.</summary>
    private static void ReleaseStartupHooks(MainForm main)
    {
        List<Exception> errors = new List<Exception>();
        if (main.mh != null)
        {
            try { main.mh.UnHook(); }
            catch (Exception error) { errors.Add(new InvalidOperationException("Startup cleanup could not remove the main-window mouse hook.", error)); }
        }
        if (main.k_hook != null)
        {
            try { main.k_hook.Stop(); }
            catch (Exception error) { errors.Add(new InvalidOperationException("Startup cleanup could not remove the main-window keyboard hook.", error)); }
        }
        try
        {
            FieldInfo timerField = typeof(MainForm).GetField("LoginOutTime", BindingFlags.Instance | BindingFlags.NonPublic);
            if (timerField == null) throw new MissingFieldException(typeof(MainForm).FullName, "LoginOutTime");
            Timer logout = (Timer)timerField.GetValue(main);
            if (logout == null) throw new InvalidOperationException("The main-window auto-logout timer was not initialized.");
            logout.Stop();
            if (logout.Enabled) throw new InvalidOperationException("The main-window auto-logout timer remained enabled.");
        }
        catch (Exception error) { errors.Add(new InvalidOperationException("Startup cleanup could not stop the main-window auto-logout timer.", error)); }
        if (errors.Count > 0) throw new AggregateException("Startup input isolation failed.", errors);
    }

    /// <summary>Release hooks again before disposing open forms and report every cleanup failure without closing the application.</summary>
    private static void ReleaseStartupForms(MainForm main)
    {
        List<Exception> errors = new List<Exception>();
        try { ReleaseStartupHooks(main); }
        catch (Exception error) { errors.Add(error); }
        foreach (Form form in Application.OpenForms.Cast<Form>().ToArray())
        {
            try { form.Dispose(); }
            catch (Exception error) { errors.Add(new InvalidOperationException("Startup cleanup could not dispose form: " + form.Name, error)); }
        }
        if (errors.Count > 0) throw new AggregateException("Startup cleanup failed.", errors);
    }

    /// <summary>Save and discard through the real MainForm entry, then verify failed staging preserves disk and committed snapshots.</summary>
    private static void VerifyMainSave(MainForm main,Type host,SystemSetting setting)
    {
        SysPara.UserPermission = PermissionType.Administrator;
        HardForm hardware = (HardForm)host.GetField("HardF",BindingFlags.Static|BindingFlags.Public).GetValue(null);
        DataForm data = (DataForm)host.GetField("DataF",BindingFlags.Static|BindingFlags.Public).GetValue(null);
        HardwareDefinition definition = hardware.Resources.NewDefinition("Axis");
        NP_AxisGroup axis = (NP_AxisGroup)hardware.Resources.Put(definition).Group; axis.Points.DataSet1.Rows.Add(21,"Main save point");
        setting.Constants.Put("Setting",null,new ConstantValue { Key="MainSaveFlag",Type="Boolean",Value="True" });
        setting.Constants.Put("Recipe",null,new ConstantValue { Key="MainRecipeNumber",Type="Number",Value="12.5" });
        data.SetProductionDraft(new List<ProductionField> { new ProductionField { Name="Barcode" },new ProductionField { Name="Result" },new ProductionField { Name="MainValue",Type="Number" } });
        FlowActions draftActions = new FlowActions(FlowResources.Capture(true)), liveActions = new FlowActions(FlowResources.Capture());
        if (!draftActions.InputNames(draftActions.MethodsFor(draftActions.FindResource("data/Production")).Single(action=>action.Id=="data.save")).Contains("MainValue") || liveActions.InputNames(liveActions.MethodsFor(liveActions.FindResource("data/Production")).Single(action=>action.Id=="data.save")).Contains("MainValue")) throw new InvalidOperationException("Production field drafts leaked into runtime or were unavailable in the designer.");
        FlowModuleEditor flow = FlowPrograms.Attach(FlowPrograms.AddModule("Main save flow")); FlowStep step = flow.AddNode(); flow.Program.Mode="Program"; flow.Program.AutoEntry=step.Id;
        if (FlowResources.Capture().Parameters.ContainsKey("Constants/Setting/MainSaveFlag") || !FlowResources.Capture(true).Parameters.ContainsKey("Constants/Setting/MainSaveFlag")) throw new InvalidOperationException("Draft and production snapshots are mixed.");
        ChooseSave(main,"Button1_dyn");
        bool dirty = hardware.Resources.IsDirty;
        bool resourceExists = HardwareResources.Load(hardware.Resources.PathName).Resources.Any(item=>item.Id==definition.Id);
        bool schema = data.GetProductionSchema().Any(field=>field.Name=="MainValue");
        bool flowIsProgram = flow.IsProgram;
        if (dirty || !resourceExists || !schema || !flowIsProgram)
            throw new InvalidOperationException("Main Save did not commit all new configuration: IsDirty=" + dirty + ", resourceExists=" + resourceExists
                + ", schema=" + schema + ", flowIsProgram=" + flowIsProgram + ", permission=" + SysPara.UserPermission + ", mode=" + SysPara.SystemMode + ".");
        if (main.Controls.Find("save",true).Length>0 || hardware.Controls.Find("resourceSave",true).Length>0 || setting.Controls.Find("constantSave",true).Length>0) throw new InvalidOperationException("A separate configuration Save button remains.");
        Console.WriteLine("PASS MainForm Save persists axis definitions, recipe points/constants, Setting constants, production fields and the published flow.");
        axis.Points.DataSet1.Rows[0]["Position"]=22;
        setting.Constants.Put("Setting","MainSaveFlag",new ConstantValue { Key="MainSaveFlag",Type="Boolean",Value="False" });
        HardwareDefinition scratch = hardware.Resources.NewDefinition("Output"); hardware.Resources.Put(scratch);
        data.SetProductionDraft(new List<ProductionField> { new ProductionField { Name="DiscardedField" } });
        ChooseSave(main,"Button3_dyn");
        axis=(NP_AxisGroup)hardware.Resources.Entries.Single(entry=>entry.Definition.Id==definition.Id).Group;
        double restoredPoint=axis.Points.DataSet1.Rows[0].Field<double>("Position");
        bool scratchRemains=hardware.Resources.Entries.Any(entry=>entry.Definition.Id==scratch.Id);
        bool discardedFieldRemains=data.GetProductionDraft().Any(field=>field.Name=="DiscardedField");
        bool savedFlag=(bool)setting.Constants.Committed("Setting").Single(pair=>pair.Key=="Constants/Setting/MainSaveFlag").Value;
        if (restoredPoint!=21 || scratchRemains || discardedFieldRemains || !savedFlag) throw new InvalidOperationException("Main Save No did not restore the saved configuration: point="+restoredPoint+", scratchRemains="+scratchRemains+", discardedFieldRemains="+discardedFieldRemains+", savedFlag="+savedFlag+", permission="+SysPara.UserPermission+", mode="+SysPara.SystemMode+".");
        Console.WriteLine("PASS MainForm Save No restores points, resource groups, constants, production fields and flow drafts.");
        string path=Path.Combine(ModuleManager.SettingDataDirectory,"SysSetting.xml"), before=File.ReadAllText(path), savedDirectory=SysPara.SettingDataDirectory;
        string blocker=Path.Combine(Environment.CurrentDirectory,"blocked-save"); File.WriteAllText(blocker,"fixture");
        setting.Constants.Put("Setting","MainSaveFlag",new ConstantValue { Key="MainSaveFlag",Type="Boolean",Value="False" });
        bool failed=false;
        try { SysPara.SettingDataDirectory=blocker; ProjectConfiguration.Save(); }
        catch (IOException) { failed=true; }
        finally { SysPara.SettingDataDirectory=savedDirectory; }
        if (!failed || File.ReadAllText(path)!=before || !(bool)setting.Constants.Committed("Setting").Single(pair=>pair.Key=="Constants/Setting/MainSaveFlag").Value || setting.Constants.Table("Setting").GetChanges()==null) throw new InvalidOperationException("Failed staging committed files or discarded pending values.");
        Console.WriteLine("PASS failed unified save preserves configuration files, committed values and editable drafts.");
        failed=false;
        using (FileStream locked = File.Open(FlowPrograms.FilePath,FileMode.Open,FileAccess.Read,FileShare.Read))
        {
            try { ProjectConfiguration.Save(); } catch (IOException) { failed=true; }
        }
        if (!failed || File.ReadAllText(path)!=before || !(bool)setting.Constants.Committed("Setting").Single(pair=>pair.Key=="Constants/Setting/MainSaveFlag").Value || setting.Constants.Table("Setting").GetChanges()==null) throw new InvalidOperationException("A late save failure did not roll back previously replaced files.");
        Console.WriteLine("PASS late unified-save replacement failure rolls back earlier configuration files and retains drafts.");
        ChooseSave(main,"Button3_dyn");
        VerifyFlowTableSave(main,setting);
        VerifyActionMainSave(main);
    }

    /// <summary>Publish typed action nodes through Main Save and preserve the published version after invalid drafts and discard.</summary>
    private static void VerifyActionMainSave(MainForm main)
    {
        FlowModuleEditor module = FlowPrograms.Attach(FlowPrograms.AddModule("Saved action flow"));
        FlowStep step = module.AddNode();
        step.ExecutionMode = "Action"; step.Action = "value"; step.Target = ""; step.Breakpoint = true;
        step.Inputs = new List<FlowInput> { new FlowInput { Name = "Value", Reference = "2" } };
        module.Program.Mode = "Program"; module.Program.AutoEntry = step.Id;
        ChooseSave(main,"Button1_dyn");
        FlowStep saved = FlowProgramFile.Load(FlowPrograms.FilePath).Modules.Single(item => item.ModuleId == module.Program.ModuleId).Steps.Single();
        if (saved.ExecutionMode != "Action" || saved.Action != "value" || !saved.Breakpoint || saved.Inputs.Single().Reference != "2")
            throw new InvalidOperationException("Main Save did not persist the complete action node.");
        string before = File.ReadAllText(FlowPrograms.FilePath);
        step.Action = "missing.action";
        bool rejected = false;
        try { ProjectConfiguration.Save(); } catch (InvalidOperationException) { rejected = true; }
        if (!rejected || File.ReadAllText(FlowPrograms.FilePath) != before || step.Action != "missing.action")
            throw new InvalidOperationException("Action validation failure overwrote the saved flow or discarded the draft.");
        module.Start(step.Id, true, false, true);
        DateTime limit = DateTime.UtcNow.AddSeconds(5);
        while (!module.Runtime.Completion.IsCompleted && DateTime.UtcNow < limit) { Application.DoEvents(); System.Threading.Thread.Sleep(5); }
        if (!module.Runtime.Completion.IsCompleted || module.Runtime.Error != null || Convert.ToDouble(FlowPrograms.Results.Read(FlowPrograms.Results.Epoch, "batch", module.Program.ModuleId + "/" + step.Id, "Value")) != 2)
            throw new InvalidOperationException("Production execution did not use the last saved action version.");
        ChooseSave(main,"Button3_dyn");
        if (module.Program.Steps.Single().Action != "value" || !module.Program.Steps.Single().Breakpoint)
            throw new InvalidOperationException("Main Save No failed to restore saved action and breakpoint.");
        Console.WriteLine("PASS Main Save persists action bindings and breakpoints; invalid actions retain drafts and the published version; discard restores saved action.");
    }

    /// <summary>Write actual Setting and Recipe rows from flow nodes, then exercise real main Save, reload, failure and discard.</summary>
    private static void VerifyFlowTableSave(MainForm main,SystemSetting setting)
    {
        string path=Path.Combine(ModuleManager.SettingDataDirectory,"SysSetting.xml"), before=File.ReadAllText(path);
        SetTableFromNode("Setting","DisScann","false"); SetTableFromNode("Recipe","MainRecipeNumber","27.5");
        DataRow row=ConstantStore.Find(setting.Constants.Table("Setting"),"DisScann");
        if(Convert.ToString(row["Value"])!="False" || Convert.ToString(row["Value",DataRowVersion.Original])!="True" || (bool)setting.SettingData.Tables["PSet"].Rows[0]["DisScann"] || File.ReadAllText(path)!=before) throw new InvalidOperationException("Flow setting did not change the actual row and legacy switch in memory only.");
        ChooseSave(main,"Button3_dyn");
        if(!(bool)setting.Constants.Committed("Setting").Single(item=>item.Key==ConstantStore.Reference("Setting","DisScann")).Value || (double)setting.Constants.Committed("Recipe").Single(item=>item.Key==ConstantStore.Reference("Recipe","MainRecipeNumber")).Value!=12.5) throw new InvalidOperationException("Discard retained a value published by an earlier flow.");
        SetTableFromNode("Setting","DisScann","false"); SetTableFromNode("Recipe","MainRecipeNumber","27.5");
        bool failed=false;
        using(FileStream locked=File.Open(FlowPrograms.FilePath,FileMode.Open,FileAccess.Read,FileShare.Read))
        { try{ProjectConfiguration.Save();}catch(IOException){failed=true;} }
        row=ConstantStore.Find(setting.Constants.Table("Setting"),"DisScann");
        if(!failed || File.ReadAllText(path)!=before || row.RowState!=DataRowState.Modified || Convert.ToString(row["Value",DataRowVersion.Original])!="True" || (bool)setting.Constants.Committed("Setting").Single(item=>item.Key==ConstantStore.Reference("Setting","DisScann")).Value) throw new InvalidOperationException("Failed Save lost the flow-written table value or committed it prematurely.");
        ChooseSave(main,"Button1_dyn");ChooseSave(main,"Button3_dyn");
        if((bool)setting.GetSettingValue("PSet","DisScann") || Convert.ToString(ConstantStore.Find(setting.Constants.Table("Setting"),"DisScann")["Value",DataRowVersion.Original])!="False" || (double)setting.Constants.Committed("Recipe").Single(item=>item.Key==ConstantStore.Reference("Recipe","MainRecipeNumber")).Value!=27.5) throw new InvalidOperationException("Main Save and reload did not preserve actual flow-written table values.");
        Console.WriteLine("PASS flow writes update real Setting/Recipe rows and legacy switches; Main Save, failed Save, discard and reload preserve the correct versions.");
    }

    /// <summary>Execute the table setter through the same typed object method used by a configured node.</summary>
    private static void SetTableFromNode(string scope,string key,string value)
    {
        FlowActions actions = new FlowActions(FlowResources.Capture());
        FlowResource resource = actions.FindResource("constants/" + scope);
        FlowMethod method = resource.Methods.Single(item => item.Id == "constant.set");
        FlowStep step = new FlowStep
        {
            Action = method.Id,
            Target = resource.Id,
            Inputs = method.Inputs.Select(field => new FlowInput { Name = field.Name, Reference = field.Name == "Key" ? key : value }).ToList()
        };
        FlowResultHub hub = new FlowResultHub();
        FlowExecutionContext context = new FlowExecutionContext { Hub = hub, RunId = hub.Epoch, Parameters = actions.Resources.Parameters };
        actions.Execute(step, context, System.Threading.CancellationToken.None).GetAwaiter().GetResult();
    }

    /// <summary>Complete the real main-save confirmation using its visible button event in the isolated fixture.</summary>
    private static void ChooseSave(MainForm main,string buttonName)
    {
        using (Timer timer = new Timer { Interval=30 })
        {
            timer.Tick += (sender,args)=>
            {
                Alpha.OptionChoiceForm dialog=Application.OpenForms.OfType<Alpha.OptionChoiceForm>().FirstOrDefault(form=>form.Visible && form.Modal);
                if (dialog==null) return;
                timer.Stop(); Control button=dialog.Controls.Find(buttonName,true)[0];
                typeof(Control).GetMethod("OnClick",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(button,new object[]{EventArgs.Empty});
            };
            timer.Start(); main.SaveData();
        }
    }

    /// <summary>Keep runtime language registration and alarm loading inside disposable test directories.</summary>
    private static void CopyXml(string source, string target)
    {
        Directory.CreateDirectory(target);
        foreach (string file in Directory.GetFiles(source,"*.xml")) File.Copy(file,Path.Combine(target,Path.GetFileName(file)),true);
    }
}
