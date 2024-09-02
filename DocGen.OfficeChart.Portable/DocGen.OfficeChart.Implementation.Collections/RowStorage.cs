using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using DocGen.Drawing;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Formula;

namespace DocGen.OfficeChart.Implementation.Collections;

internal class RowStorage : IDisposable, IBiffStorage, ICloneable, IOutline
{
	[Flags]
	private enum StorageOptions
	{
		None = 0,
		HasRKBlank = 1,
		HasMultiRKBlank = 2,
		Disposed = 4
	}

	public delegate void CellMethod(TBIFFRecord recordType, int offset, object data);

	private class OffsetData
	{
		public int StartOffset;

		public int UsedSize;
	}

	private class WriteData
	{
		public int Offset;

		public int UsedSize;
	}

	private delegate int DefragmentHelper(object helperData);

	private const int DEF_MEMORY_DELTA = 128;

	private const int DEF_MULRK_PERIOD = 6;

	private const int DEF_MULBLANK_PERIOD = 2;

	private static readonly short[] DEF_MULTIRECORDS = new short[2] { 190, 189 };

	private static readonly TBIFFRecord[] DEF_MULTIRECORDS_SUBTYPES = new TBIFFRecord[4]
	{
		TBIFFRecord.MulBlank,
		TBIFFRecord.Blank,
		TBIFFRecord.MulRK,
		TBIFFRecord.RK
	};

	private int m_iFirstColumn = -1;

	private int m_iLastColumn = -1;

	private int m_iUsedSize;

	private DataProvider m_dataProvider;

	private StorageOptions m_options;

	private int m_iCurrentColumn = -1;

	private int m_iCurrentOffset = -1;

	private OfficeVersion m_version;

	private WorkbookImpl m_book;

	private int m_row;

	private bool m_isWrapText;

	private string[] dateFormats = new string[6] { "d", "dd", "m", "mm", "yy", "yyyy" };

	private bool m_hasRowHeight;

	private ushort m_usHeight;

	private RowRecord.OptionFlags m_optionFlags = RowRecord.OptionFlags.ShowOutlineGroups;

	private ushort m_usXFIndex;

	private const string DEF_DOT = ".";

	internal TableRecord m_tableRecord;

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

	public int UsedSize => m_iUsedSize;

	public int DataSize
	{
		get
		{
			if (m_dataProvider == null)
			{
				return 0;
			}
			return m_dataProvider.Capacity;
		}
	}

	public bool HasRkBlank
	{
		get
		{
			return (m_options & StorageOptions.HasRKBlank) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= StorageOptions.HasRKBlank;
			}
			else
			{
				m_options &= ~StorageOptions.HasRKBlank;
			}
		}
	}

	public bool HasMultiRkBlank
	{
		get
		{
			return (m_options & StorageOptions.HasMultiRKBlank) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= StorageOptions.HasMultiRKBlank;
			}
			else
			{
				m_options &= ~StorageOptions.HasMultiRKBlank;
			}
		}
	}

	private bool Disposed
	{
		get
		{
			return (m_options & StorageOptions.Disposed) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= StorageOptions.Disposed;
			}
			else
			{
				m_options &= ~StorageOptions.Disposed;
			}
		}
	}

	public bool IsWrapText
	{
		get
		{
			return m_isWrapText;
		}
		set
		{
			m_isWrapText = value;
		}
	}

	public DataProvider Provider => m_dataProvider;

	public int CellPositionSize
	{
		get
		{
			if (m_version != 0)
			{
				return 8;
			}
			return 4;
		}
	}

	public OfficeVersion Version => m_version;

	internal bool HasRowHeight
	{
		get
		{
			return m_hasRowHeight;
		}
		set
		{
			m_hasRowHeight = value;
		}
	}

	public TBIFFRecord TypeCode => (TBIFFRecord)(-1);

	public int RecordCode => -1;

	public bool NeedDataArray => true;

	public long StreamPos
	{
		get
		{
			return -1L;
		}
		set
		{
		}
	}

	[CLSCompliant(false)]
	public ushort Height
	{
		get
		{
			return m_usHeight;
		}
		set
		{
			if ((double)(int)value > 8190.0)
			{
				throw new ArgumentOutOfRangeException("Row Height should be less than " + 409.5);
			}
			m_usHeight = value;
		}
	}

	[CLSCompliant(false)]
	public ushort ExtendedFormatIndex
	{
		get
		{
			return m_usXFIndex;
		}
		set
		{
			m_usXFIndex = value;
			if (value != 15)
			{
				IsFormatted = true;
			}
		}
	}

	[CLSCompliant(false)]
	public ushort OutlineLevel
	{
		get
		{
			return (ushort)(m_optionFlags & (RowRecord.OptionFlags)7);
		}
		set
		{
			if (value > 7)
			{
				throw new ArgumentOutOfRangeException();
			}
			int optionFlags = (int)m_optionFlags;
			optionFlags &= -8;
			optionFlags |= value & 7;
			m_optionFlags = (RowRecord.OptionFlags)optionFlags;
		}
	}

	public bool IsCollapsed
	{
		get
		{
			return (m_optionFlags & RowRecord.OptionFlags.Colapsed) != 0;
		}
		set
		{
			if (value)
			{
				m_optionFlags |= RowRecord.OptionFlags.Colapsed;
			}
			else
			{
				m_optionFlags &= (RowRecord.OptionFlags)(-17);
			}
		}
	}

	public bool IsHidden
	{
		get
		{
			return (m_optionFlags & RowRecord.OptionFlags.ZeroHeight) != 0;
		}
		set
		{
			if (value)
			{
				m_optionFlags |= RowRecord.OptionFlags.ZeroHeight;
			}
			else
			{
				m_optionFlags &= (RowRecord.OptionFlags)(-33);
			}
		}
	}

	public bool IsBadFontHeight
	{
		get
		{
			return (m_optionFlags & RowRecord.OptionFlags.BadFontHeight) != 0;
		}
		set
		{
			if (value)
			{
				m_optionFlags |= RowRecord.OptionFlags.BadFontHeight;
			}
			else
			{
				m_optionFlags &= (RowRecord.OptionFlags)(-65);
			}
		}
	}

	public bool IsFormatted
	{
		get
		{
			return (m_optionFlags & RowRecord.OptionFlags.Formatted) != 0;
		}
		set
		{
			if (value)
			{
				m_optionFlags |= RowRecord.OptionFlags.Formatted;
			}
			else
			{
				m_optionFlags &= (RowRecord.OptionFlags)(-129);
			}
		}
	}

	public bool IsSpaceAboveRow
	{
		get
		{
			return (m_optionFlags & RowRecord.OptionFlags.SpaceAbove) != 0;
		}
		set
		{
			if (value)
			{
				m_optionFlags |= RowRecord.OptionFlags.SpaceAbove;
			}
			else
			{
				m_optionFlags &= (RowRecord.OptionFlags)(-268435457);
			}
		}
	}

	public bool IsSpaceBelowRow
	{
		get
		{
			return (m_optionFlags & RowRecord.OptionFlags.SpaceBelow) != 0;
		}
		set
		{
			if (value)
			{
				m_optionFlags |= RowRecord.OptionFlags.SpaceBelow;
			}
			else
			{
				m_optionFlags &= (RowRecord.OptionFlags)(-536870913);
			}
		}
	}

	public bool IsGroupShown
	{
		get
		{
			return (m_optionFlags & RowRecord.OptionFlags.ShowOutlineGroups) != 0;
		}
		set
		{
			if (value)
			{
				m_optionFlags |= RowRecord.OptionFlags.ShowOutlineGroups;
			}
			else
			{
				m_optionFlags &= (RowRecord.OptionFlags)(-257);
			}
		}
	}

	ushort IOutline.Index
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public RowStorage(int iRowNumber, int height, int xfIndex)
	{
		m_usHeight = (ushort)height;
		ExtendedFormatIndex = (ushort)xfIndex;
	}

	public void Dispose(bool disposing)
	{
		if (disposing && !Disposed)
		{
			if (m_dataProvider != null)
			{
				m_dataProvider.Dispose();
				m_dataProvider = null;
				m_iFirstColumn = -1;
				m_iLastColumn = -1;
				m_iUsedSize = -1;
			}
			Disposed = true;
			GC.SuppressFinalize(this);
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	~RowStorage()
	{
		Dispose(disposing: false);
	}

	public IEnumerator GetEnumerator(RecordExtractor recordExtractor)
	{
		if (recordExtractor == null)
		{
			throw new ArgumentNullException("recordExtractor");
		}
		return new RowStorageEnumerator(this, recordExtractor);
	}

	public void SetCellStyle(int iRow, int iColumn, int iXFIndex, int iBlockSize)
	{
		bool bFound;
		int num = LocateRecord(iColumn, out bFound);
		if (!bFound)
		{
			ICellPositionFormat cellPositionFormat = UtilityMethods.CreateCell(iRow, iColumn, TBIFFRecord.Blank);
			cellPositionFormat.ExtendedFormatIndex = (ushort)iXFIndex;
			SetRecord(iColumn, cellPositionFormat, iBlockSize);
			return;
		}
		switch ((TBIFFRecord)m_dataProvider.ReadInt16(num))
		{
		case TBIFFRecord.MulRK:
			SetXFIndexMulti(num, (ushort)iXFIndex, iColumn, 6);
			break;
		case TBIFFRecord.MulBlank:
			SetXFIndexMulti(num, (ushort)iXFIndex, iColumn, 2);
			break;
		default:
			SetXFIndex(num, (ushort)iXFIndex);
			break;
		}
	}

	[CLSCompliant(false)]
	public BiffRecordRaw GetRecordAtOffset(int iOffset)
	{
		if (iOffset < 0 || iOffset >= m_iUsedSize)
		{
			throw new ArgumentOutOfRangeException("iOffset");
		}
		return BiffRecordFactory.GetRecord(m_dataProvider, iOffset, Version);
	}

	[CLSCompliant(false)]
	public ICellPositionFormat GetRecord(int iColumnIndex, int iBlockSize)
	{
		if (HasMultiRkBlank)
		{
			Decompress(bIgnoreStyles: false, iBlockSize);
		}
		if (m_iFirstColumn < 0 || iColumnIndex < m_iFirstColumn || iColumnIndex > m_iLastColumn)
		{
			return null;
		}
		bool bFound;
		int num = LocateRecord(iColumnIndex, out bFound);
		BiffRecordRaw biffRecordRaw = null;
		if (bFound)
		{
			short type = m_dataProvider.ReadInt16(num);
			int num2 = m_dataProvider.ReadInt16(num + 2);
			biffRecordRaw = BiffRecordFactory.GetRecord(type);
			biffRecordRaw.Length = num2;
			biffRecordRaw.ParseStructure(m_dataProvider, num + 4, num2, Version);
		}
		return biffRecordRaw as ICellPositionFormat;
	}

	[CLSCompliant(false)]
	public ICellPositionFormat GetRecord(int iColumnIndex, int iBlockSize, RecordExtractor recordExtractor)
	{
		if (HasMultiRkBlank)
		{
			Decompress(bIgnoreStyles: false, iBlockSize);
		}
		if (m_iFirstColumn < 0 || iColumnIndex < m_iFirstColumn || iColumnIndex > m_iLastColumn)
		{
			return null;
		}
		bool bFound;
		int num = LocateRecord(iColumnIndex, out bFound);
		BiffRecordRaw biffRecordRaw = null;
		if (bFound)
		{
			int recordType = m_dataProvider.ReadInt16(num);
			int num2 = m_dataProvider.ReadInt16(num + 2);
			biffRecordRaw = recordExtractor.GetRecord(recordType);
			biffRecordRaw.Length = num2;
			biffRecordRaw.ParseStructure(m_dataProvider, num + 4, num2, Version);
		}
		return biffRecordRaw as ICellPositionFormat;
	}

	[CLSCompliant(false)]
	public void SetRecord(int iColumnIndex, ICellPositionFormat cell, int iBlockSize)
	{
		if (HasMultiRkBlank)
		{
			Decompress(bIgnoreStyles: false, iBlockSize);
		}
		SetOrdinaryRecord(iColumnIndex, cell, iBlockSize);
	}

	public void ClearData()
	{
		m_iFirstColumn = -1;
		m_iLastColumn = -1;
		m_iUsedSize = 0;
		ExtendedFormatIndex = 0;
	}

	public void SetFormulaStringValue(int iColumnIndex, string strValue, int iBlockSize)
	{
		int iFormulaRecordOffset;
		int num = RemoveFormulaStringValue(iColumnIndex, out iFormulaRecordOffset);
		if (strValue != null)
		{
			if (num < 0)
			{
				throw new NotSupportedException("Need formula cell to set FormulaStringValue.");
			}
			FormulaRecord.SetStringValue(m_dataProvider, iFormulaRecordOffset, Version);
			StringRecord stringRecord = (StringRecord)BiffRecordFactory.GetRecord(TBIFFRecord.String);
			stringRecord.Value = strValue;
			int iRequiredSize = stringRecord.GetStoreSize(Version) + 4;
			InsertRecordData(num, 0, iRequiredSize, stringRecord, iBlockSize);
		}
	}

	[CLSCompliant(false)]
	public void SetArrayRecord(int iColumnIndex, ArrayRecord array, int iBlockSize)
	{
		int iOffset = LocateRecord(iColumnIndex, out var bFound);
		if (!bFound)
		{
			throw new ArgumentOutOfRangeException("iColumnIndex", "Cannot find record with specified index");
		}
		if (m_dataProvider.ReadInt16(iOffset) != 6)
		{
			throw new ArgumentOutOfRangeException("RecordCode", "Cannot find FormulaRecord with specified column index");
		}
		iOffset = MoveNext(iOffset);
		if (iOffset < m_iUsedSize && m_dataProvider.ReadInt16(iOffset) == 545)
		{
			RemoveRecord(iOffset);
		}
		if (array != null)
		{
			int iRequiredSize = 4 + array.GetStoreSize(Version);
			InsertRecordData(iOffset, 0, iRequiredSize, array, iBlockSize);
		}
	}

	[CLSCompliant(false)]
	public ArrayRecord GetArrayRecordByOffset(int iOffset)
	{
		if (m_dataProvider.ReadInt16(iOffset) != 6)
		{
			return null;
		}
		iOffset = MoveNext(iOffset);
		if (iOffset < m_iUsedSize && m_dataProvider.ReadInt16(iOffset) == 545)
		{
			return (ArrayRecord)BiffRecordFactory.GetRecord(m_dataProvider, iOffset, Version);
		}
		return null;
	}

	[CLSCompliant(false)]
	public ArrayRecord GetArrayRecord(int iColumnIndex)
	{
		bool bFound;
		int iOffset = LocateRecord(iColumnIndex, out bFound);
		if (!bFound)
		{
			throw new ArgumentOutOfRangeException("iColumnIndex", "Cannot find record with specified index");
		}
		return GetArrayRecordByOffset(iOffset);
	}

	public object Clone()
	{
		RowStorage rowStorage = new RowStorage(0, m_usHeight, m_usXFIndex);
		if (m_dataProvider != null && m_iUsedSize > 0 && m_iFirstColumn >= 0)
		{
			rowStorage.m_dataProvider = ApplicationImpl.CreateDataProvider();
			rowStorage.EnsureSize(DataSize, 1);
			m_dataProvider.CopyTo(0, rowStorage.m_dataProvider, 0, m_iUsedSize);
		}
		rowStorage.m_iFirstColumn = m_iFirstColumn;
		rowStorage.m_iLastColumn = m_iLastColumn;
		rowStorage.m_iUsedSize = m_iUsedSize;
		rowStorage.m_options = m_options;
		rowStorage.m_optionFlags = m_optionFlags;
		rowStorage.m_version = m_version;
		rowStorage.m_usXFIndex = m_usXFIndex;
		rowStorage.m_isWrapText = m_isWrapText;
		return rowStorage;
	}

	public object Clone(nint heapHandle)
	{
		return null;
	}

	public RowStorage Clone(int iStartColumn, int iEndColumn, int iBlockSize)
	{
		RowStorage rowStorage = new RowStorage(0, m_usHeight, m_usXFIndex);
		rowStorage.m_options = m_options;
		rowStorage.m_optionFlags = m_optionFlags;
		rowStorage.m_version = m_version;
		rowStorage.m_usXFIndex = m_usXFIndex;
		if (m_iUsedSize > 0)
		{
			iStartColumn = Math.Max(m_iFirstColumn, iStartColumn);
			iEndColumn = Math.Min(m_iLastColumn, iEndColumn);
			if (iStartColumn > iEndColumn)
			{
				return null;
			}
			Decompress(bIgnoreStyles: false, iBlockSize);
			Point offsets = GetOffsets(iStartColumn, iEndColumn, out iStartColumn, out iEndColumn);
			if (iStartColumn < 0)
			{
				return null;
			}
			int x = offsets.X;
			int iUsedSize = offsets.Y - x;
			rowStorage.m_iFirstColumn = iStartColumn;
			rowStorage.m_iLastColumn = iEndColumn;
			rowStorage.m_iUsedSize = iUsedSize;
		}
		rowStorage.m_iCurrentColumn = -1;
		rowStorage.m_iCurrentOffset = -1;
		return rowStorage;
	}

	public RowStorage Clone(SSTDictionary sourceSST, SSTDictionary destSST, Dictionary<int, int> hashExtFormatIndexes, Dictionary<string, string> hashWorksheetNames, Dictionary<int, int> dicNameIndexes, Dictionary<int, int> dicFontIndexes, Dictionary<int, int> dictExternSheets)
	{
		ApplicationImpl applicationImpl = (ApplicationImpl)destSST.Workbook.Application;
		RowStorage rowStorage = new RowStorage(0, applicationImpl.StandardHeightInRowUnits, m_usXFIndex);
		rowStorage.m_iFirstColumn = m_iFirstColumn;
		rowStorage.m_iLastColumn = m_iLastColumn;
		rowStorage.m_iUsedSize = m_iUsedSize;
		rowStorage.m_options = m_options;
		rowStorage.m_usHeight = m_usHeight;
		rowStorage.m_optionFlags = m_optionFlags;
		rowStorage.m_version = m_version;
		rowStorage.m_usXFIndex = m_usXFIndex;
		if (m_dataProvider != null && m_iUsedSize > 0 && m_iFirstColumn >= 0)
		{
			rowStorage.m_dataProvider = ApplicationImpl.CreateDataProvider();
			rowStorage.m_dataProvider.EnsureCapacity(m_iUsedSize);
			m_dataProvider.CopyTo(0, rowStorage.m_dataProvider, 0, m_iUsedSize);
		}
		rowStorage.UpdateRecordsAfterCopy(sourceSST, destSST, hashExtFormatIndexes, hashWorksheetNames, dicNameIndexes, dicFontIndexes, dictExternSheets);
		rowStorage.m_iCurrentColumn = -1;
		rowStorage.m_iCurrentOffset = -1;
		rowStorage.IsFormatted = IsFormatted;
		return rowStorage;
	}

	public void Remove(int iStartColumn, int iEndColumn, int blockSize)
	{
		if (m_iFirstColumn < 0)
		{
			return;
		}
		iStartColumn = Math.Max(m_iFirstColumn, iStartColumn);
		iEndColumn = Math.Min(m_iLastColumn, iEndColumn);
		if (iStartColumn > iEndColumn)
		{
			return;
		}
		Decompress(bIgnoreStyles: false, blockSize);
		Point offsets = GetOffsets(iStartColumn, iEndColumn, out iStartColumn, out iEndColumn);
		int x = offsets.X;
		int y = offsets.Y;
		int num = y - x;
		if (num > 0)
		{
			int num2 = m_iUsedSize - y;
			if (num2 > 0)
			{
				m_dataProvider.MoveMemory(x, y, num2);
			}
			m_iUsedSize -= num;
			UpdateColumns();
		}
	}

	public void SetArrayFormulaIndex(int iColumn, int iArrayRow, int iArrayColumn, int iBlockSize)
	{
		bool bFound;
		int iOffset = LocateRecord(iColumn, out bFound);
		if (bFound && m_dataProvider.ReadInt16(iOffset) == 6)
		{
			FormulaRecord formulaRecord = BiffRecordFactory.GetRecord(m_dataProvider, iOffset, Version) as FormulaRecord;
			Ptg[] parsedExpression = formulaRecord.ParsedExpression;
			if (parsedExpression != null && parsedExpression.Length != 0 && parsedExpression[0] is ControlPtg controlPtg)
			{
				controlPtg.RowIndex = iArrayRow;
				controlPtg.ColumnIndex = iArrayColumn;
				formulaRecord.ParsedExpression = parsedExpression;
				int num = formulaRecord.GetStoreSize(Version) + 4;
				InsertRecordData(iOffset, num, num, formulaRecord, iBlockSize);
			}
		}
	}

	public bool Contains(int iColumn)
	{
		LocateRecord(iColumn, out var bFound);
		return bFound;
	}

	public void InsertRowData(RowStorage sourceRow, int iBlockSize)
	{
		if (sourceRow == null)
		{
			throw new ArgumentNullException("sourceRow");
		}
		if (sourceRow.m_iUsedSize <= 0)
		{
			return;
		}
		m_version = sourceRow.m_version;
		int firstColumn = sourceRow.FirstColumn;
		int lastColumn = sourceRow.LastColumn;
		if (m_dataProvider == null || m_iUsedSize <= 0)
		{
			m_iUsedSize = sourceRow.m_iUsedSize;
			CreateDataProvider();
			EnsureSize(m_iUsedSize, iBlockSize);
			sourceRow.m_dataProvider.CopyTo(0, m_dataProvider, 0, m_iUsedSize);
		}
		else
		{
			Remove(firstColumn, lastColumn, iBlockSize);
			bool bFound;
			int num = LocateRecord(firstColumn, out bFound);
			int num2 = m_iUsedSize - num;
			int iUsedSize = sourceRow.m_iUsedSize;
			EnsureSize(m_iUsedSize + iUsedSize, iBlockSize);
			if (num2 > 0)
			{
				m_dataProvider.MoveMemory(num + iUsedSize, num, num2);
			}
			sourceRow.m_dataProvider.CopyTo(0, m_dataProvider, num, iUsedSize);
			m_iUsedSize += iUsedSize;
		}
		if (m_iFirstColumn >= 0)
		{
			m_iFirstColumn = Math.Min(m_iFirstColumn, firstColumn);
			m_iLastColumn = Math.Max(m_iLastColumn, lastColumn);
		}
		else
		{
			m_iFirstColumn = firstColumn;
			m_iLastColumn = lastColumn;
		}
		m_iCurrentOffset = -1;
		m_iCurrentColumn = -1;
	}

	public void UpdateFormula(int iCurIndex, int iSourceIndex, Rectangle sourceRect, int iDestIndex, Rectangle destRect, int iBlockSize, WorkbookImpl book)
	{
		if (m_dataProvider == null || m_iUsedSize <= 0 || m_iFirstColumn < 0)
		{
			return;
		}
		int i = 0;
		_ = destRect.Top;
		_ = sourceRect.Top;
		_ = destRect.Left;
		_ = sourceRect.Left;
		int num;
		for (; i < m_iUsedSize; i += num + 4)
		{
			TBIFFRecord tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(i);
			num = m_dataProvider.ReadInt16(i + 2);
			switch (tBIFFRecord)
			{
			case TBIFFRecord.Formula:
			{
				FormulaRecord formulaRecord = BiffRecordFactory.GetRecord(m_dataProvider, i, Version) as FormulaRecord;
				Ptg[] parsedExpression = formulaRecord.ParsedExpression;
				formulaRecord.ParsedExpression = book.FormulaUtil.UpdateFormula(parsedExpression, iCurIndex, iSourceIndex, sourceRect, iDestIndex, destRect, formulaRecord.Row + 1, formulaRecord.Column + 1);
				InsertRecordData(i, num + 4, formulaRecord.GetStoreSize(Version) + 4, formulaRecord, iBlockSize);
				num = formulaRecord.GetStoreSize(Version);
				break;
			}
			case TBIFFRecord.Array:
			{
				ArrayRecord arrayRecord = BiffRecordFactory.GetRecord(m_dataProvider, i, Version) as ArrayRecord;
				Ptg[] formula = arrayRecord.Formula;
				arrayRecord.Formula = book.FormulaUtil.UpdateFormula(formula, iCurIndex, iSourceIndex, sourceRect, iDestIndex, destRect, arrayRecord.FirstRow + 1, arrayRecord.FirstColumn + 1);
				int iPreparedSize = num + 4;
				InsertRecordData(i, iPreparedSize, arrayRecord.GetStoreSize(Version) + 4, arrayRecord, iBlockSize);
				num = arrayRecord.GetStoreSize(Version);
				break;
			}
			}
		}
	}

	public void RowColumnOffset(int iDeltaRow, int iDeltaCol, int iBlockSize)
	{
		int i = 0;
		m_iFirstColumn += iDeltaCol;
		m_iLastColumn += iDeltaCol;
		int num;
		for (; i < m_iUsedSize; i += num + 4)
		{
			TBIFFRecord tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(i);
			num = m_dataProvider.ReadInt16(i + 2);
			switch (tBIFFRecord)
			{
			case TBIFFRecord.Array:
			{
				ArrayRecord arrayRecord = (ArrayRecord)BiffRecordFactory.GetRecord(m_dataProvider, i, Version);
				arrayRecord.FirstColumn += iDeltaCol;
				arrayRecord.LastColumn += iDeltaCol;
				arrayRecord.FirstRow += iDeltaRow;
				arrayRecord.LastRow += iDeltaRow;
				int num2 = num + 4;
				InsertRecordData(i, num2, num2, arrayRecord, iBlockSize);
				break;
			}
			default:
			{
				int row = GetRow(i);
				SetRow(i, row + iDeltaRow);
				int column = GetColumn(i);
				SetColumn(i, column + iDeltaCol);
				if (tBIFFRecord == TBIFFRecord.MulBlank || tBIFFRecord == TBIFFRecord.MulRK)
				{
					MulBlankRecord.IncreaseLastColumn(m_dataProvider, i, num, Version, iDeltaCol);
				}
				break;
			}
			case TBIFFRecord.String:
				break;
			}
		}
	}

	public void UpdateNameIndexes(WorkbookImpl book, int[] arrNewIndex, int iBlockSize)
	{
		if (arrNewIndex == null)
		{
			throw new ArgumentNullException("arrNewIndex");
		}
		int num;
		for (int i = 0; i < m_iUsedSize; i += num + 4)
		{
			TBIFFRecord tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(i);
			num = m_dataProvider.ReadInt16(i + 2);
			if (tBIFFRecord == TBIFFRecord.Formula || tBIFFRecord == TBIFFRecord.Array)
			{
				IFormulaRecord formulaRecord = (IFormulaRecord)BiffRecordFactory.GetRecord(m_dataProvider, i, Version);
				Ptg[] formula = formulaRecord.Formula;
				if (book.FormulaUtil.UpdateNameIndex(formula, arrNewIndex))
				{
					formulaRecord.Formula = formula;
				}
				BiffRecordRaw biffRecordRaw = (BiffRecordRaw)formulaRecord;
				InsertRecordData(i, num + 4, biffRecordRaw.GetStoreSize(Version) + 4, biffRecordRaw, iBlockSize);
				num = biffRecordRaw.GetStoreSize(Version);
			}
		}
	}

	public void UpdateNameIndexes(WorkbookImpl book, IDictionary<int, int> dicNewIndex, int iBlockSize)
	{
		if (dicNewIndex == null)
		{
			throw new ArgumentNullException("dicNewIndex");
		}
		int num;
		for (int i = 0; i < m_iUsedSize; i += num + 4)
		{
			TBIFFRecord tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(i);
			num = m_dataProvider.ReadInt16(i + 2);
			if (tBIFFRecord == TBIFFRecord.Formula || tBIFFRecord == TBIFFRecord.Array)
			{
				IFormulaRecord formulaRecord = (IFormulaRecord)BiffRecordFactory.GetRecord(m_dataProvider, i, Version);
				Ptg[] formula = formulaRecord.Formula;
				if (book.FormulaUtil.UpdateNameIndex(formula, dicNewIndex))
				{
					formulaRecord.Formula = formula;
				}
				BiffRecordRaw biffRecordRaw = (BiffRecordRaw)formulaRecord;
				InsertRecordData(i, num + 4, biffRecordRaw.GetStoreSize(Version) + 4, biffRecordRaw, iBlockSize);
				num = biffRecordRaw.GetStoreSize(Version);
			}
		}
	}

	[CLSCompliant(false)]
	public void ReplaceSharedFormula(WorkbookImpl book, int row, int column, SharedFormulaRecord shared)
	{
		int rowStorageAllocationBlockSize = book.Application.RowStorageAllocationBlockSize;
		if (HasMultiRkBlank)
		{
			Decompress(bIgnoreStyles: false, rowStorageAllocationBlockSize);
		}
		if (m_iFirstColumn > shared.LastColumn || m_iLastColumn < shared.FirstColumn)
		{
			return;
		}
		bool bFound;
		int i = LocateRecord(shared.FirstColumn, out bFound);
		int num = -1;
		int num2;
		for (; i < m_iUsedSize; i += num2 + 4)
		{
			TBIFFRecord tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(i);
			num2 = m_dataProvider.ReadInt16(i + 2);
			if (tBIFFRecord == TBIFFRecord.Array || tBIFFRecord == TBIFFRecord.String)
			{
				continue;
			}
			num = GetColumn(i);
			if (num > shared.LastColumn)
			{
				break;
			}
			if (num < shared.FirstColumn || tBIFFRecord != TBIFFRecord.Formula)
			{
				continue;
			}
			FormulaRecord formulaRecord = (FormulaRecord)BiffRecordFactory.GetRecord(m_dataProvider, i, Version);
			formulaRecord.CalculateOnOpen = true;
			formulaRecord.RecalculateAlways = true;
			formulaRecord.PartOfSharedFormula = false;
			Ptg[] parsedExpression = formulaRecord.ParsedExpression;
			if (parsedExpression != null && parsedExpression.Length == 1 && parsedExpression[0].TokenCode == FormulaToken.tExp)
			{
				ControlPtg controlPtg = (ControlPtg)parsedExpression[0];
				if (controlPtg.RowIndex == row && controlPtg.ColumnIndex == column)
				{
					parsedExpression = FormulaUtil.ConvertSharedFormulaTokens(shared, book, formulaRecord.Row, num);
					formulaRecord.ParsedExpression = parsedExpression;
					int storeSize = formulaRecord.GetStoreSize(Version);
					InsertRecordData(i, num2 + 4, storeSize + 4, formulaRecord, rowStorageAllocationBlockSize);
					num2 = storeSize;
				}
			}
		}
	}

	public void UpdateStringIndexes(List<int> arrNewIndexes)
	{
		if (arrNewIndexes == null)
		{
			throw new ArgumentNullException("arrNewIndexes");
		}
		int num2;
		for (int i = 0; i < m_iUsedSize; i += num2 + 4)
		{
			short num = m_dataProvider.ReadInt16(i);
			num2 = m_dataProvider.ReadInt16(i + 2);
			if (num == 253)
			{
				int sSTIndex = LabelSSTRecord.GetSSTIndex(m_dataProvider, i, Version);
				sSTIndex = arrNewIndexes[sSTIndex];
				LabelSSTRecord.SetSSTIndex(m_dataProvider, i, sSTIndex, Version);
			}
		}
	}

	public List<long> Find(int iFirstColumn, int iLastColumn, string findValue, OfficeFindType flags, int iErrorCode, bool bIsFindFirst, WorkbookImpl book)
	{
		return Find(iFirstColumn, iLastColumn, findValue, flags, OfficeFindOptions.None, iErrorCode, bIsFindFirst, book);
	}

	public List<long> Find(int iFirstColumn, int iLastColumn, string findValue, OfficeFindType flags, OfficeFindOptions findOptions, int iErrorCode, bool bIsFindFirst, WorkbookImpl book)
	{
		bool flag = (flags & OfficeFindType.Text) == OfficeFindType.Text;
		bool flag2 = (flags & OfficeFindType.Error) == OfficeFindType.Error;
		bool flag3 = (flags & OfficeFindType.Formula) == OfficeFindType.Formula;
		bool flag4 = (flags & OfficeFindType.FormulaStringValue) == OfficeFindType.FormulaStringValue;
		if (!(flag || flag3 || flag2 || flag4))
		{
			throw new ArgumentException("Parameter flags is not valid.", "flags");
		}
		List<long> list = new List<long>();
		if (m_dataProvider == null || m_iUsedSize <= 0 || iLastColumn < m_iFirstColumn || iFirstColumn > m_iLastColumn)
		{
			return list;
		}
		bool bFound;
		for (int num = LocateRecord(iFirstColumn, out bFound); num < m_iUsedSize; num = MoveNext(num))
		{
			TBIFFRecord tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(num);
			int num2 = m_dataProvider.ReadInt16(num + 2);
			int row = GetRow(num);
			int column = GetColumn(num);
			bFound = false;
			if (column > iLastColumn)
			{
				break;
			}
			switch (tBIFFRecord)
			{
			case TBIFFRecord.Label:
				if (flag)
				{
					LabelRecord labelRecord = (LabelRecord)BiffRecordFactory.GetRecord(m_dataProvider, num, Version);
					bFound = ((findOptions != 0) ? CheckStringValue(labelRecord.Label, findValue, findOptions, book) : (labelRecord.Label == findValue));
				}
				break;
			case TBIFFRecord.LabelSST:
			{
				int sSTIndex = LabelSSTRecord.GetSSTIndex(m_dataProvider, num, Version);
				string text2 = book.InnerSST[sSTIndex].Text;
				bFound = ((findOptions != 0) ? CheckStringValue(text2, findValue, findOptions, book) : text2.ToLower().Contains(findValue.ToLower()));
				break;
			}
			case TBIFFRecord.BoolErr:
				if (flag2)
				{
					BoolErrRecord boolErrRecord = (BoolErrRecord)BiffRecordFactory.GetRecord(m_dataProvider, num, Version);
					bFound = boolErrRecord.IsErrorCode && boolErrRecord.BoolOrError == iErrorCode;
				}
				break;
			case TBIFFRecord.Formula:
			{
				if (flag3)
				{
					FormulaRecord formula = (FormulaRecord)BiffRecordFactory.GetRecord(m_dataProvider, num, Version);
					string text = "=" + book.FormulaUtil.ParseFormulaRecord(formula);
					bFound = ((findOptions != 0) ? CheckStringValue(text, findValue, findOptions, book) : text.ToLower().Contains(findValue.ToLower()));
				}
				int num3 = num + num2 + 4;
				if (num3 >= m_iUsedSize)
				{
					break;
				}
				tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(num3);
				if (tBIFFRecord == TBIFFRecord.Array)
				{
					num = num3;
					num3 = MoveNext(num3);
					tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(num3);
				}
				if (tBIFFRecord == TBIFFRecord.String)
				{
					num = num3;
					if (flag4)
					{
						StringRecord stringRecord = (StringRecord)BiffRecordFactory.GetRecord(m_dataProvider, num3, Version);
						bFound = ((findOptions != 0) ? CheckStringValue(stringRecord.Value, findValue, findOptions, book) : (stringRecord.Value == findValue));
					}
				}
				break;
			}
			}
			if (bFound)
			{
				long cellIndex = RangeImpl.GetCellIndex(column + 1, row + 1);
				list.Add(cellIndex);
				if (bIsFindFirst)
				{
					break;
				}
			}
		}
		return list;
	}

	private bool CheckStringValue(string first, string second, OfficeFindOptions options, WorkbookImpl book)
	{
		bool flag = false;
		StringComparison comparisonType = (((options & OfficeFindOptions.MatchCase) == 0) ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture);
		bool flag2 = (options & OfficeFindOptions.MatchEntireCellContent) != 0;
		if (book.IsStartsOrEndsWith.HasValue && !flag2)
		{
			if (book.IsStartsOrEndsWith != true)
			{
				return first.EndsWith(second, comparisonType);
			}
			return first.StartsWith(second, comparisonType);
		}
		if (flag2)
		{
			return first == second;
		}
		return first.IndexOf(second, 0, comparisonType) == 0;
	}

	public List<long> Find(int iFirstColumn, int iLastColumn, double findValue, OfficeFindType flags, bool bIsFindFirst, WorkbookImpl book)
	{
		bool flag = (flags & OfficeFindType.FormulaValue) != 0;
		bool flag2 = (flags & OfficeFindType.Number) != 0;
		if (!(flag || flag2))
		{
			throw new ArgumentException("Parameter flags is not valid.", "flags");
		}
		List<long> list = new List<long>();
		if (m_dataProvider == null || m_iUsedSize <= 0 || iLastColumn < m_iFirstColumn || iFirstColumn > m_iLastColumn)
		{
			return list;
		}
		Decompress(bIgnoreStyles: false, book.Application.RowStorageAllocationBlockSize);
		bool bFound;
		for (int num = LocateRecord(iFirstColumn, out bFound); num < m_iUsedSize; num = MoveNext(num))
		{
			TBIFFRecord tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(num);
			int num2 = m_dataProvider.ReadInt16(num + 2);
			int row = GetRow(num);
			int column = GetColumn(num);
			bFound = false;
			if (column > iLastColumn)
			{
				break;
			}
			switch (tBIFFRecord)
			{
			case TBIFFRecord.Number:
			case TBIFFRecord.RK:
				if (flag2)
				{
					bFound = ((IDoubleValue)BiffRecordFactory.GetRecord(m_dataProvider, num, Version)).DoubleValue == findValue;
				}
				break;
			case TBIFFRecord.Formula:
			{
				if (flag)
				{
					bFound = ((FormulaRecord)BiffRecordFactory.GetRecord(m_dataProvider, num, Version)).Value == findValue;
				}
				int num3 = num + num2 + 4;
				if (num3 < m_iUsedSize)
				{
					tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(num3);
					while (tBIFFRecord == TBIFFRecord.Array || tBIFFRecord == TBIFFRecord.String)
					{
						num = num3;
						num3 = MoveNext(num3);
						tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(num3);
					}
				}
				break;
			}
			}
			if (bFound)
			{
				long cellIndex = RangeImpl.GetCellIndex(column + 1, row + 1);
				list.Add(cellIndex);
				if (bIsFindFirst)
				{
					break;
				}
			}
		}
		return list;
	}

	public List<long> Find(int iFirstColumn, int iLastColumn, byte findValue, bool bError, bool bIsFindFirst, WorkbookImpl book)
	{
		List<long> list = new List<long>();
		if (m_dataProvider == null || m_iUsedSize <= 0 || iLastColumn < m_iFirstColumn || iFirstColumn > m_iLastColumn)
		{
			return list;
		}
		bool bFound;
		for (int num = LocateRecord(iFirstColumn, out bFound); num < m_iUsedSize; num = MoveNext(num))
		{
			TBIFFRecord tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(num);
			int num2 = m_dataProvider.ReadInt16(num + 2);
			int row = GetRow(num);
			int column = GetColumn(num);
			bFound = false;
			if (column > iLastColumn)
			{
				break;
			}
			switch (tBIFFRecord)
			{
			case TBIFFRecord.BoolErr:
			{
				BoolErrRecord boolErrRecord = (BoolErrRecord)BiffRecordFactory.GetRecord(m_dataProvider, num, Version);
				bFound = boolErrRecord.IsErrorCode == bError && boolErrRecord.BoolOrError == findValue;
				break;
			}
			case TBIFFRecord.Formula:
			{
				int num3 = num + num2 + 4;
				if (num3 < m_iUsedSize)
				{
					tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(num3);
					while ((num3 < m_iUsedSize && tBIFFRecord == TBIFFRecord.Array) || tBIFFRecord == TBIFFRecord.String)
					{
						num = num3;
						num3 = MoveNext(num3);
						tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(num3);
					}
				}
				break;
			}
			}
			if (bFound)
			{
				long cellIndex = RangeImpl.GetCellIndex(column + 1, row + 1);
				list.Add(cellIndex);
				if (bIsFindFirst)
				{
					break;
				}
			}
		}
		return list;
	}

	public void Find(Dictionary<int, object> dictIndexes, List<long> arrRanges)
	{
		if (dictIndexes == null || dictIndexes.Count == 0 || m_iUsedSize <= 0)
		{
			return;
		}
		for (int num = 0; num < m_iUsedSize; num = MoveNext(num))
		{
			if (m_dataProvider.ReadInt16(num) == 253)
			{
				int sSTIndex = LabelSSTRecord.GetSSTIndex(m_dataProvider, num, Version);
				if (dictIndexes.ContainsKey(sSTIndex))
				{
					int firstRow = GetRow(num) + 1;
					int firstColumn = GetColumn(num) + 1;
					arrRanges.Add(RangeImpl.GetCellIndex(firstColumn, firstRow));
				}
			}
		}
	}

	public int MoveNextCell(int iOffset)
	{
		if (iOffset < m_iUsedSize)
		{
			TBIFFRecord tBIFFRecord = TBIFFRecord.Unknown;
			do
			{
				int num = m_dataProvider.ReadInt16(iOffset + 2);
				iOffset += 4 + num;
				if (iOffset >= m_iUsedSize)
				{
					break;
				}
				tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(iOffset);
			}
			while (iOffset < m_iUsedSize && (tBIFFRecord == TBIFFRecord.Array || tBIFFRecord == TBIFFRecord.String));
		}
		return iOffset;
	}

	public void UpdateExtendedFormatIndex(Dictionary<int, int> dictFormats, int iBlockSize)
	{
		if (m_iUsedSize <= 0)
		{
			return;
		}
		int value;
		for (int num = 0; num < m_iUsedSize; num = MoveNext(num))
		{
			switch ((TBIFFRecord)m_dataProvider.ReadInt16(num))
			{
			case TBIFFRecord.MulRK:
			case TBIFFRecord.MulBlank:
				throw new NotImplementedException();
			default:
			{
				int xFIndex = GetXFIndex(num, bMulti: false);
				if (dictFormats.TryGetValue(xFIndex, out value))
				{
					SetXFIndex(num, (ushort)value);
				}
				break;
			}
			case TBIFFRecord.String:
			case TBIFFRecord.Array:
				break;
			}
		}
		int extendedFormatIndex = ExtendedFormatIndex;
		if (dictFormats.TryGetValue(extendedFormatIndex, out value))
		{
			ExtendedFormatIndex = (ushort)value;
		}
	}

	public void UpdateExtendedFormatIndex(int[] arrFormats, int iBlockSize)
	{
		if (m_iUsedSize <= 0)
		{
			return;
		}
		for (int num = 0; num < m_iUsedSize; num = MoveNext(num))
		{
			switch ((TBIFFRecord)m_dataProvider.ReadInt16(num))
			{
			case TBIFFRecord.MulRK:
			case TBIFFRecord.MulBlank:
				throw new NotImplementedException();
			default:
			{
				int xFIndex = GetXFIndex(num, bMulti: false);
				xFIndex = arrFormats[xFIndex];
				SetXFIndex(num, (ushort)xFIndex);
				break;
			}
			case TBIFFRecord.String:
			case TBIFFRecord.Array:
				break;
			}
		}
		int extendedFormatIndex = ExtendedFormatIndex;
		ExtendedFormatIndex = (ushort)arrFormats[extendedFormatIndex];
	}

	public void UpdateExtendedFormatIndex(int maxCount, int defaultXF)
	{
		int num;
		for (int i = 0; i < m_iUsedSize; i += num + 4)
		{
			TBIFFRecord tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(i);
			num = m_dataProvider.ReadInt16(i + 2);
			if (tBIFFRecord != TBIFFRecord.Array && tBIFFRecord != TBIFFRecord.String && GetXFIndex(i, bMulti: false) >= maxCount)
			{
				SetXFIndex(i, (ushort)defaultXF);
			}
		}
	}

	public void UpdateLabelSSTIndexes(Dictionary<int, int> dictUpdatedIndexes, IncreaseIndex method)
	{
		if (dictUpdatedIndexes == null)
		{
			throw new ArgumentNullException("dictUpdatedIndexes");
		}
		if (dictUpdatedIndexes.Count == 0)
		{
			return;
		}
		int num2;
		for (int i = 0; i < m_iUsedSize; i += num2 + 4)
		{
			short num = m_dataProvider.ReadInt16(i);
			num2 = m_dataProvider.ReadInt16(i + 2);
			if (num == 253)
			{
				int sSTIndex = LabelSSTRecord.GetSSTIndex(m_dataProvider, i, Version);
				if (dictUpdatedIndexes.TryGetValue(sSTIndex, out var value))
				{
					sSTIndex = value;
					LabelSSTRecord.SetSSTIndex(m_dataProvider, i, sSTIndex, Version);
					method(sSTIndex);
				}
			}
		}
	}

	public void AppendRecordData(short type, short length, BiffRecordRaw data, int iBlockSize)
	{
		EnsureSize(m_iUsedSize + 4 + length, iBlockSize);
		m_dataProvider.WriteInt16(m_iUsedSize, type);
		m_iUsedSize += 2;
		m_dataProvider.WriteInt16(m_iUsedSize, length);
		m_iUsedSize += 2;
		data.InfillInternalData(m_dataProvider, m_iUsedSize, Version);
		m_iUsedSize += length;
	}

	[CLSCompliant(false)]
	public void AppendRecordData(short type, short length, byte[] data, int iBlockSize)
	{
		EnsureSize(m_iUsedSize + 4 + length, iBlockSize);
		m_dataProvider.WriteInt16(m_iUsedSize, type);
		m_iUsedSize += 2;
		m_dataProvider.WriteInt16(m_iUsedSize, length);
		m_iUsedSize += 2;
		m_dataProvider.WriteBytes(m_iUsedSize, data, 0, length);
		m_iUsedSize += length;
	}

	public void AppendRecordData(int length, byte[] data, int iBlockSize)
	{
		EnsureSize(m_iUsedSize + length, iBlockSize);
		m_dataProvider.WriteBytes(m_iUsedSize, data, 0, length);
		m_iUsedSize += length;
	}

	public void InsertRecordData(int columnIndex, int length, byte[] data, int iBlockSize)
	{
		EnsureSize(m_iUsedSize + length, iBlockSize);
		bool bFound;
		int num = LocateRecord(columnIndex, out bFound);
		if (bFound)
		{
			RemoveRecord(num);
			InsertRecordData(columnIndex, length, data, iBlockSize);
		}
		else
		{
			m_dataProvider.MoveMemory(num + length, num, m_iUsedSize - num);
			m_dataProvider.WriteBytes(num, data, 0, length);
			m_iUsedSize += length;
		}
	}

	[CLSCompliant(false)]
	public void AppendRecordData(BiffRecordRaw[] records, byte[] arrBuffer, bool bIgnoreStyles, int iBlockSize)
	{
		int num = records.Length;
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			BiffRecordRaw biffRecordRaw = records[i];
			num2 += 4 + biffRecordRaw.GetStoreSize(Version);
		}
		EnsureSize(m_iUsedSize + num2, iBlockSize);
		for (int j = 0; j < num; j++)
		{
			BiffRecordRaw biffRecordRaw2 = records[j];
			AppendRecordData((short)biffRecordRaw2.RecordCode, (short)biffRecordRaw2.GetStoreSize(Version), biffRecordRaw2, iBlockSize);
		}
	}

	public void Decompress(bool bIgnoreStyles, int iBlockSize)
	{
		if (HasMultiRkBlank)
		{
			MulBlankRecord mulBlank = (MulBlankRecord)BiffRecordFactory.GetRecord(TBIFFRecord.MulBlank);
			MulRKRecord mulRK = (MulRKRecord)BiffRecordFactory.GetRecord(TBIFFRecord.MulRK);
			int iSizeDelta;
			List<int> multiRecordsOffsets = GetMultiRecordsOffsets(mulBlank, mulRK, out iSizeDelta);
			if (iSizeDelta != 0)
			{
				EnsureSize(m_iUsedSize + iSizeDelta, iBlockSize);
				DecompressStorage(multiRecordsOffsets, iSizeDelta, mulBlank, mulRK, bIgnoreStyles);
				HasMultiRkBlank = false;
				HasRkBlank = true;
				m_iCurrentColumn = -1;
				m_iCurrentOffset = -1;
			}
		}
	}

	public void Compress()
	{
		if (m_iUsedSize > 0 && HasRkBlank)
		{
			WriteData writeData = new WriteData();
			writeData.UsedSize = 0;
			DefragmentHelper rkRecordHelper = CompressRKRecords;
			DefragmentHelper blankRecordHelper = CompressBlankRecords;
			DefragmentHelper ordinaryHelper = CompressRecord;
			DefragmentDataStorage(rkRecordHelper, blankRecordHelper, ordinaryHelper, writeData);
			m_iUsedSize = writeData.UsedSize;
			HasRkBlank = false;
		}
	}

	public bool PrepareRowData(SSTDictionary sst, ref Dictionary<long, SharedFormulaRecord> arrShared)
	{
		if (sst == null)
		{
			throw new ArgumentNullException("sst");
		}
		int firstRow = -1;
		int firstColumn = -1;
		int i = 0;
		List<int> list = new List<int>();
		bool result = true;
		int num2;
		for (; i < m_iUsedSize; i += num2 + 4)
		{
			if (i < 0)
			{
				result = false;
				break;
			}
			int num = m_dataProvider.ReadInt32(i);
			TBIFFRecord tBIFFRecord = (TBIFFRecord)(num & 0xFFFF);
			num2 = num >> 16;
			switch (tBIFFRecord)
			{
			case TBIFFRecord.LabelSST:
			{
				int index = m_dataProvider.ReadInt32(i + 6 + 4);
				sst.AddIncrease(index);
				break;
			}
			case TBIFFRecord.MulRK:
			case TBIFFRecord.MulBlank:
				m_options |= StorageOptions.HasMultiRKBlank;
				break;
			case TBIFFRecord.Formula:
				firstRow = GetRow(i);
				firstColumn = GetColumn(i);
				break;
			case TBIFFRecord.SharedFormula2:
			{
				list.Add(i);
				if (arrShared == null)
				{
					arrShared = new Dictionary<long, SharedFormulaRecord>();
				}
				SharedFormulaRecord value = (SharedFormulaRecord)BiffRecordFactory.GetRecord(m_dataProvider, i, Version);
				long cellIndex = RangeImpl.GetCellIndex(firstColumn, firstRow);
				arrShared.Add(cellIndex, value);
				break;
			}
			}
		}
		int count = list.Count;
		if (count > 0)
		{
			for (int num3 = count - 1; num3 >= 0; num3--)
			{
				i = list[num3];
				RemoveRecord(i);
			}
		}
		return result;
	}

	[CLSCompliant(false)]
	public void UpdateRowInfo(RowRecord row, bool useFastParsing)
	{
		if (useFastParsing)
		{
			m_iFirstColumn = row.FirstColumn;
			m_iLastColumn = row.LastColumn;
		}
		m_optionFlags = (RowRecord.OptionFlags)row.Options;
		m_usHeight = row.Height;
		m_usXFIndex = row.ExtendedFormatIndex;
	}

	[CLSCompliant(false)]
	public RowRecord CreateRowRecord(WorkbookImpl book)
	{
		RowRecord obj = (RowRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Row);
		obj.FirstColumn = (ushort)Math.Max(0, m_iFirstColumn);
		obj.LastColumn = (ushort)Math.Max(0, m_iLastColumn);
		obj.Height = m_usHeight;
		obj.Options = (int)m_optionFlags;
		obj.ExtendedFormatIndex = ((m_usXFIndex > book.MaxXFCount) ? ((ushort)book.DefaultXFIndex) : m_usXFIndex);
		return obj;
	}

	public void CopyRowRecordFrom(RowStorage sourceRow)
	{
		m_usHeight = sourceRow.m_usHeight;
		m_optionFlags = sourceRow.m_optionFlags;
		m_usXFIndex = sourceRow.ExtendedFormatIndex;
	}

	public void SetDefaultRowOptions()
	{
		m_optionFlags = RowRecord.OptionFlags.ShowOutlineGroups;
	}

	public void UpdateColumnIndexes(int iColumnIndex, int iLastColumnIndex)
	{
		m_iFirstColumn = ((m_iUsedSize == 0 || m_iFirstColumn < 0) ? iColumnIndex : (m_iFirstColumn = Math.Min(m_iFirstColumn, iColumnIndex)));
		m_iLastColumn = Math.Max(iLastColumnIndex, m_iLastColumn);
	}

	public void SetCellPositionSize(int newSize, int iBlockSize, OfficeVersion version)
	{
		if (CellPositionSize != newSize)
		{
			switch (newSize)
			{
			case 4:
				ShrinkDataStorage();
				break;
			case 8:
				ExtendDataStorage(iBlockSize);
				break;
			default:
				throw new NotSupportedException();
			}
			m_version = version;
		}
	}

	public int GetXFIndexByColumn(int column)
	{
		bool bFound;
		bool bMul;
		int recordStart = LocateRecord(column, out bFound, out bMul, bGetRkOffset: true);
		if (!bFound)
		{
			return int.MinValue;
		}
		return GetXFIndex(recordStart, bMul);
	}

	public void ReAddAllStrings(SSTDictionary sst)
	{
		int i = 0;
		int num = 253;
		int num3;
		for (; i < m_iUsedSize; i += num3 + 4)
		{
			short num2 = m_dataProvider.ReadInt16(i);
			num3 = m_dataProvider.ReadInt16(i + 2);
			if (num2 == num)
			{
				int sSTIndex = LabelSSTRecord.GetSSTIndex(m_dataProvider, i, Version);
				sst.AddIncrease(sSTIndex);
			}
		}
	}

	public void SetVersion(OfficeVersion version, int iBlockSize)
	{
		if (Version != version)
		{
			switch (version)
			{
			case OfficeVersion.Excel97to2003:
				ShrinkDataStorage();
				break;
			case OfficeVersion.Excel2007:
			case OfficeVersion.Excel2010:
			case OfficeVersion.Excel2013:
				ExtendDataStorage(iBlockSize);
				break;
			default:
				throw new ArgumentOutOfRangeException("version");
			}
			m_version = version;
		}
	}

	private void UpdateColumns()
	{
		m_iFirstColumn = -1;
		m_iLastColumn = -1;
		int num;
		for (int i = 0; i < m_iUsedSize; i += num + 4)
		{
			TBIFFRecord tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(i);
			num = m_dataProvider.ReadInt16(i + 2);
			int column = GetColumn(i);
			if (tBIFFRecord != TBIFFRecord.Array && tBIFFRecord != TBIFFRecord.String)
			{
				AccessColumn(column);
				if (tBIFFRecord == TBIFFRecord.MulRK || tBIFFRecord == TBIFFRecord.MulBlank)
				{
					int iColumnIndex = m_dataProvider.ReadInt16(i + num + 4 - 2);
					AccessColumn(iColumnIndex);
				}
			}
		}
	}

	public void IterateCells(CellMethod method, object data)
	{
		int num;
		for (int i = 0; i < m_iUsedSize; i += num + 4)
		{
			TBIFFRecord recordType = (TBIFFRecord)m_dataProvider.ReadInt16(i);
			num = m_dataProvider.ReadInt16(i + 2);
			method(recordType, i, data);
		}
	}

	public void MarkCellUsedReferences(TBIFFRecord recordType, int offset, object data)
	{
		if (recordType == TBIFFRecord.Formula || recordType == TBIFFRecord.Array)
		{
			FormulaUtil.MarkUsedReferences((BiffRecordFactory.GetRecord(m_dataProvider, offset, Version) as IFormulaRecord).Formula, (bool[])data);
		}
	}

	public void UpdateReferenceIndexes(TBIFFRecord recordType, int offset, object data)
	{
		if (recordType == TBIFFRecord.Formula || recordType == TBIFFRecord.Array)
		{
			FormulaUtil.UpdateReferenceIndexes((BiffRecordFactory.GetRecord(m_dataProvider, offset, Version) as IFormulaRecord).Formula, (int[])data);
		}
	}

	internal void CreateDataProvider()
	{
		if (m_dataProvider == null)
		{
			m_dataProvider = ApplicationImpl.CreateDataProvider();
			if (m_book != null && m_book.MaxImportColumns > 1)
			{
				m_dataProvider.EnsureCapacity(18 * m_book.MaxImportColumns);
			}
			else
			{
				m_dataProvider.EnsureCapacity(18);
			}
		}
	}

	public void MarkUsedReferences(bool[] usedItems)
	{
		IterateCells(MarkCellUsedReferences, usedItems);
	}

	public void UpdateReferenceIndexes(int[] arrUpdatedIndexes)
	{
		IterateCells(UpdateReferenceIndexes, arrUpdatedIndexes);
	}

	public int FindRecord(TBIFFRecord recordType, int startColumn, int endColumn)
	{
		if (startColumn > m_iLastColumn || endColumn > m_iLastColumn)
		{
			return endColumn + 1;
		}
		bool bFound;
		int num = LocateRecord(startColumn, out bFound);
		if (!bFound && num >= m_iUsedSize)
		{
			return endColumn + 1;
		}
		int num2 = (bFound ? startColumn : GetColumn(num));
		if (num >= m_iUsedSize)
		{
			num2 = endColumn + 1;
		}
		TBIFFRecord tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(num);
		while (num2 <= endColumn && tBIFFRecord != recordType)
		{
			num = MoveNext(num);
			if (num >= m_iUsedSize)
			{
				num2 = endColumn + 1;
				break;
			}
			tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(num);
			if (tBIFFRecord != TBIFFRecord.String && tBIFFRecord != TBIFFRecord.Array)
			{
				num2 = GetColumn(num);
			}
		}
		if (num2 <= endColumn && num < m_iUsedSize)
		{
			m_iCurrentColumn = num2;
			m_iCurrentOffset = num;
		}
		return num2;
	}

	public int FindFirstCell(int startColumn, int endColumn)
	{
		bool bFound;
		int num = LocateRecord(startColumn, out bFound);
		if (num >= UsedSize)
		{
			return endColumn + 1;
		}
		return GetColumn(num);
	}

	internal void GetUsedNames(Dictionary<int, object> result)
	{
		if (result == null)
		{
			throw new ArgumentNullException("result");
		}
		for (int num = 0; num < m_iUsedSize; num = MoveNext(num))
		{
			TBIFFRecord tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(num);
			if (tBIFFRecord == TBIFFRecord.Formula || tBIFFRecord == TBIFFRecord.Array)
			{
				Ptg[] formula = ((IFormulaRecord)BiffRecordFactory.GetRecord(m_dataProvider, num, Version)).Formula;
				AddNamedRangeTokens(result, formula);
			}
		}
	}

	private void AddNamedRangeTokens(Dictionary<int, object> result, Ptg[] tokens)
	{
		if (result == null)
		{
			throw new ArgumentNullException("result");
		}
		if (tokens == null)
		{
			return;
		}
		for (int num = tokens.Length - 1; num >= 0; num--)
		{
			if (tokens[num] is NamePtg namePtg)
			{
				result[namePtg.ExternNameIndex - 1] = null;
			}
		}
	}

	private void InsertRecordData(int iOffset, BiffRecordRaw[] records)
	{
		int i = 0;
		for (int num = records.Length; i < num; i++)
		{
			BiffRecordRaw biffRecordRaw = records[i];
			m_dataProvider.WriteInt16(iOffset, (short)biffRecordRaw.RecordCode);
			iOffset += 2;
			int storeSize = biffRecordRaw.GetStoreSize(Version);
			m_dataProvider.WriteInt16(iOffset, (short)storeSize);
			iOffset += 2;
			biffRecordRaw.InfillInternalData(m_dataProvider, iOffset, Version);
			iOffset += storeSize;
		}
	}

	private void ShrinkDataStorage()
	{
		if ((CellPositionSize == 4) | (m_iUsedSize == 0))
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		int num3 = 65535;
		int num4 = 255;
		if (m_iFirstColumn > num4)
		{
			m_iFirstColumn = 0;
			m_iLastColumn = 0;
			m_iUsedSize = num2;
			return;
		}
		while (num < m_iUsedSize)
		{
			TBIFFRecord tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(num);
			num += 2;
			int num5 = m_dataProvider.ReadInt16(num);
			num += 2;
			if (tBIFFRecord != TBIFFRecord.Array && tBIFFRecord != TBIFFRecord.String)
			{
				int num6 = m_dataProvider.ReadInt32(num);
				int num7 = m_dataProvider.ReadInt32(num + 4);
				if (num7 > num4 || num6 > num3)
				{
					break;
				}
				m_iLastColumn = num7;
				m_dataProvider.WriteInt16(num2, (short)tBIFFRecord);
				num2 += 2;
				if (tBIFFRecord != TBIFFRecord.Formula)
				{
					bool num8 = tBIFFRecord == TBIFFRecord.MulRK || tBIFFRecord == TBIFFRecord.MulBlank;
					int num9 = num5 - 4;
					if (num8)
					{
						num9 -= 2;
					}
					m_dataProvider.WriteInt16(num2, (short)num9);
					num2 += 2;
					m_dataProvider.WriteUInt16(num2, (ushort)num6);
					num2 += 2;
					m_dataProvider.WriteInt16(num2, (short)num7);
					num2 += 2;
					m_dataProvider.CopyTo(num + 8, m_dataProvider, num2, num9);
					num2 += num9 - 4;
				}
				else
				{
					FormulaRecord obj = (FormulaRecord)BiffRecordFactory.GetRecord(m_dataProvider, num - 4, OfficeVersion.Excel2007);
					int storeSize = obj.GetStoreSize(OfficeVersion.Excel97to2003);
					m_dataProvider.WriteInt16(num2, (short)storeSize);
					num2 += 2;
					FormulaRecord.ConvertFormulaTokens(obj.ParsedExpression, bFromExcel07To97: true);
					obj.InfillInternalData(m_dataProvider, num2, OfficeVersion.Excel97to2003);
					num2 += storeSize;
				}
			}
			else if (tBIFFRecord == TBIFFRecord.Array)
			{
				BiffRecordRaw record = BiffRecordFactory.GetRecord(m_dataProvider, num - 4, OfficeVersion.Excel2007);
				int storeSize2 = record.GetStoreSize(OfficeVersion.Excel97to2003);
				m_dataProvider.WriteInt16(num2, (short)tBIFFRecord);
				num2 += 2;
				m_dataProvider.WriteInt16(num2, (short)storeSize2);
				num2 += 2;
				record.InfillInternalData(m_dataProvider, num2, OfficeVersion.Excel97to2003);
				num2 += storeSize2;
			}
			else
			{
				m_dataProvider.CopyTo(num - 4, m_dataProvider, num2, num5 + 4);
				num2 += num5 + 4;
			}
			num += num5;
		}
		m_iUsedSize = num2;
	}

	private void ExtendDataStorage(int iBlockSize)
	{
		if (CellPositionSize == 8 || m_iUsedSize == 0)
		{
			return;
		}
		int num = 0;
		int iWriteOffset = 0;
		DataProvider dataProvider = ApplicationImpl.CreateDataProvider();
		List<FormulaRecord> list = new List<FormulaRecord>();
		int enlargedDataSize = GetEnlargedDataSize(list);
		dataProvider.EnsureCapacity((enlargedDataSize / iBlockSize + 1) * iBlockSize);
		int num2 = 0;
		while (num < m_iUsedSize)
		{
			TBIFFRecord tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(num);
			num += 2;
			int num3 = m_dataProvider.ReadInt16(num);
			num += 2;
			if (tBIFFRecord != TBIFFRecord.Array && tBIFFRecord != TBIFFRecord.String)
			{
				int iRow = m_dataProvider.ReadUInt16(num);
				int iColumn = m_dataProvider.ReadUInt16(num + 2);
				dataProvider.WriteInt16(iWriteOffset, (short)tBIFFRecord);
				iWriteOffset += 2;
				if (tBIFFRecord != TBIFFRecord.Formula)
				{
					EnlargeCellRecord(dataProvider, ref iWriteOffset, num, tBIFFRecord, num3, iRow, iColumn);
				}
				else
				{
					FormulaRecord formulaRecord = list[num2];
					FormulaRecord.ConvertFormulaTokens(formulaRecord.ParsedExpression, bFromExcel07To97: false);
					num2++;
					EnlargeFormulaRecord(dataProvider, ref iWriteOffset, formulaRecord);
				}
			}
			else if (tBIFFRecord == TBIFFRecord.Array)
			{
				BiffRecordRaw record = BiffRecordFactory.GetRecord(m_dataProvider, num - 4, OfficeVersion.Excel97to2003);
				int storeSize = record.GetStoreSize(OfficeVersion.Excel2007);
				dataProvider.WriteInt16(iWriteOffset, (short)tBIFFRecord);
				iWriteOffset += 2;
				dataProvider.WriteInt16(iWriteOffset, (short)storeSize);
				iWriteOffset += 2;
				record.InfillInternalData(dataProvider, iWriteOffset, OfficeVersion.Excel2007);
				iWriteOffset += storeSize;
			}
			else
			{
				int num4 = num3 + 4;
				m_dataProvider.CopyTo(num - 4, dataProvider, iWriteOffset, num4);
				iWriteOffset += num4;
			}
			num += num3;
		}
		if (enlargedDataSize != iWriteOffset)
		{
			throw new InvalidOperationException("Wrong offset");
		}
		m_iUsedSize = iWriteOffset;
		m_iCurrentColumn = -1;
		m_iCurrentOffset = -1;
		m_dataProvider.Dispose();
		m_dataProvider = dataProvider;
	}

	private void EnlargeCellRecord(DataProvider result, ref int iWriteOffset, int iReadOffset, TBIFFRecord code, int iLength, int iRow, int iColumn)
	{
		bool num = code == TBIFFRecord.MulRK || code == TBIFFRecord.MulBlank;
		int num2 = iLength + 4;
		if (num)
		{
			num2 += 2;
		}
		result.EnsureCapacity(iWriteOffset + num2 + 2);
		result.WriteInt16(iWriteOffset, (short)num2);
		iWriteOffset += 2;
		result.WriteInt32(iWriteOffset, iRow);
		iWriteOffset += 4;
		result.WriteInt32(iWriteOffset, iColumn);
		iWriteOffset += 4;
		m_dataProvider.CopyTo(iReadOffset + 4, result, iWriteOffset, iLength - 4);
		iWriteOffset += iLength - 4;
		if (num)
		{
			int value = m_dataProvider.ReadInt16(iReadOffset + iLength - 2);
			result.WriteInt32(iWriteOffset - 2, value);
			iWriteOffset += 2;
		}
	}

	private void EnlargeFormulaRecord(DataProvider result, ref int iWriteOffset, FormulaRecord formula)
	{
		int storeSize = formula.GetStoreSize(OfficeVersion.Excel2007);
		result.WriteInt16(iWriteOffset, (short)storeSize);
		iWriteOffset += 2;
		result.EnsureCapacity(iWriteOffset + storeSize);
		formula.InfillInternalData(result, iWriteOffset, OfficeVersion.Excel2007);
		iWriteOffset += storeSize;
	}

	private int GetEnlargedDataSize(List<FormulaRecord> arrFormulas)
	{
		if (arrFormulas == null)
		{
			throw new ArgumentNullException("arrFormulas");
		}
		int i = 0;
		int num = 0;
		int num2;
		for (; i < m_iUsedSize; i += num2 + 4)
		{
			TBIFFRecord tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(i);
			num2 = m_dataProvider.ReadInt16(i + 2);
			switch (tBIFFRecord)
			{
			case TBIFFRecord.String:
				num += num2 + 4;
				break;
			case TBIFFRecord.Array:
			{
				ArrayRecord arrayRecord = (ArrayRecord)BiffRecordFactory.GetRecord(m_dataProvider, i, Version);
				num += arrayRecord.GetStoreSize(OfficeVersion.Excel2007) + 4;
				break;
			}
			case TBIFFRecord.Formula:
			{
				FormulaRecord formulaRecord = (FormulaRecord)BiffRecordFactory.GetRecord(m_dataProvider, i, Version);
				arrFormulas.Add(formulaRecord);
				num += formulaRecord.GetStoreSize(OfficeVersion.Excel2007) + 4;
				break;
			}
			case TBIFFRecord.MulRK:
			case TBIFFRecord.MulBlank:
				num += num2 + 4 + 6;
				break;
			default:
				num += num2 + 4 + 4;
				break;
			}
		}
		return num;
	}

	private int GetRecordCount()
	{
		int num = 0;
		int num2 = 0;
		while (num < m_iUsedSize)
		{
			int num3 = m_dataProvider.ReadInt16(num + 2);
			num += num3 + 4;
			num2++;
		}
		return num2;
	}

	private void UpdateRecordsAfterCopy(SSTDictionary sourceSST, SSTDictionary destSST, Dictionary<int, int> hashExtFormatIndexes, Dictionary<string, string> hashWorksheetNames, Dictionary<int, int> dicNameIndexes, Dictionary<int, int> dicFontIndexes, Dictionary<int, int> dictExternSheets)
	{
		if (sourceSST == null)
		{
			throw new ArgumentNullException("sourceSST");
		}
		if (destSST == null)
		{
			throw new ArgumentNullException("destSST");
		}
		int i = 0;
		int extendedFormatIndex = ExtendedFormatIndex;
		bool flag = sourceSST == destSST;
		WorkbookImpl workbook = destSST.Workbook;
		if (hashExtFormatIndexes.ContainsKey(extendedFormatIndex))
		{
			ExtendedFormatIndex = (IsFormatted ? ((ushort)hashExtFormatIndexes[extendedFormatIndex]) : ((ushort)workbook.DefaultXFIndex));
		}
		int num;
		for (; i < m_iUsedSize; i += num + 4)
		{
			TBIFFRecord tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(i);
			num = m_dataProvider.ReadInt16(i + 2);
			if (tBIFFRecord == TBIFFRecord.String || tBIFFRecord == TBIFFRecord.Array)
			{
				continue;
			}
			if (!flag)
			{
				if (hashExtFormatIndexes != null && hashExtFormatIndexes.Count > 0)
				{
					switch (tBIFFRecord)
					{
					case TBIFFRecord.MulRK:
						UpdateMulReference(hashExtFormatIndexes, i, num, bIsMulRK: true);
						break;
					case TBIFFRecord.MulBlank:
						UpdateMulReference(hashExtFormatIndexes, i, num, bIsMulRK: false);
						break;
					default:
					{
						int xFIndex = GetXFIndex(i, bMulti: false);
						if (hashExtFormatIndexes.ContainsKey(xFIndex))
						{
							xFIndex = hashExtFormatIndexes[xFIndex];
							SetXFIndex(i, (ushort)xFIndex);
						}
						break;
					}
					}
				}
				switch (tBIFFRecord)
				{
				case TBIFFRecord.LabelSST:
					UpdateLabelSST(sourceSST, destSST, i, bIsLocal: false, dicFontIndexes);
					break;
				case TBIFFRecord.Formula:
					UpdateFormulaRefs(sourceSST, destSST, i, hashWorksheetNames, dicNameIndexes, num, dictExternSheets);
					break;
				}
			}
			else
			{
				switch (tBIFFRecord)
				{
				case TBIFFRecord.Formula:
					UpdateFormulaRefs(sourceSST, destSST, i, hashWorksheetNames, dicNameIndexes, num, dictExternSheets);
					break;
				case TBIFFRecord.LabelSST:
					UpdateLabelSST(sourceSST, destSST, i, bIsLocal: true, dicFontIndexes);
					break;
				}
			}
		}
	}

	private void UpdateMulReference(Dictionary<int, int> hashXFIndexes, int iOffset, int iLength, bool bIsMulRK)
	{
		iLength = iOffset + 4 + iLength - 2;
		iOffset += 8;
		int num = (bIsMulRK ? 6 : 2);
		for (int i = iOffset; i < iLength; i += num)
		{
			int value = m_dataProvider.ReadInt16(i);
			if (hashXFIndexes.TryGetValue(value, out value))
			{
				m_dataProvider.WriteInt16(i, (short)value);
			}
		}
	}

	private void UpdateLabelSST(SSTDictionary sourceSST, SSTDictionary destSST, int iOffset, bool bIsLocal, Dictionary<int, int> dicFontIndexes)
	{
		if (sourceSST == null)
		{
			throw new ArgumentNullException("sourceSST");
		}
		if (destSST == null)
		{
			throw new ArgumentNullException("destSST");
		}
		int sSTIndex = LabelSSTRecord.GetSSTIndex(m_dataProvider, iOffset, Version);
		if (bIsLocal)
		{
			destSST.AddIncrease(sSTIndex);
			return;
		}
		TextWithFormat textWithFormat = sourceSST[sSTIndex];
		object key = ((textWithFormat.FormattingRunsCount == 0) ? textWithFormat.Text : textWithFormat.Clone(dicFontIndexes));
		sSTIndex = destSST.AddIncrease(key);
		LabelSSTRecord.SetSSTIndex(m_dataProvider, iOffset, sSTIndex, Version);
	}

	private void UpdateFormulaRefs(SSTDictionary sourceSST, SSTDictionary destSST, int iOffset, Dictionary<string, string> hashWorksheetNames, Dictionary<int, int> dicNameIndexes, int iLength, Dictionary<int, int> dictExternSheets)
	{
		FormulaRecord formulaRecord = (FormulaRecord)BiffRecordFactory.GetRecord(m_dataProvider, iOffset, Version);
		formulaRecord.ParseStructure(m_dataProvider, iOffset + 4, iLength, Version);
		WorkbookImpl workbook = destSST.Workbook;
		if (UpdateNameSheetReferences(formulaRecord, hashWorksheetNames, sourceSST.Workbook, workbook, dicNameIndexes, dictExternSheets))
		{
			formulaRecord.IsFillFromExpression = true;
			int storeSize = formulaRecord.GetStoreSize(Version);
			InsertRecordData(iOffset, iLength + 4, storeSize + 4, formulaRecord, workbook.Application.RowStorageAllocationBlockSize);
		}
	}

	private bool UpdateSheetReferences(FormulaRecord formula, IDictionary dicSheetNames, WorkbookImpl sourceBook, WorkbookImpl destBook)
	{
		if (formula == null)
		{
			throw new ArgumentNullException("formula");
		}
		if (sourceBook == null)
		{
			throw new ArgumentNullException("book");
		}
		Ptg[] parsedExpression = formula.ParsedExpression;
		formula.GetStoreSize(Version);
		bool result = false;
		int i = 0;
		for (int num = parsedExpression.Length; i < num; i++)
		{
			Ptg ptg = parsedExpression[i];
			if (ptg is ISheetReference)
			{
				ISheetReference obj = (ISheetReference)ptg;
				ushort refIndex = obj.RefIndex;
				string text = sourceBook.GetSheetNameByReference(refIndex);
				if (dicSheetNames != null && dicSheetNames.Contains(text))
				{
					text = (string)dicSheetNames[text];
				}
				int num2 = destBook.AddSheetReference(text);
				obj.RefIndex = (ushort)num2;
				result = true;
			}
		}
		return result;
	}

	private bool UpdateNameReferences(FormulaRecord formula, Dictionary<int, int> dicNameIndexes)
	{
		if (formula == null)
		{
			throw new ArgumentNullException("formula");
		}
		if (dicNameIndexes == null)
		{
			throw new ArgumentNullException("dicNameIndexes");
		}
		Ptg[] parsedExpression = formula.ParsedExpression;
		formula.GetStoreSize(Version);
		bool result = false;
		int i = 0;
		for (int num = parsedExpression.Length; i < num; i++)
		{
			if (parsedExpression[i] is NamePtg namePtg)
			{
				int num2 = namePtg.ExternNameIndex - 1;
				int num3 = (dicNameIndexes.ContainsKey(num2) ? dicNameIndexes[num2] : num2);
				namePtg.ExternNameIndex = (ushort)(num3 + 1);
				result = true;
			}
		}
		return result;
	}

	private bool UpdateNameSheetReferences(FormulaRecord formula, Dictionary<string, string> dicSheetNames, WorkbookImpl sourceBook, WorkbookImpl destBook, Dictionary<int, int> dicNameIndexes, Dictionary<int, int> dictExternSheets)
	{
		if (formula == null)
		{
			throw new ArgumentNullException("formula");
		}
		if (sourceBook == null)
		{
			throw new ArgumentNullException("book");
		}
		if (dicNameIndexes == null)
		{
			throw new ArgumentNullException("dicNameIndexes");
		}
		Ptg[] parsedExpression = formula.ParsedExpression;
		formula.GetStoreSize(Version);
		bool result = false;
		int i = 0;
		for (int num = parsedExpression.Length; i < num; i++)
		{
			Ptg ptg = parsedExpression[i];
			if (ptg is ISheetReference)
			{
				ISheetReference sheetReference = (ISheetReference)ptg;
				ushort refIndex = sheetReference.RefIndex;
				if (!sourceBook.IsExternalReference(refIndex))
				{
					string text = sourceBook.GetSheetNameByReference(refIndex);
					if (dicSheetNames != null && dicSheetNames.ContainsKey(text))
					{
						text = dicSheetNames[text];
					}
					int num2 = destBook.AddSheetReference(text);
					sheetReference.RefIndex = (ushort)num2;
					result = true;
				}
				else if (ptg is NameXPtg nameXPtg)
				{
					int refIndex2 = nameXPtg.RefIndex;
					int num3 = (dictExternSheets.ContainsKey(refIndex2) ? dictExternSheets[refIndex2] : refIndex2);
					nameXPtg.RefIndex = (ushort)num3;
					result = true;
				}
			}
			else if (ptg is NamePtg namePtg)
			{
				int num4 = namePtg.ExternNameIndex - 1;
				int num5 = (dicNameIndexes.ContainsKey(num4) ? dicNameIndexes[num4] : num4);
				namePtg.ExternNameIndex = (ushort)(num5 + 1);
				result = true;
			}
		}
		return result;
	}

	private bool IsSameSubType(ICellPositionFormat cell, int iOffset)
	{
		if (cell == null)
		{
			throw new ArgumentNullException("cell");
		}
		TBIFFRecord tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(iOffset);
		int i = 0;
		for (int num = DEF_MULTIRECORDS_SUBTYPES.Length; i < num; i += 2)
		{
			if (DEF_MULTIRECORDS_SUBTYPES[i] == tBIFFRecord)
			{
				return DEF_MULTIRECORDS_SUBTYPES[i + 1] == cell.TypeCode;
			}
		}
		return false;
	}

	private int SplitRecord(int iOffset, int iColumnIndex, int iBlockSize)
	{
		BiffRecordRaw record = BiffRecordFactory.GetRecord(m_dataProvider, iOffset, Version);
		IMultiCellRecord obj = (IMultiCellRecord)record;
		int num = record.GetStoreSize(Version) + 4;
		ICellPositionFormat[] array = obj.Split(iColumnIndex);
		int num2 = array.Length;
		int num3 = 0;
		for (int i = 0; i < num2; i++)
		{
			_ = (BiffRecordRaw)array[i];
			if (record != null)
			{
				num3 += record.GetStoreSize(Version) + 4;
			}
		}
		int num4 = num3 - num;
		EnsureSize(m_iUsedSize + num4, iBlockSize);
		InsertRecordData(iOffset, num, 0, null, iBlockSize);
		for (int j = 0; j < num2; j++)
		{
			BiffRecordRaw biffRecordRaw = (BiffRecordRaw)array[j];
			if (biffRecordRaw != null)
			{
				int num5 = biffRecordRaw.GetStoreSize(Version) + 4;
				InsertRecordData(iOffset, 0, num5, biffRecordRaw, iBlockSize);
				if (j == 0)
				{
					iOffset += num5;
				}
			}
		}
		return iOffset;
	}

	private void InsertIntoRecord(int iOffset, ICellPositionFormat cell)
	{
		((IMultiCellRecord)BiffRecordFactory.GetRecord(m_dataProvider, iOffset, Version)).Insert(cell);
	}

	private void InsertRecordData(int iOffset, int iPreparedSize, int iRequiredSize, BiffRecordRaw record, int iBlockSize)
	{
		int num = iRequiredSize / iBlockSize * iBlockSize + iBlockSize;
		if (m_dataProvider.Capacity - UsedSize < iRequiredSize)
		{
			EnsureSize(num + m_iUsedSize, iBlockSize);
		}
		if (iPreparedSize != iRequiredSize)
		{
			int num2 = m_iUsedSize - iOffset - iPreparedSize;
			if (num2 > 0)
			{
				m_dataProvider.MoveMemory(iOffset + iRequiredSize, iOffset + iPreparedSize, num2);
			}
			m_iUsedSize += iRequiredSize - iPreparedSize;
		}
		if (record != null)
		{
			m_dataProvider.WriteInt16(iOffset, (short)record.TypeCode);
			OfficeVersion version = Version;
			m_dataProvider.WriteInt16(iOffset + 2, (short)record.GetStoreSize(version));
			record.InfillInternalData(m_dataProvider, iOffset + 4, version);
		}
	}

	private void InsertRecordData(int iOffset, int iPreparedSize, IList arrRecords, int iBlockSize)
	{
		if (arrRecords == null)
		{
			throw new ArgumentNullException("arrRecords");
		}
		int num = 0;
		int count = arrRecords.Count;
		int val = 0;
		for (int i = 0; i < count; i++)
		{
			int storeSize = ((BiffRecordRaw)arrRecords[i]).GetStoreSize(Version);
			num += storeSize + 4;
			val = Math.Max(storeSize, val);
		}
		int num2 = num - iPreparedSize;
		if (num2 > 0)
		{
			EnsureSize(num2 + m_iUsedSize, iBlockSize);
		}
		if (iPreparedSize != num)
		{
			int num3 = m_iUsedSize - iOffset - iPreparedSize;
			if (num3 > 0)
			{
				m_dataProvider.MoveMemory(iOffset + num, iOffset + iPreparedSize, num3);
			}
			m_iUsedSize += num - iPreparedSize;
		}
		if (count > 0)
		{
			for (int j = 0; j < count; j++)
			{
				BiffRecordRaw biffRecordRaw = (BiffRecordRaw)arrRecords[j];
				m_dataProvider.WriteInt16(iOffset, (short)biffRecordRaw.TypeCode);
				m_dataProvider.WriteInt16(iOffset + 2, (short)biffRecordRaw.GetStoreSize(Version));
				biffRecordRaw.InfillInternalData(m_dataProvider, iOffset, Version);
				iOffset += biffRecordRaw.Length + 4;
			}
		}
	}

	private int LocateRecord(int iColumnIndex, out bool bFound)
	{
		bool bMul;
		return LocateRecord(iColumnIndex, out bFound, out bMul, bGetRkOffset: false);
	}

	private int LocateRecord(int iColumnIndex, out bool bFound, out bool bMul, bool bGetRkOffset)
	{
		bFound = false;
		bMul = false;
		if (iColumnIndex < m_iFirstColumn || m_iUsedSize <= 0)
		{
			return 0;
		}
		int iOffset;
		int num;
		if (iColumnIndex > m_iLastColumn)
		{
			iOffset = m_iUsedSize;
			num = int.MaxValue;
		}
		else
		{
			iOffset = 0;
			bool num2 = iColumnIndex >= m_iCurrentColumn && m_iCurrentColumn >= 0;
			int num3 = -4;
			bool hasMultiRkBlank = HasMultiRkBlank;
			if (num2)
			{
				iOffset = m_iCurrentOffset;
				num = m_iCurrentColumn;
			}
			else
			{
				num = m_iFirstColumn;
			}
			do
			{
				iOffset += num3 + 4;
				if (iOffset >= m_iUsedSize)
				{
					num = int.MaxValue;
					break;
				}
				long num4 = m_dataProvider.ReadInt64(iOffset);
				TBIFFRecord tBIFFRecord = (TBIFFRecord)(num4 & 0xFFFF);
				num4 >>= 16;
				num3 = (int)(num4 & 0xFFFF);
				if (tBIFFRecord != TBIFFRecord.String && tBIFFRecord != TBIFFRecord.Array)
				{
					num4 >>= 32;
					num = (int)((CellPositionSize != 4) ? m_dataProvider.ReadInt32(iOffset + 4 + 4) : (num4 & 0xFFFF));
					if (hasMultiRkBlank && (tBIFFRecord == TBIFFRecord.MulRK || tBIFFRecord == TBIFFRecord.MulBlank) && GetOffsetToSubRecord(ref iOffset, num3, num, iColumnIndex, ref bMul, tBIFFRecord, bGetRkOffset))
					{
						break;
					}
				}
			}
			while (num < iColumnIndex);
		}
		bFound = num <= iColumnIndex;
		if (bFound && !bMul)
		{
			m_iCurrentColumn = num;
			m_iCurrentOffset = iOffset;
		}
		return iOffset;
	}

	private bool GetOffsetToSubRecord(ref int iOffset, int iLength, int iCurrentColumn, int iColumnIndex, ref bool bMul, TBIFFRecord biffCode, bool bGetRkOffset)
	{
		int cellPositionSize = CellPositionSize;
		int iOffset2 = iOffset + 4 + iLength - cellPositionSize / 2;
		int num = ((cellPositionSize == 4) ? m_dataProvider.ReadInt16(iOffset2) : m_dataProvider.ReadInt32(iOffset2));
		bool result = false;
		if (iCurrentColumn <= iColumnIndex && num >= iColumnIndex)
		{
			bMul = true;
			int num2 = ((biffCode == TBIFFRecord.MulRK) ? 6 : 2);
			if (bGetRkOffset)
			{
				m_iCurrentColumn = iCurrentColumn;
				m_iCurrentOffset = iOffset;
				int num3 = iColumnIndex - iCurrentColumn;
				iOffset = iOffset + 4 + cellPositionSize + num3 * num2;
			}
			result = true;
		}
		return result;
	}

	private void EnsureSize(int iRequiredSize, int iBlockSize)
	{
		if (m_dataProvider == null)
		{
			throw new NotImplementedException();
		}
		int size = iRequiredSize / iBlockSize * iBlockSize + iBlockSize;
		if (m_book != null)
		{
			m_dataProvider.EnsureCapacity(size, m_book.MaxImportColumns);
		}
		else
		{
			m_dataProvider.EnsureCapacity(size);
		}
	}

	private void AccessColumn(int iColumnIndex)
	{
		if (iColumnIndex < 0)
		{
			throw new ArgumentOutOfRangeException("iColumnIndex", "Column index cannot be less than 0");
		}
		m_iFirstColumn = ((m_iFirstColumn >= 0) ? Math.Min(m_iFirstColumn, iColumnIndex) : iColumnIndex);
		m_iLastColumn = Math.Max(m_iLastColumn, iColumnIndex);
	}

	private void AccessColumn(int iColumnIndex, ICellPositionFormat cell)
	{
		if (iColumnIndex < 0)
		{
			throw new ArgumentOutOfRangeException("iColumnIndex", "Column index cannot be less than 0");
		}
		if (cell != null)
		{
			m_iFirstColumn = ((m_iFirstColumn >= 0) ? Math.Min(m_iFirstColumn, iColumnIndex) : iColumnIndex);
			m_iLastColumn = Math.Max(m_iLastColumn, iColumnIndex);
		}
		else if (iColumnIndex == m_iFirstColumn)
		{
			if (iColumnIndex == m_iLastColumn)
			{
				m_iLastColumn = (m_iFirstColumn = -1);
				m_iUsedSize = 0;
			}
			else
			{
				m_iFirstColumn = GetColumn(0);
			}
		}
		else
		{
			if (iColumnIndex != m_iLastColumn)
			{
				return;
			}
			int num = -1;
			int num2;
			for (int i = 0; i < m_iUsedSize; i += 4 + num2)
			{
				TBIFFRecord tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(i);
				num2 = m_dataProvider.ReadInt16(i + 2);
				switch (tBIFFRecord)
				{
				case TBIFFRecord.MulRK:
				case TBIFFRecord.MulBlank:
					num = GetLastColumn(i, num2);
					break;
				default:
					num = GetColumn(i);
					break;
				case TBIFFRecord.String:
				case TBIFFRecord.Array:
					break;
				}
			}
			if (num >= 0)
			{
				m_iLastColumn = num;
				return;
			}
			m_iFirstColumn = -1;
			m_iLastColumn = -1;
			m_iUsedSize = 0;
		}
	}

	private int RemoveFormulaStringValue(int iColumnIndex)
	{
		int iFormulaRecordOffset;
		return RemoveFormulaStringValue(iColumnIndex, out iFormulaRecordOffset);
	}

	private int RemoveFormulaStringValue(int iColumnIndex, out int iFormulaRecordOffset)
	{
		iFormulaRecordOffset = -1;
		if (iColumnIndex < m_iFirstColumn || iColumnIndex > m_iLastColumn)
		{
			return -1;
		}
		int num = LocateRecord(iColumnIndex, out var bFound);
		if (!bFound)
		{
			return -1;
		}
		TBIFFRecord tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(num);
		if (tBIFFRecord != TBIFFRecord.Formula)
		{
			return -1;
		}
		iFormulaRecordOffset = num;
		num = MoveNext(num);
		if (num >= m_iUsedSize)
		{
			return num;
		}
		tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(num);
		if (tBIFFRecord == TBIFFRecord.Array)
		{
			num = MoveNext(num);
			if (num >= m_iUsedSize)
			{
				return num;
			}
			tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(num);
		}
		if (tBIFFRecord == TBIFFRecord.String)
		{
			RemoveRecord(num);
		}
		return num;
	}

	private void RemoveRecord(int iOffset)
	{
		if (iOffset < m_iCurrentOffset)
		{
			m_iCurrentOffset = -1;
			m_iCurrentColumn = -1;
		}
		int num = m_dataProvider.ReadInt16(iOffset + 2) + 4;
		int num2 = m_iUsedSize - iOffset - num;
		if (num2 > 0)
		{
			m_dataProvider.MoveMemory(iOffset, iOffset + num, num2);
		}
		m_iUsedSize -= num;
	}

	private int MoveNext(int iOffset)
	{
		int num = m_dataProvider.ReadInt16(iOffset + 2);
		return iOffset + 4 + num;
	}

	private Point GetOffsets(int iStartColumn, int iEndColumn, out int iRealStartColumn, out int iRealEndColumn)
	{
		iRealStartColumn = -1;
		iRealEndColumn = -1;
		if (m_iFirstColumn < 0)
		{
			return Point.Empty;
		}
		iStartColumn = Math.Max(m_iFirstColumn, iStartColumn);
		iEndColumn = Math.Min(m_iLastColumn, iEndColumn);
		if (iStartColumn > iEndColumn)
		{
			return Point.Empty;
		}
		if (iStartColumn == m_iFirstColumn && iEndColumn == m_iLastColumn)
		{
			iRealStartColumn = iStartColumn;
			iRealEndColumn = iEndColumn;
			return new Point(0, m_iUsedSize);
		}
		bool bFound;
		int num = LocateRecord(iStartColumn, out bFound);
		if (num >= m_iUsedSize)
		{
			return Point.Empty;
		}
		iStartColumn = GetColumn(num);
		iRealStartColumn = iStartColumn;
		iRealEndColumn = iStartColumn;
		int num2 = num;
		if (iRealEndColumn == iEndColumn)
		{
			num2 = MoveAfterRecord(num2);
		}
		else
		{
			while (iRealEndColumn < iEndColumn && num2 < m_iUsedSize)
			{
				TBIFFRecord tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(num2);
				int num3 = m_dataProvider.ReadInt16(num2 + 2);
				int column = GetColumn(num2);
				if (column > iEndColumn)
				{
					break;
				}
				num2 += 4 + num3;
				bool flag = tBIFFRecord == TBIFFRecord.MulRK || tBIFFRecord == TBIFFRecord.MulBlank;
				iRealEndColumn = (flag ? m_dataProvider.ReadInt16(num2 - 2) : column);
				if (num2 >= m_iUsedSize)
				{
					continue;
				}
				tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(num2);
				if (tBIFFRecord == TBIFFRecord.Array)
				{
					num2 = MoveNext(num2);
					if (num2 < m_iUsedSize)
					{
						tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(num2);
					}
				}
				if (tBIFFRecord == TBIFFRecord.String)
				{
					num2 = MoveNext(num2);
				}
			}
		}
		return new Point(num, num2);
	}

	private int MoveAfterRecord(int iOffset)
	{
		if (iOffset < m_iUsedSize)
		{
			iOffset = MoveNext(iOffset);
			if (iOffset < m_iUsedSize)
			{
				TBIFFRecord tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(iOffset);
				if (tBIFFRecord == TBIFFRecord.Array)
				{
					iOffset = MoveNext(iOffset);
					if (iOffset < m_iUsedSize)
					{
						tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(iOffset);
					}
				}
				if (tBIFFRecord == TBIFFRecord.String)
				{
					iOffset = MoveNext(iOffset);
				}
			}
		}
		return iOffset;
	}

	private IMultiCellRecord CreateMultiRecord(TBIFFRecord subCode)
	{
		return (IMultiCellRecord)BiffRecordFactory.GetRecord((subCode == TBIFFRecord.RK) ? TBIFFRecord.MulRK : TBIFFRecord.MulBlank);
	}

	private ICellPositionFormat GetNextColumnRecord(int iColumnIndex, ICellPositionFormat prevRecord, ref int iOffset, bool bMulti)
	{
		int num = iColumnIndex;
		ICellPositionFormat result = null;
		if (bMulti)
		{
			num = GetLastColumnFromMultiRecord(iOffset);
		}
		if (num >= iColumnIndex + 1)
		{
			result = prevRecord;
		}
		else
		{
			if (prevRecord != null)
			{
				iOffset += 4 + m_dataProvider.ReadInt16(iOffset + 2);
			}
			if (iOffset < m_iUsedSize && GetColumn(iOffset) == iColumnIndex + 1)
			{
				result = (ICellPositionFormat)GetRecordAtOffset(iOffset);
			}
		}
		return result;
	}

	private int GetLastColumnFromMultiRecord(int iOffset)
	{
		int num = m_dataProvider.ReadInt16(iOffset + 2);
		return m_dataProvider.ReadInt16(iOffset + 4 + num - 2);
	}

	private void SetOrdinaryRecord(int iColumnIndex, ICellPositionFormat cell, int iBlockSize)
	{
		if (cell != null && !HasRkBlank)
		{
			TBIFFRecord typeCode = cell.TypeCode;
			if (typeCode == TBIFFRecord.RK || typeCode == TBIFFRecord.Blank)
			{
				HasRkBlank = true;
			}
		}
		bool bFound;
		int num = LocateRecord(iColumnIndex, out bFound);
		int num2 = 0;
		if (bFound)
		{
			num2 = m_dataProvider.ReadInt16(num + 2) + 4;
			int num3 = num + num2;
			if (num3 < m_iUsedSize)
			{
				TBIFFRecord tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(num3);
				if (tBIFFRecord == TBIFFRecord.Array)
				{
					int num4 = m_dataProvider.ReadInt16(num3 + 2) + 4;
					num3 += num4;
					num2 += num4;
					if (num3 < m_iUsedSize)
					{
						tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(num3);
					}
				}
				if (tBIFFRecord == TBIFFRecord.String && (cell == null || cell.TypeCode != TBIFFRecord.Formula))
				{
					int num5 = m_dataProvider.ReadInt16(num3 + 2);
					num2 += num5 + 4;
				}
			}
		}
		else
		{
			m_iCurrentOffset = -1;
			m_iCurrentColumn = -1;
		}
		BiffRecordRaw biffRecordRaw = (BiffRecordRaw)cell;
		int iRequiredSize = ((biffRecordRaw != null) ? (biffRecordRaw.GetStoreSize(Version) + 4) : 0);
		InsertRecordData(num, num2, iRequiredSize, biffRecordRaw, iBlockSize);
		AccessColumn(iColumnIndex, cell);
	}

	private int DefragmentDataStorage(DefragmentHelper rkRecordHelper, DefragmentHelper blankRecordHelper, DefragmentHelper ordinaryHelper, object userData)
	{
		int num = 0;
		while (num < m_iUsedSize)
		{
			TBIFFRecord tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(num);
			m_dataProvider.ReadInt16(num + 2);
			num = tBIFFRecord switch
			{
				TBIFFRecord.RK => rkRecordHelper(userData), 
				TBIFFRecord.Blank => blankRecordHelper(userData), 
				_ => ordinaryHelper(userData), 
			};
		}
		return num;
	}

	private int SkipRecord(object userData)
	{
		OffsetData obj = (OffsetData)userData;
		int startOffset = obj.StartOffset;
		int num = m_dataProvider.ReadInt16(startOffset + 2) + 4;
		obj.UsedSize += num;
		return obj.StartOffset = startOffset + num;
	}

	private int SkipRKRecords(object userData)
	{
		OffsetData offsetData = (OffsetData)userData;
		int startOffset = offsetData.StartOffset;
		int column = GetColumn(startOffset);
		int num = column;
		int num2 = 4 + m_dataProvider.ReadInt16(startOffset + 2);
		startOffset += num2;
		int usedSize = offsetData.UsedSize;
		while (startOffset < m_iUsedSize && m_dataProvider.ReadInt16(startOffset) == 638)
		{
			int column2 = GetColumn(startOffset);
			if (num + 1 != column2)
			{
				break;
			}
			startOffset += 4 + m_dataProvider.ReadInt16(startOffset + 2);
			num = column2;
		}
		int num3 = num - column + 1;
		usedSize = ((num3 <= 1) ? (usedSize + num2) : (usedSize + (6 * num3 + 6 + 4)));
		offsetData.UsedSize = usedSize;
		offsetData.StartOffset = startOffset;
		return startOffset;
	}

	private int SkipBlankRecords(object userData)
	{
		OffsetData offsetData = (OffsetData)userData;
		int startOffset = offsetData.StartOffset;
		int column = GetColumn(startOffset);
		int num = column;
		int num2 = 4 + m_dataProvider.ReadInt16(startOffset + 2);
		startOffset += num2;
		int usedSize = offsetData.UsedSize;
		for (; startOffset < m_iUsedSize && m_dataProvider.ReadInt16(startOffset) == 513; startOffset += 4 + m_dataProvider.ReadInt16(startOffset + 2))
		{
			int column2 = GetColumn(startOffset);
			if (num + 1 != column2)
			{
				break;
			}
			num = column2;
		}
		int num3 = num - column + 1;
		usedSize = ((num3 <= 1) ? (usedSize + num2) : (usedSize + (2 * num3 + 6 + 4)));
		offsetData.UsedSize = usedSize;
		offsetData.StartOffset = startOffset;
		return startOffset;
	}

	private int CompressRecord(object userData)
	{
		WriteData writeData = (WriteData)userData;
		int offset = writeData.Offset;
		int num = m_dataProvider.ReadInt16(offset + 2) + 4;
		m_dataProvider.MoveMemory(writeData.UsedSize, offset, num);
		writeData.UsedSize += num;
		return writeData.Offset = offset + num;
	}

	private int CompressRKRecords(object userData)
	{
		WriteData writeData = (WriteData)userData;
		int offset = writeData.Offset;
		_ = writeData.UsedSize;
		int column = GetColumn(offset);
		int num = column;
		int num2 = 4 + m_dataProvider.ReadInt16(offset + 2);
		offset += num2;
		while (offset < m_iUsedSize && m_dataProvider.ReadInt16(offset) == 638)
		{
			int column2 = GetColumn(offset);
			if (num + 1 != column2)
			{
				break;
			}
			offset += 4 + m_dataProvider.ReadInt16(offset + 2);
			num = column2;
		}
		int num3 = num - column + 1;
		if (num3 > 1)
		{
			CreateMulRKRecord(writeData, num3);
			HasMultiRkBlank = true;
		}
		else
		{
			offset = CompressRecord(userData);
		}
		return offset;
	}

	private int CompressBlankRecords(object userData)
	{
		WriteData writeData = (WriteData)userData;
		int offset = writeData.Offset;
		int usedSize = writeData.UsedSize;
		int column = GetColumn(offset);
		int num = column;
		int num2 = 4 + m_dataProvider.ReadInt16(offset + 2);
		offset += num2;
		while (offset < m_iUsedSize && m_dataProvider.ReadInt16(offset) == 513)
		{
			int column2 = GetColumn(offset);
			if (num + 1 != column2)
			{
				break;
			}
			offset += 4 + m_dataProvider.ReadInt16(offset + 2);
			num = column2;
		}
		int num3 = num - column + 1;
		if (num3 > 1)
		{
			int num4 = 2 * num3 + 6;
			BiffRecordRaw biffRecordRaw = CreateMulBlankRecord(writeData.Offset, num3);
			m_dataProvider.WriteInt16(usedSize, (short)biffRecordRaw.RecordCode);
			usedSize += 2;
			m_dataProvider.WriteInt16(usedSize, (short)num4);
			usedSize += 2;
			biffRecordRaw.InfillInternalData(m_dataProvider, usedSize, Version);
			usedSize += num4;
			num4 += 4;
			writeData.UsedSize += num4;
			writeData.Offset = offset;
			HasMultiRkBlank = true;
		}
		else
		{
			offset = CompressRecord(userData);
		}
		return offset;
	}

	private MulRKRecord CreateMulRKRecord(WriteData writeData, int iRecordsCount)
	{
		if (Version != 0)
		{
			throw new NotSupportedException("This method is supported only for Excel97-2003 file format");
		}
		int offset = writeData.Offset;
		int usedSize = writeData.UsedSize;
		if (offset < m_iCurrentOffset)
		{
			m_iCurrentOffset = -1;
			m_iCurrentColumn = -1;
		}
		m_dataProvider.WriteInt16(usedSize, 189);
		offset += 2;
		usedSize += 2;
		int num = 6 + 6 * iRecordsCount;
		m_dataProvider.WriteInt16(usedSize, (short)num);
		offset += 2;
		usedSize += 2;
		byte[] array = new byte[6];
		m_dataProvider.ReadArray(offset, array, 4);
		short value = (short)(m_dataProvider.ReadInt16(offset + 2) + iRecordsCount - 1);
		m_dataProvider.WriteBytes(usedSize, array, 0, 4);
		usedSize += 4;
		offset = writeData.Offset + 4 + 4;
		for (int i = 0; i < iRecordsCount; i++)
		{
			m_dataProvider.ReadArray(offset, array);
			m_dataProvider.WriteBytes(usedSize, array, 0, 6);
			offset += 14;
			usedSize += 6;
		}
		m_dataProvider.WriteInt16(usedSize, value);
		usedSize += 2;
		writeData.UsedSize = usedSize;
		writeData.Offset = offset - 4 - 4;
		return null;
	}

	private MulRKRecord CreateMulRKRecord(int iOffset, int iRecordsCount)
	{
		if (iOffset < m_iCurrentOffset)
		{
			m_iCurrentOffset = -1;
			m_iCurrentColumn = -1;
		}
		MulRKRecord mulRKRecord = (MulRKRecord)BiffRecordFactory.GetRecord(TBIFFRecord.MulRK);
		mulRKRecord.Row = GetRow(iOffset);
		mulRKRecord.FirstColumn = GetColumn(iOffset);
		mulRKRecord.LastColumn = mulRKRecord.FirstColumn + iRecordsCount - 1;
		List<MulRKRecord.RkRec> list = new List<MulRKRecord.RkRec>(iRecordsCount);
		for (int i = 0; i < iRecordsCount; i++)
		{
			ushort xFIndex = GetXFIndex(iOffset, bMulti: false);
			int rk = m_dataProvider.ReadInt32(iOffset + 10);
			MulRKRecord.RkRec item = new MulRKRecord.RkRec(xFIndex, rk);
			list.Add(item);
			iOffset += 14;
		}
		mulRKRecord.Records = list;
		return mulRKRecord;
	}

	private MulBlankRecord CreateMulBlankRecord(int iOffset, int iRecordsCount)
	{
		if (iOffset < m_iCurrentOffset)
		{
			m_iCurrentOffset = -1;
			m_iCurrentColumn = -1;
		}
		MulBlankRecord mulBlankRecord = (MulBlankRecord)BiffRecordFactory.GetRecord(TBIFFRecord.MulBlank);
		mulBlankRecord.Row = GetRow(iOffset);
		mulBlankRecord.FirstColumn = GetColumn(iOffset);
		mulBlankRecord.LastColumn = mulBlankRecord.FirstColumn + iRecordsCount - 1;
		List<ushort> list = new List<ushort>(iRecordsCount);
		for (int i = 0; i < iRecordsCount; i++)
		{
			ushort xFIndex = GetXFIndex(iOffset, bMulti: false);
			list.Add(xFIndex);
			iOffset += 10;
		}
		mulBlankRecord.ExtendedFormatIndexes = list;
		return mulBlankRecord;
	}

	private List<int> GetMultiRecordsOffsets(MulBlankRecord mulBlank, MulRKRecord mulRK, out int iSizeDelta)
	{
		iSizeDelta = 0;
		int i = 0;
		List<int> list = new List<int>();
		int iLength;
		for (; i < m_iUsedSize; i += iLength)
		{
			IMultiCellRecord multiCellRecord = CreateMultiCellRecord(i, mulBlank, mulRK, out iLength);
			if (multiCellRecord != null)
			{
				iSizeDelta += (multiCellRecord.LastColumn - multiCellRecord.FirstColumn + 1) * multiCellRecord.GetSeparateSubRecordSize(Version) - iLength;
				list.Add(i);
			}
		}
		return list;
	}

	private void DecompressStorage(List<int> arrOffsets, int iSizeDelta, MulBlankRecord mulBlank, MulRKRecord mulRK, bool bIgnoreStyles)
	{
		m_iUsedSize += iSizeDelta;
		int num = m_iUsedSize;
		for (int num2 = arrOffsets.Count - 1; num2 >= 0; num2--)
		{
			int num3 = arrOffsets[num2];
			int iLength;
			IMultiCellRecord multiCellRecord = CreateMultiCellRecord(num3, mulBlank, mulRK, out iLength);
			int num4 = num3 + iLength;
			int num5 = num - iSizeDelta - num4;
			if (num5 > 0)
			{
				m_dataProvider.MoveMemory(num4 + iSizeDelta, num4, num5);
			}
			int num6 = (multiCellRecord.LastColumn - multiCellRecord.FirstColumn + 1) * multiCellRecord.GetSeparateSubRecordSize(Version);
			BiffRecordRaw[] records = multiCellRecord.Split(bIgnoreStyles);
			InsertRecordData(num3, records);
			num = num4 + iSizeDelta;
			iSizeDelta -= num6 - iLength;
		}
	}

	private IMultiCellRecord CreateMultiCellRecord(int iOffset, MulBlankRecord mulBlank, MulRKRecord mulRK, out int iLength)
	{
		IMultiCellRecord result = null;
		TBIFFRecord tBIFFRecord = (TBIFFRecord)m_dataProvider.ReadInt16(iOffset);
		int num = m_dataProvider.ReadInt16(iOffset + 2);
		iLength = num + 4;
		switch (tBIFFRecord)
		{
		case TBIFFRecord.MulRK:
			mulRK.Length = num;
			mulRK.ParseStructure(m_dataProvider, iOffset + 4, 0, Version);
			result = mulRK;
			break;
		case TBIFFRecord.MulBlank:
			mulBlank.Length = num;
			mulBlank.ParseStructure(m_dataProvider, iOffset + 4, 0, Version);
			result = mulBlank;
			break;
		}
		return result;
	}

	private void DecompressRecord(int iOffset, IMultiCellRecord multi, bool bIgnoreStyles)
	{
		BiffRecordRaw[] records = multi.Split(bIgnoreStyles);
		InsertRecordData(iOffset, records);
	}

	public int GetRow(int recordStart)
	{
		switch (Version)
		{
		case OfficeVersion.Excel97to2003:
			return m_dataProvider.ReadUInt16(recordStart + 4);
		case OfficeVersion.Excel2007:
		case OfficeVersion.Excel2010:
		case OfficeVersion.Excel2013:
			return m_dataProvider.ReadInt32(recordStart + 4);
		default:
			throw new NotImplementedException();
		}
	}

	private void SetRow(int recordStart, int rowIndex)
	{
		switch (Version)
		{
		case OfficeVersion.Excel97to2003:
			m_dataProvider.WriteUInt16(recordStart + 4, (ushort)rowIndex);
			break;
		case OfficeVersion.Excel2007:
		case OfficeVersion.Excel2010:
		case OfficeVersion.Excel2013:
			m_dataProvider.WriteInt32(recordStart + 4, rowIndex);
			break;
		default:
			throw new NotImplementedException();
		}
	}

	public int GetColumn(int recordStart)
	{
		switch (Version)
		{
		case OfficeVersion.Excel97to2003:
			return m_dataProvider.ReadUInt16(recordStart + 4 + 2);
		case OfficeVersion.Excel2007:
		case OfficeVersion.Excel2010:
		case OfficeVersion.Excel2013:
			return m_dataProvider.ReadInt32(recordStart + 4 + 4);
		default:
			throw new NotImplementedException();
		}
	}

	private void SetColumn(int recordStart, int columnIndex)
	{
		switch (Version)
		{
		case OfficeVersion.Excel97to2003:
			m_dataProvider.WriteUInt16(recordStart + 4 + 2, (ushort)columnIndex);
			break;
		case OfficeVersion.Excel2007:
		case OfficeVersion.Excel2010:
		case OfficeVersion.Excel2013:
			m_dataProvider.WriteInt32(recordStart + 4 + 4, columnIndex);
			break;
		default:
			throw new NotImplementedException();
		}
	}

	private int GetLastColumn(int recordStart, int iLength)
	{
		switch (Version)
		{
		case OfficeVersion.Excel97to2003:
			return m_dataProvider.ReadInt16(recordStart + 4 + iLength - 2);
		case OfficeVersion.Excel2007:
		case OfficeVersion.Excel2010:
		case OfficeVersion.Excel2013:
			return m_dataProvider.ReadInt32(recordStart + 4 + iLength - 4);
		default:
			throw new NotImplementedException();
		}
	}

	[CLSCompliant(false)]
	public ushort GetXFIndex(int recordStart, bool bMulti)
	{
		if (!bMulti)
		{
			recordStart += 8;
			if (Version != 0)
			{
				recordStart += 4;
			}
		}
		return m_dataProvider.ReadUInt16(recordStart);
	}

	private void SetXFIndex(int recordStart, ushort xfIndex)
	{
		int num = recordStart + 4 + 4;
		if (Version != 0)
		{
			num += 4;
		}
		m_dataProvider.WriteUInt16(num, xfIndex);
	}

	private void SetXFIndexMulti(int recordStart, ushort xfIndex, int iColumnIndex, int subRecordSize)
	{
		int column = GetColumn(recordStart);
		recordStart += 8;
		if (Version != 0)
		{
			recordStart += 4;
		}
		recordStart += subRecordSize * (iColumnIndex - column);
		m_dataProvider.WriteUInt16(recordStart, xfIndex);
	}

	internal void UpdateFormulaFlags()
	{
		for (int num = 0; num < m_iUsedSize; num = MoveNext(num))
		{
			if (m_dataProvider.ReadInt16(num) == 6)
			{
				FormulaRecord.UpdateOptions(m_dataProvider, num);
			}
		}
	}

	public int GetStoreSize(OfficeVersion version)
	{
		Compress();
		return Math.Max(0, m_iUsedSize - 4);
	}

	public int GetBoolValue(int iCol)
	{
		bool bFound;
		int num = LocateRecord(iCol, out bFound);
		if (bFound && m_dataProvider.ReadInt16(num) == 517)
		{
			int num2 = BoolErrRecord.ReadValue(m_dataProvider, num, Version);
			if ((num2 & 0xFF00) == 0)
			{
				return num2;
			}
		}
		return 0;
	}

	public int GetFormulaBoolValue(int iCol)
	{
		bool bFound;
		int num = LocateRecord(iCol, out bFound);
		if (bFound && m_dataProvider.ReadInt16(num) == 6)
		{
			ulong num2 = (ulong)FormulaRecord.ReadInt64Value(m_dataProvider, num, Version);
			if ((num2 & 0xFFFF0000000000FFuL) == 18446462598732840961uL)
			{
				return (int)(num2 & 0xFF0000);
			}
		}
		return 0;
	}

	public string GetErrorValue(int iCol)
	{
		bool bFound;
		int num = LocateRecord(iCol, out bFound);
		if (bFound && m_dataProvider.ReadInt16(num) == 517)
		{
			int num2 = BoolErrRecord.ReadValue(m_dataProvider, num, Version);
			if (((uint)num2 & 0xFF00u) != 0)
			{
				return GetErrorString(num2 & 0xFF);
			}
		}
		return null;
	}

	public string GetFormulaErrorValue(int iCol)
	{
		bool bFound;
		int num = LocateRecord(iCol, out bFound);
		if (bFound && m_dataProvider.ReadInt16(num) == 6)
		{
			ulong num2 = (ulong)FormulaRecord.ReadInt64Value(m_dataProvider, num, Version);
			if ((num2 & 0xFFFF0000000000FFuL) == 18446462598732840962uL)
			{
				return GetErrorString((int)((num2 & 0xFF0000) >> 16));
			}
		}
		return null;
	}

	public double GetNumberValue(int iCol, int sheetIndex)
	{
		bool bFound;
		bool bMul;
		int num = LocateRecord(iCol, out bFound, out bMul, bGetRkOffset: true);
		if (bFound)
		{
			if (bMul)
			{
				return RKRecord.EncodeRK(m_dataProvider.ReadInt32(num + 2));
			}
			switch (m_dataProvider.ReadInt16(num))
			{
			case 515:
				return NumberRecord.ReadValue(m_dataProvider, num, Version);
			case 638:
				return RKRecord.EncodeRK(RKRecord.ReadValue(m_dataProvider, num, Version));
			case 253:
			{
				int sSTIndex = LabelSSTRecord.GetSSTIndex(m_dataProvider, num, Version);
				string text = m_book.InnerSST[sSTIndex].Text;
				if (m_book.ActiveSheet == null)
				{
					break;
				}
				string numberFormat = m_book.Worksheets[sheetIndex][m_row, iCol + 1].NumberFormat;
				RangeImpl rangeImpl = m_book.Worksheets[sheetIndex][m_row, iCol + 1] as RangeImpl;
				if (!CheckFormat(numberFormat))
				{
					break;
				}
				int num2 = numberFormat.IndexOf(';');
				if (num2 != -1)
				{
					numberFormat = numberFormat.Remove(num2, numberFormat.Length - num2);
				}
				if (!DateTime.TryParseExact(text, CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out var result) && !DateTime.TryParseExact(text, CultureInfo.CurrentCulture.NumberFormat.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
				{
					return double.NaN;
				}
				if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
				{
					if (rangeImpl != null && rangeImpl.TryGetDateTimeByCulture(text, text.Contains("."), out var dtValue))
					{
						result = dtValue;
					}
					try
					{
						return result.ToFileTimeUtc();
					}
					catch (Exception)
					{
						return double.NaN;
					}
				}
				if (rangeImpl != null && rangeImpl.TryGetDateTimeByCulture(text, isUKCulture: true, out var dtValue2))
				{
					result = dtValue2;
					return result.ToFileTimeUtc();
				}
				break;
			}
			}
		}
		return double.NaN;
	}

	private bool CheckFormat(string format)
	{
		int num = 0;
		string[] array = dateFormats;
		foreach (string value in array)
		{
			if (format.Contains(value))
			{
				num++;
			}
		}
		if (num > 1)
		{
			return true;
		}
		return false;
	}

	public double GetFormulaNumberValue(int iCol)
	{
		bool bFound;
		int num = LocateRecord(iCol, out bFound);
		if (bFound && m_dataProvider.ReadInt16(num) == 6)
		{
			return FormulaRecord.ReadDoubleValue(m_dataProvider, num, Version);
		}
		return double.NaN;
	}

	public string GetStringValue(int iColumn, SSTDictionary sst)
	{
		bool bFound;
		int num = LocateRecord(iColumn, out bFound);
		if (bFound)
		{
			switch (m_dataProvider.ReadInt16(num))
			{
			case 253:
			{
				int sSTIndex = LabelSSTRecord.GetSSTIndex(m_dataProvider, num, Version);
				return sst[sSTIndex];
			}
			case 214:
			case 516:
			{
				int num2 = 10;
				if (Version != 0)
				{
					num2 += 4;
				}
				int iFullLength;
				return m_dataProvider.ReadString16Bit(num + num2, out iFullLength);
			}
			default:
				return null;
			}
		}
		return null;
	}

	public string GetFormulaStringValue(int iColumnIndex)
	{
		bool bFound;
		int iOffset = LocateRecord(iColumnIndex, out bFound);
		if (!bFound)
		{
			return null;
		}
		return GetFormulaStringValueByOffset(iOffset);
	}

	public string GetFormulaStringValueByOffset(int iOffset)
	{
		if (m_dataProvider.ReadInt16(iOffset) != 6)
		{
			return null;
		}
		int num = m_dataProvider.ReadInt16(iOffset + 2);
		iOffset += 4 + num;
		if (iOffset >= m_iUsedSize)
		{
			return null;
		}
		if (m_dataProvider.ReadInt16(iOffset) != 519)
		{
			return null;
		}
		iOffset += 4;
		int iStrLen = m_dataProvider.ReadInt16(iOffset);
		int iBytesInString;
		return m_dataProvider.ReadString(iOffset + 2, iStrLen, out iBytesInString, isByteCounted: false);
	}

	public Ptg[] GetFormulaValue(int iCol)
	{
		bool bFound;
		int num = LocateRecord(iCol, out bFound);
		if (bFound && m_dataProvider.ReadInt16(num) == 6)
		{
			return FormulaRecord.ReadValue(m_dataProvider, num, Version);
		}
		return null;
	}

	public WorksheetImpl.TRangeValueType GetCellType(int iCol, bool bNeedFormulaSubType)
	{
		bool bFound;
		int num = LocateRecord(iCol, out bFound);
		if (bFound)
		{
			switch ((TBIFFRecord)m_dataProvider.ReadInt16(num))
			{
			case TBIFFRecord.BoolErr:
				if (((uint)BoolErrRecord.ReadValue(m_dataProvider, num, Version) & 0xFF00u) != 0)
				{
					return WorksheetImpl.TRangeValueType.Error;
				}
				return WorksheetImpl.TRangeValueType.Boolean;
			case TBIFFRecord.MulRK:
			case TBIFFRecord.Number:
			case TBIFFRecord.RK:
				return WorksheetImpl.TRangeValueType.Number;
			case TBIFFRecord.RString:
			case TBIFFRecord.LabelSST:
			case TBIFFRecord.Label:
				return WorksheetImpl.TRangeValueType.String;
			case TBIFFRecord.Formula:
				if (!bNeedFormulaSubType)
				{
					return WorksheetImpl.TRangeValueType.Formula;
				}
				return GetSubFormulaType(num);
			default:
				return WorksheetImpl.TRangeValueType.Blank;
			}
		}
		return WorksheetImpl.TRangeValueType.Blank;
	}

	private WorksheetImpl.TRangeValueType GetSubFormulaType(int iOffset)
	{
		WorksheetImpl.TRangeValueType tRangeValueType = WorksheetImpl.TRangeValueType.Formula;
		switch ((ulong)(FormulaRecord.ReadInt64Value(m_dataProvider, iOffset, Version) & -281474976710401L))
		{
		case 18446462598732840961uL:
			return tRangeValueType | WorksheetImpl.TRangeValueType.Boolean;
		case 18446462598732840962uL:
			return tRangeValueType | WorksheetImpl.TRangeValueType.Error;
		case 18446462598732840963uL:
			return tRangeValueType | WorksheetImpl.TRangeValueType.Blank;
		default:
		{
			int num = m_dataProvider.ReadInt16(iOffset + 2);
			int num2 = iOffset + 4 + num;
			bool flag = num2 < m_iUsedSize && m_dataProvider.ReadInt16(num2) == 519;
			return tRangeValueType | (flag ? WorksheetImpl.TRangeValueType.String : WorksheetImpl.TRangeValueType.Number);
		}
		}
	}

	public bool HasFormulaRecord(int iColumn)
	{
		bool bFound;
		int iOffset = LocateRecord(iColumn, out bFound);
		if (bFound && m_dataProvider.ReadInt16(iOffset) == 6)
		{
			return true;
		}
		return false;
	}

	public bool HasFormulaArrayRecord(int iCol)
	{
		int iOffset = LocateRecord(iCol, out var bFound);
		if (bFound && m_dataProvider.ReadInt16(iOffset) == 6)
		{
			iOffset = MoveNext(iOffset);
			if (iOffset < m_iUsedSize && m_dataProvider.ReadInt16(iOffset) == 545)
			{
				return true;
			}
		}
		return false;
	}

	internal string GetErrorString(int value)
	{
		IDictionary errorCodeToName = FormulaUtil.ErrorCodeToName;
		if (!errorCodeToName.Contains(value))
		{
			return null;
		}
		return (string)errorCodeToName[value];
	}

	internal void SetWorkbook(WorkbookImpl book, int iRow)
	{
		m_book = book;
		m_row = iRow;
	}

	[CLSCompliant(false)]
	public void SetFormulaValue(int iColumn, double value, StringRecord strRecord, int iBlockSize)
	{
		bool flag = false;
		int num = LocateRecord(iColumn, out var bFound);
		if (!bFound)
		{
			throw new ApplicationException("Cannot set formula number.");
		}
		FormulaRecord.WriteDoubleValue(m_dataProvider, num, Version, value);
		num += m_dataProvider.ReadInt16(num + 2) + 4;
		if (num < m_iUsedSize)
		{
			int num2 = m_dataProvider.ReadInt16(num);
			if (num2 == 545)
			{
				flag = true;
				num = MoveNext(num);
				num2 = m_dataProvider.ReadInt16(num);
			}
			if (num2 == 519 && !flag)
			{
				RemoveRecord(num);
			}
		}
		if (strRecord != null)
		{
			int iRequiredSize = strRecord.GetStoreSize(Version) + 4;
			InsertRecordData(num, 0, iRequiredSize, strRecord, iBlockSize);
		}
	}
}
