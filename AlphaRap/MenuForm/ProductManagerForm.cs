using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using AlphaRapLibrary;

namespace AlphaRap
{
    /// <summary>
    /// 物料管理页（嵌入 MainForm 的单例页面，通过 MiddleLayer.ProductF 访问）：
    /// 左侧型号列表（36%），右侧新建型号与型号参数两张卡片（64%），随宿主面板缩放。
    /// "删除 / 使用"按钮按列表选中状态启用，列表底部显示型号数量。
    /// this.Text 用作模块名（定位 ModuleData\SettingData\ProductManagerForm.xml），不做本地化。
    /// </summary>
    public partial class ProductManagerForm : ModuleBaseForm
    {
        public List<string> RECIPEIDList = new List<string>();

        /// <summary>型号条数文案的格式串，由 ApplyLanguage 按当前语言填写。</summary>
        private string _countFormat = "共 {0} 个型号";

        public ProductManagerForm()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();

            StyleListView();
            ApplyIcons();
            ApplyLanguage();
            BindFields();

            // 静态文案由语言表管理（InitialLanguageData 末尾登记），此事件用于刷新型号条数等动态文字
            MiddleLayer.LanguageChanged += ProductManagerForm_LanguageChanged;

            // 选中项变化 → 重新计算"删除 / 使用"是否可点
            listView1.SelectedIndexChanged += listView1_SelectedIndexChanged;

            ReadAllProductData();
            RefreshCurrentModelText();   // 当前配方名

            RefreshActionState();
        }

        // ==================== 外观 ====================

        /// <summary>
        /// ListView 的扁平化设置。
        /// 这些属性放在代码里而不是设计器里，是因为 ListView 自带的 3D 边框 + 网格线
        /// 在白色卡片上非常突兀，集中在这里改一处就能整体调。
        /// </summary>
        private void StyleListView()
        {
            try
            {
                listView1.BorderStyle = BorderStyle.None;
                listView1.BackColor = UiKit.Surface;
                listView1.ForeColor = UiKit.TextPrimary;
                listView1.GridLines = false;
                listView1.Font = UiKit.Regular(10.5f);
                listView1.HeaderStyle = ColumnHeaderStyle.None;
                listView1.FullRowSelect = true;
                listView1.MultiSelect = false;
                listView1.HideSelection = false;

                // 单列铺满卡片宽度，避免出现无意义的横向滚动条；
                // 窗口拉伸时列宽也要跟着走，否则会冒出滚动条。
                listView1.Resize += delegate { FitColumn(); };
                FitColumn();
            }
            catch { }
        }

        /// <summary>让唯一那一列的宽度跟着 ListView 走。</summary>
        private void FitColumn()
        {
            try
            {
                if (listView1 == null || listView1.Columns.Count == 0) return;
                int w = listView1.ClientSize.Width - 6;
                if (w > 80) listView1.Columns[0].Width = w;
            }
            catch { }
        }

        private void ApplyIcons()
        {
            try
            {
                picPageIcon.Image = AppIcons.Get(AppIcon.Product, 34, UiKit.Brand);

                // 参数输入框不加前置图标
                fbNewName.Icon = AppIcons.Get(AppIcon.Edit, 20, UiKit.TextMuted);

                btnCreate.Icon = AppIcons.Get(AppIcon.Plus, 18, Color.White);
                btnDelete.Icon = AppIcons.Get(AppIcon.Trash, 18, Color.White);
                btnUse.Icon = AppIcons.Get(AppIcon.Check, 18, Color.White);
            }
            catch { }
        }

        /// <summary>
        /// 页面每次显示时重新套一次文案。
        /// 必须这么做：本页在启动阶段就被创建（MiddleLayer.InitialProject 里 CreateForm），
        /// 早于语言表扫描；之后用户切换语言时，本页一显示就会被这里纠正过来。
        /// </summary>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible) ApplyLanguage();
        }

        /// <summary>语言切换完成 → 重套本页文案（见构造函数里的说明）。</summary>
        private void ProductManagerForm_LanguageChanged(object sender, EventArgs e)
        {
            ApplyLanguage();
        }

        private void ApplyLanguage()
        {
            try
            {
                // 动态文案：型号条数（"共 N 个型号"）；静态文案见 LanguageData\*.xml 的 ProductManagerForm 段
                _countFormat = MiddleLayer.LangMsg("ProductManagerForm", "msg_ModelCount",
                    "共 {0} 个型号", "{0} model(s)", "{0} modelo(s)");

                // 型号列表的表头（ListView 不进语言表）
                if (listView1 != null && listView1.Columns.Count > 0)
                    listView1.Columns[0].Text = MiddleLayer.LangMsg("ProductManagerForm", "msg_AllModels",
                        "所有产品型号", "All Product Models", "Todos los modelos de productos");

                RefreshModelCount();
                RefreshCurrentModelText();   // 这个标签显示的是数据，被语言表覆盖后要纠正回来
            }
            catch { }
        }

        // ==================== 数据绑定 ====================

        /// <summary>
        /// 把 6 个参数输入框绑定到配方表 ProductSetting（绑定在 FieldBox.Inner 的 Text 属性上，双向同步）。
        /// </summary>
        private void BindFields()
        {
            BindField(fbProductId, "PRODUCTID");
            BindField(fbStepId, "STEPID");
            BindField(fbRecipeId, "RECIPEID");
            BindField(fbPortId, "PORTID");
            BindField(fbSn1, "SN1KeySub");
            BindField(fbSn2, "SN2KeySub");
        }

        private void BindField(FieldBox box, string column)
        {
            if (box == null) return;
            box.Inner.DataBindings.Add("Text", RecipeData, "ProductSetting." + column, true);

            // 内容变化时标记为已修改
            box.ValueChanged += textBox4_TextChanged;
        }

        // ==================== 状态刷新 ====================

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshActionState();
        }

        /// <summary>删除 / 使用两个动作必须先选中一个型号才能点。</summary>
        private void RefreshActionState()
        {
            try
            {
                bool hasSelection = listView1 != null && listView1.SelectedIndices.Count > 0;
                if (btnDelete != null) btnDelete.Enabled = hasSelection;
                if (btnUse != null) btnUse.Enabled = hasSelection;
            }
            catch { }
        }

        private void RefreshModelCount()
        {
            try
            {
                if (lblModelCount == null) return;
                lblModelCount.Text = string.Format(_countFormat, listView1.Items.Count);
            }
            catch { }
        }

        /// <summary>
        /// 按当前配方刷新"当前型号"标签。该标签登记在语言表中，切换语言后由 <see cref="ApplyLanguage"/> 调用以恢复配方名。
        /// </summary>
        private void RefreshCurrentModelText()
        {
            try
            {
                if (CurrentModel == null) return;
                string[] a = SysPara.FilePath.Split('\\');
                string[] b = a[a.Length - 1].Split(new string[] { ".xml" }, StringSplitOptions.RemoveEmptyEntries);
                if (b.Length > 0 && !string.IsNullOrEmpty(b[0])) CurrentModel.Text = b[0];
            }
            catch { }
        }

        /// <summary>
        /// 从指定文件的路径读取数据
        /// </summary>
        public void ReadAllProductData()
        {
            RECIPEIDList.Clear();

            string path = System.Windows.Forms.Application.StartupPath + "\\" + SysPara.RecipeDataDirectory.Substring(1);
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

            RefreshModelCount();
            RefreshActionState();
        }
        /// <summary>
        /// 创建物料信息
        /// </summary>
        private void btCreate_Click(object sender, EventArgs e)
        {
            if (SysPara.SystemMode != RunMode.IDLE)
            {
                MessageBox.Show("Machine is running, can't create model!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                return;
            }

            string Directory = SysPara.RecipeDataDirectory.Replace(".\\", System.IO.Directory.GetCurrentDirectory() + "\\");
            if (fbNewName.Value != "")
            {
                OperationLog.Write("ProductManagerForm", "新建型号：" + fbNewName.Value
                    + "（复制自 " + CurrentModel.Text + "）");
                string filePath = Directory + "\\" + fbNewName.Value + ".xml";

                string filePath1 = Directory + "\\" + CurrentModel.Text + ".xml";
                string filePath2 = Directory + "\\" + fbNewName.Value + ".xml";
                System.IO.File.Copy(filePath1, filePath2, true);

                for (int i = 0; i < VisionproInterface.VList.Count; i++)
                {
                    string BBB = VisionproInterface.VList[i].GetVppPath("AllModel");
                    string CCC = VisionproInterface.VList[i].GetVppPath(fbNewName.Value);
                }
            }

            ReadAllProductData();
            fbNewName.Value = "";
            SysPara.items++;
            MiddleLayer.MainF.SaveData();
        }
        /// <summary>
        /// 删除当前选中的物料
        /// </summary>
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

                OperationLog.Write("ProductManagerForm", "删除型号：" + listView1.SelectedItems[0].Text);
                string filePath = SysPara.RecipeDataDirectory + "\\" + listView1.SelectedItems[0].Text + ".xml";
                File.Delete(filePath);

                for (int i = 0; i < VisionproInterface.VList.Count; i++)
                {
                    string VisionfilePath = VisionproInterface.VList[i].GetVppPath(listView1.SelectedItems[0].Text);

                    File.Delete(VisionfilePath);
                }

                ReadAllProductData();
            }
        }
        /// <summary>
        /// 使用当前选中的物料
        /// </summary>
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

                OperationLog.Write("ProductManagerForm", "切换当前型号：" + listView1.SelectedItems[0].Text);
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
