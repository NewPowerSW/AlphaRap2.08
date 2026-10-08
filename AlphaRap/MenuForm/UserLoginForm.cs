using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using static AlphaRap.MainForm;
using System.Threading;

namespace AlphaRap
{
    /// <summary>
    /// 用户登录。
    ///
    /// 重构要点：
    ///   1. 原来是"TabControl + 两个 TabPage 互相抢 Parent"来切页（button1_Click 里
    ///      `FingerPrintLog.Parent = null`），既难读又和 ItemSize=(10,5) 这种"把标签头压到看不见"
    ///      的写法绑在一起。现在改成**分段切换（账号 / 指纹）**，两个面板只切可见性。
    ///   2. 原来的布局全在 resx 里（本地化窗体 + resources.ApplyResources），且
    ///      TabControl 尺寸 1092×819 比窗体 649 还高 → 底部内容必然被裁。
    ///      现在改成显式的自适应布局，文字由 ApplyLanguage() 按 SysPara.LanguageShow 输出。
    ///   3. 图标全部用 AppIcons 现画，不再依赖 resx 里的 3 张位图（合计约 140KB）。
    ///
    /// 对外契约（ReadUserData / OnTemplate / ResetFingerprintDB / FingerPrinfInfo /
    /// ReadAllUserData / UserPermission / str1）全部保留原签名。
    /// </summary>
    public partial class UserLoginForm : Form
    {
        public bool str1 = false;

        public PermissionType UserPermission = PermissionType.None;

        /// <summary>当前是否处于指纹登录页。</summary>
        private bool _fingerMode;

        private bool _dragging;
        private Point _dragOffset;

        public UserLoginForm()
        {
            // 保留原有的 UI 区域设置（工程内其它组件仍可能依赖它）
            // 注意：这不是界面文案切换，只影响 .NET 自带对话框/异常消息的区域；三语都要给。
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

            InitializeComponent();

            // 本窗体的文案交给语言表：按需 new 出来的窗体，构造里"登记 + 立刻按当前语言套一遍"
            // （键 = 控件名，翻译在 LanguageData\UserLoginForm 段里改；切语言统一走 SwitchLanguageTo）。
            // 只 Register 不套字的话，英文/西语环境下打开登录框会显示设计器里的中文。
            MiddleLayer.RegisterAndApplyLanguage(this, this.Name);

            // 图标只依赖 AppIcons，运行期与设计期都安全 → 放在这里，VS 设计视图也能看到真实图标
            ApplyIcons();
            ApplyLanguage();

            // 回车提交（原来挂在 textPassword 上，现在输入框是自绘容器，事件挂在它的内层 TextBox）
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

        /// <summary>
        /// 这里只剩**不随语言变**的固定串（品牌副标题、版本号）；
        /// 界面文案已由语言表接管（构造里 RegisterAndApplyLanguage，键 = 控件名）。
        /// </summary>
        private void ApplyLanguage()
        {
            try
            {
                lblBrandSub.Text = "Vision Inspection System";
                lblVersion.Text = "AlphaRap  v2.08";
            }
            catch { }
        }

        /// <summary>切换"账号登录 / 指纹登录"。两个面板都 Dock=Fill，只切可见性即可（不可见的控件不参与停靠布局）。</summary>
        private void ShowMode(bool fingerprint)
        {
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

        // ==================== 业务逻辑（沿用原有实现） ====================

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
                            MessageBox.Show(
                                MiddleLayer.LangMsg("UserLoginForm", "msg_LoginBadPassword", "登录失败！密码错误！", "Login Fail！Please check your Password!", "¡El inicio de sesión falló! ¡Error de contraseña!"),
                                MiddleLayer.LangMsg("Common", "msg_NoteTitle", "提示", "Note", "Consejo"),
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            textPassword.FocusInput();
                        }
                    }
                    else
                    {
                        MessageBox.Show(
                            MiddleLayer.LangMsg("UserLoginForm", "msg_LoginNoUser", "登录失败！用户名不存在！", "Login Fail！Please check your UserName!", "¡El inicio de sesión falló! ¡El usuario no existe!"),
                            MiddleLayer.LangMsg("Common", "msg_NoteTitle", "提示", "Note", "Consejo"),
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        textUserName.FocusInput();
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

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (ReadUserData(textUserName.Value, textPassword.Value, ref UserPermission))
            {
                SysPara.UserName = textUserName.Value;
                SysPara.UserLoginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                MiddleLayer.SwitchPermission(UserPermission);

                textUserName.Value = "";
                textPassword.Value = "";
                SysPara.bLogin = false;
                this.Close();

                SysPara.bLogin = true;
            }
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
            textUserName.Value = "";
            textPassword.Value = "";
            lblFingerStatus.Text = "";
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

        public void ResetFingerprintDB()
        {
            string strSQL = "select * from UserData";
            bool Successful = false;
            DataTable readData = DataBase.ReadData_Adapter(SysPara.MdbPath, strSQL, ref Successful);
            if (Successful)
                for (int i = 0; i < readData.Rows.Count; i++)
                {
                    string Name = readData.Rows[i]["UserName"].ToString();
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
                    textUserName.Value = readData.Rows[UserIndex]["UserName"].ToString();
                    textPassword.Value = readData.Rows[UserIndex]["Password"].ToString();
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
            ReadAllUserData();
            // 构造函数里窗体还没有句柄，Focus 不会生效，这里补一次
            textUserName.FocusInput();
        }

        public string ReadAllUserData()
        {
            string gUser = "None";

            try
            {
                string strSQL = "select UserName,Permission from UserData";
                bool Successful = false;

                DataTable readData = DataBase.ReadData_Adapter(SysPara.MdbPath, strSQL, ref Successful);
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

        /// <summary>建立一个方法用来把窗体2值传给窗体1</summary>
        public void FingerPrinfInfo(string strInfo)
        {
            RefreshDifferentThreadUI(lblFingerStatus, () =>
            {
                lblFingerStatus.Text = strInfo;
            });
        }

        private void dgvUserList_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvUserList.CurrentCell != null)
            {
                if (dgvUserList.CurrentCell.RowIndex >= 0)
                {
                    textUserName.Value = dgvUserList[0, dgvUserList.CurrentCell.RowIndex].Value.ToString();
                    if (_fingerMode) ShowMode(false);
                }
            }
        }
    }
}
