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
            moduleText.Leave += RefreshModuleCaption;
            add.Click += AddModule; moduleList.SelectedIndexChanged += SelectModule;
            moduleDelete.Click += DeleteModule; browse.Click += BrowseDirectory; ok.Click += Submit;
            moduleSource.Click += ShowConfigurationSource;
            findConfiguration.Click += ShowConfigurationSource;
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
            owner = data;
            definitions.AddRange(data.GetLogModules());
            RefreshModules(definitions.FirstOrDefault(module => module.ShowOnHome)?.Id);
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
            moduleText.Text = selected?.Text ?? "";
            directory.Text = selected == null ? "" : selected.Directory;
            moduleText.Enabled = moduleName.Enabled = directory.Enabled = browse.Enabled = fields.Enabled = selected != null;
            showOnHome.Enabled = selected != null;
            showOnHome.Checked = selected?.ShowOnHome == true;
            fields.Rows.Clear();
            if (selected != null) foreach (ProductionField field in selected.Fields) fields.Rows.Add(field.Name, field.Type, field.Text);
        }

        /// <summary>将正在编辑的模块名称、目录及字段保留到窗口副本。</summary>
        private void CaptureSelection()
        {
            if (selected == null) return;
            fields.EndEdit(); selected.Name = moduleName.Text; selected.Text = moduleText.Text.Trim(); selected.Directory = directory.Text;
            selected.ShowOnHome = showOnHome.Checked;
            if (selected.ShowOnHome)
                foreach (LogModuleDefinition module in definitions.Where(module => module != selected)) module.ShowOnHome = false;
            selected.Fields = fields.Rows.Cast<DataGridViewRow>().Where(row => !row.IsNewRow)
                .Select(row => new ProductionField { Name = Convert.ToString(row.Cells[0].Value), Type = Convert.ToString(row.Cells[1].Value), Text = Convert.ToString(row.Cells[2].Value).Trim() }).ToList();
        }

        /// <summary>名称编辑完成后刷新列表标题，不重建字段编辑状态。</summary>
        private void RefreshModuleCaption(object sender, EventArgs args)
        {
            CaptureSelection();
            int index = moduleList.SelectedIndex;
            if (selected == null || index < 0) return;
            refreshing = true; moduleList.Items[index] = selected; moduleList.SelectedIndex = index; refreshing = false;
        }

        /// <summary>语言切换时重绘模块标题，保留窗口中正在编辑的字段。</summary>
        protected override void RefreshLanguageChoices()
        {
            refreshing = true;
            try
            {
                int index = moduleList.SelectedIndex;
                for (int row = 0; row < moduleList.Items.Count; row++) moduleList.Items[row] = moduleList.Items[row];
                moduleList.SelectedIndex = index;
            }
            finally { refreshing = false; }
        }

        /// <summary>刷新模块列表并按稳定身份恢复选中项。</summary>
        private void RefreshModules(string id)
        {
            refreshing = true; moduleList.Items.Clear();
            foreach (LogModuleDefinition module in definitions) moduleList.Items.Add(module);
            moduleList.SelectedItem = definitions.FirstOrDefault(module => module.Id == id) ?? definitions.FirstOrDefault();
            refreshing = false; selected = null; SelectModule(this, EventArgs.Empty);
        }

        /// <summary>按名称或 ID 查找日志定义，显示编辑草稿与主保存状态。</summary>
        private void ShowConfigurationSource(object sender, EventArgs args)
        {
            if (owner == null) return;
            CaptureSelection();
            var saved = owner.GetLogModules(false).ToDictionary(module => module.Id);
            var sources = definitions.Select(module =>
            {
                LogModuleDefinition published;
                saved.TryGetValue(module.Id, out published);
                return new ConfigurationSourceEntry(module.Id, module.Name, () => DescribeConfiguration(module, published));
            });
            using (var dialog = new ConfigurationSourceForm(LogModuleStore.FilePath, sources, selected?.Id))
                dialog.ShowDialog(this);
        }

        /// <summary>列出客户字段和当前来源状态，不写入日志或修改绑定。</summary>
        private static string DescribeConfiguration(LogModuleDefinition module, LogModuleDefinition published)
        {
            string scope = ConfigurationSourceForm.T("machineScope", "作用域　Setting（机器）", "Scope  Setting (machine)", "Ámbito  Setting (máquina)");
            bool changed = published == null || published.Name != module.Name || published.Text != module.Text || published.Directory != module.Directory ||
                published.ShowOnHome != module.ShowOnHome || published.Fields.Count != module.Fields.Count ||
                module.Fields.Where((field, index) => field.Name != published.Fields[index].Name || field.Type != published.Fields[index].Type || field.Text != published.Fields[index].Text).Any();
            string binding = module.ShowOnHome
                ? ConfigurationSourceForm.T("homeYes", "首页生产记录　是", "Home production records  Yes", "Registros de producción de Inicio  Sí")
                : ConfigurationSourceForm.T("homeNo", "首页生产记录　否", "Home production records  No", "Registros de producción de Inicio  No");
            var lines = new[]
            {
                module.Name, "ID  " + module.Id, scope,
                changed ? ConfigurationSourceForm.T("draft", "有未保存修改。以下为当前配置。", "Unsaved changes. Current configuration shown.", "Hay cambios sin guardar. Se muestra la configuración actual.") : "",
                FlowText.T("logDirectory") + "  " + module.Directory,
                binding, "",
                ConfigurationSourceForm.T("logColumns", "记录列（按保存顺序）", "Record columns in save order", "Columnas en orden de guardado"),
                ConfigurationSourceForm.T("logTime", "Time  自动生成", "Time  Automatic", "Time  Automático")
            };
            return string.Join(Environment.NewLine, lines.Concat(module.Fields.Select(field => field.Name + "  ·  " + field.Type)));
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
