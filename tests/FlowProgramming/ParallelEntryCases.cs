using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using AlphaRap;
using AlphaRap.FlowProgramming;
using NPSDK;

internal sealed class ParallelEntryPage : Flow_BaseForm
{
    internal readonly Flow_Chart A = new Flow_Chart { Name = "entryA", text = "初始化 A", Location = new Point(40, 40) };
    internal readonly Flow_Chart B = new Flow_Chart { Name = "entryB", text = "初始化 B", Location = new Point(370, 40) };
    internal readonly Flow_Chart C = new Flow_Chart { Name = "entryC", text = "自动 C", Location = new Point(40, 40) };
    internal readonly Flow_Chart D = new Flow_Chart { Name = "entryD", text = "自动 D", Location = new Point(370, 40) };
    internal volatile bool ReadyA, ReadyB, ReadyC, ReadyD;
    internal int CallsA, CallsB, CallsC, CallsD;

    /// <summary>Build two independent native entries in each stage without device access.</summary>
    internal ParallelEntryPage()
    {
        Name = "Parallel_" + Guid.NewGuid().ToString("N"); Size = new Size(780, 350);
        A.FlowChartRun += RunA; B.FlowChartRun += RunB; C.FlowChartRun += RunC; D.FlowChartRun += RunD;
        InitialPage.Controls.AddRange(new Control[] { A, B }); AutoPage.Controls.AddRange(new Control[] { C, D });
    }
    /// <summary>Poll only the first initialization entry.</summary>
    [FlowTool("初始化 A", "Initialization A", "Inicialización A", UsesHardware = false)]
    private Flow_Chart.ResultType RunA() { Interlocked.Increment(ref CallsA); return ReadyA ? Flow_Chart.ResultType.End : Flow_Chart.ResultType.IDLE; }
    /// <summary>Poll only the second initialization entry.</summary>
    [FlowTool("初始化 B", "Initialization B", "Inicialización B", UsesHardware = false)]
    private Flow_Chart.ResultType RunB() { Interlocked.Increment(ref CallsB); return ReadyB ? Flow_Chart.ResultType.End : Flow_Chart.ResultType.IDLE; }
    /// <summary>Poll only the first automatic entry.</summary>
    [FlowTool("自动 C", "Automatic C", "Automático C", UsesHardware = false)]
    private Flow_Chart.ResultType RunC() { Interlocked.Increment(ref CallsC); return ReadyC ? Flow_Chart.ResultType.End : Flow_Chart.ResultType.IDLE; }
    /// <summary>Poll only the second automatic entry.</summary>
    [FlowTool("自动 D", "Automatic D", "Automático D", UsesHardware = false)]
    private Flow_Chart.ResultType RunD() { Interlocked.Increment(ref CallsD); return ReadyD ? Flow_Chart.ResultType.End : Flow_Chart.ResultType.IDLE; }
}

internal static partial class Cases
{
    /// <summary>Register multi-entry configuration, timing, stage completion and protected-source behavior.</summary>
    private static void RegisterParallelEntryCases()
    {
        Test("one module starts all selected initialization and automatic entries", ParallelMachineEntries);
        Test("entry multi-selection preserves checks across languages and respects Cancel", ParallelEntryDialog);
        Test("generated source viewing is read-only and cannot alter or traverse owned files", ProtectedSourceViewer);
    }

    /// <summary>Create a module whose two initialization and two automatic roots are explicitly selected.</summary>
    private static FlowModuleEditor ParallelOwner(ParallelEntryPage page)
    {
        FlowModuleEditor owner = FlowPrograms.Attach(page);
        owner.SetEntries(true, owner.Program.Steps.Where(step => step.ControlName == "entryA" || step.ControlName == "entryB").Select(step => step.Id));
        owner.SetEntries(false, owner.Program.Steps.Where(step => step.ControlName == "entryC" || step.ControlName == "entryD").Select(step => step.Id));
        owner.Program.Mode = "Program"; owner.Publish(); return owner;
    }

    /// <summary>Exercise real native callbacks, all-entry completion, pause timing, Stop clearing and fresh restarts.</summary>
    private static void ParallelMachineEntries()
    {
        using (ParallelEntryPage page = new ParallelEntryPage())
        {
            FlowModuleEditor owner = ParallelOwner(page); page.Show(); Application.DoEvents();
            try
            {
                owner.Handle(FlowProgramCommand.Initialize);
                ActionUntil(() => page.CallsA > 0 && page.CallsB > 0, "Both initialization flows did not start.");
                page.ReadyA = true; NativePump(80);
                Assert(owner.Runtime.IsRunning && !page.InitialStatus, "One completed entry marked the module initialized while a peer was still running.");
                owner.Handle(FlowProgramCommand.Pause); NativePump(60);
                Assert(page.B.FlowChart_ElapsedMilliseconds == 0, "Pause failed to clear the active peer's clock.");
                owner.Handle(FlowProgramCommand.Stop); Wait(owner.Runtime.Completion); NativePump(30);
                Assert(new[] { page.A, page.B, page.C, page.D }.All(node => node.Controls.Find("elapsedTime", true).Single().Text == ""), "Stop left a parallel node's elapsed display.");
                page.ReadyB = true; int previousA = page.CallsA, previousB = page.CallsB;
                owner.Handle(FlowProgramCommand.Initialize); Wait(owner.Runtime.Completion);
                Assert(page.InitialStatus && page.CallsA > previousA && page.CallsB > previousB, "Stopped initialization did not restart both selected roots.");
                owner.Handle(FlowProgramCommand.Start);
                ActionUntil(() => page.CallsC > 0 && page.CallsD > 0, "Both automatic flows did not start.");
                page.ReadyC = page.ReadyD = true; Wait(owner.Runtime.Completion);
                Assert(owner.Runtime.Error == null, "Parallel automatic execution failed: " + owner.Runtime.Error);
            }
            finally { owner.Handle(FlowProgramCommand.Stop); Wait(owner.Runtime.Completion); }
        }
    }

    /// <summary>The modal selector filters by stage and changes only the accepted in-memory selection.</summary>
    private static void ParallelEntryDialog()
    {
        using (ParallelEntryPage page = new ParallelEntryPage())
        {
            FlowModuleEditor owner = ParallelOwner(page); string[] selected = owner.Program.InitialEntries.ToArray();
            using (FlowEntryForm dialog = new FlowEntryForm(owner, true))
            {
                dialog.Show(); CheckedListBox list = ActionControl<CheckedListBox>(dialog, "entryChoices_dyn");
                Assert(list.Items.Count == 2 && list.CheckedItems.Count == 2, "The selector included the other stage or lost checked entries.");
                list.SetItemChecked(1, false);
                foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
                {
                    SwitchLanguage(language); Application.DoEvents(); Assert(list.CheckedItems.Count == 1, "Language switching changed pending checks.");
                    Screenshot(dialog, "parallel-entry-selection-" + language);
                }
                ActionControl<Button>(dialog, "entryCancel").PerformClick();
                Assert(owner.Program.InitialEntries.SequenceEqual(selected), "Cancel changed the startup group.");
            }
            using (FlowEntryForm dialog = new FlowEntryForm(owner, true))
            {
                dialog.Show(); ActionControl<CheckedListBox>(dialog, "entryChoices_dyn").SetItemChecked(1, false);
                ActionControl<Button>(dialog, "entryOk").PerformClick();
                Assert(dialog.DialogResult == DialogResult.OK && owner.Program.InitialEntries.Count == 1 && owner.Program.AutoEntries.Count == 2, "Accept changed another stage or failed to retain its selection.");
            }
            bool rejected = false;
            try { owner.SetEntries(true, owner.Program.AutoEntries); } catch (InvalidOperationException) { rejected = true; }
            Assert(rejected, "An automatic-page node became an initialization entry.");
            SwitchLanguage(LanguageType.Chinese);
        }
    }

    /// <summary>Source display reads only owned files and exposes no editing or Explorer actions.</summary>
    private static void ProtectedSourceViewer()
    {
        string directory = Path.Combine(root, "protected-source"); Directory.CreateDirectory(directory);
        string name = "Probe.Flow.g.cs", text = "// generated source\nclass Probe {}";
        string file = Path.Combine(directory, name); File.WriteAllText(file, text, Encoding.UTF8);
        FlowSourceGenerator.Manifest manifest = new FlowSourceGenerator.Manifest();
        manifest.Files.Add(new FlowSourceGenerator.OwnedFile { ModuleId = "probe", FileName = name, Sha256 = "fixture" });
        string path = Path.Combine(directory, "FlowSources.manifest.xml");
        using (FileStream stream = File.Create(path)) new System.Xml.Serialization.XmlSerializer(typeof(FlowSourceGenerator.Manifest)).Serialize(stream, manifest);
        string project = FlowSourceGenerator.SourceProjectPath, output = FlowSourceGenerator.OutputDirectory;
        try
        {
            FlowSourceGenerator.SourceProjectPath = ""; FlowSourceGenerator.OutputDirectory = directory;
            byte[] before = File.ReadAllBytes(file); IDictionary<string,string> documents = FlowSourceGenerator.ReadGeneratedSources();
            using (FlowSourceViewer viewer = new FlowSourceViewer(documents, directory))
            {
                viewer.Show(); RichTextBox source = ActionControl<RichTextBox>(viewer, "sourceCode_dyn");
                foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
                {
                    SwitchLanguage(language); Application.DoEvents();
                    Assert(source.ReadOnly && source.Text == text && File.ReadAllBytes(file).SequenceEqual(before), "Source viewing or translation changed file contents.");
                    Screenshot(viewer, "protected-source-" + language);
                }
                Assert(viewer.Controls.Find("save", true).Length == 0 && viewer.Controls.Find("delete", true).Length == 0, "Source viewing exposes file mutation commands.");
            }
            manifest.Files[0].FileName = "../Probe.Flow.g.cs";
            using (FileStream stream = File.Create(path)) new System.Xml.Serialization.XmlSerializer(typeof(FlowSourceGenerator.Manifest)).Serialize(stream, manifest);
            bool rejected = false;
            try { FlowSourceGenerator.ReadGeneratedSources(); } catch (InvalidDataException) { rejected = true; }
            Assert(rejected && File.ReadAllBytes(file).SequenceEqual(before), "The source viewer followed an unsafe manifest path.");
        }
        finally { FlowSourceGenerator.SourceProjectPath = project; FlowSourceGenerator.OutputDirectory = output; SwitchLanguage(LanguageType.Chinese); }
    }
}
