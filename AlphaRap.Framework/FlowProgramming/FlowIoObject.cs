using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NPSDK;
using NPSDK.Motion;

namespace AlphaRap.FlowProgramming
{
    /// <summary>将既有 SDK IO 控件的公开能力适配到同一流程对象契约。</summary>
    internal sealed class FlowIoObject : IFlowObject
    {
        private readonly object device;
        /// <summary>复用界面的实际 IO 对象，不另行构造驱动。</summary>
        internal FlowIoObject(object device) { this.device = device; }

        /// <summary>只声明该输入、输出或气缸对象具备的方法。</summary>
        public IEnumerable<FlowMethod> GetFlowMethods(bool includeDraft = false)
        {
            NP_Input input = device as NP_Input;
            if (input != null)
            {
                yield return FlowMethods.Sync("io.read", "读取输入", "Read input", "Leer entrada", new FlowField[0], new[] { new FlowField("Value", typeof(bool)) { DisplayName = new[] { "输入状态", "Input state", "Estado de entrada" },
                    Help = new[] { "本次采样状态；需要持续等待时使用等待输入稳定。", "State sampled once; use Wait for stable input to keep waiting.", "Estado de una sola lectura; use Esperar entrada estable para seguir esperando." } } },
                    call => { RequireReady(input.IsReady); return FlowCall.Result("Value", input.IsOn()); });
                FlowMethod wait = FlowMethods.Sync("io.wait", "等待输入稳定", "Wait for stable input", "Esperar entrada estable",
                    new[] { new FlowField("Value", typeof(bool), "True") { DisplayName = new[] { "等待状态", "Expected state", "Estado esperado" },
                        Help = new[] { "True 等待输入有效，False 等待输入无效；执行中持续读取。", "True waits for an active input; False waits for an inactive input. The input is read continuously.", "True espera una entrada activa; False espera una entrada inactiva. La entrada se lee continuamente." } },
                        new FlowField("StableMs", typeof(int), "50") { DisplayName = new[] { "稳定保持时间", "Stable hold time", "Tiempo de estabilidad" }, Unit = "ms", Advanced = true,
                            Help = new[] { "输入达到等待状态后需连续保持的时间；0 表示无需稳定延时。", "Time the input must continuously remain in the expected state; 0 disables the hold delay.", "Tiempo continuo que debe mantenerse el estado esperado; 0 desactiva el retardo de estabilidad." } } },
                    new[] { new FlowField("Value", typeof(bool)) { DisplayName = new[] { "已达到状态", "Reached state", "Estado alcanzado" } } }, call => null);
                wait.Invoke = call => { RequireReady(input.IsReady); return WaitInput(input, new Dictionary<string, object>(call.Inputs), call.Token); };
                yield return wait;
                yield break;
            }
            NP_Output output = device as NP_Output;
            if (output != null)
            {
                yield return FlowMethods.Sync("io.set", "设置输出", "Set output", "Establecer salida", new[] { new FlowField("Value", typeof(bool), "True") { DisplayName = new[] { "输出状态", "Output state", "Estado de salida" },
                    Help = new[] { "True 打开输出，False 关闭输出。", "True turns the output on; False turns it off.", "True activa la salida; False la desactiva." } } },
                    new[] { new FlowField("Value", typeof(bool)) { DisplayName = new[] { "实际输出状态", "Actual output state", "Estado real de salida" } } }, call => {
                        RequireReady(output.IsReady);
                        if (call.Get<bool>("Value")) output.On(); else output.Off();
                        return FlowCall.Result("Value", output.IsOn());
                    });
                yield return FlowMethods.Sync("io.output.read", "读取输出状态", "Read output state", "Leer estado de salida", new FlowField[0],
                    new[] { new FlowField("Value", typeof(bool)) { DisplayName = new[] { "输出状态", "Output state", "Estado de salida" } } }, call => { RequireReady(output.IsReady); return FlowCall.Result("Value", output.IsOn()); });
                yield break;
            }
            NP_Cylinder cylinder = device as NP_Cylinder;
            if (cylinder == null) yield break;
            FlowMethod drive = FlowMethods.Sync("cylinder.set", "气缸动作并等待到位", "Drive cylinder and wait", "Accionar cilindro y esperar",
                new[] { new FlowField("Value", typeof(bool), "True") { DisplayName = new[] { "气缸动作", "Cylinder action", "Acción del cilindro" },
                    Help = new[] { "True 伸出，False 缩回；按此气缸原有到位输入等待完成。", "True extends and False retracts; wait for this cylinder's configured position inputs.", "True extiende y False retrae; espera las entradas de posición configuradas para este cilindro." } } },
                new[] { new FlowField("Value", typeof(bool)) { DisplayName = new[] { "气缸到位状态", "Cylinder position state", "Estado de posición del cilindro" } } }, call => null);
            drive.Invoke = call => DriveCylinder(cylinder, call.Get<bool>("Value"), call.Token);
            yield return drive;
        }

        /// <summary>硬件读取或写入前确认已有驱动。</summary>
        private static void RequireReady(bool ready)
        {
            if (!ready) throw new InvalidOperationException(FlowText.T("error.ioNotReady"));
        }
        /// <summary>持续读取输入并确认稳定保持时间，不重复触发输出。</summary>
        private static async Task<IDictionary<string, object>> WaitInput(NP_Input input, Dictionary<string, object> values, CancellationToken token)
        {
            bool expected = FlowActions.Boolean(values["Value"]);
            int stableMs = FlowActions.Integer(values["StableMs"]);
            if (stableMs < 0) throw new InvalidOperationException(FlowText.T("error.stableTime"));
            System.Diagnostics.Stopwatch stable = new System.Diagnostics.Stopwatch();
            while (true)
            {
                token.ThrowIfCancellationRequested();
                if (input.IsOn() == expected) { if (!stable.IsRunning) stable.Start(); }
                else stable.Reset();
                if (stable.IsRunning && stable.ElapsedMilliseconds >= stableMs) return FlowCall.Result("Value", expected);
                await Task.Delay(20, token).ConfigureAwait(false);
            }
        }

        /// <summary>气缸只发出一次输出命令，之后只检查原有到位输入。</summary>
        private static async Task<IDictionary<string, object>> DriveCylinder(NP_Cylinder cylinder, bool extended, CancellationToken token)
        {
            NP_Input[] on = new[] { cylinder.CyOn_IB1, cylinder.CyOn_IB2, cylinder.CyOn_IB3, cylinder.CyOn_IB4 };
            NP_Input[] off = new[] { cylinder.CyOff_IB1, cylinder.CyOff_IB2, cylinder.CyOff_IB3, cylinder.CyOff_IB4 };
            if (on.All(item => item == null) && off.All(item => item == null)) throw new InvalidOperationException(FlowText.T("error.cylinderInputs"));
            if (on.Concat(off).Any(item => item != null && !item.IsReady)) throw new InvalidOperationException(FlowText.T("error.cylinderInputReady"));
            if (new[] { cylinder.CyOn_OB1, cylinder.CyOn_OB2, cylinder.CyOff_OB1, cylinder.CyOff_OB2 }.Any(item => item != null && !item.IsReady)) throw new InvalidOperationException(FlowText.T("error.cylinderOutputReady"));
            if (extended) cylinder.On(); else cylinder.Off();
            try
            {
                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    bool arrived = on.Where(item => item != null).All(item => item.IsOn() == extended)
                        && off.Where(item => item != null).All(item => item.IsOn() != extended);
                    if (arrived) return FlowCall.Result("Value", extended);
                    await Task.Delay(20, token).ConfigureAwait(false);
                }
            }
            catch { cylinder.Stop(); throw; }
        }


    }
}
