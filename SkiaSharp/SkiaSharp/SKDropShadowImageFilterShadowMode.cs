using System;
using System.ComponentModel;

namespace SkiaSharp;

[EditorBrowsable(EditorBrowsableState.Never)]
[Obsolete("Use CreateDropShadow or CreateDropShadowOnly instead.")]
public enum SKDropShadowImageFilterShadowMode
{
	DrawShadowAndForeground,
	DrawShadowOnly
}
