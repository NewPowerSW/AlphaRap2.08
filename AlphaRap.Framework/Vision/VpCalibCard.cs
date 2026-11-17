using System;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>
    /// 标定卡（一台相机一张）：标题 / 标定点表格 / 路径提示 / 标定曝光 / 操作按钮。
    /// 本类负责布局、填充数据并把控件事件转给 <see cref="Owner"/>（VPForm），标定业务在 VPForm 中处理；
    /// 相机入口存放在 Tag 中，由 VPForm 读写。
    /// Designer 中的停靠顺序须保持 grid → tip → expRow → btns → title（Dock 后加入的先停靠）。
    /// </summary>
    public partial class VpCalibCard : UserControl
    {
        /// <summary>宿主页面：所有操作的实现都在 VPForm 里。</summary>
        public VPForm Owner;

        /// <summary>本相机的标定配置；null = 还没建标定（此时只显示"添加标定"）。</summary>
        public VpCalibration Calib;

        /// <summary>语言包里的分组名（= 窗体名）。</summary>
        private const string LangForm = "VPForm";
        private bool refreshingFeature;

        public DataGridView GridPoints { get { return gridPoints; } }
        public TextBox ExposureBox { get { return tbExposure; } }
        public UiLabel TitleLabel { get { return lblTitle; } }
        public UiLabel TipLabel { get { return lblTip; } }
        internal VpCalibrationFeature SelectedFeature { get { return (VpCalibrationFeature)cmbFeature.SelectedIndex; } }
        internal bool UseRoughLocation { get { return cbRough.Checked; } }

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
                cbRough.CheckedChanged += FeatureSelectionChanged;
                btnClearSamples.Click += delegate { if (Owner != null) Owner.ClearCalibrationSamples(this); };

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

                btnAddCalibration.Click += delegate { if (Owner != null) Owner.AddCalibration(this); };
                btnEditTb.Click += delegate { if (Owner != null) Owner.EditCalibrationTB(this); };
                btnCapture.Click += delegate { if (Owner != null) Owner.SnapCalibration(this); };
                btnAddPoint.Click += delegate { if (Owner != null) Owner.AddCalibrationPoint(this); };
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
                    lblTip.Text = Owner.BuildCalibTipText(this);
                }

                lblExpCaption.Text = LangText("vpRow_CalibExposure", "标定曝光");
                btnAddCalibration.Text = LangText("vpBtn_AddCalibration", "添加标定");
                btnEditTb.Text = LangText("vpBtn_EditTb", "编辑 TB");
                btnCapture.Text = LangText("vpBtn_Capture", "拍照");
                btnAddPoint.Text = LangText("vpBtn_AddPoint", "添加标定点");
                btnDeletePoint.Text = LangText("vpBtn_DeletePoint", "删除选中点");
                btnApplyToCamera.Text = LangText("vpBtn_ApplyToCamera", "应用到本相机");
                RefreshFeatureTexts();
            }
            catch (Exception) { }
        }

        /// <summary>从当前配方算法回填采点设置，失败和语言刷新均不触发编辑事件。</summary>
        internal void RefreshFeatureSelection()
        {
            refreshingFeature = true;
            try
            {
                VpCalibrationFeature feature = VpCalibrationFeature.Circle; bool rough = false;
                bool available = Calib != null && Calib.Station != null && Calib.Station.TryGetCalibrationFeature(out feature, out rough);
                cmbFeature.SelectedIndex = (int)feature; cbRough.Checked = rough;
                cmbFeature.Enabled = available; cbRough.Enabled = available && feature != VpCalibrationFeature.Pattern;
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
                cbRough.Text = FrameworkLanguage.LangMsg(LangForm, "vpCb_CalibRough", "PM 粗定位", "PM rough locate", "Localización PM");
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
        }

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
