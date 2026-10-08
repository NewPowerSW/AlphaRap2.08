using System;
using System.Windows.Forms;

namespace AlphaRap.FunctionForms
{
    public partial class ErrorOptionForm : Form
    {
        public ErrorOptionForm()
        {
            this.TopMost = true;
            //CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
           // timer1.Start();
        }

        public void SetAlarmCode(string sMsg)
        {
            txtAlarm.Text = sMsg;
        }

        public void SetCaption(string sMsg)
        {
            label1.Text = sMsg;
        }
        public void SetButtonText(string sMsg)
        {
            button1.Text = sMsg;
        }
        public void SetButtonText(string sMsg1 , string sMsg2)
        {
            button1.Text = sMsg1;
            button2.Text = sMsg2;
        }
        public void SetButtonText(string sMsg1, string sMsg2,string sMsg3)
        {
            button1.Text = sMsg1;
            button2.Text = sMsg2;
            button3.Text = sMsg3;
        }
        public void SetButton(int ButtonNumber)
        {
            switch (ButtonNumber)
            {
                case 0:
                    button1.Visible = false;
                    button2.Visible = true;
                    button2.Text = "OK";
                    button3.Visible = false;
                    break;

                case 1:
                    button1.Visible = true;
                    button2.Visible = false;
                    button3.Visible = false;
                    break;
                case 2:
                    button1.Visible = true;
                    button2.Visible = true;
                    button3.Visible = false;
                    break;
                case 3:
                    button1.Visible = true;
                    button2.Visible = true;
                    button3.Visible = true;
                    break;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            SysPara.ErrorWindowStatus = false;
            this.DialogResult = DialogResult.Retry;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SysPara.ErrorWindowStatus = false;
            this.DialogResult = DialogResult.Abort;            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SysPara.ErrorWindowStatus = false;
            this.DialogResult = DialogResult.Cancel;
        }

        private void ErrorOptionForm_Load(object sender, EventArgs e)
        {
            SysPara.ErrorWindowStatus = true;
            this.TopMost = true;
            this.Focus();
        }

        private void ErrorOptionForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            SysPara.ErrorWindowStatus = false;
        }
    }
}
