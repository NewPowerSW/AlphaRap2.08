using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AlphaRap;
using AlphaRap.Configuration;
using AlphaRap.FlowProgramming;
using AlphaRapLibrary;

internal static class ConfigurationProbe
{
    private static string root, current;
    private static int passed;

    private sealed class Feature : IFrameworkModule, IConfigurationParticipant
    {
        internal string Draft = "draft", Saved = "saved", Target;
        internal int Validations, Preparations, Accepts, Restores;
        internal int Order = 80;
        internal Action Validating, Preparing, Accepting, Discarding;
        internal bool FailValidate, FailPrepare, FailDiscard, CompareOriginal;
        internal byte[] Expected;
        internal ConfigurationFile[] ExtraFiles = new ConfigurationFile[0];
        public string Id { get; private set; }
        public string ConfigurationId { get; set; }
        public int ConfigurationOrder { get { return Order; } }
        /// <summary>声明一个新增文件配置模块，不创建页面。</summary>
        internal Feature(string id) { Id = id; ConfigurationId = id; Target = Path.Combine(current, id + ".json"); }
        /// <summary>通过应用模块宿主登记，初始化不写文件。</summary>
        public void Initialize(ModuleContext context) { }
        /// <summary>验证本次草稿并允许注入校验错误。</summary>
        public void ValidateConfiguration()
        {
            Validations++; if (Validating != null) Validating();
            if (FailValidate) throw new InvalidOperationException("validation");
        }
        /// <summary>捕获值和接受动作，序列化只操作事务暂存文件。</summary>
        public ConfigurationChange PrepareConfigurationSave()
        {
            Preparations++;
            if (FailPrepare) throw new InvalidOperationException("preparation");
            string captured = Draft;
            Action<string> write = path => File.WriteAllText(path, captured);
            ConfigurationFile file = CompareOriginal ? ConfigurationFile.CompareAndReplace(Target, Expected, write) : new ConfigurationFile(Target, write);
            var files = new List<ConfigurationFile> { file }; files.AddRange(ExtraFiles);
            var change = new ConfigurationChange(files, () => { if (Accepting != null) Accepting(); Saved = captured; Accepts++; });
            files.Clear(); if (Preparing != null) Preparing();
            return change;
        }
        /// <summary>先准备恢复动作，错误发生在任何恢复动作执行前。</summary>
        public Action PrepareConfigurationDiscard()
        {
            if (Discarding != null) Discarding();
            if (FailDiscard) throw new InvalidOperationException("discard");
            return () => { Draft = Saved; Restores++; };
        }
        /// <summary>本测试模块没有硬件或后台任务。</summary>
        public void Dispose() { }
    }

    /// <summary>先解析框架依赖，再运行独立配置故障注入。</summary>
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
        root = Path.GetFullPath(args[1]); return Run();
    }

    /// <summary>验证扩展发现、事务边界和失败恢复，不创建窗口或连接设备。</summary>
    private static int Run()
    {
        try
        {
            Test("registered file modules automatically join main save and discard", Extension);
            Test("acceptance happens only after every participant file exists", AcceptBoundary);
            Test("duplicate participant IDs fail before validation or writing", DuplicateIds);
            Test("validation failures preserve all drafts and files", ValidationFailure);
            Test("preparation failures never accept earlier snapshots", PreparationFailure);
            Test("late replacement failure restores replaced files and global recipe", ReplacementFailure);
            Test("conflicting normalized target paths are rejected before staging", DuplicatePaths);
            Test("compare-and-replace clones expected bytes and rejects external edits", CompareOriginal);
            Test("discard preflight failure leaves every participant draft intact", DiscardFailure);
            Test("nested save and discard are rejected and the outer operation stays usable", Reentrancy);
            Test("participant discovery is fixed for the complete operation", ParticipantSnapshot);
            Test("captured file lists and values survive later draft changes", FileSnapshot);
            Test("transition monitor loads historical stations and preserves the original flow file on Save", TransitionMonitor);
            Console.WriteLine("RESULT configuration participants: " + passed + "/13 passed.");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
        finally { ApplicationHost.Shutdown(); }
    }

    /// <summary>登记功能，不向保存核心增加分支。</summary>
    private static Feature Register(string id)
    {
        return ApplicationHost.Modules.Register(new Feature(id), new ModuleContext(SysPara.SettingDataDirectory));
    }

    /// <summary>普通文件模块由接口被发现，保存和放弃沿用同一主入口。</summary>
    private static void Extension()
    {
        Feature feature = Register("extension"); ProjectConfiguration.Save();
        Check(File.ReadAllText(feature.Target) == "draft" && feature.Saved == "draft" && feature.Accepts == 1, "The registered feature was omitted.");
        feature.Draft = "changed"; ProjectConfiguration.Discard(); Check(feature.Draft == "draft" && feature.Restores == 1, "Discard ignored the feature.");
    }

    /// <summary>接受内存快照前，所有参与者的正式文件都已落盘。</summary>
    private static void AcceptBoundary()
    {
        Feature one = Register("one"), two = Register("two");
        one.Accepting = () => Check(File.ReadAllText(two.Target) == "draft", "Acceptance preceded another file commit.");
        ProjectConfiguration.Save(); Check(one.Accepts == 1 && two.Accepts == 1, "Saved snapshots were not accepted once.");
    }

    /// <summary>重复业务身份不能悄悄覆盖配置功能。</summary>
    private static void DuplicateIds()
    {
        Feature one = Register("one"), two = Register("two"); two.ConfigurationId = one.ConfigurationId;
        Throws<InvalidOperationException>(() => ProjectConfiguration.Save());
        Check(one.Validations == 0 && two.Validations == 0 && !File.Exists(one.Target), "Duplicate IDs reached feature callbacks.");
    }

    /// <summary>校验失败不进入捕获或写入，修正后可以重试。</summary>
    private static void ValidationFailure()
    {
        Feature one = Register("one"), two = Register("two"); two.FailValidate = true;
        Throws<InvalidOperationException>(() => ProjectConfiguration.Save());
        Check(one.Preparations == 0 && one.Accepts == 0 && !File.Exists(one.Target), "Validation failure committed a snapshot.");
        two.FailValidate = false; ProjectConfiguration.Save(); Check(one.Accepts == 1, "Failure retained the operation guard.");
    }

    /// <summary>后续参与者准备失败时，前面的快照也不写入或接受。</summary>
    private static void PreparationFailure()
    {
        Feature one = Register("one"), two = Register("two"); two.FailPrepare = true;
        Throws<InvalidOperationException>(() => ProjectConfiguration.Save());
        Check(one.Preparations == 1 && one.Accepts == 0 && !File.Exists(one.Target), "Preparation failure committed earlier files.");
    }

    /// <summary>制造已完成前面替换后的失败，确认回滚与草稿保留。</summary>
    private static void ReplacementFailure()
    {
        Feature one = Register("one"), two = Register("two"); ProjectConfiguration.Save();
        byte[] recipe = File.ReadAllBytes(SysPara.FilePath);
        one.Draft = "new-one"; two.Draft = "new-two";
        string blocked = Path.Combine(current, "directory.json"); Directory.CreateDirectory(blocked); two.Target = blocked;
        Throws<IOException>(() => ProjectConfiguration.Save());
        Check(File.ReadAllText(one.Target) == "draft" && recipe.SequenceEqual(File.ReadAllBytes(SysPara.FilePath)), "A late failure did not roll back earlier replacements.");
        Check(one.Accepts == 1 && two.Accepts == 1 && one.Draft == "new-one", "Rollback accepted or discarded a draft.");
    }

    /// <summary>同一文件的不同大小写路径也属于冲突。</summary>
    private static void DuplicatePaths()
    {
        Feature one = Register("one"), two = Register("two"); two.Target = one.Target.ToUpperInvariant();
        Throws<IOException>(() => ProjectConfiguration.Save());
        Check(one.Accepts == 0 && !File.Exists(one.Target), "Conflicting paths were written.");
    }

    /// <summary>原始字节快照不与调用方共享，外部变更触发明确冲突。</summary>
    private static void CompareOriginal()
    {
        Feature feature = Register("compare"); File.WriteAllText(feature.Target, "saved");
        feature.CompareOriginal = true; feature.Expected = File.ReadAllBytes(feature.Target);
        feature.Preparing = () => feature.Expected[0] = (byte)'X';
        ProjectConfiguration.Save(); Check(feature.Accepts == 1, "Expected-original bytes were aliased.");
        feature.Expected = Encoding.UTF8.GetBytes("draft"); feature.Preparing = () => File.WriteAllText(feature.Target, "external");
        Throws<IOException>(() => ProjectConfiguration.Save());
        Check(File.ReadAllText(feature.Target) == "external" && feature.Accepts == 1, "An external edit was overwritten or accepted.");
    }

    /// <summary>先准备全部恢复动作，准备阶段错误不会执行前面的恢复动作。</summary>
    private static void DiscardFailure()
    {
        Feature one = Register("one"), two = Register("two"); two.FailDiscard = true;
        Throws<InvalidOperationException>(() => ProjectConfiguration.Discard());
        Check(one.Draft == "draft" && one.Restores == 0, "Discard preflight partially restored drafts.");
        two.FailDiscard = false; ProjectConfiguration.Discard(); Check(one.Draft == "saved", "Discard could not retry.");
    }

    /// <summary>参与者回调不能创建嵌套配置事务。</summary>
    private static void Reentrancy()
    {
        Feature feature = Register("nested");
        feature.Preparing = () => { Throws<InvalidOperationException>(() => ProjectConfiguration.Save()); Throws<InvalidOperationException>(() => ProjectConfiguration.Discard()); };
        ProjectConfiguration.Save();
        feature.Discarding = () => Throws<InvalidOperationException>(() => ProjectConfiguration.Save());
        ProjectConfiguration.Discard(); Check(feature.Accepts == 1 && feature.Restores == 1, "Nested operations broke the outer operation.");
    }

    /// <summary>操作中注册的功能留到下一次保存，避免未校验功能混入当前事务。</summary>
    private static void ParticipantSnapshot()
    {
        Feature first = Register("first"), late = null;
        first.Validating = () => { if (late == null) late = Register("late"); };
        ProjectConfiguration.Save(); Check(late.Validations == 0 && late.Accepts == 0 && !File.Exists(late.Target), "Participant membership changed during save.");
        ProjectConfiguration.Save(); Check(late.Validations == 1 && late.Accepts == 1, "The next save did not discover the new participant.");
    }

    /// <summary>准备后继续改变草稿时，只接受本次捕获的值。</summary>
    private static void FileSnapshot()
    {
        Feature feature = Register("snapshot"); feature.Preparing = () => feature.Draft = "later";
        ProjectConfiguration.Save();
        Check(File.ReadAllText(feature.Target) == "draft" && feature.Saved == "draft" && feature.Draft == "later", "A prepared snapshot followed later edits.");
    }

    /// <summary>确认监视页挂载已保存工站，主保存保留外部流程文件。</summary>
    private static void TransitionMonitor()
    {
        Directory.CreateDirectory(SysPara.SettingDataDirectory);
        var project = new NPSDK.FlowProject();
        project.Modules.Add(new NPSDK.FlowProgram { ModuleId = typeof(NPSDK.Flow_BaseForm).FullName + "/HistoricalStation", Mode = "Disabled" });
        NPSDK.FlowProgramFile.Save(FlowPrograms.FilePath, project);
        File.AppendAllText(FlowPrograms.FilePath, Environment.NewLine);
        byte[] before = File.ReadAllBytes(FlowPrograms.FilePath);
        using (var monitor = new Flow())
        {
            monitor.AddFlowChart();
            var tabs = (System.Windows.Forms.TabControl)monitor.Controls["tabControl1"];
            Check(tabs.TabPages.Cast<System.Windows.Forms.TabPage>().Any(page => page.Text == "HistoricalStation"), "Historical station missing.");
            Check(monitor.Controls.Count == 2 && !tabs.AllowDrop && monitor.Controls.Find("toolbox", true).Length == 0, "Flow monitor exposed an editing surface.");
            Check(typeof(Flow).Assembly.GetType("AlphaRap.FlowProgramming.FlowToolboxControl") == null, "Drag toolbox is present in this assembly.");
            Check(typeof(NPSDK.Flow_Chart).GetMethod("EnableProgramEditor") == null, "SDK node drag handlers are available.");
            ProjectConfiguration.Save();
            ProjectConfiguration.Discard();
            Check(File.ReadAllBytes(FlowPrograms.FilePath).SequenceEqual(before), "Main Save or Discard replaced the original flow.");
        }
    }

    /// <summary>每项使用独立目录及模块宿主，保持生产配置和窗口不受影响。</summary>
    private static void Test(string name, Action action)
    {
        ApplicationHost.Shutdown(); current = Path.Combine(root, passed.ToString()); Directory.CreateDirectory(current); Environment.CurrentDirectory = current;
        SysPara.SettingDataDirectory = Path.Combine(current, "Settings"); ModuleManager.SettingDataDirectory = SysPara.SettingDataDirectory;
        SysPara.LanguageDataDirectory = Path.Combine(root, "LanguageData"); SysPara.RecipeDataDirectory = Path.Combine(current, "Recipes");
        SysPara.RecipeName = "Model"; SysPara.FilePath = Path.Combine(SysPara.RecipeDataDirectory, "Model.xml");
        SysPara.UserPermission = PermissionType.Administrator; SysPara.SystemMode = RunMode.IDLE;
        action(); passed++; Console.WriteLine("PASS " + name);
    }
    /// <summary>断言具体配置语义。</summary>
    private static void Check(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
    /// <summary>要求明确失败，保留原异常类型。</summary>
    private static void Throws<T>(Action action) where T : Exception
    {
        try { action(); } catch (T) { return; }
        throw new InvalidOperationException("Expected " + typeof(T).Name);
    }
}
