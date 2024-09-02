using System;
using System.ComponentModel;

namespace SkiaSharp;

[EditorBrowsable(EditorBrowsableState.Never)]
[Obsolete("Use SKEncodedOrigin instead.")]
public enum SKCodecOrigin
{
	TopLeft = 1,
	TopRight,
	BottomRight,
	BottomLeft,
	LeftTop,
	RightTop,
	RightBottom,
	LeftBottom
}
