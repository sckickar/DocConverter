using System;

namespace DocGen.OfficeChart;

[Flags]
internal enum OfficeReadingOrderType
{
	Context = 0,
	LeftToRight = 1,
	RightToLeft = 2
}
