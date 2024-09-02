using System;

namespace DocGen.OfficeChart;

[Flags]
internal enum OfficeSheetProtection
{
	None = 0,
	Objects = 1,
	Scenarios = 2,
	FormattingCells = 4,
	FormattingColumns = 8,
	FormattingRows = 0x10,
	InsertingColumns = 0x20,
	InsertingRows = 0x40,
	InsertingHyperlinks = 0x80,
	DeletingColumns = 0x100,
	DeletingRows = 0x200,
	LockedCells = 0x400,
	Sorting = 0x800,
	Filtering = 0x1000,
	UsingPivotTables = 0x2000,
	UnLockedCells = 0x4000,
	Content = 0x8000,
	All = 0xFFFF
}
