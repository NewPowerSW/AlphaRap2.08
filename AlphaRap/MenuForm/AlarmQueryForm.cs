using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>
    /// 报警统计与历史查询：数据来自报警日志 {AlarmPath}\{年}\{月}\{日}.csv（见 <see cref="AlarmHistory"/>）。
    /// 自上而下 = 品牌蓝标题栏 / 查询条（时间范围 + 快捷 + 类型分段 + 条件）/ 停机统计数字条 /
    /// Top 报警号与每日趋势两张图 / 明细表，内容区均为白色圆角卡片。
    /// 默认只看会停机的报警；按需创建、复用同一个实例（见 MainForm.OpenAlarmQuery），显示时重新查询一次。
    /// </summary>
    public partial class AlarmQueryForm : Form
    {
        /// <summary>明细表格最多显示的行数，避免一次查询几十万行把界面拖死。</summary>
        private const int MaxRowsShown = 5000;

        /// <summary>类型分段里「停机」的下标（默认选中项：统计只算停机报警）。</summary>
        private const int TypeStopIndex = 3;

        /// <summary>明细行左缘状态色条的宽度（96 DPI 设计值）。</summary>
        private const int RowAccentW = 4;

        /// <summary>明细表的斑马纹底色（比 UiKit 默认再浅一档，和状态色条同处一行时不抢眼）。</summary>
        private static readonly Color RowZebraBack = Color.FromArgb(249, 251, 253);

        /// <summary>E 类「类型」胶囊的底色。</summary>
        private static readonly Color PillAlarmBack = Color.FromArgb(253, 236, 236);
        /// <summary>W 类「类型」胶囊的底色。</summary>
        private static readonly Color PillWarnBack = Color.FromArgb(255, 245, 225);

        /// <summary>表格「停机」列的显示文字（随语言变化）。</summary>
        private string _stopText = "停机";

        private List<AlarmRecord> _rows = new List<AlarmRecord>();
        private bool _loading;

        public AlarmQueryForm()
        {
            InitializeComponent();
            UiTheme.ApplyDpi(this);

            StyleDetailGrid();
            SetupCards();
            SetupQueryBar();
            SetupRowPainting();

            _loading = true;
            dateFrom.Value = DateTime.Today;
            dateTo.Value = DateTime.Today;
            _loading = false;

            btnQuery.Click += btnQuery_Click;
            btnExport.Click += btnExport_Click;

            KeyPreview = true;
            SetupWindowChrome();

            ApplyLanguage();
            Reload();

            try { MiddleLayer.LanguageChanged += AlarmQueryForm_LanguageChanged; }
            catch { }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // 静态事件必须解绑，否则已释放的窗体会被语言切换一直拉住
            try { MiddleLayer.LanguageChanged -= AlarmQueryForm_LanguageChanged; }
            catch { }
            base.OnFormClosed(e);
        }

        private void AlarmQueryForm_LanguageChanged(object sender, EventArgs e)
        {
            ApplyLanguage();
            UpdateDetailTitle();
        }

        // ---------------- 外观搭建 ----------------

        /// <summary>明细表：在 UiKit 扁平样式之上，把左内边距留宽给状态色条，行高压到 30 以便多显示几行。</summary>
        private void StyleDetailGrid()
        {
            UiKit.StyleGrid(grdAlarm);
            try
            {
                grdAlarm.DefaultCellStyle.Padding = UiTheme.Dpi(new Padding(10, 0, 6, 0));
                grdAlarm.ColumnHeadersDefaultCellStyle.Padding = UiTheme.Dpi(new Padding(10, 0, 6, 0));
                grdAlarm.AlternatingRowsDefaultCellStyle.BackColor = RowZebraBack;
                grdAlarm.RowTemplate.Height = UiTheme.Dpi(30);
            }
            catch { }
        }

        /// <summary>卡片与查询控件的圆角。描边靠「外层底色=线色 + Padding=1」留出，不自绘。</summary>
        private void SetupCards()
        {
            RoundControl(pnlBarOuter, 12);
            RoundControl(pnlCardOuter, 12);
            pnlBarOuter.Resize += delegate { RoundControl(pnlBarOuter, 12); };
            pnlCardOuter.Resize += delegate { RoundControl(pnlCardOuter, 12); };

        }

        /// <summary>把控件裁成圆角（半径按 96 DPI 设计值给）。</summary>
        private static void RoundControl(Control c, int radius)
        {
            if (c == null || c.Width <= 1 || c.Height <= 1) return;
            try
            {
                using (GraphicsPath path = UiKit.RoundRect(new Rectangle(0, 0, c.Width, c.Height), UiTheme.Dpi(radius)))
                {
                    Region old = c.Region;
                    c.Region = new Region(path);
                    if (old != null) old.Dispose();
                }
            }
            catch { }
        }

        private void SetupQueryBar()
        {
            segType.SelectedIndexChanged += delegate { UpdateDetailTitle(); };

            btnToday.Click += delegate { SetQuickRange(0); };
            btnWeek.Click += delegate { SetQuickRange(6); };
            btnMonth.Click += delegate { SetQuickRange(29); };

        }

        /// <summary>行左缘画状态色条 + 「类型」列画胶囊：报警行不再整行铺色，长表才看得下去。</summary>
        private void SetupRowPainting()
        {
            grdAlarm.RowPostPaint += GrdAlarm_RowPostPaint;
            grdAlarm.CellPainting += GrdAlarm_CellPainting;
        }

        private void GrdAlarm_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            try
            {
                AlarmRecord r = RowRecord(e.RowIndex);
                if (r == null) return;

                int w = UiTheme.Dpi(RowAccentW);
                Rectangle bar = new Rectangle(e.RowBounds.Left + UiTheme.Dpi(1),
                    e.RowBounds.Top + UiTheme.Dpi(4), w, e.RowBounds.Height - UiTheme.Dpi(8));
                if (bar.Height <= 0) return;

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath p = UiKit.RoundRect(bar, w / 2))
                using (SolidBrush b = new SolidBrush(r.IsAlarm ? UiKit.Danger : UiKit.Warn))
                    e.Graphics.FillPath(b, p);
            }
            catch { }
        }

        private void GrdAlarm_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != colType.Index) return;
            AlarmRecord r = RowRecord(e.RowIndex);
            if (r == null) return;

            try
            {
                e.PaintBackground(e.CellBounds, true);

                int h = UiTheme.Dpi(18);
                int w = UiTheme.Dpi(30);
                Rectangle pill = new Rectangle(e.CellBounds.Left + UiTheme.Dpi(10),
                    e.CellBounds.Top + (e.CellBounds.Height - h) / 2, w, h);

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (GraphicsPath p = UiKit.RoundRect(pill, h / 2))
                using (SolidBrush b = new SolidBrush(r.IsAlarm ? PillAlarmBack : PillWarnBack))
                    e.Graphics.FillPath(b, p);

                TextRenderer.DrawText(e.Graphics, r.IsAlarm ? "E" : "W", UiKit.Bold(9f), pill,
                    r.IsAlarm ? UiKit.Danger : UiTheme.WarnText,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                e.Handled = true;
            }
            catch { e.Handled = false; }
        }

        private AlarmRecord RowRecord(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= grdAlarm.Rows.Count) return null;
            return grdAlarm.Rows[rowIndex].Tag as AlarmRecord;
        }

        // ---------------- 无边框窗口（FormBorderStyle.None）----------------

        /// <summary>窗口圆角半径（96 DPI 设计值）。</summary>
        private const int WindowCornerRadius = 12;

        private const int WmNcLButtonDown = 0x00A1;
        private const int HtCaption = 2;
        private const int HtBottomRight = 17;

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(int left, int top, int right, int bottom, int w, int h);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        /// <summary>无边框窗口的自定义外观：标题栏拖动、最小化/关闭按钮、右下角缩放柄、圆角窗口。</summary>
        private void SetupWindowChrome()
        {
            try
            {
                picTitleIcon.Image = AppIcons.Get(AppIcon.Chart, UiTheme.Dpi(20), Color.White);

                // 标题栏按钮用矢量图标：字符「✕」在微软雅黑里没有字形，会渲染成方块
                btnMin.Icon = AppIcons.Get(AppIcon.Minimize, UiTheme.Dpi(14), Color.White);
                btnClose.Icon = AppIcons.Get(AppIcon.Close, UiTheme.Dpi(14), Color.White);

                pnlTitle.MouseDown += TitleBar_MouseDown;
                lblTitle.MouseDown += TitleBar_MouseDown;
                pnlTitleLine.MouseDown += TitleBar_MouseDown;
                picTitleIcon.MouseDown += TitleBar_MouseDown;

                btnMin.Click += delegate { WindowState = FormWindowState.Minimized; };
                // Ghost 变体的悬停底色是浅蓝，白图标会糊掉 ⇒ 悬停时图标同步转品牌色
                btnMin.MouseEnter += delegate
                {
                    btnMin.Icon = AppIcons.Get(AppIcon.Minimize, UiTheme.Dpi(14), UiKit.Brand);
                };
                btnMin.MouseLeave += delegate
                {
                    btnMin.Icon = AppIcons.Get(AppIcon.Minimize, UiTheme.Dpi(14), Color.White);
                };

                btnClose.MouseEnter += delegate { btnClose.Variant = FlatButtonVariant.Danger; };
                btnClose.MouseLeave += delegate { btnClose.Variant = FlatButtonVariant.Ghost; };
                btnClose.Click += delegate { Close(); };

                gripResize.BringToFront();                            // 缩放柄浮在内容之上（它的颜色与页面底色相同）
                gripResize.MouseDown += Grip_MouseDown;
                gripResize.Paint += Grip_Paint;

                UpdateWindowRegion();
            }
            catch { }
        }

        /// <summary>按住标题栏拖动窗口：无边框窗口没有系统标题栏，借系统的 WM_NCLBUTTONDOWN(HTCAPTION) 走原生拖动。</summary>
        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            try { ReleaseCapture(); SendMessage(Handle, WmNcLButtonDown, (IntPtr)HtCaption, IntPtr.Zero); }
            catch { }
        }

        /// <summary>右下角缩放柄：同样是借系统的边框缩放（HTBOTTOMRIGHT）。</summary>
        private void Grip_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || WindowState != FormWindowState.Normal) return;
            try { ReleaseCapture(); SendMessage(Handle, WmNcLButtonDown, (IntPtr)HtBottomRight, IntPtr.Zero); }
            catch { }
        }

        /// <summary>右下角缩放柄上的三道斜线（系统缩放柄的经典样式）。</summary>
        private void Grip_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                e.Graphics.Clear(gripResize.BackColor);
                int w = gripResize.Width - 1;
                int h = gripResize.Height - 1;
                using (Pen p = new Pen(UiKit.LineStrong, 1f))
                {
                    for (int i = 1; i <= 3; i++)
                    {
                        int off = UiTheme.Dpi(i * 5);
                        if (off >= w || off >= h) break;
                        e.Graphics.DrawLine(p, w - off, h, w, h - off);
                    }
                }
            }
            catch { }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateWindowRegion();
        }

        /// <summary>按当前尺寸重建窗口圆角；最大化时不裁角，否则四角会把全屏内容切掉。</summary>
        private void UpdateWindowRegion()
        {
            if (IsDisposed || Width <= 0 || Height <= 0) return;
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;
            if (WindowState == FormWindowState.Minimized) return;
            if (WindowState == FormWindowState.Maximized)
            {
                Region = null;
                return;
            }

            try
            {
                int corner = UiTheme.Dpi(WindowCornerRadius);
                IntPtr hRgn = CreateRoundRectRgn(0, 0, Width + 1, Height + 1, corner, corner);
                if (hRgn == IntPtr.Zero) return;

                System.Drawing.Region oldRegion = Region;
                Region = System.Drawing.Region.FromHrgn(hRgn);
                DeleteObject(hRgn);
                if (oldRegion != null) oldRegion.Dispose();
            }
            catch { }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)      // 没有标题栏，Esc 就是"关掉"
            {
                Close();
                e.Handled = true;
                return;
            }
            base.OnKeyDown(e);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            // 打开时别把焦点落在第一个筛选框上：空框带焦点蓝环、占位文字又不显示，看着像坏了
            ActiveControl = null;
        }

        // ---------------- 文案 ----------------

        private void ApplyLanguage()
        {
            try
            {
                this.Text = UiTheme.T("AlarmQueryForm", "title",
                    "报警统计与历史查询", "Alarm statistics and history", "Estadisticas e historial de alarmas");
                lblTitle.Text = this.Text;      // 无边框窗口，标题要自己画

                // 「编号 / 内容关键字」的说明文字改用输入框占位符，省掉一排裸露标签
                boxCode.PlaceholderText = UiTheme.T("AlarmQueryForm", "code", "编号", "Code", "Codigo");
                boxKeyword.PlaceholderText = UiTheme.T("AlarmQueryForm", "keyword", "内容关键字", "Keyword", "Palabra clave");

                btnToday.Text = UiTheme.T("AlarmQueryForm", "rangeToday", "今天", "Today", "Hoy");
                btnWeek.Text = UiTheme.T("AlarmQueryForm", "rangeWeek", "近 7 天", "Last 7 days", "7 dias");
                btnMonth.Text = UiTheme.T("AlarmQueryForm", "rangeMonth", "近 30 天", "Last 30 days", "30 dias");

                btnQuery.Text = UiTheme.T("AlarmQueryForm", "query", "查询", "Query", "Consultar");
                btnExport.Text = UiTheme.T("AlarmQueryForm", "export", "导出", "Export", "Exportar");

                btnQuery.Icon = AppIcons.Get(AppIcon.Search, 16, Color.White);
                btnExport.Icon = AppIcons.Get(AppIcon.Save, 16, UiKit.TextPrimary);

                // 类型分段：四项固定含义，切换语言时保留当前选择；首次进入默认「停机」
                int sel = segType.SelectedIndex;
                segType.SetItems(new string[]
                {
                    UiTheme.T("AlarmQueryForm", "typeAll", "全部", "All", "Todos"),
                    UiTheme.T("AlarmQueryForm", "typeWarn", "警告", "Warning", "Aviso"),
                    UiTheme.T("AlarmQueryForm", "typeAlarm", "报警", "Alarm", "Alarma"),
                    UiTheme.T("AlarmQueryForm", "typeStop", "停机", "Stop", "Parada")
                }, (sel < 0 || sel > TypeStopIndex) ? TypeStopIndex : sel);

                chartRank.Title = UiTheme.T("AlarmQueryForm", "rankTitle",
                    "报警号发生次数 Top 10", "Top 10 alarm codes", "Top 10 codigos");
                chartTrend.Title = UiTheme.T("AlarmQueryForm", "trendTitle",
                    "每日次数", "Count per day", "Por dia");
                chartRank.EmptyText = UiTheme.T("AlarmQueryForm", "empty", "无数据", "No data", "Sin datos");
                chartTrend.EmptyText = chartRank.EmptyText;

                colTime.HeaderText = UiTheme.T("AlarmQueryForm", "colTime", "时间", "Time", "Hora");
                colType.HeaderText = UiTheme.T("AlarmQueryForm", "colType", "类型", "Type", "Tipo");
                colStop.HeaderText = UiTheme.T("AlarmQueryForm", "colStop", "停机", "Stop", "Parada");
                colCode.HeaderText = UiTheme.T("AlarmQueryForm", "colCode", "编号", "Code", "Codigo");
                colContent.HeaderText = UiTheme.T("AlarmQueryForm", "colContent", "内容", "Content", "Contenido");
                colRecipe.HeaderText = UiTheme.T("AlarmQueryForm", "colRecipe", "配方", "Recipe", "Receta");
                colMode.HeaderText = UiTheme.T("AlarmQueryForm", "colMode", "模式", "Mode", "Modo");
                colUser.HeaderText = UiTheme.T("AlarmQueryForm", "colUser", "工号", "User", "Usuario");
                _stopText = colStop.HeaderText;

                UpdateDetailTitle();
            }
            catch { }
        }

        /// <summary>明细卡标题：按当前类型筛选切换「停机明细 / 报警明细」。</summary>
        private void UpdateDetailTitle()
        {
            try
            {
                lblDetail.Text = segType.SelectedIndex == TypeStopIndex
                    ? UiTheme.T("AlarmQueryForm", "detailStop", "停机明细", "Stop records", "Paradas")
                    : UiTheme.T("AlarmQueryForm", "detailAll", "报警明细", "Alarm records", "Registros");
            }
            catch { }
        }

        /// <summary>快捷时间范围：结束 = 今天，开始 = 今天往前 days 天，随即重查一次。</summary>
        private void SetQuickRange(int days)
        {
            try
            {
                DateTime to = DateTime.Today;
                dateFrom.Value = to.AddDays(-days);
                dateTo.Value = to;
                Reload();
                OperationLog.Write("AlarmQueryForm", string.Format("查询报警历史（快捷 {0} 天）：{1:yyyy-MM-dd} ~ {2:yyyy-MM-dd}，{3} 条",
                    days + 1, dateFrom.Value, dateTo.Value, _rows.Count));
            }
            catch { }
        }

        /// <summary>重新查询并刷新界面（外部再次打开窗口时调用）。</summary>
        public void Reload()
        {
            if (_loading) return;
            _loading = true;
            Cursor = Cursors.WaitCursor;
            try
            {
                UpdateDetailTitle();

                if (!AlarmHistory.RootReady())
                {
                    _rows = new List<AlarmRecord>();
                    lblStatus.ForeColor = UiTheme.WarnText;
                    lblStatus.Text = UiTheme.T("AlarmQueryForm", "noPath",
                        "未配置报警日志目录：请在【日志】页设置 AlarmPath",
                        "Alarm log path not configured (see the Logs page)",
                        "Ruta de registro de alarmas no configurada");
                    FillStats();
                    FillCharts();
                    FillGrid();
                    return;
                }

                int files;
                _rows = AlarmHistory.Query(dateFrom.Value, dateTo.Value, CurrentTypeFilter(),
                    boxCode.Value, boxKeyword.Value, out files);

                lblStatus.ForeColor = UiKit.TextSecondary;
                lblStatus.Text = string.Format(UiTheme.T("AlarmQueryForm", "status",
                    "共 {0} 条 · 停机 {1} 条 · 读取 {2} 个日志文件{3}",
                    "Total {0} · stop {1} · {2} files{3}",
                    "Total {0} · paradas {1} · {2} archivos{3}"),
                    _rows.Count, AlarmHistory.CountStop(_rows), files,
                    _rows.Count > MaxRowsShown
                        ? string.Format(UiTheme.T("AlarmQueryForm", "truncated",
                            " · 表格只显示前 {0} 条", " · table shows the first {0}", " · se muestran {0}"), MaxRowsShown)
                        : "");

                FillStats();
                FillCharts();
                FillGrid();
            }
            catch (Exception ex)
            {
                lblStatus.ForeColor = UiKit.Danger;
                lblStatus.Text = ex.Message;
            }
            finally
            {
                Cursor = Cursors.Default;
                _loading = false;
            }
        }

        /// <summary>分段控件的「全部 / 警告 / 报警 / 停机」映射成查询用的筛选值。</summary>
        private string CurrentTypeFilter()
        {
            switch (segType.SelectedIndex)
            {
                case 1: return AlarmHistory.FilterWarn;
                case 2: return AlarmHistory.FilterAlarm;
                case 3: return AlarmHistory.FilterStop;
                default: return AlarmHistory.FilterAll;
            }
        }

        // ---------------- 填充 ----------------

        /// <summary>统计数字条：停机次数 / 报警号种类 / 涉及天数 / 涉及配方 / 涉及工号。</summary>
        private void FillStats()
        {
            List<StatItem> items = new List<StatItem>();
            items.Add(new StatItem(UiTheme.T("AlarmQueryForm", "statStop", "停机次数", "Stops", "Paradas"),
                AlarmHistory.CountStop(_rows).ToString(), UiKit.Danger));
            items.Add(new StatItem(UiTheme.T("AlarmQueryForm", "statKinds", "报警号种类", "Distinct codes", "Codigos"),
                AlarmHistory.CountCodeKinds(_rows).ToString(), UiKit.Brand));
            items.Add(new StatItem(UiTheme.T("AlarmQueryForm", "statDays", "涉及天数", "Days", "Dias"),
                AlarmHistory.CountDays(_rows).ToString(), UiKit.BrandDark));
            items.Add(new StatItem(UiTheme.T("AlarmQueryForm", "statRecipes", "涉及配方", "Recipes", "Recetas"),
                AlarmHistory.CountRecipeKinds(_rows).ToString(), UiKit.TextPrimary));
            items.Add(new StatItem(UiTheme.T("AlarmQueryForm", "statUsers", "涉及工号", "Users", "Usuarios"),
                AlarmHistory.CountUserKinds(_rows).ToString(), UiKit.TextSecondary));
            statStrip1.SetItems(items);
        }

        /// <summary>两张图：左 = 报警号 Top 10，右 = 每日次数（按查询区间补零）。</summary>
        private void FillCharts()
        {
            chartRank.SetData(AlarmHistory.GroupByCode(_rows, 10));
            chartTrend.SetData(AlarmHistory.GroupByDay(_rows, dateFrom.Value, dateTo.Value));
        }

        /// <summary>
        /// 结果表格：行底色只用斑马纹，级别靠 RowPostPaint 的左侧状态色条与「类型」列的胶囊表达
        /// （整行铺浅红/浅琥珀在几十行的表里会糊成一片色块）。记录存进行 Tag 供绘制时取级别。
        /// </summary>
        private void FillGrid()
        {
            grdAlarm.SuspendLayout();
            try
            {
                grdAlarm.Rows.Clear();
                int n = Math.Min(_rows.Count, MaxRowsShown);
                for (int i = 0; i < n; i++)
                {
                    AlarmRecord r = _rows[i];
                    int idx = grdAlarm.Rows.Add(
                        r.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                        r.IsAlarm ? "E" : "W",
                        r.Stop ? _stopText : "",
                        r.Code,
                        r.Content,
                        r.RecipeName,
                        r.Mode,
                        r.User);
                    grdAlarm.Rows[idx].Tag = r;
                    if (r.Stop) grdAlarm.Rows[idx].Cells[colStop.Index].Style.ForeColor = UiKit.Danger;
                }
                grdAlarm.ClearSelection();
            }
            catch { }
            finally
            {
                grdAlarm.ResumeLayout();
            }
        }

        // ---------------- 事件 ----------------

        private void btnQuery_Click(object sender, EventArgs e)
        {
            Reload();
            try
            {
                OperationLog.Write("AlarmQueryForm", string.Format("查询报警历史：{0:yyyy-MM-dd} ~ {1:yyyy-MM-dd}，{2} 条（停机 {3} 条）",
                    dateFrom.Value, dateTo.Value, _rows.Count, AlarmHistory.CountStop(_rows)));
            }
            catch { }
        }

        /// <summary>把当前查询结果导出成 CSV（UTF-8 带 BOM，Excel 直接打开不乱码）。</summary>
        private void btnExport_Click(object sender, EventArgs e)
        {
            if (_rows.Count == 0)
            {
                lblStatus.ForeColor = UiTheme.WarnText;
                lblStatus.Text = UiTheme.T("AlarmQueryForm", "nothingToExport",
                    "当前没有可导出的记录", "Nothing to export", "Nada que exportar");
                return;
            }

            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Filter = "CSV (*.csv)|*.csv";
                dlg.FileName = string.Format("Alarm_{0:yyyyMMdd}-{1:yyyyMMdd}.csv", dateFrom.Value, dateTo.Value);
                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                string err = AlarmHistory.Export(_rows, dlg.FileName);
                lblStatus.ForeColor = err.Length == 0 ? UiKit.Success : UiKit.Danger;
                lblStatus.Text = err.Length == 0
                    ? string.Format(UiTheme.T("AlarmQueryForm", "exportOk", "已导出 {0} 条 → {1}",
                        "Exported {0} rows to {1}", "Exportadas {0} filas a {1}"), _rows.Count, dlg.FileName)
                    : err;
            }
        }
    }
}
