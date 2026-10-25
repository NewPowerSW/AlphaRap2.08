using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    internal static class FlowCodeUiText
    {
        internal static readonly Dictionary<string, string[]> Defaults = new Dictionary<string, string[]>
        {
            { "toolbox", new[] { "功能工具箱", "Function toolbox", "Caja de funciones" } },
            { "search", new[] { "搜索", "Search", "Buscar" } },
            { "toolboxHint", new[] { "拖到画布；也可双击或按 Enter 添加。", "Drag onto the canvas, double-click, or press Enter to add.", "Arrastre al lienzo, haga doble clic o pulse Intro para agregar." } },
            { "toolboxEmpty", new[] { "没有匹配的已注册功能。", "No registered functions match.", "No hay funciones registradas coincidentes." } },
            { "codeMode", new[] { "代码模块", "Code module", "Módulo de código" } },
            { "codeChoice", new[] { "选择代码模块", "Choose a code module", "Elegir módulo de código" } },
            { "resourceRole", new[] { "模块资源 / 业务字段", "Module resource / business field", "Recurso del módulo / campo" } },
            { "resourceBinding", new[] { "实际对象 / VPP 字段", "Actual object / VPP field", "Objeto real / campo VPP" } },
            { "bindingStatus", new[] { "说明", "Details", "Detalles" } },
            { "choose", new[] { "请选择", "Select", "Seleccione" } },
            { "missing", new[] { "已失效：{0}", "Unavailable: {0}", "No disponible: {0}" } },
            { "required", new[] { "必选", "Required", "Obligatorio" } },
            { "optional", new[] { "可不绑定", "Optional", "Opcional" } },
            { "noResource", new[] { "此模块不需要绑定设备对象。", "This module needs no device bindings.", "Este módulo no requiere vínculos de dispositivos." } },
            { "missingDefinition", new[] { "代码模块不存在，请重新选择；原绑定已保留。", "The code module is unavailable. Select one to repair it; bindings were retained.", "El módulo no está disponible. Seleccione uno para repararlo; se conservaron los vínculos." } },
            { "clearBinding", new[] { "清除此绑定", "Clear this binding", "Borrar este vínculo" } },
            { "oldBinding", new[] { "旧绑定已保留，请确认后修正或右键清除。", "Previous binding retained. Repair it or right-click to clear.", "Se conservó el vínculo anterior. Repárelo o bórrelo con el menú contextual." } },
            { "fieldInput", new[] { "输入", "Input", "Entrada" } },
            { "fieldOutput", new[] { "输出", "Output", "Salida" } },
            { "codeRoutesHelp", new[] { "模块代码返回 NEXT / CASE；这里只选择去向，不再重复编辑判定条件。", "The module returns NEXT / CASE. Choose destinations here; conditions belong to the code.", "El módulo devuelve NEXT / CASE. Elija destinos; las condiciones pertenecen al código." } },
            { "unconfigured", new[] { "尚未配置去向", "Destination not configured", "Destino sin configurar" } },
            { "sourceDirectory", new[] { "查看生成源码", "View generated sources", "Ver código generado" } },
            { "sourceDirectoryMissing", new[] { "源码目录尚未生成：{0}", "The source directory has not been generated: {0}", "Aún no se ha generado el directorio de código: {0}" } }
        };

        /// <summary>读取工具箱与模块绑定的三语固定文案。</summary>
        internal static string T(string key)
        {
            string[] words = Defaults[key];
            return UiTheme.IsDesignTime ? words[0] : MiddleLayer.LangMsg("EquipmentFlow", "codeUi_" + key, words[0], words[1], words[2]);
        }

        /// <summary>固定控件使用实际名称登记，嵌入页面后沿用该页面的语言作用域。</summary>
        internal static string Fixed(Control control, string key)
        {
            string[] words = Defaults[key];
            Form owner = control.FindForm();
            string scope = owner == null || owner is FlowLanguageForm ? "EquipmentFlow" : owner.Name;
            return UiTheme.IsDesignTime ? words[0] : MiddleLayer.LangMsg(scope, control.Name, words[0], words[1], words[2]);
        }

        /// <summary>按稳定元数据身份翻译标题，业务名称不写入语言表。</summary>
        internal static string Title(string key, string[] words, string fallback)
        {
            if (words == null || words.Length != 3) return fallback;
            if (words[0] == words[1] && words[1] == words[2]) return words[0];
            return UiTheme.IsDesignTime ? words[0] : MiddleLayer.LangMsg("EquipmentFlow", XmlConvert.EncodeLocalName("code_" + key), words[0], words[1], words[2]);
        }

        /// <summary>显示代码模块的三语标题，缺失定义继续显示稳定身份。</summary>
        internal static string DefinitionTitle(FlowCodeDefinition definition)
        {
            return Title(definition.Id, definition.Title, definition.Id);
        }

        /// <summary>复用已有功能分组翻译，自定义分组保持作者名称。</summary>
        internal static string Category(string category)
        {
            if (category == "code") return T("codeMode");
            return FlowText.Defaults.ContainsKey(category ?? "") ? FlowText.T(category) : category ?? T("codeMode");
        }
    }
}
