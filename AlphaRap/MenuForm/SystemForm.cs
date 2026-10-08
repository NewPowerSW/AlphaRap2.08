using System;
using System.Drawing;
using System.Windows.Forms;


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
                // 嵌入的页面保持 Normal 状态，由 Dock = Fill 随容器缩放
                ShowPage.WindowState = FormWindowState.Normal;
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
