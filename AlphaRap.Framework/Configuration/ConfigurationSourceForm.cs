using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AlphaRap.Configuration
{
    internal sealed class ConfigurationSourceEntry
    {
        internal string Id { get; }
        internal string Name { get; }
        internal Func<string> Describe { get; }

        /// <summary>记录资源身份和用于只读显示的说明。</summary>
        internal ConfigurationSourceEntry(string id, string name, Func<string> describe)
        {
            Id = id;
            Name = name;
            Describe = describe;
        }

        /// <summary>列表用配置中的原名称显示资源。</summary>
        public override string ToString() { return Name; }
    }

    public partial class ConfigurationSourceForm : Form
    {
        private ConfigurationSourceEntry[] entries = new ConfigurationSourceEntry[0];
        private string loadFailure;

        /// <summary>初始化只读配置定位窗口。</summary>
        public ConfigurationSourceForm()
        {
            InitializeComponent();
            if (UiTheme.IsDesignTime) return;
            sourceSearch.TextChanged += (sender, args) => FilterEntries((sourceList.SelectedItem as ConfigurationSourceEntry)?.Id);
            sourceList.SelectedIndexChanged += (sender, args) => ShowDetails();
            sourceCopy.Click += CopyPath;
            FrameworkLanguage.LanguageChanged += RefreshLanguage;
            Disposed += (sender, args) => FrameworkLanguage.LanguageChanged -= RefreshLanguage;
            RefreshLanguage(this, EventArgs.Empty);
            FrameworkLanguage.RegisterAndApplyLanguage(this, Name);
        }

        /// <summary>以指定文件和资源快照初始化列表；可直接定位命中的资源。</summary>
        internal ConfigurationSourceForm(string file, IEnumerable<ConfigurationSourceEntry> items, string selectedId = null, string error = null)
            : this()
        {
            sourcePath.Text = file;
            entries = items.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase).ToArray();
            loadFailure = error;
            FilterEntries(selectedId);
        }

        /// <summary>按资源名或稳定 ID 查找，优先保留当前选中项。</summary>
        private void FilterEntries(string selectedId)
        {
            string query = sourceSearch.Text.Trim();
            ConfigurationSourceEntry[] matches = entries.Where(item =>
                (item.Name ?? "").IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                item.Id.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0).ToArray();

            sourceList.BeginUpdate();
            try
            {
                sourceList.Items.Clear();
                sourceList.Items.AddRange(matches);
                sourceList.SelectedItem = matches.FirstOrDefault(item => item.Id == selectedId) ?? matches.FirstOrDefault();
            }
            finally { sourceList.EndUpdate(); }
            ShowDetails();
        }

        /// <summary>显示匹配项的配置说明，加载失败时显示原始诊断。</summary>
        private void ShowDetails()
        {
            var item = sourceList.SelectedItem as ConfigurationSourceEntry;
            sourceDetails.Text = item?.Describe() ?? loadFailure ??
                (entries.Length == 0
                    ? T("empty", "此文件没有可显示的配置记录。", "This file has no configuration entries to display.", "El archivo no tiene registros de configuración para mostrar.")
                    : T("noMatch", "没有匹配的名称或 ID。", "No matching name or ID.", "Ningún nombre o ID coincide."));
        }

        /// <summary>复制完整配置路径；剪贴板暂时不可用时选中路径供手动复制。</summary>
        private void CopyPath(object sender, EventArgs args)
        {
            if (sourcePath.TextLength == 0) return;
            try { Clipboard.SetText(sourcePath.Text); }
            catch (ExternalException) { sourcePath.Focus(); sourcePath.SelectAll(); }
        }

        /// <summary>刷新文案，保留查找条件和当前选中记录。</summary>
        private void RefreshLanguage(object sender, EventArgs args)
        {
            Text = T(Name, "配置来源", "Configuration source", "Origen de configuración");
            sourcePathLabel.Text = T(sourcePathLabel.Name, "配置文件", "Configuration file", "Archivo de configuración");
            sourceSearchLabel.Text = T(sourceSearchLabel.Name, "查找名称或 ID", "Find name or ID", "Buscar nombre o ID");
            sourceCopy.Text = T(sourceCopy.Name, "复制路径", "Copy path", "Copiar ruta");
            sourceClose.Text = T(sourceClose.Name, "关闭", "Close", "Cerrar");
            ShowDetails();
        }

        /// <summary>以三语底稿显示配置来源相关文案。</summary>
        internal static string T(string key, string zh, string en, string es)
        {
            return UiTheme.IsDesignTime ? zh : FrameworkLanguage.LangMsg("ConfigurationSourceForm", key, zh, en, es);
        }
    }
}
