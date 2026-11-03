namespace AlphaRap
{
    partial class HardForm
    {
        private System.Data.DataTable dataTable1;
        private System.Data.DataTable tb_H1_Srew1Post;
        private System.Data.DataColumn dataColumn1, dataColumn8;
        private System.Data.DataTable dataTable2;
        public System.Data.DataColumn dataColumn29;
        private System.Windows.Forms.FlowLayoutPanel manualToolbar;
        private NPSDK.Motion.NP_ManualControl nP_ManualControl1;
        private AlphaRap.Configuration.HardwareResourcePanel hardwareResourcePanel;

        /// <summary>固定显示全轴调试入口及动态轴管理页，保留历史配置的非可视表结构。</summary>
        private void InitializeComponent()
        {
            this.dataTable1 = new System.Data.DataTable();
            this.tb_H1_Srew1Post = new System.Data.DataTable();
            this.dataColumn1 = new System.Data.DataColumn();
            this.dataColumn8 = new System.Data.DataColumn();
            this.dataTable2 = new System.Data.DataTable();
            this.dataColumn29 = new System.Data.DataColumn();
            this.manualToolbar = new System.Windows.Forms.FlowLayoutPanel();
            this.nP_ManualControl1 = new NPSDK.Motion.NP_ManualControl();
            this.hardwareResourcePanel = new AlphaRap.Configuration.HardwareResourcePanel();
            ((System.ComponentModel.ISupportInitialize)(this.SettingData)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.RecipeData)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataTable1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tb_H1_Srew1Post)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataTable2)).BeginInit();
            this.manualToolbar.SuspendLayout();
            this.SuspendLayout();
            this.SettingData.Tables.AddRange(new System.Data.DataTable[] { this.dataTable2 });
            this.RecipeData.Tables.AddRange(new System.Data.DataTable[] { this.dataTable1, this.tb_H1_Srew1Post });
            this.dataTable1.TableName = "RSet";
            this.tb_H1_Srew1Post.Columns.AddRange(new System.Data.DataColumn[] { this.dataColumn1, this.dataColumn8 });
            this.tb_H1_Srew1Post.TableName = "tb_X1_Post";
            this.dataColumn1.ColumnName = "X1";
            this.dataColumn1.DataType = typeof(double);
            this.dataColumn8.ColumnName = "Annotation";
            this.dataTable2.Columns.AddRange(new System.Data.DataColumn[] { this.dataColumn29 });
            this.dataTable2.TableName = "Mset";
            this.dataColumn29.ColumnName = "AxisSpeed";
            this.dataColumn29.DefaultValue = "100";
            this.manualToolbar.Name = "manualToolbar";
            this.manualToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.manualToolbar.Height = 64;
            this.manualToolbar.Padding = new System.Windows.Forms.Padding(5);
            this.manualToolbar.WrapContents = false;
            this.nP_ManualControl1.Name = "nP_ManualControl1";
            this.nP_ManualControl1.Text = "手动调试";
            this.nP_ManualControl1.Size = new System.Drawing.Size(134, 48);
            this.nP_ManualControl1.Margin = new System.Windows.Forms.Padding(3);
            this.nP_ManualControl1.DefaultSpeed = 30D;
            this.nP_ManualControl1.MaximumJogSpeed = 100D;
            this.nP_ManualControl1.WorkMotor = null;
            this.manualToolbar.Controls.Add(this.nP_ManualControl1);
            this.hardwareResourcePanel.Name = "hardwareResourcePanel";
            this.hardwareResourcePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hardwareResourcePanel.ResourceKind = "Axis";
            this.Controls.Add(this.hardwareResourcePanel);
            this.Controls.Add(this.manualToolbar);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(1284, 735);
            this.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(6, 7, 6, 7);
            this.Name = "HardForm";
            this.Text = "HardForm";
            ((System.ComponentModel.ISupportInitialize)(this.SettingData)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.RecipeData)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataTable1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tb_H1_Srew1Post)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataTable2)).EndInit();
            this.manualToolbar.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
