using System.Collections.Generic;
using System.Linq;

namespace AlphaRap.FlowProgramming
{
    public partial class FlowSourceViewer : FlowLanguageForm
    {
        private IDictionary<string, string> documents = new Dictionary<string, string>();

        /// <summary>创建只读源码预览，未提供文件修改、删除或外部目录入口。</summary>
        public FlowSourceViewer()
        {
            InitializeComponent();
            if (UiTheme.IsDesignTime) return;
            files.SelectedIndexChanged += (sender, args) => ShowSource();
        }

        /// <summary>显示已读取的源码快照，关闭窗口不产生磁盘写入。</summary>
        public FlowSourceViewer(IDictionary<string, string> sources, string directory) : this()
        {
            documents = sources;
            sourcePath.Text = directory;
            foreach (string name in sources.Keys.OrderBy(name => name)) files.Items.Add(name);
            if (files.Items.Count > 0) files.SelectedIndex = 0;
            else source.Text = FlowText.T("sourceNotGenerated");
        }

        /// <summary>按选中文件显示纯文本，保留复制和滚动能力。</summary>
        private void ShowSource()
        {
            string name = files.SelectedItem as string;
            if (name != null) source.Text = documents[name];
        }

        /// <summary>空源码状态随语言刷新，实际代码文本和文件选择保持不变。</summary>
        protected override void RefreshLanguageChoices()
        {
            if (files.Items.Count == 0) source.Text = FlowText.T("sourceNotGenerated");
        }
    }
}
