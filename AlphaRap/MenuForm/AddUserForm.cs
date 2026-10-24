
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using AlphaRap.Configuration;

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
        private DataTable _savedPermissions;
        private DataTable _pendingPermissions;
        private Exception _permissionLoadError;
        private bool _bindingPermissions;

        public AddUserForm()
        {
            InitializeComponent();
            UiTheme.ApplyDpi(this);
            cbSelectedPermission.FormattingEnabled = true;
            cbAddUserPermission.FormattingEnabled = true;
            cbSelectedPermission.Format += delegate(object sender, ListControlConvertEventArgs e) { e.Value = MiddleLayer.PermissionText(e.ListItem); };
            cbAddUserPermission.Format += delegate(object sender, ListControlConvertEventArgs e) { e.Value = MiddleLayer.PermissionText(e.ListItem); };
            dgvUserList.CellFormatting += PermissionCellFormatting;
            dgvRightsConfig.CellFormatting += PermissionCellFormatting;
            dgvUserList.DataBindingComplete += delegate { ApplyLanguage(); };
            dgvRightsConfig.DataBindingComplete += delegate { ApplyLanguage(); };

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
            Disposed += delegate { MiddleLayer.LanguageChanged -= AddUserForm_LanguageChanged; };

            // 三个输入框和权限下拉框任意一个变化时，重新判断"新增用户"按钮是否可用
            textAddUserName.ValueChanged += AddUser_TextChanged;
            textPassword.ValueChanged += AddUser_TextChanged;
            textPasswordConfirm.ValueChanged += AddUser_TextChanged;
            cbAddUserPermission.SelectedIndexChanged += AddUser_TextChanged;

            UiKit.StyleGrid(dgvUserList);
            UiKit.StyleGrid(dgvRightsConfig);

            // 设计期不访问数据库：VS 设计器会执行构造函数，连着库失败会让设计面加载异常，
            // 进而使 VS 回写 Designer 时丢掉控件的父子挂载（Controls.Add），表格变成孤儿控件。
            if (!InDesigner)
            {
                ReadAllUserData();
                ReadPermission();
            }
        }

        /// <summary>是否运行在 VS 设计器中（设计期不访问数据库）。</summary>
        private static bool InDesigner
        {
            get
            {
                try { return LicenseManager.UsageMode == LicenseUsageMode.Designtime; }
                catch { return false; }
            }
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

        /// <summary>为用户管理操作应用统一图标。</summary>
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
            }
            catch { }
        }

        /// <summary>刷新权限提示和角色显示文案，不重建或更改正在编辑的权限数据。</summary>
        private void ApplyLanguage()
        {
            try
            {
                // 保存权限后用于还原的提示文字；静态文案见 LanguageData\*.xml 的 AddUserForm 段
                _rightsHintText = MiddleLayer.LangMsg("AddUserForm", "msg_RightsMainSaveHint",
                    "勾选各权限允许的功能后，使用主界面【保存】统一保存。",
                    "Select the allowed functions, then use Save on the main window.",
                    "Seleccione las funciones permitidas y use Guardar en la ventana principal.");
                ResetRightsHint();
                MiddleLayer.RefreshUserHeaders(dgvUserList);
                MiddleLayer.RefreshUserHeaders(dgvRightsConfig);
                cbSelectedPermission.FormatInfo = System.Globalization.CultureInfo.GetCultureInfo(
                    SysPara.LanguageShow == LanguageType.Chinese ? "zh-CN" : SysPara.LanguageShow == LanguageType.English ? "en-US" : "es-ES");
                cbAddUserPermission.FormatInfo = cbSelectedPermission.FormatInfo;
            }
            catch { }
        }

        /// <summary>仅翻译权限代码的显示值，保留数据库记录供增删改查使用。</summary>
        private void PermissionCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            DataGridView grid = (DataGridView)sender;
            if (e.ColumnIndex < 0 || grid.Columns[e.ColumnIndex].DataPropertyName != "Permission") return;
            e.Value = MiddleLayer.PermissionText(e.Value);
            e.FormattingApplied = true;
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
                    // 列由 DataSource 自动生成；没挂进窗体时不会生成列，先确认列数再按列设样式
                    if (dgvUserList.Columns.Count >= 3)
                    {
                        dgvUserList.Columns[0].Width = 250;
                        dgvUserList.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        dgvUserList.Columns[2].Width = 0;
                        dgvUserList.Columns[2].HeaderText = "";
                    }

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
                MessageBox.Show(ex.Message, MiddleLayer.LangMsg("Common", "msg_SystemTitle", "系统提示", "System message", "Mensaje del sistema"), MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            OperationLog.Write("AddUserForm", "修改用户权限：" + textSelectedUserName.Value
                + " → " + cbSelectedPermission.SelectedItem);
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
            OperationLog.Write("AddUserForm", "删除用户：" + textSelectedUserName.Value);
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
                OperationLog.Write("AddUserForm", "新增用户：" + textAddUserName.Value
                    + "（权限 " + cbAddUserPermission.SelectedItem + "）");
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

        /// <summary>启动时读取权限表；读取失败保留错误并阻止主保存覆盖数据库。</summary>
        private bool ReadPermission()
        {
            try
            {
                BindPermissionTable(ReadPermissionTable(PermissionDatabasePath));
                _permissionLoadError = null;
                return true;
            }
            catch (Exception error)
            {
                _permissionLoadError = error;
                return false;
            }
        }

        /// <summary>绑定完整权限草稿并记录已保存快照，保留角色和列的稳定业务键。</summary>
        private void BindPermissionTable(DataTable readData)
        {
            ValidatePermissionTable(readData);
            dgvRightsConfig.AutoGenerateColumns = true;
            _bindingPermissions = true;
            try { dgvRightsConfig.DataSource = readData; }
            finally { _bindingPermissions = false; }
            _savedPermissions = readData.Copy();
            _savedPermissions.AcceptChanges();
            _pendingPermissions = null;
            if (dgvRightsConfig.Columns.Count == 0) return;
            dgvRightsConfig.ClearSelection();
            dgvRightsConfig.Columns[0].ReadOnly = true;
            for (int i = 0; i < dgvRightsConfig.Columns.Count; i++)
            {
                dgvRightsConfig.Columns[i].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgvRightsConfig.Columns[i].Width = 90;
                if (i == 0)
                {
                    dgvRightsConfig.Columns[i].Width = 160;
                }
            }
        }

        /// <summary>权限与用户账户共用的机器数据库路径；权限保存不会单独另建数据库。</summary>
        private static string PermissionDatabasePath
        {
            get { return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SysPara.MdbPath)); }
        }

        /// <summary>读取权限映射；连接与适配器始终释放，文件损坏或表不可读时抛出具体错误。</summary>
        private static DataTable ReadPermissionTable(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException(PermissionText("msg_RightsDatabaseMissing",
                "权限数据库不存在。", "The permissions database does not exist.", "La base de datos de permisos no existe."), path);
            DataTable table = new DataTable("PermissionSetup");
            using (OleDbConnection connection = OpenPermissionConnection(path))
            using (OleDbDataAdapter adapter = new OleDbDataAdapter("SELECT * FROM [PermissionSetup]", connection))
                adapter.Fill(table);
            ValidatePermissionTable(table);
            return table;
        }

        /// <summary>打开无连接池的短期 ACE 连接，关闭后允许事务替换或删除数据库文件。</summary>
        private static OleDbConnection OpenPermissionConnection(string path)
        {
            OleDbConnectionStringBuilder settings = new OleDbConnectionStringBuilder();
            settings.Provider = "Microsoft.ACE.OLEDB.12.0";
            settings.DataSource = path;
            settings["OLE DB Services"] = -2;
            OleDbConnection connection = new OleDbConnection(settings.ConnectionString);
            try { connection.Open(); return connection; }
            catch { connection.Dispose(); throw; }
        }

        /// <summary>校验角色唯一且每个功能值均为布尔量，禁止把不完整权限草稿写入数据库。</summary>
        private static void ValidatePermissionTable(DataTable table)
        {
            string invalid = PermissionText("msg_RightsInvalidTable", "权限表不完整或包含无效值。",
                "The permissions table is incomplete or contains invalid values.", "La tabla de permisos está incompleta o contiene valores no válidos.");
            if (table == null || table.Rows.Count == 0 || !table.Columns.Contains("Permission") || table.Columns.Count < 2)
                throw new InvalidOperationException(invalid);
            if (table.Columns["Permission"].DataType != typeof(string)) throw new InvalidOperationException(invalid);
            foreach (DataColumn column in table.Columns)
                if (column.ColumnName != "Permission" && column.DataType != typeof(bool)) throw new InvalidOperationException(invalid);
            HashSet<string> roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (DataRow row in table.Rows)
            {
                if (row.RowState == DataRowState.Deleted) throw new InvalidOperationException(invalid);
                string role = Convert.ToString(row["Permission"]);
                if (string.IsNullOrWhiteSpace(role) || !roles.Add(role)) throw new InvalidOperationException(invalid);
                foreach (DataColumn column in table.Columns)
                    if (row.IsNull(column)) throw new InvalidOperationException(invalid);
            }
        }

        /// <summary>比较权限业务键及功能值，不把行序或显示语言变化视为配置修改。</summary>
        private static bool PermissionsEqual(DataTable left, DataTable right)
        {
            if (left == null || right == null || left.Rows.Count != right.Rows.Count || left.Columns.Count != right.Columns.Count) return false;
            Dictionary<string, DataRow> rows = new Dictionary<string, DataRow>(StringComparer.OrdinalIgnoreCase);
            foreach (DataRow row in right.Rows) rows.Add(Convert.ToString(row["Permission"]), row);
            foreach (DataRow row in left.Rows)
            {
                DataRow other;
                if (!rows.TryGetValue(Convert.ToString(row["Permission"]), out other)) return false;
                foreach (DataColumn column in left.Columns)
                    if (!right.Columns.Contains(column.ColumnName) || !Equals(row[column], other[column.ColumnName])) return false;
            }
            return true;
        }

        /// <summary>捕获已结束编辑的权限草稿；只有映射确实变化时生成更新数据库副本的主保存任务。</summary>
        internal ProjectConfiguration.PendingFile PreparePermissionSave()
        {
            _pendingPermissions = null;
            if (_savedPermissions == null || _permissionLoadError != null)
                throw new InvalidOperationException(PermissionText("msg_RightsNotLoaded", "权限表尚未成功加载，无法保存。",
                    "The permissions table has not loaded successfully and cannot be saved.",
                    "La tabla de permisos no se ha cargado correctamente y no se puede guardar."), _permissionLoadError);
            if (!dgvRightsConfig.EndEdit()) throw new InvalidOperationException(PermissionText("msg_RightsInvalidEdit",
                "请先完成权限表中正在编辑的值。", "Finish editing the permissions value first.", "Termine de editar el valor de permisos."));
            DataTable table = dgvRightsConfig.DataSource as DataTable;
            if (table != null) BindingContext[table].EndCurrentEdit();
            ValidatePermissionTable(table);
            DataTable snapshot = table.Copy();
            if (PermissionsEqual(snapshot, _savedPermissions)) return null;
            string source = PermissionDatabasePath;
            _pendingPermissions = snapshot;
            return new ProjectConfiguration.PendingFile { Path = source, Write = path => WritePermissionDraft(source, path, snapshot) };
        }

        /// <summary>复制当前数据库后仅在副本的单个事务内更新权限映射，用户账户与其他表保持原内容。</summary>
        private static void WritePermissionDraft(string source, string target, DataTable snapshot)
        {
            try
            {
                File.Copy(source, target, false);
                using (OleDbConnection connection = OpenPermissionConnection(target))
                using (OleDbTransaction transaction = connection.BeginTransaction())
                {
                    foreach (DataRow row in snapshot.Rows) UpdatePermissionRow(connection, transaction, snapshot.Columns, row);
                    transaction.Commit();
                }
            }
            catch (Exception error)
            {
                throw new InvalidOperationException(PermissionText("msg_RightsStageFailed", "无法准备权限配置，正式数据库未更新。",
                    "Cannot prepare the permissions configuration; the original database was not updated.",
                    "No se puede preparar la configuración de permisos; la base de datos original no se ha actualizado."), error);
            }
        }

        /// <summary>按稳定角色键参数化更新全部功能列；不存在或重复的角色记录使整个事务失败。</summary>
        private static void UpdatePermissionRow(OleDbConnection connection, OleDbTransaction transaction, DataColumnCollection columns, DataRow row)
        {
            using (OleDbCommand command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                List<string> assignments = new List<string>();
                foreach (DataColumn column in columns)
                {
                    if (column.ColumnName == "Permission") continue;
                    assignments.Add("[" + column.ColumnName.Replace("]", "]]") + "] = ?");
                    command.Parameters.Add("p" + command.Parameters.Count, OleDbType.Boolean).Value = row[column];
                }
                command.Parameters.Add("role", OleDbType.VarWChar, 255).Value = row["Permission"];
                command.CommandText = "UPDATE [PermissionSetup] SET " + string.Join(", ", assignments) + " WHERE [Permission] = ?";
                if (command.ExecuteNonQuery() != 1)
                    throw new InvalidOperationException(PermissionText("msg_RightsRoleChanged", "权限角色不存在或出现重复，请重新加载权限表。",
                        "A permissions role is missing or duplicated. Reload the permissions table.",
                        "Falta un rol de permisos o está duplicado. Vuelva a cargar la tabla de permisos."));
            }
        }

        /// <summary>主保存全部成功后接受已写入的权限快照；不执行数据库写入。</summary>
        internal void AcceptPermissionSave()
        {
            if (_pendingPermissions == null) return;
            _savedPermissions = _pendingPermissions.Copy();
            _savedPermissions.AcceptChanges();
            DataTable table = dgvRightsConfig.DataSource as DataTable;
            if (table != null && PermissionsEqual(table, _savedPermissions)) table.AcceptChanges();
            _pendingPermissions = null;
            ResetRightsHint();
        }

        /// <summary>预读完整的已保存权限映射，返回只恢复内存草稿的动作；读取失败不改变当前表格。</summary>
        internal Action PreparePermissionDiscard()
        {
            DataTable table = ReadPermissionTable(PermissionDatabasePath);
            return delegate
            {
                BindPermissionTable(table);
                _permissionLoadError = null;
                ResetRightsHint();
            };
        }

        /// <summary>获取权限配置提示的三语文案，业务角色与列名不参与翻译。</summary>
        private static string PermissionText(string key, string chinese, string english, string spanish)
        {
            return MiddleLayer.LangMsg("AddUserForm", key, chinese, english, spanish);
        }

        /// <summary>记录权限编辑状态，配置由主界面保存或放弃。</summary>
        private void PermissionValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_bindingPermissions || _savedPermissions == null || e.RowIndex < 0 || e.ColumnIndex < 0) return;
            SysPara.items++;
            ResetRightsHint();
        }

        /// <summary>及时提交勾选值以标记权限草稿；编辑过程中不写数据库。</summary>
        private void PermissionCellDirtyChanged(object sender, EventArgs e)
        {
            if (_bindingPermissions || !dgvRightsConfig.IsCurrentCellDirty) return;
            dgvRightsConfig.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        /// <summary>离开权限编辑页时沿用主保存确认；失败或取消保留修改标记。</summary>
        private void AddUserForm_Leave(object sender, EventArgs e)
        {
            if (SysPara.items != 1)
            {
                MiddleLayer.MainF.SaveData();
            }
        }
    }
}
