using System;
using System.Runtime.InteropServices;

public class GetTickCountEx
{
	private bool isPerfCounterSupported = false;

	public long frequency = 0L;

	private const string lib = "kernel32.dll";

	public long Value
	{
		get
		{
			long count = 0L;
			if (isPerfCounterSupported)
			{
				QueryPerformanceCounter(ref count);
			}
			else
			{
				count = Environment.TickCount;
			}
			double num = (double)count * 1000.0 / (double)frequency;
			return (long)num;
		}
	}

	[DllImport("kernel32.dll")]
	private static extern int QueryPerformanceCounter(ref long count);

	[DllImport("kernel32.dll")]
	private static extern int QueryPerformanceFrequency(ref long frequency);

	public GetTickCountEx()
	{
		if (QueryPerformanceFrequency(ref frequency) != 0 && frequency != 1000)
		{
			isPerfCounterSupported = true;
		}
		else
		{
			frequency = 1000L;
		}
	}
}
