using System;
using System.Globalization;
using System.Text.RegularExpressions;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[Preserve(AllMembers = true)]
[Token(FormulaToken.tRef1)]
[Token(FormulaToken.tRef2)]
[Token(FormulaToken.tRef3)]
internal class RefPtg : Ptg, IRangeGetterToken, IRangeGetter, IRectGetter, IToken3D
{
	public const byte RowBitMask = 128;

	public const byte ColumnBitMask = 64;

	private const char DEF_R1C1_OPEN_BRACKET = '[';

	private const char DEF_R1C1_CLOSE_BRACKET = ']';

	public const string DEF_R1C1_ROW = "R";

	public const string DEF_R1C1_COLUMN = "C";

	public const char DEF_OPEN_BRACKET = '[';

	public const char DEF_CLOSE_BRACKET = ']';

	private int m_iRowIndex;

	private int m_iColumnIndex;

	private byte m_options;

	[CLSCompliant(false)]
	public virtual int RowIndex
	{
		get
		{
			return m_iRowIndex;
		}
		set
		{
			m_iRowIndex = value;
		}
	}

	public virtual bool IsRowIndexRelative
	{
		get
		{
			return IsRelative(m_options, 128);
		}
		set
		{
			m_options = SetRelative(m_options, 128, value);
		}
	}

	public virtual bool IsColumnIndexRelative
	{
		get
		{
			return IsRelative(m_options, 64);
		}
		set
		{
			m_options = SetRelative(m_options, 64, value);
		}
	}

	public virtual int ColumnIndex
	{
		get
		{
			return m_iColumnIndex;
		}
		set
		{
			m_iColumnIndex = value;
		}
	}

	protected byte Options
	{
		get
		{
			return m_options;
		}
		set
		{
			m_options = value;
		}
	}

	[Preserve]
	public RefPtg()
	{
	}

	[Preserve]
	public RefPtg(string strCell)
	{
		Match match = FormulaUtil.CellRegex.Match(strCell);
		string value = match.Groups["Column1"].Value;
		string value2 = match.Groups["Row1"].Value;
		SetCellA1(value, value2);
		TokenCode = FormulaToken.tRef2;
	}

	[Preserve]
	public RefPtg(DataProvider provider, int offset, OfficeVersion version)
		: base(provider, offset, version)
	{
	}

	[Preserve]
	public RefPtg(int iRowIndex, int iColIndex, byte options)
	{
		m_iRowIndex = iRowIndex;
		m_iColumnIndex = iColIndex;
		m_options = options;
	}

	[Preserve]
	public RefPtg(int iCellRow, int iCellColumn, string strRow, string strColumn, bool bR1C1)
	{
		SetCell(iCellRow, iCellColumn, strRow, strColumn, bR1C1);
	}

	[Preserve]
	public RefPtg(RefPtg twin)
	{
		m_iRowIndex = twin.RowIndex;
		m_iColumnIndex = twin.m_iColumnIndex;
		m_options = twin.m_options;
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
			return 10;
		default:
			throw new ArgumentOutOfRangeException("version");
		}
	}

	public override string ToString()
	{
		return GetCellName(0, 0, m_iRowIndex, m_iColumnIndex, IsRowIndexRelative, IsColumnIndexRelative, bR1C1: false);
	}

	public override string ToString(FormulaUtil formulaUtil, int iRow, int iColumn, bool bR1C1, NumberFormatInfo numberFormat, bool isForSerialization)
	{
		return GetCellName(iRow, iColumn, RowIndex, ColumnIndex, IsRowIndexRelative, IsColumnIndexRelative, bR1C1);
	}

	public override byte[] ToByteArray(OfficeVersion version)
	{
		byte[] array = base.ToByteArray(version);
		int num = 1;
		if (version == OfficeVersion.Excel97to2003)
		{
			if (m_iRowIndex > 65535 || m_iColumnIndex > 255)
			{
				FormulaToken correspondingErrorCode = GetCorrespondingErrorCode();
				array[0] = (byte)correspondingErrorCode;
			}
			BitConverter.GetBytes((ushort)m_iRowIndex).CopyTo(array, num);
			num += 2;
			array[num] = (byte)m_iColumnIndex;
			num++;
		}
		else
		{
			if (version == OfficeVersion.Excel97to2003)
			{
				throw new ArgumentOutOfRangeException("version");
			}
			BitConverter.GetBytes(m_iRowIndex).CopyTo(array, num);
			num += 4;
			BitConverter.GetBytes(m_iColumnIndex).CopyTo(array, num);
			num += 4;
		}
		array[num] = m_options;
		return array;
	}

	protected void SetCell(int iCellRow, int iCellColumn, string strRow, string strColumn, bool bR1C1)
	{
		if (bR1C1)
		{
			SetCellR1C1(iCellRow, iCellColumn, strColumn, strRow);
		}
		else
		{
			SetCellA1(strColumn, strRow);
		}
	}

	protected void SetCellA1(string strColumn, string strRow)
	{
		if (strRow == null)
		{
			throw new ArgumentNullException("strRow");
		}
		if (strRow.Length == 0)
		{
			throw new ArgumentException("strRow - string cannot be empty");
		}
		if (strColumn == null)
		{
			throw new ArgumentNullException("strColumn");
		}
		if (strColumn.Length == 0)
		{
			throw new ArgumentException("strColumn - string cannot be empty");
		}
		m_iColumnIndex = GetColumnIndex(0, strColumn, bR1C1: false, out var bRelative);
		IsColumnIndexRelative = bRelative;
		m_iRowIndex = GetRowIndex(0, strRow, bR1C1: false, out bRelative);
		IsRowIndexRelative = bRelative;
	}

	protected void SetCellR1C1(int iCellRow, int iCellColumn, string column, string row)
	{
		m_iColumnIndex = GetR1C1Index(iCellColumn, column, out var bRelative);
		IsColumnIndexRelative = bRelative;
		m_iRowIndex = GetR1C1Index(iCellRow, row, out bRelative);
		IsRowIndexRelative = bRelative;
	}

	public static int GetR1C1Index(int iIndex, string strValue, out bool bRelative)
	{
		bRelative = false;
		if (strValue == null)
		{
			return -1;
		}
		int length = strValue.Length;
		if (length < 2)
		{
			bRelative = true;
			return iIndex;
		}
		strValue = strValue.Substring(1);
		length--;
		if (strValue[0] == '[' && strValue[length - 1] == ']')
		{
			length -= 2;
			strValue = strValue.Substring(1, length);
			bRelative = true;
		}
		int num = int.Parse(strValue);
		if (!bRelative)
		{
			return num - 1;
		}
		return iIndex + num;
	}

	public override Ptg Offset(int iRowOffset, int iColumnOffset, WorkbookImpl book)
	{
		RefPtg refPtg = (RefPtg)base.Offset(iRowOffset, iColumnOffset, book);
		int num = (IsRowIndexRelative ? (RowIndex + iRowOffset) : RowIndex);
		int num2 = (IsColumnIndexRelative ? (ColumnIndex + iColumnOffset) : ColumnIndex);
		if (num < 0 || num > book.MaxRowCount - 1 || num2 < 0 || num2 > book.MaxColumnCount - 1)
		{
			return FormulaUtil.CreatePtg(GetCorrespondingErrorCode(), ToString(book.FormulaUtil), book);
		}
		refPtg.RowIndex = num;
		refPtg.ColumnIndex = num2;
		return refPtg;
	}

	public override Ptg Offset(int iCurSheetIndex, int iTokenRow, int iTokenColumn, int iSourceSheetIndex, Rectangle rectSource, int iDestSheetIndex, Rectangle rectDest, out bool bChanged, WorkbookImpl book)
	{
		bChanged = false;
		RefPtg refPtg = (RefPtg)base.Offset(iCurSheetIndex, iTokenRow, iTokenColumn, iSourceSheetIndex, rectSource, iDestSheetIndex, rectDest, out bChanged, book);
		int num = rectDest.Top - rectSource.Top;
		int num2 = rectDest.Left - rectSource.Left;
		int rowIndex = RowIndex;
		int columnIndex = ColumnIndex;
		if (iCurSheetIndex == iSourceSheetIndex)
		{
			if (Ptg.RectangleContains(rectSource, rowIndex, columnIndex))
			{
				rowIndex += num;
				columnIndex += num2;
				return refPtg.UpdateReferencedCell(iCurSheetIndex, iDestSheetIndex, rowIndex, columnIndex, ref bChanged, book);
			}
			if (Ptg.RectangleContains(rectDest, rowIndex, columnIndex))
			{
				return ConvertToError();
			}
		}
		else if (iCurSheetIndex == iDestSheetIndex && iSourceSheetIndex != iDestSheetIndex && Ptg.RectangleContains(rectDest, iTokenRow, iTokenColumn))
		{
			bChanged = true;
			return MoveIntoDifferentSheet(refPtg, iSourceSheetIndex, rectSource, iDestSheetIndex, num, num2, book);
		}
		return refPtg;
	}

	protected virtual Ptg MoveIntoDifferentSheet(RefPtg result, int iSourceSheetIndex, Rectangle rectSource, int iDestSheetIndex, int iRowOffset, int iColOffset, WorkbookImpl book)
	{
		int num = iSourceSheetIndex;
		bool num2 = ReferencedCellMoved(rectSource);
		int num3 = result.RowIndex;
		int num4 = result.ColumnIndex;
		if (num2)
		{
			num = iDestSheetIndex;
			num3 += iRowOffset;
			num4 += iColOffset;
		}
		return result = (RefPtg)FormulaUtil.CreatePtg(Ref3DPtg.IndexToCode(CodeToIndex()), num, num3, num4, m_options);
	}

	private bool ReferencedCellMoved(Rectangle rectSource)
	{
		int rowIndex = RowIndex;
		int columnIndex = ColumnIndex;
		return Ptg.RectangleContains(rectSource, rowIndex, columnIndex);
	}

	public virtual int CodeToIndex()
	{
		return CodeToIndex(TokenCode);
	}

	public virtual FormulaToken GetCorrespondingErrorCode()
	{
		return RefErrorPtg.IndexToCode(CodeToIndex());
	}

	private Ptg UpdateReferencedCell(int iCurSheetIndex, int iDestSheetIndex, int iRowIndex, int iColIndex, ref bool bChanged, WorkbookImpl book)
	{
		if (iRowIndex < 0 || iRowIndex > book.MaxRowCount - 1 || iColIndex < 0 || iColIndex > book.MaxColumnCount - 1)
		{
			return FormulaUtil.CreatePtg(GetCorrespondingErrorCode(), ToString());
		}
		if (iCurSheetIndex == iDestSheetIndex)
		{
			RowIndex = iRowIndex;
			ColumnIndex = iColIndex;
			bChanged = true;
			return this;
		}
		bChanged = true;
		return FormulaUtil.CreatePtg(Ref3DPtg.IndexToCode(CodeToIndex()), iDestSheetIndex, iRowIndex, iColIndex, m_options);
	}

	public override Ptg ConvertPtgToNPtg(IWorkbook parent, int iRow, int iColumn)
	{
		int columnIndex = (IsColumnIndexRelative ? (ColumnIndex - iColumn) : ColumnIndex);
		int rowIndex = (IsRowIndexRelative ? (RowIndex - iRow) : RowIndex);
		RefNPtg refNPtg = (RefNPtg)FormulaUtil.CreatePtg(RefNPtg.IndexToCode(CodeToIndex()));
		if (parent.Version == OfficeVersion.Excel97to2003)
		{
			refNPtg.RowIndex = rowIndex;
			refNPtg.ColumnIndex = columnIndex;
		}
		else
		{
			refNPtg.RowIndex = rowIndex;
			refNPtg.ColumnIndex = columnIndex;
		}
		refNPtg.Options = Options;
		return refNPtg;
	}

	public static bool IsRelative(byte Options, byte mask)
	{
		return (Options & mask) != 0;
	}

	public static byte SetRelative(byte Options, byte mask, bool value)
	{
		Options &= (byte)(~mask);
		if (value)
		{
			Options += mask;
		}
		return Options;
	}

	[CLSCompliant(false)]
	public static string GetCellName(int iCurCellRow, int iCurCellColumn, int row, int column, bool bRowRelative, bool bColumnRelative, bool bR1C1)
	{
		if (!bR1C1)
		{
			return GetA1CellName(column, row, bColumnRelative, bRowRelative);
		}
		return GetR1C1CellName(iCurCellRow, iCurCellColumn, row, column, bRowRelative, bColumnRelative);
	}

	private static string GetA1CellName(int column, int row, bool isColumnRelative, bool isRowRelative)
	{
		string text = RangeImpl.GetColumnName(column + 1);
		string text2 = (row + 1).ToString();
		if (!isRowRelative)
		{
			text2 = "$" + text2;
		}
		if (!isColumnRelative)
		{
			text = "$" + text;
		}
		return text + text2;
	}

	public static string GetRCCellName(int column, int row)
	{
		return "R" + ((row == 0) ? "" : ("[" + row + "]")) + "C" + ((column == 0) ? "" : ("[" + column + "]"));
	}

	public static int GetColumnIndex(int iCellColumn, string columnName, bool bR1C1, out bool bRelative)
	{
		if (!bR1C1)
		{
			return GetA1ColumnIndex(columnName, out bRelative);
		}
		return GetR1C1Index(iCellColumn, columnName, out bRelative);
	}

	public static int GetA1ColumnIndex(string columnName, out bool bRelative)
	{
		if (columnName == null)
		{
			throw new ArgumentNullException("columnName");
		}
		if (columnName.Length == 0)
		{
			throw new ArgumentException("columnName - string cannot be empty.");
		}
		bRelative = columnName[0] != '$';
		if (!bRelative)
		{
			columnName = columnName.Substring(1);
		}
		return RangeImpl.GetColumnIndex(columnName) - 1;
	}

	public static int GetRowIndex(int iCellRow, string strRowName, bool bR1C1, out bool bRelative)
	{
		if (!bR1C1)
		{
			return GetA1RowIndex(strRowName, out bRelative);
		}
		return GetR1C1Index(iCellRow, strRowName, out bRelative);
	}

	public static int GetA1RowIndex(string strRowName, out bool bRelative)
	{
		if (strRowName == null)
		{
			throw new ArgumentNullException("strRowName");
		}
		if (strRowName.Length == 0)
		{
			throw new ArgumentException("strRowName - string cannot be empty.");
		}
		bRelative = strRowName[0] != '$';
		if (!bRelative)
		{
			strRowName = strRowName.Substring(1);
		}
		return int.Parse(strRowName) - 1;
	}

	public static FormulaToken IndexToCode(int index)
	{
		return index switch
		{
			1 => FormulaToken.tRef1, 
			2 => FormulaToken.tRef2, 
			3 => FormulaToken.tRef3, 
			_ => throw new ArgumentOutOfRangeException("index"), 
		};
	}

	public static int CodeToIndex(FormulaToken token)
	{
		switch (token)
		{
		case FormulaToken.tRef1:
		case FormulaToken.tRefErr1:
			return 1;
		case FormulaToken.tRef2:
		case FormulaToken.tRefErr2:
			return 2;
		case FormulaToken.tRef3:
		case FormulaToken.tRefErr3:
			return 3;
		default:
			throw new ArgumentOutOfRangeException("index");
		}
	}

	public static string GetR1C1CellName(int iCurRow, int iCurColumn, int row, int column, bool bRowRelative, bool bColumnRelative)
	{
		return GetR1C1Name(iCurRow, "R", row, bRowRelative) + GetR1C1Name(iCurColumn, "C", column, bColumnRelative);
	}

	public static string GetR1C1Name(int iCurIndex, string strStart, int iIndex, bool bIsRelative)
	{
		if (strStart == null)
		{
			throw new ArgumentNullException("strStart");
		}
		if (bIsRelative)
		{
			iIndex -= iCurIndex;
			if (iIndex == 0)
			{
				return strStart;
			}
		}
		string text = strStart;
		if (bIsRelative)
		{
			text += "[";
		}
		else
		{
			iIndex++;
		}
		text += iIndex;
		if (bIsRelative)
		{
			text += "]";
		}
		return text;
	}

	public IRange GetRange(IWorkbook book, IWorksheet sheet)
	{
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		return sheet.Range[m_iRowIndex + 1, m_iColumnIndex + 1];
	}

	public Rectangle GetRectangle()
	{
		return Rectangle.FromLTRB(ColumnIndex, RowIndex, ColumnIndex, RowIndex);
	}

	public Ptg UpdateRectangle(Rectangle rectangle)
	{
		RefPtg obj = (RefPtg)Clone();
		obj.ColumnIndex = rectangle.Left;
		obj.RowIndex = rectangle.Top;
		return obj;
	}

	public virtual Ptg Get3DToken(int iSheetReference)
	{
		FormulaToken tokenCode = Ref3DPtg.IndexToCode(CodeToIndex(TokenCode));
		return new Ref3DPtg(iSheetReference, RowIndex, ColumnIndex, Options)
		{
			TokenCode = tokenCode
		};
	}

	public virtual Ptg ConvertToError()
	{
		return FormulaUtil.CreatePtg(GetCorrespondingErrorCode(), this);
	}

	public override void InfillPTG(DataProvider provider, ref int offset, OfficeVersion version)
	{
		if (version == OfficeVersion.Excel97to2003)
		{
			m_iRowIndex = provider.ReadUInt16(offset);
			offset += 2;
			m_iColumnIndex = provider.ReadByte(offset++);
			m_options = provider.ReadByte(offset++);
			return;
		}
		if (version != 0)
		{
			m_iRowIndex = provider.ReadInt32(offset);
			offset += 4;
			m_iColumnIndex = provider.ReadInt32(offset);
			offset += 4;
			m_options = provider.ReadByte(offset++);
			return;
		}
		throw new NotImplementedException();
	}
}
