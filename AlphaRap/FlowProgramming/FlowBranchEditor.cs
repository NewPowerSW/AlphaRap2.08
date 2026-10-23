using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    public partial class FlowBranchEditor : UserControl
    {
        public string Code { get; private set; }
        public IEnumerable<FlowConditionRow> ConditionRows { get { return conditions.Controls.OfType<FlowConditionRow>(); } }
        public string TargetId { get { return Convert.ToString(target.SelectedValue); } }
        public bool IsUnconditional { get { return unconditional.Checked; } }
        public event EventHandler RemoveRequested;
        private FlowActions catalog;
        private FlowMethod method;
        private FlowConditionRow contextCondition;
        private bool binding;

        /// <summary>同一出口模板用于默认 NEXT 和按需添加的 CASE。</summary>
        public FlowBranchEditor()
        {
            InitializeComponent();
            unconditional.CheckedChanged += ModeChanged;
            remove.Click += (sender,args) => { if (RemoveRequested != null) RemoveRequested(this,EventArgs.Empty); };
            addCondition.Click += (sender,args) => AddCondition(DefaultCondition());
            conditionMenu.Opening += OpenConditionMenu;
            conditionDelete.Click += (sender,args) => { if (contextCondition == null) return; conditions.Controls.Remove(contextCondition); contextCondition.Dispose(); contextCondition = null; ResizeRows(); };
            SizeChanged += (sender,args) => ResizeRows();
            ResizeRows();
        }

        /// <summary>用当前方法的真实输出更新选择目录，保留所有已编辑条件。</summary>
        internal void Configure(FlowActions actions,FlowMethod selectedMethod)
        {
            catalog = actions; method = selectedMethod;
            foreach (FlowConditionRow row in ConditionRows) row.RefreshChoices(catalog,method);
        }

        /// <summary>绑定出口目标与条件副本，加载过程不自动增加条件。</summary>
        public void Bind(FlowBranch branch,string targetId,List<FlowChoice> destinations)
        {
            binding = true;
            Code = branch.Code; codeLabel.Text = Code; remove.Enabled = Code != "NEXT";
            target.DisplayMember = "Label"; target.ValueMember = "Id"; target.DataSource = destinations; target.SelectedValue = targetId ?? "";
            unconditional.Checked = branch.Unconditional; all.Checked = !branch.Unconditional && !branch.AnyCondition; any.Checked = !branch.Unconditional && branch.AnyCondition;
            foreach (Control row in conditions.Controls.Cast<Control>().ToArray()) row.Dispose();
            foreach (FlowCondition condition in branch.Conditions) AddCondition(condition);
            binding = false; ResizeRows();
        }

        /// <summary>进入条件模式时给出一条可直接修改的默认条件。</summary>
        private void ModeChanged(object sender,EventArgs args)
        {
            if (!binding && !unconditional.Checked && !ConditionRows.Any()) AddCondition(DefaultCondition());
            ResizeRows();
        }

        /// <summary>布尔结果默认与 true 比较，其他字段默认与空固定值比较。</summary>
        private FlowCondition DefaultCondition()
        {
            FlowField field = method == null ? null : method.Outputs.FirstOrDefault();
            return new FlowCondition { Left = new FlowInput { Source = "Result", Reference = field == null ? "" : field.Name },
                Operator = "==", Right = new FlowInput { Reference = field != null && field.Type == typeof(bool) ? "true" : "" } };
        }

        /// <summary>新增一条扁平条件，删除统一使用行右键。</summary>
        private void AddCondition(FlowCondition condition)
        {
            FlowConditionRow row = new FlowConditionRow(); row.Bind(condition,catalog,method); BindMenu(row);
            conditions.Controls.Add(row); ResizeRows();
        }

        /// <summary>行内控件共用命中该行的右键菜单。</summary>
        private void BindMenu(Control control)
        {
            control.ContextMenuStrip = conditionMenu;
            foreach (Control child in control.Controls) BindMenu(child);
        }

        /// <summary>右键命中条件行，空白处不复用旧目标。</summary>
        private void OpenConditionMenu(object sender,System.ComponentModel.CancelEventArgs args)
        {
            Control source = conditionMenu.SourceControl;
            while (source != null && !(source is FlowConditionRow)) source = source.Parent;
            contextCondition = source as FlowConditionRow;
            args.Cancel = contextCondition == null || !conditions.Controls.Contains(contextCondition);
        }

        /// <summary>无条件出口收起空白，条件行随出口宽度排列。</summary>
        private void ResizeRows()
        {
            conditions.Visible = !unconditional.Checked; conditions.Enabled = !unconditional.Checked;
            addCondition.Visible = !unconditional.Checked; conditionHint.Visible = !unconditional.Checked;
            layout.RowStyles[2].Height = unconditional.Checked ? 0 : 34; layout.RowStyles[3].Height = unconditional.Checked ? 0 : 32;
            int headerHeight = Math.Max(32,header.GetPreferredSize(new System.Drawing.Size(Math.Max(1,ClientSize.Width-Padding.Horizontal),0)).Height);
            layout.RowStyles[0].Height = headerHeight;
            foreach (FlowConditionRow row in ConditionRows) row.Width = Math.Max(row.MinimumSize.Width,ClientSize.Width-Padding.Horizontal-8);
            Height = headerHeight + Padding.Vertical + (unconditional.Checked ? 0 : 70 + ConditionRows.Count()*40);
        }

        /// <summary>添加 CASE 时默认 NEXT 转为条件模式，保留已填写的条件。</summary>
        public void MakeConditional() { all.Checked = true; if (!ConditionRows.Any()) AddCondition(DefaultCondition()); }

        /// <summary>刷新翻译及字段文案，业务引用和正在输入的文本保持不变。</summary>
        internal void RefreshDestinationLanguage()
        {
            List<FlowChoice> choices = target.DataSource as List<FlowChoice>;
            if (choices == null) return;
            string selected = TargetId; FlowChoice end = choices.FirstOrDefault(choice => string.IsNullOrEmpty(choice.Id));
            if (end != null) end.Label = FlowText.T("end");
            target.DataSource = choices.ToList(); target.SelectedValue = selected;
            foreach (FlowConditionRow row in ConditionRows) row.RefreshChoices(catalog,method);
            ResizeRows();
        }

        /// <summary>只返回条件副本，确定窗口之前不修改节点定义。</summary>
        public FlowBranch ReadBranch()
        {
            return new FlowBranch { Code = Code, Unconditional = unconditional.Checked, AnyCondition = any.Checked, Conditions = ConditionRows.Select(row => row.ReadCondition()).ToList() };
        }
    }
}
