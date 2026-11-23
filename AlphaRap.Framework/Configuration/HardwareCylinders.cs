using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AlphaRap.FlowProgramming;
using NPSDK;
using NPSDK.Motion;

namespace AlphaRap.Configuration
{
    /// <summary>保存气缸名称和两路输出的稳定 ID。</summary>
    public sealed class HardwareCylinderDefinition
    {
        public string Id = Guid.NewGuid().ToString("N");
        public string Name = "";
        public string Text = "";
        public string StationKey = "";
        public string OnOutputId = "";
        public string OffOutputId = "";

        /// <summary>复制编辑或保存用的配置快照。</summary>
        public HardwareCylinderDefinition Copy() { return (HardwareCylinderDefinition)MemberwiseClone(); }
    }

    internal sealed class HardwareCylinderEntry
    {
        internal HardwareCylinderDefinition Definition;
        internal NP_Cylinder Cylinder;
        internal GroupBox Card;
    }

    public sealed partial class HardwareResources
    {
        private readonly Dictionary<string, HardwareCylinderEntry> cylinders =
            new Dictionary<string, HardwareCylinderEntry>(StringComparer.OrdinalIgnoreCase);
        private bool cylindersMigrated;
        internal IEnumerable<HardwareCylinderEntry> CylinderEntries => cylinders.Values.ToArray();

        /// <summary>列出气缸引用的已配置输出，供宿主初始化驱动。</summary>
        public IEnumerable<HardwareEntry> CylinderOutputs()
        {
            return cylinders.Values.SelectMany(item => new[] { item.Definition.OnOutputId, item.Definition.OffOutputId })
                .Distinct(StringComparer.OrdinalIgnoreCase).Select(FindConfiguredOutput).Where(entry => entry != null).ToArray();
        }

        /// <summary>按稳定 ID 查找输出，待配置点位仍可展示和编辑。</summary>
        internal HardwareEntry FindCylinderOutput(string id)
        {
            HardwareEntry entry = Find(id) ?? entries.Values.FirstOrDefault(item => string.Equals(item.Definition.Id, id, StringComparison.OrdinalIgnoreCase));
            return entry?.Device is NP_Output ? entry : null;
        }

        /// <summary>待配置点位不会加入实际气缸动作或初始化。</summary>
        private HardwareEntry FindConfiguredOutput(string id)
        {
            HardwareEntry entry = FindCylinderOutput(id);
            return entry != null && !entry.Definition.ConnectionPending ? entry : null;
        }

        /// <summary>按组合名称获取就绪对象，调用方可直接调用 On 或 Off。</summary>
        public NP_Cylinder GetCylinder(string name)
        {
            HardwareCylinderEntry entry = cylinders.Values.FirstOrDefault(item => string.Equals(item.Definition.Name, name, StringComparison.OrdinalIgnoreCase));
            return RequireReadyCylinder(entry, name);
        }

        /// <summary>按工站键获取就绪对象，显示名称修改后工艺仍使用同一个组合。</summary>
        public NP_Cylinder GetStationCylinder(string key)
        {
            return RequireReadyCylinder(FindStationCylinder(key), key);
        }

        /// <summary>工站预检检查组合存在、连接可用且配置已保存。</summary>
        public bool IsStationCylinderReady(string key)
        {
            return CanDriveCylinder(FindStationCylinder(key)?.Cylinder);
        }

        /// <summary>查询和显示现有工站组合名称。</summary>
        public string StationCylinderName(string key)
        {
            var definition = FindStationCylinder(key)?.Definition;
            return definition == null ? key : DisplayText(definition);
        }

        /// <summary>以气缸 ID 读取操作卡标题。</summary>
        internal static string DisplayText(HardwareCylinderDefinition definition) => FrameworkLanguage.ResourceText("HardwareResources",
            "Cylinder_" + definition.Id, definition.Name, definition.Text);

        /// <summary>找出项目声明的工艺气缸。</summary>
        private HardwareCylinderEntry FindStationCylinder(string key)
        {
            return cylinders.Values.FirstOrDefault(entry => !string.IsNullOrEmpty(entry.Definition.StationKey) &&
                string.Equals(entry.Definition.StationKey, key, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>运行时拒绝缺失、待保存及待重启的组合。</summary>
        private NP_Cylinder RequireReadyCylinder(HardwareCylinderEntry entry, string name)
        {
            if (CanDriveCylinder(entry?.Cylinder)) return entry.Cylinder;
            throw new InvalidOperationException(string.Format(HardwareText.T("cylinderNotReady"), name));
        }

        /// <summary>两路真实输出均就绪才允许动作。</summary>
        private bool CanDriveCylinder(NP_Cylinder cylinder)
        {
            return !SysPara.Simulation && !IsDirty && !RequiresRestart && !ProjectConfiguration.IsBusy &&
                cylinder != null && !cylinder.IsDisposed &&
                cylinder.CyOn_OB1?.IsReady == true && cylinder.CyOff_OB1?.IsReady == true &&
                FlowResourceGate.CanAccess(cylinder.CyOn_OB1) && FlowResourceGate.CanAccess(cylinder.CyOff_OB1);
        }

        /// <summary>按配置 ID 找到气缸组合的当前版本。</summary>
        internal HardwareCylinderEntry FindCylinder(string id)
        {
            return id != null && cylinders.TryGetValue(id, out HardwareCylinderEntry entry) ? entry : null;
        }

        /// <summary>校验后提交一组气缸的内存草稿，复用已有操作对象。</summary>
        internal HardwareCylinderEntry PutCylinder(HardwareCylinderDefinition value)
        {
            RequireIoPageEdit();
            HardwareCylinderDefinition candidate = value.Copy();
            HardwareCylinderEntry current = FindCylinder(candidate.Id);
            candidate.StationKey = current?.Definition.StationKey ?? "";
            var project = CaptureProject();
            project.Cylinders.RemoveAll(item => string.Equals(item.Id, candidate.Id, StringComparison.OrdinalIgnoreCase));
            project.Cylinders.Add(candidate);
            ValidateCylinders(project, false);
            ValidateCylinderOutputs(candidate, project.Resources);
            if (current == null) current = CreateCylinder(candidate);
            current.Definition = candidate;
            current.Card.Tag = candidate;
            NotifyIoPageChange();
            return current;
        }

        /// <summary>删除无固定工站调用的组合，保留物理输出点。</summary>
        internal void DeleteCylinder(string id)
        {
            RequireIoPageEdit();
            HardwareCylinderEntry entry = FindCylinder(id);
            if (entry == null) return;
            if (!string.IsNullOrEmpty(entry.Definition.StationKey))
                throw new InvalidOperationException(HardwareText.T("stationCylinder"));
            cylinders.Remove(id);
            entry.Card.Dispose();
            NotifyIoPageChange();
        }

        /// <summary>输出点仍被组合引用时，提示先修改组合的绑定。</summary>
        private void RequireCylinderOutputUnused(string id)
        {
            string[] names = cylinders.Values.Where(entry => IsCylinderOutput(entry.Definition, id))
                .Select(entry => DisplayText(entry.Definition)).ToArray();
            if (names.Length > 0)
                throw new InvalidOperationException(string.Format(HardwareText.T("cylinderOutputUsed"), string.Join(", ", names)));
        }

        /// <summary>按 ID 判断输出属于组合的任一侧。</summary>
        private static bool IsCylinderOutput(HardwareCylinderDefinition cylinder, string id)
        {
            return string.Equals(cylinder.OnOutputId, id, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(cylinder.OffOutputId, id, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>保留旧文件及所选输出缺失时的编辑入口，主保存要求两路输出均存在。</summary>
        private static void ValidateCylinders(HardwareProject project, bool requireOutputs)
        {
            var ids = new HashSet<string>(project.Resources.Select(resource => resource.Id), StringComparer.OrdinalIgnoreCase);
            var names = new HashSet<string>(project.Resources.Select(resource => resource.Name), StringComparer.OrdinalIgnoreCase);
            var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (project.Cylinders == null) throw new InvalidOperationException(HardwareText.T("invalid"));
            foreach (HardwareCylinderDefinition cylinder in project.Cylinders)
            {
                if (cylinder == null || !Guid.TryParseExact(cylinder.Id, "N", out _) || string.IsNullOrWhiteSpace(cylinder.Name) || cylinder.Name != cylinder.Name.Trim())
                    throw new InvalidOperationException(HardwareText.T("invalid"));
                if (!ids.Add(cylinder.Id) || !names.Add(cylinder.Name) ||
                    !string.IsNullOrEmpty(cylinder.StationKey) && !keys.Add(cylinder.StationKey))
                    throw new InvalidOperationException(HardwareText.T("duplicate") + " " + cylinder.Name);
                if (requireOutputs) ValidateCylinderOutputs(cylinder, project.Resources);
            }
        }

        /// <summary>选择的 ON 和 OFF 必须是两个不同的已有数字输出。</summary>
        private static void ValidateCylinderOutputs(HardwareCylinderDefinition cylinder, IEnumerable<HardwareDefinition> points)
        {
            if (string.Equals(cylinder.OnOutputId, cylinder.OffOutputId, StringComparison.OrdinalIgnoreCase) ||
                !points.Any(point => point.Kind == "Output" && string.Equals(point.Id, cylinder.OnOutputId, StringComparison.OrdinalIgnoreCase)) ||
                !points.Any(point => point.Kind == "Output" && string.Equals(point.Id, cylinder.OffOutputId, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException(string.Format(HardwareText.T("cylinderOutputsInvalid"), cylinder.Name));
        }

        /// <summary>老硬件文件首次载入时迁入项目气缸，解析到现场已经保存的资源 ID。</summary>
        private static void MergeCylinderDefaults(HardwareProject project, HardwareProject defaults)
        {
            if (defaults == null || project.CylindersMigrated || defaults.Cylinders.Count == 0) return;
            foreach (HardwareCylinderDefinition seed in defaults.Cylinders)
            {
                if (project.Cylinders.Any(item => string.Equals(item.StationKey, seed.StationKey, StringComparison.OrdinalIgnoreCase))) continue;
                HardwareCylinderDefinition entry = seed.Copy();
                entry.OnOutputId = FindMigratedOutputId(project, defaults, seed.OnOutputId);
                entry.OffOutputId = FindMigratedOutputId(project, defaults, seed.OffOutputId);
                project.Cylinders.Add(entry);
            }
            project.CylindersMigrated = true;
        }

        /// <summary>已有机器可能预先创建过同用途的点位，迁移时保留它的 ID。</summary>
        private static string FindMigratedOutputId(HardwareProject project, HardwareProject defaults, string templateId)
        {
            string key = defaults.Resources.FirstOrDefault(item => item.Id == templateId)?.StationKey;
            if (string.IsNullOrEmpty(key)) return templateId;
            return project.Resources.FirstOrDefault(item => item.Kind == "Output" && item.StationKey == key)?.Id ?? templateId;
        }

        /// <summary>重载配置时释放旧组合，重新使用恢复后的真实输出。</summary>
        private void ReplaceCylinders(HardwareProject project)
        {
            foreach (HardwareCylinderEntry entry in cylinders.Values) entry.Card.Dispose();
            cylinders.Clear();
            cylindersMigrated = project.CylindersMigrated;
            foreach (HardwareCylinderDefinition definition in project.Cylinders) CreateCylinder(definition);
            BindCylinderOutputs(this, EventArgs.Empty);
        }

        /// <summary>生成一张有独立稳定名称的操作卡，配置过程中不连接设备。</summary>
        private HardwareCylinderEntry CreateCylinder(HardwareCylinderDefinition definition)
        {
            var entry = new HardwareCylinderEntry
            {
                Definition = definition,
                Cylinder = new NP_Cylinder { Name = "cylinder_" + definition.Id + "_dyn", Dock = DockStyle.Fill },
                Card = new GroupBox { Name = "action_" + definition.Id + "_dyn", Tag = definition, Height = 56, Margin = new Padding(5) }
            };
            entry.Cylinder.ManualOperationAllowed = CanOperate;
            entry.Cylinder.OperationAllowed = () => CanDriveCylinder(entry.Cylinder);
            entry.Cylinder.ManualDriven += state => OperationLog.Write("ManualForm", "手动气缸动作：" + entry.Card.Text);
            entry.Card.Controls.Add(entry.Cylinder);
            cylinders.Add(definition.Id, entry);
            return entry;
        }

        /// <summary>由输出 ID 解析并替换绑定，输出改名和放弃修改后保持关联。</summary>
        private void BindCylinderOutputs(object sender, EventArgs args)
        {
            foreach (HardwareCylinderEntry entry in cylinders.Values)
            {
                entry.Cylinder.CyOn_OB1 = FindConfiguredOutput(entry.Definition.OnOutputId)?.Device as NP_Output;
                entry.Cylinder.CyOff_OB1 = FindConfiguredOutput(entry.Definition.OffOutputId)?.Device as NP_Output;
            }
        }
    }
}
