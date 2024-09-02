using System;
using System.Globalization;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[Preserve(AllMembers = true)]
[Token(FormulaToken.tTbl)]
[Token(FormulaToken.tExp)]
[CLSCompliant(false)]
internal class ControlPtg : RefPtg
{
	public override bool IsColumnIndexRelative
	{
		get
		{
			return true;
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public override bool IsRowIndexRelative
	{
		get
		{
			return true;
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	[Preserve]
	public ControlPtg()
	{
	}

	[Preserve]
	public ControlPtg(DataProvider provider, int offset, OfficeVersion version)
		: base(provider, offset, version)
	{
	}

	[Preserve]
	public ControlPtg(int iRow, int iColumn)
	{
		RowIndex = iRow;
		ColumnIndex = iColumn;
	}

	public override int GetSize(OfficeVersion version)
	{
		switch (version)
		{
		case OfficeVersion.Excel97to2003:
			return 5;
		case OfficeVersion.Excel2007:
		case OfficeVersion.Excel2010:
		case OfficeVersion.Excel2013:
			return 9;
		default:
			throw new ArgumentOutOfRangeException("version");
		}
	}

	public override string ToString(FormulaUtil formulaUtil, int iRow, int iColumn, bool bR1C1, NumberFormatInfo numberFormat, bool isForSerialization)
	{
		return "( ControlToken " + RangeImpl.GetCellName(ColumnIndex + 1, RowIndex + 1) + ")";
	}

	public override byte[] ToByteArray(OfficeVersion version)
	{
		byte[] array = new byte[GetSize(version)];
		array[0] = (byte)TokenCode;
		switch (version)
		{
		case OfficeVersion.Excel97to2003:
			BitConverter.GetBytes((ushort)RowIndex).CopyTo(array, 1);
			BitConverter.GetBytes((ushort)ColumnIndex).CopyTo(array, 3);
			break;
		case OfficeVersion.Excel2007:
		case OfficeVersion.Excel2010:
		case OfficeVersion.Excel2013:
		{
			int num = 1;
			BitConverter.GetBytes(RowIndex).CopyTo(array, num);
			num += 4;
			BitConverter.GetBytes(ColumnIndex).CopyTo(array, num);
			break;
		}
		default:
			throw new ArgumentOutOfRangeException("version");
		}
		return array;
	}

	public override FormulaToken GetCorrespondingErrorCode()
	{
		return FormulaToken.tRef2;
	}

	protected override Ptg MoveIntoDifferentSheet(RefPtg result, int iSourceSheetIndex, Rectangle rectSource, int iDestSheetIndex, int iRowOffset, int iColOffset, WorkbookImpl book)
	{
		return Offset(iRowOffset, iColOffset, book);
	}

	public override void InfillPTG(DataProvider provider, ref int offset, OfficeVersion version)
	{
		if (version == OfficeVersion.Excel97to2003)
		{
			RowIndex = provider.ReadUInt16(offset);
			offset += 2;
			ColumnIndex = (byte)provider.ReadUInt16(offset);
			offset += 2;
		}
		else if (version != 0)
		{
			RowIndex = provider.ReadInt32(offset);
			offset += 4;
			ColumnIndex = provider.ReadInt32(offset);
			offset += 4;
		}
	}
}
