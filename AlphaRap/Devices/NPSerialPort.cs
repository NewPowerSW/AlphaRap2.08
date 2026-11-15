using System;
using System.Globalization;
using System.IO.Ports;
using System.Text;
using System.Threading;
using NPSDK;

namespace AlphaRap.Classes
{
    /// <summary>兼容旧串口工具；每次连接创建新对象，读写共享有限时限和实际占用。</summary>
    public class NPSerialPort : IDisposable
    {
        private SerialPort sp = new SerialPort();
        private readonly SerialOperation operations = new SerialOperation();
        private CommunicationStatus lastStatus = CommunicationStatus.NotConnected;
        public CommunicationStatus LastStatus { get { return lastStatus; } }
        public bool IsBusy { get { return operations.IsBusy; } }

        /// <summary>先验证配置，再回收旧端口并创建新对象，不复用已 Dispose 的实例。</summary>
        public bool Connect(string strPortName, int iRate, int iDataBits, int iParity, int iStopBits)
        {
            Flow_Chart.ThrowIfExecutionStopped();
            if (!SerialFrames.Valid(strPortName, iRate, iDataBits, iParity, iStopBits)) { lastStatus = CommunicationStatus.InvalidConfiguration; return false; }
            SerialPort previous = sp;
            operations.StopAsync(previous.Close).GetAwaiter().GetResult();
            var candidate = new SerialPort();
            var result = operations.RunAsync(token =>
            {
                SerialPort prior = sp;
                if (!ReferenceEquals(prior, previous)) { prior.Close(); prior.Dispose(); }
                token.ThrowIfCancellationRequested();
                SerialFrames.Configure(candidate, strPortName, iRate, iDataBits, iParity, iStopBits);
                candidate.Open(); token.ThrowIfCancellationRequested(); sp = candidate; return candidate.IsOpen;
            }, candidate.Close, 1500, Flow_Chart.CurrentExecutionToken).GetAwaiter().GetResult();
            lastStatus = result.Status;
            previous.Dispose();
            if (!result.Succeeded) candidate.Dispose();
            if (result.Status == CommunicationStatus.Cancelled) result.RequireValue(Flow_Chart.CurrentExecutionToken);
            return result.Succeeded && result.Value;
        }

        /// <summary>关闭和 IO 都实际退出后释放端口；后续 Connect 使用全新对象。</summary>
        public void Dispose()
        {
            SerialPort current = sp;
            operations.StopAsync(current.Close).GetAwaiter().GetResult(); current.Dispose();
        }

        /// <summary>读取公开状态，不等待正在执行的通信锁。</summary>
        public bool ConnectStates() { return sp.IsOpen; }

        /// <summary>发送文本，不清空未发送的输出或其他请求的数据。</summary>
        public bool Write(string strWrite)
        {
            if (strWrite == null) return false;
            return Write(sp.Encoding.GetBytes(strWrite));
        }

        /// <summary>发送原始数据，失败后不自动重发。</summary>
        public bool Write(byte[] cmd)
        {
            Flow_Chart.ThrowIfExecutionStopped();
            if (cmd == null || cmd.Length > 65536) { lastStatus = CommunicationStatus.InvalidConfiguration; return false; }
            byte[] bytes = (byte[])cmd.Clone(); SerialPort current = sp;
            var result = operations.RunAsync(token => { SerialFrames.Write(current, bytes, token); return true; }, current.Close, 1000,
                Flow_Chart.CurrentExecutionToken, true).GetAwaiter().GetResult();
            lastStatus = result.Status;
            if (result.CommandMayHaveExecuted) Flow_Module.Module_AddAlarmLog(result.Message);
            if (result.Status == CommunicationStatus.Cancelled) result.RequireValue(Flow_Chart.CurrentExecutionToken);
            return result.Succeeded;
        }

        /// <summary>校验并发送十六进制数据，非法字串不访问端口。</summary>
        public bool Write_StringToHex(string cmd)
        {
            byte[] bytes;
            try { bytes = strToHexByte(cmd); }
            catch (FormatException) { lastStatus = CommunicationStatus.InvalidConfiguration; return false; }
            return Write(bytes);
        }

        /// <summary>读取当前可用的实际字节；取消抛出，其他失败沿用 null 约定。</summary>
        public byte[] readChar()
        {
            Flow_Chart.ThrowIfExecutionStopped(); SerialPort current = sp;
            var result = operations.RunAsync(token => SerialFrames.ReadAvailable(current, token), current.Close, 1000,
                Flow_Chart.CurrentExecutionToken).GetAwaiter().GetResult();
            lastStatus = result.Status;
            if (result.Status == CommunicationStatus.Cancelled) result.RequireValue(Flow_Chart.CurrentExecutionToken);
            return result.Value;
        }

        /// <summary>解码实际字节，不把缓冲区容量或零填充当成接收数据。</summary>
        public string readSting() { byte[] bytes = readChar(); return bytes == null ? null : Encoding.UTF8.GetString(bytes); }

        /// <summary>返回实际接收数据的十六进制表示。</summary>
        public string readStingByHex() { byte[] bytes = readChar(); return bytes == null ? null : ToHexStrFromByte(bytes); }

        /// <summary>保留旧文本读取的 Err 失败标记。</summary>
        public string ReadStr() { return readSting() ?? "Err"; }

        /// <summary>按明确请求清除输入缓冲，不在普通写入时隐式调用。</summary>
        public void ClearInBuffer() { Clear(true); }

        /// <summary>按明确请求清除输出缓冲，不丢弃正常写入的待发数据。</summary>
        public void ClearOutBuffer() { Clear(false); }

        /// <summary>串行执行显式清空操作并报告失败。</summary>
        private void Clear(bool input)
        {
            SerialPort current = sp;
            operations.RunAsync(token => { token.ThrowIfCancellationRequested(); if (input) current.DiscardInBuffer(); else current.DiscardOutBuffer(); return true; },
                current.Close, 1000, Flow_Chart.CurrentExecutionToken).GetAwaiter().GetResult().RequireValue(Flow_Chart.CurrentExecutionToken);
        }

        /// <summary>校验旧逗号配置的四项参数，再使用统一连接路径。</summary>
        public bool ConnectComPressL(string strPortName, string CommunicationPara)
        {
            string[] fields = (CommunicationPara ?? "").Split(','); int rate, bits, parity, stop;
            if (fields.Length != 4 || !int.TryParse(fields[0], out rate) || !int.TryParse(fields[1], out bits) || !int.TryParse(fields[2], out parity) || !int.TryParse(fields[3], out stop))
            { lastStatus = CommunicationStatus.InvalidConfiguration; return false; }
            return Connect(strPortName, rate, bits, parity, stop);
        }

        /// <summary>严格解析成对的十六进制字节，拒绝缺失半字节的输入。</summary>
        public byte[] strToHexByte(string hexString)
        {
            if (hexString == null) throw new FormatException("Hex");
            string text = hexString.Replace(" ", "");
            if (text.Length % 2 != 0 || text.Length > 131072) throw new FormatException("Hex length");
            byte[] bytes = new byte[text.Length / 2];
            for (int index = 0; index < bytes.Length; index++)
                if (!byte.TryParse(text.Substring(index * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out bytes[index])) throw new FormatException("Hex byte");
            return bytes;
        }

        /// <summary>按空格分隔编码实际字节数组。</summary>
        public string ToHexStrFromByte(byte[] byteDatas)
        {
            if (byteDatas == null) return "";
            return BitConverter.ToString(byteDatas).Replace('-', ' ');
        }
    }
}
