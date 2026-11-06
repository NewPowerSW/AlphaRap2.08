using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace NPSDK.Motion
{
	partial class NP_PositionTable
	{
		/// <summary>创建点位表与固定操作栏，支持选中行删除及右键删除。</summary>
		private void InitializeComponent()
		{
			DataGridViewCellStyle headerStyle = new DataGridViewCellStyle();
			DataGridViewCellStyle rowHeaderStyle = new DataGridViewCellStyle();
			DataGridViewCellStyle cellStyle = new DataGridViewCellStyle();

			tableLayoutPanel1 = new TableLayoutPanel();
			tableLayoutPanel2 = new TableLayoutPanel();
			btn_AddPost = new Button();
			btn_DeletePost = new Button();
			pointMenu = new ContextMenuStrip { Name = "pointMenu" };
			pointDelete = new ToolStripMenuItem { Name = "pointDelete", Text = "Delete point" };
			pointMenu.Items.Add(pointDelete);
			pointMenu.Opening += OpenPointMenu;
			pointDelete.Click += DeletePoint;
			btn_ReplacePost = new Button();
			btn_UP = new Button();
			btn_GotoPost = new Button();
			Position = new DataGridView();

			tableLayoutPanel1.SuspendLayout();
			tableLayoutPanel2.SuspendLayout();
			((ISupportInitialize)Position).BeginInit();
			SuspendLayout();

			tableLayoutPanel1.ColumnCount = 1;
			tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
			tableLayoutPanel1.Controls.Add(Position, 0, 1);
			tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 0);
			tableLayoutPanel1.Dock = DockStyle.Fill;
			tableLayoutPanel1.Location = new Point(0, 0);
			tableLayoutPanel1.Margin = new Padding(0);
			tableLayoutPanel1.Name = "tableLayoutPanel1";
			tableLayoutPanel1.RowCount = 2;
			tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 48f));
			tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			tableLayoutPanel1.Size = new Size(521, 294);
			tableLayoutPanel1.TabIndex = 0;

			tableLayoutPanel2.ColumnCount = 6;
			tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 84f));
			tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 108f));
			tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 72f));
			tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 84f));
			tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
			tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88f));
			tableLayoutPanel2.Controls.Add(btn_AddPost, 0, 0);
			tableLayoutPanel2.Controls.Add(btn_ReplacePost, 1, 0);
			tableLayoutPanel2.Controls.Add(btn_UP, 2, 0);
			tableLayoutPanel2.Controls.Add(btn_DeletePost, 3, 0);
			tableLayoutPanel2.Controls.Add(btn_GotoPost, 5, 0);
			tableLayoutPanel2.Dock = DockStyle.Fill;
			tableLayoutPanel2.Location = new Point(0, 238);
			tableLayoutPanel2.Margin = new Padding(0);
			tableLayoutPanel2.Padding = new Padding(12, 6, 4, 6);
			tableLayoutPanel2.BackColor = Color.White;
			tableLayoutPanel2.Name = "tableLayoutPanel2";
			tableLayoutPanel2.RowCount = 1;
			tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
			tableLayoutPanel2.Size = new Size(521, 56);
			tableLayoutPanel2.TabIndex = 0;

			ConfigureButton(btn_AddPost, "btn_AddPost", "Add", btn_AddPost_Click);
			ConfigureButton(btn_ReplacePost, "btn_ReplacePost", "Replace", btn_ReplacePost_Click);
			ConfigureButton(btn_UP, "btn_UP", "Up", btn_UP_Click);
			ConfigureButton(btn_GotoPost, "btn_GotoPost", "Go To", btn_GotoPost_Click);
			ConfigureButton(btn_DeletePost, "btn_DeletePost", "Delete", DeleteSelectedPoint);
			btn_DeletePost.ForeColor = Color.FromArgb(185, 28, 28);
			btn_DeletePost.FlatAppearance.BorderColor = Color.FromArgb(254, 202, 202);
			btn_GotoPost.BackColor = Color.FromArgb(37, 99, 180);
			btn_GotoPost.ForeColor = Color.White;
			btn_GotoPost.FlatAppearance.BorderSize = 0;

			Position.AllowUserToAddRows = false;
			Position.AllowUserToDeleteRows = false;
			Position.ContextMenuStrip = pointMenu;
			Position.MouseDown += SelectContextRow;
			Position.CellBeginEdit += BeginEditPoint;
			Position.BorderStyle = BorderStyle.None;
			Position.BackgroundColor = Color.White;
			Position.EnableHeadersVisualStyles = false;
			Position.GridColor = Color.FromArgb(226, 232, 240);
			Position.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
			Position.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
			Position.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
			Position.AllowUserToResizeRows = false;
			Position.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			Position.MultiSelect = false;
			Position.DefaultCellStyle.BackColor = Color.White;
			Position.DefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
			Position.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
			Position.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 64, 120);
			Position.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
			headerStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			headerStyle.BackColor = Color.FromArgb(241, 245, 249);
			headerStyle.Font = new Font("Microsoft YaHei", 9.5f, FontStyle.Regular);
			headerStyle.ForeColor = Color.FromArgb(71, 85, 105);
			headerStyle.SelectionBackColor = Color.FromArgb(241, 245, 249);
			headerStyle.SelectionForeColor = Color.FromArgb(30, 64, 120);
			headerStyle.WrapMode = DataGridViewTriState.True;
			Position.ColumnHeadersDefaultCellStyle = headerStyle;
			Position.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			Position.ColumnHeadersHeight = 36;
			Position.Dock = DockStyle.Fill;
			Position.Location = new Point(0, 0);
			Position.Margin = new Padding(0);
			Position.Name = "Position";
			rowHeaderStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			rowHeaderStyle.BackColor = Color.FromArgb(241, 245, 249);
			rowHeaderStyle.Font = new Font("Microsoft YaHei", 9.5f, FontStyle.Regular);
			rowHeaderStyle.ForeColor = Color.FromArgb(100, 116, 139);
			rowHeaderStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
			rowHeaderStyle.SelectionForeColor = Color.FromArgb(30, 64, 120);
			rowHeaderStyle.WrapMode = DataGridViewTriState.True;
			Position.RowHeadersDefaultCellStyle = rowHeaderStyle;
			cellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			Position.RowsDefaultCellStyle = cellStyle;
			Position.RowHeadersWidth = 50;
			Position.RowTemplate.Height = 34;
			Position.ScrollBars = ScrollBars.Vertical;
			Position.Size = new Size(521, 238);
			Position.TabIndex = 1;
			Position.CellValueChanged += Position_CellValueChanged;
			Position.EditingControlShowing += Position_EditingControlShowing;
			Position.RowPostPaint += Position_RowPostPaint;
			Position.Click += Position_Click;
			Position.ColumnAdded += Position_ColumnAdded;
			Position.DataBindingComplete += Position_DataBindingComplete;

			AutoScaleMode = AutoScaleMode.None;
			BackgroundImageLayout = ImageLayout.Stretch;
			BorderStyle = BorderStyle.None;
			BackColor = Color.White;
			Controls.Add(tableLayoutPanel1);
			Font = new Font("Microsoft YaHei", 9.5f, FontStyle.Regular);
			Margin = new Padding(1);
			Name = "NP_PositionTable";
			Size = new Size(521, 294);

			tableLayoutPanel1.ResumeLayout(false);
			tableLayoutPanel2.ResumeLayout(false);
			((ISupportInitialize)Position).EndInit();
			ResumeLayout(false);
		}

		/// <summary>配置固定点位按钮的布局和事件。</summary>
		private static void ConfigureButton(Button button, string name, string text, EventHandler click)
		{
			button.Dock = DockStyle.Fill;
			button.Margin = new Padding(0, 0, 8, 0);
			button.FlatStyle = FlatStyle.Flat;
			button.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
			button.BackColor = Color.White;
			button.ForeColor = Color.FromArgb(51, 65, 85);
			button.Name = name;
			button.Text = text;
			button.UseVisualStyleBackColor = false;
			button.Click += click;
		}
	}
}
