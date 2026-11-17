using System;
using System.Linq;
using Cognex.VisionPro;
using Cognex.VisionPro.CalibFix;
using Cognex.VisionPro.Caliper;
using Cognex.VisionPro.Dimensioning;
using Cognex.VisionPro.Implementation.Internal;
using Cognex.VisionPro.PMAlign;
using Cognex.VisionPro.ToolBlock;

namespace AlphaRap
{
    /// <summary>标定采样所用的特征定义；序号对应内置算法的 FeatureMode 输入。</summary>
    public enum VpCalibrationFeature { Circle = 0, LineIntersection = 1, Pattern = 2 }

    public static partial class VpAlgorithmTemplates
    {
        private const string CalibrationFixtureSpace = "CalibrationFeature";

        /// <summary>在标准模板中建立可见的粗定位、找圆、找线和交点连接，保留已有标定模型及旋转样本。</summary>
        private static void AddPrecisionCalibration(CogToolBlock block, VpCalibrationFeature mode)
        {
            block.Outputs.DataBindings.Clear();
            CogToolBlock previous = (CogToolBlock)block.Tools["PositionResult"];
            block.Tools.Remove(previous); previous.Dispose();
            AddInput(block, "FeatureMode", typeof(int), (int)mode);
            AddInput(block, "UsePatternFixture", typeof(bool), false);
            AddInput(block, "MaxFitRms", typeof(double), 0.5);
            AddInput(block, "MinCirclePoints", typeof(int), 12);
            AddInput(block, "MinLinePoints", typeof(int), 6);
            AddInput(block, "MinLineAngleDegrees", typeof(double), 10.0);
            AddOutput(block, "FitRms", typeof(double), double.NaN);
            CogToolBlock pose = CreateCoarsePose(); block.Tools.Insert(3, pose);
            CogFixtureTool fixture = new CogFixtureTool { Name = "FeatureFixture", FailOnInvalidDataBinding = true };
            fixture.RunParams.FixturedSpaceName = CalibrationFixtureSpace; block.Tools.Insert(4, fixture);
            CogFindCircleTool circle = CreateCalibrationCircle(); block.Tools.Insert(5, circle);
            CogFindLineTool first = CreateCalibrationLine("FindLine1", false); block.Tools.Insert(6, first);
            CogFindLineTool second = CreateCalibrationLine("FindLine2", true); block.Tools.Insert(7, second);
            CogIntersectLineLineTool cross = new CogIntersectLineLineTool { Name = "LineIntersection", FailOnInvalidDataBinding = true };
            cross.DataBindings.SyncOnInitialize = false; block.Tools.Insert(8, cross);
            CogToolBlock point = CreateCalibrationPoint(); block.Tools.Add(point);
            ConnectPrecisionGraph(block, pose, fixture, circle, first, second, cross, point);
            block.Inputs["UseCalibration"].Value = false;
            block.UserData[TemplateKey] = "Calibration/3";
            SetScript(block, "Calibration.txt");
        }

        /// <summary>为圆边缘建立足够数量的卡尺，实际圆位置、极性与搜索宽度由应用人员在编辑器调整。</summary>
        private static CogFindCircleTool CreateCalibrationCircle()
        {
            CogFindCircleTool tool = new CogFindCircleTool { Name = "FindCircle", FailOnInvalidDataBinding = true };
            tool.RunParams.NumCalipers = 32; tool.RunParams.NumToIgnore = 3;
            tool.RunParams.CaliperSearchLength = 40; tool.RunParams.CaliperProjectionLength = 5;
            tool.RunParams.CaliperRunParams.Edge0Polarity = CogCaliperPolarityConstants.DontCare;
            tool.RunParams.ExpectedCircularArc.SetCenterRadiusAngleStartAngleSpan(100, 100, 30, 0, 2 * Math.PI);
            tool.RunParams.ExpectedCircularArc.SelectedSpaceName = CalibrationFixtureSpace;
            ExposeTerminal(tool, "InputImage", true); ExposeTerminal(tool, "Results", false);
            return tool;
        }

        /// <summary>建立可独立调整搜索段和卡尺方向的拟合直线。</summary>
        private static CogFindLineTool CreateCalibrationLine(string name, bool vertical)
        {
            CogFindLineTool tool = new CogFindLineTool { Name = name, FailOnInvalidDataBinding = true };
            tool.RunParams.NumCalipers = 20; tool.RunParams.NumToIgnore = 2;
            tool.RunParams.CaliperSearchLength = 40; tool.RunParams.CaliperProjectionLength = 5;
            tool.RunParams.CaliperRunParams.Edge0Polarity = CogCaliperPolarityConstants.DontCare;
            tool.RunParams.ExpectedLineSegment.SetStartEnd(vertical ? 100 : 30, vertical ? 30 : 100, vertical ? 100 : 170, vertical ? 170 : 100);
            tool.RunParams.CaliperSearchDirection = Math.PI / 2;
            tool.RunParams.ExpectedLineSegment.SelectedSpaceName = CalibrationFixtureSpace;
            ExposeTerminal(tool, "InputImage", true); ExposeTerminal(tool, "Results", false);
            CogToolTerminals.SetToolOutputTerminals(tool, CogToolTerminals.GetToolOutputTerminals(tool).Concat(new[] { CogToolTerminals.FormatTerminal("Line", "Results.GetLine()") }).ToArray());
            return tool;
        }

        /// <summary>把可选 PM 位姿转换为仅含平移和旋转的夹具，保持精定位残差单位为像素。</summary>
        private static CogToolBlock CreateCoarsePose()
        {
            CogToolBlock block = new CogToolBlock { Name = "CoarsePose", FailOnInvalidDataBinding = true };
            try
            {
                AddInput(block, "Matches", typeof(CogPMAlignResults), null);
                AddInput(block, "UsePattern", typeof(bool), false);
                AddOutput(block, "Transform", typeof(ICogTransform2D), new CogTransform2DLinear());
                SetScript(block, "CoarsePose.txt"); return block;
            }
            catch { block.Dispose(); throw; }
        }

        /// <summary>声明精定位的统一像素输出；未运行分支的绑定可暂不可读，脚本只校验选中分支。</summary>
        private static CogToolBlock CreateCalibrationPoint()
        {
            CogToolBlock block = new CogToolBlock { Name = "CalibrationPoint", FailOnInvalidDataBinding = false };
            try
            {
                AddInput(block, "Mode", typeof(int), 0); AddInput(block, "Language", typeof(int), 0);
                AddInput(block, "Matches", typeof(CogPMAlignResults), null);
                AddInput(block, "CircleResults", typeof(CogFindCircleResults), null);
                AddInput(block, "Line1Results", typeof(CogFindLineResults), null); AddInput(block, "Line2Results", typeof(CogFindLineResults), null);
                AddInput(block, "IntersectionX", typeof(double), double.NaN); AddInput(block, "IntersectionY", typeof(double), double.NaN);
                AddInput(block, "Intersects", typeof(bool), false);
                AddInput(block, "Transform", typeof(ICogTransform2D), null); AddInput(block, "Image", typeof(ICogImage), null);
                AddInput(block, "MaxFitRms", typeof(double), 0.5); AddInput(block, "MinCirclePoints", typeof(int), 12);
                AddInput(block, "MinLinePoints", typeof(int), 6); AddInput(block, "MinLineAngleDegrees", typeof(double), 10.0);
                AddOutput(block, "Accepted", typeof(bool), false); AddOutput(block, "Calibrated", typeof(bool), false);
                foreach (string name in new[] { "x", "y", "A", "FitRms" }) AddOutput(block, name, typeof(double), double.NaN);
                AddOutput(block, "Score", typeof(double), 0.0); AddOutput(block, "MatchCount", typeof(int), 0);
                SetScript(block, "CalibrationPoint.txt"); return block;
            }
            catch { block.Dispose(); throw; }
        }

        /// <summary>使用标准端口路径连接全部采样工具，未经选择的分支不会代替当前模式的结果。</summary>
        private static void ConnectPrecisionGraph(CogToolBlock block, CogToolBlock pose, CogFixtureTool fixture, CogFindCircleTool circle,
            CogFindLineTool first, CogFindLineTool second, CogIntersectLineLineTool cross, CogToolBlock point)
        {
            ICogTool capture = block.Tools["AcquireImage"], pattern = block.Tools[PatternToolName];
            Connect(pose, "Matches", pattern, "Results"); Connect(pose, "UsePattern", block.Inputs, "UsePatternFixture");
            ExposeTerminal(fixture, "InputImage", true); ExposeTerminal(fixture, "OutputImage", false);
            CogToolTerminals.SetToolInputTerminals(fixture, CogToolTerminals.GetToolInputTerminals(fixture).Concat(new[] { CogToolTerminals.FormatTerminal("Transform", "RunParams.UnfixturedFromFixturedTransform") }).ToArray());
            Connect(fixture, "InputImage", capture, "OutputImage"); Connect(fixture, "Transform", pose, "Transform");
            foreach (ICogTool tool in new ICogTool[] { circle, first, second }) Connect(tool, "InputImage", fixture, "OutputImage");
            foreach (string name in new[] { "InputImage", "LineA", "LineB" }) ExposeTerminal(cross, name, true);
            foreach (string name in new[] { "X", "Y", "Intersects" }) ExposeTerminal(cross, name, false);
            Connect(cross, "InputImage", fixture, "OutputImage"); Connect(cross, "LineA", first, "Line"); Connect(cross, "LineB", second, "Line");
            Connect(point, "Mode", block.Inputs, "FeatureMode"); Connect(point, "Language", block.Inputs, "Language");
            Connect(point, "Matches", pattern, "Results"); Connect(point, "CircleResults", circle, "Results");
            Connect(point, "Line1Results", first, "Results"); Connect(point, "Line2Results", second, "Results");
            Connect(point, "IntersectionX", cross, "X"); Connect(point, "IntersectionY", cross, "Y"); Connect(point, "Intersects", cross, "Intersects");
            Connect(point, "Transform", pose, "Transform"); Connect(point, "Image", capture, "OutputImage");
            foreach (string name in new[] { "MaxFitRms", "MinCirclePoints", "MinLinePoints", "MinLineAngleDegrees" }) Connect(point, name, block.Inputs, name);
            foreach (CogToolBlockTerminal terminal in point.Outputs) Connect(block.Outputs, terminal.Name, point, terminal.Name);
            Connect(block.Outputs, "OutputImage", capture, "OutputImage");
            ICogTool rotation = block.Tools[RotationToolName];
            foreach (string name in new[] { "CenterX", "CenterY", "RmsError" }) Connect(block.Outputs, name, rotation, name);
            Connect(block.Outputs, "RotationValid", rotation, "Valid");
        }

        /// <summary>只升级脚本、端口和连接仍保持标准定义的第二版标定，保留原 PM 采点模式。</summary>
        private static bool TryUpgradePrecisionV2(CogToolBlock original, out CogToolBlock upgraded)
        {
            upgraded = null;
            if (original.Tools.Count != 5 || original.Inputs.Count != 10 || original.Outputs.Count != 12 || original.DisabledTools.Count != 0) return false;
            if (original.DataBindings.Count != 0 || original.Inputs.DataBindings.Count != 0 || !Convert.ToBoolean(original.UserData["RawCalibrationPixels"])) return false;
            if (!ScriptMatchesResource(original, "Positioning.txt")) return false;
            if (!original.Tools.Contains("AcquireImage") || !(original.Tools["AcquireImage"] is CogToolBlock)) return false;
            if (!original.Tools.Contains("PositionResult") || !(original.Tools["PositionResult"] is CogToolBlock)) return false;
            if (!original.Tools.Contains(RotationToolName) || !(original.Tools[RotationToolName] is CogToolBlock)) return false;
            if (!original.Tools.Contains("Calibration") || !(original.Tools["Calibration"] is CogCalibNPointToNPointTool)) return false;
            if (!original.Tools.Contains(PatternToolName) || !(original.Tools[PatternToolName] is CogPMAlignTool)) return false;
            CogToolBlock capture = (CogToolBlock)original.Tools["AcquireImage"], result = (CogToolBlock)original.Tools["PositionResult"], rotation = (CogToolBlock)original.Tools[RotationToolName];
            if (capture.Inputs.Count != 5 || capture.Outputs.Count != 1 || result.Inputs.Count != 4 || result.Outputs.Count != 7 || rotation.Inputs.Count != 4 || rotation.Outputs.Count != 4) return false;
            if (!ScriptMatchesResource(capture, "Acquisition.txt") || !ScriptMatchesResource(result, "PositionResult.txt") || !ScriptMatchesResource(rotation, "RotationCenter.txt")) return false;
            if (capture.Tools.Count != 1 || !(capture.Tools[0] is CogAcqFifoTool) || result.Tools.Count != 0 || rotation.Tools.Count != 0) return false;
            if (new[] { capture, result, rotation }.Any(child => child.DisabledTools.Count != 0 || child.Inputs.DataBindings.Count != 0 || child.Outputs.DataBindings.Count != 0)) return false;
            if (!HasV2Connections(original)) return false;
            CogToolBlock copy = (CogToolBlock)CogSerializer.DeepCopyObject(original);
            try
            {
                AddPrecisionCalibration(copy, VpCalibrationFeature.Pattern);
                ApplyDescriptions(copy); ApplyMessages(copy); upgraded = copy; return true;
            }
            catch { copy.Dispose(); throw; }
        }

        /// <summary>确认既有脚本正文与发布资源相同，拒绝覆盖用户脚本。</summary>
        private static bool ScriptMatchesResource(CogToolBlock block, string resource)
        {
            if (block.Script == null) return false;
            using (var stream = typeof(VpAlgorithmTemplates).Assembly.GetManifestResourceStream("AlphaRap.Vision.Templates." + resource))
            using (var reader = new System.IO.StreamReader(stream))
                return block.Script.Source.Replace("\r\n", "\n").Trim() == reader.ReadToEnd().Replace("\r\n", "\n").Trim();
        }

        /// <summary>按工具名和编辑器端口识别标准连接，端口 ID 不参与版本判断。</summary>
        private static bool HasV2Connections(CogToolBlock block)
        {
            var tools = block.Tools.Cast<ICogTool>().Concat(new ICogTool[] { block.Inputs, block.Outputs }).ToArray();
            var actual = new System.Collections.Generic.List<string>();
            foreach (ICogTool target in tools)
            foreach (CogDataBinding binding in target.DataBindings)
            {
                ICogTool source = binding.Source as ICogTool;
                if (source == null || !tools.Contains(source)) return false;
                string input = TerminalName(target, binding.DestinationPath, true), output = TerminalName(source, binding.SourcePath, false);
                if (input == null || output == null) return false;
                actual.Add(V2ToolName(block, target) + "." + input + "=" + V2ToolName(block, source) + "." + output);
            }
            var expected = new System.Collections.Generic.List<string>();
            foreach (string name in new[] { "CameraSN", "Exposure", "UseInputImage", "InputImage", "Language" }) expected.Add("AcquireImage." + name + "=Inputs." + name);
            expected.Add("Calibration.InputImage=AcquireImage.OutputImage"); expected.Add("LocatePattern.InputImage=AcquireImage.OutputImage");
            expected.Add("PositionResult.Matches=LocatePattern.Results"); expected.Add("PositionResult.CalibrationModel=Calibration.Calibration");
            expected.Add("PositionResult.UseCalibration=Inputs.UseCalibration"); expected.Add("PositionResult.Language=Inputs.Language");
            foreach (string name in new[] { "PixelX", "PixelY", "AnglesDegrees" }) expected.Add("RotationCenter." + name + "=Inputs.Rotation" + name);
            expected.Add("RotationCenter.Language=Inputs.Language");
            foreach (string name in new[] { "Accepted", "Calibrated", "x", "y", "A", "Score", "MatchCount" }) expected.Add("Outputs." + name + "=PositionResult." + name);
            expected.Add("Outputs.OutputImage=AcquireImage.OutputImage"); expected.Add("Outputs.RotationValid=RotationCenter.Valid");
            foreach (string name in new[] { "CenterX", "CenterY", "RmsError" }) expected.Add("Outputs." + name + "=RotationCenter." + name);
            return actual.OrderBy(value => value, StringComparer.Ordinal).SequenceEqual(expected.OrderBy(value => value, StringComparer.Ordinal));
        }

        /// <summary>取得与数据连接路径一致的可见端口名。</summary>
        private static string TerminalName(ICogTool tool, string path, bool input)
        {
            string[] terminals = input ? CogToolTerminals.GetToolInputTerminals(tool) : CogToolTerminals.GetToolOutputTerminals(tool);
            string terminal = (terminals ?? new string[0]).FirstOrDefault(value => CogToolTerminals.GetTerminalPath(value) == path);
            return terminal == null ? null : CogToolTerminals.GetTerminalName(terminal);
        }

        /// <summary>为根输入输出集合提供不随内部名字变化的连接角色。</summary>
        private static string V2ToolName(CogToolBlock root, ICogTool tool)
        {
            if (ReferenceEquals(tool, root.Inputs)) return "Inputs";
            if (ReferenceEquals(tool, root.Outputs)) return "Outputs";
            return tool.Name;
        }

        /// <summary>为采点模式、夹具和质量限值提供三语说明。</summary>
        private static void ApplyPrecisionDescriptions(CogToolBlock block)
        {
            if (!block.Inputs.Contains("FeatureMode")) return;
            block.Inputs["FeatureMode"].Description = FrameworkLanguage.LangMsg("Vision", "template_FeatureMode", "0：圆心；1：两条拟合直线的交点；2：PM 模板定位（兼容）。圆心 A=0，不测量方向。", "0: circle center; 1: intersection of two fitted lines; 2: PM pattern location (legacy). Circle A=0; direction is not measured.", "0: centro de círculo; 1: intersección de dos líneas ajustadas; 2: patrón PM (compatibilidad). A=0 en círculo; no mide dirección.");
            block.Inputs["UsePatternFixture"].Description = FrameworkLanguage.LangMsg("Vision", "template_PatternFixture", "启用后先运行 LocatePattern，再在 FeatureFixture 图像中设置找圆/找线区域。PM 只跟随区域，不代替精定位坐标。", "Enable LocatePattern before measuring. Set circle/line regions in the FeatureFixture image. PM follows regions; precise coordinates still come from geometry.", "Active LocatePattern antes de medir. Configure regiones en FeatureFixture. PM desplaza las regiones; las coordenadas precisas proceden de la geometría.");
            block.Inputs["MaxFitRms"].Description = FrameworkLanguage.LangMsg("Vision", "template_FitRms", "圆或每条直线允许的最大拟合 RMS，单位像素；只是拟合质量门限，不代表设备标定精度。", "Maximum circle/line fit RMS in pixels; this is a fit-quality limit, not the machine calibration accuracy.", "RMS máximo del ajuste de círculo/línea en píxeles; es un límite de calidad, no la precisión de calibración de la máquina.");
            block.Inputs["MinCirclePoints"].Description = FrameworkLanguage.LangMsg("Vision", "template_MinCirclePoints", "圆拟合至少使用的有效边缘点数，不能小于 3。", "Minimum number of used circle edge points; at least 3.", "Número mínimo de puntos de borde usados para el círculo; al menos 3.");
            block.Inputs["MinLinePoints"].Description = FrameworkLanguage.LangMsg("Vision", "template_MinLinePoints", "每条拟合直线至少使用的有效边缘点数，不能小于 2。", "Minimum number of used edge points on each fitted line; at least 2.", "Número mínimo de puntos usados por cada línea ajustada; al menos 2.");
            block.Inputs["MinLineAngleDegrees"].Description = FrameworkLanguage.LangMsg("Vision", "template_MinLineAngle", "两条直线的最小锐夹角，单位度，范围大于 0 且不超过 90；近似平行时拒绝采点。", "Minimum acute angle between lines in degrees, greater than 0 and at most 90; near-parallel lines are rejected.", "Ángulo agudo mínimo entre líneas en grados, mayor que 0 y no superior a 90; se rechazan líneas casi paralelas.");
            block.Outputs["FitRms"].Description = FrameworkLanguage.LangMsg("Vision", "template_FitRmsOutput", "圆的拟合 RMS 或两条直线中较大的 RMS，单位像素；模板兼容模式为 NaN。", "Circle fit RMS or the larger line fit RMS, in pixels; NaN in legacy pattern mode.", "RMS del círculo o el mayor RMS de las líneas, en píxeles; NaN en modo de patrón compatible.");
        }

        /// <summary>传递现场语言包中的精定位信息给独立 VPP 脚本。</summary>
        private static void ApplyPrecisionMessages(CogToolBlock block)
        {
            if (!block.Inputs.Contains("FeatureMode")) return;
            SetMessage(block, "FeatureModeInvalid", "请选择圆心、两线交点或模板定位。", "Select circle center, line intersection, or pattern location.", "Seleccione centro de círculo, intersección de líneas o localización de patrón.");
            SetMessage(block, "CoarseMissing", "粗定位未找到特征，未执行精定位采点。", "Coarse location found no feature; no precision point was sampled.", "La localización aproximada no encontró la característica; no se tomó una muestra precisa.");
            SetMessage(block, "PrecisionInvalid", "标定精定位无效：请检查找边区域、有效点数、拟合残差及两线夹角；不会使用粗定位坐标代替。", "Invalid calibration feature: check edge regions, used points, fit RMS, and line angle. Coarse coordinates are not substituted.", "Característica de calibración no válida: revise regiones, puntos usados, RMS y ángulo entre líneas. No se sustituyen las coordenadas aproximadas.");
        }
    }
}
