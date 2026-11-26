using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;
using AlphaRap.FlowProgramming;
using NPSDK;
using NPSDK.Motion;

namespace AlphaRap.Configuration
{
    public sealed class HardwareProject
    {
        public int Version = 1;
        public string DefaultsTag = "";
        public List<HardwareDefinition> Resources = new List<HardwareDefinition>();
        public List<HardwareIoPage> IoPages = new List<HardwareIoPage>();
        public List<HardwareCylinderDefinition> Cylinders = new List<HardwareCylinderDefinition>();
        public bool CylindersMigrated;
    }

    public sealed class HardwareIoPage
    {
        public string Id = Guid.NewGuid().ToString("N");
        public string Name = "IO";
        public string Text = "";

        /// <summary>返回不持有内部草稿的选项卡副本。</summary>
        public HardwareIoPage Copy() { return (HardwareIoPage)MemberwiseClone(); }
    }

    public sealed class HardwareEntry
    {
        public HardwareDefinition Definition;
        public Control Group;
        public ControlBaseInterface Device;
        /// <summary>显示名称为工程师配置的业务名称。</summary>
        public override string ToString() { return (string.IsNullOrWhiteSpace(Device.Text) ? Definition.Name : Device.Text) + " · " + Definition.Kind; }
    }

    public sealed partial class HardwareResources
    {
        private readonly HardForm owner;
        private readonly FlowLayoutPanel canvas;
        private FlowLayoutPanel ioCanvas;
        private readonly Dictionary<string,HardwareEntry> entries = new Dictionary<string,HardwareEntry>();
        private readonly List<HardwareIoPage> ioPages = new List<HardwareIoPage>();
        public IEnumerable<HardwareEntry> Entries { get { return entries.Values.ToArray(); } }
        public IEnumerable<HardwareIoPage> IoPages { get { return ioPages.Select(page => page.Copy()).ToArray(); } }
        public bool IsDirty { get; private set; }
        public bool RequiresRestart { get; private set; }
        public event EventHandler Changed;
        private Exception loadError;
        private string defaultsTag;
        public string PathName { get { return Path.GetFullPath(Path.Combine(SysPara.SettingDataDirectory, "HardwareResources.xml")); } }
        internal string LoadFailure { get { return loadError?.Message; } }

        /// <summary>读取经过结构校验的资源定义，创建界面但不连接硬件。</summary>
        public HardwareResources(HardForm form, FlowLayoutPanel host)
        {
            owner = form; canvas = host;
            Changed += BindCylinderOutputs;
            try
            {
                HardwareProject project = LoadWithDefaults();
                defaultsTag = project.DefaultsTag;
                ioPages.AddRange(project.IoPages);
                foreach (HardwareDefinition definition in project.Resources) Create(definition);
                ReplaceCylinders(project);
            }
            catch (Exception error)
            {
                foreach (HardwareEntry entry in Entries) RemoveGroup(entry);
                loadError = error;
            }
        }

        /// <summary>按配置 ID 取当前的资源；读取不会创建控件或访问设备。</summary>
        internal HardwareEntry Find(string id)
        {
            HardwareEntry entry;
            return id != null && entries.TryGetValue(id, out entry) && !entry.Group.IsDisposed ? entry : null;
        }

        /// <summary>将 IO 组合交给独立的手动 IO 页面，保持原驱动和绑定身份。</summary>
        public void BindIoCanvas(FlowLayoutPanel host)
        {
            if (host == null) throw new ArgumentNullException("host");
            ioCanvas = host;
            foreach (HardwareEntry entry in Entries.Where(entry=>entry.Definition.Kind!="Axis"))
            {
                host.Controls.Add(entry.Group); entry.Group.Visible = true;
            }
        }

        /// <summary>将共享 IO 和气缸归还硬件目录，关闭或替换手动页时保留同一驱动。</summary>
        internal void UnbindIoCanvas(FlowLayoutPanel host)
        {
            if (!ReferenceEquals(ioCanvas, host)) return;
            ioCanvas = null;
            if (canvas.IsDisposed || canvas.Disposing) return;
            foreach (HardwareEntry entry in Entries.Where(entry => entry.Group.Parent == host))
            {
                entry.Group.Hide();
                canvas.Controls.Add(entry.Group);
            }
            foreach (HardwareCylinderEntry entry in CylinderEntries.Where(entry => entry.Card.Parent == host))
            {
                entry.Card.Hide();
                canvas.Controls.Add(entry.Card);
            }
        }

        /// <summary>为新增资源选择未占用通道及唯一名称，默认参数可直接确认。</summary>
        public HardwareDefinition NewDefinition(string kind)
        {
            RequireIoPageEdit();
            HardwareDefinition draft = new HardwareDefinition { Kind=kind, IoPageId=kind=="Axis" ? "" : ioPages.First().Id };
            int nameIndex = 1;
            while (Entries.Any(entry => string.Equals(entry.Definition.Name, kind + "_" + nameIndex, StringComparison.OrdinalIgnoreCase)) ||
                cylinders.Values.Any(entry => string.Equals(entry.Definition.Name, kind + "_" + nameIndex, StringComparison.OrdinalIgnoreCase))) nameIndex++;
            draft.Name = kind+"_"+nameIndex;
            int count = kind=="Axis" ? 10 : 999*32;
            for (int index=0;index<count;index++)
            {
                draft.Port = kind=="Axis" ? index.ToString() : (index/32+1).ToString("D3")+(index%32).ToString("D2");
                try { ValidatePhysicalAddress(draft,null); return draft; }
                catch (InvalidOperationException) { }
            }
            throw new InvalidOperationException(HardwareText.T("noFreePort"));
        }

        /// <summary>加载全部定义并验证唯一身份，文件损坏时不生成空工程覆盖原配置。</summary>
        public static HardwareProject Load(string path)
        {
            path = Path.GetFullPath(path);
            try { return ReadProject(path); }
            catch (Exception error) when (error is InvalidDataException || error is IOException || error is UnauthorizedAccessException || error is InvalidOperationException)
            {
                string details = error.Message;
                if (error.InnerException != null) details += Environment.NewLine + error.InnerException.Message;
                throw new InvalidDataException(string.Format(HardwareText.T("fileReadError"), path, details), error);
            }
        }

        /// <summary>读取机器硬件定义并在创建任何控件前验证完整配置。</summary>
        private static HardwareProject ReadProject(string path)
        {
            HardwareProject project;
            try
            {
                using (FileStream input = File.OpenRead(path)) project = (HardwareProject)new XmlSerializer(typeof(HardwareProject)).Deserialize(input);
            }
            catch (FileNotFoundException) { project = new HardwareProject(); }
            catch (DirectoryNotFoundException) { project = new HardwareProject(); }
            if (project?.Resources == null || project.IoPages == null || project.Resources.Any(item => item == null))
                throw new InvalidDataException(HardwareText.T("invalid"));
            if (project.Version != 1) throw new InvalidDataException("Unsupported hardware project version.");
            if (project.IoPages.Count == 0) project.IoPages.Add(new HardwareIoPage { Id="default", Name="IO" });
            foreach (HardwareDefinition definition in project.Resources.Where(item => item.Kind != "Axis" && string.IsNullOrEmpty(item.IoPageId))) definition.IoPageId = project.IoPages[0].Id;
            ValidateIoPages(project);
            foreach (HardwareDefinition definition in project.Resources)
            {
                try { definition.Validate(); }
                catch (InvalidOperationException error)
                {
                    throw new InvalidDataException(string.Format(HardwareText.T("resourceInvalid"), definition.Name, definition.Id, error.Message), error);
                }
            }
            CheckDuplicateResources(project.Resources, item => item.Id, "idConflict");
            CheckDuplicateResources(project.Resources, item => item.Name, "nameConflict");
            CheckDuplicateResources(project.Resources.Where(item => !string.IsNullOrEmpty(item.StationKey)), item => item.StationKey, "nameConflict");
            CheckDuplicateResources(project.Resources.Where(item => !item.ConnectionPending), PhysicalAddress, "addressConflict", includePorts: true);
            ValidateCylinders(project, false);
            return project;
        }

        /// <summary>合入机器默认 IO 和气缸，一次加载完成后再创建控件。</summary>
        private HardwareProject LoadWithDefaults()
        {
            HardwareProject project = Load(PathName);
            HardwareProject defaults = owner.ResourceDefaults;
            if (defaults != null && project.DefaultsTag != defaults.DefaultsTag) MergeIoDefaults(project, defaults);
            MergeCylinderDefaults(project, defaults);
            ValidateCylinders(project, false);
            return project;
        }

        /// <summary>首次加载项目模板时合入缺失资源及分组，保留已有配置和 ID。</summary>
        private static void MergeIoDefaults(HardwareProject project, HardwareProject defaults)
        {
            foreach (HardwareIoPage page in defaults.IoPages)
            {
                HardwareIoPage existing = project.IoPages.FirstOrDefault(item => item.Id == page.Id ||
                    string.Equals(item.Name, page.Name, StringComparison.OrdinalIgnoreCase));
                if (existing == null) project.IoPages.Insert(Math.Min(defaults.IoPages.IndexOf(page), project.IoPages.Count), page.Copy());
                string pageId = existing?.Id ?? page.Id;
                foreach (HardwareDefinition definition in defaults.Resources.Where(item => item.IoPageId == page.Id))
                    MergeDefault(project, definition, pageId);
            }
            ValidateIoPages(project);
            CheckDuplicateResources(project.Resources, item => item.Id, "idConflict");
            CheckDuplicateResources(project.Resources.Where(item => !string.IsNullOrEmpty(item.StationKey)), item => item.StationKey, "nameConflict");
            project.DefaultsTag = defaults.DefaultsTag;
        }

        /// <summary>只将同方向且同名的现有 IO 交给相应工站信号。</summary>
        private static void MergeDefault(HardwareProject project, HardwareDefinition template, string pageId)
        {
            if (project.Resources.Any(entry => entry.StationKey == template.StationKey)) return;
            HardwareDefinition current = project.Resources.FirstOrDefault(entry =>
                string.Equals(entry.Name, template.Name, StringComparison.OrdinalIgnoreCase));
            if (current != null)
            {
                if (current.Kind != template.Kind || !string.IsNullOrEmpty(current.StationKey))
                    throw new InvalidDataException(string.Format(HardwareText.T("resourceInvalid"), current.Name, current.Id, HardwareText.T("duplicate")));
                current.StationKey = template.StationKey;
                return;
            }
            HardwareDefinition added = template.Copy();
            added.IoPageId = pageId;
            added.Validate();
            project.Resources.Add(added);
        }

        /// <summary>以资源 ID 读取轴或 IO 的显示文字。</summary>
        public static string DisplayText(HardwareDefinition definition) => FrameworkLanguage.ResourceText("HardwareResources",
            definition.Kind + "_" + definition.Id, definition.Name, definition.Text);

        /// <summary>以页签 ID 读取 IO 分组的显示文字。</summary>
        internal static string DisplayText(HardwareIoPage page) => FrameworkLanguage.ResourceText("HardwareResources",
            "IoPage_" + page.Id, page.Name, page.Text);

        /// <summary>列出首组冲突的键与所有关联资源，便于在文件中定位。</summary>
        private static void CheckDuplicateResources(IEnumerable<HardwareDefinition> resources, Func<HardwareDefinition, string> key, string message, bool includePorts = false)
        {
            var duplicate = resources.GroupBy(key, StringComparer.OrdinalIgnoreCase).FirstOrDefault(group => group.Count() > 1);
            if (duplicate == null) return;
            string names = string.Join(", ", duplicate.Select(item => item.Name + " [" + item.Id + "]" +
                (includePorts && item.Vendor != NPVendor.Keyence && item.Vendor != NPVendor.Omron ? " (" + item.Port + ")" : "")));
            throw new InvalidDataException(string.Format(HardwareText.T(message), duplicate.Key, names));
        }

        /// <summary>新增或修改资源草稿；已初始化硬件的参数变更留待重启，避免热切换轴号。</summary>
        public HardwareEntry Put(HardwareDefinition definition)
        {
            FlowPrograms.RequireIdle();
            if (loadError != null) throw new InvalidOperationException(loadError.Message, loadError);
            definition = definition.Copy();
            if (definition.Kind != "Axis" && string.IsNullOrEmpty(definition.IoPageId)) definition.IoPageId = ioPages.First().Id;
            if (definition.Kind != "Axis" && !ioPages.Any(page => page.Id == definition.IoPageId)) throw new InvalidOperationException(HardwareText.T("ioPageMissing"));
            definition.Validate();
            if (entries.Values.Any(item=>item.Definition.Id!=definition.Id && string.Equals(item.Definition.Name,definition.Name,StringComparison.OrdinalIgnoreCase))) throw new InvalidOperationException(HardwareText.T("duplicate"));
            if (cylinders.Values.Any(item => string.Equals(item.Definition.Name, definition.Name, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(item.Definition.Id, definition.Id, StringComparison.OrdinalIgnoreCase))) throw new InvalidOperationException(HardwareText.T("duplicate"));
            HardwareEntry current;
            entries.TryGetValue(definition.Id, out current);
            definition.StationKey = current?.Definition.StationKey ?? FindStationKey(definition);
            if (current == null && entries.Keys.Contains(definition.Id, StringComparer.OrdinalIgnoreCase))
                throw new InvalidOperationException(HardwareText.T("duplicate"));
            ValidatePhysicalAddress(definition, current?.Device);
            if (current != null)
            {
                if (definition.Kind != current.Definition.Kind) throw new InvalidOperationException(HardwareText.T("invalid"));
                if (!FlowResourceGate.CanAccess(current.Device) || (current.Group is NP_AxisGroup && ((NP_AxisGroup)current.Group).IsManualRunning)) throw new InvalidOperationException(FlowText.T("busy"));
                if (HardwareControlTypes.Resolve(definition.Kind,definition.ControlType) != HardwareControlTypes.Resolve(current.Definition.Kind,current.Definition.ControlType))
                    FlowPrograms.RequireHardwareUnused(definition.Id,PointsName(definition.Id));
                bool connectionChanged = current.Definition.Fields().Union(definition.Fields()).Where(name => name != "Name" && name != "Text")
                    .Any(name => !Equals(typeof(HardwareDefinition).GetField(name).GetValue(current.Definition), typeof(HardwareDefinition).GetField(name).GetValue(definition)));
                if (!connectionChanged || Ready(current.Device))
                {
                    RequiresRestart |= connectionChanged;
                    current.Definition = definition.Copy(); current.Group.Tag = current.Definition;
                    if (current.Group is NP_AxisGroup axis)
                    {
                        current.Device.Text = definition.Name;
                        axis.AxisCaption = DisplayText(definition);
                    }
                    else current.Device.Text = DisplayText(definition);
                    IsDirty = true; if (Changed != null) Changed(this,EventArgs.Empty); return current;
                }
            }
            HardwareEntry created = BuildEntry(definition.Copy());
            if (current != null) RemoveGroup(current);
            MountEntry(created);
            IsDirty = true;
            if (Changed != null) Changed(this,EventArgs.Empty);
            return created;
        }

        /// <summary>恢复同名的原工站 IO 时继续识别其用途。</summary>
        private string FindStationKey(HardwareDefinition definition)
        {
            string key = owner.ResourceDefaults?.Resources.FirstOrDefault(item => item.Kind == definition.Kind &&
                string.Equals(item.Name, definition.Name, StringComparison.OrdinalIgnoreCase))?.StationKey;
            return string.IsNullOrEmpty(key) || entries.Values.Any(item => item.Definition.StationKey == key) ? "" : key;
        }

        /// <summary>删除前检查流程动作、参数引用、占用和忙状态，再移除组合控件及扫描登记。</summary>
        public void Delete(string id)
        {
            FlowPrograms.RequireIdle();
            HardwareEntry entry;
            if (!entries.TryGetValue(id,out entry)) throw new InvalidOperationException(HardwareText.T("missing"));
            RequireCylinderOutputUnused(id);
            FlowPrograms.RequireHardwareUnused(id,PointsName(id));
            if (!FlowResourceGate.CanAccess(entry.Device) || (entry.Group is NP_AxisGroup && ((NP_AxisGroup)entry.Group).IsManualRunning)) throw new InvalidOperationException(FlowText.T("busy"));
            RemoveGroup(entry);
            IsDirty = true;
            if (Changed != null) Changed(this,EventArgs.Empty);
        }

        /// <summary>只保存机器资源定义，点位沿用主界面保存到当前配方的既有机制。</summary>
        public void Save()
        {
            FlowPrograms.RequireIdle();
            if (loadError != null) throw new InvalidOperationException(loadError.Message, loadError);
            HardwareProject project = CaptureProject();
            ValidateIoPages(project);
            ValidateCylinders(project, true);
            foreach (HardwareDefinition definition in project.Resources) definition.Validate();
            string path = System.IO.Path.GetFullPath(PathName); Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            string temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                using (FileStream output = File.Create(temp)) new XmlSerializer(typeof(HardwareProject)).Serialize(output,project);
                if (File.Exists(path)) File.Replace(temp,path,path+".bak"); else File.Move(temp,path);
                IsDirty = false;
            }
            finally { if (File.Exists(temp)) File.Delete(temp); }
        }

        /// <summary>校验主保存使用的机器资源草稿，允许保存需重启的参数变更。</summary>
        public void ValidateForSave()
        {
            FlowPrograms.RequireIdle();
            if (loadError != null) throw new InvalidOperationException(loadError.Message, loadError);
            if (Entries.Any(entry=>entry.Group is NP_AxisGroup && ((NP_AxisGroup)entry.Group).IsManualRunning)) throw new InvalidOperationException(FlowText.T("busy"));
            foreach (HardwareEntry entry in Entries) { entry.Definition.Validate(); ValidatePhysicalAddress(entry.Definition,entry.Device); }
            HardwareProject project = CaptureProject();
            ValidateIoPages(project);
            ValidateCylinders(project, true);
        }

        /// <summary>将资源草稿写入主保存的暂存文件，不改变已保存状态。</summary>
        public void WriteDraft(string path)
        {
            ValidateForSave();
            HardwareProject project = CaptureProject();
            using (FileStream output = File.Create(path)) new XmlSerializer(typeof(HardwareProject)).Serialize(output,project);
        }

        /// <summary>完整主保存成功后清除资源脏标记，待重启标记保持不变。</summary>
        public void AcceptSaved() { IsDirty = false; }

        /// <summary>放弃修改后恢复已保存资源，驱动需通过主初始化入口重新就绪。</summary>
        public void DiscardChanges()
        {
            FlowPrograms.RequireIdle();
            if (Entries.Any(entry=>entry.Group is NP_AxisGroup && ((NP_AxisGroup)entry.Group).IsManualRunning)) throw new InvalidOperationException(FlowText.T("busy"));
            HardwareProject restored = LoadWithDefaults();
            defaultsTag = restored.DefaultsTag;
            foreach (HardwareEntry entry in Entries) RemoveGroup(entry);
            ioPages.Clear(); ioPages.AddRange(restored.IoPages);
            foreach (HardwareDefinition definition in restored.Resources) Create(definition);
            ReplaceCylinders(restored);
            IsDirty = false; RequiresRestart = false; loadError = null;
            if (Changed != null) Changed(this,EventArgs.Empty);
        }

        /// <summary>新增机器级 IO 分组选项卡，不创建或初始化 IO 驱动。</summary>
        public HardwareIoPage AddIoPage(string name)
        {
            RequireIoPageEdit(); ValidateIoPageName(name, null);
            HardwareIoPage page = new HardwareIoPage { Name=name };
            ioPages.Add(page); NotifyIoPageChange();
            return page.Copy();
        }

        /// <summary>修改页签显示名，保留 IO、流程绑定及配方数据的身份。</summary>
        public void RenameIoPage(string id, string name)
        {
            RequireIoPageEdit();
            HardwareIoPage page = FindIoPage(id);
            ValidateIoPageName(name, id);
            page.Name = name; NotifyIoPageChange();
        }

        /// <summary>仅修改页签默认显示文字，主界面保存后写入硬件资源文件。</summary>
        internal void SetIoPageText(string id, string text)
        {
            RequireIoPageEdit();
            HardwareIoPage page = FindIoPage(id);
            if (page.Text == text) return;
            page.Text = text;
            NotifyIoPageChange();
        }

        /// <summary>只允许删除空页，并始终保留至少一个 IO 分组。</summary>
        public void DeleteIoPage(string id)
        {
            RequireIoPageEdit();
            HardwareIoPage page = FindIoPage(id);
            if (Entries.Any(entry => entry.Definition.Kind != "Axis" && entry.Definition.IoPageId == id)) throw new InvalidOperationException(HardwareText.T("ioPageNotEmpty"));
            if (ioPages.Count == 1) throw new InvalidOperationException(HardwareText.T("ioPageLast"));
            ioPages.Remove(page); NotifyIoPageChange();
        }

        /// <summary>移动 IO 到另一分组，仅修改显示归属，不重建驱动或改变流程引用。</summary>
        public void MoveIoToPage(string resourceId, string pageId)
        {
            RequireIoPageEdit(); FindIoPage(pageId);
            HardwareEntry entry;
            if (!entries.TryGetValue(resourceId, out entry) || entry.Definition.Kind == "Axis") throw new InvalidOperationException(HardwareText.T("missing"));
            if (entry.Definition.IoPageId == pageId) return;
            entry.Definition.IoPageId = pageId; NotifyIoPageChange();
        }

        /// <summary>读取明确的分组身份，缺失时拒绝静默落到其他页面。</summary>
        private HardwareIoPage FindIoPage(string id)
        {
            HardwareIoPage page = ioPages.FirstOrDefault(item => item.Id == id);
            if (page == null) throw new InvalidOperationException(HardwareText.T("ioPageMissing"));
            return page;
        }

        /// <summary>校验非空且唯一的用户页签名，名称不参与语言翻译。</summary>
        private void ValidateIoPageName(string name, string id)
        {
            if (string.IsNullOrWhiteSpace(name) || name != name.Trim() || ioPages.Any(page => page.Id != id && string.Equals(page.Name, name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException(HardwareText.T("ioPageNameInvalid"));
        }

        /// <summary>分组编辑沿用资源权限与运行互斥，加载失败时保护原文件。</summary>
        private void RequireIoPageEdit()
        {
            FlowPrograms.RequireIdle();
            if (loadError != null) throw new InvalidOperationException(loadError.Message, loadError);
        }

        /// <summary>标记分组草稿并让绑定视图同步显示，不在子页面落盘。</summary>
        private void NotifyIoPageChange()
        {
            IsDirty = true;
            if (Changed != null) Changed(this, EventArgs.Empty);
        }

        /// <summary>统一捕获资源和 IO 页签，主保存与独立回归使用同一份快照。</summary>
        private HardwareProject CaptureProject()
        {
            return new HardwareProject
            {
                DefaultsTag = defaultsTag, Resources = Entries.Select(entry => entry.Definition.Copy()).ToList(), IoPages = IoPages.ToList(),
                CylindersMigrated = cylindersMigrated, Cylinders = cylinders.Values.Select(entry => entry.Definition.Copy()).ToList()
            };
        }

        /// <summary>验证页签身份与 IO 归属完整性，损坏引用不能生成错误页面。</summary>
        private static void ValidateIoPages(HardwareProject project)
        {
            if (project.IoPages == null || project.IoPages.Count == 0 || project.IoPages.Any(page => page == null || string.IsNullOrWhiteSpace(page.Name) || page.Name != page.Name.Trim()))
                throw new InvalidDataException(HardwareText.T("ioPageNameInvalid"));
            Guid identity;
            if (project.IoPages.Any(page => page.Id != "default" && !Guid.TryParseExact(page.Id, "N", out identity))
                || project.IoPages.GroupBy(page => page.Id, StringComparer.OrdinalIgnoreCase).Any(group => group.Count() != 1)
                || project.IoPages.GroupBy(page => page.Name, StringComparer.OrdinalIgnoreCase).Any(group => group.Count() != 1))
                throw new InvalidDataException(HardwareText.T("ioPageNameInvalid"));
            var missing = project.Resources.FirstOrDefault(item => item.Kind != "Axis" && !project.IoPages.Any(page => page.Id == item.IoPageId));
            if (missing != null)
                throw new InvalidDataException(string.Format(HardwareText.T("ioPageReference"), missing.Name, missing.IoPageId));
        }

        /// <summary>检测新组合轴在当前配方中的未保存点位修改。</summary>
        public bool HasRecipeChanges()
        {
            return Entries.Where(entry=>entry.Group is NP_AxisGroup).Any(entry=>((NP_AxisGroup)entry.Group).Points.DataSet1.GetChanges()!=null);
        }

        /// <summary>创建一个资源组合和其真实驱动对象，名称及点位表身份来自稳定 ID。</summary>
        private HardwareEntry Create(HardwareDefinition definition)
        {
            HardwareEntry entry = BuildEntry(definition);
            MountEntry(entry);
            return entry;
        }

        /// <summary>先完整创建候选控件，构造或参数赋值失败不移除现有资源。</summary>
        private HardwareEntry BuildEntry(HardwareDefinition definition)
        {
            HardwareEntry entry = new HardwareEntry { Definition = definition };
            try
            {
                if (definition.Kind == "Axis") CreateAxis(entry);
                else CreateIo(entry);
            }
            catch
            {
                if (entry.Group != null) entry.Group.Dispose();
                throw;
            }
            entry.Group.Name = "resource_" + definition.Id; entry.Group.Tag = definition;
            entry.Device.InitializeWithSdk = false;
            return entry;
        }

        /// <summary>候选资源创建成功后加入所属页面及稳定身份目录。</summary>
        private void MountEntry(HardwareEntry entry)
        {
            HardwareDefinition definition = entry.Definition;
            FlowLayoutPanel host = definition.Kind=="Axis" ? canvas : ioCanvas;
            (host??canvas).Controls.Add(entry.Group); entry.Group.Visible = host!=null;
            entries.Add(definition.Id,entry);
        }

        /// <summary>轴模板绑定独立的当前配方点位表，内部字段不进入通用扫描路径。</summary>
        private void CreateAxis(HardwareEntry entry)
        {
            HardwareDefinition definition = entry.Definition;
            NP_AxisGroup group = new NP_AxisGroup { AxisCaption = DisplayText(definition), LowLimit = definition.LowLimit, HighLimit = definition.HighLimit };
            entry.Group = group;
            Type type = HardwareControlTypes.Resolve(definition.Kind,definition.ControlType);
            if (type != typeof(NP_Motor))
            {
                NP_Motor motor = (NP_Motor)Activator.CreateInstance(type);
                motor.Visible = false;
                group.BindMotor(motor);
                group.Controls.Add(motor);
            }
            group.Motor.Name = "Axis_" + definition.Id; group.Motor.Text = definition.Name;
            CopyProperties(definition,group.Motor,definition.Fields());
            DataTable table = owner.RecipeData.Tables[PointsName(definition.Id)];
            if (table == null)
            {
                table = new DataTable(PointsName(definition.Id)) { Namespace = owner.Text };
                table.Columns.Add("Position",typeof(double)); table.Columns.Add("Annotation",typeof(string)); owner.RecipeData.Tables.Add(table);
            }
            table.ExtendedProperties["AllowEmpty"] = true;
            group.BindPoints(owner.RecipeData,table,"Position");
            group.CanOperate = () => CanOperate() && !RequiresRestart;
            entry.Group = group; entry.Device = group.Motor;
        }

        /// <summary>输入或输出只实例化所属方向的对象，沿用 SDK 支持的 IO 参数。</summary>
        private void CreateIo(HardwareEntry entry)
        {
            HardwareDefinition definition = entry.Definition;
            NP_IoGroup group = definition.Kind == "Output" ? (NP_IoGroup)new NP_OutputGroup() : new NP_InputGroup();
            entry.Group = group;
            Type type = HardwareControlTypes.Resolve(definition.Kind,definition.ControlType);
            Type basis = definition.Kind == "Output" ? typeof(NP_Output) : typeof(NP_Input);
            if (type != basis)
            {
                ControlBaseInterface signal = (ControlBaseInterface)Activator.CreateInstance(type);
                group.BindIO(signal);
            }
            group.SignalCaption = DisplayText(definition); group.IO.Name = "IO_" + definition.Id;
            CopyProperties(definition,group.IO,definition.Fields());
            if (definition.ConnectionPending)
            {
                if (group.IO is NP_Input input) input.Port = "";
                if (group.IO is NP_Output output) output.Port = "";
            }
            group.CanOperate = () => CanOperate() && !RequiresRestart && !entry.Definition.ConnectionPending
                && (string.IsNullOrEmpty(entry.Definition.StationKey) || (!IsDirty && !SysPara.Simulation));
            entry.Group = group; entry.Device = group.IO;
        }

        /// <summary>仅拷贝 DTO 中预先列出的可配置属性，不执行任意动态对象方法。</summary>
        private static void CopyProperties(HardwareDefinition definition, object target, IEnumerable<string> fields)
        {
            foreach (string field in fields.Where(name=>name!="Name" && name!="Text" && name!="ControlType"))
            {
                System.Reflection.PropertyInfo property = target.GetType().GetProperty(field);
                if (property != null && property.CanWrite) property.SetValue(target,typeof(HardwareDefinition).GetField(field).GetValue(definition),null);
            }
        }

        /// <summary>从界面移除并释放资源，点位表留存于配方以便撤销或恢复旧工程。</summary>
        private void RemoveGroup(HardwareEntry entry)
        {
            entries.Remove(entry.Definition.Id);
            if (entry.Group.Parent != null) entry.Group.Parent.Controls.Remove(entry.Group);
            entry.Group.Dispose();
        }

        /// <summary>机器空闲且有维护权限时才允许手动资源操作。</summary>
        private static bool CanOperate() { return FlowPrograms.CanEdit && !FlowPrograms.ConfigurationBusy && !ProjectConfiguration.IsBusy; }

        /// <summary>读取具体类型已有的驱动就绪状态。</summary>
        private static bool Ready(ControlBaseInterface device)
        {
            if (device is NP_Motor) return ((NP_Motor)device).IsReady;
            if (device is NP_Input) return ((NP_Input)device).IsReady;
            return ((NP_Output)device).IsReady;
        }

        /// <summary>阻止两个独立资源对象绑定同一物理通道，避免按对象加锁绕过通道互斥。</summary>
        private void ValidatePhysicalAddress(HardwareDefinition definition,ControlBaseInterface current)
        {
            if (definition.ConnectionPending) return;
            string address = PhysicalAddress(definition);
            HardwareEntry conflict = entries.Values.FirstOrDefault(item => !ReferenceEquals(item.Device, current)
                && !item.Definition.ConnectionPending && string.Equals(PhysicalAddress(item.Definition), address, StringComparison.OrdinalIgnoreCase));
            if (conflict != null) throw new InvalidOperationException(HardwareText.T("portDuplicate") + " " + conflict.Definition.Name);
            foreach (ControlBaseInterface device in SDKPara.ControlsSnapshot())
            {
                if (ReferenceEquals(device,current)) continue;
                string kind = device is NP_Motor ? "Axis" : device is NP_Input ? "Input" : device is NP_Output ? "Output" : null;
                if (kind != definition.Kind) continue;
                if ((device is NP_Input input && string.IsNullOrEmpty(input.Port)) || (device is NP_Output output && string.IsNullOrEmpty(output.Port))) continue;
                HardwareDefinition existing = new HardwareDefinition { Kind = kind };
                foreach (string field in HardwareDefinition.CommonFields.Concat(kind == "Axis" ? new string[0] : HardwareDefinition.IoFields).Where(name=>name!="Name" && name!="Text" && name!="ControlType"))
                {
                    System.Reflection.PropertyInfo property = device.GetType().GetProperty(field);
                    if (property != null) typeof(HardwareDefinition).GetField(field).SetValue(existing,property.GetValue(device,null));
                }
                if (string.Equals(PhysicalAddress(existing), address, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException(HardwareText.T("portDuplicate")+" "+device.Name);
            }
        }

        /// <summary>区分 IO 方向和各厂商地址形式，显示名不参与物理通道身份。</summary>
        private static string PhysicalAddress(HardwareDefinition definition)
        {
            string address = definition.Port;
            if (definition.Kind == "Axis" && definition.Vendor == NPVendor.Leadshine && !string.IsNullOrEmpty(address)) address = address.Substring(address.Length-1);
            if (definition.Kind != "Axis" && definition.Vendor == NPVendor.Leadshine && address != null && address.Length >= 5)
            {
                int card, bit;
                if (int.TryParse(address.Substring(0,3),out card) && int.TryParse(address.Substring(3),out bit)) address = ((Math.Max(card,1)-1)*32+bit+8).ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            if (definition.Kind != "Axis" && definition.Vendor == NPVendor.Keyence) address = definition.PlcIp+":"+definition.PlcPort+"/"+definition.DeviceType+definition.Address;
            if (definition.Kind != "Axis" && definition.Vendor == NPVendor.Omron) address = definition.PlcIp+":"+definition.OmronPort+"/"+definition.OmronAddress;
            return definition.Kind+"/"+definition.Vendor+"/"+address;
        }

        /// <summary>稳定资源 ID 对应唯一的配方点位表。</summary>
        public static string PointsName(string id) { return "AxisPoints_" + id; }
    }
}
