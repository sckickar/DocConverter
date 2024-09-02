using System;

namespace SkiaSharp;

public abstract class SKStreamRewindable : SKStream
{
	internal SKStreamRewindable(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}
}
