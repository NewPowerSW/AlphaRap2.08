using System;
using System.Windows.Forms;

namespace AlphaRap
{
    public partial class VPForm
    {
        /// <summary>读取旋转步骤的九点前置状态，界面显示与实际进入时使用同一校验。</summary>
        internal bool CanStartRotation(VpCalibCard card, out string error)
        {
            CameraEntry entry = EntryOf(card);
            if (entry != null && card.Calib != null) return card.Calib.CanStartRotation(entry.Config, SysPara.RecipeName, out error);
            error = VpCalibration.NinePointRequiredMessage(); return false;
        }

        /// <summary>通过九点校验后准备标准旋转工作流，编辑窗口只保留草稿。</summary>
        internal void OpenRotationCalibration(VpCalibCard card)
        {
            if (card == null || !EnsureIdle(T("vpAct_Edit", "编辑视觉算法"))) return;
            string error;
            if (!CanStartRotation(card, out error)) { MessageBox.Show(error, T("vpMsg_CalibrationTitle", "标定")); return; }
            VpStation station = card.Calib.Station;
            if (!station.TryCreateTemplateTB(station.GetVppPath(SysPara.RecipeName), true, out error)) { MessageBox.Show(error, T("vpMsg_CalibrationTitle", "标定")); return; }
            if (!station.CanEditRotationCalibration(out error)) { MessageBox.Show(error, T("vpMsg_CalibrationTitle", "标定")); return; }
            using (VpRotationForm form = new VpRotationForm(card.Calib, EntryOf(card).Config, SysPara.RecipeName)) form.ShowDialog(this);
            card.RefreshTexts();
        }

        /// <summary>切换标定采点定义前要求清空旧样本，防止不同特征坐标混入同一批标定点。</summary>
        internal void ChangeCalibrationFeature(VpCalibCard card)
        {
            if (card == null || card.Calib == null) return;
            try
            {
                string error;
                if (!TryChangeCalibrationFeature(card, out error)) MessageBox.Show(error, T("vpMsg_CalibrationTitle", "标定"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally { card.RefreshFeatureSelection(); }
        }

        /// <summary>修改当前配方内存算法的采点模式，不重建相机配置或写入文件。</summary>
        internal bool TryChangeCalibrationFeature(VpCalibCard card, out string error)
        {
            error = "";
            if (card == null || card.Calib == null || card.Calib.Station == null) return false;
            if (card.Calib.Points.Count != 0)
            {
                error = FrameworkLanguage.LangMsg("VPForm", "vpMsg_FeatureHasSamples", "请先清空采样，再切换采点方式或 PM 粗定位，随后重新采集标定点。", "Clear the samples before changing the sampling method or PM fixture, then collect new calibration points.", "Vacíe las muestras antes de cambiar el método o la localización PM y vuelva a tomar los puntos.");
                return false;
            }
            VpCalibrationFeature current;
            bool rough;
            if (!card.Calib.Station.TryGetCalibrationFeature(out current, out rough)) return false;
            rough = card.SelectedFeature != VpCalibrationFeature.Pattern && rough;
            if (!card.Calib.Station.TrySetCalibrationFeature(card.SelectedFeature, rough, out error)) return false;
            MarkVpConfigDraft(); return true;
        }

        /// <summary>打开当前特征的图形设置，确认为当前配方的算法草稿。</summary>
        internal void OpenCalibrationSetup(VpCalibCard card, long previewRun, string previewSN)
        {
            if (card == null || card.Calib == null || card.Calib.Station == null || !EnsureIdle(T("vpAct_Edit", "编辑视觉算法"))) return;
            VpStation station = card.Calib.Station;
            VpCalibrationFeature feature;
            bool rough;
            if (!station.TryGetCalibrationFeature(out feature, out rough) || rough || feature == VpCalibrationFeature.Pattern)
            {
                ShowCalibrationHint(card, "vpGuide_UseAdvanced", "当前采点方式请用高级 TB 设置。快捷找边支持圆心和两线交点（不启用 PM 跟随）。", "Use Advanced TB for this method. Quick edge setup supports circles and two lines without PM tracking.", "Use TB avanzado para este método. El ajuste rápido admite círculos y dos líneas sin seguimiento PM.");
                return;
            }
            if (card.Calib.Points.Count != 0)
            {
                ShowCalibrationHint(card, "vpGuide_EdgesWithSamples", "已有标定点。若要调整找边，请先清空采样，再重新采点。", "Clear existing points before adjusting the edges, then collect new points.", "Vacíe los puntos antes de ajustar los bordes y tome puntos nuevos.");
                return;
            }
            VpCalibrationSetup setup;
            if (previewSN != station.RunLiveCCDSN || !station.TryReadCalibrationSetup(previewRun, out setup))
            {
                ShowCalibrationHint(card, "vpGuide_NeedPhoto", "请先试拍取得照片，再设置找边。即使本次未找到特征，也可以用这张照片设置。", "Take a test shot first. You can set up edges on the photo even if the feature was not found.", "Tome primero una foto de prueba. Puede ajustar los bordes aunque no se encuentre la característica.");
                return;
            }
            using (VpCalibrationSetupForm form = new VpCalibrationSetupForm(setup))
                if (form.ShowDialog(this) != DialogResult.OK) return;
            if (!station.IsLoadedFrom(station.GetVppPath(SysPara.RecipeName)) || !station.TryApplyCalibrationSetup(setup))
            {
                ShowCalibrationHint(card, "vpGuide_EdgesInvalid", "未应用找边设置。检查圆和线的位置，或重新试拍后再设置。", "Edge settings were not applied. Check the shapes or take a fresh test shot and try again.", "No se aplicó el ajuste. Revise las figuras o tome otra foto de prueba.");
                return;
            }
            MarkVpConfigDraft();
            card.ResetCaptureHint(true);
        }

        /// <summary>拒绝占用中的相机，启动一次试拍或九点采样。</summary>
        internal bool TryStartCalibrationCapture(VpCalibCard card, out long runId, out string cameraSN, out string error)
        {
            runId = 0;
            cameraSN = "";
            error = "";
            CameraEntry entry = EntryOf(card);
            if (entry == null || card.Calib == null || card.Calib.Station == null || !EnsureIdle(T("vpAct_Capture", "手动拍照"))) return false;
            if (IsLiveRunning(entry))
            {
                error = FrameworkLanguage.LangMsg("VPForm", "vpGuide_StopLive", "请先关闭实时显示，再试拍或采点。", "Stop live view before taking a test shot or collecting a point.", "Detenga la vista en directo antes de probar o tomar un punto.");
                return false;
            }
            VpStation station = card.Calib.Station;
            cameraSN = station.RunLiveCCDSN;
            if (station.TryRunTB(out runId)) return true;
            error = station.LastError;
            return false;
        }

        /// <summary>把成功拍照与预先输入的位置配对，只写当前相机和配方的采样表。</summary>
        internal bool TryRecordCalibrationPoint(VpCalibCard card, VisionRunResult result, VpCalibPoint point, string recipe, string cameraSN)
        {
            CameraEntry entry = EntryOf(card);
            if (entry == null || entry.Config == null || card.Calib == null || point == null || result.State != VisionRunState.Succeeded) return false;
            VpStation station = card.Calib.Station;
            if (station == null || !ReferenceEquals(result, station.LastRunResult) || recipe != SysPara.RecipeName || entry.Config.CameraSN != cameraSN) return false;
            card.Calib.Points.Add(new VpCalibPoint(result.X, result.Y, point.MotorPosX, point.MotorPosY));
            card.Calib.AppliedSignature = "none";
            FillCalibrationGrid(card);
            int last = card.GridPoints.Rows.Count - 1;
            if (last >= 0) card.GridPoints.FirstDisplayedScrollingRowIndex = last;
            MarkVpConfigDraft();
            return true;
        }

        /// <summary>用当前语言提示相机页的操作条件。</summary>
        private static void ShowCalibrationHint(VpCalibCard card, string key, string zh, string en, string es)
        {
            MessageBox.Show(card, FrameworkLanguage.LangMsg("VPForm", key, zh, en, es));
        }

        /// <summary>明确确认后清空当前采样表，保留已经计算的标定模型，等待重新采点、应用及主保存。</summary>
        internal void ClearCalibrationSamples(VpCalibCard card)
        {
            if (card == null || card.Calib == null || card.Calib.Points.Count == 0) return;
            if (!EnsureIdle(T("vpAct_Point", "修改标定点"))) return;
            string prompt = FrameworkLanguage.LangMsg("VPForm", "vpMsg_ClearCalibSamples", "清空当前标定采样点吗？已有标定模型保留，重新采点并应用后由主界面保存。", "Clear the calibration samples? Existing models are retained. Collect new points, apply calibration, and use Main Save.", "¿Vaciar las muestras? Se conservan los modelos. Tome nuevos puntos, aplique la calibración y use Guardar en la ventana principal.");
            if (MessageBox.Show(prompt, T("vpMsg_CalibrationTitle", "标定"), MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK) return;
            card.Calib.Points.Clear(); card.Calib.AppliedSignature = "none";
            if (card.Calib.Station != null) card.Calib.Station.MarkToolBlockDraft();
            FillCalibrationGrid(card); MarkVpConfigDraft(); card.RefreshTexts(); card.RefreshFeatureSelection();
        }
    }
}
