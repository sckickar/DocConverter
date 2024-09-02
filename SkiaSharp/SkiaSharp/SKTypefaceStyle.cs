using System;
using System.ComponentModel;

namespace SkiaSharp;

[EditorBrowsable(EditorBrowsableState.Never)]
[Flags]
[Obsolete("Use SKFontStyleWeight and SKFontStyleSlant instead.")]
public enum SKTypefaceStyle
{
	Normal = 0,
	Bold = 1,
	Italic = 2,
	BoldItalic = 3
}
