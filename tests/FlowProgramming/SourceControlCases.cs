using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AlphaRap;
using AlphaRap.FlowProgramming;
using NPSDK;

internal sealed class SourceSyncPage : Flow_BaseForm
{
    internal readonly Flow_Chart First = new Flow_Chart { Name = "sourceFirst", text = "First", Location = new Point(35, 25) };
    internal readonly Flow_Chart Last = new Flow_Chart { Name = "sourceLast", text = "Last", Location = new Point(35, 145) };
    internal int Calls;

    /// <summary>Represent compiled form revisions, including an old unbound duplicate and an explicitly bypassed middle node.</summary>
    internal SourceSyncPage(string name, bool middle, bool bound = false, bool bypass = false)
    {
        Name = name;
        First.FlowChartRun += RunFirst; Last.FlowChartRun += RunLast;
        InitialPage.Controls.AddRange(new Control[] { First, Last });
        Flow_Chart between = null;
        if (middle)
        {
            between = new Flow_Chart { Name = "sourceMiddle", text = "Duplicate", Location = new Point(35, 85), Next = Last };
            if (bound) between.FlowChartRun += RunMiddle;
            InitialPage.Controls.Add(between);
        }
        First.Next = middle && !bypass ? between : Last;
        First.Casse1 = First.Next;
    }

    /// <summary>Count actual invocations without accessing hardware.</summary>
    private Flow_Chart.ResultType RunFirst() { Calls++; return Flow_Chart.ResultType.NEXT; }
    /// <summary>Provide a removable original native callback.</summary>
    private Flow_Chart.ResultType RunMiddle() { Calls++; return Flow_Chart.ResultType.NEXT; }
    /// <summary>End the isolated native route.</summary>
    private Flow_Chart.ResultType RunLast() { Calls++; return Flow_Chart.ResultType.End; }
    /// <summary>Release an offline page from the SDK module catalogue.</summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing) Flow_Module.FlowChart_ModuleList.Remove(this);
        base.Dispose(disposing);
    }
}

internal static partial class Cases
{
    /// <summary>Cover source deletion, baseline merging, dynamic-node preservation and references that require explicit repair.</summary>
    private static void RegisterSourceControlCases()
    {
        Test("old saved unbound source nodes do not return after source deletion", SourceLegacyDeletion);
        Test("saved source metadata removes deleted native nodes and preserves dynamic native and ordinary nodes", SourceNativeDeletion);
        Test("source link edits update inherited links while preserving explicit mixed-flow overrides", SourceLinkMerge);
        Test("deleted source nodes still used by data or dynamic links remain blocked and repairable", SourceReferencedDeletion);
        Test("deleted source entry requires explicit replacement and is then removed", SourceEntryDeletion);
    }

    /// <summary>Create an original form snapshot without publishing configuration files.</summary>
    private static FlowProgram SourceSnapshot(string name, bool bound)
    {
        using (SourceSyncPage page = new SourceSyncPage(name, true, bound))
            return FlowProgramFile.Snapshot(new FlowModuleEditor(page, new FlowProgram { ModuleId = page.GetType().FullName + "/" + name }).Program);
    }

    /// <summary>Find a source step by its stable control identity.</summary>
    private static FlowStep SourceStep(FlowProgram program, string name) { return program.Steps.Single(step => step.ControlName == name); }

    /// <summary>Read an old schema file, remove its unbound duplicate and repair NEXT and CASE1 from the current form.</summary>
    private static void SourceLegacyDeletion()
    {
        string name = "SourceLegacy_" + Guid.NewGuid().ToString("N");
        FlowProgram old = SourceSnapshot(name, false);
        foreach (FlowStep step in old.Steps) { step.ControlOrigin = null; step.SourceLinks = null; }
        SourceStep(old, "sourceFirst").X = 345;
        string path = Path.Combine(root, name + ".xml");
        FlowProgramFile.Save(path, new FlowProject { Modules = new List<FlowProgram> { old } });
        byte[] original = File.ReadAllBytes(path);
        using (SourceSyncPage page = new SourceSyncPage(name, false))
        {
            FlowModuleEditor editor = new FlowModuleEditor(page, FlowProgramFile.Load(path).Modules.Single());
            FlowStep first = SourceStep(editor.Program, "sourceFirst"), last = SourceStep(editor.Program, "sourceLast");
            Assert(editor.Program.Steps.Count == 2 && first.Next == last.Id && first.Case1 == last.Id,
                "The deleted legacy control returned or the compiled bypass was not restored.");
            Assert(first.X == 345 && first.Id == SourceStep(old, "sourceFirst").Id && page.Calls == 0
                && original.SequenceEqual(File.ReadAllBytes(path)), "Source migration changed layout, identity, disk or invoked a callback.");
            editor.Validate(null);
            editor.Restore(FlowProgramFile.Load(path).Modules.Single());
            Assert(editor.Program.Steps.Count == 2 && ReferenceEquals(page.First.Next, page.Last), "Reload lost the immutable source link snapshot.");
            FlowProgramFile.Save(path, new FlowProject { Modules = new List<FlowProgram> { editor.Program } });
            FlowProgram saved = FlowProgramFile.Load(path).Modules.Single();
            Assert(saved.Steps.All(step => step.ControlOrigin == "Form") && SourceStep(saved, "sourceFirst").SourceLinks.Next == "sourceLast",
                "Saved source provenance or connection baseline did not round trip.");
        }
    }

    /// <summary>Use explicit source metadata to remove a formerly bound native control, retaining both kinds of dynamic node.</summary>
    private static void SourceNativeDeletion()
    {
        string name = "SourceNative_" + Guid.NewGuid().ToString("N");
        FlowProgram old = SourceSnapshot(name, true);
        FlowStep dynamic = FlowProgramFile.Snapshot(old).Steps.Single(step => step.ControlName == "sourceMiddle");
        dynamic.Id = Guid.NewGuid().ToString("N"); dynamic.ControlName = "flow_" + Guid.NewGuid().ToString("N");
        dynamic.ControlOrigin = "Designer"; dynamic.SourceLinks = null; dynamic.Next = null;
        old.Steps.Add(dynamic);
        FlowStep ordinary = new FlowStep { ControlName = "customDynamicName", ControlOrigin = "Designer", Action = "value", ParentPath = dynamic.ParentPath,
            Caption = "Keep ordinary node", Branches = FlowBranch.Default(), Inputs = new List<FlowInput> { new FlowInput { Name = "Value", Reference = "Keep me" } } };
        old.Steps.Add(ordinary);
        using (SourceSyncPage page = new SourceSyncPage(name, false))
        {
            FlowModuleEditor editor = new FlowModuleEditor(page, old);
            Assert(editor.Program.Steps.All(step => step.ControlName != "sourceMiddle") && editor.Program.Steps.Any(step => step.Id == dynamic.Id)
                && editor.Program.Steps.Any(step => step.Id == ordinary.Id && step.Inputs[0].Reference == "Keep me"), "Source deletion removed dynamic business definitions.");
            editor.Validate(null);
        }
    }

    /// <summary>Three-way merge source connections without replacing a saved link intentionally redirected into a dynamic node.</summary>
    private static void SourceLinkMerge()
    {
        string name = "SourceMerge_" + Guid.NewGuid().ToString("N");
        FlowProgram old = SourceSnapshot(name, true);
        FlowStep originalFirst = SourceStep(old, "sourceFirst");
        FlowStep custom = new FlowStep { ControlName = "flow_" + Guid.NewGuid().ToString("N"), ControlOrigin = "Designer", Action = "flow.none", ExecutionMode = "None",
            ParentPath = originalFirst.ParentPath, Caption = "Inserted condition", Branches = FlowBranch.Default() };
        old.Steps.Add(custom); originalFirst.Next = custom.Id;
        using (SourceSyncPage page = new SourceSyncPage(name, true, true, true))
        {
            FlowModuleEditor editor = new FlowModuleEditor(page, old);
            FlowStep first = SourceStep(editor.Program, "sourceFirst"), last = SourceStep(editor.Program, "sourceLast");
            Assert(first.Next == custom.Id && first.Case1 == last.Id && first.SourceLinks.Next == "sourceLast", "Source updates overwrote a mixed-flow override or failed to update an inherited CASE.");
            editor.Validate(null);
            editor.Restore(FlowProgramFile.Snapshot(editor.Program));
            Assert(SourceStep(editor.Program, "sourceFirst").Next == custom.Id, "Repeated loading replaced the preserved dynamic connection.");
        }
    }

    /// <summary>Keep a visible blocked placeholder for dangling data and dynamic links, then remove it once those references are repaired.</summary>
    private static void SourceReferencedDeletion()
    {
        string name = "SourceReferences_" + Guid.NewGuid().ToString("N");
        FlowProgram old = SourceSnapshot(name, true);
        FlowStep middle = SourceStep(old, "sourceMiddle");
        FlowStep consumer = new FlowStep { ControlName = "flow_" + Guid.NewGuid().ToString("N"), ControlOrigin = "Designer", Action = "value", ParentPath = middle.ParentPath,
            Caption = "Referenced consumer", Next = middle.Id, Branches = FlowBranch.Default(), Inputs = new List<FlowInput> {
                new FlowInput { Name = "Value", Source = "Output", Reference = old.ModuleId + "/" + middle.Id + "/Result" } } };
        old.Steps.Add(consumer); old.InitialEntry = consumer.Id;
        using (SourceSyncPage page = new SourceSyncPage(name, false))
        {
            FlowModuleEditor editor = new FlowModuleEditor(page, old);
            Assert(SourceStep(editor.Program, "sourceMiddle").ControlOrigin == "MissingForm", "A referenced removed source was silently dropped.");
            bool rejected = false;
            try { editor.Validate(middle.Id); } catch (InvalidOperationException error) { rejected = error.Message.Contains("sourceMiddle"); }
            Assert(rejected, "A removed source callback could still validate for production.");
            FlowStep current = editor.Program.Steps.Single(step => step.Id == consumer.Id);
            current.Next = SourceStep(editor.Program, "sourceLast").Id;
            current.Inputs[0].Source = "Constant"; current.Inputs[0].Reference = "Reviewed";
            editor.Restore(FlowProgramFile.Snapshot(editor.Program));
            Assert(editor.Program.Steps.All(step => step.ControlName != "sourceMiddle"), "Repairing references could not clear the missing source placeholder.");
            editor.Validate(null);
        }
    }

    /// <summary>Do not silently select a different production entry when the selected source entry has been removed.</summary>
    private static void SourceEntryDeletion()
    {
        string name = "SourceEntry_" + Guid.NewGuid().ToString("N");
        FlowProgram old = SourceSnapshot(name, true);
        old.InitialEntry = SourceStep(old, "sourceMiddle").Id;
        using (SourceSyncPage page = new SourceSyncPage(name, false))
        {
            FlowModuleEditor editor = new FlowModuleEditor(page, old);
            Assert(editor.Program.Steps.Any(step => step.Id == editor.Program.InitialEntry && step.ControlOrigin == "MissingForm"), "A removed entry silently selected a new root.");
            editor.Program.InitialEntry = SourceStep(editor.Program, "sourceFirst").Id;
            editor.Restore(FlowProgramFile.Snapshot(editor.Program));
            Assert(editor.Program.Steps.All(step => step.ControlName != "sourceMiddle"), "Replacing an invalid entry failed to remove the orphan.");
            editor.Validate(null);
        }
    }
}
