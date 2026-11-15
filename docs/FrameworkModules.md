# 无窗口模块与系统参数试点

框架 6.4.0 将业务模块生命周期与 WinForms 页面分开。`IFrameworkModule` 声明固定 Id、应用初始化和释放；需要配置能力的模块实现 `IConfigurationModule`，提供 `ModuleConfiguration` 和草稿校验。Id 不使用翻译后的名称。宿主在应用组装阶段显式注册，不在构造函数中启动业务或生产。

框架 6.5.0 增加文件型配置参与者。硬件、视觉、数据和权限页各自通过接口声明保存能力，ProjectConfiguration 不再按页面类型寻找校验、写入和恢复方法。可复用的配置功能不需要继承这些页面。

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

除 DataSet 型配置外，文件型功能可同时实现 `IFrameworkModule` 和 `IConfigurationParticipant`，通过原有模块宿主注册后自动接入主保存。接口定义固定 `ConfigurationId`、执行次序、草稿校验、保存快照和放弃预检。已登记页面也可直接实现该接口；同一实例只发现一次，重复标识和重复目标路径会在写入前拒绝。

```csharp
public ConfigurationChange PrepareConfigurationSave()
{
    string captured = draftText;
    return new ConfigurationChange(
        new[] { new ConfigurationFile(configPath, temporary => File.WriteAllText(temporary, captured)) },
        () => savedText = captured);
}
```

准备方法应捕获本功能的值，写入委托只序列化到传入的暂存路径，不能改写正式文件或其他模块草稿；接受动作只接受对应快照，不执行磁盘写入。`ConfigurationFile.CompareAndReplace` 可要求目标内容仍匹配预读字节，检测外部编辑。事务执行状态保持私有，每次操作使用独立任务，调用方的文件列表和原始字节不会共享给执行状态。

`ConfigurationOrder` 越小越先执行，同次序按固定标识排序。既有视觉、设备配置、权限、硬件、模块数据、生产字段与日志定义、流程分别保留其恢复先后关系。操作开始前先检查机器状态和权限，再发现及校验参与者。

主保存固定本次参与者集合，先校验、捕获全部快照，再暂存与替换；任一步准备或写入失败都不接受快照。放弃修改先收集全部恢复动作，全部预检成功后才按执行次序恢复。参与者应在预检阶段读取和校验必要输入，返回的恢复动作仍可能因执行期错误而失败，框架不承诺任意业务恢复动作的内存原子回滚。保存或放弃回调不能再创建嵌套配置事务。

ProductManagerForm 继续通过既有应用入口切换配方，已注册模块的配方草稿阻止静默切换。配方按模块命名空间加载，机器设置不随配方改变。独立读取失败不会先清空正在编辑的表；旧表实例与绑定保持不变。

FlowResources 同时发现已注册模块及旧页面，按配置实例去重。系统参数页关闭后，宿主模块仍提供常量及原有参数引用；普通无窗口模块按固定模块 Id 暴露参数。

## 兼容与验证

`ModuleBaseForm` 保留已有公开数据字段和方法，配置读取委托给 ModuleConfiguration；旧模块仍可沿用 ModuleManager.ModuleList。旧 Form 构造时登记到兼容清单的行为暂时保留，运行模块使用独立的显式注册机制。本轮未迁移生产工位、手动任务和全部其他编辑页面，也未改变 NPSDK 硬件实现。

兼容页的有效旧配置名称保持原样；标题为空或包含文件名非法字符时，依次使用有效标题、控件 Name 或类型名，首次确定后固定。系统参数的显式业务身份和存储名不受此回退规则影响。

`tests/FrameworkModules/Run.ps1` 验证无窗口初始化、旧设置、注册失败、释放顺序、命名空间隔离、读失败保留草稿、主保存和放弃、保存失败、流程参数发现，以及页面绑定和三语切换。无窗口模型仍位于现有框架包中；本轮解除类与窗体的依赖，没有拆分成单独的无 WinForms 程序集。

`tests/ConfigurationParticipants/Run.ps1` 使用无窗口文件模块验证自动发现、接受边界、重复标识、校验和准备失败、中途替换回滚、目标冲突、外部编辑、放弃预检、重入保护以及参与者和文件快照。测试只使用隔离目录，不操作生产文件或设备。
