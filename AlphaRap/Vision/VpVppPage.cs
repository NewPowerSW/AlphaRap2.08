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
        private bool _refreshingProduction;
        private string _editingItemCaption;

        public UiLabel InfoLabel { get { return lblInfo; } }
        public DataGridView CompGrid { get { return gridComp; } }
        public TextBox ExposureBox { get { return tbExp; } }
        public TextBox RenameBox { get { return tbRename; } }

        /// <summary>创建可在设计器预览的 VPP 配置页面。</summary>
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
                if (Owner != null) Owner.FillCompLimitGrid(gridComp, Vpp);

                RefreshTexts();

                // 补偿限制表只更新草稿；主界面统一保存。
                gridComp.CellEndEdit += delegate(object s, DataGridViewCellEventArgs e)
                {
                    if (Owner != null) Owner.SyncCompLimitFromGrid(gridComp, Vpp, e.RowIndex, e.ColumnIndex);
                    _editingItemCaption = null;
                };

                // 存图设置的独立开关均保留为配置草稿。
                WireCheck(cbSaveImg, delegate { return Vpp.SaveImageEnabled; },
                                     delegate(bool v) { Vpp.SaveImageEnabled = v; LogSaveImage("存图", v ? "开" : "关"); });
                WireCheck(cbOrig, delegate { return Vpp.SaveImageOriginal; },
                                  delegate(bool v) { Vpp.SaveImageOriginal = v; LogSaveImage("原图", v ? "开" : "关"); });
                WireCheck(cbSnap, delegate { return Vpp.SaveImageSnapshot; },
                                  delegate(bool v) { Vpp.SaveImageSnapshot = v; LogSaveImage("截图", v ? "开" : "关"); });
                WireCheck(cbAutoDel, delegate { return Vpp.SaveImageAutoDelete; },
                                     delegate(bool v) { Vpp.SaveImageAutoDelete = v; LogSaveImage("自动删除", v ? "开" : "关"); });

                tbSavePath.Text = (Vpp != null && Vpp.SaveImagePath != null) ? Vpp.SaveImagePath : "";
                tbSavePath.TextChanged += delegate
                {
                    string path = tbSavePath.Text.Trim();
                    if (Vpp == null || Vpp.SaveImagePath == path) return;
                    Vpp.SaveImagePath = path;
                    if (Owner != null) Owner.MarkVpConfigDraft();
                };
                // 失焦时记录已经更新的路径草稿。
                tbSavePath.Leave += delegate
                {
                    LogSaveImage("路径", tbSavePath.Text.Trim());
                };

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
                    if (Owner != null) Owner.MarkVpConfigDraft();
                    LogSaveImage("保留天数", days.ToString());
                };

                // 操作按钮（「实时显示」为相机级，按钮在相机属性条上）
                btnEditTb.Click += delegate { if (Owner != null) Owner.EditStation(Vpp); };
                btnCapture.Click += delegate { if (Owner != null) Owner.RunStation(Vpp); };
                btnAddCompRow.Click += delegate { if (Owner != null) Owner.AddCompLimitRow(gridComp, Vpp); };
                btnDelCompRow.Click += delegate { if (Owner != null) Owner.RemoveCompLimitRow(gridComp, Vpp); };
                _productionCheck.CheckedChanged += delegate
                {
                    if (_refreshingProduction || Owner == null) return;
                    Owner.SetProductionVpp(Vpp, _productionCheck.Checked);
                    RefreshProductionBinding();
                };
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
                    Owner.RefreshCompLimitHeaders(gridComp);
                }

                lblSavePath.Text = LangText("vpRow_SavePath", "存图路径", "Image path", "Ruta de imágenes");
                cbSaveImg.Text = LangText("vpRow_SaveImage", "存图", "Save images", "Guardar imágenes");
                cbOrig.Text = LangText("vpRow_SaveOriginal", "原图", "Original", "Original");
                cbSnap.Text = LangText("vpRow_SaveSnapshot", "截图", "Snapshot", "Captura");
                cbAutoDel.Text = LangText("vpRow_SaveAutoDel", "自动删除", "Auto delete", "Eliminar antiguas");
                lblSaveDays.Text = LangText("vpRow_SaveDays", "保留天数", "Keep days", "Días de retención");
                lblExp.Text = LangText("vpRow_Exposure", "曝光", "Exposure", "Exposición");
                lblVppName.Text = LangText("vpRow_VppName", "VPP 名称", "VPP name", "Nombre VPP");
                btnEditTb.Text = LangText("vpBtn_EditTb", "编辑 TB", "Edit TB", "Editar TB");
                btnCapture.Text = LangText("vpBtn_Capture", "拍照", "Capture", "Capturar");
                btnSaveVpp.Text = LangText("vpBtn_SaveVpp", "保存 VPP", "Save VPP", "Guardar VPP");
                RefreshProductionBinding();
                btnAddCompRow.Text = LangText("vpBtn_AddCompRow", "添加行", "Add row", "Añadir fila");
                btnDelCompRow.Text = LangText("vpBtn_DelCompRow", "删除行", "Delete row", "Eliminar fila");
            }
            catch (Exception) { }
        }

        /// <summary>记录项目开始编辑时的显示文字，切换语言后提交仍能保留原始业务键。</summary>
        private void CompGrid_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            _editingItemCaption = e.ColumnIndex == cItem.Index
                ? Convert.ToString(gridComp.Rows[e.RowIndex].Cells[e.ColumnIndex].FormattedValue) : null;
        }

        /// <summary>只翻译内置角度项目的显示文字，单元格值和配置仍使用原始业务键。</summary>
        private void CompGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex != cItem.Index || Convert.ToString(e.Value) != "角度") return;
            e.Value = LangText("vpCompItem_Angle", "角度", "Angle", "Ángulo");
            e.FormattingApplied = true;
        }

        /// <summary>项目显示文字未被修改时，将编辑器提交的译文还原为原始角度键。</summary>
        private void CompGrid_CellParsing(object sender, DataGridViewCellParsingEventArgs e)
        {
            if (e.ColumnIndex != cItem.Index || e.RowIndex < 0) return;
            VpCompLimitItem item = gridComp.Rows[e.RowIndex].Tag as VpCompLimitItem;
            if (item == null || item.Item != "角度" || Convert.ToString(e.Value) != _editingItemCaption) return;
            e.Value = item.Item;
            e.ParsingApplied = true;
        }

        /// <summary>回填生产定位勾选状态，回填期间不触发保存。</summary>
        private void RefreshProductionBinding()
        {
            _refreshingProduction = true;
            try
            {
                _productionCheck.Text = MiddleLayer.LangMsg(LangForm, "vpRow_Production",
                    "生产定位", "Production positioning", "Posicionamiento de producción");
                _productionCheck.Checked = Owner != null && Owner.IsProductionVpp(Vpp);
            }
            finally { _refreshingProduction = false; }
        }

        /// <summary>把勾选框接到 VPP 配置草稿，等待主界面统一保存。</summary>
        private void WireCheck(UiCheckBox box, Func<bool> getter, Action<bool> setter)
        {
            if (box == null) return;
            box.Checked = getter();
            box.CheckedChanged += delegate
            {
                if (getter() == box.Checked) return;
                setter(box.Checked);
                if (Owner != null) Owner.MarkVpConfigDraft();
            };
        }

        /// <summary>存图设置改动时记一条操作日志（归到 VPForm 目录）。</summary>
        private void LogSaveImage(string what, string value)
        {
            string cam = (Cam != null) ? Cam.Name : "";
            string vpp = (Vpp != null) ? Vpp.Name : "";
            OperationLog.Write("VPForm", "存图设置：" + cam + " / " + vpp + " " + what + " = " + value);
        }

        /// <summary>取当前语言的文案（LanguageData\{语言}.xml），没有该键时使用底稿。</summary>
        private static string LangText(string key, string zh, string en, string es)
        {
            return MiddleLayer.LangMsg(LangForm, key, zh, en, es);
        }
    }
}
