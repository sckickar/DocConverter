using System;

namespace SkiaSharp;

public abstract class SKXmlWriter : SKObject
{
	internal SKXmlWriter(IntPtr h, bool owns)
		: base(h, owns)
	{
	}
}
