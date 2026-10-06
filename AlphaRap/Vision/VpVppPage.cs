using System;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>
    /// 一个 VPP 页（**每个 VPP 一份**，塞进相机的 VPP 子 TabControl 的那一页里）：
    /// 归属行（相机 / Index / VPP 名）→ 底部一条：vpp 路径 / 补偿限制表 / 存图设置 / 参数 / 操作按钮。
    ///
    /// 为什么抽成 UserControl：
    ///   原来这一页是 VPForm 运行时用代码拼的（VS 设计器里看不到），改行高、间距、勾选框位置只能改代码。
    ///   现在**布局写在 VpVppPage.Designer.cs 里**，设计视图看到的就是 EXE 里的样子。
    ///
    /// 分工：本类只管布局、填数据、把控件事件转给 <see cref="Owner"/>(VPForm)；
    ///   业务（拍照 / 保存 VPP / 应用曝光 / 增删限制行 / 落盘）仍在 VPForm 里。
    ///
    /// ⚠ 停靠顺序（Dock 是"最后加入的最先停靠"）在 Designer 里必须保持
    ///   band 内：`path → compRow → saveRow → paramRow → foot`（自上而下就是 路径/补偿表/存图/参数/按钮）。
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
                // 路径打完字失焦才落盘，免得每敲一个字符写一次文件
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

                // 操作按钮（「实时显示」不在这里 —— 它是相机级的，按钮在相机属性条上）
                btnEditTb.Click += delegate { if (Owner != null) Owner.EditStation(Vpp); };
                btnCapture.Click += delegate { if (Owner != null) Owner.RunStation(Vpp); };
                btnSaveVpp.Click += delegate { if (Owner != null) Owner.SaveStation(Vpp); };
                btnAddCompRow.Click += delegate { if (Owner != null) Owner.AddCompLimitRow(gridComp, Vpp); };
                btnDelCompRow.Click += delegate { if (Owner != null) Owner.RemoveCompLimitRow(gridComp, Vpp); };
            }
            catch (Exception) { }
        }

        /// <summary>
        /// 归属行 / 路径行 / 表头 / 标签按钮 按当前语言重算。
        /// · 归属行与路径行带相机名、通道号、配方路径 ⇒ **动态文字**（控件名以 `_dyn` 结尾，不进语言表），
        ///   切语言时由 <c>VPForm.RefreshDynamicTexts</c>（以及换配方时的 OnRecipeChanged）调到这里现算；
        /// · 其余是固定文案，语言表也管，但**新建的页面当场就要是当前语言**（新建时没人替它调 SwitchLanguage）。
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

        /// <summary>取当前语言的文案：LanguageData\{语言}.xml 是唯一来源，底稿只在键还不存在时兜底。</summary>
        private static string LangText(string key, string baseText)
        {
            return MiddleLayer.LangText(LangForm, key, baseText);
        }
    }
}
