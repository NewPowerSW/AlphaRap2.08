# NPSDK Flow_Chart 设计器交付

本实现替代此前独立 FlowWorkbench 原型作为当前测试方向。节点、执行回调、IDLE 等待、Next/Case 交接均使用真实 NPSDK；设计器没有自行建立一套分支调度器。

本次包含拖拽、双击属性、功能与现有实例选择、有类型参数、前驱输出引用、条件与分支、可选超时、Alarm.Show、本地 CSV、三语，以及保存后重启自动恢复。AlphaRap 流程页提供入口，原有生产流程保留。

## SDK 二进制兼容

发布 DLL 从 SDK 已提交基线 `247b846` 加本次核心补丁构建，未从带其他修改的 SDK 工作目录直接打包。公共符号对照没有移除现用 DLL 接口。`AlarmReset` 和 `ReverseState` 兼容入口一起保留。

对现用 DLL 做了 8 组 IO 极性对照，涵盖输入物理高低、ReverseState 开关以及输出 On/Off。新 DLL 沿用现用 DLL 行为：默认不取反，ReverseState=true 时取反。

SDK 原工作目录的未提交极性代码与现用 DLL 行为相反，未混入本次发布；其余运动面板、厂商驱动和参数面板等未提交修改也保留在工作目录。提交索引按干净基线补丁构建，不覆盖这些修改。

`SDK/NPSDK.Core.FlowDesigner.patch` 保存可对照的核心源码补丁；SDK 远端提交及 DLL 摘要记录在 `SDK/NPSDK.FlowDesigner/CoreSource.txt`。

## 验证

设计器回归使用实际 SDK 控件及 BackgroundWorker，开启跨线程 UI 检查，不连接硬件。覆盖真实拖拽/双击、Next/Case、一次触发和轮询、重复运行、自循环、暂停/停止、在途任务等待、实际 SDK 报警、CSV 数据引用、XML 原子保存、重启恢复、缺失对象、损坏备份回退和三语编辑值保护。

AlphaRap 使用新 DLL 完整构建，并运行既有视觉/语言回归。三语 XML 另外执行键、占位符和译文检查。验证日志和截图保存在本地 `.vp-validation`，该目录不上传源码仓库。

尚未现场验证真实运动、IO 电气信号、相机取图、扫码通讯及设备互锁。离线通过不等于设备流程已经验收；本版作为设计器测试入口交付。
