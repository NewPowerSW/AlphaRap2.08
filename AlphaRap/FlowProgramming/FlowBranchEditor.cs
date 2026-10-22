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
        public DataGridView ConditionsGrid { get { return conditions; } }
        public string TargetId { get { return Convert.ToString(target.SelectedValue); } }
        public bool IsUnconditional { get { return unconditional.Checked; } }
        public event EventHandler RemoveRequested;
        private DataGridViewRow contextCondition;

        /// <summary>建立可在设计器预览的单条原生出口模板。</summary>
        public FlowBranchEditor()
        {
            InitializeComponent();
            unconditional.CheckedChanged += (sender, args) => conditions.Enabled = !unconditional.Checked;
            remove.Click += (sender, args) => { if (RemoveRequested != null) RemoveRequested(this, EventArgs.Empty); };
            conditions.MouseDown += SelectCondition;
            conditionMenu.Opening += (sender, args) => args.Cancel = contextCondition == null || contextCondition.IsNewRow;
            conditionDelete.Click += (sender, args) => { if (contextCondition != null && !contextCondition.IsNewRow && contextCondition.DataGridView == conditions) conditions.Rows.Remove(contextCondition); contextCondition = null; };
        }

        /// <summary>右键只定位命中的条件，空白行和新增行不执行删除。</summary>
        private void SelectCondition(object sender, MouseEventArgs args)
        {
            if (args.Button != MouseButtons.Right) return;
            contextCondition = null;
            int index = conditions.HitTest(args.X, args.Y).RowIndex;
            if (index < 0 || conditions.Rows[index].IsNewRow) return;
            contextCondition = conditions.Rows[index];
            conditions.CurrentCell = contextCondition.Cells[0];
        }

        /// <summary>绑定一条出口的目标和独立条件，原生代码不参加翻译。</summary>
        public void Bind(FlowBranch branch, string targetId, List<FlowChoice> destinations)
        {
            Code = branch.Code;
            codeLabel.Text = Code;
            remove.Enabled = Code != "NEXT";
            target.DisplayMember = "Label"; target.ValueMember = "Id"; target.DataSource = destinations;
            target.SelectedValue = targetId ?? "";
            unconditional.Checked = branch.Unconditional;
            all.Checked = !branch.Unconditional && !branch.AnyCondition;
            any.Checked = !branch.Unconditional && branch.AnyCondition;
            conditions.Rows.Clear();
            foreach (FlowCondition condition in branch.Conditions)
                conditions.Rows.Add(condition.Left.Source, condition.Left.Reference, "…", condition.Operator, condition.Right.Source, condition.Right.Reference, "…");
            conditions.Enabled = !unconditional.Checked;
        }

        /// <summary>增加 CASE 时把默认 NEXT 切为可编辑条件，避免无条件出口抢先通过。</summary>
        public void MakeConditional(string field)
        {
            all.Checked = true;
            if (conditions.Rows.Cast<DataGridViewRow>().Any(row => !row.IsNewRow)) return;
            conditions.Rows.Add(string.IsNullOrEmpty(field) ? "Constant" : "Result", field ?? "0", "…", "==", "Constant", "", "…");
        }

        /// <summary>读取独立编辑副本，不向节点对象写入半完成的条件。</summary>
        public FlowBranch ReadBranch()
        {
            conditions.EndEdit();
            return new FlowBranch { Code = Code, Unconditional = unconditional.Checked, AnyCondition = any.Checked,
                Conditions = conditions.Rows.Cast<DataGridViewRow>().Where(row => !row.IsNewRow).Select(row => new FlowCondition {
                    Left = ReadInput(row, 0, 1), Operator = Convert.ToString(row.Cells[3].Value), Right = ReadInput(row, 4, 5) }).ToList() };
        }

        /// <summary>读取一个已选择的数据来源，保留稳定引用键。</summary>
        private static FlowInput ReadInput(DataGridViewRow row, int source, int reference)
        {
            return new FlowInput { Source = Convert.ToString(row.Cells[source].Value), Reference = Convert.ToString(row.Cells[reference].Value) };
        }
    }
}
