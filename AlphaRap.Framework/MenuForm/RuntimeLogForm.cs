using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>现有运行、错误日志的只读查询页面。</summary>
    public partial class RuntimeLogForm : Form
    {
        private RuntimeLogResult result = new RuntimeLogResult();
        private CancellationTokenSource queryCancellation;
        private int queryVersion;
        private bool loading, applyingTexts, criteriaDirty, languageSubscribed, bindingLogs;

        /// <summary>创建固定布局；设计期不读取日志文件或访问设备。</summary>
        public RuntimeLogForm()
        {
            InitializeComponent();
            UiTheme.ApplyDpi(this);
            UiKit.StyleGrid(gridLogs);
            applyingTexts = true;
            dateFrom.Value = dateTo.Value = DateTime.Today;
            applyingTexts = false;
            ApplyTexts();
            if (UiTheme.IsDesignTime)
            {
                BindResult(new RuntimeLogResult { Records = new System.Collections.Generic.List<RuntimeLogRecord>
                {
                    new RuntimeLogRecord { Time = DateTime.Today.AddHours(9), Level = RuntimeLogHistory.Run, Content = "Load station: Ready" },
                    new RuntimeLogRecord { Time = DateTime.Today.AddHours(8), Level = RuntimeLogHistory.Error, Content = "Vision inspection: Connection unavailable" }
                }, MatchedCount = 2 });
                return;
            }
            FrameworkLanguage.RegisterAndApplyLanguage(this, Name);
            FrameworkLanguage.LanguageChanged += LanguageChanged;
            languageSubscribed = true;
        }

        /// <summary>切换语言时只刷新标题与显示格式，保留查询条件、原始内容和选择。</summary>
        private void LanguageChanged(object sender, EventArgs e)
        {
            ApplyTexts();
            gridLogs.Invalidate();
        }

        /// <summary>从三语底稿更新可见文字，保留类型筛选的稳定索引。</summary>
        private void ApplyTexts()
        {
            applyingTexts = true;
            int index = Math.Max(0, levelFilter.SelectedIndex);
            lblTitle.Text = T("lblTitle", "运行日志", "Runtime logs", "Registro de ejecución");
            lblSubtitle.Text = T("lblSubtitle", "查看设备运行与错误记录，日志内容保留原文", "View machine runtime and error records in their original text", "Consulte los registros de ejecución y errores en su texto original");
            lblFrom.Text = T("lblFrom", "日期", "Date", "Fecha");
            lblTo.Text = T("lblTo", "至", "to", "a");
            btnToday.Text = T("btnToday", "今天", "Today", "Hoy");
            btnWeek.Text = T("btnWeek", "近 7 天", "Last 7 days", "Últimos 7 días");
            autoRefresh.Text = T("autoRefresh", "自动刷新（3 秒）", "Auto refresh (3 s)", "Actualizar (3 s)");
            lblLevel.Text = T("lblLevel", "类型", "Type", "Tipo");
            lblKeyword.Text = T("lblKeyword", "关键字", "Keyword", "Buscar");
            btnQuery.Text = T("btnQuery", "查询", "Query", "Consultar");
            btnExport.Text = T("btnExport", "导出", "Export", "Exportar");
            lblDetail.Text = T("lblDetail", "日志详情", "Log details", "Detalle del registro");
            gridLogs.Columns["colTime"].HeaderText = T("gridLogs_colTime", "时间", "Time", "Hora");
            gridLogs.Columns["colLevel"].HeaderText = T("gridLogs_colLevel", "类型", "Type", "Tipo");
            gridLogs.Columns["colContent"].HeaderText = T("gridLogs_colContent", "内容", "Content", "Contenido");
            levelFilter.BeginUpdate();
            levelFilter.Items.Clear();
            levelFilter.Items.AddRange(new object[] { T("filter_All", "全部", "All", "Todos"), LevelText(RuntimeLogHistory.Run), LevelText(RuntimeLogHistory.Error) });
            levelFilter.SelectedIndex = index;
            levelFilter.EndUpdate();
            applyingTexts = false;
            RefreshStatus();
        }

        /// <summary>按页面命名空间读取完整三语底稿。</summary>
        private static string T(string key, string chinese, string english, string spanish)
        {
            return UiTheme.T("RuntimeLogForm", key, chinese, english, spanish);
        }

        /// <summary>显示日志类别，保留数据中的 RUN 和 ERROR 代码。</summary>
        private static string LevelText(string level)
        {
            return level == RuntimeLogHistory.Error ? T("level_Error", "错误", "Error", "Error") : T("level_Run", "运行", "Runtime", "Ejecución");
        }

        /// <summary>页面显示时查询并启动刷新；离开页面时取消读取。</summary>
        protected override async void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (refreshTimer == null || UiTheme.IsDesignTime) return;
            refreshTimer.Enabled = Visible && autoRefresh.Checked;
            if (!Visible) { CancelQuery(); return; }
            await ReloadAsync();
        }

        /// <summary>查询当前条件，重复查询会取消旧任务，旧结果不会覆盖新条件。</summary>
        private async Task ReloadAsync()
        {
            if (IsDisposed) return;
            if (dateTo.Value.Date < dateFrom.Value.Date || (dateTo.Value.Date - dateFrom.Value.Date).TotalDays >= RuntimeLogHistory.MaxDays)
            {
                lblStatus.Text = T("range_Error", "请选择有效日期范围，最多 366 天。", "Choose a valid date range of up to 366 days.", "Seleccione un intervalo válido de hasta 366 días.");
                return;
            }
            if (ApplicationHost.Data == null)
            {
                lblStatus.Text = T("source_Unavailable", "日志数据源尚未初始化", "Log sources have not been initialized", "Los registros aún no están inicializados");
                return;
            }
            CancelQuery();
            int version = queryVersion;
            CancellationTokenSource cancellation = new CancellationTokenSource();
            queryCancellation = cancellation;
            DateTime from = dateFrom.Value.Date, to = dateTo.Value.Date;
            string level = levelFilter.SelectedIndex == 1 ? RuntimeLogHistory.Run : levelFilter.SelectedIndex == 2 ? RuntimeLogHistory.Error : RuntimeLogHistory.All;
            string keyword = keywordFilter.Text;
            string runDirectory = ApplicationHost.Data.GetLogSource("LogRun")?.SavePath;
            string errorDirectory = ApplicationHost.Data.GetLogSource("LogError")?.SavePath;
            loading = true;
            btnQuery.Enabled = btnExport.Enabled = false;
            RefreshStatus();
            bool succeeded = false;
            try
            {
                RuntimeLogResult loaded = await Task.Run(() => RuntimeLogHistory.Query(runDirectory, errorDirectory, from, to, level, keyword, cancellation.Token), cancellation.Token);
                if (IsDisposed || version != queryVersion || cancellation.IsCancellationRequested) return;
                criteriaDirty = false;
                BindResult(loaded);
                succeeded = true;
            }
            catch (OperationCanceledException) { }
            catch (Exception error)
            {
                if (!IsDisposed && version == queryVersion)
                    lblStatus.Text = string.Format(T("query_Failed", "日志读取失败：{0}", "Could not read logs: {0}", "No se pudieron leer los registros: {0}"), error.Message);
            }
            finally
            {
                if (ReferenceEquals(queryCancellation, cancellation)) queryCancellation = null;
                cancellation.Dispose();
                if (!IsDisposed && version == queryVersion)
                {
                    loading = false;
                    btnQuery.Enabled = true;
                    btnExport.Enabled = !criteriaDirty && result.Records.Count > 0;
                    if (succeeded) RefreshStatus();
                }
            }
        }

        /// <summary>保留当前选中记录及滚动位置，显示新的只读查询快照。</summary>
        private void BindResult(RuntimeLogResult loaded)
        {
            RuntimeLogRecord selected = gridLogs.CurrentRow == null ? null : gridLogs.CurrentRow.DataBoundItem as RuntimeLogRecord;
            int first = gridLogs.FirstDisplayedScrollingRowIndex;
            result = loaded;
            bindingLogs = true;
            try
            {
                gridLogs.DataSource = new BindingList<RuntimeLogRecord>(result.Records);
                int index = selected == null ? -1 : result.Records.FindIndex(row => row.Time == selected.Time && row.Level == selected.Level && row.Content == selected.Content);
                if (index >= 0) gridLogs.CurrentCell = gridLogs.Rows[index].Cells[0];
                if (first >= 0 && gridLogs.Rows.Count > 0) gridLogs.FirstDisplayedScrollingRowIndex = Math.Min(first, gridLogs.Rows.Count - 1);
            }
            finally { bindingLogs = false; }
            ShowDetail(this, EventArgs.Empty);
            RefreshStatus();
        }

        /// <summary>显示匹配数量、上限和不完整读取状态。</summary>
        private void RefreshStatus()
        {
            if (loading) { lblStatus.Text = T("status_Loading", "正在读取日志…", "Reading logs…", "Leyendo registros…"); return; }
            if (criteriaDirty) { lblStatus.Text = T("status_Changed", "条件已修改，请点击查询。", "Filters changed. Click Query.", "Filtros modificados. Pulse Consultar."); return; }
            lblStatus.Text = string.Format(T("status_Count", "显示 {0} / {1} 条 · 读取 {2} 个文件", "Showing {0} / {1} records · {2} files read", "Se muestran {0} / {1} registros · {2} archivos leídos"), result.Records.Count, result.MatchedCount, result.FileCount);
            if (result.MatchedCount > result.Records.Count)
                lblStatus.Text += T("status_Limited", " · 仅显示最新 10000 条，请缩小范围", " · Latest 10000 only; narrow the filters", " · Solo los últimos 10000; reduzca el intervalo");
            if (result.FailedDays > 0 || result.InvalidRecords > 0)
                lblStatus.Text += string.Format(T("status_Partial", " · {0} 个日期/类型读取失败，{1} 条记录格式无效", " · {0} day/type reads failed, {1} malformed records", " · {0} lecturas fallidas, {1} registros no válidos"), result.FailedDays, result.InvalidRecords);
        }

        /// <summary>点击查询时读取当前条件。</summary>
        private async void QueryClick(object sender, EventArgs e) { await ReloadAsync(); }

        /// <summary>输入框回车时执行查询。</summary>
        private async void KeywordKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            await ReloadAsync();
        }

        /// <summary>使用今天作为完整日期范围并立即查询。</summary>
        private async void TodayClick(object sender, EventArgs e) { SetRange(0); await ReloadAsync(); }

        /// <summary>使用最近七天作为完整日期范围并立即查询。</summary>
        private async void WeekClick(object sender, EventArgs e) { SetRange(6); await ReloadAsync(); }

        /// <summary>设置快捷日期，保留类型和关键字。</summary>
        private void SetRange(int days)
        {
            applyingTexts = true;
            dateFrom.Value = DateTime.Today.AddDays(-days);
            dateTo.Value = DateTime.Today;
            applyingTexts = false;
            CriteriaChanged(this, EventArgs.Empty);
        }

        /// <summary>条件变化后取消旧任务，导出必须先完成新条件查询。</summary>
        private void CriteriaChanged(object sender, EventArgs e)
        {
            if (applyingTexts) return;
            CancelQuery();
            criteriaDirty = true;
            btnExport.Enabled = false;
            RefreshStatus();
        }

        /// <summary>取消后台任务，并使已经排队的旧结果失效。</summary>
        private void CancelQuery()
        {
            queryVersion++;
            if (queryCancellation != null) queryCancellation.Cancel();
            loading = false;
            if (!IsDisposed && btnQuery != null) btnQuery.Enabled = true;
        }

        /// <summary>页面可见且条件未编辑时刷新当前快照，不与已有查询重叠。</summary>
        private async void RefreshTick(object sender, EventArgs e)
        {
            if (!Visible || loading || criteriaDirty) return;
            await ReloadAsync();
        }

        /// <summary>仅在页面可见时启用自动刷新。</summary>
        private void AutoRefreshChanged(object sender, EventArgs e)
        {
            refreshTimer.Enabled = !UiTheme.IsDesignTime && Visible && autoRefresh.Checked;
        }

        /// <summary>显示选中记录的完整原文，复制时保留换行。</summary>
        private void ShowDetail(object sender, EventArgs e)
        {
            if (bindingLogs || gridLogs.IsDisposed || detailText.IsDisposed) return;
            RuntimeLogRecord row = gridLogs.CurrentRow == null ? null : gridLogs.CurrentRow.DataBoundItem as RuntimeLogRecord;
            detailText.Text = row == null ? "" : row.Content;
        }

        /// <summary>只格式化时间和类型显示，不改写记录。</summary>
        private void FormatCell(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            RuntimeLogRecord row = gridLogs.Rows[e.RowIndex].DataBoundItem as RuntimeLogRecord;
            if (row == null) return;
            if (gridLogs.Columns[e.ColumnIndex].Name == "colLevel") { e.Value = LevelText(row.Level); e.FormattingApplied = true; }
            if (row.Level == RuntimeLogHistory.Error) e.CellStyle.ForeColor = Color.FromArgb(204, 52, 65);
        }

        /// <summary>将已显示的查询快照导出为 CSV，不修改原始日志。</summary>
        private void ExportClick(object sender, EventArgs e)
        {
            if (criteriaDirty || result.Records.Count == 0) return;
            RuntimeLogRecord[] snapshot = result.Records.ToArray();
            using (SaveFileDialog dialog = new SaveFileDialog { Filter = "CSV (*.csv)|*.csv", FileName = "RuntimeLog-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".csv" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    RuntimeLogHistory.Export(snapshot, dialog.FileName);
                    lblStatus.Text = string.Format(T("export_Done", "已导出 {0} 条记录", "Exported {0} records", "Se exportaron {0} registros"), snapshot.Length);
                }
                catch (Exception error)
                {
                    MessageBox.Show(this, string.Format(T("export_Failed", "导出失败：{0}", "Export failed: {0}", "Error al exportar: {0}"), error.Message), lblTitle.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>释放前终止查询，并解除静态语言事件。</summary>
        private void ReleasePage()
        {
            CancelQuery();
            bindingLogs = true;
            if (languageSubscribed) FrameworkLanguage.LanguageChanged -= LanguageChanged;
            languageSubscribed = false;
        }
    }
}
