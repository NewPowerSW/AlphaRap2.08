namespace Alpha
{
    partial class OptionChoiceForm
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
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.panelHeader = new System.Windows.Forms.Panel();
            this.panelButtons = new System.Windows.Forms.Panel();
            this.Button1 = new AlphaRap.FlatButton();
            this.Button2 = new AlphaRap.FlatButton();
            this.Button3 = new AlphaRap.FlatButton();
            this.panelMessage = new System.Windows.Forms.Panel();
            this.lblMessage = new AlphaRap.UiLabel();
            this.panelButtons.SuspendLayout();
            this.panelMessage.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelHeader
            // 顶部严重度色条：颜色由代码按"第一个可见按钮"的语义色自动设置
            // 
            this.panelHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(4)))), ((int)(((byte)(108)))), ((int)(((byte)(182)))));
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(1, 1);
            this.panelHeader.Margin = new System.Windows.Forms.Padding(0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(558, 6);
            this.panelHeader.TabIndex = 0;
            // 
            // Button1
            // 主操作（确认 / 重试）：绿色
            // 
            this.Button1.CornerRadius = 8;
            this.Button1.Location = new System.Drawing.Point(93, 17);
            this.Button1.Margin = new System.Windows.Forms.Padding(0);
            this.Button1.Name = "Button1_dyn";
            this.Button1.Size = new System.Drawing.Size(116, 42);
            this.Button1.TabIndex = 0;
            this.Button1.Text = "确定";
            this.Button1.Variant = AlphaRap.FlatButtonVariant.Success;
            this.Button1.Click += new System.EventHandler(this.btnChoice1_Click);
            // 
            // Button2
            // 次要操作（跳过 / 知道了）：中性白底
            // 
            this.Button2.CornerRadius = 8;
            this.Button2.Location = new System.Drawing.Point(221, 17);
            this.Button2.Margin = new System.Windows.Forms.Padding(0);
            this.Button2.Name = "Button2_dyn";
            this.Button2.Size = new System.Drawing.Size(116, 42);
            this.Button2.TabIndex = 1;
            this.Button2.Text = "跳过";
            this.Button2.Variant = AlphaRap.FlatButtonVariant.Surface;
            this.Button2.Click += new System.EventHandler(this.btnChoice2_Click);
            // 
            // Button3
            // 取消类：红色
            // 
            this.Button3.CornerRadius = 8;
            this.Button3.Location = new System.Drawing.Point(349, 17);
            this.Button3.Margin = new System.Windows.Forms.Padding(0);
            this.Button3.Name = "Button3_dyn";
            this.Button3.Size = new System.Drawing.Size(116, 42);
            this.Button3.TabIndex = 2;
            this.Button3.Text = "取消";
            this.Button3.Variant = AlphaRap.FlatButtonVariant.Danger;
            this.Button3.Click += new System.EventHandler(this.btnChoice3_Click);
            // 
            // panelButtons
            // 按钮区：隐藏的按钮不占位，可见按钮由 RelayoutButtons() 居中排布
            // 
            this.panelButtons.BackColor = System.Drawing.Color.White;
            this.panelButtons.Controls.Add(this.Button1);
            this.panelButtons.Controls.Add(this.Button2);
            this.panelButtons.Controls.Add(this.Button3);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelButtons.Location = new System.Drawing.Point(1, 153);
            this.panelButtons.Margin = new System.Windows.Forms.Padding(0);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new System.Drawing.Size(558, 76);
            this.panelButtons.TabIndex = 2;
            // 
            // lblMessage
            // 消息文字：运行时才填，用 UiLabel 避免被语言包接管
            // 
            this.lblMessage.AutoSize = false;
            this.lblMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMessage.Font = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblMessage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.lblMessage.Location = new System.Drawing.Point(28, 16);
            this.lblMessage.Margin = new System.Windows.Forms.Padding(0);
            this.lblMessage.Name = "lblMessage_dyn";
            this.lblMessage.Size = new System.Drawing.Size(502, 138);
            this.lblMessage.TabIndex = 0;
            this.lblMessage.Text = "请选择";
            this.lblMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelMessage
            // 
            this.panelMessage.BackColor = System.Drawing.Color.White;
            this.panelMessage.Controls.Add(this.lblMessage);
            this.panelMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMessage.Location = new System.Drawing.Point(1, 7);
            this.panelMessage.Margin = new System.Windows.Forms.Padding(0);
            this.panelMessage.Name = "panelMessage";
            this.panelMessage.Padding = new System.Windows.Forms.Padding(28, 16, 28, 12);
            this.panelMessage.Size = new System.Drawing.Size(558, 146);
            this.panelMessage.TabIndex = 1;
            // 
            // OptionChoiceForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(231)))), ((int)(((byte)(240)))));
            this.ClientSize = new System.Drawing.Size(560, 230);
            this.ControlBox = false;
            this.Controls.Add(this.panelMessage);
            this.Controls.Add(this.panelButtons);
            this.Controls.Add(this.panelHeader);
            this.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OptionChoiceForm";
            this.Padding = new System.Windows.Forms.Padding(1);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "OptionChoiceForm";
            this.TopMost = true;
            this.panelButtons.ResumeLayout(false);
            this.panelMessage.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Panel panelButtons;
        private AlphaRap.FlatButton Button1;
        private AlphaRap.FlatButton Button2;
        private AlphaRap.FlatButton Button3;
        private System.Windows.Forms.Panel panelMessage;
        private AlphaRap.UiLabel lblMessage;
    }
}
