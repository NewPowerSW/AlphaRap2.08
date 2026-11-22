using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using AlphaRap.Configuration;
using AlphaRap.FlowProgramming;
using DataSave;

namespace AlphaRap
{
    public partial class DataForm
    {
        private const int HomeLogLimit = 100;
        private readonly LogModuleStore logStore = new LogModuleStore();
        private readonly object homeLogSync = new object();
        private long homeLogRevision;
        private HomeLogPreview homeLogPreview;

        private sealed class HomeLogPreview
        {
            internal DateTime Day;
            internal bool Simulation;
            internal string[] Columns;
            internal List<string[]> Rows = new List<string[]>();
        }

        /// <summary>在绑定或所选模块的持久化记录变化后递增，供首页合并刷新。</summary>
        public long HomeLogRevision => Interlocked.Read(ref homeLogRevision);
        private readonly List<TypeCollection1> customLogs = new List<TypeCollection1>();

        private readonly List<RegisteredLog> registeredLogs = new List<RegisteredLog>();

        private sealed class RegisteredLog
        {
            internal readonly string Id;
            internal readonly TypeCollection1 Source;
            internal readonly string Directory;
            internal string LastMessage = string.Empty;

            /// <summary>记录应用登记的日志实例与根目录。</summary>
            internal RegisteredLog(string id, TypeCollection1 source)
            {
                Id = id;
                Source = source;
                Directory = source.SavePath;
            }
        }

        /// <summary>登记应用的固定日志实例，并沿用给定的流程资源键。</summary>
        protected void RegisterLogSources(params KeyValuePair<string, TypeCollection1>[] sources)
        {
            foreach (KeyValuePair<string, TypeCollection1> entry in sources)
            {
                if (string.IsNullOrWhiteSpace(entry.Key) || entry.Value == null || string.IsNullOrWhiteSpace(entry.Value.SavePath) ||
                    FindRegisteredLog(entry.Key) != null || registeredLogs.Any(log => ReferenceEquals(log.Source, entry.Value)))
                    throw new InvalidOperationException(FlowText.T("logInvalid"));

                registeredLogs.Add(new RegisteredLog(entry.Key, entry.Value));
            }

            if (!UiTheme.IsDesignTime) RefreshLogSources(false);
        }

        /// <summary>按稳定键取得应用登记的日志实例，未登记时返回空。</summary>
        public TypeCollection1 GetLogSource(string id) { return FindRegisteredLog(id)?.Source; }

        /// <summary>在应用固定日志中查找指定资源键。</summary>
        private RegisteredLog FindRegisteredLog(string id) { return registeredLogs.Find(log => log.Id == id); }

        /// <summary>释放数据页持有的固定日志组件和动态查询对象。</summary>
        private void DisposeLogSources()
        {
            foreach (RegisteredLog log in registeredLogs) log.Source.Dispose();
            foreach (TypeCollection1 log in customLogs) log.Dispose();
        }

        /// <summary>读取日志配置独立副本，模块身份不随名称或配方改变。</summary>
        public List<LogModuleDefinition> GetLogModules(bool includeDraft = true) { return logStore.Read(includeDraft); }

        /// <summary>按实例提供日志模块能力；编辑读取草稿，运行只读取已保存定义。</summary>
        public IEnumerable<FlowLogModule> GetFlowLogs(bool includeDraft = false)
        {
            foreach (RegisteredLog entry in registeredLogs)
                yield return new FlowLogModule("builtin/" + entry.Id, entry.Source,
                    entry.Source.Items.Skip(1).Select(field => new ProductionField { Name = field.Name, Type = "Text" }));
            List<LogModuleDefinition> definitions = logStore.Read(includeDraft); ValidateBuiltinLogs(definitions);
            foreach (LogModuleDefinition definition in definitions)
                yield return new FlowLogModule(definition.Id, FlowLogModule.Create(definition), definition.Fields);
        }

        /// <summary>按已保存的客户日志名称写入一条具名记录，返回写入文件的路径。</summary>
        public string WriteLogByName(string name, IDictionary<string, object> values)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(nameof(name));
            lock (homeLogSync)
            {
                var definitions = GetLogModules(false);
                ValidateBuiltinLogs(definitions);
                var definition = definitions.Find(module => string.Equals(module.Name, name, StringComparison.OrdinalIgnoreCase));
                if (definition == null)
                    throw new KeyNotFoundException(string.Format(ApplicationHost.Text("ResourceAccess", "logMissing", "找不到已保存的日志模块「{0}」。",
                        "The saved log module '{0}' was not found.", "No se encontró el módulo de registro guardado '{0}'."), name));

                return WriteNamedLog(definition, values, SysPara.Simulation);
            }
        }

        /// <summary>写入已保存日志，只有落盘成功的绑定记录才进入首页缓存。</summary>
        private string WriteNamedLog(LogModuleDefinition definition, IDictionary<string, object> values, bool simulation)
        {
            using (var source = FlowLogModule.Create(definition))
            {
                var log = new FlowLogModule(definition.Id, source, definition.Fields);
                if (!definition.ShowOnHome) return log.AppendRecord(values, simulation);
                return log.AppendRecord(values, simulation, written: (time, row) => AddHomeLogRecord(time, row, simulation));
            }
        }

        /// <summary>追加到同一天及运行模式的预览，保留最近记录。</summary>
        private void AddHomeLogRecord(DateTime time, string[] row, bool simulation)
        {
            if (homeLogPreview != null && homeLogPreview.Day == time.Date && homeLogPreview.Simulation == simulation)
            {
                homeLogPreview.Rows.Add(FormatHomeLogRow(time, row));
                if (homeLogPreview.Rows.Count > HomeLogLimit) homeLogPreview.Rows.RemoveAt(0);
            }
            Interlocked.Increment(ref homeLogRevision);
        }

        /// <summary>取得已保存的首页来源，未绑定时由页面使用原生产记录。</summary>
        internal LogModuleDefinition GetHomeLogModule() => GetLogModules(false).SingleOrDefault(module => module.ShowOnHome);

        /// <summary>从已选日志加载当天最近记录，再由写入接口增量更新。</summary>
        public List<string[]> ReadHomeLogPreview(DateTime day, bool simulation, out string[] columns)
        {
            lock (homeLogSync)
            {
                if (homeLogPreview == null || homeLogPreview.Day != day.Date || homeLogPreview.Simulation != simulation)
                    homeLogPreview = LoadHomeLogPreview(day.Date, simulation);
                columns = homeLogPreview.Columns == null ? null : (string[])homeLogPreview.Columns.Clone();
                return Enumerable.Reverse(homeLogPreview.Rows).Select(row => (string[])row.Clone()).ToList();
            }
        }

        /// <summary>按保存模块的字段顺序生成可显示的当天快照。</summary>
        private HomeLogPreview LoadHomeLogPreview(DateTime day, bool simulation)
        {
            var preview = new HomeLogPreview { Day = day, Simulation = simulation };
            LogModuleDefinition definition = GetHomeLogModule();
            if (definition == null) return preview;
            preview.Columns = new[] { "Time" }.Concat(definition.Fields.Select(field => field.Name)).ToArray();
            List<ProductionRecord> rows = QueryLogModule(definition, day, day, "", simulation, out _, out _);
            preview.Rows = rows.Skip(Math.Max(0, rows.Count - HomeLogLimit)).Select(row => FormatHomeLogRow(row.Time, row.Values)).ToList();
            return preview;
        }

        /// <summary>按首页时间列和日志字段生成一行显示值。</summary>
        private static string[] FormatHomeLogRow(DateTime time, string[] values) => new[] { time.ToString("HH:mm:ss") }.Concat(values).ToArray();

        /// <summary>使用日志文件的表头读取选中模块，并按当前模块字段投影查询结果。</summary>
        internal static List<ProductionRecord> QueryLogModule(LogModuleDefinition definition, DateTime from, DateTime to, string keyword,
            bool simulation, out int fileCount, out int total)
        {
            if (to < from) { DateTime first = to; to = from; from = first; }
            if ((to.Date - from.Date).TotalDays > 366) to = from.Date.AddDays(366);
            List<string[]> rows;
            using (var source = FlowLogModule.Create(definition, simulation)) rows = source.ReadDataFromToFile(from, to, out fileCount);
            total = 0;
            var records = new List<ProductionRecord>();
            if (rows.Count <= 1) return records;

            string[] columns = definition.Fields.Select(field => field.Name).ToArray();
            string[] header = rows[0];
            int timeIndex = Array.IndexOf(header, "Time");
            if (timeIndex < 0) return records;
            int[] indices = columns.Select(column => Array.IndexOf(header, column)).ToArray();
            string filter = (keyword ?? "").Trim();
            foreach (string[] row in rows.Skip(1))
            {
                DateTime time = ProductionDataStore.ParseTime(row[timeIndex]);
                if (time == DateTime.MinValue) continue;
                total++;
                string[] values = indices.Select(index => index < 0 ? "" : row[index]).ToArray();
                if (filter.Length == 0 || values.Any(value => value.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0))
                    records.Add(new ProductionRecord { Time = time, Columns = columns, Values = values });
            }
            return records;
        }

        /// <summary>提交窗口草稿并刷新查询模块，尚未保存的定义不供运行流程使用。</summary>
        public void SetLogModulesDraft(List<LogModuleDefinition> definitions)
        {
            FlowPrograms.RequireIdle();
            if (definitions == null) throw new InvalidOperationException(FlowText.T("logInvalid"));
            ValidateBuiltinLogs(definitions);
            HashSet<string> retained = new HashSet<string>(definitions.Select(module => "log/" + module.Id));
            foreach (FlowModuleEditor module in FlowPrograms.Modules.Where(module => !module.Program.Removed))
                if (module.Program.Steps.Any(step => step.Target != null && step.Target.StartsWith("log/") && !step.Target.StartsWith("log/builtin/") && !retained.Contains(step.Target)))
                    throw new InvalidOperationException(FlowText.T("logInUse"));
            logStore.SetDraft(definitions); RefreshLogSources(true);
        }

        /// <summary>校验并暂存日志草稿，由主保存事务决定何时发布。</summary>
        internal void ValidateLogModulesDraft() { logStore.ValidateDraft(); ValidateBuiltinLogs(logStore.Read(true)); }
        /// <summary>将日志定义写入主保存的暂存文件。</summary>
        internal void WriteLogModulesDraft(string path) { logStore.WriteDraft(path); }
        /// <summary>主保存完成后发布日志定义并刷新查询入口。</summary>
        internal void AcceptLogModulesDraft()
        {
            lock (homeLogSync)
            {
                logStore.Accept();
                homeLogPreview = null;
                Interlocked.Increment(ref homeLogRevision);
            }
            RefreshLogSources(false);
        }
        /// <summary>放弃未保存的日志修改并恢复查询入口。</summary>
        internal void DiscardLogModulesDraft() { logStore.Discard(); RefreshLogSources(false); }

        /// <summary>自定义模块不能覆盖内置报警或其他系统日志的名称与目录。</summary>
        private void ValidateBuiltinLogs(IEnumerable<LogModuleDefinition> definitions)
        {
            foreach (LogModuleDefinition module in definitions)
            {
                if (module == null || string.IsNullOrWhiteSpace(module.Directory)) throw new InvalidOperationException(FlowText.T("logInvalid"));
                string path = LogModuleStore.ResolveDirectory(module.Directory);
                if (registeredLogs.Any(entry => string.Equals(entry.Source.Text, module.Name, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(LogModuleStore.ResolveDirectory(entry.Source.SavePath), path, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(LogModuleStore.ResolveDirectory(entry.Directory), path, StringComparison.OrdinalIgnoreCase))) throw new InvalidOperationException(FlowText.T("logInvalid"));
            }
        }

        /// <summary>保持内置日志实例，替换动态查询对象并释放旧组件。</summary>
        private void RefreshLogSources(bool includeDraft)
        {
            List<LogModuleDefinition> definitions = logStore.Read(includeDraft);
            ValidateBuiltinLogs(definitions);
            data1.ParameterSet.Clear();
            foreach (RegisteredLog entry in registeredLogs) data1.ParameterSet.Add(entry.Source);
            foreach (TypeCollection1 log in customLogs) log.Dispose();
            customLogs.Clear();
            foreach (LogModuleDefinition definition in definitions)
            {
                TypeCollection1 log = FlowLogModule.Create(definition); customLogs.Add(log); data1.ParameterSet.Add(log);
            }
            data1.ReloadSources();
        }

        /// <summary>打开单层模块与字段编辑窗口；确定只更新草稿。</summary>
        private void ManageLogModules(object sender, EventArgs args)
        {
            try
            {
                FlowPrograms.RequireIdle();
                using (LogModulesForm dialog = new LogModulesForm(this)) dialog.ShowDialog(this);
            }
            catch (Exception error) { MessageBox.Show(string.Format(FlowText.T("error"), error.Message), FlowText.T("log")); }
        }
    }
}
