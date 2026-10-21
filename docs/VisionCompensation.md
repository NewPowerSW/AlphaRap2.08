# 补偿配置表接口

VPP 页的补偿表包含项目（Item）、补偿值（Compensation）、下限（Min）、上限（Max）。补偿值默认 0，编辑完成后与上下限一起保存到机器配置 `VPForm.Cameras.xml`，不随配方变化。旧配置缺少 `Compensation` 属性时按 0 读取。

应用流程通过以下方法获取指定相机和 VPP 已提交的数据：

```csharp
List<VpCompLimitItem> table = MiddleLayer.VPF.GetCompensationTable("TakePCBA", "VPP1");
if (table == null)
    throw new InvalidOperationException("Camera or VPP configuration was not found.");

foreach (VpCompLimitItem row in table)
{
    string item = row.Item;
    double compensation = row.Compensation;
    double min = row.Min;
    double max = row.Max;
    // 应用流程自行决定补偿的使用方式、上下限判断及处理顺序。
}
```

接口返回独立快照；修改列表及其中的行不会写回界面或配置。相机或 VPP 不存在时返回 null，存在但没有配置行时返回空列表。接口只读取已提交的配置，不提交界面里尚未结束的编辑，不需要先拍照，也不会运行算法。

`Item` 始终为原始业务键，默认是 `X`、`Y`、`角度`。语言切换只影响表头和内置角度项目的显示，不改变接口返回的键、数值或用户自定义名称。

框架只负责编辑、保存和读取 Compensation；不会自动把它加入视觉输出、坐标、运动目标或超限计算。应用者在自己的流程中明确读取并使用，新增补偿值不会改变现有拍照结果。
