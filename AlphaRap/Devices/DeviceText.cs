using System.Collections.Generic;

namespace AlphaRap
{
    internal static class DeviceText
    {
        private static readonly Dictionary<string, string[]> Words = new Dictionary<string, string[]>
        {
            { "error.keyenceAck", new[] { "请先按实际控制器的成功应答配置 ProgramAck；不猜测切换是否成功，也不继续触发测量。", "Configure ProgramAck from the actual controller's successful reply. Program switching is not assumed and measurement will not be triggered.", "Configure ProgramAck según la respuesta real del controlador. No se supone el cambio ni se activa la medición." } },
            { "error.communication", new[] { "设备通信失败：{0}。{1}", "Device communication failed: {0}. {1}", "Falló la comunicación del dispositivo: {0}. {1}" } },
            { "communication.NotConnected", new[] { "未连接", "Not connected", "Sin conexión" } },
            { "communication.InvalidConfiguration", new[] { "通信参数无效", "Invalid communication settings", "Configuración de comunicación no válida" } },
            { "communication.Timeout", new[] { "操作超时", "Operation timed out", "Tiempo de operación agotado" } },
            { "communication.Cancelled", new[] { "已停止或旧请求已撤销", "Stopped or stale request revoked", "Detenido o solicitud antigua revocada" } },
            { "communication.Disconnected", new[] { "设备在完整应答前断开", "Device disconnected before a complete response", "Dispositivo desconectado antes de completar la respuesta" } },
            { "communication.ProtocolError", new[] { "应答格式、长度或校验不正确", "Invalid response format, length or checksum", "Formato, longitud o verificación de respuesta no válido" } },
            { "communication.Rejected", new[] { "设备拒绝命令", "Device rejected the command", "El dispositivo rechazó el comando" } },
            { "communication.Unconfirmed", new[] { "命令已发送，但设备执行结果未确认", "Command sent but execution is unconfirmed", "Comando enviado, ejecución sin confirmar" } },
            { "communication.TransportError", new[] { "连接或收发失败", "Connection or I/O failed", "Falló la conexión o transferencia" } },
            { "error.scannerCommunication", new[] { "扫码枪通信失败：{0}。{1}", "Scanner communication failed: {0}. {1}", "Falló la comunicación del lector: {0}. {1}" } },
            { "error.scannerUncertain", new[] { "命令可能已经执行。请确认设备状态后显式重连，不自动重发。", "The command may have executed. Verify the device state before reconnecting explicitly; it is not retried automatically.", "El comando puede haberse ejecutado. Verifique el estado antes de reconectar explícitamente; no se reenvía automáticamente." } },
            { "scanner.status.NotConnected", new[] { "未连接，请先打开连接", "Not connected; open the connection first", "Sin conexión; abra primero la conexión" } },
            { "scanner.status.Busy", new[] { "手动命令尚未接收完成，请先接收或停止", "A manual command is awaiting a response; receive or stop first", "Un comando manual espera respuesta; reciba o detenga primero" } },
            { "scanner.status.InvalidConfiguration", new[] { "地址、端口、超时或最大报文长度无效，或端点参数已改变", "Invalid host, port, timeout or frame limit, or endpoint settings changed", "Host, puerto, tiempo o límite de trama no válido, o cambió el destino" } },
            { "scanner.status.Timeout", new[] { "连接、排队或完整应答超时", "Connection, queue or complete-response timeout", "Tiempo agotado de conexión, cola o respuesta completa" } },
            { "scanner.status.Cancelled", new[] { "操作已停止或旧请求已撤销", "Operation stopped or stale request revoked", "Operación detenida o solicitud antigua revocada" } },
            { "scanner.status.Disconnected", new[] { "设备已断开，未收到完整应答", "Device disconnected before a complete response", "El dispositivo se desconectó antes de completar la respuesta" } },
            { "scanner.status.Rejected", new[] { "设备返回空条码或 ERROR", "Device returned an empty barcode or ERROR", "El dispositivo devolvió un código vacío o ERROR" } },
            { "scanner.status.ProtocolError", new[] { "应答超过允许的最大字节数", "Response exceeded the maximum byte length", "La respuesta superó la longitud máxima de bytes" } },
            { "scanner.status.TransportError", new[] { "网络连接或收发失败", "Network connection or I/O failure", "Falló la conexión o la transferencia de red" } },
            { "error.plcCommunication", new[] { "PLC 通信失败：{0}，驱动码 {1}。{2}", "PLC communication failed: {0}, driver code {1}. {2}", "Falló la comunicación del PLC: {0}, código del controlador {1}. {2}" } },
            { "error.plcWriteUnknown", new[] { "写入结果未知。请核对设备实际状态后显式重连，禁止自动重发。", "Write outcome is unknown. Verify the actual device state before reconnecting explicitly. Do not retry the write automatically.", "El resultado de escritura es desconocido. Verifique el estado real antes de reconectar explícitamente. No repita la escritura automáticamente." } },
            { "error.plcInvalidData", new[] { "PLC 地址、长度或数据类型无效，数值超出范围，或应答数据不完整。", "Invalid PLC address, length or data type, out-of-range value, or incomplete response.", "Dirección, longitud o tipo de dato del PLC no válido, valor fuera de rango o respuesta incompleta." } },
            { "error.scannerEmpty", new[] { "扫码枪未返回条码。", "Scanner returned no barcode.", "El lector no devolvió ningún código." } },
            { "error.plcWaitAddress", new[] { "请输入汇川 PLC 的 D/R/X/Y/S/M 完整地址，例如 D1010。当前值：{0}", "Enter a full Inovance D/R/X/Y/S/M address, such as D1010. Current value: {0}", "Introduzca una dirección Inovance D/R/X/Y/S/M completa, como D1010. Valor actual: {0}" } },
            { "error.plcWaitLimit", new[] { "PLC 等待上限须大于 0 毫秒。", "The PLC wait limit must exceed zero milliseconds.", "El límite de espera del PLC debe superar cero milisegundos." } },
            { "error.plcWaitBit", new[] { "X/Y/S/M 位地址的期望值只能是 0 或 1。", "An X/Y/S/M bit address expects 0 or 1.", "Una dirección de bit X/Y/S/M espera 0 o 1." } },
            { "error.plcWaitTimeout", new[] { "等待 PLC 值超时。地址 {0}，期望 {1}，最近读到 {2}，等待上限 {3} ms。", "Timed out waiting for a PLC value. Address {0}, expected {1}, last read {2}, wait limit {3} ms.", "Tiempo agotado al esperar un valor del PLC. Dirección {0}, esperado {1}, última lectura {2}, límite {3} ms." } },
            { "error.plcWaitNoSample", new[] { "无采样", "No sample", "Sin lectura" } },
            { "error.plcWaitRead", new[] { "PLC 读取 {0} 失败，驱动返回码 {1}。", "PLC read failed at {0}, driver return code {1}.", "Falló la lectura del PLC en {0}, código del controlador {1}." } },
            { "error.plcWaitValue", new[] { "PLC 地址 {0} 返回了无法解析的值：{1}", "PLC address {0} returned an unrecognized value: {1}", "La dirección {0} del PLC devolvió un valor no reconocido: {1}" } },
            { "error.resistanceLength", new[] { "电阻结果通道数量不符。", "Resistance result length is invalid.", "El número de canales del resultado de resistencia no es válido." } },
            { "error.modbusEmpty", new[] { "Modbus 未返回结果。", "Modbus returned no result.", "Modbus no devolvió ningún resultado." } },
        };

        /// <summary>读取设备动作的三语文案，沿用已保存的流程语言键。</summary>
        internal static string T(string key)
        {
            string[] words = Words[key];
            return ApplicationHost.Text("EquipmentFlow", key, words[0], words[1], words[2]);
        }
    }
}
