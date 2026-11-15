using System;
using System.Globalization;
using System.Threading;

namespace AlphaRap
{
    public enum ScannerStatus { Success, NotConnected, Busy, InvalidConfiguration, Timeout, Cancelled, Disconnected, Rejected, ProtocolError, TransportError }
    public enum ScannerConnectionState { Disconnected, Connecting, Connected, Closing, Faulted }

    /// <summary>扫码操作的独立结果，失败不携带可供生产使用的条码。</summary>
    public sealed class ScannerResult
    {
        public ScannerStatus Status { get; private set; }
        public bool Succeeded { get { return Status == ScannerStatus.Success; } }
        public string Value { get; private set; }
        public string Detail { get; private set; }
        public bool CommandMayHaveExecuted { get; private set; }

        /// <summary>构造成功数据或保留状态的失败结果。</summary>
        internal ScannerResult(ScannerStatus status, string value = null, string detail = "", bool uncertain = false)
        {
            Status = status; Value = status == ScannerStatus.Success ? value : null;
            Detail = detail; CommandMayHaveExecuted = uncertain;
        }

        public string Message
        {
            get
            {
                if (Succeeded) return "";
                string message = string.Format(CultureInfo.CurrentCulture, DeviceText.T("error.scannerCommunication"),
                    DeviceText.T("scanner.status." + Status), Detail);
                return CommandMayHaveExecuted ? message + " " + DeviceText.T("error.scannerUncertain") : message;
            }
        }

        /// <summary>返回确认数据，取消沿用流程令牌，其他失败携带具体通信状态抛出。</summary>
        public string RequireValue(CancellationToken token)
        {
            if (Succeeded) return Value;
            if (Status == ScannerStatus.Cancelled) throw new OperationCanceledException(Message, token);
            throw new ScannerCommunicationException(this);
        }
    }

    /// <summary>使流程故障保留通信原因，不将 ERROR 文本或空字符串当成条码。</summary>
    public sealed class ScannerCommunicationException : InvalidOperationException
    {
        public ScannerStatus Status { get; private set; }
        public bool CommandMayHaveExecuted { get; private set; }

        /// <summary>从一次失败结果建立可供日志识别的异常。</summary>
        internal ScannerCommunicationException(ScannerResult result) : base(result.Message)
        {
            Status = result.Status; CommandMayHaveExecuted = result.CommandMayHaveExecuted;
        }
    }
}
