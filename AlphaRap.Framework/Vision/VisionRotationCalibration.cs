using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Cognex.VisionPro;
using Cognex.VisionPro.CalibFix;
using Cognex.VisionPro.ToolBlock;

namespace AlphaRap
{
    public abstract partial class VisionproInterface
    {
        /// <summary>进入旋转窗口前确认内置算法已具备九点依赖接口。</summary>
        public bool CanEditRotationCalibration(out string error) { lock (_operationLock) return CanEditRotation(out error); }

        /// <summary>对实际九点模型和当前样本逐点校验，指纹只包含坐标关系，不因旋转样本编辑而改变。</summary>
        internal bool TryGetNinePointStamp(IList<VpCalibPoint> points, string name, out string stamp)
        {
            lock (_operationLock)
            {
                stamp = "";
                CogCalibNPointToNPoint model = RotationNinePointModel();
                if (_isRunning || name != "Calibration" || points == null || model == null || model.NumPoints != points.Count) return false;
                for (int index = 0; index < points.Count; index++)
                {
                    VpCalibPoint point = points[index];
                    if (point == null || point.PixelX != model.GetUncalibratedPointX(index) || point.PixelY != model.GetUncalibratedPointY(index) ||
                        point.MotorPosX != model.GetRawCalibratedPointX(index) || point.MotorPosY != model.GetRawCalibratedPointY(index)) return false;
                }
                stamp = RotationModelStamp(model); return true;
            }
        }

        /// <summary>读取独立的旋转样本，不暴露可修改的 ToolBlock 数组。</summary>
        public IList<VpRotationSample> GetRotationSamples()
        {
            lock (_operationLock)
            {
                if (TB == null || !TB.Inputs.Contains("RotationPixelX")) return new List<VpRotationSample>();
                double[] x = (double[])TB.Inputs["RotationPixelX"].Value, y = (double[])TB.Inputs["RotationPixelY"].Value, angles = (double[])TB.Inputs["RotationAnglesDegrees"].Value;
                if (x == null || y == null || angles == null || x.Length != y.Length || x.Length != angles.Length) return new List<VpRotationSample>();
                return Enumerable.Range(0, x.Length).Select(index => new VpRotationSample(x[index], y[index], angles[index])).ToList();
            }
        }

        /// <summary>添加本次成功拍照的像素和固定角度，记录草稿后该拍照不能再次用于重复添加。</summary>
        internal bool TryAppendRotationSample(VpRotationContext context, long runId, double angle, out string error)
        {
            lock (_operationLock)
            {
                if (!CanEditRotation(out error) || !RotationContextMatches(context, out error)) return false;
                if (_lastRunResult == null || _lastRunResult.RunId != runId || _lastRunResult.State != VisionRunState.Succeeded)
                    return FailOperation(FrameworkLanguage.LangMsg("Vision", "rotation_FreshCapture", "请先在旋转窗口输入实际 R 角度并重新拍照。", "Enter the actual R angle in the rotation window and take a new image first.", "Introduzca el ángulo R real en la ventana de rotación y tome una imagen nueva."), out error);
                List<VpRotationSample> samples = GetRotationSamples().ToList();
                samples.Add(new VpRotationSample(_lastRunResult.X, _lastRunResult.Y, angle));
                return TrySetRotationSamples(context, samples, out error);
            }
        }

        /// <summary>原子更新当前配方的旋转数组；九点关系变化时拒绝把新旧样本混合。</summary>
        internal bool TrySetRotationSamples(VpRotationContext context, IList<VpRotationSample> samples, out string error)
        {
            lock (_operationLock)
            {
                if (!CanEditRotation(out error) || !RotationContextMatches(context, out error)) return false;
                if (samples == null || samples.Any(point => point == null || !RotationFinite(point.PixelX) || !RotationFinite(point.PixelY) || !RotationFinite(point.AngleDegrees)))
                    return FailOperation(FrameworkLanguage.LangMsg("Vision", "rotation_InvalidSample", "像素和实际 R 角度必须是有效数值。", "Pixels and the actual R angle must be finite numbers.", "Los píxeles y el ángulo R real deben ser números finitos."), out error);
                if (samples.Count != 0 && !RotationSamplesMatch(context)) return FailOperation(RotationSamplesChangedMessage(), out error);
                WriteRotationSamples(samples); TB.UserData["Rotation.SampleContext"] = samples.Count == 0 ? "" : context.Key;
                VpAlgorithmTemplates.InvalidateRotation(TB, false); MarkToolBlockDraft(true); LastError = error = ""; return true;
            }
        }

        /// <summary>清空旧样本时不要求九点仍有效，保留九点模型并撤销旋转结果。</summary>
        internal bool TryClearRotationSamples(out string error)
        {
            lock (_operationLock)
            {
                if (!CanEditRotation(out error)) return false;
                WriteRotationSamples(new List<VpRotationSample>()); VpAlgorithmTemplates.InvalidateRotation(TB, true);
                MarkToolBlockDraft(true); LastError = error = ""; return true;
            }
        }

        /// <summary>单独运行旋转中心子块，九点必须有效且所有样本属于同一坐标关系。</summary>
        internal bool TryCalculateRotation(VpRotationContext context, out VpRotationResult result, out string error)
        {
            lock (_operationLock)
            {
                result = null;
                if (!CanEditRotation(out error) || !RotationContextMatches(context, out error)) return false;
                if (!RotationSamplesMatch(context)) return FailOperation(RotationSamplesChangedMessage(), out error);
                VpAlgorithmTemplates.InvalidateRotation(TB, false);
                VpAlgorithmTemplates.Configure(TB, RunLiveCCDSN, RunExposure);
                CogToolBlock rotation = (CogToolBlock)TB.Tools[VpAlgorithmTemplates.RotationToolName];
                rotation.Run();
                if (rotation.RunStatus.Result != CogToolResultConstants.Accept || !(bool)rotation.Outputs["Valid"].Value) return FailOperation(rotation.RunStatus.Message, out error);
                TB.UserData["Rotation.ResultContext"] = context.Key;
                MarkToolBlockDraft(true);
                if (!TryReadRotationResult(context, out result)) return FailOperation(RotationSamplesChangedMessage(), out error);
                LastError = error = ""; return true;
            }
        }

        /// <summary>旋转结果必须仍匹配当前模型及样本来源，旧文件中的数值不自动视为有效。</summary>
        internal bool TryReadRotationResult(VpRotationContext context, out VpRotationResult result)
        {
            lock (_operationLock)
            {
                result = null; string error;
                if (!RotationContextMatches(context, out error) || !RotationSamplesMatch(context) || !TB.Outputs.Contains("MachineCenterX")) return false;
                if (!TB.UserData.Contains("Rotation.ResultContext") || Convert.ToString(TB.UserData["Rotation.ResultContext"]) != context.Key || !(bool)TB.Outputs["RotationValid"].Value) return false;
                result = new VpRotationResult { PixelCenterX = (double)TB.Outputs["CenterX"].Value, PixelCenterY = (double)TB.Outputs["CenterY"].Value,
                    MachineCenterX = (double)TB.Outputs["MachineCenterX"].Value, MachineCenterY = (double)TB.Outputs["MachineCenterY"].Value, RmsPixels = (double)TB.Outputs["RmsError"].Value };
                return new[] { result.PixelCenterX, result.PixelCenterY, result.MachineCenterX, result.MachineCenterY, result.RmsPixels }.All(RotationFinite);
            }
        }

        /// <summary>空样本可开始采集，已有样本必须带有当前九点上下文。</summary>
        internal bool RotationSamplesMatch(VpRotationContext context)
        {
            lock (_operationLock)
                return context != null && (GetRotationSamples().Count == 0 || (TB.UserData.Contains("Rotation.SampleContext") && Convert.ToString(TB.UserData["Rotation.SampleContext"]) == context.Key));
        }

        /// <summary>拒绝在运行、编辑、非空闲或不支持工作流的自定义工具中更改旋转数据。</summary>
        private bool CanEditRotation(out string error)
        {
            if (SysPara.SystemMode != RunMode.IDLE || _isRunning || _editingThreadId != 0) return FailOperation(FrameworkLanguage.LangMsg("Vision", "rotation_Busy", "请先停止视觉运行和编辑，再操作旋转标定。", "Stop vision runs and editing before rotation calibration.", "Detenga las ejecuciones y la edición antes de calibrar la rotación."), out error);
            CogToolBlock rotation = TB != null && TB.Tools.Contains(VpAlgorithmTemplates.RotationToolName) ? TB.Tools[VpAlgorithmTemplates.RotationToolName] as CogToolBlock : null;
            if (!VpAlgorithmTemplates.IsBuiltIn(TB) || rotation == null || !rotation.Inputs.Contains("CalibrationModel") || !TB.Outputs.Contains("MachineCenterX"))
                return FailOperation(FrameworkLanguage.LangMsg("Vision", "rotation_Unsupported", "请先通过编辑 TB 升级标准标定范例；自定义旋转算法不会被自动替换。", "Upgrade the standard calibration through Edit TB first; custom rotation algorithms are preserved.", "Actualice la calibración estándar mediante Editar TB; se conservan los algoritmos personalizados."), out error);
            error = ""; return true;
        }

        /// <summary>再次检查真实模型指纹，避免上下文验证后模型被替换。</summary>
        private bool RotationContextMatches(VpRotationContext context, out string error)
        {
            CogCalibNPointToNPoint model = RotationNinePointModel();
            if (context == null || model == null || RotationModelStamp(model) != context.ModelStamp) return FailOperation(VpCalibration.NinePointRequiredMessage(), out error);
            error = ""; return true;
        }

        /// <summary>读取已计算且至少包含九个点的内置标定模型。</summary>
        private CogCalibNPointToNPoint RotationNinePointModel()
        {
            CogCalibNPointToNPointTool tool = TB != null && TB.Tools.Contains("Calibration") ? TB.Tools["Calibration"] as CogCalibNPointToNPointTool : null;
            if (tool == null || !tool.Calibration.Calibrated || tool.Calibration.NumPoints < 9 || !RotationFinite(tool.Calibration.ComputedRMSError)) return null;
            return tool.Calibration;
        }

        /// <summary>记录完整点对和实际变换，参数和转换关系改变都会使旧旋转数据失效。</summary>
        private static string RotationModelStamp(CogCalibNPointToNPoint model)
        {
            StringBuilder text = new StringBuilder(); text.Append(model.NumPoints);
            for (int index = 0; index < model.NumPoints; index++)
                foreach (double value in new[] { model.GetUncalibratedPointX(index), model.GetUncalibratedPointY(index), model.GetRawCalibratedPointX(index), model.GetRawCalibratedPointY(index) }) text.Append('|').Append(value.ToString("R", CultureInfo.InvariantCulture));
            ICogTransform2D transform = model.GetComputedUncalibratedFromCalibratedTransform().InvertBase();
            foreach (double[] point in new[] { new[] { 0.0, 0.0 }, new[] { 1.0, 0.0 }, new[] { 0.0, 1.0 }, new[] { 100.0, 100.0 } })
            {
                double x, y; transform.MapPoint(point[0], point[1], out x, out y);
                text.Append('|').Append(x.ToString("R", CultureInfo.InvariantCulture)).Append('|').Append(y.ToString("R", CultureInfo.InvariantCulture));
            }
            using (var hash = System.Security.Cryptography.SHA256.Create()) return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(text.ToString()))).Replace("-", "");
        }

        /// <summary>用新的独立数组替换所有采样输入，使原生连接同步到旋转工具。</summary>
        private void WriteRotationSamples(IList<VpRotationSample> samples)
        {
            TB.Inputs["RotationPixelX"].Value = samples.Select(point => point.PixelX).ToArray();
            TB.Inputs["RotationPixelY"].Value = samples.Select(point => point.PixelY).ToArray();
            TB.Inputs["RotationAnglesDegrees"].Value = samples.Select(point => point.AngleDegrees).ToArray();
        }

        /// <summary>识别可用于旋转拟合的有限数据。</summary>
        private static bool RotationFinite(double value) { return !double.IsNaN(value) && !double.IsInfinity(value); }

        /// <summary>提示保留但不能继续混用的旧旋转数据。</summary>
        private static string RotationSamplesChangedMessage()
        {
            return FrameworkLanguage.LangMsg("Vision", "rotation_SamplesChanged", "旋转样本未关联当前九点标定或算法已修改，请清空旋转样本后重新采集。", "Rotation samples do not match the current nine-point calibration or the algorithm changed. Clear them and collect new samples.", "Las muestras no corresponden a la calibración de nueve puntos actual o cambió el algoritmo. Vacíelas y vuelva a tomarlas.");
        }
    }
}
