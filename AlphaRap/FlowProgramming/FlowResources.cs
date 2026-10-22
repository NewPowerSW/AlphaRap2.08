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
        /// <summary>显示名称保持业务原值。</summary>
        public override string ToString() { return Label; }
    }

    public sealed class FlowResource
    {
        public string Id;
        public string Label;
        public string Kind;
        public object Instance;
        public List<FlowChoice> Points = new List<FlowChoice>();
    }

    public sealed class FlowPointAxis
    {
        public NP_Motor Motor;
        public string TableName;
        public Dictionary<int, double> Positions = new Dictionary<int, double>();
        public double Low;
        public double High;
    }

    public sealed class FlowTray
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
            List<Form> forms = MiddleLayer.lstForm.OfType<Form>().Concat(Flow_Module.FlowChart_ModuleList).Distinct().ToList();
            foreach (Form form in forms) { catalog.AddControls(form, form.Name); catalog.AddPlcFields(form); }
            foreach (HardForm form in forms.OfType<HardForm>()) catalog.AddHardwareGroups(form);
            foreach (ModuleBaseForm module in ModuleManager.ModuleList.ToArray())
            {
                catalog.AddParameters(module.Name, "Recipe", module.RecipeData);
                catalog.AddParameters(module.Name, "Machine", module.SettingData);
                SystemSetting constants = module as SystemSetting;
                if (constants != null)
                    foreach (string scope in new[] { "Setting", "Recipe" }) catalog.AddConstants(constants, scope);
            }
            foreach (KeyValuePair<string, AbstractDevice> device in DeviceControl.GetFlowDevices())
                catalog.Add("device/" + device.Key, device.Value.DeviceName + " · " + device.Value.GetType().Name, "device", device.Value);
            if (MiddleLayer.VPF != null)
                foreach (KeyValuePair<string, VisionproInterface> station in MiddleLayer.VPF.GetFlowStations())
                    catalog.Add("vision/" + station.Key, station.Value.StationName + " · " + station.Key, "vision", station.Value);
            if (MiddleLayer.DataF != null) catalog.Add("data/Production", "DataForm · Production", "data", MiddleLayer.DataF);
            catalog.AddTrays();
            return catalog;
        }
        private bool includeDraft;
        internal bool IncludesDraft { get { return includeDraft; } }

        /// <summary>登记窗体已有的 PLC 对象字段，不调用构造函数或属性获取器。</summary>
        private void AddPlcFields(Form form)
        {
            for (Type type = form.GetType(); type != null && type != typeof(Form); type = type.BaseType)
                foreach (System.Reflection.FieldInfo field in type.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly))
                    if (field.FieldType == typeof(PLC.INOVANCE)) Add(form.Name + "/" + type.FullName + "/" + field.Name, form.Name + " · " + field.Name, "plc", field.GetValue(form));
        }

        /// <summary>从已提交配方读取全部托盘行列及间距，动态新增托盘自动进入目录。</summary>
        private void AddTrays()
        {
            if (MiddleLayer.PlatF == null) return;
            DataTable table = MiddleLayer.PlatF.RecipeData.Tables["PSet"];
            if (table == null || table.Rows.Count == 0 || !table.Rows[0].HasVersion(DataRowVersion.Original)) return;
            foreach (TrayRowSpec spec in TrayConfig.BuildRows())
            {
                if (!spec.CanRemove) continue;
                string[] columns = { spec.RowNumCol, spec.ColumnNumCol, spec.RowPitchCol, spec.ColumnPitchCol };
                if (columns.Any(column => !table.Columns.Contains(column) || table.Rows[0][column, DataRowVersion.Original] == DBNull.Value)) continue;
                FlowTray tray = new FlowTray { Rows = FlowActions.Integer(table.Rows[0][spec.RowNumCol, DataRowVersion.Original]), Columns = FlowActions.Integer(table.Rows[0][spec.ColumnNumCol, DataRowVersion.Original]),
                    RowPitch = FlowActions.Number(table.Rows[0][spec.RowPitchCol, DataRowVersion.Original]), ColumnPitch = FlowActions.Number(table.Rows[0][spec.ColumnPitchCol, DataRowVersion.Original]) };
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
                Add(key + "axis",entry.Definition.Name,"axis",axis.Motor);
                AddPointTable(key+"points",axis.Points);
                FlowResource point = Items[key+"points/Position"];
                point.Label = entry.Definition.Name + " · " + axis.Points.DataSet1.TableName;
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
                ParameterChoices.Add(new FlowChoice { Id = item.Key, Label = item.Key + " · " + item.Value.GetType().Name });
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
                ParameterChoices.Add(new FlowChoice { Id = key, Label = key + " · " + column.DataType.Name });
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
