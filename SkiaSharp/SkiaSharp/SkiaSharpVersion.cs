using System;
using System.Runtime.InteropServices;

namespace SkiaSharp;

public static class SkiaSharpVersion
{
	private static readonly Version Zero = new Version(0, 0);

	private static Version nativeMinimum;

	private static Version nativeVersion;

	public static Version NativeMinimum => nativeMinimum ?? (nativeMinimum = new Version(88, 1));

	public static Version Native
	{
		get
		{
			try
			{
				return nativeVersion ?? (nativeVersion = new Version(SkiaApi.sk_version_get_milestone(), SkiaApi.sk_version_get_increment()));
			}
			catch (EntryPointNotFoundException)
			{
				return nativeVersion ?? (nativeVersion = Zero);
			}
		}
	}

	internal unsafe static string NativeString => Marshal.PtrToStringAnsi((IntPtr)SkiaApi.sk_version_get_string());

	public static bool CheckNativeLibraryCompatible(bool throwIfIncompatible = false)
	{
		return CheckNativeLibraryCompatible(NativeMinimum, Native, throwIfIncompatible);
	}

	internal static bool CheckNativeLibraryCompatible(Version minSupported, Version current, bool throwIfIncompatible = false)
	{
		if ((object)minSupported == null)
		{
			minSupported = Zero;
		}
		if ((object)current == null)
		{
			current = Zero;
		}
		if (minSupported <= Zero)
		{
			return true;
		}
		Version version = new Version(minSupported.Major + 1, 0);
		if (current <= Zero)
		{
			if (throwIfIncompatible)
			{
				throw new InvalidOperationException($"The version of the native libSkiaSharp library is incompatible with this version of SkiaSharp. Supported versions of the native libSkiaSharp library are in the range [{minSupported.ToString(2)}, {version.ToString(2)}).");
			}
			return false;
		}
		bool flag = current < minSupported || current >= version;
		if (flag && throwIfIncompatible)
		{
			throw new InvalidOperationException($"The version of the native libSkiaSharp library ({current.ToString(2)}) is incompatible with this version of SkiaSharp. Supported versions of the native libSkiaSharp library are in the range [{minSupported.ToString(2)}, {version.ToString(2)}).");
		}
		return !flag;
	}
}
