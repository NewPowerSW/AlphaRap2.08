using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NPSDK;
using AlphaRap.FlowProgramming;

namespace AlphaRap
{
    public abstract partial class AbstractDevice : IFlowObject
    {
        /// <summary>提供父类的通用连接和协议能力，子类可增加自己的具体方法。</summary>
        public virtual IEnumerable<FlowMethod> GetFlowMethods(bool includeDraft = false)
        {
            foreach (FlowMethod method in FlowInheritedMethods.Discover(this)) yield return method;
            yield return FlowMethods.Sync("device.open", "连接设备", "Open device", "Conectar dispositivo", new FlowField[0],
                new[] { new FlowField("Connected", typeof(bool)) { DisplayName = new[] { "已连接", "Connected", "Conectado" } } }, call => {
                    Open();
                    if (!IsConnected) throw new InvalidOperationException(FlowText.T("error.deviceConnect") + DeviceName);
                    return FlowCall.Result("Connected", true);
                });
            yield return FlowMethods.Sync("device.close", "断开设备", "Close device", "Desconectar dispositivo", new FlowField[0],
                new[] { new FlowField("Connected", typeof(bool)) { DisplayName = new[] { "已连接", "Connected", "Conectado" } } }, call => {
                    Close();
                    if (IsConnected) throw new InvalidOperationException(FlowText.T("error.deviceClose"));
                    return FlowCall.Result("Connected", false);
                });
            yield return FlowMethods.Sync("device.status", "读取设备状态", "Read device status", "Leer estado del dispositivo", new FlowField[0],
                new[] { new FlowField("Connected", typeof(bool)) { DisplayName = new[] { "已连接", "Connected", "Conectado" } },
                    new FlowField("Running", typeof(bool)) { DisplayName = new[] { "设备运行中", "Device running", "Dispositivo en ejecución" } } },
                call => new Dictionary<string, object> { { "Connected", IsConnected }, { "Running", IsRunning } });
            if (OverridesLifecycle("StartRunning"))
                yield return FlowMethods.Sync("device.start", "启动设备", "Start device", "Iniciar dispositivo", new FlowField[0], new FlowField[0],
                    call => { StartRunning(); return new Dictionary<string, object>(); });
            if (OverridesLifecycle("StopRunning"))
                yield return FlowMethods.Sync("device.stop", "停止设备", "Stop device", "Detener dispositivo", new FlowField[0], new FlowField[0],
                    call => { StopRunning(); return new Dictionary<string, object>(); });
            if (!SupportsRawIo) yield break;
            yield return FlowMethods.Sync("device.send", "发送命令", "Send command", "Enviar comando",
                new[] { new FlowField("Text", typeof(string)) { DisplayName = new[] { "发送命令", "Command text", "Texto del comando" },
                    Help = new[] { "按此设备协议发送原始文本，不自动替换业务内容。", "Send raw text according to this device's protocol; the text is not replaced.", "Envía texto sin procesar según el protocolo de este dispositivo; el texto no se sustituye." } } },
                new[] { new FlowField("Value", typeof(bool)) { DisplayName = new[] { "发送成功", "Sent successfully", "Enviado correctamente" } } }, call => {
                    string error = Send(call.Get<string>("Text"));
                    if (!string.IsNullOrEmpty(error)) throw new InvalidOperationException(error);
                    return FlowCall.Result("Value", true);
                });
            yield return FlowMethods.Sync("device.receive", "读取接收数据", "Receive data", "Recibir datos", new FlowField[0],
                new[] { new FlowField("Value", typeof(string)) { DisplayName = new[] { "接收数据", "Received data", "Datos recibidos" } } }, call => FlowCall.Result("Value", Receive()));
        }

        /// <summary>只把实际重写的无参生命周期操作加入标准动作，业务重载不影响目录发现。</summary>
        private bool OverridesLifecycle(string name)
        {
            MethodInfo method = GetType().GetMethod(name, BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
            return method != null && method.DeclaringType != typeof(AbstractDevice)
                && method.GetBaseDefinition().DeclaringType == typeof(AbstractDevice);
        }
    }

    public partial class ScannerKeyenceTcp
    {
        /// <summary>声明扫码枪实例的单次触发方法，空条码按失败处理。</summary>
        public override IEnumerable<FlowMethod> GetFlowMethods(bool includeDraft = false)
        {
            foreach (FlowMethod method in base.GetFlowMethods(includeDraft)) yield return method;
            yield return FlowMethods.Sync("scanner.once", "扫码一次", "Scan once", "Escanear una vez", new FlowField[0],
                new[] { new FlowField("Barcode", typeof(string)) { DisplayName = new[] { "条码", "Barcode", "Código de barras" } } }, call => {
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
                new[] { new FlowField("Valid", typeof(bool)) { DisplayName = new[] { "结果有效", "Valid result", "Resultado válido" } },
                    new FlowField("Values", typeof(double[])) { DisplayName = new[] { "全部通道结果", "All channel results", "Resultados de todos los canales" } } }, call => {
                    bool valid = false;
                    List<double> values = GetFinalResult(ref valid);
                    if (!valid) throw new InvalidOperationException(FlowText.T("error.resistanceLength"));
                    Dictionary<string, object> result = new Dictionary<string, object> { { "Valid", true }, { "Values", values.ToArray() } };
                    for (int index = 0; index < values.Count; index++) result.Add("Value" + index, values[index]);
                    return result;
                });
            for (int index = 0; index < ChannelNum; index++) read.Outputs.Add(new FlowField("Value" + index, typeof(double)) {
                DisplayName = new[] { "通道结果 " + index, "Channel result " + index, "Resultado del canal " + index }
            });
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
                new[] { new FlowField("Position", typeof(int), "0") { DisplayName = new[] { "3D 位置号", "3D position number", "Número de posición 3D" } } },
                new[] { new FlowField("Value", typeof(string)) { DisplayName = new[] { "3D 返回结果", "3D returned result", "Resultado devuelto por 3D" } } },
                call => FlowCall.Result("Value", Triger3D(call.Get<int>("Position"))));
            yield return FlowMethods.Sync("keyence.change", "切换 3D 程序", "Change 3D program", "Cambiar programa 3D",
                new[] { new FlowField("Position", typeof(int), "0") { DisplayName = new[] { "3D 位置号", "3D position number", "Número de posición 3D" } } },
                new[] { new FlowField("Value", typeof(string)) { DisplayName = new[] { "切换返回结果", "Program change response", "Respuesta de cambio de programa" } } },
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
                new[] { new FlowField("Value", typeof(string)) { DisplayName = new[] { "气密检测结果", "Leak test result", "Resultado de prueba de fugas" } } }, call => FlowCall.Result("Value", Readdata()));
        }
    }

    public partial class ModBus_RTU
    {
        /// <summary>声明寄存器读取方法，参数初值来自这个串口对象的实际配置。</summary>
        public override IEnumerable<FlowMethod> GetFlowMethods(bool includeDraft = false)
        {
            foreach (FlowMethod method in base.GetFlowMethods(includeDraft)) yield return method;
            yield return FlowMethods.Sync("modbus.read", "读取 Modbus 寄存器", "Read Modbus registers", "Leer registros Modbus",
                new[] { new FlowField("Slave", typeof(byte), Slaveld.ToString()) { DisplayName = new[] { "从站地址", "Slave address", "Dirección de esclavo" } },
                    new FlowField("Start", typeof(ushort), StartRegister.ToString()) { DisplayName = new[] { "起始寄存器", "Start register", "Registro inicial" } },
                    new FlowField("Count", typeof(ushort), Count.ToString()) { DisplayName = new[] { "寄存器数量", "Register count", "Cantidad de registros" } },
                    new FlowField("DelayMs", typeof(int), ThreadSleep.ToString()) { DisplayName = new[] { "接收前等待时间", "Wait before receiving", "Espera antes de recibir" }, Unit = "ms", Advanced = true,
                        Help = new[] { "发送读取命令后等待设备返回数据的时间，沿用此串口对象配置。", "Wait after sending the read command before receiving data, using this serial device's configuration.", "Espera tras enviar el comando de lectura antes de recibir datos, según la configuración de este dispositivo serie." } } },
                new[] { new FlowField("Bytes", typeof(byte[])) { DisplayName = new[] { "原始字节", "Raw bytes", "Bytes sin procesar" } },
                    new FlowField("Hex", typeof(string)) { DisplayName = new[] { "十六进制结果", "Hexadecimal result", "Resultado hexadecimal" } } }, call => {
                    byte[] bytes = ReadOutputRegisters(call.Get<byte>("Slave"), call.Get<ushort>("Start"), call.Get<ushort>("Count"), call.Get<int>("DelayMs"));
                    if (bytes == null) throw new InvalidOperationException(FlowText.T("error.modbusEmpty"));
                    return new Dictionary<string, object> { { "Bytes", bytes }, { "Hex", BitConverter.ToString(bytes) } };
                });
        }
    }
}
