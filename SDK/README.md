# 核心程序集引用

`lib/NPSDK.dll` 是 AlphaRap 与 AlphaRapLibrary 共用的核心程序集，包含 JTimer、Alarm、Flow_Chart、Flow_BaseForm 和 NPSDK.Motion 等接口。

两个项目均通过仓库相对路径 `..\SDK\lib\NPSDK.dll` 引用它。不要将核心 DLL 的唯一副本放在构建输出目录，清理或重新生成可能删除输出目录中的依赖副本。

AlphaRap 使用 ProjectReference 引用 AlphaRapLibrary；其输出名称仍为 NewPowerLibrary.dll。MSBuild 会按依赖顺序构建并复制到主程序输出目录，Debug/Release 不再依赖固定的 `AlphaRapLibrary\bin\Debug` 文件。

## 当前二进制来源

- 来源：[NPSDK 0f4b0a8a1b66e77226a1aa07e80282eb4e1bf10d](https://github.com/JeffreyWub/NPSDK/commit/0f4b0a8a1b66e77226a1aa07e80282eb4e1bf10d)，分支 `codex/flowchart-designer-20261022`。
- 从该提交包含的源码构建，排除了外部工作区尚未提交的其他运动控件改动；Debug / x64，SDK 目标 .NET Framework 4.7.1，宿主目标 4.8。
- 大小：475648 字节。
- SHA256：`12ac6d2a03ac4fb2f85891fff7228a294b2fbf578a268d3b0b31c6128e4c44ec`。
- 包含全部 Flow_BaseForm 的统一控制器工厂、鼠标捕获拖动和配置入口、版本 2 工程 XML、NEXT / CASE 独立条件运行器、独立超时报警、空模块删除标记校验、具名结果与资源租约，以及现有宿主使用的 AlarmReset 转发接口。另包含轴和 IO 组合控件、点位右键删除、宿主管理的延迟初始化，以及动态控件扫描与释放互斥。IO 组合直接显示实际信号卡片，手动输出先检查宿主权限与租约，内部 Name 保持身份。核心提供同批次参数发布；宿主 constant.set 直接更新实际 Setting / Recipe 表值，后续批次读取该值，文件仍由主 Save 写入。流程能力契约 IFlowObject / FlowMethod / FlowField / FlowCall 由对象声明类型化方法，支持输入约束与独立模拟实现。FlowField 另声明可选的三语业务标题、帮助、单位、高级参数及来源动作／输出约束，宿主据此呈现业务参数行和统一绑定窗口，底层字段身份和调用协议保持兼容。

升级 NPSDK 时替换此处的已验证版本并更新来源和校验值；两个项目始终引用同一版本。不要直接引用其他开发工作区中含未提交修改的构建产物。

## 构建和回归

安装 .NET Framework 4.8 开发工具、项目所需的 Cognex VisionPro SDK，并还原 packages.config 依赖后，在 Visual Studio 中重新生成 `AlphaRap.sln`，选择 `Debug | x64`。

开发者命令行可运行：

```powershell
MSBuild.exe AlphaRap.sln /t:Rebuild /p:Configuration=Debug /p:Platform=x64
./tests/VisionRuntime/Run.ps1 -AssemblyDirectory AlphaRap/bin/x64/Debug
./tests/FlowProgramming/Run.ps1 -AssemblyDirectory AlphaRap/bin/x64/Debug
```

Debug / x64 隔离构建通过，使用实际输出程序集通过原有 62 项回归和 80 项流程与资源集成回归及 6 项启动 / 主保存检查，语言 XML 1901 键三语校验无错误。验证没有使用临时 ReferencePath。隔离运行真实主程序验证了进入流程、节点拖动、双击和添加 CASE；未进行现场硬件或 Visual Studio 交互式设计器实测。

关闭运行实例后，原 `AlphaRap/bin/x64/Debug` 已正常构建更新，输出 NPSDK.dll 的 SHA256 与上方发布版本一致。直接使用该目录的程序集再次通过 80 项流程与资源检查、6 项启动 / 主保存检查和 62 项视觉与语言检查；测试配置、配方和生产记录均写入独立测试目录，没有启动硬件或生产线程。

功能入口与操作见 [节点设计器使用说明](../docs/FlowProgrammingUsage.md) 和 [组合硬件资源使用说明](../docs/HardwareResources.md)。
