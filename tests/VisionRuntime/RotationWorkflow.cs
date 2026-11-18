using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using AlphaRap;
using Cognex.VisionPro;
using Cognex.VisionPro.CalibFix;
using Cognex.VisionPro.Caliper;
using Cognex.VisionPro.ToolBlock;
using Cognex.VisionPro.Implementation.Internal;

internal static partial class Tests
{
    /// <summary>覆盖九点前置条件、上下文失效、拍照角度配对、保存回读和窗口草稿。</summary>
    private static void RunRotationWorkflowTests()
    {
        Run("rotation tool rejects absent or three-point calibration and maps a nine-point center", RotationModelPrerequisite);
        Run("nine-point-only save remains valid and rotation uses the applied draft", RotationWorkflowStates);
        Run("camera-only nine-point calibration does not require a production VPP or rotation", RotationWithoutTarget);
        Run("rotation samples and result survive save reload and invalidate on edits", RotationPersistence);
        Run("rotation samples pair actual angles with a single fresh capture", RotationCapturePair);
        Run("stock V3 calibration gains the nine-point rotation guard without replacing custom scripts", RotationV3Upgrade);
        Run("rotation step gating and dialog language switching preserve editable angles and cancel", RotationWorkflowUi);
    }

    /// <summary>为独立旋转算法建立明确的九点变换，不连接相机。</summary>
    private static void CalibrateRotationModel(CogToolBlock block, int count = 9)
    {
        CogCalibNPointToNPoint model = ((CogCalibNPointToNPointTool)block.Tools["Calibration"]).Calibration;
        model.NumPoints = count;
        for (int index = 0; index < count; index++)
        {
            double x = count == 3 ? (index == 1 ? 180 : 20) : 20 + index % 3 * 80;
            double y = count == 3 ? (index == 2 ? 140 : 20) : 20 + index / 3 * 60;
            model.SetUncalibratedPointX(index, x); model.SetUncalibratedPointY(index, y);
            model.SetRawCalibratedPointX(index, 1 + 0.01 * x); model.SetRawCalibratedPointY(index, 2 + 0.02 * y);
        }
        model.Calibrate();
    }

    /// <summary>绕过页面直接运行 TB 时同样需要至少九点模型，不能沿用旧中心。</summary>
    private static void RotationModelPrerequisite()
    {
        using (CogToolBlock block = VpAlgorithmTemplates.Create(true, "", 10))
        {
            SetRotationSamples(block, new double[] { 140, 120, 100, 120 }, new double[] { 90, 105, 90, 75 }, new double[] { 0, 90, 180, 270 });
            block.Inputs["RunRotation"].Value = true; block.Run();
            Check(block.RunStatus.Result == CogToolResultConstants.Error && !(bool)block.Outputs["RotationValid"].Value && double.IsNaN((double)block.Outputs["CenterX"].Value), "Rotation ran without nine-point calibration.");
            CalibrateRotationModel(block, 3); block.Run();
            Check(block.RunStatus.Result == CogToolResultConstants.Error && !(bool)block.Outputs["RotationValid"].Value, "Three points satisfied the nine-point prerequisite.");
            CalibrateRotationModel(block); block.Run();
            Check(block.RunStatus.Result == CogToolResultConstants.Accept && (bool)block.Outputs["RotationValid"].Value, block.RunStatus.Message);
            Check(Math.Abs((double)block.Outputs["MachineCenterX"].Value - 2.2) < 1e-8 && Math.Abs((double)block.Outputs["MachineCenterY"].Value - 3.8) < 1e-8, "Rotation center did not use the nine-point transform.");
            CheckEditorConnections(block);
        }
    }

    /// <summary>构建真实标准算法及对应相机配置，应用过程沿用正式标定接口。</summary>
    private sealed class RotationFixture : IDisposable
    {
        internal readonly string PreviousRecipe = SysPara.RecipeName, PreviousVision = SysPara.VisionFileDirectory, PreviousSettings = SysPara.SettingDataDirectory;
        internal readonly RunMode PreviousMode = SysPara.SystemMode;
        internal readonly VpCameraConfig Camera = new VpCameraConfig { Name = "WorkflowCamera", FolderName = "WorkflowCamera", CameraSN = "WorkflowSN" };
        internal readonly VpCalibration Calibration = new VpCalibration();
        internal readonly VpStation Target, Station;
        internal readonly List<VisionproInterface> PreviousStations = VisionproInterface.VList.ToList();

        /// <summary>隔离当前配方和文件目录，建立标定及目标 VPP 草稿。</summary>
        internal RotationFixture()
        {
            SysPara.SystemMode = RunMode.IDLE; SysPara.RecipeName = "RotationRecipe"; SysPara.VisionFileDirectory = Path.Combine(TestDirectory, Guid.NewGuid().ToString("N"));
            SysPara.SettingDataDirectory = Path.Combine(TestDirectory, Guid.NewGuid().ToString("N")); VisionproInterface.VList.Clear();
            Station = new VpStation(Camera.Name, "Calibration", VpStationKind.Calibration) { AutoVisionRunDisplay = false, RunLiveCCDSN = Camera.CameraSN };
            Target = new VpStation(Camera.Name, "Position", VpStationKind.Custom) { AutoVisionRunDisplay = false, RunLiveCCDSN = Camera.CameraSN };
            Station.SetOwner(Camera.Name, "Calibration", Path.Combine(Camera.StorageFolder, "Calibration"));
            Target.SetOwner(Camera.Name, "Position", Path.Combine(Camera.StorageFolder, "Position"));
            string error;
            Check(Station.TryCreateTemplateTB(Station.GetVppPath(SysPara.RecipeName), true, out error), error);
            Check(Target.TryCreateTemplateTB(Target.GetVppPath(SysPara.RecipeName), false, out error), error);
            Camera.Vpps.Add(new VpVppConfig { Name = "Position", FolderName = "Position", Station = Target }); Calibration.Station = Station;
            for (int index = 0; index < 9; index++) { double x = 20 + index % 3 * 80, y = 20 + index / 3 * 60; Calibration.Points.Add(new VpCalibPoint(x, y, 1 + 0.01 * x, 2 + 0.02 * y)); }
        }

        /// <summary>在标定和目标工位上应用九点，记录待 Main Save 的同次版本。</summary>
        internal void ApplyNine()
        {
            var points = Calibration.Points.Select(point => new CalibrationData { PixelX = point.PixelX, PixelY = point.PixelY, MotorPosX = point.MotorPosX, MotorPosY = point.MotorPosY }).ToList();
            foreach (VpStation station in new[] { Station, Target })
            {
                object[] arguments = { points, "Calibration", null };
                Check((bool)typeof(VisionproInterface).GetMethod("SetCalibrationDraft", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(station, arguments), "Nine-point application failed.");
            }
            typeof(VpCalibration).GetMethod("MarkAppliedDraft", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Calibration, new object[] { Camera, SysPara.RecipeName });
        }

        /// <summary>提供同一特征绕像素中心 (120,90) 旋转的明确角度采样。</summary>
        internal List<VpRotationSample> Samples()
        {
            return new[] { -90.0, -30, 30, 90, 150 }.Select(angle => new VpRotationSample(120 + 20 * Math.Cos(angle * Math.PI / 180), 90 + 15 * Math.Sin(angle * Math.PI / 180), angle)).ToList();
        }

        /// <summary>恢复全局配置并释放本夹具创建的算法。</summary>
        public void Dispose()
        {
            foreach (VisionproInterface station in VisionproInterface.VList.Except(PreviousStations).ToArray()) { if (station.TB != null) station.TB.Dispose(); VisionproInterface.VList.Remove(station); }
            VisionproInterface.VList.Clear(); VisionproInterface.VList.AddRange(PreviousStations); SysPara.SettingDataDirectory = PreviousSettings;
            SysPara.RecipeName = PreviousRecipe; SysPara.VisionFileDirectory = PreviousVision; SysPara.SystemMode = PreviousMode;
        }
    }

    /// <summary>九点本身可完成保存；旋转可紧接有效应用草稿进行，并严格检查相机、配方和逐点数据。</summary>
    private static void RotationWorkflowStates()
    {
        using (RotationFixture fixture = new RotationFixture())
        {
            string error; VpRotationResult result;
            Check(!fixture.Calibration.CanStartRotation(fixture.Camera, SysPara.RecipeName, out error), "Unapplied points enabled rotation.");
            fixture.ApplyNine();
            Check(fixture.Calibration.CanStartRotation(fixture.Camera, SysPara.RecipeName, out error), error);
            Check(fixture.Calibration.GetRotationSamples().Count == 0 && fixture.Station.TrySaveTB(out error) && fixture.Target.TrySaveTB(out error), "Nine-point-only save required rotation.");
            fixture.Calibration.MarkApplied(fixture.Camera, SysPara.RecipeName);
            Check(fixture.Calibration.TrySetRotationSamples(fixture.Camera, SysPara.RecipeName, fixture.Samples(), out error), error);
            Check(fixture.Calibration.TryCalculateRotation(fixture.Camera, SysPara.RecipeName, out result, out error), error);
            Check(Math.Abs(result.MachineCenterX - 2.2) < 1e-8 && Math.Abs(result.MachineCenterY - 3.8) < 1e-8, "Machine center is incorrect.");
            fixture.Camera.CameraSN = "OtherSN"; Check(!fixture.Calibration.CanStartRotation(fixture.Camera, SysPara.RecipeName, out error), "Camera change retained rotation permission."); fixture.Camera.CameraSN = "WorkflowSN";
            Check(!fixture.Calibration.CanStartRotation(fixture.Camera, "OtherRecipe", out error), "Another recipe reused calibration.");
            fixture.Calibration.Points[0].MotorPosX += 0.00001;
            Check(!fixture.Calibration.CanStartRotation(fixture.Camera, SysPara.RecipeName, out error), "A sub-display-precision point edit bypassed the prerequisite.");
            fixture.ApplyNine();
            Check(fixture.Calibration.CanStartRotation(fixture.Camera, SysPara.RecipeName, out error) && !fixture.Calibration.RotationSamplesMatch(fixture.Camera, SysPara.RecipeName), "Reapplying nine points reused old rotation samples.");
            Check(!fixture.Calibration.TryCalculateRotation(fixture.Camera, SysPara.RecipeName, out result, out error), "Stale samples were calculated.");
        }
    }

    /// <summary>保存重载保留旋转上下文，算法编辑和目标修改使旧结果失效，放弃可恢复原结果。</summary>
    private static void RotationPersistence()
    {
        using (RotationFixture fixture = new RotationFixture())
        {
            string error; VpRotationResult result; fixture.ApplyNine();
            SaveRotationDraft(fixture);
            Check(fixture.Calibration.TrySetRotationSamples(fixture.Camera, SysPara.RecipeName, fixture.Samples(), out error) && fixture.Calibration.TryCalculateRotation(fixture.Camera, SysPara.RecipeName, out result, out error), error);
            SaveRotationDraft(fixture);
            Check(fixture.Station.TryLoadTB(fixture.Station.TBPath, out error), error);
            Check(fixture.Calibration.TryGetRotationResult(fixture.Camera, SysPara.RecipeName, out result) && result.RmsPixels < 1e-7, "Saved result lost its nine-point context.");
            typeof(VisionproInterface).GetMethod("MarkToolBlockDraft", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(fixture.Station, new object[] { false });
            Check(!fixture.Calibration.RotationSamplesMatch(fixture.Camera, SysPara.RecipeName) && !fixture.Calibration.TryGetRotationResult(fixture.Camera, SysPara.RecipeName, out result), "Algorithm edit retained old rotation validity.");
            Check(fixture.Station.TryDiscardTBChanges(out error) && fixture.Calibration.TryGetRotationResult(fixture.Camera, SysPara.RecipeName, out result), error);
            typeof(VisionproInterface).GetMethod("MarkToolBlockDraft", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(fixture.Target, new object[] { false });
            Check(!fixture.Calibration.CanStartRotation(fixture.Camera, SysPara.RecipeName, out error), "Target algorithm version change retained the nine-point prerequisite.");
        }
    }

    /// <summary>相机自身的九点模型可以独立发布，生产 VPP 和旋转都不是九点完成条件。</summary>
    private static void RotationWithoutTarget()
    {
        using (RotationFixture fixture = new RotationFixture())
        {
            fixture.Camera.Vpps.Clear(); fixture.Calibration.Points[0].PixelX += 0.0000123456789; fixture.ApplyNine(); string error;
            Check(fixture.Calibration.IsAppliedTo(fixture.Camera, SysPara.RecipeName) && fixture.Calibration.CanStartRotation(fixture.Camera, SysPara.RecipeName, out error), "Camera-only nine-point calibration could not finish.");
            VpConfigFile loaded = SaveRotationDraft(fixture);
            Check(fixture.Station.TryLoadTB(fixture.Station.TBPath, out error) && fixture.Calibration.IsAppliedTo(fixture.Camera, SysPara.RecipeName) && fixture.Calibration.GetRotationSamples().Count == 0, "Nine-only reload required rotation data.");
            loaded.GetCalibration(fixture.Camera.Name).Station = fixture.Station;
            Check(loaded.GetCalibration(fixture.Camera.Name).Points[0].PixelX == fixture.Calibration.Points[0].PixelX, "Main Save rounded calibration sample precision.");
            Check(loaded.GetCalibration(fixture.Camera.Name).CanStartRotation(loaded.Cameras[0], SysPara.RecipeName, out error), "Main Save did not retain the camera-only calibration context.");
        }
    }

    /// <summary>用真实主保存事务写入当前夹具的算法与配置，再读取发布记录。</summary>
    private static VpConfigFile SaveRotationDraft(RotationFixture fixture)
    {
        using (VPForm owner = new VPForm())
        {
            VpConfigFile config = (VpConfigFile)typeof(VPForm).GetField("_config", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner);
            config.Cameras.Add(fixture.Camera); config.Calibrations.Add(fixture.Camera.Name, fixture.Calibration);
            CommitVisionDraft(owner); string error; VpConfigFile loaded = VpConfigStore.Load(out error);
            Check(string.IsNullOrEmpty(error) && !owner.HasRecipeDraft, "Main Save did not accept the rotation workflow draft: " + error);
            return loaded;
        }
    }

    /// <summary>每次角度必须配对相同运行编号，错误编号及重复添加不发布采样。</summary>
    private static void RotationCapturePair()
    {
        using (RotationFixture fixture = new RotationFixture())
        {
            fixture.ApplyNine(); string error; long run;
            fixture.Station.TB.Inputs["UseInputImage"].Value = true; fixture.Station.TB.Inputs["InputImage"].Value = PrecisionImage(120.35, 90.65, true);
            ((CogFindCircleTool)fixture.Station.TB.Tools["FindCircle"]).RunParams.ExpectedCircularArc.SetCenterRadiusAngleStartAngleSpan(120, 90, 25.4, 0, 2 * Math.PI);
            Check(fixture.Station.TryRunTB(out run) && System.Threading.SpinWait.SpinUntil(() => !fixture.Station.IsBusy, 5000), "Calibration capture did not finish.");
            Check(fixture.Station.LastRunResult.State == VisionRunState.Succeeded, fixture.Station.LastRunResult.Error);
            Check(!fixture.Calibration.TryAddRotationSample(fixture.Camera, SysPara.RecipeName, run + 1, 45, out error), "A different capture identifier was accepted.");
            double x = fixture.Station.LastRunResult.X;
            Check(fixture.Calibration.TryAddRotationSample(fixture.Camera, SysPara.RecipeName, run, 45, out error), error);
            Check(fixture.Calibration.GetRotationSamples().Single().AngleDegrees == 45 && fixture.Calibration.GetRotationSamples().Single().PixelX == x, "Capture and actual angle were not paired.");
            Check(!fixture.Calibration.TryAddRotationSample(fixture.Camera, SysPara.RecipeName, run, 90, out error) && fixture.Calibration.GetRotationSamples().Count == 1, "An old image was paired with a new angle.");
        }
    }

    /// <summary>标准第三版升级只增加九点依赖和机械中心输出，用户旋转脚本保持原样。</summary>
    private static void RotationV3Upgrade()
    {
        string path = Path.Combine(TestDirectory, "calibration-v3.vpp");
        using (var source = typeof(Tests).Assembly.GetManifestResourceStream("Fixtures.CalibrationV3.vpp"))
        using (var target = File.Create(path)) source.CopyTo(target);
        VpStation station = new VpStation("V3Upgrade", VpStationKind.Calibration);
        try
        {
            string error; byte[] before = File.ReadAllBytes(path);
            Check(station.TryCreateTemplateTB(path, true, out error), error);
            Check(Convert.ToString(station.TB.UserData["AlphaRap.BuiltInTemplate"]) == "Calibration/4" && station.TB.Outputs.Contains("MachineCenterX"), "V3 did not gain the rotation prerequisite.");
            Check(before.SequenceEqual(File.ReadAllBytes(path)) && ((double[])station.TB.Inputs["RotationPixelX"].Value)[0] == 12, "V3 upgrade overwrote saved data.");
            Check(station.TryDiscardTBChanges(out error), error); CogToolBlock original = station.TB;
            ChangeLegacyScript((CogToolBlock)original.Tools["RotationCenter"]);
            Check(station.TryCreateTemplateTB(path, true, out error) && ReferenceEquals(original, station.TB) && !station.TB.Outputs.Contains("MachineCenterX"), "A customized rotation script was replaced.");
            Check(station.TryLoadTB(path, out error), error); original = station.TB;
            CogToolBlock rotation = (CogToolBlock)original.Tools["RotationCenter"];
            rotation.DataBindings.Clear();
            foreach (string name in new[] { "PixelX", "PixelY", "AnglesDegrees", "Language" })
            {
                string sourceName = name == "Language" ? name : "Rotation" + (name == "PixelX" ? "PixelY" : name);
                string destination = CogToolTerminals.GetToolInputTerminals(rotation).First(terminal => CogToolTerminals.GetTerminalName(terminal) == name);
                string sourcePort = CogToolTerminals.GetToolOutputTerminals(original.Inputs).First(terminal => CogToolTerminals.GetTerminalName(terminal) == sourceName);
                rotation.DataBindings.Add(CogToolTerminals.GetTerminalPath(destination), original.Inputs, CogToolTerminals.GetTerminalPath(sourcePort));
            }
            Check(station.TryCreateTemplateTB(path, true, out error) && ReferenceEquals(original, station.TB), "A customized rotation connection was upgraded.");
        }
        finally { VisionproInterface.VList.Remove(station); if (station.TB != null) station.TB.Dispose(); }
    }

    /// <summary>步骤按钮依据九点状态开放；旋转窗口取消恢复快照，三语切换不覆盖角度编辑。</summary>
    private static void RotationWorkflowUi()
    {
        WithLanguageFiles(delegate
        {
            using (RotationFixture fixture = new RotationFixture())
            using (VPForm owner = new VPForm())
            using (VpCalibCard card = new VpCalibCard { Owner = owner, Calib = fixture.Calibration })
            {
                Type entryType = typeof(VPForm).GetNestedType("CameraEntry", BindingFlags.NonPublic); object entry = Activator.CreateInstance(entryType, true);
                entryType.GetField("Key").SetValue(entry, fixture.Camera.Name); entryType.GetField("Config").SetValue(entry, fixture.Camera); card.Tag = entry;
                var config = (VpConfigFile)typeof(VPForm).GetField("_config", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner);
                config.Cameras.Add(fixture.Camera); config.Calibrations.Add(fixture.Camera.Name, fixture.Calibration);
                card.Bind(); Button step = card.Controls.Find("vpStep_Rotation", true).OfType<Button>().Single();
                Check(!step.Enabled, "Rotation step was enabled before applying nine points.");
                fixture.ApplyNine(); card.RefreshTexts(); Check(step.Enabled, "Valid applied draft did not open the optional rotation step.");
                string error; Check(fixture.Calibration.TrySetRotationSamples(fixture.Camera, SysPara.RecipeName, fixture.Samples(), out error), error);
                using (VpRotationForm form = new VpRotationForm(fixture.Calibration, fixture.Camera, SysPara.RecipeName))
                {
                    form.StartPosition = FormStartPosition.Manual; form.Location = new Point(-2000, -2000); form.Show(); Application.DoEvents();
                    TextBox angle = form.Controls.Find("tbAngle", true).OfType<TextBox>().Single(); angle.Text = "37.25";
                    DataGridView grid = form.Controls.Find("gridSamples", true).OfType<DataGridView>().Single(); grid.CurrentCell = grid.Rows[0].Cells[2]; grid.BeginEdit(false); ((TextBox)grid.EditingControl).Text = "-83.125";
                    foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
                    {
                        SetLanguage(language); Check(angle.Text == "37.25" && ((TextBox)grid.EditingControl).Text == "-83.125", "Language refresh lost an edited R angle.");
                        card.RefreshTexts(); card.PerformLayout();
                        using (Bitmap bitmap = new Bitmap(card.Width, card.Height)) { card.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size)); bitmap.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "calibration-steps-" + language + ".png")); }
                        Check(form.Controls.Find("btnDone", true).OfType<Button>().Single().Text == new[] { "完成", "Done", "Finalizar" }[(int)language], "Rotation actions did not switch language.");
                        form.Refresh(); Application.DoEvents(); using (Bitmap bitmap = new Bitmap(form.Width, form.Height)) { form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size)); bitmap.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "rotation-workflow-" + language + ".png")); }
                    }
                    grid.CancelEdit();
                    fixture.Station.TB.Inputs["UseInputImage"].Value = true;
                    fixture.Station.TB.Inputs["InputImage"].Value = PrecisionImage(120.35, 90.65, true);
                    ((CogFindCircleTool)fixture.Station.TB.Tools["FindCircle"]).RunParams.ExpectedCircularArc.SetCenterRadiusAngleStartAngleSpan(120, 90, 25.4, 0, 2 * Math.PI);
                    Button collect = form.Controls.Find("rotationCollectAngle", true).OfType<Button>().Single();
                    collect.PerformClick();
                    Check(System.Threading.SpinWait.SpinUntil(() => { Application.DoEvents(); return fixture.Calibration.GetRotationSamples().Count == 6; }, 5000), "Rotation button did not collect the new angle in one action.");
                    Check(fixture.Calibration.GetRotationSamples().Last().AngleDegrees == 37.25 && angle.Text == "", "Rotation did not pair the actual angle or clear the successful input.");
                    form.DialogResult = DialogResult.Cancel; form.Close();
                }
                Check(fixture.Calibration.GetRotationSamples().Count == 5, "Cancelling rotation did not restore the prior draft.");
                fixture.Calibration.Points.RemoveAt(0); card.RefreshTexts(); Check(!step.Enabled, "Invalidated nine points left rotation enabled.");
            }
        });
    }
}
