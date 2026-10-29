using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AlphaRap.FlowProgramming;

namespace AlphaRap.Configuration
{
    public partial class LogModulesForm : FlowLanguageForm
    {
        private readonly List<LogModuleDefinition> definitions = new List<LogModuleDefinition>();
        private DataForm owner;
        private LogModuleDefinition selected;
        private bool refreshing;
        private DataGridViewRow fieldContext;

        /// <summary>创建可在 VS 预览的固定模块编辑布局，不访问日志文件。</summary>
        public LogModulesForm()
        {
            InitializeComponent();
            if (UiTheme.IsDesignTime) return;
            moduleName.Leave += RefreshModuleCaption;
            add.Click += AddModule; moduleList.SelectedIndexChanged += SelectModule;
            moduleDelete.Click += DeleteModule; browse.Click += BrowseDirectory; ok.Click += Submit;
            moduleList.MouseDown += SelectContextModule;
            moduleMenu.Opening += (sender, args) => args.Cancel = moduleList.SelectedItem == null;
            fields.MouseDown += SelectContextField;
            fieldMenu.Opening += (sender, args) => args.Cancel = fieldContext == null;
            fieldDelete.Click += (sender, args) => { if (fieldContext != null && fieldContext.DataGridView == fields) fields.Rows.Remove(fieldContext); fieldContext = null; };
            fields.DefaultValuesNeeded += (sender, args) => args.Row.Cells[1].Value = "Text";
        }

        /// <summary>编辑 DataForm 草稿副本，取消不会影响模块或文件。</summary>
        public LogModulesForm(DataForm data) : this()
        {
            owner = data; definitions.AddRange(data.GetLogModules()); RefreshModules(null);
        }

        /// <summary>创建默认日志模块和一个内容字段。</summary>
        private void AddModule(object sender, EventArgs args)
        {
            CaptureSelection();
            int number = 1; while (definitions.Any(item => item.Name == "Log_" + number)) number++;
            LogModuleDefinition module = new LogModuleDefinition { Name = "Log_" + number };
            module.Directory = Path.Combine("LogData", "FlowLogs", module.Id);
            module.Fields.Add(new ProductionField { Name = "Value", Type = "Text" });
            definitions.Add(module); RefreshModules(module.Id);
        }

        /// <summary>保存当前行内输入后切换模块，不写文件。</summary>
        private void SelectModule(object sender, EventArgs args)
        {
            if (refreshing) return;
            CaptureSelection(); selected = moduleList.SelectedItem as LogModuleDefinition;
            moduleName.Text = selected == null ? "" : selected.Name;
            directory.Text = selected == null ? "" : selected.Directory;
            moduleName.Enabled = directory.Enabled = browse.Enabled = fields.Enabled = selected != null;
            fields.Rows.Clear();
            if (selected != null) foreach (ProductionField field in selected.Fields) fields.Rows.Add(field.Name, field.Type);
        }

        /// <summary>将正在编辑的模块名称、目录及字段保留到窗口副本。</summary>
        private void CaptureSelection()
        {
            if (selected == null) return;
            fields.EndEdit(); selected.Name = moduleName.Text; selected.Directory = directory.Text;
            selected.Fields = fields.Rows.Cast<DataGridViewRow>().Where(row => !row.IsNewRow)
                .Select(row => new ProductionField { Name = Convert.ToString(row.Cells[0].Value), Type = Convert.ToString(row.Cells[1].Value) }).ToList();
        }

        /// <summary>名称编辑完成后刷新列表标题，不重建字段编辑状态。</summary>
        private void RefreshModuleCaption(object sender, EventArgs args)
        {
            CaptureSelection();
            int index = moduleList.SelectedIndex;
            if (selected == null || index < 0) return;
            refreshing = true; moduleList.Items[index] = selected; moduleList.SelectedIndex = index; refreshing = false;
        }

        /// <summary>刷新模块列表并按稳定身份恢复选中项。</summary>
        private void RefreshModules(string id)
        {
            refreshing = true; moduleList.Items.Clear();
            foreach (LogModuleDefinition module in definitions) moduleList.Items.Add(module);
            moduleList.SelectedItem = definitions.FirstOrDefault(module => module.Id == id) ?? definitions.FirstOrDefault();
            refreshing = false; selected = null; SelectModule(this, EventArgs.Empty);
        }

        /// <summary>右键定位模块，空白区域不删除之前的选择。</summary>
        private void SelectContextModule(object sender, MouseEventArgs args)
        {
            if (args.Button != MouseButtons.Right) return;
            moduleList.SelectedIndex = moduleList.IndexFromPoint(args.Location);
        }

        /// <summary>从窗口副本移除模块，绑定检查在确定提交时完成。</summary>
        private void DeleteModule(object sender, EventArgs args)
        {
            LogModuleDefinition module = moduleList.SelectedItem as LogModuleDefinition;
            if (module == null) return;
            selected = null; definitions.Remove(module); RefreshModules(null);
        }

        /// <summary>右键按命中行删除字段，不影响其他行。</summary>
        private void SelectContextField(object sender, MouseEventArgs args)
        {
            if (args.Button != MouseButtons.Right) return;
            int row = fields.HitTest(args.X, args.Y).RowIndex;
            fieldContext = row >= 0 && !fields.Rows[row].IsNewRow ? fields.Rows[row] : null;
            if (fieldContext != null) fields.CurrentCell = fieldContext.Cells[0];
        }

        /// <summary>选择保存目录，不在选择时创建目录或写入文件。</summary>
        private void BrowseDirectory(object sender, EventArgs args)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                if (dialog.ShowDialog(this) == DialogResult.OK) directory.Text = dialog.SelectedPath;
        }

        /// <summary>校验并提交内存草稿，主界面保存后才供运行流程使用。</summary>
        private void Submit(object sender, EventArgs args)
        {
            try { CaptureSelection(); owner.SetLogModulesDraft(definitions); DialogResult = DialogResult.OK; }
            catch (Exception error) { MessageBox.Show(string.Format(FlowText.T("error"), error.Message), FlowText.T("log")); }
        }
    }
}
