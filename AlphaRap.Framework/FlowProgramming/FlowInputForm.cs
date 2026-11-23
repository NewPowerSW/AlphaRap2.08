using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlphaRap.FlowProgramming
{
    public partial class FlowInputForm : FlowLanguageForm
    {
        private Func<string> promptText;
        public string InputValue { get { return value.Text; } set { this.value.Text = value ?? ""; } }
        /// <summary>建立固定操作员输入布局，不读取工程数据或设备。</summary>
        public FlowInputForm() { InitializeComponent(); }

        /// <summary>显示业务提示并等待操作员明确提交，关闭表示取消。</summary>
        public FlowInputForm(string message) : this()
        {
            if (UiTheme.IsDesignTime) return;
            prompt.Text = message;
        }

        /// <summary>提供随语言切换刷新的固定提示，保留用户正在编辑的输入值。</summary>
        public FlowInputForm(Func<string> message) : this()
        {
            promptText = message;
            RefreshLanguageChoices();
        }

        /// <summary>仅刷新由宿主提供的提示文字，不改动输入内容。</summary>
        protected override void RefreshLanguageChoices()
        {
            if (promptText != null) prompt.Text = promptText();
        }

        /// <summary>编辑资源的默认显示文字，允许留空以使用工程名称。</summary>
        internal static bool TryEditResourceText(IWin32Window owner, string currentText, out string text)
        {
            using (var dialog = new FlowInputForm(() => FrameworkLanguage.LangText("LanguageSetting", "resourceText", "显示文字")))
            {
                dialog.InputValue = currentText;
                text = currentText;
                if (dialog.ShowDialog(owner) != DialogResult.OK) return false;
                text = dialog.InputValue.Trim();
                return true;
            }
        }

        /// <summary>为动态资源页签添加显示文字菜单，只操作右键命中的页签。</summary>
        internal static void AttachResourceTextMenu(TabControl tabs, Action<TabPage> edit)
        {
            var menu = new ContextMenuStrip();
            var item = new ToolStripMenuItem { Name = "editResourceText_dyn" };
            menu.Items.Add(item);
            tabs.ContextMenuStrip = menu;
            tabs.Disposed += (sender, args) => menu.Dispose();
            TabPage target = null;
            menu.Opening += (sender, args) =>
            {
                target = null;
                var point = tabs.PointToClient(Cursor.Position);
                for (int index = 0; index < tabs.TabCount; index++)
                {
                    if (!tabs.GetTabRect(index).Contains(point)) continue;
                    target = tabs.TabPages[index];
                    break;
                }
                args.Cancel = target == null;
                item.Text = FrameworkLanguage.LangText("LanguageSetting", "resourceText", "显示文字");
            };
            item.Click += (sender, args) => { if (target != null) edit(target); };
        }

        /// <summary>将后台输入请求投递到主界面线程，超时关闭本次输入窗口。</summary>
        public static Task<string> Request(string message, CancellationToken token)
        {
            TaskCompletionSource<string> completion = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            Action show = () =>
            {
                if (token.IsCancellationRequested) { completion.TrySetCanceled(); return; }
                using (FlowInputForm form = new FlowInputForm(message))
                {
                    IntPtr handle = form.Handle;
                    using (token.Register(() => { if (!form.IsDisposed && form.IsHandleCreated) form.BeginInvoke(new Action(form.Close)); }))
                    {
                        if (token.IsCancellationRequested) { completion.TrySetCanceled(); return; }
                        if (form.ShowDialog(ApplicationHost.MainWindow) == DialogResult.OK && !token.IsCancellationRequested) completion.TrySetResult(form.value.Text);
                        else completion.TrySetCanceled();
                    }
                }
            };
            if (ApplicationHost.MainWindow == null || !ApplicationHost.MainWindow.IsHandleCreated) throw new InvalidOperationException("Main window is unavailable.");
            ApplicationHost.MainWindow.BeginInvoke(show);
            return completion.Task;
        }
    }
}
