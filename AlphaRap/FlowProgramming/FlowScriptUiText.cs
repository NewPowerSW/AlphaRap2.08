using System.Collections.Generic;

namespace AlphaRap.FlowProgramming
{
    /// <summary>脚本编辑器固定三语文案，源码和端口身份保持不变。</summary>
    internal static class FlowScriptUiText
    {
        internal static readonly Dictionary<string, string[]> Defaults = new Dictionary<string, string[]>
        {
            { "scriptMode", new[] { "执行方式", "Execution", "Ejecución" } },
            { "scriptObjectLabel", new[] { "对象", "Object", "Objeto" } },
            { "scriptMethodLabel", new[] { "方法", "Method", "Método" } },
            { "scriptAction", new[] { "现有功能", "Existing function", "Función existente" } },
            { "scriptCustom", new[] { "C# 脚本", "C# script", "Script C#" } },
            { "scriptNone", new[] { "仅判断条件", "Conditions only", "Solo condiciones" } },
            { "scriptBreakpoint", new[] { "执行前断点", "Break before execution", "Pausa antes de ejecutar" } },
            { "scriptTab", new[] { "C# 脚本", "C# script", "Script C#" } },
            { "scriptParameters", new[] { "输入输出", "Inputs and outputs", "Entradas y salidas" } },
            { "scriptRoutes", new[] { "下一步", "Next step", "Siguiente paso" } },
            { "scriptCodeHelp", new[] { "Inputs.名称 读取输入；Outputs.名称 返回输出。", "Read inputs with Inputs.Name; return outputs with Outputs.Name.", "Lea entradas con Inputs.Nombre y devuelva salidas con Outputs.Nombre." } },
            { "scriptSettingsHelp", new[] { "先定义输入输出；需要调用实际对象时再选择对象和方法。右键删除端口或对象绑定。", "Define inputs and outputs. Select an object and method only when needed. Right-click to remove ports or bindings.", "Defina entradas y salidas. Seleccione un objeto y método solo si hace falta. Clic derecho para eliminar puertos o vínculos." } },
            { "scriptInputs", new[] { "输入端口", "Input ports", "Puertos de entrada" } },
            { "scriptOutputs", new[] { "输出端口", "Output ports", "Puertos de salida" } },
            { "scriptName", new[] { "名称（C# 标识符）", "Name (C# identifier)", "Nombre (identificador C#)" } },
            { "scriptType", new[] { "类型", "Type", "Tipo" } },
            { "scriptDefault", new[] { "默认值", "Default", "Valor inicial" } },
            { "scriptAddInput", new[] { "+ 输入", "+ Input", "+ Entrada" } },
            { "scriptAddOutput", new[] { "+ 输出", "+ Output", "+ Salida" } },
            { "scriptDeletePort", new[] { "删除端口", "Delete port", "Eliminar puerto" } },
            { "scriptCompile", new[] { "编译检查", "Check compilation", "Comprobar compilación" } },
            { "scriptCompiled", new[] { "编译通过；尚未保存。", "Compilation passed; not saved yet.", "Compilación correcta; aún no guardado." } },
            { "scriptSourcePath", new[] { "调试源码：{0}", "Debug source: {0}", "Código de depuración: {0}" } },
            { "scriptCode", new[] { "源码", "Source", "Código fuente" } },
            { "scriptWordWrap", new[] { "自动换行", "Word wrap", "Ajuste de línea" } },
            { "scriptHelp", new[] { "用 Inputs.名称 读取绑定值，用 Outputs.名称 输出结果。流程跳转仍在 NEXT / CASE 设置。", "Read bindings with Inputs.Name and return results with Outputs.Name. Configure routes under NEXT / CASE.", "Lea vínculos con Inputs.Nombre y devuelva resultados con Outputs.Nombre. Configure rutas en NEXT / CASE." } },
            { "scriptDebugHelp", new[] { "节点断点可检查输入输出；C# 行级调试可使用 Visual Studio 附加到 AlphaRap。保存统一使用主界面 Save。", "Node breakpoints inspect inputs and outputs. Attach Visual Studio to AlphaRap for C# line debugging. Save with Main Save.", "Las pausas de nodo muestran entradas y salidas. Adjunte Visual Studio a AlphaRap para depurar líneas C#. Guarde con Save principal." } },
            { "scriptBoolean", new[] { "布尔", "Boolean", "Booleano" } },
            { "scriptInteger", new[] { "整数", "Integer", "Entero" } },
            { "scriptNumber", new[] { "数值", "Number", "Número" } },
            { "scriptText", new[] { "文本", "Text", "Texto" } },
            { "scriptInsert", new[] { "插入对象方法", "Insert object call", "Insertar llamada a objeto" } },
            { "scriptBoundObjects", new[] { "已绑定对象", "Bound objects", "Objetos vinculados" } },
            { "scriptRemoveObject", new[] { "移除此对象绑定", "Remove object binding", "Eliminar vínculo al objeto" } },
            { "scriptRemovedObject", new[] { "已移除对象绑定；源码保持不变，请移除或替换其中的调用。", "Object binding removed. Source is retained; remove or replace its calls.", "Se eliminó el vínculo. Se conserva el código; elimine o sustituya sus llamadas." } },
            { "scriptChooseObject", new[] { "选择实际对象", "Select actual object", "Seleccione objeto real" } },
            { "scriptChooseMethod", new[] { "选择方法", "Select method", "Seleccione método" } },
            { "scriptInsertHelp", new[] { "已按对象、动作和参数分行插入；参数使用对象默认值，可替换为 Inputs.名称。", "Inserted with separate object, method, and argument lines. Defaults can be replaced with Inputs.Name.", "Se insertaron líneas separadas para objeto, método y argumentos. Puede sustituir valores iniciales por Inputs.Nombre." } }
        };

        /// <summary>沿用框架语言入口读取固定文案。</summary>
        internal static string T(string key)
        {
            string[] words = Defaults[key];
            return UiTheme.IsDesignTime ? words[0] : MiddleLayer.LangMsg("EquipmentFlow", key, words[0], words[1], words[2]);
        }
    }
}
