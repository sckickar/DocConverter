using System;
using System.ComponentModel;

namespace SkiaSharp;

[EditorBrowsable(EditorBrowsableState.Never)]
[Obsolete]
[Flags]
public enum SKColorSpaceFlags
{
	None = 0,
	NonLinearBlending = 1
}
