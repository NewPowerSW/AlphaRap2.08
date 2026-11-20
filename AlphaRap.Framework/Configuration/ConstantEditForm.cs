using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace AlphaRap.Configuration
{
    public partial class ConstantEditForm : Form
    {
        private readonly Dictionary<string, string> textDrafts = new Dictionary<string, string>
        {
            { ConstantStore.Int32Type, "0" },
            { "Number", "0.0" },
            { "Text", "" }
        };
        private Action<ConstantValue> commit;
        private string valueEditorType;
        private bool UsesBooleanValue => valueEditorType == "Boolean";

        /// <summary>创建独立于设备状态的编辑布局。</summary>
        public ConstantEditForm() { InitializeComponent(); }

        /// <summary>编辑已有常量时保留原类型，提交成功才关闭窗口。</summary>
        public ConstantEditForm(ConstantValue value, Action<ConstantValue> save) : this(value, false, save) { }

        /// <summary>新增时提供类型选择，编辑时显示当前固定类型。</summary>
        public ConstantEditForm(ConstantValue value, bool isNew, Action<ConstantValue> save) : this()
        {
            commit = save;
            constantKeyValue.Text = value.Key;
            constantDescriptionValue.Text = value.Description;
            if (value.Type != null && value.Type != "Boolean" && value.Value != null) textDrafts[value.Type] = value.Value;
            if (value.Type == "Boolean" && bool.TryParse(value.Value, out bool flag)) constantBooleanValue.SelectedItem = flag ? "true" : "false";
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

        /// <summary>保存切换前的值草稿，并显示所选类型对应的草稿或默认值。</summary>
        private void UpdateValueEditor()
        {
            if (valueEditorType != null && !UsesBooleanValue) textDrafts[valueEditorType] = constantValueValue.Text;
            valueEditorType = (string)constantTypeValue.SelectedItem;
            constantValueValue.Visible = !UsesBooleanValue;
            constantBooleanValue.Visible = UsesBooleanValue;
            if (UsesBooleanValue) return;
            constantValueValue.Text = valueEditorType != null && textDrafts.TryGetValue(valueEditorType, out string draft) ? draft : "";
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
