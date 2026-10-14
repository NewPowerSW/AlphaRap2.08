using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace AlphaRap
{
    /// <summary>一条报警历史记录，对应日志 CSV 的一行。</summary>
    public sealed class AlarmRecord
    {
        public DateTime Time;
        public string User = "";
        public string RecipeId = "";
        public string RecipeName = "";
        public string Mode = "";
        /// <summary>原始类型：日志文件里写作 Information / ERROR。</summary>
        public string Type = "";
        public string Code = "";
        public string Content = "";

        /// <summary>是否 E 类报警（日志文件里的 ERROR）。</summary>
        public bool IsAlarm
        {
            get { return string.Equals(Type, "ERROR", StringComparison.OrdinalIgnoreCase)
                      || string.Equals(Type, "E", StringComparison.OrdinalIgnoreCase); }
        }
    }

    /// <summary>
    /// 报警历史的读取与统计。
    /// 数据源 = AlarmRunTask 写出的日志：{AlarmPath}\{yyyy}\{MM}\{yyyy-MM-dd}.csv，
    /// 列 = Time,User,RecipeID,RecipeName,Mode,Type,Code,Content（时间列由库自动前置）。
    /// 只读、不写盘；读不到的文件直接跳过，不让一次查询整体失败。
    /// </summary>
    public static class AlarmHistory
    {
        public const string FilterAll = "ALL";
        public const string FilterWarn = "WARN";
        public const string FilterAlarm = "ALARM";

        /// <summary>报警日志根目录（取自日志页配置的 AlarmPath）；取不到时返回空串。</summary>
        public static string RootPath()
        {
            try
            {
                if (MiddleLayer.LogF == null) return "";
                string p = MiddleLayer.LogF.GetSettingValue("Path", "AlarmPath");
                return string.IsNullOrEmpty(p) ? "" : p.Trim();
            }
            catch { return ""; }
        }

        /// <summary>报警日志根目录是否存在（不存在时界面提示去日志页配置）。</summary>
        public static bool RootReady()
        {
            string root = RootPath();
            return root.Length > 0 && Directory.Exists(root);
        }

        /// <summary>
        /// 读取 [from, to]（含首尾）范围内的报警记录。
        /// 一天一个文件，只打开存在的那些；from 晚于 to 时自动对调。
        /// </summary>
        public static List<AlarmRecord> Query(DateTime from, DateTime to, string typeFilter,
            string code, string keyword, out int fileCount)
        {
            List<AlarmRecord> list = new List<AlarmRecord>();
            fileCount = 0;

            string root = RootPath();
            if (root.Length == 0 || !Directory.Exists(root)) return list;
            if (to < from) { DateTime t = from; from = to; to = t; }

            code = (code ?? "").Trim();
            keyword = (keyword ?? "").Trim();

            // 上限一年：防止误选日期把界面卡死
            if ((to.Date - from.Date).TotalDays > 366) to = from.Date.AddDays(366);

            for (DateTime d = from.Date; d <= to.Date; d = d.AddDays(1))
            {
                string file = Path.Combine(Path.Combine(Path.Combine(root, d.ToString("yyyy")),
                    d.ToString("MM")), d.ToString("yyyy-MM-dd") + ".csv");
                if (!File.Exists(file)) continue;

                string[] lines;
                try { lines = File.ReadAllLines(file); }
                catch (Exception) { continue; }
                fileCount++;

                for (int i = 0; i < lines.Length; i++)
                {
                    AlarmRecord r = ParseLine(lines[i]);
                    if (r == null) continue;
                    if (!Match(r, typeFilter, code, keyword)) continue;
                    list.Add(r);
                }
            }
            return list;
        }

        /// <summary>
        /// 解析一行；表头或列数不足的行返回 null。
        /// Content 里可能带逗号，所以按前 7 个逗号切分，其余整段算 Content。
        /// </summary>
        private static AlarmRecord ParseLine(string line)
        {
            if (string.IsNullOrEmpty(line)) return null;

            int[] cut = new int[7];
            int idx = -1;
            for (int k = 0; k < 7; k++)
            {
                idx = line.IndexOf(',', idx + 1);
                if (idx < 0) return null;
                cut[k] = idx;
            }

            AlarmRecord r = new AlarmRecord();
            r.Time = ParseTime(line.Substring(0, cut[0]));
            if (r.Time == DateTime.MinValue) return null;          // 表头走这里

            r.User = line.Substring(cut[0] + 1, cut[1] - cut[0] - 1);
            r.RecipeId = line.Substring(cut[1] + 1, cut[2] - cut[1] - 1);
            r.RecipeName = line.Substring(cut[2] + 1, cut[3] - cut[2] - 1);
            r.Mode = line.Substring(cut[3] + 1, cut[4] - cut[3] - 1);
            r.Type = line.Substring(cut[4] + 1, cut[5] - cut[4] - 1);
            r.Code = line.Substring(cut[5] + 1, cut[6] - cut[5] - 1);
            r.Content = line.Substring(cut[6] + 1).TrimEnd(',').Trim();
            return r;
        }

        /// <summary>解析库写出的时间（形如 2026-10-14 08:58:41:839）；失败返回 DateTime.MinValue。</summary>
        private static DateTime ParseTime(string s)
        {
            DateTime t;
            string[] fmts = { "yyyy-MM-dd HH:mm:ss:fff", "yyyy-MM-dd HH:mm:ss.fff",
                              "yyyy-MM-dd HH:mm:ss", "yyyy/MM/dd HH:mm:ss" };
            if (DateTime.TryParseExact(s.Trim(), fmts, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out t)) return t;
            if (DateTime.TryParse(s.Trim(), out t)) return t;
            return DateTime.MinValue;
        }

        /// <summary>类型 / 编号 / 关键字筛选；编号与关键字都是包含匹配、忽略大小写。</summary>
        private static bool Match(AlarmRecord r, string typeFilter, string code, string keyword)
        {
            if (typeFilter == FilterAlarm && !r.IsAlarm) return false;
            if (typeFilter == FilterWarn && r.IsAlarm) return false;
            if (code.Length > 0 && r.Code.IndexOf(code, StringComparison.OrdinalIgnoreCase) < 0) return false;
            if (keyword.Length > 0
                && r.Content.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) < 0
                && r.Code.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) < 0) return false;
            return true;
        }

        /// <summary>E 类报警条数。</summary>
        public static int CountAlarm(List<AlarmRecord> rows)
        {
            int n = 0;
            for (int i = 0; i < rows.Count; i++) if (rows[i].IsAlarm) n++;
            return n;
        }

        /// <summary>出现过的报警号种类数。</summary>
        public static int CountCodeKinds(List<AlarmRecord> rows)
        {
            HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < rows.Count; i++) set.Add(rows[i].Code);
            return set.Count;
        }

        /// <summary>按报警号统计条数，降序取前 top 个。</summary>
        public static List<KeyValuePair<string, int>> GroupByCode(List<AlarmRecord> rows, int top)
        {
            Dictionary<string, int> map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < rows.Count; i++)
            {
                string k = string.IsNullOrEmpty(rows[i].Code) ? "-" : rows[i].Code;
                int n;
                map[k] = map.TryGetValue(k, out n) ? n + 1 : 1;
            }

            List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>(map);
            list.Sort(delegate (KeyValuePair<string, int> a, KeyValuePair<string, int> b)
            {
                int c = b.Value.CompareTo(a.Value);
                return c != 0 ? c : string.Compare(a.Key, b.Key, StringComparison.OrdinalIgnoreCase);
            });
            if (top > 0 && list.Count > top) list.RemoveRange(top, list.Count - top);
            return list;
        }

        /// <summary>按天统计条数；区间内没有记录的那天补 0，便于趋势图对齐。</summary>
        public static List<KeyValuePair<string, int>> GroupByDay(List<AlarmRecord> rows,
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

        /// <summary>把记录写成 CSV（导出用）；成功返回空串，失败返回原因。</summary>
        public static string Export(List<AlarmRecord> rows, string file)
        {
            try
            {
                using (StreamWriter w = new StreamWriter(file, false, new System.Text.UTF8Encoding(true)))
                {
                    w.WriteLine("Time,Type,Code,Content,RecipeName,Mode,User");
                    for (int i = 0; i < rows.Count; i++)
                    {
                        AlarmRecord r = rows[i];
                        w.WriteLine(string.Join(",", new string[] {
                            r.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                            r.IsAlarm ? "E" : "W",
                            Csv(r.Code), Csv(r.Content), Csv(r.RecipeName), Csv(r.Mode), Csv(r.User) }));
                    }
                }
                return "";
            }
            catch (Exception ex) { return ex.Message; }
        }

        /// <summary>CSV 字段转义：含逗号或引号时用双引号包起来。</summary>
        private static string Csv(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            if (s.IndexOf(',') < 0 && s.IndexOf('"') < 0 && s.IndexOf('\n') < 0) return s;
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        }
    }
}
