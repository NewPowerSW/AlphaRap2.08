using System;
using System.Windows.Forms;

namespace AlphaRap.FlowProgramming
{
    public class FlowLanguageForm : Form
    {
        private bool languageSubscribed;

        /// <summary>初始化统一窗口外观，设计期不接触宿主和设备。</summary>
        public FlowLanguageForm()
        {
            if (UiTheme.IsDesignTime) return;
            MiddleLayer.LanguageChanged += LanguageChanged;
            languageSubscribed = true;
        }

        /// <summary>按稳定名称登记窗口及表头。</summary>
        protected override void OnLoad(EventArgs e)
        {
            ApplyTexts(this);
            if (!UiTheme.IsDesignTime) MiddleLayer.RegisterAndApplyLanguage(this, "EquipmentFlow");
            base.OnLoad(e);
        }

        /// <summary>切换语言只修改文字，不重新绑定数据行或提交正在编辑的值。</summary>
        private void LanguageChanged(object sender, EventArgs e) { ApplyTexts(this); RefreshLanguageChoices(); }

        /// <summary>派生窗口可刷新目录文案，不重建业务输入和草稿。</summary>
        protected virtual void RefreshLanguageChoices() { }

        /// <summary>刷新 Designer 中以 Tag 标识的固定文字。</summary>
        internal static void ApplyTexts(Control parent)
        {
            string key = parent.Tag as string;
            Form owner = parent as Form ?? parent.FindForm();
            string scope = owner is FlowLanguageForm || owner == null ? "EquipmentFlow" : owner.Name;
            string[] words;
            if (key != null && FlowText.Defaults.TryGetValue(key, out words))
                parent.Text = UiTheme.IsDesignTime ? words[0] : MiddleLayer.LangMsg(scope, parent.Name, words[0], words[1], words[2]);
            DataGridView grid = parent as DataGridView;
            if (grid != null)
                foreach (DataGridViewColumn column in grid.Columns) ApplyColumnText(grid, column, scope);
            foreach (Control child in parent.Controls) ApplyTexts(child);
            if (parent.ContextMenuStrip != null)
                foreach (ToolStripItem item in parent.ContextMenuStrip.Items)
                    if (FlowText.Defaults.TryGetValue(item.Tag as string ?? "", out words))
                        item.Text = UiTheme.IsDesignTime ? words[0] : MiddleLayer.LangMsg(scope, item.Name, words[0], words[1], words[2]);
        }

        /// <summary>按表名和列名登记固定表头。</summary>
        private static void ApplyColumnText(DataGridView grid, DataGridViewColumn column, string scope)
        {
            string[] words;
            string key = column.Tag as string;
            if (key == null || !FlowText.Defaults.TryGetValue(key, out words)) return;
            column.HeaderText = UiTheme.IsDesignTime ? words[0] : MiddleLayer.LangMsg(scope, grid.Name + "_" + column.Name, words[0], words[1], words[2]);
        }

        /// <summary>释放窗口时解除静态语言事件。</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && languageSubscribed)
            {
                MiddleLayer.LanguageChanged -= LanguageChanged;
                languageSubscribed = false;
            }
            base.Dispose(disposing);
        }
    }
}
