using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace AlphaRap.FlowProgramming
{
    public sealed class ProductionField
    {
        public string Name { get; set; }
        public string Type { get; set; } = "Text";
    }

    public static class FlowValue
    {
        /// <summary>将标量、字节和集合转换为明确的落盘值。</summary>
        public static string Text(object value)
        {
            if (value == null || value == DBNull.Value) return "";
            byte[] bytes = value as byte[];
            if (bytes != null) return BitConverter.ToString(bytes);
            IEnumerable list = value as IEnumerable;
            if (list != null && !(value is string)) return string.Join(";", list.Cast<object>().Select(Text));
            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }
    }
}

namespace AlphaRap
{
    public partial class DataForm
    {
        private readonly object productionSync = new object();
        private List<FlowProgramming.ProductionField> productionFields;

        /// <summary>生产表定义与 DataForm 的机器配置保存在同一目录。</summary>
        private static string ProductionSchemaPath
        {
            get { return Path.Combine(string.IsNullOrEmpty(SysPara.SettingDataDirectory) ? @".\ModuleData\SettingData" : SysPara.SettingDataDirectory, "DataForm.Production.xml"); }
        }

        /// <summary>读取生产表字段；兼容已有首页列定义，调用方得到独立快照。</summary>
        public List<FlowProgramming.ProductionField> GetProductionSchema()
        {
            lock (productionSync)
            {
                if (productionFields == null) productionFields = ReadProductionSchema();
                return productionFields.Select(field => new FlowProgramming.ProductionField { Name = field.Name, Type = field.Type }).ToList();
            }
        }

        /// <summary>载入机器级生产表定义，损坏时报告错误，禁止静默覆盖。</summary>
        private static List<FlowProgramming.ProductionField> ReadProductionSchema()
        {
            if (File.Exists(ProductionSchemaPath))
                using (FileStream stream = File.OpenRead(ProductionSchemaPath))
                    return (List<FlowProgramming.ProductionField>)new XmlSerializer(typeof(List<FlowProgramming.ProductionField>)).Deserialize(stream);
            string[] existing = ProductionDataStore.GetColumns();
            if (existing.Length == 0) existing = new[] { "Barcode", "Result" };
            return existing.Where(name => !IsProductionMetadata(name)).Select(name => new FlowProgramming.ProductionField { Name = name }).ToList();
        }

        /// <summary>获取保存动作需要绑定的业务字段，时间与记录身份由框架填写。</summary>
        public string[] GetProductionFields() { return GetProductionSchema().Select(field => field.Name).ToArray(); }

        /// <summary>校验并原子保存生产表定义，字段键保持业务原值。</summary>
        public void SaveProductionSchema(List<FlowProgramming.ProductionField> fields)
        {
            if (fields.Count == 0) throw new InvalidDataException("At least one production field is required.");
            HashSet<string> names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (FlowProgramming.ProductionField field in fields)
            {
                if (string.IsNullOrWhiteSpace(field.Name) || field.Name.IndexOfAny(new[] { '\r', '\n' }) >= 0 || !names.Add(field.Name) || IsProductionMetadata(field.Name))
                    throw new InvalidDataException("Invalid, duplicate or reserved field: " + field.Name);
                if (!new[] { "Text", "Number", "Boolean" }.Contains(field.Type)) throw new InvalidDataException("Unsupported field type: " + field.Type);
            }
            lock (productionSync)
            {
                string path = ProductionSchemaPath;
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
                string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
                try
                {
                    using (FileStream stream = File.Create(temporary)) new XmlSerializer(typeof(List<FlowProgramming.ProductionField>)).Serialize(stream, fields);
                    if (File.Exists(path)) File.Replace(temporary, path, path + ".bak"); else File.Move(temporary, path);
                    productionFields = fields.Select(field => new FlowProgramming.ProductionField { Name = field.Name, Type = field.Type }).ToList();
                }
                finally { if (File.Exists(temporary)) File.Delete(temporary); }
            }
        }

        /// <summary>类型校验后保存具名生产记录；只有持久化成功才返回文件路径。</summary>
        public string SaveProductionRecord(string recordId, string cycleId, string model, IDictionary<string, object> values, bool simulated = false)
        {
            Dictionary<string, string> fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (FlowProgramming.ProductionField field in GetProductionSchema())
            {
                object value;
                if (!values.TryGetValue(field.Name, out value)) throw new InvalidDataException("Missing production field: " + field.Name);
                if (field.Type == "Number") value = FlowProgramming.FlowActions.Number(value);
                if (field.Type == "Boolean") value = FlowProgramming.FlowActions.Boolean(value);
                fields.Add(field.Name, FlowProgramming.FlowValue.Text(value));
            }
            fields.Add("RecordId", recordId);
            fields.Add("CycleId", cycleId);
            fields.Add("ModelName", model);
            fields.Add("Simulated", simulated.ToString());
            DateTime now = DateTime.Now;
            string error = ProductionDataStore.AppendNamed(now, fields, recordId);
            if (!string.IsNullOrEmpty(error)) throw new IOException(error);
            return Path.GetFullPath(ProductionDataStore.DayFile(now));
        }

        /// <summary>框架保留时间、记录身份、产品关联及型号字段。</summary>
        private static bool IsProductionMetadata(string name)
        {
            return new[] { "Time", "RecordId", "CycleId", "ModelName", "Simulated" }.Contains(name, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>在 DataForm 打开字段配置，运行期间禁止修改正在使用的表结构。</summary>
        private void FlowProductionFieldsClick(object sender, EventArgs e)
        {
            if (FlowProgramming.FlowPrograms.AnyRunning) { System.Windows.Forms.MessageBox.Show(FlowProgramming.FlowText.T("busy")); return; }
            using (FlowProgramming.FlowSchemaForm dialog = new FlowProgramming.FlowSchemaForm(this)) dialog.ShowDialog(this);
        }
    }
}
