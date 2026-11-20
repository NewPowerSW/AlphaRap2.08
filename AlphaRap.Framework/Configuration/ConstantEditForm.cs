using System;
using System.Windows.Forms;

namespace AlphaRap.Configuration
{
    public partial class ConstantEditForm : Form
    {
        private Action<ConstantValue> commit;
        private bool UsesBooleanValue => (string)constantTypeValue.SelectedItem == "Boolean";

        /// <summary>创建独立于设备状态的编辑布局。</summary>
        public ConstantEditForm() { InitializeComponent(); }

        /// <summary>编辑已有常量时保留原类型，提交成功才关闭窗口。</summary>
        public ConstantEditForm(ConstantValue value, Action<ConstantValue> save) : this(value, false, save) { }

        /// <summary>新增时提供类型选择，编辑时显示当前固定类型。</summary>
        public ConstantEditForm(ConstantValue value, bool isNew, Action<ConstantValue> save) : this()
        {
            commit = save;
            constantKeyValue.Text = value.Key;
            constantValueValue.Text = value.Value;
            constantDescriptionValue.Text = value.Description;
            if (bool.TryParse(value.Value, out bool flag)) constantBooleanValue.SelectedItem = flag ? "true" : "false";
            else if (isNew) constantBooleanValue.SelectedItem = "false";
            constantTypeValue.Format += (sender, args) => args.Value = ConstantText.TypeName(Convert.ToString(args.ListItem));
            if (isNew) constantTypeValue.Items.AddRange(ConstantStore.Types);
            else constantTypeValue.Items.Add(value.Type);
            constantTypeValue.SelectedItem = value.Type;
            constantTypeValue.Enabled = isNew;
            constantTypeValue.SelectedIndexChanged += (sender, args) => UpdateValueEditor();
            UpdateValueEditor();
            if (!isNew)
            {
                ActiveControl = UsesBooleanValue ? (Control)constantBooleanValue : constantValueValue;
                if (!UsesBooleanValue) constantValueValue.SelectAll();
            }
            constantOk.Click += Save;
            ApplyLanguage(null, EventArgs.Empty);
            FrameworkLanguage.LanguageChanged += ApplyLanguage;
            Disposed += (sender, args) => FrameworkLanguage.LanguageChanged -= ApplyLanguage;
            Load += (sender, args) => FrameworkLanguage.RegisterAndApplyLanguage(this, "DesignerConstants");
        }

        /// <summary>根据所选类型显示值下拉框或文本框，两个控件各自保留草稿。</summary>
        private void UpdateValueEditor()
        {
            bool useBooleanValue = UsesBooleanValue;
            constantValueValue.Visible = !useBooleanValue;
            constantBooleanValue.Visible = useBooleanValue;
        }

        /// <summary>校验并提交，失败时保留输入以便修正。</summary>
        private void Save(object sender, EventArgs args)
        {
            try
            {
                commit(new ConstantValue
                {
                    Key = constantKeyValue.Text,
                    Type = Convert.ToString(constantTypeValue.SelectedItem),
                    Value = UsesBooleanValue ? Convert.ToString(constantBooleanValue.SelectedItem) : constantValueValue.Text,
                    Description = constantDescriptionValue.Text
                });
                DialogResult = DialogResult.OK;
            }
            catch (Exception error) { MessageBox.Show(error.Message, ConstantText.T("title")); }
        }

        /// <summary>切换语言时刷新标签与按钮，保留输入和类型选择。</summary>
        private void ApplyLanguage(object sender, EventArgs args)
        {
            ConstantText.Apply(this);
            constantOk.Text = FlowProgramming.FlowText.T("ok"); constantCancel.Text = FlowProgramming.FlowText.T("cancel");
        }
    }
}
