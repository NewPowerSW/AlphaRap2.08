using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AlphaRap
{
    /// <summary>串口阻塞调用的串行占用；取消关闭捕获端口，真实 IO 和关闭都退出后才释放。</summary>
    internal sealed class SerialOperation
    {
        private readonly SemaphoreSlim gate = new SemaphoreSlim(1, 1);
        private readonly object sync = new object();
        private readonly object closeSync = new object();
        private CancellationTokenSource active;
        private Action activeClose;
        private long generation;
        private Task stopping = Task.FromResult(0);
        private Exception cleanupError;
        internal bool IsBusy { get { lock (sync) return active != null || !stopping.IsCompleted || cleanupError != null; } }

        /// <summary>在后台执行一次实际串口操作，排队取消不影响另一个请求。</summary>
        internal async Task<CommunicationResult<T>> RunAsync<T>(Func<CancellationToken, T> action, Action close, int timeout,
            CancellationToken caller, bool writes = false)
        {
            if (timeout < 1 || timeout > 60000) return new CommunicationResult<T>(CommunicationStatus.InvalidConfiguration);
            long version; lock (sync) version = generation;
            using (var deadline = CancellationTokenSource.CreateLinkedTokenSource(caller))
            {
                deadline.CancelAfter(timeout);
                try { await gate.WaitAsync(deadline.Token).ConfigureAwait(false); }
                catch (OperationCanceledException) { return new CommunicationResult<T>(CancelStatus(caller, version)); }
                try { return await RunOwnedAsync(action, close, version, caller, deadline, writes).ConfigureAwait(false); }
                finally { gate.Release(); }
            }
        }

        /// <summary>保留原生工作和取消关闭的真实任务，不以超时掩盖仍在运行的驱动。</summary>
        private async Task<CommunicationResult<T>> RunOwnedAsync<T>(Func<CancellationToken, T> action, Action close, long version,
            CancellationToken caller, CancellationTokenSource deadline, bool writes)
        {
            lock (sync)
            {
                if (version != generation || !stopping.IsCompleted) return new CommunicationResult<T>(CommunicationStatus.Cancelled);
                if (cleanupError != null) return new CommunicationResult<T>(CommunicationStatus.TransportError, detail: cleanupError.Message);
                active = deadline; activeClose = close;
            }
            Task cleanup = Task.FromResult(0);
            object cleanupSync = new object();
            bool started = false;
            CancellationTokenRegistration registration = deadline.Token.Register(() => { lock (cleanupSync) cleanup = CloseInBackground(close); });
            try
            {
                T value = await Task.Factory.StartNew(() =>
                {
                    deadline.Token.ThrowIfCancellationRequested(); started = true;
                    return action(deadline.Token);
                }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default).ConfigureAwait(false);
                deadline.Token.ThrowIfCancellationRequested();
                lock (sync) if (version != generation) return new CommunicationResult<T>(CommunicationStatus.Cancelled, uncertain: writes && started);
                return new CommunicationResult<T>(CommunicationStatus.Success, value);
            }
            catch (Exception error) when (error is IOException || error is TimeoutException || error is InvalidOperationException || error is ArgumentException || error is OperationCanceledException || error is UnauthorizedAccessException || error is InvalidDataException)
            {
                registration.Dispose();
                Task early; lock (cleanupSync) early = cleanup;
                await early.ConfigureAwait(false);
                await CloseInBackground(close).ConfigureAwait(false);
                CommunicationStatus status = Classify(error, caller, deadline.Token, version);
                lock (sync) if (version == generation) generation++;
                return new CommunicationResult<T>(status, detail: error.Message, uncertain: writes && started);
            }
            finally
            {
                registration.Dispose();
                Task pending; lock (cleanupSync) pending = cleanup;
                try { await pending.ConfigureAwait(false); }
                catch (Exception error) { lock (sync) cleanupError = error; throw; }
                finally { lock (sync) { active = null; activeClose = null; } }
            }
        }

        /// <summary>撤销旧队列并等待关闭及当前驱动返回，重复停止共用同一个任务。</summary>
        internal Task StopAsync(Action close)
        {
            lock (sync)
            {
                if (!stopping.IsCompleted) return stopping;
                generation++;
                if (active != null) active.Cancel();
                stopping = StopOwnedAsync(activeClose ?? close);
                return stopping;
            }
        }

        /// <summary>关闭只在后台进行，驱动级关闭阻塞会继续保留设备占用。</summary>
        private async Task StopOwnedAsync(Action close)
        {
            await CloseInBackground(close).ConfigureAwait(false);
            await gate.WaitAsync().ConfigureAwait(false);
            lock (sync) cleanupError = null;
            gate.Release();
        }

        /// <summary>独立派发原生关闭，不让取消回调阻塞 UI 或其他设备。</summary>
        private Task CloseInBackground(Action close)
        {
            return Task.Factory.StartNew(() =>
            {
                try { lock (closeSync) close(); }
                catch (ObjectDisposedException) { }
                catch (Exception error) { lock (sync) cleanupError = error; throw; }
            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        /// <summary>保留协议失败与取消、时限和 IO 异常的不同原因。</summary>
        private CommunicationStatus Classify(Exception error, CancellationToken caller, CancellationToken token, long version)
        {
            if (token.IsCancellationRequested) return CancelStatus(caller, version);
            var known = error as DeviceCommunicationException;
            if (known != null) return known.Status;
            if (error is OperationCanceledException) return CommunicationStatus.Cancelled;
            if (error is TimeoutException) return CommunicationStatus.Timeout;
            if (error is InvalidDataException) return CommunicationStatus.ProtocolError;
            if (error is ArgumentException) return CommunicationStatus.InvalidConfiguration;
            return CommunicationStatus.TransportError;
        }

        /// <summary>区分总时限和用户或旧代次停止。</summary>
        private CommunicationStatus CancelStatus(CancellationToken caller, long version)
        {
            lock (sync) return caller.IsCancellationRequested || version != generation ? CommunicationStatus.Cancelled : CommunicationStatus.Timeout;
        }
    }
}
