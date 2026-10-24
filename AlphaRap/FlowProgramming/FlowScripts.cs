using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    /// <summary>按 C# 5 编译脚本函数体并复用已加载版本；脚本运行在当前进程，不是安全沙箱。</summary>
    public static class FlowScripts
    {
        private const int VersionLimit = 256;
        private static readonly object cacheSync = new object();
        private static readonly Dictionary<string, CompiledScript> compiled = new Dictionary<string, CompiledScript>();
        private static readonly Assembly classicSystem = typeof(Uri).Assembly;
        private static readonly string cacheDirectory = Path.Combine(Path.GetTempPath(), "AlphaRap", "FlowScripts", Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture));

        private sealed class ScriptDefinition
        {
            internal string Source, Key;
            internal List<FlowScriptPort> Inputs, Outputs;
            internal List<string> Resources;
        }

        private sealed class CompiledScript
        {
            internal MethodInfo Entry;
        }

        /// <summary>声明脚本或空节点的实际输入输出，业务端口名称与持久化键保持一致。</summary>
        public static FlowMethod Define(FlowStep step)
        {
            if (step == null) throw new ArgumentNullException("step");
            if (step.ExecutionMode == "None")
            {
                FlowMethod empty = new FlowMethod("flow.none", "空节点", "Empty node", "Nodo vacío") { UsesHardware = false };
                empty.Invoke = call => { call.Token.ThrowIfCancellationRequested(); return Task.FromResult<IDictionary<string, object>>(new Dictionary<string, object>()); };
                return empty;
            }
            if (step.ExecutionMode != "Script") throw new InvalidOperationException(FlowScriptText.T("scriptInvalidMode"));
            ScriptDefinition definition = ReadDefinition(step, false);
            FlowMethod method = new FlowMethod("script.execute", "执行 C# 脚本", "Execute C# script", "Ejecutar script C#") { UsesHardware = false };
            method.Inputs.AddRange(definition.Inputs.Select(port => new FlowField(port.Name, PortType(port), port.DefaultValue ?? "")));
            method.Outputs.AddRange(definition.Outputs.Select(port => new FlowField(port.Name, PortType(port))));
            method.Invoke = call => { throw new InvalidOperationException(FlowScriptText.T("scriptPipelineRequired")); };
            return method;
        }

        /// <summary>校验并缓存 C# 5 编译结果，不执行函数体；非脚本节点无需编译。</summary>
        public static void Compile(FlowStep step)
        {
            if (step == null) throw new ArgumentNullException("step");
            if (step.ExecutionMode != "Script") return;
            GetCompiled(ReadDefinition(step, true));
        }

        /// <summary>返回当前源码版本映射的用户脚本文件路径，便于定位错误或在附加调试器中打开。</summary>
        public static string GetSourcePath(FlowStep step)
        {
            if (step == null) throw new ArgumentNullException("step");
            if (step.ExecutionMode != "Script") return "";
            ScriptDefinition definition = ReadDefinition(step, false);
            return Path.Combine(cacheDirectory, definition.Key, "UserScript.cs");
        }

        /// <summary>执行脚本并收拢所有受控调用；错误和取消路径仍等待实际动作结束后退出。</summary>
        public static async Task<IDictionary<string, object>> Execute(FlowStep step, FlowCall call, FlowScriptContext context)
        {
            if (context == null) throw new ArgumentNullException("context");
            ScriptDefinition definition = null;
            bool entered = false;
            try
            {
                if (step == null) throw new ArgumentNullException("step");
                if (call == null) throw new ArgumentNullException("call");
                call.Token.ThrowIfCancellationRequested();
                context.Token.ThrowIfCancellationRequested();
                if (step.ExecutionMode == "None")
                {
                    await context.CloseAsync(false).ConfigureAwait(false);
                    call.Token.ThrowIfCancellationRequested();
                    return new Dictionary<string, object>();
                }
                if (step.ExecutionMode != "Script") throw new InvalidOperationException(FlowScriptText.T("scriptInvalidMode"));
                definition = ReadDefinition(step, true);
                Dictionary<string, object> inputs = ReadInputs(definition, call.Inputs);
                CompiledScript script = GetCompiled(definition);
                call.Token.ThrowIfCancellationRequested();
                context.Token.ThrowIfCancellationRequested();
                entered = true;
                IDictionary<string, object> result = await InvokeScript(script, context, inputs).ConfigureAwait(false);
                call.Token.ThrowIfCancellationRequested();
                context.Token.ThrowIfCancellationRequested();
                IDictionary<string, object> outputs = ReadOutputs(definition, result);
                await context.CloseAsync(false).ConfigureAwait(false);
                call.Token.ThrowIfCancellationRequested();
                context.Token.ThrowIfCancellationRequested();
                return outputs;
            }
            catch (Exception error)
            {
                await CloseAfterFailure(context, error).ConfigureAwait(false);
                if (!entered || error is OperationCanceledException) throw;
                throw RuntimeError(definition, error);
            }
        }

        /// <summary>取消并等待节点作用域，不用后续调用异常覆盖已经发生的用户异常。</summary>
        private static async Task CloseAfterFailure(FlowScriptContext context, Exception original)
        {
            try { await context.CloseAsync(true).ConfigureAwait(false); }
            catch (Exception pending)
            {
                if (!ReferenceEquals(original, pending)) original.Data["FlowScript.PendingCallsError"] = pending;
            }
        }

        /// <summary>由生成的强类型输出属性检查赋值状态，错误文案使用执行时当前语言。</summary>
        public static void RequireOutput(bool assigned, string name)
        {
            if (!assigned) throw new InvalidOperationException(FlowScriptText.T("scriptMissingOutput", name));
        }

        /// <summary>调用已编译入口并保留原始异常堆栈，等待函数返回后才结束此脚本调用。</summary>
        private static async Task<IDictionary<string, object>> InvokeScript(CompiledScript script, FlowScriptContext context, IDictionary<string, object> inputs)
        {
            Task<IDictionary<string, object>> pending;
            try { pending = (Task<IDictionary<string, object>>)script.Entry.Invoke(null, new object[] { context, inputs }); }
            catch (TargetInvocationException error)
            {
                ExceptionDispatchInfo.Capture(error.InnerException ?? error).Throw();
                throw;
            }
            return await pending.ConfigureAwait(false);
        }

        /// <summary>把可用 PDB 的真实用户源码位置加入错误消息，保留原异常供详细诊断。</summary>
        private static Exception RuntimeError(ScriptDefinition definition, Exception error)
        {
            string source = Path.Combine(cacheDirectory, definition.Key, "UserScript.cs");
            StackFrame[] frames = new StackTrace(error, true).GetFrames() ?? new StackFrame[0];
            StackFrame user = frames.FirstOrDefault(frame => frame.GetFileLineNumber() > 0 && !string.IsNullOrEmpty(frame.GetFileName()) &&
                string.Equals(frame.GetFileName().Replace('/', '\\'), source.Replace('/', '\\'), StringComparison.OrdinalIgnoreCase));
            int line = user == null ? 0 : user.GetFileLineNumber();
            string message = line > 0 ? FlowScriptText.T("scriptRuntimeFailed", source, line, error.Message) : FlowScriptText.T("scriptRuntimeNoLine", source, error.Message);
            InvalidOperationException located = new InvalidOperationException(message, error);
            located.Data["FlowScript.SourcePath"] = source;
            located.Data["FlowScript.Line"] = line;
            return located;
        }

        /// <summary>复制并校验端口与允许对象清单，源码和程序集身份共同确定缓存版本。</summary>
        private static ScriptDefinition ReadDefinition(FlowStep step, bool requireSource)
        {
            ScriptDefinition definition = new ScriptDefinition { Source = step.ScriptSource ?? "", Inputs = CopyPorts(step.ScriptInputs), Outputs = CopyPorts(step.ScriptOutputs), Resources = (step.ScriptResources ?? new List<string>()).ToList() };
            if (requireSource && string.IsNullOrWhiteSpace(definition.Source)) throw new InvalidOperationException(FlowScriptText.T("scriptEmptySource"));
            ValidatePorts(definition.Inputs);
            ValidatePorts(definition.Outputs);
            if (definition.Resources.Any(string.IsNullOrWhiteSpace) || definition.Resources.Distinct(StringComparer.Ordinal).Count() != definition.Resources.Count)
                throw new InvalidOperationException(FlowScriptText.T("scriptInvalidResource"));
            definition.Key = DefinitionKey(definition);
            return definition;
        }

        /// <summary>复制端口列表，拒绝空端口项，不在运行中引用可编辑的原列表。</summary>
        private static List<FlowScriptPort> CopyPorts(List<FlowScriptPort> ports)
        {
            if (ports == null) return new List<FlowScriptPort>();
            if (ports.Any(port => port == null)) throw new InvalidOperationException(FlowScriptText.T("scriptInvalidPort", ""));
            return ports.Select(port => new FlowScriptPort { Name = port.Name, Type = port.Type, DefaultValue = port.DefaultValue }).ToList();
        }

        /// <summary>检查 C# 标识符、同方向唯一性和显式默认值，不将空默认值当作数值零。</summary>
        private static void ValidatePorts(List<FlowScriptPort> ports)
        {
            HashSet<string> names = new HashSet<string>(StringComparer.Ordinal);
            using (IDisposable provider = CreateProvider())
            {
                MethodInfo validIdentifier = provider.GetType().GetMethod("IsValidIdentifier", new[] { typeof(string) });
                foreach (FlowScriptPort port in ports)
                {
                    if (string.IsNullOrWhiteSpace(port.Name) || port.Name.StartsWith("__", StringComparison.Ordinal) || port.Name.Contains("@") || !(bool)validIdentifier.Invoke(provider, new object[] { port.Name }))
                        throw new InvalidOperationException(FlowScriptText.T("scriptInvalidPort", port.Name));
                    if (!names.Add(port.Name)) throw new InvalidOperationException(FlowScriptText.T("scriptDuplicatePort", port.Name));
                    Type type = PortType(port);
                    if (string.IsNullOrEmpty(port.DefaultValue)) continue;
                    try { new FlowField(port.Name, type).ConvertValue(port.DefaultValue); }
                    catch (Exception error) when (error is FormatException || error is OverflowException || error is InvalidCastException || error is InvalidOperationException || error is ArgumentException)
                    {
                        throw new InvalidOperationException(FlowScriptText.T("scriptInvalidDefault", port.Name), error);
                    }
                }
            }
        }

        /// <summary>脚本端口只采用四种稳定业务类型，Integer 明确使用 Int64。</summary>
        private static Type PortType(FlowScriptPort port)
        {
            switch (port.Type)
            {
                case "Boolean": return typeof(bool);
                case "Integer": return typeof(long);
                case "Number": return typeof(double);
                case "Text": return typeof(string);
                default: throw new InvalidOperationException(FlowScriptText.T("scriptInvalidType", port.Name, port.Type));
            }
        }

        /// <summary>使用有长度边界的内容摘要关联源码、端口、对象白名单与实际程序集版本。</summary>
        private static string DefinitionKey(ScriptDefinition definition)
        {
            using (MemoryStream data = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(data, Encoding.UTF8))
            {
                writer.Write("CSharp5.Debug.v1"); writer.Write(definition.Source);
                WritePorts(writer, definition.Inputs); WritePorts(writer, definition.Outputs);
                writer.Write(definition.Resources.Count);
                foreach (string resource in definition.Resources.OrderBy(value => value, StringComparer.Ordinal)) writer.Write(resource);
                foreach (Assembly assembly in new[] { typeof(FlowScripts).Assembly, typeof(FlowStep).Assembly, typeof(object).Assembly, typeof(Enumerable).Assembly })
                {
                    writer.Write(assembly.FullName); writer.Write(assembly.ManifestModule.ModuleVersionId.ToString("D"));
                }
                writer.Flush();
                using (SHA256 algorithm = SHA256.Create()) return string.Concat(algorithm.ComputeHash(data.ToArray()).Select(value => value.ToString("x2", CultureInfo.InvariantCulture)));
            }
        }

        /// <summary>按声明顺序记录端口身份、类型和默认值，避免不同结构共享缓存。</summary>
        private static void WritePorts(BinaryWriter writer, List<FlowScriptPort> ports)
        {
            writer.Write(ports.Count);
            foreach (FlowScriptPort port in ports) { writer.Write(port.Name); writer.Write(port.Type); writer.Write(port.DefaultValue ?? ""); }
        }

        /// <summary>同版本只加载一次；进程达到版本上限后明确要求重启，不伪装卸载程序集。</summary>
        private static CompiledScript GetCompiled(ScriptDefinition definition)
        {
            lock (cacheSync)
            {
                CompiledScript cached;
                if (compiled.TryGetValue(definition.Key, out cached)) return cached;
                if (compiled.Count >= VersionLimit) throw new InvalidOperationException(FlowScriptText.T("scriptVersionLimit", VersionLimit));
                CompiledScript result = Build(definition);
                compiled.Add(definition.Key, result);
                return result;
            }
        }

        /// <summary>在本地临时目录保存用户源码、生成包装、DLL 和 PDB，保留 C# 5 调试信息。</summary>
        private static CompiledScript Build(ScriptDefinition definition)
        {
            string directory = Path.Combine(cacheDirectory, definition.Key);
            string userSource = Path.Combine(directory, "UserScript.cs");
            try
            {
                Directory.CreateDirectory(directory);
                string generated = Path.Combine(directory, "GeneratedScript.cs");
                string assemblyPath = Path.Combine(directory, "FlowScript_" + definition.Key + ".dll");
                File.WriteAllText(userSource, definition.Source, new UTF8Encoding(false));
                File.WriteAllText(generated, GenerateSource(definition, userSource), new UTF8Encoding(false));
                if (!File.Exists(assemblyPath) || !File.Exists(Path.ChangeExtension(assemblyPath, ".pdb"))) CompileSource(generated, assemblyPath, userSource);
                Type entryType = Assembly.LoadFrom(assemblyPath).GetType("AlphaRap.GeneratedFlowScripts.Script_" + definition.Key, true);
                return new CompiledScript { Entry = entryType.GetMethod("Execute", BindingFlags.Public | BindingFlags.Static) };
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException || error is BadImageFormatException || error is TypeLoadException || error is FileLoadException)
            {
                throw new InvalidOperationException(FlowScriptText.T("scriptCacheFailed", userSource), error);
            }
        }

        /// <summary>CodeDom 编译仅验证函数与生成端口，不加载或运行任何设备方法。</summary>
        private static void CompileSource(string generated, string assemblyPath, string userSource)
        {
            Type parameterType = classicSystem.GetType("System.CodeDom.Compiler.CompilerParameters", true);
            object options = Activator.CreateInstance(parameterType);
            parameterType.GetProperty("GenerateExecutable").SetValue(options, false, null);
            parameterType.GetProperty("GenerateInMemory").SetValue(options, false, null);
            parameterType.GetProperty("IncludeDebugInformation").SetValue(options, true, null);
            parameterType.GetProperty("OutputAssembly").SetValue(options, assemblyPath, null);
            parameterType.GetProperty("CompilerOptions").SetValue(options, "/langversion:5 /optimize- /debug:full", null);
            IList references = (IList)parameterType.GetProperty("ReferencedAssemblies").GetValue(options, null);
            foreach (string reference in new[] { typeof(object).Assembly.Location, classicSystem.Location, typeof(Enumerable).Assembly.Location, typeof(FlowScripts).Assembly.Location, typeof(FlowStep).Assembly.Location,
                Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), "Microsoft.CSharp.dll") }.Distinct(StringComparer.OrdinalIgnoreCase)) references.Add(reference);
            using (IDisposable provider = CreateProvider())
            {
                object results = InvokeCompiler(provider, options, parameterType, generated);
                IEnumerable diagnostics = (IEnumerable)results.GetType().GetProperty("Errors").GetValue(results, null);
                List<string> errors = diagnostics.Cast<object>().Where(error => !(bool)error.GetType().GetProperty("IsWarning").GetValue(error, null)).Select(CompilerDiagnostic).ToList();
                if (errors.Count == 0) return;
                string details = string.Join(Environment.NewLine, errors);
                throw new InvalidOperationException(FlowScriptText.T("scriptCompileFailed", details, userSource));
            }
        }

        /// <summary>明确使用 Framework 的编译器，避免与宿主已引用的 System.CodeDom 包出现同名类型冲突。</summary>
        private static IDisposable CreateProvider()
        {
            Type providerType = classicSystem.GetType("Microsoft.CSharp.CSharpCodeProvider", true);
            return (IDisposable)Activator.CreateInstance(providerType, new object[] { new Dictionary<string, string> { { "CompilerVersion", "v4.0" } } });
        }

        /// <summary>调用 Framework CodeDom 并保留编译器原异常，不在此层执行脚本。</summary>
        private static object InvokeCompiler(object provider, object options, Type parameterType, string generated)
        {
            try
            {
                MethodInfo compile = provider.GetType().GetMethod("CompileAssemblyFromFile", new[] { parameterType, typeof(string[]) });
                return compile.Invoke(provider, new object[] { options, new[] { generated } });
            }
            catch (TargetInvocationException error)
            {
                ExceptionDispatchInfo.Capture(error.InnerException ?? error).Throw();
                throw;
            }
        }

        /// <summary>保留编译器提供的用户源码文件、行列、错误编号和说明。</summary>
        private static string CompilerDiagnostic(object error)
        {
            Type type = error.GetType();
            return string.Format(CultureInfo.InvariantCulture, "{0}({1},{2}): {3} {4}", type.GetProperty("FileName").GetValue(error, null),
                type.GetProperty("Line").GetValue(error, null), type.GetProperty("Column").GetValue(error, null),
                type.GetProperty("ErrorNumber").GetValue(error, null), type.GetProperty("ErrorText").GetValue(error, null));
        }

        /// <summary>生成带用户源码行号的异步函数，输入只读、输出逐项记录赋值状态。</summary>
        private static string GenerateSource(ScriptDefinition definition, string userSource)
        {
            StringBuilder code = new StringBuilder();
            code.AppendLine("using System; using System.Collections.Generic; using System.Linq; using System.Threading; using System.Threading.Tasks; using AlphaRap.FlowProgramming;");
            code.AppendLine("namespace AlphaRap.GeneratedFlowScripts { public static class Script_" + definition.Key + " {");
            code.AppendLine("public static async Task<IDictionary<string, object>> Execute(FlowScriptContext Context, IDictionary<string, object> __data) {");
            code.AppendLine("Context.Token.ThrowIfCancellationRequested(); var Inputs = new __Inputs(__data); var Outputs = new __Outputs();");
            code.AppendLine("#line 1 " + Literal(userSource.Replace('\\', '/')));
            code.AppendLine("if (Context.DebugBreak && System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();");
            code.AppendLine("#line default");
            code.AppendLine("await __Run(Context, Inputs, Outputs).ConfigureAwait(false); Context.Token.ThrowIfCancellationRequested(); return Outputs.__Collect(); }");
            AppendInputs(code, definition.Inputs);
            AppendOutputs(code, definition.Outputs);
            code.AppendLine("private static async Task __Run(FlowScriptContext Context, __Inputs Inputs, __Outputs Outputs) {");
            code.AppendLine("#line 1 " + Literal(userSource.Replace('\\', '/')));
            code.AppendLine(definition.Source);
            code.AppendLine("#line default"); code.AppendLine("} } }");
            return code.ToString();
        }

        /// <summary>生成类型明确且只读的 Inputs 属性，值由运行器完成转换后传入。</summary>
        private static void AppendInputs(StringBuilder code, List<FlowScriptPort> inputs)
        {
            code.AppendLine("public sealed class __Inputs { private readonly IDictionary<string, object> __data; public __Inputs(IDictionary<string, object> data) { __data = data; }");
            foreach (FlowScriptPort port in inputs) code.AppendLine("public " + PortType(port).FullName + " " + port.Name + " { get { return (" + PortType(port).FullName + ")__data[" + Literal(port.Name) + "]; } }");
            code.AppendLine("}");
        }

        /// <summary>生成带赋值标记的 Outputs 属性；读取或提交未赋值输出时明确报错。</summary>
        private static void AppendOutputs(StringBuilder code, List<FlowScriptPort> outputs)
        {
            code.AppendLine("public sealed class __Outputs {");
            for (int index = 0; index < outputs.Count; index++)
            {
                FlowScriptPort port = outputs[index]; string suffix = index.ToString(CultureInfo.InvariantCulture), type = PortType(port).FullName;
                code.AppendLine("private " + type + " __value" + suffix + "; private bool __assigned" + suffix + ";");
                code.AppendLine("public " + type + " " + port.Name + " { get { FlowScripts.RequireOutput(__assigned" + suffix + ", " + Literal(port.Name) + "); return __value" + suffix + "; } set { __value" + suffix + " = value; __assigned" + suffix + " = true; } }");
            }
            code.AppendLine("public IDictionary<string, object> __Collect() { var __result = new Dictionary<string, object>();");
            foreach (FlowScriptPort port in outputs) code.AppendLine("__result.Add(" + Literal(port.Name) + ", " + port.Name + ");");
            code.AppendLine("return __result; } }");
        }

        /// <summary>转义生成源码中的稳定端口键和文件路径，不将业务文本作为代码拼接。</summary>
        private static string Literal(string value)
        {
            return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t").Replace("\0", "\\0") + "\"";
        }

        /// <summary>按端口类型读取本次输入，缺失和非法数据不使用默认值继续运行。</summary>
        private static Dictionary<string, object> ReadInputs(ScriptDefinition definition, IDictionary<string, object> values)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            foreach (FlowScriptPort port in definition.Inputs)
            {
                object value;
                if (values == null || !values.TryGetValue(port.Name, out value)) throw new InvalidOperationException(FlowScriptText.T("scriptMissingInput", port.Name));
                result.Add(port.Name, ConvertPortValue(port, value, "scriptInputType"));
            }
            return result;
        }

        /// <summary>在脚本完成后再次校验声明输出，拒绝空数值、NaN、无穷和类型错误。</summary>
        private static IDictionary<string, object> ReadOutputs(ScriptDefinition definition, IDictionary<string, object> values)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            foreach (FlowScriptPort port in definition.Outputs)
            {
                object value;
                if (values == null || !values.TryGetValue(port.Name, out value)) throw new InvalidOperationException(FlowScriptText.T("scriptMissingOutput", port.Name));
                result.Add(port.Name, ConvertPortValue(port, value, "scriptOutputType"));
            }
            return result;
        }

        /// <summary>复用框架严格类型转换并补充脚本端口的业务错误位置。</summary>
        private static object ConvertPortValue(FlowScriptPort port, object value, string errorKey)
        {
            try { return new FlowField(port.Name, PortType(port)).ConvertValue(value); }
            catch (Exception error) when (error is FormatException || error is OverflowException || error is InvalidCastException || error is InvalidOperationException || error is ArgumentException)
            {
                throw new InvalidOperationException(FlowScriptText.T(errorKey, port.Name), error);
            }
        }
    }
}
