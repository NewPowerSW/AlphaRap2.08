using System;
using System.Linq;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>当前相机与配方的可选旋转步骤，只操作算法草稿；取消恢复进入窗口前的算法。</summary>
    public partial class VpRotationForm : Form
    {
        private readonly VpCalibration calibration;
        private readonly VpCameraConfig camera;
        private readonly string recipe;
        private readonly VisionproInterface.ToolBlockSnapshot original;
        private bool filling, restored, released;
        private long pendingRun, capturedRun;
        private double pendingAngle, capturedAngle;

        /// <summary>设计器初始化不连接设备或执行算法。</summary>
        public VpRotationForm() { InitializeComponent(); }

        /// <summary>捕获进入旋转步骤前的内存草稿并绑定现有采样。</summary>
        public VpRotationForm(VpCalibration calibration, VpCameraConfig camera, string recipe) : this()
        {
            this.calibration = calibration; this.camera = camera; this.recipe = recipe;
            original = calibration.Station.CaptureToolBlockSnapshot();
            UiTheme.ApplyDpi(this); UiKit.StyleGrid(gridSamples);
            btnCapture.Click += CaptureClicked; btnAdd.Click += AddClicked; btnDelete.Click += DeleteClicked;
            btnClear.Click += ClearClicked; btnCalculate.Click += CalculateClicked;
            btnDone.Click += delegate { if (gridSamples.EndEdit()) { DialogResult = DialogResult.OK; Close(); } };
            btnCancel.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };
            tbAngle.TextChanged += delegate { RefreshState(); };
            gridSamples.CellEndEdit += AngleEdited;
            FormClosing += ClosingRotation; FormClosed += RotationClosed;
            calibration.Station.RunCompleted += CaptureCompleted;
            FrameworkLanguage.LanguageChanged += LanguageChanged;
            RefreshTexts();
            FrameworkLanguage.RegisterAndApplyLanguage(this, "VpRotationForm");
            FillSamples();
        }

        /// <summary>刷新三语文案时保留角度输入、表格行和正在编辑的单元格。</summary>
        private void RefreshTexts()
        {
            Text = T("title", "2 旋转标定（可选）", "2 Rotation calibration (optional)", "2 Calibración de rotación (opcional)");
            lblInstructions.Text = T("lblInstructions", "九点有效后，保持相机和 XYZ 不动，仅旋转 R 轴。选偏离旋转中心的同一特征，至少采三组不同角度；每次输入实际 R 角度后拍照，再添加采样。", "After valid nine-point calibration, keep camera and XYZ fixed and rotate R only. Track the same feature away from the rotation center at three or more angles. Enter the actual R angle before each capture, then add the sample.", "Con nueve puntos válidos, mantenga cámara y XYZ fijos y gire solo R. Siga la misma característica alejada del centro en tres ángulos o más. Introduzca el ángulo R real antes de capturar y añada la muestra.");
            lblAngle.Text = T("lblAngle", "实际 R 角度（°）", "Actual R angle (degrees)", "Ángulo R real (grados)");
            btnCapture.Text = T("btnCapture", "拍照", "Capture", "Capturar");
            btnAdd.Text = T("btnAdd", "添加采样", "Add sample", "Añadir muestra");
            btnDelete.Text = T("btnDelete", "删除选中点", "Delete selected", "Eliminar punto");
            btnClear.Text = T("btnClear", "清空旋转采样", "Clear samples", "Vaciar muestras");
            btnCalculate.Text = T("btnCalculate", "计算旋转中心", "Calculate center", "Calcular centro");
            btnDone.Text = T("btnDone", "完成", "Done", "Finalizar");
            btnCancel.Text = T("btnCancel", "取消修改", "Cancel changes", "Cancelar cambios");
            cPixelX.HeaderText = T("gridSamples_cPixelX", "像素 X", "Pixel X", "Píxel X");
            cPixelY.HeaderText = T("gridSamples_cPixelY", "像素 Y", "Pixel Y", "Píxel Y");
            cAngle.HeaderText = T("gridSamples_cAngle", "实际 R 角度（°）", "Actual R angle (degrees)", "Ángulo R real (grados)");
            RefreshState();
        }

        /// <summary>只在样本实际改变时重新填表，语言切换不重建数据行。</summary>
        private void FillSamples()
        {
            filling = true;
            try
            {
                gridSamples.Rows.Clear();
                foreach (VpRotationSample point in calibration.GetRotationSamples()) gridSamples.Rows.Add(point.PixelX.ToString("F3"), point.PixelY.ToString("F3"), point.AngleDegrees.ToString("R"));
            }
            finally { filling = false; }
            RefreshState();
        }

        /// <summary>用当前九点和采样关系控制操作权限，禁止把新角度与旧拍照配对。</summary>
        private void RefreshState()
        {
            if (calibration == null || calibration.Station == null) return;
            bool busy = calibration.Station.IsBusy; string error;
            bool ready = !busy && calibration.CanStartRotation(camera, recipe, out error);
            bool sameSamples = ready && calibration.RotationSamplesMatch(camera, recipe);
            double angle; bool angleValid = VpConfigStore.TryParseInputDouble(tbAngle.Text, out angle);
            tbAngle.Enabled = !busy; btnCapture.Enabled = sameSamples && angleValid;
            btnAdd.Enabled = sameSamples && capturedRun != 0 && angleValid && angle == capturedAngle;
            btnDelete.Enabled = sameSamples && gridSamples.Rows.Count > 0;
            btnClear.Enabled = !busy && gridSamples.Rows.Count > 0;
            btnCalculate.Enabled = sameSamples && gridSamples.Rows.Count >= 3;
            btnDone.Enabled = !busy; btnCancel.Enabled = !busy; gridSamples.ReadOnly = !sameSamples;
            if (busy) { lblResult.Text = T("busy", "正在拍照，请保持轴位置不变。", "Capturing; keep the axes stationary.", "Capturando; mantenga los ejes inmóviles."); return; }
            if (!ready) { lblResult.Text = VpCalibration.NinePointRequiredMessage(); return; }
            if (!sameSamples) { lblResult.Text = T("stale", "旧旋转样本与当前九点或算法不一致，请清空后重新采集。", "Old samples do not match the current nine-point calibration or algorithm. Clear and recollect them.", "Las muestras antiguas no corresponden a la calibración o algoritmo actual. Vacíelas y vuelva a tomarlas."); return; }
            VpRotationResult result;
            if (calibration.TryGetRotationResult(camera, recipe, out result))
            {
                lblResult.Text = string.Format(T("result", "机械中心 X={0:F4}，Y={1:F4} mm；像素中心 ({2:F3}, {3:F3})，RMS={4:F4} px。\r\n点击完成保留草稿，再由主界面 Save 保存。", "Machine center X={0:F4}, Y={1:F4} mm; pixel center ({2:F3}, {3:F3}), RMS={4:F4} px.\r\nDone keeps the draft; use Main Save to persist it.", "Centro mecánico X={0:F4}, Y={1:F4} mm; centro en píxeles ({2:F3}, {3:F3}), RMS={4:F4} px.\r\nFinalizar conserva el borrador; use Guardar en la ventana principal."), result.MachineCenterX, result.MachineCenterY, result.PixelCenterX, result.PixelCenterY, result.RmsPixels);
                return;
            }
            lblResult.Text = T("pending", "待采集并计算。完成只保留内存草稿，主界面 Save 才写入文件。", "Collect samples and calculate. Done retains the in-memory draft; Main Save writes the file.", "Tome las muestras y calcule. Finalizar conserva el borrador; Guardar en la ventana principal escribe el archivo.");
        }

        /// <summary>拍照前固定输入角度；采集完成前锁定角度及编辑操作。</summary>
        private void CaptureClicked(object sender, EventArgs e)
        {
            double angle; string error;
            if (!VpConfigStore.TryParseInputDouble(tbAngle.Text, out angle) || !calibration.CanStartRotation(camera, recipe, out error)) { ShowError(VpCalibration.NinePointRequiredMessage()); return; }
            pendingAngle = angle; capturedRun = 0;
            if (!calibration.Station.TryRunTB(out pendingRun)) ShowError(calibration.Station.LastError);
            RefreshState();
        }

        /// <summary>只接收本窗口启动的拍照结果，配对时采用拍照前固定的实际 R 角度。</summary>
        private void CaptureCompleted(VisionRunResult result)
        {
            if (IsDisposed || !IsHandleCreated || result.RunId != pendingRun) return;
            if (InvokeRequired) { BeginInvoke(new Action(() => CaptureCompleted(result))); return; }
            capturedRun = result.State == VisionRunState.Succeeded ? result.RunId : 0; capturedAngle = pendingAngle;
            RefreshState();
        }

        /// <summary>添加一次当前成功拍照，加入草稿后必须重新拍照才能继续添加。</summary>
        private void AddClicked(object sender, EventArgs e)
        {
            string error;
            if (!calibration.TryAddRotationSample(camera, recipe, capturedRun, capturedAngle, out error)) { ShowError(error); return; }
            capturedRun = 0; FillSamples();
        }

        /// <summary>删除选中的旋转样本，不改变九点标定。</summary>
        private void DeleteClicked(object sender, EventArgs e)
        {
            if (gridSamples.CurrentRow == null) return;
            var samples = calibration.GetRotationSamples(); int index = gridSamples.CurrentRow.Index;
            if (index < 0 || index >= samples.Count) return;
            samples.RemoveAt(index); string error;
            if (!calibration.TrySetRotationSamples(camera, recipe, samples, out error)) { ShowError(error); return; }
            capturedRun = 0; FillSamples();
        }

        /// <summary>确认后清空旧旋转样本及结果，保留九点模型。</summary>
        private void ClearClicked(object sender, EventArgs e)
        {
            if (MessageBox.Show(T("confirmClear", "清空本配方的旋转样本和结果吗？九点标定保留。", "Clear rotation samples and results for this recipe? Nine-point calibration is retained.", "¿Vaciar muestras y resultados de rotación de esta receta? Se conserva la calibración de nueve puntos."), Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK) return;
            string error;
            if (!calibration.TryClearRotationSamples(out error)) { ShowError(error); return; }
            capturedRun = 0; FillSamples();
        }

        /// <summary>提交正在编辑的角度后计算旋转中心，不执行轴运动或拍照。</summary>
        private void CalculateClicked(object sender, EventArgs e)
        {
            if (!gridSamples.EndEdit()) return;
            VpRotationResult result; string error;
            if (!calibration.TryCalculateRotation(camera, recipe, out result, out error)) ShowError(error);
            capturedRun = 0; RefreshState();
        }

        /// <summary>修正实际 R 角度时保留原像素精度，非法值恢复旧样本。</summary>
        private void AngleEdited(object sender, DataGridViewCellEventArgs e)
        {
            if (filling || e.RowIndex < 0 || e.ColumnIndex != 2) return;
            var samples = calibration.GetRotationSamples();
            if (e.RowIndex >= samples.Count) return;
            double angle; string error;
            if (!VpConfigStore.TryParseInputDouble(Convert.ToString(gridSamples.Rows[e.RowIndex].Cells[2].Value), out angle)) { FillSamples(); return; }
            VpRotationSample prior = samples[e.RowIndex]; samples[e.RowIndex] = new VpRotationSample(prior.PixelX, prior.PixelY, angle);
            if (!calibration.TrySetRotationSamples(camera, recipe, samples, out error)) ShowError(error);
            capturedRun = 0; FillSamples();
        }

        /// <summary>采集结束前不关闭窗口；取消或关闭恢复原算法，完成仅保留草稿。</summary>
        private void ClosingRotation(object sender, FormClosingEventArgs e)
        {
            if (calibration.Station.IsBusy) { e.Cancel = true; DialogResult = DialogResult.None; return; }
            if (DialogResult == DialogResult.OK) return;
            calibration.Station.RestoreToolBlockSnapshot(original); restored = true;
        }

        /// <summary>解除静态语言和运行事件订阅，释放未转回工位的快照。</summary>
        private void RotationClosed(object sender, FormClosedEventArgs e)
        {
            ReleaseRotationResources();
        }

        /// <summary>未显示就释放窗口时也解除订阅，已转回工位的快照不在窗口中释放。</summary>
        private void ReleaseRotationResources()
        {
            if (released) return;
            released = true;
            if (calibration != null && calibration.Station != null) calibration.Station.RunCompleted -= CaptureCompleted;
            FrameworkLanguage.LanguageChanged -= LanguageChanged;
            if (!restored && original != null && original.ToolBlock != null) original.ToolBlock.Dispose();
        }

        /// <summary>通过框架语言事件刷新当前显示文字。</summary>
        private void LanguageChanged(object sender, EventArgs e) { if (!IsDisposed) RefreshTexts(); }

        /// <summary>按当前语言显示操作失败原因。</summary>
        private void ShowError(string error) { MessageBox.Show(error, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); RefreshState(); }

        /// <summary>按稳定键读取三语文案并保留现场翻译。</summary>
        private static string T(string key, string zh, string en, string es) { return FrameworkLanguage.LangMsg("VpRotationForm", key, zh, en, es); }
    }
}
