using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cognex.VisionPro;
using Cognex.VisionPro.CalibFix;
using Cognex.VisionPro.Caliper;
using Cognex.VisionPro.Implementation.Internal;
using Cognex.VisionPro.PMAlign;
using Cognex.VisionPro.ToolBlock;

namespace AlphaRap
{
    /// <summary>创建可继续在 VisionPro 中编辑的标定与单模板定位算法，不连接相机或写入文件。</summary>
    public static partial class VpAlgorithmTemplates
    {
        private const string TemplateKey = "AlphaRap.BuiltInTemplate";
        internal const string PatternToolName = "LocatePattern";
        internal const string RotationToolName = "RotationCenter";
        private static readonly object PositioningTemplatesLock = new object();
        private static readonly List<PositioningTemplate> PositioningTemplates = new List<PositioningTemplate>
        {
            new PositioningTemplate("Pattern", "普通定位（单目标）", "Single-target positioning", "Posicionamiento de un objetivo",
                (sn, exposure) => Create(false, sn, exposure)),
            new PositioningTemplate("PatternCount", "多目标匹配计数", "Multi-target match count", "Recuento de coincidencias",
                CreatePatternCount)
        };

        /// <summary>登记 TB 编辑器中的定位模板。工厂须返回独立、已连线且符合标准定位端口的 ToolBlock。</summary>
        public static void RegisterPositioningTemplate(string id, string chinese, string english, string spanish,
            Func<string, double, CogToolBlock> create)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException(nameof(id));
            System.Xml.XmlConvert.VerifyNCName("positioning_" + id);
            if (string.IsNullOrWhiteSpace(chinese) || string.IsNullOrWhiteSpace(english) || string.IsNullOrWhiteSpace(spanish))
                throw new ArgumentException(nameof(chinese) + ", " + nameof(english) + ", " + nameof(spanish));
            if (create == null) throw new ArgumentNullException(nameof(create));
            lock (PositioningTemplatesLock)
            {
                if (PositioningTemplates.Any(template => string.Equals(template.Id, id, StringComparison.OrdinalIgnoreCase)))
                    throw new ArgumentException(nameof(id) + ": " + id);
                PositioningTemplates.Add(new PositioningTemplate(id, chinese, english, spanish, create));
            }
        }

        /// <summary>返回注册快照，避免其他模块登记时改变正在打开的模板列表。</summary>
        internal static PositioningTemplate[] GetPositioningTemplates()
        {
            lock (PositioningTemplatesLock) return PositioningTemplates.ToArray();
        }

        /// <summary>通过相同的取图、定位及坐标转换图查找多个目标，并输出实际匹配数量。</summary>
        private static CogToolBlock CreatePatternCount(string cameraSN, double exposure)
        {
            CogToolBlock block = Create(false, cameraSN, exposure);
            block.Name = "PatternMatchCount";
            block.UserData[TemplateKey] = "PatternCount/1";
            ((CogPMAlignTool)block.Tools[PatternToolName]).RunParams.ApproximateNumberToFind = 10;
            return block;
        }

        internal sealed class PositioningTemplate
        {
            internal string Id { get; }
            private readonly string chinese, english, spanish;
            private readonly Func<string, double, CogToolBlock> create;

            /// <summary>记录模板稳定标识、三语显示名和每次创建独立工具块的工厂。</summary>
            internal PositioningTemplate(string id, string chinese, string english, string spanish, Func<string, double, CogToolBlock> create)
            {
                Id = id;
                this.chinese = chinese;
                this.english = english;
                this.spanish = spanish;
                this.create = create;
            }

            /// <summary>返回当地语言的模板名称，保留列表对象的业务身份。</summary>
            public override string ToString()
            {
                return FrameworkLanguage.LangMsg("TBEditForm", "positioning_" + Id, chinese, english, spanish);
            }

            /// <summary>验证定位运行、标定和流程使用的固定端口，失败时释放刚创建的工具块。</summary>
            internal CogToolBlock CreateBlock(string cameraSN, double exposure)
            {
                CogToolBlock block = create(cameraSN, exposure);
                try
                {
                    if (block == null || !HasPositioningPorts(block) || !block.Tools.Contains("Calibration") ||
                        !(block.Tools["Calibration"] is CogCalibNPointToNPointTool) || block.Tools.Contains(RotationToolName))
                        throw new InvalidOperationException(FrameworkLanguage.LangMsg("TBEditForm", "msg_InvalidPositioningTemplate",
                            "定位模板缺少标准端口或九点标定工具。", "The positioning template is missing standard ports or its nine-point calibration tool.",
                            "A la plantilla le faltan puertos estándar o la herramienta de calibración de nueve puntos."));
                    if (!IsBuiltIn(block)) block.UserData[TemplateKey] = "Registered:" + Id;
                    block.UserData["RawCalibrationPixels"] = false;
                    Configure(block, cameraSN, exposure);
                    return block;
                }
                catch { block?.Dispose(); throw; }
            }

            /// <summary>允许自定义业务端口，同时要求框架依赖的端口和值类型保持一致。</summary>
            private static bool HasPositioningPorts(CogToolBlock block)
            {
                return HasPorts(block.Inputs,
                    new[] { "CameraSN", "Exposure", "Language", "UseInputImage", "InputImage", "UseCalibration" },
                    new[] { typeof(string), typeof(double), typeof(int), typeof(bool), typeof(ICogImage), typeof(bool) })
                    && HasPorts(block.Outputs,
                    new[] { "Accepted", "x", "y", "A", "Score", "MatchCount", "Calibrated", "OutputImage" },
                    new[] { typeof(bool), typeof(double), typeof(double), typeof(double), typeof(double), typeof(int), typeof(bool), typeof(ICogImage) });
            }

        }

        /// <summary>建立采集、模板定位及坐标标定工具；标定模板另带独立的旋转中心计算工具。</summary>
        public static CogToolBlock Create(bool calibration, string cameraSN, double exposure)
        {
            CogToolBlock block = new CogToolBlock();
            try
            {
                block.Name = calibration ? "CameraCalibration" : "PatternPositioning";
                block.UserData[TemplateKey] = calibration ? "Calibration/2" : "Positioning/2";
                AddInput(block, "CameraSN", typeof(string), cameraSN ?? "");
                AddInput(block, "Exposure", typeof(double), exposure);
                AddInput(block, "UseInputImage", typeof(bool), false);
                AddInput(block, "InputImage", typeof(ICogImage), null);
                AddInput(block, "Language", typeof(int), (int)SysPara.LanguageShow);
                AddOutput(block, "Accepted", typeof(bool), false);
                AddOutput(block, "x", typeof(double), double.NaN);
                AddOutput(block, "y", typeof(double), double.NaN);
                AddOutput(block, "A", typeof(double), double.NaN);
                AddOutput(block, "Score", typeof(double), 0.0);
                AddOutput(block, "MatchCount", typeof(int), 0);
                AddOutput(block, "Calibrated", typeof(bool), false);
                AddOutput(block, "OutputImage", typeof(ICogImage), null);
                block.Tools.Add(new CogAcqFifoTool { Name = "AcquireImage" });
                block.Tools.Add(new CogCalibNPointToNPointTool { Name = "Calibration" });
                CogPMAlignTool pattern = new CogPMAlignTool { Name = PatternToolName };
                pattern.SearchRegion = null;
                pattern.RunParams.ApproximateNumberToFind = 1;
                pattern.RunParams.AcceptThreshold = 0.8;
                block.Tools.Add(pattern);
                if (calibration) block.Tools.Add(CreateRotationCenter());
                block.UserData["RawCalibrationPixels"] = calibration;
                ConnectGraph(block, calibration);
                if (calibration) AddPrecisionCalibration(block, VpCalibrationFeature.Circle);
                else SetScript(block, "Positioning.txt");
                ApplyDescriptions(block);
                ApplyMessages(block);
                return block;
            }
            catch { block.Dispose(); throw; }
        }

        /// <summary>识别框架生成的工具块，不按工具名称猜测或修改用户自定义算法。</summary>
        public static bool IsBuiltIn(CogToolBlock block)
        {
            return block != null && block.UserData.Contains(TemplateKey);
        }

        /// <summary>识别没有工具、端口、脚本或业务数据的占位算法，允许 SDK 自带的空 ScriptData。</summary>
        internal static bool IsEmptyPlaceholder(CogToolBlock block)
        {
            if (block == null || block.Tools.Count != 0 || block.DisabledTools.Count != 0) return false;
            if (block.Inputs.Count != 0 || block.Outputs.Count != 0 || block.DataBindings.Count != 0) return false;
            if (block.Script != null || block.ScriptData.Count != 0) return false;
            foreach (DictionaryEntry entry in block.UserData)
            {
                CogDictionary scriptData = entry.Value as CogDictionary;
                if (!string.Equals(entry.Key as string, "ScriptData", StringComparison.Ordinal) || scriptData == null || scriptData.Count != 0) return false;
            }
            return true;
        }

        /// <summary>模板内部的采集和离线调试端口不要求生产流程绑定；用户业务输入保持开放。</summary>
        internal static bool IsManagedInput(CogToolBlock block, string name)
        {
            if (!IsBuiltIn(block)) return false;
            return name == "CameraSN" || name == "Exposure" || name == "Language" || name == "UseInputImage" || name == "InputImage" || name == "UseCalibration" || name == "RunRotation" || name == "RotationPixelX" || name == "RotationPixelY" || name == "RotationAnglesDegrees" || name == "FeatureMode" || name == "UsePatternFixture" || name == "MaxFitRms" || name == "MinCirclePoints" || name == "MinLinePoints" || name == "MinLineAngleDegrees";
        }

        /// <summary>按当前相机绑定和本站曝光更新模板输入，保留用户训练数据及其他端口。</summary>
        internal static void Configure(CogToolBlock block, string cameraSN, double exposure)
        {
            if (!IsBuiltIn(block)) return;
            block.Inputs["CameraSN"].Value = cameraSN ?? "";
            block.Inputs["Exposure"].Value = exposure;
            block.Inputs["Language"].Value = (int)SysPara.LanguageShow;
            if (block.Inputs.Contains("RunRotation")) block.Inputs["RunRotation"].Value = false;
            if (block.Inputs.Contains("UseCalibration") && Convert.ToBoolean(block.UserData["RawCalibrationPixels"])) block.Inputs["UseCalibration"].Value = false;
            if (block.Tools.Contains(RotationToolName))
                ((CogToolBlock)block.Tools[RotationToolName]).Inputs["Language"].Value = (int)SysPara.LanguageShow;
            ApplyDescriptions(block);
            ApplyMessages(block);
        }

        /// <summary>建立独立可运行的旋转中心工具，输入像素采样与实际角度，输出中心及轨迹残差。</summary>
        private static CogToolBlock CreateRotationCenter()
        {
            CogToolBlock block = new CogToolBlock { Name = RotationToolName };
            try
            {
                AddInput(block, "PixelX", typeof(double[]), new double[0]);
                AddInput(block, "PixelY", typeof(double[]), new double[0]);
                AddInput(block, "AnglesDegrees", typeof(double[]), new double[0]);
                AddInput(block, "Language", typeof(int), (int)SysPara.LanguageShow);
                AddOutput(block, "CenterX", typeof(double), double.NaN);
                AddOutput(block, "CenterY", typeof(double), double.NaN);
                AddOutput(block, "RmsError", typeof(double), double.NaN);
                AddOutput(block, "Valid", typeof(bool), false);
                SetScript(block, "RotationCenter.txt");
                return block;
            }
            catch { block.Dispose(); throw; }
        }

        /// <summary>加载随框架发布的独立脚本并检查编译结果，生成的 VPP 不引用框架类型。</summary>
        private static void SetScript(CogToolBlock block, string resource)
        {
            block.CreateNewScript(CogScriptLanguageConstants.ScriptCSharp, CogToolBlockScriptTypeConstants.Advanced);
            CogScriptSupport script = new CogScriptSupport(block.Script);
            using (Stream stream = typeof(VpAlgorithmTemplates).Assembly.GetManifestResourceStream("AlphaRap.Vision.Templates." + resource))
            {
                if (stream == null) throw new InvalidOperationException(FrameworkLanguage.LangMsg("Vision", "template_ResourceMissing", "缺少内置视觉模板资源：", "Missing built-in vision template resource: ", "Falta un recurso de plantilla de visión integrada: ") + resource);
                using (StreamReader reader = new StreamReader(stream)) script.Source = reader.ReadToEnd();
            }
            script.References.Add(new CogReferencedAssembly(typeof(CogPMAlignTool).Assembly));
            script.References.Add(new CogReferencedAssembly(typeof(CogCalibNPointToNPointTool).Assembly));
            script.References.Add(new CogReferencedAssembly(typeof(CogFindCircleTool).Assembly));
            if (!script.TempCompile()) throw new InvalidOperationException(FrameworkLanguage.LangMsg("Vision", "template_CompileFailed",
                "内置视觉模板编译失败：", "The built-in vision template could not compile: ", "No se pudo compilar la plantilla de visión integrada: ") + string.Join("; ", script.Results.Cast<CogCompileMessage>().Select(item => item.Line + ": " + item.Text)));
            block.Script = script;
        }

        /// <summary>声明具有稳定业务键和明确初值的输入。</summary>
        private static void AddInput(CogToolBlock block, string name, Type type, object value)
        {
            block.Inputs.Add(new CogToolBlockTerminal(name, value, type));
        }

        /// <summary>声明输出初值，尚未成功定位时不使用零坐标冒充有效结果。</summary>
        private static void AddOutput(CogToolBlock block, string name, Type type, object value)
        {
            block.Outputs.Add(new CogToolBlockTerminal(name, value, type));
        }

        /// <summary>建立编辑器可见的图像、模型、匹配结果及旋转样本连接，脚本只调度工具。</summary>
        private static void ConnectGraph(CogToolBlock block, bool calibration)
        {
            CogAcqFifoTool native = (CogAcqFifoTool)block.Tools["AcquireImage"];
            block.Tools.Remove(native);
            CogToolBlock capture = CreateAcquisition(native);
            block.Tools.Insert(0, capture);
            CogToolBlock output = CreatePositionResult(); block.Tools.Add(output);
            capture.FailOnInvalidDataBinding = true; output.FailOnInvalidDataBinding = true;
            block.Outputs.FailOnInvalidDataBinding = true;
            AddInput(block, "UseCalibration", typeof(bool), !calibration);
            foreach (string name in new[] { "CameraSN", "Exposure", "UseInputImage", "InputImage", "Language" })
                Connect(capture, name, block.Inputs, name);
            CogPMAlignTool pattern = (CogPMAlignTool)block.Tools[PatternToolName];
            CogCalibNPointToNPointTool coordinates = (CogCalibNPointToNPointTool)block.Tools["Calibration"];
            pattern.FailOnInvalidDataBinding = true; coordinates.FailOnInvalidDataBinding = true;
            ExposeTerminal(pattern, "InputImage", true); ExposeTerminal(pattern, "Results", false);
            ExposeTerminal(coordinates, "InputImage", true); ExposeTerminal(coordinates, "Calibration", false);
            Connect(pattern, "InputImage", capture, "OutputImage");
            Connect(coordinates, "InputImage", capture, "OutputImage");
            Connect(output, "Matches", pattern, "Results");
            Connect(output, "CalibrationModel", coordinates, "Calibration");
            Connect(output, "UseCalibration", block.Inputs, "UseCalibration");
            Connect(output, "Language", block.Inputs, "Language");
            foreach (CogToolBlockTerminal terminal in output.Outputs) Connect(block.Outputs, terminal.Name, output, terminal.Name);
            Connect(block.Outputs, "OutputImage", capture, "OutputImage");
            if (!calibration) return;
            CogToolBlock rotation = (CogToolBlock)block.Tools[RotationToolName];
            rotation.FailOnInvalidDataBinding = true;
            AddInput(block, "RunRotation", typeof(bool), false);
            foreach (string name in new[] { "PixelX", "PixelY", "AnglesDegrees" })
            {
                double[] samples = (double[])rotation.Inputs[name].Value;
                AddInput(block, "Rotation" + name, typeof(double[]), (double[])samples.Clone());
                Connect(rotation, name, block.Inputs, "Rotation" + name);
            }
            Connect(rotation, "Language", block.Inputs, "Language");
            foreach (string name in new[] { "CenterX", "CenterY", "RmsError" })
            {
                AddOutput(block, name, typeof(double), double.NaN);
                Connect(block.Outputs, name, rotation, name);
            }
            if (!rotation.Outputs.Contains("Valid"))
            {
                AddOutput(rotation, "Valid", typeof(bool), false);
                SetScript(rotation, "RotationCenter.txt");
            }
            AddOutput(block, "RotationValid", typeof(bool), false);
            Connect(block.Outputs, "RotationValid", rotation, "Valid");
        }

        /// <summary>把相机配置和离线图像收拢为具有实际端口的采集工具块。</summary>
        private static CogToolBlock CreateAcquisition(CogAcqFifoTool original)
        {
            CogToolBlock block = new CogToolBlock { Name = "AcquireImage" };
            try
            {
                AddInput(block, "CameraSN", typeof(string), ""); AddInput(block, "Exposure", typeof(double), 10.0);
                AddInput(block, "UseInputImage", typeof(bool), false); AddInput(block, "InputImage", typeof(ICogImage), null);
                AddInput(block, "Language", typeof(int), (int)SysPara.LanguageShow);
                AddOutput(block, "OutputImage", typeof(ICogImage), null);
                original.Name = "CameraCapture";
                block.Tools.Add(original);
                SetScript(block, "Acquisition.txt");
                return block;
            }
            catch { block.Dispose(); throw; }
        }

        /// <summary>建立可连接和单步调试的定位结果转换节点。</summary>
        private static CogToolBlock CreatePositionResult()
        {
            CogToolBlock block = new CogToolBlock { Name = "PositionResult" };
            try
            {
                AddInput(block, "Matches", typeof(CogPMAlignResults), null);
                AddInput(block, "CalibrationModel", typeof(CogCalibNPointToNPoint), null);
                AddInput(block, "UseCalibration", typeof(bool), false); AddInput(block, "Language", typeof(int), (int)SysPara.LanguageShow);
                AddOutput(block, "Accepted", typeof(bool), false); AddOutput(block, "Calibrated", typeof(bool), false);
                foreach (string name in new[] { "x", "y", "A" }) AddOutput(block, name, typeof(double), double.NaN);
                AddOutput(block, "Score", typeof(double), 0.0); AddOutput(block, "MatchCount", typeof(int), 0);
                SetScript(block, "PositionResult.txt");
                return block;
            }
            catch { block.Dispose(); throw; }
        }

        /// <summary>使用编辑器端口定义中的稳定 ID、类型和路径建立原生连接。</summary>
        private static void Connect(ICogTool target, string input, ICogTool source, string output)
        {
            string destinationPath = CogToolTerminals.GetToolInputTerminals(target).Single(terminal => CogToolTerminals.GetTerminalName(terminal) == input);
            string sourcePath = CogToolTerminals.GetToolOutputTerminals(source).Single(terminal => CogToolTerminals.GetTerminalName(terminal) == output);
            target.DataBindings.Add(CogToolTerminals.GetTerminalPath(destinationPath), source, CogToolTerminals.GetTerminalPath(sourcePath));
        }

        /// <summary>把原生工具的数据属性登记为编辑器可见端口，保留已有显示项。</summary>
        private static void ExposeTerminal(ICogTool tool, string property, bool input)
        {
            string[] terminals = (input ? CogToolTerminals.GetToolInputTerminals(tool) : CogToolTerminals.GetToolOutputTerminals(tool)) ?? new string[0];
            if (terminals.Any(terminal => CogToolTerminals.GetTerminalName(terminal) == property)) return;
            CogToolTerminals.SetToolTerminals(tool, input, terminals.Concat(new[] { CogToolTerminals.FormatTerminal(property, property) }).ToArray());
        }

        /// <summary>只升级未经脚本或连线自定义的第一版模板，保留模型、标定、样本及原文件。</summary>
        internal static bool TryUpgrade(CogToolBlock original, out CogToolBlock upgraded)
        {
            upgraded = null;
            if (!IsBuiltIn(original)) return false;
            string identity = Convert.ToString(original.UserData[TemplateKey]);
            if (identity == "Calibration/2") return TryUpgradePrecisionV2(original, out upgraded);
            if (identity == "Calibration/3") return TryUpgradeRotationV3(original, out upgraded);
            if (identity != "Calibration/1" && identity != "Positioning/1") return false;
            if (!HasStockScript(original, "47C9140F68B2EBAB214D6AF0D5E20689F1694A893B096AEDF5C058DE6E7E193C")) return false;
            bool calibration = Convert.ToBoolean(original.UserData["RawCalibrationPixels"]);
            if (original.Tools.Count != (calibration ? 4 : 3) || original.Inputs.Count != 5 || original.Outputs.Count != 8) return false;
            if (!HasPorts(original.Inputs, new[] { "CameraSN", "Exposure", "UseInputImage", "InputImage", "Language" }, new[] { typeof(string), typeof(double), typeof(bool), typeof(ICogImage), typeof(int) })) return false;
            if (!HasPorts(original.Outputs, new[] { "Accepted", "x", "y", "A", "Score", "MatchCount", "Calibrated", "OutputImage" }, new[] { typeof(bool), typeof(double), typeof(double), typeof(double), typeof(double), typeof(int), typeof(bool), typeof(ICogImage) })) return false;
            if (!original.Tools.Contains("AcquireImage") || !(original.Tools["AcquireImage"] is CogAcqFifoTool)) return false;
            if (!original.Tools.Contains("Calibration") || !(original.Tools["Calibration"] is CogCalibNPointToNPointTool)) return false;
            if (!original.Tools.Contains(PatternToolName) || !(original.Tools[PatternToolName] is CogPMAlignTool)) return false;
            if (original.Tools.Cast<ICogTool>().Any(tool => tool.DataBindings.Count != 0)) return false;
            if (calibration && !HasStockRotation(original)) return false;
            CogToolBlock copy = (CogToolBlock)CogSerializer.DeepCopyObject(original);
            try
            {
                ConnectGraph(copy, calibration); SetScript(copy, "Positioning.txt");
                copy.UserData[TemplateKey] = calibration ? "Calibration/2" : "Positioning/2";
                if (calibration) AddPrecisionCalibration(copy, VpCalibrationFeature.Pattern);
                ApplyDescriptions(copy); ApplyMessages(copy); upgraded = copy;
                return true;
            }
            catch { copy.Dispose(); throw; }
        }

        /// <summary>仅识别无自定义绑定、禁用工具且脚本内容未改动的已发布范例。</summary>
        private static bool HasStockScript(CogToolBlock block, string signature)
        {
            if (block.Script == null || block.DisabledTools.Count != 0 || block.DataBindings.Count != 0) return false;
            if (block.Inputs.DataBindings.Count != 0 || block.Outputs.DataBindings.Count != 0) return false;
            using (var hash = System.Security.Cryptography.SHA256.Create())
            {
                string source = block.Script.Source.Replace("\r\n", "\n").Trim();
                return BitConverter.ToString(hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(source))).Replace("-", "") == signature;
            }
        }

        /// <summary>确认旋转子块仍是标准脚本和端口，避免升级覆盖用户编写的计算。</summary>
        private static bool HasStockRotation(CogToolBlock block)
        {
            if (!block.Tools.Contains(RotationToolName)) return false;
            CogToolBlock rotation = block.Tools[RotationToolName] as CogToolBlock;
            if (rotation == null || rotation.Tools.Count != 0 || rotation.Inputs.Count != 4 || rotation.Outputs.Count != 3) return false;
            if (!HasPorts(rotation.Inputs, new[] { "PixelX", "PixelY", "AnglesDegrees", "Language" }, new[] { typeof(double[]), typeof(double[]), typeof(double[]), typeof(int) })) return false;
            if (!HasPorts(rotation.Outputs, new[] { "CenterX", "CenterY", "RmsError" }, new[] { typeof(double), typeof(double), typeof(double) })) return false;
            return HasStockScript(rotation, "1C7BEF1EE2C9A112461C421C05FFDC163477CDDB7035F9AC02C882FF75381CA2");
        }

        /// <summary>确认端口业务键和类型仍与已发布范例相同。</summary>
        private static bool HasPorts(CogToolBlockTerminalCollection ports, string[] names, Type[] types)
        {
            for (int index = 0; index < names.Length; index++)
                if (!ports.Contains(names[index]) || ports[names[index]].ValueType != types[index]) return false;
            return true;
        }

        /// <summary>给模板的关键操作提供三语说明，端口名保持稳定。</summary>
        private static void ApplyDescriptions(CogToolBlock block)
        {
            block.Inputs["UseInputImage"].Description = FrameworkLanguage.LangMsg("Vision", "template_UseInputImage",
                "勾选后使用 InputImage 离线测试；生产拍照请取消勾选。", "Enable to test InputImage offline; disable for production capture.", "Active para probar InputImage sin conexión; desactive para capturar en producción.");
            block.Outputs["Calibrated"].Description = FrameworkLanguage.LangMsg("Vision", "template_Calibrated",
                "false：x/y 为像素；true：x/y 为 Calibration 定义的坐标。A 的单位为度。标定采点始终输出像素。",
                "false: x/y are pixels; true: x/y use the Calibration coordinate space. A is in degrees. Calibration sampling always returns pixels.",
                "false: x/y en píxeles; true: x/y en el espacio de Calibration. A en grados. Las muestras de calibración siempre usan píxeles.");
            if (block.Inputs.Contains("UseCalibration")) block.Inputs["UseCalibration"].Description = FrameworkLanguage.LangMsg("Vision", "template_UseCalibration",
                "使用有效九点标定转换定位坐标；标定采点时保持 false，输出原始像素。", "Use a valid nine-point calibration to transform the pose; keep false when sampling raw calibration pixels.", "Use una calibración válida de nueve puntos para transformar la posición; mantenga false al tomar muestras de píxeles.");
            if (block.Inputs.Contains("RunRotation")) block.Inputs["RunRotation"].Description = FrameworkLanguage.LangMsg("Vision", "template_RunRotationNinePoint",
                "true：要求至少九点的有效 Calibration 模型，仅计算旋转样本；false：采图定位。生产拍照自动使用 false。", "true: requires a valid Calibration model with at least nine points and calculates rotation samples only; false: capture and locate. Production capture uses false.", "true: requiere Calibration válida con al menos nueve puntos y calcula solo las muestras de rotación; false: capture y localice. La captura de producción usa false.");
            foreach (string name in new[] { "MachineCenterX", "MachineCenterY" })
                if (block.Outputs.Contains(name)) block.Outputs[name].Description = FrameworkLanguage.LangMsg("Vision", "template_MachineRotationCenter",
                    "把每个旋转样本经有效九点关系转换后拟合的机械中心，单位 mm；CenterX/CenterY 和 RmsError 仍为像素。", "Machine center fitted after converting each rotation sample through valid nine-point calibration, in mm. CenterX/CenterY and RmsError remain pixels.", "Centro mecánico ajustado tras transformar cada muestra mediante nueve puntos válidos, en mm. CenterX/CenterY y RmsError siguen en píxeles.");
            ApplyPrecisionDescriptions(block);
            if (block.Tools.Contains(RotationToolName))
                ((CogToolBlock)block.Tools[RotationToolName]).Inputs["PixelX"].Description = FrameworkLanguage.LangMsg("Vision", "template_RotationSamples",
                    "相机和 XY 保持不动；输入同一特征的 PixelX/PixelY 及对应 R 轴实际角度 AnglesDegrees。中心和残差单位为像素，不执行轴补偿。",
                    "Keep camera and XY fixed; supply PixelX/PixelY of the same feature and matching actual R angles in AnglesDegrees. Center and residual are pixels; no axis compensation is executed.",
                    "Mantenga cámara y XY fijos; introduzca PixelX/PixelY de la misma característica y los ángulos R reales en AnglesDegrees. Centro y residuo en píxeles; no se ejecuta compensación de ejes.");
        }

        /// <summary>把现场语言包解析后的错误文案交给独立脚本，不改变算法输入及业务数据。</summary>
        private static void ApplyMessages(CogToolBlock block)
        {
            SetMessage(block, "TrainPattern", "请打开 LocatePattern，选择训练图像并框选特征后训练模板。", "Open LocatePattern, select a training image and region, then train the pattern.", "Abra LocatePattern, seleccione imagen y región de entrenamiento y entrene el patrón.");
            SetMessage(block, "NoMatch", "未找到满足分数要求的定位特征。", "No feature met the matching score.", "Ninguna característica alcanzó la puntuación requerida.");
            SetMessage(block, "InputRequired", "请提供离线 InputImage。", "Provide an offline InputImage.", "Proporcione InputImage para la prueba sin conexión.");
            SetMessage(block, "CameraRequired", "请先在相机页选择相机 SN。", "Select the camera SN on the camera page first.", "Seleccione primero el SN en la página de cámara.");
            SetMessage(block, "ExposureInvalid", "曝光必须为有效正数。", "Exposure must be a finite positive value.", "La exposición debe ser un valor positivo finito.");
            SetMessage(block, "CameraNotFound", "未找到已绑定的相机：", "Bound camera was not found: ", "No se encontró la cámara vinculada: ");
            ApplyPrecisionMessages(block);
            foreach (string name in new[] { "AcquireImage", "PositionResult", "CalibrationPoint" })
            {
                CogToolBlock child = block.Tools.Contains(name) ? block.Tools[name] as CogToolBlock : null;
                if (child == null) continue;
                child.UserData["MessageLanguage"] = (int)SysPara.LanguageShow;
                foreach (DictionaryEntry entry in block.UserData)
                    if (Convert.ToString(entry.Key).StartsWith("Message.", StringComparison.Ordinal)) child.UserData[entry.Key] = entry.Value;
            }
            if (!block.Tools.Contains(RotationToolName)) return;
            CogToolBlock rotation = (CogToolBlock)block.Tools[RotationToolName];
            SetMessage(rotation, "RotationPairs", "请提供至少三组对应的 PixelX/PixelY/AnglesDegrees 采样。", "Supply at least three matching PixelX/PixelY/AnglesDegrees samples.", "Introduzca al menos tres muestras correspondientes de PixelX/PixelY/AnglesDegrees.");
            SetMessage(rotation, "RotationFinite", "旋转采样必须为有限数值。", "Rotation samples must be finite.", "Las muestras de rotación deben ser valores finitos.");
            SetMessage(rotation, "RotationCoverage", "旋转角度重复、转动范围不足或特征未发生位移。", "Rotation angles are repeated, cover too little rotation, or the feature did not move.", "Los ángulos se repiten, cubren muy poca rotación o la característica no se movió.");
            SetMessage(rotation, "RotationFitFinite", "旋转拟合结果不是有限数值。", "The fitted rotation is not finite.", "La rotación calculada no es finita.");
            SetMessage(rotation, "RotationNinePointRequired", "请先完成至少九点标定并应用到本相机，再进行旋转标定。", "Complete and apply at least nine calibration points to this camera before rotation calibration.", "Complete y aplique al menos nueve puntos a esta cámara antes de calibrar la rotación.");
        }

        /// <summary>按稳定语言键解析文案，脚本独立运行时仍可使用随附三语底稿。</summary>
        private static void SetMessage(CogToolBlock block, string key, string chinese, string english, string spanish)
        {
            block.UserData["MessageLanguage"] = (int)SysPara.LanguageShow;
            block.UserData["Message." + english] = FrameworkLanguage.LangMsg("Vision", "template_" + key, chinese, english, spanish);
        }
    }
}
