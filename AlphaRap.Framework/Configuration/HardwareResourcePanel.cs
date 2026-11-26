using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using AlphaRap.FlowProgramming;
using NPSDK.Motion;

namespace AlphaRap.Configuration
{
    public partial class HardwareResourcePanel : UserControl
    {
        public HardwareResources Resources { get; private set; }
        private string contextResourceId;
        private TabPage contextAxisTab;
        private string contextIoPageId;
        private bool refreshingTabs;
        private string resourceKind = "Axis";
        [DefaultValue("Axis")]
        public string ResourceKind
        {
            get { return resourceKind; }
            set { resourceKind = value; ApplyKind(); }
        }

        /// <summary>初始化可在设计器预览的资源管理工具栏。</summary>
        public HardwareResourcePanel()
        {
            InitializeComponent();
            canvas.Resize += (sender, args) => ArrangeIo();
            ApplyKind();
            axisTabs.SelectedIndexChanged += (sender, args) => edit.Enabled = axisTabs.SelectedTab != null;
            axisTabs.MouseDown += SelectContextTab;
            ioTabs.SelectedIndexChanged += (sender, args) => ShowIoPage();
            ioTabs.MouseDown += SelectIoContextTab;
            if (UiTheme.IsDesignTime) return;
            HardwareText.Apply(this);
            foreach (Control control in new Control[] { toolbar, status, hint }) FrameworkLanguage.RegisterAndApplyLanguage(control,"HardwareResources");
            FrameworkLanguage.LanguageChanged += ApplyLanguage;
            Disposed += (sender,args) => FrameworkLanguage.LanguageChanged -= ApplyLanguage;
            addAxis.Click += (sender,args) => Execute(()=>EditNew("Axis"));
            addInput.Click += (sender,args) => Execute(()=>EditNew("Input"));
            addOutput.Click += (sender,args) => Execute(()=>EditNew("Output"));
            addCylinder.Click += (sender,args) => Execute(() => EditCylinder(new HardwareCylinderDefinition()));
            edit.Click += (sender,args) => Execute(()=>EditEntry(Selected()));
            resourceMenu.Opening += OpenResourceMenu;
            resourceDelete.Click += (sender,args) => Execute(DeleteEntry);
            resourceProperties.Click += (sender,args) => Execute(EditContextEntry);
            resourceSource.Click += (sender, args) => Execute(() => ShowConfigurationSource(contextResourceId));
            findConfiguration.Click += (sender, args) => Execute(() => ShowConfigurationSource(null));
            addIoPage.Click += (sender,args) => Execute(() => EditIoPageName(null));
            ioPageAdd.Click += (sender,args) => Execute(() => EditIoPageName(null));
            ioPageRename.Click += (sender,args) => Execute(() => EditIoPageName(contextIoPageId));
            ioPageText.Click += (sender,args) => Execute(EditIoPageText);
            ioPageDelete.Click += (sender,args) => Execute(DeleteIoPage);
            ioPageMenu.Opening += OpenIoPageMenu;
        }

        /// <summary>在所属硬件模块初始化后，加载资源并绑定当前配方点位表。</summary>
        public void BindOwner(HardForm owner)
        {
            if (Resources != null) return;
            Resources = new HardwareResources(owner,canvas);
            BindEvents();
        }

        /// <summary>IO 页面复用轴页面创建的资源目录，不重复实例化驱动。</summary>
        public void BindResources(HardwareResources resources)
        {
            if (Resources != null || resources == null) return;
            Resources = resources;
            Resources.BindIoCanvas(canvas);
            BindEvents();
        }

        /// <summary>移除本面板的交互事件，并在面板销毁前归还共享硬件。</summary>
        private void ReleaseResources()
        {
            HardwareResources resources = Resources;
            if (resources == null) return;
            Resources = null;
            resources.Changed -= ResourcesChanged;
            if (resourceKind != "IO") return;
            foreach (NP_IoGroup group in canvas.Controls.OfType<NP_IoGroup>())
            {
                group.ParametersRequested -= GroupSettings;
                ApplyResourceMenu(group, null);
            }
            foreach (Control card in canvas.Controls.Cast<Control>().Where(item => item.Tag is HardwareCylinderDefinition))
                ApplyResourceMenu(card, null);
            resources.UnbindIoCanvas(canvas);
        }

        /// <summary>绑定目录变更并挂接当前页面所属资源的交互。</summary>
        private void BindEvents()
        {
            Resources.Changed += ResourcesChanged;
            foreach (HardwareEntry entry in Resources.Entries.Where(BelongsHere)) Attach(entry);
            RefreshEntries(); ApplyLanguage(null,EventArgs.Empty);
            if (resourceKind == "IO")
            {
                string initialPage = Resources.IoPages.FirstOrDefault()?.Id;
                TabPage tab = ioTabs.TabPages.Cast<TabPage>().FirstOrDefault(page => Equals(page.Tag, initialPage));
                if (tab != null) ioTabs.SelectedTab = tab;
            }
        }

        /// <summary>按页面用途筛选轴或 IO，业务资源仍由同一个目录管理。</summary>
        private bool BelongsHere(HardwareEntry entry) { return ResourceKind=="IO" ? entry.Definition.Kind!="Axis" : entry.Definition.Kind=="Axis"; }

        /// <summary>固定模板在设计器和运行时使用同一轴或 IO 操作模式。</summary>
        private void ApplyKind()
        {
            if (addAxis == null) return;
            addAxis.Visible = resourceKind!="IO";
            addInput.Visible = resourceKind=="IO"; addOutput.Visible = resourceKind=="IO";
            addIoPage.Visible = resourceKind=="IO";
            addCylinder.Visible = resourceKind=="IO";
            edit.Visible = resourceKind!="IO";
            axisTabs.Visible = resourceKind!="IO"; ioTabs.Visible = resourceKind=="IO"; canvas.Visible = resourceKind=="IO";
            toolbar.Height = 48;
            hint.Height = resourceKind == "IO" ? 64 : 48;
            if (Resources != null) RefreshEntries();
            hint.Name = resourceKind=="IO" ? "ioResourceHint" : "resourceHint"; hint.Tag = resourceKind=="IO" ? "ioHint" : "hint";
            hint.Text = HardwareText.T(resourceKind=="IO" ? "ioHint" : "hint");
            ArrangeIo();
        }

        /// <summary>参数按钮始终定位当前轴选项卡，业务名称不作为身份。</summary>
        private HardwareEntry Selected()
        {
            HardwareDefinition definition = axisTabs.SelectedTab == null ? null : axisTabs.SelectedTab.Tag as HardwareDefinition;
            HardwareEntry entry = Resources?.Find(definition?.Id);
            if (entry == null) throw new InvalidOperationException(HardwareText.T("missing"));
            return entry;
        }

        /// <summary>先编辑资源草稿，确定后才创建驱动对象及组合控件。</summary>
        private void EditNew(string kind)
        {
            FlowPrograms.RequireIdle();
            if (Resources == null) throw new InvalidOperationException(HardwareText.T("missing"));
            HardwareDefinition definition = Resources.NewDefinition(kind);
            if (kind != "Axis" && ioTabs.SelectedTab != null) definition.IoPageId = (string)ioTabs.SelectedTab.Tag;
            using (HardwareEditForm dialog = new HardwareEditForm(definition,value=>Resources.Put(value))) dialog.ShowDialog(TopLevelControl);
        }

        /// <summary>双击组合控件和工具栏参数按钮共用同一编辑入口。</summary>
        private void EditEntry(HardwareEntry entry)
        {
            FlowPrograms.RequireIdle();
            using (HardwareEditForm dialog = new HardwareEditForm(entry.Definition,value=>{ Resources.Put(value); if (Resources.RequiresRestart) status.Text = HardwareText.T("restart"); })) dialog.ShowDialog(this);
        }

        /// <summary>配置一组气缸的名称及两路输出，并显示到 ON 输出所在集合。</summary>
        private void EditCylinder(HardwareCylinderDefinition definition)
        {
            FlowPrograms.RequireIdle();
            if (Resources == null) throw new InvalidOperationException(HardwareText.T("missing"));
            using (var dialog = new HardwareCylinderEditForm(Resources, definition))
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
            HardwareCylinderEntry saved = Resources.FindCylinder(definition.Id);
            string pageId = Resources.FindCylinderOutput(saved.Definition.OnOutputId)?.Definition.IoPageId;
            TabPage page = ioTabs.TabPages.Cast<TabPage>().FirstOrDefault(item => Equals(item.Tag, pageId));
            if (page != null) ioTabs.SelectedTab = page;
        }

        /// <summary>检查当前资源后确认删除，服务层再次检查流程引用和占用。</summary>
        private void DeleteEntry()
        {
            FlowPrograms.RequireIdle();
            HardwareCylinderEntry cylinder = Resources.FindCylinder(contextResourceId);
            if (cylinder != null)
            {
                if (MessageBox.Show(this, string.Format(HardwareText.T("cylinderRemove"), cylinder.Card.Text),
                    HardwareText.T("editCylinder"), MessageBoxButtons.OKCancel) == DialogResult.OK) Resources.DeleteCylinder(contextResourceId);
                return;
            }
            HardwareEntry entry = Resources.Find(contextResourceId);
            if (entry == null) return;
            if (MessageBox.Show(string.Format(HardwareText.T("remove"),entry.Definition.Name),HardwareText.T("title"),MessageBoxButtons.OKCancel) != DialogResult.OK) return;
            Resources.Delete(entry.Definition.Id);
        }

        /// <summary>右键属性编辑始终打开当前命中的组合对象，与工具栏选择无关。</summary>
        private void EditContextEntry()
        {
            FlowPrograms.RequireIdle();
            HardwareCylinderEntry cylinder = Resources.FindCylinder(contextResourceId);
            if (cylinder != null) { EditCylinder(cylinder.Definition); return; }
            HardwareEntry entry = Resources.Find(contextResourceId);
            if (entry != null) EditEntry(entry);
        }

        /// <summary>在全部 IO 选项卡或轴页签中按配置名称或稳定身份查找资源。</summary>
        private void ShowConfigurationSource(string selectedId)
        {
            if (Resources == null) throw new InvalidOperationException(HardwareText.T("missing"));
            var sources = Resources.Entries.Where(BelongsHere).Select(entry =>
            {
                HardwareDefinition definition = entry.Definition;
                return new ConfigurationSourceEntry(definition.Id, definition.Name, () => DescribeConfiguration(definition));
            });
            if (resourceKind == "IO")
                sources = sources.Concat(Resources.CylinderEntries.Select(entry => new ConfigurationSourceEntry(entry.Definition.Id,
                    HardwareResources.DisplayText(entry.Definition), () => DescribeCylinder(entry.Definition))));
            using (var dialog = new ConfigurationSourceForm(Resources.PathName, sources, selectedId, Resources.LoadFailure))
                dialog.ShowDialog(this);
        }

        /// <summary>显示组合的两个引用及工程名称，便于从界面定位 XML。</summary>
        private string DescribeCylinder(HardwareCylinderDefinition definition)
        {
            string on = Resources.FindCylinderOutput(definition.OnOutputId)?.Definition.Name ?? HardwareText.T("cylinderMissing");
            string off = Resources.FindCylinderOutput(definition.OffOutputId)?.Definition.Name ?? HardwareText.T("cylinderMissing");
            return string.Join(Environment.NewLine, new[] { definition.Name, "ID  " + definition.Id,
                HardwareText.T("cylinderOn") + "  " + on + " [" + definition.OnOutputId + "]",
                HardwareText.T("cylinderOff") + "  " + off + " [" + definition.OffOutputId + "]" });
        }

        /// <summary>显示配置值和归属；只访问内存定义，不读取硬件信号。</summary>
        private string DescribeConfiguration(HardwareDefinition definition)
        {
            string scope = ConfigurationSourceForm.T("machineScope", "作用域　Setting（机器）", "Scope  Setting (machine)", "Ámbito  Setting (máquina)");
            string kind = definition.Kind == "Axis" ? HardwareText.Translate("configurationAxis", "轴", "Axis", "Eje")
                : FlowText.T(definition.Kind == "Input" ? "di" : "do");
            string tab = definition.Kind == "Axis" ? "" : Resources.IoPages.FirstOrDefault(page => page.Id == definition.IoPageId)?.Name;
            string heading = definition.Name + "  ·  " + kind;
            if (!string.IsNullOrEmpty(tab)) heading += "  ·  " + tab;
            var lines = new[]
            {
                heading, "ID  " + definition.Id, scope,
                Resources.IsDirty ? ConfigurationSourceForm.T("draft", "有未保存修改。以下为当前配置。", "Unsaved changes. Current configuration shown.", "Hay cambios sin guardar. Se muestra la configuración actual.") : "",
                Resources.RequiresRestart ? HardwareText.T("restart") : "", ""
            };
            var values = definition.Fields().Where(field => field != "Name" && field != "ControlType").Select(field =>
            {
                string label = field == "Port" ? (definition.Kind == "Axis" ? "AxisPort" : "IoPort") : field;
                object value = typeof(HardwareDefinition).GetField(field).GetValue(definition);
                return HardwareText.T(label) + "  " + Convert.ToString(value, CultureInfo.InvariantCulture);
            });
            return string.Join(Environment.NewLine, lines.Concat(values));
        }

        /// <summary>资源增删改后更新组合控件事件及选择列表，避免外部调用服务后界面失效。</summary>
        private void ResourcesChanged(object sender,EventArgs args)
        {
            foreach (HardwareEntry entry in Resources.Entries.Where(BelongsHere)) Attach(entry);
            RefreshEntries();
        }

        /// <summary>为新组合控件挂接双击设置和框架三语刷新。</summary>
        private void Attach(HardwareEntry entry)
        {
            BindResourceMenu(entry.Group);
            NP_AxisGroup axis = entry.Group as NP_AxisGroup;
            NP_IoGroup io = entry.Group as NP_IoGroup;
            if (axis != null) { axis.ParametersRequested -= GroupSettings; axis.ParametersRequested += GroupSettings; axis.ApplyLanguage(HardwareText.Translate); }
            if (io != null) { io.ParametersRequested -= GroupSettings; io.ParametersRequested += GroupSettings; IoTileStyle.Style(io.IO); IoTileStyle.PlaceIndicator(io.IO); }
        }

        /// <summary>轴和 IO 表面共用属性及删除菜单，点位表保留自己的行菜单。</summary>
        private void BindResourceMenu(Control control) { ApplyResourceMenu(control, resourceMenu); }

        /// <summary>同步资源卡的右键菜单，保留点位表自身菜单。</summary>
        private static void ApplyResourceMenu(Control control, ContextMenuStrip menu)
        {
            if (control is NP_PositionTable) return;
            control.ContextMenuStrip = menu;
            foreach (Control child in control.Controls) ApplyResourceMenu(child, menu);
        }

        /// <summary>按右键控件的资源身份定位，不依赖工具栏上次选中的对象。</summary>
        private void OpenResourceMenu(object sender, CancelEventArgs args)
        {
            contextResourceId = null;
            Control control = resourceMenu.SourceControl;
            if (control == axisTabs) control = contextAxisTab;
            while (control != null && !(control.Tag is HardwareDefinition) && !(control.Tag is HardwareCylinderDefinition)) control = control.Parent;
            var cylinder = control?.Tag as HardwareCylinderDefinition;
            if (cylinder != null)
            {
                contextResourceId = cylinder.Id;
                resourceProperties.Enabled = FlowPrograms.CanEdit && !FlowPrograms.ConfigurationBusy;
                resourceDelete.Enabled = resourceProperties.Enabled && string.IsNullOrEmpty(cylinder.StationKey);
                resourceMove.Visible = false;
                return;
            }
            HardwareDefinition definition = control == null ? null : control.Tag as HardwareDefinition;
            if (definition == null || Resources == null) { args.Cancel = true; return; }
            contextResourceId = definition.Id;
            resourceDelete.Enabled = FlowPrograms.CanEdit && !FlowPrograms.AnyRunning && SysPara.SystemMode == RunMode.IDLE && !NPSDK.Flow_Module.Module_GetRunningStatus;
            resourceProperties.Enabled = resourceDelete.Enabled;
            RefreshMoveMenu(definition, resourceDelete.Enabled);
        }

        /// <summary>按发送组合控件定位资源，不使用旧的下拉框选择。</summary>
        private void GroupSettings(object sender,EventArgs args)
        {
            var definition = (sender as Control)?.Tag as HardwareDefinition;
            HardwareEntry entry = Resources?.Find(definition?.Id);
            if (entry != null && ReferenceEquals(entry.Group, sender)) Execute(() => EditEntry(entry));
        }

        /// <summary>右键命中轴页签后选择该页，空白处不借用上次右键的资源。</summary>
        private void SelectContextTab(object sender, MouseEventArgs args)
        {
            if (args.Button != MouseButtons.Right) return;
            contextAxisTab = null;
            for (int index = 0; index < axisTabs.TabCount; index++)
            {
                if (!axisTabs.GetTabRect(index).Contains(args.Location)) continue;
                contextAxisTab = axisTabs.TabPages[index];
                axisTabs.SelectedTab = contextAxisTab;
                return;
            }
        }

        /// <summary>资源变更只同步所属页面；语言和配方切换不重建轴实例。</summary>
        private void RefreshEntries()
        {
            if (resourceKind == "IO") { RefreshIoTabs(); return; }
            RefreshAxisTabs();
        }

        /// <summary>每个轴使用一个稳定身份页签，保留当前选择并让新建轴直接显示。</summary>
        private void RefreshAxisTabs()
        {
            if (refreshingTabs) return;
            refreshingTabs = true;
            axisTabs.SuspendLayout();
            try
            {
                HardwareEntry[] axes = Resources.Entries.Where(entry => entry.Definition.Kind == "Axis").ToArray();
                TabPage selection = axisTabs.SelectedTab;
                bool hadTabs = axisTabs.TabCount > 0;
                foreach (TabPage page in axisTabs.TabPages.Cast<TabPage>().ToArray())
                {
                    HardwareDefinition definition = page.Tag as HardwareDefinition;
                    if (definition != null && axes.Any(entry => entry.Definition.Id == definition.Id)) continue;
                    if (ReferenceEquals(contextAxisTab, page)) contextAxisTab = null;
                    axisTabs.TabPages.Remove(page); page.Dispose();
                }
                foreach (HardwareEntry entry in axes)
                {
                    TabPage page = axisTabs.TabPages.Cast<TabPage>().FirstOrDefault(item => ((HardwareDefinition)item.Tag).Id == entry.Definition.Id);
                    if (page == null)
                    {
                        page = new TabPage { Name = "axisTab_" + entry.Definition.Id + "_dyn", AutoScroll = true, Padding = Padding.Empty, UseVisualStyleBackColor = true };
                        axisTabs.TabPages.Add(page);
                        if (hadTabs || axes.Length == 1) selection = page;
                    }
                    page.Tag = entry.Definition; page.Text = HardwareResources.DisplayText(entry.Definition); page.ToolTipText = entry.Definition.Name;
                    if (entry.Group.Parent == page) continue;
                    entry.Group.Dock = DockStyle.Fill; entry.Group.Margin = Padding.Empty;
                    page.AutoScrollMinSize = entry.Group.MinimumSize + page.Padding.Size;
                    page.Controls.Add(entry.Group); entry.Group.Visible = true;
                }
                if (selection != null && axisTabs.TabPages.Contains(selection)) axisTabs.SelectedTab = selection;
                else if (axisTabs.SelectedTab == null && axisTabs.TabCount > 0) axisTabs.SelectedTab = axisTabs.TabPages[0];
                edit.Enabled = axisTabs.SelectedTab != null;
            }
            finally { axisTabs.ResumeLayout(); refreshingTabs = false; }
        }

        /// <summary>输入和输出在同一画布分区平铺，卡片使用原 IO 外观而不增加可见外框。</summary>
        private void ArrangeIo()
        {
            if (Resources == null || resourceKind != "IO") return;
            int width = System.Math.Max(240,canvas.ClientSize.Width - canvas.Padding.Horizontal - SystemInformation.VerticalScrollBarWidth);
            int columns = System.Math.Max(1,width / 275);
            int tileWidth = System.Math.Min(360,width / columns - 10);
            int position = 0;
            canvas.SuspendLayout();
            foreach (string kind in new[] { "Input","Output" })
            {
                string pageId = ioTabs.SelectedTab == null ? null : ioTabs.SelectedTab.Tag as string;
                HardwareEntry[] entries = Resources.Entries.Where(entry => entry.Definition.Kind == kind && entry.Definition.IoPageId == pageId).ToArray();
                Label header = kind == "Input" ? inputHeader : outputHeader;
                header.Text = FlowText.T(kind == "Input" ? "di" : "do") + " · " + entries.Length;
                header.Visible = entries.Length > 0; header.Width = width - 6;
                canvas.Controls.SetChildIndex(header,position++);
                foreach (HardwareEntry entry in Resources.Entries.Where(entry => entry.Definition.Kind == kind)) entry.Group.Visible = entry.Definition.IoPageId == pageId;
                foreach (HardwareEntry entry in entries)
                {
                    entry.Device.Text = HardwareResources.DisplayText(entry.Definition);
                    if (entry.Definition.ConnectionPending) entry.Device.Text += " · " + HardwareText.T("ConnectionPending");
                    entry.Group.Size = new System.Drawing.Size(tileWidth,40);
                    canvas.Controls.SetChildIndex(entry.Group,position++);
                    IoTileStyle.PlaceIndicator(entry.Device);
                }
            }
            ArrangeIoActions(tileWidth, position);
            canvas.ResumeLayout();
        }

        /// <summary>动态生成的气缸卡跟随 ON 输出分组，并显示缺失的绑定。</summary>
        private void ArrangeIoActions(int tileWidth, int position)
        {
            string pageId = ioTabs.SelectedTab?.Tag as string;
            foreach (HardwareCylinderEntry entry in Resources.CylinderEntries)
            {
                if (entry.Card.Parent != canvas)
                {
                    BindResourceMenu(entry.Card);
                    canvas.Controls.Add(entry.Card);
                }
                HardwareEntry on = Resources.FindCylinderOutput(entry.Definition.OnOutputId);
                HardwareEntry off = Resources.FindCylinderOutput(entry.Definition.OffOutputId);
                string group = on?.Definition.IoPageId ?? off?.Definition.IoPageId ?? Resources.IoPages.FirstOrDefault()?.Id;
                entry.Card.Text = HardwareResources.DisplayText(entry.Definition);
                if (on == null || off == null) entry.Card.Text += " · " + HardwareText.T("cylinderMissing");
                else if (on.Definition.ConnectionPending || off.Definition.ConnectionPending)
                    entry.Card.Text += " · " + HardwareText.T("ConnectionPending");
                bool visible = group == pageId;
                entry.Card.Visible = visible;
                if (visible)
                {
                    entry.Card.Width = tileWidth;
                    canvas.Controls.SetChildIndex(entry.Card, position++);
                }
            }
        }

        /// <summary>按稳定分组身份同步 IO 页签，移除旧页之前保留共享画布和实际 IO。</summary>
        private void RefreshIoTabs()
        {
            if (refreshingTabs) return;
            refreshingTabs = true;
            ioTabs.SuspendLayout();
            try
            {
                HardwareIoPage[] pages = Resources.IoPages.ToArray();
                TabPage selection = ioTabs.SelectedTab;
                foreach (TabPage page in ioTabs.TabPages.Cast<TabPage>().ToArray())
                {
                    if (pages.Any(item => item.Id == page.Tag as string)) continue;
                    if (canvas.Parent == page) Controls.Add(canvas);
                    ioTabs.TabPages.Remove(page); page.Dispose();
                }
                foreach (HardwareIoPage definition in pages)
                {
                    TabPage page = ioTabs.TabPages.Cast<TabPage>().FirstOrDefault(item => item.Tag as string == definition.Id);
                    if (page == null)
                    {
                        page = new TabPage { Name="ioTab_"+definition.Id+"_dyn", Tag=definition.Id, UseVisualStyleBackColor=true };
                        ioTabs.TabPages.Add(page);
                        selection = page;
                    }
                    page.Text = HardwareResources.DisplayText(definition); page.ToolTipText = definition.Name;
                }
                if (selection != null && ioTabs.TabPages.Contains(selection)) ioTabs.SelectedTab = selection;
                else if (ioTabs.TabCount > 0) ioTabs.SelectedIndex = 0;
            }
            finally { ioTabs.ResumeLayout(); refreshingTabs = false; }
            ShowIoPage();
        }

        /// <summary>切换显示归属只搬动画布，复用同一批 IO 控件且不执行读写。</summary>
        private void ShowIoPage()
        {
            if (refreshingTabs || resourceKind != "IO" || ioTabs.SelectedTab == null) return;
            if (canvas.Parent != ioTabs.SelectedTab) ioTabs.SelectedTab.Controls.Add(canvas);
            ArrangeIo();
        }

        /// <summary>复用输入窗口编辑页签业务名称，确定后仅修改资源草稿。</summary>
        private void EditIoPageName(string id)
        {
            FlowPrograms.RequireIdle();
            if (Resources == null) throw new InvalidOperationException(HardwareText.T("missing"));
            HardwareIoPage page = id == null ? null : Resources.IoPages.FirstOrDefault(item => item.Id == id);
            if (id != null && page == null) throw new InvalidOperationException(HardwareText.T("ioPageMissing"));
            int index = 1;
            while (Resources.IoPages.Any(item => string.Equals(item.Name, "IO_"+index, StringComparison.OrdinalIgnoreCase))) index++;
            using (FlowInputForm dialog = new FlowInputForm(() => HardwareText.T("ioPageName")))
            {
                dialog.InputValue = page == null ? "IO_"+index : page.Name;
                if (dialog.ShowDialog(TopLevelControl) != DialogResult.OK) return;
                string name = dialog.InputValue.Trim();
                if (page == null) Resources.AddIoPage(name);
                else Resources.RenameIoPage(id, name);
            }
        }

        /// <summary>从现有右键菜单编辑当前 IO 页的默认显示文字。</summary>
        private void EditIoPageText()
        {
            FlowPrograms.RequireIdle();
            HardwareIoPage page = Resources.IoPages.FirstOrDefault(item => item.Id == contextIoPageId);
            if (page == null) return;
            if (FlowInputForm.TryEditResourceText(TopLevelControl, page.Text, out string text)) Resources.SetIoPageText(page.Id, text);
        }

        /// <summary>右键只定位实际命中的 IO 页签，空白区域不复用旧目标。</summary>
        private void SelectIoContextTab(object sender, MouseEventArgs args)
        {
            if (args.Button != MouseButtons.Right) return;
            contextIoPageId = null;
            for (int index = 0; index < ioTabs.TabCount; index++)
            {
                if (!ioTabs.GetTabRect(index).Contains(args.Location)) continue;
                ioTabs.SelectedIndex = index;
                contextIoPageId = ioTabs.TabPages[index].Tag as string;
                return;
            }
        }

        /// <summary>空页可删除，非空页先移动或删除 IO，操作权限由服务层再次校验。</summary>
        private void OpenIoPageMenu(object sender, CancelEventArgs args)
        {
            bool editable = Resources != null && FlowPrograms.CanEdit && !FlowPrograms.AnyRunning && SysPara.SystemMode == RunMode.IDLE && !NPSDK.Flow_Module.Module_GetRunningStatus;
            ioPageAdd.Enabled = editable;
            ioPageRename.Enabled = editable && contextIoPageId != null;
            ioPageText.Enabled = ioPageRename.Enabled;
            bool occupied = Resources != null && Resources.Entries.Any(entry => entry.Definition.Kind != "Axis" && entry.Definition.IoPageId == contextIoPageId);
            bool lastPage = Resources == null || Resources.IoPages.Count() <= 1;
            ioPageDelete.Enabled = ioPageRename.Enabled && !occupied && !lastPage;
            ioPageDelete.ToolTipText = occupied ? HardwareText.T("ioPageNotEmpty") : lastPage ? HardwareText.T("ioPageLast") : "";
        }

        /// <summary>确认删除右键页签，服务拒绝非空页和最后一页。</summary>
        private void DeleteIoPage()
        {
            FlowPrograms.RequireIdle();
            HardwareIoPage page = Resources.IoPages.FirstOrDefault(item => item.Id == contextIoPageId);
            if (page == null) return;
            if (MessageBox.Show(string.Format(HardwareText.T("removeIoPage"), page.Name), HardwareText.T("title"), MessageBoxButtons.OKCancel) != DialogResult.OK) return;
            Resources.DeleteIoPage(page.Id);
            contextIoPageId = null;
        }

        /// <summary>每次打开 IO 菜单重新列出可移动分组，点击只更新该 IO 的显示归属。</summary>
        private void RefreshMoveMenu(HardwareDefinition definition, bool editable)
        {
            foreach (ToolStripItem item in resourceMove.DropDownItems.Cast<ToolStripItem>().ToArray()) item.Dispose();
            resourceMove.DropDownItems.Clear();
            resourceMove.Visible = definition.Kind != "Axis";
            resourceMove.Enabled = editable;
            if (definition.Kind == "Axis") return;
            string resourceId = definition.Id;
            foreach (HardwareIoPage page in Resources.IoPages)
            {
                ToolStripMenuItem choice = new ToolStripMenuItem(HardwareResources.DisplayText(page)) { Name="moveIo_"+page.Id, Checked=page.Id==definition.IoPageId, Enabled=page.Id!=definition.IoPageId };
                string pageId = page.Id;
                choice.Click += (sender, args) => Execute(() => Resources.MoveIoToPage(resourceId, pageId));
                resourceMove.DropDownItems.Add(choice);
            }
        }

        /// <summary>只刷新固定标签及组合控件标题，保留点位数据和选中资源。</summary>
        private void ApplyLanguage(object sender,EventArgs args)
        {
            HardwareText.Apply(this);
            if (Resources != null) RefreshEntries();
            resourceDelete.Text = HardwareText.Translate(resourceDelete.Name, "删除资源", "Delete resource", "Eliminar recurso");
            resourceProperties.Text = HardwareText.Translate(resourceProperties.Name, "属性编辑", "Edit properties", "Editar propiedades");
            resourceMove.Text = HardwareText.T("moveIoPage");
            resourceSource.Text = ConfigurationSourceForm.T(resourceSource.Name, "查看配置来源", "View configuration source", "Ver origen de configuración");
            findConfiguration.Text = ConfigurationSourceForm.T(findConfiguration.Name, "查找配置", "Find configuration", "Buscar configuración");
            if (Resources?.LoadFailure != null) status.Text = HardwareText.T("checkSource");
            ioPageAdd.Text = HardwareText.T("addIoPage");
            ioPageRename.Text = HardwareText.T("renameIoPage");
            ioPageText.Text = HardwareText.T("Text");
            ioPageDelete.Text = HardwareText.T("deleteIoPage");
            if (Resources == null) return;
            foreach (HardwareEntry entry in Resources.Entries.Where(BelongsHere))
            {
                if (!(entry.Group is NP_AxisGroup axis)) continue;
                axis.AxisCaption = HardwareResources.DisplayText(entry.Definition);
                axis.ApplyLanguage(HardwareText.Translate);
            }
        }

        /// <summary>把操作错误显示在当前页面，不丢弃资源草稿。</summary>
        private void Execute(Action action)
        {
            try { action(); }
            catch (Exception error) { status.Text = error.Message; MessageBox.Show(error.Message,HardwareText.T("title")); }
        }
    }
}
