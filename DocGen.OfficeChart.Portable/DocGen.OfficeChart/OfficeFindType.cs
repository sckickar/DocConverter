using System;

namespace DocGen.OfficeChart;

[Flags]
internal enum OfficeFindType
{
	Text = 1,
	Formula = 2,
	FormulaStringValue = 4,
	Error = 8,
	Number = 0x10,
	FormulaValue = 0x20
}
