using System;

namespace AlphaRap
{
    /// <summary>
    /// 界面操作日志：谁（工号 / 权限）在什么时候、在哪个界面、做了什么。
    ///
    /// 只记录**对设备有实际影响的操作**：增删改、参数修改、设备初始化、运行 / 启动 / 停止 / 暂停 / 复位、
    /// 权限与使能变更、报警表变更等；普通点击（翻页、展开、查看）不记。
    ///
    /// 落盘复用 DataForm 的 DataSave 机制（见 <see cref="DataForm.AddOperationLog"/>）：
    /// 每个界面一个目录 D:\操作日志\{界面名}\{年}\{月}\{yyyy-MM-dd}.csv，
    /// 列：Time（库自动填） / UserID（工号） / UserPermission（权限） / Action（做了什么）。
    /// </summary>
    public static class OperationLog
    {
        /// <summary>日志根目录；其下按界面名分目录。</summary>
        public const string RootPath = @"D:\操作日志";

        /// <summary>写一条操作记录；<paramref name="formName"/> 同时用作分目录名。</summary>
        public static void Write(string formName, string action)
        {
            if (string.IsNullOrEmpty(action)) return;
            try
            {
                if (MiddleLayer.DataF != null) MiddleLayer.DataF.AddOperationLog(formName, action);
            }
            catch (Exception) { }
        }
    }
}
