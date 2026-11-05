using System.Windows.Forms;

namespace AlphaRap
{
    public partial class TBEditForm : Form
    {
        VisionproInterface EditInterface;
        private readonly VisionproInterface.ToolBlockSnapshot original;

        /// <summary>编辑实际工位的算法，并保存进入编辑器前的草稿供取消恢复。</summary>
        public TBEditForm(VisionproInterface CogInterface)
        {
            InitializeComponent();
            UiTheme.ApplyDpi(this);
            EditInterface = CogInterface;
            original = EditInterface.CaptureToolBlockSnapshot();

            cogToolBlockEditV21.Subject = EditInterface.TB;
            this.Text = EditInterface.StationName;
        }

        /// <summary>关闭时确认内存草稿或恢复编辑前内容；文件由主界面保存。</summary>
        private void TBEditForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult = MessageBox.Show(
                MiddleLayer.LangMsg("TBEditForm", "msg_KeepDraft", "保留 ToolBlock 修改吗？修改由主界面【保存】统一保存。", "Keep the ToolBlock changes? Use Save in the main window to save them.", "¿Conservar los cambios de ToolBlock? Use Guardar en la ventana principal para guardarlos."),
                MiddleLayer.LangMsg("TBEditForm", "msg_DraftTitle", "算法草稿", "Algorithm draft", "Borrador del algoritmo"),
                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (DialogResult == DialogResult.Cancel) { e.Cancel = true; return; }
            try
            {
                if (DialogResult == DialogResult.Yes) EditInterface.MarkToolBlockDraft();
                else EditInterface.RestoreToolBlockSnapshot(original);
            }
            catch (System.Exception error)
            {
                e.Cancel = true;
                MessageBox.Show(error.Message, "ToolBlock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
