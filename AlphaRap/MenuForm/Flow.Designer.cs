namespace AlphaRap
{
    partial class Flow
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.flowChartTab_0 = new System.Windows.Forms.TabPage();
            this.flowChartTab_1 = new System.Windows.Forms.TabPage();
            this.flowChartTab_2 = new System.Windows.Forms.TabPage();
            this.tabControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 设计器里放占位站位页，字号/高度与 UiTheme 一级 Tab 一致；
            // 运行时 AddFlowChart 会 Clear 后按真实模块重建，再 StyleTabControl(OwnerDraw)。
            // 
            this.tabControl1.Controls.Add(this.flowChartTab_0);
            this.tabControl1.Controls.Add(this.flowChartTab_1);
            this.tabControl1.Controls.Add(this.flowChartTab_2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.DrawMode = System.Windows.Forms.TabDrawMode.Normal;
            this.tabControl1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tabControl1.ItemSize = new System.Drawing.Size(148, 40);
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.Padding = new System.Drawing.Point(16, 6);
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1280, 920);
            this.tabControl1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabControl1.TabIndex = 1;
            // 
            // flowChartTab_0
            // 
            this.flowChartTab_0.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(243)))), ((int)(((byte)(248)))));
            this.flowChartTab_0.Location = new System.Drawing.Point(4, 44);
            this.flowChartTab_0.Name = "flowChartTab_0";
            this.flowChartTab_0.Padding = new System.Windows.Forms.Padding(4);
            this.flowChartTab_0.Size = new System.Drawing.Size(1272, 872);
            this.flowChartTab_0.TabIndex = 0;
            this.flowChartTab_0.Text = "Gantry";
            // 
            // flowChartTab_1
            // 
            this.flowChartTab_1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(243)))), ((int)(((byte)(248)))));
            this.flowChartTab_1.Location = new System.Drawing.Point(4, 44);
            this.flowChartTab_1.Name = "flowChartTab_1";
            this.flowChartTab_1.Padding = new System.Windows.Forms.Padding(4);
            this.flowChartTab_1.Size = new System.Drawing.Size(1272, 872);
            this.flowChartTab_1.TabIndex = 1;
            this.flowChartTab_1.Text = "UpConveryor";
            // 
            // flowChartTab_2
            // 
            this.flowChartTab_2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(243)))), ((int)(((byte)(248)))));
            this.flowChartTab_2.Location = new System.Drawing.Point(4, 44);
            this.flowChartTab_2.Name = "flowChartTab_2";
            this.flowChartTab_2.Padding = new System.Windows.Forms.Padding(4);
            this.flowChartTab_2.Size = new System.Drawing.Size(1272, 872);
            this.flowChartTab_2.TabIndex = 2;
            this.flowChartTab_2.Text = "DownConveryor";
            // 
            // Flow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoScroll = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(243)))), ((int)(((byte)(248)))));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(1280, 920);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Flow";
            this.Text = "Flow";
            this.Load += new System.EventHandler(this.Flow_Load);
            this.tabControl1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage flowChartTab_0;
        private System.Windows.Forms.TabPage flowChartTab_1;
        private System.Windows.Forms.TabPage flowChartTab_2;
    }
}