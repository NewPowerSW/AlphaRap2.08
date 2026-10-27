# 继承类并自动生成工具

**继承现有业务父类，实现普通公开业务方法，原页面创建实际实例后，方法就出现在工具箱。** 不必再为工具箱实现接口、编写动作清单或登记代码。

设备使用 `AbstractDevice`；输入、输出和轴使用 `NP_Input`、`NP_Output`、`NP_Motor`。框架只调用原页面使用的同一个对象，保留对象身份、语言、配方、统一保存和资源占用规则。新增业务方法属于【现有功能】，不是已经移除的脚本或代码模块。

## 一、给设备增加一个方法

下面以已有称重设备类为例。设备原有构造函数、`Open()`、`Close()`、`IsConnected` 和通信实现保持原样，只在派生类中添加普通方法：

```csharp
using System;

namespace AlphaRap
{
    public partial class ProjectScale : AbstractDevice
    {
        /// <summary>扣除零点偏移，返回本次产品的净重量。</summary>
        public double CorrectWeight(double rawWeight, double zeroOffset)
        {
            if (double.IsNaN(rawWeight) || double.IsInfinity(rawWeight))
                throw new ArgumentOutOfRangeException("rawWeight");

            if (double.IsNaN(zeroOffset) || double.IsInfinity(zeroOffset))
                throw new ArgumentOutOfRangeException("zeroOffset");

            return rawWeight - zeroOffset;
        }
    }
}
```

这个片段添加到已经实现设备基础成员的 `ProjectScale` 类；不是一个独立的串口或网口驱动。新建设备类仍需按 `AbstractDevice` 完成构造函数和连接成员，构造函数通常为 `public ProjectScale(string deviceName) : base(deviceName)`，由原 Device 页面建立实例。工具箱不会因为浏览类列表就自动创建或连接设备。

重新编译、启动程序，在原 Device 页面创建并配置 `ProjectScale` 的实际实例，例如“点胶称重仪”。进入流程编辑，工具箱【设备】中会出现这个实例的 `CorrectWeight`。已有实例则重新打开节点或刷新流程页面读取新方法目录。

拖入该工具后，节点自动得到：

- `rawWeight` 输入：绑定实际称重动作的结果。
- `zeroOffset` 输入：绑定 Setting 的零点偏移，或直接填固定数值。
- `Value` 输出：本次修正后的重量，下游判定及 DataForm 保存节点直接选择它。

方法参数是本次节点的输入，不必再通过实例字段或全局变量传递产品数据。算法上下限等型号参数绑定 Recipe；机器校准量绑定 Setting。型号切换仍只使用 ProductManagerForm。

## 二、给输入或输出控件增加业务方法

继承实际 IO 控件，添加公开方法即可。下面输入控件增加一个“无效状态”判断：

```csharp
using System;
using NPSDK.Motion;

namespace AlphaRap
{
    public class ProjectInput : NP_Input
    {
        /// <summary>读取此输入是否处于无效状态；驱动未就绪时报错。</summary>
        public bool IsInactive()
        {
            if (!IsReady)
                throw new InvalidOperationException("Input is not ready.");

            return !IsOn();
        }
    }
}
```

在原 IO 窗体中把对应输入使用的控件类型设为 `ProjectInput`，仍设置原有厂商、地址、Text 等属性。可以通过 Visual Studio 工具箱放置派生控件；不需要另建一套工具登记。只声明类，但页面实际控件仍为 `NP_Input`，不会凭空获得派生方法。

工具箱继续显示此输入的标准【读取输入】【等待输入稳定】，并增加 `IsInactive`。这个方法返回布尔 `Value`，NEXT 可以判断 `Value == true`。显示名称仍取实际控件 Text，内部 Name 保持原身份。

输出继承 `NP_Output` 的规则相同。普通公开业务方法自动增加，原【设置输出】【读取输出状态】继续可用。在原 IO 页面新增输入／输出，或右键现有动态资源【属性编辑】，通过【控件类型】选择已经编译加载的项目派生类；无需编写工具登记代码。输入只列出 `NP_Input` 的公开无参派生类，输出只列出 `NP_Output` 的公开无参派生类。

## 三、给轴控件增加业务方法

轴仍使用原页面的实际对象和它绑定的点位表。以下派生类只增加一个位置区间判断：

```csharp
using System;
using NPSDK.Motion;

namespace AlphaRap
{
    public class ProjectMotor : NP_Motor
    {
        /// <summary>判断本轴当前编码器位置是否位于指定区间。</summary>
        public bool IsWithin(double minimum, double maximum)
        {
            if (minimum > maximum)
                throw new ArgumentException("minimum must not exceed maximum.");

            if (!IsReady)
                throw new InvalidOperationException("Axis is not ready.");

            double position = GetEncPos();
            return position >= minimum && position <= maximum;
        }
    }
}
```

原轴页面使用 `ProjectMotor` 后，工具箱自动列出该轴的 `IsWithin`。参数 `minimum`、`maximum` 可以绑定当前配方，输出 `Value` 用于 NEXT／CASE。

原“移动到绑定表的点位”继续使用同一轴、绑定表和限位校验。框架不会额外将基础 `Goto(double)` 暴露为任意坐标工具，也不会混入其他轴的点位。项目自行添加的运动方法需自行实现其运动完成、限位和取消逻辑；继承本身不会把一个发送指令的方法自动变成“等待运动完成”。

在原轴页面新增轴，或右键现有动态轴【属性编辑】，通过【控件类型】选择项目派生类。列表来自已加载程序集内公开、非抽象且有公开无参构造函数的 `NP_Motor` 类型，不会在浏览时实例化或连接设备。保留【默认控件】即可继续使用标准轴。增加派生类不会自动替换现场已有轴，需明确选择所需类型。

【控件类型】和原厂商、地址、轴参数一同提交内存草稿，使用主界面【保存】持久化；下次加载创建同一类型。类型身份由程序集简单名称与完整类型名组成，不包含程序集版本。控件类型或程序集被移除时会明确报错，不能静默换成标准轴或 IO。更改已被流程引用的类型会被阻止；已经初始化的硬件参数变更仍须保存并重启。轴点位保持同一稳定资源 ID 和原配方表，不需要重新选择其他表。

派生控件新增的额外 CLR 属性暂不自动出现在该硬件参数表中，也不自动保存；产品参数通过方法输入绑定 `Recipe`，机器参数通过 `Setting` 或原有配置表维护。运行状态、驱动句柄等属性不会被当作配置反射落盘。构造函数仅建立对象，不应连接硬件或启动动作。

## 四、公开方法的参数和结果

自动发现针对业务派生层的 `public` 实例方法，包括中间业务父类的方法。方法重载按参数签名区分，重写同一方法不会产生一串重复工具。

已有显式 `GetFlowMethods` 的同一类也可继续添加普通业务方法；标准包装和配置辅助方法通过 `[FlowToolIgnore]` 精确排除。DataForm 和 INOVANCE PLC 的派生业务方法遵守同一规则，保留标准数据保存及 PLC 读写动作。

- 标量、枚举、一维标量／枚举数组参数：按原参数名生成输入；有默认值时保留默认值。数组输入需绑定实际数组数据，不能用固定值文本代替数组。
- 普通标量、枚举或数组返回值：生成 `Value` 输出，类型与方法返回值一致。数组不会自动拆成多个命名通道。
- `void`：没有返回值输出。它表示方法正常返回，不会自动生成业务合格结果。
- `Task`、`Task<T>`：等待任务结束；后者把 `T` 作为 `Value` 输出，不把 Task 对象传给下一节点。
- `CancellationToken` 参数：由运行器注入，不显示为需要应用工程师配置的输入。
- `ref` 参数：既是输入，也是执行后的同名输出；`out` 参数：仅生成执行后的同名输出。返回值使用 `Value`；如果与参数重名则使用 `ReturnValue`，仍重名时继续添加 `Return` 前缀。

例如：

```csharp
/// <summary>按产品的重量和允许范围判定，并输出实际采用的重量。</summary>
public bool CheckWeight(double weight, double minimum, double maximum,
    out double checkedWeight)
{
    checkedWeight = weight;
    return weight >= minimum && weight <= maximum;
}
```

该方法的输入是 `weight`、`minimum`、`maximum`，输出是布尔 `Value` 和数值 `checkedWeight`。

需要等待设备变化的异步方法应将 Token 传给等待和通信操作，并在合适位置检查取消。框架等待实际调用结束后才释放该对象的占用；同步阻塞通信不会因为节点超时就被强制终止。

静态方法、泛型方法、特殊成员、事件处理签名以及不支持的参数／返回类型不会自动进入工具箱。WinForms／Object 的界面生命周期、属性访问器和 SDK 标准接口也不会被当成业务动作全部铺开。目录发现只检查方法信息，不执行方法或读取任意属性。

## 五、名称和隐藏方法可选配置

不加任何标记时，方法标题和参数名按代码名称显示，业务名称不自动翻译。框架提供稳定语言键，现场可通过原语言表覆盖标题。

希望直接提供三语底稿时，可使用可选标记：

```csharp
using AlphaRap.FlowProgramming;

[FlowTool("重量判定", "Check weight", "Comprobar peso")]
public bool CheckWeight(double weight, double minimum, double maximum)
{
    return weight >= minimum && weight <= maximum;
}
```

只有项目内部调用、无需提供给应用工程师的方法可设为 `private`／`protected`。如果它必须保持公开，可加 `[FlowToolIgnore]`：

```csharp
[FlowToolIgnore]
public void ResetInternalCache()
{
    // 清理由此设备自己维护的缓存。
}
```

这些标记只改善显示和筛选，不要求为每个新工具再写元数据类或注册步骤。

## 六、保持现有工程和运行约定

同一实例的标准动作和派生业务动作共用资源租约，不能并发争用同一轴或 IO。异常进入原运行器的报警处理，业务不合格可返回 `false` 再由 CASE 分流。模拟运行不会调用没有独立模拟实现的真实派生业务方法。

节点主保存记录对象身份、方法身份、输入绑定和出口，不序列化实例。节点【确定】仅提交草稿，持久化仍使用主界面【保存】；新增实现需重新编译部署。方法或参数改名、变更签名、删除方法后，旧节点需重新核对绑定；不会用同名的另一个对象静默替换。

已有 `IFlowObject.GetFlowMethods()` 可用于专用字段标题、具名多通道输出、输入组合校验及独立模拟等高级需求，和自动发现合并使用；普通设备、轴及 IO 派生类不用实现它。

这里覆盖的是 `AbstractDevice` 与 NP 业务控件的派生方法。`MotorBaseInterface`／`IOBaseInterface` 厂商驱动由控件内部持有，驱动新增的方法尚不会自动穿透为控件工具。SDK 的 IO 组合支持公开绑定派生信号，并沿用手动输出权限及资源占用检查；框架没有将所有程序集内的任意类或 Form 公共方法暴露为流程节点。

使用入口和应用工程师操作顺序见 [FlowChat 功能工具箱](FlowToolboxUsage.md) 和 [设备流程设计器使用说明](FlowProgrammingUsage.md)。

复杂工位逻辑可以直接实现原生事件方法并进入本页工具箱，见 [原生代码 FlowChat](NativeFlowEvents.md)。
