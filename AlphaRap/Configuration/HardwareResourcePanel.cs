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
            ApplyKind();
            if (UiTheme.IsDesignTime) return;
            HardwareText.Apply(this);
            foreach (Control control in Controls) if (control != canvas) MiddleLayer.RegisterAndApplyLanguage(control,"HardwareResources");
            MiddleLayer.LanguageChanged += ApplyLanguage;
            Disposed += (sender,args) => MiddleLayer.LanguageChanged -= ApplyLanguage;
            addAxis.Click += (sender,args) => Execute(()=>EditNew("Axis"));
            addInput.Click += (sender,args) => Execute(()=>EditNew("Input"));
            addOutput.Click += (sender,args) => Execute(()=>EditNew("Output"));
            edit.Click += (sender,args) => Execute(()=>EditEntry(Selected()));
            resourceMenu.Opening += OpenResourceMenu;
            resourceDelete.Click += (sender,args) => Execute(DeleteEntry);
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
        }

        /// <summary>选择列表按对象身份定位，业务名称不作翻译。</summary>
        private HardwareEntry Selected()
        {
            HardwareEntry entry = selected.SelectedItem as HardwareEntry;
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
            if (io != null) { io.ParametersRequested -= GroupSettings; io.ParametersRequested += GroupSettings; io.ApplyLanguage(HardwareText.Translate); }
        }

        /// <summary>轴和 IO 的表面共用删除菜单，点位表保留自己的行菜单。</summary>
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
            while (control != null && !(control.Tag is HardwareDefinition)) control = control.Parent;
            HardwareDefinition definition = control == null ? null : control.Tag as HardwareDefinition;
            if (definition == null || Resources == null) { args.Cancel = true; return; }
            contextResourceId = definition.Id;
            resourceDelete.Enabled = FlowPrograms.CanEdit && !FlowPrograms.AnyRunning && SysPara.SystemMode == RunMode.IDLE && !NPSDK.Flow_Module.Module_GetRunningStatus;
        }

        /// <summary>按发送组合控件定位资源，不使用旧的下拉框选择。</summary>
        private void GroupSettings(object sender,EventArgs args)
        {
            Execute(()=>EditEntry(Resources.Entries.Single(entry=>ReferenceEquals(entry.Group,sender))));
        }

        /// <summary>增删改后刷新列表，语言切换不执行此数据绑定。</summary>
        private void RefreshEntries()
        {
            selected.DataSource = Resources.Entries.Where(BelongsHere).ToList();
        }

        /// <summary>只刷新固定标签及组合控件标题，保留点位数据和选中资源。</summary>
        private void ApplyLanguage(object sender,EventArgs args)
        {
            HardwareText.Apply(this);
            resourceDelete.Text = HardwareText.Translate(resourceDelete.Name, "删除资源", "Delete resource", "Eliminar recurso");
            if (Resources == null) return;
            foreach (HardwareEntry entry in Resources.Entries.Where(BelongsHere))
            {
                NP_AxisGroup axis = entry.Group as NP_AxisGroup; if (axis != null) axis.ApplyLanguage(HardwareText.Translate);
                NP_IoGroup io = entry.Group as NP_IoGroup; if (io != null) io.ApplyLanguage(HardwareText.Translate);
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
