using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>
    /// 生产数据（按时间段）查询：数据来自 ProductionDataStore 落盘的每日 CSV
    ///   {ProducPath}\ProductionData\{yyyy}\{MM}\{yyyy-MM-dd}.csv
    ///
    /// 自上而下 = 查询条（时间范围 + 快捷 + 关键字 + 查询/导出）/ 统计数字条 / 明细表，
    /// 内容区都是白色圆角卡片。按需创建、复用同一个实例（见 <see cref="MainForm.OpenProductionQuery"/>），
    /// 每次打开重新查询一次。
    ///
    /// 本窗体是**纯代码构建**（没有 .Designer.cs）：列是动态的、设计器帮不上忙，
    /// 装配的都是工程里已有的自绘控件（DateField / FieldBox / FlatButton / StatStrip / CardPanel）。
    /// </summary>
    public class ProductionQueryForm : Form
    {
        /// <summary>明细表最多显示的行数，避免一次查询几十万行把界面拖死。</summary>
        private const int MaxRowsShown = 5000;

        private CardPanel cardQuery;
        private CardPanel cardDetail;
        private Panel detailHost;

        private UiLabel lblRange;
        private UiLabel lblTo;
        private UiLabel lblDetail;
        private UiLabel lblStatus;

        private DateField dateFrom;
        private DateField dateTo;
        private FieldBox boxKeyword;
        private FlatButton btnToday;
        private FlatButton btnWeek;
        private FlatButton btnMonth;
        private FlatButton btnQuery;
        private FlatButton btnExport;

        private StatStrip statStrip;
        private DataGridView grd;
        private WindowChrome _chrome;

        private List<ProductionRecord> _rows = new List<ProductionRecord>();
        private bool _loading;

        public ProductionQueryForm()
        {
            UiTheme.ApplyDpi(this);

            BuildUi();
            // 框架风格的无边框标题栏（与报警查询窗口一致）
            _chrome = WindowChrome.Attach(this, AppIcon.Data, Text);
            WireEvents();

            _loading = true;
            dateFrom.Value = DateTime.Today;
            dateTo.Value = DateTime.Today;
            _loading = false;

            ApplyLanguage();

            // VS 设计器：填几行示例，画出来的列/行/统计和运行期一致；
            // 不读文件、也不订阅 MiddleLayer（它的静态初始化会构造硬件对象，设计器里会抛异常）
            if (UiTheme.IsDesignTime)
            {
                LoadDesignSample();
                return;
            }

            Reload();

            try { MiddleLayer.LanguageChanged += ProductionQueryForm_LanguageChanged; }
            catch { }
        }

        /// <summary>设计期样本：只为让 VS 设计器里看到的和运行起来的 EXE 一致。</summary>
        private void LoadDesignSample()
        {
            try
            {
                _loading = true;
                dateFrom.Value = DateTime.Today.AddDays(-2);
                dateTo.Value = DateTime.Today;
                _loading = false;

                string[] cols = { "PalletCode", "Driver", "Pressure", "PressureMax", "PressureMin", "Result" };
                string[][] values =
                {
                    new[] { "PLT-0001", "A01", "0.42", "0.55", "0.31", "OK" },
                    new[] { "PLT-0002", "A01", "0.44", "0.56", "0.33", "OK" },
                    new[] { "PLT-0003", "A02", "0.39", "0.51", "0.28", "NG" },
                    new[] { "PLT-0004", "A02", "0.45", "0.57", "0.34", "OK" },
                    new[] { "PLT-0005", "A01", "0.41", "0.53", "0.30", "OK" },
                    new[] { "PLT-0006", "A01", "0.43", "0.54", "0.32", "OK" }
                };

                DateTime day = DateTime.Today.AddDays(-2);
                _rows = new List<ProductionRecord>();
                for (int i = 0; i < values.Length; i++)
                {
                    ProductionRecord r = new ProductionRecord();
                    r.Time = day.AddDays(i % 3).AddHours(8 + i).AddMinutes(7 * i);
                    r.Columns = cols;
                    r.Values = values[i];
                    _rows.Add(r);
                }

                FillGrid();
                FillStats();
                FillStatus(3, _rows.Count);
            }
            catch { }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            // 打开时别把焦点丢给关键字框：FieldBox 有焦点时不画占位提示，用户就看不到说明
            ActiveControl = null;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // 静态事件必须解绑，否则已释放的窗体会被语言切换一直拉住
            try { MiddleLayer.LanguageChanged -= ProductionQueryForm_LanguageChanged; }
            catch { }
            if (_chrome != null) _chrome.Detach();
            base.OnFormClosed(e);
        }

        private void ProductionQueryForm_LanguageChanged(object sender, EventArgs e)
        {
            ApplyLanguage();
            FillGrid();
            FillStats();
        }

        // ---------------- 界面 ----------------

        private void BuildUi()
        {
            SuspendLayout();

            Text = "生产数据查询";
            StartPosition = FormStartPosition.CenterParent;
            Font = UiKit.Regular(10.5f);
            BackColor = UiKit.PageBg;
            ClientSize = new Size(1180, 720);
            MinimumSize = new Size(880, 560);

            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.BackColor = UiKit.PageBg;
            root.Padding = new Padding(D(16), D(14), D(22), D(20));
            root.ColumnCount = 1;
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            root.RowCount = 5;
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, D(134)));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, D(10)));
            // 统计条高度：数值画在 y=46、占 Height-52，给 74 会把大数字裁掉
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, D(96)));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, D(10)));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            Controls.Add(root);

            // ---- 查询条：第一行＝时间范围 +「查询/导出」；第二行＝快捷 + 关键字
            //      （与报警统计窗口同一版式）
            cardQuery = new CardPanel();
            cardQuery.Dock = DockStyle.Fill;
            cardQuery.Margin = new Padding(0);
            cardQuery.Padding = new Padding(D(18), D(14), D(18), D(14));
            root.Controls.Add(cardQuery, 0, 0);

            TableLayoutPanel barTop = new TableLayoutPanel();
            barTop.Dock = DockStyle.Top;
            barTop.Height = D(44);
            barTop.BackColor = UiKit.Surface;
            barTop.Margin = new Padding(0);
            barTop.ColumnCount = 2;
            barTop.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            barTop.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, D(214)));
            barTop.RowCount = 1;
            barTop.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            cardQuery.Controls.Add(barTop);

            FlowLayoutPanel rowRange = NewRow();
            barTop.Controls.Add(rowRange, 0, 0);

            lblRange = NewLabel("时间范围", UiKit.TextSecondary, 10f);
            rowRange.Controls.Add(lblRange);

            dateFrom = NewDateField();
            rowRange.Controls.Add(dateFrom);

            lblTo = NewLabel("至", UiKit.TextSecondary, 10f);
            rowRange.Controls.Add(lblTo);

            dateTo = NewDateField();
            rowRange.Controls.Add(dateTo);

            FlowLayoutPanel rowActions = NewRow();
            rowActions.Padding = new Padding(0, D(4), 0, D(4));
            barTop.Controls.Add(rowActions, 1, 0);

            btnQuery = NewButton(D(98));
            rowActions.Controls.Add(btnQuery);
            btnExport = NewButton(D(98));
            rowActions.Controls.Add(btnExport);

            FlowLayoutPanel barBottom = NewRow();
            barBottom.Dock = DockStyle.Bottom;
            barBottom.Height = D(44);
            barBottom.Padding = new Padding(0, D(4), 0, 0);
            cardQuery.Controls.Add(barBottom);

            btnToday = NewButton(D(76));
            barBottom.Controls.Add(btnToday);
            btnWeek = NewButton(D(88));
            barBottom.Controls.Add(btnWeek);
            btnMonth = NewButton(D(88));
            barBottom.Controls.Add(btnMonth);

            boxKeyword = new FieldBox();
            boxKeyword.Size = new Size(D(320), D(34));
            boxKeyword.Margin = new Padding(D(12), D(4), 0, 0);
            boxKeyword.BackColor = UiKit.Surface;
            barBottom.Controls.Add(boxKeyword);

            // ---- 统计数字条 ----
            statStrip = new StatStrip();
            statStrip.Dock = DockStyle.Fill;
            statStrip.Margin = new Padding(0);
            root.Controls.Add(statStrip, 0, 2);

            // ---- 明细卡：先放内容（Fill）、再放表头（Top）——
            //      Dock 顺序上后加的先占边；反过来表头会被内容挤没
            cardDetail = new CardPanel();
            cardDetail.Dock = DockStyle.Fill;
            cardDetail.Margin = new Padding(0);
            cardDetail.Padding = new Padding(D(18), D(12), D(18), D(14));
            root.Controls.Add(cardDetail, 0, 4);

            detailHost = new Panel();
            detailHost.Dock = DockStyle.Fill;
            detailHost.BackColor = UiKit.Surface;
            cardDetail.Controls.Add(detailHost);

            grd = new DataGridView();
            grd.Dock = DockStyle.Fill;
            grd.Margin = new Padding(0);
            UiKit.StyleGrid(grd);
            grd.ReadOnly = true;
            grd.AllowUserToAddRows = false;
            grd.AllowUserToDeleteRows = false;
            grd.AllowUserToResizeRows = false;
            grd.RowHeadersVisible = false;
            grd.MultiSelect = false;
            grd.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grd.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            grd.ColumnHeadersDefaultCellStyle.Padding = D4();
            detailHost.Controls.Add(grd);

            // 明细卡表头：左「生产数据明细」、右状态行（与报警统计窗口同一版式）
            Panel detailHead = new Panel();
            detailHead.Dock = DockStyle.Top;
            detailHead.Height = D(34);
            detailHead.BackColor = UiKit.Surface;
            cardDetail.Controls.Add(detailHead);

            lblDetail = NewLabel("生产数据明细", UiKit.TextPrimary, 12f, true);
            lblDetail.AutoSize = false;
            lblDetail.Dock = DockStyle.Fill;
            lblDetail.Margin = new Padding(0);
            lblDetail.TextAlign = ContentAlignment.MiddleLeft;
            detailHead.Controls.Add(lblDetail);

            lblStatus = NewLabel("", UiKit.TextSecondary, 9.5f);
            lblStatus.AutoSize = true;
            lblStatus.Dock = DockStyle.Right;
            lblStatus.Margin = new Padding(0);
            lblStatus.TextAlign = ContentAlignment.MiddleRight;
            detailHead.Controls.Add(lblStatus);

            ResumeLayout(false);
        }

        /// <summary>查询条里的一行：不换行的横向流式容器。</summary>
        private static FlowLayoutPanel NewRow()
        {
            FlowLayoutPanel p = new FlowLayoutPanel();
            p.Dock = DockStyle.Fill;
            p.Margin = new Padding(0);
            p.BackColor = UiKit.Surface;
            p.WrapContents = false;
            p.FlowDirection = FlowDirection.LeftToRight;
            return p;
        }

        private static int D(int v) { return UiTheme.Dpi(v); }

        private static Padding D4()
        {
            return UiTheme.Dpi(new Padding(4, 0, 4, 0));
        }

        /// <summary>按文字量定列宽（夹在 [min,max] 之间）——不依赖延迟生效的 AutoSizeColumnsMode。</summary>
        private static int ColWidth(string text, Font f, int min, int max)
        {
            int w = min;
            try
            {
                w = TextRenderer.MeasureText(string.IsNullOrEmpty(text) ? " " : text, f).Width + D(28);
            }
            catch { w = min; }
            if (w < min) w = min;
            if (w > max) w = max;
            return w;
        }

        private static UiLabel NewLabel(string text, Color color, float pt, bool bold = false)
        {
            UiLabel l = new UiLabel();
            l.Text = text;
            l.AutoSize = true;
            l.ForeColor = color;
            l.Font = bold ? UiKit.Bold(pt) : UiKit.Regular(pt);
            l.BackColor = Color.Transparent;
            l.Margin = new Padding(0, D(10), D(8), 0);
            return l;
        }

        private static DateField NewDateField()
        {
            DateField d = new DateField();
            d.Size = new Size(D(140), D(34));
            d.Margin = new Padding(0, D(4), D(10), 0);
            return d;
        }

        private static FlatButton NewButton(int width)
        {
            FlatButton b = new FlatButton();
            b.Variant = FlatButtonVariant.Surface;
            b.Size = new Size(width, D(34));
            b.Margin = new Padding(0, D(3), D(8), 0);
            b.IconSize = D(14);
            b.IconGap = D(6);
            return b;
        }

        private void WireEvents()
        {
            btnQuery.Click += delegate { RunQuery(); };
            btnExport.Click += delegate { Export(); };
            btnToday.Click += delegate { SetQuickRange(0); };
            btnWeek.Click += delegate { SetQuickRange(6); };
            btnMonth.Click += delegate { SetQuickRange(29); };

            dateFrom.ValueChanged += delegate { if (!_loading) RunQuery(); };
            dateTo.ValueChanged += delegate { if (!_loading) RunQuery(); };
            boxKeyword.ValueChanged += delegate { if (!_loading) RunQuery(); };
        }

        private void SetQuickRange(int daysBack)
        {
            _loading = true;
            dateTo.Value = DateTime.Today;
            dateFrom.Value = DateTime.Today.AddDays(-daysBack);
            _loading = false;
            RunQuery();
        }

        // ---------------- 文字 ----------------

        private void ApplyLanguage()
        {
            try
            {
                Text = UiTheme.T("ProductionQuery", "title", "生产数据查询", "Production data", "Datos de produccion");
                lblRange.Text = UiTheme.T("ProductionQuery", "range", "时间范围", "Date range", "Rango");
                lblTo.Text = UiTheme.T("ProductionQuery", "to", "至", "to", "a");
                lblDetail.Text = UiTheme.T("ProductionQuery", "detail", "生产数据明细", "Records", "Registros");
                if (_chrome != null) _chrome.Title = Text;

                btnToday.Text = UiTheme.T("ProductionQuery", "today", "今天", "Today", "Hoy");
                btnWeek.Text = UiTheme.T("ProductionQuery", "week", "近 7 天", "Last 7 days", "7 dias");
                btnMonth.Text = UiTheme.T("ProductionQuery", "month", "近 30 天", "Last 30 days", "30 dias");
                btnQuery.Text = UiTheme.T("ProductionQuery", "query", "查询", "Query", "Consultar");
                btnExport.Text = UiTheme.T("ProductionQuery", "export", "导出", "Export", "Exportar");

                boxKeyword.PlaceholderText = UiTheme.T("ProductionQuery", "keyword", "内容关键字", "Keyword", "Palabra clave");

                btnQuery.Icon = AppIcons.Get(AppIcon.Search, D(14), UiKit.TextPrimary);
                btnExport.Icon = AppIcons.Get(AppIcon.Save, D(14), UiKit.TextPrimary);
                btnToday.Icon = AppIcons.Get(AppIcon.Calendar, D(14), UiKit.TextSecondary);
                btnWeek.Icon = AppIcons.Get(AppIcon.Calendar, D(14), UiKit.TextSecondary);
                btnMonth.Icon = AppIcons.Get(AppIcon.Calendar, D(14), UiKit.TextSecondary);
            }
            catch { }
        }

        // ---------------- 查询 ----------------

        /// <summary>按当前条件重新查询（打开窗口时、以及外部调用时用）。</summary>
        public void Reload()
        {
            RunQuery();
        }

        private void RunQuery()
        {
            if (_loading) return;
            try
            {
                Cursor = Cursors.WaitCursor;

                int fileCount, total;
                _rows = ProductionDataStore.Query(dateFrom.Value, dateTo.Value,
                    boxKeyword.Value, out fileCount, out total);

                FillGrid();
                FillStats();
                FillStatus(fileCount, total);

                OperationLog.Write("ProductionQueryForm", string.Format(
                    "查询生产数据：{0:yyyy-MM-dd} ~ {1:yyyy-MM-dd}，命中 {2} 条（共 {3} 条，{4} 个文件）",
                    dateFrom.Value, dateTo.Value, _rows.Count, total, fileCount));
            }
            catch (Exception) { }
            finally { Cursor = Cursors.Default; }
        }

        /// <summary>按查询结果重建列（时间列 + 结果里出现过的所有列）并填行。</summary>
        private void FillGrid()
        {
            if (grd == null) return;
            try
            {
                grd.SuspendLayout();
                grd.Rows.Clear();
                grd.Columns.Clear();

                string[] cols = ProductionDataStore.DistinctColumns(_rows);

                // 列宽**自己算**：DisplayedCells 的自适应是延迟重算的，DrawToBitmap / 首次显示时还没生效
                grd.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;

                DataGridViewTextBoxColumn cTime = new DataGridViewTextBoxColumn();
                cTime.Name = "colTime";
                cTime.HeaderText = ProductionDataStore.TimeColumn;
                cTime.ReadOnly = true;
                cTime.SortMode = DataGridViewColumnSortMode.NotSortable;
                cTime.MinimumWidth = D(52);
                cTime.Width = ColWidth("2026-10-14 08:00:00", UiKit.Regular(9.5f), D(90), D(190));
                grd.Columns.Add(cTime);

                for (int i = 0; i < cols.Length; i++)
                {
                    DataGridViewTextBoxColumn c = new DataGridViewTextBoxColumn();
                    c.Name = "col" + i;
                    c.HeaderText = cols[i];
                    c.HeaderCell.ToolTipText = cols[i];
                    c.ReadOnly = true;
                    c.SortMode = DataGridViewColumnSortMode.NotSortable;
                    c.MinimumWidth = D(52);
                    c.Width = ColWidth(cols[i], UiKit.Bold(9.5f), D(80), D(220));
                    grd.Columns.Add(c);
                }

                // 最后一列吃掉剩余宽度：否则尾部留一大片空白（Time 列宽是算好的，不会被挤）
                if (grd.Columns.Count > 0)
                    grd.Columns[grd.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

                int shown = Math.Min(_rows.Count, MaxRowsShown);
                for (int i = 0; i < shown; i++)
                {
                    ProductionRecord r = _rows[i];
                    object[] cells = new object[grd.Columns.Count];
                    cells[0] = r.Time.ToString("yyyy-MM-dd HH:mm:ss");
                    for (int k = 0; k < cols.Length; k++) cells[k + 1] = r.Get(cols[k]);
                    grd.Rows.Add(cells);
                }

                grd.ClearSelection();
                grd.ResumeLayout();
                Invalidate();
            }
            catch { }
        }

        private void FillStats()
        {
            if (statStrip == null) return;
            try
            {
                string[] cols = ProductionDataStore.DistinctColumns(_rows);
                List<StatItem> items = new List<StatItem>();
                items.Add(new StatItem(UiTheme.T("ProductionQuery", "statRows", "记录条数", "Records", "Registros"),
                    _rows.Count.ToString(), UiKit.Brand));
                items.Add(new StatItem(UiTheme.T("ProductionQuery", "statDays", "涉及天数", "Days", "Dias"),
                    ProductionDataStore.CountDays(_rows).ToString(), UiKit.Success));
                items.Add(new StatItem(UiTheme.T("ProductionQuery", "statCols", "数据列数", "Columns", "Columnas"),
                    cols.Length.ToString(), UiKit.Warn));
                statStrip.SetItems(items);
            }
            catch { }
        }

        private void FillStatus(int fileCount, int total)
        {
            if (lblStatus == null) return;
            try
            {
                if (fileCount == 0)
                {
                    lblStatus.Text = string.Format(UiTheme.T("ProductionQuery", "noFile",
                        "该时间段没有数据（{0}）", "No data in this range ({0})", "Sin datos ({0})"),
                        ProductionDataStore.RootPath());
                    return;
                }

                string s = string.Format(UiTheme.T("ProductionQuery", "status",
                    "共 {0} 条 · 命中 {1} 条 · {2} 个文件",
                    "{0} records · {1} matched · {2} files",
                    "{0} registros · {1} coincidencias · {2} archivos"),
                    total, _rows.Count, fileCount);

                if (_rows.Count > MaxRowsShown)
                    s += string.Format(UiTheme.T("ProductionQuery", "truncated",
                        "（仅显示前 {0} 条）", " (showing first {0})", " (mostrando {0})"), MaxRowsShown);

                lblStatus.Text = s;
            }
            catch { }
        }

        // ---------------- 导出 ----------------

        private void Export()
        {
            try
            {
                if (_rows.Count == 0)
                {
                    MessageBox.Show(this,
                        UiTheme.T("ProductionQuery", "nothingToExport", "没有可导出的数据", "Nothing to export", "Nada que exportar"),
                        Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (SaveFileDialog dlg = new SaveFileDialog())
                {
                    dlg.Filter = "CSV (*.csv)|*.csv";
                    dlg.FileName = string.Format("ProductionData_{0:yyyyMMdd}_{1:yyyyMMdd}.csv",
                        dateFrom.Value, dateTo.Value);

                    if (dlg.ShowDialog(this) != DialogResult.OK) return;

                    string err = ProductionDataStore.Export(_rows, dlg.FileName);
                    if (err.Length == 0)
                    {
                        MessageBox.Show(this, string.Format(UiTheme.T("ProductionQuery", "exportOk",
                            "已导出 {0} 条 → {1}", "Exported {0} records -> {1}", "Exportados {0} -> {1}"),
                            _rows.Count, dlg.FileName), Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        OperationLog.Write("ProductionQueryForm", "导出生产数据：" + dlg.FileName);
                    }
                    else
                    {
                        MessageBox.Show(this, err, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception) { }
        }
    }
}
