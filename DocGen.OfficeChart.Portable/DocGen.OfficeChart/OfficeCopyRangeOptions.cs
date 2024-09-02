using System;

namespace DocGen.OfficeChart;

[Flags]
internal enum OfficeCopyRangeOptions
{
	None = 0,
	UpdateFormulas = 1,
	UpdateMerges = 2,
	CopyStyles = 4,
	CopyShapes = 8,
	CopyErrorIndicators = 0x10,
	CopyConditionalFormats = 0x20,
	CopyDataValidations = 0x40,
	All = 0x7F
}
