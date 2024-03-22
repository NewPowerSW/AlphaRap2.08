using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using AlphaRap.Classes;
using AlphaRapLibrary;

namespace AlphaRap
{
    public partial class ProductManagerForm : ModuleBaseForm
    {



        public List<string> RECIPEIDList = new List<string>();

        public ProductManagerForm()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
            ReadAllProductData();

            string[] Sp2 = new string[1];
            Sp2[0] = ".xml";

            string[] a = SysPara.FilePath.Split('\\');

            string[] b = a[a.Length - 1].Split(Sp2, StringSplitOptions.RemoveEmptyEntries);
            CurrentModel.Text = b[0];
        }
        /// <summary>
        /// 从指定文件的路径读取数据
        /// </summary>
        public void ReadAllProductData()
        {

            RECIPEIDList.Clear();

            string path = System.Windows.Forms.Application.StartupPath+"\\"+ SysPara.RecipeDataDirectory.Substring(1);
            DirectoryInfo root = new DirectoryInfo(path);
            listView1.BeginUpdate();
            listView1.Items.Clear();
            foreach (FileInfo f in root.GetFiles())
            {
                string[] Sp2 = new string[1];
                Sp2[0] = ".xml";

                string name = f.Name;
                string[] name1 = name.Split(Sp2, StringSplitOptions.RemoveEmptyEntries);
                ListViewItem lvi = new ListViewItem(name1[0]);
                listView1.Items.Add(lvi);


                string XMLString = File.ReadAllText(f.FullName, Encoding.GetEncoding("GB2312"));

                string[] Sp = new string[2];
                Sp[0] = "<RECIPEID>";
                Sp[1] = "</RECIPEID>";
                string[] dataARRY = XMLString.Split(Sp, StringSplitOptions.RemoveEmptyEntries);

                RECIPEIDList.Add(dataARRY[1]);

            }

            listView1.EndUpdate();
        }
        /// <summary>
        /// 创建物料信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btCreate_Click(object sender, EventArgs e)
        {

            if (SysPara.SystemMode != RunMode.IDLE)
            {

                MessageBox.Show("Machine is running, can't create model!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                return;

            }

           
            string Directory = SysPara.RecipeDataDirectory.Replace(".\\", System.IO.Directory.GetCurrentDirectory() + "\\");
            if (txtProduct.Text != "")
            {
                string filePath = Directory + "\\" + txtProduct.Text + ".xml";

                string filePath1 = Directory + "\\" + CurrentModel.Text + ".xml";
                string filePath2 = Directory + "\\" + txtProduct.Text + ".xml";
                System.IO.File.Copy(filePath1, filePath2, true);

                for (int i = 0; i < VisionproInterface.VList.Count; i++)
                {

                    string BBB = string.Format(@"{0}\{1}\{2}.vpp", SysPara.VisionFileDirectory, VisionproInterface.VList[i].GetType().Name, "AllModel");
                    string CCC = string.Format(@"{0}\{1}\{2}.vpp", SysPara.VisionFileDirectory, VisionproInterface.VList[i].GetType().Name, txtProduct.Text);

                    //System.IO.File.Copy(BBB, CCC, true);
                }


                //string filePath1 = System.IO.Directory.GetCurrentDirectory() + "\\ModuleData\\RobotData\\" + CurrentModel.Text + ".xml";
                //string filePath2 = System.IO.Directory.GetCurrentDirectory() + "\\ModuleData\\RobotData\\" + txtProduct.Text + ".xml";
                //System.IO.File.Copy(filePath1, filePath2, true);
            }

            ReadAllProductData();
            txtProduct.Text = "";
            SysPara.items++;
            MiddleLayer.MainF.SaveData();
        }
        /// <summary>
        /// 删除当前选中的物料
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btDelete_Click(object sender, EventArgs e)
        {
            if (this.listView1.SelectedIndices.Count > 0)
            {

                if (SysPara.SystemMode != RunMode.IDLE)
                {

                    MessageBox.Show("Machine is running, can't delete!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                    return;

                }


                if (listView1.SelectedItems[0].Text == CurrentModel.Text)
                {

                    MessageBox.Show("This model is using, can't delete!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                    return;

                }
                if (listView1.SelectedItems[0].Text == "AllModel")
                {

                    MessageBox.Show("AllModel can't delete!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                    return;

                }
               


                DialogResult result = MessageBox.Show("Are you sure to delete " + listView1.SelectedItems[0].Text + " model?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                if (result == System.Windows.Forms.DialogResult.Cancel)
                {

                    return;

                }

                string filePath = SysPara.RecipeDataDirectory + "\\" + listView1.SelectedItems[0].Text + ".xml";
                File.Delete(filePath);

                for (int i = 0; i < VisionproInterface.VList.Count; i++)
                {


                    string VisionfilePath = string.Format(@"{0}\{1}\{2}.vpp", SysPara.VisionFileDirectory, VisionproInterface.VList[i].GetType().Name, listView1.SelectedItems[0].Text);

                    File.Delete(VisionfilePath);
                }

                //string filePath3 = System.IO.Directory.GetCurrentDirectory() + "\\ModuleData\\RobotData\\" + listView1.SelectedItems[0].Text + ".xml";
                // File.Delete(filePath3);

                ReadAllProductData();
                //if (listView1.Items.Count > 0)
                //{
                //    string data = listView1.Items[0].Text;
                //    string filePath1 = SysPara.RecipeDataDirectory + "\\" + data + ".xml";
                //    MiddleLayer.OpenRecipe(filePath1);
                //    string[] a = filePath1.Split('\\');
                //    string[] b = a[a.Length - 1].Split('.');
                //    CurrentModel.Text = b[0];
                //}
                //else
                //{
                //    CurrentModel.Text = "";
                //    //string filePath2 = SysPara.RecipeDataDirectory + "\\" + "" + ".xml";
                //    string filePath2 = SysPara.RecipeDataDirectory + "\\" + "";
                //    SysPara.RecipeName = Path.GetFileNameWithoutExtension(filePath2);
                //    SysPara.FilePath = SysPara.RecipeDataDirectory + "\\" + SysPara.RecipeName + ".xml";
                //    IniFile IniFile = new IniFile(".\\MachineSetup.ini");
                //    IniFile.WriteString("MachineSetup", "RecipeName", SysPara.RecipeName);
                //    IniFile.WriteString("PathSetup", "RecipeDirectory", SysPara.RecipeDataDirectory.Replace(System.IO.Directory.GetCurrentDirectory() + "\\", ".\\"));

                //    //MiddleLayer.HardF.dgv_H1_SolderPost.DataSource = null;
                //    // MiddleLayer.HardF.dgv_H1_SolderPost.Enabled = false;   
                //}
            }
        }
        /// <summary>
        /// 使用当前选中的物料
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btUse_Click(object sender, EventArgs e)
        {


            if (listView1.SelectedIndices.Count > 0)
            {

                if (SysPara.SystemMode != RunMode.IDLE)
                {

                    MessageBox.Show("Machine is running, can't change model!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                    return;

                }


                string[] Sp2 = new string[1];
                Sp2[0] = ".xml";

                
                string filePath = SysPara.RecipeDataDirectory + "\\" + listView1.SelectedItems[0].Text + ".xml";
                MiddleLayer.OpenRecipe(filePath);
                string[] a = filePath.Split('\\');
                string[] b = a[a.Length - 1].Split(Sp2, StringSplitOptions.RemoveEmptyEntries);
                CurrentModel.Text = b[0];
                MiddleLayer.OpenRecipe(SysPara.FilePath);
                MiddleLayer.show();
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            SysPara.items++;
        }

        private void ProductManagerForm_Load(object sender, EventArgs e)
        {
            CurrentModel.Text = SysPara.RecipeName;
        }
    }
}
