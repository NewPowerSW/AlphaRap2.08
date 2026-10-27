using System.Collections.Generic;
using System.Windows.Forms;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    /// <summary>流程工具箱和生成源码入口的三语固定文案。</summary>
    internal static class FlowToolboxText
    {
        internal static readonly Dictionary<string, string[]> Defaults = new Dictionary<string, string[]>
        {
            { "toolbox", new[] { "流程工具箱", "Flow toolbox", "Caja de flujos" } },
            { "search", new[] { "搜索", "Search", "Buscar" } },
            { "toolboxHint", new[] { "拖到画布；也可双击或按 Enter 添加。", "Drag onto the canvas, double-click, or press Enter to add.", "Arrastre al lienzo, haga doble clic o pulse Intro para agregar." } },
            { "toolboxEmpty", new[] { "没有匹配的节点。", "No matching nodes.", "No hay nodos coincidentes." } },
            { "noneEntry", new[] { "条件节点", "Condition node", "Nodo de condición" } },
            { "sourceDirectory", new[] { "查看生成源码", "View generated sources", "Ver código generado" } },
            { "sourceDirectoryMissing", new[] { "源码目录尚未生成：{0}", "The source directory has not been generated: {0}", "Aún no se ha generado el directorio de código: {0}" } }
        };

        /// <summary>读取工具箱的三语固定文案。</summary>
        internal static string T(string key)
        {
            string[] words = Defaults[key];
            return UiTheme.IsDesignTime ? words[0] : MiddleLayer.LangMsg("EquipmentFlow", "toolbox_" + key, words[0], words[1], words[2]);
        }

        /// <summary>固定控件使用实际名称登记，嵌入页面后沿用该页面的语言作用域。</summary>
        internal static string Fixed(Control control, string key)
        {
            string[] words = Defaults[key];
            Form owner = control.FindForm();
            string scope = owner == null || owner is FlowLanguageForm ? "EquipmentFlow" : owner.Name;
            return UiTheme.IsDesignTime ? words[0] : MiddleLayer.LangMsg(scope, control.Name, words[0], words[1], words[2]);
        }
    }
}
