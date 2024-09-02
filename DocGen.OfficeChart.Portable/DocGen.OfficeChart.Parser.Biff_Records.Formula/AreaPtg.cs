using System;
using System.Globalization;
using System.Text.RegularExpressions;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.Formula;

[Preserve(AllMembers = true)]
[Token(FormulaToken.tArea1)]
[Token(FormulaToken.tArea2)]
[Token(FormulaToken.tArea3)]
[CLSCompliant(false)]
internal class AreaPtg : Ptg, IRangeGetterToken, IRangeGetter, IRectGetter, IToken3D
{
	private int m_iFirstRow;

	private int m_iLastRow;

	private int m_iFirstColumn;

	private byte m_firstOptions;

	private int m_iLastColumn;

	private byte m_lastOptions;

	public int FirstRow
	{
		get
		{
			return m_iFirstRow;
		}
		set
		{
			m_iFirstRow = value;
		}
	}

	public bool IsFirstRowRelative
	{
		get
		{
			return RefPtg.IsRelative(m_firstOptions, 128);
		}
		set
		{
			m_firstOptions = RefPtg.SetRelative(m_firstOptions, 128, value);
		}
	}

	public bool IsFirstColumnRelative
	{
		get
		{
			return RefPtg.IsRelative(m_firstOptions, 64);
		}
		set
		{
			m_firstOptions = RefPtg.SetRelative(m_firstOptions, 64, value);
		}
	}

	public int FirstColumn
	{
		get
		{
			return m_iFirstColumn;
		}
		set
		{
			m_iFirstColumn = value;
		}
	}

	public int LastRow
	{
		get
		{
			return m_iLastRow;
		}
		set
		{
			m_iLastRow = value;
		}
	}

	public bool IsLastRowRelative
	{
		get
		{
			return RefPtg.IsRelative(m_lastOptions, 128);
		}
		set
		{
			m_lastOptions = RefPtg.SetRelative(m_lastOptions, 128, value);
		}
	}

	public bool IsLastColumnRelative
	{
		get
		{
			return RefPtg.IsRelative(m_lastOptions, 64);
		}
		set
		{
			m_lastOptions = RefPtg.SetRelative(m_lastOptions, 64, value);
		}
	}

	public int LastColumn
	{
		get
		{
			return m_iLastColumn;
		}
		set
		{
			m_iLastColumn = value;
		}
	}

	protected byte FirstOptions
	{
		get
		{
			return m_firstOptions;
		}
		set
		{
			m_firstOptions = value;
		}
	}

	protected byte LastOptions
	{
		get
		{
			return m_lastOptions;
		}
		set
		{
			m_lastOptions = value;
		}
	}

	[Preserve]
	public AreaPtg()
	{
	}

	[Preserve]
	public AreaPtg(string strFormula, IWorkbook book)
	{
		Match match = FormulaUtil.CellRangeRegex.Match(strFormula);
		string value = match.Groups["Column1"].Value;
		string value2 = match.Groups["Row1"].Value;
		string value3 = match.Groups["Column2"].Value;
		string value4 = match.Groups["Row2"].Value;
		if (!match.Success)
		{
			throw new ArgumentException();
		}
		SetArea(0, 0, value2, value, value4, value3, bR1C1: false, book);
	}

	[Preserve]
	public AreaPtg(DataProvider provider, int offset, OfficeVersion version)
		: base(provider, offset, version)
	{
	}

	[Preserve]
	public AreaPtg(AreaPtg ptg)
	{
		m_iFirstRow = ptg.m_iFirstRow;
		m_iLastRow = ptg.m_iLastRow;
		m_iFirstColumn = ptg.m_iFirstColumn;
		m_firstOptions = ptg.m_firstOptions;
		m_iLastColumn = ptg.m_iLastColumn;
		m_lastOptions = ptg.m_lastOptions;
	}

	[Preserve]
	public AreaPtg(int iFirstRow, int iFirstCol, int iLastRow, int iLastCol, byte firstOptions, byte lastOptions)
	{
		m_iFirstRow = iFirstRow;
		m_iLastRow = iLastRow;
		m_iFirstColumn = iFirstCol;
		m_iLastColumn = iLastCol;
		m_firstOptions = firstOptions;
		m_lastOptions = lastOptions;
	}

	[Preserve]
	public AreaPtg(int iCellRow, int iCellColumn, string strFirstRow, string strFirstColumn, string strLastRow, string strLastColumn, bool bR1C1, IWorkbook book)
	{
		SetArea(iCellRow, iCellColumn, strFirstRow, strFirstColumn, strLastRow, strLastColumn, bR1C1, book);
	}

	protected void SetArea(int iCellRow, int iCellColumn, string row1, string column1, string row2, string column2, bool bR1C1, IWorkbook book)
	{
		bool bRelative;
		int num = RefPtg.GetColumnIndex(iCellColumn, column1, bR1C1, out bRelative);
		IsFirstColumnRelative = bRelative;
		int num2 = RefPtg.GetRowIndex(iCellRow, row1, bR1C1, out bRelative);
		IsFirstRowRelative = bRelative;
		int num3 = RefPtg.GetColumnIndex(iCellColumn, column2, bR1C1, out bRelative);
		IsLastColumnRelative = bRelative;
		int num4 = RefPtg.GetRowIndex(iCellRow, row2, bR1C1, out bRelative);
		IsLastRowRelative = bRelative;
		if (num2 == -1 && num4 == -1)
		{
			num2 = 0;
			num4 = book.MaxRowCount - 1;
		}
		else if (num == -1 && num3 == -1)
		{
			num = 0;
			num3 = book.MaxColumnCount - 1;
		}
		m_iFirstRow = num2;
		m_iLastRow = num4;
		m_iFirstColumn = num;
		m_iLastColumn = num3;
	}

	public virtual int CodeToIndex()
	{
		return CodeToIndex(TokenCode);
	}

	public virtual FormulaToken GetCorrespondingErrorCode()
	{
		return AreaErrorPtg.IndexToCode(CodeToIndex());
	}

	protected bool IsWholeRow(IWorkbook book)
	{
		if (FirstRow == LastRow)
		{
			return IsWholeRows(book);
		}
		return false;
	}

	protected bool IsWholeRows(IWorkbook book)
	{
		if (book == null)
		{
			return false;
		}
		if (IsFirstRowRelative == IsLastRowRelative && FirstColumn == 0)
		{
			return LastColumn == book.MaxColumnCount - 1;
		}
		return false;
	}

	protected bool IsWholeColumns(IWorkbook book)
	{
		if (book == null)
		{
			return false;
		}
		if (IsFirstColumnRelative == IsLastColumnRelative && FirstRow == 0)
		{
			return LastRow == book.MaxRowCount - 1;
		}
		return false;
	}

	protected bool IsWholeColumn(IWorkbook book)
	{
		if (FirstColumn == LastColumn)
		{
			return IsWholeColumns(book);
		}
		return false;
	}

	public virtual AreaPtg ConvertToErrorPtg()
	{
		return new AreaErrorPtg(this);
	}

	public override void InfillPTG(DataProvider provider, ref int offset, OfficeVersion version)
	{
		if (version == OfficeVersion.Excel97to2003)
		{
			m_iFirstRow = provider.ReadUInt16(offset);
			offset += 2;
			m_iLastRow = provider.ReadUInt16(offset);
			offset += 2;
			m_iFirstColumn = provider.ReadByte(offset++);
			m_firstOptions = provider.ReadByte(offset++);
			m_iLastColumn = provider.ReadByte(offset++);
			m_lastOptions = provider.ReadByte(offset++);
			return;
		}
		if (version != 0)
		{
			m_iFirstRow = provider.ReadInt32(offset);
			offset += 4;
			m_iLastRow = provider.ReadInt32(offset);
			offset += 4;
			m_iFirstColumn = provider.ReadInt32(offset);
			offset += 4;
			m_firstOptions = provider.ReadByte(offset++);
			m_iLastColumn = provider.ReadInt32(offset);
			offset += 4;
			m_lastOptions = provider.ReadByte(offset++);
			return;
		}
		throw new NotImplementedException();
	}

	public static FormulaToken IndexToCode(int index)
	{
		return index switch
		{
			1 => FormulaToken.tArea1, 
			2 => FormulaToken.tArea2, 
			3 => FormulaToken.tArea3, 
			_ => throw new ArgumentOutOfRangeException("index"), 
		};
	}

	public static int CodeToIndex(FormulaToken code)
	{
		return code switch
		{
			FormulaToken.tArea1 => 1, 
			FormulaToken.tArea2 => 2, 
			FormulaToken.tArea3 => 3, 
			_ => throw new ArgumentOutOfRangeException("index"), 
		};
	}

	public override int GetSize(OfficeVersion version)
	{
		switch (version)
		{
		case OfficeVersion.Excel97to2003:
			return 9;
		case OfficeVersion.Excel2007:
		case OfficeVersion.Excel2010:
		case OfficeVersion.Excel2013:
			return 19;
		default:
			throw new ArgumentOutOfRangeException("version");
		}
	}

	public override string ToString(FormulaUtil formulaUtil, int iRow, int iColumn, bool bR1C1, NumberFormatInfo numberFormat, bool isForSerialization)
	{
		WorkbookImpl book = ((formulaUtil != null) ? ((WorkbookImpl)formulaUtil.ParentWorkbook) : null);
		bool flag = IsWholeRows(book);
		bool flag2 = IsWholeColumns(book);
		if (flag && bR1C1)
		{
			return RefPtg.GetR1C1Name(iRow, "R", FirstRow, IsFirstRowRelative) + ":" + RefPtg.GetR1C1Name(iRow, "R", LastRow, IsFirstRowRelative);
		}
		if (flag2 && bR1C1)
		{
			return RefPtg.GetR1C1Name(iColumn, "C", FirstColumn, IsFirstColumnRelative) + ":" + RefPtg.GetR1C1Name(iColumn, "C", LastColumn, IsFirstColumnRelative);
		}
		if (flag)
		{
			(FirstRow + 1).ToString();
			return "$" + (FirstRow + 1) + ":$" + (LastRow + 1);
		}
		if (flag2)
		{
			RangeImpl.GetColumnName(FirstColumn + 1);
			return "$" + RangeImpl.GetColumnName(FirstColumn + 1) + ":$" + RangeImpl.GetColumnName(LastColumn + 1);
		}
		return RefPtg.GetCellName(iRow, iColumn, FirstRow, FirstColumn, IsFirstRowRelative, IsFirstColumnRelative, bR1C1) + ":" + RefPtg.GetCellName(iRow, iColumn, LastRow, LastColumn, IsLastRowRelative, IsLastColumnRelative, bR1C1);
	}

	public override byte[] ToByteArray(OfficeVersion version)
	{
		byte[] array = base.ToByteArray(version);
		int num = 1;
		if (version == OfficeVersion.Excel97to2003)
		{
			if (m_iFirstRow > 65535 || m_iLastRow > 65535 || m_iFirstColumn > 255 || m_iLastColumn > 255)
			{
				FormulaToken correspondingErrorCode = GetCorrespondingErrorCode();
				array[0] = (byte)correspondingErrorCode;
			}
			BitConverter.GetBytes((ushort)m_iFirstRow).CopyTo(array, num);
			num += 2;
			BitConverter.GetBytes((ushort)m_iLastRow).CopyTo(array, num);
			num += 2;
			array[num++] = (byte)m_iFirstColumn;
			array[num++] = m_firstOptions;
			array[num++] = (byte)m_iLastColumn;
		}
		else if (version != 0)
		{
			BitConverter.GetBytes(m_iFirstRow).CopyTo(array, num);
			num += 4;
			BitConverter.GetBytes(m_iLastRow).CopyTo(array, num);
			num += 4;
			BitConverter.GetBytes(m_iFirstColumn).CopyTo(array, num);
			num += 4;
			array[num++] = m_firstOptions;
			BitConverter.GetBytes(m_iLastColumn).CopyTo(array, num);
			num += 4;
		}
		array[num] = m_lastOptions;
		return array;
	}

	public override Ptg Offset(int iRowOffset, int iColumnOffset, WorkbookImpl book)
	{
		AreaPtg areaPtg = (AreaPtg)base.Offset(iRowOffset, iColumnOffset, book);
		int num = (IsFirstRowRelative ? (FirstRow + iRowOffset) : FirstRow);
		int num2 = (IsFirstColumnRelative ? (FirstColumn + iColumnOffset) : FirstColumn);
		int num3 = (IsLastRowRelative ? (LastRow + iRowOffset) : LastRow);
		int num4 = (IsLastColumnRelative ? (LastColumn + iColumnOffset) : LastColumn);
		if (num < 0 || num > book.MaxRowCount - 1 || num2 < 0 || num2 > book.MaxColumnCount - 1 || num3 < 0 || num3 > book.MaxRowCount - 1 || num4 < 0 || num4 > book.MaxColumnCount - 1)
		{
			return FormulaUtil.CreatePtg(GetCorrespondingErrorCode(), this);
		}
		areaPtg.FirstRow = num;
		areaPtg.FirstColumn = num2;
		areaPtg.LastRow = num3;
		areaPtg.LastColumn = num4;
		return areaPtg;
	}

	public override Ptg Offset(int iCurSheetIndex, int iTokenRow, int iTokenColumn, int iSourceSheetIndex, Rectangle rectSource, int iDestSheetIndex, Rectangle rectDest, out bool bChanged, WorkbookImpl book)
	{
		AreaPtg areaPtg = (AreaPtg)base.Offset(iCurSheetIndex, iTokenRow, iTokenColumn, iSourceSheetIndex, rectSource, iDestSheetIndex, rectDest, out bChanged, book);
		if (iCurSheetIndex == iSourceSheetIndex)
		{
			return MoveReferencedArea(iSourceSheetIndex, rectSource, iDestSheetIndex, rectDest, ref bChanged, book);
		}
		if (iCurSheetIndex == iDestSheetIndex && iSourceSheetIndex != iDestSheetIndex && Ptg.RectangleContains(rectDest, iTokenRow, iTokenColumn))
		{
			int iRowOffset = rectDest.Top - rectSource.Top;
			int iColOffset = rectDest.Left - rectSource.Left;
			bChanged = true;
			return areaPtg.MoveIntoDifferentSheet(areaPtg, iSourceSheetIndex, rectSource, iDestSheetIndex, iRowOffset, iColOffset);
		}
		return areaPtg;
	}

	public override Ptg ConvertPtgToNPtg(IWorkbook parent, int iRow, int iColumn)
	{
		AreaNPtg obj = (AreaNPtg)FormulaUtil.CreatePtg(AreaNPtg.IndexToCode(CodeToIndex()));
		bool flag = IsWholeRows(parent);
		bool flag2 = IsWholeColumns(parent);
		int firstRow = ((IsFirstRowRelative && !flag2) ? (FirstRow - iRow) : FirstRow);
		short firstColumn = ((IsFirstColumnRelative && !flag) ? ((short)(FirstColumn - iColumn)) : ((short)FirstColumn));
		int lastRow = ((IsLastRowRelative && !flag2) ? (LastRow - iRow) : LastRow);
		short lastColumn = ((IsLastColumnRelative && !flag) ? ((short)(LastColumn - iColumn)) : ((short)LastColumn));
		obj.FirstRow = firstRow;
		obj.FirstColumn = firstColumn;
		obj.LastRow = lastRow;
		obj.LastColumn = lastColumn;
		obj.FirstOptions = FirstOptions;
		obj.LastOptions = LastOptions;
		return obj;
	}

	public AreaPtg ConvertFullRowColumnAreaPtgs(bool bFromExcel07To97)
	{
		UtilityMethods.GetMaxRowColumnCount(out var iRows, out var iColumns, OfficeVersion.Excel2007);
		UtilityMethods.GetMaxRowColumnCount(out var iRows2, out var iColumns2, OfficeVersion.Excel97to2003);
		if (bFromExcel07To97)
		{
			if (FirstColumn == 0 && LastColumn == iColumns - 1)
			{
				LastColumn = iColumns2 - 1;
			}
			else if (FirstRow == 0 && LastRow == iRows - 1)
			{
				LastRow = iRows2 - 1;
			}
			else if (FirstColumn > iColumns2 || LastColumn > iColumns2 || FirstRow > iRows2 || LastRow > iRows2)
			{
				return ConvertToErrorPtg();
			}
		}
		else if (FirstColumn == 0 && LastColumn == iColumns2 - 1)
		{
			LastColumn = iColumns - 1;
		}
		else if (FirstRow == 0 && LastRow == iRows2 - 1)
		{
			LastRow = iRows - 1;
		}
		return this;
	}

	private Ptg MoveIntoDifferentSheet(AreaPtg result, int iSourceSheetIndex, Rectangle rectSource, int iDestSheetIndex, int iRowOffset, int iColOffset)
	{
		int num = iSourceSheetIndex;
		bool num2 = ReferencedAreaMoved(rectSource);
		int num3 = result.FirstRow;
		int num4 = result.FirstColumn;
		int num5 = result.LastRow;
		int num6 = result.LastColumn;
		if (num2)
		{
			num = iDestSheetIndex;
			num3 += iRowOffset;
			num4 += iColOffset;
			num5 += iRowOffset;
			num6 += iColOffset;
		}
		return FormulaUtil.CreatePtg(Area3DPtg.IndexToCode(CodeToIndex()), num, num3, num4, num5, num6, m_firstOptions, m_lastOptions);
	}

	private bool ReferencedAreaMoved(Rectangle rectSource)
	{
		if (Ptg.RectangleContains(rectSource, FirstRow, FirstColumn))
		{
			return Ptg.RectangleContains(rectSource, LastRow, LastColumn);
		}
		return false;
	}

	private Ptg UpdateReferencedArea(int iCurSheetIndex, int iDestSheetIndex, int iRowOffset, int iColOffset, ref bool bChanged, WorkbookImpl book)
	{
		if (m_iLastRow + iRowOffset < 0 || m_iFirstColumn + iColOffset < 0 || m_iLastRow + iRowOffset > book.MaxRowCount - 1 || m_iLastColumn + iColOffset > book.MaxColumnCount - 1)
		{
			return FormulaUtil.CreatePtg(GetCorrespondingErrorCode(), ToString(book.FormulaUtil), book);
		}
		if (iCurSheetIndex == iDestSheetIndex)
		{
			m_iFirstRow += iRowOffset;
			m_iLastRow += iRowOffset;
			m_iFirstColumn += iColOffset;
			m_iLastColumn += iColOffset;
			bChanged = true;
			return this;
		}
		bChanged = true;
		return FormulaUtil.CreatePtg(Area3DPtg.IndexToCode(CodeToIndex()), iDestSheetIndex, m_iFirstRow + iRowOffset, m_iFirstColumn + iColOffset, m_iLastRow + iRowOffset, m_iLastColumn + iColOffset, m_firstOptions, m_lastOptions);
	}

	private Ptg UpdateFirstCell(int iCurSheetIndex, int iDestSheetIndex, int iRowOffset, int iColOffset, ref bool bChanged, IWorkbook book)
	{
		int num = m_iFirstRow + iRowOffset;
		int num2 = m_iFirstColumn + iColOffset;
		if (num < 0 || num2 < 0 || num > book.MaxRowCount - 1 || num2 > book.MaxColumnCount - 1)
		{
			return FormulaUtil.CreatePtg(GetCorrespondingErrorCode(), ToString());
		}
		if (num > m_iLastRow || num2 > m_iLastColumn)
		{
			return this;
		}
		if (iCurSheetIndex == iDestSheetIndex)
		{
			m_iFirstRow = num;
			m_iFirstColumn = num2;
			bChanged = true;
			return this;
		}
		bChanged = true;
		return FormulaUtil.CreatePtg(Area3DPtg.IndexToCode(CodeToIndex()), iDestSheetIndex, num, num2, m_iLastRow, m_iLastColumn, m_firstOptions, m_lastOptions);
	}

	private Ptg UpdateLastCell(int iCurSheetIndex, int iDestSheetIndex, int iRowOffset, int iColOffset, ref bool bChanged, IWorkbook book)
	{
		int num = m_iLastRow + iRowOffset;
		int num2 = m_iLastColumn + iColOffset;
		if (num < 0 || num2 < 0 || num > book.MaxRowCount - 1 || num2 > book.MaxColumnCount - 1)
		{
			return FormulaUtil.CreatePtg(GetCorrespondingErrorCode(), ToString());
		}
		if (num < m_iFirstRow || num2 < m_iFirstColumn)
		{
			return this;
		}
		if (iCurSheetIndex == iDestSheetIndex)
		{
			m_iLastRow = num;
			m_iLastColumn = num2;
			bChanged = true;
			return this;
		}
		bChanged = true;
		return FormulaUtil.CreatePtg(Area3DPtg.IndexToCode(CodeToIndex()), iDestSheetIndex, m_iFirstRow, m_iFirstColumn, num, num2, m_firstOptions, m_lastOptions);
	}

	private bool FullFirstRowMove(Rectangle rectSource)
	{
		if (Ptg.RectangleContains(rectSource, FirstRow, FirstColumn))
		{
			return Ptg.RectangleContains(rectSource, FirstRow, LastColumn);
		}
		return false;
	}

	private bool FullLastRowMove(Rectangle rectSource)
	{
		if (Ptg.RectangleContains(rectSource, LastRow, FirstColumn))
		{
			return Ptg.RectangleContains(rectSource, LastRow, LastColumn);
		}
		return false;
	}

	private bool FullFirstColMove(Rectangle rectSource)
	{
		if (Ptg.RectangleContains(rectSource, FirstRow, FirstColumn))
		{
			return Ptg.RectangleContains(rectSource, LastRow, FirstColumn);
		}
		return false;
	}

	private bool FullLastColMove(Rectangle rectSource)
	{
		if (Ptg.RectangleContains(rectSource, FirstRow, LastColumn))
		{
			return Ptg.RectangleContains(rectSource, LastRow, LastColumn);
		}
		return false;
	}

	private Ptg MoveReferencedArea(int iSourceSheetIndex, Rectangle rectSource, int iDestSheetIndex, Rectangle rectDest, ref bool bChanged, WorkbookImpl book)
	{
		int num = rectDest.Top - rectSource.Top;
		int num2 = rectSource.Top - rectDest.Top;
		int num3 = rectDest.Left - rectSource.Left;
		if (num == 0 && num3 == 0 && iSourceSheetIndex == iDestSheetIndex)
		{
			return this;
		}
		rectSource = Rectangle.FromLTRB(rectSource.Left, rectSource.Top, m_iLastColumn, m_iLastRow);
		rectDest = Rectangle.FromLTRB(rectDest.Left, rectDest.Top, m_iLastColumn, m_iLastRow + 1);
		bool flag = ((num <= 0) ? Ptg.RectangleContains(rectDest, FirstRow, FirstColumn) : Ptg.RectangleContains(rectSource, FirstRow, FirstColumn));
		bool flag2 = ((num2 <= 0) ? Ptg.RectangleContains(rectSource, LastRow, LastColumn) : Ptg.RectangleContains(rectDest, LastRow, LastColumn));
		if (m_iLastRow >= book.MaxRowCount - 1 || m_iLastColumn >= book.MaxColumnCount - 1)
		{
			flag = false;
			flag2 = false;
		}
		if (!flag && !flag2)
		{
			if (book.MaxRowCount - 1 == LastRow && FirstRow == 0)
			{
				return this;
			}
			Rectangle rect = Rectangle.FromLTRB(FirstColumn, FirstRow, LastColumn, LastRow);
			bool num4 = Ptg.RectangleContains(rect, rectDest.Top, rect.Left);
			bool flag3 = Ptg.RectangleContains(rect, rectDest.Bottom, rect.Right);
			if (!(num4 && flag3))
			{
				return this;
			}
		}
		if (num3 == 0 && iSourceSheetIndex == iDestSheetIndex)
		{
			return VerticalMove(iSourceSheetIndex, rectSource, num, rectDest, ref bChanged, book);
		}
		if (num == 0 && iSourceSheetIndex == iDestSheetIndex)
		{
			return HorizontalMove(iSourceSheetIndex, rectSource, num3, rectDest, ref bChanged, book);
		}
		if (flag || flag2)
		{
			return UpdateReferencedArea(iSourceSheetIndex, iDestSheetIndex, num, num3, ref bChanged, book);
		}
		return this;
	}

	private Ptg VerticalMove(int iSourceSheetIndex, Rectangle rectSource, int iRowOffset, Rectangle rectDest, ref bool bChanged, WorkbookImpl book)
	{
		bool flag = ((iRowOffset < 0) ? FullFirstRowMove(rectSource) : FullFirstRowMove(rectDest));
		if (rectSource.X <= rectDest.X && iRowOffset > 0 && !flag)
		{
			flag = !FullFirstRowMove(rectDest);
		}
		flag = ((m_iFirstRow >= rectSource.Y) ? true : false);
		bool flag2 = ((iRowOffset < 0) ? FullLastRowMove(rectDest) : FullLastRowMove(rectSource));
		Rectangle a = Rectangle.FromLTRB(FirstColumn, FirstRow, LastColumn, LastRow);
		if (flag && flag2)
		{
			return UpdateReferencedArea(iSourceSheetIndex, iSourceSheetIndex, iRowOffset, 0, ref bChanged, book);
		}
		if (flag)
		{
			return FirstRowVerticalMove(iSourceSheetIndex, iRowOffset, rectSource, rectDest, ref bChanged, book);
		}
		if (flag2)
		{
			return LastRowVerticalMove(iSourceSheetIndex, iRowOffset, rectSource, rectDest, ref bChanged, book);
		}
		if (!Rectangle.Intersect(a, rectDest).IsEmpty)
		{
			if (LastRow == rectDest.Bottom || FirstRow == rectDest.Top)
			{
				return this;
			}
			if (iRowOffset < 0)
			{
				LastRow = (ushort)(rectDest.Top - 1);
			}
			else
			{
				FirstRow = (ushort)(rectDest.Bottom + 1);
			}
		}
		return this;
	}

	private Ptg FirstRowVerticalMove(int iSourceSheetIndex, int iRowOffset, Rectangle rectSource, Rectangle rectDest, ref bool bChanged, IWorkbook book)
	{
		if (iRowOffset < 0)
		{
			return UpdateFirstCell(iSourceSheetIndex, iSourceSheetIndex, iRowOffset, 0, ref bChanged, book);
		}
		if (FirstRow + iRowOffset <= LastRow)
		{
			if (rectDest.Top <= rectSource.Bottom)
			{
				FirstRow = (ushort)(rectSource.Top + iRowOffset);
			}
			else
			{
				FirstRow = (ushort)(FirstRow + iRowOffset);
			}
			return this;
		}
		return this;
	}

	private Ptg LastRowVerticalMove(int iSourceSheetIndex, int iRowOffset, Rectangle rectSource, Rectangle rectDest, ref bool bChanged, IWorkbook book)
	{
		if (iRowOffset > 0)
		{
			return UpdateLastCell(iSourceSheetIndex, iSourceSheetIndex, iRowOffset, 0, ref bChanged, book);
		}
		if (LastRow + iRowOffset >= FirstRow)
		{
			LastRow = (ushort)(LastRow + iRowOffset);
		}
		return this;
	}

	private Ptg HorizontalMove(int iSourceSheetIndex, Rectangle rectSource, int iColOffset, Rectangle rectDest, ref bool bChanged, WorkbookImpl book)
	{
		bool flag = FullFirstColMove(rectSource);
		bool flag2 = FullLastColMove(rectSource);
		Rectangle rectangle = Rectangle.FromLTRB(FirstColumn, FirstRow, LastColumn, LastRow);
		if (flag && flag2)
		{
			return UpdateReferencedArea(iSourceSheetIndex, iSourceSheetIndex, 0, iColOffset, ref bChanged, book);
		}
		if (flag)
		{
			return FirstColumnHorizontalMove(iSourceSheetIndex, iColOffset, rectSource, rectDest, ref bChanged, book);
		}
		if (flag2)
		{
			return LastColumnHorizontalMove(iSourceSheetIndex, iColOffset, rectSource, rectDest, ref bChanged, book);
		}
		if (!flag && !flag2)
		{
			return this;
		}
		if (!Rectangle.Intersect(rectangle, rectDest).IsEmpty)
		{
			if (InsideRectangle(rectDest, rectangle) && OutsideRectangle(rectSource, rectangle))
			{
				return ConvertToErrorPtg();
			}
			if (LastColumn == rectDest.Right || FirstColumn == rectDest.Left)
			{
				return this;
			}
			if (iColOffset < 0)
			{
				LastColumn = (byte)(rectDest.Left - 1);
			}
			else
			{
				FirstColumn = (byte)(rectDest.Right + 1);
			}
		}
		return this;
	}

	private bool OutsideRectangle(Rectangle owner, Rectangle toCheck)
	{
		if (owner.Top <= toCheck.Bottom && owner.Bottom >= toCheck.Top && owner.Left <= toCheck.Right)
		{
			return owner.Right < toCheck.Left;
		}
		return true;
	}

	private bool InsideRectangle(Rectangle owner, Rectangle toCheck)
	{
		if (owner.Left <= toCheck.Left && owner.Right >= toCheck.Right && owner.Top <= toCheck.Top)
		{
			return owner.Bottom >= toCheck.Bottom;
		}
		return false;
	}

	private Ptg FirstColumnHorizontalMove(int iSourceSheetIndex, int iColOffset, Rectangle rectSource, Rectangle rectDest, ref bool bChanged, IWorkbook book)
	{
		if (iColOffset < 0)
		{
			return UpdateFirstCell(iSourceSheetIndex, iSourceSheetIndex, 0, iColOffset, ref bChanged, book);
		}
		if (FirstColumn + iColOffset <= LastColumn)
		{
			if (rectDest.Left <= rectSource.Right)
			{
				FirstColumn = (byte)(rectSource.Right + 1);
			}
			else
			{
				FirstColumn = (byte)(FirstColumn + iColOffset);
			}
			return this;
		}
		return this;
	}

	private Ptg LastColumnHorizontalMove(int iSourceSheetIndex, int iColOffset, Rectangle rectSource, Rectangle rectDest, ref bool bChanged, IWorkbook book)
	{
		if (iColOffset > 0)
		{
			return UpdateLastCell(iSourceSheetIndex, iSourceSheetIndex, 0, iColOffset, ref bChanged, book);
		}
		if (LastColumn + iColOffset >= FirstColumn)
		{
			if (rectDest.Right <= rectSource.Left)
			{
				LastColumn = (byte)(rectSource.Left + 1);
			}
			else
			{
				LastColumn = (byte)(LastColumn + iColOffset);
			}
		}
		return this;
	}

	public IRange GetRange(IWorkbook book, IWorksheet sheet)
	{
		if (sheet == null)
		{
			throw new ArgumentNullException("sheet");
		}
		if (FirstColumn > LastColumn)
		{
			int lastColumn = LastColumn;
			LastColumn = FirstColumn;
			FirstColumn = lastColumn;
		}
		return sheet[FirstRow + 1, FirstColumn + 1, LastRow + 1, LastColumn + 1];
	}

	public Rectangle GetRectangle()
	{
		return Rectangle.FromLTRB(FirstColumn, FirstRow, LastColumn, LastRow);
	}

	public Ptg UpdateRectangle(Rectangle rectangle)
	{
		AreaPtg obj = (AreaPtg)Clone();
		obj.FirstColumn = rectangle.Left;
		obj.LastColumn = rectangle.Right;
		obj.FirstRow = rectangle.Top;
		obj.LastRow = rectangle.Bottom;
		return obj;
	}

	public virtual Ptg ConvertToError()
	{
		return FormulaUtil.CreatePtg(GetCorrespondingErrorCode(), this);
	}

	public Ptg Get3DToken(int iSheetReference)
	{
		FormulaToken tokenCode = Area3DPtg.IndexToCode(CodeToIndex(TokenCode));
		return new Area3DPtg(iSheetReference, FirstRow, FirstColumn, LastRow, LastColumn, FirstOptions, LastOptions)
		{
			TokenCode = tokenCode
		};
	}
}
