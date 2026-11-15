using System;
using System.Globalization;
using System.Text;
using System.Threading;

namespace AlphaRap.PLC
{
    public enum SoftElemType
    {
        ELEM_QX = 0, ELEM_MW = 1, ELEM_X = 2, ELEM_Y = 3,
        REGI_H3U_Y = 0x20, REGI_H3U_X = 0x21, REGI_H3U_S = 0x22, REGI_H3U_M = 0x23,
        REGI_H3U_TB = 0x24, REGI_H3U_TW = 0x25, REGI_H3U_CB = 0x26, REGI_H3U_CW = 0x27,
        REGI_H3U_DW = 0x28, REGI_H3U_CW2 = 0x29, REGI_H3U_SM = 0x2a, REGI_H3U_SD = 0x2b, REGI_H3U_R = 0x2c,
        REGI_H5U_Y = 0x30, REGI_H5U_X = 0x31, REGI_H5U_S = 0x32, REGI_H5U_M = 0x33,
        REGI_H5U_B = 0x34, REGI_H5U_D = 0x35, REGI_H5U_R = 0x36
    }

    /// <summary>汇川 H5U 驱动；同一连接编号共用隔离进程和请求队列。</summary>
    public partial class INOVANCE
    {
        private readonly PlcSession session;
        private readonly int netId;
        public string WritErrorMessage = string.Empty;
        public string ReadErrorMessage = string.Empty;

        /// <summary>使用已有代码默认的零号连接，不创建网络连接。</summary>
        public INOVANCE() : this(0) { }

        /// <summary>绑定指定共享会话，不在构造或设计阶段连接设备。</summary>
        public INOVANCE(int connectionId)
        {
            netId = connectionId; session = PlcSession.Get(connectionId);
        }

        /// <summary>连接兼容入口；总上限三秒，最多三次握手，不恢复任何生产流程。</summary>
        public static bool Connet(string sIpAddr, int nNetId = 0, int IpPort = 502)
        {
            NPSDK.Flow_Chart.ThrowIfExecutionStopped();
            CancellationToken token = NPSDK.Flow_Chart.CurrentExecutionToken;
            PlcResult<byte[]> result = PlcSession.Get(nNetId).Connect(sIpAddr, IpPort, 3000, 3, token);
            if (result.Status == PlcStatus.Cancelled) result.RequireValue(token);
            NPSDK.Flow_Chart.ThrowIfExecutionStopped();
            return result.Succeeded;
        }

        /// <summary>保留原公开入口，通过受控会话连接，原生 DLL 不进入主进程。</summary>
        public static bool Init_ETH_String(string sIpAddr, int nNetId = 0, int IpPort = 502)
        {
            return Connet(sIpAddr, nNetId, IpPort);
        }

        /// <summary>关闭指定编号的共享连接，不触及其他设备进程。</summary>
        public static bool Exit_ETH(int nNetId = 0) { return PlcSession.Get(nNetId).Disconnect(); }

        /// <summary>在主程序退出时回收所有 PLC 通信进程。</summary>
        public static void CloseAll() { PlcSession.CloseAll(); }

        /// <summary>显式连接当前会话，连接失败与取消保留独立状态。</summary>
        public PlcResult<bool> Connect(string host, int port = 502, int timeoutMs = 3000, CancellationToken token = default(CancellationToken))
        {
            PlcResult<byte[]> result = session.Connect(host, port, timeoutMs, 3, token);
            return ConvertResult(result, result.Succeeded);
        }

        /// <summary>读取完整数据；count 为字地址的 16 位字数或位地址的位数，多值用逗号分隔。</summary>
        public PlcResult<string> ReadPlcResult(string address, int count, string type = "int", int timeoutMs = 1000, CancellationToken token = default(CancellationToken))
        {
            return ReadCore(address, count, type, timeoutMs, token, false);
        }

        /// <summary>保留旧读取格式和失败哨兵，流程新调用使用类型化结果。</summary>
        public string ReadPlc(string nStartAddr, int nCount, string type = "int")
        {
            int code;
            return ReadPlc(nStartAddr, nCount, type, out code);
        }

        /// <summary>兼容旧流程读取，并通过结果码区分合法的负数数据。</summary>
        private string ReadPlc(string address, int count, string type, out int code)
        {
            NPSDK.Flow_Chart.ThrowIfExecutionStopped();
            CancellationToken token = NPSDK.Flow_Chart.CurrentExecutionToken;
            PlcResult<string> result = ReadCore(address, count, type, 1000, token, true);
            code = result.Succeeded ? 1 : result.DriverCode == 1 ? 0 : result.DriverCode;
            ReadErrorMessage = result.Succeeded ? "" : result.Message;
            if (result.Status == PlcStatus.Cancelled) result.RequireValue(token);
            NPSDK.Flow_Chart.ThrowIfExecutionStopped();
            return result.Succeeded ? result.Value : "-999";
        }

        /// <summary>先校验地址和长度，再调用共享会话；失败缓冲区不会参与解码。</summary>
        private PlcResult<string> ReadCore(string address, int count, string type, int timeout, CancellationToken token, bool legacy)
        {
            PlcRequest request;
            bool word;
            if (!TryAddress(address, count, out request, out word) || !ValidType(type)) return Invalid<string>();
            int width = WordWidth(type);
            if (word && count % width != 0) return Invalid<string>();
            request.Command = PlcCommand.Read;
            PlcResult<byte[]> reply = session.Execute(request, timeout, token);
            if (!reply.Succeeded) return ConvertResult(reply, (string)null);
            int required = word ? count * 2 : count;
            if (reply.Value.Length < required) return new PlcResult<string>(PlcStatus.WorkerError, detail: "Truncated PLC data.");
            return new PlcResult<string>(PlcStatus.Success, Decode(reply.Value, count, type, word, legacy), reply.DriverCode);
        }

        /// <summary>读取固定字数的 UTF-8 字符串；无零结束符时返回完整字符串。</summary>
        public string ReadPlcString(string nStartAddr, int nCount)
        {
            NPSDK.Flow_Chart.ThrowIfExecutionStopped();
            PlcRequest request;
            bool word;
            if (!TryAddress(nStartAddr, nCount, out request, out word) || !word) { ReadErrorMessage = Invalid<string>().Message; return "-999"; }
            request.Command = PlcCommand.Read;
            CancellationToken token = NPSDK.Flow_Chart.CurrentExecutionToken;
            PlcResult<byte[]> reply = session.Execute(request, 1000, token);
            ReadErrorMessage = reply.Succeeded ? "" : reply.Message;
            if (reply.Status == PlcStatus.Cancelled) reply.RequireValue(token);
            NPSDK.Flow_Chart.ThrowIfExecutionStopped();
            if (!reply.Succeeded) return "-999";
            if (reply.Value.Length < nCount * 2) { ReadErrorMessage = DeviceText.T("error.plcInvalidData"); return "-999"; }
            string value = Encoding.UTF8.GetString(reply.Value, 0, nCount * 2);
            int end = value.IndexOf('\0');
            return end < 0 ? value : value.Substring(0, end);
        }

        /// <summary>发送前完整校验数值和长度；不重试任何一次写入。</summary>
        public PlcResult<bool> WritePlcResult(string address, string value, string type = "int", int timeoutMs = 1000, CancellationToken token = default(CancellationToken))
        {
            if (value == null || value.Length > PlcRequest.MaxBytes * 12 || !ValidType(type)) return Invalid<bool>();
            string[] values = value.Split(',');
            PlcRequest request;
            bool word;
            if (!TryAddress(address, 1, out request, out word)) return Invalid<bool>();
            int width = word ? WordWidth(type) : 1;
            int count = values.Length * width;
            if (!TryAddress(address, count, out request, out word)) return Invalid<bool>();
            byte[] buffer = new byte[word ? count * 2 : count];
            for (int index = 0; index < values.Length; index++)
                if (!EncodeValue(values[index], type, word, buffer, index * (word ? width * 2 : 1))) return Invalid<bool>();
            request.Command = PlcCommand.Write; request.Data = buffer;
            PlcResult<byte[]> reply = session.Execute(request, timeoutMs, token);
            if (reply.WriteOutcomeUnknown) NPSDK.Flow_Module.Module_AddAlarmLog(reply.Message);
            return ConvertResult(reply, reply.Succeeded);
        }

        /// <summary>写入兼容入口，取消向上抛出，不将未确认写入误报成功。</summary>
        public bool WritePlc(string nStartAddr, string writeValue, string type = "int")
        {
            NPSDK.Flow_Chart.ThrowIfExecutionStopped();
            CancellationToken token = NPSDK.Flow_Chart.CurrentExecutionToken;
            PlcResult<bool> result = WritePlcResult(nStartAddr, writeValue, type, 1000, token);
            WritErrorMessage = result.Succeeded ? "" : result.Message;
            if (result.Status == PlcStatus.Cancelled) result.RequireValue(token);
            NPSDK.Flow_Chart.ThrowIfExecutionStopped();
            return result.Succeeded;
        }

        /// <summary>按 16 位字数写入 UTF-8 字符串，奇数字节补零，不越界读取缓冲区。</summary>
        public bool WritePlcString(string nStartAddr, string writeValue)
        {
            NPSDK.Flow_Chart.ThrowIfExecutionStopped();
            if (writeValue == null || Encoding.UTF8.GetByteCount(writeValue) > PlcRequest.MaxBytes) { WritErrorMessage = Invalid<bool>().Message; return false; }
            byte[] value = Encoding.UTF8.GetBytes(writeValue);
            int count = (value.Length + 1) / 2;
            PlcRequest request;
            bool word;
            if (!TryAddress(nStartAddr, count, out request, out word) || !word) { WritErrorMessage = Invalid<bool>().Message; return false; }
            request.Command = PlcCommand.Write; request.Data = new byte[count * 2];
            Buffer.BlockCopy(value, 0, request.Data, 0, value.Length);
            CancellationToken token = NPSDK.Flow_Chart.CurrentExecutionToken;
            PlcResult<byte[]> result = session.Execute(request, 1000, token);
            if (result.WriteOutcomeUnknown) NPSDK.Flow_Module.Module_AddAlarmLog(result.Message);
            WritErrorMessage = result.Succeeded ? "" : result.Message;
            if (result.Status == PlcStatus.Cancelled) result.RequireValue(token);
            NPSDK.Flow_Chart.ThrowIfExecutionStopped();
            return result.Succeeded;
        }

        /// <summary>解析明确的 H5U 元件，禁止缺失或未知前缀默认落到输出区。</summary>
        private bool TryAddress(string address, int count, out PlcRequest request, out bool word)
        {
            request = null; word = false;
            if (string.IsNullOrEmpty(address) || address.Length < 2 || count < 1 || count > PlcRequest.MaxBytes / 2) return false;
            int prefix = "YXSMDR".IndexOf(address[0]);
            int index;
            if (prefix < 0 || !int.TryParse(address.Substring(1), NumberStyles.None, CultureInfo.InvariantCulture, out index) || index > int.MaxValue - count) return false;
            word = address[0] == 'D' || address[0] == 'R';
            request = new PlcRequest { NetId = netId, Element = 0x30 + prefix + (word ? 1 : 0), Address = index, Count = count };
            return true;
        }

        /// <summary>识别兼容的数据类型，未知类型不会写入默认值。</summary>
        private static bool ValidType(string type) { return type == "bit" || type == "int16" || type == "int" || type == "int32" || type == "float"; }

        /// <summary>返回一个值占用的 16 位字数。</summary>
        private static int WordWidth(string type) { return type == "bit" || type == "int16" ? 1 : 2; }

        /// <summary>解码完整数据，新接口多值分隔明确，兼容接口保留旧字符串格式。</summary>
        private static string Decode(byte[] data, int count, string type, bool word, bool legacy)
        {
            int width = word ? WordWidth(type) : 1;
            var values = new string[count / width];
            for (int index = 0; index < values.Length; index++)
            {
                if (!word) { values[index] = data[index].ToString(CultureInfo.CurrentCulture); continue; }
                int offset = index * width * 2;
                values[index] = width == 1 ? BitConverter.ToInt16(data, offset).ToString(CultureInfo.CurrentCulture)
                    : type == "float" ? BitConverter.ToSingle(data, offset).ToString(CultureInfo.CurrentCulture) : BitConverter.ToInt32(data, offset).ToString(CultureInfo.CurrentCulture);
            }
            if (!legacy) return string.Join(",", values);
            return word && width == 1 ? string.Join(",", values) + "," : string.Join("", values);
        }

        /// <summary>将一个经过范围校验的值编码到完整请求，禁止半包或非有限浮点数。</summary>
        private static bool EncodeValue(string text, string type, bool word, byte[] buffer, int offset)
        {
            int integer;
            if (!word)
            {
                if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out integer) || (integer != 0 && integer != 1)) return false;
                buffer[offset] = (byte)integer; return true;
            }
            byte[] bytes;
            if (type == "float")
            {
                float number;
                if (!float.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out number) || float.IsNaN(number) || float.IsInfinity(number)) return false;
                bytes = BitConverter.GetBytes(number);
            }
            else
            {
                if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out integer)) return false;
                if (WordWidth(type) == 1 && (integer < short.MinValue || integer > short.MaxValue)) return false;
                bytes = WordWidth(type) == 1 ? BitConverter.GetBytes((short)integer) : BitConverter.GetBytes(integer);
            }
            Buffer.BlockCopy(bytes, 0, buffer, offset, bytes.Length);
            return true;
        }

        /// <summary>传递通信状态，失败数据保持缺省而不是伪造成功值。</summary>
        private static PlcResult<T> ConvertResult<T>(PlcResult<byte[]> result, T value)
        {
            return new PlcResult<T>(result.Status, result.Succeeded ? value : default(T), result.DriverCode, result.WriteOutcomeUnknown, result.Detail);
        }

        /// <summary>生成发送前校验失败，不访问通信会话。</summary>
        private static PlcResult<T> Invalid<T>() { return new PlcResult<T>(PlcStatus.InvalidArgument, detail: DeviceText.T("error.plcInvalidData")); }
    }
}
