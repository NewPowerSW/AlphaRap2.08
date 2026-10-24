using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
            string arguments = string.Join(", ", method.Inputs.Select(field => "{ " + Quote(field.Name) + ", " + DefaultLiteral(field) + " }"));
            int suffix = 1;
            while (source.Text.Contains("callResult" + suffix)) suffix++;
            source.SelectedText = "var callResult" + suffix + " = await Context.CallAsync(" + Quote(target) + ", " + Quote(method.Id) + ", new Dictionary<string, object> { " + arguments + " });" + Environment.NewLine;
            ShowError(FlowScriptUiText.T("scriptInsertHelp"));
            EventHandler handler = SourceRequested;
            if (handler != null) handler(this, EventArgs.Empty);
            source.Focus();
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
        private static string Quote(string text) { return "\"" + (text ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n") + "\""; }

        /// <summary>切换固定标题，源码、光标和已有诊断保持原值。</summary>
        private void RefreshLanguage(object sender, EventArgs args) { FlowLanguageForm.ApplyTexts(this); }

        /// <summary>解除语言和同级设置事件，不释放宿主管理的设置控件。</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
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
