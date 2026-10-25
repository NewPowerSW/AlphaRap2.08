using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using NPSDK;
using NPSDK.Motion;

namespace AlphaRap.FlowProgramming
{
    /// <summary>外部应用拥有的资源登记；宿主只引用实例，不连接或释放外部设备。</summary>
    public static class FlowExternalResources
    {
        internal sealed class Entry
        {
            internal string Id, Name, Kind;
            internal object Instance;
            internal NP_PositionTable Points;
        }

        private static readonly Dictionary<string, Entry> entries = new Dictionary<string, Entry>(StringComparer.Ordinal);
        private static readonly object sync = new object();

        /// <summary>登记已创建的真实对象；稳定身份可跨显示改名保持，轴可同时提供原点位表。</summary>
        public static void Register(string id, string name, string kind, object instance, NP_PositionTable points = null)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id");
            if (instance == null) throw new ArgumentNullException("instance");
            RequireRegistrationIdle();
            if (!new[] { "axis", "input", "output", "cylinder", "vision", "device", "data" }.Contains(kind)) throw new ArgumentException("kind");
            bool valid = kind == "axis" ? instance is NP_Motor : kind == "input" ? instance is NP_Input
                : kind == "output" ? instance is NP_Output : kind == "cylinder" ? instance is NP_Cylinder : instance is IFlowObject;
            if (!valid) throw new ArgumentException("The instance does not support resource kind: " + kind);
            if (points != null && kind != "axis") throw new ArgumentException("Only axis resources can bind a point table.");
            lock (sync)
            {
                if (entries.ContainsKey(id)) throw new InvalidOperationException(string.Format(FlowCodeText.T("externalDuplicate"), id));
                entries.Add(id, new Entry { Id = "external/" + Uri.EscapeDataString(id), Name = name ?? id, Kind = kind, Instance = instance, Points = points });
            }
        }

        /// <summary>重新创建外部对象后替换同一登记，下一次运行捕获当前实例；运行中禁止替换。</summary>
        public static void Replace(string id, object instance, NP_PositionTable points = null)
        {
            RequireRegistrationIdle();
            lock (sync)
            {
                Entry previous;
                if (!entries.TryGetValue(id, out previous)) throw new KeyNotFoundException(id);
                if (!FlowResourceGate.CanAccess(previous.Instance)) throw new InvalidOperationException(FlowText.T("busy"));
                entries.Remove(id);
                try { Register(id, previous.Name, previous.Kind, instance, points); }
                catch { entries.Add(id, previous); throw; }
            }
        }

        /// <summary>注销前检查全部节点依赖；应用继续拥有对象的释放职责。</summary>
        public static void Unregister(string id)
        {
            RequireRegistrationIdle();
            lock (sync)
            {
                Entry entry;
                if (!entries.TryGetValue(id, out entry)) return;
                FlowPrograms.RequireResourceUnused(entry.Id);
                if (!FlowResourceGate.CanAccess(entry.Instance)) throw new InvalidOperationException(FlowText.T("busy"));
                entries.Remove(id);
            }
        }

        /// <summary>返回登记快照；已释放的 UI 对象不再作为可用资源。</summary>
        internal static Entry[] Capture()
        {
            lock (sync) return entries.Values.Where(item => !(item.Instance is Control) || !((Control)item.Instance).IsDisposed).ToArray();
        }

        /// <summary>宿主初始化登记不依赖登录角色，已显示主窗体后只允许其 UI 线程修改登记。</summary>
        private static void RequireRegistrationIdle()
        {
            if (MiddleLayer.MainF != null && MiddleLayer.MainF.IsHandleCreated && MiddleLayer.MainF.InvokeRequired)
                throw new InvalidOperationException(FlowCodeText.T("registrationThread"));
            if (FlowPrograms.AnyRunning || SysPara.SystemMode != RunMode.IDLE || Flow_Module.Module_GetRunningStatus)
                throw new InvalidOperationException(FlowText.T("busy"));
        }
    }

    /// <summary>代码资源角色的解析、VPP 端口约束和当前配方点位确认。</summary>
    public static class FlowResourceAccess
    {
        /// <summary>为节点建立独立方法元数据，显式点位参数只列其角色轴的绑定表。</summary>
        public static FlowMethod CreateMethod(FlowCodeDefinition definition, FlowStep step, FlowResources resources)
        {
            FlowMethod method = definition.CreateMethod();
            method.Id = "code." + definition.Id;
            method.Inputs.Clear();
            foreach (FlowField source in definition.Inputs)
            {
                FlowField field = new FlowField(source.Name, source.Type, source.DefaultValue) { DisplayName = source.DisplayName,
                    Help = source.Help, Unit = source.Unit, Advanced = source.Advanced, OutputSourceMethod = source.OutputSourceMethod,
                    OutputSourceField = source.OutputSourceField, ResourceSlot = source.ResourceSlot };
                field.Options.Clear();
                field.Options.AddRange(source.Options);
                FlowResource axis = BoundAxis(step, resources, source.ResourceSlot);
                if (axis != null && axis.BoundPoints != null)
                    field.Options.AddRange(axis.BoundPoints.Points.Select(point => new FlowOption { Value = point.Id, Label = point.Label }));
                method.Inputs.Add(field);
            }
            method.ValidateInputs = values => ValidatePoints(method, step, resources, values);
            return method;
        }

        /// <summary>在保存及调用前检查已知点位索引，动态来源在真实调用前再次检查。</summary>
        private static void ValidatePoints(FlowMethod method, FlowStep step, FlowResources resources, IDictionary<string, object> values)
        {
            foreach (FlowField field in method.Inputs.Where(item => !string.IsNullOrEmpty(item.ResourceSlot) && values.ContainsKey(item.Name)))
            {
                FlowResource axis = BoundAxis(step, resources, field.ResourceSlot);
                int index = (int)field.ConvertValue(values[field.Name]);
                if (axis == null || axis.BoundPoints == null || !((FlowPointAxis)axis.BoundPoints.Instance).Positions.ContainsKey(index))
                    throw new InvalidOperationException(FlowText.T("error.pointIndex") + " " + field.Name);
            }
        }

        /// <summary>仅解析声明的轴角色，不通过字段名或显示名称猜测点位来源。</summary>
        private static FlowResource BoundAxis(FlowStep step, FlowResources resources, string slot)
        {
            if (string.IsNullOrEmpty(slot)) return null;
            FlowResourceBinding binding = step.ResourceBindings.SingleOrDefault(item => item.Slot == slot);
            FlowResource axis;
            return binding != null && resources.Items.TryGetValue(binding.Target ?? "", out axis) && axis.Kind == "axis" ? axis : null;
        }

        /// <summary>校验角色指向当前真实对象、其业务端口和已确认的点位表。</summary>
        public static void Validate(FlowCodeDefinition definition, FlowStep step, FlowResources resources)
        {
            definition.ValidateBindings(step);
            foreach (FlowField input in definition.Inputs.Where(field => !string.IsNullOrEmpty(field.ResourceSlot)))
            {
                FlowResource axis = BoundAxis(step, resources, input.ResourceSlot);
                if (axis == null || axis.BoundPoints == null)
                    throw new InvalidOperationException(string.Format(FlowCodeText.T("bindingMissing"), input.ResourceSlot));
            }
            foreach (FlowCodeResourceSlot slot in definition.ResourceSlots)
            {
                FlowResourceBinding binding = step.ResourceBindings.SingleOrDefault(item => item.Slot == slot.Name);
                if (binding == null || string.IsNullOrEmpty(binding.Target)) continue;
                FlowResource resource;
                if (!resources.Items.TryGetValue(binding.Target, out resource) || !Matches(slot.Capability, resource))
                    throw new InvalidOperationException(string.Format(FlowCodeText.T("bindingMissing"), slot.Name));
                foreach (FlowCodeResourceFieldDefinition field in slot.Fields) ValidateField(slot, field, binding, resource);
                if (resource.Kind == "axis" && resource.BoundPoints != null && !PointsConfirmed(binding, resource, resources.RecipeName))
                    throw new InvalidOperationException(string.Format(FlowCodeText.T("pointsChanged"), resource.Label));
            }
        }

        /// <summary>按真实对象能力筛选，不允许将输入当输出或将托盘当轴。</summary>
        public static bool Matches(string capability, FlowResource resource)
        {
            if (resource == null || resource.Kind == "code" || resource.Kind == "point") return false;
            return string.Equals(capability, resource.Kind, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>节点确定时记录本配方点位签名，其他配方已确认的签名仍保留。</summary>
        public static void ConfirmPoints(FlowResourceBinding binding, FlowResource resource, string recipe)
        {
            if (resource == null || resource.Kind != "axis" || resource.BoundPoints == null) { binding.PointTableSignature = null; return; }
            Dictionary<string, string> signatures = ReadSignatures(binding.PointTableSignature);
            signatures[recipe ?? ""] = PointSignature(resource);
            binding.PointTableSignature = string.Join("\n", signatures.OrderBy(item => item.Key, StringComparer.Ordinal)
                .Select(item => Uri.EscapeDataString(item.Key) + "=" + item.Value));
        }

        /// <summary>检查当前配方已确认内容；数值和行序变化均保守要求重新确认。</summary>
        public static bool PointsConfirmed(FlowResourceBinding binding, FlowResource resource, string recipe)
        {
            string signature;
            return ReadSignatures(binding.PointTableSignature).TryGetValue(recipe ?? "", out signature) && signature == PointSignature(resource);
        }

        /// <summary>对所属表、有效行及注释建立稳定签名，不依赖当前选中行或翻译标题。</summary>
        private static string PointSignature(FlowResource resource)
        {
            FlowResource bound = resource.BoundPoints;
            FlowPointAxis points = (FlowPointAxis)bound.Instance;
            StringBuilder value = new StringBuilder(bound.Id).Append('|').Append(points.TableName).Append('|').Append(points.RowCount);
            foreach (KeyValuePair<int, double> point in points.Positions.OrderBy(item => item.Key))
            {
                FlowChoice label = bound.Points.FirstOrDefault(item => item.Id == point.Key.ToString(CultureInfo.InvariantCulture));
                value.Append('\n').Append(point.Key).Append('=').Append(point.Value.ToString("R", CultureInfo.InvariantCulture)).Append('|').Append(label == null ? "" : label.Label);
            }
            using (SHA256 hash = SHA256.Create()) return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(value.ToString()))).Replace("-", "");
        }

        /// <summary>读取按配方保存的签名；损坏记录不作为确认成功。</summary>
        private static Dictionary<string, string> ReadSignatures(string source)
        {
            Dictionary<string, string> values = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (string line in (source ?? "").Split('\n'))
            {
                int separator = line.IndexOf('=');
                if (separator < 0) continue;
                values[Uri.UnescapeDataString(line.Substring(0, separator))] = line.Substring(separator + 1);
            }
            return values;
        }

        /// <summary>确认业务字段映射存在于当前算法，且值类型可以按声明读取。</summary>
        private static void ValidateField(FlowCodeResourceSlot slot, FlowCodeResourceFieldDefinition field, FlowResourceBinding binding, FlowResource resource)
        {
            FlowMethod method = resource.Methods.SingleOrDefault(item => item.Id == "vision.run");
            FlowResourceFieldBinding map = binding.Fields.Single(item => item.Name == field.Name && item.Direction == field.Direction);
            FlowField actual = method == null ? null : (field.Direction == "Input" ? method.Inputs : method.Outputs).SingleOrDefault(item => item.Name == map.Field);
            if (actual == null || !Compatible(actual.Type, field.Type))
                throw new InvalidOperationException(string.Format(FlowCodeText.T("fieldMissing"), slot.Name + "/" + field.Name));
        }

        /// <summary>端口类型一致或均为数值时可绑定，实际数值仍由统一转换严格检查。</summary>
        public static bool Compatible(Type actual, Type expected)
        {
            if (actual == expected || expected.IsAssignableFrom(actual)) return true;
            TypeCode left = Type.GetTypeCode(actual), right = Type.GetTypeCode(expected);
            return !actual.IsEnum && !expected.IsEnum && left >= TypeCode.SByte && left <= TypeCode.Decimal && right >= TypeCode.SByte && right <= TypeCode.Decimal;
        }
    }
}
