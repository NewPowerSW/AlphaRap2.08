using System;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>
    /// 一台相机的整页（**每个相机一份**）：相机属性条 / 标定卡区 / VPP 子页容器 / 右侧共用显示区。
    ///
    /// 为什么抽成 UserControl：
    ///   相机页原来是 VPForm 运行时用代码拼的（VS 设计视图里 blank），布局只能改代码。
    ///   现在**骨架写在 VpCameraPage.Designer.cs 里**，VPForm 只按配置实例化；
    ///   VPForm 的设计器里也放了一份"代表页"，所以 VS 里打开 VPForm 也能看到这一页的样子。
    ///
    /// 两个"运行时才填"的容器：
    ///   · <see cref="CalibHostPanel"/> —— VPForm 把设计器定义的 VpCalibCard 塞进来（一台相机一份标定）；
    ///   · <see cref="DispHost"/>       —— VPForm 把 ActiveX 的 CogRecordDisplay 塞进来
    ///     （ActiveX 需要 OcxState 资源，留在代码里建，不搬进设计器）。
    /// VPP 子页由 VPForm 按配置往 <see cref="VppTabs"/> 里加（每个 VPP 一个 VpVppPage）。
    ///
    /// ⚠ 停靠顺序（Dock 是"最后加入的最先停靠"）在 Designer 里必须保持
    ///   `vppTabs → calibHost → dispHost → head`：自上而下 head(52) / 标定卡(268) / vppTabs(填)，右侧一列显示(380)。
    /// </summary>
    public partial class VpCameraPage : UserControl
    {
        /// <summary>宿主页面：参数编辑、实时显示、标定卡等逻辑都在 VPForm 里。</summary>
        public VPForm Owner;

        /// <summary>本页对应的相机配置。</summary>
        public VpCameraConfig Cam;

        private const string LangForm = "VPForm";

        /// <summary>标定卡宿主（VPForm 往里面塞 VpCalibCard）。</summary>
        public Panel CalibHostPanel { get { return calibHost; } }
        /// <summary>本相机的 VPP 子页容器（VPForm 往里面加 TabPage）。</summary>
        public TabControl VppTabs { get { return vppTabs; } }
        /// <summary>右侧显示区（VPForm 往里面塞 CogRecordDisplay）。</summary>
        public Panel DispHost { get { return dispHost; } }

        public TextBox NameBox { get { return tbName; } }
        public TextBox IndexBox { get { return tbIndex; } }
        public TextBox ExposureBox { get { return tbExposure; } }
        /// <summary>实时显示按钮（文字跟语言 + 实时状态走，由 VPForm.RefreshLiveButton 现算）。</summary>
        public UiButton LiveButton { get { return btnLive; } }
        public UiButton DeleteCameraButton { get { return btnDeleteCamera; } }

        public VpCameraPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 属性条上的固定文案按当前语言套一遍（新建的页面当场就该是当前语言 ——
        /// 新建时没人会替它调 SwitchLanguage）。定时刷新由语言表负责。
        /// 「实时显示」按钮的文字是动态的（带状态），不在这里管，见 VPForm.RefreshLiveButton。
        /// </summary>
        public void RefreshTexts()
        {
            try
            {
                lblCapCameraName.Text = LangText("vpCap_CameraName", "相机名称");
                lblCapCameraIndex.Text = LangText("vpCap_CameraIndex", "CameraIndex");
                lblCapDefaultExposure.Text = LangText("vpCap_DefaultExposure", "默认曝光");
                btnDeleteCamera.Text = LangText("vpBtn_DeleteCamera", "删除本相机");
            }
            catch (Exception) { }
        }

        /// <summary>取当前语言的文案：LanguageData\{语言}.xml 是唯一来源，底稿只在键还不存在时兜底。</summary>
        private static string LangText(string key, string baseText)
        {
            return MiddleLayer.LangText(LangForm, key, baseText);
        }
    }
}
