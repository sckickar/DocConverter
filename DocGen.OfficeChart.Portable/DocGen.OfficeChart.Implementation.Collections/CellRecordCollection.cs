using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using DocGen.Drawing;
using DocGen.OfficeChart.Calculate;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Formula;

namespace DocGen.OfficeChart.Implementation.Collections;

internal class CellRecordCollection : CommonObject, IDictionary, ICollection, IEnumerable
{
	private RecordTable m_dicRecords;

	private SFTable m_colRanges;

	private IInternalWorksheet m_worksheet;

	private WorkbookImpl m_book;

	private bool m_bUseCache;

	private RecordExtractor m_recordExtractor;

	public int FirstRow => m_worksheet.FirstRow;

	public int LastRow => m_worksheet.LastRow;

	public int FirstColumn => m_worksheet.FirstColumn;

	public int LastColumn => m_worksheet.LastColumn;

	internal IInternalWorksheet Sheet
	{
		get
		{
			if (m_worksheet is ExternWorksheetImpl)
			{
				return (ExternWorksheetImpl)m_worksheet;
			}
			return (WorksheetImpl)m_worksheet;
		}
	}

	public RecordTable Table
	{
		[DebuggerStepThrough]
		get
		{
			return m_dicRecords;
		}
		[DebuggerStepThrough]
		set
		{
			m_dicRecords = value;
		}
	}

	public bool UseCache
	{
		get
		{
			return m_bUseCache;
		}
		set
		{
			if (value != m_bUseCache)
			{
				if (value)
				{
					CreateRangesCollection();
				}
				else
				{
					m_colRanges = null;
				}
				m_bUseCache = value;
			}
		}
	}

	public OfficeVersion Version
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			m_dicRecords.RowCount = m_book.MaxRowCount;
			if (FirstRow < 0)
			{
				return;
			}
			int num = -1;
			int i = FirstRow - 1;
			for (int lastRow = LastRow; i < lastRow; i++)
			{
				ApplicationImpl appImplementation = m_book.AppImplementation;
				RowStorage orCreateRow = m_dicRecords.GetOrCreateRow(i, appImplementation.StandardHeightInRowUnits, bCreate: false, value);
				if (orCreateRow != null)
				{
					orCreateRow.SetVersion(value, base.AppImplementation.RowStorageAllocationBlockSize);
					num = i;
				}
			}
			if (num >= 0)
			{
				m_worksheet.LastRow = num + 1;
			}
		}
	}

	public RecordExtractor RecordExtractor => m_recordExtractor;

	public int Count
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public bool IsFixedSize => false;

	public bool IsReadOnly => false;

	public ICollection Keys
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public ICollection Values
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public object this[object key]
	{
		get
		{
			if (key is long)
			{
				return this[(long)key];
			}
			throw new NotSupportedException("Non Int64 keys are not support");
		}
		set
		{
			if (key is long)
			{
				this[(long)key] = value as ICellPositionFormat;
				return;
			}
			throw new NotSupportedException("Non Int64 keys are not support");
		}
	}

	[CLSCompliant(false)]
	public ICellPositionFormat this[long key]
	{
		get
		{
			int rowIndex = RangeImpl.GetRowFromCellIndex(key) - 1;
			int colIndex = RangeImpl.GetColumnFromCellIndex(key) - 1;
			return m_dicRecords[rowIndex, colIndex] as ICellPositionFormat;
		}
		set
		{
			int rowFromCellIndex = RangeImpl.GetRowFromCellIndex(key);
			int columnFromCellIndex = RangeImpl.GetColumnFromCellIndex(key);
			this[rowFromCellIndex, columnFromCellIndex] = value;
		}
	}

	[CLSCompliant(false)]
	public ICellPositionFormat this[int iRow, int iColumn]
	{
		get
		{
			return m_dicRecords[iRow - 1, iColumn - 1] as ICellPositionFormat;
		}
		set
		{
			if (value == null)
			{
				Remove(iRow, iColumn);
				return;
			}
			m_dicRecords[iRow - 1, iColumn - 1] = value;
			WorksheetHelper.AccessColumn(m_worksheet, iColumn);
			WorksheetHelper.AccessRow(m_worksheet, iRow);
		}
	}

	public bool IsSynchronized
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public object SyncRoot
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public CellRecordCollection(IApplication application, object parent)
		: base(application, parent)
	{
		SetParents();
		m_colRanges = new SFTable(m_book.MaxRowCount, m_book.MaxColumnCount);
		m_dicRecords = new RecordTable(m_book.MaxRowCount, m_worksheet);
		m_recordExtractor = new RecordExtractor();
	}

	private void SetParents()
	{
		m_worksheet = FindParent(typeof(IInternalWorksheet)) as IInternalWorksheet;
		if (m_worksheet == null)
		{
			throw new ArgumentOutOfRangeException("parent", "Can't find parent worksheet");
		}
		m_book = m_worksheet.ParentWorkbook;
	}

	public void Clear()
	{
		if (m_dicRecords != null)
		{
			m_dicRecords.Clear();
		}
	}

	public void Add(object key, object value)
	{
		if (key is long)
		{
			Add((long)key, value as ICellPositionFormat);
		}
	}

	public IDictionaryEnumerator GetEnumerator()
	{
		return new RecordTableEnumerator(this);
	}

	public void Remove(object key)
	{
		if (key is long)
		{
			Remove((long)key);
		}
	}

	public bool Contains(object key)
	{
		return Contains((long)key);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new RecordTableEnumerator(this);
	}

	[CLSCompliant(false)]
	public void Add(long key, ICellPositionFormat value)
	{
		Add(value);
	}

	[CLSCompliant(false)]
	public void Add(ICellPositionFormat value)
	{
		int row = value.Row;
		int column = value.Column;
		if (m_dicRecords.Contains(row, column))
		{
			throw new ArgumentOutOfRangeException("Collection already contains such member.");
		}
		SetCellRecord(row + 1, column + 1, value);
	}

	public void Remove(long key)
	{
		int rowIndex = RangeImpl.GetRowFromCellIndex(key) - 1;
		int colIndex = RangeImpl.GetColumnFromCellIndex(key) - 1;
		m_dicRecords[rowIndex, colIndex] = null;
	}

	public void Remove(int iRow, int iColumn)
	{
		m_dicRecords[iRow - 1, iColumn - 1] = null;
	}

	public bool ContainsRow(int iRowIndex)
	{
		return m_dicRecords.ContainsRow(iRowIndex);
	}

	public bool Contains(long key)
	{
		int rowIndex = RangeImpl.GetRowFromCellIndex(key) - 1;
		int colIndex = RangeImpl.GetColumnFromCellIndex(key) - 1;
		return m_dicRecords.Contains(rowIndex, colIndex);
	}

	public bool Contains(int iRow, int iColumn)
	{
		return m_dicRecords.Contains(iRow - 1, iColumn - 1);
	}

	public void CopyTo(Array array, int index)
	{
		throw new NotSupportedException();
	}

	[CLSCompliant(false)]
	public int Serialize(OffsetArrayList records, List<DBCellRecord> arrDBCells)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		OfficeVersion version = OfficeVersion.Excel97to2003;
		int num = 0;
		if (FirstRow < 0)
		{
			return num;
		}
		int firstRow = FirstRow;
		int lastRow = LastRow;
		int firstColumn = FirstColumn;
		int lastColumn = LastColumn;
		List<RowStorage> list = new List<RowStorage>(32);
		int num2;
		for (num2 = firstRow; num2 <= lastRow; num2++)
		{
			int iRowRecSize = 0;
			int iFirstRowOffset = 0;
			num2 = PrepareNextRowsBlock(records, list, num2, ref iRowRecSize, ref iFirstRowOffset, lastRow, firstColumn, lastColumn, OfficeVersion.Excel97to2003);
			DBCellRecord dBCellRecord = (DBCellRecord)BiffRecordFactory.GetRecord(TBIFFRecord.DBCell);
			int count = list.Count;
			dBCellRecord.CellOffsets = new ushort[count];
			arrDBCells.Add(dBCellRecord);
			if (count > 0)
			{
				dBCellRecord.CellOffsets[0] = (ushort)iRowRecSize;
				int i = 0;
				for (int num3 = count - 1; i < num3; i++)
				{
					RowStorage rowStorage = list[i];
					int num4 = rowStorage?.GetStoreSize(version) ?? 0;
					if (num4 != 0)
					{
						num4 += 4;
					}
					dBCellRecord.CellOffsets[i + 1] = (ushort)num4;
					if (rowStorage != null && num4 > 0)
					{
						records.Add(rowStorage);
					}
					iFirstRowOffset += num4;
				}
				dBCellRecord.RowOffset = iFirstRowOffset;
				RowStorage rowStorage2 = list[count - 1];
				if (rowStorage2 != null && rowStorage2.GetStoreSize(version) > 0)
				{
					records.Add(rowStorage2);
					dBCellRecord.RowOffset += rowStorage2.GetStoreSize(version) + 4;
				}
			}
			records.Add(dBCellRecord);
			num++;
			list.Clear();
		}
		return num;
	}

	private int PrepareNextRowsBlock(OffsetArrayList records, List<RowStorage> ranges, int i, ref int iRowRecSize, ref int iFirstRowOffset, int iLastRow, int iFirstSheetCol, int iLastSheetCol, OfficeVersion version)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (ranges == null)
		{
			throw new ArgumentNullException("ranges");
		}
		int num = 0;
		int num2 = i;
		int defaultRowHeight = m_worksheet.DefaultRowHeight;
		while (i <= iLastRow)
		{
			int min;
			int max;
			RowStorage rowData = GetRowData(i, iFirstSheetCol, iLastSheetCol, out min, out max, version);
			RowRecord rowRecord;
			if (rowData != null)
			{
				rowRecord = rowData.CreateRowRecord(m_book);
				rowRecord.Worksheet = m_worksheet as WorksheetImpl;
				rowRecord.RowNumber = (ushort)(i - 1);
			}
			else
			{
				rowRecord = null;
			}
			if (rowData != null && rowData.UsedSize > 0)
			{
				ranges.Add(rowData);
				if (rowRecord == null)
				{
					rowRecord = (RowRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Row);
					rowRecord.RowNumber = (ushort)(i - 1);
					rowRecord.Height = (ushort)defaultRowHeight;
					rowRecord.ExtendedFormatIndex = (ushort)m_book.DefaultXFIndex;
				}
				rowRecord.FirstColumn = (ushort)rowData.FirstColumn;
				rowRecord.LastColumn = (ushort)rowData.LastColumn;
				int num3 = rowRecord.MaximumRecordSize + 4;
				if (i != num2)
				{
					iRowRecSize += num3;
				}
				iFirstRowOffset += num3;
				records.Add(rowRecord);
			}
			else if (rowRecord != null)
			{
				records.Add(rowRecord);
				ranges.Add(rowData);
				int num4 = rowRecord.GetStoreSize(version) + 4;
				iFirstRowOffset += num4;
				if (i != num2)
				{
					iRowRecSize += num4;
				}
			}
			else if (i == num2)
			{
				num2++;
			}
			num++;
			if (num == 32)
			{
				break;
			}
			i++;
		}
		return i;
	}

	[CLSCompliant(false)]
	protected RowStorage GetRowData(int index, int iFirstColumn, int iLastColumn, out int min, out int max, OfficeVersion version)
	{
		RowStorage rowStorage = m_dicRecords.Rows[index - 1];
		min = int.MinValue;
		max = int.MaxValue;
		if (rowStorage != null)
		{
			if (version != rowStorage.Version)
			{
				rowStorage = (RowStorage)rowStorage.Clone(IntPtr.Zero);
				rowStorage.SetVersion(version, base.AppImplementation.RowStorageAllocationBlockSize);
			}
			min = rowStorage.FirstColumn + 1;
			max = rowStorage.LastColumn + 1;
		}
		return rowStorage;
	}

	[CLSCompliant(false)]
	public bool ExtractRangesFast(IndexRecord index, BiffReader reader, bool bIgnoreStyles, Dictionary<int, int> hashNewXFIndexes)
	{
		return m_dicRecords.ExtractRangesFast(index, reader, bIgnoreStyles, m_book.InnerSST, (WorksheetImpl)m_worksheet);
	}

	[CLSCompliant(false)]
	public void AddRecord(BiffRecordRaw recordToAdd, bool bIgnoreStyles)
	{
		if (recordToAdd == null)
		{
			throw new ArgumentNullException("recordToAdd");
		}
		switch (recordToAdd.TypeCode)
		{
		case TBIFFRecord.MulBlank:
			AddRecord((MulBlankRecord)recordToAdd, bIgnoreStyles);
			break;
		case TBIFFRecord.MulRK:
			AddRecord((MulRKRecord)recordToAdd, bIgnoreStyles);
			break;
		default:
			AddRecord((ICellPositionFormat)recordToAdd, bIgnoreStyles);
			break;
		}
	}

	[CLSCompliant(false)]
	public void AddRecord(ICellPositionFormat cell, bool bIgnoreStyles)
	{
		if (cell == null)
		{
			throw new ArgumentNullException("cell");
		}
		if (bIgnoreStyles)
		{
			cell.ExtendedFormatIndex = (ushort)m_book.DefaultXFIndex;
		}
		SetCellRecord(cell.Row + 1, cell.Column + 1, cell);
	}

	private void AddRecord(MulRKRecord mulRK, bool bIgnoreStyles)
	{
		if (mulRK == null)
		{
			throw new ArgumentNullException("mulRK");
		}
		List<MulRKRecord.RkRec> records = mulRK.Records;
		int row = mulRK.Row;
		int num = mulRK.FirstColumn;
		int num2 = 0;
		int lastColumn = mulRK.LastColumn;
		while (num <= lastColumn)
		{
			RKRecord rKRecord = (RKRecord)m_recordExtractor.GetRecord(638);
			rKRecord.SetRKRecord(records[num2]);
			rKRecord.Row = row;
			rKRecord.Column = num;
			AddRecord((ICellPositionFormat)rKRecord, bIgnoreStyles);
			num++;
			num2++;
		}
	}

	private void AddRecord(MulBlankRecord mulBlank, bool bIgnoreStyles)
	{
		if (mulBlank == null)
		{
			throw new ArgumentNullException("mulBlank");
		}
		if (!bIgnoreStyles)
		{
			int i = mulBlank.FirstColumn;
			for (int lastColumn = mulBlank.LastColumn; i <= lastColumn; i++)
			{
				AddRecord((ICellPositionFormat)mulBlank.GetBlankRecord(i), bIgnoreStyles);
			}
		}
	}

	private void AddRecord(FormulaRecord formula, StringRecord stringRecord, bool bIgnoreStyles)
	{
		if (formula == null)
		{
			throw new ArgumentNullException("formula");
		}
		AddRecord((ICellPositionFormat)formula, bIgnoreStyles);
	}

	internal bool IsRequireRange(FormulaRecord formula)
	{
		if (formula == null)
		{
			throw new ArgumentNullException("formula");
		}
		Ptg[] parsedExpression = formula.ParsedExpression;
		int i = 0;
		for (int num = parsedExpression.Length; i < num; i++)
		{
			FormulaToken tokenCode = parsedExpression[i].TokenCode;
			if (FormulaUtil.IndexOf(FormulaUtil.NameCodes, tokenCode) != -1 || FormulaUtil.IndexOf(FormulaUtil.NameXCodes, tokenCode) != -1)
			{
				return true;
			}
		}
		return false;
	}

	internal void UpdateRows(int rowCount)
	{
		m_dicRecords.UpdateRows(rowCount);
	}

	public CellRecordCollection Clone(object parent)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		CellRecordCollection cellRecordCollection = (CellRecordCollection)MemberwiseClone();
		cellRecordCollection.SetParent(parent);
		cellRecordCollection.SetParents();
		cellRecordCollection.m_dicRecords = (RecordTable)m_dicRecords.Clone(cellRecordCollection.m_worksheet);
		cellRecordCollection.m_colRanges = new SFTable(m_book.MaxRowCount, m_book.MaxColumnCount);
		return cellRecordCollection;
	}

	[CLSCompliant(false)]
	public void SetRange(long key, RangeImpl range)
	{
		if (m_bUseCache)
		{
			int rowFromCellIndex = RangeImpl.GetRowFromCellIndex(key);
			int columnFromCellIndex = RangeImpl.GetColumnFromCellIndex(key);
			SetRange(rowFromCellIndex, columnFromCellIndex, range);
		}
	}

	[CLSCompliant(false)]
	public void SetRange(int iRow, int iColumn, RangeImpl range)
	{
		if (m_bUseCache)
		{
			m_colRanges[iRow - 1, iColumn - 1] = range;
		}
	}

	public RangeImpl GetRange(long key)
	{
		int rowFromCellIndex = RangeImpl.GetRowFromCellIndex(key);
		int columnFromCellIndex = RangeImpl.GetColumnFromCellIndex(key);
		return GetRange(rowFromCellIndex, columnFromCellIndex);
	}

	public RangeImpl GetRange(int iRow, int iColumn)
	{
		RangeImpl result = null;
		if (m_bUseCache)
		{
			result = m_colRanges[iRow - 1, iColumn - 1] as RangeImpl;
		}
		return result;
	}

	[CLSCompliant(false)]
	public void SetCellRecord(long key, ICellPositionFormat cell)
	{
		this[key] = cell;
	}

	[CLSCompliant(false)]
	public void SetCellRecord(int iRow, int iColumn, ICellPositionFormat cell)
	{
		this[iRow, iColumn] = cell;
	}

	[CLSCompliant(false)]
	public ICellPositionFormat GetCellRecord(long key)
	{
		return this[key];
	}

	[CLSCompliant(false)]
	public ICellPositionFormat GetCellRecord(int iRow, int iColumn)
	{
		return this[iRow, iColumn];
	}

	public void ClearRange(Rectangle rect)
	{
		int top = rect.Top;
		int left = rect.Left;
		int bottom = rect.Bottom;
		int right = rect.Right;
		int rowStorageAllocationBlockSize = m_book.Application.RowStorageAllocationBlockSize;
		for (int i = top; i <= bottom; i++)
		{
			m_dicRecords.Rows[i]?.Remove(left, right, rowStorageAllocationBlockSize);
		}
	}

	public void CopyCells(CellRecordCollection sourceCells, Dictionary<string, string> hashStyleNames, Dictionary<string, string> hashWorksheetNames, Dictionary<int, int> hashExtFormatIndexes, Dictionary<int, int> dicNewNameIndexes, Dictionary<int, int> dicFontIndexes, Dictionary<int, int> dictExternSheet)
	{
		SSTDictionary innerSST = sourceCells.m_book.InnerSST;
		SSTDictionary innerSST2 = m_book.InnerSST;
		Clear();
		m_dicRecords.CopyCells(sourceCells.m_dicRecords, innerSST, innerSST2, hashExtFormatIndexes, hashWorksheetNames, dicNewNameIndexes, dicFontIndexes, dictExternSheet);
	}

	public RichTextString GetRTFString(long cellIndex, bool bAutofitRows)
	{
		ICellPositionFormat cellRecord = GetCellRecord(cellIndex);
		if (cellRecord == null)
		{
			return null;
		}
		switch (cellRecord.TypeCode)
		{
		case TBIFFRecord.LabelSST:
			return GetLabelSSTRTFString(cellIndex, bAutofitRows);
		case TBIFFRecord.Formula:
		case TBIFFRecord.Number:
		case TBIFFRecord.RK:
		{
			TextWithFormat textWithFormat2 = new TextWithFormat();
			string formulaStringValue = GetFormulaStringValue(cellIndex);
			if (formulaStringValue != null)
			{
				textWithFormat2.Text = formulaStringValue;
			}
			else
			{
				double number = GetNumber(cellIndex);
				if (cellRecord.TypeCode == TBIFFRecord.Formula && double.IsNaN(number))
				{
					textWithFormat2.Text = GetFormulaErrorBoolText(cellRecord as FormulaRecord);
				}
				else
				{
					FormatImpl format = GetFormat(cellIndex);
					textWithFormat2.Text = format.ApplyFormat(number, bShowHiddenSymbols: true);
				}
			}
			return new RangeRichTextString(base.Application, m_worksheet, cellIndex, textWithFormat2);
		}
		case TBIFFRecord.BoolErr:
		{
			TextWithFormat textWithFormat = new TextWithFormat();
			textWithFormat.Text = RangeImpl.ParseBoolError((BoolErrRecord)cellRecord);
			return new RangeRichTextString(base.Application, m_worksheet, cellIndex, textWithFormat);
		}
		default:
			return null;
		}
	}

	public void FillRTFString(long cellIndex, bool bAutofitRows, RichTextString richText)
	{
		richText.ClearFormatting();
		richText.Text = string.Empty;
		ICellPositionFormat cellRecord = GetCellRecord(cellIndex);
		if (cellRecord == null)
		{
			return;
		}
		switch (cellRecord.TypeCode)
		{
		case TBIFFRecord.LabelSST:
		{
			LabelSSTRecord labelSST = (LabelSSTRecord)cellRecord;
			FillLabelSSTRTFString(labelSST, bAutofitRows, richText);
			break;
		}
		case TBIFFRecord.Formula:
		case TBIFFRecord.Number:
		case TBIFFRecord.RK:
		{
			string text = GetFormulaStringValue(cellIndex);
			int extendedFormatIndex = cellRecord.ExtendedFormatIndex;
			ExtendedFormatImpl extendedFormatImpl = m_book.InnerExtFormats[extendedFormatIndex];
			richText.DefaultFontIndex = extendedFormatImpl.FontIndex;
			if (text == null)
			{
				double number = GetNumber(cellIndex);
				text = ((cellRecord.TypeCode == TBIFFRecord.Formula && double.IsNaN(number)) ? GetFormulaErrorBoolText(cellRecord as FormulaRecord) : (double.IsNaN(number) ? string.Empty : extendedFormatImpl.NumberFormatObject.ApplyFormat(number, bShowHiddenSymbols: true)));
			}
			richText.ClearFormatting();
			richText.Text = text;
			break;
		}
		case TBIFFRecord.BoolErr:
			richText.ClearFormatting();
			richText.Text = RangeImpl.ParseBoolError((BoolErrRecord)cellRecord);
			break;
		}
	}

	public RichTextString GetLabelSSTRTFString(long cellIndex, bool bAutofitRows)
	{
		RangeRichTextString rangeRichTextString = ((WorksheetImpl)Sheet).CreateLabelSSTRTFString(cellIndex);
		string text = GetFormat(cellIndex).ApplyFormat(rangeRichTextString.Text, bShowHiddenSymbols: true);
		if (text == rangeRichTextString.Text || bAutofitRows)
		{
			return rangeRichTextString;
		}
		IOfficeFont font = GetFont(cellIndex);
		RichTextString richTextString = new RichTextString(base.Application, m_book, isReadOnly: false, bCreateText: true);
		richTextString.Text = text;
		richTextString.SetFont(0, text.Length - 1, font);
		return richTextString;
	}

	[CLSCompliant(false)]
	public void FillLabelSSTRTFString(LabelSSTRecord labelSST, bool bAutofitRows, RichTextString richText)
	{
		FillRichText(richText, labelSST.SSTIndex);
		int extendedFormatIndex = labelSST.ExtendedFormatIndex;
		ExtendedFormatImpl extendedFormatImpl = m_book.InnerExtFormats[extendedFormatIndex];
		int numberFormatIndex = extendedFormatImpl.NumberFormatIndex;
		string text = m_book.InnerFormats[numberFormatIndex].ApplyFormat(richText.Text, bShowHiddenSymbols: true);
		if (!(text == richText.Text || bAutofitRows))
		{
			IOfficeFont font = extendedFormatImpl.Font;
			richText.Text = text;
			richText.SetFont(0, text.Length - 1, font);
		}
		else
		{
			richText.DefaultFontIndex = extendedFormatImpl.FontIndex;
		}
	}

	public string GetText(long cellIndex)
	{
		ICellPositionFormat cellRecord = GetCellRecord(cellIndex);
		if (cellRecord != null)
		{
			if (cellRecord.TypeCode == TBIFFRecord.Label)
			{
				return ((LabelRecord)cellRecord).Label;
			}
			if (cellRecord.TypeCode == TBIFFRecord.LabelSST)
			{
				int sSTIndex = ((LabelSSTRecord)cellRecord).SSTIndex;
				return m_book.InnerSST[sSTIndex].Text;
			}
		}
		return null;
	}

	public string GetError(long cellIndex)
	{
		if (GetCellRecord(cellIndex) is BoolErrRecord { IsErrorCode: not false } boolErrRecord)
		{
			string result = "#N/A";
			int boolOrError = boolErrRecord.BoolOrError;
			if (FormulaUtil.ErrorCodeToName.ContainsKey(boolOrError))
			{
				result = FormulaUtil.ErrorCodeToName[boolOrError];
			}
			return result;
		}
		return null;
	}

	public bool GetBool(long cellIndex, out bool value)
	{
		BoolErrRecord boolErrRecord = GetCellRecord(cellIndex) as BoolErrRecord;
		value = false;
		if (boolErrRecord != null && !boolErrRecord.IsErrorCode)
		{
			value = boolErrRecord.BoolOrError > 0;
			return true;
		}
		return false;
	}

	public bool ContainNumber(long cellIndex)
	{
		return GetCellRecord(cellIndex) is IDoubleValue;
	}

	public bool ContainBoolOrError(long cellIndex)
	{
		return GetCellRecord(cellIndex) is BoolErrRecord;
	}

	public bool ContainFormulaNumber(long cellIndex)
	{
		if (GetCellRecord(cellIndex) is FormulaRecord { IsBool: false } formulaRecord)
		{
			return !formulaRecord.IsError;
		}
		return false;
	}

	public bool ContainFormulaBoolOrError(long cellIndex)
	{
		if (GetCellRecord(cellIndex) is FormulaRecord formulaRecord)
		{
			if (!formulaRecord.IsBool)
			{
				return formulaRecord.IsError;
			}
			return true;
		}
		return false;
	}

	public double GetNumber(long cellIndex)
	{
		if (!(GetCellRecord(cellIndex) is IDoubleValue doubleValue))
		{
			return double.MinValue;
		}
		return doubleValue.DoubleValue;
	}

	public double GetNumberWithoutFormula(long cellIndex)
	{
		if (!(GetCellRecord(cellIndex) is IDoubleValue { TypeCode: not TBIFFRecord.Formula } doubleValue))
		{
			return double.MinValue;
		}
		return doubleValue.DoubleValue;
	}

	public double GetFormulaNumberValue(long cellIndex)
	{
		if (GetCellRecord(cellIndex) is FormulaRecord formulaRecord)
		{
			return formulaRecord.Value;
		}
		return double.MinValue;
	}

	public void SetStringValue(long cellIndex, string strValue)
	{
		int index = RangeImpl.GetRowFromCellIndex(cellIndex) - 1;
		int iColumnIndex = RangeImpl.GetColumnFromCellIndex(cellIndex) - 1;
		(m_dicRecords.Rows[index] ?? throw new NotSupportedException("This property is only for formula ranges.")).SetFormulaStringValue(iColumnIndex, strValue, base.Application.RowStorageAllocationBlockSize);
	}

	public string GetFormulaStringValue(long cellIndex)
	{
		int index = RangeImpl.GetRowFromCellIndex(cellIndex) - 1;
		int iColumnIndex = RangeImpl.GetColumnFromCellIndex(cellIndex) - 1;
		return m_dicRecords.Rows[index].GetFormulaStringValue(iColumnIndex);
	}

	public DateTime GetDateTime(long cellIndex)
	{
		double numberWithoutFormula = GetNumberWithoutFormula(cellIndex);
		if (numberWithoutFormula == double.MinValue)
		{
			return DateTime.MinValue;
		}
		if (GetFormat(cellIndex).GetFormatType(numberWithoutFormula) == OfficeFormatType.DateTime)
		{
			return CalcEngineHelper.FromOADate(numberWithoutFormula);
		}
		return DateTime.MinValue;
	}

	public RecordTable CacheIntersection(IRange destination, IRange source, out Rectangle rectIntersection)
	{
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (destination.Worksheet != source.Worksheet)
		{
			rectIntersection = Rectangle.FromLTRB(-1, -1, -1, -1);
			return null;
		}
		int column = source.Column;
		int row = source.Row;
		int height = source.LastRow - row + 1;
		int width = source.LastColumn - column + 1;
		Rectangle rectangle = new Rectangle(destination.Column, destination.Row, width, height);
		Rectangle rectangle2 = new Rectangle(column, row, width, height);
		if (!UtilityMethods.Intersects(rectangle, rectangle2))
		{
			rectIntersection = Rectangle.FromLTRB(-1, -1, -1, -1);
			return null;
		}
		rectIntersection = Rectangle.Intersect(rectangle, rectangle2);
		if (rectIntersection.Width == 0 || rectIntersection.Height == 0)
		{
			rectIntersection = Rectangle.FromLTRB(-1, -1, -1, -1);
			return null;
		}
		RecordTable recordTable = new RecordTable(m_book.MaxRowCount, m_worksheet);
		for (int i = rectIntersection.Top; i < rectIntersection.Bottom; i++)
		{
			RowStorage rowStorage = m_dicRecords.Rows[i - 1];
			if (rowStorage != null)
			{
				RowStorage row2 = rowStorage.Clone(rectIntersection.Left - 1, rectIntersection.Right - 1, base.Application.RowStorageAllocationBlockSize);
				recordTable.SetRow(i - 1, row2);
			}
		}
		return recordTable;
	}

	public int GetMinimumRowIndex(int iStartColumn, int iEndColumn)
	{
		int firstRow = m_worksheet.FirstRow;
		int num = m_worksheet.LastRow;
		for (int i = firstRow; i < num; i++)
		{
			for (int j = iStartColumn; j <= iEndColumn; j++)
			{
				if (m_dicRecords.Contains(i - 1, j - 1))
				{
					num = i;
					break;
				}
			}
		}
		return num;
	}

	public int GetMaximumRowIndex(int iStartColumn, int iEndColumn)
	{
		int lastRow = m_worksheet.LastRow;
		int num = m_worksheet.FirstRow;
		for (int num2 = lastRow; num2 >= num; num2--)
		{
			for (int i = iStartColumn; i <= iEndColumn; i++)
			{
				long cellIndex = RangeImpl.GetCellIndex(i, num2);
				if (Contains(cellIndex))
				{
					num = num2;
					break;
				}
			}
		}
		return num;
	}

	public int GetMinimumColumnIndex(int iStartRow, int iEndRow)
	{
		return m_dicRecords.GetMinimumColumnIndex(iStartRow - 1, iEndRow - 1) + 1;
	}

	public int GetMaximumColumnIndex(int iStartRow, int iEndRow)
	{
		return m_dicRecords.GetMaximumColumnIndex(iStartRow - 1, iEndRow - 1) + 1;
	}

	public string GetFormula(long cellIndex)
	{
		return GetFormula(cellIndex, isR1C1: false);
	}

	public string GetFormula(long cellIndex, bool isR1C1)
	{
		return GetFormula(cellIndex, isR1C1, null);
	}

	public string GetFormula(long cellIndex, bool isR1C1, NumberFormatInfo numberInfo)
	{
		if (GetCellRecord(cellIndex) is FormulaRecord formulaRecord)
		{
			try
			{
				FormulaUtil formulaUtil = m_book.FormulaUtil;
				return "=" + formulaUtil.ParsePtgArray(formulaRecord.ParsedExpression, formulaRecord.Row, formulaRecord.Column, isR1C1, numberInfo, isForSerialization: false);
			}
			catch (Exception)
			{
				return null;
			}
		}
		return null;
	}

	public string GetValue(long cellIndex, int row, int column, IRange range, string seperator)
	{
		if (Contains(cellIndex))
		{
			string text = "";
			bool flag = false;
			string formula = GetFormula(cellIndex);
			if (formula != null)
			{
				return range[row, column].DisplayText;
			}
			formula = GetText(cellIndex);
			if (formula != null)
			{
				text = formula;
				flag = true;
			}
			if (flag)
			{
				if (text.StartsWith("\""))
				{
					text = text.Replace("\"", "\"\"");
					text = "\"" + text + "\"";
				}
				else if (text.Contains("\""))
				{
					text = text.Replace("\"", "\"\"");
					return "\"" + text + "\"";
				}
				if (text.Contains(seperator))
				{
					text = "\"" + text + "\"";
				}
				return text;
			}
			formula = GetError(cellIndex);
			if (formula != null)
			{
				text = formula;
			}
			if (GetBool(cellIndex, out var value))
			{
				text = value.ToString();
			}
			double numberWithoutFormula = GetNumberWithoutFormula(cellIndex);
			if (numberWithoutFormula != double.MinValue)
			{
				text = GetFormat(cellIndex).ApplyFormat(numberWithoutFormula);
			}
			if (GetDateTime(cellIndex) != DateTime.MinValue)
			{
				text = range[row, column].DisplayText;
			}
			return text;
		}
		return string.Empty;
	}

	public int GetExtendedFormatIndex(long cellIndex)
	{
		int rowFromCellIndex = RangeImpl.GetRowFromCellIndex(cellIndex);
		int columnFromCellIndex = RangeImpl.GetColumnFromCellIndex(cellIndex);
		return GetExtendedFormatIndex(rowFromCellIndex, columnFromCellIndex);
	}

	public int GetExtendedFormatIndex(int row, int column)
	{
		row--;
		column--;
		RowStorage rowStorage = Table.Rows[row];
		int result = int.MinValue;
		if (rowStorage != null)
		{
			result = rowStorage.GetXFIndexByColumn(column);
		}
		return result;
	}

	public int GetExtendedFormatIndexByRow(int row)
	{
		row--;
		RowStorage rowStorage = Table.Rows[row];
		int result = int.MinValue;
		if (rowStorage != null)
		{
			result = rowStorage.ExtendedFormatIndex;
		}
		return result;
	}

	public int GetExtendedFormatIndexByColumn(int column)
	{
		int result = int.MinValue;
		ColumnInfoRecord columnInfoRecord = (ColumnInfoRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ColumnInfo);
		columnInfoRecord.FirstColumn = (ushort)(column - 1);
		if (columnInfoRecord != null)
		{
			result = columnInfoRecord.ExtendedFormatIndex;
		}
		return result;
	}

	public IOfficeFont GetCellFont(long cellIndex)
	{
		int extendedFormatIndex = GetExtendedFormatIndex(cellIndex);
		if (extendedFormatIndex < 0)
		{
			return null;
		}
		return m_book.InnerExtFormats[extendedFormatIndex].Font;
	}

	public void CopyStyle(int iSourceRow, int iSourceColumn, int iDestRow, int iDestColumn)
	{
		ICellPositionFormat cellRecord = GetCellRecord(iSourceRow, iSourceColumn);
		if (cellRecord != null)
		{
			ushort extendedFormatIndex = cellRecord.ExtendedFormatIndex;
			SetCellStyle(iDestRow, iDestColumn, extendedFormatIndex);
		}
	}

	[CLSCompliant(false)]
	public ICellPositionFormat CreateCellNoAdd(int iRow, int iColumn, TBIFFRecord recordType)
	{
		ICellPositionFormat obj = (ICellPositionFormat)BiffRecordFactory.GetRecord(recordType);
		obj.Row = iRow - 1;
		obj.Column = iColumn - 1;
		return obj;
	}

	[CLSCompliant(false)]
	public ICellPositionFormat CreateCell(int iRow, int iColumn, TBIFFRecord recordType)
	{
		ICellPositionFormat cellPositionFormat = CreateCellNoAdd(iRow, iColumn, recordType);
		SetCellRecord(iRow, iColumn, cellPositionFormat);
		return cellPositionFormat;
	}

	public IStyle GetCellStyle(long cellIndex)
	{
		int extendedFormatIndex = GetCellRecord(cellIndex).ExtendedFormatIndex;
		return m_book.InnerStyles.GetByXFIndex(extendedFormatIndex);
	}

	public IExtendedFormat GetCellFormatting(long cellIndex)
	{
		int extendedFormatIndex = GetCellRecord(cellIndex).ExtendedFormatIndex;
		return m_book.InnerExtFormats[extendedFormatIndex];
	}

	public void SetNumberValue(int iRow, int iCol, double dValue)
	{
		SetNumberValue(iCol, iRow, dValue, m_book.DefaultXFIndex);
	}

	public void SetNumberValue(long cellIndex, double dValue)
	{
		int rowFromCellIndex = RangeImpl.GetRowFromCellIndex(cellIndex);
		int columnFromCellIndex = RangeImpl.GetColumnFromCellIndex(cellIndex);
		SetNumberValue(columnFromCellIndex, rowFromCellIndex, dValue);
	}

	public void SetNumberValue(int iRow, int iCol, double dValue, int iXFIndex)
	{
		NumberRecord numberRecord = (NumberRecord)m_recordExtractor.GetRecord(515);
		numberRecord.Value = dValue;
		numberRecord.Row = iRow - 1;
		numberRecord.Column = iCol - 1;
		numberRecord.ExtendedFormatIndex = (ushort)iXFIndex;
		this[iRow, iCol] = numberRecord;
	}

	public void SetBooleanValue(int iRow, int iCol, bool bValue)
	{
		SetBooleanValue(iCol, iRow, bValue, m_book.DefaultXFIndex);
	}

	public void SetBooleanValue(long cellIndex, bool bValue)
	{
		int rowFromCellIndex = RangeImpl.GetRowFromCellIndex(cellIndex);
		int columnFromCellIndex = RangeImpl.GetColumnFromCellIndex(cellIndex);
		SetBooleanValue(columnFromCellIndex, rowFromCellIndex, bValue);
	}

	public void SetBooleanValue(int iRow, int iCol, bool bValue, int iXFIndex)
	{
		BoolErrRecord boolErrRecord = (BoolErrRecord)m_recordExtractor.GetRecord(517);
		boolErrRecord.IsErrorCode = false;
		boolErrRecord.BoolOrError = (bValue ? ((byte)1) : ((byte)0));
		boolErrRecord.Row = iRow - 1;
		boolErrRecord.Column = iCol - 1;
		boolErrRecord.ExtendedFormatIndex = (ushort)iXFIndex;
		SetCellRecord(RangeImpl.GetCellIndex(iCol, iRow), boolErrRecord);
	}

	public void SetErrorValue(int iRow, int iCol, string strValue)
	{
		SetErrorValue(iCol, iRow, strValue, m_book.DefaultXFIndex);
	}

	public void SetErrorValue(long cellIndex, string strValue)
	{
		int rowFromCellIndex = RangeImpl.GetRowFromCellIndex(cellIndex);
		int columnFromCellIndex = RangeImpl.GetColumnFromCellIndex(cellIndex);
		SetErrorValue(columnFromCellIndex, rowFromCellIndex, strValue);
	}

	public void SetErrorValue(int iRow, int iCol, string strValue, int iXFIndex)
	{
		if (FormulaUtil.ErrorNameToCode.TryGetValue(strValue, out var value))
		{
			SetErrorValue(iRow, iCol, (byte)value, iXFIndex);
			return;
		}
		throw new ArgumentOutOfRangeException("strValue");
	}

	public void SetErrorValue(int iRow, int iCol, byte errorCode, int iXFIndex)
	{
		BoolErrRecord boolErrRecord = (BoolErrRecord)m_recordExtractor.GetRecord(517);
		boolErrRecord.IsErrorCode = true;
		boolErrRecord.BoolOrError = errorCode;
		boolErrRecord.Row = iRow - 1;
		boolErrRecord.Column = iCol - 1;
		boolErrRecord.ExtendedFormatIndex = (ushort)iXFIndex;
		SetCellRecord(RangeImpl.GetCellIndex(iCol, iRow), boolErrRecord);
	}

	public void SetFormula(int iRow, int iCol, string strValue, int iXFIndex)
	{
		SetFormula(iRow, iCol, strValue, iXFIndex, isR1C1: false);
	}

	public void SetFormula(int iRow, int iCol, string strValue, int iXFIndex, bool isR1C1, NumberFormatInfo formatInfo)
	{
		SetFormula(iRow, iCol, strValue, iXFIndex, isR1C1, bParse: true, formatInfo);
	}

	public void SetFormula(int iRow, int iCol, string strValue, int iXFIndex, bool isR1C1)
	{
		SetFormula(iRow, iCol, strValue, iXFIndex, isR1C1, bParse: true, null);
	}

	public void SetFormula(int iRow, int iCol, string strValue, int iXFIndex, bool isR1C1, bool bParse, NumberFormatInfo formatInfo)
	{
		FormulaRecord formulaRecord = (FormulaRecord)m_recordExtractor.GetRecord(6);
		strValue = strValue.Substring(1);
		FormulaUtil formulaUtil = m_worksheet.ParentWorkbook.FormulaUtil;
		if (bParse)
		{
			formulaUtil.NumberFormat = NumberFormatInfo.InvariantInfo;
			formulaRecord.ParsedExpression = formulaUtil.ParseString(strValue, Sheet, null, iRow - 1, iCol - 1, isR1C1);
			formulaUtil.NumberFormat = null;
		}
		formulaRecord.Row = iRow - 1;
		formulaRecord.Column = iCol - 1;
		formulaRecord.ExtendedFormatIndex = (ushort)iXFIndex;
		SetCellRecord(RangeImpl.GetCellIndex(iCol, iRow), formulaRecord);
	}

	public void SetBlank(int iRow, int iCol, int iXFIndex)
	{
		BlankRecord blankRecord = (BlankRecord)m_recordExtractor.GetRecord(513);
		blankRecord.Row = iRow - 1;
		blankRecord.Column = iCol - 1;
		blankRecord.ExtendedFormatIndex = (ushort)iXFIndex;
		SetCellRecord(iRow, iCol, blankRecord);
	}

	public void SetRTF(int iRow, int iCol, int iXFIndex, TextWithFormat rtf)
	{
		if (rtf == null)
		{
			throw new ArgumentNullException("rtf");
		}
		LabelSSTRecord labelSSTRecord = (LabelSSTRecord)m_recordExtractor.GetRecord(253);
		labelSSTRecord.Row = iRow - 1;
		labelSSTRecord.Column = iCol - 1;
		labelSSTRecord.ExtendedFormatIndex = (ushort)iXFIndex;
		SortedList<int, int> innerFormattingRuns = rtf.InnerFormattingRuns;
		if (innerFormattingRuns != null && innerFormattingRuns.Count <= 1)
		{
			rtf.FormattingRuns.Clear();
		}
		labelSSTRecord.SSTIndex = m_book.InnerSST.AddIncrease(rtf);
		SetCellRecord(iRow, iCol, labelSSTRecord);
	}

	public void SetSingleStringValue(int iRow, int iCol, int iXFIndex, int iSSTIndex)
	{
		LabelSSTRecord labelSSTRecord = (LabelSSTRecord)m_recordExtractor.GetRecord(253);
		labelSSTRecord.Row = iRow - 1;
		labelSSTRecord.Column = iCol - 1;
		labelSSTRecord.ExtendedFormatIndex = (ushort)iXFIndex;
		labelSSTRecord.SSTIndex = iSSTIndex;
		m_book.InnerSST.AddIncrease(iSSTIndex);
		Add(labelSSTRecord);
	}

	internal void SetNonSSTString(int row, int column, int iXFIndex, string strValue)
	{
		LabelRecord labelRecord = (LabelRecord)m_recordExtractor.GetRecord(516);
		labelRecord.Row = row - 1;
		labelRecord.Column = column - 1;
		labelRecord.ExtendedFormatIndex = (ushort)iXFIndex;
		labelRecord.Label = strValue;
		SetCellRecord(row, column, labelRecord);
	}

	public void FreeRange(int iRow, int iColumn)
	{
		SetRange(iRow, iColumn, null);
		ICellPositionFormat cellRecord = GetCellRecord(iRow, iColumn);
		if (cellRecord != null && cellRecord.TypeCode == TBIFFRecord.Blank)
		{
			ushort extendedFormatIndex = cellRecord.ExtendedFormatIndex;
			int num = m_book.DefaultXFIndex;
			IOutline rowOutline = WorksheetHelper.GetRowOutline(Sheet, iRow);
			ColumnInfoRecord columnInfoRecord = ((Sheet is WorksheetImpl worksheetImpl) ? worksheetImpl.ColumnInformation[iColumn] : null);
			if (columnInfoRecord != null)
			{
				num = columnInfoRecord.ExtendedFormatIndex;
			}
			if (rowOutline != null)
			{
				num = rowOutline.ExtendedFormatIndex;
			}
			if (extendedFormatIndex == num)
			{
				Remove(iRow, iColumn);
			}
		}
	}

	public void ClearData()
	{
		if (m_dicRecords == null)
		{
			return;
		}
		int i = 0;
		for (int rowCount = m_dicRecords.RowCount; i < rowCount; i++)
		{
			RowStorage rowStorage = m_dicRecords.Rows[i];
			if (rowStorage != null)
			{
				rowStorage.ClearData();
				rowStorage.ExtendedFormatIndex = (ushort)m_book.DefaultXFIndex;
			}
		}
	}

	[CLSCompliant(false)]
	public void SetArrayFormula(ArrayRecord record)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		m_dicRecords.Rows[record.FirstRow].SetArrayRecord(record.FirstColumn, record, base.Application.RowStorageAllocationBlockSize);
	}

	[CLSCompliant(false)]
	public ArrayRecord GetArrayRecord(int iRow, int iColumn)
	{
		iRow--;
		iColumn--;
		RowStorage rowStorage = m_dicRecords.Rows[iRow];
		if (rowStorage == null)
		{
			return null;
		}
		FormulaRecord formulaRecord = (rowStorage.HasFormulaRecord(iColumn) ? (rowStorage.GetRecord(iColumn, base.Application.RowStorageAllocationBlockSize) as FormulaRecord) : null);
		if (formulaRecord == null)
		{
			return null;
		}
		Ptg[] parsedExpression = formulaRecord.ParsedExpression;
		if (parsedExpression.Length != 1)
		{
			return null;
		}
		Ptg ptg = parsedExpression[0];
		if (ptg.TokenCode != FormulaToken.tExp)
		{
			return null;
		}
		ControlPtg controlPtg = ptg as ControlPtg;
		if (controlPtg.RowIndex != iRow)
		{
			rowStorage = m_dicRecords.Rows[controlPtg.RowIndex];
		}
		return rowStorage.GetArrayRecord(controlPtg.ColumnIndex);
	}

	public void UpdateFormula(int iCurIndex, int iSourceIndex, Rectangle sourceRect, int iDestIndex, Rectangle destRect)
	{
		m_dicRecords.UpdateFormula(iCurIndex, iSourceIndex, sourceRect, iDestIndex, destRect);
	}

	public void RemoveLastColumn(int iColumnIndex)
	{
		m_dicRecords.RemoveLastColumn(iColumnIndex - 1);
	}

	public void RemoveRow(int iRowIndex)
	{
		m_dicRecords.RemoveRow(iRowIndex - 1);
	}

	public void UpdateNameIndexes(WorkbookImpl book, int[] arrNewIndex)
	{
		if (arrNewIndex == null)
		{
			throw new ArgumentNullException("arrNewIndex");
		}
		m_dicRecords.UpdateNameIndexes(book, arrNewIndex);
	}

	public void UpdateNameIndexes(WorkbookImpl book, IDictionary<int, int> dicNewIndex)
	{
		if (dicNewIndex == null)
		{
			throw new ArgumentNullException("dicNewIndex");
		}
		m_dicRecords.UpdateNameIndexes(book, dicNewIndex);
	}

	[CLSCompliant(false)]
	public void ReplaceSharedFormula()
	{
		m_dicRecords.ReplaceSharedFormula(m_book);
	}

	public void UpdateStringIndexes(List<int> arrNewIndexes)
	{
		if (arrNewIndexes == null)
		{
			throw new ArgumentNullException("arrNewIndexes");
		}
		m_dicRecords.UpdateStringIndexes(arrNewIndexes);
	}

	public List<long> Find(IRange range, string findValue, OfficeFindType flags, bool bIsFindFirst)
	{
		return m_dicRecords.Find(range, findValue, flags, bIsFindFirst, m_book);
	}

	public List<long> Find(IRange range, string findValue, OfficeFindType flags, OfficeFindOptions findOptions, bool bIsFindFirst)
	{
		return m_dicRecords.Find(range, findValue, flags, findOptions, bIsFindFirst, m_book);
	}

	public List<long> Find(IRange range, double findValue, OfficeFindType flags, bool bIsFindFirst)
	{
		return m_dicRecords.Find(range, findValue, flags, bIsFindFirst, m_book);
	}

	public List<long> Find(IRange range, byte findValue, bool bIsError, bool bIsFindFirst)
	{
		return m_dicRecords.Find(range, findValue, bIsError, bIsFindFirst, m_book);
	}

	public List<long> Find(Dictionary<int, object> dictIndexes)
	{
		return m_dicRecords.Find(dictIndexes);
	}

	public RecordTable CacheAndRemove(RangeImpl sourceRange, int iDeltaRow, int iDeltaColumn, ref int iMaxRow, ref int iMaxColumn)
	{
		Rectangle rectSource = sourceRange.GetRectangles()[0];
		RecordTable result = m_dicRecords.CacheAndRemove(rectSource, iDeltaRow, iDeltaColumn, ref iMaxRow, ref iMaxColumn);
		iMaxRow++;
		iMaxColumn++;
		return result;
	}

	public void UpdateExtendedFormatIndex(Dictionary<int, int> dictFormats)
	{
		m_dicRecords.UpdateExtendedFormatIndex(dictFormats);
	}

	public void UpdateExtendedFormatIndex(int[] arrFormats)
	{
		m_dicRecords.UpdateExtendedFormatIndex(arrFormats);
	}

	public void UpdateExtendedFormatIndex(int maxCount)
	{
		m_dicRecords.UpdateExtendedFormatIndex(maxCount);
	}

	public void SetCellStyle(int iRow, int iColumn, int iXFIndex)
	{
		Table.GetOrCreateRow(iRow - 1, base.AppImplementation.StandardHeightInRowUnits, bCreate: true, m_worksheet.Version).SetCellStyle(iRow - 1, iColumn - 1, iXFIndex, base.Application.RowStorageAllocationBlockSize);
		if (iXFIndex != m_book.DefaultXFIndex)
		{
			WorksheetHelper.AccessColumn(m_worksheet, iColumn);
			WorksheetHelper.AccessRow(m_worksheet, iRow);
		}
	}

	public void SetCellStyle(int iRow, int index)
	{
		RowStorage orCreateRow = Table.GetOrCreateRow(iRow - 1, base.AppImplementation.StandardHeightInRowUnits, bCreate: true, m_worksheet.Version);
		orCreateRow.ExtendedFormatIndex = (ushort)index;
		orCreateRow.IsFormatted = true;
	}

	public void ReAddAllStrings()
	{
		int i = FirstRow;
		for (int lastRow = LastRow; i <= lastRow; i++)
		{
			ApplicationImpl appImplementation = m_book.AppImplementation;
			m_dicRecords.GetOrCreateRow(i - 1, appImplementation.StandardHeightInRowUnits, bCreate: false, OfficeVersion.Excel97to2003)?.ReAddAllStrings(m_book.InnerSST);
		}
	}

	public void MarkUsedReferences(bool[] usedItems)
	{
		m_dicRecords.MarkUsedReferences(usedItems);
	}

	public void UpdateReferenceIndexes(int[] arrUpdatedIndexes)
	{
		m_dicRecords.UpdateReferenceIndexes(arrUpdatedIndexes);
	}

	private void InsertIntoDefaultRows(int iRowIndex, int iRowCount)
	{
		Table.InsertIntoDefaultRows(iRowIndex, iRowCount);
	}

	private string GetFormulaErrorBoolText(FormulaRecord formula)
	{
		if (formula == null)
		{
			throw new ArgumentNullException("formula");
		}
		if (!double.IsNaN(formula.Value))
		{
			throw new ArgumentException("Formula record doesnot support error or bool.");
		}
		if (formula.IsBool)
		{
			return formula.BooleanValue.ToString().ToUpper();
		}
		int errorValue = formula.ErrorValue;
		if (!FormulaUtil.ErrorCodeToName.TryGetValue(errorValue, out var value))
		{
			return "#N/A";
		}
		return value;
	}

	private void CreateRangesCollection()
	{
		m_colRanges = new SFTable(m_book.MaxRowCount, m_book.MaxColumnCount);
	}

	private void UpdateSheetReferences(FormulaRecord formula, IDictionary dicSheetNames, WorkbookImpl book)
	{
		if (formula == null)
		{
			throw new ArgumentNullException("formula");
		}
		if (book == null)
		{
			throw new ArgumentNullException("book");
		}
		Ptg[] parsedExpression = formula.ParsedExpression;
		int i = 0;
		for (int num = parsedExpression.Length; i < num; i++)
		{
			Ptg ptg = parsedExpression[i];
			if (ptg is ISheetReference)
			{
				ISheetReference obj = (ISheetReference)ptg;
				ushort refIndex = obj.RefIndex;
				string text = book.GetSheetNameByReference(refIndex);
				if (dicSheetNames != null && dicSheetNames.Contains(text))
				{
					text = (string)dicSheetNames[text];
				}
				int num2 = m_book.AddSheetReference(text);
				obj.RefIndex = (ushort)num2;
			}
		}
	}

	private void CopyStrings(IDictionary dicSourceStrings)
	{
	}

	private FormatImpl GetFormat(long cellIndex)
	{
		ICellPositionFormat cellRecord = GetCellRecord(cellIndex);
		if (cellRecord != null)
		{
			int extendedFormatIndex = cellRecord.ExtendedFormatIndex;
			int numberFormatIndex = m_book.InnerExtFormats[extendedFormatIndex].NumberFormatIndex;
			return m_book.InnerFormats[numberFormatIndex];
		}
		return null;
	}

	private IOfficeFont GetFont(long cellIndex)
	{
		ICellPositionFormat cellRecord = GetCellRecord(cellIndex);
		if (cellRecord != null)
		{
			int extendedFormatIndex = cellRecord.ExtendedFormatIndex;
			int fontIndex = m_book.InnerExtFormats[extendedFormatIndex].FontIndex;
			return m_book.InnerFonts[fontIndex];
		}
		return null;
	}

	private int GetXFIndex(int iOldIndex, IDictionary dicXFIndexes, OfficeCopyRangeOptions options)
	{
		if ((options & OfficeCopyRangeOptions.CopyStyles) == 0)
		{
			iOldIndex = m_book.DefaultXFIndex;
		}
		else if (dicXFIndexes != null && dicXFIndexes.Contains(iOldIndex))
		{
			iOldIndex = (int)dicXFIndexes[iOldIndex];
		}
		return iOldIndex;
	}

	internal void UpdateLabelSSTIndexes(Dictionary<int, int> dictUpdatedIndexes, IncreaseIndex method)
	{
		m_dicRecords.UpdateLabelSSTIndexes(dictUpdatedIndexes, method);
	}

	private void FillRichText(RichTextString richText, int sstIndex)
	{
		object obj = m_book.InnerSST[sstIndex];
		if (obj is TextWithFormat textWithFormat)
		{
			richText.SetTextObject(textWithFormat.TypedClone());
			return;
		}
		TextWithFormat textObject = richText.TextObject;
		if (textObject != null)
		{
			textObject.ClearFormatting();
			textObject.Text = obj as string;
		}
		else
		{
			richText.SetTextObject((TextWithFormat)obj);
		}
	}

	internal WorksheetImpl.TRangeValueType GetCellType(int row, int column)
	{
		return m_dicRecords.Rows[row - 1]?.GetCellType(column - 1, bNeedFormulaSubType: false) ?? WorksheetImpl.TRangeValueType.Blank;
	}

	protected override void OnDispose()
	{
		if (!m_bIsDisposed)
		{
			if (m_dicRecords != null)
			{
				m_dicRecords.Dispose();
				m_dicRecords = null;
			}
			if (m_colRanges != null)
			{
				m_colRanges.Clear();
				m_colRanges = null;
			}
			m_book = null;
			m_worksheet = null;
		}
		base.OnDispose();
	}

	public int FindRecord(TBIFFRecord recordType, int iRow, int iCol, int iLastCol)
	{
		RowStorage rowStorage = m_dicRecords.Rows[iRow - 1];
		if (rowStorage == null)
		{
			return iLastCol + 1;
		}
		return rowStorage.FindRecord(recordType, iCol - 1, iLastCol - 1) + 1;
	}
}
