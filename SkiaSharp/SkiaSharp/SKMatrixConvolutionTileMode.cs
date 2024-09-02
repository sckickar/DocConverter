using System;
using System.ComponentModel;

namespace SkiaSharp;

[EditorBrowsable(EditorBrowsableState.Never)]
[Obsolete("Use SKShaderTileMode instead.")]
public enum SKMatrixConvolutionTileMode
{
	Clamp,
	Repeat,
	ClampToBlack
}
