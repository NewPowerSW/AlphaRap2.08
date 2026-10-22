using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace NPSDK.Design
{
    public sealed class FlowDesignerEnvironment
    {
        public FlowActionRegistry Registry;
        public Func<string, string> Text;
        public Action<Control> RegisterLanguage;
        public Func<string> CanStart;
        public Func<bool> ExternalPause;
        public Action ShowInstances;
        public string OutputDirectory;
        public string DefaultPath;
        public FlowDesignDocument Example;
        public string Locale = "Chinese";
        public Action<string> ChangeLanguage;
        /// <summary>读取文案键，宿主负责沿用自己的三语言包。</summary>
        public string T(string key)
        {
            return Text == null ? key : Text(key);
        }
    }
}
