using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using AlphaRapLibrary;
using AlphaRap.Configuration;
using AlphaRap.FlowProgramming;

namespace AlphaRap
{
    public partial class SystemSetting : ModuleBaseForm
    {
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ConstantStore Constants { get; private set; }
        private string contextKey, contextScope;
        private string Scope { get { return scopes.SelectedTab == settingPage ? "Setting" : "Recipe"; } }

        /// <summary>创建固定常量表布局，设计期不读取机器文件。</summary>
        public SystemSetting()
        {
            InitializeComponent();
            InitializeConfigurationSchema();
            if (UiTheme.IsDesignTime) return;
            Constants = new ConstantStore(this);
            ConstantText.Apply(this);
            MiddleLayer.RegisterAndApplyLanguage(this, Name);
            MiddleLayer.LanguageChanged += ApplyLanguage;
            Disposed += (sender, args) => MiddleLayer.LanguageChanged -= ApplyLanguage;
            scopes.SelectedIndexChanged += (sender, args) => BindScope();
            searchValue.TextChanged += (sender, args) => Filter();
            add.Click += (sender, args) => Execute(() => EditConstant(null));
            edit.Click += (sender, args) => Execute(() => EditConstant(SelectedKey()));
            constants.MouseDown += SelectContextRow;
            constantMenu.Opening += OpenConstantMenu;
            constantDelete.Click += (sender, args) => Execute(DeleteSelected);
            ApplyLanguage(null, EventArgs.Empty);
            constants.CellDoubleClick += (sender, args) => { if (args.RowIndex >= 0) Execute(() => EditConstant(SelectedKey())); };
            constants.CellFormatting += FormatCell;
            VisibleChanged += (sender, args) => { if (Visible) { BindScope(); RefreshPermission(); } };
            permissionTimer.Tick += (sender, args) => RefreshPermission();
            permissionTimer.Start();
        }

        /// <summary>建立机器设置、兼容开关和配方常量的独立内存表，不读取现场配置。</summary>
        private void InitializeConfigurationSchema()
        {
            DataTable marker = new DataTable("MSet");
            marker.Columns.Add("ConstantsInitialized", typeof(bool)).DefaultValue = false;
            DataTable legacy = new DataTable("PSet");
            legacy.Columns.Add("DisScann", typeof(bool)).DefaultValue = false;
            legacy.Columns.Add("UseTryRun", typeof(bool)).DefaultValue = false;
            SettingData.Tables.Add(marker);
            SettingData.Tables.Add(legacy);
            SettingData.Tables.Add(ConstantStore.CreateTable());
            RecipeData.Tables.Add(ConstantStore.CreateTable());
        }

        /// <summary>沿用模块生命周期加载 Setting 并迁移现有开关，配方仍由统一入口加载。</summary>
        public override void ModuleInitialize(string moduleName)
        {
            base.ModuleInitialize(moduleName);
            Constants.InitializeExamples();
            BindScope();
        }

        /// <summary>两个作用域绑定各自的数据表，不改变稳定参数键。</summary>
        private void BindScope()
        {
            constants.DataSource = Constants.Table(Scope).DefaultView;
            Filter();
            recipeCaption.Text = string.Format(ConstantText.T("context"), SysPara.RecipeName);
        }

        /// <summary>按键或说明查询，将输入转义为普通匹配文本。</summary>
        private void Filter()
        {
            string text = searchValue.Text.Replace("'", "''").Replace("[", "[[]").Replace("%", "[%]").Replace("*", "[*]");
            string translated = Scope == "Setting" ? string.Concat(ConstantStore.ExampleKeys.Where(key => ConstantText.T(key).IndexOf(searchValue.Text, StringComparison.CurrentCultureIgnoreCase) >= 0).Select(key => " OR Key = '" + key + "'")) : "";
            Constants.Table(Scope).DefaultView.RowFilter = "Key <> '' AND (Key LIKE '%" + text + "%' OR Description LIKE '%" + text + "%'" + translated + ")";
        }

        /// <summary>按绑定行的业务键定位，不使用排序后行号。</summary>
        private string SelectedKey()
        {
            DataRowView row = constants.CurrentRow == null ? null : constants.CurrentRow.DataBoundItem as DataRowView;
            if (row == null) throw new InvalidOperationException(ConstantText.T("missing"));
            return Convert.ToString(row["Key"]);
        }

        /// <summary>在编辑副本校验成功后一次提交，取消不改变原常量。</summary>
        private void EditConstant(string key)
        {
            FlowPrograms.RequireIdle();
            DataRow row = key == null ? null : ConstantStore.Find(Constants.Table(Scope), key);
            ConstantValue value = row == null ? new ConstantValue { Type = "Boolean", Value = "False" } : ConstantStore.Read(row, DataRowVersion.Current);
            using (ConstantEditForm dialog = new ConstantEditForm(value, candidate => Constants.Put(Scope, key, candidate))) dialog.ShowDialog(this);
        }

        /// <summary>确认后按稳定键删除，服务层检查所有流程参数引用。</summary>
        private void DeleteSelected()
        {
            FlowPrograms.RequireIdle();
            string key = contextKey;
            if (key == null || contextScope != Scope) return;
            if (MessageBox.Show(string.Format(ConstantText.T("remove"), key), ConstantText.T("title"), MessageBoxButtons.OKCancel) != DialogResult.OK) return;
            Constants.Delete(Scope, key);
        }

        /// <summary>只记录右键命中的绑定行，表头和空白区不沿用旧选择。</summary>
        private void SelectContextRow(object sender, MouseEventArgs args)
        {
            if (args.Button != MouseButtons.Right) return;
            contextKey = null; contextScope = Scope;
            int index = constants.HitTest(args.X, args.Y).RowIndex;
            if (index < 0) return;
            constants.CurrentCell = constants.Rows[index].Cells[0];
            constants.ClearSelection(); constants.Rows[index].Selected = true;
            contextKey = SelectedKey();
        }

        /// <summary>只有命中行且可编辑时才允许删除，服务层仍检查流程引用。</summary>
        private void OpenConstantMenu(object sender, CancelEventArgs args)
        {
            if (contextKey == null || contextScope != Scope) { args.Cancel = true; return; }
            constantDelete.Enabled = FlowPrograms.CanEdit && SysPara.SystemMode == RunMode.IDLE && !FlowPrograms.AnyRunning && !NPSDK.Flow_Module.Module_GetRunningStatus;
        }

        /// <summary>翻译类型和内置说明，不翻译常量业务键或用户备注。</summary>
        private void FormatCell(object sender, DataGridViewCellFormattingEventArgs args)
        {
            if (args.RowIndex < 0) return;
            if (constants.Columns[args.ColumnIndex].Name == "constantType") { args.Value = ConstantText.T(Convert.ToString(args.Value)); args.FormattingApplied = true; }
            if (constants.Columns[args.ColumnIndex].Name != "constantDescription" || !string.IsNullOrEmpty(Convert.ToString(args.Value))) return;
            string key = Convert.ToString(constants.Rows[args.RowIndex].Cells["constantKey"].Value);
            if (Scope == "Setting" && ConstantStore.ExampleKeys.Contains(key)) { args.Value = ConstantText.T(key); args.FormattingApplied = true; }
        }

        /// <summary>全局保存前校验两个作用域的类型和值。</summary>
        public void ValidateConstants() { Constants.Validate("Setting"); Constants.Validate("Recipe"); }

        /// <summary>切换配方前拦截未保存常量，由主保存统一处理。</summary>
        public bool ConfirmRecipeChange()
        {
            if (Constants.Table("Recipe").GetChanges() == null) return true;
            MessageBox.Show(ConstantText.T("pendingMainSave"),ConstantText.T("title"));
            return false;
        }

        /// <summary>只刷新文字和显示格式，保留过滤条件及草稿。</summary>
        private void ApplyLanguage(object sender, EventArgs args)
        {
            ConstantText.Apply(this);
            constantDelete.Text = MiddleLayer.LangMsg(Name, constantDelete.Name, "删除", "Delete", "Eliminar");
            recipeCaption.Text = string.Format(ConstantText.T("context"), SysPara.RecipeName);
            constants.Invalidate();
        }

        /// <summary>运行中及操作员角色只读查看，服务入口再次校验。</summary>
        private void RefreshPermission()
        {
            bool allowed = FlowPrograms.CanEdit && SysPara.SystemMode == RunMode.IDLE && !FlowPrograms.AnyRunning && !NPSDK.Flow_Module.Module_GetRunningStatus;
            foreach (Control control in new Control[] { add, edit }) control.Enabled = allowed;
            recipeCaption.Text = string.Format(ConstantText.T("context"), SysPara.RecipeName);
        }

        /// <summary>显示操作错误并保留草稿，供用户修正后重试。</summary>
        private void Execute(Action action)
        {
            try { action(); }
            catch (Exception error) { status.Text = error.Message; MessageBox.Show(error.Message, ConstantText.T("title")); }
        }
    }
}
