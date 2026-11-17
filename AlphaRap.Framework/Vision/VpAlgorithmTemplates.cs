using System;
using System.Collections;
using System.IO;
using System.Linq;
using Cognex.VisionPro;
using Cognex.VisionPro.CalibFix;
using Cognex.VisionPro.Implementation.Internal;
using Cognex.VisionPro.PMAlign;
using Cognex.VisionPro.ToolBlock;

namespace AlphaRap
{
    /// <summary>创建可继续在 VisionPro 中编辑的标定与单模板定位算法，不连接相机或写入文件。</summary>
    public static class VpAlgorithmTemplates
    {
        private const string TemplateKey = "AlphaRap.BuiltInTemplate";
        internal const string PatternToolName = "LocatePattern";
        internal const string RotationToolName = "RotationCenter";

        /// <summary>建立采集、模板定位及坐标标定工具；标定模板另带独立的旋转中心计算工具。</summary>
        public static CogToolBlock Create(bool calibration, string cameraSN, double exposure)
        {
            CogToolBlock block = new CogToolBlock();
            try
            {
                block.Name = calibration ? "CameraCalibration" : "PatternPositioning";
                block.UserData[TemplateKey] = calibration ? "Calibration/1" : "Positioning/1";
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
                SetScript(block, "Positioning.txt");
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
            return name == "CameraSN" || name == "Exposure" || name == "Language" || name == "UseInputImage" || name == "InputImage";
        }

        /// <summary>按当前相机绑定和本站曝光更新模板输入，保留用户训练数据及其他端口。</summary>
        internal static void Configure(CogToolBlock block, string cameraSN, double exposure)
        {
            if (!IsBuiltIn(block)) return;
            block.Inputs["CameraSN"].Value = cameraSN ?? "";
            block.Inputs["Exposure"].Value = exposure;
            block.Inputs["Language"].Value = (int)SysPara.LanguageShow;
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

        /// <summary>给模板的关键操作提供三语说明，端口名保持稳定。</summary>
        private static void ApplyDescriptions(CogToolBlock block)
        {
            block.Inputs["UseInputImage"].Description = FrameworkLanguage.LangMsg("Vision", "template_UseInputImage",
                "勾选后使用 InputImage 离线测试；生产拍照请取消勾选。", "Enable to test InputImage offline; disable for production capture.", "Active para probar InputImage sin conexión; desactive para capturar en producción.");
            block.Outputs["Calibrated"].Description = FrameworkLanguage.LangMsg("Vision", "template_Calibrated",
                "false：x/y 为像素；true：x/y 为 Calibration 定义的坐标。A 的单位为度。标定采点始终输出像素。",
                "false: x/y are pixels; true: x/y use the Calibration coordinate space. A is in degrees. Calibration sampling always returns pixels.",
                "false: x/y en píxeles; true: x/y en el espacio de Calibration. A en grados. Las muestras de calibración siempre usan píxeles.");
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
            if (!block.Tools.Contains(RotationToolName)) return;
            CogToolBlock rotation = (CogToolBlock)block.Tools[RotationToolName];
            SetMessage(rotation, "RotationPairs", "请提供至少三组对应的 PixelX/PixelY/AnglesDegrees 采样。", "Supply at least three matching PixelX/PixelY/AnglesDegrees samples.", "Introduzca al menos tres muestras correspondientes de PixelX/PixelY/AnglesDegrees.");
            SetMessage(rotation, "RotationFinite", "旋转采样必须为有限数值。", "Rotation samples must be finite.", "Las muestras de rotación deben ser valores finitos.");
            SetMessage(rotation, "RotationCoverage", "旋转角度重复、转动范围不足或特征未发生位移。", "Rotation angles are repeated, cover too little rotation, or the feature did not move.", "Los ángulos se repiten, cubren muy poca rotación o la característica no se movió.");
            SetMessage(rotation, "RotationFitFinite", "旋转拟合结果不是有限数值。", "The fitted rotation is not finite.", "La rotación calculada no es finita.");
        }

        /// <summary>按稳定语言键解析文案，脚本独立运行时仍可使用随附三语底稿。</summary>
        private static void SetMessage(CogToolBlock block, string key, string chinese, string english, string spanish)
        {
            block.UserData["MessageLanguage"] = (int)SysPara.LanguageShow;
            block.UserData["Message." + english] = FrameworkLanguage.LangMsg("Vision", "template_" + key, chinese, english, spanish);
        }
    }
}
