using System;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AlphaRap;
using AlphaRap.Security;
using AlphaRap.Configuration;
using AlphaRap.FlowProgramming;
using AlphaRapLibrary;
using Cognex.VisionPro;
using Cognex.VisionPro.ToolBlock;

internal static class PriorityProbe
{
    private static string root;
    private static int passed;
    /// <summary>先解析实际待测程序集，不使用现场数据库。</summary>
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
        if (args.Length > 2) return Child(args[2], args[3]);
        return Run(args[0]);
    }

    /// <summary>在新建隔离 MDB 和模型中验证五项框架边界。</summary>
    private static int Run(string assemblies)
    {
        SysPara.LanguageDataDirectory = Path.Combine(root, "LanguageData");
        try
        {
            Test("password hashes use independent salts and reject invalid records", Passwords);
            Test("legacy accounts migrate after successful authentication only", Migration);
            Test("quoted names remain data and new accounts never store plaintext", ParameterizedAccounts);
            Test("account mutations require administrator authorization", Authorization);
            Test("machine transitions publish consistent immutable snapshots", StateTransitions);
            Test("legacy state assignments cannot publish contradictory pairs", LegacyState);
            Test("shutdown retains both blocked work and blocked stop callbacks", Supervision);
            Test("task failures are observed and late tasks receive stop requests", LateTasks);
            Test("host shutdown shares in-flight work and releases modules only after actual exit", HostShutdown);
            Test("recipe application failure restores every applied participant", RecipeRollback);
            Test("module rollback preserves table identity and original row versions", ModuleRollback);
            Test("recipe preparation rejects invalid data without touching active values", RecipePreflight);
            Test("missing recipe VPP preserves all modules, model references and identity", () => WholeRecipe("missing"));
            Test("recipe selection write failure restores data and model references", () => WholeRecipe("write"));
            Test("complete recipe commits models and identity and blocks unsaved replacement", () => WholeRecipe("success"));
            foreach (string stage in new[] { "Prepared", "File:0", "Committed" })
            {
                string point = stage;
                Test("abrupt process exit recovers " + point, () => CrashRecovery(assemblies, point));
            }
            Test("publication failure fences operations while retaining committed files", () => Publication(assemblies));
            Test("failed recipe compensation remains fenced after an ordinary stop reset", () => Launch(assemblies, "recipe-failure", ""));
            Test("unknown external edits are preserved and recovery refuses to overwrite", () => Conflict(assemblies));
            Console.WriteLine("RESULT priority framework: " + passed + "/21 passed; isolated fixtures only.");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }

    /// <summary>创建与旧现场相同的短密码列，不从生产 MDB 复制账号。</summary>
    private static string Database(string name)
    {
        string path = Path.Combine(root, name + ".mdb");
        var builder = new OleDbConnectionStringBuilder { Provider = "Microsoft.ACE.OLEDB.12.0", DataSource = path };
        dynamic catalog = Activator.CreateInstance(Type.GetTypeFromProgID("ADOX.Catalog", true));
        try { catalog.Create(builder.ConnectionString); dynamic active = catalog.ActiveConnection; active.Close(); Marshal.FinalReleaseComObject(active); }
        finally { Marshal.FinalReleaseComObject(catalog); }
        using (var connection = new OleDbConnection(builder.ConnectionString))
        {
            connection.Open();
            using (var command = new OleDbCommand("CREATE TABLE [UserData] ([UserName] TEXT(50),[Password] TEXT(50),[Permission] TEXT(50))", connection)) command.ExecuteNonQuery();
        }
        return path;
    }

    /// <summary>写入明确的测试旧账号，不包含现场凭据。</summary>
    private static void Legacy(string path, string name, string password, string permission)
    {
        using (var connection = new OleDbConnection(new OleDbConnectionStringBuilder { Provider = "Microsoft.ACE.OLEDB.12.0", DataSource = path }.ConnectionString))
        {
            connection.Open();
            using (var command = new OleDbCommand("INSERT INTO [UserData] ([UserName],[Password],[Permission]) VALUES (?,?,?)", connection))
            {
                command.Parameters.AddWithValue("n", name); command.Parameters.AddWithValue("p", password); command.Parameters.AddWithValue("r", permission); command.ExecuteNonQuery();
            }
        }
    }

    /// <summary>验证哈希随机性、完整格式与错误密码。</summary>
    private static void Passwords()
    {
        string one = PasswordCredentials.Hash("Test credential"), two = PasswordCredentials.Hash("Test credential");
        Check(one != two && !one.Contains("Test credential") && PasswordCredentials.Verify("Test credential", one), "Salt or hash verification failed.");
        Check(!PasswordCredentials.Verify("wrong", one) && !PasswordCredentials.Verify("", "np1$999999999$bad$bad"), "Invalid credential was accepted.");
    }

    /// <summary>错误登录不迁移；成功登录在同一更新中写哈希并清空旧密码。</summary>
    private static void Migration()
    {
        string path = Database("migration"); Legacy(path, "administrator", "fixture-password", "Administrator");
        var accounts = new AccountService(path);
        Check(accounts.Authenticate("administrator", "wrong") == null && accounts.Authenticate("' OR '1'='1", "fixture-password") == null, "Invalid authentication succeeded.");
        AccountIdentity identity = accounts.Authenticate("administrator", "fixture-password");
        Check(identity != null && identity.Permission == PermissionType.Administrator, "Legacy login failed.");
        using (var connection = new OleDbConnection(new OleDbConnectionStringBuilder { Provider = "Microsoft.ACE.OLEDB.12.0", DataSource = path }.ConnectionString))
        {
            connection.Open();
            using (var command = new OleDbCommand("SELECT [Password],[PasswordHash] FROM [UserData]", connection))
            using (OleDbDataReader reader = command.ExecuteReader())
            {
                Check(reader.Read() && Convert.ToString(reader[0]) == "" && PasswordCredentials.Verify("fixture-password", Convert.ToString(reader[1])), "Migration retained plaintext or lost the credential.");
            }
        }
        Check(accounts.Authenticate("administrator", "fixture-password") != null && accounts.Authenticate("administrator", "") == null, "Migrated account fell back to the blank legacy column.");
    }

    /// <summary>查询、插入、改权和删除均把带引号的用户名作为普通数据。</summary>
    private static void ParameterizedAccounts()
    {
        SysPara.UserPermission = PermissionType.Administrator;
        string path = Database("quoted"); var accounts = new AccountService(path);
        accounts.Add("O'Brien", "fixture-password", "Maintenance"); accounts.Add("Other", "other-password", "Operator");
        Check(accounts.Authenticate("O'Brien", "fixture-password") != null && !accounts.Exists("O'Brien' OR '1'='1"), "User input became SQL.");
        accounts.ChangePermission("O'Brien", "Operator"); Check(accounts.FindIdentity("Other").Permission == PermissionType.Operator, "Role update changed another account.");
        accounts.Delete("O'Brien"); Check(!accounts.Exists("O'Brien") && accounts.Exists("Other"), "Delete changed another account.");
        Check(accounts.Users().Columns.Count == 2, "Public user listing exposes credential columns.");
    }

    /// <summary>绕过按钮直接调用账号修改时仍拒绝低权限身份。</summary>
    private static void Authorization()
    {
        string path = Database("authorization"); var accounts = new AccountService(path); SysPara.UserPermission = PermissionType.Operator;
        Throws<UnauthorizedAccessException>(() => accounts.Add("name", "fixture-password", "Administrator"));
        Throws<UnauthorizedAccessException>(() => accounts.ChangePermission("name", "Administrator"));
        Throws<UnauthorizedAccessException>(() => accounts.Delete("name"));
        Check(accounts.Users().Rows.Count == 0, "Rejected account operation wrote data.");
        SysPara.UserPermission = PermissionType.Administrator;
    }

    /// <summary>非法转换不改变状态；停止失败保留停止中且不伪装待机。</summary>
    private static void StateTransitions()
    {
        var state = new MachineState(); MachineStateSnapshot initial = state.Current;
        Throws<InvalidOperationException>(() => state.Start(false));
        state.Initialize(); state.Start(true); MachineStateSnapshot running = state.Current;
        state.Pause(); Check(state.Current.Mode == RunMode.PAUSE && state.Current.Automatic, "Pause lost its automatic session.");
        state.Start(true); state.BeginStop(); Throws<InvalidOperationException>(() => state.Start(true));
        state.CompleteStop(false); Check(state.Current.Mode == RunMode.STOPPING && !state.Current.Automatic, "Failed stop announced idle.");
        state.CompleteStop(true); Check(state.Current.Mode == RunMode.IDLE && !state.Current.Automatic && state.Current.Revision > running.Revision, "Stop did not publish idle atomically.");
        Check(initial.Mode == RunMode.IDLE && running.Mode == RunMode.RUN && running.Automatic, "Published snapshot was mutated.");
    }

    /// <summary>旧赋值入口与并发读取均使用同一完整状态组合。</summary>
    private static void LegacyState()
    {
        Parallel.For(0, 1000, i =>
        {
            SysPara.SystemMode = RunMode.IDLE; SysPara.SystemRun = true; SysPara.SystemRun = false;
            MachineStateSnapshot snapshot = MachineState.Global.Current;
            Check(snapshot.Mode != RunMode.RUN || snapshot.Automatic, "RUN was published without automatic state.");
            Check(snapshot.Mode != RunMode.IDLE || !snapshot.Automatic, "Idle retained an automatic session.");
        });
        SysPara.SystemMode = RunMode.IDLE;
    }

    /// <summary>完成等待只取决于真实工作及取消回调，两者分别阻塞也不能提前释放。</summary>
    private static void Supervision()
    {
        var supervisor = new ApplicationTasks(); var work = new TaskCompletionSource<int>();
        var stop = new TaskCompletionSource<int>(); var called = new ManualResetEventSlim();
        supervisor.Track("blocked", work.Task, () => { called.Set(); return stop.Task; });
        Task shutdown = supervisor.StopAllAsync(); Check(called.Wait(3000), "Stop was not dispatched.");
        Check(!shutdown.IsCompleted && supervisor.Pending.Length == 1, "Blocked work was released.");
        work.SetResult(0); Check(!shutdown.Wait(50) && supervisor.Pending.Length == 1, "Stop callback was abandoned after work completed.");
        stop.SetResult(0); Check(shutdown.Wait(3000) && supervisor.Pending.Length == 0, "Completed work remained occupied.");
        supervisor.BeginScope();
    }

    /// <summary>停止期间新增的工作也受监督，异常由监督器与原调用方共同观察。</summary>
    private static void LateTasks()
    {
        var supervisor = new ApplicationTasks(); var one = new TaskCompletionSource<int>(); var two = new TaskCompletionSource<int>();
        supervisor.Track("first", one.Task); Task shutdown = supervisor.StopAllAsync();
        var stopped = new ManualResetEventSlim();
        supervisor.Track("late", two.Task, () => { stopped.Set(); two.TrySetResult(0); return Task.FromResult(0); });
        Check(stopped.Wait(3000), "Late work did not receive stop."); one.SetResult(0); Check(shutdown.Wait(3000), "Late work broke shutdown.");
        supervisor.BeginScope(); var failure = new TaskCompletionSource<int>(); supervisor.Track("fault", failure.Task);
        Task finishing = supervisor.StopAllAsync(); failure.SetException(new IOException("fixture failure"));
        Throws<AggregateException>(() => finishing.Wait(3000));
        Check(supervisor.Failures.Any(item => item.Contains("fixture failure")) && supervisor.Pending.Length == 0, "Task failure was lost.");
    }

    private sealed class ShutdownApplication : MachineApplication
    {
        internal readonly TaskCompletionSource<int> Stop = new TaskCompletionSource<int>();
        internal int Calls;
        /// <summary>创建不连接硬件或创建窗口的宿主。</summary>
        public override void Initialize(Action<string> progress) { }
        /// <summary>以可控异步闸门模拟真实应用停止。</summary>
        public override Task ShutdownAsync() { Calls++; return Stop.Task; }
    }
    private sealed class ShutdownModule : IFrameworkModule
    {
        internal bool Disposed;
        public string Id { get { return "shutdown-fixture"; } }
        /// <summary>本测试模块不初始化硬件。</summary>
        public void Initialize(ModuleContext context) { }
        /// <summary>记录模型释放时机。</summary>
        public void Dispose() { Disposed = true; }
    }

    /// <summary>并发关闭共享任务；应用和后台工作都退出后才释放模块。</summary>
    private static void HostShutdown()
    {
        var app = new ShutdownApplication(); ApplicationHost.Initialize(app);
        var module = ApplicationHost.Modules.Register(new ShutdownModule(), new ModuleContext(root));
        var work = new TaskCompletionSource<int>(); var called = new ManualResetEventSlim();
        ApplicationTasks.Global.Track("host-blocked", work.Task, () => { called.Set(); return Task.FromResult(0); });
        Task shutdown = ApplicationHost.ShutdownAsync();
        Check(ReferenceEquals(shutdown, ApplicationHost.ShutdownAsync()) && app.Calls == 1 && ApplicationHost.IsShuttingDown && !module.Disposed, "Shutdown was duplicated or released early.");
        app.Stop.SetResult(0); Check(called.Wait(3000) && !shutdown.IsCompleted && !module.Disposed, "Blocked work did not retain its models.");
        work.SetResult(0); Check(shutdown.Wait(3000) && module.Disposed && !ApplicationHost.IsShuttingDown, "Shutdown did not release completed models.");
        ApplicationTasks.Global.BeginScope();
    }

    /// <summary>失败的应用动作也参与逆序补偿，发布失败同样恢复旧内存。</summary>
    private static void RecipeRollback()
    {
        int one = 1, two = 2; string order = "";
        Throws<IOException>(() => RecipeChange.Commit(new[] {
            new RecipeChange(() => one = 10, () => { one = 1; order += "1"; }),
            new RecipeChange(() => { two = 20; throw new IOException("apply"); }, () => { two = 2; order += "2"; }) }, () => { }));
        Check(one == 1 && two == 2 && order == "21", "Partial recipe was retained.");
        Throws<IOException>(() => RecipeChange.Commit(new[] { new RecipeChange(() => one = 10, () => one = 1) }, () => { throw new IOException("publish"); }));
        Check(one == 1, "Publication failure retained new recipe values.");
    }

    /// <summary>配方失败后保留同一表实例、草稿值和保存前 Original 值。</summary>
    private static void ModuleRollback()
    {
        var recipe = new DataSet(); var table = new DataTable("Values"); table.Columns.Add("Value", typeof(int)); recipe.Tables.Add(table);
        var module = new ModuleConfiguration("fixture", new DataSet(), recipe);
        table.Rows.Add(1); recipe.AcceptChanges(); table.Rows[0][0] = 2;
        string path = Path.Combine(root, "next.xml"); var next = recipe.Copy(); next.Tables[0].Rows[0][0] = 9; next.AcceptChanges(); next.WriteXml(path);
        Throws<IOException>(() => RecipeChange.Commit(new[] { module.PrepareRecipe(path) }, () => { throw new IOException("publish"); }));
        Check(ReferenceEquals(recipe.Tables[0], table) && (int)table.Rows[0][0] == 2 && (int)table.Rows[0][0, DataRowVersion.Original] == 1 && table.Rows[0].RowState == DataRowState.Modified, "Rollback lost binding or row versions.");
    }

    /// <summary>业务校验在准备阶段读取候选数据，失败不改变当前配方。</summary>
    private static void RecipePreflight()
    {
        var recipe = new DataSet(); var table = new DataTable("Values"); table.Columns.Add("Value", typeof(int)); recipe.Tables.Add(table);
        var module = new ModuleConfiguration("fixture", new DataSet(), recipe); table.Rows.Add(1); recipe.AcceptChanges();
        string path = Path.Combine(root, "invalid.xml"); var next = recipe.Copy(); next.Tables[0].Rows[0][0] = -1; next.WriteXml(path);
        module.ValidateRecipe = data => { if ((int)data.Tables[0].Rows[0][0] < 0) throw new InvalidDataException("invalid"); };
        Throws<InvalidDataException>(() => module.PrepareRecipe(path)); Check((int)table.Rows[0][0] == 1 && !recipe.HasChanges(), "Preflight mutated the active recipe.");
    }

    private sealed class RecipeModule : IConfigurationModule
    {
        public string Id { get { return "recipe-fixture"; } }
        public ModuleConfiguration Configuration { get; private set; }
        /// <summary>创建可参与整份配方切换的无窗口模块。</summary>
        public RecipeModule()
        {
            var recipe = new DataSet(); var table = new DataTable("Values"); table.Columns.Add("Value", typeof(int)); recipe.Tables.Add(table);
            Configuration = new ModuleConfiguration("fixture", new DataSet(), recipe); table.Rows.Add(1); recipe.AcceptChanges();
        }
        /// <summary>测试模块不读取真实机器配置。</summary>
        public void Initialize(ModuleContext context) { }
        /// <summary>整份配方测试使用合法的单列表。</summary>
        public void ValidateConfiguration() { }
        /// <summary>测试模块没有设备资源。</summary>
        public void Dispose() { }
    }
    private sealed class RecipeVision : VisionproInterface { }

    /// <summary>通过完整配方协调入口验证模块、视觉引用和选择文件的共同提交边界。</summary>
    private static void WholeRecipe(string scenario)
    {
        var module = new RecipeModule(); SysPara.SystemMode = RunMode.IDLE; SysPara.RecipeName = "Old";
        SysPara.VisionFileDirectory = Path.Combine(root, "vision" + passed);
        ApplicationHost.Modules.Register(module, new ModuleContext(root));
        var one = new RecipeVision { VppFolderName = "One", TB = new CogToolBlock(), IsLoadTBOk = true };
        var two = new RecipeVision { VppFolderName = "Two", TB = new CogToolBlock(), IsLoadTBOk = true };
        CogToolBlock oldOne = one.TB, oldTwo = two.TB;
        string selection = Path.Combine(root, "selection" + passed + ".txt"); File.WriteAllText(selection, "Old");
        string recipe = Path.Combine(root, "Next.xml"); DataSet next = module.Configuration.Recipe.Copy(); next.Tables[0].Rows[0][0] = 9; next.WriteXml(recipe);
        foreach (VisionproInterface station in new[] { one, two })
        {
            if (scenario == "missing" && ReferenceEquals(station, two)) continue;
            string path = station.GetVppPath("Next"); Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (var block = new CogToolBlock()) CogSerializer.SaveObjectToFile(block, path);
        }
        try
        {
            Action change = () => ProjectConfiguration.ChangeRecipe(recipe, () => SysPara.RecipeName = "Next", new[] { new ConfigurationFile(selection, path =>
            {
                if (scenario == "write") throw new IOException("selection fixture"); File.WriteAllText(path, "Next");
            }) });
            if (scenario != "success")
            {
                Throws<IOException>(change);
                Check(ReferenceEquals(one.TB, oldOne) && ReferenceEquals(two.TB, oldTwo) && (int)module.Configuration.Recipe.Tables[0].Rows[0][0] == 1 && SysPara.RecipeName == "Old" && File.ReadAllText(selection) == "Old", "Whole recipe failure retained mixed state.");
                return;
            }
            change(); Check(!ReferenceEquals(one.TB, oldOne) && !ReferenceEquals(two.TB, oldTwo) && one.ModelSignature.Length > 0 && (int)module.Configuration.Recipe.Tables[0].Rows[0][0] == 9 && SysPara.RecipeName == "Next" && File.ReadAllText(selection) == "Next", "Whole recipe was not committed.");
            module.Configuration.Recipe.Tables[0].Rows[0][0] = 10;
            Throws<InvalidOperationException>(change); Check((int)module.Configuration.Recipe.Tables[0].Rows[0][0] == 10, "Switch silently replaced a recipe draft.");
        }
        finally
        {
            VisionproInterface.VList.Remove(one); VisionproInterface.VList.Remove(two);
            if (!ReferenceEquals(one.TB, oldOne)) one.TB.Dispose(); if (!ReferenceEquals(two.TB, oldTwo)) two.TB.Dispose();
            oldOne.Dispose(); oldTwo.Dispose(); ApplicationHost.Shutdown(); ApplicationTasks.Global.BeginScope(); SysPara.SystemMode = RunMode.IDLE;
        }
    }

    /// <summary>仅在测试子进程的独立目录中写入文件。</summary>
    private static int Child(string mode, string stage)
    {
        try
        {
            SysPara.LanguageDataDirectory = Path.Combine(root, "LanguageData");
            if (mode == "publication") return PublishChild();
            if (mode == "recipe-failure")
            {
                Throws<AggregateException>(() => RecipeChange.Commit(new[] { new RecipeChange(() => { throw new IOException("apply"); }, () => { throw new IOException("restore"); }) }, () => { }));
                Check(ProjectConfiguration.RecoveryRequired && SysPara.SystemMode == RunMode.STOPPING, "Failed compensation was not fenced.");
                MachineState.Global.CompleteStop(true);
                Throws<InvalidOperationException>(() => ProjectConfiguration.CommitFileChanges(new ConfigurationFile[0]));
                Throws<InvalidOperationException>(() => FlowPrograms.RequireIdle()); return 73;
            }
            File.WriteAllText("one.txt", "old-one"); File.WriteAllText("two.txt", "old-two");
            typeof(ConfigurationJournal).GetField("Checkpoint", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, (Action<string>)(point => { if (point == stage) Environment.Exit(73); }));
            ProjectConfiguration.CommitFileChanges(new[] { new ConfigurationFile("one.txt", path => File.WriteAllText(path, "new-one")), new ConfigurationFile("two.txt", path => File.WriteAllText(path, "new-two")) });
            return 2;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }

    /// <summary>用主保存的真实发布回调制造异常，确认磁盘提交后拒绝再次运行或保存。</summary>
    private static int PublishChild()
    {
        SysPara.SettingDataDirectory = Path.Combine(root, "Settings"); ModuleManager.SettingDataDirectory = SysPara.SettingDataDirectory;
        SysPara.RecipeDataDirectory = root; SysPara.FilePath = Path.Combine(root, "Model.xml"); SysPara.UserPermission = PermissionType.Administrator; SysPara.SystemMode = RunMode.IDLE;
        FlowSourceGenerator.SourceProjectPath = ""; FlowSourceGenerator.OutputDirectory = Path.Combine(root, "Generated");
        ApplicationHost.Modules.Register(new FailingPublication(), new ModuleContext(SysPara.SettingDataDirectory));
        Throws<InvalidOperationException>(() => ProjectConfiguration.Save());
        Check(ProjectConfiguration.RecoveryRequired && SysPara.SystemMode == RunMode.STOPPING && File.ReadAllText("published.txt") == "new", "Committed publication failure was not fenced.");
        SysPara.SystemMode = RunMode.IDLE;
        Throws<InvalidOperationException>(() => FlowPrograms.RequireIdle());
        Throws<InvalidOperationException>(() => ProjectConfiguration.Save());
        Throws<InvalidOperationException>(() => ProjectConfiguration.CommitFileChanges(new ConfigurationFile[0]));
        File.WriteAllText("verified.txt", "fenced"); return 73;
    }

    private sealed class FailingPublication : IFrameworkModule, IConfigurationParticipant
    {
        public string Id { get { return "fixture"; } }
        public string ConfigurationId { get { return "fixture"; } }
        public int ConfigurationOrder { get { return 90; } }
        /// <summary>测试模块没有设备初始化。</summary>
        public void Initialize(ModuleContext context) { }
        /// <summary>测试草稿始终合法。</summary>
        public void ValidateConfiguration() { }
        /// <summary>文件提交成功后模拟模型发布失败。</summary>
        public ConfigurationChange PrepareConfigurationSave() { return new ConfigurationChange(new[] { new ConfigurationFile("published.txt", path => File.WriteAllText(path, "new")) }, () => { throw new IOException("publication fixture"); }); }
        /// <summary>测试模块没有恢复草稿。</summary>
        public Action PrepareConfigurationDiscard() { return () => { }; }
        /// <summary>测试模块不持有资源。</summary>
        public void Dispose() { }
    }

    /// <summary>真正退出测试子进程，在下一次读取前恢复完整旧集合或确认完整新集合。</summary>
    private static void CrashRecovery(string assemblies, string stage)
    {
        string directory = Launch(assemblies, "crash", stage); string previous = Environment.CurrentDirectory;
        try
        {
            Environment.CurrentDirectory = directory; ConfigurationJournal.RecoverPending();
            string prefix = stage == "Committed" ? "new" : "old";
            Check(File.ReadAllText("one.txt") == prefix + "-one" && File.ReadAllText("two.txt") == prefix + "-two", "Recovery retained mixed configuration versions.");
            Check(Directory.GetDirectories(ConfigurationJournal.DirectoryPath).Length == 0, "Recovery retained a completed journal.");
        }
        finally { Environment.CurrentDirectory = previous; }
    }

    /// <summary>验证主保存发布失败的记录能够在重启阶段接受已提交版本。</summary>
    private static void Publication(string assemblies)
    {
        string directory = Launch(assemblies, "publication", ""); string previous = Environment.CurrentDirectory;
        try { Environment.CurrentDirectory = directory; Check(File.ReadAllText("verified.txt") == "fenced", "Publication fixture did not verify fencing."); ConfigurationJournal.RecoverPending(); Check(File.ReadAllText("published.txt") == "new", "Recovery discarded committed files."); }
        finally { Environment.CurrentDirectory = previous; }
    }

    /// <summary>恢复遇到第三方修改时保留文件和记录，不猜测覆盖。</summary>
    private static void Conflict(string assemblies)
    {
        string directory = Launch(assemblies, "crash", "File:0"); string previous = Environment.CurrentDirectory;
        try { Environment.CurrentDirectory = directory; File.WriteAllText("one.txt", "external"); Throws<IOException>(() => ConfigurationJournal.RecoverPending()); Check(File.ReadAllText("one.txt") == "external" && Directory.GetDirectories(ConfigurationJournal.DirectoryPath).Length == 1, "Unknown external edit was overwritten."); }
        finally { Environment.CurrentDirectory = previous; }
    }

    /// <summary>启动隐藏子进程，最长等待二十秒且只终止本测试创建的进程。</summary>
    private static string Launch(string assemblies, string mode, string stage)
    {
        string directory = Path.Combine(root, "child" + passed); Directory.CreateDirectory(directory);
        var start = new ProcessStartInfo(Assembly.GetExecutingAssembly().Location, Quote(assemblies) + " " + Quote(directory) + " " + Quote(mode) + " " + Quote(stage)) { UseShellExecute = false, CreateNoWindow = true, WorkingDirectory = directory };
        using (Process process = Process.Start(start))
        {
            if (!process.WaitForExit(20000)) { process.Kill(); throw new TimeoutException("Owned regression child did not exit."); }
            Check(process.ExitCode == 73, "Regression child failed: " + process.ExitCode);
        }
        return directory;
    }
    /// <summary>引用受控测试路径，不交给命令解释器执行。</summary>
    private static string Quote(string value) { return "\"" + value.TrimEnd('\\') + "\""; }
    /// <summary>输出各项业务验证结果。</summary>
    private static void Test(string name, Action test) { test(); passed++; Console.WriteLine("PASS " + name); }
    /// <summary>失败时保留具体条件。</summary>
    private static void Check(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
    /// <summary>要求明确拒绝无效操作。</summary>
    private static void Throws<T>(Action action) where T : Exception
    {
        try { action(); } catch (T) { return; }
        throw new InvalidOperationException("Expected " + typeof(T).Name);
    }
}
