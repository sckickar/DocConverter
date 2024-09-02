using System;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[CLSCompliant(false)]
internal interface ISheetReference : IReference
{
	string BaseToString(FormulaUtil formulaUtil, int iRow, int iColumn, bool bR1C1);
}
