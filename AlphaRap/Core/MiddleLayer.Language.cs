using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AlphaRap.Classes;

namespace AlphaRap
{
    partial class MiddleLayer
    {
        private sealed class LanguagePart
        {
            internal Control Owner;
            internal object Target;
            internal string Form;
            internal string Key;
            internal string DefaultText;
            internal Action<string> Apply;
        }

        private static readonly List<LanguagePart> LanguageParts = new List<LanguagePart>();
        internal static readonly object LanguageSync = new object();

        /// <summary>在创建任何页面之前读取界面语言和目录，数值区域格式保持不变。</summary>
        internal static void ReadLanguageSettings()
        {
            IniFile ini = new IniFile(".\\MachineSetup.ini");
            SysPara.LanguageDataDirectory = ini.ReadString("PathSetup", "LanguageDirectory", ".\\LanguageData");
            SysPara.LanguageName = ini.ReadString("MachineSetup", "LanguageName", "Chinese");
            SysPara.LanguageShow = SysPara.LanguageName == "English" ? LanguageType.English
                : SysPara.LanguageName == "Español" || SysPara.LanguageName == "Spanish" ? LanguageType.Español : LanguageType.Chinese;
        }

        /// <summary>返回权限代码的显示文字，数据库和下拉框的实际枚举值保持不变。</summary>
        internal static string PermissionText(object value)
        {
            string code = Convert.ToString(value);
            switch (code)
            {
                case "Operator": return LangMsg("Common", "role_Operator", "操作员", "Operator", "Operador");
                case "Maintenance": return LangMsg("Common", "role_Maintenance", "维护人员", "Maintenance", "Mantenimiento");
                case "Administrator": return LangMsg("Common", "role_Administrator", "管理员", "Administrator", "Administrador");
                case "None": return LangMsg("Common", "role_None", "未登录", "Not signed in", "Sin sesión");
                default: return code;
            }
        }

        /// <summary>刷新用户表的显示表头，不改变数据库字段或权限代码。</summary>
        internal static void RefreshUserHeaders(DataGridView grid)
        {
            foreach (DataGridViewColumn column in grid.Columns)
            {
                if (column.DataPropertyName == "UserName") column.HeaderText = LangMsg("Common", "col_UserName", "用户名", "User name", "Usuario");
                if (column.DataPropertyName == "Permission") column.HeaderText = LangMsg("Common", "col_Permission", "权限", "Permission", "Permiso");
            }
            grid.Invalidate();
        }

        /// <summary>登记表头和工具栏文案；只改变显示文字，不改变绑定字段、选中项或单元格数据。</summary>
        private static void RegisterLanguageParts(Control owner, string form)
        {
            LanguageParts.RemoveAll(part => part.Owner.IsDisposed || ReferenceEquals(part.Owner, owner));
            if (owner.Name.EndsWith(DynTextSuffix)) return;
            FieldBox field = owner as FieldBox;
            if (field != null && !string.IsNullOrWhiteSpace(field.Name))
                RegisterLanguagePart(owner, field, form, field.Name + "_Placeholder",
                    field.PlaceholderText, value => field.PlaceholderText = value);
            DataGridView grid = owner as DataGridView;
            if (grid != null && !string.IsNullOrWhiteSpace(grid.Name))
                foreach (DataGridViewColumn column in grid.Columns)
                    RegisterLanguagePart(owner, column, form, grid.Name + "_" + column.Name,
                        column.HeaderText, value => column.HeaderText = value);

            ListView list = owner as ListView;
            if (list != null && !string.IsNullOrWhiteSpace(list.Name))
                foreach (ColumnHeader column in list.Columns)
                    RegisterLanguagePart(owner, column, form, list.Name + "_" + column.Name,
                        column.Text, value => column.Text = value);

            ToolStrip strip = owner as ToolStrip;
            if (strip != null) RegisterLanguageMenu(owner, strip.Items, form);
            if (owner.ContextMenuStrip != null) RegisterLanguageMenu(owner, owner.ContextMenuStrip.Items, form);
        }

        /// <summary>递归登记工具栏与下拉菜单，保留项目的命令名和事件绑定。</summary>
        private static void RegisterLanguageMenu(Control owner, ToolStripItemCollection items, string form)
        {
            foreach (ToolStripItem item in items)
            {
                if (item is ToolStripSeparator || item is ToolStripControlHost) continue;
                RegisterLanguagePart(owner, item, form, item.Name, item.Text, value => item.Text = value);
                ToolStripDropDownItem menu = item as ToolStripDropDownItem;
                if (menu != null) RegisterLanguageMenu(owner, menu.DropDownItems, form);
            }
        }

        /// <summary>按对象替换重复登记项，并立即套用当前语言。</summary>
        private static void RegisterLanguagePart(Control owner, object target, string form, string key,
            string text, Action<string> apply)
        {
            if (string.IsNullOrWhiteSpace(key) || key.EndsWith(DynTextSuffix) || string.IsNullOrEmpty(text)) return;
            LanguageParts.RemoveAll(part => part.Owner.IsDisposed || ReferenceEquals(part.Target, target));
            LanguageParts.Add(new LanguagePart { Owner = owner, Target = target, Form = form,
                Key = key, DefaultText = text, Apply = apply });
            foreach (LanguageType language in Enum.GetValues(typeof(LanguageType)))
                SeedLangText(language, form, key, text);
            apply(LangText(form, key, text));
        }

        /// <summary>重新读取已保存的语言包，更新登记缓存并刷新当前页面。</summary>
        internal static void ReloadLanguageData()
        {
            lock (LanguageSync) _langDocs = null;
            if (SysPara.ComponentLangurageList != null)
                for (int i = 0; i < SysPara.ComponentLangurageList.Length; i++)
                    for (int j = 0; j < SysPara.ComponentLangurageList[i].Count; j++)
                    {
                        ComponentTextInfo entry = SysPara.ComponentLangurageList[i][j];
                        entry.ComponentText = LangTextOf((LanguageType)i, entry.FormName, entry.ComponentName) ?? entry.ComponentText;
                        SysPara.ComponentLangurageList[i][j] = entry;
                    }
            if (SysPara.TStripLangurageList != null)
                for (int i = 0; i < SysPara.TStripLangurageList.Length; i++)
                    for (int j = 0; j < SysPara.TStripLangurageList[i].Count; j++)
                    {
                        TStripItemTextInfo entry = SysPara.TStripLangurageList[i][j];
                        entry.ComponentText = LangTextOf((LanguageType)i, entry.FormName, entry.ComponentName) ?? entry.ComponentText;
                        SysPara.TStripLangurageList[i][j] = entry;
                    }
            SwitchLanguage(SysPara.LanguageShow);
        }

        /// <summary>刷新单个控件，隔离已释放控件或自定义控件的文字赋值异常。</summary>
        private static void ApplyRegisteredControl(ComponentTextInfo entry)
        {
            try
            {
                if (entry.Component == null || entry.Component.IsDisposed) return;
                NPSDK.Flow_Chart chart = entry.Component as NPSDK.Flow_Chart;
                if (chart != null) chart.text = entry.ComponentText;
                else entry.Component.Text = entry.ComponentText;
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex); }
        }

        /// <summary>刷新表头、工具栏和动态页面；一个订阅者失败不影响其他页面。</summary>
        private static void NotifyLanguageChanged()
        {
            LanguageParts.RemoveAll(part => part.Owner.IsDisposed);
            foreach (LanguagePart part in LanguageParts.ToArray())
            {
                try { part.Apply(LangText(part.Form, part.Key, part.DefaultText)); }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex); }
            }
            EventHandler handlers = LanguageChanged;
            if (handlers == null) return;
            foreach (EventHandler handler in handlers.GetInvocationList())
            {
                try { handler(null, EventArgs.Empty); }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex); }
            }
        }
    }
}
