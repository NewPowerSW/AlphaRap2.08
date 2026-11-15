using System.Text;

namespace AlphaRap
{
    /// <summary>为扫码枪复用 TCP 帧缓冲和连接所有权，并保留其 LOFF 停止命令。</summary>
    internal sealed class ScannerConnection : TcpConnection
    {
        /// <summary>绑定现有扫码枪编码及停止协议，不创建额外连接。</summary>
        internal ScannerConnection(string host, int port) : base(host, port, Encoding.Default, "LOFF\r\n") { }
    }
}
