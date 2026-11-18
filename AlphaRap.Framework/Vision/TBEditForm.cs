using System;
using System.Windows.Forms;

namespace AlphaRap
{
    public partial class TBEditForm : Form
    {
        private readonly VisionproInterface editInterface;
        private readonly VisionproInterface.ToolBlockSnapshot original;
        private readonly VpAlgorithmTemplates.PositioningTemplate[] positioningTemplates;

        /// <summary>在设计器中显示窗口布局。</summary>
        public TBEditForm()
        {
            InitializeComponent();
        }

        /// <summary>编辑实际工位的算法，并保存进入编辑器前的草稿供取消恢复。</summary>
        public TBEditForm(VisionproInterface CogInterface) : this()
        {
            UiTheme.ApplyDpi(this);
            editInterface = CogInterface;
            original = editInterface.CaptureToolBlockSnapshot();
            cogToolBlockEditV21.Subject = editInterface.TB;
            Text = editInterface.StationName;
            pnlTemplates.Visible = original.CanChoosePositioningTemplate;
            if (!original.CanChoosePositioningTemplate) return;

            positioningTemplates = VpAlgorithmTemplates.GetPositioningTemplates();
            TemplateLanguageChanged(this, EventArgs.Empty);
            FrameworkLanguage.RegisterAndApplyLanguage(pnlTemplates, "TBEditForm");
            FrameworkLanguage.LanguageChanged += TemplateLanguageChanged;
            Disposed += delegate { FrameworkLanguage.LanguageChanged -= TemplateLanguageChanged; };
        }

        /// <summary>刷新模板名称和按钮文案，保留当前选项及编辑器中的工具块。</summary>
        private void TemplateLanguageChanged(object sender, EventArgs e)
        {
            lblTemplates.Text = FrameworkLanguage.LangMsg("TBEditForm", "lblTemplates",
                "定位模板", "Positioning template", "Plantilla de posicionamiento");
            btnUseTemplate.Text = FrameworkLanguage.LangMsg("TBEditForm", "btnUseTemplate",
                "使用此模板", "Use template", "Usar plantilla");
            object selected = cmbTemplates.SelectedItem ?? positioningTemplates[0];
            cmbTemplates.BeginUpdate();
            try
            {
                cmbTemplates.Items.Clear();
                cmbTemplates.Items.AddRange(positioningTemplates);
                cmbTemplates.SelectedItem = selected;
            }
            finally { cmbTemplates.EndUpdate(); }
        }

        /// <summary>明确确认后更换未定稿的工具链，保留主界面保存及窗口取消的入口。</summary>
        private void UseTemplateClicked(object sender, EventArgs e)
        {
            var template = cmbTemplates.SelectedItem as VpAlgorithmTemplates.PositioningTemplate;
            if (editInterface == null || template == null) return;
            string prompt = FrameworkLanguage.LangMsg("TBEditForm", "msg_ReplaceDraft",
                "使用“{0}”将替换此窗口中当前的工具和参数。继续吗？",
                "Using “{0}” replaces the tools and settings currently in this window. Continue?",
                "Usar «{0}» reemplaza las herramientas y ajustes de esta ventana. ¿Continuar?");
            if (MessageBox.Show(this, string.Format(prompt, template), lblTemplates.Text,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
            string error;
            if (!InstallTemplate(template, out error))
                MessageBox.Show(this, error, lblTemplates.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>在模板创建成功后切换 Cognex 编辑器，并释放被替换的草稿。</summary>
        private bool InstallTemplate(VpAlgorithmTemplates.PositioningTemplate template, out string error)
        {
            var previous = editInterface.TB;
            if (!editInterface.TryApplyPositioningTemplate(template, out error)) return false;
            cogToolBlockEditV21.Subject = editInterface.TB;
            previous.Dispose();
            return true;
        }

        /// <summary>关闭时确认内存草稿或恢复编辑前内容；文件由主界面保存。</summary>
        private void TBEditForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (editInterface == null) return;
            DialogResult = MessageBox.Show(this,
                FrameworkLanguage.LangMsg("TBEditForm", "msg_KeepDraft", "保留 ToolBlock 修改吗？修改由主界面【保存】统一保存。", "Keep the ToolBlock changes? Use Save in the main window to save them.", "¿Conservar los cambios de ToolBlock? Use Guardar en la ventana principal para guardarlos."),
                FrameworkLanguage.LangMsg("TBEditForm", "msg_DraftTitle", "算法草稿", "Algorithm draft", "Borrador del algoritmo"),
                MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (DialogResult == DialogResult.Cancel) { e.Cancel = true; return; }
            try
            {
                CompleteEdit(DialogResult == DialogResult.Yes);
            }
            catch (Exception error)
            {
                e.Cancel = true;
                MessageBox.Show(this, error.Message, "ToolBlock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>结束编辑并释放未使用的工具块；放弃时还原进入窗口前的草稿。</summary>
        private void CompleteEdit(bool keepDraft)
        {
            if (keepDraft)
            {
                editInterface.MarkToolBlockDraft();
                original.ToolBlock.Dispose();
                return;
            }
            var edited = editInterface.TB;
            editInterface.RestoreToolBlockSnapshot(original);
            cogToolBlockEditV21.Subject = null;
            edited.Dispose();
        }
    }
}
