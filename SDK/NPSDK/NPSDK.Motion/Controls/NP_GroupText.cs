using System;
using System.Windows.Forms;

namespace NPSDK.Motion
{
    internal static class NP_GroupText
    {
        /// <summary>复用点位表的固定按钮名称翻译文字，不修改列键或点位数据。</summary>
        internal static void ApplyPoints(NP_PositionTable table, Func<string,string,string,string,string> text)
        {
            table.ApplyLanguage(text);
        }
    }
}
