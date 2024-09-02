using System;

namespace SkiaSharp;

public abstract class SKStreamMemory : SKStreamAsset
{
	internal SKStreamMemory(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}
}
