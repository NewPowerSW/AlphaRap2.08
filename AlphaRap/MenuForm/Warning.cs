using System;
using AlphaRapLibrary;

namespace AlphaRap
{
	public partial class Warning : ModuleBaseForm
	{


		public string Tipstring = "";
		public string button1string = "";
		public string button2string = "";

		public int SelectIndex = 0;

		public Warning()
		{
			InitializeComponent();
			this.ControlBox = false;

		}


		private void button1_Click(object sender, EventArgs e)
		{

			SelectIndex = 0;
			this.Close();
		}



		private void Warning_Load(object sender, EventArgs e)
		{
			this.TopMost = true;
			button1.Focus();


		}
		public void SetShowMessage(string Label, string Button)
		{

			textBox1.Text = Label;
			button1.Text = Button;
		}
	}
}
