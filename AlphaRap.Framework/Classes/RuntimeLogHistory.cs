using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace AlphaRap
{
    /// <summary>一条运行或错误日志，保留写盘时的原始内容。</summary>
    public sealed class RuntimeLogRecord
    {
        private string content;
        private RuntimeLogDescription description;
        public DateTime Time { get; set; }
        public string Level { get; set; }
        public string Content { get { return content; } set { content = value; description = null; } }
        public string Station { get { return Description.Station; } }
        public string Node { get { return Description.Node; } }
        internal RuntimeLogDescription Description { get { return description ?? (description = RuntimeLogDescription.Parse(content)); } }
        internal long Sequence { get; set; }
    }

    /// <summary>从已有日志提取显示字段，不改写原文、标识符或记录类别。</summary>
    internal sealed class RuntimeLogDescription
    {
        private static readonly Regex NodeHeader = new Regex(@"^Node=(.*?) \[(?:[a-fA-F0-9]{32}|[a-fA-F0-9]{8}(?:-[a-fA-F0-9]{4}){3}-[a-fA-F0-9]{12})\](?: |$)");
        private static readonly Regex Timeout = new Regex(@"Timeout after (\d+) ms");
        private static readonly Regex ExceptionType = new Regex(@"^(?:[\w]+\.)*[\w]+Exception:\s*");
        internal string Station { get; private set; } = "";
        internal string Node { get; private set; } = "";
        internal string Event { get; private set; } = "";
        internal string Message { get; private set; } = "";
        internal string Exit { get; private set; } = "";
        internal long TimeoutMilliseconds { get; private set; }

        /// <summary>识别流程前缀及旧控制命令，未知格式完整保留为消息。</summary>
        internal static RuntimeLogDescription Parse(string content)
        {
            RuntimeLogDescription result = new RuntimeLogDescription();
            string message = content ?? "";
            if (message.StartsWith("FlowChart [Module=", StringComparison.Ordinal))
            {
                int moduleEnd = message.IndexOf(", Entry=", StringComparison.Ordinal);
                int headerEnd = message.IndexOf("] ", StringComparison.Ordinal);
                if (moduleEnd > 17 && headerEnd > moduleEnd)
                {
                    string module = message.Substring(17, moduleEnd - 17);
                    result.Station = module.Substring(module.LastIndexOf('/') + 1);
                    message = message.Substring(headerEnd + 2);
                    Match node = NodeHeader.Match(message);
                    if (node.Success) { result.Node = node.Groups[1].Value; message = message.Substring(node.Length); }
                    result.ReadFlowEvent(message);
                    return result;
                }
            }
            int command = message.IndexOf("+++++++++++++++", StringComparison.Ordinal);
            if (command > 0)
            {
                result.Station = message.Substring(0, command);
                result.Event = message.Substring(command + 15).Trim();
                result.Message = result.Event;
                return result;
            }
            result.Message = message;
            return result;
        }

        /// <summary>提取流程状态、出口和首行异常；内部取消异常不会覆盖实际超时原因。</summary>
        private void ReadFlowEvent(string message)
        {
            string firstLine = message.Split(new[] { '\r', '\n' }, 2)[0];
            Match timeout = Timeout.Match(firstLine);
            long milliseconds;
            if (timeout.Success && long.TryParse(timeout.Groups[1].Value, out milliseconds))
            {
                Event = "Timeout";
                TimeoutMilliseconds = milliseconds;
                return;
            }
            if (message.StartsWith("Exit=", StringComparison.Ordinal))
            {
                int end = message.IndexOf(", Next=", StringComparison.Ordinal);
                Exit = end < 0 ? message.Substring(5) : message.Substring(5, end - 5);
                Event = "Exit";
                return;
            }
            if (message.StartsWith("Finished: ", StringComparison.Ordinal))
            {
                Event = message == "Finished: Cancelled" ? "Cancelled" : "FinishedError";
                Message = ExceptionType.Replace(firstLine.Substring(10), "");
                return;
            }
            Event = firstLine;
            Message = ExceptionType.Replace(firstLine, "");
            int inner = Message.IndexOf(" ---> ", StringComparison.Ordinal);
            if (inner >= 0) Message = Message.Substring(0, inner);
            if (Node.Length > 0 && Message.StartsWith(Node + ": ", StringComparison.Ordinal)) Message = Message.Substring(Node.Length + 2);
        }
    }

    /// <summary>日志查询结果及部分文件不可读、显示数量受限的状态。</summary>
    public sealed class RuntimeLogResult
    {
        public List<RuntimeLogRecord> Records { get; internal set; } = new List<RuntimeLogRecord>();
        public int FileCount { get; internal set; }
        public int FailedDays { get; internal set; }
        public int InvalidRecords { get; internal set; }
        public long MatchedCount { get; internal set; }
    }

    /// <summary>只读查询 DataForm 现有运行和错误日志；不改变写盘目录和配置。</summary>
    public static class RuntimeLogHistory
    {
        public const int MaxRows = 10000;
        public const int MaxDays = 366;
        public const string All = "ALL";
        public const string Run = "RUN";
        public const string Error = "ERROR";
        private static readonly string[] TimestampFormats = { "yyyy-MM-dd HH:mm:ss:fff", "yyyy-MM-dd HH:mm:ss.fff", "yyyy-MM-dd HH:mm:ss", "yyyy/MM/dd HH:mm:ss" };

        /// <summary>查询含首尾的日期范围，保留最新记录；取消时立即终止后续读取。</summary>
        public static RuntimeLogResult Query(string runDirectory, string errorDirectory, DateTime from, DateTime to,
            string level, string keyword, CancellationToken cancellation)
        {
            if (to.Date < from.Date) throw new ArgumentException("Invalid date range.");
            if ((to.Date - from.Date).TotalDays >= MaxDays) throw new ArgumentException("Date range exceeds 366 days.");
            if (level != All && level != Run && level != Error) throw new ArgumentException("Invalid log level.");
            RuntimeLogResult result = new RuntimeLogResult();
            SortedSet<RuntimeLogRecord> newest = new SortedSet<RuntimeLogRecord>(Comparer<RuntimeLogRecord>.Create(CompareRecords));
            keyword = (keyword ?? "").Trim();
            for (DateTime day = to.Date; ; day = day.AddDays(-1))
            {
                cancellation.ThrowIfCancellationRequested();
                if (level != Error) ReadDay(runDirectory, day, Run, keyword, result, newest, cancellation);
                if (level != Run) ReadDay(errorDirectory, day, Error, keyword, result, newest, cancellation);
                if (day == from.Date) break;
            }
            result.Records = newest.Reverse().ToList();
            return result;
        }

        /// <summary>按时间和查询序号排序，保留同一毫秒的重复记录。</summary>
        private static int CompareRecords(RuntimeLogRecord left, RuntimeLogRecord right)
        {
            int time = left.Time.CompareTo(right.Time);
            return time != 0 ? time : left.Sequence.CompareTo(right.Sequence);
        }

        /// <summary>使用现有日志库读取一天，读取失败保留其他日期并明确统计失败。</summary>
        private static void ReadDay(string directory, DateTime day, string level, string keyword,
            RuntimeLogResult result, SortedSet<RuntimeLogRecord> newest, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(directory)) return;
            string folder = Path.Combine(directory, day.ToString("yyyy"), day.ToString("MM"));
            if (!Directory.Exists(folder)) return;
            try
            {
                string[] files = Directory.GetFiles(folder, day.ToString("yyyy-MM-dd") + "*.csv");
                if (files.Length == 0) return;
                List<string[]> values;
                using (DataSave.TypeCollection1 source = new DataSave.TypeCollection1 { SavePath = directory })
                    values = source.ReadDataFromToFile(day, day);
                result.FileCount += files.Length;
                if (values == null || values.Count == 0) { result.FailedDays++; return; }
                int timeIndex = Array.FindIndex(values[0], name => string.Equals(name, "Time", StringComparison.OrdinalIgnoreCase));
                int contentIndex = Array.FindIndex(values[0], name => string.Equals(name, "Content", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(name, "Value", StringComparison.OrdinalIgnoreCase));
                if (timeIndex < 0 || contentIndex < 0) { result.FailedDays++; return; }
                foreach (string[] row in values.Skip(1))
                    AddRecord(row, timeIndex, contentIndex, day, level, keyword, result, newest, cancellation);
            }
            catch (IOException) { result.FailedDays++; }
            catch (UnauthorizedAccessException) { result.FailedDays++; }
            catch (FormatException) { result.FailedDays++; }
        }

        /// <summary>筛选真实时间和内容，并限制内存中保留的结果数量。</summary>
        private static void AddRecord(string[] values, int timeIndex, int contentIndex, DateTime day, string level,
            string keyword, RuntimeLogResult result, SortedSet<RuntimeLogRecord> newest, CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();
            if (values == null || values.Length <= Math.Max(timeIndex, contentIndex)) { result.InvalidRecords++; return; }
            DateTime time;
            if (!DateTime.TryParseExact(values[timeIndex], TimestampFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out time))
            {
                result.InvalidRecords++;
                return;
            }
            if (time.Date != day.Date) return;
            string content = values[contentIndex] ?? "";
            if (keyword.Length > 0 && content.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) < 0) return;
            result.MatchedCount++;
            newest.Add(new RuntimeLogRecord { Time = time, Level = level, Content = content, Sequence = result.MatchedCount });
            if (newest.Count > MaxRows) newest.Remove(newest.Min);
        }

        /// <summary>导出当前查询快照，使用稳定列键和 UTF-8 BOM，保留逗号、引号与换行。</summary>
        public static void Export(IEnumerable<RuntimeLogRecord> records, string path)
        {
            using (StreamWriter writer = new StreamWriter(path, false, new UTF8Encoding(true)))
            {
                writer.WriteLine("Time,Level,Content");
                foreach (RuntimeLogRecord record in records)
                    writer.WriteLine(Csv(record.Time.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)) + ","
                        + Csv(record.Level) + "," + Csv(record.Content));
            }
        }

        /// <summary>对单个 CSV 字段进行标准引号转义。</summary>
        private static string Csv(string value)
        {
            return "\"" + (value ?? "").Replace("\"", "\"\"") + "\"";
        }
    }
}
