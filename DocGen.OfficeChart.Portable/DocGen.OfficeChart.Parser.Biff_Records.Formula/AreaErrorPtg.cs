using System;
using System.Globalization;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[Preserve(AllMembers = true)]
[Token(FormulaToken.tAreaErr1)]
[Token(FormulaToken.tAreaErr2)]
[Token(FormulaToken.tAreaErr3)]
[CLSCompliant(false)]
internal class AreaErrorPtg : AreaPtg, IRangeGetter
{
	[Preserve]
	public AreaErrorPtg()
	{
	}

	[Preserve]
	public AreaErrorPtg(DataProvider provider, int offset, OfficeVersion version)
		: base(provider, offset, version)
	{
	}

	[Preserve]
	public AreaErrorPtg(AreaPtg area)
		: base(area)
	{
		TokenCode = area.TokenCode - 37 + 43;
	}

	[Preserve]
	public AreaErrorPtg(string value, IWorkbook book)
		: base(value, book)
	{
		TokenCode = FormulaToken.tAreaErr1;
	}

	public override string ToString(FormulaUtil formulaUtil, int iRow, int iColumn, bool bR1C1, NumberFormatInfo numberFormat, bool isForSerialization)
	{
		return "#REF!";
	}

	public override int CodeToIndex()
	{
		return CodeToIndex(TokenCode);
	}

	public override Ptg Offset(int iCurSheetIndex, int iTokenRow, int iTokenColumn, int iSourceSheetIndex, Rectangle rectSource, int iDestSheetIndex, Rectangle rectDest, out bool bChanged, WorkbookImpl book)
	{
		bChanged = false;
		return (Ptg)Clone();
	}

	public new static FormulaToken IndexToCode(int index)
	{
		return index switch
		{
			1 => FormulaToken.tAreaErr1, 
			2 => FormulaToken.tAreaErr2, 
			3 => FormulaToken.tAreaErr3, 
			_ => throw new ArgumentOutOfRangeException("index"), 
		};
	}

	public new static int CodeToIndex(FormulaToken code)
	{
		return code switch
		{
			FormulaToken.tAreaErr1 => 1, 
			FormulaToken.tAreaErr2 => 2, 
			FormulaToken.tAreaErr3 => 3, 
			_ => throw new ArgumentOutOfRangeException("index"), 
		};
	}

	public new IRange GetRange(IWorkbook book, IWorksheet sheet)
	{
		return null;
	}
}
