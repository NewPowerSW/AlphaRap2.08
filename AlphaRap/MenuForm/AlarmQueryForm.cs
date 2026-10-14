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
    /// 自上而下 = 查询条 / 停机统计数字条 / Top 报警号与每日趋势两张图 / 明细表，均为白色卡片。
    /// 默认只看会停机的报警；按需创建、复用同一个实例（见 MainForm.OpenAlarmQuery），显示时重新查询一次。
    /// </summary>
    public partial class AlarmQueryForm : Form
    {
        /// <summary>明细表格最多显示的行数，避免一次查询几十万行把界面拖死。</summary>
        private const int MaxRowsShown = 5000;

        /// <summary>类型下拉里「停机」的下标（默认选中项：统计只算停机报警）。</summary>
        private const int TypeStopIndex = 3;

        /// <summary>表格「停机」列的显示文字（随语言变化）。</summary>
        private string _stopText = "停机";

        /// <summary>E 类报警行的底色（浅红）。</summary>
        private static readonly Color RowAlarmBack = Color.FromArgb(255, 238, 238);
        /// <summary>W 类警告行的底色（浅琥珀）。</summary>
        private static readonly Color RowWarnBack = Color.FromArgb(255, 249, 235);

        private List<AlarmRecord> _rows = new List<AlarmRecord>();
        private bool _loading;

        public AlarmQueryForm()
        {
            InitializeComponent();
            UiTheme.ApplyDpi(this);
            UiKit.StyleGrid(grdAlarm);

            _loading = true;
            dtpFrom.Value = DateTime.Today;
            dtpTo.Value = DateTime.Today;
            _loading = false;

            btnQuery.Click += btnQuery_Click;
            btnExport.Click += btnExport_Click;

            RoundCard(pnlBarOuter);
            RoundCard(pnlCardOuter);
            pnlBarOuter.Resize += delegate { RoundCard(pnlBarOuter); };
            pnlCardOuter.Resize += delegate { RoundCard(pnlCardOuter); };

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

        /// <summary>把卡片外框裁成圆角；描边靠「外层底色=线色 + Padding=1」留出，不自绘。</summary>
        private static void RoundCard(Panel p)
        {
            if (p == null || p.Width <= 1 || p.Height <= 1) return;
            try
            {
                using (GraphicsPath path = UiKit.RoundRect(new Rectangle(0, 0, p.Width, p.Height), UiTheme.Dpi(10)))
                {
                    Region old = p.Region;
                    p.Region = new Region(path);
                    if (old != null) old.Dispose();
                }
            }
            catch { }
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
                btnMin.TextColorOverride = UiKit.TextSecondary;
                btnClose.TextColorOverride = UiKit.TextSecondary;

                pnlTitle.MouseDown += TitleBar_MouseDown;
                lblTitle.MouseDown += TitleBar_MouseDown;
                pnlTitleLine.MouseDown += TitleBar_MouseDown;

                btnMin.Click += delegate { WindowState = FormWindowState.Minimized; };
                btnClose.MouseEnter += delegate
                {
                    btnClose.Variant = FlatButtonVariant.Danger;      // 悬停变红，与系统关闭键一致
                    btnClose.TextColorOverride = Color.White;
                };
                btnClose.MouseLeave += delegate
                {
                    btnClose.Variant = FlatButtonVariant.Ghost;
                    btnClose.TextColorOverride = UiKit.TextSecondary;
                };
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

        // ---------------- 文案 ----------------

        private void ApplyLanguage()
        {
            try
            {
                this.Text = UiTheme.T("AlarmQueryForm", "title",
                    "报警统计与历史查询", "Alarm statistics and history", "Estadisticas e historial de alarmas");
                lblTitle.Text = this.Text;      // 无边框窗口，标题要自己画

                lblFrom.Text = UiTheme.T("AlarmQueryForm", "from", "开始", "From", "Desde");
                lblTo.Text = UiTheme.T("AlarmQueryForm", "to", "结束", "To", "Hasta");
                lblType.Text = UiTheme.T("AlarmQueryForm", "type", "类型", "Type", "Tipo");
                lblCode.Text = UiTheme.T("AlarmQueryForm", "code", "编号", "Code", "Codigo");
                lblKeyword.Text = UiTheme.T("AlarmQueryForm", "keyword", "内容关键字", "Keyword", "Palabra clave");
                btnQuery.Text = UiTheme.T("AlarmQueryForm", "query", "查询", "Query", "Consultar");
                btnExport.Text = UiTheme.T("AlarmQueryForm", "export", "导出", "Export", "Exportar");

                btnQuery.Icon = AppIcons.Get(AppIcon.Search, 16, Color.White);
                btnExport.Icon = AppIcons.Get(AppIcon.Save, 16, UiKit.TextPrimary);

                // 类型下拉：四项固定含义，切换语言时保留当前选择；首次进入默认「停机」
                int sel = cboType.SelectedIndex;
                cboType.Items.Clear();
                cboType.Items.Add(UiTheme.T("AlarmQueryForm", "typeAll", "全部", "All", "Todos"));
                cboType.Items.Add(UiTheme.T("AlarmQueryForm", "typeWarn", "警告", "Warning", "Aviso"));
                cboType.Items.Add(UiTheme.T("AlarmQueryForm", "typeAlarm", "报警", "Alarm", "Alarma"));
                cboType.Items.Add(UiTheme.T("AlarmQueryForm", "typeStop", "停机", "Stop", "Parada"));
                cboType.SelectedIndex = (sel < 0 || sel > TypeStopIndex) ? TypeStopIndex : sel;

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
                lblDetail.Text = cboType.SelectedIndex == TypeStopIndex
                    ? UiTheme.T("AlarmQueryForm", "detailStop", "停机明细", "Stop records", "Paradas")
                    : UiTheme.T("AlarmQueryForm", "detailAll", "报警明细", "Alarm records", "Registros");
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
                _rows = AlarmHistory.Query(dtpFrom.Value, dtpTo.Value, CurrentTypeFilter(),
                    txtCode.Text, txtKeyword.Text, out files);

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

        /// <summary>下拉框的「全部 / 警告 / 报警 / 停机」映射成查询用的筛选值。</summary>
        private string CurrentTypeFilter()
        {
            switch (cboType.SelectedIndex)
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
            chartTrend.SetData(AlarmHistory.GroupByDay(_rows, dtpFrom.Value, dtpTo.Value));
        }

        /// <summary>结果表格：E 类浅红底、W 类浅琥珀底，超过上限只显示前 N 条。</summary>
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
                    grdAlarm.Rows[idx].DefaultCellStyle.BackColor = r.IsAlarm ? RowAlarmBack : RowWarnBack;
                    grdAlarm.Rows[idx].DefaultCellStyle.ForeColor = r.IsAlarm ? UiKit.Danger : UiTheme.WarnText;
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
                    dtpFrom.Value, dtpTo.Value, _rows.Count, AlarmHistory.CountStop(_rows)));
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
                dlg.FileName = string.Format("Alarm_{0:yyyyMMdd}-{1:yyyyMMdd}.csv", dtpFrom.Value, dtpTo.Value);
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
