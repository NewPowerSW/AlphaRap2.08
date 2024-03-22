using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AlphaRap.MainForm;
using System.Threading;
using AlphaRap.Classes;

namespace AlphaRap
{
    public partial class UserLoginForm : Form
    {
        //  private AxZKFPEngXControl.AxZKFPEngX axZKFPEngX1;
        public bool str1 = false;
        private Task CheckCaptureTask;
        private CancellationTokenSource StopTask = new CancellationTokenSource();
        public UserLoginForm()
        {
            switch (SysPara.LanguageShow)
            {
                case LanguageType.Chinese:
                    Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("zh-CN");
                    break;
                case LanguageType.English:
                    Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
                    break;
            }
            InitializeComponent();
        }
        public bool ReadUserData(string UserName, string Password, ref PermissionType UserPermission)
        {
            try
            {
                string strSQL = "select * from UserData where UserName = '" + UserName + "'";
                bool Successful = false;
                DataTable readData = DataBase.ReadData_Adapter(SysPara.MdbPath, strSQL, ref Successful);
                if (Successful)
                    if (readData.Rows.Count > 0)
                    {
                        if (readData.Rows[0]["Password"].ToString() == Password)
                        {
                            UserPermission = (PermissionType)Enum.Parse(typeof(PermissionType), readData.Rows[0]["Permission"].ToString());
                            return true;
                        }
                        else
                        {
                            switch (SysPara.LanguageShow)
                            {
                                case LanguageType.Chinese:
                                    MessageBox.Show("登录失败！密码错误！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    break;
                                case LanguageType.English:
                                    MessageBox.Show("Login Fail！Please check your Password!", "Note", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    break;
								case LanguageType.Español:
									MessageBox.Show("¡El inicio de sesión falló! ¡Error de contraseña!", "Consejo", MessageBoxButtons.OK, MessageBoxIcon.Error);
									break;
							}
                            textPassword.Focus();
                        }
                    }
                    else
                    {
                        switch (SysPara.LanguageShow)
                        {
                            case LanguageType.Chinese:
                                MessageBox.Show("登录失败！用户名不存在！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;
                            case LanguageType.English:
                                MessageBox.Show("Login Fail！Please check your UserName!", "Note", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;
							case LanguageType.Español:
								MessageBox.Show("¡El inicio de sesión falló! ¡Error de contraseña!", "Consejo", MessageBoxButtons.OK, MessageBoxIcon.Error);
								break;
						}
                        textUserName.Focus();
                    }
            }
            catch (Exception) { }
            return false;
        }
        private void textPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
                btnLogin.PerformClick();
        }

        public PermissionType UserPermission = PermissionType.None;
        private void btnLogin_Click(object sender, EventArgs e)
        {

            if (ReadUserData(textUserName.Text, textPassword.Text, ref UserPermission))
            {
                SysPara.UserName = textUserName.Text;
                SysPara.UserLoginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                MiddleLayer.SwitchPermission(UserPermission);
            
                textUserName.Text = "";
                textPassword.Text = "";
                SysPara.bLogin = false;
                this.Close();
               
                

                SysPara.bLogin = true;
            }
        }

        private void UserLoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            textUserName.Text = "";
            textPassword.Text = "";
            Labl_FingerPrintInfo.Text = "";
            if (SysPara.bLogin)
            {
                SysPara.UserName = MiddleLayer.AddF.ReadAllUserData();
                SysPara.UserPermission = PermissionType.Operator;
                MiddleLayer.MainF.SwitchPermission(SysPara.UserPermission);
                MiddleLayer.MainF.SwitchMainPage(MENU_PageType.Home);
                MiddleLayer.MainF.RefreshMenuBackcolor();
                SysPara.bLogin = false;
            }
        }

        private void UserLoginForm_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                if (SysPara.UseFingerprint)
                {
                    ResetFingerprintDB();
                    MiddleLayer.FingerprintCaptureF.StartVerification();
                    //CheckHaveNewCapture();
                }
            }
            else
            {
                if (SysPara.UseFingerprint)
                {
                    MiddleLayer.FingerprintCaptureF.StopVerification();
                    //StopTask.Cancel();
                }
            }
        }
        public void ResetFingerprintDB()
        {
            MiddleLayer.FingerprintCaptureF.ClearTemplate();
            string strSQL = "select * from UserData";
            bool Successful = false;
            DataTable readData = DataBase.ReadData_Adapter(SysPara.MdbPath, strSQL, ref Successful);
            if (Successful)
                for (int i = 0; i < readData.Rows.Count; i++)
                {
                    string Name = readData.Rows[i]["UserName"].ToString();
                    MiddleLayer.FingerprintCaptureF.AddTemplate(i, readData.Rows[i]["UserName"].ToString());
                }
        }
        public void OnTemplate(int UserIndex)
        {
            string strSQL = "select * from UserData";
            bool Successful = false;
            DataTable readData = DataBase.ReadData_Adapter(SysPara.MdbPath, strSQL, ref Successful);
            if (Successful)
            {
                RefreshDifferentThreadUI(textUserName, () =>
                {
                    textUserName.Text = readData.Rows[UserIndex]["UserName"].ToString();
                    textPassword.Text = readData.Rows[UserIndex]["Password"].ToString();
                    btnLogin.PerformClick();
                });
            }
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





        private void UserLoginForm_Load(object sender, EventArgs e)
        {
            if (MiddleLayer.FingerprintCaptureF.InitFingerprintF())
            {
                UserLog.Parent = null;
            }
            else
            {
                FingerPrintLog.Parent = null;
            }
            if (SysPara.UseFingerprint)
                MiddleLayer.FingerprintCaptureF.OnTemplate += this.OnTemplate;
            MiddleLayer.FingerprintCaptureF.FingerPrinfInfo += this.FingerPrinfInfo;


            ReadAllUserData();

        }



        public string ReadAllUserData()
        {

            string gUser = "None";

            try
            {
                string strSQL = "select UserName,Permission from UserData";
                bool Successful = false;

                DataTable readData = DataBase.ReadData_Adapter(SysPara.MdbPath, strSQL, ref Successful);
                //DataColumn FillCol = new DataColumn();
                //readData.Columns.Add(FillCol);
                if (Successful)
                {
                    dgvUserList.DataSource = readData;
                    dgvUserList.Columns[0].Width = 250;
                    dgvUserList.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

            return gUser;
        }









        //建立一个方法用来把窗体2值传给窗体1
        public void FingerPrinfInfo(string strInfo)
        {
            RefreshDifferentThreadUI(Labl_FingerPrintInfo, () =>
            {
                // Labl_FingerPrintInfo.Text = "";
                Labl_FingerPrintInfo.Text = strInfo;
            });
        }

        private void UserLog_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void textUserName_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            FingerPrintLog.Parent = null;
            UserLog.Parent = FingerPrintLogin;
        }

        private void dgvUserList_SelectionChanged(object sender, EventArgs e)
        {

            if (dgvUserList.CurrentCell != null)
            {
                if (dgvUserList.CurrentCell.RowIndex >= 0)
                {
                    textUserName.Text = dgvUserList[0, dgvUserList.CurrentCell.RowIndex].Value.ToString();

                }
            }


        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            
        }
    }
}
