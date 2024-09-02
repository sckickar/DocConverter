using System;

namespace SkiaSharp;

[Flags]
public enum SKPathMeasureMatrixFlags
{
	GetPosition = 1,
	GetTangent = 2,
	GetPositionAndTangent = 3
}
