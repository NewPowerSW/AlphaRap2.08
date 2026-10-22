using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace AlphaRap.FlowProgramming
{
    public partial class FlowSchemaForm : FlowLanguageForm
    {
        private readonly DataForm data;
        private DataGridViewRow contextField;
        /// <summary>Build the fixed controls without loading application data.</summary>
        public FlowSchemaForm()
        {
            InitializeComponent();
            fields.MouseDown += SelectField;
            fieldMenu.Opening += (sender, args) => args.Cancel = contextField == null || contextField.IsNewRow;
            fieldDelete.Click += (sender, args) => { if (contextField != null && !contextField.IsNewRow && contextField.DataGridView == fields) fields.Rows.Remove(contextField); contextField = null; };
        }

        /// <summary>右键按命中位置选择待删除的生产字段草稿。</summary>
        private void SelectField(object sender, MouseEventArgs args)
        {
            if (args.Button != MouseButtons.Right) return;
            contextField = null;
            int index = fields.HitTest(args.X, args.Y).RowIndex;
            if (index < 0 || fields.Rows[index].IsNewRow) return;
            contextField = fields.Rows[index]; fields.CurrentCell = contextField.Cells[0];
        }

        /// <summary>从 DataForm 读取生产表结构，编辑副本不影响运行数据。</summary>
        public FlowSchemaForm(DataForm owner) : this()
        {
            data = owner;
            foreach (ProductionField field in data.GetProductionDraft()) fields.Rows.Add(field.Name, field.Type);
            fields.DefaultValuesNeeded += (sender, args) => args.Row.Cells[1].Value = "Text";
            ok.Click += SaveFields;
        }

        /// <summary>验证并提交生产表草稿，持久化由主界面保存负责。</summary>
        private void SaveFields(object sender, EventArgs args)
        {
            fields.EndEdit();
            try
            {
                FlowPrograms.RequireIdle();
                List<ProductionField> schema = fields.Rows.Cast<DataGridViewRow>().Where(row => !row.IsNewRow)
                    .Select(row => new ProductionField { Name = Convert.ToString(row.Cells[0].Value), Type = Convert.ToString(row.Cells[1].Value) }).ToList();
                data.SetProductionDraft(schema);
                DialogResult = DialogResult.OK;
            }
            catch (Exception error) { MessageBox.Show(string.Format(FlowText.T("error"), error.Message), FlowText.T("title")); }
        }
    }
}
