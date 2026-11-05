using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AlphaRapLibrary;

namespace AlphaRap
{
    /// <summary>
    /// 托盘参数页（**表格式**）：一行 = 一组托盘，单元格直接改数值。
    ///
    /// 表格行是按配方表 PSet 的列**动态发现**的（见 <see cref="TrayConfig.BuildRows"/>），不是写死的；
    /// 上面的「新托盘名称 + 添加」会按名字建出 &lt;名称&gt;RowNum / ColumnNum / Rowdis / ColumnDis 四个配方列
    /// （写完立刻回存配方文件），行随即出现在表格里 —— 所以加一个托盘不用改代码也不用动设计器。
    /// 参数值存在配方 PSet 里，其它模块要读请用 <see cref="TrayConfig"/>。
    /// </summary>
    public partial class PlatFormSetting : ModuleBaseForm
    {
        private const int ToolRowHeight = 54;      // 96 DPI 设计值（运行期新增控件由 ApplyDpi 的钩子自动放大）
        private const int StatusRowHeight = 26;

        private DataGridView grdTray;
        private FieldBox boxNewName;
        private FlatButton btnAddTray;
        private FlatButton btnDelTray;
        private UiLabel lblStatus;

        private List<TrayRowSpec> _rowSpecs = new List<TrayRowSpec>();
        private bool _fillingGrid;

        public PlatFormSetting()
        {
            InitializeComponent();
            UiTheme.ApplyDpi(this);
            if (!UiTheme.IsDesignTime) ReadSettingData();
            ApplyPageLanguage();
        }

        /// <summary>
        /// 页面必须在 <c>_ModuleName</c> 定下来之后才建：基类用它定位配方/设置里的命名空间，
        /// 而表格行是按配方表 PSet 的列生成的。
        /// </summary>
        public override void ModuleInitialize(string ModuleName)
        {
            base.ModuleInitialize(ModuleName);
            try { MiddleLayer.LanguageChanged += PlatFormSetting_LanguageChanged; } catch { }
            RebuildTrayTable();
        }

        public override void ModuleDispose()
        {
            try { MiddleLayer.LanguageChanged -= PlatFormSetting_LanguageChanged; } catch { }
            base.ModuleDispose();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // 静态事件必须解绑，否则已释放的页面会被语言切换一直拉住
            try { MiddleLayer.LanguageChanged -= PlatFormSetting_LanguageChanged; } catch { }
            base.OnFormClosed(e);
        }

        private void PlatFormSetting_LanguageChanged(object sender, EventArgs e)
        {
            ApplyPageLanguage();
            if (boxNewName == null) return;
            Control[] labels = pnlBody.Controls.Find("lblNewTrayName", true);
            if (labels.Length > 0) labels[0].Text = T("newName", "新托盘名称", "New tray", "Nueva bandeja");
            boxNewName.PlaceholderText = T("newNameHint", "例如 RejectTray2", "e.g. RejectTray2", "p. ej. RejectTray2");
            btnAddTray.Text = T("addTray", "+ 添加托盘", "+ Add tray", "+ Añadir");
            btnDelTray.Text = T("delTray", "删除选中", "Delete", "Eliminar");
        }

        // ---------------------------------------------------------------- 建页

        /// <summary>按当前配方表重建整页（配方换了 / 增删了托盘 / 切了语言都走这里）。</summary>
        public void RebuildTrayTable()
        {
            if (pnlBody == null) return;

            pnlBody.SuspendLayout();
            try
            {
                pnlBody.Controls.Clear();
                pnlBody.Controls.Add(BuildTableCard());
            }
            catch { }
            finally
            {
                pnlBody.ResumeLayout(true);
            }
        }

        /// <summary>托盘表格卡：顶部「新托盘名称 + 添加 / 删除」，下面是可编辑表格。</summary>
        private Control BuildTableCard()
        {
            CardPanel card = new CardPanel();
            card.Dock = DockStyle.Fill;
            card.Margin = new Padding(0);
            card.Padding = new Padding(16, 0, 16, 12);

            // ---- 工具行
            FlowLayoutPanel tools = new FlowLayoutPanel();
            tools.Dock = DockStyle.Top;
            tools.Height = ToolRowHeight;
            tools.BackColor = UiKit.Surface;
            tools.FlowDirection = FlowDirection.LeftToRight;
            tools.WrapContents = false;
            tools.Padding = new Padding(0, 12, 0, 0);

            UiLabel lblNew = MakeLabel("lblNewTrayName", T("newName", "新托盘名称", "New tray", "Nueva bandeja"), 96,
                ContentAlignment.MiddleLeft);
            lblNew.Margin = new Padding(0, 0, 6, 0);
            tools.Controls.Add(lblNew);

            boxNewName = new FieldBox();
            boxNewName.Name = "boxNewTrayName_dyn";
            boxNewName.Size = new Size(190, 34);
            boxNewName.Margin = new Padding(0, 0, 10, 0);
            boxNewName.PlaceholderText = T("newNameHint", "例如 RejectTray2", "e.g. RejectTray2", "p.ej. RejectTray2");
            tools.Controls.Add(boxNewName);

            btnAddTray = new FlatButton();
            btnAddTray.Name = "btnAddTray";
            btnAddTray.Variant = FlatButtonVariant.Primary;
            btnAddTray.CornerRadius = 8;
            btnAddTray.IconSize = 16;
            btnAddTray.Size = new Size(128, 34);
            btnAddTray.Margin = new Padding(0, 0, 8, 0);
            btnAddTray.Text = T("addTray", "+ 添加托盘", "+ Add tray", "+ Anadir");
            btnAddTray.Click += btnAddTray_Click;
            tools.Controls.Add(btnAddTray);

            btnDelTray = new FlatButton();
            btnDelTray.Name = "btnDelTray";
            btnDelTray.Variant = FlatButtonVariant.Surface;
            btnDelTray.CornerRadius = 8;
            btnDelTray.IconSize = 16;
            btnDelTray.Size = new Size(128, 34);
            btnDelTray.Margin = new Padding(0);
            btnDelTray.Text = T("delTray", "删除选中", "Delete", "Eliminar");
            btnDelTray.Click += btnDelTray_Click;
            tools.Controls.Add(btnDelTray);

            // ---- 表格
            grdTray = new DataGridView();
            grdTray.Name = "grdTray";
            grdTray.Dock = DockStyle.Fill;
            grdTray.AllowUserToAddRows = false;
            grdTray.AllowUserToDeleteRows = false;
            grdTray.AllowUserToOrderColumns = false;
            grdTray.MultiSelect = false;
            grdTray.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
            // 表头一律用代码里的名字（= 配方列后缀），**不参与中英文切换**：
            // 表里的行列是参数名/数据，不是界面文案，跟着语言变会让人对不上代码。
            grdTray.Columns.Add(MakeColumn("colTrayName", "TrayName", 220, true));
            grdTray.Columns.Add(MakeColumn("colTrayRowNum", TrayConfig.SufRowNum, 110, false));
            grdTray.Columns.Add(MakeColumn("colTrayColNum", TrayConfig.SufColNum, 110, false));
            grdTray.Columns.Add(MakeColumn("colTrayRowPitch", TrayConfig.SufRowDis, 130, false));
            grdTray.Columns.Add(MakeColumn("colTrayColPitch", TrayConfig.SufColDis, 130, false));
            UiKit.StyleGrid(grdTray);
            grdTray.CellValueChanged += grdTray_CellValueChanged;
            grdTray.DataError += grdTray_DataError;

            // ---- 状态行
            lblStatus = MakeLabel("lblTrayStatus", "", 0, ContentAlignment.MiddleLeft);
            lblStatus.Dock = DockStyle.Bottom;
            lblStatus.Height = StatusRowHeight;
            lblStatus.ForeColor = UiKit.TextSecondary;

            card.Controls.Add(grdTray);       // Fill 先加
            card.Controls.Add(lblStatus);     // Bottom
            card.Controls.Add(tools);         // Top 后加 ⇒ 先占顶部（停靠顺序 = 加序的逆序）
            RefreshTrayGrid();
            return card;
        }


        // ---------------------------------------------------------------- 表格

        /// <summary>按当前配方重建表格行（一行一个托盘）。</summary>
        private void RefreshTrayGrid()
        {
            if (grdTray == null) return;

            _fillingGrid = true;
            try
            {
                grdTray.Rows.Clear();
                _rowSpecs = TrayConfig.BuildRows();

                for (int i = 0; i < _rowSpecs.Count; i++)
                {
                    TrayRowSpec s = _rowSpecs[i];
                    int idx = grdTray.Rows.Add(
                        s.Key,      // 稳定的行标识，与 TrayConfig 的 name 一致，不本地化
                        CellText(s.RowNumCol), CellText(s.ColumnNumCol),
                        CellText(s.RowPitchCol), CellText(s.ColumnPitchCol));
                    grdTray.Rows[idx].Tag = s;

                    // 配方里缺的那一列不可编辑（免得改出一半）
                    grdTray.Rows[idx].Cells[1].ReadOnly = (s.RowNumCol == null);
                    grdTray.Rows[idx].Cells[2].ReadOnly = (s.ColumnNumCol == null);
                    grdTray.Rows[idx].Cells[3].ReadOnly = (s.RowPitchCol == null);
                    grdTray.Rows[idx].Cells[4].ReadOnly = (s.ColumnPitchCol == null);
                }
                grdTray.ClearSelection();
            }
            catch { }
            finally
            {
                _fillingGrid = false;
            }

            UpdateStatus(_rowSpecs.Count == 0
                ? T("empty", "配方里还没有托盘参数，用上面的「添加托盘」建一组",
                       "No tray parameters yet - use Add tray above",
                       "Aun no hay parametros - use Anadir arriba")
                : "");
        }

        /// <summary>单元格改完写回配方列；不是数字就还原，不让脏值进配方。</summary>
        private void grdTray_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_fillingGrid || e.RowIndex < 0 || e.ColumnIndex < 1) return;

            DataGridViewRow row = grdTray.Rows[e.RowIndex];
            TrayRowSpec spec = row.Tag as TrayRowSpec;
            if (spec == null) return;

            string column = ColumnAt(spec, e.ColumnIndex);
            if (column == null) return;

            DataGridViewCell cell = row.Cells[e.ColumnIndex];
            double v = TrayConfig.ToDouble(Convert.ToString(cell.Value), double.NaN);
            if (double.IsNaN(v))
            {
                _fillingGrid = true;
                cell.Value = CellText(column);
                _fillingGrid = false;
                UpdateStatus(T("badNumber", "请输入数字", "Numbers only", "Solo numeros"), true);
                return;
            }

            if (TrayConfig.SetValue(column, v))
            {
                SysPara.items++;                     // 标记配方有改动，离开页面时统一保存
                cell.Value = TrayConfig.ToText(v);   // 统一显示格式（12.50 → 12.5）
                UpdateStatus(string.Format(T("saved", "已修改 {0}", "Updated {0}", "Actualizado {0}"), column));
            }
        }

        /// <summary>数据类型不符时吞掉异常并提示，不让 DataGridView 弹系统错误框。</summary>
        private void grdTray_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
            e.Cancel = true;
        }

        private static string ColumnAt(TrayRowSpec s, int columnIndex)
        {
            switch (columnIndex)
            {
                case 1: return s.RowNumCol;
                case 2: return s.ColumnNumCol;
                case 3: return s.RowPitchCol;
                case 4: return s.ColumnPitchCol;
                default: return null;
            }
        }

        private static string CellText(string column)
        {
            return string.IsNullOrEmpty(column) ? "" : TrayConfig.ToText(TrayConfig.Num(column));
        }

        // ---------------------------------------------------------------- 增删

        /// <summary>按输入的名字建一组托盘列；建完立刻回存配方文件。</summary>
        private void btnAddTray_Click(object sender, EventArgs e)
        {
            string name = boxNewName == null ? "" : (boxNewName.Value ?? "").Trim();

            // 名字会变成配方列名，先本地化地挡掉非法输入（数据层也会再挡一次）
            if (name.Length == 0)
            {
                UpdateStatus(T("needName", "请输入新托盘名称", "Enter a tray name", "Escriba un nombre"), true);
                return;
            }
            if (!TrayConfig.IsValidName(name))
            {
                UpdateStatus(T("badName",
                    "名称只能用英文字母开头 + 英文字母/数字/下划线（不能用中文）",
                    "Name must start with an ASCII letter; only letters, digits and underscore",
                    "El nombre debe empezar por letra ASCII: solo letras, digitos y guion bajo"), true);
                try { boxNewName.FocusInput(); } catch { }
                return;
            }

            string err = TrayConfig.AddTray(name);
            if (err.Length != 0)
            {
                UpdateStatus(err, true);
                return;
            }

            SysPara.items++;
            RebuildTrayTable();
            UpdateStatus(string.Format(T("added", "已添加托盘 {0}", "Added tray {0}", "Bandeja añadida {0}"), name));
            try { OperationLog.Write("PlatFormSetting", "添加托盘：" + name); } catch { }
        }

        /// <summary>删掉选中那一组托盘的 4 个配方列。</summary>
        private void btnDelTray_Click(object sender, EventArgs e)
        {
            if (grdTray == null || grdTray.CurrentRow == null)
            {
                UpdateStatus(T("pickOne", "请先在表格里选中一行", "Select a row first", "Seleccione una fila"), true);
                return;
            }

            TrayRowSpec spec = grdTray.CurrentRow.Tag as TrayRowSpec;
            if (spec == null) return;

            string title = spec.Key;
            string ask = string.Format(T("delConfirm",
                "确定删除「{0}」吗？\r\n会从配方里删掉它的 4 个参数列，无法撤销。",
                "Delete \"{0}\"? Its 4 recipe columns will be removed. This cannot be undone.",
                "Eliminar \"{0}\"? Se quitaran sus 4 columnas del recetario."), title);
            if (MessageBox.Show(ask, T("delTitle", "删除托盘", "Delete tray", "Eliminar bandeja"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            string err = TrayConfig.RemoveTray(spec);
            if (err.Length != 0)
            {
                UpdateStatus(err, true);
                return;
            }

            SysPara.items++;
            RebuildTrayTable();
            UpdateStatus(string.Format(T("removed", "已删除 {0}", "Deleted {0}", "Eliminado {0}"), title));
            try { OperationLog.Write("PlatFormSetting", "删除托盘：" + title); } catch { }
        }

        // ---------------------------------------------------------------- 文案 / 小工具

        /// <summary>页面标题（窗体自身的 Text 是模块名，不能本地化）。</summary>
        private void ApplyPageLanguage()
        {
            try
            {
                lblPageTitle.Text = T("pageTitle", "托盘参数", "Tray settings", "Ajustes de bandeja");
            }
            catch { }
        }

        private static string T(string key, string zh, string en, string es)
        {
            return UiTheme.T("PlatFormSetting", key, zh, en, es);
        }

        private void UpdateStatus(string text, bool warn = false)
        {
            if (lblStatus == null) return;
            lblStatus.ForeColor = warn ? UiTheme.WarnText : UiKit.TextSecondary;
            lblStatus.Text = text ?? "";
        }

        private static UiLabel MakeLabel(string name, string text, int width, ContentAlignment align)
        {
            UiLabel l = new UiLabel();
            l.Name = name;
            l.AutoSize = false;                     // Dock/AutoSize 混用会让标签撑不住
            l.Font = UiTheme.Body;
            l.ForeColor = UiKit.TextSecondary;
            l.TextAlign = align;
            l.Text = text;
            if (width > 0) l.Size = new Size(width, 34);
            return l;
        }

        private static DataGridViewTextBoxColumn MakeColumn(string name, string header, int width, bool fill)
        {
            DataGridViewTextBoxColumn c = new DataGridViewTextBoxColumn();
            c.Name = name;
            c.HeaderText = header;
            c.ReadOnly = fill;
            c.SortMode = DataGridViewColumnSortMode.NotSortable;
            if (fill) c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            else c.Width = width;
            return c;
        }



        // ---------------------------------------------------------------- 读取接口（转发到 TrayConfig）

        /// <summary>
        /// **通过「托盘名 + 参数项」取一个具体数值**，例：
        /// <c>MiddleLayer.PlatF.GetTrayValue("UnloadTray2", TrayField.Rows)</c> → 下料盘 B 的行数。
        /// 名字就是表格 TrayName 列里的值；找不到返回 0。完整接口见 <see cref="TrayConfig"/>。
        /// </summary>
        public double GetTrayValue(string name, TrayField field)
        {
            return TrayConfig.Get(name, field);
        }

        /// <summary>同上，但能区分"没有这个托盘 / 没有这一项"。见 <see cref="TrayConfig.TryGet(string,TrayField,out double)"/>。</summary>
        public bool TryGetTrayValue(string name, TrayField field, out double value)
        {
            return TrayConfig.TryGet(name, field, out value);
        }

        /// <summary>通过「托盘名 + 配方列后缀」取值（后缀同表头 RowNum/ColumnNum/Rowdis/ColumnDis）。</summary>
        public double GetTrayValue(string name, string columnSuffix)
        {
            return TrayConfig.Get(name, columnSuffix);
        }

        /// <summary>一条语句读整张托盘表（含运行时添加的托盘）。见 <see cref="TrayConfig.GetTable"/>。</summary>
        public List<TrayTableRow> GetTrayTable()
        {
            return TrayConfig.GetTable();
        }

        // ---------------------------------------------------------------- 原有点击/保存逻辑

        /// <summary>离开页面时由主界面确认保存；取消或失败保留未保存标记。</summary>
        private void PlatFormSetting_Leave(object sender, EventArgs e)
        {
            if (SysPara.items > 1)
            {
                MiddleLayer.MainF.SaveData();
            }
        }

    }
}
