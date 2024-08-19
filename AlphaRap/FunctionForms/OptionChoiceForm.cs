using System;
using System.Drawing;
using System.Windows.Forms;

namespace Alpha
{
    public partial class OptionChoiceForm : Form
    {
        /// <summary>
        /// DialogResult.Yes es un reintentar y Abort es no reintentar
        /// </summary>
        public DialogResult dResult = DialogResult.None;

        public OptionChoiceForm()
        {
            InitializeComponent();
            this.ControlBox = false;
            this.Show();
            this.Hide();
        }

        public void fnChangeButtonsText(string sButton1Text, string sButton2Text="", string sButton3Text = "")
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() =>
                {
                    Button1.Text = sButton1Text;
                    Button2.Text = sButton2Text;
                    Button3.Text = sButton3Text;
                }));
            }
            else
            {
                Button1.Text = sButton1Text;
                Button2.Text = sButton2Text;
                Button3.Text = sButton3Text;
            }
        }

        public void fnSetMessageAndButtons(string sMessage, bool bShowButton1, bool bShowButton2, bool bShowButton3)
        {
            dResult = DialogResult.None;

            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() =>
                {
                    Button1.Visible = bShowButton1;
                    Button2.Visible = bShowButton2;
                    Button3.Visible = bShowButton3;
                    lblMessage.Text = sMessage;
                }));
            }

            else
            {
                Button1.Visible = bShowButton1;
                Button2.Visible = bShowButton2;
                Button3.Visible = bShowButton3;
                lblMessage.Text = sMessage;
            }
        }
		public void fnSetTextMessageNShowBackColor(Color MessageBackColoer, Color bShowButton1BackColor, Color bShowButton2Color,  Color bShowButton3Color)
        {
			lblMessage.BackColor = MessageBackColoer;
			Button1.BackColor = bShowButton1BackColor;
			Button2.BackColor = bShowButton2Color;
			Button3.BackColor = bShowButton3Color;

		}

		public void fnSetTextMessageNShow(string strMessage, bool bShowButton1, bool bShowButton2, bool bShowButton3)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() =>
                {
                    Button1.Visible = (bShowButton1) ? true : false;
                    Button2.Visible = (bShowButton2) ? true : false;
                    Button3.Visible = (bShowButton3) ? true : false;
                    dResult = DialogResult.None;
                    lblMessage.Text = strMessage;
                    this.Show();
                }));
            }
            else
            {
                Button1.Visible = (bShowButton1) ? true : false;
                Button2.Visible = (bShowButton2) ? true : false;
                Button3.Visible = (bShowButton3) ? true : false;
                dResult = DialogResult.None;
                lblMessage.Text = strMessage;
                this.Show();
            }
        }

        public void fnChangeButtonRetryMessage(string msg)
        {
            this.BeginInvoke((MethodInvoker)(() =>
            {
                Button1.Text = msg;
            }));
        }
        public void doHideWindow()
        {
            if (this.InvokeRequired)
                this.Invoke((MethodInvoker)(() => this.Hide()));
            else
                this.Hide();
        }
        private void btnReintentar_Click(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() =>
                {
                    dResult = DialogResult.Yes;
                    this.Hide();
                }));
            }
            else
            {
                dResult = DialogResult.Yes;
                this.Hide();
            }
        }
        private void btnSaltar_Click(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() =>
                {
                    dResult = DialogResult.Ignore;
                    this.Hide();
                }));
            }
            else
            {
                dResult = DialogResult.Ignore;
                this.Hide();
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)(() =>
                {
                    dResult = DialogResult.Abort;
                    this.Hide();
                }));
            }
            else
            {
                dResult = DialogResult.Abort;
                this.Hide();
            }
        }
    }
}
