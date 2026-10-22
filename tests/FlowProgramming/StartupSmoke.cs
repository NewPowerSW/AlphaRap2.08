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

    /// <summary>Load a historical empty MSet and open the real main window in an isolated configuration directory.</summary>
    private static void Run(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
        string root = Path.GetFullPath(args[1]); Directory.CreateDirectory(root); Environment.CurrentDirectory = root;
        SysPara.SettingDataDirectory = Path.Combine(root,"Settings"); Directory.CreateDirectory(SysPara.SettingDataDirectory);
        ModuleManager.SettingDataDirectory = SysPara.SettingDataDirectory;
        SysPara.LanguageDataDirectory = Path.Combine(root,"LanguageData");
        SysPara.AlarmTableDirectory = Path.Combine(root,"AlarmTable");
        CopyXml(Path.Combine(args[2],"LanguageData"),SysPara.LanguageDataDirectory);
        CopyXml(Path.Combine(args[2],"AlarmTable"),SysPara.AlarmTableDirectory);
        SysPara.RecipeDataDirectory = Path.Combine(root,"Recipes"); Directory.CreateDirectory(SysPara.RecipeDataDirectory);
        SysPara.FilePath = Path.Combine(SysPara.RecipeDataDirectory,"Startup.xml"); SysPara.RecipeName = "Startup";
        SysPara.SystemMode = RunMode.IDLE; SysPara.Simulation = true;
        File.WriteAllText(Path.Combine(SysPara.SettingDataDirectory,"SysSetting.xml"),"<NewDataSet><MSet /><PSet><DisScann>true</DisScann><UseTryRun>false</UseTryRun></PSet></NewDataSet>");
        Type host = typeof(SysPara).Assembly.GetType("AlphaRap.MiddleLayer");
        host.GetMethod("LoadAlarmTable",BindingFlags.Static|BindingFlags.NonPublic,null,Type.EmptyTypes,null).Invoke(null,null);
        host.GetMethod("CreateProjectForms",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);
        host.GetMethod("InitialLanguageData",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,null);
        host.GetMethod("SwitchLanguage",BindingFlags.Static|BindingFlags.Public).Invoke(null,new object[]{LanguageType.Chinese});
        host.GetField("alarmRunTask",BindingFlags.Static|BindingFlags.Public).SetValue(null,System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(AlarmRunTask)));
        SystemSetting setting = (SystemSetting)host.GetField("SystemS",BindingFlags.Static|BindingFlags.Public).GetValue(null);
        if (setting.Constants.Table("Setting").Select("Key <> ''").Length!=8 || !(bool)setting.GetSettingValue("PSet","DisScann")) throw new InvalidOperationException("Legacy settings migration changed the original flag.");
        Form main = (Form)host.GetField("MainF",BindingFlags.Static|BindingFlags.Public).GetValue(null);
        try
        {
            main.Show(); Application.DoEvents();
            if (!main.Visible || !main.IsHandleCreated) throw new InvalidOperationException("Main window did not open.");
            using (Bitmap image = new Bitmap(main.Width,main.Height)) { main.DrawToBitmap(image,new Rectangle(Point.Empty,main.Size)); image.Save(Path.Combine(root,"startup-main.png")); }
            Console.WriteLine("PASS real startup form creation and main window with empty legacy MSet; hardware initialization and machine threads were not started.");
            VerifyMainSave((MainForm)main,host,setting);
        }
        finally { foreach (Form form in Application.OpenForms.Cast<Form>().ToArray()) form.Dispose(); }
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
        if (hardware.Resources.IsDirty || !HardwareResources.Load(hardware.Resources.PathName).Resources.Any(item=>item.Id==definition.Id) || !data.GetProductionSchema().Any(field=>field.Name=="MainValue") || !flow.IsProgram) throw new InvalidOperationException("Main Save did not commit all new configuration.");
        if (main.Controls.Find("save",true).Length>0 || hardware.Controls.Find("resourceSave",true).Length>0 || setting.Controls.Find("constantSave",true).Length>0) throw new InvalidOperationException("A separate configuration Save button remains.");
        Console.WriteLine("PASS MainForm Save persists axis definitions, recipe points/constants, Setting constants, production fields and the published flow.");
        axis.Points.DataSet1.Rows[0]["Position"]=22;
        setting.Constants.Put("Setting","MainSaveFlag",new ConstantValue { Key="MainSaveFlag",Type="Boolean",Value="False" });
        HardwareDefinition scratch = hardware.Resources.NewDefinition("Output"); hardware.Resources.Put(scratch);
        data.SetProductionDraft(new List<ProductionField> { new ProductionField { Name="DiscardedField" } });
        ChooseSave(main,"Button3_dyn");
        axis=(NP_AxisGroup)hardware.Resources.Entries.Single(entry=>entry.Definition.Id==definition.Id).Group;
        if (axis.Points.DataSet1.Rows[0].Field<double>("Position")!=21 || hardware.Resources.Entries.Any(entry=>entry.Definition.Id==scratch.Id) || data.GetProductionDraft().Any(field=>field.Name=="DiscardedField") || !(bool)setting.Constants.Committed("Setting").Single(pair=>pair.Key=="Constants/Setting/MainSaveFlag").Value) throw new InvalidOperationException("Main Save No did not restore the saved configuration.");
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
