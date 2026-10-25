using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using AlphaRap;
using AlphaRapLibrary;
using NPSDK;

internal static class Program
{
    /// <summary>Load the selected published application build and run offline source, metadata and design-construction checks.</summary>
    [STAThread]
    private static int Main(string[] args)
    {
        AppDomain.CurrentDomain.AssemblyResolve += (sender, request) =>
        {
            string name = new AssemblyName(request.Name).Name;
            foreach (string folder in new[] { args[0], @"C:\Program Files\Cognex\VisionPro\ReferencedAssemblies" })
                foreach (string extension in new[] { ".dll", ".exe" })
                {
                    string path = Path.Combine(folder, name + extension);
                    if (File.Exists(path)) return Assembly.LoadFrom(path);
                }
            return null;
        };
        return Cases.Run(args);
    }
}

internal static class Cases
{
    private static string projectRoot, outputRoot, configurationRoot;
    private static Type[] visualTypes;
    private static int passed, failed;

    private sealed class DesignContext : LicenseContext
    {
        public override LicenseUsageMode UsageMode { get { return LicenseUsageMode.Designtime; } }
    }

    /// <summary>Discover every FlowProgramming visual type and SystemSetting, then verify their source and safe construction paths.</summary>
    internal static int Run(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
        projectRoot = Path.GetFullPath(args[1]); outputRoot = Path.GetFullPath(args[2]);
        Directory.CreateDirectory(outputRoot); Environment.CurrentDirectory = outputRoot;
        configurationRoot = Path.Combine(outputRoot, "configuration"); Directory.CreateDirectory(configurationRoot);
        SysPara.SettingDataDirectory = Path.Combine(configurationRoot, "Settings");
        ModuleManager.SettingDataDirectory = SysPara.SettingDataDirectory;
        SysPara.LanguageDataDirectory = Path.Combine(configurationRoot, "LanguageData");
        SysPara.RecipeDataDirectory = Path.Combine(configurationRoot, "Recipes");
        SysPara.VisionFileDirectory = Path.Combine(configurationRoot, "VisionData");
        SysPara.RecipeName = "DesignerProbe"; SysPara.FilePath = Path.Combine(SysPara.RecipeDataDirectory, "DesignerProbe.xml");
        visualTypes = typeof(SystemSetting).Assembly.GetTypes().Where(type => !type.IsAbstract && !type.IsNested && typeof(Control).IsAssignableFrom(type)
            && type.Namespace == "AlphaRap.FlowProgramming").Concat(new[] { typeof(SystemSetting) }).OrderBy(type => type.FullName).ToArray();
        Test("visual classes have complete project metadata and public parameterless constructors", ProjectMetadata);
        Test("InitializeComponent contains only explicit designer-supported UI initialization", DesignerSource);
        foreach (Type type in visualTypes)
        {
            Type current = type;
            Test("design construction and layout: " + current.FullName, () => ConstructDesign(current));
        }
        Test("runtime SystemSetting preserves independent typed Setting and Recipe schemas", SystemSettingSchema);
        Console.WriteLine("RESULT: " + passed + "/" + (passed + failed) + " passed");
        Console.WriteLine("This probe does not replace Visual Studio's C# source designer loading.");
        return failed == 0 ? 0 : 1;
    }

    /// <summary>Require every discovered concrete visual component to remain available to the project designer without business arguments.</summary>
    private static void ProjectMetadata()
    {
        Check(visualTypes.Length >= 12, "A known FlowProgramming visual class or SystemSetting was omitted from discovery.");
        XDocument project = XDocument.Load(Path.Combine(projectRoot, "AlphaRap", "AlphaRap.csproj"));
        XNamespace ns = project.Root.Name.Namespace;
        foreach (Type type in visualTypes)
        {
            string source = SourceName(type);
            XElement item = project.Descendants(ns + "Compile").SingleOrDefault(entry => string.Equals((string)entry.Attribute("Include"), source, StringComparison.OrdinalIgnoreCase));
            Check(item != null && File.Exists(Path.Combine(projectRoot, "AlphaRap", source)), "Missing source or Compile entry: " + source);
            Check(type.GetConstructor(Type.EmptyTypes) != null, "No public parameterless design constructor: " + type.FullName);
            if (type.Name == "FlowLanguageForm") continue;
            string expected = typeof(Form).IsAssignableFrom(type) ? "Form" : "UserControl";
            Check((string)item.Element(ns + "SubType") == expected, "Incorrect SubType for " + source + "; expected " + expected + ".");
            string designer = source.Substring(0, source.Length - 3) + ".Designer.cs";
            XElement child = project.Descendants(ns + "Compile").SingleOrDefault(entry => string.Equals((string)entry.Attribute("Include"), designer, StringComparison.OrdinalIgnoreCase));
            Check(child != null && (string)child.Element(ns + "DependentUpon") == Path.GetFileName(source), "Missing Designer dependency: " + designer);
            string designerPath = Path.Combine(projectRoot, "AlphaRap", designer);
            Check(File.Exists(designerPath), "Missing Designer source: " + designerPath);
            if (!File.ReadAllText(designerPath).Contains("ComponentResourceManager")) continue;
            string resource = designer.Substring(0, designer.Length - ".Designer.cs".Length) + ".resx";
            XElement embedded = project.Descendants(ns + "EmbeddedResource").SingleOrDefault(entry => string.Equals((string)entry.Attribute("Include"), resource, StringComparison.OrdinalIgnoreCase));
            Check(embedded != null && (string)embedded.Element(ns + "DependentUpon") == Path.GetFileName(source), "Missing designer resource dependency: " + resource);
        }
        HashSet<string> controlNames = new HashSet<string>(typeof(SystemSetting).Assembly.GetTypes().Where(type => typeof(Control).IsAssignableFrom(type)).Select(type => type.Name));
        foreach (XElement item in project.Descendants(ns + "Compile"))
        {
            string source = (string)item.Attribute("Include");
            if (source == null || !source.StartsWith("FlowProgramming\\", StringComparison.OrdinalIgnoreCase) || source.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase)) continue;
            string path = Path.Combine(projectRoot, "AlphaRap", source);
            if (!File.Exists(path)) continue;
            Match declaration = Regex.Match(Sanitize(File.ReadAllText(path)), @"\bpartial\s+class\s+(?<name>[A-Za-z_]\w*)");
            if (!declaration.Success || !controlNames.Contains(declaration.Groups["name"].Value) || Path.GetFileNameWithoutExtension(source) == declaration.Groups["name"].Value) continue;
            string subtype = (string)item.Element(ns + "SubType");
            Check(subtype != "Form" && subtype != "UserControl", "A business or supporting partial file is incorrectly marked as a standalone designer: " + source);
        }
    }

    /// <summary>Guard the supported generated-code subset without claiming to emulate Visual Studio's proprietary source parser.</summary>
    private static void DesignerSource()
    {
        foreach (Type type in visualTypes.Where(item => item.Name != "FlowLanguageForm"))
        {
            string file = DesignerPath(type), source = File.ReadAllText(file);
            string body = InitializeBody(Sanitize(source), file);
            Check(!Regex.IsMatch(body, @"\bnew\s*\[\s*\]"), "Implicit array inside InitializeComponent: " + file);
            Check(!Regex.IsMatch(body, @"\bnew\s+(?:global::)?[A-Za-z_][\w.<>]*(?:\s*\([^;{}]*\))?\s*\{"), "Object initializer inside InitializeComponent: " + file);
            foreach (Match local in Regex.Matches(body, @"(?m)^\s*(?<type>(?:global::)?[A-Za-z_][\w.:<>]*)\s+(?<name>[A-Za-z_]\w*)\s*="))
                Check(local.Groups["type"].Value.EndsWith("ComponentResourceManager", StringComparison.Ordinal) && local.Groups["name"].Value == "resources", "Business or inferred local variable inside InitializeComponent: " + file);
            Check(!Regex.IsMatch(body, @"\b(?:if|for|foreach|while|switch|try|using)\s*\("), "Business control flow inside InitializeComponent: " + file);
            Check(!body.Contains("=>"), "Lambda inside InitializeComponent: " + file);
            Check(!Regex.IsMatch(body, @"\b(?:SettingData|RecipeData|DataTable|ConstantStore|MiddleLayer|FlowPrograms|FlowActions|UiTheme)\b"), "Business data or services inside InitializeComponent: " + file);
            foreach (Match call in Regex.Matches(body, @"(?<name>[A-Za-z_][\w.]*)\s*\("))
            {
                string prefix = body.Substring(0, call.Index);
                if (Regex.IsMatch(prefix, @"\bnew\s+(?:global::)?$")) continue;
                string method = call.Groups["name"].Value.Split('.').Last();
                Check(new[] { "SuspendLayout", "ResumeLayout", "PerformLayout", "BeginInit", "EndInit", "Add", "AddRange", "Insert", "SetColumnSpan", "SetRowSpan", "SetChildIndex", "FromArgb", "ApplyResources", "GetObject", "GetString", "SetToolTip", "SetIconAlignment", "SetIconPadding" }.Contains(method),
                    "Custom method inside InitializeComponent: " + call.Groups["name"].Value + " / " + file);
            }
        }
    }

    /// <summary>Blank comments and string literals while preserving newlines and positions for structural source checks.</summary>
    private static string Sanitize(string source)
    {
        const string tokens = @"//[^\r\n]*|/\*[\s\S]*?\*/|@""(?:""""|[^""])*""|""(?:\\.|[^""\\])*""|'(?:\\.|[^'\\])*'";
        return Regex.Replace(source, tokens, match => Regex.Replace(match.Value, @"[^\r\n]", " "));
    }

    /// <summary>Extract the initialized method body with balanced braces after comments and strings have been blanked.</summary>
    private static string InitializeBody(string source, string file)
    {
        Match method = Regex.Match(source, @"\bvoid\s+InitializeComponent\s*\(\s*\)\s*\{");
        Check(method.Success, "InitializeComponent was not found: " + file);
        int start = method.Index + method.Length, depth = 1;
        for (int index = start; index < source.Length; index++)
        {
            if (source[index] == '{') depth++;
            if (source[index] == '}' && --depth == 0) return source.Substring(start, index - start);
        }
        throw new InvalidOperationException("Unbalanced InitializeComponent source: " + file);
    }

    /// <summary>Create and render one visual class under the genuine design LicenseContext without configuring objects or executing actions.</summary>
    private static void ConstructDesign(Type type)
    {
        LicenseContext previous = LicenseManager.CurrentContext;
        string before = ConfigurationTree();
        int hardwareCount = SDKPara.ControlList.Count;
        Control component = null; Form window = null;
        try
        {
            LicenseManager.CurrentContext = new DesignContext();
            component = (Control)Activator.CreateInstance(type);
            Check(component is Form || component is UserControl, "Unexpected visual base: " + type.FullName);
            VerifyInitializedFields(type, component);
            for (Type current = type; current != null; current = current.BaseType)
            {
                FieldInfo subscription = current.GetField("languageSubscribed", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (subscription != null) Check(!(bool)subscription.GetValue(component), "Design construction subscribed a runtime language event: " + type.FullName);
            }
            SystemSetting settings = component as SystemSetting;
            if (settings != null)
            {
                Timer timer = (Timer)typeof(SystemSetting).GetField("permissionTimer", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(settings);
                Check(timer != null && !timer.Enabled, "SystemSetting started its permission timer in design mode.");
            }
            window = component as Form;
            if (window == null)
            {
                window = new Form { ClientSize = new Size(Math.Max(700, component.Width), Math.Max(220, component.Height)) };
                component.Dock = DockStyle.Fill; window.Controls.Add(component);
            }
            window.ShowInTaskbar = false; window.StartPosition = FormStartPosition.Manual; window.Location = new Point(-30000, -30000);
            window.Show(); component.PerformLayout(); Application.DoEvents();
            Check(component.IsHandleCreated, "The visual component did not create a handle: " + type.FullName);
            using (Bitmap image = new Bitmap(Math.Max(1, window.Width), Math.Max(1, window.Height)))
            {
                window.DrawToBitmap(image, new Rectangle(Point.Empty, image.Size));
                image.Save(Path.Combine(outputRoot, type.FullName.Replace('.', '_') + ".png"));
            }
            Check(ConfigurationTree() == before, "Design construction wrote configuration files or directories: " + type.FullName);
            Check(SDKPara.ControlList.Count == hardwareCount, "Design construction registered hardware controls: " + type.FullName);
        }
        finally
        {
            LicenseManager.CurrentContext = previous;
            try
            {
                if (window != null) window.Dispose();
                else if (component != null) component.Dispose();
            }
            finally
            {
                ModuleBaseForm module = component as ModuleBaseForm;
                if (module != null) ModuleManager.ModuleList.Remove(module);
            }
        }
        Check(ConfigurationTree() == before, "Disposing a design-created component after restoring the runtime license wrote configuration: " + type.FullName);
        Check(SDKPara.ControlList.Count == hardwareCount, "Disposing a design-created component registered hardware controls: " + type.FullName);
    }

    /// <summary>Check every designer-created control or component field exists after its parameterless constructor completes.</summary>
    private static void VerifyInitializedFields(Type type, Control component)
    {
        if (type.Name == "FlowLanguageForm") return;
        string body = InitializeBody(Sanitize(File.ReadAllText(DesignerPath(type))), DesignerPath(type));
        foreach (Match assignment in Regex.Matches(body, @"(?m)^\s*(?:this\.)?(?<field>[A-Za-z_]\w*)\s*=\s*new\s+"))
        {
            string name = assignment.Groups["field"].Value;
            FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null && type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null) continue;
            Check(field != null && field.GetValue(component) != null, "Designer field was not initialized: " + type.FullName + "." + name);
        }
    }

    /// <summary>Verify moving schema initialization out of the Designer preserves both independent constant scopes and legacy boolean defaults.</summary>
    private static void SystemSettingSchema()
    {
        using (SystemSetting page = new SystemSetting())
        {
            try
            {
                Check(page.Name == "SystemSetting" && page.Text == "SysSetting", "The module identity changed.");
                Check(page.SettingData.Tables.Count == 3 && page.RecipeData.Tables.Count == 1, "The constant schemas are missing or duplicated.");
                DataTable marker = page.SettingData.Tables["MSet"], flags = page.SettingData.Tables["PSet"];
                Check(marker != null && marker.Columns["ConstantsInitialized"].DataType == typeof(bool) && Equals(marker.Columns["ConstantsInitialized"].DefaultValue, false), "The migration marker lost its boolean default.");
                foreach (string key in new[] { "DisScann", "UseTryRun" })
                    Check(flags != null && flags.Columns[key].DataType == typeof(bool) && Equals(flags.Columns[key].DefaultValue, false), "The legacy switch lost its boolean default: " + key);
                DataTable setting = page.Constants.Table("Setting"), recipe = page.Constants.Table("Recipe");
                foreach (DataTable table in new[] { setting, recipe })
                    Check(table.Columns.Cast<DataColumn>().Select(column => column.ColumnName).SequenceEqual(new[] { "Key", "Type", "Value", "Description" }) && table.Columns.Cast<DataColumn>().All(column => column.DataType == typeof(string)), "A constant scope lost its typed schema.");
                Check(!ReferenceEquals(setting, recipe), "Setting and Recipe share one mutable table.");
                setting.Rows.Add("Independent", "Integer", "1", "Probe");
                Check(recipe.Rows.Count == 0, "A Setting row appeared in Recipe.");
            }
            finally { ModuleManager.ModuleList.Remove(page); }
        }
    }

    /// <summary>Return a stable snapshot of the isolated configuration tree without including screenshots created by the test.</summary>
    private static string ConfigurationTree()
    {
        return string.Join("\n", Directory.GetFileSystemEntries(configurationRoot, "*", SearchOption.AllDirectories).OrderBy(path => path, StringComparer.Ordinal)
            .Select(path => Directory.Exists(path) ? "D|" + path : "F|" + path + "|" + Convert.ToBase64String(File.ReadAllBytes(path))));
    }

    /// <summary>Resolve the project-relative primary source path of a discovered visual class.</summary>
    private static string SourceName(Type type)
    {
        return (type == typeof(SystemSetting) ? "MenuForm\\" : "FlowProgramming\\") + type.Name + ".cs";
    }

    /// <summary>Resolve the fixed Designer source associated with a discovered concrete visual class.</summary>
    private static string DesignerPath(Type type)
    {
        string source = SourceName(type);
        return Path.Combine(projectRoot, "AlphaRap", source.Substring(0, source.Length - 3) + ".Designer.cs");
    }

    /// <summary>Report independent failures with their original exception and continue checking the remaining visual classes.</summary>
    private static void Test(string name, Action action)
    {
        try { action(); passed++; Console.WriteLine("PASS " + name); }
        catch (Exception error) { failed++; Console.WriteLine("FAIL " + name + ": " + error); }
    }

    /// <summary>Fail a violated compatibility invariant with its component and source context.</summary>
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
