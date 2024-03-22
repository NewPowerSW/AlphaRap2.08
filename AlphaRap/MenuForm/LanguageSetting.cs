using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace AlphaRap.MenuForm
{
    public partial class LanguageSetting : Form
    {
        DataTable dtTable = new DataTable();
        public LanguageSetting()
        {
            InitializeComponent();
            if (dtTable.Columns.Count == 0)
            {
                dtTable.Columns.Add("ID");
                dtTable.Columns.Add("Form");
                dtTable.Columns.Add("Object");
                dtTable.Columns.Add("English");
                dtTable.Columns.Add("Spanish");
                dtTable.Columns.Add("Chinese");
            }
        }

        public void LoadTables()
        {

                dtTable.Rows.Clear();
            
            XElement rootElementEnglish = XElement.Load(Application.StartupPath + "\\LanguageData\\English.xml");
            XElement rootElementSpanish = XElement.Load(Application.StartupPath + "\\LanguageData\\Español.xml");
            XElement rootElementChinese = XElement.Load(Application.StartupPath + "\\LanguageData\\Chinese.xml");

            rootElementEnglish.Nodes();
            rootElementSpanish.Nodes();
            rootElementChinese.Nodes();

            IEnumerable<XNode> nodes =
                from nd in rootElementEnglish.Nodes()
                select nd;
            
            foreach (XElement item in nodes)
            {
                IEnumerable<XElement> elementsEnglish
               = from elemento in rootElementEnglish.Element(item.Name)
                   .Elements()
                 select elemento;


                foreach (XElement element in elementsEnglish)
                {
                    string Form = item.Name.ToString();
                    string English = (string)element.Attribute("ComponentText");
                    string Object = element.Name.ToString();

                    DataRow dtNewRow = this.dtTable.NewRow();
                    dtNewRow["ID"] = this.dtTable.Rows.Count + 1;
                    dtNewRow["Form"] = Form;
                    dtNewRow["Object"] = Object;
                    dtNewRow["English"] = English;
                    dtNewRow["Spanish"] = "";
                    dtNewRow["Chinese"] = "";
                    dtTable.Rows.Add(dtNewRow);

                }

            }

            IEnumerable<XNode> nodesSpanish =
                from nd in rootElementSpanish.Nodes()
                select nd;

            foreach (XElement item in nodesSpanish)
            {
                IEnumerable<XElement> elementsSpanish
               = from elemento in rootElementSpanish.Element(item.Name)
                   .Elements()
                 select elemento;


                foreach (XElement element in elementsSpanish)
                {
                    string Form = item.Name.ToString();
                    string Spanish = (string)element.Attribute("ComponentText");
                    string Object = element.Name.ToString();
                    
                    DataRow[] dtSelect = (from d in this.dtTable.AsEnumerable() where d.Field<string>("Object") == Object.Trim() && d.Field<string>("Form") == Form.Trim() select d).ToArray();
                    if (dtSelect.Length > 0)
                    {
                        dtSelect[0]["Spanish"] = Spanish;
                        dtSelect[0].AcceptChanges();
                    }
                    else
                    {
                        DataRow dtNewRow = this.dtTable.NewRow();
                        dtNewRow["ID"] = this.dtTable.Rows.Count + 1;
                        dtNewRow["Form"] = Form;
                        dtNewRow["Object"] = Object;
                        dtNewRow["English"] = "";
                        dtNewRow["Spanish"] = Spanish;
                        dtNewRow["Chinese"] = "";
                        dtTable.Rows.Add(dtNewRow);
                    }

                }

            }

            IEnumerable<XNode> nodesChinese =
                from nd in rootElementChinese.Nodes()
                select nd;

            foreach (XElement item in nodesChinese)
            {
                IEnumerable<XElement> elementsChinese
               = from elemento in rootElementChinese.Element(item.Name)
                   .Elements()
                 select elemento;


                foreach (XElement element in elementsChinese)
                {
                    string Form = item.Name.ToString();
                    string Chinese = (string)element.Attribute("ComponentText");
                    string Object = element.Name.ToString();

                    DataRow[] dtSelect = (from d in this.dtTable.AsEnumerable() where d.Field<string>("Object") == Object.Trim() && d.Field<string>("Form") == Form.Trim() select d).ToArray();
                    if (dtSelect.Length > 0)
                    {
                        dtSelect[0]["Chinese"] = Chinese;
                        dtSelect[0].AcceptChanges();
                    }
                    else
                    {
                        DataRow dtNewRow = this.dtTable.NewRow();
                        dtNewRow["ID"] = this.dtTable.Rows.Count + 1;
                        dtNewRow["Form"] = Form;
                        dtNewRow["Object"] = Object;
                        dtNewRow["English"] = "";
                        dtNewRow["Spanish"] = "";
                        dtNewRow["Chinese"] = Chinese;
                        dtTable.Rows.Add(dtNewRow);
                    }

                }

            }


            this.bindingSource1.DataSource = this.dtTable;
            this.dataGridView1.DataSource = this.bindingSource1;
            foreach (DataGridViewRow forRow in this.dataGridView1.Rows)
                foreach (DataGridViewCell forCell in forRow.Cells)
                    forCell.ReadOnly = true;
        }

        private void LanguageSetting_Load(object sender, EventArgs e)
        {
            LoadTables();
            
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            #region MyRegion
            foreach (DataGridViewRow forRow in this.dataGridView1.Rows)
                foreach (DataGridViewCell forCell in forRow.Cells)
                    if (forCell.ColumnIndex != 0 && forCell.ColumnIndex != 1 && forCell.ColumnIndex != 2)
                        forCell.ReadOnly = false;
            this.btnUpdate.Enabled = false;
            this.btnSave.Enabled = true;
            this.btnCancel.Enabled = true;
            this.btnRefresh.Enabled = false;
            #endregion
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string message1 = "";
            string message2 = "";
            if (SysPara.LanguageShow == LanguageType.Chinese)
            {
                message1 = "是否要保存？";
                message2 = "温馨提示";
            }
            else if (SysPara.LanguageShow == LanguageType.English)
            {
                message1 = "Are you sure to save it?";
                message2 = "Save";
            }
            else if (SysPara.LanguageShow == LanguageType.Español)
            {
                message1 = "Seguro que quieres guardar?";
                message2 = "Guardar";
            }
            if (MessageBox.Show(message1, message2, MessageBoxButtons.YesNo) == DialogResult.No)
                return;


            int index = 0;
            dataGridView1.EndEdit();
            XElement rootElementEnglish = XElement.Load(Application.StartupPath + "\\LanguageData\\English.xml");
            XElement rootElementSpanish = XElement.Load(Application.StartupPath + "\\LanguageData\\Español.xml");
            XElement rootElementChinese = XElement.Load(Application.StartupPath + "\\LanguageData\\Chinese.xml");

            rootElementEnglish.Nodes();
            rootElementSpanish.Nodes();
            rootElementChinese.Nodes();

            IEnumerable<XNode> nodesEn =
                from nd in rootElementEnglish.Nodes()
                select nd;

            foreach (XElement item in nodesEn)
            {
                IEnumerable<XElement> elementsEnglish
               = from elemento in rootElementEnglish.Element(item.Name)
                   .Elements()
                 select elemento;


                foreach (XElement element in elementsEnglish)
                {
                    string dgvalue = (string)dataGridView1.Rows[index].Cells[2].Value;
                    string dgvalue2 = (string)dataGridView1.Rows[index].Cells[1].Value;
                    if (dgvalue == element.Name.LocalName && dgvalue2 == item.Name)
                    {
                        element.ReplaceAttributes(new XAttribute("ComponentText", dataGridView1.Rows[index].Cells[3].Value.ToString()));
                        index++;
                    }   
                }

            }
            rootElementEnglish.Save(Application.StartupPath + "\\LanguageData\\English.xml");

            index = 0;
            IEnumerable<XNode> nodesES =
                from nd in rootElementSpanish.Nodes()
                select nd;

            foreach (XElement item in nodesES)
            {
                IEnumerable<XElement> elementsSpanish
               = from elemento in rootElementSpanish.Element(item.Name)
                   .Elements()
                 select elemento;


                foreach (XElement element in elementsSpanish)
                {
                    string dgvalue = (string)dataGridView1.Rows[index].Cells[2].Value;
                    string dgvalue2 = (string)dataGridView1.Rows[index].Cells[1].Value;
                    if (dgvalue == element.Name.LocalName && dgvalue2==item.Name)
                    {
                        element.ReplaceAttributes(new XAttribute("ComponentText", dataGridView1.Rows[index].Cells[4].Value.ToString()));
                        index++;
                    }

                }

            }
            rootElementSpanish.Save(Application.StartupPath + "\\LanguageData\\Español.xml");

            index = 0;
            IEnumerable<XNode> nodesCH =
                from nd in rootElementChinese.Nodes()
                select nd;

            foreach (XElement item in nodesCH)
            {
                IEnumerable<XElement> elementsChinese
               = from elemento in rootElementChinese.Element(item.Name)
                   .Elements()
                 select elemento;

                foreach (XElement element in elementsChinese)
                {
                    string dgvalue = (string)dataGridView1.Rows[index].Cells[2].Value;
                    string dgvalue2 = (string)dataGridView1.Rows[index].Cells[1].Value;
                    if (dgvalue == element.Name.LocalName && dgvalue2 == item.Name)
                    {
                        element.ReplaceAttributes(new XAttribute("ComponentText", dataGridView1.Rows[index].Cells[5].Value.ToString()));
                        index++;
                    }
                }

            }
            rootElementChinese.Save(Application.StartupPath + "\\LanguageData\\Chinese.xml");


            this.btnUpdate.Enabled = true;
            this.btnSave.Enabled = false;
            this.btnCancel.Enabled = false;
            this.btnRefresh.Enabled = true;
            foreach (DataGridViewRow forRow in this.dataGridView1.Rows)
                foreach (DataGridViewCell forCell in forRow.Cells)
                    forCell.ReadOnly = true;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadTables();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.btnUpdate.Enabled = true;
            this.btnSave.Enabled = false;
            this.btnCancel.Enabled = false;
            this.btnRefresh.Enabled = true;
            //foreach (DataGridViewRow forRow in this.dgvData.Rows)
            //{
            //    forRow.ReadOnly = true;
            //}
            LoadTables();
        }
    }
}
