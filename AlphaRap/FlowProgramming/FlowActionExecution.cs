using System;
using AlaphaRap;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NPSDK;
using NPSDK.Motion;

namespace AlphaRap.FlowProgramming
{
    public sealed partial class FlowActions
    {
        /// <summary>按登记动作执行具体框架方法，不接受任意类名或反射方法调用。</summary>
        private async Task<IDictionary<string, object>> ExecuteCore(FlowStep step, FlowExecutionContext context,
            Dictionary<string, object> input, FlowResource resource, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (step.Action == "value") return Result("Value", input["Value"]);
            if (step.Action == "calculate")
            {
                double a = Number(input["A"]), b = Number(input["B"]);
                string operation = Convert.ToString(input["Operation"]);
                double value;
                switch (operation)
                {
                    case "+": value = a + b; break;
                    case "-": value = a - b; break;
                    case "*": value = a * b; break;
                    case "/": if (b == 0) throw new InvalidOperationException("Division by zero."); value = a / b; break;
                    default: throw new InvalidOperationException("Select +, -, * or /.");
                }
                return Result("Value", Number(value));
            }
            if (step.Action == "counter")
            {
                int start = Integer(input["Start"]), end = Integer(input["End"]), increment = Integer(input["Step"]);
                if (increment <= 0 || end < start) throw new InvalidOperationException("Counter requires End >= Start and Step > 0.");
                object previous;
                long next = context.Variables.TryGetValue(step.Id, out previous) ? (long)(int)previous + increment : start;
                int index = next > end ? start : (int)next;
                context.Variables[step.Id] = index;
                return Result("Index", index);
            }
            if (step.Action == "tray.position")
            {
                FlowTray tray = (FlowTray)resource.Instance;
                int index = Integer(input["Index"]);
                if (tray.Rows < 1 || tray.Columns < 1 || index < 0 || index >= (long)tray.Rows * tray.Columns) throw new InvalidOperationException("Tray pocket index is outside the configured layout.");
                int row = index / tray.Columns, column = index % tray.Columns;
                return new Dictionary<string, object> { { "Row", row }, { "Column", column }, { "OffsetX", column * tray.ColumnPitch }, { "OffsetY", row * tray.RowPitch } };
            }
            if (step.Action == "delay")
            {
                int delay = Integer(input["Milliseconds"]);
                if (delay < 0) throw new InvalidOperationException("Delay must be nonnegative.");
                await Task.Delay(delay, token).ConfigureAwait(false);
                return Result("ElapsedMs", delay);
            }
            if (step.Action == "cycle.begin")
            {
                string key = Convert.ToString(input["Key"], CultureInfo.InvariantCulture);
                context.BeginCycle(key);
                return Result("CycleId", context.CycleId);
            }
            if (step.Action == "cycle.end") { context.CompleteCycle(); return new Dictionary<string, object>(); }
            if (step.Action == "operator.input")
            {
                if (RequestInput == null) throw new InvalidOperationException("Operator input host is unavailable.");
                return Result("Value", await RequestInput(Convert.ToString(input["Prompt"]), token).ConfigureAwait(false));
            }
            if (step.Action == "data.save")
            {
                string id = context.RunId + "/" + context.ModuleId + "/" + context.ExecutionId;
                string path = ((DataForm)resource.Instance).SaveProductionRecord(id, context.CycleId, context.RecipeName, input, Simulate);
                return new Dictionary<string, object> { { "RecordId", id }, { "Path", path } };
            }
            if (step.Action == "alarm.show")
            {
                string code = Convert.ToString(input["Code"]);
                if (string.IsNullOrWhiteSpace(code)) throw new InvalidOperationException("Alarm code is required.");
                if (!Simulate) Alarm.Show(code);
                return Result("Code", code);
            }
            if (Simulate) return SimulatedResult(step, input, resource);
            if (step.Action.StartsWith("plc."))
            {
                PLC.INOVANCE plc = (PLC.INOVANCE)resource.Instance;
                return await Task.Run(() => ExecutePlc(step.Action, plc, input), token).ConfigureAwait(false);
            }
            NP_Input inputPoint = resource == null ? null : resource.Instance as NP_Input;
            NP_Output outputPoint = resource == null ? null : resource.Instance as NP_Output;
            if (inputPoint != null && !inputPoint.IsReady) throw new InvalidOperationException("Input is not initialized.");
            if (outputPoint != null && !outputPoint.IsReady) throw new InvalidOperationException("Output is not initialized.");
            NP_Motor motor = resource == null ? null : resource.Instance as NP_Motor;
            if (motor != null && !motor.IsReady) throw new InvalidOperationException("Axis is not initialized.");
            switch (step.Action)
            {
                case "io.read": return Result("Value", ((NP_Input)resource.Instance).IsOn());
                case "io.wait": return await WaitInput((NP_Input)resource.Instance, input, token).ConfigureAwait(false);
                case "io.set":
                    NP_Output output = (NP_Output)resource.Instance;
                    if (Boolean(input["Value"])) output.On(); else output.Off();
                    return Result("Value", output.IsOn());
                case "cylinder.set": return await DriveCylinder((NP_Cylinder)resource.Instance, Boolean(input["Value"]), token).ConfigureAwait(false);
                case "point.move": return await MovePoint((FlowPointAxis)resource.Instance, input, token).ConfigureAwait(false);
                case "axis.position": return Result("Position", ((NP_Motor)resource.Instance).GetEncPos());
                case "axis.stop": ((NP_Motor)resource.Instance).Stop(); return Result("Position", ((NP_Motor)resource.Instance).GetEncPos());
                case "vision.run": return await RunVision((VisionproInterface)resource.Instance, token).ConfigureAwait(false);
                default: return await Task.Run(() => ExecuteDevice(step.Action, (AbstractDevice)resource.Instance, input), token).ConfigureAwait(false);
            }
        }

        /// <summary>模拟模式产生明确标记的结构化结果，仍校验实际点位关联并真实保存本地记录。</summary>
        private IDictionary<string, object> SimulatedResult(FlowStep step, Dictionary<string, object> input, FlowResource resource)
        {
            if (step.Action == "point.move")
            {
                int index = Integer(input["Index"]);
                FlowPointAxis point = (FlowPointAxis)resource.Instance;
                double position;
                if (!point.Positions.TryGetValue(index, out position)) throw new InvalidOperationException("Point index is missing or uncommitted.");
                position += Number(input["Offset"]);
                if (position < point.Low || position > point.High) throw new InvalidOperationException("Compensated target exceeds point limits.");
                return new Dictionary<string, object> { { "Index", index }, { "Position", position }, { "Simulated", true } };
            }
            FlowAction action = Actions.Single(item => item.Id == step.Action);
            Dictionary<string, object> result = OutputNames(action, resource).ToDictionary(name => name, name => (object)0);
            if (step.Action == "scanner.once") result["Barcode"] = "SIM-" + Guid.NewGuid().ToString("N");
            if (step.Action.StartsWith("io.") || step.Action == "cylinder.set") result["Value"] = input.ContainsKey("Value") ? Boolean(input["Value"]) : false;
            if (step.Action == "device.open" || step.Action == "device.close") result["Connected"] = step.Action == "device.open";
            result["Simulated"] = true;
            return result;
        }

        /// <summary>持续读取输入并确认稳定保持时间，不重复触发输出。</summary>
        private static async Task<IDictionary<string, object>> WaitInput(NP_Input input, Dictionary<string, object> values, CancellationToken token)
        {
            bool expected = Boolean(values["Value"]);
            int stableMs = Integer(values["StableMs"]);
            if (stableMs < 0) throw new InvalidOperationException("Stable time must be nonnegative.");
            System.Diagnostics.Stopwatch stable = new System.Diagnostics.Stopwatch();
            while (true)
            {
                token.ThrowIfCancellationRequested();
                if (input.IsOn() == expected) { if (!stable.IsRunning) stable.Start(); }
                else stable.Reset();
                if (stable.IsRunning && stable.ElapsedMilliseconds >= stableMs) return Result("Value", expected);
                await Task.Delay(20, token).ConfigureAwait(false);
            }
        }

        /// <summary>气缸只发出一次输出命令，之后只检查原有到位输入。</summary>
        private static async Task<IDictionary<string, object>> DriveCylinder(NP_Cylinder cylinder, bool extended, CancellationToken token)
        {
            NP_Input[] on = new[] { cylinder.CyOn_IB1, cylinder.CyOn_IB2, cylinder.CyOn_IB3, cylinder.CyOn_IB4 };
            NP_Input[] off = new[] { cylinder.CyOff_IB1, cylinder.CyOff_IB2, cylinder.CyOff_IB3, cylinder.CyOff_IB4 };
            if (on.All(item => item == null) && off.All(item => item == null)) throw new InvalidOperationException("Cylinder arrival inputs are not configured.");
            if (on.Concat(off).Any(item => item != null && !item.IsReady)) throw new InvalidOperationException("Cylinder input is not initialized.");
            if (new[] { cylinder.CyOn_OB1, cylinder.CyOn_OB2, cylinder.CyOff_OB1, cylinder.CyOff_OB2 }.Any(item => item != null && !item.IsReady)) throw new InvalidOperationException("Cylinder output is not initialized.");
            if (extended) cylinder.On(); else cylinder.Off();
            try
            {
                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    bool arrived = on.Where(item => item != null).All(item => item.IsOn() == extended)
                        && off.Where(item => item != null).All(item => item.IsOn() != extended);
                    if (arrived) return Result("Value", extended);
                    await Task.Delay(20, token).ConfigureAwait(false);
                }
            }
            catch { cylinder.Stop(); throw; }
        }

        /// <summary>按点位快照启动一次运动，等待停止及位置确认；取消后确认轴停止再释放占用。</summary>
        private static async Task<IDictionary<string, object>> MovePoint(FlowPointAxis axis, Dictionary<string, object> input, CancellationToken token)
        {
            int index = Integer(input["Index"]);
            double tolerance = Number(input["Tolerance"]);
            double target;
            if (tolerance <= 0 || !axis.Positions.TryGetValue(index, out target)) throw new InvalidOperationException("Invalid point index or tolerance.");
            target += Number(input["Offset"]);
            if (target < axis.Low || target > axis.High) throw new InvalidOperationException("Compensated target exceeds point limits.");
            if (!axis.Motor.IsReady || axis.Motor.GetBusy()) throw new InvalidOperationException("Axis is not ready or is already moving.");
            if (!axis.Motor.Goto(target)) throw new InvalidOperationException("Axis rejected the movement command.");
            try
            {
                while (axis.Motor.GetBusy()) await Task.Delay(20, token).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                double actual = axis.Motor.GetEncPos();
                if (Math.Abs(actual - target) > tolerance) throw new InvalidOperationException("Axis stopped before reaching the point.");
                return new Dictionary<string, object> { { "Index", index }, { "Position", actual } };
            }
            catch
            {
                axis.Motor.Stop();
                while (axis.Motor.GetBusy()) await Task.Delay(20).ConfigureAwait(false);
                throw;
            }
        }

        /// <summary>等待同一 VPP 请求，取消后等待底层 Busy 解除，不接纳迟到结果。</summary>
        private static async Task<IDictionary<string, object>> RunVision(VisionproInterface station, CancellationToken token)
        {
            long id;
            if (!station.TryRunTB(out id)) throw new InvalidOperationException(station.LastError);
            try
            {
                while (station.LastRunResult != null && station.LastRunResult.RunId == id && station.LastRunResult.State == VisionRunState.Running)
                    await Task.Delay(20, token).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                VisionRunResult result = station.LastRunResult;
                if (result == null || result.RunId != id || result.State != VisionRunState.Succeeded) throw new InvalidOperationException(result == null ? "Missing vision result." : result.Error);
                Dictionary<string, object> output = new Dictionary<string, object> { { "RunId", id }, { "X", result.X }, { "Y", result.Y }, { "Angle", result.Angle } };
                foreach (Cognex.VisionPro.ToolBlock.CogToolBlockTerminal terminal in station.TB.Outputs) output.Add("TB:" + terminal.Name, terminal.Value);
                return output;
            }
            catch
            {
                station.CancelRun(id);
                while (station.IsBusy) await Task.Delay(20).ConfigureAwait(false);
                throw;
            }
        }

        /// <summary>调用已登记设备协议，保留具名结果和原始数据，不把空响应视为扫码成功。</summary>
        private static IDictionary<string, object> ExecuteDevice(string action, AbstractDevice device, Dictionary<string, object> input)
        {
            switch (action)
            {
                case "device.open": device.Open(); if (!device.IsConnected) throw new InvalidOperationException("Device connection failed."); return Result("Connected", true);
                case "device.close": device.Close(); return Result("Connected", device.IsConnected);
                case "device.send":
                    string error = device.Send(Convert.ToString(input["Text"]));
                    if (!string.IsNullOrEmpty(error)) throw new InvalidOperationException(error);
                    return Result("Value", true);
                case "device.receive": return Result("Value", device.Receive());
                case "scanner.once":
                    string barcode = ((ScannerKeyenceTcp)device).ScanOnce();
                    if (string.IsNullOrWhiteSpace(barcode)) throw new InvalidOperationException("Scanner returned no barcode.");
                    return Result("Barcode", barcode);
                case "ateq.result": return Result("Value", ((ATEQ_F620)device).Readdata());
                case "hioki.result":
                    bool valid = false;
                    List<double> values = ((HIOKI_RM3545)device).GetFinalResult(ref valid);
                    if (!valid) throw new InvalidOperationException("Resistance result length is invalid.");
                    Dictionary<string, object> result = new Dictionary<string, object> { { "Valid", valid }, { "Values", values.ToArray() } };
                    for (int i = 0; i < values.Count; i++) result.Add("Value" + i, values[i]);
                    return result;
                case "keyence.trigger": return Result("Value", ((Keyence3DTcp)device).Triger3D(Integer(input["Position"])));
                case "keyence.change": return Result("Value", ((Keyence3DTcp)device).Change3D(Integer(input["Position"])));
                case "modbus.read":
                    byte[] bytes = ((ModBus_RTU)device).ReadOutputRegisters(checked((byte)Integer(input["Slave"])), checked((ushort)Integer(input["Start"])), checked((ushort)Integer(input["Count"])), Integer(input["DelayMs"]));
                    if (bytes == null) throw new InvalidOperationException("Modbus returned no result.");
                    return new Dictionary<string, object> { { "Bytes", bytes }, { "Hex", BitConverter.ToString(bytes) } };
                default: throw new InvalidOperationException("Unsupported device action: " + action);
            }
        }

        /// <summary>通过现有 PLC 对象读写，通信错误与合法值零分开。</summary>
        private static IDictionary<string, object> ExecutePlc(string action, PLC.INOVANCE plc, Dictionary<string, object> input)
        {
            string address = Convert.ToString(input["Address"]), type = Convert.ToString(input["Type"]);
            if (action == "plc.read")
            {
                string value = plc.ReadPlc(address, Integer(input["Count"]), type);
                if (value == "-999") throw new InvalidOperationException(plc.ReadErrorMessage);
                return Result("Value", value);
            }
            string data = FlowValue.Text(input["Value"]);
            if (!plc.WritePlc(address, data, type)) throw new InvalidOperationException(plc.WritErrorMessage);
            return Result("Value", data);
        }
    }
}
