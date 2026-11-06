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
}
