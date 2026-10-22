using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace AlphaRap.FlowProgramming
{
    public partial class FlowSchemaForm : FlowLanguageForm
    {
        private readonly DataForm data;
        /// <summary>Build the fixed controls without loading application data.</summary>
        public FlowSchemaForm() { InitializeComponent(); }

        /// <summary>从 DataForm 读取生产表结构，编辑副本不影响运行数据。</summary>
        public FlowSchemaForm(DataForm owner) : this()
        {
            data = owner;
            foreach (ProductionField field in data.GetProductionSchema()) fields.Rows.Add(field.Name, field.Type);
            fields.DefaultValuesNeeded += (sender, args) => args.Row.Cells[1].Value = "Text";
            ok.Click += SaveFields;
        }

        /// <summary>验证并保存生产表结构，失败保留编辑值。</summary>
        private void SaveFields(object sender, EventArgs args)
        {
            fields.EndEdit();
            try
            {
                FlowPrograms.RequireIdle();
                List<ProductionField> schema = fields.Rows.Cast<DataGridViewRow>().Where(row => !row.IsNewRow)
                    .Select(row => new ProductionField { Name = Convert.ToString(row.Cells[0].Value), Type = Convert.ToString(row.Cells[1].Value) }).ToList();
                data.SaveProductionSchema(schema);
                DialogResult = DialogResult.OK;
            }
            catch (Exception error) { MessageBox.Show(string.Format(FlowText.T("error"), error.Message), FlowText.T("title")); }
        }
    }
}
