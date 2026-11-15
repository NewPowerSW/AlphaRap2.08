using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading;
using NPSDK;

namespace AlphaRap
{
    /// <summary>串口参数验证和有限阻塞的精确读取，不以缓冲区容量代替实际字节数。</summary>
    internal static class SerialFrames
    {
        /// <summary>在接触端口之前验证可配置的串口参数。</summary>
        internal static bool Valid(string name, int baud, int bits, int parity, int stop)
        {
            return !string.IsNullOrWhiteSpace(name) && baud > 0 && bits >= 5 && bits <= 8 && parity >= 0 && parity <= 4 && stop >= 1 && stop <= 3;
        }

        /// <summary>只在关闭状态配置端口，并为原生读写提供有限时限。</summary>
        internal static void Configure(SerialPort port, string name, int baud, int bits, int parity, int stop)
        {
            if (!Valid(name, baud, bits, parity, stop)) throw new ArgumentException("Serial settings");
            port.PortName = name; port.BaudRate = baud; port.DataBits = bits; port.Parity = (Parity)parity; port.StopBits = (StopBits)stop;
            port.ReadTimeout = 100; port.WriteTimeout = 1000;
        }

        /// <summary>每次调用前后检查执行令牌，不清除尚未发送的输出缓冲。</summary>
        internal static void Write(SerialPort port, byte[] bytes, CancellationToken token)
        {
            Flow_Chart.ThrowIfExecutionStopped(); token.ThrowIfCancellationRequested();
            if (!port.IsOpen) throw new DeviceCommunicationException(CommunicationStatus.NotConnected, false, "Serial port closed");
            port.WriteTimeout = 1000; port.Write(bytes, 0, bytes.Length);
            token.ThrowIfCancellationRequested(); Flow_Chart.ThrowIfExecutionStopped();
        }

        /// <summary>按协议所需长度累积，原生超时片段不会被返回为完整数据。</summary>
        internal static byte[] ReadExactly(SerialPort port, int count, CancellationToken token)
        {
            if (count < 0 || count > 65536) throw new InvalidDataException("Serial length");
            if (!port.IsOpen) throw new DeviceCommunicationException(CommunicationStatus.NotConnected, false, "Serial port closed");
            byte[] bytes = new byte[count]; int offset = 0;
            port.ReadTimeout = 100;
            while (offset < count)
            {
                token.ThrowIfCancellationRequested(); Flow_Chart.ThrowIfExecutionStopped();
                try { int read = port.Read(bytes, offset, count - offset); if (read == 0) throw new EndOfStreamException(); offset += read; }
                catch (TimeoutException) { token.ThrowIfCancellationRequested(); }
            }
            return bytes;
        }

        /// <summary>只返回当前可用的实际字节，不强读 ReadBufferSize，也不丢弃后续应答。</summary>
        internal static byte[] ReadAvailable(SerialPort port, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (!port.IsOpen) throw new DeviceCommunicationException(CommunicationStatus.NotConnected, false, "Serial port closed");
            int count = port.BytesToRead;
            if (count > 65536) throw new InvalidDataException("Serial buffer length");
            return ReadExactly(port, count, token);
        }

        /// <summary>读取一条有终止符的完整文本，跨包累计并限制总长度。</summary>
        internal static string ReadLine(SerialPort port, CancellationToken token)
        {
            var bytes = new List<byte>();
            while (true)
            {
                byte value = ReadExactly(port, 1, token)[0];
                if (value == 10 || value == 13) { if (bytes.Count == 0) continue; return port.Encoding.GetString(bytes.ToArray()).Trim(); }
                if (bytes.Count >= 65536) throw new InvalidDataException("Serial frame length");
                bytes.Add(value);
            }
        }
    }
}
