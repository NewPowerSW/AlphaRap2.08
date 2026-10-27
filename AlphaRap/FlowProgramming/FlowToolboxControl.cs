using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    public sealed class FlowToolboxEntry
    {
        public string Id, ExecutionMode, TargetId, ActionId, Category, Title;
    }

    /// <summary>显示真实对象动作及条件节点，拖放和键盘使用同一稳定条目。</summary>
    public partial class FlowToolboxControl : UserControl
    {
        public const string DragFormat = "AlphaRap.FlowToolboxEntry";
        private FlowActions actions;
        private List<FlowToolboxEntry> choices = new List<FlowToolboxEntry>();
        private string selectedEntryId;
        private bool languageSubscribed;
        public event Action<FlowToolboxEntry> InsertRequested;

        /// <summary>创建固定工具箱模板，设计期不读取模块或设备目录。</summary>
        public FlowToolboxControl()
        {
            InitializeComponent();
            if (UiTheme.IsDesignTime) return;
            search.TextChanged += (sender, args) => RefreshTree();
            entries.AfterSelect += (sender, args) =>
            {
                FlowToolboxEntry entry = args.Node.Tag as FlowToolboxEntry;
                if (entry != null) selectedEntryId = entry.Id;
            };
            entries.ItemDrag += DragEntry;
            entries.NodeMouseDoubleClick += (sender, args) => Insert(args.Node);
            entries.KeyDown += InsertWithKeyboard;
            MiddleLayer.LanguageChanged += LanguageChanged;
            languageSubscribed = true;
            RefreshLanguage();
        }

        /// <summary>绑定当前对象动作目录，保留工具箱查询文字。</summary>
        public void BindActions(FlowActions value)
        {
            actions = value;
            choices = Choices();
            RefreshTree();
        }

        /// <summary>从实际对象和流程基础能力建立条目，不发现脚本或代码模块。</summary>
        private List<FlowToolboxEntry> Choices()
        {
            List<FlowToolboxEntry> result = new List<FlowToolboxEntry> {
                new FlowToolboxEntry { Id = "None", ExecutionMode = "None", Category = "flow", Title = FlowToolboxText.T("noneEntry") }
            };
            if (actions == null) return result;
            foreach (FlowResource resource in actions.Resources.Items.Values.Where(actions.CanSelectResource))
                foreach (FlowMethod method in actions.MethodsFor(resource))
                    result.Add(new FlowToolboxEntry { Id = "action/" + Uri.EscapeDataString(resource.Id) + "/" + Uri.EscapeDataString(method.Id), ExecutionMode = "Action",
                        TargetId = resource.Id, ActionId = method.Id, Category = resource.Category,
                        Title = resource.Kind == "native" ? FlowMethods.Title(method) : resource.Label + " · " + FlowMethods.Title(method) });
            foreach (FlowMethod method in actions.MethodsFor(null))
                result.Add(new FlowToolboxEntry { Id = "flow/" + method.Id, ExecutionMode = "Action", TargetId = "", ActionId = method.Id, Category = "flow", Title = FlowMethods.Title(method) });
            return result;
        }

        /// <summary>按分组和搜索重建标题，保留已选条目身份和搜索草稿。</summary>
        private void RefreshTree()
        {
            string filter = search.Text.Trim();
            entries.BeginUpdate();
            try
            {
                entries.Nodes.Clear();
                IEnumerable<FlowToolboxEntry> visible = choices.Where(item => filter.Length == 0
                    || item.Title.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                    || item.Id.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                    || (item.TargetId ?? "").IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                    || (item.ActionId ?? "").IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);
                foreach (IGrouping<string, FlowToolboxEntry> group in visible.GroupBy(item => item.Category).OrderBy(group => CategoryTitle(group.Key)))
                {
                    TreeNode category = entries.Nodes.Add(group.Key, CategoryTitle(group.Key));
                    foreach (FlowToolboxEntry entry in group.OrderBy(item => item.Title))
                    {
                        TreeNode node = category.Nodes.Add(entry.Id, entry.Title);
                        node.Tag = entry;
                        if (entry.Id == selectedEntryId) entries.SelectedNode = node;
                    }
                    category.Expand();
                }
            }
            finally { entries.EndUpdate(); }
            emptyHint.Visible = entries.Nodes.Count == 0;
        }

        /// <summary>分组排序和显示使用相同的当前语言标题。</summary>
        private static string CategoryTitle(string category)
        {
            return category == "native" ? FlowNativeText.T("category") : FlowText.T(category);
        }

        /// <summary>只允许实际功能条目开始拖放，不拖动分组标题。</summary>
        private void DragEntry(object sender, ItemDragEventArgs args)
        {
            TreeNode node = args.Item as TreeNode;
            FlowToolboxEntry entry = node == null ? null : node.Tag as FlowToolboxEntry;
            if (entry == null || !Enabled) return;
            entries.DoDragDrop(new DataObject(DragFormat, entry), DragDropEffects.Copy);
        }

        /// <summary>选中功能后按 Enter 与双击执行相同的新增请求。</summary>
        private void InsertWithKeyboard(object sender, KeyEventArgs args)
        {
            if (args.KeyCode != Keys.Enter) return;
            args.Handled = true; args.SuppressKeyPress = true; Insert(entries.SelectedNode);
        }

        /// <summary>将新增交给宿主统一校验编辑权限、画布及工程草稿。</summary>
        private void Insert(TreeNode node)
        {
            FlowToolboxEntry entry = node == null ? null : node.Tag as FlowToolboxEntry;
            if (entry != null && Enabled && InsertRequested != null) InsertRequested(entry);
        }

        /// <summary>刷新固定文案与功能标题，不清空查询文字。</summary>
        public void RefreshLanguage()
        {
            title.Text = FlowToolboxText.Fixed(title, "toolbox"); searchLabel.Text = FlowToolboxText.Fixed(searchLabel, "search");
            help.Text = FlowToolboxText.Fixed(help, "toolboxHint"); emptyHint.Text = FlowToolboxText.Fixed(emptyHint, "toolboxEmpty");
            search.AccessibleName = searchLabel.Text; entries.AccessibleName = title.Text;
            choices = Choices();
            RefreshTree();
        }

        /// <summary>响应语言切换，搜索和选择身份保持不变。</summary>
        private void LanguageChanged(object sender, EventArgs args) { RefreshLanguage(); }

        /// <summary>解除语言事件并释放控件模板。</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && languageSubscribed) { MiddleLayer.LanguageChanged -= LanguageChanged; languageSubscribed = false; }
            base.Dispose(disposing);
        }
    }
}
