using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using AlphaRapLibrary;

namespace AlphaRap
{
    /// <summary>托盘类型（对应配方表 PSet 里的列前缀）。</summary>
    public enum TrayKind
    {
        /// <summary>上料盘 load Tray</summary>
        Load = 0,
        /// <summary>下料盘 A Unload TrayA</summary>
        UnloadA,
        /// <summary>下料盘 B Unload TrayB</summary>
        UnloadB,
        /// <summary>返修盘</summary>
        Reject,
        /// <summary>NG 盘</summary>
        NG
    }

    /// <summary>一组托盘参数：行数 / 列数 / 行距 / 列距。</summary>
    public struct TrayLayout
    {
        /// <summary>行数</summary>
        public int Rows;
        /// <summary>列数</summary>
        public int Columns;
        /// <summary>行距</summary>
        public double RowPitch;
        /// <summary>列距</summary>
        public double ColumnPitch;

        /// <summary>四项全为 0（配方里没有这一组，或还没设过值）。</summary>
        public bool IsEmpty
        {
            get { return Rows == 0 && Columns == 0 && RowPitch == 0 && ColumnPitch == 0; }
        }

        public override string ToString()
        {
            return string.Format("{0} x {1} @ {2} / {3}", Rows, Columns, RowPitch, ColumnPitch);
        }
    }

    /// <summary>
    /// 表格里的一行 = 一组托盘。四个列名可能为 null（配方里少了那一列时该格不可编辑）。
    /// </summary>
    public sealed class TrayRowSpec
    {
        /// <summary>稳定标识，例如 Tray / UnloadTray2。</summary>
        public string Key;
        /// <summary>列名前缀，例如 UnloadTray。</summary>
        public string BaseKey;
        /// <summary>同名前缀的序号，1 起。</summary>
        public int Index;

        public string TitleZh;
        public string TitleEn;
        public string TitleEs;

        public string RowNumCol;
        public string ColumnNumCol;
        public string RowPitchCol;
        public string ColumnPitchCol;

        /// <summary>四列齐全才允许整组删除（缺列说明这行不是本页建的）。</summary>
        public bool CanRemove
        {
            get
            {
                return RowNumCol != null && ColumnNumCol != null &&
                       RowPitchCol != null && ColumnPitchCol != null;
            }
        }
    }

    /// <summary>托盘参数里的某一项（与配方列后缀一一对应，也和界面表头同名）。</summary>
    public enum TrayField
    {
        /// <summary>行数 RowNum</summary>
        Rows = 0,
        /// <summary>列数 ColumnNum</summary>
        Columns,
        /// <summary>行距 Rowdis</summary>
        RowPitch,
        /// <summary>列距 ColumnDis</summary>
        ColumnPitch
    }

    /// <summary>表格里的一行（已取值）：行标识 + 显示名 + 参数。</summary>
    public sealed class TrayTableRow
    {
        /// <summary>行标识，与界面上那一行对应（Tray / UnloadTray2 / 动态添加的 FooTray）。</summary>
        public string Key;
        public string TitleZh;
        public string TitleEn;
        public string TitleEs;
        /// <summary>行数 / 列数 / 行距 / 列距。</summary>
        public TrayLayout Layout;
    }

    /// <summary>
    /// 托盘参数的读取入口 + 表格行的发现 + "快速添加 / 删除托盘"。
    ///
    /// 数据源 = 模块 PlatFormSetting 的配方表 <c>RecipeData.PSet</c>：**列名就是字段名**，第 0 行就是值。
    /// 一组托盘 = 四个列 <c>&lt;前缀&gt;(RowNum|ColumnNum|Rowdis|ColumnDis)</c>，序号 &gt;1 时加数字后缀
    /// （UnloadTrayRowNum / UnloadTrayRowNum2）—— 所以"加一个托盘"就是建这 4 个列。
    ///
    /// 其它模块要读参数请用 <see cref="Get(TrayKind)"/> / <see cref="Get(string,int)"/> / <see cref="Get3DLimit"/>，
    /// 不要再自己去翻 DataTable。
    /// </summary>
    public static class TrayConfig
    {
        /// <summary>配方表名。</summary>
        public const string TableName = "PSet";

        /// <summary>行数后缀。</summary>
        public const string SufRowNum = "RowNum";
        /// <summary>列数后缀。</summary>
        public const string SufColNum = "ColumnNum";
        /// <summary>行距后缀。</summary>
        public const string SufRowDis = "Rowdis";
        /// <summary>列距后缀。</summary>
        public const string SufColDis = "ColumnDis";

        private static readonly string[] Suffixes = { SufColDis, SufRowDis, SufColNum, SufRowNum };

        /// <summary>3D 高度限制的两个列名（不属于"四个一组"的托盘模式，单独一组）。</summary>
        public const string Col3DUp = "Limit3D_Up";
        public const string Col3DDown = "Limit3D_Down";

        // ---------------------------------------------------------------- 读数

        /// <summary>按列名前缀拼出某组的某个列名（序号 1 不带后缀数字，与现有命名一致）。</summary>
        public static string Col(string baseKey, int index, string suffix)
        {
            return baseKey + suffix + (index > 1 ? index.ToString() : "");
        }

        /// <summary>取指定托盘的行/列/行距/列距；缺列或没值一律返回 0，不抛异常、不报报警。</summary>
        public static TrayLayout Get(string baseKey, int index)
        {
            TrayLayout l = new TrayLayout();
            if (string.IsNullOrEmpty(baseKey)) return l;

            l.Rows = (int)Math.Round(Num(Col(baseKey, index, SufRowNum)));
            l.Columns = (int)Math.Round(Num(Col(baseKey, index, SufColNum)));
            l.RowPitch = Num(Col(baseKey, index, SufRowDis));
            l.ColumnPitch = Num(Col(baseKey, index, SufColDis));
            return l;
        }

        /// <summary>取指定托盘类型的参数。</summary>
        public static TrayLayout Get(TrayKind kind)
        {
            switch (kind)
            {
                case TrayKind.Load: return Get("Tray", 1);
                case TrayKind.UnloadA: return Get("UnloadTray", 1);
                case TrayKind.UnloadB: return Get("UnloadTray", 2);
                case TrayKind.Reject: return Get("RejectTray", 1);
                case TrayKind.NG: return Get("NGTray", 1);
                default: return new TrayLayout();
            }
        }

        /// <summary>
        /// 一条语句读整张托盘表（含值）：界面上有几行就返回几行，**运行时新加的托盘也在里面**。
        /// 只要某一组参数就用 <see cref="Get(string)"/>。
        /// </summary>
        public static List<TrayTableRow> GetTable()
        {
            List<TrayTableRow> list = new List<TrayTableRow>();
            List<TrayRowSpec> specs = BuildRows();
            for (int i = 0; i < specs.Count; i++)
            {
                TrayTableRow r = new TrayTableRow();
                r.Key = specs[i].Key;
                r.TitleZh = specs[i].TitleZh;
                r.TitleEn = specs[i].TitleEn;
                r.TitleEs = specs[i].TitleEs;
                r.Layout = Get(specs[i].BaseKey, specs[i].Index);
                list.Add(r);
            }
            return list;
        }

        /// <summary>按行标识读一组参数（"Tray" / "UnloadTray2" / 动态添加的 "FooTray"）；没有这一行返回空。</summary>
        public static TrayLayout Get(string key)
        {
            TrayLayout l;
            return TryGet(key, out l) ? l : new TrayLayout();
        }

        /// <summary>按行标识读一组参数；行不存在返回 false。</summary>
        public static bool TryGet(string key, out TrayLayout layout)
        {
            layout = new TrayLayout();
            TrayRowSpec spec = Find(key);
            if (spec == null) return false;
            layout = Get(spec.BaseKey, spec.Index);
            return true;
        }

        /// <summary>参数项 → 配方列后缀（与界面表头、配方列名同字面）。</summary>
        public static string SuffixOf(TrayField field)
        {
            switch (field)
            {
                case TrayField.Rows: return SufRowNum;
                case TrayField.Columns: return SufColNum;
                case TrayField.RowPitch: return SufRowDis;
                default: return SufColDis;
            }
        }

        /// <summary>
        /// **通过「托盘名 + 其中一项」直接取值**，例如：
        /// <c>TrayConfig.Get("UnloadTray2", TrayField.Rows)</c> → 下料盘 B 的行数。
        /// 名字就是界面上 TrayName 那一列的值（Tray / UnloadTray / UnloadTray2 / RejectTray / NGTray / 动态加的托盘）；
        /// 没有这个托盘或这一项时返回 0。要区分"没有"请用 <see cref="TryGet(string,TrayField,out double)"/>。
        /// </summary>
        public static double Get(string name, TrayField field)
        {
            double v;
            return TryGet(name, field, out v) ? v : 0;
        }

        /// <summary>同 <see cref="Get(string,TrayField)"/>，但能区分"没有这个托盘 / 没有这一项"。</summary>
        public static bool TryGet(string name, TrayField field, out double value)
        {
            value = 0;
            TrayRowSpec spec = Find(name);
            if (spec == null) return false;

            string column;
            switch (field)
            {
                case TrayField.Rows: column = spec.RowNumCol; break;
                case TrayField.Columns: column = spec.ColumnNumCol; break;
                case TrayField.RowPitch: column = spec.RowPitchCol; break;
                default: column = spec.ColumnPitchCol; break;
            }
            if (string.IsNullOrEmpty(column)) return false;

            value = Num(column);
            return true;
        }

        /// <summary>
        /// 通过「托盘名 + 列名后缀」取值，后缀同界面表头：<c>RowNum / ColumnNum / Rowdis / ColumnDis</c>。
        /// 例：<c>TrayConfig.Get("Tray", TrayConfig.SufColDis)</c>。
        /// </summary>
        public static double Get(string name, string columnSuffix)
        {
            TrayRowSpec spec = Find(name);
            if (spec == null || string.IsNullOrEmpty(columnSuffix)) return 0;

            string column = spec.BaseKey + columnSuffix.Trim() + (spec.Index > 1 ? spec.Index.ToString() : "");
            return Num(column);
        }

        /// <summary>按行标识（= 界面 TrayName 列的值）找一行定义；不区分大小写。</summary>
        private static TrayRowSpec Find(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            name = name.Trim();

            List<TrayRowSpec> specs = BuildRows();
            for (int i = 0; i < specs.Count; i++)
                if (string.Equals(specs[i].Key, name, StringComparison.OrdinalIgnoreCase)) return specs[i];
            return null;
        }

        /// <summary>3D 高度上/下限；列不存在时返回 false 并给 0。</summary>
        public static bool Get3DLimit(out double up, out double down)
        {
            DataTable t = PSet();
            up = 0;
            down = 0;
            if (t == null) return false;
            bool hasUp = t.Columns.Contains(Col3DUp);
            bool hasDown = t.Columns.Contains(Col3DDown);
            if (!hasUp && !hasDown) return false;
            up = Num(Col3DUp);
            down = Num(Col3DDown);
            return true;
        }

        /// <summary>读一个数值列；列不存在 / 没行 / 空值 / 解析失败都返回 0。</summary>
        public static double Num(string column)
        {
            DataTable t = PSet();
            if (t == null || string.IsNullOrEmpty(column)) return 0;
            try
            {
                if (t.Rows.Count == 0 || !t.Columns.Contains(column)) return 0;
                object v = t.Rows[0][column];
                if (v == null || v == DBNull.Value) return 0;
                return ToDouble(Convert.ToString(v), 0);
            }
            catch { return 0; }
        }

        /// <summary>字符串转数字（当前区域优先，再退回不变区域）。</summary>
        public static double ToDouble(string s, double fallback)
        {
            double d;
            if (double.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, out d)) return d;
            if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out d)) return d;
            return fallback;
        }

        /// <summary>数值转可编辑文本（去掉多余的小数 0）。</summary>
        public static string ToText(double v)
        {
            return v.ToString("0.###", CultureInfo.CurrentCulture);
        }

        /// <summary>配方表 PSet（可能为 null，例如窗体还没建好）。只读用途。</summary>
        public static DataTable PSet()
        {
            try
            {
                ModuleBaseForm m = MiddleLayer.PlatF;
                if (m == null || m.RecipeData == null) return null;
                return m.RecipeData.Tables.Contains(TableName) ? m.RecipeData.Tables[TableName] : null;
            }
            catch { return null; }
        }

        // ---------------------------------------------------------------- 表格行

        /// <summary>
        /// 扫 PSet 的列，生成"表格里该有哪些行"。
        /// 识别规则：<c>&lt;前缀&gt; + (RowNum|Rowdis|ColumnNum|ColumnDis) [+ 序号]</c>，
        /// 例如 TrayRowNum / UnloadTrayRowdis2 → 行 (Tray,1) / (UnloadTray,2)。
        /// </summary>
        public static List<TrayRowSpec> BuildRows()
        {
            List<TrayRowSpec> rows = new List<TrayRowSpec>();
            DataTable t = PSet();
            if (t == null) return rows;

            List<string> order = new List<string>();
            Dictionary<string, TrayRowSpec> map =
                new Dictionary<string, TrayRowSpec>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < t.Columns.Count; i++)
            {
                string baseKey, suffix;
                int index;
                if (!ParseColumn(t.Columns[i].ColumnName, out baseKey, out index, out suffix)) continue;

                string key = baseKey + "#" + index;
                TrayRowSpec spec;
                if (!map.TryGetValue(key, out spec))
                {
                    spec = new TrayRowSpec();
                    spec.Key = index > 1 ? baseKey + index : baseKey;
                    spec.BaseKey = baseKey;
                    spec.Index = index;
                    Title(baseKey, index, out spec.TitleZh, out spec.TitleEn, out spec.TitleEs);
                    map[key] = spec;
                    order.Add(key);
                }

                string column = t.Columns[i].ColumnName;
                if (suffix == SufRowNum) spec.RowNumCol = column;
                else if (suffix == SufColNum) spec.ColumnNumCol = column;
                else if (suffix == SufRowDis) spec.RowPitchCol = column;
                else spec.ColumnPitchCol = column;
            }

            order.Sort(delegate (string a, string b)
            {
                TrayRowSpec sa = map[a], sb = map[b];
                int ra = Rank(sa.BaseKey), rb = Rank(sb.BaseKey);
                if (ra != rb) return ra - rb;
                int c = string.Compare(sa.BaseKey, sb.BaseKey, StringComparison.OrdinalIgnoreCase);
                return c != 0 ? c : sa.Index - sb.Index;
            });

            for (int i = 0; i < order.Count; i++) rows.Add(map[order[i]]);
            return rows;
        }

        // ---------------------------------------------------------------- 写

        /// <summary>把一个数值写进当前配方行；列不存在时返回 false。</summary>
        public static bool SetValue(string column, double value)
        {
            try
            {
                DataTable t = PSet();
                if (t == null || string.IsNullOrEmpty(column) || !t.Columns.Contains(column)) return false;
                EnsureRow(t);
                t.Rows[0][column] = value;
                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// 快速添加一组托盘：按名字建 4 个配方列（已存在的不重复建）。
        /// 名字只能是字母/数字/下划线且以字母开头（它会变成列名）。成功返回空串。
        /// </summary>
        public static string AddTray(string name)
        {
            DataTable t = PSet();
            if (t == null) return "配方表不可用";

            name = (name ?? "").Trim();
            if (name.Length == 0) return "请输入托盘名称";
            if (!IsValidName(name)) return "名称只能是字母、数字、下划线，且以字母开头";

            try
            {
                EnsureRow(t);
                for (int i = 0; i < Suffixes.Length; i++)
                {
                    string column = name + Suffixes[i];
                    if (!t.Columns.Contains(column))
                        t.Columns.Add(new DataColumn(column, typeof(double)));
                }
            }
            catch (Exception ex) { return ex.Message; }

            Commit();
            return "";
        }

        /// <summary>删除一组托盘（连同它的 4 个配方列）。成功返回空串。</summary>
        public static string RemoveTray(TrayRowSpec row)
        {
            DataTable t = PSet();
            if (t == null || row == null) return "配方表不可用";
            if (!row.CanRemove) return "这一组列不全，不能删除";

            try
            {
                string[] cols = { row.RowNumCol, row.ColumnNumCol, row.RowPitchCol, row.ColumnPitchCol };
                for (int i = 0; i < cols.Length; i++)
                    if (cols[i] != null && t.Columns.Contains(cols[i])) t.Columns.Remove(cols[i]);
            }
            catch (Exception ex) { return ex.Message; }

            Commit();
            return "";
        }

        /// <summary>把改动写回配方文件；失败返回 false（内存里已经改了）。</summary>
        public static bool Commit()
        {
            try
            {
                ModuleBaseForm m = MiddleLayer.PlatF;
                if (m == null) return false;
                string path = SysPara.FilePath;
                if (string.IsNullOrEmpty(path)) return false;
                m.WriteRecipeData(path);
                return true;
            }
            catch { return false; }
        }

        private static void EnsureRow(DataTable t)
        {
            if (t.Rows.Count > 0) return;
            t.Rows.Add(t.NewRow());
        }

        private static bool IsValidName(string name)
        {
            if (name.Length == 0 || !char.IsLetter(name[0])) return false;
            for (int i = 0; i < name.Length; i++)
                if (!char.IsLetterOrDigit(name[i]) && name[i] != '_') return false;
            for (int i = 0; i < Suffixes.Length; i++)
                if (name.EndsWith(Suffixes[i], StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }

        /// <summary>把列名拆成 (列名前缀, 序号, 后缀)；不符合托盘命名规则时返回 false。</summary>
        private static bool ParseColumn(string name, out string baseKey, out int index, out string suffix)
        {
            baseKey = null;
            index = 1;
            suffix = null;
            if (string.IsNullOrEmpty(name)) return false;

            int end = name.Length;
            while (end > 0 && char.IsDigit(name[end - 1])) end--;

            if (end < name.Length)
            {
                int n;
                if (!int.TryParse(name.Substring(end), out n) || n <= 1) return false;   // 序号必须 ≥ 2
                index = n;
            }

            string body = name.Substring(0, end);
            for (int i = 0; i < Suffixes.Length; i++)
            {
                if (body.Length <= Suffixes[i].Length) continue;
                if (!body.EndsWith(Suffixes[i], StringComparison.OrdinalIgnoreCase)) continue;
                baseKey = body.Substring(0, body.Length - Suffixes[i].Length);
                suffix = Suffixes[i];
                return true;
            }
            return false;
        }

        /// <summary>让常见托盘排在前面，其余排后面（按名字）。</summary>
        private static int Rank(string baseKey)
        {
            switch ((baseKey ?? "").ToUpperInvariant())
            {
                case "TRAY": return 0;
                case "UNLOADTRAY": return 1;
                case "REJECTTRAY": return 2;
                case "NGTRAY": return 3;
                default: return 10;
            }
        }

        /// <summary>行标题：认识的组给中/英/西名，不认识的直接用列名前缀。</summary>
        private static void Title(string baseKey, int index, out string zh, out string en, out string es)
        {
            string letter = Letter(index);                                        // 1 → A
            switch ((baseKey ?? "").ToUpperInvariant())
            {
                case "TRAY":
                    zh = "上料盘"; en = "Load tray"; es = "Bandeja de entrada"; return;
                case "UNLOADTRAY":
                    zh = "下料盘 " + letter; en = "Unload tray " + letter; es = "Bandeja de descarga " + letter; return;
                case "REJECTTRAY":
                    zh = "返修盘"; en = "Rework tray"; es = "Bandeja de retrabajo"; return;
                case "NGTRAY":
                    zh = "NG 盘"; en = "NG tray"; es = "Bandeja NG"; return;
                default:
                    string tail = index > 1 ? " " + letter : "";
                    zh = baseKey + tail; en = baseKey + tail; es = baseKey + tail; return;
            }
        }

        /// <summary>序号转字母：1→A、2→B（托盘 A/B 的叫法）。</summary>
        private static string Letter(int index)
        {
            return index >= 1 && index <= 26 ? ((char)('A' + index - 1)).ToString() : index.ToString();
        }
    }
}
