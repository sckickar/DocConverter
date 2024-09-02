using System;

namespace DocGen.OfficeChart;

[Flags]
internal enum ExcelAutoFormatOptions
{
	Number = 1,
	Border = 2,
	Font = 4,
	Patterns = 8,
	Alignment = 0x10,
	Width_Height = 0x20,
	None = 0,
	All = 0x3F
}
