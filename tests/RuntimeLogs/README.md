# 运行日志回归

先编译项目，再执行 `./tests/RuntimeLogs/Run.ps1 -AssemblyDirectory 'AlphaRap/bin/x64/Debug'`。

测试只在 `.vp-validation/runtime-logs/<运行目录>` 写入隔离日志，不改动现场日志或配置。测试实际 DataSave 写入的运行、错误记录，校验筛选、重复时间戳、数量上限、取消、异常格式提示和 CSV 原文导出，并验证三语页面、输入保护、后台查询完成状态及隐藏后停止刷新。

截图包括最小主窗口对应的 1048×708 页面和 1368×908 页面。界面验证需要可以创建 Windows 桌面控件的执行环境；不运行机器初始化、运动、IO 写入或相机采集。
