using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace AlphaRap.MenuForm
{
    public partial class AlarmSetting : Form
    {
        DataTable dtTable = new DataTable();
		public List<string> mlist;

		public AlarmSetting()
        {
            InitializeComponent();
            // 本窗体是 SystemForm 里按需 new 出来的，不在 lstForm 里，赶不上启动扫描 ——
            // 就地补登记进语言表（此时语言表已建好），Designer 里的文案才能跟随切语言。
            MiddleLayer.RegisterAndApplyLanguage(this, this.Name);
            if (dtTable.Columns.Count == 0)
            {
                dtTable.Columns.Add("Index");
                dtTable.Columns.Add("AlarID");
                dtTable.Columns.Add("DoStop");
                dtTable.Columns.Add("Type");
                dtTable.Columns.Add("Content");
                dtTable.Columns.Add("EContent");
                dtTable.Columns.Add("SPContent");
            }
        }
        private void AlarmSetting_Load(object sender, EventArgs e)
        {
            #region MyRegion
            this.btnAdd.Enabled = true;
            this.btnUpdate.Enabled = true;
            this.btnRemove.Enabled = true;
            this.btnSave.Enabled = false;
            this.btnCancel.Enabled = false;
            #endregion
        }
        private void AlarmSetting_Shown(object sender, EventArgs e)
        {
            #region MyRegion
            this.dtTable.Rows.Clear();
            AlphaRap.srvConfigReadWriteXML srv = new AlphaRap.srvConfigReadWriteXML();
            System.Xml.XmlDocument _xmlDoc = srv.XmlDocumentLoad(System.Windows.Forms.Application.StartupPath + "\\AlarmTable\\Chinese.xml");
            string strInnerXml = _xmlDoc.FirstChild.InnerXml;
            if (strInnerXml.Trim().Length > 0)
            {
                List<string> mlist = strInnerXml.Trim().Replace("/><", "/>\n<").Split('\n').ToList();
                foreach (string forRor in mlist)
                {
                    string strValue = forRor.Substring(1, forRor.IndexOf(' '));
                    if (strValue.Trim().Length > 0)
                    {
                        string strIndexOf = "DoStop=\"";
                        int iIndex = forRor.IndexOf(strIndexOf) + strIndexOf.Trim().Length;
                        string strDoStop = forRor.Substring(iIndex, forRor.Trim().Length - iIndex);
                        strDoStop = strDoStop.Substring(0, strDoStop.IndexOf('\"'));

                        strIndexOf = "Content=\"";
                        iIndex = forRor.IndexOf(strIndexOf) + strIndexOf.Trim().Length;
                        string strContent = forRor.Substring(iIndex, forRor.Trim().Length - iIndex);
                        strContent = strContent.Substring(0, strContent.IndexOf('\"'));

                        strIndexOf = "Type=\"";
                        iIndex = forRor.IndexOf(strIndexOf) + strIndexOf.Trim().Length;
                        string strType = forRor.Substring(iIndex, forRor.Trim().Length - iIndex);
                        strType = strType.Substring(0, strType.IndexOf('\"'));

                        DataRow dtNewRow = this.dtTable.NewRow();
                        dtNewRow["Index"] = this.dtTable.Rows.Count + 1;
                        dtNewRow["AlarID"] = strValue.Trim();
                        dtNewRow["DoStop"] = strDoStop;
                        dtNewRow["Type"] = strType;
                        dtNewRow["Content"] = strContent;
                        dtNewRow["EContent"] = "";
                        dtNewRow["SPContent"] = "";
                        dtTable.Rows.Add(dtNewRow);
                    }
                }
            }
            _xmlDoc = srv.XmlDocumentLoad(System.Windows.Forms.Application.StartupPath + "\\AlarmTable\\English.xml");
            strInnerXml = _xmlDoc.FirstChild.InnerXml;
            if (strInnerXml.Trim().Length > 0)
            {
               mlist = strInnerXml.Trim().Replace("/><", "/>\n<").Split('\n').ToList();
                foreach (string forRor in mlist)
                {
                    string strValue = forRor.Substring(1, forRor.IndexOf(' '));
                    if (strValue.Trim().Length > 0)
                    {
                        string strIndexOf = "DoStop=\"";
                        int iIndex = forRor.IndexOf(strIndexOf) + strIndexOf.Trim().Length;
                        string strDoStop = forRor.Substring(iIndex, forRor.Trim().Length - iIndex);
                        strDoStop = strDoStop.Substring(0, strDoStop.IndexOf('\"'));

                        strIndexOf = "Content=\"";
                        iIndex = forRor.IndexOf(strIndexOf) + strIndexOf.Trim().Length;
                        string strContent = forRor.Substring(iIndex, forRor.Trim().Length - iIndex);
                        strContent = strContent.Substring(0, strContent.IndexOf('\"'));

                        strIndexOf = "Type=\"";
                        iIndex = forRor.IndexOf(strIndexOf) + strIndexOf.Trim().Length;
                        string strType = forRor.Substring(iIndex, forRor.Trim().Length - iIndex);
                        strType = strType.Substring(0, strType.IndexOf('\"'));

                        DataRow[] dtSelect = (from d in this.dtTable.AsEnumerable() where d.Field<string>("AlarID") == strValue.Trim() select d).ToArray();
                        if (dtSelect.Length > 0)
                        {
                            dtSelect[0]["EContent"] = strContent;
                            dtSelect[0].AcceptChanges();
                        }
                        else
                        {
                            DataRow dtNewRow = this.dtTable.NewRow();
                            dtNewRow["Index"] = this.dtTable.Rows.Count + 1;
                            dtNewRow["AlarID"] = strValue.Trim();
                            dtNewRow["DoStop"] = strDoStop;
                            dtNewRow["Type"] = strType;
                            dtNewRow["Content"] = "";
                            dtNewRow["EContent"] = strContent;
                            dtNewRow["SPContent"] = "";
                            dtTable.Rows.Add(dtNewRow);
                        }
                    }
                }
            }

            _xmlDoc = srv.XmlDocumentLoad(System.Windows.Forms.Application.StartupPath + "\\AlarmTable\\Spanish.xml");
            strInnerXml = _xmlDoc.FirstChild.InnerXml;
            if (strInnerXml.Trim().Length > 0)
            {
                mlist = strInnerXml.Trim().Replace("/><", "/>\n<").Split('\n').ToList();
                foreach (string forRor in mlist)
                {
                    string strValue = forRor.Substring(1, forRor.IndexOf(' '));
                    if (strValue.Trim().Length > 0)
                    {
                        string strIndexOf = "DoStop=\"";
                        int iIndex = forRor.IndexOf(strIndexOf) + strIndexOf.Trim().Length;
                        string strDoStop = forRor.Substring(iIndex, forRor.Trim().Length - iIndex);
                        strDoStop = strDoStop.Substring(0, strDoStop.IndexOf('\"'));

                        strIndexOf = "Content=\"";
                        iIndex = forRor.IndexOf(strIndexOf) + strIndexOf.Trim().Length;
                        string strContent = forRor.Substring(iIndex, forRor.Trim().Length - iIndex);
                        strContent = strContent.Substring(0, strContent.IndexOf('\"'));

                        strIndexOf = "Type=\"";
                        iIndex = forRor.IndexOf(strIndexOf) + strIndexOf.Trim().Length;
                        string strType = forRor.Substring(iIndex, forRor.Trim().Length - iIndex);
                        strType = strType.Substring(0, strType.IndexOf('\"'));

                        DataRow[] dtSelect = (from d in this.dtTable.AsEnumerable() where d.Field<string>("AlarID") == strValue.Trim() select d).ToArray();
                        if (dtSelect.Length > 0)
                        {
                            dtSelect[0]["SPContent"] = strContent;
                            dtSelect[0].AcceptChanges();
                        }
                        else
                        {
                            DataRow dtNewRow = this.dtTable.NewRow();
                            dtNewRow["Index"] = this.dtTable.Rows.Count + 1;
                            dtNewRow["AlarID"] = strValue.Trim();
                            dtNewRow["DoStop"] = strDoStop;
                            dtNewRow["Type"] = strType;
                            dtNewRow["Content"] = "";
                            dtNewRow["EContent"] = "";
                            dtNewRow["SPContent"] = strContent;
                            dtTable.Rows.Add(dtNewRow);
                        }
                    }
                }
            }

            this.bindingSource1.DataSource = this.dtTable;
            this.dgvData.DataSource = this.bindingSource1;
            foreach (DataGridViewRow forRow in this.dgvData.Rows)
                foreach (DataGridViewCell forCell in forRow.Cells)
                    forCell.ReadOnly = true;
            #endregion
        }
        private void btnAdd_Click(object sender, EventArgs e)
        {
            #region MyRegion
            DataRow dtNewRow = this.dtTable.NewRow();
            foreach (DataColumn forColumn in this.dtTable.Columns)
            {
                if (forColumn.ColumnName.Trim() == "Index")
                    dtNewRow[forColumn.ColumnName.Trim()] = this.dtTable.Rows.Count + 1;
                else
                    dtNewRow[forColumn.ColumnName.Trim()] = "";
            }
            this.dtTable.Rows.Add(dtNewRow);
            this.dgvData.FirstDisplayedScrollingRowIndex = this.dgvData.Rows.Count - 1;

            this.dgvData.Rows[this.dgvData.Rows.Count - 1].ReadOnly = false;
            this.btnAdd.Enabled = true;
            this.btnUpdate.Enabled = false;
            this.btnRemove.Enabled = false;
            this.btnSave.Enabled = true;
            this.btnCancel.Enabled = true;
            this.btnRefresh.Enabled = false;
            #endregion
        }
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            #region MyRegion
            foreach (DataGridViewRow forRow in this.dgvData.Rows)
                foreach (DataGridViewCell forCell in forRow.Cells)
                    if (forCell.ColumnIndex != 0)
                        forCell.ReadOnly = false;
            this.btnAdd.Enabled = false;
            this.btnUpdate.Enabled = false;
            this.btnRemove.Enabled = false;
            this.btnSave.Enabled = true;
            this.btnCancel.Enabled = true;
            this.btnRefresh.Enabled = false;
            #endregion
        }
        private void btnRemove_Click(object sender, EventArgs e)
        {
            #region MyRegion
            string message1 = MiddleLayer.LangMsg("AlarmSetting", "msg_DeleteConfirm", "您確認要移除當前選中的行嗎？", "Are you sure to delete?", "Estas seguro que desea eliminar?");
            string message2 = MiddleLayer.LangMsg("AlarmSetting", "msg_DeleteTitle", "溫馨提示", "Delete", "Eliminar");

            if (this.dgvData.Rows.Count > 0 && this.dgvData.Columns.Count > 0)
            {
                if (this.dgvData.CurrentRow.Index >= 0)
                {
                    if (MessageBox.Show(message1, message2, MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        this.dgvData.Rows.RemoveAt(this.dgvData.CurrentRow.Index);
                        this.dtTable.AcceptChanges();

                        this.btnAdd.Enabled = false;
                        this.btnUpdate.Enabled = false;
                        this.btnRemove.Enabled = true;
                        this.btnSave.Enabled = true;
                        this.btnCancel.Enabled = true;
                        this.btnRefresh.Enabled = false;
                    }
                }
            }
            #endregion
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            #region MyRegion
            this.dgvData.EndEdit();
            string message1 = MiddleLayer.LangMsg("AlarmSetting", "msg_SaveConfirm", "是否要保存？", "Are you sure to save it?", "Seguro que quieres guardar?");
            string message2 = MiddleLayer.LangMsg("AlarmSetting", "msg_SaveTitle", "温馨提示", "Save", "Guardar");
            if (MessageBox.Show(message1, message2, MessageBoxButtons.YesNo) == DialogResult.No)
                return;
            foreach (DataRow forRow in this.dtTable.Rows)
            {
                string strIndex = forRow["Index"].ToString().Trim();
                if (forRow["AlarID"].ToString().Trim().Length == 0)
                {
                    MessageBox.Show(string.Format(
                        MiddleLayer.LangMsg("AlarmSetting", "msg_AlarmRowEmpty", "行{0}，[{1}]不能為空！", "Row {0}: [{1}] must not be empty!", "La fila {0}: ¡[{1}] no puede estar vacía!"),
                        strIndex, "AlarID"));
                    return;
                }
                if (forRow["DoStop"].ToString().Trim().Length == 0)
                {
                    MessageBox.Show("行" + strIndex + "，[DoStop]不能為空！");
                    return;
                }
                if (forRow["Type"].ToString().Trim().Length == 0)
                {
                    MessageBox.Show("行" + strIndex + "，[Type]不能為空！");
                    return;
                }
            }
            List<string> mlist = new List<string>();
            List<string> Emlist = new List<string>();
            List<string> Esmlist = new List<string>();
            foreach (DataRow forRow in this.dtTable.Rows)
            {
                mlist.Add("  <" + forRow["AlarID"].ToString().Trim() + " Type = \"" + forRow["Type"].ToString().Trim() + "\" Content = \"" + forRow["Content"].ToString().Trim() + "\" DoStop = \"" + forRow["DoStop"].ToString().Trim() + "\" />");
                Emlist.Add("  <" + forRow["AlarID"].ToString().Trim() + " Type = \"" + forRow["Type"].ToString().Trim() + "\" Content = \"" + forRow["EContent"].ToString().Trim() + "\" DoStop = \"" + forRow["DoStop"].ToString().Trim() + "\" />");
                Esmlist.Add("  <" + forRow["AlarID"].ToString().Trim() + " Type = \"" + forRow["Type"].ToString().Trim() + "\" Content = \"" + forRow["SPContent"].ToString().Trim() + "\" DoStop = \"" + forRow["DoStop"].ToString().Trim() + "\" />");
            }
            string strText = "<AlarmTable>\r\n" + String.Join("\r\n", mlist) + "\r\n</AlarmTable>";
            string strEText = "<AlarmTable>\r\n" + String.Join("\r\n", Emlist) + "\r\n</AlarmTable>";
            string strEsText = "<AlarmTable>\r\n" + String.Join("\r\n", Esmlist) + "\r\n</AlarmTable>";

            //6.获取启动了应用程序的可执行文件的路径  
            string strPath = System.Windows.Forms.Application.StartupPath + "\\AlarmTable";
            if (!System.IO.Directory.Exists(strPath))//如果不存在就创建file文件夹　　             　　                
                System.IO.Directory.CreateDirectory(strPath);

            string strPathName = strPath + @"\Chinese.xml";
            if (System.IO.File.Exists(strPathName))//如果文件存在则删除 
                System.IO.File.Delete(strPathName);
            System.IO.FileStream fs = new System.IO.FileStream(strPathName, System.IO.FileMode.CreateNew);
            System.IO.StreamWriter sw = new System.IO.StreamWriter(fs);
            sw.Write(strText);  //这里是写入的内容
            sw.Flush();
            sw.Dispose();

            string _strPathName = strPath + @"\English.xml";
            if (System.IO.File.Exists(_strPathName))//如果文件存在则删除 
                System.IO.File.Delete(_strPathName);
            System.IO.FileStream _fs = new System.IO.FileStream(_strPathName, System.IO.FileMode.CreateNew);
            System.IO.StreamWriter _sw = new System.IO.StreamWriter(_fs);
            _sw.Write(strEText);  //这里是写入的内容
            _sw.Flush();
            _sw.Dispose();

            string _esstrPathName = strPath + @"\Spanish.xml";
            if (System.IO.File.Exists(_esstrPathName))//如果文件存在则删除 
                System.IO.File.Delete(_esstrPathName);
            System.IO.FileStream _esfs = new System.IO.FileStream(_esstrPathName, System.IO.FileMode.CreateNew);
            System.IO.StreamWriter _essw = new System.IO.StreamWriter(_esfs);
            _essw.Write(strEsText);  //这里是写入的内容
            _essw.Flush();
            _essw.Dispose();

            this.btnAdd.Enabled = true;
            this.btnUpdate.Enabled = true;
            this.btnRemove.Enabled = true;
            this.btnSave.Enabled = false;
            this.btnCancel.Enabled = false;
            this.btnRefresh.Enabled = true;
            #endregion
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            #region MyRegion
            this.btnAdd.Enabled = true;
            this.btnUpdate.Enabled = true;
            this.btnRemove.Enabled = true;
            this.btnSave.Enabled = false;
            this.btnCancel.Enabled = false;
            this.btnRefresh.Enabled = true;
            this.AlarmSetting_Shown(this, null);
            #endregion
        }
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            #region MyRegion
            this.AlarmSetting_Shown(this, null);
            #endregion
        }
        private void dgvData_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            #region MyRegion
            if (e.RowIndex > -1 && e.ColumnIndex > -1)
            {
                DataGridView dgvDataGrid = (DataGridView)sender;
                dgvDataGrid.Rows[e.RowIndex].ErrorText = "";
                switch (dgvDataGrid.Columns[e.ColumnIndex].HeaderText.Trim())
                {
                    case "AlarID":
                        if (e.FormattedValue.ToString().Trim().Length > 0)
                        {
                            if (e.FormattedValue.ToString().Trim().Length > 0)
                            {
                                if (e.FormattedValue.ToString().Trim().IndexOf(' ') != -1)
                                {
                                    e.Cancel = true;
                    string strError = string.Format(
                        MiddleLayer.LangMsg("AlarmSetting", "msg_ValNoSpace", "行{0}，{1} 值不能含有空格", "Row {0}: value of {1} must not contain spaces", "La fila {0}: el valor de {1} no puede contener espacios"),
                        e.RowIndex, dgvDataGrid.Columns[e.ColumnIndex].HeaderText.Trim());
                                    dgvDataGrid.Rows[e.RowIndex].ErrorText = strError;
                                    MessageBox.Show(strError, MiddleLayer.LangMsg("AlarmSetting", "msg_ErrorTitle", "錯誤提示", "Error", "Error"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                    return;
                                }
                            }
                        }
                        break;
                    case "Type":
                        if (e.FormattedValue.ToString().Trim().Length > 0)
                        {
                            if (e.FormattedValue.ToString().Trim().ToUpper() != "E" && e.FormattedValue.ToString().Trim().ToUpper() != "I" && e.FormattedValue.ToString().Trim().ToUpper() != "Q" && e.FormattedValue.ToString().Trim().ToUpper() != "W")
                            {
                                e.Cancel = true;
                                string strError = string.Format(
                                    MiddleLayer.LangMsg("AlarmSetting", "msg_ValOnlyEIQW", "行{0}，{1} 值只能為E、I、Q、W", "Row {0}: value of {1} must be E, I, Q or W", "La fila {0}: el valor de {1} solo puede ser E, I, Q, W"),
                                    e.RowIndex, dgvDataGrid.Columns[e.ColumnIndex].HeaderText.Trim());
                                dgvDataGrid.Rows[e.RowIndex].ErrorText = strError;
                                MessageBox.Show(strError, MiddleLayer.LangMsg("AlarmSetting", "msg_ErrorTitle", "錯誤提示", "Error", "Error"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }
                        }
                        break;
                    case "DoStop":
                        if (e.FormattedValue.ToString().Trim().Length > 0)
                        {
                            if (e.FormattedValue.ToString().Trim().ToUpper() != "FALSE" && e.FormattedValue.ToString().Trim().ToUpper() != "TRUE")
                            {
                                e.Cancel = true;
                                string strError = string.Format(
                                    MiddleLayer.LangMsg("AlarmSetting", "msg_ValTrueFalse", "行{0}，{1} 值只能為True 或False", "Row {0}: value of {1} must be True or False", "La fila {0}: el valor de {1} solo puede ser True o False"),
                                    e.RowIndex, dgvDataGrid.Columns[e.ColumnIndex].HeaderText.Trim());
                                dgvDataGrid.Rows[e.RowIndex].ErrorText = strError;
                                MessageBox.Show(strError, MiddleLayer.LangMsg("AlarmSetting", "msg_ErrorTitle", "錯誤提示", "Error", "Error"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            }
                        }
                        break;
                }
            }
            #endregion
        }
    }
}
