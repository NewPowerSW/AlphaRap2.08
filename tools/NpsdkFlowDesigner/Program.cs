using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using NPSDK;
using NPSDK.Design;

internal static class Program
{
    /// <summary>运行以真实 NPSDK Flow_Chart 为核心的离线设计与试运行程序。</summary>
    [STAThread]
    private static void Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        if (args.Length == 2 && args[0] == "--export-language")
        {
            ExportLanguages(args[1]);
            return;
        }

        var environment = FlowDesignerDemo.CreateEnvironment();
        var translations = ReadLanguages(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LanguageData"));
        environment.Text = key =>
        {
            string value;
            return translations[environment.Locale].TryGetValue(key, out value) ? value : key;
        };
        environment.ChangeLanguage = language => Thread.CurrentThread.CurrentUICulture = new CultureInfo(language == "Chinese" ? "zh-CN" : language == "English" ? "en-US" : "es-ES");
        string alarmPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DemoAlarm.xml");
        new XElement("AlarmTable", new XElement("A99001", new XAttribute("Content", environment.T("DemoTimeout")), new XAttribute("DoStop", "false"), new XAttribute("Type", "E"))).Save(alarmPath);
        if (!Alarm.Initial(alarmPath))
            throw new InvalidOperationException("Demo alarm table could not be loaded.");
        environment.Registry.AlarmCodes.Add("99001");
        environment.Example.Nodes.First(n => n.Id == "Vision").AlarmCode = "99001";
        using (var form = new FlowDesignerForm(environment))
            Application.Run(form);
    }

    /// <summary>读取三语 XML，保留现场自定义翻译并补齐缺项。</summary>
    private static Dictionary<string, Dictionary<string, string>> ReadLanguages(string directory)
    {
        var result = new Dictionary<string, Dictionary<string, string>>();
        string[] locales =
        {
            "Chinese",
            "English",
            "Español"
        };
        for (int index = 0; index < locales.Length; index++)
        {
            int language = index;
            var texts = FlowDesignerTexts.Defaults.ToDictionary(p => p.Key, p => p.Value[language]);
            string path = Path.Combine(directory, locales[index] + ".xml");
            var section = File.Exists(path) ? XElement.Load(path).Element("FlowDesigner") : null;
            if (section != null)
                foreach (var item in section.Elements())
                    texts[item.Name.LocalName] = (string)item.Attribute("ComponentText");
            result.Add(locales[index], texts);
        }

        return result;
    }

    /// <summary>向指定语言目录补齐三语底稿，已有译文保持原样。</summary>
    private static void ExportLanguages(string directory)
    {
        Directory.CreateDirectory(directory);
        string[] locales =
        {
            "Chinese",
            "English",
            "Español"
        };
        for (int index = 0; index < locales.Length; index++)
        {
            string path = Path.Combine(directory, locales[index] + ".xml");
            var root = File.Exists(path) ? XElement.Load(path) : new XElement(locales[index]);
            var section = root.Element("FlowDesigner");
            if (section == null)
            {
                section = new XElement("FlowDesigner");
                root.Add(section);
            }

            foreach (var item in FlowDesignerTexts.Defaults)
                if (section.Element(item.Key) == null)
                    section.Add(new XElement(item.Key, new XAttribute("ComponentText", item.Value[index])));
            var flow = root.Element("Flow");
            if (flow != null && flow.Element("btnFlowDesigner") == null)
                flow.Add(new XElement("btnFlowDesigner", new XAttribute("ComponentText", new[] { "Flow_Chart 设计器", "Flow_Chart Designer", "Diseñador Flow_Chart" }[index])));
            using (var writer = XmlWriter.Create(path, new XmlWriterSettings { Indent = true, Encoding = new UnicodeEncoding(false, true) }))
                root.Save(writer);
        }
    }
}
