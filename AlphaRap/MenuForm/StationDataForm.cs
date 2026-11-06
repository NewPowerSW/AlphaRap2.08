using System.Collections.Generic;
using DataSave;

namespace AlphaRap
{
    /// <summary>为本站数据页提供固定日志组件及稳定的流程资源键。</summary>
    public partial class StationDataForm : DataForm
    {
        /// <summary>登记本站日志，供数据页、流程及历史查询复用。</summary>
        public StationDataForm()
        {
            InitializeComponent();
            RegisterLogSources(
                new KeyValuePair<string, TypeCollection1>("MESLOG", MESLOG),
                new KeyValuePair<string, TypeCollection1>("Alarm", Alarm),
                new KeyValuePair<string, TypeCollection1>("LogRun", LogRun),
                new KeyValuePair<string, TypeCollection1>("LogError", LogError),
                new KeyValuePair<string, TypeCollection1>("UserLogin", UserLogin),
                new KeyValuePair<string, TypeCollection1>("OperationLog", UserOperationLog));
        }
    }
}
