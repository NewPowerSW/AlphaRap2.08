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
        private bool filling;
        private readonly Dictionary<Flow_BaseForm, TabPage> pages = new Dictionary<Flow_BaseForm, TabPage>();
        /// <summary>Bind the engineering commands to all registered base-form instances.</summary>
        public Flow()
        {
            InitializeComponent();
            if (UiTheme.IsDesignTime) return;
            FlowLanguageForm.ApplyTexts(this);
            MiddleLayer.RegisterAndApplyLanguage(this, Name);
            UiTheme.ApplyDpi(this);
            Load += (sender, args) => { AddFlowChart(); RefreshSelection(); timer.Start(); };
            VisibleChanged += (sender, args) => { if (Visible && IsHandleCreated) AddFlowChart(); };
            MiddleLayer.LanguageChanged += ApplyLanguage;
            Disposed += (sender, args) => MiddleLayer.LanguageChanged -= ApplyLanguage;
            tabControl1.SelectedIndexChanged += (sender, args) => RefreshSelection();
            edit.CheckedChanged += ToggleEditing;
            modeValue.SelectedIndexChanged += UpdateDefinition;
            initialValue.SelectedIndexChanged += UpdateDefinition;
            autoValue.SelectedIndexChanged += UpdateDefinition;
            addModule.Click += (sender, args) => Execute(() =>
            {
                using (FlowInputForm dialog = new FlowInputForm(FlowText.T("moduleName")))
                    if (dialog.ShowDialog(this) == DialogResult.OK) { FlowPrograms.AddModule(dialog.InputValue); AddFlowChart(); RefreshSelection(); }
            });
            add.Click += (sender, args) => Execute(() => { Current().AddNode(); RefreshSelection(); });
            delete.Click += (sender, args) => Execute(DeleteSelected);
            save.Click += (sender, args) => Execute(() => { FlowPrograms.Save(); status.Text = string.Format(FlowText.T("saved"), FlowPrograms.FilePath); });
            reload.Click += (sender, args) => Execute(() => { if (MessageBox.Show(FlowText.T("discard"), FlowText.T("title"), MessageBoxButtons.OKCancel) == DialogResult.OK) { FlowPrograms.Reload(); RefreshSelection(); } });
            validate.Click += (sender, args) => Execute(() => { Current().Validate(null); status.Text = FlowText.T("valid"); });
            test.Click += (sender, args) => Execute(() => RunEntry(false));
            initialize.Click += (sender, args) => Execute(() => RunEntry(true));
            stop.Click += (sender, args) => { if (tabControl1.SelectedTab != null) Current().Runtime.Stop(); };
            pause.Click += (sender, args) => { if (tabControl1.SelectedTab != null) Current().TogglePause(); };
            schema.Click += (sender, args) => Execute(() => { FlowPrograms.RequireIdle(); using (FlowSchemaForm dialog = new FlowSchemaForm(MiddleLayer.DataF)) dialog.ShowDialog(this); });
            timer.Tick += RefreshRuntime;
        }
        /// <summary>Host every Flow_BaseForm subclass without a station-name allowlist.</summary>
        public void AddFlowChart()
        {
            FlowPrograms.Install();
            foreach (Flow_BaseForm form in Flow_Module.FlowChart_ModuleList.ToArray())
            {
                if (form.IsDisposed || pages.ContainsKey(form)) continue;
                FlowModuleEditor editor = FlowPrograms.Attach(form);
                TabPage page = new TabPage(form.Name) { Name = "flowModule_" + pages.Count + "_dyn", Tag = editor, AutoScroll = true };
                form.TopLevel = false; form.FormBorderStyle = FormBorderStyle.None; form.Dock = DockStyle.Fill; form.AutoScroll = true;
                page.Controls.Add(form); tabControl1.TabPages.Add(page); pages.Add(form, page);
                editor.Changed += (sender, args) => RefreshSelection();
                form.Show();
            }
        }
        /// <summary>Resolve the active module controller.</summary>
        private FlowModuleEditor Current()
        {
            if (tabControl1.SelectedTab == null) throw new InvalidOperationException("No Flow_BaseForm instance is registered.");
            return (FlowModuleEditor)tabControl1.SelectedTab.Tag;
        }
        /// <summary>Refresh node and entry pickers while keeping business identifiers stable.</summary>
        private void RefreshSelection()
        {
            if (tabControl1.SelectedTab == null) return;
            filling = true;
            FlowModuleEditor module = Current();
            modeValue.DisplayMember = "Label"; modeValue.ValueMember = "Id";
            modeValue.DataSource = new[] { "Legacy", "Program", "Disabled" }.Select(id => new FlowChoice { Id = id, Label = FlowText.T(id) }).ToList();
            modeValue.SelectedValue = module.Program.Mode;
            foreach (ComboBox combo in new[] { initialValue, autoValue, selectedValue })
            {
                combo.DisplayMember = "Label"; combo.ValueMember = "Id";
                combo.DataSource = new[] { new FlowChoice { Id = "", Label = FlowText.T("end") } }.Concat(module.Program.Steps.Select(step => new FlowChoice { Id = step.Id, Label = step.Caption })).ToList();
            }
            initialValue.SelectedValue = module.Program.InitialEntry ?? ""; autoValue.SelectedValue = module.Program.AutoEntry ?? "";
            initialValue.AccessibleName = FlowText.T("initialEntry"); autoValue.AccessibleName = FlowText.T("autoEntry"); selectedValue.AccessibleName = FlowText.T("delete");
            edit.Checked = module.IsEditing;
            filling = false;
        }
        /// <summary>Update the engineering draft; production uses only the saved definition.</summary>
        private void UpdateDefinition(object sender, EventArgs args)
        {
            if (filling || tabControl1.SelectedTab == null) return;
            Execute(() => { FlowPrograms.RequireIdle(); Current().Program.Mode = Convert.ToString(modeValue.SelectedValue); Current().Program.InitialEntry = Convert.ToString(initialValue.SelectedValue); Current().Program.AutoEntry = Convert.ToString(autoValue.SelectedValue); });
        }
        /// <summary>Enable dragging and configuration for the active module.</summary>
        private void ToggleEditing(object sender, EventArgs args)
        {
            if (filling || tabControl1.SelectedTab == null) return;
            Execute(() => Current().SetEditing(edit.Checked));
            edit.Checked = Current().IsEditing;
        }
        /// <summary>Delete the selected node and its connections after an explicit choice.</summary>
        private void DeleteSelected()
        {
            string id = Convert.ToString(selectedValue.SelectedValue);
            if (string.IsNullOrEmpty(id)) return;
            if (MessageBox.Show(FlowText.T("remove"), FlowText.T("title"), MessageBoxButtons.OKCancel) != DialogResult.OK) return;
            Current().Delete(id); RefreshSelection();
        }
        /// <summary>Run the active draft entry with the visible simulation setting.</summary>
        private void RunEntry(bool initializing)
        {
            FlowPrograms.RequireIdle();
            string entry = initializing ? Current().Program.InitialEntry : Current().Program.AutoEntry;
            if (string.IsNullOrEmpty(entry)) throw new InvalidOperationException("Select an entry node first.");
            FlowPrograms.BeginBatch(); Current().Start(entry, simulation.Checked, initializing);
            filling = true; edit.Checked = false; filling = false;
        }
        /// <summary>Refresh availability and completion errors without rebinding editable rows.</summary>
        private void RefreshRuntime(object sender, EventArgs args)
        {
            if (tabControl1.SelectedTab == null) return;
            bool busy = FlowPrograms.AnyRunning;
            foreach (Control control in new Control[] { addModule, edit, modeValue, initialValue, autoValue, selectedValue, add, delete, save, reload, validate, test, initialize, schema, simulation }) control.Enabled = !busy;
            pause.Enabled = Current().Runtime.IsRunning; stop.Enabled = Current().Runtime.IsRunning;
            if (Current().Runtime.Error != null) status.Text = string.Format(FlowText.T("error"), Current().Runtime.Error);
        }
        /// <summary>Translate labels without changing names or selected data references.</summary>
        private void ApplyLanguage(object sender, EventArgs args) { FlowLanguageForm.ApplyTexts(this); }
        /// <summary>Keep the draft available when an engineering operation fails.</summary>
        private void Execute(Action action)
        {
            try { action(); }
            catch (Exception error) { status.Text = string.Format(FlowText.T("error"), error.Message); MessageBox.Show(status.Text, FlowText.T("title")); }
        }
    }
}
