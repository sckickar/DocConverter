using System;
using System.Globalization;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[Preserve(AllMembers = true)]
[Token(FormulaToken.tAreaErr3d1)]
[Token(FormulaToken.tAreaErr3d2)]
[Token(FormulaToken.tAreaErr3d3)]
[CLSCompliant(false)]
internal class AreaError3DPtg : Area3DPtg, IRangeGetter
{
	[Preserve]
	public AreaError3DPtg()
	{
	}

	[Preserve]
	public AreaError3DPtg(DataProvider provider, int offset, OfficeVersion version)
		: base(provider, offset, version)
	{
	}

	[Preserve]
	public AreaError3DPtg(Area3DPtg ptg)
		: base(ptg)
	{
		TokenCode = ptg.TokenCode - 59 + 61;
	}

	[Preserve]
	public AreaError3DPtg(string value, IWorkbook book)
		: base(value, book)
	{
		TokenCode = FormulaToken.tAreaErr3d1;
	}

	public override string ToString(FormulaUtil formulaUtil, int iRow, int iColumn, bool bR1C1, NumberFormatInfo numberFormat, bool isForSerialization)
	{
		IWorkbook obj = formulaUtil?.ParentWorkbook;
		string sheetName = Ref3DPtg.GetSheetName(obj, base.RefIndex);
		if (obj == null)
		{
			return "#REF!";
		}
		return string.Format("'{0}'!{1}", sheetName, "#REF!");
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
			1 => FormulaToken.tAreaErr3d1, 
			2 => FormulaToken.tAreaErr3d2, 
			3 => FormulaToken.tAreaErr3d3, 
			_ => throw new ArgumentOutOfRangeException("index", "Must be less than 4 and greater than than 0."), 
		};
	}

	public new static int CodeToIndex(FormulaToken code)
	{
		return code switch
		{
			FormulaToken.tAreaErr3d1 => 1, 
			FormulaToken.tAreaErr3d2 => 2, 
			FormulaToken.tAreaErr3d3 => 3, 
			_ => throw new ArgumentOutOfRangeException("index"), 
		};
	}

	public new IRange GetRange(IWorkbook book, IWorksheet sheet)
	{
		return null;
	}
}
