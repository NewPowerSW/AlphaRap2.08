using System.Windows.Forms;

namespace AlphaRap
{
    public partial class TBEditForm : Form
    {
        VisionproInterface EditInterface;
        public TBEditForm(VisionproInterface CogInterface)
        {
            InitializeComponent();
            UiTheme.ApplyDpi(this);
            EditInterface = CogInterface;

            cogToolBlockEditV21.Subject = EditInterface.TB;
            this.Text = EditInterface.StationName;
        }

        /// <summary>关闭前保存或重载算法；操作失败保留编辑窗口及当前内容。</summary>
        private void TBEditForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult = MessageBox.Show(
                MiddleLayer.LangMsg("TBEditForm", "msg_SaveConfirm", "保存 ToolBlock 吗？", "Save ToolBlock?", "¿Guardar ToolBlock?"),
                MiddleLayer.LangMsg("TBEditForm", "msg_SaveTitle", "保存", "Save", "Guardar"),
                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (DialogResult == DialogResult.Cancel) { e.Cancel = true; return; }
            string error;
            bool saved = DialogResult == DialogResult.Yes
                ? EditInterface.TrySaveTB(out error)
                : EditInterface.TryDiscardTBChanges(out error);
            if (saved) return;
            e.Cancel = true;
            MessageBox.Show(error, "ToolBlock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
