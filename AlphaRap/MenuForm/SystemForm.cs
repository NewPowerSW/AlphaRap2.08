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
    public partial class SystemForm : Form
    {
        public SystemForm()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 呈现参数页面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btParameter_Click(object sender, EventArgs e)
        {
            labParameter.BackColor = Color.Green; 
            labPlat.BackColor = Color.FromArgb(4, 108, 182);
            labSystem.BackColor = Color.FromArgb(4, 108, 182);
            ShowhMainPage(MiddleLayer.ParF);
        }
        /// <summary>
        /// 呈现平台页面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btPlat_Click(object sender, EventArgs e)
        {
		

			labPlat.BackColor = Color.Green;
            labParameter.BackColor = Color.FromArgb(4, 108, 182);
            labSystem.BackColor = Color.FromArgb(4, 108, 182);
            ShowhMainPage(MiddleLayer.PlatF);
        }
        /// <summary>
        /// 呈现系统设置页面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btSystem_Click(object sender, EventArgs e)
        {
            labSystem.BackColor = Color.Green; 
            labPlat.BackColor = Color.FromArgb(4, 108, 182);
            labParameter.BackColor = Color.FromArgb(4, 108, 182);
            ShowhMainPage(MiddleLayer.SystemS);

        }
        /// <summary>
        /// 页面呈现
        /// </summary>
        /// <param name="ShowPage"></param>
        private void ShowhMainPage(dynamic ShowPage)
        {
            SystemGroup.Focus();
            foreach (Control control in SystemGroup.Controls)
            {
                control.Parent = null;
                control.Visible = false;
            }
            if (ShowPage.GetType().IsSubclassOf(typeof(Form)))
            {
                ShowPage.TopLevel = false;
                ShowPage.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                ShowPage.WindowState = FormWindowState.Maximized;
                ShowPage.Dock = DockStyle.Fill;
            }
            else
                ShowPage.Dock = DockStyle.Fill;
            ShowPage.Parent = SystemGroup;
            ShowPage.Show();
        }

        private void SystemForm_Load(object sender, EventArgs e)
        {
            labParameter.BackColor = Color.FromArgb(4, 108, 182);
         
            ShowhMainPage(MiddleLayer.ParF);
        }

        private void label1_Click(object sender, EventArgs e)
        {
			MenuForm.LanguageSetting FLS = new MenuForm.LanguageSetting();
			FLS.Show();
		}

        private void label2_Click(object sender, EventArgs e)
        {
            MenuForm.AlarmSetting FRM = new MenuForm.AlarmSetting();
            FRM.Show();
        }
    }
}
