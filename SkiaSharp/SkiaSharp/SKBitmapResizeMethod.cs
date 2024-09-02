using System;
using System.ComponentModel;

namespace SkiaSharp;

[EditorBrowsable(EditorBrowsableState.Never)]
[Obsolete]
public enum SKBitmapResizeMethod
{
	Box,
	Triangle,
	Lanczos3,
	Hamming,
	Mitchell
}
