using System;
using System.Collections.Generic;

namespace AlphaRap.TaskProcess
{
    internal enum DownConveyorInput
    {
        StopperUp, StopperDown, BoardIn, Slowdown, BoardAtStop, BoardOut, UpstreamAvailable, DownstreamReady
    }

    internal enum DownConveyorOutput
    {
        Forward, Reverse, SlowSpeed, Stopper, ReadyForBoard, BoardAvailable
    }

    /// <summary>一次完成的下料交接，供生产记录和客户文件使用同一编号及时间。</summary>
    internal sealed class DownConveyorTransfer
    {
        internal string Id { get; }
        internal string Model { get; }
        internal DateTime CompletedAt { get; }

        /// <summary>固定交接记录的标识、型号及完成时间。</summary>
        internal DownConveyorTransfer(string id, string model, DateTime completedAt)
        {
            Id = id;
            Model = model;
            CompletedAt = completedAt;
        }
    }

    /// <summary>用注入的 IO 执行下料动作及信号判断，可脱离手动页运行。</summary>
    internal sealed class DownConveyorProcess
    {
        private static readonly DownConveyorOutput[] StopOutputs =
        {
            DownConveyorOutput.SlowSpeed, DownConveyorOutput.Forward,
            DownConveyorOutput.BoardAvailable, DownConveyorOutput.ReadyForBoard
        };

        private readonly Func<DownConveyorInput, bool?> read;
        private readonly Action<DownConveyorOutput, bool> write;
        private readonly Action<DownConveyorTransfer> recordTransfer;
        private string transferId;
        private string transferModel;

        /// <summary>连接工站 IO；采样未知时返回 null。可选的完工接收方负责持久化。</summary>
        internal DownConveyorProcess(Func<DownConveyorInput, bool?> read, Action<DownConveyorOutput, bool> write,
            Action<DownConveyorTransfer> recordTransfer = null)
        {
            this.read = read ?? throw new ArgumentNullException(nameof(read));
            this.write = write ?? throw new ArgumentNullException(nameof(write));
            this.recordTransfer = recordTransfer;
        }

        /// <summary>采样板件位置，未知时由工站继续等待。</summary>
        internal bool? BoardAtStop() { return read(DownConveyorInput.BoardAtStop); }

        /// <summary>仅当信号有效且达到所需电平时完成等待。</summary>
        internal bool WaitFor(DownConveyorInput input, bool expected = true) { return read(input) == expected; }

        /// <summary>读取现有减速感应器供原工艺节点采样。</summary>
        internal void SampleSlowdown() { read(DownConveyorInput.Slowdown); }

        /// <summary>设置一个工站输出。</summary>
        internal void Set(DownConveyorOutput output, bool active) { write(output, active); }

        /// <summary>切换挡停气缸并等待对应到位感应器。</summary>
        internal bool MoveStopper(bool up)
        {
            Set(DownConveyorOutput.Stopper, up);
            return WaitFor(up ? DownConveyorInput.StopperUp : DownConveyorInput.StopperDown);
        }

        /// <summary>结束定位输送并关闭减速输出。</summary>
        internal void StopTransport()
        {
            Set(DownConveyorOutput.SlowSpeed, false);
            Set(DownConveyorOutput.Forward, false);
        }

        /// <summary>已读取入站状态后登记本次交接，固定配方名称。</summary>
        internal void BeginTransfer(string model)
        {
            if (recordTransfer == null || transferId != null) return;
            transferId = Guid.NewGuid().ToString("N");
            transferModel = model;
        }

        /// <summary>完成出站握手后通知接收方，每次交接只通知一次。</summary>
        internal void CompleteTransfer(DateTime time)
        {
            if (transferId == null) return;
            var transfer = new DownConveyorTransfer(transferId, transferModel, time);
            recordTransfer(transfer);
            transferId = null;
        }

        /// <summary>停止下料，取消未完成的交接并逐项尝试撤销已有输出。</summary>
        internal void Stop()
        {
            transferId = null;
            var errors = new List<Exception>();
            foreach (DownConveyorOutput output in StopOutputs)
            {
                try { Set(output, false); }
                catch (Exception error) { errors.Add(error); }
            }
            if (errors.Count > 0) throw new AggregateException(errors);
        }
    }
}
