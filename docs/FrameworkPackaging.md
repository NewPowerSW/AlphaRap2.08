# AlphaRap 框架与应用

- **AlphaRap.Framework** 提供设备和控件发现、流程、动态资源、视觉、配方、权限、报警和日志等通用模块与编辑页。
- **AlphaRap** 是本机应用，负责进程启动、主窗体、首页、页面导航、整机运行和配方切换，并拥有设备驱动、PLC、固定 IO、工位事件、视觉配方表及客户 MES。
- **NPSDK** 提供流程控件、运动和 IO，源码工程位于 `SDK/NPSDK`。框架和应用的源码构建均引用该项目。模块基类和模块注册由框架提供。

应用入口调用 `ApplicationHost.Initialize(profile)`，由 `MachineApplication.Initialize` 组装页面和启动模块。应用将窗口赋给 `ApplicationHost.MainWindow`，通过 `ApplicationHost.RegisterModule(page)` 登记真实页面，并在窗口关闭时异步等待 `ApplicationHost.ShutdownAsync()` 完成应用清理。窗口可实现 `IModuleWindow` 接入统一保存、权限更新和视觉预览。Device 派生类和 NP 控件派生类仍按已有规则发现，站点的 `FlowChartRun` 事件由运行器调用，流程页按工站显示执行状态。项目代码可以访问设计器创建的字段，也可以通过 `ApplicationHost.GetResource<T>(id)` 取得流程目录里的同一个动态实例。

### 过渡版中新增页面、工站或 IO

- 增加固定页面或工站，从应用项目的 `StationApplication.cs` 入手。固定控件放在各自窗体的设计器里。启动时先建立手动页和硬件目录，手动页通过 `BindResources` 显示已有 IO，再由 `CreateStations()` 把工站需要的对象传入构造函数。下料工站的绑定集中在 `ConnectDownConveyor`。
- 增加现场可配置的轴或 IO，使用硬件页和主窗口保存。配置中的 `HardwareDefinition.Id` 是稳定身份。应用在组装工站时可用 `ApplicationHost.GetResource<NP_Input>("hardware/" + inputId + "/input")` 取得已有输入，输出和轴的末段分别为 `output`、`axis`。改显示名或移动 IO 分组保留对象。删除资源或更换驱动后，后续使用应按 ID 重新绑定。
- 流程页展示已安装的工站。站点运行依赖在启动阶段组装，关闭或重新打开监视页都会保留工站实例。

### 动态控件命名

同一资源查找范围内的名称必须唯一。轴和 IO 在硬件目录内唯一，新增 Device 和改名在整机范围查重，相机名在视觉模块内唯一，VPP 名在所属相机内唯一。硬件、Device、相机和 VPP 查重或查询时忽略字母大小写。配方常量、机器常量及客户日志沿用各自的配置键。

动态页面和资源控件的 `Name` 使用控件用途和稳定身份，设备页签与面板各有自己的名称。显示名称修改时继续用原设备面板键、硬件 ID 和 VPP ID。通过对象引用或页签绑定查找页面。模板内部固定子控件在自己的容器内查找，沿用设计器和语言包中的原名称。旧配置如果已有跨面板同名设备，可按面板键访问，新增和改名时必须选择未占用的名称。

### 按名称访问现场资源

工站在对应模块初始化后，可直接使用配置中的业务名称。

```csharp
var axis = ApplicationHost.GetAxis("Axis_1");
var start = ApplicationHost.GetInput("StartButton");
var output = ApplicationHost.GetOutput("Output_1");
var points = ApplicationHost.GetAxisPoints("Axis_1");
var scanner = ApplicationHost.GetDevice<ScannerKeyenceTcp>("KeyenceScann");
var camera = ApplicationHost.GetCamera("TakePCBA");
var vpp = ApplicationHost.GetVpp("TakePCBA", "VPP1");
bool skipScan = ApplicationHost.GetConstant<bool>("Setting", "DisScann");
```

Device 同名时传入第二个参数，例如 `ApplicationHost.GetDevice<ScannerKeyenceTcp>("KeyenceScann", "Parameter_dc2")`。硬件和 Device 找不到时抛出异常，相机和 VPP 找不到时返回空。按名称查询会随现场名称修改变化，持久化绑定继续使用原资源 ID。生产定位使用已选定的 VPP 时，仍调用视觉页的 `TryGetProductionStation`。

常量用 `Setting` 或 `Recipe` 指定作用域，并按定义使用 `bool`、`long`、`double` 或 `string`。点位随配方变化，切换配方后重新获取点位快照。

客户日志先在数据页定义名称和字段并主保存，然后调用 `ApplicationHost.Data.WriteLogByName("客户检测记录", values)`。`values` 是按字段名赋值的字典。调用成功返回文件路径，仿真时写入模块目录下的 Simulation 子目录。产品主表仍使用 `SaveProductionRecord`。


## 过渡版工站开发

在本解决方案的 `AlphaRap/TaskProcess` 中维护工站 C# 步骤。已有工程按应用配置的设置目录读取 `FlowPrograms.xml`。迁移现有设备时使用该设备已验证的流程文件、配方和 IO 映射，分别核对初始化和自动运行入口。流程页供现场监视，主保存校验现有流程的资源引用并保存设备与配方配置。视觉 VPP 通过 TB 编辑器维护。

### 迁入本机运行数据

首次在另一台开发机检出过渡分支时，从该设备原运行目录导入基础数据，例如 `.\build\Import-TransitionRuntimeData.ps1 -SourceDirectory E:\Machine\AlphaRap`。该目录应含 `MachineSetup.ini`、`SystemData.mdb` 及标准的 `ModuleData` 子目录。本机快照位于 `AlphaRap/RuntimeData.local`，独立于 Git。构建时仅向当前 EXE 目录补充缺失文件。导入保留原设备的仿真设置和点位映射，INI 中自定义的外部路径需沿用对应的现场目录。

## 构建和发布

仓库开发时从 `AlphaRap.Transition.sln` 以 Debug/x64 构建 NPSDK、框架和应用。打包需要 .NET Framework 4.7.1 和 4.8 开发工具、Cognex VisionPro SDK、原有 NuGet 依赖以及 .NET SDK。

```powershell
./build/Pack-Framework.ps1
```

生成 `artifacts/framework/AlphaRap.Framework.<版本>.nupkg` 及 SHA-256 文件。包包含框架及其共享程序集，其中 `NPSDK.dll` 由同一次 Release/x64 构建生成。设备驱动、站点 EXE、配方、报警及语言文件由应用发布。Cognex 运行库、授权和机器的原生硬件驱动随现场部署环境提供。

应用也可以仅引用本地版本包。以下命令在当前仓库验证这个入口：

```powershell
MSBuild.exe AlphaRap/AlphaRap.csproj /restore /t:Build /p:Configuration=Release /p:Platform=x64 /p:UsePackagedFramework=true
```

## 版本规则

- 框架发布版本由 `AlphaRap.Framework/FrameworkVersion.props` 管理，与 NuGet 版本和框架 DLL 产品版本一致。应用包版本固定在 `AlphaRap/AlphaRap.csproj`，由应用团队明确升级。
- 补丁修复升 PATCH，兼容新增接口升 MINOR，破坏扩展契约时升 MAJOR 并更新程序集兼容版本。修复已经发布的内容时发布新版本，打包脚本会拒绝覆盖同名包。
- 主窗口底栏显示应用 EXE 版本，版本提示包含实际载入的框架版本。升级发布时按一套验证应用 EXE、框架包和现场运行依赖。

### 从 1.x 升级到 2.x

应用引用升级到 `AlphaRap.Framework 2.0.0` 后重新构建。FFU 使用的 `AlphaRap.Classes.NPSerialPort` 和基恩士 3D 设备使用的 `NPClient.TCPCLient` 源码由应用项目编译。自行使用这两个类的应用可沿用应用侧实现。旧版未接入流程的 `Warning`、`SpanLife` 和 `NoFocusChart`，以及旧日志工具 `LocalLogSave`、`XMLExpand` 已从框架包移除。


### 从 2.x 升级到 3.x

应用引用升级到 `AlphaRap.Framework 3.0.0` 后重新构建。统一保存使用的 `Alpha.OptionChoiceForm` 由应用项目编译。框架通过 `MachineApplication.ShowWarning` 发出警告，应用可以在这个入口使用自己的弹窗。


### 从 3.x 升级到 4.x

应用通过 `MachineApplication.CreateDataPage` 提供自己的数据页，并在派生页中用 `RegisterLogSources` 登记固定日志组件。AlphaRap 的六个固定日志定义位于 `StationDataForm`。写入使用 `WriteLog` 或 `WriteDistinctLog`，可通过 `GetLogSource` 取得原始组件。既有日志目录、CSV 列和 `log/builtin/` 流程键沿用原值。

### 从 4.x 升级到 5.x

应用和扩展使用 AlphaRap.Framework 5.0.0 重新构建。ModuleBaseForm 和 ModuleManager 由 AlphaRap.Framework.dll 提供，沿用 AlphaRapLibrary 命名空间。项目可移除 NewPowerLibrary.dll 的直接引用，旧二进制扩展须重新编译。

### 从 5.x 升级到 6.x

`MiddleLayer` 由应用项目编译。自定义应用升级后需实现 `MachineApplication.Initialize` 和 `Shutdown`，自行决定模块组装和启停顺序。通用页面通过 `ApplicationHost.FindModule<T>()` 获取已登记的同一实例，语言初始化可调用 `ApplicationHost.InitializeLanguage(language)`。应用通过 `PauseMachine`、`StopMachine` 和 `ChangeRecipe` 分别处理报警暂停、流程停止和配方页发出的切换请求。使用 `FlowControl` 构造参数接入应用的周期任务和初始化复位。

### 从 6.x 升级到 7.x

应用及扩展必须重新构建。SysPara.SystemMode/SystemRun 已由字段变为统一状态属性，LogManagement.Instance 已改为惰性属性；旧二进制不能直接替换 DLL。机器应用可覆盖 ShutdownAsync，窗口关闭应异步等待，不再强制结束进程。账号迁移、整份配方准备和持久事务恢复的边界见 [框架基础边界](FrameworkSafety.md)。
