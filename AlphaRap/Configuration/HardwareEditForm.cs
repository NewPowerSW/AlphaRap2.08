using System;
using System.Globalization;
using System.Windows.Forms;

namespace AlphaRap.Configuration
{
    public partial class HardwareEditForm : Form
    {
        private HardwareDefinition draft;
        private Action<HardwareDefinition> commit;

        /// <summary>建立固定资源参数编辑模板。</summary>
        public HardwareEditForm() { InitializeComponent(); }

        /// <summary>编辑明确 DTO 字段，宿主提交成功后才关闭窗口。</summary>
        public HardwareEditForm(HardwareDefinition value, Action<HardwareDefinition> save) : this()
        {
            draft = value.Copy(); commit = save;
            foreach (string name in draft.Fields()) AddField(name);
            ok.Click += Save;
            parameters.CellFormatting += (sender,args) => { if (args.ColumnIndex == 0 && args.RowIndex >= 0) { args.Value = HardwareText.T(Convert.ToString(args.Value)); args.FormattingApplied = true; } };
            ApplyLanguage(null,EventArgs.Empty); MiddleLayer.LanguageChanged += ApplyLanguage;
            Disposed += (sender,args) => MiddleLayer.LanguageChanged -= ApplyLanguage;
            Load += (sender,args) => MiddleLayer.RegisterAndApplyLanguage(this,"HardwareResources");
        }

        /// <summary>枚举和布尔字段使用可选列表，其余字段保留原始文本值。</summary>
        private void AddField(string name)
        {
            System.Reflection.FieldInfo field = typeof(HardwareDefinition).GetField(name);
            int row = parameters.Rows.Add(name,Convert.ToString(field.GetValue(draft),CultureInfo.InvariantCulture));
            DataGridViewComboBoxCell choices = null;
            if (field.FieldType.IsEnum) { choices = new DataGridViewComboBoxCell(); foreach (string item in Enum.GetNames(field.FieldType)) choices.Items.Add(item); }
            if (field.FieldType == typeof(bool)) { choices = new DataGridViewComboBoxCell(); choices.Items.AddRange("True","False"); }
            if (choices == null) return;
            choices.Value = Convert.ToString(field.GetValue(draft),CultureInfo.InvariantCulture); parameters.Rows[row].Cells[1] = choices;
        }

        /// <summary>全部解析和参数校验完成后，一次提交资源定义副本。</summary>
        private void Save(object sender,EventArgs args)
        {
            try
            {
                parameters.EndEdit(); HardwareDefinition candidate = draft.Copy();
                foreach (DataGridViewRow row in parameters.Rows) candidate.SetValue(Convert.ToString(row.Cells[0].Value),Convert.ToString(row.Cells[1].Value));
                candidate.Validate(); commit(candidate); DialogResult = DialogResult.OK;
            }
            catch (Exception error) { MessageBox.Show(error.Message,HardwareText.T("title")); }
        }

        /// <summary>切换语言只刷新说明与标签，不提交当前活动编辑单元格。</summary>
        private void ApplyLanguage(object sender,EventArgs args)
        {
            HardwareText.Apply(this); parameters.Invalidate();
            ok.Text = FlowProgramming.FlowText.T("ok"); cancel.Text = FlowProgramming.FlowText.T("cancel");
        }
    }
}
