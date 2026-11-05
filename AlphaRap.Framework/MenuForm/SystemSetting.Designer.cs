using System.Drawing;
using System.Windows.Forms;

namespace AlphaRap
{
    partial class SystemSetting
    {
        private System.ComponentModel.IContainer components = null;
        private Label title;
        private TabControl scopes;
        private TabPage settingPage;
        private TabPage recipePage;
        private FlowLayoutPanel toolbar;
        private Button add;
        private Button edit;
        private ContextMenuStrip constantMenu;
        private ToolStripMenuItem constantDelete;
        private Label searchLabel;
        private Label recipeCaption;
        private Label hint;
        private Label status;
        private TextBox searchValue;
        private DataGridView constants;
        private DataGridViewTextBoxColumn constantKey;
        private DataGridViewTextBoxColumn constantType;
        private DataGridViewTextBoxColumn constantValue;
        private DataGridViewTextBoxColumn constantDescription;
        private Timer permissionTimer;

        /// <summary>释放常量表窗口的组件和子控件。</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>建立设计器与运行时共用的常量表布局。</summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.title = new System.Windows.Forms.Label();
            this.scopes = new System.Windows.Forms.TabControl();
            this.settingPage = new System.Windows.Forms.TabPage();
            this.recipePage = new System.Windows.Forms.TabPage();
            this.toolbar = new System.Windows.Forms.FlowLayoutPanel();
            this.add = new System.Windows.Forms.Button();
            this.edit = new System.Windows.Forms.Button();
            this.constantMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.constantDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.searchLabel = new System.Windows.Forms.Label();
            this.searchValue = new System.Windows.Forms.TextBox();
            this.recipeCaption = new System.Windows.Forms.Label();
            this.constants = new System.Windows.Forms.DataGridView();
            this.constantKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.constantType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.constantValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.constantDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.hint = new System.Windows.Forms.Label();
            this.status = new System.Windows.Forms.Label();
            this.permissionTimer = new System.Windows.Forms.Timer(this.components);
            this.scopes.SuspendLayout();
            this.toolbar.SuspendLayout();
            this.constantMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.constants)).BeginInit();
            this.SuspendLayout();
            // title
            this.title.Name = "constantsTitle";
            this.title.Tag = "title";
            this.title.Text = "设计器常量";
            this.title.Dock = System.Windows.Forms.DockStyle.Top;
            this.title.Height = 42;
            this.title.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.title.BackColor = System.Drawing.Color.FromArgb(4, 108, 182);
            this.title.ForeColor = System.Drawing.Color.White;
            this.title.Font = new System.Drawing.Font("Microsoft YaHei", 14F);
            // scopes
            this.scopes.Name = "constantScopes";
            this.scopes.Dock = System.Windows.Forms.DockStyle.Top;
            this.scopes.Height = 34;
            this.scopes.Controls.Add(this.settingPage);
            this.scopes.Controls.Add(this.recipePage);
            this.settingPage.Name = "settingPage";
            this.settingPage.Tag = "Setting";
            this.settingPage.Text = "Setting · 机器常量";
            this.recipePage.Name = "recipePage";
            this.recipePage.Tag = "Recipe";
            this.recipePage.Text = "Recipe · 当前配方常量";
            // toolbar
            this.toolbar.Name = "constantToolbar";
            this.toolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.toolbar.Height = 76;
            this.toolbar.Padding = new System.Windows.Forms.Padding(6);
            this.toolbar.WrapContents = true;
            this.toolbar.Controls.AddRange(new System.Windows.Forms.Control[] {
                this.add, this.edit, this.searchLabel, this.searchValue, this.recipeCaption });
            this.add.Name = "constantAdd";
            this.add.Tag = "add";
            this.add.Text = "新增";
            this.add.AutoSize = true;
            this.edit.Name = "constantEdit";
            this.edit.Tag = "edit";
            this.edit.Text = "修改";
            this.edit.AutoSize = true;
            this.constantMenu.Name = "constantMenu";
            this.constantMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this.constantDelete });
            this.constantDelete.Name = "constantDelete";
            this.constantDelete.Text = "删除";
            this.searchLabel.Name = "constantSearchLabel";
            this.searchLabel.Tag = "search";
            this.searchLabel.Text = "查询键或说明";
            this.searchLabel.AutoSize = true;
            this.searchLabel.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.searchValue.Name = "constantSearchValue";
            this.searchValue.Width = 210;
            this.recipeCaption.Name = "constantRecipeCaption_dyn";
            this.recipeCaption.AutoSize = true;
            this.recipeCaption.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
            // constants
            this.constants.Name = "constantsGrid";
            this.constants.Dock = System.Windows.Forms.DockStyle.Fill;
            this.constants.AutoGenerateColumns = false;
            this.constants.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.constants.ReadOnly = true;
            this.constants.AllowUserToAddRows = false;
            this.constants.AllowUserToDeleteRows = false;
            this.constants.MultiSelect = false;
            this.constants.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.constants.RowHeadersVisible = false;
            this.constants.ContextMenuStrip = this.constantMenu;
            this.constants.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                this.constantKey, this.constantType, this.constantValue, this.constantDescription });
            this.constantKey.Name = "constantKey";
            this.constantKey.Tag = "key";
            this.constantKey.HeaderText = "常量键";
            this.constantKey.DataPropertyName = "Key";
            this.constantKey.FillWeight = 25F;
            this.constantType.Name = "constantType";
            this.constantType.Tag = "type";
            this.constantType.HeaderText = "类型";
            this.constantType.DataPropertyName = "Type";
            this.constantType.FillWeight = 15F;
            this.constantValue.Name = "constantValue";
            this.constantValue.Tag = "value";
            this.constantValue.HeaderText = "值";
            this.constantValue.DataPropertyName = "Value";
            this.constantValue.FillWeight = 20F;
            this.constantDescription.Name = "constantDescription";
            this.constantDescription.Tag = "description";
            this.constantDescription.HeaderText = "说明";
            this.constantDescription.DataPropertyName = "Description";
            this.constantDescription.FillWeight = 40F;
            // hint
            this.hint.Name = "constantHint";
            this.hint.Tag = "hint";
            this.hint.Text = "Setting 不随配方切换；Recipe 跟随物料管理的当前配方。修改后使用主界面【保存】。流程节点的“型号 / 机器参数”可绑定 Constants。";
            this.hint.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.hint.Height = 55;
            this.hint.Padding = new System.Windows.Forms.Padding(6);
            this.status.Name = "constantStatus_dyn";
            this.status.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.status.Height = 28;
            this.status.AutoEllipsis = true;
            this.permissionTimer.Interval = 300;
            // SystemSetting
            this.Controls.Add(this.constants);
            this.Controls.Add(this.toolbar);
            this.Controls.Add(this.scopes);
            this.Controls.Add(this.title);
            this.Controls.Add(this.status);
            this.Controls.Add(this.hint);
            this.Name = "SystemSetting";
            this.Text = "SysSetting";
            this.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.ClientSize = new System.Drawing.Size(1100, 650);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.scopes.ResumeLayout(false);
            this.toolbar.ResumeLayout(false);
            this.toolbar.PerformLayout();
            this.constantMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.constants)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
