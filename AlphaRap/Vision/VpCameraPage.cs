using System;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>
    /// 一台相机的页面（每台相机一份）：相机属性条 / 标定卡区 / VPP 子页容器。
    /// 蓝色照片窗在 VPForm 级共用，不跟相机页走；本页 <c>dispHost</c> 仅设计器预览用，运行时隐藏。
    /// 运行时由 VPForm 填入：<see cref="CalibHostPanel"/> 放 VpCalibCard，
    /// <see cref="VppTabs"/> 按配置加入各 VpVppPage。
    /// Designer 停靠顺序：vppTabs → calibHost → head（dispHost 运行时隐藏）。
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
        /// <summary>设计器预览用；运行时由 <see cref="HidePageDisplayHost"/> 隐藏。</summary>
        public Panel DispHost { get { return dispHost; } }

        /// <summary>隐藏页内显示区，把宽度还给标定 / VPP（照片窗已提到 VPForm）。</summary>
        public void HidePageDisplayHost()
        {
            if (dispHost == null) return;
            dispHost.Visible = false;
            dispHost.Width = 0;
            dispHost.Dock = DockStyle.None;
        }

        public TextBox NameBox { get { return tbName; } }
        public TextBox IndexBox { get { return tbIndex; } }
        public TextBox ExposureBox { get { return tbExposure; } }
        /// <summary>实时显示按钮（文字跟语言 + 实时状态走，由 VPForm.RefreshLiveButton 现算）。</summary>
        public FlatButton LiveButton { get { return btnLive; } }
        public FlatButton DeleteCameraButton { get { return btnDeleteCamera; } }

        public VpCameraPage()
        {
            InitializeComponent();
            ApplyPageStyle();
        }

        /// <summary>
        /// 按当前语言设置属性条上的固定文案（新建页面时调用）。「实时显示」按钮的文字见 VPForm.RefreshLiveButton。
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

        /// <summary>取当前语言的文案（LanguageData\{语言}.xml），没有该键时使用底稿。</summary>
        private static string LangText(string key, string baseText)
        {
            return MiddleLayer.LangText(LangForm, key, baseText);
        }
    }
}
