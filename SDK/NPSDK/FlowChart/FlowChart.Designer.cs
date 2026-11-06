namespace NPSDK
{
    partial class Flow_Chart
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing) Flow_Module.FlowChart_List.Remove(this);
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Flow_Chart));
			this.button1 = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
            this.elapsedTime = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.BackColor = System.Drawing.SystemColors.ButtonHighlight;
			resources.ApplyResources(this.button1, "button1");
			this.button1.Name = "button1";
			this.button1.Padding = new System.Windows.Forms.Padding(0, 14, 0, 0);
			this.button1.UseVisualStyleBackColor = false;
			//this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// label1
			// 
			resources.ApplyResources(this.label1, "label1");
			this.label1.ForeColor = System.Drawing.Color.Black;
			this.label1.Name = "label1";
            this.elapsedTime.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.elapsedTime.AutoSize = false;
            this.elapsedTime.Location = new System.Drawing.Point(77, 3);
            this.elapsedTime.Size = new System.Drawing.Size(68, 15);
            this.elapsedTime.Font = new System.Drawing.Font("Arial", 6.75F, System.Drawing.FontStyle.Bold);
            this.elapsedTime.ForeColor = System.Drawing.Color.Black;
            this.elapsedTime.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.elapsedTime.Name = "elapsedTime";
            this.elapsedTime.Text = "0.00";
			// 
			// Flow_Chart
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.Controls.Add(this.elapsedTime);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.button1);
			resources.ApplyResources(this, "$this");
			this.Name = "Flow_Chart";
			this.Load += new System.EventHandler(this.FlowChart_Load);
			this.FontChanged += new System.EventHandler(this.Flow_Chart_FontChanged);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.FlowChart_Paint);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label elapsedTime;
    }
}
