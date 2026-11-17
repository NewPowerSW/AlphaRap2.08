namespace AlphaRap
{
    partial class VpRotationForm
    {
        private System.ComponentModel.IContainer components;
        /// <summary>释放窗口控件。</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing) ReleaseRotationResources();
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>建立可在设计器预览的旋转采样表、实际角度输入和草稿操作按钮。</summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lblInstructions = new System.Windows.Forms.Label();
            this.angleRow = new System.Windows.Forms.Panel();
            this.lblAngle = new System.Windows.Forms.Label();
            this.tbAngle = new System.Windows.Forms.TextBox();
            this.actions = new System.Windows.Forms.FlowLayoutPanel();
            this.btnCapture = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnCalculate = new System.Windows.Forms.Button();
            this.gridSamples = new System.Windows.Forms.DataGridView();
            this.cPixelX = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cPixelY = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cAngle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblResult = new System.Windows.Forms.Label();
            this.footer = new System.Windows.Forms.FlowLayoutPanel();
            this.btnDone = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.gridSamples).BeginInit();
            this.lblInstructions.Name = "lblInstructions";
            this.lblInstructions.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblInstructions.Height = 64;
            this.lblInstructions.Text = "九点标定有效后，保持相机和 XYZ 不动，仅旋转 R 轴。每次输入实际 R 角度后拍照，再添加采样。";
            this.angleRow.Name = "angleRow";
            this.angleRow.Dock = System.Windows.Forms.DockStyle.Top;
            this.angleRow.Height = 38;
            this.angleRow.Controls.Add(this.lblAngle);
            this.angleRow.Controls.Add(this.tbAngle);
            this.lblAngle.Name = "lblAngle";
            this.lblAngle.Text = "实际 R 角度（°）";
            this.lblAngle.Location = new System.Drawing.Point(0, 6);
            this.lblAngle.Size = new System.Drawing.Size(180, 26);
            this.tbAngle.Name = "tbAngle";
            this.tbAngle.Location = new System.Drawing.Point(184, 3);
            this.tbAngle.Size = new System.Drawing.Size(110, 27);
            this.actions.Name = "actions";
            this.actions.Dock = System.Windows.Forms.DockStyle.Top;
            this.actions.Height = 40;
            this.actions.WrapContents = false;
            this.actions.Controls.Add(this.btnCapture);
            this.actions.Controls.Add(this.btnAdd);
            this.actions.Controls.Add(this.btnDelete);
            this.actions.Controls.Add(this.btnClear);
            this.actions.Controls.Add(this.btnCalculate);
            this.btnCapture.Name = "btnCapture"; this.btnCapture.Text = "拍照";
            this.btnAdd.Name = "btnAdd"; this.btnAdd.Text = "添加采样";
            this.btnDelete.Name = "btnDelete"; this.btnDelete.Text = "删除选中点";
            this.btnClear.Name = "btnClear"; this.btnClear.Text = "清空旋转采样";
            this.btnCalculate.Name = "btnCalculate"; this.btnCalculate.Text = "计算旋转中心";
            this.btnCapture.Size = new System.Drawing.Size(128, 32);
            this.btnAdd.Size = new System.Drawing.Size(128, 32);
            this.btnDelete.Size = new System.Drawing.Size(128, 32);
            this.btnClear.Size = new System.Drawing.Size(145, 32);
            this.btnCalculate.Size = new System.Drawing.Size(170, 32);
            this.gridSamples.Name = "gridSamples";
            this.gridSamples.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridSamples.AllowUserToAddRows = false;
            this.gridSamples.AllowUserToDeleteRows = false;
            this.gridSamples.AutoGenerateColumns = false;
            this.gridSamples.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.gridSamples.RowHeadersVisible = false;
            this.gridSamples.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridSamples.MultiSelect = false;
            this.gridSamples.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { this.cPixelX, this.cPixelY, this.cAngle });
            this.cPixelX.Name = "cPixelX"; this.cPixelX.HeaderText = "像素 X"; this.cPixelX.ReadOnly = true;
            this.cPixelY.Name = "cPixelY"; this.cPixelY.HeaderText = "像素 Y"; this.cPixelY.ReadOnly = true;
            this.cAngle.Name = "cAngle"; this.cAngle.HeaderText = "实际 R 角度（°）";
            this.lblResult.Name = "rotationResult_dyn";
            this.lblResult.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblResult.Height = 76;
            this.lblResult.Text = "待采集并计算；结果由主界面保存。";
            this.lblResult.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.footer.Name = "footer";
            this.footer.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.footer.Height = 42;
            this.footer.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.footer.Controls.Add(this.btnDone);
            this.footer.Controls.Add(this.btnCancel);
            this.btnDone.Name = "btnDone"; this.btnDone.Text = "完成"; this.btnDone.Size = new System.Drawing.Size(130, 32);
            this.btnCancel.Name = "btnCancel"; this.btnCancel.Text = "取消修改"; this.btnCancel.Size = new System.Drawing.Size(150, 32);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Font = new System.Drawing.Font("宋体", 10F);
            this.Padding = new System.Windows.Forms.Padding(14);
            this.ClientSize = new System.Drawing.Size(790, 540);
            this.MinimumSize = new System.Drawing.Size(806, 480);
            this.Controls.Add(this.gridSamples);
            this.Controls.Add(this.lblResult);
            this.Controls.Add(this.footer);
            this.Controls.Add(this.actions);
            this.Controls.Add(this.angleRow);
            this.Controls.Add(this.lblInstructions);
            this.Name = "VpRotationForm";
            this.Text = "2 旋转标定（可选）";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            ((System.ComponentModel.ISupportInitialize)this.gridSamples).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label lblInstructions, lblAngle, lblResult;
        private System.Windows.Forms.Panel angleRow;
        private System.Windows.Forms.TextBox tbAngle;
        private System.Windows.Forms.FlowLayoutPanel actions, footer;
        private System.Windows.Forms.Button btnCapture, btnAdd, btnDelete, btnClear, btnCalculate, btnDone, btnCancel;
        private System.Windows.Forms.DataGridView gridSamples;
        private System.Windows.Forms.DataGridViewTextBoxColumn cPixelX, cPixelY, cAngle;
    }
}
