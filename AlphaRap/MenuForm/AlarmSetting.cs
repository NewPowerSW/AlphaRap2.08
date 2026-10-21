using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace AlphaRap.MenuForm
{
    public partial class AlarmSetting : Form
    {
        private DataTable dtTable;

        /// <summary>初始化报警编辑页，保留三语内容列及稳定绑定字段。</summary>
        public AlarmSetting()
        {
            InitializeComponent();
            UiTheme.ApplyDpi(this);
            if (!UiTheme.IsDesignTime) MiddleLayer.RegisterAndApplyLanguage(this, Name);
        }

        /// <summary>设置页面初始编辑状态。</summary>
        private void AlarmSetting_Load(object sender, EventArgs e) { SetEditing(false); }

        /// <summary>读取配置目录下的三语报警表。</summary>
        private void AlarmSetting_Shown(object sender, EventArgs e)
        {
            try
            {
                dtTable = AlarmCatalog.Load();
                bindingSource1.DataSource = dtTable;
                dgvData.DataSource = bindingSource1;
                MiddleLayer.RegisterAndApplyLanguage(dgvData, Name);
                SetEditing(false);
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        /// <summary>添加空白报警行并允许编辑。</summary>
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (dtTable == null) return;
            dtTable.Rows.Add(dtTable.Rows.Count + 1, "", "True", "E", "", "", "");
            SetEditing(true);
            if (dgvData.Rows.Count > 0) dgvData.FirstDisplayedScrollingRowIndex = dgvData.Rows.Count - 1;
        }

        /// <summary>允许编辑报警属性和三语内容。</summary>
        private void btnUpdate_Click(object sender, EventArgs e) { SetEditing(true); }

        /// <summary>确认后删除当前报警行。</summary>
        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (dgvData.CurrentRow == null || dgvData.CurrentRow.IsNewRow) return;
            string message = MiddleLayer.LangMsg(Name, "msg_DeleteConfirm", "确认删除当前报警行吗？", "Delete the selected alarm?", "¿Eliminar la alarma seleccionada?");
            if (MessageBox.Show(message, Text, MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            dgvData.Rows.Remove(dgvData.CurrentRow);
            dtTable.AcceptChanges();
            SetEditing(true);
        }

        /// <summary>验证稳定字段和编号唯一性后保存全部报警译文。</summary>
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (dtTable == null || !dgvData.EndEdit()) return;
            bindingSource1.EndEdit();
            string error = ValidateRows();
            if (error != null) { ShowError(error); return; }
            string message = MiddleLayer.LangMsg(Name, "msg_SaveConfirm", "保存报警表吗？", "Save the alarm table?", "¿Guardar la tabla de alarmas?");
            if (MessageBox.Show(message, Text, MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            try
            {
                AlarmCatalog.Save(dtTable);
                OperationLog.Write("AlarmSetting", "保存报警表：" + dtTable.Rows.Count);
                SetEditing(false);
            }
            catch (Exception ex) { ShowError(ex.Message); }
        }

        /// <summary>校验所有行，错误提示包含行号；校验使用字段键，不依赖翻译后的表头。</summary>
        private string ValidateRows()
        {
            HashSet<string> keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (DataRow row in dtTable.Rows)
            {
                string id = Convert.ToString(row["AlarID"]).Trim();
                string reason = ValidateField("AlarID", id) ?? ValidateField("Type", Convert.ToString(row["Type"]))
                    ?? ValidateField("DoStop", Convert.ToString(row["DoStop"]));
                if (!keys.Add(id)) reason = MiddleLayer.LangMsg(Name, "msg_Duplicate", "报警编号重复", "Duplicate alarm ID", "ID de alarma duplicado");
                if (reason != null) return string.Format(MiddleLayer.LangMsg(Name, "msg_RowError", "第 {0} 行：{1}", "Row {0}: {1}", "Fila {0}: {1}"), dtTable.Rows.IndexOf(row) + 1, reason);
            }
            return null;
        }

        /// <summary>校验报警编号、类型和停机标志，返回可显示的三语错误。</summary>
        private string ValidateField(string field, string value)
        {
            value = value.Trim();
            if (field == "AlarID")
            {
                try { System.Xml.XmlConvert.VerifyNCName(value); }
                catch (Exception) { return MiddleLayer.LangMsg(Name, "msg_InvalidID", "报警编号必须是有效的 XML 名称，例如 A4013", "Alarm ID must be a valid XML name, e.g. A4013", "El ID de alarma debe ser un nombre XML válido, p. ej. A4013"); }
            }
            if (field == "Type" && !new[] { "E", "I", "Q", "W", "K" }.Contains(value.ToUpperInvariant()))
                return MiddleLayer.LangMsg(Name, "msg_InvalidType", "类型必须为 E、I、Q、W 或 K", "Type must be E, I, Q, W or K", "El tipo debe ser E, I, Q, W o K");
            bool stop;
            if (field == "DoStop" && !bool.TryParse(value, out stop))
                return MiddleLayer.LangMsg(Name, "msg_InvalidStop", "停机标志必须为 True 或 False", "Stop flag must be True or False", "El indicador de parada debe ser True o False");
            return null;
        }

        /// <summary>验证正在编辑的字段，不改变绑定键和用户输入。</summary>
        private void dgvData_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || dgvData.Columns[e.ColumnIndex].ReadOnly) return;
            string error = ValidateField(dgvData.Columns[e.ColumnIndex].DataPropertyName, Convert.ToString(e.FormattedValue));
            dgvData.Rows[e.RowIndex].ErrorText = error ?? "";
            e.Cancel = error != null;
        }

        /// <summary>取消编辑并重新读取已保存的报警表。</summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            dgvData.CancelEdit();
            bindingSource1.CancelEdit();
            AlarmSetting_Shown(sender, e);
        }

        /// <summary>刷新三语报警表。</summary>
        private void btnRefresh_Click(object sender, EventArgs e) { AlarmSetting_Shown(sender, e); }

        /// <summary>按列控制编辑状态，序号始终只读。</summary>
        private void SetEditing(bool editing)
        {
            foreach (DataGridViewColumn column in dgvData.Columns) column.ReadOnly = !editing || column.Index == 0;
            btnAdd.Enabled = true;
            btnUpdate.Enabled = !editing;
            btnRemove.Enabled = true;
            btnSave.Enabled = editing;
            btnCancel.Enabled = editing;
            btnRefresh.Enabled = !editing;
        }

        /// <summary>显示报警表操作失败的原因。</summary>
        private void ShowError(string error)
        {
            MessageBox.Show(error, MiddleLayer.LangMsg(Name, "msg_ErrorTitle", "报警表错误", "Alarm table error", "Error en la tabla de alarmas"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
