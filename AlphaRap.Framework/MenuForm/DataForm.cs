using System.IO;
using System.Windows.Forms;

namespace AlphaRap
{
    public partial class DataForm : Form
    {
        private string operationLogRoot;

        /// <summary>创建通用数据页和已保存的动态日志查询入口。</summary>
        public DataForm()
        {
            InitializeComponent();
            if (!UiTheme.IsDesignTime) RefreshLogSources(false);
            FlowProgramming.FlowLanguageForm.ApplyTexts(this);
            if (!UiTheme.IsDesignTime) MiddleLayer.RegisterAndApplyLanguage(this, Name);
            UiTheme.ApplyDpi(this);
        }

        /// <summary>向已登记的错误日志写入去重消息。</summary>
        public void AddLogError(string data) { WriteDistinctLog("LogError", data); }

        /// <summary>向已登记的运行日志写入去重消息。</summary>
        public void AddRunLog(string data) { WriteDistinctLog("LogRun", data); }

        /// <summary>写入一条已登记日志的字段值；未配置的日志不写入。</summary>
        public void WriteLog(string id, params string[] values)
        {
            var source = GetLogSource(id);
            if (source == null) return;
            lock (source) source.SaveDataToFile(values);
        }

        /// <summary>向指定日志写入消息，成功后才记录上次内容。</summary>
        public void WriteDistinctLog(string id, string message)
        {
            RegisteredLog log = FindRegisteredLog(id);
            if (log == null) return;
            lock (log.Source)
            {
                if (log.LastMessage == message) return;
                log.Source.SaveDataToFile(new[] { message });
                log.LastMessage = message;
            }
        }

        /// <summary>在应用配置的操作日志目录下按界面记录工号、权限和操作。</summary>
        public void AddOperationLog(string formName, string action)
        {
            RegisteredLog log = FindRegisteredLog("OperationLog");
            if (log == null) return;
            lock (log.Source)
            {
                if (operationLogRoot == null) operationLogRoot = log.Source.SavePath;
                log.Source.SavePath = Path.Combine(operationLogRoot, SafeFolder(formName));
                log.Source.SaveDataToFile(new[] { SysPara.UserName, SysPara.UserPermission.ToString(), action });
            }
        }

        /// <summary>界面名转成合法目录名。</summary>
        private static string SafeFolder(string name)
        {
            if (string.IsNullOrEmpty(name)) return "Unknown";
            foreach (char character in Path.GetInvalidFileNameChars()) name = name.Replace(character, '_');
            return name;
        }
    }
}
