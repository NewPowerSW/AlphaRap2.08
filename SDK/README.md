# 核心程序集引用

`lib/NPSDK.dll` 是 AlphaRap 与 AlphaRapLibrary 共用的核心程序集，包含 JTimer、Alarm、Flow_Chart、Flow_BaseForm 和 NPSDK.Motion 等接口。

两个项目均通过仓库相对路径 `..\SDK\lib\NPSDK.dll` 引用它。不要将核心 DLL 的唯一副本放在构建输出目录，清理或重新生成可能删除输出目录中的依赖副本。

AlphaRap 使用 ProjectReference 引用 AlphaRapLibrary；其输出名称仍为 NewPowerLibrary.dll。MSBuild 会按依赖顺序构建并复制到主程序输出目录，Debug/Release 不再依赖固定的 `AlphaRapLibrary\bin\Debug` 文件。

## 当前二进制来源

- 来源：本仓库 `ef11eff` 提交的 `AlphaRap/bin/x64/Debug/NPSDK.dll`，与回滚后的框架接口一致。
- 大小：427520 字节。
- SHA256：`23ee59b1d45534e348ef897db5cdd06aff83ac98a3a982593e16178e9f2c0609`。
- 本次仅固定引用位置，未重新编译或修改外部 NPSDK 工作区。

升级 NPSDK 时替换此处的已验证版本并更新来源和校验值；两个项目始终引用同一版本。不要直接引用其他开发工作区中含未提交修改的构建产物。

## 构建和回归

安装 .NET Framework 4.8 开发工具、项目所需的 Cognex VisionPro SDK，并还原 packages.config 依赖后，在 Visual Studio 中重新生成 `AlphaRap.sln`，选择 `Debug | x64`。

开发者命令行可运行：

```powershell
MSBuild.exe AlphaRap.sln /t:Rebuild /p:Configuration=Debug /p:Platform=x64
./tests/VisionRuntime/Run.ps1 -AssemblyDirectory AlphaRap/bin/x64/Debug
```

Debug/x64 已验证两个项目正常生成，并使用实际输出程序集通过 62 项视觉、语言和运行入口回归。验证没有使用临时 ReferencePath；本次未进行现场硬件运行或 Visual Studio 设计器实测。
