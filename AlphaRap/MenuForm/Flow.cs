using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using AlphaRap.FlowProgramming;
using NPSDK;
namespace AlphaRap
{
    public partial class Flow : Form
    {
        private bool filling;
        private int toolboxWidth;
        private string runtimeStatus;
        private FlowModuleEditor contextModule;
        private string contextNodeId;
        private FlowModuleEditor contextPage;
        private readonly ToolStripMenuItem documentLinks = new ToolStripMenuItem();
        private readonly Dictionary<Flow_BaseForm, TabPage> pages = new Dictionary<Flow_BaseForm, TabPage>();
        /// <summary>Bind the engineering commands to all registered base-form instances.</summary>
        public Flow()
        {
            InitializeComponent();
            if (UiTheme.IsDesignTime) return;
            FlowLanguageForm.ApplyTexts(this);
            MiddleLayer.RegisterAndApplyLanguage(this, Name);
            UiTheme.ApplyDpi(this);
            toolboxWidth = workspace.SplitterDistance;
            toolboxToggle.Click += ToggleToolbox;
            status.TextChanged += (sender, args) => status.Visible = status.Text.Length > 0;
            Load += (sender, args) => { AddFlowChart(); timer.Start(); };
            VisibleChanged += (sender, args) => { if (Visible && IsHandleCreated) AddFlowChart(); };
            MiddleLayer.LanguageChanged += ApplyLanguage;
            Disposed += (sender, args) => MiddleLayer.LanguageChanged -= ApplyLanguage;
            tabControl1.SelectedIndexChanged += (sender, args) => RefreshSelection();
            edit.CheckedChanged += ToggleEditing;
            moduleEnabled.CheckedChanged += UpdateDefinition;
            initialValue.Click += (sender, args) => Execute(() => SelectEntries(true));
            autoValue.Click += (sender, args) => Execute(() => SelectEntries(false));
            addModule.Click += (sender, args) => Execute(() =>
            {
                using (FlowInputForm dialog = new FlowInputForm(FlowText.T("moduleName")))
                    if (dialog.ShowDialog(this) == DialogResult.OK) { FlowPrograms.AddModule(dialog.InputValue); AddFlowChart(); }
            });
            nodeMenu.Opening += OpenNodeMenu;
            nodeMenu.Items.Insert(0, documentLinks);
            nodeDelete.Click += (sender, args) => Execute(() => DeleteNode(contextModule, contextNodeId));
            nodeProperties.Click += (sender, args) => Execute(() => { if (contextModule != null) contextModule.EditNode(contextNodeId); });
            nodeBreakpoint.Click += (sender, args) => Execute(() => { if (contextModule != null) contextModule.ToggleBreakpoint(contextNodeId); });
            debugMode.CheckedChanged += (sender, args) =>
            {
                if (filling) return;
                Execute(() => { FlowPrograms.RequireIdle(); FlowPrograms.DebugEnabled = debugMode.Checked; });
                filling = true; debugMode.Checked = FlowPrograms.DebugEnabled; filling = false;
                RefreshRuntime(null, EventArgs.Empty);
            };
            debugPanel.ContinueRequested += (sender, args) => Execute(() => { if (FlowPrograms.CanEdit && tabControl1.SelectedTab != null) Current().Runtime.ContinueDebug(debugPanel.SelectedStepId); });
            debugPanel.StepRequested += (sender, args) => Execute(() => { if (FlowPrograms.CanEdit && tabControl1.SelectedTab != null) Current().Runtime.StepDebug(debugPanel.SelectedStepId); });
            tabControl1.MouseDown += OpenModuleMenu;
            moduleDelete.Click += (sender, args) => Execute(() => DeleteModule(contextPage));
            ApplyLanguage(null, EventArgs.Empty);
            reload.Click += (sender, args) => Execute(() => { if (MessageBox.Show(FlowText.T("discard"), FlowText.T("title"), MessageBoxButtons.OKCancel) == DialogResult.OK) { FlowPrograms.Reload(); AddFlowChart(); } });
            validate.Click += (sender, args) => Execute(() => { Current().Validate(null); status.Text = FlowText.T("valid"); });
            stop.Click += (sender, args) => MiddleLayer.StopRun();
            pause.Click += (sender, args) => { if (tabControl1.SelectedTab != null) Current().TogglePause(); };
            schema.Click += (sender, args) => Execute(() => { FlowPrograms.RequireIdle(); using (FlowSchemaForm dialog = new FlowSchemaForm(MiddleLayer.DataF)) dialog.ShowDialog(this); });
            viewSources.Click += (sender, args) => Execute(ViewGeneratedSources);
            timer.Tick += RefreshRuntime;
        }
        /// <summary>Host every Flow_BaseForm subclass without a station-name allowlist.</summary>
        public void AddFlowChart()
        {
            FlowPrograms.Install();
            filling = true;
            tabControl1.SuspendLayout();
            try
            {
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
                foreach (Flow_BaseForm form in pages.Keys) { BindNodeMenus(form); BindDropTargets(form); }
            }
            finally { filling = false; tabControl1.ResumeLayout(); }
            RefreshSelection();
        }

        /// <summary>刷新真实对象动作目录，保留工具箱搜索草稿。</summary>
        private void RefreshToolbox() { toolbox.BindActions(new FlowActions(FlowResources.Capture(true), tabControl1.SelectedTab == null ? null : Current().Form)); }

        /// <summary>收放工具箱并保留拖动后的宽度，不重建目录或改变画布节点坐标。</summary>
        private void ToggleToolbox(object sender, EventArgs args)
        {
            if (!workspace.Panel1Collapsed)
            {
                toolboxWidth = workspace.SplitterDistance;
                workspace.Panel1Collapsed = true;
                RefreshToolboxToggle();
                return;
            }
            int available = workspace.ClientSize.Width - workspace.Panel2MinSize - workspace.SplitterWidth;
            if (available < workspace.Panel1MinSize) return;
            workspace.SplitterDistance = Math.Min(toolboxWidth, available);
            workspace.Panel1Collapsed = false;
            RefreshToolboxToggle();
        }

        /// <summary>工具箱图标按展开状态高亮，悬停说明和无障碍文字沿用语言切换。</summary>
        private void RefreshToolboxToggle()
        {
            string description = FlowToolboxText.T(workspace.Panel1Collapsed ? "showToolbox" : "hideToolbox");
            toolboxToggle.AccessibleName = FlowToolboxText.T("toolbox");
            toolboxToggle.AccessibleDescription = description;
            sourceHint.SetToolTip(toolboxToggle, description);
            toolboxToggle.BackColor = workspace.Panel1Collapsed ? toolboxRail.BackColor : UiKit.BrandSoft;
            toolboxToggle.Image = AppIcons.Get(AppIcon.Toolbox, UiTheme.Dpi(24), UiKit.Brand);
        }

        /// <summary>为所有实际基类页面中的容器接入同一工具箱拖放入口。</summary>
        private void BindDropTargets(Control parent)
        {
            if (parent is Flow_Chart) return;
            if (parent is ScrollableControl && !(parent is ToolStrip))
            {
                parent.AllowDrop = true;
                parent.DragEnter -= ToolboxDragOver; parent.DragEnter += ToolboxDragOver;
                parent.DragOver -= ToolboxDragOver; parent.DragOver += ToolboxDragOver;
                parent.DragDrop -= ToolboxDrop; parent.DragDrop += ToolboxDrop;
            }
            foreach (Control child in parent.Controls) BindDropTargets(child);
        }

        /// <summary>仅在空闲编辑的实际画布接受工具箱条目。</summary>
        private void ToolboxDragOver(object sender, DragEventArgs args)
        {
            Control canvas = (Control)sender;
            FlowModuleEditor module = FlowPrograms.Modules.FirstOrDefault(item => canvas == item.Form || item.Form.Contains(canvas));
            args.Effect = args.Data.GetDataPresent(FlowToolboxControl.DragFormat) && module != null && module.IsEditing && !module.Program.Removed && FlowPrograms.CanEdit && !FlowPrograms.AnyRunning && SysPara.SystemMode == RunMode.IDLE ? DragDropEffects.Copy : DragDropEffects.None;
        }

        /// <summary>将屏幕落点换算成该容器的逻辑坐标，滚动后仍落在鼠标处。</summary>
        private void ToolboxDrop(object sender, DragEventArgs args)
        {
            if (args.Effect != DragDropEffects.Copy) return;
            FlowToolboxEntry entry = args.Data.GetData(FlowToolboxControl.DragFormat) as FlowToolboxEntry;
            if (entry == null) return;
            Control canvas = (Control)sender;
            Point point = canvas.PointToClient(new Point(args.X, args.Y));
            ScrollableControl scrolling = canvas as ScrollableControl;
            if (scrolling != null) point.Offset(-scrolling.AutoScrollPosition.X, -scrolling.AutoScrollPosition.Y);
            Execute(() => InsertToolboxEntry(entry, canvas, point));
        }

        /// <summary>按拖放位置新增已绑定对象的节点，不创建替身硬件对象。</summary>
        private void InsertToolboxEntry(FlowToolboxEntry entry, Control canvas, Point point)
        {
            FlowModuleEditor module = FlowPrograms.Modules.FirstOrDefault(item => canvas == item.Form || item.Form.Contains(canvas));
            if (module == null || !module.IsEditing) throw new InvalidOperationException(FlowText.T("edit"));
            point = new Point(Math.Max(0, point.X), Math.Max(0, point.Y));
            if (entry.ExecutionMode == "None") module.AddNode("None", canvas, point);
            else if (entry.ExecutionMode == "Action") module.AddNode(entry.TargetId, entry.ActionId, canvas, point);
            else throw new InvalidOperationException(FlowText.T("retiredNodeChoice"));
        }

        /// <summary>在应用内只读查看已生成源码，不开放文件修改或删除入口。</summary>
        private void ViewGeneratedSources()
        {
            using (FlowSourceViewer viewer = new FlowSourceViewer(FlowSourceGenerator.ReadGeneratedSources(), FlowSourceGenerator.OutputDirectory)) viewer.ShowDialog(this);
        }

        /// <summary>多选当前阶段需要同时启动的入口，确定仅修改内存草稿。</summary>
        private void SelectEntries(bool initializing)
        {
            FlowPrograms.RequireIdle();
            using (FlowEntryForm dialog = new FlowEntryForm(Current(), initializing)) dialog.ShowDialog(this);
            RefreshSelection(false);
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
        private void ModuleChanged(object sender, EventArgs args)
        {
            FlowModuleEditor module = (FlowModuleEditor)sender;
            BindNodeMenus(module.Form);
            if (tabControl1.SelectedTab != null && ReferenceEquals(Current(), module)) RefreshSelection(false);
        }
        /// <summary>Resolve the active module controller.</summary>
        private FlowModuleEditor Current()
        {
            if (tabControl1.SelectedTab == null) throw new InvalidOperationException("No Flow_BaseForm instance is registered.");
            return (FlowModuleEditor)tabControl1.SelectedTab.Tag;
        }
        /// <summary>Refresh node and entry pickers while keeping business identifiers stable.</summary>
        private void RefreshSelection(bool refreshTools = true)
        {
            if (filling) return;
            if (tabControl1.SelectedTab == null)
            {
                filling = true;
                initialValue.Text = autoValue.Text = FlowText.T("chooseEntries");
                moduleEnabled.Checked = false;
                edit.Checked = false;
                filling = false;
                RefreshRuntime(null, EventArgs.Empty);
                if (refreshTools) RefreshToolbox();
                return;
            }
            filling = true;
            FlowModuleEditor module = Current();
            moduleEnabled.Checked = module.Program.Mode != "Disabled";
            RefreshEntryButton(initialValue, module.Program.InitialEntries, module, true);
            RefreshEntryButton(autoValue, module.Program.AutoEntries, module, false);
            edit.Checked = module.IsEditing;
            BindNodeMenus(module.Form);
            filling = false;
            RefreshRuntime(null, EventArgs.Empty);
            if (refreshTools) RefreshToolbox();
        }
        /// <summary>Update the engineering draft; production uses only the saved definition.</summary>
        private void UpdateDefinition(object sender, EventArgs args)
        {
            if (filling || tabControl1.SelectedTab == null) return;
            Execute(() =>
            {
                FlowPrograms.RequireIdle();
                FlowProgram program = Current().Program;
                program.Mode = moduleEnabled.Checked ? "Program" : "Disabled";
            });
        }

        /// <summary>入口按钮显示选中数量，悬停列出全部入口名称。</summary>
        private void RefreshEntryButton(Button button, IList<string> entries, FlowModuleEditor module, bool initializing)
        {
            button.Text = entries.Count == 0 ? FlowText.T("chooseEntries") : string.Format(FlowText.T("selectedEntries"), entries.Count);
            button.AccessibleName = FlowText.T(initializing ? "initialFlows" : "automaticFlows");
            sourceHint.SetToolTip(button, string.Join(Environment.NewLine, entries.Select(id => module.Program.Steps.Where(step => step.Id == id).Select(step => step.Caption).FirstOrDefault() ?? id)));
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
            FlowStep step = contextModule == null ? null : contextModule.Program.Steps.SingleOrDefault(item => item.Id == contextNodeId);
            nodeDelete.Enabled = contextNodeId != null && FlowPrograms.CanEdit && !FlowPrograms.AnyRunning && SysPara.SystemMode == RunMode.IDLE && !Flow_Module.Module_GetRunningStatus;
            nodeProperties.Available = step != null && !step.UsesFlowResult;
            nodeProperties.Enabled = nodeProperties.Available && nodeDelete.Enabled && contextModule.IsEditing;
            nodeBreakpoint.Enabled = nodeDelete.Enabled;
            nodeBreakpoint.Checked = step != null && step.Breakpoint;
            documentLinks.Text = FlowText.T("documentLinks");
            documentLinks.Available = step != null && step.UsesFlowResult && !string.IsNullOrEmpty(step.PageId);
            documentLinks.Enabled = documentLinks.Available && nodeDelete.Enabled && contextModule.IsEditing;
            documentLinks.DropDownItems.Clear();
            if (!documentLinks.Available) return;
            foreach (string exit in new[] { "NEXT", "CASE1", "CASE2", "CASE3" })
            {
                ToolStripMenuItem route = new ToolStripMenuItem(exit);
                route.DropDownItems.Add(ConnectionChoice(step, exit, null, FlowText.T("end")));
                foreach (FlowStep target in contextModule.Program.Steps)
                    route.DropDownItems.Add(ConnectionChoice(step, exit, target.Id, target.Caption));
                documentLinks.DropDownItems.Add(route);
            }
        }

        /// <summary>连接菜单以稳定节点 ID 提交，不修改代码绑定及输入配置。</summary>
        private ToolStripMenuItem ConnectionChoice(FlowStep step, string exit, string target, string caption)
        {
            FlowModuleEditor module = contextModule;
            ToolStripMenuItem item = new ToolStripMenuItem(caption) { Checked = step.TargetOf(exit) == target };
            item.Click += (sender, args) => Execute(() => module.SetConnection(step.Id, exit, target));
            return item;
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
            AddFlowChart();
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
            FlowModuleEditor module = tabControl1.SelectedTab == null ? null : Current();
            bool hasModule = module != null;
            debugMode.Enabled = !busy && canEdit && hasModule && SysPara.SystemMode == RunMode.IDLE;
            foreach (Control control in new Control[] { addModule, reload, schema }) control.Enabled = !busy && canEdit;
            foreach (Control control in new Control[] { edit, moduleEnabled, initialValue, autoValue, validate }) control.Enabled = !busy && canEdit && hasModule;
            toolbox.Enabled = !busy && canEdit && hasModule && module.IsEditing && SysPara.SystemMode == RunMode.IDLE;
            if (!canEdit)
            {
                foreach (FlowModuleEditor editingModule in FlowPrograms.Modules.Where(item => item.IsEditing)) editingModule.SetEditing(false);
                filling = true; edit.Checked = false; filling = false;
                if (!busy) FlowPrograms.DebugEnabled = false;
            }
            pause.Enabled = stop.Enabled = hasModule && module.Runtime.IsRunning;
            filling = true; debugMode.Checked = FlowPrograms.DebugEnabled; filling = false;
            debugPanel.Visible = canEdit && (FlowPrograms.DebugEnabled || hasModule && module.Runtime.IsDebugging);
            debugPanel.Bind(module);
            if (hasModule && module.Runtime.IsDebugPaused) pause.Enabled = false;
            if (hasModule && module.IsInitializing && module.Runtime.IsPaused) pause.Enabled = false;
            if (!hasModule) return;
            module.RefreshElapsed();
            if (module.ExecutionError != null) { runtimeStatus = string.Format(FlowText.T("error"), module.ExecutionError); status.Text = runtimeStatus; }
            else if (runtimeStatus != null) { if (status.Text == runtimeStatus) status.Text = ""; runtimeStatus = null; }
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
            words = FlowText.Defaults["nodeBreakpoint"];
            nodeBreakpoint.Text = MiddleLayer.LangMsg("Flow", "nodeBreakpoint", words[0], words[1], words[2]);
            debugPanel.RefreshLanguage();
            viewSources.Text = FlowToolboxText.Fixed(viewSources, "sourceDirectory");
            sourceHint.SetToolTip(viewSources, FlowSourceGenerator.ModeDescription);
            viewSources.AccessibleDescription = FlowSourceGenerator.ModeDescription;
            RefreshToolboxToggle();
            RefreshSelection(false);
        }
        /// <summary>Keep the draft available when an engineering operation fails.</summary>
        private void Execute(Action action)
        {
            try { action(); }
            catch (Exception error) { status.Text = string.Format(FlowText.T("error"), error.Message); MessageBox.Show(status.Text, FlowText.T("title")); }
        }
    }
}
