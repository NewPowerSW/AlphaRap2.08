using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlphaRap.FlowProgramming
{
    public partial class FlowInputForm : FlowLanguageForm
    {
        public string InputValue { get { return value.Text; } }
        /// <summary>建立固定操作员输入布局，不读取工程数据或设备。</summary>
        public FlowInputForm() { InitializeComponent(); }

        /// <summary>显示业务提示并等待操作员明确提交，关闭表示取消。</summary>
        public FlowInputForm(string message) : this()
        {
            if (UiTheme.IsDesignTime) return;
            prompt.Text = message;
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
                        if (form.ShowDialog(MiddleLayer.MainF) == DialogResult.OK && !token.IsCancellationRequested) completion.TrySetResult(form.value.Text);
                        else completion.TrySetCanceled();
                    }
                }
            };
            if (MiddleLayer.MainF == null || !MiddleLayer.MainF.IsHandleCreated) throw new InvalidOperationException("Main window is unavailable.");
            MiddleLayer.MainF.BeginInvoke(show);
            return completion.Task;
        }
    }
}
