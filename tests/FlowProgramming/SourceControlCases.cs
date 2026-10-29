using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
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
        Test("deleted source nodes stay deleted while surviving data consumers require rebinding", SourceReferencedDeletion);
        Test("deleted source entry is cleared without selecting a replacement", SourceEntryDeletion);
        RegisterContainerCases();
        RegisterDocumentPageCases();
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
            Assert(first.X == page.First.Left && first.Id == SourceStep(old, "sourceFirst").Id && page.Calls == 0
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

    /// <summary>Never recreate deleted source controls for surviving data bindings or dynamic links.</summary>
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
            Assert(editor.Program.Steps.All(step => step.Id != middle.Id) && page.Controls.Find("sourceMiddle", true).Length == 0,
                "A deleted source control was recreated because another node still referenced it.");
            bool rejected = false;
            try { editor.Validate(consumer.Id); } catch (InvalidOperationException) { rejected = true; }
            Assert(rejected, "A consumer of deleted source data was silently assigned a replacement value.");
            FlowStep current = editor.Program.Steps.Single(step => step.Id == consumer.Id);
            Assert(current.Next == null, "A surviving dynamic link still pointed at a deleted source node.");
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
            Assert(editor.Program.InitialEntries.Count == 0 && editor.Program.Steps.All(step => step.ControlName != "sourceMiddle"),
                "A removed entry was recreated or silently replaced by another source flow.");
            editor.Program.InitialEntry = SourceStep(editor.Program, "sourceFirst").Id;
            editor.Restore(FlowProgramFile.Snapshot(editor.Program));
            Assert(editor.Program.Steps.All(step => step.ControlName != "sourceMiddle"), "Replacing an invalid entry failed to remove the orphan.");
            editor.Validate(null);
        }
    }
}

internal sealed class ContainerRevisionPage : Flow_BaseForm
{
    internal TabPage PointPage;
    internal int Calls;

    /// <summary>Represent a source revision with a renamed, moved or deleted nested tab.</summary>
    internal ContainerRevisionPage(string name, string tab, bool rootNode = false)
    {
        Name = name;
        if (rootNode)
        {
            Flow_Chart root = new Flow_Chart { Name = "survivingRoot", text = "Surviving root" };
            root.FlowChartRun += Work; InitialPage.Controls.Add(root);
        }
        if (tab == null) return;
        TabControl nested = new TabControl { Name = "nestedTabs", Dock = DockStyle.Fill };
        PointPage = new TabPage { Name = tab, Text = tab };
        Flow_Chart node = new Flow_Chart { Name = "nativeWork", text = "Native work", Location = new Point(40, 40) };
        node.FlowChartRun += Work;
        PointPage.Controls.Add(node); nested.TabPages.Add(PointPage); InitialPage.Controls.Add(nested);
    }

    /// <summary>Record invocation without touching hardware.</summary>
    private Flow_Chart.ResultType Work() { Calls++; return Flow_Chart.ResultType.End; }

    /// <summary>Release an isolated fixture from the SDK catalogue.</summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing) Flow_Module.FlowChart_ModuleList.Remove(this);
        base.Dispose(disposing);
    }
}

internal static partial class Cases
{
    /// <summary>Verify saved layouts follow actual source deletion without creating replacement tabs.</summary>
    private static void RegisterContainerCases()
    {
        Test("deleted tabs remove saved nodes, entries and links without restoring tabs or changing disk automatically", RemovedContainerCleanup);
        Test("legacy import does not replace a deleted selected entry with another surviving flow", RemovedLegacyEntryCleanup);
        Test("moved source nodes keep their identity while deleted-container dynamic nodes are removed", MovedContainerCleanup);
        Test("ambiguous container paths do not choose the first page or recreate deleted nodes", AmbiguousContainerCleanup);
        Test("incompatible saved containers do not accept flow nodes or cause startup exceptions", InvalidContainerCleanup);
        Test("deleted derived Forms are not recreated or executed from saved configuration", RemovedFormCompatibility);
        Test("reusing a deleted page name keeps only the new source controls and their designer positions", ReusedPageCleanup);
        Test("workflow page translation keys do not reuse deleted page titles", ReusedPageLanguages);
        Test("the actual Gantry designer survives a saved obsolete MES graph without adding controls", GantryDesignerAuthority);
    }

    /// <summary>Create a source node and a dynamic node in a nested tab before the source structure changes.</summary>
    private static FlowProgram ContainerSnapshot(string name)
    {
        using (ContainerRevisionPage page = new ContainerRevisionPage(name, "oldTab"))
        {
            FlowModuleEditor owner = new FlowModuleEditor(page, new FlowProgram { ModuleId = page.GetType().FullName + "/" + name });
            FlowStep native = owner.Program.Steps.Single(); native.X = 345;
            owner.AddNode("", "value", page.PointPage, new Point(40, 145));
            owner.Program.InitialEntry = native.Id;
            return FlowProgramFile.Snapshot(owner.Program);
        }
    }

    /// <summary>Remove deleted-tab content and its routing while keeping surviving output bindings explicitly invalid.</summary>
    private static void RemovedContainerCleanup()
    {
        string name = "ContainerDeleted_" + Guid.NewGuid().ToString("N");
        FlowProgram saved = ContainerSnapshot(name);
        FlowStep native = saved.Steps.Single(step => step.ControlName == "nativeWork");
        FlowStep dynamic = saved.Steps.Single(step => step.ControlOrigin == "Designer");
        string reference = saved.ModuleId + "/" + native.Id + "/Value";
        FlowStep consumer = new FlowStep { ControlName = "flow_" + Guid.NewGuid().ToString("N"), Caption = "Surviving action", Action = "value",
            ControlOrigin = "Designer", ParentPath = "/FlowChartTable/InitialPage", Branches = FlowBranch.Default(),
            Inputs = new List<FlowInput> { new FlowInput { Name = "Value", Source = "Output", Reference = reference } } };
        consumer.Next = native.Id; consumer.Case1 = dynamic.Id; saved.Steps.Add(consumer);
        string path = Path.Combine(root, name + ".xml");
        FlowProgramFile.Save(path, new FlowProject { Modules = new List<FlowProgram> { saved } });
        byte[] before = File.ReadAllBytes(path);
        using (ContainerRevisionPage page = new ContainerRevisionPage(name, null))
        {
            FlowModuleEditor owner = new FlowModuleEditor(page, FlowProgramFile.Load(path).Modules.Single());
            Assert(owner.Program.Steps.Count == 1 && owner.Program.Steps[0].Id == consumer.Id
                && owner.Program.InitialEntries.Count == 0 && owner.Program.AutoEntries.Count == 0,
                "Deleted-tab nodes survived or the loader silently chose a replacement entry.");
            Assert(owner.Program.Steps[0].Next == null && owner.Program.Steps[0].Case1 == null
                && owner.Program.RemovedControls.Contains(native.ControlName) && owner.Program.RemovedControls.Contains(dynamic.ControlName),
                "Deleted-node routing or deletion markers were not cleaned.");
            Assert(page.FlowChartTable.TabPages.Count == 2 && page.Controls.Find("oldTab", true).Length == 0 && page.Calls == 0,
                "A deleted or recovery tab was created, or a callback was invoked during loading.");
            owner.CaptureLayout();
            Assert(before.SequenceEqual(File.ReadAllBytes(path)) && owner.Program.Steps[0].Inputs[0].Reference == reference,
                "Loading overwrote disk or replaced a surviving action's data binding with a fabricated value.");
            owner.SetEntries(true, new[] { consumer.Id });
            bool rejected = false; try { owner.Validate(null); } catch (InvalidOperationException) { rejected = true; }
            Assert(rejected, "An action consuming deleted-node data was allowed to run with a replacement value.");
            owner.Program.Steps[0].Inputs[0].Source = "Constant"; owner.Program.Steps[0].Inputs[0].Reference = "Retained value";
            owner.Validate(null); saved = FlowProgramFile.Snapshot(owner.Program);
        }
        FlowProgramFile.Save(path, new FlowProject { Modules = new List<FlowProgram> { saved } });
        using (ContainerRevisionPage page = new ContainerRevisionPage(name, null))
        {
            FlowModuleEditor owner = new FlowModuleEditor(page, FlowProgramFile.Load(path).Modules.Single());
            Assert(owner.Program.Steps.Count == 1 && page.FlowChartTable.TabPages.Count == 2 && owner.Program.InitialEntry == consumer.Id,
                "Save/reload resurrected deleted tab contents or lost the chosen surviving entry.");
            owner.Start(consumer.Id, false, true); Wait(owner.Runtime.Completion);
            Assert(owner.Runtime.Error == null && page.Calls == 0, "The surviving flow could not execute independently.");
        }
    }

    /// <summary>A surviving source control provides the only evidence for moving a saved node to a different container.</summary>
    private static void MovedContainerCleanup()
    {
        string name = "ContainerMoved_" + Guid.NewGuid().ToString("N"); FlowProgram saved = ContainerSnapshot(name);
        using (ContainerRevisionPage page = new ContainerRevisionPage(name, "newTab"))
        {
            FlowModuleEditor owner = new FlowModuleEditor(page, saved);
            FlowStep native = owner.Program.Steps.Single();
            Assert(native.ControlName == "nativeWork" && native.ParentPath.EndsWith("/newTab") && native.X == 40
                && owner.Program.InitialEntry == native.Id && page.FlowChartTable.TabPages.Count == 2,
                "A moved source node lost its identity, or deleted-container contents were recreated.");
            owner.Validate(null); owner.Start(native.Id, false, true); Wait(owner.Runtime.Completion);
            Assert(owner.Runtime.Error == null && page.Calls == 1, "The surviving native callback did not run independently.");
        }
    }

    /// <summary>A deleted explicit entry must not become an unrelated initialization flow during legacy import.</summary>
    private static void RemovedLegacyEntryCleanup()
    {
        string name = "LegacyContainerDeleted_" + Guid.NewGuid().ToString("N");
        FlowProgram saved = ContainerSnapshot(name); saved.Mode = "Legacy";
        using (ContainerRevisionPage page = new ContainerRevisionPage(name, null, true))
        {
            FlowModuleEditor owner = new FlowModuleEditor(page, saved);
            Assert(owner.Program.Steps.Count == 1 && owner.Program.Steps[0].ControlName == "survivingRoot"
                && owner.Program.InitialEntries.Count == 0 && page.Calls == 0,
                "Deleting a legacy entry silently selected a different source flow.");
        }
    }

    /// <summary>Do not move nodes into an arbitrary same-named sibling.</summary>
    private static void AmbiguousContainerCleanup()
    {
        string name = "ContainerAmbiguous_" + Guid.NewGuid().ToString("N"); FlowProgram saved = ContainerSnapshot(name);
        using (ContainerRevisionPage page = new ContainerRevisionPage(name, "oldTab"))
        {
            page.InitialPage.Controls.Add(new TabControl { Name = "nestedTabs" });
            FlowModuleEditor owner = new FlowModuleEditor(page, saved);
            Assert(owner.Program.Steps.Count == 0 && owner.Program.InitialEntries.Count == 0 && page.Calls == 0
                && page.Controls.Find("nativeWork", true).Length == 0 && page.FlowChartTable.TabPages.Count == 2,
                "An ambiguous container selected a sibling, recreated a node or invoked a callback.");
        }
    }

    /// <summary>TabControls only accept TabPages and cannot directly host flow nodes.</summary>
    private static void InvalidContainerCleanup()
    {
        string name = "ContainerTypeChanged_" + Guid.NewGuid().ToString("N"); FlowProgram saved = ContainerSnapshot(name);
        FlowStep dynamic = saved.Steps.Single(step => step.ControlOrigin == "Designer");
        dynamic.ParentPath = dynamic.ParentPath.Substring(0, dynamic.ParentPath.LastIndexOf('/'));
        using (ContainerRevisionPage page = new ContainerRevisionPage(name, "oldTab"))
        {
            FlowModuleEditor owner = new FlowModuleEditor(page, saved);
            Assert(owner.Program.Steps.Count == 1 && owner.Program.Steps[0].ControlName == "nativeWork" && page.Calls == 0,
                "An incompatible container was used or a valid source node was lost.");
        }
    }

    /// <summary>Missing source Form definitions do not create phantom runtime modules or change saved files on load.</summary>
    private static void RemovedFormCompatibility()
    {
        Type type = typeof(FlowPrograms);
        System.Reflection.FieldInfo project = type.GetField("project", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        System.Reflection.FieldInfo installed = type.GetField("installed", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        object previous = project.GetValue(null), wasInstalled = installed.GetValue(null);
        var factory = Flow_Module.ProgramControllerFactory;
        string settings = SysPara.SettingDataDirectory;
        try
        {
            SysPara.SettingDataDirectory = Path.Combine(root, "removed-form");
            FlowProgram ghost = new FlowProgram { ModuleId = "Removed.Namespace.DeletedForm/OldStation", Mode = "Program" };
            FlowProgramFile.Save(FlowPrograms.FilePath, new FlowProject { Modules = new List<FlowProgram> { ghost } });
            byte[] before = File.ReadAllBytes(FlowPrograms.FilePath); installed.SetValue(null, false);
            FlowPrograms.Install();
            Assert(!FlowPrograms.Modules.Any(module => module.Program.ModuleId == ghost.ModuleId) && before.SequenceEqual(File.ReadAllBytes(FlowPrograms.FilePath)),
                "A deleted source Form was reinstantiated or its saved definition was silently overwritten.");
        }
        finally { SysPara.SettingDataDirectory = settings; project.SetValue(null, previous); installed.SetValue(null, wasInstalled); Flow_Module.ProgramControllerFactory = factory; }
    }

    /// <summary>Simulate deleting MES then creating a screw page with the same WinForms-generated tab name.</summary>
    private static void ReusedPageCleanup()
    {
        string name = "PageReused_" + Guid.NewGuid().ToString("N"); FlowProgram saved = ContainerSnapshot(name);
        for (int i = 0; i < saved.Steps.Count; i++)
        { saved.Steps[i].ControlName = "deletedMes" + i; saved.Steps[i].ControlOrigin = "Form"; }
        using (ContainerRevisionPage page = new ContainerRevisionPage(name, "oldTab"))
        {
            page.PointPage.Text = "打螺丝";
            FlowModuleEditor owner = new FlowModuleEditor(page, saved);
            Assert(owner.Program.Steps.Count == 1 && owner.Program.Steps[0].ControlName == "nativeWork"
                && owner.Program.InitialEntries.Count == 0 && page.PointPage.Controls.OfType<Flow_Chart>().Count() == 1,
                "Reusing tabPage identity recreated deleted MES controls or selected a different entry.");
            FlowStep current = owner.Program.Steps[0]; current.X = 750; current.Y = 410; current.Caption = "Stale saved title";
            string action = current.Action; current.Action = "native.ObsoleteHandler"; current.Target = "native/DeletedOwner";
            owner.Restore(FlowProgramFile.Snapshot(owner.Program));
            Assert(owner.Program.Steps[0].Id == current.Id && owner.Program.Steps[0].X == 40 && owner.Program.Steps[0].Y == 40
                && owner.Program.Steps[0].Caption == "Native work" && owner.Program.Steps[0].Action == action
                && owner.Program.Steps[0].Target != "native/DeletedOwner" && page.PointPage.Text == "打螺丝" && page.Calls == 0,
                "Saved source layout overrode the actual designer or a callback ran during loading.");
            FlowProgram cleared = FlowProgramFile.Snapshot(owner.Program);
            owner.Restore(cleared);
            Assert(page.PointPage.Controls.Count == 1 && owner.Program.Steps.Count == 1, "Repeated reload duplicated a source control.");
        }
    }

    /// <summary>Preserve old language records while isolating new workflow titles and retaining translation support.</summary>
    private static void ReusedPageLanguages()
    {
        string name = "PageLanguages_" + Guid.NewGuid().ToString("N");
        if (SysPara.ComponentLangurageList == null) host.GetMethod("InitialLanguageData", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
        host.GetMethod("LangMsg").Invoke(null, new object[] { name, "oldTab", "MES流程", "MES workflow", "Flujo MES" });
        using (ContainerRevisionPage page = new ContainerRevisionPage(name, "oldTab"))
        {
            page.PointPage.Text = "打螺丝";
            RegisterSourcePageLanguage(page, name);
            foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
            {
                SwitchLanguage(language);
                Assert(page.PointPage.Text == "打螺丝", "A deleted MES translation replaced the new source page title.");
            }
            string key = SysPara.ComponentLangurageList[0].Single(item => ReferenceEquals(item.Component, page.PointPage)).ComponentName;
            MethodInfo setText = host.GetMethod("SetLangText", BindingFlags.Static | BindingFlags.NonPublic);
            setText.Invoke(null, new object[] { LanguageType.English, name, key, "Screw fastening" });
            setText.Invoke(null, new object[] { LanguageType.Español, name, key, "Atornillado" });
            RegisterSourcePageLanguage(page, name);
            SwitchLanguage(LanguageType.English); Assert(page.PointPage.Text == "Screw fastening", "New page English translation was not applied.");
            RegisterSourcePageLanguage(page, name);
            SwitchLanguage(LanguageType.Español); Assert(page.PointPage.Text == "Atornillado", "Re-registering a translated page changed its language identity.");
            SwitchLanguage(LanguageType.Chinese); Assert(page.PointPage.Text == "打螺丝", "Chinese source title was lost.");
            Assert((string)host.GetMethod("LangText").Invoke(null, new object[] { name, "oldTab", "" }) == "MES流程", "The migration overwrote the old site translation.");
        }
        using (ContainerRevisionPage unchanged = new ContainerRevisionPage(name, "oldTab"))
        {
            unchanged.PointPage.Text = "MES流程"; RegisterSourcePageLanguage(unchanged, name);
            SwitchLanguage(LanguageType.English);
            Assert(unchanged.PointPage.Text == "MES workflow", "An unchanged source page lost its existing translation.");
        }
        using (ContainerRevisionPage englishSource = new ContainerRevisionPage(name, "oldTab"))
        {
            englishSource.PointPage.Text = "MES workflow"; RegisterSourcePageLanguage(englishSource, name);
            SwitchLanguage(LanguageType.Chinese);
            Assert(englishSource.PointPage.Text == "MES流程", "An unchanged English source page lost its Chinese translation.");
        }
        SwitchLanguage(LanguageType.Chinese);
    }

    /// <summary>Use the real application Form and its current Designer source; never start initialization or hardware.</summary>
    private static void GantryDesignerAuthority()
    {
        using (AlphaRap.TaskProcess.Gantry page = new AlphaRap.TaskProcess.Gantry())
        {
            try
            {
                Flow_Chart[] source = SourceCharts(page).ToArray();
                Dictionary<string, Rectangle> bounds = source.ToDictionary(node => node.Name, node => node.Bounds);
                FlowProgram saved = new FlowProgram { ModuleId = page.GetType().FullName + "/" + page.Name };
                for (int i = 0; i < 5; i++) saved.Steps.Add(new FlowStep { ControlName = "deleted_MES_" + i, ControlOrigin = "Form",
                    Caption = "Old MES " + i, ParentPath = "/FlowChartTable/AutoPage/tabControl1/tabPage3", X = 223, Y = 80 + i * 55 });
                for (int i = 0; i < 5; i++) saved.Steps[i].Next = saved.Steps[(i + 1) % 5].Id;
                saved.AutoEntry = saved.Steps[0].Id;
                RegisterSourcePageLanguage(page, page.Name);
                FlowModuleEditor owner = new FlowModuleEditor(page, saved);
                Assert(owner.Program.Steps.Count == source.Length && SourceCharts(page).Count() == source.Length
                    && owner.Program.Steps.All(step => bounds.ContainsKey(step.ControlName)) && owner.Program.AutoEntries.Count == 0,
                    "The real Gantry recreated saved MES controls or kept a deleted MES entry.");
                Assert(SourceCharts(page).All(node => bounds[node.Name] == node.Bounds), "Gantry runtime positions differ from the current designer.");
                foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
                {
                    SwitchLanguage(language);
                    Assert(page.Controls.Find("tabPage3", true).Cast<TabPage>().All(tab => tab.Text != "MES流程"),
                        "The old Gantry MES language key overwrote the screw page.");
                }
                SwitchLanguage(LanguageType.Chinese);
                page.Size = new Size(1280, 780); page.FlowChartTable.SelectedTab = page.AutoPage;
                TabPage screw = page.Controls.Find("tabPage3", true).OfType<TabPage>().SingleOrDefault();
                if (screw != null) ((TabControl)screw.Parent).SelectedTab = screw;
                page.Show(); Application.DoEvents();
                using (Bitmap bitmap = new Bitmap(page.Width, page.Height))
                { page.DrawToBitmap(bitmap, new Rectangle(Point.Empty, page.Size)); bitmap.Save(Path.Combine(root, "gantry-designer-authority.png")); }
            }
            finally { Flow_Module.FlowChart_ModuleList.Remove(page); SwitchLanguage(LanguageType.Chinese); }
        }
    }

    /// <summary>Exercise the same language registration used by startup without initializing hardware.</summary>
    private static void RegisterSourcePageLanguage(Control page, string name)
    {
        host.GetMethod("RegisterAndApplyLanguage").Invoke(null, new object[] { page, name });
    }

    /// <summary>Enumerate actual node instances while excluding internal node decorations.</summary>
    private static IEnumerable<Flow_Chart> SourceCharts(Control parent)
    {
        foreach (Control child in parent.Controls)
        {
            Flow_Chart node = child as Flow_Chart;
            if (node != null) { yield return node; continue; }
            foreach (Flow_Chart nested in SourceCharts(child)) yield return nested;
        }
    }
}
