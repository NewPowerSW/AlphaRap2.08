using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using AlphaRap;

internal static partial class Tests
{
    /// <summary>在临时目录中使用发布语言包，隔离现场语言和报警表。</summary>
    private static void WithLanguageFiles(Action action)
    {
        string directory = Path.Combine(TestDirectory, "locale-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string previousDirectory = SysPara.LanguageDataDirectory;
        string previousAlarm = SysPara.AlarmTableDirectory;
        LanguageType previousLanguage = SysPara.LanguageShow;
        List<ComponentTextInfo>[] previous = SysPara.ComponentLangurageList;
        SysPara.LanguageDataDirectory = directory;
        SysPara.AlarmTableDirectory = directory;
        SysPara.ComponentLangurageList = new[] { new List<ComponentTextInfo>(), new List<ComponentTextInfo>(), new List<ComponentTextInfo>() };
        foreach (string name in new[] { "Chinese", "English", "Español" })
            File.Copy(Path.Combine(Path.GetDirectoryName(typeof(VPForm).Assembly.Location), "LanguageData", name + ".xml"), Path.Combine(directory, name + ".xml"));
        new XElement("AlarmTable").Save(Path.Combine(directory, "Spanish.xml"));
        LanguageHost.GetField("_langDocs", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, null);
        try { action(); }
        finally
        {
            SysPara.ComponentLangurageList = previous;
            SysPara.LanguageDataDirectory = previousDirectory;
            SysPara.AlarmTableDirectory = previousAlarm;
            SysPara.LanguageShow = previousLanguage;
            LanguageHost.GetField("_langDocs", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, null);
            UiTheme.ForceDesignTimeForTest(false);
        }
    }

    /// <summary>调用框架的真实语言入口，包含动态刷新通知。</summary>
    private static void SetLanguage(LanguageType language)
    {
        LanguageHost.GetMethod("SwitchLanguage").Invoke(null, new object[] { language });
    }

    /// <summary>调用登记入口，测试实际使用的控件引用。</summary>
    private static void RegisterPage(Control page, string form)
    {
        LanguageHost.GetMethod("RegisterAndApplyLanguage").Invoke(null, new object[] { page, form });
    }

    /// <summary>排序和缺键不会把译文写到其他条目；保存立即替换缓存中的旧值。</summary>
    private static void LanguageEditorSave()
    {
        WithLanguageFiles(delegate
        {
            Type catalog = typeof(VPForm).Assembly.GetType("AlphaRap.LanguageCatalog");
            using (Form page = new Form())
            {
                Label label = new UiLabel { Name = "lblTitle", Text = "标题" };
                page.Controls.Add(label);
                LanguageHost.GetMethod("LangMsg").Invoke(null, new object[] { "EditorTest", "lblTitle", "标题", "Title", "Título" });
                RegisterPage(page, "EditorTest");
                DataTable data = (DataTable)catalog.GetMethod("Load", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
                DataRow row = data.AsEnumerable().Single(r => (string)r["Form"] == "EditorTest" && (string)r["Object"] == "lblTitle");
                row["Spanish"] = "Título del sitio & <guardado>";
                data.Rows.Add(data.Rows.Count + 1, "EditorTest", "newKey", "New", "Nuevo", "新建");
                data.DefaultView.Sort = "Form DESC, Object DESC";
                SysPara.LanguageShow = LanguageType.Español;
                catalog.GetMethod("Save", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { data.DefaultView.ToTable() });
                Check(label.Text == "Título del sitio & <guardado>", "Saved translation did not refresh a registered control.");
                XElement spanish = XElement.Load(Path.Combine(SysPara.LanguageDataDirectory, "Español.xml"));
                Check((string)spanish.Element("EditorTest").Element("newKey").Attribute("ComponentText") == "Nuevo", "New/sorted key was not saved.");
            }
        });
    }

    /// <summary>表头和菜单使用稳定键；切换不会替换数据行、输入值或选中项。</summary>
    private static void FixedLanguageParts()
    {
        WithLanguageFiles(delegate
        {
            using (Form page = new Form())
            {
                UiLabel label = new UiLabel { Name = "lblTitle", Text = "标题" };
                TextBox input = new TextBox { Name = "input", Text = "用户名称" };
                DataGridView grid = new DataGridView { Name = "grid", AllowUserToAddRows = false };
                grid.Columns.Add("item", "项目");
                grid.Rows.Add("角度");
                ToolStrip strip = new ToolStrip();
                ToolStripDropDownButton menu = new ToolStripDropDownButton { Name = "menu", Text = "菜单" };
                ToolStripMenuItem child = new ToolStripMenuItem { Name = "save", Text = "保存" };
                menu.DropDownItems.Add(child); strip.Items.Add(menu);
                page.Controls.AddRange(new Control[] { label, input, grid, strip });
                foreach (string key in new[] { "lblTitle", "grid_item", "menu", "save" })
                    LanguageHost.GetMethod("LangMsg").Invoke(null, new object[] { "PartsTest", key, "中文", "English", "Español" });
                DataGridViewRow original = grid.Rows[0];
                RegisterPage(page, "PartsTest");
                SetLanguage(LanguageType.Español);
                Check(label.Text == "Español" && grid.Columns[0].HeaderText == "Español" && child.Text == "Español", "A fixed component did not switch.");
                Check(input.Text == "用户名称" && ReferenceEquals(original, grid.Rows[0]) && Convert.ToString(original.Cells[0].Value) == "角度", "Language switching changed business data.");
            }
        });
    }

    /// <summary>失败的事件订阅者不会阻止后续页面收到语言变化。</summary>
    private static void LanguageEventIsolation()
    {
        WithLanguageFiles(delegate
        {
            EventInfo languageEvent = LanguageHost.GetEvent("LanguageChanged");
            bool notified = false;
            EventHandler fail = delegate { throw new InvalidOperationException("Test subscriber"); };
            EventHandler next = delegate { notified = true; };
            languageEvent.AddEventHandler(null, fail); languageEvent.AddEventHandler(null, next);
            try { SetLanguage(LanguageType.English); Check(notified, "A failed subscriber blocked another page."); }
            finally { languageEvent.RemoveEventHandler(null, fail); languageEvent.RemoveEventHandler(null, next); }
        });
    }

    /// <summary>模拟设计器环境，构建项目页面且不连接硬件或数据库。</summary>
    private sealed class DesignContext : LicenseContext
    {
        public override LicenseUsageMode UsageMode { get { return LicenseUsageMode.Designtime; } }
    }

    /// <summary>逐页核对固定控件的三语登记，保留模块身份和文本输入。</summary>
    private static void AllPageLanguages()
    {
        WithLanguageFiles(delegate
        {
            LicenseContext previous = LicenseManager.CurrentContext;
            LicenseManager.CurrentContext = new DesignContext();
            List<string> untranslated = new List<string>();
            string[] pages = { "AlphaRap.SystemForm", "AlphaRap.SystemSetting", "AlphaRap.HardForm", "AlphaRap.ManualForm", "AlphaRap.CheckForm", "AlphaRap.LogForm", "AlphaRap.DataForm", "AlphaRap.Parameter", "AlphaRap.ProductManagerForm", "AlphaRap.AddUserForm", "AlphaRap.UserLoginForm", "AlphaRap.MenuForm.LockForm1", "AlphaRap.MenuForm.LanguageSetting", "AlphaRap.MenuForm.AlarmSetting", "AlphaRap.MES.LENS", "AlphaRap.MES.Amphenol", "AlphaRap.MES.DongJu", "AlphaRap.Flow", "AlphaRap.SpanLife", "AlphaRap.Warning", "AlphaRap.HomeForm", "AlphaRap.PlatFormSetting", "AlphaRap.VPForm", "AlphaRap.MainForm", "AlphaRap.AlarmQueryForm", "AlphaRap.ProductionQueryForm", "AlphaRap.LoadingForm", "AlphaRap.TaskProcess.Gantry", "AlphaRap.TaskProcess.UpConveryor", "AlphaRap.TaskProcess.DownConveryor" };
            try
            {
                foreach (string name in pages)
                {
                    LicenseManager.CurrentContext = new DesignContext();
                    using (Form page = (Form)Activator.CreateInstance(typeof(VPForm).Assembly.GetType(name)))
                    {
                        LicenseManager.CurrentContext = previous;
                        string identity = page.Text;
                        RegisterPage(page, page.Name);
                        foreach (LanguageType language in Enum.GetValues(typeof(LanguageType)))
                        {
                            SetLanguage(language);
                            List<ComponentTextInfo> entries = SysPara.ComponentLangurageList[(int)language];
                            foreach (ComponentTextInfo entry in entries.Where(e => e.FormName == page.Name && e.Component != null && !e.Component.IsDisposed))
                            {
                                NPSDK.Flow_Chart chart = entry.Component as NPSDK.Flow_Chart;
                                string actual = chart == null ? entry.Component.Text : chart.text;
                                Check(actual == entry.ComponentText, "Page did not apply its registered text: " + name + "/" + entry.ComponentName + " (" + actual + " != " + entry.ComponentText + ")");
                                if (language != LanguageType.Chinese)
                                    if (System.Text.RegularExpressions.Regex.IsMatch(actual, "[\\u4e00-\\u9fff]"))
                                        untranslated.Add(name + "/" + entry.ComponentName + " = " + actual);
                            }
                        }
                        if (page is AlphaRapLibrary.ModuleBaseForm) Check(page.Text == identity, "Module identity was translated: " + name);
                        Console.WriteLine("  PAGE " + name + " (Chinese / English / Español)");
                    }
                }
                Check(untranslated.Count == 0, "Untranslated Chinese captions: " + string.Join("; ", untranslated.Distinct()));
            }
            finally { LicenseManager.CurrentContext = previous; }
        });
    }

    /// <summary>托盘页语言刷新保留行对象、新名称输入和未提交的单元格值。</summary>
    private static void TrayLanguageEdits()
    {
        WithLanguageFiles(delegate
        {
            UiTheme.ForceDesignTimeForTest(true);
            using (PlatFormSetting page = new PlatFormSetting())
            {
                page.RebuildTrayTable();
                DataGridView grid = (DataGridView)typeof(PlatFormSetting).GetField("grdTray", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(page);
                grid.Rows.Add("用户托盘", "pending", "2", "3", "4");
                DataGridViewRow original = grid.Rows[0];
                FieldBox input = (FieldBox)typeof(PlatFormSetting).GetField("boxNewName", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(page);
                input.Value = "待添加的托盘";
                UiTheme.ForceDesignTimeForTest(false);
                SysPara.LanguageShow = LanguageType.Español;
                typeof(PlatFormSetting).GetMethod("PlatFormSetting_LanguageChanged", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(page, new object[] { null, EventArgs.Empty });
                Check(ReferenceEquals(original, grid.Rows[0]) && Convert.ToString(grid[1, 0].Value) == "pending" && input.Value == "待添加的托盘", "Tray language refresh discarded pending edits.");
            }
        });
    }

    /// <summary>报警字段校验依赖稳定字段键，翻译后的列名不影响类型和停机标志检查。</summary>
    private static void AlarmFieldValidation()
    {
        WithLanguageFiles(delegate
        {
            using (AlphaRap.MenuForm.AlarmSetting page = new AlphaRap.MenuForm.AlarmSetting())
            {
                MethodInfo validate = page.GetType().GetMethod("ValidateField", BindingFlags.Instance | BindingFlags.NonPublic);
                foreach (LanguageType language in Enum.GetValues(typeof(LanguageType)))
                {
                    SetLanguage(language);
                    Check(validate.Invoke(page, new object[] { "Type", "INVALID" }) != null, "Translated type header disabled validation.");
                    Check(validate.Invoke(page, new object[] { "DoStop", "INVALID" }) != null, "Translated stop header disabled validation.");
                    Check(validate.Invoke(page, new object[] { "Type", "K" }) == null, "Existing K alarm type rejected.");
                }
            }
        });
    }

    /// <summary>首页报警文本使用配置目录和 Spanish.xml，并在文件更新后重新读取。</summary>
    private static void AlarmLanguageLookup()
    {
        WithLanguageFiles(delegate
        {
            UiTheme.ForceDesignTimeForTest(true);
            using (HomeForm page = new HomeForm())
            {
                string file = Path.Combine(SysPara.AlarmTableDirectory, "Spanish.xml");
                new XElement("AlarmTable", new XElement("A4013", new XAttribute("Content", "Error de lectura & PLC"))).Save(file);
                Check(page.GetAlarmConentOf(LanguageType.Español, "4013") == "Error de lectura & PLC", "Configured Spanish alarm file was not used.");
                new XElement("AlarmTable", new XElement("A4013", new XAttribute("Content", "Texto actualizado"))).Save(file);
                File.SetLastWriteTimeUtc(file, DateTime.UtcNow.AddSeconds(2));
                Check(page.GetAlarmConentOf(LanguageType.Español, "4013") == "Texto actualizado", "Alarm lookup retained stale translations.");
            }
        });
    }

    /// <summary>本地化只替换报警文本，中文主表的停机标志和类型不随语言包改变。</summary>
    private static void AlarmRulesStayFixed()
    {
        WithLanguageFiles(delegate
        {
            new XElement("AlarmTable", new XElement("A4004", new XAttribute("Content", "上升超时"), new XAttribute("DoStop", "True"), new XAttribute("Type", "E")))
                .Save(Path.Combine(SysPara.AlarmTableDirectory, "Chinese.xml"));
            new XElement("AlarmTable", new XElement("A4004", new XAttribute("Content", "Tiempo agotado"), new XAttribute("DoStop", "False"), new XAttribute("Type", "W")))
                .Save(Path.Combine(SysPara.AlarmTableDirectory, "Spanish.xml"));
            Type catalog = typeof(VPForm).Assembly.GetType("AlphaRap.AlarmCatalog");
            XElement table = (XElement)catalog.GetMethod("RuntimeTable", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { LanguageType.Español });
            XElement alarm = table.Element("A4004");
            Check((string)alarm.Attribute("Content") == "Tiempo agotado", "Translated alarm text was not applied.");
            Check((string)alarm.Attribute("DoStop") == "True" && (string)alarm.Attribute("Type") == "E", "Language selection changed an alarm rule.");
        });
    }

    /// <summary>后台视觉和界面同时登记错误提示时，三语 XML 均完整保留所有条目。</summary>
    private static void ConcurrentLanguageMessages()
    {
        WithLanguageFiles(delegate
        {
            MethodInfo message = LanguageHost.GetMethod("LangMsg");
            Parallel.For(0, 20, i => message.Invoke(null, new object[] { "Concurrent", "message" + i, "消息 " + i, "Message " + i, "Mensaje " + i }));
            foreach (string language in new[] { "Chinese", "English", "Español" })
            {
                XElement root = XElement.Load(Path.Combine(SysPara.LanguageDataDirectory, language + ".xml"));
                Check(root.Element("Concurrent").Elements().Count() == 20, "Concurrent language registration lost entries: " + language);
            }
        });
    }

    /// <summary>任一语言文件无法暂存时，三份原语言表均保留原始内容。</summary>
    private static void LanguageSaveFailure()
    {
        WithLanguageFiles(delegate
        {
            Type catalog = typeof(VPForm).Assembly.GetType("AlphaRap.LanguageCatalog");
            DataTable data = (DataTable)catalog.GetMethod("Load", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
            data.Rows[0]["English"] = "Must not be committed";
            Dictionary<string, byte[]> originals = new Dictionary<string, byte[]>();
            foreach (string language in new[] { "Chinese", "English", "Español" })
            {
                string file = Path.Combine(SysPara.LanguageDataDirectory, language + ".xml");
                originals[file] = File.ReadAllBytes(file);
            }
            string blocker = Path.Combine(SysPara.LanguageDataDirectory, "Español.xml.tmp");
            Directory.CreateDirectory(blocker);
            bool failed = false;
            try { catalog.GetMethod("Save", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { data }); }
            catch (TargetInvocationException) { failed = true; }
            Check(failed, "Expected a staging failure.");
            foreach (KeyValuePair<string, byte[]> original in originals)
                Check(original.Value.SequenceEqual(File.ReadAllBytes(original.Key)), "A failed save changed an original language file.");
        });
    }

    /// <summary>权限下拉框刷新显示语言，保存使用的枚举值和当前选择保持不变。</summary>
    private static void PermissionLanguageDisplay()
    {
        WithLanguageFiles(delegate
        {
            LicenseContext previous = LicenseManager.CurrentContext;
            LicenseManager.CurrentContext = new DesignContext();
            try
            {
                using (AddUserForm page = new AddUserForm())
                {
                    LicenseManager.CurrentContext = previous;
                    ComboBox combo = (ComboBox)page.Controls.Find("cbSelectedPermission", true)[0];
                    IntPtr handle = combo.Handle;
                    object selected = combo.SelectedItem;
                    foreach (LanguageType language in Enum.GetValues(typeof(LanguageType)))
                    {
                        SetLanguage(language);
                        string expected = language == LanguageType.Chinese ? "操作员" : language == LanguageType.English ? "Operator" : "Operador";
                        Check(combo.Text == expected, "Permission display did not refresh: " + combo.Text);
                        Check(Equals(selected, combo.SelectedItem) && Convert.ToString(combo.SelectedItem) == "Operator", "Language switching changed the stored permission code.");
                    }
                }
            }
            finally { LicenseManager.CurrentContext = previous; }
        });
    }

    /// <summary>渲染真实语言设置页的三语表头和工具栏，用于检查裁切和空白控件。</summary>
    private static void LanguageSettingsRenders()
    {
        WithLanguageFiles(delegate
        {
            using (AlphaRap.MenuForm.LanguageSetting page = new AlphaRap.MenuForm.LanguageSetting())
            {
                page.LoadTables();
                page.Size = new System.Drawing.Size(1200, 720);
                page.ShowInTaskbar = false;
                page.StartPosition = FormStartPosition.Manual;
                page.Location = new System.Drawing.Point(-30000, -30000);
                page.Show();
                Application.DoEvents();
                foreach (LanguageType language in Enum.GetValues(typeof(LanguageType)))
                {
                    SetLanguage(language);
                    page.PerformLayout();
                    DataGridView grid = (DataGridView)page.Controls.Find("dataGridView1", true)[0];
                    string expected = language == LanguageType.Chinese ? "窗体" : language == LanguageType.English ? "Form" : "Formulario";
                    Check(grid.Columns["form"].HeaderText == expected, "A bound language-editor header did not switch: " + grid.Columns["form"].HeaderText);
                    using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(page.Width, page.Height))
                    {
                        page.DrawToBitmap(bitmap, new System.Drawing.Rectangle(0, 0, page.Width, page.Height));
                        HashSet<int> colors = new HashSet<int>();
                        for (int y = 60; y < bitmap.Height - 20; y += 5)
                            for (int x = 20; x < bitmap.Width - 20; x += 5) colors.Add(bitmap.GetPixel(x, y).ToArgb());
                        Check(colors.Count > 20, "Language settings client area rendered blank.");
                        bitmap.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "language-settings-" + language + ".png"));
                    }
                }
            }
        });
    }

    /// <summary>当前语言不变时保存译文，主框架筛选标签仍从新语言表即时刷新。</summary>
    private static void MainFrameLanguageReload()
    {
        WithLanguageFiles(delegate
        {
            using (MainForm page = new MainForm())
            {
                SetLanguage(LanguageType.Español);
                string file = Path.Combine(SysPara.LanguageDataDirectory, "Español.xml");
                XElement document = XElement.Load(file);
                document.Element("MainForm").Element("alarm_All").SetAttributeValue("ComponentText", "Todos del sitio");
                document.Save(file);
                LanguageHost.GetMethod("ReloadLanguageData", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
                Control chip = page.Controls.Find("btnAlarmFilterAll_dyn", true)[0];
                Check(chip.Text.StartsWith("Todos del sitio ", StringComparison.Ordinal), "Main frame retained a cached translation after saving the same language.");
            }
        });
    }

    /// <summary>现场语言包缺少条目时使用发布译文，已有现场翻译仍优先且可以在编辑页查看完整键集合。</summary>
    private static void PartialSiteCatalog()
    {
        WithLanguageFiles(delegate
        {
            string file = Path.Combine(SysPara.LanguageDataDirectory, "Español.xml");
            new XElement("Español", new XElement("LanguageSetting", new XElement("btnUpdate", new XAttribute("ComponentText", "Editar del sitio")))).Save(file);
            LanguageHost.GetField("_langDocs", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, null);
            SysPara.LanguageShow = LanguageType.Español;
            MethodInfo text = LanguageHost.GetMethod("LangText");
            Check((string)text.Invoke(null, new object[] { "LanguageSetting", "btnSave", "底稿" }) == "Guardar", "Missing site key did not use the bundled Spanish translation.");
            Check((string)text.Invoke(null, new object[] { "LanguageSetting", "btnUpdate", "底稿" }) == "Editar del sitio", "Bundled defaults replaced a site translation.");
            Type catalog = typeof(VPForm).Assembly.GetType("AlphaRap.LanguageCatalog");
            DataTable rows = (DataTable)catalog.GetMethod("Load", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
            DataRow save = rows.AsEnumerable().Single(row => (string)row["Form"] == "LanguageSetting" && (string)row["Object"] == "btnSave");
            Check((string)save["Spanish"] == "Guardar", "Language editor did not include bundled fallback keys.");
            Check(XElement.Load(file).Element("LanguageSetting").Element("btnSave") == null, "Reading defaults wrote over the partial site file.");
        });
    }
}
