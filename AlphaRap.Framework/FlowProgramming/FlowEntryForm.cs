using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    public partial class FlowEntryForm : FlowLanguageForm
    {
        private FlowModuleEditor module;
        private bool initializing;

        /// <summary>创建可预览的多入口勾选窗口，不访问设备或修改工程。</summary>
        public FlowEntryForm()
        {
            InitializeComponent();
            if (UiTheme.IsDesignTime) return;
            ok.Click += AcceptEntries;
        }

        /// <summary>只列出当前阶段的节点，已保存的失效入口保留为可取消的选项。</summary>
        public FlowEntryForm(FlowModuleEditor owner, bool initial) : this()
        {
            module = owner;
            initializing = initial;
            List<string> selected = initial ? owner.Program.InitialEntries : owner.Program.AutoEntries;
            List<FlowChoice> choices = owner.EntryChoices(initial);
            foreach (string id in selected.Where(id => choices.All(choice => choice.Id != id)))
                choices.Add(new FlowChoice { Id = id, Label = "[?] " + id });
            entries.DisplayMember = "Label";
            foreach (FlowChoice choice in choices) entries.Items.Add(choice, selected.Contains(choice.Id));
            stage.Text = owner.Form.Name + " · " + FlowText.T(initial ? "initialFlows" : "automaticFlows");
        }

        /// <summary>验证入口集合并提交内存草稿，文件仍由主保存统一写入。</summary>
        private void AcceptEntries(object sender, EventArgs args)
        {
            if (module == null) return;
            try
            {
                module.SetEntries(initializing, entries.CheckedItems.Cast<FlowChoice>().Select(item => item.Id));
                DialogResult = DialogResult.OK;
            }
            catch (Exception error) { MessageBox.Show(error.Message, FlowText.T("entrySelection")); }
        }

        /// <summary>切换语言仅刷新阶段标题，不重建勾选项。</summary>
        protected override void RefreshLanguageChoices()
        {
            if (module != null) stage.Text = module.Form.Name + " · " + FlowText.T(initializing ? "initialFlows" : "automaticFlows");
        }
    }
}
