using System.Globalization;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[Preserve(AllMembers = true)]
internal class CellIntersectionPtg : Ptg
{
	[Preserve]
	public CellIntersectionPtg()
	{
	}

	[Preserve]
	public CellIntersectionPtg(string strFormula)
	{
	}

	[Preserve]
	public CellIntersectionPtg(DataProvider provider, int offset, OfficeVersion version)
		: base(provider, offset, version)
	{
	}

	public override string ToString(FormulaUtil formulaUtil, int iRow, int iColumn, bool bR1C1, NumberFormatInfo numberFormat, bool isForSerialization)
	{
		return "CellIntersection";
	}

	public override int GetSize(OfficeVersion version)
	{
		return 1;
	}
}
