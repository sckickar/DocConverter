using System;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[Preserve(AllMembers = true)]
[Token(FormulaToken.tCellRangeList, ",")]
internal class CellRangeListPtg : BinaryOperationPtg
{
	[Preserve]
	public CellRangeListPtg()
	{
	}

	[Preserve]
	public CellRangeListPtg(string operation)
	{
		if (operation == null)
		{
			throw new ArgumentNullException("operation");
		}
		if (operation.Length == 0)
		{
			throw new ArgumentException("operation - string cannot be empty");
		}
		base.OperationSymbol = operation;
		TokenCode = FormulaToken.tCellRangeList;
	}

	[Preserve]
	public CellRangeListPtg(DataProvider provider, int offset, OfficeVersion version)
		: base(provider, offset, version)
	{
	}

	public override string ToString(FormulaUtil formulaUtil, int iRow, int iColumn, bool bR1C1)
	{
		return GetOperandsSeparator(formulaUtil);
	}
}
