# 核心程序集引用

AlphaRap 与 AlphaRapLibrary 统一引用仓库内 `SDK/lib/NPSDK.dll`。核心包含 Flow_Chart、Flow_BaseForm、流程运行器、报警、运动控制和 IO；主程序使用 ProjectReference 引用 AlphaRapLibrary，输出名为 NewPowerLibrary.dll。

## 当前发布来源

- 源码基线：[NPSDK 91f6fb6deeee70f510d5d56bbac87fe34a445552](https://github.com/JeffreyWub/NPSDK/commit/91f6fb6deeee70f510d5d56bbac87fe34a445552)。
- 来源元数据扩展：[NPSDK c9f57200e619c7cb4dd9a7e0975160e02564373a](https://github.com/JeffreyWub/NPSDK/commit/c9f57200e619c7cb4dd9a7e0975160e02564373a)，分支 `codex/source-control-sync-20261028`，已推送并核对远端提交。
- 实际运行状态桥接：[NPSDK 1d7535de524b1c8f51d6907ed6591394fbde8d27](https://github.com/JeffreyWub/NPSDK/commit/1d7535de524b1c8f51d6907ed6591394fbde8d27)，分支 `codex/flow-runtime-state-20261028`。
- 轴手动会话停止：[NPSDK c124b9d4549ea3f6ad87ef0c254d37dbe0fd4373](https://github.com/JeffreyWub/NPSDK/commit/c124b9d4549ea3f6ad87ef0c254d37dbe0fd4373)，分支 `codex/flow-runtime-state-20261028`。
- 雷赛初始化返回值与轴状态校验：[NPSDK 3b6dbedca23521b894802c0e4c6ebdc30589752a](https://github.com/JeffreyWub/NPSDK/commit/3b6dbedca23521b894802c0e4c6ebdc30589752a)，分支 `codex/flow-runtime-state-20261028`。
- 初始化故障节点恢复：[NPSDK 69783aa4ae3fe267ab9c4b4afeb64727c6864884](https://github.com/JeffreyWub/NPSDK/commit/69783aa4ae3fe267ab9c4b4afeb64727c6864884)，分支 `codex/flow-runtime-state-20261028`。
- 原生节点可关闭自动期限：[NPSDK 8ebbf7fd094fc679260efe6850d59e301ce35d60](https://github.com/JeffreyWub/NPSDK/commit/8ebbf7fd094fc679260efe6850d59e301ce35d60)，分支 `codex/flow-runtime-state-20261028`。
- 节点右上角执行耗时：[NPSDK 77830aadaa698818cbff58c16bfdb094dfcbfcc1](https://github.com/JeffreyWub/NPSDK/commit/77830aadaa698818cbff58c16bfdb094dfcbfcc1)，分支 `codex/flow-runtime-state-20261028`。
- 暂停归零与旧刷新保护：[NPSDK 20c39268e49a81493237497cf8ae4740e9202470](https://github.com/JeffreyWub/NPSDK/commit/20c39268e49a81493237497cf8ae4740e9202470)，分支 `codex/flow-runtime-state-20261028`。
- 构建：Debug / x64；SDK 为 .NET Framework 4.7.1，宿主为 4.8。
- 大小：544256 字节。
- SHA256：`99f4028913950c420f10fc06b380915105087696bf996237c98c1cef122f6132`。

该版本从独立、干净的源码工作树构建，不包含原 SDK 开发目录中的未提交改动。

可选 `FlowStep.ControlOrigin`、`SourceLinks` 记录节点来源与源码四出口基准。宿主在加载时清理没有剩余引用的已删源码节点，合并源码连接变化，并保留拖拽节点及明确的混合流程连接。原 V1/V2/V3 文件继续兼容，配置仍统一由主 Save 保存。使用见 [源码节点同步](../docs/NativeFlowEvents.md#源码节点删除与连接同步)。

可选 `IFlowProgramStatus` 让 SDK、页面与主保存统一读取实际任务状态。完成的托管流程忽略旧运行标记，真实未返回的调用和旧工作器仍保持忙碌；旧控制器接口兼容。宿主待机模式下仍忙碌时显示三语“停止中”，保存提示包含忙碌工位。原生事件/状态 18/18、核心流程 13/13、IO 绑定回归通过；宿主流程 136/136、统一保存 17 项及源码生成回归通过。

`NP_Motor.Stop()` 同步取消绑定轴组合控件的手动回零/点动会话，与回零指令发送共用轴锁；停止后定时刷新不会再次启动回零。驱动的真实忙碌反馈继续保留在轴对象中。编辑模式使用独立提示。轴专项 5/5、宿主流程 138/138、统一保存 17 项、主界面 11/11 及原核心/IO 回归通过；旧 DLL 可复现停止后回零重启，未访问真实硬件。

雷赛 `dmc_board_init()` 返回 0 表示没有找到板卡或板卡异常，1–8 才是成功找到的板卡数；依据 [官方手册 9.1](https://kzcp.leisai.com/product%20literature/Data%20download/PC%20Base/DMC5000E%26DMC3000E/EtherCAT20220224.pdf)。失败时清除连接标记，轴数或参数配置失败不能发布就绪。未初始化轴不查询运动反馈，异常反馈也不能当作静止而授权定位或回零。显式调用 SDK 驱动初始化只连接和配置对应资源，不自动发出运动。生产驱动源码与记录型原生替身回归 6/6，旧源码可复现无板卡假成功；初始化未发出任何运动命令。宿主 138/138、主保存 17 项、轴组 5/5、原生流程 18/18、核心 13/13 和 IO 绑定回归通过。

主界面【初始化／启动】只触发对应的已保存流程，不遍历组合硬件，不自动连接，也不检测就绪或分析资源依赖。连接和硬件状态处理由流程节点代码负责；资源配置仍由主 Save 保存。原全量启动检查与自动初始化方法已移除。真实主入口在无关硬件未就绪的离线场景完成初始化及自动流程，未读轴忙状态；宿主流程 139/139、主保存 17 项及源码生成回归通过。

正式初始化启用 `FlowProgramRuntime.PauseOnFailure`：异常或超时保留当前节点，底层调用退出后才允许重试；普通报警暂停不消耗初始化期限，也不重发已完成动作。处理原因并清除报警后，再点原【初始化】继续，保留已完成工位、上游结果和参数快照。【停止】结束会话，下次初始化从入口开始；主【启动】仍只用于自动生产，原生产失败语义不变。恢复位置不持久化，不增加配置项。SDK 原生事件 23/23、核心 13/13；宿主流程 142/142、主保存 17 项、主界面 11/11。实际初始化按钮覆盖普通报警、异常、超时、完成工位不重跑和三语状态切换，主保存与配方保护沿用现有机制。验证均在离线环境完成，未操作现场硬件。

源码代码节点按原 FlowChat 的 `AlarmStaus`、`AlarmTimeOut`、`AlarmCode` 运行，加载旧工程时修正误带入的 10000ms／空编号，保留位置、身份及连接。关闭自动报警的原生节点使用 `TimeoutMs = -1`，仍可暂停、停止和报告业务异常；普通功能及条件节点仍要求正超时。新拖入的纯代码函数不默认附加 10 秒期限，已有明确保存的拖拽代码设置保留。SDK 原生事件 24/24、核心 13/13；宿主流程 147/147、主保存 17 项及源码生成回归通过，含实际下流道节点只读绑定核对，未调用现场 IO。

节点右上角显示执行秒数，与原生 `FlowChart_ElapsedMilliseconds` 共用计时器；每次暂停归零并停表，继续从零重新计时，等待超时也获得完整周期，完成节点保留数值。停止立即清空全部节点耗时、执行标记、调试快照和流程页错误提示，旧的异步状态消息不能重新填回已清空界面；实际调用退出前仍保留运行占用。耗时不写入配置。流程页停止统一到整机停止，撤销初始化续跑位置，下次初始化从入口执行。主页为蓝色导航和重点卡片，报警筛选条对齐；新 VPP 默认 X/Y/R，兼容旧角度键，表格紧跟标题。暂停与清空专项：SDK 原生 26/26、核心 13/13，宿主流程 149/149、主保存 17 项及源码生成回归通过，包含三语清空检查；均为离线验证。

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
- SDK 创建和设计预览不连接硬件；资源连接由流程节点自己的代码负责。

## 验证与构建

SDK 的 IO 绑定回归覆盖实际实例所有权、方向匹配、重复绑定、替换释放、卡片与指示图单次点击、权限和资源租约；原核心 13 组运行回归通过。宿主的构建、流程、主保存、语言及设计器验证随同 AlphaRap 提交说明交付。现场硬件和 Visual Studio 交互式设计器重开仍需现场验证。

原生事件 15 组回归覆盖事件快照、XML身份、全部出口、单一期限、暂停、取消与超时等待在途调用、断点单步、异常报警及不修改业务名称的持久类型标记；原核心及 IO 回归同时通过。

源码来源元数据扩展后，SDK 原生事件与 XML 兼容回归为 16/16、核心流程为 13/13，IO 绑定回归通过；宿主流程 133/133、统一保存 17 项及源码生成回归通过。实际 UpConveryor 旧配置的只读验证确认重复节点清理及 NEXT 修复，未调用真实设备。

安装 .NET Framework 4.8 开发工具、Cognex VisionPro SDK，并还原 packages.config 后，使用 Visual Studio 或 MSBuild 构建 AlphaRap.sln 的 Debug / x64。

```powershell
MSBuild.exe AlphaRap.sln /t:Rebuild /p:Configuration=Debug /p:Platform=x64
./tests/FlowProgramming/Run.ps1 -AssemblyDirectory AlphaRap/bin/x64/Debug
./tests/VisionRuntime/Run.ps1 -AssemblyDirectory AlphaRap/bin/x64/Debug
./tests/DesignerCompatibility/Run.ps1 -AssemblyDirectory AlphaRap/bin/x64/Debug
```

升级核心时同步更新来源、校验值和验证结果，避免直接引用其他工作区的临时 DLL。
