using System.Collections.Generic;

namespace AlphaRap
{
    internal static class DeviceText
    {
        private static readonly Dictionary<string, string[]> Words = new Dictionary<string, string[]>
        {
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
