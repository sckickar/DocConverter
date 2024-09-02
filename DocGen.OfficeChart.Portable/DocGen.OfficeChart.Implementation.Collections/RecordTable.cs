using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DocGen.Drawing;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Formula;

namespace DocGen.OfficeChart.Implementation.Collections;

internal class RecordTable : ICloneable, IDisposable
{
	private int m_iRowCount;

	private ArrayListEx m_arrRows = new ArrayListEx();

	private int m_iFirstRow = -1;

	private int m_iLastRow = -1;

	private bool m_bIsDisposed;

	private Dictionary<long, SharedFormulaRecord> m_arrShared;

	private WorkbookImpl m_book;

	private IInternalWorksheet m_sheet;

	public IApplication Application
	{
		[DebuggerStepThrough]
		get
		{
			return m_book.Application;
		}
	}

	public ApplicationImpl AppImplementation => m_book.AppImplementation;

	public ArrayListEx Rows => m_arrRows;

	public int RowCount
	{
		get
		{
			return m_iRowCount;
		}
		set
		{
			m_iRowCount = value;
			m_arrRows.ReduceSizeIfNecessary(value);
			m_iLastRow = Math.Min(m_iLastRow, value);
			m_iFirstRow = Math.Min(m_iFirstRow, value);
		}
	}

	public int FirstRow => m_iFirstRow;

	public int LastRow => m_iLastRow;

	public object this[int rowIndex, int colIndex]
	{
		get
		{
			if (rowIndex >= m_iRowCount || rowIndex < 0)
			{
				return null;
			}
			return Rows[rowIndex]?.GetRecord(colIndex, Application.RowStorageAllocationBlockSize);
		}
		set
		{
			if (rowIndex >= m_iRowCount || rowIndex < 0)
			{
				throw new ArgumentOutOfRangeException("rowIndex");
			}
			RowStorage orCreateRow = GetOrCreateRow(rowIndex, m_sheet.DefaultRowHeight, value != null, m_book.Version);
			if (orCreateRow != null)
			{
				orCreateRow.SetWorkbook(m_book, rowIndex);
				orCreateRow.SetRecord(colIndex, (ICellPositionFormat)value, Application.RowStorageAllocationBlockSize);
			}
		}
	}

	[CLSCompliant(false)]
	public Dictionary<long, SharedFormulaRecord> SharedFormulas
	{
		get
		{
			if (m_arrShared == null)
			{
				m_arrShared = new Dictionary<long, SharedFormulaRecord>();
			}
			return m_arrShared;
		}
	}

	private int SharedCount
	{
		get
		{
			if (m_arrShared == null)
			{
				return 0;
			}
			return m_arrShared.Count;
		}
	}

	public WorkbookImpl Workbook => m_book;

	public RecordTable(int iRowCount, IInternalWorksheet sheet)
	{
		m_sheet = sheet;
		m_book = sheet.ParentWorkbook;
		m_iRowCount = iRowCount;
	}

	protected RecordTable(RecordTable data, bool clone, IInternalWorksheet sheet)
	{
		m_iRowCount = data.m_iRowCount;
		m_sheet = sheet;
		m_book = sheet.ParentWorkbook;
		if (!clone)
		{
			return;
		}
		m_iFirstRow = data.m_iFirstRow;
		m_iLastRow = data.m_iLastRow;
		_ = data.m_arrRows;
		for (int i = 0; i < m_iRowCount; i++)
		{
			RowStorage rowStorage = data.Rows[i];
			if (rowStorage != null)
			{
				m_arrRows[i] = (RowStorage)rowStorage.Clone();
			}
		}
	}

	public void Dispose()
	{
		if (m_bIsDisposed)
		{
			return;
		}
		if (m_iFirstRow >= 0)
		{
			for (int i = m_iFirstRow; i <= m_iLastRow; i++)
			{
				m_arrRows[i]?.Dispose();
			}
		}
		m_iFirstRow = -1;
		m_iLastRow = -1;
		m_iRowCount = -1;
		m_arrRows = null;
		m_bIsDisposed = true;
		m_book = null;
		if (m_arrShared != null)
		{
			m_arrShared.Clear();
			m_arrShared = null;
		}
		GC.SuppressFinalize(this);
	}

	~RecordTable()
	{
		Dispose();
	}

	public virtual object Clone()
	{
		return new RecordTable(this, clone: true, m_sheet);
	}

	public virtual object Clone(IInternalWorksheet parentWorksheet)
	{
		return new RecordTable(this, clone: true, parentWorksheet);
	}

	public void Clear()
	{
		if (m_iFirstRow != -1)
		{
			for (int i = m_iFirstRow; i <= m_iLastRow; i++)
			{
				RowStorage rowStorage = m_arrRows[i];
				m_arrRows[i] = null;
				rowStorage?.Dispose();
			}
		}
		if (m_sheet != null)
		{
			m_sheet = null;
		}
		if (m_book != null)
		{
			m_book = null;
		}
	}

	internal void UpdateRows(int rowCount)
	{
		m_arrRows.UpdateSize(rowCount);
	}

	public virtual RowStorage CreateCellCollection(int iRowIndex, int height, OfficeVersion version)
	{
		RowStorage rowStorage = new RowStorage(iRowIndex, height, m_book.DefaultXFIndex);
		rowStorage.IsFormatted = false;
		switch (version)
		{
		case OfficeVersion.Excel97to2003:
			rowStorage.SetCellPositionSize(4, AppImplementation.RowStorageAllocationBlockSize, version);
			break;
		case OfficeVersion.Excel2007:
		case OfficeVersion.Excel2010:
		case OfficeVersion.Excel2013:
			rowStorage.SetCellPositionSize(8, AppImplementation.RowStorageAllocationBlockSize, version);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		return rowStorage;
	}

	public bool Contains(int rowIndex, int colIndex)
	{
		if (rowIndex < 0 || rowIndex >= m_iRowCount)
		{
			return false;
		}
		if (m_arrRows == null)
		{
			return false;
		}
		return m_arrRows[rowIndex]?.Contains(colIndex) ?? false;
	}

	[CLSCompliant(false)]
	public ArrayRecord GetArrayRecord(ICellPositionFormat cell)
	{
		if (cell == null)
		{
			throw new ArgumentNullException("cell");
		}
		if (cell.TypeCode != TBIFFRecord.Formula)
		{
			return null;
		}
		Ptg[] parsedExpression = ((FormulaRecord)cell).ParsedExpression;
		if (parsedExpression == null || parsedExpression.Length != 1)
		{
			return null;
		}
		if (!(parsedExpression[0] is ControlPtg { RowIndex: var rowIndex, ColumnIndex: var columnIndex }))
		{
			return null;
		}
		return Rows[rowIndex]?.GetArrayRecord(columnIndex);
	}

	public void AccessRow(int iRowIndex)
	{
		if (m_iFirstRow < 0)
		{
			m_iFirstRow = iRowIndex;
			m_iLastRow = iRowIndex;
		}
		else
		{
			m_iFirstRow = Math.Min(m_iFirstRow, iRowIndex);
			m_iLastRow = Math.Max(m_iLastRow, iRowIndex);
		}
	}

	public void SetRow(int iRowIndex, RowStorage row)
	{
		Rows[iRowIndex] = row;
		AccessRow(iRowIndex);
	}

	public void UpdateFormula(int iCurIndex, int iSourceIndex, Rectangle sourceRect, int iDestIndex, Rectangle destRect)
	{
		if (m_iFirstRow >= 0 && m_arrRows != null)
		{
			int rowStorageAllocationBlockSize = Application.RowStorageAllocationBlockSize;
			for (int i = m_iFirstRow; i <= m_iLastRow; i++)
			{
				m_arrRows[i]?.UpdateFormula(iCurIndex, iSourceIndex, sourceRect, iDestIndex, destRect, rowStorageAllocationBlockSize, m_book);
			}
		}
	}

	public void RemoveLastColumn(int iColumnIndex)
	{
		if (m_arrRows != null)
		{
			int rowStorageAllocationBlockSize = m_book.Application.RowStorageAllocationBlockSize;
			for (int i = m_iFirstRow; i <= m_iLastRow; i++)
			{
				m_arrRows[i]?.Remove(iColumnIndex, iColumnIndex + 1, rowStorageAllocationBlockSize);
			}
		}
	}

	public void RemoveRow(int iRowIndex)
	{
		if (m_arrRows == null)
		{
			return;
		}
		m_arrRows[iRowIndex]?.Dispose();
		SetRow(iRowIndex, null);
		bool flag = false;
		for (int i = m_iFirstRow; i <= m_iLastRow; i++)
		{
			if (m_arrRows[i] != null)
			{
				flag = true;
				m_iFirstRow = i;
				break;
			}
		}
		for (int num = m_iLastRow; num >= m_iFirstRow; num--)
		{
			if (m_arrRows[num] != null)
			{
				flag = true;
				m_iLastRow = num;
				break;
			}
		}
		if (!flag)
		{
			m_iFirstRow = -1;
			m_iLastRow = -1;
		}
	}

	public void UpdateNameIndexes(WorkbookImpl book, int[] arrNewIndex)
	{
		if (arrNewIndex == null)
		{
			throw new ArgumentNullException("arrNewIndex");
		}
		if (m_iFirstRow >= 0)
		{
			int rowStorageAllocationBlockSize = Application.RowStorageAllocationBlockSize;
			for (int i = m_iFirstRow; i <= m_iLastRow; i++)
			{
				m_arrRows[i]?.UpdateNameIndexes(book, arrNewIndex, rowStorageAllocationBlockSize);
			}
		}
	}

	public void UpdateNameIndexes(WorkbookImpl book, IDictionary<int, int> dicNewIndex)
	{
		if (dicNewIndex == null)
		{
			throw new ArgumentNullException("dicNewIndex");
		}
		if (m_iFirstRow >= 0)
		{
			int rowStorageAllocationBlockSize = Application.RowStorageAllocationBlockSize;
			for (int i = m_iFirstRow; i <= m_iLastRow; i++)
			{
				m_arrRows[i]?.UpdateNameIndexes(book, dicNewIndex, rowStorageAllocationBlockSize);
			}
		}
	}

	public void ReplaceSharedFormula(WorkbookImpl book)
	{
		if (SharedCount > 0)
		{
			foreach (KeyValuePair<long, SharedFormulaRecord> sharedFormula in SharedFormulas)
			{
				long key = sharedFormula.Key;
				SharedFormulaRecord value = sharedFormula.Value;
				int rowFromCellIndex = RangeImpl.GetRowFromCellIndex(key);
				int columnFromCellIndex = RangeImpl.GetColumnFromCellIndex(key);
				ReplaceSharedFormula(book, rowFromCellIndex, columnFromCellIndex, value);
			}
		}
		m_arrShared = null;
	}

	[CLSCompliant(false)]
	public void ReplaceSharedFormula(WorkbookImpl book, int row, int column, SharedFormulaRecord shared)
	{
		int i = shared.FirstRow;
		for (int lastRow = shared.LastRow; i <= lastRow; i++)
		{
			m_arrRows[i]?.ReplaceSharedFormula(book, row, column, shared);
		}
	}

	public void UpdateStringIndexes(List<int> arrNewIndexes)
	{
		if (arrNewIndexes == null)
		{
			throw new ArgumentNullException("arrNewIndexes");
		}
		if (m_iFirstRow >= 0)
		{
			for (int i = m_iFirstRow; i <= m_iLastRow; i++)
			{
				m_arrRows[i]?.UpdateStringIndexes(arrNewIndexes);
			}
		}
	}

	public void CopyCells(RecordTable sourceCells, SSTDictionary sourceSST, SSTDictionary destSST, Dictionary<int, int> hashExtFormatIndexes, Dictionary<string, string> hashWorksheetNames, Dictionary<int, int> dicNameIndexes, Dictionary<int, int> dicFontIndexes, Dictionary<int, int> dictExternSheet)
	{
		m_iFirstRow = sourceCells.m_iFirstRow;
		m_iLastRow = sourceCells.m_iLastRow;
		m_arrRows = new ArrayListEx();
		if (sourceCells.m_iFirstRow < 0)
		{
			return;
		}
		for (int i = m_iFirstRow; i <= m_iLastRow; i++)
		{
			RowStorage rowStorage = sourceCells.m_arrRows[i];
			if (rowStorage != null)
			{
				RowStorage row = rowStorage.Clone(sourceSST, destSST, hashExtFormatIndexes, hashWorksheetNames, dicNameIndexes, dicFontIndexes, dictExternSheet);
				SetRow(i, row);
			}
		}
	}

	public List<long> Find(IRange range, string findValue, OfficeFindType flags, bool bIsFindFirst, WorkbookImpl book)
	{
		return Find(range, findValue, flags, OfficeFindOptions.None, bIsFindFirst, book);
	}

	public List<long> Find(IRange range, string findValue, OfficeFindType flags, OfficeFindOptions findOptions, bool bIsFindFirst, WorkbookImpl book)
	{
		if (range == null)
		{
			throw new ArgumentNullException("range");
		}
		if (findValue == null || findValue.Length == 0)
		{
			return null;
		}
		bool flag = FormulaUtil.ErrorNameToCode.ContainsKey(findValue);
		bool num = (flags & OfficeFindType.Text) == OfficeFindType.Text;
		bool flag2 = (flags & OfficeFindType.Error) == OfficeFindType.Error;
		bool flag3 = (flags & OfficeFindType.Formula) == OfficeFindType.Formula;
		bool flag4 = (flags & OfficeFindType.FormulaStringValue) == OfficeFindType.FormulaStringValue;
		if (!(num || flag3 || flag2 || flag4))
		{
			throw new ArgumentException("Parameter flags is not valid.", "flags");
		}
		int iErrorCode = 0;
		flag2 = flag2 && flag;
		List<long> list = new List<long>();
		if (flag2)
		{
			iErrorCode = FormulaUtil.ErrorNameToCode[findValue];
		}
		int iFirstColumn = range.Column - 1;
		int iLastColumn = range.LastColumn - 1;
		if (m_arrRows != null)
		{
			int i = range.Row - 1;
			for (int num2 = range.LastRow - 1; i <= num2; i++)
			{
				RowStorage rowStorage = m_arrRows[i];
				if (rowStorage != null)
				{
					list.AddRange(rowStorage.Find(iFirstColumn, iLastColumn, findValue, flags, findOptions, iErrorCode, bIsFindFirst, book));
				}
			}
		}
		book.IsStartsOrEndsWith = null;
		return list;
	}

	public List<long> Find(IRange range, double findValue, OfficeFindType flags, bool bIsFindFirst, WorkbookImpl book)
	{
		if (range == null)
		{
			throw new ArgumentNullException("range");
		}
		bool num = (flags & OfficeFindType.FormulaValue) != 0;
		bool flag = (flags & OfficeFindType.Number) != 0;
		if (!(num || flag))
		{
			throw new ArgumentException("Parameter flags is not valid.", "flags");
		}
		List<long> list = new List<long>();
		int iFirstColumn = range.Column - 1;
		int iLastColumn = range.LastColumn - 1;
		if (m_arrRows != null)
		{
			int i = range.Row - 1;
			for (int num2 = range.LastRow - 1; i <= num2; i++)
			{
				RowStorage rowStorage = m_arrRows[i];
				if (rowStorage != null)
				{
					list.AddRange(rowStorage.Find(iFirstColumn, iLastColumn, findValue, flags, bIsFindFirst, book));
					if (bIsFindFirst && list.Count > 0)
					{
						break;
					}
				}
			}
		}
		return list;
	}

	public List<long> Find(IRange range, byte findValue, bool bErrorCode, bool bIsFindFirst, WorkbookImpl book)
	{
		if (range == null)
		{
			throw new ArgumentNullException("range");
		}
		List<long> list = new List<long>();
		int iFirstColumn = range.Column - 1;
		int iLastColumn = range.LastColumn - 1;
		if (m_arrRows != null)
		{
			int i = range.Row - 1;
			for (int num = range.LastRow - 1; i <= num; i++)
			{
				RowStorage rowStorage = m_arrRows[i];
				if (rowStorage != null)
				{
					list.AddRange(rowStorage.Find(iFirstColumn, iLastColumn, findValue, bErrorCode, bIsFindFirst, book));
				}
			}
		}
		return list;
	}

	public int GetMinimumColumnIndex(int iStartRow, int iEndRow)
	{
		bool flag = false;
		int num = int.MaxValue;
		if (m_arrRows == null)
		{
			return -1;
		}
		for (int i = iStartRow; i <= iEndRow; i++)
		{
			RowStorage rowStorage = m_arrRows[i];
			if (rowStorage != null && rowStorage.UsedSize > 0)
			{
				num = Math.Min(num, rowStorage.FirstColumn);
				flag = true;
			}
		}
		if (!flag)
		{
			return -1;
		}
		return num;
	}

	public int GetMaximumColumnIndex(int iStartRow, int iEndRow)
	{
		bool flag = false;
		int num = int.MinValue;
		if (m_arrRows == null)
		{
			return -1;
		}
		for (int i = iStartRow; i <= iEndRow; i++)
		{
			RowStorage rowStorage = m_arrRows[i];
			if (rowStorage != null && rowStorage.UsedSize > 0)
			{
				num = Math.Max(num, rowStorage.LastColumn);
				flag = true;
			}
		}
		if (!flag)
		{
			return -1;
		}
		return num;
	}

	public bool ContainsRow(int iRowIndex)
	{
		if (m_arrRows != null && iRowIndex >= m_iFirstRow && iRowIndex <= m_iLastRow)
		{
			return m_arrRows[iRowIndex] != null;
		}
		return false;
	}

	public List<long> Find(Dictionary<int, object> dictIndexes)
	{
		List<long> list = new List<long>();
		if (dictIndexes == null || dictIndexes.Count == 0 || m_arrRows == null)
		{
			return list;
		}
		for (int i = m_iFirstRow; i <= m_iLastRow; i++)
		{
			m_arrRows[i]?.Find(dictIndexes, list);
		}
		return list;
	}

	public RecordTable CacheAndRemove(Rectangle rectSource, int iDeltaRow, int iDeltaColumn, ref int iMaxRow, ref int iMaxColumn)
	{
		RecordTable recordTable = new RecordTable(m_iRowCount, m_sheet);
		int num = Math.Max(rectSource.Y, m_iFirstRow);
		int num2 = Math.Min(rectSource.Bottom, m_iLastRow);
		int x = rectSource.X;
		int right = rectSource.Right;
		int rowStorageAllocationBlockSize = Application.RowStorageAllocationBlockSize;
		for (int i = num; i <= num2; i++)
		{
			RowStorage rowStorage = m_arrRows[i];
			if (rowStorage != null)
			{
				RowStorage rowStorage2 = rowStorage.Clone(x, right, Application.RowStorageAllocationBlockSize);
				if (rowStorage2 != null)
				{
					rowStorage2.RowColumnOffset(iDeltaRow, iDeltaColumn, rowStorageAllocationBlockSize);
					rowStorage.Remove(x, right, rowStorageAllocationBlockSize);
					iMaxRow = Math.Max(iMaxRow, i + iDeltaRow);
					iMaxColumn = Math.Max(iMaxColumn, rowStorage2.LastColumn);
				}
				recordTable.SetRow(i + iDeltaRow, rowStorage2);
			}
		}
		recordTable.m_iFirstRow = rectSource.Y + iDeltaRow;
		recordTable.m_iLastRow = rectSource.Bottom + iDeltaRow;
		return recordTable;
	}

	public void UpdateExtendedFormatIndex(Dictionary<int, int> dictFormats)
	{
		if (m_arrRows != null && m_iFirstRow >= 0)
		{
			for (int i = m_iFirstRow; i <= m_iLastRow; i++)
			{
				m_arrRows[i]?.UpdateExtendedFormatIndex(dictFormats, Application.RowStorageAllocationBlockSize);
			}
		}
	}

	public void UpdateExtendedFormatIndex(int[] arrFormats)
	{
		if (m_arrRows != null && m_iFirstRow >= 0)
		{
			for (int i = m_iFirstRow; i <= m_iLastRow; i++)
			{
				m_arrRows[i]?.UpdateExtendedFormatIndex(arrFormats, Application.RowStorageAllocationBlockSize);
			}
		}
	}

	public void UpdateExtendedFormatIndex(int maxCount)
	{
		if (m_arrRows != null && m_iFirstRow >= 0)
		{
			int defaultXFIndex = m_book.DefaultXFIndex;
			for (int i = m_iFirstRow; i <= m_iLastRow; i++)
			{
				m_arrRows[i]?.UpdateExtendedFormatIndex(maxCount, defaultXFIndex);
			}
		}
	}

	internal void UpdateLabelSSTIndexes(Dictionary<int, int> dictUpdatedIndexes, IncreaseIndex method)
	{
		if (m_arrRows != null)
		{
			for (int i = m_iFirstRow; i <= m_iLastRow; i++)
			{
				m_arrRows[i]?.UpdateLabelSSTIndexes(dictUpdatedIndexes, method);
			}
		}
	}

	public void AddSharedFormula(int row, int column, SharedFormulaRecord shared)
	{
		long cellIndex = RangeImpl.GetCellIndex(column, row);
		SharedFormulas.Add(cellIndex, shared);
	}

	[CLSCompliant(false)]
	public bool ExtractRangesFast(IndexRecord index, BiffReader reader, bool bIgnoreStyles, SSTDictionary sst, WorksheetImpl sheet)
	{
		if (index == null)
		{
			return false;
		}
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		Stream baseStream = reader.BaseStream;
		long position = baseStream.Position;
		_ = reader.BaseReader;
		DataProvider dataProvider = reader.DataProvider;
		byte[] buffer = reader.Buffer;
		DBCellRecord dBCellRecord = (DBCellRecord)BiffRecordFactory.GetRecord(TBIFFRecord.DBCell);
		bool flag = true;
		int[] dbCells = index.DbCells;
		int i = 0;
		for (int num = dbCells.Length; i < num && flag; i++)
		{
			int num2 = dbCells[i];
			baseStream.Position = num2;
			baseStream.Read(buffer, 0, 4);
			short num3 = BitConverter.ToInt16(buffer, 0);
			int num4 = BitConverter.ToInt16(buffer, 2);
			if (num3 != 215)
			{
				flag = false;
				break;
			}
			baseStream.Read(buffer, 4, num4);
			dBCellRecord.Length = num4;
			dBCellRecord.ParseStructure(dataProvider, 4, num4, OfficeVersion.Excel97to2003);
			dBCellRecord.StreamPos = num2;
			flag = ParseDBCellRecord(dBCellRecord, reader, bIgnoreStyles, sst, sheet);
		}
		if (!flag)
		{
			baseStream.Position = position;
			Clear();
		}
		return flag;
	}

	public RowStorage GetOrCreateRow(int rowIndex, int height, bool bCreate, OfficeVersion version)
	{
		RowStorage rowStorage = Rows[rowIndex];
		if (rowStorage == null && bCreate)
		{
			rowStorage = (m_arrRows[rowIndex] = CreateCellCollection(rowIndex, height, version));
			AccessRow(rowIndex);
			WorksheetHelper.AccessRow(m_sheet, rowIndex + 1);
		}
		if (rowStorage != null && bCreate && rowStorage.Provider == null)
		{
			rowStorage.CreateDataProvider();
		}
		return rowStorage;
	}

	public void EnsureSize(int iSize)
	{
		m_arrRows.UpdateSize(iSize);
	}

	public void InsertIntoDefaultRows(int iRowIndex, int iRowCount)
	{
		if (iRowIndex <= m_iLastRow)
		{
			EnsureSize(m_iLastRow + iRowCount + 1);
			m_arrRows.Insert(iRowIndex, iRowCount, m_iLastRow - iRowIndex + 1);
			m_iLastRow += iRowCount;
		}
	}

	private bool ParseDBCellRecord(DBCellRecord dbCell, BiffReader reader, bool bIgnoreStyles, SSTDictionary sst, WorksheetImpl sheet)
	{
		if (dbCell == null)
		{
			throw new ArgumentNullException("dbCell");
		}
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		RowRecord row = (RowRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Row);
		ushort[] cellOffsets = dbCell.CellOffsets;
		int num = cellOffsets.Length;
		if (num == 0)
		{
			return true;
		}
		long num2 = dbCell.StreamPos - dbCell.RowOffset;
		byte[] buffer = reader.Buffer;
		DataProvider dataProvider = reader.DataProvider;
		_ = buffer.Length;
		Stream baseStream = reader.BaseStream;
		long num3 = num2 + cellOffsets[0] + 16 + 4;
		bool flag = true;
		int i = 1;
		for (int num4 = num; i < num4 && flag; i++)
		{
			baseStream.Position = num2;
			int num5 = FillRowRecord(row, baseStream, buffer, dataProvider);
			if (num5 < 0)
			{
				return false;
			}
			num2 += num5 + 4;
			sheet.ParseRowRecord(row, bIgnoreStyles);
			baseStream.Position = num3;
			int num6 = cellOffsets[i];
			num3 += num6;
			flag &= ReadStorageData(row, baseStream, num6, buffer, sst);
		}
		if (flag)
		{
			long num7 = ((num > 1) ? baseStream.Position : (num2 + 16 + 4));
			int num6 = (int)(dbCell.StreamPos - num7);
			baseStream.Position = num2;
			int num5 = FillRowRecord(row, baseStream, buffer, dataProvider);
			flag = flag && num5 > 0;
			if (flag)
			{
				sheet.ParseRowRecord(row, bIgnoreStyles);
				baseStream.Position = num3;
				flag &= ReadStorageData(row, baseStream, num6, buffer, sst);
			}
		}
		return flag;
	}

	private int FillRowRecord(RowRecord row, Stream stream, byte[] arrBuffer, DataProvider provider)
	{
		stream.Read(arrBuffer, 0, 20);
		short num = BitConverter.ToInt16(arrBuffer, 0);
		int num2 = BitConverter.ToInt16(arrBuffer, 2);
		if (num != 520)
		{
			return -1;
		}
		if (16 < num2)
		{
			stream.Read(arrBuffer, 20, num2 - 16);
		}
		row.ParseStructure(provider, 4, num2, OfficeVersion.Excel97to2003);
		return num2;
	}

	private bool ReadStorageData(RowRecord row, Stream stream, int iDataSize, byte[] arrBuffer, SSTDictionary sst)
	{
		bool result = true;
		if (iDataSize > 0)
		{
			RowStorage orCreateRow = GetOrCreateRow(row.RowNumber, 0, bCreate: true, OfficeVersion.Excel97to2003);
			orCreateRow.UpdateRowInfo(row, Application.UseFastRecordParsing);
			int val = arrBuffer.Length;
			while (iDataSize > 0)
			{
				int num = Math.Min(iDataSize, val);
				stream.Read(arrBuffer, 0, num);
				orCreateRow.AppendRecordData(num, arrBuffer, Application.RowStorageAllocationBlockSize);
				iDataSize -= num;
			}
			result = orCreateRow.PrepareRowData(sst, ref m_arrShared);
		}
		return result;
	}

	public void UpdateFormulaFlags()
	{
		for (int i = m_iFirstRow; i <= m_iLastRow; i++)
		{
			m_arrRows[i]?.UpdateFormulaFlags();
		}
	}

	public int GetBoolValue(int iRow, int iCol)
	{
		return m_arrRows[iRow - 1]?.GetBoolValue(iCol - 1) ?? 0;
	}

	public int GetFormulaBoolValue(int iRow, int iCol)
	{
		return m_arrRows[iRow - 1]?.GetFormulaBoolValue(iCol - 1) ?? 0;
	}

	public string GetErrorValue(int iRow, int iCol)
	{
		return Rows[iRow - 1]?.GetErrorValue(iCol - 1);
	}

	internal string GetErrorValue(byte value, int iRow)
	{
		return Rows[iRow - 1]?.GetErrorString(value & 0xFF);
	}

	public string GetFormulaErrorValue(int iRow, int iCol)
	{
		return Rows[iRow - 1]?.GetFormulaErrorValue(iCol - 1);
	}

	public double GetNumberValue(int iRow, int iCol)
	{
		RowStorage rowStorage = m_arrRows[iRow - 1];
		if (rowStorage == null)
		{
			return double.NaN;
		}
		rowStorage.SetWorkbook(m_book, iRow);
		return rowStorage.GetNumberValue(iCol - 1, m_sheet.Index);
	}

	public double GetFormulaNumberValue(int iRow, int iCol)
	{
		return m_arrRows[iRow - 1]?.GetFormulaNumberValue(iCol - 1) ?? double.NaN;
	}

	public string GetStringValue(int iRow, int iCol, SSTDictionary sst)
	{
		return m_arrRows[iRow - 1]?.GetStringValue(iCol - 1, sst);
	}

	public string GetFormulaStringValue(int iRow, int iCol, SSTDictionary sst)
	{
		return m_arrRows[iRow - 1]?.GetFormulaStringValue(iCol - 1);
	}

	public Ptg[] GetFormulaValue(int iRow, int iCol)
	{
		return m_arrRows[iRow - 1]?.GetFormulaValue(iCol - 1);
	}

	public bool HasFormulaRecord(int iRow, int iCol)
	{
		return Rows[iRow - 1]?.HasFormulaRecord(iCol - 1) ?? false;
	}

	public bool HasFormulaArrayRecord(int iRow, int iCol)
	{
		return Rows[iRow]?.HasFormulaArrayRecord(iCol) ?? false;
	}

	public WorksheetImpl.TRangeValueType GetCellType(int row, int column, bool bNeedFormulaSubType)
	{
		return Rows[row - 1]?.GetCellType(column - 1, bNeedFormulaSubType) ?? WorksheetImpl.TRangeValueType.Blank;
	}

	[CLSCompliant(false)]
	public void SetFormulaValue(int iRow, int iColumn, double value, StringRecord strRecord)
	{
		(m_arrRows[iRow - 1] ?? throw new ApplicationException("Cannot sets formula value.")).SetFormulaValue(iColumn - 1, value, strRecord, Application.RowStorageAllocationBlockSize);
	}

	public void MarkUsedReferences(bool[] usedItems)
	{
		for (int i = m_iFirstRow; i <= m_iLastRow; i++)
		{
			m_arrRows[i]?.MarkUsedReferences(usedItems);
		}
	}

	public void UpdateReferenceIndexes(int[] arrUpdatedIndexes)
	{
		for (int i = m_iFirstRow; i <= m_iLastRow; i++)
		{
			m_arrRows[i]?.UpdateReferenceIndexes(arrUpdatedIndexes);
		}
	}
}
