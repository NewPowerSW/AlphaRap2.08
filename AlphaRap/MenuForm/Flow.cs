using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using AlphaRap.FlowProgramming;
using NPSDK;
namespace AlphaRap
{
    public partial class Flow : Form
    {
        private bool filling;
        private FlowModuleEditor contextModule;
        private string contextNodeId;
        private FlowModuleEditor contextPage;
        private readonly Dictionary<Flow_BaseForm, TabPage> pages = new Dictionary<Flow_BaseForm, TabPage>();
        /// <summary>Bind the engineering commands to all registered base-form instances.</summary>
        public Flow()
        {
            InitializeComponent();
            if (UiTheme.IsDesignTime) return;
            FlowLanguageForm.ApplyTexts(this);
            MiddleLayer.RegisterAndApplyLanguage(this, Name);
            UiTheme.ApplyDpi(this);
            Load += (sender, args) => { AddFlowChart(); RefreshSelection(); timer.Start(); };
            VisibleChanged += (sender, args) => { if (Visible && IsHandleCreated) AddFlowChart(); };
            MiddleLayer.LanguageChanged += ApplyLanguage;
            Disposed += (sender, args) => MiddleLayer.LanguageChanged -= ApplyLanguage;
            tabControl1.SelectedIndexChanged += (sender, args) => RefreshSelection();
            edit.CheckedChanged += ToggleEditing;
            modeValue.SelectedIndexChanged += UpdateDefinition;
            initialValue.SelectedIndexChanged += UpdateDefinition;
            autoValue.SelectedIndexChanged += UpdateDefinition;
            addModule.Click += (sender, args) => Execute(() =>
            {
                using (FlowInputForm dialog = new FlowInputForm(FlowText.T("moduleName")))
                    if (dialog.ShowDialog(this) == DialogResult.OK) { FlowPrograms.AddModule(dialog.InputValue); AddFlowChart(); RefreshSelection(); }
            });
            add.Click += (sender, args) => Execute(() => { Current().AddNode(); RefreshSelection(); });
            nodeMenu.Opening += OpenNodeMenu;
            nodeDelete.Click += (sender, args) => Execute(() => DeleteNode(contextModule, contextNodeId));
            nodeProperties.Click += (sender, args) => Execute(() => { if (contextModule != null) contextModule.EditNode(contextNodeId); });
            tabControl1.MouseDown += OpenModuleMenu;
            moduleDelete.Click += (sender, args) => Execute(() => DeleteModule(contextPage));
            ApplyLanguage(null, EventArgs.Empty);
            reload.Click += (sender, args) => Execute(() => { if (MessageBox.Show(FlowText.T("discard"), FlowText.T("title"), MessageBoxButtons.OKCancel) == DialogResult.OK) { FlowPrograms.Reload(); AddFlowChart(); RefreshSelection(); } });
            validate.Click += (sender, args) => Execute(() => { Current().Validate(null); status.Text = FlowText.T("valid"); });
            stop.Click += (sender, args) => { if (tabControl1.SelectedTab != null) Current().Runtime.Stop(); };
            pause.Click += (sender, args) => { if (tabControl1.SelectedTab != null) Current().TogglePause(); };
            schema.Click += (sender, args) => Execute(() => { FlowPrograms.RequireIdle(); using (FlowSchemaForm dialog = new FlowSchemaForm(MiddleLayer.DataF)) dialog.ShowDialog(this); });
            timer.Tick += RefreshRuntime;
        }
        /// <summary>Host every Flow_BaseForm subclass without a station-name allowlist.</summary>
        public void AddFlowChart()
        {
            FlowPrograms.Install();
            foreach (KeyValuePair<Flow_BaseForm, TabPage> item in pages.ToArray())
            {
                FlowModuleEditor editor = (FlowModuleEditor)item.Value.Tag;
                if (!item.Key.IsDisposed && !editor.Program.Removed) continue;
                item.Key.Hide();
                item.Value.Controls.Remove(item.Key);
                tabControl1.TabPages.Remove(item.Value);
                item.Value.Dispose();
                pages.Remove(item.Key);
            }
            foreach (Flow_BaseForm form in Flow_Module.FlowChart_ModuleList.ToArray())
            {
                if (form.IsDisposed || pages.ContainsKey(form)) continue;
                FlowModuleEditor editor = FlowPrograms.Attach(form);
                if (editor.Program.Removed) continue;
                TabPage page = new TabPage(form.Name) { Name = "flowModule_" + pages.Count + "_dyn", Tag = editor, AutoScroll = true };
                form.TopLevel = false; form.FormBorderStyle = FormBorderStyle.None; form.Dock = DockStyle.Fill; form.AutoScroll = true;
                page.Controls.Add(form); tabControl1.TabPages.Add(page); pages.Add(form, page);
                editor.Changed -= ModuleChanged;
                editor.Changed += ModuleChanged;
                form.Show();
            }
            foreach (Flow_BaseForm form in pages.Keys) BindNodeMenus(form);
        }

        /// <summary>复用节点右键菜单，覆盖 Flow_Chart 内部按钮的鼠标表面。</summary>
        private void BindNodeMenus(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                if (child is Flow_Chart || parent.ContextMenuStrip == nodeMenu) child.ContextMenuStrip = nodeMenu;
                BindNodeMenus(child);
            }
        }

        /// <summary>节点编辑完成后刷新列表及新节点的右键入口。</summary>
        private void ModuleChanged(object sender, EventArgs args) { RefreshSelection(); }
        /// <summary>Resolve the active module controller.</summary>
        private FlowModuleEditor Current()
        {
            if (tabControl1.SelectedTab == null) throw new InvalidOperationException("No Flow_BaseForm instance is registered.");
            return (FlowModuleEditor)tabControl1.SelectedTab.Tag;
        }
        /// <summary>Refresh node and entry pickers while keeping business identifiers stable.</summary>
        private void RefreshSelection()
        {
            if (tabControl1.SelectedTab == null)
            {
                filling = true;
                foreach (ComboBox combo in new[] { modeValue, initialValue, autoValue }) combo.DataSource = null;
                edit.Checked = false;
                filling = false;
                RefreshRuntime(null, EventArgs.Empty);
                return;
            }
            filling = true;
            FlowModuleEditor module = Current();
            modeValue.DisplayMember = "Label"; modeValue.ValueMember = "Id";
            modeValue.DataSource = new[] { "Legacy", "Program", "Disabled" }.Select(id => new FlowChoice { Id = id, Label = FlowText.T(id) }).ToList();
            modeValue.SelectedValue = module.Program.Mode;
            foreach (ComboBox combo in new[] { initialValue, autoValue })
            {
                combo.DisplayMember = "Label"; combo.ValueMember = "Id";
                combo.DataSource = new[] { new FlowChoice { Id = "", Label = FlowText.T("end") } }.Concat(module.Program.Steps.Select(step => new FlowChoice { Id = step.Id, Label = step.Caption })).ToList();
            }
            initialValue.SelectedValue = module.Program.InitialEntry ?? ""; autoValue.SelectedValue = module.Program.AutoEntry ?? "";
            initialValue.AccessibleName = FlowText.T("initialEntry"); autoValue.AccessibleName = FlowText.T("autoEntry");
            edit.Checked = module.IsEditing;
            BindNodeMenus(module.Form);
            filling = false;
            RefreshRuntime(null, EventArgs.Empty);
        }
        /// <summary>Update the engineering draft; production uses only the saved definition.</summary>
        private void UpdateDefinition(object sender, EventArgs args)
        {
            if (filling || tabControl1.SelectedTab == null) return;
            Execute(() => { FlowPrograms.RequireIdle(); Current().Program.Mode = Convert.ToString(modeValue.SelectedValue); Current().Program.InitialEntry = Convert.ToString(initialValue.SelectedValue); Current().Program.AutoEntry = Convert.ToString(autoValue.SelectedValue); });
        }
        /// <summary>Enable dragging and configuration for the active module.</summary>
        private void ToggleEditing(object sender, EventArgs args)
        {
            if (filling || tabControl1.SelectedTab == null) return;
            Execute(() => Current().SetEditing(edit.Checked));
            edit.Checked = Current().IsEditing;
        }
        /// <summary>右键菜单仅使用打开菜单时命中的节点身份，不依赖旧的下拉框选择。</summary>
        private void OpenNodeMenu(object sender, CancelEventArgs args)
        {
            contextModule = null; contextNodeId = null;
            Control source = nodeMenu.SourceControl;
            while (source != null && !(source is Flow_Chart)) source = source.Parent;
            if (source == null) { args.Cancel = true; return; }
            contextModule = FlowPrograms.Modules.FirstOrDefault(module => module.Form.Contains(source) && !module.Program.Removed);
            if (contextModule != null) contextNodeId = contextModule.IdOf((Flow_Chart)source);
            nodeDelete.Enabled = contextNodeId != null && FlowPrograms.CanEdit && !FlowPrograms.AnyRunning && SysPara.SystemMode == RunMode.IDLE && !Flow_Module.Module_GetRunningStatus;
            nodeProperties.Enabled = nodeDelete.Enabled && contextModule.IsEditing;
        }

        /// <summary>确认后删除指定节点，统一清理入口和 NEXT / CASE 连接。</summary>
        private void DeleteNode(FlowModuleEditor module, string id)
        {
            if (module == null || string.IsNullOrEmpty(id)) return;
            FlowPrograms.RequireIdle();
            FlowPrograms.ValidateNodeDeletion(module, id);
            if (MessageBox.Show(FlowText.T("remove"), FlowText.T("title"), MessageBoxButtons.OKCancel) != DialogResult.OK) return;
            module.Delete(id); RefreshSelection();
        }

        /// <summary>先检查整个空模块再确认删除，重新加载可恢复未保存的删除。</summary>
        private void DeleteModule(FlowModuleEditor module)
        {
            if (module == null) return;
            FlowPrograms.ValidateModuleDeletion(module);
            if (MessageBox.Show(string.Format(FlowText.T("removeModule"), module.Form.Name), FlowText.T("title"), MessageBoxButtons.OKCancel) != DialogResult.OK) return;
            FlowPrograms.DeleteModule(module);
            AddFlowChart(); RefreshSelection();
        }
        /// <summary>右键页签按命中位置定位模块，空白处不显示删除入口。</summary>
        private void OpenModuleMenu(object sender, MouseEventArgs args)
        {
            if (args.Button != MouseButtons.Right) return;
            contextPage = null;
            for (int index = 0; index < tabControl1.TabCount; index++)
            {
                if (!tabControl1.GetTabRect(index).Contains(args.Location)) continue;
                tabControl1.SelectedIndex = index;
                contextPage = tabControl1.TabPages[index].Tag as FlowModuleEditor;
                break;
            }
            if (contextPage == null) return;
            moduleDelete.Enabled = FlowPrograms.CanEdit && !FlowPrograms.AnyRunning && SysPara.SystemMode == RunMode.IDLE && !Flow_Module.Module_GetRunningStatus;
            moduleMenu.Show(tabControl1, args.Location);
        }
        /// <summary>Refresh availability and completion errors without rebinding editable rows.</summary>
        private void RefreshRuntime(object sender, EventArgs args)
        {
            bool busy = FlowPrograms.AnyRunning;
            bool canEdit = FlowPrograms.CanEdit;
            bool hasModule = tabControl1.SelectedTab != null;
            foreach (Control control in new Control[] { addModule, reload, schema }) control.Enabled = !busy && canEdit;
            foreach (Control control in new Control[] { edit, modeValue, initialValue, autoValue, add, validate }) control.Enabled = !busy && canEdit && hasModule;
            if (!canEdit)
            {
                foreach (FlowModuleEditor module in FlowPrograms.Modules.Where(module => module.IsEditing)) module.SetEditing(false);
                filling = true; edit.Checked = false; filling = false;
            }
            if (!canEdit) status.Text = FlowText.T("permission");
            pause.Enabled = hasModule && Current().Runtime.IsRunning; stop.Enabled = hasModule && Current().Runtime.IsRunning;
            if (!hasModule) return;
            if (Current().Runtime.Error != null) status.Text = string.Format(FlowText.T("error"), Current().Runtime.Error);
        }
        /// <summary>Translate labels without changing names or selected data references.</summary>
        private void ApplyLanguage(object sender, EventArgs args)
        {
            FlowLanguageForm.ApplyTexts(this);
            string[] words = FlowText.Defaults["delete"];
            nodeDelete.Text = MiddleLayer.LangMsg("Flow", "nodeDelete", words[0], words[1], words[2]);
            words = FlowText.Defaults["properties"];
            nodeProperties.Text = MiddleLayer.LangMsg("Flow", "nodeProperties", words[0], words[1], words[2]);
            words = FlowText.Defaults["deleteModule"];
            moduleDelete.Text = MiddleLayer.LangMsg("Flow", "moduleDelete", words[0], words[1], words[2]);
        }
        /// <summary>Keep the draft available when an engineering operation fails.</summary>
        private void Execute(Action action)
        {
            try { action(); }
            catch (Exception error) { status.Text = string.Format(FlowText.T("error"), error.Message); MessageBox.Show(status.Text, FlowText.T("title")); }
        }
    }
}
