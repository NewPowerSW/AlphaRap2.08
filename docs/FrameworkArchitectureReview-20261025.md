# AlphaRap / NPSDK 框架评估与优化顺序

评估日期：2026-10-25。宿主基线：`18a6616`；NPSDK 基线：`c655605`。

本次是只读源码评估，结合已交付工具箱、代码模块及前一轮验证结果。没有修改生产代码或 DLL，没有连接硬件、测量现场节拍，亦未在本轮重新运行全部测试。工作区原有未提交修改不属于本报告交付。

## 1. 总体判断

这套框架已经具有设备应用平台的基础：真实设备对象、流程执行、视觉、配方、语言、本地保存和扩展模块已经能够协同。新工具箱允许开发人员封装一次业务能力，应用工程师重复绑定现场对象。这条路线符合目标，可以继续发展。

目前还不能把它视为已经完成的低代码生产平台。主要欠缺是：**应用工程师需要理解的概念仍然偏多；产品在设备中的身份、位置和工序事实尚未形成持久模型；新运行器与旧整机生命周期之间仍有边界差异。**

下一步应围绕一个真实设备闭环验收，而非继续增加通用下拉框或新的执行方式。点胶转盘就是合适的样板：多件同时在制、转位、重拍、停机恢复、装盘、本地存档及 MES 回执，能够暴露普通演示流程看不到的问题。

## 2. 应当保留的优点

### 2.1 符合自动化工程的现有资产

轴与点位、输入输出、气缸、Device、VPP、报警、配方和数据保存均有现成基础。绑定的是轴页面、IO 页面和视觉页面中的实际对象，不需要为流程重新创建第二套设备。动态组合对象和外部登记实例已经具备稳定身份及使用保护。

价值：新增同类硬件或 VPP 实例时，可以重用已有动作和模块；新协议、新工艺才需要开发人员补一次能力。

证据：[资源登记与绑定校验](https://github.com/NewPowerSW/AlphaRap2.08/blob/18a6616e008b1775e8dc04f15670db6ac592b737/AlphaRap/FlowProgramming/FlowResourceAccess.cs#L26)、[资源目录](https://github.com/NewPowerSW/AlphaRap2.08/blob/18a6616e008b1775e8dc04f15670db6ac592b737/AlphaRap/FlowProgramming/FlowResources.cs#L89)。

### 2.2 普通动作、脚本、代码模块已经共用运行器

配置动作、C# 脚本和继承 `FlowCodeNode<TInputs,TOutputs>` 的业务模块，可以在同一工程内运行，沿用节点出口、报警、暂停和调试入口。代码模块具有类型化输入输出、独立执行实例和一次进入逻辑；代码自行决定 NEXT/CASE 时，界面只配置出口去向。

价值：常规任务交给拖拽配置，复杂工艺交给可复用模块，工程师不用在每个节点重复写相同脚本。

证据：[统一启动与运行快照](https://github.com/NewPowerSW/AlphaRap2.08/blob/18a6616e008b1775e8dc04f15670db6ac592b737/AlphaRap/FlowProgramming/FlowPrograms.cs#L718)、[SDK 执行边界](https://github.com/JeffreyWub/NPSDK/blob/c655605da5b8fbcee5a20a71903d07fbfaf147ea/NPSDK/FlowChart/FlowProgramRuntime.cs#L348)。

### 2.3 已有必要的运行约束

实际对象调用共用资源互斥，取消会等待实际调用收拢；运行计划与编辑草稿隔离，结果按运行、周期和节点区分。代码模块未等待的内部调用也会收拢，避免把迟到输出当作已完成结果。以上是已实现能力。

边界：单次资源调用的互斥，不等于整个多轴、多执行器工序天然具有原子性；整机协调仍需明确协议。现有断点主要是节点边界调试，不能等同于内置完整的 C# 逐行调试器。

证据：[实际资源调用](https://github.com/NewPowerSW/AlphaRap2.08/blob/18a6616e008b1775e8dc04f15670db6ac592b737/AlphaRap/FlowProgramming/FlowActions.cs#L212)、[资源租约](https://github.com/JeffreyWub/NPSDK/blob/c655605da5b8fbcee5a20a71903d07fbfaf147ea/NPSDK/FlowChart/FlowResourceGate.cs#L32)。

### 2.4 配置保存、本地记录和多语言已经有共同规则

MainForm Save 使用同一流程快照写 XML、生成定义源码并发布；异常回滚及生成文件所有权校验已经存在。Setting 与 Recipe 作用域有区分，沿用 ProductManagerForm 配方管理及中、英、西三语。DataForm 已支持类型化生产字段、本地 CSV 和日文件内 RecordId 去重。

价值：后续新增功能可以沿用现有规则，避免页面各自保存、各自持有另一份配方。

证据：[主保存事务](https://github.com/NewPowerSW/AlphaRap2.08/blob/18a6616e008b1775e8dc04f15670db6ac592b737/AlphaRap/Configuration/ProjectConfiguration.cs#L37)、[生产保存](https://github.com/NewPowerSW/AlphaRap2.08/blob/18a6616e008b1775e8dc04f15670db6ac592b737/AlphaRap/FlowProgramming/FlowProductionData.cs#L137)。

## 3. 优化优先级

P0 表示相关设备正式投产前应先闭合的行为；P1 是下一轮平台完善重点；P2 在闭环稳定、具备测量证据后推进。优先级考虑本项目的点胶转盘场景，不代表所有问题已经在现场发生。

### P0-1：统一点位引用的保护

现状：普通运动按绑定表的数字索引取坐标，校验索引存在、限位和误差；代码模块还额外校验已确认的点位表签名。两类节点的保护不一致。

可静态推导的风险：原先三个点，节点引用索引 1；删掉首行后索引 1 仍有效，却可能指向原来的第三点。数值合法不能证明仍是原来的业务点位。

建议：先统一普通动作、脚本调用和代码模块的结构变更检查；最终使用稳定 PointId，行号只负责显示或兼容旧数据。正常修改坐标按配方规则生效，删除、重排、替换点位要检查全部引用。

验收：删除首行、上移点位、切换配方后，三种调用方式都不能静默改指向；失效引用能明确定位到节点、轴和原点位。

证据：[普通运动取点与校验](https://github.com/NewPowerSW/AlphaRap2.08/blob/18a6616e008b1775e8dc04f15670db6ac592b737/AlphaRap/FlowProgramming/FlowAxisObject.cs#L62)、[代码模块签名检查](https://github.com/NewPowerSW/AlphaRap2.08/blob/18a6616e008b1775e8dc04f15670db6ac592b737/AlphaRap/FlowProgramming/FlowResourceAccess.cs#L153)。本轮未做硬件复现。

### P0-2：统一停止、暂停和退出的实际状态

现状：新运行器保留“调用实际结束后才停止”的边界；旧 `StopRun` 先把全局模式设为 IDLE，再请求停止。退出已检查新流程是否在运行，但后续仍存在 `Process.Kill()`；手动任务取消也缺少共同的等待收尾过程。

建议：整机统一为“请求停止 → 停止中 → 调用收拢 → 已停止 → 释放资源 → 退出”。保留设备驱动无法退出时的受控兜底，先补完整收尾路径。节点暂停要明确显示“等待当前动作结束”；分别规定动作和条件等待的超时是否在暂停期间计时。

验收：模拟驱动延迟返回时仍显示停止中；收尾前不能重新启动或接管资源；恢复不会重发已完成点胶。正常退出应等待生产记录和待传任务落地。此处是生命周期风险，不能据此断言目前必然丢数据。

证据：[整机停止](https://github.com/NewPowerSW/AlphaRap2.08/blob/18a6616e008b1775e8dc04f15670db6ac592b737/AlphaRap/Core/MiddleLayer.RunControl.cs#L103)、[退出路径](https://github.com/NewPowerSW/AlphaRap2.08/blob/18a6616e008b1775e8dc04f15670db6ac592b737/AlphaRap/MainForm.cs#L1681)、[节点边界暂停](https://github.com/JeffreyWub/NPSDK/blob/c655605da5b8fbcee5a20a71903d07fbfaf147ea/NPSDK/FlowChart/FlowProgramRuntime.cs#L237)。

### P0-3：建立产品随行记录，独立保存加工事实

现状：结果 Hub 是内存中的节点数据通道；宿主启动周期默认是 `batch`。已有跨节点、跨模块结果引用，但尚无持久产品台账、治具穴位占用和转盘相位管理。普通动作返回后还要等出口条件成立才发布结果；这与“点胶已经发生”的工序事实不是同一个提交点。

建议：增加一个持久产品记录：ProductId、条码、配方内容版本、治具/穴位及占用代次、当前工序、测量值、加工状态、装盘信息。工位进入时获得固定产品句柄，迟到回执仍指向原产品。转位确认成功后才更新位置，失败则标记待确认。点胶等不可简单重复的动作记录意图、实际完成/失败/未知及稳定 OperationId。

节点临时计算仍使用现有结果通道；跨工位、停机恢复及最终追溯使用产品记录。产品状态持久化应由生产动作自动完成；用户编辑的配置继续只由 Main Save 保存。

验收：多件不同条码同时生产不串数据；转位失败不推进映射；换件后的旧 AOI 回执不写入新产品；点胶完成但出口条件未通过就强退，重启后明确提示核对，不能默认再次点胶。

证据：[宿主周期默认值](https://github.com/NewPowerSW/AlphaRap2.08/blob/18a6616e008b1775e8dc04f15670db6ac592b737/AlphaRap/FlowProgramming/FlowPrograms.cs#L740)、[内存结果与周期](https://github.com/JeffreyWub/NPSDK/blob/c655605da5b8fbcee5a20a71903d07fbfaf147ea/NPSDK/FlowChart/FlowProgramRuntime.cs#L54)、[动作完成与出口](https://github.com/JeffreyWub/NPSDK/blob/c655605da5b8fbcee5a20a71903d07fbfaf147ea/NPSDK/FlowChart/FlowProgramRuntime.cs#L378)。[转盘追溯文档](RotaryProductTraceability.md) 仍是设计提案，不是已实现功能。

### P1-1：把工程师的常规操作缩成四件事

建议常规界面只表达：**做什么 → 用哪个实际对象 → 参数/数据从哪里来 → 完成后去哪。** 例如“X 轴移动到取料位”，主要选择应是轴和该轴的点位；偏移、容差进入高级选项。拖入明确动作后，不再要求用户重复理解类别、对象方法、执行方式。

工具箱先按业务能力组织，再选择实例，避免随轴数、IO 数和方法数相乘膨胀。脚本、开发模块类型和兼容模式保留在开发入口。节点摘要显示实际轴/点位、VPP/方法、参数来源和结果去向；保存失败定位到具体问题。

同时明确：当前 `.Flow.g.cs` 生成的是流程定义；XML 是当前设计模型的加载来源。业务实现位于动作或代码模块。手改生成文件会触发所有权冲突，Main Save 也不会自动编译/替换 EXE。界面应区分“草稿、已保存、运行版本、需重新编译”。

验收：工程师完成扫码、移动、视觉判定和本地保存时，无需阅读 C#、资源路径或端口反射规则。高级功能可发现但不挡住常规配置。

证据：[工具箱条目展开](https://github.com/NewPowerSW/AlphaRap2.08/blob/18a6616e008b1775e8dc04f15670db6ac592b737/AlphaRap/FlowProgramming/FlowToolboxControl.cs#L53)、[生成流程定义](https://github.com/NewPowerSW/AlphaRap2.08/blob/18a6616e008b1775e8dc04f15670db6ac592b737/AlphaRap/FlowProgramming/FlowSourceGenerator.Render.cs#L34)。

### P1-2：补业务子流程、同产品重试和跨模块交接

现状：运行器主要是单游标顺序执行，多模块可以并行；跨模块结果需要一致的产品周期键。普通图重复经过节点受循环检查约束，目前容易把工艺重试与开始产品周期混为一谈。

建议先提供“调用子流程、等待模块完成、有限次数重试”三类明确业务能力，再考虑任意并行图。重拍属于同一产品、同一工序的新尝试；新产品建档才创建新身份。模块交接携带产品句柄，取消/失败明确传给等待者。

验收：同一产品重拍三次仍是同一产品，测量尝试可以追溯；两个产品跨模块并行不串结果；上游取消后下游有明确失败原因；结束产品后临时结果被释放。

证据：[循环检查](https://github.com/JeffreyWub/NPSDK/blob/c655605da5b8fbcee5a20a71903d07fbfaf147ea/NPSDK/FlowChart/FlowProgramRuntime.cs#L280)、[周期开始与释放](https://github.com/JeffreyWub/NPSDK/blob/c655605da5b8fbcee5a20a71903d07fbfaf147ea/NPSDK/FlowChart/FlowProgramRuntime.cs#L110)。宿主 `FlowUtilityObject` 中“开始周期会释放上一周期结果”的帮助与当前实现也应同步修正。

### P1-3：最终记录与 MES 上传形成可恢复闭环

现状：DataForm 已能存本地 CSV，客户 MES 也已有接口；但日志不等于待传任务。默认保存 RecordId 来自运行/模块/调用，日文件去重不是跨重启、跨日期的业务幂等。相同 ID 不同内容也需要冲突检查。

建议先持久提交产品最终记录和不可变 MES 请求，再异步上传。任务包含固定 MessageId、业务身份、请求内容、重试次数、响应和待确认状态。CSV 保留为现场查询/导出；追溯主记录使用具备事务能力的本地存储。MES 超时可能是“对方已接收、我方未收到回执”，应按对方幂等或查询能力核对，不能仅靠重试次数保证不重复。

已有适配器也需逐项校验：例如 Amphenol 在发送前清空测试项，LENS 的 `ReturnValue` 用文本包含 `true` 判断。先保留请求快照、解析明确的业务响应字段，再改变任务状态。

验收：断网、响应丢失、程序重启后数据和待传任务仍在；跨午夜重试不生成第二份业务记录；同 ID 不同内容报冲突；假成功响应不被接受。

证据：[默认记录身份](https://github.com/NewPowerSW/AlphaRap2.08/blob/18a6616e008b1775e8dc04f15670db6ac592b737/AlphaRap/FlowProgramming/FlowDataMethods.cs#L19)、[CSV 去重边界](https://github.com/NewPowerSW/AlphaRap2.08/blob/18a6616e008b1775e8dc04f15670db6ac592b737/AlphaRap/Classes/ProductionDataStore.cs#L209)、[Amphenol 发送](https://github.com/NewPowerSW/AlphaRap2.08/blob/18a6616e008b1775e8dc04f15670db6ac592b737/AlphaRap/MES/Amphenol.cs#L329)、[LENS 响应辅助](https://github.com/NewPowerSW/AlphaRap2.08/blob/18a6616e008b1775e8dc04f15670db6ac592b737/AlphaRap/MES/LENS.cs#L204)。

### P1-4：配方切换与配置保存补齐故障恢复

现状：已有运行中禁止切方、视觉忙与草稿保护；但加载时先修改全局配方再逐模块读取，异常路径改用默认名称，未完整恢复已加载状态。主 Save 的多文件异常回滚已实现，仍依赖进程执行 catch；不能据此保证保存中掉电后的整组一致性。

建议：配方全部预读校验，通过后整体接受，失败保留完整旧状态；产品记录捕获配方内容版本。有在制品时默认禁止切换生产配方。主 Save 增加持久事务清单、文件摘要和提交代次，启动时核对并恢复未完成提交。

验收：任一模块/VPP 文件损坏时，切换失败后名称、点位、参数和视觉全部仍是旧配方；在任一文件替换后结束进程，重启只能加载完整旧版本、完整新版本或进入明确的修复状态。

证据：[配方加载与异常路径](https://github.com/NewPowerSW/AlphaRap2.08/blob/18a6616e008b1775e8dc04f15670db6ac592b737/AlphaRap/Core/MiddleLayer.Recipe.cs#L43)、[多文件提交](https://github.com/NewPowerSW/AlphaRap2.08/blob/18a6616e008b1775e8dc04f15670db6ac592b737/AlphaRap/Configuration/ProjectConfiguration.cs#L95)。掉电混合状态是静态风险，本轮未注入验证。

### P2-1：逐步减少业务对窗体的依赖，统一资源身份

现状：SDK 运行器本身已可离线测试；宿主目录仍扫描 MiddleLayer、窗体树和字段，DataForm 同时承担界面及保存能力。旧控件身份依赖父路径/Name，新动态资源则有稳定 ID。分文件提高可读性，但不等于已解除耦合。

建议：按能力逐项提取现有服务，先从本地生产保存、产品台账、资源登记开始。窗体负责编辑和显示同一服务，运行器通过能力接口访问。为旧路径建立兼容别名，新选择器显示唯一规范身份。先给资源声明用途、参数类型、单位、返回值、模拟和停止行为，避免把所有 public 方法直接暴露给工程师。

验收：移动控件容器、改显示名称后绑定仍有效；同一实例只出现一次；不创建 MainForm 也能完成一个模拟工艺与保存链路。NPSDK 保留通用契约和运行能力，宿主负责具体对象、协议和项目集成。

证据：[宿主资源捕获](https://github.com/NewPowerSW/AlphaRap2.08/blob/18a6616e008b1775e8dc04f15670db6ac592b737/AlphaRap/FlowProgramming/FlowResources.cs#L89)、[旧控件身份](https://github.com/NewPowerSW/AlphaRap2.08/blob/18a6616e008b1775e8dc04f15670db6ac592b737/AlphaRap/FlowProgramming/FlowResources.cs#L195)、[保存能力所在窗体](https://github.com/NewPowerSW/AlphaRap2.08/blob/18a6616e008b1775e8dc04f15670db6ac592b737/AlphaRap/FlowProgramming/FlowDataMethods.cs#L10)。

### P2-2：先能定位故障，再按测量优化性能

建议每次节点执行留下有界轨迹：运行/产品/节点/操作身份、配方与模块版本、资源等待、动作耗时、条件等待、出口、异常原因。现场能回答“为什么没往下走”，再优化节拍。

性能方向：在运行计划发布时缓存静态元数据、端口解析和校验结果；运行时保留实际值、取消、限位和资源状态检查。生产查询按日期/分页读取；避免扩展字段时频繁重写大型日文件。IO 扫描记录耗时分布和超周期次数，慢协议操作与扫描/界面刷新分离。

验收：一次超时能定位到资源、动作或条件；用同一离线工况比较耗时、分配与长时间内存趋势。以上是待测优化方向，本轮没有证据给出性能提升百分比。

证据：[重复输入解析与检查](https://github.com/NewPowerSW/AlphaRap2.08/blob/18a6616e008b1775e8dc04f15670db6ac592b737/AlphaRap/FlowProgramming/FlowActions.cs#L157)、[扫描循环](https://github.com/NewPowerSW/AlphaRap2.08/blob/18a6616e008b1775e8dc04f15670db6ac592b737/AlphaRap/Controls/FlowControl.cs#L1)、[存储查询及扩展表头](https://github.com/NewPowerSW/AlphaRap2.08/blob/18a6616e008b1775e8dc04f15670db6ac592b737/AlphaRap/Classes/ProductionDataStore.cs#L255)。

## 4. 推荐推进顺序

1. **先补共同运行规则。** 统一点位引用、停止/退出及配方故障处理，建立可离线注入故障的验收场景。
2. **再完成点胶转盘纵向样板。** 产品建档 → 扫码 → 点胶/称重 → AOI → 已确认转位 → 装盘 → 最终记录 → MES 回执。把产品身份、重试、停机恢复和数据闭环做实。
3. **用样板反推简单界面。** 将重复部分封装成工具箱业务模块或子流程，删除重复选择，收起开发细节；以首次使用的应用工程师能否独立完成任务验收。最后依据运行证据调整性能和部署管理。

每一步继续沿用三语、ProductManagerForm、Setting/Recipe 作用域和 MainForm Save。配置保存与生产自动落盘各有明确职责。现有工具箱、代码模块、真实对象绑定及本地日志均应保留并收敛，避免另起第二套框架。

## 5. 验证边界

前一轮已完成工具箱、脚本/运行器、视觉/语言、设计器兼容探针、主保存和源码生成的离线验证，并更新原 Debug。这里引用的是交付基线的既有结果；它们不等于真实硬件验收，也不等于完整交互式 Visual Studio 设计器验证。

本次新增的只有评估文档与可筛选优先级的本地视图。结论以源码和已有验证为依据，明确区分已实现能力、静态风险和建议新增能力；没有实施本报告建议的生产变更。
