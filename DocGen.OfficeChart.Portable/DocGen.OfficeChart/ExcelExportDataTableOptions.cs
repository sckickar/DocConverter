using System;

namespace DocGen.OfficeChart;

[Flags]
internal enum ExcelExportDataTableOptions
{
	None = 0,
	ColumnNames = 1,
	ComputedFormulaValues = 2,
	DetectColumnTypes = 4,
	DefaultStyleColumnTypes = 8,
	PreserveOleDate = 0x10
}
