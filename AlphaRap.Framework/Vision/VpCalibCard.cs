using System;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>
    /// 标定卡（一台相机一张）：标题 / 标定点表格 / 路径提示 / 标定曝光 / 操作按钮。
    /// 本类负责布局、填充数据并把控件事件转给 <see cref="Owner"/>（VPForm），标定业务在 VPForm 中处理；
    /// 相机入口存放在 Tag 中，由 VPForm 读写。
    /// 曝光和找边设置在上方，试拍及采点结果在下方。
    /// </summary>
    public partial class VpCalibCard : UserControl
    {
        /// <summary>宿主页面：所有操作的实现都在 VPForm 里。</summary>
        public VPForm Owner;

        /// <summary>本相机的标定配置；null = 还没建标定（此时只显示"添加标定"）。</summary>
        public VpCalibration Calib;

        /// <summary>语言包里的分组名（= 窗体名）。</summary>
        private const string LangForm = "VPForm";
        private bool refreshingFeature, edgesUpdated;
        private VpStation captureStation;
        private long pendingRun, previewRun;
        private string pendingRecipe, pendingSN;
        private VpCalibPoint pendingPoint;
        private VisionRunResult captureResult;

        public DataGridView GridPoints { get { return gridPoints; } }
        public TextBox ExposureBox { get { return tbExposure; } }
        public UiLabel TitleLabel { get { return lblTitle; } }
        public UiLabel TipLabel { get { return lblTip; } }
        internal VpCalibrationFeature SelectedFeature { get { return (VpCalibrationFeature)cmbFeature.SelectedIndex; } }

        public VpCalibCard()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 建好卡片后调一次：按当前语言套文字 → 填表格 → 接事件 → 按"有没有标定"显示按钮。
        /// 相机入口（Tag）、Calib、Owner 由调用方先设好。
        /// </summary>
        public void Bind()
        {
            try
            {
                UiKit.StyleGrid(gridPoints);      // 扁平化外观（表头/边框）
                RefreshTexts();
                if (Owner != null) Owner.FillCalibrationGrid(this);

                // 标定曝光回填：已建标定用标定的曝光，没有标定时用相机的默认曝光
                // （不先回填的话输入框是空的，看起来像"参数没加载"）
                tbExposure.Text = DefaultExposure().ToString("F1");
                RefreshFeatureSelection();
                cmbFeature.SelectedIndexChanged += FeatureSelectionChanged;
                btnClearSamples.Click += delegate { if (Owner != null) Owner.ClearCalibrationSamples(this); };
                btnRotationStep.Click += delegate { if (Owner != null) Owner.OpenRotationCalibration(this); };
                btnSetup.Click += delegate { if (Owner != null) Owner.OpenCalibrationSetup(this, previewRun, pendingSN); };
                tbMotorX.TextChanged += CoordinateChanged;
                tbMotorY.TextChanged += CoordinateChanged;
                captureStation = Calib == null ? null : Calib.Station;
                if (captureStation != null) captureStation.RunCompleted += CaptureCompleted;

                // 单元格编辑完成后写入标定点草稿，统一由主界面保存。
                gridPoints.CellEndEdit += delegate(object s, DataGridViewCellEventArgs e)
                {
                    if (Owner != null) Owner.OnCalibGridEdited(this, e);
                };

                // 合法曝光写入内存草稿，输入不合法时失焦还原。
                tbExposure.TextChanged += delegate
                {
                    if (Owner != null) Owner.ApplyCalibrationExposure(this);
                };
                tbExposure.Leave += delegate
                {
                    double v;
                    if (double.TryParse((tbExposure.Text ?? "").Trim(), out v) && v > 0) return;
                    tbExposure.Text = DefaultExposure().ToString("F1");
                };

                // 有标定 → 5 个操作按钮；没有 → 只留"添加标定"（FlowLayoutPanel 自动跳过隐藏项）
                bool has = (Calib != null);
                btnAddCalibration.Visible = !has;
                btnEditTb.Visible = has;
                btnCapture.Visible = has;
                btnAddPoint.Visible = has;
                btnDeletePoint.Visible = has;
                btnApplyToCamera.Visible = has;
                btnSetup.Visible = has;
                coordinateRow.Visible = has;
                btnClearSamples.Visible = has;

                btnAddCalibration.Click += delegate { if (Owner != null) Owner.AddCalibration(this); };
                btnEditTb.Click += delegate { if (Owner != null) Owner.EditCalibrationTB(this); };
                btnCapture.Click += delegate { StartCapture(false); };
                btnAddPoint.Click += delegate { StartCapture(true); };
                btnDeletePoint.Click += delegate { if (Owner != null) Owner.RemoveCalibrationPoint(this); };
                btnApplyToCamera.Click += delegate { if (Owner != null) Owner.ApplyCalibration(this); };
            }
            catch (Exception) { }
        }

        /// <summary>
        /// 按当前语言刷新标题、提示和固定文案。标题和提示为动态文字（控件名以 _dyn 结尾，不进语言表），
        /// 由 VPForm.RefreshDynamicTexts 调用；新建卡片时也调用一次。
        /// </summary>
        public void RefreshTexts()
        {
            try
            {
                if (Owner != null)
                {
                    lblTitle.Text = Owner.BuildCalibTitleText(this);
                    help.SetToolTip(btnEditTb, Owner.BuildCalibTipText(this));
                }

                lblExpCaption.Text = LangText("vpRow_CalibExposure", "标定曝光");
                btnAddCalibration.Text = LangText("vpBtn_AddCalibration", "添加标定");
                btnEditTb.Text = Guide("vpGuide_Advanced", "高级 TB", "Advanced TB", "TB avanzado");
                btnCapture.Text = Guide("vpGuide_Preview", "试拍", "Test shot", "Probar foto");
                btnAddPoint.Text = Guide("vpGuide_Collect", "采集此点", "Collect point", "Tomar punto");
                btnSetup.Text = Guide("vpGuide_Edges", "设置找边", "Set up edges", "Ajustar bordes");
                lblMotorX.Text = Guide("vpGuide_MotorX", "实际机械 X (mm)", "Actual X (mm)", "X real (mm)");
                lblMotorY.Text = Guide("vpGuide_MotorY", "实际机械 Y (mm)", "Actual Y (mm)", "Y real (mm)");
                btnDeletePoint.Text = LangText("vpBtn_DeletePoint", "删除选中点");
                btnApplyToCamera.Text = Guide("vpGuide_ApplyNine", "应用九点", "Apply 9 points", "Aplicar 9 puntos");
                cPx.HeaderText = Guide("vpCalibGrid_cPx", "像素 X (px)", "Image X (px)", "Imagen X (px)");
                cPy.HeaderText = Guide("vpCalibGrid_cPy", "像素 Y (px)", "Image Y (px)", "Imagen Y (px)");
                cMx.HeaderText = Guide("vpCalibGrid_cMx", "机械 X (mm)", "Machine X (mm)", "Máquina X (mm)");
                cMy.HeaderText = Guide("vpCalibGrid_cMy", "机械 Y (mm)", "Machine Y (mm)", "Máquina Y (mm)");
                RefreshFeatureTexts();
                RefreshRotationStep();
                RefreshCaptureControls();
            }
            catch (Exception) { }
        }

        /// <summary>九点可以单独结束；只有当前九点仍有效时才开放可选的旋转步骤。</summary>
        private void RefreshRotationStep()
        {
            lblNineStep.Text = FrameworkLanguage.LangMsg(LangForm, "vpStep_Nine", "1 九点标定", "1 Nine-point calibration", "1 Calibración de nueve puntos");
            btnRotationStep.Text = FrameworkLanguage.LangMsg(LangForm, "vpStep_Rotation", "2 旋转标定（可选）", "2 Rotation (optional)", "2 Rotación (opcional)");
            string reason; bool ready = Owner != null && Owner.CanStartRotation(this, out reason);
            btnRotationStep.Enabled = ready && pendingRun == 0;
            lblStepState.Text = ready ? FrameworkLanguage.LangMsg(LangForm, "vpStep_NineReady", "九点有效，可保存结束或继续旋转", "Nine points valid: finish with Save or continue rotation", "Nueve puntos válidos: guarde o continúe con rotación")
                : FrameworkLanguage.LangMsg(LangForm, "vpStep_NineRequired", "旋转前请先完成九点并应用", "Complete and apply nine points before rotation", "Complete y aplique nueve puntos antes de rotar");
        }

        /// <summary>从当前配方算法回填采点设置，失败和语言刷新均不触发编辑事件。</summary>
        internal void RefreshFeatureSelection()
        {
            refreshingFeature = true;
            try
            {
                VpCalibrationFeature feature = VpCalibrationFeature.Circle; bool rough = false;
                bool available = Calib != null && Calib.Station != null && Calib.Station.TryGetCalibrationFeature(out feature, out rough);
                cmbFeature.SelectedIndex = Enum.IsDefined(typeof(VpCalibrationFeature), feature) ? (int)feature : -1;
                cmbFeature.Enabled = available && pendingRun == 0;
                btnClearSamples.Enabled = Calib != null && Calib.Points.Count > 0;
            }
            finally { refreshingFeature = false; }
        }

        /// <summary>只更新固定选项的译文，保留选择、曝光和表格编辑值。</summary>
        private void RefreshFeatureTexts()
        {
            int selected = cmbFeature.SelectedIndex; refreshingFeature = true;
            try
            {
                lblFeature.Text = FrameworkLanguage.LangMsg(LangForm, "vpRow_CalibFeature", "采点方式", "Sampling", "Muestreo");
                btnClearSamples.Text = FrameworkLanguage.LangMsg(LangForm, "vpBtn_ClearCalibSamples", "清空采样", "Clear samples", "Vaciar muestras");
                cmbFeature.Items[0] = FrameworkLanguage.LangMsg(LangForm, "vpFeature_Circle", "圆心", "Circle center", "Centro de círculo");
                cmbFeature.Items[1] = FrameworkLanguage.LangMsg(LangForm, "vpFeature_Lines", "两线交点", "Line intersection", "Intersección de líneas");
                cmbFeature.Items[2] = FrameworkLanguage.LangMsg(LangForm, "vpFeature_Pattern", "模板定位（兼容）", "Pattern (legacy)", "Patrón (compatible)");
                cmbFeature.SelectedIndex = selected;
                btnClearSamples.Enabled = Calib != null && Calib.Points.Count > 0;
            }
            finally { refreshingFeature = false; }
        }

        /// <summary>由宿主验证采样状态并修改算法草稿，更新失败时恢复原选择。</summary>
        private void FeatureSelectionChanged(object sender, EventArgs e)
        {
            if (refreshingFeature || Owner == null) return;
            Owner.ChangeCalibrationFeature(this);
            ResetCaptureHint();
        }

        /// <summary>试拍仅用于看图，采点将输入坐标绑定到新拍照。</summary>
        private void StartCapture(bool collect)
        {
            if (Owner == null || pendingRun != 0) return;
            double x = 0, y = 0;
            if (collect && !ReadCoordinates(out x, out y)) return;
            string error;
            if (!Owner.TryStartCalibrationCapture(this, out pendingRun, out pendingSN, out error))
            {
                if (!string.IsNullOrEmpty(error)) MessageBox.Show(this, error);
                return;
            }
            pendingRecipe = SysPara.RecipeName;
            pendingPoint = collect ? new VpCalibPoint(0, 0, x, y) : null;
            captureResult = null;
            RefreshTexts();
        }

        /// <summary>在 UI 线程消费本卡发起的运行编号，保存与这次拍摄对应的坐标。</summary>
        private void CaptureCompleted(VisionRunResult result)
        {
            if (IsDisposed || !IsHandleCreated) return;
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => CaptureCompleted(result)));
                return;
            }
            if (result.RunId != pendingRun) return;
            pendingRun = 0;
            previewRun = result.RunId;
            captureResult = result;
            VpCalibPoint point = pendingPoint;
            pendingPoint = null;
            if (point != null && result.State == VisionRunState.Succeeded &&
                Owner.TryRecordCalibrationPoint(this, result, point, pendingRecipe, pendingSN))
            {
                tbMotorX.Clear();
                tbMotorY.Clear();
                tbMotorX.Focus();
            }
            RefreshTexts();
        }

        /// <summary>设置或配方变化后清除本次试拍状态。</summary>
        internal void ResetCaptureHint(bool settingsApplied = false)
        {
            previewRun = 0;
            captureResult = null;
            edgesUpdated = settingsApplied;
            RefreshTexts();
        }

        /// <summary>释放卡片时结束该卡对运行完成通知的订阅。</summary>
        private void UnbindCapture()
        {
            if (captureStation != null) captureStation.RunCompleted -= CaptureCompleted;
        }

        /// <summary>输入完实际位置后更新采样按钮。</summary>
        private void CoordinateChanged(object sender, EventArgs e) { RefreshCaptureControls(); }

        /// <summary>只接受两个明确填写的有限机械坐标，零也是合法坐标。</summary>
        private bool ReadCoordinates(out double x, out double y)
        {
            bool hasX = VpConfigStore.TryParseInputDouble(tbMotorX.Text, out x);
            bool hasY = VpConfigStore.TryParseInputDouble(tbMotorY.Text, out y);
            return hasX && hasY;
        }

        /// <summary>按是否有图、是否已满九点及本次拍照结果提示下一步。</summary>
        private void RefreshCaptureControls()
        {
            bool has = Calib != null && Calib.Station != null;
            int count = has ? Calib.Points.Count : 0;
            bool busy = pendingRun != 0;
            double x, y;
            btnCapture.Enabled = has && !busy;
            btnSetup.Enabled = has && !busy;
            btnEditTb.Enabled = has && !busy;
            btnAddPoint.Enabled = has && !busy && ReadCoordinates(out x, out y);
            btnApplyToCamera.Enabled = has && !busy && count >= 9;
            btnClearSamples.Enabled = btnDeletePoint.Enabled = has && !busy && count != 0;
            tbMotorX.Enabled = tbMotorY.Enabled = tbExposure.Enabled = gridPoints.Enabled = !busy;
            VpCalibrationFeature feature; bool rough;
            cmbFeature.Enabled = has && !busy && Calib.Station.TryGetCalibrationFeature(out feature, out rough);
            lblTip.Text = CaptureHint(has, busy, count);
            help.SetToolTip(lblTip, string.IsNullOrWhiteSpace(captureResult?.Error) ? lblTip.Text : lblTip.Text + "\r\n" + captureResult.Error);
        }

        /// <summary>显示试拍或采样结果以及接下来的操作。</summary>
        private string CaptureHint(bool has, bool busy, int count)
        {
            if (!has) return Guide("vpGuide_Create", "选择相机后，点击添加标定。", "Select a camera, then add calibration.", "Elija una cámara y añada la calibración.");
            if (busy) return Guide("vpGuide_Capturing", "正在拍照，请保持位置不动。", "Capturing. Hold the current position.", "Capturando. Mantenga la posición.");
            if (captureResult != null && captureResult.State != VisionRunState.Succeeded && Calib.Station.HasCalibrationPhoto(captureResult.RunId))
                return Guide("vpGuide_NoFeature", "有照片，未找到特征。检查特征是否在视野内，或打开设置找边。", "Photo captured. Check that the feature is in view, or open Set up edges.", "Foto capturada. Compruebe que la característica sea visible o ajuste los bordes.");
            if (captureResult != null && captureResult.State != VisionRunState.Succeeded)
                return Guide("vpGuide_Failed", "本次未采点。", "No point was collected.", "No se tomó un punto.") + " " + captureResult.Error;
            if (count >= 9) return Guide("vpGuide_Enough", "点数已满九个。检查点位后点击应用九点，再在主界面保存或继续旋转。", "Nine or more points. Review them, apply calibration, then save or continue with rotation.", "Revise los puntos, aplique la calibración y guarde o continúe con la rotación.");
            if (captureResult != null)
                return Guide("vpGuide_Found", "已找到特征。在视野内分散取点，每次输入实际 X/Y，再点击采集此点。", "Feature found. Spread points across the image. Enter actual X/Y at each position and collect the point.", "Característica encontrada. Reparta los puntos en la imagen. Introduzca X/Y reales y tome cada punto.");
            if (edgesUpdated) return Guide("vpGuide_EdgesReady", "找边已设置。可先试拍确认，再输入实际 X/Y 采点。", "Edges set. Take a test shot, then enter actual X/Y to collect points.", "Bordes ajustados. Tome una foto de prueba y luego introduzca X/Y reales.");
            return Guide("vpGuide_FirstShot", "选择圆心或两线交点，试拍取图，再设置找边。", "Select a circle or line intersection, take a test shot, then set up edges.", "Elija círculo o intersección, tome una foto de prueba y ajuste los bordes.");
        }

        /// <summary>读取引导标签和状态的三语文案。</summary>
        private static string Guide(string key, string zh, string en, string es) { return FrameworkLanguage.LangMsg(LangForm, key, zh, en, es); }

        /// <summary>取当前语言的文案（LanguageData\{语言}.xml），没有该键时使用底稿。</summary>
        private static string LangText(string key, string baseText)
        {
            return FrameworkLanguage.LangText(LangForm, key, baseText);
        }

        /// <summary>曝光框的默认值：标定的曝光，没有标定时用相机的默认曝光（未设置时为 10）。</summary>
        private double DefaultExposure()
        {
            if (Calib != null) return Calib.Exposure;
            if (Owner != null) return Owner.CameraDefaultExposure(this);
            return 10;
        }
    }
}
