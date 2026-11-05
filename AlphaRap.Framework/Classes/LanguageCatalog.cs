using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace AlphaRap
{
    /// <summary>管理语言设置表的读取和保存，按窗体名和文案键匹配条目。</summary>
    internal static class LanguageCatalog
    {
        private static readonly string[] Columns = { "Chinese", "English", "Spanish" };

        /// <summary>读取现场语言包，以程序随附的对应语言补齐缺键，保留现场已有译文和属性。</summary>
        internal static XElement ReadDocument(LanguageType language)
        {
            string file = MiddleLayer.LangFilePath(language);
            XElement root = File.Exists(file) ? XElement.Load(file) : new XElement(language.ToString());
            if (root.Name.LocalName != language.ToString()) throw new InvalidDataException("Invalid language XML root: " + file);
            string bundled = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LanguageData", language + ".xml");
            if (!File.Exists(bundled) || string.Equals(Path.GetFullPath(file), Path.GetFullPath(bundled), StringComparison.OrdinalIgnoreCase)) return root;
            foreach (XElement defaultForm in XElement.Load(bundled).Elements())
            {
                XElement form = root.Element(defaultForm.Name);
                if (form == null) { root.Add(new XElement(defaultForm)); continue; }
                foreach (XElement defaultEntry in defaultForm.Elements())
                {
                    XElement entry = form.Element(defaultEntry.Name);
                    if (entry == null) { form.Add(new XElement(defaultEntry)); continue; }
                    if (string.IsNullOrEmpty((string)entry.Attribute("ComponentText")))
                        entry.SetAttributeValue("ComponentText", (string)defaultEntry.Attribute("ComponentText"));
                }
            }
            return root;
        }

        /// <summary>合并三个语言包的全部键，缺少的译文留空供编辑。</summary>
        internal static DataTable Load()
        {
            lock (MiddleLayer.LanguageSync)
            {
                DataTable table = new DataTable();
                foreach (string column in new[] { "ID", "Form", "Object", "English", "Spanish", "Chinese" })
                    table.Columns.Add(column);
                Dictionary<string, DataRow> rows = new Dictionary<string, DataRow>(StringComparer.Ordinal);
                foreach (LanguageType language in Enum.GetValues(typeof(LanguageType)))
                    ReadRows(table, rows, language);
                return table;
            }
        }

        /// <summary>把指定语言的条目按稳定键合并到编辑表，保留 XML 中的原始顺序。</summary>
        private static void ReadRows(DataTable table, Dictionary<string, DataRow> rows, LanguageType language)
        {
            XElement root = ReadDocument(language);
            foreach (XElement form in root.Elements())
            {
                foreach (XElement entry in form.Elements())
                {
                    string key = form.Name.LocalName + "/" + entry.Name.LocalName;
                    DataRow row;
                    if (!rows.TryGetValue(key, out row))
                    {
                        row = table.Rows.Add(table.Rows.Count + 1, form.Name.LocalName, entry.Name.LocalName, "", "", "");
                        rows.Add(key, row);
                    }
                    row[Columns[(int)language]] = (string)entry.Attribute("ComponentText") ?? "";
                }
            }
        }

        /// <summary>先生成全部文件，再替换正式语言包；保存失败时恢复已替换文件，成功后刷新界面缓存。</summary>
        internal static void Save(DataTable table)
        {
            lock (MiddleLayer.LanguageSync)
            {
                if (table == null) throw new ArgumentNullException("table");
                Dictionary<string, XElement> documents = new Dictionary<string, XElement>();
                foreach (LanguageType language in Enum.GetValues(typeof(LanguageType)))
                {
                    string file = MiddleLayer.LangFilePath(language);
                    XElement root = ReadDocument(language);
                    foreach (DataRow row in table.Rows) SetEntry(root, row, Columns[(int)language]);
                    documents.Add(file, root);
                }
                SaveFiles(documents);
                MiddleLayer.ReloadLanguageData();
            }
        }

        /// <summary>暂存全部语言文件后替换，失败时恢复已替换文件。</summary>
        internal static void SaveFiles(Dictionary<string, XElement> documents)
        {
            lock (MiddleLayer.LanguageSync)
            {
                Dictionary<string, byte[]> originals = new Dictionary<string, byte[]>();
                List<string> staged = new List<string>();
                List<string> replaced = new List<string>();
                try
                {
                    foreach (KeyValuePair<string, XElement> document in documents)
                    {
                        string file = document.Key;
                        originals[file] = File.Exists(file) ? File.ReadAllBytes(file) : null;
                        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(file)));
                        string temporary = file + ".tmp";
                        staged.Add(temporary);
                        document.Value.Save(temporary);
                    }
                    foreach (string file in originals.Keys)
                    {
                        if (File.Exists(file)) File.Replace(file + ".tmp", file, null);
                        else File.Move(file + ".tmp", file);
                        replaced.Add(file);
                    }
                }
                catch
                {
                    foreach (string file in replaced)
                    {
                        if (originals[file] == null) File.Delete(file);
                        else File.WriteAllBytes(file, originals[file]);
                    }
                    throw;
                }
                finally
                {
                    foreach (string file in staged)
                        if (File.Exists(file)) File.Delete(file);
                }
            }
        }

        /// <summary>更新一个稳定键的译文，保留其他属性和未出现在编辑表中的条目。</summary>
        private static void SetEntry(XElement root, DataRow row, string column)
        {
            string formName = Convert.ToString(row["Form"]);
            string key = Convert.ToString(row["Object"]);
            System.Xml.XmlConvert.VerifyNCName(formName);
            System.Xml.XmlConvert.VerifyNCName(key);
            XElement form = root.Element(formName);
            if (form == null) { form = new XElement(formName); root.Add(form); }
            XElement entry = form.Element(key);
            if (entry == null) { entry = new XElement(key); form.Add(entry); }
            entry.SetAttributeValue("ComponentText", Convert.ToString(row[column]));
        }
    }
}
