using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NPSDK;
using NPSDK.Motion;

namespace AlphaRap.FlowProgramming
{
    /// <summary>轴实例及它在轴页面绑定的点位快照，点位表不再作为新节点的独立运动对象。</summary>
    internal sealed class FlowAxisObject : IFlowObject
    {
        private readonly NP_Motor motor;
        private readonly List<FlowResource> tables;
        private readonly bool legacyPoint;

        /// <summary>绑定实际轴与其关联表，兼容已保存的单表引用。</summary>
        internal FlowAxisObject(NP_Motor motor, IEnumerable<FlowResource> tables, bool legacyPoint = false)
        {
            this.motor = motor; this.tables = tables.ToList(); this.legacyPoint = legacyPoint;
        }

        /// <summary>轴方法只访问本轴；点位选择范围限定为绑定该轴的表。</summary>
        public IEnumerable<FlowMethod> GetFlowMethods(bool includeDraft = false)
        {
            if (!legacyPoint)
            {
                yield return FlowMethods.Sync("axis.position", "读取编码器位置", "Read encoder position", "Leer posición del codificador", new FlowField[0],
                    new[] { new FlowField("Position", typeof(double)) }, call => { RequireReady(false); return FlowCall.Result("Position", motor.GetEncPos()); });
                yield return FlowMethods.Sync("axis.command", "读取指令位置", "Read command position", "Leer posición de comando", new FlowField[0],
                    new[] { new FlowField("Position", typeof(double)) }, call => { RequireReady(false); return FlowCall.Result("Position", motor.GetPos()); });
                yield return FlowMethods.Sync("axis.stop", "停止轴", "Stop axis", "Detener eje", new FlowField[0], new[] { new FlowField("Position", typeof(double)) },
                    call => { RequireReady(false); motor.Stop(); return FlowCall.Result("Position", motor.GetEncPos()); });
                FlowMethod home = FlowMethods.Sync("axis.home", "轴回零并等待完成", "Home axis and wait", "Referenciar eje y esperar", new FlowField[0],
                    new[] { new FlowField("Position", typeof(double)) }, call => null);
                home.Invoke = Home;
                yield return home;
                yield return FlowMethods.Sync("axis.reset", "复位轴报警", "Reset axis alarm", "Restablecer alarma de eje", new FlowField[0], new FlowField[0],
                    call => { RequireReady(true); motor.AlarmReset(); return new Dictionary<string, object>(); });
            }
            if (tables.Count == 0) yield break;
            FlowField index = new FlowField("Index", typeof(int), "0");
            if (tables.Count == 1) index.Options.AddRange(tables[0].Points.Select(point => new FlowOption { Value = point.Id, Label = point.Label }));
            FlowMethod move = FlowMethods.Sync("point.move", "移动到绑定表的点位", "Move to a bound table point", "Mover a un punto de la tabla vinculada",
                new[] { index, new FlowField("Offset", typeof(double), "0"), new FlowField("Tolerance", typeof(double), "0.2") },
                new[] { new FlowField("Index", typeof(int)), new FlowField("Position", typeof(double)) }, call => null);
            move.Invoke = Move;
            move.Simulate = Move;
            move.ValidateInputs = ValidatePoint;
            yield return move;
        }

        /// <summary>保存前检查可确定的点位绑定，动态来源在执行前使用同一规则校验。</summary>
        private void ValidatePoint(IDictionary<string, object> values)
        {
            FlowResource table = BoundTable();
            if (!values.ContainsKey("Index")) return;
            double position;
            FlowPointAxis points = (FlowPointAxis)table.Instance;
            if (!points.Positions.TryGetValue(FlowActions.Integer(values["Index"]), out position)) throw new InvalidOperationException(FlowText.T("error.pointIndex"));
            if (values.ContainsKey("Tolerance") && FlowActions.Number(values["Tolerance"]) <= 0) throw new InvalidOperationException(FlowText.T("error.pointIndex"));
            if (!values.ContainsKey("Offset")) return;
            position = FlowActions.Number(position + FlowActions.Number(values["Offset"]));
            if (position < points.Low || position > points.High) throw new InvalidOperationException(FlowText.T("error.pointLimit"));
        }

        /// <summary>按表身份与索引取坐标，偏移必须显式绑定并满足该列限位。</summary>
        private async Task<IDictionary<string, object>> Move(FlowCall call)
        {
            FlowResource table = BoundTable();
            FlowPointAxis points = (FlowPointAxis)table.Instance;
            int index = call.Get<int>("Index");
            double target, tolerance = call.Get<double>("Tolerance");
            if (tolerance <= 0 || !points.Positions.TryGetValue(index, out target)) throw new InvalidOperationException(FlowText.T("error.pointIndex"));
            target = FlowActions.Number(target + call.Get<double>("Offset"));
            if (target < points.Low || target > points.High) throw new InvalidOperationException(FlowText.T("error.pointLimit"));
            if (call.Simulation) return new Dictionary<string, object> { { "Index", index }, { "Position", target }, { "Simulated", true } };
            RequireReady(true);
            try
            {
                call.Token.ThrowIfCancellationRequested();
                motor.Goto(target);
                await WaitStopped(call).ConfigureAwait(false);
                double actual = motor.GetEncPos();
                if (Math.Abs(actual - target) > tolerance) throw new InvalidOperationException(FlowText.T("error.axisArrival"));
                return new Dictionary<string, object> { { "Index", index }, { "Position", actual } };
            }
            catch { await StopAndWait().ConfigureAwait(false); throw; }
        }

        /// <summary>只使用轴页面明确绑定的唯一点位表，多重绑定必须先在原页面修正。</summary>
        private FlowResource BoundTable()
        {
            if (tables.Count != 1) throw new InvalidOperationException(FlowText.T("boundTable"));
            return tables[0];
        }

        /// <summary>调用本轴回零，等待完成；取消后等待物理运动结束。</summary>
        private async Task<IDictionary<string, object>> Home(FlowCall call)
        {
            RequireReady(true); motor.HomeReset();
            try
            {
                while (true)
                {
                    call.Token.ThrowIfCancellationRequested();
                    if (motor.Home()) break;
                    await Task.Delay(20, call.Token).ConfigureAwait(false);
                }
                return FlowCall.Result("Position", motor.GetEncPos());
            }
            catch { await StopAndWait().ConfigureAwait(false); throw; }
        }

        /// <summary>读取动作需要初始化，发起运动前还需要轴空闲。</summary>
        private void RequireReady(bool idle)
        {
            if (!motor.IsReady || (idle && motor.GetBusy())) throw new InvalidOperationException(FlowText.T("error.axisReady"));
        }

        /// <summary>等待当前运动结束，不重复发送运动指令。</summary>
        private async Task WaitStopped(FlowCall call)
        {
            while (motor.GetBusy()) await Task.Delay(20, call.Token).ConfigureAwait(false);
            call.Token.ThrowIfCancellationRequested();
        }

        /// <summary>停轴后保留占用至驱动确认静止，避免取消后另一节点接管运动轴。</summary>
        private async Task StopAndWait()
        {
            motor.Stop();
            while (motor.GetBusy()) await Task.Delay(20).ConfigureAwait(false);
        }
    }
}
