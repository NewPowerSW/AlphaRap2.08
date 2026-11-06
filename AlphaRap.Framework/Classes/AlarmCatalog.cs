using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace AlphaRap
{
    /// <summary>报警三语文本的文件访问，西班牙文统一使用 Spanish.xml。</summary>
    internal static class AlarmCatalog
    {
        private static readonly Dictionary<string, Dictionary<string, string>> AlarmTableCache = new Dictionary<string, Dictionary<string, string>>();
        private static readonly object AlarmTableCacheLock = new object();

        /// <summary>按文件路径和修改时间缓存报警文本，兼容现场修改和配置目录。</summary>
        private static Dictionary<string, string> LoadAlarmTableMap(LanguageType lang)
        {
            string file = FilePath(lang, true);
            string key = Path.GetFullPath(file) + "|" + File.GetLastWriteTimeUtc(file).Ticks;
            lock (AlarmTableCacheLock)
            {
                Dictionary<string, string> map;
                if (AlarmTableCache.TryGetValue(key, out map)) return map;
                map = new Dictionary<string, string>();
                if (!File.Exists(file)) return map;
                foreach (XElement entry in XElement.Load(file).Elements())
                    map[entry.Name.LocalName.TrimStart('A')] = (string)entry.Attribute("Content") ?? "";
                if (AlarmTableCache.Count > 12) AlarmTableCache.Clear();
                AlarmTableCache[key] = map;
                return map;
            }
        }

        /// <summary>按指定语言查报警表内容（找不到返回 "Error"，与旧口径一致）。</summary>
        internal static string GetAlarmText(LanguageType lang, string Index)
        {
            try
            {
                Dictionary<string, string> map = LoadAlarmTableMap(lang);
                string v;
                if (map.TryGetValue((Index ?? "").Trim(), out v)) return v;
            }
            catch (Exception) { }
            return "Error";
        }

        /// <summary>
        /// 报警列表显示用的文案：按编号在三种语言的报警表中做前缀匹配，匹配到则把前缀换成当前语言的内容并保留后面的细节
        /// （如 NPSDK 初始化失败报警中的 Name= / Port=），匹配不到则原样返回。
        /// </summary>
        internal static string ResolveAlarmContent(string code, string stored)
        {
            try
            {
                if (string.IsNullOrEmpty(stored)) return stored;
                string cur = GetAlarmText(SysPara.LanguageShow, code);
                if (cur == "Error" || string.IsNullOrEmpty(cur) || cur == stored) return stored;

                LanguageType[] all = (LanguageType[])Enum.GetValues(typeof(LanguageType));
                foreach (LanguageType lang in all)
                {
                    string t = GetAlarmText(lang, code);
                    if (t != "Error" && !string.IsNullOrEmpty(t) && stored.StartsWith(t, StringComparison.Ordinal))
                        return cur + stored.Substring(t.Length);
                }
            }
            catch (Exception) { }
            return stored;
        }

        /// <summary>切换语言时只替换报警文本，类型和停机规则始终使用中文主表。</summary>
        internal static XElement RuntimeTable(LanguageType language)
        {
            string translatedFile = FilePath(language, true);
            string primaryFile = FilePath(LanguageType.Chinese);
            XElement translated = XElement.Load(translatedFile);
            if (!File.Exists(primaryFile)) return translated;
            XElement result = XElement.Load(primaryFile);
            Dictionary<string, Queue<XElement>> texts = translated.Elements().GroupBy(entry => entry.Name.LocalName)
                .ToDictionary(group => group.Key, group => new Queue<XElement>(group));
            foreach (XElement entry in result.Elements())
            {
                Queue<XElement> entries;
                if (!texts.TryGetValue(entry.Name.LocalName, out entries) || entries.Count == 0) continue;
                string text = (string)entries.Dequeue().Attribute("Content");
                if (!string.IsNullOrEmpty(text)) entry.SetAttributeValue("Content", text);
            }
            foreach (Queue<XElement> entries in texts.Values)
                foreach (XElement entry in entries) result.Add(new XElement(entry));
            return result;
        }

        /// <summary>返回配置目录下的报警表路径；可选择读取时回退至兼容文件名或英文。</summary>
        internal static string FilePath(LanguageType language, bool fallback = false)
        {
            string directory = string.IsNullOrWhiteSpace(SysPara.AlarmTableDirectory) ? ".\\AlarmTable" : SysPara.AlarmTableDirectory;
            string file = Path.Combine(directory, (language == LanguageType.Español ? "Spanish" : language.ToString()) + ".xml");
            if (!fallback || File.Exists(file)) return file;
            string legacy = Path.Combine(directory, language + ".xml");
            return File.Exists(legacy) ? legacy : Path.Combine(directory, "English.xml");
        }

        /// <summary>按报警编号合并三语内容，报警类型和停机设置取中文主表。</summary>
        internal static DataTable Load()
        {
            DataTable table = new DataTable();
            foreach (string name in new[] { "Index", "AlarID", "DoStop", "Type", "Content", "EContent", "SPContent" }) table.Columns.Add(name);
            Dictionary<string, DataRow> rows = new Dictionary<string, DataRow>();
            foreach (LanguageType language in Enum.GetValues(typeof(LanguageType)))
                ReadRows(table, rows, language);
            return table;
        }

        /// <summary>合并一种语言的报警条目，使用 XML 解析器保留转义字符的原意。</summary>
        private static void ReadRows(DataTable table, Dictionary<string, DataRow> rows, LanguageType language)
        {
            string file = FilePath(language);
            if (!File.Exists(file)) return;
            string column = language == LanguageType.Chinese ? "Content" : language == LanguageType.English ? "EContent" : "SPContent";
            Dictionary<string, int> occurrences = new Dictionary<string, int>();
            foreach (XElement entry in XElement.Load(file).Elements())
            {
                string key = entry.Name.LocalName;
                int occurrence;
                occurrences.TryGetValue(key, out occurrence);
                occurrences[key] = occurrence + 1;
                string rowKey = key + "#" + occurrence;
                DataRow row;
                if (!rows.TryGetValue(rowKey, out row))
                {
                    row = table.Rows.Add(table.Rows.Count + 1, key, (string)entry.Attribute("DoStop"), (string)entry.Attribute("Type"), "", "", "");
                    rows.Add(rowKey, row);
                }
                row[column] = (string)entry.Attribute("Content") ?? "";
            }
        }

        /// <summary>先生成三个完整报警表，再写入配置目录；使用 XML 属性转义保证译文合法。</summary>
        internal static void Save(DataTable table)
        {
            Dictionary<string, XElement> documents = new Dictionary<string, XElement>();
            foreach (LanguageType language in Enum.GetValues(typeof(LanguageType)))
            {
                string column = language == LanguageType.Chinese ? "Content" : language == LanguageType.English ? "EContent" : "SPContent";
                XElement root = new XElement("AlarmTable");
                foreach (DataRow row in table.Rows)
                    root.Add(new XElement(Convert.ToString(row["AlarID"]).Trim(),
                        new XAttribute("Type", Convert.ToString(row["Type"]).Trim().ToUpperInvariant()),
                        new XAttribute("DoStop", bool.Parse(Convert.ToString(row["DoStop"]))),
                        new XAttribute("Content", Convert.ToString(row[column]))));
                documents.Add(FilePath(language), root);
            }
            LanguageCatalog.SaveFiles(documents);
            MiddleLayer.ReloadLanguageData();
        }
    }
}
