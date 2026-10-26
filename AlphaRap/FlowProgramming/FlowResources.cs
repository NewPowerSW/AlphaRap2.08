using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using AlphaRapLibrary;
using NPSDK;
using NPSDK.Motion;

namespace AlphaRap.FlowProgramming
{
    public sealed class FlowChoice
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public Type ValueType { get; set; }
        public string ActionId { get; set; }
        public string FieldName { get; set; }
        public string TargetId { get; set; }
        public bool Advanced { get; set; }
        /// <summary>显示名称保持业务原值。</summary>
        public override string ToString() { return Label; }
    }

    public sealed class FlowResource
    {
        public string Id;
        private string label;
        public string Label
        {
            get
            {
                Control control = Instance as Control;
                bool hardware = Kind == "axis" || Kind == "input" || Kind == "output" || Kind == "cylinder";
                return hardware && control != null && !control.IsDisposed && !string.IsNullOrWhiteSpace(control.Text) ? control.Text : label;
            }
            set { label = value; }
        }
        public string Kind;
        public object Instance;
        public List<FlowChoice> Points = new List<FlowChoice>();
        public List<FlowMethod> Methods = new List<FlowMethod>();
        public FlowResource BoundPoints;
        public string Category
        {
            get
            {
                switch (Kind)
                {
                    case "axis": case "point": return "motion";
                    case "input": return "di";
                    case "output": return "do";
                    case "cylinder": return Kind;
                    case "vision": case "data": case "tray": case "constants": case "code": return Kind;
                    default: return "device";
                }
            }
        }
        public bool Selectable { get { return Kind != "point" && Kind != "code"; } }
    }

    public sealed class FlowPointAxis
    {
        public NP_Motor Motor;
        public string TableName;
        public int RowCount;
        public Dictionary<int, double> Positions = new Dictionary<int, double>();
        public double Low;
        public double High;
    }

    public sealed partial class FlowTray
    {
        public int Rows;
        public int Columns;
        public double RowPitch;
        public double ColumnPitch;
    }

    public sealed class FlowResources
    {
        public string RecipeName { get; private set; }
        public readonly Dictionary<string, FlowResource> Items = new Dictionary<string, FlowResource>();
        public readonly Dictionary<string, object> Parameters = new Dictionary<string, object>();
        public readonly List<FlowChoice> ParameterChoices = new List<FlowChoice>();

        /// <summary>在 UI 线程建立资源和已提交参数快照，不连接设备或执行设备方法。</summary>
        public static FlowResources Capture(bool includeDraft = false)
        {
            FlowResources catalog = new FlowResources { RecipeName = SysPara.RecipeName, includeDraft = includeDraft };
            foreach (KeyValuePair<string, AbstractDevice> device in DeviceControl.GetFlowDevices())
                catalog.Add("device/" + device.Key, device.Value.DeviceName + " · " + device.Value.GetType().Name, "device", device.Value);
            if (MiddleLayer.VPF != null)
                foreach (KeyValuePair<string, VisionproInterface> station in MiddleLayer.VPF.GetFlowStations())
                    catalog.Add("vision/" + station.Key, station.Value.StationName, "vision", station.Value);
            if (MiddleLayer.DataF != null) catalog.Add("data/Production", "DataForm · Production", "data", MiddleLayer.DataF);
            List<Form> forms = MiddleLayer.lstForm.OfType<Form>().Concat(Flow_Module.FlowChart_ModuleList).Distinct().ToList();
            foreach (Form form in forms)
            {
                if (form is IFlowObject && !(form is DataForm)) catalog.Add("module/" + form.GetType().FullName + "/" + form.Name, form.Name, "object", form);
                catalog.AddControls(form, form.Name); catalog.AddObjectFields(form);
            }
            foreach (HardForm form in forms.OfType<HardForm>()) catalog.AddHardwareGroups(form);
            foreach (FlowExternalResources.Entry entry in FlowExternalResources.Capture())
            {
                FlowResource resource = catalog.Add(entry.Id, entry.Name, entry.Kind, entry.Instance);
                if (entry.Points == null) continue;
                catalog.AddPointTable(entry.Id + "/points", entry.Points);
                resource.BoundPoints = catalog.Items.Values.SingleOrDefault(item => item.Kind == "point" && item.Id.StartsWith(entry.Id + "/points/", StringComparison.Ordinal)
                    && ReferenceEquals(((FlowPointAxis)item.Instance).Motor, entry.Instance));
            }
            foreach (ModuleBaseForm module in ModuleManager.ModuleList.ToArray())
            {
                catalog.AddParameters(module.Name, "Recipe", module.RecipeData);
                catalog.AddParameters(module.Name, "Machine", module.SettingData);
                SystemSetting constants = module as SystemSetting;
                if (constants != null)
                    foreach (string scope in new[] { "Setting", "Recipe" })
                    {
                        catalog.AddConstants(constants, scope);
                        catalog.Add("constants/" + scope, scope, "constants", new FlowConstantTable(constants.Constants, scope, includeDraft));
                    }
            }
            catalog.AddTrays();
            catalog.BindMethods();
            return catalog;
        }
        private bool includeDraft;

        /// <summary>发现窗体已有的能力对象字段，不调用构造函数、属性获取器或业务操作。</summary>
        private void AddObjectFields(Form form)
        {
            for (Type type = form.GetType(); type != null && type != typeof(Form); type = type.BaseType)
                foreach (System.Reflection.FieldInfo field in type.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly))
                    if (typeof(IFlowObject).IsAssignableFrom(field.FieldType))
                        AddObjectField(form, type, field);
        }

        /// <summary>登记已有的能力对象字段，不调用构造器、属性获取器或业务方法。</summary>
        private void AddObjectField(Form form, Type type, System.Reflection.FieldInfo field)
        {
            object instance = field.GetValue(form);
            if (form is VPForm && instance is VisionproInterface) return;
            if (instance == null || Items.Values.Any(item => ReferenceEquals(item.Instance, instance))) return;
            Add(form.Name + "/" + type.FullName + "/" + field.Name, form.Name + " · " + field.Name, "object", instance);
        }

        /// <summary>从对象契约读取方法；SDK 旧控件通过轻量适配器复用同一实际实例。</summary>
        private void BindMethods()
        {
            foreach (FlowResource resource in Items.Values)
            {
                IFlowObject provider = resource.Instance as IFlowObject;
                List<FlowMethod> declared = provider == null ? new List<FlowMethod>() : provider.GetFlowMethods(includeDraft).ToList();
                NP_Motor motor = resource.Instance as NP_Motor;
                if (motor != null)
                {
                    List<FlowResource> bound = resource.BoundPoints == null
                        ? Items.Values.Where(item => item.Kind == "point" && ReferenceEquals(((FlowPointAxis)item.Instance).Motor, motor)).ToList()
                        : new List<FlowResource> { resource.BoundPoints };
                    if (bound.Count == 1) resource.BoundPoints = bound[0];
                    provider = new FlowAxisObject(motor, bound);
                }
                if (resource.Kind == "point") provider = new FlowAxisObject(((FlowPointAxis)resource.Instance).Motor, new[] { resource }, true);
                if (resource.Kind == "input" || resource.Kind == "output" || resource.Kind == "cylinder") provider = new FlowIoObject(resource.Instance);
                List<FlowMethod> standard = provider == null || ReferenceEquals(provider, resource.Instance)
                    ? new List<FlowMethod>() : provider.GetFlowMethods(includeDraft).ToList();
                resource.Methods = standard.Concat(declared).ToList();
                if (!(resource.Instance is AbstractDevice)) resource.Methods.AddRange(FlowInheritedMethods.Discover(resource.Instance));
                foreach (FlowMethod method in resource.Methods) method.ValidateDefinition();
                if (resource.Methods.GroupBy(method => method.Id).Any(group => group.Count() > 1)) throw new InvalidOperationException("Duplicate method: " + resource.Id);
            }
        }

        /// <summary>按编辑草稿或已提交配方读取托盘行列与间距，动态托盘自动进入目录。</summary>
        private void AddTrays()
        {
            if (MiddleLayer.PlatF == null) return;
            DataTable table = MiddleLayer.PlatF.RecipeData.Tables["PSet"];
            DataRowVersion version = includeDraft ? DataRowVersion.Current : DataRowVersion.Original;
            if (table == null || table.Rows.Count == 0 || !table.Rows[0].HasVersion(version)) return;
            foreach (TrayRowSpec spec in TrayConfig.BuildRows())
            {
                if (!spec.CanRemove) continue;
                string[] columns = { spec.RowNumCol, spec.ColumnNumCol, spec.RowPitchCol, spec.ColumnPitchCol };
                if (columns.Any(column => !table.Columns.Contains(column) || table.Rows[0][column, version] == DBNull.Value)) continue;
                FlowTray tray = new FlowTray { Rows = FlowActions.Integer(table.Rows[0][spec.RowNumCol, version]), Columns = FlowActions.Integer(table.Rows[0][spec.ColumnNumCol, version]),
                    RowPitch = FlowActions.Number(table.Rows[0][spec.RowPitchCol, version]), ColumnPitch = FlowActions.Number(table.Rows[0][spec.ColumnPitchCol, version]) };
                Add("tray/" + spec.Key, spec.Key, "tray", tray);
            }
        }

        /// <summary>递归发现所有页面中的资源，支持任意 Flow_BaseForm 派生窗体。</summary>
        private void AddControls(Control root, string path)
        {
            foreach (Control control in root.Controls)
            {
                string id = path + "/" + control.Name;
                if (control.Tag is Configuration.HardwareDefinition) continue;
                if (control is NP_Motor) Add(id, control.Name, "axis", control);
                if (control is NP_Input) Add(id, control.Name, "input", control);
                if (control is NP_Output) Add(id, control.Name, "output", control);
                if (control is NP_Cylinder) Add(id, control.Name, "cylinder", control);
                NP_PositionTable points = control as NP_PositionTable;
                if (points != null) AddPointTable(id, points);
                if (!(control is Flow_Chart)) AddControls(control, id);
            }
        }

        /// <summary>动态组合资源使用稳定 ID，内部布局或显示名称变化不改变流程绑定。</summary>
        private void AddHardwareGroups(HardForm form)
        {
            if (form.Resources == null) return;
            foreach (Configuration.HardwareEntry entry in form.Resources.Entries)
            {
                string key = "hardware/" + entry.Definition.Id + "/";
                NP_AxisGroup axis = entry.Group as NP_AxisGroup;
                if (axis == null) { Add(key + entry.Definition.Kind.ToLowerInvariant(), entry.Definition.Name, entry.Definition.Kind.ToLowerInvariant(), entry.Device); continue; }
                FlowResource axisResource = Add(key + "axis",entry.Definition.Name,"axis",axis.Motor);
                AddPointTable(key+"points",axis.Points);
                FlowResource point = Items[key+"points/Position"];
                point.Label = entry.Definition.Name;
                axisResource.BoundPoints = point;
            }
        }

        /// <summary>从 ParameterSet 和 WorkMotor 读取轴与点位表关系，不依赖当前选中行。</summary>
        private void AddPointTable(string id, NP_PositionTable table)
        {
            if (table.DataSet1 == null) return;
            List<PositionColumn> columns = table.ParameterSet.Where(item => item.Axis != null).ToList();
            if (columns.Count == 0 && table.WorkMotor != null)
                columns.Add(new PositionColumn { Axis = table.WorkMotor, Name = table.DataSet1.Columns[0].ColumnName });
            foreach (PositionColumn column in columns)
            {
                if (!table.DataSet1.Columns.Contains(column.Name)) continue;
                FlowPointAxis axis = new FlowPointAxis { Motor = column.Axis, TableName = table.DataSet1.TableName, Low = column.Limit_Low, High = column.Limit_High };
                FlowResource resource = Add(id + "/" + column.Name, column.Axis.Name + " · " + table.DataSet1.TableName + "/" + column.Name, "point", axis);
                DataRow[] rows = table.DataSet1.Rows.Cast<DataRow>().Where(row=>!includeDraft || row.RowState!=DataRowState.Deleted).ToArray();
                axis.RowCount = rows.Count(row => row.HasVersion(includeDraft ? DataRowVersion.Current : DataRowVersion.Original));
                for (int i = 0; i < rows.Length; i++) AddPoint(resource, axis, rows[i], column, i);
            }
        }

        /// <summary>只收录已保存且满足列限位的有限坐标，显示 P1 与内部索引 0 的对应关系。</summary>
        private void AddPoint(FlowResource resource, FlowPointAxis axis, DataRow row, PositionColumn column, int index)
        {
            DataRowVersion version = includeDraft ? DataRowVersion.Current : DataRowVersion.Original;
            if (!row.HasVersion(version)) return;
            object value = row[column.Name, version];
            if (value == DBNull.Value) return;
            double position = Convert.ToDouble(value, CultureInfo.InvariantCulture);
            if (double.IsNaN(position) || double.IsInfinity(position) || position < column.Limit_Low || position > column.Limit_High) return;
            axis.Positions.Add(index, position);
            string annotation = row.Table.Columns.Contains("Annotation") ? Convert.ToString(row["Annotation", version]) : "";
            resource.Points.Add(new FlowChoice { Id = index.ToString(CultureInfo.InvariantCulture), Label = "P" + (index + 1) + " [" + index + "] " + annotation + " = " + position.ToString(CultureInfo.InvariantCulture) });
        }

        /// <summary>按模块、作用域、表、行和列提供有类型的已提交参数。</summary>
        private void AddParameters(string module, string scope, DataSet data)
        {
            foreach (DataTable table in data.Tables)
            {
                if (table.TableName == Configuration.ConstantStore.TableName) continue;
                DataRow[] rows = table.Rows.Cast<DataRow>().Where(row=>!includeDraft || row.RowState!=DataRowState.Deleted).ToArray();
                for (int rowIndex = 0; rowIndex < rows.Length; rowIndex++) AddParameterRow(module, scope, table, rows[rowIndex], rowIndex);
            }
        }

        /// <summary>常量绑定按作用域和键生成稳定引用，排序、删除其他行和切换配方不改变引用。</summary>
        private void AddConstants(SystemSetting form, string scope)
        {
            foreach (KeyValuePair<string, object> item in form.Constants.Snapshot(scope,includeDraft))
            {
                Parameters.Add(item.Key, item.Value);
                string prefix = "Constants/" + scope + "/";
                ParameterChoices.Add(new FlowChoice { Id = item.Key, Label = scope + " / " + Uri.UnescapeDataString(item.Key.Substring(prefix.Length)), ValueType = item.Value.GetType() });
            }
        }

        /// <summary>添加一行参数；空值保留空值语义，不替换为零。</summary>
        private void AddParameterRow(string module, string scope, DataTable table, DataRow row, int rowIndex)
        {
            DataRowVersion version = includeDraft ? DataRowVersion.Current : DataRowVersion.Original;
            if (!row.HasVersion(version)) return;
            foreach (DataColumn column in table.Columns)
            {
                string key = scope + "/" + module + "/" + table.TableName + "/" + rowIndex + "/" + column.ColumnName;
                Parameters.Add(key, row[column, version]);
                ParameterChoices.Add(new FlowChoice { Id = key, Label = scope + " / " + module + " / " + table.TableName + " / " + (rowIndex + 1) + " / " + column.ColumnName, ValueType = column.DataType, Advanced = scope == "Setting" });
            }
        }

        /// <summary>登记实际资源，重复身份立即报告而不选取任意对象。</summary>
        private FlowResource Add(string id, string label, string kind, object instance)
        {
            if (Items.ContainsKey(id)) throw new InvalidOperationException("Duplicate flow resource: " + id);
            FlowResource resource = new FlowResource { Id = id, Label = label, Kind = kind, Instance = instance };
            Items.Add(id, resource);
            return resource;
        }
    }
}
