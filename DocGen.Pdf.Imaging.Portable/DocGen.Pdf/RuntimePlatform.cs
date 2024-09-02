using System.Runtime.InteropServices;

namespace DocGen.Pdf;

internal static class RuntimePlatform
{
	internal static bool IsNonWindows
	{
		get
		{
			if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
			}
			return true;
		}
	}
}
