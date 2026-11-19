using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AlphaRap.FlowProgramming;
using NPSDK;

namespace AlphaRap
{
    public partial class Flow : Form
    {
        private readonly Dictionary<Flow_BaseForm, TabPage> pages = new Dictionary<Flow_BaseForm, TabPage>();

        /// <summary>创建工站监视页。</summary>
        public Flow()
        {
            InitializeComponent();
            if (UiTheme.IsDesignTime) return;
            FrameworkLanguage.RegisterAndApplyLanguage(this, Name);
            UiTheme.ApplyDpi(this);
            Load += (sender, args) => AddFlowChart();
            VisibleChanged += (sender, args) =>
            {
                timer.Enabled = Visible;
                if (Visible && IsHandleCreated) AddFlowChart();
            };
            timer.Tick += (sender, args) => RefreshRuntime();
            tabControl1.SelectedIndexChanged += (sender, args) => RefreshRuntime();
        }

        /// <summary>将运行中的工站按页签显示。</summary>
        public void AddFlowChart()
        {
            FlowPrograms.Install();
            tabControl1.SuspendLayout();
            try
            {
                foreach (KeyValuePair<Flow_BaseForm, TabPage> item in pages.ToArray())
                {
                    FlowModuleEditor editor = (FlowModuleEditor)item.Value.Tag;
                    if (!item.Key.IsDisposed && !editor.Program.Removed) continue;
                    item.Key.Hide();
                    item.Value.Controls.Remove(item.Key);
                    tabControl1.TabPages.Remove(item.Value);
                    item.Value.Dispose();
                    pages.Remove(item.Key);
                }
                foreach (Flow_BaseForm form in Flow_Module.FlowChart_ModuleList.ToArray())
                {
                    if (form.IsDisposed || pages.ContainsKey(form)) continue;
                    FlowModuleEditor editor = FlowPrograms.Attach(form);
                    if (editor.Program.Removed) continue;
                    TabPage page = new TabPage(form.Name) { Name = "flowModule_" + pages.Count + "_dyn", Tag = editor, AutoScroll = true };
                    form.TopLevel = false; form.FormBorderStyle = FormBorderStyle.None; form.Dock = DockStyle.Fill; form.AutoScroll = true;
                    page.Controls.Add(form); tabControl1.TabPages.Add(page); pages.Add(form, page);
                    form.Show();
                }
            }
            finally { tabControl1.ResumeLayout(); }
            RefreshRuntime();
        }

        /// <summary>显示当前工站的耗时及错误。</summary>
        private void RefreshRuntime()
        {
            FlowModuleEditor module = tabControl1.SelectedTab?.Tag as FlowModuleEditor;
            module?.RefreshElapsed();
            string error = module?.ExecutionError;
            status.Text = string.IsNullOrEmpty(error) ? "" : string.Format(FlowText.T("error"), error);
            status.Visible = status.Text.Length > 0;
        }
    }
}
