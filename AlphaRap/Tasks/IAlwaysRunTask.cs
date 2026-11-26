using System.Threading;
using System.Threading.Tasks;
using AlphaRap.TaskProcess;

namespace AlphaRap
{
    /// <summary>实现一个由应用自动发现的常驻任务，实例跨多轮刷新保留。</summary>
    public interface IAlwaysRunTask
    {
        int IntervalMs { get; }
        string[] InputNames { get; }

        /// <summary>执行一次刷新，取消后及时退出；下一轮等待本轮结束。</summary>
        Task RefreshAsync(AlwaysRunContext context, CancellationToken cancellationToken);
    }

    /// <summary>按资源名称访问常驻任务所需的应用数据。</summary>
    public sealed class AlwaysRunContext
    {
        private readonly StationIo io;

        /// <summary>复用机器当前的 IO 目录。</summary>
        internal AlwaysRunContext(StationIo io) { this.io = io; }

        /// <summary>读取配置后的有效电平，缺失、未连接或读取失败时返回 null。</summary>
        public bool? ReadInput(string name) { return io.ReadNamedInput(name); }
    }
}
