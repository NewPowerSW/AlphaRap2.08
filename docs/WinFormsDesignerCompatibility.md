# WinForms 设计器检查

`HardForm`、`ManualForm`、`HardwareEditForm` 和 `ConstantEditForm` 的固定界面使用显式控件创建、属性赋值和 `Controls.Add`，避免在 `InitializeComponent` 中使用对象初始化器、隐式数组和循环生成固定行。控件名称、绑定关系和原有布局保持不变。

`AddUserForm`、`ProductManagerForm`、`DeviceControl` 和 `UserLoginForm` 在固定界面初始化后检查设计环境。设计时不读取账号、加载配方、登记运行期语言事件或写入语言配置；登录窗口保留安全的图标预览。

主窗口、灯塔设置、系统容器、报警表和语言表的运行期加载同样设置保护。对于已被设计宿主接管的页面，还检查控件的 `DesignMode`，防止构造完成后 LicenseContext 改变，导致 Load、Shown 或 VisibleChanged 执行业务。

全页面验证入口：

```powershell
tests/DesignerCompatibility/Run.ps1 -AssemblyDirectory '<隔离编译输出目录>'
```

检查器从项目中自动发现带 Designer 的已编译窗体和控件，校验项目元数据、设计器支持的初始化语法和字段初始化。每个类型先在设计 LicenseContext 下独立构造，检查配置目录和硬件状态；布局预览另用 .NET DesignSurface 和已有的预览模式，避免显示生产用窗体。

预览模式仅用于布局渲染，不能代替前面的真实设计 LicenseContext 构造检查。离线验证同样不能替代 Visual Studio 的 C# 源码解析器；需要在 VS 中重新打开相应窗体确认。遇到缓存的错误页，应关闭该设计文档再打开，不使用“忽略并继续”覆盖源文件。

验证覆盖 49 个窗体与控件，合计 53 项设计器检查通过；针对已接入设计站点的页面，还在恢复运行期 LicenseContext 后触发加载和显示事件，检查不会访问配置。视觉及三语 64 项、主页 14 项、动态轴发现 7 项、SDK 轴停止 5 项通过，完整流程 178 项通过。未连接现场硬件；由于 Windows 锁屏，VS 逐页交互重开验证未全部完成。
