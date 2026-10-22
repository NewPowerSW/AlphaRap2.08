using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace AlphaRap.FlowWorkbench
{
    public sealed class FlowLanguage
    {
        private Dictionary<string, string> texts = new Dictionary<string, string>();
        public string Language { get; private set; }

        /// <summary>读取与框架相同的语言、窗体、文案键和 ComponentText 结构。</summary>
        public void Switch(string language)
        {
            if (!new[] { "Chinese", "English", "Español" }.Contains(language)) throw new ArgumentException("language");
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LanguageData", language + ".xml");
            using (var reader = XmlReader.Create(path, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null }))
            {
                var section = XElement.Load(reader).Element("FlowWorkbench");
                if (section == null) throw new InvalidDataException("FlowWorkbench language section missing");
                texts = section.Elements().ToDictionary(e => e.Name.LocalName, e => (string)e.Attribute("ComponentText") ?? "");
            }
            Language = language;
            CultureInfo.CurrentUICulture = language == "Chinese" ? CultureInfo.GetCultureInfo("zh-CN") : language == "English" ? CultureInfo.GetCultureInfo("en-US") : CultureInfo.GetCultureInfo("es-ES");
        }

        /// <summary>读取固定键并用占位符插入业务值，不改变数值输入文化。</summary>
        public string Text(string key, params object[] arguments)
        {
            string value;
            if (!texts.TryGetValue(key, out value)) value = key;
            return arguments.Length == 0 ? value : string.Format(CultureInfo.CurrentCulture, value, arguments);
        }

        /// <summary>按稳定控件名与表格列名更新标题，保留所有编辑行和业务参数。</summary>
        public void RegisterAndApplyLanguage(Control root)
        {
            if (root is Form || root is Button || root is Label || root is TabPage)
                if (texts.ContainsKey(root.Name)) root.Text = Text(root.Name);
            var grid = root as DataGridView;
            if (grid != null)
                foreach (DataGridViewColumn column in grid.Columns)
                    column.HeaderText = Text(grid.Name + "_" + column.Name);
            if (root is TextBoxBase || root is ComboBox || grid != null) return;
            foreach (Control child in root.Controls) RegisterAndApplyLanguage(child);
        }
    }
}
