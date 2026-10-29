using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using AlphaRap;
using Cognex.VisionPro.ToolBlock;
using Cognex.VisionPro.CalibFix;

/// <summary>离线视觉回归测试入口，加载项目构建产物及本机 Cognex 运行库。</summary>
internal static class Program
{
    /// <summary>注册依赖查找路径，执行不使用相机与 PLC 的测试。</summary>
    [STAThread]
    private static int Main(string[] args)
    {
        AppDomain.CurrentDomain.AssemblyResolve += delegate(object sender, ResolveEventArgs request)
        {
            string name = new AssemblyName(request.Name).Name + ".dll";
            foreach (string directory in args)
            {
                string path = Path.Combine(directory, name);
                if (File.Exists(path)) return Assembly.LoadFrom(path);
                path = Path.ChangeExtension(path, ".exe");
                if (File.Exists(path)) return Assembly.LoadFrom(path);
            }
            return null;
        };
        return Tests.RunAll();
    }
}

/// <summary>通过受控算法替身测试真实 VpStation 的运行、等待与文件操作。</summary>
internal sealed class ControlledStation : VpStation
{
    public readonly ManualResetEvent Entered = new ManualResetEvent(false);
    public readonly ManualResetEvent Release = new ManualResetEvent(false);
    public bool Accept = true;
    public bool ThrowError;

    /// <summary>创建内存 ToolBlock，不打开采集驱动或显示控件。</summary>
    public ControlledStation() : base("Regression", VpStationKind.Fiducial)
    {
        TB = new CogToolBlock();
        IsLoadTBOk = true;
        AutoVisionRunDisplay = false;
    }

    /// <summary>等待测试放行，模拟成功、NG、异常及迟到结果。</summary>
    public override void VisionRun()
    {
        Entered.Set();
        if (!Release.WaitOne(5000)) throw new TimeoutException("Test did not release the worker.");
        if (ThrowError) throw new InvalidOperationException("Simulated algorithm failure.");
        Fiducial.x = 2;
        Fiducial.y = 3;
        Fiducial.u = 4;
        IsAccept = Accept;
    }
}

/// <summary>覆盖运行身份、超时隔离、文件失败和输入判据的回归场景。</summary>
internal static partial class Tests
{
    private static int _failed;
    private static int _passed;
    private static readonly string TestDirectory = Path.Combine(Path.GetTempPath(), "AlphaRap-Vision-" + Guid.NewGuid().ToString("N"));
    private static readonly Type LanguageHost = typeof(VPForm).Assembly.GetType("AlphaRap.MiddleLayer");

    /// <summary>执行各独立场景，失败时返回非零退出码。</summary>
    public static int RunAll()
    {
        Directory.CreateDirectory(TestDirectory);
        SysPara.LanguageDataDirectory = Path.Combine(TestDirectory, "language");
        LanguageHost.GetField("_langDocs", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, null);
        Run("successful run captures immutable coordinates", Success);
        Run("capture SN and format remain fixed through completion and cancellation", ProductCaptureIdentity);
        Run("duplicate run and mutation while busy are rejected", Busy);
        Run("concurrent callers start exactly one worker", ConcurrentStarts);
        Run("timed out worker cannot publish late success", Timeout);
        Run("cancelled worker cannot publish late success", Cancel);
        Run("RunAndWait timeout retains busy ownership", WaitTimeout);
        Run("algorithm exception has explicit fault state", Fault);
        Run("algorithm NG has explicit rejected state", Rejected);
        Run("missing VPP invalidates old model without creating a file", MissingFile);
        Run("corrupt VPP invalidates old model", CorruptFile);
        Run("save is synchronous and keeps the current object", Save);
        Run("failed save preserves previous file and path", SaveFailure);
        Run("discarding an unsaved VPP clears the draft", DiscardDraft);
        Run("failed discard keeps the editor model available", DiscardFailure);
        Run("numeric input rejects NaN and infinity", FiniteInput);
        Run("compensation limits reject rather than clamp", Compensation);
        Run("calibration save failure restores the original model", CalibrationRollback);
        Run("recipe criterion reads column names and validates ranges", Criteria);
        Run("production resolves a stable VPP identity", ProductionBinding);
        Run("missing explicit binding never falls back to legacy", MissingBinding);
        Run("production rejects a different recipe and invalid outputs", ProductionContract);
        Run("production requires every compensation limit", ProductionLimits);
        Run("unbound configuration preserves legacy compatibility", LegacyBinding);
        Run("calibration context follows recipe camera and model versions", CalibrationContext);
        Run("configuration persists IDs binding and calibration records", ConfigRoundTrip);
        Run("invalid duplicate IDs cannot overwrite a valid config", DuplicateIds);
        Run("legacy configuration migration does not invent calibration success", LegacyConfig);
        Run("renaming preserves model paths and production binding", RenameStorage);
        Run("storage folders cannot collide or escape the vision root", StorageValidation);
        Run("VPP page displays and persists the production selection", ProductionPage);
        Run("designer contains the fixed production checkbox", DesignerProductionControl);
        Run("three-language refresh preserves edited VPP data", VppLanguageRefresh);
        Run("compensation table is an independent configuration snapshot", CompensationTable);
        Run("new R limits validate positioning and VPP content aligns below its header", VppDefaultsAndLayout);
        Run("compensation values persist and legacy values default to zero", CompensationConfig);
        Run("compensation editor stages finite input until main save without rounding other fields", CompensationEditor);
        Run("localized angle editing preserves the business key across languages", CompensationItemEditing);
        Run("configured compensation is not automatically applied to vision", CompensationNotApplied);
        Run("site translations take priority over default text", SiteTranslation);
        Run("user camera and VPP names are not registered as captions", UserNamedTabs);
        Run("disposed vision forms release their language subscription", LanguageSubscription);
        Run("language editor saves sorted asymmetric catalogs by stable keys", LanguageEditorSave);
        Run("fixed headers menus and custom controls switch without changing data", FixedLanguageParts);
        Run("a failing language subscriber does not block other pages", LanguageEventIsolation);
        Run("all application pages register three-language designer captions", AllPageLanguages);
        Run("language switching preserves pending tray edits and row instances", TrayLanguageEdits);
        Run("alarm editor validation is independent of translated headers", AlarmFieldValidation);
        Run("alarm lookup uses configured Spanish file and reloads saved text", AlarmLanguageLookup);
        Run("alarm language changes cannot change stop rules", AlarmRulesStayFixed);
        Run("concurrent message registration preserves every translation key", ConcurrentLanguageMessages);
        Run("language save failure leaves all original catalogs intact", LanguageSaveFailure);
        Run("permission translations preserve enum values and selected items", PermissionLanguageDisplay);
        Run("language settings render in Chinese English and Spanish", LanguageSettingsRenders);
        Run("saving same-language translations refreshes the main frame cache", MainFrameLanguageReload);
        Run("partial site catalogs use bundled translations without overwriting overrides", PartialSiteCatalog);
        Run("form factory reuses instances without duplicate registration", FormFactoryReuse);
        Run("machine initialization resets stale state and signals each flow once", MachineInitialTransition);
        Run("machine stop signals flows once and cancels manual tasks", MachineStopTransition);
        Run("machine pause retains resume state and cancels manual tasks", MachinePauseTransition);
        Run("periodic callback failure is isolated from the next module", ModulePeriodicIsolation);
        Run("reset failure stops production and retains module isolation", ModuleResetFailureStops);
        Run("indexed language lookup matches every bundled translation", LanguageIndexLookup);
        Run("scan shutdown waits for completion and avoids joining itself", ScanThreadShutdown);
        Console.WriteLine("RESULT: " + _passed + "/" + (_passed + _failed) + " passed");
        return _failed == 0 ? 0 : 1;
    }

    /// <summary>记录独立测试结果，失败不阻止其余场景运行。</summary>
    private static void Run(string name, Action test)
    {
        try { test(); _passed++; Console.WriteLine("PASS " + name); }
        catch (Exception ex) { _failed++; Console.WriteLine("FAIL " + name + ": " + ex); }
    }

    /// <summary>断言业务条件，不满足时使当前场景失败。</summary>
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    /// <summary>等待算法线程真正退出，避免把超时判定当作底层停止。</summary>
    private static void WaitIdle(ControlledStation station)
    {
        Check(SpinWait.SpinUntil(() => !station.IsBusy, 3000), "Worker did not exit.");
    }

    /// <summary>启动受控运行并等待进入算法内部。</summary>
    private static long Start(ControlledStation station)
    {
        long id;
        Check(station.TryRunTB(out id), "Run was rejected.");
        Check(station.Entered.WaitOne(2000), "Worker did not start.");
        return id;
    }

    /// <summary>成功结果与原公共坐标随后发生的修改相互独立。</summary>
    private static void Success()
    {
        ControlledStation station = new ControlledStation();
        station.Release.Set();
        Check(station.RunAndWait(2000), "Run should succeed.");
        VisionRunResult result = station.LastRunResult;
        station.Fiducial.x = 99;
        Check(result.X == 2 && result.Y == 3 && result.Angle == 4, "Snapshot changed.");
    }

    /// <summary>拍照时复制 SN 与格式，完成和取消都不能被下一件产品覆盖。</summary>
    private static void ProductCaptureIdentity()
    {
        ControlledStation station = new ControlledStation(); long id;
        string sn = "PRODUCT-A";
        Check(station.TryRunTB(out id, sn, "jpg") && station.Entered.WaitOne(2000), "SN capture did not start.");
        VisionRunResult pending = station.LastRunResult; sn = "PRODUCT-B";
        station.CancelRun(id); station.Release.Set(); WaitIdle(station);
        Check(station.LastRunResult.State == VisionRunState.Cancelled && station.LastRunResult.Capture.ProductSN == "PRODUCT-A" &&
            ReferenceEquals(pending.Capture, station.LastRunResult.Capture), "Cancel replaced the capture identity.");
        Check(station.RunAndWait(2000, sn, "bmp"), "Code capture with SN did not complete.");
        Check(station.LastRunResult.Capture.ProductSN == "PRODUCT-B" && station.LastRunResult.Capture.ImageFormat == "bmp" &&
            pending.Capture.ProductSN == "PRODUCT-A", "Next capture changed an earlier SN or format.");
    }

    /// <summary>运行排队和执行期间均不允许第二次触发、加载、保存或标定。</summary>
    private static void Busy()
    {
        ControlledStation station = new ControlledStation();
        long id = Start(station);
        try
        {
            long refused;
            string error;
            Check(!station.TryRunTB(out refused) && refused == 0, "Duplicate accepted.");
            Check(!station.TryLoadTB(Path.Combine(TestDirectory, "busy.vpp"), out error), "Load accepted.");
            Check(!station.TrySaveTB(out error), "Save accepted.");
            CalibApplyInfo info;
            Check(!station.SetCalibration(new List<CalibrationData> { new CalibrationData(), new CalibrationData(), new CalibrationData() }, "Calibration", out info), "Calibration accepted.");
            Check(station.IsLoadTBOk && station.LastRunResult.RunId == id, "Rejected request changed active state.");
        }
        finally { station.Release.Set(); WaitIdle(station); }
    }

    /// <summary>超时后保持底层占用，退出后旧成功结果仍不可见，下一次运行获得新编号。</summary>
    private static void Timeout()
    {
        ControlledStation station = new ControlledStation();
        long id = Start(station);
        station.TimeoutRun(id + 1);
        Check(station.LastRunResult.State == VisionRunState.Running, "Unrelated request invalidated this run.");
        station.TimeoutRun(id);
        Check(station.RunTBOk() && station.IsBusy, "Timeout released worker ownership.");
        station.Release.Set();
        WaitIdle(station);
        Check(station.LastRunResult.State == VisionRunState.TimedOut && !station.IsAccept, "Late success was published.");
        Check(station.RunAndWait(2000) && station.LastRunResult.RunId > id, "New run did not receive a new identity.");
    }

    /// <summary>取消只使请求结果失效，不允许迟到任务覆盖取消状态。</summary>
    private static void Cancel()
    {
        ControlledStation station = new ControlledStation();
        station.CancelRun(Start(station));
        station.Release.Set();
        WaitIdle(station);
        Check(station.LastRunResult.State == VisionRunState.Cancelled && !station.IsAccept, "Cancellation lost.");
    }

    /// <summary>同步等待超时时返回失败，底层任务仍占用工位直到退出。</summary>
    private static void WaitTimeout()
    {
        ControlledStation station = new ControlledStation();
        Check(!station.RunAndWait(30), "Wait should time out.");
        Check(station.IsBusy && station.LastRunResult.State == VisionRunState.TimedOut, "Timeout state missing.");
        station.Release.Set();
        WaitIdle(station);
        Check(!station.IsAccept, "Late success escaped timeout.");
    }

    /// <summary>算法异常作为 Faulted 返回并保留错误原因。</summary>
    private static void Fault()
    {
        ControlledStation station = new ControlledStation { ThrowError = true };
        station.Release.Set();
        Check(!station.RunAndWait(2000), "Fault accepted.");
        Check(station.LastRunResult.State == VisionRunState.Faulted && station.LastError.Contains("Simulated"), "Fault lost.");
    }

    /// <summary>算法 NG 与异常状态区分。</summary>
    private static void Rejected()
    {
        ControlledStation station = new ControlledStation { Accept = false };
        station.Release.Set();
        Check(!station.RunAndWait(2000) && station.LastRunResult.State == VisionRunState.Rejected, "NG accepted.");
    }

    /// <summary>缺失文件时不自动创建，不保留此前已接受的模型与结果。</summary>
    private static void MissingFile()
    {
        ControlledStation station = new ControlledStation();
        station.Release.Set();
        Check(station.RunAndWait(2000), "Setup failed.");
        string path = Path.Combine(TestDirectory, "missing.vpp");
        string error;
        Check(!station.TryLoadTB(path, out error), "Missing file accepted.");
        Check(station.TB == null && !station.IsLoadTBOk && !station.IsAccept && station.LastRunResult == null, "Old model survived.");
        Check(!File.Exists(path), "Missing file was created.");
    }

    /// <summary>损坏文件使工位不可运行，不能沿用旧算法。</summary>
    private static void CorruptFile()
    {
        ControlledStation station = new ControlledStation();
        string path = Path.Combine(TestDirectory, "corrupt.vpp");
        File.WriteAllText(path, "not a VPP");
        string error;
        Check(!station.TryLoadTB(path, out error), "Corrupt file accepted.");
        Check(station.TB == null && !station.IsLoadTBOk, "Old model survived corrupt load.");
    }

    /// <summary>保存返回时磁盘文件已存在，算法引用保持不变，已有文件也可替换。</summary>
    private static void Save()
    {
        ControlledStation station = new ControlledStation();
        CogToolBlock original = station.TB;
        string path = Path.Combine(TestDirectory, "saved.vpp");
        string error;
        Check(station.TrySaveTB(path, out error), error);
        Check(File.Exists(path) && ReferenceEquals(station.TB, original), "Save was asynchronous or replaced model.");
        Check(station.TrySaveTB(out error), error);
        Check(station.TryLoadTB(path, out error) && station.IsLoadTBOk, error);
    }

    /// <summary>不可写目标的失败不改变旧路径或破坏此前文件。</summary>
    private static void SaveFailure()
    {
        ControlledStation station = new ControlledStation();
        string path = Path.Combine(TestDirectory, "preserved.vpp");
        string error;
        Check(station.TrySaveTB(path, out error), error);
        byte[] before = File.ReadAllBytes(path);
        string blocker = Path.Combine(TestDirectory, "file-not-directory");
        File.WriteAllText(blocker, "block");
        Check(!station.TrySaveTB(Path.Combine(blocker, "failed.vpp"), out error), "Invalid destination accepted.");
        Check(station.TBPath == path && Convert.ToBase64String(before) == Convert.ToBase64String(File.ReadAllBytes(path)), "Original changed.");
    }

    /// <summary>多个线程同时触发时，包括尚未进入执行的排队阶段，只能接受一个请求。</summary>
    private static void ConcurrentStarts()
    {
        ControlledStation station = new ControlledStation();
        int accepted = 0;
        try
        {
            System.Threading.Tasks.Parallel.For(0, 32, delegate(int index)
            {
                long id;
                if (station.TryRunTB(out id)) Interlocked.Increment(ref accepted);
            });
            Check(accepted == 1, "Concurrent requests started " + accepted + " workers.");
        }
        finally { station.Release.Set(); WaitIdle(station); }
    }

    /// <summary>新建算法与此前对象无关，放弃未保存的算法不产生文件也不保留可运行对象。</summary>
    private static void DiscardDraft()
    {
        ControlledStation station = new ControlledStation();
        CogToolBlock original = station.TB;
        string path = Path.Combine(TestDirectory, "unsaved-draft.vpp");
        string error;
        Check(station.TryCreateTB(path, out error), error);
        Check(!ReferenceEquals(original, station.TB) && !File.Exists(path), "Draft reused old algorithm or wrote a file.");
        Check(station.TryDiscardTBChanges(out error), error);
        Check(station.TB == null && !station.IsLoadTBOk && !File.Exists(path), "Draft survived discard.");
    }

    /// <summary>放弃编辑时遇到损坏文件，编辑内容保持可保存，避免窗口陷入无法关闭的状态。</summary>
    private static void DiscardFailure()
    {
        ControlledStation station = new ControlledStation();
        CogToolBlock edited = station.TB;
        string path = Path.Combine(TestDirectory, "corrupt-discard.vpp");
        File.WriteAllText(path, "not a VPP");
        station.TBPath = path;
        string error;
        Check(!station.TryDiscardTBChanges(out error), "Corrupt file accepted.");
        Check(station.IsLoadTBOk && ReferenceEquals(edited, station.TB), "Editor content was lost.");
        Check(station.TrySaveTB(out error), error);
    }

    /// <summary>界面与文件数值解析统一拒绝 NaN 和无穷值。</summary>
    private static void FiniteInput()
    {
        double value;
        Check(!VpConfigStore.TryParseInputDouble("NaN", out value), "NaN accepted.");
        Check(!VpConfigStore.TryParseFileDouble("Infinity", out value), "Infinity accepted.");
        Check(VpConfigStore.TryParseInputDouble("1.25", out value) && value == 1.25, "Finite value rejected.");
    }

    /// <summary>补偿超限必须拒绝，缺失和非有限输出也必须拒绝。</summary>
    private static void Compensation()
    {
        VpStation station = new VpStation("Limits", VpStationKind.Fiducial);
        station.TB = new CogToolBlock();
        station.IsLoadTBOk = true;
        station.AutoVisionRunDisplay = false;
        station.TB.Outputs.Add(new CogToolBlockTerminal("x", typeof(double)));
        station.TB.Outputs.Add(new CogToolBlockTerminal("y", typeof(double)));
        station.TB.Outputs.Add(new CogToolBlockTerminal("A", typeof(double)));
        station.TB.Outputs["x"].Value = 20.0;
        station.TB.Outputs["y"].Value = 0.0;
        station.TB.Outputs["A"].Value = 0.0;
        station.Config = new VpVppConfig();
        station.Config.CompLimits.Add(new VpCompLimitItem("X", -1, 1));
        Check(!station.RunAndWait(2000), "Excessive compensation accepted.");
        Check(station.Fiducial.x == 20 && !station.IsAccept && station.LastError.Contains("X"), "Raw output was clamped or lost.");
        MethodInfo validate = typeof(VpStation).GetMethod("ValidateCompensation", BindingFlags.Instance | BindingFlags.NonPublic);
        validate.Invoke(station, new object[] { "X", 0.5 });
        ExpectFailure(() => validate.Invoke(station, new object[] { "X", 20.0 }));
        station.Config.CompLimits[0].Max = double.NaN;
        ExpectFailure(() => validate.Invoke(station, new object[] { "X", 0.5 }));
    }

    /// <summary>标定在副本上计算，不可写目标失败后保留原算法引用和原标定。</summary>
    private static void CalibrationRollback()
    {
        ControlledStation station = new ControlledStation();
        CogCalibNPointToNPointTool tool = new CogCalibNPointToNPointTool();
        tool.Name = "Calibration";
        station.TB.Tools.Add(tool);
        CogToolBlock original = station.TB;
        string blocker = Path.Combine(TestDirectory, "calibration-blocker");
        File.WriteAllText(blocker, "block");
        station.TBPath = Path.Combine(blocker, "calibration.vpp");
        List<CalibrationData> points = new List<CalibrationData>();
        for (int y = 0; y < 3; y++)
            for (int x = 0; x < 3; x++)
                points.Add(new CalibrationData { PixelX = x * 100, PixelY = y * 100, MotorPosX = x, MotorPosY = y });
        CalibApplyInfo info;
        Check(!station.SetCalibration(points, "Calibration", out info), "Invalid destination accepted.");
        Check(info.Calibrated && info.Error.Contains("calibration.vpp"), "Test did not reach the save failure.");
        Check(ReferenceEquals(original, station.TB) && !tool.Calibration.Calibrated, "Original calibration changed on failure.");
    }

    /// <summary>列顺序改变不影响读取，倒置范围与 NaN 被拒绝，禁用行允许空数值。</summary>
    private static void Criteria()
    {
        DataTable table = new DataTable();
        string[] names = { "Enable", "U_Hi", "Y_Hi", "X_Hi", "U_Low", "Y_Low", "X_Low", "U", "Y", "X" };
        foreach (string name in names) table.Columns.Add(name, name == "Enable" ? typeof(bool) : typeof(double));
        DataRow row = table.NewRow();
        row["Enable"] = true;
        foreach (string name in names) if (name != "Enable") row[name] = name.EndsWith("Low") ? -10 : 10;
        MethodInfo read = typeof(VPForm).GetMethod("ReadVisionCriterion", BindingFlags.Static | BindingFlags.NonPublic);
        VPForm.VisionPostData result = (VPForm.VisionPostData)read.Invoke(null, new object[] { row, 0 });
        Check(result.X_Low == -10 && result.R_Hi == 10, "Column mapping failed.");
        row["X_Low"] = 20;
        ExpectFailure(() => read.Invoke(null, new object[] { row, 0 }));
        row["X_Low"] = double.NaN;
        ExpectFailure(() => read.Invoke(null, new object[] { row, 0 }));
        row = table.NewRow();
        row["Enable"] = false;
        result = (VPForm.VisionPostData)read.Invoke(null, new object[] { row, 0 });
        Check(!result.Enable, "Disabled criterion was rejected.");
    }

    /// <summary>创建已保存的定位模型与独立机器配置，退出时还原全局测试目录。</summary>
    private sealed class BindingFixture : IDisposable
    {
        public readonly VpConfigFile Config = new VpConfigFile();
        public readonly VpCameraConfig Camera = new VpCameraConfig { Name = "Camera-" + Guid.NewGuid().ToString("N"), CameraSN = "SN-001" };
        public readonly VpVppConfig Vpp = new VpVppConfig { Name = "Locate" };
        public readonly VpStation Station;
        public const string Recipe = "RecipeA";
        private readonly string _previousVisionDirectory = SysPara.VisionFileDirectory;
        private readonly string _previousSettingsDirectory = SysPara.SettingDataDirectory;
        private readonly string _previousRecipe = SysPara.RecipeName;

        /// <summary>建立不含采集工具的真实 ToolBlock，在专属临时目录中保存。</summary>
        public BindingFixture()
        {
            SysPara.VisionFileDirectory = Path.Combine(TestDirectory, "models");
            SysPara.SettingDataDirectory = Path.Combine(TestDirectory, Guid.NewGuid().ToString("N"));
            SysPara.RecipeName = Recipe;
            Camera.FolderName = Camera.Name;
            Vpp.FolderName = Vpp.Name;
            Station = new VpStation(Camera.Name, Vpp.Name, VpStationKind.Fiducial);
            Station.TB = new CogToolBlock();
            Station.IsLoadTBOk = true;
            Station.AutoVisionRunDisplay = false;
            foreach (string name in new[] { "x", "y", "A" })
            {
                Station.TB.Outputs.Add(new CogToolBlockTerminal(name, typeof(double)));
                Station.TB.Outputs[name].Value = 0.0;
            }
            Vpp.EnsureCompLimits();
            Vpp.GetCompLimit("R").Item = "角度";
            Vpp.Station = Station;
            Station.Config = Vpp;
            Camera.Vpps.Add(Vpp);
            Config.Cameras.Add(Camera);
            Config.ProductionVppId = Vpp.Id;
            string error;
            Check(Station.TrySaveTB(Station.GetVppPath(Recipe), out error), error);
        }

        /// <summary>测试不得改变后续场景使用的应用目录。</summary>
        public void Dispose()
        {
            SysPara.VisionFileDirectory = _previousVisionDirectory;
            SysPara.SettingDataDirectory = _previousSettingsDirectory;
            SysPara.RecipeName = _previousRecipe;
        }
    }

    /// <summary>生产实际使用配置选中的动态工位，查找关系不依赖相机和 VPP 的显示名称。</summary>
    private static void ProductionBinding()
    {
        using (BindingFixture fixture = new BindingFixture())
        {
            VpStation station;
            string error;
            Check(VpProductionBinding.TryResolve(fixture.Config, null, BindingFixture.Recipe, out station, out error), error);
            Check(ReferenceEquals(station, fixture.Station), "Wrong production station selected.");
            fixture.Camera.Name = "Renamed camera";
            fixture.Vpp.Name = "Renamed VPP";
            VpCameraConfig camera;
            Check(ReferenceEquals(fixture.Config.FindVpp(fixture.Vpp.Id, out camera), fixture.Vpp), "Rename broke the binding identity.");
        }
    }

    /// <summary>已经明确绑定的工位丢失时，不能用其他已加载工位代替。</summary>
    private static void MissingBinding()
    {
        using (BindingFixture fixture = new BindingFixture())
        {
            fixture.Config.ProductionVppId = "missing-station";
            VpStation station;
            string error;
            Check(!VpProductionBinding.TryResolve(fixture.Config, fixture.Station, BindingFixture.Recipe, out station, out error), "Missing binding fell back to legacy.");
            Check(station == null, "A substitute station escaped failed resolution.");
        }
    }

    /// <summary>启动前检查实际加载配方以及定位输出名称和类型。</summary>
    private static void ProductionContract()
    {
        using (BindingFixture fixture = new BindingFixture())
        {
            VpStation station;
            string error;
            Check(!VpProductionBinding.TryResolve(fixture.Config, null, "RecipeB", out station, out error), "Wrong recipe accepted.");
            fixture.Station.TB.Outputs.Remove("A");
            Check(!VpProductionBinding.TryResolve(fixture.Config, null, BindingFixture.Recipe, out station, out error), "Missing angle output accepted.");
            fixture.Station.TB.Outputs.Add(new CogToolBlockTerminal("A", typeof(string)));
            Check(!fixture.Station.ValidateFiducialOutputs(out error), "Text output accepted as a position.");
        }
    }

    /// <summary>缺少任何必需限制时，启动检查与实际算法执行均拒绝定位结果。</summary>
    private static void ProductionLimits()
    {
        using (BindingFixture fixture = new BindingFixture())
        {
            fixture.Vpp.CompLimits.Remove(fixture.Vpp.GetCompLimit("角度"));
            VpStation station;
            string error;
            Check(!VpProductionBinding.TryResolve(fixture.Config, null, BindingFixture.Recipe, out station, out error), "Missing limit accepted.");
            Check(!fixture.Station.RunAndWait(2000) && fixture.Station.LastError.Contains("角度"), "Runtime bypassed a missing limit.");
        }
    }

    /// <summary>只有未配置动态绑定时允许使用已校验的内置兼容工位。</summary>
    private static void LegacyBinding()
    {
        using (BindingFixture fixture = new BindingFixture())
        {
            fixture.Config.ProductionVppId = "";
            VpStation station;
            string error;
            Check(VpProductionBinding.TryResolve(fixture.Config, fixture.Station, BindingFixture.Recipe, out station, out error), error);
            Check(ReferenceEquals(station, fixture.Station), "Legacy station changed.");
        }
    }

    /// <summary>标定发布后，换配方、换相机、新增目标或保存算法新版本均使记录失效。</summary>
    private static void CalibrationContext()
    {
        using (BindingFixture fixture = new BindingFixture())
        {
            VpCalibration calibration = new VpCalibration();
            fixture.Config.Calibrations.Add(fixture.Camera.Name, calibration);
            VpStation station;
            string error;
            Check(!VpProductionBinding.TryResolve(fixture.Config, null, BindingFixture.Recipe, out station, out error), "Unpublished calibration accepted.");
            calibration.MarkApplied(fixture.Camera, BindingFixture.Recipe);
            Check(calibration.IsAppliedTo(fixture.Camera, BindingFixture.Recipe), "Published context did not match.");
            Check(VpProductionBinding.TryResolve(fixture.Config, null, BindingFixture.Recipe, out station, out error), error);
            Check(!calibration.IsAppliedTo(fixture.Camera, "RecipeB"), "Recipe change kept calibration valid.");
            fixture.Camera.CameraSN = "SN-002";
            Check(!calibration.IsAppliedTo(fixture.Camera, BindingFixture.Recipe), "Camera change kept calibration valid.");
            fixture.Camera.CameraSN = "SN-001";
            VpVppConfig extra = new VpVppConfig { Name = "Extra" };
            fixture.Camera.Vpps.Add(extra);
            Check(!calibration.IsAppliedTo(fixture.Camera, BindingFixture.Recipe), "New target inherited an old publication.");
            fixture.Camera.Vpps.Remove(extra);
            Check(fixture.Station.TryLoadTB(fixture.Station.TBPath, out error), error);
            Check(calibration.IsAppliedTo(fixture.Camera, BindingFixture.Recipe), "Reloading the same file changed its version.");
            fixture.Station.TB.Outputs["x"].Value = 0.25;
            Check(fixture.Station.TrySaveTB(out error), error);
            Check(!calibration.IsAppliedTo(fixture.Camera, BindingFixture.Recipe), "New model version kept calibration valid.");
        }
    }

    /// <summary>XML 往返保留工位身份、生产绑定与标定版本，目标记录不被误读成标定点。</summary>
    private static void ConfigRoundTrip()
    {
        using (BindingFixture fixture = new BindingFixture())
        {
            VpCalibration calibration = new VpCalibration();
            calibration.Points.Add(new VpCalibPoint(0, 0, 0, 0));
            calibration.Points.Add(new VpCalibPoint(10, 0, 1, 0));
            calibration.Points.Add(new VpCalibPoint(0, 10, 0, 1));
            calibration.MarkApplied(fixture.Camera, BindingFixture.Recipe);
            fixture.Config.Calibrations.Add(fixture.Camera.Name, calibration);
            VpConfigStore.Save(fixture.Config);
            Check(string.IsNullOrEmpty(VpConfigStore.LastSaveError), VpConfigStore.LastSaveError);
            string error;
            VpConfigFile loaded = VpConfigStore.Load(out error);
            Check(string.IsNullOrEmpty(error), error);
            VpCameraConfig camera;
            VpVppConfig vpp = loaded.FindVpp(loaded.ProductionVppId, out camera);
            Check(vpp != null && vpp.Id == fixture.Vpp.Id, "ID or binding changed on reload.");
            VpCalibration applied = loaded.GetCalibration(camera.Name);
            Check(applied.Points.Count == 3 && applied.AppliedModels.Count == 1, "Publication metadata was read as points.");
            Check(applied.AppliedRecipe == BindingFixture.Recipe && applied.AppliedCameraSN == "SN-001", "Publication context lost.");
        }
    }

    /// <summary>重复身份配置在写入前失败，原配置文件保持完整。</summary>
    private static void DuplicateIds()
    {
        using (BindingFixture fixture = new BindingFixture())
        {
            VpConfigStore.Save(fixture.Config);
            byte[] before = File.ReadAllBytes(VpConfigStore.FilePath);
            fixture.Camera.Vpps.Add(new VpVppConfig { Id = fixture.Vpp.Id, Name = "Duplicate" });
            VpStation station;
            string error;
            Check(!VpProductionBinding.TryResolve(fixture.Config, null, BindingFixture.Recipe, out station, out error), "Ambiguous production identity accepted.");
            VpConfigStore.Save(fixture.Config);
            Check(!string.IsNullOrEmpty(VpConfigStore.LastSaveError), "Duplicate station IDs accepted.");
            Check(Convert.ToBase64String(before) == Convert.ToBase64String(File.ReadAllBytes(VpConfigStore.FilePath)), "Invalid config overwrote the valid one.");
        }
    }

    /// <summary>旧配置自动获得工位身份，但不能凭缺失的发布信息宣称标定已经成功。</summary>
    private static void LegacyConfig()
    {
        using (BindingFixture fixture = new BindingFixture())
        {
            Directory.CreateDirectory(SysPara.SettingDataDirectory);
            File.WriteAllText(VpConfigStore.FilePath, "<VpConfig><Cameras><Camera Name='Old'><Vpp Name='Locate'/></Camera></Cameras><Calibrations><Calibration Camera='Old'><Point PixelX='1' PixelY='2' MotorPosX='3' MotorPosY='4'/></Calibration></Calibrations></VpConfig>");
            string error;
            VpConfigFile loaded = VpConfigStore.Load(out error);
            Check(string.IsNullOrEmpty(error), error);
            Check(!string.IsNullOrEmpty(loaded.Cameras[0].Vpps[0].Id) && loaded.ProductionVppId == "", "Legacy identity migration failed.");
            Check(loaded.GetCalibration("Old").AppliedSignature == "none", "Calibration success was invented.");
        }
    }

    /// <summary>修改显示名称后保留原文件路径，重载配置也能定位到同一算法。</summary>
    private static void RenameStorage()
    {
        using (BindingFixture fixture = new BindingFixture())
        {
            string path = fixture.Station.TBPath;
            fixture.Camera.Name = "Renamed camera";
            fixture.Vpp.Name = "Renamed VPP";
            fixture.Station.SetOwner(fixture.Camera.Name, fixture.Vpp.Name,
                Path.Combine(fixture.Camera.StorageFolder, fixture.Vpp.StorageFolder));
            Check(fixture.Station.GetVppPath(BindingFixture.Recipe) == path, "Rename changed the model file path.");
            VpStation station;
            string error;
            Check(VpProductionBinding.TryResolve(fixture.Config, null, BindingFixture.Recipe, out station, out error), error);
            VpConfigStore.Save(fixture.Config);
            VpConfigFile loaded = VpConfigStore.Load(out error);
            Check(string.IsNullOrEmpty(error), error);
            VpCameraConfig camera;
            VpVppConfig vpp = loaded.FindVpp(loaded.ProductionVppId, out camera);
            Check(camera.Name == "Renamed camera" && vpp.Name == "Renamed VPP", "Display names lost.");
            Check(camera.StorageFolder == fixture.Camera.FolderName && vpp.StorageFolder == fixture.Vpp.FolderName, "Storage folders lost.");
        }
    }

    /// <summary>存储目录冲突和父目录路径必须在配置写盘前拒绝。</summary>
    private static void StorageValidation()
    {
        using (BindingFixture fixture = new BindingFixture())
        {
            VpVppConfig extra = new VpVppConfig { Name = "Another", FolderName = fixture.Vpp.FolderName };
            fixture.Camera.Vpps.Add(extra);
            VpConfigStore.Save(fixture.Config);
            Check(!string.IsNullOrEmpty(VpConfigStore.LastSaveError), "Shared VPP directory accepted.");
            fixture.Camera.Vpps.Remove(extra);
            fixture.Camera.FolderName = "..";
            VpConfigStore.Save(fixture.Config);
            Check(!string.IsNullOrEmpty(VpConfigStore.LastSaveError), "Parent directory accepted.");
        }
    }

    /// <summary>离线创建实际 VPP 页面，验证绑定只改草稿、主保存任务提交后才重载新值，并渲染页面。</summary>
    private static void ProductionPage()
    {
        using (BindingFixture fixture = new BindingFixture())
        using (VPForm owner = new VPForm())
        using (VpVppPage page = new VpVppPage())
        {
            VpConfigFile config = (VpConfigFile)typeof(VPForm).GetField("_config", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner);
            config.Cameras.Add(fixture.Camera);
            config.ProductionVppId = fixture.Vpp.Id;
            CommitVisionDraft(owner);
            page.Owner = owner;
            page.Cam = fixture.Camera;
            page.Vpp = fixture.Vpp;
            page.Bind();
            page.ExposureBox.Text = fixture.Vpp.Exposure.ToString("F1");
            page.RenameBox.Text = fixture.Vpp.Name;
            System.Windows.Forms.CheckBox choice = (System.Windows.Forms.CheckBox)page.Controls.Find("vpRow_Production", true)[0];
            Check(choice.Checked, "Saved production selection was not displayed.");
            string before = File.ReadAllText(VpConfigStore.FilePath), error;
            choice.Checked = false;
            Check(config.ProductionVppId == "" && fixture.Station.Kind == VpStationKind.Custom, "Unbinding was not applied.");
            Check(File.ReadAllText(VpConfigStore.FilePath) == before && VpConfigStore.Load(out error).ProductionVppId == fixture.Vpp.Id, "Unbinding bypassed Main Save.");
            CommitVisionDraft(owner);
            Check(VpConfigStore.Load(out error).ProductionVppId == "", "Main Save tasks did not persist unbinding.");
            before = File.ReadAllText(VpConfigStore.FilePath);
            choice.Checked = true;
            Check(config.ProductionVppId == fixture.Vpp.Id && fixture.Station.Kind == VpStationKind.Fiducial, "Selection was not applied.");
            Check(File.ReadAllText(VpConfigStore.FilePath) == before && VpConfigStore.Load(out error).ProductionVppId == "", "Production selection bypassed Main Save.");
            CommitVisionDraft(owner);
            Check(VpConfigStore.Load(out error).ProductionVppId == fixture.Vpp.Id, "Selection was not saved.");
            page.Size = new System.Drawing.Size(900, 310);
            page.CreateControl();
            page.PerformLayout();
            using (System.Drawing.Bitmap image = new System.Drawing.Bitmap(900, 310))
            {
                page.DrawToBitmap(image, new System.Drawing.Rectangle(0, 0, 900, 310));
                HashSet<int> colors = new HashSet<int>();
                for (int y = 0; y < image.Height; y += 3)
                    for (int x = 0; x < image.Width; x += 3) colors.Add(image.GetPixel(x, y).ToArgb());
                Check(colors.Count > 8, "UI render was blank.");
                image.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "production-vpp-preview.png"));
            }
        }
    }

    /// <summary>固定生产选项在设计器初始化后即存在，绑定业务时不重复创建。</summary>
    private static void DesignerProductionControl()
    {
        using (VpVppPage page = new VpVppPage())
        {
            Check(page.Controls.Find("vpRow_Production", true).Length == 1, "Fixed checkbox is missing from the designer.");
            Check(page.Controls.Find("vpRow_Production", true)[0].Text == "生产定位", "Designer caption is missing.");
        }
    }

    /// <summary>切换三种语言只改变标题和标签，不重建行、不覆盖输入，也不改变绑定和补偿键。</summary>
    private static void VppLanguageRefresh()
    {
        LanguageType previous = SysPara.LanguageShow;
        try
        {
            using (BindingFixture fixture = new BindingFixture())
            using (VPForm owner = new VPForm())
            using (VpVppPage page = new VpVppPage())
            {
                VpConfigFile config = (VpConfigFile)typeof(VPForm).GetField("_config", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner);
                config.Cameras.Add(fixture.Camera);
                config.ProductionVppId = fixture.Vpp.Id;
                page.Owner = owner;
                page.Cam = fixture.Camera;
                page.Vpp = fixture.Vpp;
                page.Bind();
                System.Windows.Forms.DataGridViewRow row = page.CompGrid.Rows[0];
                page.CompGrid.CurrentCell = row.Cells[1];
                row.Cells[1].Value = "0.12345";
                page.ExposureBox.Text = "12.345";
                page.RenameBox.Text = "Operator input";
                string[] captions = { "生产定位", "Production positioning", "Posicionamiento de producción" };
                string[] capture = { "拍照", "Capture", "Capturar" };
                string[] compensation = { "补偿值", "Compensation", "Compensación" };
                string[] angle = { "角度", "Angle", "Ángulo" };
                for (int i = 0; i < 3; i++)
                {
                    SysPara.LanguageShow = (LanguageType)i;
                    page.RefreshTexts();
                    Check(page.Controls.Find("vpRow_Production", true)[0].Text == captions[i], "Production caption did not switch.");
                    Check(page.Controls.Find("vpBtn_Capture", true)[0].Text == capture[i], "Fixed button did not switch.");
                    Check(page.CompGrid.Columns["cCompensation"].HeaderText == compensation[i], "Compensation header did not switch.");
                    Check(Convert.ToString(page.CompGrid.Rows[2].Cells["cItem"].FormattedValue) == angle[i], "Angle display did not switch.");
                    Check(Convert.ToString(page.CompGrid.Rows[2].Cells["cItem"].Value) == "角度", "Angle display replaced the raw key.");
                    Check(page.InfoLabel.Text.Contains(captions[i]), "Dynamic production caption did not switch.");
                    Check(ReferenceEquals(row, page.CompGrid.Rows[0]) && row.Cells[1].Value.ToString() == "0.12345", "Language refresh destroyed an edited row.");
                    Check(page.ExposureBox.Text == "12.345" && page.RenameBox.Text == "Operator input", "Language refresh changed input data.");
                    Check(config.ProductionVppId == fixture.Vpp.Id && fixture.Vpp.GetCompLimit("角度") != null, "Language refresh changed business keys.");
                }
            }
        }
        finally { SysPara.LanguageShow = previous; }
    }

    /// <summary>New defaults use R, remain valid for positioning, and dock the editor immediately below its header at any height.</summary>
    private static void VppDefaultsAndLayout()
    {
        using (BindingFixture fixture = new BindingFixture())
        using (VpVppPage page = new VpVppPage())
        {
            fixture.Vpp.CompLimits.Clear(); fixture.Vpp.EnsureCompLimits();
            Check(fixture.Vpp.CompLimits.Select(row => row.Item).SequenceEqual(new[] { "X", "Y", "R" }), "New VPP defaults do not use X/Y/R.");
            Check(fixture.Station.RunAndWait(2000), fixture.Station.LastError);
            fixture.Station.TB.Outputs["A"].Value = 6.0;
            Check(!fixture.Station.RunAndWait(2000), "The R limit did not reject an excessive angle.");
            Control head = page.Controls.Find("head", true).Single(), band = page.Controls.Find("band", true).Single();
            foreach (int height in new[] { 400, 700 })
            {
                page.Size = new System.Drawing.Size(900, height); page.PerformLayout(); band.PerformLayout();
                Check(band.Top == head.Bottom && page.CompGrid.Parent.Top <= 2, "The VPP editor has a blank gap: head=" + head.Bottom + ", band=" + band.Top + ", grid=" + page.CompGrid.Parent.Top);
            }
        }
    }

    /// <summary>表接口无需加载算法，返回独立行和列表，查找失败与空表可区分。</summary>
    private static void CompensationTable()
    {
        using (VPForm owner = new VPForm())
        {
            VpConfigFile config = (VpConfigFile)typeof(VPForm).GetField("_config", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner);
            VpCameraConfig camera = new VpCameraConfig { Name = "TakePCBA" };
            VpVppConfig vpp = new VpVppConfig { Name = "VPP1" };
            vpp.EnsureCompLimits();
            vpp.GetCompLimit("X").Compensation = 0.125;
            vpp.CompLimits.Add(new VpCompLimitItem("Custom", -10, 10) { Compensation = -2 });
            camera.Vpps.Add(vpp);
            config.Cameras.Add(camera);
            List<VpCompLimitItem> table = owner.GetCompensationTable("TakePCBA", "VPP1");
            Check(table.Count == 4 && table[0].Item == "X" && table[2].Item == "R" && table[3].Item == "Custom", "Snapshot lost rows or raw keys.");
            Check(table[0].Compensation == 0.125 && table[0].Min == -1 && table[0].Max == 1 && table[3].Compensation == -2, "Snapshot lost configuration values.");
            table[0].Item = "Changed";
            table[0].Compensation = 999;
            table.Clear();
            Check(vpp.CompLimits.Count == 4 && vpp.GetCompLimit("X").Compensation == 0.125, "Snapshot mutation changed source configuration.");
            Check(owner.GetCompensationTable("Missing", "VPP1") == null && owner.GetCompensationTable("TakePCBA", "Missing") == null, "Missing configuration did not return null.");
            vpp.CompLimits.Clear();
            Check(owner.GetCompensationTable("TakePCBA", "VPP1").Count == 0, "Empty table was not preserved.");
        }
    }

    /// <summary>补偿值以不依赖区域设置的完整精度保存，旧 XML 缺省为零。</summary>
    private static void CompensationConfig()
    {
        using (BindingFixture fixture = new BindingFixture())
        {
            double expected = -0.12345678901234567;
            fixture.Vpp.GetCompLimit("角度").Compensation = expected;
            VpConfigStore.Save(fixture.Config);
            string error;
            VpConfigFile loaded = VpConfigStore.Load(out error);
            Check(string.IsNullOrEmpty(error), error);
            Check(loaded.Cameras[0].Vpps[0].GetCompLimit("角度").Compensation == expected, "Compensation precision was lost on reload.");
            System.Xml.XmlDocument document = new System.Xml.XmlDocument();
            document.Load(VpConfigStore.FilePath);
            foreach (System.Xml.XmlElement row in document.SelectNodes("/VpConfig/Cameras/Camera/Vpp/Comp"))
                row.RemoveAttribute("Compensation");
            document.Save(VpConfigStore.FilePath);
            loaded = VpConfigStore.Load(out error);
            Check(string.IsNullOrEmpty(error), error);
            Check(loaded.Cameras[0].Vpps[0].GetCompLimit("角度").Compensation == 0, "Legacy compensation did not default to zero.");
        }
    }

    /// <summary>表格编辑只保留补偿草稿，主保存任务提交后重载新值，非法输入不改变数值精度。</summary>
    private static void CompensationEditor()
    {
        using (BindingFixture fixture = new BindingFixture())
        using (VPForm owner = new VPForm())
        using (VpVppPage page = new VpVppPage())
        {
            VpConfigFile config = (VpConfigFile)typeof(VPForm).GetField("_config", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner);
            config.Cameras.Add(fixture.Camera);
            fixture.Vpp.GetCompLimit("X").Min = -0.123456789;
            CommitVisionDraft(owner);
            string before = File.ReadAllText(VpConfigStore.FilePath);
            page.Owner = owner; page.Cam = fixture.Camera; page.Vpp = fixture.Vpp;
            page.Bind();
            Check(page.CompGrid.Columns.Count == 4, "Designer compensation column is missing.");
            System.Windows.Forms.DataGridViewCell cell = page.CompGrid.Rows[0].Cells["cCompensation"];
            MethodInfo sync = typeof(VPForm).GetMethod("SyncCompLimitFromGrid", BindingFlags.NonPublic | BindingFlags.Instance);
            cell.Value = "-0.12345678901234567";
            sync.Invoke(owner, new object[] { page.CompGrid, fixture.Vpp, 0, cell.ColumnIndex });
            double expected = fixture.Vpp.GetCompLimit("X").Compensation;
            Check(expected == -0.12345678901234567 && fixture.Vpp.GetCompLimit("X").Min == -0.123456789, "Editing compensation rounded another field.");
            string error;
            Check(File.ReadAllText(VpConfigStore.FilePath) == before && VpConfigStore.Load(out error).Cameras[0].Vpps[0].GetCompLimit("X").Compensation == 0, "Compensation editing bypassed Main Save.");
            CommitVisionDraft(owner);
            Check(VpConfigStore.Load(out error).Cameras[0].Vpps[0].GetCompLimit("X").Compensation == expected, "Edited compensation was not saved.");
            before = File.ReadAllText(VpConfigStore.FilePath);
            page.CompGrid.Rows[0].Cells["cMin"].Value = "-0.5";
            sync.Invoke(owner, new object[] { page.CompGrid, fixture.Vpp, 0, page.CompGrid.Columns["cMin"].Index });
            Check(fixture.Vpp.GetCompLimit("X").Compensation == expected, "Editing a limit rounded the compensation value.");
            foreach (string invalid in new[] { "NaN", "Infinity", "-Infinity", "", "invalid" })
            {
                cell.Value = invalid;
                sync.Invoke(owner, new object[] { page.CompGrid, fixture.Vpp, 0, cell.ColumnIndex });
                Check(fixture.Vpp.GetCompLimit("X").Compensation == expected && Convert.ToDouble(cell.Value) == expected, "Invalid compensation was accepted or not restored.");
            }
            Check(File.ReadAllText(VpConfigStore.FilePath) == before, "Later limit or invalid-input edits bypassed Main Save.");
        }
    }

    /// <summary>在离线视觉单元中执行真实主保存写盘引擎及视觉接受接口，不另写一套事务算法。</summary>
    private static void CommitVisionDraft(VPForm owner)
    {
        MethodInfo prepare = typeof(VPForm).GetMethod("PrepareConfigurationSave", BindingFlags.Instance | BindingFlags.NonPublic);
        object files = prepare.Invoke(owner, null);
        Type configuration = typeof(VPForm).Assembly.GetType("AlphaRap.Configuration.ProjectConfiguration");
        configuration.GetMethod("WriteFiles", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new[] { files });
        typeof(VPForm).GetMethod("AcceptSaved", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(owner, null);
    }

    /// <summary>角度名称编辑期间切换语言，提交未改动译文仍保存原键；自定义名称保持原样。</summary>
    private static void CompensationItemEditing()
    {
        WithLanguageFiles(delegate
        {
            using (BindingFixture fixture = new BindingFixture())
            using (VPForm owner = new VPForm())
            using (System.Windows.Forms.Form host = new System.Windows.Forms.Form())
            using (VpVppPage page = new VpVppPage())
            {
                VpConfigFile config = (VpConfigFile)typeof(VPForm).GetField("_config", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner);
                config.Cameras.Add(fixture.Camera);
                fixture.Vpp.CompLimits.Add(new VpCompLimitItem("Operator angle", -1, 1));
                page.Owner = owner; page.Cam = fixture.Camera; page.Vpp = fixture.Vpp; page.Bind();
                host.Size = new System.Drawing.Size(940, 380);
                host.ShowInTaskbar = false;
                host.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
                host.Location = new System.Drawing.Point(-30000, -30000);
                page.Dock = System.Windows.Forms.DockStyle.Fill;
                host.Controls.Add(page); host.Show(); System.Windows.Forms.Application.DoEvents();
                SetLanguage(LanguageType.English); page.RefreshTexts();
                System.Windows.Forms.DataGridViewCell cell = page.CompGrid.Rows[2].Cells["cItem"];
                page.CompGrid.CurrentCell = cell;
                Check(page.CompGrid.BeginEdit(true), "Angle name editing did not begin.");
                string pending = page.CompGrid.EditingControl.Text;
                Check(pending == "Angle", "Editor did not show the localized angle.");
                SetLanguage(LanguageType.Español); page.RefreshTexts();
                Check(page.CompGrid.EditingControl.Text == pending, "Language switch overwrote the pending name edit.");
                Check(page.CompGrid.EndEdit(), "Angle name editing did not commit.");
                Check(fixture.Vpp.GetCompLimit("角度") != null && Convert.ToString(cell.Value) == "角度", "Unchanged localized name replaced the raw key.");
                Check(Convert.ToString(cell.FormattedValue) == "Ángulo", "Committed cell did not use the new language.");
                Check(Convert.ToString(page.CompGrid.Rows[3].Cells["cItem"].FormattedValue) == "Operator angle", "Custom project name was translated.");
                page.CompGrid.CurrentCell = page.CompGrid.Rows[0].Cells["cItem"];
                page.CompGrid.FirstDisplayedScrollingRowIndex = 0;
                foreach (LanguageType language in Enum.GetValues(typeof(LanguageType)))
                {
                    SetLanguage(language); page.RefreshTexts(); page.PerformLayout();
                    using (System.Drawing.Bitmap image = new System.Drawing.Bitmap(page.Width, page.Height))
                    {
                        page.DrawToBitmap(image, new System.Drawing.Rectangle(0, 0, page.Width, page.Height));
                        image.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "compensation-vpp-" + language + ".png"));
                    }
                }
            }
        });
    }

    /// <summary>补偿配置仅供流程读取，设置较大值也不改变原始视觉输出和既有超限判断。</summary>
    private static void CompensationNotApplied()
    {
        using (BindingFixture fixture = new BindingFixture())
        {
            foreach (VpCompLimitItem item in fixture.Vpp.CompLimits) item.Compensation = 100;
            Check(fixture.Station.RunAndWait(2000), fixture.Station.LastError);
            Check(fixture.Station.Fiducial.x == 0 && fixture.Station.Fiducial.y == 0 && fixture.Station.Fiducial.u == 0, "Compensation was added to vision coordinates.");
            Check(fixture.Station.LastRunResult.X == 0 && fixture.Station.LastRunResult.Y == 0 && fixture.Station.LastRunResult.Angle == 0, "Compensation was added to published results.");
        }
    }

    /// <summary>已有现场翻译优先于三语底稿，不被页面刷新覆盖。</summary>
    private static void SiteTranslation()
    {
        string path = Path.Combine(SysPara.LanguageDataDirectory, "English.xml");
        Directory.CreateDirectory(SysPara.LanguageDataDirectory);
        if (!File.Exists(path))
            File.Copy(Path.Combine(Path.GetDirectoryName(typeof(VPForm).Assembly.Location), "LanguageData", "English.xml"), path);
        System.Xml.XmlDocument document = new System.Xml.XmlDocument();
        document.Load(path);
        System.Xml.XmlElement entry = (System.Xml.XmlElement)document.SelectSingleNode("/English/VPForm/vpRow_Production");
        string original = entry.GetAttribute("ComponentText");
        LanguageType previous = SysPara.LanguageShow;
        try
        {
            entry.SetAttribute("ComponentText", "Site positioning");
            document.Save(path);
            LanguageHost.GetField("_langDocs", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, null);
            SysPara.LanguageShow = LanguageType.English;
            using (VpVppPage page = new VpVppPage())
            {
                page.RefreshTexts();
                Check(page.Controls.Find("vpRow_Production", true)[0].Text == "Site positioning", "Site translation was overwritten.");
            }
        }
        finally
        {
            entry.SetAttribute("ComponentText", original);
            document.Save(path);
            LanguageHost.GetField("_langDocs", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, null);
            SysPara.LanguageShow = previous;
        }
    }

    /// <summary>带业务对象的动态 Tab 页在启动扫描和运行时登记中都不作为翻译条目。</summary>
    private static void UserNamedTabs()
    {
        List<ComponentTextInfo>[] previous = SysPara.ComponentLangurageList;
        try
        {
            SysPara.ComponentLangurageList = new List<ComponentTextInfo>[3];
            for (int i = 0; i < 3; i++) SysPara.ComponentLangurageList[i] = new List<ComponentTextInfo>();
            using (System.Windows.Forms.TabControl tabs = new System.Windows.Forms.TabControl())
            using (System.Windows.Forms.TabPage page = new System.Windows.Forms.TabPage())
            {
                page.Name = "dynCam_OperatorCamera";
                page.Text = "Operator camera name";
                page.Tag = new VpCameraConfig { Name = page.Text };
                tabs.TabPages.Add(page);
                LanguageHost.GetMethod("RegisterLanguage", BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { tabs, "VPForm" });
                foreach (List<ComponentTextInfo> list in SysPara.ComponentLangurageList)
                    Check(list.Count == 0, "User camera name registered by the control scanner.");
            }
        }
        finally { SysPara.ComponentLangurageList = previous; }
    }

    /// <summary>窗体释放后移除静态语言事件订阅，避免无效页面留在刷新链中。</summary>
    private static void LanguageSubscription()
    {
        FieldInfo field = LanguageHost.GetField("LanguageChanged", BindingFlags.Static | BindingFlags.NonPublic);
        Delegate before = (Delegate)field.GetValue(null);
        int count = before == null ? 0 : before.GetInvocationList().Length;
        VPForm form = new VPForm();
        Delegate active = (Delegate)field.GetValue(null);
        Check(active.GetInvocationList().Length == count + 1, "Language handler was not registered.");
        form.Dispose();
        Delegate after = (Delegate)field.GetValue(null);
        Check((after == null ? 0 : after.GetInvocationList().Length) == count, "Disposed form retained its language handler.");
    }

    /// <summary>被反射调用的业务校验必须以明确异常拒绝非法输入。</summary>
    private static void ExpectFailure(Action action)
    {
        try { action(); }
        catch (TargetInvocationException ex) { Check(ex.InnerException is InvalidOperationException, "Unexpected failure: " + ex); return; }
        throw new InvalidOperationException("Invalid input was accepted.");
    }
}
