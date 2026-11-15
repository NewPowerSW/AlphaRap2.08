using System;
using System.Threading;

namespace AlphaRap
{
    public enum CommunicationStatus { Success, NotConnected, InvalidConfiguration, Timeout, Cancelled, Disconnected, ProtocolError, Rejected, Unconfirmed, TransportError }

    /// <summary>设备通信的数据与失败原因分离，失败不提供可用业务值。</summary>
    public sealed class CommunicationResult<T>
    {
        public CommunicationStatus Status { get; private set; }
        public bool Succeeded { get { return Status == CommunicationStatus.Success; } }
        public T Value { get; private set; }
        public string Detail { get; private set; }
        public bool CommandMayHaveExecuted { get; private set; }

        /// <summary>保存一次操作结果和命令执行的不确定性。</summary>
        internal CommunicationResult(CommunicationStatus status, T value = default(T), string detail = "", bool uncertain = false)
        {
            Status = status; Value = Succeeded ? value : default(T); Detail = detail; CommandMayHaveExecuted = uncertain;
        }

        public string Message
        {
            get
            {
                if (Succeeded) return "";
                string message = string.Format(DeviceText.T("error.communication"), DeviceText.T("communication." + Status), Detail);
                return CommandMayHaveExecuted ? message + " " + DeviceText.T("error.scannerUncertain") : message;
            }
        }

        /// <summary>成功时返回值，取消和其他失败分别抛出可识别异常。</summary>
        public T RequireValue(CancellationToken token)
        {
            if (Succeeded) return Value;
            if (Status == CommunicationStatus.Cancelled) throw new OperationCanceledException(Message, token);
            throw new DeviceCommunicationException(Status, CommandMayHaveExecuted, Message);
        }
    }

    /// <summary>保留设备通信的状态码及未确认命令，不使用错误字符串充当数据。</summary>
    public sealed class DeviceCommunicationException : InvalidOperationException
    {
        public CommunicationStatus Status { get; private set; }
        public bool CommandMayHaveExecuted { get; private set; }

        /// <summary>建立供流程和日志识别的设备通信异常。</summary>
        internal DeviceCommunicationException(CommunicationStatus status, bool uncertain, string message) : base(message)
        {
            Status = status; CommandMayHaveExecuted = uncertain;
        }
    }
}
