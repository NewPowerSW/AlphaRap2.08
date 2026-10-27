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
        foreach (Size size in new[] { new Size(1600, 1000), new Size(1440, 900), new Size(1280, 800) })
            for (int language = 0; language < 3; language++)
            {
                int current = language; Size currentSize = size;
                Test(currentSize.Width + "x" + currentSize.Height + " language=" + current, () => Render(currentSize, current));
            }
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
                foreach (string field in new[] { "tableLayoutPanel_Main", "tableLayoutPanel2", "Panl_PictureShowTop", "Panl_PictureShowDown", "panel_PictureShowLeft", "panel_PictureShowRight" })
                    CheckChildren(Field<Control>(frame, field));
                CheckChildren(Field<Control>(home, "kpiLayout"));
                Check(Field<Control>(frame, "panel_PictureShowDown").Top == 0, "Machine commands are not at the top.");
                Check(Field<Control>(frame, "panel4").Height == frame.ClientSize.Height, "Navigation does not span the window height.");
                Check(Field<Control>(frame, "WarnningMessage").Parent == Field<Control>(frame, "tabAlarms"), "The existing alarm list is not accessible in its tab.");
                CheckClick(frame, "MENU_Save", "SaveData_Click"); CheckClick(frame, "MENU_Reset", "MENU_Reset_Click");
                CheckClick(frame, "MENU_Run", "btStart_Click"); CheckClick(frame, "MENU_Pause", "btPause_Click");
                CheckClick(frame, "MENU_Stop", "btStop_Click"); CheckClick(frame, "AlarmReset", "btAlarmReset_Click");
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
                    TabPage alarms = Field<TabPage>(frame, "tabAlarms");
                    ((TabControl)alarms.Parent).SelectedTab = alarms;
                    Application.DoEvents();
                    using (Bitmap alarmImage = new Bitmap(frame.Width, frame.Height))
                    {
                        frame.DrawToBitmap(alarmImage, new Rectangle(Point.Empty, alarmImage.Size));
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

    /// <summary>Exercise the original alarm cache and filters after the same ListView has moved to its tab.</summary>
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
