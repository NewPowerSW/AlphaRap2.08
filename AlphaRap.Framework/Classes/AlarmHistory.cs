using System.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

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

        /// <summary>是否停机报警：报警表 AlarmTable\*.xml 里该编号的 DoStop。</summary>
        public bool Stop;

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
        /// <summary>只看会导致停机的报警（报警表里 DoStop=true）。</summary>
        public const string FilterStop = "STOP";

        /// <summary>报警历史与实际报警写盘使用同一个 DataForm 日志目录。</summary>
        public static string RootPath()
        {
            if (MiddleLayer.DataF == null || MiddleLayer.DataF.Alarm == null) return "";
            return (MiddleLayer.DataF.Alarm.SavePath ?? "").Trim();
        }

        /// <summary>实际报警日志目录是否存在。</summary>
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

            using (DataSave.TypeCollection1 source = new DataSave.TypeCollection1 { SavePath = root })
                for (DateTime day = from.Date; day <= to.Date; day = day.AddDays(1))
                {
                    List<string[]> rows;
                    try { rows = source.ReadDataFromToFile(day, day); }
                    catch (Exception) { continue; }
                    if (rows.Count < 2) continue;
                    string folder = Path.Combine(root, day.ToString("yyyy"), day.ToString("MM"));
                    fileCount += Directory.GetFiles(folder, day.ToString("yyyy-MM-dd") + "*.csv").Length;
                    foreach (string[] values in rows.Skip(1))
                    {
                        AlarmRecord record = ParseRecord(rows[0], values);
                        if (record == null) continue;
                        record.Stop = IsStopCode(record.Code);
                        if (Match(record, typeFilter, code, keyword)) list.Add(record);
                    }
                }
            return list;
        }

        /// <summary>按历史文件实际列名读取报警，兼容字段内逗号、引号及换行。</summary>
        private static AlarmRecord ParseRecord(string[] columns, string[] values)
        {
            Func<string, string> value = name => {
                int index = Array.FindIndex(columns, column => string.Equals(column, name, StringComparison.OrdinalIgnoreCase));
                return index >= 0 && index < values.Length ? values[index] : "";
            };
            DateTime time = ParseTime(value("Time"));
            if (time == DateTime.MinValue) return null;
            return new AlarmRecord { Time = time, User = value("User"), RecipeId = value("RecipeID"), RecipeName = value("RecipeName"),
                Mode = value("Mode"), Type = value("Type"), Code = value("Code"), Content = value("Content") };
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
            if (typeFilter == FilterStop && !r.Stop) return false;
            if (code.Length > 0 && r.Code.IndexOf(code, StringComparison.OrdinalIgnoreCase) < 0) return false;
            if (keyword.Length > 0
                && r.Content.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) < 0
                && r.Code.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) < 0) return false;
            return true;
        }

        /// <summary>停机报警条数。</summary>
        public static int CountStop(List<AlarmRecord> rows)
        {
            int n = 0;
            for (int i = 0; i < rows.Count; i++) if (rows[i].Stop) n++;
            return n;
        }

        /// <summary>出现过的天数（按日期去重）。</summary>
        public static int CountDays(List<AlarmRecord> rows)
        {
            HashSet<string> set = new HashSet<string>();
            for (int i = 0; i < rows.Count; i++) set.Add(rows[i].Time.ToString("yyyy-MM-dd"));
            return set.Count;
        }

        /// <summary>出现过的配方种类数（配方名为空的不计）。</summary>
        public static int CountRecipeKinds(List<AlarmRecord> rows)
        {
            HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < rows.Count; i++)
                if (!string.IsNullOrEmpty(rows[i].RecipeName)) set.Add(rows[i].RecipeName);
            return set.Count;
        }

        /// <summary>出现过的工号数（工号为空的不计）。</summary>
        public static int CountUserKinds(List<AlarmRecord> rows)
        {
            HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < rows.Count; i++)
                if (!string.IsNullOrEmpty(rows[i].User)) set.Add(rows[i].User);
            return set.Count;
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

        // ---------------- 停机判定（读报警表的 DoStop 属性） ----------------

        /// <summary>报警表目录，与运行时加载报警表用的是同一个。</summary>
        private static string AlarmTableDir()
        {
            string dir = SysPara.AlarmTableDirectory;
            return string.IsNullOrEmpty(dir) ? ".\\AlarmTable" : dir;
        }

        /// <summary>按当前语言取报警表文件，映射规则与 MiddleLayer.LoadAlarmTable 一致。</summary>
        private static string LangTableFile(string dir)
        {
            string langName;
            switch (SysPara.LanguageShow)
            {
                case LanguageType.Chinese: langName = "Chinese"; break;
                case LanguageType.English: langName = "English"; break;
                case LanguageType.Español: langName = "Spanish"; break;   // 枚举名 Español，文件名是 Spanish
                default: langName = SysPara.LanguageShow.ToString(); break;
            }

            string file = Path.Combine(dir, langName + ".xml");
            if (File.Exists(file)) return file;
            file = Path.Combine(dir, SysPara.LanguageShow.ToString() + ".xml");
            if (File.Exists(file)) return file;
            return Path.Combine(dir, "English.xml");
        }

        private static Dictionary<string, bool> _stopMap;
        private static string _stopMapDir = "\0";

        /// <summary>
        /// 编号 → 是否停机。当前语言的报警表优先，该表里没有的编号用其它语言表补齐；
        /// 报警表目录变了才重建。
        /// </summary>
        private static Dictionary<string, bool> StopMap()
        {
            string dir = AlarmTableDir();
            if (_stopMap != null && string.Equals(_stopMapDir, dir, StringComparison.OrdinalIgnoreCase))
                return _stopMap;

            Dictionary<string, bool> map = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            ReadStopFile(map, LangTableFile(dir));

            string[] others = { "Chinese.xml", "English.xml", "Spanish.xml", "Español.xml" };
            for (int i = 0; i < others.Length; i++)
                ReadStopFile(map, Path.Combine(dir, others[i]));

            _stopMap = map;
            _stopMapDir = dir;
            return map;
        }

        /// <summary>把一份报警表里「编号 → DoStop」并进 map；已存在的编号不覆盖。</summary>
        private static void ReadStopFile(Dictionary<string, bool> map, string file)
        {
            try
            {
                if (!File.Exists(file)) return;

                XmlDocument doc = new XmlDocument();
                doc.Load(file);
                XmlNode root = doc.DocumentElement;
                if (root == null) return;

                foreach (XmlNode n in root.ChildNodes)
                {
                    if (n.NodeType != XmlNodeType.Element) continue;

                    string code = n.Name;                                  // 形如 A1000
                    if (code.Length > 1 && (code[0] == 'A' || code[0] == 'a')) code = code.Substring(1);
                    if (code.Length == 0 || map.ContainsKey(code)) continue;

                    bool stop = false;
                    if (n.Attributes != null)
                    {
                        for (int i = 0; i < n.Attributes.Count; i++)
                        {
                            XmlAttribute a = n.Attributes[i];
                            if (!string.Equals(a.Name, "DoStop", StringComparison.OrdinalIgnoreCase)) continue;
                            stop = string.Equals(a.Value.Trim(), "true", StringComparison.OrdinalIgnoreCase);
                            break;
                        }
                    }
                    map[code] = stop;
                }
            }
            catch { }   // 单份报警表读不了不影响其它表，也不让整次查询失败
        }

        /// <summary>该报警号是否会停机；报警表里没有这个编号时按"不停机"处理。</summary>
        public static bool IsStopCode(string code)
        {
            if (string.IsNullOrEmpty(code)) return false;
            bool v;
            return StopMap().TryGetValue(code.Trim(), out v) && v;
        }

        /// <summary>把记录写成 CSV（导出用）；成功返回空串，失败返回原因。</summary>
        public static string Export(List<AlarmRecord> rows, string file)
        {
            try
            {
                using (StreamWriter w = new StreamWriter(file, false, new System.Text.UTF8Encoding(true)))
                {
                    w.WriteLine("Time,Type,Stop,Code,Content,RecipeName,Mode,User");
                    for (int i = 0; i < rows.Count; i++)
                    {
                        AlarmRecord r = rows[i];
                        w.WriteLine(string.Join(",", new string[] {
                            r.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                            r.IsAlarm ? "E" : "W",
                            r.Stop ? "Y" : "N",
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
