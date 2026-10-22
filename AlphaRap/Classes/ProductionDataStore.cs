using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace AlphaRap
{
    /// <summary>一条生产数据记录：时间 + 该行写入时的列名与值。</summary>
    public sealed class ProductionRecord
    {
        public DateTime Time;
        /// <summary>写入这行时该文件的表头（列名），与 <see cref="Values"/> 一一对应。</summary>
        public string[] Columns = new string[0];
        public string[] Values = new string[0];

        /// <summary>按列名取值；列名不存在或该列在旧文件里没有时返回空串。</summary>
        public string Get(string column)
        {
            if (string.IsNullOrEmpty(column)) return "";
            for (int i = 0; i < Columns.Length && i < Values.Length; i++)
                if (string.Equals(Columns[i], column, StringComparison.OrdinalIgnoreCase))
                    return Values[i];
            return "";
        }
    }

    /// <summary>
    /// 生产数据的**本地保存与按时间段查询**。
    ///
    /// 落盘格式沿用工程里报警日志的套路：**一天一个文件**
    ///   {root}\{yyyy}\{MM}\{yyyy-MM-dd}.csv
    /// 首行是该文件写入时的表头（列名），之后每行一条记录。读的时候**按列名取值**，
    /// 所以后来改了列名也能读旧文件。
    ///
    /// root 取日志页配置的 ProducPath；没配时退回 &lt;当前目录&gt;\ModuleData\ProductionData。
    ///
    /// 同一天新增字段时先原子扩展文件表头，旧记录保留原值；写入由同一锁串行处理。
    /// </summary>
    public static class ProductionDataStore
    {
        private static readonly object WriteSync = new object();
        private static string recordIndexPath;
        private static long recordIndexLength = -1;
        private static readonly HashSet<string> recordIndex = new HashSet<string>(StringComparer.Ordinal);

        /// <summary>时间列的列名（固定第一列）。</summary>
        public const string TimeColumn = "Time";

        /// <summary>一天一个文件；最多一次查一年，防止误选日期把界面卡死。</summary>
        private const int MaxDays = 366;

        // ---------------- 路径 ----------------

        /// <summary>程序指定的根目录；非空时优先于配置（也让存储层能脱离 MiddleLayer 单独测试）。</summary>
        private static string _rootOverride = "";

        /// <summary>
        /// 指定生产数据的根目录。传空串恢复为"按配置取"。
        /// 不调用时取日志页的 ProducPath，没有则退回 &lt;当前目录&gt;\ModuleData\ProductionData。
        /// </summary>
        public static void SetRoot(string dir)
        {
            _rootOverride = string.IsNullOrEmpty(dir) ? "" : dir;
        }

        /// <summary>生产数据根目录（日志页 ProducPath 之下再放一层 ProductionData）。</summary>
        public static string RootPath()
        {
            if (_rootOverride.Length > 0) return _rootOverride;

            string baseDir = "";
            try
            {
                if (MiddleLayer.LogF != null)
                    baseDir = MiddleLayer.LogF.GetSettingValue("Path", "ProducPath");
            }
            catch { baseDir = ""; }

            if (string.IsNullOrEmpty(baseDir) || baseDir.Trim().Length == 0)
                return Path.Combine(@".\ModuleData", "ProductionData");

            return Path.Combine(baseDir.Trim(), "ProductionData");
        }

        /// <summary>列定义文件（列名一行一条）。</summary>
        private static string ColumnsFile()
        {
            return Path.Combine(RootPath(), "columns.txt");
        }

        /// <summary>某一天的数据文件。</summary>
        public static string DayFile(DateTime day)
        {
            return Path.Combine(Path.Combine(Path.Combine(RootPath(), day.ToString("yyyy")),
                day.ToString("MM")), day.ToString("yyyy-MM-dd") + ".csv");
        }

        // ---------------- 列定义 ----------------

        /// <summary>读列定义；没有或读失败返回空数组。</summary>
        public static string[] GetColumns()
        {
            try
            {
                string f = ColumnsFile();
                if (!File.Exists(f)) return new string[0];

                string[] lines = File.ReadAllLines(f);
                List<string> cols = new List<string>();
                for (int i = 0; i < lines.Length; i++)
                {
                    string s = lines[i].Trim();
                    if (s.Length > 0) cols.Add(s);
                }
                return cols.ToArray();
            }
            catch { return new string[0]; }
        }

        /// <summary>写列定义（会建目录）。返回错误信息，空串表示成功。</summary>
        public static string SetColumns(string[] columns)
        {
            try
            {
                if (columns == null || columns.Length == 0) return "";

                string dir = RootPath();
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < columns.Length; i++)
                    sb.Append(columns[i] ?? "").Append("\r\n");
                File.WriteAllText(ColumnsFile(), sb.ToString(), new UTF8Encoding(true));
                return "";
            }
            catch (Exception ex) { return ex.Message; }
        }

        // ---------------- 追加 ----------------

        /// <summary>按列定义追加一条记录（时间取当前时间）。返回错误信息，空串表示成功。</summary>
        public static string Append(string[] values)
        {
            return Append(DateTime.Now, values);
        }

        /// <summary>
        /// 按列定义追加一条记录到当天文件。文件不存在时先写表头。
        /// 行按**该文件自己的表头顺序**落盘：值取自 <see cref="GetColumns"/> 的列序，
        /// 文件里没有的列丢弃、文件有而本次没给的列写空。
        /// </summary>
        public static string Append(DateTime time, string[] values)
        {
            lock (WriteSync) return AppendCore(time, values);
        }

        /// <summary>Serialize production writes and expand changed headers before writing values.</summary>
        private static string AppendCore(DateTime time, string[] values)
        {
            try
            {
                string[] cols = GetColumns();
                if (cols.Length == 0) return "尚未设置列（表头）";

                string file = DayFile(time);
                string dir = Path.GetDirectoryName(file);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

                string[] fileCols = ReadHeader(file);
                bool needHeader = fileCols.Length == 0;
                if (needHeader) fileCols = cols;
                else fileCols = ExpandHeader(file, fileCols, cols);

                StringBuilder line = new StringBuilder();
                line.Append(time.ToString("yyyy-MM-dd HH:mm:ss"));
                for (int i = 0; i < fileCols.Length; i++)
                {
                    line.Append(',');
                    line.Append(Csv(ValueOf(cols, values, fileCols[i])));
                }

                using (StreamWriter w = new StreamWriter(file, true, new UTF8Encoding(true)))
                {
                    if (needHeader)
                    {
                        StringBuilder h = new StringBuilder(TimeColumn);
                        for (int i = 0; i < fileCols.Length; i++) h.Append(',').Append(Csv(fileCols[i]));
                        w.WriteLine(h.ToString());
                    }
                    w.WriteLine(line.ToString());
                }
                return "";
            }
            catch (Exception ex) { return ex.Message; }
        }

        /// <summary>从"列名数组 + 值数组"里取某列的值；没有该列返回空串。</summary>
        private static string ValueOf(string[] cols, string[] values, string column)
        {
            if (cols == null || values == null) return "";
            for (int i = 0; i < cols.Length && i < values.Length; i++)
                if (string.Equals(cols[i], column, StringComparison.OrdinalIgnoreCase))
                    return values[i] ?? "";
            return "";
        }

        /// <summary>Write named fields with a stable record identity; retries do not append duplicates.</summary>
        public static string AppendNamed(DateTime time, IDictionary<string, string> fields, string recordId)
        {
            lock (WriteSync)
            {
                if (string.IsNullOrWhiteSpace(recordId)) return "RecordId is required.";
                foreach (KeyValuePair<string, string> field in fields)
                    if ((field.Value ?? "").IndexOfAny(new[] { '\r', '\n' }) >= 0) return "Production values must not contain line breaks: " + field.Key;
                string file = DayFile(time);
                LoadRecordIndex(file);
                if (recordIndex.Contains(recordId)) return "";
                List<string> columns = new List<string>(GetColumns());
                foreach (string field in fields.Keys)
                    if (!columns.Contains(field)) columns.Add(field);
                string error = SetColumns(columns.ToArray());
                if (!string.IsNullOrEmpty(error)) return error;
                string[] values = new string[columns.Count];
                for (int index = 0; index < columns.Count; index++) fields.TryGetValue(columns[index], out values[index]);
                error = AppendCore(time, values);
                if (!string.IsNullOrEmpty(error)) return error;
                recordIndex.Add(recordId);
                recordIndexLength = new FileInfo(file).Length;
                return "";
            }
        }

        /// <summary>Rebuild the single-file record index when another writer changes its length.</summary>
        private static void LoadRecordIndex(string file)
        {
            long length = File.Exists(file) ? new FileInfo(file).Length : 0;
            if (recordIndexPath == file && recordIndexLength == length) return;
            recordIndex.Clear();
            recordIndexPath = file;
            recordIndexLength = length;
            if (length == 0) return;
            string[] lines = DecodeLines(File.ReadAllBytes(file));
            string[] header = SplitLine(lines[0]);
            int column = Array.IndexOf(header, "RecordId");
            if (column < 0) return;
            for (int row = 1; row < lines.Length; row++)
            {
                string[] values = SplitLine(lines[row]);
                if (column < values.Length && !string.IsNullOrEmpty(values[column])) recordIndex.Add(values[column]);
            }
        }

        /// <summary>Atomically add new columns to today's file without dropping old values.</summary>
        private static string[] ExpandHeader(string file, string[] previous, string[] requested)
        {
            List<string> combined = new List<string>(previous);
            foreach (string column in requested) if (!combined.Contains(column)) combined.Add(column);
            if (combined.Count == previous.Length) return previous;
            string[] lines = DecodeLines(File.ReadAllBytes(file));
            StringBuilder text = new StringBuilder(Csv(TimeColumn));
            foreach (string column in combined) text.Append(',').Append(Csv(column));
            text.AppendLine();
            string suffix = new string(',', combined.Count - previous.Length);
            for (int row = 1; row < lines.Length; row++)
                if (!string.IsNullOrEmpty(lines[row])) text.Append(lines[row]).Append(suffix).AppendLine();
            string temporary = file + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                File.WriteAllText(temporary, text.ToString(), new UTF8Encoding(true));
                File.Replace(temporary, file, file + ".bak");
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
            return combined.ToArray();
        }

        // ---------------- 查询 ----------------

        /// <summary>
        /// 读 [from, to]（含首尾）范围内的生产数据。from 晚于 to 时自动对调。
        /// </summary>
        /// <param name="keyword">关键字：任一列的值包含它即算命中；空串表示不过滤。</param>
        /// <param name="fileCount">实际读到的文件数。</param>
        /// <param name="total">命中关键字前的总条数。</param>
        public static List<ProductionRecord> Query(DateTime from, DateTime to, string keyword,
            out int fileCount, out int total)
        {
            List<ProductionRecord> list = new List<ProductionRecord>();
            fileCount = 0;
            total = 0;

            if (to < from) { DateTime t = from; from = to; to = t; }
            if ((to.Date - from.Date).TotalDays > MaxDays) to = from.Date.AddDays(MaxDays);

            keyword = (keyword ?? "").Trim();

            for (DateTime d = from.Date; d <= to.Date; d = d.AddDays(1))
            {
                string file = DayFile(d);
                if (!File.Exists(file)) continue;

                string[] lines;
                try { lines = DecodeLines(File.ReadAllBytes(file)); }
                catch (Exception) { continue; }
                if (lines.Length < 2) continue;

                fileCount++;
                string[] cols = SplitLine(lines[0]);
                if (cols.Length > 0 && string.Equals(cols[0], TimeColumn, StringComparison.OrdinalIgnoreCase))
                {
                    string[] rest = new string[cols.Length - 1];
                    Array.Copy(cols, 1, rest, 0, rest.Length);
                    cols = rest;
                }

                for (int i = 1; i < lines.Length; i++)
                {
                    if (lines[i].Length == 0) continue;

                    string[] cells = SplitLine(lines[i]);
                    if (cells.Length == 0) continue;

                    DateTime t = ParseTime(cells[0]);
                    if (t == DateTime.MinValue) continue;

                    ProductionRecord r = new ProductionRecord();
                    r.Time = t;
                    r.Columns = cols;
                    r.Values = new string[cols.Length];
                    for (int k = 0; k < cols.Length; k++)
                        r.Values[k] = (k + 1 < cells.Length) ? cells[k + 1] : "";

                    total++;
                    if (keyword.Length > 0 && !Match(r, keyword)) continue;
                    list.Add(r);
                }
            }

            list.Sort(delegate (ProductionRecord a, ProductionRecord b) { return a.Time.CompareTo(b.Time); });
            return list;
        }

        private static bool Match(ProductionRecord r, string keyword)
        {
            for (int i = 0; i < r.Values.Length; i++)
                if (!string.IsNullOrEmpty(r.Values[i])
                    && r.Values[i].IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }

        /// <summary>查询结果里出现过的所有列名（按首次出现顺序，用于建表头）。</summary>
        public static string[] DistinctColumns(List<ProductionRecord> rows)
        {
            List<string> cols = new List<string>();
            for (int i = 0; i < rows.Count; i++)
            {
                string[] c = rows[i].Columns;
                for (int k = 0; k < c.Length; k++)
                {
                    bool has = false;
                    for (int m = 0; m < cols.Count; m++)
                        if (string.Equals(cols[m], c[k], StringComparison.OrdinalIgnoreCase)) { has = true; break; }
                    if (!has) cols.Add(c[k]);
                }
            }
            return cols.ToArray();
        }

        /// <summary>出现过的天数。</summary>
        public static int CountDays(List<ProductionRecord> rows)
        {
            HashSet<string> set = new HashSet<string>();
            for (int i = 0; i < rows.Count; i++) set.Add(rows[i].Time.ToString("yyyy-MM-dd"));
            return set.Count;
        }

        /// <summary>区间内哪天没有记录 → 补 0，便于画趋势。</summary>
        public static List<KeyValuePair<string, int>> GroupByDay(List<ProductionRecord> rows,
            DateTime from, DateTime to)
        {
            if (to < from) { DateTime t = from; from = to; to = t; }

            Dictionary<string, int> map = new Dictionary<string, int>();
            for (int i = 0; i < rows.Count; i++)
            {
                string k = rows[i].Time.ToString("yyyy-MM-dd");
                int n;
                map[k] = map.TryGetValue(k, out n) ? n + 1 : 1;
            }

            List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>();
            for (DateTime d = from.Date; d <= to.Date; d = d.AddDays(1))
            {
                string k = d.ToString("yyyy-MM-dd");
                int n;
                list.Add(new KeyValuePair<string, int>(k, map.TryGetValue(k, out n) ? n : 0));
            }
            return list;
        }

        // ---------------- 导出 ----------------

        /// <summary>把查询结果写成 CSV（带表头）。成功返回空串，失败返回原因。</summary>
        public static string Export(List<ProductionRecord> rows, string file)
        {
            try
            {
                string[] cols = DistinctColumns(rows);
                using (StreamWriter w = new StreamWriter(file, false, new UTF8Encoding(true)))
                {
                    StringBuilder h = new StringBuilder(TimeColumn);
                    for (int i = 0; i < cols.Length; i++) h.Append(',').Append(Csv(cols[i]));
                    w.WriteLine(h.ToString());

                    for (int i = 0; i < rows.Count; i++)
                    {
                        ProductionRecord r = rows[i];
                        StringBuilder line = new StringBuilder(r.Time.ToString("yyyy-MM-dd HH:mm:ss"));
                        for (int k = 0; k < cols.Length; k++)
                            line.Append(',').Append(Csv(r.Get(cols[k])));
                        w.WriteLine(line.ToString());
                    }
                }
                return "";
            }
            catch (Exception ex) { return ex.Message; }
        }

        // ---------------- 读写小工具 ----------------

        /// <summary>读文件首行（表头，不含第一列的 Time）；文件不存在返回空数组。</summary>
        private static string[] ReadHeader(string file)
        {
            try
            {
                if (!File.Exists(file)) return new string[0];

                using (StreamReader r = new StreamReader(file, Encoding.UTF8, true))
                {
                    string first = r.ReadLine();
                    if (string.IsNullOrEmpty(first)) return new string[0];

                    string[] cells = SplitLine(first);
                    if (cells.Length == 0) return new string[0];
                    if (string.Equals(cells[0], TimeColumn, StringComparison.OrdinalIgnoreCase))
                    {
                        string[] rest = new string[cells.Length - 1];
                        Array.Copy(cells, 1, rest, 0, rest.Length);
                        return rest;
                    }
                    return cells;   // 没有时间列的老文件，整行都算列名
                }
            }
            catch { return new string[0]; }
        }

        /// <summary>按字节读并拆行；UTF-8（可带 BOM）优先，解不开退回 GBK，否则中文会乱码。</summary>
        private static string[] DecodeLines(byte[] raw)
        {
            string text;
            if (raw.Length >= 3 && raw[0] == 0xEF && raw[1] == 0xBB && raw[2] == 0xBF)
            {
                text = Encoding.UTF8.GetString(raw, 3, raw.Length - 3);
            }
            else
            {
                try { text = new UTF8Encoding(false, true).GetString(raw); }
                catch (DecoderFallbackException)
                {
                    try { text = Encoding.GetEncoding(936).GetString(raw); }
                    catch { text = Encoding.Default.GetString(raw); }
                }
            }
            return text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        }

        /// <summary>支持双引号包裹（值里可以有逗号）。</summary>
        private static string[] SplitLine(string line)
        {
            if (string.IsNullOrEmpty(line)) return new string[0];

            List<string> cells = new List<string>();
            StringBuilder cur = new StringBuilder();
            bool quoted = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (quoted)
                {
                    if (c == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"') { cur.Append('"'); i++; }
                        else quoted = false;
                    }
                    else cur.Append(c);
                }
                else if (c == '"') quoted = true;
                else if (c == ',') { cells.Add(cur.ToString()); cur.Length = 0; }
                else cur.Append(c);
            }
            cells.Add(cur.ToString());
            return cells.ToArray();
        }

        private static string Csv(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            if (s.IndexOf(',') < 0 && s.IndexOf('"') < 0 && s.IndexOf('\n') < 0 && s.IndexOf('\r') < 0) return s;
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        }

        /// <summary>解析时间列（形如 2026-10-14 08:58:41）；失败返回 DateTime.MinValue。</summary>
        private static DateTime ParseTime(string s)
        {
            DateTime t;
            string[] fmts = { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss:fff",
                              "yyyy-MM-dd HH:mm:ss.fff", "yyyy/MM/dd HH:mm:ss" };
            if (DateTime.TryParseExact((s ?? "").Trim(), fmts, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out t)) return t;
            if (DateTime.TryParse((s ?? "").Trim(), out t)) return t;
            return DateTime.MinValue;
        }
    }
}
