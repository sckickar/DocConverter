using System;
using System.ComponentModel;

namespace SkiaSharp;

[EditorBrowsable(EditorBrowsableState.Never)]
[Obsolete("Use SKColorSpaceXyz instead.")]
public enum SKColorSpaceGamut
{
	AdobeRgb = 1,
	Dcip3D65 = 2,
	Rec2020 = 3,
	Srgb = 0
}
