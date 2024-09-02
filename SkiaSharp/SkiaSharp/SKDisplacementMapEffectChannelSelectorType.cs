using System;
using System.ComponentModel;

namespace SkiaSharp;

[EditorBrowsable(EditorBrowsableState.Never)]
[Obsolete("Use SKColorChannel instead.")]
public enum SKDisplacementMapEffectChannelSelectorType
{
	Unknown,
	R,
	G,
	B,
	A
}
