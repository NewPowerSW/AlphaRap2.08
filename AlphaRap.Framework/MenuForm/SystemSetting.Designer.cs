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
            this.searchLabel = new System.Windows.Forms.Label();
            this.searchValue = new System.Windows.Forms.TextBox();
            this.recipeCaption = new System.Windows.Forms.Label();
            this.constantMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.constantDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.constants = new System.Windows.Forms.DataGridView();
            this.constantKey = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.constantType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.constantValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.constantDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.hint = new System.Windows.Forms.Label();
            this.status = new System.Windows.Forms.Label();
            this.permissionTimer = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.SettingData)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.RecipeData)).BeginInit();
            this.scopes.SuspendLayout();
            this.toolbar.SuspendLayout();
            this.constantMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.constants)).BeginInit();
            this.SuspendLayout();
            // 
            // title
            // 
            this.title.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(108)))), ((int)(((byte)(182)))));
            this.title.Dock = System.Windows.Forms.DockStyle.Top;
            this.title.Font = new System.Drawing.Font("微软雅黑", 14F);
            this.title.ForeColor = System.Drawing.Color.White;
            this.title.Location = new System.Drawing.Point(0, 0);
            this.title.Name = "title";
            this.title.Size = new System.Drawing.Size(1100, 42);
            this.title.TabIndex = 3;
            this.title.Tag = "title";
            this.title.Text = "设计器常量";
            this.title.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // scopes
            // 
            this.scopes.Controls.Add(this.settingPage);
            this.scopes.Controls.Add(this.recipePage);
            this.scopes.Dock = System.Windows.Forms.DockStyle.Top;
            this.scopes.Location = new System.Drawing.Point(0, 42);
            this.scopes.Name = "scopes";
            this.scopes.SelectedIndex = 0;
            this.scopes.Size = new System.Drawing.Size(1100, 34);
            this.scopes.TabIndex = 2;
            // 
            // settingPage
            // 
            this.settingPage.Location = new System.Drawing.Point(4, 26);
            this.settingPage.Name = "settingPage";
            this.settingPage.Size = new System.Drawing.Size(1092, 4);
            this.settingPage.TabIndex = 0;
            this.settingPage.Tag = "Setting";
            this.settingPage.Text = "Setting · 机器常量";
            // 
            // recipePage
            // 
            this.recipePage.Location = new System.Drawing.Point(4, 22);
            this.recipePage.Name = "recipePage";
            this.recipePage.Size = new System.Drawing.Size(192, 8);
            this.recipePage.TabIndex = 1;
            this.recipePage.Tag = "Recipe";
            this.recipePage.Text = "Recipe · 当前配方常量";
            // 
            // toolbar
            // 
            this.toolbar.Controls.Add(this.add);
            this.toolbar.Controls.Add(this.edit);
            this.toolbar.Controls.Add(this.searchLabel);
            this.toolbar.Controls.Add(this.searchValue);
            this.toolbar.Controls.Add(this.recipeCaption);
            this.toolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.toolbar.Location = new System.Drawing.Point(0, 76);
            this.toolbar.Name = "toolbar";
            this.toolbar.Padding = new System.Windows.Forms.Padding(6);
            this.toolbar.Size = new System.Drawing.Size(1100, 76);
            this.toolbar.TabIndex = 1;
            // 
            // add
            // 
            this.add.AutoSize = true;
            this.add.Location = new System.Drawing.Point(9, 9);
            this.add.Name = "add";
            this.add.Size = new System.Drawing.Size(75, 27);
            this.add.TabIndex = 0;
            this.add.Tag = "add";
            this.add.Text = "新增";
            // 
            // edit
            // 
            this.edit.AutoSize = true;
            this.edit.Location = new System.Drawing.Point(90, 9);
            this.edit.Name = "edit";
            this.edit.Size = new System.Drawing.Size(75, 27);
            this.edit.TabIndex = 1;
            this.edit.Tag = "edit";
            this.edit.Text = "修改";
            // 
            // searchLabel
            // 
            this.searchLabel.AutoSize = true;
            this.searchLabel.Location = new System.Drawing.Point(171, 6);
            this.searchLabel.Name = "searchLabel";
            this.searchLabel.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.searchLabel.Size = new System.Drawing.Size(80, 22);
            this.searchLabel.TabIndex = 2;
            this.searchLabel.Tag = "search";
            this.searchLabel.Text = "查询键或说明";
            // 
            // searchValue
            // 
            this.searchValue.Location = new System.Drawing.Point(257, 9);
            this.searchValue.Name = "searchValue";
            this.searchValue.Size = new System.Drawing.Size(210, 23);
            this.searchValue.TabIndex = 3;
            // 
            // recipeCaption
            // 
            this.recipeCaption.AutoSize = true;
            this.recipeCaption.Location = new System.Drawing.Point(473, 6);
            this.recipeCaption.Name = "recipeCaption";
            this.recipeCaption.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.recipeCaption.Size = new System.Drawing.Size(0, 22);
            this.recipeCaption.TabIndex = 4;
            // 
            // constantMenu
            // 
            this.constantMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.constantDelete});
            this.constantMenu.Name = "constantMenu";
            this.constantMenu.Size = new System.Drawing.Size(101, 26);
            // 
            // constantDelete
            // 
            this.constantDelete.Name = "constantDelete";
            this.constantDelete.Size = new System.Drawing.Size(100, 22);
            this.constantDelete.Text = "删除";
            // 
            // constants
            // 
            this.constants.AllowUserToAddRows = false;
            this.constants.AllowUserToDeleteRows = false;
            this.constants.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.constants.ContextMenuStrip = this.constantMenu;
            this.constants.Dock = System.Windows.Forms.DockStyle.Fill;
            this.constants.Location = new System.Drawing.Point(0, 152);
            this.constants.MultiSelect = false;
            this.constants.Name = "constants";
            this.constants.ReadOnly = true;
            this.constants.RowHeadersVisible = false;
            this.constants.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.constants.Size = new System.Drawing.Size(1100, 415);
            this.constants.TabIndex = 0;
            this.constants.AutoGenerateColumns = false;
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
            // 
            // hint
            // 
            this.hint.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.hint.Location = new System.Drawing.Point(0, 595);
            this.hint.Name = "hint";
            this.hint.Padding = new System.Windows.Forms.Padding(6);
            this.hint.Size = new System.Drawing.Size(1100, 55);
            this.hint.TabIndex = 5;
            this.hint.Tag = "hint";
            this.hint.Text = "Setting 不随配方切换；Recipe 跟随物料管理的当前配方。修改后使用主界面【保存】。流程节点的“型号 / 机器参数”可绑定 Constants。";
            // 
            // status
            // 
            this.status.AutoEllipsis = true;
            this.status.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.status.Location = new System.Drawing.Point(0, 567);
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(1100, 28);
            this.status.TabIndex = 4;
            // 
            // permissionTimer
            // 
            this.permissionTimer.Interval = 300;
            // 
            // SystemSetting
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1100, 650);
            this.Controls.Add(this.constants);
            this.Controls.Add(this.toolbar);
            this.Controls.Add(this.scopes);
            this.Controls.Add(this.title);
            this.Controls.Add(this.status);
            this.Controls.Add(this.hint);
            this.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.Name = "SystemSetting";
            this.Text = "SysSetting";
            ((System.ComponentModel.ISupportInitialize)(this.SettingData)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.RecipeData)).EndInit();
            this.scopes.ResumeLayout(false);
            this.toolbar.ResumeLayout(false);
            this.toolbar.PerformLayout();
            this.constantMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.constants)).EndInit();
            this.ResumeLayout(false);

        }
    }
}
