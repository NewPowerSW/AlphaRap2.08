namespace AlphaRap.FlowProgramming
{
    partial class FlowNodeForm
    {
        private System.Windows.Forms.TableLayoutPanel root;
        private System.Windows.Forms.TableLayoutPanel header;
        private System.Windows.Forms.TableLayoutPanel routes;
        private System.Windows.Forms.TextBox captionValue;
        private System.Windows.Forms.ComboBox categoryValue;
        private System.Windows.Forms.ComboBox targetValue;
        private System.Windows.Forms.ComboBox actionValue;
        private System.Windows.Forms.NumericUpDown timeoutValue;
        private System.Windows.Forms.ComboBox alarmValue;
        private System.Windows.Forms.Label captionLabel;
        private System.Windows.Forms.Label categoryLabel;
        private System.Windows.Forms.Label targetLabel;
        private System.Windows.Forms.Label actionLabel;
        private System.Windows.Forms.Label timeoutLabel;
        private System.Windows.Forms.Label alarmLabel;
        private System.Windows.Forms.Panel inputsGroup;
        private System.Windows.Forms.Panel branchesGroup;
        private System.Windows.Forms.FlowLayoutPanel inputs;
        private System.Windows.Forms.TableLayoutPanel inputLayout;
        private System.Windows.Forms.Label methodHint;
        private System.Windows.Forms.CheckBox showAdvanced;
        private FlowInputRow inputPreview;
        private System.Windows.Forms.FlowLayoutPanel branchPanel;
        private System.Windows.Forms.Button addCase;
        private FlowBranchEditor nextBranch;
        private System.Windows.Forms.TextBox outputsValue;
        private System.Windows.Forms.Label hint;
        private System.Windows.Forms.FlowLayoutPanel buttons;
        private System.Windows.Forms.Button ok;
        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.TableLayoutPanel executionModes;
        private System.Windows.Forms.TableLayoutPanel actionSettings;
        private System.Windows.Forms.RadioButton actionMode;
        private System.Windows.Forms.RadioButton scriptMode;
        private System.Windows.Forms.RadioButton conditionMode;
        private System.Windows.Forms.CheckBox breakpointValue;
        private System.Windows.Forms.Label executionLabel;
        private System.Windows.Forms.TabControl executionTabs;
        private System.Windows.Forms.TabPage parameterPage;
        private System.Windows.Forms.TabPage scriptPage;
        private System.Windows.Forms.TabPage branchPage;
        private System.Windows.Forms.TabPage settingsPage;
        private FlowScriptSettings scriptSettings;
        private FlowScriptEditor scriptEditor;
        private System.Windows.Forms.RadioButton codeMode;
        private System.Windows.Forms.TableLayoutPanel codeSettings;
        private System.Windows.Forms.Label codeChoiceLabel;
        private System.Windows.Forms.ComboBox codeChoice;
        private FlowCodeBindingsControl codeBindings;
        private FlowCodeRoutesControl codeRoutes;
        /// <summary>按执行内容、数据、下一步和运行设置建立同级任务页，设计期不加载设备。</summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.codeMode = new System.Windows.Forms.RadioButton();
            this.codeSettings = new System.Windows.Forms.TableLayoutPanel();
            this.codeChoiceLabel = new System.Windows.Forms.Label();
            this.codeChoice = new System.Windows.Forms.ComboBox();
            this.codeBindings = new AlphaRap.FlowProgramming.FlowCodeBindingsControl();
            this.codeRoutes = new AlphaRap.FlowProgramming.FlowCodeRoutesControl();
            this.codeSettings.SuspendLayout();
            this.root = new System.Windows.Forms.TableLayoutPanel();
            this.header = new System.Windows.Forms.TableLayoutPanel();
            this.captionLabel = new System.Windows.Forms.Label();
            this.captionValue = new System.Windows.Forms.TextBox();
            this.executionLabel = new System.Windows.Forms.Label();
            this.executionModes = new System.Windows.Forms.TableLayoutPanel();
            this.actionMode = new System.Windows.Forms.RadioButton();
            this.scriptMode = new System.Windows.Forms.RadioButton();
            this.conditionMode = new System.Windows.Forms.RadioButton();
            this.executionTabs = new System.Windows.Forms.TabControl();
            this.parameterPage = new System.Windows.Forms.TabPage();
            this.inputsGroup = new System.Windows.Forms.Panel();
            this.inputLayout = new System.Windows.Forms.TableLayoutPanel();
            this.inputs = new System.Windows.Forms.FlowLayoutPanel();
            this.inputPreview = new AlphaRap.FlowProgramming.FlowInputRow();
            this.showAdvanced = new System.Windows.Forms.CheckBox();
            this.methodHint = new System.Windows.Forms.Label();
            this.outputsValue = new System.Windows.Forms.TextBox();
            this.scriptSettings = new AlphaRap.FlowProgramming.FlowScriptSettings();
            this.actionSettings = new System.Windows.Forms.TableLayoutPanel();
            this.categoryLabel = new System.Windows.Forms.Label();
            this.categoryValue = new System.Windows.Forms.ComboBox();
            this.targetLabel = new System.Windows.Forms.Label();
            this.targetValue = new System.Windows.Forms.ComboBox();
            this.actionLabel = new System.Windows.Forms.Label();
            this.actionValue = new System.Windows.Forms.ComboBox();
            this.scriptPage = new System.Windows.Forms.TabPage();
            this.scriptEditor = new AlphaRap.FlowProgramming.FlowScriptEditor();
            this.branchPage = new System.Windows.Forms.TabPage();
            this.branchesGroup = new System.Windows.Forms.Panel();
            this.branchPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.nextBranch = new AlphaRap.FlowProgramming.FlowBranchEditor();
            this.addCase = new System.Windows.Forms.Button();
            this.hint = new System.Windows.Forms.Label();
            this.settingsPage = new System.Windows.Forms.TabPage();
            this.routes = new System.Windows.Forms.TableLayoutPanel();
            this.breakpointValue = new System.Windows.Forms.CheckBox();
            this.timeoutLabel = new System.Windows.Forms.Label();
            this.timeoutValue = new System.Windows.Forms.NumericUpDown();
            this.alarmLabel = new System.Windows.Forms.Label();
            this.alarmValue = new System.Windows.Forms.ComboBox();
            this.buttons = new System.Windows.Forms.FlowLayoutPanel();
            this.cancel = new System.Windows.Forms.Button();
            this.ok = new System.Windows.Forms.Button();
            this.root.SuspendLayout();
            this.header.SuspendLayout();
            this.executionModes.SuspendLayout();
            this.executionTabs.SuspendLayout();
            this.parameterPage.SuspendLayout();
            this.inputsGroup.SuspendLayout();
            this.inputLayout.SuspendLayout();
            this.inputs.SuspendLayout();
            this.actionSettings.SuspendLayout();
            this.scriptPage.SuspendLayout();
            this.branchPage.SuspendLayout();
            this.branchesGroup.SuspendLayout();
            this.branchPanel.SuspendLayout();
            this.settingsPage.SuspendLayout();
            this.routes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.timeoutValue)).BeginInit();
            this.buttons.SuspendLayout();
            this.SuspendLayout();
            //
            // root
            //
            this.root.ColumnCount = 1;
            this.root.Controls.Add(this.header, 0, 0);
            this.root.Controls.Add(this.executionTabs, 0, 1);
            this.root.Controls.Add(this.buttons, 0, 2);
            this.root.Dock = System.Windows.Forms.DockStyle.Fill;
            this.root.Location = new System.Drawing.Point(0, 0);
            this.root.Name = "root";
            this.root.Padding = new System.Windows.Forms.Padding(10);
            this.root.RowCount = 3;
            this.root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 96F));
            this.root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.root.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.root.Size = new System.Drawing.Size(1000, 780);
            this.root.TabIndex = 0;
            //
            // header
            //
            this.header.ColumnCount = 2;
            this.header.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.header.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.header.Controls.Add(this.captionLabel, 0, 0);
            this.header.Controls.Add(this.captionValue, 1, 0);
            this.header.Controls.Add(this.executionLabel, 0, 1);
            this.header.Controls.Add(this.executionModes, 1, 1);
            this.header.Dock = System.Windows.Forms.DockStyle.Fill;
            this.header.Location = new System.Drawing.Point(13, 13);
            this.header.Name = "header";
            this.header.RowCount = 2;
            this.header.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.header.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.header.Size = new System.Drawing.Size(974, 90);
            this.header.TabIndex = 0;
            //
            // captionLabel
            //
            this.captionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.captionLabel.Location = new System.Drawing.Point(3, 0);
            this.captionLabel.Name = "captionLabel";
            this.captionLabel.Size = new System.Drawing.Size(104, 40);
            this.captionLabel.TabIndex = 0;
            this.captionLabel.Tag = "caption";
            this.captionLabel.Text = "节点名称";
            this.captionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // captionValue
            //
            this.captionValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.captionValue.Location = new System.Drawing.Point(113, 8);
            this.captionValue.Name = "captionValue";
            this.captionValue.Size = new System.Drawing.Size(858, 23);
            this.captionValue.TabIndex = 1;
            //
            // executionLabel
            //
            this.executionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.executionLabel.Location = new System.Drawing.Point(3, 40);
            this.executionLabel.Name = "scriptMode";
            this.executionLabel.Size = new System.Drawing.Size(104, 50);
            this.executionLabel.TabIndex = 2;
            this.executionLabel.Tag = "scriptMode";
            this.executionLabel.Text = "执行方式";
            this.executionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // executionModes
            //
            this.executionModes.ColumnCount = 4;
            this.executionModes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.executionModes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.executionModes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.executionModes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.executionModes.Controls.Add(this.actionMode, 0, 0);
            this.executionModes.Controls.Add(this.scriptMode, 1, 0);
            this.executionModes.Controls.Add(this.conditionMode, 2, 0);
            this.executionModes.Controls.Add(this.codeMode, 3, 0);
            this.codeMode.Name = "codeModuleMode";
            this.codeMode.Text = "代码模块";
            this.codeMode.Appearance = System.Windows.Forms.Appearance.Button;
            this.codeMode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.codeMode.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.codeMode.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.codeMode.AutoSize = false;
            this.executionModes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.executionModes.Location = new System.Drawing.Point(113, 43);
            this.executionModes.Name = "executionModes";
            this.executionModes.Padding = new System.Windows.Forms.Padding(0, 2, 0, 5);
            this.executionModes.RowCount = 1;
            this.executionModes.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.executionModes.Size = new System.Drawing.Size(858, 44);
            this.executionModes.TabIndex = 3;
            //
            // actionMode
            //
            this.actionMode.Appearance = System.Windows.Forms.Appearance.Button;
            this.actionMode.Checked = true;
            this.actionMode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.actionMode.Location = new System.Drawing.Point(3, 5);
            this.actionMode.Name = "scriptAction";
            this.actionMode.Size = new System.Drawing.Size(279, 31);
            this.actionMode.TabIndex = 0;
            this.actionMode.TabStop = true;
            this.actionMode.Tag = "scriptAction";
            this.actionMode.Text = "现有功能";
            this.actionMode.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // scriptMode
            //
            this.scriptMode.Appearance = System.Windows.Forms.Appearance.Button;
            this.scriptMode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scriptMode.Location = new System.Drawing.Point(288, 5);
            this.scriptMode.Name = "scriptCustom";
            this.scriptMode.Size = new System.Drawing.Size(279, 31);
            this.scriptMode.TabIndex = 1;
            this.scriptMode.Tag = "scriptCustom";
            this.scriptMode.Text = "自定义 C#";
            this.scriptMode.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // conditionMode
            //
            this.conditionMode.Appearance = System.Windows.Forms.Appearance.Button;
            this.conditionMode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.conditionMode.Location = new System.Drawing.Point(573, 5);
            this.conditionMode.Name = "scriptNone";
            this.conditionMode.Size = new System.Drawing.Size(282, 31);
            this.conditionMode.TabIndex = 2;
            this.conditionMode.Tag = "scriptNone";
            this.conditionMode.Text = "仅判断条件";
            this.conditionMode.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // executionTabs
            //
            this.executionTabs.Controls.Add(this.parameterPage);
            this.executionTabs.Controls.Add(this.scriptPage);
            this.executionTabs.Controls.Add(this.branchPage);
            this.executionTabs.Controls.Add(this.settingsPage);
            this.executionTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.executionTabs.Location = new System.Drawing.Point(13, 109);
            this.executionTabs.Name = "executionTabs";
            this.executionTabs.SelectedIndex = 0;
            this.executionTabs.Size = new System.Drawing.Size(974, 616);
            this.executionTabs.TabIndex = 1;
            //
            // parameterPage
            //
            this.parameterPage.Controls.Add(this.inputsGroup);
            this.parameterPage.Controls.Add(this.codeSettings);
            this.codeSettings.Name = "codeSettings";
            this.codeSettings.Dock = System.Windows.Forms.DockStyle.Top;
            this.codeSettings.Height = 180;
            this.codeSettings.Visible = false;
            this.codeSettings.ColumnCount = 2;
            this.codeSettings.RowCount = 2;
            this.codeSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.codeSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.codeSettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.codeSettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.codeSettings.Controls.Add(this.codeChoiceLabel, 0, 0);
            this.codeSettings.Controls.Add(this.codeChoice, 1, 0);
            this.codeSettings.Controls.Add(this.codeBindings, 0, 1);
            this.codeSettings.SetColumnSpan(this.codeBindings, 2);
            this.codeChoiceLabel.Name = "codeChoiceLabel";
            this.codeChoiceLabel.Text = "选择模块";
            this.codeChoiceLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.codeChoiceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.codeChoice.Name = "codeModuleChoice";
            this.codeChoice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.codeChoice.Dock = System.Windows.Forms.DockStyle.Fill;
            this.codeBindings.Name = "codeResourceBindings";
            this.codeBindings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.parameterPage.Controls.Add(this.outputsValue);
            this.parameterPage.Controls.Add(this.scriptSettings);
            this.parameterPage.Controls.Add(this.actionSettings);
            this.parameterPage.Location = new System.Drawing.Point(4, 26);
            this.parameterPage.Name = "scriptActionPage";
            this.parameterPage.Padding = new System.Windows.Forms.Padding(6);
            this.parameterPage.Size = new System.Drawing.Size(966, 586);
            this.parameterPage.TabIndex = 0;
            this.parameterPage.Tag = "nodeActionPage";
            this.parameterPage.Text = "动作设置";
            //
            // inputsGroup
            //
            this.inputsGroup.Controls.Add(this.inputLayout);
            this.inputsGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputsGroup.Location = new System.Drawing.Point(6, 356);
            this.inputsGroup.Name = "inputsGroup";
            this.inputsGroup.Size = new System.Drawing.Size(954, 201);
            this.inputsGroup.TabIndex = 0;
            //
            // inputLayout
            //
            this.inputLayout.ColumnCount = 1;
            this.inputLayout.Controls.Add(this.inputs, 0, 0);
            this.inputLayout.Controls.Add(this.showAdvanced, 0, 1);
            this.inputLayout.Controls.Add(this.methodHint, 0, 2);
            this.inputLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputLayout.Location = new System.Drawing.Point(0, 0);
            this.inputLayout.Name = "inputLayout";
            this.inputLayout.RowCount = 3;
            this.inputLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.inputLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.inputLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 0F));
            this.inputLayout.Size = new System.Drawing.Size(954, 201);
            this.inputLayout.TabIndex = 0;
            //
            // inputs
            //
            this.inputs.AutoScroll = true;
            this.inputs.Controls.Add(this.inputPreview);
            this.inputs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputs.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.inputs.Location = new System.Drawing.Point(3, 3);
            this.inputs.Name = "inputs";
            this.inputs.Size = new System.Drawing.Size(948, 167);
            this.inputs.TabIndex = 0;
            this.inputs.WrapContents = false;
            //
            // inputPreview
            //
            this.inputPreview.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.inputPreview.Location = new System.Drawing.Point(0, 1);
            this.inputPreview.Margin = new System.Windows.Forms.Padding(0, 1, 0, 1);
            this.inputPreview.MinimumSize = new System.Drawing.Size(560, 44);
            this.inputPreview.Name = "inputPreview";
            this.inputPreview.Size = new System.Drawing.Size(930, 44);
            this.inputPreview.TabIndex = 0;
            //
            // showAdvanced
            //
            this.showAdvanced.AutoSize = true;
            this.showAdvanced.Dock = System.Windows.Forms.DockStyle.Fill;
            this.showAdvanced.Location = new System.Drawing.Point(3, 176);
            this.showAdvanced.Name = "showAdvanced";
            this.showAdvanced.Size = new System.Drawing.Size(948, 22);
            this.showAdvanced.TabIndex = 1;
            this.showAdvanced.Tag = "showAdvanced";
            this.showAdvanced.Text = "显示高级参数";
            //
            // methodHint
            //
            this.methodHint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.methodHint.Location = new System.Drawing.Point(3, 201);
            this.methodHint.Name = "methodHint_dyn";
            this.methodHint.Padding = new System.Windows.Forms.Padding(4);
            this.methodHint.Size = new System.Drawing.Size(948, 1);
            this.methodHint.TabIndex = 2;
            this.methodHint.Visible = false;
            //
            // outputsValue
            //
            this.outputsValue.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.outputsValue.Location = new System.Drawing.Point(6, 557);
            this.outputsValue.Name = "outputsValue_dyn";
            this.outputsValue.ReadOnly = true;
            this.outputsValue.Size = new System.Drawing.Size(954, 23);
            this.outputsValue.TabIndex = 1;
            //
            // scriptSettings
            //
            this.scriptSettings.Dock = System.Windows.Forms.DockStyle.Top;
            this.scriptSettings.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.scriptSettings.Location = new System.Drawing.Point(6, 96);
            this.scriptSettings.MinimumSize = new System.Drawing.Size(600, 240);
            this.scriptSettings.Name = "scriptSettings";
            this.scriptSettings.Size = new System.Drawing.Size(954, 260);
            this.scriptSettings.TabIndex = 2;
            this.scriptSettings.Visible = false;
            //
            // actionSettings
            //
            this.actionSettings.ColumnCount = 4;
            this.actionSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.actionSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.actionSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.actionSettings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.actionSettings.Controls.Add(this.categoryLabel, 0, 0);
            this.actionSettings.Controls.Add(this.categoryValue, 1, 0);
            this.actionSettings.Controls.Add(this.targetLabel, 2, 0);
            this.actionSettings.Controls.Add(this.targetValue, 3, 0);
            this.actionSettings.Controls.Add(this.actionLabel, 0, 1);
            this.actionSettings.Controls.Add(this.actionValue, 1, 1);
            this.actionSettings.Dock = System.Windows.Forms.DockStyle.Top;
            this.actionSettings.Location = new System.Drawing.Point(6, 6);
            this.actionSettings.Name = "actionSettings";
            this.actionSettings.RowCount = 2;
            this.actionSettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.actionSettings.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.actionSettings.Size = new System.Drawing.Size(954, 90);
            this.actionSettings.TabIndex = 3;
            //
            // categoryLabel
            //
            this.categoryLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.categoryLabel.Location = new System.Drawing.Point(3, 0);
            this.categoryLabel.Name = "categoryLabel";
            this.categoryLabel.Size = new System.Drawing.Size(104, 45);
            this.categoryLabel.TabIndex = 0;
            this.categoryLabel.Tag = "category";
            this.categoryLabel.Text = "功能类别";
            this.categoryLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // categoryValue
            //
            this.categoryValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.categoryValue.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.categoryValue.Location = new System.Drawing.Point(113, 12);
            this.categoryValue.Name = "categoryValue";
            this.categoryValue.Size = new System.Drawing.Size(361, 25);
            this.categoryValue.TabIndex = 1;
            //
            // targetLabel
            //
            this.targetLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.targetLabel.Location = new System.Drawing.Point(480, 0);
            this.targetLabel.Name = "targetLabel";
            this.targetLabel.Size = new System.Drawing.Size(104, 45);
            this.targetLabel.TabIndex = 2;
            this.targetLabel.Tag = "target";
            this.targetLabel.Text = "实际对象";
            this.targetLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // targetValue
            //
            this.targetValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.targetValue.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.targetValue.Location = new System.Drawing.Point(590, 12);
            this.targetValue.Name = "targetValue";
            this.targetValue.Size = new System.Drawing.Size(361, 25);
            this.targetValue.TabIndex = 3;
            //
            // actionLabel
            //
            this.actionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.actionLabel.Location = new System.Drawing.Point(3, 45);
            this.actionLabel.Name = "actionLabel";
            this.actionLabel.Size = new System.Drawing.Size(104, 45);
            this.actionLabel.TabIndex = 4;
            this.actionLabel.Tag = "method";
            this.actionLabel.Text = "对象方法";
            this.actionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // actionValue
            //
            this.actionValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.actionSettings.SetColumnSpan(this.actionValue, 3);
            this.actionValue.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.actionValue.Location = new System.Drawing.Point(113, 57);
            this.actionValue.Name = "actionValue";
            this.actionValue.Size = new System.Drawing.Size(838, 25);
            this.actionValue.TabIndex = 5;
            //
            // scriptPage
            //
            this.scriptPage.Controls.Add(this.scriptEditor);
            this.scriptPage.Location = new System.Drawing.Point(4, 26);
            this.scriptPage.Name = "scriptTab";
            this.scriptPage.Size = new System.Drawing.Size(192, 0);
            this.scriptPage.TabIndex = 1;
            this.scriptPage.Tag = "scriptTab";
            this.scriptPage.Text = "C# 脚本";
            //
            // scriptEditor
            //
            this.scriptEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scriptEditor.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.scriptEditor.Location = new System.Drawing.Point(0, 0);
            this.scriptEditor.MinimumSize = new System.Drawing.Size(500, 220);
            this.scriptEditor.Name = "scriptEditor";
            this.scriptEditor.Size = new System.Drawing.Size(500, 220);
            this.scriptEditor.TabIndex = 0;
            //
            // branchPage
            //
            this.branchPage.Controls.Add(this.branchesGroup);
            this.branchPage.Location = new System.Drawing.Point(4, 26);
            this.branchPage.Name = "scriptRoutes";
            this.branchPage.Padding = new System.Windows.Forms.Padding(6);
            this.branchPage.Size = new System.Drawing.Size(192, 0);
            this.branchPage.TabIndex = 2;
            this.branchPage.Tag = "nodeNextPage";
            this.branchPage.Text = "下一步";
            //
            // branchesGroup
            //
            this.branchesGroup.Controls.Add(this.branchPanel);
            this.branchesGroup.Controls.Add(this.codeRoutes);
            this.codeRoutes.Name = "codeExitTargets";
            this.codeRoutes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.codeRoutes.Visible = false;
            this.branchesGroup.Controls.Add(this.addCase);
            this.branchesGroup.Controls.Add(this.hint);
            this.branchesGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.branchesGroup.Location = new System.Drawing.Point(6, 6);
            this.branchesGroup.Name = "branchesGroup";
            this.branchesGroup.Size = new System.Drawing.Size(180, 0);
            this.branchesGroup.TabIndex = 0;
            //
            // branchPanel
            //
            this.branchPanel.AutoScroll = true;
            this.branchPanel.Controls.Add(this.nextBranch);
            this.branchPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.branchPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.branchPanel.Location = new System.Drawing.Point(0, 28);
            this.branchPanel.Name = "branchPanel";
            this.branchPanel.Size = new System.Drawing.Size(180, 0);
            this.branchPanel.TabIndex = 0;
            this.branchPanel.WrapContents = false;
            //
            // nextBranch
            //
            this.nextBranch.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.nextBranch.Location = new System.Drawing.Point(0, 3);
            this.nextBranch.Margin = new System.Windows.Forms.Padding(0, 3, 0, 8);
            this.nextBranch.MinimumSize = new System.Drawing.Size(600, 40);
            this.nextBranch.Name = "branch_NEXT_dyn";
            this.nextBranch.Padding = new System.Windows.Forms.Padding(4);
            this.nextBranch.Size = new System.Drawing.Size(930, 40);
            this.nextBranch.TabIndex = 0;
            //
            // addCase
            //
            this.addCase.Dock = System.Windows.Forms.DockStyle.Top;
            this.addCase.Location = new System.Drawing.Point(0, 0);
            this.addCase.Name = "addCase";
            this.addCase.Size = new System.Drawing.Size(180, 28);
            this.addCase.TabIndex = 1;
            this.addCase.Tag = "addCase";
            this.addCase.Text = "添加 CASE";
            //
            // hint
            //
            this.hint.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.hint.Location = new System.Drawing.Point(0, -46);
            this.hint.Name = "branchHint";
            this.hint.Size = new System.Drawing.Size(180, 46);
            this.hint.TabIndex = 2;
            this.hint.Tag = "branchHint";
            this.hint.Text = "默认 NEXT，按需添加 CASE。每条出口独立判定，超时直接报警。";
            //
            // settingsPage
            //
            this.settingsPage.Controls.Add(this.routes);
            this.settingsPage.Location = new System.Drawing.Point(4, 26);
            this.settingsPage.Name = "scriptRunSettings";
            this.settingsPage.Padding = new System.Windows.Forms.Padding(10);
            this.settingsPage.Size = new System.Drawing.Size(192, 0);
            this.settingsPage.TabIndex = 3;
            this.settingsPage.Tag = "nodeRunSettings";
            this.settingsPage.Text = "超时与调试";
            //
            // routes
            //
            this.routes.ColumnCount = 4;
            this.routes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.routes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.routes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.routes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.routes.Controls.Add(this.breakpointValue, 1, 1);
            this.routes.Controls.Add(this.timeoutLabel, 0, 0);
            this.routes.Controls.Add(this.timeoutValue, 1, 0);
            this.routes.Controls.Add(this.alarmLabel, 2, 0);
            this.routes.Controls.Add(this.alarmValue, 3, 0);
            this.routes.Dock = System.Windows.Forms.DockStyle.Top;
            this.routes.Location = new System.Drawing.Point(10, 10);
            this.routes.Name = "routes";
            this.routes.RowCount = 2;
            this.routes.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));
            this.routes.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.routes.Size = new System.Drawing.Size(172, 104);
            this.routes.TabIndex = 0;
            //
            // breakpointValue
            //
            this.breakpointValue.AutoSize = true;
            this.routes.SetColumnSpan(this.breakpointValue, 3);
            this.breakpointValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.breakpointValue.Location = new System.Drawing.Point(113, 51);
            this.breakpointValue.Name = "scriptBreakpoint";
            this.breakpointValue.Size = new System.Drawing.Size(56, 50);
            this.breakpointValue.TabIndex = 0;
            this.breakpointValue.Tag = "scriptBreakpoint";
            this.breakpointValue.Text = "执行前断点";
            //
            // timeoutLabel
            //
            this.timeoutLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.timeoutLabel.Location = new System.Drawing.Point(3, 0);
            this.timeoutLabel.Name = "timeoutLabel";
            this.timeoutLabel.Size = new System.Drawing.Size(104, 48);
            this.timeoutLabel.TabIndex = 1;
            this.timeoutLabel.Tag = "timeout";
            this.timeoutLabel.Text = "超时 (ms)";
            this.timeoutLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // timeoutValue
            //
            this.timeoutValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.timeoutValue.Location = new System.Drawing.Point(113, 12);
            this.timeoutValue.Maximum = new decimal(new int[] {
            86400000,
            0,
            0,
            0});
            this.timeoutValue.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.timeoutValue.Name = "timeoutValue";
            this.timeoutValue.Size = new System.Drawing.Size(1, 23);
            this.timeoutValue.TabIndex = 2;
            this.timeoutValue.Value = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            //
            // alarmLabel
            //
            this.alarmLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.alarmLabel.Location = new System.Drawing.Point(89, 0);
            this.alarmLabel.Name = "alarmLabel";
            this.alarmLabel.Size = new System.Drawing.Size(104, 48);
            this.alarmLabel.TabIndex = 3;
            this.alarmLabel.Tag = "alarm";
            this.alarmLabel.Text = "异常报警号";
            this.alarmLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // alarmValue
            //
            this.alarmValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.alarmValue.Location = new System.Drawing.Point(199, 14);
            this.alarmValue.Name = "alarmValue";
            this.alarmValue.Size = new System.Drawing.Size(1, 25);
            this.alarmValue.TabIndex = 4;
            //
            // buttons
            //
            this.buttons.Controls.Add(this.cancel);
            this.buttons.Controls.Add(this.ok);
            this.buttons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.buttons.Location = new System.Drawing.Point(13, 731);
            this.buttons.Name = "buttons";
            this.buttons.Size = new System.Drawing.Size(974, 36);
            this.buttons.TabIndex = 2;
            //
            // cancel
            //
            this.cancel.AutoSize = true;
            this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancel.Location = new System.Drawing.Point(896, 3);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(75, 27);
            this.cancel.TabIndex = 0;
            this.cancel.Tag = "cancel";
            this.cancel.Text = "取消";
            //
            // ok
            //
            this.ok.AutoSize = true;
            this.ok.Location = new System.Drawing.Point(815, 3);
            this.ok.Name = "ok";
            this.ok.Size = new System.Drawing.Size(75, 27);
            this.ok.TabIndex = 1;
            this.ok.Tag = "ok";
            this.ok.Text = "确定";
            //
            // FlowNodeForm
            //
            this.AcceptButton = this.ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.cancel;
            this.ClientSize = new System.Drawing.Size(1000, 780);
            this.Controls.Add(this.root);
            this.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.MinimumSize = new System.Drawing.Size(900, 720);
            this.Name = "FlowNodeForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Tag = "title";
            this.Text = "流程工程";
            this.root.ResumeLayout(false);
            this.header.ResumeLayout(false);
            this.header.PerformLayout();
            this.executionModes.ResumeLayout(false);
            this.executionTabs.ResumeLayout(false);
            this.parameterPage.ResumeLayout(false);
            this.parameterPage.PerformLayout();
            this.inputsGroup.ResumeLayout(false);
            this.inputLayout.ResumeLayout(false);
            this.inputLayout.PerformLayout();
            this.inputs.ResumeLayout(false);
            this.actionSettings.ResumeLayout(false);
            this.scriptPage.ResumeLayout(false);
            this.branchPage.ResumeLayout(false);
            this.branchesGroup.ResumeLayout(false);
            this.branchPanel.ResumeLayout(false);
            this.settingsPage.ResumeLayout(false);
            this.routes.ResumeLayout(false);
            this.routes.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.timeoutValue)).EndInit();
            this.buttons.ResumeLayout(false);
            this.buttons.PerformLayout();
            this.ResumeLayout(false);
            this.codeSettings.ResumeLayout(false);

        }

        private System.ComponentModel.IContainer components;
    }
}
