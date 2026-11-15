using System;
using System.IO;
using System.IO.Pipes;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace AlphaRap.PLC
{
    internal enum PlcCommand : byte { Connect = 1, Read = 2, Write = 3 }

    /// <summary>有界的本机管道报文；不接受外部网络连接。</summary>
    internal sealed class PlcRequest
    {
        internal const int MaxBytes = 16000;
        internal PlcCommand Command;
        internal int NetId, Port, Element, Address, Count;
        internal string Host = "";
        internal byte[] Data = new byte[0];

        /// <summary>写入一条完整请求；所有字节长度在本机通信前校验。</summary>
        internal void Write(BinaryWriter writer)
        {
            writer.Write((byte)Command); writer.Write(NetId); writer.Write(Port); writer.Write(Element);
            writer.Write(Address); writer.Write(Count); writer.Write(Host); writer.Write(Data.Length); writer.Write(Data); writer.Flush();
        }

        /// <summary>读取固定结构和受限数据，拒绝损坏或过长的管道报文。</summary>
        internal static PlcRequest Read(BinaryReader reader)
        {
            var request = new PlcRequest { Command = (PlcCommand)reader.ReadByte(), NetId = reader.ReadInt32(), Port = reader.ReadInt32(),
                Element = reader.ReadInt32(), Address = reader.ReadInt32(), Count = reader.ReadInt32(), Host = reader.ReadString() };
            request.Data = ReadBytes(reader);
            if (request.Host.Length > 255 || request.Count < 0 || request.Count > MaxBytes / 2) throw new InvalidDataException("PLC request size");
            return request;
        }

        /// <summary>读取一段有长度限制的数据，截断报文按通道故障处理。</summary>
        internal static byte[] ReadBytes(BinaryReader reader)
        {
            int length = reader.ReadInt32();
            if (length < 0 || length > MaxBytes) throw new InvalidDataException("PLC payload size");
            byte[] data = reader.ReadBytes(length);
            if (data.Length != length) throw new EndOfStreamException();
            return data;
        }
    }

    internal sealed class PlcReply
    {
        internal int Code;
        internal byte[] Data = new byte[0];
        internal string Error = "";

        /// <summary>写入驱动结果，不在失败响应中伪造 PLC 数值。</summary>
        internal void Write(BinaryWriter writer)
        {
            writer.Write(Code); writer.Write(Error); writer.Write(Data.Length); writer.Write(Data); writer.Flush();
        }

        /// <summary>读取完整响应；通道每次只允许一条未完成请求。</summary>
        internal static PlcReply Read(BinaryReader reader)
        {
            var reply = new PlcReply { Code = reader.ReadInt32(), Error = reader.ReadString() };
            reply.Data = PlcRequest.ReadBytes(reader);
            return reply;
        }
    }

    /// <summary>原生驱动只在独立子进程运行，超时关闭子进程即关闭其网络连接。</summary>
    internal static class PlcWorker
    {
        internal const string Switch = "--plc-native-worker";

        /// <summary>仅处理由父进程传入的继承管道句柄，在应用和硬件初始化之前返回。</summary>
        internal static bool TryRun(string[] args)
        {
            if (args.Length == 0 || args[0] != Switch) return false;
            if (args.Length != 4) return true;
            WatchParent(int.Parse(args[3], System.Globalization.CultureInfo.InvariantCulture));
            Run(args[1], args[2], InvokeNative);
            return true;
        }

        /// <summary>父进程意外退出时回收仍阻塞在驱动中的子进程。</summary>
        internal static void WatchParent(int parentId)
        {
            var watcher = new Thread(() =>
            {
                try { using (Process parent = Process.GetProcessById(parentId)) parent.WaitForExit(); }
                catch (ArgumentException) { }
                Process.GetCurrentProcess().Kill();
            }) { IsBackground = true };
            watcher.Start();
        }

        /// <summary>父进程退出或管道关闭时结束；一次驱动异常只产生一条失败响应。</summary>
        internal static void Run(string inputHandle, string outputHandle, Func<PlcRequest, PlcReply> invoke)
        {
            try
            {
                using (var input = new AnonymousPipeClientStream(PipeDirection.In, inputHandle))
                using (var output = new AnonymousPipeClientStream(PipeDirection.Out, outputHandle))
                using (var reader = new BinaryReader(input))
                using (var writer = new BinaryWriter(output))
                    Serve(reader, writer, invoke);
            }
            catch (IOException) { }
        }

        /// <summary>按接收顺序执行请求，原生调用不并发，也不重试写入。</summary>
        private static void Serve(BinaryReader reader, BinaryWriter writer, Func<PlcRequest, PlcReply> invoke)
        {
            while (true)
            {
                PlcRequest request = PlcRequest.Read(reader);
                PlcReply reply;
                try { reply = invoke(request); }
                catch (Exception error) { reply = new PlcReply { Error = error.GetType().Name + ": " + error.Message }; }
                reply.Write(writer);
            }
        }

        /// <summary>保留汇川原有 ABI、元件编号和缓冲区顺序，不替换设备协议。</summary>
        private static PlcReply InvokeNative(PlcRequest request)
        {
            if (request.Command == PlcCommand.Connect)
                return new PlcReply { Code = NativeConnect(request.Host, request.NetId, request.Port) ? 1 : 0 };
            if (request.Command == PlcCommand.Read)
            {
                byte[] buffer = new byte[PlcRequest.MaxBytes];
                int code = NativeRead((SoftElemType)request.Element, request.Address, request.Count, buffer, request.NetId);
                int width = request.Element == (int)SoftElemType.REGI_H5U_D || request.Element == (int)SoftElemType.REGI_H5U_R ? 2 : 1;
                if (code == 1) Array.Resize(ref buffer, request.Count * width);
                return new PlcReply { Code = code, Data = code == 1 ? buffer : new byte[0] };
            }
            if (request.Command == PlcCommand.Write)
                return new PlcReply { Code = NativeWrite((SoftElemType)request.Element, request.Address, request.Count, request.Data, request.NetId) };
            throw new InvalidDataException("PLC command");
        }

        /// <summary>在子进程建立汇川连接。</summary>
        [DllImport("StandardModbusApi.dll", EntryPoint = "Init_ETH_String", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool NativeConnect(string ip, int netId, int port);

        /// <summary>在子进程读取汇川元件块。</summary>
        [DllImport("StandardModbusApi.dll", EntryPoint = "H5u_Read_Device_Block", CallingConvention = CallingConvention.Cdecl)]
        private static extern int NativeRead(SoftElemType element, int address, int count, byte[] buffer, int netId);

        /// <summary>在子进程写入汇川元件块。</summary>
        [DllImport("StandardModbusApi.dll", EntryPoint = "H5u_Write_Device_Block", CallingConvention = CallingConvention.Cdecl)]
        private static extern int NativeWrite(SoftElemType element, int address, int count, byte[] buffer, int netId);
    }
}
