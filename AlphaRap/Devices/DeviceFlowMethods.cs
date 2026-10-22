using System;
using System.Collections.Generic;
using System.Linq;
using NPSDK;
using AlphaRap.FlowProgramming;

namespace AlphaRap
{
    public abstract partial class AbstractDevice : IFlowObject
    {
        /// <summary>提供父类的通用连接和协议能力，子类可增加自己的具体方法。</summary>
        public virtual IEnumerable<FlowMethod> GetFlowMethods(bool includeDraft = false)
        {
            yield return FlowMethods.Sync("device.open", "连接设备", "Open device", "Conectar dispositivo", new FlowField[0],
                new[] { new FlowField("Connected", typeof(bool)) }, call => {
                    Open();
                    if (!IsConnected) throw new InvalidOperationException(FlowText.T("error.deviceConnect") + DeviceName);
                    return FlowCall.Result("Connected", true);
                });
            yield return FlowMethods.Sync("device.close", "断开设备", "Close device", "Desconectar dispositivo", new FlowField[0],
                new[] { new FlowField("Connected", typeof(bool)) }, call => {
                    Close();
                    if (IsConnected) throw new InvalidOperationException(FlowText.T("error.deviceClose"));
                    return FlowCall.Result("Connected", false);
                });
            yield return FlowMethods.Sync("device.status", "读取设备状态", "Read device status", "Leer estado del dispositivo", new FlowField[0],
                new[] { new FlowField("Connected", typeof(bool)), new FlowField("Running", typeof(bool)) },
                call => new Dictionary<string, object> { { "Connected", IsConnected }, { "Running", IsRunning } });
            if (!SupportsRawIo) yield break;
            yield return FlowMethods.Sync("device.send", "发送命令", "Send command", "Enviar comando",
                new[] { new FlowField("Text", typeof(string)) }, new[] { new FlowField("Value", typeof(bool)) }, call => {
                    string error = Send(call.Get<string>("Text"));
                    if (!string.IsNullOrEmpty(error)) throw new InvalidOperationException(error);
                    return FlowCall.Result("Value", true);
                });
            yield return FlowMethods.Sync("device.receive", "读取接收数据", "Receive data", "Recibir datos", new FlowField[0],
                new[] { new FlowField("Value", typeof(string)) }, call => FlowCall.Result("Value", Receive()));
        }
    }

    public partial class ScannerKeyenceTcp
    {
        /// <summary>声明扫码枪实例的单次触发方法，空条码按失败处理。</summary>
        public override IEnumerable<FlowMethod> GetFlowMethods(bool includeDraft = false)
        {
            foreach (FlowMethod method in base.GetFlowMethods(includeDraft)) yield return method;
            yield return FlowMethods.Sync("scanner.once", "扫码一次", "Scan once", "Escanear una vez", new FlowField[0],
                new[] { new FlowField("Barcode", typeof(string)) }, call => {
                    string barcode = ScanOnce();
                    if (string.IsNullOrWhiteSpace(barcode)) throw new InvalidOperationException(FlowText.T("error.scannerEmpty"));
                    return FlowCall.Result("Barcode", barcode);
                });
        }
    }



    partial class HIOKI_RM3545
    {
        /// <summary>关闭本电阻计当前串口连接。</summary>
        public override void Close()
        {
            if (MyCom.IsOpen) MyCom.Close();
        }

        /// <summary>按本仪表实际通道数声明具名结果，禁止把长度错误当作成功。</summary>
        public override IEnumerable<FlowMethod> GetFlowMethods(bool includeDraft = false)
        {
            foreach (FlowMethod method in base.GetFlowMethods(includeDraft)) yield return method;
            FlowMethod read = FlowMethods.Sync("hioki.result", "读取电阻结果", "Read resistance results", "Leer resultados de resistencia", new FlowField[0],
                new[] { new FlowField("Valid", typeof(bool)), new FlowField("Values", typeof(double[])) }, call => {
                    bool valid = false;
                    List<double> values = GetFinalResult(ref valid);
                    if (!valid) throw new InvalidOperationException(FlowText.T("error.resistanceLength"));
                    Dictionary<string, object> result = new Dictionary<string, object> { { "Valid", true }, { "Values", values.ToArray() } };
                    for (int index = 0; index < values.Count; index++) result.Add("Value" + index, values[index]);
                    return result;
                });
            for (int index = 0; index < ChannelNum; index++) read.Outputs.Add(new FlowField("Value" + index, typeof(double)));
            yield return read;
        }
    }

    public partial class Keyence3DTcp
    {
        /// <summary>声明该 3D 控制器自己的触发和程序选择方法。</summary>
        public override IEnumerable<FlowMethod> GetFlowMethods(bool includeDraft = false)
        {
            foreach (FlowMethod method in base.GetFlowMethods(includeDraft)) yield return method;
            yield return FlowMethods.Sync("keyence.trigger", "触发 3D", "Trigger 3D", "Activar 3D",
                new[] { new FlowField("Position", typeof(int), "0") }, new[] { new FlowField("Value", typeof(string)) },
                call => FlowCall.Result("Value", Triger3D(call.Get<int>("Position"))));
            yield return FlowMethods.Sync("keyence.change", "切换 3D 程序", "Change 3D program", "Cambiar programa 3D",
                new[] { new FlowField("Position", typeof(int), "0") }, new[] { new FlowField("Value", typeof(string)) },
                call => FlowCall.Result("Value", Change3D(call.Get<int>("Position"))));
        }
    }


}

namespace AlaphaRap
{
    public partial class ATEQ_F620
    {
        /// <summary>声明当前气密仪实例的结果读取能力。</summary>
        public override IEnumerable<FlowMethod> GetFlowMethods(bool includeDraft = false)
        {
            foreach (FlowMethod method in base.GetFlowMethods(includeDraft)) yield return method;
            yield return FlowMethods.Sync("ateq.result", "读取气密结果", "Read leak test result", "Leer prueba de fugas", new FlowField[0],
                new[] { new FlowField("Value", typeof(string)) }, call => FlowCall.Result("Value", Readdata()));
        }
    }

    public partial class ModBus_RTU
    {
        /// <summary>声明寄存器读取方法，参数初值来自这个串口对象的实际配置。</summary>
        public override IEnumerable<FlowMethod> GetFlowMethods(bool includeDraft = false)
        {
            foreach (FlowMethod method in base.GetFlowMethods(includeDraft)) yield return method;
            yield return FlowMethods.Sync("modbus.read", "读取 Modbus 寄存器", "Read Modbus registers", "Leer registros Modbus",
                new[] { new FlowField("Slave", typeof(byte), Slaveld.ToString()), new FlowField("Start", typeof(ushort), StartRegister.ToString()),
                    new FlowField("Count", typeof(ushort), Count.ToString()), new FlowField("DelayMs", typeof(int), ThreadSleep.ToString()) },
                new[] { new FlowField("Bytes", typeof(byte[])), new FlowField("Hex", typeof(string)) }, call => {
                    byte[] bytes = ReadOutputRegisters(call.Get<byte>("Slave"), call.Get<ushort>("Start"), call.Get<ushort>("Count"), call.Get<int>("DelayMs"));
                    if (bytes == null) throw new InvalidOperationException(FlowText.T("error.modbusEmpty"));
                    return new Dictionary<string, object> { { "Bytes", bytes }, { "Hex", BitConverter.ToString(bytes) } };
                });
        }
    }
}
