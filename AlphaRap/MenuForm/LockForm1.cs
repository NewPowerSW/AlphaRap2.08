using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlphaRap.MenuForm
{
    public partial class LockForm1 : Form
    {
        public LockForm1()
        {
            InitializeComponent();
        }

        public string password_IN=string.Empty;
        public string password2_OUT = string.Empty;

     
     
		private void LockForm1_Load(object sender, EventArgs e)
		{
			groupBox2.Visible = false;
			groupBox_logout.Visible = false;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			if(textBox1.Text.Length>0)
			{
				password_IN = textBox1.Text;
				groupBox2.Visible = true;
				groupBox_logout.Visible = false;
				groupBox_Login.Visible = false;
				textBox1.Clear();
			}
			
		
		
		}

		private void button3_Click(object sender, EventArgs e)
		{
			groupBox2.Visible = false;
			groupBox_logout.Visible = true;
			groupBox_Login.Visible = false;
			label6.Visible = false;
			
		}

		private void button5_Click(object sender, EventArgs e)
		{
			
			if(password_IN == textBox2.Text)
			{
				Close();
				textBox2.Clear();
			}
			label6.Visible = true;
		}

		private void button4_Click(object sender, EventArgs e)
		{
			groupBox2.Visible = true;
			groupBox_logout.Visible = false;
			groupBox_Login.Visible = false;
			textBox2.Clear();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			textBox1.Clear();
			Close();
		}
	}
}
