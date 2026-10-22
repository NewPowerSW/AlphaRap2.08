using System.Collections.Generic;

namespace AlphaRap.FlowProgramming
{
    public static class FlowText
    {
        public static readonly Dictionary<string, string[]> Defaults = new Dictionary<string, string[]>
        {
            { "title", new[] { "流程工程", "Flow engineering", "Ingeniería de flujo" } },
            { "edit", new[] { "编辑流程", "Edit flow", "Editar flujo" } },
            { "addModule", new[] { "添加流程模块", "Add flow module", "Añadir módulo de flujo" } },
            { "moduleName", new[] { "输入流程模块名称", "Enter a flow module name", "Introduzca un nombre de módulo" } },
            { "save", new[] { "保存工程", "Save project", "Guardar proyecto" } },
            { "reload", new[] { "重新加载", "Reload", "Recargar" } },
            { "add", new[] { "添加节点", "Add node", "Añadir nodo" } },
            { "delete", new[] { "删除节点", "Delete node", "Eliminar nodo" } },
            { "validate", new[] { "校验", "Validate", "Validar" } },
            { "test", new[] { "运行自动入口", "Run automatic entry", "Ejecutar entrada automática" } },
            { "initialize", new[] { "运行初始化入口", "Run initialization entry", "Ejecutar inicialización" } },
            { "stop", new[] { "停止", "Stop", "Detener" } },
            { "pause", new[] { "边界暂停 / 继续", "Pause at boundary / Resume", "Pausar en límite / Continuar" } },
            { "mode", new[] { "运行模式", "Execution mode", "Modo de ejecución" } },
            { "Legacy", new[] { "保留原流程", "Keep existing flow", "Conservar flujo existente" } },
            { "Program", new[] { "使用可配置流程", "Use configured flow", "Usar flujo configurado" } },
            { "Disabled", new[] { "停用此流程模块", "Disable flow module", "Desactivar módulo" } },
            { "initialEntry", new[] { "初始化入口", "Initialization entry", "Entrada de inicialización" } },
            { "autoEntry", new[] { "自动入口", "Automatic entry", "Entrada automática" } },
            { "caption", new[] { "节点名称", "Node name", "Nombre del nodo" } },
            { "category", new[] { "功能类别", "Category", "Categoría" } },
            { "target", new[] { "实际对象", "Configured object", "Objeto configurado" } },
            { "action", new[] { "执行动作", "Action", "Acción" } },
            { "timeout", new[] { "超时 (ms)", "Timeout (ms)", "Tiempo límite (ms)" } },
            { "alarm", new[] { "异常报警号", "Failure alarm code", "Código de alarma" } },
            { "next", new[] { "条件满足 →", "Condition met →", "Condición cumplida →" } },
            { "otherwise", new[] { "条件不满足 →", "Condition not met →", "Condición no cumplida →" } },
            { "failure", new[] { "异常 / 超时 →", "Failure / Timeout →", "Error / Tiempo agotado →" } },
            { "wait", new[] { "条件不满足时等待至超时", "Wait for condition until timeout", "Esperar condición hasta el límite" } },
            { "any", new[] { "任一条件满足 (OR)", "Any condition (OR)", "Cualquier condición (OR)" } },
            { "inputs", new[] { "输入与数据来源", "Inputs and sources", "Entradas y fuentes" } },
            { "conditions", new[] { "判定条件", "Conditions", "Condiciones" } },
            { "outputs", new[] { "该动作的输出字段", "Action output fields", "Campos de salida" } },
            { "name", new[] { "字段", "Field", "Campo" } },
            { "source", new[] { "来源类型", "Source type", "Tipo de fuente" } },
            { "reference", new[] { "选择值或引用", "Select value or reference", "Seleccionar valor o referencia" } },
            { "left", new[] { "左值", "Left value", "Valor izquierdo" } },
            { "operator", new[] { "比较", "Comparison", "Comparación" } },
            { "right", new[] { "右值", "Right value", "Valor derecho" } },
            { "Constant", new[] { "固定值", "Constant", "Constante" } },
            { "Parameter", new[] { "型号 / 机器参数", "Recipe / Machine parameter", "Parámetro de receta / máquina" } },
            { "Output", new[] { "其他节点输出", "Other node output", "Salida de otro nodo" } },
            { "Result", new[] { "当前动作结果", "Current action result", "Resultado de acción actual" } },
            { "Context", new[] { "当前产品 / 型号", "Current product / Model", "Producto / Modelo actual" } },
            { "ok", new[] { "确定", "OK", "Aceptar" } },
            { "cancel", new[] { "取消", "Cancel", "Cancelar" } },
            { "end", new[] { "结束", "End", "Fin" } },
            { "hint", new[] { "编辑模式下拖动节点，双击配置。原回调节点须完成配置后才能执行。资源参数先在原页面保存。", "Drag nodes in edit mode; double-click to configure. Configure legacy nodes before execution. Save resource settings on their original pages first.", "Arrastre nodos en edición; haga doble clic para configurar. Configure los nodos heredados antes de ejecutar. Guarde primero los recursos en sus páginas originales." } },
            { "busy", new[] { "流程正在运行或停止，请等待完成。", "The flow is running or stopping. Wait for completion.", "El flujo se está ejecutando o deteniendo. Espere a que termine." } },
            { "error", new[] { "操作失败：{0}", "Operation failed: {0}", "Error en la operación: {0}" } },
            { "saved", new[] { "工程已保存：{0}", "Project saved: {0}", "Proyecto guardado: {0}" } },
            { "valid", new[] { "校验通过。", "Validation passed.", "Validación correcta." } },
            { "discard", new[] { "重新加载将放弃未保存的流程修改，是否继续？", "Reload discards unsaved flow edits. Continue?", "Recargar descarta los cambios sin guardar. ¿Continuar?" } },
            { "remove", new[] { "删除选中节点并清除其连接，是否继续？", "Delete the selected node and its links?", "¿Eliminar el nodo seleccionado y sus conexiones?" } },
            { "schema", new[] { "生产表字段", "Production fields", "Campos de producción" } },
            { "fieldType", new[] { "数据类型", "Data type", "Tipo de dato" } },
            { "simulation", new[] { "模拟执行（不访问硬件）", "Simulate (no hardware access)", "Simular (sin acceso al hardware)" } },
            { "flow", new[] { "流程与数据", "Flow and data", "Flujo y datos" } },
            { "io", new[] { "输入输出", "Input / Output", "Entrada / Salida" } },
            { "motion", new[] { "轴与点位", "Axes and points", "Ejes y posiciones" } },
            { "vision", new[] { "视觉", "Vision", "Visión" } },
            { "device", new[] { "设备", "Devices", "Dispositivos" } },
            { "data", new[] { "本地数据保存", "Local data storage", "Almacenamiento local" } },
            { "input", new[] { "操作员输入", "Operator input", "Entrada del operador" } },
            { "Running", new[] { "执行中", "Running", "Ejecutando" } },
            { "Waiting", new[] { "等待条件", "Waiting", "Esperando" } },
            { "Completed", new[] { "完成", "Completed", "Completado" } },
            { "Rejected", new[] { "条件不满足", "Condition not met", "Condición no cumplida" } },
            { "Failed", new[] { "故障", "Failed", "Error" } },
            { "Timeout", new[] { "超时", "Timed out", "Tiempo agotado" } },
            { "Cancelled", new[] { "已取消", "Cancelled", "Cancelado" } },
        };

        /// <summary>读取三语文案，设计期不触发宿主静态初始化。</summary>
        public static string T(string key)
        {
            string[] words;
            if (!Defaults.TryGetValue(key, out words)) return key;
            if (UiTheme.IsDesignTime) return words[0];
            return MiddleLayer.LangMsg("EquipmentFlow", key, words[0], words[1], words[2]);
        }
    }
}
