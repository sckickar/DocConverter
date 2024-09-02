using System;

namespace SkiaSharp;

public abstract class SKStreamSeekable : SKStreamRewindable
{
	internal SKStreamSeekable(IntPtr handle, bool owns)
		: base(handle, owns)
	{
	}
}
