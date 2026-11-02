using System;
using System.ComponentModel;
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
            if (UiTheme.IsDesignTime) return;
            HardwareText.Apply(this);
            foreach (Control control in Controls) if (control != canvas && control != axisTabs) MiddleLayer.RegisterAndApplyLanguage(control,"HardwareResources");
            MiddleLayer.LanguageChanged += ApplyLanguage;
            Disposed += (sender,args) => MiddleLayer.LanguageChanged -= ApplyLanguage;
            addAxis.Click += (sender,args) => Execute(()=>EditNew("Axis"));
            addInput.Click += (sender,args) => Execute(()=>EditNew("Input"));
            addOutput.Click += (sender,args) => Execute(()=>EditNew("Output"));
            edit.Click += (sender,args) => Execute(()=>EditEntry(Selected()));
            resourceMenu.Opening += OpenResourceMenu;
            resourceDelete.Click += (sender,args) => Execute(DeleteEntry);
            resourceProperties.Click += (sender,args) => Execute(EditContextEntry);
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

        /// <summary>绑定目录变更并挂接当前页面所属资源的交互。</summary>
        private void BindEvents()
        {
            Resources.Changed += ResourcesChanged;
            foreach (HardwareEntry entry in Resources.Entries.Where(BelongsHere)) Attach(entry);
            RefreshEntries(); ApplyLanguage(null,EventArgs.Empty);
            Disposed += (sender,args) => Resources.Changed -= ResourcesChanged;
        }

        /// <summary>按页面用途筛选轴或 IO，业务资源仍由同一个目录管理。</summary>
        private bool BelongsHere(HardwareEntry entry) { return ResourceKind=="IO" ? entry.Definition.Kind!="Axis" : entry.Definition.Kind=="Axis"; }

        /// <summary>固定模板在设计器和运行时使用同一轴或 IO 操作模式。</summary>
        private void ApplyKind()
        {
            if (addAxis == null) return;
            addAxis.Visible = resourceKind!="IO";
            addInput.Visible = resourceKind=="IO"; addOutput.Visible = resourceKind=="IO";
            edit.Visible = resourceKind!="IO";
            axisTabs.Visible = resourceKind!="IO"; canvas.Visible = resourceKind=="IO";
            toolbar.Height = 42;
            if (Resources != null) RefreshEntries();
            hint.Name = resourceKind=="IO" ? "ioResourceHint" : "resourceHint"; hint.Tag = resourceKind=="IO" ? "ioHint" : "hint";
            hint.Text = HardwareText.T(resourceKind=="IO" ? "ioHint" : "hint");
            ArrangeIo();
        }

        /// <summary>参数按钮始终定位当前轴选项卡，业务名称不作为身份。</summary>
        private HardwareEntry Selected()
        {
            HardwareDefinition definition = axisTabs.SelectedTab == null ? null : axisTabs.SelectedTab.Tag as HardwareDefinition;
            HardwareEntry entry = definition == null || Resources == null ? null : Resources.Entries.FirstOrDefault(item => item.Definition.Id == definition.Id);
            if (entry == null) throw new InvalidOperationException(HardwareText.T("missing"));
            return entry;
        }

        /// <summary>先编辑资源草稿，确定后才创建驱动对象及组合控件。</summary>
        private void EditNew(string kind)
        {
            FlowPrograms.RequireIdle();
            if (Resources == null) throw new InvalidOperationException(HardwareText.T("missing"));
            HardwareDefinition definition = Resources.NewDefinition(kind);
            using (HardwareEditForm dialog = new HardwareEditForm(definition,value=>Resources.Put(value))) dialog.ShowDialog(TopLevelControl);
        }

        /// <summary>双击组合控件和工具栏参数按钮共用同一编辑入口。</summary>
        private void EditEntry(HardwareEntry entry)
        {
            FlowPrograms.RequireIdle();
            using (HardwareEditForm dialog = new HardwareEditForm(entry.Definition,value=>{ Resources.Put(value); if (Resources.RequiresRestart) status.Text = HardwareText.T("restart"); })) dialog.ShowDialog(this);
        }

        /// <summary>检查当前资源后确认删除，服务层再次检查流程引用和占用。</summary>
        private void DeleteEntry()
        {
            FlowPrograms.RequireIdle();
            HardwareEntry entry = Resources.Entries.FirstOrDefault(value => value.Definition.Id == contextResourceId);
            if (entry == null) return;
            if (MessageBox.Show(string.Format(HardwareText.T("remove"),entry.Definition.Name),HardwareText.T("title"),MessageBoxButtons.OKCancel) != DialogResult.OK) return;
            Resources.Delete(entry.Definition.Id);
        }

        /// <summary>右键属性编辑始终打开当前命中的组合对象，与工具栏选择无关。</summary>
        private void EditContextEntry()
        {
            FlowPrograms.RequireIdle();
            HardwareEntry entry = Resources.Entries.FirstOrDefault(value => value.Definition.Id == contextResourceId);
            if (entry != null) EditEntry(entry);
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
        private void BindResourceMenu(Control control)
        {
            if (control is NP_PositionTable) return;
            control.ContextMenuStrip = resourceMenu;
            foreach (Control child in control.Controls) BindResourceMenu(child);
        }

        /// <summary>按右键控件的资源身份定位，不依赖工具栏上次选中的对象。</summary>
        private void OpenResourceMenu(object sender, CancelEventArgs args)
        {
            contextResourceId = null;
            Control control = resourceMenu.SourceControl;
            if (control == axisTabs) control = contextAxisTab;
            while (control != null && !(control.Tag is HardwareDefinition)) control = control.Parent;
            HardwareDefinition definition = control == null ? null : control.Tag as HardwareDefinition;
            if (definition == null || Resources == null) { args.Cancel = true; return; }
            contextResourceId = definition.Id;
            resourceDelete.Enabled = FlowPrograms.CanEdit && !FlowPrograms.AnyRunning && SysPara.SystemMode == RunMode.IDLE && !NPSDK.Flow_Module.Module_GetRunningStatus;
            resourceProperties.Enabled = resourceDelete.Enabled;
        }

        /// <summary>按发送组合控件定位资源，不使用旧的下拉框选择。</summary>
        private void GroupSettings(object sender,EventArgs args)
        {
            Execute(()=>EditEntry(Resources.Entries.Single(entry=>ReferenceEquals(entry.Group,sender))));
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
            if (resourceKind == "IO") { ArrangeIo(); return; }
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
                        page = new TabPage { Name = "axisTab_" + entry.Definition.Id + "_dyn", AutoScroll = true, Padding = new Padding(5), UseVisualStyleBackColor = true };
                        axisTabs.TabPages.Add(page);
                        if (hadTabs || axes.Length == 1) selection = page;
                    }
                    page.Tag = entry.Definition; page.Text = entry.Definition.Name; page.ToolTipText = entry.Definition.Name;
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
                HardwareEntry[] entries = Resources.Entries.Where(entry => entry.Definition.Kind == kind).ToArray();
                Label header = kind == "Input" ? inputHeader : outputHeader;
                header.Text = FlowText.T(kind == "Input" ? "di" : "do") + " · " + entries.Length;
                header.Visible = entries.Length > 0; header.Width = width - 6;
                canvas.Controls.SetChildIndex(header,position++);
                foreach (HardwareEntry entry in entries)
                {
                    entry.Group.Size = new System.Drawing.Size(tileWidth,40);
                    canvas.Controls.SetChildIndex(entry.Group,position++);
                    IoTileStyle.PlaceIndicator(entry.Device);
                }
            }
            canvas.ResumeLayout();
        }

        /// <summary>只刷新固定标签及组合控件标题，保留点位数据和选中资源。</summary>
        private void ApplyLanguage(object sender,EventArgs args)
        {
            HardwareText.Apply(this);
            ArrangeIo();
            resourceDelete.Text = HardwareText.Translate(resourceDelete.Name, "删除资源", "Delete resource", "Eliminar recurso");
            resourceProperties.Text = HardwareText.Translate(resourceProperties.Name, "属性编辑", "Edit properties", "Editar propiedades");
            if (Resources == null) return;
            foreach (HardwareEntry entry in Resources.Entries.Where(BelongsHere))
            {
                NP_AxisGroup axis = entry.Group as NP_AxisGroup; if (axis != null) axis.ApplyLanguage(HardwareText.Translate);
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
