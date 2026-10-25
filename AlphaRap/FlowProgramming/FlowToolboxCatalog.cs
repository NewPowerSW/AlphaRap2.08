using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    /// <summary>在明确登记的程序集内发现代码模块，目录读取不构造业务节点。</summary>
    public static class FlowToolboxCatalog
    {
        private static readonly object sync = new object();
        private static readonly HashSet<Assembly> assemblies = new HashSet<Assembly>();
        private static List<FlowCodeDefinition> definitions;
        private static readonly List<string> diagnostics = new List<string>();

        /// <summary>读取模块加载诊断；失效模块保留为缺失绑定，其余实际资源继续可用。</summary>
        public static string[] Diagnostics
        {
            get { lock (sync) { if (definitions == null) definitions = Discover(); return diagnostics.ToArray(); } }
        }

        /// <summary>取得已部署模块的独立目录列表，稳定身份不随语言改变。</summary>
        public static IEnumerable<FlowCodeDefinition> Definitions
        {
            get
            {
                lock (sync)
                {
                    if (definitions == null) definitions = Discover();
                    return definitions.ToArray();
                }
            }
        }

        /// <summary>外部应用在初始化时登记已有模块程序集，不执行模块构造函数。</summary>
        public static void RegisterAssembly(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");
            if (FlowPrograms.AnyRunning || SysPara.SystemMode != RunMode.IDLE || Flow_Module.Module_GetRunningStatus)
                throw new InvalidOperationException(FlowText.T("busy"));
            lock (sync) { assemblies.Add(assembly); definitions = null; }
        }

        /// <summary>按模块业务身份查找，缺失模块保留为空供编辑器提示。</summary>
        public static FlowCodeDefinition Find(string id)
        {
            return Definitions.SingleOrDefault(item => item.Id == id);
        }

        /// <summary>限定发现范围并检查重复身份，避免按类名或目录顺序挑选实现。</summary>
        private static List<FlowCodeDefinition> Discover()
        {
            IEnumerable<Assembly> sources = assemblies.Concat(new[] { typeof(FlowToolboxCatalog).Assembly, Assembly.GetEntryAssembly() })
                .Where(item => item != null).Distinct();
            List<FlowCodeDefinition> result = new List<FlowCodeDefinition>();
            diagnostics.Clear();
            foreach (Assembly source in sources)
            {
                foreach (Type type in ReadTypes(source))
                    DescribeModule(type, result);
            }
            foreach (IGrouping<string, FlowCodeDefinition> duplicate in result.GroupBy(item => item.Id, StringComparer.Ordinal).Where(group => group.Count() != 1).ToArray())
            {
                diagnostics.Add(FlowCodeText.T("duplicateModule") + " " + duplicate.Key);
                result.RemoveAll(item => item.Id == duplicate.Key);
            }
            return result.OrderBy(item => item.Category).ThenBy(item => item.Id).ToList();
        }

        /// <summary>部分类型缺依赖时保留可加载类型，同时记录具体依赖错误。</summary>
        private static IEnumerable<Type> ReadTypes(Assembly assembly)
        {
            try { return assembly.GetTypes(); }
            catch (ReflectionTypeLoadException error)
            {
                diagnostics.Add(assembly.GetName().Name + ": " + string.Join("; ", error.LoaderExceptions.Where(item => item != null).Select(item => item.Message).Distinct()));
                return error.Types.Where(type => type != null).ToArray();
            }
        }

        /// <summary>不构造模块读取声明；无效扩展只禁用该模块并提供诊断。</summary>
        private static void DescribeModule(Type type, List<FlowCodeDefinition> result)
        {
            try
            {
                if (type.IsAbstract || type.ContainsGenericParameters || !type.IsDefined(typeof(FlowCodeModuleAttribute), false)) return;
                result.Add(FlowCodeCatalog.Describe(type));
            }
            catch (Exception error) when (error is TypeLoadException || error is FileNotFoundException || error is FileLoadException || error is BadImageFormatException || error is InvalidOperationException || error is ArgumentException)
            { diagnostics.Add(type.FullName + ": " + error.Message); }
        }
    }

    /// <summary>代码模块集成文案沿用三语语言入口，业务资源名称保持原值。</summary>
    internal static class FlowCodeText
    {
        private static readonly Dictionary<string, string[]> words = new Dictionary<string, string[]>
        {
            { "duplicateModule", new[] { "代码模块身份重复，请检查已登记程序集。", "Duplicate code module identifier. Check registered assemblies.", "Identificador de módulo de código duplicado. Revise los ensamblados registrados." } },
            { "moduleMissing", new[] { "代码模块缺失或契约版本不匹配：{0}", "Code module is missing or its contract version differs: {0}", "Falta el módulo de código o su versión de contrato no coincide: {0}" } },
            { "bindingMissing", new[] { "模块资源「{0}」缺失或类型不匹配，请重新绑定。", "Module resource '{0}' is missing or incompatible. Bind it again.", "Falta el recurso del módulo «{0}» o es incompatible. Vuelva a vincularlo." } },
            { "fieldMissing", new[] { "模块字段「{0}」的 VPP 映射缺失或类型已改变。", "The VPP mapping for module field '{0}' is missing or its type changed.", "Falta la asignación VPP del campo «{0}» o cambió su tipo." } },
            { "pointsChanged", new[] { "轴「{0}」的点位表已改变，请打开节点确认本配方的资源绑定。", "Axis '{0}' point table changed. Open the node to confirm its bindings for this recipe.", "Cambió la tabla de puntos del eje «{0}». Abra el nodo y confirme los vínculos de esta receta." } },
            { "exitMissing", new[] { "代码模块出口「{0}」尚未配置。", "Code module exit '{0}' has not been configured.", "No se ha configurado la salida «{0}» del módulo de código." } },
            { "resourceUsed", new[] { "资源仍被以下流程节点引用：{0}", "The resource is still referenced by these flow nodes: {0}", "Estos nodos de flujo todavía usan el recurso: {0}" } },
            { "externalDuplicate", new[] { "外部资源身份已存在：{0}", "External resource identifier already exists: {0}", "Ya existe el identificador del recurso externo: {0}" } },
            { "registrationThread", new[] { "请在主界面线程登记或更换外部资源。", "Register or replace external resources on the main UI thread.", "Registre o sustituya recursos externos en el hilo de la interfaz principal." } },
            { "catalogErrors", new[] { "部分代码模块无法使用，请检查：{0}", "Some code modules are unavailable. Check: {0}", "Algunos módulos de código no están disponibles. Revise: {0}" } }
        };

        /// <summary>取得固定底稿翻译；参数格式化由调用方完成。</summary>
        internal static string T(string key)
        {
            string[] text = words[key];
            if (UiTheme.IsDesignTime) return text[0];
            return MiddleLayer.LangMsg("EquipmentFlow", "code." + key, text[0], text[1], text[2]);
        }
    }
}
