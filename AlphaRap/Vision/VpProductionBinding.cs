using System;

namespace AlphaRap
{
    /// <summary>解析生产定位绑定，验证当前配方的算法与必需补偿限制。</summary>
    public static class VpProductionBinding
    {
        /// <summary>未配置动态绑定时使用兼容工位，明确绑定失败时禁止退回其他算法。</summary>
        public static bool TryResolve(VpConfigFile config, VpStation legacy, string recipe,
            out VpStation station, out string error)
        {
            station = null;
            error = "";
            if (config == null) { error = MiddleLayer.LangMsg("Vision", "binding_ConfigMissing", "视觉配置尚未加载。", "Vision configuration is not loaded.", "La configuración de visión no está cargada."); return false; }
            try { config.ValidateStationIds(); }
            catch (InvalidOperationException ex) { error = ex.Message; return false; }
            if (string.IsNullOrEmpty(config.ProductionVppId))
            {
                station = legacy;
                return ValidateStation(station, recipe, out error);
            }
            VpCameraConfig camera;
            VpVppConfig vpp = config.FindVpp(config.ProductionVppId, out camera);
            if (vpp == null) { error = MiddleLayer.LangMsg("Vision", "binding_TargetMissing", "生产定位绑定的 VPP 已不存在，请重新选择。", "The production VPP no longer exists. Select it again.", "El VPP de producción ya no existe. Selecciónelo de nuevo."); return false; }
            if (string.IsNullOrWhiteSpace(camera.CameraSN)) { error = MiddleLayer.LangMsg("Vision", "binding_CameraMissing", "生产定位相机尚未绑定 SN：", "Production camera SN is not configured: ", "No se ha configurado el SN de la cámara de producción: ") + camera.Name; return false; }
            station = vpp.Station;
            if (!ValidateStation(station, recipe, out error)) return false;
            foreach (string name in new[] { "X", "Y", "角度" })
            {
                VpCompLimitItem limit = vpp.GetCompLimit(name);
                if (limit == null) { error = MiddleLayer.LangMsg("Vision", "binding_LimitMissing", "生产定位缺少补偿限制：", "Production compensation limit is missing: ", "Falta el límite de compensación de producción: ") + name; return false; }
                if (double.IsNaN(limit.Min) || double.IsInfinity(limit.Min) || double.IsNaN(limit.Max)
                    || double.IsInfinity(limit.Max) || limit.Min > limit.Max)
                { error = MiddleLayer.LangMsg("Vision", "binding_LimitInvalid", "生产定位补偿限制无效：", "Production compensation limit is invalid: ", "El límite de compensación de producción no es válido: ") + name; return false; }
            }
            VpCalibration calibration = config.GetCalibration(camera.Name);
            if (calibration != null && !calibration.IsAppliedTo(camera, recipe))
            { error = MiddleLayer.LangMsg("Vision", "binding_CalibrationMismatch", "生产相机的标定与当前配方、相机或算法版本不一致，请重新应用标定。", "Calibration does not match the current recipe, camera or model version. Apply calibration again.", "La calibración no coincide con la receta, cámara o versión del algoritmo. Aplique la calibración de nuevo."); return false; }
            return true;
        }

        /// <summary>工位必须加载当前配方，并采用定位输出契约。</summary>
        private static bool ValidateStation(VpStation station, string recipe, out string error)
        {
            error = "";
            if (station == null) { error = MiddleLayer.LangMsg("Vision", "binding_StationMissing", "生产定位工位尚未创建。", "The production positioning station has not been created.", "No se ha creado la estación de posicionamiento de producción."); return false; }
            if (!station.IsLoadedFrom(station.GetVppPath(recipe))) { error = MiddleLayer.LangMsg("Vision", "binding_RecipeMismatch", "生产定位未加载当前配方：", "The production station has not loaded the current recipe: ", "La estación de producción no ha cargado la receta actual: ") + recipe; return false; }
            if (station.Kind != VpStationKind.Fiducial) { error = MiddleLayer.LangMsg("Vision", "binding_KindMismatch", "生产工位未设置为定位类型。", "The production station is not configured for positioning.", "La estación de producción no está configurada para posicionamiento."); return false; }
            return station.ValidateFiducialOutputs(out error);
        }
    }
}
