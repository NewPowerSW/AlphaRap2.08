using System.Drawing;
using System.Windows.Forms;

namespace AlphaRap
{
    partial class DownConveyorIoForm
    {
        private Label lblHelp_dyn;
        private DataGridView grid;
        private DataGridViewTextBoxColumn colSignal;
        private DataGridViewComboBoxColumn colResource;
        private FlowLayoutPanel buttons;
        private Button btnOk_dyn;
        private Button btnCancel_dyn;

        /// <summary>布置下料信号的选项表和确认区，可在设计器预览。</summary>
        private void InitializeComponent()
        {
            this.lblHelp_dyn = new Label();
            this.grid = new DataGridView();
            this.colSignal = new DataGridViewTextBoxColumn();
            this.colResource = new DataGridViewComboBoxColumn();
            this.buttons = new FlowLayoutPanel();
            this.btnOk_dyn = new Button();
            this.btnCancel_dyn = new Button();
            ((System.ComponentModel.ISupportInitialize)this.grid).BeginInit();
            this.buttons.SuspendLayout();
            this.SuspendLayout();

            this.lblHelp_dyn.Name = "lblHelp_dyn";
            this.lblHelp_dyn.Dock = DockStyle.Top;
            this.lblHelp_dyn.Height = 48;
            this.lblHelp_dyn.TextAlign = ContentAlignment.MiddleLeft;
            this.lblHelp_dyn.Text = "按名称选择 IO。确定后，点主界面的「保存」应用绑定。";

            this.grid.Name = "grid";
            this.grid.Dock = DockStyle.Fill;
            this.grid.BackgroundColor = SystemColors.Window;
            this.grid.AllowUserToAddRows = false;
            this.grid.AllowUserToDeleteRows = false;
            this.grid.AllowUserToResizeRows = false;
            this.grid.RowHeadersVisible = false;
            this.grid.RowTemplate.Height = 28;
            this.grid.MultiSelect = false;
            this.grid.EditMode = DataGridViewEditMode.EditOnEnter;
            this.grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.grid.Columns.AddRange(this.colSignal, this.colResource);

            this.colSignal.Name = "colSignal";
            this.colSignal.HeaderText = "下料信号";
            this.colSignal.FillWeight = 42;
            this.colSignal.ReadOnly = true;
            this.colSignal.SortMode = DataGridViewColumnSortMode.NotSortable;
            this.colResource.Name = "colResource";
            this.colResource.HeaderText = "绑定 IO";
            this.colResource.FillWeight = 58;

            this.buttons.Name = "buttons";
            this.buttons.Dock = DockStyle.Bottom;
            this.buttons.Height = 48;
            this.buttons.Padding = new Padding(0, 10, 0, 0);
            this.buttons.FlowDirection = FlowDirection.RightToLeft;
            this.buttons.WrapContents = false;
            this.buttons.Controls.Add(this.btnCancel_dyn);
            this.buttons.Controls.Add(this.btnOk_dyn);

            this.btnOk_dyn.Name = "btnOk_dyn";
            this.btnOk_dyn.Size = new Size(96, 30);
            this.btnOk_dyn.Text = "确定";
            this.btnOk_dyn.Click += new System.EventHandler(this.AcceptBindings);
            this.btnCancel_dyn.Name = "btnCancel_dyn";
            this.btnCancel_dyn.Size = new Size(96, 30);
            this.btnCancel_dyn.Text = "取消";
            this.btnCancel_dyn.DialogResult = DialogResult.Cancel;

            this.AcceptButton = this.btnOk_dyn;
            this.CancelButton = this.btnCancel_dyn;
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.ClientSize = new Size(640, 550);
            this.MinimumSize = new Size(530, 380);
            this.Padding = new Padding(16, 8, 16, 12);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Name = "DownConveyorIoForm";
            this.Text = "下料 IO 绑定";
            this.Controls.Add(this.grid);
            this.Controls.Add(this.buttons);
            this.Controls.Add(this.lblHelp_dyn);
            this.buttons.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)this.grid).EndInit();
            this.ResumeLayout(false);
        }
    }
}
