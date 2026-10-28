using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using AlphaRap;

internal static class TypographyCases
{
    private static readonly MethodInfo GetControlState = typeof(Control).GetMethod("GetState", BindingFlags.Instance | BindingFlags.NonPublic);

    private sealed class DesignerContext : LicenseContext
    {
        public override LicenseUsageMode UsageMode { get { return LicenseUsageMode.Designtime; } }
    }

    /// <summary>Check embedded pages under the actual frame's ambient text color without loading machine configuration.</summary>
    internal static void Run(string output)
    {
        LicenseContext previous = LicenseManager.CurrentContext;
        List<string> failures = new List<string>();
        int pages = 0, captions = 0;
        try
        {
            LicenseManager.CurrentContext = new DesignerContext();
            UiTheme.ForceDesignTimeForTest(true);
            using (MainForm frame = new MainForm())
            {
                DetachLoad(frame);
                frame.ClientSize = new Size(1600, 1000);
                Panel host = (Panel)typeof(MainForm).GetField("panel2", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(frame);
                foreach (Control existing in host.Controls) existing.Visible = false;
                foreach (Type type in PageTypes())
                {
                    using (Control page = (Control)Activator.CreateInstance(type))
                    {
                        Form form = page as Form;
                        if (form != null) { form.TopLevel = false; form.FormBorderStyle = FormBorderStyle.None; DetachLoad(form); }
                        page.Dock = DockStyle.Fill; host.Controls.Add(page);
                        Audit(page, type.Name, failures, ref captions);
                        pages++;
                        if (type == typeof(HardForm))
                        {
                            frame.Show(); page.Show(); Application.DoEvents();
                            Control jogOptions = (Control)type.GetField("nP_JogOption1", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(page);
                            Control jogRight = (Control)type.GetField("nP_MotorJog2", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(page);
                            if (jogOptions.Bounds.IntersectsWith(jogRight.Bounds)) failures.Add("HardForm: jog options cover the right arrow.");
                            using (Bitmap image = new Bitmap(frame.Width, frame.Height))
                            {
                                frame.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
                                image.Save(Path.Combine(output, "hardware-typography.png"));
                            }
                            frame.Hide();
                        }
                        host.Controls.Remove(page);
                    }
                }
                Audit(frame, "MainForm", failures, ref captions);
            }
        }
        finally { UiTheme.ForceDesignTimeForTest(false); LicenseManager.CurrentContext = previous; }
        Console.WriteLine("TYPOGRAPHY: " + pages + " pages and " + captions + " text controls checked.");
        if (failures.Count > 0) throw new InvalidOperationException(string.Join(Environment.NewLine, failures.Distinct()));
    }

    /// <summary>Include application pages and parameterless flow/configuration editors already supported in design mode.</summary>
    private static IEnumerable<Type> PageTypes()
    {
        string[] names = { "SystemForm", "SystemSetting", "HardForm", "ManualForm", "CheckForm", "LogForm", "DataForm", "Parameter", "ProductManagerForm", "AddUserForm", "UserLoginForm", "MenuForm.LockForm1", "MenuForm.LanguageSetting", "MenuForm.AlarmSetting", "MES.LENS", "MES.Amphenol", "MES.DongJu", "Flow", "SpanLife", "Warning", "HomeForm", "PlatFormSetting", "VPForm", "AlarmQueryForm", "ProductionQueryForm", "LoadingForm", "TaskProcess.Gantry", "TaskProcess.UpConveryor", "TaskProcess.DownConveryor" };
        Assembly assembly = typeof(HomeForm).Assembly;
        return names.Select(name => assembly.GetType("AlphaRap." + name, true)).Concat(assembly.GetTypes().Where(type =>
            !type.IsAbstract && !type.IsNested && typeof(Control).IsAssignableFrom(type) && type.GetConstructor(Type.EmptyTypes) != null
            && (type.Namespace == "AlphaRap.FlowProgramming" || type.Namespace == "AlphaRap.Configuration"))).Distinct();
    }

    /// <summary>Detect near-white text on light surfaces while preserving intentional light text on dark headers.</summary>
    private static void Audit(Control control, string page, List<string> failures, ref int captions)
    {
        if (!(control is TabPage) && !(control is Form) && GetControlState != null && !(bool)GetControlState.Invoke(control, new object[] { 2 })) return;
        if ((control is Label || control is ButtonBase || control is TextBoxBase || control is GroupBox)
            && !string.IsNullOrWhiteSpace(control.Text))
        {
            captions++;
            Color background = Background(control);
            if (Luminance(control.ForeColor) > 0.7 && Luminance(background) > 0.7)
                failures.Add(page + "/" + control.Name + ": light text " + control.ForeColor + " on " + background);
            if ((control is ButtonBase || control is TextBoxBase) && control.Font.SizeInPoints < 8f)
                failures.Add(page + "/" + control.Name + ": small interactive font " + control.Font.SizeInPoints);
        }
        foreach (Control child in control.Controls) Audit(child, page, failures, ref captions);
    }

    /// <summary>Resolve transparent surfaces and the existing painted dark branding panel.</summary>
    private static Color Background(Control control)
    {
        for (Control parent = control; parent != null; parent = parent.Parent)
        {
            if (parent is BrandPanel) return UiKit.BrandDark;
            if (parent.BackColor.A == 255) return parent.BackColor;
        }
        return SystemColors.Control;
    }

    /// <summary>Convert sRGB to relative luminance for a conservative unreadable-text check.</summary>
    private static double Luminance(Color color)
    {
        return 0.2126 * Channel(color.R) + 0.7152 * Channel(color.G) + 0.0722 * Channel(color.B);
    }

    /// <summary>Convert one sRGB channel to linear intensity.</summary>
    private static double Channel(byte value)
    {
        double component = value / 255.0;
        return component <= 0.04045 ? component / 12.92 : Math.Pow((component + 0.055) / 1.055, 2.4);
    }

    /// <summary>Keep machine and page Load handlers outside the offline appearance fixture.</summary>
    private static void DetachLoad(Form form)
    {
        EventHandlerList events = (EventHandlerList)typeof(Component).GetProperty("Events", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form, null);
        FieldInfo key = typeof(Form).GetFields(BindingFlags.Static | BindingFlags.NonPublic)
            .Single(field => string.Equals(field.Name.Replace("_", ""), "EVENTLOAD", StringComparison.OrdinalIgnoreCase));
        Delegate handler = events[key.GetValue(null)];
        if (handler != null) events.RemoveHandler(key.GetValue(null), handler);
    }
}
