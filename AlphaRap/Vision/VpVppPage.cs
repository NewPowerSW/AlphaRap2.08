using System;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>
    /// VPP 页（每个 VPP 一份，放在相机的 VPP 子 TabControl 中）：归属行（相机 / Index / VPP 名）、
    /// vpp 路径、补偿限制表、存图设置、参数和操作按钮。
    /// 本类负责布局、填充数据并把控件事件转给 <see cref="Owner"/>（VPForm），业务在 VPForm 中处理。
    /// 停靠：head Top（内含 lblInfo Fill + path Right）；pathRow / paramRow 自下而上 Bottom
    ///（paramRow=曝光+VPP名称+操作按钮一行，在存图上方）；compRow Fill。
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
                ApplyPageStyle();
                if (Vpp != null && Vpp.EnsureCompLimits() && Owner != null)
                    Owner.SaveVpConfig();

                RefreshTexts();
                WireEvents();
            }
            catch (Exception) { }
        }

        private void WireEvents()
        {
            gridComp.CellEndEdit += OnCompGridCellEndEdit;

            WireCheck(cbSaveImg, GetSaveImageEnabled, SetSaveImageEnabled);
            WireCheck(cbOrig, GetSaveOriginal, SetSaveOriginal);
            WireCheck(cbSnap, GetSaveSnapshot, SetSaveSnapshot);
            WireCheck(cbAutoDel, GetSaveAutoDelete, SetSaveAutoDelete);

            tbSavePath.Text = (Vpp != null && Vpp.SaveImagePath != null) ? Vpp.SaveImagePath : "";
            tbSavePath.TextChanged += OnSavePathTextChanged;
            tbSavePath.Leave += OnSavePathLeave;
            btnBrowsePath.Click += OnBrowsePathClick;

            tbSaveDays.Text = (Vpp != null) ? Vpp.SaveImageKeepDays.ToString() : "0";
            tbSaveDays.Leave += OnSaveDaysLeave;

            btnEditTb.Click += OnEditTbClick;
            btnCapture.Click += OnCaptureClick;
            btnSaveVpp.Click += OnSaveVppClick;
            btnAddCompRow.Click += OnAddCompRowClick;
            btnDelCompRow.Click += OnDelCompRowClick;
        }

        private void OnCompGridCellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (Owner != null) Owner.SyncCompLimitFromGrid(gridComp, Vpp, e.RowIndex);
        }

        private bool GetSaveImageEnabled() { return Vpp != null && Vpp.SaveImageEnabled; }
        private void SetSaveImageEnabled(bool v)
        {
            if (Vpp == null) return;
            Vpp.SaveImageEnabled = v;
            LogSaveImage("存图", v ? "开" : "关");
        }

        private bool GetSaveOriginal() { return Vpp != null && Vpp.SaveImageOriginal; }
        private void SetSaveOriginal(bool v)
        {
            if (Vpp == null) return;
            Vpp.SaveImageOriginal = v;
            LogSaveImage("原图", v ? "开" : "关");
        }

        private bool GetSaveSnapshot() { return Vpp != null && Vpp.SaveImageSnapshot; }
        private void SetSaveSnapshot(bool v)
        {
            if (Vpp == null) return;
            Vpp.SaveImageSnapshot = v;
            LogSaveImage("截图", v ? "开" : "关");
        }

        private bool GetSaveAutoDelete() { return Vpp != null && Vpp.SaveImageAutoDelete; }
        private void SetSaveAutoDelete(bool v)
        {
            if (Vpp == null) return;
            Vpp.SaveImageAutoDelete = v;
            LogSaveImage("自动删除", v ? "开" : "关");
        }

        private void OnSavePathTextChanged(object sender, EventArgs e)
        {
            if (Vpp != null) Vpp.SaveImagePath = tbSavePath.Text.Trim();
        }

        private void OnSavePathLeave(object sender, EventArgs e)
        {
            if (Owner != null) Owner.SaveVpConfig();
            LogSaveImage("路径", tbSavePath.Text.Trim());
        }

        private void OnBrowsePathClick(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = LangText("vpMsg_BrowseSavePath", "选择存图保存目录");
                string cur = (tbSavePath.Text ?? "").Trim();
                if (cur != "" && System.IO.Directory.Exists(cur))
                    dlg.SelectedPath = cur;

                if (dlg.ShowDialog(FindForm()) != DialogResult.OK) return;

                tbSavePath.Text = dlg.SelectedPath;
                if (Vpp != null) Vpp.SaveImagePath = dlg.SelectedPath;
                if (Owner != null) Owner.SaveVpConfig();
                LogSaveImage("路径", dlg.SelectedPath);
            }
        }

        private void OnSaveDaysLeave(object sender, EventArgs e)
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
            LogSaveImage("保留天数", days.ToString());
        }

        private void OnEditTbClick(object sender, EventArgs e)
        {
            if (Owner != null) Owner.EditStation(Vpp);
        }

        private void OnCaptureClick(object sender, EventArgs e)
        {
            if (Owner != null) Owner.RunStation(Vpp);
        }

        private void OnSaveVppClick(object sender, EventArgs e)
        {
            if (Owner != null) Owner.SaveStation(Vpp);
        }

        private void OnAddCompRowClick(object sender, EventArgs e)
        {
            if (Owner != null) Owner.AddCompLimitRow(gridComp, Vpp);
        }

        private void OnDelCompRowClick(object sender, EventArgs e)
        {
            if (Owner != null) Owner.RemoveCompLimitRow(gridComp, Vpp);
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
                    Owner.FillCompLimitGrid(gridComp, Vpp);
                }

                lblSavePath.Text = LangText("vpRow_SavePath", "存图路径");
                cbSaveImg.Text = LangText("vpRow_SaveImage", "存图");
                cbOrig.Text = LangText("vpRow_SaveOriginal", "原图");
                cbSnap.Text = LangText("vpRow_SaveSnapshot", "截图");
                cbAutoDel.Text = LangText("vpRow_SaveAutoDel", "自动删除");
                lblSaveDays.Text = LangText("vpRow_SaveDays", "保留天数");
                lblExp.Text = LangText("vpRow_Exposure", "曝光");
                lblVppName.Text = LangText("vpRow_VppName", "VPP 名称");
                btnBrowsePath.Text = LangText("vpBtn_BrowsePath", "...");
                btnEditTb.Text = LangText("vpBtn_EditTb", "编辑 TB");
                btnCapture.Text = LangText("vpBtn_Capture", "拍照");
                btnSaveVpp.Text = LangText("vpBtn_SaveVpp", "保存 VPP");
                btnAddCompRow.Text = LangText("vpBtn_AddCompRow", "添加行");
                btnDelCompRow.Text = LangText("vpBtn_DelCompRow", "删除行");

                LayoutParamRow();
                LayoutKeepGroup();
                LayoutTitlePath();
            }
            catch (Exception) { }
        }

        /// <summary>把勾选框接到 VPP 的某个布尔配置上：勾选变化即写回 + 落盘。</summary>
        private void WireCheck(UiCheckBox box, Func<bool> getter, Action<bool> setter)
        {
            if (box == null) return;
            box.Checked = getter();
            box.Tag = new CheckWire(getter, setter);
            box.CheckedChanged += OnSaveCheckChanged;
        }

        private void OnSaveCheckChanged(object sender, EventArgs e)
        {
            UiCheckBox box = sender as UiCheckBox;
            CheckWire wire = (box != null) ? box.Tag as CheckWire : null;
            if (box == null || wire == null) return;
            if (wire.Getter() == box.Checked) return;
            wire.Setter(box.Checked);
            if (Owner != null) Owner.SaveVpConfig();
        }

        private sealed class CheckWire
        {
            public readonly Func<bool> Getter;
            public readonly Action<bool> Setter;
            public CheckWire(Func<bool> getter, Action<bool> setter)
            {
                Getter = getter;
                Setter = setter;
            }
        }

        private void LogSaveImage(string what, string value)
        {
            string cam = (Cam != null) ? Cam.Name : "";
            string vpp = (Vpp != null) ? Vpp.Name : "";
            OperationLog.Write("VPForm", "存图设置：" + cam + " / " + vpp + " " + what + " = " + value);
        }

        private static string LangText(string key, string baseText)
        {
            return MiddleLayer.LangText(LangForm, key, baseText);
        }
    }
}
