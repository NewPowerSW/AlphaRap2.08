using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using AlphaRap;

internal static class DashboardProbe
{
    private static string sourceRoot, outputRoot;
    private static int passed, failed;

    private sealed class DesignerContext : LicenseContext
    {
        public override LicenseUsageMode UsageMode { get { return LicenseUsageMode.Designtime; } }
    }

    private sealed class PendingController : NPSDK.IFlowProgramController, NPSDK.IFlowProgramStatus
    {
        internal bool Active;
        public bool IsEnabled { get { return true; } }
        public bool IsRunning { get { return Active; } }

        /// <summary>Represent an isolated pending task without running a machine lifecycle command.</summary>
        public bool Handle(NPSDK.FlowProgramCommand command) { return true; }
    }

    /// <summary>Render the actual dashboard without showing the machine window or invoking its production Load handler.</summary>
    [STAThread]
    private static int Main(string[] args)
    {
        string assemblyRoot = Path.GetFullPath(args[0]);
        AppDomain.CurrentDomain.AssemblyResolve += (sender, request) =>
        {
            string name = new AssemblyName(request.Name).Name;
            foreach (string folder in new[] { assemblyRoot, @"C:\Program Files\Cognex\VisionPro\ReferencedAssemblies" })
                foreach (string extension in new[] { ".dll", ".exe" })
                {
                    string path = Path.Combine(folder, name + extension);
                    if (File.Exists(path)) return Assembly.LoadFrom(path);
                }
            return null;
        };
        sourceRoot = Path.GetFullPath(args[1]); outputRoot = Path.GetFullPath(args[2]);
        Application.EnableVisualStyles();
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
        Environment.CurrentDirectory = outputRoot;
        foreach (Size size in new[] { new Size(1600, 1000), new Size(1440, 900), new Size(1280, 800), new Size(1280, 760) })
            for (int language = 0; language < 3; language++)
            {
                int current = language; Size currentSize = size;
                Test(currentSize.Width + "x" + currentSize.Height + " language=" + current, () => Render(currentSize, current));
            }
        Test("all embedded pages retain readable fonts and text colors", () => TypographyCases.Run(outputRoot));
        Test("actual language switching preserves stopping status and four distinct alarm headers", PendingMachineStatus);
        Console.WriteLine("RESULT: " + passed + "/" + (passed + failed) + " passed");
        Console.WriteLine("Offline designer rendering; no machine Load, motion, IO write or camera capture was executed.");
        return failed == 0 ? 0 : 1;
    }

    /// <summary>Check real layout, sample data updates and event bindings, then write a screenshot for visual inspection.</summary>
    private static void Render(Size size, int language)
    {
        LicenseContext previous = LicenseManager.CurrentContext;
        Func<string, string, string, string, string, string> previousText = UiTheme.TextProvider;
        try
        {
            LicenseManager.CurrentContext = new DesignerContext();
            UiTheme.ForceDesignTimeForTest(true);
            using (MainForm frame = CreateFrame())
            using (HomeForm home = CreateHome())
            {
                Check(typeof(MainForm).GetField("MENU_Log", BindingFlags.Instance | BindingFlags.NonPublic) == null && typeof(MainForm).Assembly.GetType("AlphaRap.LogForm") == null, "The deleted log navigation/page remains in the application.");
                RemoveMachineLoad(frame);
                frame.ClientSize = size;
                home.TopLevel = false; home.FormBorderStyle = FormBorderStyle.None; home.Dock = DockStyle.Fill;
                Field<Panel>(frame, "plMainShow").Controls.Add(home);
                home.Visible = true;
                UiTheme.TextProvider = (group, key, zh, en, es) => language == 0 ? zh : language == 1 ? en : es;
                LicenseManager.CurrentContext = previous;
                UiTheme.ForceDesignTimeForTest(false);
                Invoke(frame, "LoadFrameTexts"); Invoke(frame, "RefreshMenuBackcolor");
                Field<LanguageSwitch>(frame, "languageSwitch").Current = (LanguageType)language;
                Invoke(home, "BindDeviceStatuses", new List<DeviceControl.DeviceStatus>
                {
                    new DeviceControl.DeviceStatus { Name = "Load station", IsConnected = true },
                    new DeviceControl.DeviceStatus { Name = "Vision inspection", IsConnected = true },
                    new DeviceControl.DeviceStatus { Name = "Press station", IsConnected = false },
                    new DeviceControl.DeviceStatus { Name = "ATEQ1", IsConnected = true },
                    new DeviceControl.DeviceStatus { Name = "Unload station", IsConnected = false }
                });
                Invoke(home, "ShowValues", 1236d, 12d, "42", Enumerable.Range(0, 24).Select(hour => (double)(hour < 10 ? hour * 7 : Math.Max(0, 140 - hour * 4))).ToArray(), new double[24], 14);
                Invoke(home, "RefreshPageTexts");
                CheckAlarmTable(frame);
                UiTheme.ForceDesignTimeForTest(true);
                LicenseManager.CurrentContext = new DesignerContext();
                Field<Label>(frame, "lbRecipeName").Text = "AllModel";
                Field<Label>(frame, "LoginText").Text = "admin\n" + (language == 0 ? "管理员" : language == 1 ? "Administrator" : "Administrador");
                Field<ToolStripStatusLabel>(frame, "toolStripStatusLabel5").Text = "16:51";
                frame.Show();
                Application.DoEvents();
                Console.WriteLine("  Layout");
                Layout(frame);
                CheckMachineStatus(frame, size, language);
                CheckFooterIdentity(frame, size, language);
                foreach (string field in new[] { "tableLayoutPanel_Main", "tableLayoutPanel2", "Panl_PictureShowTop", "Panl_PictureShowDown", "panel_PictureShowLeft", "panel_PictureShowRight" })
                    CheckChildren(Field<Control>(frame, field));
                CheckChildren(Field<Control>(home, "kpiLayout"));
                Check(Field<Control>(frame, "panel_PictureShowDown").Top == 0, "Machine commands are not at the top.");
                Check(Field<Control>(frame, "panel4").Height == frame.ClientSize.Height, "Navigation does not span the window height.");
                Color blue = Field<AlphaRap.GradientTableLayoutPanel>(frame, "panel4").GradientTop;
                Check(blue.B > blue.R + 60 && blue.G > blue.R + 50 && Field<Control>(home, "kpiOkCard").BackColor.B > 150,
                    "The main shell and highlighted KPI did not use the blue palette.");
                Control alarmTitle = Field<Control>(frame, "lblAlarmTitle");
                foreach (string chip in new[] { "btnAlarmFilterAll", "btnAlarmFilterWarn", "btnAlarmFilterAlarm", "btnAlarmQuery" })
                {
                    Control control = Field<Control>(frame, chip);
                    int center = control.Parent.PointToScreen(new Point(control.Left, control.Top + control.Height / 2)).Y;
                    int titleCenter = alarmTitle.Parent.PointToScreen(new Point(alarmTitle.Left, alarmTitle.Top + alarmTitle.Height / 2)).Y;
                    Check(control.Height == 28 && Math.Abs(center - titleCenter) <= 1, "The alarm title/filter/query row is vertically misaligned.");
                }
                Control alarms = Field<Control>(frame, "alarmCard");
                Check(frame.WarnningMessage.Parent == alarms && alarms.Visible && frame.WarnningMessage.Visible,
                    "The original alarm list is not visible on the main dashboard.");
                CheckChildren(Field<Control>(frame, "tableLayoutPanel4"));
                Rectangle alarmBounds = frame.RectangleToClient(alarms.RectangleToScreen(alarms.ClientRectangle));
                Check(frame.ClientRectangle.Contains(alarmBounds) && frame.WarnningMessage.Height >= 64,
                    "The persistent alarm table is clipped or too short.");
                TabControl tabs = (TabControl)Field<Control>(frame, "uiTabControl1");
                tabs.SelectedTab = Field<TabPage>(frame, "tabPage10");
                Application.DoEvents();
                Check(alarms.Visible && frame.WarnningMessage.Visible, "Switching to vision hid the main alarm table.");
                tabs.SelectedTab = Field<TabPage>(frame, "tabPage22");
                Application.DoEvents();
                PictureBox product = Field<PictureBox>(frame, "MENU_Product");
                Control productParent = Field<Control>(frame, "Panl_PictureShowTop");
                Check(ReferenceEquals(product.Parent, productParent) && product.Width >= 180 && product.Height >= 36,
                    "Material management is still an icon-only footer shortcut.");
                Check(frame.ClientRectangle.Contains(frame.RectangleToClient(productParent.RectangleToScreen(product.Bounds))), "Material navigation is outside the visible window.");
                string productText = (string)typeof(MainForm).GetMethod("ButtonText", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(frame, new object[] { product });
                Check(productText == (language == 0 ? "物料管理" : language == 1 ? "Materials" : "Materiales"), "Material navigation lacks a translated visible title.");
                bool productEnabled = product.Enabled; product.Enabled = false; Invoke(frame, "RefreshMenuBackcolor");
                Check(ReferenceEquals(product.Parent, productParent), "Permission refresh removed material navigation.");
                product.Enabled = true; Invoke(frame, "RefreshMenuBackcolor");
                typeof(MainForm).GetField("MENU_SelectPage", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(frame, MainForm.MENU_PageType.Product);
                Invoke(frame, "RefreshMenuBackcolor"); Check(product.BackColor != Color.Transparent, "The selected materials page is not highlighted.");
                typeof(MainForm).GetField("MENU_SelectPage", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(frame, MainForm.MENU_PageType.Home);
                product.Enabled = productEnabled; Invoke(frame, "RefreshMenuBackcolor");
                CheckClick(frame, "MENU_Product", "MENU_Click");
                CheckClick(frame, "MENU_Save", "SaveData_Click"); CheckClick(frame, "MENU_Reset", "MENU_Reset_Click");
                CheckClick(frame, "MENU_Run", "btStart_Click"); CheckClick(frame, "MENU_Pause", "btPause_Click");
                CheckClick(frame, "MENU_Stop", "btStop_Click"); CheckClick(frame, "AlarmReset", "btAlarmReset_Click");
                CheckClick(frame, "MachineStatus", "DashboardAlarm_Click");
                Check(Field<Control>(home, "lblKpiOkValue").Text.Replace(",", "").Replace(" ", "") == "1236", "Good-product count was not rendered.");
                Check(Field<Control>(home, "lblKpiInputValue").Text.Replace(",", "").Replace(" ", "") == "1248", "Input count was not rendered.");
                using (Bitmap image = new Bitmap(frame.Width, frame.Height))
                {
                    frame.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
                    Check(image.GetPixel(20, 100).ToArgb() != image.GetPixel(300, 140).ToArgb(), "The screenshot contains only a blank form background.");
                    image.Save(Path.Combine(outputRoot, "dashboard-" + size.Width + "x" + size.Height + "-" + language + ".png"));
                }
                if (size.Width == 1600 && language == 0)
                {
                    using (Bitmap alarmImage = new Bitmap(alarms.Width, alarms.Height))
                    {
                        alarms.DrawToBitmap(alarmImage, new Rectangle(Point.Empty, alarmImage.Size));
                        alarmImage.Save(Path.Combine(outputRoot, "alarms-1600x1000.png"));
                    }
                }
                Invoke(home, "ShowValues", 0d, 0d, "", new double[24], new double[24], 0);
                Check(Field<Control>(home, "lblKpiOkValue").Text == "0", "Empty production data retained the previous count.");
                Check(!Field<Control>(home, "lblKpiYieldValue").Text.Contains("NaN"), "Empty data rendered an invalid percentage.");
            }
        }
        finally
        {
            UiTheme.TextProvider = previousText;
            UiTheme.ForceDesignTimeForTest(false);
            LicenseManager.CurrentContext = previous;
        }
    }

    /// <summary>Render every machine-state appearance and require full translated text inside the compact footer badge.</summary>
    private static void CheckMachineStatus(MainForm frame, Size size, int language)
    {
        string[,] captions = {
            { "待机状态", "IDLE", "modo espera" },
            { "正在初始化状态", "INITIAL", "Estado inicialización" },
            { "初始化完成", "INITIAL Completed", "cargando terminado" },
            { "运行状态", "RUNNING", "CORRER" },
            { "暂停状态", "PAUSE", "Estado suspendido" },
            { "设备报警  ×2", "ALARM  ×2", "ALARMA  ×2" },
            { "停止中", "STOPPING", "DETENIENDO" }
        };
        string[] colors = { "StatusIdleFill", "StatusInitFill", "StatusInitFill", "StatusRunFill", "StatusPauseFill", "StatusAlarmFill", "StatusPauseFill" };
        AppIcon[] icons = { AppIcon.Stop, AppIcon.Reset, AppIcon.Reset, AppIcon.Run, AppIcon.Pause, AppIcon.Alarm, AppIcon.Stop };
        Label badge = Field<Label>(frame, "MachineStatus");
        Check(badge.Height >= 44 && badge.Font.SizeInPoints >= 14, "The machine state is too small to read at a glance.");
        for (int state = 0; state < colors.Length; state++)
        {
            Color fill = (Color)typeof(MainForm).GetField(colors[state], BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            string[] inkFields = { "StatusWhiteInk", "StatusWhiteInk", "StatusWhiteInk", "StatusWhiteInk", "StatusDarkInk", "StatusWhiteInk", "StatusDarkInk" };
            Color ink = (Color)typeof(MainForm).GetField(inkFields[state], BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            Invoke(frame, "SetStatusLook", fill, ink, captions[state, language], icons[state]);
            Layout(Field<Control>(frame, "tableLayoutPanel2"));
            CheckChildren(Field<Control>(frame, "tableLayoutPanel2"));
            Size text = TextRenderer.MeasureText(badge.Text, badge.Font);
            Check(text.Width <= badge.ClientSize.Width - badge.Padding.Horizontal && text.Height <= badge.ClientSize.Height,
                "Translated machine state is clipped: " + badge.Text);
            if (size.Width != 1280) continue;
            using (Bitmap image = new Bitmap(badge.Width, badge.Height))
            {
                badge.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
                image.Save(Path.Combine(outputRoot, "machine-state-" + state + "-" + language + ".png"));
            }
        }
        Color idle = (Color)typeof(MainForm).GetField("StatusIdleFill", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
        Invoke(frame, "SetStatusLook", idle, Color.White, captions[0, language], AppIcon.Stop);
        Layout(Field<Control>(frame, "tableLayoutPanel2"));
    }

    /// <summary>Keep active recipe and compact state aligned without a textbox-style fill or footer overlap.</summary>
    private static void CheckFooterIdentity(MainForm frame, Size size, int language)
    {
        Control footer = Field<Control>(frame, "tableLayoutPanel2"), recipe = Field<Control>(frame, "recipeDetails");
        Label badge = Field<Label>(frame, "MachineStatus"), name = Field<Label>(frame, "lbRecipeName"), caption = Field<Label>(frame, "lblRecipeCaption");
        Check(ReferenceEquals(name.Parent, recipe) && name.BackColor == Color.White && name.AutoEllipsis, "Recipe still looks like a filled input field.");
        Check(caption.Text == (language == 0 ? "当前配方" : language == 1 ? "Active recipe" : "Receta activa"), "Recipe caption lost its translation.");
        Check(badge.Height <= 48 && recipe.Height >= badge.Height && name.Font.SizeInPoints >= 10, "Footer identity block is oversized or unreadable.");
        string previous = name.Text;
        string longName = "Long_Product_Recipe_2026_10_AOI_Dispensing_Assembly_Variant_123456789";
        Type host = typeof(MainForm).Assembly.GetType("AlphaRap.MiddleLayer", true);
        FieldInfo productField = host.GetField("ProductF", BindingFlags.Public | BindingFlags.Static);
        object originalProduct = productField.GetValue(null); string originalRecipe = SysPara.RecipeName;
        try
        {
            productField.SetValue(null, null); SysPara.RecipeName = longName; Invoke(frame, "UpdateRecipeDisplay");
            Check(name.Text == longName && Field<ToolTip>(frame, "_statusTip").GetToolTip(name) == longName,
                "Active recipe was not refreshed or its full-name tooltip was lost.");
        }
        finally { productField.SetValue(null, originalProduct); SysPara.RecipeName = originalRecipe; }
        Layout(footer); CheckChildren(footer); CheckChildren(recipe);
        Check(recipe.Width >= 100 && name.Width <= recipe.Width && name.Text.EndsWith("123456789"), "Long recipe name was modified or displaced the status controls.");
        using (Bitmap image = new Bitmap(footer.Width, footer.Height))
        {
            footer.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
            image.Save(Path.Combine(outputRoot, "footer-long-" + size.Width + "-" + language + ".png"));
        }
        name.Text = previous;
        using (Bitmap image = new Bitmap(footer.Width, footer.Height))
        {
            footer.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
            image.Save(Path.Combine(outputRoot, "footer-normal-" + size.Width + "-" + language + ".png"));
        }
    }

    /// <summary>Exercise the real machine-state update using an isolated controller and copied language packs.</summary>
    private static void PendingMachineStatus()
    {
        LicenseContext previousContext = LicenseManager.CurrentContext;
        Func<string, string, string, string, string, string> previousText = UiTheme.TextProvider;
        string previousDirectory = SysPara.LanguageDataDirectory;
        string previousAlarmDirectory = SysPara.AlarmTableDirectory;
        LanguageType previousLanguage = SysPara.LanguageShow;
        RunMode previousMode = SysPara.SystemMode;
        bool previousAlarm = NPSDK.Alarm.IsError;
        System.Globalization.CultureInfo previousCulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
        Type middleLayer = typeof(SysPara).Assembly.GetType("AlphaRap.MiddleLayer");
        MethodInfo reloadLanguage = middleLayer.GetMethod("ReloadLanguageData", BindingFlags.Static | BindingFlags.NonPublic);
        MethodInfo switchLanguage = middleLayer.GetMethod("SwitchLanguage", BindingFlags.Static | BindingFlags.Public);
        MethodInfo registerLanguage = middleLayer.GetMethod("RegisterLanguage", BindingFlags.Static | BindingFlags.Public);
        string languageDirectory = Path.Combine(outputRoot, "stopping-languages");
        Directory.CreateDirectory(languageDirectory);
        foreach (string language in new[] { "Chinese", "English", "Español" })
            File.Copy(Path.Combine(sourceRoot, "LanguageData", language + ".xml"), Path.Combine(languageDirectory, language + ".xml"), true);
        SeedLegacyAlarmHeaderKeys(languageDirectory);
        MainForm frame = null;
        NPSDK.Flow_BaseForm module = null;
        PendingController controller = new PendingController();
        try
        {
            LicenseManager.CurrentContext = new DesignerContext();
            UiTheme.ForceDesignTimeForTest(true);
            frame = CreateFrame();
            module = new NPSDK.Flow_BaseForm { Name = "OfflineStoppingStatus", ProgramController = controller };
            RemoveMachineLoad(frame);
            frame.Show(); Application.DoEvents();
            LicenseManager.CurrentContext = previousContext;
            UiTheme.ForceDesignTimeForTest(false);
            SysPara.LanguageDataDirectory = languageDirectory;
            SysPara.AlarmTableDirectory = Path.Combine(outputRoot, "empty-alarm-definitions");
            Directory.CreateDirectory(SysPara.AlarmTableDirectory);
            SysPara.SystemMode = RunMode.IDLE;
            NPSDK.Alarm.IsError = false;
            reloadLanguage.Invoke(null, null);
            ListViewItem alarm = PrepareAlarmHeaderCheck(frame);
            registerLanguage.Invoke(null, new object[] { frame.WarnningMessage, "MainForm" });
            string[] expected = { "停止中", "STOPPING", "DETENIENDO" };
            for (int language = 0; language < expected.Length; language++)
            {
                switchLanguage.Invoke(null, new object[] { (LanguageType)language });
                CheckPendingLanguage(frame, controller, language, expected[language]);
                Layout(frame); frame.Refresh(); Application.DoEvents();
                CheckAlarmHeaderLanguage(frame, language, alarm);
            }
            CheckSiteAlarmCaption(frame, languageDirectory, switchLanguage, reloadLanguage);
        }
        finally
        {
            controller.Active = false;
            if (module != null) { NPSDK.Flow_Module.FlowChart_ModuleList.Remove(module); module.Dispose(); }
            if (frame != null) frame.Dispose();
            SysPara.LanguageDataDirectory = previousDirectory;
            SysPara.AlarmTableDirectory = previousAlarmDirectory;
            SysPara.LanguageShow = previousLanguage;
            SysPara.SystemMode = previousMode;
            NPSDK.Alarm.IsError = previousAlarm;
            UiTheme.TextProvider = previousText;
            UiTheme.ForceDesignTimeForTest(false);
            LicenseManager.CurrentContext = previousContext;
            reloadLanguage.Invoke(null, null);
            System.Threading.Thread.CurrentThread.CurrentUICulture = previousCulture;
        }
    }

    /// <summary>Keep a polluted old empty-column key in the isolated packs to prove stable identities avoid it.</summary>
    private static void SeedLegacyAlarmHeaderKeys(string directory)
    {
        foreach (string language in new[] { "Chinese", "English", "Español" })
        {
            string path = Path.Combine(directory, language + ".xml");
            System.Xml.XmlDocument document = new System.Xml.XmlDocument(); document.Load(path);
            System.Xml.XmlElement owner = document.DocumentElement["MainForm"];
            System.Xml.XmlElement key = owner["WarnningMessage_"];
            if (key == null) { key = document.CreateElement("WarnningMessage_"); owner.AppendChild(key); }
            key.SetAttribute("ComponentText", "Time");
            document.Save(path);
        }
    }

    /// <summary>Require unique designer identities and verify theme repair fills missing names without replacing text.</summary>
    private static ListViewItem PrepareAlarmHeaderCheck(MainForm frame)
    {
        ColumnHeader[] columns = frame.WarnningMessage.Columns.Cast<ColumnHeader>().ToArray();
        Check(columns.Length == 4 && columns.All(column => !string.IsNullOrWhiteSpace(column.Name))
            && columns.Select(column => column.Name).Distinct(StringComparer.Ordinal).Count() == 4,
            "Alarm headers do not have four stable language identities.");
        foreach (ColumnHeader column in columns) column.Name = "";
        columns[2].Text = "Draft heading";
        Invoke(frame, "InitializeAlarmColumnNames");
        Check(columns.Select(column => column.Name).SequenceEqual(new[] { "columnHeader1", "columnHeader2", "columnHeader3", "columnHeader4" })
            && columns[2].Text == "Draft heading", "Alarm identity repair changed text or retained duplicate language keys.");
        ListViewItem alarm = new ListViewItem(new[] { "12:34:56", "E", "11000", "Keep alarm payload" });
        frame.WarnningMessage.Items.Add(alarm);
        return alarm;
    }

    /// <summary>Apply the real language registry to four headers while preserving the alarm row and its values.</summary>
    private static void CheckAlarmHeaderLanguage(MainForm frame, int language, ListViewItem alarm)
    {
        string[][] captions = { new[] { "时间", "类型", "编号", "内容" }, new[] { "Time", "Type", "Code", "Content" }, new[] { "Hora", "Tipo", "Código", "Contenido" } };
        Check(frame.WarnningMessage.Columns.Cast<ColumnHeader>().Select(column => column.Text).SequenceEqual(captions[language]),
            "Actual language switching produced duplicated or incorrect alarm headers.");
        Check(frame.WarnningMessage.Items.Count == 1 && ReferenceEquals(frame.WarnningMessage.Items[0], alarm)
            && alarm.SubItems.Cast<ListViewItem.ListViewSubItem>().Select(item => item.Text).SequenceEqual(new[] { "12:34:56", "E", "11000", "Keep alarm payload" }),
            "Language switching changed the existing alarm row or its data.");
        using (Bitmap image = new Bitmap(frame.WarnningMessage.Width, frame.WarnningMessage.Height))
        {
            frame.WarnningMessage.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
            image.Save(Path.Combine(outputRoot, "alarm-headers-" + language + ".png"));
        }
    }

    /// <summary>Respect a site-defined qualified caption instead of overwriting it with a hard-coded runtime header.</summary>
    private static void CheckSiteAlarmCaption(MainForm frame, string directory, MethodInfo switchLanguage, MethodInfo reloadLanguage)
    {
        string path = Path.Combine(directory, "English.xml");
        System.Xml.XmlDocument document = new System.Xml.XmlDocument(); document.Load(path);
        ((System.Xml.XmlElement)document.SelectSingleNode("/English/MainForm/WarnningMessage_columnHeader3")).SetAttribute("ComponentText", "Site code heading");
        document.Save(path);
        switchLanguage.Invoke(null, new object[] { LanguageType.English });
        reloadLanguage.Invoke(null, null);
        Check(frame.WarnningMessage.Columns[2].Text == "Site code heading", "A site-defined alarm caption was overwritten.");
    }

    /// <summary>Require the pending-task translation, then restore the idle translation only after task completion.</summary>
    private static void CheckPendingLanguage(MainForm frame, PendingController controller, int language, string expected)
    {
        SysPara.LanguageShow = (LanguageType)language;
        controller.Active = true;
        Invoke(frame, "UpdateMachineStatus");
        Layout(Field<Control>(frame, "tableLayoutPanel2"));
        Label badge = Field<Label>(frame, "MachineStatus");
        Check(badge.Text == expected, "Pending execution was shown as idle or lost its translation: " + badge.Text);
        CheckChildren(Field<Control>(frame, "tableLayoutPanel2"));
        using (Bitmap image = new Bitmap(badge.Width, badge.Height))
        {
            badge.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
            image.Save(Path.Combine(outputRoot, "stopping-status-" + language + ".png"));
        }
        controller.Active = false;
        Invoke(frame, "UpdateMachineStatus");
        string idle = Field<Dictionary<RunMode, string>>(frame, "_statusTextMap")[RunMode.IDLE];
        Check(badge.Text == idle && badge.Text != expected, "The returned task did not restore the translated idle state.");
    }

    /// <summary>Exercise the original alarm cache and filters in the persistent main dashboard table.</summary>
    private static void CheckAlarmTable(MainForm frame)
    {
        frame.AlarmLogCache.Add(new[] { "16:48:12", "E", "11000", "Offline alarm fixture" });
        frame.AlarmLogCache.Add(new[] { "16:47:55", "W", "11001", "Offline warning fixture" });
        frame.RebuildAlarmLogFromCache();
        Check(frame.WarnningMessage.Columns.Count == 4 && frame.WarnningMessage.Items.Count == 2, "The original alarm columns or rows were lost.");
        Invoke(frame, "SetAlarmLogFilter", "E");
        Check(frame.WarnningMessage.Items.Count == 1 && frame.WarnningMessage.Items[0].SubItems[2].Text == "11000", "Alarm filtering no longer works.");
        Invoke(frame, "SetAlarmLogFilter", "OTHER");
        Check(frame.WarnningMessage.Items.Count == 1 && frame.WarnningMessage.Items[0].SubItems[2].Text == "11001", "Warning filtering no longer works.");
        Invoke(frame, "SetAlarmLogFilter", "ALL");
        Check(frame.WarnningMessage.Items.Count == 2 && frame.AlarmLogCache.Count == 2, "Filtering discarded alarm records.");
    }

    /// <summary>Detach only the main machine Load handler before showing the offline design fixture.</summary>
    private static void RemoveMachineLoad(MainForm frame)
    {
        EventHandlerList events = (EventHandlerList)typeof(Component).GetProperty("Events", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(frame, null);
        FieldInfo key = typeof(Form).GetFields(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(field => string.Equals(field.Name.Replace("_", ""), "EVENTLOAD", StringComparison.OrdinalIgnoreCase));
        Delegate handler = events[key.GetValue(null)];
        Check(handler != null && handler.GetInvocationList().All(item => item.Method.Name == "MainForm_Load"), "Unexpected main window Load handler.");
        events.RemoveHandler(key.GetValue(null), handler);
    }

    /// <summary>Identify a design-construction failure before creating any machine window.</summary>
    private static MainForm CreateFrame()
    {
        Console.WriteLine("  Create MainForm");
        MainForm frame = new MainForm();
        Console.WriteLine("  MainForm ready");
        return frame;
    }

    /// <summary>Identify a dashboard design-construction failure independently of the frame.</summary>
    private static HomeForm CreateHome()
    {
        Console.WriteLine("  Create HomeForm");
        HomeForm home = new HomeForm();
        Console.WriteLine("  HomeForm ready");
        return home;
    }

    /// <summary>Recursively finish docking and table layout before checking geometry.</summary>
    private static void Layout(Control control)
    {
        control.PerformLayout();
        foreach (Control child in control.Controls) Layout(child);
        control.PerformLayout();
    }

    /// <summary>Require sibling layout cells and flow buttons to remain within their container without overlapping.</summary>
    private static void CheckChildren(Control parent)
    {
        Control[] children = parent.Controls.Cast<Control>().Where(child => child.Width > 0 && child.Height > 0).ToArray();
        foreach (Control child in children)
            Check(parent.ClientRectangle.Contains(child.Bounds), "Clipped control: " + parent.Name + "/" + child.Name + " " + child.Bounds + " inside " + parent.ClientRectangle);
        for (int i = 0; i < children.Length; i++)
            for (int j = i + 1; j < children.Length; j++)
                Check(!children[i].Bounds.IntersectsWith(children[j].Bounds), "Overlapping controls: " + children[i].Name + " / " + children[j].Name);
    }

    /// <summary>Verify a production button still has its original action exactly once without invoking it.</summary>
    private static void CheckClick(Control owner, string name, string handlerName)
    {
        Control control = Field<Control>(owner, name);
        EventHandlerList events = (EventHandlerList)typeof(Component).GetProperty("Events", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(control, null);
        FieldInfo key = typeof(Control).GetField("EventClick", BindingFlags.Static | BindingFlags.NonPublic);
        Delegate handler = events[key.GetValue(null)];
        Check(handler != null && handler.GetInvocationList().Count(item => item.Method.Name == handlerName) == 1, "Missing or duplicate command handler: " + name);
    }

    /// <summary>Read a designer field from the actual form.</summary>
    private static T Field<T>(object instance, string name)
    {
        FieldInfo field = instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (field == null) throw new InvalidOperationException("Missing field: " + name);
        return (T)field.GetValue(instance);
    }

    /// <summary>Call a UI-only method while keeping hardware actions outside the probe.</summary>
    private static object Invoke(object instance, string name, params object[] args)
    {
        return instance.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(instance, args);
    }

    /// <summary>Run one independent screen-size and language case.</summary>
    private static void Test(string name, Action action)
    {
        Console.WriteLine("RUN " + name);
        try { action(); passed++; Console.WriteLine("PASS " + name); }
        catch (Exception error) { failed++; Console.WriteLine("FAIL " + name + ": " + error); }
    }

    /// <summary>Report a failed layout or behavior check.</summary>
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
