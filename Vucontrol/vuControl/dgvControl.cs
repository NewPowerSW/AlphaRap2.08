using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace vuControl
{
	public class dgvControl : Form
	{
		private IContainer components = null;

		public dgvControl()
		{
			InitializeComponent();
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
			base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 12f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(686, 318);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			base.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			base.Name = "dgvControl";
			base.Padding = new System.Windows.Forms.Padding(0, 22, 0, 0);
			this.Text = "Form1";
			base.ResumeLayout(false);
		}
	}
}
