using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using AlphaRap.Security;

namespace AlphaRap
{
    /// <summary>
    /// 用户登录窗口：分段切换账号登录 / 指纹登录两个面板，文字由 ApplyLanguage() 按 SysPara.LanguageShow 显示，
    /// 图标由 AppIcons 绘制。
    /// </summary>
    public partial class UserLoginForm : Form
    {
        public bool str1 = false;

        public PermissionType UserPermission = PermissionType.None;

        /// <summary>当前是否处于指纹登录页。</summary>
        private bool _fingerMode;
        private string[] fingerprintUsers = new string[0];
        private readonly Func<PermissionType, Action> preparePermission;
        private bool bindingUsers;
        private bool authenticating;
        private int authenticationVersion;

        private bool _dragging;
        private Point _dragOffset;

        /// <summary>创建登录界面与图标，运行时再设置语言、账号绑定及交互。</summary>
        public UserLoginForm() : this(null) { }

        /// <summary>创建登录窗口，并允许应用在后台准备菜单权限。</summary>
        public UserLoginForm(Func<PermissionType, Action> preparePermission)
        {
            this.preparePermission = preparePermission;
            InitializeComponent();
            UiTheme.ApplyDpi(this);
            ApplyIcons();
            if (UiTheme.IsDesignTime) return;

            // 设置 UI 区域（影响 .NET 自带对话框和异常消息的语言）
            switch (SysPara.LanguageShow)
            {
                case LanguageType.Chinese:
                    Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("zh-CN");
                    break;
                case LanguageType.English:
                    Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
                    break;
                case LanguageType.Español:
                    Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("es-ES");
                    break;
            }

            // 本窗体的文案交给语言表：按需 new 出来的窗体，构造里"登记 + 立刻按当前语言套一遍"
            // （键 = 控件名，翻译在 LanguageData\UserLoginForm 段里改；切语言统一走 SwitchLanguageTo）。
            // 只 Register 不套字的话，英文/西语环境下打开登录框会显示设计器里的中文。
            if (!UiTheme.IsDesignTime) FrameworkLanguage.RegisterAndApplyLanguage(this, this.Name);

            // 图标只依赖 AppIcons，运行期与设计期都安全 → 放在这里，VS 设计视图也能看到真实图标
            ApplyLanguage();
            dgvUserList.DataBindingComplete += delegate { FrameworkLanguage.RefreshUserHeaders(dgvUserList); };
            dgvUserList.CellFormatting += delegate(object sender, DataGridViewCellFormattingEventArgs e)
            {
                if (e.ColumnIndex < 0 || dgvUserList.Columns[e.ColumnIndex].DataPropertyName != "Permission") return;
                e.Value = FrameworkLanguage.PermissionText(e.Value);
                e.FormattingApplied = true;
            };
            FrameworkLanguage.LanguageChanged += UserLoginForm_LanguageChanged;
            Disposed += delegate { FrameworkLanguage.LanguageChanged -= UserLoginForm_LanguageChanged; };

            // 密码框回车提交（事件挂在 FieldBox 内部的 TextBox 上）
            textPassword.Inner.KeyPress += textPassword_KeyPress;

            chipAccount.Click += delegate { ShowMode(false); };
            chipFinger.Click += delegate { ShowMode(true); };
            btnClose.Click += delegate { Close(); };

            // 无边框窗体：用标题栏 / 品牌区拖动
            AttachDrag(pnlTitle);
            AttachDrag(pnlBrand);

            ShowMode(false);
        }

        // ==================== 外观 ====================

        private void ApplyIcons()
        {
            try
            {
                picBrandLogo.Image = AppIcons.Get(AppIcon.Logo, 76, Color.White);

                picFeature1.Image = AppIcons.Get(AppIcon.Vision, 20, Color.White);
                picFeature2.Image = AppIcons.Get(AppIcon.Data, 20, Color.White);
                picFeature3.Image = AppIcons.Get(AppIcon.Check, 20, Color.White);

                textUserName.Icon = AppIcons.Get(AppIcon.Login, 20, UiKit.TextMuted);
                textPassword.Icon = AppIcons.Get(AppIcon.Lock, 20, UiKit.TextMuted);

                btnLogin.Icon = AppIcons.Get(AppIcon.Login, 18, Color.White);
                btnManual.Icon = AppIcons.Get(AppIcon.Fingerprint, 18, UiKit.TextPrimary);

                chipAccount.Icon = AppIcons.Get(AppIcon.Login, 24, UiKit.Brand);
                chipAccount.IconSelected = AppIcons.Get(AppIcon.Login, 24, Color.White);
                chipFinger.Icon = AppIcons.Get(AppIcon.Fingerprint, 24, UiKit.Brand);
                chipFinger.IconSelected = AppIcons.Get(AppIcon.Fingerprint, 24, Color.White);

                btnClose.Icon = AppIcons.Get(AppIcon.Close, 16, UiKit.TextMuted);

                // 主视觉指纹单独画（24 网格图标放大后描边会过粗，纹路之间的缝会被吃光）
                picFinger.Image = UiKit.HeroFingerprint(128, UiKit.Brand);

                UiKit.StyleGrid(dgvUserList);
                UiKit.Round(this, 12);   // 无边框窗体做圆角（FlatButton/CardPanel 是自绘圆角，不能再用 Region 硬裁）
            }
            catch { }
        }

        /// <summary>设置不随语言变化的文字（品牌副标题、版本号）；界面文案由语言表管理。</summary>
        private void ApplyLanguage()
        {
            try
            {
                lblBrandSub.Text = FrameworkLanguage.LangMsg("UserLoginForm", "lblBrandSub", "视觉检测系统", "Vision Inspection System", "Sistema de inspección visual");
                lblVersion.Text = "AlphaRap  v2.08";
                FrameworkLanguage.RefreshUserHeaders(dgvUserList);
                RefreshLoginButton();
            }
            catch { }
        }

        /// <summary>刷新登录页动态文案，保留账号输入和所选登录方式。</summary>
        private void UserLoginForm_LanguageChanged(object sender, EventArgs e)
        {
            ApplyLanguage();
        }

        /// <summary>切换"账号登录 / 指纹登录"。两个面板都 Dock=Fill，只切可见性即可（不可见的控件不参与停靠布局）。</summary>
        private void ShowMode(bool fingerprint)
        {
            if (authenticating) return;
            try
            {
                _fingerMode = fingerprint;
                pnlAccount.Visible = !fingerprint;
                pnlFinger.Visible = fingerprint;
                chipAccount.Selected = !fingerprint;
                chipFinger.Selected = fingerprint;
                pnlBody.PerformLayout();
                if (!fingerprint) textUserName.FocusInput();
            }
            catch { }
        }

        /// <summary>无边框窗体的拖动：挂到标题行 / 品牌区（递归挂到其子控件，标签也要能拖）。</summary>
        private void AttachDrag(Control root)
        {
            if (root == null) return;
            foreach (Control c in root.Controls)
            {
                if (c is FlatButton || c is AlarmChip || c is FieldBox) continue;
                AttachDrag(c);
            }
            root.MouseDown += DragMouseDown;
            root.MouseMove += DragMouseMove;
            root.MouseUp += DragMouseUp;
        }

        private void DragMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            _dragging = true;
            // 记录"鼠标屏幕坐标 → 窗体左上角"的偏移。
            // 不能用 sender 的客户区坐标：鼠标可能按在品牌区的某个标签上，
            // 那个坐标还要再加上标签在窗体里的偏移才是窗体坐标。
            _dragOffset = new Point(Cursor.Position.X - Location.X, Cursor.Position.Y - Location.Y);
        }

        private void DragMouseMove(object sender, MouseEventArgs e)
        {
            if (!_dragging) return;
            Location = new Point(Cursor.Position.X - _dragOffset.X, Cursor.Position.Y - _dragOffset.Y);
        }

        private void DragMouseUp(object sender, MouseEventArgs e)
        {
            _dragging = false;
        }

        // ==================== 业务逻辑 ====================

        /// <summary>兼容同步账号校验，凭据读取与迁移统一通过账号服务。</summary>
        public bool ReadUserData(string UserName, string Password, ref PermissionType UserPermission)
        {
            UserPermission = PermissionType.None;
            try
            {
                AccountIdentity identity = new AccountService(SysPara.MdbPath).Authenticate(UserName, Password);
                if (identity == null) { ShowAuthenticationError(AccountText.Failed); return false; }
                UserPermission = identity.Permission; return true;
            }
            catch (Exception error) { ShowAuthenticationError(error.Message); return false; }
        }

        /// <summary>显示统一登录错误，不回显任何凭据或散列。</summary>
        private void ShowAuthenticationError(string message)
        {
            if (IsDisposed) return;
            MessageBox.Show(this, message, FrameworkLanguage.LangMsg("Common", "msg_NoteTitle", "提示", "Note", "Consejo"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            textPassword.FocusInput();
        }
        private void textPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
                btnLogin.PerformClick();
        }

        /// <summary>收集界面凭据后交给统一验证入口。</summary>
        private async void btnLogin_Click(object sender, EventArgs e)
        {
            string name = textUserName.Value;
            string password = textPassword.Value;
            string path = SysPara.MdbPath;
            await AuthenticateAsync(() => new AccountService(path).Authenticate(name, password));
        }

        /// <summary>在工作线程验证账号并读取菜单权限，仅在当前登录请求仍有效时提交。</summary>
        private async Task AuthenticateAsync(Func<AccountIdentity> authenticate)
        {
            if (authenticating || IsDisposed || Disposing || ApplicationHost.IsShuttingDown) return;
            authenticating = true;
            int version = ++authenticationVersion;
            RefreshLoginButton();
            try
            {
                var work = Task.Run(() =>
                {
                    AccountIdentity identity = authenticate();
                    Action apply = identity == null ? null : preparePermission?.Invoke(identity.Permission);
                    return Tuple.Create(identity, apply);
                });
                ApplicationTasks.Global.Track("Authentication", work);
                var result = await work;
                if (!IsAuthenticationCurrent(version)) return;
                if (result.Item1 == null) { ShowAuthenticationError(AccountText.Failed); return; }
                CompleteLogin(result.Item1, result.Item2);
            }
            catch (Exception error)
            {
                if (IsAuthenticationCurrent(version)) ShowAuthenticationError(error.Message);
            }
            finally
            {
                authenticating = false;
                if (IsAuthenticationCurrent(version)) RefreshLoginButton();
            }
        }

        /// <summary>只允许当前仍打开的登录请求更新身份或显示错误。</summary>
        private bool IsAuthenticationCurrent(int version)
        {
            return !IsDisposed && !Disposing && !ApplicationHost.IsShuttingDown && version == authenticationVersion;
        }

        /// <summary>在登录按钮上显示当前验证状态，并遵循界面语言。</summary>
        private void RefreshLoginButton()
        {
            btnLogin.Enabled = !authenticating;
            btnLogin.Text = authenticating
                ? FrameworkLanguage.LangMsg(Name, "loginInProgress", "正在验证…", "Signing in…", "Iniciando sesión…")
                : FrameworkLanguage.LangText(Name, "btnLogin", "登录");
        }

        /// <summary>账号和指纹共享身份提交，菜单权限就绪后更新全局身份和权限。</summary>
        private void CompleteLogin(AccountIdentity identity, Action applyMenu)
        {
            UserPermission = identity.Permission;
            SysPara.UserName = identity.Name;
            SysPara.UserLoginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            ApplicationHost.SetPermission(identity.Permission, applyMenu);
            SysPara.bLogin = false;
            Close();
            SysPara.bLogin = true;
        }

        /// <summary>指纹页的"手动登录"：切回账号登录页。</summary>
        private void btnManual_Click(object sender, EventArgs e)
        {
            ShowMode(false);
        }

        private void UserLoginForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) Close();
        }

        private void UserLoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            authenticationVersion++;
            textUserName.Value = "";
            textPassword.Value = "";
            lblFingerStatus.Text = "";
            if (SysPara.bLogin)
            {
                SysPara.UserName = "None";
                SysPara.UserPermission = PermissionType.Operator;
                ApplicationHost.ModuleWindow?.CompleteLogin();
                SysPara.bLogin = false;
            }
        }

        /// <summary>刷新指纹模板索引对应的用户名。</summary>
        public void ResetFingerprintDB()
        {
            try
            {
                using (DataTable users = new AccountService(SysPara.MdbPath).Users()) ReadFingerprintUsers(users);
            }
            catch (Exception error) { fingerprintUsers = new string[0]; ShowAuthenticationError(error.Message); }
        }

        /// <summary>保留数据库行顺序，使用公开身份建立指纹索引。</summary>
        private void ReadFingerprintUsers(DataTable users)
        {
            fingerprintUsers = users.Rows.Cast<DataRow>().Select(row => Convert.ToString(row["UserName"])).ToArray();
        }

        /// <summary>可信指纹回调重新确认账号身份，并与密码登录共用后台验证。</summary>
        public void OnTemplate(int userIndex)
        {
            RefreshDifferentThreadUI(this, async () =>
            {
                if (IsDisposed || Disposing || !_fingerMode || userIndex < 0 || userIndex >= fingerprintUsers.Length) return;
                string path = SysPara.MdbPath;
                string name = fingerprintUsers[userIndex];
                await AuthenticateAsync(() => new AccountService(path).FindIdentity(name));
            });
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

        /// <summary>先显示登录窗口，再后台读取一次公开用户列表。</summary>
        private async void UserLoginForm_Shown(object sender, EventArgs e)
        {
            if (UiTheme.IsDesignTime || ApplicationHost.IsShuttingDown) return;
            textUserName.FocusInput();
            string path = SysPara.MdbPath;
            try
            {
                Task<DataTable> work = Task.Run(() => new AccountService(path).Users());
                ApplicationTasks.Global.Track("LoginUsers", work);
                DataTable users = await work;
                if (IsDisposed || Disposing || ApplicationHost.IsShuttingDown) { users.Dispose(); return; }
                BindUserList(users);
            }
            catch (Exception error)
            {
                if (!IsDisposed && !Disposing && !ApplicationHost.IsShuttingDown && !authenticating) ShowAuthenticationError(error.Message);
            }
        }

        /// <summary>兼容同步列表读取入口，使用同一账号快照同时更新指纹索引。</summary>
        public string ReadAllUserData()
        {
            try { BindUserList(new AccountService(SysPara.MdbPath).Users()); }
            catch (Exception error) { ShowAuthenticationError(error.Message); }
            return "None";
        }

        /// <summary>绑定公开账号和指纹索引，保留用户已输入的账号以及当前登录方式。</summary>
        private void BindUserList(DataTable users)
        {
            ReadFingerprintUsers(users);
            bindingUsers = true;
            try
            {
                dgvUserList.DataSource = users;
                if (dgvUserList.Columns.Count >= 2)
                {
                    dgvUserList.Columns[0].Width = 250;
                    dgvUserList.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
            }
            finally { bindingUsers = false; }

            if (!authenticating && !_fingerMode && string.IsNullOrEmpty(textUserName.Value) && string.IsNullOrEmpty(textPassword.Value))
                dgvUserList_SelectionChanged(dgvUserList, EventArgs.Empty);
        }

        /// <summary>建立一个方法用来把窗体2值传给窗体1</summary>
        public void FingerPrinfInfo(string strInfo)
        {
            RefreshDifferentThreadUI(lblFingerStatus, () =>
            {
                lblFingerStatus.Text = strInfo;
            });
        }

        /// <summary>选中账号时回填名称，后台绑定和验证期间保留当前输入。</summary>
        private void dgvUserList_SelectionChanged(object sender, EventArgs e)
        {
            if (bindingUsers || authenticating || dgvUserList.CurrentCell == null) return;
            int rowIndex = dgvUserList.CurrentCell.RowIndex;
            if (rowIndex < 0) return;
            textUserName.Value = Convert.ToString(dgvUserList[0, rowIndex].Value);
            if (_fingerMode) ShowMode(false);
        }
    }
}
