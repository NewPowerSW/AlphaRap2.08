using System;
using System.Data;
using System.Data.OleDb;
using System.IO;

namespace AlphaRap.Security
{
    /// <summary>经过认证的账号身份，不携带密码或散列。</summary>
    public sealed class AccountIdentity
    {
        public string Name { get; private set; }
        public PermissionType Permission { get; private set; }
        /// <summary>建立通过数据库校验的身份。</summary>
        internal AccountIdentity(string name, PermissionType permission) { Name = name; Permission = permission; }
    }

    /// <summary>账号查询与凭据迁移，所有业务输入通过参数传递。</summary>
    public sealed class AccountService
    {
        private readonly string path;
        private static readonly object schemaSync = new object();
        /// <summary>与权限配置继续使用同一个机器数据库。</summary>
        public AccountService(string databasePath) { path = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, databasePath)); }

        /// <summary>校验凭据，旧格式只有在成功验证后才迁移并清除原密码字段。</summary>
        public AccountIdentity Authenticate(string name, string password)
        {
            using (OleDbConnection connection = Open())
            {
                bool hashed = HasHashColumn(connection);
                DataTable users = Read(connection, "SELECT [UserName],[Password],[Permission]" + (hashed ? ",[PasswordHash]" : "") + " FROM [UserData] WHERE [UserName]=?", name);
                if (users.Rows.Count != 1) return null;
                DataRow row = users.Rows[0];
                AccountIdentity identity = Identity(row);
                if (identity == null) return null;
                string stored = hashed ? Convert.ToString(row["PasswordHash"]) : "";
                if (stored.Length > 0) return PasswordCredentials.Verify(password, stored) ? identity : null;
                string legacy = Convert.ToString(row["Password"]);
                if (!PasswordCredentials.VerifyLegacy(password, legacy)) return null;
                if (Configuration.ProjectConfiguration.RecoveryRequired) throw new InvalidOperationException(AccountText.Changed);
                EnsureHashColumn(connection);
                string replacement = PasswordCredentials.Hash(password);
                using (OleDbTransaction transaction = connection.BeginTransaction())
                using (OleDbCommand command = Command(connection, transaction, "UPDATE [UserData] SET [PasswordHash]=?,[Password]=? WHERE [UserName]=? AND [Password]=? AND ([PasswordHash] IS NULL OR [PasswordHash]='')", replacement, "", name, legacy))
                {
                    if (command.ExecuteNonQuery() != 1) throw new InvalidOperationException(AccountText.Changed);
                    transaction.Commit();
                }
                return identity;
            }
        }

        /// <summary>列出公开身份，不读取凭据列；保留既有指纹列表的读取顺序。</summary>
        public DataTable Users()
        {
            using (OleDbConnection connection = Open()) return Read(connection, "SELECT [UserName],[Permission] FROM [UserData]");
        }

        /// <summary>读取指定角色的菜单权限，调用方验证应用所需的功能列。</summary>
        public DataTable ReadMenuPermissions(PermissionType permission)
        {
            using (OleDbConnection connection = Open())
                return Read(connection, "SELECT * FROM [PermissionSetup] WHERE [Permission]=?", permission.ToString());
        }

        /// <summary>可信指纹回调按已绑定用户名重新确认当前权限，不读取或回填密码。</summary>
        public AccountIdentity FindIdentity(string name)
        {
            using (OleDbConnection connection = Open())
            {
                DataTable users = Read(connection, "SELECT [UserName],[Permission] FROM [UserData] WHERE [UserName]=?", name);
                return users.Rows.Count == 1 ? Identity(users.Rows[0]) : null;
            }
        }

        /// <summary>按参数查询账号是否存在，不把输入解释为查询语法。</summary>
        public bool Exists(string name) { return FindIdentity(name) != null; }

        /// <summary>新增账号只保存哈希，兼容原操作员密码政策。</summary>
        public void Add(string name, string password, string permission)
        {
            RequireAdministrator();
            PermissionType role = ParsePermission(permission);
            if (string.IsNullOrWhiteSpace(name) || name.Length > 50) throw new ArgumentException(AccountText.InvalidName);
            if (role != PermissionType.Operator && (password ?? "").Length < 8) throw new ArgumentException(AccountText.PasswordShort);
            string hashed = PasswordCredentials.Hash(password);
            using (OleDbConnection connection = Open())
            {
                EnsureHashColumn(connection);
                using (OleDbTransaction transaction = connection.BeginTransaction())
                {
                    if (Read(connection, transaction, "SELECT [UserName] FROM [UserData] WHERE [UserName]=?", name).Rows.Count != 0) throw new InvalidOperationException(AccountText.Exists);
                    using (OleDbCommand command = Command(connection, transaction, "INSERT INTO [UserData] ([UserName],[Password],[Permission],[PasswordHash]) VALUES (?,?,?,?)", name, "", role.ToString(), hashed)) command.ExecuteNonQuery();
                    transaction.Commit();
                }
            }
        }

        /// <summary>权限修改由服务入口再次检查管理员身份。</summary>
        public void ChangePermission(string name, string permission)
        {
            RequireAdministrator();
            PermissionType role = ParsePermission(permission);
            using (OleDbConnection connection = Open())
            using (OleDbCommand command = Command(connection, null, "UPDATE [UserData] SET [Permission]=? WHERE [UserName]=?", role.ToString(), name))
                if (command.ExecuteNonQuery() != 1) throw new InvalidOperationException(AccountText.Changed);
        }

        /// <summary>删除账号使用固定语句和用户名参数。</summary>
        public void Delete(string name)
        {
            RequireAdministrator();
            using (OleDbConnection connection = Open())
            using (OleDbCommand command = Command(connection, null, "DELETE FROM [UserData] WHERE [UserName]=?", name))
                if (command.ExecuteNonQuery() != 1) throw new InvalidOperationException(AccountText.Changed);
        }

        /// <summary>只接受现有有效业务角色，不把未知枚举当作权限。</summary>
        private static PermissionType ParsePermission(string text)
        {
            PermissionType role;
            if (!Enum.TryParse(text, out role) || !Enum.IsDefined(typeof(PermissionType), role) || role == PermissionType.None) throw new ArgumentException(AccountText.InvalidRole);
            return role;
        }

        /// <summary>数据库角色无效时拒绝认证。</summary>
        private static AccountIdentity Identity(DataRow row)
        {
            try { return new AccountIdentity(Convert.ToString(row["UserName"]), ParsePermission(Convert.ToString(row["Permission"]))); }
            catch (ArgumentException) { return null; }
        }

        /// <summary>账号管理服务不依赖按钮是否可见来授权。</summary>
        private static void RequireAdministrator() { if (SysPara.UserPermission != PermissionType.Administrator) throw new UnauthorizedAccessException(AccountText.Denied); }

        /// <summary>缺少机器数据库时拒绝操作，不创建空替代数据库。</summary>
        private OleDbConnection Open()
        {
            if (!File.Exists(path)) throw new FileNotFoundException(AccountText.Missing, path);
            var builder = new OleDbConnectionStringBuilder { Provider = "Microsoft.ACE.OLEDB.12.0", DataSource = path };
            var connection = new OleDbConnection(builder.ConnectionString);
            try { connection.Open(); return connection; } catch { connection.Dispose(); throw; }
        }

        /// <summary>检测可选凭据列，不读取账号内容。</summary>
        private static bool HasHashColumn(OleDbConnection connection)
        {
            using (DataTable columns = connection.GetSchema("Columns", new[] { null, null, "UserData", "PasswordHash" })) return columns.Rows.Count > 0;
        }

        /// <summary>增加可容纳版本化哈希的列，保留既有表、短密码列和权限配置。</summary>
        private static void EnsureHashColumn(OleDbConnection connection)
        {
            lock (schemaSync)
            {
                if (HasHashColumn(connection)) return;
                using (var command = new OleDbCommand("ALTER TABLE [UserData] ADD COLUMN [PasswordHash] MEMO", connection)) command.ExecuteNonQuery();
            }
        }

        /// <summary>建立参数化命令，ACE 的问号参数严格按出现顺序添加。</summary>
        private static OleDbCommand Command(OleDbConnection connection, OleDbTransaction transaction, string sql, params string[] values)
        {
            var command = new OleDbCommand(sql, connection, transaction);
            for (int i = 0; i < values.Length; i++) command.Parameters.Add("p" + i, OleDbType.VarWChar, Math.Max(1, (values[i] ?? "").Length)).Value = values[i] ?? "";
            return command;
        }

        /// <summary>读取参数化查询并释放适配器。</summary>
        private static DataTable Read(OleDbConnection connection, string sql, params string[] values) { return Read(connection, null, sql, values); }
        /// <summary>在指定事务中读取账号快照。</summary>
        private static DataTable Read(OleDbConnection connection, OleDbTransaction transaction, string sql, params string[] values)
        {
            var table = new DataTable();
            using (OleDbCommand command = Command(connection, transaction, sql, values))
            using (var adapter = new OleDbDataAdapter(command)) adapter.Fill(table);
            return table;
        }
    }

    internal static class AccountText
    {
        internal static string Failed { get { return FrameworkLanguage.LangMsg("UserLoginForm", "credentialsFailed", "用户名或密码不正确。", "Invalid username or password.", "Nombre de usuario o contraseña no válidos."); } }
        internal static string Changed { get { return FrameworkLanguage.LangMsg("UserLoginForm", "accountChanged", "账号已被修改，请重试。", "The account changed. Please retry.", "La cuenta ha cambiado. Inténtelo de nuevo."); } }
        internal static string InvalidName { get { return FrameworkLanguage.LangMsg("AddUserForm", "invalidAccountName", "用户名不能为空且不得超过50个字符。", "Username must contain 1 to 50 characters.", "El nombre de usuario debe tener entre 1 y 50 caracteres."); } }
        internal static string PasswordShort { get { return FrameworkLanguage.LangMsg("AddUserForm", "msg_PasswordShort", "密码少于8个字符！", "Password less than 8 characters!", "Contraseña de menos de 8 caracteres!"); } }
        internal static string Exists { get { return FrameworkLanguage.LangMsg("AddUserForm", "accountExists", "账号已存在。", "Account already exists.", "La cuenta ya existe."); } }
        internal static string InvalidRole { get { return FrameworkLanguage.LangMsg("AddUserForm", "invalidRole", "账号权限无效。", "Invalid account permission.", "Permiso de cuenta no válido."); } }
        internal static string Denied { get { return FrameworkLanguage.LangMsg("AddUserForm", "adminRequired", "仅管理员可以管理账号。", "Administrator access is required to manage accounts.", "Se requiere acceso de administrador para gestionar cuentas."); } }
        internal static string Missing { get { return FrameworkLanguage.LangMsg("UserLoginForm", "accountDatabaseMissing", "账号数据库不存在。", "Account database is missing.", "Falta la base de datos de cuentas."); } }
    }
}
