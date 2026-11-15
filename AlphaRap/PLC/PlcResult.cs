using System;
using System.Globalization;
using System.Threading;

namespace AlphaRap.PLC
{
    public enum PlcStatus { Success, NotConnected, InvalidArgument, Timeout, Cancelled, DriverError, WorkerError }

    /// <summary>一次 PLC 操作的结果；失败不携带可供流程使用的数据。</summary>
    public sealed class PlcResult<T>
    {
        public PlcStatus Status { get; private set; }
        public bool Succeeded { get { return Status == PlcStatus.Success; } }
        public T Value { get; private set; }
        public int DriverCode { get; private set; }
        public bool WriteOutcomeUnknown { get; private set; }
        public string Detail { get; private set; }

        /// <summary>构造成功数据或独立的失败状态。</summary>
        internal PlcResult(PlcStatus status, T value = default(T), int driverCode = 0, bool unknown = false, string detail = "")
        {
            Status = status; Value = value; DriverCode = driverCode; WriteOutcomeUnknown = unknown; Detail = detail;
        }

        /// <summary>成功时返回数据；取消沿用流程令牌，其余失败抛出包含通信状态的异常。</summary>
        public T RequireValue(CancellationToken token)
        {
            if (Succeeded) return Value;
            if (Status == PlcStatus.Cancelled) throw new OperationCanceledException(Message, token);
            throw new PlcCommunicationException(Status, WriteOutcomeUnknown, Message);
        }

        public string Message
        {
            get
            {
                string message = string.Format(CultureInfo.CurrentCulture, DeviceText.T("error.plcCommunication"), Status, DriverCode, Detail);
                return WriteOutcomeUnknown ? message + " " + DeviceText.T("error.plcWriteUnknown") : message;
            }
        }
    }

    /// <summary>保留通信状态和写入不确定性，供流程报警及日志识别。</summary>
    public sealed class PlcCommunicationException : InvalidOperationException
    {
        public PlcStatus Status { get; private set; }
        public bool WriteOutcomeUnknown { get; private set; }

        /// <summary>创建不允许静默转为读值或自动重发的通信异常。</summary>
        internal PlcCommunicationException(PlcStatus status, bool unknown, string message) : base(message)
        {
            Status = status; WriteOutcomeUnknown = unknown;
        }
    }
}
