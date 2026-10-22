using System.Collections.Generic;

namespace NPSDK.Design
{
    public static class FlowDesignerTexts
    {
        public static readonly Dictionary<string, string[]> Defaults = new Dictionary<string, string[]>
        {
            {
                "FlowDesignerForm",
                new[]
                {
                    "NPSDK Flow_Chart 设计器",
                    "NPSDK Flow_Chart Designer",
                    "Diseñador NPSDK Flow_Chart"
                }
            },
            {
                "FlowNodeDialog",
                new[]
                {
                    "Flow_Chart 节点属性",
                    "Flow_Chart Properties",
                    "Propiedades de Flow_Chart"
                }
            },
            {
                "FlowDemoInstanceDialog",
                new[]
                {
                    "示例实例参数（DEMO）",
                    "Demo instance parameters",
                    "Parámetros de instancias de ejemplo"
                }
            },
            {
                "DemoTimeout",
                new[]
                {
                    "示例流程判定条件等待超时。",
                    "Demo flow condition timed out.",
                    "La condición del flujo de ejemplo agotó el tiempo de espera."
                }
            },
            {
                "demoHint",
                new[]
                {
                    "这些参数只作用于 DEMO 实例。可将 VisionX 改为 5，测试条件不满足及超时报警。",
                    "These parameters affect DEMO instances only. Set VisionX to 5 to test a false condition and timeout alarm.",
                    "Estos parámetros solo afectan a instancias DEMO. Establezca VisionX en 5 para probar una condición falsa y su alarma."
                }
            },
            {
                "exampleButton",
                new[]
                {
                    "示例",
                    "Example",
                    "Ejemplo"
                }
            },
            {
                "openButton",
                new[]
                {
                    "打开",
                    "Open",
                    "Abrir"
                }
            },
            {
                "saveButton",
                new[]
                {
                    "保存",
                    "Save",
                    "Guardar"
                }
            },
            {
                "validateButton",
                new[]
                {
                    "检查",
                    "Validate",
                    "Validar"
                }
            },
            {
                "runButton",
                new[]
                {
                    "运行",
                    "Run",
                    "Ejecutar"
                }
            },
            {
                "pauseButton",
                new[]
                {
                    "暂停 / 继续",
                    "Pause / Resume",
                    "Pausar / Reanudar"
                }
            },
            {
                "stopButton",
                new[]
                {
                    "停止",
                    "Stop",
                    "Detener"
                }
            },
            {
                "editButton",
                new[]
                {
                    "节点属性",
                    "Properties",
                    "Propiedades"
                }
            },
            {
                "deleteButton",
                new[]
                {
                    "删除节点",
                    "Delete node",
                    "Eliminar nodo"
                }
            },
            {
                "instancesButton",
                new[]
                {
                    "示例实例参数",
                    "Demo instances",
                    "Instancias de ejemplo"
                }
            },
            {
                "folderButton",
                new[]
                {
                    "记录目录",
                    "Records folder",
                    "Carpeta de registros"
                }
            },
            {
                "entryLabel",
                new[]
                {
                    "入口",
                    "Entry",
                    "Entrada"
                }
            },
            {
                "canvasHint",
                new[]
                {
                    "拖出功能创建节点；拖动节点调整位置；双击配置动作、条件及跳转。",
                    "Drag a function onto the canvas. Move nodes and double-click to configure actions, conditions and links.",
                    "Arrastre una función al lienzo. Mueva los nodos y haga doble clic para configurar acciones, condiciones y enlaces."
                }
            },
            {
                "tracePage",
                new[]
                {
                    "运行轨迹",
                    "Execution trace",
                    "Traza de ejecución"
                }
            },
            {
                "resultsPage",
                new[]
                {
                    "本次输出",
                    "Run outputs",
                    "Resultados de ejecución"
                }
            },
            {
                "outputLabel",
                new[]
                {
                    "本地记录目录",
                    "Local records",
                    "Registros locales"
                }
            },
            {
                "parameterHint",
                new[]
                {
                    "Source 为空时使用常量；选择 Node.Field 可传递前驱输出。范围写 min,max，时间单位毫秒，超时 0 表示无限等待。",
                    "Empty Source uses a literal; Node.Field binds an earlier output. Range: min,max; time: ms; timeout 0 waits indefinitely.",
                    "Source vacío usa un literal; Node.Field vincula un resultado anterior. Rango: min,max; tiempo: ms; espera 0 sin límite."
                }
            },
            {
                "okButton",
                new[]
                {
                    "确定",
                    "OK",
                    "Aceptar"
                }
            },
            {
                "cancelButton",
                new[]
                {
                    "取消",
                    "Cancel",
                    "Cancelar"
                }
            },
            {
                "addParameter",
                new[]
                {
                    "增加保存字段",
                    "Add saved field",
                    "Añadir campo"
                }
            },
            {
                "deleteParameter",
                new[]
                {
                    "删除保存字段",
                    "Delete saved field",
                    "Eliminar campo"
                }
            },
            {
                "Literal",
                new[]
                {
                    "常量",
                    "Literal",
                    "Literal"
                }
            },
            {
                "Busy",
                new[]
                {
                    "流程仍在运行。",
                    "The flow is still running.",
                    "El flujo sigue en ejecución."
                }
            },
            {
                "AxisNotReady",
                new[]
                {
                    "轴未就绪、存在限位/报警或正在运动。",
                    "Axis is not ready, at a limit, in alarm or moving.",
                    "El eje no está listo, está en un límite, en alarma o en movimiento."
                }
            },
            {
                "HardwareNotReady",
                new[]
                {
                    "所选硬件尚未成功初始化。",
                    "The selected hardware has not initialized successfully.",
                    "El hardware seleccionado no se ha inicializado correctamente."
                }
            },
            {
                "ScannerNotReady",
                new[]
                {
                    "扫码枪未连接、忙碌或读取失败，请检查设备和读取超时。",
                    "Scanner disconnected, busy or read failed; check the device and read timeout.",
                    "Lector desconectado, ocupado o lectura fallida; revise el dispositivo y su tiempo límite."
                }
            },
            {
                "VisionResultChanged",
                new[]
                {
                    "视觉结果已被其他请求替换。",
                    "Vision result was replaced by another request.",
                    "Otra solicitud reemplazó el resultado de visión."
                }
            },
            {
                "Validated",
                new[]
                {
                    "配置检查通过。",
                    "Configuration validated.",
                    "Configuración validada."
                }
            },
            {
                "SavedDesign",
                new[]
                {
                    "设计已保存：{0}。下次启动自动恢复，不自动运行。",
                    "Design saved: {0}. Restored on next start; execution remains manual.",
                    "Diseño guardado: {0}. Se restaurará al iniciar; la ejecución sigue siendo manual."
                }
            },
            {
                "RestoredDesign",
                new[]
                {
                    "已恢复设计：{0}。请检查后手动运行。",
                    "Design restored: {0}. Review before running manually.",
                    "Diseño restaurado: {0}. Revíselo antes de ejecutar manualmente."
                }
            },
            {
                "BindingNeedsReview",
                new[]
                {
                    "配置已保留，运行前需修复：{0}",
                    "Configuration preserved; fix before running: {0}",
                    "Configuración conservada; corrija antes de ejecutar: {0}"
                }
            },
            {
                "RestoredBackup",
                new[]
                {
                    "设计文件损坏，已恢复上次备份：{0}。请核对后保存。",
                    "Design file is damaged; restored backup: {0}. Review before saving.",
                    "Archivo dañado; copia restaurada: {0}. Revise antes de guardar."
                }
            },
            {
                "RestoreFailed",
                new[]
                {
                    "无法恢复设计 {0}：{1}。已显示示例，原文件未被覆盖。",
                    "Cannot restore design {0}: {1}. Showing the example; original file preserved.",
                    "No se puede restaurar {0}: {1}. Se muestra el ejemplo; archivo original conservado."
                }
            },
            {
                "MaintenanceRequired",
                new[]
                {
                    "请使用维护或管理员账号。",
                    "A maintenance or administrator account is required.",
                    "Se requiere una cuenta de mantenimiento o administrador."
                }
            },
            {
                "MachineBusy",
                new[]
                {
                    "请先结束自动生产或初始化并清除报警。",
                    "Finish production or initialization and clear alarms first.",
                    "Finalice la producción o inicialización y borre las alarmas primero."
                }
            },
            {
                "Enum_Always",
                new[]
                {
                    "直接通过",
                    "Always",
                    "Siempre"
                }
            },
            {
                "Enum_IsTrue",
                new[]
                {
                    "为真",
                    "Is true",
                    "Es verdadero"
                }
            },
            {
                "Enum_IsFalse",
                new[]
                {
                    "为假",
                    "Is false",
                    "Es falso"
                }
            },
            {
                "Enum_Equal",
                new[]
                {
                    "等于",
                    "Equals",
                    "Igual"
                }
            },
            {
                "Enum_NotEqual",
                new[]
                {
                    "不等于",
                    "Not equal",
                    "Distinto"
                }
            },
            {
                "Enum_Greater",
                new[]
                {
                    "大于",
                    "Greater than",
                    "Mayor que"
                }
            },
            {
                "Enum_Less",
                new[]
                {
                    "小于",
                    "Less than",
                    "Menor que"
                }
            },
            {
                "Enum_InRange",
                new[]
                {
                    "范围内（含边界）",
                    "Within range (inclusive)",
                    "Dentro del rango (inclusivo)"
                }
            },
            {
                "Enum_NotEmpty",
                new[]
                {
                    "非空",
                    "Not empty",
                    "No vacío"
                }
            },
            {
                "Enum_Wait",
                new[]
                {
                    "持续等待",
                    "Wait",
                    "Esperar"
                }
            },
            {
                "Enum_Case1",
                new[]
                {
                    "跳转 Case1",
                    "Branch Case1",
                    "Saltar a Case1"
                }
            },
            {
                "Enum_Case2",
                new[]
                {
                    "跳转 Case2",
                    "Branch Case2",
                    "Saltar a Case2"
                }
            },
            {
                "Enum_Case3",
                new[]
                {
                    "跳转 Case3",
                    "Branch Case3",
                    "Saltar a Case3"
                }
            },
            {
                "Enum_End",
                new[]
                {
                    "结束",
                    "End",
                    "Terminar"
                }
            },
            {
                "InvalidDocument",
                new[]
                {
                    "配置版本、节点数量或结构无效。",
                    "Invalid document version, node count or structure.",
                    "Versión, cantidad de nodos o estructura no válida."
                }
            },
            {
                "InvalidNode",
                new[]
                {
                    "节点标识重复或无效：{0}",
                    "Duplicate or invalid node ID: {0}",
                    "ID de nodo duplicado o no válido: {0}"
                }
            },
            {
                "MissingEntry",
                new[]
                {
                    "请选择有效入口节点。",
                    "Select a valid entry node.",
                    "Seleccione un nodo de entrada válido."
                }
            },
            {
                "UnknownBinding",
                new[]
                {
                    "节点 {0} 未找到动作 {1} 的实例 {2}。",
                    "Node {0}: instance {2} of {1} is unavailable.",
                    "Nodo {0}: instancia {2} de {1} no disponible."
                }
            },
            {
                "InvalidTime",
                new[]
                {
                    "节点 {0} 的时间须在 0～600000 ms。",
                    "Node {0}: time must be 0–600000 ms.",
                    "Nodo {0}: el tiempo debe ser 0–600000 ms."
                }
            },
            {
                "UnknownNext",
                new[]
                {
                    "节点 {0} 的跳转不存在：{1}",
                    "Node {0}: missing link {1}",
                    "Nodo {0}: enlace inexistente {1}"
                }
            },
            {
                "UnknownAlarm",
                new[]
                {
                    "报警编号未登记：{0}",
                    "Unregistered alarm code: {0}",
                    "Código de alarma no registrado: {0}"
                }
            },
            {
                "InvalidCondition",
                new[]
                {
                    "节点 {0} 的判据无效。",
                    "Node {0}: invalid condition.",
                    "Nodo {0}: condición no válida."
                }
            },
            {
                "InvalidParameter",
                new[]
                {
                    "参数名称、类型或常量无效。",
                    "Invalid parameter name, type or literal.",
                    "Nombre, tipo o literal de parámetro no válido."
                }
            },
            {
                "InvalidSource",
                new[]
                {
                    "节点 {0} 的数据来源无效：{1}",
                    "Node {0}: invalid source {1}",
                    "Nodo {0}: origen no válido {1}"
                }
            },
            {
                "MissingData",
                new[]
                {
                    "本轮尚未产生数据：{0}",
                    "No value produced in this run: {0}",
                    "Sin valor producido en esta ejecución: {0}"
                }
            },
            {
                "Running",
                new[]
                {
                    "运行中",
                    "Running",
                    "En ejecución"
                }
            },
            {
                "Completed",
                new[]
                {
                    "已完成",
                    "Completed",
                    "Completado"
                }
            },
            {
                "Stopped",
                new[]
                {
                    "已停止",
                    "Stopped",
                    "Detenido"
                }
            },
            {
                "Failed",
                new[]
                {
                    "执行失败",
                    "Failed",
                    "Error"
                }
            },
            {
                "TimedOut",
                new[]
                {
                    "等待超时",
                    "Timed out",
                    "Tiempo agotado"
                }
            },
            {
                "False",
                new[]
                {
                    "条件不满足",
                    "Condition false",
                    "Condición no cumplida"
                }
            },
            {
                "Group_Flow",
                new[]
                {
                    "1. 流程设置",
                    "1. Flow",
                    "1. Flujo"
                }
            },
            {
                "Group_Identity",
                new[]
                {
                    "2. 动作与实例",
                    "2. Action and instance",
                    "2. Acción e instancia"
                }
            },
            {
                "Group_Links",
                new[]
                {
                    "3. 节点跳转",
                    "3. Node links",
                    "3. Enlaces"
                }
            },
            {
                "Group_Condition",
                new[]
                {
                    "4. 判定条件",
                    "4. Condition",
                    "4. Condición"
                }
            },
            {
                "Group_Timing",
                new[]
                {
                    "5. 时间设置",
                    "5. Timing",
                    "5. Tiempos"
                }
            },
            {
                "Group_Alarm",
                new[]
                {
                    "6. 报警绑定",
                    "6. Alarm binding",
                    "6. Alarma"
                }
            }
        };
        /// <summary>补齐表头及属性三语底稿，业务键保持不变。</summary>
        static FlowDesignerTexts()
        {
            AddField("Node", "节点", "Node", "Nodo");
            AddField("State", "状态", "State", "Estado");
            AddField("Alarm", "报警", "Alarm", "Alarma");
            AddField("Detail", "详情", "Detail", "Detalle");
            AddField("Field", "字段", "Field", "Campo");
            AddField("Value", "值", "Value", "Valor");
            AddField("Name", "名称", "Name", "Nombre");
            AddField("Type", "类型", "Type", "Tipo");
            AddField("Source", "数据来源", "Source", "Origen");
            AddProperty("CanPause", "允许暂停", "Can pause", "Permitir pausa", "仅控制 SDK 节点暂停点，不能暂停已发出的物理动作。", "Controls SDK pause points; does not pause physical motion already issued.", "Controla puntos de pausa; no pausa un movimiento físico ya iniciado.");
            AddProperty("MethodName", "功能方法", "Method", "Método", "选择已登记的功能，不输入任意函数。", "Select a registered action.", "Seleccione una acción registrada.");
            AddProperty("Name", "节点标识", "Node ID", "ID del nodo", "稳定数据引用标识。", "Stable ID for data references.", "ID estable para referencias de datos.");
            AddProperty("Text", "节点文字", "Text", "Texto", "应用者自定义显示名称，语言切换不覆盖。", "User caption preserved during language changes.", "Texto del usuario conservado al cambiar idioma.");
            AddProperty("Instance", "绑定实例", "Instance", "Instancia", "选择现有对象，不自动创建或连接设备。", "Select an existing object; devices are not created or connected automatically.", "Seleccione un objeto existente; no se crean ni conectan dispositivos automáticamente.");
            foreach (string link in new[]
            {
                "Next",
                "Case1",
                "Case2",
                "Case3"
            }

            )
                AddProperty(link, link + " 跳转", link, link, "Next 为条件满足分支，Case 为条件不满足时选择的分支。空 Next 表示结束。", "Next is the success link; Case is selected on a false condition. Empty Next ends the run.", "Next es el enlace correcto; Case se elige con condición falsa. Next vacío termina.");
            AddProperty("Source", "判定数据", "Condition source", "Origen de condición", "当前输出字段或前驱 Node.Field。", "Current output field or earlier Node.Field.", "Campo actual o Node.Field anterior.");
            AddProperty("Comparison", "比较方式", "Comparison", "Comparación", "InRange 使用包含边界的 min,max。", "InRange uses inclusive min,max.", "InRange usa min,max inclusivo.");
            AddProperty("Expected", "比较值", "Expected", "Valor esperado", "数值小数点固定为点，范围示例 -1,1。", "Invariant decimal point; example range -1,1.", "Punto decimal invariable; ejemplo de rango -1,1.");
            AddProperty("WhenFalse", "不满足时", "When false", "Si es falsa", "Wait 持续采样；Case1/2/3 立即跳转；End 结束。", "Wait polls; Case1/2/3 branches immediately; End finishes.", "Wait muestrea; Case1/2/3 salta inmediatamente; End termina.");
            AddProperty("Delay", "执行前延时 ms", "Delay ms", "Demora ms", "延时期间不重复触发动作。", "The action starts once after the delay.", "La acción se inicia una vez después de la demora.");
            AddProperty("Timeout", "超时 ms", "Timeout ms", "Tiempo límite ms", "0 无限等待；超时包括延时和暂停，触发绑定报警并停止本流程。", "0 waits indefinitely. Includes delay and pause; timeout raises the bound alarm and stops this flow.", "0 espera sin límite. Incluye demora y pausa; al vencer activa la alarma y detiene el flujo.");
            AddProperty("AlarmCode", "报警编号", "Alarm code", "Código de alarma", "从 NPSDK 报警表选择；为空时超时仍停止但不弹报警。", "Select a code from the NPSDK alarm table. Empty stops without raising an alarm.", "Seleccione un código de la tabla NPSDK. Vacío detiene sin alarma.");
            AddProperty("AlarmContent", "报警内容", "Alarm content", "Contenido de alarma", "为空使用报警表当前语言；填写则使用自定义内容。", "Empty uses the alarm table language; otherwise uses this custom content.", "Vacío usa el idioma de la tabla; en otro caso usa este contenido.");
        }

        /// <summary>登记固定表头底稿。</summary>
        private static void AddField(string key, string zh, string en, string es)
        {
            foreach (string grid in new[]
            {
                "traceGrid",
                "resultGrid",
                "parameterGrid"
            }

            )
                Defaults[grid + "_" + key] = new[]
                {
                    zh,
                    en,
                    es
                };
        }

        /// <summary>登记属性标题和帮助，不改变配置字段。</summary>
        private static void AddProperty(string key, string zh, string en, string es, string helpZh, string helpEn, string helpEs)
        {
            Defaults["Property_" + key] = new[]
            {
                zh,
                en,
                es
            };
            Defaults["Help_" + key] = new[]
            {
                helpZh,
                helpEn,
                helpEs
            };
        }
    }
}
