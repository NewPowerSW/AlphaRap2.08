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
                    new RuntimeLogRecord { Time = DateTime.Today.AddHours(9), Level = RuntimeLogHistory.Error,
                        Content = "FlowChart [Module=LoadStation/LoadStation, Entry=preview, Run=preview] Node=Wait for input [613205fab6794e3d909008e00c0307f6] System.TimeoutException: Timeout after 5000 ms" },
                    new RuntimeLogRecord { Time = DateTime.Today.AddHours(8), Level = RuntimeLogHistory.Run, Content = "LoadStation+++++++++++++++Pause" }
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
            ShowDetail(this, EventArgs.Empty);
        }

        /// <summary>从三语底稿更新可见文字，保留类型筛选的稳定索引。</summary>
        private void ApplyTexts()
        {
            applyingTexts = true;
            int index = Math.Max(0, levelFilter.SelectedIndex);
            lblTitle.Text = T("lblTitle", "运行日志", "Runtime logs", "Registro de ejecución");
            lblSubtitle.Text = T("lblSubtitle", "按工位、节点查看运行事件；完整报错见原始日志", "Review events by station and node; full errors are in Original log", "Consulte eventos por estación y nodo; errores completos en Registro original");
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
            summaryPage.Text = T("summaryPage", "详情", "Details", "Detalles");
            rawPage.Text = T("rawPage", "原始日志", "Original log", "Registro original");
            gridLogs.Columns["colTime"].HeaderText = T("gridLogs_colTime", "时间", "Time", "Hora");
            gridLogs.Columns["colLevel"].HeaderText = T("gridLogs_colLevel", "类型", "Type", "Tipo");
            gridLogs.Columns["colStation"].HeaderText = T("gridLogs_colStation", "工位", "Station", "Estación");
            gridLogs.Columns["colNode"].HeaderText = T("gridLogs_colNode", "节点", "Node", "Nodo");
            gridLogs.Columns["colContent"].HeaderText = T("gridLogs_colContent", "事件", "Event", "Evento");
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

        /// <summary>同时提供可读详情和完整原文，保持当前详情页签。</summary>
        private void ShowDetail(object sender, EventArgs e)
        {
            if (bindingLogs || gridLogs.IsDisposed || detailText.IsDisposed) return;
            RuntimeLogRecord row = gridLogs.CurrentRow == null ? null : gridLogs.CurrentRow.DataBoundItem as RuntimeLogRecord;
            string original = row == null ? "" : row.Content;
            if (detailText.Text != original) { detailText.Text = original; detailText.Select(0, 0); }
            if (row == null) { summaryText.Clear(); return; }
            string location = string.Join("  /  ", new[] { row.Station, row.Node }.Where(value => value.Length > 0));
            string summary = row.Time.ToString("yyyy-MM-dd HH:mm:ss.fff") + "  ·  " + LevelText(row.Level)
                + (location.Length == 0 ? "" : Environment.NewLine + location)
                + Environment.NewLine + Environment.NewLine + EventText(row);
            if (summaryText.Text != summary) { summaryText.Text = summary; summaryText.Select(0, 0); }
        }

        /// <summary>将流程事件转换成简短三语提示，未知消息保持原意。</summary>
        private static string EventText(RuntimeLogRecord row)
        {
            RuntimeLogDescription info = row.Description;
            switch (info.Event)
            {
                case "Started": case "Start": return T("event_Started", "流程启动", "Flow started", "Flujo iniciado");
                case "Initial": return T("event_Initial", "开始初始化", "Initialization started", "Inicialización iniciada");
                case "ContinusStart": return T("event_Resume", "继续运行", "Resumed", "Reanudado");
                case "Running": return T("event_Running", "正在执行", "Executing", "Ejecutando");
                case "Waiting": return T("event_Waiting", "等待条件满足", "Waiting for condition", "Esperando condición");
                case "Pause": return T("event_Pause", "收到暂停指令", "Pause requested", "Pausa solicitada");
                case "Paused": return T("event_Paused", "已暂停", "Paused", "En pausa");
                case "RecoveryPaused": return T("event_RecoveryPaused", "故障暂停，等待处理", "Paused after fault; awaiting action", "Pausa por fallo; esperando intervención");
                case "DebugPaused": return T("event_DebugPaused", "调试暂停", "Debug pause", "Pausa de depuración");
                case "Completed": return T("event_Completed", "节点执行完成", "Node completed", "Nodo completado");
                case "Finished": return T("event_Finished", "流程结束", "Flow finished", "Flujo finalizado");
                case "Stop": return T("event_Stop", "收到停止指令", "Stop requested", "Parada solicitada");
                case "Cancelled": return T("event_Cancelled", "执行已取消", "Execution cancelled", "Ejecución cancelada");
                case "Failed": return T("event_Failed", "节点执行失败，详见原始日志", "Node failed; see Original log", "Error en nodo; consulte Registro original");
                case "FinishedError": return T("event_FinishedError", "流程异常结束：", "Flow ended with error: ", "Flujo finalizado con error: ") + info.Message;
                case "Exit":
                    if (info.Exit == "NEXT") return T("event_Next", "转到下一节点", "Continue to next node", "Continuar al siguiente nodo");
                    if (info.Exit == "End") return T("event_End", "到达流程终点", "Flow end reached", "Fin del flujo alcanzado");
                    return string.Format(T("event_Exit", "节点出口：{0}", "Node exit: {0}", "Salida del nodo: {0}"), info.Exit);
                case "Timeout":
                    if (info.TimeoutMilliseconds == 0) return T("event_Timeout", "等待超时", "Wait timed out", "Tiempo de espera agotado");
                    return string.Format(T("event_TimeoutDuration", "等待超时（{0} 秒）", "Wait timed out ({0} s)", "Tiempo de espera agotado ({0} s)"), (info.TimeoutMilliseconds / 1000m).ToString("0.###"));
                default: return info.Message;
            }
        }

        /// <summary>列表只展示事件摘要；完整原文留在详情，错误保持醒目的红色。</summary>
        private void FormatCell(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            RuntimeLogRecord row = gridLogs.Rows[e.RowIndex].DataBoundItem as RuntimeLogRecord;
            if (row == null) return;
            if (gridLogs.Columns[e.ColumnIndex].Name == "colLevel") { e.Value = LevelText(row.Level); e.FormattingApplied = true; }
            if (gridLogs.Columns[e.ColumnIndex].Name == "colContent")
            {
                string message = EventText(row).Replace("\r", " ").Replace("\n", " ");
                e.Value = message.Length > 240 ? message.Substring(0, 240) + "…" : message;
                e.FormattingApplied = true;
            }
            if (row.Level == RuntimeLogHistory.Error)
            {
                e.CellStyle.ForeColor = Color.FromArgb(204, 52, 65);
                e.CellStyle.SelectionForeColor = Color.FromArgb(204, 52, 65);
            }
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
