using System;
using System.Globalization;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[Preserve(AllMembers = true)]
[Token(FormulaToken.tMissingArgument)]
internal class MissingArgumentPtg : Ptg
{
	[Preserve]
	public MissingArgumentPtg()
	{
		TokenCode = FormulaToken.tMissingArgument;
	}

	[Preserve]
	public MissingArgumentPtg(DataProvider provider, int offset, OfficeVersion version)
		: base(provider, offset, version)
	{
	}

	[Preserve]
	public MissingArgumentPtg(string strFormula)
	{
		if (strFormula != string.Empty)
		{
			throw new ArgumentOutOfRangeException("strFormula", "should be empty string");
		}
		TokenCode = FormulaToken.tMissingArgument;
	}

	public override int GetSize(OfficeVersion version)
	{
		return 1;
	}

	public override string ToString(FormulaUtil formulaUtil, int iRow, int iColumn, bool bR1C1, NumberFormatInfo numberFormat, bool isForSerialization)
	{
		return "";
	}
}
