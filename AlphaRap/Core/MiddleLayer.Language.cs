using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using AlphaRap.Classes;
using AlphaRapLibrary;
using NPSDK;
using NPSDK.Motion;

namespace AlphaRap
{
    partial class MiddleLayer
    {
        /// <summary>固定文案更新后通知动态页面刷新，业务数据保持原值。</summary>
        public static event EventHandler LanguageChanged;

        public const string DynTextSuffix = "_dyn";

        /// <summary>初始化三语登记表，扫描所有启动页及框架自定义控件。</summary>
        private static void InitialLanguageData()
        {
            int count = Languages.Length;
            SysPara.ComponentLangurageList = new List<ComponentTextInfo>[count];
            SysPara.TStripLangurageList = new List<TStripItemTextInfo>[count];
            SysPara.CyCtrlLangList = new List<CylinderCtrlTextInfo>[count];
            for (int i = 0; i < count; i++)
            {
                SysPara.ComponentLangurageList[i] = new List<ComponentTextInfo>();
                SysPara.TStripLangurageList[i] = new List<TStripItemTextInfo>();
                SysPara.CyCtrlLangList[i] = new List<CylinderCtrlTextInfo>();
            }
            foreach (Control form in lstForm) RegisterLanguage(form, form.Name);
            if (LockForm1 != null) RegisterLanguage(LockForm1, "LockForm1");
            if (FlowF != null) RegisterLanguage(FlowF, "Flow");
            if (SpanLifeF != null) RegisterLanguage(SpanLifeF, "SpanLife");
        }

        /// <summary>按调用方传入的窗体和语言底稿生成气缸按钮文案。</summary>
        public static void InitialLangCallbackCyldCtrl(Form FormName, LanguageType Lantype, ref List<CyldCtrlLangIniData> cyCtrlLangIniDataList, ref List<CylinderCtrlTextInfo> CyCtrlLangList)
        {
            if (FormName == null || cyCtrlLangIniDataList == null || CyCtrlLangList == null) return;
            List<CylinderCtrlTextInfo> cyCtrlList = new List<CylinderCtrlTextInfo>();
            OtherItemsForLang.GetCylinderCtlItems(FormName, FormName.Name, ref cyCtrlList);
            Dictionary<string, CyldCtrlLangIniData> translations = new Dictionary<string, CyldCtrlLangIniData>(StringComparer.Ordinal);
            foreach (CyldCtrlLangIniData text in cyCtrlLangIniDataList)
                if (text.Language == Lantype && !translations.ContainsKey(text.ComponentName)) translations.Add(text.ComponentName, text);
            foreach (CylinderCtrlTextInfo cyCtrl in cyCtrlList)
            {
                CyldCtrlLangIniData text;
                if (!translations.TryGetValue(cyCtrl.ComponentName, out text)) continue;
                CyCtrlLangList.Add(new CylinderCtrlTextInfo { FormName = FormName.Name, ComponentName = text.ComponentName,
                    CyOff_BtnText = text.CyOff_BtnText, CyOn_BtnText = text.CyOn_BtnText });
            }
        }

        /// <summary>
        /// 把启动后创建的控件按控件名登记进语言表（文字来自 LanguageData\{语言}.xml 的 /{语言}/{窗体名}/{控件名}，
        /// XML 中没有的键用控件当前 Text 写入三份 XML）。登记后由 SwitchLanguage 更新文字。
        /// </summary>
        public static void RegisterLanguage(Control root, string formName)
        {
            if (root == null) return;
            RegisterLanguageControl(root, formName);
            RegisterLanguageParts(root, formName);
            if (root.GetType() == typeof(NPSDK.Flow_Chart)) return;
            if (root is TextBoxBase || root is FieldBox || root is DeviceControl || root is NP_Cylinder || root is ComboBox || root is DataGridView || root is PropertyGrid) return;
            for (int i = 0; i < root.Controls.Count; i++) RegisterLanguage(root.Controls[i], formName);
        }

        /// <summary>
        /// 把控件登记进语言表并立即按当前语言设置文字，用于按需创建的窗体（登录框 / 设置框 / 报警弹窗）。
        /// </summary>
        public static void RegisterAndApplyLanguage(Control root, string formName)
        {
            RegisterLanguage(root, formName);
            ApplyLanguageTexts(root, formName);
        }

        /// <summary>按当前语言从语言包刷新控件树中可登记控件的文字（逻辑与 SwitchLanguage 一致）。</summary>
        private static void ApplyLanguageTexts(Control root, string formName)
        {
            try
            {
                if (root == null) return;
                if (root.GetType() == typeof(NPSDK.Flow_Chart))
                {
                    ((NPSDK.Flow_Chart)root).text = LangText(formName, root.Name, ((NPSDK.Flow_Chart)root).text);
                    return;
                }

                string key = root.Name;
                if (!string.IsNullOrEmpty(key) && !key.EndsWith(DynTextSuffix) && IsLanguageControl(root))
                {
                    string t = LangTextOf(SysPara.LanguageShow, formName, key);
                    if (t != null)
                    {
                        root.Text = t;
                    }
                }

                if (root is TextBoxBase || root is FieldBox || root is DeviceControl || root is NP_Cylinder || root is ComboBox || root is DataGridView || root is PropertyGrid) return;
                for (int i = 0; i < root.Controls.Count; i++) ApplyLanguageTexts(root.Controls[i], formName);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// 判断控件的 Text 是否为界面文案：Form / Label / Button / CheckBox / RadioButton / TabPage / GroupBox /
        /// CheckedListBox / Flow_Chart 及其子类，以及 FlatButton / AlarmChip（二者直接继承 Control，需单独列出）。
        /// TextBox / DataGridView 等文字即数据的控件不计入。
        /// </summary>
        private static bool IsLanguageControl(Control c)
        {
            if (c == null) return false;
            if (c is ModuleBaseForm) return false;
            if (c.DataBindings.Count > 0) return false;
            if (c is TextBoxBase) return false;
            if (c is DataGridView) return false;
            if (c is ComboBox) return false;
            if (c is ListControl) return false;
            if (c is TabPage && (c.Tag is VpCameraConfig || c.Tag is VpVppConfig)) return false;
            if (c is TabPage && c.Tag is DeviceControl) return false;

            return c is Form
                || c is Label
                || c is Button
                || c is CheckBox
                || c is RadioButton
                || c is TabPage
                || c is GroupBox
                || c is CheckedListBox
                || c is FlatButton
                || c is AlarmChip
                || c is NP_Input
                || c is NP_Output
                || c is NP_Motor
                || c.GetType() == typeof(NPSDK.Flow_Chart);
        }

        /// <summary>按控件身份登记三语固定文字，排除数据绑定、动态内容和模块身份。</summary>
        private static void RegisterLanguageControl(Control c, string formName)
        {
            try
            {
                if (c == null) return;

                string key = c.Name;
                if (string.IsNullOrEmpty(key) || key.EndsWith(DynTextSuffix)) return;
                if (!IsLanguageControl(c)) return;
                string text = c.Text;
                if (string.IsNullOrEmpty(text)) return;

                int listCount = (SysPara.ComponentLangurageList == null) ? 0 : SysPara.ComponentLangurageList.Length;
                for (int i = 0; i < Languages.Length; i++)
                {
                    string t = LangTextOf((LanguageType)i, formName, key);
                    if (t == null) { SetLangText((LanguageType)i, formName, key, text); t = text; }
                    if (i >= listCount) continue;
                    List<ComponentTextInfo> list = SysPara.ComponentLangurageList[i];
                    if (list == null) continue;

                    list.RemoveAll(delegate (ComponentTextInfo x) { return ReferenceEquals(x.Component, c); });

                    ComponentTextInfo info = new ComponentTextInfo();
                    info.FormName = formName;
                    info.ComponentName = key;
                    info.ComponentText = t;
                    info.Component = c;
                    list.Add(info);
                }
            }
            catch (Exception) { }
        }

        /// <summary>切换框架语言，依次刷新控件、报警和动态页面，并保留业务数据。</summary>
        public static void SwitchLanguage(LanguageType LanType)
        {
            if (!Enum.IsDefined(typeof(LanguageType), LanType)) return;
            SysPara.LanguageShow = LanType;
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(
                LanType == LanguageType.Chinese ? "zh-CN" : LanType == LanguageType.English ? "en-US" : "es-ES");
            LoadAlarmTable(LanType);
            if (SysPara.ComponentLangurageList != null)
            {
                foreach (List<ComponentTextInfo> list in SysPara.ComponentLangurageList)
                    if (list != null) list.RemoveAll(entry => entry.Component == null || entry.Component.IsDisposed);
                List<ComponentTextInfo> entries = SysPara.ComponentLangurageList[(int)LanType];
                foreach (ComponentTextInfo entry in entries.ToArray()) ApplyRegisteredControl(entry);
            }
            if (MainF != null)
            {
                MainF.NumC1.Image = LanType == LanguageType.Chinese ? MainF.NumC2.Image
                    : LanType == LanguageType.English ? MainF.NumC3.Image : MainF.NumC4.Image;
                MainF.NumC1.Text = LanType == LanguageType.Chinese ? "CN" : LanType == LanguageType.English ? "EN" : "ES";
            }
            foreach (Flow_BaseForm module in Flow_Module.FlowChart_ModuleList) module.Refresh();
            NotifyLanguageChanged();
        }

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
            using (IniFile ini = new IniFile(".\\MachineSetup.ini"))
            {
                SysPara.LanguageDataDirectory = ini.ReadString("PathSetup", "LanguageDirectory", ".\\LanguageData");
                SysPara.LanguageName = ini.ReadString("MachineSetup", "LanguageName", "Chinese");
            }
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
            foreach (LanguageType language in Languages)
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
