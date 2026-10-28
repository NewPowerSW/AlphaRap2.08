# 语言切换约定

程序支持 `Chinese`、`English`、`Español`，枚举值依次为 0、1、2。

启动时 `MiddleLayer.ReadLanguageSettings()` 先读取 MachineSetup.ini 的 `MachineSetup/LanguageName` 和 `PathSetup/LanguageDirectory`，然后创建页面。语言目录默认是工作目录下的 LanguageData。界面切换入口为 `MainForm.SwitchLanguageTo()`；它调用 `MiddleLayer.SwitchLanguage()` 并保存当前语言。

界面只修改 CurrentUICulture，不修改 CurrentCulture，避免语言切换改变参数的小数点格式。

## 固定文案

语言包文件为 LanguageData/Chinese.xml、English.xml、Español.xml。文案结构为 `语言/窗体名/文案键/@ComponentText`。

启动页面和按需创建的页面共用 `RegisterLanguage()`；按需页面使用 `RegisterAndApplyLanguage()` 立即应用当前语言。固定控件使用稳定的 Name，支持 Label、Button、CheckBox、TabPage、UiLabel、FlatButton 和项目 IO 控件等。DataGridView 和 ListView 表头的键为 `表格Name_列Name`，ToolStrip 使用项目 Name。固定 FieldBox 占位文字使用 `控件Name_Placeholder`。

固定控件及布局维护在 Designer.cs。已有固定列的绑定表格设置 AutoGenerateColumns=false，绑定后重新登记表头。复用页由可在设计器中预览的 UserControl 承载。

Flow_BaseForm 内的流程子选项卡使用 `Name_FlowPage_源码标题SHA256前16位` 语言键，初始化/自动两个固定阶段页保持原键。底稿在首次登记、应用译文之前捕获并随控件生命周期缓存，重登及切换语言不会把译文当成新底稿。新建页面复用已删 tabPage 名称时不继承旧页面译文；未改变的源码标题若匹配任一旧语言条目，可迁移该组现场译文到新键。旧条目保留，语言编辑仍通过统一主保存落盘。

FieldBox 内部的提示 Label、Flow_Chart 内部按钮、PropertyGrid 内部编辑控件和自管 DeviceControl 不重复扫描，避免内部重名控件互相覆盖。

## 动态文案和数据

计数、状态、当前型号、相机 SN 数量、窗口标题副本等动态内容使用 `_dyn` 后缀，订阅 LanguageChanged 刷新；Dispose 时解除静态事件订阅。

消息、错误和动态格式串使用 `MiddleLayer.LangMsg(form, key, zh, en, es)` 或 `UiTheme.T()`，提供三语底稿。读取时尊重现场语言包，缺少的条目才补写底稿。普通 LangText 只读取，适合已随程序发布的固定语言键。

包含业务名称的提示使用固定占位符，例如“已添加托盘 {0}”，取出译文后再用 string.Format 插入名称。不要把某次输入的托盘名、配方名或设备名写进语言底稿。

现场自定义语言目录缺少固定条目时，使用程序目录下 LanguageData 中对应语言的默认译文补齐内存视图，现场已有译文优先。语言设置页也显示合并后的完整条目；读取默认译文不会覆盖现场文件，保存时才写入编辑结果。

以下内容保留原值：账号、用户名称、权限枚举代码、相机和 VPP 名称、SN、路径、模块身份、参数值、补偿项目键以及生产记录的业务列键。权限通过显示格式化翻译，数据库仍存 Operator、Maintenance、Administrator 等代码。ModuleBaseForm 的 Text 和 Name 保留模块身份，可见标题用独立控件翻译。

VPP 补偿表中的内置“角度”键通过 CellFormatting 显示为 Angle / Ángulo，配置和接口仍使用“角度”。编辑未改动的显示名称时还原原始键，包括编辑期间切换语言的情况。用户自定义项目名称保持原样。

切换语言只改显示文字，不重新绑定数据源、不重建参数行、不提交或取消当前编辑。历史日志和厂商异常详情属于原始数据，不改写其落盘内容。Cognex 编辑器等厂商内部界面由对应 SDK 的资源决定。

## 语言设置与保存

LanguageCatalog 负责语言文件读取和保存，页面只管理编辑状态。三语按“窗体名＋文案键”合并，不按行号对齐；排序、过滤和各语言条目顺序不同不会影响保存目标。

保存先生成全部临时文件再替换正式文件，失败时恢复已替换文件。保存成功调用 ReloadLanguageData，清空 XML 缓存并刷新固定控件、表头、菜单和动态页面。主框架的状态和前缀缓存也随通知失效，即使当前语言没有改变也会刷新。后台错误提示和编辑保存共用语言文件锁。

项目文件包含三份界面语言包和三份报警表，构建时复制到输出目录；现场自定义目录仍由 MachineSetup.ini 控制。

## 报警语言

报警目录由 `PathSetup/AlarmTableDirectory` 指定，默认为工作目录下的 AlarmTable。文件名是 Chinese.xml、English.xml、Spanish.xml。Español.xml 仅作为旧文件名兼容读取，不作为新报警表的保存目标。

运行内核、首页报警解析和报警设置页共用 AlarmCatalog 的路径映射。运行表从中文主表取得 Type、DoStop，只从所选语言表替换 Content，避免切换语言改变停机规则。首页按文件路径和修改时间缓存报警内容，保存后可以重新读取。

原始报警定义仍有以下待核对项，未自动重编号或删除记录：

- A4014 在中文主表中重复，分别描述扫码枪连接失败和 PLC 写入失败。
- 程序引用 4、11、12、4015、7020，但当前报警表缺少相应编号。
- 西班牙文原表独有 A3041，内容为“Español”，缺少其他语言的原始含义。
- A4004、A4010、A4012 的 DoStop 在原始各语言文件中不一致。运行时统一使用中文主表，原始文件中的配置保留供核对。

## 验证

`diagnostics/validate_localization.py` 校验三语键集合、重复键、空译文、英西文残留中文、格式占位符和换行；同时列出报警定义待核对项。

`tests/VisionRuntime/Run.ps1` 使用隔离语言目录执行视觉和语言回归，包括 30 个窗体的三语登记、绑定表头、菜单、权限显示、排序保存、缓存刷新、并发补写、失败回滚、编辑值保护和报警规则不随语言改变。语言设置页生成中文、英文、西班牙文截图供核对。

离线验证不连接相机和 PLC，也不代替设备运行状态、多 DPI 显示和厂商编辑器的现场检查。
