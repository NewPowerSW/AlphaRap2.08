using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    /// <summary>代码自行决定出口，本控件只编辑出口的连接目标。</summary>
    public partial class FlowCodeRoutesControl : UserControl
    {
        private readonly string[] codes = { "NEXT", "CASE1", "CASE2", "CASE3" };
        private List<string> exits = new List<string>();
        private List<FlowChoice> destinations = new List<FlowChoice>();
        private bool languageSubscribed;
        private ComboBox[] Pickers { get { return new[] { nextTarget, case1Target, case2Target, case3Target }; } }
        private Label[] Labels { get { return new[] { nextLabel, case1Label, case2Label, case3Label }; } }

        /// <summary>创建固定四出口模板，设计期不加载工程。</summary>
        public FlowCodeRoutesControl()
        {
            InitializeComponent();
            if (UiTheme.IsDesignTime) return;
            MiddleLayer.LanguageChanged += LanguageChanged; languageSubscribed = true;
            help.Text = FlowCodeUiText.T("codeRoutesHelp");
        }

        /// <summary>按模块声明显示出口，未配置和显式结束保持不同状态。</summary>
        internal void Bind(FlowCodeDefinition definition, FlowStep step, List<FlowChoice> targets)
        {
            exits = definition == null ? codes.Where(code => code == "NEXT" || step.TargetOf(code) != null || step.Branches.Any(branch => branch.Code == code)).ToList() : definition.Exits.ToList();
            destinations = targets;
            ComboBox[] pickers = Pickers; Label[] labels = Labels;
            for (int index = 0; index < codes.Length; index++)
            {
                bool visible = exits.Contains(codes[index]);
                layout.RowStyles[index].Height = visible ? 42 : 0;
                pickers[index].Visible = visible; labels[index].Visible = visible;
                FillTarget(pickers[index], step.TargetOf(codes[index]));
            }
            layout.Height = exits.Count * 42;
            help.Text = FlowCodeUiText.T("codeRoutesHelp");
        }

        /// <summary>保留失效目标且不默认采用结束；首次新增需要工程师明确选择。</summary>
        private void FillTarget(ComboBox picker, string selected)
        {
            List<FlowChoice> values = destinations.Select(choice => new FlowChoice { Id = choice.Id, Label = string.IsNullOrEmpty(choice.Id) ? FlowText.T("end") : choice.Label }).ToList();
            if (selected != null && values.All(choice => choice.Id != selected)) values.Add(new FlowChoice { Id = selected, Label = string.Format(FlowCodeUiText.T("missing"), selected) });
            picker.DisplayMember = "Label"; picker.ValueMember = "Id"; picker.DataSource = values; picker.SelectedIndex = -1;
            if (selected != null) picker.SelectedValue = selected;
            picker.AccessibleDescription = FlowCodeUiText.T("unconfigured");
        }

        /// <summary>只保存模块出口的去向，清除外层条件而不改模块内部判定。</summary>
        internal void ReadTo(FlowStep step)
        {
            step.Branches = exits.Select(code => new FlowBranch { Code = code, Unconditional = true }).ToList();
            for (int index = 0; index < codes.Length; index++) step.SetTarget(codes[index], exits.Contains(codes[index]) && Pickers[index].SelectedIndex >= 0 ? Convert.ToString(Pickers[index].SelectedValue) : null);
        }

        /// <summary>语言切换只刷新目标显示文字，空选择和失效引用保持不变。</summary>
        public void RefreshLanguage()
        {
            foreach (ComboBox picker in Pickers)
            {
                string selected = picker.SelectedIndex < 0 ? null : Convert.ToString(picker.SelectedValue);
                FillTarget(picker, selected);
            }
            help.Text = FlowCodeUiText.T("codeRoutesHelp");
        }

        /// <summary>刷新出口说明和目标标题。</summary>
        private void LanguageChanged(object sender, EventArgs args) { RefreshLanguage(); }

        /// <summary>解除三语事件，工程由宿主管理。</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && languageSubscribed) { MiddleLayer.LanguageChanged -= LanguageChanged; languageSubscribed = false; }
            base.Dispose(disposing);
        }
    }
}
