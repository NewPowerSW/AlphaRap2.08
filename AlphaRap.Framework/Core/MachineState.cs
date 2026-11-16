using System;
using System.Threading;

namespace AlphaRap
{
    /// <summary>一次原子发布的机器状态，界面可读取一致的模式与自动会话标记。</summary>
    public sealed class MachineStateSnapshot
    {
        public RunMode Mode { get; private set; }
        public bool Automatic { get; private set; }
        public long Revision { get; private set; }
        public string Reason { get; private set; }
        /// <summary>固定状态组合及其修订，发布后不可修改。</summary>
        internal MachineStateSnapshot(RunMode mode, bool automatic, long revision, string reason) { Mode = mode; Automatic = automatic; Revision = revision; Reason = reason; }
    }

    /// <summary>机器模式的唯一存储与发布入口，旧属性设置也在此规范化。</summary>
    public sealed class MachineState
    {
        public static readonly MachineState Global = new MachineState();
        private readonly object sync = new object();
        private MachineStateSnapshot current = new MachineStateSnapshot(RunMode.IDLE, false, 0, "Created");
        public MachineStateSnapshot Current { get { return Volatile.Read(ref current); } }

        /// <summary>退出旧会话后开始初始化，不允许跨过仍在停止的状态。</summary>
        public void Initialize()
        {
            lock (sync)
            {
                if (current.Mode != RunMode.IDLE && current.Mode != RunMode.INITIAL && current.Mode != RunMode.PAUSE) throw InvalidTransition();
                Publish(RunMode.INITIAL, false, "Initialize");
            }
        }
        /// <summary>只有宿主已确认初始化就绪时发布自动运行。</summary>
        public void Start(bool initialized)
        {
            lock (sync)
            {
                if (!initialized || current.Mode == RunMode.STOPPING || current.Mode == RunMode.RUN) throw InvalidTransition();
                Publish(RunMode.RUN, true, "Start");
            }
        }
        /// <summary>暂停运行或初始化，保留是否具有可恢复的自动会话。</summary>
        public void Pause()
        {
            lock (sync)
            {
                if (current.Mode != RunMode.RUN && current.Mode != RunMode.INITIAL && current.Mode != RunMode.PAUSE) throw InvalidTransition();
                Publish(RunMode.PAUSE, current.Automatic, "Pause");
            }
        }
        /// <summary>立即撤销自动会话，真实任务退出前持续处于停止中。</summary>
        public void BeginStop() { lock (sync) Publish(RunMode.STOPPING, false, "StopRequested"); }
        /// <summary>只在停止协调结束后发布待机；失败保留停止状态。</summary>
        public void CompleteStop(bool success)
        {
            lock (sync)
            {
                if (current.Mode != RunMode.STOPPING) throw InvalidTransition();
                Publish(success ? RunMode.IDLE : RunMode.STOPPING, false, success ? "Stopped" : "StopFailed");
            }
        }
        /// <summary>保留旧源码设置入口，单次赋值也不能产生 RUN 且自动标记为假的组合。</summary>
        internal void SetLegacyMode(RunMode mode)
        {
            if (!Enum.IsDefined(typeof(RunMode), mode)) throw new ArgumentOutOfRangeException(nameof(mode));
            lock (sync) Publish(mode, mode == RunMode.RUN || (mode == RunMode.PAUSE && current.Automatic), "CompatibilityMode");
        }
        /// <summary>旧自动标记修改通过原子状态组合表达，不保留矛盾的两个独立字段。</summary>
        internal void SetLegacyAutomatic(bool value)
        {
            lock (sync)
            {
                RunMode mode = current.Mode;
                if (value && mode != RunMode.RUN && mode != RunMode.PAUSE) mode = RunMode.RUN;
                if (!value && mode == RunMode.RUN) mode = RunMode.PAUSE;
                Publish(mode, value, "CompatibilityAutomatic");
            }
        }
        /// <summary>在状态锁中一次替换完整快照。</summary>
        private void Publish(RunMode mode, bool automatic, string reason) { Volatile.Write(ref current, new MachineStateSnapshot(mode, automatic, current.Revision + 1, reason)); }
        /// <summary>提供非法状态转换的三语诊断。</summary>
        private static InvalidOperationException InvalidTransition()
        {
            return new InvalidOperationException(FrameworkLanguage.LangMsg("MachineState", "invalidTransition", "当前机器状态不允许执行此操作。", "The current machine state does not permit this operation.", "El estado actual de la máquina no permite esta operación."));
        }
    }
}
