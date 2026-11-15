using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using AlphaRap;
using AlphaRap.Configuration;
using AlphaRap.FlowProgramming;
using AlphaRapLibrary;

internal static class ModuleProbe
{
    private static string root;
    private static int passed;

    private sealed class SampleModule : IConfigurationModule
    {
        internal int Initializes, Disposals;
        internal bool FailInitialize, FailValidation, FailDispose;
        internal Action Disposing;
        public string Id { get; private set; }
        public ModuleConfiguration Configuration { get; private set; }
        /// <summary>模拟独立参数模块，不引用或创建任何窗体。</summary>
        internal SampleModule(string id, string storage)
        {
            Id = id; Configuration = Data(storage);
        }
        /// <summary>记录初始化并读取机器配置，允许注入注册失败。</summary>
        public void Initialize(ModuleContext context)
        {
            Initializes++;
            if (FailInitialize) throw new InvalidOperationException("initialization");
            Configuration.LoadSettings(context.SettingsDirectory);
        }
        /// <summary>模拟草稿校验失败，不提交内存版本。</summary>
        public void ValidateConfiguration() { if (FailValidation) throw new InvalidOperationException("validation"); }
        /// <summary>记录释放顺序并允许注入释放故障。</summary>
        public void Dispose() { Disposals++; if (Disposing != null) Disposing(); if (FailDispose) throw new InvalidOperationException("disposal"); }
    }

    /// <summary>先设置程序集解析，再进入实际测试。</summary>
    [STAThread]
    private static int Main(string[] args)
    {
        AppDomain.CurrentDomain.AssemblyResolve += (sender, request) =>
        {
            string name = new AssemblyName(request.Name).Name;
            foreach (string directory in new[] { args[0], @"C:\Program Files\Cognex\VisionPro\ReferencedAssemblies" })
                foreach (string extension in new[] { ".dll", ".exe" })
                    if (File.Exists(Path.Combine(directory, name + extension))) return Assembly.LoadFrom(Path.Combine(directory, name + extension));
            return null;
        };
        root = Path.GetFullPath(args[1]); Environment.CurrentDirectory = root;
        return Run();
    }

    /// <summary>验证无窗口模块、配置兼容与统一保存，不启动设备或生产流程。</summary>
    private static int Run()
    {
        SysPara.LanguageDataDirectory = Path.Combine(root, "LanguageData");
        SysPara.SettingDataDirectory = Path.Combine(root, "Settings");
        ModuleManager.SettingDataDirectory = SysPara.SettingDataDirectory; Directory.CreateDirectory(SysPara.SettingDataDirectory);
        SysPara.RecipeDataDirectory = Path.Combine(root, "Recipes");
        SysPara.RecipeName = "Model"; SysPara.FilePath = Path.Combine(SysPara.RecipeDataDirectory, "Model.xml");
        SysPara.UserPermission = PermissionType.Administrator; SysPara.SystemMode = RunMode.IDLE;
        FlowSourceGenerator.SourceProjectPath = ""; FlowSourceGenerator.OutputDirectory = Path.Combine(root, "Generated");
        try
        {
            Test("headless parameter initialization preserves legacy keys and uses no forms", HeadlessParameters);
            Test("registration is explicit and duplicate identities cannot replace modules", Registration);
            Test("failed registration cleans up and reverse disposal reaches every module", LifecycleFailure);
            Test("recipe namespaces isolate same-named typed tables and preserve settings", RecipeScopes);
            Test("failed XML reads preserve existing drafts and committed versions", ReadFailure);
            Test("headless modules join main save discard and recipe loading", MainSave);
            Test("legacy recipe writers preserve registered modules without views", LegacyRecipeSave);
            Test("main save failures retain drafts and leave persisted values unchanged", SaveFailure);
            Test("parameter view shares one model and display changes keep its storage identity", ViewBinding);
            Test("legacy page captions fall back to stable configuration names", LegacyIdentity);
            Console.WriteLine("RESULT framework modules: " + passed + "/10 passed; offline configuration only.");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
        finally { ApplicationHost.Shutdown(); }
    }

    /// <summary>建立同名但不同命名空间的参数表，以验证模块身份隔离。</summary>
    private static ModuleConfiguration Data(string name)
    {
        DataSet settings = new DataSet("NewDataSet"), recipe = new DataSet("NewDataSet");
        settings.Tables.Add("MSet").Columns.Add("Value", typeof(int)).DefaultValue = 7;
        recipe.Tables.Add("RSet").Columns.Add("Value", typeof(string)).DefaultValue = "";
        return new ModuleConfiguration(name, settings, recipe);
    }

    /// <summary>初始化兼容布尔设置，不创建 Form 或登记视图。</summary>
    private static void HeadlessParameters()
    {
        int forms = ModuleManager.ModuleList.Count;
        using (var module = new SystemParametersModule(new[] { new BooleanSettingDefault("Switch", "开关", "Switch", "Interruptor", true) }))
        {
            module.Initialize(new ModuleContext(SysPara.SettingDataDirectory));
            Check(ModuleManager.ModuleList.Count == forms && module.Id == "system.parameters", "A data module created a view.");
            Check(module.Configuration.StorageName == "SysSetting", "Legacy storage identity changed.");
            module.Constants.Put("Setting", "Switch", new ConstantValue { Key = "Switch", Type = "Boolean", Value = "True" });
            Check((bool)module.Configuration.Settings.Tables["PSet"].Rows[0]["Switch"], "Legacy Boolean column diverged.");
            module.ValidateConfiguration();
            module.Dispose();
            Throws<ObjectDisposedException>(() => module.ValidateConfiguration());
        }
    }

    /// <summary>构造不注册，同一实例注册幂等，另一个同 Id 实例不能覆盖。</summary>
    private static void Registration()
    {
        using (var registry = new ModuleRegistry())
        {
            var module = new SampleModule("sample", "Sample");
            Check(registry.Modules.Count == 0 && module.Initializes == 0, "Construction registered a module.");
            registry.Register(module, new ModuleContext(SysPara.SettingDataDirectory));
            registry.Register(module, new ModuleContext(SysPara.SettingDataDirectory));
            Check(module.Initializes == 1, "Registration initialized twice.");
            Throws<InvalidOperationException>(() => registry.Register(new SampleModule("sample", "Other"), new ModuleContext(SysPara.SettingDataDirectory)));
            Check(ReferenceEquals(registry.Modules.Single(), module), "Duplicate registration replaced the owner.");
        }
    }

    /// <summary>初始化失败保留错误并释放半成品，释放失败仍继续处理其他模块。</summary>
    private static void LifecycleFailure()
    {
        var registry = new ModuleRegistry(); var failed = new SampleModule("failed", "Failed") { FailInitialize = true, FailDispose = true };
        Throws<AggregateException>(() => registry.Register(failed, new ModuleContext(SysPara.SettingDataDirectory)));
        Check(failed.Disposals == 1 && registry.Modules.Count == 0, "Failed initialization leaked registration.");
        string order = "";
        var one = new SampleModule("one", "One") { Disposing = () => order += "1" };
        var two = new SampleModule("two", "Two") { FailDispose = true, Disposing = () => order += "2" };
        registry.Register(one, new ModuleContext(SysPara.SettingDataDirectory)); registry.Register(two, new ModuleContext(SysPara.SettingDataDirectory));
        Throws<AggregateException>(() => registry.Dispose()); registry.Dispose();
        Check(order == "21" && one.Disposals == 1 && two.Disposals == 1, "Module disposal order or failure isolation changed.");
        Throws<ObjectDisposedException>(() => registry.Register(one, new ModuleContext(SysPara.SettingDataDirectory)));
    }

    /// <summary>同名参数表按照模块命名空间加载，表引用及机器参数不随配方替换。</summary>
    private static void RecipeScopes()
    {
        var one = Data("First"); var two = Data("Second");
        one.LoadSettings(SysPara.SettingDataDirectory); two.LoadSettings(SysPara.SettingDataDirectory);
        one.LoadRecipe("absent.xml"); two.LoadRecipe("absent.xml");
        one.Recipe.Tables[0].Rows[0][0] = "A"; two.Recipe.Tables[0].Rows[0][0] = "B";
        var combined = new DataSet("NewDataSet"); combined.Merge(one.Recipe); combined.Merge(two.Recipe);
        string path = Path.Combine(root, "scopes.xml"); combined.WriteXml(path);
        DataTable reference = one.Recipe.Tables[0]; one.Recipe.Tables[0].Rows[0][0] = "draft";
        one.LoadRecipe(path); two.LoadRecipe(path);
        Check((string)reference.Rows[0][0] == "A" && (string)two.Recipe.Tables[0].Rows[0][0] == "B", "Recipe namespaces mixed values.");
        Check(ReferenceEquals(reference, one.Recipe.Tables[0]) && (int)one.Settings.Tables[0].Rows[0][0] == 7, "Loading replaced bindings or machine settings.");
        reference.ExtendedProperties["AllowEmpty"] = true; one.LoadRecipe("absent.xml"); Check(reference.Rows.Count == 0, "Empty detail table gained a row.");
    }

    /// <summary>损坏 XML 不清空当前编辑内容，也不提前提交 Original 版本。</summary>
    private static void ReadFailure()
    {
        var data = Data("Broken"); data.LoadSettings(SysPara.SettingDataDirectory); data.LoadRecipe("absent.xml");
        data.Settings.Tables[0].Rows[0][0] = 12;
        File.WriteAllText(data.SettingsPath(SysPara.SettingDataDirectory), "<broken>");
        Throws<System.Xml.XmlException>(() => data.LoadSettings(SysPara.SettingDataDirectory));
        Check((int)data.Settings.Tables[0].Rows[0][0] == 12 && (int)data.Settings.Tables[0].Rows[0][0, DataRowVersion.Original] == 7, "Failed load lost a settings draft.");
    }

    /// <summary>仅注册新模块即可参与既有主保存、放弃和配方加载。</summary>
    private static void MainSave()
    {
        var module = ApplicationHost.Modules.Register(new SampleModule("headless", "Headless"), new ModuleContext(SysPara.SettingDataDirectory));
        ProjectConfiguration.LoadModuleRecipes("absent.xml");
        module.Configuration.Settings.Tables[0].Rows[0][0] = 23; module.Configuration.Recipe.Tables[0].Rows[0][0] = "Recipe-A";
        Check(ProjectConfiguration.HasModuleRecipeChanges, "New module recipe draft was ignored.");
        ProjectConfiguration.Save();
        Check(!module.Configuration.Settings.HasChanges() && !ProjectConfiguration.HasModuleRecipeChanges, "Successful main save did not accept versions.");
        var loaded = Data("Headless"); loaded.LoadSettings(SysPara.SettingDataDirectory); loaded.LoadRecipe(SysPara.FilePath);
        Check((int)loaded.Settings.Tables[0].Rows[0][0] == 23 && (string)loaded.Recipe.Tables[0].Rows[0][0] == "Recipe-A", "Main save omitted a headless module.");
        Check((int)FlowResources.Capture().Parameters["Machine/headless/MSet/0/Value"] == 23, "Flow resources ignored the registered data module.");
        module.Configuration.Settings.Tables[0].Rows[0][0] = 99; ProjectConfiguration.Discard();
        Check((int)module.Configuration.Settings.Tables[0].Rows[0][0] == 23, "Discard did not restore a registered module.");
    }

    /// <summary>旧托盘等配方写入入口不会清除无窗口模块数据。</summary>
    private static void LegacyRecipeSave()
    {
        using (var legacy = new ModuleBaseForm { Name = "Legacy", Text = "Legacy" })
        {
            try
            {
                legacy.ModuleInitialize("Legacy");
                string path = Path.Combine(root, "legacy.xml"); legacy.WriteRecipeData(path);
                var loaded = Data("Headless"); loaded.LoadRecipe(path);
                Check((string)loaded.Recipe.Tables[0].Rows[0][0] == "Recipe-A", "Legacy writer dropped registered module data.");
            }
            finally { ModuleManager.ModuleList.Remove(legacy); }
        }
    }

    /// <summary>共享主保存遇到文件占用时不提交草稿，重试和放弃仍可使用。</summary>
    private static void SaveFailure()
    {
        var module = (SampleModule)ApplicationHost.Modules.Modules.Single();
        module.Configuration.Settings.Tables[0].Rows[0][0] = 32;
        using (FileStream occupied = File.Open(module.Configuration.SettingsPath(SysPara.SettingDataDirectory), FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            Throws<IOException>(() => ProjectConfiguration.Save());
        Check(module.Configuration.Settings.HasChanges() && (int)module.Configuration.Settings.Tables[0].Rows[0][0, DataRowVersion.Original] == 23, "Failed save accepted a draft.");
        module.FailValidation = true; Throws<InvalidOperationException>(() => ProjectConfiguration.Save()); module.FailValidation = false;
        ProjectConfiguration.Discard(); Check((int)module.Configuration.Settings.Tables[0].Rows[0][0] == 23, "Failed save changed the persisted value.");
        ApplicationHost.Shutdown();
    }

    /// <summary>参数页绑定同一业务模型，显示变化和关闭页面不丢失模块数据。</summary>
    private static void ViewBinding()
    {
        var shared = ApplicationHost.Modules.Register(new SystemParametersModule(new BooleanSettingDefault[0]), new ModuleContext(SysPara.SettingDataDirectory));
        var form = new SystemSetting(shared);
        try
        {
            form.RecipeData.Tables.Add("Extension").Columns.Add("Value", typeof(string));
            ApplicationHost.RegisterModule(form);
            Check(form.RecipeData.Tables["Extension"].Namespace == "SysSetting", "Page extensions lost their recipe namespace.");
            var model = form.Parameters;
            Check(ReferenceEquals(form.SettingData, model.Configuration.Settings) && ApplicationHost.Modules.Modules.Count == 1, "View created another configuration.");
            form.Text = "显示标题";
            Check(form.ConfigurationData.SettingsPath(SysPara.SettingDataDirectory).EndsWith("SysSetting.xml"), "Display text changed the settings file.");
            foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
            {
                model.Constants.Put("Recipe", ConstantStore.Find(model.Constants.Table("Recipe"), "Value") == null ? null : "Value", new ConstantValue { Key = "Value", Type = "Text", Value = "draft" });
                ApplicationHost.InitializeLanguage(language);
                Check(Convert.ToString(ConstantStore.Find(model.Constants.Table("Recipe"), "Value")["Value"]) == "draft", "Language switch changed data.");
            }
            form.Dispose(); ModuleManager.ModuleList.Remove(form);
            model.ValidateConfiguration(); Check(ApplicationHost.Modules.Modules.Single() == model, "Closing a view removed the business module.");
            ProjectConfiguration.Save();
            Check((string)FlowResources.Capture().Parameters["Constants/Recipe/Value"] == "draft", "Flow parameters depended on the disposed view.");
        }
        finally { form.Dispose(); ModuleManager.ModuleList.Remove(form); }
    }

    /// <summary>标题为空或包含路径分隔符时仍能初始化，既有有效存储名不迁移。</summary>
    private static void LegacyIdentity()
    {
        using (var page = new ModuleBaseForm { Name = "StableLegacy", Text = "Header / Display | page" })
        {
            try
            {
                page.ModuleInitialize(page.Text);
                Check(page.ConfigurationData.StorageName == "StableLegacy", "A display caption became a file name.");
                page.Text = "Changed";
                Check(page.ConfigurationData.StorageName == "StableLegacy", "The resolved identity changed later.");
            }
            finally { ModuleManager.ModuleList.Remove(page); }
        }
        using (var page = new ModuleBaseForm { Name = "", Text = "" })
        {
            try { page.ModuleInitialize(""); Check(page.ConfigurationData.StorageName == "ModuleBaseForm", "Anonymous legacy page could not initialize."); }
            finally { ModuleManager.ModuleList.Remove(page); }
        }
    }

    /// <summary>输出各项独立验证的结果。</summary>
    private static void Test(string name, Action action) { action(); passed++; Console.WriteLine("PASS " + name); }
    /// <summary>断言失败时保留具体业务原因。</summary>
    private static void Check(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
    /// <summary>要求操作明确失败，不允许空结果冒充成功。</summary>
    private static void Throws<T>(Action action) where T : Exception
    {
        try { action(); } catch (T) { return; }
        throw new InvalidOperationException("Expected " + typeof(T).Name);
    }
}
