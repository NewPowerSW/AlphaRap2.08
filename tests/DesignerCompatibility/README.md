# WinForms 设计器兼容性检查

覆盖 `SystemSetting`、`FlowProgramming` 的所有 Form/UserControl（自动发现）以及语言窗体基类。

```powershell
./tests/DesignerCompatibility/Run.ps1 -AssemblyDirectory 'AlphaRap/bin/x64/Debug'
```

检查内容：

- 可视类具有公开无参构造函数，项目中的 SubType 和 Designer 依赖正确；业务 partial 不作为独立窗体打开。
- `InitializeComponent` 仅使用显式字段创建、属性赋值、明确类型数组和常规布局/集合调用。配置表、默认业务行和运行绑定放在对应的业务源码中。
- 使用 `LicenseUsageMode.Designtime` 构造每个窗口/控件，创建句柄、布局并输出截图。设计期不写配置、不登记硬件、不启动系统设置权限计时器。
- 系统设置仍具有独立的 Setting/Recipe 常量表和原有 Boolean 默认值。

截图位于运行输出的 `.vp-validation/designer-compatibility/<本次运行目录>`。该探针不等价于 Visual Studio 的源码设计器解析。交付前仍应在 VS 中重新打开相关 `.cs` 的设计视图，确认没有解析错误、缺控件或布局裁切；纯业务 `.cs` 使用代码视图。

语言切换、配方作用域、常量读写及 MainForm Save 的行为由以下回归一并验证：

```powershell
./tests/FlowProgramming/Run.ps1 -AssemblyDirectory 'AlphaRap/bin/x64/Debug'
./tests/VisionRuntime/Run.ps1 -AssemblyDirectory 'AlphaRap/bin/x64/Debug'
```

测试使用隔离目录，不调用轴运动、相机采集或生产流程。设计器源码检查是对项目维护规范的限制，不代表所有被限制的 C# 语法均无法编译。
