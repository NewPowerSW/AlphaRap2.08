using Cognex.VisionPro;
using Cognex.VisionPro.ToolBlock;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlphaRap
{
    public partial class TBEditForm : Form
    {
        VisionproInterface EditInterface;
        public TBEditForm(VisionproInterface CogInterface)
        {
            InitializeComponent();
            EditInterface = CogInterface;
            
            cogToolBlockEditV21.Subject = EditInterface.TB;
            this.Text = EditInterface.StationName;
        }

        private void TBEditForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult = MessageBox.Show("Save ToolBlock?", "Save", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (DialogResult == DialogResult.Yes)
            {
                EditInterface.SaveTB();
            }
            else if (DialogResult == System.Windows.Forms.DialogResult.No)
            {
                EditInterface.LoadTB();
            }
            else if (DialogResult == System.Windows.Forms.DialogResult.Cancel)
            {
                e.Cancel = true;
            }
        }
    }
}
