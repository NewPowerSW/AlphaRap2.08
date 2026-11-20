using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;
using AlphaRap.Security;
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

    private sealed class OfflineVision : VPForm
    {
        /// <summary>保留内存视觉配置，跳过本机相机枚举。</summary>
        public override void ModuleInitialize(string moduleName) { }
    }

    /// <summary>先解析框架依赖，再运行独立配置故障注入。</summary>
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
            Test("hardware lookup and display edits reuse the registered device", HardwareIdentity);
            Test("device names are unique across panels and legacy keys still resolve", NamedDevices);
            Test("device tabs and panels have distinct control names", NamedDeviceControls);
            Test("VPP names resolve within the selected camera", NamedVision);
            Test("named constants read committed values in the selected scope", NamedConstants);
            Test("named customer logs use the saved schema and isolate simulated records", NamedLogs);
            Test("passwords remain compatible with existing managed hashes", LoginHashCompatibility);
            Test("login shows progress and waits for background permissions", () => LoginWindow(false));
            Test("closing a login window discards late authentication", () => LoginWindow(true));
            Test("monitor borrows installed stations and preserves the original flow file on Save", TransitionMonitor);
            Console.WriteLine("RESULT configuration participants: " + passed + "/22 passed.");
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

    /// <summary>验证动态 IO 的编辑、查找和删除都针对同一个配置身份。</summary>
    private static void HardwareIdentity()
    {
        using (var manual = new DefaultManualPage())
        using (var hardware = ApplicationHost.RegisterModule(new HardForm()))
        {
            var resources = hardware.Resources;
            manual.BindResources(resources);
            var definition = resources.NewDefinition("Input");
            definition.Id = "abcdef0123456789abcdef0123456789";
            var original = resources.Put(definition);
            string key = "hardware/" + definition.Id + "/input";
            var duplicate = definition.Copy();
            duplicate.Id = definition.Id.ToUpperInvariant();
            duplicate.Name = "Different input";
            Throws<InvalidOperationException>(() => resources.Put(duplicate));
            Check(ReferenceEquals(original.Device, ApplicationHost.GetResource<NPSDK.Motion.NP_Input>(key)), "Lookup returned a different input.");
            Check(ReferenceEquals(original.Device, ApplicationHost.GetInput(definition.Name)), "Named input differs from ID lookup.");
            string previousName = definition.Name;
            definition.Name = "Board present";
            var renamed = resources.Put(definition);
            Check(ReferenceEquals(renamed.Device, original.Device) && !original.Group.IsDisposed, "Renaming replaced a live control.");
            Check(renamed.Device.Text == definition.Name, "The control caption did not follow its definition.");
            Check(ReferenceEquals(original.Device, ApplicationHost.GetInput("board present")), "Named lookup missed an edited caption.");
            Throws<KeyNotFoundException>(() => ApplicationHost.GetInput(previousName));

            var axis = resources.Put(resources.NewDefinition("Axis"));
            Check(ReferenceEquals(axis.Device, ApplicationHost.GetAxis(axis.Definition.Name)), "Named axis differs from ID lookup.");
            var pointTable = ((NPSDK.Motion.NP_AxisGroup)axis.Group).Points.DataSet1;
            pointTable.Rows.Add(12.5, "P1");
            pointTable.AcceptChanges();
            pointTable.Rows[0]["Position"] = 15.0;
            Check(ApplicationHost.GetAxisPoints(axis.Definition.Name).Positions[0] == 12.5, "Axis points read an uncommitted value.");
            var output = resources.Put(resources.NewDefinition("Output"));
            Check(ReferenceEquals(output.Device, ApplicationHost.GetOutput(output.Definition.Name)), "Named output differs from ID lookup.");
            resources.MoveIoToPage(definition.Id, resources.AddIoPage("Station IO").Id);
            manual.BindResources(resources);
            Check(original.Group.FindForm() == manual && ReferenceEquals(original.Device, ApplicationHost.GetResource<object>(key)), "Moving the IO lost its manual view or identity.");
            Throws<InvalidCastException>(() => ApplicationHost.GetResource<NPSDK.Motion.NP_Output>(key));
            var saved = new NPSDK.FlowProject();
            var station = new NPSDK.FlowProgram { ModuleId = typeof(NPSDK.Flow_BaseForm).FullName + "/Consumer" };
            station.Steps.Add(new NPSDK.FlowStep { Id = "read", ExecutionMode = "Action", Target = key });
            saved.Modules.Add(station);
            NPSDK.FlowProgramFile.Save(FlowPrograms.FilePath, saved);
            Throws<InvalidOperationException>(() => resources.Delete(definition.Id));
            Check(!FlowPrograms.HasPrograms && ReferenceEquals(original.Device, ApplicationHost.GetResource<object>(key)), "Reference check changed the station or device.");
            File.WriteAllText(FlowPrograms.FilePath, "<broken");
            Throws<InvalidDataException>(() => resources.Delete(definition.Id));
            Check(!original.Group.IsDisposed, "Invalid flow data allowed deletion.");
            File.Delete(FlowPrograms.FilePath);
            NPSDK.FlowProgramFile.Save(FlowPrograms.FilePath, new NPSDK.FlowProject());
            resources.Delete(definition.Id);
            Check(original.Group.IsDisposed, "Deleted control still exists.");
            Throws<KeyNotFoundException>(() => ApplicationHost.GetResource<object>(key));
        }
    }

    /// <summary>验证新建设备跨面板唯一，旧配置同名设备仍可按面板定位。</summary>
    private static void NamedDevices()
    {
        using (var form = new System.Windows.Forms.Form { Name = "NamedDevices" })
        using (var first = new DeviceControl { Name = "first" })
        using (var second = new DeviceControl { Name = "second" })
        {
            form.Controls.Add(first);
            form.Controls.Add(second);
            string typeName = HardwareControlTypes.Identity(typeof(ScannerKeyenceTcp));
            string name = "Scanner / A";
            Check(first.BindDevice(typeName, name) == "", "First device could not be loaded.");
            Check(ReferenceEquals(first.Device, ApplicationHost.GetDevice<ScannerKeyenceTcp>(name)), "Named device differs from registered instance.");
            Check(!string.IsNullOrEmpty(second.BindDevice(typeName, name.ToUpperInvariant())), "A new cross-panel duplicate was accepted.");
            Check(first.BindDevice(typeName, null) == "" && first.DeviceName == "Device1", "First automatic device name was incorrect.");
            Check(second.BindDevice(typeName, null) == "" && second.DeviceName == "Device2", "Automatic names were reused across panels.");
            var box = (System.Windows.Forms.TextBox)typeof(DeviceControl).GetField("txtName", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(second);
            box.Text = "device1";
            typeof(DeviceControl).GetMethod("txtName_Leave", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(second, new object[] { box, EventArgs.Empty });
            Check(second.DeviceName == "Device2" && box.Text == "Device2", "Device rename accepted a name owned by another panel.");
            Directory.CreateDirectory(SysPara.SettingDataDirectory);
            var legacy = new System.Xml.Linq.XElement("DeviceControl",
                new System.Xml.Linq.XElement("Device", new System.Xml.Linq.XAttribute("Key", "NamedDevices_second"),
                    new System.Xml.Linq.XElement("Item", new System.Xml.Linq.XAttribute("TypeId", typeName),
                        new System.Xml.Linq.XAttribute("Name", name))));
            legacy.Save(DeviceControl.XmlFilePath);
            second.LoadFromXml();
            Throws<InvalidOperationException>(() => ApplicationHost.GetDevice<ScannerKeyenceTcp>(name));
            Check(ReferenceEquals(second.Device, ApplicationHost.GetDevice<ScannerKeyenceTcp>(name, "NamedDevices_second")), "Panel scope did not resolve the device.");
            Throws<KeyNotFoundException>(() => ApplicationHost.GetDevice<ScannerKeyenceTcp>("Missing"));
        }
    }

    /// <summary>验证设备页签、面板的独立名称及旧面板键。</summary>
    private static void NamedDeviceControls()
    {
        using (var page = new Parameter())
        {
            var create = typeof(Parameter).GetMethod("NewPanel", BindingFlags.Instance | BindingFlags.NonPublic);
            var first = (DeviceControl)create.Invoke(page, new object[] { "dc1" });
            var second = (DeviceControl)create.Invoke(page, new object[] { "dc2" });
            string[] names = { first.Name, first.Parent.Name, second.Name, second.Parent.Name };
            Check(names.Distinct(StringComparer.OrdinalIgnoreCase).Count() == names.Length, "Device tabs and panels share a control name.");
            Check(first.Name == "dc1" && second.Name == "dc2", "Saved device panel identities were changed.");
        }
    }

    /// <summary>用两台内存相机验证相同 VPP 名称按相机范围查找。</summary>
    private static void NamedVision()
    {
        var first = new VpStation("Camera A", "VPP1");
        var second = new VpStation("Camera B", "VPP1");
        try
        {
            using (var page = ApplicationHost.RegisterModule(new OfflineVision()))
            {
                var cameraA = new VpCameraConfig { Name = "Camera A" };
                var cameraB = new VpCameraConfig { Name = "Camera B" };
                cameraA.Vpps.Add(new VpVppConfig { Name = "VPP1", Station = first });
                cameraB.Vpps.Add(new VpVppConfig { Name = "VPP1", Station = second });
                var config = (VpConfigFile)typeof(VPForm).GetField("_config", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(page);
                config.Cameras.Add(cameraA);
                config.Cameras.Add(cameraB);
                Check(ReferenceEquals(cameraA, ApplicationHost.GetCamera("Camera A")), "Camera lookup returned a different configuration.");
                Check(ReferenceEquals(first, ApplicationHost.GetVpp("Camera A", "VPP1")), "VPP from the first camera was not found.");
                Check(ReferenceEquals(second, ApplicationHost.GetVpp("Camera B", "VPP1")), "Duplicate VPP name resolved to the wrong camera.");
                Check(ApplicationHost.GetVpp("Camera A", "Unknown") == null, "Missing model returned a VPP from another camera.");
                Check(ReferenceEquals(first, ApplicationHost.GetVpp("camera a", "vpp1")), "VPP lookup did not follow the case-insensitive name rule.");

                cameraA.Vpps[0].Id = "abcdef0123456789abcdef0123456789";
                cameraB.Vpps[0].Id = cameraA.Vpps[0].Id.ToUpperInvariant();
                Throws<InvalidOperationException>(() => config.ValidateStationIds());
                cameraB.Vpps[0].Id = Guid.NewGuid().ToString("N");
                config.ValidateStationIds();
                var buildPage = typeof(VPForm).GetMethod("BuildVppPage", BindingFlags.Instance | BindingFlags.NonPublic);
                cameraA.Name = "Camera_A";
                cameraA.Vpps[0].Name = "Look";
                cameraB.Name = "Camera";
                cameraB.Vpps[0].Name = "A_Look";
                using (var tabs = new System.Windows.Forms.TabControl())
                using (var a = (System.Windows.Forms.TabPage)buildPage.Invoke(page, new object[] { cameraA, cameraA.Vpps[0], tabs }))
                using (var b = (System.Windows.Forms.TabPage)buildPage.Invoke(page, new object[] { cameraB, cameraB.Vpps[0], tabs }))
                {
                    Check(!string.Equals(a.Name, b.Name, StringComparison.OrdinalIgnoreCase), "Camera/VPP pairs produced the same tab name.");
                    Check(!string.Equals(a.Controls[0].Name, b.Controls[0].Name, StringComparison.OrdinalIgnoreCase), "VPP template instances have duplicate names.");
                }
            }
        }
        finally
        {
            VisionproInterface.VList.Remove(first);
            VisionproInterface.VList.Remove(second);
        }
    }

    /// <summary>验证同键在机器和配方中的隔离，以及主保存前后的取值。</summary>
    private static void NamedConstants()
    {
        var context = new ModuleContext(SysPara.SettingDataDirectory);
        var module = ApplicationHost.Modules.Register(new SystemParametersModule(new BooleanSettingDefault[0]), context);
        module.Constants.Put("Setting", null, new ConstantValue { Key = "CheckEnabled", Type = "Boolean", Value = "True" });
        module.Constants.Put("Recipe", null, new ConstantValue { Key = "CheckEnabled", Type = "Boolean", Value = "False" });
        Throws<KeyNotFoundException>(() => ApplicationHost.GetConstant<bool>("Setting", "CheckEnabled"));
        ProjectConfiguration.Save();
        Check(ApplicationHost.GetConstant<bool>("Setting", "CheckEnabled"), "Machine value was not read.");
        Check(!ApplicationHost.GetConstant<bool>("Recipe", "CheckEnabled"), "Recipe value inherited the machine value.");
        Throws<InvalidCastException>(() => ApplicationHost.GetConstant<int>("Recipe", "CheckEnabled"));
        module.Constants.Put("Recipe", "CheckEnabled", new ConstantValue { Key = "CheckEnabled", Type = "Boolean", Value = "True" });
        Check(!ApplicationHost.GetConstant<bool>("Recipe", "CheckEnabled"), "Recipe draft leaked into runtime lookup.");
    }

    /// <summary>按日志名称写真实格式的模拟记录，验证草稿名称和缺失字段均被拒绝。</summary>
    private static void NamedLogs()
    {
        Directory.CreateDirectory(SysPara.SettingDataDirectory);
        var definition = new LogModuleDefinition { Name = "Customer record", Directory = Path.Combine(current, "Customer") };
        definition.Fields.Add(new ProductionField { Name = "Barcode" });
        string file = Path.Combine(SysPara.SettingDataDirectory, "DataForm.LogModules.xml");
        using (var output = File.Create(file))
            new System.Xml.Serialization.XmlSerializer(typeof(List<LogModuleDefinition>)).Serialize(output, new List<LogModuleDefinition> { definition });
        bool previousSimulation = SysPara.Simulation;
        SysPara.Simulation = true;
        try
        {
            using (var data = new DataForm())
            {
                var values = new Dictionary<string, object> { { "Barcode", "SAMPLE-01" } };
                Throws<InvalidDataException>(() => data.WriteLogByName(definition.Name, new Dictionary<string, object>()));
                string path = data.WriteLogByName("customer record", values);
                Check(path.StartsWith(Path.Combine(definition.Directory, "Simulation") + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                    && File.ReadAllText(path).Contains("SAMPLE-01"), "Named log missed the configured schema or simulation folder.");
                var draft = data.GetLogModules(false);
                draft[0].Name = "Unsaved rename";
                data.SetLogModulesDraft(draft);
                Throws<KeyNotFoundException>(() => data.WriteLogByName("Unsaved rename", values));
                Check(File.Exists(data.WriteLogByName(definition.Name, values)), "Draft changed access to the published module.");
            }
        }
        finally { SysPara.Simulation = previousSimulation; }
    }

    /// <summary>验证系统加密库与现有 Windows .NET 凭据的二进制结果一致。</summary>
    private static void LoginHashCompatibility()
    {
        byte[] salt = Enumerable.Range(0, 16).Select(value => (byte)value).ToArray();
        const int cost = 600000;
        foreach (string password in new[] { "", "账号Pruebañ🔐" })
        {
            byte[] key;
            using (var managed = new Rfc2898DeriveBytes(password, salt, cost, HashAlgorithmName.SHA256)) key = managed.GetBytes(32);
            string encoded = "np1$" + cost + "$" + Convert.ToBase64String(salt) + "$" + Convert.ToBase64String(key);
            Check(PasswordCredentials.Verify(password, encoded), "Existing managed credential was rejected.");
            Check(!PasswordCredentials.Verify(password + "wrong", encoded), "Incorrect password was accepted.");
            string[] fields = PasswordCredentials.Hash(password).Split('$');
            Check(fields[0] == "np1" && fields[1] == "600000", "Stored credential format or cost changed.");
            using (var managed = new Rfc2898DeriveBytes(password, Convert.FromBase64String(fields[2]), cost, HashAlgorithmName.SHA256))
                Check(managed.GetBytes(32).SequenceEqual(Convert.FromBase64String(fields[3])), "Generated credential could not be verified by the former implementation.");
        }
    }

    /// <summary>在独立目录创建 MDB，使用测试账号验证真实登录入口。</summary>
    private static string CreateLoginDatabase()
    {
        string path = Path.Combine(current, "LoginFixture.mdb");
        string connectionString = new OleDbConnectionStringBuilder { Provider = "Microsoft.ACE.OLEDB.12.0", DataSource = path }.ConnectionString;
        Type catalogType = Type.GetTypeFromProgID("ADOX.Catalog", true);
        object catalog = Activator.CreateInstance(catalogType);
        try
        {
            catalogType.InvokeMember("Create", BindingFlags.InvokeMethod, null, catalog, new object[] { connectionString });
            object connection = catalogType.InvokeMember("ActiveConnection", BindingFlags.GetProperty, null, catalog, null);
            try { connection.GetType().InvokeMember("Close", BindingFlags.InvokeMethod, null, connection, null); }
            finally { Marshal.FinalReleaseComObject(connection); }
        }
        finally { Marshal.FinalReleaseComObject(catalog); }
        using (var connection = new OleDbConnection(connectionString))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "CREATE TABLE [UserData] ([UserName] TEXT(50),[Password] TEXT(50),[Permission] TEXT(50))";
                command.ExecuteNonQuery();
                command.CommandText = "INSERT INTO [UserData] ([UserName],[Password],[Permission]) VALUES ('first','fixture-1','Operator')";
                command.ExecuteNonQuery();
                command.CommandText = "INSERT INTO [UserData] ([UserName],[Password],[Permission]) VALUES ('tester','fixture-2','Maintenance')";
                command.ExecuteNonQuery();
            }
        }
        return path;
    }

    /// <summary>在菜单权限查询被人为阻塞时观察真实 WinForms 消息处理及关闭行为。</summary>
    private static void LoginWindow(bool closeEarly)
    {
        string originalDatabase = SysPara.MdbPath, originalUser = SysPara.UserName;
        bool originalLogin = SysPara.bLogin;
        PermissionType originalRole = SysPara.UserPermission;
        SysPara.MdbPath = CreateLoginDatabase();
        SysPara.UserName = "None";
        SysPara.bLogin = false;
        SysPara.UserPermission = PermissionType.Operator;
        int uiThread = Thread.CurrentThread.ManagedThreadId, loadedThread = 0, appliedThread = 0, preparations = 0;
        bool observedPending = false, expired = false;
        using (var prepared = new ManualResetEventSlim())
        using (var release = new ManualResetEventSlim())
        using (var login = new UserLoginForm(permission =>
        {
            Interlocked.Increment(ref preparations);
            loadedThread = Thread.CurrentThread.ManagedThreadId;
            prepared.Set();
            if (!release.Wait(6000)) throw new TimeoutException("Permission test was not released.");
            return () => appliedThread = Thread.CurrentThread.ManagedThreadId;
        }))
        using (var timer = new System.Windows.Forms.Timer { Interval = 15 })
        {
            var name = (FieldBox)login.Controls.Find("textUserName", true).Single();
            var password = (FieldBox)login.Controls.Find("textPassword", true).Single();
            var button = (FlatButton)login.Controls.Find("btnLogin", true).Single();
            var grid = (DataGridView)login.Controls.Find("dgvUserList", true).Single();
            string caption = button.Text;
            var timeout = Stopwatch.StartNew();
            try
            {
                login.Shown += (sender, args) =>
                {
                    name.Value = "tester";
                    password.Value = "fixture-2";
                    button.PerformClick();
                    button.PerformClick();
                    timer.Start();
                };
                timer.Tick += (sender, args) =>
                {
                    if (timeout.ElapsedMilliseconds > 7000)
                    {
                        expired = true;
                        release.Set();
                        login.Close();
                        return;
                    }
                    if (!prepared.IsSet || grid.Rows.Count < 2 || observedPending) return;
                    observedPending = !button.Enabled && button.Text != caption && name.Value == "tester"
                        && SysPara.UserName == "None" && SysPara.UserPermission == PermissionType.Operator;
                    if (closeEarly) login.Close();
                    release.Set();
                };
                login.ShowDialog();
                timer.Stop();
                ApplicationTasks.Global.StopAllAsync().GetAwaiter().GetResult();
                Application.DoEvents();
                Check(!expired && observedPending && preparations == 1 && loadedThread != uiThread,
                    "The login window did not process messages, lost input, or submitted twice.");
                if (closeEarly)
                    Check(appliedThread == 0 && SysPara.UserName == "None" && SysPara.UserPermission == PermissionType.Operator, "A closed dialog granted permissions.");
                else
                    Check(appliedThread == uiThread && SysPara.UserName == "tester" && SysPara.UserPermission == PermissionType.Maintenance,
                        "Login failed to apply verified permissions on the message thread.");
            }
            finally
            {
                release.Set();
                SysPara.MdbPath = originalDatabase;
                SysPara.UserName = originalUser;
                SysPara.bLogin = originalLogin;
                SysPara.UserPermission = originalRole;
            }
        }
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
            monitor.Show();
            var tabs = (System.Windows.Forms.TabControl)monitor.Controls["tabControl1"];
            Check(tabs.TabCount == 0 && !FlowPrograms.HasPrograms, "Opening the monitor installed a station.");
            FlowPrograms.RequireHardwareUnused(Guid.NewGuid().ToString("N"), "UnusedPoints");
            FlowPrograms.RequireParameterUnused("UnusedParameter");
            FlowPrograms.RequireResourceUnused("UnusedResource");
            Check(!FlowPrograms.HasPrograms && NPSDK.Flow_Module.FlowChart_ModuleList.Count == 0, "Checking references created a station.");
            monitor.Hide();
            FlowPrograms.Install();
            monitor.Show();
            var tab = tabs.TabPages[0];
            monitor.AddFlowChart();
            Check(tabs.TabCount == 1 && tabs.TabPages[0] == tab, "Refreshing replaced a station tab.");
            Check(tabs.TabPages.Cast<System.Windows.Forms.TabPage>().Any(page => page.Text == "HistoricalStation"), "Historical station missing.");
            Check(monitor.Controls.Count == 2 && !tabs.AllowDrop && monitor.Controls.Find("toolbox", true).Length == 0, "Flow monitor exposed an editing surface.");
            Check(typeof(Flow).Assembly.GetType("AlphaRap.FlowProgramming.FlowToolboxControl") == null, "Drag toolbox is present in this assembly.");
            Check(typeof(NPSDK.Flow_Chart).GetMethod("EnableProgramEditor") == null, "SDK node drag handlers are available.");
            ProjectConfiguration.Save();
            ProjectConfiguration.Discard();
            Check(File.ReadAllBytes(FlowPrograms.FilePath).SequenceEqual(before), "Main Save or Discard replaced the original flow.");
        }
        var station = FlowPrograms.Modules.Single().Form;
        try
        {
            Check(!station.IsDisposed && station.Parent == null, "Closing the monitor destroyed its station.");
            using (var reopened = new Flow())
            {
                reopened.Show();
                var tabs = (System.Windows.Forms.TabControl)reopened.Controls["tabControl1"];
                Check(tabs.TabCount == 1 && tabs.TabPages[0].Controls.Contains(station), "Reopening lost the station.");
            }
        }
        finally { station.Dispose(); }
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
