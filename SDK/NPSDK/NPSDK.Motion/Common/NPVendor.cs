namespace NPSDK.Motion
{
	/// <summary>
	/// 厂商。Adlink/Leadshine = 板卡（含轴）；Keyence、Omron = 仅 PLC IO。
	/// </summary>
	public enum NPVendor
	{
		Adlink = 0,
		Leadshine = 1,
		Keyence = 2,
		Omron = 3
	}
}
