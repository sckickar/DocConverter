using System;

namespace DocGen.OfficeChart;

[Flags]
internal enum OfficeParseFormulaOptions
{
	None = 0,
	RootLevel = 1,
	InArray = 2,
	InName = 4,
	ParseOperand = 8,
	ParseComplexOperand = 0x10,
	UseR1C1 = 0x20
}
