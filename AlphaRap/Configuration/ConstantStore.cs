using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NPSDK;
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
        private readonly int ownerThread;
        private readonly object flowSync = new object();
        private readonly Dictionary<string, FlowValue> flowValues = new Dictionary<string, FlowValue>();
        private sealed class FlowValue { internal DataRow Row; internal object Value; }
        public static readonly string[] Types = { "Boolean", "Integer", "Number", "Text" };
        public static readonly string[] ExampleKeys = { "DisScann", "UseTryRun", "DisVision", "DisMes", "DisDoor", "DisRecheck", "DisBuzzer", "Dis3D" };

        /// <summary>使用系统设置模块已有的 SettingData 和 RecipeData，不维护第二套配方目录。</summary>
        public ConstantStore(SystemSetting form)
        {
            owner = form; ownerThread = Thread.CurrentThread.ManagedThreadId;
            foreach (string scope in new[] { "Setting", "Recipe" })
            {
                Table(scope).RowChanged += ClearPublishedRow;
                Table(scope).TableCleared += ClearPublishedTable;
            }
        }

        /// <summary>提交或放弃数据行时清理流程发布值，读取重新跟随表的已保存版本。</summary>
        private void ClearPublishedRow(object sender, DataRowChangeEventArgs args)
        {
            if (args.Action != DataRowAction.Commit && args.Action != DataRowAction.Rollback) return;
            lock (flowSync)
                foreach (string key in flowValues.Where(item => ReferenceEquals(item.Value.Row,args.Row)).Select(item => item.Key).ToArray()) flowValues.Remove(key);
        }

        /// <summary>配方或设置重新加载时清理该表的流程发布值，不影响另一作用域。</summary>
        private void ClearPublishedTable(object sender, DataTableClearEventArgs args)
        {
            lock (flowSync)
                foreach (string key in flowValues.Where(item => ReferenceEquals(item.Value.Row.Table,sender)).Select(item => item.Key).ToArray()) flowValues.Remove(key);
        }

        /// <summary>读取流程已经写入真实表的值，普通页面的未保存编辑不会被发布。</summary>
        internal bool TryReadFlowValue(string scope,string key,out object value)
        {
            lock (flowSync)
            {
                FlowValue published;
                if (flowValues.TryGetValue(Reference(scope,key),out published)) { value=published.Value; return true; }
                value=null; return false;
            }
        }

        /// <summary>在界面线程设置表值并发布到当前流程；取消、失效批次或配方变更不修改数据。</summary>
        internal Task<object> SetFromFlowAsync(string scope,string key,object value,Type expectedType,FlowCall call,string recipePath)
        {
            TaskCompletionSource<object> completion=new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            Action apply=()=>{
                try
                {
                    call.Token.ThrowIfCancellationRequested();
                    if (owner.IsDisposed || call.Context.Hub.Epoch!=call.Context.RunId) throw new OperationCanceledException();
                    if (scope=="Recipe" && !string.Equals(recipePath,SysPara.FilePath,StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException(FlowText.T("constantScopeChanged"));
                    object actual=SetTableValue(scope,key,value,expectedType);
                    call.Context.Hub.SetParameter(call.Context.RunId,Reference(scope,key),actual);
                    completion.TrySetResult(actual);
                }
                catch (OperationCanceledException) { completion.TrySetCanceled(); }
                catch (Exception error) { completion.TrySetException(error); }
            };
            if (Thread.CurrentThread.ManagedThreadId==ownerThread) { apply(); return completion.Task; }
            Control dispatcher=MiddleLayer.MainF!=null && !MiddleLayer.MainF.IsDisposed && MiddleLayer.MainF.IsHandleCreated ? (Control)MiddleLayer.MainF : owner;
            if (dispatcher.IsDisposed || !dispatcher.IsHandleCreated) return Task.FromException<object>(new InvalidOperationException(FlowText.T("constantUiUnavailable")));
            try { dispatcher.BeginInvoke(apply); }
            catch (InvalidOperationException error) { completion.TrySetException(error); }
            return completion.Task;
        }

        /// <summary>只修改已有行的 Value 和兼容开关，保留键、类型、备注与保存前 Original 版本。</summary>
        private object SetTableValue(string scope,string key,object value,Type expectedType)
        {
            DataRow row=Find(Table(scope),key);
            if (row==null) throw new InvalidOperationException(ConstantText.T("missing"));
            object parsed=Parse(Convert.ToString(row["Type"]),Convert.ToString(value,CultureInfo.InvariantCulture));
            if (parsed.GetType()!=expectedType) throw new InvalidOperationException(FlowText.T("constantTypeChanged"));
            row["Value"]=parsed is double ? ((double)parsed).ToString("R",CultureInfo.InvariantCulture) : Convert.ToString(parsed,CultureInfo.InvariantCulture);
            if (scope=="Setting" && IsLegacyKey(key)) owner.SettingData.Tables["PSet"].Rows[0][key]=parsed;
            lock (flowSync) flowValues[Reference(scope,key)]=new FlowValue { Row=row,Value=parsed };
            return parsed;
        }

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

        /// <summary>读取已保存值及流程明确写入表的发布值，稳定引用不使用行索引。</summary>
        public IEnumerable<KeyValuePair<string, object>> Committed(string scope) { return Snapshot(scope,false); }

        /// <summary>工程校验读取草稿；流程使用保存值或流程写入的表值，忽略普通页面编辑草稿。</summary>
        public IEnumerable<KeyValuePair<string, object>> Snapshot(string scope,bool includeDraft)
        {
            foreach (DataRow row in Table(scope).Rows)
            {
                DataRowVersion version = includeDraft ? DataRowVersion.Current : DataRowVersion.Original;
                if (!row.HasVersion(version)) continue;
                ConstantValue value = Read(row, version);
                if (string.IsNullOrWhiteSpace(value.Key)) continue;
                object published;
                object result=!includeDraft && TryReadFlowValue(scope,value.Key,out published) ? published : Parse(value.Type,value.Value);
                yield return new KeyValuePair<string, object>(Reference(scope, value.Key), result);
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
