using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AlphaRap.TaskProcess;
using NPSDK;
using static AlphaRap.FrameworkLanguage;

namespace AlphaRap
{
    internal sealed class AlwaysRunTask
    {
        private const string SafetyAlarmCode = "3023";
        private static readonly string[] SafetyInputs =
        {
            "IB_Front_EmergencyStop",
            "IB_Back_EmergencyStop",
            "IB_Air",
            "IB_SafetyReady"
        };

        private readonly Stopwatch scanTimer = Stopwatch.StartNew();
        private readonly Dictionary<string, InputWatch> inputWatches = new Dictionary<string, InputWatch>(StringComparer.OrdinalIgnoreCase);
        private bool?[] previousSafety;
        private readonly JTimer blinkTimer = new JTimer();
        private bool blinkIsOn;

        /// <summary>按资源名称登记普通 IO 状态回调。</summary>
        internal AlwaysRunTask()
        {
            // WatchInput("IB_MaterialLow", on => Debug.WriteLine(on ? "缺料" : "料位正常"));
        }

        /// <summary>返回初始化时要连接的输入名称。</summary>
        internal IEnumerable<string> InputNames => SafetyInputs.Concat(inputWatches.Keys);

        /// <summary>按名称登记普通输入，取得首个有效值或有效状态变化时执行回调。</summary>
        private void WatchInput(string name, Action<bool> onChanged)
        {
            inputWatches.Add(name, new InputWatch(onChanged));
        }

        /// <summary>在真机模式监视运行许可和普通输入，并更新本站闪烁时基。</summary>
        internal void AlwaysRun(StationIo io, Action stopOnFault)
        {
            if (SysPara.Simulation)
            {
                previousSafety = null;
                foreach (InputWatch watch in inputWatches.Values) watch.Update(null);
                return;
            }
            if (blinkTimer.IsOn(500))
            {
                blinkIsOn = !blinkIsOn;
                blinkTimer.Restart();
            }
            if (io == null || scanTimer.ElapsedMilliseconds < 100) return;
            scanTimer.Restart();

            ScanSafety(io, stopOnFault);
            ScanInputs(io);
        }

        /// <summary>刷新普通输入，隔离单点采样和回调异常。</summary>
        private void ScanInputs(StationIo io)
        {
            foreach (var entry in inputWatches)
            {
                try { entry.Value.Update(io.ReadNamedInput(entry.Key)); }
                catch (Exception error) { Debug.WriteLine(entry.Key + " " + error); }
            }
        }

        /// <summary>监视运行许可变化，并通过既有入口请求整机停止。</summary>
        private void ScanSafety(StationIo io, Action stopOnFault)
        {
            bool?[] states = SampleSafety(io);
            bool changed = previousSafety == null || !previousSafety.SequenceEqual(states);
            bool initialUnknown = previousSafety == null && states.All(state => !state.HasValue);
            if (states.All(state => state == true))
            {
                previousSafety = states;
                return;
            }
            stopOnFault();
            if (!changed || initialUnknown) return;

            previousSafety = states;
            if (Alarm.AlarmList != null) Alarm.Show(SafetyAlarmCode, DescribeFault(io, states), false, "I");
        }

        /// <summary>在派发初始化、启动或继续前重新读取现场状态，返回阻止动作的原因。</summary>
        internal static string SafetyBlockReason(StationIo io)
        {
            return SysPara.Simulation ? null : DescribeFault(io, SampleSafety(io));
        }

        /// <summary>逐点读取健康状态，单点异常保留为未知并继续读取其他点。</summary>
        private static bool?[] SampleSafety(StationIo io)
        {
            var states = new bool?[SafetyInputs.Length];
            for (int index = 0; index < states.Length; index++)
            {
                try { states[index] = io?.ReadSafetyInput(SafetyInputs[index]); }
                catch (Exception error) { Debug.WriteLine(SafetyInputs[index] + " " + error); }
            }
            return states;
        }

        /// <summary>按当前语言列出异常输入，区分状态异常和无法读取。</summary>
        private static string DescribeFault(StationIo io, bool?[] states)
        {
            var details = new List<string>(SafetyInputs.Length);
            for (int index = 0; index < states.Length; index++)
            {
                if (states[index] == true) continue;
                string key = SafetyInputs[index];
                string name = io?.SafetyInputCaption(key) ?? key;
                string reason = states[index].HasValue
                    ? LangMsg("SafetyMonitor", "inactive", "{0}条件未满足", "{0} is not ready", "{0} no está listo")
                    : LangMsg("SafetyMonitor", "unavailable", "{0}无法读取，请检查 IO 配置和连接", "{0} cannot be read. Check the I/O configuration and connection", "No se puede leer {0}. Compruebe la configuración y la conexión de E/S");
                details.Add(string.Format(reason, name));
            }
            if (details.Count == 0) return null;
            string title = ApplicationHost.GetAlarmText(SafetyAlarmCode);
            if (title == "Error" || string.IsNullOrWhiteSpace(title))
                title = LangMsg("SafetyMonitor", "fault", "设备运行条件异常", "Machine operating conditions are not met", "No se cumplen las condiciones de funcionamiento");
            return title + " · " + string.Join(" / ", details);
        }

        private sealed class InputWatch
        {
            private readonly Action<bool> onChanged;
            private bool? lastState;

            /// <summary>保存一个输入状态变化时的回调。</summary>
            internal InputWatch(Action<bool> onChanged)
            {
                this.onChanged = onChanged ?? throw new ArgumentNullException(nameof(onChanged));
            }

            /// <summary>仅发布有效状态，读数失效后重新取得的值作为首个有效值。</summary>
            internal void Update(bool? state)
            {
                if (state == lastState) return;
                lastState = state;
                if (state.HasValue) onChanged(state.Value);
            }
        }
    }
}
