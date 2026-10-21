using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>
    /// 一台相机的页面（每台相机一份）：相机属性条 / 标定卡区 / VPP 子页容器 / 右侧共用显示区。
    /// 运行时由 VPForm 填入：<see cref="CalibHostPanel"/> 放 VpCalibCard，<see cref="DispHost"/> 放 CogRecordDisplay，
    /// <see cref="VppTabs"/> 按配置加入各 VpVppPage。
    /// Designer 中的停靠顺序须保持 vppTabs → calibHost → dispHost → head
    /// （head 52 / 标定卡 268 / vppTabs 填充，右侧显示区 380）。
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
        /// <summary>相机 SN 下拉框：列出整机枚举到的全部相机 SN（见 <see cref="CogCameraDevices"/>），选中即按 SN 绑定。</summary>
        public ComboBox SNBox { get { return cbSN; } }
        public TextBox ExposureBox { get { return tbExposure; } }
        /// <summary>实时显示按钮（文字跟语言 + 实时状态走，由 VPForm.RefreshLiveButton 现算）。</summary>
        public UiButton LiveButton { get { return btnLive; } }
        public UiButton DeleteCameraButton { get { return btnDeleteCamera; } }

        public VpCameraPage()
        {
            InitializeComponent();
            InitSNBox();
        }

        /// <summary>SN 下拉框的自动补全字符串集合：与 Items 分开维护，重建列表时不会打断补全状态。</summary>
        private readonly AutoCompleteStringCollection _snAutoComplete = new AutoCompleteStringCollection();

        /// <summary>最近一次枚举到的设备台数（显示在"相机SN"标签上，方便判断是枚举不到还是没选）。</summary>
        private int _snDeviceCount;

        /// <summary>
        /// 按当前语言设置属性条上的固定文案（新建页面时调用）。「实时显示」按钮的文字见 VPForm.RefreshLiveButton。
        /// </summary>
        public void RefreshTexts()
        {
            try
            {
                lblCapCameraName.Text = LangText("vpCap_CameraName", "相机名称");
                lblCapDefaultExposure.Text = LangText("vpCap_DefaultExposure", "默认曝光");
                btnDeleteCamera.Text = LangText("vpBtn_DeleteCamera", "删除本相机");
                SetSNDeviceCount(_snDeviceCount);
            }
            catch (Exception) { }
        }

        /// <summary>更新"相机SN(2台)"上的台数：下拉框里一共有几台相机可选。</summary>
        public void SetSNDeviceCount(int count)
        {
            _snDeviceCount = count;
            try
            {
                string fmt = LangText("vpCap_CameraSNCount", "相机SN({0}台)");
                lblCapCameraSN.Text = string.Format(fmt, count);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// 相机 SN 下拉框的骨架设置（由 VPForm 负责往里填可用 SN 并接事件）。
        /// 可用 SN 由 <see cref="CogCameraDevices.Enumerate"/> 枚举，文字形如 "#1  TakePCBA  SN:123456"。
        /// 自动补全用自有集合（CustomSource）：列表项会被反复重建，ListItems 模式下边重建边改文本会打断下拉窗口。
        /// </summary>
        private void InitSNBox()
        {
            cbSN.DropDownStyle = ComboBoxStyle.DropDown;   // 也能手输 SN
            cbSN.AutoCompleteMode = AutoCompleteMode.Suggest;
            cbSN.AutoCompleteSource = AutoCompleteSource.CustomSource;
            cbSN.AutoCompleteCustomSource = _snAutoComplete;
        }

        /// <summary>
        /// 用给定设备清单重建下拉框内容并同步补全集合；<paramref name="devices"/> 为空表示一台都没检测到。
        /// 重建前先收起下拉框：下拉窗口还开着的时候改动列表项会让 ComboBox 崩。
        /// </summary>
        public void SetSNItems(IList<CogCameraDevice> devices)
        {
            CloseSNDropDown();
            _snAutoComplete.Clear();

            cbSN.BeginUpdate();
            try
            {
                cbSN.Items.Clear();
                if (devices == null) return;

                for (int i = 0; i < devices.Count; i++)
                {
                    CogCameraDevice d = devices[i];
                    if (d == null) continue;

                    cbSN.Items.Add(d);
                    _snAutoComplete.Add(d.ToString());
                    if (!string.IsNullOrEmpty(d.BindKey) && !_snAutoComplete.Contains(d.BindKey))
                        _snAutoComplete.Add(d.BindKey);
                }
            }
            finally
            {
                cbSN.EndUpdate();
            }

            SetSNDeviceCount(devices == null ? 0 : devices.Count);
        }

        /// <summary>在下拉框里选中某台设备（只改界面，绑定动作由 VPForm 的事件处理）。</summary>
        public void SelectSNDevice(CogCameraDevice device)
        {
            if (device == null) return;
            CloseSNDropDown();
            cbSN.SelectedItem = device;
        }

        /// <summary>下拉框只显示一段文本：还没扫描过、或当前机器上没有这台相机时用。</summary>
        public void ShowSNText(string text)
        {
            CloseSNDropDown();
            cbSN.SelectedIndex = -1;
            cbSN.Text = text ?? "";
        }

        /// <summary>下拉框正在展开就先收起来（收不起来也不能影响后续操作）。</summary>
        private void CloseSNDropDown()
        {
            try
            {
                if (cbSN.DroppedDown) cbSN.DroppedDown = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[VpCameraPage] CloseSNDropDown → " + ex.Message);
            }
        }

        /// <summary>取当前语言的文案（LanguageData\{语言}.xml），没有该键时使用底稿。</summary>
        private static string LangText(string key, string baseText)
        {
            return MiddleLayer.LangText(LangForm, key, baseText);
        }
    }
}
