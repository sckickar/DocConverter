using System;
using System.ComponentModel;

namespace SkiaSharp;

[EditorBrowsable(EditorBrowsableState.Never)]
[Flags]
[Obsolete]
public enum GRBackendTextureDescFlags
{
	None = 0,
	RenderTarget = 1
}
