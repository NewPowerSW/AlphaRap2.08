
using System;
using System.Windows.Forms;
using AlphaRapLibrary;

namespace AlphaRap
{
    public partial class LogForm : ModuleBaseForm
    {
        public LogForm()
        {
            InitializeComponent();
            if (!UiTheme.IsDesignTime) MiddleLayer.RegisterAndApplyLanguage(this, Name);
            UiTheme.ApplyDpi(this);
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
        /// <summary>离开页面时由主界面确认保存；取消或失败保留未保存标记。</summary>
        private void MESForm_Leave(object sender, EventArgs e)
        {
            if (SysPara.items != 1)
            {
                txtUser.Focus();
                MiddleLayer.MainF.SaveData();
            }
        }

		private void btPath_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog dilog = new FolderBrowserDialog();
			dilog.Description = MiddleLayer.LangMsg("LogForm", "msg_SelectFolder", "请选择日志保存目录", "Select the log folder", "Seleccione la carpeta de registros");
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
	}
}
