using System;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation;

internal class WorksheetHelper
{
	public static bool HasFormulaRecord(IInternalWorksheet sheet, int row, int column)
	{
		return sheet.CellRecords.Table.HasFormulaRecord(row, column);
	}

	public static RowStorage GetOrCreateRow(IInternalWorksheet sheet, int rowIndex, bool bCreate)
	{
		CellRecordCollection cellRecords = sheet.CellRecords;
		if (cellRecords == null && !bCreate)
		{
			return null;
		}
		OfficeVersion version = sheet.Workbook.Version;
		int standardHeightInRowUnits = ((sheet.Workbook as WorkbookImpl).Application as ApplicationImpl).StandardHeightInRowUnits;
		return cellRecords.Table.GetOrCreateRow(rowIndex, standardHeightInRowUnits, bCreate, version);
	}

	[CLSCompliant(false)]
	public static IOutline GetRowOutline(IInternalWorksheet sheet, int iRowIndex)
	{
		return GetOrCreateRow(sheet, iRowIndex - 1, bCreate: false);
	}

	public static void AccessColumn(IInternalWorksheet sheet, int iColumnIndex)
	{
		int firstColumn = sheet.FirstColumn;
		int lastColumn = sheet.LastColumn;
		if (firstColumn > iColumnIndex || firstColumn == int.MaxValue)
		{
			sheet.FirstColumn = (ushort)iColumnIndex;
		}
		if (lastColumn < iColumnIndex || lastColumn == int.MaxValue)
		{
			sheet.LastColumn = (ushort)iColumnIndex;
		}
	}

	public static void AccessRow(IInternalWorksheet sheet, int iRowIndex)
	{
		int firstRow = sheet.FirstRow;
		int lastRow = sheet.LastRow;
		if (firstRow > iRowIndex || firstRow < 0)
		{
			sheet.FirstRow = iRowIndex;
		}
		if (lastRow < iRowIndex || lastRow < 0)
		{
			sheet.LastRow = iRowIndex;
		}
	}
}
