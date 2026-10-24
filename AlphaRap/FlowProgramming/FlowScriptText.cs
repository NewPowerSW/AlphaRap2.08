using System;
using System.Collections.Generic;
using System.Globalization;

namespace AlphaRap.FlowProgramming
{
    /// <summary>脚本服务的固定三语文案，源码、端口名和编译器诊断保持原值。</summary>
    internal static class FlowScriptText
    {
        internal static readonly Dictionary<string, string[]> Defaults = new Dictionary<string, string[]>
        {
            { "scriptInvalidMode", new[] { "此节点不是脚本或空节点。", "This node is neither a script nor an empty node.", "Este nodo no es de script ni está vacío." } },
            { "scriptEmptySource", new[] { "脚本函数体不能为空；不执行动作时请选择空节点。", "The script body cannot be empty. Select an empty node when no action is needed.", "El cuerpo del script no puede estar vacío. Seleccione un nodo vacío si no necesita ninguna acción." } },
            { "scriptInvalidPort", new[] { "脚本端口「{0}」名称无效。请使用合法 C# 标识符，不使用关键字、@ 或 __ 前缀。", "Script port '{0}' is invalid. Use a valid C# identifier without keywords, @, or the __ prefix.", "El puerto «{0}» no es válido. Use un identificador C# sin palabras reservadas, @ ni el prefijo __." } },
            { "scriptDuplicatePort", new[] { "脚本的同一输入或输出列表中端口「{0}」重复。", "Script port '{0}' is duplicated in the same input or output list.", "El puerto «{0}» está duplicado en la misma lista de entradas o salidas." } },
            { "scriptInvalidType", new[] { "脚本端口「{0}」的类型「{1}」无效；支持 Boolean、Integer、Number、Text。", "Script port '{0}' has unsupported type '{1}'. Use Boolean, Integer, Number, or Text.", "El puerto «{0}» tiene el tipo no admitido «{1}». Use Boolean, Integer, Number o Text." } },
            { "scriptInvalidDefault", new[] { "脚本端口「{0}」的默认值不符合声明类型。", "The default value of script port '{0}' does not match its declared type.", "El valor predeterminado del puerto «{0}» no coincide con su tipo declarado." } },
            { "scriptInvalidResource", new[] { "脚本允许调用的对象列表含空身份或重复身份。", "The script's allowed-object list contains an empty or duplicate identity.", "La lista de objetos permitidos del script contiene una identidad vacía o duplicada." } },
            { "scriptMissingInput", new[] { "脚本缺少输入「{0}」，请配置固定值或数据绑定。", "Script input '{0}' is missing. Configure a fixed value or data binding.", "Falta la entrada «{0}» del script. Configure un valor fijo o un vínculo de datos." } },
            { "scriptInputType", new[] { "脚本输入「{0}」不符合声明类型。", "Script input '{0}' does not match its declared type.", "La entrada «{0}» del script no coincide con su tipo declarado." } },
            { "scriptMissingOutput", new[] { "脚本输出「{0}」尚未赋值；不能以默认值继续执行。", "Script output '{0}' has not been assigned; execution cannot continue with a default value.", "La salida «{0}» del script no tiene un valor asignado; no se puede continuar con un valor predeterminado." } },
            { "scriptOutputType", new[] { "脚本输出「{0}」不符合声明类型。", "Script output '{0}' does not match its declared type.", "La salida «{0}» del script no coincide con su tipo declarado." } },
            { "scriptCompileFailed", new[] { "脚本编译失败（C# 5）：\r\n{0}\r\n脚本源码：{1}", "Script compilation failed (C# 5):\r\n{0}\r\nScript source: {1}", "Error de compilación del script (C# 5):\r\n{0}\r\nCódigo fuente: {1}" } },
            { "scriptRuntimeFailed", new[] { "脚本执行失败：{0}({1})\r\n{2}", "Script execution failed: {0}({1})\r\n{2}", "Error de ejecución del script: {0}({1})\r\n{2}" } },
            { "scriptRuntimeNoLine", new[] { "脚本执行失败：{0}\r\n异常未提供用户源码行号。\r\n{1}", "Script execution failed: {0}\r\nThe exception did not provide a user source line.\r\n{1}", "Error de ejecución del script: {0}\r\nLa excepción no proporcionó una línea del código del usuario.\r\n{1}" } },
            { "scriptCacheFailed", new[] { "无法生成或加载脚本缓存：{0}", "Unable to create or load the script cache: {0}", "No se puede crear o cargar la caché del script: {0}" } },
            { "scriptVersionLimit", new[] { "本次程序已加载 {0} 个脚本版本。请保存工程并重启程序后继续编译；已加载程序集不能单独卸载。", "This process has loaded {0} script versions. Save the project and restart before compiling another version; loaded assemblies cannot be unloaded individually.", "Este proceso ha cargado {0} versiones de scripts. Guarde el proyecto y reinicie antes de compilar otra versión; los ensamblados cargados no se pueden descargar individualmente." } },
            { "scriptPipelineRequired", new[] { "脚本必须通过流程运行器的受控调用入口执行。", "Scripts must execute through the flow runtime's controlled call pipeline.", "Los scripts deben ejecutarse mediante la canalización de llamadas controladas del flujo." } },
            { "scriptCallUnavailable", new[] { "脚本调用入口不可用，未执行对象方法。", "The script call pipeline is unavailable. No object method was executed.", "La canalización de llamadas no está disponible. No se ejecutó ningún método de objeto." } },
            { "scriptScopeClosed", new[] { "此脚本节点已结束或正在结束，不能再启动对象调用。", "This script node has finished or is closing. No new object call can start.", "Este nodo de script ha terminado o se está cerrando. No se puede iniciar otra llamada a un objeto." } },
            { "scriptCallArguments", new[] { "脚本调用必须提供对象身份、方法身份和输入字典。", "Script calls require an object identity, method identity, and input dictionary.", "Las llamadas de script requieren una identidad de objeto, una identidad de método y un diccionario de entradas." } },
            { "scriptResultMissing", new[] { "调用结果缺少字段「{0}」。", "The call result is missing field '{0}'.", "Al resultado de la llamada le falta el campo «{0}»." } },
            { "scriptResultType", new[] { "调用结果字段「{0}」不能转换为请求的类型。", "Call result field '{0}' cannot be converted to the requested type.", "El campo «{0}» del resultado no se puede convertir al tipo solicitado." } },
        };

        /// <summary>沿用主程序语言与现场 XML 覆盖，按占位符填入业务数据。</summary>
        internal static string T(string key, params object[] values)
        {
            string[] words = Defaults[key];
            string format = UiTheme.IsDesignTime ? words[0] : MiddleLayer.LangMsg("EquipmentFlow", key, words[0], words[1], words[2]);
            return values.Length == 0 ? format : string.Format(CultureInfo.CurrentCulture, format, values);
        }
    }
}
