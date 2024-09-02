using System;

namespace SkiaSharp;

internal interface ISKReferenceCounted
{
	IntPtr Handle { get; }
}
