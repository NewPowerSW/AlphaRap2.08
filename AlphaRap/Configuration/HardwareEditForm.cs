using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace AlphaRap.Configuration
{
    public partial class HardwareEditForm : Form
    {
        private HardwareDefinition draft;
        private Action<HardwareDefinition> commit;
        private readonly Dictionary<string, string> edited = new Dictionary<string, string>();
        private bool filling;

        private sealed class TypeChoice
        {
            public string Value { get; set; }
            public string Text { get { return string.IsNullOrEmpty(Value) ? HardwareText.T("controlTypeDefault") : Value.Substring(Value.IndexOf('|') + 1); } }
        }

        /// <summary>建立固定资源参数编辑模板。</summary>
        public HardwareEditForm() { InitializeComponent(); }

        /// <summary>编辑明确 DTO 字段，宿主提交成功后才关闭窗口。</summary>
        public HardwareEditForm(HardwareDefinition value, Action<HardwareDefinition> save) : this()
        {
            draft = value.Copy(); commit = save;
            FillFields();
            ok.Click += Save;
            parameters.CellFormatting += (sender,args) => {
                if (args.RowIndex < 0) return;
                string fieldName = Convert.ToString(parameters.Rows[args.RowIndex].Cells[0].Value);
                if (args.ColumnIndex == 1 && fieldName == "ControlType")
                {
                    string identity = Convert.ToString(args.Value);
                    args.Value = identity.Length == 0 ? HardwareText.T("controlTypeDefault") : identity.Substring(identity.IndexOf('|') + 1);
                    args.FormattingApplied = true;
                    return;
                }
                if (args.ColumnIndex != 0) return;
                string name = Convert.ToString(args.Value);
                args.Value = HardwareText.T(name == "Port" ? draft.Kind == "Axis" ? "AxisPort" : "IoPort" : name); args.FormattingApplied = true;
            };
            parameters.CellValueChanged += VendorChanged;
            parameters.CurrentCellDirtyStateChanged += (sender, args) => { if (parameters.IsCurrentCellDirty && parameters.CurrentCell is DataGridViewComboBoxCell) parameters.CommitEdit(DataGridViewDataErrorContexts.Commit); };
            ApplyLanguage(null,EventArgs.Empty); MiddleLayer.LanguageChanged += ApplyLanguage;
            Disposed += (sender,args) => MiddleLayer.LanguageChanged -= ApplyLanguage;
            Load += (sender,args) => MiddleLayer.RegisterAndApplyLanguage(this,"HardwareResources");
        }

        /// <summary>枚举和布尔字段使用可选列表，其余字段保留原始文本值。</summary>
        private void AddField(string name)
        {
            System.Reflection.FieldInfo field = typeof(HardwareDefinition).GetField(name);
            string value;
            if (!edited.TryGetValue(name, out value)) value = Convert.ToString(field.GetValue(draft), CultureInfo.InvariantCulture);
            int row = parameters.Rows.Add(name, value);
            DataGridViewComboBoxCell choices = null;
            if (name == "ControlType")
            {
                List<TypeChoice> types = new List<TypeChoice> { new TypeChoice { Value="" } };
                types.AddRange(HardwareControlTypes.Available(draft.Kind)
                    .Where(type => type != HardwareControlTypes.Resolve(draft.Kind,""))
                    .Select(type => new TypeChoice { Value=HardwareControlTypes.Identity(type) }));
                if (!types.Any(type => type.Value == value)) types.Add(new TypeChoice { Value=value });
                choices = new DataGridViewComboBoxCell { DataSource=types, DisplayMember="Text", ValueMember="Value" };
            }
            if (field.FieldType.IsEnum) { choices = new DataGridViewComboBoxCell(); foreach (string item in name == "Vendor" ? draft.Vendors() : Enum.GetNames(field.FieldType)) choices.Items.Add(item); }
            if (field.FieldType == typeof(bool)) { choices = new DataGridViewComboBoxCell(); choices.Items.AddRange("True","False"); }
            if (choices == null) return;
            choices.Value = value; parameters.Rows[row].Cells[1] = choices;
        }

        /// <summary>按当前驱动显示实际有效字段，保留切换前输入的文本。</summary>
        private void FillFields()
        {
            filling = true;
            try { parameters.Rows.Clear(); foreach (string name in draft.Fields()) AddField(name); }
            finally { filling = false; }
        }

        /// <summary>记住编辑文本，不在厂商或语言切换时强制解析未完成的数值。</summary>
        private void CaptureText()
        {
            foreach (DataGridViewRow row in parameters.Rows) edited[Convert.ToString(row.Cells[0].Value)] = Convert.ToString(row.Cells[1].Value);
        }

        /// <summary>厂商选定后结束当前单元格事件，再刷新该驱动的有效参数。</summary>
        private void VendorChanged(object sender, DataGridViewCellEventArgs args)
        {
            if (filling || args.RowIndex < 0 || args.ColumnIndex != 1 || Convert.ToString(parameters.Rows[args.RowIndex].Cells[0].Value) != "Vendor") return;
            if (!IsHandleCreated) { ChangeVendor(); return; }
            BeginInvoke(new Action(() => { if (!IsDisposed) ChangeVendor(); }));
        }

        /// <summary>仅修改草稿厂商和显示字段，不创建或重新连接驱动。</summary>
        private void ChangeVendor()
        {
            parameters.EndEdit(); CaptureText();
            string vendor;
            if (!edited.TryGetValue("Vendor", out vendor) || vendor == draft.Vendor.ToString()) return;
            draft.SetValue("Vendor", vendor); FillFields();
        }

        /// <summary>全部解析和参数校验完成后，一次提交资源定义副本。</summary>
        private void Save(object sender,EventArgs args)
        {
            try
            {
                parameters.EndEdit(); ChangeVendor(); CaptureText(); HardwareDefinition candidate = draft.Copy();
                foreach (string name in candidate.Fields()) candidate.SetValue(name,edited[name]);
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
