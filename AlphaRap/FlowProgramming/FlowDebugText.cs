using System.Collections.Generic;

namespace AlphaRap.FlowProgramming
{
    internal static class FlowDebugText
    {
        internal static readonly Dictionary<string, string[]> Defaults = new Dictionary<string, string[]>
        {
            { "debugMode", new[] { "节点调试", "Node debugging", "Depurar nodos" } },
            { "nodeBreakpoint", new[] { "设置 / 取消断点", "Toggle breakpoint", "Alternar punto de interrupción" } },
            { "debugContinue", new[] { "继续", "Continue", "Continuar" } },
            { "debugStep", new[] { "单步执行节点", "Step one node", "Ejecutar un nodo" } },
            { "debugDirection", new[] { "方向", "Direction", "Dirección" } },
            { "debugField", new[] { "字段", "Field", "Campo" } },
            { "debugValue", new[] { "本次值", "Current value", "Valor actual" } },
            { "debugInput", new[] { "输入", "Input", "Entrada" } },
            { "debugOutput", new[] { "输出", "Output", "Salida" } },
            { "debugExit", new[] { "所选出口", "Selected exit", "Salida seleccionada" } },
            { "debugReady", new[] { "节点调试已开启，请使用主界面初始化 / 启动；先停在入口节点。", "Node debugging is enabled. Use the main Initialize / Start buttons; execution first pauses at the entry node.", "Depuración activa. Use Inicializar / Iniciar principales; la ejecución se pausa primero en la entrada." } },
            { "debugNodeState", new[] { "{0} / {1}：{2}", "{0} / {1}: {2}", "{0} / {1}: {2}" } },
            { "debugPreparing", new[] { "准备执行", "Preparing", "Preparando" } },
            { "debugRunning", new[] { "动作执行中", "Action running", "Ejecutando acción" } },
            { "DebugPaused", new[] { "断点暂停", "Paused at node", "Pausado en nodo" } },
            { "debugActionCompleted", new[] { "动作已完成，正在判定出口", "Action completed; evaluating exits", "Acción completada; evaluando salidas" } },
            { "debugCompleted", new[] { "节点完成", "Node completed", "Nodo completado" } },
            { "debugFinished", new[] { "调试已结束", "Debug session finished", "Depuración finalizada" } },
            { "debugUnavailable", new[] { "尚不可读取：{0}", "Not available yet: {0}", "Aún no disponible: {0}" } },
            { "debugHelp", new[] { "继续到下一个断点；单步执行一个节点。节点前暂停不计超时；停止沿用原停止按钮。", "Continue to the next breakpoint or execute one node. Pausing before a node does not consume its timeout. Use the existing Stop button.", "Continúe al siguiente punto o ejecute un nodo. La pausa previa no consume su tiempo límite. Use Detener existente." } },
            { "scriptObjectMissing", new[] { "脚本绑定的对象已不存在：", "A script-bound object is missing: ", "Falta un objeto vinculado al script: " } },
            { "scriptObjectNotBound", new[] { "请先在脚本编辑器中绑定该对象：", "Bind this object in the script editor first: ", "Vincule primero este objeto en el editor de scripts: " } }
        };
    }
}
