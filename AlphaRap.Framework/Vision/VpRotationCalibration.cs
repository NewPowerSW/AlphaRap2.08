using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AlphaRap
{
    /// <summary>同一特征的一次像素采样及 R 轴实际角度，角度单位为度。</summary>
    public sealed class VpRotationSample
    {
        public double PixelX { get; private set; }
        public double PixelY { get; private set; }
        public double AngleDegrees { get; private set; }
        /// <summary>建立不与算法输入数组共享存储的旋转采样。</summary>
        public VpRotationSample(double pixelX, double pixelY, double angleDegrees) { PixelX = pixelX; PixelY = pixelY; AngleDegrees = angleDegrees; }
    }

    /// <summary>旋转中心同时提供原始像素和九点转换后的机械坐标；RMS 的单位为像素。</summary>
    public sealed class VpRotationResult
    {
        public double PixelCenterX { get; internal set; }
        public double PixelCenterY { get; internal set; }
        public double MachineCenterX { get; internal set; }
        public double MachineCenterY { get; internal set; }
        public double RmsPixels { get; internal set; }
    }

    internal sealed class VpRotationContext
    {
        internal string Key, ModelStamp;
    }

    public partial class VpCalibration
    {
        /// <summary>只有本相机、当前配方的九点样本及全部目标模型仍有效时才允许进入旋转步骤。</summary>
        public bool CanStartRotation(VpCameraConfig camera, string recipe, out string error)
        {
            VpRotationContext context; return TryGetRotationContext(camera, recipe, out context, out error);
        }

        /// <summary>取得旋转样本副本；读取旧数据不表示它仍匹配当前九点标定。</summary>
        public IList<VpRotationSample> GetRotationSamples() { return Station == null ? new List<VpRotationSample>() : Station.GetRotationSamples(); }

        /// <summary>把指定成功拍照与其实际 R 角度配成一个样本，已有样本必须关联同一九点关系。</summary>
        public bool TryAddRotationSample(VpCameraConfig camera, string recipe, long runId, double angle, out string error)
        {
            VpRotationContext context;
            if (!TryGetRotationContext(camera, recipe, out context, out error)) return false;
            return Station.TryAppendRotationSample(context, runId, angle, out error);
        }

        /// <summary>修改当前旋转样本的角度或删除点，修改后必须重新计算中心。</summary>
        public bool TrySetRotationSamples(VpCameraConfig camera, string recipe, IList<VpRotationSample> samples, out string error)
        {
            VpRotationContext context;
            if (!TryGetRotationContext(camera, recipe, out context, out error)) return false;
            return Station.TrySetRotationSamples(context, samples, out error);
        }

        /// <summary>清除当前算法内的旋转样本和结果；九点失效时也允许清除旧样本。</summary>
        public bool TryClearRotationSamples(out string error)
        {
            if (Station != null) return Station.TryClearRotationSamples(out error);
            error = NinePointRequiredMessage(); return false;
        }

        /// <summary>只运行旋转数学工具，不采图或移动轴，成功结果仍等待 Main Save。</summary>
        public bool TryCalculateRotation(VpCameraConfig camera, string recipe, out VpRotationResult result, out string error)
        {
            result = null; VpRotationContext context;
            if (!TryGetRotationContext(camera, recipe, out context, out error)) return false;
            return Station.TryCalculateRotation(context, out result, out error);
        }

        /// <summary>仅返回仍关联当前相机、配方及九点模型的旋转结果。</summary>
        public bool TryGetRotationResult(VpCameraConfig camera, string recipe, out VpRotationResult result)
        {
            result = null; VpRotationContext context; string error;
            return TryGetRotationContext(camera, recipe, out context, out error) && Station.TryReadRotationResult(context, out result);
        }

        /// <summary>判断旧旋转样本是否仍属于当前九点关系，失效样本只能查看和清空。</summary>
        public bool RotationSamplesMatch(VpCameraConfig camera, string recipe)
        {
            VpRotationContext context; string error;
            return TryGetRotationContext(camera, recipe, out context, out error) && Station.RotationSamplesMatch(context);
        }

        /// <summary>接受同次九点应用的内存草稿或已保存版本，拒绝换相机、换配方、改单点及目标算法修改。</summary>
        private bool TryGetRotationContext(VpCameraConfig camera, string recipe, out VpRotationContext context, out string error)
        {
            context = null; error = NinePointRequiredMessage();
            if (camera == null || string.IsNullOrWhiteSpace(camera.CameraSN) || string.IsNullOrWhiteSpace(recipe) || recipe != SysPara.RecipeName) return false;
            if (Station == null || Points.Count < 9 || !IsApplied || AppliedRecipe != recipe || AppliedCameraSN != camera.CameraSN) return false;
            if (!Station.IsLoadedFrom(Station.GetVppPath(recipe)) || Station.RunLiveCCDSN != camera.CameraSN) return false;
            if (AppliedModels.Count != camera.Vpps.Count) return false;
            if (camera.Vpps.Any(vpp => !RotationTargetMatches(vpp, recipe))) return false;
            string model;
            if (!Station.TryGetNinePointStamp(Points, ToolName, out model)) return false;
            string identity = camera.CameraSN.Length + ":" + camera.CameraSN + "|" + recipe.Length + ":" + recipe + "|" + ComputeSignature() + "|" + model;
            context = new VpRotationContext { Key = identity, ModelStamp = model }; error = ""; return true;
        }

        /// <summary>确认九点发布记录仍对应同一目标的保存摘要或应用草稿版本。</summary>
        private bool RotationTargetMatches(VpVppConfig vpp, string recipe)
        {
            string expected; VpStation station = vpp.Station;
            if (station == null || !station.IsLoadedFrom(station.GetVppPath(recipe)) || !AppliedModels.TryGetValue(vpp.Id, out expected)) return false;
            if (expected == "draft:" + station.ToolBlockRevision.ToString(CultureInfo.InvariantCulture)) return true;
            return !station.HasToolBlockDraft && !string.IsNullOrEmpty(expected) && expected == station.ModelSignature;
        }

        /// <summary>说明旋转的九点前置条件及需要重新应用的范围。</summary>
        internal static string NinePointRequiredMessage()
        {
            return FrameworkLanguage.LangMsg("Vision", "rotation_NinePointRequired", "请先在当前相机和配方完成至少九点标定，并成功应用到本相机。九点、相机或配方变化后需要重新应用。", "Complete at least nine points and apply calibration to this camera and recipe first. Reapply after point, camera, or recipe changes.", "Complete al menos nueve puntos y aplique la calibración a esta cámara y receta. Vuelva a aplicar tras cambiar puntos, cámara o receta.");
        }
    }
}
