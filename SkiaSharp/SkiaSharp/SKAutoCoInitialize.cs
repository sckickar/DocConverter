using System;
using System.Runtime.InteropServices;
using SkiaSharp.Internals;

namespace SkiaSharp;

public class SKAutoCoInitialize : IDisposable
{
	private long hResult;

	private const long S_OK = 0L;

	private const long RPC_E_CHANGED_MODE = 2147549446L;

	private const uint COINIT_MULTITHREADED = 0u;

	private const uint COINIT_APARTMENTTHREADED = 2u;

	private const uint COINIT_DISABLE_OLE1DDE = 4u;

	private const uint COINIT_SPEED_OVER_MEMORY = 8u;

	public bool Initialized
	{
		get
		{
			if (hResult < 0)
			{
				return hResult == 2147549446u;
			}
			return true;
		}
	}

	public SKAutoCoInitialize()
	{
		if (PlatformConfiguration.IsWindows)
		{
			hResult = CoInitializeEx(IntPtr.Zero, 6u);
		}
		else
		{
			hResult = 0L;
		}
	}

	public void Uninitialize()
	{
		if (hResult >= 0)
		{
			if (PlatformConfiguration.IsWindows)
			{
				CoUninitialize();
			}
			hResult = -1L;
		}
	}

	public void Dispose()
	{
		Uninitialize();
	}

	[DllImport("ole32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, SetLastError = true)]
	private static extern long CoInitializeEx([Optional][In] IntPtr pvReserved, [In] uint dwCoInit);

	[DllImport("ole32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, SetLastError = true)]
	private static extern void CoUninitialize();
}
