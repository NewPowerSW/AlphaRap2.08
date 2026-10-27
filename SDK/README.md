# 核心程序集引用

AlphaRap 与 AlphaRapLibrary 统一引用仓库内 `SDK/lib/NPSDK.dll`。核心包含 Flow_Chart、Flow_BaseForm、流程运行器、报警、运动控制和 IO；主程序使用 ProjectReference 引用 AlphaRapLibrary，输出名为 NewPowerLibrary.dll。

## 当前发布来源

- 源码：[NPSDK 91f6fb6deeee70f510d5d56bbac87fe34a445552](https://github.com/JeffreyWub/NPSDK/commit/91f6fb6deeee70f510d5d56bbac87fe34a445552)。
- 分支：`codex/native-flow-events-20261026`，基于上一发布版本 `fa0a77e`。
- 构建：Debug / x64；SDK 为 .NET Framework 4.7.1，宿主为 4.8。
- 大小：537600 字节。
- SHA256：`2eba16447ec8286e527b5dd123d614ad404bc07f2180a9e9cd17d0322f63be61`。

该版本从独立、干净的源码工作树构建，不包含原 SDK 开发目录中的未提交改动。

## 继承工具接入

宿主自动识别实际 AbstractDevice、NP_Motor、NP_Input、NP_Output、NP_Cylinder 及视觉对象的派生业务方法，不要求为每个工具重新实现 IFlowObject、编写目录或登记工程。标准轴点位和 IO 动作继续沿用既有适配器。具体使用和支持的签名见 [继承工具说明](../docs/InheritedTools.md)。

动态轴、输入和输出资源可以在属性中的“控件类型”选择匹配的公开无参派生类；类型身份由主 Save 保存。SDK 的 NP_IoGroup.BindIO 复用同方向的实际 IO，替换后释放旧控件，并保持卡片布局。NP_Output.ManualOperationAllowed 将派生输出的卡片和指示图点击统一接入宿主权限与资源租约检查。轴组合继续使用原 BindMotor 和绑定点位表。

目前宿主节点只保留“现有功能”和“仅判断条件”。C# 脚本与代码模块的编辑、发现和执行接入已经移除；核心中保留的 V3 元数据和兼容 API 不代表宿主仍提供这些入口。旧 Script/Code 节点必须明确重新配置或删除。

当前 Form 的原生无参 FlowChartRun 方法属于普通动作，自动进入本页【代码流程】工具箱。FlowMethod／FlowStep.UsesFlowResult 明确表示函数返回值直接决定出口；原运行器在同一超时期限内处理 IDLE 轮询、暂停、停止、断点及 NEXT／CASE／End。GetFlowChartHandlers 仅提供原事件快照供旧节点精确导入。使用见 [原生代码 FlowChat](../docs/NativeFlowEvents.md)。

宿主统一使用混合节点流程，加载旧 Legacy 定义时保留布局、连接和可识别的事件绑定，入口有歧义时要求明确选择。界面通过【启用流程】控制模块参与运行，停用模块也不回退旧生命周期；SDK 文件格式仍可读取旧模式供宿主迁移。节点及参数草稿不改变已发布运行版本。

## 配置与运行

- 配置保存统一使用 MainForm Save；节点确定只接受内存草稿。
- Setting 为机器级参数，Recipe 和轴点位由 ProductManagerForm 切换。
- 普通动作的 NEXT/CASE 使用独立条件；原生函数按返回值选择对应出口。超时单独报警，不自动跳 CASE。
- 节点输出遵循当前运行和产品周期；停止等待实际操作完成后释放资源，不发布迟到结果。
- SDK 创建和设计预览不连接硬件；新资源由原主初始化入口连接。

## 验证与构建

SDK 的 IO 绑定回归覆盖实际实例所有权、方向匹配、重复绑定、替换释放、卡片与指示图单次点击、权限和资源租约；原核心 13 组运行回归通过。宿主的构建、流程、主保存、语言及设计器验证随同 AlphaRap 提交说明交付。现场硬件和 Visual Studio 交互式设计器重开仍需现场验证。

原生事件 15 组回归覆盖事件快照、XML身份、全部出口、单一期限、暂停、取消与超时等待在途调用、断点单步、异常报警及不修改业务名称的持久类型标记；原核心及 IO 回归同时通过。

安装 .NET Framework 4.8 开发工具、Cognex VisionPro SDK，并还原 packages.config 后，使用 Visual Studio 或 MSBuild 构建 AlphaRap.sln 的 Debug / x64。

```powershell
MSBuild.exe AlphaRap.sln /t:Rebuild /p:Configuration=Debug /p:Platform=x64
./tests/FlowProgramming/Run.ps1 -AssemblyDirectory AlphaRap/bin/x64/Debug
./tests/VisionRuntime/Run.ps1 -AssemblyDirectory AlphaRap/bin/x64/Debug
./tests/DesignerCompatibility/Run.ps1 -AssemblyDirectory AlphaRap/bin/x64/Debug
```

升级核心时同步更新来源、校验值和验证结果，避免直接引用其他工作区的临时 DLL。
