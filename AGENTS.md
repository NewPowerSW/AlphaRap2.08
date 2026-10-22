# 项目编码约定

编写、修改或审查本项目代码前，读取并遵循 `.cursor/rules/csharp-standards.mdc`。
其中的增删改查可读性、主流程平铺和减少嵌套要求适用于本项目所有增删改查代码。

修改 WinForms 界面时，同时遵循 `.cursor/rules/winforms-designer-consistency.mdc`，最大程度保持设计器预览与运行后的 EXE 一致。

所有新增或更新功能同时遵循 `.cursor/rules/feature-integration.mdc`：沿用语言切换和 ProductManagerForm 配方切换，配置保存统一接入 MainForm Save 按钮。

每次修改完成后，遵循 `.cursor/rules/github-delivery.mdc`：完成必要验证，提交并推送 GitHub，并清楚说明本次更新内容、验证结果、分支及提交位置。
