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
        public List<HardwareDefinition> Resources = new List<HardwareDefinition>();
    }

    public sealed class HardwareEntry
    {
        public HardwareDefinition Definition;
        public Control Group;
        public ControlBaseInterface Device;
        /// <summary>显示名称为工程师配置的业务名称。</summary>
        public override string ToString() { return Definition.Name + " · " + Definition.Kind; }
    }

    public sealed class HardwareResources
    {
        private readonly HardForm owner;
        private readonly FlowLayoutPanel canvas;
        private FlowLayoutPanel ioCanvas;
        private readonly Dictionary<string,HardwareEntry> entries = new Dictionary<string,HardwareEntry>();
        public IEnumerable<HardwareEntry> Entries { get { return entries.Values.ToArray(); } }
        public bool IsDirty { get; private set; }
        public bool RequiresRestart { get; private set; }
        public event EventHandler Changed;
        private Exception loadError;
        public string PathName { get { return Path.Combine(SysPara.SettingDataDirectory,"HardwareResources.xml"); } }

        /// <summary>读取经过结构校验的资源定义，创建界面但不连接硬件。</summary>
        public HardwareResources(HardForm form, FlowLayoutPanel host)
        {
            owner = form; canvas = host;
            try
            {
                HardwareProject project = Load(PathName);
                foreach (HardwareDefinition definition in project.Resources) Create(definition);
            }
            catch (Exception error) { loadError = error; }
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

        /// <summary>为新增资源选择未占用通道及唯一名称，默认参数可直接确认。</summary>
        public HardwareDefinition NewDefinition(string kind)
        {
            FlowPrograms.RequireIdle();
            HardwareDefinition draft = new HardwareDefinition { Kind=kind };
            int nameIndex = 1;
            while (Entries.Any(entry=>string.Equals(entry.Definition.Name,kind+"_"+nameIndex,StringComparison.OrdinalIgnoreCase))) nameIndex++;
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
            if (!File.Exists(path)) return new HardwareProject();
            HardwareProject project;
            using (FileStream input = File.OpenRead(path)) project = (HardwareProject)new XmlSerializer(typeof(HardwareProject)).Deserialize(input);
            if (project.Version != 1) throw new InvalidDataException("Unsupported hardware project version.");
            foreach (HardwareDefinition definition in project.Resources) definition.Validate();
            if (project.Resources.GroupBy(item=>item.Id).Any(group=>group.Count()!=1) || project.Resources.GroupBy(item=>item.Name,StringComparer.OrdinalIgnoreCase).Any(group=>group.Count()!=1)) throw new InvalidDataException(HardwareText.T("duplicate"));
            if (project.Resources.GroupBy(PhysicalAddress).Any(group=>group.Count()!=1)) throw new InvalidDataException(HardwareText.T("portDuplicate"));
            return project;
        }

        /// <summary>新增或修改资源草稿；已初始化硬件的参数变更留待重启，避免热切换轴号。</summary>
        public HardwareEntry Put(HardwareDefinition definition)
        {
            FlowPrograms.RequireIdle();
            if (loadError != null) throw new InvalidOperationException(HardwareText.T("loadError"),loadError);
            definition.Validate();
            if (entries.Values.Any(item=>item.Definition.Id!=definition.Id && string.Equals(item.Definition.Name,definition.Name,StringComparison.OrdinalIgnoreCase))) throw new InvalidOperationException(HardwareText.T("duplicate"));
            HardwareEntry current;
            entries.TryGetValue(definition.Id,out current);
            ValidatePhysicalAddress(definition,current == null ? null : current.Device);
            if (entries.TryGetValue(definition.Id,out current))
            {
                if (definition.Kind != current.Definition.Kind) throw new InvalidOperationException(HardwareText.T("invalid"));
                if (!FlowResourceGate.CanAccess(current.Device) || (current.Group is NP_AxisGroup && ((NP_AxisGroup)current.Group).IsManualRunning)) throw new InvalidOperationException(FlowText.T("busy"));
                if (Ready(current.Device)) { current.Definition = definition.Copy(); RequiresRestart = true; IsDirty = true; if (Changed != null) Changed(this,EventArgs.Empty); return current; }
                RemoveGroup(current);
            }
            HardwareEntry created = Create(definition.Copy());
            IsDirty = true;
            if (Changed != null) Changed(this,EventArgs.Empty);
            return created;
        }

        /// <summary>删除前检查流程动作、参数引用、占用和忙状态，再移除组合控件及扫描登记。</summary>
        public void Delete(string id)
        {
            FlowPrograms.RequireIdle();
            HardwareEntry entry;
            if (!entries.TryGetValue(id,out entry)) throw new InvalidOperationException(HardwareText.T("missing"));
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
            if (loadError != null) throw new InvalidOperationException(HardwareText.T("loadError"),loadError);
            HardwareProject project = new HardwareProject { Resources = Entries.Select(entry=>entry.Definition.Copy()).ToList() };
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
            if (loadError != null) throw new InvalidOperationException(HardwareText.T("loadError"),loadError);
            if (Entries.Any(entry=>entry.Group is NP_AxisGroup && ((NP_AxisGroup)entry.Group).IsManualRunning)) throw new InvalidOperationException(FlowText.T("busy"));
            foreach (HardwareEntry entry in Entries) { entry.Definition.Validate(); ValidatePhysicalAddress(entry.Definition,entry.Device); }
        }

        /// <summary>将资源草稿写入主保存的暂存文件，不改变已保存状态。</summary>
        public void WriteDraft(string path)
        {
            ValidateForSave();
            HardwareProject project = new HardwareProject { Resources=Entries.Select(entry=>entry.Definition.Copy()).ToList() };
            using (FileStream output = File.Create(path)) new XmlSerializer(typeof(HardwareProject)).Serialize(output,project);
        }

        /// <summary>完整主保存成功后清除资源脏标记，待重启标记保持不变。</summary>
        public void AcceptSaved() { IsDirty = false; }

        /// <summary>放弃修改后恢复已保存资源，驱动需通过主初始化入口重新就绪。</summary>
        public void DiscardChanges()
        {
            FlowPrograms.RequireIdle();
            if (Entries.Any(entry=>entry.Group is NP_AxisGroup && ((NP_AxisGroup)entry.Group).IsManualRunning)) throw new InvalidOperationException(FlowText.T("busy"));
            HardwareProject restored = Load(PathName);
            foreach (HardwareEntry entry in Entries) RemoveGroup(entry);
            foreach (HardwareDefinition definition in restored.Resources) Create(definition);
            IsDirty = false; RequiresRestart = false; loadError = null;
            if (Changed != null) Changed(this,EventArgs.Empty);
        }

        /// <summary>检测新组合轴在当前配方中的未保存点位修改。</summary>
        public bool HasRecipeChanges()
        {
            return Entries.Where(entry=>entry.Group is NP_AxisGroup).Any(entry=>((NP_AxisGroup)entry.Group).Points.DataSet1.GetChanges()!=null);
        }

        /// <summary>正式初始化前检查定义已保存，运行时连接变更必须通过重启应用。</summary>
        public void ValidateStart(bool requireReady = false)
        {
            if (loadError != null) throw new InvalidOperationException(HardwareText.T("loadError"),loadError);
            if (IsDirty) throw new InvalidOperationException(HardwareText.T("saveFirst"));
            if (RequiresRestart) throw new InvalidOperationException(HardwareText.T("restart"));
            foreach (HardwareEntry entry in Entries) ValidatePhysicalAddress(entry.Definition,entry.Device);
            if (requireReady && Entries.Any(entry=>!Ready(entry.Device))) throw new InvalidOperationException(HardwareText.T("notReady"));
            if (Entries.Any(entry=>entry.Group is NP_AxisGroup && ((NP_AxisGroup)entry.Group).IsManualRunning)) throw new InvalidOperationException(FlowText.T("busy"));
        }

        /// <summary>主界面初始化时连接新加且未初始化的对象，模拟模式只设置标记。</summary>
        public void InitializePending()
        {
            ValidateStart();
            foreach (HardwareEntry entry in Entries)
            {
                if (Ready(entry.Device)) continue;
                entry.Device.SetSimulation(SysPara.Simulation);
                if (SysPara.Simulation) continue;
                lock (entry.Device) entry.Device.Initial();
                if (!Ready(entry.Device)) throw new InvalidOperationException(entry.Definition.Name + ": " + HardwareText.T("notReady"));
            }
        }

        /// <summary>创建一个资源组合和其真实驱动对象，名称及点位表身份来自稳定 ID。</summary>
        private HardwareEntry Create(HardwareDefinition definition)
        {
            HardwareEntry entry = new HardwareEntry { Definition = definition };
            if (definition.Kind == "Axis") CreateAxis(entry);
            else CreateIo(entry);
            entry.Group.Name = "resource_" + definition.Id; entry.Group.Tag = definition;
            entry.Device.InitializeWithSdk = false;
            FlowLayoutPanel host = definition.Kind=="Axis" ? canvas : ioCanvas;
            (host??canvas).Controls.Add(entry.Group); entry.Group.Visible = host!=null;
            entries.Add(definition.Id,entry);
            return entry;
        }

        /// <summary>轴模板绑定独立的当前配方点位表，内部字段不进入通用扫描路径。</summary>
        private void CreateAxis(HardwareEntry entry)
        {
            HardwareDefinition definition = entry.Definition;
            NP_AxisGroup group = new NP_AxisGroup { AxisCaption = definition.Name, LowLimit = definition.LowLimit, HighLimit = definition.HighLimit };
            group.Motor.Name = "Axis_" + definition.Id; group.Motor.Text = definition.Name;
            CopyProperties(definition,group.Motor,HardwareDefinition.CommonFields.Concat(HardwareDefinition.AxisFields));
            DataTable table = owner.RecipeData.Tables[PointsName(definition.Id)];
            if (table == null)
            {
                table = new DataTable(PointsName(definition.Id)) { Namespace = owner.Text };
                table.Columns.Add("Position",typeof(double)); table.Columns.Add("Annotation",typeof(string)); owner.RecipeData.Tables.Add(table);
            }
            group.BindPoints(owner.RecipeData,table,"Position");
            group.CanOperate = () => CanOperate() && !RequiresRestart;
            entry.Group = group; entry.Device = group.Motor;
        }

        /// <summary>输入或输出只实例化所属方向的对象，沿用 SDK 支持的 IO 参数。</summary>
        private void CreateIo(HardwareEntry entry)
        {
            HardwareDefinition definition = entry.Definition;
            NP_IoGroup group = definition.Kind == "Output" ? (NP_IoGroup)new NP_OutputGroup() : new NP_InputGroup();
            group.SignalCaption = definition.Name; group.IO.Name = "IO_" + definition.Id;
            CopyProperties(definition,group.IO,HardwareDefinition.CommonFields.Concat(HardwareDefinition.IoFields));
            group.CanOperate = () => CanOperate() && !RequiresRestart;
            entry.Group = group; entry.Device = group.IO;
        }

        /// <summary>仅拷贝 DTO 中预先列出的可配置属性，不执行任意动态对象方法。</summary>
        private static void CopyProperties(HardwareDefinition definition, object target, IEnumerable<string> fields)
        {
            foreach (string field in fields.Where(name=>name!="Name"))
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
        private static bool CanOperate() { return FlowPrograms.CanEdit && SysPara.SystemMode == RunMode.IDLE && !FlowPrograms.AnyRunning; }

        /// <summary>读取具体类型已有的驱动就绪状态。</summary>
        private static bool Ready(ControlBaseInterface device)
        {
            if (device is NP_Motor) return ((NP_Motor)device).IsReady;
            if (device is NP_Input) return ((NP_Input)device).IsReady;
            return ((NP_Output)device).IsReady;
        }

        /// <summary>阻止两个独立资源对象绑定同一物理通道，避免按对象加锁绕过通道互斥。</summary>
        private static void ValidatePhysicalAddress(HardwareDefinition definition,ControlBaseInterface current)
        {
            string address = PhysicalAddress(definition);
            foreach (ControlBaseInterface device in SDKPara.ControlsSnapshot())
            {
                if (ReferenceEquals(device,current)) continue;
                string kind = device is NP_Motor ? "Axis" : device is NP_Input ? "Input" : device is NP_Output ? "Output" : null;
                if (kind != definition.Kind) continue;
                HardwareDefinition existing = new HardwareDefinition { Kind = kind };
                foreach (string field in HardwareDefinition.CommonFields.Concat(kind == "Axis" ? new string[0] : HardwareDefinition.IoFields).Where(name=>name!="Name"))
                {
                    System.Reflection.PropertyInfo property = device.GetType().GetProperty(field);
                    if (property != null) typeof(HardwareDefinition).GetField(field).SetValue(existing,property.GetValue(device,null));
                }
                if (PhysicalAddress(existing) == address) throw new InvalidOperationException(HardwareText.T("portDuplicate")+" "+device.Name);
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
