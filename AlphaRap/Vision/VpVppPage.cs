using System;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>
    /// VPP 页（每个 VPP 一份，放在相机的 VPP 子 TabControl 中）：归属行（相机 / Index / VPP 名）、
    /// vpp 路径、补偿限制表、存图设置、参数和操作按钮。
    /// 本类负责布局、填充数据并把控件事件转给 <see cref="Owner"/>（VPForm），业务在 VPForm 中处理。
    /// Designer 中 band 内的停靠顺序须保持 path → compRow → saveRow → paramRow → foot。
    /// </summary>
    public partial class VpVppPage : UserControl
    {
        /// <summary>宿主页面：所有操作的实现都在 VPForm 里。</summary>
        public VPForm Owner;
        /// <summary>本页所属相机（归属行要显示相机名与通道号）。</summary>
        public VpCameraConfig Cam;
        /// <summary>本页代表的 VPP（配置对象；页上的勾选/参数都写回它）。</summary>
        public VpVppConfig Vpp;

        private const string LangForm = "VPForm";

        public UiLabel InfoLabel { get { return lblInfo; } }
        public DataGridView CompGrid { get { return gridComp; } }
        public TextBox ExposureBox { get { return tbExp; } }
        public TextBox RenameBox { get { return tbRename; } }

        public VpVppPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 建好这一页后调一次：套当前语言 → 填表格与勾选状态 → 接事件。
        /// Owner / Cam / Vpp 由调用方先设好。
        /// </summary>
        public void Bind()
        {
            try
            {
                UiKit.StyleGrid(gridComp);
                if (Vpp != null) Vpp.EnsureCompLimits();      // 老配置没有限制行时补默认三行

                RefreshTexts();

                // 补偿限制表：编辑落到配置里（解析失败还原、Min&gt;Max 拉回），改完立刻落盘
                gridComp.CellEndEdit += delegate(object s, DataGridViewCellEventArgs e)
                {
                    if (Owner != null) Owner.SyncCompLimitFromGrid(gridComp, Vpp, e.RowIndex);
                };

                // 存图设置：四个勾 = 四个独立开关，改动即时落盘（不跟主页【保存】）
                WireCheck(cbSaveImg, delegate { return Vpp.SaveImageEnabled; },
                                     delegate(bool v) { Vpp.SaveImageEnabled = v; });
                WireCheck(cbOrig, delegate { return Vpp.SaveImageOriginal; },
                                  delegate(bool v) { Vpp.SaveImageOriginal = v; });
                WireCheck(cbSnap, delegate { return Vpp.SaveImageSnapshot; },
                                  delegate(bool v) { Vpp.SaveImageSnapshot = v; });
                WireCheck(cbAutoDel, delegate { return Vpp.SaveImageAutoDelete; },
                                     delegate(bool v) { Vpp.SaveImageAutoDelete = v; });

                tbSavePath.Text = (Vpp != null && Vpp.SaveImagePath != null) ? Vpp.SaveImagePath : "";
                tbSavePath.TextChanged += delegate
                {
                    if (Vpp != null) Vpp.SaveImagePath = tbSavePath.Text.Trim();
                };
                // 路径在失焦时保存
                tbSavePath.Leave += delegate { if (Owner != null) Owner.SaveVpConfig(); };

                tbSaveDays.Text = (Vpp != null) ? Vpp.SaveImageKeepDays.ToString() : "0";
                tbSaveDays.Leave += delegate
                {
                    int days;
                    if (!int.TryParse((tbSaveDays.Text ?? "").Trim(), out days) || days < 0)
                    {
                        tbSaveDays.Text = (Vpp != null) ? Vpp.SaveImageKeepDays.ToString() : "0";
                        return;
                    }
                    if (Vpp == null || days == Vpp.SaveImageKeepDays) return;
                    Vpp.SaveImageKeepDays = days;
                    if (Owner != null) Owner.SaveVpConfig();
                };

                // 操作按钮（「实时显示」为相机级，按钮在相机属性条上）
                btnEditTb.Click += delegate { if (Owner != null) Owner.EditStation(Vpp); };
                btnCapture.Click += delegate { if (Owner != null) Owner.RunStation(Vpp); };
                btnSaveVpp.Click += delegate { if (Owner != null) Owner.SaveStation(Vpp); };
                btnAddCompRow.Click += delegate { if (Owner != null) Owner.AddCompLimitRow(gridComp, Vpp); };
                btnDelCompRow.Click += delegate { if (Owner != null) Owner.RemoveCompLimitRow(gridComp, Vpp); };
            }
            catch (Exception) { }
        }

        /// <summary>
        /// 按当前语言刷新归属行、路径行、表头和标签按钮。归属行与路径行为动态文字（控件名以 _dyn 结尾，不进语言表），
        /// 由 VPForm.RefreshDynamicTexts 和 OnRecipeChanged 调用；新建页面时也调用一次。
        /// </summary>
        public void RefreshTexts()
        {
            try
            {
                if (Owner != null)
                {
                    lblInfo.Text = Owner.BuildVppInfoText(Cam, Vpp);
                    path.Text = VPForm.BuildVppPathText(Vpp);
                    Owner.FillCompLimitGrid(gridComp, Vpp);      // 表头是语言键（vpCompCol_*）
                }

                lblSavePath.Text = LangText("vpRow_SavePath", "存图路径");
                cbSaveImg.Text = LangText("vpRow_SaveImage", "存图");
                cbOrig.Text = LangText("vpRow_SaveOriginal", "原图");
                cbSnap.Text = LangText("vpRow_SaveSnapshot", "截图");
                cbAutoDel.Text = LangText("vpRow_SaveAutoDel", "自动删除");
                lblSaveDays.Text = LangText("vpRow_SaveDays", "保留天数");
                lblExp.Text = LangText("vpRow_Exposure", "曝光");
                lblVppName.Text = LangText("vpRow_VppName", "VPP 名称");
                btnEditTb.Text = LangText("vpBtn_EditTb", "编辑 TB");
                btnCapture.Text = LangText("vpBtn_Capture", "拍照");
                btnSaveVpp.Text = LangText("vpBtn_SaveVpp", "保存 VPP");
                btnAddCompRow.Text = LangText("vpBtn_AddCompRow", "添加行");
                btnDelCompRow.Text = LangText("vpBtn_DelCompRow", "删除行");
            }
            catch (Exception) { }
        }

        /// <summary>把勾选框接到 VPP 的某个布尔配置上：勾选变化即写回 + 落盘（与补偿限制表同一口径）。</summary>
        private void WireCheck(UiCheckBox box, Func<bool> getter, Action<bool> setter)
        {
            if (box == null) return;
            box.Checked = getter();
            box.CheckedChanged += delegate
            {
                if (getter() == box.Checked) return;
                setter(box.Checked);
                if (Owner != null) Owner.SaveVpConfig();
            };
        }

        /// <summary>取当前语言的文案（LanguageData\{语言}.xml），没有该键时使用底稿。</summary>
        private static string LangText(string key, string baseText)
        {
            return MiddleLayer.LangText(LangForm, key, baseText);
        }
    }
}
