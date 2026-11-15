# 无窗口模块与系统参数试点

框架 6.4.0 将业务模块生命周期与 WinForms 页面分开。`IFrameworkModule` 声明固定 Id、应用初始化和释放；需要配置能力的模块实现 `IConfigurationModule`，提供 `ModuleConfiguration` 和草稿校验。Id 不使用翻译后的名称。宿主在应用组装阶段显式注册，不在构造函数中启动业务或生产。

```csharp
var context = new ModuleContext(SysPara.SettingDataDirectory);
var parameters = ApplicationHost.Modules.Register(
    new SystemParametersModule(defaults), context);

// 界面可选；显示与无窗口调用共享同一数据源。
var page = ApplicationHost.RegisterModule(new SystemSetting(parameters));
```

`ModuleRegistry` 对同一实例幂等注册；重复 Id 拒绝覆盖；初始化失败清理半成品，不发布失败实例；应用关闭时按注册逆序释放，累计错误并继续释放其余模块。模块组装、保存和配方加载由应用宿主串行调度，不支持生产过程中并发重组模块。页面关闭不释放宿主拥有的模块。

## 参数页

`SystemParametersModule` 持有常量、设置表、配方表和校验。`SystemSetting` 保留 Designer 布局、权限显示和编辑事件，通过构造参数绑定模块。`ConstantStore` 不再持有 SystemSetting，也不再实现 WinForms 调度；UI 适配器负责消息线程切换，无窗口宿主直接使用模型入口。

系统参数固定业务 Id 为 `system.parameters`，沿用 `SysSetting.xml`、配方命名空间 `SysSetting`、旧 PSet 布尔列以及 `Constants/Setting/...` 和 `Constants/Recipe/...` 引用。页面显示名与配置存储名分离。语言切换沿用现有三语机制，只刷新文案和显示格式，不重建数据或覆盖草稿。

## 主保存与配方

注册配置模块后，既有 MainForm Save 自动校验并捕获其配置，继续使用原有暂存、替换和失败回滚机制。全部文件成功后才接受内存版本；放弃修改重新加载同一模型。模块不增加保存按钮或配方选择器。

ProductManagerForm 继续通过既有应用入口切换配方，已注册模块的配方草稿阻止静默切换。配方按模块命名空间加载，机器设置不随配方改变。独立读取失败不会先清空正在编辑的表；旧表实例与绑定保持不变。

FlowResources 同时发现已注册模块及旧页面，按配置实例去重。系统参数页关闭后，宿主模块仍提供常量及原有参数引用；普通无窗口模块按固定模块 Id 暴露参数。

## 兼容与验证

`ModuleBaseForm` 保留已有公开数据字段和方法，配置读取委托给 ModuleConfiguration；旧模块仍可沿用 ModuleManager.ModuleList。旧 Form 构造时登记到兼容清单的行为暂时保留，运行模块使用独立的显式注册机制。本轮未迁移生产工位、手动任务和全部其他编辑页面，也未改变 NPSDK 硬件实现。

`tests/FrameworkModules/Run.ps1` 验证无窗口初始化、旧设置、注册失败、释放顺序、命名空间隔离、读失败保留草稿、主保存和放弃、保存失败、流程参数发现，以及页面绑定和三语切换。无窗口模型仍位于现有框架包中；本轮解除类与窗体的依赖，没有拆分成单独的无 WinForms 程序集。
