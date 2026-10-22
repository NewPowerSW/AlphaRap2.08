using System.Data;
using System.Drawing;
using System.Windows.Forms;
using AlphaRap.Configuration;

namespace AlphaRap
{
    partial class SystemSetting
    {
        private Label title;
        private TabControl scopes;
        private TabPage settingPage, recipePage;
        private FlowLayoutPanel toolbar;
        private Button add, edit;
        private ContextMenuStrip constantMenu;
        private ToolStripMenuItem constantDelete;
        private Label searchLabel, recipeCaption, hint, status;
        private TextBox searchValue;
        private DataGridView constants;
        private DataGridViewTextBoxColumn constantKey, constantType, constantValue, constantDescription;
        private Timer permissionTimer;

        /// <summary>释放计时器和子控件。</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && permissionTimer != null) permissionTimer.Dispose();
            if (disposing && constantMenu != null) constantMenu.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>设计器中可预览常量增删改查界面，保留旧模块身份和 PSet。</summary>
        private void InitializeComponent()
        {
            DataTable marker = new DataTable("MSet");
            marker.Columns.Add(new DataColumn("ConstantsInitialized", typeof(bool)) { DefaultValue = false });
            DataTable legacy = new DataTable("PSet");
            legacy.Columns.Add(new DataColumn("DisScann", typeof(bool)) { DefaultValue = false });
            legacy.Columns.Add(new DataColumn("UseTryRun", typeof(bool)) { DefaultValue = false });
            SettingData.Tables.AddRange(new[] { marker, legacy, ConstantStore.CreateTable() });
            RecipeData.Tables.Add(ConstantStore.CreateTable());
            title = new Label { Name = "constantsTitle", Tag = "title", Text = "设计器常量", Dock = DockStyle.Top, Height = 42, TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.FromArgb(4,108,182), ForeColor = Color.White, Font = new Font("Microsoft YaHei", 14F) };
            scopes = new TabControl { Name = "constantScopes", Dock = DockStyle.Top, Height = 34 };
            settingPage = new TabPage { Name = "settingPage", Tag = "Setting", Text = "Setting · 机器常量" };
            recipePage = new TabPage { Name = "recipePage", Tag = "Recipe", Text = "Recipe · 当前配方常量" };
            scopes.TabPages.AddRange(new[] { settingPage, recipePage });
            toolbar = new FlowLayoutPanel { Name = "constantToolbar", Dock = DockStyle.Top, Height = 76, Padding = new Padding(6), WrapContents = true };
            add = new Button { Name = "constantAdd", Tag = "add", Text = "新增", AutoSize = true };
            edit = new Button { Name = "constantEdit", Tag = "edit", Text = "修改", AutoSize = true };
            constantMenu = new ContextMenuStrip { Name = "constantMenu" };
            constantDelete = new ToolStripMenuItem { Name = "constantDelete", Text = "删除" };
            constantMenu.Items.Add(constantDelete);
            searchLabel = new Label { Name = "constantSearchLabel", Tag = "search", Text = "查询键或说明", AutoSize = true, Padding = new Padding(0,5,0,0) };
            searchValue = new TextBox { Name = "constantSearchValue", Width = 210 };
            recipeCaption = new Label { Name = "constantRecipeCaption_dyn", AutoSize = true, Padding = new Padding(0,5,0,0) };
            toolbar.Controls.AddRange(new Control[] { add, edit, searchLabel, searchValue, recipeCaption });
            constants = new DataGridView { Name = "constantsGrid", Dock = DockStyle.Fill, AutoGenerateColumns = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, RowHeadersVisible = false };
            constantKey = new DataGridViewTextBoxColumn { Name = "constantKey", Tag = "key", HeaderText = "常量键", DataPropertyName = "Key", FillWeight = 25 };
            constantType = new DataGridViewTextBoxColumn { Name = "constantType", Tag = "type", HeaderText = "类型", DataPropertyName = "Type", FillWeight = 15 };
            constantValue = new DataGridViewTextBoxColumn { Name = "constantValue", Tag = "value", HeaderText = "值", DataPropertyName = "Value", FillWeight = 20 };
            constantDescription = new DataGridViewTextBoxColumn { Name = "constantDescription", Tag = "description", HeaderText = "说明", DataPropertyName = "Description", FillWeight = 40 };
            constants.Columns.AddRange(constantKey, constantType, constantValue, constantDescription);
            constants.ContextMenuStrip = constantMenu;
            hint = new Label { Name = "constantHint", Tag = "hint", Text = "Setting 不随配方切换；Recipe 跟随物料管理的当前配方。修改后使用主界面【保存】。流程节点的“型号 / 机器参数”可绑定 Constants。", Dock = DockStyle.Bottom, Height = 55, Padding = new Padding(6) };
            status = new Label { Name = "constantStatus_dyn", Dock = DockStyle.Bottom, Height = 28, AutoEllipsis = true };
            permissionTimer = new Timer { Interval = 300 };
            Controls.Add(constants); Controls.Add(toolbar); Controls.Add(scopes); Controls.Add(title); Controls.Add(status); Controls.Add(hint);
            Name = "SystemSetting"; Text = "SysSetting";
            Font = new Font("Microsoft YaHei", 9F); ClientSize = new Size(1100,650); AutoScaleMode = AutoScaleMode.None;
        }
    }
}
