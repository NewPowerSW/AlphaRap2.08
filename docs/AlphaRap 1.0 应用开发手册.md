# AlphaRap 1.0 应用开发手册

适合第一次接触 AlphaRap 的 C# 开发人员。打开 `AlphaRap1.0.sln` 后，可以从第一个定时任务开始练习，再按工位需求选用 IO、点位、设备、视觉、日志和 MES 范例。界面按钮和操作员开机流程另见[新手使用手册](AlphaRap%201.0%20新手使用手册.md)。

## 目录

- [01 建立第一个可调试的 C# 文件](#01-建立第一个可调试的-c-文件)
- [02 从哪里找到代码入口](#02-从哪里找到代码入口)
- [03 配置名称和代码名称](#03-配置名称和代码名称)
- [04 输入输出与气缸](#04-输入输出与气缸)
- [05 轴与配方点位](#05-轴与配方点位)
- [06 Setting Recipe 和产品数据](#06-setting-recipe-和产品数据)
- [07 扫码枪与 PLC](#07-扫码枪与-plc)
- [08 从代码调用相机 VPP](#08-从代码调用相机-vpp)
- [09 工站节点如何接入](#09-工站节点如何接入)
- [10 产品记录与运行日志](#10-产品记录与运行日志)
- [11 AlwaysRun 与报警](#11-alwaysrun-与报警)
- [12 MES 调用与离线测试](#12-mes-调用与离线测试)
- [13 保存 换型和多语言](#13-保存-换型和多语言)
- [14 断点与交付前验证](#14-断点与交付前验证)

## 01 建立第一个可调试的 C# 文件

1. 用 Visual Studio 打开 `AlphaRap1.0.sln`。在解决方案资源管理器中右击 **AlphaRap** 项目，选【设为启动项目】。工具栏选择 `Debug | x64`。
2. 先执行【生成 → 生成解决方案】。本工程目标为 .NET Framework 4.8，构建需要项目引用的 SDK 和 VisionPro 等依赖。
3. 右击 `AlphaRap/Tasks` 文件夹，选【添加 → 类】，命名 `TrainingHeartbeat.cs`，把内容替换成下方范例。这个操作会把文件纳入旧式 `.csproj` 的编译列表。如果你在资源管理器中复制文件，请回到 VS 用【添加 → 现有项】。每个范例开头都标有文件名，复制该代码块后用原样的命名空间编译。
4. 再生成一次，在 `Debug.WriteLine` 上打断点。通过 VS【输出】窗口的【调试】频道也能看到该消息。

### 范例 01 每秒执行一次自定义代码

**文件** `AlphaRap/Tasks/TrainingHeartbeat.cs`。保存、编译后由 AlphaRap 应用程序集自动发现。

```csharp
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AlphaRap
{
    internal sealed class TrainingHeartbeat : IAlwaysRunTask
    {
        public int IntervalMs => 1000;
        public string[] InputNames => Array.Empty<string>();

        /// <summary>向调试输出写入一次刷新时间。</summary>
        public Task RefreshAsync(AlwaysRunContext context, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            Debug.WriteLine("TrainingHeartbeat " + DateTime.Now.ToString("HH:mm:ss"));
            return Task.CompletedTask;
        }
    }
}
```

这里的 `1000` 是毫秒。任务从一轮结束起再等待这个间隔。`InputNames` 为空，因为这一例没有读取输入。实现类由 [AlwaysRunTasks](../AlphaRap/Tasks/AlwaysRunTasks.cs) 发现并调用，有无参构造函数即可。当前版本在程序加载阶段启动这类任务，仿真模式跳过执行。只需练习编辑和编译时，可以停在生成成功。进入真机模式启动应用可能初始化设备甚至使能伺服，运行断点只在获准的调试条件下进行。

## 02 从哪里找到代码入口

- **AlphaRap** 放机型和客户代码。业务练习代码加到这个项目。`TaskProcess` 是上料、下料和 Gantry 工位，`Tasks` 是后台刷新，`Devices` 和 `PLC` 是实际设备驱动，`MES` 是客户接口。
- **AlphaRap.Framework** 放可复用的配置、界面和资源访问。业务代码常用的入口是 [ApplicationHost.cs](../AlphaRap.Framework/Core/ApplicationHost.cs)。例如 `ApplicationHost.GetInput("IB_MaterialLow")` 返回输入对象，再调用对象的 `ReadState()`。
- **NPSDK** 放轴、IO、气缸和已有工站执行器。想找 `GetPos()`、`On()`、`ResultType` 的定义时，从调用处按 `F12` 跳转。

从 [Program.cs](../AlphaRap/Program.cs) 进入应用后，[StationApplication.cs](../AlphaRap/StationApplication.cs) 决定要创建的页面和工站，[MiddleLayer.Startup.cs](../AlphaRap/Core/MiddleLayer.Startup.cs) 负责初始化和加载配方。要改 **每片板做什么**，从对应工站的节点方法找起。要加 **开机后持续看的诊断项**，实现 `IAlwaysRunTask`。要改 **布局**，找对应 WinForms 页及其设计器文件。

## 03 配置名称和代码名称

1. 先在界面建资源并填唯一名称，完成顶部主【保存】。下文 `IB_MaterialLow`、`OB_TestLamp`、`StopCylinder`、`Cam_Check`、`LocatePart`、`InspectLog` 均为示例名称。
2. 代码填已保存的 `Name`，界面翻译来自 `Text` 和语言 XML。比如资源名 `IB_MaterialLow` 在中英文菜单都能用同一个字符串访问。
3. 资源查到后，还要判断设备的连接和本次执行结果。配置存在、能够连接、产生有效值是三项不同的检查。

用法速查：

- `GetInput(name)` → `NP_Input`，调用 `ReadState()` 读取可空电平。新后台监视走 `context.ReadInput(name)`。
- `GetOutput(name)` → `NP_Output`，调用 `IsOn()`、`On()`、`Off()`。在批准的工站动作里使用写入方法。
- `GetCylinder(name)` → 就绪的 `NP_Cylinder`，调用 `On()`、`Off()` 并按工站要求核对到位。
- `GetAxis(name)` → `NP_Motor`，调用 `GetPos()`、`GetBusy()`。`GetAxisPoints(name)` 返回已提交的点位副本。
- `GetDevice<实际类>(name)` → 已配置的设备，调用该驱动提供的方法。同名设备可加第二个参数 `panelKey`。
- `GetCamera(cameraName)` → 相机配置。`GetVpp(cameraName, vppName)` → 视觉工位。任一查不到时返回 `null`。
- `GetConstant<T>(scope, key)` → 已保存的 `Setting` 或 `Recipe` 值。`ApplicationHost.Data.WriteLogByName(name, values)` → 写入记录并返回路径。

常用查找失败有 `KeyNotFoundException`（名称不存在）、`InvalidOperationException`（名称存在多个候选，或某些资源未就绪）、`InvalidCastException`（设备类型或常量类型不匹配）。资源替换或配方切换后，下一件产品开始时重新按名称取对象或快照。

## 04 输入输出与气缸

**先配置**在【手动 → IO 资源】添加输入、输出或气缸组合，保存名称和正确的设备地址。气缸绑定已有输出、互锁和到位信号，动作方向按接线和工艺确认。

### 范例 02 读取输入电平和试验指示灯

**文件** `AlphaRap/TaskProcess/IoTraining.cs`。两个方法都要由你的诊断入口显式调用。`ReadMaterialLevel()` 返回 SDK 读值，适合核对驱动和地址。业务需要按「反转有效电平」解释时，使用第 11 章的 `context.ReadInput()`。`SetTestLamp()` 只用于现场批准的非动作试验灯。

```csharp
using System;
using NPSDK.Motion;

namespace AlphaRap
{
    internal static class IoTraining
    {
        /// <summary>读缺料输入的驱动电平，驱动无有效读数时返回 null。</summary>
        internal static bool? ReadMaterialLevel()
        {
            NP_Input input = ApplicationHost.GetInput("IB_MaterialLow");
            return input.ReadState();
        }

        /// <summary>在获准的真机调试操作中设置独立的试验指示灯。</summary>
        internal static void SetTestLamp(bool on)
        {
            if (SysPara.Simulation) return;

            NP_Output lamp = ApplicationHost.GetOutput("OB_TestLamp");
            if (!lamp.IsReady) throw new InvalidOperationException("试验指示灯未就绪");

            if (on) lamp.On();
            else lamp.Off();
        }
    }
}
```

- `bool?` 能表达 `true`、`false`、`null`。`null` 表示没有可信读数。工站等待输入时分别处理这三种情况，尤其是等待信号断开的节点。
- 输出 `On/Off` 返回 `void`。动作确认用工艺要求的反馈信号。气缸 `On/Off` 返回 `bool`，仍应根据现场接法检查到位和超时。
- 上料和下料现有代码使用 `StationApplication.Io.Read(key)`、`Output(key)`、`Cylinder(key)`。这里传的是已有工站键，可在 [StationIoDefaults.xml](../AlphaRap/TaskProcess/StationIoDefaults.xml) 核对。新增独立资源可按 `Name` 访问。若要由整机【初始化】统一连接某个新的工站必需 IO，检查 [StationIo.Initialize](../AlphaRap/TaskProcess/StationIo.cs) 的工站键来源、就绪与停止处理。`GetInput/GetOutput` 本身只取实例。

## 05 轴与配方点位

### 范例 03 读取 X1 的 P1 和当前坐标

**文件** `AlphaRap/TaskProcess/AxisTraining.cs`。先在【硬件】确认 `X1` 是已保存的轴。当前型号下录入或核对 `P1` 并主保存。`P1` 对应索引 `0`，`P2` 对应 `1`。读取实时坐标前先通过现有工站流程准备相应轴驱动。

```csharp
using System;
using NPSDK.Motion;

namespace AlphaRap
{
    internal static class AxisTraining
    {
        /// <summary>取得当前已提交配方的 P1 位置。</summary>
        internal static double ReadP1()
        {
            var points = ApplicationHost.GetAxisPoints("X1");
            if (!points.Positions.TryGetValue(0, out double position))
                throw new InvalidOperationException("X1 的 P1 未保存");
            return position;
        }

        /// <summary>读取已准备好的 X1 轴当前位置，单位取决于该轴配置。</summary>
        internal static double ReadCurrentPosition()
        {
            NP_Motor axis = ApplicationHost.GetAxis("X1");
            if (!axis.IsReady) throw new InvalidOperationException("X1 轴未就绪");

            return axis.GetPos();
        }
    }
}
```

`GetAxisPoints` 是当前已提交配方的快照。改返回的字典不会保存点位，也不会带动轴。现有流程用 `NP_Motor.Goto(position)` 发起定位，再检查运动状态和到位条件。需要新增动作时，在已经具备回零、限位、互锁和停止处理的工站动作中接入。先用 `ReadP1()` 验证换型后读到的目标。底层轴未就绪时 `GetPos()` 可能返回 0，不能用 0 或 `GetBusy() == false` 判定到位。

## 06 Setting Recipe 和产品数据

### 范例 04 按当前配方判定一条测量值

**文件** `AlphaRap/TaskProcess/MeasurementTraining.cs`。在顶部【系统】的参数页建立 `Recipe / CheckLength` 与 `Recipe / CheckTolerance`，均选 `double`，例如 `12.5`、`0.2`，保存到练习配方。下例的长度与允许偏差单位必须一致。

```csharp
using System;

namespace AlphaRap
{
    internal static class MeasurementTraining
    {
        /// <summary>按已保存的配方长度和允许偏差判定本件测量值。</summary>
        internal static bool IsWithinRecipe(double actual)
        {
            if (double.IsNaN(actual) || double.IsInfinity(actual)) return false;

            double target = ApplicationHost.GetConstant<double>("Recipe", "CheckLength");
            double tolerance = ApplicationHost.GetConstant<double>("Recipe", "CheckTolerance");
            if (double.IsNaN(target) || double.IsInfinity(target) ||
                double.IsNaN(tolerance) || double.IsInfinity(tolerance) || tolerance < 0)
                throw new InvalidOperationException("请检查配方的标准长度和允许偏差");

            return Math.Abs(actual - target) <= tolerance;
        }
    }
}
```

调用 `MeasurementTraining.IsWithinRecipe(12.6)` 应得到 `true`，传入 `12.8` 应得到 `false`。`Setting` 用于整机共用值。两种作用域支持 `int`、`double`、`bool`、`string`，首次创建时选定类型，取值的泛型要一致。已有页面专用的旧字段可按 [ModuleBaseForm](../AlphaRap.Framework/Core/ModuleBaseForm.cs) 的 `GetSettingValue(table, column, rowIndex)` 和 `GetRecipeValue(...)` 读取。

每件产品的条码、测量值、判定应随着本件传给下一步，例如第 10 章用三个实参传递。多工位并行时让工站持有自己的本件记录，必要时查阅已有 [BoardInfo](../AlphaRap/StationData.cs)。`StationData` 中的初始化完成标志是整机运行状态，`Setting` 和 `Recipe` 是配置，两者都不适合充当每件产品的临时变量。

## 07 扫码枪与 PLC

### 范例 05 对已经登记的设备扫码一次

**文件** `AlphaRap/Devices/ScanTraining.cs`。在设备页确认配置名 `KeyenceScann`、类型 `ScannerKeyenceTcp`、地址和超时。由整机既有流程连接设备。调用方收到产品触发后只发起一次请求，等待现有驱动返回。

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AlphaRap
{
    internal static class ScanTraining
    {
        /// <summary>读取一次已登记扫码枪的结果，失败交给调用方处理。</summary>
        internal static async Task<string> ReadBarcodeAsync(CancellationToken token)
        {
            if (SysPara.Simulation)
                throw new InvalidOperationException("仿真中请由调用方传入练习条码");

            var scanner = ApplicationHost.GetDevice<ScannerKeyenceTcp>("KeyenceScann");
            ScannerResult result = await scanner.ScanOnceAsync(token).ConfigureAwait(false);
            return result.RequireValue(token);
        }
    }
}
```

`RequireValue` 成功才返回条码，取消和通信失败会以异常交给调用方。排障找 `ScannerResult.Status`、`Message` 和 `CommandMayHaveExecuted`。如果结果不明，先核对设备应答与本件状态，再决定是否重扫。打开自定义设备类时，可从 [ScannerKeyenceTcp](../AlphaRap/Devices/ScannerKeyenceTcp.cs) 学习连接、停止、结果三个入口。需支持其他厂商时，将该厂商类放在 `Devices`，核对 [DeviceControl](../AlphaRap.Framework/Controls/DeviceControl.cs) 加载设备类型和持久化属性的约定。

### 范例 06 读取已连接的汇川 PLC 诊断值

**文件** `AlphaRap/PLC/PlcReadTraining.cs`。本站 `INOVANCE` 是按连接号复用会话的访问对象，默认 `0`。确认 Gantry 或你的机型已经连接相同编号。传入的只读地址、长度和类型来自现场 PLC 点表。

```csharp
using System;
using System.Threading;
using AlphaRap.PLC;

namespace AlphaRap
{
    internal static class PlcReadTraining
    {
        /// <summary>读取一个诊断地址并返回本次完整通信结果。</summary>
        internal static PlcResult<string> ReadDiagnostic(string address, int count, string type,
            CancellationToken token)
        {
            if (SysPara.Simulation)
                throw new InvalidOperationException("仿真请使用离线诊断数据");

            var plc = new INOVANCE(0);
            return plc.ReadPlcResult(address, count, type, timeoutMs: 500, token: token);
        }
    }
}
```

调用方先查 `Succeeded`，再读 `Value`。失败时记录 `Status` 和 `Message`，分别排查未连接、超时和驱动异常。旧 `ReadPlc` 的特殊失败字符串容易被当作工艺量，新代码使用带状态的接口。定时读 PLC 时，在一次 `RefreshAsync` 中调用一次读方法并传入取消令牌，间隔应适合设备负载。

## 08 从代码调用相机 VPP

### 范例 07 在限时内取得本次定位结果

**文件** `AlphaRap/TaskProcess/LocateTraining.cs`。在视觉页配置 `Cam_Check / LocatePart`，用【编辑 TB】训练和测试，保存当前型号的 VPP。此例要求 VPP 输出可用于定位的 `x`、`y`、`A`。在工站自己的处理线程或能异步等待的入口调用，传入本件条码和取消令牌。

```csharp
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AlphaRap
{
    internal static class LocateTraining
    {
        /// <summary>触发一次定位并返回同一运行编号的终止状态与数据。</summary>
        internal static async Task<VisionRunResult> LocateAsync(string barcode, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            VpStation station = ApplicationHost.GetVpp("Cam_Check", "LocatePart");
            if (station == null)
                throw new InvalidOperationException("找不到 Cam_Check / LocatePart");

            if (!station.TryRunTB(out long runId, barcode))
                throw new InvalidOperationException(station.LastError);

            var elapsed = Stopwatch.StartNew();
            try
            {
                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    VisionRunResult result = station.LastRunResult;
                    if (result == null || result.RunId != runId)
                        throw new InvalidOperationException("视觉运行编号不一致");

                    if (result.State != VisionRunState.Running) return result;
                    if (elapsed.ElapsedMilliseconds >= 3000) station.TimeoutRun(runId);
                    await Task.Delay(20, token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                station.CancelRun(runId);
                throw;
            }
        }
    }
}
```

- `TryRunTB` 返回 `true` 表示请求被接受。调用方用 `result.State == VisionRunState.Succeeded` 判定成功，再取 `X/Y/Angle`。NG、出错和超时分别查 `State`、`Error`。`RunId` 用于避免上一件的结果被用到本件。
- 使用自定义 TB 时，按端口名称取本次结果，先核对算法分支会不会发布定位坐标。坐标单位由 TB 输出定义。只有在已应用的标定和已验证的 TB 输出下，才可把视觉坐标用于机械计算。角度字段是否有效取决于定位特征和工具配置。
- 已有 Gantry 用两个同步节点分别启动、等待，比在 `FlowChartRun` 内阻塞等拍照更适合当前执行器。参照 [Gantry.cs](../AlphaRap/TaskProcess/Gantry.cs) 中 `FlowAuto_Vison3_2_FlowChartRun` 与 `FlowAuto_Vison3_3_FlowChartRun`。此范例的 `TryRunTB` 路径把结果和存图分开，启用存图时按 `SaveLastImage(result.RunId)` 保存同一请求的照片，并检查返回路径。
- TB 的连接、九点和旋转标定继续用视觉页和 TB 编辑。先应用当前型号、相机对应的九点结果，再做旋转标定。圆心和两线交点的精定位及旋转采样见[内置视觉算法](VisionBuiltInAlgorithms.md)。

## 09 工站节点如何接入

工站已有的 `FlowChartRun` 是同步回调。用 Visual Studio 在 [DownConveryor.cs](../AlphaRap/TaskProcess/DownConveryor.cs) 找 `DownConveyorAuto_Flow1_6_FlowChartRun`，再在其 [Designer 文件](../AlphaRap/TaskProcess/DownConveryor.Designer.cs) 查同名方法的事件绑定。进入节点时，执行器反复调用同步方法直到它返回可走的分支或流程停止。

- `NEXT` 走已连的下一节点。`IDLE` 留在当前节点，下次调度重新检查。`CASE1` 等返回值依赖设计器已有的对应分支连线。
- 节点的动作与等待适合分开。例如一个节点发一次扫码或拍照请求，下一节点查询本次结果，用 `IDLE` 等待。
- 修改节点连线或新加工位前，先对照现有工站的 `ICompiledFlowEntries.InitialNodes`、`AutoNodes`，以及 `StationApplication.CreateStations()`。运行界面显示已编译工站，代码保存后要重新生成。

### 范例 08 复现一个只读的工站等待节点

**文件** `AlphaRap/TaskProcess/StationWaitTraining.cs`。这是参照下料实际节点整理的可调用范例。先在代码中打断点阅读同一工站键的真实节点。需要接入自有节点时，从已有的 `…_FlowChartRun()` 调用 `StationWaitTraining.WaitForUpstreamBoard()`，并沿用工站的超时和报警出口。

```csharp
using Result = NPSDK.Flow_Chart.ResultType;

namespace AlphaRap.TaskProcess
{
    internal static class StationWaitTraining
    {
        /// <summary>确认上游有板后进入下一节点，读不到或无板时等待。</summary>
        internal static Result WaitForUpstreamBoard()
        {
            bool? hasBoard = StationApplication.Io?.Read("IB_DownConveyor_UpMachineAvailable_SMEMA");
            return hasBoard == true ? Result.NEXT : Result.IDLE;
        }
    }
}
```

排查一直停留时看 `hasBoard` 是否为 `null`。已初始化仍为 `null` 时，查工站键、地址待配置、驱动状态和仿真状态。状态是 `false` 时，按上游实际信号排查。涉及不可无限等待的工序时，用现有节点 `FlowChart_ElapsedMilliseconds` 和项目已登记的报警编号实现超时。

## 10 产品记录与运行日志

### 范例 09 保存一件产品的最终结果

**文件** `AlphaRap/TaskProcess/InspectionRecordTraining.cs`。在【数据 → 日志模块】新增 `InspectLog`，建立 `Barcode (Text)`、`MeasureValue (Number)`、`Result (Text)`。`Time` 由模块自动加。希望首页看到这些字段时，给这个模块勾选【用作首页生产记录】并主保存。

```csharp
using System;
using System.Collections.Generic;

namespace AlphaRap
{
    internal static class InspectionRecordTraining
    {
        /// <summary>为本件产品追加一条记录并返回实际写入文件。</summary>
        internal static string Save(string barcode, double measurement, bool passed)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                throw new ArgumentException("需要本件产品的条码", nameof(barcode));
            if (double.IsNaN(measurement) || double.IsInfinity(measurement))
                throw new ArgumentOutOfRangeException(nameof(measurement));

            return ApplicationHost.Data.WriteLogByName("InspectLog", new Dictionary<string, object>
            {
                ["Barcode"] = barcode,
                ["MeasureValue"] = measurement,
                ["Result"] = passed ? "OK" : "NG"
            });
        }
    }
}
```

**从哪里调用**　放到本件的完工步骤。应用把本件条码、实测值、最终判定传入。验证时，在准备好的验证环境里调用 `InspectionRecordTraining.Save("SAMPLE-001", 12.6, true)` 一次，用返回路径查文件，确认三列正确，再到首页或数据页查询。仿真时文件写入配置目录下的 `Simulation`。

`WriteLogByName` 每调用一次追加一行。发生异常时保留本件数据并走工站失败处理。让保存成功后才推进到下一流程，避免 `IDLE` 轮询重复写入、同一件产品重进完工步骤，或未经核实就重试。首页只绑定一个客户日志模块，表头来自该模块，成功写入才更新首页记录。首页总产量、节拍和良率是独立业务统计，写入一行日志不会自动代替统计。机型已有的产量加计入口可在 [MainForm.cs](../AlphaRap/MainForm.cs) 查找 iProductOKAdd 和 iProductNGAdd。

**同一段代码的诊断信息**可以用 `ApplicationHost.Data.AddRunLog("...")`、`AddLogError("...")`。有操作者动作时用 `AddOperationLog("页面名", "动作描述")`。日志写入和页面报警列表分别由不同入口处理。

### 范例 10 串起配方判定和产品记录

**文件** `AlphaRap/TaskProcess/CompletedInspectionTraining.cs`。同一项目中需已添加范例 04 和范例 09。在已得到条码与实际测量值的完工节点调用一次，返回成功后再推进节点。

```csharp
namespace AlphaRap
{
    internal static class CompletedInspectionTraining
    {
        /// <summary>按当前练习配方判定本件产品并保存最终结果。</summary>
        internal static string Finish(string barcode, double measurement)
        {
            bool passed = MeasurementTraining.IsWithinRecipe(measurement);
            return InspectionRecordTraining.Save(barcode, measurement, passed);
        }
    }
}
```

在隔离的练习配置中调用 `CompletedInspectionTraining.Finish("SAMPLE-001", 12.6)`。若标准长度是 `12.5`、允许偏差是 `0.2`，应保存一行 OK。另用新条码传入 `12.8`，应保存一行 NG。这里的 NG 是测量超差。测量设备失败而没有有效值时，工站先处理设备故障，再决定是否需要另存失败记录。

## 11 AlwaysRun 与报警

新建一个独立刷新类就能写一套自己的处理逻辑。多个任务分别调度，实例字段跨轮保留。同一个任务上一轮结束后才排下一轮。仿真、程序退出及配置保存忙碌时会跳过或停止调度。把读写限制在当轮，支持取消的驱动调用传入 `token`。后台任务直接修改 WinForms 控件会跨线程，界面更新应沿用界面的线程切换入口。

### 范例 11 缺料变化时写运行日志和普通提醒

**文件** `AlphaRap/Tasks/MaterialAlarmTraining.cs`。配置 `IB_MaterialLow`，保存地址与有效电平。在报警设置选一个新编号，例如 `7301`，维护文本和类型 `W`，并确认该编号当前配置为无停机动作且已加载。在【初始化】准备好 IO 后测试有料、缺料、断开和恢复。

```csharp
using System.Threading;
using System.Threading.Tasks;
using NPSDK;

namespace AlphaRap
{
    internal sealed class MaterialAlarmTraining : IAlwaysRunTask
    {
        private const string InputName = "IB_MaterialLow";
        private const string AlarmCode = "7301";
        private bool? previous;

        public int IntervalMs => 250;
        public string[] InputNames => new[] { InputName };

        /// <summary>缺料时登记普通提醒，状态变化时记录读数恢复或失败。</summary>
        public Task RefreshAsync(AlwaysRunContext context, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            bool? current = context.ReadInput(InputName);
            if (previous == current) return Task.CompletedTask;

            if (current == true) Alarm.Show(AlarmCode);
            if (current == false) ApplicationHost.Data?.AddRunLog("料位正常");
            if (current == null) ApplicationHost.Data?.AddLogError("缺料输入读数不可用");

            previous = current;
            return Task.CompletedTask;
        }
    }
}
```

1. `InputNames` 告诉工站初始化要准备哪些输入。`RefreshAsync` 用同一个名称读取有效状态。
2. 样例记录有效状态的变化。初始 `null` 和持续 `null` 不会每 250 ms 写日志。第一次有效缺料会走报警，恢复有料会写运行日志。
3. `Alarm.Show(code)` 使用已加载的报警表编号。配置不存在时不会产生条目。上方练习不自动清报警，按项目规定确认与清除。
4. 急停、气源、安全许可的整机判断在 [AlwaysRunTask.cs](../AlphaRap/Tasks/AlwaysRunTask.cs) 的 `ScanSafety`、`CheckSafetyBeforeRun`。`3025` 是现有软件运行许可报警。安全项目继续沿用原有硬件安全回路及整机停止和复位逻辑。

## 12 MES 调用与离线测试

本站由 [StationApplication.CreateMesPage](../AlphaRap/StationApplication.cs) 选择 [LENS](../AlphaRap/MES/LENS.cs)。当前接口会从页面配置取得列名、工序等信息，公开方法返回客户应答的业务判定，`Post_Message` 保留最近应答。其中有依赖类内部本件字段的方法，例如 `Integrated_Transit_Interface()` 使用本件 SN 和客户采集字段。现场扩展时，从对应客户类建立明确的单件入参，并按客户协议在工站的进站或完工步骤串行调用。核对 `DisMes`、离线策略、回执和重复提交处理。

### 范例 12 离线验证本件判定交给 MES 的方式

**文件** `AlphaRap/MES/MesTraining.cs`。此例使用内存委托模拟客户调用，运行 `MesTraining.VerifyOffline()` 可验证成功及客户拒绝两个分支，不访问网络。要接实际客户接口时，把 `send` 替换成你的客户适配方法，由它用本次输入构造请求并读取业务回执。

```csharp
using System;

namespace AlphaRap.MES
{
    internal static class MesTraining
    {
        /// <summary>把已确认的单件结果交给客户适配方法并返回业务判定。</summary>
        internal static bool Complete(string barcode, bool passed, Func<string, string, bool> send)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                throw new ArgumentException("需要本件产品的条码", nameof(barcode));
            if (send == null) throw new ArgumentNullException(nameof(send));

            string result = passed ? "OK" : "NG";
            return send(barcode, result);
        }

        /// <summary>在无网络的条件下检查成功和拒绝两种结果。</summary>
        internal static void VerifyOffline()
        {
            bool accepted = Complete("SAMPLE-001", true,
                (barcode, result) => barcode == "SAMPLE-001" && result == "OK");
            bool rejected = Complete("SAMPLE-002", false, (barcode, result) => false);

            if (!accepted || rejected)
                throw new InvalidOperationException("MES 离线验证失败");
        }
    }
}
```

工站应能分别追查本地日志保存和 MES 应答。请求超时后，先按条码与工单查是否已过站，避免重复提交。客户字段和放行策略以项目协议为准。练习接口中的 `OK/NG` 代表本项目传给适配器的判定，实际上传格式由适配器转换。

## 13 保存 换型和多语言

- **机器级**　轴和 IO 资源在设置目录的 `HardwareResources.xml`，设备在 `DeviceControl.xml`，视觉资源配置在 `VPForm.Cameras.xml`，日志模块定义在 `DataForm.LogModules.xml`。这些文件用于备份和排查，先确认运行目录与实际设置目录，可从 `SysPara.SettingDataDirectory` 核对。
- **配方级**　当前型号文件由 `SysPara.FilePath` 指示，包含对应的点位、配方常量等。视觉 TB 以实际目录和型号加载。仍由 `ProductManagerForm` 切换，保存或处理草稿后再核对新型号。
- **保存入口**　资源编辑完成后的【确定】通常更新内存草稿。用顶部主【保存】提交配置。业务日志是逐件自动落盘。新增可配置功能前，查看 [ProjectConfiguration](../AlphaRap.Framework/Configuration/ProjectConfiguration.cs) 的模块保存与配方切换路径。
- **语言**　应用页面用 `ApplicationHost.RegisterLanguage(control, formName)` 接入当前语言表，提示可以用 `ApplicationHost.Text(formName, key, fallback)`。中、英、西语底稿按工程现有 [LanguageData](../LanguageData) 维护。动态资源显示用既有 `Text` 和相应语言条目，代码仍使用业务名称和字段键。

## 14 断点与交付前验证

1. **断点没进**　先在解决方案确认项目已经包含文件，配置为 `Debug | x64`。在 VS【调试 → 窗口 → 模块】核对 `AlphaRap.exe` 的加载路径和符号。入口断点打在需要重复执行的 `RefreshAsync` 或节点回调。构造方法通常只在创建对象时执行一次。
2. **AlwaysRun 无输出**　检查仿真模式、任务类是否在 AlphaRap 应用程序集、是否有可用的无参构造、`IntervalMs` 是否大于零。打开【运行日志】按任务类名查异常。
3. **读到 null**　检查 `InputNames` 中的业务名、地址、主保存、是否要求重启、整机 IO 准备和驱动连接。`context.ReadInput` 使用已配置的反相规则。
4. **修改保存后结果仍旧**　确认执行目录、当前配方和 `SysPara.FilePath`，再看是否仍持有换型前的对象或点位快照。VPP 要确认实际加载的型号文件与运行编号。
5. **按接入顺序验收**　先生成，再用纯逻辑或模拟结果核对流程分支，接着用验证样品核对日志字段和状态。连接设备后再逐项核对超时、断线、停止、复位和下一件清理。涉及运动、IO 输出、标定或客户上传的操作由现场负责人验收。

如果只能记住一条开发顺序，先在界面定义唯一名称并主保存，在应用项目里按名称取资源，检查本次执行的结果，再推进到下一个工站步骤。
