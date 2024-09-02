using System;

namespace DocGen.OfficeChart;

[Flags]
internal enum OfficeSkipExtRecords
{
	None = 0,
	Macros = 1,
	Drawings = 2,
	SummaryInfo = 4,
	CopySubstreams = 0x10,
	All = 0x17
}
