using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using AlphaRap.FlowProgramming;

namespace AlphaRap.Configuration
{
    public sealed class ConstantValue
    {
        public string Key;
        public string Type;
        public string Value;
        public string Description;
    }

    public sealed class ConstantStore
    {
        public const string TableName = "DesignerConstants";
        private readonly SystemSetting owner;
        public static readonly string[] Types = { "Boolean", "Integer", "Number", "Text" };
        public static readonly string[] ExampleKeys = { "DisScann", "UseTryRun", "DisVision", "DisMes", "DisDoor", "DisRecheck", "DisBuzzer", "Dis3D" };

        /// <summary>使用系统设置模块已有的 SettingData 和 RecipeData，不维护第二套配方目录。</summary>
        public ConstantStore(SystemSetting form) { owner = form; }

        /// <summary>建立固定结构，数值以不受界面语言影响的文本保存。</summary>
        public static DataTable CreateTable()
        {
            DataTable table = new DataTable(TableName);
            foreach (string name in new[] { "Key", "Type", "Value", "Description" }) table.Columns.Add(name, typeof(string));
            return table;
        }

        /// <summary>读取作用域对应的表，Setting 不随当前配方改变。</summary>
        public DataTable Table(string scope)
        {
            if (scope == "Setting") return owner.SettingData.Tables[TableName];
            if (scope == "Recipe") return owner.RecipeData.Tables[TableName];
            throw new ArgumentException("Unknown constant scope: " + scope);
        }

        /// <summary>仅首次迁移现有开关；以后删除的示例不会在启动时重新出现。</summary>
        public void InitializeExamples()
        {
            DataRow marker = owner.SettingData.Tables["MSet"].Rows[0];
            if (!marker.IsNull("ConstantsInitialized") && Convert.ToBoolean(marker["ConstantsInitialized"])) return;
            DataTable table = Table("Setting");
            foreach (string key in ExampleKeys)
            {
                if (Find(table, key) != null) continue;
                bool legacy = IsLegacyKey(key) && Convert.ToBoolean(owner.SettingData.Tables["PSet"].Rows[0][key] == DBNull.Value ? false : owner.SettingData.Tables["PSet"].Rows[0][key]);
                table.Rows.Add(key, "Boolean", legacy.ToString(), "");
            }
            marker["ConstantsInitialized"] = true;
            owner.SettingData.AcceptChanges();
        }

        /// <summary>按业务键查询，忽略基础模块在空表中生成的占位行。</summary>
        public static DataRow Find(DataTable table, string key)
        {
            return table.Rows.Cast<DataRow>().FirstOrDefault(row => row.RowState != DataRowState.Deleted && Convert.ToString(row["Key"]) == key);
        }

        /// <summary>新增或修改前完成类型、唯一键与引用校验，然后一次写入草稿。</summary>
        public void Put(string scope, string oldKey, ConstantValue value)
        {
            FlowPrograms.RequireIdle();
            ValidateValue(value);
            DataTable table = Table(scope);
            DataRow row = oldKey == null ? null : Find(table, oldKey);
            if (oldKey != null && row == null) throw new InvalidOperationException(ConstantText.T("missing"));
            DataRow duplicate = Find(table, value.Key);
            if (duplicate != null && duplicate != row) throw new InvalidOperationException(ConstantText.T("duplicate"));
            if (scope == "Setting" && IsLegacyKey(oldKey) && (value.Key != oldKey || value.Type != "Boolean")) throw new InvalidOperationException(ConstantText.T("legacy"));
            if (row != null && (oldKey != value.Key || Convert.ToString(row["Type"]) != value.Type)) FlowPrograms.RequireParameterUnused(Reference(scope, oldKey));
            object parsed = Parse(value.Type, value.Value);
            string canonical = parsed is double ? ((double)parsed).ToString("R", CultureInfo.InvariantCulture) : Convert.ToString(parsed, CultureInfo.InvariantCulture);
            if (row == null) row = table.Rows.Add(value.Key, value.Type, canonical, value.Description ?? "");
            else row.ItemArray = new object[] { value.Key, value.Type, canonical, value.Description ?? "" };
            if (scope == "Setting" && IsLegacyKey(value.Key)) owner.SettingData.Tables["PSet"].Rows[0][value.Key] = parsed;
        }

        /// <summary>删除前检查旧接口兼容和所有流程参数引用，不按表格排序后的行号定位。</summary>
        public void Delete(string scope, string key)
        {
            FlowPrograms.RequireIdle();
            if (scope == "Setting" && IsLegacyKey(key)) throw new InvalidOperationException(ConstantText.T("legacy"));
            DataRow row = Find(Table(scope), key);
            if (row == null) throw new InvalidOperationException(ConstantText.T("missing"));
            FlowPrograms.RequireParameterUnused(Reference(scope, key));
            row.Delete();
        }

        /// <summary>检查当前草稿全部业务行，空表占位行不作为常量。</summary>
        public void Validate(string scope)
        {
            HashSet<string> keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (DataRow row in Table(scope).Rows)
            {
                if (row.RowState == DataRowState.Deleted || string.IsNullOrWhiteSpace(Convert.ToString(row["Key"]))) continue;
                ConstantValue value = Read(row, DataRowVersion.Current);
                ValidateValue(value);
                if (!keys.Add(value.Key)) throw new InvalidOperationException(ConstantText.T("duplicate"));
            }
        }

        /// <summary>只在完整文件落盘后提交版本，配方常量替换当前 ProductManagerForm 文件中的同命名空间表。</summary>
        public void Save(string scope)
        {
            FlowPrograms.RequireIdle();
            Validate(scope);
            string path = scope == "Setting" ? Path.Combine(AlphaRapLibrary.ModuleManager.SettingDataDirectory, owner.Text + ".xml") : SysPara.FilePath;
            if (scope == "Setting") AtomicWrite(path, owner.SettingData.WriteXml);
            else
            {
                if (!File.Exists(path)) throw new InvalidOperationException(ConstantText.T("recipeRequired"));
                XDocument document = XDocument.Load(path, LoadOptions.PreserveWhitespace);
                XName name = XName.Get(TableName, Table(scope).Namespace);
                document.Root.Elements(name).Remove();
                DataSet replacement = new DataSet(); replacement.Tables.Add(Table(scope).Copy());
                document.Root.Add(XElement.Parse(replacement.GetXml()).Elements(name));
                AtomicWrite(path, document.Save);
            }
            if (scope == "Setting") owner.SettingData.AcceptChanges(); else Table(scope).AcceptChanges();
            OperationLog.Write("Constants", "Save " + scope + " constants: " + path);
        }

        /// <summary>以临时文件原子替换，保留上次文件备份；失败不提交内存草稿。</summary>
        private static void AtomicWrite(string path, Action<string> write)
        {
            string full = Path.GetFullPath(path);
            Directory.CreateDirectory(Path.GetDirectoryName(full));
            string temporary = full + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                write(temporary);
                if (File.Exists(full)) File.Replace(temporary, full, full + ".bak"); else File.Move(temporary, full);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }

        /// <summary>运行快照只读取已保存版本，稳定引用使用作用域和键，不使用行索引。</summary>
        public IEnumerable<KeyValuePair<string, object>> Committed(string scope) { return Snapshot(scope,false); }

        /// <summary>工程校验可读取草稿，运行快照只读取已保存版本。</summary>
        public IEnumerable<KeyValuePair<string, object>> Snapshot(string scope,bool includeDraft)
        {
            foreach (DataRow row in Table(scope).Rows)
            {
                DataRowVersion version = includeDraft ? DataRowVersion.Current : DataRowVersion.Original;
                if (!row.HasVersion(version)) continue;
                ConstantValue value = Read(row, version);
                if (string.IsNullOrWhiteSpace(value.Key)) continue;
                yield return new KeyValuePair<string, object>(Reference(scope, value.Key), Parse(value.Type, value.Value));
            }
        }

        /// <summary>读取表的一份明确版本，不将删除或未提交值混入运行。</summary>
        public static ConstantValue Read(DataRow row, DataRowVersion version)
        {
            return new ConstantValue { Key = Convert.ToString(row["Key", version]), Type = Convert.ToString(row["Type", version]), Value = Convert.ToString(row["Value", version]), Description = Convert.ToString(row["Description", version]) };
        }

        /// <summary>两项历史参数保留键、布尔类型和 PSet 兼容入口。</summary>
        private static bool IsLegacyKey(string key) { return key == "DisScann" || key == "UseTryRun"; }

        /// <summary>形成不受翻译、表格排序和配方名称影响的绑定键。</summary>
        public static string Reference(string scope, string key) { return "Constants/" + scope + "/" + Uri.EscapeDataString(key); }

        /// <summary>校验可读键和有限、有类型的数值。</summary>
        private static void ValidateValue(ConstantValue value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.Key) || value.Key.Trim() != value.Key || value.Key.Any(char.IsControl)) throw new InvalidOperationException(ConstantText.T("keyRequired"));
            Parse(value.Type, value.Value);
        }

        /// <summary>按稳定类型代码解析值，拒绝 NaN、无穷及依赖地区的小数格式。</summary>
        public static object Parse(string type, string text)
        {
            bool flag; long integer; double number;
            if (type == "Text") return text ?? "";
            if (type == "Boolean" && bool.TryParse(text, out flag)) return flag;
            if (type == "Integer" && long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out integer)) return integer;
            if (type == "Number" && double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out number) && !double.IsNaN(number) && !double.IsInfinity(number)) return number;
            throw new InvalidOperationException(ConstantText.T("invalidValue"));
        }
    }
}
