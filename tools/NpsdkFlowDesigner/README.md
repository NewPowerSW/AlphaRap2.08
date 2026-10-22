# NPSDK Flow_Chart 设计器测试版

本版以真实的 `NPSDK.Flow_Chart` 控件为节点。运行入口仍是 `FlowChart_Run()`，回调返回 `IDLE / NEXT / CASE1 / CASE2 / CASE3 / End`，下一节点由 SDK 的 `Next / Casse1 / Casse2 / Casse3` 引用执行。旧 `tools/FlowWorkbench` 是独立运行器原型，不是本版入口。

## 启动

离线测试运行本目录的 `Start.ps1`，或运行已编译包中的 `NpsdkFlowDesigner.exe`。源码构建需要 Visual Studio 的 MSBuild 和 .NET Framework 4.8 目标包。离线程序只登记 DEMO 实例，不连接相机、板卡或扫码枪；它使用真正的 SDK 节点、报警接口和 CSV 文件保存。

在 AlphaRap 中，从“流程”页面点击“Flow_Chart 设计器”。维护或管理员账号可在自动生产、初始化结束并清除报警后试运行。主程序启动和初始化入口会检查设计流程是否仍在执行；框架停止入口也会停止设计流程。

## 编辑与运行

1. 从左边拖出功能到画布；拖动节点调整位置；双击节点内部按钮打开属性。
2. 在“功能方法”选择 `Motion.Move`、`IO.Read`、`IO.Write`、`Vision.Run`、`Scanner.Trigger`、`Data.Save` 或 `Alarm.Show`。在“绑定实例”选择具体对象。`DEMO.*` 表示仿真，AlphaRap 还会列出已创建的真实对象。
3. 下方参数表填写常量，或选择 `Node.Field` 数据来源。带来源的常量格只读。参数类型使用稳定代码 `String / Boolean / Int32 / Double`，小数点固定用点。
4. 在“判定数据”选择当前动作输出或前驱输出。比较方式包括真、假、非空、文本相等/不等、数值大于/小于、范围内。范围写成 `-1,1`，包含边界。
5. 条件满足时走 `Next`；空 `Next` 表示结束。不满足时可选“持续等待”、`Case1/2/3` 或结束；选择 Case 时填写对应跳转目标。画布会显示这些真实 SDK 引用。
6. 延时和超时单位均为 ms。超时 `0` 持续等待；超时后停止本流程，并调用绑定的 `NPSDK.Alarm.Show`。报警编号从已加载表中选择。报警内容为空时沿用框架报警表文案、类型及停机规则。
7. 选择入口，点击“检查”，再手动点击“运行”。下方显示运行轨迹和本轮输出。运行中锁定画布、参数及输出路径；停止后等实际 worker 和已触发任务结束再解锁。

示例为 `Scan → Move → Vision → Save`。`Save` 参数引用 `Scan.Barcode`、`Vision.X/Y/Angle`，执行后写入本地 CSV。“记录目录”打开实际输出位置。

## 保存与下次加载

点击“保存”直接保存默认配置，下次打开设计器自动恢复画布，**不会自动启动设备**。

- AlphaRap：`SysPara.SettingDataDirectory/FlowChartDesigner.xml`，默认目录为 `ModuleData/SettingData`。
- 离线程序：EXE 所在目录的 `FlowChart.xml`。

XML 包含节点稳定 ID、文字、坐标、功能、实例标识、参数类型/常量/来源、Next/Case 跳转、入口、暂停设置、延时、判定条件、超时、报警绑定及生产记录目录。对象本身不序列化；下次启动按身份重新解析现有实例。

允许保存尚未修复的离线绑定草稿。下次加载仍保留这些值，并指出缺失的实例；运行必须通过完整检查。“打开”用于导入其他 XML，之后点击“保存”才写入默认启动配置。

保存通过临时文件刷盘后原子替换，上一版本保存在 `.bak`。主文件损坏时尝试备份；两个文件都无法读取时显示示例，保留损坏原文件。未点击保存的编辑不保证保留。

## 验证条件与报警

离线程序“示例实例参数”中将 `VisionX` 改为 `5`。示例视觉节点要求 `X` 在 `-1,1`，等待超过 3000 ms 后会调用 SDK 报警 `99001` 并结束，后继 Save 不执行。AlphaRap 不向现场报警表自动加入这个示例编号，应选择现场已有报警。

要测试分支，在视觉节点将“不满足时”设为 Case1，再将 Case1 指向一个现有节点。若希望重新拍照，可把 Case1 指回视觉节点；“持续等待”只轮询同一次动作结果，不重复发出拍照、扫码或定位指令。

## 数据保存

每次执行有独立 RunId，节点输出只在本轮会话内共享。循环中读取某节点最近一次已完成的快照；未执行过的来源会明确报错。没有跨轮复用上次条码或坐标的兜底。

在 `Data.Save` 下增加或删除自定义字段，填写字段名和类型，选择前驱输出。`RunId / UtcTime / Node` 为记录保留列。每次进入保存节点产生一个独立 UTF-8 CSV，成功刷盘后发布正式文件。已完成的记录不会因停止而删除。记录不自动触发 MES 或其他上传。

视觉补偿仍由应用者显式配置动作和数据绑定，不自动加到视觉结果中。例如可以把流程调整为 `Scan → Vision → Move → Save`，再将运动 Position 参数绑定到 `Vision.X`；这就是显式数据传递，不是隐式补偿。

## 当前范围

本版提供单入口、顺序和条件分支，以及通过 Case/Next 回跳形成循环；没有并行汇合、多条件表达式编辑器和断点单步。新增应用功能需要工程师登记一个 `FlowActionBinding`，常规流程组合、参数修改、判定、跳转和记录可通过界面完成。

暂停遵循 SDK 节点暂停点，不承诺暂停已发出的物理运动。超时按本次节点进入后的墙钟计算，包含延时及中途暂停，在 SDK 下一次轮询时检查。扫码停止会等待设备读取超时；视觉取消会作废本次结果并等待底层算法释放工位。关闭窗口也会等待在途操作结束。

真实硬件适配已接入，但离线验证不覆盖板卡驱动、行程限制、相机取图、网络扫码和现场互锁；这些项目仍需设备联调。设计器试运行尚未替换原有自动生产入口。

## 语言与代码位置

AlphaRap 使用原有 `RegisterAndApplyLanguage / LangMsg / LanguageChanged` 和三语 XML。固定标题、属性帮助、枚举显示、表头和状态可切换语言；节点名称、参数、实例身份和数据键保留原值。离线程序使用同结构的 `LanguageData`。

- SDK 设计器：`SDK/NPSDK.FlowDesigner`。
- 主程序实例适配：`AlphaRap/Core/MiddleLayer.FlowDesigner.cs`。
- 离线入口：`tools/NpsdkFlowDesigner`。
- 设计器回归：`tests/NpsdkFlowDesigner/Run.ps1`。
- 视觉及语言回归：`tests/VisionRuntime/Run.ps1`。

验证结果与 SDK 源码来源见 `docs/NpsdkFlowDesignerDelivery.md`。
