using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using AlphaRap;
using Cognex.VisionPro;
using Cognex.VisionPro.Caliper;
using Cognex.VisionPro.Dimensioning;
using Cognex.VisionPro.PMAlign;
using Cognex.VisionPro.ToolBlock;

internal static partial class Tests
{
    /// <summary>验证真实边缘拟合、粗定位跟随、质量拒绝和采点设置的保存及界面集成。</summary>
    private static void RunPrecisionCalibrationTests()
    {
        Run("calibration fits circle centers without a trained PM pattern", PrecisionCircle);
        Run("calibration computes the intersection of two fitted image edges", PrecisionLines);
        Run("optional PM fixture follows regions but preserves fine circle coordinates", PrecisionCoarseFixture);
        Run("V2 migration preserves training samples links and custom content", PrecisionV2Upgrade);
        Run("sampling modes are recipe drafts with reload discard and busy guards", PrecisionDraftSettings);
        Run("calibration card preserves selections and edited values in three languages", PrecisionCardLanguages);
    }

    /// <summary>生成灰度边缘的合成图像，参数使用像素中心坐标；超采样保留分数像素变化。</summary>
    private static CogImage8Grey PrecisionImage(double x, double y, bool circle, bool marker = false)
    {
        using (Bitmap large = new Bitmap(1920, 1440))
        using (Bitmap small = new Bitmap(240, 180))
        {
            using (Graphics graphics = Graphics.FromImage(large))
            {
                graphics.Clear(Color.Black);
                if (circle) graphics.FillEllipse(Brushes.White, (float)((x - 25.4) * 8), (float)((y - 25.4) * 8), 50.8f * 8, 50.8f * 8);
                else graphics.FillRectangle(Brushes.White, (float)(x * 8), (float)(y * 8), 1920, 1440);
                if (marker) graphics.FillRectangle(Brushes.White, (float)((x + 33) * 8), (float)((y - 40) * 8), 64, 96);
            }
            using (Graphics graphics = Graphics.FromImage(small))
            {
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic; graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.DrawImage(large, new Rectangle(0, 0, 240, 180));
            }
            return new CogImage8Grey(small);
        }
    }

    /// <summary>使用实际找圆工具验证分数像素圆心和失败清除，质量门限不能被 PM 结果绕过。</summary>
    private static void PrecisionCircle()
    {
        using (CogToolBlock block = VpAlgorithmTemplates.Create(true, "", 10))
        {
            Check((int)block.Inputs["FeatureMode"].Value == 0 && !((CogPMAlignTool)block.Tools["LocatePattern"]).Pattern.Trained, "New calibration does not default to circle fitting.");
            CogFindCircleTool circle = (CogFindCircleTool)block.Tools["FindCircle"];
            circle.RunParams.ExpectedCircularArc.SetCenterRadiusAngleStartAngleSpan(120, 90, 25, 0, 2 * Math.PI);
            block.Inputs["UseInputImage"].Value = true;
            foreach (double shift in new[] { 0.0, 0.27, 0.63 })
            {
                block.Inputs["InputImage"].Value = PrecisionImage(120.35 + shift, 90.65 - shift, true); block.Run();
                Check(block.RunStatus.Result == CogToolResultConstants.Accept && (bool)block.Outputs["Accepted"].Value, block.RunStatus.Message);
                Check(Math.Abs((double)block.Outputs["x"].Value - 120.35 - shift) < 0.2 && Math.Abs((double)block.Outputs["y"].Value - 90.65 + shift) < 0.2, "Circle center lost fractional pixel motion.");
                Check((double)block.Outputs["A"].Value == 0 && !(bool)block.Outputs["Calibrated"].Value && (double)block.Outputs["FitRms"].Value <= 0.5, "Circle sampling invented a direction or world coordinate.");
            }
            string path = Path.Combine(TestDirectory, "precision-circle-roundtrip.vpp"); CogSerializer.SaveObjectToFile(block, path);
            using (CogToolBlock loaded = (CogToolBlock)CogSerializer.LoadObjectFromFile(path))
            using (CogToolBlock copy = (CogToolBlock)CogSerializer.DeepCopyObject(loaded))
            {
                CheckEditorConnections(copy); copy.Run();
                Check(copy.RunStatus.Result == CogToolResultConstants.Accept && (bool)copy.Outputs["Accepted"].Value && Math.Abs((double)copy.Outputs["x"].Value - 120.98) < 0.2, "Serialized precision tools lost geometry or connections.");
            }
            block.Inputs["MaxFitRms"].Value = 1e-8; block.Run();
            Check(!(bool)block.Outputs["Accepted"].Value && double.IsNaN((double)block.Outputs["x"].Value), "Excessive fit RMS retained valid coordinates.");
            block.Inputs["MaxFitRms"].Value = 0.5; block.Inputs["MinCirclePoints"].Value = 100; block.Run();
            Check(!(bool)block.Outputs["Accepted"].Value, "Too few used circle points were accepted.");
            block.Inputs["MinCirclePoints"].Value = 12; block.Inputs["InputImage"].Value = new CogImage8Grey(240, 180); block.Run();
            Check(!(bool)block.Outputs["Accepted"].Value && double.IsNaN((double)block.Outputs["x"].Value), "Missing circle fell back to another result.");
            CheckEditorConnections(block);
        }
    }

    /// <summary>从两段图像边缘拟合交点，并拒绝平行线和超出边缘数量要求的结果。</summary>
    private static void PrecisionLines()
    {
        using (CogToolBlock block = VpAlgorithmTemplates.Create(true, "", 10))
        {
            block.Inputs["FeatureMode"].Value = 1; block.Inputs["UseInputImage"].Value = true;
            block.Inputs["InputImage"].Value = PrecisionImage(124.3, 92.6, false);
            CogFindLineTool first = (CogFindLineTool)block.Tools["FindLine1"], second = (CogFindLineTool)block.Tools["FindLine2"];
            first.RunParams.ExpectedLineSegment.SetStartEnd(145, 92, 210, 92);
            second.RunParams.ExpectedLineSegment.SetStartEnd(124, 112, 124, 155);
            block.Run();
            Check(block.RunStatus.Result == CogToolResultConstants.Accept && (bool)block.Outputs["Accepted"].Value, block.RunStatus.Message);
            Check(Math.Abs((double)block.Outputs["x"].Value - 124.3) < 0.2 && Math.Abs((double)block.Outputs["y"].Value - 92.6) < 0.2, "Fitted line intersection is incorrect: " + block.Outputs["x"].Value + ", " + block.Outputs["y"].Value);
            Check(((CogIntersectLineLineTool)block.Tools["LineIntersection"]).Intersects, "Native intersection tool was not used.");
            block.Inputs["MinLinePoints"].Value = 100; block.Run();
            Check(!(bool)block.Outputs["Accepted"].Value && double.IsNaN((double)block.Outputs["x"].Value), "Insufficient line support retained a point.");
            block.Inputs["MinLinePoints"].Value = 6; second.RunParams.ExpectedLineSegment.SetStartEnd(145, 92, 210, 92); block.Run();
            Check(!(bool)block.Outputs["Accepted"].Value && double.IsNaN((double)block.Outputs["x"].Value), "Parallel lines retained a valid point.");
            CheckEditorConnections(block);
        }
    }

    /// <summary>PM 仅改变测量区域，最终中心来自图像边缘，粗定位或精定位失败均不得发布采样。</summary>
    private static void PrecisionCoarseFixture()
    {
        using (CogToolBlock block = VpAlgorithmTemplates.Create(true, "", 10))
        {
            CogImage8Grey train = PrecisionImage(90.35, 90.65, true, true);
            CogPMAlignTool pattern = (CogPMAlignTool)block.Tools["LocatePattern"];
            CogRectangleAffine region = new CogRectangleAffine(); region.SetCenterLengthsRotationSkew(95, 85, 110, 120, 0, 0);
            pattern.Pattern.TrainImage = train; pattern.Pattern.TrainRegion = region;
            pattern.Pattern.Origin.TranslationX = 82; pattern.Pattern.Origin.TranslationY = 102; pattern.Pattern.Train();
            CogFindCircleTool circle = (CogFindCircleTool)block.Tools["FindCircle"];
            circle.RunParams.ExpectedCircularArc.SetCenterRadiusAngleStartAngleSpan(8.35, -11.35, 25.4, 0, 2 * Math.PI);
            block.Inputs["UsePatternFixture"].Value = true; block.Inputs["UseInputImage"].Value = true;
            block.Inputs["InputImage"].Value = PrecisionImage(150.7, 110.4, true, true); block.Run();
            Check(block.RunStatus.Result == CogToolResultConstants.Accept && (bool)block.Outputs["Accepted"].Value, block.RunStatus.Message);
            Check(Math.Abs((double)block.Outputs["x"].Value - 150.7) < 0.2 && Math.Abs((double)block.Outputs["y"].Value - 110.4) < 0.2, "Fixture did not return the fitted center to raw pixels.");
            Check(Math.Abs((double)block.Outputs["x"].Value - pattern.Results[0].GetPose().TranslationX) > 5, "Sampling returned the PM origin instead of the fitted circle center.");
            block.Inputs["MinCirclePoints"].Value = 100; block.Run();
            Check(pattern.Results.Count > 0 && !(bool)block.Outputs["Accepted"].Value && double.IsNaN((double)block.Outputs["x"].Value), "Fine fitting failure fell back to a successful PM match.");
            block.Inputs["MinCirclePoints"].Value = 12; block.Inputs["InputImage"].Value = new CogImage8Grey(240, 180); block.Run();
            Check(!(bool)block.Outputs["Accepted"].Value, "Coarse location failure retained a fitted point.");
        }
    }

    /// <summary>用已发布 V2 文件验证连接升级保留训练和样本，用户修改过连接时不替换。</summary>
    private static void PrecisionV2Upgrade()
    {
        string path = Path.Combine(TestDirectory, "calibration-v2.vpp");
        using (Stream source = typeof(Tests).Assembly.GetManifestResourceStream("Fixtures.CalibrationV2.vpp"))
        using (FileStream target = File.Create(path)) source.CopyTo(target);
        byte[] before = File.ReadAllBytes(path);
        VpStation station = new VpStation("PrecisionUpgrade", VpStationKind.Calibration);
        try
        {
            string error; Check(station.TryCreateTemplateTB(path, true, out error), error);
            Check(station.TB.Tools.Contains("FindCircle") && (int)station.TB.Inputs["FeatureMode"].Value == 2 && ((CogPMAlignTool)station.TB.Tools["LocatePattern"]).Pattern.Trained, "V2 migration changed sampling behavior or training.");
            station.TB.Run(); Check((bool)station.TB.Outputs["Accepted"].Value, station.TB.RunStatus.Message);
            CogPMAlignTool pattern = (CogPMAlignTool)station.TB.Tools["LocatePattern"];
            pattern.Pattern.Origin.TranslationX = -20; pattern.Pattern.Train(); station.TB.Run();
            Check((bool)station.TB.Outputs["Accepted"].Value && (double)station.TB.Outputs["x"].Value < 0, "Compatibility mode rejected an existing virtual pattern origin outside the image.");
            Check(((double[])station.TB.Inputs["RotationPixelX"].Value)[0] == 12 && before.SequenceEqual(File.ReadAllBytes(path)), "V2 migration lost samples or bypassed Save.");
            Check(station.TryDiscardTBChanges(out error), error);
            CogToolBlock old = station.TB; old.Tools["LocatePattern"].DataBindings.Clear();
            Check(station.TryCreateTemplateTB(path, true, out error) && ReferenceEquals(old, station.TB) && !station.TB.Tools.Contains("FindCircle"), "Customized V2 connections were replaced.");
        }
        finally { VisionproInterface.VList.Remove(station); if (station.TB != null) station.TB.Dispose(); }
    }

    /// <summary>采点选项随当前算法草稿保存，放弃及忙碌保护遵循现有路径。</summary>
    private static void PrecisionDraftSettings()
    {
        VpStation station = new VpStation("PrecisionSettings", VpStationKind.Calibration);
        RunMode mode = SysPara.SystemMode;
        try
        {
            SysPara.SystemMode = RunMode.IDLE; string error; string path = Path.Combine(TestDirectory, "precision-options.vpp");
            Check(station.TryCreateTemplateTB(path, true, out error) && station.TrySaveTB(out error), error);
            Check(station.TrySetCalibrationFeature(VpCalibrationFeature.LineIntersection, true, out error), error);
            VpCalibrationFeature feature; bool rough;
            Check(station.TryGetCalibrationFeature(out feature, out rough) && feature == VpCalibrationFeature.LineIntersection && rough, "Feature settings did not change.");
            Check(station.TryDiscardTBChanges(out error) && station.TryGetCalibrationFeature(out feature, out rough) && feature == VpCalibrationFeature.Circle && !rough, "Discard did not restore sampling settings.");
            Check(station.TrySetCalibrationFeature(VpCalibrationFeature.LineIntersection, true, out error) && station.TrySaveTB(out error) && station.TryLoadTB(path, out error), error);
            Check(station.TryGetCalibrationFeature(out feature, out rough) && feature == VpCalibrationFeature.LineIntersection && rough, "Reload did not retain the sampling method.");
            SysPara.SystemMode = RunMode.RUN;
            Check(!station.TrySetCalibrationFeature(VpCalibrationFeature.Circle, false, out error) && station.TryGetCalibrationFeature(out feature, out rough) && feature == VpCalibrationFeature.LineIntersection, "Production allowed sampling changes.");
        }
        finally { SysPara.SystemMode = mode; VisionproInterface.VList.Remove(station); if (station.TB != null) station.TB.Dispose(); }
    }

    /// <summary>三语切换保留采点选择、曝光和机械坐标编辑，已有采样时阻止混用不同特征。</summary>
    private static void PrecisionCardLanguages()
    {
        WithLanguageFiles(delegate
        {
            VpStation station = new VpStation("PrecisionCard", VpStationKind.Calibration);
            try
            {
                string error; Check(station.TryCreateTemplateTB(Path.Combine(TestDirectory, "precision-card.vpp"), true, out error), error);
                using (VPForm owner = new VPForm())
                using (VpCalibCard card = new VpCalibCard { Owner = owner, Calib = new VpCalibration { Station = station } })
                using (Form preview = new Form { ClientSize = new Size(900, 320), ShowInTaskbar = false, StartPosition = FormStartPosition.Manual, Location = new Point(-2000, -2000) })
                {
                    Type entryType = typeof(VPForm).GetNestedType("CameraEntry", BindingFlags.NonPublic);
                    object cameraEntry = Activator.CreateInstance(entryType, true);
                    VpCameraConfig camera = new VpCameraConfig { Name = "PrecisionCamera", FolderName = "PrecisionCamera" };
                    entryType.GetField("Key").SetValue(cameraEntry, camera.Name); entryType.GetField("Config").SetValue(cameraEntry, camera);
                    card.Tag = cameraEntry;
                    VpConfigFile config = (VpConfigFile)typeof(VPForm).GetField("_config", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner);
                    config.Cameras.Add(camera); config.Calibrations.Add(camera.Name, card.Calib);
                    preview.Controls.Add(card); card.Dock = DockStyle.Fill; preview.Show(); Application.DoEvents();
                    card.Bind(); card.Calib.Points.Add(new VpCalibPoint(12, 34, 5, 6)); card.GridPoints.Rows.Add(12, 34, 5, 6);
                    card.ExposureBox.Text = "17.25"; card.GridPoints.CurrentCell = card.GridPoints.Rows[0].Cells[2]; card.GridPoints.BeginEdit(false);
                    ((TextBox)card.GridPoints.EditingControl).Text = "123.456";
                    foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
                    {
                        SetLanguage(language); card.RefreshTexts();
                        ComboBox choices = card.Controls.Find("vpCalibFeature_dyn", true).OfType<ComboBox>().Single();
                        Check(Convert.ToString(choices.Items[0]) == new[] { "圆心", "Circle center", "Centro de círculo" }[(int)language], "Sampling choices were not translated: " + choices.Items[0]);
                        Check(choices.SelectedIndex == 0 && card.ExposureBox.Text == "17.25" && ((TextBox)card.GridPoints.EditingControl).Text == "123.456", "Language changed sampling or editing data.");
                        card.Refresh(); preview.Refresh(); Application.DoEvents();
                        using (Bitmap bitmap = new Bitmap(900, 320)) { card.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size)); bitmap.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "precision-card-" + language + ".png")); }
                    }
                    object[] arguments = { card, null };
                    typeof(VpCalibCard).GetField("refreshingFeature", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(card, true);
                    card.Controls.Find("vpCalibFeature_dyn", true).OfType<ComboBox>().Single().SelectedIndex = 1;
                    typeof(VpCalibCard).GetField("refreshingFeature", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(card, false);
                    bool changed = (bool)typeof(VPForm).GetMethod("TryChangeCalibrationFeature", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(owner, arguments);
                    Check(!changed && card.Calib.Points.Count == 1 && (int)station.TB.Inputs["FeatureMode"].Value == 0, "Sampling mode changed while previous points existed.");
                }
            }
            finally { VisionproInterface.VList.Remove(station); if (station.TB != null) station.TB.Dispose(); }
        });
    }
}
