# AlphaRap / NPSDK 分级更新建议

审查日期：2026-10-22。建议以实际运行 DLL 为依据，先处理运行与设备语义，再处理可靠性与性能，最后收拢界面和架构。此次仅增加规则与审查文档，未修复下列业务问题。

## 审查范围与证据边界

- 检查 AlphaRap 源码结构、启动退出、MiddleLayer、流程、设备/PLC、视觉、报警、数据、用户和语言切换的关键调用关系；源码清单含 144 个 C# 文件（含设计器）。
- 找到 SDK 源码目录：`E:/测试程序/NPSDK/NPSDK/NPSDK`。对实际引用的 `AlphaRap/bin/x64/Debug/NPSDK.dll` 全量反编译，清点 176 个顶层类型，按模块审查托管实现及原生接口声明。完整类型清单见 [NpsdkReviewCoverage.json](NpsdkReviewCoverage.json)。
- 实际 DLL：427520 字节，SHA-256 `23ee59b1d45534e348ef897db5cdd06aff83ac98a3a982593e16178e9f2c0609`。SDK 源码目录现成构建产物为 428544 字节，哈希 `8a45e0cd2acd63b57186cf0714b1eac3480de00a0e304fd4083ad79bf596ddec`。两者不同，源码只能辅助对照，核心结论已回看实际 DLL。
- 原生声明包含 APS168.dll 505 项、APS168x64.dll 505 项、LTDMC.dll 762 项、kernel32.dll 2 项。这不代表已阅读厂商原生 DLL 内部实现，亦不代表所有硬件分支已实机运行。
- 本轮 11 项隔离观察使用临时文件、托管编码方法、替身驱动和空设备列表，没有启动真实生产流程或调用真实运动驱动。证据标注区分“已复现”“静态确认”“需实机确认”。

## 已有基础应保留

视觉已具备 RunId 与结果快照、超时晚到隔离、单工位运行门控、稳定 VppId、标定指纹以及 TB 原子保存等机制；MiddleLayer 已按功能拆分，语言查找已使用索引。现有回归基线为 62/62 通过，不能据此推导全部设备行为已验证。补偿继续由应用者在流程内主动使用，框架只提供表获取接口。

## 第一优先级 P1：先修复运行正确性

共 6 项。建议在新增自动运行功能前完成，并分别提交和验证。

### P1-01 让停止指令真正结束本轮运行，并统一退出流程

证据级别：已复现＋静态确认。

**现状与影响：** Flow_Module 的启动后台任务等待 100ms 后仍直接 StartRun，没有再次校验停止或运行代次。已复现 Stop 后仍出现 starts=1、RunningStatus=true、StopStatus=true。Flow_Chart 调用 CancelAsync，但执行循环不检查 CancellationPending；长回调也不响应取消。MainForm 正常关窗仍直接 Kill 当前进程，绕过 Application.Run 之后的 DisposeProject。

**建议：** 在 NPSDK 内串行处理 Start/Pause/Resume/Stop；每轮分配运行代次，等待后和回调派发前检查取消与代次。阻塞设备操作增加超时和取消。关窗依次请求停止、等待工作线程、刷新日志、释放驱动和相机；超时应显示失败状态。软件停止与设备硬件急停分别验收。

**验收：** 反复 Start→立即 Stop，Stop 完成后没有新回调；暂停/停止竞态不会自恢复。关闭应用后无残留工作线程，日志落盘，清理只执行一次。

**定位：** `NPSDK.Flow_Module.Module_StartRunWork / Module_StopRun`；`NPSDK.Flow_Chart.Do_work / FlowChart_Run`；[AlphaRap/MainForm.cs:1676](../AlphaRap/MainForm.cs#L1676)；[AlphaRap/Program.cs:41](../AlphaRap/Program.cs#L41)。

### P1-02 把仿真落实到驱动边界

证据级别：已复现。

**现状与影响：** SDKKernal.SetSimulation 只传播标志；NP_Motor.Goto、NP_Output.On 仍派发驱动命令。替身驱动测试中 Simulation=true，电机和输出驱动调用均为 1 次。上层某些检查跳过硬件不等于整个 SDK 已隔离硬件。

**建议：** 在硬件创建入口选择真实驱动或仿真驱动，仿真状态、位置、完成和错误由替身实现；仿真路径不得初始化真实板卡或建立真实控制连接。已初始化后的模式切换只允许在停止并释放资源后进行。

**验收：** 仿真启动、手动、回零、自动流程和退出全过程，真实驱动工厂及原生调用计数均为 0；模拟断线、超时和限位能进入同一错误流程。

**定位：** `NPSDK.SDKKernal.SetSimulation`；`NPSDK.ControlBaseInterface.SetSimulation`；`NPSDK.Motion.NP_Motor.Goto`；`NPSDK.Motion.NP_Output.On`。

### P1-03 明确 PLC 数值类型、地址和完成握手

证据级别：已复现＋静态确认，寄存器约定需实机确认。

**现状与影响：** Gantry 将 X/Y/Angle.ToString() 交给默认 int 的 WritePlc。0.125 在托管编码器中抛 FormatException；部分越界输入返回错误和零缓冲区，但调用方忽略编码错误。ReadPlc("6804",2) 缺少地址区前缀，会落入解析默认分支。

**建议：** 先按 PLC 寄存器表确认 Float32/Int32/缩放整数、字序及地址；随后提供少量强类型读写方法，禁止靠字符串默认类型猜测。编码失败直接返回，验证每次写入成功后才置完成位。运行界面文化与协议编码分开，避免逗号小数点被当成多值分隔符。

**验收：** 负数、小数、边界值和三种语言环境生成相同协议字节；任一结果写入失败都不置完成位；地址在离线编码测试和 PLC 联调中一致。

**定位：** [AlphaRap/TaskProcess/Gantry.cs:189](../AlphaRap/TaskProcess/Gantry.cs#L189)；[AlphaRap/TaskProcess/Gantry.cs:228](../AlphaRap/TaskProcess/Gantry.cs#L228)；[AlphaRap/PLC/INOVANCE.cs:285](../AlphaRap/PLC/INOVANCE.cs#L285)；[AlphaRap/PLC/INOVANCE.cs:338](../AlphaRap/PLC/INOVANCE.cs#L338)。

### P1-04 切换语言不能清空活动报警

证据级别：已复现＋静态确认。

**现状与影响：** SwitchLanguage→LoadAlarmTable→Alarm.Initial 会重建 AlarmList。已复现活动报警 1→0，而 IsError 仍为 true。SDK 对未定义报警码静默返回；语言表还存在重复和缺失代码及 DoStop 不一致。历史记录与运行时停机属性不一致也已复现。

**建议：** 把报警定义目录与活动报警实例分开；语言切换仅重新解释显示文字，保留代码、时间、确认和恢复状态。DoStop 等行为属性以唯一主定义为准，各语言仅提供文本。启动时校验重复代码、调用方引用及语言完整性；未知码也应记录并显示诊断。

**验收：** 带活动报警连续切换中英西文，数量、时间及停机行为不变；确认/恢复流程完整。重复或缺失定义在发布检查中失败，历史记录保留事件发生时的行为属性。

**定位：** [AlphaRap/Core/MiddleLayer.Language.cs:177](../AlphaRap/Core/MiddleLayer.Language.cs#L177)；[AlphaRap/Classes/AlarmCatalog.cs](../AlphaRap/Classes/AlarmCatalog.cs)；[AlphaRap/Classes/AlarmHistory.cs](../AlphaRap/Classes/AlarmHistory.cs)；`NPSDK.Alarm.Initial / Show`。

### P1-05 区分设备值、通信失败和命令成功

证据级别：静态确认。

**现状与影响：** Keyence 读失败会成为 false，NP_Input.ReverseState 又能将其变为 true。NP_Motor 未就绪时的位置/忙状态返回 0/false。Adlink 输出执行后固定返回 false，Leadshine 某些输出固定返回 true，NP_Output 又丢弃返回值。业务难以区别 OFF、断线、未完成与失败。

**建议：** 驱动读取返回值、质量、采样时间和错误；反向逻辑只作用于有效值。命令返回明确的已接受/完成/失败状态及原始错误码，上层对关键输入配置断线策略。按供应商手册解释原生返回码，不统一猜测。

**验收：** 断线、超时、未初始化和驱动拒绝都不能伪装成正常 OFF、零位或成功；每种厂商驱动有一致的契约测试，关键输入故障会阻止依赖动作。

**定位：** `NPSDK.KeyenceHostLink.Read / Write`；`NPSDK.Motion.NP_Input.IsOn`；`NPSDK.AdlinkNP_Output.On / Off`；`NPSDK.LeadshineNP_Output.On / Off`；`NPSDK.Motion.NP_Motor.GetPos / GetBusy`。

### P1-06 配置、配方和设备就绪必须成为启动门槛

证据级别：静态确认。

**现状与影响：** MiddleLayer.Startup 忽略 OpenRecipe 返回值，随后启动扫描或 ServoOn；模块配方逐个修改，失败后没有整体回滚。NSDK 初始化未汇总每个控件的成功状态。MotorParaForm.Save 先 ApplyDataToMotor，再写文件，保存失败时内存/驱动参数可能已改变。

**建议：** 平铺准备→校验→提交步骤，汇总设备、轴参数、配方、VPP 和标定就绪结果；失败时阻止自动运行及不应执行的使能动作。参数先校验并原子持久化，在空闲和权限满足时应用；应用失败保留旧快照并报告。

**验收：** 损坏 XML、缺失 VPP、任一模块加载失败、目录只读或设备未就绪时均有明确失败结果；不会出现界面显示旧配方而部分模块已切换，重启前后参数一致。

**定位：** [AlphaRap/Core/MiddleLayer.Startup.cs:38](../AlphaRap/Core/MiddleLayer.Startup.cs#L38)；[AlphaRap/Core/MiddleLayer.Recipe.cs](../AlphaRap/Core/MiddleLayer.Recipe.cs)；`NPSDK.NSDK.InitialControls`；`NPSDK.Motion.MotorParaForm.Save / TryLoadFile`。

## 第二优先级 P2：提高长期可靠性和可维护性

共 8 项。按设备生命周期、采样和通信、追溯数据、发布验证顺序推进。

### P2-01 明确板卡、连接、控件和定时器的所有权

证据级别：已复现＋静态确认。

**现状与影响：** ControlBaseInterface 构造时注册到全局列表，Dispose 不注销；已复现已释放控件仍留在注册表。部分 Adlink 单点驱动 Dispose 会停止总线并 APS_close，影响共享板卡。若干控件自行创建的 Timer 未纳入统一释放。

**建议：** 板卡及连接由唯一设备会话管理，控件仅注册/注销自身。释放一个点位不得关闭其他点位共用的板卡；退出统一关闭会话。为注册表、事件订阅和定时器建立成对生命周期。

**验收：** 反复打开/关闭手动和设备页面后，注册数量、定时器数量和连接数回到基线；释放一个控件不影响同卡其他轴/IO；整机退出只关闭一次。

**定位：** `NPSDK.ControlBaseInterface.Dispose`；`NPSDK.SDKPara.ControlList`；`NPSDK.AdlinkNP_Input.Dispose`；`NPSDK.AdlinkNP_Output.Dispose`；`NPSDK.Motion.NP_PositionShow / NP_Home`。

### P2-02 让设备采样与界面刷新各有节奏

证据级别：静态确认，性能收益需测量。

**现状与影响：** SDK 刷新 IO 时逐点执行通信并更新界面，慢连接可阻塞整轮扫描；AlarmRunTask 后台线程直接操作 UI，SDK 还全局关闭跨线程检查。CheckMotorProtected 的 motor.Refresh() 实际调用 WinForms 重绘，并非 SDK 的 RefreshControl。

**建议：** 按设备连接批量采样成带时间的状态快照；关键检查消费快照并检查过期，界面通过 UI 线程合并刷新。去掉业务扫描中的重绘，逐步恢复跨线程检查。记录扫描周期、最慢驱动和 UI 等待时间，再决定缓存或分配优化。

**验收：** 注入单个慢 PLC 后，其余设备状态和关键检查的延迟有明确上限；后台不直接写控件，长期运行无跨线程异常。用 p95/p99 数据验证改进，不把 Sleep(5) 当成 5ms 实时保证。

**定位：** `NPSDK.SDKKernal.RefreshIO`；`NPSDK.Flow_BaseForm constructor`；[AlphaRap/Controls/FlowControl.cs](../AlphaRap/Controls/FlowControl.cs)；[AlphaRap/Tasks/AlarmRunTask.cs](../AlphaRap/Tasks/AlarmRunTask.cs)；[AlphaRap/Core/MiddleLayer.Motion.cs:73](../AlphaRap/Core/MiddleLayer.Motion.cs#L73)。

### P2-03 统一运动能力、单位和手动动作边界

证据级别：静态确认，轴映射和单位需实机确认。

**现状与影响：** Leadshine 从 Port 最后一位解析轴号并使用板号 0；速度比例在不同后端的解释不同，部分软限位基类实现为空。NP_Home 没有整体回零超时；持续点动主要依赖 MouseUp 停止。SDK 部分通用 PLC 写接口还残留 String 写 "abc"、Double 写 1.2f 的固定值。

**建议：** 将板号/轴号独立配置；统一速度比例、工程单位和最大速度约束。驱动明确声明软限位、耦合、触发等能力，不支持时返回失败。手动指令统一检查权限和机器状态，补充失去鼠标捕获、窗口停用及回零超时处理；修复或移除带固定值的旧接口。

**验收：** 多位轴号不冲突；同一参数在两种运动卡得到约定一致的命令；超速、不支持能力、窗口失活和回零失败均有可测结果。旧接口即便当前未调用也不能静默写入固定值。

**定位：** `NPSDK.LeadshineNP_Motor.Initial / ApplySpeed`；`NPSDK.AdlinkNP_Motor.JogP / JogN`；`NPSDK.MotorBaseInterface.SetSoftLimit`；`NPSDK.Motion.NP_Home / NP_JogOption`；`NPSDK.OmronPLC.WriteOmromPLC`。

### P2-04 修复生产记录、报警历史和日志的完整性

证据级别：已复现＋静态确认。

**现状与影响：** 生产查询按日期取文件但没有按记录时间精确过滤；带换行的 CSV 字段读取被截断；当天新增列会丢值。SDK TypeCollection1 的跨月查询只访问当前年月目录。日志队列没有完整退出冲刷机制，部分写盘失败被吞掉。

**建议：** 统一 CSV 记录级解析和 UTF-8 编码，显式管理列版本；按时间戳过滤，跨月枚举实际日期目录。日志使用单写入者、失败可见、可等待的 Flush/Stop，队列锁内不执行磁盘 IO；数据量足够大再评估索引存储。

**验收：** 09:00—11:00 查询只返回窗口内记录；逗号、引号、换行、中英西字符往返不变；新增列不静默丢失；跨月查询完整，断电/磁盘满策略明确，正常退出写完已确认接收的日志。

**定位：** [AlphaRap/Classes/ProductionDataStore.cs:149](../AlphaRap/Classes/ProductionDataStore.cs#L149)；[AlphaRap/Classes/ProductionDataStore.cs:205](../AlphaRap/Classes/ProductionDataStore.cs#L205)；[AlphaRap/Classes/LogManagement.cs](../AlphaRap/Classes/LogManagement.cs)；`NPSDK: DataSave.TypeCollection1.ReadDataFromToFile`；`NPSDK: DataSave.FileDataControl / CCSVObj`。

### P2-05 按协议帧处理通信，并消除重复请求

证据级别：静态确认。

**现状与影响：** ModBus_RTU.GetResultData 对同一结果调用两次 ReadOutputRegisters，读取依赖固定等待和一次 BytesToRead。TCP 客户端、扫码等路径多处使用单次 Read/Receive；Keyence Write 收到任意数据即认为成功。TCP 一次接收不保证得到一帧。

**建议：** 每个协议明确结束符/长度、最大帧、校验、截止时间和取消；累积直到完整响应，解析设备错误码。一次业务请求只读取一次并复用结果；重连用连接代次拒绝旧回调，按指令幂等性决定重试。

**验收：** 拆包、粘包、迟到响应、连接重建和设备错误回复均有离线测试；不会把前一次响应配给下一次请求；运动等非幂等操作不盲目重发。

**定位：** [AlphaRap/Devices/ModBus_RTU.cs:261](../AlphaRap/Devices/ModBus_RTU.cs#L261)；`NPSDK.KeyenceHostLink.Read / Write`；`NPSDK: TcpipClient`；[AlphaRap/Devices](../AlphaRap/Devices)。

### P2-06 统一用户数据访问与操作权限

证据级别：静态确认，未实施攻击测试。

**现状与影响：** 登录和用户增删改使用拼接 SQL，密码按明文比较；指纹回调按查询行下标关联用户，查询未保证稳定排序。删除或重排用户后，行号不应充当身份。

**建议：** 提供少量参数化 OleDb 用户方法，保持校验、查询、写入平铺可读；密码采用带盐哈希及兼容迁移。指纹模板绑定稳定用户 ID。关键手动、参数保存及配方切换在执行入口核对权限，不能只依赖按钮可见性。

**验收：** 用户名含单引号仍可正确处理；数据库不再保存可直接比较的明文密码；增删用户后模板仍匹配原用户 ID；越权调用执行方法也被拒绝。

**定位：** [AlphaRap/MenuForm/UserLoginForm.cs:182](../AlphaRap/MenuForm/UserLoginForm.cs#L182)；[AlphaRap/MenuForm/UserLoginForm.cs:283](../AlphaRap/MenuForm/UserLoginForm.cs#L283)；[AlphaRap/MenuForm/AddUserForm.cs](../AlphaRap/MenuForm/AddUserForm.cs)。

### P2-07 建立 NPSDK 与框架的可复现发布关系

证据级别：哈希差异已确认。

**现状与影响：** AlphaRap 当前引用的 NPSDK.dll 与 SDK 源码目录现成构建产物的 SHA-256 不同。仅修改源码或复制 DLL，不能证明运行的 EXE 使用了同一实现。现有 62 项回归主要覆盖上层视觉与本轮 MiddleLayer，不能证明核心驱动调度正确。

**建议：** 发布清单记录两个仓库提交、DLL 哈希、框架版本、原生驱动位数与版本；清理对临时 bin 目录的隐式依赖。在 CI 构建固定版本 SDK，再构建框架；加入本次仿真、启动停止、报警语言和驱动故障契约回归。

**验收：** 从干净检出能复建匹配的 SDK/EXE 组合；安装包运行时展示的版本可追溯；DLL 更新触发契约测试，不能只以文件名或 AssemblyVersion 判断一致。

**定位：** [AlphaRap/AlphaRap.csproj](../AlphaRap/AlphaRap.csproj)；[AlphaRap/bin/x64/Debug/NPSDK.dll](../AlphaRap/bin/x64/Debug/NPSDK.dll)；`docs/NpsdkReviewCoverage.json`；`tests/VisionRuntime/Run.ps1`。

### P2-08 明确生产取像与标定应用的事务边界

证据级别：静态风险，需要相机/VPP 联调。

**现状与影响：** 实时显示使用相机 SN/索引/曝光上下文，生产 VisionRun 直接执行 TB，实际采集还取决于 VPP 内部工具。不能仅凭 UI 选中相机就证明生产取像来自同一设备。多目标标定目前主要是单工位回滚，整体提交仍需核查。

**建议：** 用相机 SN 作为生产采集身份，明确图像由框架提供还是 VPP 自采集；同相机并发统一排队。多目标标定先校验全部目标，再提交并保留整体回滚。沿用已有 RunId、超时隔离和稳定 VppId。补偿只提供表快照，仍由应用流程主动使用。

**验收：** 两相机同时请求、交换枚举顺序、超时晚到和重连时结果身份不混淆；任一标定目标失败不会留下一半更新。框架不会自动把补偿加进视觉结果。

**定位：** [AlphaRap/Vision/VpStation.cs](../AlphaRap/Vision/VpStation.cs)；[AlphaRap/Vision/VisionproInterface.cs](../AlphaRap/Vision/VisionproInterface.cs)；[AlphaRap/Core/MiddleLayer.Modules.cs](../AlphaRap/Core/MiddleLayer.Modules.cs)；`MiddleLayer.VPF.GetCompensationTable`。

## 第三优先级 P3：统一产品体验和持续演进

共 2 项。语言与设计器规则从每次修改即刻遵守；集中整理可在核心语义修复后进行。

### P3-01 把 SDK 页面纳入既有三语言与设计器规则

证据级别：静态确认。

**现状与影响：** 上层已建立中英西 XML 语言体系，但 SDK MotorParaLanguage 只有中英两项，参数页存在独立语言选择和硬编码文本；部分查询页面固定布局在运行时 BuildUi，设计器难以预览真实效果。

**建议：** SDK 接受宿主提供的翻译接口/语言变更通知，保持业务键、控件 Name、轴号和 VPP 标识稳定；不要让 SDK 反向依赖 MiddleLayer。静态布局放入 Designer，动态数据运行时绑定；对中英西长文本和 DPI 做预览对照。任何后续修改当下就应遵守规则，不必等待专项重构。

**验收：** 主界面切换三语言后，已打开和新开的 SDK 页同步变化；西语不存在退回中文的固定文字；设计器与 EXE 在约定 DPI 下布局一致，语言切换不改变业务值。

**定位：** `NPSDK.Motion.MotorParaLanguage`；`NPSDK.Motion.MotorParaForm.ApplyLanguage / BuildUi`；[AlphaRap/MenuForm/ProductionQueryForm.cs](../AlphaRap/MenuForm/ProductionQueryForm.cs)；[AlphaRap/Core/MiddleLayer.Language.cs](../AlphaRap/Core/MiddleLayer.Language.cs)；`.cursor/rules/winforms-designer-consistency.mdc`。

### P3-02 按业务责任逐步收拢依赖，并用测量指导性能工作

证据级别：架构建议。

**现状与影响：** MiddleLayer 已拆为功能文件，但 static 全局状态和 WinForms 控件仍同时承担设备对象、状态容器与操作入口；仅增加更多转发层会继续降低可读性。查询关键字变化同步全量读取文件，也会随数据增长卡住 UI。

**建议：** 优先抽取运行协调、设备会话、报警目录/事件、数据读写四个有明确责任的模块；MiddleLayer 保留少量直接门面。CRUD 使用早返回和平铺步骤。查询加防抖、取消与分页。记录扫描、取像、保存、查询和退出耗时，再决定下一步性能投入。

**验收：** 新业务无需遍历 UI 控件寻找硬件；关键状态转换能脱离窗口测试；同一份 CRUD 逻辑只有一个归属，不出现层层包装。真实数据规模下 UI 可响应，优化有前后测量。

**定位：** [AlphaRap/Core/MiddleLayer.cs](../AlphaRap/Core/MiddleLayer.cs)；[AlphaRap/MenuForm/ProductionQueryForm.cs](../AlphaRap/MenuForm/ProductionQueryForm.cs)；[AlphaRap/Controls/DeviceControl.cs](../AlphaRap/Controls/DeviceControl.cs)；[AlphaRap/Classes/ProductionDataStore.cs](../AlphaRap/Classes/ProductionDataStore.cs)；`.cursor/rules/csharp-standards.mdc`。

## 隔离复现记录

以下是本轮实际观察，不是未来测试承诺：

1. 默认 int 路径编码 0.125：FormatException。
2. 越界编码：返回错误，同时保留零缓冲区；上层调用没有检查编码错误。
3. 生产记录 09:00—11:00 查询：预期 1 条，实际 3 条。
4. CSV 多行字段：预期 first\\nsecond，读回 first。
5. 同日新增 CSV 列：预期 lost，读回空字符串。
6. 停机元数据：历史查询 false，运行目录 true。
7. 语言切换：活动报警 1→0，IsError 仍为 true。
8. Simulation=true：替身电机驱动收到 1 次命令。
9. Simulation=true：替身 IO 驱动收到 1 次命令。
10. Dispose SDK 控件后：全局列表仍包含该控件。
11. 延迟启动等待期间 Stop：后续 StartRun 仍发生 1 次，同时 running=true、stopped=true。

临时验证脚本及完整反编译位于本地忽略目录 `.vp-validation/`，未将 SDK 反编译源码或构建缓存上传到本仓库。既有 62 项回归结果是上一轮业务提交基线；本轮文档变更不把它表述为重新执行的全系统测试。

## 推荐实施顺序

先完成 P1-01 与 P1-02，并建立不接硬件的核心回归；再处理 P1-03、P1-04、P1-05、P1-06。随后处理共享设备生命周期、采样与 UI 分离、协议和数据可靠性。运动参数/轴映射与 PLC 寄存器规范必须有对应硬件验收记录。每次修改保持小范围提交，写明行为变化、验证及未验证部分，推送 GitHub；不直接重写整个 SDK。

## 本次文档验证

报告包含 16 项分级建议，所有 Markdown 本地链接目标存在；类型清单 JSON 可解析，176 项记录与全量反编译顶层类型数一致。配套交互审查看板使用本机 Canvas SDK 声明执行 TypeScript 检查，诊断数为 0；未将类型检查等同于界面实际渲染验收。本次未修改生产代码，未重新执行构建或真实硬件运行。

## NPSDK 定位方法

本文 `NPSDK.类型.方法` 指实际 DLL 反编译行为，`DataSave`、`TcpipClient` 等也是此 DLL 内的命名空间/类型。源码对应目录主要为 `FlowChart/`、`NPSDK.Motion/Controls/`、`NPSDK.Motion/Interface/`、`NPSDK.Motion/Vendors/`、`NPSDK.Motion/Forms/`、`DataSave/` 及根目录 `Alarm.cs`。由于源码现成产物与引用 DLL 不同，修改时须从受版本控制的 SDK 源码重新构建并执行契约测试，不能直接修改反编译文件作为发布源。
