using System;

namespace DocGen.OfficeChart;

[Flags]
internal enum ExcelIgnoreError
{
	None = 0,
	EvaluateToError = 1,
	EmptyCellReferences = 2,
	NumberAsText = 4,
	OmittedCells = 8,
	InconsistentFormula = 0x10,
	TextDate = 0x20,
	UnlockedFormulaCells = 0x40,
	All = 0x7F
}
