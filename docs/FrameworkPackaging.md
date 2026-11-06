# AlphaRap 框架与应用

- **AlphaRap.Framework** 提供设备和控件发现、流程、动态资源、视觉、配方、权限、报警和日志等通用模块与编辑页。
- **AlphaRap** 是本机应用，负责进程启动、主窗体、首页和页面导航，并拥有设备驱动、PLC、固定 IO、工位事件、视觉配方表及客户 MES。
- **AlphaRapLibrary 和 NPSDK** 是框架使用的公共依赖。引用方向为应用 → 框架 → 公共依赖。

应用入口调用 `ApplicationHost.Initialize(profile)` 初始化模块，随后用自己的窗口运行 WinForms 消息循环，退出时调用 `ApplicationHost.Shutdown()`。应用可继承 `MachineApplication` 提供窗口与工位。窗口可实现 `IModuleWindow` 接入统一保存、权限更新和视觉预览。Device 派生类和 NP 控件派生类仍按已有规则发现，站点的 `FlowChartRun` 事件继续直接进入工具箱。项目代码可直接访问设计器创建的字段，也可通过 `ApplicationHost.GetResource<T>(id)` 取得流程目录里的同一个动态实例。稳定资源键沿用原流程引用。

## 构建和发布

仓库开发时从 `AlphaRap.sln` 以 Debug/x64 构建框架、应用及公共依赖。发框架包时，使用安装了 .NET Framework 4.8 开发工具、Cognex VisionPro SDK、原有 NuGet 依赖和 .NET SDK 的开发机：

```powershell
./build/Pack-Framework.ps1
```

生成 `artifacts/framework/AlphaRap.Framework.<版本>.nupkg` 及 SHA-256 文件。包只含框架和框架使用的共享程序集。设备驱动、站点 EXE、配方、报警及语言文件由应用发布。Cognex 运行库、授权和机器的原生硬件驱动随现场部署环境提供。

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
