using System;
using System.Globalization;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[Preserve(AllMembers = true)]
[ErrorCode("#REF!", 23)]
[Token(FormulaToken.tRefErr1)]
[Token(FormulaToken.tRefErr2)]
[Token(FormulaToken.tRefErr3)]
internal class RefErrorPtg : RefPtg, IRangeGetter
{
	public const string ReferenceError = "#REF!";

	[Preserve]
	static RefErrorPtg()
	{
	}

	[Preserve]
	public RefErrorPtg()
	{
	}

	[Preserve]
	public RefErrorPtg(DataProvider provider, int offset, OfficeVersion version)
		: base(provider, offset, version)
	{
	}

	[Preserve]
	public RefErrorPtg(string errorName)
		: base("A1")
	{
		TokenCode = FormulaToken.tRefErr2;
	}

	[Preserve]
	public RefErrorPtg(string errorName, IWorkbook book)
		: this(errorName)
	{
	}

	[Preserve]
	public RefErrorPtg(RefPtg dataHolder)
		: base(dataHolder)
	{
		int index = RefPtg.CodeToIndex(dataHolder.TokenCode);
		TokenCode = IndexToCode(index);
	}

	public override string ToString()
	{
		return "RefErr (" + base.ToString() + ")";
	}

	public override string ToString(FormulaUtil formulaUtil, int iRow, int iColumn, bool bR1C1, NumberFormatInfo numberFormat, bool isForSerialization)
	{
		return "#REF!";
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
			1 => FormulaToken.tRefErr1, 
			2 => FormulaToken.tRefErr2, 
			3 => FormulaToken.tRefErr3, 
			_ => throw new ArgumentOutOfRangeException("index"), 
		};
	}

	public override Ptg ConvertPtgToNPtg(IWorkbook parent, int iRow, int iColumn)
	{
		int columnIndex = (IsColumnIndexRelative ? (ColumnIndex - iColumn) : ColumnIndex);
		int rowIndex = (IsRowIndexRelative ? (RowIndex - iRow) : RowIndex);
		RefErrorPtg refErrorPtg = (RefErrorPtg)FormulaUtil.CreatePtg(IndexToCode(CodeToIndex()));
		if (parent.Version == OfficeVersion.Excel97to2003)
		{
			refErrorPtg.RowIndex = rowIndex;
			refErrorPtg.ColumnIndex = columnIndex;
		}
		else
		{
			refErrorPtg.RowIndex = rowIndex;
			refErrorPtg.ColumnIndex = columnIndex;
		}
		refErrorPtg.Options = base.Options;
		return refErrorPtg;
	}

	public new IRange GetRange(IWorkbook book, IWorksheet sheet)
	{
		return null;
	}
}
