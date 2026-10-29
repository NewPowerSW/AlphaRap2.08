using System;
using System.Collections;
using System.Collections.Generic;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using AlphaRap;
using AlphaRap.Configuration;
using AlphaRap.FlowProgramming;
using Microsoft.CSharp;
using NPSDK;

internal static class SourceGenerationCases
{
    private static readonly MethodInfo prepare = typeof(FlowSourceGenerator).GetMethod("Prepare", BindingFlags.Static | BindingFlags.NonPublic);
    private static readonly MethodInfo write = typeof(ProjectConfiguration).GetMethod("WriteFiles", BindingFlags.Static | BindingFlags.NonPublic);

    /// <summary>Test generated source compilation and transaction ownership only in a disposable fixture.</summary>
    internal static void Run(string root)
    {
        string previousSettings = SysPara.SettingDataDirectory;
        string directory = Path.Combine(root, "source-generation-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            SysPara.SettingDataDirectory = Path.Combine(directory, "settings");
            Assert(FlowSourceGenerator.SourceProjectPath == "", "A test runner automatically selected the real development project.");
            Assert(FlowSourceGenerator.OutputDirectory.StartsWith(SysPara.SettingDataDirectory, StringComparison.OrdinalIgnoreCase), "Default export escaped the isolated settings directory.");
            string source = Path.Combine(directory, "Carrier.cs");
            string custom = Path.Combine(directory, "Carrier.Custom.cs");
            File.WriteAllText(source, "namespace Fixture { public partial class Carrier : NPSDK.Flow_BaseForm { } }");
            File.WriteAllText(custom, "namespace Fixture { public partial class Carrier { public static string Handwritten() { return \"keep me\"; } } }");
            string project = Path.Combine(directory, "Host.csproj");
            File.WriteAllText(project, "<Project xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"><ItemGroup><Compile Include=\"Carrier.cs\"/><Compile Include=\"Carrier.Custom.cs\"/></ItemGroup></Project>");
            FlowSourceGenerator.SourceProjectPath = project;
            FlowProject model = Model();
            IList pending = Prepare(model);
            Assert(!Directory.Exists(FlowSourceGenerator.OutputDirectory), "Preparation wrote formal source files or created their directory.");
            Commit(pending);
            string[] generated = Directory.GetFiles(FlowSourceGenerator.OutputDirectory, "*.Flow.g.cs");
            Assert(generated.Length == 3, "Two instances of the same source Form and a dynamic page did not get separate definitions.");
            byte[][] first = generated.OrderBy(path => path).Select(File.ReadAllBytes).ToArray();
            Commit(Prepare(model));
            Assert(first.Zip(generated.OrderBy(path => path).Select(File.ReadAllBytes), (left, right) => left.SequenceEqual(right)).All(equal => equal), "The same flow snapshot produced nondeterministic C#.");
            Assert(File.ReadAllText(source).Contains("partial class Carrier") && File.ReadAllText(custom).Contains("keep me"), "Generation touched handwritten source.");
            CompileAndCompare(new[] { source, custom }.Concat(generated).ToArray(), model);
            Console.WriteLine("PASS generated typed definitions compile with partial Form and standalone provider, roundtrip all model fields, and preserve handwritten source.");

            string carrier = generated.Single(path => File.ReadAllText(path).Contains("// Module: Fixture.Carrier/FormOne\r\n"));
            string oldNodeId = model.Modules[0].Steps[0].Id;
            model.Modules[0].Steps.RemoveAt(0);
            model.Modules[0].AutoEntry = model.Modules[0].Steps[0].Id;
            Commit(Prepare(model));
            Assert(!File.ReadAllText(carrier).Contains(oldNodeId), "Deleting a node retained its generated definition or link.");
            model.Modules[0].Steps.Clear(); model.Modules[0].AutoEntry = null;
            Commit(Prepare(model));
            Assert(File.Exists(carrier) && !File.ReadAllText(carrier).Contains("CreateNode_"), "Deleting the last node removed the partial file or retained a node method.");
            Console.WriteLine("PASS node and link deletion regenerates owned code; an empty module retains a compilable empty definition.");

            Dictionary<string, byte[]> before = FormalFiles(Prepare(model));
            model.Modules[1].Steps[0].Caption = "changed before failed commit";
            IList failed = Prepare(model);
            Type taskType = failed.GetType().GetGenericArguments()[0];
            object blocker = Activator.CreateInstance(taskType);
            string blocked = Path.Combine(directory, "blocked-target"); Directory.CreateDirectory(blocked);
            taskType.GetField("Path").SetValue(blocker, blocked);
            taskType.GetField("Write").SetValue(blocker, new Action<string>(path => File.WriteAllText(path, "fixture")));
            failed.Add(blocker);
            Reject(() => Commit(failed));
            foreach (KeyValuePair<string, byte[]> item in before) Assert(File.ReadAllBytes(item.Key).SequenceEqual(item.Value), "Late failure did not roll back generated source, manifest and settings together: " + item.Key);
            Console.WriteLine("PASS late file replacement failure rolls back generated source, ownership manifest and generation settings.");

            byte[] owned = File.ReadAllBytes(carrier);
            IList staged = Prepare(model);
            File.AppendAllText(carrier, "// manual change\r\n");
            byte[] edited = File.ReadAllBytes(carrier);
            Reject(() => Commit(staged));
            Assert(File.ReadAllBytes(carrier).SequenceEqual(edited), "Save overwrote a source edited after preparation.");
            Reject(() => Prepare(model));
            Assert(File.ReadAllBytes(carrier).SequenceEqual(edited), "Ownership conflict overwrote handwritten modifications.");
            File.WriteAllBytes(carrier, owned);
            string unowned = Path.Combine(FlowSourceGenerator.OutputDirectory, "FlowSources.manifest.xml");
            File.Delete(unowned);
            Reject(() => Prepare(model));
            Assert(File.ReadAllBytes(carrier).SequenceEqual(owned), "A missing ownership manifest allowed overwriting existing code.");
            Console.WriteLine("PASS manual source edits, preparation races and missing ownership manifests reject replacement without losing user files.");
        }
        finally { SysPara.SettingDataDirectory = previousSettings; }
    }

    /// <summary>Build configured actions plus historical script and module definitions solely to verify export fidelity.</summary>
    private static FlowProject Model()
    {
        FlowStep first = new FlowStep { Caption = "移动到取料点 <A>\r\n\"quoted\"", Target = "hardware/axis-stable-id/axis", Action = "axis.point", ParentPath = "/tabControl1/Auto", AlarmCode = "2019", X = 35, Y = 77 };
        first.ControlOrigin = "Form";
        first.SourceLinks = new FlowControlLinks { Next = "compiledNext", Case1 = "compiledCase" };
        first.Inputs.Add(new FlowInput { Name = "Index", Source = "Parameter", Reference = "Constants/Recipe/LoadPoint" });
        first.Branches = FlowBranch.Default();
        first.Branches[0].Unconditional = false;
        first.Branches[0].Conditions.Add(new FlowCondition { Left = new FlowInput { Source = "Output", Reference = "Value" }, Right = new FlowInput { Source = "Constant", Reference = "true" } });
        FlowStep second = new FlowStep { Caption = "业务脚本", ExecutionMode = "Script", ScriptSource = "// 中文\r\nOutputs.Value = Inputs.Value + 1;\r\n", Breakpoint = true };
        second.ScriptInputs.Add(new FlowScriptPort { Name = "Value", Type = "Number", DefaultValue = "1.5" });
        second.ScriptOutputs.Add(new FlowScriptPort { Name = "Value", Type = "Number" });
        second.ScriptResources.Add("vision/vpp/stable-id");
        second.Inputs.Add(new FlowInput { Name = "Value", Reference = "2.5" });
        second.Branches = FlowBranch.Default();
        AddCodeBinding(first);
        first.Next = second.Id;
        FlowProgram form = new FlowProgram { ModuleId = "Fixture.Carrier/FormOne", Mode = "Program", AutoEntry = first.Id };
        FlowPageDefinition page = new FlowPageDefinition { Key = "fixture.auto", Caption = "自动流程" };
        form.Pages.Add(page); first.PageId = page.Id; first.Width = 276; first.Height = 36;
        form.Steps.Add(first); form.Steps.Add(second);
        FlowProgram dynamic = new FlowProgram { ModuleId = "NPSDK.Flow_BaseForm/Dynamic 页面", Mode = "Program" };
        FlowStep empty = new FlowStep { ExecutionMode = "None", Caption = "Dynamic node" }; empty.Inputs.Clear(); empty.Branches = FlowBranch.Default();
        dynamic.Steps.Add(empty); dynamic.AutoEntry = empty.Id;
        FlowProject project = new FlowProject { Version = 5 }; project.Modules.Add(form); project.Modules.Add(dynamic);
        FlowProgram reused = FlowProgramFile.Snapshot(form); reused.ModuleId = "Fixture.Carrier/FormTwo"; project.Modules.Add(reused);
        return project;
    }

    /// <summary>Retain historical SDK code-resource fields in the source round-trip test without registering or executing a module.</summary>
    private static void AddCodeBinding(FlowStep step)
    {
        FieldInfo module = typeof(FlowStep).GetField("CodeModuleId");
        FieldInfo resources = typeof(FlowStep).GetField("ResourceBindings");
        if (module == null || resources == null) return;
        step.ExecutionMode = "Action";
        module.SetValue(step, "fixture.code");
        FieldInfo version = typeof(FlowStep).GetField("CodeModuleVersion");
        version.SetValue(step, version.FieldType == typeof(string) ? (object)"1" : 1);
        IList bindings = (IList)resources.GetValue(step);
        Type type = resources.FieldType.GetGenericArguments()[0];
        object binding = Activator.CreateInstance(type);
        type.GetField("Slot").SetValue(binding, "Inspection"); type.GetField("Target").SetValue(binding, "vision/vpp/stable-id");
        FieldInfo fields = type.GetField("Fields");
        if (fields != null)
        {
            Type fieldType = fields.FieldType.GetGenericArguments()[0];
            object field = Activator.CreateInstance(fieldType);
            fieldType.GetField("Name").SetValue(field, "Height"); fieldType.GetField("Field").SetValue(field, "TB:Height");
            ((IList)fields.GetValue(binding)).Add(field);
        }
        bindings.Add(binding);
    }

    /// <summary>Compile readable generated C# and reconstruct definitions without constructing Forms or hardware.</summary>
    private static void CompileAndCompare(string[] sources, FlowProject expected)
    {
        CompilerParameters options = new CompilerParameters { GenerateInMemory = true, GenerateExecutable = false };
        options.ReferencedAssemblies.AddRange(new[] { "System.dll", "System.Core.dll", "System.Windows.Forms.dll", "System.Drawing.dll", typeof(FlowProject).Assembly.Location });
        using (CSharpCodeProvider compiler = new CSharpCodeProvider())
        {
            CompilerResults result = compiler.CompileAssemblyFromFile(options, sources);
            Assert(!result.Errors.HasErrors, "Generated C# did not compile: " + string.Join("; ", result.Errors.Cast<CompilerError>().Select(error => error.ToString())));
            List<FlowProgram> actual = result.CompiledAssembly.GetTypes().SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly)).Where(method => method.Name.StartsWith("CreateGeneratedFlowDefinition_", StringComparison.Ordinal)).Select(method => (FlowProgram)method.Invoke(null, null)).ToList();
            Assert(actual.Count == expected.Modules.Count, "A generated module definition is missing.");
            foreach (FlowProgram module in expected.Modules) Assert(Xml(module) == Xml(actual.Single(item => item.ModuleId == module.ModuleId)), "Generated source changed model fields for " + module.ModuleId);
        }
    }

    /// <summary>Capture the actual transaction list returned by the generator.</summary>
    private static IList Prepare(FlowProject project) { return (IList)prepare.Invoke(null, new object[] { project }); }

    /// <summary>Commit through the real common file transaction rather than a test-only writer.</summary>
    private static void Commit(IList files) { write.Invoke(null, new object[] { files }); }

    /// <summary>Capture all formal generated destinations before injecting replacement failures.</summary>
    private static Dictionary<string, byte[]> FormalFiles(IList files)
    {
        return files.Cast<object>().Select(file => (string)file.GetType().GetField("Path").GetValue(file)).ToDictionary(path => path, File.ReadAllBytes);
    }

    /// <summary>Compare all serialized flow fields, including current SDK optional bindings.</summary>
    private static string Xml(FlowProgram program)
    {
        using (StringWriter output = new StringWriter()) { new XmlSerializer(typeof(FlowProgram)).Serialize(output, program); return output.ToString(); }
    }

    /// <summary>Require an operation to fail without assuming which wrapped file error is returned.</summary>
    private static void Reject(Action operation)
    {
        bool rejected = false;
        try { operation(); }
        catch (TargetInvocationException) { rejected = true; }
        catch (IOException) { rejected = true; }
        Assert(rejected, "An unsafe source replacement was accepted.");
    }

    /// <summary>Report deterministic offline source-generation failures.</summary>
    private static void Assert(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}
