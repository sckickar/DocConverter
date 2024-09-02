using System;

namespace DocGen.Chart;

[Flags]
internal enum ChartPaintFlags
{
	Background = 1,
	Border = 2,
	InteractiveCursors = 8,
	Axes = 0x16,
	All = 0x1F
}
