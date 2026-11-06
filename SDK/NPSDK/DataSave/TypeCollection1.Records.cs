using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DataSave
{
    public partial class TypeCollection1
    {
        private static readonly ConcurrentDictionary<string, object> recordLocks = new ConcurrentDictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        /// <summary>校验具名字段后追加一条日志；时间自动生成，写入失败向调用方抛出。</summary>
        public string AppendRecord(string[] values)
        {
            return AppendRecordAt(DateTime.Now, values);
        }

        /// <summary>按指定业务日期写入日志并保留文件原有编码。</summary>
        private string AppendRecordAt(DateTime now, string[] values)
        {
            string root = RecordRoot();
            string[] columns = Items.Select(item => item.Name).ToArray();
            if (columns.Length < 2 || columns[0] != "Time" || columns.Any(string.IsNullOrWhiteSpace)
                || columns.Distinct(StringComparer.OrdinalIgnoreCase).Count() != columns.Length || values == null || values.Length != columns.Length - 1)
                throw new InvalidDataException("Log columns and values do not match.");
            lock (recordLocks.GetOrAdd(root, key => new object()))
            {
                string folder = Path.Combine(root, now.ToString("yyyy"), now.ToString("MM"));
                Directory.CreateDirectory(folder);
                string path = Path.Combine(folder, now.ToString("yyyy-MM-dd") + ".csv");
                string header = CsvLine(columns);
                if (File.Exists(path) && new FileInfo(path).Length > 0 && !(ReadCsv(path).FirstOrDefault() ?? new string[0]).SequenceEqual(columns))
                    path = Path.Combine(folder, now.ToString("yyyy-MM-dd") + "." + HeaderKey(header) + ".csv");
                bool empty = !File.Exists(path) || new FileInfo(path).Length == 0;
                Encoding encoding = empty ? new UTF8Encoding(true) : FileEncoding(path);
                using (FileStream file = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read))
                using (StreamWriter writer = new StreamWriter(file, encoding))
                {
                    if (empty) writer.WriteLine(header);
                    writer.WriteLine(CsvLine(new[] { now.ToString("yyyy-MM-dd HH:mm:ss:fff", CultureInfo.InvariantCulture) }.Concat(values)));
                    writer.Flush(); file.Flush(true);
                }
                return path;
            }
        }

        /// <summary>跨年月读取日志并按文件列名对齐；兼容旧编码及包含逗号、引号、换行的 CSV。</summary>
        private List<string[]> ReadRecords(DateTime start, DateTime end)
        {
            if (end.Date < start.Date || (end.Date - start.Date).TotalDays > 366) throw new ArgumentOutOfRangeException("end");
            string root = RecordRoot();
            List<string> columns = new List<string>();
            List<Dictionary<string, string>> records = new List<Dictionary<string, string>>();
            lock (recordLocks.GetOrAdd(root, key => new object()))
                foreach (string path in RecordFiles(root, start, end)) ReadDay(path, columns, records);
            if (columns.Count == 0) return new List<string[]>();
            List<string[]> result = new List<string[]> { columns.ToArray() };
            result.AddRange(records.OrderBy(row => row.ContainsKey("Time") ? row["Time"] : "", StringComparer.Ordinal)
                .Select(row => columns.Select(column => row.ContainsKey(column) ? row[column] : "").ToArray()));
            return result;
        }

        /// <summary>只枚举所选日期对应的年月目录和日志版本文件。</summary>
        private static IEnumerable<string> RecordFiles(string root, DateTime start, DateTime end)
        {
            for (DateTime day = start.Date; day <= end.Date; day = day.AddDays(1))
            {
                string folder = Path.Combine(root, day.ToString("yyyy"), day.ToString("MM"));
                if (!Directory.Exists(folder)) continue;
                foreach (string path in Directory.GetFiles(folder, day.ToString("yyyy-MM-dd") + "*.csv").OrderBy(value => value)) yield return path;
            }
        }

        /// <summary>从文件自身表头读取历史值，保留增删字段之前的记录。</summary>
        private static void ReadDay(string path, List<string> columns, List<Dictionary<string, string>> records)
        {
            using (IEnumerator<string[]> rows = ReadCsv(path).GetEnumerator())
            {
                if (!rows.MoveNext()) return;
                string[] header = rows.Current;
                foreach (string column in header.Where(column => !string.IsNullOrEmpty(column)))
                    if (!columns.Contains(column)) columns.Add(column);
                while (rows.MoveNext())
                {
                    string[] values = rows.Current;
                    if (values.Length > header.Length)
                    {
                        int end = values.Length;
                        while (end > header.Length && string.IsNullOrEmpty(values[end - 1])) end--;
                        values = values.Take(header.Length - 1).Concat(new[] { string.Join(",", values.Skip(header.Length - 1).Take(end - header.Length + 1)) }).ToArray();
                    }
                    Dictionary<string, string> row = new Dictionary<string, string>(StringComparer.Ordinal);
                    for (int index = 0; index < header.Length; index++)
                        if (!string.IsNullOrEmpty(header[index])) row[header[index]] = index < values.Length ? values[index] : "";
                    records.Add(row);
                }
            }
        }

        /// <summary>相对日志目录以程序目录为基准，不依赖 VS 或文件对话框的当前目录。</summary>
        private string RecordRoot()
        {
            if (string.IsNullOrWhiteSpace(SavePath)) throw new InvalidDataException("Log directory is empty.");
            return Path.GetFullPath(Path.IsPathRooted(SavePath) ? SavePath : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SavePath));
        }

        /// <summary>识别已有文件编码，保持历史文件的原编码。</summary>
        private static Encoding FileEncoding(string path)
        {
            using (StreamReader reader = new StreamReader(path, Encoding.Default, true)) { reader.Peek(); return reader.CurrentEncoding; }
        }

        /// <summary>为同一天不同字段版本生成确定的文件后缀，避免破坏历史表头。</summary>
        private static string HeaderKey(string header)
        {
            using (SHA256 hash = SHA256.Create()) return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(header))).Replace("-", "").Substring(0, 16);
        }

        /// <summary>转义 CSV 特殊字符并保留用户原始字符串。</summary>
        private static string CsvLine(IEnumerable<string> values)
        {
            return string.Join(",", values.Select(value => value == null ? "" : value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) < 0 ? value : "\"" + value.Replace("\"", "\"\"") + "\""));
        }

        /// <summary>按引号状态解析完整 CSV 记录，不把字段内的换行当作新记录。</summary>
        private static IEnumerable<string[]> ReadCsv(string path)
        {
            using (StreamReader reader = new StreamReader(path, Encoding.Default, true))
            {
                List<string> row = new List<string>(); StringBuilder cell = new StringBuilder(); bool quoted = false;
                int next;
                while ((next = reader.Read()) != -1)
                {
                    char value = (char)next;
                    if (value == '"' && (quoted || cell.Length == 0))
                    {
                        if (quoted && reader.Peek() == '"') { reader.Read(); cell.Append('"'); }
                        else quoted = !quoted;
                        continue;
                    }
                    if (value == ',' && !quoted) { row.Add(cell.ToString()); cell.Clear(); continue; }
                    if ((value == '\n' || value == '\r') && !quoted)
                    {
                        if (value == '\r' && reader.Peek() == '\n') reader.Read();
                        row.Add(cell.ToString()); cell.Clear(); yield return row.ToArray(); row.Clear(); continue;
                    }
                    cell.Append(value);
                }
                if (quoted) throw new InvalidDataException("Unterminated CSV value: " + path);
                if (cell.Length > 0 || row.Count > 0) { row.Add(cell.ToString()); yield return row.ToArray(); }
            }
        }
    }
}
