using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Linq;
using AlphaRap;
using Cognex.VisionPro;
using Cognex.VisionPro.CalibFix;
using Cognex.VisionPro.PMAlign;
using Cognex.VisionPro.ToolBlock;
using Cognex.VisionPro.Implementation.Internal;

internal static partial class Tests
{
    /// <summary>运行真实生成脚本、标准工具与新增入口，完全不连接相机。</summary>
    private static void RunBuiltInAlgorithmTests()
    {
        Run("new calibration contains acquisition pattern nine-point and rotation tools", BuiltInCalibration);
        Run("rotation calibration fits angled affine projections and rejects stale results", BuiltInRotation);
        Run("positioning resets outputs when offline image is missing", BuiltInFailure);
        Run("trained standard VPP returns image coordinates and calibrated coordinates", BuiltInPositioning);
        Run("template creation preserves existing and corrupt VPP files", BuiltInExistingFiles);
        Run("saved empty placeholders become template drafts and remain discardable", BuiltInEmptyPlaceholder);
        Run("script-only metadata-only and intentionally empty drafts stay unchanged", BuiltInNonEmptyGuards);
        Run("built-in VPP scripts and rotation samples survive save and reload", BuiltInRoundTrip);
        Run("legacy template upgrade preserves training calibration samples and discard", BuiltInUpgrade);
        Run("legacy template upgrade preserves customized scripts ports and connections", BuiltInUpgradeGuards);
        Run("camera add calibration and add VPP create main-save drafts", BuiltInCreationEntries);
        Run("TB editor uses registered templates and protects saved algorithms", PositioningTemplateEditor);
        Run("built-in script messages honor site translations and preserve image inputs", BuiltInLanguage);
        Run("legacy reserved criteria keep their values without blocking the production row", LegacyReservedCriteria);
        RunPrecisionCalibrationTests();
    }

    /// <summary>标定包含原始像素定位、九点标定和独立旋转算法；生产 VPP 包含定位和标定工具。</summary>
    private static void BuiltInCalibration()
    {
        using (CogToolBlock calibration = VpAlgorithmTemplates.Create(true, "", 12))
        using (CogToolBlock positioning = VpAlgorithmTemplates.Create(false, "", 15))
        {
            CogToolBlock capture = calibration.Tools["AcquireImage"] as CogToolBlock;
            Check(capture != null && capture.Tools["CameraCapture"] is CogAcqFifoTool, "Acquisition tool is missing.");
            Check(calibration.Tools["LocatePattern"] is CogPMAlignTool, "Calibration feature locator is missing.");
            Check(calibration.Tools["Calibration"] is CogCalibNPointToNPointTool, "Nine-point tool is missing.");
            Check(calibration.Tools["RotationCenter"] is CogToolBlock, "Rotation center algorithm is missing.");
            Check(VpAlgorithmTemplates.IsBuiltIn(positioning) && positioning.Tools.Count == 4, "Standard positioning tools are missing.");
            Check(capture.DataBindings.Count == 5 && calibration.Outputs.DataBindings.Count == 15, "Root image and result ports have no native connections.");
            Check(calibration.Tools["LocatePattern"].DataBindings.Count == 1 && calibration.Tools["Calibration"].DataBindings.Count == 1, "Native image inputs have no connections.");
            Check((string)positioning.Inputs["CameraSN"].Value == "" && (double)positioning.Inputs["Exposure"].Value == 15, "Camera defaults changed.");
            Check(!(bool)positioning.Inputs["UseInputImage"].Value && double.IsNaN((double)positioning.Outputs["x"].Value), "The template invented a valid production result.");
            CheckEditorConnections(calibration); CheckEditorConnections(positioning);
        }
    }

    /// <summary>验证带旋转角的仿射轨迹拟合，以及错误输入清除上次中心。</summary>
    private static void BuiltInRotation()
    {
        using (CogToolBlock calibration = VpAlgorithmTemplates.Create(true, "", 10))
        {
            CalibrateRotationModel(calibration);
            double[] angles = { -90, -45, 0, 40, 95, 150 };
            double[] x = angles.Select(a => 1234 + 82 * Math.Cos(a * Math.PI / 180) + 17 * Math.Sin(a * Math.PI / 180)).ToArray();
            double[] y = angles.Select(a => -567 + 12 * Math.Cos(a * Math.PI / 180) - 63 * Math.Sin(a * Math.PI / 180)).ToArray();
            SetRotationSamples(calibration, x, y, angles);
            calibration.Inputs["RunRotation"].Value = true; calibration.Run();
            Check(calibration.RunStatus.Result == CogToolResultConstants.Accept && (bool)calibration.Outputs["RotationValid"].Value, calibration.RunStatus.Message);
            Check(Math.Abs((double)calibration.Outputs["CenterX"].Value - 1234) < 1e-7 && Math.Abs((double)calibration.Outputs["CenterY"].Value + 567) < 1e-7, "Affine rotation center is incorrect.");
            Check((double)calibration.Outputs["RmsError"].Value < 1e-7, "Exact sample residual is not zero.");
            foreach (double[] invalid in new[] { new double[] { 0, 0, 0, 0, 0, 0 }, new double[] { 0, 360, 720, 1080, 1440, 1800 }, new double[] { 0, 10, double.NaN, 20, 30, 40 }, new double[] { 0, 1 } })
            {
                SetRotationSamples(calibration, x, y, invalid); calibration.Run();
                Check(calibration.RunStatus.Result == CogToolResultConstants.Error && !(bool)calibration.Outputs["RotationValid"].Value, "Degenerate samples were accepted.");
                Check(double.IsNaN((double)calibration.Outputs["CenterX"].Value), "A stale center survived invalid samples.");
            }
        }
    }

    /// <summary>给独立旋转算法提供同一顺序的像素及实际轴角度。</summary>
    private static void SetRotationSamples(CogToolBlock calibration, double[] x, double[] y, double[] angles)
    {
        calibration.Inputs["RotationPixelX"].Value = x; calibration.Inputs["RotationPixelY"].Value = y; calibration.Inputs["RotationAnglesDegrees"].Value = angles;
    }

    /// <summary>离线图像缺失时不枚举相机，也不沿用旧产品坐标。</summary>
    private static void BuiltInFailure()
    {
        using (CogToolBlock block = VpAlgorithmTemplates.Create(false, "", 10))
        {
            block.Inputs["UseInputImage"].Value = true;
            block.Outputs["Accepted"].Value = true; block.Outputs["x"].Value = 42.0;
            block.Run();
            Check(block.RunStatus.Result == CogToolResultConstants.Error, "Missing input image was accepted.");
            Check(!(bool)block.Outputs["Accepted"].Value && double.IsNaN((double)block.Outputs["x"].Value), "Old coordinates survived a failed run.");
        }
    }

    /// <summary>使用真实 PMAlign 训练和运行，检查内置脚本结果以及像素到机械坐标的工具连接。</summary>
    private static void BuiltInPositioning()
    {
        VpStation station = new VpStation("BuiltInRegression", VpStationKind.Custom) { AutoVisionRunDisplay = false };
        try
        {
            string error;
            Check(station.TryCreateTemplateTB(Path.Combine(TestDirectory, "builtin-position.vpp"), false, out error), error);
            CogToolBlock block = station.TB;
            block.Inputs.Add(new CogToolBlockTerminal("UserThreshold", 0.5, typeof(double)));
            var flowMethod = station.GetFlowMethods(true).Single(method => method.Id == "vision.run");
            Check(flowMethod.Inputs.Any(field => field.Name == "TB:UserThreshold") && flowMethod.Inputs.Count(field => field.Name.StartsWith("TB:")) == 1, "Flow binding exposed managed ports or hid user inputs.");
            using (Bitmap bitmap = new Bitmap(240, 180))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.Black);
                graphics.FillRectangle(Brushes.White, 70, 45, 18, 72);
                graphics.FillRectangle(Brushes.White, 70, 100, 70, 17);
                graphics.FillEllipse(Brushes.White, 125, 50, 20, 28);
                CogImage8Grey image = new CogImage8Grey(bitmap);
                block.Inputs["UseInputImage"].Value = true; block.Inputs["InputImage"].Value = image;
                long untrainedRun;
                Check(station.TryRunTB(out untrainedRun) && System.Threading.SpinWait.SpinUntil(() => !station.IsBusy, 5000), "Untrained algorithm did not finish.");
                Check(station.LastRunResult.State == VisionRunState.Faulted && station.LastRunResult.Error.Contains("LocatePattern"), "Capture hid the template-training instructions.");
                CogPMAlignTool pattern = (CogPMAlignTool)block.Tools["LocatePattern"];
                CogRectangleAffine region = new CogRectangleAffine(); region.SetCenterLengthsRotationSkew(110, 85, 110, 110, 0, 0);
                pattern.Pattern.TrainImage = image; pattern.Pattern.TrainRegion = region;
                pattern.Pattern.Origin.TranslationX = 110; pattern.Pattern.Origin.TranslationY = 85;
                pattern.Pattern.Train();
                block.Inputs["UseInputImage"].Value = true; block.Inputs["InputImage"].Value = image;
                station.VisionRun();
                Check(station.IsAccept && (bool)block.Outputs["Accepted"].Value, "Raw positioning: " + block.RunStatus.Message);
                Check(ReferenceEquals(pattern.InputImage, block.Outputs["OutputImage"].Value) && ReferenceEquals(pattern.InputImage, ((CogCalibNPointToNPointTool)block.Tools["Calibration"]).InputImage), "Image connections did not reach the native tools.");
                Check(Math.Abs(station.Fiducial.x - 110) < 0.2 && Math.Abs(station.Fiducial.y - 85) < 0.2, "Custom station discarded template coordinates.");
                Check(!(bool)block.Outputs["Calibrated"].Value, "Uncalibrated coordinates were marked calibrated.");
                CogCalibNPointToNPoint calibration = ((CogCalibNPointToNPointTool)block.Tools["Calibration"]).Calibration;
                calibration.NumPoints = 9;
                for (int i = 0; i < 9; i++)
                {
                    double px = 20 + (i % 3) * 80, py = 20 + (i / 3) * 60;
                    calibration.SetUncalibratedPointX(i, px); calibration.SetUncalibratedPointY(i, py);
                    calibration.SetRawCalibratedPointX(i, 1 + 0.01 * px); calibration.SetRawCalibratedPointY(i, 2 + 0.02 * py);
                }
                calibration.Calibrate(); station.VisionRun();
                Check(station.IsAccept && (bool)block.Outputs["Calibrated"].Value, "Calibrated positioning: " + block.RunStatus.Message);
                Check(Math.Abs(station.Fiducial.x - 2.1) < 0.02 && Math.Abs(station.Fiducial.y - 3.7) < 0.02, "Nine-point transform was not applied to positioning outputs.");
                block.Inputs["InputImage"].Value = null; block.Run();
                Check(block.RunStatus.Result == CogToolResultConstants.Error && !(bool)block.Outputs["Accepted"].Value && double.IsNaN((double)block.Outputs["x"].Value) && block.Outputs["OutputImage"].Value == null, "Capture failure retained a previous result or image.");
                block.Inputs["InputImage"].Value = image; block.Run();
                Check((bool)block.Outputs["Accepted"].Value, "Connections did not recover after failed capture.");
                RunMode previous = SysPara.SystemMode;
                try
                {
                    SysPara.SystemMode = RunMode.RUN;
                    bool rejected = false; try { station.VisionRun(); } catch (InvalidOperationException) { rejected = true; }
                    Check(rejected && !station.IsAccept, "Production accepted an offline sample image.");
                }
                finally { SysPara.SystemMode = previous; }
            }
        }
        finally { VisionproInterface.VList.Remove(station); if (station.TB != null) station.TB.Dispose(); }
    }

    /// <summary>已有文件保留自定义内容，损坏文件不被生成模板覆盖。</summary>
    private static void BuiltInExistingFiles()
    {
        string path = Path.Combine(TestDirectory, "builtin-existing.vpp");
        using (CogToolBlock original = new CogToolBlock())
        {
            original.Outputs.Add(new CogToolBlockTerminal("UserOutput", 7.0, typeof(double)));
            CogSerializer.SaveObjectToFile(original, path);
        }
        VpStation station = new VpStation("ExistingRegression", VpStationKind.Custom);
        try
        {
            byte[] before = File.ReadAllBytes(path); string error;
            Check(station.TryCreateTemplateTB(path, false, out error), error);
            Check(!VpAlgorithmTemplates.IsBuiltIn(station.TB) && (double)station.TB.Outputs["UserOutput"].Value == 7, "Existing custom algorithm was replaced.");
            Check(before.SequenceEqual(File.ReadAllBytes(path)), "Existing file changed during creation.");
            string corrupt = Path.Combine(TestDirectory, "builtin-corrupt.vpp"); File.WriteAllText(corrupt, "not a vpp");
            Check(!station.TryCreateTemplateTB(corrupt, true, out error) && File.ReadAllText(corrupt) == "not a vpp", "A corrupt existing file was overwritten.");
        }
        finally { VisionproInterface.VList.Remove(station); if (station.TB != null) station.TB.Dispose(); }
    }

    /// <summary>兼容旧空文件及 SDK 空 ScriptData，补齐后不提前落盘，放弃时恢复旧文件。</summary>
    private static void BuiltInEmptyPlaceholder()
    {
        foreach (bool calibration in new[] { true, false })
        {
            string path = Path.Combine(TestDirectory, calibration ? "empty-calibration.vpp" : "empty-positioning.vpp");
            using (CogToolBlock empty = new CogToolBlock())
            {
                empty.UserData["ScriptData"] = new CogDictionary();
                CogSerializer.SaveObjectToFile(empty, path);
            }
            VpStation station = new VpStation("EmptyRegression", VpStationKind.Custom);
            try
            {
                byte[] before = File.ReadAllBytes(path); string error;
                Check(station.TryLoadTB(path, out error) && station.TB.Tools.Count == 0, "Empty fixture did not load.");
                Check(station.TryCreateTemplateTB(path, calibration, out error), error);
                Check(VpAlgorithmTemplates.IsBuiltIn(station.TB) && station.TB.Tools.Count == (calibration ? 11 : 4), "The loaded empty placeholder remained empty.");
                Check(before.SequenceEqual(File.ReadAllBytes(path)), "Filling an empty placeholder bypassed Main Save.");
                CogToolBlock draft = station.TB; draft.Inputs["Exposure"].Value = 35.0;
                Check(station.TryCreateTemplateTB(path, calibration, out error) && ReferenceEquals(draft, station.TB) && (double)station.TB.Inputs["Exposure"].Value == 35, "Reopening replaced the in-memory draft.");
                Check(station.TryDiscardTBChanges(out error) && station.TB.Tools.Count == 0 && !VpAlgorithmTemplates.IsBuiltIn(station.TB), "Discard did not restore the saved empty placeholder.");
                Check(station.TryCreateTemplateTB(path, calibration, out error) && station.TrySaveTB(out error), error);
                using (CogToolBlock loaded = (CogToolBlock)CogSerializer.LoadObjectFromFile(path))
                    Check(VpAlgorithmTemplates.IsBuiltIn(loaded) && loaded.Tools.Count == (calibration ? 11 : 4), "Saved templates did not survive reload.");
            }
            finally { VisionproInterface.VList.Remove(station); if (station.TB != null) station.TB.Dispose(); }
        }
    }

    /// <summary>没有工具不等于没有算法内容，用户脚本、业务元数据及主动编辑的空草稿都应保留。</summary>
    private static void BuiltInNonEmptyGuards()
    {
        Action<CogToolBlock>[] configure = {
            block => block.UserData["Owner"] = "Application",
            block => { CogDictionary data = new CogDictionary(); data["Business"] = 1; block.UserData["ScriptData"] = data; },
            block => block.CreateNewScript(Cognex.VisionPro.Implementation.Internal.CogScriptLanguageConstants.ScriptCSharp, CogToolBlockScriptTypeConstants.Advanced)
        };
        for (int index = 0; index < configure.Length; index++)
        {
            string path = Path.Combine(TestDirectory, "preserved-content-" + index + ".vpp");
            using (CogToolBlock original = new CogToolBlock()) { configure[index](original); CogSerializer.SaveObjectToFile(original, path); }
            VpStation station = new VpStation("ContentRegression", VpStationKind.Custom);
            try
            {
                string error; byte[] before = File.ReadAllBytes(path);
                Check(station.TryCreateTemplateTB(path, true, out error) && !VpAlgorithmTemplates.IsBuiltIn(station.TB), "Existing non-tool content was replaced.");
                Check(before.SequenceEqual(File.ReadAllBytes(path)), "Protected content was written during editing.");
            }
            finally { VisionproInterface.VList.Remove(station); if (station.TB != null) station.TB.Dispose(); }
        }
        VpStation emptyDraft = new VpStation("EmptyDraftRegression", VpStationKind.Custom);
        try
        {
            string error; string path = Path.Combine(TestDirectory, "intentional-empty-draft.vpp");
            Check(emptyDraft.TryCreateTB(path, out error), error); CogToolBlock current = emptyDraft.TB;
            Check(emptyDraft.TryCreateTemplateTB(path, true, out error) && ReferenceEquals(current, emptyDraft.TB) && !VpAlgorithmTemplates.IsBuiltIn(emptyDraft.TB), "An intentional empty edit was overwritten.");
        }
        finally { VisionproInterface.VList.Remove(emptyDraft); if (emptyDraft.TB != null) emptyDraft.TB.Dispose(); }
    }

    /// <summary>文件重载和主保存使用的深拷贝都保留可执行脚本及独立采样数组。</summary>
    private static void BuiltInRoundTrip()
    {
        using (CogToolBlock block = VpAlgorithmTemplates.Create(true, "", 10))
        {
            CalibrateRotationModel(block);
            SetRotationSamples(block, new double[] { 12, 2, -8, 2 }, new double[] { 3, 13, 3, -7 }, new double[] { 0, 90, 180, 270 });
            string path = Path.Combine(TestDirectory, "builtin-roundtrip.vpp"); CogSerializer.SaveObjectToFile(block, path);
            using (CogToolBlock loaded = (CogToolBlock)CogSerializer.LoadObjectFromFile(path))
            using (CogToolBlock copy = (CogToolBlock)CogSerializer.DeepCopyObject(loaded))
            {
                Check(VpAlgorithmTemplates.IsBuiltIn(loaded), "Template identity was lost.");
                CheckEditorConnections(loaded); CheckEditorConnections(copy);
                copy.Inputs["RunRotation"].Value = true; copy.Run();
                Check(copy.RunStatus.Result == CogToolResultConstants.Accept && Math.Abs((double)copy.Outputs["CenterX"].Value - 2) < 1e-8 && (bool)copy.Outputs["RotationValid"].Value, "Serialized script did not execute.");
                ((double[])copy.Inputs["RotationPixelX"].Value)[0] = 999;
                Check(((double[])loaded.Inputs["RotationPixelX"].Value)[0] == 12, "Save snapshots share mutable sample arrays.");
            }
        }
    }

    /// <summary>用已发布第一版生成的真实文件验证升级、坐标结果、主保存和放弃修改。</summary>
    private static void BuiltInUpgrade()
    {
        foreach (bool calibration in new[] { true, false })
        {
            string path = WriteLegacyFixture(calibration);
            byte[] before = File.ReadAllBytes(path);
            VpStation station = new VpStation("UpgradeRegression", VpStationKind.Custom) { AutoVisionRunDisplay = false };
            try
            {
                string error;
                Check(station.TryLoadTB(path, out error) && station.TB.Tools["AcquireImage"] is CogAcqFifoTool, "Loading silently upgraded a production algorithm.");
                Check(station.TryCreateTemplateTB(path, calibration, out error), error);
                CogToolBlock block = station.TB;
                CheckEditorConnections(block);
                Check(block.Tools["AcquireImage"] is CogToolBlock && ((CogPMAlignTool)block.Tools["LocatePattern"]).Pattern.Trained, "Upgrade lost the trained model.");
                Check(((CogCalibNPointToNPointTool)block.Tools["Calibration"]).Calibration.Calibrated && (double)block.Inputs["Exposure"].Value == 17, "Upgrade lost calibration or edited parameters.");
                Check(before.SequenceEqual(File.ReadAllBytes(path)), "Upgrade bypassed Main Save.");
                block.Run();
                double expectedX = calibration ? 110 : 2.1, expectedY = calibration ? 85 : 3.7;
                Check(block.RunStatus.Result == CogToolResultConstants.Accept && (bool)block.Outputs["Accepted"].Value, "Upgraded graph: " + block.RunStatus.Message);
                Check(Math.Abs((double)block.Outputs["x"].Value - expectedX) < 0.2 && Math.Abs((double)block.Outputs["y"].Value - expectedY) < 0.2, "Upgrade changed pixel/world coordinate behavior.");
                if (calibration)
                {
                    block.Inputs["RunRotation"].Value = true; block.Run();
                    Check(block.RunStatus.Result == CogToolResultConstants.Accept && (bool)block.Outputs["RotationValid"].Value && Math.Abs((double)block.Outputs["CenterX"].Value - 2) < 1e-8, "Upgrade lost rotation samples or connections.");
                    typeof(VpAlgorithmTemplates).GetMethod("Configure", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { block, "", 17.0 });
                    Check(!(bool)block.Inputs["RunRotation"].Value && !(bool)block.Inputs["UseCalibration"].Value, "Calibration capture retained rotation-only or world-coordinate mode.");
                }
                Check(station.TryCreateTemplateTB(path, calibration, out error) && ReferenceEquals(block, station.TB), "Reopening replaced the upgraded draft.");
                Check(station.TryDiscardTBChanges(out error) && station.TB.Tools["AcquireImage"] is CogAcqFifoTool && before.SequenceEqual(File.ReadAllBytes(path)), "Discard did not restore the saved V1 template.");
                Check(station.TryCreateTemplateTB(path, calibration, out error) && station.TrySaveTB(out error), error);
                using (CogToolBlock loaded = (CogToolBlock)CogSerializer.LoadObjectFromFile(path))
                {
                    CheckEditorConnections(loaded);
                    loaded.Run();
                    Check(loaded.RunStatus.Result == CogToolResultConstants.Accept && (bool)loaded.Outputs["Accepted"].Value && Math.Abs((double)loaded.Outputs["x"].Value - expectedX) < 0.2, "Saved graph connections or trained data were lost.");
                }
            }
            finally { VisionproInterface.VList.Remove(station); if (station.TB != null) station.TB.Dispose(); }
        }
    }

    /// <summary>自定义过脚本、端口或连线的范例保持原样，避免覆盖应用工程。</summary>
    private static void BuiltInUpgradeGuards()
    {
        Action<CogToolBlock>[] customize = {
            block => ChangeLegacyScript(block),
            block => ChangeLegacyScript((CogToolBlock)block.Tools["RotationCenter"]),
            block => block.Inputs.Add(new CogToolBlockTerminal("SiteParameter", 5.0, typeof(double))),
            block => block.Inputs["Exposure"].Name = "SiteExposure",
            block => block.Tools["LocatePattern"].DataBindings.Add("InputImage", block.Inputs, "Item[\"InputImage\"].Value"),
            block => ((CogToolBlock)block.Tools["RotationCenter"]).Outputs.Add(new CogToolBlockTerminal("SiteResult", 3.0, typeof(double)))
        };
        foreach (Action<CogToolBlock> change in customize)
        {
            string path = WriteLegacyFixture(true);
            using (CogToolBlock original = (CogToolBlock)CogSerializer.LoadObjectFromFile(path)) { change(original); CogSerializer.SaveObjectToFile(original, path); }
            VpStation station = new VpStation("UpgradeGuardRegression", VpStationKind.Custom);
            try
            {
                string error; byte[] before = File.ReadAllBytes(path);
                Check(station.TryLoadTB(path, out error), error); CogToolBlock current = station.TB;
                Check(station.TryCreateTemplateTB(path, true, out error) && ReferenceEquals(current, station.TB), "Upgrade replaced a customized template.");
                Check(before.SequenceEqual(File.ReadAllBytes(path)), "Upgrade wrote a customized template.");
            }
            finally { VisionproInterface.VList.Remove(station); if (station.TB != null) station.TB.Dispose(); }
        }
    }

    /// <summary>修改旧版脚本正文，模拟应用人员已有的自定义逻辑。</summary>
    private static void ChangeLegacyScript(CogToolBlock block)
    {
        var script = new Cognex.VisionPro.Implementation.Internal.CogScriptSupport(block.Script);
        script.Source = script.Source.Replace("return false;", "return true;"); block.Script = script;
    }

    /// <summary>展开不含设备或现场数据的旧版合成图像定位夹具。</summary>
    private static string WriteLegacyFixture(bool calibration)
    {
        string path = Path.Combine(TestDirectory, Guid.NewGuid().ToString("N") + ".vpp");
        string name = calibration ? "CalibrationV1" : "PositioningV1";
        using (Stream source = typeof(Tests).Assembly.GetManifestResourceStream("Fixtures." + name + ".vpp"))
        using (FileStream target = File.Create(path)) source.CopyTo(target);
        return path;
    }

    /// <summary>连接两端必须是当前编辑器可见的标准工具端口，保存重载后路径和 ID 仍然一致。</summary>
    private static void CheckEditorConnections(CogToolBlock block)
    {
        ICogTool[] visible = block.Tools.Cast<ICogTool>().Concat(new ICogTool[] { block.Inputs, block.Outputs }).ToArray();
        foreach (ICogTool target in visible)
        foreach (CogDataBinding binding in target.DataBindings)
        {
            Check(ReferenceEquals(binding.Destination, target) && visible.Contains(binding.Source as ICogTool), "A binding bypasses the editor's visible tools.");
            Check(CogToolTerminals.GetToolInputTerminals(target).Any(terminal => CogToolTerminals.GetTerminalPath(terminal) == binding.DestinationPath), "A connection destination is not an editor terminal.");
            Check(CogToolTerminals.GetToolOutputTerminals((ICogTool)binding.Source).Any(terminal => CogToolTerminals.GetTerminalPath(terminal) == binding.SourcePath), "A connection source is not an editor terminal.");
        }
    }

    /// <summary>直接调用既有新增入口，验证无需算法文件即可获得完整草稿且未提前落盘。</summary>
    private static void BuiltInCreationEntries()
    {
        string oldVision = SysPara.VisionFileDirectory, oldRecipe = SysPara.RecipeName;
        RunMode oldMode = SysPara.SystemMode;
        List<VisionproInterface> oldStations = VisionproInterface.VList.ToList();
        try
        {
            VisionproInterface.VList.Clear(); SysPara.SystemMode = RunMode.IDLE;
            SysPara.VisionFileDirectory = Path.Combine(TestDirectory, "builtin-entry"); SysPara.RecipeName = "BuiltInRecipe";
            using (VPForm form = new VPForm())
            using (TabControl tabs = new TabControl())
            using (Panel host = new Panel())
            using (TextBox exposure = new TextBox { Text = "10" })
            {
                VpCameraConfig camera = new VpCameraConfig { Name = "NewCamera", FolderName = "NewCamera" };
                VpConfigFile config = (VpConfigFile)typeof(VPForm).GetField("_config", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);
                config.Cameras.Add(camera);
                typeof(VPForm).GetMethod("AddVpp", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(form, new object[] { camera, tabs });
                Check(camera.Vpps.Count == 1 && VpAlgorithmTemplates.IsBuiltIn(camera.Vpps[0].Station.TB), "Add VPP still creates an empty algorithm.");
                Type entryType = typeof(VPForm).GetNestedType("CameraEntry", BindingFlags.NonPublic);
                object entry = Activator.CreateInstance(entryType, true);
                entryType.GetField("Key").SetValue(entry, camera.Name); entryType.GetField("Config").SetValue(entry, camera);
                entryType.GetField("CalibHost").SetValue(entry, host);
                MethodInfo add = typeof(VPForm).GetMethod("AddCalibration", BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { entryType, typeof(TextBox) }, null);
                add.Invoke(form, new[] { entry, exposure });
                Check(config.GetCalibration(camera.Name) != null && VpAlgorithmTemplates.IsBuiltIn(config.GetCalibration(camera.Name).Station.TB), "Add calibration still creates an empty algorithm.");
                Check(form.HasRecipeDraft && VisionproInterface.VList.Count == 2, "New algorithms did not join recipe draft tracking.");
                Check(!Directory.Exists(SysPara.VisionFileDirectory), "New algorithms were written before Main Save.");
                var pending = (System.Collections.IEnumerable)typeof(VPForm).GetMethod("PrepareConfigurationSave", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(form, null);
                Check(pending.Cast<object>().Count() == 3, "Main Save did not include both new VPP files and camera configuration.");
            }
        }
        finally
        {
            foreach (VisionproInterface station in VisionproInterface.VList) if (station.TB != null) station.TB.Dispose();
            VisionproInterface.VList.Clear(); VisionproInterface.VList.AddRange(oldStations);
            SysPara.VisionFileDirectory = oldVision; SysPara.RecipeName = oldRecipe; SysPara.SystemMode = oldMode;
        }
    }

    /// <summary>从新增 VPP 进入真实 TB 窗口，验证选择、取消、拒绝错误工厂和主保存回读。</summary>
    private static void PositioningTemplateEditor()
    {
        VpAlgorithmTemplates.RegisterPositioningTemplate("TestExtension", "扩展定位", "Extension", "Extensión",
            (sn, exposure) =>
            {
                CogToolBlock block = VpAlgorithmTemplates.Create(false, sn, exposure);
                block.Name = "ExtensionPositioning";
                return block;
            });
        VpAlgorithmTemplates.RegisterPositioningTemplate("TestInvalid", "无效模板", "Invalid", "No válida",
            (sn, exposure) => new CogToolBlock());
        string oldVision = SysPara.VisionFileDirectory, oldRecipe = SysPara.RecipeName;
        RunMode oldMode = SysPara.SystemMode;
        LanguageType oldLanguage = SysPara.LanguageShow;
        VpStation station = null;
        try
        {
            SysPara.SystemMode = RunMode.IDLE;
            SysPara.RecipeName = "TemplateRecipe";
            SysPara.VisionFileDirectory = Path.Combine(TestDirectory, "tb-editor");
            using (VPForm owner = new VPForm())
            using (TabControl tabs = new TabControl())
            {
                var camera = new VpCameraConfig { Name = "EditorCamera", FolderName = "EditorCamera" };
                var config = (VpConfigFile)typeof(VPForm).GetField("_config", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner);
                config.Cameras.Add(camera);
                typeof(VPForm).GetMethod("AddVpp", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(owner, new object[] { camera, tabs });
                VpVppConfig vpp = camera.Vpps.Single();
                station = vpp.Station;
                string path = station.GetVppPath(SysPara.RecipeName);
                WithPositioningEditor(owner, vpp, false, editor =>
                {
                    Control panel = editor.Controls.Find("pnlTemplates", true).Single();
                    ComboBox choices = (ComboBox)editor.Controls.Find("cmbTemplates", true).Single();
                    Control native = editor.Controls.Find("cogToolBlockEditV21", true).Single();
                    Check(panel.Visible && choices.Items.Count == 4 && panel.Bottom <= native.Top, "Template selector is missing or overlaps the editor.");
                    choices.SelectedIndex = 2;
                    var block = station.TB;
                    SetLanguage(LanguageType.English);
                    Check(choices.SelectedIndex == 2 && choices.Text == "Extension" && ReferenceEquals(station.TB, block), "Language refresh changed the template or ToolBlock.");
                    Check(InstallPositioningTemplate(editor, choices.Items[2]) && station.TB.Name == "ExtensionPositioning",
                        "The custom registered template could not be installed.");
                    block = station.TB;
                    Check(!InstallPositioningTemplate(editor, choices.Items[3]) && ReferenceEquals(block, station.TB), "An invalid factory replaced the active ToolBlock.");
                    Check(InstallPositioningTemplate(editor, choices.Items[1]), "The count template could not be installed.");
                    CheckPatternCount(station.TB);
                });
                Check(station.TB.Name != "PatternMatchCount" && !File.Exists(path), "Closing with discard did not restore the original draft.");
                WithPositioningEditor(owner, vpp, true, editor =>
                {
                    ComboBox choices = (ComboBox)editor.Controls.Find("cmbTemplates", true).Single();
                    Check(choices.Visible && InstallPositioningTemplate(editor, choices.Items[1]), "Discard did not restore template selection.");
                });
                Check(!File.Exists(path) && owner.HasRecipeDraft, "The editor bypassed Main Save.");
                CommitVisionDraft(owner);
                using (var saved = (CogToolBlock)CogSerializer.LoadObjectFromFile(path))
                {
                    Check(saved.Name == "PatternMatchCount", "Main Save lost the chosen template.");
                    CheckEditorConnections(saved);
                }
                WithPositioningEditor(owner, vpp, false, editor =>
                    Check(!editor.Controls.Find("pnlTemplates", true).Single().Visible, "A saved VPP can still be replaced with an editor template."));
            }
        }
        finally
        {
            if (station != null) { VisionproInterface.VList.Remove(station); station.TB.Dispose(); }
            SysPara.VisionFileDirectory = oldVision; SysPara.RecipeName = oldRecipe; SysPara.SystemMode = oldMode;
            SetLanguage(oldLanguage);
        }
    }

    /// <summary>在实际模态窗口打开时检查控件，显式执行与正常关闭相同的保留或恢复路径。</summary>
    private static void WithPositioningEditor(VPForm owner, VpVppConfig vpp, bool keepDraft, Action<TBEditForm> inspect)
    {
        Exception failure = null;
        bool opened = false;
        using (var timer = new Timer { Interval = 50 })
        {
            timer.Tick += delegate
            {
                var editor = Application.OpenForms.OfType<TBEditForm>().FirstOrDefault();
                if (editor == null) return;
                timer.Stop();
                opened = true;
                var closeMethod = typeof(TBEditForm).GetMethod("TBEditForm_FormClosing", BindingFlags.Instance | BindingFlags.NonPublic);
                editor.FormClosing -= (FormClosingEventHandler)Delegate.CreateDelegate(typeof(FormClosingEventHandler), editor, closeMethod);
                try
                {
                    inspect(editor);
                    typeof(TBEditForm).GetMethod("CompleteEdit", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(editor, new object[] { keepDraft });
                }
                catch (Exception error) { failure = error; }
                finally { editor.Close(); }
            };
            timer.Start();
            typeof(VPForm).GetMethod("EditStation", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(owner, new object[] { vpp });
        }
        if (failure != null) throw failure;
        Check(opened, "The Edit TB entry did not open the native editor.");
    }

    /// <summary>通过编辑器使用所选模板，跳过覆盖确认框以支持无人值守回归。</summary>
    private static bool InstallPositioningTemplate(TBEditForm editor, object template)
    {
        object[] arguments = { template, "" };
        return (bool)typeof(TBEditForm).GetMethod("InstallTemplate", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(editor, arguments);
    }

    /// <summary>在离线图像中训练一个目标并找到两个重复特征，检查与标准输出的真实连接。</summary>
    private static void CheckPatternCount(CogToolBlock block)
    {
        CheckEditorConnections(block);
        using (var bitmap = new Bitmap(350, 170))
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            graphics.Clear(Color.Black);
            foreach (int x in new[] { 45, 215 })
            {
                graphics.FillRectangle(Brushes.White, x, 45, 18, 72);
                graphics.FillRectangle(Brushes.White, x, 100, 70, 17);
                graphics.FillEllipse(Brushes.White, x + 55, 50, 20, 28);
            }
            var image = new CogImage8Grey(bitmap);
            var pattern = (CogPMAlignTool)block.Tools["LocatePattern"];
            var region = new CogRectangleAffine();
            region.SetCenterLengthsRotationSkew(85, 85, 105, 105, 0, 0);
            pattern.Pattern.TrainImage = image;
            pattern.Pattern.TrainRegion = region;
            pattern.Pattern.Origin.TranslationX = 85; pattern.Pattern.Origin.TranslationY = 85;
            pattern.Pattern.Train();
            block.Inputs["UseInputImage"].Value = true;
            block.Inputs["InputImage"].Value = image;
            block.Run();
            Check((bool)block.Outputs["Accepted"].Value && (int)block.Outputs["MatchCount"].Value == 2,
                "The count template did not locate both targets: " + block.RunStatus.Message);
        }
    }

    /// <summary>脚本使用现场语言包覆盖，切换语言时不清除离线输入或采样数据。</summary>
    private static void BuiltInLanguage()
    {
        WithLanguageFiles(delegate
        {
            string path = Path.Combine(SysPara.LanguageDataDirectory, "English.xml");
            XElement document = XElement.Load(path);
            XElement form = document.Element("Vision");
            if (form == null) { form = new XElement("Vision"); document.Add(form); }
            XElement entry = form.Element("template_InputRequired");
            if (entry == null) { entry = new XElement("template_InputRequired"); form.Add(entry); }
            entry.SetAttributeValue("ComponentText", "Site: please select a sample image."); document.Save(path);
            SetLanguage(LanguageType.English);
            using (CogToolBlock block = VpAlgorithmTemplates.Create(false, "", 10))
            {
                block.Inputs["UseInputImage"].Value = true; block.Run();
                Check(block.RunStatus.Message.Contains("Site: please select a sample image."), "Script ignored the site translation.");
                SetLanguage(LanguageType.Español);
                typeof(VpAlgorithmTemplates).GetMethod("Configure", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { block, "", 10.0 });
                block.Run();
                Check(block.RunStatus.Message.Contains("Proporcione"), "Script did not refresh its language.");
                Check((bool)block.Inputs["UseInputImage"].Value, "Language refresh changed the edited input mode.");
            }
        });
    }

    /// <summary>预留判据可保留零范围，当前生产行仍严格校验，非法数值不进入快照。</summary>
    private static void LegacyReservedCriteria()
    {
        List<VisionproInterface> previous = VisionproInterface.VList.ToList();
        try
        {
            using (StationVisionForm form = new StationVisionForm())
            {
                MethodInfo validate = typeof(StationVisionForm).GetMethod("ValidateRecipeCriteria", BindingFlags.Static | BindingFlags.NonPublic);
                System.Data.DataTable table = form.RecipeData.Tables["tb_H1_Visdata"];
                for (int index = 0; index < 6; index++)
                {
                    System.Data.DataRow row = table.NewRow();
                    row["Enable"] = true; row["X"] = 100.0 + index; row["Y"] = 200.0 + index; row["U"] = 10.0 + index;
                    foreach (string axis in new[] { "X", "Y", "U" }) { row[axis + "_Low"] = index < 2 ? -5.0 : 0.0; row[axis + "_Hi"] = index < 2 ? 5.0 : 0.0; }
                    table.Rows.Add(row);
                }
                string before = form.RecipeData.GetXml(); string error;
                form.ConfigurationData.ValidateRecipe(form.RecipeData);
                Check(form.TryLoadVisionCriteria(out error), error);
                Check(form.dgv_H1_VisionData_List.Count == 6 && form.dgv_H1_VisionData_List[2].Enable && form.dgv_H1_VisionData_List[2].X == 102 && form.dgv_H1_VisionData_List[2].X_Low == 0 && form.dgv_H1_VisionData_List[2].X_Hi == 0, "Reserved criterion values changed.");
                StationVisionForm.VisionPostData active;
                Check(form.TryGetProductionVisionData(out active, out error) && active.Point == 0 && active.X_Low == -5 && active.X_Hi == 5, "Production no longer uses the validated first criterion.");
                Check(before == form.RecipeData.GetXml(), "Validation rewrote the recipe.");
                table.Rows[0]["X_Low"] = 5.0;
                ExpectFailure(() => validate.Invoke(null, new object[] { form.RecipeData }));
                Check(!form.TryLoadVisionCriteria(out error) && form.dgv_H1_VisionData_List.Count == 0, "Invalid production limits retained a usable snapshot.");
                table.Rows[0]["X_Low"] = -5.0; table.Rows[2]["Y"] = double.NaN;
                ExpectFailure(() => validate.Invoke(null, new object[] { form.RecipeData }));
                table.Rows[2]["Enable"] = false;
                form.ConfigurationData.ValidateRecipe(form.RecipeData);
                Check(form.TryLoadVisionCriteria(out error) && !form.dgv_H1_VisionData_List[2].Enable, "Disabled historical rows were rejected.");
            }
        }
        finally
        {
            foreach (VisionproInterface station in VisionproInterface.VList.Except(previous).ToArray()) { if (station.TB != null) station.TB.Dispose(); VisionproInterface.VList.Remove(station); }
        }
    }
}
