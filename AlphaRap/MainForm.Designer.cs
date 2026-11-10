namespace AlphaRap
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing) DisposeFrameResources();
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.LoginOutTime = new System.Windows.Forms.Timer(this.components);
            this.txtCyCT = new System.Windows.Forms.TextBox();
            this.miniToolStrip = new System.Windows.Forms.MenuStrip();
            this.tableLayoutPanel_Main = new System.Windows.Forms.TableLayoutPanel();
            this.panel4 = new AlphaRap.GradientTableLayoutPanel();
            this.panelBrand = new System.Windows.Forms.Panel();
            this.picLogo = new System.Windows.Forms.PictureBox();
            this.lblBrandName = new System.Windows.Forms.Label();
            this.lblBrandCaption = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.NumC1 = new System.Windows.Forms.ToolStripMenuItem();
            this.NumC2 = new System.Windows.Forms.ToolStripMenuItem();
            this.NumC3 = new System.Windows.Forms.ToolStripMenuItem();
            this.NumC4 = new System.Windows.Forms.ToolStripMenuItem();
            this.lblMonitorGroup = new System.Windows.Forms.Label();
            this.Panl_PictureShowTop = new System.Windows.Forms.FlowLayoutPanel();
            this.MENU_Home = new System.Windows.Forms.PictureBox();
            this.MENU_Vision = new System.Windows.Forms.PictureBox();
            this.MENU_Save = new System.Windows.Forms.PictureBox();
            this.MENU_Rapid = new System.Windows.Forms.PictureBox();
            this.MENU_Hard = new System.Windows.Forms.PictureBox();
            this.MENU_Manual = new System.Windows.Forms.PictureBox();
            this.MENU_LifeSpan = new System.Windows.Forms.PictureBox();
            this.lblSystemGroup = new System.Windows.Forms.Label();
            this.Panl_PictureShowDown = new System.Windows.Forms.FlowLayoutPanel();
            this.MENU_Mes = new System.Windows.Forms.PictureBox();
            this.MENU_AddUser = new System.Windows.Forms.PictureBox();
            this.MENU_Data = new System.Windows.Forms.PictureBox();
            this.MENU_Check = new System.Windows.Forms.PictureBox();
            this.MENU_RuntimeLog = new System.Windows.Forms.PictureBox();
            this.panelLogin = new AlphaRap.CardPanel();
            this.MENU_Login = new System.Windows.Forms.PictureBox();
            this.LoginText = new System.Windows.Forms.Label();
            this.panel_PictureShowDown = new System.Windows.Forms.Panel();
            this.panel_PictureShowLeft = new System.Windows.Forms.FlowLayoutPanel();
            this.MENU_Reset = new System.Windows.Forms.PictureBox();
            this.MENU_Run = new System.Windows.Forms.PictureBox();
            this.MENU_Pause = new System.Windows.Forms.PictureBox();
            this.MENU_Stop = new System.Windows.Forms.PictureBox();
            this.AlarmReset = new System.Windows.Forms.PictureBox();
            this.panel_PictureShowRight = new System.Windows.Forms.FlowLayoutPanel();
            this.MENU_Exit = new System.Windows.Forms.PictureBox();
            this.MENU_System = new System.Windows.Forms.PictureBox();
            this.MENU_Lock = new System.Windows.Forms.PictureBox();
            this.panel_Second = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.dashboardToolbar = new System.Windows.Forms.Panel();
            this.dashboardHomeTab = new System.Windows.Forms.Label();
            this.dashboardVisionTab = new System.Windows.Forms.Label();
            this.uiTabControl1 = new Sunny.UI.UITabControl();
            this.tabPage22 = new System.Windows.Forms.TabPage();
            this.plMainShow = new System.Windows.Forms.Panel();
            this.tabPage10 = new System.Windows.Forms.TabPage();
            this.alarmCard = new AlphaRap.CardPanel();
            this.WarnningMessage = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panelAlarmFilter = new System.Windows.Forms.Panel();
            this.flowAlarmFilter = new System.Windows.Forms.FlowLayoutPanel();
            this.btnAlarmFilterAll = new AlphaRap.AlarmChip();
            this.btnAlarmFilterWarn = new AlphaRap.AlarmChip();
            this.btnAlarmFilterAlarm = new AlphaRap.AlarmChip();
            this.btnAlarmQuery = new AlphaRap.AlarmChip();
            this.lblAlarmTitle = new System.Windows.Forms.Label();
            this.panel1 = new AlphaRap.CardPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.MachineStatus = new System.Windows.Forms.Label();
            this.MENU_Product = new System.Windows.Forms.PictureBox();
            this.lbRecipeName = new System.Windows.Forms.Label();
            this.recipeDetails = new System.Windows.Forms.TableLayoutPanel();
            this.lblRecipeCaption = new System.Windows.Forms.Label();
            this.picVirtualKeyboard = new System.Windows.Forms.PictureBox();
            this.languageSwitch = new AlphaRap.LanguageSwitch();
            this.statusStrip2 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel5 = new AlphaRap.StatusClockLabel();
            this.footerTimeCaption = new System.Windows.Forms.ToolStripStatusLabel();
            this.footerVersionCaption = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel4 = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)(this.SettingData)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.RecipeData)).BeginInit();
            this.tableLayoutPanel_Main.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panelBrand.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.Panl_PictureShowTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MENU_Home)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MENU_Vision)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MENU_Save)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MENU_Rapid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MENU_Hard)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MENU_Manual)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MENU_LifeSpan)).BeginInit();
            this.Panl_PictureShowDown.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MENU_Mes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MENU_AddUser)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MENU_Data)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MENU_Check)).BeginInit();
            this.panelLogin.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MENU_Login)).BeginInit();
            this.panel_PictureShowDown.SuspendLayout();
            this.panel_PictureShowLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MENU_Reset)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MENU_Run)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MENU_Pause)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MENU_Stop)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AlarmReset)).BeginInit();
            this.panel_PictureShowRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MENU_Exit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MENU_System)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MENU_Lock)).BeginInit();
            this.panel_Second.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.panel5.SuspendLayout();
            this.uiTabControl1.SuspendLayout();
            this.tabPage22.SuspendLayout();
            this.alarmCard.SuspendLayout();
            this.panelAlarmFilter.SuspendLayout();
            this.flowAlarmFilter.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MENU_Product)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picVirtualKeyboard)).BeginInit();
            this.statusStrip2.SuspendLayout();
            this.SuspendLayout();
            //
            // timer1
            //
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            //
            // LoginOutTime
            //
            this.LoginOutTime.Enabled = true;
            this.LoginOutTime.Interval = 60000;
            this.LoginOutTime.Tick += new System.EventHandler(this.LoginOutTime_Tick);
            //
            // txtCyCT
            //
            this.txtCyCT.BackColor = System.Drawing.Color.White;
            this.txtCyCT.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtCyCT.Font = new System.Drawing.Font("微软雅黑", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtCyCT.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(108)))), ((int)(((byte)(182)))));
            this.txtCyCT.Location = new System.Drawing.Point(66, 496);
            this.txtCyCT.Name = "txtCyCT";
            this.txtCyCT.Size = new System.Drawing.Size(54, 25);
            this.txtCyCT.TabIndex = 236;
            this.txtCyCT.Text = "0";
            this.txtCyCT.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            //
            // miniToolStrip
            //
            this.miniToolStrip.AccessibleName = "新项选择";
            this.miniToolStrip.AccessibleRole = System.Windows.Forms.AccessibleRole.ComboBox;
            this.miniToolStrip.AutoSize = false;
            this.miniToolStrip.BackColor = System.Drawing.Color.White;
            this.miniToolStrip.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.miniToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.miniToolStrip.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
            this.miniToolStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.miniToolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Table;
            this.miniToolStrip.Location = new System.Drawing.Point(0, 36);
            this.miniToolStrip.Name = "miniToolStrip";
            this.miniToolStrip.Padding = new System.Windows.Forms.Padding(0);
            this.miniToolStrip.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.miniToolStrip.Size = new System.Drawing.Size(152, 49);
            this.miniToolStrip.TabIndex = 0;
            //
            // tableLayoutPanel_Main
            //
            this.tableLayoutPanel_Main.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(237)))), ((int)(((byte)(244)))));
            this.tableLayoutPanel_Main.ColumnCount = 2;
            this.tableLayoutPanel_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 232F));
            this.tableLayoutPanel_Main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_Main.Controls.Add(this.panelBrand, 0, 0);
            this.tableLayoutPanel_Main.Controls.Add(this.panel4, 0, 1);
            this.tableLayoutPanel_Main.Controls.Add(this.panel_PictureShowDown, 1, 0);
            this.tableLayoutPanel_Main.Controls.Add(this.panel_Second, 1, 1);
            this.tableLayoutPanel_Main.Controls.Add(this.panel1, 1, 3);
            this.tableLayoutPanel_Main.SetRowSpan(this.panel_Second, 2);
            this.tableLayoutPanel_Main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_Main.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_Main.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel_Main.Name = "tableLayoutPanel_Main";
            this.tableLayoutPanel_Main.RowCount = 4;
            this.tableLayoutPanel_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 52F));
            this.tableLayoutPanel_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));
            this.tableLayoutPanel_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_Main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel_Main.Size = new System.Drawing.Size(1600, 1000);
            this.tableLayoutPanel_Main.TabIndex = 0;
            //
            // panel4
            //
            this.panel4.BackColor = System.Drawing.Color.FromArgb(7, 105, 169);
            this.panel4.ColumnCount = 1;
            this.panel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.panel4.Controls.Add(this.lblMonitorGroup, 0, 0);
            this.panel4.Controls.Add(this.Panl_PictureShowTop, 0, 1);
            this.panel4.Controls.Add(this.lblSystemGroup, 0, 2);
            this.panel4.Controls.Add(this.Panl_PictureShowDown, 0, 3);
            this.panel4.Controls.Add(this.panelLogin, 0, 5);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.GradientBottom = System.Drawing.Color.FromArgb(5, 80, 135);
            this.panel4.GradientTop = System.Drawing.Color.FromArgb(7, 105, 169);
            this.panel4.Location = new System.Drawing.Point(0, 0);
            this.panel4.Margin = new System.Windows.Forms.Padding(0);
            this.panel4.Name = "panel4";
            this.panel4.RowCount = 6;
            this.tableLayoutPanel_Main.SetRowSpan(this.panel4, 3);
            this.panel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.panel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 336F));
            this.panel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.panel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 210F));
            this.panel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.panel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.panel4.Size = new System.Drawing.Size(224, 1000);
            this.panel4.TabIndex = 0;
            //
            // panelBrand
            //
            this.panelBrand.BackColor = System.Drawing.Color.FromArgb(7, 113, 182);
            this.panelBrand.Controls.Add(this.picLogo);
            this.panelBrand.Controls.Add(this.lblBrandName);
            this.panelBrand.Controls.Add(this.lblBrandCaption);
            this.panelBrand.Controls.Add(this.menuStrip1);
            this.panelBrand.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBrand.Location = new System.Drawing.Point(0, 0);
            this.panelBrand.Margin = new System.Windows.Forms.Padding(0);
            this.panelBrand.Name = "panelBrand";
            this.panelBrand.Size = new System.Drawing.Size(232, 52);
            this.panelBrand.TabIndex = 0;
            //
            // picLogo
            //
            this.picLogo.BackColor = System.Drawing.Color.FromArgb(7, 113, 182);
            this.picLogo.Image = ((System.Drawing.Image)(resources.GetObject("picLogo.Image")));
            this.picLogo.Location = new System.Drawing.Point(18, 12);
            this.picLogo.Margin = new System.Windows.Forms.Padding(8, 6, 0, 0);
            this.picLogo.Name = "picLogo";
            this.picLogo.Size = new System.Drawing.Size(30, 30);
            this.picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picLogo.TabIndex = 220;
            this.picLogo.TabStop = false;
            //
            // lblBrandName
            //
            this.lblBrandName.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblBrandName.ForeColor = System.Drawing.Color.White;
            this.lblBrandName.Location = new System.Drawing.Point(58, 9);
            this.lblBrandName.Name = "lblBrandName";
            this.lblBrandName.Size = new System.Drawing.Size(150, 24);
            this.lblBrandName.TabIndex = 221;
            this.lblBrandName.Text = "AlphaRap";
            //
            // lblBrandCaption
            //
            this.lblBrandCaption.Font = new System.Drawing.Font("Segoe UI", 7F);
            this.lblBrandCaption.ForeColor = System.Drawing.Color.FromArgb(218, 239, 253);
            this.lblBrandCaption.Location = new System.Drawing.Point(58, 30);
            this.lblBrandCaption.Name = "lblBrandCaption";
            this.lblBrandCaption.Size = new System.Drawing.Size(150, 18);
            this.lblBrandCaption.TabIndex = 222;
            this.lblBrandCaption.Text = "MES & FLOW STUDIO";
            //
            // menuStrip1
            //
            this.menuStrip1.AutoSize = false;
            this.menuStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(108)))), ((int)(((byte)(182)))));
            this.menuStrip1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.menuStrip1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.menuStrip1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.menuStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Visible;
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(45, 30);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NumC1});
            this.menuStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Table;
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(0);
            this.menuStrip1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.menuStrip1.Size = new System.Drawing.Size(157, 50);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.Visible = false;
            //
            // NumC1
            //
            this.NumC1.AutoSize = false;
            this.NumC1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(108)))), ((int)(((byte)(182)))));
            this.NumC1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NumC2,
            this.NumC3,
            this.NumC4});
            this.NumC1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.NumC1.ForeColor = System.Drawing.Color.White;
            this.NumC1.Image = ((System.Drawing.Image)(resources.GetObject("NumC1.Image")));
            this.NumC1.Margin = new System.Windows.Forms.Padding(5);
            this.NumC1.Name = "NumC1";
            this.NumC1.Size = new System.Drawing.Size(152, 36);
            this.NumC1.Text = "EN";
            this.NumC1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // NumC2
            //
            this.NumC2.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.NumC2.ForeColor = System.Drawing.Color.Black;
            this.NumC2.Image = ((System.Drawing.Image)(resources.GetObject("NumC2.Image")));
            this.NumC2.Name = "NumC2";
            this.NumC2.Size = new System.Drawing.Size(132, 24);
            this.NumC2.Text = "Chinese";
            //
            // NumC3
            //
            this.NumC3.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.NumC3.ForeColor = System.Drawing.Color.Black;
            this.NumC3.Image = ((System.Drawing.Image)(resources.GetObject("NumC3.Image")));
            this.NumC3.Name = "NumC3";
            this.NumC3.Size = new System.Drawing.Size(132, 24);
            this.NumC3.Text = "English";
            //
            // NumC4
            //
            this.NumC4.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.NumC4.ForeColor = System.Drawing.Color.Black;
            this.NumC4.Image = ((System.Drawing.Image)(resources.GetObject("NumC4.Image")));
            this.NumC4.Name = "NumC4";
            this.NumC4.Size = new System.Drawing.Size(132, 24);
            this.NumC4.Text = "Español";
            //
            // lblMonitorGroup
            //
            this.lblMonitorGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMonitorGroup.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F);
            this.lblMonitorGroup.ForeColor = System.Drawing.Color.FromArgb(218, 239, 253);
            this.lblMonitorGroup.Location = new System.Drawing.Point(24, 68);
            this.lblMonitorGroup.Margin = new System.Windows.Forms.Padding(24, 0, 0, 0);
            this.lblMonitorGroup.Name = "lblMonitorGroup";
            this.lblMonitorGroup.Size = new System.Drawing.Size(200, 32);
            this.lblMonitorGroup.TabIndex = 1;
            this.lblMonitorGroup.Text = "监控";
            this.lblMonitorGroup.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // Panl_PictureShowTop
            //
            this.Panl_PictureShowTop.BackColor = System.Drawing.Color.Transparent;
            this.Panl_PictureShowTop.Controls.Add(this.MENU_Home);
            this.Panl_PictureShowTop.Controls.Add(this.MENU_Product);
            this.Panl_PictureShowTop.Controls.Add(this.MENU_Vision);
            this.Panl_PictureShowTop.Controls.Add(this.MENU_Save);
            this.Panl_PictureShowTop.Controls.Add(this.MENU_Rapid);
            this.Panl_PictureShowTop.Controls.Add(this.MENU_Hard);
            this.Panl_PictureShowTop.Controls.Add(this.MENU_Manual);
            this.Panl_PictureShowTop.Controls.Add(this.MENU_LifeSpan);
            this.Panl_PictureShowTop.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Panl_PictureShowTop.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.Panl_PictureShowTop.Location = new System.Drawing.Point(0, 100);
            this.Panl_PictureShowTop.Margin = new System.Windows.Forms.Padding(0);
            this.Panl_PictureShowTop.Name = "Panl_PictureShowTop";
            this.Panl_PictureShowTop.Padding = new System.Windows.Forms.Padding(12, 0, 12, 0);
            this.Panl_PictureShowTop.Size = new System.Drawing.Size(224, 336);
            this.Panl_PictureShowTop.TabIndex = 2;
            this.Panl_PictureShowTop.WrapContents = false;
            //
            // MENU_Home
            //
            this.MENU_Home.BackColor = System.Drawing.Color.Transparent;
            this.MENU_Home.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.MENU_Home.Location = new System.Drawing.Point(12, 2);
            this.MENU_Home.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.MENU_Home.Name = "MENU_Home";
            this.MENU_Home.Size = new System.Drawing.Size(200, 38);
            this.MENU_Home.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.MENU_Home.TabIndex = 3;
            this.MENU_Home.TabStop = false;
            this.MENU_Home.Tag = "Home";
            this.MENU_Home.Click += new System.EventHandler(this.MENU_Click);
            this.MENU_Home.MouseEnter += new System.EventHandler(this.MouseEnter1);
            //
            // MENU_Vision
            //
            this.MENU_Vision.BackColor = System.Drawing.Color.Transparent;
            this.MENU_Vision.Location = new System.Drawing.Point(12, 86);
            this.MENU_Vision.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.MENU_Vision.Name = "MENU_Vision";
            this.MENU_Vision.Padding = new System.Windows.Forms.Padding(1);
            this.MENU_Vision.Size = new System.Drawing.Size(200, 38);
            this.MENU_Vision.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.MENU_Vision.TabIndex = 3;
            this.MENU_Vision.TabStop = false;
            this.MENU_Vision.Tag = "Vision";
            this.MENU_Vision.Click += new System.EventHandler(this.MENU_Click);
            this.MENU_Vision.MouseEnter += new System.EventHandler(this.MouseEnter1);
            //
            // MENU_Save
            //
            this.MENU_Save.BackColor = System.Drawing.Color.Transparent;
            this.MENU_Save.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.MENU_Save.Location = new System.Drawing.Point(12, 128);
            this.MENU_Save.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.MENU_Save.Name = "MENU_Save";
            this.MENU_Save.Size = new System.Drawing.Size(200, 38);
            this.MENU_Save.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.MENU_Save.TabIndex = 3;
            this.MENU_Save.TabStop = false;
            this.MENU_Save.Tag = "Save";
            this.MENU_Save.Click += new System.EventHandler(this.SaveData_Click);
            this.MENU_Save.MouseEnter += new System.EventHandler(this.MouseEnter1);
            //
            // MENU_Rapid
            //
            this.MENU_Rapid.BackColor = System.Drawing.Color.Transparent;
            this.MENU_Rapid.Location = new System.Drawing.Point(12, 170);
            this.MENU_Rapid.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.MENU_Rapid.Name = "MENU_Rapid";
            this.MENU_Rapid.Padding = new System.Windows.Forms.Padding(1);
            this.MENU_Rapid.Size = new System.Drawing.Size(200, 38);
            this.MENU_Rapid.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.MENU_Rapid.TabIndex = 3;
            this.MENU_Rapid.TabStop = false;
            this.MENU_Rapid.Tag = "Rapid";
            this.MENU_Rapid.Click += new System.EventHandler(this.MENU_Click);
            this.MENU_Rapid.MouseEnter += new System.EventHandler(this.MouseEnter1);
            //
            // MENU_Hard
            //
            this.MENU_Hard.BackColor = System.Drawing.Color.Transparent;
            this.MENU_Hard.Location = new System.Drawing.Point(12, 212);
            this.MENU_Hard.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.MENU_Hard.Name = "MENU_Hard";
            this.MENU_Hard.Size = new System.Drawing.Size(200, 38);
            this.MENU_Hard.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.MENU_Hard.TabIndex = 12;
            this.MENU_Hard.TabStop = false;
            this.MENU_Hard.Tag = "Hard";
            this.MENU_Hard.Click += new System.EventHandler(this.MENU_Click);
            this.MENU_Hard.MouseEnter += new System.EventHandler(this.MouseEnter1);
            //
            // MENU_Manual
            //
            this.MENU_Manual.BackColor = System.Drawing.Color.Transparent;
            this.MENU_Manual.Location = new System.Drawing.Point(12, 254);
            this.MENU_Manual.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.MENU_Manual.Name = "MENU_Manual";
            this.MENU_Manual.Padding = new System.Windows.Forms.Padding(1);
            this.MENU_Manual.Size = new System.Drawing.Size(200, 38);
            this.MENU_Manual.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.MENU_Manual.TabIndex = 3;
            this.MENU_Manual.TabStop = false;
            this.MENU_Manual.Tag = "Manual";
            this.MENU_Manual.Click += new System.EventHandler(this.MENU_Click);
            this.MENU_Manual.MouseEnter += new System.EventHandler(this.MouseEnter1);
            //
            // MENU_LifeSpan
            //
            this.MENU_LifeSpan.BackColor = System.Drawing.Color.Transparent;
            this.MENU_LifeSpan.Location = new System.Drawing.Point(12, 296);
            this.MENU_LifeSpan.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.MENU_LifeSpan.Name = "MENU_LifeSpan";
            this.MENU_LifeSpan.Padding = new System.Windows.Forms.Padding(1);
            this.MENU_LifeSpan.Size = new System.Drawing.Size(200, 38);
            this.MENU_LifeSpan.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.MENU_LifeSpan.TabIndex = 7;
            this.MENU_LifeSpan.TabStop = false;
            this.MENU_LifeSpan.Tag = "LifeSpan";
            this.MENU_LifeSpan.Click += new System.EventHandler(this.MENU_Click);
            this.MENU_LifeSpan.MouseEnter += new System.EventHandler(this.MouseEnter1);
            //
            // lblSystemGroup
            //
            this.lblSystemGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSystemGroup.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F);
            this.lblSystemGroup.ForeColor = System.Drawing.Color.FromArgb(218, 239, 253);
            this.lblSystemGroup.Location = new System.Drawing.Point(24, 436);
            this.lblSystemGroup.Margin = new System.Windows.Forms.Padding(24, 0, 0, 0);
            this.lblSystemGroup.Name = "lblSystemGroup";
            this.lblSystemGroup.Size = new System.Drawing.Size(200, 30);
            this.lblSystemGroup.TabIndex = 3;
            this.lblSystemGroup.Text = "系统";
            this.lblSystemGroup.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // Panl_PictureShowDown
            //
            this.Panl_PictureShowDown.BackColor = System.Drawing.Color.Transparent;
            this.Panl_PictureShowDown.Controls.Add(this.MENU_Mes);
            this.Panl_PictureShowDown.Controls.Add(this.MENU_AddUser);
            this.Panl_PictureShowDown.Controls.Add(this.MENU_Data);
            this.Panl_PictureShowDown.Controls.Add(this.MENU_Check);
            this.Panl_PictureShowDown.Controls.Add(this.MENU_RuntimeLog);
            this.Panl_PictureShowDown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Panl_PictureShowDown.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.Panl_PictureShowDown.Location = new System.Drawing.Point(0, 466);
            this.Panl_PictureShowDown.Margin = new System.Windows.Forms.Padding(0);
            this.Panl_PictureShowDown.Name = "Panl_PictureShowDown";
            this.Panl_PictureShowDown.Padding = new System.Windows.Forms.Padding(12, 0, 12, 0);
            this.Panl_PictureShowDown.Size = new System.Drawing.Size(224, 168);
            this.Panl_PictureShowDown.TabIndex = 4;
            this.Panl_PictureShowDown.WrapContents = false;
            //
            // MENU_Mes
            //
            this.MENU_Mes.BackColor = System.Drawing.Color.Transparent;
            this.MENU_Mes.Location = new System.Drawing.Point(12, 2);
            this.MENU_Mes.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.MENU_Mes.Name = "MENU_Mes";
            this.MENU_Mes.Padding = new System.Windows.Forms.Padding(1);
            this.MENU_Mes.Size = new System.Drawing.Size(200, 38);
            this.MENU_Mes.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.MENU_Mes.TabIndex = 3;
            this.MENU_Mes.TabStop = false;
            this.MENU_Mes.Tag = "Mes";
            this.MENU_Mes.Click += new System.EventHandler(this.MENU_Click);
            this.MENU_Mes.MouseEnter += new System.EventHandler(this.MouseEnter1);
            //
            // MENU_AddUser
            //
            this.MENU_AddUser.BackColor = System.Drawing.Color.Transparent;
            this.MENU_AddUser.Location = new System.Drawing.Point(12, 44);
            this.MENU_AddUser.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.MENU_AddUser.Name = "MENU_AddUser";
            this.MENU_AddUser.Padding = new System.Windows.Forms.Padding(1);
            this.MENU_AddUser.Size = new System.Drawing.Size(200, 38);
            this.MENU_AddUser.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.MENU_AddUser.TabIndex = 3;
            this.MENU_AddUser.TabStop = false;
            this.MENU_AddUser.Tag = "AddUser";
            this.MENU_AddUser.Click += new System.EventHandler(this.MENU_Click);
            this.MENU_AddUser.MouseEnter += new System.EventHandler(this.MouseEnter1);
            //
            // MENU_Data
            //
            this.MENU_Data.BackColor = System.Drawing.Color.Transparent;
            this.MENU_Data.Location = new System.Drawing.Point(12, 86);
            this.MENU_Data.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.MENU_Data.Name = "MENU_Data";
            this.MENU_Data.Padding = new System.Windows.Forms.Padding(1);
            this.MENU_Data.Size = new System.Drawing.Size(200, 38);
            this.MENU_Data.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.MENU_Data.TabIndex = 3;
            this.MENU_Data.TabStop = false;
            this.MENU_Data.Tag = "Data";
            this.MENU_Data.Click += new System.EventHandler(this.MENU_Click);
            this.MENU_Data.MouseEnter += new System.EventHandler(this.MouseEnter1);
            //
            // MENU_Check
            //
            this.MENU_Check.BackColor = System.Drawing.Color.Transparent;
            this.MENU_Check.Location = new System.Drawing.Point(12, 128);
            this.MENU_Check.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.MENU_Check.Name = "MENU_Check";
            this.MENU_Check.Padding = new System.Windows.Forms.Padding(1);
            this.MENU_Check.Size = new System.Drawing.Size(200, 38);
            this.MENU_Check.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.MENU_Check.TabIndex = 3;
            this.MENU_Check.TabStop = false;
            this.MENU_Check.Tag = "Check";
            this.MENU_Check.Click += new System.EventHandler(this.MENU_Click);
            this.MENU_Check.MouseEnter += new System.EventHandler(this.MouseEnter1);
            this.MENU_RuntimeLog.Name = "MENU_RuntimeLog";
            this.MENU_RuntimeLog.Tag = "RuntimeLog";
            this.MENU_RuntimeLog.BackColor = System.Drawing.Color.Transparent;
            this.MENU_RuntimeLog.Size = new System.Drawing.Size(212, 38);
            this.MENU_RuntimeLog.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.MENU_RuntimeLog.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.MENU_RuntimeLog.TabStop = false;
            this.MENU_RuntimeLog.Click += new System.EventHandler(this.MENU_Click);
            this.MENU_RuntimeLog.MouseEnter += new System.EventHandler(this.MouseEnter1);
            //
            // panelLogin
            //
            this.panelLogin.BackColor = System.Drawing.Color.FromArgb(5, 65, 110);
            this.panelLogin.FillColor = System.Drawing.Color.FromArgb(5, 65, 110);
            this.panelLogin.CornerRadius = 9;
            this.panelLogin.ShowBorder = false;
            this.panelLogin.ShowShadow = false;
            this.panelLogin.Controls.Add(this.MENU_Login);
            this.panelLogin.Controls.Add(this.LoginText);
            this.panelLogin.Cursor = System.Windows.Forms.Cursors.Hand;
            this.panelLogin.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelLogin.Location = new System.Drawing.Point(0, 930);
            this.panelLogin.Margin = new System.Windows.Forms.Padding(12, 4, 12, 12);
            this.panelLogin.Name = "panelLogin";
            this.panelLogin.Size = new System.Drawing.Size(224, 70);
            this.panelLogin.TabIndex = 223;
            //
            // MENU_Login
            //
            this.MENU_Login.BackColor = System.Drawing.Color.Transparent;
            this.MENU_Login.Location = new System.Drawing.Point(12, 12);
            this.MENU_Login.Margin = new System.Windows.Forms.Padding(0);
            this.MENU_Login.Name = "MENU_Login";
            this.MENU_Login.Size = new System.Drawing.Size(30, 30);
            this.MENU_Login.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.MENU_Login.TabIndex = 3;
            this.MENU_Login.TabStop = false;
            this.MENU_Login.Tag = "Login";
            this.MENU_Login.Click += new System.EventHandler(this.UserLogin_Click);
            this.MENU_Login.MouseEnter += new System.EventHandler(this.MouseEnter1);
            //
            // LoginText
            //
            this.LoginText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LoginText.AutoEllipsis = true;
            this.LoginText.BackColor = System.Drawing.Color.Transparent;
            this.LoginText.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LoginText.ForeColor = System.Drawing.Color.White;
            this.LoginText.Location = new System.Drawing.Point(50, 8);
            this.LoginText.Margin = new System.Windows.Forms.Padding(0);
            this.LoginText.Name = "LoginText_dyn";
            this.LoginText.Size = new System.Drawing.Size(144, 38);
            this.LoginText.TabIndex = 7;
            this.LoginText.Text = "admin\r\n管理员";
            this.LoginText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // panel_PictureShowDown
            //
            this.panel_PictureShowDown.BackColor = System.Drawing.Color.FromArgb(7, 105, 169);
            this.panel_PictureShowDown.Controls.Add(this.panel_PictureShowLeft);
            this.panel_PictureShowDown.Controls.Add(this.panel_PictureShowRight);
            this.panel_PictureShowDown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_PictureShowDown.Location = new System.Drawing.Point(232, 0);
            this.panel_PictureShowDown.Margin = new System.Windows.Forms.Padding(0);
            this.panel_PictureShowDown.Name = "panel_PictureShowDown";
            this.panel_PictureShowDown.Size = new System.Drawing.Size(1368, 52);
            this.panel_PictureShowDown.TabIndex = 26;
            //
            // panel_PictureShowLeft
            //
            this.panel_PictureShowLeft.BackColor = System.Drawing.Color.Transparent;
            this.panel_PictureShowLeft.Controls.Add(this.MENU_Reset);
            this.panel_PictureShowLeft.Controls.Add(this.MENU_Run);
            this.panel_PictureShowLeft.Controls.Add(this.MENU_Pause);
            this.panel_PictureShowLeft.Controls.Add(this.MENU_Stop);
            this.panel_PictureShowLeft.Controls.Add(this.AlarmReset);
            this.panel_PictureShowLeft.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel_PictureShowLeft.Location = new System.Drawing.Point(584, 0);
            this.panel_PictureShowLeft.Name = "panel_PictureShowLeft";
            this.panel_PictureShowLeft.Padding = new System.Windows.Forms.Padding(0, 10, 16, 0);
            this.panel_PictureShowLeft.Size = new System.Drawing.Size(486, 52);
            this.panel_PictureShowLeft.TabIndex = 26;
            this.panel_PictureShowLeft.WrapContents = false;
            //
            // MENU_Reset
            //
            this.MENU_Reset.BackColor = System.Drawing.Color.Transparent;
            this.MENU_Reset.Location = new System.Drawing.Point(12, 6);
            this.MENU_Reset.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.MENU_Reset.Name = "MENU_Reset";
            this.MENU_Reset.Padding = new System.Windows.Forms.Padding(1);
            this.MENU_Reset.Size = new System.Drawing.Size(88, 40);
            this.MENU_Reset.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.MENU_Reset.TabIndex = 6;
            this.MENU_Reset.TabStop = false;
            this.MENU_Reset.Tag = "Reset";
            this.MENU_Reset.Click += new System.EventHandler(this.MENU_Reset_Click);
            //
            // MENU_Run
            //
            this.MENU_Run.BackColor = System.Drawing.Color.Transparent;
            this.MENU_Run.Location = new System.Drawing.Point(108, 6);
            this.MENU_Run.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.MENU_Run.Name = "MENU_Run";
            this.MENU_Run.Padding = new System.Windows.Forms.Padding(1);
            this.MENU_Run.Size = new System.Drawing.Size(88, 40);
            this.MENU_Run.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.MENU_Run.TabIndex = 5;
            this.MENU_Run.TabStop = false;
            this.MENU_Run.Tag = "Run";
            this.MENU_Run.Click += new System.EventHandler(this.btStart_Click);
            //
            // MENU_Pause
            //
            this.MENU_Pause.BackColor = System.Drawing.Color.Transparent;
            this.MENU_Pause.Location = new System.Drawing.Point(204, 6);
            this.MENU_Pause.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.MENU_Pause.Name = "MENU_Pause";
            this.MENU_Pause.Padding = new System.Windows.Forms.Padding(1);
            this.MENU_Pause.Size = new System.Drawing.Size(88, 40);
            this.MENU_Pause.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.MENU_Pause.TabIndex = 4;
            this.MENU_Pause.TabStop = false;
            this.MENU_Pause.Tag = "Pause";
            this.MENU_Pause.Click += new System.EventHandler(this.btPause_Click);
            //
            // MENU_Stop
            //
            this.MENU_Stop.BackColor = System.Drawing.Color.Transparent;
            this.MENU_Stop.Location = new System.Drawing.Point(300, 6);
            this.MENU_Stop.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.MENU_Stop.Name = "MENU_Stop";
            this.MENU_Stop.Size = new System.Drawing.Size(88, 40);
            this.MENU_Stop.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.MENU_Stop.TabIndex = 3;
            this.MENU_Stop.TabStop = false;
            this.MENU_Stop.Tag = "Stop";
            this.MENU_Stop.Click += new System.EventHandler(this.btStop_Click);
            //
            // AlarmReset
            //
            this.AlarmReset.BackColor = System.Drawing.Color.Transparent;
            this.AlarmReset.Location = new System.Drawing.Point(396, 6);
            this.AlarmReset.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.AlarmReset.Name = "AlarmReset";
            this.AlarmReset.Size = new System.Drawing.Size(88, 40);
            this.AlarmReset.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.AlarmReset.TabIndex = 25;
            this.AlarmReset.TabStop = false;
            this.AlarmReset.Tag = "Stop";
            this.AlarmReset.Click += new System.EventHandler(this.btAlarmReset_Click);
            //
            // panel_PictureShowRight
            //
            this.panel_PictureShowRight.BackColor = System.Drawing.Color.Transparent;
            this.panel_PictureShowRight.Controls.Add(this.MENU_Exit);
            this.panel_PictureShowRight.Controls.Add(this.MENU_System);
            this.panel_PictureShowRight.Controls.Add(this.MENU_Lock);
            this.panel_PictureShowRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel_PictureShowRight.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.panel_PictureShowRight.Location = new System.Drawing.Point(1070, 0);
            this.panel_PictureShowRight.Name = "panel_PictureShowRight";
            this.panel_PictureShowRight.Padding = new System.Windows.Forms.Padding(0, 10, 16, 0);
            this.panel_PictureShowRight.Size = new System.Drawing.Size(298, 52);
            this.panel_PictureShowRight.TabIndex = 27;
            this.panel_PictureShowRight.WrapContents = false;
            //
            // MENU_Exit
            //
            this.MENU_Exit.BackColor = System.Drawing.Color.Transparent;
            this.MENU_Exit.Location = new System.Drawing.Point(180, 6);
            this.MENU_Exit.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.MENU_Exit.Name = "MENU_Exit";
            this.MENU_Exit.Size = new System.Drawing.Size(88, 40);
            this.MENU_Exit.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.MENU_Exit.TabIndex = 4;
            this.MENU_Exit.TabStop = false;
            this.MENU_Exit.Tag = "Exit";
            this.MENU_Exit.Click += new System.EventHandler(this.btExit_Click);
            this.MENU_Exit.MouseEnter += new System.EventHandler(this.MouseEnter1);
            //
            // MENU_System
            //
            this.MENU_System.BackColor = System.Drawing.Color.Transparent;
            this.MENU_System.Location = new System.Drawing.Point(84, 6);
            this.MENU_System.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.MENU_System.Name = "MENU_System";
            this.MENU_System.Padding = new System.Windows.Forms.Padding(1);
            this.MENU_System.Size = new System.Drawing.Size(88, 40);
            this.MENU_System.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.MENU_System.TabIndex = 3;
            this.MENU_System.TabStop = false;
            this.MENU_System.Tag = "System";
            this.MENU_System.Click += new System.EventHandler(this.MENU_Click);
            //
            // MENU_Lock
            //
            this.MENU_Lock.BackColor = System.Drawing.Color.Transparent;
            this.MENU_Lock.Location = new System.Drawing.Point(-12, 6);
            this.MENU_Lock.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.MENU_Lock.Name = "MENU_Lock";
            this.MENU_Lock.Padding = new System.Windows.Forms.Padding(1);
            this.MENU_Lock.Size = new System.Drawing.Size(88, 40);
            this.MENU_Lock.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.MENU_Lock.TabIndex = 10;
            this.MENU_Lock.TabStop = false;
            this.MENU_Lock.Tag = "LOTO";
            this.MENU_Lock.Click += new System.EventHandler(this.LOTO_Click);
            //
            // panel_Second
            //
            this.panel_Second.Controls.Add(this.tableLayoutPanel1);
            this.panel_Second.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_Second.Location = new System.Drawing.Point(248, 74);
            this.panel_Second.Margin = new System.Windows.Forms.Padding(0);
            this.panel_Second.Name = "panel_Second";
            this.panel_Second.Size = new System.Drawing.Size(1328, 834);
            this.panel_Second.TabIndex = 13;
            //
            // tableLayoutPanel1
            //
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.panel2, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1328, 834);
            this.tableLayoutPanel1.TabIndex = 0;
            //
            // panel2
            //
            this.panel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(237)))), ((int)(((byte)(244)))));
            this.panel2.Controls.Add(this.tableLayoutPanel4);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Margin = new System.Windows.Forms.Padding(0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1328, 834);
            this.panel2.TabIndex = 12;
            //
            // tableLayoutPanel4
            //
            this.tableLayoutPanel4.ColumnCount = 1;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Controls.Add(this.panel5, 0, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(1328, 834);
            this.tableLayoutPanel4.TabIndex = 0;
            //
            // panel5
            //
            this.panel5.Controls.Add(this.uiTabControl1);
            this.panel5.Controls.Add(this.dashboardToolbar);
            this.dashboardToolbar.Name = "dashboardToolbar";
            this.dashboardToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.dashboardToolbar.Height = 48;
            this.dashboardToolbar.BackColor = System.Drawing.Color.White;
            this.dashboardToolbar.Controls.Add(this.dashboardVisionTab);
            this.dashboardToolbar.Controls.Add(this.dashboardHomeTab);
            this.dashboardHomeTab.Name = "dashboardHomeTab_dyn";
            this.dashboardHomeTab.Dock = System.Windows.Forms.DockStyle.Left;
            this.dashboardHomeTab.Width = 68;
            this.dashboardHomeTab.Text = "主页";
            this.dashboardHomeTab.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.dashboardHomeTab.Cursor = System.Windows.Forms.Cursors.Hand;
            this.dashboardHomeTab.ForeColor = System.Drawing.Color.FromArgb(0, 143, 211);
            this.dashboardVisionTab.Name = "dashboardVisionTab_dyn";
            this.dashboardVisionTab.Dock = System.Windows.Forms.DockStyle.Left;
            this.dashboardVisionTab.Width = 68;
            this.dashboardVisionTab.Text = "视觉";
            this.dashboardVisionTab.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.dashboardVisionTab.Cursor = System.Windows.Forms.Cursors.Hand;
            this.dashboardVisionTab.ForeColor = System.Drawing.Color.FromArgb(83, 102, 125);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel5.Location = new System.Drawing.Point(0, 0);
            this.panel5.Margin = new System.Windows.Forms.Padding(0);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(1328, 834);
            this.panel5.TabIndex = 0;
            //
            // uiTabControl1
            //
            this.uiTabControl1.Controls.Add(this.tabPage22);
            this.uiTabControl1.Controls.Add(this.tabPage10);
            this.uiTabControl1.Cursor = System.Windows.Forms.Cursors.No;
            this.uiTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiTabControl1.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.uiTabControl1.Font = new System.Drawing.Font("微软雅黑", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiTabControl1.ItemSize = new System.Drawing.Size(68, 1);
            this.uiTabControl1.TabVisible = false;
            this.uiTabControl1.Location = new System.Drawing.Point(0, 48);
            this.uiTabControl1.MainPage = "主页";
            this.uiTabControl1.Margin = new System.Windows.Forms.Padding(0);
            this.uiTabControl1.MenuStyle = Sunny.UI.UIMenuStyle.Custom;
            this.uiTabControl1.Name = "uiTabControl1";
            this.uiTabControl1.SelectedIndex = 0;
            this.uiTabControl1.Size = new System.Drawing.Size(1328, 786);
            this.uiTabControl1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.uiTabControl1.Style = Sunny.UI.UIStyle.Custom;
            this.uiTabControl1.TabBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(3)))), ((int)(((byte)(92)))), ((int)(((byte)(156)))));
            this.uiTabControl1.TabIndex = 206;
            this.uiTabControl1.TabSelectedColor = System.Drawing.Color.White;
            this.uiTabControl1.TabSelectedForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(108)))), ((int)(((byte)(182)))));
            this.uiTabControl1.TabUnSelectedForeColor = System.Drawing.Color.White;
            this.uiTabControl1.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            //
            // tabPage22
            //
            this.tabPage22.Controls.Add(this.plMainShow);
            this.tabPage22.Controls.Add(this.alarmCard);
            this.tabPage22.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tabPage22.Location = new System.Drawing.Point(0, 38);
            this.tabPage22.Name = "tabPage22";
            this.tabPage22.Size = new System.Drawing.Size(1328, 748);
            this.tabPage22.TabIndex = 6;
            this.tabPage22.Text = "MainShow";
            this.tabPage22.UseVisualStyleBackColor = true;
            //
            // plMainShow
            //
            this.plMainShow.AutoScroll = true;
            this.plMainShow.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.plMainShow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(229)))), ((int)(((byte)(237)))), ((int)(((byte)(244)))));
            this.plMainShow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.plMainShow.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.plMainShow.ForeColor = System.Drawing.Color.Black;
            this.plMainShow.Location = new System.Drawing.Point(0, 0);
            this.plMainShow.Margin = new System.Windows.Forms.Padding(0);
            this.plMainShow.Name = "plMainShow";
            this.plMainShow.Size = new System.Drawing.Size(1328, 600);
            this.plMainShow.TabIndex = 12;
            //
            // tabPage10
            //
            this.tabPage10.BackColor = System.Drawing.Color.White;
            this.tabPage10.Location = new System.Drawing.Point(0, 40);
            this.tabPage10.Name = "tabPage10";
            this.tabPage10.Size = new System.Drawing.Size(200, 60);
            this.tabPage10.TabIndex = 5;
            this.tabPage10.Text = "Vision";
            //
            // alarmCard
            //
            this.alarmCard.BackColor = System.Drawing.Color.White;
            this.alarmCard.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(231)))), ((int)(((byte)(240)))));
            this.alarmCard.Controls.Add(this.WarnningMessage);
            this.alarmCard.Controls.Add(this.panelAlarmFilter);
            this.alarmCard.CornerRadius = 14;
            this.alarmCard.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.alarmCard.FillColor = System.Drawing.Color.White;
            this.alarmCard.Location = new System.Drawing.Point(0, 600);
            this.alarmCard.Margin = new System.Windows.Forms.Padding(0);
            this.alarmCard.Name = "alarmCard";
            this.alarmCard.Padding = new System.Windows.Forms.Padding(16, 8, 16, 12);
            this.alarmCard.ShadowColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(176)))), ((int)(((byte)(196)))), ((int)(((byte)(218)))));
            this.alarmCard.ShowBorder = true;
            this.alarmCard.ShowShadow = true;
            this.alarmCard.Size = new System.Drawing.Size(1328, 148);
            this.alarmCard.TabIndex = 1;
            //
            // WarnningMessage
            //
            this.WarnningMessage.AllowDrop = true;
            this.WarnningMessage.BackColor = System.Drawing.Color.White;
            this.WarnningMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.WarnningMessage.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.WarnningMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.WarnningMessage.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.WarnningMessage.ForeColor = System.Drawing.Color.Black;
            this.WarnningMessage.FullRowSelect = true;
            this.WarnningMessage.HideSelection = false;
            this.WarnningMessage.Location = new System.Drawing.Point(12, 34);
            this.WarnningMessage.Margin = new System.Windows.Forms.Padding(0);
            this.WarnningMessage.Name = "WarnningMessage";
            this.WarnningMessage.RightToLeftLayout = true;
            this.WarnningMessage.ShowItemToolTips = true;
            this.WarnningMessage.Size = new System.Drawing.Size(1304, 106);
            this.WarnningMessage.TabIndex = 30;
            this.WarnningMessage.UseCompatibleStateImageBehavior = false;
            this.WarnningMessage.View = System.Windows.Forms.View.Details;
            //
            // columnHeader1
            //
            this.columnHeader1.Name = "columnHeader1";
            this.columnHeader1.Text = "时间";
            this.columnHeader1.Width = 171;
            //
            // columnHeader2
            //
            this.columnHeader2.Name = "columnHeader2";
            this.columnHeader2.Text = "类型";
            this.columnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader2.Width = 63;
            //
            // columnHeader3
            //
            this.columnHeader3.Name = "columnHeader3";
            this.columnHeader3.Text = "编号";
            this.columnHeader3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            //
            // columnHeader4
            //
            this.columnHeader4.Name = "columnHeader4";
            this.columnHeader4.Text = "内容";
            this.columnHeader4.Width = 1240;
            //
            // panelAlarmFilter
            //
            this.panelAlarmFilter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(243)))), ((int)(((byte)(249)))));
            this.panelAlarmFilter.Controls.Add(this.flowAlarmFilter);
            this.panelAlarmFilter.Controls.Add(this.lblAlarmTitle);
            this.panelAlarmFilter.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelAlarmFilter.Location = new System.Drawing.Point(12, 4);
            this.panelAlarmFilter.Name = "panelAlarmFilter";
            this.panelAlarmFilter.Size = new System.Drawing.Size(1304, 72);
            this.panelAlarmFilter.TabIndex = 32;
            //
            // flowAlarmFilter
            //
            this.flowAlarmFilter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(243)))), ((int)(((byte)(249)))));
            this.flowAlarmFilter.Controls.Add(this.btnAlarmFilterAll);
            this.flowAlarmFilter.Controls.Add(this.btnAlarmFilterWarn);
            this.flowAlarmFilter.Controls.Add(this.btnAlarmFilterAlarm);
            this.flowAlarmFilter.Controls.Add(this.btnAlarmQuery);
            this.flowAlarmFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowAlarmFilter.Location = new System.Drawing.Point(77, 0);
            this.flowAlarmFilter.Name = "flowAlarmFilter";
            this.flowAlarmFilter.Padding = new System.Windows.Forms.Padding(8, 5, 0, 5);
            this.flowAlarmFilter.Size = new System.Drawing.Size(1227, 30);
            this.flowAlarmFilter.TabIndex = 0;
            this.flowAlarmFilter.WrapContents = false;
            //
            // btnAlarmFilterAll
            //
            this.btnAlarmFilterAll.AccentColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(108)))), ((int)(((byte)(182)))));
            this.btnAlarmFilterAll.BackColor = System.Drawing.Color.Transparent;
            this.btnAlarmFilterAll.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.btnAlarmFilterAll.CornerRadius = 14;
            this.btnAlarmFilterAll.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAlarmFilterAll.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnAlarmFilterAll.Icon = null;
            this.btnAlarmFilterAll.IconSelected = null;
            this.btnAlarmFilterAll.InfoBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(242)))), ((int)(((byte)(246)))));
            this.btnAlarmFilterAll.InfoTextColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(72)))), ((int)(((byte)(88)))));
            this.btnAlarmFilterAll.Location = new System.Drawing.Point(6, 4);
            this.btnAlarmFilterAll.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.btnAlarmFilterAll.Name = "btnAlarmFilterAll_dyn";
            this.btnAlarmFilterAll.Selected = false;
            this.btnAlarmFilterAll.Size = new System.Drawing.Size(48, 28);
            this.btnAlarmFilterAll.Style = AlphaRap.AlarmChipStyle.Toggle;
            this.btnAlarmFilterAll.TabIndex = 0;
            this.btnAlarmFilterAll.TabStop = false;
            this.btnAlarmFilterAll.Text = "全部";
            this.btnAlarmFilterAll.TextColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            //
            // btnAlarmFilterWarn
            //
            this.btnAlarmFilterWarn.AccentColor = System.Drawing.Color.FromArgb(((int)(((byte)(219)))), ((int)(((byte)(149)))), ((int)(((byte)(44)))));
            this.btnAlarmFilterWarn.BackColor = System.Drawing.Color.Transparent;
            this.btnAlarmFilterWarn.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.btnAlarmFilterWarn.CornerRadius = 14;
            this.btnAlarmFilterWarn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAlarmFilterWarn.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnAlarmFilterWarn.Icon = null;
            this.btnAlarmFilterWarn.IconSelected = null;
            this.btnAlarmFilterWarn.InfoBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(242)))), ((int)(((byte)(246)))));
            this.btnAlarmFilterWarn.InfoTextColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(72)))), ((int)(((byte)(88)))));
            this.btnAlarmFilterWarn.Location = new System.Drawing.Point(60, 4);
            this.btnAlarmFilterWarn.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.btnAlarmFilterWarn.Name = "btnAlarmFilterWarn_dyn";
            this.btnAlarmFilterWarn.Selected = false;
            this.btnAlarmFilterWarn.Size = new System.Drawing.Size(48, 28);
            this.btnAlarmFilterWarn.Style = AlphaRap.AlarmChipStyle.Toggle;
            this.btnAlarmFilterWarn.TabIndex = 1;
            this.btnAlarmFilterWarn.TabStop = false;
            this.btnAlarmFilterWarn.Text = "警告";
            this.btnAlarmFilterWarn.TextColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            //
            // btnAlarmFilterAlarm
            //
            this.btnAlarmFilterAlarm.AccentColor = System.Drawing.Color.FromArgb(((int)(((byte)(214)))), ((int)(((byte)(69)))), ((int)(((byte)(69)))));
            this.btnAlarmFilterAlarm.BackColor = System.Drawing.Color.Transparent;
            this.btnAlarmFilterAlarm.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.btnAlarmFilterAlarm.CornerRadius = 14;
            this.btnAlarmFilterAlarm.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAlarmFilterAlarm.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnAlarmFilterAlarm.Icon = null;
            this.btnAlarmFilterAlarm.IconSelected = null;
            this.btnAlarmFilterAlarm.InfoBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(242)))), ((int)(((byte)(246)))));
            this.btnAlarmFilterAlarm.InfoTextColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(72)))), ((int)(((byte)(88)))));
            this.btnAlarmFilterAlarm.Location = new System.Drawing.Point(114, 4);
            this.btnAlarmFilterAlarm.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.btnAlarmFilterAlarm.Name = "btnAlarmFilterAlarm_dyn";
            this.btnAlarmFilterAlarm.Selected = false;
            this.btnAlarmFilterAlarm.Size = new System.Drawing.Size(48, 28);
            this.btnAlarmFilterAlarm.Style = AlphaRap.AlarmChipStyle.Toggle;
            this.btnAlarmFilterAlarm.TabIndex = 2;
            this.btnAlarmFilterAlarm.TabStop = false;
            this.btnAlarmFilterAlarm.Text = "报警";
            this.btnAlarmFilterAlarm.TextColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            //
            // btnAlarmQuery
            //
            this.btnAlarmQuery.AccentColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(108)))), ((int)(((byte)(126)))));
            this.btnAlarmQuery.BackColor = System.Drawing.Color.Transparent;
            this.btnAlarmQuery.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            this.btnAlarmQuery.CornerRadius = 14;
            this.btnAlarmQuery.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAlarmQuery.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnAlarmQuery.Icon = null;
            this.btnAlarmQuery.IconSelected = null;
            this.btnAlarmQuery.InfoBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(242)))), ((int)(((byte)(246)))));
            this.btnAlarmQuery.InfoTextColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(72)))), ((int)(((byte)(88)))));
            this.btnAlarmQuery.Location = new System.Drawing.Point(168, 4);
            this.btnAlarmQuery.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.btnAlarmQuery.Name = "btnAlarmQuery";
            this.btnAlarmQuery.Selected = false;
            this.btnAlarmQuery.Size = new System.Drawing.Size(48, 28);
            this.btnAlarmQuery.Style = AlphaRap.AlarmChipStyle.Toggle;
            this.btnAlarmQuery.TabIndex = 3;
            this.btnAlarmQuery.TabStop = false;
            this.btnAlarmQuery.Text = "查询";
            this.btnAlarmQuery.TextColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(62)))), ((int)(((byte)(80)))));
            //
            // lblAlarmTitle
            //
            this.lblAlarmTitle.AutoSize = false;
            this.lblAlarmTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblAlarmTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblAlarmTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(36)))), ((int)(((byte)(59)))));
            this.lblAlarmTitle.Location = new System.Drawing.Point(0, 0);
            this.lblAlarmTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblAlarmTitle.Name = "lblAlarmTitle";
            this.lblAlarmTitle.Padding = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.lblAlarmTitle.Size = new System.Drawing.Size(92, 38);
            this.lblAlarmTitle.TabIndex = 1;
            this.lblAlarmTitle.Text = "报警信息";
            //
            // panel1
            //
            this.panel1.BackColor = System.Drawing.Color.FromArgb(6, 104, 166);
            this.panel1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(231)))), ((int)(((byte)(240)))));
            this.panel1.Controls.Add(this.tableLayoutPanel2);
            this.panel1.CornerRadius = 0;
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.FillColor = System.Drawing.Color.FromArgb(6, 104, 166);
            this.panel1.Location = new System.Drawing.Point(248, 916);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(16, 0, 12, 0);
            this.panel1.ShadowColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(176)))), ((int)(((byte)(196)))), ((int)(((byte)(218)))));
            this.panel1.ShowBorder = false;
            this.panel1.ShowShadow = false;
            this.panel1.Size = new System.Drawing.Size(1328, 68);
            this.panel1.TabIndex = 27;
            //
            // tableLayoutPanel2
            //
            this.tableLayoutPanel2.BackColor = System.Drawing.Color.FromArgb(6, 104, 166);
            this.tableLayoutPanel2.ColumnCount = 5;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 24F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 78F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.MachineStatus, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.recipeDetails, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.picVirtualKeyboard, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.languageSwitch, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.statusStrip2, 4, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(16, 5);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1300, 58);
            this.tableLayoutPanel2.TabIndex = 0;
            //
            // MachineStatus
            //
            this.MachineStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MachineStatus.AutoSize = false;
            this.MachineStatus.BackColor = System.Drawing.Color.FromArgb(10, 112, 172);
            this.MachineStatus.Cursor = System.Windows.Forms.Cursors.Hand;
            this.MachineStatus.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.MachineStatus.ForeColor = System.Drawing.Color.White;
            this.MachineStatus.Location = new System.Drawing.Point(0, 7);
            this.MachineStatus.Margin = new System.Windows.Forms.Padding(0, 8, 12, 8);
            this.MachineStatus.Name = "MachineStatus";
            this.MachineStatus.Padding = new System.Windows.Forms.Padding(24, 0, 8, 0);
            this.MachineStatus.Size = new System.Drawing.Size(192, 44);
            this.MachineStatus.TabIndex = 211;
            this.MachineStatus.Text = "IDLE";
            this.MachineStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MachineStatus.Click += new System.EventHandler(this.DashboardAlarm_Click);
            //
            // MENU_Product
            //
            this.MENU_Product.BackColor = System.Drawing.Color.Transparent;
            this.MENU_Product.Location = new System.Drawing.Point(12, 44);
            this.MENU_Product.Margin = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.MENU_Product.Name = "MENU_Product";
            this.MENU_Product.Size = new System.Drawing.Size(200, 38);
            this.MENU_Product.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.MENU_Product.TabIndex = 3;
            this.MENU_Product.TabStop = false;
            this.MENU_Product.Tag = "Product";
            this.MENU_Product.Click += new System.EventHandler(this.MENU_Click);
            this.MENU_Product.MouseEnter += new System.EventHandler(this.MouseEnter1);
            //
            // lbRecipeName
            //
            this.recipeDetails.BackColor = System.Drawing.Color.Transparent;
            this.recipeDetails.ColumnCount = 2;
            this.recipeDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize));
            this.recipeDetails.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.recipeDetails.RowCount = 1;
            this.recipeDetails.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.recipeDetails.Controls.Add(this.lblRecipeCaption, 0, 0);
            this.recipeDetails.Controls.Add(this.lbRecipeName, 1, 0);
            this.recipeDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.recipeDetails.Margin = new System.Windows.Forms.Padding(0);
            this.recipeDetails.Padding = new System.Windows.Forms.Padding(16, 0, 8, 0);
            this.recipeDetails.Name = "recipeDetails";
            this.lblRecipeCaption.AutoSize = true;
            this.lblRecipeCaption.BackColor = System.Drawing.Color.Transparent;
            this.lblRecipeCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRecipeCaption.Font = new System.Drawing.Font("Microsoft YaHei UI", 8F);
            this.lblRecipeCaption.ForeColor = System.Drawing.Color.FromArgb(218, 239, 253);
            this.lblRecipeCaption.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.lblRecipeCaption.Name = "lblRecipeCaption";
            this.lblRecipeCaption.Text = "当前配方";
            this.lblRecipeCaption.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lbRecipeName.AutoEllipsis = true;
            this.lbRecipeName.BackColor = System.Drawing.Color.Transparent;
            this.lbRecipeName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbRecipeName.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lbRecipeName.ForeColor = System.Drawing.Color.White;
            this.lbRecipeName.Margin = new System.Windows.Forms.Padding(0);
            this.lbRecipeName.Name = "lbRecipeName";
            this.lbRecipeName.TabIndex = 0;
            this.lbRecipeName.Text = "AllModel";
            this.lbRecipeName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // picVirtualKeyboard
            //
            this.picVirtualKeyboard.BackColor = System.Drawing.Color.FromArgb(6, 104, 166);
            this.picVirtualKeyboard.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picVirtualKeyboard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picVirtualKeyboard.Location = new System.Drawing.Point(951, 4);
            this.picVirtualKeyboard.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
            this.picVirtualKeyboard.Name = "picVirtualKeyboard";
            this.picVirtualKeyboard.Size = new System.Drawing.Size(30, 50);
            this.picVirtualKeyboard.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picVirtualKeyboard.TabIndex = 222;
            this.picVirtualKeyboard.TabStop = false;
            //
            // languageSwitch
            //
            this.languageSwitch.BackColor = System.Drawing.Color.FromArgb(6, 104, 166);
            this.languageSwitch.Compact = true;
            this.languageSwitch.Current = AlphaRap.LanguageType.Chinese;
            this.languageSwitch.Cursor = System.Windows.Forms.Cursors.Hand;
            this.languageSwitch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.languageSwitch.Font = new System.Drawing.Font("微软雅黑", 8F, System.Drawing.FontStyle.Regular);
            this.languageSwitch.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(83)))), ((int)(((byte)(102)))), ((int)(((byte)(125)))));
            this.languageSwitch.Location = new System.Drawing.Point(983, 0);
            this.languageSwitch.Margin = new System.Windows.Forms.Padding(0, 8, 8, 8);
            this.languageSwitch.Name = "languageSwitch";
            this.languageSwitch.Size = new System.Drawing.Size(80, 58);
            this.languageSwitch.TabIndex = 221;
            this.languageSwitch.TabStop = false;
            //
            // statusStrip2
            //
            this.statusStrip2.BackColor = System.Drawing.Color.FromArgb(6, 104, 166);
            this.statusStrip2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.statusStrip2.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.statusStrip2.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.footerTimeCaption,
            this.toolStripStatusLabel5,
            this.footerVersionCaption,
            this.toolStripStatusLabel3,
            this.toolStripStatusLabel4});
            this.statusStrip2.Location = new System.Drawing.Point(1063, 0);
            this.statusStrip2.Name = "statusStrip2";
            this.statusStrip2.Padding = new System.Windows.Forms.Padding(1, 0, 12, 0);
            this.statusStrip2.Size = new System.Drawing.Size(237, 58);
            this.statusStrip2.SizingGrip = false;
            this.statusStrip2.TabIndex = 27;
            this.statusStrip2.Text = "statusStrip2";
            //
            // toolStripStatusLabel5
            //
            this.footerTimeCaption.Name = "footerTimeCaption_dyn";
            this.footerTimeCaption.Text = "时间";
            this.footerVersionCaption.Name = "footerVersionCaption_dyn";
            this.footerVersionCaption.Text = "Version";
            this.toolStripStatusLabel5.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.None;
            this.toolStripStatusLabel5.AutoSize = false;
            this.toolStripStatusLabel5.ForeColor = System.Drawing.Color.White;
            this.toolStripStatusLabel5.Name = "toolStripStatusLabel5_dyn";
            this.toolStripStatusLabel5.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.toolStripStatusLabel5.Size = new System.Drawing.Size(154, 40);
            this.toolStripStatusLabel5.Text = "0000-00-00 00:00:00";
            //
            // toolStripStatusLabel3
            //
            this.toolStripStatusLabel3.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Left;
            this.toolStripStatusLabel3.ForeColor = System.Drawing.Color.White;
            this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            this.toolStripStatusLabel3.Size = new System.Drawing.Size(100, 53);
            this.toolStripStatusLabel3.Text = "AlphaRap";
            this.toolStripStatusLabel3.Visible = false;
            //
            // toolStripStatusLabel4
            //
            this.toolStripStatusLabel4.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.None;
            this.toolStripStatusLabel4.BorderStyle = System.Windows.Forms.Border3DStyle.Bump;
            this.toolStripStatusLabel4.ForeColor = System.Drawing.Color.White;
            this.toolStripStatusLabel4.Name = "toolStripStatusLabel4_dyn";
            this.toolStripStatusLabel4.Size = new System.Drawing.Size(87, 53);
            this.toolStripStatusLabel4.Text = "2.0.8";
            //
            // MainForm
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(243)))), ((int)(((byte)(248)))));
            this.CausesValidation = false;
            this.ClientSize = new System.Drawing.Size(1600, 1000);
            this.ControlBox = false;
            this.Controls.Add(this.tableLayoutPanel_Main);
            this.Font = new System.Drawing.Font("微软雅黑", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(36)))), ((int)(((byte)(59)))));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.miniToolStrip;
            this.Margin = new System.Windows.Forms.Padding(5);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(1280, 800);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Alpha";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.SettingData)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.RecipeData)).EndInit();
            this.tableLayoutPanel_Main.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panelBrand.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.Panl_PictureShowTop.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MENU_Home)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MENU_Vision)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MENU_Save)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MENU_Rapid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MENU_Hard)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MENU_Manual)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MENU_LifeSpan)).EndInit();
            this.Panl_PictureShowDown.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MENU_Mes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MENU_AddUser)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MENU_Data)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MENU_Check)).EndInit();
            this.panelLogin.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MENU_Login)).EndInit();
            this.panel_PictureShowDown.ResumeLayout(false);
            this.panel_PictureShowLeft.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MENU_Reset)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MENU_Run)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MENU_Pause)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MENU_Stop)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AlarmReset)).EndInit();
            this.panel_PictureShowRight.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MENU_Exit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MENU_System)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MENU_Lock)).EndInit();
            this.panel_Second.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.uiTabControl1.ResumeLayout(false);
            this.tabPage22.ResumeLayout(false);
            this.alarmCard.ResumeLayout(false);
            this.panelAlarmFilter.ResumeLayout(false);
            this.panelAlarmFilter.PerformLayout();
            this.flowAlarmFilter.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MENU_Product)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picVirtualKeyboard)).EndInit();
            this.statusStrip2.ResumeLayout(false);
            this.statusStrip2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panelBrand;
        private System.Windows.Forms.Label lblBrandName;
        private System.Windows.Forms.Label lblBrandCaption;
        private System.Windows.Forms.Label lblMonitorGroup;
        private System.Windows.Forms.Label lblSystemGroup;
        private AlphaRap.CardPanel alarmCard;
        private System.Windows.Forms.Panel dashboardToolbar;
        private System.Windows.Forms.PictureBox MENU_RuntimeLog;
        private System.Windows.Forms.Label dashboardHomeTab;
        private System.Windows.Forms.Label dashboardVisionTab;
        private System.Windows.Forms.Label lblAlarmTitle;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Timer LoginOutTime;

		public System.Windows.Forms.TextBox txtCyCT;

		private System.Windows.Forms.MenuStrip miniToolStrip;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_Main;
		private System.Windows.Forms.StatusStrip statusStrip2;
		public System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;
		public System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel4;
		private AlphaRap.StatusClockLabel toolStripStatusLabel5;
        private System.Windows.Forms.ToolStripStatusLabel footerTimeCaption;
        private System.Windows.Forms.ToolStripStatusLabel footerVersionCaption;
		private System.Windows.Forms.Panel panel_PictureShowDown;
		private System.Windows.Forms.FlowLayoutPanel panel_PictureShowRight;
		private System.Windows.Forms.PictureBox MENU_Exit;
		private System.Windows.Forms.FlowLayoutPanel panel_PictureShowLeft;
		private System.Windows.Forms.PictureBox MENU_Stop;
		public System.Windows.Forms.PictureBox MENU_Run;
		public System.Windows.Forms.PictureBox MENU_Pause;
		public System.Windows.Forms.PictureBox MENU_Reset;
		private AlphaRap.CardPanel panel1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private System.Windows.Forms.Label MachineStatus;
		private System.Windows.Forms.PictureBox MENU_Login;
		private AlphaRap.CardPanel panelLogin;
		private System.Windows.Forms.MenuStrip menuStrip1;
		public System.Windows.Forms.ToolStripMenuItem NumC1;
		public System.Windows.Forms.ToolStripMenuItem NumC2;
		public System.Windows.Forms.ToolStripMenuItem NumC3;
		public System.Windows.Forms.ToolStripMenuItem NumC4;
		private System.Windows.Forms.Label LoginText;
		public System.Windows.Forms.Label lbRecipeName;
        private System.Windows.Forms.TableLayoutPanel recipeDetails;
        private System.Windows.Forms.Label lblRecipeCaption;
		private System.Windows.Forms.PictureBox MENU_Product;
		private System.Windows.Forms.PictureBox picLogo;
		private System.Windows.Forms.PictureBox picVirtualKeyboard;
	private AlphaRap.LanguageSwitch languageSwitch;
		private System.Windows.Forms.Panel panel_Second;
		private System.Windows.Forms.PictureBox MENU_Lock;
		private System.Windows.Forms.PictureBox MENU_System;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private AlphaRap.GradientTableLayoutPanel panel4;
		private System.Windows.Forms.FlowLayoutPanel Panl_PictureShowDown;
		private System.Windows.Forms.PictureBox MENU_LifeSpan;
		private System.Windows.Forms.PictureBox MENU_Mes;
		private System.Windows.Forms.PictureBox MENU_AddUser;
		private System.Windows.Forms.PictureBox MENU_Data;
		private System.Windows.Forms.PictureBox MENU_Check;
		private System.Windows.Forms.FlowLayoutPanel Panl_PictureShowTop;
		private System.Windows.Forms.PictureBox MENU_Home;
		private System.Windows.Forms.PictureBox MENU_Vision;
		public System.Windows.Forms.PictureBox MENU_Save;
		private System.Windows.Forms.PictureBox MENU_Rapid;
		private System.Windows.Forms.PictureBox MENU_Hard;
		private System.Windows.Forms.PictureBox MENU_Manual;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
		public System.Windows.Forms.ListView WarnningMessage;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.Panel panel5;
		private System.Windows.Forms.Panel panelAlarmFilter;
		private System.Windows.Forms.FlowLayoutPanel flowAlarmFilter;
		private AlphaRap.AlarmChip btnAlarmFilterAll;
		private AlphaRap.AlarmChip btnAlarmFilterWarn;
		private AlphaRap.AlarmChip btnAlarmFilterAlarm;
		private AlphaRap.AlarmChip btnAlarmQuery;
		public Sunny.UI.UITabControl uiTabControl1;
		private System.Windows.Forms.TabPage tabPage22;
		public System.Windows.Forms.Panel plMainShow;
		public System.Windows.Forms.TabPage tabPage10;
		private System.Windows.Forms.PictureBox AlarmReset;
    }
}
