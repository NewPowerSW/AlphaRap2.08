namespace AlphaRap
{
    partial class HardForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HardForm));
            NPSDK.Motion.PositionColumn positionColumn1 = new NPSDK.Motion.PositionColumn();
            this.MTR_H1_X = new NPSDK.Motion.NP_Motor();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.dataTable1 = new System.Data.DataTable();
            this.tb_H1_Srew1Post = new System.Data.DataTable();
            this.dataColumn1 = new System.Data.DataColumn();
            this.dataColumn8 = new System.Data.DataColumn();
            this.dataTable2 = new System.Data.DataTable();
            this.dataColumn29 = new System.Data.DataColumn();
            this.X1_Group = new System.Windows.Forms.GroupBox();
            this.nP_MotorJog2 = new NPSDK.Motion.NP_MotorJog();
            this.nP_JogOption1 = new NPSDK.Motion.NP_JogOption();
            this.nP_MotorJog1 = new NPSDK.Motion.NP_MotorJog();
            this.X1_Home = new NPSDK.Motion.NP_Home();
            this.X1_PositionTable1 = new NPSDK.Motion.NP_PositionTable();
            this.nP_PositionShow1 = new NPSDK.Motion.NP_PositionShow();
            this.nP_ManualControl1 = new NPSDK.Motion.NP_ManualControl();
            ((System.ComponentModel.ISupportInitialize)(this.SettingData)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.RecipeData)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataTable1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tb_H1_Srew1Post)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataTable2)).BeginInit();
            this.X1_Group.SuspendLayout();
            this.SuspendLayout();
            // 
            // SettingData
            // 
            this.SettingData.Tables.AddRange(new System.Data.DataTable[] {
            this.dataTable2});
            // 
            // RecipeData
            // 
            this.RecipeData.Tables.AddRange(new System.Data.DataTable[] {
            this.dataTable1,
            this.tb_H1_Srew1Post});
            // 
            // MTR_H1_X
            // 
            this.MTR_H1_X.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.MTR_H1_X.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.MTR_H1_X.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.MTR_H1_X.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MTR_H1_X.Location = new System.Drawing.Point(26, 22);
            this.MTR_H1_X.Margin = new System.Windows.Forms.Padding(1);
            this.MTR_H1_X.MaximumSize = new System.Drawing.Size(500, 40);
            this.MTR_H1_X.Name = "MTR_H1_X";
            this.MTR_H1_X.Port = "0";
            this.MTR_H1_X.Size = new System.Drawing.Size(109, 28);
            this.MTR_H1_X.TabIndex = 6;
            this.MTR_H1_X.Text = "XAxis";
            this.MTR_H1_X.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.MTR_H1_X.Vendor = NPSDK.Motion.NPVendor.Leadshine;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "42.png");
            this.imageList1.Images.SetKeyName(1, "43.png");
            this.imageList1.Images.SetKeyName(2, "44.png");
            this.imageList1.Images.SetKeyName(3, "45.png");
            this.imageList1.Images.SetKeyName(4, "44.jpg");
            this.imageList1.Images.SetKeyName(5, "48.png");
            this.imageList1.Images.SetKeyName(6, "49.png");
            this.imageList1.Images.SetKeyName(7, "61.png");
            this.imageList1.Images.SetKeyName(8, "63.png");
            // 
            // dataTable1
            // 
            this.dataTable1.TableName = "RSet";
            // 
            // tb_H1_Srew1Post
            // 
            this.tb_H1_Srew1Post.Columns.AddRange(new System.Data.DataColumn[] {
            this.dataColumn1,
            this.dataColumn8});
            this.tb_H1_Srew1Post.TableName = "tb_X1_Post";
            // 
            // dataColumn1
            // 
            this.dataColumn1.ColumnName = "X1";
            this.dataColumn1.DataType = typeof(double);
            // 
            // dataColumn8
            // 
            this.dataColumn8.ColumnName = "Annotation";
            // 
            // dataTable2
            // 
            this.dataTable2.Columns.AddRange(new System.Data.DataColumn[] {
            this.dataColumn29});
            this.dataTable2.TableName = "Mset";
            // 
            // dataColumn29
            // 
            this.dataColumn29.ColumnName = "AxisSpeed";
            this.dataColumn29.DefaultValue = "100";
            // 
            // X1_Group
            // 
            this.X1_Group.Controls.Add(this.nP_MotorJog2);
            this.X1_Group.Controls.Add(this.nP_JogOption1);
            this.X1_Group.Controls.Add(this.nP_MotorJog1);
            this.X1_Group.Controls.Add(this.X1_Home);
            this.X1_Group.Controls.Add(this.X1_PositionTable1);
            this.X1_Group.Controls.Add(this.nP_PositionShow1);
            this.X1_Group.Controls.Add(this.MTR_H1_X);
            this.X1_Group.Location = new System.Drawing.Point(53, 94);
            this.X1_Group.Name = "X1_Group";
            this.X1_Group.Size = new System.Drawing.Size(585, 491);
            this.X1_Group.TabIndex = 7;
            this.X1_Group.TabStop = false;
            this.X1_Group.Text = "X1";
            // 
            // nP_MotorJog2
            // 
            this.nP_MotorJog2.BackColor = System.Drawing.Color.Transparent;
            this.nP_MotorJog2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("nP_MotorJog2.BackgroundImage")));
            this.nP_MotorJog2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.nP_MotorJog2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.nP_MotorJog2.Direction = NPSDK.Motion.NP_MotorJog.DirectionType.Right;
            this.nP_MotorJog2.FlatAppearance.BorderSize = 0;
            this.nP_MotorJog2.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.nP_MotorJog2.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.nP_MotorJog2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.nP_MotorJog2.JogDirection = NPSDK.Motion.NP_MotorJog.JogDirectionType.JogN;
            this.nP_MotorJog2.JogOption = this.nP_JogOption1;
            this.nP_MotorJog2.Location = new System.Drawing.Point(312, 400);
            this.nP_MotorJog2.Name = "nP_MotorJog2";
            this.nP_MotorJog2.RelativeDistance = 0D;
            this.nP_MotorJog2.Size = new System.Drawing.Size(60, 60);
            this.nP_MotorJog2.SpeedRatio = 10D;
            this.nP_MotorJog2.TabIndex = 12;
            this.nP_MotorJog2.UseVisualStyleBackColor = false;
            this.nP_MotorJog2.WorkMotor = this.MTR_H1_X;
            // 
            // nP_JogOption1
            // 
            this.nP_JogOption1.BackColor = System.Drawing.SystemColors.Control;
            this.nP_JogOption1.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nP_JogOption1.Location = new System.Drawing.Point(92, 416);
            this.nP_JogOption1.Name = "nP_JogOption1";
            this.nP_JogOption1.Size = new System.Drawing.Size(280, 32);
            this.nP_JogOption1.TabIndex = 11;
            this.nP_JogOption1.WorkMotor = this.MTR_H1_X;
            // 
            // nP_MotorJog1
            // 
            this.nP_MotorJog1.BackColor = System.Drawing.Color.Transparent;
            this.nP_MotorJog1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("nP_MotorJog1.BackgroundImage")));
            this.nP_MotorJog1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.nP_MotorJog1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.nP_MotorJog1.Direction = NPSDK.Motion.NP_MotorJog.DirectionType.Left;
            this.nP_MotorJog1.FlatAppearance.BorderSize = 0;
            this.nP_MotorJog1.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.nP_MotorJog1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.nP_MotorJog1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.nP_MotorJog1.JogOption = this.nP_JogOption1;
            this.nP_MotorJog1.Location = new System.Drawing.Point(26, 400);
            this.nP_MotorJog1.Name = "nP_MotorJog1";
            this.nP_MotorJog1.RelativeDistance = 0D;
            this.nP_MotorJog1.Size = new System.Drawing.Size(60, 60);
            this.nP_MotorJog1.SpeedRatio = 10D;
            this.nP_MotorJog1.TabIndex = 10;
            this.nP_MotorJog1.UseVisualStyleBackColor = false;
            this.nP_MotorJog1.WorkMotor = this.MTR_H1_X;
            // 
            // X1_Home
            // 
            this.X1_Home.Location = new System.Drawing.Point(26, 70);
            this.X1_Home.Name = "X1_Home";
            this.X1_Home.Size = new System.Drawing.Size(109, 28);
            this.X1_Home.TabIndex = 9;
            this.X1_Home.Text = "X1 Home";
            this.X1_Home.UseVisualStyleBackColor = true;
            this.X1_Home.WorkMotor = this.MTR_H1_X;
            // 
            // X1_PositionTable1
            // 
            this.X1_PositionTable1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.X1_PositionTable1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.X1_PositionTable1.DataSet = this.RecipeData;
            this.X1_PositionTable1.DataSet1 = this.tb_H1_Srew1Post;
            this.X1_PositionTable1.Font = new System.Drawing.Font("Arial", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.X1_PositionTable1.Location = new System.Drawing.Point(26, 102);
            this.X1_PositionTable1.Margin = new System.Windows.Forms.Padding(1);
            this.X1_PositionTable1.Name = "X1_PositionTable1";
            positionColumn1.Axis = this.MTR_H1_X;
            positionColumn1.Name = "X1";
            this.X1_PositionTable1.ParameterSet.Add(positionColumn1);
            this.X1_PositionTable1.Size = new System.Drawing.Size(521, 294);
            this.X1_PositionTable1.TabIndex = 8;
            this.X1_PositionTable1.WorkMotor = null;
            // 
            // nP_PositionShow1
            // 
            this.nP_PositionShow1.Location = new System.Drawing.Point(160, 22);
            this.nP_PositionShow1.Name = "nP_PositionShow1";
            this.nP_PositionShow1.ShowEncoder = true;
            this.nP_PositionShow1.Size = new System.Drawing.Size(182, 63);
            this.nP_PositionShow1.TabIndex = 7;
            this.nP_PositionShow1.WorkMotor = this.MTR_H1_X;
            // 
            // nP_ManualControl1
            // 
            this.nP_ManualControl1.BackColor = System.Drawing.Color.White;
            this.nP_ManualControl1.DefaultSpeed = 30D;
            this.nP_ManualControl1.Location = new System.Drawing.Point(669, 116);
            this.nP_ManualControl1.MaximumJogSpeed = 100D;
            this.nP_ManualControl1.Name = "nP_ManualControl1";
            this.nP_ManualControl1.Size = new System.Drawing.Size(134, 48);
            this.nP_ManualControl1.TabIndex = 13;
            this.nP_ManualControl1.Text = "nP_ManualControl1";
            this.nP_ManualControl1.UseVisualStyleBackColor = false;
            this.nP_ManualControl1.WorkMotor = null;
            // 
            // HardForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(1284, 735);
            this.resourceToolbar = new System.Windows.Forms.FlowLayoutPanel { Name = "resourceToolbar", Dock = System.Windows.Forms.DockStyle.Top, Height = 42 };
            this.legacyResources = new System.Windows.Forms.Button { Name = "legacyResources", Text = "原有轴页面", AutoSize = true };
            this.configuredResources = new System.Windows.Forms.Button { Name = "configuredResources", Text = "轴资源", AutoSize = true };
            this.resourceToolbar.Controls.Add(this.legacyResources);
            this.resourceToolbar.Controls.Add(this.configuredResources);
            this.resourcePanel = new AlphaRap.Configuration.HardwareResourcePanel { Name = "hardwareResourcePanel", Dock = System.Windows.Forms.DockStyle.Fill, Visible = false };
            this.Controls.Add(this.resourcePanel);
            this.Controls.Add(this.resourceToolbar);
            this.Controls.Add(this.nP_ManualControl1);
            this.Controls.Add(this.X1_Group);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
            this.Name = "HardForm";
            this.Text = "HardForm";
            ((System.ComponentModel.ISupportInitialize)(this.SettingData)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.RecipeData)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataTable1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tb_H1_Srew1Post)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataTable2)).EndInit();
            this.X1_Group.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Timer timer1;
        private System.Data.DataTable dataTable1;
        private System.Data.DataTable tb_H1_Srew1Post;
        private System.Data.DataColumn dataColumn1;
        private System.Data.DataColumn dataColumn8;
		private System.Data.DataTable dataTable2;
        public System.Data.DataColumn dataColumn29;
        private System.Windows.Forms.FlowLayoutPanel resourceToolbar;
        private System.Windows.Forms.Button legacyResources, configuredResources;
        private AlphaRap.Configuration.HardwareResourcePanel resourcePanel;
        public NPSDK.Motion.NP_Motor MTR_H1_X;
        private System.Windows.Forms.GroupBox X1_Group;
        private NPSDK.Motion.NP_JogOption nP_JogOption1;
        private NPSDK.Motion.NP_MotorJog nP_MotorJog1;
        private NPSDK.Motion.NP_Home X1_Home;
        private NPSDK.Motion.NP_PositionTable X1_PositionTable1;
        private NPSDK.Motion.NP_PositionShow nP_PositionShow1;
        private NPSDK.Motion.NP_MotorJog nP_MotorJog2;
        private NPSDK.Motion.NP_ManualControl nP_ManualControl1;
    }
}
