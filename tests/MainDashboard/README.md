# 主框架与首页回归

先编译实际解决方案，再执行：

```powershell
./tests/MainDashboard/Run.ps1 -AssemblyDirectory '.vp-validation/dashboard-redesign-build/bin'
```

检查 1600×1000、1440×900、1280×800 的中文、英文、西班牙文布局；验证顶部命令、全高导航、KPI 数值与空数据、报警原控件位置以及保存/初始化/启动/暂停/停止/清除报警原事件。

测试通过 LicenseContext 设计态构造窗体，显式解绑 MainForm_Load 后显示离线测试窗口。设备清单、生产数量及表格示例仅在测试实例中填充，不操作设备、不保存机器配置。

截图输出到 `.vp-validation/main-dashboard/<运行目录>`，需检查文字裁切、图表刻度和卡片间距。Windows GUI 沙箱限制窗口创建时，应在允许桌面句柄的测试环境执行。
