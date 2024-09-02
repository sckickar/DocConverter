using System;
using System.ComponentModel;

namespace SkiaSharp;

[EditorBrowsable(EditorBrowsableState.Never)]
[Obsolete("Use SKColorSpaceTransferFn instead.")]
public enum SKColorSpaceRenderTargetGamma
{
	Linear,
	Srgb
}
