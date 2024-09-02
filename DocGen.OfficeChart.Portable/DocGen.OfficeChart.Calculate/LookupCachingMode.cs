using System;

namespace DocGen.OfficeChart.Calculate;

[Flags]
internal enum LookupCachingMode
{
	None = 0,
	VLOOKUP = 1,
	HLOOKUP = 2,
	Both = 3,
	OptimizeForMatches = 4
}
