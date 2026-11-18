using System;
using Cognex.VisionPro;
using Cognex.VisionPro.Caliper;

namespace AlphaRap
{
    /// <summary>试拍图像上的找边设置副本，确认前不修改算法。</summary>
    internal sealed class VpCalibrationSetup
    {
        internal ICogImage Image;
        internal CogCircle Circle;
        internal CogLineSegment Line1, Line2;
        internal long Revision, RunId;
        internal string CameraSN;
        internal double MaxFitRms;
    }

    public abstract partial class VisionproInterface
    {
        /// <summary>取得当前试拍的新图及独立形状，以原始像素编辑无夹具的几何方式。</summary>
        internal bool TryReadCalibrationSetup(long runId, out VpCalibrationSetup setup)
        {
            lock (_operationLock)
            {
                setup = null;
                ICogImage image = ReadCalibrationPhoto(runId);
                if (image == null || !CanEditCalibrationEdges()) return false;

                setup = new VpCalibrationSetup { Image = image.CopyBase(CogImageCopyModeConstants.SharePixels),
                    Revision = _toolBlockRevision, RunId = runId, CameraSN = RunLiveCCDSN, MaxFitRms = (double)TB.Inputs["MaxFitRms"].Value };
                if ((int)TB.Inputs["FeatureMode"].Value == (int)VpCalibrationFeature.Circle)
                {
                    var arc = ((CogFindCircleTool)TB.Tools["FindCircle"]).RunParams.ExpectedCircularArc;
                    setup.Circle = new CogCircle { SelectedSpaceName = image.CoordinateSpaceTree.RootName,
                        CenterX = arc.CenterX, CenterY = arc.CenterY, Radius = arc.Radius,
                        Color = CogColorConstants.Yellow, GraphicDOFEnable = CogCircleDOFConstants.All };
                    return true;
                }
                setup.Line1 = CopyLine("FindLine1", image.CoordinateSpaceTree.RootName, CogColorConstants.Yellow);
                setup.Line2 = CopyLine("FindLine2", image.CoordinateSpaceTree.RootName, CogColorConstants.Cyan);
                return true;
            }
        }

        /// <summary>识别已取图但找边失败的试拍，供界面给出调整提示。</summary>
        internal bool HasCalibrationPhoto(long runId)
        {
            lock (_operationLock) return ReadCalibrationPhoto(runId) != null;
        }

        /// <summary>读取本次完成的图像，忽略取消、采集异常或历史结果。</summary>
        private ICogImage ReadCalibrationPhoto(long runId)
        {
            if (IsBusy || TB == null || !TB.Outputs.Contains("OutputImage") || _lastRunResult == null || _lastRunResult.RunId != runId) return null;
            if (_lastRunResult.State != VisionRunState.Succeeded && _lastRunResult.State != VisionRunState.Rejected &&
                _lastRunResult.State != VisionRunState.Faulted) return null;
            return TB.Outputs["OutputImage"].Value as ICogImage;
        }

        /// <summary>只更新所选特征的形状与拟合误差，其他卡尺配置沿用算法原值。</summary>
        internal bool TryApplyCalibrationSetup(VpCalibrationSetup setup)
        {
            lock (_operationLock)
            {
                if (!CanEditCalibrationEdges() || setup == null || _toolBlockRevision != setup.Revision ||
                    _lastRunResult == null || _lastRunResult.RunId != setup.RunId || RunLiveCCDSN != setup.CameraSN ||
                    !ValidEdgeSetup(setup)) return false;
                if (setup.Circle != null)
                {
                    var arc = ((CogFindCircleTool)TB.Tools["FindCircle"]).RunParams.ExpectedCircularArc;
                    arc.SetCenterRadiusAngleStartAngleSpan(setup.Circle.CenterX, setup.Circle.CenterY, setup.Circle.Radius, arc.AngleStart, arc.AngleSpan);
                }
                else
                {
                    ApplyLine("FindLine1", setup.Line1);
                    ApplyLine("FindLine2", setup.Line2);
                }
                TB.Inputs["MaxFitRms"].Value = setup.MaxFitRms;
                MarkToolBlockDraft();
                return true;
            }
        }

        /// <summary>快捷设置适用于使用原始像素的标准圆和双线算法。</summary>
        private bool CanEditCalibrationEdges()
        {
            VpCalibrationFeature feature;
            bool rough;
            if (SysPara.SystemMode != RunMode.IDLE || IsBusy || !TryGetCalibrationFeature(out feature, out rough) || rough) return false;
            if (!TB.Inputs.Contains("MaxFitRms") || !TB.Outputs.Contains("OutputImage")) return false;
            if (feature == VpCalibrationFeature.Circle) return TB.Tools.Contains("FindCircle") && TB.Tools["FindCircle"] is CogFindCircleTool;
            return feature == VpCalibrationFeature.LineIntersection && TB.Tools.Contains("FindLine1") && TB.Tools["FindLine1"] is CogFindLineTool &&
                TB.Tools.Contains("FindLine2") && TB.Tools["FindLine2"] is CogFindLineTool;
        }

        /// <summary>复制搜索线端点，图形与算法分开保存。</summary>
        private CogLineSegment CopyLine(string name, string space, CogColorConstants color)
        {
            var source = ((CogFindLineTool)TB.Tools[name]).RunParams.ExpectedLineSegment;
            var displayLine = new CogLineSegment { SelectedSpaceName = space, Color = color, GraphicDOFEnable = CogLineSegmentDOFConstants.All };
            displayLine.SetStartEnd(source.StartX, source.StartY, source.EndX, source.EndY);
            return displayLine;
        }

        /// <summary>在原有夹具坐标空间中应用新线段端点，无 PM 时该空间与原始图像一致。</summary>
        private void ApplyLine(string name, CogLineSegment line)
        {
            ((CogFindLineTool)TB.Tools[name]).RunParams.ExpectedLineSegment.SetStartEnd(line.StartX, line.StartY, line.EndX, line.EndY);
        }

        /// <summary>在写入算法前验证像素坐标和拟合门限。</summary>
        private static bool ValidEdgeSetup(VpCalibrationSetup setup)
        {
            if (!RotationFinite(setup.MaxFitRms) || setup.MaxFitRms <= 0) return false;
            if (setup.Circle != null)
                return RotationFinite(setup.Circle.CenterX) && RotationFinite(setup.Circle.CenterY) && RotationFinite(setup.Circle.Radius) && setup.Circle.Radius > 0;
            return ValidSetupLine(setup.Line1) && ValidSetupLine(setup.Line2);
        }

        /// <summary>拒绝无效端点及退化直线。</summary>
        private static bool ValidSetupLine(CogLineSegment line)
        {
            return line != null && RotationFinite(line.StartX) && RotationFinite(line.StartY) && RotationFinite(line.EndX) && RotationFinite(line.EndY) &&
                (line.StartX != line.EndX || line.StartY != line.EndY);
        }
    }
}
