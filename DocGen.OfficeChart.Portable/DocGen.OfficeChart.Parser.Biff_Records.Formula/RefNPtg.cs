using System;
using System.Globalization;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[Preserve(AllMembers = true)]
[Token(FormulaToken.tRefN1)]
[Token(FormulaToken.tRefN2)]
[Token(FormulaToken.tRefN3)]
[CLSCompliant(false)]
internal class RefNPtg : RefPtg
{
	[Preserve]
	public RefNPtg()
	{
	}

	[Preserve]
	public RefNPtg(DataProvider provider, int offset, OfficeVersion version)
		: base(provider, offset, version)
	{
	}

	[Preserve]
	public RefNPtg(int iCellRow, int iCellColumn, string strRow, string strColumn, bool bR1C1)
	{
		SetCell(iCellRow, iCellColumn, strRow, strColumn, bR1C1);
		ColumnIndex -= iCellColumn;
		RowIndex -= iCellRow;
		base.IsRowIndexRelative = true;
		base.IsColumnIndexRelative = true;
	}

	public override Ptg ConvertSharedToken(IWorkbook parent, int iRow, int iColumn)
	{
		int num = ((parent.Version == OfficeVersion.Excel97to2003) ? ColumnIndex : base.ColumnIndex);
		int num2 = ((parent.Version == OfficeVersion.Excel97to2003) ? RowIndex : base.RowIndex);
		int num3 = (IsColumnIndexRelative ? (iColumn + num) : num);
		int num4 = (IsRowIndexRelative ? (iRow + num2) : num2);
		if (parent.Version == OfficeVersion.Excel97to2003)
		{
			num3 = (byte)num3;
			num4 = (ushort)num4;
		}
		RefPtg refPtg = new RefPtg(num4, num3, base.Options);
		int index = CodeToIndex(TokenCode);
		refPtg.TokenCode = RefPtg.IndexToCode(index);
		return refPtg;
	}

	public override string ToString(FormulaUtil formulaUtil, int iRow, int iColumn, bool bR1C1, NumberFormatInfo numberFormat, bool isForSerialization)
	{
		short num = 256;
		short num2 = (short)RowIndex;
		short num3 = (short)ColumnIndex;
		if (num3 >= 255)
		{
			num3 -= num;
		}
		int row = (IsRowIndexRelative ? Math.Abs(iRow + num2 - 1) : RowIndex);
		int column = (IsColumnIndexRelative ? Math.Abs(iColumn + num3 - 1) : ColumnIndex);
		return RefPtg.GetCellName(iRow, iColumn, row, column, IsRowIndexRelative, IsColumnIndexRelative, bR1C1);
	}

	public override Ptg Get3DToken(int iSheetReference)
	{
		FormulaToken tokenCode = Ref3DPtg.IndexToCode(CodeToIndex(TokenCode));
		return new Ref3DPtg(iSheetReference, RowIndex, ColumnIndex, base.Options)
		{
			TokenCode = tokenCode
		};
	}

	public new static int CodeToIndex(FormulaToken token)
	{
		return token switch
		{
			FormulaToken.tRefN1 => 1, 
			FormulaToken.tRefN2 => 2, 
			FormulaToken.tRefN3 => 3, 
			_ => throw new ArgumentOutOfRangeException("index"), 
		};
	}

	public new static FormulaToken IndexToCode(int index)
	{
		return index switch
		{
			1 => FormulaToken.tRefN1, 
			2 => FormulaToken.tRefN2, 
			3 => FormulaToken.tRefN3, 
			_ => throw new ArgumentOutOfRangeException("index"), 
		};
	}
}
