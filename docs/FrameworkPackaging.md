# AlphaRap 框架与应用

- **AlphaRap.Framework** 提供主界面、设备与控件基类的发现、流程运行与编辑、配方、动态资源及统一保存。
- **AlphaRap** 是本机应用，拥有设备驱动、PLC、固定 IO、工位默认事件、视觉配方表、客户 MES 及机器配置。
- **AlphaRapLibrary 和 NPSDK** 是框架使用的公共依赖。引用方向为应用 → 框架 → 公共依赖。

应用入口调用 `ApplicationHost.Run(new StationApplication())`。简单项目可继承 `MachineApplication` 并只覆盖 `CreateStations`。Device 派生类和 NP 控件派生类仍按已有规则发现，站点的 `FlowChartRun` 事件继续直接进入工具箱。项目代码可直接访问设计器创建的字段，也可通过 `ApplicationHost.GetResource<T>(id)` 取得流程目录里的同一个动态实例。稳定资源键沿用原流程引用。

## 构建和发布

仓库开发时从 `AlphaRap.sln` 以 Debug/x64 构建两个项目。发框架包时，使用安装了 .NET Framework 4.8 开发工具、Cognex VisionPro SDK、原有 NuGet 依赖和 .NET SDK 的开发机：

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
