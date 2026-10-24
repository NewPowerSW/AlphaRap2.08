using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NPSDK;

namespace AlphaRap.FlowProgramming
{
    /// <summary>只编辑节点源码与编译信息；端口和对象声明由同级设置控件管理。</summary>
    public partial class FlowScriptEditor : UserControl
    {
        private FlowStep bound;
        private FlowActions catalog;
        private FlowScriptSettings settings;
        public event EventHandler DefinitionChanged;
        internal event EventHandler SourceRequested;

        /// <summary>创建固定源码编辑区，不读取设备或编译脚本。</summary>
        public FlowScriptEditor()
        {
            InitializeComponent();
            InitializeAppearance();
            compile.Click += CompileSource;
            errors.TextChanged += (sender, args) => UpdateDiagnostics();
            source.TextChanged += (sender, args) => ShowError("");
            if (!UiTheme.IsDesignTime) MiddleLayer.LanguageChanged += RefreshLanguage;
        }

        /// <summary>关联同级输入输出设置；旧控件只解除事件，不由编辑器释放。</summary>
        internal void BindSettings(FlowScriptSettings value)
        {
            if (ReferenceEquals(settings, value)) return;
            if (settings != null)
            {
                if (bound != null) settings.ReadTo(bound);
                settings.DefinitionChanged -= SettingsChanged;
                settings.InsertRequested -= InsertCall;
                settings.MessageRequested -= ShowError;
            }
            settings = value;
            if (settings == null) return;
            settings.DefinitionChanged += SettingsChanged;
            settings.InsertRequested += InsertCall;
            settings.MessageRequested += ShowError;
            if (bound != null) settings.Bind(bound);
            if (catalog != null) settings.BindObjects(catalog);
        }

        /// <summary>绑定节点副本；新脚本提供最小输入输出，不改变原工程。</summary>
        public void Bind(FlowStep step)
        {
            bound = FlowProgramFile.Snapshot(new FlowProgram { Steps = new List<FlowStep> { step } }).Steps[0];
            bool empty = string.IsNullOrWhiteSpace(bound.ScriptSource) && bound.ScriptInputs.Count == 0 && bound.ScriptOutputs.Count == 0;
            if (empty)
            {
                bound.ScriptSource = "Outputs.Value = Inputs.Value;";
                bound.ScriptInputs.Add(new FlowScriptPort { Name = "Value", Type = "Number", DefaultValue = "0" });
                bound.ScriptOutputs.Add(new FlowScriptPort { Name = "Value", Type = "Number", DefaultValue = "0" });
            }
            source.Text = bound.ScriptSource ?? "";
            if (settings != null) settings.Bind(bound);
            ShowError("");
            RefreshLanguage(this, EventArgs.Empty);
        }

        /// <summary>收集源码及同级设置草稿；未关联设置控件时保留原有端口和对象清单。</summary>
        public void ReadTo(FlowStep step)
        {
            if (settings != null) settings.ReadTo(step);
            else if (bound != null)
            {
                step.ScriptInputs = CopyPorts(bound.ScriptInputs);
                step.ScriptOutputs = CopyPorts(bound.ScriptOutputs);
                step.ScriptResources = (bound.ScriptResources ?? new List<string>()).ToList();
            }
            step.ScriptSource = source.Text;
        }

        /// <summary>复制端口定义，不共用可变实例。</summary>
        private static List<FlowScriptPort> CopyPorts(IEnumerable<FlowScriptPort> ports)
        {
            return ports.Select(port => new FlowScriptPort { Name = port.Name, Type = port.Type, DefaultValue = port.DefaultValue }).ToList();
        }

        /// <summary>将只读对象目录交给输入输出设置，不连接任何实际对象。</summary>
        internal void BindObjects(FlowActions actions)
        {
            catalog = actions;
            if (settings != null) settings.BindObjects(actions);
        }

        /// <summary>转发端口定义修改，宿主按稳定名称保留参数绑定。</summary>
        private void SettingsChanged(object sender, EventArgs args)
        {
            ShowError("");
            EventHandler handler = DefinitionChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>显示诊断或设置提示，空内容收起整行以保留源码面积。</summary>
        internal void ShowError(string message) { errors.Text = message ?? ""; UpdateDiagnostics(); }

        /// <summary>仅有编译或设置诊断时展开信息区。</summary>
        private void UpdateDiagnostics()
        {
            bool visible = errors.TextLength > 0;
            errors.Visible = visible;
            scriptLayout.RowStyles[2].Height = visible ? 96 : 0;
        }

        /// <summary>编译节点副本并显示源码路径和行号，不执行或保存脚本。</summary>
        private void CompileSource(object sender, EventArgs args)
        {
            if (bound == null) return;
            try
            {
                FlowStep candidate = FlowProgramFile.Snapshot(new FlowProgram { Steps = new List<FlowStep> { bound } }).Steps[0];
                candidate.ExecutionMode = "Script";
                ReadTo(candidate);
                FlowScripts.Compile(candidate);
                ShowError(FlowScriptUiText.T("scriptCompiled") + Environment.NewLine + string.Format(FlowScriptUiText.T("scriptSourcePath"), FlowScripts.GetSourcePath(candidate)));
            }
            catch (Exception error) { ShowError(error.Message); }
        }

        /// <summary>插入由设置控件选定的对象方法，并通知宿主回到源码页。</summary>
        private void InsertCall(string target, FlowMethod method)
        {
            int suffix = 1;
            while (source.Text.Contains("callResult" + suffix)) suffix++;
            string text = source.Text;
            int caret = source.SelectionStart;
            bool appendAllowed;
            int start = FindInsertionLine(text, caret, out appendAllowed);
            int end = start;
            while (end < text.Length && (text[end] == ' ' || text[end] == '\t')) end++;
            string indent = text.Substring(start, end - start);
            bool append = appendAllowed && caret == text.Length && caret > end;
            int insertion = append ? caret : start;
            string code = (append ? Environment.NewLine : "") + BuildCall(target, method, suffix, indent);
            source.Select(insertion, 0);
            source.SelectedText = code;
            source.Select(insertion + source.TextLength - text.Length, 0);
            ShowError(FlowScriptUiText.T("scriptInsertHelp"));
            EventHandler handler = SourceRequested;
            if (handler != null) handler(this, EventArgs.Empty);
            source.Focus();
            source.ScrollToCaret();
        }

        /// <summary>定位普通语句插入行；多行字符串或注释内的行退回该标记起始行，仅普通代码末尾允许追加。</summary>
        private static int FindInsertionLine(string text, int caret, out bool appendAllowed)
        {
            const int Code = 0, LineComment = 1, BlockComment = 2, String = 3, VerbatimString = 4, Character = 5;
            int lineStart = caret == 0 ? 0 : text.LastIndexOfAny(new[] { '\r', '\n', '\u0085', '\u2028', '\u2029' }, caret - 1) + 1;
            int state = Code, currentLine = 0, tokenLine = 0, safeLine = lineStart;
            for (int index = 0; index < caret; index++)
            {
                if (index == lineStart && (state == BlockComment || state == VerbatimString)) safeLine = tokenLine;
                char value = text[index], next = index + 1 < caret ? text[index + 1] : '\0';
                if (value == '\r' || value == '\n' || value == '\u0085' || value == '\u2028' || value == '\u2029')
                {
                    currentLine = index + 1;
                    if (state == LineComment) state = Code;
                    continue;
                }
                if (state == LineComment) continue;
                if (state == BlockComment)
                {
                    if (value == '*' && next == '/') { state = Code; index++; }
                    continue;
                }
                if (state == VerbatimString)
                {
                    if (value != '"') continue;
                    if (next == '"') { index++; continue; }
                    state = Code;
                    continue;
                }
                if (state == String || state == Character)
                {
                    if (value == '\\') { index++; continue; }
                    if (value == (state == String ? '"' : '\'')) state = Code;
                    continue;
                }
                if (value == '/' && next == '/')
                {
                    state = LineComment; index++;
                    continue;
                }
                if (value == '/' && next == '*')
                {
                    state = BlockComment; tokenLine = currentLine; index++;
                    continue;
                }
                if (value == '@' && next == '"')
                {
                    state = VerbatimString; tokenLine = currentLine; index++;
                    continue;
                }
                if (value == '"') { state = String; continue; }
                if (value == '\'') state = Character;
            }
            if (caret == lineStart && (state == BlockComment || state == VerbatimString)) safeLine = tokenLine;
            appendAllowed = state == Code && safeLine == lineStart;
            return safeLine;
        }

        /// <summary>按对象和参数的业务标题生成独立代码块；稳定身份及参数键仍用于实际调用。</summary>
        private string BuildCall(string target, FlowMethod method, int suffix, string indent)
        {
            FlowResource resource = catalog == null ? null : catalog.FindResource(target);
            string label = resource == null ? target : resource.Label;
            StringBuilder code = new StringBuilder();
            code.AppendLine(indent + "// " + CommentText(label) + " · " + CommentText(FlowMethods.Title(method)));
            code.AppendLine(indent + "var callResult" + suffix + " = await Context.CallAsync(");
            code.AppendLine(indent + "    targetId: " + Quote(target) + ",");
            code.AppendLine(indent + "    methodId: " + Quote(method.Id) + ",");
            code.AppendLine(indent + "    inputs: new Dictionary<string, object>");
            code.AppendLine(indent + "    {");
            for (int index = 0; index < method.Inputs.Count; index++)
            {
                FlowField field = method.Inputs[index];
                string separator = index + 1 < method.Inputs.Count ? "," : "";
                code.AppendLine(indent + "        { " + Quote(field.Name) + ", " + DefaultLiteral(field) + " }" + separator
                    + " // " + CommentText(FlowMethods.FieldTitle(method.Id, field)));
            }
            code.AppendLine(indent + "    });");
            return code.ToString();
        }

        /// <summary>将业务说明限制在单行注释内，避免名称中的换行成为可执行源码。</summary>
        private static string CommentText(string text)
        {
            return (text ?? "").Replace('\r', ' ').Replace('\n', ' ').Replace('\u0085', ' ').Replace('\u2028', ' ').Replace('\u2029', ' ');
        }

        /// <summary>根据对象字段默认值生成 C# 字面值，真实类型仍由调用服务校验。</summary>
        private static string DefaultLiteral(FlowField field)
        {
            if (field.Type == typeof(string) || field.Type.IsEnum || field.Type == typeof(object)) return Quote(field.DefaultValue ?? "");
            if (field.Type == typeof(bool)) return string.Equals(field.DefaultValue, "true", StringComparison.OrdinalIgnoreCase) || field.DefaultValue == "1" ? "true" : "false";
            try
            {
                string text = Convert.ToString(field.ConvertValue(field.DefaultValue), CultureInfo.InvariantCulture);
                if (field.Type == typeof(double)) return text + (text.Contains(".") || text.Contains("E") || text.Contains("e") ? "d" : ".0d");
                if (field.Type == typeof(float)) return text + "f";
                if (field.Type == typeof(decimal)) return text + "m";
                if (field.Type == typeof(long)) return text + "L";
                if (field.Type.IsPrimitive && field.Type != typeof(char)) return text;
            }
            catch (Exception error) when (error is FormatException || error is OverflowException || error is InvalidCastException || error is InvalidOperationException || error is ArgumentException) { }
            return "null";
        }

        /// <summary>将对象身份和字段键转换为安全的 C# 字符串。</summary>
        private static string Quote(string text)
        {
            return "\"" + (text ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n")
                .Replace("\t", "\\t").Replace("\0", "\\0").Replace("\u0085", "\\u0085").Replace("\u2028", "\\u2028").Replace("\u2029", "\\u2029") + "\"";
        }

        /// <summary>切换固定标题，源码、光标和已有诊断保持原值。</summary>
        private void RefreshLanguage(object sender, EventArgs args) { FlowLanguageForm.ApplyTexts(this); }

        /// <summary>解除语言和同级设置事件，不释放宿主管理的设置控件。</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeAppearance();
                if (!UiTheme.IsDesignTime) MiddleLayer.LanguageChanged -= RefreshLanguage;
                if (settings != null)
                {
                    settings.DefinitionChanged -= SettingsChanged;
                    settings.InsertRequested -= InsertCall;
                    settings.MessageRequested -= ShowError;
                    settings = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}
