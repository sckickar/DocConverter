using System;

namespace SkiaSharp;

[Flags]
public enum SKCropRectFlags
{
	HasNone = 0,
	HasLeft = 1,
	HasTop = 2,
	HasWidth = 4,
	HasHeight = 8,
	HasAll = 0xF
}
