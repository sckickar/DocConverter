using System;

namespace DocGen.OfficeChart;

[Flags]
internal enum OfficeWorksheetCopyFlags
{
	None = 0,
	ClearBefore = 1,
	CopyNames = 2,
	CopyCells = 4,
	CopyRowHeight = 8,
	CopyColumnHeight = 0x10,
	CopyOptions = 0x20,
	CopyMerges = 0x40,
	CopyShapes = 0x80,
	CopyConditionlFormats = 0x100,
	CopyAutoFilters = 0x200,
	CopyDataValidations = 0x400,
	CopyPageSetup = 0x800,
	CopyTables = 0xA00,
	CopyPivotTables = 0x1000,
	CopyPalette = 0x2000,
	CopyAll = 0x3FFF,
	CopyWithoutNames = 0x1FFD
}
