using System;

namespace DocGen.OfficeChart;

[Flags]
internal enum OfficeParseOptions
{
	Default = 0,
	[Obsolete("This value is obsolete and won't affect on the XlsIO. It will be removed in next release. Sorry for inconvenience.")]
	SkipStyles = 1,
	DoNotParseCharts = 2,
	[Obsolete("This value is obsolete and won't affect on the XlsIO performance. It will be removed in next release. Sorry for inconvenience.")]
	StringsReadOnly = 4,
	DoNotParsePivotTable = 8,
	ParseWorksheetsOnDemand = 0x10
}
