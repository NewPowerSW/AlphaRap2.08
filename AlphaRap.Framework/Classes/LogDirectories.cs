using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using AlphaRapLibrary;

namespace AlphaRap
{
    /// <summary>只读兼容已有日志目录，使页面移除后历史数据仍位于原位置。</summary>
    internal static class LogDirectories
    {
        private static readonly object sync = new object();
        private static readonly Dictionary<string, Dictionary<string, string>> paths = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>初始化实际报警写盘目录；新配置继续使用 DataForm 日志自身的默认目录。</summary>
        internal static void RestoreAlarmPath(DataForm data)
        {
            string directory = LegacyPath("AlarmPath");
            if (directory.Length > 0) data.Alarm.SavePath = directory;
        }

        /// <summary>按配置目录读取一次旧路径文件，不创建窗体、不修改旧文件。</summary>
        internal static string LegacyPath(string column)
        {
            string folder = ModuleManager.SettingDataDirectory;
            if (string.IsNullOrWhiteSpace(folder)) folder = SysPara.SettingDataDirectory;
            if (string.IsNullOrWhiteSpace(folder)) folder = @".\ModuleData\SettingData";
            string file = Path.GetFullPath(Path.Combine(folder, "MESForm.xml"));
            lock (sync)
            {
                Dictionary<string, string> stored;
                if (!paths.TryGetValue(file, out stored)) { stored = ReadPaths(file); paths.Add(file, stored); }
                string directory;
                return stored.TryGetValue(column, out directory) ? directory : "";
            }
        }

        /// <summary>只读取仍被使用的报警、生产目录；缺失或不可读时保留当前存储默认值。</summary>
        private static Dictionary<string, string> ReadPaths(string file)
        {
            Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.Ordinal);
            if (!File.Exists(file)) return result;
            try
            {
                XmlDocument document = new XmlDocument { XmlResolver = null };
                document.Load(file);
                foreach (string name in new[] { "AlarmPath", "ProducPath" })
                {
                    XmlNode node = document.SelectSingleNode("/NewDataSet/Path/" + name);
                    if (node != null && !string.IsNullOrWhiteSpace(node.InnerText)) result[name] = node.InnerText.Trim();
                }
            }
            catch (Exception error) { System.Diagnostics.Trace.WriteLine(error.Message); }
            return result;
        }
    }
}
