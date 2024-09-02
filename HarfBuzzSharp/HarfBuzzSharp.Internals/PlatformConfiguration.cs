using System;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp.Internals;

public static class PlatformConfiguration
{
	private const string LibCLibrary = "libc";

	private static string linuxFlavor;

	private static readonly Lazy<bool> isGlibcLazy;

	public static bool IsUnix { get; }

	public static bool IsWindows { get; }

	public static bool IsMac { get; }

	public static bool IsLinux { get; }

	public static bool IsArm { get; }

	public static bool Is64Bit { get; }

	public static string LinuxFlavor
	{
		get
		{
			if (!IsLinux)
			{
				return null;
			}
			if (!string.IsNullOrEmpty(linuxFlavor))
			{
				return linuxFlavor;
			}
			if (!IsGlibc)
			{
				return "musl";
			}
			return null;
		}
		set
		{
			linuxFlavor = value;
		}
	}

	public static bool IsGlibc
	{
		get
		{
			if (IsLinux)
			{
				return isGlibcLazy.Value;
			}
			return false;
		}
	}

	static PlatformConfiguration()
	{
		isGlibcLazy = new Lazy<bool>(IsGlibcImplementation);
		IsMac = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
		IsLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
		IsUnix = IsMac || IsLinux;
		IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
		Architecture processArchitecture = RuntimeInformation.ProcessArchitecture;
		IsArm = processArchitecture == Architecture.Arm || processArchitecture == Architecture.Arm64;
		Is64Bit = IntPtr.Size == 8;
	}

	private static bool IsGlibcImplementation()
	{
		try
		{
			gnu_get_libc_version();
			return true;
		}
		catch (TypeLoadException)
		{
			return false;
		}
	}

	[DllImport("libc", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
	private static extern IntPtr gnu_get_libc_version();
}
