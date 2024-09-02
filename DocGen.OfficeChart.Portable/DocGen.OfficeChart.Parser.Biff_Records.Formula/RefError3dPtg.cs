using System;
using System.Globalization;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[Preserve(AllMembers = true)]
[Token(FormulaToken.tRefErr3d1)]
[Token(FormulaToken.tRefErr3d2)]
[Token(FormulaToken.tRefErr3d3)]
[CLSCompliant(false)]
internal class RefError3dPtg : Ref3DPtg, ISheetReference, IReference, IRangeGetter
{
	[Preserve]
	public RefError3dPtg()
	{
	}

	[Preserve]
	public RefError3dPtg(DataProvider provider, int offset, OfficeVersion version)
		: base(provider, offset, version)
	{
	}

	[Preserve]
	public RefError3dPtg(string strFormula, IWorkbook parent)
		: base(strFormula, parent)
	{
		TokenCode = FormulaToken.tRefErr3d1;
	}

	[Preserve]
	public RefError3dPtg(Ref3DPtg dataHolder)
		: base(dataHolder)
	{
	}

	public override string ToString()
	{
		return "RefErr3d (" + base.RefIndex + base.ToString() + ")";
	}

	public override string ToString(FormulaUtil formulaUtil, int iRow, int iColumn, bool bR1C1, NumberFormatInfo numberFormat, bool isForSerialization)
	{
		string sheetName = Ref3DPtg.GetSheetName(formulaUtil.ParentWorkbook, base.RefIndex);
		if (formulaUtil == null || sheetName == null)
		{
			return "#REF!";
		}
		return string.Format("'{0}'!{1}", sheetName, "#REF!");
	}

	public override Ptg Offset(int iCurSheetIndex, int iTokenRow, int iTokenColumn, int iSourceSheetIndex, Rectangle rectSource, int iDestSheetIndex, Rectangle rectDest, out bool bChanged, WorkbookImpl book)
	{
		bChanged = false;
		return this;
	}

	public new static FormulaToken IndexToCode(int index)
	{
		return index switch
		{
			1 => FormulaToken.tRefErr3d1, 
			2 => FormulaToken.tRefErr3d2, 
			3 => FormulaToken.tRefErr3d3, 
			_ => throw new ArgumentOutOfRangeException("index"), 
		};
	}

	public new IRange GetRange(IWorkbook book, IWorksheet sheet)
	{
		return null;
	}
}
