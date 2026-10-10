namespace AlphaRap
{
    partial class DeviceControl
    {
        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
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

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.panelTop = new System.Windows.Forms.Panel();
            this.txtName = new System.Windows.Forms.TextBox();
            this.lblName = new AlphaRap.UiLabel();
            this.cboType = new System.Windows.Forms.ComboBox();
            this.lblType = new AlphaRap.UiLabel();
            this.panelMid = new System.Windows.Forms.Panel();
            this.lblState = new AlphaRap.UiLabel();
            this.btnRecv = new AlphaRap.FlatButton();
            this.btnClose = new AlphaRap.FlatButton();
            this.btnOpen = new AlphaRap.FlatButton();
            this.panelParam = new System.Windows.Forms.Panel();
            this.propGrid = new System.Windows.Forms.PropertyGrid();
            this.lblParam = new AlphaRap.UiLabel();
            this.panelSend = new System.Windows.Forms.Panel();
            this.btnSend = new AlphaRap.FlatButton();
            this.txtSend = new System.Windows.Forms.TextBox();
            this.txtRecv = new System.Windows.Forms.TextBox();
            this.panelTop.SuspendLayout();
            this.panelMid.SuspendLayout();
            this.panelParam.SuspendLayout();
            this.panelSend.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.White;
            this.panelTop.Controls.Add(this.txtName);
            this.panelTop.Controls.Add(this.lblName);
            this.panelTop.Controls.Add(this.cboType);
            this.panelTop.Controls.Add(this.lblType);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(620, 34);
            this.panelTop.TabIndex = 0;
            // 
            // lblType
            // 
            this.lblType.AutoSize = false;
            this.lblType.Font = new System.Drawing.Font("宋体", 10F);
            this.lblType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.lblType.Location = new System.Drawing.Point(8, 8);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(62, 18);
            this.lblType.TabIndex = 0;
            this.lblType.Text = "设备类";
            this.lblType.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cboType
            // 
            this.cboType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboType.Font = new System.Drawing.Font("宋体", 10F);
            this.cboType.FormattingEnabled = true;
            this.cboType.Location = new System.Drawing.Point(72, 4);
            this.cboType.Name = "cboType";
            this.cboType.Size = new System.Drawing.Size(240, 24);
            this.cboType.TabIndex = 1;
            this.cboType.SelectedIndexChanged += new System.EventHandler(this.cboType_SelectedIndexChanged);
            // 
            // lblName
            // 
            this.lblName.AutoSize = false;
            this.lblName.Font = new System.Drawing.Font("宋体", 10F);
            this.lblName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(50)))), ((int)(((byte)(64)))));
            this.lblName.Location = new System.Drawing.Point(326, 8);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(56, 18);
            this.lblName.TabIndex = 2;
            this.lblName.Text = "设备名";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtName
            // 
            this.txtName.Font = new System.Drawing.Font("宋体", 10F);
            this.txtName.Location = new System.Drawing.Point(386, 4);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(180, 24);
            this.txtName.TabIndex = 3;
            this.txtName.Text = "Device1";
            this.txtName.Leave += new System.EventHandler(this.txtName_Leave);
            // 
            // panelMid
            // 
            this.panelMid.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(248)))), ((int)(((byte)(249)))));
            this.panelMid.Controls.Add(this.lblState);
            this.panelMid.Controls.Add(this.btnRecv);
            this.panelMid.Controls.Add(this.btnClose);
            this.panelMid.Controls.Add(this.btnOpen);
            this.panelMid.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelMid.Location = new System.Drawing.Point(0, 34);
            this.panelMid.Name = "panelMid";
            this.panelMid.Size = new System.Drawing.Size(620, 38);
            this.panelMid.TabIndex = 1;
            // 
            // btnOpen
            // 
            this.btnOpen.CornerRadius = 6;
            this.btnOpen.Location = new System.Drawing.Point(6, 5);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(88, 28);
            this.btnOpen.TabIndex = 0;
            this.btnOpen.Text = "打开连接";
            this.btnOpen.Variant = AlphaRap.FlatButtonVariant.Primary;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnClose
            // 
            this.btnClose.CornerRadius = 6;
            this.btnClose.Location = new System.Drawing.Point(100, 5);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(88, 28);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "关闭连接";
            this.btnClose.Variant = AlphaRap.FlatButtonVariant.Surface;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnRecv
            // 
            this.btnRecv.CornerRadius = 6;
            this.btnRecv.Location = new System.Drawing.Point(194, 5);
            this.btnRecv.Name = "btnRecv";
            this.btnRecv.Size = new System.Drawing.Size(88, 28);
            this.btnRecv.TabIndex = 2;
            this.btnRecv.Text = "接收一次";
            this.btnRecv.Variant = AlphaRap.FlatButtonVariant.Surface;
            this.btnRecv.Click += new System.EventHandler(this.btnRecv_Click);
            // 
            // lblState
            // 
            this.lblState.AutoSize = false;
            this.lblState.Font = new System.Drawing.Font("宋体", 10F);
            this.lblState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(138)))), ((int)(((byte)(155)))));
            this.lblState.Location = new System.Drawing.Point(292, 10);
            this.lblState.Name = "lblState";
            this.lblState.Size = new System.Drawing.Size(320, 18);
            this.lblState.TabIndex = 3;
            this.lblState.Text = "未选择设备类";
            this.lblState.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panelParam
            // 
            this.panelParam.BackColor = System.Drawing.Color.White;
            this.panelParam.Controls.Add(this.propGrid);
            this.panelParam.Controls.Add(this.lblParam);
            this.panelParam.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelParam.Location = new System.Drawing.Point(0, 72);
            this.panelParam.Name = "panelParam";
            this.panelParam.Size = new System.Drawing.Size(620, 168);
            this.panelParam.TabIndex = 2;
            // 
            // lblParam
            // 
            this.lblParam.AutoSize = false;
            this.lblParam.Font = new System.Drawing.Font("宋体", 10F);
            this.lblParam.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(122)))), ((int)(((byte)(138)))), ((int)(((byte)(155)))));
            this.lblParam.Location = new System.Drawing.Point(8, 6);
            this.lblParam.Name = "lblParam";
            this.lblParam.Size = new System.Drawing.Size(420, 18);
            this.lblParam.TabIndex = 0;
            this.lblParam.Text = "连接参数（按所选设备类自动列出，可直接编辑）";
            this.lblParam.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // propGrid
            // 
            this.propGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.propGrid.CommandsVisibleIfAvailable = false;
            this.propGrid.Font = new System.Drawing.Font("宋体", 9F);
            this.propGrid.HelpVisible = false;
            this.propGrid.Location = new System.Drawing.Point(6, 26);
            this.propGrid.Name = "propGrid";
            this.propGrid.PropertySort = System.Windows.Forms.PropertySort.Alphabetical;
            this.propGrid.Size = new System.Drawing.Size(608, 136);
            this.propGrid.TabIndex = 1;
            this.propGrid.ToolbarVisible = false;
            // 
            // panelSend
            // 
            this.panelSend.BackColor = System.Drawing.Color.White;
            this.panelSend.Controls.Add(this.btnSend);
            this.panelSend.Controls.Add(this.txtSend);
            this.panelSend.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelSend.Location = new System.Drawing.Point(0, 424);
            this.panelSend.Name = "panelSend";
            this.panelSend.Size = new System.Drawing.Size(620, 36);
            this.panelSend.TabIndex = 3;
            // 
            // txtSend
            // 
            this.txtSend.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSend.Font = new System.Drawing.Font("Consolas", 10F);
            this.txtSend.Location = new System.Drawing.Point(6, 5);
            this.txtSend.Name = "txtSend";
            this.txtSend.Size = new System.Drawing.Size(504, 23);
            this.txtSend.TabIndex = 0;
            // 
            // btnSend
            // 
            this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSend.CornerRadius = 6;
            this.btnSend.Location = new System.Drawing.Point(518, 4);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(88, 28);
            this.btnSend.TabIndex = 1;
            this.btnSend.Text = "发送";
            this.btnSend.Variant = AlphaRap.FlatButtonVariant.Primary;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // txtRecv
            // 
            this.txtRecv.BackColor = System.Drawing.Color.White;
            this.txtRecv.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtRecv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtRecv.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtRecv.Location = new System.Drawing.Point(0, 240);
            this.txtRecv.Multiline = true;
            this.txtRecv.Name = "txtRecv";
            this.txtRecv.ReadOnly = true;
            this.txtRecv.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtRecv.Size = new System.Drawing.Size(620, 184);
            this.txtRecv.TabIndex = 4;
            // 
            // DeviceControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(248)))), ((int)(((byte)(249)))));
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.txtRecv);
            this.Controls.Add(this.panelSend);
            this.Controls.Add(this.panelParam);
            this.Controls.Add(this.panelMid);
            this.Controls.Add(this.panelTop);
            this.Font = new System.Drawing.Font("宋体", 10F);
            this.Name = "DeviceControl";
            this.Size = new System.Drawing.Size(620, 460);
            this.Load += new System.EventHandler(this.DeviceControl_Load);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panelMid.ResumeLayout(false);
            this.panelParam.ResumeLayout(false);
            this.panelSend.ResumeLayout(false);
            this.panelSend.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private UiLabel lblType;
        private System.Windows.Forms.ComboBox cboType;
        private UiLabel lblName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Panel panelMid;
        private FlatButton btnOpen;
        private FlatButton btnClose;
        private FlatButton btnRecv;
        private UiLabel lblState;
        private System.Windows.Forms.Panel panelParam;
        private UiLabel lblParam;
        private System.Windows.Forms.PropertyGrid propGrid;
        private System.Windows.Forms.Panel panelSend;
        private System.Windows.Forms.TextBox txtSend;
        private FlatButton btnSend;
        private System.Windows.Forms.TextBox txtRecv;
    }
}
