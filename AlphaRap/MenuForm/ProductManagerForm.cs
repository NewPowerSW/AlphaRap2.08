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
    /// 物料管理页（内嵌在 MainForm 里的单例页面，通过 MiddleLayer.ProductF 访问）。
    ///
    /// 重构要点：
    ///   1. 原来是固定 1707x1102 的绝对坐标 + 三块 FixedSingle 边框面板，
    ///      而宿主面板只有约 1360x780 —— 右侧"管理"整块和底部都被裁掉。
    ///      现在根容器 Dock=Fill，内部用 TableLayoutPanel 分成
    ///      「型号列表 36% / 右侧上下两张卡 64%」，整页跟着宿主伸缩。
    ///   2. 视觉统一到 UiKit：页头图标 + 白色圆角卡片 + 圆角输入框(FieldBox) + 圆角按钮(FlatButton)。
    ///   3. 输入框从裸 TextBox 换成 FieldBox，但**数据绑定一条都没少**：
    ///      绑定挂在 FieldBox.Inner 上（也就是它内部那个无边框 TextBox），路径与原来完全一致。
    ///   4. 补了两处实际缺陷：
    ///      - 未在列表里选中型号时"删除/使用"按钮可点（点了没反应，容易误以为程序坏了）→ 现在按选中状态启用/禁用；
    ///      - 列表没有条数提示 → 卡片底部显示"共 N 个型号"。
    ///
    /// 对外契约（ctor / ReadAllProductData / CurrentModel / listView1）
    /// 全部保留：MainForm 的语言切换与底栏配方名、MiddleLayer、AlarmRunTask 都在直接访问它们。
    ///
    /// 注意：**this.Text 不能改**。ModuleBaseForm.ModuleInitialize 拿 this.Text 当模块名，
    /// 去定位 ModuleData\SettingData\ProductManagerForm.xml；所以这里不做 Text 的本地化。
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

            // 本页静态文案（UiLabel / FlatButton 这些自绘子类）已登记进语言表：
            // RegisterLanguage 用 is 判断，子类也能进；真正的登记在 InitialLanguageData 末尾
            // （那时内存列表才建好），之后切语言由 SwitchLanguageTo → SwitchLanguage 自动换字。
            // 这里订阅事件只为**动态文案**：型号条数是运行时拼的，页面正显示着切语言要重算一次。
            MiddleLayer.LanguageChanged += ProductManagerForm_LanguageChanged;

            // 选中项变化 → 重新计算"删除 / 使用"是否可点
            listView1.SelectedIndexChanged += listView1_SelectedIndexChanged;

            ReadAllProductData();
            RefreshCurrentModelText();   // 当前配方名（构造里原来那一小段取值逻辑统一挪进本方法）

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

                // 参数输入框刻意不加前置图标：6 个框都挂图标会把版面切得很碎，
                // 而且 PRODUCTID / STEPID 这类字段本来也没有可对应的语义图形。
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
                // 本页**静态文案已交给语言表**：InitialLanguageData 末尾补登记 ProductManagerForm
                // （RegisterLanguage 用 is 判断，UiLabel / FlatButton 这些自绘子类也能进），
                // 键 = 控件名，翻译在 LanguageData\*.xml 的 ProductManagerForm 段里改；
                // 切语言统一走 MainForm.SwitchLanguageTo → SwitchLanguage，这里不用再逐个控件写文案。
                //
                // 剩下要自己管的只有**动态文案**：型号条数是运行时拼的（"共 N 个型号"），语言包管不到。
                _countFormat = MiddleLayer.LangMsg("ProductManagerForm", "msg_ModelCount",
                    "共 {0} 个型号", "{0} model(s)", "{0} modelo(s)");

                // 型号列表的表头（ListView 不进语言表 —— 它的文字算数据/列头，这里自己刷）
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
        /// 把 6 个 FieldBox 绑到配方表的 ProductSetting 表上。
        ///
        /// 为什么绑在 Inner 而不是 FieldBox 自己：
        ///   FieldBox 是自绘 Panel，内容走 Value 属性（刻意避开 Text，防止被语言扫描覆盖），
        ///   而 WinForms 的 Binding 只认标准控件属性；绑 Inner（内部那个真正的 TextBox）
        ///   就等于回到原来 textBox1.DataBindings.Add("Text", ...) 的写法，双向同步的语义完全一致。
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

            // 内容变化 → 打脏标记（沿用原 textBox4_TextChanged 的逻辑）
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
        /// 把"当前型号"这个标签的文字重新按当前配方写一遍。
        ///
        /// 为什么要显式刷：`CurrentModel` 按名字被登记进了语言表（控件名是**对外契约，不能改**），
        /// 于是切语言时语言表会拿语言包里的静态文字覆盖它 —— 但它显示的是**数据**（配方名），
        /// 被覆盖后会显示成语言包里那条没意义的底稿，直到下次换型号才恢复。
        /// <see cref="ApplyLanguage"/> 在切换语言后（LanguageChanged）会调用本方法，把它纠正回来。
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
