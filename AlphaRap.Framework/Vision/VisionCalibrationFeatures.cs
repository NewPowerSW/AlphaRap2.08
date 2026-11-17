using System;

namespace AlphaRap
{
    public abstract partial class VisionproInterface
    {
        /// <summary>读取内置标定采点方式；原版标定按兼容模板定位显示，自定义算法返回 false。</summary>
        public bool TryGetCalibrationFeature(out VpCalibrationFeature feature, out bool rough)
        {
            lock (_operationLock)
            {
                feature = VpCalibrationFeature.Pattern; rough = false;
                if (!VpAlgorithmTemplates.IsBuiltIn(TB) || !Convert.ToBoolean(TB.UserData["RawCalibrationPixels"])) return false;
                if (!TB.Inputs.Contains("FeatureMode")) return true;
                feature = (VpCalibrationFeature)Convert.ToInt32(TB.Inputs["FeatureMode"].Value);
                rough = Convert.ToBoolean(TB.Inputs["UsePatternFixture"].Value); return true;
            }
        }

        /// <summary>在空闲状态修改当前配方的采点方式和粗定位，升级标准范例后只记录内存草稿。</summary>
        public bool TrySetCalibrationFeature(VpCalibrationFeature feature, bool rough, out string error)
        {
            lock (_operationLock)
            {
                if (SysPara.SystemMode != RunMode.IDLE || _editingThreadId != 0 || !CanModifyToolBlock()) return FailOperation(FrameworkLanguage.LangMsg("Vision", "template_FeatureBusy", "请在工位空闲并关闭算法编辑器后修改采点方式。", "Change the sampling method while idle and after closing the algorithm editor.", "Cambie el método de muestreo en reposo y después de cerrar el editor."), out error);
                if (!Enum.IsDefined(typeof(VpCalibrationFeature), feature)) return FailOperation(FrameworkLanguage.LangMsg("Vision", "template_FeatureModeInvalid", "请选择圆心、两线交点或模板定位。", "Select circle center, line intersection, or pattern location.", "Seleccione centro de círculo, intersección de líneas o localización de patrón."), out error);
                VpCalibrationFeature current; bool currentRough;
                if (!TryGetCalibrationFeature(out current, out currentRough)) return FailOperation(UnsupportedFeatureMessage(), out error);
                if (!TryCreateTemplateTB(TBPath, true, out error)) return false;
                if (!TB.Inputs.Contains("FeatureMode") || !TB.Inputs.Contains("UsePatternFixture")) return FailOperation(UnsupportedFeatureMessage(), out error);
                if (current == feature && currentRough == rough) { error = ""; return true; }
                TB.Inputs["FeatureMode"].Value = (int)feature; TB.Inputs["UsePatternFixture"].Value = rough;
                MarkToolBlockDraft(); LastError = error = ""; return true;
            }
        }

        /// <summary>说明自定义标定算法未被替换，内置方式只适用于标准范例。</summary>
        private static string UnsupportedFeatureMessage()
        {
            return FrameworkLanguage.LangMsg("Vision", "template_FeatureUnsupported", "当前标定包含自定义内容，未自动替换。请在编辑 TB 中配置其精定位算法。", "This calibration has custom content and was preserved. Configure its precision algorithm in Edit TB.", "Esta calibración contiene personalizaciones y se ha conservado. Configure su algoritmo preciso en Editar TB.");
        }
    }
}
