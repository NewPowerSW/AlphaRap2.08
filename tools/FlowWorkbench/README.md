# FlowWorkbench 流程配置测试版

这是一版可运行的独立原型，用来验证“选择动作及实例 → 绑定数据 → 条件分支 → 本地保存”。运行内核为 `AlphaRap.FlowRuntime.dll`，界面为 `FlowWorkbench.exe`，目标框架为 .NET Framework 4.8。

## 启动

已构建程序位于项目根目录的 `.vp-validation/flow-workbench/FlowWorkbench.exe`，双击即可运行。只分发测试包时，完整解压 `FlowWorkbench-Test.zip`，保持 EXE、DLL 和 LanguageData 在同一目录。

从源码构建并启动，在项目根目录执行：

```powershell
& ./tools/FlowWorkbench/Start.ps1
```

只构建：

```powershell
& ./tools/FlowWorkbench/Start.ps1 -BuildOnly
```

重新生成可分发测试包：`& ./tools/FlowWorkbench/Package.ps1`。

构建需要 Visual Studio MSBuild 和 .NET Framework 4.8 开发包；运行已有测试包只需要 .NET Framework 4.8。也可以用 Visual Studio 打开 `tools/FlowWorkbench/FlowWorkbench.csproj`。

## 先测试默认流程

1. 启动后已经载入示例，点击“校验”，应显示通过。
2. 点击“运行”，默认约一秒完成。
3. 在“运行 / 报警”查看顺序：Start → Wait → Move → Vision → Check → OK → Save。
4. 在“本次数据”查看每个节点的输出，包括视觉坐标、判定和保存路径。
5. 点击“查看结果”，进入底部显示的保存目录。日期子目录中会产生一份 UTF-8 CSV，可用文本编辑器或 Excel 查看。

每次运行都有新 RunId，每个保存节点产生独立文件，避免多产品共享一份可覆盖结果。默认位置为程序目录下 `ProductionData/yyyy-MM-dd/<RunId>-Save.csv`，可用“保存目录”更改。

默认 CSV 字段：RecordId、RunId、StartedAt、FlowVersion、Barcode、X、Y、Angle、Passed。Passed=True 为范围内，False 为范围外；这是检测结果，不是流程执行是否成功。

## 不写代码修改参数与数据绑定

左侧“节点”表选择一行，右侧显示该动作的输入。

- “来源”为“常量”：使用右侧“常量”列的值。
- 选择 `节点ID.字段`：读取前驱节点本次执行的输出，常量格自动只读。
- 常量数字使用小数点，例如 `0.12`；布尔值使用 `true` 或 `false`。

默认保存节点的绑定：

```text
Barcode ← Start.Barcode
X       ← Vision.X
Y       ← Vision.Y
Angle   ← Vision.Angle
Passed  ← Check.Passed
```

右侧来源列表展示所有可选输出，点击“校验”会拒绝未来节点、错误类型以及只在另一分支存在的值。绑定必须在所有到达路径上都有效，不能用零或上次结果补缺失数据。

## 测试 NG 分支

1. 在“节点”页选择 Vision。
2. 把“实例”从 CameraA 改成 CameraB。
3. 点击“运行”。CameraB 的 X=2.5，默认范围为 [-1, 1]，应经过 NG 节点，再进入 Save。
4. 本次 CSV 的 X 应为 2.5，Passed 应为 False。

也可以选择 Check，修改右侧 Min、Max。范围判断为闭区间，结果输出 `Passed`。Next/True 和 False 分别选择目标节点；两条路径都要配置保存。

## 测试超时报警与停止

1. 在“仿真实例”页取消 ReadyInput 的 State 勾选，表示输入始终未满足。
2. 回“节点”页，将 Wait 的超时设为 500 ms，报警选 SIM_TIMEOUT。
3. 点击运行，应在 Wait 超时终止；运行/报警页出现绑定报警，不会继续移动、拍照或保存伪造的生产结果。
4. 再次运行，在超时前点击“停止”，应显示已停止，后续节点不执行。
5. 恢复 ReadyInput.State=true 后可再次运行。每次运行的结果独立。

SIM_ACTION 和 SIM_TIMEOUT 是工作台自身的仿真报警标识。它们不调用生产框架的报警清除、暂停或停机接口。异常/超时会终止本次测试流程。

## 添加字段、节点与实例

### 保存字段

选择 Save，点击“增加保存字段”，例如把 Field1 改为 StationNote，来源选择 Move.X。下次 CSV 就会包含 StationNote 及本次模组输出。也可使用常量保存批次或备注。

字段名使用字母、汉字、数字及下划线，不能以数字开头。RecordId、RunId、StartedAt、FlowVersion 为保留字段。选中自定义字段后按 Delete 可删除；动作必需字段受到保护。

### 节点

点击“加节点”，选择动作、类型匹配的实例、下一步及超时。它不会自动插入已有连线，必须把前驱的 Next 改为新节点 ID，再把新节点 Next 接回原后继。校验会拒绝不可达节点和缺失出口。

“流程图”页预览当前连线，点击节点可以查看右侧参数。T/F 表示范围判断的真假出口。此版通过节点表编辑连线，不提供拖拽连线和自由布局。

### 实例

点击“加实例”，在“仿真实例”页选择 Input、Output、Module 或 Vision，配置状态、坐标和延迟。节点目标下拉框按动作所需类型筛选。被节点引用的实例不能删除。

实例 ID 由工作台稳定分配，不随语言或显示标题变化。设备参数在运行开始时复制，运行期间编辑被锁定。

## 保存和重新加载流程

点击“保存流程”把节点、输入绑定、分支、超时、报警和仿真实例保存为 XML；重新启动后点击“打开”恢复。示例按钮会重新载入默认流程，操作前请先保存需要保留的编辑。

XML 用版本化数据描述流程，不保存委托、控件地址或任意 CLR 对象。保存先写临时文件，再替换正式文件。流程版本默认 1，后续版本管理界面尚未提供。

## 首版边界

- IO、模组和视觉均为仿真；本地 CSV 保存是真实执行。原型不引用 NPSDK、Cognex 或 PLC 驱动，也不启动现有 AlphaRap 生产程序。
- 已提供手动条码触发、读取 IO、设置输出 IO、等待 IO、模组移动、视觉定位、范围分支、布尔分支、分支说明和生产保存十类动作。模组/视觉数值用于验证传值，不代表真实设备完成或安全互锁。
- 顺序执行，支持无环分支与汇合；尚未提供循环、并行、子流程、暂停/续跑、断点、断电恢复、MES 上传、复杂表达式和真实报警主表绑定。
- 同一运行器拒绝重叠启动；等待动作支持超时和协作取消。保存已经提交的记录不会因之后停止而删除。
- 默认要求每条结束路径都经过保存；保存失败不返回成功记录 ID，不会自动重试。
- CSV 是每次运行/每个保存节点一份文件，尚未提供日报合并、数据库索引或上传补传。

## 语言与设计器

右上角支持 Chinese、English、Español。语言包遵循框架既有的“语言/窗体/键/@ComponentText”结构，表头按“表格名_列名”登记；此独立程序使用自己的 LanguageData 文件，以便离线测试不启动主框架。

切换语言不重建参数行、不提交未完成编辑、不改变 CurrentCulture。节点 ID、动作 ID、实例 ID、输出键和业务值保持不变。主框架入口及 MiddleLayer 的语言实现未修改。

固定控件及布局位于 FlowWorkbenchForm.Designer.cs；流程图使用 FlowGraphView 控件，并提供设计环境示例。已实际渲染检查三语运行界面，尚未在 Visual Studio 中打开设计器或验证所有 DPI。

## 验证与扩展

执行离线回归：

```powershell
& ./tests/FlowWorkbench/Run.ps1
```

覆盖分支、类型与路径检查、跨运行隔离、超时、停止、重复启动、XML、CSV 转义、保存失败、三语编辑保护、实际窗口操作和关闭等待。

2026-10-22 离线验证：56 项断言通过；三语各 88 个键集合一致、占位符一致，英西译文无中文残留；三语实际窗口已截图检查。未连接真实硬件，未验证 Visual Studio 设计器及其他 DPI。

运行内核与 WinForms 分开，动作通过 ActionSpec 显式登记输入输出和 Execute。后续接真实设备时需要受控实例注册、单位与互锁、明确完成/失败状态和设备停止确认；当前仿真适配器不能直接当作已验证的真实驱动接口。
