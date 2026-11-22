using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AlphaRap.FlowProgramming;
using AlphaRap.TaskProcess;
using static AlphaRap.TaskProcess.DownConveyorIoBindings;

namespace AlphaRap
{
    public partial class DownConveyorIoForm : Form
    {
        private readonly DownConveyorIoBindings bindings;

        internal static string Title => TextFor("title", "下料 IO 绑定", "Down conveyor IO bindings", "Asignaciones de E/S de descarga");

        private sealed class IoChoice
        {
            public string Id { get; }
            public string Caption { get; set; }
            internal bool Missing { get; set; }

            /// <summary>选择项保存资源 ID，显示名称可独立刷新。</summary>
            internal IoChoice(string id, string caption)
            {
                Id = id;
                Caption = caption;
            }
        }

        /// <summary>提供可在 WinForms 设计器中打开的布局。</summary>
        public DownConveyorIoForm() { InitializeComponent(); }

        /// <summary>使用现有硬件目录和草稿填充工艺信号对应的下拉选项。</summary>
        internal DownConveyorIoForm(DownConveyorIoBindings bindings) : this()
        {
            this.bindings = bindings;
            ApplicationHost.RegisterLanguage(this, Name);
            foreach (string signal in bindings.Signals) AddSignal(signal);
            ApplyLanguage(this, EventArgs.Empty);
            ApplicationHost.LanguageChanged += ApplyLanguage;
            Disposed += (sender, args) => ApplicationHost.LanguageChanged -= ApplyLanguage;
            UiTheme.ApplyDpi(this);
        }

        /// <summary>读取绑定界面与绑定服务共用的三语词条。</summary>
        private static string TextFor(string key, string zh, string en, string es)
        {
            return DownConveyorIoBindings.Text(key, zh, en, es);
        }

        /// <summary>单行只列同方向的 IO，已经删除的项仍可识别并重新选择。</summary>
        private void AddSignal(string signal)
        {
            var choices = new List<IoChoice> { new IoChoice("", FixedPoint) };
            choices.AddRange(bindings.Choices(signal).Select(entry => new IoChoice(entry.Definition.Id, entry.Definition.Name)));
            bindings.Draft.TryGetValue(signal, out string selectedId);
            IoChoice selected = choices.Find(choice => string.Equals(choice.Id, selectedId ?? "", StringComparison.OrdinalIgnoreCase));
            if (selected == null)
            {
                selected = new IoChoice(selectedId, MissingPoint(selectedId)) { Missing = true };
                choices.Add(selected);
            }
            var cell = new DataGridViewComboBoxCell
            {
                DataSource = choices,
                ValueMember = nameof(IoChoice.Id),
                DisplayMember = nameof(IoChoice.Caption),
                DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton,
                FlatStyle = FlatStyle.Flat,
                Value = selected.Id
            };
            int index = grid.Rows.Add();
            DataGridViewRow row = grid.Rows[index];
            row.Tag = signal;
            row.Cells[0].Value = SignalName(signal);
            row.Cells[1] = cell;
        }

        private static string FixedPoint => TextFor("fixed", "沿用固定点", "Use existing fixed IO", "Usar E/S fija existente");

        /// <summary>显示已失效的引用及其 ID。</summary>
        private static string MissingPoint(string id)
        {
            return string.Format(TextFor("missingChoice", "无效引用（{0}）", "Invalid reference ({0})", "Referencia no válida ({0})"), id);
        }

        /// <summary>刷新界面语言和选项标题，保留正在选择及已选中的资源 ID。</summary>
        private void ApplyLanguage(object sender, EventArgs args)
        {
            Text = Title;
            lblHelp_dyn.Text = TextFor("help", "按名称选择 IO。确定后，点主界面的「保存」应用绑定。",
                "Select IO by name. Confirm here, then use Save in the main window.",
                "Seleccione la E/S por nombre. Confirme aquí y use Guardar en la ventana principal.");
            colSignal.HeaderText = TextFor("signal", "下料信号", "Down conveyor signal", "Señal de descarga");
            colResource.HeaderText = TextFor("resource", "绑定 IO", "Assigned IO", "E/S asignada");
            btnOk_dyn.Text = TextFor("ok", "确定", "OK", "Aceptar");
            btnCancel_dyn.Text = TextFor("cancel", "取消", "Cancel", "Cancelar");
            foreach (DataGridViewRow row in grid.Rows)
            {
                string signal = (string)row.Tag;
                row.Cells[0].Value = SignalName(signal);
                var cell = (DataGridViewComboBoxCell)row.Cells[1];
                var choices = (List<IoChoice>)cell.DataSource;
                choices[0].Caption = FixedPoint;
                IoChoice last = choices[choices.Count - 1];
                if (last.Missing) last.Caption = MissingPoint(last.Id);
            }
            grid.Invalidate();
        }

        /// <summary>确定时提交编辑器当前选择，校验通过后更新内存草稿。</summary>
        private void AcceptBindings(object sender, EventArgs args)
        {
            try
            {
                FlowPrograms.RequireIdle();
                if (!grid.EndEdit()) return;
                var choices = new Dictionary<string, string>();
                foreach (DataGridViewRow row in grid.Rows)
                {
                    string id = (string)row.Cells[1].Value;
                    if (!string.IsNullOrEmpty(id)) choices.Add((string)row.Tag, id);
                }
                bindings.Edit(choices);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (InvalidOperationException error)
            {
                MessageBox.Show(this, error.Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
