using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlphaRap.Configuration
{
    /// <summary>参数模型需要的串行调度入口，不暴露控件类型。</summary>
    internal interface IModuleDispatcher
    {
        /// <summary>执行一次模型写入并保留取消及原始异常。</summary>
        Task<object> InvokeAsync(Func<object> action);
    }

    internal sealed class InlineModuleDispatcher : IModuleDispatcher
    {
        /// <summary>无窗口宿主在自己的调用线程执行模型动作。</summary>
        public Task<object> InvokeAsync(Func<object> action)
        {
            try { return Task.FromResult(action()); }
            catch (OperationCanceledException) { return Task.FromCanceled<object>(new CancellationToken(true)); }
            catch (Exception error) { return Task.FromException<object>(error); }
        }
    }

    /// <summary>WinForms 适配器负责消息线程切换，业务模型不持有 Form。</summary>
    internal sealed class WinFormsModuleDispatcher : IModuleDispatcher
    {
        private readonly Control owner;
        private readonly int ownerThread = Thread.CurrentThread.ManagedThreadId;
        private readonly InlineModuleDispatcher inline = new InlineModuleDispatcher();
        /// <summary>绑定页面的调度环境，不创建额外窗口。</summary>
        internal WinFormsModuleDispatcher(Control owner) { this.owner = owner; }

        /// <summary>有页面绑定时在 UI 写表，无窗口时继续按模型入口执行。</summary>
        public Task<object> InvokeAsync(Func<object> action)
        {
            if (Thread.CurrentThread.ManagedThreadId == ownerThread) return inline.InvokeAsync(action);
            Control target = ApplicationHost.MainWindow;
            if (target == null || target.IsDisposed || !target.IsHandleCreated) target = owner;
            if (target.IsDisposed) return inline.InvokeAsync(action);
            if (!target.IsHandleCreated) return Task.FromException<object>(new InvalidOperationException(FlowProgramming.FlowText.T("constantUiUnavailable")));
            var completion = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            EventHandler disposed = (sender, args) => completion.TrySetCanceled();
            target.Disposed += disposed;
            try
            {
                target.BeginInvoke(new Action(() =>
                {
                    if (completion.Task.IsCompleted) return;
                    try { completion.TrySetResult(action()); }
                    catch (OperationCanceledException) { completion.TrySetCanceled(); }
                    catch (Exception error) { completion.TrySetException(error); }
                }));
            }
            catch (InvalidOperationException error) { completion.TrySetException(error); }
            return RemoveSubscriptionAsync(target, disposed, completion.Task);
        }

        /// <summary>完成或取消后解除页面订阅，不把已结束请求保留到窗口关闭。</summary>
        private static async Task<object> RemoveSubscriptionAsync(Control target, EventHandler disposed, Task<object> task)
        {
            try { return await task.ConfigureAwait(false); }
            finally { target.Disposed -= disposed; }
        }
    }
}
