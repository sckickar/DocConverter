using System;
using System.Globalization;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[Preserve(AllMembers = true)]
[Token(FormulaToken.tAreaN1)]
[Token(FormulaToken.tAreaN2)]
[Token(FormulaToken.tAreaN3)]
[CLSCompliant(false)]
internal class AreaNPtg : AreaPtg
{
	public new short FirstColumn
	{
		get
		{
			return (short)(ushort)base.FirstColumn;
		}
		set
		{
			base.FirstColumn = (ushort)value;
		}
	}

	public new short LastColumn
	{
		get
		{
			return (short)(ushort)base.LastColumn;
		}
		set
		{
			base.LastColumn = (ushort)value;
		}
	}

	[Preserve]
	public AreaNPtg()
	{
	}

	[Preserve]
	public AreaNPtg(string strFormula, IWorkbook book)
		: base(strFormula, book)
	{
	}

	[Preserve]
	public AreaNPtg(DataProvider provider, int offset, OfficeVersion version)
		: base(provider, offset, version)
	{
	}

	public override Ptg ConvertSharedToken(IWorkbook parent, int iRow, int iColumn)
	{
		bool flag = IsWholeRows(parent);
		bool flag2 = IsWholeColumns(parent);
		int num = ((base.IsFirstColumnRelative && !flag) ? (iColumn + FirstColumn) : FirstColumn);
		int num2 = ((base.IsFirstRowRelative && !flag2) ? (iRow + base.FirstRow) : base.FirstRow);
		int num3 = ((base.IsLastColumnRelative && !flag) ? (iColumn + LastColumn) : LastColumn);
		int num4 = ((base.IsLastRowRelative && !flag2) ? (iRow + base.LastRow) : base.LastRow);
		if (parent.Version == OfficeVersion.Excel97to2003)
		{
			num = (byte)num;
			num3 = (byte)num3;
			num2 = (ushort)num2;
			num4 = (ushort)num4;
		}
		AreaPtg areaPtg = new AreaPtg(num2, num, num4, num3, base.FirstOptions, base.LastOptions);
		int index = CodeToIndex(TokenCode);
		areaPtg.TokenCode = AreaPtg.IndexToCode(index);
		return areaPtg;
	}

	public new static int CodeToIndex(FormulaToken token)
	{
		return token switch
		{
			FormulaToken.tAreaN1 => 1, 
			FormulaToken.tAreaN2 => 2, 
			FormulaToken.tAreaN3 => 3, 
			_ => throw new ArgumentOutOfRangeException("index"), 
		};
	}

	public new static FormulaToken IndexToCode(int index)
	{
		return index switch
		{
			1 => FormulaToken.tAreaN1, 
			2 => FormulaToken.tAreaN2, 
			3 => FormulaToken.tAreaN3, 
			_ => throw new ArgumentOutOfRangeException("index"), 
		};
	}

	public override string ToString(FormulaUtil formulaUtil, int iRow, int iColumn, bool bR1C1, NumberFormatInfo numberFormat, bool isForSerialization)
	{
		WorkbookImpl book = ((formulaUtil != null) ? ((WorkbookImpl)formulaUtil.ParentWorkbook) : null);
		bool flag = IsWholeRows(book);
		bool flag2 = IsWholeColumns(book);
		if (flag && bR1C1)
		{
			return RefPtg.GetR1C1Name(iRow, "R", base.FirstRow, base.IsFirstRowRelative) + ":" + RefPtg.GetR1C1Name(iRow, "R", base.LastRow, base.IsFirstRowRelative);
		}
		if (flag2 && bR1C1)
		{
			return RefPtg.GetR1C1Name(iColumn, "C", FirstColumn, base.IsFirstColumnRelative) + ":" + RefPtg.GetR1C1Name(iColumn, "C", LastColumn, base.IsFirstColumnRelative);
		}
		if (flag)
		{
			(base.FirstRow + 1).ToString();
			return "$" + (base.FirstRow + 1) + ":$" + (base.LastRow + 1);
		}
		if (flag2)
		{
			RangeImpl.GetColumnName(FirstColumn + 1);
			return "$" + RangeImpl.GetColumnName(FirstColumn + 1) + ":$" + RangeImpl.GetColumnName(LastColumn + 1);
		}
		int num = (base.IsFirstRowRelative ? GetUpdatedRowIndex(iRow, base.FirstRow, isFirst: true) : base.FirstRow);
		int row = (base.IsLastRowRelative ? GetUpdatedRowIndex(iRow, base.LastRow, isFirst: false) : base.LastRow);
		int num2 = (base.IsFirstColumnRelative ? GetUpdatedColumnIndex(iColumn, FirstColumn, isFirst: true) : FirstColumn);
		int column = (base.IsLastColumnRelative ? GetUpdatedColumnIndex(iColumn, LastColumn, isFirst: false) : LastColumn);
		if (FirstColumn == LastColumn)
		{
			column = num2;
		}
		if (base.FirstRow == base.LastRow)
		{
			row = num;
		}
		return RefPtg.GetCellName(iRow, iColumn, num, num2, base.IsFirstRowRelative, base.IsFirstColumnRelative, bR1C1) + ":" + RefPtg.GetCellName(iRow, iColumn, row, column, base.IsLastRowRelative, base.IsLastColumnRelative, bR1C1);
	}

	private int GetUpdatedRowIndex(int iRow, int row, bool isFirst)
	{
		int result = 0;
		if (iRow == 0 && row == 0)
		{
			return result;
		}
		bool num;
		if (!isFirst)
		{
			if (iRow > row && row < iRow)
			{
				num = row == 0;
				goto IL_002d;
			}
		}
		else if (row > iRow && iRow < row)
		{
			num = iRow == 0;
			goto IL_002d;
		}
		goto IL_002f;
		IL_002f:
		return row + iRow - 1;
		IL_002d:
		if (!num)
		{
			return iRow - (65536 - row);
		}
		goto IL_002f;
	}

	private int GetUpdatedColumnIndex(int iColumn, int column, bool isFirst)
	{
		int result = 0;
		if (iColumn == 0 && column == 0)
		{
			return result;
		}
		bool num;
		if (!isFirst)
		{
			if (iColumn > column && column < iColumn)
			{
				goto IL_0037;
			}
			num = column == 0;
		}
		else
		{
			if (column <= iColumn || iColumn >= column)
			{
				goto IL_002f;
			}
			num = iColumn == 0;
		}
		if (num)
		{
			goto IL_002f;
		}
		goto IL_0037;
		IL_002f:
		return column + iColumn - 1;
		IL_0037:
		return column + iColumn - 256 - 1;
	}
}
