# PM 任务板

> 规则见 `.cursor/rules/pm-cursor-workflow.mdc`
> 状态：`TODO` / `DOING` / `待验收` / `DONE` / `返工` / `待老板定`
> **每条都要有可复现的证据**（实际命令 + 实际输出），PM 会独立复核，不看结论只看证据。

---

## A. 待老板拍板

| # | 事项 | 为什么需要你定 |
|---|---|---|
| A1 | 谁来调 `HomeF.AddInfoRow(...)`？ | 现在**没有任何代码调用它** ⇒ 不产生数据文件。需定数据来源：扫码流程 / Gantry / MES / 定时读 PLC |
| A2 | 日志页要不要补「生产数据路径」输入框？ | `ProducPath` 在 `Path` 表里存在，但日志页只有 User/Run/Alarm 三个框，默认会被塞成 `C:\Log` |
| A3 | 「生产信息」表格的真实列名与列数？ | 现用你给的参考表 7 列占位（`Drver`/`Preasure` 已按正确拼写写成 `Driver`/`Pressure`，要跟旧程序一致就改回来） |
| A4 | 「生产数据查询」窗口要不要给标准 `.Designer.cs`？ | 现在是纯代码构建（列由查询结果动态生成）。VS 能打开、能看，但**不能拖控件** |

---

## B. 来自 `AlphaRap-代码评审与优化建议-20261014.md`（PM 已逐条取证）

| # | 事项 | PM 复核结论 | 状态 |
|---|---|---|---|
| B1 | `WindowChrome` 右下角缩放柄坏了 | **已复现 + 已修 + 已复验**。实测原状：`grip.Bounds={0,0,22,20}`、期望 `{578,480}`、`被内容面板盖住=True`；修后：初值 `{578,480}` ✓、拉大到 900×640 → `{878,620}` ✓、`仍在最前=True`、内容面板 `{0,46,900,594}`（Dock 未被破坏） | `DONE` |
| B2 | 全局异常处理完全缺失 | **成立**。全工程搜不到 `Application.ThreadException` / `UnhandledException` / `SetUnhandledExceptionMode`；`Program.cs` 只有 `SetCompatibleTextRenderingDefault` | `TODO` |
| B3 | 仓库提交了 `bin/`，`.git` 过大 | **成立**。实测 `.git = 491 MB`，`AlphaRap/bin` 下 **225 个文件入库**。这正是反复出现「VS 生成删掉已入库 DLL ⇒ 每次提交前得 `git checkout` 还原」的根因 | `TODO` |
| B4 | 报警线程 `while(true)` 不认 `gEXIT`，且跨线程改 UI | **两条全部成立**。① `Tasks/AlarmRunTask.cs` 是 `while (true)` + `Thread.Sleep(10)`，`gEXIT` 出现 **0 次** ⇒ 退出时线程不停；② 该线程（`new Thread(AlarmRunWork)`）在 `AlarmRun()` 里**直接操作 UI**：`MainF.WarnningMessage.BeginUpdate()/Items.Clear()/Items.Add()`、`EndUpdate()`、`MainF.UpdateAlarmFilterCount()`，而**全文件 0 处 `Invoke`/`BeginInvoke`** ⇒ 确凿的跨线程 UI 访问 | `TODO` |
| B5 | `AlarmQueryForm` 与 `WindowChrome` 两套窗口框架重复 | **同意**。建议保留 `WindowChrome`（框架级、可复用），把 `AlarmQueryForm` 并过去；顺带把它的 `RoundCard()` 提到 `UiKit` 共用。<br>⚠ 验收要求：迁移前后**渲染逐位对比**，像素一致才算过 | `待老板定` |

---

## C. 本会话已完成（PM 已验收）

| 提交 | 内容 | 验收证据 |
|---|---|---|
| `5f342c8` | 报警统计查询窗口现代化改版 | 渲染 + 对比度精算 |
| `1ad8ded` | 禁用态与次要文字对比度 | 对比度由源码常量精算 |
| `b3a27dd` | 图标与文字拆开渲染、标签去加粗 | 真实渲染前后对照，墨迹量测 |
| `8f738f4` | 禁用小锁角标、标签 11→11.5pt | 真实渲染 + 最长标签不截断验证 |
| `45fbfeb` | 卡片层次（投影 + 页面底色压深） | 三级亮度数值 + 真实渲染 |
| `41c78f0` | 报警窗口卡片统一 `CardPanel` | 投影像素采样 |
| `38bb244` | 「生产信息」改代码驱动数据表 | 真实渲染按真实卡片宽度 |
| `bd73bd4` | 加宽「生产信息」卡容纳 6~7 列 | 真实渲染 759px 下 7 列完整 |
| `f39543c` | 生产数据本地保存 + 按时间段查询 | 存储层真自测（跨天/时段/关键字/逗号转义/导出）+ 窗体真实渲染 |
| `be54fdd` | 设计器与运行 EXE 一致 | 强制设计期渲染三个窗体 |
| `671aac5` | 查询窗口融入框架风格（抽出 `WindowChrome`） | 与报警窗口并排渲染对照 |
| `4c821de` | 机台状态徽章整块填状态色 | 5 态真实渲染 + WCAG 对比度精算（5.34~10.40:1） |
| `79ea205` | 同步 VS 回写的 `HomeForm.Designer.cs` | `git status` 取证 |

---

## D. 工作方式备忘（给下一个接手的会话）

- 每步都要：**真实渲染/量化验证 → 编译 0 错 0 警 → 跑静态检查 → 提交推送 → 写记忆**
- 有疑问先**取证再下结论**：本板的 B1 就是这么发现并修掉 PM 自己写的 bug 的
- 遇到无法整屏渲染的窗体（`MainForm` / `HomeForm`）：用 `galleryprobe` 按**真实画法**渲染局部 + 单独精算数值，并**明确说明这是局部复刻、不是整屏截图**

---

## E. 员工（Cursor）在飞的改动 —— PM 复核

复核时工作区出现**大量未提交 WIP**，并且已有 5 个新提交
（`2382bcb` / `700930f` / `691016b` / `7c59200` / `eeb1e8d`，20:12~20:36，Vision 页面 / 托盘表 / VPForm）。
PM **没有**把这些 WIP 一起提交 —— 那是员工的进行中工作。

| # | 事项 | 证据（可复现） | 结论 |
|---|---|---|---|
| E1 | `Vision/VpVppPage.Designer.cs` 遗留两个**死控件** `foot` / `optionsRow` | `check_orphan_controls.py` 报「总计 2 处孤儿控件」（L33 / L45）；代码注释写着「按钮已并入 paramRow，不再单独占行」「已并入 pathRow」，却用 `Visible = false` + `Size = (0,0)` 糊着 | **返工**：连字段声明、`new`、`Suspend/ResumeLayout` 一起**删掉**，别用隐藏糊过去 |
| E2 | `AlphaRap.csproj` 出现 **2597 行** diff | `git diff --stat` 2597 行，但 `git diff --ignore-cr-at-eol --stat` 只剩 **16 增 / 1 删** ⇒ **整文件行尾被改成了 LF** | **返工**：csproj 行尾统一回 **CRLF**，否则以后每次提交都带 2500 行噪音，review 没法看 |
| E3 | 并发编辑风险（两个代理同改一个仓库） | 员工的 WIP 与 PM 的改动同时在工作区 | **流程**：提交前必须 `git status` 并**只 add 自己的文件**（PM 本次即如此）。**不要 `git add -A`** |
| E4 | 提交里带 VS 回写的 `*.Designer.cs` | `700930f` 提交信息即「HomeForm 设计器去掉未使用的 components 容器」 | 已按红线处理（VS 回写一并提交，不回退）✅ |

### PM 本次自己的改动（单独提交，不含员工 WIP）

| 文件 | 内容 |
|---|---|
| `AlphaRap/Controls/WindowChrome.cs` | 修 B1 缩放柄：显式 `Location` + `BringToFront`；探针复验 PASS |
| `.cursor/rules/pm-cursor-workflow.mdc` | 分工与验收流程（规则文件） |
| `.cursor/PM-TASKBOARD.md` | 本任务板 |

---

# F. 已派发任务（执行：Cursor；PM 只验收）

## T-001 修「VPForm 的参数在程序启动时没有被加载」 ★最高优先

**状态：`TODO`（已派发，等 Cursor 交证据）**

### 目标
程序启动完成后，**不点任何东西**，VPForm（视觉参数窗口）的参数就应处于已加载状态。
现在必须**点一下该窗口**才加载上来。

### 现象
- 原话：「该 form 存在 bug：为什么程序加载的时候默认参数没有加载上来？点击一下该 form 再上传下来。」
- 点开之后界面是正常的：相机名 / CameraIndex / 默认曝光、标定页（已录点数、vpp 路径）、
  VPP 页（相机名 / VPP 名 / vpp 路径 / 补偿限制表 X、Y、R = -1~1、-1~1、-5~5 / 曝光 / 存图勾选 / 存图路径）都在。
  ⇒ 缺的是**启动那一刻**没加载。

### PM 已核实的线索（可直接用，不必重查）

| 事实 | 证据 |
|---|---|
| 启动流程：`Program.Main` → `MiddleLayer.InitialProject()` | `Program.cs:31`，在 `Application.Run(MainF)` **之前**，加载期间显示 LoadingForm |
| 窗体在此时被创建：`VPF = CreateForm(VPF, "VPForm")` | `MiddleLayer.cs:177`；`CreateForm` 会**立刻**调 `ModuleInitialize`（`MiddleLayer.cs:958-959`） |
| `VPForm : ModuleBaseForm`，其 `ModuleInitialize` 里调 `BuildVpUi()` | `VPForm.cs:133-147`；`BuildVpUi` 在 `VPForm.cs:177-186`<br>⇒ **不是**等用户打开页面才建 |
| `SysPara.RecipeName` 在建 VPF **之前**已赋值 | `MiddleLayer.cs:130` < `177` ⇒ vpp 路径里的配方名理论上是对的 |
| `VPForm.OnVisibleChanged` → `OnRecipeChanged()` | `VPForm.cs:1221-1228` |

### ⚠ PM 未核实的三个怀疑点（**必须由你证实或推翻，不许猜**）

1. **`_vpUiBuilt` 在干活之前就置 true**（`VPForm.cs:179-180`）：
   若 `CreateVpToolBar()` / `LoadConfigFromDisk()` / `BuildVpPages()` / `ApplyPageStyle()` 任一抛异常，
   `ModuleInitialize` 的 catch 只记 `_vpBuildError` 就吞掉了，而 `_vpUiBuilt` 已是 `true`
   ⇒ **永不重建**，界面停在半成品。这能解释"点一下之后才对"（点开时另一条路径刷新了）。
2. **`OnRecipeChanged()` 做的事可能比 `BuildVpUi()` 多**（例如按当前配方 `LoadTB` / 刷新 vpp 路径）
   ⇒ 启动路径漏了那一步，而"点开一次"正好补上。
3. **`SysPara.LanguageShow` 在 L186-191 才赋值**（`SwitchLanguage` 在 L193），
   而 VPF 在 L177 就建好了 ⇒ 建窗时语言还是初值。
   （PM 判断它只影响文案、不该影响参数 —— 但**请以实测为准**。）

### 要求

1. **先定位，再动手**。用临时日志把 `BuildVpUi()` 每一步的进入/完成/异常打出来（原样保留，别只写结论）。
   对照 `OnRecipeChanged()` 与 `BuildVpUi()` 的差异，找出「启动路径漏掉/失败的那一步」。
2. **修根因**：让启动路径加载到的参数与"点开一次之后"完全一致。
   若确认是"异常被吞" ⇒ 别吞；`_vpUiBuilt` 只在**成功**后置 true（或失败可重试）。
3. **禁止**用「打开页面时再加载一次」糊过去 —— 那只是把 bug 挪个地方。
4. 遵守本项目红线：UTF-8 BOM + CRLF（改完 `.cs` 跑 `.workbuddy\tools\_norm_sources.py`）、
   方法不套方法、设计器与运行期一致。

### 验收标准（PM 会逐条独立复核）

1. 启动后**不点任何东西**，上述所有参数与**点开一次之后**逐项一致。
2. 附**启动期日志**（`BuildVpUi()` 每步 + 任何异常原文）。
3. 附**对照证据**：启动后未点击时的截图 + 点击后的截图（或探针渲染），参数区一致。
4. 编译 **0 错误 / 0 警告**；`check_orphan_controls.py` / `check_control_names.py` / `check_cs_edits.py` / `_scan_dup.py` 全绿。
5. 提交并推送 `NewPowerSW/AlphaRap2.08`；提交信息写清**根因**（哪一行、为什么）；更新 `.workbuddy/memory/` 当日日志。

### 边界
- 本任务由 **Cursor 实现并自证**；**PM 只做独立复核，不代写修复代码**。
- 若定位后发现根因与上面三个怀疑点都不同 —— 以你的实测为准，但要在交付里写清证据链。


