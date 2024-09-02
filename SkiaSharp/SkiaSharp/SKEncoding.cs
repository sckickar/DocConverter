using System;
using System.ComponentModel;

namespace SkiaSharp;

[EditorBrowsable(EditorBrowsableState.Never)]
[Obsolete("Use SKTextEncoding instead.")]
public enum SKEncoding
{
	Utf8,
	Utf16,
	Utf32
}
