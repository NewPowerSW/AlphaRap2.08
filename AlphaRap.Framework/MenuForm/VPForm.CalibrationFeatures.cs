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
            bool rough = card.SelectedFeature != VpCalibrationFeature.Pattern && card.UseRoughLocation;
            if (!card.Calib.Station.TrySetCalibrationFeature(card.SelectedFeature, rough, out error)) return false;
            MarkVpConfigDraft(); return true;
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
