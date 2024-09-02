using System;
using System.Collections;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation;

internal class MergeCellsImpl : CommonObject, ICloneParent
{
	private WorksheetImpl m_sheet;

	private List<MergeCellsRecord> m_arrRecordsToParse = new List<MergeCellsRecord>();

	private bool m_bParsed = true;

	private List<Rectangle> m_arrCells = new List<Rectangle>();

	internal int MergeCount
	{
		get
		{
			Parse();
			return m_arrCells.Count;
		}
	}

	internal List<Rectangle> MergedRegions
	{
		get
		{
			Parse();
			return m_arrCells;
		}
	}

	internal MergeCellsRecord.MergedRegion this[Rectangle rect] => FindMergedRegion(rect);

	internal Rectangle this[int index]
	{
		get
		{
			Parse();
			return m_arrCells[index];
		}
	}

	public MergeCellsImpl(IApplication application, object parent)
		: base(application, parent)
	{
		FindParents();
	}

	[CLSCompliant(false)]
	public void Serialize(OffsetArrayList records)
	{
		if (m_bParsed)
		{
			int mergeCount = MergeCount;
			if (mergeCount < 0)
			{
				return;
			}
			MergeCellsRecord.MergedRegion[] array = new MergeCellsRecord.MergedRegion[mergeCount];
			for (int i = 0; i < mergeCount; i++)
			{
				Rectangle rect = m_arrCells[i];
				MergeCellsRecord.MergedRegion mergedRegion = RectangleToMergeRegion(rect);
				array[i] = mergedRegion;
			}
			int num;
			for (int j = 0; j != mergeCount; j += num)
			{
				num = mergeCount - j;
				if (num > 1027)
				{
					num = 1027;
				}
				MergeCellsRecord mergeCellsRecord = (MergeCellsRecord)BiffRecordFactory.GetRecord(TBIFFRecord.MergeCells);
				mergeCellsRecord.SetRegions(j, num, array);
				records.Add(mergeCellsRecord);
			}
		}
		else
		{
			records.AddRange(m_arrRecordsToParse);
		}
	}

	private void FindParents()
	{
		object obj = FindParent(typeof(WorksheetImpl));
		if (obj == null)
		{
			throw new ArgumentNullException("Can't find parent workbook");
		}
		m_sheet = (WorksheetImpl)obj;
	}

	public IList<ExtendedFormatImpl> GetMergedExtendedFormats()
	{
		Parse();
		IList<ExtendedFormatImpl> list = new List<ExtendedFormatImpl>();
		_ = m_sheet.CellRecords;
		int i = 0;
		for (int mergeCount = MergeCount; i < mergeCount; i++)
		{
			Rectangle rect = m_arrCells[i];
			MergeCellsRecord.MergedRegion region = RectangleToMergeRegion(rect);
			ExtendedFormatImpl format = GetFormat(region);
			list.Add(format);
		}
		return list;
	}

	[CLSCompliant(false)]
	public ExtendedFormatImpl GetFormat(MergeCellsRecord.MergedRegion region)
	{
		WorkbookImpl workbookImpl = (WorkbookImpl)m_sheet.Workbook;
		CellRecordCollection cellRecords = m_sheet.CellRecords;
		int num = cellRecords.GetExtendedFormatIndex(region.RowFrom + 1, region.ColumnFrom + 1);
		int num2 = cellRecords.GetExtendedFormatIndex(region.RowTo + 1, region.ColumnTo + 1);
		if (num < 0)
		{
			num = workbookImpl.DefaultXFIndex;
		}
		if (num2 < 0)
		{
			num2 = workbookImpl.DefaultXFIndex;
		}
		return workbookImpl.InnerExtFormats.GatherTwoFormats(num, num2);
	}

	public void AddMerge(RangeImpl range, OfficeMergeOperation operation)
	{
		int rowFrom = range.FirstRow - 1;
		int colFrom = range.FirstColumn - 1;
		int rowTo = range.LastRow - 1;
		int colTo = range.LastColumn - 1;
		Parse();
		AddMerge(rowFrom, rowTo, colFrom, colTo, operation);
	}

	private void AddMerge(MergeCellsRecord.MergedRegion region, OfficeMergeOperation operation)
	{
		int rowFrom = region.RowFrom;
		int columnFrom = region.ColumnFrom;
		int rowTo = region.RowTo;
		int columnTo = region.ColumnTo;
		AddMerge(rowFrom, rowTo, columnFrom, columnTo, operation);
	}

	public void AddMerge(int RowFrom, int RowTo, int ColFrom, int ColTo, OfficeMergeOperation operation)
	{
		Rectangle rectangle = new Rectangle(ColFrom, RowFrom, ColTo - ColFrom, RowTo - RowFrom);
		if (operation == OfficeMergeOperation.Delete)
		{
			DeleteMerge(rectangle);
		}
		m_arrCells.Add(rectangle);
	}

	public void DeleteMerge(Rectangle range)
	{
		Parse();
		List<Rectangle> list = new List<Rectangle>();
		int i = 0;
		for (int mergeCount = MergeCount; i < mergeCount; i++)
		{
			Rectangle rectangle = m_arrCells[i];
			if (UtilityMethods.Intersects(rectangle, range))
			{
				list.Add(rectangle);
			}
		}
		int j = 0;
		for (int count = list.Count; j < count; j++)
		{
			m_arrCells.Remove(list[j]);
		}
	}

	public void Clear()
	{
		m_arrCells.Clear();
		m_arrRecordsToParse = null;
		m_bParsed = true;
	}

	[CLSCompliant(false)]
	public void AddMerge(MergeCellsRecord mergeRecord)
	{
		m_arrRecordsToParse.Add(mergeRecord);
		m_bParsed = false;
	}

	public void Parse()
	{
		if (m_bParsed)
		{
			return;
		}
		int i = 0;
		for (int count = m_arrRecordsToParse.Count; i < count; i++)
		{
			MergeCellsRecord.MergedRegion[] regions = m_arrRecordsToParse[i].Regions;
			int j = 0;
			for (int num = regions.Length; j < num; j++)
			{
				AddMerge(regions[j], OfficeMergeOperation.Leave);
			}
		}
		m_bParsed = true;
		m_arrRecordsToParse = null;
	}

	public void RemoveRow(int iRowIndex)
	{
		if (iRowIndex < 1 || iRowIndex > m_sheet.Workbook.MaxRowCount)
		{
			throw new ArgumentOutOfRangeException("iRowIndex");
		}
		InsertRemoveRow(iRowIndex, isRemove: true, 1);
	}

	public void RemoveRow(int rowIndex, int count)
	{
		if (rowIndex < 1 || rowIndex > m_sheet.Workbook.MaxRowCount)
		{
			throw new ArgumentOutOfRangeException("rowIndex");
		}
		InsertRemoveRow(rowIndex, isRemove: true, count);
	}

	public void InsertRow(int iRowIndex, int iRowCount)
	{
		if (iRowIndex < 1 || iRowIndex > m_sheet.Workbook.MaxRowCount)
		{
			throw new ArgumentOutOfRangeException("iRowIndex");
		}
		InsertRemoveRow(iRowIndex, isRemove: false, iRowCount);
	}

	public void RemoveColumn(int iColumnIndex)
	{
		InsertRemoveColumn(iColumnIndex, isRemove: true, 1);
	}

	public void RemoveColumn(int index, int count)
	{
		InsertRemoveColumn(index, isRemove: true, count);
	}

	public void InsertColumn(int iColumnIndex)
	{
		InsertRemoveColumn(iColumnIndex, isRemove: false, 1);
	}

	public void InsertColumn(int iColumnIndex, int iColumnCount)
	{
		InsertRemoveColumn(iColumnIndex, isRemove: false, iColumnCount);
	}

	protected void InsertRemoveRow(int iRowIndex, bool isRemove, int iRowCount)
	{
		iRowIndex--;
		if (iRowIndex < 0 || iRowIndex > m_sheet.Workbook.MaxRowCount)
		{
			throw new ArgumentOutOfRangeException("iRowIndex");
		}
		Parse();
		List<Rectangle> arrCells = m_arrCells;
		m_arrCells = new List<Rectangle>();
		int i = 0;
		for (int count = arrCells.Count; i < count; i++)
		{
			Rectangle rect = arrCells[i];
			MergeCellsRecord.MergedRegion region = RectangleToMergeRegion(rect);
			region = InsertRemoveRow(region, iRowIndex, isRemove, iRowCount, m_sheet.Workbook);
			if (region == null)
			{
				continue;
			}
			AddMerge(region, OfficeMergeOperation.Delete);
			if (!isRemove && region.RowFrom + 1 == iRowIndex)
			{
				for (int j = 1; j <= iRowCount; j++)
				{
					region.RowFrom++;
					region.RowTo++;
					AddMerge(region, OfficeMergeOperation.Delete);
				}
			}
		}
	}

	protected void InsertRemoveColumn(int iColumnIndex, bool isRemove, int iCount)
	{
		iColumnIndex--;
		if (iColumnIndex < 0 || iColumnIndex >= m_sheet.Workbook.MaxColumnCount)
		{
			throw new ArgumentOutOfRangeException("iColumnIndex");
		}
		if (iCount < 0 || iCount >= m_sheet.Workbook.MaxColumnCount)
		{
			throw new ArgumentOutOfRangeException("iCount");
		}
		Parse();
		List<Rectangle> arrCells = m_arrCells;
		m_arrCells = new List<Rectangle>();
		int i = 0;
		for (int count = arrCells.Count; i < count; i++)
		{
			Rectangle rect = arrCells[i];
			MergeCellsRecord.MergedRegion region = RectangleToMergeRegion(rect);
			region = InsertRemoveColumn(region, iColumnIndex, isRemove, iCount, m_sheet.Workbook);
			if (region != null)
			{
				AddMerge(region, OfficeMergeOperation.Delete);
			}
		}
	}

	public void CopyMoveMerges(IRange destination, IRange source, bool bIsMove)
	{
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		Parse();
		int iRowDelta = destination.Row - source.Row;
		int iColDelta = destination.Column - source.Column;
		List<MergeCellsRecord.MergedRegion> list = new List<MergeCellsRecord.MergedRegion>();
		CacheMerges(source, list);
		if (bIsMove)
		{
			RemoveCache(list);
		}
		if (list.Count == 0)
		{
			MergeCellsImpl mergeCells = (destination as RangeImpl).InnerWorksheet.MergeCells;
			Rectangle range = Rectangle.FromLTRB(destination.Column - 1, destination.Row - 1, destination.LastColumn - 1, destination.LastRow - 1);
			mergeCells.DeleteMerge(range);
		}
		((WorksheetImpl)source.Worksheet).MergeCells.AddCache(list, iRowDelta, iColDelta);
	}

	[CLSCompliant(false)]
	public List<MergeCellsRecord.MergedRegion> FindMergesToCopyMove(IRange range, bool bIsMove)
	{
		List<MergeCellsRecord.MergedRegion> list = new List<MergeCellsRecord.MergedRegion>();
		CacheMerges(range, list);
		if (bIsMove)
		{
			RemoveCache(list);
		}
		return list;
	}

	[CLSCompliant(false)]
	public void CacheMerges(IRange range, List<MergeCellsRecord.MergedRegion> lstRegions)
	{
		if (range == null)
		{
			throw new ArgumentNullException("range");
		}
		if (lstRegions == null)
		{
			throw new ArgumentNullException("lstRegions");
		}
		Parse();
		int num = range.Row - 1;
		int num2 = range.Column - 1;
		int num3 = range.LastRow - 1;
		int num4 = range.LastColumn - 1;
		for (int i = num; i <= num3; i++)
		{
			for (int j = num2; j <= num4; j++)
			{
				Rectangle rectangle = new Rectangle(j, i, 0, 0);
				MergeCellsRecord.MergedRegion mergedRegion = FindMergedRegion(rectangle);
				if (mergedRegion != null && !lstRegions.Contains(mergedRegion))
				{
					lstRegions.Add(mergedRegion);
				}
			}
		}
	}

	private static void CheckRegion(MergeCellsRecord.MergedRegion region, IRange range)
	{
		if (region == null)
		{
			throw new ArgumentNullException("region");
		}
		if (range == null)
		{
			throw new ArgumentNullException("range");
		}
		int row = range.Row;
		int column = range.Column;
		int lastRow = range.LastRow;
		int lastColumn = range.LastColumn;
		if (region.ColumnFrom < column || region.ColumnTo > lastColumn || region.RowFrom < row || region.RowTo > lastRow)
		{
			throw new ArgumentOutOfRangeException("region");
		}
	}

	private void RemoveCache(List<MergeCellsRecord.MergedRegion> lstRegions)
	{
		if (lstRegions == null)
		{
			throw new ArgumentNullException("lstRegions");
		}
		Parse();
		foreach (MergeCellsRecord.MergedRegion lstRegion in lstRegions)
		{
			Rectangle item = Rectangle.FromLTRB(lstRegion.ColumnFrom, lstRegion.RowFrom, lstRegion.ColumnTo, lstRegion.RowTo);
			m_arrCells.Remove(item);
		}
	}

	[CLSCompliant(false)]
	public void AddCache(List<MergeCellsRecord.MergedRegion> lstRegions, int iRowDelta, int iColDelta)
	{
		if (lstRegions == null)
		{
			throw new ArgumentNullException("lstRegions");
		}
		Parse();
		foreach (MergeCellsRecord.MergedRegion lstRegion in lstRegions)
		{
			lstRegion.MoveRegion(iRowDelta, iColDelta);
			AddMerge(lstRegion, OfficeMergeOperation.Delete);
		}
	}

	public void AddMerges(IDictionary dictMerges, int iRowDelta, int iColumnDelta)
	{
		if (dictMerges == null)
		{
			throw new ArgumentNullException("dictMerges");
		}
		Parse();
		foreach (DictionaryEntry dictMerge in dictMerges)
		{
			long index = (long)dictMerge.Key;
			long index2 = (long)dictMerge.Value;
			int rowFromCellIndex = RangeImpl.GetRowFromCellIndex(index);
			int columnFromCellIndex = RangeImpl.GetColumnFromCellIndex(index);
			int rowFromCellIndex2 = RangeImpl.GetRowFromCellIndex(index2);
			int columnFromCellIndex2 = RangeImpl.GetColumnFromCellIndex(index2);
			AddMerge(rowFromCellIndex + iRowDelta - 1, rowFromCellIndex2 + iRowDelta - 1, columnFromCellIndex + iColumnDelta - 1, columnFromCellIndex2 + iColumnDelta - 1, OfficeMergeOperation.Delete);
		}
	}

	public Rectangle GetLeftTopCell(Rectangle rect)
	{
		MergeCellsRecord.MergedRegion mergedRegion = FindMergedRegion(rect);
		if (mergedRegion == null)
		{
			return Rectangle.FromLTRB(-1, -1, -1, -1);
		}
		return new Rectangle(mergedRegion.ColumnFrom, mergedRegion.RowFrom, 0, 0);
	}

	public object Clone(object parent)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		MergeCellsImpl obj = (MergeCellsImpl)MemberwiseClone();
		obj.SetParent(parent);
		obj.FindParents();
		obj.m_arrCells = CloneList(m_arrCells);
		obj.m_arrRecordsToParse = CloneUtils.CloneCloneable(m_arrRecordsToParse);
		return obj;
	}

	private List<Rectangle> CloneList(List<Rectangle> list)
	{
		int count = list.Count;
		List<Rectangle> list2 = new List<Rectangle>(count);
		for (int i = 0; i < count; i++)
		{
			list2.Add(list[i]);
		}
		return list2;
	}

	public void SetNewDimensions(int newRowCount, int newColumnCount)
	{
		newRowCount--;
		newColumnCount--;
		Parse();
		List<Rectangle> arrCells = m_arrCells;
		m_arrCells = new List<Rectangle>();
		int i = 0;
		for (int count = arrCells.Count; i < count; i++)
		{
			Rectangle rect = arrCells[i];
			MergeCellsRecord.MergedRegion mergedRegion = RectangleToMergeRegion(rect);
			mergedRegion.RowTo = Math.Min(mergedRegion.RowTo, newRowCount);
			mergedRegion.ColumnTo = Math.Min(mergedRegion.ColumnTo, newColumnCount);
			if (mergedRegion.CellsCount > 1)
			{
				AddMerge(mergedRegion, OfficeMergeOperation.Delete);
			}
		}
	}

	[CLSCompliant(false)]
	public MergeCellsRecord.MergedRegion FindMergedRegion(Rectangle rectangle)
	{
		MergeCellsRecord.MergedRegion result = null;
		int i = 0;
		for (int mergeCount = MergeCount; i < mergeCount; i++)
		{
			Rectangle rectangle2 = m_arrCells[i];
			if (UtilityMethods.Intersects(rectangle2, rectangle))
			{
				result = RectangleToMergeRegion(rectangle2);
				break;
			}
		}
		return result;
	}

	[CLSCompliant(false)]
	public MergeCellsRecord.MergedRegion RectangleToMergeRegion(Rectangle rect)
	{
		int x = rect.X;
		int y = rect.Y;
		int right = rect.Right;
		int bottom = rect.Bottom;
		return new MergeCellsRecord.MergedRegion(y, bottom, x, right);
	}

	[CLSCompliant(false)]
	public static MergeCellsRecord.MergedRegion InsertRemoveRowLower(MergeCellsRecord.MergedRegion region, bool isRemove, int iRowIndex, int iRowCount, IWorkbook book)
	{
		if (iRowCount <= 0 || iRowCount > book.MaxRowCount)
		{
			throw new ArgumentOutOfRangeException("iRowCount");
		}
		int num = (isRemove ? (-iRowCount) : iRowCount);
		int num2 = region.RowFrom + num;
		if (num2 >= book.MaxRowCount)
		{
			return null;
		}
		if (num2 < iRowIndex)
		{
			num2 = iRowIndex;
		}
		int num3 = region.RowTo + num;
		if (num3 < iRowIndex)
		{
			return null;
		}
		num2 = NormalizeRow(num2, book);
		num3 = NormalizeRow(num3, book);
		return new MergeCellsRecord.MergedRegion(num2, num3, region.ColumnFrom, region.ColumnTo);
	}

	[CLSCompliant(false)]
	public static MergeCellsRecord.MergedRegion InsertRemoveRowStart(MergeCellsRecord.MergedRegion region, bool isRemove, int iRowCount, IWorkbook book)
	{
		if (iRowCount <= 0 || iRowCount > book.MaxRowCount)
		{
			throw new ArgumentOutOfRangeException("iRowCount");
		}
		int num = (isRemove ? (-iRowCount) : iRowCount);
		int num2 = region.RowTo + num;
		int num3 = (isRemove ? region.RowFrom : (region.RowFrom + num));
		if (num2 < region.RowFrom)
		{
			return null;
		}
		if (num3 >= book.MaxRowCount)
		{
			return null;
		}
		num3 = NormalizeRow(num3, book);
		num2 = NormalizeRow(num2, book);
		return new MergeCellsRecord.MergedRegion(num3, num2, region.ColumnFrom, region.ColumnTo);
	}

	[CLSCompliant(false)]
	public static MergeCellsRecord.MergedRegion InsertRemoveRowMiddleEnd(MergeCellsRecord.MergedRegion region, bool isRemove, int iRowIndex, int iRowCount, IWorkbook book)
	{
		if (iRowCount <= 0 || iRowCount > book.MaxRowCount)
		{
			throw new ArgumentOutOfRangeException("iRowCount");
		}
		int num = (isRemove ? (-iRowCount) : iRowCount);
		int num2 = region.RowTo + num;
		if (num2 < iRowIndex)
		{
			num2 = iRowIndex - 1;
		}
		num2 = NormalizeRow(num2, book);
		return new MergeCellsRecord.MergedRegion(region.RowFrom, num2, region.ColumnFrom, region.ColumnTo);
	}

	[CLSCompliant(false)]
	public static MergeCellsRecord.MergedRegion InsertRemoveRowAbove(MergeCellsRecord.MergedRegion region, bool isRemove, int iRowCount)
	{
		return new MergeCellsRecord.MergedRegion(region.RowFrom, region.RowTo, region.ColumnFrom, region.ColumnTo);
	}

	[CLSCompliant(false)]
	public static MergeCellsRecord.MergedRegion InsertRemoveRow(MergeCellsRecord.MergedRegion region, int iRowIndex, bool isRemove, int iRowCount, IWorkbook book)
	{
		if (region.RowFrom == 0 && region.RowTo == book.MaxRowCount - 1)
		{
			return new MergeCellsRecord.MergedRegion(region.RowFrom, region.RowTo, region.ColumnFrom, region.ColumnTo);
		}
		if (region.RowFrom > iRowIndex)
		{
			return InsertRemoveRowLower(region, isRemove, iRowIndex, iRowCount, book);
		}
		if (region.RowFrom == iRowIndex)
		{
			return InsertRemoveRowStart(region, isRemove, iRowCount, book);
		}
		if (region.RowFrom < iRowIndex && region.RowTo > iRowIndex)
		{
			return InsertRemoveRowMiddleEnd(region, isRemove, iRowIndex, iRowCount, book);
		}
		if (region.RowTo == iRowIndex)
		{
			return InsertRemoveRowMiddleEnd(region, isRemove, iRowIndex, iRowCount, book);
		}
		if (region.RowTo < iRowIndex)
		{
			return InsertRemoveRowAbove(region, isRemove, iRowCount);
		}
		return null;
	}

	[CLSCompliant(false)]
	public static MergeCellsRecord.MergedRegion InsertRemoveColumnLower(MergeCellsRecord.MergedRegion region, bool isRemove, int iColumnIndex, int iCount, IWorkbook book)
	{
		if (iCount <= 0 || iCount > book.MaxColumnCount)
		{
			throw new ArgumentOutOfRangeException("iCount");
		}
		int num = (isRemove ? (-iCount) : iCount);
		int num2 = region.ColumnFrom + num;
		if (num2 >= book.MaxColumnCount)
		{
			return null;
		}
		if (num2 < iColumnIndex)
		{
			num2 = iColumnIndex;
		}
		int num3 = region.ColumnTo + num;
		if (num3 < iColumnIndex)
		{
			return null;
		}
		num2 = NormalizeColumn(num2, book);
		num3 = NormalizeColumn(num3, book);
		return new MergeCellsRecord.MergedRegion(region.RowFrom, region.RowTo, num2, num3);
	}

	[CLSCompliant(false)]
	public static MergeCellsRecord.MergedRegion InsertRemoveColumnStart(MergeCellsRecord.MergedRegion region, bool isRemove, int iCount, IWorkbook book)
	{
		if (iCount <= 0 || iCount > book.MaxColumnCount)
		{
			throw new ArgumentOutOfRangeException("iCount");
		}
		int num = (isRemove ? (-iCount) : iCount);
		int num2 = region.ColumnTo + num;
		int num3 = (isRemove ? region.ColumnFrom : (region.ColumnFrom + num));
		if (num2 < region.ColumnFrom)
		{
			return null;
		}
		if (num3 >= book.MaxColumnCount)
		{
			return null;
		}
		num3 = NormalizeRow(num3, book);
		num2 = NormalizeRow(num2, book);
		return new MergeCellsRecord.MergedRegion(region.RowFrom, region.RowTo, num3, num2);
	}

	[CLSCompliant(false)]
	public static MergeCellsRecord.MergedRegion InsertRemoveColumnMiddleEnd(MergeCellsRecord.MergedRegion region, bool isRemove, int iColumnIndex, int iCount, IWorkbook book)
	{
		if (iCount <= 0 || iCount > book.MaxColumnCount)
		{
			throw new ArgumentOutOfRangeException("iCount");
		}
		int num = (isRemove ? (-iCount) : iCount);
		int num2 = region.ColumnTo + num;
		if (num2 < iColumnIndex)
		{
			num2 = iColumnIndex - 1;
		}
		num2 = NormalizeColumn(num2, book);
		return new MergeCellsRecord.MergedRegion(region.RowFrom, region.RowTo, region.ColumnFrom, num2);
	}

	[CLSCompliant(false)]
	public static MergeCellsRecord.MergedRegion InsertRemoveColumnAbove(MergeCellsRecord.MergedRegion region, bool isRemove, int iCount)
	{
		return new MergeCellsRecord.MergedRegion(region.RowFrom, region.RowTo, region.ColumnFrom, region.ColumnTo);
	}

	[CLSCompliant(false)]
	public static MergeCellsRecord.MergedRegion InsertRemoveColumn(MergeCellsRecord.MergedRegion region, int iColumnIndex, bool isRemove, int iCount, IWorkbook book)
	{
		if (region.ColumnFrom == 0 && region.ColumnTo == book.MaxColumnCount - 1)
		{
			return new MergeCellsRecord.MergedRegion(region);
		}
		if (region.ColumnFrom > iColumnIndex)
		{
			return InsertRemoveColumnLower(region, isRemove, iColumnIndex, iCount, book);
		}
		if (region.ColumnFrom == iColumnIndex)
		{
			return InsertRemoveColumnStart(region, isRemove, iCount, book);
		}
		if (region.ColumnFrom < iColumnIndex && region.ColumnTo > iColumnIndex)
		{
			return InsertRemoveColumnMiddleEnd(region, isRemove, iColumnIndex, iCount, book);
		}
		if (region.ColumnTo == iColumnIndex)
		{
			return InsertRemoveColumnMiddleEnd(region, isRemove, iColumnIndex, iCount, book);
		}
		if (region.ColumnTo < iColumnIndex)
		{
			return InsertRemoveColumnAbove(region, isRemove, iCount);
		}
		return null;
	}

	[CLSCompliant(false)]
	public static int NormalizeRow(int iRowIndex, IWorkbook book)
	{
		if (iRowIndex < 0)
		{
			return 0;
		}
		return Math.Min(iRowIndex, book.MaxRowCount - 1);
	}

	[CLSCompliant(false)]
	public static int NormalizeColumn(int iColumnIndex, IWorkbook book)
	{
		if (iColumnIndex < 0)
		{
			return 0;
		}
		return Math.Min(iColumnIndex, book.MaxColumnCount - 1);
	}
}
