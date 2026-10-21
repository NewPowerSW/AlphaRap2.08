using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace AlphaRap
{
    partial class MiddleLayer
    {
        private static readonly LanguageType[] Languages = (LanguageType[])Enum.GetValues(typeof(LanguageType));
        private static XmlDocument[] _langDocs;
        private static Dictionary<string, string>[] _languageTexts;

        /// <summary>获取界面语言包路径；西班牙语文件名为 Español.xml。</summary>
        internal static string LangFilePath(LanguageType lang)
        {
            string directory = SysPara.LanguageDataDirectory;
            return Path.Combine(string.IsNullOrEmpty(directory) ? ".\\LanguageData" : directory, lang + ".xml");
        }

        /// <summary>加载并缓存对应语言的 XML 及查找索引，所有调用均持有 LanguageSync。</summary>
        private static XmlDocument LangDoc(LanguageType language)
        {
            int index = (int)language;
            if (_langDocs == null)
            {
                _langDocs = new XmlDocument[Languages.Length];
                _languageTexts = new Dictionary<string, string>[Languages.Length];
            }
            if (_langDocs[index] != null) return _langDocs[index];
            XmlDocument document = new XmlDocument();
            try
            {
                using (XmlReader reader = LanguageCatalog.ReadDocument(language).CreateReader()) document.Load(reader);
            }
            catch (Exception error) { System.Diagnostics.Debug.WriteLine(error); }
            if (document.DocumentElement == null || document.DocumentElement.Name != language.ToString())
            {
                document = new XmlDocument();
                document.AppendChild(document.CreateElement(language.ToString()));
            }
            Dictionary<string, string> entries = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (XmlNode form in document.DocumentElement.ChildNodes)
            {
                foreach (XmlNode node in form.ChildNodes)
                {
                    XmlElement entry = node as XmlElement;
                    if (entry == null) continue;
                    string key = form.Name + "/" + entry.Name;
                    if (!entries.ContainsKey(key)) entries.Add(key, entry.GetAttribute("ComponentText"));
                }
            }
            _languageTexts[index] = entries;
            _langDocs[index] = document;
            return document;
        }

        /// <summary>按窗体和稳定键读取当前语言，没有译文时返回底稿。</summary>
        public static string LangText(string formName, string key, string baseText)
        {
            return LangTextOf(SysPara.LanguageShow, formName, key) ?? baseText;
        }

        /// <summary>从内存索引读取指定语言的文案，无键或空译文返回 null。</summary>
        private static string LangTextOf(LanguageType language, string formName, string key)
        {
            if (string.IsNullOrEmpty(formName) || string.IsNullOrEmpty(key)) return null;
            if ((int)language < 0 || (int)language >= Languages.Length) return null;
            lock (LanguageSync)
            {
                LangDoc(language);
                string value;
                return _languageTexts[(int)language].TryGetValue(formName + "/" + key, out value) && !string.IsNullOrEmpty(value) ? value : null;
            }
        }

        /// <summary>登记三语消息底稿并返回当前译文；保留现场已有译文，省略西语时回落英文。</summary>
        public static string LangMsg(string formName, string key, string zh, string en, string es = null)
        {
            lock (LanguageSync)
            {
                string spanish = string.IsNullOrEmpty(es) ? en : es;
                string fallback = SysPara.LanguageShow == LanguageType.Chinese ? zh : SysPara.LanguageShow == LanguageType.English ? en : spanish;
                try
                {
                    SeedLangText(LanguageType.Chinese, formName, key, zh);
                    SeedLangText(LanguageType.English, formName, key, en);
                    SeedLangText(LanguageType.Español, formName, key, spanish);
                    string value = LangTextOf(SysPara.LanguageShow, formName, key);
                    if (value != null) return value;
                    if (SysPara.LanguageShow == LanguageType.Español) return LangTextOf(LanguageType.English, formName, key) ?? fallback;
                }
                catch (Exception error) { System.Diagnostics.Debug.WriteLine(error); }
                return fallback;
            }
        }

        /// <summary>只在语言包缺少有效译文时补写底稿。</summary>
        private static void SeedLangText(LanguageType language, string formName, string key, string text)
        {
            if (string.IsNullOrEmpty(text) || LangTextOf(language, formName, key) != null) return;
            SetLangText(language, formName, key, text);
        }

        /// <summary>更新一条语言记录和内存索引，保存时保留其他节点与属性。</summary>
        private static void SetLangText(LanguageType language, string formName, string key, string text)
        {
            lock (LanguageSync)
            {
                try
                {
                    XmlConvert.VerifyNCName(formName);
                    XmlConvert.VerifyNCName(key);
                    XmlDocument document = LangDoc(language);
                    XmlElement root = document.DocumentElement;
                    XmlElement form = root[formName];
                    if (form == null) { form = document.CreateElement(formName); root.AppendChild(form); }
                    XmlElement entry = form[key];
                    if (entry == null) { entry = document.CreateElement(key); form.AppendChild(entry); }
                    entry.SetAttribute("ComponentText", text ?? "");
                    _languageTexts[(int)language][formName + "/" + key] = text ?? "";
                    SaveLangDoc(document, LangFilePath(language));
                }
                catch (Exception error) { System.Diagnostics.Debug.WriteLine(error); }
            }
        }

        /// <summary>以 UTF-16 保存语言 XML，保留唯一声明及既有属性。</summary>
        private static void SaveLangDoc(XmlDocument document, string file)
        {
            try
            {
                for (int i = document.ChildNodes.Count - 1; i >= 0; i--)
                    if (document.ChildNodes[i] is XmlDeclaration) document.RemoveChild(document.ChildNodes[i]);
                document.InsertBefore(document.CreateXmlDeclaration("1.0", "unicode", "yes"), document.DocumentElement);
                string directory = Path.GetDirectoryName(file);
                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
                document.Save(file);
            }
            catch (Exception error) { System.Diagnostics.Debug.WriteLine(error); }
        }
    }
}
