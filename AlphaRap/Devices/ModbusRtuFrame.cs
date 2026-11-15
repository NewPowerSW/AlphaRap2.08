using System;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace AlphaRap
{
    /// <summary>Modbus 03 功能码的请求、完整应答和低字节在前的 CRC16 校验。</summary>
    internal static class ModbusRtuFrame
    {
        /// <summary>校验站号、寄存器范围及 03 功能码数量限制。</summary>
        internal static bool Valid(byte slave, ushort start, ushort count)
        {
            return slave >= 1 && slave <= 247 && count >= 1 && count <= 125 && (int)start + count <= 65536;
        }

        /// <summary>编码一次读取请求，不重复发送。</summary>
        internal static byte[] Request(byte slave, ushort start, ushort count)
        {
            if (!Valid(slave, start, count)) throw new ArgumentException("Modbus address/count");
            byte[] frame = { slave, 3, (byte)(start >> 8), (byte)start, (byte)(count >> 8), (byte)count, 0, 0 };
            SetCrc(frame); return frame;
        }

        /// <summary>按头部声明接收完整正常或异常帧，而不是立即读取 BytesToRead 快照。</summary>
        internal static byte[] Read(SerialPort port, byte slave, ushort count, CancellationToken token)
        {
            byte[] head = SerialFrames.ReadExactly(port, 3, token);
            if (head[0] != slave || (head[1] != 3 && head[1] != 0x83)) throw new InvalidDataException("Modbus station/function");
            int tail = head[1] == 0x83 ? 2 : head[2] + 2;
            if (head[1] == 3 && head[2] != count * 2) throw new InvalidDataException("Modbus byte count");
            byte[] rest = SerialFrames.ReadExactly(port, tail, token);
            byte[] frame = new byte[head.Length + rest.Length]; Buffer.BlockCopy(head, 0, frame, 0, 3); Buffer.BlockCopy(rest, 0, frame, 3, rest.Length);
            return Decode(frame, slave, count);
        }

        /// <summary>校验帧长度、站号、功能码、数据字节数及 CRC，失败不返回数据。</summary>
        internal static byte[] Decode(byte[] frame, byte slave, ushort count)
        {
            if (frame == null || frame.Length < 5 || frame[0] != slave) throw new InvalidDataException("Modbus response");
            ushort crc = Crc(frame, frame.Length - 2);
            if (frame[frame.Length - 2] != (byte)crc || frame[frame.Length - 1] != (byte)(crc >> 8)) throw new InvalidDataException("Modbus CRC");
            if (frame[1] == 0x83 && frame.Length == 5) throw new DeviceCommunicationException(CommunicationStatus.Rejected, false, "Modbus exception " + frame[2]);
            if (frame[1] != 3 || frame[2] != count * 2 || frame.Length != count * 2 + 5) throw new InvalidDataException("Modbus frame length/function");
            byte[] data = new byte[count * 2]; Buffer.BlockCopy(frame, 3, data, 0, data.Length); return data;
        }

        /// <summary>给完整帧尾部写入 Modbus CRC，低字节先发送。</summary>
        private static void SetCrc(byte[] frame)
        {
            ushort crc = Crc(frame, frame.Length - 2); frame[frame.Length - 2] = (byte)crc; frame[frame.Length - 1] = (byte)(crc >> 8);
        }

        /// <summary>计算指定字节范围的 Modbus CRC16。</summary>
        private static ushort Crc(byte[] bytes, int length)
        {
            ushort crc = 0xFFFF;
            for (int index = 0; index < length; index++)
            {
                crc ^= bytes[index];
                for (int bit = 0; bit < 8; bit++) crc = (ushort)((crc & 1) != 0 ? (crc >> 1) ^ 0xA001 : crc >> 1);
            }
            return crc;
        }
    }
}
