# 视觉运行回归测试

测试针对项目真实构建产物，不连接相机或 PLC。需要本机已安装的 .NET Framework 4.8、Visual Studio MSBuild 和 Cognex VisionPro。

在项目根目录的 Visual Studio Developer PowerShell 中构建到独立目录，避免覆盖现有运行程序：

```powershell
MSBuild.exe AlphaRap/AlphaRap.csproj /t:Build /p:Configuration=Debug /p:Platform=x64 /p:OutputPath=../.vp-validation/build/bin/ /p:IntermediateOutputPath=../.vp-validation/build/obj/ /v:minimal
./tests/VisionRuntime/Run.ps1 -AssemblyDirectory ./.vp-validation/build/bin
```

测试覆盖运行结果快照、重复触发、运行期间拒绝加载/保存/标定、超时与取消的迟到结果、算法 NG 和异常、VPP 缺失与损坏、同步保存、保存失败保留原文件、编辑放弃处理、非有限数值、补偿超限、标定保存失败回滚，以及按列名读取配方判据。

生产绑定测试覆盖持久化 ID、明确绑定丢失时禁止回退、配方和输出契约检查、必需补偿限制、内置工位兼容、XML 往返、重复 ID 拒绝、旧配置迁移、改名保留路径、存储目录校验，以及实际 VPP 页的勾选回填与保存。标定测试覆盖配方、相机 SN、目标集合与算法版本变化后的失效判定。

语言测试使用隔离的临时语言目录，覆盖 30 个窗体的三语登记、固定控件、绑定表头、工具栏、权限显示、排序后按键保存、当前语言缓存刷新、并发补写、保存失败保护、未提交编辑值保护、现场翻译优先、缺键使用随附语言包、业务名称排除翻译，以及事件订阅释放。报警测试覆盖配置目录、Spanish.xml 映射、文本缓存更新和停机规则保持不变。当前共有 62 个测试场景。

补偿表测试覆盖独立读取快照、补偿值完整精度保存、旧配置默认值、非法编辑还原、其他列精度保护，以及角度名称在编辑期间切换语言后仍保留原始业务键。另有真实内存算法运行确认补偿值不会自动加入视觉结果；流程调用说明见 `docs/VisionCompensation.md`。补偿页三语截图为 compensation-vpp-Chinese.png、compensation-vpp-English.png、compensation-vpp-Español.png。

语言设置页的三语截图保存在构建输出目录，文件名为 language-settings-Chinese.png、language-settings-English.png、language-settings-Español.png。截图通过离屏窗口生成，不连接相机或 PLC。多 DPI 和真实设备运行仍需现场验证。

运行 `python diagnostics/validate_localization.py` 检查语言包键集合、占位符和翻译遗漏；语言开发规则及尚需确认的报警编号见 `docs/Localization.md`。

在 VPP 页勾选“生产定位”即可选择生产算法；取消勾选表示切回内置兼容工位。绑定只在设备与视觉工位空闲、当前 VPP 已加载且校验通过时保存。动态生产工位使用本页 X、Y、角度限制；内置兼容工位继续使用旧配方判据。

相机和 VPP 显示名称可以修改，存储目录通过 XML 的 Folder 属性保持不变。旧配置读取时以原名称作为目录，保存时补齐持久化 ID 和目录属性，不移动现有算法文件。

超时只使本次结果失效。SDK 任务真正退出前，工位保持忙碌，不允许下一次拍照或算法修改。

标定发布记录关联当前配方、相机 SN、目标工位 ID 与已保存 VPP 的 SHA-256 摘要。缺少上下文的旧记录保留点位，但需要重新应用标定；改变配方、相机、目标集合或算法版本后也需要重新应用。

标定保存回滚目前以单个工位为单位；多工位应用部分失败时，发布记录保持未应用状态并禁止相应动态工位生产。相机采集仍由现有 VPP 内部工具负责，统一采集调度和跨工位标定事务仍需后续实现。

硬件联调需要验证实际 VPP 采集来源、曝光、PLC 数据类型、完成标志时序以及停止后的流程恢复行为。离线测试不替代这些检查。

MiddleLayer 测试覆盖窗体重复创建、初始化/停止/暂停状态、周期回调隔离、复位失败停止、全部三语文案索引一致性及扫描线程退出。PERF 日志对比 10 万次 XPath 与索引查找，功能划分和调用约定见 `docs/MiddleLayer.md`。
