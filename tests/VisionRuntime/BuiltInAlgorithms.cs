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
        Run("camera add calibration and add VPP create main-save drafts", BuiltInCreationEntries);
        Run("built-in script messages honor site translations and preserve image inputs", BuiltInLanguage);
    }

    /// <summary>标定包含原始像素定位、九点标定和独立旋转算法；生产 VPP 包含定位和标定工具。</summary>
    private static void BuiltInCalibration()
    {
        using (CogToolBlock calibration = VpAlgorithmTemplates.Create(true, "", 12))
        using (CogToolBlock positioning = VpAlgorithmTemplates.Create(false, "", 15))
        {
            Check(calibration.Tools["AcquireImage"] is CogAcqFifoTool, "Acquisition tool is missing.");
            Check(calibration.Tools["LocatePattern"] is CogPMAlignTool, "Calibration feature locator is missing.");
            Check(calibration.Tools["Calibration"] is CogCalibNPointToNPointTool, "Nine-point tool is missing.");
            Check(calibration.Tools["RotationCenter"] is CogToolBlock, "Rotation center algorithm is missing.");
            Check(VpAlgorithmTemplates.IsBuiltIn(positioning) && positioning.Tools.Count == 3, "Standard positioning tools are missing.");
            Check((string)positioning.Inputs["CameraSN"].Value == "" && (double)positioning.Inputs["Exposure"].Value == 15, "Camera defaults changed.");
            Check(!(bool)positioning.Inputs["UseInputImage"].Value && double.IsNaN((double)positioning.Outputs["x"].Value), "The template invented a valid production result.");
        }
    }

    /// <summary>验证带旋转角的仿射轨迹拟合，以及错误输入清除上次中心。</summary>
    private static void BuiltInRotation()
    {
        using (CogToolBlock calibration = VpAlgorithmTemplates.Create(true, "", 10))
        {
            CogToolBlock rotation = (CogToolBlock)calibration.Tools["RotationCenter"];
            double[] angles = { -90, -45, 0, 40, 95, 150 };
            double[] x = angles.Select(a => 1234 + 82 * Math.Cos(a * Math.PI / 180) + 17 * Math.Sin(a * Math.PI / 180)).ToArray();
            double[] y = angles.Select(a => -567 + 12 * Math.Cos(a * Math.PI / 180) - 63 * Math.Sin(a * Math.PI / 180)).ToArray();
            SetRotationSamples(rotation, x, y, angles);
            rotation.Run();
            Check(rotation.RunStatus.Result == CogToolResultConstants.Accept, rotation.RunStatus.Message);
            Check(Math.Abs((double)rotation.Outputs["CenterX"].Value - 1234) < 1e-7 && Math.Abs((double)rotation.Outputs["CenterY"].Value + 567) < 1e-7, "Affine rotation center is incorrect.");
            Check((double)rotation.Outputs["RmsError"].Value < 1e-7, "Exact sample residual is not zero.");
            foreach (double[] invalid in new[] { new double[] { 0, 0, 0, 0, 0, 0 }, new double[] { 0, 360, 720, 1080, 1440, 1800 }, new double[] { 0, 10, double.NaN, 20, 30, 40 }, new double[] { 0, 1 } })
            {
                SetRotationSamples(rotation, x, y, invalid); rotation.Run();
                Check(rotation.RunStatus.Result == CogToolResultConstants.Error, "Degenerate samples were accepted.");
                Check(double.IsNaN((double)rotation.Outputs["CenterX"].Value), "A stale center survived invalid samples.");
            }
        }
    }

    /// <summary>给独立旋转算法提供同一顺序的像素及实际轴角度。</summary>
    private static void SetRotationSamples(CogToolBlock rotation, double[] x, double[] y, double[] angles)
    {
        rotation.Inputs["PixelX"].Value = x; rotation.Inputs["PixelY"].Value = y; rotation.Inputs["AnglesDegrees"].Value = angles;
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
            Check(flowMethod.Inputs.Any(field => field.Name == "TB:UserThreshold") && !flowMethod.Inputs.Any(field => field.Name == "TB:InputImage" || field.Name == "TB:CameraSN" || field.Name == "TB:UseInputImage" || field.Name == "TB:Language" || field.Name == "TB:Exposure"), "Flow binding exposed internal capture ports or hid user inputs.");
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
                Check(VpAlgorithmTemplates.IsBuiltIn(station.TB) && station.TB.Tools.Count == (calibration ? 4 : 3), "The loaded empty placeholder remained empty.");
                Check(before.SequenceEqual(File.ReadAllBytes(path)), "Filling an empty placeholder bypassed Main Save.");
                CogToolBlock draft = station.TB; draft.Inputs["Exposure"].Value = 35.0;
                Check(station.TryCreateTemplateTB(path, calibration, out error) && ReferenceEquals(draft, station.TB) && (double)station.TB.Inputs["Exposure"].Value == 35, "Reopening replaced the in-memory draft.");
                Check(station.TryDiscardTBChanges(out error) && station.TB.Tools.Count == 0 && !VpAlgorithmTemplates.IsBuiltIn(station.TB), "Discard did not restore the saved empty placeholder.");
                Check(station.TryCreateTemplateTB(path, calibration, out error) && station.TrySaveTB(out error), error);
                using (CogToolBlock loaded = (CogToolBlock)CogSerializer.LoadObjectFromFile(path))
                    Check(VpAlgorithmTemplates.IsBuiltIn(loaded) && loaded.Tools.Count == (calibration ? 4 : 3), "Saved templates did not survive reload.");
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
            CogToolBlock rotation = (CogToolBlock)block.Tools["RotationCenter"];
            SetRotationSamples(rotation, new double[] { 12, 2, -8, 2 }, new double[] { 3, 13, 3, -7 }, new double[] { 0, 90, 180, 270 });
            string path = Path.Combine(TestDirectory, "builtin-roundtrip.vpp"); CogSerializer.SaveObjectToFile(block, path);
            using (CogToolBlock loaded = (CogToolBlock)CogSerializer.LoadObjectFromFile(path))
            using (CogToolBlock copy = (CogToolBlock)CogSerializer.DeepCopyObject(loaded))
            {
                Check(VpAlgorithmTemplates.IsBuiltIn(loaded), "Template identity was lost.");
                CogToolBlock copiedRotation = (CogToolBlock)copy.Tools["RotationCenter"]; copiedRotation.Run();
                Check(copiedRotation.RunStatus.Result == CogToolResultConstants.Accept && Math.Abs((double)copiedRotation.Outputs["CenterX"].Value - 2) < 1e-8, "Serialized script did not execute.");
                ((double[])copiedRotation.Inputs["PixelX"].Value)[0] = 999;
                Check(((double[])((CogToolBlock)loaded.Tools["RotationCenter"]).Inputs["PixelX"].Value)[0] == 12, "Save snapshots share mutable sample arrays.");
            }
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
}
