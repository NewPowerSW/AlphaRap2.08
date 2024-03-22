using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AlphaRap
{
    public partial class LoadForm : Form
    {
        
        public LoadForm()
        {
            InitializeComponent();
            //InitializeBackgroundWorker();
        }

     

        private void LoadForm_Load(object sender, EventArgs e)
        {
            //this.ProgressBar1.Maximum = 100;                   
        }

        private void tmr_Refresh_Tick(object sender, EventArgs e)
        {
            ProgressBar1.Value = MiddleLayer.LoadProcessRate;

            if (MiddleLayer.LoadProcessRate >= 99)
            {
                
                tmr_Refresh.Enabled = false;
                this.Close();
            }
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
