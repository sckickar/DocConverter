using System;
using System.ComponentModel;

namespace SkiaSharp;

[EditorBrowsable(EditorBrowsableState.Never)]
[Obsolete("Use SKColorSpaceTransferFn instead.")]
public enum SKNamedGamma
{
	Linear,
	Srgb,
	TwoDotTwoCurve,
	NonStandard
}
