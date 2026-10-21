using System;
using System.Data;
using System.Windows.Forms;

namespace AlphaRap.MenuForm
{
    public partial class LanguageSetting : Form
    {
        private DataTable dtTable;

        /// <summary>初始化语言编辑页，控件和表头使用框架语言登记。</summary>
        public LanguageSetting()
        {
            InitializeComponent();
            UiTheme.ApplyDpi(this);
            if (!UiTheme.IsDesignTime) MiddleLayer.RegisterAndApplyLanguage(this, Name);
        }

        /// <summary>从配置的语言目录读取三语条目；读取失败时保留当前编辑内容。</summary>
        public void LoadTables()
        {
            try
            {
                DataTable loaded = LanguageCatalog.Load();
                dtTable = loaded;
                bindingSource1.DataSource = dtTable;
                dataGridView1.DataSource = bindingSource1;
                MiddleLayer.RegisterAndApplyLanguage(dataGridView1, Name);
                SetEditing(false);
            }
            catch (Exception ex) { ShowError(ex); }
        }

        /// <summary>打开页面时加载语言条目。</summary>
        private void LanguageSetting_Load(object sender, EventArgs e) { LoadTables(); }

        /// <summary>允许编辑译文，锁定窗体名和条目键。</summary>
        private void btnUpdate_Click(object sender, EventArgs e) { SetEditing(true); }

        /// <summary>提交单元格并按稳定键保存三语译文，立即刷新所有页面。</summary>
        private void btnSave_Click(object sender, EventArgs e)
        {
            string message = MiddleLayer.LangMsg(Name, "msg_SaveConfirm", "是否保存语言表？", "Save the translations?", "¿Guardar las traducciones?");
            string title = MiddleLayer.LangMsg(Name, "msg_SaveTitle", "保存语言表", "Save translations", "Guardar traducciones");
            if (MessageBox.Show(message, title, MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            if (!dataGridView1.EndEdit()) return;
            bindingSource1.EndEdit();
            try
            {
                LanguageCatalog.Save(dtTable);
                SetEditing(false);
            }
            catch (Exception ex) { ShowError(ex); }
        }

        /// <summary>重新读取语言包。</summary>
        private void btnRefresh_Click(object sender, EventArgs e) { LoadTables(); }

        /// <summary>放弃未保存译文并重新加载。</summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            dataGridView1.CancelEdit();
            bindingSource1.CancelEdit();
            LoadTables();
        }

        /// <summary>按列控制译文编辑权限，不逐层修改每个单元格。</summary>
        private void SetEditing(bool editing)
        {
            foreach (DataGridViewColumn column in dataGridView1.Columns)
                column.ReadOnly = !editing || column.Index < 3;
            btnUpdate.Enabled = !editing;
            btnSave.Enabled = editing;
            btnCancel.Enabled = editing;
            btnRefresh.Enabled = !editing;
        }

        /// <summary>显示语言文件读取或保存失败的原因。</summary>
        private void ShowError(Exception error)
        {
            MessageBox.Show(error.Message, MiddleLayer.LangMsg(Name, "msg_FileError", "语言表操作失败", "Language file operation failed", "Error al gestionar los archivos de idioma"),
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
