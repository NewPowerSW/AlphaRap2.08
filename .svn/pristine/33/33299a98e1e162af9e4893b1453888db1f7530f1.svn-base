using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using VUControl.Properties;

namespace VUControl
{
	public class NgDispControl : UserControl
	{
		public TableLayoutPanel layoutPanel = new TableLayoutPanel();

		private const string pbxName = "pVideo";

		private PictureBox myBox;

		private int rowCount = 6;

		private int colCount = 5;

		public PictureBox pVideo;

		private Stack<Control> allControls;

		private IContainer components = null;

		public int RowCount
		{
			get
			{
				return rowCount;
			}
			set
			{
				rowCount = value;
			}
		}

		public int ColCount
		{
			get
			{
				return colCount;
			}
			set
			{
				colCount = value;
			}
		}

		public Stack<Control> AllControls
		{
			get
			{
				return allControls;
			}
			set
			{
				allControls = value;
			}
		}

		public NgDispControl()
		{
			InitializeComponent();
			base.Controls.Add(layoutPanel);
			InitializeVideo(layoutPanel, colCount, rowCount);
			layoutPanel.Dock = DockStyle.Fill;
		}

		private void Display_Load(object sender, EventArgs e)
		{
		}

		private bool InitializeVideo(TableLayoutPanel MainPanel, int videoNum)
		{
			if (videoNum <= 0 || !int.TryParse(Math.Sqrt(videoNum).ToString(), out var result))
			{
				return false;
			}
			int num = MainPanel.Width / result;
			MainPanel.Controls.Clear();
			int num3 = (MainPanel.ColumnCount = result);
			MainPanel.RowCount = num3;
			MainPanel.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
			MainPanel.Refresh();
			for (int i = 0; i < MainPanel.ColumnStyles.Count; i++)
			{
				MainPanel.ColumnStyles[i].SizeType = SizeType.Absolute;
				MainPanel.ColumnStyles[i].Width = num;
			}
			for (int j = 0; j < MainPanel.RowStyles.Count; j++)
			{
				MainPanel.RowStyles[j].SizeType = SizeType.Absolute;
				MainPanel.RowStyles[j].Height = num;
			}
			for (int k = 0; k < videoNum; k++)
			{
				PictureBox pictureBox = new PictureBox();
				Padding padding2 = (pictureBox.Margin = new Padding(20));
				pictureBox.Padding = padding2;
				pictureBox.Name = "pVideo" + k;
				num3 = (pictureBox.Height = num);
				pictureBox.Width = num3;
				pictureBox.Dock = DockStyle.Fill;
				pictureBox.BackgroundImage = Resources.Gray;
				pictureBox.BackgroundImageLayout = ImageLayout.Stretch;
				pictureBox.Click += pVideo_Click;
				MainPanel.Controls.Add(pictureBox, k % result, k / result);
			}
			return true;
		}

		private bool InitializeVideo2(TableLayoutPanel MainPanel, int rowNum, int colNum)
		{
			int num = MainPanel.Width / colNum;
			int num2 = MainPanel.Height / rowNum;
			MainPanel.Controls.Clear();
			MainPanel.RowCount = rowNum;
			MainPanel.ColumnCount = colNum;
			MainPanel.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
			MainPanel.Refresh();
			for (int i = 0; i < MainPanel.ColumnStyles.Count; i++)
			{
				MainPanel.ColumnStyles[i].SizeType = SizeType.Absolute;
				MainPanel.ColumnStyles[i].Width = num;
			}
			for (int j = 0; j < MainPanel.RowStyles.Count; j++)
			{
				MainPanel.RowStyles[j].SizeType = SizeType.Absolute;
				MainPanel.RowStyles[j].Height = num2;
			}
			for (int k = 0; k < rowNum; k++)
			{
				for (int l = 0; l < colNum; l++)
				{
					pVideo = new PictureBox();
					PictureBox pictureBox = pVideo;
					Padding padding2 = (pVideo.Margin = new Padding(10));
					pictureBox.Padding = padding2;
					pVideo.Name = "pVideo" + k;
					pVideo.Width = num;
					pVideo.Height = num2;
					pVideo.Dock = DockStyle.None;
					pVideo.BackgroundImage = Resources.Gray;
					pVideo.BackgroundImageLayout = ImageLayout.Zoom;
					pVideo.Click += pVideo_Click;
					MainPanel.Controls.Add(pVideo, k % colNum, k / rowNum);
				}
			}
			return true;
		}

		private bool InitializeVideo(TableLayoutPanel MainPanel, int rowNum, int colNum)
		{
			int num = MainPanel.Width / colNum;
			int num2 = MainPanel.Height / rowNum;
			MainPanel.Controls.Clear();
			MainPanel.RowCount = rowNum;
			MainPanel.ColumnCount = colNum;
			MainPanel.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
			MainPanel.Refresh();
			for (int i = 0; i < MainPanel.ColumnStyles.Count; i++)
			{
				MainPanel.ColumnStyles[i].SizeType = SizeType.Absolute;
				MainPanel.ColumnStyles[i].Width = num;
			}
			for (int j = 0; j < MainPanel.RowStyles.Count; j++)
			{
				MainPanel.RowStyles[j].SizeType = SizeType.Absolute;
				MainPanel.RowStyles[j].Height = num2;
			}
			for (int k = 0; k < colNum * rowNum; k++)
			{
				PictureBox pictureBox = new PictureBox();
				Padding padding2 = (pictureBox.Margin = new Padding(5));
				pictureBox.Padding = padding2;
				pictureBox.Name = "pVideo" + k;
				pictureBox.Width = num;
				pictureBox.Height = num;
				pictureBox.Dock = DockStyle.Fill;
				pictureBox.BackgroundImage = Resources.Gray;
				pictureBox.BackgroundImageLayout = ImageLayout.Zoom;
				MainPanel.Controls.Add(pictureBox, k % colNum, k / colNum);
			}
			return true;
		}

		private void pVideo_Click(object sender, EventArgs e)
		{
			PictureBox pictureBox = (PictureBox)sender;
			TableLayoutPanelCellPosition position = default(TableLayoutPanelCellPosition);
			if (layoutPanel.GetColumnSpan(pictureBox) == 1)
			{
				foreach (Control control3 in layoutPanel.Controls)
				{
					if (control3.Name != pictureBox.Name)
					{
						control3.Visible = false;
					}
				}
				position = layoutPanel.GetPositionFromControl(pictureBox);
				layoutPanel.SetCellPosition(pictureBox, new TableLayoutPanelCellPosition(0, 0));
				layoutPanel.SetRowSpan(pictureBox, layoutPanel.RowCount);
				layoutPanel.SetColumnSpan(pictureBox, layoutPanel.ColumnCount);
				return;
			}
			foreach (Control control4 in layoutPanel.Controls)
			{
				control4.Visible = true;
			}
			layoutPanel.SetCellPosition(pictureBox, position);
			layoutPanel.SetRowSpan(pictureBox, 1);
			layoutPanel.SetColumnSpan(pictureBox, 1);
		}

		public Stack<Control> GetAllControlOfWindow(ControlCollection containChildControlWind)
		{
			Stack<Control> stack = new Stack<Control>();
			foreach (Control item in containChildControlWind)
			{
				stack.Push(item);
			}
			return stack;
		}

		public Stack<T> GetAppointControlOfWindows<T>(ControlCollection containChildControlWindow)
		{
			Stack<T> stack = new Stack<T>();
			foreach (object item in containChildControlWindow)
			{
				if (item is T)
				{
					stack.Push((T)item);
				}
			}
			return stack;
		}

		public Control GetControlOfName(string controlName, ControlCollection containChildWindow)
		{
			if (string.IsNullOrEmpty(controlName))
			{
				return null;
			}
			foreach (Control item in containChildWindow)
			{
				if (item.Name.Equals(controlName))
				{
					return item;
				}
			}
			return null;
		}

		public void SetControlImage(ref Control control, bool bFlag = true)
		{
			if (control is PictureBox)
			{
				PictureBox pictureBox = control as PictureBox;
				if (bFlag)
				{
					pictureBox.BackgroundImage = Resources.Green;
				}
				else
				{
					pictureBox.BackgroundImage = Resources.Red;
				}
			}
		}

		public void InitAllControls()
		{
			foreach (Control allControl in allControls)
			{
				if (allControl is PictureBox)
				{
					PictureBox pictureBox = allControl as PictureBox;
					pictureBox.BackgroundImage = Resources.Gray;
				}
			}
		}

		public void SetImageColor(int index, bool bFlag = false)
		{
			string controlName = "pVideo" + index;
			Control control = GetControlOfName(controlName, layoutPanel.Controls);
			SetControlImage(ref control, bFlag);
		}

		private void Display_Resize(object sender, EventArgs e)
		{
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			base.SuspendLayout();
			base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 15f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
			base.Name = "NgDispControl";
			base.Size = new System.Drawing.Size(329, 250);
			base.Load += new System.EventHandler(Display_Load);
			base.Resize += new System.EventHandler(Display_Resize);
			base.ResumeLayout(false);
		}
	}
}
