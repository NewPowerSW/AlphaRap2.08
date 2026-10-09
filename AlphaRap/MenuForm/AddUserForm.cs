
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace AlphaRap
{
    /// <summary>
    /// 用户管理页（嵌入 MainForm 的单例页面，通过 MiddleLayer.AddF 访问）：用户列表、新增 / 修改 / 删除用户、权限设置，
    /// 三块为白色圆角卡片，随宿主面板缩放。
    /// </summary>
    public partial class AddUserForm : Form
    {
        /// <summary>权限提示的基准文案，用于把保存结果清掉后恢复。</summary>
        private string _rightsHintText = "";

        public AddUserForm()
        {
            InitializeComponent();

            foreach (var item in Enum.GetValues(typeof(PermissionType)))
            {
                if (item.ToString() != "None")
                {
                    cbSelectedPermission.Items.Add(item);
                    cbAddUserPermission.Items.Add(item);
                }
            }
            cbSelectedPermission.SelectedIndex = 0;
            cbAddUserPermission.SelectedIndex = 0;

            ApplyIcons();
            ApplyLanguage();

            // 切语言时重新套一次本页文案（理由同 ProductManagerForm：
            // 页面正显示着时切语言不会触发 VisibleChanged）。
            MiddleLayer.LanguageChanged += AddUserForm_LanguageChanged;

            // 三个输入框和权限下拉框任意一个变化时，重新判断"新增用户"按钮是否可用
            textAddUserName.ValueChanged += AddUser_TextChanged;
            textPassword.ValueChanged += AddUser_TextChanged;
            textPasswordConfirm.ValueChanged += AddUser_TextChanged;
            cbAddUserPermission.SelectedIndexChanged += AddUser_TextChanged;

            UiKit.StyleGrid(dgvUserList);
            UiKit.StyleGrid(dgvRightsConfig);

            ReadAllUserData();
            ReadPermission();
        }

        /// <summary>
        /// 页面每次显示时重新套一次文案。
        /// 必须这么做：本页在启动阶段就被创建（MiddleLayer.InitialProject 里 CreateForm），
        /// 早于语言表扫描，它注册进 XML 的文案只会是创建那一刻的语言；
        /// 之后切换语言虽然会应用 XML 里的旧文案，但本页一显示就会被这里纠正过来。
        /// </summary>
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible) ApplyLanguage();
        }

        /// <summary>语言切换完成 → 重套本页文案。</summary>
        private void AddUserForm_LanguageChanged(object sender, EventArgs e)
        {
            ApplyLanguage();
        }

        // ==================== 外观 ====================

        private void ApplyIcons()
        {
            try
            {
                picPageIcon.Image = AppIcons.Get(AppIcon.AddUser, 34, UiKit.Brand);

                textSelectedUserName.Icon = AppIcons.Get(AppIcon.Login, 20, UiKit.TextMuted);
                textAddUserName.Icon = AppIcons.Get(AppIcon.Login, 20, UiKit.TextMuted);
                textPassword.Icon = AppIcons.Get(AppIcon.Lock, 20, UiKit.TextMuted);
                textPasswordConfirm.Icon = AppIcons.Get(AppIcon.Lock, 20, UiKit.TextMuted);

                btnModifyPermission.Icon = AppIcons.Get(AppIcon.Edit, 18, UiKit.TextPrimary);
                btnDeleteUser.Icon = AppIcons.Get(AppIcon.Trash, 18, Color.White);
                btnAddUser.Icon = AppIcons.Get(AppIcon.Plus, 18, Color.White);
                btnSaveRights.Icon = AppIcons.Get(AppIcon.Save, 18, Color.White);
            }
            catch { }
        }

        private void ApplyLanguage()
        {
            try
            {
                // 保存权限后用于还原的提示文字；静态文案见 LanguageData\*.xml 的 AddUserForm 段
                _rightsHintText = MiddleLayer.LangMsg("AddUserForm", "msg_RightsHint",
                    "勾选各权限允许的功能后点保存", "Tick the allowed functions, then save", "Marque las funciones permitidas y guarde");
                ResetRightsHint();
            }
            catch { }
        }

        private void ResetRightsHint()
        {
            try
            {
                lblRightsHint.Text = _rightsHintText;
                lblRightsHint.ForeColor = UiKit.TextMuted;
            }
            catch { }
        }

        // ==================== 用户列表 ====================

        public string ReadAllUserData()
        {
            string gUser = "None";

            try
            {
                string strSQL = "select UserName,Permission from UserData";
                bool Successful = false;

                DataTable readData = DataBase.ReadData_Adapter(SysPara.MdbPath, strSQL, ref Successful);
                DataColumn FillCol = new DataColumn();
                readData.Columns.Add(FillCol);
                if (Successful)
                {
                    dgvUserList.DataSource = readData;
                    dgvUserList.Columns[0].Width = 250;
                    dgvUserList.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    dgvUserList.Columns[2].Width = 0;
                    dgvUserList.Columns[2].HeaderText = "";

                    for (int i = 0; i < readData.Rows.Count; i++)
                    {
                        DataRow dr = readData.Rows[i];

                        if ((string)dr[1] == "Operator")
                        {
                            gUser = (string)dr[0];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "System Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return gUser;
        }

        private void dgvUserList_CurrentCellChanged(object sender, EventArgs e)
        {
            if (dgvUserList.CurrentCell != null)
            {
                if (dgvUserList.CurrentCell.RowIndex >= 0)
                {
                    int tmpIndex = Array.IndexOf(Enum.GetNames(typeof(PermissionType)),
                        dgvUserList[1, dgvUserList.CurrentCell.RowIndex].Value.ToString());
                    cbSelectedPermission.SelectedIndex = tmpIndex;
                    textSelectedUserName.Value = dgvUserList[0, dgvUserList.CurrentCell.RowIndex].Value.ToString();
                    btnDeleteUser.Enabled = true;
                    btnModifyPermission.Enabled = true;
                    return;
                }
            }
            cbSelectedPermission.SelectedIndex = -1;
            textSelectedUserName.Value = "";
            btnDeleteUser.Enabled = false;
            btnModifyPermission.Enabled = false;
        }

        private void btnModifyPermission_Click(object sender, EventArgs e)
        {
            if (cbSelectedPermission.SelectedItem == null) return;
            ModifyUserPermission(textSelectedUserName.Value, cbSelectedPermission.SelectedItem.ToString());
            ReadAllUserData();
        }

        public bool ModifyUserPermission(string UserName, string UserPermission)
        {
            try
            {
                string strSQL = "update [UserData] set [Permission]='" + UserPermission + "' where [UserName] ='" + UserName + "'";
                int result = DataBase.DataBaseExecute(SysPara.MdbPath, strSQL);
                if (result == 0)
                    return true;
            }
            catch (Exception) { }
            return false;
        }

        private void btnDeleteUser_Click(object sender, EventArgs e)
        {
            DeleteUser(textSelectedUserName.Value);
            ReadAllUserData();
        }

        private bool DeleteUser(string UserName)
        {
            try
            {
                string strSQL = "delete * from UserData where UserName ='" + UserName + "'";
                int result = DataBase.DataBaseExecute(SysPara.MdbPath, strSQL);
                if (result == 0)
                    return true;
            }
            catch (Exception) { }
            return false;
        }

        // ==================== 新增用户 ====================

        private void btnAddUser_Click(object sender, EventArgs e)
        {
            if (cbAddUserPermission.SelectedItem == null) return;

            if (CheckUserWhetherExist(textAddUserName.Value))
            {
                MessageBox.Show(
                    MiddleLayer.LangMsg("AddUserForm", "msg_UserExists", "用户名已经存在！", "The user name already exist!", "¡El nombre de usuario ya existe!"),
                    MiddleLayer.LangMsg("Common", "msg_NoteTitle", "提示", "Note", "Consejo"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                textAddUserName.FocusInput();
                return;
            }

            if ((textPassword.Value.Length < 8) && cbAddUserPermission.SelectedItem.ToString() != ((PermissionType.Operator).ToString()))
            {
                MessageBox.Show(
                    MiddleLayer.LangMsg("AddUserForm", "msg_PasswordShort", "密码少于8个字符！", "Password less than 8 characters!", "Contraseña de menos de 8 caracteres!"),
                    MiddleLayer.LangMsg("Common", "msg_NoteTitle", "提示", "Note", "Consejo"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                textPassword.FocusInput();
                return;
            }
            if (textPassword.Value != textPasswordConfirm.Value && cbAddUserPermission.SelectedItem.ToString() != ((PermissionType.Operator).ToString()))
            {
                MessageBox.Show(
                    MiddleLayer.LangMsg("AddUserForm", "msg_PasswordMismatch", "密码与密码确认不匹配！", "Password not match!", "¡La contraseña y la contraseña no coinciden!"),
                    MiddleLayer.LangMsg("Common", "msg_NoteTitle", "提示", "Note", "Consejo"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                textPassword.FocusInput();
                return;
            }

            int TmpIndex = dgvUserList.Rows.Count;
            if (AddUser(textAddUserName.Value, textPassword.Value, cbAddUserPermission.SelectedItem.ToString()))
            {
                ReadAllUserData();
                textAddUserName.Value = string.Empty;
                textPassword.Value = string.Empty;
                textPasswordConfirm.Value = string.Empty;
                AddUser_TextChanged(null, EventArgs.Empty);
                if (TmpIndex < dgvUserList.Rows.Count)
                    dgvUserList[0, TmpIndex].Selected = true;
            }
        }

        private bool CheckUserWhetherExist(string UserName)
        {
            string strSQL = "select * from UserData where UserName ='" + UserName + "'";
            bool Successful = false;

            DataTable readData = DataBase.ReadData_Adapter(SysPara.MdbPath, strSQL, ref Successful);
            if (Successful)
                if (readData.Rows.Count > 0)
                    return true;
            return false;
        }

        private bool AddUser(string UserName, string UserPassword, string UserLevel)
        {
            try
            {
                string strSQL = "insert into UserData values ('" + UserName + "','" + UserPassword + "','" + UserLevel + "')";
                int result = DataBase.DataBaseExecute(SysPara.MdbPath, strSQL);
                if (result == 0)
                    return true;
            }
            catch (Exception) { }
            return false;
        }

        private void AddUser_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (cbAddUserPermission.SelectedItem == null)
                {
                    btnAddUser.Enabled = false;
                    return;
                }

                bool nameReady = textAddUserName.Value != string.Empty;
                bool passwordReady = textPassword.Value != string.Empty && textPasswordConfirm.Value != string.Empty;
                bool operatorOnly = cbAddUserPermission.SelectedItem.ToString() == (PermissionType.Operator).ToString();

                btnAddUser.Enabled = (nameReady && passwordReady) || (nameReady && operatorOnly);
            }
            catch { btnAddUser.Enabled = false; }
        }

        // ==================== 权限设置 ====================

        private bool ReadPermission()
        {
            string strSQL = "select * from PermissionSetup";
            bool Successful = false;
            DataTable readData = DataBase.ReadData_Adapter(SysPara.MdbPath, strSQL, ref Successful);
            if (Successful)
            {
                if (readData.Rows.Count > 0)
                {
                    dgvRightsConfig.DataSource = readData;
                    dgvRightsConfig.ClearSelection();
                    dgvRightsConfig.Columns[0].ReadOnly = true;
                    for (int i = 0; i < readData.Columns.Count; i++)
                    {
                        dgvRightsConfig.Columns[i].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        dgvRightsConfig.Columns[i].Width = 90;
                        if (i == 0)
                        {
                            dgvRightsConfig.Columns[i].Width = 160;
                        }
                    }

                    return true;
                }
            }
            return false;
        }

        public bool WritePermission()
        {
            bool IsUserExist = false;
            for (int row = 0; row < dgvRightsConfig.Rows.Count; row++)
            {
                string strSQL = "update PermissionSetup set ";
                for (int column = 1; column < dgvRightsConfig.Columns.Count; column++)
                    strSQL += String.Format("[{0}]={1}{2}", dgvRightsConfig.Columns[column].Name,
                        dgvRightsConfig[column, row].Value, (column == (dgvRightsConfig.Columns.Count - 1)) ? "" : ",");
                strSQL += " where [Permission]= '" + dgvRightsConfig[0, row].Value.ToString() + "'";
                int result = DataBase.DataBaseExecute(SysPara.MdbPath, strSQL);
                if (result != 0)
                {
                    IsUserExist = false;
                    break;
                }
                else
                {
                    IsUserExist = true;
                }
            }
            return IsUserExist;
        }

        private void DataChange_Click(object sender, EventArgs e)
        {
            SysPara.items++;
            ResetRightsHint();
        }

        /// <summary>保存权限表（离开页面或退出程序时也会通过 MainForm.SaveData() 保存）。</summary>
        private void btnSaveRights_Click(object sender, EventArgs e)
        {
            bool ok = WritePermission();
            // 运行时提示语走语言包（三语，可在 LanguageData\AddUserForm 段里改）
            lblRightsHint.Text = ok
                ? MiddleLayer.LangMsg("AddUserForm", "msg_RightsSaved", "权限已保存", "Permissions saved", "Permisos guardados")
                : MiddleLayer.LangMsg("AddUserForm", "msg_RightsSaveFail", "保存失败，请检查数据库", "Save failed, check the database", "Error al guardar");
            lblRightsHint.ForeColor = ok ? UiKit.Success : UiKit.Danger;
        }

        private void AddUserForm_Leave(object sender, EventArgs e)
        {
            if (SysPara.items != 1)
            {
                MiddleLayer.MainF.SaveData();
                SysPara.items = 1;
            }
        }
    }
}
