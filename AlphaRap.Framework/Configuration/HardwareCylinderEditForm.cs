using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AlphaRap.FlowProgramming;

namespace AlphaRap.Configuration
{
    public partial class HardwareCylinderEditForm : Form
    {
        private HardwareResources resources;
        private HardwareCylinderDefinition draft;

        private sealed class OutputChoice
        {
            public string Id { get; }
            public string Caption { get; }

            /// <summary>下拉框显示名称，选中值使用稳定 ID。</summary>
            internal OutputChoice(string id, string caption) { Id = id; Caption = caption; }
        }

        /// <summary>为设计器加载固定的气缸属性布局。</summary>
        public HardwareCylinderEditForm() { InitializeComponent(); }

        /// <summary>编辑内存草稿，保留两路输出的原有选择。</summary>
        internal HardwareCylinderEditForm(HardwareResources resources, HardwareCylinderDefinition value) : this()
        {
            this.resources = resources;
            draft = value.Copy();
            cylinderName.Text = draft.Name;
            cylinderText.Text = draft.Text;
            ApplyLanguage(this, EventArgs.Empty);
            FrameworkLanguage.LanguageChanged += ApplyLanguage;
            Disposed += (sender, args) => FrameworkLanguage.LanguageChanged -= ApplyLanguage;
        }

        /// <summary>从现有数字输出填充参数选项，缺失的旧输出可重新选择。</summary>
        private void BindOutputs(ComboBox box, string selected)
        {
            List<OutputChoice> choices = resources.Entries.Where(entry => entry.Definition.Kind == "Output")
                .Select(entry => new OutputChoice(entry.Definition.Id, OutputCaption(entry.Definition)))
                .OrderBy(item => item.Caption, StringComparer.CurrentCultureIgnoreCase).ToList();
            OutputChoice choice = choices.FirstOrDefault(item => string.Equals(item.Id, selected, StringComparison.OrdinalIgnoreCase));
            if (choice == null && !string.IsNullOrEmpty(selected))
            {
                choice = new OutputChoice(selected, HardwareText.T("cylinderMissing") + " · " + selected);
                choices.Add(choice);
            }

            box.DataSource = choices;
            box.DisplayMember = nameof(OutputChoice.Caption);
            box.ValueMember = nameof(OutputChoice.Id);
            box.SelectedItem = choice;
        }

        /// <summary>同时显示工程 IO 名称及卡片标题，便于区分同名译文。</summary>
        private string OutputCaption(HardwareDefinition output)
        {
            string caption = HardwareResources.DisplayText(output);
            if (caption != output.Name) caption += " · " + output.Name;
            if (output.ConnectionPending) caption += " · " + HardwareText.T("ConnectionPending");
            return caption;
        }

        /// <summary>刷新三语标题，保留编辑的名称和两个输出 ID。</summary>
        private void ApplyLanguage(object sender, EventArgs args)
        {
            Text = HardwareText.T("editCylinder");
            lblName.Text = HardwareText.T("Name");
            lblText.Text = HardwareText.T("Text");
            lblOn.Text = HardwareText.T("cylinderOn");
            lblOff.Text = HardwareText.T("cylinderOff");
            btnOk.Text = FlowText.T("ok");
            btnCancel.Text = FlowText.T("cancel");
            string on = onOutput.DataSource == null ? draft.OnOutputId : onOutput.SelectedValue as string;
            string off = offOutput.DataSource == null ? draft.OffOutputId : offOutput.SelectedValue as string;
            BindOutputs(onOutput, on);
            BindOutputs(offOutput, off);
        }

        /// <summary>校验后提交内存草稿，主界面保存后绑定投入使用。</summary>
        private void Confirm(object sender, EventArgs args)
        {
            if (resources == null) return;
            try
            {
                draft.Name = cylinderName.Text.Trim();
                draft.Text = cylinderText.Text.Trim();
                draft.OnOutputId = onOutput.SelectedValue as string ?? "";
                draft.OffOutputId = offOutput.SelectedValue as string ?? "";
                resources.PutCylinder(draft);
                DialogResult = DialogResult.OK;
            }
            catch (InvalidOperationException error)
            {
                MessageBox.Show(this, error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
