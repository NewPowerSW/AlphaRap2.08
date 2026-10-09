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

        public DataGridView GridPoints { get { return gridPoints; } }
        public TextBox ExposureBox { get { return tbExposure; } }
        public UiLabel TitleLabel { get { return lblTitle; } }
        public UiLabel TipLabel { get { return lblTip; } }

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

                // 单元格编辑完成后立即写回标定点并保存
                gridPoints.CellEndEdit += delegate(object s, DataGridViewCellEventArgs e)
                {
                    if (Owner != null) Owner.OnCalibGridEdited(this, e);
                };

                // 标定曝光：改一下就进内存 + 落盘；输入不合法时失焦还原
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
            }
            catch (Exception) { }
        }

        /// <summary>取当前语言的文案（LanguageData\{语言}.xml），没有该键时使用底稿。</summary>
        private static string LangText(string key, string baseText)
        {
            return MiddleLayer.LangText(LangForm, key, baseText);
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
