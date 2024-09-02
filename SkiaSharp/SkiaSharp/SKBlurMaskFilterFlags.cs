using System;
using System.ComponentModel;

namespace SkiaSharp;

[EditorBrowsable(EditorBrowsableState.Never)]
[Flags]
[Obsolete]
public enum SKBlurMaskFilterFlags
{
	None = 0,
	IgnoreTransform = 1,
	HighQuality = 2,
	All = 3
}
