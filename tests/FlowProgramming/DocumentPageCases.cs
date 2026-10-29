using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AlphaRap;
using AlphaRap.FlowProgramming;
using NPSDK;

internal sealed class DocumentTrialForm : Flow_BaseForm
{
    internal readonly FlowDocumentCanvas Canvas;
    internal readonly Flow_Chart First = new Flow_Chart { Name = "docFirst", text = "First", Location = new Point(30, 25), Size = new Size(210, 36) };
    internal readonly Flow_Chart Last = new Flow_Chart { Name = "docLast", text = "Last", Location = new Point(30, 100), Size = new Size(210, 36) };
    internal int Calls;

    /// <summary>Create an isolated document host with existing native instances and no hardware.</summary>
    internal DocumentTrialForm(string name, string hostName = "documentCanvas", string key = "trial.initial")
    {
        Name = name;
        Canvas = new FlowDocumentCanvas { Name = hostName, DocumentKey = key, ModuleId = GetType().FullName + "/" + name, LegacyParentPath = "/FlowChartTable/InitialPage" };
        InitialPage.Controls.Add(Canvas); Canvas.Dock = DockStyle.Fill;
        First.Next = Last; First.FlowChartRun += First_FlowChartRun; Last.FlowChartRun += Last_FlowChartRun;
        Canvas.RegisterLegacyNodes(First, Last);
    }

    /// <summary>Count invocations while returning the current native routing contract.</summary>
    private Flow_Chart.ResultType First_FlowChartRun() { Calls++; return Flow_Chart.ResultType.NEXT; }
    /// <summary>End the test chain without equipment actions.</summary>
    private Flow_Chart.ResultType Last_FlowChartRun() { Calls++; return Flow_Chart.ResultType.End; }
    /// <summary>Remove fixtures from the SDK global catalogue.</summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing) Flow_Module.FlowChart_ModuleList.Remove(this);
        base.Dispose(disposing);
    }
}

internal static partial class Cases
{
    /// <summary>Cover a document-owned page from migration through editing, execution, reload and read-only preview.</summary>
    private static void RegisterDocumentPageCases()
    {
        Test("document page imports source once and retains its stable page identity", DocumentPageImport);
        Test("document moves additions deletion and native connections survive a fresh form", DocumentPageRoundtrip);
        Test("an empty saved document page never imports source templates again", EmptyDocumentPage);
        Test("document identity survives a host control rename and rejects a missing host", DocumentPageHostIdentity);
        Test("read-only document preview matches saved controls without callbacks or runtime catalogue entries", DocumentPagePreview);
        Test("the Gantry heartbeat page migrates existing deletion without resurrecting the loop", GantryDocumentMigration);
    }

    /// <summary>Build the smallest real page and capture its initially imported graph.</summary>
    private static FlowProgram DocumentSnapshot(string name)
    {
        using (DocumentTrialForm page = new DocumentTrialForm(name))
        {
            FlowModuleEditor owner = new FlowModuleEditor(page, new FlowProgram { ModuleId = page.GetType().FullName + "/" + name });
            Assert(owner.Program.Pages.Count == 1 && owner.Program.Steps.Count == 2 && owner.Program.Steps.All(step => step.PageId == owner.Program.Pages[0].Id), "Source import lost the document page identity.");
            return FlowProgramFile.Snapshot(owner.Program);
        }
    }

    /// <summary>Initial source routing is imported once, with existing instances and a V5 model roundtrip.</summary>
    private static void DocumentPageImport()
    {
        string name = "DocumentImport_" + Guid.NewGuid().ToString("N"); FlowProgram saved = DocumentSnapshot(name);
        Assert(saved.Steps.Single(step => step.ControlName == "docFirst").Next == saved.Steps.Single(step => step.ControlName == "docLast").Id, "Native source links were not imported.");
        string path = Path.Combine(root, name + ".xml"); FlowProject project = new FlowProject { Modules = new List<FlowProgram> { saved } };
        FlowProgramFile.Save(path, project);
        Assert(project.Version == 5 && FlowProgramFile.Load(path).Modules[0].Pages[0].Id == saved.Pages[0].Id, "Document page was not persisted with V5 protection.");
        using (DocumentTrialForm page = new DocumentTrialForm(name))
        {
            FlowModuleEditor owner = new FlowModuleEditor(page, FlowProgramFile.Load(path).Modules[0]);
            owner.Validate(null); owner.Start(owner.Program.InitialEntry, false, true); Wait(owner.Runtime.Completion);
            Assert(owner.Runtime.Error == null && page.Calls == 2 && ReferenceEquals(page.First.Next, page.Last), "The original native instances did not execute the saved graph.");
        }
    }

    /// <summary>Dynamic and code nodes share one graph while source fields no longer override saved document layout.</summary>
    private static void DocumentPageRoundtrip()
    {
        string name = "DocumentRoundtrip_" + Guid.NewGuid().ToString("N"); FlowProgram saved = DocumentSnapshot(name);
        string first = saved.Steps.Single(step => step.ControlName == "docFirst").Id;
        using (DocumentTrialForm page = new DocumentTrialForm(name))
        {
            FlowModuleEditor owner = new FlowModuleEditor(page, saved); owner.SetEditing(true);
            owner.Delete(saved.Steps.Single(step => step.ControlName == "docLast").Id);
            page.First.Location = new Point(260, 160); page.First.Size = new Size(230, 42);
            FlowStep ordinary = owner.AddNode("None", page.Canvas, new Point(260, 240));
            owner.SetConnection(first, "NEXT", ordinary.Id); owner.CaptureLayout(); saved = FlowProgramFile.Snapshot(owner.Program);
            owner.Restore(FlowProgramFile.Snapshot(saved));
            Assert(page.First.Location == new Point(260, 160), "Reload replaced the saved position with the source template.");
        }
        using (DocumentTrialForm page = new DocumentTrialForm(name))
        {
            FlowModuleEditor owner = new FlowModuleEditor(page, saved);
            Assert(owner.Program.Steps.Count == 2 && owner.Program.Steps.All(step => step.ControlName != "docLast") && !page.Canvas.Controls.Contains(page.Last)
                && page.First.Location == new Point(260, 160) && page.First.Size == new Size(230, 42), "Fresh source templates changed the document graph or layout.");
            owner.Start(first, false, true); Wait(owner.Runtime.Completion);
            Assert(owner.Runtime.Error == null && page.Calls == 1, "A removed native node executed or the mixed document could not finish.");
        }
    }

    /// <summary>Pages remain explicit even when their last node has been deleted.</summary>
    private static void EmptyDocumentPage()
    {
        string name = "EmptyDocument_" + Guid.NewGuid().ToString("N"); FlowProgram saved = DocumentSnapshot(name); string pageId = saved.Pages[0].Id;
        using (DocumentTrialForm page = new DocumentTrialForm(name))
        {
            FlowModuleEditor owner = new FlowModuleEditor(page, saved);
            foreach (string id in owner.Program.Steps.Select(step => step.Id).ToArray()) owner.Delete(id);
            saved = FlowProgramFile.Snapshot(owner.Program);
        }
        using (DocumentTrialForm page = new DocumentTrialForm(name))
        {
            FlowModuleEditor owner = new FlowModuleEditor(page, saved);
            Assert(owner.Program.Pages.Single().Id == pageId && owner.Program.Steps.Count == 0 && page.Canvas.Controls.Count == 0, "An empty document was treated as never imported.");
        }
    }

    /// <summary>UI paths are derived from a stable host key, never selected by a reused tab caption.</summary>
    private static void DocumentPageHostIdentity()
    {
        string name = "HostDocument_" + Guid.NewGuid().ToString("N"); FlowProgram saved = DocumentSnapshot(name);
        using (DocumentTrialForm page = new DocumentTrialForm(name, "renamedCanvas"))
        {
            FlowModuleEditor owner = new FlowModuleEditor(page, FlowProgramFile.Snapshot(saved));
            Assert(owner.Program.Pages[0].Id == saved.Pages[0].Id && owner.Program.Steps.All(step => step.ParentPath.EndsWith("/renamedCanvas")), "A host rename changed document identity.");
        }
        using (DocumentTrialForm page = new DocumentTrialForm(name, "documentCanvas", "replacement.initial"))
        {
            bool rejected = false;
            try { new FlowModuleEditor(page, saved); } catch (InvalidOperationException) { rejected = true; }
            Assert(rejected && page.Calls == 0, "A replacement host silently inherited another page's nodes.");
        }
    }

    /// <summary>Preview consumes the exact saved graph and does not join the runtime resource catalogue.</summary>
    private static void DocumentPagePreview()
    {
        string name = "PreviewDocument_" + Guid.NewGuid().ToString("N"); FlowProgram saved = DocumentSnapshot(name);
        string path = Path.Combine(root, name + ".xml"); FlowProgramFile.Save(path, new FlowProject { Modules = new List<FlowProgram> { saved } });
        byte[] original = File.ReadAllBytes(path); int count = Flow_Module.FlowChart_List.Count;
        using (FlowDocumentCanvas preview = new FlowDocumentCanvas { DocumentKey = "trial.initial", ModuleId = saved.ModuleId, Size = new Size(800, 520) })
        {
            preview.RenderPreview(path);
            Flow_Chart[] nodes = preview.Controls.OfType<Flow_Chart>().ToArray();
            Assert(nodes.Length == saved.Steps.Count && nodes.All(node => saved.Steps.Any(step => step.ControlName == node.Name && node.Location == new Point(step.X, step.Y)
                && node.Size == new Size(step.Width, step.Height))), "Preview differs from the published document.");
            Assert(Flow_Module.FlowChart_List.Count == count && original.SequenceEqual(File.ReadAllBytes(path)), "Preview registered runtime nodes or modified the file.");
            using (Bitmap bitmap = new Bitmap(preview.Width, preview.Height)) { preview.DrawToBitmap(bitmap, new Rectangle(Point.Empty, preview.Size)); bitmap.Save(Path.Combine(root, "document-page-preview.png")); }
        }
    }

    /// <summary>Use the real migrated Gantry host, preserving the user's previously saved deletion marker.</summary>
    private static void GantryDocumentMigration()
    {
        using (AlphaRap.TaskProcess.Gantry page = new AlphaRap.TaskProcess.Gantry())
        {
            try
            {
                FlowDocumentCanvas canvas = page.Controls.Find("heartbeatCanvas", true).OfType<FlowDocumentCanvas>().Single();
                FlowProgram legacy = new FlowProgram { ModuleId = page.GetType().FullName + "/" + page.Name };
                legacy.RemovedControls.Add("FlowAuto_Heart5_4");
                FlowStep first = new FlowStep { ControlName = "FlowAuto_Heart5_1", ControlOrigin = "Form", ParentPath = canvas.LegacyParentPath, Caption = "Saved heartbeat", X = 510, Y = 170 };
                legacy.Steps.Add(first); legacy.AutoEntry = first.Id;
                FlowModuleEditor owner = new FlowModuleEditor(page, legacy);
                Assert(owner.Program.Pages.Count == 1 && owner.Program.Pages[0].Key == canvas.DocumentKey && owner.Program.Steps.Any(step => step.Id == first.Id && step.PageId == owner.Program.Pages[0].Id)
                    && canvas.Controls.OfType<Flow_Chart>().Count() == 1 && canvas.Controls[0].Left == 510,
                    "Gantry discarded the old layout or reintroduced deleted source nodes during migration.");
                owner.Restore(FlowProgramFile.Snapshot(owner.Program));
                Assert(canvas.Controls.OfType<Flow_Chart>().Count() == 1 && page.Controls.Find("FlowAuto_Heart5_4", true).Length == 0, "Reload resurrected the deleted heartbeat loop.");
            }
            finally { Flow_Module.FlowChart_ModuleList.Remove(page); }
        }
    }
}
