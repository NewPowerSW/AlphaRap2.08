public class JTimer
{
	private GetTickCountEx tick = new GetTickCountEx();

	private long StartTM;

	public void Restart()
	{
		StartTM = tick.Value;
	}

	public bool IsOn(long millisecond)
	{
		long num = tick.Value - StartTM;
		if (num >= millisecond)
		{
			return true;
		}
		return false;
	}

	public long GetTime()
	{
		return tick.Value - StartTM;
	}
}
