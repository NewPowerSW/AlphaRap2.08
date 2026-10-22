# NPSDK.FlowDesigner

这是围绕 `NPSDK.Flow_Chart` 的 WinForms 设计器组件，不提供另一套线程流程调度器。界面放置真实 SDK 控件，并将动作适配器接到控件的 `FlowChartRun` 事件。等待返回 IDLE，后继交接使用 SDK 的 Next/Case 引用。

`FlowDesignerForm` 负责画布、属性和配置存取；`FlowChartSession` 处理单次动作、参数引用、条件比较和运行数据；`FlowActionRegistry` 显式登记允许选择的方法及实例，不执行任意反射配置。

宿主创建 `FlowDesignerEnvironment`，注入 Registry、Example、DefaultPath、OutputDirectory、语言入口及试运行前检查，再打开 `FlowDesignerForm`。动作使用稳定的 MethodName 和 TargetId；参数声明 Name/Type/Value，输出声明 Outputs。

适配器 `Start` 每次进入节点执行一次，`Poll` 在 SDK 轮询中采样，尚未完成时返回 Waiting。阻塞 SDK 调用应在 Start 中创建 Task，保存到 FlowInvocation.Pending，Poll 只观察结果。Cancel 负责动作的明确取消策略。不要在 Poll 中无条件重新触发拍照或运动。

参数 Source 使用 `Node.Field`；会话只读取本轮已发布输出。Stop 后不再接受迟到结果。界面会等 SDK worker、交接线程及 Pending 任务结束再允许修改或重新运行。

DefaultPath 用于显式保存和下次自动恢复完整设计，加载不启动。XML 存稳定对象身份而不是序列化硬件对象。未知绑定可离线修复；运行前必须通过完整校验。文件原子替换并保留上次 `.bak`。

语言由宿主提供键到译文的映射。FlowDesignerTexts.Defaults 提供完整中、英、西三语底稿；UI 不翻译用户数据。固定界面位于 Designer.cs，默认构造器只建立 DEMO 对象，不连接设备。

`lib/NPSDK.dll` 包含对应 SDK 核心扩展：独立执行范围、节点配置暂停、在途忙状态、非生产节点创建、UI 线程派发及初始化状态接口。保留原公共 API 和现用 DLL 的 IO 极性。示例注册不会调用 Alarm.Initial；只有独立离线程序初始化其专用示例报警表。

构建目标 .NET Framework 4.8。完整使用说明和回归测试位于 AlphaRap 仓库的 `tools/NpsdkFlowDesigner` 与 `tests/NpsdkFlowDesigner`。
