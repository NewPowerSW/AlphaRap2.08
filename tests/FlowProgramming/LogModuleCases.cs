using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using AlphaRap;
using AlphaRap.Configuration;
using AlphaRap.FlowProgramming;
using DataSave;
using NPSDK;

internal static partial class Cases
{
    /// <summary>Exercise configurable log modules without devices or production paths.</summary>
    private static void RegisterLogModuleCases()
    {
        Test("log drafts publish only through Main Save and reload independently of recipes", LogDraftLifetime);
        Test("mixed flow binds numeric output to a selected log module and persists one record", LogFlowBinding);
        Test("log simulation and conversion failures never pollute production logs", LogSimulation);
        Test("log editor preserves typed drafts across three languages and cancellation", LogEditorLanguages);
        Test("invalid log definitions cannot replace built-in alarm tables or share directories", LogValidation);
        Test("bound log modules cannot be deleted and failed legacy writes remain retryable", LogDeleteAndRetry);
        Test("alarm history resolves original fields and multiline content from CSV headers", LogAlarmHistory);
        Test("removing the path page preserves actual alarm storage and existing production directories", LogWithoutPathPage);
    }

    private sealed class LogFixture : IDisposable
    {
        internal readonly DataForm Form;
        internal readonly string Folder;
        private readonly string settings = SysPara.SettingDataDirectory;
        private readonly object previous;
        /// <summary>Isolate machine-level log definitions and the host resource catalogue.</summary>
        internal LogFixture()
        {
            Folder = Path.Combine(root, "logs-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(Folder);
            SysPara.SettingDataDirectory = Folder; previous = host.GetField("DataF").GetValue(null);
            Form = new DataForm(); SetHost("DataF", Form);
        }
        /// <summary>Restore host state without removing test evidence.</summary>
        public void Dispose() { SetHost("DataF", previous); Form.Dispose(); SysPara.SettingDataDirectory = settings; }
        /// <summary>Construct a typed product log in the isolated directory.</summary>
        internal LogModuleDefinition Definition()
        {
            return new LogModuleDefinition { Name = "点胶日志", Directory = Path.Combine(Folder, "records"), Fields = new List<ProductionField> {
                new ProductionField { Name = "Barcode" }, new ProductionField { Name = "Weight", Type = "Number" }, new ProductionField { Name = "OK", Type = "Boolean" } } };
        }
        /// <summary>Use the exact save participant to stage and accept a definition.</summary>
        internal void Save()
        {
            Method("WriteLogModulesDraft", Path.Combine(Folder, "DataForm.LogModules.xml")); Method("AcceptLogModulesDraft");
        }
        /// <summary>Invoke the internal participant without adding test-only production APIs.</summary>
        internal void Method(string name, params object[] args) { typeof(DataForm).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Form, args); }
    }

    /// <summary>Draft changes are visible in the toolbox but not executable until committed.</summary>
    private static void LogDraftLifetime()
    {
        using (LogFixture fixture = new LogFixture())
        {
            LogModuleDefinition definition = fixture.Definition(); fixture.Form.SetLogModulesDraft(new List<LogModuleDefinition> { definition });
            Assert(!fixture.Form.GetLogModules(false).Any() && FlowResources.Capture(true).Items.ContainsKey("log/" + definition.Id)
                && !FlowResources.Capture().Items.ContainsKey("log/" + definition.Id), "Draft log was executable or not available to editing.");
            fixture.Save(); definition.Name = "Renamed"; fixture.Form.SetLogModulesDraft(new List<LogModuleDefinition> { definition });
            fixture.Method("DiscardLogModulesDraft");
            Assert(fixture.Form.GetLogModules().Single().Name == "点胶日志", "Discard did not recover saved log definitions.");
            string recipe = SysPara.FilePath; SysPara.FilePath = Path.Combine(fixture.Folder, "OtherRecipe.xml");
            try { using (DataForm fresh = new DataForm()) Assert(fresh.GetLogModules(false).Single().Id == definition.Id, "Recipe switch changed machine log definitions."); }
            finally { SysPara.FilePath = recipe; }
            Assert(fixture.Form.Alarm.Items.Count == 8 && fixture.Form.Alarm.Text == "Alarm", "Built-in alarm log changed.");
        }
    }

    /// <summary>Run a real two-node graph with a typed upstream data binding and the existing runtime.</summary>
    private static void LogFlowBinding()
    {
        using (LogFixture fixture = new LogFixture())
        using (Flow_BaseForm form = new Flow_BaseForm { Name = "LogRuntime" })
        {
            try
            {
                LogModuleDefinition definition = fixture.Definition(); fixture.Form.SetLogModulesDraft(new List<LogModuleDefinition> { definition }); fixture.Save();
                FlowModuleEditor owner = FlowPrograms.Attach(form); owner.SetEditing(true);
                FlowStep calculate = owner.AddNode("", "calculate", form.AutoPage, new Point(40, 30));
                calculate.Inputs.Single(input => input.Name == "A").Reference = "1.25";
                calculate.Inputs.Single(input => input.Name == "B").Reference = "2.5";
                FlowStep log = owner.AddNode("log/" + definition.Id, "log.append", form.AutoPage, new Point(40, 120));
                log.Inputs.Single(input => input.Name == "Barcode").Reference = "SN,\"中文\"\n001";
                FlowInput weight = log.Inputs.Single(input => input.Name == "Weight"); weight.Source = "Output"; weight.Reference = owner.Program.ModuleId + "/" + calculate.Id + "/Value";
                log.Inputs.Single(input => input.Name == "OK").Reference = "true";
                calculate.Next = log.Id; owner.ApplyLinks(); owner.Start(calculate.Id, false, false); Wait(owner.Runtime.Completion);
                Assert(owner.Runtime.Error == null, "Logging flow failed: " + owner.Runtime.Error);
                TypeCollection1 query = new TypeCollection1 { SavePath = definition.Directory }; List<string[]> rows = query.ReadDataFromToFile(DateTime.Today, DateTime.Today);
                Assert(rows.Count == 2 && rows[1][1] == "SN,\"中文\"\n001" && rows[1][2] == "3.75" && rows[1][3] == "True", "Flow output did not reach the selected log fields.");
            }
            finally { Flow_Module.FlowChart_ModuleList.Remove(form); }
        }
    }

    /// <summary>Simulation is isolated and invalid typed values create no record.</summary>
    private static void LogSimulation()
    {
        using (LogFixture fixture = new LogFixture())
        {
            LogModuleDefinition definition = fixture.Definition(); fixture.Form.SetLogModulesDraft(new List<LogModuleDefinition> { definition }); fixture.Save();
            FlowMethod method = fixture.Form.GetFlowLogs().Single(log => log.Id == "log/" + definition.Id).GetFlowMethods().Single();
            FlowCall call = new FlowCall { Inputs = new Dictionary<string, object> { { "Barcode", "A" }, { "Weight", 1.25 }, { "OK", true } }, Token = CancellationToken.None, Simulation = true };
            string path = Convert.ToString(method.Invoke(call).GetAwaiter().GetResult()["Path"]);
            Assert(path.Contains("Simulation") && !File.Exists(Path.Combine(definition.Directory, DateTime.Now.ToString("yyyy/MM/yyyy-MM-dd") + ".csv")), "Simulation wrote a production log.");
            call.Simulation = false; call.Inputs["Weight"] = "bad"; bool rejected = false;
            try { method.Invoke(call).GetAwaiter().GetResult(); } catch { rejected = true; }
            Assert(rejected && Directory.GetFiles(definition.Directory, "*.csv", SearchOption.AllDirectories).Length == 1, "Invalid input was silently saved.");
        }
    }

    /// <summary>Render the editor and retain unsaved user values through language changes.</summary>
    private static void LogEditorLanguages()
    {
        using (LogFixture fixture = new LogFixture())
        {
            fixture.Form.SetLogModulesDraft(new List<LogModuleDefinition> { fixture.Definition() });
            LanguageType previous = SysPara.LanguageShow;
            using (LogModulesForm dialog = new LogModulesForm(fixture.Form))
            {
                dialog.Show(); Application.DoEvents();
                TextBox name = (TextBox)dialog.Controls.Find("logModuleName", true).Single(); name.Text = "Unsaved name";
                DataGridView fields = (DataGridView)dialog.Controls.Find("logFields", true).Single(); fields.Rows[1].Cells[0].Value = "NewWeight";
                foreach (LanguageType language in new[] { LanguageType.Chinese, LanguageType.English, LanguageType.Español })
                {
                    SwitchLanguage(language); Application.DoEvents();
                    Assert(name.Text == "Unsaved name" && Convert.ToString(fields.Rows[1].Cells[0].Value) == "NewWeight", "Language switch discarded the active log draft.");
                    using (Bitmap image = new Bitmap(dialog.Width, dialog.Height)) { dialog.DrawToBitmap(image, new Rectangle(0, 0, dialog.Width, dialog.Height)); image.Save(Path.Combine(root, "log-modules-" + language + ".png")); }
                }
                dialog.Close();
            }
            SwitchLanguage(previous);
            Assert(fixture.Form.GetLogModules().Single().Name == "点胶日志", "Cancel committed the window draft.");
        }
    }

    /// <summary>Protect built-in alarm modules, identities, automatic time and output directories.</summary>
    private static void LogValidation()
    {
        using (LogFixture fixture = new LogFixture())
        {
            LogModuleDefinition definition = fixture.Definition(); definition.Name = "Alarm";
            bool rejected = false; try { fixture.Form.SetLogModulesDraft(new List<LogModuleDefinition> { definition }); } catch { rejected = true; }
            Assert(rejected && fixture.Form.Alarm.Items.Count == 8, "Custom module replaced the alarm schema.");
            definition.Name = "Custom"; definition.Fields.Add(new ProductionField { Name = "Time" }); rejected = false;
            try { fixture.Form.SetLogModulesDraft(new List<LogModuleDefinition> { definition }); } catch { rejected = true; }
            Assert(rejected && !fixture.Form.GetLogModules().Any(), "Reserved Time was accepted.");
        }
    }

    /// <summary>Require explicit removal of flow bindings and record duplicate suppression only after successful IO.</summary>
    private static void LogDeleteAndRetry()
    {
        using (LogFixture fixture = new LogFixture())
        using (Flow_BaseForm form = new Flow_BaseForm { Name = "BoundLog" })
        {
            LogModuleDefinition definition = fixture.Definition(); fixture.Form.SetLogModulesDraft(new List<LogModuleDefinition> { definition });
            FlowModuleEditor owner = FlowPrograms.Attach(form); owner.SetEditing(true);
            FlowStep node = owner.AddNode("log/" + definition.Id, "log.append", form.AutoPage, new Point(20, 20));
            bool blocked = false; try { fixture.Form.SetLogModulesDraft(new List<LogModuleDefinition>()); } catch (InvalidOperationException) { blocked = true; }
            Assert(blocked, "A bound log definition was removed."); owner.Delete(node.Id);
            fixture.Form.SetLogModulesDraft(new List<LogModuleDefinition>());
            string path = Path.Combine(fixture.Folder, "blocked"); File.WriteAllText(path, "file"); fixture.Form.LogRun.SavePath = path;
            blocked = false; try { fixture.Form.AddRunLog("retry same message"); } catch (IOException) { blocked = true; }
            fixture.Form.LogRun.SavePath = Path.Combine(fixture.Folder, "retry"); fixture.Form.AddRunLog("retry same message");
            Assert(blocked && fixture.Form.LogRun.ReadDataFromToFile(DateTime.Today, DateTime.Today).Count == 2, "A failed log was marked as successfully recorded.");
        }
    }

    /// <summary>Check the same named-column mapping used by the existing alarm history screen.</summary>
    private static void LogAlarmHistory()
    {
        string[] columns = { "Time", "User", "RecipeID", "RecipeName", "Mode", "Type", "Code", "Content" };
        string[] values = { "2026-10-29 12:00:00:123", "admin", "R1", "Product", "Auto", "ERROR", "2019", "error,\"quoted\"\r\nnext" };
        AlarmRecord record = (AlarmRecord)typeof(AlarmHistory).GetMethod("ParseRecord", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { columns, values });
        Assert(record != null && record.Code == "2019" && record.Content == values[7] && record.RecipeId == "R1" && record.IsAlarm, "Alarm columns or original content were lost.");
    }
    /// <summary>Actual alarm storage and historical production paths do not require the deleted form or writes to its settings.</summary>
    private static void LogWithoutPathPage()
    {
        using (LogFixture fixture = new LogFixture())
        {
            string previousSettings = AlphaRapLibrary.ModuleManager.SettingDataDirectory;
            FieldInfo overrideRoot = typeof(ProductionDataStore).GetField("_rootOverride", BindingFlags.NonPublic | BindingFlags.Static);
            string previousRoot = (string)overrideRoot.GetValue(null);
            try
            {
                AlphaRapLibrary.ModuleManager.SettingDataDirectory = fixture.Folder;
                string alarm = Path.Combine(fixture.Folder, "alarm-data"), production = Path.Combine(fixture.Folder, "production-data");
                string file = Path.Combine(fixture.Folder, "MESForm.xml");
                string xml = "<NewDataSet><Path><AlarmPath>" + System.Security.SecurityElement.Escape(alarm) + "</AlarmPath><ProducPath>" + System.Security.SecurityElement.Escape(production) + "</ProducPath></Path></NewDataSet>";
                File.WriteAllText(file, xml);
                Type directories = typeof(DataForm).Assembly.GetType("AlphaRap.LogDirectories", true);
                directories.GetMethod("RestoreAlarmPath", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { fixture.Form });
                Assert(AlarmHistory.RootPath() == alarm && fixture.Form.Alarm.SavePath == alarm, "Alarm history does not use the actual writer directory.");
                fixture.Form.Alarm.SaveDataToFile(new[] { "operator", "R1", "Recipe", "Auto", "ERROR", "2019", "kept after page removal" });
                int count; List<AlarmRecord> records = AlarmHistory.Query(DateTime.Today, DateTime.Today, AlarmHistory.FilterAll, "", "", out count);
                Assert(records.Count == 1 && records[0].Code == "2019" && count == 1, "Removing the path page broke alarm write/query.");
                ProductionDataStore.SetRoot("");
                Assert(ProductionDataStore.RootPath() == Path.Combine(production, "ProductionData") && File.ReadAllText(file) == xml, "Existing production data was relocated or legacy settings were rewritten.");
                Assert(host.GetField("LogF") == null && typeof(DataForm).Assembly.GetType("AlphaRap.LogForm") == null, "The deleted log path form still starts.");
            }
            finally { AlphaRapLibrary.ModuleManager.SettingDataDirectory = previousSettings; ProductionDataStore.SetRoot(previousRoot); }
        }
    }

}
