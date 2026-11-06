using System;
using System.Windows.Forms;

namespace AlphaRap.Configuration
{
    public partial class ConstantEditForm : Form
    {
        private Action<ConstantValue> commit;

        /// <summary>创建独立于设备状态的编辑布局。</summary>
        public ConstantEditForm() { InitializeComponent(); }

        /// <summary>编辑副本，提交成功才关闭窗口。</summary>
        public ConstantEditForm(ConstantValue value, Action<ConstantValue> save) : this()
        {
            commit = save;
            constantKeyValue.Text = value.Key; constantValueValue.Text = value.Value; constantDescriptionValue.Text = value.Description;
            constantTypeValue.Items.AddRange(ConstantStore.Types); constantTypeValue.SelectedItem = value.Type;
            constantTypeValue.Format += (sender, args) => args.Value = ConstantText.T(Convert.ToString(args.ListItem));
            constantOk.Click += Save;
            ApplyLanguage(null, EventArgs.Empty);
            MiddleLayer.LanguageChanged += ApplyLanguage;
            Disposed += (sender, args) => MiddleLayer.LanguageChanged -= ApplyLanguage;
            Load += (sender, args) => MiddleLayer.RegisterAndApplyLanguage(this, "DesignerConstants");
        }

        /// <summary>校验并提交，失败时保留输入以便修正。</summary>
        private void Save(object sender, EventArgs args)
        {
            try
            {
                commit(new ConstantValue { Key = constantKeyValue.Text, Type = Convert.ToString(constantTypeValue.SelectedItem), Value = constantValueValue.Text, Description = constantDescriptionValue.Text });
                DialogResult = DialogResult.OK;
            }
            catch (Exception error) { MessageBox.Show(error.Message, ConstantText.T("title")); }
        }

        /// <summary>只刷新标签及类型显示，不重建数据。</summary>
        private void ApplyLanguage(object sender, EventArgs args)
        {
            ConstantText.Apply(this);
            constantOk.Text = FlowProgramming.FlowText.T("ok"); constantCancel.Text = FlowProgramming.FlowText.T("cancel");
            constantTypeValue.Refresh();
        }
    }
}
