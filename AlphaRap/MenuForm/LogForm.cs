
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlphaRapLibrary;

namespace AlphaRap
{
    public partial class LogForm : ModuleBaseForm
    {
        public LogForm()
        {
            InitializeComponent();
        }

        public void RefreshTxtData()
        {
            RefreshDifferentThreadUI(txtRun, () => txtClear());
        }
        public void txtClear()
        {
      
            txtRun.Text = GetSettingValue("Path", "RunPath");
            txtUser.Text = GetSettingValue("Path", "UserPath");
 
        }
        public static void RefreshDifferentThreadUI(Control control, Action action)
        {
            if (control.InvokeRequired)
            {
                Action refreshUI = new Action(action);
                control.Invoke(refreshUI);
            }
            else
            {
                action.Invoke();
            }
        }
        /// <summary>
        /// 页面离开时执行的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MESForm_Leave(object sender, EventArgs e)
        {
            if (SysPara.items != 1)
            {
                txtUser.Focus();
                MiddleLayer.MainF.SaveData();
                SysPara.items = 1;
            }
        }

		private void btPath_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog dilog = new FolderBrowserDialog();
			dilog.Description = "Please select the Panel Save folder";
			if (dilog.ShowDialog() == DialogResult.OK || dilog.ShowDialog() == DialogResult.Yes)
			{
				Button tb = sender as Button;
				string TextName = tb.Name.Replace("bt", "").Replace("Path", "");
				TextName = "txt" + TextName;
				
				foreach (Control control in Controls)
				{
					if ((string)control.Name == TextName)
					{
						control.Text = dilog.SelectedPath;
						control.Focus();
					}
				}
					
			}
			SysPara.items++;
		}

		private void LogForm_Load(object sender, EventArgs e)
		{

		}
	}
}
